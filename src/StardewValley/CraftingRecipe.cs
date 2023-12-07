using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;

namespace StardewValley
{
	public class CraftingRecipe
	{
		public const int wild_seed_special_category = -777;

		public string name;

		public string DisplayName;

		public string description;

		public static Dictionary<string, string> craftingRecipes;

		public static Dictionary<string, string> cookingRecipes;

		public Dictionary<int, int> recipeList = new Dictionary<int, int>();

		public List<int> itemToProduce = new List<int>();

		public bool bigCraftable;

		public bool isCookingRecipe;

		public int timesCrafted;

		public int numberProducedPerCraft;

		public string itemType;

		public string ItemType
		{
			get
			{
				if (itemType != null && !(itemType == ""))
				{
					return itemType;
				}
				if (!bigCraftable)
				{
					return "O";
				}
				return "BO";
			}
		}

		public static void InitShared()
		{
			craftingRecipes = Game1.content.Load<Dictionary<string, string>>("Data\\CraftingRecipes");
			cookingRecipes = Game1.content.Load<Dictionary<string, string>>("Data\\CookingRecipes");
		}

		public CraftingRecipe(string name)
			: this(name, cookingRecipes.ContainsKey(name))
		{
		}

		public CraftingRecipe(string name, bool isCookingRecipe)
		{
			this.isCookingRecipe = isCookingRecipe;
			this.name = name;
			string text = ((isCookingRecipe && cookingRecipes.ContainsKey(name)) ? cookingRecipes[name] : (craftingRecipes.ContainsKey(name) ? craftingRecipes[name] : null));
			if (text == null)
			{
				this.name = "Torch";
				name = "Torch";
				text = craftingRecipes[name];
			}
			string[] array = text.Split('/');
			string[] array2 = array[0].Split(' ');
			for (int i = 0; i < array2.Length; i += 2)
			{
				recipeList.Add(Convert.ToInt32(array2[i]), Convert.ToInt32(array2[i + 1]));
			}
			string[] array3 = array[2].Split(' ');
			for (int j = 0; j < array3.Length; j += 2)
			{
				itemToProduce.Add(Convert.ToInt32(array3[j]));
				numberProducedPerCraft = ((array3.Length <= 1) ? 1 : Convert.ToInt32(array3[j + 1]));
			}
			if (!isCookingRecipe)
			{
				if (array[3] == "true")
				{
					itemType = "BO";
					bigCraftable = true;
				}
				else if (array[3] == "false")
				{
					itemType = "O";
				}
				else
				{
					itemType = array[3];
				}
			}
			try
			{
				description = (bigCraftable ? Game1.bigCraftablesInformation[itemToProduce[0]].Split('/')[4] : Game1.objectInformation[itemToProduce[0]].Split('/')[5]);
			}
			catch (Exception)
			{
				description = "";
			}
			timesCrafted = (Game1.player.craftingRecipes.ContainsKey(name) ? Game1.player.craftingRecipes[name] : 0);
			if (name.Equals("Crab Pot") && Game1.player.professions.Contains(7))
			{
				recipeList = new Dictionary<int, int>();
				recipeList.Add(388, 25);
				recipeList.Add(334, 2);
			}
			if (LocalizedContentManager.CurrentLanguageCode != 0)
			{
				DisplayName = array[array.Length - 1];
			}
			else
			{
				DisplayName = name;
			}
		}

		public int getIndexOfMenuView()
		{
			if (itemToProduce.Count <= 0)
			{
				return -1;
			}
			return itemToProduce[0];
		}

		public virtual bool doesFarmerHaveIngredientsInInventory(IList<Item> extraToCheck = null)
		{
			foreach (KeyValuePair<int, int> recipe in recipeList)
			{
				int value = recipe.Value;
				value -= Game1.player.getItemCount(recipe.Key, 5);
				if (value <= 0)
				{
					continue;
				}
				if (extraToCheck != null)
				{
					value -= Game1.player.getItemCountInList(extraToCheck, recipe.Key, 5);
					if (value <= 0)
					{
						continue;
					}
				}
				return false;
			}
			return true;
		}

