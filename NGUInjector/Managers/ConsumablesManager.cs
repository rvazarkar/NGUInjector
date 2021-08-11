using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGUInjector.Managers
{

    class ConsumablesManager
    {
        private static readonly Character _character = Main.Character;
        private static ArbitraryController _arbitraryController = Main.ArbitraryController;
        private static string[] lastConsumables = new string[0];
        private static double lastTime = 0;

        internal static readonly Dictionary<string, int> ConsumablePrices = new Dictionary<string, int>
        {
            {"EPOT-A", 5000 },
            {"EPOT-B", 10000 },
            {"EPOT-C", 100000 },
            {"MPOT-A", 5000 },
            {"MPOT-B", 10000 },
            {"MPOT-C", 100000 },
            {"R3POT-A", 4000 },
            {"R3POT-B", 40000 },
            {"R3POT-C", 40000 },
            {"EBARBAR", 10000 },
            {"MBARBAR", 10000 },
            {"MUFFIN", 50000 },
            {"LC", 5000 },
            {"SLC", 50000 },
            {"MAYO", 40000 }
        };

        internal static void EatConsumables(string[] consumables, double time, int[] quantity)
        {
            if (Enumerable.SequenceEqual(consumables, lastConsumables) && (Math.Abs(time - lastTime) < 1))
            {
                // We already did this set of consumables, wait for next one
                return;
            }

            for (int i = 0; i < consumables.Length; i++)
            {
                string consumable = consumables[i];

                if (!IsValidConsumable(consumable))
                {
                    Main.Log($"ConsumablesManager - Invalid consumable name: {consumable}");
                    continue;
                }

                if (!HasEnoughConsumable(consumable, quantity[i]))
                {
                    if (Main.Settings.AutoBuyConsumables && HasEnoughAP(consumable, quantity[i]))
                    {
                        Main.Log($"ConsumablesManager - Not enough of consumable, buying {quantity[i]} more: {consumable}");
                        BuyConsumable(consumable, quantity[i]);
                    }
                    else
                    {
                        Main.Log($"ConsumablesManager - Not enough {consumable} owned, unable to use consumable. Buy more manually, or turn on AutoBuyConsumables in settings.json");
                        return;
                    }
                }

                
                UseConsumables(consumable, quantity[i]);
            }

            Array.Resize(ref lastConsumables, consumables.Length);
            lastConsumables = consumables;
            lastTime = time;
        }

        private static bool HasEnoughConsumable(string consumable, int quantity)
        {
            int owned = GetOwnedConsumableCount(consumable);
            Main.Log($"ConsumablesManager - Owned {consumable}s: {owned} , Needed: {quantity}");
            return owned >= quantity;
        }

        private static bool HasEnoughAP(string consumable, int quantity)
        {
            bool enough = false;
            ConsumablePrices.TryGetValue(consumable, out int price);

            if (price > 0)
            {
                enough = _character.arbitrary.curArbitraryPoints > (price * quantity);
                if (!enough)
                {
                    Main.Log($"ConsumablesManager - Not enough AP[{_character.arbitrary.curArbitraryPoints}] to buy consumable, need {price * quantity} AP for {quantity} {consumable}");
                }
            }

            return enough;
        }

        private static int GetOwnedConsumableCount(string consumable)
        {
            switch (consumable)
            {
                case "EPOT-A":
                    return _character.arbitrary.energyPotion1Count;
                case "EPOT-B":
                    return _character.arbitrary.energyPotion2Count;
                case "EPOT-C":
                    return _character.arbitrary.energyPotion3Count;
                case "MPOT-A":
                    return _character.arbitrary.magicPotion1Count;
                case "MPOT-B":
                    return _character.arbitrary.magicPotion2Count;
                case "MPOT-C":
                    return _character.arbitrary.magicPotion3Count;
                case "R3POT-A":
                    return _character.arbitrary.res3Potion1Count;
                case "R3POT-B":
                    return _character.arbitrary.res3Potion2Count;
                case "R3POT-C":
                    return _character.arbitrary.res3Potion3Count;
                case "EBARBAR":
                    return _character.arbitrary.energyBarBar1Count;
                case "MBARBAR":
                    return _character.arbitrary.magicBarBar1Count;
                case "MUFFIN":
                    return _character.arbitrary.macGuffinBooster1Count;
                case "LC":
                    return _character.arbitrary.lootCharm1Count;
                case "SLC":
                    return _character.arbitrary.lootCharm2Count;
                case "MAYO":
                    return _character.arbitrary.mayoSpeedPotCount;
                default:
                    break;
            }
            return -1;
        }

        private static void BuyConsumable(string consumable, int count)
        {
            for (int i = 0; i < count; i++)
            {
                switch (consumable)
                {
                    case "EPOT-A":
                        _arbitraryController.buyEnergyPotion1AP();
                        break;
                    case "EPOT-B":
                        _arbitraryController.buyEnergyPotion2AP();
                        break;
                    case "EPOT-C":
                        _arbitraryController.buyEnergyPotion3();
                        break;
                    case "MPOT-A":
                        _arbitraryController.buyMagicPotion1AP();
                        break;
                    case "MPOT-B":
                        _arbitraryController.buyMagicPotion2AP();
                        break;
                    case "MPOT-C":
                        _arbitraryController.buyMagicPotion3();
                        break;
                    case "R3POT-A":
                        _arbitraryController.buyRes3Potion1();
                        break;
                    case "R3POT-B":
                        _arbitraryController.buyRes3Potion2();
                        break;
                    case "R3POT-C":
                        _arbitraryController.buyRes3Potion3();
                        break;
                    case "EBARBAR":
                        _arbitraryController.buyEnergyBarBar1AP();
                        break;
                    case "MBARBAR":
                        _arbitraryController.buyMagicBarBar1AP();
                        break;
                    case "MUFFIN":
                        _arbitraryController.buyMacguffinBooster1AP();
                        break;
                    case "LC":
                        _arbitraryController.buyLootCharm1AP();
                        break;
                    case "SLC":
                        _arbitraryController.buyLootCharm2AP();
                        break;
                    case "MAYO":
                        _arbitraryController.buyMayoSpeedConsumableAP();
                        break;
                    default:
                        break;
                }

                ConsumablePrices.TryGetValue(consumable, out int price);
                _character.arbitrary.curArbitraryPoints -= price;
            }
        }

        private static void UseConsumables(string consumable, int count)
        {
            Main.Log($"ConsumablesManager - Eating {count} {consumable}");

            for (int i = 0; i < count; i++)
            {
                switch (consumable)
                {
                    case "EPOT-A":
                        _character.arbitrary.energyPotion1Time.advanceTime(60 * 60);
                        _character.arbitrary.energyPotion1Count--;
                        break;
                    case "EPOT-B":
                        if (!_character.arbitrary.energyPotion2InUse)
                        {
                            _character.arbitrary.energyPotion2InUse = true;
                            _character.arbitrary.energyPotion2Count--;
                        } 
                        else
                        {
                            Main.Log($"ConsumablesManager - Energy Potion Beta already active, not eating");
                        }
                        break;
                    case "EPOT-C":
                        _character.arbitrary.energyPotion1Time.advanceTime(60 * 60 * 24);
                        _character.arbitrary.energyPotion3Count--;
                        break;
                    case "MPOT-A":
                        _character.arbitrary.magicPotion1Time.advanceTime(60 * 60);
                        _character.arbitrary.magicPotion1Count--;
                        break;
                    case "MPOT-B":
                        if (!_character.arbitrary.magicPotion2InUse)
                        {
                            _character.arbitrary.magicPotion2InUse = true;
                            _character.arbitrary.magicPotion2Count--;
                        }
                        else
                        {
                            Main.Log($"ConsumablesManager - Magic Potion Beta already active, not eating");
                        }
                        break;
                    case "MPOT-C":
                        _character.arbitrary.magicPotion1Time.advanceTime(60 * 60 * 24);
                        _character.arbitrary.magicPotion3Count--;
                        break;
                    case "R3POT-A":
                        _character.arbitrary.res3Potion1Time.advanceTime(60 * 60);
                        _character.arbitrary.res3Potion1Count--;
                        break;
                    case "R3POT-B":
                        if (!_character.arbitrary.res3Potion2InUse)
                        {
                            _character.arbitrary.res3Potion2InUse = true;
                            _character.arbitrary.res3Potion2Count--;
                        }
                        else
                        {
                            Main.Log($"ConsumablesManager - R3 Potion Beta already active, not eating");
                        }
                        break;
                    case "R3POT-C":
                        _character.arbitrary.res3Potion1Time.advanceTime(60 * 60 * 24);
                        _character.arbitrary.res3Potion3Count--;
                        break;
                    case "EBARBAR":
                        _character.arbitrary.energyBarBar1Time.advanceTime(60 * 60);
                        _character.arbitrary.energyBarBar1Count--;
                        break;
                    case "MBARBAR":
                        _character.arbitrary.magicBarBar1Time.advanceTime(60 * 60);
                        _character.arbitrary.magicBarBar1Count--;
                        break;
                    case "MUFFIN":
                        // boolean _character.arbitrary.macGuffinBooster1InUse
                        // The boolean doesn't seem to work, so going by remaining time instead
                        if (_character.arbitrary.macGuffinBooster1Time.totalseconds < 1)
                        {
                            _character.arbitrary.macGuffinBooster1Time.advanceTime(60 * 60 * 24);
                            _character.arbitrary.macGuffinBooster1Count--;
                        } else
                        {
                            Main.Log($"ConsumablesManager - Macguffin Muffin already active, not eating");
                        }
                        break;
                    case "LC":
                        _character.arbitrary.lootcharm1Time.advanceTime(30 * 60);
                        _character.arbitrary.lootCharm1Count--;
                        break;
                    case "SLC":
                        _character.arbitrary.lootcharm1Time.advanceTime(60 * 60 * 12);
                        _character.arbitrary.lootCharm2Count--;
                        break;
                    case "MAYO":
                        _character.arbitrary.mayoSpeedPotTime.advanceTime(60 * 60 * 24);
                        _character.arbitrary.mayoSpeedPotCount--;
                        break;
                    default:
                        Main.Log($"ConsumablesManager - Unknown consumable: {consumable}");
                        break;
                }
            }
        }

        private static bool IsValidConsumable(string consumable)
        {
            return ConsumablePrices.ContainsKey(consumable);
        }

        internal static void resetLastConsumables()
        {
            ConsumablesManager.lastConsumables = new string[0];
            ConsumablesManager.lastTime = 0;
        }
    }
}