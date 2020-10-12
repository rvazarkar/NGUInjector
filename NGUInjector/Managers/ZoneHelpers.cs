using System;
using System.Linq;
using System.Reflection;
using static NGUInjector.Main;

namespace NGUInjector.Managers
{
    static class ZoneHelpers
    {
        internal static readonly int[] TitanZones = { 6, 8, 11, 14, 16, 19, 23, 26, 30, 34, 38, 40, 42 };
        internal static bool ZoneIsTitan(int zone)
        {
            return TitanZones.Contains(zone);
        }

        internal static TitanSpawn TitansSpawningSoon()
        {
            var result = new TitanSpawn
            {
                IsHighest = false,
                SpawningSoon = false
            };

            if (!Main.Character.buttons.adventure.IsInteractable())
            {
                result.SpawningSoon = false;
                return result;
            }

            if (Main.Character.bossID >= 58)
            {
                result.Merge(GetTitanSpawn(1));
            }

            if (Main.Character.bossID >= 66)
            {
                result.Merge(GetTitanSpawn(2));
            }

            if (Main.Character.bossID >= 82)
            {
                result.Merge(GetTitanSpawn(3));
            }

            if (Main.Character.bossID >= 100)
            {
                result.Merge(GetTitanSpawn(4));
            }

            if (Main.Character.bossID >= 116)
            {
                result.Merge(GetTitanSpawn(5));
            }

            if (Main.Character.bossID >= 132)
            {
                result.Merge(GetTitanSpawn(6));
            }

            if (Main.Character.effectiveBossID() >= 426)
            {
                result.Merge(GetTitanSpawn(7));
            }

            if (Main.Character.effectiveBossID() >= 467)
            {
                result.Merge(GetTitanSpawn(8));
            }

            if (Main.Character.effectiveBossID() >= 491)
            {
                result.Merge(GetTitanSpawn(9));
            }

            if (Main.Character.effectiveBossID() >= 727)
            {
                result.Merge(GetTitanSpawn(10));
            }

            if (Main.Character.effectiveBossID() >= 826)
            {
                result.Merge(GetTitanSpawn(11));
            }

            if (Main.Character.effectiveBossID() >= 848)
            {
                result.Merge(GetTitanSpawn(12));
            }

            return result;
        }

        private static TitanSpawn GetTitanSpawn(int bossId)
        {
            var result = new TitanSpawn
            {
                SpawningSoon = false,
                IsHighest = false
            };

            if (Test)
            {
                result.SpawningSoon = true;
            }

            if (bossId > Settings.HighestAKZone)
            {
                return result;
            }

            var controller = Main.Character.adventureController;
            var adventure = Main.Character.adventure;

            var spawnMethod = controller.GetType().GetMethod($"boss{bossId}SpawnTime",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var spawnTimeObj = spawnMethod?.Invoke(controller, null);
            if (spawnTimeObj == null)
                return result;
            var spawnTime = (float)spawnTimeObj;

            var spawnField = adventure.GetType().GetField($"boss{bossId}Spawn",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var spawnObj = spawnField?.GetValue(adventure);

            if (spawnObj == null)
                return result;
            var spawn = (PlayerTime)spawnObj;

            if (Math.Abs(spawnTime - spawn.totalseconds) < 20)
            {
                result.SpawningSoon = true;
            }
            else
            {
                return result;
            }

            if (ZoneHelpers.ZoneIsTitan(Settings.GoldZone))
            {
                var id = Array.IndexOf(ZoneHelpers.TitanZones, Settings.GoldZone) + 1;
                if (id == bossId)
                    result.IsHighest = true;
            }

            return result;
        }



    }
    public class TitanSpawn
    {
        internal bool SpawningSoon { get; set; }
        internal bool IsHighest { get; set; }

        internal void Merge(TitanSpawn other)
        {
            SpawningSoon = SpawningSoon || other.SpawningSoon;
            IsHighest = IsHighest || other.IsHighest;
        }
    }
}
