using NGUInjector.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UnityEngine;

namespace NGUInjector
{
    [Serializable]
    public class SavedSettings
    {
        [SerializeField] private int _snipeZone = -1;
        [SerializeField] private bool _precastBuffs;
        [SerializeField] private bool _swapTitanLoadouts;
        [SerializeField] private bool _swapYggdrasilLoadouts;
        [SerializeField] private int[] _priorityBoosts;
        [SerializeField] private bool _manageEnergy;
        [SerializeField] private bool _manageMagic;
        [SerializeField] private bool _fastCombat;
        [SerializeField] private bool _manageGear;
        [SerializeField] private bool _manageDiggers;
        [SerializeField] private bool _upgradeDiggers;
        [SerializeField] private bool _manageYggdrasil;
        [SerializeField] private int[] _titanLoadout;
        [SerializeField] private int[] _yggdrasilLoadout;
        [SerializeField] private bool _manageInventory;
        [SerializeField] private bool _autoFight;
        [SerializeField] private bool _autoQuest;
        [SerializeField] private bool _allowMajorQuests;
        [SerializeField] private bool _autoConvertBoosts;
        [SerializeField] private bool _autoQuestItopod;
        [SerializeField] private int[] _goldDropLoadout;
        [SerializeField] private bool _autoMoneyPit;
        [SerializeField] private bool _autoSpin;
        [SerializeField] private int[] _moneyPitLoadout;
        [SerializeField] private bool _autoRebirth;
        [SerializeField] private bool _manageWandoos;
        [SerializeField] private double _moneyPitThreshold;
        [SerializeField] private bool _doGoldSwap;
        [SerializeField] private int[] _boostBlacklist;
        [SerializeField] private bool _snipeBossOnly;
        [SerializeField] private bool _recoverHealth;
        [SerializeField] private int _combatMode;
        [SerializeField] private bool _allowZoneFallback;
        [SerializeField] private bool _abandonMinors;
        [SerializeField] private int _minorAbandonThreshold;
        [SerializeField] private int _questCombatMode;
        [SerializeField] private bool _questFastCombat;
        [SerializeField] private bool _autoSpellSwap;
        [SerializeField] private int _spaghettiThreshold;
        [SerializeField] private int _counterfeitThreshold;
        [SerializeField] private bool _castBloodSpells;
        [SerializeField] private int _ironPillThreshold;
        [SerializeField] private int _bloodMacGuffinAThreshold;
        [SerializeField] private int _bloodMacGuffinBThreshold;
        [SerializeField] private bool _autoBuyEm;
        [SerializeField] private bool _autoBuyAdventure;
        [SerializeField] private double _bloodNumberThreshold;
        [SerializeField] private int[] _quickLoadout;
        [SerializeField] private int[] _quickDiggers;
        [SerializeField] private bool _globalEnabled;
        [SerializeField] private bool _combatEnabled;
        [SerializeField] private bool _useButterMajor;
        [SerializeField] private bool _useButterMinor;
        [SerializeField] private bool _manualMinors;
        [SerializeField] private bool _manageR3;
        [SerializeField] private bool _activateFruits;
        [SerializeField] private int[] _wishPriorities;
        [SerializeField] private int[] _wishBlacklist;
        [SerializeField] private bool _wishSortPriorities;
        [SerializeField] private bool _wishSortOrder;
        [SerializeField] private bool _beastMode;
        [SerializeField] private int _cubePriority;
        [SerializeField] private bool _manageNguDiff;
        [SerializeField] private string _allocationFile;
        [SerializeField] private bool _manageGoldLoadouts;
        [SerializeField] private int _resnipeTime;
        [SerializeField] private bool[] _titanMoneyDone;
        [SerializeField] private bool[] _titanGoldTargets;
        [SerializeField] private bool[] _titanSwapTargets;
        [SerializeField] private bool _goldCBlockMode;
        [SerializeField] private bool _debugAllocation;
        [SerializeField] private bool _optimizeItopodFloor;
        [SerializeField] private int _itopodCombatMode;
        [SerializeField] private bool _itopodBeastMode;
        [SerializeField] private bool _itopodFastCombat;
        [SerializeField] private bool _itopodRecoverHp;
        [SerializeField] private bool _itopodPrecastBuffs;
        [SerializeField] private bool _adventureTargetItopod;
        [SerializeField] private bool _disableOverlay;
        [SerializeField] private int _yggSwapThreshold;
        [SerializeField] private bool _moreBlockParry;
        [SerializeField] private int[] _specialBoostBlacklist;
        [SerializeField] private int[] _blacklistedBosses;
        [SerializeField] private bool _manageMayo;
        [SerializeField] private bool _trashCards;
        [SerializeField] private int _cardsTrashQuality;
        [SerializeField] private bool _autoCastCards;
        [SerializeField] private int _autoCastCardType;
        [SerializeField] private bool _trashAdventureCards;
        [SerializeField] private string[] _cardSortOrder;
        [SerializeField] private bool _cardSortEnabled;
        [SerializeField] private int _trashCardCost;
        [SerializeField] private string[] _dontCastCardType;
        [SerializeField] private bool _trashChunkers;
        [SerializeField] private bool _hackAdvance;
        [SerializeField] private bool _manageCooking;
        [SerializeField] private bool _manageQuestLoadouts;
        [SerializeField] private bool _manageCookingLoadouts;
        [SerializeField] private int[] _questLoadout;
        [SerializeField] private int[] _cookingLoadout;
        [SerializeField] private bool _manageConsumables;
        [SerializeField] private bool _autoBuyConsumables;
        [SerializeField] private bool _doMove69;
        [SerializeField] private int[] _mergeBlacklist;

