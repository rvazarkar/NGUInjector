using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NGUInjector
{
    internal static class DiggerManager
    {
        private static int[] _savedDiggers;
        internal static LockType CurrentLock { get; set; }
        private static int[] _titanDiggers = { 0, 3, 11 };

        internal static bool CanSwap()
        {
            return CurrentLock == LockType.None;
        }

        internal static void TryTitanSwap()
        {
            if (CurrentLock == LockType.Titan)
            {
                if (LoadoutManager.TitansSpawningSoon())
                    return;

                RestoreDiggers();
                ReleaseLock();
                return;
            }

            if (LoadoutManager.TitansSpawningSoon())
            {
                CurrentLock = LockType.Titan;
                SaveDiggers();
                EquipDiggers(_titanDiggers);
            }
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
            Main.OutputWriter.WriteLine($"Equipping Diggers: {string.Join(",", diggers.Select(x => x.ToString()).ToArray())}");
            Main.Character.allDiggers.clearAllActiveDiggers();
            var sorted = diggers.OrderByDescending(x => x).ToArray();
            for (var i = 0; i < sorted.Length; i++)
            {
                if (Main.Character.diggers.diggers[i].maxLevel <= 0)
                    continue;
                Main.Character.allDiggers.setLevelMaxAffordable(sorted[i]);
            }
        }

        internal static void RestoreDiggers()
        {
            Main.Character.allDiggers.clearAllActiveDiggers();
            EquipDiggers(_savedDiggers);
        }
    }
}