		public int howManyCanWeMake(IList<Item> extraToCheck = null)
		{
			int num = 999;
			foreach (KeyValuePair<int, int> recipe in recipeList)
			{
				int num2 = Game1.player.howManyOfItemInInventory(recipe.Key) / recipe.Value;
				if (extraToCheck != null)
				{
					num2 += Game1.player.howManyOfItemInList(extraToCheck, recipe.Key) / recipe.Value;
				}
				if (num2 < num)
				{
					num = num2;
				}
			}
			return num;
		}

		public virtual void drawMenuView(SpriteBatch b, int x, int y, float layerDepth = 0.88f, bool shadow = true)
		{
			if (bigCraftable)
			{
				Utility.drawWithShadow(b, Game1.bigCraftableSpriteSheet, new Vector2(x, y), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, getIndexOfMenuView(), 16, 32), Color.White, 0f, Vector2.Zero, 4f, flipped: false, layerDepth);
			}
			else
			{
				Utility.drawWithShadow(b, Game1.objectSpriteSheet, new Vector2(x, y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, getIndexOfMenuView(), 16, 16), Color.White, 0f, Vector2.Zero, 4f, flipped: false, layerDepth);
			}
		}

		public virtual Item createItem()
		{
			int num = itemToProduce.ElementAt(Game1.random.Next(itemToProduce.Count));
			if (bigCraftable)
			{
				if (name.Equals("Chest"))
				{
					return new Chest(playerChest: true);
				}
				return new Object(Vector2.Zero, num);
			}
			if (name.Equals("Torch"))
			{
				return new Torch(Vector2.Zero, numberProducedPerCraft);
			}
			if ((num >= 516 && num <= 534) || num == 801)
			{
				return new Ring(num);
			}
			Item itemFromStandardTextDescription = Utility.getItemFromStandardTextDescription(ItemType + " " + num + " " + numberProducedPerCraft, Game1.player);
			if (isCookingRecipe && itemFromStandardTextDescription is Object && Game1.player.team.SpecialOrderRuleActive("QI_COOKING"))
			{
				(itemFromStandardTextDescription as Object).orderData.Value = "QI_COOKING";
				itemFromStandardTextDescription.MarkContextTagsDirty();
			}
			return itemFromStandardTextDescription;
		}

		public static bool isThereSpecialIngredientRule(Object potentialIngredient, int requiredIngredient)
		{
			if (requiredIngredient == -777 && ((int)potentialIngredient.parentSheetIndex == 495 || (int)potentialIngredient.parentSheetIndex == 496 || (int)potentialIngredient.parentSheetIndex == 497 || (int)potentialIngredient.parentSheetIndex == 498))
			{
				return true;
			}
			return false;
		}

