using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Parses Content/ArmorData.cs. Each armor is a static readonly field declaration.
/// Skips the sibling ShieldData class that lives in the same file.
public class ArmorFileParser
{
    public string FilePath { get; }
    public string SourceText { get; private set; } = "";
    public List<ArmorModel> Armors { get; } = new();
    public string? Error { get; private set; }

    public ArmorFileParser(string filePath) { FilePath = filePath; }

    public bool TryLoad()
    {
        try
        {
            SourceText = File.ReadAllText(FilePath);
            var tree = CSharpSyntaxTree.ParseText(SourceText);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var cls = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.Text == "ArmorData");
            if (cls == null) { Error = "ArmorData class not found."; return false; }

            // Inspect the Purchasable aggregator first so each entry can record
            // whether it currently appears in that list.
            var purchasableNames = new HashSet<string>();
            var purchasableProp = cls.Members.OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => p.Identifier.Text == "Purchasable");
            if (purchasableProp?.ExpressionBody?.Expression is ImplicitObjectCreationExpressionSyntax pioce
                && pioce.Initializer != null)
            {
                foreach (var e in pioce.Initializer.Expressions)
                    if (e is IdentifierNameSyntax n) purchasableNames.Add(n.Identifier.Text);
            }

            foreach (var field in cls.Members.OfType<FieldDeclarationSyntax>())
            {
                if (field.Declaration.Type is not IdentifierNameSyntax id || id.Identifier.Text != "Armor") continue;
                foreach (var v in field.Declaration.Variables)
                {
                    if (v.Initializer?.Value is not ImplicitObjectCreationExpressionSyntax create
                        || create.Initializer == null) continue;
                    var m = new ArmorModel
                    {
                        MemberName = v.Identifier.Text,
                        OriginalSpan = field.FullSpan,
                        Purchasable = purchasableNames.Contains(v.Identifier.Text),
                    };
                    Apply(create.Initializer, m);
                    Armors.Add(m);
                }
            }
            return true;
        }
        catch (Exception ex) { Error = ex.Message; return false; }
    }

    private static void Apply(InitializerExpressionSyntax init, ArmorModel m)
    {
        foreach (var expr in init.Expressions)
        {
            if (expr is not AssignmentExpressionSyntax pa) continue;
            if (pa.Left is not IdentifierNameSyntax name) continue;
            switch (name.Identifier.Text)
            {
                case "Name":     m.Name  = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "DiceCode": m.Dice  = ReadDiceFirstArg(pa.Right); break;
                case "Price":    m.Price = ItemFileParser.ReadInt(pa.Right); break;
                case "Climate":  m.Climate = ItemFileParser.ReadEnumMember(pa.Right, "Climate") ?? ""; break;
            }
        }
    }

    private static int ReadDiceFirstArg(ExpressionSyntax e)
    {
        if (e is not ObjectCreationExpressionSyntax oce) return 0;
        var args = oce.ArgumentList?.Arguments;
        if (args is null || args.Value.Count == 0) return 0;
        return ItemFileParser.ReadInt(args.Value[0].Expression);
    }
}
