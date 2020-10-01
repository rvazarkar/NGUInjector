using static NGUInjector.Main;

namespace NGUInjector.Managers
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

        internal int IsQuesting()
        {
            if (!Settings.AutoQuest)
                return -1;

            if (!_character.beastQuest.inQuest)
                return -1;

            if (_character.beastQuest.reducedRewards)
                return -1;

            if (_character.beastQuestController.curQuestZone() > _character.adventureController.zoneDropdown.options.Count - 2) 
                return -1;

            return _character.beastQuestController.curQuestZone();
        }

        internal void ManageQuests()
        {
            //We're in a quest already.
            if (_character.beastQuest.inQuest)
            {
                // Its a minor quest
                if (_character.beastQuest.reducedRewards)
                {
                    //Check if we want to abandon minor quests in favor of major quests
                    if (Settings.AllowMajorQuests && Settings.AbandonMinors & _character.beastQuest.curBankedQuests > 0)
                    {
                        //Check if we're under the threshold for abandoning
                        if ((_character.beastQuest.curDrops / _character.beastQuest.targetDrops) * 100 <
                            Settings.MinorAbandonThreshold)
                        {
                            //If all this is true get rid of this minor quest and pick up a new one.
                            _character.beastQuestController.skipQuest();
                            _character.settings.useMajorQuests = true;
                            _character.beastQuestController.updateButtons();
                            _character.beastQuestController.updateButtonText();
                            _character.beastQuestController.startQuest();

                            //Combat logic will pick up from here
                            return;
                        }
                    }

                    if (!_character.beastQuest.idleMode)
                    {
                        _character.beastQuest.idleMode = true;
                        _character.beastQuestController.updateButtons();
                        _character.beastQuestController.updateButtonText();
                    }
                }

                //We have nothing else to do here
                return;
            }
            
            //We're not in a quest, so we need to accept a new one
            if (Settings.AllowMajorQuests && _character.beastQuest.curBankedQuests > 0)
            {
                //We're allowed to accept major quests, so toggle this to true, and then lets accept one.
                _character.settings.useMajorQuests = true;
                _character.beastQuestController.updateButtons();
                _character.beastQuestController.updateButtonText();
                _character.beastQuestController.startQuest();
            }
            else
            {
                _character.settings.useMajorQuests = false;
                _character.beastQuestController.startQuest();
            }
        }
    }
}
