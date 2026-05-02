using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Parses Content/DialogueData.cs. Surfaces both:
///   - Dialogue factories: `public static Dialogue Foo(string npcName, string playerName) => new() { ... }`
///   - Pools:              `public static List<Func<string, string, Dialogue>> Bar => new() { Foo, Baz, ... }`
public class DialogueFileParser
{
    public string FilePath { get; }
    public string SourceText { get; private set; } = "";
    public List<DialogueModel> Dialogues { get; } = new();
    public List<DialoguePoolModel> Pools { get; } = new();
    public string? Error { get; private set; }

    public DialogueFileParser(string filePath) { FilePath = filePath; }

    public bool TryLoad()
    {
        try
        {
            SourceText = File.ReadAllText(FilePath);
            var tree = CSharpSyntaxTree.ParseText(SourceText);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var cls = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.Text == "DialogueData");
            if (cls == null) { Error = "DialogueData class not found."; return false; }

            // Dialogue factories: parameterized methods returning Dialogue.
            foreach (var method in cls.Members.OfType<MethodDeclarationSyntax>())
            {
                if (method.ReturnType is not IdentifierNameSyntax id || id.Identifier.Text != "Dialogue") continue;
                if (method.ExpressionBody?.Expression is not ImplicitObjectCreationExpressionSyntax create
                    || create.Initializer == null) continue;
                var d = new DialogueModel
                {
                    MemberName = method.Identifier.Text,
                    OriginalSpan = method.FullSpan,
                };
                ApplyDialogueInitializer(create.Initializer, d);
                Dialogues.Add(d);
            }

            // Pools: properties typed `List<Func<string, string, Dialogue>>`.
            foreach (var prop in cls.Members.OfType<PropertyDeclarationSyntax>())
            {
                if (!IsPoolType(prop.Type)) continue;
                var pool = new DialoguePoolModel
                {
                    PoolName = prop.Identifier.Text,
                    OriginalSpan = prop.FullSpan,
                };
                if (prop.ExpressionBody?.Expression is ImplicitObjectCreationExpressionSyntax pioce
                    && pioce.Initializer != null)
                {
                    foreach (var e in pioce.Initializer.Expressions)
                        if (e is IdentifierNameSyntax ins) pool.FactoryNames.Add(ins.Identifier.Text);
                }
                Pools.Add(pool);
            }

            return true;
        }
        catch (Exception ex) { Error = ex.Message; return false; }
    }

    private static bool IsPoolType(TypeSyntax t)
    {
        if (t is not GenericNameSyntax g || g.Identifier.Text != "List") return false;
        if (g.TypeArgumentList.Arguments.Count != 1) return false;
        if (g.TypeArgumentList.Arguments[0] is not GenericNameSyntax inner) return false;
        if (inner.Identifier.Text != "Func") return false;
        if (inner.TypeArgumentList.Arguments.Count != 3) return false;
        return inner.TypeArgumentList.Arguments[2] is IdentifierNameSyntax retType
            && retType.Identifier.Text == "Dialogue";
    }

    private static void ApplyDialogueInitializer(InitializerExpressionSyntax init, DialogueModel d)
    {
        foreach (var expr in init.Expressions)
        {
            if (expr is not AssignmentExpressionSyntax pa) continue;
            if (pa.Left is not IdentifierNameSyntax name) continue;
            if (name.Identifier.Text != "Lines") continue;
            // Lines = new() { new() { Speaker = ..., Line = ... }, ... }
            var linesInit = pa.Right is ImplicitObjectCreationExpressionSyntax ioce ? ioce.Initializer
                          : pa.Right is ObjectCreationExpressionSyntax oce ? oce.Initializer
                          : null;
            if (linesInit == null) continue;
            foreach (var lineExpr in linesInit.Expressions)
            {
                var lineInit = lineExpr is ImplicitObjectCreationExpressionSyntax lioce ? lioce.Initializer
                             : lineExpr is ObjectCreationExpressionSyntax loce ? loce.Initializer
                             : null;
                if (lineInit == null) continue;
                var dl = new DialogueLineModel();
                foreach (var assn in lineInit.Expressions)
                {
                    if (assn is not AssignmentExpressionSyntax a) continue;
                    if (a.Left is not IdentifierNameSyntax key) continue;
                    switch (key.Identifier.Text)
                    {
                        case "Speaker":
                            // Speaker references the parameter identifier (npcName / playerName).
                            // Fall back to a string literal if used.
                            if (a.Right is IdentifierNameSyntax ins)
                                dl.Speaker = ins.Identifier.Text;
                            else
                                dl.Speaker = ItemFileParser.ReadString(a.Right) ?? "npcName";
                            break;
                        case "Line":
                            dl.Line = ItemFileParser.ReadString(a.Right) ?? "";
                            break;
                    }
                }
                d.Lines.Add(dl);
            }
        }
    }
}
