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
    public List<Func<SpaceEncounter>> SpaceEncounters { get; set; } = new();
    public double SpaceEncounterChance { get; set; } = 0.0;
    
    public string PlanetName { get; set; } = "";
    
    public string StarSystemName { get; set; } = "";
    
    public string SectorName { get; set; } = "";
    
    public string TerritoryName { get; set; } = "";

    /// <summary>When Hot or Cold, the player must be in a vehicle or wearing armor with the corresponding trait.</summary>
    public Climate Climate { get; set; } = Climate.Normal;

    /// <summary>When true, the player must be aboard a vehicle (land or space) to enter.</summary>
    public bool RequiresVehicle { get; set; } = false;

    /// <summary>True when this location represents in-system space (as opposed to deep space / hyperspace).</summary>
    public bool IsSystemSpace { get; set; } = false;

    /// <summary>Galactic hyperspace coordinates for this location, e.g. [0, 0].</summary>
    public int[] HyperspaceCoordinates { get; set; } = new[] { 0, 0 };
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
            Exits = new() { ["north"] = "market", ["east"] = "tatooine_docking_bay", ["south"] = "alley" },
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
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };

        world["market"] = new Location
        {
            Id = "market",
            Name = "Mos Espa Market District",
            Description = "A bustling open-air bazaar crammed between durasteel bulkheads. Vendors hawk everything from reprocessed ration packs to suspiciously pristine military hardware. Holographic signs flicker in a dozen languages.",
            Exits = new() { ["south"] = "cantina", ["east"] = "tatooine_hangar", ["north"] = "upper_district" },
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
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };

        world["tatooine_docking_bay"] = new Location
        {
            Id = "tatooine_docking_bay",
            Name = "Docking Bay 7",
            Description = "A cavernous docking bay where ships of all sizes rest on scarred landing pads. Fuel lines snake across the floor like mechanical vines. Maintenance droids whir and beep as they tend to battered hulls.",
            Exits = new() { ["west"] = "cantina", ["north"] = "tatooine_hangar", ["up"] = "tatooine_orbit" },
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
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };

        world["alley"] = new Location
        {
            Id = "alley",
            Name = "Underbelly Alley",
            Description = "A narrow, trash-strewn corridor beneath the station's main level. Flickering lights cast long shadows between dripping pipes. This is where deals go wrong and people go missing.",
            Exits = new() { ["north"] = "cantina", ["east"] = "tunnels", ["southeast"] = "mos_entha" },
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
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };

        world["tunnels"] = new Location
        {
            Id = "tunnels",
            Name = "Maintenance Tunnels",
            Description = "A labyrinth of service tunnels running through the station's infrastructure. The air is stale and warm. Pipes hiss with steam and the distant rumble of machinery never stops.",
            Exits = new() { ["west"] = "alley", ["down"] = "reactor" },
            PossibleEncounters = new() { NPCData.CreatureSmall, NPCData.CreatureSmall, NPCData.Diagnoga },
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
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };

        world["reactor"] = new Location
        {
            Id = "reactor",
            Name = "Reactor Core Access",
            Description = "The massive reactor core of Mos Espa hums with terrifying power behind reinforced transparisteel barriers. Warning signs in every language plaster the walls. The air crackles with residual energy.",
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
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };

        world["tatooine_hangar"] = new Location
        {
            Id = "tatooine_hangar",
            Name = "Mos Espa Private Hangar Bay",
            Description = "A smaller, more exclusive hangar reserved for those with credits or connections. Ships here are sleeker, better maintained. Armed guards watch the entrances.",
            Exits = new() { ["south"] = "tatooine_docking_bay", ["west"] = "market", ["up"] = "tatooine_orbit", ["east"] = "beggars_canyon" },
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
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };

        world["upper_district"] = new Location
        {
            Id = "upper_district",
            Name = "Mos Espa Upper District",
            Description = "The polished upper level of Mos Espa, where the wealthy and powerful conduct their affairs. Clean corridors, functioning lights, and the faint scent of something almost floral. The contrast with below is stark.",
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
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };

        world["command"] = new Location
        {
            Id = "command",
            Name = "Mos Espa Imperial Station Command Center",
            Description = "The nerve center of the Imperial Regiment at Mos Espa. Massive viewscreens display system-wide sensor data, shipping routes, and Imperial patrol patterns. Officers and technicians bustle between consoles.",
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
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };

        world["tatooine_orbit"] = new Location
        {
            Id = "tatooine_orbit",
            Name = "Tatooine Orbit",
            Description = "The cold void of space stretches endlessly around you. Tatooine, a glittering web of lights against the dark planet surface. Ships drift in and out of traffic lanes. The nearest star paints everything in pale gold.",
            IsSpace = true,
            Exits = new() { ["dock"] = "tatooine_docking_bay", ["land"] = "tatooine_hangar", ["jump"] = "deep_space" },
            PossibleEncounters = new() { NPCData.BountyHunter },
            EncounterChance = 0.2,
            AmbientMessages = new()
            {
                "A patrol frigate glides past, its running lights blinking methodically.",
                "A burst of static on the comm—someone's distress signal, quickly silenced.",
                "The station's defense turrets track a passing freighter, then stand down.",
            },
            SpaceEncounters = new() { SpaceEncounterData.ImperialPatrol, SpaceEncounterData.SmugglerFreighter },
            SpaceEncounterChance = 0.25,
            PlanetName = "Tatooine System Space",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal,
            IsSystemSpace = true,
            HyperspaceCoordinates = [18, 16]
        };

        world["deep_space"] = new Location
        {
            Id = "deep_space",
            Name = "Deep Space — The Rift Expanse",
            Description = "You've jumped to the edge of the Rift Expanse, a treacherous region of space filled with asteroid fields and navigational hazards. Sensors flicker with ghost readings. Out here, you're on your own.",
            IsSpace = true,
            // `jump` is a sentinel — CommandParser intercepts it here and shows a
            // numeric menu of in-system space destinations (IsSpace && IsSystemSpace).
            Exits = new() { ["jump"] = "deep_space", ["explore"] = "derelict" },
            PossibleEncounters = new() { NPCData.BountyHunter, NPCData.PirateThugs },
            EncounterChance = 0.4,
            SpaceEncounters = new() { SpaceEncounterData.PirateInterceptor, SpaceEncounterData.BountyHunterShip, SpaceEncounterData.ImperialGunboat },
            SpaceEncounterChance = 0.45,
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
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };

        world["derelict"] = new Location
        {
            Id = "derelict",
            Name = "Derelict Station Omega - Exterior",
            Description = "An abandoned research station drifts in the Rift, its hull scarred by weapons fire and micrometeorites. Emergency lights cast a blood-red glow through shattered viewports. Something went very wrong here.",
            IsSpace = true,
            Exits = new() { ["leave"] = "deep_space", ["board"] = "derelict_interior" },
            PossibleEncounters = new() { NPCData.DarkAdept },
            EncounterChance = 0.3,
            SpaceEncounters = new() { SpaceEncounterData.PirateInterceptor, SpaceEncounterData.BountyHunterShip },
            SpaceEncounterChance = 0.35,
            AmbientMessages = new()
            {
                "The station rotates slowly, revealing blast marks across its midsection.",
                "A faint power signature emanates from deep within. Someone—or something—is still here.",
                "Debris from the station forms a halo of wreckage around it.",
            },
            Climate = Climate.Normal,
            PlanetName = "Tatooine Orbit",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
        };

        world["derelict_interior"] = new Location
        {
            Id = "derelict_interior",
            Name = "Derelict Station Omega — Interior",
            Description = "The interior is a tomb. The walls are scored with something that looks disturbingly like claw marks—but far too large for any creature you know.",
            Exits = new() { ["airlock"] = "derelict" },
            PossibleEncounters = new() { NPCData.DarkAdept },
            EncounterChance = 0.55,
            AmbientMessages = new()
            {
                "A console sparks to life, displays 'CONTAINMENT BREACH — LEVEL 7', then dies.",
                "You find a datapad. The last entry reads: 'They came from the rift itself...'",
                "The Force roils here like a storm. Something dark lingers in these walls.",
                "A distant metallic clang echoes through the corridors. You are not alone.",
            },
            Climate = Climate.Normal,
            PlanetName = "Tatooine Orbit",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
        };

        world["mos_entha"] = new Location
        {
            Id = "mos_entha",
            Name = "Mos Entha — Outer Streets",
            Description = "The sand-blasted streets of Mos Entha stretch beneath the twin suns, choked with Hutt-faction markings carved into every wall and doorframe. Enforcers in mismatched armor lounge at corners, eyes following every newcomer. This is Jabba Desilijic Tiure's territory—every credit earned here has a cut taken before it reaches your pocket.",
            Exits = new() { ["northwest"] = "alley", ["south"] = "hutt_compound", ["southeast"] = "mospic_high_range" },
            PossibleEncounters = new() { NPCData.PirateThugs, NPCData.PirateThugs, NPCData.BountyHunter },
            EncounterChance = 0.35,
            HasShop = true,
            AmbientMessages = new()
            {
                "A Hutt gang enforcer counts credits behind a barred window, one hand never far from a blaster.",
                "Someone sprayed 'JABBA SEES ALL' across the side of a moisture vaporator. Someone else added 'AND TAKES ALL'.",
                "A nervous-looking Rodian slips down a side passage as you approach.",
                "The smell of spice and charred metal hangs heavy in the afternoon heat.",
                "Two armored heavies settle a dispute with raised voices and a pointed blaster—the losing argument holsters first.",
            },
            FriendlyNPCs = new() { NPCData.Merchant },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Hot
        };

        world["hutt_compound"] = new Location
        {
            Id = "hutt_compound",
            Name = "Mos Entha — Hutt Compound",
            Description = "Beyond the rusted gate lies the inner compound ruled by Jabba's lieutenants. The architecture is bloated and ornate, carved from the same red sandstone as the surrounding desert but gilded with Hutt excess—bronze fixtures, slave-worked mosaics, and the pervasive stench of Hutt musk and contraband spice. Jabba's banner—a bloated slug sigil—flies from every post. Only the bold or the foolish come here uninvited.",
            Exits = new() { ["north"] = "mos_entha" },
            PossibleEncounters = new() { NPCData.PirateThugs, NPCData.BountyHunter, NPCData.BountyHunter, NPCData.ImperialOfficer },
            EncounterChance = 0.5,
            AmbientMessages = new()
            {
                "A protocol droid translates demands from somewhere deep inside, its voice strained.",
                "Caged creatures growl from the shadows beneath an iron-barred alcove.",
                "A lieutenant in Hutt livery lists 'outstanding debts' from a crumpled flimsiplast scroll.",
                "Blaster burns scar the inner walls—someone made a run for it recently. They didn't make it far.",
                "The compound's central pit is sealed with durasteel grating. You decide not to look too closely.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Hot
        };

        world["beggars_canyon"] = new Location
        {
            Id = "beggars_canyon",
            Name = "Beggar's Canyon Entrance",
            Description = "The legendary canyon cuts a jagged scar through the Tatooine badlands east of Mos Espa. Towering sandstone walls streaked rust and amber, riddled with overhangs and narrow chicanes that racers call the 'Stone Needle Run.' The air currents here are treacherous; updrafts appear without warning and the canyon floor is littered with the wreckage of vehicles whose pilots misjudged a turn.",
            Exits = new() { ["west"] = "tatooine_hangar" },
            PossibleEncounters = new() { NPCData.TuskenRaider, NPCData.TuskenRaider, NPCData.CreatureSmall, NPCData.Diagnoga },
            EncounterChance = 0.4,
            RequiresVehicle = true,
            AmbientMessages = new()
            {
                "A gust of wind howls through a gap in the canyon wall, scattering your footing.",
                "Bleached bones of something large lie wedged between two boulders at the canyon floor.",
                "The walls close in overhead—at the narrowest point the sky is just a thin stripe of blue.",
                "Distant Bantha bellows echo off the stone, far too close for comfort.",
                "A rusted T-16 skyhopper frame is half-buried in a sandbank, its markings long since scoured away.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Hot
        };

        world["mospic_high_range"] = new Location
        {
            Id = "mospic_high_range",
            Name = "Mospic High Range",
            Description = "The Mospic High Range looms above the desert floor in broken tiers of wind-carved red-rock plateaus and deep shadowed canyons. Ancient Tusken burial cairns mark every ridge line; the Raiders consider these highlands sacred ground and defend them without mercy. There are no roads here—only goat trails worn by Banthas and the patient footsteps of Sand People who have called this wilderness home for ten thousand years.",
            Exits = new() { ["northwest"] = "mos_entha" },
            PossibleEncounters = new() { NPCData.TuskenRaider, NPCData.TuskenRaider, NPCData.TuskenRaider, NPCData.Diagnoga },
            EncounterChance = 0.55,
            AmbientMessages = new()
            {
                "A Bantha lowing call rolls down from the plateau above—answered, moments later, from below.",
                "You spot movement on a high ridge: a robed silhouette watching you, then gone.",
                "The wind here carries a rhythmic tapping—a Gaderffii stick on stone, unhurried and deliberate.",
                "Sun-bleached skulls have been placed along the trail at intervals. A warning, or a boundary marker.",
                "The canyon walls are covered in ancient Tusken pictographs—hunting scenes, star maps, something that might be a warning.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Hot
        };
        
        world["rodia_orbit"] = new Location
        {
            Id = "rodia_orbit",
            Name = "Rodia Orbit",
            Description = "",
            IsSpace = true,
            Exits = new() { ["rodia_dock"] = "rodia_docking_bay", ["rodia_land"] = "rodia_hangar", ["jump"] = "deep_space" },
            PossibleEncounters = new() { NPCData.BountyHunter },
            EncounterChance = 0.2,
            AmbientMessages = new()
            {
                "A patrol frigate glides past, its running lights blinking methodically.",
                "A burst of static on the comm—someone's distress signal, quickly silenced.",
                "The station's defense turrets track a passing freighter, then stand down.",
            },
            SpaceEncounters = new() { SpaceEncounterData.ImperialPatrol, SpaceEncounterData.SmugglerFreighter },
            SpaceEncounterChance = 0.25,
            PlanetName = "Rodia",
            StarSystemName = "Tyrius System",
            SectorName = "Savareen Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal,
            IsSystemSpace = true,
            HyperspaceCoordinates = [18, 16]
        };
        
        world["nar_shadaa_orbit"] = new Location
        {
            Id = "nar_shadaa_orbit",
            Name = "Orbit",
            Description = "Neon lights flicker across the surface of the Smuggler Moon.",
            IsSpace = true,
            Exits = new() { ["rodia_dock"] = "rodia_docking_bay", ["rodia_land"] = "rodia_hangar", ["jump"] = "deep_space" },
            PossibleEncounters = new() { NPCData.BountyHunter },
            EncounterChance = 0.2,
            AmbientMessages = new()
            {
                "The lights flicker across the surface of the Smuggler Moon.",
                "An illicit transaction is described over open communications channel before a hurried buzz cuts it short.",
                "Nar Shadaa glistens neon across its space lanes and vertical buildings.",
            },
            SpaceEncounters = new() { SpaceEncounterData.ImperialPatrol, SpaceEncounterData.SmugglerFreighter },
            SpaceEncounterChance = 0.25,
            PlanetName = "Nar Shadaa",
            StarSystemName = "Tyrius System",
            SectorName = "Savareen Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal,
            IsSystemSpace = true,
            HyperspaceCoordinates = [18, 16]
        };
        
        world["coruscant_orbit"] = new Location
        {
            Id = "coruscant_orbit",
            Name = "Orbit",
            Description = "Jewel of the Core Worlds the city-planet descends into layers of ruthless Imperial control.",
            IsSpace = true,
            Exits = new() { ["rodia_dock"] = "rodia_docking_bay", ["rodia_land"] = "rodia_hangar", ["jump"] = "deep_space" },
            PossibleEncounters = new() { NPCData.BountyHunter },
            EncounterChance = 0.2,
            AmbientMessages = new()
            {
                "The amber lights of Coruscant's surface resemble a pattern of dense roots.",
                "Imperial checkpoints line the dense and orderly space lanes. No one is getting through -- not with a fleet of Imperial Star Destroyers.",
                "Coruscant: Heart of a Heartless Empire.",
            },
            SpaceEncounters = new() { SpaceEncounterData.ImperialPatrol, SpaceEncounterData.SmugglerFreighter },
            SpaceEncounterChance = 0.25,
            PlanetName = "Coruscant",
            StarSystemName = "Coruscant System",
            SectorName = "Corusca Sector",
            TerritoryName = "Core Territories",
            Climate = Climate.Normal,
            IsSystemSpace = true,
            HyperspaceCoordinates = [19, 12]
        };

        // Note: deep_space uses a numeric `jump` menu (handled in CommandParser)
        // rather than static exits to in-system space locations.

        // Initialize NPC resolve for encounters
        return world;
    }
}
