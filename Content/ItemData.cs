using TerminalHyperspace.Models;

namespace TerminalHyperspace.Content;

public static partial class ItemData
{
    public static Item BlasterPistol => new()
    {
        Name = "Blaster Pistol",
        Description = "Standard-issue sidearm. Reliable and common across the galaxy.",
        IsWeapon = true, Damage = new DiceCode(3), AttackSkill = SkillType.Blasters, Range = 30,
        Price = 50
    };

    public static Item HeavyBlaster => new()
    {
        Name = "Heavy Blaster Pistol",
        Description = "A more powerful sidearm favored by smugglers and bounty hunters.",
        IsWeapon = true, Damage = new DiceCode(3, 2), AttackSkill = SkillType.Blasters, Range = 25,
        Price = 120
    };

    public static Item BlasterRifle => new()
    {
        Name = "Blaster Rifle",
        Description = "Military-grade long-range weapon with superior stopping power.",
        IsWeapon = true, Damage = new DiceCode(4), AttackSkill = SkillType.Blasters, Range = 60,
        Price = 200
    };

    public static Item Vibroblade => new()
    {
        Name = "Vibroblade",
        Description = "A melee weapon with a vibrating blade that can cut through armor.",
        IsWeapon = true, Damage = new DiceCode(3, 1), AttackSkill = SkillType.Melee, Range = 0,
        Price = 80
    };

    public static Item VibroAxe => new()
    {
        Name = "Vibro-Axe",
        Description = "A melee weapon with a vibrating blade that can cut through armor.",
        IsWeapon = true, Damage = new DiceCode(3, 3), AttackSkill = SkillType.Melee, Range = 0,
        Price = 150
    };

    public static Item ForcePike => new()
    {
        Name = "Force Pike",
        Description = "An electrified polearm that delivers a nasty shock on contact.",
        IsWeapon = true, Damage = new DiceCode(3, 2), AttackSkill = SkillType.Melee, Range = 0,
        Price = 180
    };

    public static Item ThermalDetonator => new()
    {
        Name = "Thermal Detonator",
        Description = "A devastating explosive device. Handle with extreme caution.",
        IsWeapon = true, Damage = new DiceCode(5, 2), AttackSkill = SkillType.Throw, Range = 15,
        Price = 300, IsConsumable = true
    };

    public static Item ConcussionGrenades => new()
    {
        Name = "Concussion Grenade",
        Description = "Crowd-control device leaving targets stunned.",
        IsWeapon = true, Damage = new DiceCode(5), AttackSkill = SkillType.Throw, Range = 15,
        Price = 300, IsConsumable = true
    };

    public static Item Medpack => new()
    {
        Name = "Medpack",
        Description = "Restores some Resolve when used. Requires a Medicine check.",
        IsWeapon = false, Price = 30, IsConsumable = true
    };

    public static Item DataPad => new()
    {
        Name = "DataPad",
        Description = "A portable computing device. Useful for slicing terminals and accessing data.",
        IsWeapon = false, Price = 40
    };

    public static Item Macrobinoculars => new()
    {
        Name = "Macrobinoculars",
        Description = "Enhances long-range visual scanning. +1 pip to Search at range.",
        IsWeapon = false, Price = 60
    };

    public static List<Item> StarterWeapons => new() { BlasterPistol, Vibroblade };

    public static List<Item> AllWeapons => new()
    {
        BlasterPistol, HeavyBlaster, BlasterRifle, Vibroblade, VibroAxe, ForcePike, ThermalDetonator, ConcussionGrenades
    };

    public static List<Item> AllItems => new()
    {
        BlasterPistol, HeavyBlaster, BlasterRifle, Vibroblade, VibroAxe, ForcePike,
        ThermalDetonator, Medpack, DataPad, Macrobinoculars, ConcussionGrenades
    };
}
