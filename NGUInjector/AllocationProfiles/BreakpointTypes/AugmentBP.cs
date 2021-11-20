using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGUInjector.AllocationProfiles.BreakpointTypes
{
    internal class AugmentBP : BaseBreakpoint
    {
        private int AugIndex => (int)Math.Floor((double)(Index / 2));

        protected override bool Unlocked()
        {
            if (!Character.buttons.augmentation.interactable)
                return false;

            if (Character.challenges.noAugsChallenge.inChallenge)
                return false;

            if (Index > 13)
                return false;


            if (Index % 2 == 0)
            {
                return Character.bossID > Character.augmentsController.augments[AugIndex].augBossRequired;
            }

            return Character.bossID > Character.augmentsController.augments[AugIndex].upgradeBossRequired;
        }

        protected override bool TargetMet()
        {
            if (Index % 2 == 0)
            {
                var target = Character.augments.augs[AugIndex].augmentTarget;
                return target == -1 || target != 0 && Character.augments.augs[AugIndex].augLevel >= target;
            }
            else
            {
                var target = Character.augments.augs[AugIndex].upgradeTarget;
                return target == -1 || target != 0 && Character.augments.augs[AugIndex].upgradeLevel >= target;
            }
        }

        internal override bool Allocate()
        {
            var alloc = CalculateAugCap(this.Index, MaxAllocation);
            SetInput(alloc);
            if (Index % 2 == 0)
            {
                Character.augmentsController.augments[AugIndex].addEnergyAug();
            }
            else
            {
                Character.augmentsController.augments[AugIndex].addEnergyUpgrade();
            }
            return true;
        }

        protected override bool CorrectResourceType()
        {
            return Type == ResourceType.Energy;
        }

        internal float CalculateAugCap(int index, float allocation)
        {
            var calcA = CalculateAugCapCalc(500, index, allocation);
            if (calcA.PPT < 1)
            {
                var calcB = CalculateAugCapCalc(calcA.GetOffset(), index, allocation);
                return calcB.Num;
            }

            return calcA.Num;
        }

        internal CapCalc CalculateAugCapCalc(int offset, int index, float allocation)
        {
            int augIndex;
            var ret = new CapCalc
            {
                Num = 0,
                PPT = 1
            };
            double formula = 0;
            double num1 = 1D;
            if (index % 2 == 0)
            {
                augIndex = index / 2;
                if (Character.settings.rebirthDifficulty == difficulty.normal)
                {
                    num1 = (Character.totalEnergyPower() / 50000D / Character.augmentsController.normalAugSpeedDividers[augIndex] / (Character.augments.augs[augIndex].augLevel + 1D + offset));
                }
                else if (Character.settings.rebirthDifficulty == difficulty.evil)
                {
                    num1 = (Character.totalEnergyPower() / 50000D / Character.augmentsController.evilAugSpeedDividers[augIndex] / (Character.augments.augs[augIndex].augLevel + 1D + offset));
                }
                else if (Character.settings.rebirthDifficulty == difficulty.sadistic)
                {
                    num1 = (Character.totalEnergyPower() / Character.augmentsController.sadisticAugSpeedDividers[augIndex] / (Character.augments.augs[augIndex].augLevel + 1D + offset));
                }
            }
            else
            {
                augIndex = (index - 1) / 2;
                if (Character.settings.rebirthDifficulty == difficulty.normal)
                {
                    num1 = (Character.totalEnergyPower() / 50000D / Character.augmentsController.normalUpgradeSpeedDividers[augIndex] / (Character.augments.augs[augIndex].upgradeLevel + 1D + offset));
                }
                else if (Character.settings.rebirthDifficulty == difficulty.evil)
                {
                    num1 = (Character.totalEnergyPower() / 50000f / Character.augmentsController.evilUpgradeSpeedDividers[augIndex] / (Character.augments.augs[augIndex].upgradeLevel + 1D + offset));
                }
                else if (Character.settings.rebirthDifficulty == difficulty.sadistic)
                {
                    num1 = (Character.totalEnergyPower() / Character.augmentsController.sadisticUpgradeSpeedDividers[augIndex] / (Character.augments.augs[augIndex].upgradeLevel + 1D + offset));
                }
            }
            num1 *= (1D + Character.inventoryController.bonuses[specType.Augs]);
            num1 *= Character.inventory.macguffinBonuses[12];
            num1 *= Character.hacksController.totalAugSpeedBonus();
            num1 *= Character.adventureController.itopod.totalAugSpeedBonus();
            num1 *= Character.cardsController.getBonus(cardBonus.augSpeed);
            num1 *= (1D + Character.allChallenges.noAugsChallenge.evilCompletions() * 0.05D);
            if (Character.allChallenges.noAugsChallenge.completions() >= 1)
            {
                num1 *= 1.1000000238418579;
            }
            if (Character.allChallenges.noAugsChallenge.evilCompletions() >= Character.allChallenges.noAugsChallenge.maxCompletions)
            {
                num1 *= 1.25;
            }
            if (Character.settings.rebirthDifficulty >= difficulty.sadistic)
            {
                num1 /= Character.augmentsController.augments[augIndex].sadisticDivider();
            }
            formula = 1 / num1;
            if (formula >= Character.hardCap())
                formula = Character.hardCap();
            var num4 = formula <= 1.0 ? 1L : (long)formula;
            var num = (long)(num4 / (long)Math.Ceiling(num4 / (double)allocation) * 1.00000202655792);
            if (num + 1L <= long.MaxValue)
                ++num;
            if (num > Character.idleEnergy)
                num = Character.idleEnergy;
            if (num < 0L)
                num = 0L;
            var ppt = (double)num / num4;
            ret.Num = num;
            ret.PPT = ppt;
            Main.LogAllocation($"Returning from Index : {index} : {ret.Num} of {allocation} vs {ret.PPT} BB");
            return ret;
        }
    }
}
