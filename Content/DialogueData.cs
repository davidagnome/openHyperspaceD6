namespace TerminalHyperspace.Content;

/// <summary>
/// One conversation between an NPC and the player. Speakers are interleaved —
/// the renderer in CommandParser.Talk() emits each line in order.
/// </summary>
public class Dialogue
{
    public List<DialogueLine> Lines { get; set; } = new();
}

public class DialogueLine
{
    public string Speaker { get; set; } = "";
    public string Line { get; set; } = "";
}

/// <summary>
/// Library of canned NPC dialogues. Each factory takes the live NPC name and
/// player name so a single template can be reused across encounters and the
/// generated DialogueLine objects don't need to look up runtime state.
///
/// Locations reference these factories from their per-location DialoguePool,
/// letting different rooms (cantinas, marketplaces, hangars, etc.) offer
/// thematically-appropriate flavor.
/// </summary>
public static class DialogueData
{

    public static Dialogue WorkOpportunity(string npcName, string playerName) => new()
    {
        Lines = new()
        {
            new() { Speaker = npcName, Line = "You look like someone who can handle themselves. Interested in work?" },
            new() { Speaker = playerName, Line = "Depends on the pay." },
            new() { Speaker = npcName, Line = "Smart answer. The Outer Sectors are crawling with opportunity... and danger." },
        },
    };

    public static Dialogue ImperialPatrols(string npcName, string playerName) => new()
    {
        Lines = new()
        {
            new() { Speaker = npcName, Line = "Imperial patrols have been getting bolder. Bad for business, if you catch my meaning." },
            new() { Speaker = playerName, Line = "I've noticed." },
            new() { Speaker = npcName, Line = "Word of advice—keep your head down in the Upper District. Eyes everywhere up there." },
        },
    };

    public static Dialogue DerelictStationRumor(string npcName, string playerName) => new()
    {
        Lines = new()
        {
            new() { Speaker = npcName, Line = "I've got goods from a dozen systems. What are you looking for?" },
            new() { Speaker = playerName, Line = "Information, mostly." },
            new() { Speaker = npcName, Line = "That's the most expensive commodity in the galaxy, friend. But I like your face. There's a derelict station out in the Rift Expanse. Salvagers keep disappearing near it." },
        },
    };

    public static Dialogue TunnelCreatures(string npcName, string playerName) => new()
    {
        Lines = new()
        {
            new() { Speaker = npcName, Line = "Careful down in the tunnels. Something's been breeding down there." },
            new() { Speaker = playerName, Line = "What kind of something?" },
            new() { Speaker = npcName, Line = "The kind that doesn't show up on sensors until it's already too close." },
        },
    };

    public static Dialogue HangarShipForSale(string npcName, string playerName) => new()
    {
        Lines = new()
        {
            new() { Speaker = npcName, Line = "You a pilot? I've got a lead on a ship for sale in the hangar. Previous owner... won't be needing it anymore." },
            new() { Speaker = playerName, Line = "What happened to the previous owner?" },
            new() { Speaker = npcName, Line = "Let's just say he made the jump to hyperspace without a ship. Gambling debts are ugly business." },
        },
    };

    public static Dialogue NarShadaaaBar(string npcName, string playerName) => new()
    {
        Lines = new()
        {
            new() { Speaker = npcName, Line = "Care for a drink?" },
            new() { Speaker = playerName, Line = "Like what?" },
            new() { Speaker = npcName, Line = "I recommend the Newt Fireray. A classic from the Republic era and a weak, industrial solvent." },
        },
    };

    public static Dialogue NarShadaaBarMusic(string npcName, string playerName) => new()
    {
        Lines = new()
        {
            new() { Speaker = playerName, Line = "Can you change the music on the holo player?" },
            new() { Speaker = npcName, Line = "Read the sign." },
            new() { Speaker = playerName, Line = "Paying customers only. Got it." },
        },
    };

    public static List<Func<string, string, Dialogue>> Default => new()
    {
        WorkOpportunity,
        ImperialPatrols,
        DerelictStationRumor,
        TunnelCreatures,
        HangarShipForSale,
    };

    public static List<Func<string, string, Dialogue>> NarShaddaRimmersRest => new()
    {
        WorkOpportunity,
        NarShadaaaBar,
        ImperialPatrols,
    };

        public static List<Func<string, string, Dialogue>> TatooineMosEspa => new()
        {
            DerelictStationRumor,
            HangarShipForSale,
            ImperialPatrols,
            TunnelCreatures,
            WorkOpportunity,
        };
}
