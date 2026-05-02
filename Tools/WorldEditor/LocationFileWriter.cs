using System.Globalization;
using System.Text;

namespace TerminalHyperspace.WorldEditor;

/// Saves the rooms back to LocationData.cs by replacing each existing room's
/// statement span in the source text and inserting new rooms before the
/// RegisterImported(world); call.
public static class LocationFileWriter
{
    /// Pool-property names from sibling DialogueData.cs. Populated at the start
    /// of Save() and consulted by RenderRoomStatement when deciding whether the
    /// `DialoguePool = DialogueData.X,` shortcut is type-safe.
    private static HashSet<string> _knownDialoguePoolNames = new();

    private static HashSet<string> DiscoverDialoguePoolNames(string locationDataPath)
    {
        try
        {
            var dir = Path.GetDirectoryName(locationDataPath);
            if (string.IsNullOrEmpty(dir)) return new HashSet<string>();
            var dlgPath = Path.Combine(dir, "DialogueData.cs");
            if (!File.Exists(dlgPath)) return new HashSet<string>();
            var dp = new DialogueFileParser(dlgPath);
            if (!dp.TryLoad()) return new HashSet<string>();
            return dp.Pools.Select(p => p.PoolName).ToHashSet();
        }
        catch { return new HashSet<string>(); }
    }

    public static void Save(LocationFileParser parser, IEnumerable<RoomModel> rooms)
    {
        var ordered = rooms.ToList();
        var text = parser.SourceText;

        // Discover which DialogueData.* members are pool *properties* (lists of
        // dialogue factories) vs single Dialogue *factories*. The shortcut form
        // `DialoguePool = DialogueData.X,` only compiles when X is a pool, so we
        // need this distinction to avoid emitting an invalid C# expression for
        // single-factory pools.
        _knownDialoguePoolNames = DiscoverDialoguePoolNames(parser.FilePath);

        // Build a unified edit list across the original source: each edit is
        // either a replacement (existing room kept and edited) or a deletion
        // (existing room removed). Both reference original spans, so we apply
        // them back-to-front in one pass to keep span positions valid.
        var keptSpans = new HashSet<Microsoft.CodeAnalysis.Text.TextSpan>(
            ordered.Where(r => r.OriginalSpan.HasValue).Select(r => r.OriginalSpan!.Value));

        var edits = new List<(Microsoft.CodeAnalysis.Text.TextSpan span, string replacement)>();
        foreach (var room in ordered.Where(r => r.OriginalSpan.HasValue))
        {
            var span = room.OriginalSpan!.Value;
            var newBlock = RenderRoomStatement(room, leadingWhitespace: ExtractLeadingWhitespace(text, span));
            edits.Add((span, newBlock));
        }
        foreach (var orig in parser.Rooms.Where(r => r.OriginalSpan.HasValue && !keptSpans.Contains(r.OriginalSpan!.Value)))
            edits.Add((orig.OriginalSpan!.Value, ""));

        foreach (var edit in edits.OrderByDescending(e => e.span.Start))
            text = text.Substring(0, edit.span.Start) + edit.replacement + text.Substring(edit.span.End);

        // Third pass: insert new rooms before the RegisterImported(world); call.
        // After deletes, the parser.RegisterImportedCallSpan may have shifted, so
        // re-locate it in the current text.
        var anchor = text.IndexOf("RegisterImported(world);", StringComparison.Ordinal);
        if (anchor < 0) anchor = text.LastIndexOf("return world;", StringComparison.Ordinal);
        if (anchor < 0)
            throw new InvalidOperationException("Could not locate RegisterImported(world) anchor in LocationData.cs");

        // Walk back to the start of that line so insertion lands above it.
        var lineStart = text.LastIndexOf('\n', anchor) + 1;
        var indent = ExtractLeadingWhitespace(text, lineStart);

        var sb = new StringBuilder();
        foreach (var room in ordered.Where(r => r.IsNew))
        {
            sb.Append(RenderRoomStatement(room, indent));
            sb.Append('\n');
        }

        if (sb.Length > 0)
            text = text.Substring(0, lineStart) + sb.ToString() + text.Substring(lineStart);

        File.WriteAllText(parser.FilePath, text);
    }

    private static string ExtractLeadingWhitespace(string source, int start)
    {
        var sb = new StringBuilder();
        for (int i = start; i < source.Length && (source[i] == ' ' || source[i] == '\t'); i++)
            sb.Append(source[i]);
        return sb.Length > 0 ? sb.ToString() : "        ";
    }

    private static string ExtractLeadingWhitespace(string source, Microsoft.CodeAnalysis.Text.TextSpan span)
        => ExtractLeadingWhitespace(source, span.Start);

