using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SimpleJSON;

namespace NGUInjector.Managers
{
    internal class ZoneStatHelper
    {
        internal static Dictionary<int, ZoneStats> UserOverrides;

        internal static void CreateOverrides(string dir)
        {
            UserOverrides = Defaults.ToDictionary(entry => entry.Key, entry => entry.Value);
            var overridePath = Path.Combine(dir, "zoneOverride.json");
            if (!File.Exists(overridePath))
            {
                var emptyZones = @"{
    ""zones"": {
    ""0"": {
      ""MPower"": 10,
      ""MToughness"": 10,
      ""IPower"": 13,
      ""IToughness"": 13,
      ""OPower"": 129.5,
      ""Name"": ""Tutorial Zone""
    }
}
}
        ";

                using (var writer = new StreamWriter(File.Open(overridePath, FileMode.CreateNew)))
                {
                    writer.WriteLine(emptyZones);
                    writer.Flush();
                }
            }

            var overrides = new List<string>();

            try
            {
                var text = File.ReadAllText(overridePath);
                var parsed = JSON.Parse(text);
                var zones = parsed["zones"];
                
                foreach (var key in zones.Keys)
                {
                    var success = int.TryParse(key.Value, out var index);
                    if (!success)
                        continue;
                    Main.Log($"Key: {index}");
                    var stat = new ZoneStats
                    {
                        IPower = zones[key.Value]["IPower"].AsDouble,
                        IToughness = zones[key.Value]["IToughness"].AsDouble,
                        MPower = zones[key.Value]["MPower"].AsDouble,
                        MToughness = zones[key.Value]["MToughness"].AsDouble,
                        OPower = zones[key.Value]["OPower"].AsDouble,
                        Name = zones[key.Value]["Name"]
                    };
                    UserOverrides[index] = stat;
                    overrides.Add(stat.Name);
                }
            }
            catch (Exception e)
            {
                Main.Log(e.Message);
                Main.Log(e.StackTrace);
            }

            if (overrides.Count > 0)
                Main.Log($"Loaded Zone Overrides: {string.Join(",", overrides.ToArray())}");
        }

        internal static ZoneTarget GetBestZone()
        {
            if (UserOverrides == null)
                return null;
            var power = Main.Character.totalAdvAttack();
            var toughness = Main.Character.totalAdvDefense();
            var availableZones = UserOverrides.Where(x => x.Key <= ZoneHelpers.GetMaxReachableZone(false));
            var finalAvailableZones = availableZones.Where(x => x.Value.FightType(power, toughness) > 0).ToDictionary(x => x.Key, x => x.Value);;
            var bestZoneId = finalAvailableZones.Max(x => x.Key);
            var zoneStats = finalAvailableZones[bestZoneId];
            var bestZone = new ZoneTarget
            {
                FightType = zoneStats.FightType(power, toughness),
                Zone = bestZoneId
            };

            return bestZone;
        }


