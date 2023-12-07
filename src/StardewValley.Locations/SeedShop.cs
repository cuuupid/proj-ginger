using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Objects;

namespace StardewValley.Locations
{
	public class SeedShop : ShopLocation
	{
		protected bool _stockListGranted;

		public SeedShop()
		{
		}

		public SeedShop(string map, string name)
			: base(map, name)
		{
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if ((int)Game1.player.maxItems == 12)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(456f, 1088f)), new Rectangle(255, 1436, 12, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1232f);
			}
			else if ((int)Game1.player.maxItems < 36)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(456f, 1088f)), new Rectangle(267, 1436, 12, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1232f);
			}
			else
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Rectangle(452, 1184, 112, 20)), new Rectangle(258, 1449, 1, 1), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.1232f);
			}
		}

		public override string getPurchasedItemDialogueForNPC(Object i, NPC n)
		{
			string result = "...";
			string[] array = Game1.content.LoadString("Strings\\Lexicon:GenericPlayerTerm").Split('^');
			string text = array[0];
			if (array.Length > 1 && !Game1.player.isMale)
			{
				text = array[1];
			}
			string text2 = ((Game1.random.NextDouble() < (double)(Game1.player.getFriendshipLevelForNPC(n.Name) / 1250)) ? Game1.player.Name : text);
			if (n.Age != 0)
			{
				text2 = Game1.player.Name;
			}
			string text3 = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? Lexicon.getProperArticleForWord(i.name) : "");
			if ((i.Category == -4 || i.Category == -75 || i.Category == -79) && Game1.random.NextDouble() < 0.5)
			{
				text3 = Game1.content.LoadString("Strings\\StringsFromCSFiles:SeedShop.cs.9701");
			}
			int num = Game1.random.Next(5);
			if (n.Manners == 2)
			{
				num = 2;
			}
			switch (num)
			{
			case 0:
				result = ((!(Game1.random.NextDouble() < (double)(int)i.quality * 0.5 + 0.2)) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_1_QualityLow", text2, text3, i.DisplayName, Lexicon.getRandomNegativeFoodAdjective(n)) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_1_QualityHigh", text2, text3, i.DisplayName, Lexicon.getRandomDeliciousAdjective(n)));
				break;
			case 1:
				result = (((int)i.quality != 0) ? ((!n.Name.Equals("Jodi")) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_2_QualityHigh", text2, text3, i.DisplayName) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_2_QualityHigh_Jodi", text2, text3, i.DisplayName)) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_2_QualityLow", text2, text3, i.DisplayName));
				break;
			case 2:
				if (n.Manners == 2)
				{
					if ((int)i.quality != 2)
					{
						result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_3_QualityLow_Rude", text2, text3, i.DisplayName, i.salePrice() / 2, Lexicon.getRandomNegativeFoodAdjective(n), Lexicon.getRandomNegativeItemSlanderNoun());
					}
					else
					{
						Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_3_QualityHigh_Rude", text2, text3, i.DisplayName, i.salePrice() / 2, Lexicon.getRandomSlightlyPositiveAdjectiveForEdibleNoun(n));
					}
				}
				else
				{
					Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_3_NonRude", text2, text3, i.DisplayName, i.salePrice() / 2);
				}
				break;
			case 3:
				result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_4", text2, text3, i.DisplayName);
				break;
			case 4:
				if (i.Category == -75 || i.Category == -79)
				{
					result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_5_VegetableOrFruit", text2, text3, i.DisplayName);
				}
				else if (i.Category == -7)
				{
					string randomPositiveAdjectiveForEventOrPerson = Lexicon.getRandomPositiveAdjectiveForEventOrPerson(n);
					result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_5_Cooking", text2, text3, i.DisplayName, Lexicon.getProperArticleForWord(randomPositiveAdjectiveForEventOrPerson), randomPositiveAdjectiveForEventOrPerson);
				}
				else
				{
					result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_5_Foraged", text2, text3, i.DisplayName);
				}
				break;
			}
			if (n.Age == 1 && Game1.random.NextDouble() < 0.6)
			{
				result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Teen", text2, text3, i.DisplayName);
			}
			string text4 = n.Name;
			if (text4 != null)
			{
				switch (text4.Length)
				{
				case 7:
					switch (text4[0])
					{
					case 'A':
						if (text4 == "Abigail")
						{
							result = (((int)i.quality != 0) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Abigail_QualityHigh", text2, text3, i.DisplayName) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Abigail_QualityLow", text2, text3, i.DisplayName, Lexicon.getRandomNegativeItemSlanderNoun()));
						}
						break;
					case 'E':
						if (text4 == "Elliott")
						{
							result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Elliott", text2, text3, i.DisplayName);
						}
						break;
					}
					break;
				case 4:
					switch (text4[0])
					{
					case 'A':
						if (text4 == "Alex")
						{
							result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Alex", text2, text3, i.DisplayName);
						}
						break;
					case 'L':
						if (text4 == "Leah")
						{
							result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Leah", text2, text3, i.DisplayName);
						}
						break;
					}
					break;
				case 8:
					if (text4 == "Caroline")
					{
						result = (((int)i.quality != 0) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Caroline_QualityHigh", text2, text3, i.DisplayName) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Caroline_QualityLow", text2, text3, i.DisplayName));
					}
					break;
				case 6:
					if (text4 == "Pierre")
					{
						result = (((int)i.quality != 0) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Pierre_QualityHigh", text2, text3, i.DisplayName) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Pierre_QualityLow", text2, text3, i.DisplayName));
					}
					break;
				case 5:
					if (text4 == "Haley")
					{
						result = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Haley", text2, text3, i.DisplayName);
					}
					break;
				}
			}
			return result;
		}

		private void addStock(Dictionary<ISalable, int[]> stock, int parentSheetIndex, int buyPrice = -1, string item_season = null)
		{
			float num = 2f;
			int num2 = buyPrice;
			Object @object = new Object(Vector2.Zero, parentSheetIndex, 1);
			if (buyPrice == -1)
			{
				num2 = @object.salePrice();
				num = 1f;
			}
			else if (@object.isSapling())
			{
				num *= Game1.MasterPlayer.difficultyModifier;
			}
			if (item_season != null && item_season != Game1.currentSeason)
			{
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("PierreStocklist"))
				{
					return;
				}
				num *= 1.5f;
			}
			num2 = (int)((float)num2 * num);
			if (item_season != null)
			{
				foreach (KeyValuePair<ISalable, int[]> item in stock)
				{
					if (item.Key == null || !(item.Key is Object))
					{
						continue;
					}
					Object object2 = item.Key as Object;
					if (Utility.IsNormalObjectAtParentSheetIndex(object2, parentSheetIndex))
					{
						if (item.Value.Length != 0 && num2 < item.Value[0])
						{
							item.Value[0] = num2;
							stock[object2] = item.Value;
						}
						return;
					}
				}
			}
			stock.Add(@object, new int[2] { num2, 2147483647 });
		}

		public Dictionary<ISalable, int[]> shopStock()
		{
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			addStock(dictionary, 472, -1, "spring");
			addStock(dictionary, 473, -1, "spring");
			addStock(dictionary, 474, -1, "spring");
			addStock(dictionary, 475, -1, "spring");
			addStock(dictionary, 427, -1, "spring");
			addStock(dictionary, 477, -1, "spring");
			addStock(dictionary, 429, -1, "spring");
			if (Game1.year > 1)
			{
				addStock(dictionary, 476, -1, "spring");
				addStock(dictionary, 273, -1, "spring");
			}
			addStock(dictionary, 479, -1, "summer");
			addStock(dictionary, 480, -1, "summer");
			addStock(dictionary, 481, -1, "summer");
			addStock(dictionary, 482, -1, "summer");
			addStock(dictionary, 483, -1, "summer");
			addStock(dictionary, 484, -1, "summer");
			addStock(dictionary, 453, -1, "summer");
			addStock(dictionary, 455, -1, "summer");
			addStock(dictionary, 302, -1, "summer");
			addStock(dictionary, 487, -1, "summer");
			addStock(dictionary, 431, 100, "summer");
			if (Game1.year > 1)
			{
				addStock(dictionary, 485, -1, "summer");
			}
			addStock(dictionary, 490, -1, "fall");
			addStock(dictionary, 487, -1, "fall");
			addStock(dictionary, 488, -1, "fall");
			addStock(dictionary, 491, -1, "fall");
			addStock(dictionary, 492, -1, "fall");
			addStock(dictionary, 493, -1, "fall");
			addStock(dictionary, 483, -1, "fall");
			addStock(dictionary, 431, 100, "fall");
			addStock(dictionary, 425, -1, "fall");
			addStock(dictionary, 299, -1, "fall");
			addStock(dictionary, 301, -1, "fall");
			if (Game1.year > 1)
			{
				addStock(dictionary, 489, -1, "fall");
			}
			addStock(dictionary, 297);
			if (!Game1.player.craftingRecipes.ContainsKey("Grass Starter"))
			{
				dictionary.Add(new Object(297, 1, isRecipe: true), new int[2] { 1000, 1 });
			}
			addStock(dictionary, 245);
			addStock(dictionary, 246);
			addStock(dictionary, 423);
			addStock(dictionary, 247);
			addStock(dictionary, 419);
			if ((int)Game1.stats.DaysPlayed >= 15)
			{
				addStock(dictionary, 368, 50);
				addStock(dictionary, 370, 50);
				addStock(dictionary, 465, 50);
			}
			if (Game1.year > 1)
			{
				addStock(dictionary, 369, 75);
				addStock(dictionary, 371, 75);
				addStock(dictionary, 466, 75);
			}
			Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			int num = random.Next(112);
			if (num == 21)
			{
				num = 36;
			}
			Wallpaper wallpaper = new Wallpaper(num);
			dictionary.Add(wallpaper, new int[2]
			{
				wallpaper.salePrice(),
				2147483647
			});
			wallpaper = new Wallpaper(random.Next(56), isFloor: true);
			dictionary.Add(wallpaper, new int[2]
			{
				wallpaper.salePrice(),
				2147483647
			});
			Furniture furniture = new Furniture(1308, Vector2.Zero);
			dictionary.Add(furniture, new int[2]
			{
				furniture.salePrice(),
				2147483647
			});
			addStock(dictionary, 628, 1700);
			addStock(dictionary, 629, 1000);
			addStock(dictionary, 630, 2000);
			addStock(dictionary, 631, 3000);
			addStock(dictionary, 632, 3000);
			addStock(dictionary, 633, 2000);
			foreach (Item item in itemsFromPlayerToSell)
			{
				if (item.Stack > 0)
				{
					int num2 = item.salePrice();
					if (item is Object)
					{
						num2 = (item as Object).sellToStorePrice(-1L);
					}
					dictionary.Add(item, new int[2] { num2, item.Stack });
				}
			}
			if (Game1.player.hasAFriendWithHeartLevel(8, datablesOnly: true))
			{
				addStock(dictionary, 458);
			}
			return dictionary;
		}
	}
}
