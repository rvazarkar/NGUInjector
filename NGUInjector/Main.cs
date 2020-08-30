using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using NGUInjector.AllocationProfiles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NGUInjector
{
    public class ih
    {
        internal int slot { get; set; }
        internal string name { get; set; }
        internal int level { get; set; }
        internal bool locked { get; set; }
        internal int id { get; set; }
        internal Equipment equipment { get; set; }
    }
    public static class Extension
    {
        public static ih MaxItem(this IEnumerable<ih> items)
        {
            return items.Aggregate(
                new { max = int.MinValue, t = (ih)null },
                (state, el) =>
                {
                    var current = el.locked ? el.level + 101 : el.level;
                    return current > state.max ? new { max = current, t = el } : state;
                }).t;
        }

        public static IEnumerable<ih> GetConvertedInventory(this Inventory inv, InventoryController controller)
        {
            return inv.inventory.Select((x, i) => new ih
                {
                    level = x.level,
                    locked = !x.removable,
                    name = controller.itemInfo.itemName[x.id],
                    slot = i,
                    id = x.id,
                    equipment = x
                }).Where(x => x.id != 0);
        }

        public static T GetPV<T>(this EnemyAI ai, string val)
        {
            var type = ai.GetType().GetField(val,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return (T) type?.GetValue(ai);
        }
    }
    
    internal class Main : MonoBehaviour
    {
        internal static InventoryController Controller;
        internal static Character Character;
        internal static PlayerController PlayerController;
        internal static StreamWriter OutputWriter;
        internal static StreamWriter LootWriter;
        private YggdrasilManager _yggManager;
        private InventoryManager _invManager;
        private CombatManager _combManager;
        private CustomAllocation _profile;
        private float _timeLeft = 15.0f;

        private Rect _windowRect = new Rect(20, 20, 500,400);

        private bool _optionsVisible;

        private static string _dir;

        private bool _active;
        internal static bool SnipeActive;

        private bool _manageInventory;
        private bool _manageYggdrasil;
        
        private readonly Dictionary<int, string> _titanList = new Dictionary<int, string>();


        internal static int HighestAk;
        internal static int SnipeZoneTarget;
        private static int[] _boostedItems;

        private static FileSystemWatcher _configWatcher;
        private FileSystemWatcher _allocationWatcher;

        internal static bool SnipeWithBuffs { get; set; }

        internal static bool ManageTitanLoadouts { get; set; }

        internal static bool ManageYggdrasilLoadouts { get; set; }

        internal static bool ManageEnergy { get; set; }

        internal static bool ManageMagic { get; set; }

        internal static bool FastCombat { get; set; }

        internal static bool ManageGear { get; set; }

        private static SavedSettings _currentSettings;


        public void Start()
        {
            _dir = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%/Desktop"), "NGUInjector");
            OutputWriter = new StreamWriter(Path.Combine(_dir, "inject.log")) {AutoFlush = true};
            LootWriter = new StreamWriter(Path.Combine(_dir, "loot.log"));
            OutputWriter.WriteLine("Injected");
            OutputWriter.Flush();
            LootWriter.WriteLine("Starting Loot Writer");
            LootWriter.Flush();
            try
            {
                if (!Directory.Exists(_dir))
                {
                    Directory.CreateDirectory(_dir);
                }

                Character = FindObjectOfType<Character>();
                Controller = Character.inventoryController;
                PlayerController = FindObjectOfType<PlayerController>();
                _invManager = new InventoryManager();
                _yggManager = new YggdrasilManager(_invManager);
                _combManager = new CombatManager();
                LoadoutManager.ReleaseLock();
                _profile = new CustomAllocation(_dir);
                _profile.ReloadAllocation();
                

                _titanList.Add(0, "None");
                _titanList.Add(1, "GRB");
                _titanList.Add(2, "GCT");
                _titanList.Add(3, "Jake");
                _titanList.Add(4, "UUG");
                _titanList.Add(5, "Walderp");
                _titanList.Add(6, "Beast");
                _titanList.Add(7, "Greasy Nerd");
                _titanList.Add(8, "Godmother");
                _titanList.Add(9, "Exile");
                _titanList.Add(10, "IT HUNGERS");
                _titanList.Add(11, "Rock Lobster");
                _titanList.Add(12, "Amalgamate");

                _active = true;
                SnipeActive = false;

                ManageEnergy = false;
                ManageMagic = false;
                ManageGear = false;
                _manageInventory = true;
                _manageYggdrasil = true;
                LoadoutManager.TitanLoadout = new int[] { };
                LoadoutManager.YggdrasilLoadout = new int[] { };

                if (!LoadSettings())
                {
                    SnipeZoneTarget = -1;
                    SnipeWithBuffs = true;
                    HighestAk = 0;
                    ManageTitanLoadouts = false;
                    _boostedItems = new int[] { };
                    LoadoutManager.TitanLoadout = new int[] { };
                    LoadoutManager.YggdrasilLoadout = new int[] { };
                    SaveSettings();
                    OutputWriter.WriteLine($"Loaded default settings");
                }

                _configWatcher = new FileSystemWatcher
                {
                    Path = _dir,
                    Filter = "settings.json",
                    NotifyFilter = NotifyFilters.LastWrite,
                    EnableRaisingEvents = true
                };

                _configWatcher.Changed += (sender, args) => { LoadSettings(); };

                _allocationWatcher = new FileSystemWatcher
                {
                    Path = _dir,
                    Filter = "allocation.json",
                    NotifyFilter = NotifyFilters.LastWrite,
                    EnableRaisingEvents = true
                };

                _allocationWatcher.Changed += (sender, args) => { LoadAllocation(); };

                InvokeRepeating("AutomationRoutine", 0.0f, 15.0f);
                InvokeRepeating("SnipeZone", 0.0f, .1f);
                InvokeRepeating("MonitorLog", 0.0f, 1f);
            }
            catch (Exception e)
            {
                OutputWriter.WriteLine(e);
                OutputWriter.Flush();
            }

        }

        public void Update()
        {
            _timeLeft -= Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                _active = !_active;
            }

            if (Input.GetKeyDown(KeyCode.KeypadMultiply))
            {
                _optionsVisible = !_optionsVisible;
            }
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                _active = false;
                SnipeActive = false;
                Loader.Unload();
            }

            

        }

        private void LoadAllocation()
        {
            _profile.ReloadAllocation();
        }

        static bool LoadSettings()
        {
            var path = Path.Combine(_dir, "settings.json");
            if (File.Exists(path))
            {
                try
                {
                    var settings = JsonUtility.FromJson<SavedSettings>(File.ReadAllText(path));
                    
                    SnipeWithBuffs = settings.PrecastBuffs;
                    ManageTitanLoadouts = settings.SwapTitanLoadouts;
                    ManageYggdrasilLoadouts = settings.SwapYggdrasilLoadouts;
                    _boostedItems = settings.BoostIDs;
                    ManageEnergy = settings.ManageEnergy;
                    ManageMagic = settings.ManageMagic;
                    FastCombat = settings.FastCombat;
                    ManageGear = settings.ManageGear;

                    HighestAk = settings.HighestAKZone;
                    SnipeZoneTarget = settings.SnipeZone;
                    LoadoutManager.TitanLoadout = settings.TitanLoadout;
                    LoadoutManager.YggdrasilLoadout = settings.YggdrasilLoadout;
                    OutputWriter.WriteLine($"Loaded settings: {JsonUtility.ToJson(settings, true)}");
                    _currentSettings = settings;
                    return true;
                }
                catch (Exception e)
                {
                    OutputWriter.WriteLine(e);
                    return false;
                }
            }

            return false;
        }

        static void SaveSettings()
        {
            //OutputWriter.WriteLine(Environment.StackTrace);
            //OutputWriter.Flush();
            var path = Path.Combine(_dir, "settings.json");
            var settings = new SavedSettings
            {
                SnipeZone = SnipeZoneTarget,
                HighestAKZone = HighestAk,
                PrecastBuffs = SnipeWithBuffs,
                SwapTitanLoadouts = ManageTitanLoadouts,
                SwapYggdrasilLoadouts = ManageYggdrasilLoadouts,
                BoostIDs = _boostedItems,
                ManageEnergy = ManageEnergy,
                ManageMagic = ManageMagic,
                ManageGear = ManageGear,
                YggdrasilLoadout = LoadoutManager.YggdrasilLoadout,
                TitanLoadout = LoadoutManager.TitanLoadout,
                FastCombat = FastCombat
            };

            if (!settings.Equals(_currentSettings))
            {
                _currentSettings = settings;
                var serialized = JsonUtility.ToJson(settings, true);
                using (var writer = new StreamWriter(path))
                {
                    writer.Write(serialized);
                    writer.Flush();
                }
            }
            
        }

        public void OnGUI()
        {
            if (_optionsVisible)
                _windowRect = GUI.Window(0, _windowRect, DoGui, "Injector Settings");

            GUI.Label(new Rect(10, 10, 200, 40), $"Injected");
            GUI.Label(new Rect(10, 20, 200, 40), $"Automation - {(_active ? "Active" : "Inactive")}");
            GUI.Label(new Rect(10, 30, 200, 40), $"Next Loop - {_timeLeft:00.0}s");
        }

        private void SnipeZone()
        {
            _combManager.SnipeZone();
        }

        private void AkBack()
        {
            if (HighestAk == 0)
                return;
            HighestAk -= 1;
            SaveSettings();
        }

        private void AkForward()
        {
            if (HighestAk == 11)
                return;
            HighestAk += 1;
            SaveSettings();
        }

        private void ZoneBack()
        {
            if (SnipeZoneTarget == -1)
                return;
            SnipeZoneTarget -= 1;
            SaveSettings();
        }

        private void ZoneForward()
        {
            if (SnipeZoneTarget == 44)
                return;
            SnipeZoneTarget += 1;
            SaveSettings();
        }

        void DoGui(int windowId)
        {
            var centered = new GUIStyle("label") {alignment = TextAnchor.MiddleCenter};

            _active = GUILayout.Toggle(_active, "Global Enable");

            GUILayout.BeginHorizontal();
            _manageInventory = GUILayout.Toggle(_manageInventory, "Manage Inventory");
            _manageYggdrasil = GUILayout.Toggle(_manageYggdrasil, "Manage Yggdrasil");
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            SnipeActive = GUILayout.Toggle(SnipeActive, "Zone Boss Sniping");
            SnipeWithBuffs = GUILayout.Toggle(SnipeWithBuffs, "Precast Buffs");
            FastCombat = GUILayout.Toggle(FastCombat, "Fast Combat");
            GUILayout.EndHorizontal();


            GUILayout.Label("Zone to Snipe");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<"))
            {
                ZoneBack();
            }
            GUILayout.Label(Character.adventureController.zoneName(SnipeZoneTarget), centered);
            if (GUILayout.Button(">"))
            {
                ZoneForward();
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Highest Titan AK (For gear swap)");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<"))
            {
                AkBack();
            }
            GUILayout.Label(_titanList[HighestAk], centered);
            if (GUILayout.Button(">"))
            {
                AkForward();
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            ManageEnergy = GUILayout.Toggle(ManageEnergy, "Manage Energy");
            ManageMagic = GUILayout.Toggle(ManageMagic, "Manage Magic");
            ManageGear = GUILayout.Toggle(ManageGear, "Manage Gear");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            ManageTitanLoadouts = GUILayout.Toggle(ManageTitanLoadouts, "Swap Loadout For Titan");
            ManageYggdrasilLoadouts = GUILayout.Toggle(ManageYggdrasilLoadouts, "Swap Loadout For Yggdrasil");
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Test Swap"))
            {
                _invManager.TestSwap();
            }


            GUI.DragWindow(new Rect(0,0, 10000,10000));
        }

        void MonitorLog()
        {
            var bLog = Character.adventureController.log;
            var type = bLog.GetType().GetField("Eventlog",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var val = type?.GetValue(bLog);
            if (val == null)
                return;

            var log = (List<string>) val;
            for (var i = log.Count - 1; i >= 0; i--)
            {
                var line = log[i];
                if (!line.Contains("dropped")) continue;
                if (line.Contains("gold")) continue;
                if (line.EndsWith("<b></b>")) continue;
                LootWriter.WriteLine($"{DateTime.Now.ToShortDateString()}-{DateTime.Now.ToShortTimeString()}: {line}");
                log[i] = $"{line}<b></b>";
            }
            LootWriter.Flush();
        }

        void AutomationRoutine()
        {
            try
            {
                SaveSettings();
                
                if (!_active)
                {
                    _timeLeft = 15f;
                    return;
                }

                if (Character.adventureController.zone >= 1000 && !Character.adventure.autoattacking)
                {
                    Character.adventureController.idleAttackMove.setToggle();
                }

                GetTotalTrainingCaps();

                if (_manageInventory)
                {
                    var converted = Character.inventory.GetConvertedInventory(Controller).ToArray();
                    _invManager.EnsureFiltered(converted);
                    _invManager.ManagePendant(converted);
                    _invManager.ManageLooty(converted);
                    //_invManager.ManageWandoos(converted);
                    _invManager.MergeEquipped();
                    _invManager.MergeInventory(converted);
                    _invManager.MergeBoosts(converted);
                    _invManager.MergeGuffs();
                    _invManager.BoostAccessories();
                    _invManager.BoostEquipped();
                    _invManager.BoostInventory(
                        _boostedItems, converted);
                    _invManager.BoostInfinityCube();
                }

                if (ManageTitanLoadouts)
                    LoadoutManager.TryTitanSwap();

                if (_manageYggdrasil)
                {
                    _yggManager.ManageYggHarvest();
                    _yggManager.CheckFruits();
                }

                if (ManageGear)
                    _profile.EquipGear();
                if (ManageEnergy)
                    _profile.AllocateEnergy();
                if (ManageMagic)
                    _profile.AllocateMagic();
                
            }
            catch (Exception e)
            {
                OutputWriter.WriteLine(e.Message);
                OutputWriter.WriteLine(e.StackTrace);
                OutputWriter.Flush();
            }
            _timeLeft = 15f;
        }

        void GetTotalTrainingCaps()
        {
            //_trainingCap = Character.training.attackCaps.Sum() + Character.training.defenseCaps.Sum();
        }




        //private void StartPipeServer()
        //{
        //    do
        //    {
        //        _server = new NamedPipeServerStream("NGUPipe", PipeDirection.In, 10);
        //        _server.WaitForConnection();
        //        var br = new BinaryReader(_server);

                

        //        while (true)
        //        {
        //            try
        //            {
        //                var len = (int)br.ReadUInt32();            // Read string length
        //                var data = new string(br.ReadChars(len));    // Read string

        //                if (data.StartsWith("boostequipped"))
        //                {
        //                    outputWriter.WriteLine("Got command boostequipped");
        //                    _controller.applyAllBoosts(-1);
        //                    _controller.applyAllBoosts(-2);
        //                    _controller.applyAllBoosts(-3);
        //                    _controller.applyAllBoosts(-4);
        //                    _controller.applyAllBoosts(-5);

        //                    if (_controller.weapon2Unlocked())
        //                    {
        //                        _controller.applyAllBoosts(-6);
        //                    }

        //                    for (var i = 10000; _controller.accessoryID(i) < _controller.accessorySpaces(); i++)
        //                    {
        //                        _controller.applyAllBoosts(i);
        //                    }

        //                    //_controller.itemInfo.itemName[item.id]
        //                    var msg = "Finished Boosting Equipped Items";
        //                    outputWriter.WriteLine(msg);
        //                    outputWriter.Flush();
        //                }else if (data.StartsWith("boostinventory"))
        //                {
        //                    var argument = data.Split("-");
        //                }
        //                else if (data.StartsWith("close"))
        //                {
        //                    outputWriter.WriteLine("Client Disconnected");
        //                    outputWriter.Flush();
        //                    break;
        //                }
        //                else
        //                {
        //                    break;
        //                }
        //            }
        //            catch
        //            {
        //                outputWriter.WriteLine("Client Disconnected");
        //                outputWriter.Flush();
        //                break;                    // When client disconnects
        //            }
        //        }
        //        _server.Close();
        //        _server.Dispose();
        //    } while (true);
        //}
    }
}
