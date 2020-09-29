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
            if (Main.Character.adventureController.chargeMove.button.interactable)
            {
                Main.Character.adventureController.chargeMove.doMove();
                return true;
            }

            return false;
        }

        internal static bool ChargeReady()
        {
            return Main.Character.adventureController.chargeMove.button.interactable;
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
    }
}
