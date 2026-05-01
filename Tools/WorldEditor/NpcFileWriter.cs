using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

public static class NpcFileWriter
{
    public static void Save(NpcFileParser parser, IEnumerable<NpcModel> npcs)
    {
        var ordered = npcs.ToList();
        var text = parser.SourceText;

        var keptSpans = new HashSet<TextSpan>(
            ordered.Where(n => n.OriginalSpan.HasValue).Select(n => n.OriginalSpan!.Value));

        var edits = new List<(TextSpan span, string replacement)>();
        foreach (var n in ordered.Where(n => n.OriginalSpan.HasValue))
        {
            var span = n.OriginalSpan!.Value;
            var indent = ItemFileWriter.ExtractIndent(text, span.Start);
            edits.Add((span, "\n" + Render(n, indent)));
        }
        foreach (var orig in parser.Npcs.Where(n => n.OriginalSpan.HasValue && !keptSpans.Contains(n.OriginalSpan!.Value)))
            edits.Add((orig.OriginalSpan!.Value, ""));

        foreach (var edit in edits.OrderByDescending(e => e.span.Start))
            text = text.Substring(0, edit.span.Start) + edit.replacement + text.Substring(edit.span.End);

        // Insert new NPCs immediately before the closing brace of the NPCData class.
        var newOnes = ordered.Where(n => n.IsNew).ToList();
        if (newOnes.Count > 0)
        {
            var anchor = text.LastIndexOf('}');
            if (anchor < 0) throw new InvalidOperationException("No class closing brace found");
            // Walk back to start of that line for indent.
            var lineStart = text.LastIndexOf('\n', anchor) + 1;
            var indent = ItemFileWriter.ExtractIndent(text, lineStart) + "    ";
            var sb = new StringBuilder();
            foreach (var n in newOnes) sb.Append(Render(n, indent)).Append('\n');
            text = text.Substring(0, lineStart) + sb + text.Substring(lineStart);
        }

        File.WriteAllText(parser.FilePath, text);
    }

    public static string Render(NpcModel m, string indent = "    ")
    {
        var inner = indent + "    ";
        var inner2 = inner + "    ";
        var sb = new StringBuilder();
        sb.Append(indent).Append("public static Character ").Append(m.MemberName).Append("() => new()\n");
        sb.Append(indent).Append("{\n");
        sb.Append(inner).Append("Name = \"").Append(ItemFileWriter.Escape(m.DisplayName)).Append("\",\n");
        sb.Append(inner).Append("IsPlayer = ").Append(m.IsPlayer ? "true" : "false").Append(",\n");

        if (m.Attributes.Count > 0)
        {
            sb.Append(inner).Append("Attributes = new()\n").Append(inner).Append("{\n");
            foreach (var (k, v) in m.Attributes)
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
            sb.Append(inner).Append("},\n");
        }

        if (m.Inventory.Count > 0)
            sb.Append(inner).Append("Inventory = new() { ")
              .Append(string.Join(", ", m.Inventory.Select(i => $"ItemData.{i}")))
              .Append(" },\n");

        if (!string.IsNullOrEmpty(m.EquippedWeaponMember))
            sb.Append(inner).Append("EquippedWeapon = ItemData.").Append(m.EquippedWeaponMember).Append(",\n");

        if (!string.IsNullOrEmpty(m.EquippedArmorMember))
            sb.Append(inner).Append("EquippedArmor = ArmorData.").Append(m.EquippedArmorMember).Append(",\n");

        sb.Append(indent).Append("};\n");
        return sb.ToString();
    }
}
