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
        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
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
