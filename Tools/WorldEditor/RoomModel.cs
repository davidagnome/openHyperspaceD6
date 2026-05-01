using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Mutable in-memory representation of a single Location entry. Mirrors the
/// public Location class fields. SourceSpan tracks the original source range
/// so save can replace the exact statement; null for newly-added rooms.
public class RoomModel
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsSpace { get; set; }
    public bool IsSystemSpace { get; set; }
    public bool RequiresVehicle { get; set; }
    public bool HasShop { get; set; }
    public bool HasVehicleShop { get; set; }
    public string Climate { get; set; } = "Normal";
    public int HyperspaceX { get; set; }
    public int HyperspaceY { get; set; }
    public string PlanetName { get; set; } = "";
    public string StarSystemName { get; set; } = "";
    public string SectorName { get; set; } = "";
    public string TerritoryName { get; set; } = "";
    public double EncounterChance { get; set; } = 0.3;
    public double SpaceEncounterChance { get; set; }

    public List<KeyValueEntry> Exits { get; set; } = new();
    public List<string> AmbientMessages { get; set; } = new();
    public List<string> PossibleEncounters { get; set; } = new();
    public List<string> FriendlyNPCs { get; set; } = new();
    public bool FriendlyNPCsPresent { get; set; }
    public List<string> SpaceEncounters { get; set; } = new();

    /// SourceSpan of the original `world["id"] = new Location { ... };` statement
    /// in LocationData.cs. Null for rooms added in the editor — those get inserted
    /// before the RegisterImported(world); call on save.
    public TextSpan? OriginalSpan { get; set; }

    public bool IsNew => OriginalSpan == null;
}

public class KeyValueEntry
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public KeyValueEntry() { }
    public KeyValueEntry(string k, string v) { Key = k; Value = v; }
}
