using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NGUInjector.Managers
{
    internal static class MoneyPitManager
    {
        internal static void CheckMoneyPit()
        {
            if (Main.Character.pit.pitTime.totalseconds < Main.Character.pitController.currentPitTime()) return;
            if (Main.Character.realGold < Main.Settings.MoneyPitThreshold) return;
            if (Main.Character.realGold < 1e15) return;

            if (Main.Settings.MoneyPitLoadout.Length > 0)
            {
                if (!LoadoutManager.TryMoneyPitSwap()) return;
            }
            var controller = Main.Character.pitController;
            typeof(PitController).GetMethod("engage", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(controller, null);

            Main.LogLoot($"Money Pit Reward: {controller.pitText.text}");

            if (Main.Settings.MoneyPitLoadout.Length > 0)
            {
                LoadoutManager.RestoreGear();
                LoadoutManager.ReleaseLock();
            }
        }

        internal static void DoDailySpin()
        {
            if (Main.Character.daily.spinTime.totalseconds < Main.Character.dailyController.targetSpinTime()) return;

            Main.Character.dailyController.startNoBullshitSpin();
            var result = Main.Character.dailyController.outcomeText.text;
            Main.LogLoot($"Daily Spin Reward: {result}");
        }
    }
}
