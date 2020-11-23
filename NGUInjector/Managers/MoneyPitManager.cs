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
            if (Main.Character.realGold < 1e5) return;

            if (Main.Settings.MoneyPitLoadout.Length > 0)
            {
                if (!LoadoutManager.TryMoneyPitSwap()) return;
            }
            if (Main.Character.realGold >= 1e50 && Main.Settings.ManageMagic && Main.Character.wishes.wishes[4].level > 0)
            {
                Main.Character.removeMostMagic();
                for (var i = Main.Character.bloodMagic.ritual.Count - 1; i >= 0; i--)
                {
                    Main.Character.bloodMagicController.bloodMagics[i].cap();
                }
                DiggerManager.SaveDiggers();
                DiggerManager.EquipDiggers(new[] {10});
                DoMoneyPit();
                DiggerManager.RestoreDiggers();
            }
            else
            {
                DoMoneyPit();
            }
            
            if (Main.Settings.MoneyPitLoadout.Length > 0)
            {
                LoadoutManager.RestoreGear();
                LoadoutManager.ReleaseLock();
            }
        }

        private static void DoMoneyPit()
        {
            var controller = Main.Character.pitController;
            typeof(PitController).GetMethod("engage", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(controller, null);

            Main.LogPitSpin($"Money Pit Reward: {controller.pitText.text}");
        }

        internal static void DoDailySpin()
        {
            if (Main.Character.daily.spinTime.totalseconds < Main.Character.dailyController.targetSpinTime()) return;

            Main.Character.dailyController.startNoBullshitSpin();
            var result = Main.Character.dailyController.outcomeText.text;
            Main.LogPitSpin($"Daily Spin Reward: {result}");
        }
    }
}