        private readonly string _savePath;
        private bool _disableSave;

        public SavedSettings(string dir)
        {
            if (dir != null)
                _savePath = Path.Combine(dir, "settings.json");
        }

        internal void SaveSettings()
        {
            if (_savePath == null) return;
            if (_disableSave) return;
            Main.Log("Saving Settings");
            Main.IgnoreNextChange = true;
            var serialized = JsonUtility.ToJson(this, true);
            using (var writer = new StreamWriter(_savePath))
            {
                writer.Write(serialized);
                writer.Flush();
            }
            Main.UpdateForm(this);
        }

        internal void SetSaveDisabled(bool disabled)
        {
            _disableSave = disabled;
        }

        internal bool LoadSettings()
        {
            if (File.Exists(_savePath))
            {
                try
                {
                    var newSettings = JsonUtility.FromJson<SavedSettings>(File.ReadAllText(_savePath));
                    MassUpdate(newSettings);
                    Main.Log("Loaded Settings");
                    Main.Log(JsonUtility.ToJson(this, true));
                    return true;
                }
                catch (Exception e)
                {
                    Main.Log(e.Message);
                    Main.Log(e.StackTrace);
                    return false;
                }
            }

            return false;
        }

        internal void MassUpdate(SavedSettings other)
        {
            _priorityBoosts = other.PriorityBoosts;
            _boostBlacklist = other.BoostBlacklist;

            _yggdrasilLoadout = other.YggdrasilLoadout;
            _swapYggdrasilLoadouts = other.SwapYggdrasilLoadouts;

            _swapTitanLoadouts = other.SwapTitanLoadouts;
            _titanLoadout = other.TitanLoadout;

            _manageDiggers = other.ManageDiggers;
            _manageYggdrasil = other.ManageYggdrasil;
            _manageEnergy = other.ManageEnergy;
            _manageMagic = other.ManageMagic;
            _manageInventory = other.ManageInventory;
            _manageGear = other.ManageGear;
            _manageWandoos = other.ManageWandoos;
            _autoConvertBoosts = other.AutoConvertBoosts;

            _snipeZone = other.SnipeZone;
            _fastCombat = other.FastCombat;
            _precastBuffs = other.PrecastBuffs;

            _autoFight = other.AutoFight;

            _autoQuest = other.AutoQuest;
            _autoQuestItopod = other.AutoQuestITOPOD;
            _allowMajorQuests = other.AllowMajorQuests;

            _goldDropLoadout = other.GoldDropLoadout;

            _autoMoneyPit = other.AutoMoneyPit;
            _autoSpin = other.AutoSpin;
            _moneyPitLoadout = other.MoneyPitLoadout;
            _moneyPitThreshold = other.MoneyPitThreshold;

            _autoRebirth = other.AutoRebirth;
            _manageWandoos = other.ManageWandoos;
            _doGoldSwap = other.DoGoldSwap;

            _combatMode = other.CombatMode;
            _recoverHealth = other.RecoverHealth;
            _snipeBossOnly = other.SnipeBossOnly;
            _allowZoneFallback = other.AllowZoneFallback;
            _abandonMinors = other.AbandonMinors;
            _minorAbandonThreshold = other.MinorAbandonThreshold;
            _questCombatMode = other.QuestCombatMode;
            _questFastCombat = other.QuestFastCombat;
            _autoSpellSwap = other.AutoSpellSwap;
            _counterfeitThreshold = other.CounterfeitThreshold;
            _spaghettiThreshold = other.SpaghettiThreshold;
            _castBloodSpells = other.CastBloodSpells;
            _ironPillThreshold = other.IronPillThreshold;
            _bloodMacGuffinAThreshold = other.BloodMacGuffinAThreshold;
            _bloodMacGuffinBThreshold = other.BloodMacGuffinBThreshold;
            _autoBuyEm = other.AutoBuyEM;
            _autoBuyAdventure = other.AutoBuyAdventure;
            _bloodNumberThreshold = other.BloodNumberThreshold;
            _quickDiggers = other.QuickDiggers;
            _quickLoadout = other.QuickLoadout;
            _combatEnabled = other.CombatEnabled;
            _globalEnabled = other.GlobalEnabled;
            _useButterMajor = other.UseButterMajor;
            _useButterMinor = other.UseButterMinor;
            _manualMinors = other.ManualMinors;
            _manageR3 = other.ManageR3;
            _activateFruits = other.ActivateFruits;
            _wishPriorities = other.WishPriorities;
            _wishBlacklist = other.WishBlacklist;
            _wishSortPriorities = other.WishSortPriorities;
            _wishSortOrder = other.WishSortOrder;
            _beastMode = other.BeastMode;
            _cubePriority = other.CubePriority;
            _manageNguDiff = other.ManageNGUDiff;
            _allocationFile = other.AllocationFile;
            _manageGoldLoadouts = other.ManageGoldLoadouts;

            bool[] tempTitanGoldTargets = new bool[other.TitanGoldTargets.Length];
            Array.Copy(other.TitanGoldTargets, tempTitanGoldTargets, other.TitanGoldTargets.Length);
            Array.Resize(ref tempTitanGoldTargets, ZoneHelpers.TitanCount());

            _titanGoldTargets = tempTitanGoldTargets;

            bool[] tempTitanSwapTargets = new bool[other.TitanSwapTargets.Length];
            Array.Copy(other.TitanSwapTargets, tempTitanSwapTargets, other.TitanSwapTargets.Length);
            Array.Resize(ref tempTitanSwapTargets, ZoneHelpers.TitanCount());
            _titanSwapTargets = tempTitanSwapTargets;

            bool[] tempTitanMoneyDone = new bool[other.TitanMoneyDone.Length];
            Array.Copy(other.TitanMoneyDone, tempTitanMoneyDone, other.TitanMoneyDone.Length);
            Array.Resize(ref tempTitanMoneyDone, ZoneHelpers.TitanCount());
            _titanMoneyDone = tempTitanMoneyDone;

            _resnipeTime = other.ResnipeTime;
            _goldCBlockMode = other.GoldCBlockMode;
            _debugAllocation = other.DebugAllocation;
            _optimizeItopodFloor = other.OptimizeITOPODFloor;
            _adventureTargetItopod = other.AdventureTargetITOPOD;
            _itopodBeastMode = other.ITOPODBeastMode;
            _itopodCombatMode = other.ITOPODCombatMode;
            _itopodFastCombat = other.ITOPODFastCombat;
            _itopodPrecastBuffs = other.ITOPODPrecastBuffs;
            _itopodRecoverHp = other.ITOPODRecoverHP;
            _disableOverlay = other.DisableOverlay;
            _upgradeDiggers = other._upgradeDiggers;
            _yggSwapThreshold = other.YggSwapThreshold;
            _moreBlockParry = other.MoreBlockParry;
            _specialBoostBlacklist = other.SpecialBoostBlacklist;
            _blacklistedBosses = other.BlacklistedBosses;
            _manageMayo = other._manageMayo;
            _trashCards = other._trashCards;
            _cardsTrashQuality = other._cardsTrashQuality;
            _autoCastCards = other._autoCastCards;
            _autoCastCardType = other._autoCastCardType;
            _trashAdventureCards = other._trashAdventureCards;
            _trashCardCost = other._trashCardCost;
            _dontCastCardType = other._dontCastCardType;
            _cardSortOrder = other._cardSortOrder;
            _cardSortEnabled = other._cardSortEnabled;
            _trashChunkers = other._trashChunkers;
            _hackAdvance = other.HackAdvance;
            _manageCooking = other._manageCooking;
            _manageQuestLoadouts = other._manageQuestLoadouts;
            _manageCookingLoadouts = other._manageCookingLoadouts;
            _questLoadout = other._questLoadout;
            _cookingLoadout = other._cookingLoadout;
            _manageConsumables = other._manageConsumables;
            _autoBuyConsumables = other._autoBuyConsumables;
            _doMove69 = other._doMove69;
            _mergeBlacklist = other._mergeBlacklist;
        }

