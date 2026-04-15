using TerminalHyperspace.Models;

namespace TerminalHyperspace.Content;

public static class ArmorData
{
    public static readonly Armor Unarmored = new()
    {
        Name = "Unarmored", DiceCode = new DiceCode(0), Price = 0
    };

    public static readonly Armor PaddedFlightSuit = new()
    {
        Name = "Padded Flight Suit", DiceCode = new DiceCode(1), Price = 75
    };

    public static readonly Armor LightArmor = new()
    {
        Name = "Light Armor", DiceCode = new DiceCode(2), Price = 200
    };

    public static readonly Armor MediumArmor = new()
    {
        Name = "Medium Armor", DiceCode = new DiceCode(3), Price = 500
    };

    public static readonly Armor HeavyArmor = new()
    {
        Name = "Heavy Armor", DiceCode = new DiceCode(4), Price = 1000
    };

    public static readonly Armor BattleArmor = new()
    {
        Name = "Battle Armor", DiceCode = new DiceCode(5), Price = 2000
    };

    public static readonly Armor ThermalSuit = new()
    {
        Name = "Thermal Suit", DiceCode = new DiceCode(1), Price = 250, Climate = Climate.Hot
    };

    public static readonly Armor InsulatedSuit = new()
    {
        Name = "Insulated Suit", DiceCode = new DiceCode(1), Price = 250, Climate = Climate.Cold
    };

    public static readonly Armor AquaticSuit = new()
    {
        Name = "Aquatic Suit", DiceCode = new DiceCode(1), Price = 300, Climate = Climate.Aquatic
    };

    public static List<Armor> All => new()
    {
        Unarmored, PaddedFlightSuit, LightArmor, MediumArmor, HeavyArmor, BattleArmor,
        ThermalSuit, InsulatedSuit, AquaticSuit
    };

    public static List<Armor> Purchasable => new()
    {
        PaddedFlightSuit, LightArmor, MediumArmor, HeavyArmor, BattleArmor,
        ThermalSuit, InsulatedSuit, AquaticSuit
    };

    public static Armor FindByName(string name)
        => All.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? Unarmored;
}

public static class ShieldData
{
    public static readonly VehicleShield Unshielded = new()
    {
        Name = "Unshielded", DiceCode = new DiceCode(0), Price = 0
    };

    public static readonly VehicleShield CivilianShields = new()
    {
        Name = "Civilian Shields", DiceCode = new DiceCode(1), Price = 150
    };

    public static readonly VehicleShield ReconShields = new()
    {
        Name = "Recon Shields", DiceCode = new DiceCode(2), Price = 400
    };

    public static readonly VehicleShield FighterShields = new()
    {
        Name = "Fighter Shields", DiceCode = new DiceCode(3), Price = 800
    };

    public static readonly VehicleShield BomberShields = new()
    {
        Name = "Bomber Shields", DiceCode = new DiceCode(4), Price = 1500
    };

    public static readonly VehicleShield CapitalShields = new()
    {
        Name = "Capital Shields", DiceCode = new DiceCode(5), Price = 3000
    };

    public static List<VehicleShield> All => new()
    {
        Unshielded, CivilianShields, ReconShields, FighterShields, BomberShields, CapitalShields
    };

    public static VehicleShield FindByName(string name)
        => All.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? Unshielded;
}
