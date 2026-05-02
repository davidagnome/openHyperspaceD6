using System.Text;

namespace TerminalHyperspace.WorldEditor;

/// Rewrites the `LocationChecks` dictionary initializer in
/// Content/SkillCheckData.cs by replacing the entire `{ ... }` block parsed
/// by LocationChecksFileParser. Entries with empty CheckMemberNames are
/// dropped so the file stays clean after deletes.
public static class LocationChecksFileWriter
{
    public static void Save(LocationChecksFileParser parser, IEnumerable<LocationCheckEntryModel> entries)
    {
        var ordered = entries
            .Where(e => !string.IsNullOrWhiteSpace(e.LocationId))
            .Where(e => e.CheckMemberNames.Count > 0)
            .ToList();

        var text = parser.SourceText;
        var span = parser.DictionaryInitializerSpan;
        if (span.IsEmpty)
            throw new InvalidOperationException("LocationChecks dictionary span not loaded");

        // Match the indentation of the line that holds the dictionary's `{`.
        var braceLineStart = text.LastIndexOf('\n', span.Start) + 1;
        var openIndent = ItemFileWriter.ExtractIndent(text, braceLineStart);

        // Per-entry indent: prefer whatever the first existing entry uses so we
        // don't churn the whole block when the original file's indentation
        // doesn't match a strict (parent + 4) rule. Falls back to parent + 4
        // when the dictionary is empty.
        var entryIndent = DetectEntryIndent(text, span) ?? openIndent + "    ";

        var sb = new StringBuilder();
        sb.Append("{\n");
        foreach (var e in ordered)
        {
            sb.Append(entryIndent)
              .Append("[\"").Append(Escape(e.LocationId)).Append("\"] = new() { ")
              .Append(string.Join(", ", e.CheckMemberNames))
              .Append(" },\n");
        }
        sb.Append(openIndent).Append("}");

        var newText = text.Substring(0, span.Start) + sb + text.Substring(span.End);
        File.WriteAllText(parser.FilePath, newText);
    }

    private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    /// Look inside the dictionary span (between `{` and `}`) for the first
    /// `["…"]` element and return the whitespace prefix of that line. Returns
    /// null if no entries are present.
    private static string? DetectEntryIndent(string text, Microsoft.CodeAnalysis.Text.TextSpan span)
    {
        var openBrace = text.IndexOf('{', span.Start);
        if (openBrace < 0 || openBrace >= span.End) return null;
        var firstEntry = text.IndexOf('[', openBrace);
        if (firstEntry < 0 || firstEntry >= span.End) return null;
        var lineStart = text.LastIndexOf('\n', firstEntry) + 1;
        return ItemFileWriter.ExtractIndent(text, lineStart);
    }
}
