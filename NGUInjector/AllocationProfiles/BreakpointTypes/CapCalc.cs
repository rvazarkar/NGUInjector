using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGUInjector.AllocationProfiles.BreakpointTypes
{
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
