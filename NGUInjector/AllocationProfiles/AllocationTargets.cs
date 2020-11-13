using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGUInjector.AllocationProfiles
{
    internal class AllocationTargets
    {
        private readonly Character _character;
        public AllocationTargets(Character character)
        {
            _character = character;
        }

        internal bool HackTargetMet(CBreakpoint breakpoint)
        {
            if (breakpoint.Index < 0 || breakpoint.Index > 14)
                return true;

            return _character.hacksController.hitTarget(breakpoint.Index);
        }

    }
}
