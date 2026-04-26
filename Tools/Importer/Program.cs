using System.Globalization;
using System.Text;
using ClosedXML.Excel;

namespace TerminalHyperspace.Importer;

public static class Program
{
    public static int Main(string[] args)
    {
        string? inPath = null, outDir = null;
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--in" when i + 1 < args.Length: inPath = args[++i]; break;
                case "--out" when i + 1 < args.Length: outDir = args[++i]; break;
                case "-h" or "--help":
                    PrintUsage(); return 0;
                default:
                    if (inPath == null) inPath = args[i];
                    else if (outDir == null) outDir = args[i];
                    break;
            }
        }

        if (inPath == null)
        {
            PrintUsage();
            return 1;
        }
        outDir ??= ResolveDefaultOutDir(inPath);

        try
        {
            using var wb = new XLWorkbook(inPath);
            Directory.CreateDirectory(outDir);
            var ctx = new ImportContext(wb);
            int total = new Emitter(ctx, outDir).EmitAll();
            Console.WriteLine($"Imported {total} entries → {outDir}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Import failed: {ex.Message}");
            return 2;
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("th-import — import Terminal Hyperspace content from xlsx");
        Console.WriteLine();
        Console.WriteLine("Usage:  th-import <ContentTemplate.xlsx> [<output-dir>]");
        Console.WriteLine("        th-import --in <xlsx> --out <dir>");
        Console.WriteLine();
        Console.WriteLine("Default output: <repo-root>/Content/Generated");
    }

    private static string ResolveDefaultOutDir(string inPath)
    {
        var probe = Path.GetDirectoryName(Path.GetFullPath(inPath))!;
        for (int i = 0; i < 6 && probe != null; i++)
        {
            var content = Path.Combine(probe, "Content");
            if (Directory.Exists(content)) return Path.Combine(content, "Generated");
            probe = Path.GetDirectoryName(probe);
        }
        return Path.Combine(Environment.CurrentDirectory, "Content", "Generated");
    }
}
