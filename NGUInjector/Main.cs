using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using NGUInjector.AllocationProfiles;
using NGUInjector.Managers;
using UnityEngine;
using UnityEngine.UI;
using Application = UnityEngine.Application;

namespace NGUInjector
{
    internal class Main : MonoBehaviour
    {
        internal static InventoryController Controller;
        internal static Character Character;
        internal static PlayerController PlayerController;
        internal static ArbitraryController ArbitraryController;
        internal static StreamWriter OutputWriter;
        internal static StreamWriter LootWriter;
        internal static StreamWriter CombatWriter;
        internal static StreamWriter AllocationWriter;
        internal static StreamWriter PitSpinWriter;
        internal static StreamWriter CardsWriter;
        internal static Main reference;
        private YggdrasilManager _yggManager;
        private InventoryManager _invManager;
        private CombatManager _combManager;
        private QuestManager _questManager;
        private CardManager _cardManager;
        private CookingManager _cookingManager;
        private ConsumablesManager _consumablesManager;
        private static CustomAllocation _profile;
        private float _timeLeft = 10.0f;
        internal static SettingsForm settingsForm;
        internal static WishManager WishManager;
        internal const string Version = "3.6.14";
        private static int _furthestZone;


        internal static bool Test { get; set; }

        private static string _dir;
        private static string _profilesDir;

        private static bool _tempSwapped = false;

        internal static FileSystemWatcher ConfigWatcher;
        internal static FileSystemWatcher AllocationWatcher;
        internal static FileSystemWatcher ZoneWatcher;

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
            if (!Settings.DebugAllocation) return;
            AllocationWriter.WriteLine($"{DateTime.Now.ToShortDateString()}-{ DateTime.Now.ToShortTimeString()} ({Math.Floor(Character.rebirthTime.totalseconds)}s): {msg}");
        }

        internal static void LogCard(string msg)
        {
            CardsWriter.WriteLine($"{DateTime.Now.ToShortDateString()}-{ DateTime.Now.ToShortTimeString()} ({Math.Floor(Character.rebirthTime.totalseconds)}s): {msg}");
        }
        internal static string GetProfilesDir()
        {
            return _profilesDir;
        }

        internal void Unload()
        {
            try
            {
                CancelInvoke("AutomationRoutine");
                CancelInvoke("SnipeZone");
                CancelInvoke("MonitorLog");
                CancelInvoke("QuickStuff");
                CancelInvoke("SetResnipe");
                CancelInvoke("ShowBoostProgress");


                LootWriter.Close();
                CombatWriter.Close();
                AllocationWriter.Close();
                PitSpinWriter.Close();
                CardsWriter.Close();
                settingsForm.Close();
                settingsForm.Dispose();

                ConfigWatcher.Dispose();
                AllocationWatcher.Dispose();
                ZoneWatcher.Dispose();
            }
            catch (Exception e)
            {
                Log(e.Message);
            }
            OutputWriter.Close();
        }

        public void Start()
        {
            try
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

                OutputWriter = new StreamWriter(Path.Combine(logDir, "inject.log")) { AutoFlush = true };
                LootWriter = new StreamWriter(Path.Combine(logDir, "loot.log")) { AutoFlush = true };
                CombatWriter = new StreamWriter(Path.Combine(logDir, "combat.log")) { AutoFlush = true };
                AllocationWriter = new StreamWriter(Path.Combine(logDir, "allocation.log")) { AutoFlush = true };
                PitSpinWriter = new StreamWriter(Path.Combine(logDir, "pitspin.log"), true) { AutoFlush = true };
                CardsWriter = new StreamWriter(Path.Combine(logDir, "cards.log")) { AutoFlush = true };

                _profilesDir = Path.Combine(_dir, "profiles");
                if (!Directory.Exists(_profilesDir))
                {
                    Directory.CreateDirectory(_profilesDir);
                }


                var oldPath = Path.Combine(_dir, "allocation.json");
                var newPath = Path.Combine(_profilesDir, "default.json");

                if (File.Exists(oldPath) && !File.Exists(newPath))
                {
                    File.Move(oldPath, newPath);
                }
            }
            catch (Exception e)
            {
                Log(e.Message);
                Log(e.StackTrace);
                Loader.Unload();
                return;
            }

