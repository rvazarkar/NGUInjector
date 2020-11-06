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
        private int lastCardSpawnTime;
        private Cards _cards;
        private CardsController _cardsController;
        public CardManager()
        {

            try
            {
                _character = Main.Character;
                _cards = _character.cards;
                _cardsController = _character.cardsController;
                _maxManas = _character.cardsController.maxManaGenSize();
                lastCardSpawnTime = CardSpawnTimeToInt(_cardsController);
            }
            catch (Exception e)
            {
                Main.Log(e.Message);
                Main.Log(e.StackTrace);
            }
        }

        public void CheckManas()
        {
            try
            {
                List<Mana> manas = _character.cards.manas;
                int lowestCount = int.MaxValue;

                foreach (Mana mana in manas)
                {
                    lowestCount = Math.Min(lowestCount, mana.amount);
                }

                for (int i = 0; i < manas.Count; i++)
                {
                    if (manas[i].amount > lowestCount && manas[i].running && manas[i].amount - lowestCount > 1 && manas[i].progress > 0.1f ||
                        manas[i].amount <= lowestCount && !manas[i].running && _character.cardsController.curManaToggleCount() < _maxManas)
                    {
                        _character.cardsController.toggleManaGen(i);
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
                    int currentSpawnTime = CardSpawnTimeToInt(_cardsController);

                    //if (currentSpawnTime > lastCardSpawnTime)
                    //{
                    //lastCardSpawnTime = currentSpawnTime;

                    if (_cards.cards.Count > 0)
                    {
                        int id = 0;
                        while (id < _cards.cards.Count)
                        {
                            if (CheckAndTrashCard(id))
                            {
                                continue;
                            }
                            id++;
                        }
                    }
                    //}
                }
            }
            catch (Exception e)
            {
                Main.Log(e.Message);
                Main.Log(e.StackTrace);
            }
        }
        private bool CheckAndTrashCard(int id)
        {
            if ((int)_cards.cards[id].cardRarity <= Main.Settings.CardsTrashQuality)
            {
                _cardsController.trashCard(id);
                return true;
            }
            return false;
        }

        private static int CardSpawnTimeToInt(CardsController cc)
        {
            string[] spawnStrings = cc.timeToCardSpawn().Split(':');
            return Convert.ToInt32(spawnStrings[0]) * 60 + Convert.ToInt32(spawnStrings[1]);
        }
    }
}