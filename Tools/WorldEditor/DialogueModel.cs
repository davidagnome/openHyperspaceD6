using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// One Speaker/Line pair inside a Dialogue factory. Speaker is stored as the
/// runtime parameter identifier ("npcName" or "playerName") so the editor can
/// round-trip the source declaration without needing string-literal heuristics.
public class DialogueLineModel
{
    public string Speaker { get; set; } = "npcName";
    public string Line { get; set; } = "";
}

/// One `public static Dialogue Foo(string npcName, string playerName) => new() { ... };`
/// factory in DialogueData.cs.
public class DialogueModel
{
    public string MemberName { get; set; } = "";
    public List<DialogueLineModel> Lines { get; set; } = new();

    public TextSpan? OriginalSpan { get; set; }
    public bool IsNew => OriginalSpan == null;

    public override string ToString() => $"{MemberName}  ({Lines.Count} lines)";
}

/// One `public static List<Func<string, string, Dialogue>> Foo => new() { Bar, Baz, … };`
/// pool in DialogueData.cs. Entries reference Dialogue factory member names.
public class DialoguePoolModel
{
    public string PoolName { get; set; } = "";
    public List<string> FactoryNames { get; set; } = new();

    public TextSpan? OriginalSpan { get; set; }
    public bool IsNew => OriginalSpan == null;

    public override string ToString() => $"{PoolName}  ({FactoryNames.Count})";
}
