using System;
using static NGUInjector.Main;

namespace NGUInjector.Managers
{
	internal class CookingManager
	{
		CookingController cook;

		public CookingManager()
		{
			cook = Main.Character.cookingController;
		}

		internal void manageFood()
		{
			if (Main.Character.cooking.unlocked)
			{
				bestCooking();
			}
		}
		public void bestCooking()
		{
			if (cook.getCurScore() == cook.getOptimalScore())
			{
				if (cook.character.cooking.cookTimer >= cook.eatRate())
				{
					if (Settings.ManageCookingLoadouts && Settings.CookingLoadout.Length > 0)
                    {
						if (!LoadoutManager.TryCookingSwap())
                        {
							Log("Unable to acquire lock for gear, waiting a cycle to equip gears");
							return;
						}
                    }

					cook.consumeDish();

					if (LoadoutManager.HasCookingLock())
                    {
						LoadoutManager.RestoreGear();
						LoadoutManager.ReleaseLock();
                    }
				}
				return;
			}

			int foodA = 0;
			int foodB = 0;
			int foodC = 0;
			int foodD = 0;
			int foodE = 0;
			int foodF = 0;
			int foodG = 0;
			int foodH = 0;
			float num = 0f;
			for (int i = 0; i <= cook.maxIngredientLevel(); i++)
			{
				for (int j = 0; j <= cook.maxIngredientLevel(); j++)
				{
					float num2 = 0f;
					if (cook.ingredientUnlocked(cook.character.cooking.pair1[0]))
					{
						num2 += cook.getLocalScore(cook.character.cooking.pair1[0], i) + cook.getLocalScore(cook.character.cooking.pair1[1], i);
					}
					if (cook.ingredientUnlocked(cook.character.cooking.pair1[1]))
					{
						num2 += cook.getLocalScore(cook.character.cooking.pair1[0], j) + cook.getLocalScore(cook.character.cooking.pair1[1], j);
					}
					if (cook.ingredientUnlocked(cook.character.cooking.pair1[0]) && cook.ingredientUnlocked(cook.character.cooking.pair1[1]))
					{
						num2 += cook.getPairedScore(1, i + j);
					}
					if (num2 > num)
					{
						foodA = i;
						foodB = j;
						num = num2;
					}
				}
			}
			float num3 = 0f;
			for (int k = 0; k <= cook.maxIngredientLevel(); k++)
			{
				for (int l = 0; l <= cook.maxIngredientLevel(); l++)
				{
					float num2 = 0f;
					if (cook.ingredientUnlocked(cook.character.cooking.pair2[0]))
					{
						num2 += cook.getLocalScore(cook.character.cooking.pair2[0], k) + cook.getLocalScore(cook.character.cooking.pair2[1], k);
					}
					if (cook.ingredientUnlocked(cook.character.cooking.pair2[1]))
					{
						num2 += cook.getLocalScore(cook.character.cooking.pair2[0], l) + cook.getLocalScore(cook.character.cooking.pair2[1], l);
					}
					if (cook.ingredientUnlocked(cook.character.cooking.pair2[0]) && cook.ingredientUnlocked(cook.character.cooking.pair2[1]))
					{
						num2 += cook.getPairedScore(2, k + l);
					}
					if (num2 > num3)
					{
						foodC = k;
						foodD = l;
						num3 = num2;
					}
				}
			}
			float num4 = 0f;
			for (int m = 0; m <= cook.maxIngredientLevel(); m++)
			{
				for (int n = 0; n <= cook.maxIngredientLevel(); n++)
				{
					float num2 = 0f;
					if (cook.ingredientUnlocked(cook.character.cooking.pair3[0]))
					{
						num2 += cook.getLocalScore(cook.character.cooking.pair3[0], m) + cook.getLocalScore(cook.character.cooking.pair3[1], m);
					}
					if (cook.ingredientUnlocked(cook.character.cooking.pair3[1]))
					{
						num2 += cook.getLocalScore(cook.character.cooking.pair3[0], n) + cook.getLocalScore(cook.character.cooking.pair3[1], n);
					}
					if (cook.ingredientUnlocked(cook.character.cooking.pair3[0]) && cook.ingredientUnlocked(cook.character.cooking.pair3[1]))
					{
						num2 += cook.getPairedScore(3, m + n);
					}
					if (num2 > num4)
					{
						foodE = m;
						foodF = n;
						num4 = num2;
					}
				}
			}
			float num5 = 0f;
			for (int num6 = 0; num6 <= cook.maxIngredientLevel(); num6++)
			{
				for (int num7 = 0; num7 <= cook.maxIngredientLevel(); num7++)
				{
					float num2 = 0f;
					if (cook.ingredientUnlocked(cook.character.cooking.pair4[0]))
					{
						num2 += cook.getLocalScore(cook.character.cooking.pair4[0], num6) + cook.getLocalScore(cook.character.cooking.pair4[1], num6);
					}
					if (cook.ingredientUnlocked(cook.character.cooking.pair4[1]))
					{
						num2 += cook.getLocalScore(cook.character.cooking.pair4[0], num7) + cook.getLocalScore(cook.character.cooking.pair4[1], num7);
					}
					if (cook.ingredientUnlocked(cook.character.cooking.pair4[0]) && cook.ingredientUnlocked(cook.character.cooking.pair4[1]))
					{
						num2 += cook.getPairedScore(4, num6 + num7);
					}
					if (num2 > num5)
					{
						foodG = num6;
						foodH = num7;
						num5 = num2;
					}
				}
			}
			Main.LogAllocation($"Best Cooking:: {cook.character.cooking.pair1[0]}@{foodA} | {cook.character.cooking.pair1[1]}@{foodB} |" +
				$" {cook.character.cooking.pair2[0]}@{foodC} | {cook.character.cooking.pair2[1]}@{foodD} |" +
				$" {cook.character.cooking.pair3[0]}@{foodE} | {cook.character.cooking.pair3[1]}@{foodF} |" +
				$" {cook.character.cooking.pair4[0]}@{foodG} | {cook.character.cooking.pair4[1]}@{foodH}");
			if (cook.ingredientUnlocked(cook.character.cooking.pair1[0]))
			{
				cook.character.cooking.ingredients[cook.character.cooking.pair1[0]].curLevel = foodA;
			}
			if (cook.ingredientUnlocked(cook.character.cooking.pair1[1]))
			{
				cook.character.cooking.ingredients[cook.character.cooking.pair1[1]].curLevel = foodB;
			}
			if (cook.ingredientUnlocked(cook.character.cooking.pair2[0]))
			{
				cook.character.cooking.ingredients[cook.character.cooking.pair2[0]].curLevel = foodC;
			}
			if (cook.ingredientUnlocked(cook.character.cooking.pair2[1]))
			{
				cook.character.cooking.ingredients[cook.character.cooking.pair2[1]].curLevel = foodD;
			}
			if (cook.ingredientUnlocked(cook.character.cooking.pair3[0]))
			{
				cook.character.cooking.ingredients[cook.character.cooking.pair3[0]].curLevel = foodE;
			}
			if (cook.ingredientUnlocked(cook.character.cooking.pair3[1]))
			{
				cook.character.cooking.ingredients[cook.character.cooking.pair3[1]].curLevel = foodF;
			}
			if (cook.ingredientUnlocked(cook.character.cooking.pair4[0]))
			{
				cook.character.cooking.ingredients[cook.character.cooking.pair4[0]].curLevel = foodG;
			}
			if (cook.ingredientUnlocked(cook.character.cooking.pair4[1]))
			{
				cook.character.cooking.ingredients[cook.character.cooking.pair4[1]].curLevel = foodH;
			}
		}
	}
}