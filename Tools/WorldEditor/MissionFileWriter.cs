using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

public static class MissionFileWriter
{
    public static void Save(MissionFileParser parser, IEnumerable<MissionModel> missions)
    {
        var ordered = missions.ToList();
        var text = parser.SourceText;

        // Sweep AllOffers list for any deleted member references.
        var deletedNames = parser.Missions
            .Where(orig => !ordered.Any(o => o.OriginalSpan == orig.OriginalSpan))
            .Select(orig => orig.MemberName)
            .ToHashSet();

        // Whatever names remain after the save (original kept + new) are what
        // AllOffers should reference. Update its body unconditionally so renames
        // stay in sync too.
        var finalNames = parser.Missions
            .Where(p => ordered.Any(o => o.OriginalSpan == p.OriginalSpan))
            .Select(p => p.MemberName)
            .Concat(ordered.Where(o => o.IsNew).Select(o => o.MemberName))
            .ToList();

        text = RewriteAllOffers(text, finalNames);

        var keptSpans = new HashSet<TextSpan>(
            ordered.Where(m => m.OriginalSpan.HasValue).Select(m => m.OriginalSpan!.Value));

        var edits = new List<(TextSpan span, string replacement)>();
        foreach (var m in ordered.Where(m => m.OriginalSpan.HasValue))
        {
            var span = m.OriginalSpan!.Value;
            var indent = ItemFileWriter.ExtractIndent(text, span.Start);
            edits.Add((span, "\n" + Render(m, indent)));
        }
        foreach (var orig in parser.Missions.Where(m => m.OriginalSpan.HasValue && !keptSpans.Contains(m.OriginalSpan!.Value)))
            edits.Add((orig.OriginalSpan!.Value, ""));

        foreach (var edit in edits.OrderByDescending(e => e.span.Start))
            text = text.Substring(0, edit.span.Start) + edit.replacement + text.Substring(edit.span.End);

        var newOnes = ordered.Where(m => m.IsNew).ToList();
        if (newOnes.Count > 0)
        {
            var anchor = text.IndexOf("public static List<Func<Mission>> AllOffers", StringComparison.Ordinal);
            if (anchor < 0) throw new InvalidOperationException("AllOffers anchor not found");
            var lineStart = text.LastIndexOf('\n', anchor) + 1;
            var indent = ItemFileWriter.ExtractIndent(text, lineStart);
            var sb = new StringBuilder();
            foreach (var m in newOnes) sb.Append(Render(m, indent)).Append('\n');
            text = text.Substring(0, lineStart) + sb + text.Substring(lineStart);
        }

        File.WriteAllText(parser.FilePath, text);
    }

    private static string RewriteAllOffers(string text, IEnumerable<string> names)
    {
        // Match the AllOffers initializer body and rewrite it.
        var pattern = new System.Text.RegularExpressions.Regex(
            @"public\s+static\s+List<Func<Mission>>\s+AllOffers\s*=>\s*new\(\)\s*\{(?<body>[^}]*)\}\s*;",
            System.Text.RegularExpressions.RegexOptions.Multiline);
        return pattern.Replace(text, m =>
        {
            var nameList = names.Distinct().ToList();
            if (nameList.Count == 0)
                return m.Value.Replace(m.Groups["body"].Value, " ");
            // Group five per line for readability.
            var sb = new StringBuilder("\n");
            for (int i = 0; i < nameList.Count; i += 5)
            {
                sb.Append("        ");
                sb.Append(string.Join(", ", nameList.Skip(i).Take(5)));
                sb.Append(",\n");
            }
            sb.Append("    ");
            return m.Value.Replace(m.Groups["body"].Value, sb.ToString());
        });
    }

    public static string Render(MissionModel m, string indent = "    ")
    {
        var inner = indent + "    ";
        var sb = new StringBuilder();
        sb.Append(indent).Append("public static Mission ").Append(m.MemberName).Append("() => new()\n");
        sb.Append(indent).Append("{\n");
        sb.Append(inner).Append("Id = \"").Append(ItemFileWriter.Escape(m.Id)).Append("\",\n");
        sb.Append(inner).Append("Title = ").Append(ItemFileWriter.Quote(m.Title)).Append(",\n");
        sb.Append(inner).Append("BriefingText = ").Append(ItemFileWriter.Quote(m.BriefingText)).Append(",\n");
        sb.Append(inner).Append("Type = MissionType.").Append(string.IsNullOrEmpty(m.Type) ? "Escort" : m.Type).Append(",\n");
        sb.Append(inner).Append("DestinationLocationId = \"").Append(ItemFileWriter.Escape(m.DestinationLocationId)).Append("\",\n");
        if (!string.IsNullOrEmpty(m.EscortNpcName))
            sb.Append(inner).Append("EscortNpcName = \"").Append(ItemFileWriter.Escape(m.EscortNpcName)).Append("\",\n");
        if (m.HasMissionItem)
        {
            sb.Append(inner).Append("MissionItem = new Item\n").Append(inner).Append("{\n");
            sb.Append(inner).Append("    Name = \"").Append(ItemFileWriter.Escape(m.MissionItemName)).Append("\",\n");
            sb.Append(inner).Append("    Description = ").Append(ItemFileWriter.Quote(m.MissionItemDescription)).Append(",\n");
            sb.Append(inner).Append("    IsMissionItem = true,\n");
            if (!string.IsNullOrEmpty(m.MissionItemDestinationLocationId))
                sb.Append(inner).Append("    MissionDestinationLocationId = \"").Append(ItemFileWriter.Escape(m.MissionItemDestinationLocationId)).Append("\",\n");
            if (!string.IsNullOrEmpty(m.MissionItemDestinationName))
                sb.Append(inner).Append("    MissionDestinationName = \"").Append(ItemFileWriter.Escape(m.MissionItemDestinationName)).Append("\",\n");
            sb.Append(inner).Append("},\n");
        }
        if (!string.IsNullOrEmpty(m.CheckSkill))
        {
            sb.Append(inner).Append("CheckSkill = SkillType.").Append(m.CheckSkill).Append(",\n");
            sb.Append(inner).Append("CheckTargetNumber = ").Append(m.CheckTargetNumber).Append(",\n");
            sb.Append(inner).Append("CheckSuccessText = ").Append(ItemFileWriter.Quote(m.CheckSuccessText)).Append(",\n");
            sb.Append(inner).Append("CheckFailText = ").Append(ItemFileWriter.Quote(m.CheckFailText)).Append(",\n");
        }
        sb.Append(inner).Append("CreditReward = ").Append(m.CreditReward)
          .Append(", UpgradePointReward = ").Append(m.UpgradePointReward).Append(",\n");
        sb.Append(indent).Append("};\n");
        return sb.ToString();
    }
}
