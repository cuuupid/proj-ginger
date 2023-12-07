using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley
{
	public class BluePrint
	{
		public string name;

		public int woodRequired;

		public int stoneRequired;

		public int copperRequired;

		public int IronRequired;

		public int GoldRequired;

		public int IridiumRequired;

		public int tilesWidth;

		public int tilesHeight;

		public int maxOccupants;

		public int moneyRequired;

		public int daysToConstruct;

		public Point humanDoor;

		public Point animalDoor;

		public string mapToWarpTo;

		public string description;

		public string blueprintType;

		public string nameOfBuildingToUpgrade;

		public string actionBehavior;

		public string displayName;

		public readonly string textureName;

		public readonly Texture2D texture;

		public List<string> namesOfOkayBuildingLocations = new List<string>();

		public Rectangle sourceRectForMenuView;

		public Dictionary<int, int> itemsRequired = new Dictionary<int, int>();

		public bool canBuildOnCurrentMap;

		public bool magical;

		public List<Point> additionalPlacementTiles = new List<Point>();

		public BluePrint(string name)
		{
			this.name = name;
			if (name.Equals("Info Tool"))
			{
				textureName = "LooseSprites\\Cursors";
				displayName = name;
				description = Game1.content.LoadString("Strings\\StringsFromCSFiles:BluePrint.cs.1");
				sourceRectForMenuView = new Rectangle(576, 0, 64, 64);
			}
			else
			{
				Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\Blueprints");
				string value = null;
				dictionary.TryGetValue(name, out value);
				if (value != null)
				{
					string[] array = value.Split('/');
					if (array[0].Equals("animal"))
					{
						try
						{
							textureName = "Animals\\" + (name.Equals("Chicken") ? "White Chicken" : name);
						}
						catch (Exception)
						{
							Game1.debugOutput = "Blueprint loaded with no texture!";
						}
						moneyRequired = Convert.ToInt32(array[1]);
						sourceRectForMenuView = new Rectangle(0, 0, Convert.ToInt32(array[2]), Convert.ToInt32(array[3]));
						blueprintType = "Animals";
						tilesWidth = 1;
						tilesHeight = 1;
						displayName = array[4];
						description = array[5];
						humanDoor = new Point(-1, -1);
						animalDoor = new Point(-1, -1);
					}
					else
					{
						textureName = "Buildings\\" + name;
						string[] array2 = array[0].Split(' ');
						for (int i = 0; i < array2.Length; i += 2)
						{
							if (!array2[i].Equals(""))
							{
								itemsRequired.Add(Convert.ToInt32(array2[i]), Convert.ToInt32(array2[i + 1]));
							}
						}
						tilesWidth = Convert.ToInt32(array[1]);
						tilesHeight = Convert.ToInt32(array[2]);
						humanDoor = new Point(Convert.ToInt32(array[3]), Convert.ToInt32(array[4]));
						animalDoor = new Point(Convert.ToInt32(array[5]), Convert.ToInt32(array[6]));
						mapToWarpTo = array[7];
						displayName = array[8];
						description = array[9];
						blueprintType = array[10];
						if (blueprintType.Equals("Upgrades"))
						{
							nameOfBuildingToUpgrade = array[11];
						}
						sourceRectForMenuView = new Rectangle(0, 0, Convert.ToInt32(array[12]), Convert.ToInt32(array[13]));
						maxOccupants = Convert.ToInt32(array[14]);
						actionBehavior = array[15];
						string[] array3 = array[16].Split(' ');
						string[] array4 = array3;
						foreach (string item in array4)
						{
							namesOfOkayBuildingLocations.Add(item);
						}
						int num = 17;
						if (array.Length > num)
						{
							moneyRequired = Convert.ToInt32(array[17]);
						}
						if (array.Length > num + 1)
						{
							magical = Convert.ToBoolean(array[18]);
						}
						if (array.Length > num + 2)
						{
							daysToConstruct = Convert.ToInt32(array[19]);
						}
						else
						{
							daysToConstruct = 2;
						}
						if (array.Length > num + 3)
						{
							string text = array[20];
							additionalPlacementTiles.Clear();
							string[] array5 = text.Split(' ');
							for (int k = 0; k < array5.Length / 2; k++)
							{
								int x = Convert.ToInt32(array5[k * 2]);
								int y = Convert.ToInt32(array5[k * 2 + 1]);
								additionalPlacementTiles.Add(new Point(x, y));
							}
						}
					}
				}
			}
			try
			{
				texture = Game1.content.Load<Texture2D>(textureName);
			}
			catch (Exception)
			{
			}
		}

		public void consumeResources()
		{
			foreach (KeyValuePair<int, int> item in itemsRequired)
			{
				Game1.player.consumeObject(item.Key, item.Value);
			}
			Game1.player.Money -= moneyRequired;
		}

		public int getTileSheetIndexForStructurePlacementTile(int x, int y)
		{
			if (x == humanDoor.X && y == humanDoor.Y)
			{
				return 2;
			}
			if (x == animalDoor.X && y == animalDoor.Y)
			{
				return 4;
			}
			return 0;
		}

		public bool isUpgrade()
		{
			if (nameOfBuildingToUpgrade != null)
			{
				return nameOfBuildingToUpgrade.Length > 0;
			}
			return false;
		}

		public bool doesFarmerHaveEnoughResourcesToBuild()
		{
			if (moneyRequired < 0)
			{
				return false;
			}
			foreach (KeyValuePair<int, int> item in itemsRequired)
			{
				if (!Game1.player.hasItemInInventory(item.Key, item.Value))
				{
					return false;
				}
			}
			if (Game1.player.Money < moneyRequired)
			{
				return false;
			}
			return true;
		}

		public void drawDescription(SpriteBatch b, int x, int y, int width)
		{
			b.DrawString(Game1.smallFont, name, new Vector2(x, y), Game1.textColor);
			string text = Game1.parseText(description, Game1.smallFont, width);
			b.DrawString(Game1.smallFont, text, new Vector2(x, (float)y + Game1.smallFont.MeasureString(name).Y), Game1.textColor * 0.75f);
			int num = (int)((float)y + Game1.smallFont.MeasureString(name).Y + Game1.smallFont.MeasureString(text).Y);
			foreach (KeyValuePair<int, int> item in itemsRequired)
			{
				b.Draw(Game1.objectSpriteSheet, new Vector2(x + 8, num), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, item.Key, 16, 16), Color.White, 0f, new Vector2(6f, 3f), 2f, SpriteEffects.None, 0.999f);
				Color color = (Game1.player.hasItemInInventory(item.Key, item.Value) ? Color.DarkGreen : Color.DarkRed);
				Utility.drawTinyDigits(item.Value, b, new Vector2((float)(x + 32) - Game1.tinyFont.MeasureString(item.Value.ToString() ?? "").X, (float)(num + 32) - Game1.tinyFont.MeasureString(item.Value.ToString() ?? "").Y), 1f, 0.9f, Color.AntiqueWhite);
				b.DrawString(Game1.smallFont, Game1.objectInformation[item.Key].Split('/')[4], new Vector2(x + 32 + 16, num), color);
				num += (int)Game1.smallFont.MeasureString("P").Y;
			}
			if (moneyRequired > 0)
			{
				b.Draw(Game1.debrisSpriteSheet, new Vector2(x, num), Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8), Color.White, 0f, new Vector2(24f, 11f), 0.5f, SpriteEffects.None, 0.999f);
				Color color2 = ((Game1.player.Money >= moneyRequired) ? Color.DarkGreen : Color.DarkRed);
				b.DrawString(Game1.smallFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", moneyRequired), new Vector2(x + 16 + 8, num), color2);
				num += (int)Game1.smallFont.MeasureString(moneyRequired.ToString() ?? "").Y;
			}
		}
	}
}
