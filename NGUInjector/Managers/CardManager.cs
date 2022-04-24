using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace NGUInjector.Managers
{
    public class CardManager
    {
        private readonly Character _character;
        private readonly int _maxManas;
        private Cards _cards;
        private CardsController _cardsController;
        private readonly IDictionary<cardBonus, float> _cardValues = new Dictionary<cardBonus, float>();

        public CardManager()
        {

            try
            {
                _character = Main.Character;
                _cards = _character.cards;
                _cardsController = _character.cardsController;
                _maxManas = _character.cardsController.maxManaGenSize();
                foreach (cardBonus bonus in Enum.GetValues(typeof(cardBonus)))
                {
                    float bonusValue = _cardsController.generateCardEffect(bonus, 6, 1, 1, false);
                    _cardValues.Add(bonus, bonusValue);
                }
            }
            catch (Exception e)
            {
                Main.Log(e.Message);
                Main.Log(e.StackTrace);
            }
        }
        int CurTogg() => _character.cardsController.curManaToggleCount();
        public void CheckManas()
        {

            try
            {
                List<Mana> manas = _character.cards.manas;
                int lowestCount = int.MaxValue;
                Mana lowestProgress = manas[0];
                bool balanceMayo = true;
                if (_cards.cards.Count > 0)
                {
                    Card _card = _cards.cards[0];
                    for (int i = 0; i < manas.Count; i++)
                    {
                        if (manas[i].amount < _card.manaCosts[i])
                        {
                            balanceMayo = false;
                            break;
                        }
                    }
                    for (int i = 0; i < manas.Count && !balanceMayo; i++)
                    {
                        if (manas[i].amount >= _card.manaCosts[i] && manas[i].running)
                        {
                            _cardsController.toggleManaGen(i);
                            i = 0;
                        }
                        if (manas[i].amount < _card.manaCosts[i] && !manas[i].running && CurTogg() < _maxManas) _cardsController.toggleManaGen(i);
                    }
                }

                if (balanceMayo)
                {
                    foreach (Mana mana in manas)
                    {

                        if (lowestCount > mana.amount)
                        {
                            lowestCount = mana.amount;
                            lowestProgress = mana;
                        }
                        else if (lowestCount == mana.amount && mana.progress < lowestProgress.progress) lowestProgress = mana;
                    }

                    for (int i = 0; i < manas.Count; i++)
                    {
                        int toggStart = CurTogg();
                        float progressPerSec = _cardsController.manaGenProgressPerTick() * 50;
                        Mana mana = manas[i];

                        if (mana.running)
                        {
                            if (mana.amount - lowestCount == 1 && Math.Abs((1f + mana.progress - lowestProgress.progress) / progressPerSec) <= 10) continue;
                            else if (mana.amount == lowestCount && Math.Abs((mana.progress - lowestProgress.progress) / progressPerSec) <= 10) continue;
                        }
                        else
                        {
                            if (CurTogg() >= _maxManas) continue;
                            else if (mana.amount == lowestCount && Math.Abs((mana.progress - lowestProgress.progress) / progressPerSec) > 10) continue;
                            else if (mana.amount - lowestCount == 1 && Math.Abs((1f + mana.progress - lowestProgress.progress) / progressPerSec) > 10) continue;
                            else if (mana.amount - lowestProgress.amount > 1) continue;
                        }
                        _cardsController.toggleManaGen(i);
                        if (toggStart < CurTogg()) i = 0;
                    }
                }
            }
            catch (Exception e)
            {
                Main.Log(e.Message);
                Main.Log(e.StackTrace);
            }
        }

        public void TrashCards()
        {
            try
            {
                if (Main.Settings.TrashCards)
                {
                    _cards = Main.Character.cards;
                    if (_cards.cards.Count > 0)
                    {
                        int id = 0;
                        while (id < _cards.cards.Count)
                        {
                            Card _card = _cards.cards[id];

                            if (_card.bonusType != cardBonus.adventureStat || (_card.bonusType == cardBonus.adventureStat && Main.Settings.TrashAdventureCards))
                            {
                                if(_card.type == cardType.end)
                                {
                                    if (Main.Character.inventory.accessories.Any(c => c.id == 492))
                                    {
                                        _cardsController.trashCard(id);
                                        Main.LogCard($"Trashed Card: Bonus Type: END, due to already having the END piece");
                                        continue;
                                    }
                                    else if (_cards.cards.Count(c => c.type == cardType.end) > 1)
                                    {
                                        _cardsController.trashCard(id);
                                        Main.LogCard($"Trashed Card: Bonus Type: END, due to already having one in the cards list");
                                        continue;
                                    }
                                }
                                else if ((int)_cards.cards[id].cardRarity <= Main.Settings.CardsTrashQuality - 1)
                                {
                                    Main.LogCard($"Trashed Card: Cost: {_card.manaCosts.Sum()} Rarity: {_card.cardRarity} Bonus Type: {_card.bonusType}, due to Quality settings");
                                    if (_card.isProtected) _card.isProtected = false;
                                    _cardsController.trashCard(id);
                                    continue;
                                }
                                else if (_cards.cards[id].manaCosts.Sum() <= Main.Settings.TrashCardCost)
                                {
                                    Main.LogCard($"Trashed Card: Cost: {_card.manaCosts.Sum()} Rarity: {_card.cardRarity} Bonus Type: {_card.bonusType}, due to Cost settings");
                                    if (_card.isProtected) _card.isProtected = false;
                                    _cardsController.trashCard(id);
                                    continue;
                                }
                                else if (Main.Settings.DontCastCardType.Contains(_card.bonusType.ToString()))
                                {
                                    if (_card.cardRarity != rarity.BigChonker || _card.cardRarity == rarity.BigChonker && Main.Settings.TrashChunkers)
                                    {
                                        if (_card.isProtected) _card.isProtected = false;
                                        Main.LogCard($"Trashed Card: Cost: {_card.manaCosts.Sum()} Rarity: {_card.cardRarity} Bonus Type: {_card.bonusType}, due to trash all settings");
                                        _cardsController.trashCard(id);
                                        continue;
                                    }
                                }
                            }
                            id++;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Main.Log(e.Message);
                Main.Log(e.StackTrace);
            }
        }
        public void CastCards()
        {
            bool castCard = true;
            while (castCard && _cards.cards.Count > 0)
            {
                Card card = _cards.cards[0];
                List<Mana> manas = _character.cards.manas;
                if (Main.Settings.TrashCards) TrashCards(); //Make sure all cards in inventory are ones that should be cast
                for (int i = 0; i < card.manaCosts.Count; i++)
                {
                    //Main.LogCard(manas[i].amount.ToString());
                    //Main.LogCard(card.manaCosts[i].ToString());
                    if (manas[i].amount < card.manaCosts[i])
                    {
                        //Main.LogCard("false");
                        castCard = false;
                        break;
                    }
                }
                //Main.LogCard(castCard.ToString());
                if (castCard)
                {
                    Main.LogCard($"Cast Card: Cost: {card.manaCosts.Sum()} Rarity: {card.cardRarity} Bonus Type: {card.bonusType}");
                    if (card.isProtected) card.isProtected = false;
                    _cardsController.tryConsumeCard(0);
                }
            }
        }

        public void SortCards()
        {
            try
            {
                _character.cards.cards.Sort(CompareCards);
                for (var i = 0; i < _character.cards.cards.Count; i++)
                {
                    _cardsController.updateDeckCard(i);
                }
            }
            catch (Exception e)
            {
                Main.Log(e.Message);
                Main.Log(e.StackTrace);
            }
        }

        private float getCardChange(Card card)
        {
            return (_cardsController.getBonus(card.bonusType) + card.effectAmount) / _cardsController.getBonus(card.bonusType);
        }

        private float getCardValue(Card card)
        {
            return (getCardChange(card) - 1) / card.manaCosts.Sum();
        }

        private float getCardNormalValue(Card card)
        {
            return getCardValue(card) / _cardValues[card.bonusType];
        }

        private int CompareByPriority(string priority, Card c1, Card c2)
        {
            if (priority.ToUpper() == "RARITY")
            {
                return c2.cardRarity.CompareTo(c1.cardRarity);
            }

            if (priority.ToUpper() == "TIER")
            {
                return c2.tier.CompareTo(c1.tier);
            }

            if (priority.ToUpper() == "COST")
            {
                return c2.manaCosts.Sum().CompareTo(c1.manaCosts.Sum());
            }

            if (priority.ToUpper().StartsWith("TYPE:"))
            {
                var bonusType = priority.Substring(5);

                if (c1.bonusType.ToString() == c2.bonusType.ToString())
                    return 0;
                else if (c2.bonusType.ToString() == bonusType)
                    return 1;
                else if (c1.bonusType.ToString() == bonusType)
                    return -1;
                else
                    return 0;
            }

            if (priority.ToUpper() == "PROTECTED")
            {
                return c2.isProtected.CompareTo(c1.isProtected);
            }

            if (priority.ToUpper() == "CHANGE")
            {
                return getCardChange(c2).CompareTo(getCardChange(c1));
            }

            if (priority.ToUpper() == "VALUE")
            {
                return getCardValue(c2).CompareTo(getCardValue(c1));
            }

            if (priority.ToUpper() == "NORMALVALUE")
            {
                if(c1.bonusType == c2.bonusType)
                {
                    return getCardValue(c2).CompareTo(getCardValue(c1));
                }
                return getCardNormalValue(c2).CompareTo(getCardNormalValue(c1));
            }

            return 0;
        }

        private int CompareCards(Card c1, Card c2)
        {
            foreach (var priority in Main.Settings.CardSortOrder)
            {
                var index = CompareByPriority(priority, c1, c2);

                if (index != 0) return index;
            }

            return 0;
        }
    }
}