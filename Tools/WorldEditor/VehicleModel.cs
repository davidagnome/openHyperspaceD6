using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

public class VehicleWeaponModel
{
    public string Name { get; set; } = "";
    public int DamageDice { get; set; }
    public int DamagePips { get; set; }
    public string AttackSkill { get; set; } = "Gunnery";
}

public class VehicleEquipmentModel
{
    public string Name { get; set; } = "";
    public string BonusSkill { get; set; } = "";
    public int BonusDice { get; set; }
    public int BonusPips { get; set; }
}

public class VehicleModel
{
    public string MemberName { get; set; } = "";

    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsSpace { get; set; }
    public int ManeuverDice { get; set; }
    public int ManeuverPips { get; set; }
    public int Resolve { get; set; }
    public string ShieldMember { get; set; } = "";  // ShieldData.* member
    public List<VehicleWeaponModel> Weapons { get; set; } = new();
    public List<VehicleEquipmentModel> Equipment { get; set; } = new();
    public int Price { get; set; }

    public TextSpan? OriginalSpan { get; set; }
    public bool IsNew => OriginalSpan == null;

    public override string ToString() => $"{MemberName} — {Name}";
}
