using TerminalHyperspace.Models;

namespace TerminalHyperspace.Engine;

public static class DiceRoller
{
    private static readonly Random Rng = new();

    public static DiceResult Roll(DiceCode code)
    {
        var rolls = new List<int>();
        for (int i = 0; i < code.Dice; i++)
            rolls.Add(Rng.Next(1, 7));
        int total = rolls.Sum() + code.Pips;
        return new DiceResult(code, rolls, total);
    }

    public static int RollRaw(int dice, int pips = 0)
        => Roll(new DiceCode(dice, pips)).Total;
}

public record DiceResult(DiceCode Code, List<int> Rolls, int Total)
{
    public override string ToString()
    {
        var rollStr = string.Join("+", Rolls);
        if (Code.Pips > 0)
            return $"[{Code}] rolled ({rollStr})+{Code.Pips} = {Total}";
        return $"[{Code}] rolled ({rollStr}) = {Total}";
    }
}
