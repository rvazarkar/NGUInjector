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
            if (_character.beastQuest.curDrops >= _character.beastQuest.targetDrops - 2)
            {
                if (!_character.beastQuest.usedButter)
                {
                    if (_character.beastQuest.reducedRewards && Settings.UseButterMinor)
                    {
                        Log("Buttering Minor Quest");
                        _character.beastQuestController.tryUseButter();
                    }

                    if (!_character.beastQuest.reducedRewards && Settings.UseButterMajor)
                    {
                        Log("Buttering Major Quest");
                        _character.beastQuestController.tryUseButter();
                    }
                }
            }

            if (_character.beastQuestController.readyToHandIn())
            {
                Log("Turning in quest");
                _character.beastQuestController.completeQuest();
            }
        }

        internal int IsQuesting()
        {
            if (!Settings.AutoQuest)
                return -1;

            if (!_character.beastQuest.inQuest)
                return -1;

            var questZone = _character.beastQuestController.curQuestZone();

            if (!CombatManager.IsZoneUnlocked(questZone))
                return -1;

            if (_character.beastQuest.reducedRewards)
            {
                if (Settings.ManualMinors)
                {
                    return questZone;
                }

                return -1;
            }

            return _character.beastQuestController.curQuestZone();
        }

        private void SetIdleMode(bool idle)
        {
            _character.beastQuest.idleMode = idle;
            _character.beastQuestController.updateButtons();
            _character.beastQuestController.updateButtonText();
        }

        internal void ManageQuests()
        {
            //First logic: not in a quest
            if (!_character.beastQuest.inQuest)
            {
                //If we're allowing major quests and 
                if (Settings.AllowMajorQuests && _character.beastQuest.curBankedQuests > 0)
                {
                    _character.settings.useMajorQuests = true;
                    SetIdleMode(false);
                    _character.beastQuestController.startQuest();
                }
                else
                {
                    SetIdleMode(!Settings.ManualMinors);
                    _character.beastQuestController.startQuest();
                }

                return;
            }

            //Second logic, we're in a quest
            if (_character.beastQuest.reducedRewards)
            {
                if (Settings.AllowMajorQuests && Settings.AbandonMinors && _character.beastQuest.curBankedQuests > 0)
                {
                    var progress = (_character.beastQuest.curDrops / (float) _character.beastQuest.targetDrops) * 100;
                    if ( progress <= Settings.MinorAbandonThreshold)
                    {
                        //If all this is true get rid of this minor quest and pick up a new one.
                        _character.settings.useMajorQuests = true;
                        _character.beastQuestController.skipQuest();
                        SetIdleMode(false);
                        _character.beastQuestController.startQuest();
                        //Combat logic will pick up from here
                        return;
                    }
                }

                SetIdleMode(!Settings.ManualMinors);
            }
            else
            {
                SetIdleMode(false);
            }
        }
    }
}