        public int SnipeZone
        {
            get => _snipeZone;
            set
            {
                if (value == _snipeZone) return;
                _snipeZone = value;
                SaveSettings();
            }
        }

        public bool PrecastBuffs
        {
            get => _precastBuffs;
            set
            {
                if (value == _precastBuffs) return;
                _precastBuffs = value; SaveSettings();
            }
        }

        public bool SwapTitanLoadouts
        {
            get => _swapTitanLoadouts;
            set
            {
                if (value == _swapTitanLoadouts) return;
                _swapTitanLoadouts = value;
                SaveSettings();
            }
        }

        public bool SwapYggdrasilLoadouts
        {
            get => _swapYggdrasilLoadouts;
            set
            {
                if (value == _swapYggdrasilLoadouts) return;
                _swapYggdrasilLoadouts = value; SaveSettings();
            }
        }

        public int[] PriorityBoosts
        {
            get => _priorityBoosts;
            set
            {
                _priorityBoosts = value;
                SaveSettings();
            }
        }

        public bool ManageEnergy
        {
            get => _manageEnergy;
            set
            {
                if (value == _manageEnergy) return; _manageEnergy = value; SaveSettings();
            }
        }

        public bool ManageMagic
        {
            get => _manageMagic;
            set
            {
                if (value == _manageMagic) return; _manageMagic = value; SaveSettings();
            }
        }

