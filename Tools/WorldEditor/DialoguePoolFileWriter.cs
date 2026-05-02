using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Writes pool declarations back to DialogueData.cs. Operates on the same
/// parser as DialogueFileWriter; both can be called independently because each
/// only mutates its own kind of declaration.
public static class DialoguePoolFileWriter
{
    public static void Save(DialogueFileParser parser, IEnumerable<DialoguePoolModel> pools)
    {
        var ordered = pools.ToList();
        var text = parser.SourceText;

        var keptSpans = new HashSet<TextSpan>(
            ordered.Where(p => p.OriginalSpan.HasValue).Select(p => p.OriginalSpan!.Value));

        var edits = new List<(TextSpan span, string replacement)>();
        foreach (var p in ordered.Where(p => p.OriginalSpan.HasValue))
        {
            var span = p.OriginalSpan!.Value;
            var indent = ItemFileWriter.ExtractIndent(text, span.Start);
            edits.Add((span, "\n" + Render(p, indent)));
        }
        foreach (var orig in parser.Pools.Where(p => p.OriginalSpan.HasValue && !keptSpans.Contains(p.OriginalSpan!.Value)))
            edits.Add((orig.OriginalSpan!.Value, ""));

        foreach (var edit in edits.OrderByDescending(e => e.span.Start))
            text = text.Substring(0, edit.span.Start) + edit.replacement + text.Substring(edit.span.End);

        // New pools go just before the closing brace of the DialogueData class.
        var newOnes = ordered.Where(p => p.IsNew).ToList();
        if (newOnes.Count > 0)
        {
            // Find the closing brace of DialogueData by walking back from EOF
            // until we cross the namespace/file scope. Cheap heuristic: the very
            // last `}` in the file is the class brace because the file uses a
            // file-scoped namespace.
            int anchor = text.LastIndexOf('}');
            if (anchor < 0) throw new InvalidOperationException("Could not locate class closing brace in DialogueData.cs");
            var lineStart = text.LastIndexOf('\n', anchor) + 1;
            var indent = ItemFileWriter.ExtractIndent(text, lineStart) + "    ";
            var sb = new StringBuilder();
            foreach (var p in newOnes) sb.Append('\n').Append(Render(p, indent));
            text = text.Substring(0, lineStart) + sb + text.Substring(lineStart);
        }

        File.WriteAllText(parser.FilePath, text);
    }

    public static string Render(DialoguePoolModel p, string indent = "    ")
    {
        var inner = indent + "    ";
        var sb = new StringBuilder();
        sb.Append(indent).Append("public static List<Func<string, string, Dialogue>> ").Append(p.PoolName).Append(" => new()\n");
        sb.Append(indent).Append("{\n");
        foreach (var name in p.FactoryNames)
            sb.Append(inner).Append(name).Append(",\n");
        sb.Append(indent).Append("};\n");
        return sb.ToString();
    }
}
