namespace TerminalHyperspace.Models;

public enum MissionType { Escort, Delivery, Sabotage, Recon }
public enum MissionStatus { Active, Completed, Failed }

public class Mission
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string BriefingText { get; set; } = "";
    public MissionType Type { get; set; }
    public string DestinationLocationId { get; set; } = "";

    // Escort: NPC tagged as "with the player" until destination reached.
    public string EscortNpcName { get; set; } = "";

    // Delivery: the item to deliver (must remain in inventory at destination).
    public Item? MissionItem { get; set; }

    // Sabotage / Recon: skill check at destination.
    public SkillType CheckSkill { get; set; }
    public int CheckTargetNumber { get; set; }
    public string CheckSuccessText { get; set; } = "";
    public string CheckFailText { get; set; } = "";

    public int CreditReward { get; set; }
    public int UpgradePointReward { get; set; }

    public MissionStatus Status { get; set; } = MissionStatus.Active;

    public string DestinationName(Dictionary<string, Content.Location>? world)
        => world != null && world.TryGetValue(DestinationLocationId, out var l) ? l.Name : DestinationLocationId;
}