        public bool FastCombat
        {
            get => _fastCombat;
            set
            {
                if (value == _fastCombat) return;
                _fastCombat = value; SaveSettings();
            }
        }

        public bool ManageGear
        {
            get => _manageGear;
            set
            {
                if (value == _manageGear) return; _manageGear = value; SaveSettings();
            }
        }

        public int[] TitanLoadout
        {
            get => _titanLoadout;
            set
            {
                _titanLoadout = value;
                SaveSettings();
            }
        }

        public int[] YggdrasilLoadout
        {
            get => _yggdrasilLoadout;
            set
            {
                _yggdrasilLoadout = value;
                SaveSettings();
            }
        }

        public bool ManageYggdrasil
        {
            get => _manageYggdrasil;
            set
            {
                if (value == _manageYggdrasil) return;
                _manageYggdrasil = value; SaveSettings();
            }
        }

        public bool ManageDiggers
        {
            get => _manageDiggers;
            set
            {
                if (value == _manageDiggers) return;
                _manageDiggers = value; SaveSettings();
            }
        }

        public bool UpgradeDiggers
        {
            get => _upgradeDiggers;
            set
            {
                if (value == _upgradeDiggers) return;
                _upgradeDiggers = value; SaveSettings();
            }
        }

        public bool ManageInventory
        {
            get => _manageInventory;
            set
            {
                if (value == _manageInventory) return;
                _manageInventory = value; SaveSettings();
            }
        }

        public bool AutoFight
        {
            get => _autoFight;
            set
            {
                if (value == _autoFight) return;
                _autoFight = value; SaveSettings();
            }
        }

        public bool AutoQuest
        {
            get => _autoQuest;
            set
            {
                if (value == _autoQuest) return;
                _autoQuest = value;
                SaveSettings();
            }
        }

        public bool AllowMajorQuests
        {
            get => _allowMajorQuests;
            set
            {
                if (value == _allowMajorQuests) return;
                _allowMajorQuests = value;
                SaveSettings();
            }
        }

        public bool AutoConvertBoosts
        {
            get => _autoConvertBoosts;
            set
            {
                if (value == _autoConvertBoosts) return;
                _autoConvertBoosts = value;
                SaveSettings();
            }
        }

