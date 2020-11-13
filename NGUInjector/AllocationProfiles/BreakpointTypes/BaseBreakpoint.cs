using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NGUInjector.AllocationProfiles.BreakpointTypes
{
    internal abstract class BaseBreakpoint
    {
        protected double CapPercent { get; set; }
        protected int Index { get; set; }
        protected ResourceType Type { get; set; }
        protected bool IsCap { get; set; }
        protected Character Character { get; set; }
        protected float MaxAllocation => CalculateMaxAllocation();

        protected BaseBreakpoint()
        {
            Character = Main.Character;
        }

        internal bool IsValid()
        {
            return CorrectResourceType() && Unlocked() && !TargetMet();
        }

        protected abstract bool Unlocked();
        protected abstract bool TargetMet();
        internal abstract bool Allocate();
        protected abstract bool CorrectResourceType();

        protected float CalculateMaxAllocation()
        {
            var input = Character.energyMagicPanel.energyMagicInput;
            var character = Main.Character;
            long capValue;
            switch (Type)
            {
                case ResourceType.Energy:
                    capValue = IsCap ? character.capEnergy : character.idleEnergy;
                    break;
                case ResourceType.Magic:
                    capValue = IsCap ? character.magic.capMagic : character.magic.idleMagic;
                    break;
                case ResourceType.R3:
                    capValue = character.res3.capRes3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var capMax = (long)Math.Ceiling(capValue * CapPercent);
            return Math.Min(input, capMax); ;
        }

        protected void SetInput(float val)
        {
            Character.energyMagicPanel.energyRequested.text = val.ToString();
            Character.energyMagicPanel.validateInput();
        }

        internal static IEnumerable<BaseBreakpoint> ParseBreakpointArray(List<string> prios, ResourceType type)
        {
            foreach (var prio in prios)
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

                if (temp.StartsWith("NGU") || temp.StartsWith("CAPNGU"))
                {
                    yield return new NGUBreakpoint
                    {
                        CapPercent = cap,
                        Character = Main.Character,
                        Index = index,
                        IsCap = temp.Contains("CAP"),
                        Type = type
                    };
                }else if (temp.Contains("ALLNGU")) {
                    if (type == ResourceType.Energy) {
                        for (var i = 0; i < 9; i++)
                        {
                            yield return new NGUBreakpoint
                            {
                                CapPercent = cap,
                                Character = Main.Character,
                                Index = i,
                                IsCap = temp.Contains("CAP"),
                                Type = type
                            };
                        }
                    }else if (type == ResourceType.Magic) {
                        for (var i = 0; i < 7; i++)
                        {
                            yield return new NGUBreakpoint
                            {
                                CapPercent = cap,
                                Character = Main.Character,
                                Index = i,
                                IsCap = temp.Contains("CAP"),
                                Type = type
                            };
                        }
                    }
                }else if (temp.StartsWith("CAPAT") || temp.StartsWith("AT")) {
                    yield return new AdvancedTraining
                    {
                        CapPercent = cap,
                        Character = Main.Character,
                        Index = index,
                        IsCap = temp.Contains("CAP"),
                        Type = type
                    };
                }else if (temp.StartsWith("AUG") || temp.StartsWith("CAPAUG")) {
                    yield return new Augment
                    {
                        CapPercent = cap,
                        Character = Main.Character,
                        Index = index,
                        IsCap = temp.Contains("CAP"),
                        Type = type
                    };
                } else if (temp.StartsWith("BT") || temp.StartsWith("CAPBT"))
                {
                    yield return new BasicTraining
                    {
                        CapPercent = cap,
                        Character = Main.Character,
                        Index = index,
                        IsCap = temp.Contains("CAP"),
                        Type = type
                    };
                }else if (temp.StartsWith("HACK"))
                {
                    yield return new HackBreakpoint
                    {
                        CapPercent = cap,
                        Character = Main.Character,
                        Index = index,
                        IsCap = temp.Contains("CAP"),
                        Type = type
                    };
                }else if (temp.StartsWith("WISH"))
                {
                    yield return new WishBreakpoint
                    {
                        CapPercent = cap,
                        Character = Main.Character,
                        Index = index,
                        IsCap = temp.Contains("CAP"),
                        Type = type
                    };
                }else if (temp.StartsWith("WAN") || temp.StartsWith("CAPWAN"))
                {
                    yield return new Wandoos
                    {
                        CapPercent = cap,
                        Character = Main.Character,
                        Index = index,
                        IsCap = temp.Contains("CAP"),
                        Type = type
                    };
                }else if (temp.StartsWith("CAPALLBT"))
                {
                    for (var i = 0; i < 12; i++)
                    {
                        yield return new BasicTraining
                        {
                            CapPercent = cap,
                            Character = Main.Character,
                            Index = index,
                            IsCap = temp.Contains("CAP"),
                            Type = type
                        };
                    }
                }
            }
        }
    }
}
