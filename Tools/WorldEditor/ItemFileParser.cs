using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Parses Content/ItemData.cs. Each item is a property like:
///   public static Item BlasterPistol => new() { Name = "...", ... };
/// Aggregator List&lt;Item&gt; properties (StarterWeapons, AllWeapons, AllItems)
/// are skipped — the writer's anchor for new items is the first such aggregator.
public class ItemFileParser
{
    public string FilePath { get; }
    public string SourceText { get; private set; } = "";
    public List<ItemModel> Items { get; } = new();
    public TextSpan AnchorSpan { get; private set; }
    public string? Error { get; private set; }

    public ItemFileParser(string filePath) { FilePath = filePath; }

    public bool TryLoad()
    {
        try
        {
            SourceText = File.ReadAllText(FilePath);
            var tree = CSharpSyntaxTree.ParseText(SourceText);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            // Locate the partial class ItemData declaration.
            var cls = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.Text == "ItemData");
            if (cls == null) { Error = "ItemData class not found."; return false; }

            // Walk every property; the first List<Item> property is our anchor for inserts.
            foreach (var prop in cls.Members.OfType<PropertyDeclarationSyntax>())
            {
                if (IsListItem(prop.Type))
                {
                    if (AnchorSpan.IsEmpty) AnchorSpan = prop.FullSpan;
                    continue;
                }
                if (prop.Type is not IdentifierNameSyntax id || id.Identifier.Text != "Item") continue;
                if (prop.ExpressionBody?.Expression is not ImplicitObjectCreationExpressionSyntax create
                    || create.Initializer == null) continue;

                var item = new ItemModel
                {
                    MemberName = prop.Identifier.Text,
                    OriginalSpan = prop.FullSpan,
                };
                ApplyInitializer(create.Initializer, item);
                Items.Add(item);
            }
            return true;
        }
        catch (Exception ex) { Error = ex.Message; return false; }
    }

    private static bool IsListItem(TypeSyntax t)
        => t is GenericNameSyntax g && g.Identifier.Text == "List"
           && g.TypeArgumentList.Arguments.Count == 1
           && g.TypeArgumentList.Arguments[0] is IdentifierNameSyntax inner
           && inner.Identifier.Text == "Item";

    private static void ApplyInitializer(InitializerExpressionSyntax init, ItemModel m)
    {
        foreach (var expr in init.Expressions)
        {
            if (expr is not AssignmentExpressionSyntax pa) continue;
            if (pa.Left is not IdentifierNameSyntax name) continue;
            var rhs = pa.Right;
            switch (name.Identifier.Text)
            {
                case "Name":          m.Name          = ReadString(rhs) ?? ""; break;
                case "Description":   m.Description   = ReadString(rhs) ?? ""; break;
                case "IsWeapon":      m.IsWeapon      = ReadBool(rhs); break;
                case "Damage":        (m.DamageDice, m.DamagePips) = ReadDiceCode(rhs); break;
                case "AttackSkill":   m.AttackSkill   = ReadEnumMember(rhs, "SkillType") ?? ""; break;
                case "Range":         m.Range         = ReadInt(rhs); break;
                case "Price":         m.Price         = ReadInt(rhs); break;
                case "IsConsumable":  m.IsConsumable  = ReadBool(rhs); break;
                case "IsMissionItem": m.IsMissionItem = ReadBool(rhs); break;
                case "MissionDestinationLocationId": m.MissionDestinationLocationId = ReadString(rhs) ?? ""; break;
                case "MissionDestinationName":       m.MissionDestinationName       = ReadString(rhs) ?? ""; break;
            }
        }
    }

    public static string? ReadString(ExpressionSyntax e)
        => e is LiteralExpressionSyntax l && l.IsKind(SyntaxKind.StringLiteralExpression) ? l.Token.ValueText : null;

    public static bool ReadBool(ExpressionSyntax e) => e.IsKind(SyntaxKind.TrueLiteralExpression);

    public static int ReadInt(ExpressionSyntax e)
        => e is LiteralExpressionSyntax l && l.IsKind(SyntaxKind.NumericLiteralExpression)
            ? Convert.ToInt32(l.Token.Value) : 0;

    /// `new DiceCode(3)` → (3, 0); `new DiceCode(2, 1)` → (2, 1).
    public static (int dice, int pips) ReadDiceCode(ExpressionSyntax e)
    {
        if (e is not ObjectCreationExpressionSyntax oce
            || oce.Type is not IdentifierNameSyntax id || id.Identifier.Text != "DiceCode")
            return (0, 0);
        var args = oce.ArgumentList?.Arguments;
        if (args is null || args.Value.Count == 0) return (0, 0);
        var dice = ReadInt(args.Value[0].Expression);
        var pips = args.Value.Count > 1 ? ReadInt(args.Value[1].Expression) : 0;
        return (dice, pips);
    }

    /// `SkillType.Blasters` → "Blasters"; otherwise null.
    public static string? ReadEnumMember(ExpressionSyntax e, string typeName)
    {
        if (e is MemberAccessExpressionSyntax m
            && m.Expression is IdentifierNameSyntax cls && cls.Identifier.Text == typeName)
            return m.Name.Identifier.Text;
        return null;
    }
}
