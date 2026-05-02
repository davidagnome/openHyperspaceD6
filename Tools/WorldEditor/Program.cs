using Avalonia;

namespace TerminalHyperspace.WorldEditor;

public static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        // Lightweight headless probe to verify the parser round-trips a given
        // LocationData.cs without opening a window. Invoked by tests/CI.
        if (args.Length >= 1 && args[0] == "--probe")
        {
            var path = args.Length >= 2 ? args[1] : "Content/LocationData.cs";
            var p = new LocationFileParser(path);
            if (!p.TryLoad())
            {
                Console.Error.WriteLine($"parse failed: {p.Error}");
                return 1;
            }
            Console.WriteLine($"Loaded {p.Rooms.Count} rooms from {path}");
            foreach (var r in p.Rooms.Take(3))
                Console.WriteLine($"  {r.Id}: Name='{r.Name}' Exits={r.Exits.Count} Ambient={r.AmbientMessages.Count} Encounters=[{string.Join(",", r.PossibleEncounters)}] Climate={r.Climate}");
            return 0;
        }

        // CRUD round-trip probe for the LocationChecks editor: parse a copy of
        // SkillCheckData.cs, perform create/update/delete on a couple of entries,
        // write it back, reparse, assert the changes survived.
        if (args.Length >= 1 && args[0] == "--probe-locchecks")
        {
            var src = args.Length >= 2 ? args[1] : "Content/SkillCheckData.cs";
            return RunLocationChecksProbe(src);
        }
        if (args.Length >= 1 && args[0] == "--probe-armor")
        {
            var src = args.Length >= 2 ? args[1] : "Content/ArmorData.cs";
            return RunArmorProbe(src);
        }
        if (args.Length >= 1 && args[0] == "--probe-spaceenc")
        {
            var src = args.Length >= 2 ? args[1] : "Content/SpaceEncounterData.cs";
            return RunSpaceEncounterProbe(src);
        }
        if (args.Length >= 1 && args[0] == "--probe-dialoguepool")
        {
            var src = args.Length >= 2 ? args[1] : "Content/LocationData.cs";
            return RunDialoguePoolProbe(src);
        }
        if (args.Length >= 1 && args[0] == "--ensure-location-defaults")
        {
            var src = args.Length >= 2 ? args[1] : "Content/LocationData.cs";
            return RunEnsureLocationDefaults(src);
        }
        if (args.Length >= 1 && args[0] == "--probe-dialogue")
        {
            var src = args.Length >= 2 ? args[1] : "Content/DialogueData.cs";
            return RunDialogueProbe(src);
        }
        if (args.Length >= 1 && args[0] == "--probe-dialoguepools-file")
        {
            var src = args.Length >= 2 ? args[1] : "Content/DialogueData.cs";
            return RunDialoguePoolFileProbe(src);
        }
        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static int RunArmorProbe(string src)
    {
        if (!File.Exists(src)) { Console.Error.WriteLine($"file not found: {src}"); return 1; }
        var tmp = Path.Combine(Path.GetTempPath(), $"armor_probe_{Guid.NewGuid():N}.cs");
        File.Copy(src, tmp, overwrite: true);
        try
        {
            // READ
            var p1 = new ArmorFileParser(tmp);
            if (!p1.TryLoad()) { Console.Error.WriteLine($"initial parse failed: {p1.Error}"); return 1; }
            Console.WriteLine($"READ: {p1.Armors.Count} armors");
            if (p1.Armors.Count == 0) { Console.Error.WriteLine("no armors found"); return 1; }

            var armors = p1.Armors.ToList();
            var firstName = armors[0].MemberName;
            var lastName  = armors[^1].MemberName;
            var beforePrice = armors[0].Price;

            // UPDATE: tweak first armor's price
            armors[0].Price = beforePrice + 17;
            // DELETE: drop last armor
            armors.RemoveAt(armors.Count - 1);
            // CREATE: add a brand-new armor
            armors.Add(new ArmorModel
            {
                MemberName = "ProbeTestArmor",
                Name = "Probe Test Armor",
                Dice = 7,
                Price = 4242,
                Climate = "Hot",
                Purchasable = true,
            });

            // WRITE
            ArmorFileWriter.Save(p1, armors);
            Console.WriteLine("WRITE: saved temp copy");

            // REPARSE + ASSERT
            var p2 = new ArmorFileParser(tmp);
            if (!p2.TryLoad()) { Console.Error.WriteLine($"reparse failed: {p2.Error}"); return 1; }
            Console.WriteLine($"REREAD: {p2.Armors.Count} armors");

            var firstAfter = p2.Armors.FirstOrDefault(a => a.MemberName == firstName);
            if (firstAfter == null) { Console.Error.WriteLine($"FAIL: {firstName} missing after save"); return 1; }
            if (firstAfter.Price != beforePrice + 17)
            { Console.Error.WriteLine($"FAIL: {firstName} price {firstAfter.Price}, expected {beforePrice + 17}"); return 1; }
            Console.WriteLine($"  OK update: {firstName}.Price {beforePrice} -> {firstAfter.Price}");

            if (p2.Armors.Any(a => a.MemberName == lastName))
            { Console.Error.WriteLine($"FAIL: {lastName} should have been deleted"); return 1; }
            Console.WriteLine($"  OK delete: {lastName} pruned");

            var newAfter = p2.Armors.FirstOrDefault(a => a.MemberName == "ProbeTestArmor");
            if (newAfter == null) { Console.Error.WriteLine("FAIL: new armor missing"); return 1; }
            if (newAfter.Name != "Probe Test Armor" || newAfter.Dice != 7 || newAfter.Price != 4242 || newAfter.Climate != "Hot")
            { Console.Error.WriteLine($"FAIL: new armor wrong: Name={newAfter.Name} Dice={newAfter.Dice} Price={newAfter.Price} Climate={newAfter.Climate}"); return 1; }
            Console.WriteLine($"  OK create: ProbeTestArmor written (Climate=Hot, 7D, 4242cr)");

            // Verify aggregator-list rewrite: the deleted name must not appear in `All`,
            // and the new name must appear in both `All` and `Purchasable`.
            var saved = File.ReadAllText(tmp);
            if (saved.Contains($"All => new()") && new System.Text.RegularExpressions.Regex(@"List<Armor>\s+All\s*=>\s*new\(\)\s*\{[^}]*\b" + lastName + @"\b").IsMatch(saved))
            { Console.Error.WriteLine($"FAIL: All list still references {lastName}"); return 1; }
            if (!new System.Text.RegularExpressions.Regex(@"List<Armor>\s+All\s*=>\s*new\(\)\s*\{[^}]*\bProbeTestArmor\b").IsMatch(saved))
            { Console.Error.WriteLine("FAIL: All list missing ProbeTestArmor"); return 1; }
            if (!new System.Text.RegularExpressions.Regex(@"List<Armor>\s+Purchasable\s*=>\s*new\(\)\s*\{[^}]*\bProbeTestArmor\b").IsMatch(saved))
            { Console.Error.WriteLine("FAIL: Purchasable list missing ProbeTestArmor"); return 1; }
            Console.WriteLine("  OK aggregators: All & Purchasable rewritten correctly");

            Console.WriteLine("Armor CRUD probe PASSED");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine($"probe crashed: {ex}"); return 1; }
        finally
        {
            if (Environment.GetEnvironmentVariable("LC_PROBE_KEEP") == "1")
                Console.WriteLine($"KEEP: {tmp}");
            else try { File.Delete(tmp); } catch { }
        }
    }

    private static int RunSpaceEncounterProbe(string src)
    {
        if (!File.Exists(src)) { Console.Error.WriteLine($"file not found: {src}"); return 1; }
        var tmp = Path.Combine(Path.GetTempPath(), $"spaceenc_probe_{Guid.NewGuid():N}.cs");
        File.Copy(src, tmp, overwrite: true);
        try
        {
            // READ
            var p1 = new SpaceEncounterFileParser(tmp);
            if (!p1.TryLoad()) { Console.Error.WriteLine($"initial parse failed: {p1.Error}"); return 1; }
            Console.WriteLine($"READ: {p1.Encounters.Count} encounters");
            if (p1.Encounters.Count == 0) { Console.Error.WriteLine("no encounters found"); return 1; }

            var encs = p1.Encounters.ToList();
            var firstName = encs[0].MemberName;
            var lastName  = encs[^1].MemberName;
            var beforeResolve = encs[0].ShipResolve;

            // UPDATE: bump first encounter's ship Resolve
            encs[0].ShipResolve = beforeResolve + 5;
            // DELETE: drop last encounter
            encs.RemoveAt(encs.Count - 1);
            // CREATE: add a brand-new encounter
            var newEnc = new SpaceEncounterModel
            {
                MemberName = "ProbeTestEncounter",
                PilotName = "Probe Pilot",
                ShipName  = "Probe Ship",
                ShipDescription = "A spacecraft used solely to test the editor's CRUD pipeline.",
                ShipIsSpace = true,
                ShipManeuverDice = 2,
                ShipManeuverPips = 1,
                ShipResolve = 18,
                ShipShieldMember = "CivilianShields",
                PilotEquippedArmorMember = "PaddedFlightSuit",
            };
            newEnc.PilotAttributes["Dexterity"] = (2, 0);
            newEnc.PilotSkillBonuses["Pilot"]   = (1, 0);
            newEnc.ShipWeapons.Add(new VehicleWeaponModel { Name = "Probe Cannon", DamageDice = 3, DamagePips = 0, AttackSkill = "Gunnery" });
            encs.Add(newEnc);

            // WRITE
            SpaceEncounterFileWriter.Save(p1, encs);
            Console.WriteLine("WRITE: saved temp copy");

            // REPARSE + ASSERT
            var p2 = new SpaceEncounterFileParser(tmp);
            if (!p2.TryLoad()) { Console.Error.WriteLine($"reparse failed: {p2.Error}"); return 1; }
            Console.WriteLine($"REREAD: {p2.Encounters.Count} encounters");

            var firstAfter = p2.Encounters.FirstOrDefault(e => e.MemberName == firstName);
            if (firstAfter == null) { Console.Error.WriteLine($"FAIL: {firstName} missing"); return 1; }
            if (firstAfter.ShipResolve != beforeResolve + 5)
            { Console.Error.WriteLine($"FAIL: {firstName}.ShipResolve {firstAfter.ShipResolve}, expected {beforeResolve + 5}"); return 1; }
            Console.WriteLine($"  OK update: {firstName}.ShipResolve {beforeResolve} -> {firstAfter.ShipResolve}");

            if (p2.Encounters.Any(e => e.MemberName == lastName))
            { Console.Error.WriteLine($"FAIL: {lastName} should have been deleted"); return 1; }
            // Confirm AllEncounters aggregator no longer references the deleted name.
            var saved = File.ReadAllText(tmp);
            if (new System.Text.RegularExpressions.Regex(@"List<Func<SpaceEncounter>>\s+\w+\s*=>\s*new\(\)\s*\{[^}]*\b" + lastName + @"\b").IsMatch(saved))
            { Console.Error.WriteLine($"FAIL: AllEncounters still references {lastName}"); return 1; }
            Console.WriteLine($"  OK delete: {lastName} pruned (and stripped from AllEncounters)");

            var newAfter = p2.Encounters.FirstOrDefault(e => e.MemberName == "ProbeTestEncounter");
            if (newAfter == null) { Console.Error.WriteLine("FAIL: new encounter missing"); return 1; }
            if (newAfter.PilotName != "Probe Pilot" || newAfter.ShipName != "Probe Ship" || newAfter.ShipResolve != 18 || newAfter.ShipShieldMember != "CivilianShields")
            { Console.Error.WriteLine($"FAIL: new encounter wrong: PilotName={newAfter.PilotName} ShipName={newAfter.ShipName} Resolve={newAfter.ShipResolve} Shield={newAfter.ShipShieldMember}"); return 1; }
            if (!newAfter.PilotAttributes.TryGetValue("Dexterity", out var dex) || dex.Dice != 2)
            { Console.Error.WriteLine("FAIL: new encounter pilot Dexterity wrong"); return 1; }
            if (newAfter.ShipWeapons.Count != 1 || newAfter.ShipWeapons[0].Name != "Probe Cannon")
            { Console.Error.WriteLine("FAIL: new encounter ship weapons wrong"); return 1; }
            Console.WriteLine($"  OK create: ProbeTestEncounter written (Resolve=18, Shield=CivilianShields, 1 weapon)");

            Console.WriteLine("SpaceEncounter CRUD probe PASSED");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine($"probe crashed: {ex}"); return 1; }
        finally
        {
            if (Environment.GetEnvironmentVariable("LC_PROBE_KEEP") == "1")
                Console.WriteLine($"KEEP: {tmp}");
            else try { File.Delete(tmp); } catch { }
        }
    }

    private static int RunLocationChecksProbe(string src)
    {
        if (!File.Exists(src)) { Console.Error.WriteLine($"file not found: {src}"); return 1; }
        var tmp = Path.Combine(Path.GetTempPath(), $"locchecks_probe_{Guid.NewGuid():N}.cs");
        File.Copy(src, tmp, overwrite: true);
        try
        {
            // ---------- READ ----------
            var p1 = new LocationChecksFileParser(tmp);
            if (!p1.TryLoad()) { Console.Error.WriteLine($"initial parse failed: {p1.Error}"); return 1; }
            Console.WriteLine($"READ: {p1.Entries.Count} entries; first = {p1.Entries[0].LocationId} ({p1.Entries[0].CheckMemberNames.Count} checks)");

            var entries = p1.Entries.ToList();
            var beforeFirstCount = entries[0].CheckMemberNames.Count;
            var firstId = entries[0].LocationId;
            var lastId = entries[^1].LocationId;
            // Pick a check name that exists somewhere in the file so the reference is valid.
            var anyMember = entries.SelectMany(e => e.CheckMemberNames).First();

            // ---------- UPDATE: add a check to first entry ----------
            entries[0].CheckMemberNames.Add(anyMember);

            // ---------- DELETE: remove last entry by clearing it (writer drops empties) ----------
            var lastEntry = entries[^1];
            lastEntry.CheckMemberNames.Clear();

            // ---------- CREATE: append a brand-new entry ----------
            var newId = "probe_brand_new_location";
            entries.Add(new LocationCheckEntryModel
            {
                LocationId = newId,
                CheckMemberNames = new List<string> { anyMember },
            });

            // ---------- WRITE ----------
            LocationChecksFileWriter.Save(p1, entries);
            Console.WriteLine("WRITE: saved temp copy");

            // ---------- REPARSE + ASSERT ----------
            var p2 = new LocationChecksFileParser(tmp);
            if (!p2.TryLoad()) { Console.Error.WriteLine($"reparse failed: {p2.Error}"); return 1; }
            Console.WriteLine($"REREAD: {p2.Entries.Count} entries");

            var firstAfter = p2.Entries.FirstOrDefault(e => e.LocationId == firstId);
            if (firstAfter == null) { Console.Error.WriteLine($"FAIL: first entry {firstId} missing"); return 1; }
            if (firstAfter.CheckMemberNames.Count != beforeFirstCount + 1)
            { Console.Error.WriteLine($"FAIL: first entry expected {beforeFirstCount + 1} members, got {firstAfter.CheckMemberNames.Count}"); return 1; }
            if (firstAfter.CheckMemberNames[^1] != anyMember)
            { Console.Error.WriteLine($"FAIL: appended check not at tail: {string.Join(",", firstAfter.CheckMemberNames)}"); return 1; }
            Console.WriteLine($"  OK update: {firstId} now has {firstAfter.CheckMemberNames.Count} checks (was {beforeFirstCount})");

            if (p2.Entries.Any(e => e.LocationId == lastId))
            { Console.Error.WriteLine($"FAIL: emptied entry {lastId} should have been pruned"); return 1; }
            Console.WriteLine($"  OK delete: {lastId} pruned (was emptied)");

            var newAfter = p2.Entries.FirstOrDefault(e => e.LocationId == newId);
            if (newAfter == null) { Console.Error.WriteLine($"FAIL: new entry {newId} missing"); return 1; }
            if (newAfter.CheckMemberNames.Count != 1 || newAfter.CheckMemberNames[0] != anyMember)
            { Console.Error.WriteLine($"FAIL: new entry contents wrong: {string.Join(",", newAfter.CheckMemberNames)}"); return 1; }
            Console.WriteLine($"  OK create: {newId} written with [{string.Join(",", newAfter.CheckMemberNames)}]");

            Console.WriteLine("CRUD probe PASSED");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"probe crashed: {ex}");
            return 1;
        }
        finally
        {
            if (Environment.GetEnvironmentVariable("LC_PROBE_KEEP") == "1")
                Console.WriteLine($"KEEP: {tmp}");
            else
                try { File.Delete(tmp); } catch { }
        }
    }

    private static int RunDialoguePoolProbe(string src)
    {
        if (!File.Exists(src)) { Console.Error.WriteLine($"file not found: {src}"); return 1; }
        var tmp = Path.Combine(Path.GetTempPath(), $"dialoguepool_probe_{Guid.NewGuid():N}.cs");
        File.Copy(src, tmp, overwrite: true);
        try
        {
            // READ
            var p1 = new LocationFileParser(tmp);
            if (!p1.TryLoad()) { Console.Error.WriteLine($"initial parse failed: {p1.Error}"); return 1; }
            Console.WriteLine($"READ: {p1.Rooms.Count} rooms");

            // Find a room that has DialoguePool = DialogueData.Default (cantina seed).
            var defaultRoom = p1.Rooms.FirstOrDefault(r => r.DialoguePool.Count == 1 && r.DialoguePool[0] == "Default");
            if (defaultRoom == null) { Console.Error.WriteLine("FAIL: no room with DialoguePool=Default"); return 1; }
            Console.WriteLine($"  found Default-pool room: {defaultRoom.Id}");

            // UPDATE: replace Default with two explicit factories (round-trip the long form).
            var rooms = p1.Rooms.ToList();
            var target = rooms.First(r => r.Id == defaultRoom.Id);
            target.DialoguePool.Clear();
            target.DialoguePool.Add("WorkOpportunity");
            target.DialoguePool.Add("ImperialPatrols");

            // CREATE: pick a room with no DialoguePool, add one.
            var emptyRoom = rooms.FirstOrDefault(r => r.DialoguePool.Count == 0 && !r.IsNew);
            if (emptyRoom == null) { Console.Error.WriteLine("FAIL: no empty-pool room available"); return 1; }
            emptyRoom.DialoguePool.Add("TunnelCreatures");
            var createdRoomId = emptyRoom.Id;

            // DELETE: pick another Default-pool room (if any) and clear its pool entirely.
            var anotherDefault = rooms.FirstOrDefault(r =>
                r.Id != defaultRoom.Id && r.DialoguePool.Count == 1 && r.DialoguePool[0] == "Default");
            string? clearedRoomId = null;
            if (anotherDefault != null)
            {
                anotherDefault.DialoguePool.Clear();
                clearedRoomId = anotherDefault.Id;
            }

            // WRITE
            LocationFileWriter.Save(p1, rooms);
            Console.WriteLine("WRITE: saved temp copy");

            // REPARSE + ASSERT
            var p2 = new LocationFileParser(tmp);
            if (!p2.TryLoad()) { Console.Error.WriteLine($"reparse failed: {p2.Error}"); return 1; }
            var reTarget = p2.Rooms.FirstOrDefault(r => r.Id == defaultRoom.Id);
            if (reTarget == null || reTarget.DialoguePool.Count != 2
                || reTarget.DialoguePool[0] != "WorkOpportunity"
                || reTarget.DialoguePool[1] != "ImperialPatrols")
            { Console.Error.WriteLine($"FAIL: update wrong: [{string.Join(",", reTarget?.DialoguePool ?? new())}]"); return 1; }
            Console.WriteLine($"  OK update: {defaultRoom.Id} now has [{string.Join(",", reTarget.DialoguePool)}]");

            var reCreated = p2.Rooms.FirstOrDefault(r => r.Id == createdRoomId);
            if (reCreated == null || reCreated.DialoguePool.Count != 1 || reCreated.DialoguePool[0] != "TunnelCreatures")
            { Console.Error.WriteLine($"FAIL: create wrong: [{string.Join(",", reCreated?.DialoguePool ?? new())}]"); return 1; }
            // TunnelCreatures is a Dialogue factory (not a pool), so the writer
            // must wrap it in `new() { … }` rather than using the shortcut form
            // — `DialoguePool = DialogueData.TunnelCreatures,` would not compile.
            var saved = File.ReadAllText(tmp);
            if (!saved.Contains("DialoguePool = new() { DialogueData.TunnelCreatures }"))
            { Console.Error.WriteLine("FAIL: factory entry not wrapped in list"); return 1; }
            if (saved.Contains("DialoguePool = DialogueData.TunnelCreatures,"))
            { Console.Error.WriteLine("FAIL: writer emitted invalid shortcut for factory entry"); return 1; }
            Console.WriteLine($"  OK create: {createdRoomId} has [TunnelCreatures] (wrapped form)");

            if (clearedRoomId != null)
            {
                var reCleared = p2.Rooms.FirstOrDefault(r => r.Id == clearedRoomId);
                if (reCleared == null || reCleared.DialoguePool.Count != 0)
                { Console.Error.WriteLine($"FAIL: delete didn't clear pool on {clearedRoomId}"); return 1; }
                Console.WriteLine($"  OK delete: {clearedRoomId} pool cleared");
            }

            Console.WriteLine("DialoguePool CRUD probe PASSED");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine($"probe crashed: {ex}"); return 1; }
        finally
        {
            if (Environment.GetEnvironmentVariable("LC_PROBE_KEEP") == "1")
                Console.WriteLine($"KEEP: {tmp}");
            else try { File.Delete(tmp); } catch { }
        }
    }

    /// One-shot: parse LocationData.cs, write it back via the new always-emit
    /// writer so every room ends up with explicit FriendlyNPCs and DialoguePool
    /// declarations. Operates in place on the given path.
    private static int RunEnsureLocationDefaults(string src)
    {
        if (!File.Exists(src)) { Console.Error.WriteLine($"file not found: {src}"); return 1; }
        var p = new LocationFileParser(src);
        if (!p.TryLoad()) { Console.Error.WriteLine($"parse failed: {p.Error}"); return 1; }
        var rooms = p.Rooms.ToList();
        Console.WriteLine($"loaded {rooms.Count} rooms");
        try
        {
            LocationFileWriter.Save(p, rooms);
            Console.WriteLine($"rewrote {src} — every location now declares FriendlyNPCs + DialoguePool");

            // Re-parse and report how many rooms had each property populated, just
            // as a sanity check the file still parses.
            var p2 = new LocationFileParser(src);
            if (!p2.TryLoad()) { Console.Error.WriteLine($"reparse failed: {p2.Error}"); return 1; }
            var withNpcs = p2.Rooms.Count(r => r.FriendlyNPCs.Count > 0);
            var withPool = p2.Rooms.Count(r => r.DialoguePool.Count > 0);
            Console.WriteLine($"  {withNpcs}/{p2.Rooms.Count} rooms have FriendlyNPCs populated");
            Console.WriteLine($"  {withPool}/{p2.Rooms.Count} rooms have DialoguePool populated");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine($"save failed: {ex.Message}"); return 1; }
    }

    private static int RunDialogueProbe(string src)
    {
        if (!File.Exists(src)) { Console.Error.WriteLine($"file not found: {src}"); return 1; }
        var tmp = Path.Combine(Path.GetTempPath(), $"dialogue_probe_{Guid.NewGuid():N}.cs");
        File.Copy(src, tmp, overwrite: true);
        try
        {
            var p1 = new DialogueFileParser(tmp);
            if (!p1.TryLoad()) { Console.Error.WriteLine($"initial parse failed: {p1.Error}"); return 1; }
            Console.WriteLine($"READ: {p1.Dialogues.Count} dialogues, {p1.Pools.Count} pools");
            if (p1.Dialogues.Count < 2) { Console.Error.WriteLine("need at least 2 dialogues"); return 1; }

            var dialogues = p1.Dialogues.ToList();
            var firstName = dialogues[0].MemberName;
            var lastName  = dialogues[^1].MemberName;
            var beforeLineCount = dialogues[0].Lines.Count;

            // UPDATE: append a new line to the first dialogue.
            dialogues[0].Lines.Add(new DialogueLineModel { Speaker = "playerName", Line = "Probe injected line." });
            // DELETE: remove the last dialogue (writer should also strip it from any pool body).
            dialogues.RemoveAt(dialogues.Count - 1);
            // CREATE: brand-new dialogue.
            dialogues.Add(new DialogueModel
            {
                MemberName = "ProbeTestDialogue",
                Lines = new()
                {
                    new DialogueLineModel { Speaker = "npcName",    Line = "Probe greeting." },
                    new DialogueLineModel { Speaker = "playerName", Line = "Probe response." },
                },
            });

            DialogueFileWriter.Save(p1, dialogues);
            Console.WriteLine("WRITE: saved temp copy");

            var p2 = new DialogueFileParser(tmp);
            if (!p2.TryLoad()) { Console.Error.WriteLine($"reparse failed: {p2.Error}"); return 1; }
            Console.WriteLine($"REREAD: {p2.Dialogues.Count} dialogues, {p2.Pools.Count} pools");

            var firstAfter = p2.Dialogues.FirstOrDefault(d => d.MemberName == firstName);
            if (firstAfter == null) { Console.Error.WriteLine($"FAIL: {firstName} missing"); return 1; }
            if (firstAfter.Lines.Count != beforeLineCount + 1)
            { Console.Error.WriteLine($"FAIL: {firstName} expected {beforeLineCount + 1} lines, got {firstAfter.Lines.Count}"); return 1; }
            if (firstAfter.Lines[^1].Speaker != "playerName" || firstAfter.Lines[^1].Line != "Probe injected line.")
            { Console.Error.WriteLine("FAIL: appended line wrong"); return 1; }
            Console.WriteLine($"  OK update: {firstName} now has {firstAfter.Lines.Count} lines");

            if (p2.Dialogues.Any(d => d.MemberName == lastName))
            { Console.Error.WriteLine($"FAIL: {lastName} should have been deleted"); return 1; }
            // Pool body must have been scrubbed of the deleted name too.
            var saved = File.ReadAllText(tmp);
            foreach (var pool in p2.Pools)
                if (pool.FactoryNames.Contains(lastName))
                { Console.Error.WriteLine($"FAIL: pool {pool.PoolName} still references deleted {lastName}"); return 1; }
            Console.WriteLine($"  OK delete: {lastName} pruned (and removed from pool bodies)");

            var newAfter = p2.Dialogues.FirstOrDefault(d => d.MemberName == "ProbeTestDialogue");
            if (newAfter == null) { Console.Error.WriteLine("FAIL: new dialogue missing"); return 1; }
            if (newAfter.Lines.Count != 2 || newAfter.Lines[0].Speaker != "npcName" || newAfter.Lines[1].Speaker != "playerName")
            { Console.Error.WriteLine("FAIL: new dialogue lines wrong"); return 1; }
            Console.WriteLine($"  OK create: ProbeTestDialogue written with 2 lines");

            Console.WriteLine("Dialogue CRUD probe PASSED");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine($"probe crashed: {ex}"); return 1; }
        finally
        {
            if (Environment.GetEnvironmentVariable("LC_PROBE_KEEP") == "1")
                Console.WriteLine($"KEEP: {tmp}");
            else try { File.Delete(tmp); } catch { }
        }
    }

    private static int RunDialoguePoolFileProbe(string src)
    {
        if (!File.Exists(src)) { Console.Error.WriteLine($"file not found: {src}"); return 1; }
        var tmp = Path.Combine(Path.GetTempPath(), $"dlgpool_probe_{Guid.NewGuid():N}.cs");
        File.Copy(src, tmp, overwrite: true);
        try
        {
            var p1 = new DialogueFileParser(tmp);
            if (!p1.TryLoad()) { Console.Error.WriteLine($"initial parse failed: {p1.Error}"); return 1; }
            Console.WriteLine($"READ: {p1.Pools.Count} pools, {p1.Dialogues.Count} dialogues");
            if (p1.Pools.Count == 0) { Console.Error.WriteLine("no pools to test against"); return 1; }

            var pools = p1.Pools.ToList();
            var firstName = pools[0].PoolName;
            var beforeFirstCount = pools[0].FactoryNames.Count;
            var anyDialogue = p1.Dialogues[0].MemberName;
            var anotherDialogue = p1.Dialogues.Count > 1 ? p1.Dialogues[1].MemberName : anyDialogue;

            // UPDATE: append a member to the first pool.
            pools[0].FactoryNames.Add(anyDialogue);
            // CREATE: brand-new pool referencing two dialogues.
            pools.Add(new DialoguePoolModel
            {
                PoolName = "ProbeTestPool",
                FactoryNames = new() { anyDialogue, anotherDialogue },
            });
            // DELETE: only safe if there's a second pool to remove. We won't
            // drop Default here to keep the file well-formed; the writer's
            // delete logic is exercised via the "remove span" path below if a
            // second pool exists in the original file. Otherwise we skip.
            string? deletedPoolName = null;
            if (pools.Count > 2)
            {
                deletedPoolName = pools[1].PoolName;
                pools.RemoveAt(1);
            }

            DialoguePoolFileWriter.Save(p1, pools);
            Console.WriteLine("WRITE: saved temp copy");

            var p2 = new DialogueFileParser(tmp);
            if (!p2.TryLoad()) { Console.Error.WriteLine($"reparse failed: {p2.Error}"); return 1; }
            Console.WriteLine($"REREAD: {p2.Pools.Count} pools");

            var firstAfter = p2.Pools.FirstOrDefault(x => x.PoolName == firstName);
            if (firstAfter == null) { Console.Error.WriteLine($"FAIL: {firstName} missing"); return 1; }
            if (firstAfter.FactoryNames.Count != beforeFirstCount + 1 || firstAfter.FactoryNames[^1] != anyDialogue)
            { Console.Error.WriteLine($"FAIL: {firstName} update wrong: [{string.Join(",", firstAfter.FactoryNames)}]"); return 1; }
            Console.WriteLine($"  OK update: {firstName} now has {firstAfter.FactoryNames.Count} members");

            var newAfter = p2.Pools.FirstOrDefault(x => x.PoolName == "ProbeTestPool");
            if (newAfter == null) { Console.Error.WriteLine("FAIL: new pool missing"); return 1; }
            if (newAfter.FactoryNames.Count != 2 || newAfter.FactoryNames[0] != anyDialogue || newAfter.FactoryNames[1] != anotherDialogue)
            { Console.Error.WriteLine($"FAIL: new pool wrong: [{string.Join(",", newAfter.FactoryNames)}]"); return 1; }
            Console.WriteLine($"  OK create: ProbeTestPool with [{string.Join(",", newAfter.FactoryNames)}]");

            if (deletedPoolName != null)
            {
                if (p2.Pools.Any(x => x.PoolName == deletedPoolName))
                { Console.Error.WriteLine($"FAIL: {deletedPoolName} should have been deleted"); return 1; }
                Console.WriteLine($"  OK delete: {deletedPoolName} pruned");
            }
            else Console.WriteLine("  (skipped delete: only one pool in source)");

            Console.WriteLine("DialoguePool CRUD probe PASSED");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine($"probe crashed: {ex}"); return 1; }
        finally
        {
            if (Environment.GetEnvironmentVariable("LC_PROBE_KEEP") == "1")
                Console.WriteLine($"KEEP: {tmp}");
            else try { File.Delete(tmp); } catch { }
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
