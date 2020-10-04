using System.Collections.Generic;
using System.Linq;

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
            _curValidUpgradesList.Clear();
            for (var i = 0; i < _character.wishes.wishes.Count; i++)
            {
                _curValidUpgradesList.Add(i);
            }

            for (var i = 0; i < _curValidUpgradesList.Count; i++)
            {
                if (_character.wishesController.properties[_curValidUpgradesList[i]].difficultyRequirement > _character.wishesController.character.settings.rebirthDifficulty)
                {
                    _curValidUpgradesList.RemoveAt(i);
                    i--;
                    continue;
                }
                if (_character.wishesController.progressPerTickMax(_curValidUpgradesList[i]) <= 0f)
                {
                    _curValidUpgradesList.RemoveAt(i);
                    i--;
                    continue;
                }
                if (_character.wishesController.character.wishes.wishes[_curValidUpgradesList[i]].level >= _character.wishesController.properties[_curValidUpgradesList[i]].maxLevel)
                {
                    _curValidUpgradesList.RemoveAt(i);
                    i--;
                }
            }

            var dictDouble = new Dictionary<int, double>();

            for (var i = 0; i < _curValidUpgradesList.Count; i++)
            {
                dictDouble.Add(i, _character.wishesController.properties[i].wishSpeedDivider);
            }

            dictDouble = (from x in dictDouble
                               orderby x.Value
                               select x).ToDictionary(x => x.Key, x => x.Value);
            _curValidUpgradesList.Clear();
            for (var j = 0; j < dictDouble.Count; j++)
            {
                _curValidUpgradesList.Add(dictDouble.ElementAt(j).Key);
            }
        }
    }
}