        public bool AutoQuestITOPOD
        {
            get => _autoQuestItopod;
            set
            {
                if (value == _autoQuestItopod) return;
                _autoQuestItopod = value;
                SaveSettings();
            }
        }

        public int[] GoldDropLoadout
        {
            get => _goldDropLoadout;
            set
            {
                _goldDropLoadout = value;
                SaveSettings();
            }
        }

        public int[] MoneyPitLoadout
        {
            get => _moneyPitLoadout;
            set
            {
                _moneyPitLoadout = value;
                SaveSettings();
            }
        }

        public bool AutoMoneyPit
        {
            get => _autoMoneyPit;
            set
            {
                if (value == _autoMoneyPit) return;
                _autoMoneyPit = value;
                SaveSettings();
            }
        }

        public bool AutoSpin
        {
            get => _autoSpin;
            set
            {
                if (value == _autoSpin) return;
                _autoSpin = value;
                SaveSettings();
            }
        }

        public bool AutoRebirth
        {
            get => _autoRebirth;
            set
            {
                if (value == _autoRebirth) return;
                _autoRebirth = value;
                SaveSettings();
            }
        }

        public bool ManageWandoos
        {
            get => _manageWandoos;
            set
            {
                if (value == _manageWandoos) return;
                _manageWandoos = value;
                SaveSettings();
            }
        }

        public double MoneyPitThreshold
        {
            get => _moneyPitThreshold;
            set
            {
                _moneyPitThreshold = value;
                SaveSettings();
            }
        }

        public bool DoGoldSwap
        {
            get => _doGoldSwap;
            set
            {
                if (value == _doGoldSwap) return;
                _doGoldSwap = value;
                SaveSettings();
            }
        }

        public bool SnipeBossOnly
        {
            get => _snipeBossOnly;
            set
            {
                if (value == _snipeBossOnly) return;
                _snipeBossOnly = value;
                SaveSettings();
            }
        }

        public int[] BoostBlacklist
        {
            get => _boostBlacklist;
            set
            {
                _boostBlacklist = value;
                SaveSettings();
            }
        }

        public bool RecoverHealth
        {
            get => _recoverHealth;
            set
            {
                if (value == _recoverHealth) return;
                _recoverHealth = value;
                SaveSettings();
            }
        }

        public int CombatMode
        {
            get => _combatMode;
            set
            {
                if (value == _combatMode) return;
                _combatMode = value;
                SaveSettings();
            }
        }

        public bool AllowZoneFallback
        {
            get => _allowZoneFallback;
            set
            {
                if (value == _allowZoneFallback) return;
                _allowZoneFallback = value;
                SaveSettings();
            }
        }

        public bool AbandonMinors
        {
            get => _abandonMinors;
            set
            {
                if (value == _abandonMinors) return;
                _abandonMinors = value;
                SaveSettings();
            }
        }

        public int MinorAbandonThreshold
        {
            get => _minorAbandonThreshold;
            set
            {
                if (value == _minorAbandonThreshold) return;
                _minorAbandonThreshold = value;
                SaveSettings();
            }
        }

        public int QuestCombatMode
        {
            get => _questCombatMode;
            set
            {
                if (value == _questCombatMode) return;
                _questCombatMode = value;
                SaveSettings();
            }
        }

        public bool QuestFastCombat
        {
            get => _questFastCombat;
            set
            {
                if (value == _questFastCombat) return;
                _questFastCombat = value;
                SaveSettings();
            }
        }

        public bool AutoSpellSwap
        {
            get => _autoSpellSwap;
            set
            {
                if (value == _autoSpellSwap) return;
                _autoSpellSwap = value;
                SaveSettings();
            }
        }

        public int SpaghettiThreshold
        {
            get => _spaghettiThreshold;
            set
            {
                if (value == _spaghettiThreshold) return;
                _spaghettiThreshold = value;
                SaveSettings();
            }
        }

        public bool CastBloodSpells
        {
            get => _castBloodSpells;
            set
            {
                if (value == _castBloodSpells) return;
                _castBloodSpells = value;
                SaveSettings();
            }
        }

        public int IronPillThreshold
        {
            get => _ironPillThreshold;
            set
            {
                if (value == _ironPillThreshold) return;
                _ironPillThreshold = value;
                SaveSettings();
            }
        }

