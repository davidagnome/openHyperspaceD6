using TerminalHyperspace.Content;

namespace TerminalHyperspace.Engine;

/// BFS over compass exits within the same planet, assigning grid coordinates.
public static class MapBuilder
{
    private static readonly Dictionary<string, (int dx, int dy)> Compass = new()
    {
        ["north"]     = (0, -1), ["south"]     = (0,  1),
        ["east"]      = (1,  0), ["west"]      = (-1, 0),
        ["northeast"] = (1, -1), ["northwest"] = (-1, -1),
        ["southeast"] = (1,  1), ["southwest"] = (-1, 1),
        ["ne"] = (1, -1), ["nw"] = (-1, -1), ["se"] = (1, 1), ["sw"] = (-1, 1),
    };

    public static MapSnapshot? Build(GameState state)
    {
        var origin = state.CurrentLocation;
        var planet = origin.PlanetName;
        if (string.IsNullOrEmpty(planet)) return null;

        var sameWorld = state.World.Values
            .Where(l => l.PlanetName == planet)
            .ToDictionary(l => l.Id);

        var coords = new Dictionary<string, (int x, int y)> { [origin.Id] = (0, 0) };
        var queue = new Queue<string>();
        queue.Enqueue(origin.Id);

        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            if (!sameWorld.TryGetValue(id, out var loc)) continue;
            var (x, y) = coords[id];
            foreach (var (dir, dest) in loc.Exits)
            {
                if (!Compass.TryGetValue(dir.ToLowerInvariant(), out var d)) continue;
                if (!sameWorld.ContainsKey(dest)) continue;
                if (coords.ContainsKey(dest)) continue;
                coords[dest] = (x + d.dx, y + d.dy);
                queue.Enqueue(dest);
            }
        }

        var rooms = coords
            .Select(kv =>
            {
                var loc = sameWorld[kv.Key];
                return new MapRoom(
                    Id: loc.Id,
                    Name: loc.Name,
                    X: kv.Value.x,
                    Y: kv.Value.y,
                    Visited: state.VisitedLocations.Contains(loc.Id),
                    IsCurrent: loc.Id == state.CurrentLocationId);
            })
            .ToList();

        var edges = new List<MapEdge>();
        var seen = new HashSet<(string, string)>();
        foreach (var loc in coords.Keys.Select(id => sameWorld[id]))
        {
            foreach (var (dirRaw, destId) in loc.Exits)
            {
                var dir = dirRaw.ToLowerInvariant();
                if (!Compass.ContainsKey(dir)) continue;
                if (!coords.ContainsKey(destId)) continue;
                var key = string.CompareOrdinal(loc.Id, destId) < 0
                    ? (loc.Id, destId) : (destId, loc.Id);
                if (!seen.Add(key)) continue;
                edges.Add(new MapEdge(loc.Id, destId, dir));
            }
        }

        var orphans = sameWorld.Values
            .Where(l => !coords.ContainsKey(l.Id))
            .Select(l => l.Name)
            .OrderBy(n => n)
            .ToList();

        var nonCompass = origin.Exits
            .Where(kv => !Compass.ContainsKey(kv.Key.ToLowerInvariant()))
            .Select(kv => (kv.Key, state.World.TryGetValue(kv.Value, out var l) ? l.Name : kv.Value))
            .ToList();

        return new MapSnapshot(planet, rooms, edges, orphans, nonCompass);
    }
}
