using System;
using System.Collections.Generic;
using System.Linq;

namespace NGUInjector.Managers
{
    internal static class DiggerManager
    {
        private static int[] _savedDiggers;
        internal static LockType CurrentLock { get; set; }
        private static readonly int[] TitanDiggers = { 0, 3, 8, 11 };
        private static readonly int[] YggDiggers = {8, 11};

        internal static bool CanSwap()
        {
            return CurrentLock == LockType.None;
        }

        internal static void TryTitanSwap()
        {
            var ts = LoadoutManager.TitansSpawningSoon();
            if (CurrentLock == LockType.Titan)
            {
                if (ts.SpawningSoon)
                    return;

                RestoreDiggers();
                ReleaseLock();
                return;
            }

            if (ts.SpawningSoon)
            {
                CurrentLock = LockType.Titan;
                SaveDiggers();
                EquipDiggers(TitanDiggers);
            }
        }

        internal static bool TryYggSwap()
        {
            if (CurrentLock == LockType.Titan)
                return false;

            CurrentLock = LockType.Yggdrasil;
            SaveDiggers();
            EquipDiggers(YggDiggers);
            return true;
        }

        internal static void ReleaseLock()
        {
            CurrentLock = LockType.None;
        }

        internal static void SaveDiggers()
        {
            var temp = new List<int>();
            for (var i = 0; i < Main.Character.diggers.diggers.Count; i++)
            {
                if (Main.Character.diggers.diggers[i].active)
                {
                    temp.Add(i);
                }
                    
            }

            _savedDiggers = temp.ToArray();
        }

        internal static void EquipDiggers(int[] diggers)
        {
            Main.Log($"Equipping Diggers: {string.Join(",", diggers.Select(x => x.ToString()).ToArray())}");
            Main.Character.allDiggers.clearAllActiveDiggers();
            var sorted = diggers.OrderByDescending(x => x).ToArray();
            for (var i = 0; i < sorted.Length; i++)
            {
                if (Main.Character.diggers.diggers[i].maxLevel <= 0)
                    continue;
                Main.Character.allDiggers.setLevelMaxAffordable(sorted[i]);
            }
        }

        internal static void RecapDiggers()
        {
            for (var i = 0; i < Main.Character.diggers.diggers.Count; i++)
            {
                if (Main.Character.diggers.diggers[i].active)
                {
                    SetLevelMaxAffordable(i);
                }
            }
        }

        private static void SetLevelMaxAffordable(int id)
        {
            if (id < 0 || id > Main.Character.diggers.diggers.Count)
                return;
            var curLevel = Main.Character.diggers.diggers[id].curLevel;
            Main.Character.diggers.diggers[id].curLevel = 0L;
            if (Main.Character.goldPerSecond() < Main.Character.allDiggers.drain(id, 1, true))
            {
                Main.Character.diggers.diggers[id].curLevel = curLevel;
            }
            else
            {
                var num1 = Main.Character.goldPerSecond();
                var num2 = Main.Character.allDiggers.baseGPSDrain[id];
                var a = Main.Character.allDiggers.gpsGrowthRate[id];
                var num3 = Math.Min((long)(Math.Log(num1 / num2, Math.E) / Math.Log(a, Math.E)), Main.Character.diggers.diggers[id].maxLevel);
                if (num3 < 0L)
                    num3 = 0L;
                if (num3 > Main.Character.diggers.diggers[id].maxLevel)
                    num3 = Main.Character.diggers.diggers[id].maxLevel;
                Main.Character.diggers.diggers[id].curLevel = num3;
                if (Main.Character.diggers.diggers[id].curLevel == 0L && Main.Character.diggers.diggers[id].active)
                {
                    var num4 = 0;
                    while (num4 < Main.Character.diggers.activeDiggers.Count)
                        ++num4;
                    Main.Character.diggers.diggers[id].active = false;
                    Main.Character.diggers.activeDiggers.RemoveAt(Main.Character.diggers.activeDiggers.IndexOf(id));
                }
                if (Main.Character.grossGoldPerSecond() < Main.Character.allDiggers.totalGPSDrain())
                {
                    Main.Character.diggers.diggers[id].curLevel = curLevel;
                }
                else if (!Main.Character.diggers.diggers[id].active && Main.Character.diggers.diggers[id].curLevel > 0L && Main.Character.diggers.activeDiggers.Count < Main.Character.allDiggers.maxDiggerSlots())
                    Main.Character.allDiggers.activateDigger(id);

                Main.Character.allDiggers.refreshMenu();
            }
        }

        internal static void RestoreDiggers()
        {
            Main.Character.allDiggers.clearAllActiveDiggers();
            EquipDiggers(_savedDiggers);
        }
    }
}
