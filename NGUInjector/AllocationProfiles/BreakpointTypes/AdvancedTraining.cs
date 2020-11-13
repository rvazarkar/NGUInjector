using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NGUInjector.AllocationProfiles.BreakpointTypes
{
    internal class AdvancedTraining : BaseBreakpoint
    {
        protected override bool Unlocked()
        {
            return Index <= 4 && Character.buttons.advancedTraining.interactable;
        }

        protected override bool TargetMet()
        {
            return Character.advancedTraining.levelTarget[Index] != 0 && Character.advancedTraining.level[Index] >=
                Character.advancedTraining.levelTarget[Index];
        }

        internal override bool Allocate()
        {
            SetInput(CalculateATCap());
            switch (Index)
            {
                case 0:
                    Character.advancedTrainingController.defense.addEnergy();
                    break;
                case 1:
                    Character.advancedTrainingController.attack.addEnergy();
                    break;
                case 2:
                    Character.advancedTrainingController.block.addEnergy();
                    break;
                case 3:
                    Character.advancedTrainingController.wandoosEnergy.addEnergy();
                    break;
                case 4:
                    Character.advancedTrainingController.wandoosMagic.addEnergy();
                    break;
            }

            return true;
        }

        protected override bool CorrectResourceType()
        {
            return Type == ResourceType.Energy;
        }

        private float CalculateATCap()
        {
            var calcA = CalculateATCap(500);
            if (calcA.PPT < 1)
            {
                var calcB = CalculateATCap(calcA.GetOffset());
                return calcB.Num;
            }

            return calcA.Num;
        }

        private CapCalc CalculateATCap(int offset)
        {
            var ret = new CapCalc
            {
                Num = 0,
                PPT = 1
            };
            var divisor = GetDivisor(Index, offset);
            if (divisor == 0.0)
                return ret;

            if (Character.wishes.wishes[190].level >= 1)
                return ret;

            var formula = 50f * divisor /
                          (Mathf.Sqrt(Character.totalEnergyPower()) * Character.totalAdvancedTrainingSpeedBonus());

            if (formula >= Character.hardCap())
            {
                formula = Character.hardCap();
            }

            var num = (long)(formula / (long)Math.Ceiling(formula / (double)MaxAllocation) * 1.00000202655792);

            if (num + 1L <= long.MaxValue)
                ++num;
            if (num > Character.idleEnergy)
                num = Character.idleEnergy;
            if (num < 0L)
                num = 0L;

            ret.Num = num;
            ret.PPT = (double)num / formula;
            return ret;
        }

        private float GetDivisor(int index, int offset)
        {
            float baseTime;
            switch (index)
            {
                case 0:
                    baseTime = Character.advancedTrainingController.defense.baseTime;
                    break;
                case 1:
                    baseTime = Character.advancedTrainingController.attack.baseTime;
                    break;
                case 2:
                    baseTime = Character.advancedTrainingController.block.baseTime;
                    break;
                case 3:
                    baseTime = Character.advancedTrainingController.wandoosEnergy.baseTime;
                    break;
                case 4:
                    baseTime = Character.advancedTrainingController.wandoosMagic.baseTime;
                    break;
                default:
                    baseTime = 0.0f;
                    break;
            }

            return baseTime * (Character.advancedTraining.level[index] + offset + 1f);
        }
    }
}
