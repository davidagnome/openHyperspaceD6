using TerminalHyperspace.Models;

namespace TerminalHyperspace.Content;

public enum CheckDifficulty { Easy, Moderate, Difficult, Challenging }

public class SkillCheckEvent
{
    public string Id { get; set; } = "";
    public string Description { get; set; } = "";
    public string SuccessText { get; set; } = "";
    public string FailText { get; set; } = "";
    public SkillType Skill { get; set; }
    public CheckDifficulty Difficulty { get; set; }
    public int TargetNumber { get; set; }
    public int CreditReward { get; set; }
    public int UpgradePointReward { get; set; }
    public bool Repeatable { get; set; }

    /// Credits deducted from the player on failure (clamped at zero balance).
    public int CreditPenalty { get; set; }

    /// If set, failure spawns this NPC and forces a combat encounter.
    public Func<Character>? CombatNpcOnFail { get; set; }

    /// Extra narration line shown after FailText to describe the penalty.
    public string FailPenaltyText { get; set; } = "";
}

public static partial class SkillCheckData
{
    static partial void RegisterImportedLocationChecks(Dictionary<string, List<SkillCheckEvent>> map);
    static partial void RegisterImportedTalkChecks(List<SkillCheckEvent> list);

    // =========================================================
    // TATOOINE_ESPA_CANTINA
    // =========================================================
    public static SkillCheckEvent CantinaLockbox => new()
    {
        Id = "cantina_lockbox",
        Description = "You notice a secure lockbox wedged under a cantina booth. Its electronic lock blinks red.",
        SuccessText = "You slice through the encryption with ease. Inside: a credit chip and a data stick.",
        FailText = "The lock flashes angrily and emits a warning tone. A Trandoshan bouncer notices.",
        FailPenaltyText = "The bouncer decides you're a thief. Fists come up.",
        Skill = SkillType.Computers, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 12, CreditReward = 60, UpgradePointReward = 1,
        CombatNpcOnFail = NPCData.PirateThugs,
    };
    public static SkillCheckEvent CantinaSabaccGame => new()
    {
        Id = "cantina_sabacc",
        Description = "A sabacc game is running hot in the corner. Buy-in: 40 credits. The Rodian dealer eyes you.",
        SuccessText = "You read the table perfectly, bluff on a mediocre hand, and rake in the pot.",
        FailText = "Your hand collapses. The dealer scoops your chips and moves to the next player.",
        Skill = SkillType.Deceive, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 12, CreditReward = 100, UpgradePointReward = 1,
        Repeatable = true, CreditPenalty = 40,
    };
    public static SkillCheckEvent CantinaDrunkDiplomat => new()
    {
        Id = "cantina_drunk_diplomat",
        Description = "A slurring Twi'lek diplomat flashes a credit chit and demands someone 'settle a bet' about Outer Rim politics.",
        SuccessText = "You humor the diplomat with plausible answers. They peel off credits and stumble away.",
        FailText = "You say the wrong thing. The diplomat's two bodyguards peel off from the bar.",
        FailPenaltyText = "The bodyguards want 'satisfaction' for the insult to their employer.",
        Skill = SkillType.Galaxy, Difficulty = CheckDifficulty.Easy,
        TargetNumber = 9, CreditReward = 35, UpgradePointReward = 1,
        CombatNpcOnFail = NPCData.PirateThugs,
    };
    public static SkillCheckEvent CantinaPickpocketMark => new()
    {
        Id = "cantina_pickpocket",
        Description = "A drunk mercenary's credit pouch dangles loose from their belt. Bulging.",
        SuccessText = "A graze of the shoulder, a slip of the fingers, and the pouch is yours.",
        FailText = "The mercenary's hand clamps around your wrist like a vice. 'Thief!'",
        Skill = SkillType.Steal, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 80, UpgradePointReward = 1,
        CombatNpcOnFail = NPCData.BountyHunter,
    };
    public static SkillCheckEvent CantinaRumorMill => new()
    {
        Id = "cantina_rumors",
        Description = "Buy a round of Corellian whiskey and trade gossip with the regulars? Drinks cost 15 credits.",
        SuccessText = "One grizzled spacer mentions a cargo manifest 'left unattended' at the docking bay. You pocket the tip.",
        FailText = "You mispronounce a Huttese idiom. The regulars clam up and turn away.",
        Skill = SkillType.Streetwise, Difficulty = CheckDifficulty.Easy,
        TargetNumber = 8, CreditReward = 40, UpgradePointReward = 1,
        Repeatable = true, CreditPenalty = 15,
    };
    public static SkillCheckEvent CantinaBackroomBrawl => new()
    {
        Id = "cantina_backroom_brawl",
        Description = "A scar-faced Nikto challenges all comers to an arm-wrestling match. Pot: 50 credits.",
        SuccessText = "Your arm locks straight as durasteel. Their knuckles slam the table.",
        FailText = "Your arm folds. The Nikto laughs, pockets the pot, and shoves you out of the chair.",
        Skill = SkillType.Athletics, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 11, CreditReward = 50, UpgradePointReward = 1,
        Repeatable = true,
    };

    // =========================================================
    // TATOOINE_ESPA_MARKET
    // =========================================================
    public static SkillCheckEvent MarketHaggle => new()
    {
        Id = "market_haggle",
        Description = "A vendor is selling a crate of surplus supplies at a steep markup. Think you can talk the price down?",
        SuccessText = "Your silver tongue works wonders. The vendor sighs and gives you the 'friends and family' discount.",
        FailText = "The vendor sees right through your bluff. 'Nice try, spacer. Full price or move along.'",
        Skill = SkillType.Persuade, Difficulty = CheckDifficulty.Easy,
        TargetNumber = 8, CreditReward = 35, UpgradePointReward = 0,
    };
    public static SkillCheckEvent MarketForgedCoinScam => new()
    {
        Id = "market_forged_coin",
        Description = "A slick-talking Pantoran offers 'rare Old Republic coins' at a steep discount.",
        SuccessText = "You spot the forgery markings instantly. You threaten to call the magistrate — they flee, dropping their coin pouch.",
        FailText = "You buy the coins. At the next stall you learn they're stamped durasteel slag worth nothing.",
        FailPenaltyText = "The Pantoran is long gone with your money.",
        Skill = SkillType.Streetwise, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 55, UpgradePointReward = 1,
        CreditPenalty = 50,
    };
    public static SkillCheckEvent MarketCrowdSlip => new()
    {
        Id = "market_crowd_slip",
        Description = "Imperial patrol sweep. A Stormtrooper squad is checking IDs at the far end of the market. Slip through the crowd?",
        SuccessText = "You melt into the press of bodies, matching the rhythm of the crowd. The patrol passes by.",
        FailText = "You stand out. A trooper waves you over for questioning.",
        FailPenaltyText = "The shakedown costs you a bribe to avoid detention.",
        Skill = SkillType.Hide, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 0, UpgradePointReward = 1,
        CreditPenalty = 75, Repeatable = true,
    };
    public static SkillCheckEvent MarketWarehouseSlice => new()
    {
        Id = "market_warehouse_slice",
        Description = "A storage warehouse's back door uses an older-model lock. The inventory inside is unattended.",
        SuccessText = "The lock yields to your slicing. You grab a crate of medicinal supplies and slip out.",
        FailText = "A silent alarm trips. Private security converges on your position.",
        Skill = SkillType.Computers, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 120, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.PirateThugs,
    };
    public static SkillCheckEvent MarketExoticAnimal => new()
    {
        Id = "market_exotic_animal",
        Description = "A caged nexu cub snarls at passersby. The vendor claims it's 'tame-ish.' Try to calm it?",
        SuccessText = "You soothe the cub and identify its species for the vendor. They tip you for the appraisal.",
        FailText = "The nexu lunges through the bars. You barely avoid the swipe — but the crowd panics.",
        FailPenaltyText = "You're blamed for agitating the animal and fined by the market warden.",
        Skill = SkillType.Xenology, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 45, UpgradePointReward = 1,
        CreditPenalty = 30,
    };
    public static SkillCheckEvent MarketDroidAuction => new()
    {
        Id = "market_droid_auction",
        Description = "A battered protocol droid is up for auction, claimed to be 'ex-Imperial Intelligence.' Appraise it?",
        SuccessText = "You confirm the motivator logs — genuine Imperial hardware. You buy low and resell for a premium.",
        FailText = "You misidentify the droid as a common unit. A sharper buyer swoops in.",
        Skill = SkillType.Droids, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 12, CreditReward = 70, UpgradePointReward = 1,
    };

    // =========================================================
    // TATOOINE_ESPA_DOCKING_BAY
    // =========================================================
    public static SkillCheckEvent DockingBaySmuggle => new()
    {
        Id = "docking_bay_smuggle",
        Description = "A nervous-looking Bothan approaches. 'I need a crate moved past the customs droid. No questions asked. You in?'",
        SuccessText = "You distract the customs droid with a fake manifest while the crate slips through. The Bothan pays handsomely.",
        FailText = "The customs droid isn't fooled. The Bothan vanishes into the crowd and Imperials move in.",
        FailPenaltyText = "You're fined heavily for the smuggling attempt.",
        Skill = SkillType.Deceive, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 12, CreditReward = 70, UpgradePointReward = 1,
        CreditPenalty = 100,
    };
    public static SkillCheckEvent DockingBayHotwireSpeeder => new()
    {
        Id = "docking_bay_hotwire",
        Description = "An unattended swoop bike sits idling by pad 3. Its owner is nowhere to be seen.",
        SuccessText = "Seconds later you're gone. You fence the bike across town for a tidy sum.",
        FailText = "You trip the anti-theft grid. Alarms blare and the owner — an armed bounty hunter — sprints back.",
        Skill = SkillType.Vehicles, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 150, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.BountyHunter,
    };
    public static SkillCheckEvent DockingBayRepairBid => new()
    {
        Id = "docking_bay_repair_bid",
        Description = "A freighter captain posts an open repair bid. The hyperdrive's shaky and they're leaving in an hour.",
        SuccessText = "You diagnose and patch a misaligned capacitor. The captain pays on the spot.",
        FailText = "Your 'fix' makes the misalignment worse. The captain curses and docks your bid.",
        FailPenaltyText = "You're out the cost of components you ruined.",
        Skill = SkillType.Vehicles, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 65, UpgradePointReward = 1,
        CreditPenalty = 40,
    };
    public static SkillCheckEvent DockingBayCustomsOfficer => new()
    {
        Id = "docking_bay_customs_bribe",
        Description = "An Imperial customs officer eyes your gear. A well-placed bribe might save you a search.",
        SuccessText = "The officer pockets the 'processing fee' and waves you through with a nod.",
        FailText = "The officer takes offense. 'Are you attempting to bribe an Imperial official?' Troopers approach.",
        Skill = SkillType.Persuade, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 0, UpgradePointReward = 1,
        CreditPenalty = 50,
        CombatNpcOnFail = NPCData.Stormtrooper,
    };
    public static SkillCheckEvent DockingBaySalvageCart => new()
    {
        Id = "docking_bay_salvage",
        Description = "A Jawa salvage droid is auctioning scrap components. The top lot might contain something valuable.",
        SuccessText = "You spot a mint-condition hyperdrive motivator in the pile. You flip it to a desperate captain for a fat profit.",
        FailText = "The 'valuable' part is rusted beyond use. You've burned your bid money.",
        FailPenaltyText = "The Jawas chirp derisively and move on.",
        Skill = SkillType.Vehicles, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 12, CreditReward = 80, UpgradePointReward = 1,
        CreditPenalty = 35,
    };
    public static SkillCheckEvent DockingBaySensorGhost => new()
    {
        Id = "docking_bay_sensor_ghost",
        Description = "A dockmaster complains of a sensor ghost haunting pad 4. Could be a malfunction — or a cloaked arrival.",
        SuccessText = "You filter out the harmonic interference. The dockmaster pays for the technical consult.",
        FailText = "You misread the returns and miss a real anomaly. The dockmaster is unimpressed.",
        Skill = SkillType.Sensors, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 55, UpgradePointReward = 1,
    };

