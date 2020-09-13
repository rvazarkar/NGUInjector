using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NGUInjector
{
    internal class InventoryManager
    {
        private readonly Character _character;
        private readonly InventoryController _controller;

        private readonly int[] _pendants = { 53, 76, 94, 142, 170, 229, 295, 388, 430, 504 };
        private readonly int[] _lootys = { 67, 128, 169, 230, 296, 389, 431, 505 };
        private readonly int[] _convertibles;
        private readonly int[] _wandoos = {66, 169};
        private readonly int[] _guffs = {228, 211, 250, 291, 289, 290, 298, 299, 300};
        internal static int[] BoostBlacklist;


        //Wandoos 98, Giant Seed, Wandoos XL, Lonely Flubber, Wanderer's Cane, Guffs
        private readonly int[] _filterExcludes = { 66, 92, 163, 120, 154, 278, 279, 280, 281, 282, 283, 284, 285, 286, 287  };
        public InventoryManager()
        {
            _character = Main.Character;
            _controller = Main.Controller;
            BoostBlacklist = new int[]{};
            var temp = _pendants.Concat(_lootys).ToList();
            temp.Add(154);
            _convertibles = temp.ToArray();
        }
        
        internal void BoostEquipped()
        {
            // Boost Equipped Slots
            if (!BoostBlacklist.Contains(_character.inventory.head.id))
                _controller.applyAllBoosts(-1);
            if (!BoostBlacklist.Contains(_character.inventory.chest.id))
                _controller.applyAllBoosts(-2);
            if (!BoostBlacklist.Contains(_character.inventory.legs.id))
                _controller.applyAllBoosts(-3);
            if (!BoostBlacklist.Contains(_character.inventory.boots.id))
                _controller.applyAllBoosts(-4);
            if (!BoostBlacklist.Contains(_character.inventory.weapon.id))
                _controller.applyAllBoosts(-5);

            if (_controller.weapon2Unlocked() && !BoostBlacklist.Contains(_character.inventory.head.id))
                _controller.applyAllBoosts(-6);
        }

        internal void BoostAccessories()
        {
            for (var i = 10000; _controller.accessoryID(i) < _controller.accessorySpaces(); i++)
            {
                if (!BoostBlacklist.Contains(_character.inventory.accs[_controller.accessoryID(i)].id))
                    _controller.applyAllBoosts(i);
            }
        }

        internal void BoostInventory(int[] items, ih[] ih)
        {
            foreach (var item in items)
            {
                //Find all inventory slots that match this item name
                var targets =
                    ih.Where(x => x.id == item && !BoostBlacklist.Contains(x.id)).ToArray();

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

        private int ChangePage(int slot)
        {
            var page = (int)Math.Floor((double)slot / 60);
            _controller.changePage(page);
            return slot - (page * 60);
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
                    Main.Log($"Removing protection from {target.name} in slot {target.slot}");
                    _character.inventory.inventory[target.slot].removable = false;
                    continue;
                }

                if (ci.Count(x => x.id == target.id) <= 1) continue;
                Main.Log($"Merging {target.name} in slot {target.slot}");
                _controller.mergeAll(target.slot);
            }
        }

        internal void MergeQuestItems(ih[] ci)
        {
            var grouped = ci.Where(x =>
                x.id >= 198 && x.id <= 210 && !_character.inventory.inventory[x.slot].removable &&
                !_character.inventory.itemList.itemMaxxed[x.id]);

            foreach (var target in grouped)
            {
                if (target.level == 100)
                {
                    Main.Log($"Removing protection from {target.name} in slot {target.slot}");
                    _character.inventory.inventory[target.slot].removable = false;
                    continue;
                }

                if (ci.Count(x => x.id == target.id) <= 1) continue;
                Main.Log($"Merging {target.name} in slot {target.slot}");
                _controller.mergeAll(target.slot);
            }
        }

        internal void MergeInventory(ih[] ci)
        {
            var grouped =
                ci.Where(x => x.id > 40 && x.level < 100 && (x.id < 198 || x.id > 210)).GroupBy(x => x.id).Where(x => x.Count() > 1);

            foreach (var item in grouped)
            {
                var target = item.MaxItem();

                Main.Log($"Merging {target.name} in slot {target.slot}");
                _controller.mergeAll(target.slot);
            }
        }

        internal void MergeGuffs()
        {
            for (var id = 1000000; id - 1000000 < _character.inventory.macguffins.Count; ++id)
                _controller.mergeAll(id);
        }

        internal void ManageConvertibles(ih[] ci)
        {
            var grouped = ci.Where(x => _convertibles.Contains(x.id));
            foreach (var item in grouped)
            {
                if (item.level != 100) continue;
                var temp = _character.inventory.inventory[item.slot];
                if (!temp.removable) continue;
                var newSlot = ChangePage(item.slot);
                var ic = _controller.inventory[newSlot];
                typeof(ItemController).GetMethod("consumeItem", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(ic, null);
            }
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
            if (_pendants.Contains(id) || _lootys.Contains(id) || _wandoos.Contains(id) ||
                _filterExcludes.Contains(id) || _guffs.Contains(id) || id < 40)
                return;

            //Dont filter quest items
            if (id >= 198 && id <= 210)
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
