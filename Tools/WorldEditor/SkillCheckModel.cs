using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Mirrors a Content/SkillCheckEvent. Each entry in SkillCheckData.cs is a
/// public static SkillCheckEvent property with an arrow-expression initializer.
public class SkillCheckModel
{
    public string MemberName { get; set; } = "";

    public string Id { get; set; } = "";
    public string Description { get; set; } = "";
    public string SuccessText { get; set; } = "";
    public string FailText { get; set; } = "";
    public string FailPenaltyText { get; set; } = "";
    public string Skill { get; set; } = "";
    public string Difficulty { get; set; } = "Moderate";
    public int TargetNumber { get; set; }
    public int CreditReward { get; set; }
    public int UpgradePointReward { get; set; }
    public bool Repeatable { get; set; }
    public int CreditPenalty { get; set; }
    public string CombatNpcOnFail { get; set; } = ""; // member name in NPCData

    public TextSpan? OriginalSpan { get; set; }
    public bool IsNew => OriginalSpan == null;

    public override string ToString() => $"{MemberName} ({Skill}/{Difficulty})";
}
