namespace TerminalHyperspace.Models;

public class Species
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public Dictionary<AttributeType, DiceCode> BaseAttributes { get; set; } = new();
    public Dictionary<SkillType, DiceCode> SkillBonuses { get; set; } = new();
}