            try
            {
                Character = FindObjectOfType<Character>();

                Log("Injected");
                LogLoot("Starting Loot Writer");
                LogCombat("Starting Combat Writer");
                LogCard("Starting Card Writer");
                Controller = Character.inventoryController;
                PlayerController = FindObjectOfType<PlayerController>();
                ArbitraryController = FindObjectOfType<ArbitraryController>();
                _invManager = new InventoryManager();
                _yggManager = new YggdrasilManager();
                _questManager = new QuestManager();
                _combManager = new CombatManager();
                _cardManager = new CardManager();
                _cookingManager = new CookingManager();
                _consumablesManager = new ConsumablesManager();
                LoadoutManager.ReleaseLock();
                DiggerManager.ReleaseLock();

                Settings = new SavedSettings(_dir);

                if (!Settings.LoadSettings())
                {
                    var temp = new SavedSettings(null)
                    {
                        PriorityBoosts = new int[] { },
                        YggdrasilLoadout = new int[] { },
                        SwapYggdrasilLoadouts = false,
                        SwapTitanLoadouts = false,
                        TitanLoadout = new int[] { },
                        ManageDiggers = true,
                        ManageYggdrasil = false,
                        ManageEnergy = true,
                        ManageMagic = true,
                        ManageInventory = true,
                        ManageGear = true,
                        AutoConvertBoosts = true,
                        SnipeZone = 0,
                        FastCombat = false,
                        PrecastBuffs = true,
                        AutoFight = false,
                        AutoQuest = false,
                        AutoQuestITOPOD = false,
                        AllowMajorQuests = false,
                        GoldDropLoadout = new int[] { },
                        AutoMoneyPit = false,
                        AutoSpin = false,
                        MoneyPitLoadout = new int[] { },
                        AutoRebirth = false,
                        ManageWandoos = false,
                        MoneyPitThreshold = 1e5,
                        DoGoldSwap = false,
                        BoostBlacklist = new int[] { },
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
                        CastBloodSpells = false,
                        IronPillThreshold = 10000,
                        BloodMacGuffinAThreshold = 6,
                        BloodMacGuffinBThreshold = 6,
                        CubePriority = 0,
                        CombatEnabled = false,
                        GlobalEnabled = false,
                        QuickDiggers = new int[] { },
                        QuickLoadout = new int[] { },
                        UseButterMajor = false,
                        ManualMinors = false,
                        UseButterMinor = false,
                        ActivateFruits = false,
                        ManageR3 = true,
                        WishPriorities = new int[] { },
                        WishBlacklist = new int[] { },
                        BeastMode = true,
                        ManageNGUDiff = true,
                        AllocationFile = "default",
                        TitanGoldTargets = new bool[ZoneHelpers.TitanZones.Length],
                        ManageGoldLoadouts = false,
                        ResnipeTime = 3600,
                        TitanMoneyDone = new bool[ZoneHelpers.TitanZones.Length],
                        TitanSwapTargets = new bool[ZoneHelpers.TitanZones.Length],
                        GoldCBlockMode = false,
                        DebugAllocation = false,
                        AdventureTargetITOPOD = false,
                        ITOPODRecoverHP = false,
                        ITOPODCombatMode = 0,
                        ITOPODBeastMode = true,
                        ITOPODFastCombat = true,
                        ITOPODPrecastBuffs = false,
                        DisableOverlay = false,
                        OptimizeITOPODFloor = false,
                        YggSwapThreshold = 1,
                        UpgradeDiggers = true,
                        BlacklistedBosses = new int[0],
                        SpecialBoostBlacklist = new int[0],
                        MoreBlockParry = false,
                        WishSortOrder = false,
                        WishSortPriorities = false,
                        ManageMayo = false,
                        TrashCards = false,
                        TrashAdventureCards = false,
                        AutoCastCards = false,
                        CardsTrashQuality = 0,
                        CardSortOrder = new string[0],
                        CardSortEnabled = false,
                        TrashCardCost = 0,
                        DontCastCardType = new string[0],
                        TrashChunkers = false,
                        HackAdvance = false
                    };

                    Settings.MassUpdate(temp);

                    Log($"Created default settings");
                }

                settingsForm = new SettingsForm();

                if (string.IsNullOrEmpty(Settings.AllocationFile))
                {
                    Settings.SetSaveDisabled(true);
                    Settings.AllocationFile = "default";
                    Settings.SetSaveDisabled(false);
                }

                if (Settings.TitanGoldTargets == null || Settings.TitanGoldTargets.Length == 0)
                {
                    Settings.SetSaveDisabled(true);
                    Settings.TitanGoldTargets = new bool[ZoneHelpers.TitanZones.Length];
                    Settings.SetSaveDisabled(false);
                }

                if (Settings.TitanMoneyDone == null || Settings.TitanMoneyDone.Length == 0)
                {
                    Settings.SetSaveDisabled(true);
                    Settings.TitanMoneyDone = new bool[ZoneHelpers.TitanZones.Length];
                    Settings.SetSaveDisabled(false);
                }

                if (Settings.TitanSwapTargets == null || Settings.TitanSwapTargets.Length == 0)
                {
                    Settings.SetSaveDisabled(true);
                    Settings.TitanSwapTargets = new bool[ZoneHelpers.TitanZones.Length];
                    Settings.SetSaveDisabled(false);
                }

                if (Settings.SpecialBoostBlacklist == null)
                {
                    Settings.SetSaveDisabled(true);
                    Settings.SpecialBoostBlacklist = new int[0];
                    Settings.SetSaveDisabled(false);
                }

                if (Settings.BlacklistedBosses == null)
                {
                    Settings.SetSaveDisabled(true);
                    Settings.BlacklistedBosses = new int[0];
                    Settings.SetSaveDisabled(false);
                }

                WishManager = new WishManager();

                LoadAllocation();
                LoadAllocationProfiles();

                ZoneWatcher = new FileSystemWatcher
                {
                    Path = _dir,
                    Filter = "zoneOverride.json",
                    NotifyFilter = NotifyFilters.LastWrite,
                    EnableRaisingEvents = true
                };

                ZoneWatcher.Changed += (sender, args) =>
                {
                    Log(_dir);
                    ZoneStatHelper.CreateOverrides(_dir);
                };

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
                    LoadAllocation();
                };

