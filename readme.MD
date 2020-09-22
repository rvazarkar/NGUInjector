# NGUInjector

NGUInjector is an automation platform for the steam version of NGU Idle.

# Instructions

Releases can be found on in the releases section of the github page [here](https://github.com/rvazarkar/NGUInjector/releases). Extract the downloaded archive to a directory of your choice. To start automation, run inject.bat in the releases folder.

# Configuration

After injecting the dll, a new folder will be created on your Desktop called NGUInjector. Settings files will be automatically written to this directory. The following files are of interest:

- settings.json - Contains settings used by the application. Many settings can be configured from the in-game menu, while the following can only be configured from this file.
  - \_boostIds - The ids corresponding to items you want to boost.
  - \_titanLoadout - The item IDs for gear you want to equip for autokilling titans.
  - \_yggdrasilLoadout - The item IDs for gear you want to equip when harvesting yggdrasil fruits
- allocation.json - Breakpoints for assigning gear, magic, energy, and diggers. See the allocation section for more information

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

The list of diggers is 0 indexed. Drop chance digger is 0, PP digger is 13
