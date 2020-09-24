using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static NGUInjector.Main;

namespace NGUInjector
{
    internal class CombatManager
    {
        private readonly Character _character;
        private readonly PlayerController _pc;
        private int _snipeStage = 0;

        public CombatManager()
        {
            _character = Main.Character;
            _pc = Main.PlayerController;
        }

        bool HasFullHP()
        {
            return (Math.Abs(_character.totalAdvHP() - _character.adventure.curHP) < 5);
        }

        float GetHPPercentage()
        {
            return _character.adventure.curHP / _character.totalAdvHP();
        }

        bool ChargeActive()
        {
            return _pc.chargeFactor > 1.05;
        }

        void PrepCombat(bool needsBuff)
        {
            if (!needsBuff && HasFullHP())
            {
                LogCombat("Skipping buffs, moving to stage 3");
                _snipeStage = 3;
                return;
            }

            if (_character.adventure.autoattacking && _character.training.attackTraining[1] != 0)
            {
                LogCombat("Toggling off autoattack");
                _character.adventureController.idleAttackMove.setToggle();
            }

            if (_character.adventureController.zone != -1)
            {
                LogCombat("Moving to safe zone to prep and wait for full HP");
                _character.adventureController.zoneSelector.changeZone(-1);
            }

            _snipeStage = 1;
            LogCombat("In safe zone, moving to stage 1");
        }

        void DoBuffs()
        {
            if (!Settings.PrecastBuffs && HasFullHP())
            {
                LogCombat("Buffs disabled, moving to stage 3");
                _snipeStage = 3;
                return;
            }
            if (ChargeActive())
            {
                LogCombat("Charge active, moving to stage 2");
                _snipeStage = 2;
                return;
            }

            if (_character.adventureController.chargeMove.button.IsInteractable())
            {
                LogCombat("Using charge, moving to stage 2");
                _character.adventureController.chargeMove.doMove();
                _snipeStage = 2;
            }
        }

        void WaitForChargeCooldown()
        {
            if (!HasFullHP())
            {
                return;
            }

            if (!Settings.PrecastBuffs)
            {
                LogCombat("Buffs disabled, moving to stage 3");
                _snipeStage = 3;
                return;
            }

            if (_character.adventureController.chargeMove.button.IsInteractable() && ChargeActive() && (Math.Abs(_character.totalAdvHP() - _character.adventure.curHP) < 5))
            {
                LogCombat("Charge ready, moving to stage 4");
                _snipeStage = 3;
            }
        }

        void FindBoss(int zone)
        {
            if (_character.adventureController.chargeMove.button.IsInteractable() && !ChargeActive())
            {
                _character.adventureController.chargeMove.doMove();
            }

            if (_character.adventureController.zone == -1)
            {
                _character.adventureController.zoneSelector.changeZone(zone);
            }

            if (_character.adventureController.currentEnemy == null)
                return;

            var ec = _character.adventureController.currentEnemy.enemyType;
            if (ec != enemyType.boss && !ec.ToString().Contains("bigBoss"))
            {
                _character.adventureController.zoneSelector.changeZone(-1);
                return;
            }

            _snipeStage = 4;
        }

        private bool UltimateBuffActive()
        {
            return _pc.ultimateBuffTime > 0 && _pc.ultimateBuffTime < _character.ultimateBuffDuration();
        }

        private bool DefenseBuffActive()
        {
            return _pc.defenseBuffTime > 0 && _pc.defenseBuffTime < _character.defenseBuffDuration();
        }

        private void DoCombat()
        {
            if (!_pc.moveCheck())
                return;

            if (_character.training.attackTraining[1] == 0)
            {
                if (!_character.adventure.autoattacking)
                    _character.adventureController.idleAttackMove.setToggle();
                return;
            }
            
            if (!Settings.FastCombat)
            {
                if (CombatBuffs())
                    return;
            }
            

            CombatAttacks();
        }

