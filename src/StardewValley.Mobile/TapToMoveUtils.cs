using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Minigames;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley.Mobile
{
	public class TapToMoveUtils
	{
		public static GameLocation gameLocation
		{
			get
			{
				if (Game1.currentMinigame != null && Game1.currentMinigame is FishingGame)
				{
					return ((FishingGame)Game1.currentMinigame).location;
				}
				return Game1.currentLocation;
			}
		}

		public static bool inMiniGameWhereWeDontWantTaps
		{
			get
			{
				if (Game1.currentMinigame != null)
				{
					if (!(Game1.currentMinigame is AbigailGame) && !(Game1.currentMinigame is FantasyBoardGame) && !(Game1.currentMinigame is GrandpaStory) && !(Game1.currentMinigame is HaleyCowPictures) && !(Game1.currentMinigame is MineCart) && !(Game1.currentMinigame is PlaneFlyBy))
					{
						return Game1.currentMinigame is RobotBlastoff;
					}
					return true;
				}
				return false;
			}
		}

		public static Vector2 PlayerOffsetPosition => new Vector2(Game1.player.position.X + 32f, Game1.player.position.Y + 32f);

		public static Vector2 PlayerPositionOnScreen => new Vector2(Game1.player.position.X + 32f - (float)Game1.viewport.X, Game1.player.position.Y + 32f - (float)Game1.viewport.Y);

		public static float WarpRange
		{
			get
			{
				if (Game1.currentLocation != null && ((bool)Game1.currentLocation.isOutdoors || Game1.currentLocation is BathHousePool))
				{
					return 128f;
				}
				return 96f;
			}
		}

		public static bool ContainsTravellingCart(int pointX, int pointY)
		{
			if (gameLocation is Forest)
			{
				Forest forest = (Forest)gameLocation;
				if (forest.travelingMerchantBounds != null)
				{
					foreach (Microsoft.Xna.Framework.Rectangle travelingMerchantBound in forest.travelingMerchantBounds)
					{
						if (travelingMerchantBound.Contains(pointX, pointY))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public static bool ContainsTravellingDesertShop(int pointX, int pointY)
		{
			if (gameLocation is Desert)
			{
				return ((Desert)gameLocation).desertMerchantBounds.Contains(pointX, pointY);
			}
			return false;
		}

		public static bool ContainsCinemaDoor(int tileX, int tileY)
		{
			if (gameLocation is Town && Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater") && (tileX == 52 || tileX == 53) && tileY >= 18 && tileY <= 19)
			{
				return true;
			}
			return false;
		}

		public static bool ContainsCinemaTicketOffice(int tileX, int tileY)
		{
			if (gameLocation is Town && Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater") && tileX >= 54 && tileX <= 56 && tileY >= 19 && tileY <= 20)
			{
				return true;
			}
			return false;
		}

		public static bool SelectTool(string toolName)
		{
			for (int i = 0; i < Game1.player.items.Count; i++)
			{
				if (Game1.player.items[i] != null && Game1.player.items[i].Name.Contains(toolName))
				{
					Game1.player.CurrentToolIndex = i;
					return true;
				}
			}
			return false;
		}

		public static bool PlayerHasTool(string toolName)
		{
			for (int i = 0; i < Game1.player.items.Count; i++)
			{
				if (Game1.player.items[i] != null && Game1.player.items[i].Name.Contains(toolName))
				{
					return true;
				}
			}
			return false;
		}

		public static MeleeWeapon getBestAvailableWeapon()
		{
			MeleeWeapon meleeWeapon = null;
			for (int i = 0; i < Game1.player.items.Count; i++)
			{
				if (Game1.player.items[i] != null && Game1.player.items[i] is MeleeWeapon)
				{
					if (meleeWeapon == null)
					{
						meleeWeapon = Game1.player.items[i] as MeleeWeapon;
					}
					else if ((Game1.player.items[i] as MeleeWeapon).getItemLevel() > meleeWeapon.getItemLevel() || meleeWeapon.Name == "Scythe")
					{
						meleeWeapon = Game1.player.items[i] as MeleeWeapon;
					}
				}
			}
			return meleeWeapon;
		}

		public static Item FetchItemInInventoryByName(string itemName)
		{
			for (int i = 0; i < Game1.player.items.Count; i++)
			{
				if (Game1.player.items[i] != null && Game1.player.items[i].Name.Contains(itemName))
				{
					return Game1.player.items[i];
				}
			}
			return null;
		}

		public static bool InWarpRange(Vector2 clickPoint)
		{
			if (gameLocation.ignoreWarps)
			{
				return false;
			}
			foreach (Warp warp in gameLocation.warps)
			{
				Vector2 vector = new Vector2(warp.X * 64, warp.Y * 64);
				float num = Vector2.Distance(vector + new Vector2(32f), clickPoint);
				float num2 = Vector2.Distance(vector, Game1.player.position);
				if (num < WarpRange && num2 < WarpRange)
				{
					return true;
				}
			}
			return false;
		}

		public static bool NodeIsWarp(AStarNode aStarNode)
		{
			if (aStarNode == null)
			{
				return false;
			}
			if (gameLocation.ignoreWarps)
			{
				return false;
			}
			Vector2 nodeCenterOnMap = aStarNode.NodeCenterOnMap;
			foreach (Warp warp in gameLocation.warps)
			{
				Vector2 value = new Vector2(warp.X * 64, warp.Y * 64);
				float num = Vector2.Distance(value, nodeCenterOnMap);
				if (num < WarpRange)
				{
					return true;
				}
			}
			return false;
		}

		public static bool WarpIfInRange(Vector2 clickPoint)
		{
			if (gameLocation.ignoreWarps)
			{
				return false;
			}
			if (!Game1.player.canMove)
			{
				return false;
			}
			foreach (Warp warp2 in gameLocation.warps)
			{
				Warp warp = warp2;
				if (warp.TargetName == "VolcanoEntrance")
				{
					warp = new Warp(warp.X, warp.Y, "VolcanoDungeon0", warp.TargetX, warp.TargetY, flipFarmer: false);
				}
				Vector2 vector = new Vector2(warp.X * 64, warp.Y * 64);
				float num = Vector2.Distance(vector + new Vector2(32f), clickPoint);
				float num2 = Vector2.Distance(vector, Game1.player.position);
				if (warp2.TargetName == "IslandSouthEast" && gameLocation is IslandSouth islandSouth && !islandSouth.resortRestored.Value && num2 > 125f)
				{
					return false;
				}
				if ((!(gameLocation is BusStop) || !(warp2.TargetName == "Desert")) && num < WarpRange && num2 < WarpRange)
				{
					Game1.player.warpFarmer(warp);
					return true;
				}
			}
			return false;
		}

		public static bool NpcAtWarpOrDoor(NPC npc, GameLocation gameLocation)
		{
			Warp warp = gameLocation.isCollidingWithWarp(npc.GetBoundingBox(), npc);
			if (warp != null)
			{
				return true;
			}
			PropertyValue value = null;
			gameLocation.map.GetLayer("Buildings").PickTile(npc.nextPositionPoint(), Game1.viewport.Size)?.Properties.TryGetValue("Action", out value);
			if (value != null)
			{
				return true;
			}
			return false;
		}

		public static int ConvertWalkDirection(WalkDirection walkDirection)
		{
			return walkDirection switch
			{
				WalkDirection.Up => 0, 
				WalkDirection.Down => 2, 
				WalkDirection.Left => 3, 
				WalkDirection.Right => 1, 
				_ => -1, 
			};
		}

		public static WalkDirection WalkDirectionForAngle(float angleDegrees)
		{
			if ((double)angleDegrees >= -22.5 && (double)angleDegrees < 22.5)
			{
				return WalkDirection.Right;
			}
			if ((double)angleDegrees >= 22.5 && (double)angleDegrees < 67.5)
			{
				return WalkDirection.DownRight;
			}
			if ((double)angleDegrees >= 67.5 && (double)angleDegrees < 112.5)
			{
				return WalkDirection.Down;
			}
			if ((double)angleDegrees >= 112.5 && (double)angleDegrees < 157.5)
			{
				return WalkDirection.DownLeft;
			}
			if ((double)angleDegrees < -112.5 && (double)angleDegrees >= -157.5)
			{
				return WalkDirection.UpLeft;
			}
			if ((double)angleDegrees < -22.5 && (double)angleDegrees >= -67.5)
			{
				return WalkDirection.UpRight;
			}
			if ((double)angleDegrees < -67.5 && (double)angleDegrees >= -112.5)
			{
				return WalkDirection.Up;
			}
			return WalkDirection.Left;
		}

		public static WalkDirection WalkDirectionForAngleJustDiagonals(float angleDegrees)
		{
			if (angleDegrees >= 0f && angleDegrees < 90f)
			{
				return WalkDirection.DownRight;
			}
			if (angleDegrees >= 90f && angleDegrees <= 180f)
			{
				return WalkDirection.DownLeft;
			}
			if (angleDegrees <= -90f && angleDegrees >= -180f)
			{
				return WalkDirection.UpLeft;
			}
			return WalkDirection.UpRight;
		}

		public static int FaceDirectionForAngle(float angleDegrees)
		{
			if (angleDegrees > -135f && angleDegrees <= -45f)
			{
				return 0;
			}
			if (angleDegrees >= 45f && angleDegrees <= 135f)
			{
				return 2;
			}
			if (angleDegrees >= -45f && angleDegrees <= 45f)
			{
				return 1;
			}
			return 3;
		}

		public static bool WalkDirectionsAgree(WalkDirection walkDirectionA, WalkDirection walkDirectionB)
		{
			switch (walkDirectionA)
			{
			case WalkDirection.Up:
				if (walkDirectionB != WalkDirection.Up && walkDirectionB != WalkDirection.UpLeft)
				{
					return walkDirectionB == WalkDirection.UpRight;
				}
				return true;
			case WalkDirection.Down:
				if (walkDirectionB != WalkDirection.Down && walkDirectionB != WalkDirection.DownLeft)
				{
					return walkDirectionB == WalkDirection.DownRight;
				}
				return true;
			case WalkDirection.Left:
				if (walkDirectionB != WalkDirection.Left && walkDirectionB != WalkDirection.DownLeft)
				{
					return walkDirectionB == WalkDirection.UpLeft;
				}
				return true;
			case WalkDirection.Right:
				if (walkDirectionB != WalkDirection.Right && walkDirectionB != WalkDirection.DownRight)
				{
					return walkDirectionB == WalkDirection.UpRight;
				}
				return true;
			case WalkDirection.UpLeft:
				if (walkDirectionB != WalkDirection.Up && walkDirectionB != WalkDirection.UpLeft)
				{
					return walkDirectionB == WalkDirection.Left;
				}
				return true;
			case WalkDirection.UpRight:
				if (walkDirectionB != WalkDirection.Up && walkDirectionB != WalkDirection.UpRight)
				{
					return walkDirectionB == WalkDirection.Right;
				}
				return true;
			case WalkDirection.DownLeft:
				if (walkDirectionB != WalkDirection.Down && walkDirectionB != WalkDirection.DownLeft)
				{
					return walkDirectionB == WalkDirection.Left;
				}
				return true;
			case WalkDirection.DownRight:
				if (walkDirectionB != WalkDirection.Down && walkDirectionB != WalkDirection.DownRight)
				{
					return walkDirectionB == WalkDirection.Right;
				}
				return true;
			default:
				return false;
			}
		}

		public static bool ItemCanBePlaced(Item item, Vector2 tileLocation)
		{
			if (item.canBePlacedHere(gameLocation, tileLocation))
			{
				if (!((Object)item).isPassable())
				{
					foreach (Farmer farmer in gameLocation.farmers)
					{
						if (farmer.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64)))
						{
							return false;
						}
					}
				}
				if ((gameLocation.isTilePlaceable(tileLocation, item) && item.isPlaceable() && (((Object)item).isPassable() || !new Microsoft.Xna.Framework.Rectangle((int)(tileLocation.X * 64f), (int)(tileLocation.Y * 64f), 64, 64).Intersects(Game1.player.GetBoundingBox()))) || ((int)((Object)item).category == -74 && gameLocation.terrainFeatures.ContainsKey(tileLocation) && gameLocation.terrainFeatures[tileLocation].GetType() == typeof(HoeDirt) && ((HoeDirt)gameLocation.terrainFeatures[tileLocation]).canPlantThisSeedHere((item as Object).ParentSheetIndex, (int)tileLocation.X, (int)tileLocation.Y)))
				{
					return true;
				}
			}
			return false;
		}

		public static bool NodeContainsFurniture(AStarNode aStarNode)
		{
			if (aStarNode == null)
			{
				return false;
			}
			Microsoft.Xna.Framework.Rectangle rect = aStarNode.rect;
			foreach (Furniture item in gameLocation.furniture)
			{
				if (item.getBoundingBox(item.tileLocation).Intersects(rect))
				{
					return true;
				}
			}
			return false;
		}

		public static bool NodeContainsMusicBlock(AStarNode aStarNode)
		{
			if (gameLocation is DecoratableLocation)
			{
				DecoratableLocation decoratableLocation = (DecoratableLocation)gameLocation;
				if (decoratableLocation.objects.TryGetValue(new Vector2(aStarNode.x, aStarNode.y), out var value) && ((int)value.parentSheetIndex == 463 || (int)value.parentSheetIndex == 464))
				{
					return true;
				}
			}
			return false;
		}

		public static Furniture GetFurnitureClickedOn(int clickPointX, int clickPointY)
		{
			foreach (Furniture item in gameLocation.furniture)
			{
				if ((int)item.furniture_type != 12 && item.getBoundingBox(item.tileLocation).Contains(clickPointX, clickPointY))
				{
					return item;
				}
			}
			foreach (Furniture item2 in gameLocation.furniture)
			{
				if ((int)item2.furniture_type == 12 && item2.getBoundingBox(item2.tileLocation).Contains(clickPointX, clickPointY))
				{
					return item2;
				}
			}
			return null;
		}

		public static bool NodeContainsHousePlant(AStarNode aStarNode)
		{
			Game1.currentLocation.objects.TryGetValue(new Vector2(aStarNode.x, aStarNode.y), out var value);
			if (value != null && value.Name == "House Plant")
			{
				return true;
			}
			return false;
		}

		public static Object GetHousePlant(AStarNode aStarNode)
		{
			Game1.currentLocation.objects.TryGetValue(new Vector2(aStarNode.x, aStarNode.y), out var value);
			if (value != null && value.Name == "House Plant")
			{
				return value;
			}
			return null;
		}

		public static bool HoeSelectedAndTileHoeable(Vector2 tile)
		{
			if (Game1.player.CurrentTool is Hoe && gameLocation.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back") != null && !gameLocation.isTileOccupied(tile))
			{
				return gameLocation.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport);
			}
			return false;
		}

		public static bool IsOreAt(Vector2 tile)
		{
			if (gameLocation.orePanPoint != Point.Zero && gameLocation.orePanPoint.X == (int)tile.X)
			{
				return gameLocation.orePanPoint.Y == (int)tile.Y;
			}
			return false;
		}

		public static bool isOnOrNearSuspensionBridge(int x, int y)
		{
			if (!Game1.player.onBridge)
			{
				if (y >= 39 && y <= 41 && x > 25)
				{
					if (x >= 39)
					{
						return x > 42;
					}
					return true;
				}
				return false;
			}
			return true;
		}

		public static bool IsIslandNorthSuspensionBridgeRightSide(Vector2 tile)
		{
			if (tile.Y == 39f)
			{
				if (tile.X > 37f)
				{
					return tile.X < 48f;
				}
				return false;
			}
			return false;
		}

		public static AStarPath getPathOnIslandNorthBridge(AStarGraph graph, Vector2 start, Vector2 end)
		{
			AStarPath aStarPath = new AStarPath();
			if (start.Y == 41f)
			{
				aStarPath.nodes.Add(graph.FetchAStarNode(37, 40));
				aStarPath.nodes.Add(graph.FetchAStarNode(37, 39));
			}
			else if (start.Y == 40f)
			{
				aStarPath.nodes.Add(graph.FetchAStarNode(37, 39));
			}
			int num = (int)(end.X - start.X);
			if (num > 0)
			{
				for (int i = 1; i <= num; i++)
				{
					aStarPath.nodes.Add(graph.FetchAStarNode((int)start.X + i, 39));
				}
			}
			else if (num < 0)
			{
				for (int j = 1; j <= Math.Abs(num); j++)
				{
					aStarPath.nodes.Add(graph.FetchAStarNode((int)start.X - j, 39));
				}
			}
			return aStarPath;
		}

		public static bool IsWater(Vector2 tile)
		{
			if (gameLocation is Submarine && tile.X >= 9f && tile.X <= 20f && tile.Y >= 7f && tile.Y <= 11f)
			{
				return true;
			}
			if (gameLocation is VolcanoDungeon)
			{
				if ((gameLocation as VolcanoDungeon).IsCooledLava((int)tile.X, (int)tile.Y))
				{
					return false;
				}
				if ((gameLocation as VolcanoDungeon).CanRefillWateringCanOnTile((int)tile.X, (int)tile.Y))
				{
					return true;
				}
			}
			if (gameLocation.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Water", "Back") == null)
			{
				return gameLocation.doesTileHaveProperty((int)tile.X, (int)tile.Y, "WaterSource", "Back") != null;
			}
			return true;
		}

		public static bool IsBuildingPassable(Vector2 tile)
		{
			Tile tile2 = gameLocation.map.GetLayer("Buildings").PickTile(new Location((int)tile.X * 64, (int)tile.Y * 64), Game1.viewport.Size);
			if (tile2 != null)
			{
				if (tile2.TileIndexProperties.TryGetValue("Passable", out var value) && ((value != null && value == "T") || value == "True"))
				{
					return true;
				}
				PropertyValue value2 = null;
				tile2.Properties.TryGetValue("Passable", out value2);
				if (value2 != null)
				{
					return true;
				}
				PropertyValue value3 = null;
				tile2.TileIndexProperties.TryGetValue("Shadow", out value3);
				if (value3 != null)
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsTilePassable(GameLocation gameLocation, int tileX, int tileY)
		{
			Tile tile = gameLocation.map.GetLayer("Buildings").PickTile(new Location(tileX * 64, tileY * 64), Game1.viewport.Size);
			PropertyValue value3;
			if (tile != null)
			{
				PropertyValue value = null;
				PropertyValue value2 = null;
				tile.TileIndexProperties.TryGetValue("Passable", out value3);
				if (value3 == null)
				{
					tile.Properties.TryGetValue("Passable", out value);
					if (value == null)
					{
						tile.TileIndexProperties.TryGetValue("Shadow", out value2);
					}
				}
				if ((value3 == null || value3.ToString().ToLower()[0] != 't') && (value == null || value.ToString().ToLower()[0] != 't'))
				{
					return value2 != null;
				}
				return true;
			}
			Tile tile2 = gameLocation.map.GetLayer("Back").PickTile(new Location(tileX * 64, tileY * 64), Game1.viewport.Size);
			if (tile2 == null)
			{
				return false;
			}
			tile2.TileIndexProperties.TryGetValue("Passable", out value3);
			if (value3 != null && (value3.ToString().ToLower()[0] == 'f' || value3.ToString() == "0"))
			{
				return false;
			}
			tile2.TileIndexProperties.TryGetValue("Water", out value3);
			if (value3 != null && (!(gameLocation is VolcanoDungeon) || !(gameLocation as VolcanoDungeon).IsCooledLava(tileX, tileY)))
			{
				return false;
			}
			tile2.TileIndexProperties.TryGetValue("WaterSource", out value3);
			if (value3 != null)
			{
				return false;
			}
			return true;
		}

		public static bool IsWateringCanFillingSource(Vector2 tile)
		{
			if (IsWater(tile) && !IsTilePassable(gameLocation, (int)tile.X, (int)tile.Y))
			{
				return true;
			}
			if (gameLocation is BuildableGameLocation)
			{
				Building buildingAt = (gameLocation as BuildableGameLocation).getBuildingAt(tile);
				if (buildingAt != null && (buildingAt is FishPond || buildingAt.buildingType.Equals("Well")) && (int)buildingAt.daysOfConstructionLeft <= 0)
				{
					return true;
				}
			}
			if (gameLocation is Submarine && tile.X >= 9f && tile.X <= 20f && tile.Y >= 7f && tile.Y <= 11f)
			{
				return true;
			}
			if (gameLocation.IsGreenhouse && ((tile.X == 9f && tile.Y == 7f) || (tile.X == 10f && tile.Y == 7f)))
			{
				return true;
			}
			if (gameLocation is Railroad && tile.X >= 14f && tile.X <= 16f && tile.Y >= 55f && tile.Y <= 56f)
			{
				return true;
			}
			if (gameLocation is VolcanoDungeon && (gameLocation as VolcanoDungeon).CanRefillWateringCanOnTile((int)tile.X, (int)tile.Y))
			{
				return true;
			}
			return false;
		}

		public static bool IsMatureTreeStumpOrBoulderAt(Vector2 tile)
		{
			if (IsTreeAt((int)tile.X, (int)tile.Y) || TreeGrowthStage((int)tile.X, (int)tile.Y) > 0)
			{
				return true;
			}
			if (IsChoppableBushAtPoint((int)tile.X, (int)tile.Y))
			{
				return true;
			}
			if (!IsStumpAt((int)tile.X, (int)tile.Y))
			{
				return IsBoulderAt((int)tile.X, (int)tile.Y);
			}
			return true;
		}

		public static bool IsTreeStumpOrBoulderAt(Vector2 tile)
		{
			if (!IsTreeAt((int)tile.X, (int)tile.Y) && !IsStumpAt((int)tile.X, (int)tile.Y) && !IsBoulderAt((int)tile.X, (int)tile.Y))
			{
				return IsChoppableBushAtPoint((int)tile.X, (int)tile.Y);
			}
			return true;
		}

		public static int TreeGrowthStage(AStarNode endNode)
		{
			return TreeGrowthStage(endNode.x, endNode.y);
		}

		public static int TreeGrowthStage(Vector2 tileClicked)
		{
			return TreeGrowthStage((int)tileClicked.X, (int)tileClicked.Y);
		}

		public static int TreeGrowthStage(int x, int y)
		{
			gameLocation.terrainFeatures.TryGetValue(new Vector2(x, y), out var value);
			if (value != null)
			{
				if (value.GetType() == typeof(Tree))
				{
					return ((Tree)value).growthStage;
				}
				if (value.GetType() == typeof(FruitTree))
				{
					return ((FruitTree)value).growthStage;
				}
			}
			return 0;
		}

		public static bool IsTreeAt(AStarNode aStarNode)
		{
			if (aStarNode != null)
			{
				return IsTreeAt(aStarNode.x, aStarNode.y);
			}
			return false;
		}

		public static bool IsTreeAt(Vector2 tile)
		{
			return IsTreeAt((int)tile.X, (int)tile.Y);
		}

		public static bool IsTreeAt(int x, int y)
		{
			gameLocation.terrainFeatures.TryGetValue(new Vector2(x, y), out var value);
			if (value != null && (value is Tree || value is FruitTree))
			{
				return true;
			}
			return false;
		}

		public static TerrainFeature GetTreeAt(int x, int y)
		{
			gameLocation.terrainFeatures.TryGetValue(new Vector2(x, y), out var value);
			if (value != null && value is Tree)
			{
				return (Tree)value;
			}
			if (value != null && value is FruitTree)
			{
				return (FruitTree)value;
			}
			return null;
		}

		public static bool IsBushAt(AStarNode endNode)
		{
			if (IsBushAt(endNode.x, endNode.y))
			{
				return true;
			}
			Vector2 key = new Vector2(endNode.x, endNode.y);
			if (gameLocation.terrainFeatures.ContainsKey(key))
			{
				TerrainFeature terrainFeature = gameLocation.terrainFeatures[key];
				return terrainFeature is Bush;
			}
			return false;
		}

		public static bool IsBushAt(Vector2 tile)
		{
			if (IsBushAt((int)tile.X, (int)tile.Y))
			{
				return true;
			}
			if (gameLocation.terrainFeatures.ContainsKey(tile))
			{
				TerrainFeature terrainFeature = gameLocation.terrainFeatures[tile];
				return terrainFeature is Bush;
			}
			return false;
		}

		public static bool IsBushAt(int x, int y)
		{
			if (x == 32 && y == 9 && Game1.whichFarm == 2 && Game1.currentLocation is Farm)
			{
				return false;
			}
			return IsBushAtPoint(x * 64, y * 64);
		}

		public static bool IsBushAtPoint(int x, int y)
		{
			foreach (LargeTerrainFeature largeTerrainFeature in gameLocation.largeTerrainFeatures)
			{
				if (largeTerrainFeature is Bush && ((Bush)largeTerrainFeature).getRenderBounds(new Vector2(((Bush)largeTerrainFeature).tilePosition.X, ((Bush)largeTerrainFeature).tilePosition.Y)).Contains(x, y))
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsChoppableBushAtPoint(int x, int y)
		{
			foreach (LargeTerrainFeature largeTerrainFeature in gameLocation.largeTerrainFeatures)
			{
				if (largeTerrainFeature is Bush && ((Bush)largeTerrainFeature).getRenderBounds(new Vector2(((Bush)largeTerrainFeature).tilePosition.X, ((Bush)largeTerrainFeature).tilePosition.Y)).Contains(x, y))
				{
					return ((Bush)largeTerrainFeature).isDestroyable(gameLocation, new Vector2(x, y));
				}
			}
			return false;
		}

		public static Bush FetchBushAt(AStarNode aStarNode)
		{
			Vector2 key = new Vector2(aStarNode.x, aStarNode.y);
			if (gameLocation.terrainFeatures.ContainsKey(key))
			{
				TerrainFeature terrainFeature = gameLocation.terrainFeatures[key];
				if (terrainFeature is Bush)
				{
					return (Bush)terrainFeature;
				}
			}
			return FetchBushAtPoint(aStarNode.x * 64, aStarNode.y * 64);
		}

		public static Bush FetchBushAtPoint(int x, int y)
		{
			foreach (LargeTerrainFeature largeTerrainFeature in gameLocation.largeTerrainFeatures)
			{
				if (largeTerrainFeature is Bush && ((Bush)largeTerrainFeature).getRenderBounds(new Vector2(((Bush)largeTerrainFeature).tilePosition.X, ((Bush)largeTerrainFeature).tilePosition.Y)).Contains(x, y))
				{
					return largeTerrainFeature as Bush;
				}
			}
			return null;
		}

		public static bool IsTerrainFeatureAt(AStarNode endNode)
		{
			gameLocation.terrainFeatures.TryGetValue(new Vector2(endNode.x, endNode.y), out var value);
			if (value != null)
			{
				Log.It("TapToMoveUtils.IsTerrainFeatureAt(" + endNode.x + ", " + endNode.y + ") terrainFeature:" + value.GetType().ToString());
				return true;
			}
			return false;
		}

		public static bool IsStumpAt(AStarNode endNode)
		{
			return IsStumpAt(endNode.x, endNode.y);
		}

		public static bool IsStumpAt(Vector2 tile)
		{
			return IsStumpAt((int)tile.X, (int)tile.Y);
		}

		public static bool IsStumpAt(int x, int y)
		{
			if (gameLocation is Farm)
			{
				Farm farm = gameLocation as Farm;
				for (int i = 0; i < farm.resourceClumps.Count; i++)
				{
					ResourceClump resourceClump = farm.resourceClumps[i];
					if (resourceClump.occupiesTile(x, y) && ((int)resourceClump.parentSheetIndex == 600 || (int)resourceClump.parentSheetIndex == 602))
					{
						return true;
					}
				}
			}
			else if (gameLocation is Woods)
			{
				Woods woods = gameLocation as Woods;
				for (int j = 0; j < woods.stumps.Count; j++)
				{
					ResourceClump resourceClump2 = woods.stumps[j];
					if (resourceClump2.occupiesTile(x, y) && ((int)resourceClump2.parentSheetIndex == 600 || (int)resourceClump2.parentSheetIndex == 602))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool IsBoulderAt(AStarNode endNode)
		{
			return IsBoulderAt(endNode.x, endNode.y);
		}

		public static bool IsBoulderAt(Vector2 tile)
		{
			return IsBoulderAt((int)tile.X, (int)tile.Y);
		}

		public static bool IsBoulderAt(int x, int y)
		{
			if (gameLocation is Farm)
			{
				Farm farm = gameLocation as Farm;
				for (int i = 0; i < farm.resourceClumps.Count; i++)
				{
					if (isResourceClumpBoulderAt(farm.resourceClumps[i], x, y))
					{
						return true;
					}
				}
			}
			else if (gameLocation is MineShaft)
			{
				MineShaft mineShaft = gameLocation as MineShaft;
				for (int j = 0; j < mineShaft.resourceClumps.Count; j++)
				{
					if (isResourceClumpBoulderAt(mineShaft.resourceClumps[j], x, y))
					{
						return true;
					}
				}
			}
			gameLocation.objects.TryGetValue(new Vector2(x, y), out var value);
			if (value != null && (value.Name == "Stone" || value.Name == "Boulder"))
			{
				return true;
			}
			return false;
		}

		private static bool isResourceClumpBoulderAt(ResourceClump resourceClump, int x, int y)
		{
			if (resourceClump.occupiesTile(x, y) && ((int)resourceClump.parentSheetIndex == 622 || (int)resourceClump.parentSheetIndex == 672 || (int)resourceClump.parentSheetIndex == 752 || (int)resourceClump.parentSheetIndex == 754 || (int)resourceClump.parentSheetIndex == 756 || (int)resourceClump.parentSheetIndex == 758))
			{
				return true;
			}
			return false;
		}

		public static AStarNode FetchMostAccessibleNodeToCrabPot(AStarGraph aStarGraph, AStarNode aStarNode)
		{
			Location location = new Location(aStarNode.x, aStarNode.y - 1);
			if (!Game1.currentLocation.IsWaterTile(location))
			{
				return aStarGraph.FetchAStarNode(location.X, location.Y);
			}
			location.X = aStarNode.x;
			location.Y = aStarNode.y + 1;
			if (!Game1.currentLocation.IsWaterTile(location))
			{
				return aStarGraph.FetchAStarNode(location.X, location.Y);
			}
			location.X = aStarNode.x - 1;
			location.Y = aStarNode.y;
			if (!Game1.currentLocation.IsWaterTile(location))
			{
				return aStarGraph.FetchAStarNode(location.X, location.Y);
			}
			location.X = aStarNode.x + 1;
			location.Y = aStarNode.y;
			if (!Game1.currentLocation.IsWaterTile(location))
			{
				return aStarGraph.FetchAStarNode(location.X, location.Y);
			}
			location.X = aStarNode.x - 1;
			location.Y = aStarNode.y - 1;
			if (!Game1.currentLocation.IsWaterTile(location))
			{
				return aStarGraph.FetchAStarNode(location.X, location.Y);
			}
			location.X = aStarNode.x + 1;
			location.Y = aStarNode.y - 1;
			if (!Game1.currentLocation.IsWaterTile(location))
			{
				return aStarGraph.FetchAStarNode(location.X, location.Y);
			}
			location.X = aStarNode.x - 1;
			location.Y = aStarNode.y + 1;
			if (!Game1.currentLocation.IsWaterTile(location))
			{
				return aStarGraph.FetchAStarNode(location.X, location.Y);
			}
			location.X = aStarNode.x - 1;
			location.Y = aStarNode.y + 1;
			if (!Game1.currentLocation.IsWaterTile(location))
			{
				return aStarGraph.FetchAStarNode(location.X, location.Y);
			}
			return aStarNode;
		}

		public static AStarNode CrabPotNeighbour(AStarNode aStarNode)
		{
			List<AStarNode> neighbouringNodeListFull = aStarNode.GetNeighbouringNodeListFull(canWalkOnTile: false);
			for (int i = 0; i < neighbouringNodeListFull.Count; i++)
			{
				Object @object = neighbouringNodeListFull[i].FetchObject();
				if (@object != null && (int)@object.parentSheetIndex == 710)
				{
					return neighbouringNodeListFull[i];
				}
			}
			return null;
		}

		public static CrabPot ClickedCrabPot(AStarGraph aStarGraph, AStarNode aStarNode)
		{
			Object @object = aStarNode.FetchObject();
			if (@object != null && (int)@object.parentSheetIndex == 710)
			{
				return @object as CrabPot;
			}
			AStarNode aStarNode2 = aStarGraph.FetchAStarNode(aStarNode.x, aStarNode.y + 1);
			if (aStarNode2 != null)
			{
				@object = aStarNode2.FetchObject();
				if (@object != null && (int)@object.parentSheetIndex == 710 && (bool)((CrabPot)@object).readyForHarvest)
				{
					return @object as CrabPot;
				}
			}
			aStarNode2 = aStarGraph.FetchAStarNode(aStarNode.x, aStarNode.y + 2);
			if (aStarNode2 != null)
			{
				@object = aStarNode2.FetchObject();
				if (@object != null && (int)@object.parentSheetIndex == 710 && (bool)((CrabPot)@object).readyForHarvest)
				{
					return @object as CrabPot;
				}
			}
			return null;
		}

		public static bool IsWizardBuilding(AStarNode endNode)
		{
			return IsWizardBuilding(new Vector2(endNode.x, endNode.y));
		}

		public static bool IsWizardBuilding(Vector2 tile)
		{
			if (gameLocation is BuildableGameLocation)
			{
				BuildableGameLocation buildableGameLocation = (BuildableGameLocation)gameLocation;
				Building buildingAt = buildableGameLocation.getBuildingAt(new Vector2(tile.X, tile.Y));
				if (buildingAt != null && (buildingAt.buildingType.Contains("Obelisk") || buildingAt.buildingType == "Junimo Hut"))
				{
					return true;
				}
			}
			return false;
		}

		public static WalkDirection GetWalkDirectionFacing(Vector2 monsterPosition, Vector2 farmerPosition)
		{
			double num = Math.Atan2(monsterPosition.Y - farmerPosition.Y, monsterPosition.X - farmerPosition.X);
			double num2 = Math.PI / 4.0;
			double num3 = num2 * 3.0;
			if (num >= 0.0 - num2 && num <= num2)
			{
				return WalkDirection.Right;
			}
			if (num >= num2 && num <= num3)
			{
				return WalkDirection.Down;
			}
			if (num <= 0.0 - num2 && num >= 0.0 - num3)
			{
				return WalkDirection.Up;
			}
			return WalkDirection.Left;
		}

		public static int GetDirectionFacing(Vector2 targetPosition, Vector2 startPosition)
		{
			double num = Math.Atan2(targetPosition.Y - startPosition.Y, targetPosition.X - startPosition.X);
			double num2 = num / Math.PI * 180.0;
			double num3 = Math.PI / 4.0;
			double num4 = num3 * 3.0;
			if (num >= 0.0 - num3 && num <= num3)
			{
				return 1;
			}
			if (num >= num3 && num <= num4)
			{
				return 2;
			}
			if (num <= 0.0 - num3 && num >= 0.0 - num4)
			{
				return 0;
			}
			return 3;
		}

		public static Point FetchNextPointOut(int startX, int startY, int endX, int endY)
		{
			Point result = new Point(endX, endY);
			if (startX < endX)
			{
				result.X--;
			}
			else if (startX > endX)
			{
				result.X++;
			}
			if (startY < endY)
			{
				result.Y--;
			}
			else if (startY > endY)
			{
				result.Y++;
			}
			return result;
		}

		public static AStarNode FetchAStarNodeNearestWaterSource(AStarGraph aStarGraph, AStarNode node)
		{
			List<AStarNode> list = new List<AStarNode>();
			for (int i = 1; i < 30; i++)
			{
				AStarNode aStarNode = aStarGraph.FetchAStarNode(node.x + i, node.y);
				if (aStarNode != null && aStarNode.TileClear && !IsWateringCanFillingSource(new Vector2(aStarNode.x, aStarNode.y)))
				{
					list.Add(aStarNode);
				}
				aStarNode = aStarGraph.FetchAStarNode(node.x - i, node.y);
				if (aStarNode != null && aStarNode.TileClear && !IsWateringCanFillingSource(new Vector2(aStarNode.x, aStarNode.y)))
				{
					list.Add(aStarNode);
				}
				aStarNode = aStarGraph.FetchAStarNode(node.x, node.y + i);
				if (aStarNode != null && aStarNode.TileClear && !IsWateringCanFillingSource(new Vector2(aStarNode.x, aStarNode.y)))
				{
					list.Add(aStarNode);
				}
				aStarNode = aStarGraph.FetchAStarNode(node.x, node.y - i);
				if (aStarNode != null && aStarNode.TileClear && !IsWateringCanFillingSource(new Vector2(aStarNode.x, aStarNode.y)))
				{
					list.Add(aStarNode);
				}
				if (list.Count > 0)
				{
					break;
				}
			}
			if (list.Count == 0)
			{
				return null;
			}
			int index = 0;
			float num = 3.4028235E+38f;
			for (int j = 1; j < list.Count; j++)
			{
				float num2 = Vector2.Distance(PlayerOffsetPosition, list[j].NodeCenterOnMap);
				if (num2 < num)
				{
					num = num2;
					index = j;
				}
			}
			int x = node.x;
			int y = node.y;
			if (list[index].x != node.x)
			{
				x = ((list[index].x <= node.x) ? (list[index].x + 1) : (list[index].x - 1));
			}
			else
			{
				y = ((list[index].y <= node.y) ? (list[index].y + 1) : (list[index].y - 1));
			}
			return aStarGraph.FetchAStarNode(x, y);
		}

		public static AStarNode FetchNearestAStarLandNodePerpendicularToWaterSource(AStarGraph aStarGraph, AStarNode farmerNode, AStarNode nodeClicked)
		{
			AStarNode result = nodeClicked;
			AStarNode aStarNode;
			if (farmerNode.x == nodeClicked.x || (farmerNode.y != nodeClicked.y && Math.Abs(nodeClicked.x - farmerNode.x) > Math.Abs(nodeClicked.y - farmerNode.y)))
			{
				if (nodeClicked.y > farmerNode.y)
				{
					for (int num = nodeClicked.y; num >= farmerNode.y; num--)
					{
						aStarNode = aStarGraph.FetchAStarNode(nodeClicked.x, num);
						if (aStarNode != null && aStarNode.TileClear && !IsWateringCanFillingSource(new Vector2(aStarNode.x, aStarNode.y)))
						{
							return result;
						}
						result = aStarNode;
					}
				}
				else
				{
					for (int i = nodeClicked.y; i <= farmerNode.y; i++)
					{
						aStarNode = aStarGraph.FetchAStarNode(nodeClicked.x, i);
						if (aStarNode != null && aStarNode.TileClear && !IsWateringCanFillingSource(new Vector2(aStarNode.x, aStarNode.y)))
						{
							return result;
						}
						result = aStarNode;
					}
				}
			}
			else if (nodeClicked.x > farmerNode.x)
			{
				for (int num2 = nodeClicked.x; num2 >= farmerNode.x; num2--)
				{
					aStarNode = aStarGraph.FetchAStarNode(num2, nodeClicked.y);
					if (aStarNode != null && aStarNode.TileClear && !IsWateringCanFillingSource(new Vector2(aStarNode.x, aStarNode.y)))
					{
						return result;
					}
					result = aStarNode;
				}
			}
			else
			{
				for (int j = nodeClicked.x; j <= farmerNode.x; j++)
				{
					aStarNode = aStarGraph.FetchAStarNode(j, nodeClicked.y);
					if (aStarNode != null && aStarNode.TileClear && !IsWateringCanFillingSource(new Vector2(aStarNode.x, aStarNode.y)))
					{
						return result;
					}
					result = aStarNode;
				}
			}
			aStarNode = FetchAStarNodeNearestWaterSource(aStarGraph, nodeClicked);
			if (aStarNode == null)
			{
				aStarNode = FetchAStarNodeNearestWaterSource(aStarGraph, farmerNode);
			}
			return aStarNode;
		}

		public static Point retargetToParrotExpressSpot(AStarGraph graph, Point tileClicked)
		{
			if (Game1.currentLocation is IslandLocation)
			{
				foreach (ParrotPlatform parrotPlatform in (Game1.currentLocation as IslandLocation).parrotPlatforms)
				{
					if (parrotPlatform.OccupiesTile(Utility.PointToVector2(tileClicked)))
					{
						return Utility.Vector2ToPoint(parrotPlatform.position / 64f + new Vector2(1f, 0f));
					}
				}
				return tileClicked;
			}
			return tileClicked;
		}

		public static Point retargetToBedSpot(AStarGraph graph, Point tileClicked)
		{
			if (Game1.currentLocation is DecoratableLocation)
			{
				BedFurniture bedAtTile = BedFurniture.GetBedAtTile(graph.gameLocation, tileClicked.X, tileClicked.Y);
				if (bedAtTile != null)
				{
					Point bedSpot = bedAtTile.GetBedSpot();
					if (graph.FetchAStarNode(tileClicked.X, tileClicked.Y).isBlockingBedTile())
					{
						if (bedAtTile.bedType == BedFurniture.BedType.Single && Vector2.Distance(Utility.PointToVector2(bedSpot), Game1.player.getTileLocation()) < Vector2.Distance(new Vector2(bedSpot.X - 1, bedSpot.Y), Game1.player.getTileLocation()))
						{
							bedSpot.X--;
						}
						return bedSpot;
					}
				}
			}
			return tileClicked;
		}

		public static bool TappedEggAtEggFestival(Vector2 clickPoint)
		{
			if (Game1.CurrentEvent != null && Game1.CurrentEvent.FestivalName == "Egg Festival")
			{
				for (int i = 0; i < Game1.CurrentEvent.festivalProps.Count; i++)
				{
					Prop prop = Game1.CurrentEvent.festivalProps[i];
					if (prop.ContainsPoint(clickPoint))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static FarmAnimal FetchFarmAnimal(GameLocation gameLocation, int x, int y)
		{
			if (gameLocation is AnimalHouse)
			{
				AnimalHouse animalHouse = (AnimalHouse)gameLocation;
				foreach (FarmAnimal value in animalHouse.animals.Values)
				{
					if (value.BoundingBox.Contains(x, y))
					{
						return value;
					}
				}
			}
			if (gameLocation is Farm)
			{
				Farm farm = (Farm)gameLocation;
				foreach (FarmAnimal value2 in farm.animals.Values)
				{
					if (value2.BoundingBox.Contains(x, y))
					{
						return value2;
					}
				}
			}
			return null;
		}

		public static Fence FetchGate(GameLocation gameLocation, AStarNode aStarNode)
		{
			Vector2 key = new Vector2(aStarNode.x, aStarNode.y);
			if (gameLocation.objects.ContainsKey(key))
			{
				Object @object = gameLocation.objects[key];
				if (@object is Fence)
				{
					Fence fence = @object as Fence;
					if ((bool)fence.isGate)
					{
						return fence;
					}
				}
			}
			return null;
		}

		public static void TraceMap(Map map)
		{
			int layerWidth = map.Layers[0].LayerWidth;
			int layerHeight = map.Layers[0].LayerHeight;
			for (int i = 0; i < map.Layers.Count; i++)
			{
				Layer layer = map.Layers[i];
				Console.WriteLine(i + ") ID:" + layer.Id + ", Description:" + layer.Description + ", visible " + layer.Visible);
				TileArray tiles = map.Layers[i].Tiles;
				Console.WriteLine("totalColumns:" + layerWidth + ", totalRows:" + layerHeight + " mapLayers:" + map.Layers.Count);
				for (int j = 0; j < layerWidth; j++)
				{
					for (int k = 0; k < layerHeight; k++)
					{
						Tile tile = tiles[j, k];
						if (tile == null)
						{
							continue;
						}
						Console.WriteLine("layer:" + i + ", tile[" + j + ", " + k + "] = " + tile.ToString());
						foreach (KeyValuePair<string, PropertyValue> tileIndexProperty in tile.TileIndexProperties)
						{
							Console.WriteLine("TileIndexProperties " + tileIndexProperty.Key + " = " + tileIndexProperty.Value);
						}
						foreach (KeyValuePair<string, PropertyValue> property in tile.Properties)
						{
							Console.WriteLine("Properties " + property.Key + " = " + property.Value);
						}
					}
				}
			}
		}

		public static Vector2 GetTileNextToBuildingNearestFarmer(AStarGraph aStarGraph, Building building, Farmer who)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			if (who.getTileX() < (int)building.tileX)
			{
				num = (int)building.tileX - who.getTileX();
			}
			else if (who.getTileX() > (int)building.tileX + (int)building.tilesWide - 1)
			{
				num2 = who.getTileX() - ((int)building.tileX + (int)building.tilesWide - 1);
			}
			else
			{
				num5 = who.getTileX();
				num6 = ((who.getTileY() >= (int)building.tileY) ? ((int)building.tileY + (int)building.tilesHigh - 1) : ((int)building.tileY));
			}
			if (who.getTileY() < (int)building.tileY)
			{
				num3 = (int)building.tileY - who.getTileY();
			}
			else if (who.getTileY() > (int)building.tileY + (int)building.tilesHigh)
			{
				num4 = who.getTileY() - ((int)building.tileY + (int)building.tilesHigh - 1);
			}
			else
			{
				num6 = who.getTileY();
				num5 = ((who.getTileX() >= (int)building.tileX) ? ((int)building.tileX + (int)building.tilesWide - 1) : ((int)building.tileX));
			}
			if (num5 == 0 && num6 == 0)
			{
				if (num3 > 0 && num > 0)
				{
					num5 = building.tileX;
					num6 = building.tileY;
				}
				else if (num3 > 0 && num2 > 0)
				{
					num5 = (int)building.tileX + (int)building.tilesWide - 1;
					num6 = building.tileY;
				}
				else if (num4 > 0 && num > 0)
				{
					num5 = building.tileX;
					num6 = (int)building.tileY + (int)building.tilesHigh - 1;
				}
				else if (num4 > 0 && num2 > 0)
				{
					num5 = (int)building.tileX + (int)building.tilesWide - 1;
					num6 = (int)building.tileY + (int)building.tilesHigh - 1;
				}
			}
			List<Vector2> list = ListOfTilesSurroundingBuilding(building);
			int num7 = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if ((int)list[i].X == num5 && (int)list[i].Y == num6)
				{
					num7 = i;
					break;
				}
			}
			AStarNode farmerAStarNodeOffset = aStarGraph.FarmerAStarNodeOffset;
			Vector2 vector = FetchAccessibleTileNextToBuilding(list, num7, aStarGraph, farmerAStarNodeOffset);
			if (vector != Vector2.Zero)
			{
				return vector;
			}
			for (int j = 1; j < list.Count / 2; j++)
			{
				int num8 = num7 - j;
				if (num8 < 0)
				{
					num8 += list.Count;
					vector = FetchAccessibleTileNextToBuilding(list, num8, aStarGraph, farmerAStarNodeOffset);
					if (vector != Vector2.Zero)
					{
						return vector;
					}
				}
				num8 = num7 + j;
				if (num8 > list.Count - 1)
				{
					num8 -= list.Count;
					vector = FetchAccessibleTileNextToBuilding(list, num8, aStarGraph, farmerAStarNodeOffset);
					if (vector != Vector2.Zero)
					{
						return vector;
					}
				}
			}
			return new Vector2(who.getTileX(), who.getTileY());
		}

		private static Vector2 FetchAccessibleTileNextToBuilding(List<Vector2> tilesAroundBuilding, int offset, AStarGraph aStarGraph, AStarNode startNode)
		{
			int num = (int)tilesAroundBuilding[offset].X;
			int num2 = (int)tilesAroundBuilding[offset].Y;
			AStarNode aStarNode = aStarGraph.FetchAStarNode(num, num2);
			if (aStarNode != null)
			{
				aStarNode.FakeTileClear = true;
				AStarPath shortestPathAStarWithBubbleCheck = aStarGraph.GetShortestPathAStarWithBubbleCheck(startNode, aStarNode);
				if (shortestPathAStarWithBubbleCheck != null && shortestPathAStarWithBubbleCheck.nodes != null && shortestPathAStarWithBubbleCheck.nodes.Count > 0)
				{
					return new Vector2(num, num2);
				}
				aStarNode.FakeTileClear = false;
			}
			return Vector2.Zero;
		}

		private static List<Vector2> ListOfTilesSurroundingBuilding(Building building)
		{
			List<Vector2> list = new List<Vector2>();
			for (int i = 0; i <= (int)building.tilesWide - 1; i++)
			{
				list.Add(new Vector2((int)building.tileX + i, (int)building.tileY));
			}
			for (int j = 1; j <= (int)building.tilesHigh - 1; j++)
			{
				list.Add(new Vector2((int)building.tileX + (int)building.tilesWide - 1, (int)building.tileY + j));
			}
			for (int k = 1; k <= (int)building.tilesWide - 1; k++)
			{
				list.Add(new Vector2((int)building.tileX + (int)building.tilesWide - k - 1, (int)building.tileY + (int)building.tilesHigh - 1));
			}
			for (int l = 1; l < (int)building.tilesHigh - 1; l++)
			{
				list.Add(new Vector2((int)building.tileX, (int)building.tileY + (int)building.tilesHigh - l - 1));
			}
			return list;
		}
	}
}