    // =========================================================
    // TATOOINE_ESPA_ALLEY
    // =========================================================
    public static SkillCheckEvent AlleyBrokenDroid => new()
    {
        Id = "alley_broken_droid",
        Description = "A battered astromech droid lies on its side in the filth, sparking feebly. It beeps a distress call.",
        SuccessText = "You patch the droid's motivator and it whirs back to life. It chirps gratefully and transfers a reward to your account.",
        FailText = "You fumble with the droid's internals and accidentally short out its memory core. It goes silent.",
        Skill = SkillType.Droids, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 11, CreditReward = 40, UpgradePointReward = 1,
    };
    public static SkillCheckEvent AlleyMuggingAmbush => new()
    {
        Id = "alley_ambush_spot",
        Description = "You feel eyes on you. Check the shadows carefully?",
        SuccessText = "You spot two thugs crouched behind a trash compactor. You pull a blaster and they scatter.",
        FailText = "You see nothing. A blow catches you from behind and a knife flashes at your throat.",
        Skill = SkillType.Search, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 0, UpgradePointReward = 1,
        CombatNpcOnFail = NPCData.PirateThugs,
    };
    public static SkillCheckEvent AlleyRebelCourier => new()
    {
        Id = "alley_rebel_courier",
        Description = "A cloaked figure presses a datacard into your palm. 'Get this to the northern contact. 150 credits when done.'",
        SuccessText = "You slip through side passages and deliver the card. The contact pays in untraceable credits.",
        FailText = "You lose the courier in the crowd — or they lose you. Someone else takes the contract.",
        Skill = SkillType.Streetwise, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 12, CreditReward = 150, UpgradePointReward = 2,
    };
    public static SkillCheckEvent AlleyGraffitiCipher => new()
    {
        Id = "alley_graffiti_cipher",
        Description = "Fresh graffiti covers one wall — but the letters look wrong, like a substitution cipher.",
        SuccessText = "You decode the message: a rebel cell meeting time and location. Intel worth credits to the right ears.",
        FailText = "It just looks like gang tags to you. You shrug and move on.",
        Skill = SkillType.Computers, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 90, UpgradePointReward = 2,
    };
    public static SkillCheckEvent AlleyWoundedFugitive => new()
    {
        Id = "alley_wounded_fugitive",
        Description = "A bleeding Rodian slumps against a dumpster, clutching a blaster wound. 'Please... don't call the Imps...'",
        SuccessText = "You stabilize the wound and hide them behind a stack of crates. They press a credit chit into your hand.",
        FailText = "They bleed out before you can help. Their pursuers arrive minutes later — and you're the only witness.",
        FailPenaltyText = "The arriving bounty hunters assume YOU did it.",
        Skill = SkillType.Medicine, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 80, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.BountyHunter,
    };
    public static SkillCheckEvent AlleyStashRecovery => new()
    {
        Id = "alley_stash_recovery",
        Description = "A loose panel in the wall rattles suspiciously. Pry it open?",
        SuccessText = "Behind the panel: a small credstick and someone's hidden stash of spice.",
        FailText = "The panel's rigged. A needle-spring jabs your hand with something foul.",
        FailPenaltyText = "The toxin leaves you sluggish and short-tempered.",
        Skill = SkillType.Search, Difficulty = CheckDifficulty.Easy,
        TargetNumber = 9, CreditReward = 60, UpgradePointReward = 1,
        CreditPenalty = 20,
    };

    // =========================================================
    // TATOOINE_ESPA_TUNNELS
    // =========================================================
    public static SkillCheckEvent TunnelsPoisonGas => new()
    {
        Id = "tunnels_poison_gas",
        Description = "A ruptured pipe leaks a noxious green gas into the corridor. You'll need to hold your breath and push through.",
        SuccessText = "You power through the toxic cloud, lungs burning but intact. On the other side, you find an abandoned supply cache.",
        FailText = "The gas overwhelms you and you stumble back, coughing and disoriented.",
        Skill = SkillType.Stamina, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 30, UpgradePointReward = 1,
    };
    public static SkillCheckEvent TunnelsLooseGrate => new()
    {
        Id = "tunnels_loose_grate",
        Description = "A floor grate wobbles suspiciously. Test your weight and cross?",
        SuccessText = "You distribute your weight and skip across. Beneath the grate: a maintenance worker's forgotten toolkit.",
        FailText = "The grate collapses. You fall two meters onto broken piping.",
        FailPenaltyText = "You limp away, battered and credits lighter from the bacta patches.",
        Skill = SkillType.Athletics, Difficulty = CheckDifficulty.Easy,
        TargetNumber = 9, CreditReward = 45, UpgradePointReward = 1,
        CreditPenalty = 25,
    };
    public static SkillCheckEvent TunnelsRatKing => new()
    {
        Id = "tunnels_rat_king",
        Description = "A writhing mass of mutated womp rats blocks the corridor — a 'rat king' gone feral.",
        SuccessText = "You spot a weak point in their knot and scatter them with a single shot. The bounty office pays by the tail.",
        FailText = "The rat king lunges as one seething mass.",
        Skill = SkillType.Blasters, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 55, UpgradePointReward = 1,
        CombatNpcOnFail = NPCData.Diagnoga, Repeatable = true,
    };
    public static SkillCheckEvent TunnelsSewageCurrent => new()
    {
        Id = "tunnels_sewage_current",
        Description = "The service tunnel floods intermittently. A stash glints beneath the surface — but the current is fast.",
        SuccessText = "You time the flow and snatch the waterproof satchel. Credits and a datachip inside.",
        FailText = "The current sweeps you into a grate. You lose grip and your own credit pouch.",
        Skill = SkillType.Stamina, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 75, UpgradePointReward = 1,
        CreditPenalty = 40,
    };
    public static SkillCheckEvent TunnelsFeralBeast => new()
    {
        Id = "tunnels_feral_beast",
        Description = "Claw marks on the wall are FRESH. Something large lives here.",
        SuccessText = "You read the signs and circle around the lair, avoiding it entirely.",
        FailText = "You walk right into the nest. It wakes.",
        Skill = SkillType.Survival, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 0, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.Diagnoga,
    };
    public static SkillCheckEvent TunnelsForgottenCache => new()
    {
        Id = "tunnels_forgotten_cache",
        Description = "A rust-frozen maintenance locker clearly hasn't been opened in years. Pry it open?",
        SuccessText = "Inside: an old but functional toolkit and a bundle of Republic-era credits, still redeemable.",
        FailText = "The hinge snaps and the door collapses. Empty inside. And you cut your hand.",
        Skill = SkillType.Athletics, Difficulty = CheckDifficulty.Easy,
        TargetNumber = 9, CreditReward = 50, UpgradePointReward = 1,
    };

    // =========================================================
    // TATOOINE_ESPA_REACTOR
    // =========================================================
    public static SkillCheckEvent ReactorOverload => new()
    {
        Id = "reactor_overload",
        Description = "A reactor conduit is cycling dangerously. If you can reroute the power flow, it could be stabilized — and the station crew would owe you one.",
        SuccessText = "You recalibrate the conduit matrix and the warning lights go green. A grateful technician transfers hazard pay to your account.",
        FailText = "Sparks fly as you cross the wrong circuits. You narrowly dodge an arc of plasma.",
        FailPenaltyText = "The discharge fries the multi-tool in your pack.",
        Skill = SkillType.Armament, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 100, UpgradePointReward = 2,
        CreditPenalty = 60,
    };
    public static SkillCheckEvent ReactorForceTurbulence => new()
    {
        Id = "reactor_force_turbulence",
        Description = "The Force currents here are wild, turbulent. If you reach in, you might commune with something — or be swept up.",
        SuccessText = "You ride the current. Clarity floods you: distant visions, whispered truths.",
        FailText = "The current slams you back. You stagger, nose bleeding.",
        Skill = SkillType.Sense, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 0, UpgradePointReward = 3,
    };
    public static SkillCheckEvent ReactorCoolantBreach => new()
    {
        Id = "reactor_coolant_breach",
        Description = "A coolant line has split. The liquid is freezing as it sprays. Seal it before it floods the chamber?",
        SuccessText = "You clamp the line with a gravitic seal. The engineer on duty transfers a bonus.",
        FailText = "The line whips loose. Frostbite sets in along your arm.",
        FailPenaltyText = "Medical bills and ruined gloves take a chunk of credits.",
        Skill = SkillType.Stamina, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 70, UpgradePointReward = 1,
        CreditPenalty = 45,
    };
    public static SkillCheckEvent ReactorSecurityTerminal => new()
    {
        Id = "reactor_security_terminal",
        Description = "An Imperial security terminal controls reactor access logs. Scrubbing your entry could pay off later.",
        SuccessText = "You scrub the logs and plant a false entry. A bounty hunter's warrant on you quietly disappears.",
        FailText = "The terminal detects the intrusion. An internal security patrol is dispatched.",
        Skill = SkillType.Computers, Difficulty = CheckDifficulty.Challenging,
        TargetNumber = 21, CreditReward = 0, UpgradePointReward = 3,
        CombatNpcOnFail = NPCData.Stormtrooper,
    };
    public static SkillCheckEvent ReactorSabotage => new()
    {
        Id = "reactor_sabotage",
        Description = "A rebel contact has offered serious credits to plant a dormant disruption device on a secondary conduit.",
        SuccessText = "You tuck the device behind an access plate. Untraceable — and your bank account grows fat.",
        FailText = "A patrolling technician clocks your motion. The device is confiscated and your name flagged.",
        FailPenaltyText = "The blown contract costs you your upfront payment.",
        Skill = SkillType.Hide, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 200, UpgradePointReward = 2,
        CreditPenalty = 80,
        CombatNpcOnFail = NPCData.Stormtrooper,
    };
    public static SkillCheckEvent ReactorRadiationSurvey => new()
    {
        Id = "reactor_radiation_survey",
        Description = "The reactor chief offers a contract to sweep radiation levels in the deep chamber. Hazardous but quick.",
        SuccessText = "You complete the survey, dosimeter steady. The chief pays on completion.",
        FailText = "A pocket of high-rad exposure slips past your meter. You leave feeling ill.",
        FailPenaltyText = "Anti-rad meds cost you precious credits.",
        Skill = SkillType.Sensors, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 90, UpgradePointReward = 1,
        CreditPenalty = 50,
    };