        public int BloodMacGuffinAThreshold
        {
            get => _bloodMacGuffinAThreshold;
            set
            {
                if (value == _bloodMacGuffinAThreshold) return;
                _bloodMacGuffinAThreshold = value;
                SaveSettings();
            }
        }

        public int BloodMacGuffinBThreshold
        {
            get => _bloodMacGuffinBThreshold;
            set
            {
                if (value == _bloodMacGuffinBThreshold) return;
                _bloodMacGuffinBThreshold = value;
                SaveSettings();
            }
        }

        public int CounterfeitThreshold
        {
            get => _counterfeitThreshold;
            set
            {
                if (value == _counterfeitThreshold) return;
                _counterfeitThreshold = value;
                SaveSettings();
            }
        }

        public bool AutoBuyEM
        {
            get => _autoBuyEm;
            set
            {
                if (value == _autoBuyEm) return;
                _autoBuyEm = value;
                SaveSettings();
            }
        }

        public bool AutoBuyAdventure
        {
            get => _autoBuyAdventure;
            set
            {
                if (value == _autoBuyAdventure) return;
                _autoBuyAdventure = value;
                SaveSettings();
            }
        }

        public double BloodNumberThreshold
        {
            get => _bloodNumberThreshold;
            set
            {
                if (value == _bloodNumberThreshold) return;
                _bloodNumberThreshold = value;
                SaveSettings();
            }
        }

        public int[] QuickLoadout
        {
            get => _quickLoadout;
            set => _quickLoadout = value;
        }

        public int[] QuickDiggers
        {
            get => _quickDiggers;
            set => _quickDiggers = value;
        }

        public bool GlobalEnabled
        {
            get => _globalEnabled;
            set
            {
                if (value == _globalEnabled) return;
                _globalEnabled = value;
                SaveSettings();
            }
        }

        public bool CombatEnabled
        {
            get => _combatEnabled;
            set
            {
                if (value == _combatEnabled) return;
                _combatEnabled = value;
                SaveSettings();
            }
        }

        public bool ManualMinors
        {
            get => _manualMinors;
            set
            {
                if (value == _manualMinors) return;
                _manualMinors = value;
                SaveSettings();
            }
        }

        public bool UseButterMajor
        {
            get => _useButterMajor;
            set
            {
                if (value == _useButterMajor) return;
                _useButterMajor = value;
                SaveSettings();
            }
        }

        public bool ManageR3
        {
            get => _manageR3;
            set
            {
                if (value == _manageR3) return;
                _manageR3 = value;
                SaveSettings();
            }
        }

        public bool UseButterMinor
        {
            get => _useButterMinor;
            set
            {
                if (value == _useButterMinor) return;
                _useButterMinor = value;
                SaveSettings();
            }
        }

        public bool ActivateFruits
        {
            get => _activateFruits;
            set
            {
                if (value == _activateFruits) return;
                _activateFruits = value;
                SaveSettings();
            }
        }
        public int[] WishPriorities
        {
            get => _wishPriorities;
            set
            {
                if (value == _wishPriorities) return;
                _wishPriorities = value;
                SaveSettings();
            }
        }
        public int[] WishBlacklist
        {
            get => _wishBlacklist;
            set
            {
                if (value == _wishBlacklist) return;
                _wishBlacklist = value;
                SaveSettings();
            }
        }

        public bool WishSortPriorities
        {
            get => _wishSortPriorities;
            set
            {
                if (value == _wishSortPriorities) return;
                _wishSortPriorities = value;
                SaveSettings();
            }
        }

        public bool WishSortOrder
        {
            get => _wishSortOrder;
            set
            {
                if (value == _wishSortOrder) return;
                _wishSortOrder = value;
                SaveSettings();
            }
        }

        public bool BeastMode
        {
            get => _beastMode;
            set
            {
                if (value == _beastMode) return;
                _beastMode = value;
                SaveSettings();
            }
        }

        public int CubePriority
        {
            get => _cubePriority;
            set
            {
                if (value == _cubePriority) return;
                _cubePriority = value;
                SaveSettings();
            }
        }

        public bool ManageNGUDiff
        {
            get => _manageNguDiff;
            set
            {
                if (value == _manageNguDiff) return;
                _manageNguDiff = value;
                SaveSettings();
            }
        }

        public string AllocationFile
        {
            get => _allocationFile;
            set
            {
                if (value == _allocationFile) return;
                _allocationFile = value;
                SaveSettings();
            }
        }

