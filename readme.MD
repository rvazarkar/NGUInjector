# NGUInjector

NGUInjector is an automation platform for the steam version of NGU Idle.

# Instructions

Releases can be found in the releases section of the github page [here](https://github.com/rvazarkar/NGUInjector/releases). The latest release can be found [here](https://github.com/rvazarkar/NGUInjector/releases/latest). Do not download the source code archive, instead download the zip file with the release version in the name (3.0.0.zip for example). Extract the downloaded archive to a directory of your choice. To start automation, run inject.bat in the extracted folder when NGU Idle is open. DO NOT RUN AS ADMINISTRATOR.

You'll know the injection has worked if you see the following in the upper left portion of NGU.

![Injected](https://i.imgur.com/JIF2pl4.png)

To upgrade to a new release, restart NGU idle and run the inject script again.

# Configuration

After injecting the dll, a new folder will be created on your Desktop called NGUInjector. Settings files will be automatically written to this directory. The following files are of interest:

- **settings.json** - Contains settings used by the application. Most settings can be configured from the in-game menu.
- **zoneOverrides.json** - Override the default values for Idle/Manual Power/Toughness for gold sniping

A profiles folder will be created with the following files:

- **default.json** - Breakpoints for assigning gear, magic, energy, wandoos OS, rebirth time, and diggers. See the allocation section for more information

More profiles can be added to this folder at your discretion.

A logs folder will also be created with the following files:

- **loot.log** - Logs all the loot dropped by enemies. Also logs money pit and daily spin results.
- **inject.log** - Logs general information about the injector
- **combat.log** - Logs information from the combat algorithm
- **allocation.log** - Logs information about allocating energy/magic/gear/diggers

Saving either file will automatically reload the settings in the configuration file. Reloading the game isn't necessary

# Settings Configuration

An in game menu can be opened using the F1 button.

![General Settings](https://i.imgur.com/Ru8YXTA.png)

- **Master Switch** - If turned off, will disable all automation

- **Auto Daily Spin** - Automatically do daily spin and log the result to the loot.log file
- **Auto Money Pit** - Automatically throw money into the money pit if the Money Pit Threshold is met. Logs the result to the loot.log file
- **Auto Move to ITOPOD** - Automatically move to ITOPOD when nothing else is happening in automation.
- **Auto Fight Bosses** - Automatically nuke/fight bosses.
- **Auto Buy Energy/Magic/R3** - Automatically spend EXP on energy/magic/R3. Will use the custom caps set by the user in the spend exp page. Will always buy unlocked exp spends in groups.
- **Money Pit Threshold** - The amount of gold to wait for to throw gold into the money pit. Accepts scientific notation (ex: 1E+28)

![Allocation Settings](https://i.imgur.com/dbpniX2.png)

- **Allocation Profile** - Allows you to quickly switch allocation profiles. Any profiles stored in the profiles folder will be listed here. Press change allocation after swapping to change to load the new allocation.
- **Manage Energy** - Manage energy based on breakpoints in allocation.json
- **Manage Magic** - Manage magic based on breakpoints in allocation.json
- **Manage R3** - Manage resource 3 based on breakpoints in allocation.json
- **Manage Gear** - Manage gear based on breakpoints in allocation.json
- **Manage NGU Difficulty** - Manage NGU difficulty based on breakpoints in allocation.json
- **Manage Diggers** - Manage diggers based on breakpoints in allocation.json
- **Manage Wandoos OS** - Manage wandoos OS based on breakpoints in allocation.json
- **Auto Rebirth** - Automatically rebirth after the time in allocation.json is reached.

- **Change Auto Spells** - Manage Spoopy Spell casting
- **Spaghetti %** - Percent to cast Blood Spaghetti until
- **Number Bonus** - Number to cast Blood NUMBER Boost until
- **Counterfeit %** - Percent to cast Counterfeit Gold until

![Yggdrasil](https://i.imgur.com/VTdl8Hy.png)

- **Manage Yggdrasil** - Automatically harvest fruits.
- **Activate Fruits** - Reactivate fruits you haven't bought the exp perk for
- **Swap Loadout for Yggdrasil** - Equip the loadout specified in YggdrasilLoadout before harvesting
- **Loadout** - Edit the loadout used for harvest yggdrasil. Add items using the box in the bottom.
- **Harvest All Now** - Allows you to do a Yggdrasil swap and immediately harvest all fruits. Check the box next to the button to enable it. Good for manual rebirths

![Inventory](https://i.imgur.com/TfCk3IO.png)

- **Manage Inventory** - When enabled, the following features will be enabled
  - **Auto Boost Merging** - If a boost is locked, boosts will automatically be merged until the boost hits 100.
  - **Auto Merging** - Equipped items and items in inventory will automatically be merged, as well as MacGuffins. The highest level item will be selected for merging automatically. A locked item will always be preferred.
  - **Auto Boosting** - Automatically boost items. Will start with items in the PriorityBoosts setting, followed by equipped items, locked items in inventory and finally infinity cube.
  - **Auto Filtering** - If improved loot filter is purchased, will automatically filter items when they hit level 100
  - **Auto Convertible Management** - Will automatically transform non-locked pendants/lootys and other items
  - **Quest Item Management** - Will try to merge a quest item to level 100 if a locked one is found. Otherwise will automatically turn in quest items
- **Manage Boost Conversion** - If all normal 100 Level Challenges are completed, will calculate needed boosts for items in boostIds and then switch auto conversion appropriately. Will prioritize power > toughness > special. Will also show you the hourly average of boost progress in the inject.log file.
- **Cube Priority** - How to boost cube.
  - **None** - Will turn off boost conversion
  - **Balanced** - Will keep power and toughness on cube balanced
  - **Power** - Will only put power into cube
  - **Toughness** - Will only put toughness into the cube
- **Priority Boosts** - Edit the items you wish to boost before others. Add items using the box in the bottom. Order is respected.
- **Blacklisted Items** - Edit the items you wish to never be boosted. Add items using the box in the bottom.

![Titans](https://i.imgur.com/Ikq0bUZ.png)

- **Swap Loadout for Titans** - Equip the gear in TitanLoadout right before any titan less than or equal your highest autokillable titan spawns
- **Highest Autokillable Titan** - The highest titan you can autokill in your TitanLoadout. When this titan or any previous titan is spawning, a titan loadout will be equipped
- **Loadout** - The gear to equip when autokilling titans.

![Adventure](https://i.imgur.com/ThIghQJ.png)

- **Combat Enabled** - Turns on auto combat to go to target zone and kill stuff.
- **Bosses Only** - If enabled, will only fight bosses when doing combat. Good for zone sniping.
- **Precast Buffs** - If enabled, will cast charge and parry in safe zone before each combat and wait for the cooldowns to recharge.
- **Recover HP** - If enabled, will recover HP in safe zone between kills
- **Fast Combat** - If enabled, will just quickly attack and skip buffs altogether
- **Beast Mode** - Whether to use beast mode or not while in combat
- **Optimize ITOPOD Floor** - Will optimize your ITOPOD floor every 10 seconds. Does nothing if Lazy Floor Shifter is on
- **Combat Mode** - If set to manual, will execute a custom combat routine. If set to idle, will just idle the zone.
- **Target Zone** - The target zone to snipe/do combat in. ITOPOD is at the bottom of the list.
- **Allow Fallthrough** - If set to true, combat will use the highest unlocked zone you have until the target zone is unlocked.
- **ITOPOD** - Equivalent to setting target zone to ITOPOD, just a quicker option

![Gold](https://i.imgur.com/FplCVyi.png)

- **Enable Gold Management** - If turned off, will disable all gold swap logic
- **Do Gold Snipe** - If checked, will immediately equip gold loadout gear and perform a boss snipe in the best calculated zone based on the zone stats and gold loadout power/toughness.
- **Resnipe Time** - Set this value to re-enable gold snipe at a certain time during your rebirth. Best set to after AT is completed in order to get a higher zone.
- **CBlock Mode** - If checked, will automatically gold snipe every new zone that is unlocked. Useful for challenge blocks where zones sporadically become available.
- **Loadout** - Items to use when swapping for gold loadout.
- **Titans to Swap For** - Check the box next to titans you want to swap to gold loadouts for. Leave all boxes unchecked in order to never use gold loadouts for titans

![Quests](https://i.imgur.com/4AuAUhy.png)

- **Auto Quest** - If enabled, quests will automatically be turned in. Behavior varies based on the Allow Major Quests option
- **Allow Major Quests**
  - If this option is enabled, when major quests are available they will be accepted and automation will manually farm the items from the appropriate zone
  - If this option is disabled, major quests will not be accepted.
  - When no major quests are available, minor quests will be accepted and idled.
- **Abandon Minor Quests** - If enabled and minor quest progress is below the minor quest threshold, the minor quest will be abandoned and a major quest will be picked up.
- **Manual Minors** - If enabled will manual minor quests as well as majors
- **Use Butter on Majors** - If enabled will attempt to butter major quests
- **Use Butter on Minors** - If enabled will attempt to butter minor quests
- **Abandon Minor Quest Threshold** - Quest progress before which its acceptable to abandon minor quests.
- **Combat Mode** - If set to manual, will perform a combat routine to do your quest zone. Otherwise will idle the zone.
- **Fast Combat** - If enabled will skip all buffs during combat.

![Wishes](https://i.imgur.com/sXJuQQD.png)

- **Priority Wishes** - The wishes to prioritize doing. Will switch to cheapest wish when all wish priorities are complete

# Allocation

The allocation.json file is a JSON file that contains time breakpoints for configuring your gear, diggers, energy allocation and magic allocation. Sample allocation files can be found in the sampleprofile folder.

[Sample 24 Hour Profile](https://github.com/rvazarkar/NGUInjector/blob/master/NGUInjector/SampleProfiles/24hr_with_gear.json).

The time portion of every breakpoint refers to rebirth time in seconds.

## Energy

An energy breakpoint is structured as follows:

```
"Energy": [
    {
        "Time": 0,
        "Priorities": ["CAPNGU-0", "CAPWAN", "AT-1", "NGU-1"]
    }
]

```

Priorities come in 2 types - cap and non-cap. Any priority that has -X after it is 0 indexed.

A cap priority will use as much idle energy as possible and hit the highest BB breakpoint possible for the next 10 seconds. A non-cap priority will take a divider of your idle energy based on number of non-cap priorities and attempt to hit the highest BB breakpoint possible for the next 10 seconds. When a priority is calculated, it will push excess energy to later priorities.

In the above example the following actions will be taken:

- NGU-0 (NGU Augments) will be capped
- Wandoos energy will be capped
- Remaining energy will be split between AT-1 (Advanced Training Power) and NGU-1 (NGU Wandoos)

Available cap priorities for Energy are as follows:

- CAPNGU-X (0-8) - Calculate a cap for the NGU
- CAPALLNGU - Calculate a cap for every NGU starting from 0
- CAPAT-X (0-4) - Calculate a cap for the AT and attempt to BB it.
- CAPWAN - Calculate a cap for wandoos energy
- CAPTM - Calculate a cap for energy time machine and attempt to BB it.
- CAPBT-X (0-11) - Calculate a cap for basic training
- CAPALLBT - Caps all basic trainings
- CAPAUG-X (0-13) - Calculate a cap for augments and attempt to BB it

Available non-cap priorities for Energy are as follows:

- ALLNGU - Allocate energy to every NGU
- NGU-X (0-8) - Allocate energy to NGU
- AT-X (0-4) - Allocate energy to AT
- AUG-X (0-13) - Allocate energy to augment
- WAN - Allocate energy to energy wandoos
- TM - Allocate energy to energy time machine
- BT-X (0-11) - Allocate energy to basic training
- WISH-X (0-X) - Allocate energy to wish. X is the wish slot you wish to allocate too. If you only have 1 slot, WISH-0 will be the one you use. Wishes will follow the priority set in the WishPriorities setting and then work through cheapest wishes

More information on allocation indexes can be found on the [wiki](https://github.com/rvazarkar/NGUInjector/wiki/Allocation-Indexes)

## Magic

A magic breakpoint is structured as follows:

```
"Magic": [
    {
        "Time": 0,
        "Priorities": ["CAPNGU-0", "CAPWAN", "BR", "NGU-1"]
    }
]

```

Priorities come in 2 types - cap and non-cap. Any priority that has -X after it is 0 indexed.

A cap priority will use as much idle energy as possible and hit the highest BB breakpoint possible for the next 10 seconds. A non-cap priority will take a divider of your idle energy based on number of non-cap priorities and attempt to hit the highest BB breakpoint possible for the next 10 seconds. When a priority is calculated, it will push excess energy to later priorities.

In the above example the following actions will be taken:

- NGU-0 (NGU Yggdrasil) will be capped
- Wandoos magic will be capped
- Magic rituals will be capped from highest to lowest, with rituals taking more than 1 hour skipped
- Remaining magic will be allocated to NGU-1 (NGU Exp)

Available cap priorities for Magic are as follows:

- CAPNGU-X (0-6) - Use the cap button for the NGU
- CAPALLNGU - Use the cap button for every NGU starting from 0
- CAPWAN - Use the cap button for wandoos magic
- CAPTM - Calculate a cap for magic time machine and attempt to BB it.
- CAPRIT-X - Calculate a cap for the ritual and allocate
- BR - Cast rituals from highest to lowest ignoring rituals you cant afford or will take more than an hour
- BR-X - Cast rituals from highest to lowest that will finish before time specified by X. BR-3600 will cast rituals that will end before the 1 hour mark from your current time.

Available non-cap priorities for Magic are as follows:

- NGU-X (0-6) - Allocate energy to NGU
- WAN - Allocate energy to magic wandoos
- TM - Allocate energy to magic time machine
- RIT-X - Allocate energy to ritual
- WISH-X (0-X) - Allocate magic to wishes. X is the wish slot you wish to allocate too. If you only have 1 slot, WISH-0 will be the one you use. Wishes will follow the priority set in the WishPriorities setting and then work through cheapest wishes

More information on allocation indexes can be found on the [wiki](https://github.com/rvazarkar/NGUInjector/wiki/Allocation-Indexes)

## R3

An R3 breakpoint is structured as follows:

```
"R3": [
    {
        "Time": 0,
        "Priorities": ["HACK-1"]
    }
]

```

Available priorities for R3 are as follows:

- HACK-X (0-13) - Allocate R3 to hacks
- WISH-X (0-X) - Allocate R3 to wishes. X is the wish slot you wish to allocate too. If you only have 1 slot, WISH-0 will be the one you use. Wishes will follow the priority set in the WishPriorities setting and then work through cheapest wishes

## Gear

A gear breakpoint is structured as follows:

```
"Gear": [
    {
        "Time": 0,
        "ID": [189, 442, 160, 441, 148, 169, 139, 184, 187, 185, 186, 188]
    }
]

```

The list of IDs is the IDs of the gear desired.

You can dump your loadouts from Gear Optimizer using the method found on [this wiki page](https://github.com/rvazarkar/NGUInjector/wiki/Dump-Equipment-from-GO)

## Diggers

A digger breakpoint is structured as follows:

```
"Diggers": [
    {
        "Time": 3650,
        "List": [3, 4, 5, 8]
    }
]

```

The list of diggers is 0 indexed. Drop chance digger is 0, EXP digger is 11. More information can be found on the [wiki](https://github.com/rvazarkar/NGUInjector/wiki/Allocation-Indexes)

## Wandoos

A wandoos breakpoint is structured as follows:

```
"Wandoos": [
    {
        "Time": 0,
        "OS": 1
    }
]

```

The OS is 0 indexed. Wandoos 98 is 0, Wandoos MEH is 1, Wandoos XL is 2

## NGU Difficulty

A NGU Difficulty breakpoint is structured as follows:

```
"NGUDiff": [
    {
    "Time": 0,
    "Diff": 0
    }
]
```

The difficulty is 0 indexed. Normal NGUs is 0, Evil NGUs is 1, Sadistic NGUs is 2

## Rebirth

A rebirth breakpoint is added as follows:

```
"RebirthTime": -1
```

A setting of -1 means no rebirths. Otherwise rebirth will be performed when the time in seconds is reached.

# Zone Stat Overrides

The optimal zone for gold sniping is calculated using a set of values from pins that show stats necessary to do each zone. If the manual threshold is met for a zone, the script will snipe a boss without fast combat. If the idle threshold is met, the script will snipe a boss using fast combat. Beast Mode will always be turned off for this.

The stats for the zones can be manually overriden using the `zoneOverrides.json` file in the user's directory. For the default stats used see [here](https://github.com/rvazarkar/NGUInjector/wiki/Default-Zone-Stats-for-Sniping)

# Other Features

## Quicksave + NGUSav.ES JSON

Pressing the F3 button will dump a NGU save file as well as a ngusav.es JSON file in the folder on desktop, which can be loaded into Gear Optimizer.

## Quickload

Pressing the F7 button will load the save created by the quicksave function

## Dump Equipment to Log

Pressing the F5 buttonw will dump your current equipment to the log file for use in allocation.json

## Monitor Loot Log File

The adventure log will automatically be monitored and loot dropped will be written to loot.log in the folder on your desktop.

## Monitor Money Pit/Daily Spin/Yggdrasil Results

The pitspin.log file will contain the results of each automatic money pit, daily spin, and yggdrasil harvest and will persist across sessions.

## Quick Disable/Enable

Pressing the F2 button will globally disable all features of the app until re-enabled.

## Turn on Autoattack in ITOPOD

Have you forgotten to turn on auto attack before and left yourself AFK in ITOPOD doing nothing? Me too! The script will automatically turn auto attack on if you're in ITOPOD and its not on.

## Auto Merge Boost to 100

Lock a boost in your inventory and the script will automatically merge boosts on to that boost until it hits level 100!

# Acknowledgements

SharpMonoInjector can be found [here](https://github.com/warbler/SharpMonoInjector). This wouldn't be possible without the excellent work done there.
Obviously 4G for making NGU Idle.
Altizar on discord for WISH logic and some other fixes
Ingwar on discord for allocation profile logic and some other fixes
