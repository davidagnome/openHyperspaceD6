using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Parses Content/NPCData.cs. Each NPC is a method:
///   public static Character Stormtrooper() => new() { ... };
public class NpcFileParser
{
    public string FilePath { get; }
    public string SourceText { get; private set; } = "";
    public List<NpcModel> Npcs { get; } = new();
    public TextSpan ClassClosingBraceSpan { get; private set; }
    public string? Error { get; private set; }

    public NpcFileParser(string filePath) { FilePath = filePath; }

    public bool TryLoad()
    {
        try
        {
            SourceText = File.ReadAllText(FilePath);
            var tree = CSharpSyntaxTree.ParseText(SourceText);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var cls = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.Text == "NPCData");
            if (cls == null) { Error = "NPCData class not found."; return false; }

            ClassClosingBraceSpan = cls.CloseBraceToken.FullSpan;

            foreach (var method in cls.Members.OfType<MethodDeclarationSyntax>())
            {
                if (method.ReturnType is not IdentifierNameSyntax id || id.Identifier.Text != "Character") continue;
                if (method.ParameterList.Parameters.Count > 0) continue;
                if (method.ExpressionBody?.Expression is not ImplicitObjectCreationExpressionSyntax create
                    || create.Initializer == null) continue;

                var npc = new NpcModel
                {
                    MemberName = method.Identifier.Text,
                    OriginalSpan = method.FullSpan,
                };
                ApplyInitializer(create.Initializer, npc);
                Npcs.Add(npc);
            }
            return true;
        }
        catch (Exception ex) { Error = ex.Message; return false; }
    }

    private static void ApplyInitializer(InitializerExpressionSyntax init, NpcModel m)
    {
        foreach (var expr in init.Expressions)
        {
            if (expr is not AssignmentExpressionSyntax pa) continue;
            if (pa.Left is not IdentifierNameSyntax name) continue;
            switch (name.Identifier.Text)
            {
                case "Name":     m.DisplayName = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "IsPlayer": m.IsPlayer    = ItemFileParser.ReadBool(pa.Right); break;
                case "Attributes":     ReadDiceMap(pa.Right, m.Attributes,    "AttributeType"); break;
                case "SkillBonuses":   ReadDiceMap(pa.Right, m.SkillBonuses,  "SkillType"); break;
                case "Inventory":      m.Inventory = ReadFactoryList(pa.Right, "ItemData"); break;
                case "EquippedWeapon": m.EquippedWeaponMember = ReadFactoryName(pa.Right, "ItemData") ?? ""; break;
                case "EquippedArmor":  m.EquippedArmorMember  = ReadFactoryName(pa.Right, "ArmorData") ?? ""; break;
            }
        }
    }

    public static void ReadDiceMap(ExpressionSyntax e, Dictionary<string, (int, int)> map, string enumType)
    {
        InitializerExpressionSyntax? init = null;
        if (e is ImplicitObjectCreationExpressionSyntax ioce) init = ioce.Initializer;
        else if (e is ObjectCreationExpressionSyntax oce) init = oce.Initializer;
        if (init == null) return;
        foreach (var item in init.Expressions)
        {
            if (item is not AssignmentExpressionSyntax pa) continue;
            if (pa.Left is not ImplicitElementAccessSyntax iea) continue;
            var keyExpr = iea.ArgumentList.Arguments[0].Expression;
            if (keyExpr is not MemberAccessExpressionSyntax mae) continue;
            if (mae.Expression is not IdentifierNameSyntax cls || cls.Identifier.Text != enumType) continue;
            var key = mae.Name.Identifier.Text;
            map[key] = ItemFileParser.ReadDiceCode(pa.Right);
        }
    }

    public static List<string> ReadFactoryList(ExpressionSyntax e, string expectedClass)
    {
        var result = new List<string>();
        InitializerExpressionSyntax? init = null;
        if (e is ImplicitObjectCreationExpressionSyntax ioce) init = ioce.Initializer;
        else if (e is ObjectCreationExpressionSyntax oce) init = oce.Initializer;
        if (init == null) return result;
        foreach (var item in init.Expressions)
        {
            var name = ReadFactoryName(item, expectedClass);
            if (name != null) result.Add(name);
        }
        return result;
    }

    public static string? ReadFactoryName(ExpressionSyntax e, string expectedClass)
    {
        if (e is MemberAccessExpressionSyntax m
            && m.Expression is IdentifierNameSyntax cls
            && cls.Identifier.Text == expectedClass)
            return m.Name.Identifier.Text;
        return null;
    }
}
