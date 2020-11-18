using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGUInjector.AllocationProfiles.BreakpointTypes
{
    internal class TimeMachineBP : BaseBreakpoint
    {
        protected override bool Unlocked()
        {
            return Character.buttons.brokenTimeMachine.interactable;
        }

        protected override bool TargetMet()
        {
            var target = Type == ResourceType.Energy ? Character.machine.speedTarget : Character.machine.multiTarget;
            var level = Type == ResourceType.Energy ? Character.machine.levelSpeed : Character.machine.levelGoldMulti;

            return target > 0 && level >= target;
        }

        internal override bool Allocate()
        {
            if (Type == ResourceType.Energy)
            {
                AllocateEnergy();
            }
            else
            {
                AllocateMagic();
            }
            return true;
        }

        private void AllocateEnergy()
        {
            var toAllocate = CalculateTMEnergyCap();
            SetInput(toAllocate);
            Character.timeMachineController.addEnergy();
        }

        private void AllocateMagic()
        {
            var toAllocate = CalculateTMMagicCap();
            SetInput(toAllocate);
            Character.timeMachineController.addMagic();
        }

        protected override bool CorrectResourceType()
        {
            return Type == ResourceType.Energy || Type == ResourceType.Magic;
        }

        private float CalculateTMMagicCap()
        {
            var calcA = CalculateMagicTM(500);
            if (calcA.PPT < 1)
            {
                var calcB = CalculateMagicTM(calcA.GetOffset());
                return calcB.Num;
            }

            return calcA.Num;
        }

        private float CalculateTMEnergyCap()
        {
            var calcA = CalculateEnergyTM(500);
            if (calcA.PPT < 1)
            {
                var calcB = CalculateEnergyTM(calcA.GetOffset());
                return calcB.Num;
            }

            return calcA.Num;
        }

        #region Hidden
        private CapCalc CalculateEnergyTM(int offset)
        {
            var ret = new CapCalc
            {
                Num = 0,
                PPT = 1
            };
            var formula = 50000 * Character.timeMachineController.baseSpeedDivider() * (1f + Character.machine.levelSpeed + offset) / (
                Character.totalEnergyPower() * Character.hacksController.totalTMSpeedBonus() *
                Character.allChallenges.timeMachineChallenge.TMSpeedBonus() *
                Character.cardsController.getBonus(cardBonus.TMSpeed));

            if (Character.settings.rebirthDifficulty >= difficulty.sadistic)
            {
                formula *= Character.timeMachineController.sadisticDivider();
            }

            if (formula >= Character.hardCap())
                formula = Character.hardCap();

            var num4 = formula <= 1.0 ? 1L : (long)formula;
            var num = (long)(num4 / (long)Math.Ceiling(num4 / (double)MaxAllocation) * 1.00000202655792);
            if (num + 1L <= long.MaxValue)
                ++num;
            if (num > Character.idleEnergy)
                num = Character.idleEnergy;
            if (num < 0L)
                num = 0L;

            ret.Num = num;
            ret.PPT = (double)num / num4;
            return ret;
        }

        private CapCalc CalculateMagicTM(int offset)
        {
            var ret = new CapCalc
            {
                Num = 0,
                PPT = 1
            };
            var formula = 50000 * Character.timeMachineController.baseGoldMultiDivider() *
                (1f + Character.machine.levelGoldMulti + offset) / (
                    Character.totalMagicPower() * Character.hacksController.totalTMSpeedBonus() *
                    Character.allChallenges.timeMachineChallenge.TMSpeedBonus() *
                    Character.cardsController.getBonus(cardBonus.TMSpeed));

            if (Character.settings.rebirthDifficulty >= difficulty.sadistic)
            {
                formula *= Character.timeMachineController.sadisticDivider();
            }

            if (formula >= Character.hardCap())
                formula = Character.hardCap();


            var num4 = formula <= 1.0 ? 1L : (long)formula;
            var num = (long)(num4 / (long)Math.Ceiling(num4 / (double)MaxAllocation) * 1.00000202655792);
            if (num + 1L <= long.MaxValue)
                ++num;
            if (num > Character.magic.idleMagic)
                num = Character.magic.idleMagic;
            if (num < 0L)
                num = 0L;
            ret.Num = num;
            ret.PPT = (double)num / num4;
            return ret;
        }


        #endregion

    }
}
