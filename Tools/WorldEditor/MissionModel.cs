using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

public class MissionModel
{
    public string MemberName { get; set; } = "";

    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string BriefingText { get; set; } = "";
    public string Type { get; set; } = "Escort";  // MissionType: Escort/Delivery/Sabotage/Recon
    public string DestinationLocationId { get; set; } = "";
    public string EscortNpcName { get; set; } = "";

    // Mission item — inline `new Item { ... }`. Captured but rendered verbatim
    // when present rather than parsed deeply (V1).
    public bool HasMissionItem { get; set; }
    public string MissionItemName { get; set; } = "";
    public string MissionItemDescription { get; set; } = "";
    public string MissionItemDestinationLocationId { get; set; } = "";
    public string MissionItemDestinationName { get; set; } = "";

    public string CheckSkill { get; set; } = "";
    public int CheckTargetNumber { get; set; }
    public string CheckSuccessText { get; set; } = "";
    public string CheckFailText { get; set; } = "";

    public int CreditReward { get; set; }
    public int UpgradePointReward { get; set; }

    public TextSpan? OriginalSpan { get; set; }
    public bool IsNew => OriginalSpan == null;

    public override string ToString() => $"{MemberName} [{Type}] — {Title}";
}
