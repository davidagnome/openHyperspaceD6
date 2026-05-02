using TerminalHyperspace.Models;

namespace TerminalHyperspace.Content;

public class SpaceEncounter
{
    public Character Pilot { get; set; } = new();
    public Vehicle Ship { get; set; } = new();
}

public static partial class SpaceEncounterData
{

    public static SpaceEncounter PirateInterceptor() => new()
    {
        Pilot = new Character
        {
            Name = "Pirate Pilot",
            IsPlayer = false,
            Attributes = new()
            {
                [AttributeType.Dexterity] = new DiceCode(2, 1),
                [AttributeType.Knowledge] = new DiceCode(1, 1),
                [AttributeType.Mechanical] = new DiceCode(2, 2),
                [AttributeType.Perception] = new DiceCode(2),
                [AttributeType.Strength] = new DiceCode(2),
                [AttributeType.Technical] = new DiceCode(1, 2),
                [AttributeType.Force] = new DiceCode(0),
            },
            SkillBonuses = new()
            {
                [SkillType.Pilot] = new DiceCode(0, 2),
                [SkillType.Gunnery] = new DiceCode(0, 2),
                [SkillType.Sensors] = new DiceCode(0, 1),
            },
            EquippedArmor = ArmorData.PaddedFlightSuit,
        },
        Ship = new Vehicle
        {
            Name = "Reaver Interceptor",
            Description = "A stripped-down attack fighter cobbled together from salvage. Fast and mean.",
            IsSpace = true,
            Maneuverability = new DiceCode(2),
            Resolve = 14,
            Shield = ShieldData.CivilianShields,
            Weapons = new()
            {
                new() { Name = "Jury-rigged Laser Cannons", Damage = new DiceCode(3), AttackSkill = SkillType.Gunnery },
            },
        }
    };

    public static SpaceEncounter ImperialPatrol() => new()
    {
        Pilot = new Character
        {
            Name = "Imperial TIE Pilot",
            IsPlayer = false,
            Attributes = new()
            {
                [AttributeType.Dexterity] = new DiceCode(2),
                [AttributeType.Knowledge] = new DiceCode(1),
                [AttributeType.Mechanical] = new DiceCode(3),
                [AttributeType.Perception] = new DiceCode(2),
                [AttributeType.Strength] = new DiceCode(1),
                [AttributeType.Technical] = new DiceCode(2),
                [AttributeType.Force] = new DiceCode(0),
            },
            SkillBonuses = new()
            {
                [SkillType.Pilot] = new DiceCode(1),
                [SkillType.Gunnery] = new DiceCode(0, 2),
                [SkillType.Tactics] = new DiceCode(0, 1),
            },
            EquippedArmor = ArmorData.PaddedFlightSuit,
        },
        Ship = new Vehicle
        {
            Name = "TIE/ln Fighter",
            Description = "Standard Imperial starfighter. Agile but fragile—no shields.",
            IsSpace = true,
            Maneuverability = new DiceCode(2, 2),
            Resolve = 10,
            Shield = ShieldData.Unshielded,
            Weapons = new()
            {
                new() { Name = "Twin Laser Cannons", Damage = new DiceCode(3, 1), AttackSkill = SkillType.Gunnery },
            },
        }
    };

    public static SpaceEncounter BountyHunterShip() => new()
    {
        Pilot = new Character
        {
            Name = "Bounty Hunter Pilot",
            IsPlayer = false,
            Attributes = new()
            {
                [AttributeType.Dexterity] = new DiceCode(2, 2),
                [AttributeType.Knowledge] = new DiceCode(2),
                [AttributeType.Mechanical] = new DiceCode(3),
                [AttributeType.Perception] = new DiceCode(2, 2),
                [AttributeType.Strength] = new DiceCode(2, 1),
                [AttributeType.Technical] = new DiceCode(2, 1),
                [AttributeType.Force] = new DiceCode(0),
            },
            SkillBonuses = new()
            {
                [SkillType.Pilot] = new DiceCode(1),
                [SkillType.Gunnery] = new DiceCode(1),
                [SkillType.Sensors] = new DiceCode(0, 2),
                [SkillType.Astrogation] = new DiceCode(0, 1),
            },
            EquippedArmor = ArmorData.MediumArmor,
        },
        Ship = new Vehicle
        {
            Name = "Firespray Pursuit Craft",
            Description = "A heavily armed pursuit vessel favored by professional hunters.",
            IsSpace = true,
            Maneuverability = new DiceCode(1, 2),
            Resolve = 22,
            Shield = ShieldData.FighterShields,
            Weapons = new()
            {
                new() { Name = "Twin Blaster Cannons", Damage = new DiceCode(3, 2), AttackSkill = SkillType.Gunnery },
                new() { Name = "Concussion Missiles", Damage = new DiceCode(5, 1), AttackSkill = SkillType.Gunnery },
            },
            Equipment = new()
            {
                new() { Name = "Targeting Computer", BonusSkill = SkillType.Gunnery, Bonus = new DiceCode(0, 2) },
            },
        }
    };

