<?php
// Show Errors
ini_set('display_errors', '1');
ini_set('display_startup_errors', '1');
error_reporting(E_ALL & ~E_NOTICE);

// Don't cache
header("Cache-Control: no-store, no-cache, must-revalidate, max-age=0");
header("Cache-Control: post-check=0, pre-check=0", false);
header("Pragma: no-cache");

// Increase execution time
set_time_limit(300);

// update and maintain old values import
$dropMongo = false;
if (defined('MongoClean')) {
    $dropMongo = true;
}
$delJson = true;
if (defined('delJson')) {
    $delJson = true;
}
// Configuration
$dbhost = 'localhost';
// $dbname = 'torc_db';
$dbname = 'torc_db_test';

// Connect to test database
include_once dirname(__FILE__) . "/../../../vendor/autoload.php";
$m = new \MongoDB\Client("mongodb://$dbhost");
$db = $m->$dbname;

$genericSorts = [
    [
        ['$**' => "text"],
        ['name' => "TextIndex"]
    ],
    //["LocalizedName" => 1],
    ["LocalizedName.enMale" => 1],
    ["LocalizedName.frMale" => 1],
    ["LocalizedName.deMale" => 1],
];
$genericSorts2 = [
    [
        [
            "LocalizedName.enMale" => "text",
            "LocalizedName.frMale" => "text",
            "LocalizedName.frFemale" => "text",
            "LocalizedName.deMale" => "text",
            "LocalizedName.deFemale" => "text",
        ],
        ["name" => "textIndex"]
    ],
    ["LocalizedName" => 1],
    ["LocalizedName.enMale" => 1],
    ["LocalizedName.frMale" => 1],
    ["LocalizedName.deMale" => 1]
];

$genericSorts3 = [
    [
        [
            "LocalizedName.enMale" => "text",
            "LocalizedName.frMale" => "text",
            "LocalizedName.frFemale" => "text",
            "LocalizedName.deMale" => "text",
            "LocalizedName.deFemale" => "text",
            "LocalizedDescription.enMale" => "text",
            "LocalizedDescription.frMale" => "text",
            "LocalizedDescription.frFemale" => "text",
            "LocalizedDescription.deMale" => "text",
            "LocalizedDescription.deFemale" => "text",
        ],
        ["name" => "textIndex"]
    ],
    ["LocalizedName" => 1],
    ["LocalizedName.enMale" => 1],
    ["LocalizedName.frMale" => 1],
    ["LocalizedName.deMale" => 1],
];

$version = [
    ["current_version" => 1],
    ["previous_versions" => 1],
    ["last_seen" => 1],
    ["removed_in" => 1],
    ["hash" => 1],
];

