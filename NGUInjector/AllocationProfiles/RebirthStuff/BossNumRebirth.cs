using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGUInjector.AllocationProfiles.RebirthStuff
{
    internal class BossNumRebirth : BaseRebirth
    {
        internal double NumBosses { get; set; }
        internal override bool RebirthAvailable()
        {
            if (!Main.Settings.AutoRebirth)
                return false;

            if (!MinTimeMet())
                return false;

            var bosses = Math.Round(Math.Log10(CharObj.nextAttackMulti / CharObj.attackMulti));
            return bosses > NumBosses;
        }
    }
}
