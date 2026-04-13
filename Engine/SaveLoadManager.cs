using TerminalHyperspace.Models;

namespace TerminalHyperspace.Engine;

public static class SaveLoadManager
{
    private static string SaveDirectory =>
        Path.Combine(AppContext.BaseDirectory, "saves");

    public static void Save(GameState state, string? fileName = null)
    {
        Directory.CreateDirectory(SaveDirectory);
        fileName ??= SanitizeFileName(state.Player.Name);
        var path = Path.Combine(SaveDirectory, fileName + ".SAV");

        using var writer = new StreamWriter(path);
        var p = state.Player;

        writer.WriteLine("[CHARACTER]");
        writer.WriteLine($"Name={p.Name}");
        writer.WriteLine($"Species={p.SpeciesName}");
        writer.WriteLine($"Role={p.RoleName}");
        writer.WriteLine($"CurrentResolve={p.CurrentResolve}");

        writer.WriteLine("[ATTRIBUTES]");
        foreach (var kv in p.Attributes)
            writer.WriteLine($"{kv.Key}={kv.Value.Dice},{kv.Value.Pips}");

        writer.WriteLine("[SKILLS]");
        foreach (var kv in p.SkillBonuses)
            writer.WriteLine($"{kv.Key}={kv.Value.Dice},{kv.Value.Pips}");

        writer.WriteLine("[INVENTORY]");
        foreach (var item in p.Inventory)
        {
            writer.WriteLine($"Item={item.Name}|{item.Description}|{item.IsWeapon}|{item.Damage.Dice},{item.Damage.Pips}|{item.AttackSkill}|{item.Range}");
        }
        writer.WriteLine($"EquippedWeapon={p.EquippedWeapon?.Name ?? ""}");
        writer.WriteLine($"EquippedArmor={p.EquippedArmor.Name}");

        writer.WriteLine("[VEHICLES]");
        if (p.SpaceVehicle != null)
            WriteVehicle(writer, "Space", p.SpaceVehicle);
        if (p.LandVehicle != null)
            WriteVehicle(writer, "Land", p.LandVehicle);
        writer.WriteLine($"InVehicle={p.InVehicle}");
        writer.WriteLine($"InSpaceVehicle={p.InSpaceVehicle}");

        writer.WriteLine("[GAMESTATE]");
        writer.WriteLine($"Location={state.CurrentLocationId}");
        writer.WriteLine($"TurnCount={state.TurnCount}");
        writer.WriteLine($"Credits={state.CreditsBalance}");
        writer.WriteLine($"EnemiesDefeated={state.EnemiesDefeated}");
        writer.WriteLine($"Visited={string.Join(",", state.VisitedLocations)}");
        writer.WriteLine($"Cleared={string.Join(",", state.ClearedRooms)}");
    }

    public static GameState Load(string? fileName)
    {
        if (fileName == null) throw new FileNotFoundException("No file name specified.");
        if (!fileName.EndsWith(".SAV", StringComparison.OrdinalIgnoreCase))
            fileName += ".SAV";
        var path = Path.Combine(SaveDirectory, fileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Save file not found: {path}");

        var lines = File.ReadAllLines(path);
        var state = new GameState();
        state.Initialize();
        var player = new Character { IsPlayer = true };
        state.Player = player;

        string section = "";
        string equippedWeaponName = "";

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                section = line;
                continue;
            }

            var eqIdx = line.IndexOf('=');
            if (eqIdx < 0) continue;
            var key = line[..eqIdx];
            var val = line[(eqIdx + 1)..];

            switch (section)
            {
                case "[CHARACTER]":
                    switch (key)
                    {
                        case "Name": player.Name = val; break;
                        case "Species": player.SpeciesName = val; break;
                        case "Role": player.RoleName = val; break;
                        case "CurrentResolve": player.CurrentResolve = int.Parse(val); break;
                    }
                    break;

                case "[ATTRIBUTES]":
                    if (Enum.TryParse<AttributeType>(key, out var attr))
                    {
                        var parts = val.Split(',');
                        player.Attributes[attr] = new DiceCode(int.Parse(parts[0]), int.Parse(parts[1]));
                    }
                    break;

                case "[SKILLS]":
                    if (Enum.TryParse<SkillType>(key, out var skill))
                    {
                        var parts = val.Split(',');
                        player.SkillBonuses[skill] = new DiceCode(int.Parse(parts[0]), int.Parse(parts[1]));
                    }
                    break;

                case "[INVENTORY]":
                    if (key == "Item")
                    {
                        var parts = val.Split('|');
                        var item = new Item
                        {
                            Name = parts[0],
                            Description = parts[1],
                            IsWeapon = bool.Parse(parts[2]),
                            Range = int.Parse(parts[5]),
                        };
                        var dmgParts = parts[3].Split(',');
                        item.Damage = new DiceCode(int.Parse(dmgParts[0]), int.Parse(dmgParts[1]));
                        if (parts[4] != "" && Enum.TryParse<SkillType>(parts[4], out var atkSkill))
                            item.AttackSkill = atkSkill;
                        player.Inventory.Add(item);
                    }
                    else if (key == "EquippedWeapon")
                    {
                        equippedWeaponName = val;
                    }
                    else if (key == "EquippedArmor")
                    {
                        player.EquippedArmor = Content.ArmorData.FindByName(val);
                    }
                    break;

                case "[VEHICLES]":
                    if (key.StartsWith("Space_") || key.StartsWith("Land_"))
                        ParseVehicleLine(player, key, val);
                    else if (key == "InVehicle")
                        player.InVehicle = bool.Parse(val);
                    else if (key == "InSpaceVehicle")
                        player.InSpaceVehicle = bool.Parse(val);
                    break;

                case "[GAMESTATE]":
                    switch (key)
                    {
                        case "Location": state.CurrentLocationId = val; break;
                        case "TurnCount": state.TurnCount = int.Parse(val); break;
                        case "Credits": state.CreditsBalance = int.Parse(val); break;
                        case "EnemiesDefeated": state.EnemiesDefeated = int.Parse(val); break;
                        case "Visited":
                            foreach (var loc in val.Split(',', StringSplitOptions.RemoveEmptyEntries))
                                state.VisitedLocations.Add(loc);
                            break;
                        case "Cleared":
                            foreach (var room in val.Split(',', StringSplitOptions.RemoveEmptyEntries))
                                state.ClearedRooms.Add(room);
                            break;
                    }
                    break;
            }
        }