$data = [
    #Ability
    [
        'ability', 'Abilities',
        array_merge($genericSorts, [
            ['Level' => 1],
            [
                ['MinRange' => 1, 'MaxRange' => 1], []
            ],
            ['Cooldown' => 1],
            ['ChannelingTime' => 1],
            ['CastingTime' => 1],
            ['IsPassive' => 1],
            ['Pushback' => 1],
            ['LocalizedCategoryName' => 1],
            ["LocalizedCategoryName.enMale" => 1],
            ["LocalizedCategoryName.frMale" => 1],
            ["LocalizedCategoryName.deMale" => 1],
            [
                ["Cooldown" => 1, "LocalizedName.enMale" => 1], []
            ],
            [
                ["Cooldown" => 1, "LocalizedName.frMale" => 1], []
            ],
            [
                ["Cooldown" => 1, "LocalizedName.deMale" => 1], []
            ],
            [
                ["Cooldown" => -1, "LocalizedName.enMale" => 1], []
            ],
            [
                ["Cooldown" => -1, "LocalizedName.frMale" => 1], []
            ],
            [
                ["Cooldown" => -1, "LocalizedName.deMale" => 1], []
            ],
            [
                ["MaxRange" => 1, "LocalizedName.enMale" => 1], []
            ],
            [
                ["MaxRange" => 1, "LocalizedName.frMale" => 1], []
            ],
            [
                ["MaxRange" => 1, "LocalizedName.deMale" => 1], []
            ],
            [
                ["MaxRange" => -1, "LocalizedName.enMale" => 1], []
            ],
            [
                ["MaxRange" => -1, "LocalizedName.frMale" => 1], []
            ],
            [
                ["MaxRange" => -1, "LocalizedName.deMale" => 1], []
            ],
        ])
    ],
    ['abilitypackage', 'AbilityPackages'],
    #Achievement
    [
        'achievement', 'Achievements', [
            [
                [
                    "LocalizedName.enMale" => "text",
                    "LocalizedName.frMale" => "text",
                    "LocalizedName.frFemale" => "text",
                    "LocalizedName.deMale" => "text",
                    "LocalizedName.deFemale" => "text",
                    "LocalizedDescription.enMale" => "text",
                    "LocalizedDescription.frMale" => "text",
                    "LocalizedDescription.frFemale" => "text",
                    "LocalizedDescription.deMale" => "text",
                    "LocalizedDescription.deFemale" => "text",
                    "LocalizedNonSpoilerDesc.enMale" => "text",
                    "LocalizedNonSpoilerDesc.frMale" => "text",
                    "LocalizedNonSpoilerDesc.frFemale" => "text",
                    "LocalizedNonSpoilerDesc.deMale" => "text",
                    "LocalizedNonSpoilerDesc.deFemale" => "text",
                    "Fqn" => "text",
                ], ["name" => "textIndex"]
            ],
            ["LocalizedName" => 1],
            ["LocalizedName.enMale" => 1],
            ["LocalizedName.frMale" => 1],
            ["LocalizedName.deMale" => 1],
            ['Visibility' => 1],
            ['ItemReward' => 1],
            ['MtxReward' => 1],
            ['TitleReward' => 1],
            ['GsfReward' => 1],
            ['CategoryData.Category.Name' => 1],
            ['CategoryData.SubCategory.Name' => 1],
            ['CategoryData.TertiaryCategory.Name' => 1],
        ]
    ],
    #Advanced Class
    ['advclass', 'AdvancedClasses'],
    #Areas
    ['area', 'Areas'],
    #Classes
    [
        'classspec', 'Classes',
        array_merge($genericSorts, [
            ["IsPlayerClass" => 1],
            ["IsPlayerAdvancedClass" => 1],
            ["NpcsWithThisClass" => 1],
        ])
    ],
    #Codex
    [
        'codex', 'CodexEntries',
        array_merge($genericSorts, [
            ['LocalizedCategoryName' => 1],
            ["LocalizedCategoryName.enMale" => 1],
            ["LocalizedCategoryName.frMale" => 1],
            ["LocalizedCategoryName.deMale" => 1],
            ['Faction' => 1],
            ['IsPlanet' => 1],
            ['IsHidden' => 1],
            ['Level' => 1],
            ['ClassRestricted' => 1],
        ])
    ],
    #Collections
    [
        'collection', 'Collections',
        array_merge($genericSorts2, [])
    ],
    ['companion', 'Companions'],
    #Conquests
    ['conquest', 'Conquests'],
    #Conversations
    [
        'conversation', 'Conversations',
        array_merge($genericSorts2, [])
    ],
    #Decorations
    ['decoration', 'Decorations'],
    #Galactic Starfighter
    ['gsf', 'Ships'],
    #Item
    [
        'item', 'Items',
        array_merge($genericSorts3, [
            ['Category' => 1],
            ['SubCategory' => 1],
            ["Quality" => 1],
            ["SimpleCombinedStatModifiers.Endurance" => 1],
            ["SimpleCombinedStatModifiers.Mastery" => 1],
            ["SimpleCombinedStatModifiers.Presence" => 1],
            ["SimpleCombinedStatModifiers.Absorption Rating" => 1],
            ["SimpleCombinedStatModifiers.Defense Rating" => 1],
            ["SimpleCombinedStatModifiers.Power" => 1],
            ["SimpleCombinedStatModifiers.Accuracy Rating" => 1],
            ["SimpleCombinedStatModifiers.Alacrity Rating" => 1],
            ["SimpleCombinedStatModifiers.Shield Rating" => 1],
            ["SimpleCombinedStatModifiers.Critical Rating" => 1],
            ["SimpleCombinedStatModifiers.Expertise Rating" => 1],
            ["GiftRankNum" => 1],
            ["RequiredSocialTier" => 1],
            ["RequiredValorRank" => 1],
            ["RequiredGender" => 1],
            ["TypeBitFlags.IsModdable" => 1],
            ["TypeBitFlags.IsCrafted" => 1],
            ["TypeBitFlags.IsEquipable" => 1],
            ["TypeBitFlags.IsRepTrophy" => 1],
            ["TypeBitFlags.IsMtxItem" => 1],
            ["BindsToSlot" => 1],
            ["TypeBitFlags.Unk8" => 1],
            ["TypeBitFlags.Unk800" => 1],
            ['CombinedRequiredLevel' => 1],
            ['CombinedRating' => 1],
            [["CombinedRequiredLevel" => 1, "LocalizedName.enMale" => 1], []],
            [["CombinedRequiredLevel" => 1, "LocalizedName.frMale" => 1], []],
            [["CombinedRequiredLevel" => 1, "LocalizedName.deMale" => 1], []],
            [["CombinedRequiredLevel" => -1, "LocalizedName.enMale" => 1], []],
            [["CombinedRequiredLevel" => -1, "LocalizedName.frMale" => 1], []],
            [["CombinedRequiredLevel" => -1, "LocalizedName.deMale" => 1], []],
            [["AuctionCategory.LocalizedName.enMale" => 1, "LocalizedName.enMale" => 1], []],
            [["AuctionCategory.LocalizedName.frMale" => 1, "LocalizedName.frMale" => 1], []],
            [["AuctionCategory.LocalizedName.deMale" => 1, "LocalizedName.deMale" => 1], []],
            [["AuctionCategory.LocalizedName.enMale" => -1, "LocalizedName.enMale" => 1], []],
            [["AuctionCategory.LocalizedName.frMale" => -1, "LocalizedName.frMale" => 1], []],
            [["AuctionCategory.LocalizedName.deMale" => -1, "LocalizedName.deMale" => 1], []],
            [["AuctionSubCategory.LocalizedName.enMale" => 1, "LocalizedName.enMale" => 1], []],
            [["AuctionSubCategory.LocalizedName.frMale" => 1, "LocalizedName.frMale" => 1], []],
            [["AuctionSubCategory.LocalizedName.deMale" => 1, "LocalizedName.deMale" => 1], []],
            [["AuctionSubCategory.LocalizedName.enMale" => -1, "LocalizedName.enMale" => 1], []],
            [["AuctionSubCategory.LocalizedName.frMale" => -1, "LocalizedName.frMale" => 1], []],
            [["AuctionSubCategory.LocalizedName.deMale" => -1, "LocalizedName.deMale" => 1], []],
            ['EnhancementSlots.ModificationBase62Id' => 1],
            ['WeaponAppSpec' => 1],
            ['SoundType' => 1],
            ['SchematicB62Id' => 1],
            ['ReqArtEquipAuth' => 1]
        ])
    ],
    #Map Notes
    ['mapnotes', 'MapNotes',
        //array_merge($genericSorts, [
        //])
    ],
    #Missions
    [
        'mission', 'Quests',
        array_merge($genericSorts2, [
            ['LocalizedCategory' => 1],
            ["LocalizedCategory.enMale" => 1],
            ["LocalizedCategory.frMale" => 1],
            ["LocalizedCategory.deMale" => 1],
            [["LocalizedCategory.enMale" => 1, "LocalizedName.enMale" => 1], []],
            [["LocalizedCategory.frMale" => 1, "LocalizedName.frMale" => 1], []],
            [["LocalizedCategory.deMale" => 1, "LocalizedName.deMale" => 1], []],
            [["LocalizedCategory.enMale" => -1, "LocalizedName.enMale" => 1], []],
            [["LocalizedCategory.frMale" => -1, "LocalizedName.frMale" => 1], []],
            [["LocalizedCategory.deMale" => -1, "LocalizedName.deMale" => 1], []],
            [["IsRepeatable" => 1, "LocalizedName.enMale" => 1], []],
            [["IsRepeatable" => 1, "LocalizedName.frMale" => 1], []],
            [["IsRepeatable" => 1, "LocalizedName.deMale" => 1], []],
            [["IsRepeatable" => -1, "LocalizedName.enMale" => 1], []],
            [["IsRepeatable" => -1, "LocalizedName.frMale" => 1], []],
            [["IsRepeatable" => -1, "LocalizedName.deMale" => 1], []],
            [["RequiredLevel" => 1, "LocalizedName.enMale" => 1], []],
            [["RequiredLevel" => 1, "LocalizedName.frMale" => 1], []],
            [["RequiredLevel" => 1, "LocalizedName.deMale" => 1], []],
            [["RequiredLevel" => -1, "LocalizedName.enMale" => 1], []],
            [["RequiredLevel" => -1, "LocalizedName.frMale" => 1], []],
            [["RequiredLevel" => -1, "LocalizedName.deMale" => 1], []],
            [["XpLevel" => 1, "LocalizedName.enMale" => 1], []],
            [["XpLevel" => 1, "LocalizedName.frMale" => 1], []],
            [["XpLevel" => 1, "LocalizedName.deMale" => 1], []],
            [["XpLevel" => -1, "LocalizedName.enMale" => 1], []],
            [["XpLevel" => -1, "LocalizedName.frMale" => 1], []],
            [["XpLevel" => -1, "LocalizedName.deMale" => 1], []],
            ['RequiredLevel' => 1],
            ['XpLevel' => 1],
            ['IsRepeatable' => 1],
        ])
    ],
    # MTX Store Fronts
    [
        'mtx', 'MtxStoreFronts'
    ],
    # New Companions
    [
        'newcompanion', 'NewCompanions',
        array_merge($genericSorts2, [])
    ],
    # NPCs
    [
        'npc', 'Npcs',
        array_merge($genericSorts2, [
            ["FqnCategory" => 1],
            ["FqnSubCategory" => 1],
            ["DetFaction.LocalizedName.enMale" => 1],
            ["DetFaction.LocalizedName.frMale" => 1],
            ["DetFaction.LocalizedName.deMale" => 1],
            ["LocalizedToughness.enMale" => 1],
            ["LocalizedToughness.frMale" => 1],
            ["LocalizedToughness.deMale" => 1],
            ["Toughness" => 1],
            ["MinLevel" => 1],
            ["MaxLevel" => 1],
            ["IsVendor" => 1],
            [["MinLevel" => 1, "LocalizedName.enMale" => 1], []],
            [["MinLevel" => 1, "LocalizedName.frMale" => 1], []],
            [["MinLevel" => 1, "LocalizedName.deMale" => 1], []],
            [["MinLevel" => -1, "LocalizedName.enMale" => 1], []],
            [["MinLevel" => -1, "LocalizedName.frMale" => 1], []],
            [["MinLevel" => -1, "LocalizedName.deMale" => 1], []],
            [["MaxLevel" => 1, "LocalizedName.enMale" => 1], []],
            [["MaxLevel" => 1, "LocalizedName.frMale" => 1], []],
            [["MaxLevel" => 1, "LocalizedName.deMale" => 1], []],
            [["MaxLevel" => -1, "LocalizedName.enMale" => 1], []],
            [["MaxLevel" => -1, "LocalizedName.frMale" => 1], []],
            [["MaxLevel" => -1, "LocalizedName.deMale" => 1], []],
            [["DetFaction.LocalizedName.enMale" => 1, "LocalizedName.enMale" => 1], []],
            [["DetFaction.LocalizedName.frMale" => 1, "LocalizedName.frMale" => 1], []],
            [["DetFaction.LocalizedName.deMale" => 1, "LocalizedName.deMale" => 1], []],
            [["DetFaction.LocalizedName.enMale" => -1, "LocalizedName.enMale" => 1], []],
            [["DetFaction.LocalizedName.frMale" => -1, "LocalizedName.frMale" => 1], []],
            [["DetFaction.LocalizedName.deMale" => -1, "LocalizedName.deMale" => 1], []],
            [["LocalizedToughness.enMale" => 1, "LocalizedName.enMale" => 1], []],
            [["LocalizedToughness.frMale" => 1, "LocalizedName.frMale" => 1], []],
            [["LocalizedToughness.deMale" => 1, "LocalizedName.deMale" => 1], []],
            [["LocalizedToughness.enMale" => -1, "LocalizedName.enMale" => 1], []],
            [["LocalizedToughness.frMale" => -1, "LocalizedName.frMale" => 1], []],
            [["LocalizedToughness.deMale" => -1, "LocalizedName.deMale" => 1], []],
            [["FqnCategory" => 1, "LocalizedName.enMale" => 1], []],
            [["FqnCategory" => 1, "LocalizedName.frMale" => 1], []],
            [["FqnCategory" => 1, "LocalizedName.deMale" => 1], []],
            [["FqnCategory" => -1, "LocalizedName.enMale" => 1], []],
            [["FqnCategory" => -1, "LocalizedName.frMale" => 1], []],
            [["FqnCategory" => -1, "LocalizedName.deMale" => 1], []],
            [["FqnSubCategory" => 1, "LocalizedName.enMale" => 1], []],
            [["FqnSubCategory" => 1, "LocalizedName.frMale" => 1], []],
            [["FqnSubCategory" => 1, "LocalizedName.deMale" => 1], []],
            [["FqnSubCategory" => -1, "LocalizedName.enMale" => 1], []],
            [["FqnSubCategory" => -1, "LocalizedName.frMale" => 1], []],
            [["FqnSubCategory" => -1, "LocalizedName.deMale" => 1], []]
        ])
    ],
    # Placeables
    [
        'object', 'Placeables',
        //array_merge($genericSorts, [
        //])
    ],
    # Rooms
    [
        'rooms', 'Rooms',
        //array_merge($genericSorts, [
        //])
    ],
    # Schematics
    [
        'schematic', 'Schematics',
        array_merge($genericSorts2, [
            ['SkillOrange' => 1],
            ['LocalizedCategoryName' => 1],
            ["LocalizedCategoryName.enMale" => 1],
            ["LocalizedCategoryName.frMale" => 1],
            ["LocalizedCategoryName.deMale" => 1],
            ['LocalizedCrewSkillName' => 1],
            ["LocalizedCrewSkillName.enMale" => 1],
            ["LocalizedCrewSkillName.frMale" => 1],
            ["LocalizedCrewSkillName.deMale" => 1],
            ['LocalizedSubTypeName' => 1],
            ["LocalizedSubTypeName.enMale" => 1],
            ["LocalizedSubTypeName.frMale" => 1],
            ["LocalizedSubTypeName.deMale" => 1],
            [["LocalizedSubTypeName.enMale" => 1, "LocalizedName.enMale" => 1], []],
            [["LocalizedSubTypeName.frMale" => 1, "LocalizedName.frMale" => 1], []],
            [["LocalizedSubTypeName.deMale" => 1, "LocalizedName.deMale" => 1], []],
            [["LocalizedSubTypeName.enMale" => -1, "LocalizedName.enMale" => 1], []],
            [["LocalizedSubTypeName.frMale" => -1, "LocalizedName.frMale" => 1], []],
            [["LocalizedSubTypeName.deMale" => -1, "LocalizedName.deMale" => 1], []],
            [["LocalizedCrewSkillName.enMale" => 1, "LocalizedName.enMale" => 1], []],
            [["LocalizedCrewSkillName.frMale" => 1, "LocalizedName.frMale" => 1], []],
            [["LocalizedCrewSkillName.deMale" => 1, "LocalizedName.deMale" => 1], []],
            [["LocalizedCrewSkillName.enMale" => -1, "LocalizedName.enMale" => 1], []],
            [["LocalizedCrewSkillName.frMale" => -1, "LocalizedName.frMale" => 1], []],
            [["LocalizedCrewSkillName.deMale" => -1, "LocalizedName.deMale" => 1], []],
            [["LocalizedCategory.enMale" => 1, "LocalizedName.enMale" => 1], []],
            [["LocalizedCategory.frMale" => 1, "LocalizedName.frMale" => 1], []],
            [["LocalizedCategory.deMale" => 1, "LocalizedName.deMale" => 1], []],
            [["LocalizedCategory.enMale" => -1, "LocalizedName.enMale" => 1], []],
            [["LocalizedCategory.frMale" => -1, "LocalizedName.frMale" => 1], []],
            [["LocalizedCategory.deMale" => -1, "LocalizedName.deMale" => 1], []],
            [["SkillOrange" => 1, "LocalizedName.enMale" => 1], []],
            [["SkillOrange" => 1, "LocalizedName.frMale" => 1], []],
            [["SkillOrange" => 1, "LocalizedName.deMale" => 1], []],
            [["SkillOrange" => -1, "LocalizedName.enMale" => 1], []],
            [["SkillOrange" => -1, "LocalizedName.frMale" => 1], []],
            [["SkillOrange" => -1, "LocalizedName.deMale" => 1], []]
        ])
    ],
    ['setbonus', 'SetBonuses'],
    ['stronghold', 'Strongholds'],
    ['talent', 'Talents']
];

