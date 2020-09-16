using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using SimpleJSON;

namespace NGUInjector.AllocationProfiles
{
    [Serializable]
    internal class CustomAllocation : AllocationProfile
    {
        private BreakpointWrapper _wrapper;
        private AllocationBreakPoint _currentMagicBreakpoint;
        private AllocationBreakPoint _currentEnergyBreakpoint;
        private GearBreakpoint _currentGearBreakpoint;
        private DiggerBreakpoint _currentDiggerBreakpoint;
        private bool _hasGearSwapped;
        private bool _hasDiggerSwapped;
        private readonly string _allocationPath;

        public CustomAllocation(string dir)
        {
            var path = Path.Combine(dir, "allocation.json");
            _allocationPath = path;
        }

        internal void ReloadAllocation()
        {
            if (File.Exists(_allocationPath))
            {
                try
                {
                    var text = File.ReadAllText(_allocationPath);
                    var parsed = JSON.Parse(text);
                    var breakpoints = parsed["Breakpoints"];
                    _wrapper = new BreakpointWrapper { Breakpoints = new Breakpoints() };
                    _wrapper.Breakpoints.Magic = breakpoints["Magic"].Children.Select(bp => new AllocationBreakPoint { Time = bp["Time"].AsInt, Priorities = bp["Priorities"].AsArray.Children.Select(x => x.Value).ToArray() }).OrderByDescending(x => x.Time).ToArray();
                    _wrapper.Breakpoints.Energy = breakpoints["Energy"].Children.Select(bp => new AllocationBreakPoint { Time = bp["Time"].AsInt, Priorities = bp["Priorities"].AsArray.Children.Select(x => x.Value).ToArray() }).OrderByDescending(x => x.Time).ToArray();
                    _wrapper.Breakpoints.Gear = breakpoints["Gear"].Children.Select(bp => new GearBreakpoint { Time = bp["Time"].AsInt, Gear = bp["ID"].AsArray.Children.Select(x => x.AsInt).ToArray() }).OrderByDescending(x => x.Time).ToArray();
                    _wrapper.Breakpoints.Diggers = breakpoints["Diggers"].Children.Select(bp => new DiggerBreakpoint { Time = bp["Time"].AsInt, Diggers = bp["List"].AsArray.Children.Select(x => x.AsInt).ToArray() }).OrderByDescending(x => x.Time).ToArray();
                    Main.Log($"Loaded custom allocation:\n{_wrapper.Breakpoints.Energy.Length} energy breakpoints\n{_wrapper.Breakpoints.Magic.Length} magic breakpoints\n{_wrapper.Breakpoints.Gear.Length} gear breakpoints\n{_wrapper.Breakpoints.Diggers.Length} digger breakpoints");
                    _currentEnergyBreakpoint = null;
                    _currentMagicBreakpoint = null;
                    _currentGearBreakpoint = null;
                    _currentDiggerBreakpoint = null;
                    _character.removeMostEnergy();
                    _character.removeMostMagic();
                }
                catch (Exception e)
                {
                    Main.Log(e.Message);
                    Main.Log(e.StackTrace);
                }
            }
            else
            {
                var w = new BreakpointWrapper
                {
                    Breakpoints = new Breakpoints
                    {
                        Energy = new AllocationBreakPoint[] { },
                        Magic = new AllocationBreakPoint[] { },
                        Gear = new GearBreakpoint[] { },
                        Diggers = new DiggerBreakpoint[] {}
                    }
                };

                using (var writer = new StreamWriter(File.Open(_allocationPath, FileMode.CreateNew)))
                {
                    writer.WriteLine(JsonUtility.ToJson(w));
                    writer.Flush();
                }
            }
        }
        
        public override void AllocateEnergy()
        {
            if (_wrapper == null)
                return;

            var bp = GetCurrentBreakpoint(true);
            if (bp == null)
                return;

            if (bp.Time != _currentEnergyBreakpoint.Time)
            {
                _character.removeMostEnergy();
                _currentEnergyBreakpoint = bp;
            }

            if (_character.idleEnergy == 0)
                return;

            var usedWandoos = false;
            if (bp.Priorities.Contains("WAN"))
            {
                usedWandoos = true;
                if (_character.wandoos98.wandoosEnergy < _character.wandoos98Controller.capAmountEnergy())
                {
                    _character.removeMostEnergy();
                    _character.wandoos98Controller.addCapEnergy();
                }
                else
                {
                    _character.wandoos98Controller.addCapEnergy();
                }
            }


            var prioCount = bp.Priorities.Length - (usedWandoos ? 1 : 0);
            var toAdd = (int)Math.Floor((double)_character.idleEnergy / prioCount);
            _character.input.energyRequested.text = toAdd.ToString();
            _character.input.validateInput();

            foreach (var prio in bp.Priorities)
            {
                ReadEnergyBreakpoint(prio);
            }
        }

