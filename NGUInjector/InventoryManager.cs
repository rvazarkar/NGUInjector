using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NGUInjector
{

    class InventoryManager
    {
        internal enum LockType
        {
            Yggdrasil,
            Titan,
            None
        }

        private readonly Character _character;
        private readonly StreamWriter _outputWriter;
        private readonly InventoryController _controller;

        private readonly int[] _pendants = { 53, 76, 94, 142, 170, 229, 295, 388, 430, 504 };
        private readonly int[] _lootys = { 67, 128, 169, 230, 296, 389, 431, 505 };
        private readonly int[] _wandoos = {66, 169};

        private LockType _lock;

        //Wandoos 98, Giant Seed, Wandoos XL, Lonely Flubber, Wanderer's Cane
        private readonly int[] _filterExcludes = { 66, 92, 163, 120, 154 };
        public InventoryManager()
        {
            _character = Main.Character;
            _outputWriter = Main.OutputWriter;
            _controller = Main.Controller;
            _lock = LockType.None;
        }

        void SaveCurrentLoadout()
        {
            //var inv = _character.inventory;
            //var l = new SavedLoadout
            //{
            //    Head = inv.head.id,
            //    Boots =  inv.boots.id,
            //    Chest = inv.chest.id,
            //    Legs = inv.legs.id,
            //    Weapon = inv.weapon.id,
            //    Accessories =  new List<int>()
            //};

            //if (_character.inventoryController.weapon2Unlocked())
            //{
            //    l.Weapon2 = inv.weapon2.id;
            //}

            //for (var id = 10000; _controller.accessoryID(id) < _character.inventory.accs.Count; ++id)
            //{
            //    var index = _controller.accessoryID(id);
            //    l.Accessories.Add(_character.inventory.accs[index].id);
            //}
            //_originalLoadout = l;
            _character.inventoryController.assignCurrentEquipToLoadout(0);
        }

        int FindItemSlot(int id)
        {
            var ci = Main.Character.inventory.GetConvertedInventory(Main.Controller).ToArray();
            
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

            if (_controller.weapon2Unlocked())
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

        internal void TestSwap()
        {
            try
            {
                if (_lock == LockType.None)
                {
                    SaveCurrentLoadout();
                    _lock = LockType.Titan;
                    _controller.equipLoadout(1);
                }
                else
                {
                    RestoreOriginalLoadout();
                }
            }
            catch (Exception e)
            {
                _outputWriter.WriteLine(e);
                _outputWriter.Flush();
            }
        }

        internal void RestoreOriginalLoadout()
        {

            _lock = LockType.None;
            _character.inventoryController.equipLoadout(0);
            //var inv = _controller.character.inventory;
            //_character.removeMostEnergy();
            //_character.removeMostMagic();

            //inv.item1 = -1;
            //inv.item2 = FindItemSlot(_originalLoadout.Head);
            //if (inv.item1 != inv.item2)
            //{
            //    _outputWriter.WriteLine($"Swapping {inv.item1} with {inv.item2}");
            //    _controller.swapHead();
            //    _controller.updateBonuses();
            //}
            

            //inv.item1 = -2;
            //inv.item2 = FindItemSlot(_originalLoadout.Chest);
            //if (inv.item1 != inv.item2)
            //{
            //    _outputWriter.WriteLine($"Swapping {inv.item1} with {inv.item2}");
            //    _controller.swapChest();
            //    _controller.updateBonuses();
            //}

            //inv.item1 = -3;
            //inv.item2 = FindItemSlot(_originalLoadout.Legs);
            //if (inv.item1 != inv.item2)
            //{
            //    _outputWriter.WriteLine($"Swapping {inv.item1} with {inv.item2}");
            //    _controller.swapLegs();
            //    _controller.updateBonuses();
            //}

            //inv.item1 = -4;
            //inv.item2 = FindItemSlot(_originalLoadout.Boots);
            //if (inv.item1 != inv.item2)
            //{
            //    _outputWriter.WriteLine($"Swapping {inv.item1} with {inv.item2}");
            //    _controller.swapBoots();
            //    _controller.updateBonuses();
            //}

            //inv.item1 = -5;
            //inv.item2 = FindItemSlot(_originalLoadout.Weapon);
            //if (inv.item1 != inv.item2)
            //{
            //    _outputWriter.WriteLine($"Swapping {inv.item1} with {inv.item2}");
            //    _controller.swapWeapon();
            //    _controller.updateBonuses();
            //}

            //if (_controller.weapon2Unlocked())
            //{
            //    inv.item1 = -6;
            //    inv.item2 = FindItemSlot(_originalLoadout.Weapon2);
            //    if (inv.item1 != inv.item2)
            //    {
            //        _outputWriter.WriteLine($"Swapping {inv.item1} with {inv.item2}");
            //        _controller.swapWeapon2();
            //        _controller.updateBonuses();
            //    }
            //}

            //for (var i = 0; i < _originalLoadout.Accessories.Count; i++)
            //{
            //    inv.item1 = i + 10000;
            //    inv.item2 = FindItemSlot(_originalLoadout.Accessories[i]);

            //    if (inv.item1 != inv.item2)
            //    {
            //        _outputWriter.WriteLine($"Swapping {inv.item1} with {inv.item2}");
            //        _controller.swapAcc();
            //        _controller.updateBonuses();
            //    }
            //}
            //_controller.updateInventory();
            //_outputWriter.Flush();
        }

        bool TitansSpawningSoon()
        {
            if (!_character.buttons.adventure.IsInteractable())
                return false;

            var ak = Main.HighestAk;
            var i = 0;
            var a = _character.adventure;
            var ac = _character.adventureController;

            if (i == ak)
                return false;
            if (_character.bossID >= 58 || _character.achievements.achievementComplete[128])
            {
                if (Math.Abs(ac.boss1SpawnTime() - a.boss1Spawn.totalseconds) < 30)
                {
                    return true;
                }
            }

            i++;
            if (i == ak)
                return false;
            if (_character.bossID >= 66 || _character.achievements.achievementComplete[129])
            {
                if (Math.Abs(ac.boss2SpawnTime() - a.boss2Spawn.totalseconds) < 30)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (_character.bossID >= 82 || _character.bestiary.enemies[304].kills > 0)
            {
                if (Math.Abs(ac.boss3SpawnTime() - a.boss3Spawn.totalseconds) < 30)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (_character.bossID >= 100 || _character.achievements.achievementComplete[130])
            {
                if (Math.Abs(ac.boss4SpawnTime() - a.boss4Spawn.totalseconds) < 30)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (_character.bossID >= 116 || _character.achievements.achievementComplete[145])
            {
                if (Math.Abs(ac.boss5SpawnTime() - a.boss5Spawn.totalseconds) < 30)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (_character.bossID >= 132 || _character.adventure.boss6Kills >= 1)
            {
                if (Math.Abs(ac.boss6SpawnTime() - a.boss6Spawn.totalseconds) < 30)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (_character.effectiveBossID() >= 426 || _character.adventure.boss7Kills >= 1)
            {
                if (Math.Abs(ac.boss7SpawnTime() - a.boss7Spawn.totalseconds) < 30)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (_character.effectiveBossID() >= 467 || _character.adventure.boss8Kills >= 1)
            {
                if (Math.Abs(ac.boss8SpawnTime() - a.boss8Spawn.totalseconds) < 30)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (_character.effectiveBossID() >= 491 || _character.adventure.boss9Kills >= 1)
            {
                if (Math.Abs(ac.boss9SpawnTime() - a.boss9Spawn.totalseconds) < 30)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (_character.effectiveBossID() >= 727 || _character.adventure.boss10Kills >= 1)
            {
                if (Math.Abs(ac.boss10SpawnTime() - a.boss10Spawn.totalseconds) < 30)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (_character.effectiveBossID() >= 826 || _character.adventure.boss11Kills >= 1)
            {
                if (Math.Abs(ac.boss11SpawnTime() - a.boss11Spawn.totalseconds) < 30)
                {
                    return true;
                }
            }
            i++;
            if (i == ak)
                return false;
            if (_character.effectiveBossID() >= 848 || _character.adventure.boss12Kills >= 1)
            {
                if (Math.Abs(ac.boss12SpawnTime() - a.boss12Spawn.totalseconds) < 30)
                {
                    return true;
                }
            }

            return false;
        }

        internal bool CanSwapYgg()
        {
            return _controller.loadoutButtons[2].interactable;
        }

        internal bool SwapForYgg()
        {
            if (_lock == LockType.Titan)
            {
                return false;
            }

            if (_lock != LockType.Yggdrasil)
            {
                
                _lock = LockType.Yggdrasil;
                SaveCurrentLoadout();
                _character.removeMostEnergy();
                _character.removeMostMagic();
                _controller.equipLoadout(2);
            }
            
            return true;
        }

        internal LockType GetLockType()
        {
            return _lock;
        }

        internal bool SwapLoadoutForTitans()
        {
            if (_lock == LockType.Yggdrasil)
                return false;

            if (TitansSpawningSoon() && _lock != LockType.Titan)
            {
                _lock = LockType.Titan;
                SaveCurrentLoadout();
                _character.removeMostEnergy();
                _character.removeMostMagic();
                _controller.equipLoadout(1);
                return true;
            }

            if (_lock == LockType.Titan && !TitansSpawningSoon())
            {
                RestoreOriginalLoadout();
                return true;
            }

            return false;
        }

        internal void BoostEquipped()
        {
            // Boost Equipped Slots
            _controller.applyAllBoosts(-1);
            _controller.applyAllBoosts(-2);
            _controller.applyAllBoosts(-3);
            _controller.applyAllBoosts(-4);
            _controller.applyAllBoosts(-5);

            if (_controller.weapon2Unlocked())
            {
                _controller.applyAllBoosts(-6);
            }
        }

        internal void BoostAccessories()
        {
            for (var i = 10000; _controller.accessoryID(i) < _controller.accessorySpaces(); i++)
            {
                _controller.applyAllBoosts(i);
            }
        }

        internal void BoostInventory(string[] items, ih[] ih)
        {
            foreach (var item in items)
            {
                //Find all inventory slots that match this item name
                var targets =
                    ih.Where(x => x.name.Equals(item, StringComparison.CurrentCultureIgnoreCase)).ToArray();

                switch (targets.Length)
                {
                    case 0:
                        continue;
                    case 1:
                        _controller.applyAllBoosts(targets[0].slot);
                        continue;
                    default:
                        //Find the highest level version of the item (locked = +100) and apply boosts to it
                        _controller.applyAllBoosts(targets.MaxItem().slot);
                        break;
                }
            }
        }

        internal void BoostInventory(int[] items, ih[] ih)
        {
            foreach (var item in items)
            {
                //Find all inventory slots that match this item name
                var targets =
                    ih.Where(x => x.id == item).ToArray();

                switch (targets.Length)
                {
                    case 0:
                        continue;
                    case 1:
                        _controller.applyAllBoosts(targets[0].slot);
                        continue;
                    default:
                        //Find the highest level version of the item (locked = +100) and apply boosts to it
                        _controller.applyAllBoosts(targets.MaxItem().slot);
                        break;
                }
            }
        }

        internal void BoostInfinityCube()
        {
            _controller.infinityCubeAll();
            _controller.updateInventory();
        }

        internal void MergeEquipped()
        {
            // Boost Equipped Slots
            _controller.mergeAll(-1);
            _controller.mergeAll(-2);
            _controller.mergeAll(-3);
            _controller.mergeAll(-4);
            _controller.mergeAll(-5);

            if (_controller.weapon2Unlocked())
            {
                _controller.mergeAll(-6);
            }

            //Boost Accessories
            for (var i = 10000; _controller.accessoryID(i) < _controller.accessorySpaces(); i++)
            {
                _controller.mergeAll(i);
            }
        }

        internal void MergeBoosts(ih[] ci)
        {
            var grouped = ci.Where(x =>
                x.id <= 40 && !_character.inventory.inventory[x.slot].removable &&
                !_character.inventory.itemList.itemMaxxed[x.id]);

            foreach (var target in grouped)
            {
                if (target.level == 100)
                {
                    _outputWriter.WriteLine($"Removing protection from {target.name} in slot {target.slot}");
                    _character.inventory.inventory[target.slot].removable = false;
                    continue;
                }

                if (ci.Count(x => x.id == target.id) <= 1) continue;
                _outputWriter.WriteLine($"Merging {target.name} in slot {target.slot}");
                _controller.mergeAll(target.slot);
            }
            _outputWriter.Flush();
        }

        internal void MergeInventory(ih[] ci)
        {
            var grouped =
                ci.Where(x => x.id > 40 && x.level < 100).GroupBy(x => x.id).Where(x => x.Count() > 1);

            foreach (var item in grouped)
            {
                var target = item.MaxItem();

                _outputWriter.WriteLine($"Merging {target.name} in slot {target.slot}");
                _controller.mergeAll(target.slot);
            }
            _outputWriter.Flush();
        }

        internal void MergeGuffs()
        {
            for (var id = 1000000; id - 1000000 < _character.inventory.macguffins.Count; ++id)
                _controller.mergeAll(id);
        }

        internal void ManagePendant(ih[] ci)
        {
            var grouped = ci.Where(x => _pendants.Contains(x.id));
            foreach (var item in grouped)
            {
                if (item.level != 100) continue;
                var temp = _character.inventory.inventory[item.slot];
                if (!temp.removable) continue;
                var ic = _controller.inventory[item.slot];
                _outputWriter.WriteLine();
                typeof(ItemController).GetMethod("consumeItem", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(ic, null);
            }
        }

        internal void ManageLooty(ih[] ci)
        {
            var grouped = ci.Where(x => _lootys.Contains(x.id));
            foreach (var item in grouped)
            {
                if (item.level != 100) continue;
                var temp = _character.inventory.inventory[item.slot];
                if (!temp.removable) continue;
                var ic = _controller.inventory[item.slot];
                typeof(ItemController).GetMethod("consumeItem", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(ic, null);
            }
        }

        internal void ManageWandoos(ih[] ci)
        {
            var win = ci.Where(x => x.id == _wandoos[0]).DefaultIfEmpty(null).FirstOrDefault();
            if (win != null)
            {
                if (win.level > _character.wandoos98.OSlevel)
                {
                    var ic = _controller.inventory[win.slot];
                    typeof(ItemController).GetMethod("consumeItem", BindingFlags.NonPublic | BindingFlags.Instance)
                        ?.Invoke(ic, null);
                }
            }
        }

        internal void OptimizeSeedGain(ih[] ci)
        {
            var items = ci.Where(x => x.equipment.isEquipment() && GetSeedGain(x.equipment) > 0);
        }

        internal float GetSeedGain(Equipment e)
        {
            var amount =
                typeof(ItemController).GetMethod("effectBonus", BindingFlags.NonPublic | BindingFlags.Instance);
            if (e.spec1Type == specType.Seeds)
            {
                var p = new object[] {e.spec1Cur, e.spec1Type};
                return (float)amount?.Invoke(_controller, p);
            }
            if (e.spec2Type == specType.Seeds)
            {
                var p = new object[] { e.spec2Cur, e.spec2Type };
                return (float)amount?.Invoke(_controller, p);
            }
            if (e.spec3Type == specType.Seeds)
            {
                var p = new object[] { e.spec3Cur, e.spec3Type };
                return (float)amount?.Invoke(_controller, p);
            }

            return 0;
        }

        #region Filtering
        internal void EnsureFiltered(ih[] ci)
        {
            var targets = ci.Where(x => x.level == 100);
            foreach (var target in targets)
            {
                FilterItem(target.id);
            }

            FilterEquip(_character.inventory.head);
            FilterEquip(_character.inventory.boots);
            FilterEquip(_character.inventory.chest);
            FilterEquip(_character.inventory.legs);
            FilterEquip(_character.inventory.weapon);
            if (_character.inventoryController.weapon2Unlocked())
                FilterEquip(_character.inventory.weapon2);

            foreach (var acc in _character.inventory.accs)
            {
                FilterEquip(acc);
            }
        }

        void FilterItem(int id)
        {
            if (_pendants.Contains(id) || _lootys.Contains(id) || _filterExcludes.Contains(id) || id < 40)
                return;

            _character.inventory.itemList.itemFiltered[id] = true;
        }

        void FilterEquip(Equipment e)
        {
            if (e.level == 100)
            {
                FilterItem(e.id);
            }
        }
        #endregion

    }
}
