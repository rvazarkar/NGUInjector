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
            if (Index == 0)
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
            var allocationLeft = (long)MaxAllocation;
            for (var i = Character.bloodMagic.ritual.Count - 1; i >= 0; i--)
            {
                if (allocationLeft <= 0)
                    break;
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

                var tLeft = RitualTimeLeft(i, allocationLeft);

                if (tLeft > 3600)
                    continue;

                if (RebirthTime > 0 && Main.Settings.AutoRebirth)
                {
                    if (Character.rebirthTime.totalseconds - tLeft < 0)
                        continue;
                }

                var cap = CalculateMaxAllocation(i, allocationLeft);
                SetInput(cap);
                Character.bloodMagicController.bloodMagics[i].add();
                allocationLeft -= cap;
            }
        }

        private void CastRitualEndTime(int endTime)
        {
            var allocationLeft = (long)MaxAllocation;
            for (var i = Character.bloodMagic.ritual.Count - 1; i >= 0; i--)
            {
                if (allocationLeft <= 0)
                    break;
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

                var tLeft = RitualTimeLeft(i, allocationLeft);

                if (RebirthTime > 0 && Main.Settings.AutoRebirth)
                {
                    if (Character.rebirthTime.totalseconds - tLeft < 0)
                        continue;
                }

                if (Character.rebirthTime.totalseconds + tLeft > endTime)
                    continue;

                var cap = CalculateMaxAllocation(i, allocationLeft);
                SetInput(cap);
                Character.bloodMagicController.bloodMagics[i].add();
                allocationLeft -= cap;
            }
        }

        private float RitualProgressPerTick(int id, long remaining)
        {
            var num1 = 0.0;
            if (Character.settings.rebirthDifficulty == difficulty.normal)
                num1 = remaining * (double)Character.totalMagicPower() / 50000.0 /
                       Character.bloodMagicController.normalSpeedDividers[id];
            else if (Character.settings.rebirthDifficulty == difficulty.evil)
                num1 = remaining * (double)Character.totalMagicPower() / 50000.0 /
                       Character.bloodMagicController.evilSpeedDividers[id];
            else if (Character.settings.rebirthDifficulty == difficulty.sadistic)
                num1 = remaining * (double)Character.totalMagicPower() /
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

        public float RitualTimeLeft(int id, long remaining)
        {
            return (float)((1.0 - Character.bloodMagic.ritual[id].progress) /
                           RitualProgressPerTick(id, remaining) / 50.0);
        }

        private long CalculateMaxAllocation(int id, long remaining)
        {
            var num1 = Character.bloodMagicController.bloodMagics[id].capValue();
            if (remaining > num1)
            {
                return num1;
            }

            var num2 = (long) ((double) num1 / Math.Ceiling((double) num1 / (double) remaining)) + 1L;
            return num2;
        }
    }
}
