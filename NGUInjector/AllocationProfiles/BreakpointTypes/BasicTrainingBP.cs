using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace NGUInjector.AllocationProfiles.BreakpointTypes
{
    internal class BasicTrainingBP : BaseBreakpoint
    {
        private int BTIndex => Index <= 5 ? Index : Index - 6;

        protected override bool Unlocked()
        {
            if (Index > 11)
                return false;

            if (Index == 0 || Index == 6)
                return true;

            var trainings = Index <= 5 ? Character.training.attackTraining : Character.training.defenseTraining;

            return trainings[BTIndex - 1] >= 5000 * BTIndex;
        }

        protected override bool TargetMet()
        {
            return false;
        }

        internal override bool Allocate()
        {
            if (Index <= 5)
            {
                var cap = Character.training.attackCaps[BTIndex];
                SetInput(Math.Min(cap, MaxAllocation));
                Character.allOffenseController.trains[BTIndex].addEnergy();
            }
            else
            {
                var cap = Character.training.defenseCaps[BTIndex];
                SetInput(Math.Min(cap, MaxAllocation));
                Character.allDefenseController.trains[BTIndex].addEnergy();
            }

            return true;
        }

        protected override bool CorrectResourceType()
        {
            return Type == ResourceType.Energy;
        }
    }
}
