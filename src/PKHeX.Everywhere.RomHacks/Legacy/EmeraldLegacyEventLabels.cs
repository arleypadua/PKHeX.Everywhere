// Flag and work names from Emerald Legacy's own constants, so the editor labels what the hack means
// rather than what vanilla Emerald means. Generated from include/constants/flags.h and
// include/constants/vars.h at https://github.com/cRz-Shadows/Pokemon_Emerald_Legacy commit 6465639719.
// The indices are the hack's own: its larger trainer flag range moves everything from SYSTEM_FLAGS
// (0x860 in Emerald, 0x8C0 here) up by 0x60, the islands among them.

using PKHeX.Facade.Abstractions;

namespace PKHeX.Everywhere.RomHacks.Legacy;

internal static class EmeraldLegacyEventLabels
{
    internal static readonly EventLabel[] Flags =
    [
        new(32, "Hide Mewtwo", "Hide"), // FLAG_HIDE_MEWTWO
        new(33, "Caught Mewtwo", "Misc"), // FLAG_CAUGHT_MEWTWO
        new(34, "Defeated Mewtwo", "Defeated"), // FLAG_DEFEATED_MEWTWO
        new(35, "Hide Champions Room Steven", "Hide"), // FLAG_HIDE_CHAMPIONS_ROOM_STEVEN
        new(36, "Hide Champions Room Wallace", "Hide"), // FLAG_HIDE_CHAMPIONS_ROOM_WALLACE
        new(37, "Hide Hall Of Fame Steven", "Hide"), // FLAG_HIDE_HALL_OF_FAME_STEVEN
        new(38, "Hide Hall Of Fame Wallace", "Hide"), // FLAG_HIDE_HALL_OF_FAME_WALLACE
        new(39, "Hide Sootopolis Gym Wallace", "Hide"), // FLAG_HIDE_SOOTOPOLIS_GYM_WALLACE
        new(40, "Hide Sootopolis Gym Juan", "Hide"), // FLAG_HIDE_SOOTOPOLIS_GYM_JUAN
        new(41, "Show Mirage Island", "Misc"), // FLAG_SHOW_MIRAGE_ISLAND
        new(42, "Celebi Event", "Misc"), // FLAG_CELEBI_EVENT
        new(43, "Celebi Clock 1", "Misc"), // FLAG_CELEBI_CLOCK_1
        new(44, "Celebi Clock 2", "Misc"), // FLAG_CELEBI_CLOCK_2
        new(45, "Hide Celebi", "Hide"), // FLAG_HIDE_CELEBI
        new(46, "Battled Celebi", "Misc"), // FLAG_BATTLED_CELEBI
        new(47, "Cozmo Caught Deoxys", "Misc"), // FLAG_COZMO_CAUGHT_DEOXYS
        new(48, "Started First Purity Reading", "Misc"), // FLAG_STARTED_FIRST_PURITY_READING
        new(49, "Got First Purity Reading", "Misc"), // FLAG_GOT_FIRST_PURITY_READING
        new(50, "Started Second Purity Reading", "Misc"), // FLAG_STARTED_SECOND_PURITY_READING
        new(51, "Got Second Purity Reading", "Misc"), // FLAG_GOT_SECOND_PURITY_READING
        new(52, "Hide Suicune", "Hide"), // FLAG_HIDE_SUICUNE
        new(53, "Caught Suicune", "Misc"), // FLAG_CAUGHT_SUICUNE
        new(54, "Defeated Suicune", "Defeated"), // FLAG_DEFEATED_SUICUNE
        new(55, "Started Final Purity Reading", "Misc"), // FLAG_STARTED_FINAL_PURITY_READING
        new(56, "Hide Weather Institute 2F Post Game Workers", "Hide"), // FLAG_HIDE_WEATHER_INSTITUTE_2F_POST_GAME_WORKERS
        new(57, "Started First Lightning Search", "Misc"), // FLAG_STARTED_FIRST_LIGHTNING_SEARCH
        new(58, "Got First Lightning Search", "Misc"), // FLAG_GOT_FIRST_LIGHTNING_SEARCH
        new(59, "Started Second Lightning Search", "Misc"), // FLAG_STARTED_SECOND_LIGHTNING_SEARCH
        new(60, "Got Second Lightning Search", "Misc"), // FLAG_GOT_SECOND_LIGHTNING_SEARCH
        new(61, "Started Final Lightning Search", "Misc"), // FLAG_STARTED_FINAL_LIGHTNING_SEARCH
        new(62, "Hide Raikou1", "Hide"), // FLAG_HIDE_RAIKOU1
        new(63, "Caught Raikou", "Misc"), // FLAG_CAUGHT_RAIKOU
        new(64, "Defeated Raikou", "Defeated"), // FLAG_DEFEATED_RAIKOU
        new(65, "Started First Seismic Reading", "Misc"), // FLAG_STARTED_FIRST_SEISMIC_READING
        new(66, "Got First Seismic Reading", "Misc"), // FLAG_GOT_FIRST_SEISMIC_READING
        new(67, "Started Ash Sample", "Misc"), // FLAG_STARTED_ASH_SAMPLE
        new(68, "Got Ash Sample", "Misc"), // FLAG_GOT_ASH_SAMPLE
        new(69, "Started Final Seismic Reading", "Misc"), // FLAG_STARTED_FINAL_SEISMIC_READING
        new(70, "Hide Entei", "Hide"), // FLAG_HIDE_ENTEI
        new(71, "Caught Entei", "Misc"), // FLAG_CAUGHT_ENTEI
        new(72, "Defeated Entei", "Defeated"), // FLAG_DEFEATED_ENTEI
        new(73, "Hide Raikou2", "Hide"), // FLAG_HIDE_RAIKOU2
        new(74, "Hide Raikou3", "Hide"), // FLAG_HIDE_RAIKOU3
        new(80, "Hide Sky Pillar Top Rayquaza Still", "Hide"), // FLAG_HIDE_SKY_PILLAR_TOP_RAYQUAZA_STILL
        new(81, "Set Wall Clock", "Misc"), // FLAG_SET_WALL_CLOCK
        new(82, "Rescued Birch", "Misc"), // FLAG_RESCUED_BIRCH
        new(83, "Legendaries In Sootopolis", "Misc"), // FLAG_LEGENDARIES_IN_SOOTOPOLIS
        new(84, "Egg Moves Tutor", "Misc"), // FLAG_EGG_MOVES_TUTOR
        new(85, "Unlocked Bike Switching", "Misc"), // FLAG_UNLOCKED_BIKE_SWITCHING
        new(86, "Hide Contest Poke Ball", "Hide"), // FLAG_HIDE_CONTEST_POKE_BALL
        new(87, "Met Rival Mom", "Misc"), // FLAG_MET_RIVAL_MOM
        new(88, "Birch Aide Met", "Misc"), // FLAG_BIRCH_AIDE_MET
        new(89, "Declined Bike", "Misc"), // FLAG_DECLINED_BIKE
        new(90, "Received Bike", "Received"), // FLAG_RECEIVED_BIKE
        new(91, "Wattson Rematch Available", "Misc"), // FLAG_WATTSON_REMATCH_AVAILABLE
        new(92, "Collected All Silver Symbols", "Misc"), // FLAG_COLLECTED_ALL_SILVER_SYMBOLS
        new(93, "Good Luck Safari Zone", "Misc"), // FLAG_GOOD_LUCK_SAFARI_ZONE
        new(94, "Received Wailmer Pail", "Received"), // FLAG_RECEIVED_WAILMER_PAIL
        new(95, "Received Pokeblock Case", "Received"), // FLAG_RECEIVED_POKEBLOCK_CASE
        new(96, "Received Secret Power", "Received"), // FLAG_RECEIVED_SECRET_POWER
        new(97, "Met Team Aqua Harbor", "Misc"), // FLAG_MET_TEAM_AQUA_HARBOR
        new(98, "Tv Explained", "Misc"), // FLAG_TV_EXPLAINED
        new(99, "Mauville Gym Barriers State", "Misc"), // FLAG_MAUVILLE_GYM_BARRIERS_STATE
        new(100, "Mossdeep Gym Switch 1", "Misc"), // FLAG_MOSSDEEP_GYM_SWITCH_1
        new(101, "Mossdeep Gym Switch 2", "Misc"), // FLAG_MOSSDEEP_GYM_SWITCH_2
        new(102, "Mossdeep Gym Switch 3", "Misc"), // FLAG_MOSSDEEP_GYM_SWITCH_3
        new(103, "Mossdeep Gym Switch 4", "Misc"), // FLAG_MOSSDEEP_GYM_SWITCH_4
        new(104, "Mauville Npc Trade Completed", "Misc"), // FLAG_MAUVILLE_NPC_TRADE_COMPLETED
        new(105, "Oceanic Museum Met Reporter", "Misc"), // FLAG_OCEANIC_MUSEUM_MET_REPORTER
        new(106, "Received Hm Strength", "Received"), // FLAG_RECEIVED_HM_STRENGTH
        new(107, "Received Hm Rock Smash", "Received"), // FLAG_RECEIVED_HM_ROCK_SMASH
        new(108, "Whiteout To Lavaridge", "Misc"), // FLAG_WHITEOUT_TO_LAVARIDGE
        new(109, "Received Hm Flash", "Received"), // FLAG_RECEIVED_HM_FLASH
        new(110, "Received Hm Fly", "Received"), // FLAG_RECEIVED_HM_FLY
        new(111, "Groudon Awakened Magma Hideout", "Misc"), // FLAG_GROUDON_AWAKENED_MAGMA_HIDEOUT
        new(112, "Team Aqua Escaped In Submarine", "Misc"), // FLAG_TEAM_AQUA_ESCAPED_IN_SUBMARINE
        new(114, "Scott Call Battle Frontier", "Misc"), // FLAG_SCOTT_CALL_BATTLE_FRONTIER
        new(115, "Received Meteorite", "Received"), // FLAG_RECEIVED_METEORITE
        new(116, "Adventure Started", "Misc"), // FLAG_ADVENTURE_STARTED
        new(117, "Defeated Magma Space Center", "Defeated"), // FLAG_DEFEATED_MAGMA_SPACE_CENTER
        new(118, "Met Hidden Power Giver", "Misc"), // FLAG_MET_HIDDEN_POWER_GIVER
        new(119, "Cancel Battle Room Challenge", "Misc"), // FLAG_CANCEL_BATTLE_ROOM_CHALLENGE
        new(120, "Landmark Mirage Tower", "Landmark"), // FLAG_LANDMARK_MIRAGE_TOWER
        new(121, "Received Tm Brick Break", "Received"), // FLAG_RECEIVED_TM_BRICK_BREAK
        new(122, "Received Hm Surf", "Received"), // FLAG_RECEIVED_HM_SURF
        new(123, "Received Hm Dive", "Received"), // FLAG_RECEIVED_HM_DIVE
        new(124, "Register Rival Pokenav", "Misc"), // FLAG_REGISTER_RIVAL_POKENAV
        new(125, "Defeated Rival Route 104", "Defeated"), // FLAG_DEFEATED_RIVAL_ROUTE_104
        new(126, "Defeated Wally Victory Road", "Defeated"), // FLAG_DEFEATED_WALLY_VICTORY_ROAD
        new(127, "Met Pretty Petal Shop Owner", "Misc"), // FLAG_MET_PRETTY_PETAL_SHOP_OWNER
        new(128, "Enable Roxanne First Call", "Enable"), // FLAG_ENABLE_ROXANNE_FIRST_CALL
        new(129, "Kyogre Escaped Seafloor Cavern", "Misc"), // FLAG_KYOGRE_ESCAPED_SEAFLOOR_CAVERN
        new(130, "Defeated Rival Route103", "Defeated"), // FLAG_DEFEATED_RIVAL_ROUTE103
        new(131, "Received Doll Lanette", "Received"), // FLAG_RECEIVED_DOLL_LANETTE
        new(132, "Received Potion Oldale", "Received"), // FLAG_RECEIVED_POTION_OLDALE
        new(133, "Received Amulet Coin", "Received"), // FLAG_RECEIVED_AMULET_COIN
        new(134, "Pending Daycare Egg", "Misc"), // FLAG_PENDING_DAYCARE_EGG
        new(135, "Thanked For Playing With Wally", "Misc"), // FLAG_THANKED_FOR_PLAYING_WITH_WALLY
        new(136, "Enable First Wally Pokenav Call", "Enable"), // FLAG_ENABLE_FIRST_WALLY_POKENAV_CALL
        new(137, "Received Hm Cut", "Received"), // FLAG_RECEIVED_HM_CUT
        new(138, "Scott Call Fortree Gym", "Misc"), // FLAG_SCOTT_CALL_FORTREE_GYM
        new(139, "Defeated Evil Team Mt Chimney", "Defeated"), // FLAG_DEFEATED_EVIL_TEAM_MT_CHIMNEY
        new(140, "Received 6 Soda Pop", "Received"), // FLAG_RECEIVED_6_SODA_POP
        new(141, "Defeated Seashore House", "Defeated"), // FLAG_DEFEATED_SEASHORE_HOUSE
        new(142, "Devon Goods Stolen", "Misc"), // FLAG_DEVON_GOODS_STOLEN
        new(143, "Recovered Devon Goods", "Misc"), // FLAG_RECOVERED_DEVON_GOODS
        new(144, "Returned Devon Goods", "Misc"), // FLAG_RETURNED_DEVON_GOODS
        new(145, "Caught Lugia", "Misc"), // FLAG_CAUGHT_LUGIA
        new(146, "Caught Ho Oh", "Misc"), // FLAG_CAUGHT_HO_OH
        new(147, "Mr Briney Sailing Intro", "Misc"), // FLAG_MR_BRINEY_SAILING_INTRO
        new(148, "Dock Rejected Devon Goods", "Misc"), // FLAG_DOCK_REJECTED_DEVON_GOODS
        new(149, "Delivered Devon Goods", "Misc"), // FLAG_DELIVERED_DEVON_GOODS
        new(150, "Received Contest Pass", "Received"), // FLAG_RECEIVED_CONTEST_PASS
        new(151, "Received Castform", "Received"), // FLAG_RECEIVED_CASTFORM
        new(152, "Received Super Rod", "Received"), // FLAG_RECEIVED_SUPER_ROD
        new(153, "Rustboro Npc Trade Completed", "Misc"), // FLAG_RUSTBORO_NPC_TRADE_COMPLETED
        new(154, "Pacifidlog Npc Trade Completed", "Misc"), // FLAG_PACIFIDLOG_NPC_TRADE_COMPLETED
        new(155, "Fortree Npc Trade Completed", "Misc"), // FLAG_FORTREE_NPC_TRADE_COMPLETED
        new(156, "Battle Frontier Trade Done", "Misc"), // FLAG_BATTLE_FRONTIER_TRADE_DONE
        new(157, "Force Mirage Tower Visible", "Misc"), // FLAG_FORCE_MIRAGE_TOWER_VISIBLE
        new(158, "Sootopolis Archie Maxie Leave", "Misc"), // FLAG_SOOTOPOLIS_ARCHIE_MAXIE_LEAVE
        new(159, "Interacted With Devon Employee Goods Stolen", "Misc"), // FLAG_INTERACTED_WITH_DEVON_EMPLOYEE_GOODS_STOLEN
        new(160, "Cool Painting Made", "Misc"), // FLAG_COOL_PAINTING_MADE
        new(161, "Beauty Painting Made", "Misc"), // FLAG_BEAUTY_PAINTING_MADE
        new(162, "Cute Painting Made", "Misc"), // FLAG_CUTE_PAINTING_MADE
        new(163, "Smart Painting Made", "Misc"), // FLAG_SMART_PAINTING_MADE
        new(164, "Tough Painting Made", "Misc"), // FLAG_TOUGH_PAINTING_MADE
        new(165, "Received Tm Rock Tomb", "Received"), // FLAG_RECEIVED_TM_ROCK_TOMB
        new(166, "Received Tm Bulk Up", "Received"), // FLAG_RECEIVED_TM_BULK_UP
        new(167, "Received Tm Shock Wave", "Received"), // FLAG_RECEIVED_TM_SHOCK_WAVE
        new(168, "Received Tm Overheat", "Received"), // FLAG_RECEIVED_TM_OVERHEAT
        new(169, "Received Tm Facade", "Received"), // FLAG_RECEIVED_TM_FACADE
        new(170, "Received Tm Aerial Ace", "Received"), // FLAG_RECEIVED_TM_AERIAL_ACE
        new(171, "Received Tm Calm Mind", "Received"), // FLAG_RECEIVED_TM_CALM_MIND
        new(172, "Received Tm Water Pulse", "Received"), // FLAG_RECEIVED_TM_WATER_PULSE
        new(173, "Hide Secret Base Trainer", "Hide"), // FLAG_HIDE_SECRET_BASE_TRAINER
        new(174, "Decoration 1", "Misc"), // FLAG_DECORATION_1
        new(175, "Decoration 2", "Misc"), // FLAG_DECORATION_2
        new(176, "Decoration 3", "Misc"), // FLAG_DECORATION_3
        new(177, "Decoration 4", "Misc"), // FLAG_DECORATION_4
        new(178, "Decoration 5", "Misc"), // FLAG_DECORATION_5
        new(179, "Decoration 6", "Misc"), // FLAG_DECORATION_6
        new(180, "Decoration 7", "Misc"), // FLAG_DECORATION_7
        new(181, "Decoration 8", "Misc"), // FLAG_DECORATION_8
        new(182, "Decoration 9", "Misc"), // FLAG_DECORATION_9
        new(183, "Decoration 10", "Misc"), // FLAG_DECORATION_10
        new(184, "Decoration 11", "Misc"), // FLAG_DECORATION_11
        new(185, "Decoration 12", "Misc"), // FLAG_DECORATION_12
        new(186, "Decoration 13", "Misc"), // FLAG_DECORATION_13
        new(187, "Decoration 14", "Misc"), // FLAG_DECORATION_14
        new(188, "Received Pokenav", "Received"), // FLAG_RECEIVED_POKENAV
        new(189, "Delivered Steven Letter", "Misc"), // FLAG_DELIVERED_STEVEN_LETTER
        new(190, "Defeated Wally Mauville", "Defeated"), // FLAG_DEFEATED_WALLY_MAUVILLE
        new(191, "Defeated Grunt Space Center 1F", "Defeated"), // FLAG_DEFEATED_GRUNT_SPACE_CENTER_1F
        new(192, "Received Sun Stone Mossdeep", "Received"), // FLAG_RECEIVED_SUN_STONE_MOSSDEEP
        new(193, "Wally Speech", "Misc"), // FLAG_WALLY_SPEECH
        new(194, "Trick House Puzzle 7 Switch 1", "Misc"), // FLAG_TRICK_HOUSE_PUZZLE_7_SWITCH_1
        new(195, "Trick House Puzzle 7 Switch 2", "Misc"), // FLAG_TRICK_HOUSE_PUZZLE_7_SWITCH_2
        new(196, "Trick House Puzzle 7 Switch 3", "Misc"), // FLAG_TRICK_HOUSE_PUZZLE_7_SWITCH_3
        new(197, "Trick House Puzzle 7 Switch 4", "Misc"), // FLAG_TRICK_HOUSE_PUZZLE_7_SWITCH_4
        new(198, "Trick House Puzzle 7 Switch 5", "Misc"), // FLAG_TRICK_HOUSE_PUZZLE_7_SWITCH_5
        new(199, "Rusturf Tunnel Opened", "Misc"), // FLAG_RUSTURF_TUNNEL_OPENED
        new(200, "Received Red Scarf", "Received"), // FLAG_RECEIVED_RED_SCARF
        new(201, "Received Blue Scarf", "Received"), // FLAG_RECEIVED_BLUE_SCARF
        new(202, "Received Pink Scarf", "Received"), // FLAG_RECEIVED_PINK_SCARF
        new(203, "Received Green Scarf", "Received"), // FLAG_RECEIVED_GREEN_SCARF
        new(204, "Received Yellow Scarf", "Received"), // FLAG_RECEIVED_YELLOW_SCARF
        new(205, "Interacted With Steven Space Center", "Misc"), // FLAG_INTERACTED_WITH_STEVEN_SPACE_CENTER
        new(206, "Encountered Latias Or Latios", "Misc"), // FLAG_ENCOUNTERED_LATIAS_OR_LATIOS
        new(207, "Met Archie Meteor Falls", "Misc"), // FLAG_MET_ARCHIE_METEOR_FALLS
        new(208, "Got Basement Key From Wattson", "Misc"), // FLAG_GOT_BASEMENT_KEY_FROM_WATTSON
        new(209, "Got Tm Thunderbolt From Wattson", "Misc"), // FLAG_GOT_TM_THUNDERBOLT_FROM_WATTSON
        new(210, "Fan Club Strength Shared", "Misc"), // FLAG_FAN_CLUB_STRENGTH_SHARED
        new(211, "Defeated Rival Rustboro", "Defeated"), // FLAG_DEFEATED_RIVAL_RUSTBORO
        new(212, "Received Red Or Blue Orb", "Received"), // FLAG_RECEIVED_RED_OR_BLUE_ORB
        new(213, "Received Premier Ball Rustboro", "Received"), // FLAG_RECEIVED_PREMIER_BALL_RUSTBORO
        new(214, "Enable Wally Match Call", "Enable"), // FLAG_ENABLE_WALLY_MATCH_CALL
        new(215, "Enable Scott Match Call", "Enable"), // FLAG_ENABLE_SCOTT_MATCH_CALL
        new(216, "Enable Mom Match Call", "Enable"), // FLAG_ENABLE_MOM_MATCH_CALL
        new(217, "Met Diving Treasure Hunter", "Misc"), // FLAG_MET_DIVING_TREASURE_HUNTER
        new(218, "Met Wailmer Trainer", "Misc"), // FLAG_MET_WAILMER_TRAINER
        new(219, "Evil Leader Please Stop", "Misc"), // FLAG_EVIL_LEADER_PLEASE_STOP
        new(221, "Received Go Goggles", "Received"), // FLAG_RECEIVED_GO_GOGGLES
        new(222, "Wingull Sent On Errand", "Misc"), // FLAG_WINGULL_SENT_ON_ERRAND
        new(223, "Received Mental Herb", "Received"), // FLAG_RECEIVED_MENTAL_HERB
        new(224, "Wingull Delivered Mail", "Misc"), // FLAG_WINGULL_DELIVERED_MAIL
        new(225, "Received 20 Coins", "Received"), // FLAG_RECEIVED_20_COINS
        new(226, "Received Starter Doll", "Received"), // FLAG_RECEIVED_STARTER_DOLL
        new(227, "Received Good Rod", "Received"), // FLAG_RECEIVED_GOOD_ROD
        new(228, "Regi Doors Opened", "Misc"), // FLAG_REGI_DOORS_OPENED
        new(229, "Received Tm Return", "Received"), // FLAG_RECEIVED_TM_RETURN
        new(230, "Received Tm Sludge Bomb", "Received"), // FLAG_RECEIVED_TM_SLUDGE_BOMB
        new(231, "Received Tm Roar", "Received"), // FLAG_RECEIVED_TM_ROAR
        new(232, "Received Tm Giga Drain", "Received"), // FLAG_RECEIVED_TM_GIGA_DRAIN
        new(233, "Enable Norman Rematch Call", "Enable"), // FLAG_ENABLE_NORMAN_REMATCH_CALL
        new(234, "Received Tm Rest", "Received"), // FLAG_RECEIVED_TM_REST
        new(235, "Received Tm Attract", "Received"), // FLAG_RECEIVED_TM_ATTRACT
        new(236, "Received Glass Ornament", "Received"), // FLAG_RECEIVED_GLASS_ORNAMENT
        new(237, "Received Silver Shield", "Received"), // FLAG_RECEIVED_SILVER_SHIELD
        new(238, "Received Gold Shield", "Received"), // FLAG_RECEIVED_GOLD_SHIELD
        new(239, "Used Storage Key", "Misc"), // FLAG_USED_STORAGE_KEY
        new(240, "Used Room 1 Key", "Misc"), // FLAG_USED_ROOM_1_KEY
        new(241, "Used Room 2 Key", "Misc"), // FLAG_USED_ROOM_2_KEY
        new(242, "Used Room 4 Key", "Misc"), // FLAG_USED_ROOM_4_KEY
        new(243, "Used Room 6 Key", "Misc"), // FLAG_USED_ROOM_6_KEY
        new(244, "Met Prof Cozmo", "Misc"), // FLAG_MET_PROF_COZMO
        new(245, "Received Wailmer Doll", "Received"), // FLAG_RECEIVED_WAILMER_DOLL
        new(246, "Received Chesto Berry Route 104", "Received"), // FLAG_RECEIVED_CHESTO_BERRY_ROUTE_104
        new(247, "Defeated Ss Tidal Trainers", "Defeated"), // FLAG_DEFEATED_SS_TIDAL_TRAINERS
        new(248, "Received Spelon Berry", "Received"), // FLAG_RECEIVED_SPELON_BERRY
        new(249, "Received Pamtre Berry", "Received"), // FLAG_RECEIVED_PAMTRE_BERRY
        new(250, "Received Watmel Berry", "Received"), // FLAG_RECEIVED_WATMEL_BERRY
        new(251, "Received Durin Berry", "Received"), // FLAG_RECEIVED_DURIN_BERRY
        new(252, "Received Belue Berry", "Received"), // FLAG_RECEIVED_BELUE_BERRY
        new(253, "Enable Rival Match Call", "Enable"), // FLAG_ENABLE_RIVAL_MATCH_CALL
        new(254, "Received Charcoal", "Received"), // FLAG_RECEIVED_CHARCOAL
        new(255, "Latios Or Latias Roaming", "Misc"), // FLAG_LATIOS_OR_LATIAS_ROAMING
        new(256, "Received Repeat Ball", "Received"), // FLAG_RECEIVED_REPEAT_BALL
        new(257, "Received Old Rod", "Received"), // FLAG_RECEIVED_OLD_ROD
        new(258, "Received Coin Case", "Received"), // FLAG_RECEIVED_COIN_CASE
        new(259, "Returned Red Or Blue Orb", "Misc"), // FLAG_RETURNED_RED_OR_BLUE_ORB
        new(260, "Received Tm Snatch", "Received"), // FLAG_RECEIVED_TM_SNATCH
        new(261, "Received Tm Dig", "Received"), // FLAG_RECEIVED_TM_DIG
        new(262, "Received Tm Bullet Seed", "Received"), // FLAG_RECEIVED_TM_BULLET_SEED
        new(263, "Entered Elite Four", "Misc"), // FLAG_ENTERED_ELITE_FOUR
        new(264, "Received Tm Hidden Power", "Received"), // FLAG_RECEIVED_TM_HIDDEN_POWER
        new(265, "Received Tm Torment", "Received"), // FLAG_RECEIVED_TM_TORMENT
        new(266, "Received Lavaridge Egg", "Received"), // FLAG_RECEIVED_LAVARIDGE_EGG
        new(267, "Received Revived Fossil Mon", "Received"), // FLAG_RECEIVED_REVIVED_FOSSIL_MON
        new(268, "Secret Base Registry Enabled", "Misc"), // FLAG_SECRET_BASE_REGISTRY_ENABLED
        new(269, "Received Tm Thief", "Received"), // FLAG_RECEIVED_TM_THIEF
        new(270, "Contest Sketch Created", "Misc"), // FLAG_CONTEST_SKETCH_CREATED
        new(271, "Evil Team Escaped Stern Spoke", "Misc"), // FLAG_EVIL_TEAM_ESCAPED_STERN_SPOKE
        new(272, "Received Exp Share", "Received"), // FLAG_RECEIVED_EXP_SHARE
        new(273, "Pokerus Explained", "Misc"), // FLAG_POKERUS_EXPLAINED
        new(274, "Received Running Shoes", "Received"), // FLAG_RECEIVED_RUNNING_SHOES
        new(275, "Received Quick Claw", "Received"), // FLAG_RECEIVED_QUICK_CLAW
        new(276, "Received Kings Rock", "Received"), // FLAG_RECEIVED_KINGS_ROCK
        new(277, "Received Macho Brace", "Received"), // FLAG_RECEIVED_MACHO_BRACE
        new(278, "Received Soothe Bell", "Received"), // FLAG_RECEIVED_SOOTHE_BELL
        new(279, "Received White Herb", "Received"), // FLAG_RECEIVED_WHITE_HERB
        new(280, "Received Soft Sand", "Received"), // FLAG_RECEIVED_SOFT_SAND
        new(281, "Enable Prof Birch Match Call", "Enable"), // FLAG_ENABLE_PROF_BIRCH_MATCH_CALL
        new(282, "Received Cleanse Tag", "Received"), // FLAG_RECEIVED_CLEANSE_TAG
        new(283, "Received Focus Band", "Received"), // FLAG_RECEIVED_FOCUS_BAND
        new(284, "Declined Wally Battle Mauville", "Misc"), // FLAG_DECLINED_WALLY_BATTLE_MAUVILLE
        new(285, "Received Devon Scope", "Received"), // FLAG_RECEIVED_DEVON_SCOPE
        new(286, "Declined Rival Battle Lilycove", "Misc"), // FLAG_DECLINED_RIVAL_BATTLE_LILYCOVE
        new(287, "Met Devon Employee", "Misc"), // FLAG_MET_DEVON_EMPLOYEE
        new(288, "Met Rival Rustboro", "Misc"), // FLAG_MET_RIVAL_RUSTBORO
        new(289, "Received Silk Scarf", "Received"), // FLAG_RECEIVED_SILK_SCARF
        new(290, "Not Ready For Battle Route 120", "Misc"), // FLAG_NOT_READY_FOR_BATTLE_ROUTE_120
        new(291, "Received Ss Ticket", "Received"), // FLAG_RECEIVED_SS_TICKET
        new(292, "Met Rival Lilycove", "Misc"), // FLAG_MET_RIVAL_LILYCOVE
        new(293, "Met Rival In House After Lilycove", "Misc"), // FLAG_MET_RIVAL_IN_HOUSE_AFTER_LILYCOVE
        new(294, "Exchanged Scanner", "Misc"), // FLAG_EXCHANGED_SCANNER
        new(295, "Kecleon Fled Fortree", "Misc"), // FLAG_KECLEON_FLED_FORTREE
        new(296, "Petalburg Mart Expanded Items", "Misc"), // FLAG_PETALBURG_MART_EXPANDED_ITEMS
        new(297, "Received Miracle Seed", "Received"), // FLAG_RECEIVED_MIRACLE_SEED
        new(298, "Received Beldum", "Received"), // FLAG_RECEIVED_BELDUM
        new(299, "Received Fanclub Tm This Week", "Received"), // FLAG_RECEIVED_FANCLUB_TM_THIS_WEEK
        new(300, "Met Fanclub Younger Brother", "Misc"), // FLAG_MET_FANCLUB_YOUNGER_BROTHER
        new(301, "Rival Left For Route103", "Misc"), // FLAG_RIVAL_LEFT_FOR_ROUTE103
        new(302, "Omit Dive From Steven Letter", "Misc"), // FLAG_OMIT_DIVE_FROM_STEVEN_LETTER
        new(303, "Has Match Call", "Misc"), // FLAG_HAS_MATCH_CALL
        new(304, "Added Match Call To Pokenav", "Misc"), // FLAG_ADDED_MATCH_CALL_TO_POKENAV
        new(305, "Registered Steven Pokenav", "Misc"), // FLAG_REGISTERED_STEVEN_POKENAV
        new(306, "Enable Norman Match Call", "Enable"), // FLAG_ENABLE_NORMAN_MATCH_CALL
        new(307, "Steven Guides To Cave Of Origin", "Misc"), // FLAG_STEVEN_GUIDES_TO_CAVE_OF_ORIGIN
        new(308, "Met Archie Sootopolis", "Misc"), // FLAG_MET_ARCHIE_SOOTOPOLIS
        new(309, "Met Maxie Sootopolis", "Misc"), // FLAG_MET_MAXIE_SOOTOPOLIS
        new(310, "Met Scott Rustboro", "Misc"), // FLAG_MET_SCOTT_RUSTBORO
        new(311, "Wallace Goes To Sky Pillar", "Misc"), // FLAG_WALLACE_GOES_TO_SKY_PILLAR
        new(312, "Received Hm Waterfall", "Received"), // FLAG_RECEIVED_HM_WATERFALL
        new(313, "Beat Magma Grunt Jagged Pass", "Misc"), // FLAG_BEAT_MAGMA_GRUNT_JAGGED_PASS
        new(314, "Received Aurora Ticket", "Received"), // FLAG_RECEIVED_AURORA_TICKET
        new(315, "Received Mystic Ticket", "Received"), // FLAG_RECEIVED_MYSTIC_TICKET
        new(316, "Received Old Sea Map", "Received"), // FLAG_RECEIVED_OLD_SEA_MAP
        new(317, "Wonder Card Unused 1", "Misc"), // FLAG_WONDER_CARD_UNUSED_1
        new(318, "Wonder Card Unused 2", "Misc"), // FLAG_WONDER_CARD_UNUSED_2
        new(319, "Wonder Card Unused 3", "Misc"), // FLAG_WONDER_CARD_UNUSED_3
        new(320, "Wonder Card Unused 4", "Misc"), // FLAG_WONDER_CARD_UNUSED_4
        new(321, "Wonder Card Unused 5", "Misc"), // FLAG_WONDER_CARD_UNUSED_5
        new(322, "Wonder Card Unused 6", "Misc"), // FLAG_WONDER_CARD_UNUSED_6
        new(323, "Wonder Card Unused 7", "Misc"), // FLAG_WONDER_CARD_UNUSED_7
        new(324, "Wonder Card Unused 8", "Misc"), // FLAG_WONDER_CARD_UNUSED_8
        new(325, "Wonder Card Unused 9", "Misc"), // FLAG_WONDER_CARD_UNUSED_9
        new(326, "Wonder Card Unused 10", "Misc"), // FLAG_WONDER_CARD_UNUSED_10
        new(327, "Wonder Card Unused 11", "Misc"), // FLAG_WONDER_CARD_UNUSED_11
        new(328, "Wonder Card Unused 12", "Misc"), // FLAG_WONDER_CARD_UNUSED_12
        new(329, "Wonder Card Unused 13", "Misc"), // FLAG_WONDER_CARD_UNUSED_13
        new(330, "Wonder Card Unused 14", "Misc"), // FLAG_WONDER_CARD_UNUSED_14
        new(331, "Wonder Card Unused 15", "Misc"), // FLAG_WONDER_CARD_UNUSED_15
        new(332, "Wonder Card Unused 16", "Misc"), // FLAG_WONDER_CARD_UNUSED_16
        new(333, "Wonder Card Unused 17", "Misc"), // FLAG_WONDER_CARD_UNUSED_17
        new(334, "Mirage Tower Visible", "Misc"), // FLAG_MIRAGE_TOWER_VISIBLE
        new(335, "Chose Root Fossil", "Misc"), // FLAG_CHOSE_ROOT_FOSSIL
        new(336, "Chose Claw Fossil", "Misc"), // FLAG_CHOSE_CLAW_FOSSIL
        new(337, "Received Powder Jar", "Received"), // FLAG_RECEIVED_POWDER_JAR
        new(338, "Chosen Multi Battle Npc Partner", "Misc"), // FLAG_CHOSEN_MULTI_BATTLE_NPC_PARTNER
        new(339, "Met Battle Frontier Breeder", "Misc"), // FLAG_MET_BATTLE_FRONTIER_BREEDER
        new(340, "Met Battle Frontier Maniac", "Misc"), // FLAG_MET_BATTLE_FRONTIER_MANIAC
        new(341, "Entered Contest", "Misc"), // FLAG_ENTERED_CONTEST
        new(342, "Met Slateport Fanclub Chairman", "Misc"), // FLAG_MET_SLATEPORT_FANCLUB_CHAIRMAN
        new(343, "Met Battle Frontier Gambler", "Misc"), // FLAG_MET_BATTLE_FRONTIER_GAMBLER
        new(344, "Enable Mr Stone Pokenav", "Enable"), // FLAG_ENABLE_MR_STONE_POKENAV
        new(345, "Nurse Mentions Gold Card", "Misc"), // FLAG_NURSE_MENTIONS_GOLD_CARD
        new(346, "Met Frontier Beauty Move Tutor", "Misc"), // FLAG_MET_FRONTIER_BEAUTY_MOVE_TUTOR
        new(347, "Met Frontier Swimmer Move Tutor", "Misc"), // FLAG_MET_FRONTIER_SWIMMER_MOVE_TUTOR
        new(348, "Match Call Registered", "Misc"), // FLAG_MATCH_CALL_REGISTERED
        new(349, "Rematch Rose", "Misc"), // FLAG_REMATCH_ROSE
        new(350, "Rematch Andres", "Misc"), // FLAG_REMATCH_ANDRES
        new(351, "Rematch Dusty", "Misc"), // FLAG_REMATCH_DUSTY
        new(352, "Rematch Lola", "Misc"), // FLAG_REMATCH_LOLA
        new(353, "Rematch Ricky", "Misc"), // FLAG_REMATCH_RICKY
        new(354, "Rematch Lila And Roy", "Misc"), // FLAG_REMATCH_LILA_AND_ROY
        new(355, "Rematch Cristin", "Misc"), // FLAG_REMATCH_CRISTIN
        new(356, "Rematch Brooke", "Misc"), // FLAG_REMATCH_BROOKE
        new(357, "Rematch Wilton", "Misc"), // FLAG_REMATCH_WILTON
        new(358, "Rematch Valerie", "Misc"), // FLAG_REMATCH_VALERIE
        new(359, "Rematch Cindy", "Misc"), // FLAG_REMATCH_CINDY
        new(360, "Rematch Thalia", "Misc"), // FLAG_REMATCH_THALIA
        new(361, "Rematch Jessica", "Misc"), // FLAG_REMATCH_JESSICA
        new(362, "Rematch Winston", "Misc"), // FLAG_REMATCH_WINSTON
        new(363, "Rematch Steve", "Misc"), // FLAG_REMATCH_STEVE
        new(364, "Rematch Tony", "Misc"), // FLAG_REMATCH_TONY
        new(365, "Rematch Nob", "Misc"), // FLAG_REMATCH_NOB
        new(366, "Rematch Koji", "Misc"), // FLAG_REMATCH_KOJI
        new(367, "Rematch Fernando", "Misc"), // FLAG_REMATCH_FERNANDO
        new(368, "Rematch Dalton", "Misc"), // FLAG_REMATCH_DALTON
        new(369, "Rematch Bernie", "Misc"), // FLAG_REMATCH_BERNIE
        new(370, "Rematch Ethan", "Misc"), // FLAG_REMATCH_ETHAN
        new(371, "Rematch John And Jay", "Misc"), // FLAG_REMATCH_JOHN_AND_JAY
        new(372, "Rematch Jeffrey", "Misc"), // FLAG_REMATCH_JEFFREY
        new(373, "Rematch Cameron", "Misc"), // FLAG_REMATCH_CAMERON
        new(374, "Rematch Jacki", "Misc"), // FLAG_REMATCH_JACKI
        new(375, "Rematch Walter", "Misc"), // FLAG_REMATCH_WALTER
        new(376, "Rematch Karen", "Misc"), // FLAG_REMATCH_KAREN
        new(377, "Rematch Jerry", "Misc"), // FLAG_REMATCH_JERRY
        new(378, "Rematch Anna And Meg", "Misc"), // FLAG_REMATCH_ANNA_AND_MEG
        new(379, "Rematch Isabel", "Misc"), // FLAG_REMATCH_ISABEL
        new(380, "Rematch Miguel", "Misc"), // FLAG_REMATCH_MIGUEL
        new(381, "Rematch Timothy", "Misc"), // FLAG_REMATCH_TIMOTHY
        new(382, "Rematch Shelby", "Misc"), // FLAG_REMATCH_SHELBY
        new(383, "Rematch Calvin", "Misc"), // FLAG_REMATCH_CALVIN
        new(384, "Rematch Elliot", "Misc"), // FLAG_REMATCH_ELLIOT
        new(385, "Rematch Isaiah", "Misc"), // FLAG_REMATCH_ISAIAH
        new(386, "Rematch Maria", "Misc"), // FLAG_REMATCH_MARIA
        new(387, "Rematch Abigail", "Misc"), // FLAG_REMATCH_ABIGAIL
        new(388, "Rematch Dylan", "Misc"), // FLAG_REMATCH_DYLAN
        new(389, "Rematch Katelyn", "Misc"), // FLAG_REMATCH_KATELYN
        new(390, "Rematch Benjamin", "Misc"), // FLAG_REMATCH_BENJAMIN
        new(391, "Rematch Pablo", "Misc"), // FLAG_REMATCH_PABLO
        new(392, "Rematch Nicolas", "Misc"), // FLAG_REMATCH_NICOLAS
        new(393, "Rematch Robert", "Misc"), // FLAG_REMATCH_ROBERT
        new(394, "Rematch Lao", "Misc"), // FLAG_REMATCH_LAO
        new(395, "Rematch Cyndy", "Misc"), // FLAG_REMATCH_CYNDY
        new(396, "Rematch Madeline", "Misc"), // FLAG_REMATCH_MADELINE
        new(397, "Rematch Jenny", "Misc"), // FLAG_REMATCH_JENNY
        new(398, "Rematch Diana", "Misc"), // FLAG_REMATCH_DIANA
        new(399, "Rematch Amy And Liv", "Misc"), // FLAG_REMATCH_AMY_AND_LIV
        new(400, "Rematch Ernest", "Misc"), // FLAG_REMATCH_ERNEST
        new(401, "Rematch Cory", "Misc"), // FLAG_REMATCH_CORY
        new(402, "Rematch Edwin", "Misc"), // FLAG_REMATCH_EDWIN
        new(403, "Rematch Lydia", "Misc"), // FLAG_REMATCH_LYDIA
        new(404, "Rematch Isaac", "Misc"), // FLAG_REMATCH_ISAAC
        new(405, "Rematch Gabrielle", "Misc"), // FLAG_REMATCH_GABRIELLE
        new(406, "Rematch Catherine", "Misc"), // FLAG_REMATCH_CATHERINE
        new(407, "Rematch Jackson", "Misc"), // FLAG_REMATCH_JACKSON
        new(408, "Rematch Haley", "Misc"), // FLAG_REMATCH_HALEY
        new(409, "Rematch James", "Misc"), // FLAG_REMATCH_JAMES
        new(410, "Rematch Trent", "Misc"), // FLAG_REMATCH_TRENT
        new(411, "Rematch Sawyer", "Misc"), // FLAG_REMATCH_SAWYER
        new(412, "Rematch Kira And Dan", "Misc"), // FLAG_REMATCH_KIRA_AND_DAN
        new(413, "Rematch Wally", "Misc"), // FLAG_REMATCH_WALLY
        new(414, "Rematch Roxanne", "Misc"), // FLAG_REMATCH_ROXANNE
        new(415, "Rematch Brawly", "Misc"), // FLAG_REMATCH_BRAWLY
        new(416, "Rematch Wattson", "Misc"), // FLAG_REMATCH_WATTSON
        new(417, "Rematch Flannery", "Misc"), // FLAG_REMATCH_FLANNERY
        new(418, "Rematch Norman", "Misc"), // FLAG_REMATCH_NORMAN
        new(419, "Rematch Winona", "Misc"), // FLAG_REMATCH_WINONA
        new(420, "Rematch Tate And Liza", "Misc"), // FLAG_REMATCH_TATE_AND_LIZA
        new(421, "Rematch Sidney", "Misc"), // FLAG_REMATCH_SIDNEY
        new(422, "Rematch Phoebe", "Misc"), // FLAG_REMATCH_PHOEBE
        new(423, "Rematch Glacia", "Misc"), // FLAG_REMATCH_GLACIA
        new(424, "Rematch Drake", "Misc"), // FLAG_REMATCH_DRAKE
        new(425, "Rematch Wallace", "Misc"), // FLAG_REMATCH_WALLACE
        new(426, "Hide Lavaridge Pokemon Center Brendan", "Hide"), // FLAG_HIDE_LAVARIDGE_POKEMON_CENTER_BRENDAN
        new(427, "Hide Lavaridge Pokemon Center May", "Hide"), // FLAG_HIDE_LAVARIDGE_POKEMON_CENTER_MAY
        new(428, "Defeated Deoxys", "Defeated"), // FLAG_DEFEATED_DEOXYS
        new(429, "Battled Deoxys", "Misc"), // FLAG_BATTLED_DEOXYS
        new(430, "Shown Eon Ticket", "Misc"), // FLAG_SHOWN_EON_TICKET
        new(431, "Shown Aurora Ticket", "Misc"), // FLAG_SHOWN_AURORA_TICKET
        new(432, "Shown Old Sea Map", "Misc"), // FLAG_SHOWN_OLD_SEA_MAP
        new(433, "Move Tutor Taught Swagger", "Misc"), // FLAG_MOVE_TUTOR_TAUGHT_SWAGGER
        new(434, "Move Tutor Taught Rollout", "Misc"), // FLAG_MOVE_TUTOR_TAUGHT_ROLLOUT
        new(435, "Move Tutor Taught Fury Cutter", "Misc"), // FLAG_MOVE_TUTOR_TAUGHT_FURY_CUTTER
        new(436, "Move Tutor Taught Mimic", "Misc"), // FLAG_MOVE_TUTOR_TAUGHT_MIMIC
        new(437, "Move Tutor Taught Metronome", "Misc"), // FLAG_MOVE_TUTOR_TAUGHT_METRONOME
        new(438, "Move Tutor Taught Sleep Talk", "Misc"), // FLAG_MOVE_TUTOR_TAUGHT_SLEEP_TALK
        new(439, "Move Tutor Taught Substitute", "Misc"), // FLAG_MOVE_TUTOR_TAUGHT_SUBSTITUTE
        new(440, "Move Tutor Taught Dynamicpunch", "Misc"), // FLAG_MOVE_TUTOR_TAUGHT_DYNAMICPUNCH
        new(441, "Move Tutor Taught Double Edge", "Misc"), // FLAG_MOVE_TUTOR_TAUGHT_DOUBLE_EDGE
        new(442, "Move Tutor Taught Explosion", "Misc"), // FLAG_MOVE_TUTOR_TAUGHT_EXPLOSION
        new(443, "Defeated Regirock", "Defeated"), // FLAG_DEFEATED_REGIROCK
        new(444, "Defeated Regice", "Defeated"), // FLAG_DEFEATED_REGICE
        new(445, "Defeated Registeel", "Defeated"), // FLAG_DEFEATED_REGISTEEL
        new(446, "Defeated Kyogre", "Defeated"), // FLAG_DEFEATED_KYOGRE
        new(447, "Defeated Groudon", "Defeated"), // FLAG_DEFEATED_GROUDON
        new(448, "Defeated Rayquaza", "Defeated"), // FLAG_DEFEATED_RAYQUAZA
        new(449, "Defeated Voltorb 1 New Mauville", "Defeated"), // FLAG_DEFEATED_VOLTORB_1_NEW_MAUVILLE
        new(450, "Defeated Voltorb 2 New Mauville", "Defeated"), // FLAG_DEFEATED_VOLTORB_2_NEW_MAUVILLE
        new(451, "Defeated Voltorb 3 New Mauville", "Defeated"), // FLAG_DEFEATED_VOLTORB_3_NEW_MAUVILLE
        new(452, "Defeated Electrode 1 Aqua Hideout", "Defeated"), // FLAG_DEFEATED_ELECTRODE_1_AQUA_HIDEOUT
        new(453, "Defeated Electrode 2 Aqua Hideout", "Defeated"), // FLAG_DEFEATED_ELECTRODE_2_AQUA_HIDEOUT
        new(454, "Defeated Sudowoodo", "Defeated"), // FLAG_DEFEATED_SUDOWOODO
        new(455, "Defeated Mew", "Defeated"), // FLAG_DEFEATED_MEW
        new(456, "Defeated Latias Or Latios", "Defeated"), // FLAG_DEFEATED_LATIAS_OR_LATIOS
        new(457, "Caught Latias Or Latios", "Misc"), // FLAG_CAUGHT_LATIAS_OR_LATIOS
        new(458, "Caught Mew", "Misc"), // FLAG_CAUGHT_MEW
        new(459, "Met Scott After Obtaining Stone Badge", "Misc"), // FLAG_MET_SCOTT_AFTER_OBTAINING_STONE_BADGE
        new(460, "Met Scott In Verdanturf", "Misc"), // FLAG_MET_SCOTT_IN_VERDANTURF
        new(461, "Met Scott In Fallarbor", "Misc"), // FLAG_MET_SCOTT_IN_FALLARBOR
        new(462, "Met Scott In Lilycove", "Misc"), // FLAG_MET_SCOTT_IN_LILYCOVE
        new(463, "Met Scott In Evergrande", "Misc"), // FLAG_MET_SCOTT_IN_EVERGRANDE
        new(464, "Met Scott On Ss Tidal", "Misc"), // FLAG_MET_SCOTT_ON_SS_TIDAL
        new(465, "Scott Gives Battle Points", "Misc"), // FLAG_SCOTT_GIVES_BATTLE_POINTS
        new(466, "Collected All Gold Symbols", "Misc"), // FLAG_COLLECTED_ALL_GOLD_SYMBOLS
        new(467, "Enable Roxanne Match Call", "Enable"), // FLAG_ENABLE_ROXANNE_MATCH_CALL
        new(468, "Enable Brawly Match Call", "Enable"), // FLAG_ENABLE_BRAWLY_MATCH_CALL
        new(469, "Enable Wattson Match Call", "Enable"), // FLAG_ENABLE_WATTSON_MATCH_CALL
        new(470, "Enable Flannery Match Call", "Enable"), // FLAG_ENABLE_FLANNERY_MATCH_CALL
        new(471, "Enable Winona Match Call", "Enable"), // FLAG_ENABLE_WINONA_MATCH_CALL
        new(472, "Enable Tate And Liza Match Call", "Enable"), // FLAG_ENABLE_TATE_AND_LIZA_MATCH_CALL
        new(473, "Enable Juan Match Call", "Enable"), // FLAG_ENABLE_JUAN_MATCH_CALL
        new(474, "Received Eon Ticket", "Received"), // FLAG_RECEIVED_EON_TICKET
        new(475, "Shown Mystic Ticket", "Misc"), // FLAG_SHOWN_MYSTIC_TICKET
        new(476, "Defeated Ho Oh", "Defeated"), // FLAG_DEFEATED_HO_OH
        new(477, "Defeated Lugia", "Defeated"), // FLAG_DEFEATED_LUGIA
        new(478, "Hide Petalburg City May", "Hide"), // FLAG_HIDE_PETALBURG_CITY_MAY
        new(479, "Hide Petalburg City Brendan", "Hide"), // FLAG_HIDE_PETALBURG_CITY_BRENDAN
        new(480, "Hide Mauville City Wally2", "Hide"), // FLAG_HIDE_MAUVILLE_CITY_WALLY2
        new(481, "Hide Jirachi", "Hide"), // FLAG_HIDE_JIRACHI
        new(482, "Hide Mossdeep City Birch", "Hide"), // FLAG_HIDE_MOSSDEEP_CITY_BIRCH
        new(483, "Battled Jirachi", "Misc"), // FLAG_BATTLED_JIRACHI
        new(484, "Mystery Gift Done", "Misc"), // FLAG_MYSTERY_GIFT_DONE
        new(485, "Mystery Gift 1", "Misc"), // FLAG_MYSTERY_GIFT_1
        new(486, "Mystery Gift 2", "Misc"), // FLAG_MYSTERY_GIFT_2
        new(487, "Mystery Gift 3", "Misc"), // FLAG_MYSTERY_GIFT_3
        new(488, "Mystery Gift 4", "Misc"), // FLAG_MYSTERY_GIFT_4
        new(489, "Mystery Gift 5", "Misc"), // FLAG_MYSTERY_GIFT_5
        new(490, "Mystery Gift 6", "Misc"), // FLAG_MYSTERY_GIFT_6
        new(491, "Mystery Gift 7", "Misc"), // FLAG_MYSTERY_GIFT_7
        new(492, "Mystery Gift 8", "Misc"), // FLAG_MYSTERY_GIFT_8
        new(493, "Mystery Gift 9", "Misc"), // FLAG_MYSTERY_GIFT_9
        new(494, "Mystery Gift 10", "Misc"), // FLAG_MYSTERY_GIFT_10
        new(495, "Mystery Gift 11", "Misc"), // FLAG_MYSTERY_GIFT_11
        new(496, "Mystery Gift 12", "Misc"), // FLAG_MYSTERY_GIFT_12
        new(497, "Mystery Gift 13", "Misc"), // FLAG_MYSTERY_GIFT_13
        new(498, "Mystery Gift 14", "Misc"), // FLAG_MYSTERY_GIFT_14
        new(499, "Mystery Gift 15", "Misc"), // FLAG_MYSTERY_GIFT_15
        new(500, "Hidden Items Start", "HiddenItem"), // FLAG_HIDDEN_ITEMS_START
        new(501, "Hidden Item Trick House Nugget", "HiddenItem"), // FLAG_HIDDEN_ITEM_TRICK_HOUSE_NUGGET
        new(502, "Hidden Item Route 111 Stardust", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_111_STARDUST
        new(503, "Hidden Item Route 113 Ether", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_113_ETHER
        new(504, "Hidden Item Route 114 Carbos", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_114_CARBOS
        new(505, "Hidden Item Route 119 Calcium", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_119_CALCIUM
        new(506, "Hidden Item Route 119 Ultra Ball", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_119_ULTRA_BALL
        new(507, "Hidden Item Route 123 Super Repel", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_123_SUPER_REPEL
        new(508, "Hidden Item Underwater 124 Carbos", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_124_CARBOS
        new(509, "Hidden Item Underwater 124 Green Shard", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_124_GREEN_SHARD
        new(510, "Hidden Item Underwater 124 Pearl", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_124_PEARL
        new(511, "Hidden Item Underwater 124 Big Pearl", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_124_BIG_PEARL
        new(512, "Hidden Item Underwater 126 Blue Shard", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_126_BLUE_SHARD
        new(513, "Hidden Item Underwater 124 Heart Scale 1", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_124_HEART_SCALE_1
        new(514, "Hidden Item Underwater 126 Heart Scale", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_126_HEART_SCALE
        new(515, "Hidden Item Underwater 126 Ultra Ball", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_126_ULTRA_BALL
        new(516, "Hidden Item Underwater 126 Stardust", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_126_STARDUST
        new(517, "Hidden Item Underwater 126 Pearl", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_126_PEARL
        new(518, "Hidden Item Underwater 126 Yellow Shard", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_126_YELLOW_SHARD
        new(519, "Hidden Item Underwater 126 Iron", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_126_IRON
        new(520, "Hidden Item Underwater 126 Big Pearl", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_126_BIG_PEARL
        new(521, "Hidden Item Underwater 127 Star Piece", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_127_STAR_PIECE
        new(522, "Hidden Item Underwater 127 Hp Up", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_127_HP_UP
        new(523, "Hidden Item Underwater 127 Heart Scale", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_127_HEART_SCALE
        new(524, "Hidden Item Underwater 127 Red Shard", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_127_RED_SHARD
        new(525, "Hidden Item Underwater 128 Protein", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_128_PROTEIN
        new(526, "Hidden Item Underwater 128 Pearl", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_128_PEARL
        new(527, "Hidden Item Lilycove City Heart Scale", "HiddenItem"), // FLAG_HIDDEN_ITEM_LILYCOVE_CITY_HEART_SCALE
        new(528, "Hidden Item Fallarbor Town Nugget", "HiddenItem"), // FLAG_HIDDEN_ITEM_FALLARBOR_TOWN_NUGGET
        new(529, "Hidden Item Mt Pyre Exterior Ultra Ball", "HiddenItem"), // FLAG_HIDDEN_ITEM_MT_PYRE_EXTERIOR_ULTRA_BALL
        new(530, "Hidden Item Route 113 Tm Double Team", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_113_TM_DOUBLE_TEAM
        new(531, "Hidden Item Abandoned Ship Rm 1 Key", "HiddenItem"), // FLAG_HIDDEN_ITEM_ABANDONED_SHIP_RM_1_KEY
        new(532, "Hidden Item Abandoned Ship Rm 2 Key", "HiddenItem"), // FLAG_HIDDEN_ITEM_ABANDONED_SHIP_RM_2_KEY
        new(533, "Hidden Item Abandoned Ship Rm 4 Key", "HiddenItem"), // FLAG_HIDDEN_ITEM_ABANDONED_SHIP_RM_4_KEY
        new(534, "Hidden Item Abandoned Ship Rm 6 Key", "HiddenItem"), // FLAG_HIDDEN_ITEM_ABANDONED_SHIP_RM_6_KEY
        new(535, "Hidden Item Ss Tidal Lower Deck Leftovers", "HiddenItem"), // FLAG_HIDDEN_ITEM_SS_TIDAL_LOWER_DECK_LEFTOVERS
        new(536, "Hidden Item Underwater 124 Calcium", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_124_CALCIUM
        new(537, "Hidden Item Route 104 Potion", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_104_POTION
        new(538, "Hidden Item Underwater 124 Heart Scale 2", "HiddenItem"), // FLAG_HIDDEN_ITEM_UNDERWATER_124_HEART_SCALE_2
        new(539, "Hidden Item Route 121 Hp Up", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_121_HP_UP
        new(540, "Hidden Item Route 121 Nugget", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_121_NUGGET
        new(541, "Hidden Item Route 123 Revive", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_123_REVIVE
        new(542, "Hidden Item Route 114 Revive", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_114_REVIVE
        new(543, "Hidden Item Lilycove City Pp Up", "HiddenItem"), // FLAG_HIDDEN_ITEM_LILYCOVE_CITY_PP_UP
        new(544, "Hidden Item Route 104 Super Potion", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_104_SUPER_POTION
        new(545, "Hidden Item Route 116 Super Potion", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_116_SUPER_POTION
        new(546, "Hidden Item Route 106 Stardust", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_106_STARDUST
        new(547, "Hidden Item Route 106 Heart Scale", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_106_HEART_SCALE
        new(548, "Hidden Item Granite Cave B2F Everstone 1", "HiddenItem"), // FLAG_HIDDEN_ITEM_GRANITE_CAVE_B2F_EVERSTONE_1
        new(549, "Hidden Item Granite Cave B2F Everstone 2", "HiddenItem"), // FLAG_HIDDEN_ITEM_GRANITE_CAVE_B2F_EVERSTONE_2
        new(550, "Hidden Item Route 109 Revive", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_109_REVIVE
        new(551, "Hidden Item Route 109 Great Ball", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_109_GREAT_BALL
        new(552, "Hidden Item Route 109 Heart Scale 1", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_109_HEART_SCALE_1
        new(553, "Hidden Item Route 110 Great Ball", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_110_GREAT_BALL
        new(554, "Hidden Item Route 110 Revive", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_110_REVIVE
        new(555, "Hidden Item Route 110 Full Heal", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_110_FULL_HEAL
        new(556, "Hidden Item Route 111 Protein", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_111_PROTEIN
        new(557, "Hidden Item Route 111 Rare Candy", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_111_RARE_CANDY
        new(558, "Hidden Item Petalburg Woods Potion", "HiddenItem"), // FLAG_HIDDEN_ITEM_PETALBURG_WOODS_POTION
        new(559, "Hidden Item Petalburg Woods Tiny Mushroom 1", "HiddenItem"), // FLAG_HIDDEN_ITEM_PETALBURG_WOODS_TINY_MUSHROOM_1
        new(560, "Hidden Item Petalburg Woods Tiny Mushroom 2", "HiddenItem"), // FLAG_HIDDEN_ITEM_PETALBURG_WOODS_TINY_MUSHROOM_2
        new(561, "Hidden Item Petalburg Woods Poke Ball", "HiddenItem"), // FLAG_HIDDEN_ITEM_PETALBURG_WOODS_POKE_BALL
        new(562, "Hidden Item Route 104 Poke Ball", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_104_POKE_BALL
        new(563, "Hidden Item Route 106 Poke Ball", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_106_POKE_BALL
        new(564, "Hidden Item Route 109 Ether", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_109_ETHER
        new(565, "Hidden Item Route 110 Poke Ball", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_110_POKE_BALL
        new(566, "Hidden Item Route 118 Heart Scale", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_118_HEART_SCALE
        new(567, "Hidden Item Route 118 Iron", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_118_IRON
        new(568, "Hidden Item Route 119 Full Heal", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_119_FULL_HEAL
        new(569, "Hidden Item Route 120 Rare Candy 2", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_120_RARE_CANDY_2
        new(570, "Hidden Item Route 120 Zinc", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_120_ZINC
        new(571, "Hidden Item Route 120 Rare Candy 1", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_120_RARE_CANDY_1
        new(572, "Hidden Item Route 117 Repel", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_117_REPEL
        new(573, "Hidden Item Route 121 Full Heal", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_121_FULL_HEAL
        new(574, "Hidden Item Route 123 Hyper Potion", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_123_HYPER_POTION
        new(575, "Hidden Item Lilycove City Poke Ball", "HiddenItem"), // FLAG_HIDDEN_ITEM_LILYCOVE_CITY_POKE_BALL
        new(576, "Hidden Item Jagged Pass Great Ball", "HiddenItem"), // FLAG_HIDDEN_ITEM_JAGGED_PASS_GREAT_BALL
        new(577, "Hidden Item Jagged Pass Full Heal", "HiddenItem"), // FLAG_HIDDEN_ITEM_JAGGED_PASS_FULL_HEAL
        new(578, "Hidden Item Mt Pyre Exterior Max Ether", "HiddenItem"), // FLAG_HIDDEN_ITEM_MT_PYRE_EXTERIOR_MAX_ETHER
        new(579, "Hidden Item Mt Pyre Summit Zinc", "HiddenItem"), // FLAG_HIDDEN_ITEM_MT_PYRE_SUMMIT_ZINC
        new(580, "Hidden Item Mt Pyre Summit Rare Candy", "HiddenItem"), // FLAG_HIDDEN_ITEM_MT_PYRE_SUMMIT_RARE_CANDY
        new(581, "Hidden Item Victory Road 1F Ultra Ball", "HiddenItem"), // FLAG_HIDDEN_ITEM_VICTORY_ROAD_1F_ULTRA_BALL
        new(582, "Hidden Item Victory Road B2F Elixir", "HiddenItem"), // FLAG_HIDDEN_ITEM_VICTORY_ROAD_B2F_ELIXIR
        new(583, "Hidden Item Victory Road B2F Max Repel", "HiddenItem"), // FLAG_HIDDEN_ITEM_VICTORY_ROAD_B2F_MAX_REPEL
        new(584, "Hidden Item Route 120 Revive", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_120_REVIVE
        new(585, "Hidden Item Route 104 Antidote", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_104_ANTIDOTE
        new(586, "Hidden Item Route 108 Rare Candy", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_108_RARE_CANDY
        new(587, "Hidden Item Route 119 Max Ether", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_119_MAX_ETHER
        new(588, "Hidden Item Route 104 Heart Scale", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_104_HEART_SCALE
        new(589, "Hidden Item Route 105 Heart Scale", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_105_HEART_SCALE
        new(590, "Hidden Item Route 109 Heart Scale 2", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_109_HEART_SCALE_2
        new(591, "Hidden Item Route 109 Heart Scale 3", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_109_HEART_SCALE_3
        new(592, "Hidden Item Route 128 Heart Scale 1", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_128_HEART_SCALE_1
        new(593, "Hidden Item Route 128 Heart Scale 2", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_128_HEART_SCALE_2
        new(594, "Hidden Item Route 128 Heart Scale 3", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_128_HEART_SCALE_3
        new(595, "Hidden Item Petalburg City Rare Candy", "HiddenItem"), // FLAG_HIDDEN_ITEM_PETALBURG_CITY_RARE_CANDY
        new(596, "Hidden Item Route 116 Black Glasses", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_116_BLACK_GLASSES
        new(597, "Hidden Item Route 115 Heart Scale", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_115_HEART_SCALE
        new(598, "Hidden Item Route 113 Nugget", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_113_NUGGET
        new(599, "Hidden Item Route 123 Pp Up", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_123_PP_UP
        new(600, "Hidden Item Route 121 Max Revive", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_121_MAX_REVIVE
        new(601, "Hidden Item Artisan Cave B1F Calcium", "HiddenItem"), // FLAG_HIDDEN_ITEM_ARTISAN_CAVE_B1F_CALCIUM
        new(602, "Hidden Item Artisan Cave B1F Zinc", "HiddenItem"), // FLAG_HIDDEN_ITEM_ARTISAN_CAVE_B1F_ZINC
        new(603, "Hidden Item Artisan Cave B1F Protein", "HiddenItem"), // FLAG_HIDDEN_ITEM_ARTISAN_CAVE_B1F_PROTEIN
        new(604, "Hidden Item Artisan Cave B1F Iron", "HiddenItem"), // FLAG_HIDDEN_ITEM_ARTISAN_CAVE_B1F_IRON
        new(605, "Hidden Item Safari Zone South East Full Restore", "HiddenItem"), // FLAG_HIDDEN_ITEM_SAFARI_ZONE_SOUTH_EAST_FULL_RESTORE
        new(606, "Hidden Item Safari Zone North East Rare Candy", "HiddenItem"), // FLAG_HIDDEN_ITEM_SAFARI_ZONE_NORTH_EAST_RARE_CANDY
        new(607, "Hidden Item Safari Zone North East Zinc", "HiddenItem"), // FLAG_HIDDEN_ITEM_SAFARI_ZONE_NORTH_EAST_ZINC
        new(608, "Hidden Item Safari Zone South East Pp Up", "HiddenItem"), // FLAG_HIDDEN_ITEM_SAFARI_ZONE_SOUTH_EAST_PP_UP
        new(609, "Hidden Item Navel Rock Top Sacred Ash", "HiddenItem"), // FLAG_HIDDEN_ITEM_NAVEL_ROCK_TOP_SACRED_ASH
        new(610, "Hidden Item Route 123 Rare Candy", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_123_RARE_CANDY
        new(611, "Hidden Item Route 105 Big Pearl", "HiddenItem"), // FLAG_HIDDEN_ITEM_ROUTE_105_BIG_PEARL
        new(612, "Rematch Ready Roxanne", "Misc"), // FLAG_REMATCH_READY_ROXANNE
        new(613, "Rematch Ready Brawly", "Misc"), // FLAG_REMATCH_READY_BRAWLY
        new(614, "Rematch Ready Wattson", "Misc"), // FLAG_REMATCH_READY_WATTSON
        new(615, "Rematch Ready Flannery", "Misc"), // FLAG_REMATCH_READY_FLANNERY
        new(616, "Rematch Ready Norman", "Misc"), // FLAG_REMATCH_READY_NORMAN
        new(617, "Rematch Ready Winona", "Misc"), // FLAG_REMATCH_READY_WINONA
        new(618, "Rematch Ready Tateandliza", "Misc"), // FLAG_REMATCH_READY_TATEANDLIZA
        new(619, "Rematch Ready Juan", "Misc"), // FLAG_REMATCH_READY_JUAN
        new(620, "Hide Zapdos", "Hide"), // FLAG_HIDE_ZAPDOS
        new(621, "Caught Zapdos", "Misc"), // FLAG_CAUGHT_ZAPDOS
        new(622, "Defeated Zapdos", "Defeated"), // FLAG_DEFEATED_ZAPDOS
        new(623, "Hide Moltres", "Hide"), // FLAG_HIDE_MOLTRES
        new(624, "Caught Moltres", "Misc"), // FLAG_CAUGHT_MOLTRES
        new(625, "Defeated Moltres", "Defeated"), // FLAG_DEFEATED_MOLTRES
        new(626, "Hide Articuno", "Hide"), // FLAG_HIDE_ARTICUNO
        new(627, "Caught Articuno", "Misc"), // FLAG_CAUGHT_ARTICUNO
        new(628, "Defeated Articuno", "Defeated"), // FLAG_DEFEATED_ARTICUNO
        new(629, "Beat All 3Rd Rematches", "Misc"), // FLAG_BEAT_ALL_3RD_REMATCHES
        new(630, "Trick House Prize Eevee", "Misc"), // FLAG_TRICK_HOUSE_PRIZE_EEVEE
        new(631, "Trick House Prize Tent", "Misc"), // FLAG_TRICK_HOUSE_PRIZE_TENT
        new(632, "Hide Game Corner Porygon Prize", "Hide"), // FLAG_HIDE_GAME_CORNER_PORYGON_PRIZE
        new(633, "Game Corner Prize Porygon", "Misc"), // FLAG_GAME_CORNER_PRIZE_PORYGON
        new(634, "Got Deep Sea Tooth", "Misc"), // FLAG_GOT_DEEP_SEA_TOOTH
        new(635, "Got Deep Sea Scale", "Misc"), // FLAG_GOT_DEEP_SEA_SCALE
        new(636, "Got 2Nd Deep Sea Item", "Misc"), // FLAG_GOT_2ND_DEEP_SEA_ITEM
        new(637, "Hide Zinnia 1F", "Hide"), // FLAG_HIDE_ZINNIA_1F
        new(638, "Spoke To Zinnia 1F", "Misc"), // FLAG_SPOKE_TO_ZINNIA_1F
        new(639, "Hide Zinnia 2F", "Hide"), // FLAG_HIDE_ZINNIA_2F
        new(640, "Spoke To Zinnia 2F", "Misc"), // FLAG_SPOKE_TO_ZINNIA_2F
        new(641, "Hide Zinnia 3F", "Hide"), // FLAG_HIDE_ZINNIA_3F
        new(642, "Spoke To Zinnia 3F", "Misc"), // FLAG_SPOKE_TO_ZINNIA_3F
        new(643, "Hide Zinnia 4F", "Hide"), // FLAG_HIDE_ZINNIA_4F
        new(644, "Spoke To Zinnia 4F", "Misc"), // FLAG_SPOKE_TO_ZINNIA_4F
        new(645, "Hide Zinnia 5F", "Hide"), // FLAG_HIDE_ZINNIA_5F
        new(646, "Spoke To Zinnia 5F", "Misc"), // FLAG_SPOKE_TO_ZINNIA_5F
        new(647, "Hide Zinnia Top", "Hide"), // FLAG_HIDE_ZINNIA_TOP
        new(648, "Defeated Zinnia", "Defeated"), // FLAG_DEFEATED_ZINNIA
        new(649, "Encountered Roaming Lati", "Misc"), // FLAG_ENCOUNTERED_ROAMING_LATI
        new(650, "Beat Wally Petalburg", "Misc"), // FLAG_BEAT_WALLY_PETALBURG
        new(651, "Frontier Seedot Npc Trade Completed", "Misc"), // FLAG_FRONTIER_SEEDOT_NPC_TRADE_COMPLETED
        new(652, "Frontier Plusle Npc Trade Completed", "Misc"), // FLAG_FRONTIER_PLUSLE_NPC_TRADE_COMPLETED
        new(653, "Received Shiny Beldum", "Received"), // FLAG_RECEIVED_SHINY_BELDUM
        new(700, "Hide Route 101 Birch Starters Bag", "Hide"), // FLAG_HIDE_ROUTE_101_BIRCH_STARTERS_BAG
        new(701, "Hide Apprentice", "Hide"), // FLAG_HIDE_APPRENTICE
        new(702, "Hide Pokemon Center 2F Mystery Gift Man", "Hide"), // FLAG_HIDE_POKEMON_CENTER_2F_MYSTERY_GIFT_MAN
        new(703, "Hide Union Room Player 1", "Hide"), // FLAG_HIDE_UNION_ROOM_PLAYER_1
        new(704, "Hide Union Room Player 2", "Hide"), // FLAG_HIDE_UNION_ROOM_PLAYER_2
        new(705, "Hide Union Room Player 3", "Hide"), // FLAG_HIDE_UNION_ROOM_PLAYER_3
        new(706, "Hide Union Room Player 4", "Hide"), // FLAG_HIDE_UNION_ROOM_PLAYER_4
        new(707, "Hide Union Room Player 5", "Hide"), // FLAG_HIDE_UNION_ROOM_PLAYER_5
        new(708, "Hide Union Room Player 6", "Hide"), // FLAG_HIDE_UNION_ROOM_PLAYER_6
        new(709, "Hide Union Room Player 7", "Hide"), // FLAG_HIDE_UNION_ROOM_PLAYER_7
        new(710, "Hide Union Room Player 8", "Hide"), // FLAG_HIDE_UNION_ROOM_PLAYER_8
        new(711, "Hide Battle Tower Multi Battle Partner 1", "Hide"), // FLAG_HIDE_BATTLE_TOWER_MULTI_BATTLE_PARTNER_1
        new(712, "Hide Battle Tower Multi Battle Partner 2", "Hide"), // FLAG_HIDE_BATTLE_TOWER_MULTI_BATTLE_PARTNER_2
        new(713, "Hide Battle Tower Multi Battle Partner 3", "Hide"), // FLAG_HIDE_BATTLE_TOWER_MULTI_BATTLE_PARTNER_3
        new(714, "Hide Battle Tower Multi Battle Partner 4", "Hide"), // FLAG_HIDE_BATTLE_TOWER_MULTI_BATTLE_PARTNER_4
        new(715, "Hide Battle Tower Multi Battle Partner 5", "Hide"), // FLAG_HIDE_BATTLE_TOWER_MULTI_BATTLE_PARTNER_5
        new(716, "Hide Battle Tower Multi Battle Partner 6", "Hide"), // FLAG_HIDE_BATTLE_TOWER_MULTI_BATTLE_PARTNER_6
        new(717, "Hide Safari Zone South Construction Workers", "Hide"), // FLAG_HIDE_SAFARI_ZONE_SOUTH_CONSTRUCTION_WORKERS
        new(718, "Hide Mew", "Hide"), // FLAG_HIDE_MEW
        new(719, "Hide Route 104 Rival", "Hide"), // FLAG_HIDE_ROUTE_104_RIVAL
        new(720, "Hide Route 101 Birch Zigzagoon Battle", "Hide"), // FLAG_HIDE_ROUTE_101_BIRCH_ZIGZAGOON_BATTLE
        new(721, "Hide Littleroot Town Birchs Lab Birch", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_BIRCHS_LAB_BIRCH
        new(722, "Hide Littleroot Town Mays House Rival Bedroom", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_MAYS_HOUSE_RIVAL_BEDROOM
        new(723, "Hide Route 103 Rival", "Hide"), // FLAG_HIDE_ROUTE_103_RIVAL
        new(724, "Hide Petalburg Woods Devon Employee", "Hide"), // FLAG_HIDE_PETALBURG_WOODS_DEVON_EMPLOYEE
        new(725, "Hide Petalburg Woods Aqua Grunt", "Hide"), // FLAG_HIDE_PETALBURG_WOODS_AQUA_GRUNT
        new(726, "Hide Petalburg City Wally", "Hide"), // FLAG_HIDE_PETALBURG_CITY_WALLY
        new(727, "Hide Mossdeep City Stevens House Invisible Ninja Boy", "Hide"), // FLAG_HIDE_MOSSDEEP_CITY_STEVENS_HOUSE_INVISIBLE_NINJA_BOY
        new(728, "Hide Petalburg City Wallys Mom", "Hide"), // FLAG_HIDE_PETALBURG_CITY_WALLYS_MOM
        new(729, "Done Pokecenter First Use", "Misc"), // FLAG_DONE_POKECENTER_FIRST_USE
        new(730, "Hide Lilycove Fan Club Interviewer", "Hide"), // FLAG_HIDE_LILYCOVE_FAN_CLUB_INTERVIEWER
        new(731, "Hide Rustboro City Aqua Grunt", "Hide"), // FLAG_HIDE_RUSTBORO_CITY_AQUA_GRUNT
        new(732, "Hide Rustboro City Devon Employee 1", "Hide"), // FLAG_HIDE_RUSTBORO_CITY_DEVON_EMPLOYEE_1
        new(733, "Hide Seafloor Cavern Room 9 Kyogre Asleep", "Hide"), // FLAG_HIDE_SEAFLOOR_CAVERN_ROOM_9_KYOGRE_ASLEEP
        new(734, "Hide Players House Dad", "Hide"), // FLAG_HIDE_PLAYERS_HOUSE_DAD
        new(735, "Hide Littleroot Town Brendans House Rival Sibling", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_BRENDANS_HOUSE_RIVAL_SIBLING
        new(736, "Hide Littleroot Town Mays House Rival Sibling", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_MAYS_HOUSE_RIVAL_SIBLING
        new(737, "Hide Mossdeep City Space Center Magma Note", "Hide"), // FLAG_HIDE_MOSSDEEP_CITY_SPACE_CENTER_MAGMA_NOTE
        new(738, "Hide Route 104 Mr Briney", "Hide"), // FLAG_HIDE_ROUTE_104_MR_BRINEY
        new(739, "Hide Brineys House Mr Briney", "Hide"), // FLAG_HIDE_BRINEYS_HOUSE_MR_BRINEY
        new(740, "Hide Mr Briney Dewford Town", "Hide"), // FLAG_HIDE_MR_BRINEY_DEWFORD_TOWN
        new(741, "Hide Route 109 Mr Briney", "Hide"), // FLAG_HIDE_ROUTE_109_MR_BRINEY
        new(742, "Hide Route 104 Mr Briney Boat", "Hide"), // FLAG_HIDE_ROUTE_104_MR_BRINEY_BOAT
        new(743, "Hide Mr Briney Boat Dewford Town", "Hide"), // FLAG_HIDE_MR_BRINEY_BOAT_DEWFORD_TOWN
        new(744, "Hide Route 109 Mr Briney Boat", "Hide"), // FLAG_HIDE_ROUTE_109_MR_BRINEY_BOAT
        new(745, "Hide Littleroot Town Brendans House Brendan", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_BRENDANS_HOUSE_BRENDAN
        new(746, "Hide Littleroot Town Mays House May", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_MAYS_HOUSE_MAY
        new(747, "Hide Safari Zone South East Expansion", "Hide"), // FLAG_HIDE_SAFARI_ZONE_SOUTH_EAST_EXPANSION
        new(748, "Hide Lilycove Harbor Event Ticket Taker", "Hide"), // FLAG_HIDE_LILYCOVE_HARBOR_EVENT_TICKET_TAKER
        new(749, "Hide Slateport City Scott", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_SCOTT
        new(750, "Hide Route 101 Zigzagoon", "Hide"), // FLAG_HIDE_ROUTE_101_ZIGZAGOON
        new(751, "Hide Victory Road Exit Wally", "Hide"), // FLAG_HIDE_VICTORY_ROAD_EXIT_WALLY
        new(752, "Hide Littleroot Town Mom Outside", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_MOM_OUTSIDE
        new(753, "Hide Mossdeep City Space Center 1F Steven", "Hide"), // FLAG_HIDE_MOSSDEEP_CITY_SPACE_CENTER_1F_STEVEN
        new(754, "Hide Littleroot Town Players House Vigoroth 1", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_PLAYERS_HOUSE_VIGOROTH_1
        new(755, "Hide Littleroot Town Players House Vigoroth 2", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_PLAYERS_HOUSE_VIGOROTH_2
        new(756, "Hide Mossdeep City Space Center 1F Team Magma", "Hide"), // FLAG_HIDE_MOSSDEEP_CITY_SPACE_CENTER_1F_TEAM_MAGMA
        new(757, "Hide Littleroot Town Players Bedroom Mom", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_PLAYERS_BEDROOM_MOM
        new(758, "Hide Littleroot Town Brendans House Mom", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_BRENDANS_HOUSE_MOM
        new(759, "Hide Littleroot Town Mays House Mom", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_MAYS_HOUSE_MOM
        new(760, "Hide Littleroot Town Brendans House Rival Bedroom", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_BRENDANS_HOUSE_RIVAL_BEDROOM
        new(761, "Hide Littleroot Town Brendans House Truck", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_BRENDANS_HOUSE_TRUCK
        new(762, "Hide Littleroot Town Mays House Truck", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_MAYS_HOUSE_TRUCK
        new(763, "Hide Deoxys", "Hide"), // FLAG_HIDE_DEOXYS
        new(764, "Hide Birth Island Deoxys Triangle", "Hide"), // FLAG_HIDE_BIRTH_ISLAND_DEOXYS_TRIANGLE
        new(765, "Hide Mauville City Scott", "Hide"), // FLAG_HIDE_MAUVILLE_CITY_SCOTT
        new(766, "Hide Verdanturf Town Scott", "Hide"), // FLAG_HIDE_VERDANTURF_TOWN_SCOTT
        new(767, "Hide Fallarbor Town Battle Tent Scott", "Hide"), // FLAG_HIDE_FALLARBOR_TOWN_BATTLE_TENT_SCOTT
        new(768, "Hide Route 111 Victor Winstrate", "Hide"), // FLAG_HIDE_ROUTE_111_VICTOR_WINSTRATE
        new(769, "Hide Route 111 Victoria Winstrate", "Hide"), // FLAG_HIDE_ROUTE_111_VICTORIA_WINSTRATE
        new(770, "Hide Route 111 Vivi Winstrate", "Hide"), // FLAG_HIDE_ROUTE_111_VIVI_WINSTRATE
        new(771, "Hide Route 111 Vicky Winstrate", "Hide"), // FLAG_HIDE_ROUTE_111_VICKY_WINSTRATE
        new(772, "Hide Petalburg Gym Norman", "Hide"), // FLAG_HIDE_PETALBURG_GYM_NORMAN
        new(773, "Hide Sky Pillar Top Rayquaza", "Hide"), // FLAG_HIDE_SKY_PILLAR_TOP_RAYQUAZA
        new(774, "Hide Lilycove Contest Hall Contest Attendant 1", "Hide"), // FLAG_HIDE_LILYCOVE_CONTEST_HALL_CONTEST_ATTENDANT_1
        new(775, "Hide Lilycove Museum Curator", "Hide"), // FLAG_HIDE_LILYCOVE_MUSEUM_CURATOR
        new(776, "Hide Lilycove Museum Patron 1", "Hide"), // FLAG_HIDE_LILYCOVE_MUSEUM_PATRON_1
        new(777, "Hide Lilycove Museum Patron 2", "Hide"), // FLAG_HIDE_LILYCOVE_MUSEUM_PATRON_2
        new(778, "Hide Lilycove Museum Patron 3", "Hide"), // FLAG_HIDE_LILYCOVE_MUSEUM_PATRON_3
        new(779, "Hide Lilycove Museum Patron 4", "Hide"), // FLAG_HIDE_LILYCOVE_MUSEUM_PATRON_4
        new(780, "Hide Lilycove Museum Tourists", "Hide"), // FLAG_HIDE_LILYCOVE_MUSEUM_TOURISTS
        new(781, "Hide Petalburg Gym Greeter", "Hide"), // FLAG_HIDE_PETALBURG_GYM_GREETER
        new(782, "Hide Marine Cave Kyogre", "Hide"), // FLAG_HIDE_MARINE_CAVE_KYOGRE
        new(783, "Hide Terra Cave Groudon", "Hide"), // FLAG_HIDE_TERRA_CAVE_GROUDON
        new(784, "Hide Littleroot Town Brendans House Rival Mom", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_BRENDANS_HOUSE_RIVAL_MOM
        new(785, "Hide Littleroot Town Mays House Rival Mom", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_MAYS_HOUSE_RIVAL_MOM
        new(786, "Hide Route 119 Scott", "Hide"), // FLAG_HIDE_ROUTE_119_SCOTT
        new(787, "Hide Lilycove Motel Scott", "Hide"), // FLAG_HIDE_LILYCOVE_MOTEL_SCOTT
        new(788, "Hide Mossdeep City Scott", "Hide"), // FLAG_HIDE_MOSSDEEP_CITY_SCOTT
        new(789, "Hide Fanclub Old Lady", "Hide"), // FLAG_HIDE_FANCLUB_OLD_LADY
        new(790, "Hide Fanclub Boy", "Hide"), // FLAG_HIDE_FANCLUB_BOY
        new(791, "Hide Fanclub Little Boy", "Hide"), // FLAG_HIDE_FANCLUB_LITTLE_BOY
        new(792, "Hide Fanclub Lady", "Hide"), // FLAG_HIDE_FANCLUB_LADY
        new(793, "Hide Ever Grande Pokemon Center 1F Scott", "Hide"), // FLAG_HIDE_EVER_GRANDE_POKEMON_CENTER_1F_SCOTT
        new(794, "Hide Littleroot Town Rival", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_RIVAL
        new(795, "Hide Littleroot Town Birch", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_BIRCH
        new(796, "Hide Route 111 Gabby And Ty 1", "Hide"), // FLAG_HIDE_ROUTE_111_GABBY_AND_TY_1
        new(797, "Hide Route 118 Gabby And Ty 1", "Hide"), // FLAG_HIDE_ROUTE_118_GABBY_AND_TY_1
        new(798, "Hide Route 120 Gabby And Ty 1", "Hide"), // FLAG_HIDE_ROUTE_120_GABBY_AND_TY_1
        new(799, "Hide Route 111 Gabby And Ty 3", "Hide"), // FLAG_HIDE_ROUTE_111_GABBY_AND_TY_3
        new(800, "Hide Lugia", "Hide"), // FLAG_HIDE_LUGIA
        new(801, "Hide Ho Oh", "Hide"), // FLAG_HIDE_HO_OH
        new(802, "Hide Lilycove Contest Hall Reporter", "Hide"), // FLAG_HIDE_LILYCOVE_CONTEST_HALL_REPORTER
        new(803, "Hide Slateport City Contest Reporter", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_CONTEST_REPORTER
        new(804, "Hide Mauville City Wally", "Hide"), // FLAG_HIDE_MAUVILLE_CITY_WALLY
        new(805, "Hide Mauville City Wallys Uncle", "Hide"), // FLAG_HIDE_MAUVILLE_CITY_WALLYS_UNCLE
        new(806, "Hide Verdanturf Town Wandas House Wally", "Hide"), // FLAG_HIDE_VERDANTURF_TOWN_WANDAS_HOUSE_WALLY
        new(807, "Hide Rusturf Tunnel Wandas Boyfriend", "Hide"), // FLAG_HIDE_RUSTURF_TUNNEL_WANDAS_BOYFRIEND
        new(808, "Hide Verdanturf Town Wandas House Wandas Boyfriend", "Hide"), // FLAG_HIDE_VERDANTURF_TOWN_WANDAS_HOUSE_WANDAS_BOYFRIEND
        new(809, "Hide Verdanturf Town Wandas House Wallys Uncle", "Hide"), // FLAG_HIDE_VERDANTURF_TOWN_WANDAS_HOUSE_WALLYS_UNCLE
        new(810, "Hide Ss Tidal Corridor Scott", "Hide"), // FLAG_HIDE_SS_TIDAL_CORRIDOR_SCOTT
        new(811, "Hide Littleroot Town Birchs Lab Pokeball Cyndaquil", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_BIRCHS_LAB_POKEBALL_CYNDAQUIL
        new(812, "Hide Littleroot Town Birchs Lab Pokeball Totodile", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_BIRCHS_LAB_POKEBALL_TOTODILE
        new(813, "Hide Route 116 Dropped Glasses Man", "Hide"), // FLAG_HIDE_ROUTE_116_DROPPED_GLASSES_MAN
        new(814, "Hide Rustboro City Rival", "Hide"), // FLAG_HIDE_RUSTBORO_CITY_RIVAL
        new(815, "Hide Littleroot Town Brendans House 2F Swablu Doll", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_BRENDANS_HOUSE_2F_SWABLU_DOLL
        new(816, "Hide Sootopolis City Wallace", "Hide"), // FLAG_HIDE_SOOTOPOLIS_CITY_WALLACE
        new(817, "Hide Littleroot Town Brendans House 2F Poke Ball", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_BRENDANS_HOUSE_2F_POKE_BALL
        new(818, "Hide Littleroot Town Mays House 2F Poke Ball", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_MAYS_HOUSE_2F_POKE_BALL
        new(819, "Hide Route 112 Team Magma", "Hide"), // FLAG_HIDE_ROUTE_112_TEAM_MAGMA
        new(820, "Hide Cave Of Origin B1F Wallace", "Hide"), // FLAG_HIDE_CAVE_OF_ORIGIN_B1F_WALLACE
        new(821, "Hide Aqua Hideout 1F Grunt 1 Blocking Entrance", "Hide"), // FLAG_HIDE_AQUA_HIDEOUT_1F_GRUNT_1_BLOCKING_ENTRANCE
        new(822, "Hide Aqua Hideout 1F Grunt 2 Blocking Entrance", "Hide"), // FLAG_HIDE_AQUA_HIDEOUT_1F_GRUNT_2_BLOCKING_ENTRANCE
        new(823, "Hide Mossdeep City Team Magma", "Hide"), // FLAG_HIDE_MOSSDEEP_CITY_TEAM_MAGMA
        new(824, "Hide Petalburg Gym Wallys Dad", "Hide"), // FLAG_HIDE_PETALBURG_GYM_WALLYS_DAD
        new(825, "Hide Legend Mon Cave Of Origin", "Hide"), // FLAG_HIDE_LEGEND_MON_CAVE_OF_ORIGIN
        new(826, "Hide Sootopolis City Archie", "Hide"), // FLAG_HIDE_SOOTOPOLIS_CITY_ARCHIE
        new(827, "Hide Sootopolis City Maxie", "Hide"), // FLAG_HIDE_SOOTOPOLIS_CITY_MAXIE
        new(828, "Hide Seafloor Cavern Room 9 Archie", "Hide"), // FLAG_HIDE_SEAFLOOR_CAVERN_ROOM_9_ARCHIE
        new(829, "Hide Seafloor Cavern Room 9 Maxie", "Hide"), // FLAG_HIDE_SEAFLOOR_CAVERN_ROOM_9_MAXIE
        new(830, "Hide Petalburg City Wallys Dad", "Hide"), // FLAG_HIDE_PETALBURG_CITY_WALLYS_DAD
        new(831, "Hide Seafloor Cavern Room 9 Magma Grunts", "Hide"), // FLAG_HIDE_SEAFLOOR_CAVERN_ROOM_9_MAGMA_GRUNTS
        new(832, "Hide Lilycove Contest Hall Blend Master", "Hide"), // FLAG_HIDE_LILYCOVE_CONTEST_HALL_BLEND_MASTER
        new(833, "Hide Granite Cave Steven", "Hide"), // FLAG_HIDE_GRANITE_CAVE_STEVEN
        new(834, "Hide Route 128 Steven", "Hide"), // FLAG_HIDE_ROUTE_128_STEVEN
        new(835, "Hide Slateport City Gabby And Ty", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_GABBY_AND_TY
        new(836, "Hide Battle Frontier Reception Gate Scott", "Hide"), // FLAG_HIDE_BATTLE_FRONTIER_RECEPTION_GATE_SCOTT
        new(837, "Hide Route 110 Birch", "Hide"), // FLAG_HIDE_ROUTE_110_BIRCH
        new(838, "Hide Littleroot Town Birchs Lab Pokeball Chikorita", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_BIRCHS_LAB_POKEBALL_CHIKORITA
        new(839, "Hide Sootopolis City Man 1", "Hide"), // FLAG_HIDE_SOOTOPOLIS_CITY_MAN_1
        new(840, "Hide Slateport City Captain Stern", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_CAPTAIN_STERN
        new(841, "Hide Slateport City Harbor Captain Stern", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_HARBOR_CAPTAIN_STERN
        new(842, "Hide Battle Frontier Sudowoodo", "Hide"), // FLAG_HIDE_BATTLE_FRONTIER_SUDOWOODO
        new(843, "Hide Route 111 Rock Smash Tip Guy", "Hide"), // FLAG_HIDE_ROUTE_111_ROCK_SMASH_TIP_GUY
        new(844, "Hide Rustboro City Scientist", "Hide"), // FLAG_HIDE_RUSTBORO_CITY_SCIENTIST
        new(845, "Hide Slateport City Harbor Aqua Grunt", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_HARBOR_AQUA_GRUNT
        new(846, "Hide Slateport City Harbor Archie", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_HARBOR_ARCHIE
        new(847, "Hide Jagged Pass Magma Guard", "Hide"), // FLAG_HIDE_JAGGED_PASS_MAGMA_GUARD
        new(848, "Hide Slateport City Harbor Submarine Shadow", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_HARBOR_SUBMARINE_SHADOW
        new(849, "Hide Littleroot Town Mays House 2F Pichu Doll", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_MAYS_HOUSE_2F_PICHU_DOLL
        new(850, "Hide Magma Hideout 4F Groudon Asleep", "Hide"), // FLAG_HIDE_MAGMA_HIDEOUT_4F_GROUDON_ASLEEP
        new(851, "Hide Route 119 Rival", "Hide"), // FLAG_HIDE_ROUTE_119_RIVAL
        new(852, "Hide Lilycove City Aqua Grunts", "Hide"), // FLAG_HIDE_LILYCOVE_CITY_AQUA_GRUNTS
        new(853, "Hide Magma Hideout 4F Groudon", "Hide"), // FLAG_HIDE_MAGMA_HIDEOUT_4F_GROUDON
        new(854, "Hide Sootopolis City Residents", "Hide"), // FLAG_HIDE_SOOTOPOLIS_CITY_RESIDENTS
        new(855, "Hide Sky Pillar Wallace", "Hide"), // FLAG_HIDE_SKY_PILLAR_WALLACE
        new(856, "Hide Mt Pyre Summit Maxie", "Hide"), // FLAG_HIDE_MT_PYRE_SUMMIT_MAXIE
        new(857, "Hide Magma Hideout Grunts", "Hide"), // FLAG_HIDE_MAGMA_HIDEOUT_GRUNTS
        new(858, "Hide Victory Road Entrance Wally", "Hide"), // FLAG_HIDE_VICTORY_ROAD_ENTRANCE_WALLY
        new(859, "Hide Seafloor Cavern Room 9 Kyogre", "Hide"), // FLAG_HIDE_SEAFLOOR_CAVERN_ROOM_9_KYOGRE
        new(860, "Hide Slateport City Harbor Ss Tidal", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_HARBOR_SS_TIDAL
        new(861, "Hide Lilycove Harbor Sstidal", "Hide"), // FLAG_HIDE_LILYCOVE_HARBOR_SSTIDAL
        new(862, "Hide Mossdeep City Space Center 2F Team Magma", "Hide"), // FLAG_HIDE_MOSSDEEP_CITY_SPACE_CENTER_2F_TEAM_MAGMA
        new(863, "Hide Mossdeep City Space Center 2F Steven", "Hide"), // FLAG_HIDE_MOSSDEEP_CITY_SPACE_CENTER_2F_STEVEN
        new(864, "Hide Battle Tower Multi Battle Partner Alt 1", "Hide"), // FLAG_HIDE_BATTLE_TOWER_MULTI_BATTLE_PARTNER_ALT_1
        new(865, "Hide Battle Tower Multi Battle Partner Alt 2", "Hide"), // FLAG_HIDE_BATTLE_TOWER_MULTI_BATTLE_PARTNER_ALT_2
        new(866, "Hide Petalburg Gym Wally", "Hide"), // FLAG_HIDE_PETALBURG_GYM_WALLY
        new(868, "Hide Littleroot Town Fat Man", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_FAT_MAN
        new(869, "Hide Slateport City Sterns Shipyard Mr Briney", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_STERNS_SHIPYARD_MR_BRINEY
        new(870, "Hide Lanettes House Lanette", "Hide"), // FLAG_HIDE_LANETTES_HOUSE_LANETTE
        new(871, "Hide Fallorbor Pokemon Center Lanette", "Hide"), // FLAG_HIDE_FALLORBOR_POKEMON_CENTER_LANETTE
        new(872, "Hide Trick House Entrance Man", "Hide"), // FLAG_HIDE_TRICK_HOUSE_ENTRANCE_MAN
        new(873, "Hide Lilycove Contest Hall Blend Master Replacement", "Hide"), // FLAG_HIDE_LILYCOVE_CONTEST_HALL_BLEND_MASTER_REPLACEMENT
        new(874, "Hide Desert Underpass Fossil", "Hide"), // FLAG_HIDE_DESERT_UNDERPASS_FOSSIL
        new(875, "Hide Route 111 Player Descent", "Hide"), // FLAG_HIDE_ROUTE_111_PLAYER_DESCENT
        new(876, "Hide Route 111 Desert Fossil", "Hide"), // FLAG_HIDE_ROUTE_111_DESERT_FOSSIL
        new(877, "Hide Mt Chimney Trainers", "Hide"), // FLAG_HIDE_MT_CHIMNEY_TRAINERS
        new(878, "Hide Rusturf Tunnel Aqua Grunt", "Hide"), // FLAG_HIDE_RUSTURF_TUNNEL_AQUA_GRUNT
        new(879, "Hide Rusturf Tunnel Briney", "Hide"), // FLAG_HIDE_RUSTURF_TUNNEL_BRINEY
        new(880, "Hide Rusturf Tunnel Peeko", "Hide"), // FLAG_HIDE_RUSTURF_TUNNEL_PEEKO
        new(881, "Hide Brineys House Peeko", "Hide"), // FLAG_HIDE_BRINEYS_HOUSE_PEEKO
        new(882, "Hide Slateport City Team Aqua", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_TEAM_AQUA
        new(883, "Hide Slateport City Oceanic Museum Aqua Grunts", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_OCEANIC_MUSEUM_AQUA_GRUNTS
        new(884, "Hide Slateport City Oceanic Museum 2F Aqua Grunt 1", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_OCEANIC_MUSEUM_2F_AQUA_GRUNT_1
        new(885, "Hide Slateport City Oceanic Museum 2F Aqua Grunt 2", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_OCEANIC_MUSEUM_2F_AQUA_GRUNT_2
        new(886, "Hide Slateport City Oceanic Museum 2F Archie", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_OCEANIC_MUSEUM_2F_ARCHIE
        new(887, "Hide Slateport City Oceanic Museum 2F Captain Stern", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_OCEANIC_MUSEUM_2F_CAPTAIN_STERN
        new(888, "Hide Battle Tower Opponent", "Hide"), // FLAG_HIDE_BATTLE_TOWER_OPPONENT
        new(889, "Hide Littleroot Town Birchs Lab Rival", "Hide"), // FLAG_HIDE_LITTLEROOT_TOWN_BIRCHS_LAB_RIVAL
        new(890, "Hide Route 119 Team Aqua", "Hide"), // FLAG_HIDE_ROUTE_119_TEAM_AQUA
        new(891, "Hide Route 116 Mr Briney", "Hide"), // FLAG_HIDE_ROUTE_116_MR_BRINEY
        new(892, "Hide Weather Institute 1F Workers", "Hide"), // FLAG_HIDE_WEATHER_INSTITUTE_1F_WORKERS
        new(893, "Hide Weather Institute 2F Workers", "Hide"), // FLAG_HIDE_WEATHER_INSTITUTE_2F_WORKERS
        new(894, "Hide Route 116 Wandas Boyfriend", "Hide"), // FLAG_HIDE_ROUTE_116_WANDAS_BOYFRIEND
        new(895, "Hide Lilycove Contest Hall Contest Attendant 2", "Hide"), // FLAG_HIDE_LILYCOVE_CONTEST_HALL_CONTEST_ATTENDANT_2
        new(897, "Hide Route 101 Birch", "Hide"), // FLAG_HIDE_ROUTE_101_BIRCH
        new(898, "Hide Route 103 Birch", "Hide"), // FLAG_HIDE_ROUTE_103_BIRCH
        new(899, "Hide Trick House End Man", "Hide"), // FLAG_HIDE_TRICK_HOUSE_END_MAN
        new(900, "Hide Route 110 Team Aqua", "Hide"), // FLAG_HIDE_ROUTE_110_TEAM_AQUA
        new(901, "Hide Route 118 Gabby And Ty 2", "Hide"), // FLAG_HIDE_ROUTE_118_GABBY_AND_TY_2
        new(902, "Hide Route 120 Gabby And Ty 2", "Hide"), // FLAG_HIDE_ROUTE_120_GABBY_AND_TY_2
        new(903, "Hide Route 111 Gabby And Ty 2", "Hide"), // FLAG_HIDE_ROUTE_111_GABBY_AND_TY_2
        new(904, "Hide Route 118 Gabby And Ty 3", "Hide"), // FLAG_HIDE_ROUTE_118_GABBY_AND_TY_3
        new(905, "Hide Slateport City Harbor Patrons", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_HARBOR_PATRONS
        new(906, "Hide Route 104 White Herb Florist", "Hide"), // FLAG_HIDE_ROUTE_104_WHITE_HERB_FLORIST
        new(907, "Hide Fallarbor Azurill", "Hide"), // FLAG_HIDE_FALLARBOR_AZURILL
        new(908, "Hide Lilycove Harbor Ferry Attendant", "Hide"), // FLAG_HIDE_LILYCOVE_HARBOR_FERRY_ATTENDANT
        new(909, "Hide Lilycove Harbor Ferry Sailor", "Hide"), // FLAG_HIDE_LILYCOVE_HARBOR_FERRY_SAILOR
        new(910, "Hide Southern Island Eon Stone", "Hide"), // FLAG_HIDE_SOUTHERN_ISLAND_EON_STONE
        new(911, "Hide Southern Island Unchosen Eon Duo Mon", "Hide"), // FLAG_HIDE_SOUTHERN_ISLAND_UNCHOSEN_EON_DUO_MON
        new(912, "Hide Mauville City Wattson", "Hide"), // FLAG_HIDE_MAUVILLE_CITY_WATTSON
        new(913, "Hide Mauville Gym Wattson", "Hide"), // FLAG_HIDE_MAUVILLE_GYM_WATTSON
        new(914, "Hide Route 121 Team Aqua Grunts", "Hide"), // FLAG_HIDE_ROUTE_121_TEAM_AQUA_GRUNTS
        new(916, "Hide Mt Pyre Summit Archie", "Hide"), // FLAG_HIDE_MT_PYRE_SUMMIT_ARCHIE
        new(917, "Hide Mt Pyre Summit Team Aqua", "Hide"), // FLAG_HIDE_MT_PYRE_SUMMIT_TEAM_AQUA
        new(918, "Hide Battle Tower Reporter", "Hide"), // FLAG_HIDE_BATTLE_TOWER_REPORTER
        new(919, "Hide Route 110 Rival", "Hide"), // FLAG_HIDE_ROUTE_110_RIVAL
        new(920, "Hide Champions Room Rival", "Hide"), // FLAG_HIDE_CHAMPIONS_ROOM_RIVAL
        new(921, "Hide Champions Room Birch", "Hide"), // FLAG_HIDE_CHAMPIONS_ROOM_BIRCH
        new(922, "Hide Route 110 Rival On Bike", "Hide"), // FLAG_HIDE_ROUTE_110_RIVAL_ON_BIKE
        new(923, "Hide Route 119 Rival On Bike", "Hide"), // FLAG_HIDE_ROUTE_119_RIVAL_ON_BIKE
        new(924, "Hide Aqua Hideout Grunts", "Hide"), // FLAG_HIDE_AQUA_HIDEOUT_GRUNTS
        new(925, "Hide Lilycove Motel Game Designers", "Hide"), // FLAG_HIDE_LILYCOVE_MOTEL_GAME_DESIGNERS
        new(926, "Hide Mt Chimney Team Aqua", "Hide"), // FLAG_HIDE_MT_CHIMNEY_TEAM_AQUA
        new(927, "Hide Mt Chimney Team Magma", "Hide"), // FLAG_HIDE_MT_CHIMNEY_TEAM_MAGMA
        new(928, "Hide Fallarbor House Prof Cozmo", "Hide"), // FLAG_HIDE_FALLARBOR_HOUSE_PROF_COZMO
        new(929, "Hide Lavaridge Town Rival", "Hide"), // FLAG_HIDE_LAVARIDGE_TOWN_RIVAL
        new(930, "Hide Lavaridge Town Rival On Bike", "Hide"), // FLAG_HIDE_LAVARIDGE_TOWN_RIVAL_ON_BIKE
        new(931, "Hide Rusturf Tunnel Rock 1", "Hide"), // FLAG_HIDE_RUSTURF_TUNNEL_ROCK_1
        new(932, "Hide Rusturf Tunnel Rock 2", "Hide"), // FLAG_HIDE_RUSTURF_TUNNEL_ROCK_2
        new(933, "Hide Fortree City House 4 Wingull", "Hide"), // FLAG_HIDE_FORTREE_CITY_HOUSE_4_WINGULL
        new(934, "Hide Mossdeep City House 2 Wingull", "Hide"), // FLAG_HIDE_MOSSDEEP_CITY_HOUSE_2_WINGULL
        new(935, "Hide Regirock", "Hide"), // FLAG_HIDE_REGIROCK
        new(936, "Hide Regice", "Hide"), // FLAG_HIDE_REGICE
        new(937, "Hide Registeel", "Hide"), // FLAG_HIDE_REGISTEEL
        new(938, "Hide Meteor Falls Team Aqua", "Hide"), // FLAG_HIDE_METEOR_FALLS_TEAM_AQUA
        new(939, "Hide Meteor Falls Team Magma", "Hide"), // FLAG_HIDE_METEOR_FALLS_TEAM_MAGMA
        new(940, "Hide Dewford Hall Sludge Bomb Man", "Hide"), // FLAG_HIDE_DEWFORD_HALL_SLUDGE_BOMB_MAN
        new(941, "Hide Seafloor Cavern Entrance Aqua Grunt", "Hide"), // FLAG_HIDE_SEAFLOOR_CAVERN_ENTRANCE_AQUA_GRUNT
        new(942, "Hide Meteor Falls 1F 1R Cozmo", "Hide"), // FLAG_HIDE_METEOR_FALLS_1F_1R_COZMO
        new(943, "Hide Aqua Hideout B2F Submarine Shadow", "Hide"), // FLAG_HIDE_AQUA_HIDEOUT_B2F_SUBMARINE_SHADOW
        new(944, "Hide Route 128 Archie", "Hide"), // FLAG_HIDE_ROUTE_128_ARCHIE
        new(945, "Hide Route 128 Maxie", "Hide"), // FLAG_HIDE_ROUTE_128_MAXIE
        new(946, "Hide Seafloor Cavern Aqua Grunts", "Hide"), // FLAG_HIDE_SEAFLOOR_CAVERN_AQUA_GRUNTS
        new(947, "Hide Route 116 Devon Employee", "Hide"), // FLAG_HIDE_ROUTE_116_DEVON_EMPLOYEE
        new(948, "Hide Slateport City Tm Salesman", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_TM_SALESMAN
        new(949, "Hide Rustboro City Devon Corp 3F Employee", "Hide"), // FLAG_HIDE_RUSTBORO_CITY_DEVON_CORP_3F_EMPLOYEE
        new(950, "Hide Ss Tidal Corridor Mr Briney", "Hide"), // FLAG_HIDE_SS_TIDAL_CORRIDOR_MR_BRINEY
        new(951, "Hide Ss Tidal Rooms Snatch Giver", "Hide"), // FLAG_HIDE_SS_TIDAL_ROOMS_SNATCH_GIVER
        new(952, "Received Shoal Salt 1", "Received"), // FLAG_RECEIVED_SHOAL_SALT_1
        new(953, "Received Shoal Salt 2", "Received"), // FLAG_RECEIVED_SHOAL_SALT_2
        new(954, "Received Shoal Salt 3", "Received"), // FLAG_RECEIVED_SHOAL_SALT_3
        new(955, "Received Shoal Salt 4", "Received"), // FLAG_RECEIVED_SHOAL_SALT_4
        new(956, "Received Shoal Shell 1", "Received"), // FLAG_RECEIVED_SHOAL_SHELL_1
        new(957, "Received Shoal Shell 2", "Received"), // FLAG_RECEIVED_SHOAL_SHELL_2
        new(958, "Received Shoal Shell 3", "Received"), // FLAG_RECEIVED_SHOAL_SHELL_3
        new(959, "Received Shoal Shell 4", "Received"), // FLAG_RECEIVED_SHOAL_SHELL_4
        new(960, "Hide Route 111 Secret Power Man", "Hide"), // FLAG_HIDE_ROUTE_111_SECRET_POWER_MAN
        new(961, "Hide Slateport Museum Population", "Hide"), // FLAG_HIDE_SLATEPORT_MUSEUM_POPULATION
        new(962, "Hide Lilycove Department Store Rooftop Sale Woman", "Hide"), // FLAG_HIDE_LILYCOVE_DEPARTMENT_STORE_ROOFTOP_SALE_WOMAN
        new(963, "Hide Mirage Tower Root Fossil", "Hide"), // FLAG_HIDE_MIRAGE_TOWER_ROOT_FOSSIL
        new(964, "Hide Mirage Tower Claw Fossil", "Hide"), // FLAG_HIDE_MIRAGE_TOWER_CLAW_FOSSIL
        new(965, "Hide Slateport City Oceanic Museum Familiar Aqua Grunt", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_OCEANIC_MUSEUM_FAMILIAR_AQUA_GRUNT
        new(966, "Hide Route 118 Steven", "Hide"), // FLAG_HIDE_ROUTE_118_STEVEN
        new(967, "Hide Mossdeep City Stevens House Steven", "Hide"), // FLAG_HIDE_MOSSDEEP_CITY_STEVENS_HOUSE_STEVEN
        new(968, "Hide Mossdeep City Stevens House Beldum Pokeball", "Hide"), // FLAG_HIDE_MOSSDEEP_CITY_STEVENS_HOUSE_BELDUM_POKEBALL
        new(969, "Hide Fortree City Kecleon", "Hide"), // FLAG_HIDE_FORTREE_CITY_KECLEON
        new(970, "Hide Route 120 Kecleon Bridge", "Hide"), // FLAG_HIDE_ROUTE_120_KECLEON_BRIDGE
        new(971, "Hide Lilycove City Rival", "Hide"), // FLAG_HIDE_LILYCOVE_CITY_RIVAL
        new(972, "Hide Route 120 Steven", "Hide"), // FLAG_HIDE_ROUTE_120_STEVEN
        new(973, "Hide Sootopolis City Steven", "Hide"), // FLAG_HIDE_SOOTOPOLIS_CITY_STEVEN
        new(974, "Hide New Mauville Voltorb 1", "Hide"), // FLAG_HIDE_NEW_MAUVILLE_VOLTORB_1
        new(975, "Hide New Mauville Voltorb 2", "Hide"), // FLAG_HIDE_NEW_MAUVILLE_VOLTORB_2
        new(976, "Hide New Mauville Voltorb 3", "Hide"), // FLAG_HIDE_NEW_MAUVILLE_VOLTORB_3
        new(977, "Hide Aqua Hideout B1F Electrode 1", "Hide"), // FLAG_HIDE_AQUA_HIDEOUT_B1F_ELECTRODE_1
        new(978, "Hide Aqua Hideout B1F Electrode 2", "Hide"), // FLAG_HIDE_AQUA_HIDEOUT_B1F_ELECTRODE_2
        new(979, "Hide Oldale Town Rival", "Hide"), // FLAG_HIDE_OLDALE_TOWN_RIVAL
        new(980, "Hide Underwater Sea Floor Cavern Stolen Submarine", "Hide"), // FLAG_HIDE_UNDERWATER_SEA_FLOOR_CAVERN_STOLEN_SUBMARINE
        new(981, "Hide Route 120 Kecleon Bridge Shadow", "Hide"), // FLAG_HIDE_ROUTE_120_KECLEON_BRIDGE_SHADOW
        new(982, "Hide Route 120 Kecleon 1", "Hide"), // FLAG_HIDE_ROUTE_120_KECLEON_1
        new(983, "Hide Rusturf Tunnel Wanda", "Hide"), // FLAG_HIDE_RUSTURF_TUNNEL_WANDA
        new(984, "Hide Verdanturf Town Wandas House Wanda", "Hide"), // FLAG_HIDE_VERDANTURF_TOWN_WANDAS_HOUSE_WANDA
        new(985, "Hide Route 120 Kecleon 2", "Hide"), // FLAG_HIDE_ROUTE_120_KECLEON_2
        new(986, "Hide Route 120 Kecleon 3", "Hide"), // FLAG_HIDE_ROUTE_120_KECLEON_3
        new(987, "Hide Route 120 Kecleon 4", "Hide"), // FLAG_HIDE_ROUTE_120_KECLEON_4
        new(988, "Hide Route 120 Kecleon 5", "Hide"), // FLAG_HIDE_ROUTE_120_KECLEON_5
        new(989, "Hide Route 119 Kecleon 1", "Hide"), // FLAG_HIDE_ROUTE_119_KECLEON_1
        new(990, "Hide Route 119 Kecleon 2", "Hide"), // FLAG_HIDE_ROUTE_119_KECLEON_2
        new(991, "Hide Route 101 Boy", "Hide"), // FLAG_HIDE_ROUTE_101_BOY
        new(992, "Hide Weather Institute 2F Aqua Grunt M", "Hide"), // FLAG_HIDE_WEATHER_INSTITUTE_2F_AQUA_GRUNT_M
        new(993, "Hide Lilycove Pokemon Center Contest Lady Mon", "Hide"), // FLAG_HIDE_LILYCOVE_POKEMON_CENTER_CONTEST_LADY_MON
        new(994, "Hide Mt Chimney Lava Cookie Lady", "Hide"), // FLAG_HIDE_MT_CHIMNEY_LAVA_COOKIE_LADY
        new(995, "Hide Petalburg City Scott", "Hide"), // FLAG_HIDE_PETALBURG_CITY_SCOTT
        new(996, "Hide Sootopolis City Rayquaza", "Hide"), // FLAG_HIDE_SOOTOPOLIS_CITY_RAYQUAZA
        new(997, "Hide Sootopolis City Kyogre", "Hide"), // FLAG_HIDE_SOOTOPOLIS_CITY_KYOGRE
        new(998, "Hide Sootopolis City Groudon", "Hide"), // FLAG_HIDE_SOOTOPOLIS_CITY_GROUDON
        new(999, "Hide Rustboro City Pokemon School Scott", "Hide"), // FLAG_HIDE_RUSTBORO_CITY_POKEMON_SCHOOL_SCOTT
        new(1000, "Item Route 102 Potion", "Item"), // FLAG_ITEM_ROUTE_102_POTION
        new(1001, "Item Route 116 X Special", "Item"), // FLAG_ITEM_ROUTE_116_X_SPECIAL
        new(1002, "Item Route 104 Pp Up", "Item"), // FLAG_ITEM_ROUTE_104_PP_UP
        new(1003, "Item Route 105 Iron", "Item"), // FLAG_ITEM_ROUTE_105_IRON
        new(1004, "Item Route 106 Protein", "Item"), // FLAG_ITEM_ROUTE_106_PROTEIN
        new(1005, "Item Route 109 Pp Up", "Item"), // FLAG_ITEM_ROUTE_109_PP_UP
        new(1006, "Item Route 110 Rare Candy", "Item"), // FLAG_ITEM_ROUTE_110_RARE_CANDY
        new(1007, "Item Route 110 Dire Hit", "Item"), // FLAG_ITEM_ROUTE_110_DIRE_HIT
        new(1008, "Item Route 111 Tm Sandstorm", "Item"), // FLAG_ITEM_ROUTE_111_TM_SANDSTORM
        new(1009, "Item Route 111 Stardust", "Item"), // FLAG_ITEM_ROUTE_111_STARDUST
        new(1010, "Item Route 111 Hp Up", "Item"), // FLAG_ITEM_ROUTE_111_HP_UP
        new(1011, "Item Route 112 Nugget", "Item"), // FLAG_ITEM_ROUTE_112_NUGGET
        new(1012, "Item Route 113 Max Ether", "Item"), // FLAG_ITEM_ROUTE_113_MAX_ETHER
        new(1013, "Item Route 113 Super Repel", "Item"), // FLAG_ITEM_ROUTE_113_SUPER_REPEL
        new(1014, "Item Route 114 Rare Candy", "Item"), // FLAG_ITEM_ROUTE_114_RARE_CANDY
        new(1015, "Item Route 114 Protein", "Item"), // FLAG_ITEM_ROUTE_114_PROTEIN
        new(1016, "Item Route 115 Super Potion", "Item"), // FLAG_ITEM_ROUTE_115_SUPER_POTION
        new(1017, "Item Route 115 Tm Focus Punch", "Item"), // FLAG_ITEM_ROUTE_115_TM_FOCUS_PUNCH
        new(1018, "Item Route 115 Iron", "Item"), // FLAG_ITEM_ROUTE_115_IRON
        new(1019, "Item Route 116 Ether", "Item"), // FLAG_ITEM_ROUTE_116_ETHER
        new(1020, "Item Route 116 Repel", "Item"), // FLAG_ITEM_ROUTE_116_REPEL
        new(1021, "Item Route 116 Hp Up", "Item"), // FLAG_ITEM_ROUTE_116_HP_UP
        new(1022, "Item Route 117 Great Ball", "Item"), // FLAG_ITEM_ROUTE_117_GREAT_BALL
        new(1023, "Item Route 117 Revive", "Item"), // FLAG_ITEM_ROUTE_117_REVIVE
        new(1024, "Item Route 119 Super Repel", "Item"), // FLAG_ITEM_ROUTE_119_SUPER_REPEL
        new(1025, "Item Route 119 Zinc", "Item"), // FLAG_ITEM_ROUTE_119_ZINC
        new(1026, "Item Route 119 Elixir 1", "Item"), // FLAG_ITEM_ROUTE_119_ELIXIR_1
        new(1027, "Item Route 119 Leaf Stone", "Item"), // FLAG_ITEM_ROUTE_119_LEAF_STONE
        new(1028, "Item Route 119 Rare Candy", "Item"), // FLAG_ITEM_ROUTE_119_RARE_CANDY
        new(1029, "Item Route 119 Hyper Potion 1", "Item"), // FLAG_ITEM_ROUTE_119_HYPER_POTION_1
        new(1030, "Item Route 120 Nugget", "Item"), // FLAG_ITEM_ROUTE_120_NUGGET
        new(1031, "Item Route 120 Full Heal", "Item"), // FLAG_ITEM_ROUTE_120_FULL_HEAL
        new(1032, "Item Route 123 Calcium", "Item"), // FLAG_ITEM_ROUTE_123_CALCIUM
        new(1033, "Item Route 134 Blue Shard", "Item"), // FLAG_ITEM_ROUTE_134_BLUE_SHARD
        new(1034, "Item Route 127 Zinc", "Item"), // FLAG_ITEM_ROUTE_127_ZINC
        new(1035, "Item Route 127 Carbos", "Item"), // FLAG_ITEM_ROUTE_127_CARBOS
        new(1036, "Item Route 132 Rare Candy", "Item"), // FLAG_ITEM_ROUTE_132_RARE_CANDY
        new(1037, "Item Route 133 Big Pearl", "Item"), // FLAG_ITEM_ROUTE_133_BIG_PEARL
        new(1038, "Item Route 133 Star Piece", "Item"), // FLAG_ITEM_ROUTE_133_STAR_PIECE
        new(1039, "Item Petalburg City Max Revive", "Item"), // FLAG_ITEM_PETALBURG_CITY_MAX_REVIVE
        new(1040, "Item Petalburg City Ether", "Item"), // FLAG_ITEM_PETALBURG_CITY_ETHER
        new(1041, "Item Rustboro City X Defend", "Item"), // FLAG_ITEM_RUSTBORO_CITY_X_DEFEND
        new(1042, "Item Lilycove City Max Repel", "Item"), // FLAG_ITEM_LILYCOVE_CITY_MAX_REPEL
        new(1043, "Item Mossdeep City Net Ball", "Item"), // FLAG_ITEM_MOSSDEEP_CITY_NET_BALL
        new(1044, "Item Meteor Falls 1F 1R Tm Iron Tail", "Item"), // FLAG_ITEM_METEOR_FALLS_1F_1R_TM_IRON_TAIL
        new(1045, "Item Meteor Falls 1F 1R Full Heal", "Item"), // FLAG_ITEM_METEOR_FALLS_1F_1R_FULL_HEAL
        new(1046, "Item Meteor Falls 1F 1R Moon Stone", "Item"), // FLAG_ITEM_METEOR_FALLS_1F_1R_MOON_STONE
        new(1047, "Item Meteor Falls 1F 1R Pp Up", "Item"), // FLAG_ITEM_METEOR_FALLS_1F_1R_PP_UP
        new(1048, "Item Rusturf Tunnel Poke Ball", "Item"), // FLAG_ITEM_RUSTURF_TUNNEL_POKE_BALL
        new(1049, "Item Rusturf Tunnel Max Ether", "Item"), // FLAG_ITEM_RUSTURF_TUNNEL_MAX_ETHER
        new(1050, "Item Granite Cave 1F Escape Rope", "Item"), // FLAG_ITEM_GRANITE_CAVE_1F_ESCAPE_ROPE
        new(1051, "Item Granite Cave B1F Poke Ball", "Item"), // FLAG_ITEM_GRANITE_CAVE_B1F_POKE_BALL
        new(1052, "Item Mt Pyre 5F Lax Incense", "Item"), // FLAG_ITEM_MT_PYRE_5F_LAX_INCENSE
        new(1053, "Item Granite Cave B2F Repel", "Item"), // FLAG_ITEM_GRANITE_CAVE_B2F_REPEL
        new(1054, "Item Granite Cave B2F Rare Candy", "Item"), // FLAG_ITEM_GRANITE_CAVE_B2F_RARE_CANDY
        new(1055, "Item Petalburg Woods X Attack", "Item"), // FLAG_ITEM_PETALBURG_WOODS_X_ATTACK
        new(1056, "Item Petalburg Woods Great Ball", "Item"), // FLAG_ITEM_PETALBURG_WOODS_GREAT_BALL
        new(1057, "Item Route 104 Poke Ball", "Item"), // FLAG_ITEM_ROUTE_104_POKE_BALL
        new(1058, "Item Petalburg Woods Ether", "Item"), // FLAG_ITEM_PETALBURG_WOODS_ETHER
        new(1059, "Item Magma Hideout 3F 3R Ecape Rope", "Item"), // FLAG_ITEM_MAGMA_HIDEOUT_3F_3R_ECAPE_ROPE
        new(1060, "Item Trick House Puzzle 1 Orange Mail", "Item"), // FLAG_ITEM_TRICK_HOUSE_PUZZLE_1_ORANGE_MAIL
        new(1061, "Item Trick House Puzzle 2 Harbor Mail", "Item"), // FLAG_ITEM_TRICK_HOUSE_PUZZLE_2_HARBOR_MAIL
        new(1062, "Item Trick House Puzzle 2 Wave Mail", "Item"), // FLAG_ITEM_TRICK_HOUSE_PUZZLE_2_WAVE_MAIL
        new(1063, "Item Trick House Puzzle 3 Shadow Mail", "Item"), // FLAG_ITEM_TRICK_HOUSE_PUZZLE_3_SHADOW_MAIL
        new(1064, "Item Trick House Puzzle 3 Wood Mail", "Item"), // FLAG_ITEM_TRICK_HOUSE_PUZZLE_3_WOOD_MAIL
        new(1065, "Item Trick House Puzzle 4 Mech Mail", "Item"), // FLAG_ITEM_TRICK_HOUSE_PUZZLE_4_MECH_MAIL
        new(1066, "Item Route 124 Yellow Shard", "Item"), // FLAG_ITEM_ROUTE_124_YELLOW_SHARD
        new(1067, "Item Trick House Puzzle 6 Glitter Mail", "Item"), // FLAG_ITEM_TRICK_HOUSE_PUZZLE_6_GLITTER_MAIL
        new(1068, "Item Trick House Puzzle 7 Tropic Mail", "Item"), // FLAG_ITEM_TRICK_HOUSE_PUZZLE_7_TROPIC_MAIL
        new(1069, "Item Trick House Puzzle 8 Bead Mail", "Item"), // FLAG_ITEM_TRICK_HOUSE_PUZZLE_8_BEAD_MAIL
        new(1070, "Item Jagged Pass Burn Heal", "Item"), // FLAG_ITEM_JAGGED_PASS_BURN_HEAL
        new(1071, "Item Aqua Hideout B1F Max Elixir", "Item"), // FLAG_ITEM_AQUA_HIDEOUT_B1F_MAX_ELIXIR
        new(1072, "Item Aqua Hideout B2F Nest Ball", "Item"), // FLAG_ITEM_AQUA_HIDEOUT_B2F_NEST_BALL
        new(1073, "Item Mt Pyre Exterior Max Potion", "Item"), // FLAG_ITEM_MT_PYRE_EXTERIOR_MAX_POTION
        new(1074, "Item Mt Pyre Exterior Tm Skill Swap", "Item"), // FLAG_ITEM_MT_PYRE_EXTERIOR_TM_SKILL_SWAP
        new(1075, "Item New Mauville Ultra Ball", "Item"), // FLAG_ITEM_NEW_MAUVILLE_ULTRA_BALL
        new(1076, "Item New Mauville Escape Rope", "Item"), // FLAG_ITEM_NEW_MAUVILLE_ESCAPE_ROPE
        new(1077, "Item Abandoned Ship Hidden Floor Room 6 Luxury Ball", "Item"), // FLAG_ITEM_ABANDONED_SHIP_HIDDEN_FLOOR_ROOM_6_LUXURY_BALL
        new(1078, "Item Abandoned Ship Hidden Floor Room 2 Scanner", "Item"), // FLAG_ITEM_ABANDONED_SHIP_HIDDEN_FLOOR_ROOM_2_SCANNER
        new(1079, "Item Scorched Slab Tm Sunny Day", "Item"), // FLAG_ITEM_SCORCHED_SLAB_TM_SUNNY_DAY
        new(1080, "Item Meteor Falls B1F 2R Tm Dragon Claw", "Item"), // FLAG_ITEM_METEOR_FALLS_B1F_2R_TM_DRAGON_CLAW
        new(1081, "Item Shoal Cave Entrance Big Pearl", "Item"), // FLAG_ITEM_SHOAL_CAVE_ENTRANCE_BIG_PEARL
        new(1082, "Item Shoal Cave Inner Room Rare Candy", "Item"), // FLAG_ITEM_SHOAL_CAVE_INNER_ROOM_RARE_CANDY
        new(1083, "Item Shoal Cave Stairs Room Ice Heal", "Item"), // FLAG_ITEM_SHOAL_CAVE_STAIRS_ROOM_ICE_HEAL
        new(1084, "Item Victory Road 1F Max Elixir", "Item"), // FLAG_ITEM_VICTORY_ROAD_1F_MAX_ELIXIR
        new(1085, "Item Victory Road 1F Pp Up", "Item"), // FLAG_ITEM_VICTORY_ROAD_1F_PP_UP
        new(1086, "Item Victory Road B1F Tm Psychic", "Item"), // FLAG_ITEM_VICTORY_ROAD_B1F_TM_PSYCHIC
        new(1087, "Item Victory Road B1F Full Restore", "Item"), // FLAG_ITEM_VICTORY_ROAD_B1F_FULL_RESTORE
        new(1088, "Item Victory Road B2F Full Heal", "Item"), // FLAG_ITEM_VICTORY_ROAD_B2F_FULL_HEAL
        new(1089, "Item Mt Pyre 6F Tm Shadow Ball", "Item"), // FLAG_ITEM_MT_PYRE_6F_TM_SHADOW_BALL
        new(1090, "Item Seafloor Cavern Room 9 Tm Earthquake", "Item"), // FLAG_ITEM_SEAFLOOR_CAVERN_ROOM_9_TM_EARTHQUAKE
        new(1091, "Item Fiery Path Tm Toxic", "Item"), // FLAG_ITEM_FIERY_PATH_TM_TOXIC
        new(1092, "Item Route 124 Red Shard", "Item"), // FLAG_ITEM_ROUTE_124_RED_SHARD
        new(1093, "Item Route 124 Blue Shard", "Item"), // FLAG_ITEM_ROUTE_124_BLUE_SHARD
        new(1094, "Item Safari Zone North West Tm Solar Beam", "Item"), // FLAG_ITEM_SAFARI_ZONE_NORTH_WEST_TM_SOLAR_BEAM
        new(1095, "Item Abandoned Ship Rooms 1F Harbor Mail", "Item"), // FLAG_ITEM_ABANDONED_SHIP_ROOMS_1F_HARBOR_MAIL
        new(1096, "Item Abandoned Ship Rooms B1F Escape Rope", "Item"), // FLAG_ITEM_ABANDONED_SHIP_ROOMS_B1F_ESCAPE_ROPE
        new(1097, "Item Abandoned Ship Rooms 2 B1F Dive Ball", "Item"), // FLAG_ITEM_ABANDONED_SHIP_ROOMS_2_B1F_DIVE_BALL
        new(1098, "Item Abandoned Ship Rooms B1F Tm Ice Beam", "Item"), // FLAG_ITEM_ABANDONED_SHIP_ROOMS_B1F_TM_ICE_BEAM
        new(1099, "Item Abandoned Ship Rooms 2 1F Revive", "Item"), // FLAG_ITEM_ABANDONED_SHIP_ROOMS_2_1F_REVIVE
        new(1100, "Item Abandoned Ship Captains Office Storage Key", "Item"), // FLAG_ITEM_ABANDONED_SHIP_CAPTAINS_OFFICE_STORAGE_KEY
        new(1101, "Item Abandoned Ship Hidden Floor Room 3 Water Stone", "Item"), // FLAG_ITEM_ABANDONED_SHIP_HIDDEN_FLOOR_ROOM_3_WATER_STONE
        new(1102, "Item Abandoned Ship Hidden Floor Room 1 Tm Rain Dance", "Item"), // FLAG_ITEM_ABANDONED_SHIP_HIDDEN_FLOOR_ROOM_1_TM_RAIN_DANCE
        new(1103, "Item Route 121 Carbos", "Item"), // FLAG_ITEM_ROUTE_121_CARBOS
        new(1104, "Item Route 123 Ultra Ball", "Item"), // FLAG_ITEM_ROUTE_123_ULTRA_BALL
        new(1105, "Item Route 126 Green Shard", "Item"), // FLAG_ITEM_ROUTE_126_GREEN_SHARD
        new(1106, "Item Route 119 Hyper Potion 2", "Item"), // FLAG_ITEM_ROUTE_119_HYPER_POTION_2
        new(1107, "Item Route 120 Hyper Potion", "Item"), // FLAG_ITEM_ROUTE_120_HYPER_POTION
        new(1108, "Item Route 120 Nest Ball", "Item"), // FLAG_ITEM_ROUTE_120_NEST_BALL
        new(1109, "Item Route 123 Elixir", "Item"), // FLAG_ITEM_ROUTE_123_ELIXIR
        new(1110, "Item New Mauville Thunder Stone", "Item"), // FLAG_ITEM_NEW_MAUVILLE_THUNDER_STONE
        new(1111, "Item Fiery Path Fire Stone", "Item"), // FLAG_ITEM_FIERY_PATH_FIRE_STONE
        new(1112, "Item Shoal Cave Ice Room Tm Hail", "Item"), // FLAG_ITEM_SHOAL_CAVE_ICE_ROOM_TM_HAIL
        new(1113, "Item Shoal Cave Ice Room Never Melt Ice", "Item"), // FLAG_ITEM_SHOAL_CAVE_ICE_ROOM_NEVER_MELT_ICE
        new(1114, "Item Route 103 Guard Spec", "Item"), // FLAG_ITEM_ROUTE_103_GUARD_SPEC
        new(1115, "Item Route 104 X Accuracy", "Item"), // FLAG_ITEM_ROUTE_104_X_ACCURACY
        new(1116, "Item Mauville City X Speed", "Item"), // FLAG_ITEM_MAUVILLE_CITY_X_SPEED
        new(1117, "Item Petalburg Woods Paralyze Heal", "Item"), // FLAG_ITEM_PETALBURG_WOODS_PARALYZE_HEAL
        new(1118, "Item Route 115 Great Ball", "Item"), // FLAG_ITEM_ROUTE_115_GREAT_BALL
        new(1119, "Item Safari Zone North Calcium", "Item"), // FLAG_ITEM_SAFARI_ZONE_NORTH_CALCIUM
        new(1120, "Item Mt Pyre 3F Super Repel", "Item"), // FLAG_ITEM_MT_PYRE_3F_SUPER_REPEL
        new(1121, "Item Route 118 Hyper Potion", "Item"), // FLAG_ITEM_ROUTE_118_HYPER_POTION
        new(1122, "Item New Mauville Full Heal", "Item"), // FLAG_ITEM_NEW_MAUVILLE_FULL_HEAL
        new(1123, "Item New Mauville Paralyze Heal", "Item"), // FLAG_ITEM_NEW_MAUVILLE_PARALYZE_HEAL
        new(1124, "Item Aqua Hideout B1F Master Ball", "Item"), // FLAG_ITEM_AQUA_HIDEOUT_B1F_MASTER_BALL
        new(1125, "Item Old Magma Hideout B1F Master Ball", "Item"), // FLAG_ITEM_OLD_MAGMA_HIDEOUT_B1F_MASTER_BALL
        new(1126, "Item Old Magma Hideout B1F Max Elixir", "Item"), // FLAG_ITEM_OLD_MAGMA_HIDEOUT_B1F_MAX_ELIXIR
        new(1127, "Item Old Magma Hideout B2F Nest Ball", "Item"), // FLAG_ITEM_OLD_MAGMA_HIDEOUT_B2F_NEST_BALL
        new(1128, "Item Route 108 Blue Shard", "Item"), // FLAG_ITEM_ROUTE_108_BLUE_SHARD
        new(1129, "Item Mt Pyre 2F Ultra Ball", "Item"), // FLAG_ITEM_MT_PYRE_2F_ULTRA_BALL
        new(1130, "Item Mt Pyre 4F Sea Incense", "Item"), // FLAG_ITEM_MT_PYRE_4F_SEA_INCENSE
        new(1131, "Item Safari Zone South West Max Revive", "Item"), // FLAG_ITEM_SAFARI_ZONE_SOUTH_WEST_MAX_REVIVE
        new(1132, "Item Aqua Hideout B1F Nugget", "Item"), // FLAG_ITEM_AQUA_HIDEOUT_B1F_NUGGET
        new(1133, "Item Route 125 Yellow Shard", "Item"), // FLAG_ITEM_ROUTE_125_YELLOW_SHARD
        new(1134, "Item Route 119 Nugget", "Item"), // FLAG_ITEM_ROUTE_119_NUGGET
        new(1135, "Item Route 104 Potion", "Item"), // FLAG_ITEM_ROUTE_104_POTION
        new(1136, "Item Route 134 Red Shard", "Item"), // FLAG_ITEM_ROUTE_134_RED_SHARD
        new(1137, "Item Route 103 Pp Up", "Item"), // FLAG_ITEM_ROUTE_103_PP_UP
        new(1138, "Item Route 132 Green Shard", "Item"), // FLAG_ITEM_ROUTE_132_GREEN_SHARD
        new(1139, "Item Route 108 Star Piece", "Item"), // FLAG_ITEM_ROUTE_108_STAR_PIECE
        new(1140, "Item Route 109 Potion", "Item"), // FLAG_ITEM_ROUTE_109_POTION
        new(1141, "Item Route 110 Elixir", "Item"), // FLAG_ITEM_ROUTE_110_ELIXIR
        new(1142, "Item Route 111 Elixir", "Item"), // FLAG_ITEM_ROUTE_111_ELIXIR
        new(1143, "Item Route 113 Hyper Potion", "Item"), // FLAG_ITEM_ROUTE_113_HYPER_POTION
        new(1144, "Item Route 115 Heal Powder", "Item"), // FLAG_ITEM_ROUTE_115_HEAL_POWDER
        new(1145, "Item Route 128 Yellow Shard", "Item"), // FLAG_ITEM_ROUTE_128_YELLOW_SHARD
        new(1146, "Item Route 116 Potion", "Item"), // FLAG_ITEM_ROUTE_116_POTION
        new(1147, "Item Route 119 Elixir 2", "Item"), // FLAG_ITEM_ROUTE_119_ELIXIR_2
        new(1148, "Item Route 120 Revive", "Item"), // FLAG_ITEM_ROUTE_120_REVIVE
        new(1149, "Item Route 121 Revive", "Item"), // FLAG_ITEM_ROUTE_121_REVIVE
        new(1150, "Item Route 121 Zinc", "Item"), // FLAG_ITEM_ROUTE_121_ZINC
        new(1151, "Item Magma Hideout 1F Rare Candy", "Item"), // FLAG_ITEM_MAGMA_HIDEOUT_1F_RARE_CANDY
        new(1152, "Item Route 123 Pp Up", "Item"), // FLAG_ITEM_ROUTE_123_PP_UP
        new(1153, "Item Route 123 Revival Herb", "Item"), // FLAG_ITEM_ROUTE_123_REVIVAL_HERB
        new(1154, "Item Route 125 Big Pearl", "Item"), // FLAG_ITEM_ROUTE_125_BIG_PEARL
        new(1155, "Item Route 127 Rare Candy", "Item"), // FLAG_ITEM_ROUTE_127_RARE_CANDY
        new(1156, "Item Route 132 Protein", "Item"), // FLAG_ITEM_ROUTE_132_PROTEIN
        new(1157, "Item Route 133 Max Revive", "Item"), // FLAG_ITEM_ROUTE_133_MAX_REVIVE
        new(1158, "Item Route 134 Carbos", "Item"), // FLAG_ITEM_ROUTE_134_CARBOS
        new(1159, "Item Route 134 Star Piece", "Item"), // FLAG_ITEM_ROUTE_134_STAR_PIECE
        new(1160, "Item Route 114 Energy Powder", "Item"), // FLAG_ITEM_ROUTE_114_ENERGY_POWDER
        new(1161, "Item Route 115 Pp Up", "Item"), // FLAG_ITEM_ROUTE_115_PP_UP
        new(1162, "Item Artisan Cave B1F Hp Up", "Item"), // FLAG_ITEM_ARTISAN_CAVE_B1F_HP_UP
        new(1163, "Item Artisan Cave 1F Carbos", "Item"), // FLAG_ITEM_ARTISAN_CAVE_1F_CARBOS
        new(1164, "Item Magma Hideout 2F 2R Max Elixir", "Item"), // FLAG_ITEM_MAGMA_HIDEOUT_2F_2R_MAX_ELIXIR
        new(1165, "Item Magma Hideout 2F 2R Full Restore", "Item"), // FLAG_ITEM_MAGMA_HIDEOUT_2F_2R_FULL_RESTORE
        new(1166, "Item Magma Hideout 3F 1R Nugget", "Item"), // FLAG_ITEM_MAGMA_HIDEOUT_3F_1R_NUGGET
        new(1167, "Item Magma Hideout 3F 2R Pp Max", "Item"), // FLAG_ITEM_MAGMA_HIDEOUT_3F_2R_PP_MAX
        new(1168, "Item Magma Hideout 4F Max Revive", "Item"), // FLAG_ITEM_MAGMA_HIDEOUT_4F_MAX_REVIVE
        new(1169, "Item Safari Zone North East Nugget", "Item"), // FLAG_ITEM_SAFARI_ZONE_NORTH_EAST_NUGGET
        new(1170, "Item Safari Zone South East Big Pearl", "Item"), // FLAG_ITEM_SAFARI_ZONE_SOUTH_EAST_BIG_PEARL
        new(1171, "Hide Route 111 Rock 1", "Hide"), // FLAG_HIDE_ROUTE_111_ROCK_1
        new(1172, "Hide Route 111 Rock 2", "Hide"), // FLAG_HIDE_ROUTE_111_ROCK_2
        new(1173, "Hide Route 116 Tree 1", "Hide"), // FLAG_HIDE_ROUTE_116_TREE_1
        new(1174, "Hide Route 116 Tree 2", "Hide"), // FLAG_HIDE_ROUTE_116_TREE_2
        new(1175, "Hide Route 116 Tree 3", "Hide"), // FLAG_HIDE_ROUTE_116_TREE_3
        new(1176, "Hide Route 116 Tree 4", "Hide"), // FLAG_HIDE_ROUTE_116_TREE_4
        new(1177, "Hide Route 116 Tree 5", "Hide"), // FLAG_HIDE_ROUTE_116_TREE_5
        new(1178, "Hide Mt Pyre Summit Matt", "Hide"), // FLAG_HIDE_MT_PYRE_SUMMIT_MATT
        new(1179, "Hide Mt Pyre Summit Team Magma", "Hide"), // FLAG_HIDE_MT_PYRE_SUMMIT_TEAM_MAGMA
        new(1180, "Archie Left Marine Cave", "Misc"), // FLAG_ARCHIE_LEFT_MARINE_CAVE
        new(1181, "Maxie Left Terra Cave", "Misc"), // FLAG_MAXIE_LEFT_TERRA_CAVE
        new(1182, "Hide Archie Marine Cave", "Hide"), // FLAG_HIDE_ARCHIE_MARINE_CAVE
        new(1183, "Hide Maxie Terra Cave", "Hide"), // FLAG_HIDE_MAXIE_TERRA_CAVE
        new(1184, "Hide Sootopolis City May", "Hide"), // FLAG_HIDE_SOOTOPOLIS_CITY_MAY
        new(1185, "Hide Sootopolis City Brendan", "Hide"), // FLAG_HIDE_SOOTOPOLIS_CITY_BRENDAN
        new(1186, "Sootopolis City Rival Professor", "Misc"), // FLAG_SOOTOPOLIS_CITY_RIVAL_PROFESSOR
        new(1187, "Rival Trade 1 Completed", "Misc"), // FLAG_RIVAL_TRADE_1_COMPLETED
        new(1188, "Rival Trade 2 Completed", "Misc"), // FLAG_RIVAL_TRADE_2_COMPLETED
        new(1189, "Got Roxanne Rematch Reward", "Misc"), // FLAG_GOT_ROXANNE_REMATCH_REWARD
        new(1190, "Got Brawly Rematch Reward", "Misc"), // FLAG_GOT_BRAWLY_REMATCH_REWARD
        new(1191, "Got Wattson Rematch Reward", "Misc"), // FLAG_GOT_WATTSON_REMATCH_REWARD
        new(1192, "Got Flannery Rematch Reward", "Misc"), // FLAG_GOT_FLANNERY_REMATCH_REWARD
        new(1193, "Got Norman Rematch Reward", "Misc"), // FLAG_GOT_NORMAN_REMATCH_REWARD
        new(1194, "Got Winona Rematch Reward", "Misc"), // FLAG_GOT_WINONA_REMATCH_REWARD
        new(1195, "Got Tateandliza Rematch Reward", "Misc"), // FLAG_GOT_TATEANDLIZA_REMATCH_REWARD
        new(1196, "Got Juan Rematch Reward", "Misc"), // FLAG_GOT_JUAN_REMATCH_REWARD
        new(1197, "Hide Helix Fossil", "Hide"), // FLAG_HIDE_HELIX_FOSSIL
        new(1198, "Hide Dome Fossil", "Hide"), // FLAG_HIDE_DOME_FOSSIL
        new(1199, "Got Cyndaquil", "Misc"), // FLAG_GOT_CYNDAQUIL
        new(1200, "Got Chikorita", "Misc"), // FLAG_GOT_CHIKORITA
        new(1201, "Got Totodile", "Misc"), // FLAG_GOT_TOTODILE
        new(1202, "Trainer Hill Snorlax Ready", "Misc"), // FLAG_TRAINER_HILL_SNORLAX_READY
        new(1203, "Got Trainer Hill Snorlax", "Misc"), // FLAG_GOT_TRAINER_HILL_SNORLAX
        new(1204, "Rival Got All Kanto Mons", "Misc"), // FLAG_RIVAL_GOT_ALL_KANTO_MONS
        new(1205, "Rival Got Bulbasaur", "Misc"), // FLAG_RIVAL_GOT_BULBASAUR
        new(1206, "Rival Got Charmander", "Misc"), // FLAG_RIVAL_GOT_CHARMANDER
        new(1207, "Rival Got Squirtle", "Misc"), // FLAG_RIVAL_GOT_SQUIRTLE
        new(1208, "Hide Smith", "Hide"), // FLAG_HIDE_SMITH
        new(1209, "Received Regice Doll", "Received"), // FLAG_RECEIVED_REGICE_DOLL
        new(1210, "Received Registeel Doll", "Received"), // FLAG_RECEIVED_REGISTEEL_DOLL
        new(1211, "Received Regirock Doll", "Received"), // FLAG_RECEIVED_REGIROCK_DOLL
        new(1212, "Defeated Smith", "Defeated"), // FLAG_DEFEATED_SMITH
        new(1213, "Defeated Craig", "Defeated"), // FLAG_DEFEATED_CRAIG
        new(1214, "Defeated Weebra", "Defeated"), // FLAG_DEFEATED_WEEBRA
        new(1215, "Hide Weebra", "Hide"), // FLAG_HIDE_WEEBRA
        new(1216, "Hide Craig", "Hide"), // FLAG_HIDE_CRAIG
        new(1217, "Open Cave Of Origin", "Misc"), // FLAG_OPEN_CAVE_OF_ORIGIN
        new(1218, "Shiny Creation", "Misc"), // FLAG_SHINY_CREATION
        new(1219, "Got Starf Berry", "Misc"), // FLAG_GOT_STARF_BERRY
        new(1220, "Scott Recieved Lansat Berry", "Misc"), // FLAG_SCOTT_RECIEVED_LANSAT_BERRY
        new(1221, "Scott Recieved Hp Up", "Misc"), // FLAG_SCOTT_RECIEVED_HP_UP
        new(1222, "Scott Recieved Protein", "Misc"), // FLAG_SCOTT_RECIEVED_PROTEIN
        new(1223, "Scott Recieved Iron", "Misc"), // FLAG_SCOTT_RECIEVED_IRON
        new(1224, "Scott Recieved Calcium", "Misc"), // FLAG_SCOTT_RECIEVED_CALCIUM
        new(1225, "Scott Recieved Zinc", "Misc"), // FLAG_SCOTT_RECIEVED_ZINC
        new(1226, "Scott Recieved Carbos", "Misc"), // FLAG_SCOTT_RECIEVED_CARBOS
        new(1227, "Beat Archie Marine Cave", "Misc"), // FLAG_BEAT_ARCHIE_MARINE_CAVE
        new(1228, "Beat Maxie Terra Cave", "Misc"), // FLAG_BEAT_MAXIE_TERRA_CAVE
        new(1229, "Hide Route 103 Tree 1", "Hide"), // FLAG_HIDE_ROUTE_103_TREE_1
        new(1230, "Hide Slateport City Incense Woman", "Hide"), // FLAG_HIDE_SLATEPORT_CITY_INCENSE_WOMAN
        new(1231, "Caught Unown A", "Misc"), // FLAG_CAUGHT_UNOWN_A
        new(1232, "Show Hidden Power", "Misc"), // FLAG_SHOW_HIDDEN_POWER
        new(1264, "Defeated Rustboro Gym", "Defeated"), // FLAG_DEFEATED_RUSTBORO_GYM
        new(1265, "Defeated Dewford Gym", "Defeated"), // FLAG_DEFEATED_DEWFORD_GYM
        new(1266, "Defeated Mauville Gym", "Defeated"), // FLAG_DEFEATED_MAUVILLE_GYM
        new(1267, "Defeated Lavaridge Gym", "Defeated"), // FLAG_DEFEATED_LAVARIDGE_GYM
        new(1268, "Defeated Petalburg Gym", "Defeated"), // FLAG_DEFEATED_PETALBURG_GYM
        new(1269, "Defeated Fortree Gym", "Defeated"), // FLAG_DEFEATED_FORTREE_GYM
        new(1270, "Defeated Mossdeep Gym", "Defeated"), // FLAG_DEFEATED_MOSSDEEP_GYM
        new(1271, "Defeated Sootopolis Gym", "Defeated"), // FLAG_DEFEATED_SOOTOPOLIS_GYM
        new(1272, "Defeated Meteor Falls Steven", "Defeated"), // FLAG_DEFEATED_METEOR_FALLS_STEVEN
        new(1273, "Nuzlocke", "Misc"), // FLAG_NUZLOCKE
        new(1274, "Hard", "Misc"), // FLAG_HARD
        new(1275, "Defeated Elite 4 Sidney", "Defeated"), // FLAG_DEFEATED_ELITE_4_SIDNEY
        new(1276, "Defeated Elite 4 Phoebe", "Defeated"), // FLAG_DEFEATED_ELITE_4_PHOEBE
        new(1277, "Defeated Elite 4 Glacia", "Defeated"), // FLAG_DEFEATED_ELITE_4_GLACIA
        new(1278, "Defeated Elite 4 Drake", "Defeated"), // FLAG_DEFEATED_ELITE_4_DRAKE
        new(2240, "Sys Pokemon Get", "System"), // FLAG_SYS_POKEMON_GET
        new(2241, "Sys Pokedex Get", "System"), // FLAG_SYS_POKEDEX_GET
        new(2242, "Sys Pokenav Get", "System"), // FLAG_SYS_POKENAV_GET
        new(2244, "Sys Game Clear", "System"), // FLAG_SYS_GAME_CLEAR
        new(2245, "Sys Chat Used", "System"), // FLAG_SYS_CHAT_USED
        new(2246, "Unlocked Trendy Sayings", "Misc"), // FLAG_UNLOCKED_TRENDY_SAYINGS
        new(2247, "Badge01 Get", "Badge"), // FLAG_BADGE01_GET
        new(2248, "Badge02 Get", "Badge"), // FLAG_BADGE02_GET
        new(2249, "Badge03 Get", "Badge"), // FLAG_BADGE03_GET
        new(2250, "Badge04 Get", "Badge"), // FLAG_BADGE04_GET
        new(2251, "Badge05 Get", "Badge"), // FLAG_BADGE05_GET
        new(2252, "Badge06 Get", "Badge"), // FLAG_BADGE06_GET
        new(2253, "Badge07 Get", "Badge"), // FLAG_BADGE07_GET
        new(2254, "Badge08 Get", "Badge"), // FLAG_BADGE08_GET
        new(2255, "Visited Littleroot Town", "Visited"), // FLAG_VISITED_LITTLEROOT_TOWN
        new(2256, "Visited Oldale Town", "Visited"), // FLAG_VISITED_OLDALE_TOWN
        new(2257, "Visited Dewford Town", "Visited"), // FLAG_VISITED_DEWFORD_TOWN
        new(2258, "Visited Lavaridge Town", "Visited"), // FLAG_VISITED_LAVARIDGE_TOWN
        new(2259, "Visited Fallarbor Town", "Visited"), // FLAG_VISITED_FALLARBOR_TOWN
        new(2260, "Visited Verdanturf Town", "Visited"), // FLAG_VISITED_VERDANTURF_TOWN
        new(2261, "Visited Pacifidlog Town", "Visited"), // FLAG_VISITED_PACIFIDLOG_TOWN
        new(2262, "Visited Petalburg City", "Visited"), // FLAG_VISITED_PETALBURG_CITY
        new(2263, "Visited Slateport City", "Visited"), // FLAG_VISITED_SLATEPORT_CITY
        new(2264, "Visited Mauville City", "Visited"), // FLAG_VISITED_MAUVILLE_CITY
        new(2265, "Visited Rustboro City", "Visited"), // FLAG_VISITED_RUSTBORO_CITY
        new(2266, "Visited Fortree City", "Visited"), // FLAG_VISITED_FORTREE_CITY
        new(2267, "Visited Lilycove City", "Visited"), // FLAG_VISITED_LILYCOVE_CITY
        new(2268, "Visited Mossdeep City", "Visited"), // FLAG_VISITED_MOSSDEEP_CITY
        new(2269, "Visited Sootopolis City", "Visited"), // FLAG_VISITED_SOOTOPOLIS_CITY
        new(2270, "Visited Ever Grande City", "Visited"), // FLAG_VISITED_EVER_GRANDE_CITY
        new(2271, "Is Champion", "Misc"), // FLAG_IS_CHAMPION
        new(2272, "Nurse Union Room Reminder", "Misc"), // FLAG_NURSE_UNION_ROOM_REMINDER
        new(2280, "Sys Use Flash", "System"), // FLAG_SYS_USE_FLASH
        new(2281, "Sys Use Strength", "System"), // FLAG_SYS_USE_STRENGTH
        new(2282, "Sys Weather Ctrl", "System"), // FLAG_SYS_WEATHER_CTRL
        new(2283, "Sys Cycling Road", "System"), // FLAG_SYS_CYCLING_ROAD
        new(2284, "Sys Safari Mode", "System"), // FLAG_SYS_SAFARI_MODE
        new(2285, "Sys Cruise Mode", "System"), // FLAG_SYS_CRUISE_MODE
        new(2288, "Sys Tv Home", "System"), // FLAG_SYS_TV_HOME
        new(2289, "Sys Tv Watch", "System"), // FLAG_SYS_TV_WATCH
        new(2290, "Sys Tv Start", "System"), // FLAG_SYS_TV_START
        new(2291, "Sys Changed Dewford Trend", "System"), // FLAG_SYS_CHANGED_DEWFORD_TREND
        new(2292, "Sys Mix Record", "System"), // FLAG_SYS_MIX_RECORD
        new(2293, "Sys Clock Set", "System"), // FLAG_SYS_CLOCK_SET
        new(2294, "Sys National Dex", "System"), // FLAG_SYS_NATIONAL_DEX
        new(2295, "Sys Cave Ship", "System"), // FLAG_SYS_CAVE_SHIP
        new(2296, "Sys Cave Wonder", "System"), // FLAG_SYS_CAVE_WONDER
        new(2297, "Sys Cave Battle", "System"), // FLAG_SYS_CAVE_BATTLE
        new(2298, "Sys Shoal Tide", "System"), // FLAG_SYS_SHOAL_TIDE
        new(2299, "Sys Ribbon Get", "System"), // FLAG_SYS_RIBBON_GET
        new(2300, "Landmark Flower Shop", "Landmark"), // FLAG_LANDMARK_FLOWER_SHOP
        new(2301, "Landmark Mr Briney House", "Landmark"), // FLAG_LANDMARK_MR_BRINEY_HOUSE
        new(2302, "Landmark Abandoned Ship", "Landmark"), // FLAG_LANDMARK_ABANDONED_SHIP
        new(2303, "Landmark Seashore House", "Landmark"), // FLAG_LANDMARK_SEASHORE_HOUSE
        new(2304, "Landmark New Mauville", "Landmark"), // FLAG_LANDMARK_NEW_MAUVILLE
        new(2305, "Landmark Old Lady Rest Shop", "Landmark"), // FLAG_LANDMARK_OLD_LADY_REST_SHOP
        new(2306, "Landmark Trick House", "Landmark"), // FLAG_LANDMARK_TRICK_HOUSE
        new(2307, "Landmark Winstrate Family", "Landmark"), // FLAG_LANDMARK_WINSTRATE_FAMILY
        new(2308, "Landmark Glass Workshop", "Landmark"), // FLAG_LANDMARK_GLASS_WORKSHOP
        new(2309, "Landmark Lanettes House", "Landmark"), // FLAG_LANDMARK_LANETTES_HOUSE
        new(2310, "Landmark Pokemon Daycare", "Landmark"), // FLAG_LANDMARK_POKEMON_DAYCARE
        new(2311, "Landmark Seafloor Cavern", "Landmark"), // FLAG_LANDMARK_SEAFLOOR_CAVERN
        new(2312, "Landmark Battle Frontier", "Landmark"), // FLAG_LANDMARK_BATTLE_FRONTIER
        new(2313, "Landmark Southern Island", "Landmark"), // FLAG_LANDMARK_SOUTHERN_ISLAND
        new(2314, "Landmark Fiery Path", "Landmark"), // FLAG_LANDMARK_FIERY_PATH
        new(2315, "Sys Pc Lanette", "System"), // FLAG_SYS_PC_LANETTE
        new(2316, "Sys Mystery Event Enable", "System"), // FLAG_SYS_MYSTERY_EVENT_ENABLE
        new(2317, "Sys Enc Up Item", "System"), // FLAG_SYS_ENC_UP_ITEM
        new(2318, "Sys Enc Down Item", "System"), // FLAG_SYS_ENC_DOWN_ITEM
        new(2319, "Sys Braille Dig", "System"), // FLAG_SYS_BRAILLE_DIG
        new(2320, "Sys Regirock Puzzle Completed", "System"), // FLAG_SYS_REGIROCK_PUZZLE_COMPLETED
        new(2321, "Sys Braille Regice Completed", "System"), // FLAG_SYS_BRAILLE_REGICE_COMPLETED
        new(2322, "Sys Registeel Puzzle Completed", "System"), // FLAG_SYS_REGISTEEL_PUZZLE_COMPLETED
        new(2323, "Enable Ship Southern Island", "Enable"), // FLAG_ENABLE_SHIP_SOUTHERN_ISLAND
        new(2324, "Landmark Pokemon League", "Landmark"), // FLAG_LANDMARK_POKEMON_LEAGUE
        new(2325, "Landmark Island Cave", "Landmark"), // FLAG_LANDMARK_ISLAND_CAVE
        new(2326, "Landmark Desert Ruins", "Landmark"), // FLAG_LANDMARK_DESERT_RUINS
        new(2327, "Landmark Fossil Maniacs House", "Landmark"), // FLAG_LANDMARK_FOSSIL_MANIACS_HOUSE
        new(2328, "Landmark Scorched Slab", "Landmark"), // FLAG_LANDMARK_SCORCHED_SLAB
        new(2329, "Landmark Ancient Tomb", "Landmark"), // FLAG_LANDMARK_ANCIENT_TOMB
        new(2330, "Landmark Tunnelers Rest House", "Landmark"), // FLAG_LANDMARK_TUNNELERS_REST_HOUSE
        new(2331, "Landmark Hunters House", "Landmark"), // FLAG_LANDMARK_HUNTERS_HOUSE
        new(2332, "Landmark Sealed Chamber", "Landmark"), // FLAG_LANDMARK_SEALED_CHAMBER
        new(2333, "Sys Tv Latias Latios", "System"), // FLAG_SYS_TV_LATIAS_LATIOS
        new(2334, "Landmark Sky Pillar", "Landmark"), // FLAG_LANDMARK_SKY_PILLAR
        new(2335, "Sys Shoal Item", "System"), // FLAG_SYS_SHOAL_ITEM
        new(2336, "Sys B Dash", "System"), // FLAG_SYS_B_DASH
        new(2337, "Sys Ctrl Obj Delete", "System"), // FLAG_SYS_CTRL_OBJ_DELETE
        new(2338, "Sys Reset Rtc Enable", "System"), // FLAG_SYS_RESET_RTC_ENABLE
        new(2339, "Landmark Berry Masters House", "Landmark"), // FLAG_LANDMARK_BERRY_MASTERS_HOUSE
        new(2340, "Sys Tower Silver", "System"), // FLAG_SYS_TOWER_SILVER
        new(2341, "Sys Tower Gold", "System"), // FLAG_SYS_TOWER_GOLD
        new(2342, "Sys Dome Silver", "System"), // FLAG_SYS_DOME_SILVER
        new(2343, "Sys Dome Gold", "System"), // FLAG_SYS_DOME_GOLD
        new(2344, "Sys Palace Silver", "System"), // FLAG_SYS_PALACE_SILVER
        new(2345, "Sys Palace Gold", "System"), // FLAG_SYS_PALACE_GOLD
        new(2346, "Sys Arena Silver", "System"), // FLAG_SYS_ARENA_SILVER
        new(2347, "Sys Arena Gold", "System"), // FLAG_SYS_ARENA_GOLD
        new(2348, "Sys Factory Silver", "System"), // FLAG_SYS_FACTORY_SILVER
        new(2349, "Sys Factory Gold", "System"), // FLAG_SYS_FACTORY_GOLD
        new(2350, "Sys Pike Silver", "System"), // FLAG_SYS_PIKE_SILVER
        new(2351, "Sys Pike Gold", "System"), // FLAG_SYS_PIKE_GOLD
        new(2352, "Sys Pyramid Silver", "System"), // FLAG_SYS_PYRAMID_SILVER
        new(2353, "Sys Pyramid Gold", "System"), // FLAG_SYS_PYRAMID_GOLD
        new(2354, "Sys Frontier Pass", "System"), // FLAG_SYS_FRONTIER_PASS
        new(2355, "Map Script Checked Deoxys", "Misc"), // FLAG_MAP_SCRIPT_CHECKED_DEOXYS
        new(2356, "Deoxys Rock Complete", "Misc"), // FLAG_DEOXYS_ROCK_COMPLETE
        new(2357, "Enable Ship Birth Island", "Enable"), // FLAG_ENABLE_SHIP_BIRTH_ISLAND
        new(2358, "Enable Ship Faraway Island", "Enable"), // FLAG_ENABLE_SHIP_FARAWAY_ISLAND
        new(2359, "Shown Box Was Full Message", "Misc"), // FLAG_SHOWN_BOX_WAS_FULL_MESSAGE
        new(2360, "Arrived On Faraway Island", "Misc"), // FLAG_ARRIVED_ON_FARAWAY_ISLAND
        new(2361, "Arrived At Marine Cave Emerge Spot", "Misc"), // FLAG_ARRIVED_AT_MARINE_CAVE_EMERGE_SPOT
        new(2362, "Arrived At Terra Cave Entrance", "Misc"), // FLAG_ARRIVED_AT_TERRA_CAVE_ENTRANCE
        new(2363, "Sys Mystery Gift Enable", "System"), // FLAG_SYS_MYSTERY_GIFT_ENABLE
        new(2364, "Entered Mirage Tower", "Misc"), // FLAG_ENTERED_MIRAGE_TOWER
        new(2365, "Landmark Altering Cave", "Landmark"), // FLAG_LANDMARK_ALTERING_CAVE
        new(2366, "Landmark Desert Underpass", "Landmark"), // FLAG_LANDMARK_DESERT_UNDERPASS
        new(2367, "Landmark Artisan Cave", "Landmark"), // FLAG_LANDMARK_ARTISAN_CAVE
        new(2368, "Enable Ship Navel Rock", "Enable"), // FLAG_ENABLE_SHIP_NAVEL_ROCK
        new(2369, "Arrived At Navel Rock", "Misc"), // FLAG_ARRIVED_AT_NAVEL_ROCK
        new(2370, "Landmark Trainer Hill", "Landmark"), // FLAG_LANDMARK_TRAINER_HILL
        new(2372, "Received Pokedex From Birch", "Received"), // FLAG_RECEIVED_POKEDEX_FROM_BIRCH
    ];

    internal static readonly EventLabel[] Work =
    [
        new(0, "Temp 0", "Misc"), // VAR_TEMP_0
        new(1, "Temp 1", "Misc"), // VAR_TEMP_1
        new(2, "Temp 2", "Misc"), // VAR_TEMP_2
        new(3, "Temp 3", "Misc"), // VAR_TEMP_3
        new(4, "Temp 4", "Misc"), // VAR_TEMP_4
        new(5, "Temp 5", "Misc"), // VAR_TEMP_5
        new(6, "Temp 6", "Misc"), // VAR_TEMP_6
        new(7, "Temp 7", "Misc"), // VAR_TEMP_7
        new(8, "Temp 8", "Misc"), // VAR_TEMP_8
        new(9, "Temp 9", "Misc"), // VAR_TEMP_9
        new(10, "Temp A", "Misc"), // VAR_TEMP_A
        new(11, "Temp B", "Misc"), // VAR_TEMP_B
        new(12, "Temp C", "Misc"), // VAR_TEMP_C
        new(13, "Temp D", "Misc"), // VAR_TEMP_D
        new(14, "Temp E", "Misc"), // VAR_TEMP_E
        new(15, "Temp F", "Misc"), // VAR_TEMP_F
        new(16, "Obj Gfx Id 0", "Misc"), // VAR_OBJ_GFX_ID_0
        new(17, "Obj Gfx Id 1", "Misc"), // VAR_OBJ_GFX_ID_1
        new(18, "Obj Gfx Id 2", "Misc"), // VAR_OBJ_GFX_ID_2
        new(19, "Obj Gfx Id 3", "Misc"), // VAR_OBJ_GFX_ID_3
        new(20, "Obj Gfx Id 4", "Misc"), // VAR_OBJ_GFX_ID_4
        new(21, "Obj Gfx Id 5", "Misc"), // VAR_OBJ_GFX_ID_5
        new(22, "Obj Gfx Id 6", "Misc"), // VAR_OBJ_GFX_ID_6
        new(23, "Obj Gfx Id 7", "Misc"), // VAR_OBJ_GFX_ID_7
        new(24, "Obj Gfx Id 8", "Misc"), // VAR_OBJ_GFX_ID_8
        new(25, "Obj Gfx Id 9", "Misc"), // VAR_OBJ_GFX_ID_9
        new(26, "Obj Gfx Id A", "Misc"), // VAR_OBJ_GFX_ID_A
        new(27, "Obj Gfx Id B", "Misc"), // VAR_OBJ_GFX_ID_B
        new(28, "Obj Gfx Id C", "Misc"), // VAR_OBJ_GFX_ID_C
        new(29, "Obj Gfx Id D", "Misc"), // VAR_OBJ_GFX_ID_D
        new(30, "Obj Gfx Id E", "Misc"), // VAR_OBJ_GFX_ID_E
        new(31, "Obj Gfx Id F", "Misc"), // VAR_OBJ_GFX_ID_F
        new(32, "Recycle Goods", "Misc"), // VAR_RECYCLE_GOODS
        new(33, "Repel Step Count", "Misc"), // VAR_REPEL_STEP_COUNT
        new(34, "Ice Step Count", "Misc"), // VAR_ICE_STEP_COUNT
        new(35, "Starter Mon", "Misc"), // VAR_STARTER_MON
        new(36, "Mirage Rnd H", "Misc"), // VAR_MIRAGE_RND_H
        new(37, "Mirage Rnd L", "Misc"), // VAR_MIRAGE_RND_L
        new(38, "Secret Base Map", "Misc"), // VAR_SECRET_BASE_MAP
        new(39, "Cycling Road Record Collisions", "Misc"), // VAR_CYCLING_ROAD_RECORD_COLLISIONS
        new(40, "Cycling Road Record Time L", "Misc"), // VAR_CYCLING_ROAD_RECORD_TIME_L
        new(41, "Cycling Road Record Time H", "Misc"), // VAR_CYCLING_ROAD_RECORD_TIME_H
        new(42, "Friendship Step Counter", "Misc"), // VAR_FRIENDSHIP_STEP_COUNTER
        new(43, "Poison Step Counter", "Misc"), // VAR_POISON_STEP_COUNTER
        new(44, "Reset Rtc Enable", "Misc"), // VAR_RESET_RTC_ENABLE
        new(45, "Enigma Berry Available", "Misc"), // VAR_ENIGMA_BERRY_AVAILABLE
        new(46, "Wonder News Step Counter", "Misc"), // VAR_WONDER_NEWS_STEP_COUNTER
        new(47, "Frontier Maniac Facility", "Misc"), // VAR_FRONTIER_MANIAC_FACILITY
        new(48, "Frontier Gambler Challenge", "Misc"), // VAR_FRONTIER_GAMBLER_CHALLENGE
        new(49, "Frontier Gambler Set Challenge", "Misc"), // VAR_FRONTIER_GAMBLER_SET_CHALLENGE
        new(50, "Frontier Gambler Amount Bet", "Misc"), // VAR_FRONTIER_GAMBLER_AMOUNT_BET
        new(51, "Frontier Gambler State", "Misc"), // VAR_FRONTIER_GAMBLER_STATE
        new(52, "Deoxys Rock Step Count", "Misc"), // VAR_DEOXYS_ROCK_STEP_COUNT
        new(53, "Deoxys Rock Level", "Misc"), // VAR_DEOXYS_ROCK_LEVEL
        new(54, "Pc Box To Send Mon", "Misc"), // VAR_PC_BOX_TO_SEND_MON
        new(55, "Abnormal Weather Location", "Misc"), // VAR_ABNORMAL_WEATHER_LOCATION
        new(56, "Abnormal Weather Step Counter", "Misc"), // VAR_ABNORMAL_WEATHER_STEP_COUNTER
        new(57, "Should End Abnormal Weather", "Misc"), // VAR_SHOULD_END_ABNORMAL_WEATHER
        new(58, "Faraway Island Step Counter", "Misc"), // VAR_FARAWAY_ISLAND_STEP_COUNTER
        new(59, "Regice Steps 1", "Misc"), // VAR_REGICE_STEPS_1
        new(60, "Regice Steps 2", "Misc"), // VAR_REGICE_STEPS_2
        new(61, "Regice Steps 3", "Misc"), // VAR_REGICE_STEPS_3
        new(62, "Altering Cave Wild Set", "Misc"), // VAR_ALTERING_CAVE_WILD_SET
        new(63, "Distribute Eon Ticket", "Misc"), // VAR_DISTRIBUTE_EON_TICKET
        new(64, "Days", "Misc"), // VAR_DAYS
        new(65, "Fanclub Fan Counter", "Misc"), // VAR_FANCLUB_FAN_COUNTER
        new(66, "Fanclub Lose Fan Timer", "Misc"), // VAR_FANCLUB_LOSE_FAN_TIMER
        new(67, "Dept Store Floor", "Misc"), // VAR_DEPT_STORE_FLOOR
        new(68, "Trick House Level", "Misc"), // VAR_TRICK_HOUSE_LEVEL
        new(69, "Pokelot Prize Item", "Misc"), // VAR_POKELOT_PRIZE_ITEM
        new(70, "National Dex", "Misc"), // VAR_NATIONAL_DEX
        new(71, "Seedot Size Record", "Misc"), // VAR_SEEDOT_SIZE_RECORD
        new(72, "Ash Gather Count", "Misc"), // VAR_ASH_GATHER_COUNT
        new(73, "Birch State", "Misc"), // VAR_BIRCH_STATE
        new(74, "Cruise Step Count", "Misc"), // VAR_CRUISE_STEP_COUNT
        new(75, "Pokelot Rnd1", "Misc"), // VAR_POKELOT_RND1
        new(76, "Pokelot Rnd2", "Misc"), // VAR_POKELOT_RND2
        new(77, "Pokelot Prize Place", "Misc"), // VAR_POKELOT_PRIZE_PLACE
        new(78, "Repel Last Used", "Misc"), // VAR_REPEL_LAST_USED
        new(79, "Lotad Size Record", "Misc"), // VAR_LOTAD_SIZE_RECORD
        new(80, "Littleroot Town State", "Misc"), // VAR_LITTLEROOT_TOWN_STATE
        new(81, "Oldale Town State", "Misc"), // VAR_OLDALE_TOWN_STATE
        new(82, "Dewford Town State", "Misc"), // VAR_DEWFORD_TOWN_STATE
        new(83, "Lavaridge Town State", "Misc"), // VAR_LAVARIDGE_TOWN_STATE
        new(84, "Current Secret Base", "Misc"), // VAR_CURRENT_SECRET_BASE
        new(85, "Verdanturf Town State", "Misc"), // VAR_VERDANTURF_TOWN_STATE
        new(86, "Pacifidlog Town State", "Misc"), // VAR_PACIFIDLOG_TOWN_STATE
        new(87, "Petalburg City State", "Misc"), // VAR_PETALBURG_CITY_STATE
        new(88, "Slateport City State", "Misc"), // VAR_SLATEPORT_CITY_STATE
        new(89, "Mauville City State", "Misc"), // VAR_MAUVILLE_CITY_STATE
        new(90, "Rustboro City State", "Misc"), // VAR_RUSTBORO_CITY_STATE
        new(91, "Fortree City State", "Misc"), // VAR_FORTREE_CITY_STATE
        new(92, "Lilycove City State", "Misc"), // VAR_LILYCOVE_CITY_STATE
        new(93, "Mossdeep City State", "Misc"), // VAR_MOSSDEEP_CITY_STATE
        new(94, "Sootopolis City State", "Misc"), // VAR_SOOTOPOLIS_CITY_STATE
        new(95, "Ever Grande City State", "Misc"), // VAR_EVER_GRANDE_CITY_STATE
        new(96, "Route101 State", "Misc"), // VAR_ROUTE101_STATE
        new(97, "Route102 State", "Misc"), // VAR_ROUTE102_STATE
        new(98, "Route103 State", "Misc"), // VAR_ROUTE103_STATE
        new(99, "Route104 State", "Misc"), // VAR_ROUTE104_STATE
        new(100, "Route105 State", "Misc"), // VAR_ROUTE105_STATE
        new(101, "Route106 State", "Misc"), // VAR_ROUTE106_STATE
        new(102, "Shoal Cave Low Tide Inner Room Wild Set", "Misc"), // VAR_SHOAL_CAVE_LOW_TIDE_INNER_ROOM_WILD_SET
        new(103, "Artisan Cave B1F Wild Set", "Misc"), // VAR_ARTISAN_CAVE_B1F_WILD_SET
        new(104, "Sootopolis City Wild Set", "Misc"), // VAR_SOOTOPOLIS_CITY_WILD_SET
        new(105, "Route110 State", "Misc"), // VAR_ROUTE110_STATE
        new(106, "Shoal Cave Low Tide Ice Room Wild Set", "Misc"), // VAR_SHOAL_CAVE_LOW_TIDE_ICE_ROOM_WILD_SET
        new(107, "Petalburg City Wild Set", "Misc"), // VAR_PETALBURG_CITY_WILD_SET
        new(108, "Mossdeep City Wild Set", "Misc"), // VAR_MOSSDEEP_CITY_WILD_SET
        new(109, "Slateport City Wild Set", "Misc"), // VAR_SLATEPORT_CITY_WILD_SET
        new(110, "Meteor Falls 1F 1R Wild Set", "Misc"), // VAR_METEOR_FALLS_1F_1R_WILD_SET
        new(111, "Route116 State", "Misc"), // VAR_ROUTE116_STATE
        new(112, "Victory Road B2F Wild Set", "Misc"), // VAR_VICTORY_ROAD_B2F_WILD_SET
        new(113, "Route118 State", "Misc"), // VAR_ROUTE118_STATE
        new(114, "Route119 State", "Misc"), // VAR_ROUTE119_STATE
        new(115, "Victory Road B1F Wild Set", "Misc"), // VAR_VICTORY_ROAD_B1F_WILD_SET
        new(116, "Route121 State", "Misc"), // VAR_ROUTE121_STATE
        new(117, "Route133 Wild Set", "Misc"), // VAR_ROUTE133_WILD_SET
        new(118, "Route131 Wild Set", "Misc"), // VAR_ROUTE131_WILD_SET
        new(119, "Mt Pyre Exterior Wild Set", "Misc"), // VAR_MT_PYRE_EXTERIOR_WILD_SET
        new(120, "Mt Pyre 5F Wild Set", "Misc"), // VAR_MT_PYRE_5F_WILD_SET
        new(121, "Mt Pyre 4F Wild Set", "Misc"), // VAR_MT_PYRE_4F_WILD_SET
        new(122, "Mt Pyre 3F Wild Set", "Misc"), // VAR_MT_PYRE_3F_WILD_SET
        new(123, "Route128 State", "Misc"), // VAR_ROUTE128_STATE
        new(124, "Mt Pyre 2F Wild Set", "Misc"), // VAR_MT_PYRE_2F_WILD_SET
        new(125, "Route130 State", "Misc"), // VAR_ROUTE130_STATE
        new(126, "Route123 Wild Set", "Misc"), // VAR_ROUTE123_WILD_SET
        new(127, "Route122 Wild Set", "Misc"), // VAR_ROUTE122_WILD_SET
        new(128, "Route121 Wild Set", "Misc"), // VAR_ROUTE121_WILD_SET
        new(129, "Route120 Wild Set", "Misc"), // VAR_ROUTE120_WILD_SET
        new(130, "Littleroot Houses State May", "Misc"), // VAR_LITTLEROOT_HOUSES_STATE_MAY
        new(131, "Route119 Wild Set", "Misc"), // VAR_ROUTE119_WILD_SET
        new(132, "Birch Lab State", "Misc"), // VAR_BIRCH_LAB_STATE
        new(133, "Petalburg Gym State", "Misc"), // VAR_PETALBURG_GYM_STATE
        new(134, "Contest Hall State", "Misc"), // VAR_CONTEST_HALL_STATE
        new(135, "Cable Club State", "Misc"), // VAR_CABLE_CLUB_STATE
        new(136, "Contest Type", "Misc"), // VAR_CONTEST_TYPE
        new(137, "Secret Base Initialized", "Misc"), // VAR_SECRET_BASE_INITIALIZED
        new(138, "Contest Prize Pickup", "Misc"), // VAR_CONTEST_PRIZE_PICKUP
        new(139, "New Mauville Inside Wild Set", "Misc"), // VAR_NEW_MAUVILLE_INSIDE_WILD_SET
        new(140, "Littleroot Houses State Brendan", "Misc"), // VAR_LITTLEROOT_HOUSES_STATE_BRENDAN
        new(141, "Littleroot Rival State", "Misc"), // VAR_LITTLEROOT_RIVAL_STATE
        new(142, "Board Briney Boat State", "Misc"), // VAR_BOARD_BRINEY_BOAT_STATE
        new(143, "Devon Corp 3F State", "Misc"), // VAR_DEVON_CORP_3F_STATE
        new(144, "Briney House State", "Misc"), // VAR_BRINEY_HOUSE_STATE
        new(145, "Route115 Wild Set", "Misc"), // VAR_ROUTE115_WILD_SET
        new(146, "Littleroot Intro State", "Misc"), // VAR_LITTLEROOT_INTRO_STATE
        new(147, "Mauville Gym State", "Misc"), // VAR_MAUVILLE_GYM_STATE
        new(148, "Lilycove Museum 2F State", "Misc"), // VAR_LILYCOVE_MUSEUM_2F_STATE
        new(149, "Lilycove Fan Club State", "Misc"), // VAR_LILYCOVE_FAN_CLUB_STATE
        new(150, "Briney Location", "Misc"), // VAR_BRINEY_LOCATION
        new(151, "Init Secret Base", "Misc"), // VAR_INIT_SECRET_BASE
        new(152, "Petalburg Woods State", "Misc"), // VAR_PETALBURG_WOODS_STATE
        new(153, "Lilycove Contest Lobby State", "Misc"), // VAR_LILYCOVE_CONTEST_LOBBY_STATE
        new(154, "Rusturf Tunnel State", "Misc"), // VAR_RUSTURF_TUNNEL_STATE
        new(155, "Route107 Wild Set", "Misc"), // VAR_ROUTE107_WILD_SET
        new(156, "Elite 4 State", "Misc"), // VAR_ELITE_4_STATE
        new(157, "Jagged Pass Wild Set", "Misc"), // VAR_JAGGED_PASS_WILD_SET
        new(158, "Mossdeep Space Center Stair Guard State", "Misc"), // VAR_MOSSDEEP_SPACE_CENTER_STAIR_GUARD_STATE
        new(159, "Mossdeep Space Center State", "Misc"), // VAR_MOSSDEEP_SPACE_CENTER_STATE
        new(160, "Slateport Harbor State", "Misc"), // VAR_SLATEPORT_HARBOR_STATE
        new(161, "Fiery Path Wild Set", "Misc"), // VAR_FIERY_PATH_WILD_SET
        new(162, "Seafloor Cavern State", "Misc"), // VAR_SEAFLOOR_CAVERN_STATE
        new(163, "Cable Car Station State", "Misc"), // VAR_CABLE_CAR_STATION_STATE
        new(164, "Safari Zone State", "Misc"), // VAR_SAFARI_ZONE_STATE
        new(165, "Trick House Being Watched State", "Misc"), // VAR_TRICK_HOUSE_BEING_WATCHED_STATE
        new(166, "Trick House Found Trick Master", "Misc"), // VAR_TRICK_HOUSE_FOUND_TRICK_MASTER
        new(167, "Trick House Entrance State", "Misc"), // VAR_TRICK_HOUSE_ENTRANCE_STATE
        new(168, "Victory Road 1F Wild Set", "Misc"), // VAR_VICTORY_ROAD_1F_WILD_SET
        new(169, "Cycling Challenge State", "Misc"), // VAR_CYCLING_CHALLENGE_STATE
        new(170, "Slateport Museum 1F State", "Misc"), // VAR_SLATEPORT_MUSEUM_1F_STATE
        new(171, "Trick House Puzzle 1 State", "Misc"), // VAR_TRICK_HOUSE_PUZZLE_1_STATE
        new(172, "Trick House Puzzle 2 State", "Misc"), // VAR_TRICK_HOUSE_PUZZLE_2_STATE
        new(173, "Trick House Puzzle 3 State", "Misc"), // VAR_TRICK_HOUSE_PUZZLE_3_STATE
        new(174, "Trick House Puzzle 4 State", "Misc"), // VAR_TRICK_HOUSE_PUZZLE_4_STATE
        new(175, "Trick House Puzzle 5 State", "Misc"), // VAR_TRICK_HOUSE_PUZZLE_5_STATE
        new(176, "Trick House Puzzle 6 State", "Misc"), // VAR_TRICK_HOUSE_PUZZLE_6_STATE
        new(177, "Trick House Puzzle 7 State", "Misc"), // VAR_TRICK_HOUSE_PUZZLE_7_STATE
        new(178, "Trick House Puzzle 8 State", "Misc"), // VAR_TRICK_HOUSE_PUZZLE_8_STATE
        new(179, "Weather Institute State", "Misc"), // VAR_WEATHER_INSTITUTE_STATE
        new(180, "Ss Tidal State", "Misc"), // VAR_SS_TIDAL_STATE
        new(181, "Trick House Enter From Corridor", "Misc"), // VAR_TRICK_HOUSE_ENTER_FROM_CORRIDOR
        new(182, "Trick House Puzzle 7 State 2", "Misc"), // VAR_TRICK_HOUSE_PUZZLE_7_STATE_2
        new(183, "Slateport Fan Club State", "Misc"), // VAR_SLATEPORT_FAN_CLUB_STATE
        new(184, "Granite Cave B1F Wild Set", "Misc"), // VAR_GRANITE_CAVE_B1F_WILD_SET
        new(185, "Mt Pyre State", "Misc"), // VAR_MT_PYRE_STATE
        new(186, "New Mauville State", "Misc"), // VAR_NEW_MAUVILLE_STATE
        new(187, "Granite Cave 1F Wild Set", "Misc"), // VAR_GRANITE_CAVE_1F_WILD_SET
        new(188, "Bravo Trainer Battle Tower On", "Misc"), // VAR_BRAVO_TRAINER_BATTLE_TOWER_ON
        new(189, "Jagged Pass Ash Weather", "Misc"), // VAR_JAGGED_PASS_ASH_WEATHER
        new(190, "Glass Workshop State", "Misc"), // VAR_GLASS_WORKSHOP_STATE
        new(191, "Meteor Falls State", "Misc"), // VAR_METEOR_FALLS_STATE
        new(192, "Sootopolis Mystery Events State", "Misc"), // VAR_SOOTOPOLIS_MYSTERY_EVENTS_STATE
        new(193, "Trick House Prize Pickup", "Misc"), // VAR_TRICK_HOUSE_PRIZE_PICKUP
        new(194, "Pacifidlog Tm Received Day", "Misc"), // VAR_PACIFIDLOG_TM_RECEIVED_DAY
        new(195, "Victory Road 1F State", "Misc"), // VAR_VICTORY_ROAD_1F_STATE
        new(196, "Fossil Resurrection State", "Misc"), // VAR_FOSSIL_RESURRECTION_STATE
        new(197, "Which Fossil Revived", "Misc"), // VAR_WHICH_FOSSIL_REVIVED
        new(198, "Stevens House State", "Misc"), // VAR_STEVENS_HOUSE_STATE
        new(199, "Oldale Rival State", "Misc"), // VAR_OLDALE_RIVAL_STATE
        new(200, "Jagged Pass State", "Misc"), // VAR_JAGGED_PASS_STATE
        new(201, "Scott Petalburg Encounter", "Misc"), // VAR_SCOTT_PETALBURG_ENCOUNTER
        new(202, "Sky Pillar State", "Misc"), // VAR_SKY_PILLAR_STATE
        new(203, "Mirage Tower State", "Misc"), // VAR_MIRAGE_TOWER_STATE
        new(204, "Fossil Maniac State", "Misc"), // VAR_FOSSIL_MANIAC_STATE
        new(205, "Cable Club Tutorial State", "Misc"), // VAR_CABLE_CLUB_TUTORIAL_STATE
        new(206, "Frontier Battle Mode", "Misc"), // VAR_FRONTIER_BATTLE_MODE
        new(207, "Frontier Facility", "Misc"), // VAR_FRONTIER_FACILITY
        new(208, "Has Entered Battle Frontier", "Misc"), // VAR_HAS_ENTERED_BATTLE_FRONTIER
        new(209, "Scott State", "Misc"), // VAR_SCOTT_STATE
        new(210, "Slateport Outside Museum State", "Misc"), // VAR_SLATEPORT_OUTSIDE_MUSEUM_STATE
        new(211, "Dex Upgrade Johto Starter State", "Misc"), // VAR_DEX_UPGRADE_JOHTO_STARTER_STATE
        new(212, "Ss Tidal Scott State", "Misc"), // VAR_SS_TIDAL_SCOTT_STATE
        new(213, "Roamer Pokemon", "Misc"), // VAR_ROAMER_POKEMON
        new(214, "Trainer Hill Is Active", "Misc"), // VAR_TRAINER_HILL_IS_ACTIVE
        new(215, "Sky Pillar Rayquaza Cry Done", "Misc"), // VAR_SKY_PILLAR_RAYQUAZA_CRY_DONE
        new(216, "Sootopolis Wallace State", "Misc"), // VAR_SOOTOPOLIS_WALLACE_STATE
        new(217, "Has Talked To Seafloor Cavern Entrance Grunt", "Misc"), // VAR_HAS_TALKED_TO_SEAFLOOR_CAVERN_ENTRANCE_GRUNT
        new(218, "Register Birch State", "Misc"), // VAR_REGISTER_BIRCH_STATE
        new(219, "Petalburg Woods Wild Set", "Misc"), // VAR_PETALBURG_WOODS_WILD_SET
        new(220, "Rusturf Tunnel Wild Set", "Misc"), // VAR_RUSTURF_TUNNEL_WILD_SET
        new(221, "Gift Pichu Slot", "Misc"), // VAR_GIFT_PICHU_SLOT
        new(222, "Gift Unused 1", "Misc"), // VAR_GIFT_UNUSED_1
        new(223, "Gift Unused 2", "Misc"), // VAR_GIFT_UNUSED_2
        new(224, "Gift Unused 3", "Misc"), // VAR_GIFT_UNUSED_3
        new(225, "Gift Unused 4", "Misc"), // VAR_GIFT_UNUSED_4
        new(226, "Gift Unused 5", "Misc"), // VAR_GIFT_UNUSED_5
        new(227, "Gift Unused 6", "Misc"), // VAR_GIFT_UNUSED_6
        new(228, "Gift Unused 7", "Misc"), // VAR_GIFT_UNUSED_7
        new(229, "Route117 Wild Set", "Misc"), // VAR_ROUTE117_WILD_SET
        new(230, "Daily Slots", "Misc"), // VAR_DAILY_SLOTS
        new(231, "Daily Wilds", "Misc"), // VAR_DAILY_WILDS
        new(232, "Daily Blender", "Misc"), // VAR_DAILY_BLENDER
        new(233, "Daily Planted Berries", "Misc"), // VAR_DAILY_PLANTED_BERRIES
        new(234, "Daily Picked Berries", "Misc"), // VAR_DAILY_PICKED_BERRIES
        new(235, "Daily Roulette", "Misc"), // VAR_DAILY_ROULETTE
        new(236, "Secret Base Step Counter", "Misc"), // VAR_SECRET_BASE_STEP_COUNTER
        new(237, "Secret Base Last Item Used", "Misc"), // VAR_SECRET_BASE_LAST_ITEM_USED
        new(238, "Secret Base Low Tv Flags", "Misc"), // VAR_SECRET_BASE_LOW_TV_FLAGS
        new(239, "Secret Base High Tv Flags", "Misc"), // VAR_SECRET_BASE_HIGH_TV_FLAGS
        new(240, "Secret Base Is Not Local", "Misc"), // VAR_SECRET_BASE_IS_NOT_LOCAL
        new(241, "Daily Bp", "Misc"), // VAR_DAILY_BP
        new(242, "Wally Call Step Counter", "Misc"), // VAR_WALLY_CALL_STEP_COUNTER
        new(243, "Scott Fortree Call Step Counter", "Misc"), // VAR_SCOTT_FORTREE_CALL_STEP_COUNTER
        new(244, "Roxanne Call Step Counter", "Misc"), // VAR_ROXANNE_CALL_STEP_COUNTER
        new(245, "Scott Bf Call Step Counter", "Misc"), // VAR_SCOTT_BF_CALL_STEP_COUNTER
        new(246, "Rival Rayquaza Call Step Counter", "Misc"), // VAR_RIVAL_RAYQUAZA_CALL_STEP_COUNTER
        new(247, "Route101 Wild Set", "Misc"), // VAR_ROUTE101_WILD_SET
        new(248, "Route102 Wild Set", "Misc"), // VAR_ROUTE102_WILD_SET
        new(249, "Route103 Wild Set", "Misc"), // VAR_ROUTE103_WILD_SET
        new(250, "Route104 Wild Set", "Misc"), // VAR_ROUTE104_WILD_SET
        new(251, "Route110 Wild Set", "Misc"), // VAR_ROUTE110_WILD_SET
        new(252, "Route111 Wild Set", "Misc"), // VAR_ROUTE111_WILD_SET
        new(253, "Route112 Wild Set", "Misc"), // VAR_ROUTE112_WILD_SET
        new(254, "Route114 Wild Set", "Misc"), // VAR_ROUTE114_WILD_SET
        new(255, "Route116 Wild Set", "Misc"), // VAR_ROUTE116_WILD_SET
        new(256, "Norman Rematch Call Step Counter", "Misc"), // VAR_NORMAN_REMATCH_CALL_STEP_COUNTER
    ];
}
