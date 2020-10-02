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
                if (!_character.beastQuest.usedButter)
                {
                    if (_character.beastQuest.reducedRewards && Settings.UseButterMinor)
                    {
                        _character.beastQuestController.tryUseButter();
                    }

                    if (!_character.beastQuest.reducedRewards && Settings.UseButterMajor)
                    {
                        _character.beastQuestController.tryUseButter();
                    }
                }
                
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
                    if (_character.beastQuest.curDrops / _character.beastQuest.targetDrops * 100 <=
                        Settings.MinorAbandonThreshold)
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

        }

        internal void ManageQuestsOld()
        {
            //We're in a quest already.
            if (_character.beastQuest.inQuest)
            {
                // Its a minor quest
                if (_character.beastQuest.reducedRewards)
                {
                    if (Settings.ManualMinors)
                    {
                        if (_character.beastQuest.idleMode)
                        {
                            _character.beastQuest.idleMode = false;
                            _character.beastQuestController.updateButtons();
                            _character.beastQuestController.updateButtonText();
                        }
                    }
                    else
                    {
                        if (!_character.beastQuest.idleMode)
                        {
                            _character.beastQuest.idleMode = true;
                            _character.beastQuestController.updateButtons();
                            _character.beastQuestController.updateButtonText();
                        }
                    }

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

                }

                //We have nothing else to do here
                return;
            }

            //We're not in a quest, so we need to accept a new one
            if (Settings.AllowMajorQuests && _character.beastQuest.curBankedQuests > 0)
            {
                //Turn off idle mode
                _character.beastQuest.idleMode = false;
                _character.beastQuestController.updateButtons();
                _character.beastQuestController.updateButtonText();
                //We're allowed to accept major quests, so toggle this to true, and then lets accept one.
                _character.settings.useMajorQuests = true;
                _character.beastQuestController.updateButtons();
                _character.beastQuestController.updateButtonText();
                _character.beastQuestController.startQuest();
            }
            else
            {
                if (Settings.ManualMinors)
                {
                    _character.beastQuest.idleMode = false;
                    _character.beastQuestController.updateButtons();
                    _character.beastQuestController.updateButtonText();
                }
                else
                {
                    _character.beastQuest.idleMode = true;
                    _character.beastQuestController.updateButtons();
                    _character.beastQuestController.updateButtonText();
                }
                _character.settings.useMajorQuests = false;
                _character.beastQuestController.startQuest();
            }
        }
    }
}