        public override void AllocateMagic()
        {
            if (_wrapper == null)
                return;

            var bp = GetCurrentBreakpoint(false);
            if (bp == null)
                return;

            if (bp.Time != _currentMagicBreakpoint.Time)
            {
                _character.removeMostMagic();
                _currentMagicBreakpoint = bp;
            }

            if (_character.magic.idleMagic == 0)
                return;

            var usedWandoos = false;
            if (bp.Priorities.Contains("WAN"))
            {
                usedWandoos = true;
                if (_character.wandoos98.wandoosMagic < _character.wandoos98Controller.capAmountMagic())
                {
                    _character.removeMostMagic();
                    _character.wandoos98Controller.addCapMagic();
                }
                else
                {
                    _character.wandoos98Controller.addCapMagic();
                }
            }
            var prioCount = bp.Priorities.Length - (usedWandoos ? 1 : 0);
            var toAdd = (int)Math.Floor((double)_character.magic.idleMagic / prioCount);
            _character.input.energyRequested.text = toAdd.ToString();
            _character.input.validateInput();

            foreach (var prio in bp.Priorities)
            {
                ReadMagicBreakpoint(prio);
            }
        }

        public override void EquipGear()
        {
            if (_wrapper == null)
                return;
            var bp = GetCurrentGearBreakpoint();
            if (bp == null)
                return;

            if (bp.Time != _currentGearBreakpoint.Time)
            {
                _hasGearSwapped = false;
            }

            if (_hasGearSwapped) return;

            if (!LoadoutManager.CanSwap()) return;
            Main.Character.removeMostEnergy();
            Main.Character.removeMostMagic();
            _hasGearSwapped = true;
            _currentGearBreakpoint = bp;
            LoadoutManager.ChangeGear(bp.Gear);
            Main.Controller.assignCurrentEquipToLoadout(0);
        }

        public override void EquipDiggers()
        {
            if (_wrapper == null)
                return;
            var bp = GetCurrentDiggerBreakpoint();
            if (bp == null)
                return;

            if (bp.Time != _currentDiggerBreakpoint.Time)
            {
                _hasDiggerSwapped = false;
            }

            if (_hasDiggerSwapped) return;

            if (!DiggerManager.CanSwap()) return;
            _hasDiggerSwapped = true;
            _currentDiggerBreakpoint = bp;
            DiggerManager.EquipDiggers(bp.Diggers);
        }

        private AllocationBreakPoint GetCurrentBreakpoint(bool energy)
        {
            foreach (var b in energy ? _wrapper.Breakpoints.Energy : _wrapper.Breakpoints.Magic)
            {
                var rbTime = _character.rebirthTime.totalseconds;
                if (rbTime > b.Time)
                {
                    if (energy && _currentEnergyBreakpoint == null)
                    {
                        _character.removeMostEnergy();
                        _currentEnergyBreakpoint = b;
                    }

                    if (!energy && _currentMagicBreakpoint == null)
                    {
                        _character.removeMostMagic();
                        _currentMagicBreakpoint = b;
                    }
                    return b;
                }
            }

            return null;
        }

        private GearBreakpoint GetCurrentGearBreakpoint()
        {
            foreach (var b in _wrapper.Breakpoints.Gear)
            {
                if (_character.rebirthTime.totalseconds > b.Time)
                {
                    if (_currentGearBreakpoint == null)
                    {
                        _hasGearSwapped = false;
                        _currentGearBreakpoint = b;
                    }
                    return b;
                }
            }

            return null;
        }

        private DiggerBreakpoint GetCurrentDiggerBreakpoint()
        {
            foreach (var b in _wrapper.Breakpoints.Diggers)
            {
                if (_character.rebirthTime.totalseconds > b.Time)
                {
                    if (_currentDiggerBreakpoint == null)
                    {
                        _hasDiggerSwapped = false;
                        _currentDiggerBreakpoint = b;
                    }
                    return b;
                }
            }

            return null;
        }

