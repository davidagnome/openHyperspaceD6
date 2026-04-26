using TerminalHyperspace.Models;

namespace TerminalHyperspace.Content;

/// Pool of mission offers. The CommandParser picks one at random when an NPC
/// offers work. Each call returns a fresh instance so player state doesn't bleed.
public static partial class MissionData
{
    public static Mission EscortDiplomat() => new()
    {
        Id = "escort_diplomat",
        Title = "Escort Twi'lek Diplomat to Mos Espa Hangar",
        BriefingText = "A nervous Twi'lek diplomat needs safe passage to the Mos Espa private hangar. Imperial agents are looking for them.",
        Type = MissionType.Escort,
        DestinationLocationId = "tatooine_espa_hangar",
        EscortNpcName = "Twi'lek Diplomat",
        CreditReward = 200,
        UpgradePointReward = 1,
    };

    public static Mission EscortInformant() => new()
    {
        Id = "escort_informant",
        Title = "Escort Rebel Informant to the Cantina",
        BriefingText = "A Rebellion informant needs to be moved discreetly to the Bucket of Bolts cantina for a clandestine meeting.",
        Type = MissionType.Escort,
        DestinationLocationId = "tatooine_espa_cantina",
        EscortNpcName = "Rebel Informant",
        CreditReward = 150,
        UpgradePointReward = 1,
    };

    public static Mission DeliverySpiceCargo() => new()
    {
        Id = "delivery_spice",
        Title = "Deliver Sealed Spice Crate to the Hutt Compound",
        BriefingText = "A sealed crate of glitterstim spice must reach Jabba's people in Mos Entha. No questions asked.",
        Type = MissionType.Delivery,
        DestinationLocationId = "tatooine_entha_hutt_compound",
        MissionItem = new Item
        {
            Name = "Sealed Spice Crate",
            Description = "A heavy, locked durasteel crate. Contents tightly regulated.",
            IsMissionItem = true,
            MissionDestinationLocationId = "tatooine_entha_hutt_compound",
            MissionDestinationName = "Mos Entha — Hutt Compound",
        },
        CreditReward = 250,
        UpgradePointReward = 1,
    };

    public static Mission DeliveryDataPad() => new()
    {
        Id = "delivery_datapad",
        Title = "Courier Encrypted DataPad to the Reactor",
        BriefingText = "Hand-deliver an encrypted datapad to a contact who works the reactor core. Don't lose it.",
        Type = MissionType.Delivery,
        DestinationLocationId = "tatooine_espa_reactor",
        MissionItem = new Item
        {
            Name = "Encrypted DataPad",
            Description = "A locked datapad pulsing faintly. Tampering is unwise.",
            IsMissionItem = true,
            MissionDestinationLocationId = "tatooine_espa_reactor",
            MissionDestinationName = "Reactor Core Access",
        },
        CreditReward = 175,
        UpgradePointReward = 1,
    };

    public static Mission SabotageReactor() => new()
    {
        Id = "sabotage_reactor",
        Title = "Sabotage the Imperial Reactor Coolant Lines",
        BriefingText = "The Rebellion needs the Mos Espa reactor knocked offline for an hour. Crack the control terminal and corrupt the coolant feed.",
        Type = MissionType.Sabotage,
        DestinationLocationId = "tatooine_espa_reactor",
        CheckSkill = SkillType.Computers,
        CheckTargetNumber = 16,
        CheckSuccessText = "You spike the coolant routine. By the time anyone notices, you'll be gone.",
        CheckFailText = "An alarm trips. Sirens echo through the reactor halls — you barely make it out.",
        CreditReward = 350,
        UpgradePointReward = 2,
    };

    public static Mission SabotageCommand() => new()
    {
        Id = "sabotage_command",
        Title = "Disable the Imperial Command Center Sensors",
        BriefingText = "Slip into the Imperial command center and brick the long-range sensor array. Armament-side work.",
        Type = MissionType.Sabotage,
        DestinationLocationId = "tatooine_espa_command",
        CheckSkill = SkillType.Armament,
        CheckTargetNumber = 18,
        CheckSuccessText = "You crack the housing and ground out the relay. The array goes dark.",
        CheckFailText = "Your tools slip. Officers turn at the noise.",
        CreditReward = 400,
        UpgradePointReward = 2,
    };

    public static Mission ReconHangar() => new()
    {
        Id = "recon_hangar",
        Title = "Recon the Private Hangar Manifests",
        BriefingText = "Get into the private hangar and quietly read the cargo manifests. We need to know what's coming through.",
        Type = MissionType.Recon,
        DestinationLocationId = "tatooine_espa_hangar",
        CheckSkill = SkillType.Search,
        CheckTargetNumber = 14,
        CheckSuccessText = "You catalog three suspicious cargo entries before slipping back out.",
        CheckFailText = "A guard catches your reflection in a viewport. You leave empty-handed.",
        CreditReward = 220,
        UpgradePointReward = 1,
    };

    public static Mission ReconTunnels() => new()
    {
        Id = "recon_tunnels",
        Title = "Map the Maintenance Tunnel Layout",
        BriefingText = "We need a clean map of the maintenance tunnels under Mos Espa. Take your datapad and survey the layout.",
        Type = MissionType.Recon,
        DestinationLocationId = "tatooine_espa_tunnels",
        CheckSkill = SkillType.Computers,
        CheckTargetNumber = 12,
        CheckSuccessText = "Your datapad sketches a clean topology of the tunnels. The Rebellion will be pleased.",
        CheckFailText = "Interference scrambles your readings. The map is useless.",
        CreditReward = 180,
        UpgradePointReward = 1,
    };

    public static List<Func<Mission>> AllOffers => new()
    {
        EscortDiplomat, EscortInformant,
        DeliverySpiceCargo, DeliveryDataPad,
        SabotageReactor, SabotageCommand,
        ReconHangar, ReconTunnels,
    };
}
