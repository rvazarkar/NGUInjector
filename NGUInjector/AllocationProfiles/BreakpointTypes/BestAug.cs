using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGUInjector.AllocationProfiles.BreakpointTypes
{
    internal class BestAug : BaseBreakpoint
    {
        internal int RebirthTime { get; set; }
        private bool _useUpgrades;

        protected override bool Unlocked()
        {
            return Character.buttons.augmentation.interactable && !Character.challenges.noAugsChallenge.inChallenge;
        }

        protected override bool TargetMet()
        {
            return false;
        }

        internal override bool Allocate()
        {
            _useUpgrades = Character.bossID >= 37;
            AllocatePairs();
            return true;
        }

        private void AllocatePairs()
        {
            float[] augRatio = { 5 / 15f, 7 / 17f, 9 / 19f, 11 / 21f, 13 / 23f, 15 / 25f, 17 / 27f };
            float[] upgRatio = { 10 / 15f, 10 / 17f, 10 / 19f, 10 / 21f, 10 / 23f, 10 / 25f, 10 / 27f };
            var gold = Character.realGold;
            var bestAugment = -1;
            var bestAugmentValue = 0d;
            for (var i = 0; i < 7; i++)
            {
                var aug = Character.augmentsController.augments[i];
                if (aug.augLocked() || aug.hitAugmentTarget())
                    continue;


                if (_useUpgrades && aug.upgradeLocked() || aug.hitUpgradeTarget())
                    continue;

                var cost = aug.getAugCost();
                if (cost > gold)
                    continue;

                double time = aug.AugTimeLeftEnergyMax((long)(_useUpgrades ? MaxAllocation * augRatio[i] : MaxAllocation));

                if (time > 1200)
                    continue;

                if (RebirthTime > 0 && Main.Settings.AutoRebirth)
                    if (Character.rebirthTime.totalseconds - time < 0)
                        continue;

                if (Index > 0)
                    if (Character.rebirthTime.totalseconds + time > Index)
                        continue;

                if (time < 0.01) { time = 0.01d; }

                double value = AugmentValue(i);
                Main.LogAllocation($"Pair ID {i}: time {time} - Value: {value} - ROI : {value / time}");

                if (value / time > bestAugmentValue)
                {
                    bestAugment = i;
                    bestAugmentValue = value / time;
                }
            }
            if (bestAugment != -1)
            {
                var maxAllocation = (_useUpgrades ? MaxAllocation * augRatio[bestAugment] : MaxAllocation);
                var maxAllocationUpgrade = (_useUpgrades ? MaxAllocation * upgRatio[bestAugment] : MaxAllocation); ;
                var index = bestAugment * 2;
                var alloc = CalculateAugCap(index, maxAllocation);
                var alloc2 = CalculateAugCap(index + 1, maxAllocationUpgrade);
                SetInput(alloc);
                Character.augmentsController.augments[bestAugment].addEnergyAug();
                if (_useUpgrades)
                {
                    Main.LogAllocation($"BestAug: ({bestAugment}) @ {(_useUpgrades ? MaxAllocation / 2 : MaxAllocation)} using {augRatio[bestAugment]} : {alloc} and {upgRatio[bestAugment]} : {alloc2}");
                    SetInput(alloc2);
                    Character.augmentsController.augments[bestAugment].addEnergyUpgrade();
                }
            }
        }

        private double AugmentValue(int id)
        {
            var aug = Character.augmentsController.augments[id];
            if (_useUpgrades)
            {
                return (double)aug.baseBoost * Math.Max(Math.Pow(Character.augments.augs[aug.id].upgradeLevel + 2, 2f) + 1f, 1f) * Math.Pow(Character.augments.augs[aug.id].augLevel + 500f, aug.augTierBonus()) - aug.baseBoost * Math.Max(Math.Pow(Character.augments.augs[aug.id].upgradeLevel, 2f) + 1f, 1f) * Math.Pow(Character.augments.augs[aug.id].augLevel + 1f, (double)aug.augTierBonus());
            }
            return (double)aug.baseBoost * aug.getUpgradeBoost() * Math.Pow(Character.augments.augs[aug.id].augLevel + 500f, aug.augTierBonus()) - aug.baseBoost * aug.getUpgradeBoost() * Math.Pow(Character.augments.augs[aug.id].augLevel + 1f, (float)aug.augTierBonus());
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
            if (Index % 2 == 0)
            {
                augIndex = index / 2;
                formula = 50000 * (1f + Character.augments.augs[augIndex].augLevel + offset) /
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
                    formula *= Character.augmentsController.augments[augIndex].sadisticDivider();
                }
                if (Character.settings.rebirthDifficulty == difficulty.normal)
                {
                    formula *= Character.augmentsController.normalAugSpeedDividers[augIndex];
                }
                else if (Character.settings.rebirthDifficulty == difficulty.evil)
                {
                    formula *= Character.augmentsController.evilAugSpeedDividers[augIndex];
                }
                else if (Character.settings.rebirthDifficulty == difficulty.sadistic)
                {
                    formula *= Character.augmentsController.sadisticAugSpeedDividers[augIndex];
                }
            }
            else
            {
                augIndex = (index - 1) / 2;
                formula = 50000 * (1f + Character.augments.augs[augIndex].upgradeLevel + offset) /
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
                    formula *= Character.augmentsController.augments[augIndex].sadisticDivider();
                }
                if (Character.settings.rebirthDifficulty == difficulty.normal)
                {
                    formula *= Character.augmentsController.normalUpgradeSpeedDividers[augIndex];

                }
                else if (Character.settings.rebirthDifficulty == difficulty.evil)
                {
                    formula *= Character.augmentsController.evilUpgradeSpeedDividers[augIndex];

                }
                else if (Character.settings.rebirthDifficulty == difficulty.sadistic)
                {
                    formula *= Character.augmentsController.sadisticUpgradeSpeedDividers[augIndex];
                }
            }
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
            return ret;
        }
    }
}
