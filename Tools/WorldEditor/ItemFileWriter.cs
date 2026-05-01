using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

public static class ItemFileWriter
{
    public static void Save(ItemFileParser parser, IEnumerable<ItemModel> items)
    {
        var ordered = items.ToList();
        var text = parser.SourceText;

        // Sweep dangling references in the aggregator lists when an item is deleted
        // or renamed, so e.g. AllItems doesn't keep pointing at a member that no
        // longer exists. Done first against the original source so spans stay valid.
        var deletedNames = parser.Items
            .Where(orig => !ordered.Any(o => o.OriginalSpan == orig.OriginalSpan))
            .Select(orig => orig.MemberName)
            .ToHashSet();
        if (deletedNames.Count > 0)
            text = StripFromAggregators(text, deletedNames);

        var keptSpans = new HashSet<TextSpan>(
            ordered.Where(i => i.OriginalSpan.HasValue).Select(i => i.OriginalSpan!.Value));

        var edits = new List<(TextSpan span, string replacement)>();
        foreach (var it in ordered.Where(i => i.OriginalSpan.HasValue))
        {
            var span = it.OriginalSpan!.Value;
            var indent = ExtractIndent(text, span.Start);
            edits.Add((span, "\n" + Render(it, indent)));
        }
        foreach (var orig in parser.Items.Where(i => i.OriginalSpan.HasValue && !keptSpans.Contains(i.OriginalSpan!.Value)))
            edits.Add((orig.OriginalSpan!.Value, ""));

        foreach (var edit in edits.OrderByDescending(e => e.span.Start))
            text = text.Substring(0, edit.span.Start) + edit.replacement + text.Substring(edit.span.End);

        // Insert new items before the first List<Item> aggregator (StarterWeapons).
        var newOnes = ordered.Where(i => i.IsNew).ToList();
        if (newOnes.Count > 0)
        {
            var anchor = text.IndexOf("public static List<Item>", StringComparison.Ordinal);
            if (anchor < 0) throw new InvalidOperationException("No List<Item> aggregator found in ItemData.cs");
            var lineStart = text.LastIndexOf('\n', anchor) + 1;
            var indent = ExtractIndent(text, lineStart);
            var sb = new StringBuilder();
            foreach (var it in newOnes) sb.Append(Render(it, indent)).Append('\n');
            text = text.Substring(0, lineStart) + sb + text.Substring(lineStart);
        }

        File.WriteAllText(parser.FilePath, text);
    }

    /// Removes references to deleted item member names from any
    /// `public static List<Item> X => new() { …, Foo, … };` aggregator. The text is
    /// the raw source — this runs before any per-item span replacements.
    private static string StripFromAggregators(string text, HashSet<string> deleted)
    {
        // Match the full `public static List<Item> Name => new() { ... };` declaration,
        // then rewrite the member-name list inside the braces.
        var pattern = new System.Text.RegularExpressions.Regex(
            @"public\s+static\s+List<Item>\s+\w+\s*=>\s*new\(\)\s*\{(?<body>[^}]*)\}\s*;",
            System.Text.RegularExpressions.RegexOptions.Multiline);
        return pattern.Replace(text, m =>
        {
            var body = m.Groups["body"].Value;
            // Split on commas, trim, drop deleted names, rejoin.
            var kept = body.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0 && !deleted.Contains(s))
                .ToList();
            var newBody = " " + string.Join(", ", kept) + " ";
            return m.Value.Replace(m.Groups["body"].Value, newBody);
        });
    }

    public static string Render(ItemModel m, string indent = "    ")
    {
        var inner = indent + "    ";
        var sb = new StringBuilder();
        sb.Append(indent).Append("public static Item ").Append(m.MemberName).Append(" => new()\n");
        sb.Append(indent).Append("{\n");
        sb.Append(inner).Append("Name = \"").Append(Escape(m.Name)).Append("\",\n");
        if (!string.IsNullOrEmpty(m.Description))
            sb.Append(inner).Append("Description = ").Append(Quote(m.Description)).Append(",\n");
        sb.Append(inner).Append("IsWeapon = ").Append(m.IsWeapon ? "true" : "false").Append(",\n");
        if (m.IsWeapon || m.DamageDice != 0 || m.DamagePips != 0)
            sb.Append(inner).Append("Damage = ").Append(RenderDice(m.DamageDice, m.DamagePips)).Append(",\n");
        if (!string.IsNullOrEmpty(m.AttackSkill))
            sb.Append(inner).Append("AttackSkill = SkillType.").Append(m.AttackSkill).Append(",\n");
        sb.Append(inner).Append("Range = ").Append(m.Range).Append(",\n");
        sb.Append(inner).Append("Price = ").Append(m.Price).Append(",\n");
        if (m.IsConsumable) sb.Append(inner).Append("IsConsumable = true,\n");
        if (m.IsMissionItem) sb.Append(inner).Append("IsMissionItem = true,\n");
        if (!string.IsNullOrEmpty(m.MissionDestinationLocationId))
            sb.Append(inner).Append("MissionDestinationLocationId = \"").Append(Escape(m.MissionDestinationLocationId)).Append("\",\n");
        if (!string.IsNullOrEmpty(m.MissionDestinationName))
            sb.Append(inner).Append("MissionDestinationName = \"").Append(Escape(m.MissionDestinationName)).Append("\",\n");
        sb.Append(indent).Append("};\n");
        return sb.ToString();
    }

    public static string RenderDice(int dice, int pips)
        => pips == 0 ? $"new DiceCode({dice})" : $"new DiceCode({dice}, {pips})";

    public static string ExtractIndent(string source, int start)
    {
        var sb = new StringBuilder();
        for (int i = start; i < source.Length && (source[i] == ' ' || source[i] == '\t'); i++)
            sb.Append(source[i]);
        return sb.Length > 0 ? sb.ToString() : "    ";
    }

    public static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    public static string Quote(string s)
    {
        if (s.Contains('\n') || s.Contains('\r'))
            return "@\"" + s.Replace("\"", "\"\"") + "\"";
        return "\"" + Escape(s) + "\"";
    }
}
