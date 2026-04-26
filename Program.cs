using Avalonia;
using TerminalHyperspace.Engine;

namespace TerminalHyperspace;

public static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Contains("--console"))
        {
            GameRunner.Run();
            return 0;
        }

        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
