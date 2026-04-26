using System.Globalization;

namespace TerminalHyperspace.Importer;

/// Parses the conventional flat-text formats from the template.
public static class Parsers
{
    public static bool ParseBool(string s)
        => bool.TryParse(s.Trim(), out var b) && b;

    public static double ParseDouble(string s, double fallback = 0.0)
        => double.TryParse(s.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : fallback;

    public static int ParseInt(string s, int fallback = 0)
        => int.TryParse(s.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var i) ? i : fallback;

    /// "3D" → (3,0); "2D+1" → (2,1); "0D+2" → (0,2). Empty → null.
    public static (int dice, int pips)? ParseDice(string s)
    {
        s = s.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(s)) return null;
        int dice = 0, pips = 0;
        var dIdx = s.IndexOf('D');
        if (dIdx < 0) return null;
        if (!int.TryParse(s.AsSpan(0, dIdx), out dice)) return null;
        if (dIdx + 1 < s.Length)
        {
            var rest = s[(dIdx + 1)..];
            if (rest.StartsWith('+'))
            {
                if (!int.TryParse(rest.AsSpan(1), out pips)) return null;
            }
        }
        return (dice, pips);
    }

    /// "a; b; c" → ["a","b","c"]; trims and skips empties.
    public static List<string> SplitSemi(string s)
        => s.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    /// "Blasters:1D+2; Brawl:0D+1" → [(Blasters, 1D+2), (Brawl, 0D+1)]
    public static List<(string Key, string Value)> SplitKeyVal(string s)
    {
        var result = new List<(string, string)>();
        foreach (var item in SplitSemi(s))
        {
            var idx = item.IndexOf(':');
            if (idx < 0) continue;
            var key = item[..idx].Trim();
            var val = item[(idx + 1)..].Trim();
            if (key.Length > 0 && val.Length > 0) result.Add((key, val));
        }
        return result;
    }

    /// "Name|Damage|Skill" → ["Name","Damage","Skill"]
    public static List<string[]> SplitTuples(string s)
        => SplitSemi(s)
            .Select(t => t.Split('|', StringSplitOptions.TrimEntries))
            .ToList();

    /// "18,16" → (18, 16); fallback (0,0)
    public static (int x, int y) ParseCoords(string s)
    {
        var parts = s.Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length < 2) return (0, 0);
        return (ParseInt(parts[0]), ParseInt(parts[1]));
    }

    /// Escape a string for emission as a C# verbatim or interpolated literal.
    public static string Quote(string s)
    {
        if (s.Contains('\n') || s.Contains('"'))
            return "@\"" + s.Replace("\"", "\"\"") + "\"";
        return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }

    public static string DiceLiteral(string s)
    {
        var parsed = ParseDice(s);
        if (parsed == null) return "new DiceCode(0)";
        var (d, p) = parsed.Value;
        return p == 0 ? $"new DiceCode({d})" : $"new DiceCode({d}, {p})";
    }
}
