namespace TerminalHyperspace.UI;

public class Terminal
{
    // Color scheme:
    // Narrative/descriptions  = Cyan
    // Dialogue (NPC speech)   = Yellow
    // Dice rolls              = Magenta
    // Combat                  = Red
    // Mechanics/rules info    = DarkYellow (orange-ish)
    // Player prompts          = Green
    // Info/system             = White
    // Error                   = DarkRed
    // Headers/titles          = Blue

    public void Narrative(string text)
        => WriteColored(text, ConsoleColor.Cyan);

    public void Dialogue(string speaker, string text)
    {
        WriteColored($"  {speaker}: ", ConsoleColor.DarkYellow, newLine: false);
        WriteColored($"\"{text}\"", ConsoleColor.Yellow);
    }

    public void DiceRoll(string text)
        => WriteColored($"  (roll) {text}", ConsoleColor.Magenta);

    public void Combat(string text)
        => WriteColored(text, ConsoleColor.Red);

    public void Mechanic(string text)
        => WriteColored($"  ⚙ {text}", ConsoleColor.DarkYellow);

    public void Prompt(string text)
        => WriteColored(text, ConsoleColor.Green);

    public void Info(string text)
        => WriteColored(text, ConsoleColor.White);

    public void Error(string text)
        => WriteColored(text, ConsoleColor.DarkRed);

    public void Header(string text)
    {
        WriteRawLine();
        WriteColored(new string('═', Math.Min(text.Length + 4, 60)), ConsoleColor.Blue);
        WriteColored($"  {text}", ConsoleColor.Blue);
        WriteColored(new string('═', Math.Min(text.Length + 4, 60)), ConsoleColor.Blue);
    }

    public void SubHeader(string text)
        => WriteColored($"── {text} ──", ConsoleColor.DarkCyan);

    public void Divider()
        => WriteColored(new string('─', 50), ConsoleColor.DarkGray);

    public void Blank() => WriteRawLine();

    public void LocationHeader(string name)
    {
        WriteRawLine();
        WriteColored($"┌─ {name} ─┐", ConsoleColor.Blue);
    }

    public void LocatorFooter(string name)
    {
        WriteRawLine();
        WriteColored($"-- {name} --", ConsoleColor.Blue);
    }

    public void Exits(IEnumerable<string> exits)
    {
        WriteColored($"  Exits: {string.Join(", ", exits)}", ConsoleColor.DarkGreen);
    }

    public void CharacterSheet(Models.Character c, int upgradePoints = 0, int forcePoints = 0)
    {
        Header($"CHARACTER: {c.Name}");
        Info($"  Species: {c.SpeciesName}  |  Role: {c.RoleName}");
        if (upgradePoints > 0)
            WriteColored($"  Upgrade Points: {upgradePoints}", ConsoleColor.Green);
        if (forcePoints > 0)
            WriteColored($"  Force Points:   {forcePoints}", ConsoleColor.Cyan);
        Divider();

        SubHeader("Attributes & Skills");
        foreach (var attr in Enum.GetValues<Models.AttributeType>())
        {
            var code = c.GetAttribute(attr);
            WriteColored($"  {attr,-12} {code}", ConsoleColor.White);

            var skills = Models.SkillMap.GetSkillsFor(attr);
            foreach (var skill in skills)
            {
                var skillCode = c.GetSkill(skill);
                bool hasBonus = c.SkillBonuses.ContainsKey(skill);
                var color = hasBonus ? ConsoleColor.Cyan : ConsoleColor.DarkGray;
                WriteColored($"    {skill,-14} {skillCode}{(hasBonus ? " ★" : "")}", color);
            }
        }

        SubHeader("Derived Values");
        var dex = c.GetAttribute(Models.AttributeType.Dexterity).Dice;
        var per = c.GetAttribute(Models.AttributeType.Perception).Dice;
        var str = c.GetAttribute(Models.AttributeType.Strength).Dice;
        int AgilityPips = c.SkillBonuses.TryGetValue(Models.SkillType.Agility, out var ab) ? ab.Dice * 3 + ab.Pips : 0;
        int TacticsPips = c.SkillBonuses.TryGetValue(Models.SkillType.Tactics, out var tb) ? tb.Dice * 3 + tb.Pips : 0;
        int StaminaPips = c.SkillBonuses.TryGetValue(Models.SkillType.Stamina, out var sb) ? sb.Dice * 3 + sb.Pips : 0;
        WriteColored($"  Defense:    {c.Defense}  (6 + Dex {dex} + Agility pips {AgilityPips})", ConsoleColor.White);
        WriteColored($"  Initiative: {c.Initiative}  (6 + Per {per} + Tactics pips {TacticsPips})", ConsoleColor.White);
        WriteColored($"  Resolve:    {c.Resolve}  (Current: {c.CurrentResolve})  (6 + Str {str} + Stamina pips {StaminaPips})", ConsoleColor.White);

        SubHeader("Equipment");
        if (c.EquippedWeapon != null)
            WriteColored($"  Weapon: {c.EquippedWeapon}", ConsoleColor.Yellow);
        else
            WriteColored($"  Weapon: (unarmed)", ConsoleColor.DarkGray);
        WriteColored($"  Armor:  {c.EquippedArmor}", ConsoleColor.Yellow);

        if (c.SpaceVehicle != null)
        {
            SubHeader("Space Vehicle");
            WriteColored($"  {c.SpaceVehicle}", ConsoleColor.Yellow);
            if (c.SpaceVehicle.Weapons.Count > 0)
                WriteColored($"  Weapons: {string.Join(", ", c.SpaceVehicle.Weapons)}", ConsoleColor.DarkYellow);
            if (c.SpaceVehicle.Equipment.Count > 0)
                WriteColored($"  Equipment: {string.Join(", ", c.SpaceVehicle.Equipment)}", ConsoleColor.DarkYellow);
        }

        if (c.LandVehicle != null)
        {
            SubHeader("Land Vehicle");
            WriteColored($"  {c.LandVehicle}", ConsoleColor.Yellow);
            if (c.LandVehicle.Weapons.Count > 0)
                WriteColored($"  Weapons: {string.Join(", ", c.LandVehicle.Weapons)}", ConsoleColor.DarkYellow);
            if (c.LandVehicle.Equipment.Count > 0)
                WriteColored($"  Equipment: {string.Join(", ", c.LandVehicle.Equipment)}", ConsoleColor.DarkYellow);
        }

        Divider();
    }

