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

public static partial class LocationData
{
    static partial void RegisterImported(Dictionary<string, Location> world);

    public static Dictionary<string, Location> BuildWorld()
    {
        var world = new Dictionary<string, Location>();

        world["tatooine_espa_cantina"] = new Location
        {
            Id = "tatooine_espa_cantina",
            Name = "Mos Espa - Bucket of Bolts Cantina",
            Description = "A dim, seedy cantina on the edge of Mos Espa. The air is thick with smoke from a dozen alien imbibing pipes and drinks. A rodian band plays jizz music -- and not well.",
            Exits = new() { ["north"] = "tatooine_espa_market", ["east"] = "tatooine_espa_docking_bay", ["south"] = "tatooine_espa_alley" },
            PossibleEncounters = new() { NPCData.PirateThugs, NPCData.BountyHunter },
            EncounterChance = 0.2,
            AmbientMessages = new()
            {
                "A Trandoshan arm-wrestler slams her opponent's hand through a blade. Nobody looks up from their business.",
                "The bartender, a spider-droid, pours six drinks simultaneously. Pot-marked blaster fire riddles their chassis.",
                "A freighter pilot in the corner booth cleans a blaster under the table.",
                "The holonews broadcast hums: '...Imperial victory seems imminent over terror cells linked to Saw...'",
                "A drunk Duros argues loudly with a serving droid about the definition of 'top shelf'.",
                "Someone in a hooded cloak stares at you too long from the far booth. You look back and they're gone.",
                "A sabacc dealer deals a round of cards, shuffling with three hands.",
                "Two mercenaries compare blaster burn scars and laugh like old friends.",
                "The house band lurches into a Twi'lek love ballad. Three patrons groan audibly.",
            },
            FriendlyNPCs = new() { NPCData.Merchant },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };

        world["tatooine_espa_market"] = new Location
        {
            Id = "tatooine_espa_market",
            Name = "Mos Espa Market District",
            Description = "A bustling open-air bazaar crammed between durasteel bulkheads. Vendors hawk everything from reprocessed ration packs to suspiciously pristine military hardware. Holographic signs flicker in a dozen languages.",
            Exits = new() { ["south"] = "tatooine_espa_cantina", ["east"] = "tatooine_espa_hangar", ["north"] = "tatooine_espa_upper_district" },
            PossibleEncounters = new() { NPCData.PirateThugs },
            EncounterChance = 0.15,
            HasShop = true,
            AmbientMessages = new()
            {
                "A vendor demonstrates a 'genuine' lightsaber replica that promptly catches fire.",
                "Two droids argue about the fair market value of a motivator unit.",
                "A child darts between stalls, pocketing something shiny.",
                "The scent of roasting meat mixes with ozone and engine grease.",
                "A Jawa auction heats up over a dented but functional protocol droid head.",
                "An Imperial patrol drone buzzes through at head height, scanning faces.",
                "A spice merchant lowers his voice and makes eye contact with anyone slowing down.",
                "Two Aqualish shopkeepers shout at each other over a boundary of a shared awning.",
                "A Gamorrean haggles badly over the price of a used rifle.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };

        world["tatooine_espa_docking_bay"] = new Location
        {
            Id = "tatooine_espa_docking_bay",
            Name = "Mos Espa Docking Bay 7",
            Description = "A cavernous docking bay where ships of all sizes rest on scarred landing pads. Fuel lines snake across the floor like mechanical vines. Maintenance droids whir and beep as they tend to battered hulls.",
            Exits = new() { ["west"] = "tatooine_espa_cantina", ["north"] = "tatooine_espa_hangar", ["up"] = "tatooine_orbit" },
            PossibleEncounters = new() { NPCData.Stormtrooper, NPCData.BountyHunter },
            EncounterChance = 0.25,
            HasVehicleShop = true,
            AmbientMessages = new()
            {
                "A freighter's engines sputter to life, filling the bay with a deafening roar and thick, black exhaust.",
                "An Imperial customs droid scans cargo containers near Docking Bay 3.",
                "Sparks rain down from a welder working on a ship's communications array high above.",
                "A pilot kicks their ship's landing strut and swears in a language you don't recognize.",
                "A loading crew hustles a strapped-down mystery crate into a Corellian freighter.",
                "Pad 5's repulsor grid hiccups; a ship bobs six meters before the grid catches.",
                "A dockmaster yells into a comlink: 'I don't CARE whose cousin owns the Cantina!'",
                "A refueling droid hisses steam and refuses to connect to a non-standard fuel port. It swats at an eager repair bot.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };

        world["tatooine_espa_alley"] = new Location
        {
            Id = "tatooine_espa_alley",
            Name = "Mos Espa - Underbelly Alley",
            Description = "A narrow, trash-strewn corridor beneath the Mos Espa main level. Flickering lights cast long shadows between dripping pipes. This is where deals go wrong and people go missing.",
            Exits = new() { ["north"] = "tatooine_espa_cantina", ["east"] = "tatooine_espa_tunnels", ["southeast"] = "tatooine_mos_entha" },
            PossibleEncounters = new() { NPCData.PirateThugs, NPCData.PirateThugs, NPCData.BountyHunter },
            EncounterChance = 0.45,
            AmbientMessages = new()
            {
                "Something skitters in the darkness ahead. Probably just a rat. Probably.",
                "A propaganda poster on the wall reads: 'THE EMPIRE SEES ALL' — someone crossed out 'ALL' and wrote 'NOTHING'.",
                "A broken security droid sparks feebly in a puddle of coolant.",
                "You hear muffled shouting from behind a sealed blast door.",
                "A bloodstain on the floor hasn't quite dried. You step around it.",
                "Someone's jury-rigged a comm relay to the ceiling using data cables and desperation.",
                "A glow-rod flickers, casting manic shadows that seem to move on their own.",
                "You pass a torn-open credit pouch. Whoever owned it is nowhere to be seen.",
                "A narrow set of eyes watches you from behind a vent grate. You feel something pass by your foot.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };

        world["tatooine_espa_tunnels"] = new Location
        {
            Id = "tatooine_espa_tunnels",
            Name = "Mos Espa - Maintenance Tunnels",
            Description = "A labyrinth of service tunnels running through the station's infrastructure. The air is stale and warm. Pipes hiss with steam and the distant rumble of machinery never stops.",
            Exits = new() { ["west"] = "tatooine_espa_alley", ["down"] = "tatooine_espa_reactor" },
            PossibleEncounters = new() { NPCData.CreatureSmall, NPCData.CreatureSmall, NPCData.Diagnoga },
            EncounterChance = 0.5,
            AmbientMessages = new()
            {
                "The floor vibrates as the central exhaust from homes dumps hot air into the tunnel.",
                "A burst pipe sends a jet of steam across the corridor.",
                "Claw marks score the metal walls. Something lives down here.",
                "Your footsteps echo endlessly in the cramped passages.",
                "A faded warning sign hangs crooked: CAUTION — LEVEL 4 BREACH RISK.",
                "Condensation drips from the ceiling in a steady, maddening beat.",
                "A colony of mycellium spores glows faintly blue along one wall.",
                "Distant machinery squeals and then shudders to silence.",
                "You find a rotted ration bar wrapper from Siennar Space Industries. It's *not* half bad... it's *all* bad.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };

        world["tatooine_espa_reactor"] = new Location
        {
            Id = "tatooine_espa_reactor",
            Name = "Mos Espa - Reactor Core Access",
            Description = "The massive reactor core of Mos Espa hums with terrifying power behind reinforced transparisteel barriers. Warning signs in every language plaster the walls. The air crackles with residual energy.",
            Exits = new() { ["up"] = "tatooine_espa_tunnels" },
            PossibleEncounters = new() { NPCData.DarkAdept, NPCData.Stormtrooper },
            EncounterChance = 0.6,
            AmbientMessages = new()
            {
                "The reactor's glow pulses like a heartbeat, casting everything in blue-white light.",
                "Your hair stands on end from the ambient static charge.",
                "A warning klaxon sounds briefly, then stops. Probably routine.",
                "The Force feels... turbulent here. Like standing in a river's current.",
                "Coolant lines hiss in a harmonic chord that unsettles your teeth.",
                "Technicians in shielded suits move silently past, ignoring you entirely.",
                "Your datapad momentarily displays static before resetting.",
                "A containment field shudders visibly for a heartbeat — then steadies.",
                "The air tastes faintly of ozone and something metallic you can't name.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };

        world["tatooine_espa_hangar"] = new Location
        {
            Id = "tatooine_espa_hangar",
            Name = "Mos Espa - Private Hangar Bay",
            Description = "A smaller, more exclusive hangar reserved for those with credits or connections. Ships here are sleeker, better maintained. Armed guards watch the entrances.",
            Exits = new() { ["south"] = "tatooine_espa_docking_bay", ["west"] = "tatooine_espa_market", ["up"] = "tatooine_orbit", ["east"] = "beggars_canyon" },
            PossibleEncounters = new() { NPCData.ImperialOfficer, NPCData.BountyHunter },
            EncounterChance = 0.2,
            HasVehicleShop = true,
            AmbientMessages = new()
            {
                "A sleek yacht powers down its engines, its hull still glowing from atmospheric entry.",
                "A pair of guards check credentials at the main entrance.",
                "A protocol droid preens over cargo loading arrangements.",
                "A private security squad in matching armor runs a drill in the far bay.",
                "A mechanic fine-tunes a modified swoop engine, the whine rising and falling.",
                "A cargo manifest argument ends with a datapad shattered against the floor.",
                "Overhead, a crane lowers a sleek starfighter onto a polished landing pad.",
                "A bounty hunter in Beskar pauldrons leans against their ship and watches the crowd.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };

        world["tatooine_espa_upper_district"] = new Location
        {
            Id = "tatooine_espa_upper_district",
            Name = "Mos Espa Upper District",
            Description = "The polished upper level of Mos Espa, where the wealthy and powerful conduct their affairs. Clean corridors, functioning lights, and the faint scent of something almost floral. The contrast with below is stark.",
            Exits = new() { ["south"] = "tatooine_espa_market", ["east"] = "tatooine_espa_command" },
            PossibleEncounters = new() { NPCData.ImperialOfficer },
            EncounterChance = 0.15,
            AmbientMessages = new()
            {
                "Well-dressed beings stride purposefully past, datapads in hand.",
                "A holographic fountain projects cascading water in the central atrium.",
                "Security cameras track your movement. You don't quite fit in up here.",
                "A string quartet of droids plays chamber music in a gilded alcove.",
                "Imperial officers in dress uniforms pass a bottle of aged Corellian brandy.",
                "A protocol droid politely but firmly asks you to step off the carpeted walkway.",
                "A noble's perfume drifts past — it probably cost more than your ship.",
                "Two diplomats argue in measured tones about trade tariffs. Their bodyguards look bored.",
                "Two kids talk about the upcoming pod-race. The adults have money on Sebulba's big comeback.",
            },
            FriendlyNPCs = new() { NPCData.Merchant },
            HasShop = true,
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };

        world["tatooine_espa_command"] = new Location
        {
            Id = "tatooine_espa_command",
            Name = "Mos Espa Imperial Station Command Center",
            Description = "The nerve center of the Imperial Regiment at Mos Espa. Massive viewscreens display system-wide sensor data, shipping routes, and Imperial patrol patterns. Officers and technicians bustle between consoles.",
            Exits = new() { ["west"] = "tatooine_espa_upper_district" },
            PossibleEncounters = new() { NPCData.ImperialOfficer, NPCData.Stormtrooper, NPCData.Stormtrooper },
            EncounterChance = 0.35,
            AmbientMessages = new()
            {
                "An officer barks orders at a subordinate about docking clearances.",
                "Sensor data scrolls across a wall-sized display—dozens of ships in the sector.",
                "A priority transmission chimes. Everyone tenses for a moment.",
                "A comms technician mutters 'That's not a rebel channel, that's just interference' for the third time.",
                "Two Stormtroopers stand at perfect parade rest by the entrance.",
                "A holographic system map with fluorescent green contours rotates slowly, highlighting patrol routes in red.",
                "A droid server rolls past with hot caf for the officers on duty.",
                "A garrison commander reviews a datapad, frowning deeply.",
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
            Exits = new() { ["dock"] = "tatooine_espa_docking_bay", ["land"] = "tatooine_espa_hangar", ["jump"] = "deep_space" },
            PossibleEncounters = new() { NPCData.BountyHunter },
            EncounterChance = 0.2,
            AmbientMessages = new()
            {
                "A patrol frigate glides past -- its half kilometer of running lights blinking methodically.",
                "A distress signal cuts in then is quickly silenced.",
                "There are no space stations in Tatooine's direct orbit. They like to keep bribery and spice-running on the ground floor.",
                "Debris from an old engagement drifts past your viewport — pitted, unpainted durasteel. The word Tantive is barely legible.",
                "A mass of traffic blooms on your sensors as several freighter drops out of hyperspace nearby.",
                "Tatooine's twin suns frame the horizon in a sheet of rust-gold glare.",
                "A patrol TIE pair screams past your cockpit in formation.",
                "The comm crackles with the fragment of an illegal broadcast before blaster fire. Static remains.",
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
            Name = "Deep Space — Tatoo Asteroid Belt",
            Description = "You've jumped to the edge of Tatoo's asteroid belt, a treacherous region of space filled with asteroid fields and navigational hazards. Sensors flicker with ghost readings. Out here, you're on your own.",
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
                "A distant supernova remnant colors half the viewport a bruised purple.",
                "Your power cells cycle — an extraneous draw you can't quite trace.",
                "Something large glides past at extreme range. Your sensors can't identify it.",
                "Coded pulses flicker across the comm band, repeating every forty seconds.",
                "A wrecked hull tumbles past, hull plating scorched black by some ancient weapon.",
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
                "A severed docking arm drifts nearby, cables frozen like brittle roots.",
                "Emergency beacons blink red in lonely sequence. No one is coming.",
                "You spot an escape pod frozen against the hull — never launched.",
                "Scorched hull plating spells out a word in some forgotten alphabet.",
                "The station's hab-ring lists drunkenly on a broken axis.",
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
                "A frozen crewmember sits in a chair, hands folded neatly on the console.",
                "Dried blood traces a path from a research lab to nothing. The path simply ends.",
                "Your helmet lamp catches something huge scrawled on the bulkhead: DO NOT OPEN.",
                "The gravity plating flickers underfoot. For a second you drift.",
                "A mess hall holds a dozen uneaten meals, forks still in hands that crumbled to dust.",
            },
            Climate = Climate.Normal,
            PlanetName = "Tatooine Orbit",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
        };

        world["tatooine_mos_entha"] = new Location
        {
            Id = "tatooine_mos_entha",
            Name = "Mos Entha — Outer Streets",
            Description = "The sand-blasted streets of Mos Entha stretch beneath the twin suns, choked with Hutt-faction markings carved into every wall and doorframe. Enforcers in mismatched armor lounge at corners, eyes following every newcomer. This is Jabba Desilijic Tiure's territory—every credit earned here has a cut taken before it reaches your pocket.",
            Exits = new() { ["northwest"] = "tatooine_espa_alley", ["south"] = "tatooine_entha_hutt_compound", ["southeast"] = "tatooine_mospic_high_range" },
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
                "A Bith informant whispers into a comm, eyes darting across the street.",
                "A speeder screeches through the intersection with three pursuers firing blasters after it.",
                "A child sells forged Hutt-faction tattoos for five credits — pressed out of a stolen medical tool.",
                "A slave auctioneer's voice rises above the crowd. You keep moving.",
                "A Gamorrean enforcer eats a nerf leg raw in the shade of an awning, daring anyone to comment.",
            },
            FriendlyNPCs = new() { NPCData.Merchant },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Hot
        };

        world["tatooine_entha_hutt_compound"] = new Location
        {
            Id = "tatooine_entha_hutt_compound",
            Name = "Mos Entha — Hutt Compound",
            Description = "Beyond the rusted gate lies the inner compound ruled by Jabba's lieutenants. The architecture is bloated and ornate, carved from the same red sandstone as the surrounding desert but gilded with Hutt excess—bronze fixtures, slave-worked mosaics, and the pervasive stench of Hutt musk and contraband spice. Jabba's banner—a bloated slug sigil—flies from every post. Only the bold or the foolish come here uninvited.",
            Exits = new() { ["north"] = "tatooine_mos_entha" },
            PossibleEncounters = new() { NPCData.PirateThugs, NPCData.BountyHunter, NPCData.BountyHunter, NPCData.ImperialOfficer },
            EncounterChance = 0.5,
            AmbientMessages = new()
            {
                "A protocol droid translates demands from somewhere deep inside, its voice strained.",
                "Caged creatures growl from the shadows beneath an iron-barred alcove.",
                "A lieutenant in Hutt livery lists 'outstanding debts' from a crumpled flimsiplast scroll.",
                "Blaster burns scar the inner walls—someone made a run for it recently. They didn't make it far.",
                "The compound's central pit is sealed with durasteel grating. You decide not to look too closely.",
                "A Twi'lek dancer moves through the crowd with a practiced smile and a tracking collar.",
                "A Nikto bodyguard polishes an electrostaff, eyes never leaving the throne room's doors.",
                "Somewhere deep in the compound, a bellowing laugh shakes dust from the rafters.",
                "A tally board lists bounty marks in Huttese, names redacted and added in shaky hand.",
                "The air reeks of overripe fruit and something that was once alive.",
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
                "A swoop racer's ghostly slipstream still hangs in the air — someone ran this loop minutes ago.",
                "A womp rat colony scatters from under an overhang at your passing.",
                "A makeshift finish-line flag flaps from a rock spire, faded and shredded by wind.",
                "The canyon echoes back a scream — or the wind playing tricks.",
                "A circle of small offerings — bones, copper wire, dried meat — surrounds a weathered totem.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Hot
        };

        world["tatooine_mospic_high_range"] = new Location
        {
            Id = "tatooine_mospic_high_range",
            Name = "Mospic High Range",
            Description = "The Mospic High Range looms above the desert floor in broken tiers of wind-carved red-rock plateaus and deep shadowed canyons. Ancient Tusken burial cairns mark every ridge line; the Raiders consider these highlands sacred ground and defend them without mercy. There are no roads here—only goat trails worn by Banthas and the patient footsteps of Sand People who have called this wilderness home for ten thousand years.",
            Exits = new() { ["northwest"] = "tatooine_mos_entha", ["south"] = "tatooine_pika_oasis" },
            PossibleEncounters = new() { NPCData.TuskenRaider, NPCData.TuskenRaider },
            EncounterChance = 0.55,
            AmbientMessages = new()
            {
                "Test.",
                "You spot movement on a high ridge: a robed silhouette watching you, then gone.",
                "The wind here carries a rhythmic tapping—a Gaderffii stick on stone, unhurried and deliberate.",
                "Sun-bleached skulls have been placed along the trail at intervals. A warning, or a boundary marker.",
                "The canyon walls are covered in ancient Tusken pictographs: hunting scenes, star maps, something that might be a warning.",
                "An ululating call rises from three directions at once — coordinated, deliberate.",
                "You find a hand-woven trap laid for something larger than you.",
                "A smoke signal rises from a distant plateau. An answer threads up from the far horizon.",
                "The red rock here bears scorch marks too fresh to belong to history.",
                "Shards of a shattered gaderffii stick lie half-buried in sand, as though left behind in retreat.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };
        
        world["tatooine_western_great_mesra"] = new Location
        {
            Id = "tatooine_western_great_mesra",
            Name = "Western Great Mesra Plateau",
            Description = "Rocky plateaus descend into the Northern Dune Sea. Visible to the east across a steep valley in Jabba's Palace.",
            Exits = new() { ["west"] = "tatooine_mospic_high_range", ["east"] = "tatooine_jabba_palace_entrance" },
            PossibleEncounters = new() { NPCData.ImperialOfficer, NPCData.Stormtrooper, NPCData.Stormtrooper },
            EncounterChance = 0.35,
            AmbientMessages = new()
            {
                "An officer barks orders at a subordinate about docking clearances.",
                "Sensor data scrolls across a wall-sized display—dozens of ships in the sector.",
                "A priority transmission chimes. Everyone tenses for a moment.",
                "A comms technician mutters 'That's not a rebel channel, that's just interference' for the third time.",
                "Two Stormtroopers stand at perfect parade rest by the entrance.",
                "A holographic system map rotates slowly, highlighting patrol routes in red.",
                "A droid server rolls past with hot caf for the officers on duty.",
                "A garrison commander reviews a datapad, frowning deeply.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };
        
        world["tatooine_jabba_palace_entrance"] = new Location
        {
            Id = "tatooine_jabba_palace_entrance",
            Name = "Jabba's Palace - Entrance",
            Description = "Rocky plateaus descend into the Northern Dune Sea. Visible to the east across a steep valley in Jabba's Palace.",
            Exits = new() { ["west"] = "tatooine_western_great_mesra", ["up"] = "tatooine_jabba_palace_throne", ["down"] = "tatooine_jabba_palace_underworks" },
            PossibleEncounters = new() { NPCData.ImperialOfficer, NPCData.Stormtrooper, NPCData.Stormtrooper },
            EncounterChance = 0.35,
            AmbientMessages = new()
            {
                "A Bo'marr monk peers at you with its spider-like droid housing for its brain jar.",
                "At sunset, the wide primary tower and its narrow communications tower of the palace look like two droids.",
                "A skiff jets off from the sail barge maintenance depot with Weequay mercenaries squinting at you.",
                "Hard to believe Jabba's palace was a monastery, given the carousing and bedlam occuring there today.",
                "The unmistakable, mushroom-shaped domes of Jabba's palace encompass the entire horizon as you stand outside its doors.",
                "The difficult pathways to get to the upper levels and rancor pit below means one way in and three ways out: dead, alive, or on carbon.",
                "The sand-colored buildings take on red hues in the sunset.",
                "A Gamorrean guard in leather armor and wookie furs, relieves a pair of Klatooinians for the afternoon shift.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };
        
        world["tatooine_jabba_palace_throne"] = new Location
        {
            Id = "tatooine_jabba_palace_throne",
            Name = "Jabba's Palace - Throne Room",
            Description = "Rocky plateaus descend into the Northern Dune Sea. Visible to the east across a steep valley in Jabba's Palace.",
            Exits = new() { ["down"] = "tatooine_jabba_palace_entrance" },
            PossibleEncounters = new() { NPCData.ImperialOfficer, NPCData.Stormtrooper, NPCData.Stormtrooper },
            EncounterChance = 0.35,
            AmbientMessages = new()
            {
                "An officer barks orders at a subordinate about docking clearances.",
                "Sensor data scrolls across a wall-sized display—dozens of ships in the sector.",
                "A priority transmission chimes. Everyone tenses for a moment.",
                "A comms technician mutters 'That's not a rebel channel, that's just interference' for the third time.",
                "Two Stormtroopers stand at perfect parade rest by the entrance.",
                "A holographic system map rotates slowly, highlighting patrol routes in red.",
                "A droid server rolls past with hot caf for the officers on duty.",
                "A garrison commander reviews a datapad, frowning deeply.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };
        
        world["tatooine_jabba_palace_underworks"] = new Location
        {
            Id = "tatooine_jabba_palace_underworks",
            Name = "Jabba Palace - Underworks",
            Description = "Rocky plateaus descend into the Northern Dune Sea. Visible to the east across a steep valley in Jabba's Palace.",
            Exits = new() { ["up"] = "tatooine_jabba_palace_entrance", ["down"] = "tatooine_jabba_rancor_pit"  },
            PossibleEncounters = new() { NPCData.ImperialOfficer, NPCData.Stormtrooper, NPCData.Stormtrooper },
            EncounterChance = 0.35,
            AmbientMessages = new()
            {
                "An officer barks orders at a subordinate about docking clearances.",
                "Sensor data scrolls across a wall-sized display—dozens of ships in the sector.",
                "A priority transmission chimes. Everyone tenses for a moment.",
                "A comms technician mutters 'That's not a rebel channel, that's just interference' for the third time.",
                "Two Stormtroopers stand at perfect parade rest by the entrance.",
                "A holographic system map rotates slowly, highlighting patrol routes in red.",
                "A droid server rolls past with hot caf for the officers on duty.",
                "A garrison commander reviews a datapad, frowning deeply.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
        };
        
        world["tatooine_jabba_palace_rancor_pit"] = new Location
        {
            Id = "tatooine_jabba_palace_rancor_pit",
            Name = "Jabba Palace - Rancor Pit",
            Description = "Rocky plateaus descend into the Northern Dune Sea. Visible to the east across a steep valley in Jabba's Palace.",
            Exits = new() { ["up"] = "tatooine_jabba_palace_underworks" },
            PossibleEncounters = new() { NPCData.ImperialOfficer, NPCData.Stormtrooper, NPCData.Stormtrooper },
            EncounterChance = 0.35,
            AmbientMessages = new()
            {
                "An officer barks orders at a subordinate about docking clearances.",
                "Sensor data scrolls across a wall-sized display—dozens of ships in the sector.",
                "A priority transmission chimes. Everyone tenses for a moment.",
                "A comms technician mutters 'That's not a rebel channel, that's just interference' for the third time.",
                "Two Stormtroopers stand at perfect parade rest by the entrance.",
                "A holographic system map rotates slowly, highlighting patrol routes in red.",
                "A droid server rolls past with hot caf for the officers on duty.",
                "A garrison commander reviews a datapad, frowning deeply.",
            },
            PlanetName = "Tatooine",
            StarSystemName = "Tatoo System",
            SectorName = "Arkanis Sector",
            TerritoryName = "Outer Rim Territories",
            Climate = Climate.Normal
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
                "A Rodian-marked light freighter slides through the queue with diplomatic priority.",
                "Swamp haze curls around Rodia below, lit eerie green by the setting sun.",
                "A Huttese-flagged transport cuts hard across the lane — Rodian traffic control ignores it.",
                "Your sensors spike briefly with a civilian distress beacon, then lose the signal.",
                "A Rodian hunter-ship bristles with trophy mounts as it passes your viewport.",
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
                "A swarm of pleasure yachts cycles the orbital clubs, music leaking even through vacuum comms.",
                "Advertising drones pepper open frequencies with pitches for 'perfectly legal' substances.",
                "A docking ring lights up as a Hutt-flagged cruiser demands priority clearance.",
                "Two smugglers negotiate fuel prices on an unsecured channel, both unaware.",
                "A sector-shielded gang yacht drifts past, bristling with illegal turret mounts.",
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
            Name = "Coruscant Orbit",
            Description = "Jewel of the Core Worlds — the city-planet descends into layers of ruthless Imperial control. Traffic flows in rigid corridors patrolled by ISDs and sentinel droids.",
            IsSpace = true,
            Exits = new() { ["dock"] = "coruscant_docking_bay", ["jump"] = "deep_space" },
            PossibleEncounters = new() { NPCData.BountyHunter },
            EncounterChance = 0.2,
            AmbientMessages = new()
            {
                "The amber lights of Coruscant's surface resemble a pattern of dense roots.",
                "Imperial checkpoints line the dense and orderly space lanes. No one is getting through -- not with a fleet of Imperial Star Destroyers.",
                "Coruscant: Heart of a Heartless Empire.",
                "A diplomatic cruiser breaks from the traffic column with an ISD honor guard.",
                "Your transponder is scanned for the fourth time in as many minutes.",
                "An Imperial Star Destroyer rotates slowly into view, dwarfing everything nearby.",
                "Sentinel probes pass a meter from your hull, scanning for banned cargo.",
                "Comms chatter is flawless, polished, and utterly sterile — just the way the Empire likes it.",
            },
            SpaceEncounters = new() { SpaceEncounterData.ImperialPatrol, SpaceEncounterData.ImperialGunboat },
            SpaceEncounterChance = 0.3,
            PlanetName = "Coruscant",
            StarSystemName = "Coruscant System",
            SectorName = "Corusca Sector",
            TerritoryName = "Core Worlds",
            Climate = Climate.Normal,
            IsSystemSpace = true,
            HyperspaceCoordinates = [13, 10]
        };

        world["coruscant_docking_bay"] = new Location
        {
            Id = "coruscant_docking_bay",
            Name = "Coruscant Public Docking Bay 94",
            Description = "A cavernous public docking bay set into the spire-tops of the lower civic district. Customs droids glide between berths, scanning manifests with mechanical patience. The recycled air carries traces of ozone and high-grade rocket fuel.",
            Exits = new() { ["up"] = "coruscant_orbit", ["north"] = "coruscant_verity_courtyard" },
            PossibleEncounters = new() { NPCData.Stormtrooper, NPCData.ImperialOfficer, NPCData.BountyHunter },
            EncounterChance = 0.3,
            HasShop = true,
            AmbientMessages = new()
            {
                "An Imperial customs officer dictates a manifest aloud while their droid logs every word.",
                "A Corellian freighter lifts with a polite chime — no scorching, no exhaust, just clean repulsor hum.",
                "A queue of diplomats waits patiently behind a velvet rope while their luggage is scanned.",
                "Two Stormtroopers question a Rodian stevedore. The droid translator's voice is unfailingly polite.",
                "An ISB agent in slate-gray tunic strides past, eyes everywhere at once.",
                "A protocol droid offers you a warm towel and a complimentary cup of caf.",
                "Your transponder is scanned again. The droid apologizes for the inconvenience.",
                "A maintenance gantry slides into position with surgical precision.",
            },
            FriendlyNPCs = new() { NPCData.Merchant },
            PlanetName = "Coruscant",
            StarSystemName = "Coruscant System",
            SectorName = "Corusca Sector",
            TerritoryName = "Core Worlds",
            Climate = Climate.Normal,
            HyperspaceCoordinates = [13, 10]
        };

        world["coruscant_verity_courtyard"] = new Location
        {
            Id = "coruscant_verity_courtyard",
            Name = "Verity Courtyard",
            Description = "A vast plaza of polished durastone tiles ringed by senatorial spires. Holographic banners proclaim Imperial unity in twelve languages. Probe droids drift overhead in slow, looping arcs.",
            Exits = new() { ["south"] = "coruscant_docking_bay", ["north"] = "coruscant_federal_courtyard", ["east"] = "coruscant_compnor_arcology" },
            PossibleEncounters = new() { NPCData.Stormtrooper, NPCData.ImperialOfficer },
            EncounterChance = 0.2,
            AmbientMessages = new()
            {
                "An aerial procession of Imperial officials passes overhead in mirrored speeders.",
                "A propaganda holo cycles: 'ORDER. PROSPERITY. THE EMPIRE.'",
                "A pair of senatorial aides whisper urgently and then go silent as you pass.",
                "Probe droids hum in slow lazy circles, recording everything below.",
                "A child reaches up to touch a hovering banner; their guardian pulls them back firmly.",
                "An Imperial chorus performs an anthem from a raised dais. Attendance feels mandatory.",
                "Two ISB agents pretend to converse while watching the crowd in a polished obsidian wall.",
                "A loyalty pledge terminal blinks softly: 'STEP FORWARD TO REAFFIRM.'",
            },
            FriendlyNPCs = new() { NPCData.Merchant },
            PlanetName = "Coruscant",
            StarSystemName = "Coruscant System",
            SectorName = "Corusca Sector",
            TerritoryName = "Core Worlds",
            Climate = Climate.Normal,
            HyperspaceCoordinates = [13, 10]
        };

        world["coruscant_compnor_arcology"] = new Location
        {
            Id = "coruscant_compnor_arcology",
            Name = "COMPNOR Arcology",
            Description = "The Commission for the Preservation of the New Order's residential arcology rises in cold steel terraces. Doctrinal tapestries hang in every corridor. Even the lighting feels supervised.",
            Exits = new() { ["west"] = "coruscant_verity_courtyard", ["north"] = "coruscant_compnor_imperial_register" },
            PossibleEncounters = new() { NPCData.ImperialOfficer, NPCData.Stormtrooper, NPCData.DarkAdept },
            EncounterChance = 0.4,
            AmbientMessages = new()
            {
                "Loyalty officers conduct routine doctrinal interviews behind frosted transparisteel.",
                "A child recites Imperial creed in unison with a holo-instructor.",
                "The hum of surveillance scanners is a constant, low-frequency presence.",
                "A junior cadet eyes you uneasily and looks away before you can meet their gaze.",
                "An ISB poster glares from every wall: 'IF YOU SEE SOMETHING, REPORT EVERYTHING.'",
                "An automated greeter chirps your transponder ID and stops smiling.",
                "Two Sub-Adult Group cadets march past in formation, eyes forward.",
                "A cleaning droid scrubs at a spot on the floor that no longer exists.",
            },
            PlanetName = "Coruscant",
            StarSystemName = "Coruscant System",
            SectorName = "Corusca Sector",
            TerritoryName = "Core Worlds",
            Climate = Climate.Normal,
            HyperspaceCoordinates = [13, 10]
        };

        world["coruscant_compnor_imperial_register"] = new Location
        {
            Id = "coruscant_compnor_imperial_register",
            Name = "COMPNOR Imperial Register",
            Description = "Row upon row of registry terminals catalog every citizen of the Empire. Quiet clerks input data with mechanical patience. The very air feels indexed and cross-referenced.",
            Exits = new() { ["south"] = "coruscant_compnor_arcology" },
            PossibleEncounters = new() { NPCData.ImperialOfficer, NPCData.ImperialOfficer, NPCData.Stormtrooper },
            EncounterChance = 0.45,
            AmbientMessages = new()
            {
                "A clerk pauses, glances at you, and silently makes a note in a record.",
                "An automated voice intones: 'REGISTRATION RENEWAL: CITIZEN 4471-A. SUBMIT.'",
                "Towering data tapestries scroll endlessly with names and identification numbers.",
                "An ISB agent compares two manifests at an isolated desk and frowns.",
                "A janitor droid waxes the floor in geometric perfection, never crossing the same line twice.",
                "A holographic indicator blinks red over an empty workstation, then fades to amber.",
                "Two registrars argue politely about whether a name should be reclassified.",
                "The lighting flickers once. Every clerk pretends not to notice.",
            },
            PlanetName = "Coruscant",
            StarSystemName = "Coruscant System",
            SectorName = "Corusca Sector",
            TerritoryName = "Core Worlds",
            Climate = Climate.Normal,
            HyperspaceCoordinates = [13, 10]
        };

        world["coruscant_federal_courtyard"] = new Location
        {
            Id = "coruscant_federal_courtyard",
            Name = "Federal Courtyard",
            Description = "A vast formal plaza fronting the Imperial Palace. Honor guards in gleaming armor stand motionless at every column. Holocrystal sculptures of Imperial victories rise from reflecting pools.",
            Exits = new() { ["south"] = "coruscant_verity_courtyard", ["east"] = "coruscant_imperial_palace" },
            PossibleEncounters = new() { NPCData.ImperialOfficer, NPCData.Stormtrooper, NPCData.Stormtrooper },
            EncounterChance = 0.35,
            AmbientMessages = new()
            {
                "Two Royal Guards in crimson helms watch you with utter, perfect stillness.",
                "An aerial fly-by of TIE Defenders rattles the reflecting pools into ripples.",
                "Senators in flowing robes confer in low voices, flanked by armored escorts.",
                "A holographic Emperor speaks silently from a thirty-meter projector.",
                "A protocol droid corrects a tourist's posture, gently but firmly.",
                "The fountains shift in coordinated patterns, choreographed for the moment.",
                "An ISB officer adjusts an earpiece and turns to follow someone with their gaze.",
                "A Coruscant Guard squad runs through a precise formation drill, watched by a colonel.",
            },
            FriendlyNPCs = new() { NPCData.Merchant },
            PlanetName = "Coruscant",
            StarSystemName = "Coruscant System",
            SectorName = "Corusca Sector",
            TerritoryName = "Core Worlds",
            Climate = Climate.Normal,
            HyperspaceCoordinates = [13, 10]
        };

        world["coruscant_imperial_palace"] = new Location
        {
            Id = "coruscant_imperial_palace",
            Name = "Imperial Palace - Public Atrium",
            Description = "The Imperial Palace rises in dark mirrored facets, its public atrium a cathedral of Imperial power. Massive banners hang from the vaulted ceiling. Visitors tread softly; no one speaks above a whisper.",
            Exits = new() { ["west"] = "coruscant_federal_courtyard" },
            PossibleEncounters = new() { NPCData.ImperialOfficer, NPCData.DarkAdept, NPCData.Stormtrooper, NPCData.Stormtrooper },
            EncounterChance = 0.5,
            AmbientMessages = new()
            {
                "A Royal Guard turns their head a quarter-degree to track you, then resumes stillness.",
                "The hum of Force-rich air thickens here. Something old listens from the upper galleries.",
                "An ISB Inquisitor passes silently, eyes shielded by a mirrored visor. The temperature drops.",
                "A senator emerges pale from an audience chamber, hands trembling.",
                "Honor guard banners stir without any wind to carry them.",
                "An automated voice softly invites you to 'PROCEED NO FURTHER, CITIZEN.'",
                "A black-robed adjutant escorts a chained prisoner toward a sealed lift.",
                "The polished obsidian floor reflects torches that aren't there.",
            },
            PlanetName = "Coruscant",
            StarSystemName = "Coruscant System",
            SectorName = "Corusca Sector",
            TerritoryName = "Core Worlds",
            Climate = Climate.Normal,
            HyperspaceCoordinates = [13, 10]
        };

        // ===========================================================
        // BESTINE BRANCH — Pika Oasis → Bestine → Jundland → Old Ben
        // ===========================================================

        const string TatooineNormal = "Tatooine";
        const string TatooSystem = "Tatoo System";
        const string ArkanisSector = "Arkanis Sector";
        const string OuterRim = "Outer Rim Territories";

        world["tatooine_pika_oasis"] = new Location
        {
            Id = "tatooine_pika_oasis",
            Name = "Pika Oasis",
            Description = "A rare slash of green in the bone-pale desert. A spring fed from deep aquifers feeds a stand of pikobi-fruit trees and reed grass. Caravans, smugglers, and Tusken bands all come here under an unspoken truce — the water is too valuable for anyone to spoil with violence. Usually.",
            Exits = new() { ["north"] = "tatooine_mospic_high_range", ["south"] = "tatooine_bestine_outskirts" },
            PossibleEncounters = new() { NPCData.TuskenRaider, NPCData.PirateThugs, NPCData.CreatureSmall },
            EncounterChance = 0.3,
            AmbientMessages = new()
            {
                "Pikobi-fruit ferments quietly on the bough — a sweet, yeasty smell drifts on the breeze.",
                "A line of bantha calves wades belly-deep into the spring, escorted by their Tusken handlers.",
                "Two rival caravan masters ignore each other while filling their water bladders side by side.",
                "A reed-warbler trills from the thicket; for a moment the desert sounds almost forgiving.",
                "An old smuggler dozes in the dappled shade with a blaster pistol balanced on their stomach.",
            },
            FriendlyNPCs = new() { NPCData.Merchant },
            PlanetName = TatooineNormal, StarSystemName = TatooSystem,
            SectorName = ArkanisSector, TerritoryName = OuterRim, Climate = Climate.Normal
        };

        world["tatooine_bestine_outskirts"] = new Location
        {
            Id = "tatooine_bestine_outskirts",
            Name = "Bestine Outskirts",
            Description = "The shanties and moisture-farms thinning into the city proper. Tarp-roofed huts lean against rusted speeder skeletons; Imperial recruitment posters flap on every other wall. The smell of charred protein cubes and lubricant follows you toward the city gates.",
            Exits = new() { ["north"] = "tatooine_pika_oasis", ["south"] = "tatooine_bestine_market" },
            PossibleEncounters = new() { NPCData.Stormtrooper, NPCData.PirateThugs, NPCData.ImperialOfficer },
            EncounterChance = 0.3,
            AmbientMessages = new()
            {
                "A pair of moisture vaporators hiss in synchronized pulses, condensing the morning dew.",
                "An Imperial recruiter's holo loops endlessly: 'SERVE TATOOINE. SERVE THE EMPIRE.'",
                "A speeder up on blocks has every body panel removed; a kid sits cross-legged inside the cockpit shell, learning.",
                "Two off-duty stormtroopers haggle with an old woman over a bag of polystarch buns.",
                "A vagrant droid wanders the dust, beeping politely at anyone who might give it a charge.",
            },
            PlanetName = TatooineNormal, StarSystemName = TatooSystem,
            SectorName = ArkanisSector, TerritoryName = OuterRim, Climate = Climate.Normal
        };

        world["tatooine_bestine_market"] = new Location
        {
            Id = "tatooine_bestine_market",
            Name = "Bestine — Capital Market",
            Description = "The seat of Imperial governance on Tatooine pretends to be a market square. Banners of the Empire fly above stalls hawking surplus blasters and 'recovered' contraband. Spies watch from second-story balconies; everyone notices the spies and pretends not to.",
            Exits = new()
            {
                ["north"] = "tatooine_bestine_outskirts",
                ["east"] = "tatooine_bestine_garrison",
                ["west"] = "tatooine_bestine_spaceport",
                ["south"] = "tatooine_judland_wasteland_east"
            },
            PossibleEncounters = new() { NPCData.Stormtrooper, NPCData.ImperialOfficer, NPCData.PirateThugs, NPCData.BountyHunter },
            EncounterChance = 0.3,
            HasShop = true,
            HasVehicleShop = true,
            FriendlyNPCs = new() { NPCData.Merchant },
            AmbientMessages = new()
            {
                "An Imperial herald reads the daily bounty list aloud from a raised platform.",
                "A vendor demonstrates a 'genuine pre-Imperial' rifle whose serial numbers have been ground off.",
                "A Bothan in a long coat slips a flimsi note into another patron's pocket without breaking stride.",
                "Two Imperial officers debate the merits of a confiscated speeder bike, eyeing the crowd warily.",
                "A holo of Governor Tarkin hangs above the central fountain; the fountain has been dry for years.",
            },
            PlanetName = TatooineNormal, StarSystemName = TatooSystem,
            SectorName = ArkanisSector, TerritoryName = OuterRim, Climate = Climate.Normal
        };

        world["tatooine_bestine_garrison"] = new Location
        {
            Id = "tatooine_bestine_garrison",
            Name = "Bestine Imperial Garrison",
            Description = "The 501st Bestine Detachment occupies a fortified durasteel compound at the city's eastern edge. Walking patrols loop the perimeter with practiced precision. The air carries hot ozone from constantly-running power couplings.",
            Exits = new() { ["west"] = "tatooine_bestine_market" },
            PossibleEncounters = new() { NPCData.Stormtrooper, NPCData.Stormtrooper, NPCData.Snowtrooper, NPCData.ImperialOfficer },
            EncounterChance = 0.55,
            AmbientMessages = new()
            {
                "A drill sergeant barks orders at a squad of new conscripts in the courtyard.",
                "An AT-ST stalks past the gatehouse on heavy bipedal pistons, crew waving lazily.",
                "Sparks fall from a maintenance scaffold where a TIE Fighter is being refit between sorties.",
                "A trio of stormtroopers sits in a circle eating ration tubes, helmets at their feet.",
                "An officer reviews a glowing star map on a holotable, frowning at a region marked 'JUNDLAND.'",
            },
            PlanetName = TatooineNormal, StarSystemName = TatooSystem,
            SectorName = ArkanisSector, TerritoryName = OuterRim, Climate = Climate.Normal
        };

        world["tatooine_bestine_spaceport"] = new Location
        {
            Id = "tatooine_bestine_spaceport",
            Name = "Bestine Spaceport",
            Description = "A working spaceport for the capital — bigger than Mos Espa's docking bays and run with strict Imperial protocol. Customs droids glide between berths; a queue of merchants waits patiently while their manifests are scanned.",
            Exits = new() { ["east"] = "tatooine_bestine_market", ["up"] = "tatooine_orbit" },
            PossibleEncounters = new() { NPCData.Stormtrooper, NPCData.BountyHunter, NPCData.ImperialOfficer },
            EncounterChance = 0.3,
            HasShop = true,
            HasVehicleShop = true,
            AmbientMessages = new()
            {
                "A YT-2400 freighter clears customs and lifts in a precise vertical column of repulsor wash.",
                "Customs droids form an orderly procession from berth to berth, scanning manifests in silent unison.",
                "A merchant family sells fresh nerf jerky from a foldout cart between docking-pad gantries.",
                "An Imperial customs officer reads aloud from a confiscated cargo list, voice flat with boredom.",
                "A bounty hunter in scuffed armor leans against their landing strut, watching the queue.",
            },
            FriendlyNPCs = new() { NPCData.Merchant },
            PlanetName = TatooineNormal, StarSystemName = TatooSystem,
            SectorName = ArkanisSector, TerritoryName = OuterRim, Climate = Climate.Normal
        };

        world["tatooine_judland_wasteland_east"] = new Location
        {
            Id = "tatooine_judland_wasteland_east",
            Name = "Jundland Wastes — Eastern Approach",
            Description = "The Jundland's eastern edge: petrified canyons of ochre stone and bleached bone. Wind hisses constantly through gaps in the rock. Imperial patrols don't come this far; the few travelers you meet are fugitives, hermits, or worse.",
            Exits = new() { ["north"] = "tatooine_bestine_market", ["west"] = "tatooine_judland_wasteland_central" },
            PossibleEncounters = new() { NPCData.TuskenRaider, NPCData.CreatureSmall, NPCData.PirateThugs },
            EncounterChance = 0.4,
            AmbientMessages = new()
            {
                "A wind devil whips into being for half a second, then tears itself apart on a cliff face.",
                "Bleached krayt-dragon ribs arch over the trail; somebody's strung a warning bell from the largest one.",
                "A line of footprints crosses your path and ends abruptly mid-stride.",
                "Distant tapping echoes through the canyon — gaderffii on stone, slow and steady.",
                "A vulture-bat circles overhead, watching with patient interest.",
            },
            PlanetName = TatooineNormal, StarSystemName = TatooSystem,
            SectorName = ArkanisSector, TerritoryName = OuterRim, Climate = Climate.Normal
        };

        world["tatooine_judland_wasteland_central"] = new Location
        {
            Id = "tatooine_judland_wasteland_central",
            Name = "Jundland Wastes — Central Maze",
            Description = "The deep Jundland: a labyrinth of slot canyons that swallow sound and direction. Sand People war-shrines mark every chokepoint. Even Tatooine's twin suns barely reach the canyon floor here, and the temperature drops fast in the shadows.",
            Exits = new() { ["east"] = "tatooine_judland_wasteland_east", ["west"] = "tatooine_judland_wasteland_west" },
            PossibleEncounters = new() { NPCData.TuskenRaider, NPCData.TuskenRaider, NPCData.CreatureSmall },
            EncounterChance = 0.55,
            AmbientMessages = new()
            {
                "A row of bleached banthas' skulls watches the canyon floor from a high ledge.",
                "Tusken pictographs in red ochre cover one wall — hunting scenes, star paths, warnings.",
                "Your footsteps echo back from three different directions at once.",
                "A torn fragment of a Jedi robe — or a robe like one — flutters caught on a rock spike.",
                "The air is suddenly cold; a moment later the wind shifts and the heat returns.",
            },
            PlanetName = TatooineNormal, StarSystemName = TatooSystem,
            SectorName = ArkanisSector, TerritoryName = OuterRim, Climate = Climate.Normal
        };

        world["tatooine_judland_wasteland_west"] = new Location
        {
            Id = "tatooine_judland_wasteland_west",
            Name = "Jundland Wastes — Western Reaches",
            Description = "The western Jundland opens onto a bleak expanse of cracked clay flats and isolated rock spires. A single eroded path winds up toward a hermit's residence on a high bluff. The Force feels different here — quieter, watchful.",
            Exits = new()
            {
                ["east"] = "tatooine_judland_wasteland_central",
                ["south"] = "tatooine_northern_jawa_territories",
                ["northwest"] = "tatooine_old_ben_residence"
            },
            PossibleEncounters = new() { NPCData.TuskenRaider, NPCData.CreatureSmall },
            EncounterChance = 0.4,
            AmbientMessages = new()
            {
                "Heat shimmer dissolves a distant rock spire into a dancing apparition.",
                "An aged Tusken stands motionless on a bluff to the south, watching you and saying nothing.",
                "A circle of small stones marks a meditation cairn — not Tusken, not Jawa, something older.",
                "A womp rat skitters across the path and disappears into a crack between boulders.",
                "The wind carries a half-heard syllable, gone before you can place it.",
            },
            PlanetName = TatooineNormal, StarSystemName = TatooSystem,
            SectorName = ArkanisSector, TerritoryName = OuterRim, Climate = Climate.Normal
        };

        world["tatooine_northern_jawa_territories"] = new Location
        {
            Id = "tatooine_northern_jawa_territories",
            Name = "Northern Jawa Territories",
            Description = "The roving Jawa clans of the northern flats have parked their sandcrawlers in a loose ring, forming an impromptu trade-camp. Hooded figures swarm in chittering clusters, hauling salvage between the towering machines. The smell of solder, ozone, and unwashed wool is overwhelming.",
            Exits = new() { ["north"] = "tatooine_judland_wasteland_west" },
            PossibleEncounters = new() { NPCData.PirateThugs, NPCData.TuskenRaider },
            EncounterChance = 0.25,
            HasShop = true,
            HasVehicleShop = true,
            FriendlyNPCs = new() { NPCData.Merchant, NPCData.Merchant },
            AmbientMessages = new()
            {
                "A sandcrawler's bay door yawns open; conveyor belts spool out salvaged droid parts onto the sand.",
                "A pair of Jawas argue furiously over the fair market value of a half-corroded astromech head.",
                "A child Jawa proudly demonstrates a working power converter to its smaller siblings.",
                "An auctioneer in a battered electromegaphone calls bids in rapid-fire Jawa, tone unmistakable.",
                "A R5-series droid is wheeled onto the auction block; it whistles a sad, resigned tune.",
            },
            PlanetName = TatooineNormal, StarSystemName = TatooSystem,
            SectorName = ArkanisSector, TerritoryName = OuterRim, Climate = Climate.Normal
        };

        world["tatooine_old_ben_residence"] = new Location
        {
            Id = "tatooine_old_ben_residence",
            Name = "Old Ben's Residence",
            Description = "A modest hermit's dwelling carved into the side of a high bluff overlooking the Jundland. Dust drifts through the open doorway; a single cup of caf sits cold on a low stone table, as though the occupant just stepped out. The Force here is dense, patient — like a fingerprint pressed into the air.",
            Exits = new() { ["southeast"] = "tatooine_judland_wasteland_west" },
            PossibleEncounters = new() { NPCData.DarkAdept, NPCData.CreatureSmall },
            EncounterChance = 0.2,
            AmbientMessages = new()
            {
                "Sand has half-buried a journal on the table; the binding is hand-stitched, the script archaic.",
                "A scorched circle on the floor marks where a saber once cut through stone — long since cooled.",
                "Outside, a krayt-dragon's distant call rolls down the canyons, paused, then echoes again.",
                "A small carved wooden token sits on a high shelf — featureless except for two suns and a starbird.",
                "The Force in here is not unkind. It seems to be... waiting for someone.",
            },
            PlanetName = TatooineNormal, StarSystemName = TatooSystem,
            SectorName = ArkanisSector, TerritoryName = OuterRim, Climate = Climate.Normal
        };

        // Note: deep_space uses a numeric `jump` menu (handled in CommandParser)
        // rather than static exits to in-system space locations.

        // Initialize NPC resolve for encounters
        RegisterImported(world);
        return world;
    }
}
