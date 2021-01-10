using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGUInjector.AllocationProfiles.BreakpointTypes
{
    internal class BestHackBP : BaseBreakpoint
    {
        protected override bool Unlocked()
        {
            for (int i = 0; i < Character.hacks.hacks.Count; i++)
            {
                if (this.Unlocked(i))
                {
                    return true;
                }
            }
            return false;
        }

        protected bool Unlocked(int id) { 
            return id <= 14 && Character.buttons.hacks.interactable;
        }

        protected override bool TargetMet()
        {
            for (int i = 0; i < Character.hacks.hacks.Count; i++)
            {
                if (!this.TargetMet(i))
                {
                    return false;
                }
            }
            return true;
        }

        protected bool TargetMet(int id) { 
            return Character.hacksController.hitTarget(id);
        }

        internal override bool Allocate()
        {
            var alloc = MaxAllocation;
            int bestHack = -1;
            float bestHackTime = -1f;
            var time = 0f;
            for (int i = 0; i < Character.hacks.hacks.Count; i++)
            {
                if (!this.Unlocked(i)) {
                    continue;
                }
                if (this.TargetMet(i) && !Main.Settings.HackAdvance)
                {
                    continue;
                }
                time = this.time(i, alloc) * Character.hacksController.milestoneThreshold(i);
                if (bestHackTime == -1 || bestHackTime > time)
                {
                    bestHack = i;
                    bestHackTime = time;
                }
                Main.LogAllocation($"Time to work: {i} is {time} and target: {this.TargetMet(i)}");
            }
            Main.LogAllocation($"Best Time to work: {bestHack} is {bestHackTime} and target: {this.TargetMet(bestHack)} for {alloc}");
            if (bestHack != -1)
            {
                if (Main.Settings.HackAdvance && Character.hacks.hacks[bestHack].target > 0 && (Character.hacks.hacks[bestHack].target - Character.hacks.hacks[bestHack].level) < 20)
                {
                    Character.hacksController.setToNextMilestone(bestHack);
                }                        
                Character.hacksController.addR3(bestHack, (long)alloc);
            }
            return true;
        }

        protected override bool CorrectResourceType()
        {
            return Type == ResourceType.R3;
        }

        public float time(int id, float r3)
        {
            double num = (double)((float)r3 * Character.totalRes3Power() * Character.hacksController.totalHackSpeedBonus() / (Character.hacksController.properties[id].baseDivider * this.levelDivider(id)));
            if (num >= 3.4028234663852886E+38)
            {
                return float.MaxValue;
            }
            if (num <= -3.4028234663852886E+38)
            {
                return 0f;
            }
            return 1f / (float)num / 50f;
        }

        public float levelDivider(int id)
        {
            long target = Character.hacks.hacks[id].level + Character.hacksController.levelsToNextMilestone(id);
            double num = Math.Pow(1.0078, (double)target) * (double)(target + 1L);
            if (num > 3.4028234663852886E+38)
            {
                return float.MaxValue;
            }
            return (float)num;
        }
    }
}