        private void CastRituals()
        {
            for (var i = _character.bloodMagic.ritual.Count - 1; i >= 0; i--)
            {
                if (_character.magic.idleMagic == 0)
                    break;
                if (i >= _character.bloodMagicController.ritualsUnlocked())
                    continue;
                var goldCost = _character.bloodMagicController.bloodMagics[i].baseCost * _character.totalDiscount();
                if (goldCost > _character.realGold && _character.bloodMagic.ritual[i].progress <= 0.0)
                {
                    if (_character.bloodMagic.ritual[i].magic > 0)
                    {
                        _character.bloodMagicController.bloodMagics[i].removeAllMagic();
                    }
                    continue;
                }

                var tLeft = _character.bloodMagicController.bloodMagics[i].timeLeft();
                if (!tLeft.EndsWith("s"))
                {
                    if (tLeft.Count(x => x == ':') > 1)
                        continue;
                }

                _character.bloodMagicController.bloodMagics[i].cap();
            }
        }

        private void ReadMagicBreakpoint(string breakpoint)
        {
            if (breakpoint.Equals("BR"))
            {
                CastRituals();
                return;
            }
            if(breakpoint.StartsWith("TM"))
            {
                _character.timeMachineController.addMagic();
                return;
            }

            if (breakpoint.StartsWith("NGU"))
            {
                var success = int.TryParse(breakpoint.Split('-')[1], out var index);
                if (!success || index < 0 || index > 6)
                {
                    return;
                }
                _character.NGUController.NGUMagic[index].add();
                return;
            }

            if (breakpoint.StartsWith("CAPNGU"))
            {
                var success = int.TryParse(breakpoint.Split('-')[1], out var index);
                if (!success || index < 0 || index > 6)
                {
                    return;
                }
                _character.NGUController.NGUMagic[index].cap();
                return;
            }
        }

        private void ReadEnergyBreakpoint(string breakpoint)
        {
            if (breakpoint.StartsWith("TM"))
            {
                _character.timeMachineController.addEnergy();
                return;
            }

            if (breakpoint.StartsWith("NGU"))
            {
                var success = int.TryParse(breakpoint.Split('-')[1], out var index);
                if (!success || index < 0 || index > 8)
                {
                    return;
                }
                _character.NGUController.NGU[index].add();
                return;
            }

            if (breakpoint.StartsWith("CAPNGU"))
            {
                var success = int.TryParse(breakpoint.Split('-')[1], out var index);
                if (!success || index < 0 || index > 8)
                {
                    return;
                }
                _character.NGUController.NGU[index].cap();
                return;
            }

            if (breakpoint.StartsWith("AT"))
            {
                var success = int.TryParse(breakpoint.Split('-')[1], out var index);
                if (!success || index < 0 || index > 4)
                {
                    return;
                }

                switch (index)
                {
                    case 0:
                        _character.advancedTrainingController.defense.addEnergy();
                        break;
                    case 1:
                        _character.advancedTrainingController.attack.addEnergy();
                        break;
                    case 2:
                        _character.advancedTrainingController.block.addEnergy();
                        break;
                    case 3:
                        _character.advancedTrainingController.wandoosEnergy.addEnergy();
                        break;
                    case 4:
                        _character.advancedTrainingController.wandoosMagic.addEnergy();
                        break;
                }
                return;
            }

            if (breakpoint.StartsWith("AUG"))
            {
                var success = int.TryParse(breakpoint.Split('-')[1], out var index);
                if (!success || index < 0 || index > 13)
                {
                    return;
                }

                var augIndex = (int)Math.Floor((double)(index / 2));

                if (index % 2 == 0)
                {
                    _character.augmentsController.augments[augIndex].addEnergyAug();
                }
                else
                {
                    _character.augmentsController.augments[augIndex].addEnergyUpgrade();
                }
            }
        }
    }

    [Serializable]
    public class BreakpointWrapper
    {
        [SerializeField] public Breakpoints Breakpoints;
    }

    [Serializable]
    public class Breakpoints
    {
        [SerializeField] public AllocationBreakPoint[] Magic;
        [SerializeField] public AllocationBreakPoint[] Energy;
        [SerializeField] public GearBreakpoint[] Gear;
        [SerializeField] public DiggerBreakpoint[] Diggers;

    }

    [Serializable]
    public class AllocationBreakPoint
    {
        [SerializeField] public int Time;
        [SerializeField] public string[] Priorities;
    }

    [Serializable]
    public class GearBreakpoint
    {
        public int Time;
        public int[] Gear;
    }

    [Serializable]
    public class DiggerBreakpoint
    {
        public int Time;
        public int[] Diggers;
    }
}
