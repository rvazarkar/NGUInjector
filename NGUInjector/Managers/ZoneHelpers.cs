using System;
using System.Linq;
using System.Reflection;

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
            var result = new TitanSpawn();

            if (!Main.Character.buttons.adventure.IsInteractable())
            {
                return result;
            }
            for (var i = 0; i < TitanZones.Length; i++)
            {
                result.Merge(GetTitanSpawn(i));
            }
            return result;
        }

        private static TitanSpawn GetTitanSpawn(int bossId)
        {
            var result = new TitanSpawn();

            if (Main.Test)
            {
                result.SpawningSoon = true;
            }

            if (bossId > Main.Settings.HighestAKZone)
                return result;
            if (TitanZones[bossId] > GetMaxReachableZone(true))
                return result;

            var controller = Main.Character.adventureController;
            var adventure = Main.Character.adventure;

            var spawnMethod = controller.GetType().GetMethod($"boss{bossId + 1}SpawnTime",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var spawnTimeObj = spawnMethod?.Invoke(controller, null);
            if (spawnTimeObj == null)
                return result;
            var spawnTime = (float)spawnTimeObj;

            var spawnField = adventure.GetType().GetField($"boss{bossId + 1}Spawn",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var spawnObj = spawnField?.GetValue(adventure);

            if (spawnObj == null)
                return result;
            var spawn = (PlayerTime)spawnObj;

            if (Math.Abs(spawnTime - spawn.totalseconds) < 20)
            {
                result.SpawningSoon = true;
                // Run money once for each boss
                result.RunMoneyLoadout = Main.Settings.TitanGoldTargets[bossId] && !Main.Settings.TitanMoneyDone[bossId];
                var temp = Main.Settings.TitanMoneyDone;
                temp[bossId] = true;
                Main.Settings.TitanMoneyDone = temp;
            }

            return result;
        }

        internal static int GetMaxReachableZone(bool includingTitans)
        {
            for (var i = Main.Character.adventureController.zoneDropdown.options.Count - 2; i >= 0; i--)
            {
                if (!ZoneIsTitan(i))
                    return i;
                if (includingTitans)
                    return i;
            }
            return 0;
        }

    }

    

    public class TitanSpawn
    {
        internal bool SpawningSoon { get; set; }
        internal bool RunMoneyLoadout { get; set; }

        internal void Merge(TitanSpawn other)
        {
            SpawningSoon = SpawningSoon || other.SpawningSoon;
            RunMoneyLoadout = RunMoneyLoadout || other.RunMoneyLoadout;
        }
    }
}
