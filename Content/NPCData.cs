using TerminalHyperspace.Models;

namespace TerminalHyperspace.Content;

public static class NPCData
{
    public static Character Stormtrooper() => new()
    {
        Name = "Stormtrooper",
        IsPlayer = false,
        Attributes = new()
        {
            [AttributeType.Dexterity] = new DiceCode(2),
            [AttributeType.Knowledge] = new DiceCode(1, 2),
            [AttributeType.Mechanical] = new DiceCode(1, 2),
            [AttributeType.Perception] = new DiceCode(1, 2),
            [AttributeType.Strength] = new DiceCode(2),
            [AttributeType.Technical] = new DiceCode(1, 2),
            [AttributeType.Force] = new DiceCode(0),
        },
        SkillBonuses = new()
        {
            [SkillType.Blasters] = new DiceCode(0, 2),
            [SkillType.Brawl] = new DiceCode(0, 1),
        },
        Inventory = new() { ItemData.BlasterRifle },
        EquippedWeapon = ItemData.BlasterRifle,
        EquippedArmor = ArmorData.MediumArmor,
    };

    public static Character PirateThugs() => new()
    {
        Name = "Pirate Thug",
        IsPlayer = false,
        Attributes = new()
        {
            [AttributeType.Dexterity] = new DiceCode(2, 1),
            [AttributeType.Knowledge] = new DiceCode(1, 1),
            [AttributeType.Mechanical] = new DiceCode(2),
            [AttributeType.Perception] = new DiceCode(2),
            [AttributeType.Strength] = new DiceCode(2, 1),
            [AttributeType.Technical] = new DiceCode(1, 1),
            [AttributeType.Force] = new DiceCode(0),
        },
        SkillBonuses = new()
        {
            [SkillType.Blasters] = new DiceCode(0, 1),
            [SkillType.Melee] = new DiceCode(0, 2),
            [SkillType.Intimidate] = new DiceCode(0, 1),
        },
        Inventory = new() { ItemData.Vibroblade },
        EquippedWeapon = ItemData.Vibroblade,
        EquippedArmor = ArmorData.PaddedFlightSuit,
    };

    public static Character BountyHunter() => new()
    {
        Name = "Bounty Hunter",
        IsPlayer = false,
        Attributes = new()
        {
            [AttributeType.Dexterity] = new DiceCode(3),
            [AttributeType.Knowledge] = new DiceCode(2),
            [AttributeType.Mechanical] = new DiceCode(2, 1),
            [AttributeType.Perception] = new DiceCode(2, 2),
            [AttributeType.Strength] = new DiceCode(2, 1),
            [AttributeType.Technical] = new DiceCode(2),
            [AttributeType.Force] = new DiceCode(0),
        },
        SkillBonuses = new()
        {
            [SkillType.Blasters] = new DiceCode(1),
            [SkillType.Search] = new DiceCode(0, 2),
            [SkillType.Intimidate] = new DiceCode(0, 1),
            [SkillType.Streetwise] = new DiceCode(0, 1),
            [SkillType.Pilot] = new DiceCode(0, 2),
        },
        Inventory = new() { ItemData.HeavyBlaster },
        EquippedWeapon = ItemData.HeavyBlaster,
        EquippedArmor = ArmorData.HeavyArmor,
    };

    public static Character ImperialOfficer() => new()
    {
        Name = "Imperial Officer",
        IsPlayer = false,
        Attributes = new()
        {
            [AttributeType.Dexterity] = new DiceCode(2),
            [AttributeType.Knowledge] = new DiceCode(3),
            [AttributeType.Mechanical] = new DiceCode(2, 1),
            [AttributeType.Perception] = new DiceCode(3),
            [AttributeType.Strength] = new DiceCode(2),
            [AttributeType.Technical] = new DiceCode(2, 1),
            [AttributeType.Force] = new DiceCode(0),
        },
        SkillBonuses = new()
        {
            [SkillType.Tactics] = new DiceCode(1),
            [SkillType.Willpower] = new DiceCode(0, 2),
            [SkillType.Persuade] = new DiceCode(0, 1),
            [SkillType.Blasters] = new DiceCode(0, 1),
        },
        Inventory = new() { ItemData.BlasterPistol },
        EquippedWeapon = ItemData.BlasterPistol,
        EquippedArmor = ArmorData.LightArmor,
    };

