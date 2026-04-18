using TerminalHyperspace.UI;

namespace TerminalHyperspace.Engine;

/// <summary>
/// Handles the optional Force Point spend that doubles a player's roll total.
/// The prompt is shown BEFORE the dice are rolled, so the choice is committed
/// without knowing the outcome. Applies to skill checks and attack rolls — never
/// to armor absorption, damage rolls, or enemy rolls.
/// </summary>
public static class ForceRoller
{
    /// <summary>
    /// If the player has at least one Force Point, prompts them to spend one to double
    /// the upcoming roll's final total. Deducts the point on confirmation and returns
    /// true so the caller knows to double the resulting total.
    /// </summary>
    public static bool PromptForcePointDouble(GameState state, Terminal term)
    {
        if (state.ForcePoints < 1) return false;
        term.Prompt($"Spend 1 Force Point to DOUBLE this roll? ({state.ForcePoints} available) [y/n]");
        if (term.ReadInput().Trim().ToLower() != "y") return false;
        state.ForcePoints--;
        term.Mechanic($"Force Point spent! (Remaining: {state.ForcePoints})");
        return true;
    }

    /// <summary>
    /// Applies the Force Point doubling to a roll's total if the caller previously
    /// spent a Force Point. Reports the doubling to the terminal.
    /// </summary>
    public static int ApplyForcePointDouble(int total, bool doubling, Terminal term)
    {
        if (!doubling) return total;
        int doubled = total * 2;
        term.Mechanic($"Force Point doubles the result: {total} × 2 = {doubled}");
        return doubled;
    }
}
