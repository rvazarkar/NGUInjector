using System.Collections.Generic;
using System.Linq;
using static NGUInjector.Main;

namespace NGUInjector.Managers
{
    internal class WishManager
    {
        private readonly Character _character;
        private Dictionary<int, double> dictDouble = new Dictionary<int, double>();
        public List<int> curValidUpgradesList = new List<int>();

        public WishManager()
        {
            _character = Main.Character;
        }

        public int getSlot(int slotID)
        {
            buildWishList();
            if (slotID > this.curValidUpgradesList.Count())
            {
                return -1;
            }
            return this.curValidUpgradesList[slotID];
        }

        public void buildWishList()
        {
            for (int i = 0; i < this._character.wishes.wishes.Count; i++)
            {
                this.curValidUpgradesList.Add(i);
            }
            for (int i = 0; i < this.curValidUpgradesList.Count; i++)
            {
                if (_character.wishesController.properties[this.curValidUpgradesList[i]].difficultyRequirement > _character.wishesController.character.settings.rebirthDifficulty)
                {
                    this.curValidUpgradesList.RemoveAt(i);
                    i--;
                    continue;
                }
                if (_character.wishesController.progressPerTickMax(this.curValidUpgradesList[i]) <= 0f)
                {
                    this.curValidUpgradesList.RemoveAt(i);
                    i--;
                    continue;
                }
                if ((long)_character.wishesController.character.wishes.wishes[this.curValidUpgradesList[i]].level >= _character.wishesController.properties[this.curValidUpgradesList[i]].maxLevel)
                {
                    this.curValidUpgradesList.RemoveAt(i);
                    i--;
                    continue;
                }
            }
            for (int i = 0; i < this.curValidUpgradesList.Count; i++)
            {
                this.dictDouble.Add(this.curValidUpgradesList[i], (double)_character.wishesController.properties[this.curValidUpgradesList[i]].wishSpeedDivider);
            }
            this.dictDouble = (from x in this.dictDouble
                               orderby x.Value
                               select x).ToDictionary((KeyValuePair<int, double> x) => x.Key, (KeyValuePair<int, double> x) => x.Value);
            this.curValidUpgradesList.Clear();
            for (int j = 0; j < this.dictDouble.Count; j++)
            {
                this.curValidUpgradesList.Add(this.dictDouble.ElementAt(j).Key);
            }
        }
    }
}