    // =========================================================
    // TATOOINE_ESPA_COMMAND
    // =========================================================
    public static SkillCheckEvent CommandTerminal => new()
    {
        Id = "command_terminal",
        Description = "An unattended command terminal displays classified Imperial shipping routes. If you can download the data...",
        SuccessText = "You bypass security and download the route data. This intelligence is worth a fortune on the black market.",
        FailText = "An alarm triggers and the terminal locks down. Troopers converge.",
        Skill = SkillType.Computers, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 120, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.Stormtrooper,
    };
    public static SkillCheckEvent CommandOfficerInterrogation => new()
    {
        Id = "command_officer_interrogation",
        Description = "An Imperial officer stops you. 'Papers. Now.' Their eyes narrow at your gear.",
        SuccessText = "You stay composed under pressure. They wave you through without a second glance.",
        FailText = "Your stammered answers convince the officer you're hiding something.",
        Skill = SkillType.Willpower, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 0, UpgradePointReward = 1,
        CombatNpcOnFail = NPCData.Stormtrooper,
    };
    public static SkillCheckEvent CommandEavesdrop => new()
    {
        Id = "command_eavesdrop",
        Description = "Two officers murmur over a datapad. Want to drift closer and listen?",
        SuccessText = "You catch coordinates for a supply convoy. Worth credits to the right buyer.",
        FailText = "You trip over a cabling line. The officers spin, hands on weapons.",
        Skill = SkillType.Hide, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 100, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.ImperialOfficer,
    };
    public static SkillCheckEvent CommandSignalIntercept => new()
    {
        Id = "command_signal_intercept",
        Description = "A junior comms tech steps away from their console. Splice a receiver in before they return?",
        SuccessText = "The splice is flawless. You'll be intercepting Imperial chatter for weeks to come.",
        FailText = "The splice sparks. The tech returns early and catches you elbow-deep in their gear.",
        Skill = SkillType.Armament, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 150, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.Stormtrooper,
    };
    public static SkillCheckEvent CommandForcedEntry => new()
    {
        Id = "command_forced_entry",
        Description = "A locked records vault lies off the main floor. Force the door while nobody's looking?",
        SuccessText = "The door groans open. Inside: sealed personnel files worth trading for favors.",
        FailText = "The door bends but holds. The impact alarm trips.",
        Skill = SkillType.Athletics, Difficulty = CheckDifficulty.Challenging,
        TargetNumber = 21, CreditReward = 130, UpgradePointReward = 3,
        CombatNpcOnFail = NPCData.Stormtrooper,
    };
    public static SkillCheckEvent CommandTacticalMap => new()
    {
        Id = "command_tactical_map",
        Description = "A holo-display shows Imperial fleet dispositions. Commit the layout to memory?",
        SuccessText = "You burn the pattern into your mind. Rebel intelligence will pay dearly for this.",
        FailText = "The patterns blur. You can't parse what matters from what's obvious.",
        Skill = SkillType.Galaxy, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 160, UpgradePointReward = 2,
    };

    // =========================================================
    // TATOOINE_ESPA_UPPER_DISTRICT
    // =========================================================
    public static SkillCheckEvent UpperDistrictForge => new()
    {
        Id = "upper_forged_docs",
        Description = "A contact offers to sell you forged transit papers — if you can verify they're convincing enough to pass Imperial inspection.",
        SuccessText = "You spot the telltale signs of quality work. These will pass any checkpoint. The contact throws in a bonus for your eye.",
        FailText = "You can't tell the real from the fake. The contact shrugs.",
        FailPenaltyText = "You bought the papers anyway. They're garbage.",
        Skill = SkillType.Streetwise, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 50, UpgradePointReward = 1,
        CreditPenalty = 60,
    };
    public static SkillCheckEvent UpperDistrictArtForge => new()
    {
        Id = "upper_art_forgery",
        Description = "A gallery displays a 'priceless' Alderaanian tapestry. Something about it feels off.",
        SuccessText = "You spot the anachronistic dye — a forgery. The gallery owner pays handsomely to keep quiet.",
        FailText = "You praise the piece loudly. The owner laughs at your credulity.",
        Skill = SkillType.Xenology, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 110, UpgradePointReward = 2,
    };
    public static SkillCheckEvent UpperDistrictSocialClimb => new()
    {
        Id = "upper_social_climb",
        Description = "A wealthy noble's gala is in full swing. Sweet-talk your way onto the guest list?",
        SuccessText = "Your charm wins an invitation. Networking yields a lucrative 'consulting' contract.",
        FailText = "The majordomo isn't fooled. Private security escorts you to the street.",
        FailPenaltyText = "Your formal wear is ruined on the way out.",
        Skill = SkillType.Persuade, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 180, UpgradePointReward = 2,
        CreditPenalty = 40,
    };
    public static SkillCheckEvent UpperDistrictPickpocketAide => new()
    {
        Id = "upper_pickpocket_aide",
        Description = "A noble's aide is distracted by a holocall. Their credit chit is in plain view.",
        SuccessText = "You ghost past. The chit is yours and they won't notice for hours.",
        FailText = "A security droid's optics track your hand. Alarm klaxons.",
        Skill = SkillType.Steal, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 200, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.Stormtrooper,
    };
    public static SkillCheckEvent UpperDistrictCorruptDroid => new()
    {
        Id = "upper_corrupt_droid",
        Description = "A protocol droid serving as a courier carries sealed bribes to a magistrate. Intercept its logic core?",
        SuccessText = "You install a logic loop. The droid delivers its package — to you.",
        FailText = "The droid's counter-slice kicks in. It broadcasts an alert.",
        Skill = SkillType.Droids, Difficulty = CheckDifficulty.Challenging,
        TargetNumber = 21, CreditReward = 250, UpgradePointReward = 3,
        CombatNpcOnFail = NPCData.Stormtrooper,
    };
    public static SkillCheckEvent UpperDistrictDeceiveGuard => new()
    {
        Id = "upper_deceive_guard",
        Description = "A restricted corridor is off-limits. Talk your way past the door guard?",
        SuccessText = "Your invented credentials get you through. What's inside isn't any of their business.",
        FailText = "The guard's comm crackles. 'Negative on that name.' Their blaster comes up.",
        Skill = SkillType.Deceive, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 90, UpgradePointReward = 1,
        CombatNpcOnFail = NPCData.Stormtrooper,
    };

    // =========================================================
    // TATOOINE_ESPA_HANGAR
    // =========================================================
    public static SkillCheckEvent HangarShipInspect => new()
    {
        Id = "hangar_ship_inspect",
        Description = "A mechanic waves you over. 'Hey, you look like you know ships. Can you take a look at this hyperdrive motivator?'",
        SuccessText = "You diagnose the fault in minutes — a misaligned fuel injector. The mechanic is impressed and pays you for the consult.",
        FailText = "You poke around but can't find the issue. The mechanic thanks you for trying.",
        Skill = SkillType.Vehicles, Difficulty = CheckDifficulty.Easy,
        TargetNumber = 8, CreditReward = 30, UpgradePointReward = 1,
    };
    public static SkillCheckEvent HangarBountyPoster => new()
    {
        Id = "hangar_bounty_poster",
        Description = "A bounty terminal displays fresh warrants. Can you cross-reference one with recent chatter?",
        SuccessText = "You match a face with a name. You claim the finder's fee from the bounty office.",
        FailText = "Every face looks the same to you. Your guess is wrong.",
        Skill = SkillType.Galaxy, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 12, CreditReward = 60, UpgradePointReward = 1,
        Repeatable = true,
    };
    public static SkillCheckEvent HangarGuardPayoff => new()
    {
        Id = "hangar_guard_payoff",
        Description = "A hangar guard blocks access to a locked private bay. A bribe might open doors.",
        SuccessText = "Credits change hands. The guard looks the other way — whatever's in that bay is now fair game.",
        FailText = "The guard reports the bribe attempt. Private security responds.",
        Skill = SkillType.Persuade, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 100, UpgradePointReward = 1,
        CreditPenalty = 40,
        CombatNpcOnFail = NPCData.BountyHunter,
    };
    public static SkillCheckEvent HangarStowaway => new()
    {
        Id = "hangar_stowaway",
        Description = "You spot an unattended cargo freighter preparing to launch. Slip aboard and see what falls out?",
        SuccessText = "You rifle through cargo during launch, pocketing a few portable valuables, and slip off at the first dock.",
        FailText = "The crew finds you in the cargo hold mid-flight. They are not amused.",
        Skill = SkillType.Hide, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 130, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.PirateThugs,
    };
    public static SkillCheckEvent HangarDroidRepair => new()
    {
        Id = "hangar_droid_repair",
        Description = "A damaged loading droid malfunctions, arm swinging wildly. Shut it down before someone gets hurt?",
        SuccessText = "You jack into its port and force a graceful shutdown. The hangar master pays for the save.",
        FailText = "The droid's arm swings wide — and connects.",
        FailPenaltyText = "You limp away with a bruised rib and medical bills.",
        Skill = SkillType.Droids, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 55, UpgradePointReward = 1,
        CreditPenalty = 30,
    };
    public static SkillCheckEvent HangarGravLiftMisaligned => new()
    {
        Id = "hangar_grav_lift",
        Description = "A cargo grav-lift is misaligned. Realign it under field conditions?",
        SuccessText = "You re-seat the gravitic resonators. The hangar master pays on the spot.",
        FailText = "Your realignment makes things worse. The foreman waves another tech over.",
        Skill = SkillType.Armament, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 12, CreditReward = 65, UpgradePointReward = 1,
    };

    // =========================================================
    // DERELICT_INTERIOR
    // =========================================================
    public static SkillCheckEvent DerelictForceEcho => new()
    {
        Id = "derelict_force_echo",
        Description = "The Force ripples here — an echo of something terrible. If you open yourself to it, you might learn what happened on this station.",
        SuccessText = "Visions flood your mind: a research project, a breach, something from beyond. The knowledge strengthens your connection to the Force.",
        FailText = "The vision overwhelms you. You gasp and stumble, the echo fading before you can grasp it.",
        Skill = SkillType.Sense, Difficulty = CheckDifficulty.Challenging,
        TargetNumber = 22, CreditReward = 0, UpgradePointReward = 3,
    };
    public static SkillCheckEvent DerelictDataCoreSalvage => new()
    {
        Id = "derelict_data_core",
        Description = "The station's central data core is damaged but not destroyed. Recover what you can.",
        SuccessText = "You pull fragments of research logs — worth a fortune to the right xenobiology buyer.",
        FailText = "The core discharges as you interface. Sparks, smoke, and nothing recovered.",
        FailPenaltyText = "Your slicing kit is ruined.",
        Skill = SkillType.Computers, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 180, UpgradePointReward = 2,
        CreditPenalty = 70,
    };
    public static SkillCheckEvent DerelictSuitExposure => new()
    {
        Id = "derelict_suit_exposure",
        Description = "A section of the hull is breached. You'll need to traverse vacuum on sheer willpower — no suit.",
        SuccessText = "Lungs burning, heart hammering, you make it across before suffocation sets in.",
        FailText = "You collapse gasping just as you reach the airlock.",
        FailPenaltyText = "Capillary damage from decompression requires expensive bacta treatment.",
        Skill = SkillType.Stamina, Difficulty = CheckDifficulty.Challenging,
        TargetNumber = 21, CreditReward = 0, UpgradePointReward = 3,
        CreditPenalty = 100,
    };
    public static SkillCheckEvent DerelictCreatureTrack => new()
    {
        Id = "derelict_creature_track",
        Description = "Something left these claw marks. Track it to its lair — or away from it?",
        SuccessText = "You read the spoor and circle back around, avoiding the creature's den entirely.",
        FailText = "You walk into the lair. Whatever's in there is still alive.",
        Skill = SkillType.Search, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 0, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.Diagnoga,
    };
    public static SkillCheckEvent DerelictDarkArtifact => new()
    {
        Id = "derelict_dark_artifact",
        Description = "A pulsing obsidian artifact rests on a plinth. Its aura is... wrong. Resist reaching for it?",
        SuccessText = "You steel your mind and take it wrapped in shielded cloth. Collectors pay dearly for such cursed things.",
        FailText = "You touch it bare-handed. Dark whispers pour into your mind.",
        FailPenaltyText = "You lose a Force Point if you had any to lose.",
        Skill = SkillType.Control, Difficulty = CheckDifficulty.Challenging,
        TargetNumber = 22, CreditReward = 250, UpgradePointReward = 3,
    };
    public static SkillCheckEvent DerelictSurgicalBay => new()
    {
        Id = "derelict_surgical_bay",
        Description = "The abandoned surgical bay still has sealed bacta crates. Bypass the emergency lockdown?",
        SuccessText = "You override the lockdown. A pallet of fresh bacta is yours for the taking.",
        FailText = "The lockdown escalates. Automated defense turrets track you.",
        Skill = SkillType.Medicine, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 150, UpgradePointReward = 2,
    };

