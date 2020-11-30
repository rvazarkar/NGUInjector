using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NGUInjector.AllocationProfiles;
using NGUInjector.Managers;
using UnityEngine;
using static NGUInjector.Main;

namespace NGUInjector
{
    public static class Extensions
    {
        public static MethodInfo GetPrivateMethod(this Type t, string method)
        {
            return t.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.NonPublic);
        }


        public static ih MaxItem(this IEnumerable<ih> items)
        {
            return items.Aggregate(
                new { max = int.MinValue, t = (ih)null, b = decimal.MaxValue },
                (state, el) =>
                {
                    var current = el.locked ? el.level + 101 : el.level;
                    if (current > state.max)
                    {
                        return new {max = current, t = el, b = el.equipment.GetNeededBoosts().Total()};
                    }
                    
                    if (current == state.max)
                    {
                        return el.equipment.GetNeededBoosts().Total() > state.b ? new { max = current, t = el, b = el.equipment.GetNeededBoosts().Total() } : state;
                    }

                    return state;
                }).t;
        }

        public static ih GetInventoryHelper(this Equipment equip, int slot)
        {
            return new ih
            {
                level = equip.level,
                equipment = equip,
                id = equip.id,
                locked = !equip.removable,
                name = Controller.itemInfo.itemName[equip.id],
                slot = slot
            };
        }

        public static IEnumerable<ih> GetConvertedInventory(this Inventory inv)
        {
            return inv.inventory.Select((x, i) =>
            {
                var c = x.GetInventoryHelper(i);
                return c;
            }).Where(x => x.id != 0);
        }

        public static bool HasBoosts(this Inventory inv)
        {
            return inv.inventory.Any(x => x.id < 40 && x.id > 0);
        }

        public static IEnumerable<ih> GetConvertedEquips(this Inventory inv)
        {
            var list = new List<ih>
            {
                inv.head.GetInventoryHelper(-1), inv.chest.GetInventoryHelper(-2), inv.legs.GetInventoryHelper(-3),
                inv.boots.GetInventoryHelper(-4), inv.weapon.GetInventoryHelper(-5)
            };

            if (Controller.weapon2Unlocked())
            {
                list.Add(inv.weapon2.GetInventoryHelper(-6));
            }

            list.AddRange(inv.accs.Select((t, i) => t.GetInventoryHelper(i + 10000)));

            list.RemoveAll(x => x.id == 0);
            return list;
        }

        public static T GetPV<T>(this EnemyAI ai, string val)
        {
            var type = ai.GetType().GetField(val,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)type?.GetValue(ai);
        }

        public static BoostsNeeded GetNeededBoosts(this Equipment eq)
        {
            var n = new BoostsNeeded();

            if (eq.capAttack != 0.0)
                n.Power += CalcCap(eq.capAttack, eq.level) - (decimal)eq.curAttack;

            if (eq.capDefense != 0.0)
                n.Toughness += CalcCap(eq.capDefense, eq.level) - (decimal)eq.curDefense;

            if (Settings.SpecialBoostBlacklist.Contains(eq.id))
                return n;

            if (eq.spec1Type != specType.None)
                n.Special += CalcCap(eq.spec1Cap, eq.level) - (decimal)eq.spec1Cur;

            if (eq.spec2Type != specType.None)
                n.Special += CalcCap(eq.spec2Cap, eq.level) - (decimal)eq.spec2Cur;

            if (eq.spec3Type != specType.None)
                n.Special += CalcCap(eq.spec3Cap, eq.level) - (decimal)eq.spec3Cur;

            return n;
        }

        public static float AugTimeLeftEnergy(this AugmentController aug, long energy)
        {
            return (float)((1.0 - (double)aug.character.augments.augs[aug.id].augProgress) / (double)aug.getAugProgressPerTick(energy) / 50.0);
        }

        public static float AugTimeLeftEnergyMax(this AugmentController aug, long energy)
        {
            return (float)(1.0 / (double)aug.getAugProgressPerTick(energy) / 50.0);
        }

        private static decimal CalcCap(float cap, float level)
        {
            return (decimal)Mathf.Floor(cap * (float)(1.0 + level / 100.0));
        }

        internal static void DoAllocations(this CustomAllocation allocation)
        {
            if (!Settings.GlobalEnabled)
                return;

            if (allocation.IsAllocationRunning)
                return;

            var originalInput = Main.Character.energyMagicPanel.energyMagicInput;

            allocation.IsAllocationRunning = true;

            if (Settings.ManageNGUDiff)
                allocation.SwapNGUDiff();
            if (Settings.ManageGear)
                allocation.EquipGear();
            if (Settings.ManageEnergy)
                allocation.AllocateEnergy();
            if (Settings.ManageMagic)
                allocation.AllocateMagic();
            if (Settings.ManageR3)
                allocation.AllocateR3();

            if (Settings.ManageDiggers && Main.Character.buttons.diggers.interactable)
            {
                allocation.EquipDiggers();
                DiggerManager.RecapDiggers();
            }

            if (Settings.ManageWandoos && Main.Character.buttons.wandoos.interactable)
                allocation.SwapOS();

            Main.Character.energyMagicPanel.energyRequested.text = originalInput.ToString();
            Main.Character.energyMagicPanel.validateInput();
            
            allocation.IsAllocationRunning = false;
        }

        //Function from https://www.dotnetperls.com/pretty-date
        internal static string GetPrettyDate(this DateTime d)
        {
            // 1.
            // Get time span elapsed since the date.
            var s = DateTime.Now.Subtract(d);

            // 2.
            // Get total number of days elapsed.
            var dayDiff = (int)s.TotalDays;

            // 3.
            // Get total number of seconds elapsed.
            var secDiff = (int)s.TotalSeconds;

            // 4.
            // Don't allow out of range values.
            if (dayDiff < 0 || dayDiff >= 31)
                return null;

            // 5.
            // Handle same-day times.
            if (dayDiff == 0)
            {
                // A.
                // Less than one minute ago.
                if (secDiff < 60)
                    return "just now";
                // B.
                // Less than 2 minutes ago.
                if (secDiff < 120)
                    return "1 minute ago";
                // C.
                // Less than one hour ago.
                if (secDiff < 3600)
                    return $"{Math.Floor((double) secDiff / 60)} minutes ago";
                // D.
                // Less than 2 hours ago.
                if (secDiff < 7200)
                    return "1 hour ago";
                // E.
                // Less than one day ago.
                if (secDiff < 86400)
                    return $"{Math.Floor((double) secDiff / 3600)} hours ago";
            }
            // 6.
            // Handle previous days.
            if (dayDiff == 1)
                return "yesterday";
            if (dayDiff < 7)
                return $"{dayDiff} days ago";
            if (dayDiff < 31)
                return $"{Math.Ceiling((double) dayDiff / 7)} weeks ago";
            return null;
        }
    }
}
