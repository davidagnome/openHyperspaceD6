namespace TerminalHyperspace.Models;

public enum SkillType
{
    // Dexterity
    Agility, Blasters, Melee, Steal, Throw,
    // Knowledge
    Galaxy, Streetwise, Survival, Willpower, Xenology,
    // Mechanical
    Astrogation, Drive, Gunnery, Pilot, Sensors,
    // Perception
    Deceive, Hide, Persuade, Search, Tactics,
    // Strength
    Athletics, Brawl, Intimidate, Stamina, Swim,
    // Technical
    Armament, Computers, Droids, Medicine, Vehicles,
    // Force
    Alter, Control, Sense
}

public static class SkillMap
{
    private static readonly Dictionary<SkillType, AttributeType> Map = new()
    {
        [SkillType.Agility] = AttributeType.Dexterity,
        [SkillType.Blasters] = AttributeType.Dexterity,
        [SkillType.Melee] = AttributeType.Dexterity,
        [SkillType.Steal] = AttributeType.Dexterity,
        [SkillType.Throw] = AttributeType.Dexterity,

        [SkillType.Galaxy] = AttributeType.Knowledge,
        [SkillType.Streetwise] = AttributeType.Knowledge,
        [SkillType.Survival] = AttributeType.Knowledge,
        [SkillType.Willpower] = AttributeType.Knowledge,
        [SkillType.Xenology] = AttributeType.Knowledge,

        [SkillType.Astrogation] = AttributeType.Mechanical,
        [SkillType.Drive] = AttributeType.Mechanical,
        [SkillType.Gunnery] = AttributeType.Mechanical,
        [SkillType.Pilot] = AttributeType.Mechanical,
        [SkillType.Sensors] = AttributeType.Mechanical,

        [SkillType.Deceive] = AttributeType.Perception,
        [SkillType.Hide] = AttributeType.Perception,
        [SkillType.Persuade] = AttributeType.Perception,
        [SkillType.Search] = AttributeType.Perception,
        [SkillType.Tactics] = AttributeType.Perception,

        [SkillType.Athletics] = AttributeType.Strength,
        [SkillType.Brawl] = AttributeType.Strength,
        [SkillType.Intimidate] = AttributeType.Strength,
        [SkillType.Stamina] = AttributeType.Strength,
        [SkillType.Swim] = AttributeType.Strength,

        [SkillType.Armament] = AttributeType.Technical,
        [SkillType.Computers] = AttributeType.Technical,
        [SkillType.Droids] = AttributeType.Technical,
        [SkillType.Medicine] = AttributeType.Technical,
        [SkillType.Vehicles] = AttributeType.Technical,

        [SkillType.Alter] = AttributeType.Force,
        [SkillType.Control] = AttributeType.Force,
        [SkillType.Sense] = AttributeType.Force,
    };

    public static AttributeType GetAttribute(SkillType skill) => Map[skill];

    public static IEnumerable<SkillType> GetSkillsFor(AttributeType attr)
        => Map.Where(kv => kv.Value == attr).Select(kv => kv.Key);
}