    public static Character DarkAdept() => new()
    {
        Name = "Dark Adept",
        IsPlayer = false,
        Attributes = new()
        {
            [AttributeType.Dexterity] = new DiceCode(2, 1),
            [AttributeType.Knowledge] = new DiceCode(2, 2),
            [AttributeType.Mechanical] = new DiceCode(1, 2),
            [AttributeType.Perception] = new DiceCode(3),
            [AttributeType.Strength] = new DiceCode(2),
            [AttributeType.Technical] = new DiceCode(2),
            [AttributeType.Force] = new DiceCode(3, 1),
        },
        SkillBonuses = new()
        {
            [SkillType.Control] = new DiceCode(0, 2),
            [SkillType.Alter] = new DiceCode(1),
            [SkillType.Sense] = new DiceCode(0, 1),
            [SkillType.Melee] = new DiceCode(0, 2),
            [SkillType.Intimidate] = new DiceCode(0, 2),
        },
        Inventory = new() { ItemData.ForcePike },
        EquippedWeapon = ItemData.ForcePike,
        EquippedArmor = ArmorData.LightArmor,
    };

    public static Character Merchant() => new()
    {
        Name = "Merchant",
        IsPlayer = false,
        Attributes = new()
        {
            [AttributeType.Dexterity] = new DiceCode(1, 2),
            [AttributeType.Knowledge] = new DiceCode(2, 1),
            [AttributeType.Mechanical] = new DiceCode(2),
            [AttributeType.Perception] = new DiceCode(2, 2),
            [AttributeType.Strength] = new DiceCode(1, 2),
            [AttributeType.Technical] = new DiceCode(2),
            [AttributeType.Force] = new DiceCode(0),
        },
        SkillBonuses = new()
        {
            [SkillType.Persuade] = new DiceCode(1),
            [SkillType.Deceive] = new DiceCode(0, 2),
            [SkillType.Streetwise] = new DiceCode(0, 1),
            [SkillType.Galaxy] = new DiceCode(0, 1),
        },
        Inventory = new() { ItemData.BlasterPistol },
        EquippedWeapon = ItemData.BlasterPistol,
    };

    public static Character CreatureSmall() => new()
    {
        Name = "Tunnel Crawler",
        IsPlayer = false,
        Attributes = new()
        {
            [AttributeType.Dexterity] = new DiceCode(3),
            [AttributeType.Knowledge] = new DiceCode(0, 1),
            [AttributeType.Mechanical] = new DiceCode(0),
            [AttributeType.Perception] = new DiceCode(2, 1),
            [AttributeType.Strength] = new DiceCode(2),
            [AttributeType.Technical] = new DiceCode(0),
            [AttributeType.Force] = new DiceCode(0),
        },
        SkillBonuses = new()
        {
            [SkillType.Brawl] = new DiceCode(0, 2),
            [SkillType.Hide] = new DiceCode(0, 2),
            [SkillType.Agility] = new DiceCode(0, 1),
        },
    };

    public static Character CreatureLarge() => new()
    {
        Name = "Rift Stalker",
        IsPlayer = false,
        Attributes = new()
        {
            [AttributeType.Dexterity] = new DiceCode(2),
            [AttributeType.Knowledge] = new DiceCode(0, 1),
            [AttributeType.Mechanical] = new DiceCode(0),
            [AttributeType.Perception] = new DiceCode(2, 2),
            [AttributeType.Strength] = new DiceCode(4),
            [AttributeType.Technical] = new DiceCode(0),
            [AttributeType.Force] = new DiceCode(0),
        },
        SkillBonuses = new()
        {
            [SkillType.Brawl] = new DiceCode(1),
            [SkillType.Intimidate] = new DiceCode(0, 2),
            [SkillType.Stamina] = new DiceCode(0, 2),
        },
    };
}
