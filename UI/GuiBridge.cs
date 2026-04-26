using System.Collections.Concurrent;

namespace TerminalHyperspace.UI;

/// Bridges the synchronous game loop running on a background thread to the
/// Avalonia GUI on the main thread. When set, Terminal routes all output here
/// instead of Console, and ReadLine blocks on a queue fed by the input box.
public class GuiBridge
{
    public static GuiBridge? Instance { get; set; }

    public event Action<string, ConsoleColor, bool>? OnWrite;
    public event Action? OnClear;
    public event Action<TerminalHyperspace.Engine.MapSnapshot>? OnRenderMap;

    public void RenderMap(TerminalHyperspace.Engine.MapSnapshot snapshot)
        => OnRenderMap?.Invoke(snapshot);

    private readonly BlockingCollection<string> _input = new(new ConcurrentQueue<string>());

    public void Write(string text, ConsoleColor color, bool newLine)
        => OnWrite?.Invoke(text, color, newLine);

    public void Clear() => OnClear?.Invoke();

    public string ReadLine() => _input.Take();

    public void SubmitInput(string s) => _input.Add(s);
}
