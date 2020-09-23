using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NGUInjector
{
    abstract class AllocationProfile
    {
        protected Character _character;
        protected EnergyInputController _energyController;
        protected StreamWriter _outputWriter;

        protected AllocationProfile()
        {
            _character = Main.Character;
            _energyController = _character.energyMagicPanel;
            _outputWriter = Main.OutputWriter;
        }

        public abstract void AllocateEnergy();
        public abstract void AllocateMagic();
        public abstract void EquipGear();
        public abstract void EquipDiggers();
    }
}
