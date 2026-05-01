using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

public static class RoleSpeciesFileWriter
{
    public static void Save(RoleSpeciesFileParser parser, IEnumerable<RoleSpeciesModel> entries)
    {
        var ordered = entries.ToList();
        var text = parser.SourceText;

        var keptSpans = new HashSet<TextSpan>(
            ordered.Where(r => r.OriginalSpan.HasValue).Select(r => r.OriginalSpan!.Value));

        var edits = new List<(TextSpan span, string replacement)>();
        foreach (var r in ordered.Where(r => r.OriginalSpan.HasValue))
        {
            var span = r.OriginalSpan!.Value;
            var indent = ItemFileWriter.ExtractIndent(text, span.Start);
            edits.Add((span, "\n" + Render(r, parser.EntityType, parser.AttributesPropertyName, indent)));
        }
        foreach (var orig in parser.Entries.Where(r => r.OriginalSpan.HasValue && !keptSpans.Contains(r.OriginalSpan!.Value)))
        {
            // Trailing comma+newline after the element should also go.
            var span = orig.OriginalSpan!.Value;
            int extra = 0;
            int idx = span.End;
            while (idx < text.Length && (text[idx] == ',' || text[idx] == ' ' || text[idx] == '\t')) { idx++; extra++; }
            edits.Add((new TextSpan(span.Start, span.Length + extra), ""));
        }

        foreach (var edit in edits.OrderByDescending(e => e.span.Start))
            text = text.Substring(0, edit.span.Start) + edit.replacement + text.Substring(edit.span.End);

        // For inserts, drop them right before the list-closing `};`. Re-locate the
        // closing brace each time since the prior edits may have shifted positions.
        var newOnes = ordered.Where(r => r.IsNew).ToList();
        if (newOnes.Count > 0)
        {
            // Find the `};` that closes the list initializer. The original parser
            // captured it; after edits, find by searching for the same token shape.
            // Simplest stable approach: find `RegisterImported(list);` and back up
            // to the previous `};`.
            var anchor = text.IndexOf("RegisterImported(list);", StringComparison.Ordinal);
            if (anchor < 0) throw new InvalidOperationException("RegisterImported(list); anchor not found");
            // Walk backwards to find the closing brace of the list initializer.
            var closing = text.LastIndexOf("};", anchor, StringComparison.Ordinal);
            if (closing < 0) throw new InvalidOperationException("List closing brace not found");
            var lineStart = text.LastIndexOf('\n', closing) + 1;
            var indent = ItemFileWriter.ExtractIndent(text, lineStart) + "    ";

            var sb = new StringBuilder();
            foreach (var r in newOnes)
                sb.Append(Render(r, parser.EntityType, parser.AttributesPropertyName, indent)).Append(",\n");
            text = text.Substring(0, lineStart) + sb + text.Substring(lineStart);
        }

        File.WriteAllText(parser.FilePath, text);
    }

    public static string Render(RoleSpeciesModel m, string entityType, string attributesPropertyName, string indent = "    ")
    {
        var inner = indent + "    ";
        var inner2 = inner + "    ";
        var sb = new StringBuilder();
        sb.Append(indent).Append("new ").Append(entityType).Append("\n");
        sb.Append(indent).Append("{\n");
        sb.Append(inner).Append("Name = \"").Append(ItemFileWriter.Escape(m.Name)).Append("\",\n");
        if (!string.IsNullOrEmpty(m.Description))
            sb.Append(inner).Append("Description = ").Append(ItemFileWriter.Quote(m.Description)).Append(",\n");
        if (m.AttributeBonuses.Count > 0)
        {
            sb.Append(inner).Append(attributesPropertyName).Append(" = new()\n").Append(inner).Append("{\n");
            foreach (var (k, v) in m.AttributeBonuses)
                sb.Append(inner2).Append("[AttributeType.").Append(k).Append("] = ")
                  .Append(ItemFileWriter.RenderDice(v.Dice, v.Pips)).Append(",\n");
            sb.Append(inner).Append("},\n");
        }
        if (m.SkillBonuses.Count > 0)
        {
            sb.Append(inner).Append("SkillBonuses = new()\n").Append(inner).Append("{\n");
            foreach (var (k, v) in m.SkillBonuses)
                sb.Append(inner2).Append("[SkillType.").Append(k).Append("] = ")
                  .Append(ItemFileWriter.RenderDice(v.Dice, v.Pips)).Append(",\n");
            sb.Append(inner).Append("}\n");
        }
        sb.Append(indent).Append("}");
        return sb.ToString();
    }
}
