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

    // ===========================================================
    // BESTINE / JUNDLAND BRANCH
    // ===========================================================
    public static Mission EscortFugitiveToOldBen() => new()
    {
        Id = "escort_oldben",
        Title = "Escort a Fugitive to the Hermit's Bluff",
        BriefingText = "A frightened young woman needs to reach the hermit at the edge of the Jundland. She won't say why. She has paid in advance, in pre-Imperial credits.",
        Type = MissionType.Escort,
        DestinationLocationId = "tatooine_old_ben_residence",
        EscortNpcName = "Frightened Fugitive",
        CreditReward = 350,
        UpgradePointReward = 2,
    };

    public static Mission DeliveryJawaParts() => new()
    {
        Id = "delivery_jawaparts",
        Title = "Courier a Crate of Salvage to the Jawa Camp",
        BriefingText = "A Bestine merchant needs a sealed crate of refurbished motivators delivered to the Jawa traders in the northern wastes. They'll pay double for hand-delivery.",
        Type = MissionType.Delivery,
        DestinationLocationId = "tatooine_northern_jawa_territories",
        MissionItem = new Item
        {
            Name = "Crate of Refurbished Motivators",
            Description = "A heavy durasteel crate. The contents whistle faintly when shaken.",
            IsMissionItem = true,
            MissionDestinationLocationId = "tatooine_northern_jawa_territories",
            MissionDestinationName = "Northern Jawa Territories",
        },
        CreditReward = 280,
        UpgradePointReward = 1,
    };

    public static Mission SabotageBestineGarrison() => new()
    {
        Id = "sabotage_bestine_garrison",
        Title = "Disable the Bestine Garrison's Walker Bay",
        BriefingText = "A Rebel cell needs the AT-ST maintenance bay knocked offline for forty-eight hours. Slip in, weld a few coolant lines wrong, slip out.",
        Type = MissionType.Sabotage,
        DestinationLocationId = "tatooine_bestine_garrison",
        CheckSkill = SkillType.Armament,
        CheckTargetNumber = 18,
        CheckSuccessText = "You re-route the coolant return through a heat-soaked manifold. By the time the techs find the fault, the bay is dark for two days.",
        CheckFailText = "Sparks fly from the wrong relay. Klaxons sound and you barely make it back to the gate.",
        CreditReward = 450,
        UpgradePointReward = 3,
    };

    public static Mission ReconJundlandTuskenPaths() => new()
    {
        Id = "recon_jundland_paths",
        Title = "Map the Tusken Patrol Routes in Central Jundland",
        BriefingText = "An offworld survey team needs a clean map of the Tusken patrol patterns through the central Jundland. Survive long enough to chart them.",
        Type = MissionType.Recon,
        DestinationLocationId = "tatooine_judland_wasteland_central",
        CheckSkill = SkillType.Survival,
        CheckTargetNumber = 15,
        CheckSuccessText = "Three days, three patrols, one clear pattern. Your map sells for twice the agreed rate.",
        CheckFailText = "You lose the trail in a slot canyon and emerge with sketchy notes the buyers reject.",
        CreditReward = 220,
        UpgradePointReward = 1,
    };

    public static Mission DeliveryHermitJournal() => new()
    {
        Id = "delivery_hermit_journal",
        Title = "Return a Recovered Journal to the Hermit's Bluff",
        BriefingText = "An old hand-stitched journal surfaced in a Bestine pawnshop. A whispered contact wants it returned to the hermit's residence — quietly.",
        Type = MissionType.Delivery,
        DestinationLocationId = "tatooine_old_ben_residence",
        MissionItem = new Item
        {
            Name = "Hand-Stitched Journal",
            Description = "Cracked leather binding, archaic Aurebesh on the spine. Heavier than it looks.",
            IsMissionItem = true,
            MissionDestinationLocationId = "tatooine_old_ben_residence",
            MissionDestinationName = "Old Ben's Residence",
        },
        CreditReward = 320,
        UpgradePointReward = 2,
    };

    public static List<Func<Mission>> AllOffers => new()
    {
        EscortDiplomat, EscortInformant,
        DeliverySpiceCargo, DeliveryDataPad,
        SabotageReactor, SabotageCommand,
        ReconHangar, ReconTunnels,
        EscortFugitiveToOldBen, DeliveryJawaParts,
        SabotageBestineGarrison, ReconJundlandTuskenPaths,
        DeliveryHermitJournal,
    };
}