    public static SpaceEncounter SmugglerFreighter() => new()
    {
        Pilot = new Character
        {
            Name = "Smuggler Captain",
            IsPlayer = false,
            Attributes = new()
            {
                [AttributeType.Dexterity] = new DiceCode(2, 1),
                [AttributeType.Knowledge] = new DiceCode(2),
                [AttributeType.Mechanical] = new DiceCode(2, 2),
                [AttributeType.Perception] = new DiceCode(2, 1),
                [AttributeType.Strength] = new DiceCode(2),
                [AttributeType.Technical] = new DiceCode(2, 1),
                [AttributeType.Force] = new DiceCode(0),
            },
            SkillBonuses = new()
            {
                [SkillType.Pilot] = new DiceCode(0, 2),
                [SkillType.Gunnery] = new DiceCode(0, 1),
                [SkillType.Astrogation] = new DiceCode(0, 2),
                [SkillType.Deceive] = new DiceCode(0, 2),
            },
            EquippedArmor = ArmorData.PaddedFlightSuit,
        },
        Ship = new Vehicle
        {
            Name = "Modified YT Freighter",
            Description = "A battered freighter with some unexpected upgrades under the hull.",
            IsSpace = true,
            Maneuverability = new DiceCode(1, 1),
            Resolve = 20,
            Shield = ShieldData.ReconShields,
            Weapons = new()
            {
                new() { Name = "Dorsal Laser Turret", Damage = new DiceCode(3), AttackSkill = SkillType.Gunnery },
            },
            Equipment = new()
            {
                new() { Name = "Sensor Jammer", BonusSkill = SkillType.Pilot, Bonus = new DiceCode(0, 1) },
            },
        }
    };

    public static SpaceEncounter ImperialGunboat() => new()
    {
        Pilot = new Character
        {
            Name = "Imperial Gunboat Crew",
            IsPlayer = false,
            Attributes = new()
            {
                [AttributeType.Dexterity] = new DiceCode(2),
                [AttributeType.Knowledge] = new DiceCode(2, 1),
                [AttributeType.Mechanical] = new DiceCode(3, 1),
                [AttributeType.Perception] = new DiceCode(2, 1),
                [AttributeType.Strength] = new DiceCode(2),
                [AttributeType.Technical] = new DiceCode(2, 2),
                [AttributeType.Force] = new DiceCode(0),
            },
            SkillBonuses = new()
            {
                [SkillType.Pilot] = new DiceCode(1),
                [SkillType.Gunnery] = new DiceCode(1, 1),
                [SkillType.Sensors] = new DiceCode(0, 2),
                [SkillType.Tactics] = new DiceCode(0, 2),
            },
            EquippedArmor = ArmorData.LightArmor,
        },
        Ship = new Vehicle
        {
            Name = "Assault Gunboat",
            Description = "A heavy Imperial attack craft bristling with weapons. Not to be trifled with.",
            IsSpace = true,
            Maneuverability = new DiceCode(1, 1),
            Resolve = 28,
            Shield = ShieldData.BomberShields,
            Weapons = new()
            {
                new() { Name = "Heavy Laser Cannons", Damage = new DiceCode(4), AttackSkill = SkillType.Gunnery },
                new() { Name = "Proton Torpedoes", Damage = new DiceCode(5, 2), AttackSkill = SkillType.Gunnery },
            },
            Equipment = new()
            {
                new() { Name = "Military Targeting Computer", BonusSkill = SkillType.Gunnery, Bonus = new DiceCode(1) },
                new() { Name = "Advanced Sensors", BonusSkill = SkillType.Sensors, Bonus = new DiceCode(0, 2) },
            },
        }
    };

    public static SpaceEncounter TieBomber() => new()
    {
        Pilot = new Character
        {
            Name = "Imperial Navy Pilot",
            IsPlayer = false,
            SkillBonuses = new()
            {
                [SkillType.Gunnery] = new DiceCode(5),
                [SkillType.Pilot] = new DiceCode(4),
            },
            EquippedArmor = ArmorData.PaddedFlightSuit,
        },
        Ship = new Vehicle
        {
            Name = "TIE Bomber",
            IsSpace = true,
            Maneuverability = new DiceCode(0),
            Resolve = 9,
            Shield = ShieldData.Unshielded,
        }
    };

    public static SpaceEncounter ImperialLamdaShuttle() => new()
    {
        Pilot = new Character
        {
            Name = "Imperial Navy Courier",
            IsPlayer = false,
            SkillBonuses = new()
            {
                [SkillType.Gunnery] = new DiceCode(5),
                [SkillType.Sensors] = new DiceCode(5),
                [SkillType.Pilot] = new DiceCode(4),
            },
            EquippedArmor = ArmorData.PaddedFlightSuit,
        },
        Ship = new Vehicle
        {
            Name = "Lamda Imperial Shuttle",
            IsSpace = true,
            Maneuverability = new DiceCode(1),
            Resolve = 16,
            Weapons = new()
            {
                new() { Name = "Double-Blaster Cannon", Damage = new DiceCode(4), AttackSkill = SkillType.Gunnery },
            },
            Equipment = new()
            {
                new() { Name = "Long Range Sensor", BonusSkill = SkillType.Sensors, Bonus = new DiceCode(1, 1) },
            },
        }
    };

    public static List<Func<SpaceEncounter>> AllEncounters => new()
    {
        PirateInterceptor, ImperialPatrol, BountyHunterShip, SmugglerFreighter, ImperialGunboat
    };
}
