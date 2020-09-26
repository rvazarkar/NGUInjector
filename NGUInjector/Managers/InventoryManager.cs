using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static NGUInjector.Main;

namespace NGUInjector.Managers
{

    public class FixedSizedQueue
    {
        private Queue<int> queue = new Queue<int>();

        public int Size { get; private set; }

        public FixedSizedQueue(int size)
        {
            Size = size;
        }

        public void Enqueue(int obj)
        {
            queue.Enqueue(obj);

            while (queue.Count > Size)
            {
                queue.Dequeue();
            }
        }

        public double Avg()
        {
            return queue.Average(x => x);
        }
    }

    internal class InventoryManager
    {
        private readonly Character _character;
        private readonly InventoryController _controller;

        private readonly int[] _pendants = { 53, 76, 94, 142, 170, 229, 295, 388, 430, 504 };
        private readonly int[] _lootys = { 67, 128, 169, 230, 296, 389, 431, 505 };
        private readonly int[] _convertibles;
        private readonly int[] _wandoos = {66, 169};
        private readonly int[] _guffs = {198, 200, 199, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 228, 211, 250, 291, 289, 290, 298, 299, 300};
        private int counter = 0;
        private BoostsNeeded _previousBoostsNeeded;
        private FixedSizedQueue avg = new FixedSizedQueue(60);


        //Wandoos 98, Giant Seed, Wandoos XL, Lonely Flubber, Wanderer's Cane, Guffs
        private readonly int[] _filterExcludes = { 66, 92, 163, 120, 154, 278, 279, 280, 281, 282, 283, 284, 285, 286, 287  };
        public InventoryManager()
        {
            _character = Main.Character;
            _controller = Controller;
            var temp = _pendants.Concat(_lootys).ToList();
            temp.Add(154);
            _convertibles = temp.ToArray();
        }

        internal ih[] GetBoostSlots(ih[] ci)
        {
            var result = new List<ih>();
            //First, find items in our priority list
            foreach (var id in Settings.PriorityBoosts)
            {
                if (Settings.BoostBlacklist.Contains(id))
                    continue;
                
                var f = FindItemSlot(ci, id);
                if (f != null)
                    result.Add(f);
            }

            //Next, get equipped items that aren't in our priority list and aren't blacklisted
            var equipped = Main.Character.inventory.GetConvertedEquips()
                .Where(x => !Settings.PriorityBoosts.Contains(x.id) && !Settings.BoostBlacklist.Contains(x.id));
            result = result.Concat(equipped).ToList();

            //Finally, find locked items in inventory that aren't blacklisted
            var invItems = ci.Where(x => x.locked && !Settings.BoostBlacklist.Contains(x.id) && !Settings.PriorityBoosts.Contains(x.id));
            result = result.Concat(invItems).ToList();

            return result.ToArray();
        }

        internal void BoostInventory(ih[] boostSlots)
        {
            foreach (var item in boostSlots)
            {
                _controller.applyAllBoosts(item.slot);
            }
        }

