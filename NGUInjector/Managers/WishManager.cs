using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using static NGUInjector.Main;

namespace NGUInjector.Managers
{
    internal class WishManager
    {
        private readonly Character _character;
        private readonly List<int> _curValidUpgradesList = new List<int>();

        public WishManager()
        {
            _character = Main.Character;
        }

        public int GetSlot(int slotId)
        {
            BuildWishList();
            if (slotId + 1 > _curValidUpgradesList.Count)
            {
                return -1;
            }
            return _curValidUpgradesList[slotId];
        }

        public void BuildWishList()
        {
            var dictDouble = new Dictionary<int, double>();

            _curValidUpgradesList.Clear();
            for (var i = 0; i < Settings.WishPriorities.Length; i++)
            {
                if (isValidWish(Settings.WishPriorities[i]))
                {
                    if (Settings.WishSortPriorities)
                    {
                        dictDouble.Add(Settings.WishPriorities[i], sortValue(Settings.WishPriorities[i]) + i);
                    } else
                    {
                        _curValidUpgradesList.Add(Settings.WishPriorities[i]);
                    }
                }
            }
            if (Settings.WishSortPriorities)
            {
                dictDouble = (from x in dictDouble
                              orderby x.Value
                              select x).ToDictionary(x => x.Key, x => x.Value);
                for (var j = 0; j < dictDouble.Count; j++)
                {
                    _curValidUpgradesList.Add(dictDouble.ElementAt(j).Key);
                }
                dictDouble = new Dictionary<int, double>();
            }
            for (var i = 0; i < _character.wishes.wishes.Count; i++)
            {
                if (_curValidUpgradesList.Contains(i))
                {
                    continue;
                }
                if (isValidWish(i))
                {
                    dictDouble.Add(i, this.sortValue(i) + i);
                }
            }            
            dictDouble = (from x in dictDouble
                               orderby x.Value
                               select x).ToDictionary(x => x.Key, x => x.Value);
            for (var j = 0; j < dictDouble.Count; j++)
            {
                _curValidUpgradesList.Add(dictDouble.ElementAt(j).Key);
            }
        }

        public bool isValidWish(int wishId)
        {
            if (wishId < 0 || wishId > _character.wishes.wishSize())
            {
                return false;
            }
            if (_character.wishesController.properties[wishId].difficultyRequirement > _character.wishesController.character.settings.rebirthDifficulty)
            {
                return false;
            }
            if (_character.wishesController.progressPerTickMax(wishId) <= 0f)
            {
                return false;
            }
            if (_character.wishesController.character.wishes.wishes[wishId].level >= _character.wishesController.properties[wishId].maxLevel)
            {
                return false;
            }
            if(Settings.WishBlacklist.Length > 0 && Settings.WishBlacklist.Contains(wishId))
            {
                return false;
            }
            return true;          
        }

        public double sortValue(int wishId)
        {
            if (Settings.WishSortOrder)
            {
                return _character.wishesController.wishSpeedDivider(wishId) * (1f - _character.wishes.wishes[wishId].progress);
            }
            return _character.wishesController.properties[wishId].wishSpeedDivider;
        }
    }
}