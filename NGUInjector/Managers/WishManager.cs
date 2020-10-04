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
            if (slotId > _curValidUpgradesList.Count)
            {
                return -1;
            }
            return _curValidUpgradesList[slotId];
        }

        public void BuildWishList()
        {
            var dictDouble = new Dictionary<int, double>();
            var wishPriorities = new List<int>();
            
            for (var i = 0; i < Settings.WishPriorities.Count(); i++)
            {
                if (isValidWish(Settings.WishPriorities[i]))
                {
                    wishPriorities.Add(Settings.WishPriorities[i]);
                }
            }

            for (var i = 0; i < _character.wishes.wishes.Count; i++)
            {
                if (wishPriorities.Contains(i))
                {
                    continue;
                }
                if (isValidWish(i))
                {
                    dictDouble.Add(i, _character.wishesController.properties[i].wishSpeedDivider);
                }
            }            
            dictDouble = (from x in dictDouble
                               orderby x.Value
                               select x).ToDictionary(x => x.Key, x => x.Value);

            _curValidUpgradesList.Clear();
            for (var i = 0; i < wishPriorities.Count; i++)
            {
                _curValidUpgradesList.Add(wishPriorities[i]);
            }
            for (var j = 0; j < dictDouble.Count; j++)
            {
                _curValidUpgradesList.Add(dictDouble.ElementAt(j).Key);
            }
        }

        public bool isValidWish(int wishId)
        {
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
            return true;          
        }
    }
}