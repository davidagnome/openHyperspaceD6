using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TerminalHyperspace.WorldEditor;

/// Scans NPCData.cs / SpaceEncounterData.cs to enumerate available factory
/// member names so the editor can populate dropdowns dynamically (no need to
/// hardcode lists that drift out of sync).
public static class SymbolScanner
{
    public static List<string> ScanFactories(string filePath, string returnTypeName)
    {
        var names = new List<string>();
        if (!File.Exists(filePath)) return names;

        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath));
        var root = (CompilationUnitSyntax)tree.GetRoot();

        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            if (!method.Modifiers.Any(m => m.Text == "public")) continue;
            if (!method.Modifiers.Any(m => m.Text == "static")) continue;
            if (method.ParameterList.Parameters.Count > 0) continue;
            if (method.ReturnType is IdentifierNameSyntax id && id.Identifier.Text == returnTypeName)
                names.Add(method.Identifier.Text);
        }

        names.Sort();
        return names;
    }
}