    // =========================================================
    // TATOOINE_ORBIT
    // =========================================================
    public static SkillCheckEvent OrbitSensorSweep => new()
    {
        Id = "orbit_sensor_sweep",
        Description = "Your sensors detect a faint signal — could be a distress beacon or a salvage marker. Want to try to isolate it?",
        SuccessText = "You lock onto the signal. It's a tagged cargo pod — someone's stashed emergency supplies out here. Finders keepers.",
        FailText = "The signal fades into background noise. Whatever it was, it's gone now.",
        Skill = SkillType.Sensors, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 55, UpgradePointReward = 1,
    };
    public static SkillCheckEvent OrbitFuelScavenge => new()
    {
        Id = "orbit_fuel_scavenge",
        Description = "A derelict fuel depot drifts off the main traffic lane. Siphon a tank?",
        SuccessText = "Your EVA work pays off. Free fuel — saves you credits at the next dock.",
        FailText = "The tank's pressurized wrong. A plume of coolant shoves you out of alignment.",
        FailPenaltyText = "You burn extra fuel recovering — and it costs you at resupply.",
        Skill = SkillType.Vehicles, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 60, UpgradePointReward = 1,
        CreditPenalty = 40, Repeatable = true,
    };
    public static SkillCheckEvent OrbitImperialHailing => new()
    {
        Id = "orbit_imperial_hailing",
        Description = "An Imperial frigate demands transponder verification. Spoof your signal?",
        SuccessText = "Your signal reads as a diplomatic courier. The frigate apologizes and moves on.",
        FailText = "The spoof fails. 'Prepare to be boarded.'",
        Skill = SkillType.Deceive, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 0, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.ImperialOfficer,
    };
    public static SkillCheckEvent OrbitDebrisField => new()
    {
        Id = "orbit_debris_field",
        Description = "A recent battle left a debris field. Pick through for salvage?",
        SuccessText = "You pull a functioning sensor array from the wreckage. Fence it for a tidy sum.",
        FailText = "A sharp fragment punctures your hull. Minor — but enough to matter.",
        FailPenaltyText = "Hull patch costs you dearly at the next dock.",
        Skill = SkillType.Pilot, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 95, UpgradePointReward = 1,
        CreditPenalty = 60, Repeatable = true,
    };
    public static SkillCheckEvent OrbitCommArray => new()
    {
        Id = "orbit_comm_array",
        Description = "A private comm array broadcasts encrypted chatter. Crack the encryption?",
        SuccessText = "The traffic is corporate espionage. You sell the decrypt to a rival conglomerate.",
        FailText = "The encryption triggers a trace. Your transponder is flagged.",
        Skill = SkillType.Computers, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 140, UpgradePointReward = 2,
    };
    public static SkillCheckEvent OrbitSpaceWalker => new()
    {
        Id = "orbit_spacewalker",
        Description = "A distressed EVA worker drifts past, tether severed. Maneuver to rescue?",
        SuccessText = "Careful piloting brings you alongside. The worker's employer wires you a reward.",
        FailText = "You miss the approach. They drift into the shipping lane — and are scooped up by someone else.",
        Skill = SkillType.Pilot, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 120, UpgradePointReward = 2,
    };

    // =========================================================
    // DEEP_SPACE
    // =========================================================
    public static SkillCheckEvent DeepSpaceNavHazard => new()
    {
        Id = "deep_space_nav_hazard",
        Description = "The asteroid field ahead is thick. You'll need to plot a precise course to navigate safely.",
        SuccessText = "Your calculations are flawless. You thread through the debris field and discover a hidden salvage depot on the far side.",
        FailText = "A miscalculation sends you skimming an asteroid. Minor hull damage — no reward, just relief.",
        FailPenaltyText = "Hull repair costs you credits.",
        Skill = SkillType.Astrogation, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 80, UpgradePointReward = 2,
        CreditPenalty = 50,
    };
    public static SkillCheckEvent DeepSpacePirateSignal => new()
    {
        Id = "deep_space_pirate_signal",
        Description = "A weak signal pings your comm: 'Requesting aid, civilian vessel in distress.' Could be legit. Could be a trap.",
        SuccessText = "You recognize the signal pattern as a known pirate lure. You veer off and salvage the ambush decoy instead.",
        FailText = "You answer the call. The 'civilian' ship is crewed by pirates.",
        Skill = SkillType.Sensors, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 70, UpgradePointReward = 1,
        CombatNpcOnFail = NPCData.PirateThugs, Repeatable = true,
    };
    public static SkillCheckEvent DeepSpaceAncientWreck => new()
    {
        Id = "deep_space_ancient_wreck",
        Description = "A millennia-old capital ship hulk drifts through deep space. Board for artifacts?",
        SuccessText = "You find a pre-Republic artifact. Museums — and collectors — will bid high.",
        FailText = "The hulk's gravity wells are unstable. You barely escape decompression.",
        FailPenaltyText = "Your suit needs replacement.",
        Skill = SkillType.Xenology, Difficulty = CheckDifficulty.Challenging,
        TargetNumber = 21, CreditReward = 300, UpgradePointReward = 3,
        CreditPenalty = 80,
    };
    public static SkillCheckEvent DeepSpaceRogueJump => new()
    {
        Id = "deep_space_rogue_jump",
        Description = "Your sensors spot a hyperlane anomaly — a non-standard jump point. Risk a micro-jump to see what's on the other side?",
        SuccessText = "You emerge near a rebel resupply depot. You refuel free and they pay for news.",
        FailText = "The anomaly collapses. You lurch back, disoriented, navcomputer scrambled.",
        FailPenaltyText = "Realignment fees at the next astrogation port.",
        Skill = SkillType.Astrogation, Difficulty = CheckDifficulty.Challenging,
        TargetNumber = 21, CreditReward = 180, UpgradePointReward = 3,
        CreditPenalty = 70,
    };
    public static SkillCheckEvent DeepSpaceCreatureEncounter => new()
    {
        Id = "deep_space_creature",
        Description = "A massive space-faring creature — mynock cluster or worse — clings to your hull. Shake it off?",
        SuccessText = "Strobing your shields at the right frequency drives them off.",
        FailText = "They eat through your shield generator's power coupling. You'll need replacement parts.",
        Skill = SkillType.Pilot, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 0, UpgradePointReward = 1,
        CreditPenalty = 100, Repeatable = true,
    };
    public static SkillCheckEvent DeepSpaceSalvageBeacon => new()
    {
        Id = "deep_space_salvage_beacon",
        Description = "A tagged salvage beacon marks an abandoned cargo container. First to claim keeps it.",
        SuccessText = "You dock, breach the container, and recover valuable tech. Finder's rights.",
        FailText = "Another salvager arrives before you break in. They don't take kindly to competition.",
        Skill = SkillType.Pilot, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 110, UpgradePointReward = 1,
        CombatNpcOnFail = NPCData.BountyHunter, Repeatable = true,
    };

    // =========================================================
    // MOS_ENTHA
    // =========================================================
    public static SkillCheckEvent MosEnthaHuttTax => new()
    {
        Id = "mos_entha_hutt_tax",
        Description = "A Hutt enforcer demands the 'newcomer tax' — 75 credits, or you prove you're not worth shaking down.",
        SuccessText = "You talk circles around the enforcer until they decide you're more trouble than you're worth.",
        FailText = "They take the tax anyway. The hand on your wrist leaves a bruise.",
        Skill = SkillType.Deceive, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 0, UpgradePointReward = 1,
        CreditPenalty = 75,
    };
    public static SkillCheckEvent MosEnthaSpiceRunner => new()
    {
        Id = "mos_entha_spice_run",
        Description = "A Hutt lieutenant offers a quick spice delivery. 'No questions asked' pays well but turns ugly if intercepted.",
        SuccessText = "You deliver without incident. The lieutenant pays in untraceable credits.",
        FailText = "Imperial patrol intercepts you. You ditch the cargo — but the Hutts want answers.",
        FailPenaltyText = "You owe the Hutts for the lost spice.",
        Skill = SkillType.Hide, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 200, UpgradePointReward = 2,
        CreditPenalty = 120,
        CombatNpcOnFail = NPCData.PirateThugs,
    };
    public static SkillCheckEvent MosEnthaStreetFight => new()
    {
        Id = "mos_entha_street_fight",
        Description = "An underground brawl ring accepts your entry. 40 credit buy-in for the purse.",
        SuccessText = "Three bouts later you're standing, purse heavier.",
        FailText = "You're laid flat in the second round. Medical fees and lost buy-in.",
        FailPenaltyText = "Patching up costs extra.",
        Skill = SkillType.Brawl, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 140, UpgradePointReward = 1,
        CreditPenalty = 40, Repeatable = true,
    };
    public static SkillCheckEvent MosEnthaRepoJob => new()
    {
        Id = "mos_entha_repo",
        Description = "A Hutt loan-shark needs a speeder 'repossessed' from a debtor who's armed.",
        SuccessText = "You slip the speeder away without being seen. The Hutt pays on delivery.",
        FailText = "The debtor catches you at the door.",
        Skill = SkillType.Steal, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 180, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.PirateThugs,
    };
    public static SkillCheckEvent MosEnthaBrokenAC => new()
    {
        Id = "mos_entha_broken_cooling",
        Description = "A cantina's cooling unit has failed in 45° heat. The owner will pay to get it running.",
        SuccessText = "You diagnose and swap the refrigerant coil. The owner pays double for the speed.",
        FailText = "Your 'fix' vents coolant everywhere. The owner takes the cost out of your advance.",
        FailPenaltyText = "You're on the hook for the ruined coolant.",
        Skill = SkillType.Armament, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 85, UpgradePointReward = 1,
        CreditPenalty = 45,
    };
    public static SkillCheckEvent MosEnthaInformantMeet => new()
    {
        Id = "mos_entha_informant",
        Description = "A cloaked Bith says they have rebel intel to sell. 50 credits for the datachip.",
        SuccessText = "The chip is legitimate. You flip it to a sympathetic contact for quadruple the buy-in.",
        FailText = "The chip is blank. The Bith is gone when you look up.",
        Skill = SkillType.Streetwise, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 200, UpgradePointReward = 2,
        CreditPenalty = 50,
    };

    // =========================================================
    // TATOOINE_ENTHA_HUTT_COMPOUND
    // =========================================================
    public static SkillCheckEvent HuttCompoundAudience => new()
    {
        Id = "hutt_compound_audience",
        Description = "The lieutenant offers a 'private audience' — pay 100 credits for a face-to-face or prove your worth with words.",
        SuccessText = "You impress the lieutenant. They offer you contract work rather than taking your money.",
        FailText = "You bore them. They take the entry fee anyway.",
        Skill = SkillType.Persuade, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 0, UpgradePointReward = 2,
        CreditPenalty = 100,
    };
    public static SkillCheckEvent HuttCompoundSliceVault => new()
    {
        Id = "hutt_compound_slice_vault",
        Description = "The Hutt's vault is rumored to hold millions. Slice the locking mechanism?",
        SuccessText = "The vault opens. You grab what you can carry — and run.",
        FailText = "Internal sensors trip. Hutt enforcers flood the chamber.",
        Skill = SkillType.Computers, Difficulty = CheckDifficulty.Challenging,
        TargetNumber = 24, CreditReward = 500, UpgradePointReward = 3,
        CombatNpcOnFail = NPCData.BountyHunter,
    };
    public static SkillCheckEvent HuttCompoundRancorPen => new()
    {
        Id = "hutt_compound_rancor_pen",
        Description = "The compound's rancor pen is briefly unattended. Sneak past for access to the back passages?",
        SuccessText = "You time the beast's breathing and slip past unnoticed.",
        FailText = "The rancor's head turns. Its eyes lock on you.",
        Skill = SkillType.Hide, Difficulty = CheckDifficulty.Challenging,
        TargetNumber = 22, CreditReward = 0, UpgradePointReward = 3,
        CombatNpcOnFail = NPCData.Diagnoga,
    };
    public static SkillCheckEvent HuttCompoundDebtSettle => new()
    {
        Id = "hutt_compound_debt_settle",
        Description = "Someone you don't know insists you owe the Hutts 200 credits. Talk your way out?",
        SuccessText = "You produce a 'receipt' from memory. The collector shrugs and moves on.",
        FailText = "The collector is not convinced. 'Pay, or bleed.'",
        FailPenaltyText = "You pay the full debt.",
        Skill = SkillType.Deceive, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 0, UpgradePointReward = 1,
        CreditPenalty = 200,
    };
    public static SkillCheckEvent HuttCompoundEntertainerEscape => new()
    {
        Id = "hutt_compound_entertainer",
        Description = "A Twi'lek dancer whispers she'll pay handsomely for help escaping the compound.",
        SuccessText = "You smuggle her out under laundry. She transfers the promised payment from a secret account.",
        FailText = "You're caught at the back gate. Her owner is furious.",
        Skill = SkillType.Hide, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 300, UpgradePointReward = 3,
        CombatNpcOnFail = NPCData.BountyHunter,
    };
    public static SkillCheckEvent HuttCompoundPoisonTaste => new()
    {
        Id = "hutt_compound_poison_taste",
        Description = "You're offered 'hospitality' — a dish you don't recognize. Refusing is an insult. Taste it without dying?",
        SuccessText = "You identify the toxic component and fake enjoyment while palming a sample. The lieutenant is charmed.",
        FailText = "You swallow. Something burns.",
        FailPenaltyText = "The antidote costs you.",
        Skill = SkillType.Stamina, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 90, UpgradePointReward = 2,
        CreditPenalty = 70,
    };

