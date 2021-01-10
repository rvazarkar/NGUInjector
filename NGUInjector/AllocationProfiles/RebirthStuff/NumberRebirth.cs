using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGUInjector.AllocationProfiles.RebirthStuff
{
    internal class NumberRebirth : BaseRebirth
    {
        internal double MultTarget { get; set; }
        internal override bool RebirthAvailable()
        {
            if (!Main.Settings.AutoRebirth)
                return false;

            if (!BaseRebirthChecks())
                return false;

            if (!CharObj.challenges.inChallenge && AnyChallengesValid())
                return true;

            var target = CharObj.attackMulti * MultTarget;

            return CharObj.nextAttackMulti > target;
        }
    }
}
