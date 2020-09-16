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
        [SerializeField] private int _snipeZone;
        [SerializeField] private bool _precastBuffs;
        [SerializeField] private bool _swapTitanLoadouts;
        [SerializeField] private bool _swapYggdrasilLoadouts;
        [SerializeField] private int[] _boostIds;
        [SerializeField] private bool _manageEnergy;
        [SerializeField] private bool _manageMagic;
        [SerializeField] private bool _fastCombat;
        [SerializeField] private bool _manageGear;
        [SerializeField] private bool _manageDiggers;
        [SerializeField] private bool _manageYggdrasil;
        [SerializeField] private int[] _titanLoadout;
        [SerializeField] private int[] _yggdrasilLoadout;
        [SerializeField] private int[] _boostBlacklist;
        [SerializeField] private bool _manageInventory;
        [SerializeField] private bool _autoFight;


        private readonly string _savePath;
        
        public SavedSettings(string dir)
        {
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
            _boostIds = other.BoostIDs;
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

            _snipeZone = other.SnipeZone;
            _fastCombat = other.FastCombat;
            _precastBuffs = other.PrecastBuffs;
            
            _autoFight = other.AutoFight;
        }

        public override string ToString()
        {
            return $"{nameof(_highestAkZone)}: {_highestAkZone}, {nameof(_snipeZone)}: {_snipeZone}, {nameof(_precastBuffs)}: {_precastBuffs}, {nameof(_swapTitanLoadouts)}: {_swapTitanLoadouts}, {nameof(_swapYggdrasilLoadouts)}: {_swapYggdrasilLoadouts}, {nameof(_boostIds)}: {_boostIds}, {nameof(_manageEnergy)}: {_manageEnergy}, {nameof(_manageMagic)}: {_manageMagic}, {nameof(_fastCombat)}: {_fastCombat}, {nameof(_manageGear)}: {_manageGear}, {nameof(_titanLoadout)}: {_titanLoadout}, {nameof(_yggdrasilLoadout)}: {_yggdrasilLoadout}, {nameof(_boostBlacklist)}: {_boostBlacklist}";
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

        public int[] BoostIDs
        {
            get => _boostIds;
            set => _boostIds = value;
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
            set => _titanLoadout = value;
        }

        public int[] YggdrasilLoadout
        {
            get => _yggdrasilLoadout;
            set => _yggdrasilLoadout = value;
        }

        public int[] BoostBlacklist
        {
            get => _boostBlacklist;
            set => _boostBlacklist = value;
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
    }
}