    // =========================================================
    // BEGGARS_CANYON
    // =========================================================
    public static SkillCheckEvent BeggarsCanyonDeadManTurn => new()
    {
        Id = "beggars_canyon_dead_man_turn",
        Description = "A group of local farmhands notice your vehicle. They're placing bets that you can't make it through Dead Man's Turn.",
        SuccessText = "Your calculations are flawless, decelerating and air-braking your vehicle to make it the bend.",
        FailText = "Your vehicle scuffs against the canyon walls, scraping to a halt.",
        FailPenaltyText = "Vehicle repairs will cost you.",
        Skill = SkillType.Drive, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 11, CreditReward = 20, UpgradePointReward = 1,
        CreditPenalty = 40,
    };
    public static SkillCheckEvent BeggarsCanyonDiabloCut => new()
    {
        Id = "beggars_canyon_diablo_cut",
        Description = "Air whistles past the aerofoils and an even more daring turn lies directly ahead.",
        SuccessText = "Your calculations are flawless, decelerating and air-braking your vehicle to make it the bend.",
        FailText = "Your vehicle scuffs against the canyon walls, scraping to a halt.",
        FailPenaltyText = "Vehicle repairs will cost you.",
        Skill = SkillType.Drive, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 40, UpgradePointReward = 2,
        CreditPenalty = 60,
    };
    public static SkillCheckEvent BeggarsCanyonTuskenSnipe => new()
    {
        Id = "beggars_canyon_tusken_snipe",
        Description = "Movement on a high ridge — Tusken scouts with rifles. Spot and evade?",
        SuccessText = "You read the light glint and punch the throttle before their shot lines up.",
        FailText = "You miss the glint. Gaderffii-slug fire rakes your hull.",
        Skill = SkillType.Search, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 0, UpgradePointReward = 1,
        CombatNpcOnFail = NPCData.TuskenRaider, Repeatable = true,
    };
    public static SkillCheckEvent BeggarsCanyonWreckDive => new()
    {
        Id = "beggars_canyon_wreck_dive",
        Description = "A crashed T-16 lies at the bottom of a crevasse. Salvageable parts are visible.",
        SuccessText = "You climb down, strip the best parts, and climb back up with valuable salvage.",
        FailText = "Loose rock gives way. You tumble halfway down and scrape yourself raw.",
        FailPenaltyText = "Medical costs eat into your savings.",
        Skill = SkillType.Athletics, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 110, UpgradePointReward = 1,
        CreditPenalty = 50,
    };
    public static SkillCheckEvent BeggarsCanyonUpdraftRide => new()
    {
        Id = "beggars_canyon_updraft",
        Description = "Locals say you can skip three bends by riding the thermal updrafts. The trick is not stalling out.",
        SuccessText = "You catch the updraft perfectly. Racers at the finish line applaud and hand you a wager payout.",
        FailText = "You stall halfway up and drop hard.",
        FailPenaltyText = "Repairs aren't cheap.",
        Skill = SkillType.Pilot, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 90, UpgradePointReward = 2,
        CreditPenalty = 80, Repeatable = true,
    };
    public static SkillCheckEvent BeggarsCanyonHermitContract => new()
    {
        Id = "beggars_canyon_hermit",
        Description = "An old hermit camps at a canyon bend, wants a strange relic retrieved from higher up. Pays well.",
        SuccessText = "You return with the relic. The hermit pays in a bundle of Republic-era credits.",
        FailText = "You can't find the relic and return empty-handed. The hermit is disappointed.",
        Skill = SkillType.Survival, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 100, UpgradePointReward = 1,
    };

    // =========================================================
    // MOSPIC_HIGH_RANGE
    // =========================================================
    public static SkillCheckEvent TatooineTuskenchasePilot => new()
    {
        Id = "tatooine_tuskenchase_pilot",
        Description = "Ambush! Tusken Raider pursue you across the open dune flats. They close fast on your position.",
        SuccessText = "Your lead increase so much that the Tuskens cannot close the gap. They break off pursuit after two kilometers.",
        FailText = "A pursuing bantha clips your rear vehicle vane.",
        Skill = SkillType.Pilot, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 18, CreditReward = 0, UpgradePointReward = 2,
        Repeatable = true,
        CombatNpcOnFail = NPCData.TuskenRaider,
    };
    public static SkillCheckEvent TatooineSarlaccAthletics => new()
    {
        Id = "tatooine_sarlacc_athletics",
        Description = "Sandstone overhangs around the sarlacc pit give way. Climbing toward the highest ledge is the only way to avoid falling.",
        SuccessText = "You pull yourself up onto the ledge. Too easy.",
        FailText = "You slip on loose stone. The sarlacc tentacles stretch upward.",
        Skill = SkillType.Athletics, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 0, UpgradePointReward = 2,
        Repeatable = false,
        CombatNpcOnFail = NPCData.Diagnoga,
    };
    public static SkillCheckEvent TatooineSandstormSurvival => new()
    {
        Id = "tatooine_sandstorm_survival",
        Description = "Navigate a total red-out sandstorm using macrobinoculars.",
        SuccessText = "You emerge from the sandstorm, equipment intact and party accounted for.",
        FailText = "You lose two hours trying to find your gear.",
        FailPenaltyText = "Some equipment is lost.",
        Skill = SkillType.Survival, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 0, UpgradePointReward = 2,
        Repeatable = true, CreditPenalty = 40,
    };
    public static SkillCheckEvent MospicTuskenBurialRespect => new()
    {
        Id = "mospic_burial_respect",
        Description = "Your path crosses a Tusken burial cairn. Traditions demand the right gestures — or consequences.",
        SuccessText = "You honor the cairn correctly. An observing Tusken elder permits your passage.",
        FailText = "You make the wrong gesture. Shadows on the ridge stand up.",
        Skill = SkillType.Xenology, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 0, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.TuskenRaider,
    };
    public static SkillCheckEvent MospicAncientPictograph => new()
    {
        Id = "mospic_pictograph",
        Description = "Ancient Tusken pictographs crowd a sheltered overhang. Decipher their meaning?",
        SuccessText = "You decode a star map — a hidden moisture reserve to the east. You profit from selling the location.",
        FailText = "The symbols defy you. You note their position but learn nothing.",
        Skill = SkillType.Xenology, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 90, UpgradePointReward = 1,
    };
    public static SkillCheckEvent MospicCliffClimb => new()
    {
        Id = "mospic_cliff_climb",
        Description = "A free-climb up a red rock plateau promises a shortcut. The exposure is substantial.",
        SuccessText = "You reach the top faster than going around. The shortcut saves hours of travel.",
        FailText = "You fall, catching yourself against an outcrop. Gear tears loose.",
        FailPenaltyText = "Lost gear will cost you to replace.",
        Skill = SkillType.Athletics, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 60, UpgradePointReward = 2,
        CreditPenalty = 60,
    };

    // =========================================================
    // RODIA_ORBIT
    // =========================================================
    public static SkillCheckEvent RodiaOrbitDockingQueue => new()
    {
        Id = "rodia_orbit_docking_queue",
        Description = "The docking queue is jammed. Negotiate priority clearance?",
        SuccessText = "You charm the comms officer into letting you skip ahead.",
        FailText = "The officer takes offense and pushes you to the back of the line.",
        Skill = SkillType.Persuade, Difficulty = CheckDifficulty.Easy,
        TargetNumber = 9, CreditReward = 30, UpgradePointReward = 1,
        Repeatable = true,
    };
    public static SkillCheckEvent RodiaOrbitBountyHandoff => new()
    {
        Id = "rodia_orbit_bounty_handoff",
        Description = "A Greedo-lookalike Rodian wants to transfer a sealed bounty crate to your ship. No manifest. Good pay.",
        SuccessText = "You secure the crate correctly. The Rodian pays on delivery — no questions, no tails.",
        FailText = "Your clumsy handling breaks the seal. What's inside is... alive.",
        FailPenaltyText = "You owe the Rodian for the compromised cargo.",
        Skill = SkillType.Vehicles, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 150, UpgradePointReward = 2,
        CreditPenalty = 100,
        CombatNpcOnFail = NPCData.Diagnoga,
    };
    public static SkillCheckEvent RodiaOrbitCustomsSlip => new()
    {
        Id = "rodia_orbit_customs_slip",
        Description = "Rodian customs are slower than Imperial but just as thorough. Slip unregistered cargo through?",
        SuccessText = "Your manifest is clean enough to fool casual inspection. You're waved through.",
        FailText = "A thorough inspector finds the unregistered item.",
        FailPenaltyText = "The fine bites.",
        Skill = SkillType.Deceive, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 60, UpgradePointReward = 1,
        CreditPenalty = 80,
    };
    public static SkillCheckEvent RodiaOrbitSwampSensors => new()
    {
        Id = "rodia_orbit_swamp_sensors",
        Description = "Ground-level swamp interference garbles your sensor feed. Tune them for the local atmosphere?",
        SuccessText = "You filter the interference and spot a downed courier drone — salvage rights are yours.",
        FailText = "Your retune makes things worse. You miss a navigation buoy.",
        FailPenaltyText = "A traffic fine for the buoy miss.",
        Skill = SkillType.Sensors, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 80, UpgradePointReward = 1,
        CreditPenalty = 35,
    };
    public static SkillCheckEvent RodiaOrbitBountyHunterDuel => new()
    {
        Id = "rodia_orbit_hunter_duel",
        Description = "A rival bounty hunter hails you, demands you stand down from a contract. Stand firm with words?",
        SuccessText = "Your reputation is enough. They back off, grudgingly.",
        FailText = "They don't bluff. Weapons arm.",
        Skill = SkillType.Willpower, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 100, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.BountyHunter,
    };
    public static SkillCheckEvent RodiaOrbitTraderTip => new()
    {
        Id = "rodia_orbit_trader_tip",
        Description = "A trader wants a market rumor verified before they commit a cargo run. Sell them good intel?",
        SuccessText = "You recall the right galactic market trends. They pay for the tip.",
        FailText = "Your memory fails. They buy from a competitor instead.",
        Skill = SkillType.Galaxy, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 12, CreditReward = 70, UpgradePointReward = 1,
    };

