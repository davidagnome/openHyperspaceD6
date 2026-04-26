using ClosedXML.Excel;

namespace TerminalHyperspace.Importer;

/// Reads sheets into List&lt;Row&gt;, where Row is a case-insensitive header → cell-text dict.
/// Skips example rows whose Id matches existing shipped content.
public class ImportContext
{
    public List<Row> NPCs { get; }
    public List<Row> Locations { get; }
    public List<Row> Items { get; }
    public List<Row> Vehicles { get; }
    public List<Row> Species { get; }
    public List<Row> Roles { get; }
    public List<Row> SkillChecks { get; }
    public List<Row> SpaceEncounters { get; }
    public List<Row> Armor { get; }
    public List<Row> Shields { get; }

    private static readonly HashSet<string> ShippedNpcs = new(StringComparer.OrdinalIgnoreCase)
    {
        "Stormtrooper","Snowtrooper","PirateThugs","BountyHunter","ImperialOfficer",
        "DarkAdept","Merchant","CreatureSmall","TuskenRaider","BobaFett","Diagnoga"
    };
    private static readonly HashSet<string> ShippedItems = new(StringComparer.OrdinalIgnoreCase)
    {
        "BlasterPistol","HeavyBlaster","BlasterRifle","Vibroblade","VibroAxe",
        "ForcePike","ThermalDetonator","ConcussionGrenades","Medpack","DataPad","Macrobinoculars"
    };
    private static readonly HashSet<string> ShippedVehicles = new(StringComparer.OrdinalIgnoreCase)
    {
        "LightFreighter","Starfighter","PatrolCruiser","Speeder","ArmoredTransport","CombatSpeeder"
    };
    private static readonly HashSet<string> ShippedArmor = new(StringComparer.OrdinalIgnoreCase)
    {
        "Unarmored","PaddedFlightSuit","LightArmor","MediumArmor","HeavyArmor",
        "BattleArmor","ThermalSuit","InsulatedSuit","AquaticSuit"
    };
    private static readonly HashSet<string> ShippedShields = new(StringComparer.OrdinalIgnoreCase)
    {
        "Unshielded","CivilianShields","ReconShields","FighterShields","BomberShields","CapitalShields"
    };
    private static readonly HashSet<string> ShippedSpaceEncounters = new(StringComparer.OrdinalIgnoreCase)
    {
        "PirateInterceptor","ImperialPatrol","BountyHunterShip","SmugglerFreighter","ImperialGunboat"
    };
    private static readonly HashSet<string> ShippedSpecies = new(StringComparer.OrdinalIgnoreCase)
    {
        "Human","Bothan","Mon Calamari","Trandoshan","Synthoid","Rodian","Zabrak","Wookie","Green-Ones"
    };
    private static readonly HashSet<string> ShippedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Soldier","Pilot","Doctor","Scoundrel","Engineer","Mystic","Bounty Hunter","Politician"
    };
    private static readonly HashSet<string> ShippedLocations = new(StringComparer.OrdinalIgnoreCase)
    {
        "tatooine_espa_cantina","tatooine_espa_market","tatooine_espa_docking_bay","tatooine_espa_alley",
        "tatooine_espa_tunnels","tatooine_espa_reactor","tatooine_espa_hangar","tatooine_espa_upper_district",
        "tatooine_espa_command","tatooine_orbit","deep_space","derelict","derelict_interior",
        "tatooine_mos_entha","tatooine_entha_hutt_compound","beggars_canyon","tatooine_mospic_high_range",
        "tatooine_western_great_mesra","tatooine_jabba_palace_entrance","tatooine_jabba_palace_throne",
        "tatooine_jabba_palace_underworks","tatooine_jabba_palace_rancor_pit",
        "rodia_orbit","nar_shadaa_orbit","coruscant_orbit"
    };
    // Built-in skill checks share Ids that aren't worth duplicating; the template's example
    // Ids are listed here so authors can leave them in place without producing dupes.
    private static readonly HashSet<string> ShippedSkillCheckIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "cantina_lockbox","cantina_sabacc","cantina_drunk_diplomat","cantina_pickpocket","cantina_rumors"
    };

    public ImportContext(XLWorkbook wb)
    {
        NPCs            = ReadSheet(wb, "NPCs",            "Id",       skip: ShippedNpcs);
        Locations       = ReadSheet(wb, "Locations",       "Id",       skip: ShippedLocations);
        Items           = ReadSheet(wb, "Items",           "Id",       skip: ShippedItems);
        Vehicles        = ReadSheet(wb, "Vehicles",        "Id",       skip: ShippedVehicles);
        Species         = ReadSheet(wb, "Species",         "Name",     skip: ShippedSpecies);
        Roles           = ReadSheet(wb, "Roles",           "Name",     skip: ShippedRoles);
        SkillChecks     = ReadSheet(wb, "SkillChecks",     "Id",       skip: ShippedSkillCheckIds);
        SpaceEncounters = ReadSheet(wb, "SpaceEncounters", "Id",       skip: ShippedSpaceEncounters);

        // Armor sheet has two banded sections separated by a SHIELDS header row
        (Armor, Shields) = ReadArmorAndShields(wb, ShippedArmor, ShippedShields);
    }

    private static List<Row> ReadSheet(XLWorkbook wb, string sheetName, string idColumn, HashSet<string>? skip = null)
    {
        if (!wb.TryGetWorksheet(sheetName, out var ws)) return new();
        return ParseTable(ws, ws.FirstRowUsed()?.RowNumber() ?? 1, idColumn, skip);
    }

    private static (List<Row> armor, List<Row> shields) ReadArmorAndShields(
        XLWorkbook wb, HashSet<string> shippedArmor, HashSet<string> shippedShields)
    {
        var armor = new List<Row>();
        var shields = new List<Row>();
        if (!wb.TryGetWorksheet("Armor", out var ws)) return (armor, shields);

        int? armorHeaderRow = null, shieldHeaderRow = null;
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
        for (int r = 1; r <= lastRow; r++)
        {
            var a1 = ws.Cell(r, 1).GetString().Trim();
            if (a1.Equals("Id*", StringComparison.OrdinalIgnoreCase) || a1.Equals("Id", StringComparison.OrdinalIgnoreCase))
            {
                if (armorHeaderRow == null) armorHeaderRow = r;
                else { shieldHeaderRow = r; break; }
            }
        }

        if (armorHeaderRow != null)
        {
            int endRow = shieldHeaderRow.HasValue ? shieldHeaderRow.Value - 1 : lastRow;
            armor = ParseTable(ws, armorHeaderRow.Value, "Id", shippedArmor, endRow);
        }
        if (shieldHeaderRow != null)
        {
            shields = ParseTable(ws, shieldHeaderRow.Value, "Id", shippedShields);
        }
        return (armor, shields);
    }

    private static List<Row> ParseTable(IXLWorksheet ws, int headerRow, string idColumn,
                                         HashSet<string>? skip = null, int? maxRow = null)
    {
        var headers = new Dictionary<int, string>();
        for (int c = 1; c <= ws.LastColumnUsed()!.ColumnNumber(); c++)
        {
            var raw = ws.Cell(headerRow, c).GetString().Trim().TrimEnd('*');
            if (!string.IsNullOrEmpty(raw)) headers[c] = raw;
        }
        if (!headers.Values.Any(h => h.Equals(idColumn, StringComparison.OrdinalIgnoreCase)))
            return new();

        int idCol = headers.First(kv => kv.Value.Equals(idColumn, StringComparison.OrdinalIgnoreCase)).Key;
        int last = maxRow ?? ws.LastRowUsed()!.RowNumber();
        var rows = new List<Row>();
        for (int r = headerRow + 1; r <= last; r++)
        {
            var idVal = ws.Cell(r, idCol).GetString().Trim();
            if (string.IsNullOrEmpty(idVal)) continue;
            if (!IsValidId(idVal)) continue; // skip section banners / malformed rows
            if (skip != null && skip.Contains(idVal)) continue;
            var row = new Row();
            foreach (var (col, header) in headers)
                row[header] = ws.Cell(r, col).GetString().Trim();
            rows.Add(row);
        }
        return rows;
    }

    /// Accepts identifiers, snake_case, hyphens, and spaces (Species/Roles use display-name as id).
    private static bool IsValidId(string s)
    {
        foreach (var c in s)
            if (!(char.IsLetterOrDigit(c) || c is '_' or '-' or ' '))
                return false;
        return s.Length > 0 && (char.IsLetter(s[0]) || s[0] == '_');
    }
}

public sealed class Row : Dictionary<string, string>
{
    public Row() : base(StringComparer.OrdinalIgnoreCase) { }
    public string Get(string key) => TryGetValue(key, out var v) ? v : "";
}
