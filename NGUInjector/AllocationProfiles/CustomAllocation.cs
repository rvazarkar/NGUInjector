using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NGUInjector.Managers;
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
        private AllocationBreakPoint _currentR3Breakpoint;
        private GearBreakpoint _currentGearBreakpoint;
        private DiggerBreakpoint _currentDiggerBreakpoint;
        private WandoosBreakpoint _currentWandoosBreakpoint;
        private WishManager _wishManager;
        private bool _hasGearSwapped;
        private bool _hasDiggerSwapped;
        private bool _hasWandoosSwapped;
        private readonly string _allocationPath;
        private string[] _validEnergyPriorities = { "WAN", "CAPWAN", "TM", "CAPTM", "CAPAT", "AT", "NGU", "CAPNGU", "AUG", "BT", "CAPBT", "CAPAUG", "CAPALLNGU", "BLANK", "WISH" };
        private string[] _validMagicPriorities = { "WAN", "CAPWAN", "BR", "TM", "CAPTM", "NGU", "CAPNGU", "CAPALLNGU", "BLANK", "WISH" };
        private string[] _validR3Priorities = {"HACK", "WISH"};

        public CustomAllocation(string dir)
        {
            var path = Path.Combine(dir, "allocation.json");
            _allocationPath = path;
            _wishManager = new WishManager();
        }

        private List<string> ValidatePriorities(List<string> priorities)
        {
            if (!_character.buttons.brokenTimeMachine.interactable)
            {
                priorities.RemoveAll(x => x.Contains("TM"));
            }

            if (!_character.buttons.wandoos.interactable)
            {
                priorities.RemoveAll(x => x.Contains("WAN"));
            }

            if (!_character.buttons.advancedTraining.interactable)
            {
                priorities.RemoveAll(x => x.Contains("AT"));
            }

            if (!_character.buttons.augmentation.interactable)
            {
                priorities.RemoveAll(x => x.Contains("AUG"));
            }

            if (!_character.buttons.ngu.interactable)
            {
                priorities.RemoveAll(x => x.Contains("NGU"));
            }

            if (!_character.buttons.basicTraining.interactable)
            {
                priorities.RemoveAll(x => x.Contains("BT"));
            }

            if (!_character.buttons.bloodMagic.interactable)
            {
                priorities.RemoveAll(x => x.Contains("BR"));
            }

            if (!_character.buttons.hacks.interactable)
            {
                priorities.RemoveAll(x => x.Contains("HACK"));
            }

            if (!_character.buttons.wishes.interactable)
            {
                priorities.RemoveAll(x => x.Contains("WISH"));
            }

            priorities.RemoveAll(x => x.Contains("AUG") && !IsAUGUnlocked(ParseIndex(x)));
            priorities.RemoveAll(x => x.Contains("BT") && !IsBTUnlocked(ParseIndex(x)));

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
                    _wrapper.Breakpoints.Magic = breakpoints["Magic"].Children.Select(bp => new AllocationBreakPoint { Time = bp["Time"].AsInt, Priorities = bp["Priorities"].AsArray.Children.Select(x => x.Value.ToUpper()).Where(x => _validMagicPriorities.Any(x.StartsWith)).ToArray() }).OrderByDescending(x => x.Time).ToArray();
                    _wrapper.Breakpoints.Energy = breakpoints["Energy"].Children.Select(bp => new AllocationBreakPoint { Time = bp["Time"].AsInt, Priorities = bp["Priorities"].AsArray.Children.Select(x => x.Value.ToUpper()).Where(x => _validEnergyPriorities.Any(x.StartsWith)).ToArray() }).OrderByDescending(x => x.Time).ToArray();
                    _wrapper.Breakpoints.R3 = breakpoints["R3"].Children.Select(bp => new AllocationBreakPoint { Time = bp["Time"].AsInt, Priorities = bp["Priorities"].AsArray.Children.Select(x => x.Value.ToUpper()).Where(x => _validR3Priorities.Any(x.StartsWith)).ToArray() }).OrderByDescending(x => x.Time).ToArray();
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
                        Main.Log($"Loaded custom allocation:\n{_wrapper.Breakpoints.Energy.Length} energy breakpoints\n{_wrapper.Breakpoints.Magic.Length} magic breakpoints\n{_wrapper.Breakpoints.R3.Length} R3 breakpoints\n{_wrapper.Breakpoints.Gear.Length} gear breakpoints\n{_wrapper.Breakpoints.Diggers.Length} digger breakpoints,\n{_wrapper.Breakpoints.Wandoos.Length} wandoos OS breakpoints. \nRebirth at {_wrapper.Breakpoints.RebirthTime}");
                    }
                    else
                    {
                        Main.Log($"Loaded custom allocation:\n{_wrapper.Breakpoints.Energy.Length} energy breakpoints\n{_wrapper.Breakpoints.Magic.Length} magic breakpoints\n{_wrapper.Breakpoints.R3.Length} R3 breakpoints\n{_wrapper.Breakpoints.Gear.Length} gear breakpoints\n{_wrapper.Breakpoints.Diggers.Length} digger breakpoints.\n{_wrapper.Breakpoints.Wandoos.Length} wandoos OS breakpoints. \nNo rebirth time specified");
                    }

                    _currentDiggerBreakpoint = null;
                    _currentEnergyBreakpoint = null;
                    _currentGearBreakpoint = null;
                    _currentWandoosBreakpoint = null;
                    _currentMagicBreakpoint = null;
                    _currentR3Breakpoint = null;

                    if (Main.Settings.ManageEnergy) _character.removeMostEnergy();

                    if (Main.Settings.ManageR3) _character.removeAllRes3();

                    if (Main.Settings.ManageMagic) _character.removeMostMagic();

                    if (Main.Settings.ManageGear)
                        EquipGear();
                    if (Main.Settings.ManageEnergy)
                        AllocateEnergy();
                    if (Main.Settings.ManageMagic && Main.Character.buttons.bloodMagic.interactable)
                        AllocateMagic();
                    if (Main.Settings.ManageDiggers && Main.Character.buttons.diggers.interactable) 
                        EquipDiggers();
                    if (Main.Settings.ManageR3 && Main.Character.buttons.hacks.interactable)
                        AllocateR3();
                }
                catch (Exception e)
                {
                    Main.Log(e.Message);
                    Main.Log(e.StackTrace);
                }
            }
            else
            {
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
    ""R3"": [
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

            if (Main.Settings.SwapYggdrasilLoadouts && Main.Settings.YggdrasilLoadout.Length > 0)
            {
                if (!LoadoutManager.TryYggdrasilSwap() || !DiggerManager.TryYggSwap())
                {
                    Main.Log("Delaying rebirth to wait for ygg loadout/diggers");
                    return;
                }

                YggdrasilManager.HarvestAll();
                LoadoutManager.RestoreGear();
                LoadoutManager.ReleaseLock();
                DiggerManager.RestoreDiggers();
                DiggerManager.ReleaseLock();
            }

            _currentDiggerBreakpoint = null;
            _currentEnergyBreakpoint = null;
            _currentGearBreakpoint = null;
            _currentWandoosBreakpoint = null;
            _currentMagicBreakpoint = null;
            _currentR3Breakpoint = null;

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
                _currentEnergyBreakpoint = bp;
            }

            var temp = ValidatePriorities(bp.Priorities.ToList());
            if (temp.Count > 0) _character.removeMostEnergy();
            else
                return;

            var capPrios = temp.Where(x => x.StartsWith("BR") || x.StartsWith("CAP")).ToArray();
            temp.RemoveAll(x => x.StartsWith("BR") || x.StartsWith("CAP"));

            if (bp.Priorities.Any(x => x.Contains("BT"))) _character.removeAllEnergy();

            foreach (var prio in capPrios)
            {
                ReadEnergyBreakpoint(prio);
            }

            var prioCount = temp.Count;
            var toAdd = (long) Math.Ceiling((double) _character.idleEnergy / prioCount);
            _character.input.energyRequested.text = toAdd.ToString();
            _character.input.validateInput();

            foreach (var prio in temp)
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
                _currentMagicBreakpoint = bp;
            }

            var temp = ValidatePriorities(bp.Priorities.ToList());

            if (temp.Count > 0) _character.removeMostMagic();
            else return;

            var capPrios = temp.Where(x => x.StartsWith("BR") || x.StartsWith("CAP")).ToArray();
            temp.RemoveAll(x => x.StartsWith("BR") || x.StartsWith("CAP"));

            foreach (var prio in capPrios)
            {
                ReadMagicBreakpoint(prio);
            }

            var prioCount = temp.Count;
            var toAdd = (long)Math.Ceiling((double)_character.magic.idleMagic / prioCount);
            _character.input.energyRequested.text = toAdd.ToString();
            _character.input.validateInput();

            foreach (var prio in temp)
            {
                ReadMagicBreakpoint(prio);
            }
        }

        public override void AllocateR3()
        {
            if (_wrapper == null)
                return;

            var bp = GetCurrentR3Breakpoint();
            if (bp == null)
                return;

            if (bp.Time != _currentR3Breakpoint.Time)
            {
                _currentR3Breakpoint = bp;
            }

            var temp = ValidatePriorities(bp.Priorities.ToList());
            if (temp.Count > 0) _character.removeAllRes3();
            else return;

            var prioCount = temp.Count;
            var toAdd = (long)Math.Ceiling((double)_character.res3.idleRes3 / prioCount);
            _character.input.energyRequested.text = toAdd.ToString();
            _character.input.validateInput();

            foreach (var prio in temp)
            {
                ReadR3Breakpoint(prio);
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
                        _currentEnergyBreakpoint = b;
                    }

                    if (!energy && _currentMagicBreakpoint == null)
                    {
                        _currentMagicBreakpoint = b;
                    }
                    return b;
                }
            }

            if (energy)
            {
                _currentEnergyBreakpoint = null;
            }
            else
            {
                _currentMagicBreakpoint = null;
            }
            return null;
        }

        private AllocationBreakPoint GetCurrentR3Breakpoint()
        {
            foreach (var b in _wrapper.Breakpoints.R3)
            {
                var rbTime = _character.rebirthTime.totalseconds;
                if (rbTime > b.Time)
                {
                    if (_currentR3Breakpoint == null)
                    {
                        _character.removeAllRes3();
                        _currentR3Breakpoint = b;
                    }

                    return b;
                }
            }

            _currentR3Breakpoint = null;
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

            _currentGearBreakpoint = null;
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

            _currentDiggerBreakpoint = null;
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

            _currentWandoosBreakpoint = null;
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

        private void ReadR3Breakpoint(string breakpoint)
        {
            if (breakpoint.StartsWith("HACK"))
            {
                var success = int.TryParse(breakpoint.Split('-')[1], out var index);
                if (!success || index < 0 || index > 14)
                {
                    return;
                }

                _character.hacksController.addR3(index, _character.input.energyMagicInput);
            }

            if (breakpoint.StartsWith("WISH"))
            {
                var success = int.TryParse(breakpoint.Split('-')[1], out var index);
                if (!success || index < 0)
                {
                    return;
                }
                var wishID = _wishManager.GetSlot(index);
                if (wishID == -1)
                {
                    return;
                }
                _character.wishesController.addRes3(wishID);
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

            if (breakpoint.StartsWith("CAPALLNGU"))
            {
                for (var i = 0; i < 7; i++)
                {
                    _character.NGUController.NGUMagic[i].cap();
                }

                return;
            }

            if (breakpoint.StartsWith("WISH"))
            {
                var success = int.TryParse(breakpoint.Split('-')[1], out var index);
                if (!success || index < 0)
                {
                    return;
                }
                var wishID = _wishManager.GetSlot(index);
                if (wishID == -1)
                {
                    return;
                }
                _character.wishesController.addMagic(wishID);
            }
        }

        private void ReadEnergyBreakpoint(string breakpoint)
        {

            if (breakpoint.StartsWith("CAPBT"))
            {
                var success = int.TryParse(breakpoint.Split('-')[1], out var index);
                if (!success || index < 0 || index > 11)
                    return;

                if (index <= 5)
                    _character.allOffenseController.trains[index].cap();
                else
                {
                    index -= 6;
                    _character.allDefenseController.trains[index].cap();
                }
            }

            if (breakpoint.StartsWith("BT"))
            {
                var success = int.TryParse(breakpoint.Split('-')[1], out var index);
                if (!success || index < 0 || index > 11)
                    return;

                if (index <= 5)
                    _character.allOffenseController.trains[index].addEnergy();
                else
                {
                    index -= 6;
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

            if (breakpoint.StartsWith("CAPALLNGU"))
            {
                for (var i = 0; i < 9; i++)
                {
                    _character.NGUController.NGU[i].cap();
                }

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

                return;
            }

            if (breakpoint.StartsWith("CAPAUG"))
            {
                var success = int.TryParse(breakpoint.Split('-')[1], out var index);
                if (!success || index < 0 || index > 13)
                {
                    return;
                }

                var augIndex = (int)Math.Floor((double)(index / 2));

                var cap = CalculateAugCap(index);

                if (cap < _character.idleEnergy)
                {
                    Main.LogAllocation($"Allocating {cap} to Aug {index} ({_character.idleEnergy} idle)");
                    _character.input.energyRequested.text = cap.ToString();
                }
                else
                {
                    Main.LogAllocation($"Allocating {_character.idleEnergy} to Aug {index} ({cap} cap)");
                    _character.input.energyRequested.text = _character.idleEnergy.ToString();
                }
                _character.input.validateInput();

                if (index % 2 == 0)
                {
                    _character.augmentsController.augments[augIndex].addEnergyAug();
                }
                else
                {
                    _character.augmentsController.augments[augIndex].addEnergyUpgrade();
                }
            }

            if (breakpoint.StartsWith("WISH"))
            {
                var success = int.TryParse(breakpoint.Split('-')[1], out var index);
                if (!success || index < 0)
                {
                    return;
                }

                var wishID = _wishManager.GetSlot(index);
                if (wishID == -1)
                {
                    return;
                }
                _character.wishesController.addEnergy(wishID);
            }
        }

        internal float CalculateTMEnergyCap()
        {
            var formula = 50000 * _character.timeMachineController.baseSpeedDivider() * (1f + _character.machine.levelSpeed + 500) / (
                _character.totalEnergyPower() * _character.hacksController.totalTMSpeedBonus() *
                _character.allChallenges.timeMachineChallenge.TMSpeedBonus() *
                _character.cardsController.getBonus(cardBonus.TMSpeed));

            if (_character.settings.rebirthDifficulty >= difficulty.sadistic)
            {
                formula *= _character.timeMachineController.sadisticDivider();
            }
            return formula;
        }

        internal float CalculateTMMagicCap()
        {
            var formula = 50000 * _character.timeMachineController.baseGoldMultiDivider() *
                (1f + _character.machine.levelGoldMulti + 500) / (
                    _character.totalMagicPower() * _character.hacksController.totalTMSpeedBonus() *
                    _character.allChallenges.timeMachineChallenge.TMSpeedBonus() *
                    _character.cardsController.getBonus(cardBonus.TMSpeed));

            if (_character.settings.rebirthDifficulty >= difficulty.sadistic)
            {
                formula *= _character.timeMachineController.sadisticDivider();
            }
            return Mathf.Min(Mathf.Ceil((float)formula), (float)9e18);
        }

        internal float CalculateATCap(int index)
        {
            var divisor = GetDivisor(index);
            if (divisor == 0.0)
                return 0;

            if (_character.wishes.wishes[190].level >= 1)
                return 0;

            var formula = 50f * divisor /
                (Mathf.Sqrt(_character.totalEnergyPower()) * _character.totalAdvancedTrainingSpeedBonus());

            return Mathf.Min(Mathf.Ceil((float)formula), (float)9e18);
        }

        //internal float CalculateAugCap(int index)
        //{
        //    var augIndex = (int)Math.Floor((double)(index / 2));
        //}

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
            var num = (double)_character.totalEnergyPower() / (double)_character.timeMachineController.baseSpeedDivider() * ((double)energy / 50000) * (double)_character.hacksController.totalTMSpeedBonus() * (double)_character.allChallenges.timeMachineChallenge.TMSpeedBonus() * (double)_character.cardsController.getBonus(cardBonus.TMSpeed) / (double)(_character.machine.levelSpeed + 1L);
            Main.LogAllocation($"Calculated Energy: {energy}");
            Main.LogAllocation($"Deviation from game formula: {num}");
        }

        internal float CalculateAugCap(int index)
        {
            var augIndex = (int)Math.Floor((double)(index / 2));
            double formula = 0;
            if (index % 2 == 0)
            {
                formula = 50000 * (1f + _character.augments.augs[augIndex].augLevel + 500) /
                    (_character.totalEnergyPower() *
                    (1 + _character.inventoryController.bonuses[specType.Augs]) *
                    _character.inventory.macguffinBonuses[12] *
                    _character.hacksController.totalAugSpeedBonus() *
                    _character.cardsController.getBonus(cardBonus.augSpeed) *
                    _character.adventureController.itopod.totalAugSpeedBonus() *
                    (1.0 + (double)_character.allChallenges.noAugsChallenge.evilCompletions() * 0.0500000007450581));

                if (_character.allChallenges.noAugsChallenge.completions() >= 1)
                {
                    formula /= 1.10000002384186;
                }
                if (_character.allChallenges.noAugsChallenge.evilCompletions() >= _character.allChallenges.noAugsChallenge.maxCompletions)
                {
                    formula /= 1.25;
                }
                if (_character.settings.rebirthDifficulty >= difficulty.sadistic)
                {
                    formula *= (double)_character.augmentsController.augments[augIndex].sadisticDivider();
                }
                if (_character.settings.rebirthDifficulty == difficulty.normal)
                {
                    formula *= _character.augmentsController.normalAugSpeedDividers[augIndex];
                }
                else if (_character.settings.rebirthDifficulty == difficulty.evil)
                {
                    formula *= _character.augmentsController.evilAugSpeedDividers[augIndex];
                }
                else if (_character.settings.rebirthDifficulty == difficulty.sadistic)
                {
                    formula *= _character.augmentsController.sadisticAugSpeedDividers[augIndex];
                }
            }
            else
            {
                formula = 50000 * (1f + _character.augments.augs[augIndex].upgradeLevel + 500) /
                    (_character.totalEnergyPower() *
                    (1 + _character.inventoryController.bonuses[specType.Augs]) *
                    _character.inventory.macguffinBonuses[12] *
                    _character.hacksController.totalAugSpeedBonus() *
                    _character.cardsController.getBonus(cardBonus.augSpeed) *
                    _character.adventureController.itopod.totalAugSpeedBonus() *
                    (1.0 + (double)_character.allChallenges.noAugsChallenge.evilCompletions() * 0.0500000007450581));

                if (_character.allChallenges.noAugsChallenge.completions() >= 1)
                {
                    formula /= 1.10000002384186;
                }
                if (_character.allChallenges.noAugsChallenge.evilCompletions() >= _character.allChallenges.noAugsChallenge.maxCompletions)
                {
                    formula /= 1.25;
                }
                if (_character.settings.rebirthDifficulty >= difficulty.sadistic)
                {
                    formula *= (double)_character.augmentsController.augments[augIndex].sadisticDivider();
                }
                if (_character.settings.rebirthDifficulty == difficulty.normal)
                {
                    formula *= _character.augmentsController.normalUpgradeSpeedDividers[augIndex];

                }
                else if (_character.settings.rebirthDifficulty == difficulty.evil)
                {
                    formula *= _character.augmentsController.evilUpgradeSpeedDividers[augIndex];

                }
                else if (_character.settings.rebirthDifficulty == difficulty.sadistic)
                {
                    formula *= _character.augmentsController.sadisticUpgradeSpeedDividers[augIndex];
                }
            }
            return Mathf.Min(Mathf.Ceil((float)formula), (float)9e18);
        }

        private bool IsBTUnlocked(int index)
        {
            if (index < 0)
            {
                return false;
            }

            if (index > 11)
            {
                return false;
            }

            if (index <= 5)
            {
                if (index == 0)
                    return true;
                return _character.training.attackTraining[index - 1] >= 5000 * index;
            }

            index -= 6;
            if (index == 0)
                return true;
            return _character.training.defenseTraining[index - 1] >= 5000 * index;
        }

        private bool IsAUGUnlocked(int index)
        {
            if (index < 0)
            {
                return false;
            }

            if (index > 13)
            {
                return false;
            }

            var augIndex = (int)Math.Floor((double)(index / 2));

            if (index % 2 == 0)
            {
                return _character.bossID > _character.augmentsController.augments[augIndex].augBossRequired;
            }

            return _character.bossID > _character.augmentsController.augments[augIndex].upgradeBossRequired;
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

        private int ParseIndex(string prio)
        {
            var success = int.TryParse(prio.Split('-')[1], out var index);
            if (success) return index;
            return -1;
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
        [SerializeField] public AllocationBreakPoint[] R3;
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