    // =========================================================
    // CORUSCANT
    // =========================================================
    public static SkillCheckEvent CoruscantDockingCustoms => new()
    {
        Id = "coruscant_dock_customs",
        Description = "An Imperial customs droid demands an extra inspection of your manifest.",
        SuccessText = "You speak the right phrases in the right tone. The droid stamps your manifest and waves you through.",
        FailText = "The droid flags your transponder for re-scan. A long, expensive delay follows.",
        Skill = SkillType.Persuade, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 80, UpgradePointReward = 1, CreditPenalty = 30,
    };
    public static SkillCheckEvent CoruscantDockingSliceManifest => new()
    {
        Id = "coruscant_dock_slice",
        Description = "An unattended manifest console tempts you. A few quiet keystrokes could rewrite a record.",
        SuccessText = "You slide a falsified entry into the Imperial customs ledger. Useful intel for the Rebellion.",
        FailText = "The console flags your access. You back away before alarms trip.",
        Skill = SkillType.Computers, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 120, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.Stormtrooper,
    };
    public static SkillCheckEvent CoruscantDockingScroungeParts => new()
    {
        Id = "coruscant_dock_scrounge",
        Description = "Imperial salvage carts overflow with discarded but functional parts. Nobody is watching.",
        SuccessText = "You quietly pocket a high-grade power cell. Worth a fortune on the gray market.",
        FailText = "A maintenance droid catches you mid-grab. You slink away empty-handed.",
        Skill = SkillType.Steal, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 12, CreditReward = 60, UpgradePointReward = 1, Repeatable = true,
    };

    public static SkillCheckEvent CoruscantVerityEavesdrop => new()
    {
        Id = "coruscant_verity_eavesdrop",
        Description = "Two senatorial aides argue urgently behind a column. Their words could be valuable.",
        SuccessText = "You memorize a half-formed Imperial proposal. The Rebellion will want this.",
        FailText = "One aide spots your silhouette. They go silent and sweep the area.",
        Skill = SkillType.Hide, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 90, UpgradePointReward = 1,
    };
    public static SkillCheckEvent CoruscantVerityLoyaltyPledge => new()
    {
        Id = "coruscant_verity_pledge",
        Description = "A loyalty pledge terminal blinks: 'STEP FORWARD TO REAFFIRM.' A propaganda opportunity?",
        SuccessText = "You spoof a citizen's ID and lodge a falsely glowing pledge. The Imperials log it without suspicion.",
        FailText = "The terminal rejects your input and broadcasts your face to the local watch.",
        Skill = SkillType.Deceive, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 100, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.ImperialOfficer,
    };
    public static SkillCheckEvent CoruscantVerityProbeDroid => new()
    {
        Id = "coruscant_verity_probedroid",
        Description = "A probe droid drifts low overhead. With the right signal you could feed it a corrupted log.",
        SuccessText = "You ping the droid with a forged ISB packet. It returns to base with garbage data.",
        FailText = "The droid registers your signal as foreign and squawks an alert.",
        Skill = SkillType.Sensors, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 70, UpgradePointReward = 1,
    };

    public static SkillCheckEvent CoruscantArcologyDoctrineExam => new()
    {
        Id = "coruscant_arcology_doctrine",
        Description = "A loyalty officer corners you with surprise doctrinal questions. You must answer convincingly.",
        SuccessText = "You recite Imperial creed flawlessly. The officer commends your patriotism and waves you through.",
        FailText = "Your phrasing falters. The officer narrows their eyes and reaches for a comm.",
        Skill = SkillType.Willpower, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 50, UpgradePointReward = 1,
        CombatNpcOnFail = NPCData.ImperialOfficer,
    };
    public static SkillCheckEvent CoruscantArcologyCadetIntel => new()
    {
        Id = "coruscant_arcology_cadet",
        Description = "A nervous Sub-Adult Group cadet hesitates near you. They might know something useful — if you can read them.",
        SuccessText = "You coax a quiet confession out of them: a junior officer is sympathetic to the Rebellion.",
        FailText = "Your questions ring false. The cadet panics and flags you for questioning.",
        Skill = SkillType.Persuade, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 110, UpgradePointReward = 2,
    };
    public static SkillCheckEvent CoruscantArcologyForceWhispers => new()
    {
        Id = "coruscant_arcology_force",
        Description = "A dark adept's presence has left a residue here. Something probes the edges of your awareness.",
        SuccessText = "You ground yourself in the Force and let the probe slide past. Your thoughts stay your own.",
        FailText = "The probe finds purchase. A shadow registers your presence and files it away.",
        Skill = SkillType.Control, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 0, UpgradePointReward = 2,
    };

    public static SkillCheckEvent CoruscantRegisterRecordSlice => new()
    {
        Id = "coruscant_register_slice",
        Description = "An unattended registry terminal could be coaxed into editing a citizen's record.",
        SuccessText = "You bury a Rebel asset's name beneath three layers of bureaucratic noise. Untraceable now.",
        FailText = "The terminal rejects your forged credentials and silently flags your biometrics.",
        Skill = SkillType.Computers, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 18, CreditReward = 200, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.ImperialOfficer,
    };
    public static SkillCheckEvent CoruscantRegisterClerkBribe => new()
    {
        Id = "coruscant_register_bribe",
        Description = "A junior registrar looks tired and underpaid. A discreet credit chit might work miracles.",
        SuccessText = "The clerk pockets your chit and quietly purges three records. They never look up.",
        FailText = "The clerk stiffens and presses a silent alarm under the desk.",
        Skill = SkillType.Persuade, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 80, UpgradePointReward = 1, CreditPenalty = 50,
    };
    public static SkillCheckEvent CoruscantRegisterAuditTrail => new()
    {
        Id = "coruscant_register_audit",
        Description = "A surveillance node logs every entry and exit. If you could confuse its audit trail...",
        SuccessText = "You cross-thread the audit log so cleanly that the next ISB review will discard the day entirely.",
        FailText = "Your tampering trips a low-level integrity check. A quiet alert is logged.",
        Skill = SkillType.Computers, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 150, UpgradePointReward = 2,
    };

    public static SkillCheckEvent CoruscantFederalRoyalGuardStare => new()
    {
        Id = "coruscant_federal_guard",
        Description = "A Royal Guard's gaze tracks you. A wrong reaction will draw a crimson saber.",
        SuccessText = "You hold their gaze with calm respect. They look away first — barely.",
        FailText = "You flinch. The guard takes a half-step forward; you decide not to test them further.",
        Skill = SkillType.Willpower, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 0, UpgradePointReward = 2,
    };
    public static SkillCheckEvent CoruscantFederalSenatorFavor => new()
    {
        Id = "coruscant_federal_senator",
        Description = "A senator pauses to take stock of you. A few well-chosen words could win a small favor.",
        SuccessText = "You charm the senator into recommending you for a minor courier contract — quiet credits, off the books.",
        FailText = "Your familiarity strikes the senator as forward. Their bodyguards close ranks.",
        Skill = SkillType.Persuade, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 130, UpgradePointReward = 1,
    };
    public static SkillCheckEvent CoruscantFederalDroneIntercept => new()
    {
        Id = "coruscant_federal_drone",
        Description = "A surveillance drone sweeps low. Its receiver is briefly exposed.",
        SuccessText = "You graft a parasite packet onto the drone's outbound channel. It will report nothing for the next hour.",
        FailText = "The drone catches a glimpse of your splice attempt and pulls high in alarm.",
        Skill = SkillType.Computers, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 90, UpgradePointReward = 1,
    };

    public static SkillCheckEvent CoruscantPalaceForceEcho => new()
    {
        Id = "coruscant_palace_force",
        Description = "Something old watches from the upper galleries. The Force coils unpleasantly here.",
        SuccessText = "You shield your presence and slip out of the dark watcher's notice. A small mercy.",
        FailText = "Whatever it is touches your mind, registers, and lets you go — for now. You feel marked.",
        Skill = SkillType.Sense, Difficulty = CheckDifficulty.Challenging,
        TargetNumber = 21, CreditReward = 0, UpgradePointReward = 3,
    };
    public static SkillCheckEvent CoruscantPalaceDataCourier => new()
    {
        Id = "coruscant_palace_courier",
        Description = "A nervous adjutant drops a sealed datacard. They turn back, scanning the floor.",
        SuccessText = "You scoop it up and deliver it to your contact instead. The Rebellion thanks you.",
        FailText = "The adjutant catches you handing it back. Their relief turns to suspicion.",
        Skill = SkillType.Steal, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 250, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.ImperialOfficer,
    };
    public static SkillCheckEvent CoruscantPalaceInquisitorAvoid => new()
    {
        Id = "coruscant_palace_inquisitor",
        Description = "An ISB Inquisitor sweeps the atrium. Their gaze passes over the crowd like a saber.",
        SuccessText = "You compose your thoughts to nothing and let them slide past. The Inquisitor moves on.",
        FailText = "The Inquisitor pauses. They do not look at you, but they make a small mental note of your shape.",
        Skill = SkillType.Hide, Difficulty = CheckDifficulty.Challenging,
        TargetNumber = 22, CreditReward = 0, UpgradePointReward = 3,
    };

    // =========================================================
    // BESTINE / JUNDLAND BRANCH (Tatooine)
    // =========================================================
    public static SkillCheckEvent PikaOasisFermentedFruit => new()
    {
        Id = "pika_oasis_fermentedfruit",
        Description = "A pikobi-fruit fallen from the bough has fermented in the heat. Sniff test or harvest with care?",
        SuccessText = "You time the harvest perfectly — half a dozen ripe fruits, prized as offworld delicacy.",
        FailText = "You misjudge the spore release; the fruit collapses into a pungent, useless mash.",
        Skill = SkillType.Survival, Difficulty = CheckDifficulty.Easy,
        TargetNumber = 9, CreditReward = 35, UpgradePointReward = 1, Repeatable = true,
    };
    public static SkillCheckEvent PikaOasisTuskenTruce => new()
    {
        Id = "pika_oasis_tusken",
        Description = "Tusken handlers are watering banthas at the spring. A misstep here could break the unspoken truce.",
        SuccessText = "You move with deliberate stillness, eyes lowered. The Tuskens accept your presence and ignore you.",
        FailText = "A handler bristles at your approach and reaches for a gaderffii.",
        FailPenaltyText = "The truce is broken — at least for you.",
        Skill = SkillType.Agility, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 0, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.TuskenRaider,
    };
    public static SkillCheckEvent PikaOasisHiddenCache => new()
    {
        Id = "pika_oasis_cache",
        Description = "Old smuggler signs are scratched into a tree trunk near the spring. Read them right and there might be a cache nearby.",
        SuccessText = "Ten meters east, three paces from the boulder — credits and a pristine spice phial.",
        FailText = "You misread the marks and dig up only old ration wrappers.",
        Skill = SkillType.Streetwise, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 12, CreditReward = 80, UpgradePointReward = 1,
    };

    public static SkillCheckEvent BestineOutskirtsRecruiterDodge => new()
    {
        Id = "bestine_outskirts_recruiter",
        Description = "An Imperial recruiter blocks your path with a smile. 'Have you considered serving the Empire, citizen?'",
        SuccessText = "You ramble on about chronic foot pain and old injuries until they politely let you pass.",
        FailText = "Your evasion comes off as suspicious. They scan your transponder.",
        FailPenaltyText = "A 'civic infraction' fine is debited from your account.",
        Skill = SkillType.Deceive, Difficulty = CheckDifficulty.Easy,
        TargetNumber = 10, CreditReward = 20, UpgradePointReward = 1, CreditPenalty = 30,
    };
    public static SkillCheckEvent BestineOutskirtsSpeederSalvage => new()
    {
        Id = "bestine_outskirts_speeder",
        Description = "An abandoned speeder up on blocks looks salvageable. Could you scavenge a working part without being seen?",
        SuccessText = "You ease a working power converter free without spilling fluids. A nice score.",
        FailText = "You snap a fuel line; coolant hisses everywhere and the kid in the cockpit yells.",
        Skill = SkillType.Vehicles, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 12, CreditReward = 65, UpgradePointReward = 1, Repeatable = true,
    };
    public static SkillCheckEvent BestineOutskirtsDroidHelp => new()
    {
        Id = "bestine_outskirts_droid",
        Description = "A vagrant droid beeps weakly at your feet, motivator failing. A few minutes of work could save it.",
        SuccessText = "You patch the motivator with field improvisations. The droid follows you for a block in gratitude.",
        FailText = "You short the wrong relay; the droid sparks and goes still.",
        Skill = SkillType.Droids, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 12, CreditReward = 40, UpgradePointReward = 1,
    };

