using System;
using System.Diagnostics;
using System.Reflection;
using static NGUInjector.Main;
using static NGUInjector.Managers.CombatHelpers;

namespace NGUInjector.Managers
{
    internal class CombatManager
    {
        private readonly Character _character;
        private readonly PlayerController _pc;
        private bool isFighting = false;

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

        private void DoCombat(bool fastCombat)
        {
            if (!_pc.moveCheck())
                return;

            if (Main.PlayerController.moveTimer > 0)
                return;

            if (!fastCombat)
            {
                if (CombatBuffs())
                    return;
            }

            CombatAttacks(fastCombat);
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

            if (ac.currentEnemy.curHP / ac.currentEnemy.maxHP < .2)
            {
                return false;
            }

            if (GetHPPercentage() < .5)
            {
                if (CastHeal())
                {
                    return true;
                }
            }

            if (GetHPPercentage() < .5 && !HealReady())
            {
                if (CastHyperRegen())
                {
                    return true;
                }
            }

            if (CastMegaBuff())
            {
                return true;
            }

            if (!DefenseBuffActive())
            {
                if (CastUltimateBuff())
                {
                    return true;
                }
            }

            if (UltimateBuffActive())
            {
                if (CastOffensiveBuff())
                    return true;
            }

            if (GetHPPercentage() < .75 && !UltimateBuffActive() && !BlockActive())
            {
                if (CastDefensiveBuff())
                    return true;
            }

            if (ai != AI.charger && ai != AI.rapid && !UltimateBuffActive() && !DefenseBuffActive())
            {
                if (!BlockActive())
                {
                    if (CastParry())
                        return true;
                }

                if (!ParryActive())
                {
                    if (CastBlock())
                        return true;
                }
            }

            if (_pc.isBlocking || _pc.isParrying)
            {
                return false;
            }

            if (CastParalyze(ai, eai))
                return true;


            if (ChargeReady())
            {
                if (UltimateAttackReady())
                {
                    if (CastCharge())
                        return true;
                }

                if (GetUltimateAttackCooldown() > .45 && PierceReady())
                {
                    if (CastCharge())
                        return true;
                }
            }

            return false;
        }

        //private bool ParalyzeBoss()
        //{
        //    var ac = _character.adventureController;
        //    var ai = ac.currentEnemy.AI;
        //    var eai = ac.enemyAI;

        //    if (!ac.paralyzeMove.button.IsInteractable())
        //        return false;

        //    if (GetHPPercentage() < .2)
        //        return false;

        //    if (UltimateBuffActive())
        //        return false;

        //    if (ai == AI.charger && eai.GetPV<int>("chargeCooldown") == 0)
        //    {
        //        ac.paralyzeMove.doMove();
        //        return true;
        //    }

        //    if (ai == AI.rapid && eai.GetPV<int>("rapidEffect") < 5)
        //    {
        //        ac.paralyzeMove.doMove();
        //        return true;
        //    }

        //    if (ai != AI.rapid && ai != AI.charger)
        //    {
        //        ac.paralyzeMove.doMove();
        //        return true;
        //    }

        //    return false;
        //}

