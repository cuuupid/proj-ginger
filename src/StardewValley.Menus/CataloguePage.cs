using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Buildings;
using StardewValley.Locations;
using xTile.Dimensions;

namespace StardewValley.Menus
{
	public class CataloguePage : IClickableMenu
	{
		public static int widthToMoveActiveTab = 8;

		public static int blueprintButtonMargin = 32;

		public const int buildingsTab = 0;

		public const int upgradesTab = 1;

		public const int animalsTab = 2;

		public const int demolishTab = 3;

		public const int numberOfTabs = 4;

		private string descriptionText = "";

		private string hoverText = "";

		private InventoryMenu inventory;

		private Item heldItem;

		private int currentTab;

		private BluePrint hoveredItem;

		private List<ClickableTextureComponent> sideTabs = new List<ClickableTextureComponent>();

		private List<Dictionary<ClickableComponent, BluePrint>> blueprintButtons = new List<Dictionary<ClickableComponent, BluePrint>>();

		private bool demolishing;

		private bool upgrading;

		private bool placingStructure;

		private BluePrint structureForPlacement;

		private GameMenu parent;

		private Texture2D buildingPlacementTiles;

		public CataloguePage(int x, int y, int width, int height, GameMenu parent)
			: base(x, y, width, height)
		{
			this.parent = parent;
			buildingPlacementTiles = Game1.content.Load<Texture2D>("LooseSprites\\buildingPlacementTiles");
			widthToMoveActiveTab = 8;
			blueprintButtonMargin = 32;
			inventory = new InventoryMenu(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 320 - 16, playerInventory: false);
			sideTabs.Add(new ClickableTextureComponent("", new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen - 48 + widthToMoveActiveTab, yPositionOnScreen + 128, 64, 64), "", "Buildings", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 4), 1f));
			sideTabs.Add(new ClickableTextureComponent("", new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 192, 64, 64), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:CataloguePage.cs.10138"), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 5), 1f));
			sideTabs.Add(new ClickableTextureComponent("", new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 256, 64, 64), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:CataloguePage.cs.10139"), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 8), 1f));
			sideTabs.Add(new ClickableTextureComponent("", new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 320, 64, 64), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:CataloguePage.cs.10140"), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 6), 1f));
			for (int i = 0; i < 4; i++)
			{
				blueprintButtons.Add(new Dictionary<ClickableComponent, BluePrint>());
			}
			int num = 512;
			int[] array = new int[4];
			for (int j = 0; j < Game1.player.blueprints.Count; j++)
			{
				BluePrint bluePrint = new BluePrint(Game1.player.blueprints[j]);
				if (canPlaceThisBuildingOnTheCurrentMap(bluePrint, Game1.currentLocation))
				{
					bluePrint.canBuildOnCurrentMap = true;
				}
				int tabNumberFromName = getTabNumberFromName(bluePrint.blueprintType);
				if (bluePrint.blueprintType != null)
				{
					int num2 = (int)((float)Math.Max(bluePrint.tilesWidth, 4) / 4f * 64f) + blueprintButtonMargin;
					if (array[tabNumberFromName] % (num - IClickableMenu.borderWidth * 2) + num2 > num - IClickableMenu.borderWidth * 2)
					{
						array[tabNumberFromName] += num - IClickableMenu.borderWidth * 2 - array[tabNumberFromName] % (num - IClickableMenu.borderWidth * 2);
					}
					blueprintButtons[Math.Min(3, tabNumberFromName)].Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(x + IClickableMenu.borderWidth + array[tabNumberFromName] % (num - IClickableMenu.borderWidth * 2), y + IClickableMenu.borderWidth + array[tabNumberFromName] / (num - IClickableMenu.borderWidth * 2) * 64 * 2 + 64, num2, 128), bluePrint.name), bluePrint);
					array[tabNumberFromName] += num2;
				}
			}
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
			case "Demolish":
				result = 3;
				break;
			case "Animals":
				result = 2;
				break;
			}
			return result;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!placingStructure)
			{
				heldItem = inventory.leftClick(x, y, heldItem);
				for (int i = 0; i < sideTabs.Count; i++)
				{
					if (sideTabs[i].containsPoint(x, y) && currentTab != i)
					{
						Game1.playSound("smallSelect");
						if (i == 3)
						{
							placingStructure = true;
							demolishing = true;
							parent.invisible = true;
						}
						else
						{
							sideTabs[currentTab].bounds.X -= widthToMoveActiveTab;
							currentTab = i;
							sideTabs[i].bounds.X += widthToMoveActiveTab;
						}
					}
				}
				{
					foreach (ClickableComponent key in blueprintButtons[currentTab].Keys)
					{
						if (!key.containsPoint(x, y))
						{
							continue;
						}
						if (blueprintButtons[currentTab][key].doesFarmerHaveEnoughResourcesToBuild())
						{
							structureForPlacement = blueprintButtons[currentTab][key];
							placingStructure = true;
							parent.invisible = true;
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
						break;
					}
					return;
				}
			}
			if (demolishing)
			{
				if (!(Game1.currentLocation is Farm))
				{
					return;
				}
				if (Game1.IsClient)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:CataloguePage.cs.10148"), Color.Red, 3500f));
					return;
				}
				Vector2 tile = new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64);
				Building buildingAt = ((Farm)Game1.currentLocation).getBuildingAt(tile);
				if (Game1.IsMultiplayer && buildingAt != null && buildingAt.indoors.Value.farmers.Any())
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:CataloguePage.cs.10149"), Color.Red, 3500f));
				}
				else if (buildingAt != null && ((Farm)Game1.currentLocation).destroyStructure(buildingAt))
				{
					int groundLevelTile = (int)buildingAt.tileY + (int)buildingAt.tilesHigh;
					for (int j = 0; j < buildingAt.texture.Value.Bounds.Height / 64; j++)
					{
						Game1.createRadialDebris(Game1.currentLocation, buildingAt.textureName(), new Microsoft.Xna.Framework.Rectangle(buildingAt.texture.Value.Bounds.Center.X, buildingAt.texture.Value.Bounds.Center.Y, 4, 4), (int)buildingAt.tileX + Game1.random.Next(buildingAt.tilesWide), (int)buildingAt.tileY + (int)buildingAt.tilesHigh - j, Game1.random.Next(20, 45), groundLevelTile);
					}
					Game1.playSound("explosion");
					Utility.spreadAnimalsAround(buildingAt, (Farm)Game1.currentLocation);
				}
				else
				{
					parent.invisible = false;
					placingStructure = false;
					demolishing = false;
				}
			}
			else if (upgrading && Game1.currentLocation is Farm)
			{
				(Game1.currentLocation as Farm).tryToUpgrade(((Farm)Game1.getLocationFromName("Farm")).getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64)), structureForPlacement);
			}
			else if (!canPlaceThisBuildingOnTheCurrentMap(structureForPlacement, Game1.currentLocation))
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:CataloguePage.cs.10152"), Color.Red, 3500f));
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
			else if (!Game1.IsClient)
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:BlueprintsMenu.cs.10016"), Color.Red, 3500f));
			}
		}

		public static bool canPlaceThisBuildingOnTheCurrentMap(BluePrint structureToPlace, GameLocation map)
		{
			return true;
		}

		private bool tryToBuild()
		{
			if (structureForPlacement.blueprintType.Equals("Animals"))
			{
				return ((Farm)Game1.getLocationFromName("Farm")).placeAnimal(structureForPlacement, new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64), serverCommand: false, Game1.player.UniqueMultiplayerID);
			}
			return (Game1.currentLocation as BuildableGameLocation).buildStructure(structureForPlacement, new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64), Game1.player);
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (placingStructure)
			{
				placingStructure = false;
				upgrading = false;
				demolishing = false;
				parent.invisible = false;
			}
			else
			{
				heldItem = inventory.rightClick(x, y, heldItem);
			}
		}

		public override bool readyToClose()
		{
			if (heldItem == null)
			{
				return !placingStructure;
			}
			return false;
		}

		public override void performHoverAction(int x, int y)
		{
			descriptionText = "";
			hoverText = "";
			foreach (ClickableTextureComponent sideTab in sideTabs)
			{
				if (sideTab.containsPoint(x, y))
				{
					hoverText = sideTab.hoverText;
					return;
				}
			}
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
			if (!flag)
			{
				hoveredItem = null;
			}
		}

		private int getTileSheetIndexForStructurePlacementTile(int x, int y)
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

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && placingStructure)
			{
				placingStructure = false;
				upgrading = false;
				demolishing = false;
				parent.invisible = false;
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (!placingStructure)
			{
				foreach (ClickableTextureComponent sideTab in sideTabs)
				{
					sideTab.draw(b);
				}
				drawHorizontalPartition(b, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256);
				drawVerticalUpperIntersectingPartition(b, xPositionOnScreen + 576, 328);
				inventory.draw(b);
				foreach (ClickableComponent key in blueprintButtons[currentTab].Keys)
				{
					Texture2D texture = blueprintButtons[currentTab][key].texture;
					b.Draw(origin: new Vector2(blueprintButtons[currentTab][key].sourceRectForMenuView.Center.X, blueprintButtons[currentTab][key].sourceRectForMenuView.Center.Y), texture: texture, position: new Vector2(key.bounds.Center.X, key.bounds.Center.Y), sourceRectangle: blueprintButtons[currentTab][key].sourceRectForMenuView, color: blueprintButtons[currentTab][key].canBuildOnCurrentMap ? Color.White : (Color.Gray * 0.8f), rotation: 0f, scale: 1f * key.scale + ((currentTab == 2) ? 0.75f : 0f), effects: SpriteEffects.None, layerDepth: 0.9f);
				}
				if (hoveredItem != null)
				{
					hoveredItem.drawDescription(b, xPositionOnScreen + 576 + 42, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 32, 224);
				}
				if (heldItem != null)
				{
					heldItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);
				}
				if (!hoverText.Equals(""))
				{
					IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
				}
			}
			else
			{
				if (demolishing || upgrading)
				{
					return;
				}
				Vector2 vector = new Vector2((Game1.viewport.X + Game1.getOldMouseX()) / 64, (Game1.viewport.Y + Game1.getOldMouseY()) / 64);
				for (int i = 0; i < structureForPlacement.tilesHeight; i++)
				{
					for (int j = 0; j < structureForPlacement.tilesWidth; j++)
					{
						int num = getTileSheetIndexForStructurePlacementTile(j, i);
						Vector2 vector2 = new Vector2(vector.X + (float)j, vector.Y + (float)i);
						if (Game1.player.getTileLocation().Equals(vector2) || Game1.currentLocation.isTileOccupied(vector2) || !Game1.currentLocation.isTilePassable(new Location((int)vector2.X, (int)vector2.Y), Game1.viewport))
						{
							num++;
						}
						b.Draw(buildingPlacementTiles, Game1.GlobalToLocal(Game1.viewport, vector2 * 64f), Game1.getSourceRectForStandardTileSheet(buildingPlacementTiles, num), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.999f);
					}
				}
			}
		}
	}
}
