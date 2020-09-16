using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using NGUInjector.AllocationProfiles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NGUInjector
{
    internal class Main : MonoBehaviour
    {
        internal static InventoryController Controller;
        internal static Character Character;
        internal static PlayerController PlayerController;
        internal static StreamWriter OutputWriter;
        internal static StreamWriter LootWriter;
        internal static StreamWriter CombatWriter;
        private YggdrasilManager _yggManager;
        private InventoryManager _invManager;
        private CombatManager _combManager;
        private QuestManager _questManager;
        private CustomAllocation _profile;
        private float _timeLeft = 10.0f;

        private Rect _windowRect = new Rect(20, 20, 500,400);

        private bool _optionsVisible;

        private static string _dir;

        private bool _active;
        
        private readonly Dictionary<int, string> _titanList = new Dictionary<int, string>();

        internal static FileSystemWatcher ConfigWatcher;
        internal static FileSystemWatcher AllocationWatcher;

        internal static bool SnipeActive { get; set; }

        internal static bool IgnoreNextChange { get; set; }

        internal static SavedSettings Settings;

        internal static void Log(string msg)
        {
            OutputWriter.WriteLine($"{ DateTime.Now.ToShortDateString()}-{ DateTime.Now.ToShortTimeString()} ({Math.Floor(Character.rebirthTime.totalseconds)}s): {msg}");
        }

        internal static void LogLoot(string msg)
        {
            LootWriter.WriteLine($"{ DateTime.Now.ToShortDateString()}-{ DateTime.Now.ToShortTimeString()} ({Math.Floor(Character.rebirthTime.totalseconds)}s): {msg}");
        }

        internal static void LogCombat(string msg)
        {
            CombatWriter.WriteLine($"{DateTime.Now.ToShortDateString()}-{ DateTime.Now.ToShortTimeString()} ({Math.Floor(Character.rebirthTime.totalseconds)}s): {msg}");
        }

        public void Start()
        {
            _dir = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%/Desktop"), "NGUInjector");
            OutputWriter = new StreamWriter(Path.Combine(_dir, "inject.log")) {AutoFlush = true};
            LootWriter = new StreamWriter(Path.Combine(_dir, "loot.log")) { AutoFlush = true };
            CombatWriter = new StreamWriter(Path.Combine(_dir, "combat.log")) { AutoFlush = true };
            try
            {
                if (!Directory.Exists(_dir))
                {
                    Directory.CreateDirectory(_dir);
                }

                Character = FindObjectOfType<Character>();

                Log("Injected");
                LogLoot("Starting Loot Writer");
                LogCombat("Starting Combat Writer");
                Controller = Character.inventoryController;
                PlayerController = FindObjectOfType<PlayerController>();
                _invManager = new InventoryManager();
                _yggManager = new YggdrasilManager();
                _questManager = new QuestManager();
                _combManager = new CombatManager();
                LoadoutManager.ReleaseLock();
                DiggerManager.ReleaseLock();
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

                Settings = new SavedSettings(_dir);
                _active = true;

                if (!Settings.LoadSettings())
                {
                    var temp = new SavedSettings(null)
                    {
                        BoostIDs = new int[] { },
                        BoostBlacklist = new int[] { },
                        YggdrasilLoadout = new int[] { },
                        SwapYggdrasilLoadouts = true,
                        HighestAKZone = 0,
                        SwapTitanLoadouts = true,
                        TitanLoadout = new int[] { },
                        SnipeZone = -1,
                        PrecastBuffs = true,
                        FastCombat = false,
                        AutoFight = false,
                        ManageDiggers = true,
                        ManageYggdrasil = true,
                        ManageEnergy = true,
                        ManageMagic = true,
                        ManageInventory = true,
                        ManageGear = true
                    };

                    Settings.MassUpdate(temp);

                    Log($"Created default settings");
                }

                ConfigWatcher = new FileSystemWatcher
                {
                    Path = _dir,
                    Filter = "settings.json",
                    NotifyFilter = NotifyFilters.LastWrite,
                    EnableRaisingEvents = true
                };

                ConfigWatcher.Changed += (sender, args) =>
                {
                    if (IgnoreNextChange)
                    {
                        IgnoreNextChange = false;
                        return;
                    }
                    Settings.LoadSettings();
                };

                AllocationWatcher = new FileSystemWatcher
                {
                    Path = _dir,
                    Filter = "allocation.json",
                    NotifyFilter = NotifyFilters.LastWrite,
                    EnableRaisingEvents = true
                };

                AllocationWatcher.Changed += (sender, args) => { LoadAllocation(); };

                Settings.SaveSettings();

                InvokeRepeating("AutomationRoutine", 0.0f, 10.0f);
                InvokeRepeating("SnipeZone", 0.0f, .1f);
                InvokeRepeating("MonitorLog", 0.0f, 1f);
            }
            catch (Exception e)
            {
                Log(e.Message);
                Log(e.StackTrace);
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

            if (Input.GetKeyDown(KeyCode.KeypadDivide))
            {
                var data = Character.importExport.getBase64Data();
                using (var writer = new StreamWriter(Path.Combine(_dir, "NGUSave.txt")))
                {
                    writer.WriteLine(data);
                }

                data = JsonUtility.ToJson(Character.importExport.gameStateToData());
                using (var writer = new StreamWriter(Path.Combine(_dir, "NGUSave.json")))
                {
                    writer.WriteLine(data);
                }
            }
        }

        void AutomationRoutine()
        {
            try
            {
                if (!_active)
                {
                    _timeLeft = 10f;
                    return;
                }

                //Turn on autoattack if we're in ITOPOD and its not on
                if (Character.adventureController.zone >= 1000 && !Character.adventure.autoattacking)
                {
                    Character.adventureController.idleAttackMove.setToggle();
                }

                if (Settings.AutoFight)
                {
                    var bc = Character.bossController;
                    if (!bc.isFighting && !bc.nukeBoss)
                    {
                        if (bc.character.attack / 5.0 > bc.character.bossDefense && bc.character.defense / 5.0 > bc.character.bossAttack)
                            bc.startNuke();
                        else
                        {
                            if (bc.character.attack > (bc.character.bossDefense * 1.4) && bc.character.defense > bc.character.bossAttack * 1.4)
                            {
                                bc.beginFight();
                                bc.stopButton.gameObject.SetActive(true);
                            }
                        }
                    }
                }

                if (Settings.ManageInventory)
                {
                    var converted = Character.inventory.GetConvertedInventory(Controller).ToArray();
                    _invManager.EnsureFiltered(converted);
                    _invManager.ManageConvertibles(converted);
                    _invManager.MergeEquipped();
                    _invManager.MergeInventory(converted);
                    _invManager.MergeBoosts(converted);
                    _invManager.ManageQuestItems(converted);
                    _invManager.MergeGuffs();
                    _invManager.BoostAccessories();
                    _invManager.BoostEquipped();
                    _invManager.BoostInventory(converted);
                    _invManager.BoostInfinityCube();
                    _invManager.ChangeBoostConversion(converted);
                }

                if (Settings.SwapTitanLoadouts)
                {
                    LoadoutManager.TryTitanSwap();
                    DiggerManager.TryTitanSwap();
                }

                if (Settings.SwapYggdrasilLoadouts)
                {
                    _yggManager.ManageYggHarvest();
                    _yggManager.CheckFruits();
                }

                if (Settings.ManageGear)
                    _profile.EquipGear();
                if (Settings.ManageEnergy)
                    _profile.AllocateEnergy();
                if (Settings.ManageMagic)
                    _profile.AllocateMagic();
                if (Settings.ManageDiggers)
                    _profile.EquipDiggers();

                _questManager.CheckQuestTurnin();

            }
            catch (Exception e)
            {
                Log(e.Message);
                Log(e.StackTrace);
            }
            _timeLeft = 10f;
        }

        private void LoadAllocation()
        {
            _profile.ReloadAllocation();
        }

        private void SnipeZone()
        {
            _combManager.SnipeZone();
        }

        private void AkBack()
        {
            if (Settings.HighestAKZone == 0)
                return;
            Settings.HighestAKZone -= 1;
            Settings.SaveSettings();
        }

        private void AkForward()
        {
            if (Settings.HighestAKZone == 11)
                return;
            Settings.HighestAKZone += 1;
            Settings.SaveSettings();
        }

        private void ZoneBack()
        {
            if (Settings.SnipeZone == -1)
                return;
            Settings.SnipeZone -= 1;
            Settings.SaveSettings();
        }

        private void ZoneForward()
        {
            if (Settings.SnipeZone == 44)
                return;
            Settings.SnipeZone += 1;
            Settings.SaveSettings();
        }

        public void OnGUI()
        {
            if (_optionsVisible)
                _windowRect = GUI.Window(0, _windowRect, DoGui, "Injector Settings");

            GUI.Label(new Rect(10, 10, 200, 40), $"Injected");
            GUI.Label(new Rect(10, 20, 200, 40), $"Automation - {(_active ? "Active" : "Inactive")}");
            GUI.Label(new Rect(10, 30, 200, 40), $"Next Loop - {_timeLeft:00.0}s");
        }

        void DoGui(int windowId)
        {
            var centered = new GUIStyle("label") {alignment = TextAnchor.MiddleCenter};

            _active = GUILayout.Toggle(_active, "Global Enable");

            GUILayout.BeginHorizontal();
            Settings.ManageInventory = GUILayout.Toggle(Settings.ManageInventory, "Manage Inventory");
            Settings.ManageYggdrasil = GUILayout.Toggle(Settings.ManageYggdrasil, "Manage Yggdrasil");
            Settings.AutoFight = GUILayout.Toggle(Settings.AutoFight, "Auto Fight Bosses");
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            SnipeActive = GUILayout.Toggle(SnipeActive, "Zone Boss Sniping");
            Settings.PrecastBuffs = GUILayout.Toggle(Settings.PrecastBuffs, "Precast Buffs");
            Settings.FastCombat = GUILayout.Toggle(Settings.FastCombat, "Fast Combat");
            GUILayout.EndHorizontal();


            GUILayout.Label("Zone to Snipe");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<"))
            {
                ZoneBack();
            }
            GUILayout.Label(Character.adventureController.zoneName(Settings.SnipeZone), centered);
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
            GUILayout.Label(_titanList[Settings.HighestAKZone], centered);
            if (GUILayout.Button(">"))
            {
                AkForward();
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            Settings.ManageEnergy = GUILayout.Toggle(Settings.ManageEnergy, "Manage Energy");
            Settings.ManageMagic = GUILayout.Toggle(Settings.ManageMagic, "Manage Magic");
            Settings.ManageGear = GUILayout.Toggle(Settings.ManageGear, "Manage Gear");
            Settings.ManageDiggers = GUILayout.Toggle(Settings.ManageDiggers, "Manage Diggers");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Settings.SwapTitanLoadouts = GUILayout.Toggle(Settings.SwapTitanLoadouts, "Swap Loadout For Titan");
            Settings.SwapYggdrasilLoadouts = GUILayout.Toggle(Settings.SwapYggdrasilLoadouts, "Swap Loadout For Yggdrasil");
            GUILayout.EndHorizontal();

            //if (GUILayout.Button("Test"))
            //{
            //    Character.itemInfo.makeLevelledLoot(168, 1);
            //}

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
                var result = Regex.Replace(line, @"\r\n?|\n", "");
                LogLoot(result);
                log[i] = $"{line}<b></b>";
            }
        }
    }
}
