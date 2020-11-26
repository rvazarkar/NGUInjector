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
                return target != 0 && Character.augments.augs[AugIndex].augLevel >= target;
            }
            else
            {
                var target = Character.augments.augs[AugIndex].upgradeTarget;
                return target != 0 && Character.augments.augs[AugIndex].upgradeLevel >= target;
            }
        }

        internal override bool Allocate()
        {
            var alloc = CalculateAugCap();
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

        internal float CalculateAugCap()
        {
            var calcA = CalculateAugCapCalc(500);
            if (calcA.PPT < 1)
            {
                var calcB = CalculateAugCapCalc(calcA.GetOffset());
                return calcB.Num;
            }

            return calcA.Num;
        }

        internal CapCalc CalculateAugCapCalc(int offset)
        {
            var ret = new CapCalc
            {
                Num = 0,
                PPT = 1
            };
            double formula = 0;
            if (Index % 2 == 0)
            {
                formula = 50000 * (1f + Character.augments.augs[AugIndex].augLevel + offset) /
                    (Character.totalEnergyPower() *
                    (1 + Character.inventoryController.bonuses[specType.Augs]) *
                    Character.inventory.macguffinBonuses[12] *
                    Character.hacksController.totalAugSpeedBonus() *
                    Character.cardsController.getBonus(cardBonus.augSpeed) *
                    Character.adventureController.itopod.totalAugSpeedBonus() *
                    (1.0 + Character.allChallenges.noAugsChallenge.evilCompletions() * 0.0500000007450581));

                if (Character.allChallenges.noAugsChallenge.completions() >= 1)
                {
                    formula /= 1.10000002384186;
                }
                if (Character.allChallenges.noAugsChallenge.evilCompletions() >= Character.allChallenges.noAugsChallenge.maxCompletions)
                {
                    formula /= 1.25;
                }
                if (Character.settings.rebirthDifficulty >= difficulty.sadistic)
                {
                    formula *= Character.augmentsController.augments[AugIndex].sadisticDivider();
                }
                if (Character.settings.rebirthDifficulty == difficulty.normal)
                {
                    formula *= Character.augmentsController.normalAugSpeedDividers[AugIndex];
                }
                else if (Character.settings.rebirthDifficulty == difficulty.evil)
                {
                    formula *= Character.augmentsController.evilAugSpeedDividers[AugIndex];
                }
                else if (Character.settings.rebirthDifficulty == difficulty.sadistic)
                {
                    formula *= Character.augmentsController.sadisticAugSpeedDividers[AugIndex];
                }
            }
            else
            {
                formula = 50000 * (1f + Character.augments.augs[AugIndex].upgradeLevel + offset) /
                    (Character.totalEnergyPower() *
                    (1 + Character.inventoryController.bonuses[specType.Augs]) *
                    Character.inventory.macguffinBonuses[12] *
                    Character.hacksController.totalAugSpeedBonus() *
                    Character.cardsController.getBonus(cardBonus.augSpeed) *
                    Character.adventureController.itopod.totalAugSpeedBonus() *
                    (1.0 + Character.allChallenges.noAugsChallenge.evilCompletions() * 0.0500000007450581));

                if (Character.allChallenges.noAugsChallenge.completions() >= 1)
                {
                    formula /= 1.10000002384186;
                }
                if (Character.allChallenges.noAugsChallenge.evilCompletions() >= Character.allChallenges.noAugsChallenge.maxCompletions)
                {
                    formula /= 1.25;
                }
                if (Character.settings.rebirthDifficulty >= difficulty.sadistic)
                {
                    formula *= Character.augmentsController.augments[AugIndex].sadisticDivider();
                }
                if (Character.settings.rebirthDifficulty == difficulty.normal)
                {
                    formula *= Character.augmentsController.normalUpgradeSpeedDividers[AugIndex];

                }
                else if (Character.settings.rebirthDifficulty == difficulty.evil)
                {
                    formula *= Character.augmentsController.evilUpgradeSpeedDividers[AugIndex];

                }
                else if (Character.settings.rebirthDifficulty == difficulty.sadistic)
                {
                    formula *= Character.augmentsController.sadisticUpgradeSpeedDividers[AugIndex];
                }
            }

            if (formula >= Character.hardCap())
                formula = Character.hardCap();

            var num4 = formula <= 1.0 ? 1L : (long)formula;
            var num = (long)(num4 / (long)Math.Ceiling(num4 / (double) MaxAllocation) * 1.00000202655792);
            if (num + 1L <= long.MaxValue)
                ++num;
            if (num > Character.idleEnergy)
                num = Character.idleEnergy;
            if (num < 0L)
                num = 0L;
            var ppt = (double)num / num4;
            ret.Num = num;
            ret.PPT = ppt;
            return ret;
        }
    }
}
