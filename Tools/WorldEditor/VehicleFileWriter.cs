using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

public static class VehicleFileWriter
{
    public static void Save(VehicleFileParser parser, IEnumerable<VehicleModel> vehicles)
    {
        var ordered = vehicles.ToList();
        var text = parser.SourceText;

        var deletedNames = parser.Vehicles
            .Where(orig => !ordered.Any(o => o.OriginalSpan == orig.OriginalSpan))
            .Select(orig => orig.MemberName)
            .ToHashSet();
        if (deletedNames.Count > 0)
            text = StripFromAggregators(text, deletedNames);

        var keptSpans = new HashSet<TextSpan>(
            ordered.Where(v => v.OriginalSpan.HasValue).Select(v => v.OriginalSpan!.Value));

        var edits = new List<(TextSpan span, string replacement)>();
        foreach (var v in ordered.Where(v => v.OriginalSpan.HasValue))
        {
            var span = v.OriginalSpan!.Value;
            var indent = ItemFileWriter.ExtractIndent(text, span.Start);
            edits.Add((span, "\n" + Render(v, indent)));
        }
        foreach (var orig in parser.Vehicles.Where(v => v.OriginalSpan.HasValue && !keptSpans.Contains(v.OriginalSpan!.Value)))
            edits.Add((orig.OriginalSpan!.Value, ""));

        foreach (var edit in edits.OrderByDescending(e => e.span.Start))
            text = text.Substring(0, edit.span.Start) + edit.replacement + text.Substring(edit.span.End);

        var newOnes = ordered.Where(v => v.IsNew).ToList();
        if (newOnes.Count > 0)
        {
            var anchor = text.IndexOf("public static List<Vehicle>", StringComparison.Ordinal);
            if (anchor < 0) throw new InvalidOperationException("No List<Vehicle> aggregator found");
            var lineStart = text.LastIndexOf('\n', anchor) + 1;
            var indent = ItemFileWriter.ExtractIndent(text, lineStart);
            var sb = new StringBuilder();
            foreach (var v in newOnes) sb.Append(Render(v, indent)).Append('\n');
            text = text.Substring(0, lineStart) + sb + text.Substring(lineStart);
        }

        File.WriteAllText(parser.FilePath, text);
    }

    private static string StripFromAggregators(string text, HashSet<string> deleted)
    {
        var pattern = new System.Text.RegularExpressions.Regex(
            @"public\s+static\s+List<Vehicle>\s+\w+\s*=>\s*new\(\)\s*\{(?<body>[^}]*)\}\s*;",
            System.Text.RegularExpressions.RegexOptions.Multiline);
        return pattern.Replace(text, m =>
        {
            var body = m.Groups["body"].Value;
            var kept = body.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0 && !deleted.Contains(s))
                .ToList();
            var newBody = " " + string.Join(", ", kept) + " ";
            return m.Value.Replace(m.Groups["body"].Value, newBody);
        });
    }

    public static string Render(VehicleModel v, string indent = "    ")
    {
        var inner = indent + "    ";
        var sb = new StringBuilder();
        sb.Append(indent).Append("public static Vehicle ").Append(v.MemberName).Append(" => new()\n");
        sb.Append(indent).Append("{\n");
        sb.Append(inner).Append("Name = \"").Append(ItemFileWriter.Escape(v.Name)).Append("\",\n");
        if (!string.IsNullOrEmpty(v.Description))
            sb.Append(inner).Append("Description = ").Append(ItemFileWriter.Quote(v.Description)).Append(",\n");
        sb.Append(inner).Append("IsSpace = ").Append(v.IsSpace ? "true" : "false").Append(",\n");
        sb.Append(inner).Append("Maneuverability = ").Append(ItemFileWriter.RenderDice(v.ManeuverDice, v.ManeuverPips)).Append(",\n");
        sb.Append(inner).Append("Resolve = ").Append(v.Resolve).Append(",\n");
        if (!string.IsNullOrEmpty(v.ShieldMember))
            sb.Append(inner).Append("Shield = ShieldData.").Append(v.ShieldMember).Append(",\n");
        if (v.Weapons.Count > 0)
        {
            sb.Append(inner).Append("Weapons = new()\n").Append(inner).Append("{\n");
            foreach (var w in v.Weapons)
                sb.Append(inner).Append("    new() { Name = \"").Append(ItemFileWriter.Escape(w.Name))
                  .Append("\", Damage = ").Append(ItemFileWriter.RenderDice(w.DamageDice, w.DamagePips))
                  .Append(", AttackSkill = SkillType.").Append(w.AttackSkill).Append(" },\n");
            sb.Append(inner).Append("},\n");
        }
        if (v.Equipment.Count > 0)
        {
            sb.Append(inner).Append("Equipment = new()\n").Append(inner).Append("{\n");
            foreach (var eq in v.Equipment)
            {
                sb.Append(inner).Append("    new() { Name = \"").Append(ItemFileWriter.Escape(eq.Name))
                  .Append("\", BonusSkill = SkillType.").Append(string.IsNullOrEmpty(eq.BonusSkill) ? "Gunnery" : eq.BonusSkill)
                  .Append(", Bonus = ").Append(ItemFileWriter.RenderDice(eq.BonusDice, eq.BonusPips))
                  .Append(" },\n");
            }
            sb.Append(inner).Append("},\n");
        }
        sb.Append(inner).Append("Price = ").Append(v.Price).Append("\n");
        sb.Append(indent).Append("};\n");
        return sb.ToString();
    }
}
