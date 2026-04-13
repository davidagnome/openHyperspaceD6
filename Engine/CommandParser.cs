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
            case "up": case "down": case "dock": case "land":
            case "jump": case "explore": case "board":
            case "leave": case "airlock":
                Move(cmd);
                break;
            case "locator":
                Locator();
                break;
            case "status":
            case "sheet":
            case "char":
                _term.CharacterSheet(_state.Player);
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
            case "talk":
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

    public void Look()
    {
        var loc = _state.CurrentLocation;
        _term.LocationHeader(loc.Name);
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

        // Auto-disembark when entering a non-space location from space
        if (_state.Player.InSpaceVehicle && !_state.World[finalDestId].IsSpace)
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
        CheckEncounter();
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
            var combat = new CombatEngine(_state.Player, enemy, _term);
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
                var combat = new CombatEngine(_state.Player, enemy, _term);
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

    private void ShowInventory()
    {
        _term.SubHeader("Inventory");
        _term.Info($"  Credits: {_state.CreditsBalance}");
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

        var items = new (Item item, int price)[]
        {
            (ItemData.BlasterPistol, 50),
            (ItemData.HeavyBlaster, 120),
            (ItemData.BlasterRifle, 200),
            (ItemData.Vibroblade, 80),
            (ItemData.ForcePike, 180),
            (ItemData.Medpack, 30),
            (ItemData.DataPad, 40),
            (ItemData.Macrobinoculars, 60),
        };

        for (int i = 0; i < items.Length; i++)
            _term.Info($"  [{i + 1}] {items[i].item.Name,-25} {items[i].price} credits — {items[i].item.Description}");
        _term.Info("  [0] Leave shop");

        _term.Prompt("Buy which item?");
        int choice = _term.ReadChoice(0, items.Length);
        if (choice == 0) { _term.Info("You leave the shop."); return; }

        var (selectedItem, cost) = items[choice - 1];
        if (_state.CreditsBalance < cost)
        {
            _term.Error("Not enough credits!");
            return;
        }

        _state.CreditsBalance -= cost;
        _state.Player.Inventory.Add(selectedItem);
        _term.Info($"Purchased {selectedItem.Name} for {cost} credits. Remaining: {_state.CreditsBalance}");
    }

    private void VehicleShop()
    {
        if (!_state.CurrentLocation.HasVehicleShop) { _term.Error("No vehicle dealer here."); return; }

        _term.SubHeader("VEHICLE DEALER");
        _term.Info($"  Your credits: {_state.CreditsBalance}");

        var vehicles = new (Vehicle v, int price)[]
        {
            (VehicleData.Speeder, 200),
            (VehicleData.CombatSpeeder, 500),
            (VehicleData.ArmoredTransport, 800),
            (VehicleData.Starfighter, 1000),
            (VehicleData.LightFreighter, 1500),
        };

        for (int i = 0; i < vehicles.Length; i++)
        {
            var v = vehicles[i].v;
            var tag = v.IsSpace ? "[SPACE]" : "[LAND]";
            _term.Info($"  [{i + 1}] {tag} {v.Name,-30} {vehicles[i].price} credits");
            _term.Info($"       {v.Description}");
            _term.Mechanic($"       Maneuver: {v.Maneuverability} | Resolve: {v.Resolve} | Shields: {v.Shield}");
            if (v.Weapons.Count > 0)
                _term.Mechanic($"       Weapons: {string.Join(", ", v.Weapons)}");
        }
        _term.Info("  [0] Leave");

        _term.Prompt("Purchase which vehicle?");
        int choice = _term.ReadChoice(0, vehicles.Length);
        if (choice == 0) return;

        var (selected, cost) = vehicles[choice - 1];
        if (_state.CreditsBalance < cost) { _term.Error("Not enough credits!"); return; }

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

        _state.CreditsBalance -= cost;
        _term.Info($"Purchased {selected.Name} for {cost} credits!");
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
        }).ToList()
    };

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

        var dialogues = GetDialogue(npc.Name, loc.Id);
        foreach (var (speaker, line) in dialogues)
            _term.Dialogue(speaker, line);

        // Chance of a quest reward
        if (_rng.NextDouble() < 0.3)
        {
            int reward = _rng.Next(20, 80);
            _state.CreditsBalance += reward;
            _term.Info($"  You received {reward} credits for your time.");
        }
    }

    private List<(string speaker, string line)> GetDialogue(string npcName, string locationId)
    {
        var lines = new List<(string, string)[]>
        {
            new[]
            {
                (npcName, "You look like someone who can handle themselves. Interested in work?"),
                (_state.Player.Name, "Depends on the pay."),
                (npcName, "Smart answer. The Outer Sectors are crawling with opportunity... and danger."),
            },
            new[]
            {
                (npcName, "Imperial patrols have been getting bolder. Bad for business, if you catch my meaning."),
                (_state.Player.Name, "I've noticed."),
                (npcName, "Word of advice—keep your head down in the Upper District. Eyes everywhere up there."),
            },
            new[]
            {
                (npcName, "I've got goods from a dozen systems. What are you looking for?"),
                (_state.Player.Name, "Information, mostly."),
                (npcName, "That's the most expensive commodity in the galaxy, friend. But I like your face. There's a derelict station out in the Rift Expanse. Salvagers keep disappearing near it."),
            },
            new[]
            {
                (npcName, "Careful down in the tunnels. Something's been breeding down there."),
                (_state.Player.Name, "What kind of something?"),
                (npcName, "The kind that doesn't show up on sensors until it's already too close."),
            },
            new[]
            {
                (npcName, "You a pilot? I've got a lead on a ship for sale in the hangar. Previous owner... won't be needing it anymore."),
                (_state.Player.Name, "What happened to the previous owner?"),
                (npcName, "Let's just say he made the jump to hyperspace without a ship. Gambling debts are ugly business."),
            },
        };

        return lines[_rng.Next(lines.Count)].ToList();
    }

    private void SearchArea()
    {
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

    private void ShowHelp()
    {
        _term.Header("COMMANDS");
        _term.Info("  Movement:");
        _term.Info("    look / l              — Examine your surroundings");
        _term.Info("    go <direction>        — Move (or just type the direction)");
        _term.Info("    north/south/east/west — Cardinal directions");
        _term.Info("    up/down/dock/jump     — Special directions");
        _term.Info("    locator               — Location by Planet, Star System, Sector");
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
