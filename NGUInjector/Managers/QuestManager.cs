using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NGUInjector.Main;

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
                    if (_character.beastQuest.idleMode) return;
                    
                    Log("Turning on idle mode for minor quest.");
                    _character.beastQuest.idleMode = true;
                    _character.beastQuestController.updateButtons();
                    _character.beastQuestController.updateButtonText();
                    
                    //If we're not in ITOPOD, move there if its set
                    if (_character.adventureController.zone >= 1000 || !Settings.AutoQuestITOPOD) return;
                    Log($"Moving to ITOPOD to idle.");
                    _character.adventureController.zoneSelector.changeZone(1000);
                }
                else
                {
                    //We're in a major quest. Move to the zone
                    var zone = _character.beastQuestController.curQuestZone();
                    if (_character.adventureController.zone == zone) return;
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