        public bool ManageGoldLoadouts
        {
            get => _manageGoldLoadouts;
            set
            {
                if (value == _manageGoldLoadouts) return;
                _manageGoldLoadouts = value;
                SaveSettings();
            }
        }

        public int ResnipeTime
        {
            get => _resnipeTime;
            set
            {
                if (value == _resnipeTime) return;
                _resnipeTime = value;
                SaveSettings();
            }
        }

        public bool[] TitanMoneyDone
        {
            get => _titanMoneyDone;
            set
            {
                if (_titanMoneyDone != null && _titanMoneyDone.SequenceEqual(value)) return;
                _titanMoneyDone = value;
                SaveSettings();
            }
        }

        public bool[] TitanGoldTargets
        {
            get => _titanGoldTargets;
            set
            {
                if (_titanGoldTargets != null && _titanGoldTargets.SequenceEqual(value)) return;
                _titanGoldTargets = value;
                SaveSettings();
            }
        }

        public bool[] TitanSwapTargets
        {
            get => _titanSwapTargets;
            set
            {
                if (_titanSwapTargets != null && _titanSwapTargets.SequenceEqual(value)) return;
                _titanSwapTargets = value;
                SaveSettings();
            }
        }

        public bool GoldCBlockMode
        {
            get => _goldCBlockMode;
            set
            {
                if (value == _goldCBlockMode) return;
                _goldCBlockMode = value;
                SaveSettings();
            }
        }

        public bool DebugAllocation
        {
            get => _debugAllocation;
            set => _debugAllocation = value;
        }

        public bool OptimizeITOPODFloor
        {
            get => _optimizeItopodFloor;
            set
            {
                if (value == _optimizeItopodFloor) return;
                _optimizeItopodFloor = value;
                SaveSettings();
            }
        }

        public bool AdventureTargetITOPOD
        {
            get => _adventureTargetItopod;
            set
            {
                if (value == _adventureTargetItopod) return;
                _adventureTargetItopod = value;
                SaveSettings();
            }
        }

        public int ITOPODCombatMode
        {
            get => _itopodCombatMode;
            set
            {
                if (value == _itopodCombatMode) return;
                _itopodCombatMode = value;
                SaveSettings();
            }
        }

        public bool ITOPODBeastMode
        {
            get => _itopodBeastMode;
            set
            {
                if (value == _itopodBeastMode) return;
                _itopodBeastMode = value;
                SaveSettings();
            }
        }

        public bool ITOPODFastCombat
        {
            get => _itopodFastCombat;
            set
            {
                if (value == _itopodFastCombat) return;
                _itopodFastCombat = value;
                SaveSettings();
            }
        }

        public bool ITOPODRecoverHP
        {
            get => _itopodRecoverHp;
            set
            {
                if (value == _itopodRecoverHp) return;
                _itopodRecoverHp = value;
                SaveSettings();
            }
        }

        public bool ITOPODPrecastBuffs
        {
            get => _itopodPrecastBuffs;
            set
            {
                if (value == _itopodPrecastBuffs) return;
                _itopodPrecastBuffs = value;
                SaveSettings();
            }
        }

        public bool DisableOverlay
        {
            get => _disableOverlay;
            set
            {
                if (value == _disableOverlay) return;
                _disableOverlay = value;
                SaveSettings();
            }
        }

        public int YggSwapThreshold
        {
            get => _yggSwapThreshold;
            set
            {
                if (value == _yggSwapThreshold) return;
                _yggSwapThreshold = value;
                SaveSettings();
            }
        }

        public bool MoreBlockParry
        {
            get => _moreBlockParry;
            set
            {
                if (value == _moreBlockParry) return;
                _moreBlockParry = value;
                SaveSettings();
            }
        }

        public int[] SpecialBoostBlacklist
        {
            get => _specialBoostBlacklist;
            set
            {
                if (_specialBoostBlacklist != null && _specialBoostBlacklist.SequenceEqual(value)) return;
                _specialBoostBlacklist = value;
                SaveSettings();
            }
        }

