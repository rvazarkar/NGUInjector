using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGUInjector
{
    internal class QuestManager
    {
        private readonly Character _character;

        public QuestManager()
        {
            _character = Main.Character;
        }

        internal void CheckQuestTurnin()
        {
            if (_character.beastQuestController.readyToHandIn())
            {
                _character.beastQuestController.completeQuest();
            }
        }
    }
}
