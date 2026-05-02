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

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
