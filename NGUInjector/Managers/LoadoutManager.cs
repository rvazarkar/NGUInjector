using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using UnityEngine;
using static NGUInjector.Main;

namespace NGUInjector
{
    internal enum LockType
    {
        Titan,
        Yggdrasil,
        MoneyPit,
        Gold,
        None
    }
    internal static class LoadoutManager
    {
        private static int[] _savedLoadout;
        internal static LockType CurrentLock { get; set; }

        internal static bool CanSwap()
        {
            return CurrentLock == LockType.None;
        }

        internal static void AcquireLock(LockType type)
        {
            CurrentLock = type;
        }

        internal static void ReleaseLock()
        {
            CurrentLock = LockType.None;
        }

        internal static void RestoreGear()
        {
            Log($"Restoring original loadout");
            ChangeGear(_savedLoadout);
        }

        internal static void TryTitanSwap()
        {
            if (Settings.TitanLoadout.Length == 0)
                return;
            //Skip if we're currently locked for yggdrasil (although this generally shouldn't happen)
            if (!CanAcquireOrHasLock(LockType.Titan))
                return;

            //If we're currently holding the lock
            if (CurrentLock == LockType.Titan)
            {
                //If we haven't AKed yet, just return
                if (TitansSpawningSoon())
                    return;

                //Titans have been AKed, restore back to original gear
                RestoreGear();
                ReleaseLock();
                return;
            }

            //No lock currently, check if titans are spawning
            if (TitansSpawningSoon())
            {
                Log("Equipping Loadout for Titans");
                //Titans are spawning soon, grab a lock and swap
                AcquireLock(LockType.Titan);
                SaveCurrentLoadout();
                ChangeGear(Settings.TitanLoadout);
            }
        }

        internal static bool TryYggdrasilSwap()
        {
            if (!CanAcquireOrHasLock(LockType.Yggdrasil))
                return false;

            AcquireLock(LockType.Yggdrasil);
            SaveCurrentLoadout();
            ChangeGear(Settings.YggdrasilLoadout);
            return true;
        }

        internal static bool TryMoneyPitSwap()
        {
            if (!CanAcquireOrHasLock(LockType.MoneyPit))
                return false;

            AcquireLock(LockType.MoneyPit);
            SaveCurrentLoadout();
            ChangeGear(Settings.MoneyPitLoadout);
            return true;
        }

        internal static bool TryGoldDropSwap()
        {
            if (!CanAcquireOrHasLock(LockType.Gold))
                return false;

            //We already hold the lock so just return true
            if (CurrentLock == LockType.Gold)
            {
                return true;
            }

            AcquireLock(LockType.Gold);
            SaveCurrentLoadout();
            ChangeGear(Settings.GoldDropLoadout);

            return true;
        }

        private static bool CanAcquireOrHasLock(LockType requestor)
        {
            if (CurrentLock == requestor)
            {
                return true;
            }

            if (CurrentLock == LockType.None)
            {
                return true;
            }

            return false;
        }

        internal static void ChangeGear(int[] gearIds)
        {
            Log($"Received New Gear: {string.Join(",", gearIds.Select(x => x.ToString()).ToArray())}");
            var accSlots = new List<int>();
            var inv = Main.Character.inventory;
            var controller = Controller;
            var ci = inv.GetConvertedInventory(controller).ToArray();
            var weaponSlot = -5;
            Main.Character.removeMostEnergy();
            Main.Character.removeMostMagic();
            foreach (var itemId in gearIds)
            {
                var slot = FindItemSlot(ci, itemId);
                //We dont have the item. Dummy.
                if (slot == -1000)
                    continue;

                //Item is already equipped
                if (slot < 0)
                    continue;

                if (slot >= 10000)
                {
                    accSlots.Add(slot);
                    continue;
                }

                var type = inv.inventory[slot].type;

                inv.item2 = slot;
                switch (type)
                {
                    case part.Head:
                        inv.item1 = -1;
                        controller.swapHead();
                        controller.updateBonuses();
                        break;
                    case part.Chest:
                        inv.item1 = -2;
                        controller.swapChest();
                        controller.updateBonuses();
                        break;
                    case part.Legs:
                        inv.item1 = -3;
                        controller.swapLegs();
                        controller.updateBonuses();
                        break;
                    case part.Boots:
                        inv.item1 = -4;
                        controller.swapBoots();
                        controller.updateBonuses();
                        break;
                    case part.Weapon:
                        inv.item1 = weaponSlot;
                        if (weaponSlot == -5)
                        {
                            controller.swapWeapon();
                        }
                        else if (weaponSlot == -6)
                        {
                            if (controller.weapon2Unlocked())
                            {
                                controller.swapWeapon2();
                            }
                        }
                        else
                        {
                            break;
                        }
                        controller.updateBonuses();
                        weaponSlot--;
                        break;
                    case part.Accessory:
                        accSlots.Add(slot);
                        break;
                }
            }

            var usedSlots = accSlots.Where(x => x >= 10000).ToList();
            accSlots = accSlots.Where(x => x < 10000).ToList();

            foreach (var acc in accSlots)
            {
                for (var i = 10000; controller.accessoryID(i) < inv.accs.Count; i++)
                {
                    if (usedSlots.Contains(i))
                        continue;

                    inv.item1 = i;
                    inv.item2 = acc;
                    controller.swapAcc();
                    usedSlots.Add(i);
                    break;
                }
            }

            controller.updateBonuses();
            controller.updateInventory();
            Log($"Done equipping new gear");
        }

