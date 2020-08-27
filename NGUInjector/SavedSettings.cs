using System;
using System.Collections.Generic;
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
        [SerializeField] private int[] _boostIds;

        
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

        public int[] BoostIDs
        {
            get => _boostIds;
            set => _boostIds = value;
        }
    }
}
