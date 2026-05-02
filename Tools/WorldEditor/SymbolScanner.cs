using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TerminalHyperspace.WorldEditor;

/// Scans NPCData.cs / SpaceEncounterData.cs to enumerate available factory
/// member names so the editor can populate dropdowns dynamically (no need to
/// hardcode lists that drift out of sync).
public static class SymbolScanner
{
    public static List<string> ScanFactories(string filePath, string returnTypeName, bool allowParameters = false)
    {
        var names = new List<string>();
        if (!File.Exists(filePath)) return names;

        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath));
        var root = (CompilationUnitSyntax)tree.GetRoot();

        // Methods: `public static T Foo() => new() { ... }`. Set allowParameters
        // when the consumer treats parameterized factories as first-class (e.g.
        // DialogueData factories take an NPC + player name).
        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            if (!method.Modifiers.Any(m => m.Text == "public")) continue;
            if (!method.Modifiers.Any(m => m.Text == "static")) continue;
            if (!allowParameters && method.ParameterList.Parameters.Count > 0) continue;
            if (method.ReturnType is IdentifierNameSyntax id && id.Identifier.Text == returnTypeName)
                names.Add(method.Identifier.Text);
        }

        // Properties: `public static T Foo => new() { ... }`
        foreach (var prop in root.DescendantNodes().OfType<PropertyDeclarationSyntax>())
        {
            if (!prop.Modifiers.Any(m => m.Text == "public")) continue;
            if (!prop.Modifiers.Any(m => m.Text == "static")) continue;
            if (prop.Type is IdentifierNameSyntax id && id.Identifier.Text == returnTypeName)
                names.Add(prop.Identifier.Text);
        }

        // Fields: `public static readonly T Foo = new() { ... }`
        foreach (var field in root.DescendantNodes().OfType<FieldDeclarationSyntax>())
        {
            if (!field.Modifiers.Any(m => m.Text == "public")) continue;
            if (!field.Modifiers.Any(m => m.Text == "static")) continue;
            if (field.Declaration.Type is IdentifierNameSyntax id && id.Identifier.Text == returnTypeName)
                foreach (var v in field.Declaration.Variables)
                    names.Add(v.Identifier.Text);
        }

        names.Sort();
        return names;
    }
}
