# NGUInjector

NGUInjector is an automation platform for the steam version of NGU Idle.

# Instructions

Releases can be found on in the releases section of the github page [here](https://github.com/rvazarkar/NGUInjector/releases). Extract the downloaded archive to a directory of your choice. To start automation, run inject.bat in the releases folder when NGU Idle is open.

To upgrade to a new release, restart NGU idle and run the inject script again.

# Configuration

After injecting the dll, a new folder will be created on your Desktop called NGUInjector. Settings files will be automatically written to this directory. The following files are of interest:

- settings.json - Contains settings used by the application. Many settings can be configured from the in-game menu, while the following can only be configured from this file.
  - \_boostIds - The ids corresponding to items you want to boost. This is in order, so you can boost in a certain order
  - \_titanLoadout - The item IDs for gear you want to equip for autokilling titans.
  - \_yggdrasilLoadout - The item IDs for gear you want to equip when harvesting yggdrasil fruits
- allocation.json - Breakpoints for assigning gear, magic, energy, and diggers. See the allocation section for more information

Saving either file will automatically reload the settings in the configuration file. Reloading the game isn't necessary

# In Game Configuration

An in game menu can be opened using the F1 button.

![Settings Menu](https://i.imgur.com/jD3mV2V.png)

- **Global Enable** - If turned off, all automation functions will stop.

- **Manage Yggdrasil** - Enables automatic harvesting and activation of Yggdrasil fruits. Will only harvest max tier fruits
- **Auto Fight Bosses** - Automatically nuke/fight bosses when an attack/defense threshold is met

- **Zone Boss Sniping** - Automatically snipes bosses in the zone specified in Zone to Snipe.
- **Precast Buffs** - Cast charge/parry before and wait for cooldowns before starting the snipe process. Good for zones you're still low for.
- **Fast Combat** - Don't use any buffs during combat, just attacks. Good for zones you're not quite strong enough to idle but dont need to cast buffs for.

- **Zone to Snipe** - See above

- **Highest Titan AK** - The highest titan you can autokill in your TitanLoadout. When this titan or any previous titan is spawning, gear will be swapped to autokill.

- **Manage Inventory** - When enabled, the following features will be enabled
  - **Auto Boost Merging** - If a boost is locked, boosts will automatically be merged until the boost hits 100.
  - **Auto Merging** - Equipped items and items in inventory will automatically be merged, as well as MacGuffins. The highest level item will be selected for merging automatically. A locked item will always be preferred.
  - **Auto Boosting** - Automatically boost items in your \_boostIds setting. Will select the highest level item if multiple exist. A locked item will always be preferred. Leftover boosts will go to infinity cube.
  - **Auto Filtering** - If improved loot filter is purchased, will automatically filter items when they hit level 100
  - **Auto Convertible Management** - Will automatically transform non-locked pendants/lootys and other items
  - **Quest Item Management** - Will try to merge a quest item to level 100 if a locked one is found. Otherwise will automatically turn in quest items
- **Manage Energy** - Will automatically allocate energy based on breakpoints in allocation.json
- **Manage Magic** - Will automatically allocate magic based on breakpoints in allocation.json

- **Manage Gear** - Will automatically equip gear based on breakpoints in allocation.json
- **Manage Diggers** - Will automatically equip diggers based on breakpoints in allocation.json
- **Manage Boost Conversion** - If boost conversion is unlocked from 100 Level Challenge, will calculate needed boosts for items in boostIds and then switch auto conversion appropriately. Will prioritize power > toughness > special

- **Swap Loadout for Titan** - Will automatically swap gear and diggers for titan autokills using the TitanLoadout in your settings file.
  - Will equip the following diggers: EXP Digger, PP Digger, Adventure Digger, Drop Chance Digger
- **Swap Loadout for Yggdrasil** - Will automatically swap gear and diggers for yggdrasil harvests using the YggdrasilLoadout in your settings file.

  - Will equip the following diggers: PP Digger, EXP Digger

- **Auto Quest** - If enabled, quests will automatically be turned in. Behavior varies based on the Allow Major Quests option
- **Allow Major Quests**
  - If this option is enabled, when major quests are available they will be accepted and automation will manually farm the items from the appropriate zone
  - If this option is disabled, major quests will not be accepted.
  - When no major quests are available, minor quests will be accepted and idled.
- **Auto-Move to ITOPOD** - If enabled, you will automatically be moved to the ITOPOD when no major quests are in progress or if major quests is disabled

# Allocation

The allocation.json file is a JSON file that contains time breakpoints for configuring your gear, diggers, energy allocation and magic allocation. A sample allocation file can be found in the sampleprofile folder or on the repo [here](https://github.com/rvazarkar/NGUInjector/blob/master/NGUInjector/SampleProfiles/24hr.json).

The time portion of every breakpoint refers to rebirth time in seconds.

## Energy

An energy breakpoint is structured as follows:

```
{
"Time": 0,
"Priorities": ["CAPNGU-0", "CAPWAN", "AT-1", "NGU-1"]
}
```

Priorities come in 2 types - cap and non-cap. Any priority that has -X after it is 0 indexed.

When a cap priority is used, the equivalent of hitting the cap button will be used. If a cap button doesn't exist, the program will calculate an appropriate cap and attempt to BB for the next 10 seconds. If cap is higher than idle, all energy will be allocated to it. Cap priorities always run before regular priorities.
When a non-cap priority is used, remaining energy after cap priorities will be evenly divided between the allocations specified.

In the above example the following actions will be taken:

- NGU-0 (NGU Augments) will be capped
- Wandoos energy will be capped
- Remaining energy will be split between AT-1 (Advanced Training Power) and NGU-1 (NGU Wandoos)

Available cap priorities for Energy are as follows:

- CAPNGU-X (0-8) - Use the cap button for the NGU
- CAPAT-X (0-4) - Calculate a cap for the AT and attempt to BB it.
- CAPWAN - Use the cap button for wandoos energy
- CAPTM - Calculate a cap for energy time machine and attempt to BB it.

Available non-cap priorities for Energy are as follows:

- NGU-X (0-8) - Allocate energy to NGU
- AT-X (0-4) - Allocate energy to AT
- AUG-X (0-13) - Allocate energy to augment
- WAN - Allocate energy to energy wandoos
- TM - Allocate energy to energy time machine

More information on allocation indexes can be found on the [wiki](https://github.com/rvazarkar/NGUInjector/wiki/Allocation-Indexes)

## Magic

A magic breakpoint is structured as follows:

```
{
"Time": 0,
"Priorities": ["CAPNGU-0", "CAPWAN", "BR", "NGU-1"]
}
```

Priorities come in 2 types - cap and non-cap. Any priority that has -X after it is 0 indexed.

When a cap priority is used, the equivalent of hitting the cap button will be used. If a cap button doesn't exist, the program will calculate an appropriate cap and attempt to BB for the next 10 seconds. Cap priorities always run before regular priorities.
When a non-cap priority is used, remaining magic after cap priorities will be evenly divided between the allocations specified.

In the above example the following actions will be taken:

- NGU-0 (NGU Yggdrasil) will be capped
- Wandoos magic will be capped
- Magic rituals will be capped from highest to lowest, with rituals taking more than 1 hour skipped
- Remaining magic will be allocated to NGU-1 (NGU Exp)

Available cap priorities for Magic are as follows:

- CAPNGU-X (0-6) - Use the cap button for the NGU
- CAPWAN - Use the cap button for wandoos magic
- CAPTM - Calculate a cap for magic time machine and attempt to BB it.
- BR - Cast rituals from highest to lowest ignoring rituals you cant afford or will take more than an hour

Available non-cap priorities for Magic are as follows:

- NGU-X (0-6) - Allocate energy to NGU
- WAN - Allocate energy to magic wandoos
- TM - Allocate energy to magic time machine

More information on allocation indexes can be found on the [wiki](https://github.com/rvazarkar/NGUInjector/wiki/Allocation-Indexes)

## Gear

A gear breakpoint is structured as follows:

```
{
    "Time": 0,
    "ID": [189, 442, 160, 441, 148, 169, 139, 184, 187, 185, 186, 188]
}
```

The list of IDs is the IDs of the gear desired.

## Diggers

A digger breakpoint is structured as follows:

```
{
    "Time": 3650,
    "List": [3, 4, 5, 8]
}
```

The list of diggers is 0 indexed. Drop chance digger is 0, PP digger is 11. More information can be found on the [wiki](https://github.com/rvazarkar/NGUInjector/wiki/Allocation-Indexes)

## Wandoos

A wandoos breakpoint is structured as follows:

```
{
    "Time": 0,
    "OS": 1
}
```

The OS is 0 indexed. Wandoos 98 is 0, Wandoos MEH is 1, Wandoos XL is 2

## Rebirth

A rebirth breakpoint is added as follows:

```
"RebirthTime": -1
```

A setting of -1 means no rebirths. Otherwise rebirth will be performed when the time in seconds is reached.

# Other Features

## Quicksave + NGUSav.ES JSON

Pressing the F3 button numpad will dump a NGU save file as well as a ngusav.es JSON file in the folder on desktop, which can be loaded into Gear Optimizer.

## Monitor Loot Log File

The adventure log will automatically be monitored and loot dropped will be written to loot.log in the folder on your desktop.

## Quick Disable/Enable

Pressing the F2 button will globally disable all features of the app until re-enabled.

## Turn on Autoattack in ITOPOD

Have you forgotten to turn on auto attack before and left yourself AFK in ITOPOD doing nothing? Me too! The script will automatically turn auto attack on if you're in ITOPOD and its not on.
