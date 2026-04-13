namespace TerminalHyperspace.Models;

public class Role
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public Dictionary<AttributeType, DiceCode> AttributeBonuses { get; set; } = new();
    public Dictionary<SkillType, DiceCode> SkillBonuses { get; set; } = new();
}
