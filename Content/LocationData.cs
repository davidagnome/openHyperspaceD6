using TerminalHyperspace.Models;

namespace TerminalHyperspace.Content;

public class Location
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsSpace { get; set; }
    public Dictionary<string, string> Exits { get; set; } = new(); // direction -> locationId
    public List<Func<Character>> PossibleEncounters { get; set; } = new();
    public double EncounterChance { get; set; } = 0.3;
    public List<string> AmbientMessages { get; set; } = new();
    public List<Func<Character>>? FriendlyNPCs { get; set; }
    public bool HasShop { get; set; }
    public bool HasVehicleShop { get; set; }
    
    public string PlanetName { get; set; } = "";
    
    public string StarSystemName { get; set; } = "";
    
    public string SectorName { get; set; } = "";
    
    public string TerritoryName { get; set; } = "";
}

public static class LocationData
{
    public static Dictionary<string, Location> BuildWorld()
    {
        var world = new Dictionary<string, Location>();

        world["cantina"] = new Location
        {
            Id = "cantina",
            Name = "Bucket of Bolts Cantina",
            Description = "A dim, seedy cantina on the edge of Mos Espa. The air is thick with smoke from a dozen alien imbibing pipes and drinks. A rodian band plays jizz music -- and not well.",
            Exits = new() { ["north"] = "market", ["east"] = "docking_bay", ["south"] = "alley" },
            PossibleEncounters = new() { NPCData.PirateThugs, NPCData.BountyHunter },
            EncounterChance = 0.2,
            AmbientMessages = new()
            {
                "A Trandoshan arm-wrestler slams her opponent's hand through a blade. Nobody looks up from their business.",
                "The bartender, a spider-droid, pours six drinks simultaneously. Pot-marked blaster fire riddles their chassis.",
                "A freighter pilot in the corner booth cleans a blaster under the table.",
                "The holonews broadcast hums: '...Imperial victory seems imminent over terror cells linked to Saw...'",
            },
            FriendlyNPCs = new() { NPCData.Merchant },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories"
        };

        world["market"] = new Location
        {
            Id = "market",
            Name = "Nexus Station Market District",
            Description = "A bustling open-air bazaar crammed between durasteel bulkheads. Vendors hawk everything from reprocessed ration packs to suspiciously pristine military hardware. Holographic signs flicker in a dozen languages.",
            Exits = new() { ["south"] = "cantina", ["east"] = "hangar", ["north"] = "upper_district" },
            PossibleEncounters = new() { NPCData.PirateThugs },
            EncounterChance = 0.15,
            HasShop = true,
            AmbientMessages = new()
            {
                "A vendor demonstrates a 'genuine' lightsaber replica that promptly catches fire.",
                "Two droids argue about the fair market value of a motivator unit.",
                "A child darts between stalls, pocketing something shiny.",
                "The scent of roasting meat mixes with ozone and engine grease.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories"
        };

        world["docking_bay"] = new Location
        {
            Id = "docking_bay",
            Name = "Docking Bay 7",
            Description = "A cavernous docking bay where ships of all sizes rest on scarred landing pads. Fuel lines snake across the floor like mechanical vines. Maintenance droids whir and beep as they tend to battered hulls.",
            Exits = new() { ["west"] = "cantina", ["north"] = "hangar", ["up"] = "orbit" },
            PossibleEncounters = new() { NPCData.Stormtrooper, NPCData.BountyHunter },
            EncounterChance = 0.25,
            HasVehicleShop = true,
            AmbientMessages = new()
            {
                "A freighter's engines cough to life, filling the bay with a deafening roar.",
                "An Imperial customs droid scans cargo containers near pad 3.",
                "Sparks rain down from a welder working on a ship's belly high above.",
                "A pilot kicks their ship's landing strut and swears in a language you don't recognize.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories"
        };

        world["alley"] = new Location
        {
            Id = "alley",
            Name = "Underbelly Alley",
            Description = "A narrow, trash-strewn corridor beneath the station's main level. Flickering lights cast long shadows between dripping pipes. This is where deals go wrong and people go missing.",
            Exits = new() { ["north"] = "cantina", ["east"] = "tunnels" },
            PossibleEncounters = new() { NPCData.PirateThugs, NPCData.PirateThugs, NPCData.BountyHunter },
            EncounterChance = 0.45,
            AmbientMessages = new()
            {
                "Something skitters in the darkness ahead. Probably just a rat. Probably.",
                "Graffiti on the wall reads: 'THE EMPIRE SEES ALL' — someone crossed out 'EMPIRE' and wrote 'NOTHING'.",
                "A broken surveillance droid sparks feebly in a puddle of coolant.",
                "You hear muffled shouting from behind a sealed blast door.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories"
        };

        world["tunnels"] = new Location
        {
            Id = "tunnels",
            Name = "Maintenance Tunnels",
            Description = "A labyrinth of service tunnels running through the station's infrastructure. The air is stale and warm. Pipes hiss with steam and the distant rumble of machinery never stops.",
            Exits = new() { ["west"] = "alley", ["down"] = "reactor" },
            PossibleEncounters = new() { NPCData.CreatureSmall, NPCData.CreatureSmall, NPCData.CreatureLarge },
            EncounterChance = 0.5,
            AmbientMessages = new()
            {
                "The floor vibrates rhythmically—the station's reactor pulse.",
                "A burst pipe sends a jet of steam across the corridor.",
                "Claw marks score the metal walls. Something lives down here.",
                "Your footsteps echo endlessly in the cramped passages.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories"
        };

        world["reactor"] = new Location
        {
            Id = "reactor",
            Name = "Reactor Core Access",
            Description = "The massive reactor core of Nexus Station hums with terrifying power behind reinforced transparisteel barriers. Warning signs in every language plaster the walls. The air crackles with residual energy.",
            Exits = new() { ["up"] = "tunnels" },
            PossibleEncounters = new() { NPCData.DarkAdept, NPCData.Stormtrooper },
            EncounterChance = 0.6,
            AmbientMessages = new()
            {
                "The reactor's glow pulses like a heartbeat, casting everything in blue-white light.",
                "Your hair stands on end from the ambient static charge.",
                "A warning klaxon sounds briefly, then stops. Probably routine.",
                "The Force feels... turbulent here. Like standing in a river's current.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories"
        };

        world["hangar"] = new Location
        {
            Id = "hangar",
            Name = "Private Hangar Bay",
            Description = "A smaller, more exclusive hangar reserved for those with credits or connections. Ships here are sleeker, better maintained. Armed guards watch the entrances.",
            Exits = new() { ["south"] = "docking_bay", ["west"] = "market", ["up"] = "orbit" },
            PossibleEncounters = new() { NPCData.ImperialOfficer, NPCData.BountyHunter },
            EncounterChance = 0.2,
            HasVehicleShop = true,
            AmbientMessages = new()
            {
                "A sleek yacht powers down its engines, its hull still glowing from atmospheric entry.",
                "A pair of guards check credentials at the main entrance.",
                "A protocol droid fusses over cargo loading arrangements.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories"
        };

        world["upper_district"] = new Location
        {
            Id = "upper_district",
            Name = "Upper District",
            Description = "The polished upper level of Nexus Station, where the wealthy and powerful conduct their affairs. Clean corridors, functioning lights, and the faint scent of something almost floral. The contrast with below is stark.",
            Exits = new() { ["south"] = "market", ["east"] = "command" },
            PossibleEncounters = new() { NPCData.ImperialOfficer },
            EncounterChance = 0.15,
            AmbientMessages = new()
            {
                "Well-dressed beings stride purposefully past, datapads in hand.",
                "A holographic fountain projects cascading water in the central atrium.",
                "Security cameras track your movement. You don't quite fit in up here.",
            },
            FriendlyNPCs = new() { NPCData.Merchant },
            HasShop = true,
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories"
        };

        world["command"] = new Location
        {
            Id = "command",
            Name = "Station Command Center",
            Description = "The nerve center of Nexus Station. Massive viewscreens display system-wide sensor data, shipping routes, and Imperial patrol patterns. Officers and technicians bustle between consoles.",
            Exits = new() { ["west"] = "upper_district" },
            PossibleEncounters = new() { NPCData.ImperialOfficer, NPCData.Stormtrooper, NPCData.Stormtrooper },
            EncounterChance = 0.35,
            AmbientMessages = new()
            {
                "An officer barks orders at a subordinate about docking clearances.",
                "Sensor data scrolls across a wall-sized display—dozens of ships in the sector.",
                "A priority transmission chimes. Everyone tenses for a moment.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories"
        };

        world["orbit"] = new Location
        {
            Id = "orbit",
            Name = "Nexus Station Orbit",
            Description = "The cold void of space stretches endlessly around you. Nexus Station hangs below, a glittering web of lights against the darkness. Ships drift in and out of traffic lanes. The nearest star paints everything in pale gold.",
            IsSpace = true,
            Exits = new() { ["dock"] = "docking_bay", ["land"] = "hangar", ["jump"] = "deep_space" },
            PossibleEncounters = new() { NPCData.BountyHunter },
            EncounterChance = 0.2,
            AmbientMessages = new()
            {
                "A patrol frigate glides past, its running lights blinking methodically.",
                "A burst of static on the comm—someone's distress signal, quickly silenced.",
                "The station's defense turrets track a passing freighter, then stand down.",
            },
            PlanetName = "Tatooine System Space",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories"
        };

        world["deep_space"] = new Location
        {
            Id = "deep_space",
            Name = "Deep Space — The Rift Expanse",
            Description = "You've jumped to the edge of the Rift Expanse, a treacherous region of space filled with asteroid fields and navigational hazards. Sensors flicker with ghost readings. Out here, you're on your own.",
            IsSpace = true,
            Exits = new() { ["jump"] = "orbit", ["explore"] = "derelict" },
            PossibleEncounters = new() { NPCData.BountyHunter, NPCData.PirateThugs },
            EncounterChance = 0.4,
            AmbientMessages = new()
            {
                "An asteroid tumbles past your viewport, close enough to see the mineral veins.",
                "Your sensors ping something metallic in the debris field ahead.",
                "The hyperlane beacon here is damaged—its signal stutters and fades.",
                "Silence. The oppressive, crushing silence of deep space.",
            },
            PlanetName = "Deep Space near Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories"
        };

        world["derelict"] = new Location
        {
            Id = "derelict",
            Name = "Derelict Station Omega",
            Description = "An abandoned research station drifts in the Rift, its hull scarred by weapons fire and micrometeorites. Emergency lights cast a blood-red glow through shattered viewports. Something went very wrong here.",
            IsSpace = true,
            Exits = new() { ["leave"] = "deep_space", ["board"] = "derelict_interior" },
            PossibleEncounters = new() { NPCData.DarkAdept },
            EncounterChance = 0.3,
            AmbientMessages = new()
            {
                "The station rotates slowly, revealing blast marks across its midsection.",
                "A faint power signature emanates from deep within. Someone—or something—is still here.",
                "Debris from the station forms a halo of wreckage around it.",
            },
        };

        world["derelict_interior"] = new Location
        {
            Id = "derelict_interior",
            Name = "Derelict Station Omega — Interior",
            Description = "The interior is a tomb. Frozen atmosphere crystals drift in zero-G pockets. Consoles flicker with corrupted data. The walls are scored with something that looks disturbingly like claw marks—but far too large for any creature you know.",
            Exits = new() { ["airlock"] = "derelict" },
            PossibleEncounters = new() { NPCData.DarkAdept, NPCData.CreatureLarge },
            EncounterChance = 0.55,
            AmbientMessages = new()
            {
                "A console sparks to life, displays 'CONTAINMENT BREACH — LEVEL 7', then dies.",
                "You find a datapad. The last entry reads: 'They came from the rift itself...'",
                "The Force roils here like a storm. Something dark lingers in these walls.",
                "A distant metallic clang echoes through the corridors. You are not alone.",
            },
        };

        // Initialize NPC resolve for encounters
        return world;
    }
}
