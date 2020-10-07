using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static NGUInjector.Main;

namespace NGUInjector.Managers
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
        private static int[] _tempLoadout;
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
                if (TitansSpawningSoon().SpawningSoon)
                    return;

                //Titans have been AKed, restore back to original gear
                RestoreGear();
                ReleaseLock();
                return;
            }

            //No lock currently, check if titans are spawning
            var ts = TitansSpawningSoon();
            if (ts.SpawningSoon)
            {
                Log("Equipping Loadout for Titans");
                
                //Titans are spawning soon, grab a lock and swap
                AcquireLock(LockType.Titan);
                SaveCurrentLoadout();

                if (Settings.NextGoldSwap && ts.IsHighest)
                {
                    Log("Equipping Gold Drop Loadout");
                    ChangeGear(Settings.GoldDropLoadout);
                    Settings.NextGoldSwap = false;
                    settingsForm.UpdateGoldLoadout(Settings.NextGoldSwap);
                }
                else
                {
                    Log("Equipping Titan Loadout");
                    ChangeGear(Settings.TitanLoadout);
                }
            }
        }

        internal static bool TryYggdrasilSwap()
        {
            if (!CanAcquireOrHasLock(LockType.Yggdrasil))
                return false;

            Log("Equipping Yggdrasil Loadout");
            AcquireLock(LockType.Yggdrasil);
            SaveCurrentLoadout();
            ChangeGear(Settings.YggdrasilLoadout);
            return true;
        }

        internal static bool TryMoneyPitSwap()
        {
            if (!CanAcquireOrHasLock(LockType.MoneyPit))
                return false;

            Log("Equipping Money Pit");
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

            Log("Equipping Gold Loadout");
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
            var weaponSlot = -5;
            var accSlot = 10000;
            var controller = Controller;

            Main.Character.removeMostEnergy();
            Main.Character.removeMostMagic();
            Main.Character.removeAllRes3();

            try
            {
                foreach (var itemId in gearIds)
                {
                    var inv = Main.Character.inventory;

                    var equip = FindItemSlot(itemId);

                    if (equip == null)
                        continue;

                    var type = equip.equipment.type;

                    inv.item2 = equip.slot;
                    switch (type)
                    {
                        case part.Head:
                            inv.item1 = -1;
                            controller.swapHead();
                            break;
                        case part.Chest:
                            inv.item1 = -2;
                            controller.swapChest();
                            break;
                        case part.Legs:
                            inv.item1 = -3;
                            controller.swapLegs();
                            break;
                        case part.Boots:
                            inv.item1 = -4;
                            controller.swapBoots();
                            break;
                        case part.Weapon:
                            if (weaponSlot == -5)
                            {
                                inv.item1 = -5;
                                controller.swapWeapon();
                            }
                            else if (weaponSlot == -6 && controller.weapon2Unlocked())
                            {
                                inv.item1 = -6;
                                controller.swapWeapon2();
                            }

                            weaponSlot--;
                            break;
                        case part.Accessory:
                            if (controller.accessoryID(accSlot) < controller.accessorySpaces() && accSlot != equip.slot)
                            {
                                inv.item1 = accSlot;
                                controller.swapAcc();
                                accSlot++;
                            }

                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Log(e.Message);
                Log(e.StackTrace);
            }
            

            controller.updateBonuses();
            controller.updateInventory();
            Log("Finished equipping gear");
        }

        private static ih FindItemSlot(int id)
        {
            var inv = Main.Character.inventory;
            if (inv.head.id == id)
            {
                return inv.head.GetInventoryHelper(-1);
            }

            if (inv.chest.id == id)
            {
                return inv.chest.GetInventoryHelper(-2);
            }

            if (inv.legs.id == id)
            {
                return inv.legs.GetInventoryHelper(-3);
            }

            if (inv.boots.id == id)
            {
                return inv.boots.GetInventoryHelper(-4);
            }

            if (inv.weapon.id == id)
            {
                return inv.weapon.GetInventoryHelper(-5);
            }

            if (Controller.weapon2Unlocked())
            {
                if (inv.weapon2.id == id)
                {
                    return inv.weapon2.GetInventoryHelper(-6);
                }
            }

            for (var i = 0; i < inv.accs.Count; i++)
            {
                if (inv.accs[i].id == id)
                {
                    return inv.accs[i].GetInventoryHelper(i + 10000);
                }
            }

            var items = Main.Character.inventory.GetConvertedInventory()
                .Where(x => x.id == id && x.equipment.isEquipment()).ToArray();
            if (items.Length != 0) return items.MaxItem();

            return null;
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

        internal static void SaveTempLoadout()
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
            _tempLoadout = loadout.ToArray();
            Log($"Saved Loadout {string.Join(",", _tempLoadout.Select(x => x.ToString()).ToArray())}");
        }

        internal static void RestoreTempLoadout()
        {
            ChangeGear(_tempLoadout);
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

            if (ZoneIsTitan(Settings.GoldZone))
            {
                var id = Array.IndexOf(TitanZones, Settings.GoldZone) + 1;
                if (id == bossId)
                    result.IsHighest = true;
            }

            return result;
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
