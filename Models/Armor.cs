namespace TerminalHyperspace.Models;

public class Armor
{
    public string Name { get; set; } = "Unarmored";
    public DiceCode DiceCode { get; set; } = new(0);
    public int Price { get; set; }

    public override string ToString()
        => DiceCode.Dice > 0 ? $"{Name} ({DiceCode})" : Name;
}

public class VehicleShield
{
    public string Name { get; set; } = "Unshielded";
    public DiceCode DiceCode { get; set; } = new(0);
    public int Price { get; set; }

    public override string ToString()
        => DiceCode.Dice > 0 ? $"{Name} ({DiceCode})" : Name;
}
