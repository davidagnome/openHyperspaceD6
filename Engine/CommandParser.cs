using TerminalHyperspace.Content;
using TerminalHyperspace.Models;
using TerminalHyperspace.UI;

namespace TerminalHyperspace.Engine;

public class CommandParser
{
    private readonly GameState _state;
    private readonly Terminal _term;
    private readonly Random _rng = new();

    public CommandParser(GameState state, Terminal term)
    {
        _state = state;
        _term = term;
    }

    public void ProcessCommand(string input)
    {
        var parts = input.Trim().ToLower().Split(' ', 2);
        var cmd = parts[0];
        var arg = parts.Length > 1 ? parts[1] : "";

        switch (cmd)
        {
            case "look":
            case "l":
                Look();
                break;
            case "go":
            case "move":
                Move(arg);
                break;
            case "north": case "south": case "east": case "west":
            case "northeast": case "northwest": case "southeast": case "southwest":
            case "ne": case "nw": case "se": case "sw":
            case "up": case "down": case "dock": case "land":
            case "jump": case "explore": case "board":
            case "leave": case "airlock":
                Move(ExpandDir(cmd));
                break;
            case "locator":
                Locator();
                break;
            case "map":
                ShowMap();
                break;
            case "status":
            case "sheet":
            case "char":
                _term.CharacterSheet(_state.Player, _state.UpgradePoints, _state.ForcePoints);
                break;
            case "inventory":
            case "inv":
            case "i":
                ShowInventory();
                break;
            case "equip":
                Equip(arg);
                break;
            case "vehicle":
            case "vehicles":
                ShowVehicles();
                break;
            case "board_vehicle":
            case "enter":
                EnterVehicle(arg);
                break;
            case "exit_vehicle":
            case "disembark":
                ExitVehicle();
                break;
            case "shop":
            case "buy":
                Shop();
                break;
            case "vshop":
                VehicleShop();
                break;
            case "ashop":
                ArmorShop();
                break;
            case "sell":
                SellShop();
                break;
            case "sellv":
                SellVehicle();
                break;
            case "talk":
                Talk();
                break;
            case "t":
                Talk();
                break;
            case "search":
            case "scan":
                SearchArea();
                break;
            case "use":
                UseItem(arg);
                break;
            case "rest":
                Rest();
                break;
            case "upgrade":
            case "levelup":
                Upgrade(arg);
                break;
            case "roll":
                ManualRoll(arg);
                break;
            case "save":
                SaveGame(arg);
                break;
            case "load":
                LoadGame(arg);
                break;
            case "saves":
                ListSaves();
                break;
            case "journal":
            case "missions":
            case "mission":
                ShowJournal();
                break;
            case "help":
            case "?":
                ShowHelp();
                break;
            case "quit":
            case "exit":
                _term.Info("Thanks for playing Terminal Hyperspace!");
                _state.GameOver = true;
                break;
            default:
                _term.Error($"Unknown command: '{cmd}'. Type 'help' for a list of commands.");
                break;
        }
    }

    // ===========================================================
    // MISSIONS
    // ===========================================================
    private void ShowJournal()
    {
        _term.Header("MISSION JOURNAL");
        var active = _state.Missions.Where(m => m.Status == MissionStatus.Active).ToList();
        var completed = _state.Missions.Where(m => m.Status == MissionStatus.Completed).ToList();
        var failed = _state.Missions.Where(m => m.Status == MissionStatus.Failed).ToList();

        if (active.Count == 0 && completed.Count == 0 && failed.Count == 0)
        {
            _term.Info("  (no missions yet — talk to friendly NPCs to find work)");
            return;
        }

        if (active.Count > 0)
        {
            _term.SubHeader("Active");
            foreach (var m in active) PrintMissionLine(m);
        }
        if (completed.Count > 0)
        {
            _term.SubHeader("Completed");
            foreach (var m in completed) PrintMissionLine(m);
        }
        if (failed.Count > 0)
        {
            _term.SubHeader("Failed");
            foreach (var m in failed) PrintMissionLine(m);
        }
    }

    private void PrintMissionLine(Mission m)
    {
        _term.Info($"  [{m.Type}] {m.Title}");
        _term.Info($"      Destination: {m.DestinationName(_state.World)}");
        switch (m.Type)
        {
            case MissionType.Escort when !string.IsNullOrEmpty(m.EscortNpcName):
                _term.Info($"      Escorting:   {m.EscortNpcName}");
                break;
            case MissionType.Delivery when m.MissionItem != null:
                _term.Info($"      Cargo:       {m.MissionItem.Name}");
                break;
            case MissionType.Sabotage:
            case MissionType.Recon:
                _term.Info($"      Skill check: {m.CheckSkill} (TN {m.CheckTargetNumber})");
                break;
        }
        if (m.Status == MissionStatus.Active)
            _term.Info($"      Reward:      {m.CreditReward} cr, +{m.UpgradePointReward} UP");
        if (m.FactionBonus is { } b && b != Faction.Neutral)
            _term.Info($"      +1 standing: {FactionData.Label(b)}");
        if (m.FactionPenalty is { } p && p != Faction.Neutral)
            _term.Info($"      −1 standing: {FactionData.Label(p)}");
    }

    /// Offers a mission to the player. Returns true if accepted.
    private bool OfferMission(Mission m)
    {
        _term.Blank();
        _term.SubHeader("MISSION OFFER");
        _term.Dialogue("Contact", m.BriefingText);
        _term.Mechanic($"Type: {m.Type}    Destination: {m.DestinationName(_state.World)}");
        _term.Mechanic($"Reward: {m.CreditReward} credits, +{m.UpgradePointReward} Upgrade Point{(m.UpgradePointReward == 1 ? "" : "s")}");
        if (m.Type == MissionType.Sabotage || m.Type == MissionType.Recon)
            _term.Mechanic($"On arrival: {m.CheckSkill} skill check (Target {m.CheckTargetNumber})");
        _term.Prompt("Accept the job? [y]es / [n]o (decline)");
        var answer = _term.ReadInput().Trim().ToLower();
        if (answer != "y" && answer != "yes" && answer != "accept")
        {
            _term.Narrative("You decline. Your contact shrugs and finds someone else.");
            return false;
        }

        m.Status = MissionStatus.Active;
        _state.Missions.Add(m);
        _state.OfferedMissionIds.Add(m.Id);

        // Delivery missions place the cargo in the player's inventory.
        if (m.Type == MissionType.Delivery && m.MissionItem != null)
        {
            _state.Player.Inventory.Add(m.MissionItem);
            _term.Info($"  Received: {m.MissionItem.Name}. Take it to {m.MissionItem.MissionDestinationName}.");
        }
        else if (m.Type == MissionType.Escort)
        {
            _term.Info($"  {m.EscortNpcName} now follows you. Lead them to {m.DestinationName(_state.World)}.");
        }
        else
        {
            _term.Info($"  Mission accepted. Travel to {m.DestinationName(_state.World)}.");
        }
        _term.Info("  Type 'journal' anytime to review your active missions.");
        return true;
    }

    /// Picks a mission offer the player hasn't seen and offers it.
    private void MaybeOfferMissionDuringTalk()
    {
        var pool = MissionData.AllOffers
            .Select(f => f())
            .Where(m => !_state.OfferedMissionIds.Contains(m.Id))
            .ToList();
        if (pool.Count == 0) return;
        var pick = pool[_rng.Next(pool.Count)];
        OfferMission(pick);
    }

    /// Called after the player enters a new location. Resolves Delivery, Escort,
    /// Sabotage, and Recon missions whose DestinationLocationId matches.
    private void CheckMissionArrival()
    {
        var matches = _state.Missions
            .Where(m => m.Status == MissionStatus.Active &&
                        m.DestinationLocationId == _state.CurrentLocationId)
            .ToList();
        foreach (var m in matches) ResolveMissionArrival(m);
    }

