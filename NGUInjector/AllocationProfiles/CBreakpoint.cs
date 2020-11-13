using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGUInjector.AllocationProfiles
{
    internal class CBreakpoint
    {
        public string Priority { get; set; }
        public double CapPercent { get; set; }
        public int Index { get; set; }
        public ResourceType Type { get; set; }

        internal bool IsValid()
        {
            return Index >= 0 && Unlocked() && !TargetMet();
        }

        internal bool Allocate(long inputMax)
        {
            var character = Main.Character;
            long capValue;
            switch (Type)
            {
                case ResourceType.Energy:
                    capValue = character.capEnergy;
                    break;
                case ResourceType.Magic:
                    capValue = character.magic.capMagic;
                    break;
                case ResourceType.R3:
                    capValue = character.res3.capRes3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var capMax = (long) Math.Ceiling(capValue * CapPercent);
            var maxAllocation = Math.Min(inputMax, capMax);

            return false;
        }

        private bool Unlocked()
        {
            if (Index < 0)
                return false;

            var character = Main.Character;
            if (Priority.StartsWith("TM") || Priority.StartsWith("CAPTM"))
            {
                return character.buttons.brokenTimeMachine.interactable;
            }

            if (Priority.StartsWith("WAN") || Priority.StartsWith("CAPWAN"))
            {
                return character.buttons.wandoos.interactable;
            }

            if (Priority.StartsWith("AT") || Priority.StartsWith("CAPAT"))
            {
                if (Index > 4)
                    return false;

                return character.buttons.advancedTraining.interactable;
            }

            if (Priority.StartsWith("AUG") || Priority.StartsWith("CAPAUG"))
            {
                if (!character.buttons.augmentation.interactable)
                    return false;

                if (Index > 13)
                    return false;

                var augIndex = (int)Math.Floor((double)(Index / 2));

                if (Index % 2 == 0)
                {
                    return character.bossID > character.augmentsController.augments[augIndex].augBossRequired;
                }

                return character.bossID > character.augmentsController.augments[augIndex].upgradeBossRequired;
            }

            if (Priority.StartsWith("NGU") || Priority.StartsWith("CAPNGU"))
            {
                switch (Type)
                {
                    case ResourceType.Magic when Index > 6:
                    case ResourceType.Energy when Index > 8:
                        return false;
                    default:
                        return character.buttons.ngu.interactable;
                }
            }

            if (Priority.StartsWith("BT") || Priority.StartsWith("CAPBT"))
            {
                if (Index > 11)
                    return false;

                return character.buttons.basicTraining.interactable;
            }

            if (Priority.StartsWith("BR"))
            {
                return character.buttons.bloodMagic.interactable;
            }

            if (Priority.StartsWith("RIT") || Priority.StartsWith("CAPRIT"))
            {
                if (Index >= character.bloodMagicController.ritualsUnlocked())
                    return false;

                return character.buttons.bloodMagic.interactable;
            }

            if (Priority.StartsWith("HACK"))
            {
                return character.buttons.hacks.interactable;
            }

            if (Priority.StartsWith("WISH"))
            {
                return character.buttons.wishes.interactable;
            }

            return true;
        }

        private bool TargetMet()
        {
            var character = Main.Character;
            if (Priority.StartsWith("HACK"))
            {
                return character.hacksController.hitTarget(Index);
            }

            if (Priority.StartsWith("AT") || Priority.StartsWith("CAPAT"))
            {
                return character.advancedTraining.levelTarget[Index] != 0 && character.advancedTraining.level[Index] >=
                    character.advancedTraining.levelTarget[Index];
            }

            if (Priority.StartsWith("AUG") || Priority.StartsWith("CAPAUG"))
            {
                // ReSharper disable once PossibleLossOfFraction
                var augIndex = (int)Math.Floor((double)(Index / 2));
                if (Index % 2 == 0)
                {
                    var target = character.augments.augs[augIndex].augmentTarget;
                    return target != 0 && character.augments.augs[augIndex].augLevel >= target;
                }
                else
                {
                    var target = character.augments.augs[augIndex].upgradeTarget;
                    return target != 0 && character.augments.augs[augIndex].upgradeLevel >= target;
                }
            }

            if (Priority.StartsWith("NGU") || Priority.StartsWith("CAPNGU"))
            {
                var track = character.settings.nguLevelTrack;
                var ngus = Type == ResourceType.Energy ? character.NGU.skills : character.NGU.magicSkills;
                long target;
                long level;
                switch (track)
                {
                    case difficulty.normal:
                        target = ngus[Index].target;
                        level = ngus[Index].level;
                        break;
                    case difficulty.evil:
                        target = ngus[Index].evilTarget;
                        level = ngus[Index].evilLevel;
                        break;
                    default:
                        target = ngus[Index].sadisticTarget;
                        level = ngus[Index].sadisticLevel;
                        break;
                }

                return target > 0 && level >= target;
            }

            if (Priority.StartsWith("TM") || Priority.StartsWith("CAPTM"))
            {
                var target = Type == ResourceType.Energy ? character.machine.speedTarget : character.machine.multiTarget;
                var level = Type == ResourceType.Energy ? character.machine.levelSpeed : character.machine.levelGoldMulti;

                return target > 0 && level >= target;
            }

            if (Priority.StartsWith("BT") || Priority.StartsWith("CAPBT"))
            {
                if (Index == 0 || Index == 6)
                    return true;

                if (Index <= 5)
                    return character.training.attackTraining[Index - 1] >= 5000 * Index;

                var temp = Index -= 6;
                return character.training.defenseTraining[temp - 1] >= 5000 * temp;
            }

            return false;
        }

        internal static CBreakpoint ParseBreakpointString(string prio, ResourceType type)
        {
            int cap;
            int index;
            var temp = prio;
            if (temp.Contains(":"))
            {
                var split = prio.Split(':');
                temp = split[0];
                var success = int.TryParse(split[1], out cap);
                if (!success)
                {
                    cap = 100;
                }

                if (cap > 100)
                {
                    cap = 100;
                }

                if (cap < 0)
                {
                    cap = 0;
                }
            }
            else
            {
                cap = 100;
            }

            if (temp.Contains("-"))
            {
                var split = prio.Split('-');
                temp = split[0];
                var success = int.TryParse(split[1], out index);
                if (!success)
                {
                    index = -1;
                }
            }
            else
            {
                index = 0;
            }

            return new CBreakpoint
            {
                CapPercent = (double)cap / 100,
                Index = index,
                Priority = temp,
                Type = type
            };
        }
    }

    internal enum ResourceType
    {
        Energy,
        Magic,
        R3
    }
}
