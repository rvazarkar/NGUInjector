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
                Mana lowestProgress = manas[0];

                //Main.Log(manas[0].progress.ToString() + " " + manas[2].progress.ToString());
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
                    //Main.Log(lowestProgress.progress.ToString() + " " + manas[2].progress.ToString());
                    float progressPerSec = _cardsController.manaGenProgressPerTick() * 50;
                    Mana mana = manas[i];

                    if (mana.running)
                    {
                        if (mana.amount - lowestCount == 1 && Math.Abs(mana.progress / progressPerSec - (1f - lowestProgress.progress) / progressPerSec) <= 10) continue;
                        else if (mana.amount == lowestCount && Math.Abs((mana.progress - lowestProgress.progress) / progressPerSec) <= 10) continue;
                        else if (mana.amount == lowestCount && mana.progress == lowestProgress.progress) continue;
                    }
                    else
                    {
                        //Main.Log("");
                        //Main.Log("id: " + i + " check: " + (mana.amount == lowestCount && Math.Abs((mana.progress - lowestProgress.progress) / progressPerSec) > 10));
                        //Main.Log("Mana Progress: mana.progress: " + mana.progress + " lowest progress: " + lowestProgress.progress);
                        //Main.Log("Calc: " + (Math.Abs(mana.progress - lowestProgress.progress) / progressPerSec));
                        if (mana.amount == lowestCount && Math.Abs(mana.progress - lowestProgress.progress) / progressPerSec > 10) continue;
                        else if (mana.amount - lowestCount == 1 && Math.Abs((mana.progress - (1f - lowestProgress.progress)) / progressPerSec) > 10) continue;
                        else if (mana.amount - lowestCount > 1) continue;
                    }
                    _cardsController.toggleManaGen(i);
                    Main.Log("toggled " + i);
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