    private void ResolveMissionArrival(Mission m)
    {
        _term.Blank();
        _term.Header($"MISSION: {m.Title}");

        switch (m.Type)
        {
            case MissionType.Escort:
                _term.Narrative($"You deliver {m.EscortNpcName} safely to {m.DestinationName(_state.World)}.");
                CompleteMission(m);
                break;

            case MissionType.Delivery:
                if (m.MissionItem == null || !_state.Player.Inventory.Contains(m.MissionItem))
                {
                    _term.Error($"You arrived without {m.MissionItem?.Name ?? "the cargo"} — the mission fails.");
                    m.Status = MissionStatus.Failed;
                    return;
                }
                _state.Player.Inventory.Remove(m.MissionItem);
                _term.Narrative($"You hand over the {m.MissionItem.Name}. Your contact nods and pays out.");
                CompleteMission(m);
                break;

            case MissionType.Sabotage:
            case MissionType.Recon:
                ResolveMissionSkillCheck(m);
                break;
        }
    }

    private void ResolveMissionSkillCheck(Mission m)
    {
        var label = m.Type == MissionType.Sabotage ? "sabotage" : "recon";
        _term.Mechanic($"Skill Check: {m.CheckSkill} (Target {m.CheckTargetNumber})");
        _term.Prompt($"Attempt the {label} now? [y]es / [n]o (try later)");
        if (_term.ReadInput().Trim().ToLower() != "y")
        {
            _term.Narrative("You hold off and blend into the surroundings.");
            return;
        }

        var code = _state.Player.GetBestFor(m.CheckSkill);
        bool fpDouble = ForceRoller.PromptForcePointDouble(_state, _term);
        var roll = DiceRoller.Roll(code);
        _term.DiceRoll($"{m.CheckSkill} ({code}): {roll}");
        int finalTotal = ForceRoller.ApplyForcePointDouble(roll.Total, fpDouble, _term);

        if (finalTotal >= m.CheckTargetNumber)
        {
            _term.Narrative(m.CheckSuccessText);
            CompleteMission(m);
        }
        else
        {
            _term.Narrative(m.CheckFailText);
            m.Status = MissionStatus.Failed;
            _term.Error("  Mission failed.");
        }
    }

    private void CompleteMission(Mission m)
    {
        m.Status = MissionStatus.Completed;
        if (m.CreditReward > 0)
        {
            _state.CreditsBalance += m.CreditReward;
            _term.Info($"  +{m.CreditReward} credits (Balance: {_state.CreditsBalance})");
        }
        if (m.UpgradePointReward > 0)
        {
            _state.UpgradePoints += m.UpgradePointReward;
            _term.Info($"  +{m.UpgradePointReward} Upgrade Point{(m.UpgradePointReward == 1 ? "" : "s")} (Total: {_state.UpgradePoints})");
        }

        if (m.FactionBonus is { } bonus && bonus != Faction.Neutral)
        {
            _state.Player.AdjustStanding(bonus, +1);
            _term.Info($"  +1 standing with {FactionData.Label(bonus)} (Total: {_state.Player.GetStanding(bonus)})");
        }
        if (m.FactionPenalty is { } penalty && penalty != Faction.Neutral)
        {
            _state.Player.AdjustStanding(penalty, -1);
            _term.Info($"  −1 standing with {FactionData.Label(penalty)} (Total: {_state.Player.GetStanding(penalty)})");
        }

        _term.SubHeader("★ Mission complete!");
    }

    private static string ExpandDir(string s) => s switch
    {
        "ne" => "northeast",
        "nw" => "northwest",
        "se" => "southeast",
        "sw" => "southwest",
        _ => s
    };

    private void ShowMap()
    {
        var snap = MapBuilder.Build(_state);
        if (snap == null)
        {
            _term.Error("This location isn't on a named planet — no map to draw.");
            return;
        }
        var bridge = UI.GuiBridge.Instance;
        if (bridge != null)
        {
            bridge.RenderMap(snap);
            _term.Info($"Opened planetary map: {snap.Planet}");
        }
        else
        {
            _term.Info($"Planetary map ({snap.Planet}): {snap.Rooms.Count} rooms — open the GUI to view.");
        }
    }

    /// Silent variant: rebuilds and pushes a fresh map snapshot to the GUI
    /// after every Move/Jump so the always-visible map pane tracks the player.
    /// No-op when there's no GUI bridge or the new location lacks a planet.
    private void RefreshMap()
    {
        var bridge = UI.GuiBridge.Instance;
        if (bridge == null) return;
        var snap = MapBuilder.Build(_state);
        if (snap != null) bridge.RenderMap(snap);
    }

    public void Look()
    {
        var loc = _state.CurrentLocation;
        _term.LocationHeader(loc.PlanetName+" - "+loc.Name);
        _term.Narrative(loc.Description);
        _term.Exits(loc.Exits.Keys);

        if (_state.Player.InVehicle)
        {
            var v = _state.Player.ActiveVehicle!;
            _term.Mechanic($"You are aboard: {v.Name} (Resolve: {v.CurrentResolve}/{v.Resolve})");
        }

        if (loc.HasShop) _term.Info("  There is a shop here. Type 'shop' or 'ashop' to browse.");
        if (loc.HasVehicleShop) _term.Info("  Vehicle dealer available. Type 'vshop' to browse.");
        if (loc.FriendlyNPCs?.Count > 0) _term.Info("  There are people here you could 'talk' to.");
        
    }

    public void Locator()
    {
        var loc = _state.CurrentLocation;
        _term.LocatorFooter("Location: "+loc.PlanetName+" // System: "+loc.StarSystemName+" // Sector: "+loc.SectorName);
    }

    public void Move(string direction)
    {
        if (string.IsNullOrEmpty(direction))
        {
            _term.Error("Go where? Specify a direction.");
            _term.Exits(_state.CurrentLocation.Exits.Keys);
            return;
        }

        // Deep space uses a numeric jump menu grouped by Territory > Sector > Planet.
        if (direction == "jump" && _state.CurrentLocationId == "deep_space")
        {
            JumpFromDeepSpace();
            return;
        }

        var loc = _state.CurrentLocation;

        // Check if destination is a space location requiring a space vehicle
        if (loc.Exits.TryGetValue(direction, out var destId))
        {
            if (_state.World.TryGetValue(destId, out var destLoc) && destLoc.IsSpace)
            {
                if (_state.Player.SpaceVehicle == null)
                {
                    _term.Error("You need a space vehicle to travel there!");
                    return;
                }
                // If in a land vehicle, must disembark first — can't fly a speeder into orbit
                if (_state.Player.InVehicle && !_state.Player.InSpaceVehicle)
                {
                    _term.Error($"You can't take the {_state.Player.LandVehicle?.Name} into space! Disembark first.");
                    return;
                }
                if (!_state.Player.InVehicle)
                {
                    _state.Player.InVehicle = true;
                    _state.Player.InSpaceVehicle = true;
                    _state.Player.SpaceVehicle!.InitializeResolve();
                    _term.Narrative($"You board the {_state.Player.SpaceVehicle.Name} and prepare for departure.");
                }
            }
        }

        if (!loc.Exits.TryGetValue(direction, out var finalDestId))
        {
            _term.Error($"You can't go '{direction}' from here.");
            _term.Exits(loc.Exits.Keys);
            return;
        }

        var finalDest = _state.World[finalDestId];

        // Vehicle-required gate (e.g. Beggar's Canyon)
        if (finalDest.RequiresVehicle && !_state.Player.InVehicle)
        {
            _term.Error($"You need a vehicle to reach {finalDest.Name}. Board a land or space vehicle first.");
            return;
        }

        // Aquatic climate gate: need a land vehicle OR armor tagged Aquatic
        if (finalDest.Climate == Climate.Aquatic)
        {
            bool inLandVehicle = _state.Player.InVehicle && !_state.Player.InSpaceVehicle;
            bool hasAquaticArmor = _state.Player.EquippedArmor?.Climate == Climate.Aquatic;
            if (!inLandVehicle && !hasAquaticArmor)
            {
                _term.Error("The waters are impassable without a land vehicle or Aquatic-rated armor.");
                return;
            }
        }

        // Auto-disembark when entering a non-space location from space
        if (_state.Player.InSpaceVehicle && !finalDest.IsSpace)
        {
            _state.Player.InVehicle = false;
            _state.Player.InSpaceVehicle = false;
            _term.Narrative($"You dock the {_state.Player.SpaceVehicle?.Name} and disembark.");
        }

        _state.CurrentLocationId = finalDestId;
        _state.TurnCount++;
        _state.VisitedLocations.Add(finalDestId!);

        Look();
        ShowAmbient();
        ApplyClimateEffects();
        CheckMissionArrival();
        RefreshMap();

        // Space encounters take priority in IsSpace locations when player is in a ship
        if (_state.CurrentLocation.IsSpace && _state.Player.InSpaceVehicle)
            CheckSpaceEncounter();

        CheckEncounter();
    }
    
