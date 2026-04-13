using TerminalHyperspace.Engine;
using TerminalHyperspace.UI;

var term = new Terminal();
term.Splash();

// Outer loop: supports restart-on-defeat and load-game
while (true)
{
    GameState state;

    // Check for existing saves to offer load option
    var existingSaves = SaveLoadManager.ListSaves();
    if (existingSaves.Count > 0)
    {
        term.Prompt("Would you like to [n]ew game or [l]oad a save?");
        var choice = term.ReadInput().Trim().ToLower();

        if (choice is "l" or "load")
        {
            term.SubHeader("SAVE FILES");
            for (int i = 0; i < existingSaves.Count; i++)
                term.Info($"  [{i + 1}] {existingSaves[i]}");
            term.Prompt("Load which save?");
            int pick = term.ReadChoice(1, existingSaves.Count);

            try
            {
                state = SaveLoadManager.Load(existingSaves[pick - 1]);
                term.Info($"Loaded: {existingSaves[pick - 1]}.SAV");
            }
            catch (Exception ex)
            {
                term.Error($"Load failed: {ex.Message}");
                term.Info("Starting a new game instead...");
                state = CreateNewGame(term);
            }
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

    var parser = new CommandParser(state, term);

    // Opening / resuming
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

    // Main game loop
    while (!state.GameOver)
    {
        term.Blank();
        var input = term.ReadInput();
        if (string.IsNullOrWhiteSpace(input)) continue;

        parser.ProcessCommand(input);

        // Handle load request: swap state and restart inner loop
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

    // If game ended due to quit (not defeat), exit entirely
    if (!state.Player.IsDefeated)
    {
        term.Blank();
        term.Narrative("The stars fade to black. See you around, kid.");
        term.Blank();
        break;
    }

    // Defeat: offer restart
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
        var saves = SaveLoadManager.ListSaves();
        if (saves.Count == 0)
        {
            term.Error("No save files found. Starting a new game...");
            continue;
        }
        term.SubHeader("SAVE FILES");
        for (int i = 0; i < saves.Count; i++)
            term.Info($"  [{i + 1}] {saves[i]}");
        term.Prompt("Load which save?");
        // We'll load at the top of the outer loop by restarting
        // For simplicity, just continue to the top (which offers new/load again)
    }

    // 'r', 'restart', or anything else → loop back to top for new game / load
}

static GameState CreateNewGame(Terminal term)
{
    var creator = new CharacterCreation(term);
    var player = creator.Create();
    var state = new GameState { Player = player };
    state.Initialize();
    state.VisitedLocations.Add(state.CurrentLocationId);
    return state;
}
