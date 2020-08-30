using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;

namespace NGUInjector
{
    internal class YggdrasilManager
    {
        private readonly Character _character;
        private readonly InventoryManager _inv;
        private StreamWriter _outputWriter;

        public YggdrasilManager(InventoryManager inv)
        {
            _character = Main.Character;
            _outputWriter = Main.OutputWriter;
            _inv = inv;
        }

        bool NeedsHarvest()
        {
            for (var i = 0; i < _character.yggdrasil.fruits.Count; i++)
            {
                var fruit = _character.yggdrasil.fruits[i];
                if (fruit.maxTier == 0L)
                    continue;

                if (_character.yggdrasilController.fruits[0].harvestTier(i) >= _character.yggdrasil.fruits[i].maxTier &&
                    _character.yggdrasilController.fruits[0].harvestTier(i) >= 1)
                {
                    return true;
                }
            }

            return false;
        }
        internal void ManageYggHarvest()
        {
            //We need to harvest but we dont have a loadout to manage OR we're not managing loadout
            if (!Main.ManageYggdrasilLoadouts || LoadoutManager.YggdrasilLoadout.Length == 0)
            {
                //Not sure why this would be true, but safety first
                if (LoadoutManager.CurrentLock == LoadoutManager.LockType.Yggdrasil)
                {
                    LoadoutManager.RestoreGear();
                    LoadoutManager.ReleaseLock();
                }
                _character.yggdrasilController.consumeAll();
                return;
            }

            //We dont need to harvest anymore and we've already swapped, so swap back
            if (!NeedsHarvest() && LoadoutManager.CurrentLock == LoadoutManager.LockType.Yggdrasil)
            {
                LoadoutManager.RestoreGear();
                LoadoutManager.ReleaseLock();
            }

            //We're managing loadouts
            if (NeedsHarvest())
            {
                if (!LoadoutManager.TryYggdrasilSwap())
                    return;

                //We swapped so harvest
                _character.yggdrasilController.consumeAll();
            }
        }

        internal void CheckFruits()
        {
            for (var i = 0; i < _character.yggdrasil.fruits.Count; i++)
            {
                var fruit = _character.yggdrasil.fruits[i];
                //Skip inactive fruits
                if (fruit.maxTier == 0L)
                    continue;

                //Skip fruits that are permed
                if (fruit.permCostPaid)
                    continue;

                if (fruit.activated)
                    continue;

                if (_character.yggdrasilController.usesEnergy[i] &&
                    _character.curEnergy >= _character.yggdrasilController.activationCost[i])
                {
                    _outputWriter.WriteLine($"Removing energy for fruit {i}");
                    _character.removeMostEnergy();
                    _character.yggdrasilController.fruits[i].activate(i);
                    continue;
                }

                if (!_character.yggdrasilController.usesEnergy[i] &&
                    _character.magic.curMagic >= _character.yggdrasilController.activationCost[i])
                {
                    _outputWriter.WriteLine($"Removing magic for fruit {i}");
                    _character.removeMostMagic();
                    _character.yggdrasilController.fruits[i].activate(i);
                }
            }
        }
    }
}
