using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Parses the `LocationChecks` dictionary inside Content/SkillCheckData.cs:
///
///     public static Dictionary&lt;string, List&lt;SkillCheckEvent&gt;&gt; LocationChecks
///     {
///         get
///         {
///             var map = new Dictionary&lt;string, List&lt;SkillCheckEvent&gt;&gt;
///             {
///                 ["tatooine_espa_cantina"] = new() { CantinaLockbox, CantinaSabacc... },
///                 ...
///             };
///             RegisterImportedLocationChecks(map);
///             return map;
///         }
///     }
///
/// Each `["id"] = new() { Name1, Name2, ... }` element becomes a
/// LocationCheckEntryModel.
public class LocationChecksFileParser
{
    public string FilePath { get; }
    public string SourceText { get; private set; } = "";
    public List<LocationCheckEntryModel> Entries { get; } = new();

    /// Span of the dictionary initializer including its `{` and `}` braces.
    /// The writer replaces this entire range when saving.
    public TextSpan DictionaryInitializerSpan { get; private set; }

    public string? Error { get; private set; }

    public LocationChecksFileParser(string filePath) { FilePath = filePath; }

    public bool TryLoad()
    {
        try
        {
            SourceText = File.ReadAllText(FilePath);
            var tree = CSharpSyntaxTree.ParseText(SourceText);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var prop = root.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => p.Identifier.Text == "LocationChecks");
            if (prop == null) { Error = "LocationChecks property not found."; return false; }

            // Inside the get accessor, find the `new Dictionary<...> { ... }` creation.
            var dictCreation = prop.DescendantNodes().OfType<ObjectCreationExpressionSyntax>()
                .FirstOrDefault(oc =>
                    oc.Type is GenericNameSyntax g && g.Identifier.Text == "Dictionary"
                    && oc.Initializer != null);
            if (dictCreation?.Initializer == null) { Error = "LocationChecks dictionary initializer not found."; return false; }

            DictionaryInitializerSpan = dictCreation.Initializer.Span;

            foreach (var element in dictCreation.Initializer.Expressions)
            {
                if (element is not AssignmentExpressionSyntax assign) continue;
                if (assign.Left is not ImplicitElementAccessSyntax leftAccess) continue;

                var keyArg = leftAccess.ArgumentList.Arguments.FirstOrDefault();
                if (keyArg?.Expression is not LiteralExpressionSyntax keyLit) continue;
                var locationId = keyLit.Token.ValueText;

                // Right side is `new() { ... }` (implicit) or `new List<SkillCheckEvent> { ... }`.
                InitializerExpressionSyntax? listInit = assign.Right switch
                {
                    ImplicitObjectCreationExpressionSyntax ioc => ioc.Initializer,
                    ObjectCreationExpressionSyntax oc => oc.Initializer,
                    _ => null,
                };

                var members = new List<string>();
                if (listInit != null)
                {
                    foreach (var memberExpr in listInit.Expressions)
                    {
                        if (memberExpr is IdentifierNameSyntax ins)
                            members.Add(ins.Identifier.Text);
                        else
                        {
                            // Fallback: keep whatever's there literally so we don't lose data
                            // on round-trip if someone wrote a fully-qualified reference.
                            members.Add(memberExpr.ToString().Trim());
                        }
                    }
                }

                Entries.Add(new LocationCheckEntryModel
                {
                    LocationId = locationId,
                    CheckMemberNames = members,
                    OriginalSpan = element.FullSpan,
                });
            }

            return true;
        }
        catch (Exception ex) { Error = ex.Message; return false; }
    }
}
