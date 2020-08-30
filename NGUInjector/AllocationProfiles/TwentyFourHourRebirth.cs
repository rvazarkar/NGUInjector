using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGUInjector.AllocationProfiles
{
    internal class TwentyFourHourRebirth : AllocationProfile
    {
        public override void AllocateEnergy()
        {
            var actualHours = Math.Floor(_character.rebirthTime.totalseconds / 60 / 60);
            if (actualHours < 1)
            {
                if (_character.idleEnergy > 0)
                {
                    _energyController.maxEnergy();
                    _character.timeMachineController.addEnergy();
                }
            }
            else if (actualHours < 2)
            {
                if (_character.idleEnergy > 0)
                {
                    _energyController.halfIdleEnergy();
                    _character.advancedTrainingController.attack.addEnergy();
                    _character.advancedTrainingController.defense.addEnergy();
                }
            }
            else
            {
                if (_character.idleEnergy > 0)
                {
                    _energyController.halfIdleEnergy();
                    _character.NGUController.NGU[4].add();
                    _character.NGUController.NGU[6].add();
                }
            }
        }

        private void CastRituals()
        {
            for (var i = _character.bloodMagic.ritual.Count-1; i >= 0; i--)
            {
                if (_character.magic.idleMagic == 0)
                    break;
                if (i >= _character.bloodMagicController.ritualsUnlocked())
                    continue;
                var goldCost = _character.bloodMagicController.bloodMagics[i].baseCost * _character.totalDiscount();
                if (goldCost > _character.realGold && _character.bloodMagic.ritual[i].progress <= 0.0)
                    continue;

                var tLeft = _character.bloodMagicController.bloodMagics[i].timeLeft();
                if (!tLeft.EndsWith("s"))
                {
                    if (tLeft.Count(x => x == ':') > 1)
                        continue;
                }

                _character.bloodMagicController.bloodMagics[i].cap();
            }
        }

        public override void AllocateMagic()
        {
            if (_character.rebirthTime.hours < 1)
            {
                if (_character.magic.idleMagic > 0)
                {
                    _energyController.maxMagic();
                    _character.timeMachineController.addMagic();
                }
            }
            else
            {
                _character.removeMostMagic();
                CastRituals();
            }
        }

        public override void EquipGear()
        {
            throw new NotImplementedException();
        }
    }
}
