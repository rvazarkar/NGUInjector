using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGUInjector.AllocationProfiles.RebirthStuff
{
    internal class NoRebirth : BaseRebirth
    {
        internal override bool RebirthAvailable()
        {
            return false;
        }
    }
}