        private static int FindItemSlot(IEnumerable<ih> ci, int id)
        {
            var items = ci.Where(x => x.id == id).ToArray();
            if (items.Length != 0) return items.MaxItem().slot;
            var inv = Main.Character.inventory;
            if (inv.head.id == id)
            {
                return -1;
            }

            if (inv.chest.id == id)
            {
                return -2;
            }

            if (inv.legs.id == id)
            {
                return -3;
            }

            if (inv.boots.id == id)
            {
                return -4;
            }

            if (inv.weapon.id == id)
            {
                return -5;
            }

            if (Controller.weapon2Unlocked())
            {
                if (inv.weapon2.id == id)
                {
                    return -6;
                }
            }

            for (var i = 0; i < inv.accs.Count; i++)
            {
                if (inv.accs[i].id == id)
                {
                    return i + 10000;
                }
            }

            return -1000;
        }

        private static void SaveCurrentLoadout()
        {
            var inv = Main.Character.inventory;
            var loadout = new List<int>
            {
                inv.head.id,
                inv.boots.id,
                inv.chest.id,
                inv.legs.id,
                inv.weapon.id
            };


            if (Main.Character.inventoryController.weapon2Unlocked())
            {
                loadout.Add(inv.weapon2.id);
            }

            for (var id = 10000; Controller.accessoryID(id) < Main.Character.inventory.accs.Count; ++id)
            {
                var index = Controller.accessoryID(id);
                loadout.Add(Main.Character.inventory.accs[index].id);
            }
            _savedLoadout = loadout.ToArray();
            Log($"Saved Loadout {string.Join(",", _savedLoadout.Select(x => x.ToString()).ToArray())}");
        }
        internal static bool TitansSpawningSoon()
        {
            if (!Main.Character.buttons.adventure.IsInteractable())
                return false;

            var ak = Settings.HighestAKZone;
            var i = 0;
            var a = Main.Character.adventure;
            var ac = Main.Character.adventureController;

            if (i == ak)
                return false;
            if (Main.Character.bossID >= 58 || Main.Character.achievements.achievementComplete[128])
            {
                if (Math.Abs(ac.boss1SpawnTime() - a.boss1Spawn.totalseconds) < 20)
                {
                    return true;
                }
            }

            i++;
            if (i == ak)
                return false;
            if (Main.Character.bossID >= 66 || Main.Character.achievements.achievementComplete[129])
            {
                if (Math.Abs(ac.boss2SpawnTime() - a.boss2Spawn.totalseconds) < 20)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (Main.Character.bossID >= 82 || Main.Character.bestiary.enemies[304].kills > 0)
            {
                if (Math.Abs(ac.boss3SpawnTime() - a.boss3Spawn.totalseconds) < 20)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (Main.Character.bossID >= 100 || Main.Character.achievements.achievementComplete[130])
            {
                if (Math.Abs(ac.boss4SpawnTime() - a.boss4Spawn.totalseconds) < 20)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (Main.Character.bossID >= 116 || Main.Character.achievements.achievementComplete[145])
            {
                if (Math.Abs(ac.boss5SpawnTime() - a.boss5Spawn.totalseconds) < 20)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (Main.Character.bossID >= 132 || Main.Character.adventure.boss6Kills >= 1)
            {
                if (Math.Abs(ac.boss6SpawnTime() - a.boss6Spawn.totalseconds) < 20)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (Main.Character.effectiveBossID() >= 426 || Main.Character.adventure.boss7Kills >= 1)
            {
                if (Math.Abs(ac.boss7SpawnTime() - a.boss7Spawn.totalseconds) < 20)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (Main.Character.effectiveBossID() >= 467 || Main.Character.adventure.boss8Kills >= 1)
            {
                if (Math.Abs(ac.boss8SpawnTime() - a.boss8Spawn.totalseconds) < 20)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (Main.Character.effectiveBossID() >= 491 || Main.Character.adventure.boss9Kills >= 1)
            {
                if (Math.Abs(ac.boss9SpawnTime() - a.boss9Spawn.totalseconds) < 20)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (Main.Character.effectiveBossID() >= 727 || Main.Character.adventure.boss10Kills >= 1)
            {
                if (Math.Abs(ac.boss10SpawnTime() - a.boss10Spawn.totalseconds) < 20)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (Main.Character.effectiveBossID() >= 826 || Main.Character.adventure.boss11Kills >= 1)
            {
                if (Math.Abs(ac.boss11SpawnTime() - a.boss11Spawn.totalseconds) < 20)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (Main.Character.effectiveBossID() >= 848 || Main.Character.adventure.boss12Kills >= 1)
            {
                if (Math.Abs(ac.boss12SpawnTime() - a.boss12Spawn.totalseconds) < 20)
                {
                    return true;
                }
            }

            return false;
        }

        //private static float GetSeedGain(Equipment e)
        //{
        //    var amount =
        //        typeof(ItemController).GetMethod("effectBonus", BindingFlags.NonPublic | BindingFlags.Instance);
        //    if (e.spec1Type == specType.Seeds)
        //    {
        //        var p = new object[] { e.spec1Cur, e.spec1Type };
        //        return (float)amount?.Invoke(Main.Controller, p);
        //    }
        //    if (e.spec2Type == specType.Seeds)
        //    {
        //        var p = new object[] { e.spec2Cur, e.spec2Type };
        //        return (float)amount?.Invoke(Main.Controller, p);
        //    }
        //    if (e.spec3Type == specType.Seeds)
        //    {
        //        var p = new object[] { e.spec3Cur, e.spec3Type };
        //        return (float)amount?.Invoke(Main.Controller, p);
        //    }

        //    return 0;
        //}
    }
}
