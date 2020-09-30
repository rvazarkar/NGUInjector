using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NGUInjector
{
    [Serializable]
    public class SavedSettings
    {
        [SerializeField] private int _highestAkZone;
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
        [SerializeField] private int _initialGoldZone = -1;
        [SerializeField] private double _moneyPitThreshold;
        [SerializeField] private bool _nextGoldSwap;
        [SerializeField] private int _goldZone = -1;
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

        private readonly string _savePath;
        
        public SavedSettings(string dir)
        {
            if (dir != null)
                _savePath = Path.Combine(dir, "settings.json");
        }

        internal void SaveSettings()
        {
            if (_savePath == null) return;
            Main.Log("Saving Settings");
            Main.IgnoreNextChange = true;
            var serialized = JsonUtility.ToJson(this, true);
            using (var writer = new StreamWriter(_savePath))
            {
                writer.Write(serialized);
                writer.Flush();
            }
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

            _highestAkZone = other.HighestAKZone;
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
            _initialGoldZone = other.InitialGoldZone;
            _nextGoldSwap = other.NextGoldSwap;
            _goldZone = other.GoldZone;

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
        }

        public int HighestAKZone
        {
            get => _highestAkZone;
            set
            {
                _highestAkZone = value;
                SaveSettings();
            }
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

        public int InitialGoldZone
        {
            get => _initialGoldZone;
            set
            {
                if (value == _initialGoldZone) return;
                _initialGoldZone = value;
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

        public bool NextGoldSwap
        {
            get => _nextGoldSwap;
            set
            {
                if (value == _nextGoldSwap) return;
                _nextGoldSwap = value;
                SaveSettings();
            }
        }

        public int GoldZone
        {
            get => _goldZone;
            set
            {
                if (value == _goldZone) return;
                _goldZone = value;
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
    }
}