        private float GetUACooldown()
        {
            var ua = _character.adventureController.ultimateAttackMove;
            var type = ua.GetType().GetField("ultimateAttackTimer",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var val = type?.GetValue(ua);
            if (val == null)
            {
                return 0;
            }

            return (float) val / _character.ultimateAttackCooldown();
        }

        private bool CombatBuffs()
        {
            var ac = _character.adventureController;
            var ai = ac.currentEnemy.AI;
            var eai = ac.enemyAI;

            if (ai == AI.charger && eai.GetPV<int>("chargeCooldown") >= 3)
            {
                if (ac.parryMove.button.IsInteractable() && !_pc.isBlocking)
                {
                    ac.parryMove.doMove();
                    return true;
                }

                if (ac.blockMove.button.IsInteractable() && !_pc.isParrying)
                {
                    ac.blockMove.doMove();
                    return true;
                }
            }

            if (ai == AI.rapid && eai.GetPV<int>("rapidEffect") >= 6)
            {
                if (ac.blockMove.button.IsInteractable())
                {
                    ac.blockMove.doMove();
                    return true;
                }
            }

            if (ac.healMove.button.IsInteractable() && GetHPPercentage() < .8)
            {
                ac.healMove.doMove();
                return true;
            }

            if (ac.hyperRegenMove.button.IsInteractable() && GetHPPercentage() < .6)
            {
                ac.hyperRegenMove.doMove();
                return true;
            }

            if (ac.currentEnemy.curHP / ac.currentEnemy.maxHP < .2)
            {
                return false;
            }

            if (ac.ultimateBuffMove.button.IsInteractable() && !DefenseBuffActive())
            {
                ac.ultimateBuffMove.doMove();
                return true;
            }

            if (ac.offenseBuffMove.button.IsInteractable() && UltimateBuffActive())
            {
                ac.offenseBuffMove.doMove();
                return true;
            }

            if (ai != AI.charger && ai != AI.rapid && ac.blockMove.button.IsInteractable() && !UltimateBuffActive() &&
                !DefenseBuffActive())
            {
                ac.blockMove.doMove();
                return true;
            }

            if (ai != AI.charger && ac.parryMove.button.IsInteractable() && !ChargeActive())
            {
                ac.parryMove.doMove();
                return true;
            }

            if (ac.defenseBuffMove.button.IsInteractable() && !UltimateBuffActive() && !_pc.isBlocking)
            {
                ac.defenseBuffMove.doMove();
                return true;
            }

            if (_pc.isBlocking || _pc.isParrying)
            {
                return false;
            }

            if (ac.chargeMove.button.IsInteractable())
            {
                if (ac.ultimateAttackMove.button.IsInteractable())
                {
                    ac.chargeMove.doMove();
                    return true;
                }

                if (GetUACooldown() > .5 && ac.chargeMove.button.IsInteractable() && ac.pierceMove.button.IsInteractable())
                {
                    ac.chargeMove.doMove();
                    return true;
                }
            }

            return false;
        }

        private bool ParalyzeBoss()
        {
            var ac = _character.adventureController;
            var ai = ac.currentEnemy.AI;
            var eai = ac.enemyAI;

            if (!ac.paralyzeMove.button.IsInteractable())
                return false;

            if (GetHPPercentage() < .2)
                return false;

            if (UltimateBuffActive())
                return false;

            if (ai == AI.charger && eai.GetPV<int>("chargeCooldown") == 0)
            {
                ac.paralyzeMove.doMove();
                return true;
            }


            if (ai == AI.rapid && eai.GetPV<int>("rapidEffect") < 5)
            {
                ac.paralyzeMove.doMove();
                return true;
            }

            if (ai != AI.rapid && ai != AI.charger)
            {
                ac.paralyzeMove.doMove();
                return true;
            }

            return false;
        }

        private void CombatAttacks()
        {
            var ac = _character.adventureController;

            if (!Settings.FastCombat)
            {
                if (ParalyzeBoss())
                {
                    return;
                }
            }
            
            if (ChargeActive() && ac.ultimateAttackMove.button.IsInteractable())
            {
                ac.ultimateAttackMove.doMove();
                return;
            }

            if (ac.pierceMove.button.IsInteractable())
            {
                ac.pierceMove.doMove();
                return;
            }

            if (ac.pierceMove.button.IsInteractable())
            {
                ac.pierceMove.doMove();
                return;
            }

            if (ac.strongAttackMove.button.IsInteractable())
            {
                ac.strongAttackMove.doMove();
                return;
            }

            if (ac.regularAttackMove.button.IsInteractable())
            {
                ac.regularAttackMove.doMove();
                return;
            }
        }

        internal void SnipeZone()
        {
            var zone = Settings.SnipeZone;
            var needsBuff = Settings.PrecastBuffs;
            try
            {
                //Stage 0: Go to safe zone
                if (_snipeStage == 0)
                {
                    PrepCombat(needsBuff);
                }
                //Stage 1: Cast charge once
                else if (_snipeStage == 1)
                {
                    DoBuffs();
                }
                //Stage 2: Wait for charge to be ready
                else if (_snipeStage == 2)
                {
                    WaitForChargeCooldown();
                }
                //Stage 3: Look for a boss
                else if (_snipeStage == 3)
                {
                    FindBoss(zone);
                }
                //Stage 4: Fight Boss
                else if (_snipeStage == 4)
                {
                    var ac = _character.adventureController;

                    if (ac.currentEnemy == null || ac.zone == -1)
                    {
                        LogCombat(
                            "Character or Enemy Defeated, or back in safe zone, resetting snipeStage");
                        _snipeStage = 0;
                        return;
                    }

                    DoCombat();
                }
            }
            catch (Exception e)
            {
                LogCombat(e.Message);
                LogCombat(e.StackTrace);
            }
            
        }
    }
}
