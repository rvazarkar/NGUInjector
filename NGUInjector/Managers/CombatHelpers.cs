using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
