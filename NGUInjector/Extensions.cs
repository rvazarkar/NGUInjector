using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NGUInjector.Managers;
using UnityEngine;
using static NGUInjector.Main;

namespace NGUInjector
{
    public static class Extensions
    {
        public static ih MaxItem(this IEnumerable<ih> items)
        {
            return items.Aggregate(
                new { max = int.MinValue, t = (ih)null },
                (state, el) =>
                {
                    var current = el.locked ? el.level + 101 : el.level;
                    return current > state.max ? new { max = current, t = el } : state;
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

        public static IEnumerable<ih> GetConvertedEquips(this Inventory inv)
        {
            var list = new List<ih>
            {
                inv.head.GetInventoryHelper(-1), inv.chest.GetInventoryHelper(-2), inv.legs.GetInventoryHelper(-3),
                inv.boots.GetInventoryHelper(-4), inv.weapon.GetInventoryHelper(-5)
            };

            if (Controller.weapon2Unlocked())
            {
                list.Add(inv.weapon.GetInventoryHelper(-6));
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

            if (eq.spec1Type != specType.None)
                n.Special += CalcCap(eq.spec1Cap, eq.level) - (decimal)eq.spec1Cur;

            if (eq.spec2Type != specType.None)
                n.Special += CalcCap(eq.spec2Cap, eq.level) - (decimal)eq.spec2Cur;

            if (eq.spec3Type != specType.None)
                n.Special += CalcCap(eq.spec3Cap, eq.level) - (decimal)eq.spec3Cur;

            return n;
        }

        private static decimal CalcCap(float cap, float level)
        {
            return (decimal)Mathf.Floor(cap * (float)(1.0 + level / 100.0));
        }
    }
}