    public string ReadInput()
    {
        var bridge = GuiBridge.Instance;
        if (bridge != null)
        {
            bridge.Write("> ", ConsoleColor.Green, newLine: false);
            return bridge.ReadLine();
        }

        var prev = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("> ");
        Console.ForegroundColor = prev;
        return Console.ReadLine() ?? "";
    }

    public int ReadChoice(int min, int max)
    {
        while (true)
        {
            var input = ReadInput().Trim();
            if (int.TryParse(input, out int choice) && choice >= min && choice <= max)
                return choice;
            Error($"Enter a number between {min} and {max}.");
        }
    }

    private void WriteColored(string text, ConsoleColor color, bool newLine = true)
    {
        var bridge = GuiBridge.Instance;
        if (bridge != null)
        {
            bridge.Write(text, color, newLine);
            return;
        }

        var prev = Console.ForegroundColor;
        Console.ForegroundColor = color;
        if (newLine)
            Console.WriteLine(text);
        else
            Console.Write(text);
        Console.ForegroundColor = prev;
    }

    private void WriteRawLine()
    {
        var bridge = GuiBridge.Instance;
        if (bridge != null)
        {
            bridge.Write("", ConsoleColor.Gray, newLine: true);
            return;
        }
        Console.WriteLine();
    }

    public void Splash()
    {
        var bridge = GuiBridge.Instance;
        if (bridge != null)
        {
            bridge.Clear();
            WriteColored(@"
░▀█▀░█▀▀░█▀▄░█▄█░▀█▀░█▀█░█▀█░█░░░░░█░█░█░█░█▀█░█▀▀░█▀▄░█▀▀░█▀█░█▀█░█▀▀░█▀▀░░░█▀▄░▄▀▀
░░█░░█▀▀░█▀▄░█░█░░█░░█░█░█▀█░█░░░░░█▀█░░█░░█▀▀░█▀▀░█▀▄░▀▀█░█▀▀░█▀█░█░░░█▀▀░░░█░█░█▀▄
░░▀░░▀▀▀░▀░▀░▀░▀░▀▀▀░▀░▀░▀░▀░▀▀▀░░░▀░▀░░▀░░▀░░░▀▀▀░▀░▀░▀▀▀░▀░░░▀░▀░▀▀▀░▀▀▀░░░▀▀░░░▀░
==== Based on the classic Space Fantasy d6 rules and Hyperspace d6 design by Matt Click ", ConsoleColor.Cyan);
            WriteColored(@"
    In a galaxy far and away, the Empire tightens its grip on the
    planets. On Tatooine, a backwater waypoint for smugglers,
    bounty hunters, and those with looking to keep a low profile,
    your destiny begins...", ConsoleColor.DarkCyan);
            WriteRawLine();
            WriteColored("    Press ENTER to begin...", ConsoleColor.DarkGray);
            bridge.ReadLine();
            return;
        }

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
░▀█▀░█▀▀░█▀▄░█▄█░▀█▀░█▀█░█▀█░█░░░░░█░█░█░█░█▀█░█▀▀░█▀▄░█▀▀░█▀█░█▀█░█▀▀░█▀▀░░░█▀▄░▄▀▀
░░█░░█▀▀░█▀▄░█░█░░█░░█░█░█▀█░█░░░░░█▀█░░█░░█▀▀░█▀▀░█▀▄░▀▀█░█▀▀░█▀█░█░░░█▀▀░░░█░█░█▀▄
░░▀░░▀▀▀░▀░▀░▀░▀░▀▀▀░▀░▀░▀░▀░▀▀▀░░░▀░▀░░▀░░▀░░░▀▀▀░▀░▀░▀▀▀░▀░░░▀░▀░▀▀▀░▀▀▀░░░▀▀░░░▀░
==== Based on the classic Space Fantasy d6 rules and Hyperspace d6 design by Matt Click ");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(@"
    In a galaxy far and away, the Empire tightens its grip on the
    planets. On Tatooine, a backwater waypoint for smugglers,
    bounty hunters, and those with looking to keep a low profile,
    your destiny begins...");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine();
        Console.WriteLine("    Press ENTER to begin...");
        Console.ResetColor();
        Console.ReadLine();
    }
}
