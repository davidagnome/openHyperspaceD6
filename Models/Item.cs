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

    public override string ToString()
        => IsWeapon ? $"{Name} (Dmg: {Damage}, Skill: {AttackSkill})" : Name;
}
