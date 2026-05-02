using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

public static class ArmorFileWriter
{
    public static void Save(ArmorFileParser parser, IEnumerable<ArmorModel> armors)
    {
        var ordered = armors.ToList();
        var text = parser.SourceText;

        var keptSpans = new HashSet<TextSpan>(
            ordered.Where(a => a.OriginalSpan.HasValue).Select(a => a.OriginalSpan!.Value));

        var edits = new List<(TextSpan span, string replacement)>();
        foreach (var a in ordered.Where(a => a.OriginalSpan.HasValue))
        {
            var span = a.OriginalSpan!.Value;
            var indent = ItemFileWriter.ExtractIndent(text, span.Start);
            edits.Add((span, "\n" + Render(a, indent)));
        }
        foreach (var orig in parser.Armors.Where(a => a.OriginalSpan.HasValue && !keptSpans.Contains(a.OriginalSpan!.Value)))
            edits.Add((orig.OriginalSpan!.Value, ""));

        foreach (var edit in edits.OrderByDescending(e => e.span.Start))
            text = text.Substring(0, edit.span.Start) + edit.replacement + text.Substring(edit.span.End);

        // Insert new entries before the `All` aggregator so they're scoped to ArmorData.
        var newOnes = ordered.Where(a => a.IsNew).ToList();
        if (newOnes.Count > 0)
        {
            var anchor = text.IndexOf("public static List<Armor>", StringComparison.Ordinal);
            if (anchor < 0) throw new InvalidOperationException("No List<Armor> aggregator found in ArmorData.cs");
            var lineStart = text.LastIndexOf('\n', anchor) + 1;
            var indent = ItemFileWriter.ExtractIndent(text, lineStart);
            var sb = new StringBuilder();
            foreach (var a in newOnes) sb.Append(Render(a, indent)).Append('\n');
            text = text.Substring(0, lineStart) + sb + text.Substring(lineStart);
        }

        // Regenerate the All / Purchasable aggregator bodies from the resulting set
        // so they always reflect the live armor roster.
        text = RewriteAggregator(text, "All", ordered.Select(a => a.MemberName).ToList());
        text = RewriteAggregator(text, "Purchasable", ordered.Where(a => a.Purchasable).Select(a => a.MemberName).ToList());

        File.WriteAllText(parser.FilePath, text);
    }

    private static string RewriteAggregator(string text, string propName, List<string> members)
    {
        var pattern = new System.Text.RegularExpressions.Regex(
            @"public\s+static\s+List<Armor>\s+" + System.Text.RegularExpressions.Regex.Escape(propName)
            + @"\s*=>\s*new\(\)\s*\{(?<body>[^}]*)\}\s*;",
            System.Text.RegularExpressions.RegexOptions.Multiline);
        var match = pattern.Match(text);
        if (!match.Success) return text;

        var lineStart = text.LastIndexOf('\n', match.Index) + 1;
        var indent = ItemFileWriter.ExtractIndent(text, lineStart);
        var inner = indent + "    ";

        string newDecl;
        if (members.Count == 0)
        {
            newDecl = $"public static List<Armor> {propName} => new() {{ }};";
        }
        else
        {
            // Render with up to 6 names per line for readability, matching the original style.
            var sb = new StringBuilder();
            sb.Append("public static List<Armor> ").Append(propName).Append(" => new()\n");
            sb.Append(indent).Append("{\n");
            for (int i = 0; i < members.Count; i += 6)
            {
                sb.Append(inner);
                var chunk = members.Skip(i).Take(6).ToList();
                sb.Append(string.Join(", ", chunk));
                if (i + 6 < members.Count) sb.Append(",");
                sb.Append("\n");
            }
            sb.Append(indent).Append("};");
            newDecl = sb.ToString();
        }

        return text.Substring(0, match.Index) + newDecl + text.Substring(match.Index + match.Length);
    }

    public static string Render(ArmorModel m, string indent = "    ")
    {
        var sb = new StringBuilder();
        sb.Append(indent).Append("public static readonly Armor ").Append(m.MemberName).Append(" = new()\n");
        sb.Append(indent).Append("{\n");
        sb.Append(indent).Append("    Name = \"").Append(ItemFileWriter.Escape(m.Name)).Append("\", ");
        sb.Append("DiceCode = new DiceCode(").Append(m.Dice).Append("), ");
        sb.Append("Price = ").Append(m.Price);
        if (!string.IsNullOrEmpty(m.Climate) && m.Climate != "Normal")
            sb.Append(", Climate = Climate.").Append(m.Climate);
        sb.Append("\n").Append(indent).Append("};\n");
        return sb.ToString();
    }
}
