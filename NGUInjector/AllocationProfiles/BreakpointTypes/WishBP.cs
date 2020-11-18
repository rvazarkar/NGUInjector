using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGUInjector.AllocationProfiles.BreakpointTypes
{
    internal class WishBP : BaseBreakpoint
    {
        protected override bool Unlocked()
        {
            return Index <= Character.wishesController.curWishSlots() - 1 && Character.buttons.wishes.interactable;
        }

        protected override bool TargetMet()
        {
            
            return false;
        }

        internal override bool Allocate()
        {
            var id = Main.WishManager.GetSlot(Index);
            if (id == -1)
                return true;

            SetInput(MaxAllocation);
            var c = Character.wishesController;
            switch (Type)
            {
                case ResourceType.Energy:
                    c.addEnergy(id);
                    break;
                case ResourceType.Magic:
                    c.addMagic(id);
                    break;
                default:
                    c.addRes3(id);
                    break;
            }

            return true;
        }

        protected override bool CorrectResourceType()
        {
            return true;
        }
    }
}
