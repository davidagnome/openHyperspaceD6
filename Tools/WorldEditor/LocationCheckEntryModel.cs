using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// One entry in the `LocationChecks` dictionary in Content/SkillCheckData.cs.
/// Key is a Location id (e.g. "tatooine_espa_cantina"); the value is the list
/// of SkillCheckEvent member-name references that fire at that location.
public class LocationCheckEntryModel
{
    public string LocationId { get; set; } = "";
    public List<string> CheckMemberNames { get; set; } = new();

    /// Span of the original `["id"] = new() { ... },` element inside the
    /// dictionary initializer, used by the writer to detect deletions.
    /// Null for entries created in the editor.
    public TextSpan? OriginalSpan { get; set; }

    public bool IsNew => OriginalSpan == null;
}
