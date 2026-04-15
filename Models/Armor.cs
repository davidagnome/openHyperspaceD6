namespace TerminalHyperspace.Models;

public class Armor
{
    public string Name { get; set; } = "Unarmored";
    public DiceCode DiceCode { get; set; } = new(0);
    public int Price { get; set; }
    public Climate Climate { get; set; } = Climate.Normal;

    public override string ToString()
    {
        var climateTag = Climate != Climate.Normal ? $" [{Climate}]" : "";
        return DiceCode.Dice > 0 ? $"{Name} ({DiceCode}){climateTag}" : $"{Name}{climateTag}";
    }
}

public class VehicleShield
{
    public string Name { get; set; } = "Unshielded";
    public DiceCode DiceCode { get; set; } = new(0);
    public int Price { get; set; }

    public override string ToString()
        => DiceCode.Dice > 0 ? $"{Name} ({DiceCode})" : Name;
}