    /// Presents a numeric jump menu from deep_space to every in-system space location
    /// (IsSpace && IsSystemSpace), grouped by Territory > Sector > Planet.
    private void JumpFromDeepSpace()
    {
        if (_state.Player.SpaceVehicle == null || !_state.Player.InSpaceVehicle)
        {
            _term.Error("You need to be aboard a space vehicle to jump to hyperspace.");
            return;
        }

        // Gather candidate destinations.
        var destinations = _state.World.Values
            .Where(l => l.IsSpace && l.IsSystemSpace && l.Id != "deep_space")
            .ToList();

        if (destinations.Count == 0)
        {
            _term.Error("No known in-system jump points.");
            return;
        }

        // Group hierarchically: Territory > Sector > Planet.
        var grouped = destinations
            .GroupBy(l => string.IsNullOrWhiteSpace(l.TerritoryName) ? "Unknown Territory" : l.TerritoryName)
            .OrderBy(g => g.Key)
            .Select(tg => new
            {
                Territory = tg.Key,
                Sectors = tg
                    .GroupBy(l => string.IsNullOrWhiteSpace(l.SectorName) ? "Unknown Sector" : l.SectorName)
                    .OrderBy(sg => sg.Key)
                    .Select(sg => new
                    {
                        Sector = sg.Key,
                        Planets = sg
                            .GroupBy(l => string.IsNullOrWhiteSpace(l.PlanetName) ? "Unknown Planet" : l.PlanetName)
                            .OrderBy(pg => pg.Key)
                            .ToList()
                    })
                    .ToList()
            })
            .ToList();

        // Flat list, in the same display order, that maps menu number -> Location.
        var menu = new List<Location>();

        _term.Divider();
        _term.Info("═ Hyperspace Jump Destinations ═");
        foreach (var t in grouped)
        {
            _term.Info($"  {t.Territory}");
            foreach (var s in t.Sectors)
            {
                _term.Info($"    {s.Sector}");
                foreach (var p in s.Planets)
                {
                    _term.Info($"      {p.Key}");
                    foreach (var loc in p.OrderBy(l => l.Name))
                    {
                        menu.Add(loc);
                        var coords = loc.HyperspaceCoordinates is { Length: >= 2 }
                            ? $"[{loc.HyperspaceCoordinates[0]}, {loc.HyperspaceCoordinates[1]}]"
                            : "[?, ?]";
                        _term.Info($"        [{menu.Count}] {loc.Name} {coords}");
                    }
                }
            }
        }
        _term.Divider();
        _term.Prompt($"Select destination [1-{menu.Count}] or 'c' to cancel:");

        var input = _term.ReadInput().Trim().ToLower();
        if (input == "c" || input == "cancel" || string.IsNullOrEmpty(input))
        {
            _term.Narrative("You hold position in deep space.");
            return;
        }

        if (!int.TryParse(input, out var choice) || choice < 1 || choice > menu.Count)
        {
            _term.Error("Invalid selection. Jump aborted.");
            return;
        }

        var dest = menu[choice - 1];
        _term.Narrative($"Engaging hyperdrive... destination: {dest.Name}.");

        _state.CurrentLocationId = dest.Id;
        _state.TurnCount++;
        _state.VisitedLocations.Add(dest.Id);

        Look();
        ShowAmbient();
        ApplyClimateEffects();
        CheckMissionArrival();
        RefreshMap();

        if (_state.CurrentLocation.IsSpace && _state.Player.InSpaceVehicle)
            CheckSpaceEncounter();

        CheckEncounter();
    }

    /// <summary>
    /// Warns the player about hostile climates and applies per-turn Hot/Cold damage
    /// when the player lacks a vehicle or matching environmental armor.
    /// </summary>
    private void ApplyClimateEffects()
    {
        var loc = _state.CurrentLocation;
        if (loc.Climate == Climate.Normal) return;

        bool inVehicle = _state.Player.InVehicle;
        bool armorMatches = _state.Player.EquippedArmor?.Climate == loc.Climate;
        bool protected_ = inVehicle || armorMatches;

        switch (loc.Climate)
        {
            case Climate.Hot:
                _term.Mechanic("[CLIMATE: HOT] Blistering heat saps your strength. You need a vehicle or Thermal-rated armor to traverse safely.");
                if (!protected_)
                {
                    var damage = DiceRoller.Roll(new DiceCode(1));
                    _term.Combat($"The heat drains {damage.Total} Resolve (1D: [{string.Join(", ", damage.Rolls)}]).");
                    _state.Player.CurrentResolve -= damage.Total;
                    if (_state.Player.CurrentResolve <= 0)
                        _term.Combat("You collapse from heatstroke...");
                }
                break;
            case Climate.Cold:
                _term.Mechanic("[CLIMATE: COLD] Freezing winds cut to the bone. You need a vehicle or Insulated-rated armor to traverse safely.");
                if (!protected_)
                {
                    var damage = DiceRoller.Roll(new DiceCode(1));
                    _term.Combat($"The cold drains {damage.Total} Resolve (1D: [{string.Join(", ", damage.Rolls)}]).");
                    _state.Player.CurrentResolve -= damage.Total;
                    if (_state.Player.CurrentResolve <= 0)
                        _term.Combat("You succumb to hypothermia...");
                }
                break;
            case Climate.Aquatic:
                _term.Mechanic("[CLIMATE: AQUATIC] Deep waters surround you. Only a land vehicle or Aquatic-rated armor will carry you further.");
                break;
        }
    }

    private void ShowAmbient()
    {
        var loc = _state.CurrentLocation;
        if (loc.AmbientMessages.Count > 0)
        {
            _term.Blank();
            _term.Narrative(loc.AmbientMessages[_rng.Next(loc.AmbientMessages.Count)]);
        }
    }