    /// Renders a single `world["id"] = new Location { ... };` block. The whole
    /// block is indented by `leadingWhitespace`; inner properties get +4 spaces.
    public static string RenderRoomStatement(RoomModel r, string leadingWhitespace = "        ")
    {
        var inner = leadingWhitespace + "    ";
        var sb = new StringBuilder();

        // Recreate the leading newline + indent that the original FullSpan included.
        if (r.IsNew) sb.Append('\n');
        else sb.Append(LeadingNewline(leadingWhitespace));
        sb.Append(leadingWhitespace);
        sb.Append("world[\"").Append(Escape(r.Id)).Append("\"] = new Location\n");
        sb.Append(leadingWhitespace).Append("{\n");

        sb.Append(inner).Append("Id = \"").Append(Escape(r.Id)).Append("\",\n");
        sb.Append(inner).Append("Name = \"").Append(Escape(r.Name)).Append("\",\n");
        if (!string.IsNullOrEmpty(r.Description))
            sb.Append(inner).Append("Description = ").Append(Quote(r.Description)).Append(",\n");

        if (r.IsSpace)         sb.Append(inner).Append("IsSpace = true,\n");
        if (r.IsSystemSpace)   sb.Append(inner).Append("IsSystemSpace = true,\n");
        if (r.RequiresVehicle) sb.Append(inner).Append("RequiresVehicle = true,\n");

        if (r.Exits.Count > 0)
        {
            sb.Append(inner).Append("Exits = new()\n").Append(inner).Append("{\n");
            foreach (var e in r.Exits)
                sb.Append(inner).Append("    [\"").Append(Escape(e.Key)).Append("\"] = \"").Append(Escape(e.Value)).Append("\",\n");
            sb.Append(inner).Append("},\n");
        }

        if (r.PossibleEncounters.Count > 0)
        {
            sb.Append(inner).Append("PossibleEncounters = new() { ")
              .Append(string.Join(", ", r.PossibleEncounters.Select(n => $"NPCData.{n}")))
              .Append(" },\n");
        }

        sb.Append(inner).Append("EncounterChance = ")
          .Append(r.EncounterChance.ToString("0.0##", CultureInfo.InvariantCulture)).Append(",\n");

        if (r.HasShop)        sb.Append(inner).Append("HasShop = true,\n");
        if (r.HasVehicleShop) sb.Append(inner).Append("HasVehicleShop = true,\n");

        if (r.AmbientMessages.Count > 0)
        {
            sb.Append(inner).Append("AmbientMessages = new()\n").Append(inner).Append("{\n");
            foreach (var msg in r.AmbientMessages)
                sb.Append(inner).Append("    ").Append(Quote(msg)).Append(",\n");
            sb.Append(inner).Append("},\n");
        }

        // Always emit FriendlyNPCs so every location explicitly declares one.
        if (r.FriendlyNPCs.Count > 0)
            sb.Append(inner).Append("FriendlyNPCs = new() { ")
              .Append(string.Join(", ", r.FriendlyNPCs.Select(n => $"NPCData.{n}")))
              .Append(" },\n");
        else
            sb.Append(inner).Append("FriendlyNPCs = new(),\n");

        if (r.SpaceEncounters.Count > 0)
        {
            sb.Append(inner).Append("SpaceEncounters = new() { ")
              .Append(string.Join(", ", r.SpaceEncounters.Select(n => $"SpaceEncounterData.{n}")))
              .Append(" },\n");
            sb.Append(inner).Append("SpaceEncounterChance = ")
              .Append(r.SpaceEncounterChance.ToString("0.0##", CultureInfo.InvariantCulture)).Append(",\n");
        }

        // Always emit DialoguePool so every location explicitly declares one.
        // The `DialoguePool = DialogueData.X,` shortcut is only legal when X is
        // a pool property (List<Func<string, string, Dialogue>>); for single
        // Dialogue factories we must wrap in `new() { ... }`.
        if (r.DialoguePool.Count == 1 && _knownDialoguePoolNames.Contains(r.DialoguePool[0]))
            sb.Append(inner).Append("DialoguePool = DialogueData.").Append(r.DialoguePool[0]).Append(",\n");
        else if (r.DialoguePool.Count > 0)
            sb.Append(inner).Append("DialoguePool = new() { ")
              .Append(string.Join(", ", r.DialoguePool.Select(n => $"DialogueData.{n}")))
              .Append(" },\n");
        else
            sb.Append(inner).Append("DialoguePool = new(),\n");

        if (!string.IsNullOrEmpty(r.PlanetName))     sb.Append(inner).Append("PlanetName = \"").Append(Escape(r.PlanetName)).Append("\",\n");
        if (!string.IsNullOrEmpty(r.StarSystemName)) sb.Append(inner).Append("StarSystemName = \"").Append(Escape(r.StarSystemName)).Append("\",\n");
        if (!string.IsNullOrEmpty(r.SectorName))     sb.Append(inner).Append("SectorName = \"").Append(Escape(r.SectorName)).Append("\",\n");
        if (!string.IsNullOrEmpty(r.TerritoryName))  sb.Append(inner).Append("TerritoryName = \"").Append(Escape(r.TerritoryName)).Append("\",\n");

        sb.Append(inner).Append("Climate = Climate.").Append(string.IsNullOrEmpty(r.Climate) ? "Normal" : r.Climate).Append(",\n");

        if (r.HyperspaceX != 0 || r.HyperspaceY != 0)
            sb.Append(inner).Append("HyperspaceCoordinates = new[] { ")
              .Append(r.HyperspaceX).Append(", ").Append(r.HyperspaceY).Append(" },\n");

        sb.Append(leadingWhitespace).Append("};\n");

        return sb.ToString();
    }

    private static string LeadingNewline(string indent)
        // FullSpan of the original statement starts with a leading newline; preserve that.
        => "\n";

    private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string Quote(string s)
    {
        if (s.Contains('\n') || s.Contains('\r'))
            return "@\"" + s.Replace("\"", "\"\"") + "\"";
        return "\"" + Escape(s) + "\"";
    }
}
