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
        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