        private static ih FindItemSlot(IEnumerable<ih> ci, int id)
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
                    inv.weapon2.GetInventoryHelper(-6);
                }
            }

            for (var i = 0; i < inv.accs.Count; i++)
            {
                if (inv.accs[i].id == id)
                {
                    return inv.accs[i].GetInventoryHelper(i + 10000);
                }
            }

            var items = ci.Where(x => x.id == id).ToArray();
            if (items.Length != 0) return items.MaxItem();

            return null;
        }

        private static Equipment FindItemEquip(IEnumerable<ih> ci, int id)
        {
            var inv = Main.Character.inventory;
            if (inv.head.id == id)
            {
                return inv.head;
            }

            if (inv.chest.id == id)
            {
                return inv.chest;
            }

            if (inv.legs.id == id)
            {
                return inv.legs;
            }

            if (inv.boots.id == id)
            {
                return inv.boots;
            }

            if (inv.weapon.id == id)
            {
                return inv.weapon;
            }

            if (Controller.weapon2Unlocked())
            {
                if (inv.weapon2.id == id)
                {
                    return inv.weapon2;
                }
            }

            for (var i = 0; i < inv.accs.Count; i++)
            {
                if (inv.accs[i].id == id)
                {
                    return inv.accs[i];
                }
            }

            var items = ci.Where(x => x.id == id).ToArray();
            if (items.Length != 0) return items.MaxItem().equipment;

            return null;
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
                if (ci.Count(x => x.id == target.id) <= 1) continue;
                Log($"Merging {target.name} in slot {target.slot}");
                _controller.mergeAll(target.slot);
            }
        }

        internal void ManageQuestItems(ih[] ci)
        {
            //Merge quest items first
            var toMerge = ci.Where(x =>
                x.id >= 278 && x.id <= 287 && !_character.inventory.inventory[x.slot].removable &&
                !_character.inventory.itemList.itemMaxxed[x.id]);

            foreach (var target in toMerge)
            {
                if (ci.Count(x => x.id == target.id) <= 1) continue;
                Log($"Merging {target.name} in slot {target.slot}");
                _controller.mergeAll(target.slot);
            }

            //Consume quest items that dont need to be merged
            var questItems = ci.Where(x =>
                x.id >= 278 && x.id <= 287 && _character.inventory.inventory[x.slot].removable);

            foreach (var target in questItems)
            {
                var newSlot = ChangePage(target.slot);
                var ic = _controller.inventory[newSlot];
                Log($"Using quest item {target.name} in slot {target.slot}");
                typeof(ItemController).GetMethod("consumeItem", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(ic, null);
            }
        }

        internal void MergeInventory(ih[] ci)
        {
            var grouped =
                ci.Where(x => x.id > 40 && x.level < 100 && !_guffs.Contains(x.id) && (x.id < 278 || x.id > 287)).GroupBy(x => x.id).Where(x => x.Count() > 1);

            foreach (var item in grouped)
            {
                var target = item.MaxItem();

                Log($"Merging {target.name} in slot {target.slot}");
                _controller.mergeAll(target.slot);
            }
        }

        internal void MergeGuffs(ih[] ci)
        {
            for (var id = 1000000; id - 1000000 < _character.inventory.macguffins.Count; ++id)
                _controller.mergeAll(id);

            var invGuffs = ci.Where(x => _guffs.Contains(x.id)).GroupBy(x => x.id).Where(x => x.Count() > 1);
            foreach (var guff in invGuffs)
            {
                var target = guff.MaxItem();
                _controller.mergeAll(target.slot);
            }
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

        internal void ChangeBoostConversion(ih[] boostSlots)
        {
            if (_character.challenges.levelChallenge10k.curCompletions <
                _character.allChallenges.level100Challenge.maxCompletions)
                return;

            if (!Settings.AutoConvertBoosts)
                return;

            var needed = new BoostsNeeded();

            foreach (var item in boostSlots)
            {
                needed.Add(item.equipment.GetNeededBoosts());
            }

            if (counter == 0)
            {
                if (_previousBoostsNeeded != null)
                {
                    var current = needed.Power + needed.Toughness + needed.Special;
                    var old = _previousBoostsNeeded.Power + _previousBoostsNeeded.Toughness +
                              _previousBoostsNeeded.Special;

                    var diff = old - current;
                    var total = needed.Power + needed.Toughness + needed.Special;
                    if (diff > 0)
                    {
                        avg.Enqueue((int)diff);
                    }
                    var eta = Math.Ceiling(total / avg.Avg());

                    Log($"Boosts Needed to Green: {needed.Power} Power, {needed.Toughness} Toughness, {needed.Special} Special ({current - old} [{eta}m])");
                }
                else
                {
                    Log($"Boosts Needed to Green: {needed.Power} Power, {needed.Toughness} Toughness, {needed.Special} Special");
                }

                _previousBoostsNeeded = needed;
            }
                

            counter++;
            if (counter == 6) counter = 0;

            if (needed.Power > 0)
            {
                _controller.selectAutoPowerTransform();
                return;
            }

            if (needed.Toughness > 0)
            {
                _controller.selectAutoToughTransform();
                return;
            }

            if (needed.Special > 0)
            {
                _controller.selectAutoSpecialTransform();
                return;
            }

            _controller.selectAutoNoneTransform();
        }

        #region Filtering
        internal void EnsureFiltered(ih[] ci)
        {
            if (!Main.Character.arbitrary.lootFilter)
                return;

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
            if (id >= 278 && id <= 287)
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

    public class ih
    {
        internal int slot { get; set; }
        internal string name { get; set; }
        internal int level { get; set; }
        internal bool locked { get; set; }
        internal int id { get; set; }
        internal Equipment equipment { get; set; }
    }

    public class BoostsNeeded
    {
        internal float Power { get; set; }
        internal float Toughness { get; set; }
        internal float Special { get; set; }

        public BoostsNeeded()
        {
            Power = 0;
            Toughness = 0;
            Special = 0;
        }

        public void Add(BoostsNeeded other)
        {
            Power += other.Power;
            Toughness += other.Toughness;
            Special += other.Special;
        }
    }
}
