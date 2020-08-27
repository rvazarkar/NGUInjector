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
                    id = x.id
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
        private AllocationProfile _profile;
        
        private Rect _windowRect = new Rect(20, 20, 500,400);

        private bool _optionsVisible = false;

        private static string _dir;

        private bool _active;
        internal static bool SnipeActive;
        
        private bool _manageEnergy;
        private bool _manageMagic;
        
        private bool _manageYggdrasilLoadout;
        private bool _manageInventory;
        private bool _manageYggdrasil;
        private readonly Dictionary<int, string> _titanList = new Dictionary<int, string>();


        internal static int HighestAk;
        internal static int SnipeZoneTarget;
        private static bool _snipeWithBuffs;
        private static bool _manageTitanLoadouts;
        private static int[] _boostedItems;

        private static FileSystemWatcher _configWatcher;

        internal static bool SnipeWithBuffs
        {
            get => _snipeWithBuffs;
            set
            {
                _snipeWithBuffs = value;
                SaveSettings();
            }
        }

        internal static bool ManageTitanLoadouts
        {
            get => _manageTitanLoadouts;
            set
            {
                _manageTitanLoadouts = value;
                SaveSettings();
            }
        }


        public void Start()
        {
            _dir = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%/Desktop"), "NGUInjector");
            OutputWriter = new StreamWriter(Path.Combine(_dir, "inject.log"));
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
                _profile = new TwentyFourHourRebirth();

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

                _manageEnergy = false;
                _manageMagic = false;
                _manageYggdrasilLoadout = false;
                _manageInventory = true;
                _manageYggdrasil = true;

                if (!LoadSettings())
                {
                    SnipeZoneTarget = -1;
                    _snipeWithBuffs = true;
                    HighestAk = 0;
                    _manageTitanLoadouts = false;
                    _boostedItems = new int[] { };
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

        static bool LoadSettings()
        {
            var path = Path.Combine(_dir, "settings.json");
            if (File.Exists(path))
            {
                try
                {
                    var settings = JsonUtility.FromJson<SavedSettings>(File.ReadAllText(path));
                    HighestAk = settings.HighestAKZone;
                    SnipeZoneTarget = settings.SnipeZone;
                    _snipeWithBuffs = settings.PrecastBuffs;
                    _manageTitanLoadouts = settings.SwapTitanLoadouts;
                    _boostedItems = settings.BoostIDs;
                    OutputWriter.WriteLine($"Loaded settings: {JsonUtility.ToJson(settings, true)}");
                    return true;
                }
                catch
                {
                    return false;
                }

            }

            return false;
        }

        static void SaveSettings()
        {
            var path = Path.Combine(_dir, "settings.json");
            //remove filtered items
            var settings = new SavedSettings
            {
                SnipeZone = SnipeZoneTarget,
                HighestAKZone = HighestAk,
                PrecastBuffs = SnipeWithBuffs,
                SwapTitanLoadouts = _manageTitanLoadouts,
                BoostIDs = _boostedItems
            };
            var serialized = JsonUtility.ToJson(settings, true);
            using (var writer = new StreamWriter(path))
            {
                writer.Write(serialized);
                writer.Flush();
            }
        }

        public void OnGUI()
        {
            if (_optionsVisible)
                _windowRect = GUI.Window(0, _windowRect, DoGui, "Injector Settings");

            GUI.Label(new Rect(10, 10, 200, 40), $"Injected");
            GUI.Label(new Rect(10, 20, 200, 40), $"Automation - {(_active ? "Active" : "Inactive")}");
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
            _manageEnergy = GUILayout.Toggle(_manageEnergy, "Manage Energy");
            _manageMagic = GUILayout.Toggle(_manageMagic, "Manage Magic");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            ManageTitanLoadouts = GUILayout.Toggle(ManageTitanLoadouts, "Swap Loadout For Titan (Slot 2)");
            _manageYggdrasilLoadout = GUILayout.Toggle(_manageYggdrasilLoadout, "Swap Loadout For Yggdrasil (Slot 3)");
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
                if (!_active)
                    return;

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
                    //Walderp Items
                    _invManager.BoostInventory(
                        _boostedItems, converted);
                    _invManager.BoostInfinityCube();
                }

                if (_manageYggdrasil)
                {
                    _yggManager.ManageYggHarvest(_manageYggdrasilLoadout);
                    _yggManager.CheckFruits();
                }

                if (_manageTitanLoadouts)
                    _invManager.SwapLoadoutForTitans();

                if (_manageEnergy)
                    _profile.AllocateEnergy();
                if (_manageMagic)
                    _profile.AllocateMagic();
                
            }
            catch (Exception e)
            {
                OutputWriter.WriteLine(e.Message);
                OutputWriter.WriteLine(e.StackTrace);
                OutputWriter.Flush();
            }
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
