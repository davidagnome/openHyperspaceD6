using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

public static class SkillCheckFileWriter
{
    public static void Save(SkillCheckFileParser parser, IEnumerable<SkillCheckModel> checks)
    {
        var ordered = checks.ToList();
        var text = parser.SourceText;

        // When checks are deleted, sweep the LocationChecks dictionary entries
        // and the TalkChecks list so we don't leave dangling references.
        var deletedNames = parser.Checks
            .Where(orig => !ordered.Any(o => o.OriginalSpan == orig.OriginalSpan))
            .Select(orig => orig.MemberName)
            .ToHashSet();
        if (deletedNames.Count > 0)
            text = StripFromAggregators(text, deletedNames);

        var keptSpans = new HashSet<TextSpan>(
            ordered.Where(c => c.OriginalSpan.HasValue).Select(c => c.OriginalSpan!.Value));

        var edits = new List<(TextSpan span, string replacement)>();
        foreach (var c in ordered.Where(c => c.OriginalSpan.HasValue))
        {
            var span = c.OriginalSpan!.Value;
            var indent = ItemFileWriter.ExtractIndent(text, span.Start);
            edits.Add((span, "\n" + Render(c, indent)));
        }
        foreach (var orig in parser.Checks.Where(c => c.OriginalSpan.HasValue && !keptSpans.Contains(c.OriginalSpan!.Value)))
            edits.Add((orig.OriginalSpan!.Value, ""));

        foreach (var edit in edits.OrderByDescending(e => e.span.Start))
            text = text.Substring(0, edit.span.Start) + edit.replacement + text.Substring(edit.span.End);

        var newOnes = ordered.Where(c => c.IsNew).ToList();
        if (newOnes.Count > 0)
        {
            var anchor = text.IndexOf("public static Dictionary<string, List<SkillCheckEvent>> LocationChecks", StringComparison.Ordinal);
            if (anchor < 0) anchor = text.IndexOf("public static List<SkillCheckEvent> TalkChecks", StringComparison.Ordinal);
            if (anchor < 0) throw new InvalidOperationException("LocationChecks/TalkChecks anchor not found");
            var lineStart = text.LastIndexOf('\n', anchor) + 1;
            var indent = ItemFileWriter.ExtractIndent(text, lineStart);
            var sb = new StringBuilder();
            foreach (var c in newOnes) sb.Append(Render(c, indent)).Append('\n');
            text = text.Substring(0, lineStart) + sb + text.Substring(lineStart);
        }

        File.WriteAllText(parser.FilePath, text);
    }

    private static string StripFromAggregators(string text, HashSet<string> deleted)
    {
        // Sweep both LocationChecks dictionary and TalkChecks list. Both contain
        // bare member-name references separated by commas.
        var locDict = new System.Text.RegularExpressions.Regex(
            @"\[""(?<loc>[^""]+)""\]\s*=\s*new\(\)\s*\{(?<body>[^}]*)\}",
            System.Text.RegularExpressions.RegexOptions.Multiline);
        text = locDict.Replace(text, m => RewriteListBody(m, "body", deleted));

        var talkList = new System.Text.RegularExpressions.Regex(
            @"public\s+static\s+List<SkillCheckEvent>\s+TalkChecks\s*=>\s*new\(\)\s*\{(?<body>[^}]*)\}\s*;",
            System.Text.RegularExpressions.RegexOptions.Multiline);
        text = talkList.Replace(text, m => RewriteListBody(m, "body", deleted));

        return text;
    }

    private static string RewriteListBody(System.Text.RegularExpressions.Match m, string group, HashSet<string> deleted)
    {
        var body = m.Groups[group].Value;
        var kept = body.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0 && !deleted.Contains(s))
            .ToList();
        var newBody = " " + string.Join(", ", kept) + " ";
        return m.Value.Replace(m.Groups[group].Value, newBody);
    }

    public static string Render(SkillCheckModel c, string indent = "    ")
    {
        var inner = indent + "    ";
        var sb = new StringBuilder();
        sb.Append(indent).Append("public static SkillCheckEvent ").Append(c.MemberName).Append(" => new()\n");
        sb.Append(indent).Append("{\n");
        sb.Append(inner).Append("Id = \"").Append(ItemFileWriter.Escape(c.Id)).Append("\",\n");
        sb.Append(inner).Append("Description = ").Append(ItemFileWriter.Quote(c.Description)).Append(",\n");
        sb.Append(inner).Append("SuccessText = ").Append(ItemFileWriter.Quote(c.SuccessText)).Append(",\n");
        sb.Append(inner).Append("FailText = ").Append(ItemFileWriter.Quote(c.FailText)).Append(",\n");
        if (!string.IsNullOrEmpty(c.FailPenaltyText))
            sb.Append(inner).Append("FailPenaltyText = ").Append(ItemFileWriter.Quote(c.FailPenaltyText)).Append(",\n");
        sb.Append(inner).Append("Skill = SkillType.").Append(c.Skill).Append(", ")
          .Append("Difficulty = CheckDifficulty.").Append(string.IsNullOrEmpty(c.Difficulty) ? "Moderate" : c.Difficulty).Append(",\n");
        sb.Append(inner).Append("TargetNumber = ").Append(c.TargetNumber)
          .Append(", CreditReward = ").Append(c.CreditReward)
          .Append(", UpgradePointReward = ").Append(c.UpgradePointReward).Append(",\n");
        if (c.Repeatable) sb.Append(inner).Append("Repeatable = true,\n");
        if (c.CreditPenalty != 0) sb.Append(inner).Append("CreditPenalty = ").Append(c.CreditPenalty).Append(",\n");
        if (!string.IsNullOrEmpty(c.CombatNpcOnFail))
            sb.Append(inner).Append("CombatNpcOnFail = NPCData.").Append(c.CombatNpcOnFail).Append(",\n");
        sb.Append(indent).Append("};\n");
        return sb.ToString();
    }
}
