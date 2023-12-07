using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley.Mobile
{
	public class AStarNode
	{
		public int bubbleID = -1;

		public int bubbleID2 = -1;

		private AStarGraph _aStarGraph;

		public bool bubbleChecked;

		private AStarNode _searchAStarNode;

		private bool _fakeTileClear;

		public float fCost { get; set; }

		public float gCost { get; set; }

		public float hCost { get; set; }

		public AStarNode parentNode { get; set; }

		public int x { get; set; }

		public int y { get; set; }

		public Microsoft.Xna.Framework.Rectangle rectangle => new Microsoft.Xna.Framework.Rectangle(x * 64, y * 64, 64, 64);

		public List<AStarNode> NeighbouringNodeList => GetNeighbouringNodeList();

		public List<AStarNode> OccupiedNeighbouringNodeList => GetNeighbouringNodeList(canWalkOnTile: false);

		public Vector2 NodeCenterOnMap => new Vector2((float)(x * 64) + 32f, (float)(y * 64) + 32f);

		public bool FakeTileClear
		{
			get
			{
				return _fakeTileClear;
			}
			set
			{
				_fakeTileClear = value;
			}
		}

		public bool TileClear
		{
			get
			{
				if (_fakeTileClear)
				{
					return true;
				}
				if (_aStarGraph.gameLocation.isTileOnMap(new Vector2(x, y)) && (!_aStarGraph.gameLocation.isTileOccupiedIgnoreFloorsAndHorse(new Vector2(x, y)) || isGate()) && isTilePassable() && !ContainsStumpOrBoulder() && !ContainsFurniture() && (!isFence() || isGate()) && (!ContainsBuilding() || IsBuildingPassable()) && !ContainsAnimals() && !ContainsNPC() && !ContainsFestivalProp() && !isBlockingBedTile() && !ContainsTravellingCart() && !ContainsTravellingDesertShop() && !BrokenFestivalTile && !ContainsCinema())
				{
					return !ContainsParrotExpress();
				}
				return false;
			}
		}

		public bool BrokenFestivalTile
		{
			get
			{
				if (Game1.CurrentEvent != null)
				{
					if (x == 18 && y == 31 && Game1.dayOfMonth == 16 && Game1.currentSeason == "fall")
					{
						return true;
					}
					if (x == 16 && y == 19 && Game1.dayOfMonth == 27 && Game1.currentSeason == "fall")
					{
						return true;
					}
					if (x == 66 && y == 4 && Game1.dayOfMonth == 8 && Game1.currentSeason == "winter")
					{
						return true;
					}
					if (x == 103 && y == 28 && Game1.dayOfMonth == 8 && Game1.currentSeason == "winter")
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool fakeTileClear => _fakeTileClear;

		public Microsoft.Xna.Framework.Rectangle rect => new Microsoft.Xna.Framework.Rectangle(x * 64, y * 64, 64, 64);

		public AStarNode(AStarGraph aStarGraph, int x, int y)
		{
			_aStarGraph = aStarGraph;
			this.x = x;
			this.y = y;
		}

		public bool SetBubbleIDRecursively(int bubbleID, bool two = false)
		{
			if (bubbleChecked)
			{
				return false;
			}
			bubbleChecked = true;
			if (this.bubbleID == 0 || TileClear)
			{
				if (two)
				{
					bubbleID2 = bubbleID;
				}
				else
				{
					this.bubbleID = bubbleID;
				}
				_searchAStarNode = _aStarGraph.FetchAStarNode(x, y - 1);
				if (_searchAStarNode != null)
				{
					_searchAStarNode.SetBubbleIDRecursively(bubbleID, two);
				}
				_searchAStarNode = _aStarGraph.FetchAStarNode(x, y + 1);
				if (_searchAStarNode != null)
				{
					_searchAStarNode.SetBubbleIDRecursively(bubbleID, two);
				}
				_searchAStarNode = _aStarGraph.FetchAStarNode(x - 1, y);
				if (_searchAStarNode != null)
				{
					_searchAStarNode.SetBubbleIDRecursively(bubbleID, two);
				}
				_searchAStarNode = _aStarGraph.FetchAStarNode(x + 1, y);
				if (_searchAStarNode != null)
				{
					_searchAStarNode.SetBubbleIDRecursively(bubbleID, two);
				}
				_searchAStarNode = null;
				return true;
			}
			return false;
		}

		public List<AStarNode> GetNeighbouringNodeList(bool canWalkOnTile = true)
		{
			List<AStarNode> list = new List<AStarNode>();
			AStarNode aStarNode = _aStarGraph.FetchAStarNode(x, y - 1);
			if (aStarNode != null && aStarNode.TileClear == canWalkOnTile)
			{
				list.Add(aStarNode);
			}
			aStarNode = _aStarGraph.FetchAStarNode(x, y + 1);
			if (aStarNode != null && aStarNode.TileClear == canWalkOnTile)
			{
				list.Add(aStarNode);
			}
			aStarNode = _aStarGraph.FetchAStarNode(x - 1, y);
			if (aStarNode != null && aStarNode.TileClear == canWalkOnTile)
			{
				list.Add(aStarNode);
			}
			aStarNode = _aStarGraph.FetchAStarNode(x + 1, y);
			if (aStarNode != null && aStarNode.TileClear == canWalkOnTile)
			{
				list.Add(aStarNode);
			}
			return list;
		}

		public List<AStarNode> GetNeighbouringNodeListFull(bool canWalkOnTile = true)
		{
			List<AStarNode> list = new List<AStarNode>();
			AStarNode aStarNode = _aStarGraph.FetchAStarNode(x, y - 1);
			if (aStarNode != null && aStarNode.TileClear == canWalkOnTile)
			{
				list.Add(aStarNode);
			}
			aStarNode = _aStarGraph.FetchAStarNode(x, y + 1);
			if (aStarNode != null && aStarNode.TileClear == canWalkOnTile)
			{
				list.Add(aStarNode);
			}
			aStarNode = _aStarGraph.FetchAStarNode(x - 1, y);
			if (aStarNode != null && aStarNode.TileClear == canWalkOnTile)
			{
				list.Add(aStarNode);
			}
			aStarNode = _aStarGraph.FetchAStarNode(x + 1, y);
			if (aStarNode != null && aStarNode.TileClear == canWalkOnTile)
			{
				list.Add(aStarNode);
			}
			aStarNode = _aStarGraph.FetchAStarNode(x - 1, y - 1);
			if (aStarNode != null && aStarNode.TileClear == canWalkOnTile)
			{
				list.Add(aStarNode);
			}
			aStarNode = _aStarGraph.FetchAStarNode(x + 1, y - 1);
			if (aStarNode != null && aStarNode.TileClear == canWalkOnTile)
			{
				list.Add(aStarNode);
			}
			aStarNode = _aStarGraph.FetchAStarNode(x - 1, y + 1);
			if (aStarNode != null && aStarNode.TileClear == canWalkOnTile)
			{
				list.Add(aStarNode);
			}
			aStarNode = _aStarGraph.FetchAStarNode(x + 1, y + 1);
			if (aStarNode != null && aStarNode.TileClear == canWalkOnTile)
			{
				list.Add(aStarNode);
			}
			return list;
		}

		public Microsoft.Xna.Framework.Rectangle GetBoundingBox()
		{
			return new Microsoft.Xna.Framework.Rectangle(x * 64, y * 64, 64, 64);
		}

		public bool ContainsParrotExpress()
		{
			if (_aStarGraph.gameLocation is IslandLocation)
			{
				foreach (ParrotPlatform parrotPlatform in (_aStarGraph.gameLocation as IslandLocation).parrotPlatforms)
				{
					if (parrotPlatform.OccupiesTile(new Vector2(x, y)) && parrotPlatform.position / 64f + new Vector2(1f, 1f) != new Vector2(x, y) && parrotPlatform.position / 64f + new Vector2(1f, 0f) != new Vector2(x, y))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool DebugTileClear()
		{
			DebugObjectParentSheetIndexOnTile();
			return TileClear;
		}

		public bool ContainsTravellingCart()
		{
			if (_aStarGraph.gameLocation is Forest)
			{
				Forest forest = (Forest)_aStarGraph.gameLocation;
				if (forest.travelingMerchantBounds != null)
				{
					foreach (Microsoft.Xna.Framework.Rectangle travelingMerchantBound in forest.travelingMerchantBounds)
					{
						if (travelingMerchantBound.Intersects(rectangle))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public bool ContainsTravellingDesertShop()
		{
			if (_aStarGraph.gameLocation is Desert)
			{
				Desert desert = (Desert)_aStarGraph.gameLocation;
			}
			return false;
		}

		public bool ContainsCinema()
		{
			if (_aStarGraph.gameLocation is Town && Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater"))
			{
				if (x >= 47 && x <= 58 && y >= 17 && y <= 19)
				{
					return true;
				}
				if (x == 47 && y == 20)
				{
					return true;
				}
				if (x >= 55 && x <= 58 && y == 20)
				{
					return true;
				}
			}
			return false;
		}

		public bool isBlockingBedTile()
		{
			if (_aStarGraph.gameLocation is DecoratableLocation)
			{
				BedFurniture bedAtTile = BedFurniture.GetBedAtTile(_aStarGraph.gameLocation, x, y);
				if (bedAtTile != null)
				{
					Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(x * 64, y * 64, 64, 64);
					return bedAtTile.IntersectsForCollision(rectangle);
				}
			}
			return false;
		}

		public bool isFence()
		{
			if (_aStarGraph.gameLocation.objects.ContainsKey(new Vector2(x, y)))
			{
				Object @object = _aStarGraph.gameLocation.objects[new Vector2(x, y)];
				if (@object is Fence)
				{
					return true;
				}
			}
			return false;
		}

		public bool isGate()
		{
			if (_aStarGraph.gameLocation.objects.ContainsKey(new Vector2(x, y)))
			{
				Object @object = _aStarGraph.gameLocation.objects[new Vector2(x, y)];
				if (@object is Fence && (bool)((Fence)@object).isGate && !((Fence)@object).isSoloGate)
				{
					return true;
				}
			}
			return false;
		}

		public bool isGateOpen()
		{
			if (_aStarGraph.gameLocation.objects.ContainsKey(new Vector2(x, y)))
			{
				Object @object = _aStarGraph.gameLocation.objects[new Vector2(x, y)];
				if (@object is Fence && (bool)((Fence)@object).isGate)
				{
					if (((Fence)@object).isSoloGate)
					{
						return false;
					}
					if ((int)((Fence)@object).gatePosition == 88)
					{
						return true;
					}
				}
			}
			return false;
		}

		public Object FetchObject()
		{
			if (_aStarGraph.gameLocation.objects.ContainsKey(new Vector2(x, y)))
			{
				return _aStarGraph.gameLocation.objects[new Vector2(x, y)];
			}
			return null;
		}

		public int ObjectParentSheetIndexOnTile()
		{
			_aStarGraph.gameLocation.objects.TryGetValue(new Vector2(x, y), out var value);
			if (value != null)
			{
				return value.parentSheetIndex;
			}
			return -1;
		}

		private void DebugObjectParentSheetIndexOnTile()
		{
			_aStarGraph.gameLocation.objects.TryGetValue(new Vector2(x, y), out var value);
			if (value != null)
			{
				Log.It("obj.parentSheetIndex:" + value.parentSheetIndex?.ToString() + ", " + value.ToString());
			}
		}

		public bool isTilePassable()
		{
			return TapToMoveUtils.IsTilePassable(_aStarGraph.gameLocation, x, y);
		}

		public bool IsBuildingPassable()
		{
			Tile tile = _aStarGraph.gameLocation.map.GetLayer("Buildings").PickTile(new Location(x * 64, y * 64), Game1.viewport.Size);
			if (tile != null)
			{
				tile.TileIndexProperties.TryGetValue("Passable", out var value);
				if (value != null && (value.ToString().ToLower() == "t" || value.ToString().ToLower() == "true"))
				{
					return true;
				}
				PropertyValue value2 = null;
				tile.Properties.TryGetValue("Passable", out value2);
				if (value2 != null && (value2.ToString().ToLower() == "t" || value2.ToString().ToLower() == "true"))
				{
					return true;
				}
				PropertyValue value3 = null;
				tile.TileIndexProperties.TryGetValue("Shadow", out value3);
				if (value3 != null)
				{
					return true;
				}
			}
			return false;
		}

		public bool DebugIsTilePassable()
		{
			Log.It("AStarNode.DebugIsTilePassable (" + x + "," + y + ")... " + isTilePassable());
			Tile tile = _aStarGraph.gameLocation.map.GetLayer("Back").PickTile(new Location(x * 64, y * 64), Game1.viewport.Size);
			if (tile == null)
			{
				Log.It("AStarNode.DebugIsTilePassable A (" + x + "," + y + ") FALSE tile is null");
				return false;
			}
			tile.TileIndexProperties.TryGetValue("Passable", out var value);
			if (value != null)
			{
				Log.It("AStarNode.DebugIsTilePassable B (" + x + "," + y + ") FALSE Passable:" + value);
				return false;
			}
			Tile tile2 = _aStarGraph.gameLocation.map.GetLayer("Buildings").PickTile(new Location(x * 64, y * 64), Game1.viewport.Size);
			if (tile2 != null)
			{
				tile2.TileIndexProperties.TryGetValue("Passable", out value);
				Log.It("AStarNode.DebugIsTilePassable C (" + x + "," + y + ") BUILDING Passable:" + ((value == null) ? "Null" : value.ToString()) + ", IsBuildingPassable():" + IsBuildingPassable());
				foreach (KeyValuePair<string, PropertyValue> tileIndexProperty in tile2.TileIndexProperties)
				{
					Log.It("AStarNode.DebugIsTilePassable C TileIndexProperties:" + tileIndexProperty.Key + " => " + tileIndexProperty.Value);
				}
				foreach (KeyValuePair<string, PropertyValue> property in tile2.Properties)
				{
					Log.It("AStarNode.DebugIsTilePassable C Properties:" + property.Key + " => " + property.Value);
				}
				PropertyValue value2 = null;
				tile2.TileIndexProperties.TryGetValue("Shadow", out value2);
				if (value2 != null)
				{
					Log.It("AStarNode.DebugIsTilePassable C has shadow");
				}
				if (value == null)
				{
					return value2 != null;
				}
				return true;
			}
			tile.TileIndexProperties.TryGetValue("Water", out value);
			if (value != null)
			{
				Log.It("AStarNode.DebugIsTilePassable D (" + x + "," + y + ") FALSE Water:" + value);
				return false;
			}
			tile.TileIndexProperties.TryGetValue("WaterSource", out value);
			if (value != null)
			{
				Log.It("AStarNode.DebugIsTilePassable E (" + x + "," + y + ") FALSE WaterSource:" + value);
				return false;
			}
			Log.It("AStarNode.DebugIsTilePassable F (" + x + "," + y + ")... isTilePassable:" + isTilePassable() + ", _aStarGraph.gameLocation.isTilePassable:" + _aStarGraph.gameLocation.isTilePassable(new Location(x, y), Game1.viewport));
			if (isTilePassable() != _aStarGraph.gameLocation.isTilePassable(new Location(x, y), Game1.viewport))
			{
				PropertyValue value3 = null;
				Tile tile3 = _aStarGraph.gameLocation.map.GetLayer("Back").PickTile(new Location(x * 64, y * 64), Game1.viewport.Size);
				tile3?.TileIndexProperties.TryGetValue("Passable", out value3);
				Tile tile4 = _aStarGraph.gameLocation.map.GetLayer("Buildings").PickTile(new Location(x * 64, y * 64), Game1.viewport.Size);
				if (tile4 != null)
				{
					tile4.TileIndexProperties.TryGetValue("Passable", out var value4);
					Log.It("AStarNode.DebugIsTilePassable G (" + x + "," + y + ") BUILDING Passable:" + ((value4 == null) ? "Null" : value4.ToString()));
				}
				Log.It("AStarNode.DebugIsTilePassable H (passable == null):" + (value3 == null) + ", (tileX == null):" + (tile4 == null) + ", (tmp != null):" + (tile3 != null));
				if (value3 == null && tile4 == null)
				{
					return tile3 != null;
				}
				return false;
			}
			return true;
		}

		public bool ContainsSomeKindOfWarp()
		{
			Tile tile = _aStarGraph.gameLocation.map.GetLayer("Buildings").PickTile(new Location(x * 64, y * 64), Game1.viewport.Size);
			if (tile != null)
			{
				tile.TileIndexProperties.TryGetValue("Passable", out var _);
				foreach (KeyValuePair<string, PropertyValue> property in tile.Properties)
				{
					string text = property.Value;
					if (text.Contains("LockedDoorWarp") || text.Contains("Warp") || text.Contains("WarpMensLocker") || text.Contains("WarpWomensLocker"))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool ContainsStumpOrBoulder()
		{
			if (_aStarGraph.gameLocation is Farm)
			{
				Farm farm = _aStarGraph.gameLocation as Farm;
				for (int i = 0; i < farm.resourceClumps.Count; i++)
				{
					ResourceClump resourceClump = farm.resourceClumps[i];
					if (resourceClump.occupiesTile(x, y))
					{
						return true;
					}
				}
			}
			else if (_aStarGraph.gameLocation is MineShaft)
			{
				MineShaft mineShaft = _aStarGraph.gameLocation as MineShaft;
				for (int j = 0; j < mineShaft.resourceClumps.Count; j++)
				{
					ResourceClump resourceClump2 = mineShaft.resourceClumps[j];
					if (resourceClump2.occupiesTile(x, y))
					{
						return true;
					}
				}
			}
			else if (_aStarGraph.gameLocation is Woods)
			{
				Woods woods = _aStarGraph.gameLocation as Woods;
				for (int k = 0; k < woods.stumps.Count; k++)
				{
					ResourceClump resourceClump3 = woods.stumps[k];
					if (resourceClump3.occupiesTile(x, y))
					{
						return true;
					}
				}
			}
			else if (_aStarGraph.gameLocation is Forest)
			{
				Forest forest = _aStarGraph.gameLocation as Forest;
				if (forest.log != null && forest.log.occupiesTile(x, y))
				{
					return true;
				}
			}
			else if (_aStarGraph.gameLocation is IslandWest)
			{
				IslandWest islandWest = _aStarGraph.gameLocation as IslandWest;
				for (int l = 0; l < islandWest.resourceClumps.Count; l++)
				{
					ResourceClump resourceClump4 = islandWest.resourceClumps[l];
					if (resourceClump4.occupiesTile(x, y))
					{
						return true;
					}
				}
			}
			_aStarGraph.gameLocation.objects.TryGetValue(new Vector2(x, y), out var value);
			if (value != null && value.Name == "Boulder")
			{
				return true;
			}
			return false;
		}

		public bool ContainsGiantCrop()
		{
			if (_aStarGraph.gameLocation is Farm)
			{
				Farm farm = _aStarGraph.gameLocation as Farm;
				for (int i = 0; i < farm.resourceClumps.Count; i++)
				{
					ResourceClump resourceClump = farm.resourceClumps[i];
					if (resourceClump.occupiesTile(x, y) && resourceClump is GiantCrop)
					{
						return true;
					}
				}
			}
			return false;
		}

		public GiantCrop FetchGiantCrop()
		{
			if (_aStarGraph.gameLocation is Farm)
			{
				Farm farm = _aStarGraph.gameLocation as Farm;
				for (int i = 0; i < farm.resourceClumps.Count; i++)
				{
					ResourceClump resourceClump = farm.resourceClumps[i];
					if (resourceClump.occupiesTile(x, y) && resourceClump is GiantCrop)
					{
						return (GiantCrop)resourceClump;
					}
				}
			}
			return null;
		}

		public bool ContainsStumpOrHollowLog()
		{
			if (_aStarGraph.gameLocation is Farm)
			{
				Farm farm = _aStarGraph.gameLocation as Farm;
				for (int i = 0; i < farm.resourceClumps.Count; i++)
				{
					ResourceClump resourceClump = farm.resourceClumps[i];
					if (resourceClump.occupiesTile(x, y) && ((int)resourceClump.parentSheetIndex == 602 || (int)resourceClump.parentSheetIndex == 600))
					{
						return true;
					}
				}
			}
			else if (_aStarGraph.gameLocation is Woods)
			{
				Woods woods = _aStarGraph.gameLocation as Woods;
				for (int j = 0; j < woods.stumps.Count; j++)
				{
					ResourceClump resourceClump2 = woods.stumps[j];
					if (resourceClump2.occupiesTile(x, y))
					{
						return true;
					}
				}
			}
			else if (_aStarGraph.gameLocation is Forest)
			{
				Forest forest = _aStarGraph.gameLocation as Forest;
				if (forest.log != null && forest.log.occupiesTile(x, y))
				{
					return true;
				}
			}
			else if (_aStarGraph.gameLocation is IslandWest)
			{
				IslandWest islandWest = _aStarGraph.gameLocation as IslandWest;
				for (int k = 0; k < islandWest.resourceClumps.Count; k++)
				{
					ResourceClump resourceClump3 = islandWest.resourceClumps[k];
					if (resourceClump3.occupiesTile(x, y) && ((int)resourceClump3.parentSheetIndex == 602 || (int)resourceClump3.parentSheetIndex == 600))
					{
						return true;
					}
				}
			}
			else if (_aStarGraph.gameLocation is MineShaft)
			{
				MineShaft mineShaft = _aStarGraph.gameLocation as MineShaft;
				for (int l = 0; l < mineShaft.resourceClumps.Count; l++)
				{
					ResourceClump resourceClump4 = mineShaft.resourceClumps[l];
					if (resourceClump4.occupiesTile(x, y) && ((int)resourceClump4.parentSheetIndex == 602 || (int)resourceClump4.parentSheetIndex == 600))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool ContainsFurniture()
		{
			Microsoft.Xna.Framework.Rectangle value = rect;
			foreach (Furniture item in _aStarGraph.gameLocation.furniture)
			{
				if ((int)item.furniture_type != 12 && (int)item.furniture_type != 15 && item.getBoundingBox(item.tileLocation).Intersects(value))
				{
					return true;
				}
			}
			return false;
		}

		public Furniture GetFurniture()
		{
			Microsoft.Xna.Framework.Rectangle value = rect;
			foreach (Furniture item in _aStarGraph.gameLocation.furniture)
			{
				if (item.getBoundingBox(item.tileLocation).Intersects(value))
				{
					return item;
				}
			}
			return null;
		}

		public bool ContainsChest()
		{
			_aStarGraph.gameLocation.objects.TryGetValue(new Vector2(x, y), out var value);
			if (value != null)
			{
				return value is Chest;
			}
			return false;
		}

		public Chest FetchChest()
		{
			_aStarGraph.gameLocation.objects.TryGetValue(new Vector2(x, y), out var value);
			if (value != null && value is Chest)
			{
				return value as Chest;
			}
			return null;
		}

		public bool isBed()
		{
			if (_aStarGraph.gameLocation is FarmHouse)
			{
				Vector2 vector = Utility.PointToVector2(Utility.getHomeOfFarmer(Game1.player).getBedSpot()) * 64f;
				if (x == (int)vector.X && y == (int)vector.Y)
				{
					return true;
				}
			}
			return false;
		}

		public bool ContainsBuilding()
		{
			if (_aStarGraph.gameLocation is BuildableGameLocation)
			{
				BuildableGameLocation buildableGameLocation = (BuildableGameLocation)_aStarGraph.gameLocation;
				foreach (Building building in buildableGameLocation.buildings)
				{
					if (!building.isTilePassable(new Vector2(x, y)))
					{
						return true;
					}
				}
			}
			Tile tile = _aStarGraph.gameLocation.map.GetLayer("Buildings").PickTile(new Location(x * 64, y * 64), Game1.viewport.Size);
			if (tile != null)
			{
				return true;
			}
			return false;
		}

		public Building FetchBuilding()
		{
			if (_aStarGraph.gameLocation is BuildableGameLocation)
			{
				BuildableGameLocation buildableGameLocation = (BuildableGameLocation)_aStarGraph.gameLocation;
				foreach (Building building in buildableGameLocation.buildings)
				{
					if (!building.isTilePassable(new Vector2(x, y)))
					{
						return building;
					}
				}
			}
			return null;
		}

		public bool ContainsNPC()
		{
			if (_aStarGraph.gameLocation is Beach && ((Beach)_aStarGraph.gameLocation).oldMariner != null)
			{
				NPC oldMariner = ((Beach)_aStarGraph.gameLocation).oldMariner;
				if (oldMariner.getTileX() == x && oldMariner.getTileY() == y)
				{
					return true;
				}
			}
			for (int i = 0; i < _aStarGraph.gameLocation.characters.Count; i++)
			{
				if ((!(_aStarGraph.gameLocation.characters[i] is Pet) || !((Pet)_aStarGraph.gameLocation.characters[i]).isSleepingOnFarmerBed) && _aStarGraph.gameLocation.characters[i].getTileX() == x && _aStarGraph.gameLocation.characters[i].getTileY() == y)
				{
					return true;
				}
			}
			if (_aStarGraph.gameLocation.currentEvent != null && _aStarGraph.gameLocation.currentEvent.actors != null)
			{
				for (int j = 0; j < _aStarGraph.gameLocation.currentEvent.actors.Count; j++)
				{
					if (_aStarGraph.gameLocation.currentEvent.actors[j].getTileX() == x && _aStarGraph.gameLocation.currentEvent.actors[j].getTileY() == y)
					{
						return true;
					}
				}
			}
			return false;
		}

		public NPC FetchNPC()
		{
			if (_aStarGraph.gameLocation is Beach && ((Beach)_aStarGraph.gameLocation).oldMariner != null)
			{
				NPC oldMariner = ((Beach)_aStarGraph.gameLocation).oldMariner;
				if (oldMariner.getTileX() == x && oldMariner.getTileY() == y)
				{
					return oldMariner;
				}
			}
			for (int i = 0; i < _aStarGraph.gameLocation.characters.Count; i++)
			{
				if (_aStarGraph.gameLocation.characters[i].getTileX() == x && _aStarGraph.gameLocation.characters[i].getTileY() == y)
				{
					return _aStarGraph.gameLocation.characters[i];
				}
			}
			if (_aStarGraph.gameLocation.currentEvent != null && _aStarGraph.gameLocation.currentEvent.actors != null)
			{
				for (int j = 0; j < _aStarGraph.gameLocation.currentEvent.actors.Count; j++)
				{
					if (_aStarGraph.gameLocation.currentEvent.actors[j].getTileX() == x && _aStarGraph.gameLocation.currentEvent.actors[j].getTileY() == y)
					{
						return _aStarGraph.gameLocation.currentEvent.actors[j];
					}
				}
			}
			return null;
		}

		public bool ContainsAnimals()
		{
			if (_aStarGraph.gameLocation is AnimalHouse)
			{
				AnimalHouse animalHouse = (AnimalHouse)_aStarGraph.gameLocation;
				foreach (FarmAnimal value in animalHouse.animals.Values)
				{
					if (value.getTileX() == x && value.getTileY() == y)
					{
						return true;
					}
				}
			}
			else if (_aStarGraph.gameLocation is Farm)
			{
				Farm farm = (Farm)_aStarGraph.gameLocation;
				foreach (FarmAnimal value2 in farm.animals.Values)
				{
					if (value2.getTileX() == x && value2.getTileY() == y)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool ContainsProp()
		{
			if (Game1.CurrentEvent != null)
			{
				for (int i = 0; i < Game1.CurrentEvent.props.Count; i++)
				{
					Object @object = Game1.CurrentEvent.props[i];
					if (@object.TileLocation.X == (float)x && @object.TileLocation.Y == (float)y)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool ContainsFestivalProp()
		{
			if (Game1.CurrentEvent != null)
			{
				Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle(x * 64, y * 64, 64, 64);
				for (int i = 0; i < Game1.CurrentEvent.festivalProps.Count; i++)
				{
					Prop prop = Game1.CurrentEvent.festivalProps[i];
					if (prop.isColliding(r))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool ContainsGate()
		{
			Vector2 key = new Vector2(x, y);
			if (_aStarGraph.gameLocation.objects.ContainsKey(key))
			{
				Object @object = _aStarGraph.gameLocation.objects[key];
				if (@object is Fence && (bool)((Fence)@object).isGate)
				{
					return true;
				}
			}
			return false;
		}

		public bool ContainsScarecrow()
		{
			Vector2 key = new Vector2(x, y);
			if (_aStarGraph.gameLocation.objects.ContainsKey(key))
			{
				Object @object = _aStarGraph.gameLocation.objects[key];
				if ((int)@object.parentSheetIndex == 8 || (int)@object.parentSheetIndex == 167 || (int)@object.parentSheetIndex == 110 || (int)@object.parentSheetIndex == 113 || (int)@object.parentSheetIndex == 126 || (int)@object.parentSheetIndex == 136 || (int)@object.parentSheetIndex == 137 || (int)@object.parentSheetIndex == 138 || (int)@object.parentSheetIndex == 139 || (int)@object.parentSheetIndex == 140)
				{
					return true;
				}
			}
			return false;
		}

		public Fence FetchGate()
		{
			Vector2 key = new Vector2(x, y);
			if (_aStarGraph.gameLocation.objects.ContainsKey(key))
			{
				Object @object = _aStarGraph.gameLocation.objects[key];
				if (@object is Fence && (bool)((Fence)@object).isGate)
				{
					return @object as Fence;
				}
			}
			return null;
		}

		public override string ToString()
		{
			string text = "AStarNode -> x:" + x + ", y:" + y + "\n";
			for (int i = 0; i < _aStarGraph.map.Layers.Count; i++)
			{
				Tile tile = _aStarGraph.map.Layers[i].Tiles[x, y];
				if (tile == null)
				{
					text = text + "layer: " + i + ", tile:null\n";
					continue;
				}
				text = text + "layer: " + i + ", tile:" + tile.ToString() + "\n";
				foreach (KeyValuePair<string, PropertyValue> tileIndexProperty in tile.TileIndexProperties)
				{
					text = string.Concat(text, "TileIndexProperties: ", tileIndexProperty.Key, " = ", tileIndexProperty.Value, "\n");
				}
				foreach (KeyValuePair<string, PropertyValue> property in tile.Properties)
				{
					text = string.Concat(text, "Properties: ", property.Key, " = ", property.Value, "\n");
				}
			}
			return text;
		}
	}
}