    public void CheckEncounter()
    {
        var loc = _state.CurrentLocation;
        if (loc.PossibleEncounters.Count == 0) return;
        if (_state.ClearedRooms.Contains(loc.Id)) return;
        if (_rng.NextDouble() > loc.EncounterChance) return;

        var enemyFactory = loc.PossibleEncounters[_rng.Next(loc.PossibleEncounters.Count)];
        var enemy = enemyFactory();
        enemy.InitializeResolve();

        _term.Blank();
        _term.Combat($"A {enemy.Name} appears!");

        if (enemy.EquippedWeapon != null)
            _term.Mechanic($"Armed with: {enemy.EquippedWeapon.Name}");
        _term.Mechanic($"Defense: {enemy.Defense} | Resolve: {enemy.Resolve}");

        _term.Prompt("Engage in combat? [y]es / [n]o (attempt to avoid)");
        var choice = _term.ReadInput().Trim().ToLower();

        if (choice != "n")
        {
            var combat = new CombatEngine(_state.Player, enemy, _term, _state);
            bool survived = combat.RunCombat();
            if (!survived)
            {
                _state.GameOver = true;
                return;
            }
            if (enemy.IsDefeated)
            {
                _state.EnemiesDefeated++;
                _state.ClearedRooms.Add(loc.Id);
            }
        }
        else
        {
            // Stealth/avoidance check
            var hideRoll = DiceRoller.Roll(_state.Player.GetBestFor(SkillType.Hide));
            var searchRoll = DiceRoller.Roll(enemy.GetBestFor(SkillType.Search));
            _term.DiceRoll($"Hide: {hideRoll} vs {enemy.Name}'s Search: {searchRoll}");

            if (hideRoll.Total >= searchRoll.Total)
            {
                _term.Narrative("You slip into the shadows unnoticed.");
            }
            else
            {
                _term.Narrative("You're spotted! No choice but to fight!");
                var combat = new CombatEngine(_state.Player, enemy, _term, _state);
                bool survived = combat.RunCombat();
                if (!survived)
                {
                    _state.GameOver = true;
                    return;
                }
                if (enemy.IsDefeated)
                {
                    _state.EnemiesDefeated++;
                    _state.ClearedRooms.Add(loc.Id);
                }
            }
        }
    }

    public void CheckSpaceEncounter()
    {
        var loc = _state.CurrentLocation;
        if (!loc.IsSpace || loc.SpaceEncounters.Count == 0) return;
        if (_state.ClearedRooms.Contains("space_" + loc.Id)) return;
        if (_rng.NextDouble() > loc.SpaceEncounterChance) return;
        if (_state.Player.SpaceVehicle == null || !_state.Player.InSpaceVehicle) return;

        var encounterFactory = loc.SpaceEncounters[_rng.Next(loc.SpaceEncounters.Count)];
        var encounter = encounterFactory();
        encounter.Ship.InitializeResolve();

        _term.Blank();
        _term.Combat($"Sensors detect incoming: {encounter.Ship.Name} piloted by {encounter.Pilot.Name}!");
        _term.Mechanic($"Ship: {encounter.Ship}");
        if (encounter.Ship.Weapons.Count > 0)
            _term.Mechanic($"Weapons: {string.Join(", ", encounter.Ship.Weapons)}");
        if (encounter.Ship.Equipment.Count > 0)
            _term.Mechanic($"Equipment: {string.Join(", ", encounter.Ship.Equipment)}");

        _term.Prompt("Engage in ship combat? [y]es / [n]o (attempt to evade)");
        var choice = _term.ReadInput().Trim().ToLower();

        if (choice != "n")
        {
            var combat = new SpaceCombatEngine(
                _state.Player, _state.Player.SpaceVehicle!,
                encounter.Pilot, encounter.Ship, _term, _state);
            bool survived = combat.RunCombat();
            _state.CreditsBalance += combat.SalvageCredits;
            if (encounter.Ship.IsDestroyed)
            {
                _state.EnemiesDefeated++;
                _state.ClearedRooms.Add("space_" + loc.Id);
            }
            // If player lost their ship, they need to get back to a station
            if (_state.Player.SpaceVehicle == null)
            {
                _term.Narrative("Your escape pod drifts toward the nearest station...");
                _state.CurrentLocationId = "docking_bay";
                _state.Player.InVehicle = false;
                _state.Player.InSpaceVehicle = false;
                Look();
            }
        }
        else
        {
            // Evasion: Pilot + Maneuverability vs enemy Sensors
            var playerShip = _state.Player.SpaceVehicle!;
            var evadeCode = _state.Player.GetBestFor(SkillType.Pilot) + playerShip.Maneuverability;
            var detectCode = encounter.Pilot.GetBestFor(SkillType.Sensors) + encounter.Ship.GetSkillBonus(SkillType.Sensors);
            bool fpDouble = ForceRoller.PromptForcePointDouble(_state, _term);
            var evadeRoll = DiceRoller.Roll(evadeCode);
            var detectRoll = DiceRoller.Roll(detectCode);
            _term.DiceRoll($"Evasion (Pilot + Maneuver): {evadeRoll} vs Sensors: {detectRoll}");
            int evadeTotal = ForceRoller.ApplyForcePointDouble(evadeRoll.Total, fpDouble, _term);

            if (evadeTotal >= detectRoll.Total)
            {
                _term.Narrative("You slip into the sensor shadow and evade detection!");
            }
            else
            {
                _term.Narrative("They've locked on — no escape! Engaging!");
                var combat = new SpaceCombatEngine(
                    _state.Player, playerShip,
                    encounter.Pilot, encounter.Ship, _term, _state);
                bool survived = combat.RunCombat();
                _state.CreditsBalance += combat.SalvageCredits;
                if (encounter.Ship.IsDestroyed)
                {
                    _state.EnemiesDefeated++;
                    _state.ClearedRooms.Add("space_" + loc.Id);
                }
                if (_state.Player.SpaceVehicle == null)
                {
                    _term.Narrative("Your escape pod drifts toward the nearest station...");
                    _state.CurrentLocationId = "docking_bay";
                    _state.Player.InVehicle = false;
                    _state.Player.InSpaceVehicle = false;
                    Look();
                }
            }
        }
    }

    private void ShowInventory()
    {
        _term.SubHeader("Inventory");
        _term.Info($"  Credits: {_state.CreditsBalance}");
        _term.Info($"  Force Points: {_state.ForcePoints}");
        if (_state.Player.Inventory.Count == 0)
        {
            _term.Info("  (empty)");
            return;
        }

        for (int i = 0; i < _state.Player.Inventory.Count; i++)
        {
            var item = _state.Player.Inventory[i];
            var equipped = item == _state.Player.EquippedWeapon ? " [EQUIPPED]" : "";
            _term.Info($"  [{i + 1}] {item}{equipped}");
            if (item.IsMissionItem && !string.IsNullOrEmpty(item.MissionDestinationName))
                _term.Mechanic($"      ⮕ Mission cargo — deliver to: {item.MissionDestinationName}");
        }
    }

    private void Equip(string arg)
    {
        var weapons = _state.Player.Inventory.Where(i => i.IsWeapon).ToList();
        if (weapons.Count == 0)
        {
            _term.Error("You have no weapons to equip.");
            return;
        }

        if (!string.IsNullOrEmpty(arg) && int.TryParse(arg, out int idx) && idx >= 1 && idx <= _state.Player.Inventory.Count)
        {
            var item = _state.Player.Inventory[idx - 1];
            if (!item.IsWeapon) { _term.Error("That's not a weapon."); return; }
            _state.Player.EquippedWeapon = item;
            _term.Info($"Equipped: {item.Name}");
            return;
        }

        _term.Prompt("Equip which weapon?");
        for (int i = 0; i < weapons.Count; i++)
            _term.Info($"  [{i + 1}] {weapons[i]}");
        int choice = _term.ReadChoice(1, weapons.Count);
        _state.Player.EquippedWeapon = weapons[choice - 1];
        _term.Info($"Equipped: {weapons[choice - 1].Name}");
    }

    private void ShowVehicles()
    {
        _term.SubHeader("Vehicles");
        if (_state.Player.SpaceVehicle != null)
            _term.Info($"  Space: {_state.Player.SpaceVehicle}");
        else
            _term.Info("  Space: (none)");

        if (_state.Player.LandVehicle != null)
            _term.Info($"  Land: {_state.Player.LandVehicle}");
        else
            _term.Info("  Land: (none)");

        if (_state.Player.InVehicle)
            _term.Mechanic($"Currently aboard: {_state.Player.ActiveVehicle?.Name}");
    }

