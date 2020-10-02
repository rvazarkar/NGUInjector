using System.IO;

namespace NGUInjector.AllocationProfiles
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
        public abstract void AllocateR3();
        public abstract void EquipGear();
        public abstract void EquipDiggers();
    }
}
