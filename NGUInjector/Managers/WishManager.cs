using System.Collections.Generic;
using System.Linq;

namespace NGUInjector.Managers
{
    internal class WishManager
    {
        private readonly Character _character;
        private Dictionary<int, double> _dictDouble = new Dictionary<int, double>();
        public List<int> curValidUpgradesList = new List<int>();

        public WishManager()
        {
            _character = Main.Character;
        }

        public int GetSlot(int slotId)
        {
            BuildWishList();
            if (slotId > curValidUpgradesList.Count())
            {
                return -1;
            }
            return curValidUpgradesList[slotId];
        }

        public void BuildWishList()
        {
            for (var i = 0; i < _character.wishes.wishes.Count; i++)
            {
                curValidUpgradesList.Add(i);
            }

            for (var i = 0; i < curValidUpgradesList.Count; i++)
            {
                if (_character.wishesController.properties[curValidUpgradesList[i]].difficultyRequirement > _character.wishesController.character.settings.rebirthDifficulty)
                {
                    curValidUpgradesList.RemoveAt(i);
                    i--;
                    continue;
                }
                if (_character.wishesController.progressPerTickMax(curValidUpgradesList[i]) <= 0f)
                {
                    curValidUpgradesList.RemoveAt(i);
                    i--;
                    continue;
                }
                if (_character.wishesController.character.wishes.wishes[curValidUpgradesList[i]].level >= _character.wishesController.properties[curValidUpgradesList[i]].maxLevel)
                {
                    curValidUpgradesList.RemoveAt(i);
                    i--;
                }
            }

            for (var i = 0; i < curValidUpgradesList.Count; i++)
            {
                _dictDouble.Add(i, _character.wishesController.properties[i].wishSpeedDivider);
            }

            _dictDouble = (from x in _dictDouble
                               orderby x.Value
                               select x).ToDictionary(x => x.Key, x => x.Value);
            curValidUpgradesList.Clear();
            for (var j = 0; j < _dictDouble.Count; j++)
            {
                curValidUpgradesList.Add(_dictDouble.ElementAt(j).Key);
            }
        }
    }
}