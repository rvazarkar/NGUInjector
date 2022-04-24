using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static NGUInjector.Main;

namespace NGUInjector.Managers
{

    public class FixedSizedQueue
    {
        private Queue<decimal> queue = new Queue<decimal>();

        public int Size { get; private set; }

        public FixedSizedQueue(int size)
        {
            Size = size;
        }

        public void Enqueue(decimal obj)
        {
            queue.Enqueue(obj);

            while (queue.Count > Size)
            {
                queue.Dequeue();
            }
        }

        public void Reset()
        {
            queue.Clear();
        }

        public decimal Avg()
        {
            try
            {
                return queue.Average(x => x);
            }
            catch (Exception e)
            {
                Log(e.Message);
                return 0;
            }
        }
    }

    public class Cube
    {
        internal float Power { get; set; }
        internal float Toughness { get; set; }
        protected bool Equals(Cube other)
        {
            return Power.Equals(other.Power) && Toughness.Equals(other.Toughness);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Cube) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Power.GetHashCode() * 397) ^ Toughness.GetHashCode();
            }
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
        private readonly int[] _mergeBlacklist = { 367, 368, 369, 370, 371, 372 };
        private BoostsNeeded _previousBoostsNeeded = null;
        private Cube _lastCube = null;
        private readonly FixedSizedQueue _invBoostAvg = new FixedSizedQueue(60);
        private readonly FixedSizedQueue _cubeBoostAvg = new FixedSizedQueue(60);


        //Wandoos 98, Giant Seed, Wandoos XL, Lonely Flubber, Wanderer's Cane, Guffs, Lemmi
        private readonly int[] _filterExcludes = { 66, 92, 163, 120, 154, 195, 278, 279, 280, 281, 282, 283, 284, 285, 286, 287  };
        public InventoryManager()
        {
            _character = Main.Character;
            _controller = Controller;
            var temp = _pendants.Concat(_lootys).ToList();
            //Wanderer's Cane
            temp.Add(154);
            //Lonely Flubber
            temp.Add(120);
            //A Giant Seed
            temp.Add(92);
            _convertibles = temp.ToArray();
        }

        internal void Reset()
        {
            _invBoostAvg.Reset();
            _cubeBoostAvg.Reset();
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
            var invItems = ci.Where(x => x.locked && x.equipment.isEquipment() && !Settings.BoostBlacklist.Contains(x.id) && !Settings.PriorityBoosts.Contains(x.id));
            result = result.Concat(invItems).ToList();

            //Make sure we filter out non-equips again, just in case one snuck into priorityboosts
            return result.Where(x => x.equipment.isEquipment()).Where(x => x.equipment.GetNeededBoosts().Total() > 0).ToArray();
        }

        internal void BoostInventory(ih[] boostSlots)
        {
            foreach (var item in boostSlots)
            {
                if (!_character.inventory.HasBoosts())
                    break;
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

        private int ChangePage(int slot)
        {
            var page = (int)Math.Floor((double)slot / 60);
            _controller.changePage(page);
            return slot - (page * 60);
        }

        internal void BoostInfinityCube()
        {
            if (!_character.inventory.HasBoosts())
                return;
            _controller.infinityCubeAll();
            _controller.updateInventory();
        }

        internal void MergeEquipped(ih[] ci)
        {
            if (ci.Any(x => x.id == _character.inventory.head.id))
            {
                _controller.mergeAll(-1);
            }

            if (ci.Any(x => x.id == _character.inventory.chest.id))
            {
                _controller.mergeAll(-2);
            }

            if (ci.Any(x => x.id == _character.inventory.legs.id))
            {
                _controller.mergeAll(-3);
            }

            if (ci.Any(x => x.id == _character.inventory.boots.id))
            {
                _controller.mergeAll(-4);
            }

            if (ci.Any(x => x.id == _character.inventory.weapon.id))
            {
                _controller.mergeAll(-5);
            }

            if (_controller.weapon2Unlocked())
            {
                if (ci.Any(x => x.id == _character.inventory.weapon2.id))
                {
                    _controller.mergeAll(-6);
                }
            }

            //Boost Accessories
            for (var i = 10000; _controller.accessoryID(i) < _controller.accessorySpaces(); i++)
            {
                if (ci.Any(x => x.id == _character.inventory.accs[_controller.accessoryID(i)].id))
                {
                    _controller.mergeAll(i);
                }
            }
        }

        internal void MergeBoosts(ih[] ci)
        {
            var grouped = ci.Where(x =>
                x.id <= 39 && !_character.inventory.inventory[x.slot].removable &&
                !_character.inventory.itemList.itemMaxxed[x.id]);

            foreach (var target in grouped)
            {
                if (ci.Count(x => x.id == target.id) <= 1) continue;
                Log($"Merging {target.name} in slot {target.slot}");
                _controller.mergeAll(target.slot);
            }
        }

        private string SanitizeName(string name)
        {
            if (name.Contains("\n"))
            {
                name = name.Split('\n').Last();
            }

            return name;
        }

        internal void ManageQuestItems(ih[] ci)
        {
            var curPage = (int)Math.Floor((double)_controller.inventory[0].id / 60);
            //Merge quest items first
            var toMerge = ci.Where(x =>
                x.id >= 278 && x.id <= 287 && !_character.inventory.inventory[x.slot].removable &&
                !_character.inventory.itemList.itemMaxxed[x.id]);

            foreach (var target in toMerge)
            {
                if (ci.Count(x => x.id == target.id) <= 1) continue;
                Log($"Merging {SanitizeName(target.name)} in slot {target.slot}");
                _controller.mergeAll(target.slot);
            }

            //Consume quest items that dont need to be merged
            var questItems = ci.Where(x =>
                x.id >= 278 && x.id <= 287 && _character.inventory.inventory[x.slot].removable).ToArray();

            if (questItems.Length > 0)
                Log($"Turning in {questItems.Length} quest items");
            foreach (var target in questItems)
            {
                var newSlot = ChangePage(target.slot);
                var ic = _controller.inventory[newSlot];
                typeof(ItemController).GetMethod("consumeItem", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(ic, null);
            }
            _controller.changePage(curPage);
        }

        internal void MergeInventory(ih[] ci)
        {
            var grouped =
                ci.Where(x => x.id >= 40 && x.level < 100 && !_mergeBlacklist.Contains(x.id) && !Settings.MergeBlacklist.Contains(x.id) && !_guffs.Contains(x.id) && (x.id < 278 || x.id > 287)).GroupBy(x => x.id).Where(x => x.Count() > 1);

            foreach (var item in grouped)
            {
                if (item.All(x => x.locked))
                    continue;

                var target = item.MaxItem();

                Log($"Merging {SanitizeName(target.name)} in slot {target.slot}");
                _controller.mergeAll(target.slot);
            }
        }

        internal void MergeGuffs(ih[] ci)
        {
            for (var id = 0; id < _character.inventory.macguffins.Count; ++id)
            {
                var guffId = _character.inventory.macguffins[id].id;
                if (ci.Any(x => x.id == guffId))
                    _controller.mergeAll(_controller.globalMacguffinID(id));
            }

            var invGuffs = ci.Where(x => _guffs.Contains(x.id)).GroupBy(x => x.id).Where(x => x.Count() > 1);
            foreach (var guff in invGuffs)
            {
                var target = guff.MaxItem();
                _controller.mergeAll(target.slot);
            }
        }

        internal void ManageConvertibles(ih[] ci)
        {
            var curPage = (int)Math.Floor((double)_controller.inventory[0].id / 60);
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
            _controller.changePage(curPage);
        }

        internal void ShowBoostProgress(ih[] boostSlots)
        {
            var needed = new BoostsNeeded();
            var cube = new Cube
            {
                Power = _character.inventory.cubePower,
                Toughness = _character.inventory.cubeToughness
            };

            foreach (var item in boostSlots)
            {
                needed.Add(item.equipment.GetNeededBoosts());
            }

            var current = needed.Power + needed.Toughness + needed.Special;

            if (current > 0)
            {
                if (_previousBoostsNeeded == null)
                {
                    Log($"Boosts Needed to Green: {needed.Power} Power, {needed.Toughness} Toughness, {needed.Special} Special");
                    _previousBoostsNeeded = needed;
                }
                else
                {
                    var old = _previousBoostsNeeded.Power + _previousBoostsNeeded.Toughness +
                              _previousBoostsNeeded.Special;

                    var diff = current - old;

                    if (diff == 0) return;

                    //If diff is > 0, then we either added another item to boost or we levelled something. Don't add the diff to average
                    if (diff <= 0)
                    {
                        _invBoostAvg.Enqueue(diff * -1);
                    }

                    Log($"Boosts Needed to Green: {needed.Power} Power, {needed.Toughness} Toughness, {needed.Special} Special");
                    var average = _invBoostAvg.Avg();
                    if (average > 0)
                    {
                        var eta = current / average;
                        Log($"Last Minute: {diff}. Average Per Minute: {average:0}. ETA: {eta:0} minutes.");
                    }
                    else
                    {
                        Log($"Last Minute: {diff}.");
                    }

                    _previousBoostsNeeded = needed;
                }
            }

            if (_lastCube == null)
            {
                _lastCube = cube;
            }
            else
            {
                if (!_lastCube.Equals(cube))
                {
                    var output = $"Cube Progress:";
                    var toughnessDiff = cube.Toughness - _lastCube.Toughness;
                    var powerDiff = cube.Power - _lastCube.Power;

                    output = toughnessDiff > 0 ? $"{output} {toughnessDiff} Toughness." : output;
                    output = powerDiff > 0 ? $"{output} {powerDiff} Power." : output;

                    _cubeBoostAvg.Enqueue((decimal)(toughnessDiff + powerDiff));
                    output = $"{output} Average Per Minute: {_cubeBoostAvg.Avg():0}";
                    Log(output);
                    Log($"Cube Power: {cube.Power} ({_character.inventoryController.cubePowerSoftcap()} softcap). Cube Toughness: {cube.Toughness} ({_character.inventoryController.cubeToughnessSoftcap()} softcap)");
                }

                _lastCube = cube;
            }
        }

        internal void ManageBoostConversion(ih[] boostSlots)
        {
            if (_character.challenges.levelChallenge10k.curCompletions <
                _character.allChallenges.level100Challenge.maxCompletions)
                return;

            if (!Settings.AutoConvertBoosts)
                return;

            var converted = _character.inventory.GetConvertedInventory();
            //If we have a boost locked, we want to stay on that until its maxxed
            var lockedBoosts = converted.Where(x => x.id < 40 && x.locked).ToArray();
            if (lockedBoosts.Any())
            {
                foreach (var locked in lockedBoosts)
                {
                    //Unlock level 100 boosts
                    if (locked.level == 100)
                    {
                        _character.inventory.inventory[locked.slot].removable = true;
                        continue;
                    }

                    if (locked.id <= 13)
                    {
                        _controller.selectAutoPowerTransform();
                    }else if (locked.id <= 26)
                    {
                        _controller.selectAutoToughTransform();
                    }else if (locked.id <= 39)
                    {
                        _controller.selectAutoSpecialTransform();
                    }
                }

                return;
            }

            var needed = new BoostsNeeded();

            foreach (var item in boostSlots)
            {
                needed.Add(item.equipment.GetNeededBoosts());
            }

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

            var cube = new Cube
            {
                Power = _character.inventory.cubePower,
                Toughness = _character.inventory.cubeToughness
            };

            if (Settings.CubePriority > 0)
            {
                if (Settings.CubePriority == 1)
                {
                    if (cube.Power > cube.Toughness)
                    {
                        _controller.selectAutoToughTransform();
                    }
                    else if (cube.Toughness > cube.Power)
                    {
                        _controller.selectAutoPowerTransform();
                    }
                    else
                    {
                        _controller.selectAutoPowerTransform();
                    }
                }else if (Settings.CubePriority == 2)
                {
                    _controller.selectAutoPowerTransform();
                }
                else
                {
                    _controller.selectAutoToughTransform();
                }
                
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
                _filterExcludes.Contains(id) || _guffs.Contains(id) || id < 40 || _mergeBlacklist.Contains(id))
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
        internal decimal Power { get; set; }
        internal decimal Toughness { get; set; }
        internal decimal Special { get; set; }

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

        public decimal Total()
        {
            return Power + Toughness + Special;
        }
    }
}
