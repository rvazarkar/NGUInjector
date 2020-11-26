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
        public CardManager()
        {

            try
            {
                _character = Main.Character;
                _cards = _character.cards;
                _cardsController = _character.cardsController;
                _maxManas = _character.cardsController.maxManaGenSize();
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
                    float progressPerSec = _cardsController.manaGenProgressPerTick() * 50;
                    Mana mana = manas[i];
                    
                    if (mana.running)
                    {
                        if (mana.amount - lowestCount == 1 && Math.Abs((1f + mana.progress - lowestProgress.progress) / progressPerSec) <= 10) continue;
                        else if (mana.amount == lowestCount && Math.Abs((mana.progress - lowestProgress.progress) / progressPerSec) <= 10) continue;
                    }
                    else
                    {
                        if (mana.amount == lowestCount && Math.Abs((mana.progress - lowestProgress.progress) / progressPerSec) > 10) continue;
                        else if (mana.amount - lowestCount == 1 && Math.Abs((1f + mana.progress - lowestProgress.progress) / progressPerSec) > 10) continue;
                        else if (mana.amount - lowestProgress.amount > 1) continue; 
                    }
                    _cardsController.toggleManaGen(i);
                    Main.LogCard($"Toggled {_cardsController.getManaName(i)} {(manas[i].running ? "On" : "Off")}");
                    //Main.LogCard("#############################################################");
                    //Main.LogCard($"Mana Amount: {mana.amount}");
                    //Main.LogCard($"Mana Progress: {mana.progress}");
                    //Main.LogCard($"Mana Progress in Seconds: {mana.progress / progressPerSec}\n");
                    
                    //Main.LogCard($"Lowest Amount: {lowestCount}");
                    //Main.LogCard($"Lowest Remaining Progress: {lowestProgress.progress}");
                    //Main.LogCard($"Lowest Remaining Progress in Seconds: {(1f - lowestProgress.progress) / progressPerSec}\n");
                    //Main.LogCard($"Calculation {(1f - lowestProgress.progress + mana.progress) / progressPerSec}");
                    //Main.LogCard("#############################################################");
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
                    //int currentSpawnTime = CardSpawnTimeToInt(_cardsController);
                    _cards = Main.Character.cards;
                    if (_cards.cards.Count > 0)
                    {
                        int id = 0;
                        while (id < _cards.cards.Count)
                        {
                            if ((int)_cards.cards[id].cardRarity <= Main.Settings.CardsTrashQuality)
                            {
                                Card _card = _cards.cards[id];
                                Main.LogCard($"Trashed tier {_card.tier} {_card.cardRarity} {_card.bonusType} card");
                                _cardsController.trashCard(id);
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

        //private static int CardSpawnTimeToInt(CardsController cc)
        //{
        //    //Main.LogCard(cc.timeToCardSpawn());
        //    string[] spawnStrings = cc.timeToCardSpawn().Split(':', '.');
            
        //    if (spawnStrings[1].Contains("s"))
        //    {
        //        spawnStrings[1] = "0";
        //    }
        //    return Convert.ToInt32(spawnStrings[0]) * 60 + Convert.ToInt32(spawnStrings[1]);
        //}

        //public bool CastCards()
        //{
        //    int i = 0;
        //    int autoCastCardTypes = Main.Settings.AutoCastCardType;
        //    while (autoCastCardTypes > 0)
        //    {
        //        if((autoCastCardTypes & 1) == 1) 
        //        { 
                    
        //        }
        //    }
        //    return true;
        //}
    }
}