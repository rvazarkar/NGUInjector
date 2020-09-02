using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NGUInjector
{
    [Serializable]
    public class SavedSettings : IEquatable<SavedSettings>
    {
        [SerializeField] private int _highestAkZone;
        [SerializeField] private int _snipeZone;
        [SerializeField] private bool _precastBuffs;
        [SerializeField] private bool _swapTitanLoadouts;
        [SerializeField] private bool _swapYggdrasilLoadouts;
        [SerializeField] private int[] _boostIds;
        [SerializeField]private bool _manageEnergy;
        [SerializeField]private bool _manageMagic;
        [SerializeField] private bool _fastCombat;
        [SerializeField] private bool _manageGear;
        [SerializeField] private bool _manageYggdrasil;
        [SerializeField] private int[] _titanLoadout;
        [SerializeField] private int[] _yggdrasilLoadout;
        [SerializeField] private int[] _boostBlacklist;

        public override string ToString()
        {
            return $"{nameof(_highestAkZone)}: {_highestAkZone}, {nameof(_snipeZone)}: {_snipeZone}, {nameof(_precastBuffs)}: {_precastBuffs}, {nameof(_swapTitanLoadouts)}: {_swapTitanLoadouts}, {nameof(_swapYggdrasilLoadouts)}: {_swapYggdrasilLoadouts}, {nameof(_boostIds)}: {_boostIds}, {nameof(_manageEnergy)}: {_manageEnergy}, {nameof(_manageMagic)}: {_manageMagic}, {nameof(_fastCombat)}: {_fastCombat}, {nameof(_manageGear)}: {_manageGear}, {nameof(_titanLoadout)}: {_titanLoadout}, {nameof(_yggdrasilLoadout)}: {_yggdrasilLoadout}, {nameof(_boostBlacklist)}: {_boostBlacklist}";
        }

        public int HighestAKZone
        {
            get => _highestAkZone;
            set => _highestAkZone = value;
        }

        public int SnipeZone
        {
            get => _snipeZone;
            set => _snipeZone = value;
        }

        public bool PrecastBuffs
        {
            get => _precastBuffs;
            set => _precastBuffs = value;
        }

        public bool SwapTitanLoadouts
        {
            get => _swapTitanLoadouts;
            set => _swapTitanLoadouts = value;
        }

        public bool SwapYggdrasilLoadouts
        {
            get => _swapYggdrasilLoadouts;
            set => _swapYggdrasilLoadouts = value;
        }

        public int[] BoostIDs
        {
            get => _boostIds;
            set => _boostIds = value;
        }

        public bool ManageEnergy
        {
            get => _manageEnergy;
            set => _manageEnergy = value;
        }

        public bool ManageMagic
        {
            get => _manageMagic;
            set => _manageMagic = value;
        }
        public bool FastCombat
        {
            get => _fastCombat;
            set => _fastCombat = value;
        }

        public bool ManageGear
        {
            get => _manageGear;
            set => _manageGear = value;
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
            set => _manageYggdrasil = value;
        }

        public bool Equals(SavedSettings other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _highestAkZone == other._highestAkZone && _snipeZone == other._snipeZone && _precastBuffs == other._precastBuffs && _swapTitanLoadouts == other._swapTitanLoadouts && _swapYggdrasilLoadouts == other._swapYggdrasilLoadouts && _manageEnergy == other._manageEnergy && _manageMagic == other._manageMagic && _fastCombat == other._fastCombat && _manageGear == other._manageGear && _manageYggdrasil == other._manageYggdrasil;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SavedSettings) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _highestAkZone;
                hashCode = (hashCode * 397) ^ _snipeZone;
                hashCode = (hashCode * 397) ^ _precastBuffs.GetHashCode();
                hashCode = (hashCode * 397) ^ _swapTitanLoadouts.GetHashCode();
                hashCode = (hashCode * 397) ^ _swapYggdrasilLoadouts.GetHashCode();
                hashCode = (hashCode * 397) ^ _manageEnergy.GetHashCode();
                hashCode = (hashCode * 397) ^ _manageMagic.GetHashCode();
                hashCode = (hashCode * 397) ^ _fastCombat.GetHashCode();
                hashCode = (hashCode * 397) ^ _manageGear.GetHashCode();
                hashCode = (hashCode * 397) ^ _manageYggdrasil.GetHashCode();
                return hashCode;
            }
        }
    }
}
