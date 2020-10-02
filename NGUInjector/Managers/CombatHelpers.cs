using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NGUInjector.Managers
{
    internal static class CombatHelpers
    {
        internal static bool CastCharge()
        {
            if (Main.Character.adventureController.chargeMove.button.IsInteractable())
            {
                Main.Character.adventureController.chargeMove.doMove();
                return true;
            }

            return false;
        }

        internal static bool CastParry()
        {
            if (Main.Character.adventureController.parryMove.button.IsInteractable())
            {
                Main.Character.adventureController.parryMove.doMove();
                return true;
            }

            return false;
        }

        internal static bool ChargeReady()
        { 
            return Main.Character.adventureController.chargeMove.button.IsInteractable();
        }

        internal static bool ParryReady()
        {
            return Main.Character.adventureController.parryMove.button.IsInteractable();
        }

        internal static float GetChargeCooldown()
        {
            var ua = Main.Character.adventureController.chargeMove;
            var type = ua.GetType().GetField("chargeTimer",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var val = type?.GetValue(ua);
            if (val == null)
            {
                return 0;
            }

            return (float)val / Main.Character.chargeCooldown();
        }

        internal static bool HealReady()
        {
            return Main.Character.adventureController.healMove.button.IsInteractable();
        }

        internal static bool CastHeal()
        {
            if (Main.Character.adventureController.healMove.button.IsInteractable())
            {
                Main.Character.adventureController.healMove.doMove();
                return true;
            }

            return false;
        }

        internal static bool CastHyperRegen()
        {
            if (Main.Character.adventureController.hyperRegenMove.button.IsInteractable())
            {
                Main.Character.adventureController.hyperRegenMove.doMove();
                return true;
            }

            return false;
        }

        internal static bool ParryActive()
        {
            return Main.PlayerController.isParrying;
        }

        internal static bool ChargeActive()
        {
            return Main.PlayerController.chargeFactor > 1.05;
        }

        internal static bool UltimateBuffActive()
        {
            return Main.PlayerController.ultimateBuffTime > 0 && Main.PlayerController.ultimateBuffTime < Main.Character.ultimateBuffDuration();
        }

        internal static bool DefenseBuffActive()
        {
            return Main.PlayerController.defenseBuffTime > 0 && Main.PlayerController.defenseBuffTime < Main.Character.defenseBuffDuration();
        }

        internal static float GetUltimateAttackCooldown()
        {
            var ua = Main.Character.adventureController.ultimateAttackMove;
            var type = ua.GetType().GetField("ultimateAttackTimer",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var val = type?.GetValue(ua);
            if (val == null)
            {
                return 0;
            }

            return (float)val / Main.Character.ultimateAttackCooldown();
        }

        internal static bool CastUltimateBuff()
        {
            if (Main.Character.adventureController.ultimateBuffMove.button.IsInteractable())
            {
                Main.Character.adventureController.ultimateBuffMove.doMove();
                return true;
            }

            return false;
        }

        internal static bool CastMegaBuff()
        {
            if (Main.Character.adventureController.megaBuffMove.button.IsInteractable())
            {
                Main.Character.adventureController.megaBuffMove.doMove();
                return true;
            }

            return false;
        }

        internal static bool CastOffensiveBuff()
        {
            if (Main.Character.adventureController.offenseBuffMove.button.IsInteractable())
            {
                Main.Character.adventureController.offenseBuffMove.doMove();
                return true;
            }

            return false;
        }

        internal static bool BlockActive()
        {
            return Main.PlayerController.isBlocking;
        }

        internal static bool CastBlock()
        {
            if (Main.Character.adventureController.blockMove.button.IsInteractable())
            {
                Main.Character.adventureController.blockMove.doMove();
                return true;
            }

            return false;
        }

        internal static bool CastDefensiveBuff()
        {
            if (Main.Character.adventureController.defenseBuffMove.button.IsInteractable())
            {
                Main.Character.adventureController.defenseBuffMove.doMove();
                return true;
            }

            return false;
        }

        internal static bool CastParalyze(AI ai, EnemyAI eai)
        {
            if (!Main.Character.adventureController.paralyzeMove.button.IsInteractable())
            {
                return false;
            }

            if (ai == AI.charger && eai.GetPV<int>("chargeCooldown") == 0)
            {
                Main.Character.adventureController.paralyzeMove.doMove();
                return true;
            }

            if (ai == AI.rapid && eai.GetPV<int>("rapidEffect") < 5)
            {
                Main.Character.adventureController.paralyzeMove.doMove();
                return true;
            }

            if (ai != AI.rapid && ai != AI.charger)
            {
                Main.Character.adventureController.paralyzeMove.doMove();
                return true;
            }
            return false;
        }

        internal static bool UltimateAttackReady()
        {
            return Main.Character.adventureController.ultimateAttackMove.button.IsInteractable();
        }

        internal static bool PierceReady()
        {
            return Main.Character.adventureController.pierceMove.button.IsInteractable();
        }

        internal static bool ChargeUnlocked()
        {
            return Main.Character.training.defenseTraining[4] > 0;
        }

        internal static bool ParryUnlocked()
        {
            return Main.Character.training.attackTraining[3] > 0;
        }
    }
}