$skipped = array();
$time_start = microtime(true);

function compare_multi_arrays($array1, $array2)
{
    $result = array("more" => array(), "less" => array(), "diff" => array());
    foreach ($array1 as $k => $v) {
        if (is_array($v) && isset($array2[$k]) && is_array($array2[$k])) {
            $sub_result = compare_multi_arrays($v, $array2[$k]);
            //merge results
            foreach (array_keys($sub_result) as $key) {
                if (!empty($sub_result[$key])) {
                    $result[$key] = array_merge_recursive($result[$key], array($k => $sub_result[$key]));
                }
            }
        } else {
            if (isset($array2[$k])) {
                if ($v !== $array2[$k]) {
                    $result["diff"][$k] = array("from" => $v, "to" => $array2[$k]);
                }
            } else {
                $result["more"][$k] = $v;
            }
        }
    }
    foreach ($array2 as $k => $v) {
        if (!isset($array1[$k])) {
            $result["less"][$k] = $v;
        }
    }
    return $result;
}

foreach ($data as $row) {
    if ($row[2] !== null) {
        $row[2] = array_merge($row[2], $version);
    }
    $collection_name = $row[0];
    $c_mongo = $db->$collection_name;
    if ($dropMongo) {
        $c_mongo->drop();
    }

    $c_mongo->createIndex(array('Base62Id' => 1), array('unique' => true));
    $c_mongo->createIndex(array('removed_in' => 1));

    $varId = $row[1];
    if (file_exists("json/Full$varId.json.gz")) {
        $command = escapeshellcmd("gunzip -f json/Full$varId.json.gz");
        system($command);
    }
    if (file_exists("json/Full$varId.json")) {
        $handle = fopen(dirname(__FILE__) . "/json/Full$varId.json", "r");
        $loop_start = microtime(true);
        echo "<strong>Starting $varId</strong> - ";
        if ($handle) {
            $patch_version = trim(fgets($handle));
            echo "Patch Version $patch_version<br>";
            while (($line = fgets($handle)) !== false) {
                $segments = explode(',', $line, 3);
                $bId = $segments[0]; // bId for line
                $decoded_hash = $segments[1]; // Decoded hash for line
                $line = $segments[2]; // Rest of the JSON data for line
                $decoded = null;
                $decoded = json_decode($line, true); // Decode JSON data
                // Reset found variables
                $found_hash = null;
                $found_previous_versions = null;
                $found_current_version = null;
                $found_first_seen = null;
                $found_last_seen = null;
                // Check database if already in there, returning data as array using $options
                $options = ["typeMap" => ['root' => 'array', 'document' => 'array']];
                $found = $c_mongo->findOne(['Base62Id' => $bId], $options); // returns array
                if (!empty($found)) {
                    // Already in database, so we're going to update it
                    // echo "$bId Found:";
                    // echo "<pre>";
                    // print_r($found);
                    // echo "</pre>";

                    // echo "$bId Decoded:";
                    // echo "<pre>";
                    // print_r($decoded);
                    // echo "</pre>";

                    // Capture relevant found data
                    if (isset($found["hash"])) {
                        $found_hash = $found["hash"];
                    }
                    if (isset($found["previous_versions"])) {
                        $found_previous_versions = $found["previous_versions"];
                    }
                    if (isset($found["current_version"])) {
                        $found_current_version = $found["current_version"];
                    }
                    if (isset($found["first_seen"])) {
                        $found_first_seen = $found["first_seen"];
                    }
                    if (isset($found["last_seen"])) {
                        $found_last_seen = $found["last_seen"];
                    }

                    // Unset _id, hash, previous_versions, current_version, last_seen, and changed_fields in $found and $decoded
                    unset($found["_id"]);
                    // unset($found["hash"]);
                    unset($found["previous_versions"]);
                    unset($found["current_version"]);
                    unset($found["first_seen"]);
                    unset($found["last_seen"]);
                    unset($found["changed_fields"]);

                    $decoded["hash"] = $decoded_hash;
                    $decoded["removed_in"] = "";

                    $changed = compare_multi_arrays($decoded, $found);

                    $changed_diff = array_keys($changed["diff"]);
                    $changed_less = array_keys($changed["less"]);
                    $changed_more = array_keys($changed["more"]);
                    $changed_fields = array_merge($changed_less, $changed_more);
                    $changed_fields = array_merge($changed_fields, $changed_diff);
                    $changed_fields = array_unique($changed_fields);

                    if (!empty($changed_fields)) {
                        echo "$bId has changed fields";
                        // echo '<pre>';
                        // print_r($changed_fields);
                        // echo '</pre>';

                        if ($decoded_hash != $found_hash) {
                            // Hash is different
                            echo " - old hash: " . $found_hash . ', ';
                            echo "new hash: " . $decoded_hash;
                        }

                        echo '<br>';

                        // Get previous versions from $found and add in existing current_version
                        $previous_versions = $found_previous_versions;
                        $previous_versions[] = $found_current_version;

                        $decoded["previous_versions"] = $previous_versions;
                        $decoded["current_version"] = $patch_version;
                        $decoded["last_seen"] = $patch_version;
                        $decoded["changed_fields"] = $changed_fields;
                        $c_mongo->updateOne(
                            array("Base62Id" => $bId),
                            array('$set' => $decoded)
                        );
                    } else {
                        // No changed fields, just update last_seen
                        $c_mongo->updateOne(
                            array("Base62Id" => $bId),
                            array('$set' => array('last_seen' => $patch_version))
                        );
                    }
                } else {
                    // Not found in database, so insert it with version info
                    $decoded["first_seen"] = $patch_version;
                    $decoded["last_seen"] = $patch_version;
                    $decoded["current_version"] = $patch_version;
                    $decoded["hash"] = $decoded_hash;
                    $decoded["removed_in"] = "";
                    $c_mongo->insertOne($decoded);
                }
            }
            if ($patch_version !== null) {
                // Go back and find everything that was in the last patch but not this one, then add removed_in
                $c_mongo->updateMany(
                    ['last_seen' => ['$ne' => $patch_version], 'removed_in' => ['$exists' => true, '$eq' => ""]],
                    ['$set' => ['removed_in' => $patch_version]],
                    $options
                );
            }
            fclose($handle);
            if (!empty($row[2])) {
                foreach ($row[2] as $indexArr) {
                    if (count($indexArr) == 2) {
                        if ($indexArr[1] !== null) {
                            $c_mongo->createIndex($indexArr[0], $indexArr[1]);
                        } else {
                            $c_mongo->createIndex($indexArr[0]);
                        }
                    } else {
                        $c_mongo->createIndex($indexArr);
                    }
                }
            }
            $loop_end = microtime(true);
            $loop_time = round($loop_end - $loop_start, 2);

            echo "<strong>$varId complete in $loop_time seconds</strong><br>";
            echo "-----------------------------------<br>";
            if ($delJson) {
                unlink(dirname(__FILE__) . "/json/Full$varId.json");
            }
        } else {
            echo "$varId failed<br>";
        }
    } else {
        $skipped[] = $varId;
    }
}
echo "<br>-----------------------------------<br>";
if (count($skipped)) {
    echo implode(', ', $skipped) . " skipped.<br>";
}
$time_end = microtime(true);
$time = round($time_end - $time_start, 2);
echo "<br><strong>Completed all imports in $time seconds.</strong>";
