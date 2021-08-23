using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGUInjector.AllocationProfiles.BreakpointTypes
{
    internal class BestAug : AugmentBP
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

                double time;
                double timeRemaining;
                double cost;
                float progress;
                if (_useUpgrades)
                {
                    time = Math.Max(aug.UpgradeTimeLeftEnergyMax((long)(MaxAllocation * upgRatio[i])), aug.AugTimeLeftEnergyMax((long)(MaxAllocation * augRatio[i])));
                    if (time < 0.01) { time = 0.01d; }
                    timeRemaining = aug.UpgradeTimeLeftEnergy((long)(MaxAllocation * upgRatio[i]));
                    cost = (double)Math.Max(1, 1d / time) * (double)aug.getUpgradeCost();
                    progress = aug.UpgradeProgress();
                }
                else
                {
                    time = aug.AugTimeLeftEnergyMax((long)(MaxAllocation));
                    if (time < 0.01) { time = 0.01d; }
                    timeRemaining = aug.AugTimeLeftEnergy((long)(MaxAllocation));
                    cost = (double)Math.Max(1, 1d / time) * (double)aug.getAugCost();
                    progress = aug.AugProgress();
                }

                if (cost > gold && (progress == 0f || timeRemaining < 10))
                {
                    continue;
                }

                if (time > 300) 
                {
                    continue;
                }

                if (RebirthTime > 0 && Main.Settings.AutoRebirth)
                    if (Character.rebirthTime.totalseconds - time < 0)
                        continue;

                //if (Index > 0)
                //    if (Character.rebirthTime.totalseconds + time > Index)
                //        continue;

                double value = AugmentValue(i);

                if (value / time > bestAugmentValue)
                {
                    bestAugment = i;
                    bestAugmentValue = value / time;
                }

                Main.LogAllocation($"Pair ID {i}: cost {cost} with {(_useUpgrades ? MaxAllocation * upgRatio[bestAugment] : MaxAllocation * augRatio[bestAugment])} of {MaxAllocation} energy for time {NumberOutput.timeOutput(time)} remaining {NumberOutput.timeOutput(timeRemaining)} - Value: {value} - ROI : {value / time}");
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
                    Main.LogAllocation($"BestAug: ({bestAugment}) @ {maxAllocation} using {augRatio[bestAugment]} : {alloc} and {upgRatio[bestAugment]} : {alloc2}");
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
                        
    }
}