		public void consumeIngredients(List<Chest> additional_materials)
		{
			for (int num = recipeList.Count - 1; num >= 0; num--)
			{
				int num2 = recipeList[recipeList.Keys.ElementAt(num)];
				bool flag = false;
				for (int num3 = Game1.player.items.Count - 1; num3 >= 0; num3--)
				{
					if (Game1.player.items[num3] != null && Game1.player.items[num3] is Object && !(Game1.player.items[num3] as Object).bigCraftable && ((int)((Object)Game1.player.items[num3]).parentSheetIndex == recipeList.Keys.ElementAt(num) || ((Object)Game1.player.items[num3]).Category == recipeList.Keys.ElementAt(num) || isThereSpecialIngredientRule((Object)Game1.player.items[num3], recipeList.Keys.ElementAt(num))))
					{
						int num4 = num2;
						num2 -= Game1.player.items[num3].Stack;
						Game1.player.items[num3].Stack -= num4;
						if (Game1.player.items[num3].Stack <= 0)
						{
							Game1.player.items[num3] = null;
						}
						if (num2 <= 0)
						{
							flag = true;
							break;
						}
					}
				}
				if (additional_materials != null && !flag)
				{
					for (int i = 0; i < additional_materials.Count; i++)
					{
						Chest chest = additional_materials[i];
						if (chest == null)
						{
							continue;
						}
						bool flag2 = false;
						for (int num5 = chest.items.Count - 1; num5 >= 0; num5--)
						{
							if (chest.items[num5] != null && chest.items[num5] is Object && ((int)((Object)chest.items[num5]).parentSheetIndex == recipeList.Keys.ElementAt(num) || ((Object)chest.items[num5]).Category == recipeList.Keys.ElementAt(num) || isThereSpecialIngredientRule((Object)chest.items[num5], recipeList.Keys.ElementAt(num))))
							{
								int num6 = Math.Min(num2, chest.items[num5].Stack);
								num2 -= num6;
								chest.items[num5].Stack -= num6;
								if (chest.items[num5].Stack <= 0)
								{
									chest.items[num5] = null;
									flag2 = true;
								}
								if (num2 <= 0)
								{
									break;
								}
							}
						}
						if (flag2)
						{
							chest.clearNulls();
						}
						if (num2 <= 0)
						{
							break;
						}
					}
				}
			}
		}

		public static bool DoesFarmerHaveAdditionalIngredientsInInventory(List<KeyValuePair<int, int>> additional_recipe_items, IList<Item> extraToCheck = null)
		{
			foreach (KeyValuePair<int, int> additional_recipe_item in additional_recipe_items)
			{
				int value = additional_recipe_item.Value;
				value -= Game1.player.getItemCount(additional_recipe_item.Key, 5);
				if (value <= 0)
				{
					continue;
				}
				if (extraToCheck != null)
				{
					value -= Game1.player.getItemCountInList(extraToCheck, additional_recipe_item.Key, 5);
					if (value <= 0)
					{
						continue;
					}
				}
				return false;
			}
			return true;
		}

		public static void ConsumeAdditionalIngredients(List<KeyValuePair<int, int>> additional_recipe_items, List<Chest> additional_materials)
		{
			for (int num = additional_recipe_items.Count - 1; num >= 0; num--)
			{
				int key = additional_recipe_items[num].Key;
				int num2 = additional_recipe_items[num].Value;
				bool flag = false;
				for (int num3 = Game1.player.items.Count - 1; num3 >= 0; num3--)
				{
					if (Game1.player.items[num3] != null && Game1.player.items[num3] is Object && !(Game1.player.items[num3] as Object).bigCraftable && ((int)((Object)Game1.player.items[num3]).parentSheetIndex == key || ((Object)Game1.player.items[num3]).Category == key || isThereSpecialIngredientRule((Object)Game1.player.items[num3], key)))
					{
						int num4 = num2;
						num2 -= Game1.player.items[num3].Stack;
						Game1.player.items[num3].Stack -= num4;
						if (Game1.player.items[num3].Stack <= 0)
						{
							Game1.player.items[num3] = null;
						}
						if (num2 <= 0)
						{
							flag = true;
							break;
						}
					}
				}
				if (additional_materials != null && !flag)
				{
					for (int i = 0; i < additional_materials.Count; i++)
					{
						Chest chest = additional_materials[i];
						if (chest == null)
						{
							continue;
						}
						bool flag2 = false;
						for (int num5 = chest.items.Count - 1; num5 >= 0; num5--)
						{
							if (chest.items[num5] != null && chest.items[num5] is Object && ((int)((Object)chest.items[num5]).parentSheetIndex == key || ((Object)chest.items[num5]).Category == key || isThereSpecialIngredientRule((Object)chest.items[num5], key)))
							{
								int num6 = Math.Min(num2, chest.items[num5].Stack);
								num2 -= num6;
								chest.items[num5].Stack -= num6;
								if (chest.items[num5].Stack <= 0)
								{
									chest.items[num5] = null;
									flag2 = true;
								}
								if (num2 <= 0)
								{
									break;
								}
							}
						}
						if (flag2)
						{
							chest.clearNulls();
						}
						if (num2 <= 0)
						{
							break;
						}
					}
				}
			}
		}