        public int[] BlacklistedBosses
        {
            get => _blacklistedBosses;
            set
            {
                if (_blacklistedBosses != null && _blacklistedBosses.SequenceEqual(value)) return;
                _blacklistedBosses = value;
                SaveSettings();
            }
        }
        public bool ManageMayo
        {
            get => _manageMayo;
            set
            {
                if (value == _manageMayo) return;
                _manageMayo = value;
                SaveSettings();
            }
        }
        public bool TrashCards
        {
            get => _trashCards;
            set
            {
                if (value == _trashCards) return;
                _trashCards = value;
                SaveSettings();
            }
        }
        public int CardsTrashQuality
        {
            get => _cardsTrashQuality;
            set
            {
                if (value == _cardsTrashQuality) return;
                _cardsTrashQuality = value;
                SaveSettings();
            }
        }
        public bool AutoCastCards
        {
            get => _autoCastCards;
            set
            {
                if (value == _autoCastCards) return;
                _autoCastCards = value;
                SaveSettings();
            }
        }

        public int AutoCastCardType
        {
            get => _autoCastCardType;
            set 
            {
                if (value == _autoCastCardType) return;
                _autoCastCardType = value;
                SaveSettings();
            }
        }

        public bool TrashAdventureCards
        {
            get => _trashAdventureCards;
            set
            {
                if (value == _trashAdventureCards) return;
                _trashAdventureCards = value;
                SaveSettings();
            }
        }

        public int TrashCardCost
        {
            get => _trashCardCost;
            set
            {
                if (value == _trashCardCost) return;
                _trashCardCost = value;
                SaveSettings();
            }
        }
        public string[] DontCastCardType
        {
            get => _dontCastCardType;
            set
            {
                if (value == _dontCastCardType) return;
                _dontCastCardType = value;
                SaveSettings();
            }
        }
        public string[] CardSortOrder
        {
            get => _cardSortOrder;
            set
            {
                if (value == _cardSortOrder) return;
                _cardSortOrder = value;
                SaveSettings();
            }
        }
        public bool CardSortEnabled
        {
            get => _cardSortEnabled;
            set
            {
                if (value == _cardSortEnabled) return;
                _cardSortEnabled = value;
                SaveSettings();
            }
        }
        public bool TrashChunkers
        {
            get => _trashChunkers;
            set
            {
                if (value == _trashChunkers) return;
                _trashChunkers = value;
                SaveSettings();
            }
        }
        internal bool NeedsGoldSwap()
        {
            for (var i = 0; i < TitanSwapTargets.Length; i++)
            {
                if (!TitanSwapTargets[i])
                    continue;

                if (TitanSwapTargets[i] && !TitanMoneyDone[i])
                    return true;
            }

            return false;
        }

        public bool HackAdvance
        {
            get => _hackAdvance;
            set
            {
                if (value == _hackAdvance) return;
                _hackAdvance = value;
                SaveSettings();
            }
        }

        public bool ManageCooking
        {
            get => _manageCooking;
            set
            {
                if (value == _manageCooking) return;
                _manageCooking = value;
                SaveSettings();
            }
        }

        public bool ManageQuestLoadouts
        {
            get => _manageQuestLoadouts;
            set
            {
                if (value == _manageQuestLoadouts) return;
                _manageQuestLoadouts = value;
                SaveSettings();
            }
        }

        public bool ManageCookingLoadouts
        {
            get => _manageCookingLoadouts;
            set
            {
                if (value == _manageCookingLoadouts) return;
                _manageCookingLoadouts = value;
                SaveSettings();
            }
        }

        public int[] QuestLoadout
        {
            get => _questLoadout;
            set
            {
                _questLoadout = value;
                SaveSettings();
            }
        }

        public int[] CookingLoadout
        {
            get => _cookingLoadout;
            set
            {
                _cookingLoadout = value;
                SaveSettings();
            }
        }

        public bool AutoBuyConsumables
        {
            get => _autoBuyConsumables;
            set
            {
                if (value == _autoBuyConsumables) return;
                _autoBuyConsumables = value;
                SaveSettings();
            }
        }

        public bool ManageConsumables
        {
            get => _manageConsumables;
            set
            {
                if (value == _manageConsumables) return;
                _manageConsumables = value;
                SaveSettings();
            }
        }

        public bool DoMove69
        {
            get => _doMove69;
            set
            {
                if (value == _doMove69) return;
                _doMove69 = value;
                SaveSettings();
            }
        }

        public int[] MergeBlacklist
        {
            get => _mergeBlacklist;
            set
            {
                _mergeBlacklist = value;
                SaveSettings();
            }
        }
    }
}
