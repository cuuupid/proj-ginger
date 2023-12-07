using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buildings;
using xTile;
using xTile.Dimensions;

namespace StardewValley.Menus
{
	public class BlueprintsMenu : IClickableMenu
	{
		public static int heightOfDescriptionBox = 384;

		public static int blueprintButtonMargin = 32;

		public new static int tabYPositionRelativeToMenuY = -48;

		public const int buildingsTab = 0;

		public const int upgradesTab = 1;

		public const int decorationsTab = 2;

		public const int demolishTab = 3;

		public const int animalsTab = 4;

		public const int numberOfTabs = 5;

		private bool placingStructure;

		private bool demolishing;

		private bool upgrading;

		private bool queryingAnimals;

		private int currentTab;

		private Vector2 positionOfAnimalWhenClicked;

		private string hoverText = "";

		private List<Dictionary<ClickableComponent, BluePrint>> blueprintButtons = new List<Dictionary<ClickableComponent, BluePrint>>();

		private List<ClickableComponent> tabs = new List<ClickableComponent>();

		private BluePrint hoveredItem;

		private BluePrint structureForPlacement;

		private FarmAnimal currentAnimal;

		private Texture2D buildingPlacementTiles;

		public BlueprintsMenu(int x, int y)
			: base(x, y, Game1.viewport.Width / 2 + 96, 0)
		{
			tabYPositionRelativeToMenuY = -48;
			blueprintButtonMargin = 32;
			heightOfDescriptionBox = 384;
			for (int i = 0; i < 5; i++)
			{
				blueprintButtons.Add(new Dictionary<ClickableComponent, BluePrint>());
			}
			xPositionOnScreen = x;
			yPositionOnScreen = y;
			int[] array = new int[5];
			for (int j = 0; j < Game1.player.blueprints.Count; j++)
			{
				BluePrint bluePrint = new BluePrint(Game1.player.blueprints[j]);
				int tabNumberFromName = getTabNumberFromName(bluePrint.blueprintType);
				if (bluePrint.blueprintType != null)
				{
					int num = (int)((float)Math.Max(bluePrint.tilesWidth, 4) / 4f * 64f) + blueprintButtonMargin;
					if (array[tabNumberFromName] % (width - IClickableMenu.borderWidth * 2) + num > width - IClickableMenu.borderWidth * 2)
					{
						array[tabNumberFromName] += width - IClickableMenu.borderWidth * 2 - array[tabNumberFromName] % (width - IClickableMenu.borderWidth * 2);
					}
					blueprintButtons[Math.Min(4, tabNumberFromName)].Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(x + IClickableMenu.borderWidth + array[tabNumberFromName] % (width - IClickableMenu.borderWidth * 2), y + IClickableMenu.borderWidth + array[tabNumberFromName] / (width - IClickableMenu.borderWidth * 2) * 64 * 2 + 64, num, 128), bluePrint.name), bluePrint);
					array[tabNumberFromName] += num;
				}
			}
			blueprintButtons[4].Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(x + IClickableMenu.borderWidth + array[4] % (width - IClickableMenu.borderWidth * 2), y + IClickableMenu.borderWidth + array[4] / (width - IClickableMenu.borderWidth * 2) * 64 * 2 + 64, 64 + blueprintButtonMargin, 128), "Info Tool"), new BluePrint("Info Tool"));
			int num2 = 0;
			for (int k = 0; k < array.Length; k++)
			{
				if (array[k] > num2)
				{
					num2 = array[k];
				}
			}
			height = 128 + num2 / (width - IClickableMenu.borderWidth * 2) * 64 * 2 + IClickableMenu.borderWidth * 4 + heightOfDescriptionBox;
			buildingPlacementTiles = Game1.content.Load<Texture2D>("LooseSprites\\buildingPlacementTiles");
			tabs.Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + 64, yPositionOnScreen + tabYPositionRelativeToMenuY + 64, 64, 64), "Buildings"));
			tabs.Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + 64 + 64 + 4, yPositionOnScreen + tabYPositionRelativeToMenuY + 64, 64, 64), "Upgrades"));
			tabs.Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + 64 + 136, yPositionOnScreen + tabYPositionRelativeToMenuY + 64, 64, 64), "Decorations"));
			tabs.Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + 64 + 204, yPositionOnScreen + tabYPositionRelativeToMenuY + 64, 64, 64), "Demolish"));
			tabs.Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + 64 + 272, yPositionOnScreen + tabYPositionRelativeToMenuY + 64, 64, 64), "Animals"));
		}

		public int getTabNumberFromName(string name)
		{
			int result = -1;
			switch (name)
			{
			case "Buildings":
				result = 0;
				break;
			case "Upgrades":
				result = 1;
				break;
			case "Decorations":
				result = 2;
				break;
			case "Demolish":
				result = 3;
				break;
			case "Animals":
				result = 4;
				break;
			}
			return result;
		}

		public void changePosition(int x, int y)
		{
			int num = xPositionOnScreen - x;
			int num2 = yPositionOnScreen - y;
			xPositionOnScreen = x;
			yPositionOnScreen = y;
			foreach (Dictionary<ClickableComponent, BluePrint> blueprintButton in blueprintButtons)
			{
				foreach (ClickableComponent key in blueprintButton.Keys)
				{
					key.bounds.X += num;
					key.bounds.Y -= num2;
				}
			}
			foreach (ClickableComponent tab in tabs)
			{
				tab.bounds.X += num;
				tab.bounds.Y -= num2;
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (currentAnimal != null)
			{
				currentAnimal = null;
				placingStructure = true;
				queryingAnimals = true;
			}
			if (!placingStructure)
			{
				Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen, yPositionOnScreen, width, height);
				foreach (ClickableComponent key in blueprintButtons[currentTab].Keys)
				{
					if (!key.containsPoint(x, y))
					{
						continue;
					}
					if (key.name.Equals("Info Tool"))
					{
						placingStructure = true;
						queryingAnimals = true;
						Game1.playSound("smallSelect");
					}
					else if (blueprintButtons[currentTab][key].doesFarmerHaveEnoughResourcesToBuild())
					{
						structureForPlacement = blueprintButtons[currentTab][key];
						placingStructure = true;
						if (currentTab == 1)
						{
							upgrading = true;
						}
						Game1.playSound("smallSelect");
					}
					else
					{
						Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:BlueprintsMenu.cs.10002"), Color.Red, 3500f));
					}
					return;
				}
				foreach (ClickableComponent tab in tabs)
				{
					if (tab.containsPoint(x, y))
					{
						currentTab = getTabNumberFromName(tab.name);
						Game1.playSound("smallSelect");
						if (currentTab == 3)
						{
							placingStructure = true;
							demolishing = true;
						}
						return;
					}
				}
				if (!rectangle.Contains(x, y))
				{
					Game1.exitActiveMenu();
				}
			}
			else if (demolishing)
			{
				Building buildingAt = ((Farm)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64));
				if (buildingAt != null && ((Farm)Game1.getLocationFromName("Farm")).destroyStructure(buildingAt))
				{
					int groundLevelTile = (int)buildingAt.tileY + (int)buildingAt.tilesHigh;
					for (int i = 0; i < buildingAt.texture.Value.Bounds.Height / 64; i++)
					{
						Game1.createRadialDebris(Game1.currentLocation, buildingAt.textureName(), new Microsoft.Xna.Framework.Rectangle(buildingAt.texture.Value.Bounds.Center.X, buildingAt.texture.Value.Bounds.Center.Y, 4, 4), (int)buildingAt.tileX + Game1.random.Next(buildingAt.tilesWide), (int)buildingAt.tileY + (int)buildingAt.tilesHigh - i, Game1.random.Next(20, 45), groundLevelTile);
					}
					Game1.playSound("explosion");
					Utility.spreadAnimalsAround(buildingAt, (Farm)Game1.getLocationFromName("Farm"));
				}
				else
				{
					Game1.exitActiveMenu();
				}
			}
			else if (upgrading && Game1.currentLocation is Farm)
			{
				Building buildingAt2 = ((Farm)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64));
				if (buildingAt2 != null && structureForPlacement.name != null && buildingAt2.buildingType.Equals(structureForPlacement.nameOfBuildingToUpgrade))
				{
					buildingAt2.indoors.Value.map = Game1.game1.xTileContent.Load<Map>("Maps\\" + structureForPlacement.mapToWarpTo);
					buildingAt2.indoors.Value.name.Value = structureForPlacement.mapToWarpTo;
					buildingAt2.buildingType.Value = structureForPlacement.name;
					buildingAt2.resetTexture();
					if (buildingAt2.indoors.Value is AnimalHouse)
					{
						((AnimalHouse)(GameLocation)buildingAt2.indoors).resetPositionsOfAllAnimals();
					}
					Game1.playSound("axe");
					structureForPlacement.consumeResources();
					buildingAt2.color.Value = Color.White;
					Game1.exitActiveMenu();
				}
				else if (buildingAt2 != null)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:BlueprintsMenu.cs.10011"), Color.Red, 3500f));
				}
				else
				{
					Game1.exitActiveMenu();
				}
			}
			else
			{
				if (queryingAnimals)
				{
					if (!(Game1.currentLocation is Farm) && !(Game1.currentLocation is AnimalHouse))
					{
						return;
					}
					List<FarmAnimal> list = ((Game1.currentLocation is Farm) ? ((Farm)Game1.currentLocation).animals.Values.ToList() : ((AnimalHouse)Game1.currentLocation).animals.Values.ToList());
					{
						foreach (FarmAnimal item in list)
						{
							if (new Microsoft.Xna.Framework.Rectangle((int)item.Position.X, (int)item.Position.Y, item.Sprite.SourceRect.Width, item.Sprite.SourceRect.Height).Contains(Game1.viewport.X + Game1.getOldMouseX(), Game1.viewport.Y + Game1.getOldMouseY()))
							{
								positionOfAnimalWhenClicked = Game1.GlobalToLocal(Game1.viewport, item.Position);
								currentAnimal = item;
								queryingAnimals = false;
								placingStructure = false;
								if (item.sound.Value != null && !item.sound.Value.Equals(""))
								{
									Game1.playSound(item.sound);
								}
								break;
							}
						}
						return;
					}
				}
				if (!(Game1.currentLocation is Farm))
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:BlueprintsMenu.cs.10012"), Color.Red, 3500f));
				}
				else if (!structureForPlacement.doesFarmerHaveEnoughResourcesToBuild())
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:BlueprintsMenu.cs.10002"), Color.Red, 3500f));
				}
				else if (tryToBuild())
				{
					structureForPlacement.consumeResources();
					if (!structureForPlacement.blueprintType.Equals("Animals"))
					{
						Game1.playSound("axe");
					}
				}
				else
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:BlueprintsMenu.cs.10016"), Color.Red, 3500f));
				}
			}
		}

		public bool tryToBuild()
		{
			if (structureForPlacement.blueprintType.Equals("Animals"))
			{
				return ((Farm)Game1.getLocationFromName("Farm")).placeAnimal(structureForPlacement, new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64), serverCommand: false, Game1.player.UniqueMultiplayerID);
			}
			return ((Farm)Game1.getLocationFromName("Farm")).buildStructure(structureForPlacement, new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64), Game1.player);
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (currentAnimal != null)
			{
				currentAnimal = null;
				queryingAnimals = true;
				placingStructure = true;
			}
			else if (placingStructure)
			{
				placingStructure = false;
				queryingAnimals = false;
				upgrading = false;
				demolishing = false;
			}
			else
			{
				Game1.exitActiveMenu();
			}
		}

		public override void performHoverAction(int x, int y)
		{
			if (demolishing)
			{
				foreach (Building building in ((Farm)Game1.getLocationFromName("Farm")).buildings)
				{
					building.color.Value = Color.White;
				}
				Building buildingAt = ((Farm)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64));
				if (buildingAt != null)
				{
					buildingAt.color.Value = Color.Red * 0.8f;
				}
			}
			else if (upgrading)
			{
				foreach (Building building2 in ((Farm)Game1.getLocationFromName("Farm")).buildings)
				{
					building2.color.Value = Color.White;
				}
				Building buildingAt2 = ((Farm)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64));
				if (buildingAt2 != null && structureForPlacement.nameOfBuildingToUpgrade != null && structureForPlacement.nameOfBuildingToUpgrade.Equals(buildingAt2.buildingType))
				{
					buildingAt2.color.Value = Color.Green * 0.8f;
				}
				else if (buildingAt2 != null)
				{
					buildingAt2.color.Value = Color.Red * 0.8f;
				}
			}
			else
			{
				if (placingStructure)
				{
					return;
				}
				foreach (ClickableComponent tab in tabs)
				{
					if (tab.containsPoint(x, y))
					{
						hoverText = tab.name;
						return;
					}
				}
				hoverText = "";
				bool flag = false;
				foreach (ClickableComponent key in blueprintButtons[currentTab].Keys)
				{
					if (key.containsPoint(x, y))
					{
						key.scale = Math.Min(key.scale + 0.01f, 1.1f);
						hoveredItem = blueprintButtons[currentTab][key];
						flag = true;
					}
					else
					{
						key.scale = Math.Max(key.scale - 0.01f, 1f);
					}
				}
				if (!flag)
				{
					hoveredItem = null;
				}
			}
		}

		public int getTileSheetIndexForStructurePlacementTile(int x, int y)
		{
			if (x == structureForPlacement.humanDoor.X && y == structureForPlacement.humanDoor.Y)
			{
				return 2;
			}
			if (x == structureForPlacement.animalDoor.X && y == structureForPlacement.animalDoor.Y)
			{
				return 4;
			}
			return 0;
		}

		public override void draw(SpriteBatch b)
		{
			if (currentAnimal != null)
			{
				int num = (int)Math.Max(0f, Math.Min(positionOfAnimalWhenClicked.X - 256f + 32f, Game1.viewport.Width - 512));
				int num2 = (int)Math.Max(0f, Math.Min(Game1.viewport.Height - 256 - currentAnimal.frontBackSourceRect.Height, positionOfAnimalWhenClicked.Y - 256f - (float)currentAnimal.frontBackSourceRect.Height));
				Game1.drawDialogueBox(num, num2, 512, 352, speaker: false, drawOnlyBox: true);
				b.Draw(currentAnimal.Sprite.Texture, new Vector2(num + IClickableMenu.borderWidth + 96 - currentAnimal.frontBackSourceRect.Width / 2, num2 + IClickableMenu.borderWidth + 96), new Microsoft.Xna.Framework.Rectangle(0, 0, currentAnimal.frontBackSourceRect.Width, currentAnimal.frontBackSourceRect.Height), Color.White);
				float num3 = (float)(int)(byte)currentAnimal.fullness / 255f;
				float num4 = (float)(int)(byte)currentAnimal.happiness / 255f;
				string text = Game1.content.LoadString("Strings\\StringsFromCSFiles:BlueprintsMenu.cs.10026");
				string text2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:BlueprintsMenu.cs.10027");
				b.DrawString(Game1.dialogueFont, currentAnimal.displayName, new Vector2((float)(num + IClickableMenu.borderWidth + 96) - Game1.dialogueFont.MeasureString(currentAnimal.displayName).X / 2f, num2 + IClickableMenu.borderWidth + 96 + currentAnimal.frontBackSourceRect.Height + 8), Game1.textColor);
				b.DrawString(Game1.dialogueFont, text, new Vector2(num + IClickableMenu.borderWidth + 192, num2 + IClickableMenu.borderWidth + 96), Game1.textColor);
				b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(num + IClickableMenu.borderWidth + 192, num2 + IClickableMenu.borderWidth + 96 + (int)Game1.dialogueFont.MeasureString(text).Y + 8, 192, 16), Color.Gray);
				b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(num + IClickableMenu.borderWidth + 192, num2 + IClickableMenu.borderWidth + 96 + (int)Game1.dialogueFont.MeasureString(text).Y + 8, (int)(192f * num3), 16), (!((double)num3 > 0.33)) ? Color.Red : (((double)num3 > 0.66) ? Color.Green : Color.Goldenrod));
				b.DrawString(Game1.dialogueFont, text2, new Vector2(num + IClickableMenu.borderWidth + 192, (float)(num2 + IClickableMenu.borderWidth + 96) + Game1.dialogueFont.MeasureString(text).Y + 32f), Game1.textColor);
				b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(num + IClickableMenu.borderWidth + 192, num2 + IClickableMenu.borderWidth + 96 + (int)Game1.dialogueFont.MeasureString(text).Y + (int)Game1.dialogueFont.MeasureString(text2).Y + 32, 192, 16), Color.Gray);
				b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(num + IClickableMenu.borderWidth + 192, num2 + IClickableMenu.borderWidth + 96 + (int)Game1.dialogueFont.MeasureString(text).Y + (int)Game1.dialogueFont.MeasureString(text2).Y + 32, (int)(192f * num4), 16), (!((double)num4 > 0.33)) ? Color.Red : (((double)num4 > 0.66) ? Color.Green : Color.Goldenrod));
			}
			else if (!placingStructure)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
				Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height - heightOfDescriptionBox, speaker: false, drawOnlyBox: true);
				foreach (ClickableComponent tab in tabs)
				{
					int tilePosition = 0;
					switch (tab.name)
					{
					case "Buildings":
						tilePosition = 4;
						break;
					case "Upgrades":
						tilePosition = 5;
						break;
					case "Decorations":
						tilePosition = 7;
						break;
					case "Demolish":
						tilePosition = 6;
						break;
					case "Animals":
						tilePosition = 8;
						break;
					}
					b.Draw(Game1.mouseCursors, new Vector2(tab.bounds.X, tab.bounds.Y + ((currentTab == getTabNumberFromName(tab.name)) ? 8 : 0)), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, tilePosition), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0001f);
				}
				foreach (ClickableComponent key in blueprintButtons[currentTab].Keys)
				{
					Texture2D texture = blueprintButtons[currentTab][key].texture;
					Vector2 origin = (key.name.Equals("Info Tool") ? new Vector2(32f, 32f) : new Vector2(blueprintButtons[currentTab][key].sourceRectForMenuView.Center.X, blueprintButtons[currentTab][key].sourceRectForMenuView.Center.Y));
					b.Draw(texture, new Vector2(key.bounds.Center.X, key.bounds.Center.Y), blueprintButtons[currentTab][key].sourceRectForMenuView, Color.White, 0f, origin, 0.25f * key.scale + ((currentTab == 4) ? 0.75f : 0f), SpriteEffects.None, 0.9f);
				}
				Game1.drawWithBorder(hoverText, Color.Black, Color.White, new Vector2(Game1.getOldMouseX() + 64, Game1.getOldMouseY() + 64), 0f, 1f, 1f);
				Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen + (height - heightOfDescriptionBox) - IClickableMenu.borderWidth * 2, width, heightOfDescriptionBox, speaker: false, drawOnlyBox: true);
				_ = hoveredItem;
			}
			else
			{
				if (demolishing || upgrading || queryingAnimals)
				{
					return;
				}
				Vector2 vector = new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64);
				for (int i = 0; i < structureForPlacement.tilesHeight; i++)
				{
					for (int j = 0; j < structureForPlacement.tilesWidth; j++)
					{
						int num5 = getTileSheetIndexForStructurePlacementTile(j, i);
						Vector2 vector2 = new Vector2(vector.X + (float)j, vector.Y + (float)i);
						if (Game1.player.getTileLocation().Equals(vector2) || Game1.currentLocation.isTileOccupied(vector2) || !Game1.currentLocation.isTilePassable(new Location((int)vector2.X, (int)vector2.Y), Game1.viewport))
						{
							num5++;
						}
						b.Draw(buildingPlacementTiles, Game1.GlobalToLocal(Game1.viewport, vector2 * 64f), Game1.getSourceRectForStandardTileSheet(buildingPlacementTiles, num5), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.999f);
					}
				}
			}
		}
	}
}