    private void EnterVehicle(string arg)
    {
        arg = arg.ToLower();
        if (arg.Contains("space") || arg.Contains("ship"))
        {
            if (_state.Player.SpaceVehicle == null) { _term.Error("You don't own a space vehicle."); return; }
            _state.Player.InVehicle = true;
            _state.Player.InSpaceVehicle = true;
            _state.Player.SpaceVehicle.InitializeResolve();
            _term.Narrative($"You climb into the cockpit of the {_state.Player.SpaceVehicle.Name}.");
        }
        else
        {
            if (_state.Player.LandVehicle == null) { _term.Error("You don't own a land vehicle."); return; }
            if (_state.CurrentLocation.IsSpace) { _term.Error("Can't use a land vehicle in space!"); return; }
            _state.Player.InVehicle = true;
            _state.Player.InSpaceVehicle = false;
            _state.Player.LandVehicle.InitializeResolve();
            _term.Narrative($"You mount up in the {_state.Player.LandVehicle.Name}.");
        }
    }

    private void ExitVehicle()
    {
        if (!_state.Player.InVehicle) { _term.Error("You're not in a vehicle."); return; }
        if (_state.CurrentLocation.IsSpace) { _term.Error("You can't disembark in open space!"); return; }
        var name = _state.Player.ActiveVehicle?.Name;
        _state.Player.InVehicle = false;
        _state.Player.InSpaceVehicle = false;
        _term.Narrative($"You exit the {name}.");
    }

    private void Shop()
    {
        if (!_state.CurrentLocation.HasShop) { _term.Error("There's no shop here."); return; }

        _term.SubHeader("SHOP — Equipment");
        _term.Info($"  Your credits: {_state.CreditsBalance}");

        var items = new Item[]
        {
            ItemData.BlasterPistol, ItemData.HeavyBlaster, ItemData.BlasterRifle,
            ItemData.Vibroblade, ItemData.ForcePike,
            ItemData.Medpack, ItemData.DataPad, ItemData.Macrobinoculars,
        };

        for (int i = 0; i < items.Length; i++)
            _term.Info($"  [{i + 1}] {items[i].Name,-25} {items[i].Price} credits — {items[i].Description}");
        _term.Info("  [0] Leave shop");

        _term.Prompt("Buy which item?");
        int choice = _term.ReadChoice(0, items.Length);
        if (choice == 0) { _term.Info("You leave the shop."); return; }

        var selectedItem = items[choice - 1];
        if (_state.CreditsBalance < selectedItem.Price)
        {
            _term.Error("Not enough credits!");
            return;
        }

        _state.CreditsBalance -= selectedItem.Price;
        _state.Player.Inventory.Add(selectedItem);
        _term.Info($"Purchased {selectedItem.Name} for {selectedItem.Price} credits. Remaining: {_state.CreditsBalance}");
    }

    private void VehicleShop()
    {
        if (!_state.CurrentLocation.HasVehicleShop) { _term.Error("No vehicle dealer here."); return; }

        _term.SubHeader("VEHICLE DEALER");
        _term.Info($"  Your credits: {_state.CreditsBalance}");

        var vehicles = new Vehicle[]
        {
            VehicleData.Speeder, VehicleData.CombatSpeeder, VehicleData.ArmoredTransport,
            VehicleData.Starfighter, VehicleData.LightFreighter,
        };

        for (int i = 0; i < vehicles.Length; i++)
        {
            var v = vehicles[i];
            var tag = v.IsSpace ? "[SPACE]" : "[LAND]";
            _term.Info($"  [{i + 1}] {tag} {v.Name,-30} {v.Price} credits");
            _term.Info($"       {v.Description}");
            _term.Mechanic($"       Maneuver: {v.Maneuverability} | Resolve: {v.Resolve} | Shields: {v.Shield}");
            if (v.Weapons.Count > 0)
                _term.Mechanic($"       Weapons: {string.Join(", ", v.Weapons)}");
            if (v.Equipment.Count > 0)
                _term.Mechanic($"       Equipment: {string.Join(", ", v.Equipment)}");
        }
        _term.Info("  [0] Leave");

        _term.Prompt("Purchase which vehicle?");
        int choice = _term.ReadChoice(0, vehicles.Length);
        if (choice == 0) return;

        var selected = vehicles[choice - 1];
        if (_state.CreditsBalance < selected.Price) { _term.Error("Not enough credits!"); return; }

        if (selected.IsSpace)
        {
            if (_state.Player.SpaceVehicle != null)
            {
                _term.Prompt($"Replace your {_state.Player.SpaceVehicle.Name}? [y/n]");
                if (_term.ReadInput().Trim().ToLower() != "y") return;
            }
            // Clone the vehicle so each purchase is independent
            _state.Player.SpaceVehicle = CloneVehicle(selected);
            _state.Player.SpaceVehicle.InitializeResolve();
        }
        else
        {
            if (_state.Player.LandVehicle != null)
            {
                _term.Prompt($"Replace your {_state.Player.LandVehicle.Name}? [y/n]");
                if (_term.ReadInput().Trim().ToLower() != "y") return;
            }
            _state.Player.LandVehicle = CloneVehicle(selected);
            _state.Player.LandVehicle.InitializeResolve();
        }

        _state.CreditsBalance -= selected.Price;
        _term.Info($"Purchased {selected.Name} for {selected.Price} credits!");
    }

    private void ArmorShop()
    {
        if (!_state.CurrentLocation.HasShop) { _term.Error("There's no shop here."); return; }

        _term.SubHeader("ARMOR SHOP");
        _term.Info($"  Your credits: {_state.CreditsBalance}");
        _term.Info($"  Current armor: {_state.Player.EquippedArmor}");

        var armors = ArmorData.Purchasable;
        for (int i = 0; i < armors.Count; i++)
            _term.Info($"  [{i + 1}] {armors[i].Name,-25} {armors[i].DiceCode}  — {armors[i].Price} credits");
        _term.Info("  [0] Leave");

        _term.Prompt("Purchase which armor?");
        int choice = _term.ReadChoice(0, armors.Count);
        if (choice == 0) return;

        var selected = armors[choice - 1];
        if (_state.CreditsBalance < selected.Price) { _term.Error("Not enough credits!"); return; }

        _state.CreditsBalance -= selected.Price;
        _state.Player.EquippedArmor = selected;
        _term.Info($"Equipped {selected.Name} for {selected.Price} credits!");
    }

    private static Vehicle CloneVehicle(Vehicle v) => new()
    {
        Name = v.Name, Description = v.Description, IsSpace = v.IsSpace,
        Maneuverability = v.Maneuverability, Resolve = v.Resolve,
        Shield = v.Shield,
        Weapons = v.Weapons.Select(w => new VehicleWeapon
        {
            Name = w.Name, Damage = w.Damage, AttackSkill = w.AttackSkill
        }).ToList(),
        Equipment = v.Equipment.Select(e => new VehicleEquipment
        {
            Name = e.Name, BonusSkill = e.BonusSkill, Bonus = e.Bonus
        }).ToList()
    };

    /// <summary>
    /// Rolls Persuade and returns the sell price as a percentage of base value.
    /// 5-9 = 50%, 10-14 = 70%, 15-19 = 95%, 20+ = 110%. Below 5 = no sale.
    /// </summary>
    private int? RollSellPrice(int basePrice)
    {
        var persuadeCode = _state.Player.GetBestFor(SkillType.Persuade);
        var roll = DiceRoller.Roll(persuadeCode);
        _term.DiceRoll($"Persuade {persuadeCode}: rolled [{string.Join(", ", roll.Rolls)}] = {roll.Total}");

        double percentage;
        if (roll.Total >= 20)
        {
            percentage = 1.10;
            _term.Narrative("The dealer is impressed — you drive a hard bargain and get top credit!");
        }
        else if (roll.Total >= 15)
        {
            percentage = 0.95;
            _term.Narrative("You negotiate skillfully. The dealer agrees to a very fair price.");
        }
        else if (roll.Total >= 10)
        {
            percentage = 0.70;
            _term.Narrative("The dealer makes a reasonable offer. Not bad.");
        }
        else if (roll.Total >= 5)
        {
            percentage = 0.50;
            _term.Narrative("The dealer lowballs you, but it's credits in your pocket.");
        }
        else
        {
            _term.Error("The dealer waves you off. 'This junk isn't worth my time.'");
            return null;
        }

        return (int)Math.Round(basePrice * percentage);
    }

