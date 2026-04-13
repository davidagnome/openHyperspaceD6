namespace TerminalHyperspace.Models;

/// <summary>
/// Represents a dice code like 3D+2 (3 dice, +2 pips).
/// </summary>
public readonly struct DiceCode
{
    public int Dice { get; }
    public int Pips { get; }

    public DiceCode(int dice, int pips = 0)
    {
        Dice = dice + pips / 3;
        Pips = pips % 3;
    }

    public static DiceCode operator +(DiceCode a, DiceCode b)
        => new(a.Dice + b.Dice, a.Pips + b.Pips);

    public static DiceCode operator +(DiceCode a, int pips)
        => new(a.Dice, a.Pips + pips);

    public static bool operator >(DiceCode a, DiceCode b)
        => a.Dice > b.Dice || (a.Dice == b.Dice && a.Pips > b.Pips);

    public static bool operator <(DiceCode a, DiceCode b)
        => b > a;

    public static bool operator >=(DiceCode a, DiceCode b)
        => a > b || a == b;

    public static bool operator <=(DiceCode a, DiceCode b)
        => b >= a;

    public static bool operator ==(DiceCode a, DiceCode b)
        => a.Dice == b.Dice && a.Pips == b.Pips;

    public static bool operator !=(DiceCode a, DiceCode b)
        => !(a == b);

    public override bool Equals(object? obj) => obj is DiceCode dc && this == dc;
    public override int GetHashCode() => HashCode.Combine(Dice, Pips);

    public override string ToString()
        => Pips > 0 ? $"{Dice}D+{Pips}" : $"{Dice}D";

    public static DiceCode Parse(string s)
    {
        s = s.Trim().ToUpper();
        var parts = s.Split('+');
        int dice = int.Parse(parts[0].TrimEnd('D'));
        int pips = parts.Length > 1 ? int.Parse(parts[1]) : 0;
        return new DiceCode(dice, pips);
    }
}
