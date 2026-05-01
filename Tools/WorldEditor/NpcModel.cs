using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Mirrors a Models/Character entry as authored in Content/NPCData.cs.
/// Each NPC is a public static method like:
///   public static Character Stormtrooper() => new() { Name = "...", ... };
public class NpcModel
{
    public string MemberName { get; set; } = "";  // "Stormtrooper"
    public string DisplayName { get; set; } = "";
    public bool IsPlayer { get; set; }

    // Attributes — keyed by AttributeType enum name (Dexterity, Knowledge, …).
    public Dictionary<string, (int Dice, int Pips)> Attributes { get; set; } = new();

    // SkillBonuses — keyed by SkillType enum name.
    public Dictionary<string, (int Dice, int Pips)> SkillBonuses { get; set; } = new();

    // Item member-name references (e.g. "BlasterRifle") to ItemData.*
    public List<string> Inventory { get; set; } = new();
    public string EquippedWeaponMember { get; set; } = "";   // empty = null
    public string EquippedArmorMember  { get; set; } = "";   // empty = null

    public TextSpan? OriginalSpan { get; set; }
    public bool IsNew => OriginalSpan == null;

    public override string ToString() => $"{MemberName} — {DisplayName}";
}
