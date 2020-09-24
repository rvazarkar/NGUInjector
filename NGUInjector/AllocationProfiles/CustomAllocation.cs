using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SimpleJSON;
using UnityEngine;

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
        private WandoosBreakpoint _currentWandoosBreakpoint;
        private bool _hasGearSwapped;
        private bool _hasDiggerSwapped;
        private bool _hasWandoosSwapped;
        private readonly string _allocationPath;
        private string[] _validEnergyPriorities = { "WAN", "CAPWAN", "TM", "CAPTM", "CAPAT", "AT", "NGU", "CAPNGU", "AUG", "BT", "CAPBT" };
        private string[] _validMagicPriorities = { "WAN", "CAPWAN", "BR", "TM", "CAPTM", "NGU", "CAPNGU" };

        public CustomAllocation(string dir)
        {
            var path = Path.Combine(dir, "allocation.json");
            _allocationPath = path;
        }

        private List<string> ValidatePriorities(List<string> priorities)
        {
            if (!_character.buttons.brokenTimeMachine.enabled)
            {
                priorities.RemoveAll(x => x.Contains("TM"));
            }

            if (!_character.buttons.wandoos.enabled)
            {
                priorities.RemoveAll(x => x.Contains("WAN"));
            }

            if (!_character.buttons.advancedTraining.enabled)
            {
                priorities.RemoveAll(x => x.Contains("AT"));
            }

            if (!_character.buttons.augmentation.enabled)
            {
                priorities.RemoveAll(x => x.Contains("AUG"));
            }

            if (!_character.buttons.ngu.enabled)
            {
                priorities.RemoveAll(x => x.Contains("NGU"));
            }

            if (!_character.buttons.basicTraining.enabled)
            {
                priorities.RemoveAll(x => x.Contains("BT"));
            }

            if (!_character.buttons.bloodMagic.enabled)
            {
                priorities.RemoveAll(x => x.Contains("BR"));
            }

            return priorities;
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
                    _wrapper.Breakpoints.Magic = breakpoints["Magic"].Children.Select(bp => new AllocationBreakPoint { Time = bp["Time"].AsInt, Priorities = bp["Priorities"].AsArray.Children.Select(x => x.Value).Where(x => _validMagicPriorities.Any(x.StartsWith)).ToArray() }).OrderByDescending(x => x.Time).ToArray();
                    _wrapper.Breakpoints.Energy = breakpoints["Energy"].Children.Select(bp => new AllocationBreakPoint { Time = bp["Time"].AsInt, Priorities = bp["Priorities"].AsArray.Children.Select(x => x.Value).Where(x => _validEnergyPriorities.Any(x.StartsWith)).ToArray() }).OrderByDescending(x => x.Time).ToArray();
                    _wrapper.Breakpoints.Gear = breakpoints["Gear"].Children.Select(bp => new GearBreakpoint { Time = bp["Time"].AsInt, Gear = bp["ID"].AsArray.Children.Select(x => x.AsInt).ToArray() }).OrderByDescending(x => x.Time).ToArray();
                    _wrapper.Breakpoints.Diggers = breakpoints["Diggers"].Children.Select(bp => new DiggerBreakpoint { Time = bp["Time"].AsInt, Diggers = bp["List"].AsArray.Children.Select(x => x.AsInt).ToArray() }).OrderByDescending(x => x.Time).ToArray();
                    _wrapper.Breakpoints.Wandoos = breakpoints["Wandoos"].Children.Select(bp => new WandoosBreakpoint { Time = bp["Time"].AsInt, OS = bp["OS"].AsInt }).OrderByDescending(x => x.Time).ToArray();
                    _wrapper.Breakpoints.RebirthTime = breakpoints["RebirthTime"].AsInt;
                    
                    if (_wrapper.Breakpoints.RebirthTime < 180 && _wrapper.Breakpoints.RebirthTime != -1)
                    {
                        _wrapper.Breakpoints.RebirthTime = -1;
                        Main.Log("Invalid rebirth time in allocation. Rebirth disabled");
                    }

                    if (_wrapper.Breakpoints.RebirthTime > 0)
                    {
                        Main.Log($"Loaded custom allocation:\n{_wrapper.Breakpoints.Energy.Length} energy breakpoints\n{_wrapper.Breakpoints.Magic.Length} magic breakpoints\n{_wrapper.Breakpoints.Gear.Length} gear breakpoints\n{_wrapper.Breakpoints.Diggers.Length} digger breakpoints,\n{_wrapper.Breakpoints.Wandoos.Length} wandoos OS breakpoints. \nRebirth at {_wrapper.Breakpoints.RebirthTime}");
                    }
                    else
                    {
                        Main.Log($"Loaded custom allocation:\n{_wrapper.Breakpoints.Energy.Length} energy breakpoints\n{_wrapper.Breakpoints.Magic.Length} magic breakpoints\n{_wrapper.Breakpoints.Gear.Length} gear breakpoints\n{_wrapper.Breakpoints.Diggers.Length} digger breakpoints.\n{_wrapper.Breakpoints.Wandoos.Length} wandoos OS breakpoints. \nNo rebirth time specified");
                    }
                    
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
                        Diggers = new DiggerBreakpoint[] {},
                        Wandoos = new WandoosBreakpoint[] {},
                        RebirthTime = -1
                    }
                };

                var emptyAllocation = @"{
    ""Breakpoints"": {
      ""Magic"": [
        {
          ""Time"": 0,
          ""Priorities"": []
        }
      ],
      ""Energy"": [
        {
          ""Time"": 0,
          ""Priorities"": []
        }
      ],
      ""Gear"": [
        {
          ""Time"": 0,
          ""ID"": []
        }
      ],
      ""Wandoos"": [
        {
          ""Time"": 0,
          ""OS"": 0
        }
      ],
      ""Diggers"": [
        {
          ""Time"": 0,
          ""List"": []
        }
      ],
      ""RebirthTime"": -1
    }
  }
        ";

                Main.Log("Created empty allocation profile. Please update allocation.json");
                using (var writer = new StreamWriter(File.Open(_allocationPath, FileMode.CreateNew)))
                {
                    writer.WriteLine(emptyAllocation);
                    writer.Flush();
                }
            }
        }

        internal void SwapOS()
        {
            var bp = GetCurrentWandoosBreakpoint();
            if (bp == null)
                return;

            if (bp.Time != _currentWandoosBreakpoint.Time)
            {
                _hasWandoosSwapped = false;
            }

            if (_hasWandoosSwapped) return;

            if (bp.OS == 0 && _character.wandoos98.os == OSType.wandoos98) return;
            if (bp.OS == 1 && _character.wandoos98.os == OSType.wandoosMEH) return;
            if (bp.OS == 2 && _character.wandoos98.os == OSType.wandoosXL) return;

            var id = bp.OS;
            if (id == 0)
            {
                var controller = Main.Character.wandoos98Controller;
                var type = controller.GetType().GetField("nextOS",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                type?.SetValue(controller, id);
                
                typeof(Wandoos98Controller)
                    .GetMethod("setOSType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(controller, null);
            }
            if (id == 1 && _character.inventory.itemList.jakeComplete)
            {
                var controller = Main.Character.wandoos98Controller;
                var type = controller.GetType().GetField("nextOS",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                type?.SetValue(controller, id);
                typeof(Wandoos98Controller)
                    .GetMethod("setOSType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(controller, null);
            }

            if (id == 2 && _character.wandoos98.XLLevels > 0)
            {
                var controller = Main.Character.wandoos98Controller;
                var type = controller.GetType().GetField("nextOS",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                type?.SetValue(controller, id);
                typeof(Wandoos98Controller)
                    .GetMethod("setOSType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(controller, null);
            }
        }

        public void DoRebirth()
        {
            if (_wrapper == null)
                return;

            if (_wrapper.Breakpoints.RebirthTime < 0)
                return;

            if (_character.rebirthTime.totalseconds < _wrapper.Breakpoints.RebirthTime)
                return;

            Main.Log("Rebirth time hit, performing rebirth");
            var controller = Main.Character.rebirth;
            typeof(Rebirth).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(x => x.Name == "engage" && x.GetParameters().Length == 0).Invoke(controller, null);
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

            try
            {
                var temp = ValidatePriorities(bp.Priorities.ToList());
                var capPrios = temp.Where(x => x.StartsWith("BR") || x.StartsWith("CAP")).ToArray();
                temp.RemoveAll(x => x.StartsWith("BR") || x.StartsWith("CAP"));

                if (_character.idleEnergy == 0 && capPrios.Length == 0)
                    return;


                if (capPrios.Length > 0) _character.removeMostEnergy();
                foreach (var prio in capPrios)
                {
                    ReadEnergyBreakpoint(prio);
                }


                var prioCount = temp.Count;
                var toAdd = (int) Math.Floor((double) _character.idleEnergy / prioCount);
                _character.input.energyRequested.text = toAdd.ToString();
                _character.input.validateInput();

                foreach (var prio in temp)
                {
                    ReadEnergyBreakpoint(prio);
                }
            }
            catch (Exception e)
            {
                Main.Log(e.Message);
                Main.Log(e.StackTrace);
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

            var temp = ValidatePriorities(bp.Priorities.ToList());
            var capPrios = temp.Where(x => x.StartsWith("BR") || x.StartsWith("CAP")).ToArray();
            temp.RemoveAll(x => x.StartsWith("BR") || x.StartsWith("CAP"));

            if (_character.magic.idleMagic == 0 && capPrios.Length ==  0)
                return;

            if (capPrios.Length > 0) _character.removeMostMagic();
            foreach (var prio in capPrios)
            {
                ReadMagicBreakpoint(prio);
            }

            var prioCount = temp.Count;
            var toAdd = (int)Math.Floor((double)_character.magic.idleMagic / prioCount);
            _character.input.energyRequested.text = toAdd.ToString();
            _character.input.validateInput();

            foreach (var prio in temp)
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

        private WandoosBreakpoint GetCurrentWandoosBreakpoint()
        {
            foreach (var b in _wrapper.Breakpoints.Wandoos)
            {
                if (_character.rebirthTime.totalseconds > b.Time)
                {
                    if (_currentWandoosBreakpoint == null)
                    {
                        _hasWandoosSwapped = false;
                        _currentWandoosBreakpoint = b;
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
            if (breakpoint.Equals("CAPWAN"))
            {
                _character.wandoos98Controller.addCapMagic();
                return;
            }

            if (breakpoint.Equals("WAN"))
            {
                _character.wandoos98Controller.addMagic();
                return;
            }

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

            if (breakpoint.StartsWith("CAPTM"))
            {
                var cap = CalculateTMMagicCap();
                if (cap < _character.magic.idleMagic)
                {
                    Main.LogAllocation($"Allocating {cap} to MagicTM ({_character.magic.idleMagic} idle)");
                    _character.input.energyRequested.text = cap.ToString();
                }
                else
                {
                    Main.LogAllocation($"Allocating {_character.magic.idleMagic} to MagicTM ({cap} cap)");
                    _character.input.energyRequested.text = _character.magic.idleMagic.ToString();
                }
                _character.input.validateInput();
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
            }
        }

        private void ReadEnergyBreakpoint(string breakpoint)
        {

            if (breakpoint.StartsWith("CAPBT"))
            {
                var success = int.TryParse(breakpoint.Split('-')[1], out var index);
                if (!success || index < 0 || index > 11)
                {
                    return;
                }

                if (index <= 5)
                {
                    _character.allOffenseController.trains[index].cap();
                }
                else
                {
                    index -= 5;
                    _character.allDefenseController.trains[index].cap();
                }
            }

            if (breakpoint.StartsWith("BT"))
            {
                var success = int.TryParse(breakpoint.Split('-')[1], out var index);
                if (!success || index < 0 || index > 11)
                {
                    return;
                }

                if (index <= 5)
                {
                    _character.allOffenseController.trains[index].addEnergy();
                }
                else
                {
                    index -= 5;
                    _character.allDefenseController.trains[index].addEnergy();
                }
            }

            if (breakpoint.StartsWith("WAN"))
            {
                _character.wandoos98Controller.addEnergy();
                return;
            }

            if (breakpoint.StartsWith("CAPWAN"))
            {
                _character.wandoos98Controller.addCapEnergy();
                return;
            }

            if (breakpoint.StartsWith("TM"))
            {
                _character.timeMachineController.addEnergy();
                return;
            }

            if (breakpoint.StartsWith("CAPTM"))
            {
                var cap = CalculateTMEnergyCap();
                if (cap < _character.idleEnergy)
                {
                    Main.LogAllocation($"Allocating {cap} to EnergyTM ({_character.idleEnergy} idle)");
                    _character.input.energyRequested.text = cap.ToString();
                }
                else
                {
                    Main.LogAllocation($"Allocating {_character.idleEnergy} to EnergyTM ({cap} cap)");
                    _character.input.energyRequested.text = _character.idleEnergy.ToString();
                }
                _character.input.validateInput();
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

            if (breakpoint.StartsWith("CAPAT"))
            {
                var success = int.TryParse(breakpoint.Split('-')[1], out var index);
                if (!success || index < 0 || index > 4)
                {
                    return;
                }

                var cap = CalculateATCap(index);
                if (cap < _character.idleEnergy)
                {
                    Main.LogAllocation($"Allocating {cap} to AT{index} ({_character.idleEnergy} idle)");
                    _character.input.energyRequested.text = cap.ToString();
                }
                else
                {
                    Main.LogAllocation($"Allocating {_character.idleEnergy} to AT{index} ({cap} cap)");
                    _character.input.energyRequested.text = _character.idleEnergy.ToString();
                }
                _character.input.validateInput();

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

        internal float CalculateTMEnergyCap()
        {
            var formula = 50000 * _character.timeMachineController.baseSpeedDivider() * (1f + _character.machine.levelSpeed + 500) / (
                _character.totalEnergyPower() * _character.hacksController.totalTMSpeedBonus() *
                _character.allChallenges.timeMachineChallenge.TMSpeedBonus() *
                _character.cardsController.getBonus(cardBonus.TMSpeed));
            return formula;
        }

        internal float CalculateTMMagicCap()
        {
            var formula = 50000 * _character.timeMachineController.baseGoldMultiDivider() *
                (1f + _character.machine.levelGoldMulti + 500) / (
                    _character.totalMagicPower() * _character.hacksController.totalTMSpeedBonus() *
                    _character.allChallenges.timeMachineChallenge.TMSpeedBonus() *
                    _character.cardsController.getBonus(cardBonus.TMSpeed));
            return formula;
        }

        internal float CalculateATCap(int index)
        {
            var divisor = GetDivisor(index);
            if (divisor == 0.0)
                return 0;

            var formula = 50f * divisor /
                (Mathf.Sqrt(_character.totalEnergyPower()) * _character.totalAdvancedTrainingSpeedBonus());

            return formula;
        }

        internal void DebugATCap(int index)
        {
            var divisor = _character.advancedTrainingController.getDivisor(index);
            if (divisor == 0.0)
                return;

            var formula = (50f * divisor) /
                (Mathf.Sqrt(_character.totalEnergyPower()) * _character.totalAdvancedTrainingSpeedBonus());


            double num = formula / 50f * Mathf.Sqrt(_character.totalEnergyPower()) * _character.totalAdvancedTrainingSpeedBonus() / _character.advancedTrainingController.getDivisor(index);
            Main.LogAllocation($"Dumping values for AT {index}");
            Main.LogAllocation($"Calculated Energy: {formula}");
            Main.LogAllocation($"Deviation from Game Formula: {num}");
            Main.LogAllocation($"Total Energy Power: {_character.totalEnergyPower()}");
            Main.LogAllocation($"SQRT Energy Power: {Mathf.Sqrt(_character.totalEnergyPower())}");
            Main.LogAllocation($"Advanced Training Speed Bonus: {_character.totalAdvancedTrainingSpeedBonus()}");
            Main.LogAllocation($"Calculated Divisor: {GetDivisor(index)}");
            Main.LogAllocation($"Game Divisor: {_character.advancedTrainingController.getDivisor(index)}");
        }

        internal void DebugTMCap()
        {
            var energy = CalculateTMEnergyCap();
            double num = (double)_character.totalEnergyPower() / (double)_character.timeMachineController.baseSpeedDivider() * ((double)energy / 50000) * (double)_character.hacksController.totalTMSpeedBonus() * (double)_character.allChallenges.timeMachineChallenge.TMSpeedBonus() * (double)_character.cardsController.getBonus(cardBonus.TMSpeed) / (double)(_character.machine.levelSpeed + 1L);
            Main.LogAllocation($"Calculated Energy: {energy}");
            Main.LogAllocation($"Deviation from game formula: {num}");
        }

        private float GetDivisor(int index)
        {
            float baseTime;
            switch (index)
            {
                case 0:
                    baseTime = _character.advancedTrainingController.defense.baseTime;
                    break;
                case 1:
                    baseTime = _character.advancedTrainingController.attack.baseTime;
                    break;
                case 2:
                    baseTime = _character.advancedTrainingController.block.baseTime;
                    break;
                case 3:
                    baseTime = _character.advancedTrainingController.wandoosEnergy.baseTime;
                    break;
                case 4:
                    baseTime = _character.advancedTrainingController.wandoosMagic.baseTime;
                    break;
                default:
                    baseTime = 0.0f;
                    break;
            }

            return baseTime * (_character.advancedTraining.level[index] + 500 + 1f);
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
        [SerializeField] public WandoosBreakpoint[] Wandoos;
        [SerializeField] public int RebirthTime;

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

    [Serializable]
    public class WandoosBreakpoint
    {
        public int Time;
        public int OS;
    }
}
