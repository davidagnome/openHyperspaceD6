using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Writes Dialogue factories back to DialogueData.cs. Pools are left alone here
/// (DialoguePoolFileWriter handles those), but references to deleted dialogues
/// are stripped from any pool body so the file stays compilable.
public static class DialogueFileWriter
{
    public static void Save(DialogueFileParser parser, IEnumerable<DialogueModel> dialogues)
    {
        var ordered = dialogues.ToList();
        var text = parser.SourceText;

        // Strip references to deleted/renamed dialogue members from every pool
        // body before per-dialogue spans shift. Renames are conservative: if a
        // member was removed we drop it; a renamed member shows up as a new
        // entry, not a kept one, so callers should still update pools manually.
        var deletedNames = parser.Dialogues
            .Where(orig => !ordered.Any(o => o.OriginalSpan == orig.OriginalSpan))
            .Select(orig => orig.MemberName)
            .ToHashSet();
        if (deletedNames.Count > 0)
            text = StripFromPools(text, deletedNames);

        var keptSpans = new HashSet<TextSpan>(
            ordered.Where(d => d.OriginalSpan.HasValue).Select(d => d.OriginalSpan!.Value));

        var edits = new List<(TextSpan span, string replacement)>();
        foreach (var d in ordered.Where(d => d.OriginalSpan.HasValue))
        {
            var span = d.OriginalSpan!.Value;
            var indent = ItemFileWriter.ExtractIndent(text, span.Start);
            edits.Add((span, "\n" + Render(d, indent)));
        }
        foreach (var orig in parser.Dialogues.Where(d => d.OriginalSpan.HasValue && !keptSpans.Contains(d.OriginalSpan!.Value)))
            edits.Add((orig.OriginalSpan!.Value, ""));

        foreach (var edit in edits.OrderByDescending(e => e.span.Start))
            text = text.Substring(0, edit.span.Start) + edit.replacement + text.Substring(edit.span.End);

        // New dialogues go before the first pool property so dialogues stay
        // grouped at the top of the class.
        var newOnes = ordered.Where(d => d.IsNew).ToList();
        if (newOnes.Count > 0)
        {
            int anchor = text.IndexOf("public static List<Func<string, string, Dialogue>>", StringComparison.Ordinal);
            if (anchor < 0) anchor = text.LastIndexOf('}'); // class closing brace fallback
            var lineStart = text.LastIndexOf('\n', anchor) + 1;
            var indent = ItemFileWriter.ExtractIndent(text, lineStart);
            if (string.IsNullOrEmpty(indent)) indent = "    ";
            var sb = new StringBuilder();
            foreach (var d in newOnes) sb.Append(Render(d, indent)).Append('\n');
            text = text.Substring(0, lineStart) + sb + text.Substring(lineStart);
        }

        File.WriteAllText(parser.FilePath, text);
    }

    /// Removes the named bare-identifier references from every
    /// `public static List<Func<string, string, Dialogue>> X => new() { … }` body.
    private static string StripFromPools(string text, HashSet<string> deleted)
    {
        var pattern = new System.Text.RegularExpressions.Regex(
            @"public\s+static\s+List<Func<string,\s*string,\s*Dialogue>>\s+\w+\s*=>\s*new\(\)\s*\{(?<body>[^}]*)\}\s*;",
            System.Text.RegularExpressions.RegexOptions.Multiline);
        return pattern.Replace(text, m =>
        {
            var body = m.Groups["body"].Value;
            // Preserve any line breaks/indentation by working line-by-line.
            var entries = body.Split(',').Select(s => s.Trim('\n', '\r', ' ', '\t'))
                .Where(s => s.Length > 0 && !deleted.Contains(s)).ToList();
            var inner = "        ";
            var newBody = "\n" + string.Join(",\n", entries.Select(e => inner + e)) + ",\n    ";
            if (entries.Count == 0) newBody = " ";
            return m.Value.Replace(m.Groups["body"].Value, newBody);
        });
    }

    public static string Render(DialogueModel d, string indent = "    ")
    {
        var inner  = indent + "    ";
        var inner2 = inner  + "    ";
        var inner3 = inner2 + "    ";
        var sb = new StringBuilder();
        sb.Append(indent).Append("public static Dialogue ").Append(d.MemberName).Append("(string npcName, string playerName) => new()\n");
        sb.Append(indent).Append("{\n");
        sb.Append(inner).Append("Lines = new()\n").Append(inner).Append("{\n");
        foreach (var ln in d.Lines)
        {
            sb.Append(inner2).Append("new() { Speaker = ").Append(RenderSpeaker(ln.Speaker))
              .Append(", Line = \"").Append(ItemFileWriter.Escape(ln.Line)).Append("\" },\n");
        }
        sb.Append(inner).Append("},\n");
        sb.Append(indent).Append("};\n");
        return sb.ToString();
    }

    private static string RenderSpeaker(string speaker)
    {
        // npcName / playerName are factory parameters, emitted as bare identifiers.
        // Anything else round-trips as a string literal.
        if (speaker == "npcName" || speaker == "playerName") return speaker;
        return "\"" + ItemFileWriter.Escape(speaker) + "\"";
    }
}
