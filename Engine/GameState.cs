using TerminalHyperspace.Content;
using TerminalHyperspace.Models;

namespace TerminalHyperspace.Engine;

public class GameState
{
    public Character Player { get; set; } = new();
    public string CurrentLocationId { get; set; } = "tatooine_espa_cantina";
    public Dictionary<string, Location> World { get; set; } = new();
    public int TurnCount { get; set; }
    public bool GameOver { get; set; }
    public int CreditsBalance { get; set; } = 100;
    public HashSet<string> VisitedLocations { get; set; } = new();
    public HashSet<string> ClearedRooms { get; set; } = new();
    public int EnemiesDefeated { get; set; }
    public int UpgradePoints { get; set; }
    public int ForcePoints { get; set; }
    public HashSet<string> CompletedChecks { get; set; } = new();
    public List<Mission> Missions { get; set; } = new();
    public HashSet<string> OfferedMissionIds { get; set; } = new();

    public Location CurrentLocation => World[CurrentLocationId];

    public void Initialize()
    {
        World = LocationData.BuildWorld();
        TurnCount = 0;
        GameOver = false;
    }
}
