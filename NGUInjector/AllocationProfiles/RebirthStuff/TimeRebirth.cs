using System;

namespace NGUInjector.AllocationProfiles.RebirthStuff
{
    internal class TimeRebirth : BaseRebirth
    {
        internal double RebirthTime { get; set; }

        internal override bool RebirthAvailable()
        {
            if (!Main.Settings.AutoRebirth)
                return false;

            if (RebirthTime < 0)
                return false;

            if (!BaseRebirthChecks())
                return false;

            if (!CharObj.challenges.inChallenge && AnyChallengesValid())
                return true;

            var time = CharObj.rebirthTime.totalseconds;
            return time >= RebirthTime;
        }
    }
}
