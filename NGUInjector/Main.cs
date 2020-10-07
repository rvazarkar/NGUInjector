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
using System.Windows.Forms;
using NGUInjector.AllocationProfiles;
using NGUInjector.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Application = UnityEngine.Application;

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
        internal static StreamWriter AllocationWriter;
        internal static StreamWriter PitSpinWriter;
        private YggdrasilManager _yggManager;
        private InventoryManager _invManager;
        private CombatManager _combManager;
        private QuestManager _questManager;
        private CustomAllocation _profile;
        private float _timeLeft = 10.0f;
        internal static SettingsForm settingsForm;
        internal const string Version = "2.5.0";

        internal static bool Test { get; set; }

        private static string _dir;

        private static bool _tempSwapped = false;

        internal static readonly int[] TitanZones = {6, 8, 11, 14, 16, 19, 23, 26, 30, 34, 38, 40, 42};

        internal static FileSystemWatcher ConfigWatcher;
        internal static FileSystemWatcher AllocationWatcher;

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

        internal static void LogPitSpin(string msg)
        {
            PitSpinWriter.WriteLine($"{DateTime.Now.ToShortDateString()}-{ DateTime.Now.ToShortTimeString()} ({Math.Floor(Character.rebirthTime.totalseconds)}s): {msg}");
        }

        internal static void LogAllocation(string msg)
        {
            AllocationWriter.WriteLine($"{DateTime.Now.ToShortDateString()}-{ DateTime.Now.ToShortTimeString()} ({Math.Floor(Character.rebirthTime.totalseconds)}s): {msg}");
        }

        public void Start()
        {
            _dir = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%/Desktop"), "NGUInjector");
            if (!Directory.Exists(_dir))
            {
                Directory.CreateDirectory(_dir);
            }

            var logDir = Path.Combine(_dir, "logs");
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            OutputWriter = new StreamWriter(Path.Combine(logDir, "inject.log")) {AutoFlush = true};
            LootWriter = new StreamWriter(Path.Combine(logDir, "loot.log")) { AutoFlush = true };
            CombatWriter = new StreamWriter(Path.Combine(logDir, "combat.log")) { AutoFlush = true };
            AllocationWriter = new StreamWriter(Path.Combine(logDir, "allocation.log")) { AutoFlush = true};
            PitSpinWriter = new StreamWriter(Path.Combine(logDir, "pitspin.log"), true) {AutoFlush = true};

            try
            {
                Character = FindObjectOfType<Character>();

                Log("Injected");
                LogLoot("Starting Loot Writer");
                LogCombat("Starting Combat Writer");
                LogAllocation("Started Allocation Writer");
                Controller = Character.inventoryController;
                PlayerController = FindObjectOfType<PlayerController>();
                _invManager = new InventoryManager();
                _yggManager = new YggdrasilManager();
                _questManager = new QuestManager();
                _combManager = new CombatManager();
                LoadoutManager.ReleaseLock();
                DiggerManager.ReleaseLock();

                Settings = new SavedSettings(_dir);

                if (!Settings.LoadSettings())
                {
                    var temp = new SavedSettings(null)
                    {
                        PriorityBoosts = new int[] { },
                        YggdrasilLoadout = new int[] { },
                        SwapYggdrasilLoadouts = true,
                        HighestAKZone = 0,
                        SwapTitanLoadouts = true,
                        TitanLoadout = new int[] { },
                        ManageDiggers = true,
                        ManageYggdrasil = true,
                        ManageEnergy = true,
                        ManageMagic = true,
                        ManageInventory = true,
                        ManageGear = true,
                        AutoConvertBoosts = true,
                        SnipeZone = 0,
                        FastCombat = false,
                        PrecastBuffs = true,
                        AutoFight = false,
                        AutoQuest = true,
                        AutoQuestITOPOD = false,
                        AllowMajorQuests = false,
                        GoldDropLoadout = new int[] {},
                        AutoMoneyPit = false,
                        AutoSpin = false,
                        MoneyPitLoadout = new int[] {},
                        AutoRebirth = false,
                        ManageWandoos = false,
                        InitialGoldZone = -1,
                        MoneyPitThreshold = 1e5,
                        NextGoldSwap = false,
                        BoostBlacklist = new int[] {},
                        GoldZone = 0,
                        CombatMode = 0,
                        RecoverHealth = false,
                        SnipeBossOnly = true,
                        AllowZoneFallback = false,
                        QuestFastCombat = true,
                        AbandonMinors = false,
                        MinorAbandonThreshold = 30,
                        QuestCombatMode = 0,
                        AutoBuyEM = false,
                        AutoSpellSwap = false,
                        CounterfeitThreshold = 400,
                        SpaghettiThreshold = 30,
                        BloodNumberThreshold = 1e10,
                        CubePriority = 0,
                        CombatEnabled = false,
                        GlobalEnabled = true,
                        QuickDiggers = new int[] {},
                        QuickLoadout = new int[] {},
                        UseButterMajor = false,
                        ManualMinors =  false,
                        UseButterMinor = false,
                        ActivateFruits = true,
                        ManageR3 = true,
                        WishPriorities = new int[] {},
                        BeastMode = true
                    };

                    Settings.MassUpdate(temp);

                    Log($"Created default settings");
                }
                settingsForm = new SettingsForm();

                _profile = new CustomAllocation(_dir);
                _profile.ReloadAllocation();

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
                    settingsForm.UpdateFromSettings(Settings);
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
                Settings.LoadSettings();

                settingsForm.UpdateFromSettings(Settings);
                settingsForm.Show();

                InvokeRepeating("AutomationRoutine", 0.0f, 10.0f);
                InvokeRepeating("SnipeZone", 0.0f, .1f);
                InvokeRepeating("MonitorLog", 0.0f, 1f);
                InvokeRepeating("QuickStuff", 0.0f, .5f);
                InvokeRepeating("ShowBoostProgress", 0.0f, 60.0f);
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

            settingsForm.UpdateProgressBar((int)Math.Floor(_timeLeft / 10 * 100));

            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (!settingsForm.Visible)
                {
                    settingsForm.Show();
                }
                settingsForm.BringToFront();
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                Settings.GlobalEnabled = !Settings.GlobalEnabled;
                settingsForm.UpdateActive(Settings.GlobalEnabled);
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                QuickSave();
            }

            if (Input.GetKeyDown(KeyCode.F7))
            {
                QuickLoad();
            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                Settings.AutoQuestITOPOD = !Settings.AutoQuestITOPOD;
                settingsForm.UpdateITOPOD(Settings.AutoQuestITOPOD);
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                DumpEquipped();
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                if (Settings.QuickLoadout.Length > 0)
                {
                    if (_tempSwapped)
                    {
                        Log("Restoring Previous Loadout");
                        LoadoutManager.RestoreTempLoadout();
                    }
                    else
                    {
                        Log("Equipping Quick Loadout");
                        LoadoutManager.SaveTempLoadout();
                        LoadoutManager.ChangeGear(Settings.QuickLoadout);
                    }
                }

                if (Settings.QuickDiggers.Length > 0)
                {
                    if (_tempSwapped)
                    {
                        Log("Equipping Previous Diggers");
                        DiggerManager.RestoreTempDiggers();
                    }
                    else
                    {
                        Log("Equipping Quick Diggers");
                        DiggerManager.SaveTempDiggers();
                        DiggerManager.EquipDiggers(Settings.QuickDiggers);
                    }
                }

                _tempSwapped = !_tempSwapped;
            }

            //if (Input.GetKeyDown(KeyCode.F11))
            //{
            //    
            //}
        }

        private void QuickSave()
        {
            Log("Writing quicksave and json");
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

            Character.saveLoad.saveGamestateToSteamCloud();
        }

        private void QuickLoad()
        {
            var filename = Path.Combine(_dir, "NGUSave.txt");
            if (!File.Exists(filename))
            {
                Log("Quicksave doesn't exist");
                return;
            }

            Log("Loading quicksave");
            string base64Data;
            try
            {
                base64Data = File.ReadAllText(filename);
            }
            catch (Exception e)
            {
                Log($"Failed to read quicksave: {e.Message}");
                return;
            }

            try
            {
                var saveDataFromString = Character.importExport.getSaveDataFromString(base64Data);
                var dataFromString = Character.importExport.getDataFromString(base64Data);

                if ((dataFromString == null || dataFromString.version < 361) &&
                    Application.platform != RuntimePlatform.WindowsEditor)
                {
                    Log("Bad save version");
                    return;
                }

                if (dataFromString.version > Character.getVersion())
                {
                    Log("Bad save version");
                    return;
                }

                Character.saveLoad.loadintoGame(saveDataFromString);
            }
            catch (Exception e)
            {
                Log($"Failed to load quicksave: {e.Message}");
            }
        }

        void QuickStuff()
        {
            //Turn on autoattack if we're in ITOPOD and its not on
            if (Settings.AutoQuestITOPOD && Character.adventureController.zone >= 1000 && !Character.adventure.autoattacking && !Settings.CombatEnabled)
            {
                Character.adventureController.idleAttackMove.setToggle();
            }

            if (Settings.AutoFight)
            {
                var needsAllocation = false;
                var bc = Character.bossController;
                if (!bc.isFighting && !bc.nukeBoss)
                {
                    if (Character.bossID == 0)
                        needsAllocation = true;

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

                if (needsAllocation)
                {
                    if (Settings.ManageGear)
                        _profile.EquipGear();
                    if (Settings.ManageEnergy)
                        _profile.AllocateEnergy();
                    if (Settings.ManageMagic)
                        _profile.AllocateMagic();
                    if (Settings.ManageDiggers && Character.buttons.diggers.interactable)
                    {
                        _profile.EquipDiggers();
                        DiggerManager.RecapDiggers();
                    }

                    if (Settings.ManageR3 && Character.buttons.hacks.interactable) 
                        _profile.AllocateR3();

                    if (Settings.ManageWandoos && Character.buttons.wandoos.interactable)
                        _profile.SwapOS();
                }
            }

            if (Settings.AutoMoneyPit)
            {
                MoneyPitManager.CheckMoneyPit();
            }

            if (Settings.AutoSpin)
            {
                MoneyPitManager.DoDailySpin();
            }

            if (Settings.AutoQuestITOPOD)
            {
                MoveToITOPOD();
            }

            if (Settings.AutoSpellSwap)
            {
                var spaghetti = (Character.bloodMagicController.lootBonus() - 1) * 100;
                var counterfeit = ((Character.bloodMagicController.goldBonus() - 1)) * 100;
                var number = Character.bloodMagic.rebirthPower;
                Character.bloodMagic.rebirthAutoSpell = Settings.BloodNumberThreshold >= number;
                Character.bloodMagic.goldAutoSpell = Settings.CounterfeitThreshold >= counterfeit;
                Character.bloodMagic.lootAutoSpell = Settings.SpaghettiThreshold >= spaghetti;
                Character.bloodSpells.updateGoldToggleState();
                Character.bloodSpells.updateLootToggleState();
                Character.bloodSpells.updateRebirthToggleState();
            }
        }

        void AutomationRoutine()
        {
            try
            {
                if (!Settings.GlobalEnabled)
                {
                    _timeLeft = 10f;
                    return;
                }

                if (Settings.ManageInventory)
                {
                    var converted = Character.inventory.GetConvertedInventory().ToArray();
                    var boostSlots = _invManager.GetBoostSlots(converted);
                    _invManager.EnsureFiltered(converted);
                    _invManager.ManageConvertibles(converted);
                    _invManager.MergeEquipped();
                    _invManager.MergeInventory(converted);
                    _invManager.MergeBoosts(converted);
                    _invManager.MergeGuffs(converted);
                    _invManager.BoostInventory(boostSlots);
                    _invManager.BoostInfinityCube();
                    _invManager.ManageBoostConversion(boostSlots);
                }

                if (Settings.SwapTitanLoadouts)
                {
                    LoadoutManager.TryTitanSwap();
                    DiggerManager.TryTitanSwap();
                }

                if (Settings.ManageYggdrasil && Character.buttons.yggdrasil.interactable)
                {
                    _yggManager.ManageYggHarvest();
                    _yggManager.CheckFruits();
                }

                if (Settings.AutoBuyEM)
                {
                    //We haven't unlocked custom purchases yet
                    if (Character.highestBoss < 17) return;
                    //Magic isn't unlocked yet
                    if (Character.highestBoss < 37)
                    {
                        var ePurchase = Character.energyPurchases;
                        var total = ePurchase.customAllCost();
                        var numPurchases = Math.Floor((double)(Character.realExp / total));
                        if (numPurchases > 0)
                        {
                            var ePurchaseMethod = ePurchase.GetType().GetMethod("buyCustomAll",
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (ePurchaseMethod != null)
                            {
                                Log($"Buying {numPurchases} exp purchases");
                                for (var i = 0; i < numPurchases; i++)
                                {
                                    ePurchaseMethod.Invoke(ePurchase, null);
                                }
                            }
                        }
                        
                    }
                    else
                    {
                        var ePurchase = Character.energyPurchases;
                        var mPurchase = Character.magicPurchases;
                        var total = ePurchase.customAllCost() + mPurchase.customAllCost();
                        var numPurchases = Math.Floor((double)(Character.realExp / total));
                        if (numPurchases > 0)
                        {
                            var ePurchaseMethod = ePurchase.GetType().GetMethod("buyCustomAll",
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            var mPurchaseMethod = mPurchase.GetType().GetMethod("buyCustomAll",
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (ePurchaseMethod != null && mPurchaseMethod != null)
                            {
                                Log($"Buying {numPurchases} e/m purchases");
                                for (var i = 0; i < numPurchases; i++)
                                {
                                    ePurchaseMethod.Invoke(ePurchase, null);
                                    mPurchaseMethod.Invoke(mPurchase, null);
                                }
                            }
                        }
                        
                    }
                }

                if (Settings.ManageGear) 
                    _profile.EquipGear();
                if (Settings.ManageEnergy)
                    _profile.AllocateEnergy();
                if (Settings.ManageMagic)
                    _profile.AllocateMagic();
                if (Settings.ManageR3)
                    _profile.AllocateR3();
                
                if (Settings.ManageDiggers && Character.buttons.diggers.interactable)
                {
                    _profile.EquipDiggers();
                    DiggerManager.RecapDiggers();
                }
                    
                if (Settings.ManageWandoos && Character.buttons.wandoos.interactable)
                    _profile.SwapOS();

                if (Settings.AutoQuest && Character.buttons.beast.interactable)
                {
                    var converted = Character.inventory.GetConvertedInventory().ToArray();
                    _invManager.ManageQuestItems(converted);
                    _questManager.CheckQuestTurnin();
                    _questManager.ManageQuests();
                }

                if (Settings.AutoRebirth)
                {
                    _profile.DoRebirth();
                }

                Character.refreshMenus();

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
            if (!Settings.GlobalEnabled)
                return;

            //If tm ever drops to 0, reset our gold loadout stuff
            if (Character.machine.realBaseGold == 0.0 && !Settings.NextGoldSwap)
            {
                Log("Resetting Gold Loadout");
                Settings.NextGoldSwap = true;
                settingsForm.UpdateGoldLoadout(Settings.NextGoldSwap);
            }

            //This logic should trigger only if Time Machine is ready
            if (Character.buttons.brokenTimeMachine.interactable)
            {
                //Hit our initial gold zone first to get TM started
                if (Character.machine.realBaseGold == 0.0 && CombatManager.IsZoneUnlocked(Settings.InitialGoldZone) && Settings.InitialGoldZone >= 0)
                {
                    _combManager.ManualZone(Settings.InitialGoldZone, false, false, false, true);
                    return;
                }

                //Go to our gold loadout zone next to get a high gold drop
                if (Settings.NextGoldSwap)
                {
                    if (CombatManager.IsZoneUnlocked(Settings.GoldZone) && !ZoneIsTitan(Settings.GoldZone) && Settings.GoldZone >= 0)
                    {
                        if (LoadoutManager.TryGoldDropSwap())
                        {
                            _combManager.ManualZone(Settings.GoldZone, true, false, false, true);
                            return;
                        }
                    }
                }
            }

            var questZone = _questManager.IsQuesting();
            if (questZone > 0)
            {
                if (Settings.QuestCombatMode == 0)
                {
                    _combManager.ManualZone(questZone, false, false, false, Settings.QuestFastCombat);
                }
                else
                {
                    _combManager.IdleZone(questZone, false, false);
                }

                return;
            }

            if (!Settings.CombatEnabled)
                return;

            if (Settings.SnipeZone < 0)
                return;

            var tempZone = Settings.SnipeZone;
            if (Settings.SnipeZone < 1000)
            {
                if (!CombatManager.IsZoneUnlocked(Settings.SnipeZone))
                {
                    if (!Settings.AllowZoneFallback) return;

                    for (var i = Character.adventureController.zoneDropdown.options.Count - 2; i >= 0; i--)
                    {
                        if (!ZoneIsTitan(i))
                        {
                            tempZone = i;
                            break;
                        }
                    }
                }
            }
            
            if (Settings.CombatMode == 0)
            {
                _combManager.ManualZone(tempZone, Settings.SnipeBossOnly, Settings.RecoverHealth, Settings.PrecastBuffs, Settings.FastCombat);
            }
            else
            {
                _combManager.IdleZone(tempZone, Settings.SnipeBossOnly, Settings.RecoverHealth);
            }
        }

        private void MoveToITOPOD()
        {
            if (!Settings.GlobalEnabled)
                return;

            if (_questManager.IsQuesting() >= 0)
                return;

            if (Settings.CombatEnabled)
                return;

            if (Character.buttons.brokenTimeMachine.interactable)
            {
                if (Character.machine.realBaseGold == 0.0 && CombatManager.IsZoneUnlocked(Settings.InitialGoldZone) && Settings.InitialGoldZone > 0)
                {
                    return;
                }

                if (Settings.NextGoldSwap)
                {
                    if (CombatManager.IsZoneUnlocked(Settings.GoldZone) && !ZoneIsTitan(Settings.GoldZone) && Settings.GoldZone > 0)
                    {
                        return;
                    }
                }
            }

            //If we're not in ITOPOD, move there if its set
            if (Character.adventureController.zone >= 1000 || !Settings.AutoQuestITOPOD) return;
            Log($"Moving to ITOPOD to idle.");
            _combManager.MoveToZone(1000);
        }

        private void DumpEquipped()
        {
            var list = new List<int>
            {
                Character.inventory.head.id,
                Character.inventory.chest.id,
                Character.inventory.legs.id,
                Character.inventory.boots.id,
                Character.inventory.weapon.id
            };

            if (Character.inventoryController.weapon2Unlocked())
            {
                list.Add(Character.inventory.weapon2.id);
            }

            foreach (var acc in Character.inventory.accs)
            {
                list.Add(acc.id);
            }

            list.RemoveAll(x => x == 0);

            Log($"Equipped Items: [{string.Join(", ", list.Select(x => x.ToString()).ToArray())}]");
        }

        internal static bool ZoneIsTitan(int zone)
        {
            return TitanZones.Contains(zone);
        }

        public void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 200, 40), $"Injected");
            GUI.Label(new Rect(10, 20, 200, 40), $"Automation - {(Settings.GlobalEnabled ? "Active" : "Inactive")}");
            GUI.Label(new Rect(10, 30, 200, 40), $"Next Loop - {_timeLeft:00.0}s");
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
                var result = line;
                if (result.Contains("\n"))
                {
                    result = result.Split('\n').Last();
                }

                var sb = new StringBuilder(result);
                sb.Replace("<color=blue>", "");
                sb.Replace("<b>", "");
                sb.Replace("</color>", "");
                sb.Replace("</b>", "");

                LogLoot(sb.ToString());
                log[i] = $"{line}<b></b>";
            }
        }

        void ShowBoostProgress()
        {
            var boostSlots = _invManager.GetBoostSlots(Character.inventory.GetConvertedInventory().ToArray());
            _invManager.ShowBoostProgress(boostSlots);
        }

        public void OnApplicationQuit()
        {
            CancelInvoke("AutomationRoutine");
            CancelInvoke("SnipeZone");
            CancelInvoke("MonitorLog");
            CancelInvoke("QuickStuff");
        }
    }
}
