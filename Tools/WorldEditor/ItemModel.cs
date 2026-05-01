using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Mirrors Models/Item.cs so the WorldEditor can round-trip
/// Content/ItemData.cs property declarations.
public class ItemModel
{
    public string MemberName { get; set; } = "";  // C# identifier, e.g. "BlasterPistol"

    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsWeapon { get; set; }
    public int DamageDice { get; set; }
    public int DamagePips { get; set; }
    public string AttackSkill { get; set; } = "";  // empty = null in source
    public int Range { get; set; }
    public int Price { get; set; }
    public bool IsConsumable { get; set; }

    public bool IsMissionItem { get; set; }
    public string MissionDestinationLocationId { get; set; } = "";
    public string MissionDestinationName { get; set; } = "";

    public TextSpan? OriginalSpan { get; set; }
    public bool IsNew => OriginalSpan == null;

    public override string ToString() => $"{MemberName} — {Name}";
}
