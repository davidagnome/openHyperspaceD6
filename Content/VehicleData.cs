using TerminalHyperspace.Models;

namespace TerminalHyperspace.Content;

public static partial class VehicleData
{
    public static Vehicle LightFreighter => new()
    {
        Name = "YK-700 Light Freighter",
        Description = "A fast and reliable light freighter, popular with smugglers and independent haulers.",
        IsSpace = true,
        Maneuverability = new DiceCode(1, 1),
        Resolve = 24,
        Shield = ShieldData.ReconShields,
        Weapons = new()
        {
            new() { Name = "Dual Laser Cannon", Damage = new DiceCode(3), AttackSkill = SkillType.Gunnery }
        },
        Price = 1500
    };

    public static Vehicle Starfighter => new()
    {
        Name = "Viper-class Starfighter",
        Description = "A nimble single-seat starfighter built for speed and dogfighting.",
        IsSpace = true,
        Maneuverability = new DiceCode(2, 1),
        Resolve = 16,
        Shield = ShieldData.CivilianShields,
        Weapons = new()
        {
            new() { Name = "Laser Cannons", Damage = new DiceCode(3, 1), AttackSkill = SkillType.Gunnery },
            new() { Name = "Proton Torpedoes", Damage = new DiceCode(5), AttackSkill = SkillType.Gunnery }
        },
        Equipment = new()
        {
            new() { Name = "Basic Targeting Computer", BonusSkill = SkillType.Gunnery, Bonus = new DiceCode(0, 1) }
        },
        Price = 1000
    };

    public static Vehicle PatrolCruiser => new()
    {
        Name = "Enforcer Patrol Cruiser",
        Description = "A mid-size military patrol vessel. Well-armed but not very maneuverable.",
        IsSpace = true,
        Maneuverability = new DiceCode(1),
        Resolve = 32,
        Shield = ShieldData.FighterShields,
        Weapons = new()
        {
            new() { Name = "Turbolaser Battery", Damage = new DiceCode(4), AttackSkill = SkillType.Gunnery },
            new() { Name = "Ion Cannon", Damage = new DiceCode(3), AttackSkill = SkillType.Gunnery }
        },
        Equipment = new()
        {
            new() { Name = "Military Targeting Computer", BonusSkill = SkillType.Gunnery, Bonus = new DiceCode(0, 2) },
            new() { Name = "Advanced Sensors", BonusSkill = SkillType.Sensors, Bonus = new DiceCode(0, 1) }
        },
        Price = 3000
    };

    public static Vehicle Speeder => new()
    {
        Name = "X-34 Landspeeder",
        Description = "A rugged, open-topped repulsorlift vehicle for surface transport.",
        IsSpace = false,
        Maneuverability = new DiceCode(1, 2),
        Resolve = 14,
        Shield = ShieldData.Unshielded,
        Weapons = new(),
        Price = 200
    };

    public static Vehicle ArmoredTransport => new()
    {
        Name = "Kodiak Armored Transport",
        Description = "A heavily armored ground vehicle with mounted weapons.",
        IsSpace = false,
        Maneuverability = new DiceCode(0, 2),
        Resolve = 28,
        Shield = ShieldData.CivilianShields,
        Weapons = new()
        {
            new() { Name = "Mounted Repeating Blaster", Damage = new DiceCode(3, 2), AttackSkill = SkillType.Gunnery }
        },
        Price = 800
    };

    public static Vehicle CombatSpeeder => new()
    {
        Name = "Talon Combat Speeder",
        Description = "A fast attack speeder with forward-mounted cannons.",
        IsSpace = false,
        Maneuverability = new DiceCode(2),
        Resolve = 16,
        Shield = ShieldData.CivilianShields,
        Weapons = new()
        {
            new() { Name = "Light Blaster Cannon", Damage = new DiceCode(2, 2), AttackSkill = SkillType.Gunnery }
        },
        Price = 500
    };

    public static List<Vehicle> SpaceVehicles => new() { LightFreighter, Starfighter, PatrolCruiser };
    public static List<Vehicle> LandVehicles => new() { Speeder, ArmoredTransport, CombatSpeeder };
}