                AllocationWatcher = new FileSystemWatcher
                {
                    Path = _profilesDir,
                    Filter = "*.json",
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                    EnableRaisingEvents = true
                };

                AllocationWatcher.Changed += (sender, args) => { LoadAllocation(); };
                AllocationWatcher.Created += (sender, args) => { LoadAllocationProfiles(); };
                AllocationWatcher.Deleted += (sender, args) => { LoadAllocationProfiles(); };
                AllocationWatcher.Renamed += (sender, args) => { LoadAllocationProfiles(); };

                Settings.SaveSettings();
                Settings.LoadSettings();

                LogAllocation("Started Allocation Writer");

                ZoneStatHelper.CreateOverrides(_dir);

                settingsForm.UpdateFromSettings(Settings);
                settingsForm.Show();

                InvokeRepeating("AutomationRoutine", 0.0f, 10.0f);
                InvokeRepeating("SnipeZone", 0.0f, .1f);
                InvokeRepeating("MonitorLog", 0.0f, 1f);
                InvokeRepeating("QuickStuff", 0.0f, .5f);
                InvokeRepeating("ShowBoostProgress", 0.0f, 60.0f);
                InvokeRepeating("SetResnipe", 0f, 1f);


                reference = this;
            }
            catch (Exception e)
            {
                Log(e.ToString());
                Log(e.StackTrace);
                Log(e.InnerException.ToString());
            }
        }

        internal static void UpdateForm(SavedSettings newSettings)
        {
            settingsForm.UpdateFromSettings(newSettings);
        }

        public void Update()
        {
            _timeLeft -= Time.deltaTime;
            _combManager.UpdateFightTimer(Time.deltaTime);

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

            // F11 reserved for testing
            //if (Input.GetKeyDown(KeyCode.F11))
            //{
            //    Character.realExp += 10000;
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

            var saveTime = File.GetLastWriteTime(filename);
            var s = DateTime.Now.Subtract(saveTime);
            var secDiff = (int)s.TotalSeconds;
            if (secDiff > 120)
            {
                var diff = saveTime.GetPrettyDate();

                var confirmResult = MessageBox.Show($"Last quicksave was {diff}. Are you sure you want to load?",
                    "Load Quicksave"
                    , MessageBoxButtons.YesNo);

                if (confirmResult == DialogResult.No)
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

        // Stuff on a very short timer
        void QuickStuff()
        {
            if (!Settings.GlobalEnabled)
                return;

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
                    _profile.DoAllocations();
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
                Character.bloodMagic.rebirthAutoSpell = Settings.BloodNumberThreshold > 0 && number < Settings.BloodNumberThreshold;
                Character.bloodMagic.goldAutoSpell = Settings.CounterfeitThreshold > 0 && counterfeit < Settings.CounterfeitThreshold;
                Character.bloodMagic.lootAutoSpell = Settings.SpaghettiThreshold > 0 && spaghetti < Settings.SpaghettiThreshold;
                Character.bloodSpells.updateGoldToggleState();
                Character.bloodSpells.updateLootToggleState();
                Character.bloodSpells.updateRebirthToggleState();
            }
        }

        // Runs every 10 seconds, our main loop
        void AutomationRoutine()
        {
            try
            {
                if (!Settings.GlobalEnabled)
                {
                    _timeLeft = 10f;
                    return;
                }

                ZoneHelpers.OptimizeITOPOD();

                if (Settings.ManageInventory && !Controller.midDrag)
                {
                    var converted = Character.inventory.GetConvertedInventory().ToArray();
                    var boostSlots = _invManager.GetBoostSlots(converted);
                    _invManager.EnsureFiltered(converted);
                    _invManager.ManageConvertibles(converted);
                    _invManager.MergeEquipped(converted);
                    _invManager.MergeInventory(converted);
                    _invManager.MergeBoosts(converted);
                    _invManager.MergeGuffs(converted);
                    _invManager.BoostInventory(boostSlots);
                    _invManager.BoostInfinityCube();
                    _invManager.ManageBoostConversion(boostSlots);
                }

                //if (Settings.ManageInventory && !Controller.midDrag)
                //{
                //    var watch = Stopwatch.StartNew();
                //    var converted = Character.inventory.GetConvertedInventory().ToArray();
                //    Log($"Creating CI: {watch.ElapsedMilliseconds}");
                //    watch = Stopwatch.StartNew();
                //    var boostSlots = _invManager.GetBoostSlots(converted);
                //    Log($"Get Boost Slots: {watch.ElapsedMilliseconds}");
                //    watch = Stopwatch.StartNew();
                //    _invManager.EnsureFiltered(converted);
                //    Log($"Filtering: {watch.ElapsedMilliseconds}");
                //    watch = Stopwatch.StartNew();
                //    _invManager.ManageConvertibles(converted);
                //    Log($"Convertibles: {watch.ElapsedMilliseconds}");
                //    watch = Stopwatch.StartNew();
                //    _invManager.MergeEquipped(converted);
                //    Log($"Merge Equipped: {watch.ElapsedMilliseconds}");
                //    watch = Stopwatch.StartNew();
                //    _invManager.MergeInventory(converted);
                //    Log($"Merge Inventory: {watch.ElapsedMilliseconds}");
                //    watch = Stopwatch.StartNew();
                //    _invManager.MergeBoosts(converted);
                //    Log($"Merge Boosts: {watch.ElapsedMilliseconds}");
                //    watch = Stopwatch.StartNew();
                //    _invManager.MergeGuffs(converted);
                //    Log($"Merge Guffs: {watch.ElapsedMilliseconds}");
                //    watch = Stopwatch.StartNew();
                //    _invManager.BoostInventory(boostSlots);
                //    Log($"Boost Inventory: {watch.ElapsedMilliseconds}");
                //    watch = Stopwatch.StartNew();
                //    _invManager.BoostInfinityCube();
                //    Log($"Boost Cube: {watch.ElapsedMilliseconds}");
                //    watch = Stopwatch.StartNew();
                //    _invManager.ManageBoostConversion(boostSlots);
                //    Log($"Boost Conversion: {watch.ElapsedMilliseconds}");
                //    watch.Stop();
                //}

                if (Settings.SwapTitanLoadouts || Settings.ManageGoldLoadouts && Settings.NeedsGoldSwap())
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

                    var ePurchase = Character.energyPurchases;
                    var mPurchase = Character.magicPurchases;
                    var r3Purchase = Character.res3Purchases;

                    var energy = ePurchase.customAllCost() > 0;
                    var r3 = Character.res3.res3On && r3Purchase.customAllCost() > 0;
                    var magic = Character.highestBoss >= 37 && mPurchase.customAllCost() > 0;

                    long total = 0;

                    if (energy)
                    {
                        total += ePurchase.customAllCost();
                    }

                    if (magic)
                    {
                        total += mPurchase.customAllCost();
                    }

                    if (r3)
                    {
                        total += r3Purchase.customAllCost();
                    }

                    if (total > 0)
                    {
                        var numPurchases = Math.Floor((double)(Character.realExp / total));

                        if (numPurchases > 0)
                        {
                            var t = string.Empty;
                            if (energy)
                            {
                                t += "/exp";
                            }

                            if (magic)
                            {
                                t += "/magic";
                            }

                            if (r3)
                            {
                                t += "/res3";
                            }

                            t = t.Substring(1);

                            Log($"Buying {numPurchases} {t} purchases");
                            for (var i = 0; i < numPurchases; i++)
                            {
                                if (energy)
                                {
                                    var ePurchaseMethod = ePurchase.GetType().GetMethod("buyCustomAll",
                                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                    ePurchaseMethod?.Invoke(ePurchase, null);
                                }

                                if (magic)
                                {
                                    var mPurchaseMethod = mPurchase.GetType().GetMethod("buyCustomAll",
                                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                    mPurchaseMethod?.Invoke(mPurchase, null);
                                }

                                if (r3)
                                {
                                    var r3PurchaseMethod = r3Purchase.GetType().GetMethod("buyCustomAll",
                                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                    r3PurchaseMethod?.Invoke(r3Purchase, null);
                                }
                            }
                        }
                    }
                }

                _profile.DoAllocations();

                _profile.CastBloodSpells();

                if (Settings.AutoQuest && Character.buttons.beast.interactable)
                {
                    var converted = Character.inventory.GetConvertedInventory().ToArray();
                    if (!Character.inventoryController.midDrag)
                        _invManager.ManageQuestItems(converted);
                    _questManager.CheckQuestTurnin();
                    _questManager.ManageQuests();
                }

                if (Settings.AutoRebirth)
                {
                    _profile.DoRebirth();
                }

                if (Settings.ManageMayo)
                {
                    _cardManager.CheckManas();
                }

                if (Settings.TrashCards)
                {
                    _cardManager.TrashCards();
                }
                if (Settings.AutoCastCards)
                {
                    _cardManager.CastCards();
                }
                if (Settings.CardSortEnabled && Settings.CardSortOrder.Length > 0)
                {
                    _cardManager.SortCards();
                }
                if (Settings.ManageCooking)
                {
                    _cookingManager.manageFood();
                }
            }
            catch (Exception e)
            {
                Log(e.Message);
                Log(e.StackTrace);
            }
            _timeLeft = 10f;
        }

        internal static void LoadAllocation()
        {
            _profile = new CustomAllocation(_profilesDir, Settings.AllocationFile);
            try
            {
                _profile.ReloadAllocation();
            }
            catch (Exception e)
            {
                Log(e.Message);
            }
        }

        private static void LoadAllocationProfiles()
        {
            var files = Directory.GetFiles(_profilesDir);
            settingsForm.UpdateProfileList(files.Select(Path.GetFileNameWithoutExtension).ToArray(), Settings.AllocationFile);
        }

        private void SnipeZone()
        {
            if (!Settings.GlobalEnabled)
                return;

            //If tm ever drops to 0, reset our gold loadout stuff
            if (Character.machine.realBaseGold == 0.0 && !Settings.DoGoldSwap)
            {
                Log("Time Machine Gold is 0. Lets reset gold snipe zone.");
                Settings.DoGoldSwap = true;
                Settings.TitanMoneyDone = new bool[ZoneHelpers.TitanZones.Length];
            }

            //This logic should trigger only if Time Machine is ready
            if (Character.buttons.brokenTimeMachine.interactable)
            {
                if (Character.machine.realBaseGold == 0.0)
                {
                    _combManager.ManualZone(0, false, false, false, true, false);
                    return;
                }
                //Go to our gold loadout zone next to get a high gold drop
                if (Settings.ManageGoldLoadouts && Settings.DoGoldSwap && Settings.GoldDropLoadout.Length > 0)
                {
                    if (LoadoutManager.TryGoldDropSwap())
                    {
                        var bestZone = ZoneStatHelper.GetBestZone();
                        _furthestZone = ZoneHelpers.GetMaxReachableZone(false);

                        _combManager.ManualZone(bestZone.Zone, true, bestZone.FightType == 1, false, bestZone.FightType == 2, false);
                        return;
                    }
                }
            }

            var questZone = _questManager.IsQuesting();
            if (!Settings.CombatEnabled || Settings.AdventureTargetITOPOD || !ZoneHelpers.ZoneIsTitan(Settings.SnipeZone) ||
                !CombatManager.IsZoneUnlocked(Settings.SnipeZone) ||
                ZoneHelpers.ZoneIsTitan(Settings.SnipeZone) &&
                !ZoneHelpers.TitanSpawningSoon(Array.IndexOf(ZoneHelpers.TitanZones, Settings.SnipeZone)))
            {
                if (questZone > 0)
                {
                    if (Settings.QuestCombatMode == 0)
                    {
                        _combManager.ManualZone(questZone, false, false, false, Settings.QuestFastCombat, Settings.BeastMode);
                    }
                    else
                    {
                        _combManager.IdleZone(questZone, false, false);
                    }

                    return;
                }
            }

            if (!Settings.CombatEnabled)
                return;

            var tempZone = Settings.AdventureTargetITOPOD ? 1000 : Settings.SnipeZone;
            if (tempZone < 1000)
            {
                if (!CombatManager.IsZoneUnlocked(Settings.SnipeZone))
                {
                    tempZone = Settings.AllowZoneFallback ? ZoneHelpers.GetMaxReachableZone(false) : 1000;
                }
                else
                {
                    if (ZoneHelpers.ZoneIsTitan(Settings.SnipeZone) && !ZoneHelpers.TitanSpawningSoon(Array.IndexOf(ZoneHelpers.TitanZones, Settings.SnipeZone)))
                    {
                        tempZone = 1000;
                    }
                }
            }

            if (tempZone >= 1000)
            {
                if (Settings.ITOPODCombatMode == 0)
                {
                    _combManager.ManualZone(tempZone, false, Settings.ITOPODRecoverHP, Settings.ITOPODPrecastBuffs, Settings.ITOPODFastCombat, Settings.ITOPODBeastMode);
                }
                else
                {
                    _combManager.IdleZone(tempZone, false, Settings.ITOPODRecoverHP);
                }

                return;
            }

            if (Settings.CombatMode == 0)
            {
                _combManager.ManualZone(tempZone, Settings.SnipeBossOnly, Settings.RecoverHealth, Settings.PrecastBuffs, Settings.FastCombat, Settings.BeastMode);
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

            if (Settings.DoGoldSwap)
                return;

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
            var items = $"[{string.Join(", ", list.Select(x => x.ToString()).ToArray())}]";

            Log($"Equipped Items: {items}");
            Clipboard.SetText(items);
        }

        public void OnGUI()
        {
            if (Settings.DisableOverlay) return;
            GUI.Label(new Rect(10, 0, 200, 40), $"Automation - {(Settings.GlobalEnabled ? "Active" : "Inactive")}");
            GUI.Label(new Rect(10, 10, 200, 40), $"Next Loop - {_timeLeft:00.0}s");
            GUI.Label(new Rect(10, 20, 200, 40), $"Profile - {Settings.AllocationFile}");
        }

        public void MonitorLog()
        {
            var bLog = Character.adventureController.log;
            var type = bLog.GetType().GetField("Eventlog",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var val = type?.GetValue(bLog);
            if (val == null)
                return;

            var log = (List<string>)val;
            for (var i = log.Count - 1; i >= 0; i--)
            {
                var line = log[i];
                if (!line.Contains("dropped")) continue;
                if (line.Contains("gold")) continue;
                if (line.ToLower().Contains("special boost")) continue;
                if (line.ToLower().Contains("toughness boost")) continue;
                if (line.ToLower().Contains("power boost")) continue;
                if (line.Contains("EXP")) continue;
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

        public void SetResnipe()
        {
            if (Settings.ResnipeTime == 0 && !Settings.GoldCBlockMode) return;

            if (Settings.GoldCBlockMode)
            {
                var furthest = ZoneHelpers.GetMaxReachableZone(false);
                if (furthest > _furthestZone)
                {
                    Settings.DoGoldSwap = true;
                    _furthestZone = furthest;
                }

                return;
            }

            if (Math.Abs(Character.rebirthTime.totalseconds - Settings.ResnipeTime) <= 1)
            {
                Settings.DoGoldSwap = true;
            }
        }

        public void ShowBoostProgress()
        {
            var boostSlots = _invManager.GetBoostSlots(Character.inventory.GetConvertedInventory().ToArray());
            try
            {
                _invManager.ShowBoostProgress(boostSlots);
            }
            catch (Exception e)
            {
                Log(e.Message);
                Log(e.StackTrace);
            }

        }

        public void OnApplicationQuit()
        {
            Loader.Unload();
        }

        public void ResetBoostProgress()
        {

        }
    }
}
