using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGUInjector.AllocationProfiles.BreakpointTypes
{
    internal class BR : BaseBreakpoint
    {
        internal int RebirthTime { get; set; }
        protected override bool Unlocked()
        {
            return Character.buttons.bloodMagic.interactable;
        }

        protected override bool TargetMet()
        {
            return false;
        }

        internal override bool Allocate()
        {
            if (Index < Character.rebirthTime.totalseconds)
            {
                CastRituals();
            }
            else
            {
                CastRitualEndTime(Index);
            }

            return true;
        }

        protected override bool CorrectResourceType()
        {
            return Type == ResourceType.Magic;
        }

        private void CastRituals()
        {
            for (var i = Character.bloodMagic.ritual.Count - 1; i >= 0; i--)
            {
                if (Character.magic.idleMagic == 0)
                    break;
                if (i >= Character.bloodMagicController.ritualsUnlocked())
                    continue;
                var goldCost = Character.bloodMagicController.bloodMagics[i].baseCost * Character.totalDiscount();
                if (goldCost > Character.realGold && Character.bloodMagic.ritual[i].progress <= 0.0)
                {
                    if (Character.bloodMagic.ritual[i].magic > 0)
                    {
                        Character.bloodMagicController.bloodMagics[i].removeAllMagic();
                    }

                    continue;
                }

                var tLeft = RitualTimeLeft(i);

                if (tLeft > 3600)
                    continue;

                if (RebirthTime > 0 && Main.Settings.AutoRebirth)
                {
                    if (Character.rebirthTime.totalseconds - tLeft < 0)
                        continue;
                }

                Character.bloodMagicController.bloodMagics[i].cap();
            }
        }

        private void CastRitualEndTime(int endTime)
        {
            for (var i = Character.bloodMagic.ritual.Count - 1; i >= 0; i--)
            {
                if (Character.magic.idleMagic == 0)
                    break;
                if (i >= Character.bloodMagicController.ritualsUnlocked())
                    continue;
                var goldCost = Character.bloodMagicController.bloodMagics[i].baseCost * Character.totalDiscount();
                if (goldCost > Character.realGold && Character.bloodMagic.ritual[i].progress <= 0.0)
                {
                    if (Character.bloodMagic.ritual[i].magic > 0)
                    {
                        Character.bloodMagicController.bloodMagics[i].removeAllMagic();
                    }

                    continue;
                }

                var tLeft = RitualTimeLeft(i);

                if (RebirthTime > 0 && Main.Settings.AutoRebirth)
                {
                    if (Character.rebirthTime.totalseconds - tLeft < 0)
                        continue;
                }

                if (Character.rebirthTime.totalseconds + tLeft > endTime)
                    continue;

                Character.bloodMagicController.bloodMagics[i].cap();
            }
        }

        private float RitualProgressPerTick(int id)
        {
            var num1 = 0.0;
            if (Character.settings.rebirthDifficulty == difficulty.normal)
                num1 = Character.magic.idleMagic * (double)Character.totalMagicPower() / 50000.0 /
                       Character.bloodMagicController.normalSpeedDividers[id];
            else if (Character.settings.rebirthDifficulty == difficulty.evil)
                num1 = Character.magic.idleMagic * (double)Character.totalMagicPower() / 50000.0 /
                       Character.bloodMagicController.evilSpeedDividers[id];
            else if (Character.settings.rebirthDifficulty == difficulty.sadistic)
                num1 = Character.magic.idleMagic * (double)Character.totalMagicPower() /
                       Character.bloodMagicController.sadisticSpeedDividers[id];
            if (Character.settings.rebirthDifficulty >= difficulty.sadistic)
                num1 /= Character.bloodMagicController.bloodMagics[id].sadisticDivider();
            var num2 = num1 * Character.bloodMagicController.bloodMagics[id].totalBloodMagicSpeedBonus();
            if (num2 <= -3.40282346638529E+38)
                num2 = 0.0;
            if (num2 >= 3.40282346638529E+38)
                num2 = 3.40282346638529E+38;
            return (float)num2;
        }

        public float RitualTimeLeft(int id)
        {
            return (float)((1.0 - Character.bloodMagic.ritual[id].progress) /
                           RitualProgressPerTick(id) / 50.0);
        }
    }
}
