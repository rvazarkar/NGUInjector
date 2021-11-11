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
                //If we're allowing major quests and we have a quest available
                if (Settings.AllowMajorQuests && _character.beastQuest.curBankedQuests > 0)
                {
                    _character.settings.useMajorQuests = true;
                    SetIdleMode(false);
                }
                else
                {
                    _character.settings.useMajorQuests = false;
                    SetIdleMode(!Settings.ManualMinors);
                }


                Log("Starting Quest");

                _character.beastQuestController.startQuest();

                return;
            }

            //Second logic, we're in a quest
            if (_character.beastQuest.reducedRewards)
            {
                if (Settings.AllowMajorQuests && Settings.AbandonMinors && _character.beastQuest.curBankedQuests > 0)
                {
                    var progress = (_character.beastQuest.curDrops / (float)_character.beastQuest.targetDrops) * 100;
                    if (progress <= Settings.MinorAbandonThreshold)
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
                else
                {
                    _character.settings.useMajorQuests = false;
                }

                SetIdleMode(!Settings.ManualMinors);

                if (Settings.ManualMinors && Settings.SwapQuestLoadout && Settings.QuestLoadout.Length > 0)
                {
                    // Swap gear for minor quests if manual minors and swap gear for quests are both true
                    LoadoutManager.TryQuestSwap();
                }
                else if (LoadoutManager.CurrentLock == LockType.Quest)
                {
                    LoadoutManager.RestoreGear();
                    LoadoutManager.ReleaseLock();
                }
            }
            else
            {

                if (Settings.SwapQuestLoadout && Settings.QuestLoadout.Length > 0)
                {
                    LoadoutManager.TryQuestSwap();
                }
                SetIdleMode(false);
            }
        }
    }
}
