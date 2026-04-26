using TerminalHyperspace.UI;

namespace TerminalHyperspace.Engine;

/// Hosts the synchronous game loop. Designed to run on a background thread when
/// the GUI bridge is active; falls back to console behavior when it isn't.
public static class GameRunner
{
    public static void Run()
    {
        var term = new Terminal();
        term.Splash();

        // When the player picks "load a save" from the post-death prompt we
        // hand the chosen state straight to the next iteration so we don't
        // re-ask "new game or load?" up top.
        GameState? preloaded = null;

        while (true)
        {
            GameState state;

            if (preloaded != null)
            {
                state = preloaded;
                preloaded = null;
            }
            else
            {
                var existingSaves = SaveLoadManager.ListSaves();
                if (existingSaves.Count > 0)
                {
                    term.Prompt("Would you like to [n]ew game or [l]oad a save?");
                    var choice = term.ReadInput().Trim().ToLower();

                    if (choice is "l" or "load")
                    {
                        var loaded = PromptAndLoadSave(term);
                        state = loaded ?? CreateNewGame(term);
                    }
                    else
                    {
                        state = CreateNewGame(term);
                    }
                }
                else
                {
                    state = CreateNewGame(term);
                }
            }

            var parser = new CommandParser(state, term);

            term.Blank();
            if (state.TurnCount == 0)
            {
                term.Header("YOUR ADVENTURE BEGINS");
                term.Blank();
                term.Narrative("You step through the battered airlock into the Bucket of Bolts Cantina.");
                term.Narrative("The galaxy is wide open. Where you go from here is up to you.");
            }
            else
            {
                term.Header("ADVENTURE CONTINUES");
                term.Blank();
                term.Narrative($"Welcome back, {state.Player.Name}. You pick up where you left off.");
            }

            term.Blank();
            parser.Look();
            term.Blank();
            term.Info("Type 'help' for a list of commands.");
            term.Divider();

            while (!state.GameOver)
            {
                term.Blank();
                var input = term.ReadInput();
                if (string.IsNullOrWhiteSpace(input)) continue;

                parser.ProcessCommand(input);

                if (parser.LoadedState != null)
                {
                    state = parser.LoadedState;
                    parser = new CommandParser(state, term);
                    state.GameOver = false;

                    term.Blank();
                    term.Header("ADVENTURE CONTINUES");
                    term.Blank();
                    term.Narrative($"Welcome back, {state.Player.Name}. You pick up where you left off.");
                    term.Blank();
                    parser.Look();
                    term.Divider();
                    continue;
                }

                if (state.Player.IsDefeated && !state.GameOver)
                {
                    term.Blank();
                    term.Header("GAME OVER");
                    term.Combat("Your journey ends here, among the stars...");
                    term.Info($"  Turns survived: {state.TurnCount}");
                    term.Info($"  Enemies defeated: {state.EnemiesDefeated}");
                    term.Info($"  Locations visited: {state.VisitedLocations.Count}");
                    term.Info($"  Credits earned: {state.CreditsBalance}");
                    state.GameOver = true;
                }
            }

            if (!state.Player.IsDefeated)
            {
                term.Blank();
                term.Narrative("The stars fade to black. See you around, kid.");
                term.Blank();
                break;
            }

            term.Blank();
            term.Prompt("Would you like to [r]estart with a new character, [l]oad a save, or [q]uit?");
            var postDeath = term.ReadInput().Trim().ToLower();

            if (postDeath is "q" or "quit")
            {
                term.Blank();
                term.Narrative("The stars fade to black. Your story may end but there is another.");
                term.Blank();
                break;
            }

            if (postDeath is "l" or "load")
            {
                var loaded = PromptAndLoadSave(term);
                if (loaded != null)
                {
                    preloaded = loaded;
                }
                else
                {
                    term.Info("Starting a new game instead...");
                }
            }
        }
    }

    /// Lists save files, prompts for a selection, and returns the loaded GameState
    /// or null on cancellation / no saves / load failure.
    private static GameState? PromptAndLoadSave(Terminal term)
    {
        var saves = SaveLoadManager.ListSaves();
        if (saves.Count == 0)
        {
            term.Error("No save files found.");
            return null;
        }

        term.SubHeader("SAVE FILES");
        for (int i = 0; i < saves.Count; i++)
            term.Info($"  [{i + 1}] {saves[i]}");
        term.Prompt("Load which save?");
        int pick = term.ReadChoice(1, saves.Count);

        try
        {
            var state = SaveLoadManager.Load(saves[pick - 1]);
            term.Info($"Loaded: {saves[pick - 1]}.SAV");
            return state;
        }
        catch (Exception ex)
        {
            term.Error($"Load failed: {ex.Message}");
            return null;
        }
    }

    private static GameState CreateNewGame(Terminal term)
    {
        var creator = new CharacterCreation(term);
        var player = creator.Create();
        var state = new GameState { Player = player, UpgradePoints = 6 };
        state.Initialize();
        state.VisitedLocations.Add(state.CurrentLocationId);
        return state;
    }
}
