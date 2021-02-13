using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGUInjector.AllocationProfiles.BreakpointTypes
{
    internal class WandoosBP : BaseBreakpoint
    {
        protected override bool Unlocked()
        {
            return Character.buttons.wandoos.interactable && !Character.wandoos98.disabled;
        }

        protected override bool TargetMet()
        {
            return false;
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
            var cap = Character.wandoos98Controller.capAmountEnergy();
            SetInput(Math.Min(cap, MaxAllocation));
            Character.wandoos98Controller.addEnergy();
        }

        private void AllocateMagic()
        {
            var cap = Character.wandoos98Controller.capAmountMagic();
            SetInput(Math.Min(cap, MaxAllocation));
            Character.wandoos98Controller.addMagic();
        }

        protected override bool CorrectResourceType()
        {
            return Type == ResourceType.Energy || Type == ResourceType.Magic;
        }
    }
}
