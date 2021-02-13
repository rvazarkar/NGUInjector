using System;
using System.Linq;
using System.Reflection;

namespace NGUInjector.Managers
{
    static class ZoneHelpers
    {
        internal static readonly int[] TitanZones = { 6, 8, 11, 14, 16, 19, 23, 26, 30, 34, 38, 42 };

        internal static bool ZoneIsTitan(int zone)
        {
            return TitanZones.Contains(zone);
        }

        internal static TitanSpawn TitansSpawningSoon()
        {
            var result = new TitanSpawn();

            if (!Main.Character.buttons.adventure.IsInteractable())
                return result;
            for (var i = 0; i < TitanZones.Length; i++)
            {
                result.Merge(GetTitanSpawn(i));
            }
            return result;
        }

        internal static bool TitanSpawningSoon(int boss)
        {
            return Main.Character.buttons.adventure.IsInteractable() && CheckTitanSpawnTime(boss);
        }

        private static TitanSpawn GetTitanSpawn(int bossId)
        {
            var result = new TitanSpawn();

            if (TitanZones[bossId] > GetMaxReachableZone(true))
                return result;

            if (!CheckTitanSpawnTime(bossId)) return result;

            result.SpawningSoon = Main.Settings.SwapTitanLoadouts && Main.Settings.TitanSwapTargets[bossId];
            // Run money once for each boss
            result.RunMoneyLoadout = Main.Settings.ManageGoldLoadouts && Main.Settings.TitanGoldTargets[bossId] && !Main.Settings.TitanMoneyDone[bossId];

            if (!result.RunMoneyLoadout) return result;
            Main.Log($"Running money loadout for {bossId}");
            var temp = Main.Settings.TitanMoneyDone.ToArray();
            temp[bossId] = true;
            Main.Settings.TitanMoneyDone = temp;

            return result;
        }

        private static bool CheckTitanSpawnTime(int bossId)
        {
            if (Main.Test) return true;
            var controller = Main.Character.adventureController;
            var adventure = Main.Character.adventure;

            var spawnMethod = controller.GetType().GetMethod($"boss{bossId + 1}SpawnTime",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var spawnTimeObj = spawnMethod?.Invoke(controller, null);
            if (spawnTimeObj == null)
                return false;
            var spawnTime = (float) spawnTimeObj;

            var spawnField = adventure.GetType().GetField($"boss{bossId + 1}Spawn",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var spawnObj = spawnField?.GetValue(adventure);

            if (spawnObj == null)
                return false;

            var spawn = (PlayerTime) spawnObj;
            return Math.Abs(spawnTime - spawn.totalseconds) < 20;
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

        internal static void OptimizeITOPOD()
        {
            if (!Main.Settings.OptimizeITOPODFloor) return;
            if (Main.Character.arbitrary.boughtLazyITOPOD && Main.Character.arbitrary.lazyITOPODOn) return;
            if (Main.Character.adventure.zone < 1000) return;
            var controller = Main.Character.adventureController;
            var level = controller.itopodLevel;
            var optimal = CalculateBestItopodLevel();
            if (level == optimal) return; // we are on optimal floor
            var highestOpen = Main.Character.adventure.highestItopodLevel;
            var climbing = (level < optimal && level >= highestOpen - 1);
            controller.itopodStartInput.text = optimal.ToString();
            if (climbing)
                optimal++;
            controller.itopodEndInput.text = optimal.ToString();
            controller.verifyItopodInputs();
            if (!climbing)
                controller.zoneSelector.changeZone(1000);
        }

        internal static int CalculateBestItopodLevel()
        {
            var c = Main.Character;
            var num1 = c.totalAdvAttack() / 765f * (Main.Settings.ITOPODCombatMode == 1 || c.training.attackTraining[1] == 0 ? c.idleAttackPower() : c.regAttackPower());
            if (c.totalAdvAttack() < 700.0)
                return 0;
            var num2 = Convert.ToInt32(Math.Floor(Math.Log(num1, 1.05)));
            if (num2 < 1)
                return 1;
            var maxLevel = c.adventureController.maxItopodLevel();
            if (num2 > maxLevel)
                num2 = maxLevel;
            return num2;
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
