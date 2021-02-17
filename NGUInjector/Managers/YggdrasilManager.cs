using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using static NGUInjector.Main;

namespace NGUInjector.Managers
{
    internal class YggdrasilManager
    {
        private readonly Character _character;

        public YggdrasilManager()
        {
            _character = Main.Character;
        }

        internal static bool AnyHarvestable()
        {
            for (var i = 0; i < Main.Character.yggdrasil.fruits.Count; i++)
            {
                if (Main.Character.yggdrasilController.fruits[0].harvestTier(i) > 0)
                    return true;
            }

            return false;
        }

        internal bool NeedsHarvest()
        {
            return _character.yggdrasilController.anyFruitMaxxed();
        }

        internal bool NeedsSwap()
        {
            var thresh = Math.Max(1, Settings.YggSwapThreshold);
            for (var i = 0; i < Main.Character.yggdrasil.fruits.Count; i++)
            {
                if (Main.Character.yggdrasilController.fruits[0].harvestTier(i) >= thresh && Main.Character.yggdrasilController.fruits[0].fruitMaxxed(i))
                    return true;
            }

            return false;
        }

        internal void ManageYggHarvest()
        {
            //We need to harvest but we dont have a loadout to manage OR we're not managing loadout
            if (!Settings.SwapYggdrasilLoadouts || Settings.YggdrasilLoadout.Length == 0)
            {
                //Not sure why this would be true, but safety first
                if (LoadoutManager.CurrentLock == LockType.Yggdrasil)
                {
                    LoadoutManager.RestoreGear();
                    LoadoutManager.ReleaseLock();
                }

                if (DiggerManager.CurrentLock == LockType.Yggdrasil)
                {
                    DiggerManager.RestoreDiggers();
                    DiggerManager.ReleaseLock();
                }
                ActuallyHarvest();
                return;
            }

            //We dont need to harvest anymore and we've already swapped, so swap back
            if (!NeedsHarvest() && LoadoutManager.CurrentLock == LockType.Yggdrasil)
            {
                LoadoutManager.RestoreGear();
                LoadoutManager.ReleaseLock();
            }

            if (!NeedsHarvest() && DiggerManager.CurrentLock == LockType.Yggdrasil)
            {
                DiggerManager.RestoreDiggers();
                DiggerManager.ReleaseLock();
            }

            //We're managing loadouts
            if (NeedsHarvest())
            {
                if (NeedsSwap())
                {
                    if (!LoadoutManager.TryYggdrasilSwap() || !DiggerManager.TryYggSwap())
                        return;

                    Log("Equipping Loadout for Yggdrasil and Harvesting");
                }
                else
                {
                    Log("Harvesting without swap because threshold not met");
                }

                //Harvest stuff
                ActuallyHarvest();
            }
            else if (Settings.Level1FruitOfMacGuffinBeta && Main.Character.yggdrasilController.fruits[0].harvestTier(13) == 1)
            {
                Main.Character.yggdrasilController.fruits[0].consumeMacguffinFruit2();
            }
        }

        private void ActuallyHarvest()
        {
            ReadTooltipLog(false);
            var currentPage = _character.yggdrasilController.curPage;
            _character.yggdrasilController.changePage(0);
            _character.yggdrasilController.consumeAll();
            _character.yggdrasilController.changePage(1);
            _character.yggdrasilController.consumeAll();
            _character.yggdrasilController.changePage(2);
            _character.yggdrasilController.consumeAll();
            _character.yggdrasilController.changePage(currentPage);
            _character.yggdrasilController.refreshMenu();
            ReadTooltipLog(true);
        }

        internal static void HarvestAll()
        {
            ReadTooltipLog(false);
            var currentPage = Main.Character.yggdrasilController.curPage;
            Main.Character.yggdrasilController.changePage(0);
            Main.Character.yggdrasilController.consumeAll(true);
            Main.Character.yggdrasilController.changePage(1);
            Main.Character.yggdrasilController.consumeAll(true);
            Main.Character.yggdrasilController.changePage(2);
            Main.Character.yggdrasilController.consumeAll(true);
            Main.Character.yggdrasilController.changePage(currentPage);
            Main.Character.yggdrasilController.refreshMenu();
            ReadTooltipLog(true);
        }

        internal static void ReadTooltipLog(bool doLog)
        {
            var bLog = Main.Character.tooltip.log;
            var type = bLog.GetType().GetField("Eventlog",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            var val = type?.GetValue(bLog);

            if (val != null)
            {
                //Add something to the end of our logs to mark them as complete
                var log = (List<string>)val;
                for (var i = log.Count - 1; i >= 0; i--)
                {
                    var line = log[i];
                    if (line.EndsWith("<b></b>")) continue;
                    if (doLog)
                    {
                        LogPitSpin(line);
                    }
                    log[i] = $"{line}<b></b>";
                }
            }
        }

        internal void CheckFruits()
        {
            if (!Settings.ActivateFruits)
                return;
            var curPage = _character.yggdrasilController.curPage;
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
                    Log($"Removing energy for fruit {i}");
                    _character.removeMostEnergy();
                    var slot = ChangePage(i);
                    _character.yggdrasilController.fruits[slot].activate(i);
                    continue;
                }

                if (!_character.yggdrasilController.usesEnergy[i] &&
                    _character.magic.curMagic >= _character.yggdrasilController.activationCost[i])
                {
                    Log($"Removing magic for fruit {i}");
                    _character.removeMostMagic();
                    var slot = ChangePage(i);
                    _character.yggdrasilController.fruits[slot].activate(i);
                }
            }
            _character.yggdrasilController.changePage(curPage);
        }

        private int ChangePage(int slot)
        {
            var page = (int)Math.Floor((double)slot / 9);
            _character.yggdrasilController.changePage(page);
            return slot - (page * 9);
        }
    }
}
