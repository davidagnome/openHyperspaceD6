namespace TerminalHyperspace.Engine;

public record MapRoom(string Id, string Name, int X, int Y, bool Visited, bool IsCurrent);
public record MapEdge(string FromId, string ToId, string Direction);

/// Pure-data snapshot of a planet's map. The engine builds it; the UI paints it.
/// Kept separate from any UI types so it travels cleanly across the GuiBridge.
public record MapSnapshot(
    string Planet,
    IReadOnlyList<MapRoom> Rooms,
    IReadOnlyList<MapEdge> Edges,
    IReadOnlyList<string> OrphanNames,
    IReadOnlyList<(string Dir, string DestName)> NonCompassExits);
