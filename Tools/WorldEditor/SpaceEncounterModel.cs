using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Mirrors a Content/SpaceEncounterData.cs factory like:
///   public static SpaceEncounter PirateInterceptor() => new()
///   { Pilot = new Character { ... }, Ship = new Vehicle { ... } };
/// Pilot reuses the NPC field shape, Ship reuses the Vehicle field shape.
public class SpaceEncounterModel
{
    public string MemberName { get; set; } = "";

    // Pilot (subset of the NPC fields used inside SpaceEncounter pilots).
    public string PilotName { get; set; } = "";
    public bool PilotIsPlayer { get; set; }
    public Dictionary<string, (int Dice, int Pips)> PilotAttributes { get; set; } = new();
    public Dictionary<string, (int Dice, int Pips)> PilotSkillBonuses { get; set; } = new();
    public string PilotEquippedArmorMember { get; set; } = "";

    // Ship (subset of the Vehicle fields used inside SpaceEncounter ships).
    public string ShipName { get; set; } = "";
    public string ShipDescription { get; set; } = "";
    public bool ShipIsSpace { get; set; } = true;
    public int ShipManeuverDice { get; set; }
    public int ShipManeuverPips { get; set; }
    public int ShipResolve { get; set; }
    public string ShipShieldMember { get; set; } = "";
    public List<VehicleWeaponModel> ShipWeapons { get; set; } = new();
    public List<VehicleEquipmentModel> ShipEquipment { get; set; } = new();

    public TextSpan? OriginalSpan { get; set; }
    public bool IsNew => OriginalSpan == null;

    public override string ToString() => $"{MemberName} — {ShipName}";
}
