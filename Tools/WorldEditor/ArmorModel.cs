using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Mirrors Models/Armor.cs so the WorldEditor can round-trip
/// Content/ArmorData.cs field declarations like:
///   public static readonly Armor Foo = new() { Name = "...", DiceCode = new DiceCode(N), Price = N };
public class ArmorModel
{
    public string MemberName { get; set; } = "";

    public string Name { get; set; } = "";
    public int Dice { get; set; }
    public int Price { get; set; }
    /// Empty string means "no Climate property in source" (defaults to Climate.Normal).
    public string Climate { get; set; } = "";
    /// Tracks whether this armor is in the Purchasable aggregator list.
    public bool Purchasable { get; set; } = true;

    public TextSpan? OriginalSpan { get; set; }
    public bool IsNew => OriginalSpan == null;

    public override string ToString() => $"{MemberName} — {Name}";
}
