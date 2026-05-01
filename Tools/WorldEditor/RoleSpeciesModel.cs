using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Shared model for Role and Species. Both live as inline `new T { ... }`
/// elements inside the `All` property's collection initializer.
public class RoleSpeciesModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public Dictionary<string, (int Dice, int Pips)> AttributeBonuses { get; set; } = new();
    public Dictionary<string, (int Dice, int Pips)> SkillBonuses { get; set; } = new();

    public TextSpan? OriginalSpan { get; set; }
    public bool IsNew => OriginalSpan == null;

    public override string ToString() => Name;
}