        internal static Dictionary<int, ZoneStats> Defaults = new Dictionary<int, ZoneStats>
        {
            {
                0, new ZoneStats
                {
                    MPower = 10,
                    MToughness = 10,
                    IPower = 13,
                    IToughness = 13,
                    OPower = 129.5,
                    Name = "Tutorial Zone"
                }
            },
            {
                1, new ZoneStats
                {
                    MPower = 12,
                    MToughness = 12,
                    IPower = 21,
                    IToughness = 21,
                    OPower = 194,
                    Name = "Sewers"
                }
            },
            {
                2, new ZoneStats
                {
                    MPower = 35,
                    MToughness = 35,
                    IPower = 53,
                    IToughness = 53,
                    OPower = 1134,
                    Name = "Forest"
                }
            },
            {
                3, new ZoneStats
                {
                    MPower = 150,
                    MToughness = 150,
                    IPower = 200,
                    IToughness = 200,
                    OPower = 3811,
                    Name = "Cave of Many Things"
                }
            },
            {
                4, new ZoneStats
                {
                    MPower = 600,
                    MToughness = 400,
                    IPower = 750,
                    IToughness = 650,
                    OPower = 11420,
                    Name = "The Sky"
                }
            },
            {
                5, new ZoneStats
                {
                    MPower = 700,
                    MToughness = 500,
                    IPower = 750,
                    IToughness = 750,
                    OPower = 15220,
                    Name = "High Security Base"
                }
            },
            {
                7, new ZoneStats
                {
                    MPower = 3250,
                    MToughness = 2250,
                    IPower = 4500,
                    IToughness = 3000,
                    OPower = 107110,
                    Name = "Clock Dimension"
                }
            },
            {
                9, new ZoneStats
                {
                    MPower = 4500,
                    MToughness = 3500,
                    IPower = 8000,
                    IToughness = 6000,
                    OPower = 168223,
                    Name = "2D Universe"
                }
            },
            {
                10, new ZoneStats
                {
                    MPower = 12000,
                    MToughness = 10000,
                    IPower = 17000,
                    IToughness = 16000,
                    OPower = 282966,
                    Name = "Ancient Battlefield"
                }
            },
            {
                12, new ZoneStats
                {
                    MPower = 28000,
                    MToughness = 18000,
                    IPower = 48000,
                    IToughness = 38000,
                    OPower = 842483,
                    Name = "A Very Strange Place"
                }
            },
            {
                13, new ZoneStats
                {
                    MPower = 125000,
                    MToughness = 60000,
                    IPower = 265000,
                    IToughness = 145000,
                    OPower = 3500000,
                    Name = "Mega Lands"
                }
            },
            {
                15, new ZoneStats
                {
                    MPower = 1300000,
                    MToughness = 550000,
                    IPower = 3000000,
                    IToughness = 2200000,
                    OPower = 4.62e7,
                    Name = "Beardverse"
                }
            },
            {
                17, new ZoneStats
                {
                    MPower = 25000000,
                    MToughness = 15000000,
                    IPower = 45000000,
                    IToughness = 35000000,
                    OPower = 8.89e8,
                    Name = "Badly Drawn World"
                }
            },
            {
                18, new ZoneStats
                {
                    MPower = 180000000,
                    MToughness = 90000000,
                    IPower = 360000000,
                    IToughness = 270000000,
                    OPower = 7.21e9,
                    Name = "Boring-Ass Earth"
                }
            },
            {
                20, new ZoneStats
                {
                    MPower = 7e10,
                    MToughness = 5e10,
                    IPower = 1.5e11,
                    IToughness = 9e10,
                    OPower = 2.72e12,
                    Name = "Chocolate World"
                }
            },
            {
                21, new ZoneStats
                {
                    MPower = 1e13,
                    MToughness = 4.7e12,
                    IPower = 2.4e13,
                    IToughness = 1.6e13,
                    OPower = 4.4e14,
                    Name = "Evilverse"
                }
            },
            {
                22, new ZoneStats
                {
                    MPower = 5.4e13,
                    MToughness = 2.4e13,
                    IPower = 1.3e14,
                    IToughness = 9.7e13,
                    OPower = 2.27e15,
                    Name = "Pretty Pink Princess Land"
                }
            },
            {
                24, new ZoneStats
                {
                    MPower = 2.6e16,
                    MToughness = 1.2e16,
                    IPower = 4.5e16,
                    IToughness = 3.1e16,
                    OPower = 1.05e18,
                    Name = "Meta Land"
                }
            },
            {
                25, new ZoneStats
                {
                    MPower = 2.5e17,
                    MToughness = 1.1e17,
                    IPower = 4.8e17,
                    IToughness = 3.1e17,
                    OPower = 1.05e19,
                    Name = "Interdimensional Party"
                }
            },
            {
                27, new ZoneStats
                {
                    MPower = 1.5e20,
                    MToughness = 6.8e19,
                    IPower = 2.7e20,
                    IToughness = 2.4e20,
                    OPower = 6.92e21,
                    Name = "Typo Zonw"
                }
            },
            {
                28, new ZoneStats
                {
                    MPower = 7e20,
                    MToughness = 4e20,
                    IPower = 1.5e21,
                    IToughness = 1.1e21,
                    OPower = 3.56e22,
                    Name = "The Fad-Lands"
                }
            },
            {
                29, new ZoneStats
                {
                    MPower = 4e21,
                    MToughness = 2e21,
                    IPower = 9e21,
                    IToughness = 6e21,
                    OPower = 7.2e23,
                    Name = "JRPGVille"
                }
            },
            {
                31, new ZoneStats
                {
                    MPower = 3.2e24,
                    MToughness = 1.4e24,
                    IPower = 7.8e24,
                    IToughness = 5.2e24,
                    OPower = 1.04e25,
                    Name = "The Rad-Lands"
                }
            },
            {
                32, new ZoneStats
                {
                    MPower = 5e26,
                    MToughness = 2.5e26,
                    IPower = 1.75e27,
                    IToughness = 8.8e26,
                    OPower = 1.76e27,
                    Name = "Back To School"
                }
            },
            {
                33, new ZoneStats
                {
                    MPower = 2.65e27,
                    MToughness = 8.26e26,
                    IPower = 8.85e27,
                    IToughness = 4.6e27,
                    OPower = 9.2e27,
                    Name = "The West World"
                }
            },
            {
                35, new ZoneStats
                {
                    MPower = 1.79e29,
                    MToughness = 6.41e28,
                    IPower = 4.31e29,
                    IToughness = 2.44e29,
                    OPower = 4.88e29,
                    Name = "The Breadverse"
                }
            },
            {
                36, new ZoneStats
                {
                    MPower = 5.77e29,
                    MToughness = 1.17e29,
                    IPower = 1.07e30,
                    IToughness = 7.59e29,
                    OPower = 1.518e30,
                    Name = "That 70's Zone"
                }
            },
            {
                37, new ZoneStats
                {
                    MPower = 1.55e30,
                    MToughness = 5.51e29,
                    IPower = 3.84e30,
                    IToughness = 2.33e30,
                    OPower = 4.66e30,
                    Name = "The Halloweenies"
                }
            },
            {
                39, new ZoneStats
                {
                    MPower = 5.24e31,
                    MToughness = 2.01e31,
                    IPower = 1.45e32,
                    IToughness = 8e31,
                    OPower = 1.6e32,
                    Name = "Construction Zone"
                }
            },
            {
                40, new ZoneStats
                {
                    MPower = 1.28e32,
                    MToughness = 3.2e31,
                    IPower = 3.5e32,
                    IToughness = 2.7e32,
                    OPower = 1.0e33,
                    Name = "Duck Duck Zone"
                }
            },
            {
                41, new ZoneStats
                {
                    MPower = 3.15e32,
                    MToughness = 8.42e31,
                    IPower = 8.94e32,
                    IToughness = 6.03e32,
                    OPower = 1.12e33,
                    Name = "The Nether Regions"
                }
            }
        };

    }

    internal class ZoneTarget
    {
        public int Zone { get; set; }
        public int FightType { get; set; }
    }

    internal class ZoneStats
    {
        public double MPower { get; set; }
        public double MToughness { get; set; }
        public double IPower { get; set; }
        public double IToughness { get; set; }
        public double OPower { get; set; }
        public string Name { get; set; }

        internal int FightType(float attack, float def)
        {
            //2 Means we can use fast combat
            //1 means we need to precast buffs
            //0 Means we cant do the zone
            if (attack > OPower)
            {
                return 2;
            }
            if (attack >= IPower && def >= IToughness)
            {
                return 2;
            }
            if (attack >= MPower && def >= MToughness)
            {
                return 1;
            }

            return 0;
        }
    }
}
