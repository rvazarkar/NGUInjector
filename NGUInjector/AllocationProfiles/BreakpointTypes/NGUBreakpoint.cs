using System;

namespace NGUInjector.AllocationProfiles.BreakpointTypes
{
    internal class NGUBreakpoint : BaseBreakpoint
    {
        protected override bool Unlocked()
        {
            switch (Type)
            {
                case ResourceType.Magic when Index > 6:
                case ResourceType.Energy when Index > 8:
                    return false;
                default:
                    return Character.buttons.ngu.interactable;
            }
        }

        protected override bool TargetMet()
        {
            var track = Character.settings.nguLevelTrack;
            var ngus = Type == ResourceType.Energy ? Character.NGU.skills : Character.NGU.magicSkills;
            long target;
            long level;
            switch (track)
            {
                case difficulty.normal:
                    target = ngus[Index].target;
                    level = ngus[Index].level;
                    break;
                case difficulty.evil:
                    target = ngus[Index].evilTarget;
                    level = ngus[Index].evilLevel;
                    break;
                default:
                    target = ngus[Index].sadisticTarget;
                    level = ngus[Index].sadisticLevel;
                    break;
            }

            return target > 0 && level >= target;
        }

        internal override bool Allocate()
        {
            if (Type == ResourceType.Energy)
                AllocateEnergy();
            else
                AllocateMagic();

            return true;
        }

        private void AllocateMagic()
        {
            var alloc = CalculateNGUMagicCap();
            SetInput(alloc);
            Character.NGUController.NGUMagic[Index].add();
        }

        private void AllocateEnergy()
        {
            var alloc = CalculateNGUEnergyCap();
            SetInput(alloc);
            Character.NGUController.NGU[Index].add();
        }

        protected override bool CorrectResourceType()
        {
            return Type == ResourceType.Energy || Type == ResourceType.Magic;
        }

        private float CalculateNGUEnergyCap()
        {
            var calcA = GetNGUEnergyCapCalc(500);
            if (calcA.PPT < 1)
            {
                var calcB = GetNGUEnergyCapCalc(calcA.GetOffset());
                return calcB.Num;
            }

            return calcA.Num;
        }

        internal float CalculateNGUMagicCap()
        {
            var calcA = GetNGUMagicCapCalc(500);
            if (calcA.PPT < 1)
            {
                var calcB = GetNGUMagicCapCalc(calcA.GetOffset());
                return calcB.Num;
            }

            return calcA.Num;
        }

        internal CapCalc GetNGUEnergyCapCalc(int offset)
        {
            var ret = new CapCalc
            {
                Num = 0,
                PPT = 1
            };
            var num1 = 0.0f;
            if (Character.settings.nguLevelTrack == difficulty.normal)
                num1 = Character.NGU.skills[Index].level + 1L + offset;
            else if (Character.settings.nguLevelTrack == difficulty.evil)
                num1 = Character.NGU.skills[Index].evilLevel + 1L + offset;
            else if (Character.settings.nguLevelTrack == difficulty.sadistic)
                num1 = Character.NGU.skills[Index].sadisticLevel + 1L + offset;

            var num2 = Character.totalEnergyPower() * (double)Character.totalNGUSpeedBonus() * Character.adventureController.itopod.totalEnergyNGUBonus() * Character.inventory.macguffinBonuses[4] * Character.NGUController.energyNGUBonus() * Character.allDiggers.totalEnergyNGUBonus() * Character.hacksController.totalEnergyNGUBonus() * Character.beastQuestPerkController.totalEnergyNGUSpeed() * Character.wishesController.totalEnergyNGUSpeed() * Character.cardsController.getBonus(cardBonus.energyNGUSpeed);
            if (Character.allChallenges.trollChallenge.sadisticCompletions() >= 1)
                num2 *= 3.0;
            if (Character.settings.nguLevelTrack >= difficulty.sadistic)
                num2 /= Character.NGUController.NGU[0].sadisticDivider();
            var num3 = Character.NGUController.energySpeedDivider(Index) * (double)num1 / num2;
            if (num3 >= Character.hardCap())
                num3 = Character.hardCap();

            var num4 = num3 <= 1.0 ? 1L : (long)num3;
            var num = (long)(num4 / (long)Math.Ceiling(num4 / (double)MaxAllocation) * 1.00000202655792);
            if (num + 1L <= long.MaxValue)
                ++num;
            if (num > Character.idleEnergy)
                num = Character.idleEnergy;
            if (num < 0L)
                num = 0;

            var ppt = (double)num / num4;
            ret.Num = num;
            ret.PPT = ppt;
            return ret;
        }

        

        internal CapCalc GetNGUMagicCapCalc(int offset)
        {
            var ret = new CapCalc
            {
                Num = 0,
                PPT = 1
            };
            var num1 = 0.0f;
            if (Character.settings.nguLevelTrack == difficulty.normal)
                num1 = Character.NGU.magicSkills[Index].level + 1L + offset;
            else if (Character.settings.nguLevelTrack == difficulty.evil)
                num1 = Character.NGU.magicSkills[Index].evilLevel + 1L + offset;
            else if (Character.settings.nguLevelTrack == difficulty.sadistic)
                num1 = Character.NGU.magicSkills[Index].sadisticLevel + 1L + offset;

            var num2 = Character.totalMagicPower() * (double)Character.totalNGUSpeedBonus() * Character.adventureController.itopod.totalMagicNGUBonus() * Character.inventory.macguffinBonuses[5] * Character.NGUController.magicNGUBonus() * Character.allDiggers.totalMagicNGUBonus() * Character.hacksController.totalMagicNGUBonus() * Character.beastQuestPerkController.totalMagicNGUSpeed() * Character.wishesController.totalMagicNGUSpeed() * Character.cardsController.getBonus(cardBonus.magicNGUSpeed);
            if (Character.allChallenges.trollChallenge.completions() >= 1)
                num2 *= 3.0;
            if (Character.settings.nguLevelTrack >= difficulty.sadistic)
                num2 /= Character.NGUController.NGUMagic[0].sadisticDivider();
            var num3 = Character.NGUController.magicSpeedDivider(Index) * (double)num1 / num2;
            if (num3 >= Character.hardCap())
                num3 = Character.hardCap();
            var num4 = num3 <= 1.0 ? 1L : (long)num3;
            var num = (long)(num4 / (long)Math.Ceiling(num4 / (double)MaxAllocation) * 1.00000202655792);
            if (num + 1L <= long.MaxValue)
                ++num;
            if (num > Character.magic.idleMagic)
                num = Character.magic.idleMagic;
            if (num < 0L)
                num = 0L;

            var ppt = (double)num / num4;
            ret.Num = num;
            ret.PPT = ppt;
            return ret;
        }

        
    }
}
