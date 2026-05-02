using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

public static class SpaceEncounterFileWriter
{
    public static void Save(SpaceEncounterFileParser parser, IEnumerable<SpaceEncounterModel> encs)
    {
        var ordered = encs.ToList();
        var text = parser.SourceText;

        // Drop deleted member-name references from the AllEncounters aggregator.
        var deletedNames = parser.Encounters
            .Where(orig => !ordered.Any(o => o.OriginalSpan == orig.OriginalSpan))
            .Select(orig => orig.MemberName)
            .ToHashSet();
        if (deletedNames.Count > 0)
            text = StripFromAggregators(text, deletedNames);

        var keptSpans = new HashSet<TextSpan>(
            ordered.Where(e => e.OriginalSpan.HasValue).Select(e => e.OriginalSpan!.Value));

        var edits = new List<(TextSpan span, string replacement)>();
        foreach (var enc in ordered.Where(e => e.OriginalSpan.HasValue))
        {
            var span = enc.OriginalSpan!.Value;
            var indent = ItemFileWriter.ExtractIndent(text, span.Start);
            edits.Add((span, "\n" + Render(enc, indent)));
        }
        foreach (var orig in parser.Encounters.Where(e => e.OriginalSpan.HasValue && !keptSpans.Contains(e.OriginalSpan!.Value)))
            edits.Add((orig.OriginalSpan!.Value, ""));

        foreach (var edit in edits.OrderByDescending(e => e.span.Start))
            text = text.Substring(0, edit.span.Start) + edit.replacement + text.Substring(edit.span.End);

        // Insert new entries before the AllEncounters aggregator (or before the
        // class closing brace if no aggregator is present).
        var newOnes = ordered.Where(e => e.IsNew).ToList();
        if (newOnes.Count > 0)
        {
            var anchor = text.IndexOf("public static List<Func<SpaceEncounter>>", StringComparison.Ordinal);
            if (anchor < 0)
            {
                anchor = text.LastIndexOf('}');
                if (anchor < 0) throw new InvalidOperationException("No insertion anchor found in SpaceEncounterData.cs");
            }
            var lineStart = text.LastIndexOf('\n', anchor) + 1;
            var indent = ItemFileWriter.ExtractIndent(text, lineStart);
            if (string.IsNullOrEmpty(indent)) indent = "    ";
            var sb = new StringBuilder();
            foreach (var enc in newOnes) sb.Append(Render(enc, indent)).Append('\n');
            text = text.Substring(0, lineStart) + sb + text.Substring(lineStart);
        }

        File.WriteAllText(parser.FilePath, text);
    }

    /// Strips deleted member-names from the AllEncounters list (which holds method
    /// references like `PirateInterceptor`, not invocations).
    private static string StripFromAggregators(string text, HashSet<string> deleted)
    {
        var pattern = new System.Text.RegularExpressions.Regex(
            @"public\s+static\s+List<Func<SpaceEncounter>>\s+\w+\s*=>\s*new\(\)\s*\{(?<body>[^}]*)\}\s*;",
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

    public static string Render(SpaceEncounterModel m, string indent = "    ")
    {
        var inner  = indent + "    ";
        var inner2 = inner  + "    ";
        var inner3 = inner2 + "    ";
        var sb = new StringBuilder();
        sb.Append(indent).Append("public static SpaceEncounter ").Append(m.MemberName).Append("() => new()\n");
        sb.Append(indent).Append("{\n");

        // Pilot block.
        sb.Append(inner).Append("Pilot = new Character\n").Append(inner).Append("{\n");
        sb.Append(inner2).Append("Name = \"").Append(ItemFileWriter.Escape(m.PilotName)).Append("\",\n");
        sb.Append(inner2).Append("IsPlayer = ").Append(m.PilotIsPlayer ? "true" : "false").Append(",\n");
        if (m.PilotAttributes.Count > 0)
        {
            sb.Append(inner2).Append("Attributes = new()\n").Append(inner2).Append("{\n");
            foreach (var (k, v) in m.PilotAttributes)
                sb.Append(inner3).Append("[AttributeType.").Append(k).Append("] = ")
                  .Append(ItemFileWriter.RenderDice(v.Dice, v.Pips)).Append(",\n");
            sb.Append(inner2).Append("},\n");
        }
        if (m.PilotSkillBonuses.Count > 0)
        {
            sb.Append(inner2).Append("SkillBonuses = new()\n").Append(inner2).Append("{\n");
            foreach (var (k, v) in m.PilotSkillBonuses)
                sb.Append(inner3).Append("[SkillType.").Append(k).Append("] = ")
                  .Append(ItemFileWriter.RenderDice(v.Dice, v.Pips)).Append(",\n");
            sb.Append(inner2).Append("},\n");
        }
        if (!string.IsNullOrEmpty(m.PilotEquippedArmorMember))
            sb.Append(inner2).Append("EquippedArmor = ArmorData.").Append(m.PilotEquippedArmorMember).Append(",\n");
        sb.Append(inner).Append("},\n");

        // Ship block.
        sb.Append(inner).Append("Ship = new Vehicle\n").Append(inner).Append("{\n");
        sb.Append(inner2).Append("Name = \"").Append(ItemFileWriter.Escape(m.ShipName)).Append("\",\n");
        if (!string.IsNullOrEmpty(m.ShipDescription))
            sb.Append(inner2).Append("Description = ").Append(ItemFileWriter.Quote(m.ShipDescription)).Append(",\n");
        sb.Append(inner2).Append("IsSpace = ").Append(m.ShipIsSpace ? "true" : "false").Append(",\n");
        sb.Append(inner2).Append("Maneuverability = ").Append(ItemFileWriter.RenderDice(m.ShipManeuverDice, m.ShipManeuverPips)).Append(",\n");
        sb.Append(inner2).Append("Resolve = ").Append(m.ShipResolve).Append(",\n");
        if (!string.IsNullOrEmpty(m.ShipShieldMember))
            sb.Append(inner2).Append("Shield = ShieldData.").Append(m.ShipShieldMember).Append(",\n");
        if (m.ShipWeapons.Count > 0)
        {
            sb.Append(inner2).Append("Weapons = new()\n").Append(inner2).Append("{\n");
            foreach (var w in m.ShipWeapons)
                sb.Append(inner3).Append("new() { Name = \"").Append(ItemFileWriter.Escape(w.Name))
                  .Append("\", Damage = ").Append(ItemFileWriter.RenderDice(w.DamageDice, w.DamagePips))
                  .Append(", AttackSkill = SkillType.").Append(string.IsNullOrEmpty(w.AttackSkill) ? "Gunnery" : w.AttackSkill)
                  .Append(" },\n");
            sb.Append(inner2).Append("},\n");
        }
        if (m.ShipEquipment.Count > 0)
        {
            sb.Append(inner2).Append("Equipment = new()\n").Append(inner2).Append("{\n");
            foreach (var eq in m.ShipEquipment)
                sb.Append(inner3).Append("new() { Name = \"").Append(ItemFileWriter.Escape(eq.Name))
                  .Append("\", BonusSkill = SkillType.").Append(string.IsNullOrEmpty(eq.BonusSkill) ? "Gunnery" : eq.BonusSkill)
                  .Append(", Bonus = ").Append(ItemFileWriter.RenderDice(eq.BonusDice, eq.BonusPips))
                  .Append(" },\n");
            sb.Append(inner2).Append("},\n");
        }
        sb.Append(inner).Append("}\n");
        sb.Append(indent).Append("};\n");
        return sb.ToString();
    }
}
