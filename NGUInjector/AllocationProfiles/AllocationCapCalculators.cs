using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NGUInjector.AllocationProfiles
{
    internal class AllocationCapCalculators
    {
        private readonly Character _character;
        public AllocationCapCalculators(Character character)
        {
            _character = character;
        }

        internal CapCalc GetNGUEnergyCapCalc(int id, bool useInput, int offset)
        {
            var ret = new CapCalc
            {
                Num = 0,
                PPT = 1
            };
            var num1 = 0.0f;
            if (_character.settings.nguLevelTrack == difficulty.normal)
                num1 = _character.NGU.skills[id].level + 1L + offset;
            else if (_character.settings.nguLevelTrack == difficulty.evil)
                num1 = _character.NGU.skills[id].evilLevel + 1L + offset;
            else if (_character.settings.nguLevelTrack == difficulty.sadistic)
                num1 = _character.NGU.skills[id].sadisticLevel + 1L + offset;

            var num2 = _character.totalEnergyPower() * (double)_character.totalNGUSpeedBonus() * _character.adventureController.itopod.totalEnergyNGUBonus() * _character.inventory.macguffinBonuses[4] * _character.NGUController.energyNGUBonus() * _character.allDiggers.totalEnergyNGUBonus() * _character.hacksController.totalEnergyNGUBonus() * _character.beastQuestPerkController.totalEnergyNGUSpeed() * _character.wishesController.totalEnergyNGUSpeed() * _character.cardsController.getBonus(cardBonus.energyNGUSpeed);
            if (_character.allChallenges.trollChallenge.sadisticCompletions() >= 1)
                num2 *= 3.0;
            if (_character.settings.nguLevelTrack >= difficulty.sadistic)
                num2 /= _character.NGUController.NGU[0].sadisticDivider();
            var num3 = _character.NGUController.energySpeedDivider(id) * (double)num1 / num2;
            if (num3 >= _character.hardCap())
            {
                ret.Num = _character.hardCap();
                return ret;
            }

            var num4 = num3 <= 1.0 ? 1L : (long)num3;
            var num = (long)(num4 / (long)Math.Ceiling(num4 / (useInput ? (double)_character.energyMagicPanel.energyMagicInput : (double)_character.idleEnergy)) * 1.00000202655792);
            if (num + 1L <= long.MaxValue)
                ++num;
            if (num > _character.idleEnergy)
                num = _character.idleEnergy;
            if (num < 0L)
                num = 0;

            var ppt = (double)num / num4;
            ret.Num = num;
            ret.PPT = ppt;
            return ret;
        }

        internal float CalculateNGUEnergyCap(int id, bool useInput)
        {
            var calcA = GetNGUEnergyCapCalc(id, useInput, 500);
            if (calcA.PPT < 1)
            {
                var calcB = GetNGUEnergyCapCalc(id, useInput, calcA.GetOffset());
                return calcB.Num;
            }

            return calcA.Num;
        }

        internal CapCalc GetNGUMagicCapCalc(int id, bool useInput, int offset)
        {
            var ret = new CapCalc
            {
                Num = 0,
                PPT = 1
            };
            var num1 = 0.0f;
            if (_character.settings.nguLevelTrack == difficulty.normal)
                num1 = _character.NGU.magicSkills[id].level + 1L + offset;
            else if (_character.settings.nguLevelTrack == difficulty.evil)
                num1 = _character.NGU.magicSkills[id].evilLevel + 1L + offset;
            else if (_character.settings.nguLevelTrack == difficulty.sadistic)
                num1 = _character.NGU.magicSkills[id].sadisticLevel + 1L + offset;

            var num2 = _character.totalMagicPower() * (double)_character.totalNGUSpeedBonus() * _character.adventureController.itopod.totalMagicNGUBonus() * _character.inventory.macguffinBonuses[5] * _character.NGUController.magicNGUBonus() * _character.allDiggers.totalMagicNGUBonus() * _character.hacksController.totalMagicNGUBonus() * _character.beastQuestPerkController.totalMagicNGUSpeed() * _character.wishesController.totalMagicNGUSpeed() * _character.cardsController.getBonus(cardBonus.magicNGUSpeed);
            if (_character.allChallenges.trollChallenge.completions() >= 1)
                num2 *= 3.0;
            if (_character.settings.nguLevelTrack >= difficulty.sadistic)
                num2 /= _character.NGUController.NGUMagic[0].sadisticDivider();
            var num3 = _character.NGUController.magicSpeedDivider(id) * (double)num1 / num2;
            if (num3 >= _character.hardCap())
            {
                ret.Num = _character.hardCap();
                return ret;
            }
            var num4 = num3 <= 1.0 ? 1L : (long)num3;
            var num = (long)(num4 / (long)Math.Ceiling(num4 / (useInput ? (double)_character.energyMagicPanel.energyMagicInput : (double)_character.magic.idleMagic)) * 1.00000202655792);
            if (num + 1L <= long.MaxValue)
                ++num;
            if (num > _character.magic.idleMagic)
                num = _character.magic.idleMagic;
            if (num < 0L)
                num = 0L;

            var ppt = (double)num / num4;
            ret.Num = num;
            ret.PPT = ppt;
            return ret;
        }

        internal float CalculateNGUMagicCap(int id, bool useInput)
        {
            var calcA = GetNGUMagicCapCalc(id, useInput, 500);
            if (calcA.PPT < 1)
            {
                var calcB = GetNGUMagicCapCalc(id, useInput, calcA.GetOffset());
                return calcB.Num;
            }

            return calcA.Num;
        }

        internal float CalculateAugCap(int index, bool useInput)
        {
            var calcA = CalculateAugCapCalc(index, useInput, 500);
            if (calcA.PPT < 1)
            {
                var calcB = CalculateAugCapCalc(index, useInput, calcA.GetOffset());
                return calcB.Num;
            }

            return calcA.Num;
        }

        internal CapCalc CalculateAugCapCalc(int index, bool useInput, int offset)
        {
            var ret = new CapCalc
            {
                Num = 0,
                PPT = 1
            };
            var augIndex = (int)Math.Floor((double)(index / 2));
            double formula = 0;
            if (index % 2 == 0)
            {
                formula = 50000 * (1f + _character.augments.augs[augIndex].augLevel + offset) /
                    (_character.totalEnergyPower() *
                    (1 + _character.inventoryController.bonuses[specType.Augs]) *
                    _character.inventory.macguffinBonuses[12] *
                    _character.hacksController.totalAugSpeedBonus() *
                    _character.cardsController.getBonus(cardBonus.augSpeed) *
                    _character.adventureController.itopod.totalAugSpeedBonus() *
                    (1.0 + _character.allChallenges.noAugsChallenge.evilCompletions() * 0.0500000007450581));

                if (_character.allChallenges.noAugsChallenge.completions() >= 1)
                {
                    formula /= 1.10000002384186;
                }
                if (_character.allChallenges.noAugsChallenge.evilCompletions() >= _character.allChallenges.noAugsChallenge.maxCompletions)
                {
                    formula /= 1.25;
                }
                if (_character.settings.rebirthDifficulty >= difficulty.sadistic)
                {
                    formula *= _character.augmentsController.augments[augIndex].sadisticDivider();
                }
                if (_character.settings.rebirthDifficulty == difficulty.normal)
                {
                    formula *= _character.augmentsController.normalAugSpeedDividers[augIndex];
                }
                else if (_character.settings.rebirthDifficulty == difficulty.evil)
                {
                    formula *= _character.augmentsController.evilAugSpeedDividers[augIndex];
                }
                else if (_character.settings.rebirthDifficulty == difficulty.sadistic)
                {
                    formula *= _character.augmentsController.sadisticAugSpeedDividers[augIndex];
                }
            }
            else
            {
                formula = 50000 * (1f + _character.augments.augs[augIndex].upgradeLevel + offset) /
                    (_character.totalEnergyPower() *
                    (1 + _character.inventoryController.bonuses[specType.Augs]) *
                    _character.inventory.macguffinBonuses[12] *
                    _character.hacksController.totalAugSpeedBonus() *
                    _character.cardsController.getBonus(cardBonus.augSpeed) *
                    _character.adventureController.itopod.totalAugSpeedBonus() *
                    (1.0 + _character.allChallenges.noAugsChallenge.evilCompletions() * 0.0500000007450581));

                if (_character.allChallenges.noAugsChallenge.completions() >= 1)
                {
                    formula /= 1.10000002384186;
                }
                if (_character.allChallenges.noAugsChallenge.evilCompletions() >= _character.allChallenges.noAugsChallenge.maxCompletions)
                {
                    formula /= 1.25;
                }
                if (_character.settings.rebirthDifficulty >= difficulty.sadistic)
                {
                    formula *= _character.augmentsController.augments[augIndex].sadisticDivider();
                }
                if (_character.settings.rebirthDifficulty == difficulty.normal)
                {
                    formula *= _character.augmentsController.normalUpgradeSpeedDividers[augIndex];

                }
                else if (_character.settings.rebirthDifficulty == difficulty.evil)
                {
                    formula *= _character.augmentsController.evilUpgradeSpeedDividers[augIndex];

                }
                else if (_character.settings.rebirthDifficulty == difficulty.sadistic)
                {
                    formula *= _character.augmentsController.sadisticUpgradeSpeedDividers[augIndex];
                }
            }

            if (formula >= _character.hardCap())
            {
                ret.Num = _character.hardCap();
                return ret;
            }
            var num4 = formula <= 1.0 ? 1L : (long)formula;
            var num = (long)(num4 / (long)Math.Ceiling(num4 / (useInput ? (double)_character.energyMagicPanel.energyMagicInput : (double)_character.idleEnergy)) * 1.00000202655792);
            if (num + 1L <= long.MaxValue)
                ++num;
            if (num > _character.idleEnergy)
                num = _character.idleEnergy;
            if (num < 0L)
                num = 0L;
            var ppt = (double)num / num4;
            ret.Num = num;
            ret.PPT = ppt;
            return ret;
        }



        internal CapCalc CalculateTMEnergyCapCalc(bool useInput, int offset)
        {
            var ret = new CapCalc
            {
                Num = 0,
                PPT = 1
            };
            var formula = 50000 * _character.timeMachineController.baseSpeedDivider() * (1f + _character.machine.levelSpeed + offset) / (
                _character.totalEnergyPower() * _character.hacksController.totalTMSpeedBonus() *
                _character.allChallenges.timeMachineChallenge.TMSpeedBonus() *
                _character.cardsController.getBonus(cardBonus.TMSpeed));

            if (_character.settings.rebirthDifficulty >= difficulty.sadistic)
            {
                formula *= _character.timeMachineController.sadisticDivider();
            }

            if (formula >= _character.hardCap())
            {
                ret.Num = _character.hardCap();
                return ret;
            }

            var num4 = formula <= 1.0 ? 1L : (long)formula;
            var num = (long)(num4 / (long)Math.Ceiling(num4 / (useInput ? (double)_character.energyMagicPanel.energyMagicInput : (double)_character.idleEnergy)) * 1.00000202655792);
            if (num + 1L <= long.MaxValue)
                ++num;
            if (num > _character.idleEnergy)
                num = _character.idleEnergy;
            if (num < 0L)
                num = 0L;

            ret.Num = num;
            ret.PPT = (double)num / num4;
            return ret;
        }

        internal float CalculateTMMagicCap(bool useInput)
        {
            var calcA = CalculateTMMagicCapCalc(useInput, 500);
            if (calcA.PPT < 1)
            {
                var calcB = CalculateTMMagicCapCalc(useInput, calcA.GetOffset());
                return calcB.Num;
            }

            return calcA.Num;
        }

        internal float CalculateTMEnergyCap(bool useInput)
        {
            var calcA = CalculateTMEnergyCapCalc(useInput, 500);
            if (calcA.PPT < 1)
            {
                var calcB = CalculateTMEnergyCapCalc(useInput, calcA.GetOffset());
                return calcB.Num;
            }

            return calcA.Num;
        }

        internal CapCalc CalculateTMMagicCapCalc(bool useInput, int offset)
        {
            var ret = new CapCalc
            {
                Num = 0,
                PPT = 1
            };
            var formula = 50000 * _character.timeMachineController.baseGoldMultiDivider() *
                (1f + _character.machine.levelGoldMulti + offset) / (
                    _character.totalMagicPower() * _character.hacksController.totalTMSpeedBonus() *
                    _character.allChallenges.timeMachineChallenge.TMSpeedBonus() *
                    _character.cardsController.getBonus(cardBonus.TMSpeed));

            if (_character.settings.rebirthDifficulty >= difficulty.sadistic)
            {
                formula *= _character.timeMachineController.sadisticDivider();
            }

            if (formula >= _character.hardCap())
            {
                ret.Num = _character.hardCap();
                return ret;
            }


            var num4 = formula <= 1.0 ? 1L : (long)formula;
            var num = (long)(num4 / (long)Math.Ceiling(num4 / (useInput ? (double)_character.energyMagicPanel.energyMagicInput : (double)_character.magic.idleMagic)) * 1.00000202655792);
            if (num + 1L <= long.MaxValue)
                ++num;
            if (num > _character.magic.idleMagic)
                num = _character.magic.idleMagic;
            if (num < 0L)
                num = 0L;
            ret.Num = num;
            ret.PPT = (double)num / num4;
            return ret;
        }

        private float GetDivisor(int index, int offset)
        {
            float baseTime;
            switch (index)
            {
                case 0:
                    baseTime = _character.advancedTrainingController.defense.baseTime;
                    break;
                case 1:
                    baseTime = _character.advancedTrainingController.attack.baseTime;
                    break;
                case 2:
                    baseTime = _character.advancedTrainingController.block.baseTime;
                    break;
                case 3:
                    baseTime = _character.advancedTrainingController.wandoosEnergy.baseTime;
                    break;
                case 4:
                    baseTime = _character.advancedTrainingController.wandoosMagic.baseTime;
                    break;
                default:
                    baseTime = 0.0f;
                    break;
            }

            return baseTime * (_character.advancedTraining.level[index] + offset + 1f);
        }

        internal float CalculateATCap(int index, bool useInput)
        {
            var calcA = CalculateATCap(index, useInput, 500);
            if (calcA.PPT < 1)
            {
                var calcB = CalculateATCap(index, useInput, calcA.GetOffset());
                return calcB.Num;
            }

            return calcA.Num;
        }

        private CapCalc CalculateATCap(int index, bool useInput, int offset)
        {
            var ret = new CapCalc
            {
                Num = 0,
                PPT = 1
            };
            var divisor = GetDivisor(index, offset);
            if (divisor == 0.0)
                return ret;

            if (_character.wishes.wishes[190].level >= 1)
                return ret;

            var formula = 50f * divisor /
                          (Mathf.Sqrt(_character.totalEnergyPower()) * _character.totalAdvancedTrainingSpeedBonus());

            if (formula >= _character.hardCap())
            {
                ret.Num = _character.hardCap();
                return ret;
            }

            var num = (long)(formula / (long)Math.Ceiling(formula / (useInput ? (double)_character.energyMagicPanel.energyMagicInput : (double)_character.idleEnergy)) * 1.00000202655792);

            if (num + 1L <= long.MaxValue)
                ++num;
            if (num > _character.idleEnergy)
                num = _character.idleEnergy;
            if (num < 0L)
                num = 0L;

            ret.Num = num;
            ret.PPT = (double)num / formula;
            return ret;
        }

        internal long GetRitualCap(int index)
        {

            if (_character.settings.rebirthDifficulty == difficulty.normal)
            {
                var num = Math.Ceiling(50000.0 * _character.bloodMagicController.normalSpeedDividers[index] / (_character.totalMagicPower() * (double)_character.bloodMagicController.bloodMagics[index].totalBloodMagicSpeedBonus())) * 1.000002;
                if (num < 1.0)
                    num = 1.0;
                if (num > _character.hardCap())
                    num = _character.hardCap();
                return (long)num;
            }
            if (_character.settings.rebirthDifficulty == difficulty.evil)
            {
                var num = Math.Ceiling(50000.0 * _character.bloodMagicController.evilSpeedDividers[index] / (_character.totalMagicPower() * (double)_character.bloodMagicController.bloodMagics[index].totalBloodMagicSpeedBonus())) * 1.00000202655792;
                if (num < 1.0)
                    num = 1.0;
                if (num > _character.hardCap())
                    num = _character.hardCap();
                return (long)num;
            }
            if (_character.settings.rebirthDifficulty == difficulty.sadistic)
            {
                var num = Math.Ceiling(_character.bloodMagicController.bloodMagics[index].sadisticDivider() * (double)_character.bloodMagicController.sadisticSpeedDividers[index] / (_character.totalMagicPower() * (double)_character.bloodMagicController.bloodMagics[index].totalBloodMagicSpeedBonus())) * 1.00000202655792;
                if (num < 1.0)
                    num = 1.0;
                if (num > _character.hardCap())
                    num = _character.hardCap();
                return (long)num;
            }
            var num1 = (double)(long)(Math.Ceiling(50000.0 * _character.bloodMagicController.normalSpeedDividers[index] / (_character.totalMagicPower() * (double)_character.bloodMagicController.bloodMagics[index].totalBloodMagicSpeedBonus())) * 1.00000202655792);
            if (num1 < 1.0)
                num1 = 1.0;
            if (num1 > _character.hardCap())
                num1 = _character.hardCap();

            var num2 = (long)(num1 / (long)Math.Ceiling((double)num1 / (double)_character.energyMagicPanel.energyMagicInput) * 1.00000202655792);
            return num2;
        }
    }

    internal class CapCalc
    {
        internal double PPT { get; set; }
        internal long Num { get; set; }

        internal int GetOffset()
        {
            return (int)Math.Floor(PPT * 50 * 10);
        }
    }
}
