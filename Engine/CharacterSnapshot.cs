using TerminalHyperspace.Models;

namespace TerminalHyperspace.Engine;

public record InventoryEntry(
    string Name,
    bool IsEquipped,
    bool IsMissionItem,
    string? MissionDestination);

public record StandingEntry(Faction Faction, string Label, int Value);

/// Pure-data snapshot of the player's high-level stats, pushed across the
/// GuiBridge whenever state changes so the UI can refresh the overview pane.
public record CharacterSnapshot(
    string Name,
    string Species,
    string Role,
    int Credits,
    int TurnCount,
    int Resolve,
    int MaxResolve,
    int UpgradePoints,
    int ForcePoints,
    IReadOnlyList<InventoryEntry> Inventory,
    IReadOnlyList<StandingEntry> Standings,
    string CurrentLocation
);