        // Re-link equipped weapon
        if (!string.IsNullOrEmpty(equippedWeaponName))
            player.EquippedWeapon = player.Inventory.FirstOrDefault(i => i.Name == equippedWeaponName);

        // Re-initialize vehicle resolve if currently aboard
        if (player.InVehicle)
            player.ActiveVehicle?.InitializeResolve();

        return state;
    }

    public static List<string> ListSaves()
    {
        if (!Directory.Exists(SaveDirectory))
            return new();
        return Directory.GetFiles(SaveDirectory, "*.SAV")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => n != null)
            .Cast<string>()
            .OrderByDescending(n => File.GetLastWriteTime(Path.Combine(SaveDirectory, n + ".SAV")))
            .ToList();
    }

    private static void WriteVehicle(StreamWriter writer, string prefix, Vehicle v)
    {
        writer.WriteLine($"{prefix}_Name={v.Name}");
        writer.WriteLine($"{prefix}_Desc={v.Description}");
        writer.WriteLine($"{prefix}_IsSpace={v.IsSpace}");
        writer.WriteLine($"{prefix}_Maneuver={v.Maneuverability.Dice},{v.Maneuverability.Pips}");
        writer.WriteLine($"{prefix}_Resolve={v.Resolve}");
        writer.WriteLine($"{prefix}_CurrentResolve={v.CurrentResolve}");
        writer.WriteLine($"{prefix}_ShieldName={v.Shield.Name}");
        writer.WriteLine($"{prefix}_ShieldDice={v.Shield.DiceCode.Dice},{v.Shield.DiceCode.Pips}");
        writer.WriteLine($"{prefix}_WeaponCount={v.Weapons.Count}");
        for (int i = 0; i < v.Weapons.Count; i++)
        {
            var w = v.Weapons[i];
            writer.WriteLine($"{prefix}_Weapon{i}={w.Name}|{w.Damage.Dice},{w.Damage.Pips}|{w.AttackSkill}");
        }
    }

    private static void ParseVehicleLine(Character player, string key, string val)
    {
        var prefix = key.StartsWith("Space_") ? "Space" : "Land";
        var field = key[(prefix.Length + 1)..];
        bool isSpace = prefix == "Space";

        var vehicle = isSpace ? player.SpaceVehicle : player.LandVehicle;
        if (vehicle == null)
        {
            vehicle = new Vehicle();
            if (isSpace) player.SpaceVehicle = vehicle;
            else player.LandVehicle = vehicle;
        }

        switch (field)
        {
            case "Name": vehicle.Name = val; break;
            case "Desc": vehicle.Description = val; break;
            case "IsSpace": vehicle.IsSpace = bool.Parse(val); break;
            case "Maneuver":
                var mp = val.Split(',');
                vehicle.Maneuverability = new DiceCode(int.Parse(mp[0]), int.Parse(mp[1]));
                break;
            case "Resolve": vehicle.Resolve = int.Parse(val); break;
            case "CurrentResolve": vehicle.CurrentResolve = int.Parse(val); break;
            case "ShieldName":
                vehicle.Shield = Content.ShieldData.FindByName(val);
                break;
            case "ShieldDice":
                var sp = val.Split(',');
                vehicle.Shield = new VehicleShield
                {
                    Name = vehicle.Shield.Name,
                    DiceCode = new DiceCode(int.Parse(sp[0]), int.Parse(sp[1]))
                };
                break;
            default:
                if (field.StartsWith("Weapon") && field != "WeaponCount")
                {
                    var parts = val.Split('|');
                    var dmgParts = parts[1].Split(',');
                    var weapon = new VehicleWeapon
                    {
                        Name = parts[0],
                        Damage = new DiceCode(int.Parse(dmgParts[0]), int.Parse(dmgParts[1])),
                    };
                    if (Enum.TryParse<SkillType>(parts[2], out var atkSkill))
                        weapon.AttackSkill = atkSkill;
                    vehicle.Weapons.Add(weapon);
                }
                break;
        }
    }

    public static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
    }
}
