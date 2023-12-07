using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;
using StardewValley.Util;
using xTile;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class BuildableGameLocation : GameLocation
	{
		public readonly NetCollection<Building> buildings = new NetCollection<Building>();

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(buildings);
			buildings.InterpolationWait = false;
		}

		public BuildableGameLocation()
		{
		}

		public BuildableGameLocation(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			foreach (Building building in buildings)
			{
				building.dayUpdate(dayOfMonth);
			}
		}

		public override void cleanupBeforeSave()
		{
			foreach (Building building in buildings)
			{
				if (building.indoors.Value != null)
				{
					building.indoors.Value.cleanupBeforeSave();
				}
			}
		}

		public override bool performToolAction(Tool t, int tileX, int tileY)
		{
			foreach (Building building in buildings)
			{
				if (building.occupiesTile(new Vector2(tileX, tileY)))
				{
					building.performToolAction(t, tileX, tileY);
				}
			}
			return base.performToolAction(t, tileX, tileY);
		}

		public virtual void timeUpdate(int timeElapsed)
		{
			foreach (Building building in buildings)
			{
				if (building.indoors.Value == null || !(building.indoors.Value is AnimalHouse))
				{
					continue;
				}
				foreach (KeyValuePair<long, FarmAnimal> pair in ((AnimalHouse)(GameLocation)building.indoors).animals.Pairs)
				{
					pair.Value.updatePerTenMinutes(Game1.timeOfDay, building.indoors);
				}
			}
		}

		public Building getBuildingAt(Vector2 tile)
		{
			foreach (Building building in buildings)
			{
				if (!building.isTilePassable(tile))
				{
					return building;
				}
			}
			return null;
		}

		public Building getBuildingByName(string name)
		{
			foreach (Building building in buildings)
			{
				if (string.Equals(building.nameOfIndoors, name, StringComparison.Ordinal))
				{
					return building;
				}
			}
			return null;
		}

		public override bool leftClick(int x, int y, Farmer who)
		{
			foreach (Building building in buildings)
			{
				if (building.CanLeftClick(x, y))
				{
					building.leftClicked();
				}
			}
			return base.leftClick(x, y, who);
		}

		public bool destroyStructure(Vector2 tile)
		{
			Building buildingAt = getBuildingAt(tile);
			if (buildingAt != null)
			{
				buildingAt.performActionOnDemolition(this);
				buildings.Remove(buildingAt);
				return true;
			}
			return false;
		}

		public bool destroyStructure(Building b)
		{
			b.performActionOnDemolition(this);
			return buildings.Remove(b);
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false)
		{
			if (!glider && buildings.Count > 0)
			{
				Microsoft.Xna.Framework.Rectangle boundingBox = Game1.player.GetBoundingBox();
				FarmAnimal farmAnimal = character as FarmAnimal;
				bool flag = character is JunimoHarvester;
				bool flag2 = character is NPC;
				foreach (Building building in buildings)
				{
					if (!building.intersects(position) || (isFarmer && building.intersects(boundingBox)))
					{
						continue;
					}
					if (farmAnimal != null)
					{
						Microsoft.Xna.Framework.Rectangle rectForAnimalDoor = building.getRectForAnimalDoor();
						rectForAnimalDoor.Height += 64;
						if (rectForAnimalDoor.Contains(position) && building.buildingType.Value.Contains(farmAnimal.buildingTypeILiveIn.Value))
						{
							continue;
						}
					}
					else if (flag)
					{
						Microsoft.Xna.Framework.Rectangle rectForAnimalDoor2 = building.getRectForAnimalDoor();
						rectForAnimalDoor2.Height += 64;
						if (rectForAnimalDoor2.Contains(position))
						{
							continue;
						}
					}
					else if (flag2)
					{
						Microsoft.Xna.Framework.Rectangle rectForHumanDoor = building.getRectForHumanDoor();
						rectForHumanDoor.Height += 64;
						if (rectForHumanDoor.Contains(position))
						{
							continue;
						}
					}
					return true;
				}
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding, projectile, ignoreCharacterRequirement);
		}

		public override bool isActionableTile(int xTile, int yTile, Farmer who)
		{
			foreach (Building building in buildings)
			{
				if (building.isActionableTile(xTile, yTile, who))
				{
					return true;
				}
			}
			return base.isActionableTile(xTile, yTile, who);
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			foreach (Building building in buildings)
			{
				if (building.doAction(new Vector2(tileLocation.X, tileLocation.Y), who))
				{
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public override bool isTileOccupied(Vector2 tileLocation, string characterToIngore = "", bool ignoreAllCharacters = false)
		{
			foreach (Building building in buildings)
			{
				if (!building.isTilePassable(tileLocation))
				{
					return true;
				}
			}
			return base.isTileOccupied(tileLocation, characterToIngore, ignoreAllCharacters);
		}

		public override bool isTileOccupiedForPlacement(Vector2 tileLocation, Object toPlace = null)
		{
			foreach (Building building in buildings)
			{
				if (building.isTileOccupiedForPlacement(tileLocation, toPlace))
				{
					return true;
				}
			}
			return base.isTileOccupiedForPlacement(tileLocation, toPlace);
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
			foreach (Building building in buildings)
			{
				building.updateWhenFarmNotCurrentLocation(time);
			}
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (wasUpdated && Game1.gameMode != 0)
			{
				return;
			}
			base.UpdateWhenCurrentLocation(time);
			foreach (Building building in buildings)
			{
				building.Update(time);
			}
		}

		public override void drawFloorDecorations(SpriteBatch b)
		{
			int num = 1;
			Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X / 64 - num, Game1.viewport.Y / 64 - num, (int)Math.Ceiling((float)Game1.viewport.Width / 64f) + 2 * num, (int)Math.Ceiling((float)Game1.viewport.Height / 64f) + 3 + 2 * num);
			Microsoft.Xna.Framework.Rectangle rectangle = default(Microsoft.Xna.Framework.Rectangle);
			foreach (Building building in buildings)
			{
				int additionalTilePropertyRadius = building.GetAdditionalTilePropertyRadius();
				rectangle.X = (int)building.tileX - additionalTilePropertyRadius;
				rectangle.Width = (int)building.tilesWide + additionalTilePropertyRadius * 2;
				int num2 = (int)building.tileY + (int)building.tilesHigh + additionalTilePropertyRadius;
				rectangle.Height = num2 - (rectangle.Y = num2 - (int)Math.Ceiling((float)building.getSourceRect().Height * 4f / 64f) - additionalTilePropertyRadius);
				if (rectangle.Intersects(value))
				{
					building.drawBackground(b);
				}
			}
			base.drawFloorDecorations(b);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			int num = 1;
			Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X / 64 - num, Game1.viewport.Y / 64 - num, (int)Math.Ceiling((float)Game1.viewport.Width / 64f) + 2 * num, (int)Math.Ceiling((float)Game1.viewport.Height / 64f) + 3 + 2 * num);
			Microsoft.Xna.Framework.Rectangle rectangle = default(Microsoft.Xna.Framework.Rectangle);
			foreach (Building building in buildings)
			{
				int additionalTilePropertyRadius = building.GetAdditionalTilePropertyRadius();
				rectangle.X = (int)building.tileX - additionalTilePropertyRadius;
				rectangle.Width = (int)building.tilesWide + additionalTilePropertyRadius * 2;
				int num2 = (int)building.tileY + (int)building.tilesHigh + additionalTilePropertyRadius;
				rectangle.Height = num2 - (rectangle.Y = num2 - (int)Math.Ceiling((float)building.getSourceRect().Height * 4f / 64f) - additionalTilePropertyRadius);
				if (rectangle.Intersects(value))
				{
					building.draw(b);
				}
			}
		}

		public void tryToUpgrade(Building toUpgrade, BluePrint blueprint)
		{
			if (toUpgrade != null && blueprint.name != null && toUpgrade.buildingType.Equals(blueprint.nameOfBuildingToUpgrade))
			{
				if (toUpgrade.indoors.Value.farmers.Any())
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\Locations:BuildableLocation_CantUpgrade_SomeoneInside"), Color.Red, 3500f));
					return;
				}
				toUpgrade.indoors.Value.map = Game1.game1.xTileContent.Load<Map>("Maps\\" + blueprint.mapToWarpTo);
				toUpgrade.indoors.Value.name.Value = blueprint.mapToWarpTo;
				toUpgrade.indoors.Value.isStructure.Value = true;
				toUpgrade.buildingType.Value = blueprint.name;
				toUpgrade.resetTexture();
				if (toUpgrade.indoors.Value is AnimalHouse)
				{
					((AnimalHouse)(GameLocation)toUpgrade.indoors).resetPositionsOfAllAnimals();
				}
				playSound("axe");
				blueprint.consumeResources();
				toUpgrade.performActionOnUpgrade(this);
				toUpgrade.color.Value = Color.White;
				Game1.exitActiveMenu();
				Game1.multiplayer.globalChatInfoMessage("BuildingBuild", Game1.player.Name, Utility.AOrAn(blueprint.displayName), blueprint.displayName, Game1.player.farmName);
			}
			else if (toUpgrade != null)
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\Locations:BuildableLocation_CantUpgrade_IncorrectBuildingType"), Color.Red, 3500f));
			}
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			foreach (Building building in buildings)
			{
				building.resetLocalState();
			}
		}

		public bool isBuildingConstructed(string name)
		{
			foreach (Building building in buildings)
			{
				if (building.buildingType.Value.Equals(name) && (int)building.daysOfConstructionLeft <= 0)
				{
					return true;
				}
			}
			return false;
		}

		public int getNumberBuildingsConstructed(string name)
		{
			int num = 0;
			foreach (Building building in buildings)
			{
				if (building.buildingType.Value.Contains(name) && (int)building.daysOfConstructionLeft <= 0 && (int)building.daysUntilUpgrade <= 0)
				{
					num++;
				}
			}
			return num;
		}

		public bool isThereABuildingUnderConstruction()
		{
			foreach (Building building in buildings)
			{
				if ((int)building.daysOfConstructionLeft > 0 || (int)building.daysUntilUpgrade > 0)
				{
					return true;
				}
			}
			return false;
		}

		public Building getBuildingUnderConstruction()
		{
			foreach (Building building in buildings)
			{
				if ((int)building.daysOfConstructionLeft > 0 || (int)building.daysUntilUpgrade > 0)
				{
					return building;
				}
			}
			return null;
		}

		public bool buildStructure(Building b, Vector2 tileLocation, Farmer who, bool skipSafetyChecks = false)
		{
			if (!skipSafetyChecks)
			{
				for (int i = 0; i < (int)b.tilesHigh; i++)
				{
					for (int j = 0; j < (int)b.tilesWide; j++)
					{
						pokeTileForConstruction(new Vector2(tileLocation.X + (float)j, tileLocation.Y + (float)i));
					}
				}
				foreach (Point additionalPlacementTile in b.additionalPlacementTiles)
				{
					int x = additionalPlacementTile.X;
					int y = additionalPlacementTile.Y;
					pokeTileForConstruction(new Vector2(tileLocation.X + (float)x, tileLocation.Y + (float)y));
				}
				for (int k = 0; k < (int)b.tilesHigh; k++)
				{
					for (int l = 0; l < (int)b.tilesWide; l++)
					{
						Vector2 vector = new Vector2(tileLocation.X + (float)l, tileLocation.Y + (float)k);
						if (buildings.Contains(b) && b.occupiesTile(vector))
						{
							continue;
						}
						if (!isBuildable(vector))
						{
							return false;
						}
						foreach (Farmer farmer in farmers)
						{
							if (farmer.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(l * 64, k * 64, 64, 64)))
							{
								return false;
							}
						}
					}
				}
				foreach (Point additionalPlacementTile2 in b.additionalPlacementTiles)
				{
					int x2 = additionalPlacementTile2.X;
					int y2 = additionalPlacementTile2.Y;
					Vector2 vector2 = new Vector2(tileLocation.X + (float)x2, tileLocation.Y + (float)y2);
					if (buildings.Contains(b) && b.occupiesTile(vector2))
					{
						continue;
					}
					if (!isBuildable(vector2))
					{
						return false;
					}
					foreach (Farmer farmer2 in farmers)
					{
						if (farmer2.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(x2 * 64, y2 * 64, 64, 64)))
						{
							return false;
						}
					}
				}
				if (b.humanDoor.Value != new Point(-1, -1))
				{
					Vector2 vector3 = tileLocation + new Vector2(b.humanDoor.X, b.humanDoor.Y + 1);
					if ((!buildings.Contains(b) || !b.occupiesTile(vector3)) && !isBuildable(vector3) && !isPath(vector3))
					{
						return false;
					}
				}
				string text = b.isThereAnythingtoPreventConstruction(this, tileLocation);
				if (text != null)
				{
					Game1.addHUDMessage(new HUDMessage(text, Color.Red, 3500f));
					return false;
				}
			}
			b.tileX.Value = (int)tileLocation.X;
			b.tileY.Value = (int)tileLocation.Y;
			if (b.indoors.Value != null && b.indoors.Value is AnimalHouse)
			{
				foreach (long item in (b.indoors.Value as AnimalHouse).animalsThatLiveHere)
				{
					FarmAnimal animal = Utility.getAnimal(item);
					if (animal != null)
					{
						animal.homeLocation.Value = tileLocation;
						animal.home = b;
					}
					else if (animal == null && (b.indoors.Value as AnimalHouse).animals.ContainsKey(item))
					{
						animal = (b.indoors.Value as AnimalHouse).animals[item];
						animal.homeLocation.Value = tileLocation;
						animal.home = b;
					}
				}
			}
			if (b.indoors.Value != null)
			{
				foreach (Warp warp in b.indoors.Value.warps)
				{
					if (warp.TargetName == base.Name)
					{
						warp.TargetX = b.humanDoor.X + (int)b.tileX;
						warp.TargetY = b.humanDoor.Y + (int)b.tileY + 1;
					}
				}
			}
			if (!buildings.Contains(b))
			{
				buildings.Add(b);
			}
			Action<Vector2> action = delegate(Vector2 tile_location)
			{
				if (Utility.IsNormalObjectAtParentSheetIndex(getObjectAtTile((int)tile_location.X, (int)tile_location.Y), 590))
				{
					removeObject(tile_location, showDestroyedObject: false);
				}
			};
			for (int m = 0; m < (int)b.tilesHigh; m++)
			{
				for (int n = 0; n < (int)b.tilesWide; n++)
				{
					action(new Vector2(tileLocation.X + (float)n, tileLocation.Y + (float)m));
				}
			}
			foreach (Point additionalPlacementTile3 in b.additionalPlacementTiles)
			{
				int x3 = additionalPlacementTile3.X;
				int y3 = additionalPlacementTile3.Y;
				action(new Vector2(tileLocation.X + (float)x3, tileLocation.Y + (float)y3));
			}
			return true;
		}

		public override string doesTileHaveProperty(int xTile, int yTile, string propertyName, string layerName)
		{
			foreach (Building building in buildings)
			{
				int additionalTilePropertyRadius = building.GetAdditionalTilePropertyRadius();
				if (xTile >= (int)building.tileX - additionalTilePropertyRadius && xTile < (int)building.tileX + (int)building.tilesWide + additionalTilePropertyRadius && yTile >= (int)building.tileY - additionalTilePropertyRadius && yTile < (int)building.tileY + (int)building.tilesHigh + additionalTilePropertyRadius)
				{
					string property_value = null;
					if (building.doesTileHaveProperty(xTile, yTile, propertyName, layerName, ref property_value))
					{
						return property_value;
					}
				}
			}
			return base.doesTileHaveProperty(xTile, yTile, propertyName, layerName);
		}

		public override string doesTileHavePropertyNoNull(int xTile, int yTile, string propertyName, string layerName)
		{
			foreach (Building building in buildings)
			{
				int additionalTilePropertyRadius = building.GetAdditionalTilePropertyRadius();
				if (xTile < (int)building.tileX - additionalTilePropertyRadius || xTile >= (int)building.tileX + (int)building.tilesWide + additionalTilePropertyRadius || yTile < (int)building.tileY - additionalTilePropertyRadius || yTile >= (int)building.tileY + (int)building.tilesHigh + additionalTilePropertyRadius)
				{
					continue;
				}
				string property_value = null;
				if (building.doesTileHaveProperty(xTile, yTile, propertyName, layerName, ref property_value))
				{
					if (property_value == null)
					{
						return "";
					}
					return property_value;
				}
			}
			return base.doesTileHavePropertyNoNull(xTile, yTile, propertyName, layerName);
		}

		public virtual void pokeTileForConstruction(Vector2 tile)
		{
		}

		public bool isBuildable(Vector2 tileLocation)
		{
			if ((!Game1.player.getTileLocation().Equals(tileLocation) || !Game1.player.currentLocation.Equals(this)) && (!isTileOccupiedForPlacement(tileLocation) || Utility.IsNormalObjectAtParentSheetIndex(getObjectAtTile((int)tileLocation.X, (int)tileLocation.Y), 590)) && GetFurnitureAt(tileLocation) == null && isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y), Game1.viewport) && doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "NoFurniture", "Back") == null)
			{
				if (Game1.currentLocation.doesTileHavePropertyNoNull((int)tileLocation.X, (int)tileLocation.Y, "Buildable", "Back").ToLower().Equals("t") || Game1.currentLocation.doesTileHavePropertyNoNull((int)tileLocation.X, (int)tileLocation.Y, "Buildable", "Back").ToLower().Equals("true"))
				{
					return true;
				}
				if (Game1.currentLocation.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Diggable", "Back") != null && !Game1.currentLocation.doesTileHavePropertyNoNull((int)tileLocation.X, (int)tileLocation.Y, "Buildable", "Back").ToLower().Equals("f"))
				{
					return true;
				}
			}
			return false;
		}

		public bool isPath(Vector2 tileLocation)
		{
			Object value = null;
			TerrainFeature value2 = null;
			objects.TryGetValue(tileLocation, out value);
			terrainFeatures.TryGetValue(tileLocation, out value2);
			if (value2 != null && value2.isPassable())
			{
				return value?.isPassable() ?? true;
			}
			return false;
		}

		public override Point getWarpPointTo(string location, Character character = null)
		{
			foreach (Building building in buildings)
			{
				if (building.indoors.Value != null && (building.indoors.Value.Name == location || (building.indoors.Value.uniqueName.Value != null && building.indoors.Value.uniqueName.Value == location)))
				{
					return building.getPointForHumanDoor();
				}
			}
			return base.getWarpPointTo(location, character);
		}

		public override Warp isCollidingWithDoors(Microsoft.Xna.Framework.Rectangle position, Character character = null)
		{
			for (int i = 0; i < 4; i++)
			{
				Vector2 cornersOfThisRectangle = Utility.getCornersOfThisRectangle(ref position, i);
				Point point = new Point((int)cornersOfThisRectangle.X / 64, (int)cornersOfThisRectangle.Y / 64);
				foreach (Building building in buildings)
				{
					Point point2 = building.humanDoor;
					if (building.indoors.Value != null && point.Equals(building.getPointForHumanDoor()))
					{
						return getWarpFromDoor(building.getPointForHumanDoor(), character);
					}
				}
			}
			return base.isCollidingWithDoors(position, character);
		}

		public override Warp getWarpFromDoor(Point door, Character character = null)
		{
			foreach (Building building in buildings)
			{
				Point point = building.humanDoor;
				if (building.indoors.Value != null && door == building.getPointForHumanDoor())
				{
					return new Warp(door.X, door.Y, building.indoors.Value.uniqueName, building.indoors.Value.warps[0].X, building.indoors.Value.warps[0].Y - 1, flipFarmer: false);
				}
			}
			return base.getWarpFromDoor(door, character);
		}

		public bool buildStructure(BluePrint structureForPlacement, Vector2 tileLocation, Farmer who, bool magicalConstruction = false, bool skipSafetyChecks = false)
		{
			if (!skipSafetyChecks)
			{
				for (int i = 0; i < structureForPlacement.tilesHeight; i++)
				{
					for (int j = 0; j < structureForPlacement.tilesWidth; j++)
					{
						pokeTileForConstruction(new Vector2(tileLocation.X + (float)j, tileLocation.Y + (float)i));
					}
				}
				foreach (Point additionalPlacementTile in structureForPlacement.additionalPlacementTiles)
				{
					int x = additionalPlacementTile.X;
					int y = additionalPlacementTile.Y;
					pokeTileForConstruction(new Vector2(tileLocation.X + (float)x, tileLocation.Y + (float)y));
				}
				for (int k = 0; k < structureForPlacement.tilesHeight; k++)
				{
					for (int l = 0; l < structureForPlacement.tilesWidth; l++)
					{
						Vector2 tileLocation2 = new Vector2(tileLocation.X + (float)l, tileLocation.Y + (float)k);
						if (!isBuildable(tileLocation2))
						{
							return false;
						}
						foreach (Farmer farmer in farmers)
						{
							if (farmer.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(l * 64, k * 64, 64, 64)))
							{
								return false;
							}
						}
					}
				}
				foreach (Point additionalPlacementTile2 in structureForPlacement.additionalPlacementTiles)
				{
					int x2 = additionalPlacementTile2.X;
					int y2 = additionalPlacementTile2.Y;
					Vector2 tileLocation3 = new Vector2(tileLocation.X + (float)x2, tileLocation.Y + (float)y2);
					if (!isBuildable(tileLocation3))
					{
						return false;
					}
					foreach (Farmer farmer2 in farmers)
					{
						if (farmer2.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(x2 * 64, y2 * 64, 64, 64)))
						{
							return false;
						}
					}
				}
				if (structureForPlacement.humanDoor != new Point(-1, -1))
				{
					Vector2 tileLocation4 = tileLocation + new Vector2(structureForPlacement.humanDoor.X, structureForPlacement.humanDoor.Y + 1);
					if (!isBuildable(tileLocation4) && !isPath(tileLocation4))
					{
						return false;
					}
				}
			}
			string text = structureForPlacement.name;
			Building building;
			char c;
			if (text != null)
			{
				switch (text.Length)
				{
				case 4:
					break;
				case 8:
					goto IL_02fb;
				case 11:
					goto IL_031c;
				case 10:
					goto IL_033d;
				case 6:
					goto IL_035e;
				case 12:
					goto IL_042b;
				case 9:
					goto IL_043b;
				default:
					goto IL_04a0;
				}
				c = text[0];
				if (c != 'B')
				{
					if (c != 'C')
					{
						if (c == 'M' && text == "Mill")
						{
							building = new Mill(structureForPlacement, tileLocation);
							goto IL_04a8;
						}
					}
					else if (text == "Coop")
					{
						goto IL_045a;
					}
				}
				else if (text == "Barn")
				{
					goto IL_0464;
				}
			}
			goto IL_04a0;
			IL_031c:
			c = text[7];
			if (c != 'B')
			{
				if (c == 'C' && text == "Deluxe Coop")
				{
					goto IL_045a;
				}
			}
			else if (text == "Deluxe Barn")
			{
				goto IL_0464;
			}
			goto IL_04a0;
			IL_042b:
			if (!(text == "Shipping Bin"))
			{
				goto IL_04a0;
			}
			building = new ShippingBin(structureForPlacement, tileLocation);
			goto IL_04a8;
			IL_043b:
			if (!(text == "Fish Pond"))
			{
				goto IL_04a0;
			}
			building = new FishPond(structureForPlacement, tileLocation);
			goto IL_04a8;
			IL_045a:
			building = new Coop(structureForPlacement, tileLocation);
			goto IL_04a8;
			IL_035e:
			if (!(text == "Stable"))
			{
				goto IL_04a0;
			}
			building = new Stable(GuidHelper.NewGuid(), structureForPlacement, tileLocation);
			goto IL_04a8;
			IL_033d:
			c = text[0];
			if (c != 'G')
			{
				if (c != 'J' || !(text == "Junimo Hut"))
				{
					goto IL_04a0;
				}
				building = new JunimoHut(structureForPlacement, tileLocation);
			}
			else
			{
				if (!(text == "Greenhouse"))
				{
					goto IL_04a0;
				}
				building = new GreenhouseBuilding(structureForPlacement, tileLocation);
			}
			goto IL_04a8;
			IL_04a8:
			building.owner.Value = who.UniqueMultiplayerID;
			if (!skipSafetyChecks)
			{
				string text2 = building.isThereAnythingtoPreventConstruction(this, tileLocation);
				if (text2 != null)
				{
					Game1.addHUDMessage(new HUDMessage(text2, Color.Red, 3500f));
					return false;
				}
			}
			for (int m = 0; m < structureForPlacement.tilesHeight; m++)
			{
				for (int n = 0; n < structureForPlacement.tilesWidth; n++)
				{
					Vector2 key = new Vector2(tileLocation.X + (float)n, tileLocation.Y + (float)m);
					terrainFeatures.Remove(key);
				}
			}
			buildings.Add(building);
			building.performActionOnConstruction(this);
			if (magicalConstruction)
			{
				Game1.multiplayer.globalChatInfoMessage("BuildingMagicBuild", Game1.player.Name, Utility.AOrAn(structureForPlacement.displayName), structureForPlacement.displayName, Game1.player.farmName);
			}
			else
			{
				Game1.multiplayer.globalChatInfoMessage("BuildingBuild", Game1.player.Name, Utility.AOrAn(structureForPlacement.displayName), structureForPlacement.displayName, Game1.player.farmName);
			}
			return true;
			IL_04a0:
			building = new Building(structureForPlacement, tileLocation);
			goto IL_04a8;
			IL_0464:
			building = new Barn(structureForPlacement, tileLocation);
			goto IL_04a8;
			IL_02fb:
			c = text[4];
			if (c != 'B')
			{
				if (c == 'C' && text == "Big Coop")
				{
					goto IL_045a;
				}
			}
			else if (text == "Big Barn")
			{
				goto IL_0464;
			}
			goto IL_04a0;
		}
	}
}