        private void CombatAttacks(bool fastCombat)
        {
            var ac = _character.adventureController;

            if (ac.ultimateAttackMove.button.IsInteractable())
            {
                if (fastCombat)
                {
                    ac.ultimateAttackMove.doMove();
                }

                if (ChargeActive())
                {
                    ac.ultimateAttackMove.doMove();
                }

                if (GetChargeCooldown() > .45)
                {
                    ac.ultimateAttackMove.doMove();
                }
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

        internal static bool IsZoneUnlocked(int zone)
        {
            return zone <= Main.Character.adventureController.zoneDropdown.options.Count - 2;
        }

        internal void MoveToZone(int zone)
        {
            _character.adventureController.zoneSelector.changeZone(zone);
        }

        internal void IdleZone(int zone, bool bossOnly, bool recoverHealth)
        {
            //Enable idle attack if its not on
            if (!_character.adventure.autoattacking)
            {
                _character.adventureController.idleAttackMove.setToggle();
                return;
            }

            if (_character.adventure.beastModeOn && !Settings.BeastMode && _character.adventureController.beastModeMove.button.interactable)
            {
                _character.adventureController.beastModeMove.doMove();
                return;
            }

            if (!_character.adventure.beastModeOn && Settings.BeastMode &&
                _character.adventureController.beastModeMove.button.interactable)
            {
                _character.adventureController.beastModeMove.doMove();
                return;
            }

            if (_character.adventure.zone == -1 && !HasFullHP() && recoverHealth)
                return;

            //Check if we're in the right zone, if not move there
            if (_character.adventure.zone != zone && _character.adventure.zone != -1)
            {
                MoveToZone(-1);
            }

            //Wait for an enemy to spawn
            if (_character.adventureController.currentEnemy == null)
                return;
            
            //If we only want boss enemies
            if (bossOnly)
            {
                //Check the type of the enemy
                var ec = _character.adventureController.currentEnemy.enemyType;
                //If its not a boss, move back to safe zone. Next loop will put us back in the right zone.
                if (ec != enemyType.boss && !ec.ToString().Contains("bigBoss"))
                {
                    MoveToZone(-1);
                }
            }
        }

        internal void ManualZone(int zone, bool bossOnly, bool recoverHealth, bool precastBuffs, bool fastCombat)
        {
            //Start by turning off auto attack if its on unless we can only idle attack
            if (!_character.adventure.autoattacking)
            {
                if (_character.training.attackTraining[1] == 0)
                {
                    _character.adventureController.idleAttackMove.setToggle();
                    return;
                }
            }
            else
            {
                if (_character.training.attackTraining[1] > 0)
                {
                    _character.adventureController.idleAttackMove.setToggle();
                }
            }

            if (_character.adventure.beastModeOn && !Settings.BeastMode && _character.adventureController.beastModeMove.button.interactable)
            {
                _character.adventureController.beastModeMove.doMove();
                return;
            }

            if (!_character.adventure.beastModeOn && Settings.BeastMode &&
                _character.adventureController.beastModeMove.button.interactable)
            {
                _character.adventureController.beastModeMove.doMove();
                return;
            }

            //Move back to safe zone if we're in the wrong zone
            if (_character.adventure.zone != zone && _character.adventure.zone != -1)
            {
                MoveToZone(-1);
            }

            //If precast buffs is true and we have no enemy and charge isn't active, go back to safe zone
            if (precastBuffs && !ChargeActive() && _character.adventureController.currentEnemy == null)
            {
                MoveToZone(-1);
            }

            //If we're in safe zone, recover health if needed. Also precast buffs
            if (_character.adventure.zone == -1)
            {
                if (precastBuffs)
                {
                    if (ChargeUnlocked() && !ChargeActive())
                    {
                        if (CastCharge()) return;
                    }

                    if (ParryUnlocked() && !ParryActive())
                    {
                        if (CastParry()) return;
                    }

                    //Wait for Charge to be ready again, as well as other buffs
                    if (ChargeUnlocked() && !ChargeReady()) return;
                    if (ParryUnlocked() && !ParryReady()) return;
                    if (UltimateBuffUnlocked() && !UltimateBuffReady()) return;
                    if (MegaBuffUnlocked() && !MegaBuffReady()) return;
                    if (DefensiveBuffUnlocked() && !DefensiveBuffReady()) return;
                }

                if (recoverHealth && !HasFullHP())
                {
                    if (ChargeUnlocked() && !ChargeActive())
                    {
                        if (CastCharge()) return;
                    }

                    if (ParryUnlocked() && !ParryActive())
                    {
                        if (CastParry()) return;
                    }
                    return;
                }
            }
            
            //Move to the zone
            if (_character.adventure.zone != zone)
            {
                MoveToZone(zone);
                return;
            }

            //Wait for an enemy to spawn
            if (_character.adventureController.currentEnemy == null)
            {
                if (!precastBuffs && bossOnly)
                {
                    if (!ChargeActive())
                    {
                        if (CastCharge())
                        {
                            return;
                        }
                    }

                    if (!ParryActive())
                    {
                        if (CastParry())
                        {
                            return;
                        }
                    }

                    if (GetHPPercentage() < .75)
                    {
                        if (CastHeal())
                            return;
                    }
                }

                if (fastCombat)
                {
                    if (GetHPPercentage() < .75)
                    {
                        if (CastHeal())
                            return;
                    }
                }

                if (isFighting)
                {
                    isFighting = false;
                    if (precastBuffs)
                    {
                        MoveToZone(-1);
                        return;
                    }

                    if (LoadoutManager.CurrentLock == LockType.Gold)
                    {
                        Log("Gold Loadout kill done. Turning off setting and swapping gear");
                        Settings.NextGoldSwap = false;
                        settingsForm.UpdateGoldLoadout(Settings.NextGoldSwap);
                        LoadoutManager.RestoreGear();
                        LoadoutManager.ReleaseLock();
                        MoveToZone(-1);
                    }
                }
                return;
            }

            //We have an enemy. Lets check if we're in bossOnly mode
            if (bossOnly && zone < 1000)
            {
                var ec = _character.adventureController.currentEnemy.enemyType;
                if (ec != enemyType.boss && !ec.ToString().Contains("bigBoss"))
                {
                    MoveToZone(-1);
                    MoveToZone(zone);
                    return;
                }
            }

            isFighting = true;
            //We have an enemy and we're ready to fight. Run through our combat routine
            if (_character.training.attackTraining[1] > 0)
                DoCombat(fastCombat);
        }
    }
}
