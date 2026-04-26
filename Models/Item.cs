namespace TerminalHyperspace.Models;

public class Item
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsWeapon { get; set; }
    public DiceCode Damage { get; set; }
    public SkillType? AttackSkill { get; set; }
    public int Range { get; set; } // 0 = melee
    public int Price { get; set; }
    public bool IsConsumable { get; set; }

    /// Set when the item is granted by an active Delivery mission. The inventory
    /// view shows the destination so the player knows where to bring it.
    public bool IsMissionItem { get; set; }
    public string? MissionDestinationLocationId { get; set; }
    public string? MissionDestinationName { get; set; }

    public override string ToString()
        => IsWeapon ? $"{Name} (Dmg: {Damage}, Skill: {AttackSkill})" : Name;
}