    public static SkillCheckEvent BestineMarketBountyList => new()
    {
        Id = "bestine_market_bountylist",
        Description = "The herald's bounty list scrolls by quickly. A close read might turn up actionable intel — or your own face.",
        SuccessText = "You memorize three open contracts and one familiar name. Useful information for the right buyer.",
        FailText = "An ISB watcher notices you reading too intently. They make a note.",
        Skill = SkillType.Search, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 90, UpgradePointReward = 1,
    };
    public static SkillCheckEvent BestineMarketBothanDropoff => new()
    {
        Id = "bestine_market_bothan",
        Description = "A Bothan in a long coat brushes past you. A slip of flimsi is now in your pocket. Read it discreetly?",
        SuccessText = "Coordinates and a delivery schedule. Whoever you hand this to next will pay generously.",
        FailText = "A patrol notices you reading and moves to investigate.",
        FailPenaltyText = "The flimsi is confiscated.",
        Skill = SkillType.Hide, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 150, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.Stormtrooper,
    };
    public static SkillCheckEvent BestineMarketHaggle => new()
    {
        Id = "bestine_market_haggle",
        Description = "A vendor wants too much for a 'pre-Imperial' rifle. The serial numbers are clearly ground off.",
        SuccessText = "You point out the alterations and the vendor's tone shifts. The price drops sharply.",
        FailText = "The vendor calls your bluff and ends the haggle. Word spreads to other stalls.",
        Skill = SkillType.Persuade, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 50, UpgradePointReward = 1, Repeatable = true,
    };

    public static SkillCheckEvent GarrisonInfiltratePerimeter => new()
    {
        Id = "garrison_perimeter",
        Description = "A walking patrol turns the corner ahead. You could blend in or duck into a side alley.",
        SuccessText = "You find a vantage that lets you note patrol timing without being seen. Useful for any future op.",
        FailText = "A trooper makes eye contact through their visor and turns toward you.",
        FailPenaltyText = "Your face is now logged in the perimeter system.",
        Skill = SkillType.Hide, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 0, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.Stormtrooper,
    };
    public static SkillCheckEvent GarrisonATSTSchematic => new()
    {
        Id = "garrison_atst",
        Description = "An AT-ST sits with an open access panel; a clever scan could capture diagnostics data.",
        SuccessText = "Your datapad pulls a clean schematic dump as you hop off the AT-ST's foot. The Rebellion will be very grateful.",
        FailText = "The walker's automated countermeasures detect your scan. Best to leave before the Empire shows up.",
        Skill = SkillType.Sensors, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 200, UpgradePointReward = 2,
    };
    public static SkillCheckEvent GarrisonForgedClearance => new()
    {
        Id = "garrison_clearance",
        Description = "You spot a clearance terminal momentarily unattended.",
        SuccessText = "You slice a temporary clearance code into your datapad — useful for one round of mischief.",
        FailText = "The terminal flags your attempt and chimes for the duty officer.",
        Skill = SkillType.Computers, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 18, CreditReward = 180, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.ImperialOfficer,
    };

    public static SkillCheckEvent SpaceportCustomsBypass => new()
    {
        Id = "spaceport_customs",
        Description = "A customs droid demands an unscheduled scan. Quick talk or quick fingers?",
        SuccessText = "You produce paperwork that satisfies the droid. It chirps approval and moves on.",
        FailText = "The droid escalates to a human officer who orders a full search.",
        FailPenaltyText = "Inspection fees are levied.",
        Skill = SkillType.Persuade, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 60, UpgradePointReward = 1, CreditPenalty = 50,
    };
    public static SkillCheckEvent SpaceportRefuelScam => new()
    {
        Id = "spaceport_refuel",
        Description = "An automated refueling station has its diagnostics panel ajar. A small adjustment could buy you free fuel.",
        SuccessText = "You spoof the meter long enough to top off your tanks for free.",
        FailText = "The system alerts the dockmaster of meter tampering. Better leave quickly.",
        Skill = SkillType.Computers, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 75, UpgradePointReward = 1, Repeatable = true,
    };
    public static SkillCheckEvent SpaceportPilotIntel => new()
    {
        Id = "spaceport_pilot",
        Description = "A bounty hunter's pilot is sharing drinks with locals near the queue. They might let slip a route.",
        SuccessText = "You match them shot-for-shot and walk away with a smuggler's cleared lane through the Rift.",
        FailText = "They sober up faster than expected and demand to know who's asking.",
        Skill = SkillType.Persuade, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 130, UpgradePointReward = 2,
    };

    public static SkillCheckEvent JundlandEastWarningBell => new()
    {
        Id = "jundland_east_bell",
        Description = "A warning bell hangs from a krayt-dragon's rib. Ringing it might keep something away — or call it forth.",
        SuccessText = "You ring the bell once, briefly. The wind responds, and a shape on a distant ridge moves on.",
        FailText = "You ring it too long. Something distant takes notice.",
        Skill = SkillType.Deceive, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 0, UpgradePointReward = 1,
    };
    public static SkillCheckEvent JundlandEastFootprints => new()
    {
        Id = "jundland_east_tracks",
        Description = "Footprints cross your path and end mid-stride. The weight pattern is wrong for a humanoid.",
        SuccessText = "You read the gait, the impressions, and the timing — a scout's notes worth selling to the right cell.",
        FailText = "You misread the spoor and follow it the wrong way for an hour.",
        Skill = SkillType.Survival, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 12, CreditReward = 60, UpgradePointReward = 1,
    };
    public static SkillCheckEvent JundlandEastBanditAmbush => new()
    {
        Id = "jundland_east_ambush",
        Description = "Pirate scouts have set a clumsy ambush in a slot canyon ahead. Might be best to slip past them",
        SuccessText = "You skirt the ambush via a high ledge and watch from above, unseen.",
        FailText = "A foot slip alerts the Pirates.",
        Skill = SkillType.Hide, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 50, UpgradePointReward = 1,
        CombatNpcOnFail = NPCData.PirateThugs,
    };

    public static SkillCheckEvent JundlandCentralPictograph => new()
    {
        Id = "jundland_central_pictograph",
        Description = "Tusken pictographs cover the canyon wall. Decoding them could reveal a hidden water source — or a war path.",
        SuccessText = "You parse the symbols: a hidden cistern lies two ridges south. Useful intel for any caravan.",
        FailText = "Your interpretation is wrong; the pictographs were a curse, and you've now read it aloud.",
        Skill = SkillType.Xenology, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 100, UpgradePointReward = 2,
    };
    public static SkillCheckEvent JundlandCentralEchoNavigate => new()
    {
        Id = "jundland_central_echo",
        Description = "Sound echoes off three different canyons at once. Picking the true direction is a challenge.",
        SuccessText = "You triangulate the source: footsteps two canyons over, moving away. You move toward them.",
        FailText = "You guess wrong and lose half a day to the maze.",
        Skill = SkillType.Search, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 40, UpgradePointReward = 1,
    };
    public static SkillCheckEvent JundlandCentralJediScrap => new()
    {
        Id = "jundland_central_jediscrap",
        Description = "A scrap of robe caught on rock. Old, sun-bleached, brown. Worth a closer look?",
        SuccessText = "You recover a fragment with a faint Jedi insignia.",
        FailText = "You snag the fabric on the rock and tear it beyond recovery.",
        Skill = SkillType.Galaxy, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 160, UpgradePointReward = 2,
    };

    public static SkillCheckEvent JundlandWestSpireClimb => new()
    {
        Id = "jundland_west_climb",
        Description = "An eroded rock spire could offer a vantage of the entire western Jundland. The climb is treacherous but rewarding.",
        SuccessText = "From the top you spot a Tusken hunting party miles east — and a single light burning in the hermit's window.",
        FailText = "You slip on loose scree and tumble to a painful stop a few meters down.",
        Skill = SkillType.Athletics, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 0, UpgradePointReward = 2,
    };
    public static SkillCheckEvent JundlandWestMeditationCairn => new()
    {
        Id = "jundland_west_cairn",
        Description = "A small stone cairn sits unnatural in this place. Sitting here, the desert hums quietly.",
        SuccessText = "You feel a current of the Force pass through. Your mind sharpens; old answers click into place.",
        FailText = "You sit, but the desert is just a desert. Sand in your boots, sun in your eyes.",
        Skill = SkillType.Sense, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 16, CreditReward = 0, UpgradePointReward = 3,
    };
    public static SkillCheckEvent JundlandWestTuskenWatcher => new()
    {
        Id = "jundland_west_watcher",
        Description = "A Tusken Elder watches you from a southern bluff. Showing respect and peaceful intent will help",
        SuccessText = "You leave a small offering on a flat stone. The Tusken inclines their head, just slightly.",
        FailText = "You misjudge the gesture; the Elder rises and a coordinated cry rises from the rocks.",
        Skill = SkillType.Xenology, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 0, UpgradePointReward = 2,
        CombatNpcOnFail = NPCData.TuskenRaider,
    };

    public static SkillCheckEvent JawaTerritoriesAuctionWin => new()
    {
        Id = "jawa_auction_win",
        Description = "A R5-series droid is on the auction block. With the right bid, it could be yours.",
        SuccessText = "You win the bid for a song. The droid trills a thankful tune as it powers up under your control.",
        FailText = "The bidding spirals; you walk away with empty pockets and no droid.",
        Skill = SkillType.Persuade, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 0, UpgradePointReward = 2, CreditPenalty = 60,
    };
    public static SkillCheckEvent JawaTerritoriesSalvageStream => new()
    {
        Id = "jawa_territories_salvage",
        Description = "A sandcrawler's belt spools out salvage in a slow conveyor stream. The Jawas are distracted.",
        SuccessText = "You pluck a working motivator off the belt without being seen. Resaleable, easily.",
        FailText = "A sharp Jawa elder catches your hand mid-grab and chitters furiously.",
        Skill = SkillType.Steal, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 70, UpgradePointReward = 1, Repeatable = true,
    };
    public static SkillCheckEvent JawaTerritoriesPartIdentify => new()
    {
        Id = "jawa_territories_part",
        Description = "An auctioneer tries to pass off a damaged hyperdrive coil as 'lightly used.' Can you tell the difference?",
        SuccessText = "You point out the welded crack. The crowd murmurs; the Jawa lowers the price drastically.",
        FailText = "You bid high on a coil that won't last a single jump.",
        Skill = SkillType.Vehicles, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 60, UpgradePointReward = 1, CreditPenalty = 80,
    };

    public static SkillCheckEvent OldBenJournalRead => new()
    {
        Id = "oldben_journal",
        Description = "A hand-stitched journal half-buried in sand. The script is archaic Aurebesh.",
        SuccessText = "You decipher entries about meditation, exile, and a child to be raised in secret. Profoundly important.",
        FailText = "The script defeats you; you copy a few characters and move on, sensing you missed something.",
        Skill = SkillType.Galaxy, Difficulty = CheckDifficulty.Challenging,
        TargetNumber = 20, CreditReward = 0, UpgradePointReward = 3,
    };
    public static SkillCheckEvent OldBenForcePresence => new()
    {
        Id = "oldben_presence",
        Description = "The Force here is patient and watchful. Reaching out to it, gently, might reveal something.",
        SuccessText = "A flicker — an old Jedi's voice, calm: 'You will learn what you must, when you must.' It fades.",
        FailText = "You reach out clumsily; the watcher withdraws, and the room feels colder.",
        Skill = SkillType.Sense, Difficulty = CheckDifficulty.Challenging,
        TargetNumber = 21, CreditReward = 0, UpgradePointReward = 3,
    };
    public static SkillCheckEvent OldBenSaberCircle => new()
    {
        Id = "oldben_saber",
        Description = "A scorched circle on the floor where a saber once cut stone. Examining it might reveal the cut's age.",
        SuccessText = "You date the cut to roughly two decades ago. Whoever made it has been gone — or hidden — that long.",
        FailText = "Heat patterns elude you. The cut is old; that's all you can say.",
        Skill = SkillType.Melee, Difficulty = CheckDifficulty.Difficult,
        TargetNumber = 17, CreditReward = 0, UpgradePointReward = 2,
    };

