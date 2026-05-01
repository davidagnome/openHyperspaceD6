using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Parses Content/RoleData.cs or Content/SpeciesData.cs by entering the `All`
/// property, walking the inline `new Role/Species { ... }` elements inside the
/// list initializer, and projecting each into a RoleSpeciesModel.
///
/// Construct with `entityType` = "Role" for RoleData.cs or "Species" for
/// SpeciesData.cs. The same parser handles both.
public class RoleSpeciesFileParser
{
    public string FilePath { get; }
    public string EntityType { get; }
    public string SourceText { get; private set; } = "";
    public List<RoleSpeciesModel> Entries { get; } = new();
    /// Span of the closing `};` of the inline list initializer — that's where
    /// new entries are inserted.
    public TextSpan ListClosingSpan { get; private set; }
    /// Bonus property names per entity. Roles use AttributeBonuses; Species use BaseAttributes.
    public string AttributesPropertyName => EntityType == "Role" ? "AttributeBonuses" : "BaseAttributes";
    public string? Error { get; private set; }

    public RoleSpeciesFileParser(string filePath, string entityType)
    {
        FilePath = filePath;
        EntityType = entityType;
    }

    public bool TryLoad()
    {
        try
        {
            SourceText = File.ReadAllText(FilePath);
            var tree = CSharpSyntaxTree.ParseText(SourceText);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            // Find the `var list = new List<T> { ... }` declaration inside the All
            // property's getter. The list initializer's closing brace is our anchor.
            var listInit = root.DescendantNodes()
                .OfType<ObjectCreationExpressionSyntax>()
                .FirstOrDefault(oce =>
                    oce.Type is GenericNameSyntax gn
                    && gn.Identifier.Text == "List"
                    && gn.TypeArgumentList.Arguments.Count == 1
                    && gn.TypeArgumentList.Arguments[0] is IdentifierNameSyntax inner
                    && inner.Identifier.Text == EntityType
                    && oce.Initializer != null);
            if (listInit?.Initializer == null)
            {
                Error = $"List<{EntityType}> initializer not found.";
                return false;
            }
            ListClosingSpan = listInit.Initializer.CloseBraceToken.FullSpan;

            foreach (var element in listInit.Initializer.Expressions)
            {
                if (element is not ObjectCreationExpressionSyntax oce
                    || oce.Type is not IdentifierNameSyntax id || id.Identifier.Text != EntityType
                    || oce.Initializer == null) continue;

                var m = new RoleSpeciesModel { OriginalSpan = oce.FullSpan };
                Apply(oce.Initializer, m);
                Entries.Add(m);
            }
            return true;
        }
        catch (Exception ex) { Error = ex.Message; return false; }
    }

    private void Apply(InitializerExpressionSyntax init, RoleSpeciesModel m)
    {
        foreach (var expr in init.Expressions)
        {
            if (expr is not AssignmentExpressionSyntax pa) continue;
            if (pa.Left is not IdentifierNameSyntax name) continue;
            switch (name.Identifier.Text)
            {
                case "Name":        m.Name        = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "Description": m.Description = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "AttributeBonuses":
                case "BaseAttributes":
                    NpcFileParser.ReadDiceMap(pa.Right, m.AttributeBonuses, "AttributeType");
                    break;
                case "SkillBonuses":
                    NpcFileParser.ReadDiceMap(pa.Right, m.SkillBonuses, "SkillType");
                    break;
            }
        }
    }
}
