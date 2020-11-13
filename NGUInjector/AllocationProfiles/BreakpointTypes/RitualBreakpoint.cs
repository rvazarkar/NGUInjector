using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGUInjector.AllocationProfiles.BreakpointTypes
{
    internal class RitualBreakpoint : BaseBreakpoint
    {
        protected override bool Unlocked()
        {
            return Index <= Character.bloodMagicController.ritualsUnlocked() && Character.buttons.bloodMagic.interactable;
        }

        protected override bool TargetMet()
        {
            return false;
        }

        internal override bool Allocate()
        {
            var goldCost = Character.bloodMagicController.bloodMagics[Index].baseCost * Character.totalDiscount();
            if (goldCost > Character.realGold && Character.bloodMagic.ritual[Index].progress <= 0)
            {
                if (Character.bloodMagic.ritual[Index].magic > 0)
                {
                    Character.bloodMagicController.bloodMagics[Index].removeAllMagic();
                }

                return true;
            }

            var cap = GetRitualCap(Index);
            SetInput(Math.Min(cap, MaxAllocation));
            Character.bloodMagicController.bloodMagics[Index].add();
            return true;
        }

        protected override bool CorrectResourceType()
        {
            return Type == ResourceType.Magic;
        }

        private long GetRitualCap(int index)
        {
            if (Character.settings.rebirthDifficulty == difficulty.normal)
            {
                var num = Math.Ceiling(50000.0 * Character.bloodMagicController.normalSpeedDividers[index] / (Character.totalMagicPower() * (double)Character.bloodMagicController.bloodMagics[index].totalBloodMagicSpeedBonus())) * 1.000002;
                if (num < 1.0)
                    num = 1.0;
                if (num > Character.hardCap())
                    num = Character.hardCap();
                var num2 = (long)(num / (long)Math.Ceiling((double)num / (double)Character.energyMagicPanel.energyMagicInput) * 1.00000202655792);
                return num2;
            }
            if (Character.settings.rebirthDifficulty == difficulty.evil)
            {
                var num = Math.Ceiling(50000.0 * Character.bloodMagicController.evilSpeedDividers[index] / (Character.totalMagicPower() * (double)Character.bloodMagicController.bloodMagics[index].totalBloodMagicSpeedBonus())) * 1.00000202655792;
                if (num < 1.0)
                    num = 1.0;
                if (num > Character.hardCap())
                    num = Character.hardCap();
                var num2 = (long)(num / (long)Math.Ceiling((double)num / (double)Character.energyMagicPanel.energyMagicInput) * 1.00000202655792);
                return num2;
            }
            if (Character.settings.rebirthDifficulty == difficulty.sadistic)
            {
                var num = Math.Ceiling(Character.bloodMagicController.bloodMagics[index].sadisticDivider() * (double)Character.bloodMagicController.sadisticSpeedDividers[index] / (Character.totalMagicPower() * (double)Character.bloodMagicController.bloodMagics[index].totalBloodMagicSpeedBonus())) * 1.00000202655792;
                if (num < 1.0)
                    num = 1.0;
                if (num > Character.hardCap())
                    num = Character.hardCap();
                var num2 = (long)(num / (long)Math.Ceiling((double)num / (double)Character.energyMagicPanel.energyMagicInput) * 1.00000202655792);
                return num2;
            }

            return 0;
        }
    }
}