    // =========================================================
    // TALK-TRIGGERED CHECKS (quest pool)
    // =========================================================
    public static SkillCheckEvent TalkMedicineRequest => new()
    {
        Id = "talk_medicine",
        Description = "A wounded local leans against the wall, clutching a blaster burn. 'Please, can you patch me up?'",
        SuccessText = "You clean and dress the wound. The local thanks you profusely and insists on paying.",
        FailText = "You do your best, but the wound needs more than you can offer. 'Thanks for trying,' they wince.",
        Skill = SkillType.Medicine, Difficulty = CheckDifficulty.Easy,
        TargetNumber = 9, CreditReward = 25, UpgradePointReward = 1,
    };
    public static SkillCheckEvent TalkWillpowerInterrogation => new()
    {
        Id = "talk_willpower",
        Description = "An Imperial agent approaches. 'We're looking for a fugitive. You wouldn't happen to know anything, would you?'",
        SuccessText = "You meet their stare without flinching. 'Can't help you.' The agent moves on, satisfied you're not hiding anything.",
        FailText = "You stammer and avert your eyes. 'We'll be watching you, spacer.'",
        FailPenaltyText = "An arbitrary 'processing fee' is levied.",
        Skill = SkillType.Willpower, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 13, CreditReward = 0, UpgradePointReward = 1,
        CreditPenalty = 40,
    };
    public static SkillCheckEvent TalkXenologyIdentify => new()
    {
        Id = "talk_xenology",
        Description = "A collector shows you a strange artifact. 'I found this in the Outer Rim. Can you tell me what species made it?'",
        SuccessText = "You recognize the markings immediately — ancient Draelith ritual carvings. The collector is delighted and rewards your knowledge.",
        FailText = "The script is unfamiliar. You shake your head. 'Never seen anything like it.'",
        Skill = SkillType.Xenology, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 12, CreditReward = 45, UpgradePointReward = 1,
    };
    public static SkillCheckEvent TalkGalaxyIntel => new()
    {
        Id = "talk_galaxy_intel",
        Description = "A rebel sympathizer whispers: 'We've intercepted an Imperial transmission but can't decode the sector references. Know your way around the galaxy?'",
        SuccessText = "You cross-reference the coordinates from memory. 'That's the Kessel Run waypoint — they're moving prisoners.' The sympathizer nods gravely and pays you.",
        FailText = "The sector codes don't ring any bells. 'Sorry, can't help with this one.'",
        Skill = SkillType.Galaxy, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 15, CreditReward = 75, UpgradePointReward = 2,
    };
    public static SkillCheckEvent TalkSurvivalGuide => new()
    {
        Id = "talk_survival",
        Description = "A group of settlers is planning an expedition to a dangerous moon. 'We need someone who knows how to survive out there. Any advice?'",
        SuccessText = "You brief them on water sources, predator patterns, and shelter construction. They're impressed and pay for the consultation.",
        FailText = "You offer some general advice, but it's clear you're out of your element.",
        Skill = SkillType.Survival, Difficulty = CheckDifficulty.Easy,
        TargetNumber = 9, CreditReward = 30, UpgradePointReward = 1,
    };
    public static SkillCheckEvent TalkControlCalm => new()
    {
        Id = "talk_control",
        Description = "A panicked child has gotten separated from their family in the crowd. They're crying and won't let anyone near. Perhaps the Force can help calm them.",
        SuccessText = "You reach out with the Force, projecting warmth and safety. The child calms and lets you guide them to their grateful parent, who rewards you.",
        FailText = "You try to project calm, but your focus wavers.",
        Skill = SkillType.Control, Difficulty = CheckDifficulty.Moderate,
        TargetNumber = 14, CreditReward = 20, UpgradePointReward = 2,
    };

    // =========================================================
    // LOCATION CHECKS MAPPING
    // =========================================================
    public static Dictionary<string, List<SkillCheckEvent>> LocationChecks
    {
        get
        {
            var map = new Dictionary<string, List<SkillCheckEvent>>
            {
        ["tatooine_espa_cantina"] = new() { CantinaLockbox, CantinaSabaccGame, CantinaDrunkDiplomat, CantinaPickpocketMark, CantinaRumorMill, CantinaBackroomBrawl },
        ["tatooine_espa_market"] = new() { MarketHaggle, MarketForgedCoinScam, MarketCrowdSlip, MarketWarehouseSlice, MarketExoticAnimal, MarketDroidAuction },
        ["tatooine_espa_docking_bay"] = new() { DockingBaySmuggle, DockingBayHotwireSpeeder, DockingBayRepairBid, DockingBayCustomsOfficer, DockingBaySalvageCart, DockingBaySensorGhost },
        ["tatooine_espa_alley"] = new() { AlleyBrokenDroid, AlleyMuggingAmbush, AlleyRebelCourier, AlleyGraffitiCipher, AlleyWoundedFugitive, AlleyStashRecovery },
        ["tatooine_espa_tunnels"] = new() { TunnelsPoisonGas, TunnelsLooseGrate, TunnelsRatKing, TunnelsSewageCurrent, TunnelsFeralBeast, TunnelsForgottenCache },
        ["tatooine_espa_reactor"] = new() { ReactorOverload, ReactorForceTurbulence, ReactorCoolantBreach, ReactorSecurityTerminal, ReactorSabotage, ReactorRadiationSurvey },
        ["tatooine_espa_command"] = new() { CommandTerminal, CommandOfficerInterrogation, CommandEavesdrop, CommandSignalIntercept, CommandForcedEntry, CommandTacticalMap },
        ["tatooine_espa_upper_district"] = new() { UpperDistrictForge, UpperDistrictArtForge, UpperDistrictSocialClimb, UpperDistrictPickpocketAide, UpperDistrictCorruptDroid, UpperDistrictDeceiveGuard },
        ["tatooine_espa_hangar"] = new() { HangarShipInspect, HangarBountyPoster, HangarGuardPayoff, HangarStowaway, HangarDroidRepair, HangarGravLiftMisaligned },
        ["derelict_interior"] = new() { DerelictForceEcho, DerelictDataCoreSalvage, DerelictSuitExposure, DerelictCreatureTrack, DerelictDarkArtifact, DerelictSurgicalBay },
        ["tatooine_orbit"] = new() { OrbitSensorSweep, OrbitFuelScavenge, OrbitImperialHailing, OrbitDebrisField, OrbitCommArray, OrbitSpaceWalker },
        ["deep_space"] = new() { DeepSpaceNavHazard, DeepSpacePirateSignal, DeepSpaceAncientWreck, DeepSpaceRogueJump, DeepSpaceCreatureEncounter, DeepSpaceSalvageBeacon },
        ["tatooine_mos_entha"] = new() { MosEnthaHuttTax, MosEnthaSpiceRunner, MosEnthaStreetFight, MosEnthaRepoJob, MosEnthaBrokenAC, MosEnthaInformantMeet },
        ["tatooine_entha_hutt_compound"] = new() { HuttCompoundAudience, HuttCompoundSliceVault, HuttCompoundRancorPen, HuttCompoundDebtSettle, HuttCompoundEntertainerEscape, HuttCompoundPoisonTaste },
        ["beggars_canyon"] = new() { BeggarsCanyonDeadManTurn, BeggarsCanyonDiabloCut, BeggarsCanyonTuskenSnipe, BeggarsCanyonWreckDive, BeggarsCanyonUpdraftRide, BeggarsCanyonHermitContract },
        ["tatooine_mospic_high_range"] = new() { TatooineTuskenchasePilot, TatooineSarlaccAthletics, TatooineSandstormSurvival, MospicTuskenBurialRespect, MospicAncientPictograph, MospicCliffClimb },
        ["rodia_orbit"] = new() { RodiaOrbitDockingQueue, RodiaOrbitBountyHandoff, RodiaOrbitCustomsSlip, RodiaOrbitSwampSensors, RodiaOrbitBountyHunterDuel, RodiaOrbitTraderTip },
        ["coruscant_docking_bay"] = new() { CoruscantDockingCustoms, CoruscantDockingSliceManifest, CoruscantDockingScroungeParts },
        ["coruscant_verity_courtyard"] = new() { CoruscantVerityEavesdrop, CoruscantVerityLoyaltyPledge, CoruscantVerityProbeDroid },
        ["coruscant_compnor_arcology"] = new() { CoruscantArcologyDoctrineExam, CoruscantArcologyCadetIntel, CoruscantArcologyForceWhispers },
        ["coruscant_compnor_imperial_register"] = new() { CoruscantRegisterRecordSlice, CoruscantRegisterClerkBribe, CoruscantRegisterAuditTrail },
        ["coruscant_federal_courtyard"] = new() { CoruscantFederalRoyalGuardStare, CoruscantFederalSenatorFavor, CoruscantFederalDroneIntercept },
        ["coruscant_imperial_palace"] = new() { CoruscantPalaceForceEcho, CoruscantPalaceDataCourier, CoruscantPalaceInquisitorAvoid },
        ["tatooine_pika_oasis"] = new() { PikaOasisFermentedFruit, PikaOasisTuskenTruce, PikaOasisHiddenCache },
        ["tatooine_bestine_outskirts"] = new() { BestineOutskirtsRecruiterDodge, BestineOutskirtsSpeederSalvage, BestineOutskirtsDroidHelp },
        ["tatooine_bestine_market"] = new() { BestineMarketBountyList, BestineMarketBothanDropoff, BestineMarketHaggle },
        ["tatooine_bestine_garrison"] = new() { GarrisonInfiltratePerimeter, GarrisonATSTSchematic, GarrisonForgedClearance },
        ["tatooine_bestine_spaceport"] = new() { SpaceportCustomsBypass, SpaceportRefuelScam, SpaceportPilotIntel },
        ["tatooine_judland_wasteland_east"] = new() { JundlandEastWarningBell, JundlandEastFootprints, JundlandEastBanditAmbush },
        ["tatooine_judland_wasteland_central"] = new() { JundlandCentralPictograph, JundlandCentralEchoNavigate, JundlandCentralJediScrap },
        ["tatooine_judland_wasteland_west"] = new() { JundlandWestSpireClimb, JundlandWestMeditationCairn, JundlandWestTuskenWatcher },
        ["tatooine_northern_jawa_territories"] = new() { JawaTerritoriesAuctionWin, JawaTerritoriesSalvageStream, JawaTerritoriesPartIdentify },
        ["tatooine_old_ben_residence"] = new() { OldBenJournalRead, OldBenForcePresence, OldBenSaberCircle },
            };
            RegisterImportedLocationChecks(map);
            return map;
        }
    }

    // Pool of talk-triggered checks (any location with friendly NPCs)
    public static List<SkillCheckEvent> TalkChecks
    {
        get
        {
            var list = new List<SkillCheckEvent>
            {
                TalkMedicineRequest, TalkWillpowerInterrogation, TalkXenologyIdentify,
                TalkGalaxyIntel, TalkSurvivalGuide, TalkControlCalm,
            };
            RegisterImportedTalkChecks(list);
            return list;
        }
    }

    public static string DifficultyLabel(CheckDifficulty d) => d switch
    {
        CheckDifficulty.Easy => "Easy (TN 4-10)",
        CheckDifficulty.Moderate => "Moderate (TN 11-15)",
        CheckDifficulty.Difficult => "Difficult (TN 16-20)",
        CheckDifficulty.Challenging => "Challenging (TN 21-30)",
        _ => "Unknown"
    };
}