		public virtual int getCraftableCount(IList<Chest> additional_material_chests)
		{
			List<Item> list = new List<Item>();
			if (additional_material_chests != null)
			{
				for (int i = 0; i < additional_material_chests.Count; i++)
				{
					list.AddRange(additional_material_chests[i].items);
				}
			}
			return getCraftableCount(list);
		}

		public int getCraftableCount(IList<Item> additional_materials)
		{
			int num = -1;
			for (int num2 = recipeList.Count - 1; num2 >= 0; num2--)
			{
				int num3 = 0;
				int num4 = recipeList[recipeList.Keys.ElementAt(num2)];
				for (int num5 = Game1.player.items.Count - 1; num5 >= 0; num5--)
				{
					if (Game1.player.items[num5] != null && Game1.player.items[num5] is Object && !(Game1.player.items[num5] as Object).bigCraftable && ((int)((Object)Game1.player.items[num5]).parentSheetIndex == recipeList.Keys.ElementAt(num2) || ((Object)Game1.player.items[num5]).Category == recipeList.Keys.ElementAt(num2) || isThereSpecialIngredientRule((Object)Game1.player.items[num5], recipeList.Keys.ElementAt(num2))))
					{
						num3 += Game1.player.items[num5].Stack;
					}
				}
				if (additional_materials != null)
				{
					for (int i = 0; i < additional_materials.Count; i++)
					{
						Item item = additional_materials[i];
						if (item != null && item is Object && ((Utility.IsNormalObjectAtParentSheetIndex(item, item.ParentSheetIndex) && (int)((Object)item).parentSheetIndex == recipeList.Keys.ElementAt(num2)) || ((Object)item).Category == recipeList.Keys.ElementAt(num2) || isThereSpecialIngredientRule((Object)item, recipeList.Keys.ElementAt(num2))))
						{
							num3 += item.Stack;
						}
					}
				}
				int num6 = num3 / num4;
				if (num6 < num || num == -1)
				{
					num = num6;
				}
			}
			return num;
		}

		public virtual string getCraftCountText()
		{
			if (isCookingRecipe)
			{
				if (Game1.player.recipesCooked.ContainsKey(getIndexOfMenuView()) && Game1.player.recipesCooked[getIndexOfMenuView()] > 0)
				{
					return Game1.content.LoadString("Strings\\UI:Collections_Description_RecipesCooked", Game1.player.recipesCooked[getIndexOfMenuView()]);
				}
			}
			else if (Game1.player.craftingRecipes.ContainsKey(name) && Game1.player.craftingRecipes[name] > 0)
			{
				return Game1.content.LoadString("Strings\\UI:Crafting_NumberCrafted", Game1.player.craftingRecipes[name]);
			}
			return null;
		}

