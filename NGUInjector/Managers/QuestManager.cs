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

        internal bool IsQuesting()
        {
            if (!_character.beastQuest.inQuest)
                return false;

            if (_character.beastQuest.reducedRewards)
                return false;

            if (_character.beastQuestController.curQuestZone() > _character.adventureController.zoneDropdown.options.Count - 2) 
                return false;

            return true;
        }

        internal void ManageQuests()
        {
            if (!Settings.AutoQuest)
                return;

            //We're in a quest, so lets manage stuff
            if (_character.beastQuest.inQuest)
            {
                //We're in a minor quest
                if (_character.beastQuest.reducedRewards)
                {
                    //If idle mode is off, toggle it on
                    if (!_character.beastQuest.idleMode)
                    {
                        Log("Turning on idle mode for minor quest.");
                        _character.beastQuest.idleMode = true;
                        _character.beastQuestController.updateButtons();
                        _character.beastQuestController.updateButtonText();
                    }
                }
                else
                {
                    //We're in a major quest. Move to the zone
                    var zone = _character.beastQuestController.curQuestZone();
                    if (_character.adventureController.zone == zone) return;
                    if (zone > _character.adventureController.zoneDropdown.options.Count - 2) return;
                    Log($"Moving to zone {zone} for quest.");
                    _character.adventureController.zoneSelector.changeZone(zone);
                    _character.beastQuestController.updateButtons();
                    _character.beastQuestController.updateButtonText();
                }

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
                var zone = _character.beastQuestController.curQuestZone();
                if (_character.adventureController.zone == zone) return;
                Log($"Moving to zone {zone} for quest.");
                _character.adventureController.zoneSelector.changeZone(zone);
            }
            else
            {
                _character.settings.useMajorQuests = false;
                _character.beastQuestController.startQuest();
            }
        }
    }
}
