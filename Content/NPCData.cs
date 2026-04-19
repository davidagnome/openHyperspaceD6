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
            [AttributeType.Dexterity] = new DiceCode(3),
            [AttributeType.Knowledge] = new DiceCode(2),
            [AttributeType.Mechanical] = new DiceCode(2),
            [AttributeType.Perception] = new DiceCode(2),
            [AttributeType.Strength] = new DiceCode(2),
            [AttributeType.Technical] = new DiceCode(2),
            [AttributeType.Force] = new DiceCode(0),
        },
        SkillBonuses = new()
        {
            // Bonuses are added to parent attribute. Intended totals: Blasters 4D, Brawl 3D
            [SkillType.Blasters] = new DiceCode(1), // Dex 3D + 1D = 4D
            [SkillType.Brawl] = new DiceCode(1),    // Str 2D + 1D = 3D
        },
        Inventory = new() { ItemData.BlasterRifle },
        EquippedWeapon = ItemData.BlasterRifle,
        EquippedArmor = ArmorData.MediumArmor,
    };

    public static Character Snowtrooper() => new()
    {
        Name = "Snowtrooper",
        IsPlayer = false,
        Attributes = new()
        {
            [AttributeType.Dexterity] = new DiceCode(2),
            [AttributeType.Knowledge] = new DiceCode(2),
            [AttributeType.Mechanical] = new DiceCode(2),
            [AttributeType.Perception] = new DiceCode(2),
            [AttributeType.Strength] = new DiceCode(2),
            [AttributeType.Technical] = new DiceCode(2),
            [AttributeType.Force] = new DiceCode(0),
        },
        SkillBonuses = new()
        {
            // Bonuses are added to parent attribute. Intended totals: Blasters 5D,
            // Armament 4D, Brawl 4D, Search 3D+1, Survival 4D (all attrs are 2D)
            [SkillType.Blasters] = new DiceCode(3),    // Dex 2D + 3D = 5D
            [SkillType.Armament] = new DiceCode(2),    // Tech 2D + 2D = 4D
            [SkillType.Brawl] = new DiceCode(2),       // Str 2D + 2D = 4D
            [SkillType.Search] = new DiceCode(1, 1),   // Per 2D + 1D+1 = 3D+1
            [SkillType.Survival] = new DiceCode(2),    // Know 2D + 2D = 4D
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
            [AttributeType.Mechanical] = new DiceCode(2),
            [AttributeType.Perception] = new DiceCode(3),
            [AttributeType.Strength] = new DiceCode(2),
            [AttributeType.Technical] = new DiceCode(2),
            [AttributeType.Force] = new DiceCode(0),
        },
        SkillBonuses = new()
        {
            [SkillType.Tactics] = new DiceCode(1),
            [SkillType.Willpower] = new DiceCode(0, 2),
            [SkillType.Persuade] = new DiceCode(0, 1),
            [SkillType.Blasters] = new DiceCode(0, 2),
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

    public static Character TuskenRaider() => new()
    {
        Name = "Tusken Raider",
        IsPlayer = false,
        Attributes = new()
        {
            [AttributeType.Dexterity] = new DiceCode(2, 1),
            [AttributeType.Knowledge] = new DiceCode(1),
            [AttributeType.Mechanical] = new DiceCode(1),
            [AttributeType.Perception] = new DiceCode(2, 2),
            [AttributeType.Strength] = new DiceCode(2, 1),
            [AttributeType.Technical] = new DiceCode(1, 1),
            [AttributeType.Force] = new DiceCode(0),
        },
        SkillBonuses = new()
        {
            // Intended totals: Blasters 4D, Melee 4D, Hide 3D+2, Survival 3D, Search 3D+1
            [SkillType.Blasters] = new DiceCode(1, 2),   // Dex 2D+1 + 1D+2 = 4D
            [SkillType.Melee] = new DiceCode(1, 2),      // Dex 2D+1 + 1D+2 = 4D
            [SkillType.Hide] = new DiceCode(1),           // Per 2D+2 + 1D = 3D+2
            [SkillType.Survival] = new DiceCode(2),      // Know 1D + 2D = 3D
            [SkillType.Search] = new DiceCode(0, 2),     // Per 2D+2 + 0D+2 = 3D+1
        },
        Inventory = new() { ItemData.BlasterRifle, ItemData.Vibroblade },
        EquippedWeapon = ItemData.BlasterRifle,
        EquippedArmor = ArmorData.PaddedFlightSuit,
    };

    public static Character Diagnoga() => new()
    {
        Name = "Diagnoga",
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