		public int getDescriptionHeight(int width)
		{
			float num = 32f;
			num = Game1.smallFont.MeasureString("I").Y;
			if (Game1.options.bigFonts)
			{
				num += 4f;
			}
			return (int)(Game1.smallFont.MeasureString(Game1.parseText(description, Game1.smallFont, width)).Y + (float)getNumberOfIngredients() * (num + 4f) + (float)(int)Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.567")).Y + 21f);
		}

		public virtual void drawRecipeDescription(SpriteBatch b, Vector2 position, int width, IList<Item> additional_crafting_items = null, bool drawSmall = false)
		{
			int num = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 8 : 0);
			if (drawSmall)
			{
				position.Y -= 16f;
			}
			else
			{
				b.Draw(Game1.staminaRect, new Rectangle((int)(position.X + 8f), (int)(position.Y + 32f + Game1.smallFont.MeasureString("Ing!").Y) - 4 - 2 - (int)((float)num * 1.5f), width - 32, 2), Game1.textColor * 0.35f);
				Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.567"), Game1.smallFont, position + new Vector2(8f, 28f), Game1.textColor * 0.75f);
			}
			int num2 = 0;
			foreach (KeyValuePair<int, int> recipe in recipeList)
			{
				int value = recipe.Value;
				int key = recipe.Key;
				int itemCount = Game1.player.getItemCount(key, 8);
				int num3 = 0;
				value -= itemCount;
				if (additional_crafting_items != null)
				{
					num3 = Game1.player.getItemCountInList(additional_crafting_items, key, 8);
					if (value > 0)
					{
						value -= num3;
					}
				}
				string nameFromIndex = getNameFromIndex(recipe.Key);
				float num4 = 32f;
				num4 = Game1.smallFont.MeasureString(nameFromIndex).Y;
				Color color = ((value <= 0) ? Game1.textColor : Color.Red);
				b.Draw(Game1.objectSpriteSheet, new Vector2(position.X, position.Y + 64f + (float)num2 * num4 + (float)(num2 * 4)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, getSpriteIndexFromRawIndex(recipe.Key), 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.86f);
				Utility.drawTinyDigits(recipe.Value, b, new Vector2(position.X + 32f - Game1.tinyFont.MeasureString(recipe.Value.ToString() ?? "").X, position.Y + 64f + (float)num2 * num4 + (float)(num2 * 4) + 21f), 2f, 0.87f, Color.AntiqueWhite);
				Vector2 vector = new Vector2(position.X + 32f + 8f, position.Y + 64f + (float)num2 * num4 + (float)(num2 * 4) + 0f);
				Utility.drawTextWithShadow(b, nameFromIndex, Game1.smallFont, vector, color);
				if (Game1.options.showAdvancedCraftingInformation)
				{
					vector.X = position.X + (float)width - 40f;
					b.Draw(Game1.mouseCursors, new Rectangle((int)vector.X, (int)vector.Y + 2, 22, 26), new Rectangle(268, 1436, 11, 13), Color.White);
					Utility.drawTextWithShadow(b, (itemCount + num3).ToString() ?? "", Game1.smallFont, vector - new Vector2(Game1.smallFont.MeasureString(itemCount + num3 + " ").X, 0f), color);
				}
				num2++;
			}
			float num5 = 32f;
			num5 = Game1.smallFont.MeasureString("I").Y;
			b.Draw(Game1.staminaRect, new Rectangle((int)position.X + 8, (int)position.Y + num + 64 + 4 + recipeList.Count * ((int)num5 + 4), width - 32, 2), Game1.textColor * 0.35f);
			Utility.drawTextWithShadow(b, Game1.parseText(description, Game1.smallFont, width - 8), Game1.smallFont, position + new Vector2(0f, 76f + (float)recipeList.Count * (num5 + 4f) + (float)num), Game1.textColor * 0.75f);
		}

		public virtual int getNumberOfIngredients()
		{
			return recipeList.Count;
		}

		public int getSpriteIndexFromRawIndex(int index)
		{
			return index switch
			{
				-1 => 20, 
				-2 => 80, 
				-3 => 24, 
				-4 => 145, 
				-5 => 176, 
				-6 => 184, 
				-777 => 495, 
				_ => index, 
			};
		}

		public string getNameFromIndex(int index)
		{
			if (index < 0)
			{
				return index switch
				{
					-1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.568"), 
					-2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.569"), 
					-3 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.570"), 
					-4 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.571"), 
					-5 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.572"), 
					-6 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.573"), 
					-777 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.574"), 
					_ => "???", 
				};
			}
			string result = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.575");
			if (Game1.objectInformation.ContainsKey(index))
			{
				result = Game1.objectInformation[index].Split('/')[4];
			}
			return result;
		}
	}
}
