using System.IO;

namespace NGUInjector.AllocationProfiles
{
    internal abstract class AllocationProfile
    {
        protected Character _character;
        protected EnergyInputController _energyController;
        protected StreamWriter _outputWriter;
        protected ArbitraryController _arbitraryController;

        protected AllocationProfile()
        {
            _character = Main.Character;
            _energyController = _character.energyMagicPanel;
            _outputWriter = Main.OutputWriter;
            _arbitraryController = Main.ArbitraryController;
        }

        public abstract void AllocateEnergy();
        public abstract void AllocateMagic();
        public abstract void AllocateR3();
        public abstract void EquipGear();
        public abstract void EquipDiggers();
        public abstract void ConsumeConsumables();
    }
}