    private void SellShop()
    {
        if (!_state.CurrentLocation.HasShop) { _term.Error("There's no shop here."); return; }

        var inventory = _state.Player.Inventory;
        if (inventory.Count == 0)
        {
            _term.Error("You have nothing to sell.");
            return;
        }

        _term.SubHeader("SELL — Equipment");
        _term.Info($"  Your credits: {_state.CreditsBalance}");
        _term.Blank();

        // List sellable items (items with a known price > 0)
        var sellable = new List<(int inventoryIndex, Item item)>();
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].Price > 0)
                sellable.Add((i, inventory[i]));
        }

        if (sellable.Count == 0)
        {
            _term.Error("None of your items have resale value.");
            return;
        }

        for (int i = 0; i < sellable.Count; i++)
        {
            var item = sellable[i].item;
            var equipped = item == _state.Player.EquippedWeapon ? " [EQUIPPED]" : "";
            _term.Info($"  [{i + 1}] {item.Name,-25} (base value: {item.Price} credits){equipped}");
        }
        _term.Info("  [0] Cancel");

        _term.Prompt("Sell which item?");
        int choice = _term.ReadChoice(0, sellable.Count);
        if (choice == 0) { _term.Info("You leave without selling."); return; }

        var (invIdx, selectedItem) = sellable[choice - 1];

        // Warn if selling equipped weapon
        if (selectedItem == _state.Player.EquippedWeapon)
        {
            _term.Prompt($"Sell your equipped weapon {selectedItem.Name}? [y/n]");
            if (_term.ReadInput().Trim().ToLower() != "y") return;
            _state.Player.EquippedWeapon = null;
        }

        _term.Narrative("You present the goods to the dealer and haggle over the price...");
        var sellPrice = RollSellPrice(selectedItem.Price);
        if (sellPrice == null) return; // failed the roll

        _state.Player.Inventory.RemoveAt(invIdx);
        _state.CreditsBalance += sellPrice.Value;
        _term.Info($"  Sold {selectedItem.Name} for {sellPrice.Value} credits. Balance: {_state.CreditsBalance}");
    }

    private void SellVehicle()
    {
        if (!_state.CurrentLocation.HasVehicleShop) { _term.Error("No vehicle dealer here."); return; }

        var options = new List<(string label, Vehicle vehicle, Action remove)>();

        if (_state.Player.LandVehicle != null)
        {
            var v = _state.Player.LandVehicle;
            options.Add(($"[LAND]  {v.Name} (base value: {v.Price} credits)", v, () =>
            {
                if (_state.Player.InVehicle && !_state.Player.InSpaceVehicle)
                {
                    _state.Player.InVehicle = false;
                }
                _state.Player.LandVehicle = null;
            }));
        }

        if (_state.Player.SpaceVehicle != null)
        {
            var v = _state.Player.SpaceVehicle;
            options.Add(($"[SPACE] {v.Name} (base value: {v.Price} credits)", v, () =>
            {
                if (_state.Player.InVehicle && _state.Player.InSpaceVehicle)
                {
                    _state.Player.InVehicle = false;
                    _state.Player.InSpaceVehicle = false;
                }
                _state.Player.SpaceVehicle = null;
            }));
        }

        if (options.Count == 0)
        {
            _term.Error("You have no vehicles to sell.");
            return;
        }

        _term.SubHeader("SELL — Vehicles");
        _term.Info($"  Your credits: {_state.CreditsBalance}");
        _term.Blank();

        for (int i = 0; i < options.Count; i++)
            _term.Info($"  [{i + 1}] {options[i].label}");
        _term.Info("  [0] Cancel");

        _term.Prompt("Sell which vehicle?");
        int choice = _term.ReadChoice(0, options.Count);
        if (choice == 0) { _term.Info("You leave without selling."); return; }

        var selected = options[choice - 1];

        if (selected.vehicle.Price <= 0)
        {
            _term.Error("This vehicle has no resale value.");
            return;
        }

        _term.Prompt($"Sell your {selected.vehicle.Name}? [y/n]");
        if (_term.ReadInput().Trim().ToLower() != "y") return;

        _term.Narrative("You walk the dealer around the vehicle, pointing out its features...");
        var sellPrice = RollSellPrice(selected.vehicle.Price);
        if (sellPrice == null) return;

        selected.remove();
        _state.CreditsBalance += sellPrice.Value;
        _term.Info($"  Sold {selected.vehicle.Name} for {sellPrice.Value} credits. Balance: {_state.CreditsBalance}");
    }

    private void Talk()
    {
        var loc = _state.CurrentLocation;
        if (loc.FriendlyNPCs == null || loc.FriendlyNPCs.Count == 0)
        {
            _term.Narrative("There's nobody here who seems interested in talking.");
            return;
        }

        var npc = loc.FriendlyNPCs[_rng.Next(loc.FriendlyNPCs.Count)]();
        _term.Narrative($"You approach a {npc.Name}.");

        var dialogue = PickDialogue(loc, npc.Name);
        foreach (var dl in dialogue.Lines)
            _term.Dialogue(dl.Speaker, dl.Line);

        // Roll the conversation outcome: skill-check, mission offer, or small reward.
        var outcome = _rng.NextDouble();

        // Mission offer (~35% chance) when there's still a fresh mission to give.
        bool hasFreshMission = MissionData.AllOffers.Any(f => !_state.OfferedMissionIds.Contains(f().Id));
        if (hasFreshMission && outcome < 0.35)
        {
            MaybeOfferMissionDuringTalk();
            return;
        }

        // Talk-based skill check (~40% of remaining)
        var availableChecks = SkillCheckData.TalkChecks
            .Where(c => !_state.CompletedChecks.Contains(c.Id))
            .ToList();
        if (availableChecks.Count > 0 && outcome < 0.75)
        {
            var check = availableChecks[_rng.Next(availableChecks.Count)];
            _term.Blank();
            RunSkillCheck(check);
            return;
        }

        // Small chance of a credit tip
        if (_rng.NextDouble() < 0.3)
        {
            int reward = _rng.Next(20, 80);
            _state.CreditsBalance += reward;
            _term.Info($"  You received {reward} credits for your time.");
        }
    }

    /// Pulls a random dialogue from the location's per-location pool, falling
    /// back to DialogueData.Default if the location author hasn't curated one.
    private Dialogue PickDialogue(Location loc, string npcName)
    {
        var pool = loc.DialoguePool != null && loc.DialoguePool.Count > 0
            ? loc.DialoguePool
            : DialogueData.Default;
        var factory = pool[_rng.Next(pool.Count)];
        return factory(npcName, _state.Player.Name);
    }

    private void SearchArea()
    {
        var locId = _state.CurrentLocationId;

        // Check for location-specific skill check events first
        if (SkillCheckData.LocationChecks.TryGetValue(locId, out var checks))
        {
            var available = checks.Where(c => !_state.CompletedChecks.Contains(c.Id) || c.Repeatable).ToList();
            if (available.Count > 0)
            {
                var check = available[_rng.Next(available.Count)];
                RunSkillCheck(check);
                _state.TurnCount++;
                if (_rng.NextDouble() < 0.15)
                    CheckEncounter();
                return;
            }
        }

        // Standard search
        var searchRoll = DiceRoller.Roll(_state.Player.GetBestFor(SkillType.Search));
        _term.DiceRoll($"Search check: {searchRoll}");

        if (searchRoll.Total >= 12)
        {
            int credits = _rng.Next(10, 50);
            _state.CreditsBalance += credits;
            _term.Narrative($"You find {credits} credits stashed in a hidden compartment!");
        }
        else if (searchRoll.Total >= 8)
        {
            _term.Narrative("You find some interesting markings on the wall, but nothing of value.");
        }
        else
        {
            _term.Narrative("Your search turns up nothing.");
        }

        // Searching takes time—could trigger encounter
        _state.TurnCount++;
        if (_rng.NextDouble() < 0.15)
            CheckEncounter();
    }

    private void UseItem(string arg)
    {
        var medpacks = _state.Player.Inventory.Where(i => i.Name == "Medpack").ToList();
        if (arg.ToLower().Contains("med") && medpacks.Count > 0)
        {
            var roll = DiceRoller.Roll(_state.Player.GetBestFor(SkillType.Medicine));
            _term.DiceRoll($"Medicine check: {roll}");
            int heal = Math.Max(1, roll.Total / 2);
            _state.Player.CurrentResolve = Math.Min(_state.Player.Resolve, _state.Player.CurrentResolve + heal);
            _state.Player.Inventory.Remove(medpacks[0]);
            _term.Info($"You use a Medpack and restore {heal} Resolve. (Now: {_state.Player.CurrentResolve}/{_state.Player.Resolve})");
            return;
        }

        _term.Error("Use what? Try 'use medpack'.");
    }

    private void Rest()
    {
        if (_state.Player.CurrentResolve >= _state.Player.Resolve)
        {
            _term.Narrative("You're already at full Resolve.");
            return;
        }

        _term.Narrative("You find a quiet spot and rest for a while...");
        int heal = _rng.Next(1, 4);
        _state.Player.CurrentResolve = Math.Min(_state.Player.Resolve, _state.Player.CurrentResolve + heal);
        _term.Info($"You recover {heal} Resolve. (Now: {_state.Player.CurrentResolve}/{_state.Player.Resolve})");
        _state.TurnCount += 2;

        // Resting costs time—chance of encounter
        if (_rng.NextDouble() < 0.2)
        {
            _term.Narrative("Your rest is interrupted!");
            CheckEncounter();
        }
    }

    private void ManualRoll(string arg)
    {
        if (Enum.TryParse<SkillType>(arg, true, out var skill))
        {
            var code = _state.Player.GetBestFor(skill);
            var result = DiceRoller.Roll(code);
            _term.DiceRoll($"{skill} check: {result}");
            return;
        }

        if (Enum.TryParse<AttributeType>(arg, true, out var attr))
        {
            var code = _state.Player.GetAttribute(attr);
            var result = DiceRoller.Roll(code);
            _term.DiceRoll($"{attr} check: {result}");
            return;
        }

        // Try parsing a dice code like "3d6+1"
        try
        {
            var code = DiceCode.Parse(arg);
            var result = DiceRoller.Roll(code);
            _term.DiceRoll($"Roll: {result}");
        }
        catch
        {
            _term.Error("Usage: roll <skill|attribute|dice code>  (e.g., 'roll blasters', 'roll dexterity', 'roll 3d+1')");
        }
    }

    public GameState? LoadedState { get; private set; }

    private void SaveGame(string arg)
    {
        var fileName = string.IsNullOrWhiteSpace(arg) ? null : arg.Trim();
        try
        {
            SaveLoadManager.Save(_state, fileName);
            var displayName = fileName ?? SaveLoadManager.SanitizeFileName(_state.Player.Name);
            _term.Info($"Game saved to {displayName}.SAV");
        }
        catch (Exception ex)
        {
            _term.Error($"Save failed: {ex.Message}");
        }
    }

    private void LoadGame(string arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            var saves = SaveLoadManager.ListSaves();
            if (saves.Count == 0) { _term.Error("No save files found."); return; }

            _term.SubHeader("SAVE FILES");
            for (int i = 0; i < saves.Count; i++)
                _term.Info($"  [{i + 1}] {saves[i]}");
            _term.Prompt("Load which save?");
            int choice = _term.ReadChoice(1, saves.Count);
            arg = saves[choice - 1];
        }

        try
        {
            LoadedState = SaveLoadManager.Load(arg.Trim());
            _term.Info($"Loaded save: {arg.Trim()}");
            _state.GameOver = true; // signal the game loop to swap state
        }
        catch (Exception ex)
        {
            _term.Error($"Load failed: {ex.Message}");
        }
    }

    private void ListSaves()
    {
        var saves = SaveLoadManager.ListSaves();
        if (saves.Count == 0) { _term.Info("No save files found."); return; }

        _term.SubHeader("SAVE FILES");
        foreach (var s in saves)
            _term.Info($"  {s}.SAV");
    }

    private void RunSkillCheck(SkillCheckEvent check)
    {
        _term.Narrative(check.Description);
        _term.Mechanic($"Skill Check: {check.Skill} — {SkillCheckData.DifficultyLabel(check.Difficulty)} (Target: {check.TargetNumber})");
        _term.Prompt($"Attempt the {check.Skill} check? [y/n]");
        if (_term.ReadInput().Trim().ToLower() != "y")
        {
            _term.Narrative("You decide to leave it alone.");
            return;
        }

        var skillCode = _state.Player.GetBestFor(check.Skill);
        bool fpDouble = ForceRoller.PromptForcePointDouble(_state, _term);
        var roll = DiceRoller.Roll(skillCode);
        _term.DiceRoll($"{check.Skill} check ({skillCode}): {roll}");

        int finalTotal = roll.Total;

        // Wild die bonus: if the first die came up 6, offer extra die or Force Point
        if (roll.Rolls.Count > 0 && roll.Rolls[0] == 6)
        {
            _term.Blank();
            _term.Mechanic("The first die shows a 6! Choose a bonus:");
            _term.Info("  [1] Roll an extra 1D and add it to your total");
            _term.Info("  [2] Gain a Force Point instead");
            int bonus = _term.ReadChoice(1, 2);
            if (bonus == 1)
            {
                var extra = DiceRoller.Roll(new DiceCode(1));
                finalTotal += extra.Total;
                _term.DiceRoll($"Extra die: [{string.Join(", ", extra.Rolls)}] = +{extra.Total}  → New total: {finalTotal}");
            }
            else
            {
                _state.ForcePoints++;
                _term.Mechanic($"Force Point gained! (Total: {_state.ForcePoints})");
            }
            _term.Blank();
        }

        // Force Point doubling applies after any wild die bonus
        finalTotal = ForceRoller.ApplyForcePointDouble(finalTotal, fpDouble, _term);

        _term.Mechanic($"vs Target Number: {check.TargetNumber}");

        if (finalTotal >= check.TargetNumber)
        {
            _term.Blank();
            _term.Narrative(check.SuccessText);
            if (check.CreditReward > 0)
            {
                _state.CreditsBalance += check.CreditReward;
                _term.Info($"  +{check.CreditReward} credits");
            }
            if (check.UpgradePointReward > 0)
            {
                _state.UpgradePoints += check.UpgradePointReward;
                _term.Info($"  +{check.UpgradePointReward} Upgrade Point{(check.UpgradePointReward > 1 ? "s" : "")} (Total: {_state.UpgradePoints})");
            }
            if (!check.Repeatable)
                _state.CompletedChecks.Add(check.Id);
        }
        else
        {
            _term.Blank();
            _term.Narrative(check.FailText);

            // --- Failure penalties ---
            if (check.CreditPenalty > 0)
            {
                int actual = Math.Min(check.CreditPenalty, _state.CreditsBalance);
                _state.CreditsBalance -= actual;
                _term.Error($"  -{actual} credits (Balance: {_state.CreditsBalance})");
            }
            if (!string.IsNullOrWhiteSpace(check.FailPenaltyText))
            {
                _term.Narrative(check.FailPenaltyText);
            }
            if (check.CombatNpcOnFail != null)
            {
                var enemy = check.CombatNpcOnFail();
                enemy.InitializeResolve();
                _term.Blank();
                _term.Combat($"Things turn violent — {enemy.Name} moves to attack!");
                _term.Mechanic($"Defense: {enemy.Defense} | Resolve: {enemy.Resolve}");

                var combat = new CombatEngine(_state.Player, enemy, _term, _state);
                bool survived = combat.RunCombat();
                if (!survived)
                {
                    _state.GameOver = true;
                    return;
                }
                if (enemy.IsDefeated)
                    _state.EnemiesDefeated++;
            }
        }
    }

    /// <summary>
    /// Skill upgrade cost: 3 × the new pip position (1st pip = 3, 2nd pip = 6, 3rd pip/new die = 9).
    /// Pip position cycles 1→2→3 with each die level.
    /// </summary>
    private static int SkillUpgradeCost(DiceCode current)
    {
        // After upgrade the new pip count is (current.Pips + 1) % 3, rolling over to next die.
        // Pip position in the cycle: pips 0→1 costs 3, 1→2 costs 6, 2→0(next die) costs 9.
        int pipPos = current.Pips + 1; // 1, 2, or 3 (3 means rolling to next die)
        return 3 * pipPos;
    }

    /// <summary>
    /// Attribute upgrade cost: 6 × the die value of the resulting dice code.
    /// All upgrades within the same die tier cost the same (e.g., all upgrades to 2D/2D+1/2D+2 = 12).
    /// </summary>
    private static int AttributeUpgradeCost(DiceCode current)
    {
        // The new dice code after +1 pip:
        var next = current + 1;
        return 6 * next.Dice;
    }

    private void Upgrade(string arg)
    {
        if (_state.UpgradePoints <= 0)
        {
            _term.Error("You have no Upgrade Points. Complete skill checks and quests to earn them.");
            return;
        }

        _term.Header("UPGRADE");
        _term.Info($"  Available Upgrade Points: {_state.UpgradePoints}");
        _term.Blank();
        _term.Info($"  Skill cost:     3 / 6 / 9 points per pip (cycles each die)");
        _term.Info($"  Attribute cost:  6 × new die value per pip (e.g. →2D costs 12)");
        _term.Blank();

        // Build numbered list of upgradeable options
        var options = new List<(string label, int cost, Action apply)>();

        // Attributes
        _term.SubHeader("Attributes");
        foreach (var attr in Enum.GetValues<AttributeType>())
        {
            var current = _state.Player.GetAttribute(attr);
            var next = current + 1;
            int cost = AttributeUpgradeCost(current);
            int idx = options.Count + 1;
            bool affordable = _state.UpgradePoints >= cost;
            var color = affordable ? "" : " (not enough points)";
            _term.Info($"  [{idx}] {attr,-12} {current} → {next}  [Cost: {cost}]{color}");
            var a = attr; // capture
            options.Add(($"{attr}", cost, () =>
            {
                _state.Player.Attributes[a] = next;
                _term.Info($"  {a} upgraded to {next}!");
            }));
        }

        // Skills
        _term.SubHeader("Skills");
        foreach (var attr in Enum.GetValues<AttributeType>())
        {
            foreach (var skill in SkillMap.GetSkillsFor(attr))
            {
                var currentBonus = _state.Player.SkillBonuses.TryGetValue(skill, out var b) ? b : new DiceCode(0);
                var currentTotal = _state.Player.GetSkill(skill);
                var nextBonus = currentBonus + 1;
                var nextTotal = _state.Player.GetAttribute(attr) + nextBonus;
                int cost = SkillUpgradeCost(currentBonus);
                int idx = options.Count + 1;
                bool affordable = _state.UpgradePoints >= cost;
                var color = affordable ? "" : " (not enough points)";
                _term.Info($"  [{idx}] {skill,-14} {currentTotal} → {nextTotal}  (bonus: {currentBonus} → {nextBonus})  [Cost: {cost}]{color}");
                var s = skill; // capture
                var nb = nextBonus;
                var c = cost;
                options.Add(($"{skill}", c, () =>
                {
                    _state.Player.SkillBonuses[s] = nb;
                    _term.Info($"  {s} bonus upgraded to {nb}!");
                }));
            }
        }

        _term.Info($"  [0] Cancel");
        _term.Blank();
        _term.Prompt("Upgrade which? (enter number):");
        int choice = _term.ReadChoice(0, options.Count);
        if (choice == 0) { _term.Info("Upgrade cancelled."); return; }

        var selected = options[choice - 1];
        if (_state.UpgradePoints < selected.cost)
        {
            _term.Error($"Not enough Upgrade Points. {selected.label} requires {selected.cost} but you have {_state.UpgradePoints}.");
            return;
        }

        // Store old resolve before applying
        int oldResolve = _state.Player.Resolve;

        selected.apply();
        _state.UpgradePoints -= selected.cost;
        _term.Info($"  Spent {selected.cost} Upgrade Points. Remaining: {_state.UpgradePoints}");

        // Recalculate resolve since Strength/Stamina may have changed
        if (_state.Player.Resolve != oldResolve)
        {
            int diff = _state.Player.Resolve - oldResolve;
            _state.Player.CurrentResolve += diff;
            _term.Mechanic($"Max Resolve changed to {_state.Player.Resolve} (Current: {_state.Player.CurrentResolve})");
        }
    }

    private void ShowHelp()
    {
        _term.Header("COMMANDS");
        _term.Info("  Movement:");
        _term.Info("    look / l              — Examine your surroundings");
        _term.Info("    go <direction>        — Move (or just type the direction)");
        _term.Info("    north/south/east/west — Cardinal directions");
        _term.Info("    up/down/dock/jump     — Special directions");
        _term.Info("    locator               — Location by Planet, Star System, Sector");
        _term.Info("    map                   — Show a planetary map of all rooms");
        _term.Info("");
        _term.Info("  Character:");
        _term.Info("    status / sheet        — View your character sheet");
        _term.Info("    inventory / inv / i   — View your inventory");
        _term.Info("    equip [#]             — Equip a weapon");
        _term.Info("    vehicles              — View your vehicles");
        _term.Info("");
        _term.Info("  Actions:");
        _term.Info("    search / scan         — Search the area");
        _term.Info("    talk                  — Talk to friendly NPCs");
        _term.Info("    use <item>            — Use an item (e.g., 'use medpack')");
        _term.Info("    rest                  — Rest to recover Resolve");
        _term.Info("    journal / missions    — View active and completed missions");
        _term.Info("    upgrade / levelup     — Spend Upgrade Points on attributes/skills");
        _term.Info("    roll <skill/attr>     — Make a skill or attribute roll");
        _term.Info("");
        _term.Info("  Vehicle:");
        _term.Info("    enter <space/land>    — Board a vehicle");
        _term.Info("    disembark             — Exit current vehicle");
        _term.Info("");
        _term.Info("  Commerce:");
        _term.Info("    shop / buy            — Browse the item shop");
        _term.Info("    ashop                 — Browse the armor shop");
        _term.Info("    vshop                 — Browse the vehicle dealer");
        _term.Info("    sell                  — Sell equipment from inventory");
        _term.Info("    sellv                 — Sell a vehicle");
        _term.Info("");
        _term.Info("  Save/Load:");
        _term.Info("    save [name]           — Save game (default: character name)");
        _term.Info("    load [name]           — Load a saved game");
        _term.Info("    saves                 — List all save files");
        _term.Info("");
        _term.Info("  System:");
        _term.Info("    help / ?              — This help screen");
        _term.Info("    quit / exit           — End the game");
        _term.Blank();
        _term.SubHeader("COLOR GUIDE");
        _term.Narrative("  Cyan = Narrative descriptions");
        _term.Dialogue("NPC", "Yellow = Dialogue");
        _term.DiceRoll("Magenta = Dice rolls");
        _term.Combat("  Red = Combat");
        _term.Mechanic("Dark Yellow = Game mechanics");
        _term.Prompt("  Green = Prompts & input");
    }
}
