using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley
{
	public class Farm : BuildableGameLocation, IAnimalLocation
	{
		public class LightningStrikeEvent : NetEventArg
		{
			public Vector2 boltPosition;

			public bool createBolt;

			public bool bigFlash;

			public bool smallFlash;

			public bool destroyedTerrainFeature;

			public void Read(BinaryReader reader)
			{
				createBolt = reader.ReadBoolean();
				bigFlash = reader.ReadBoolean();
				smallFlash = reader.ReadBoolean();
				destroyedTerrainFeature = reader.ReadBoolean();
				boltPosition.X = reader.ReadInt32();
				boltPosition.Y = reader.ReadInt32();
			}

			public void Write(BinaryWriter writer)
			{
				writer.Write(createBolt);
				writer.Write(bigFlash);
				writer.Write(smallFlash);
				writer.Write(destroyedTerrainFeature);
				writer.Write((int)boltPosition.X);
				writer.Write((int)boltPosition.Y);
			}
		}

		[XmlIgnore]
		[NonInstancedStatic]
		public static Texture2D houseTextures = Game1.content.Load<Texture2D>("Buildings\\houses");

		[XmlIgnore]
		public Texture2D paintedHouseTexture;

		public Color? frameHouseColor;

		public NetRef<BuildingPaintColor> housePaintColor = new NetRef<BuildingPaintColor>();

		public const int default_layout = 0;

		public const int riverlands_layout = 1;

		public const int forest_layout = 2;

		public const int mountains_layout = 3;

		public const int combat_layout = 4;

		public const int fourCorners_layout = 5;

		public const int beach_layout = 6;

		public const int mod_layout = 7;

		public const int layout_max = 7;

		[XmlElement("animals")]
		public readonly NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> animals = new NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>();

		[XmlElement("piecesOfHay")]
		public readonly NetInt piecesOfHay = new NetInt(0);

		[XmlElement("grandpaScore")]
		public NetInt grandpaScore = new NetInt(0);

		[XmlElement("farmCaveReady")]
		public NetBool farmCaveReady = new NetBool(value: false);

		private TemporaryAnimatedSprite shippingBinLid;

		private Microsoft.Xna.Framework.Rectangle shippingBinLidOpenArea = new Microsoft.Xna.Framework.Rectangle(4480, 832, 256, 192);

		public readonly NetCollection<Item> sharedShippingBin = new NetCollection<Item>();

		public Item lastItemShipped;

		public bool hasSeenGrandpaNote;

		protected Dictionary<string, Dictionary<Point, Tile>> _baseSpouseAreaTiles = new Dictionary<string, Dictionary<Point, Tile>>();

		[XmlElement("houseSource")]
		public readonly NetRectangle houseSource = new NetRectangle();

		[XmlElement("greenhouseUnlocked")]
		public readonly NetBool greenhouseUnlocked = new NetBool();

		[XmlElement("greenhouseMoved")]
		public readonly NetBool greenhouseMoved = new NetBool();

		private readonly NetEvent1Field<Vector2, NetVector2> spawnCrowEvent = new NetEvent1Field<Vector2, NetVector2>();

		public readonly NetEvent1<LightningStrikeEvent> lightningStrikeEvent = new NetEvent1<LightningStrikeEvent>();

		private readonly List<KeyValuePair<long, FarmAnimal>> _tempAnimals = new List<KeyValuePair<long, FarmAnimal>>();

		public readonly NetBool petBowlWatered = new NetBool(value: false);

		[XmlIgnore]
		public readonly NetPoint petBowlPosition = new NetPoint();

		[XmlIgnore]
		public Point? mapGrandpaShrinePosition;

		[XmlIgnore]
		public Point? mapMainMailboxPosition;

		[XmlIgnore]
		public Point? mainFarmhouseEntry;

		[XmlIgnore]
		public Vector2? mapSpouseAreaCorner;

		[XmlIgnore]
		public Vector2? mapShippingBinPosition;

		private int chimneyTimer = 500;

		private readonly Action _ApplyHousePaint;

		protected Microsoft.Xna.Framework.Rectangle? _mountainForageRectangle;

		protected bool? _shouldSpawnForestFarmForage;

		protected bool? _shouldSpawnBeachFarmForage;

		protected bool? _oceanCrabPotOverride;

		protected string _fishLocationOverride;

		protected float _fishChanceOverride;

		public Point spousePatioSpot;

		public const int numCropsForCrow = 16;

		public NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> Animals => animals;

		public Farm()
		{
			_ApplyHousePaint = ApplyHousePaint;
		}

		public Farm(string mapPath, string name)
			: base(mapPath, name)
		{
			if (Game1.IsMasterGame)
			{
				Layer layer = map.GetLayer("Buildings");
				for (int i = 0; i < layer.LayerWidth; i++)
				{
					for (int j = 0; j < layer.LayerHeight; j++)
					{
						if (layer.Tiles[i, j] != null && layer.Tiles[i, j].TileIndex == 1938)
						{
							petBowlPosition.Set(i, j);
						}
					}
				}
			}
			AddModularShippingBin();
			_ApplyHousePaint = ApplyHousePaint;
		}

		public virtual void AddModularShippingBin()
		{
			Building building = new ShippingBin(new BluePrint("Shipping Bin"), GetStarterShippingBinLocation());
			buildings.Add(building);
			building.load();
			building = new GreenhouseBuilding(new BluePrint("Greenhouse"), GetGreenhouseStartLocation());
			buildings.Add(building);
			building.load();
		}

		public virtual Microsoft.Xna.Framework.Rectangle GetHouseRect()
		{
			Point mainFarmHouseEntry = GetMainFarmHouseEntry();
			return new Microsoft.Xna.Framework.Rectangle(mainFarmHouseEntry.X - 5, mainFarmHouseEntry.Y - 4, 9, 6);
		}

		public virtual Vector2 GetStarterShippingBinLocation()
		{
			if (!mapShippingBinPosition.HasValue)
			{
				mapShippingBinPosition = Utility.PointToVector2(GetMapPropertyPosition("ShippingBinLocation", 71, 14));
			}
			return mapShippingBinPosition.Value;
		}

		public virtual Vector2 GetGreenhouseStartLocation()
		{
			if (map.Properties.ContainsKey("GreenhouseLocation"))
			{
				int result = -1;
				int result2 = -1;
				string text = map.Properties["GreenhouseLocation"].ToString();
				string[] array = text.Split(' ');
				if (array.Length >= 2 && int.TryParse(array[0], out result) && int.TryParse(array[1], out result2))
				{
					return new Vector2(result, result2);
				}
			}
			if (Game1.whichFarm == 5)
			{
				return new Vector2(36f, 29f);
			}
			if (Game1.whichFarm == 6)
			{
				return new Vector2(14f, 14f);
			}
			return new Vector2(25f, 10f);
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(animals, piecesOfHay, sharedShippingBin, houseSource, spawnCrowEvent, petBowlWatered, petBowlPosition, lightningStrikeEvent, grandpaScore, greenhouseUnlocked, greenhouseMoved, housePaintColor, farmCaveReady);
			spawnCrowEvent.onEvent += doSpawnCrow;
			lightningStrikeEvent.onEvent += doLightningStrike;
			greenhouseMoved.fieldChangeVisibleEvent += delegate
			{
				ClearGreenhouseGrassTiles();
			};
			petBowlWatered.fieldChangeVisibleEvent += delegate
			{
				_UpdateWaterBowl();
			};
			if (housePaintColor.Value == null)
			{
				housePaintColor.Value = new BuildingPaintColor();
			}
		}

		public virtual void ClearGreenhouseGrassTiles()
		{
			if (map != null && Game1.gameMode != 6 && greenhouseMoved.Value)
			{
				if (Game1.whichFarm == 0 || Game1.whichFarm == 4 || Game1.whichFarm == 3)
				{
					ApplyMapOverride("Farm_Greenhouse_Dirt", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle((int)GetGreenhouseStartLocation().X, (int)GetGreenhouseStartLocation().Y, 9, 6));
				}
				else if (Game1.whichFarm == 5)
				{
					ApplyMapOverride("Farm_Greenhouse_Dirt_FourCorners", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle((int)GetGreenhouseStartLocation().X, (int)GetGreenhouseStartLocation().Y, 9, 6));
				}
			}
		}

		protected void _UpdateWaterBowl()
		{
			if (map != null)
			{
				if (petBowlWatered.Value)
				{
					setMapTileIndex(petBowlPosition.X, petBowlPosition.Y, 1939, "Buildings");
				}
				else
				{
					setMapTileIndex(petBowlPosition.X, petBowlPosition.Y, 1938, "Buildings");
				}
			}
		}

		public static string getMapNameFromTypeInt(int type)
		{
			switch (type)
			{
			case 0:
				return "Farm";
			case 1:
				return "Farm_Fishing";
			case 2:
				return "Farm_Foraging";
			case 3:
				return "Farm_Mining";
			case 4:
				return "Farm_Combat";
			case 5:
				return "Farm_FourCorners";
			case 6:
				return "Farm_Island";
			case 7:
				if (Game1.whichModFarm != null)
				{
					return Game1.whichModFarm.MapName;
				}
				break;
			}
			return "Farm";
		}

		public Point GetPetStartLocation()
		{
			return new Point(petBowlPosition.X - 1, petBowlPosition.Y + 1);
		}

		public override void DayUpdate(int dayOfMonth)
		{
			for (int num = animals.Count() - 1; num >= 0; num--)
			{
				animals.Pairs.ElementAt(num).Value.dayUpdate(this);
			}
			base.DayUpdate(dayOfMonth);
			UpdatePatio();
			for (int num2 = characters.Count - 1; num2 >= 0; num2--)
			{
				if (characters[num2] is Pet && (getTileIndexAt(characters[num2].getTileLocationPoint(), "Buildings") != -1 || getTileIndexAt(characters[num2].getTileX() + 1, characters[num2].getTileY(), "Buildings") != -1 || !isTileLocationTotallyClearAndPlaceable(characters[num2].getTileLocation()) || !isTileLocationTotallyClearAndPlaceable(new Vector2(characters[num2].getTileX() + 1, characters[num2].getTileY()))))
				{
					characters[num2].setTilePosition(GetPetStartLocation());
				}
			}
			lastItemShipped = null;
			for (int num3 = characters.Count - 1; num3 >= 0; num3--)
			{
				if (characters[num3] is JunimoHarvester)
				{
					characters.RemoveAt(num3);
				}
			}
			for (int num4 = characters.Count - 1; num4 >= 0; num4--)
			{
				if (characters[num4] is Monster && (characters[num4] as Monster).wildernessFarmMonster)
				{
					characters.RemoveAt(num4);
				}
			}
			if (characters.Count > 5)
			{
				int num5 = 0;
				for (int num6 = characters.Count - 1; num6 >= 0; num6--)
				{
					if (characters[num6] is GreenSlime && Game1.random.NextDouble() < 0.035)
					{
						characters.RemoveAt(num6);
						num5++;
					}
				}
				if (num5 > 0)
				{
					Game1.multiplayer.broadcastGlobalMessage((num5 == 1) ? "Strings\\Locations:Farm_1SlimeEscaped" : "Strings\\Locations:Farm_NSlimesEscaped", false, num5.ToString() ?? "");
				}
			}
			if (Game1.whichFarm == 5)
			{
				if (isTileLocationTotallyClearAndPlaceable(5, 32) && isTileLocationTotallyClearAndPlaceable(6, 32) && isTileLocationTotallyClearAndPlaceable(6, 33) && isTileLocationTotallyClearAndPlaceable(5, 33))
				{
					resourceClumps.Add(new ResourceClump(600, 2, 2, new Vector2(5f, 32f)));
				}
				if (objects.Count() > 0)
				{
					for (int i = 0; i < 6; i++)
					{
						Object value = objects.Pairs.ElementAt(Game1.random.Next(objects.Count())).Value;
						if (value.name.Equals("Weeds") && value.tileLocation.X < 36f && value.tileLocation.Y < 34f)
						{
							value.ParentSheetIndex = 792 + Utility.getSeasonNumber(Game1.currentSeason);
						}
					}
				}
			}
			if (ShouldSpawnBeachFarmForage())
			{
				while (Game1.random.NextDouble() < 0.9)
				{
					Vector2 randomTile = getRandomTile();
					if (!isTileLocationTotallyClearAndPlaceable(randomTile) || getTileIndexAt((int)randomTile.X, (int)randomTile.Y, "AlwaysFront") != -1)
					{
						continue;
					}
					int num7 = -1;
					if (doesTileHavePropertyNoNull((int)randomTile.X, (int)randomTile.Y, "BeachSpawn", "Back") != "")
					{
						num7 = 372;
						Game1.stats.incrementStat("beachFarmSpawns", 1);
						switch (Game1.random.Next(6))
						{
						case 0:
							num7 = 393;
							break;
						case 1:
							num7 = 719;
							break;
						case 2:
							num7 = 718;
							break;
						case 3:
							num7 = 723;
							break;
						case 4:
						case 5:
							num7 = 152;
							break;
						}
						if (Game1.stats.DaysPlayed > 1)
						{
							if (Game1.random.NextDouble() < 0.15 || Game1.stats.getStat("beachFarmSpawns") % 4u == 0)
							{
								num7 = Game1.random.Next(922, 925);
								objects.Add(randomTile, new Object(randomTile, num7, 1)
								{
									Fragility = 2,
									MinutesUntilReady = 3
								});
								num7 = -1;
							}
							else if (Game1.random.NextDouble() < 0.1)
							{
								num7 = 397;
							}
							else if (Game1.random.NextDouble() < 0.05)
							{
								num7 = 392;
							}
							else if (Game1.random.NextDouble() < 0.02)
							{
								num7 = 394;
							}
						}
					}
					else if (Game1.currentSeason != "winter" && new Microsoft.Xna.Framework.Rectangle(20, 66, 33, 18).Contains((int)randomTile.X, (int)randomTile.Y) && doesTileHavePropertyNoNull((int)randomTile.X, (int)randomTile.Y, "Type", "Back") == "Grass")
					{
						num7 = Utility.getRandomBasicSeasonalForageItem(Game1.currentSeason, (int)Game1.stats.DaysPlayed);
					}
					if (num7 != -1)
					{
						dropObject(new Object(randomTile, num7, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: true), randomTile * 64f, Game1.viewport, initialPlacement: true);
					}
				}
			}
			if (Game1.whichFarm == 2)
			{
				for (int j = 0; j < 20; j++)
				{
					for (int k = 0; k < map.Layers[0].LayerHeight; k++)
					{
						if (map.GetLayer("Paths").Tiles[j, k] != null && map.GetLayer("Paths").Tiles[j, k].TileIndex == 21 && isTileLocationTotallyClearAndPlaceable(j, k) && isTileLocationTotallyClearAndPlaceable(j + 1, k) && isTileLocationTotallyClearAndPlaceable(j + 1, k + 1) && isTileLocationTotallyClearAndPlaceable(j, k + 1))
						{
							resourceClumps.Add(new ResourceClump(600, 2, 2, new Vector2(j, k)));
						}
					}
				}
			}
			if (ShouldSpawnForestFarmForage() && !Game1.IsWinter)
			{
				while (Game1.random.NextDouble() < 0.75)
				{
					Vector2 vector = new Vector2(Game1.random.Next(18), Game1.random.Next(map.Layers[0].LayerHeight));
					if (Game1.random.NextDouble() < 0.5 || Game1.whichFarm != 2)
					{
						vector = getRandomTile();
					}
					if (!isTileLocationTotallyClearAndPlaceable(vector) || getTileIndexAt((int)vector.X, (int)vector.Y, "AlwaysFront") != -1 || ((Game1.whichFarm != 2 || !(vector.X < 18f)) && !doesTileHavePropertyNoNull((int)vector.X, (int)vector.Y, "Type", "Back").Equals("Grass")))
					{
						continue;
					}
					int parentSheetIndex = 792;
					switch (Game1.currentSeason)
					{
					case "spring":
						switch (Game1.random.Next(4))
						{
						case 0:
							parentSheetIndex = 16;
							break;
						case 1:
							parentSheetIndex = 22;
							break;
						case 2:
							parentSheetIndex = 20;
							break;
						case 3:
							parentSheetIndex = 257;
							break;
						}
						break;
					case "summer":
						switch (Game1.random.Next(4))
						{
						case 0:
							parentSheetIndex = 402;
							break;
						case 1:
							parentSheetIndex = 396;
							break;
						case 2:
							parentSheetIndex = 398;
							break;
						case 3:
							parentSheetIndex = 404;
							break;
						}
						break;
					case "fall":
						switch (Game1.random.Next(4))
						{
						case 0:
							parentSheetIndex = 281;
							break;
						case 1:
							parentSheetIndex = 420;
							break;
						case 2:
							parentSheetIndex = 422;
							break;
						case 3:
							parentSheetIndex = 404;
							break;
						}
						break;
					}
					dropObject(new Object(vector, parentSheetIndex, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: true), vector * 64f, Game1.viewport, initialPlacement: true);
				}
				if (objects.Count() > 0)
				{
					for (int l = 0; l < 6; l++)
					{
						Object value2 = objects.Pairs.ElementAt(Game1.random.Next(objects.Count())).Value;
						if (value2.name.Equals("Weeds"))
						{
							value2.ParentSheetIndex = 792 + Utility.getSeasonNumber(Game1.currentSeason);
						}
					}
				}
			}
			if (Game1.whichFarm == 3 || Game1.whichFarm == 5 || ShouldSpawnMountainOres())
			{
				doDailyMountainFarmUpdate();
			}
			ICollection<Vector2> collection = new List<Vector2>(terrainFeatures.Keys);
			for (int num8 = collection.Count - 1; num8 >= 0; num8--)
			{
				Vector2 key = collection.ElementAt(num8);
				if (!(terrainFeatures[key] is HoeDirt) || (terrainFeatures[key] as HoeDirt).crop != null)
				{
					continue;
				}
				if (objects.ContainsKey(key))
				{
					Object @object = objects[key];
					if (@object != null && @object.IsSpawnedObject && @object.isForage(this))
					{
						continue;
					}
				}
				if (Game1.random.NextDouble() <= 0.1)
				{
					terrainFeatures.Remove(key);
				}
			}
			if (terrainFeatures.Count() > 0 && Game1.currentSeason.Equals("fall") && Game1.dayOfMonth > 1 && Game1.random.NextDouble() < 0.05)
			{
				for (int m = 0; m < 10; m++)
				{
					TerrainFeature value3 = terrainFeatures.Pairs.ElementAt(Game1.random.Next(terrainFeatures.Count())).Value;
					if (value3 is Tree && (int)(value3 as Tree).growthStage >= 5 && !(value3 as Tree).tapped)
					{
						(value3 as Tree).treeType.Value = 7;
						(value3 as Tree).loadSprite();
						break;
					}
				}
			}
			addCrows();
			if (!Game1.currentSeason.Equals("winter"))
			{
				spawnWeedsAndStones(Game1.currentSeason.Equals("summer") ? 30 : 20);
			}
			spawnWeeds(weedsOnly: false);
			HandleGrassGrowth(dayOfMonth);
		}

		public void doDailyMountainFarmUpdate()
		{
			double num = 1.0;
			while (Game1.random.NextDouble() < num)
			{
				Vector2 zero = Vector2.Zero;
				zero = ((!ShouldSpawnMountainOres()) ? ((Game1.whichFarm == 5) ? Utility.getRandomPositionInThisRectangle(new Microsoft.Xna.Framework.Rectangle(51, 67, 11, 3), Game1.random) : Utility.getRandomPositionInThisRectangle(new Microsoft.Xna.Framework.Rectangle(5, 37, 22, 8), Game1.random)) : Utility.getRandomPositionInThisRectangle(_mountainForageRectangle.Value, Game1.random));
				if (doesTileHavePropertyNoNull((int)zero.X, (int)zero.Y, "Type", "Back").Equals("Dirt") && isTileLocationTotallyClearAndPlaceable(zero))
				{
					int parentSheetIndex = 668;
					int minutesUntilReady = 2;
					if (Game1.random.NextDouble() < 0.15)
					{
						objects.Add(zero, new Object(zero, 590, 1));
						continue;
					}
					if (Game1.random.NextDouble() < 0.5)
					{
						parentSheetIndex = 670;
					}
					if (Game1.random.NextDouble() < 0.1)
					{
						if (Game1.player.MiningLevel >= 8 && Game1.random.NextDouble() < 0.33)
						{
							parentSheetIndex = 77;
							minutesUntilReady = 7;
						}
						else if (Game1.player.MiningLevel >= 5 && Game1.random.NextDouble() < 0.5)
						{
							parentSheetIndex = 76;
							minutesUntilReady = 5;
						}
						else
						{
							parentSheetIndex = 75;
							minutesUntilReady = 3;
						}
					}
					if (Game1.random.NextDouble() < 0.21)
					{
						parentSheetIndex = 751;
						minutesUntilReady = 3;
					}
					if (Game1.player.MiningLevel >= 4 && Game1.random.NextDouble() < 0.15)
					{
						parentSheetIndex = 290;
						minutesUntilReady = 4;
					}
					if (Game1.player.MiningLevel >= 7 && Game1.random.NextDouble() < 0.1)
					{
						parentSheetIndex = 764;
						minutesUntilReady = 8;
					}
					if (Game1.player.MiningLevel >= 10 && Game1.random.NextDouble() < 0.01)
					{
						parentSheetIndex = 765;
						minutesUntilReady = 16;
					}
					objects.Add(zero, new Object(zero, parentSheetIndex, 10)
					{
						MinutesUntilReady = minutesUntilReady
					});
				}
				num *= 0.75;
			}
		}

		public override bool catchOceanCrabPotFishFromThisSpot(int x, int y)
		{
			if (map != null)
			{
				if (!_oceanCrabPotOverride.HasValue)
				{
					_oceanCrabPotOverride = map.Properties.ContainsKey("FarmOceanCrabPotOverride");
				}
				if (_oceanCrabPotOverride.Value)
				{
					return true;
				}
			}
			if (Game1.whichFarm == 6)
			{
				if (x > 28 && x < 57 && y > 46 && y < 82)
				{
					return false;
				}
				return true;
			}
			return base.catchOceanCrabPotFishFromThisSpot(x, y);
		}

		public override float getExtraTrashChanceForCrabPot(int x, int y)
		{
			if (Game1.whichFarm == 6)
			{
				if (x > 28 && x < 57 && y > 46 && y < 82)
				{
					return 0.25f;
				}
				return 0f;
			}
			return base.getExtraTrashChanceForCrabPot(x, y);
		}

		public void addCrows()
		{
			int num = 0;
			foreach (KeyValuePair<Vector2, TerrainFeature> pair in terrainFeatures.Pairs)
			{
				if (pair.Value is HoeDirt && (pair.Value as HoeDirt).crop != null)
				{
					num++;
				}
			}
			List<Vector2> list = new List<Vector2>();
			foreach (KeyValuePair<Vector2, Object> pair2 in objects.Pairs)
			{
				if ((bool)pair2.Value.bigCraftable && pair2.Value.IsScarecrow())
				{
					list.Add(pair2.Key);
				}
			}
			int num2 = Math.Min(4, num / 16);
			for (int i = 0; i < num2; i++)
			{
				if (!(Game1.random.NextDouble() < 0.3))
				{
					continue;
				}
				for (int j = 0; j < 10; j++)
				{
					Vector2 key = terrainFeatures.Pairs.ElementAt(Game1.random.Next(terrainFeatures.Count())).Key;
					if (!(terrainFeatures[key] is HoeDirt) || (terrainFeatures[key] as HoeDirt).crop == null || (int)(terrainFeatures[key] as HoeDirt).crop.currentPhase <= 1)
					{
						continue;
					}
					bool flag = false;
					foreach (Vector2 item in list)
					{
						int radiusForScarecrow = objects[item].GetRadiusForScarecrow();
						if (Vector2.Distance(item, key) < (float)radiusForScarecrow)
						{
							flag = true;
							objects[item].SpecialVariable++;
							break;
						}
					}
					if (!flag)
					{
						(terrainFeatures[key] as HoeDirt).destroyCrop(key, showAnimation: false, this);
						spawnCrowEvent.Fire(key);
					}
					break;
				}
			}
		}

		private void doSpawnCrow(Vector2 v)
		{
			if (critters == null && (bool)isOutdoors)
			{
				critters = new List<Critter>();
			}
			critters.Add(new Crow((int)v.X, (int)v.Y));
		}

		public static Point getFrontDoorPositionForFarmer(Farmer who)
		{
			Point mainFarmHouseEntry = Game1.getFarm().GetMainFarmHouseEntry();
			mainFarmHouseEntry.Y--;
			return mainFarmHouseEntry;
		}

		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			if (timeOfDay >= 1300 && Game1.IsMasterGame)
			{
				List<Character> list = new List<Character>(characters);
				foreach (NPC item in list)
				{
					if (item.isMarried())
					{
						item.returnHomeFromFarmPosition(this);
					}
				}
			}
			foreach (NPC character in characters)
			{
				if (character.getSpouse() == Game1.player)
				{
					character.checkForMarriageDialogue(timeOfDay, this);
				}
				if (character is Child)
				{
					(character as Child).tenMinuteUpdate();
				}
			}
			if (!Game1.spawnMonstersAtNight || Game1.farmEvent != null || Game1.timeOfDay < 1900 || !(Game1.random.NextDouble() < 0.25 - Game1.player.team.AverageDailyLuck() / 2.0))
			{
				return;
			}
			if (Game1.random.NextDouble() < 0.25)
			{
				if (Equals(Game1.currentLocation))
				{
					spawnFlyingMonstersOffScreen();
				}
			}
			else
			{
				spawnGroundMonsterOffScreen();
			}
		}

		public void spawnGroundMonsterOffScreen()
		{
			bool flag = false;
			for (int i = 0; i < 15; i++)
			{
				Vector2 zero = Vector2.Zero;
				zero = getRandomTile();
				if (Utility.isOnScreen(Utility.Vector2ToPoint(zero), 64, this))
				{
					zero.X -= Game1.viewport.Width / 64;
				}
				if (!isTileLocationTotallyClearAndPlaceable(zero))
				{
					continue;
				}
				if (Game1.player.CombatLevel >= 8 && Game1.random.NextDouble() < 0.15)
				{
					characters.Add(new ShadowBrute(zero * 64f)
					{
						focusedOnFarmers = true,
						wildernessFarmMonster = true
					});
					flag = true;
				}
				else if (Game1.random.NextDouble() < ((Game1.whichFarm == 4) ? 0.66 : 0.33) && isTileLocationTotallyClearAndPlaceable(zero))
				{
					characters.Add(new RockGolem(zero * 64f, Game1.player.CombatLevel)
					{
						wildernessFarmMonster = true
					});
					flag = true;
				}
				else
				{
					int mineLevel = 1;
					if (Game1.player.CombatLevel >= 10)
					{
						mineLevel = 140;
					}
					else if (Game1.player.CombatLevel >= 8)
					{
						mineLevel = 100;
					}
					else if (Game1.player.CombatLevel >= 4)
					{
						mineLevel = 41;
					}
					characters.Add(new GreenSlime(zero * 64f, mineLevel)
					{
						wildernessFarmMonster = true
					});
					flag = true;
				}
				if (!flag || !Game1.currentLocation.Equals(this))
				{
					break;
				}
				{
					foreach (KeyValuePair<Vector2, Object> pair in objects.Pairs)
					{
						if (pair.Value != null && (bool)pair.Value.bigCraftable && (int)pair.Value.parentSheetIndex == 83)
						{
							pair.Value.shakeTimer = 1000;
							pair.Value.showNextIndex.Value = true;
							Game1.currentLightSources.Add(new LightSource(4, pair.Key * 64f + new Vector2(32f, 0f), 1f, Color.Cyan * 0.75f, (int)(pair.Key.X * 797f + pair.Key.Y * 13f + 666f), LightSource.LightContext.None, 0L));
						}
					}
					break;
				}
			}
		}

		public void spawnFlyingMonstersOffScreen()
		{
			bool flag = false;
			Vector2 zero = Vector2.Zero;
			switch (Game1.random.Next(4))
			{
			case 0:
				zero.X = Game1.random.Next(map.Layers[0].LayerWidth);
				break;
			case 3:
				zero.Y = Game1.random.Next(map.Layers[0].LayerHeight);
				break;
			case 1:
				zero.X = map.Layers[0].LayerWidth - 1;
				zero.Y = Game1.random.Next(map.Layers[0].LayerHeight);
				break;
			case 2:
				zero.Y = map.Layers[0].LayerHeight - 1;
				zero.X = Game1.random.Next(map.Layers[0].LayerWidth);
				break;
			}
			if (Utility.isOnScreen(zero * 64f, 64))
			{
				zero.X -= Game1.viewport.Width;
			}
			if (Game1.player.CombatLevel >= 10 && Game1.random.NextDouble() < 0.01 && Game1.player.hasItemInInventoryNamed("Galaxy Sword"))
			{
				characters.Add(new Bat(zero * 64f, 9999)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				flag = true;
			}
			else if (Game1.player.CombatLevel >= 10 && Game1.random.NextDouble() < 0.25)
			{
				characters.Add(new Bat(zero * 64f, 172)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				flag = true;
			}
			else if (Game1.player.CombatLevel >= 10 && Game1.random.NextDouble() < 0.25)
			{
				characters.Add(new Serpent(zero * 64f)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				flag = true;
			}
			else if (Game1.player.CombatLevel >= 8 && Game1.random.NextDouble() < 0.5)
			{
				characters.Add(new Bat(zero * 64f, 81)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				flag = true;
			}
			else if (Game1.player.CombatLevel >= 5 && Game1.random.NextDouble() < 0.5)
			{
				characters.Add(new Bat(zero * 64f, 41)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				flag = true;
			}
			else
			{
				characters.Add(new Bat(zero * 64f, 1)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				flag = true;
			}
			if (!flag || !Game1.currentLocation.Equals(this))
			{
				return;
			}
			foreach (KeyValuePair<Vector2, Object> pair in objects.Pairs)
			{
				if (pair.Value != null && (bool)pair.Value.bigCraftable && (int)pair.Value.parentSheetIndex == 83)
				{
					pair.Value.shakeTimer = 1000;
					pair.Value.showNextIndex.Value = true;
					Game1.currentLightSources.Add(new LightSource(4, pair.Key * 64f + new Vector2(32f, 0f), 1f, Color.Cyan * 0.75f, (int)(pair.Key.X * 797f + pair.Key.Y * 13f + 666f), LightSource.LightContext.None, 0L));
				}
			}
		}

		public override bool performToolAction(Tool t, int tileX, int tileY)
		{
			Point point = new Point(tileX * 64 + 32, tileY * 64 + 32);
			if (t is MeleeWeapon)
			{
				foreach (FarmAnimal value in animals.Values)
				{
					if (value.GetBoundingBox().Intersects((t as MeleeWeapon).mostRecentArea))
					{
						value.hitWithWeapon(t as MeleeWeapon);
					}
				}
			}
			if (t is WateringCan && (t as WateringCan).WaterLeft > 0 && getTileIndexAt(tileX, tileY, "Buildings") == 1938 && !petBowlWatered.Value)
			{
				petBowlWatered.Set(newValue: true);
				_UpdateWaterBowl();
			}
			return base.performToolAction(t, tileX, tileY);
		}

		public override void timeUpdate(int timeElapsed)
		{
			base.timeUpdate(timeElapsed);
			if (Game1.IsMasterGame)
			{
				foreach (FarmAnimal value in animals.Values)
				{
					value.updatePerTenMinutes(Game1.timeOfDay, this);
				}
			}
			foreach (Building building in buildings)
			{
				if ((int)building.daysOfConstructionLeft > 0)
				{
					continue;
				}
				building.performTenMinuteAction(timeElapsed);
				if (building.indoors.Value != null && !Game1.locations.Contains(building.indoors.Value) && timeElapsed >= 10)
				{
					building.indoors.Value.performTenMinuteUpdate(Game1.timeOfDay);
					if (timeElapsed > 10)
					{
						building.indoors.Value.passTimeForObjects(timeElapsed - 10);
					}
				}
			}
		}

		public bool placeAnimal(BluePrint blueprint, Vector2 tileLocation, bool serverCommand, long ownerID)
		{
			for (int i = 0; i < blueprint.tilesHeight; i++)
			{
				for (int j = 0; j < blueprint.tilesWidth; j++)
				{
					Vector2 vector = new Vector2(tileLocation.X + (float)j, tileLocation.Y + (float)i);
					if (Game1.player.getTileLocation().Equals(vector) || isTileOccupied(vector) || !isTilePassable(new Location((int)vector.X, (int)vector.Y), Game1.viewport))
					{
						return false;
					}
				}
			}
			long newID = Game1.multiplayer.getNewID();
			FarmAnimal farmAnimal = new FarmAnimal(blueprint.name, newID, ownerID);
			farmAnimal.Position = new Vector2(tileLocation.X * 64f + 4f, tileLocation.Y * 64f + 64f - (float)farmAnimal.Sprite.getHeight() - 4f);
			animals.Add(newID, farmAnimal);
			if (farmAnimal.sound.Value != null && !farmAnimal.sound.Value.Equals(""))
			{
				localSound(farmAnimal.sound);
			}
			return true;
		}

		public int tryToAddHay(int num)
		{
			int num2 = Utility.numSilos();
			int val = num2 * 240 - (int)piecesOfHay;
			int num3 = Math.Min(val, num);
			piecesOfHay.Value += num3;
			_ = 0;
			return num - num3;
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false)
		{
			if (!glider)
			{
				if (resourceClumps.Count > 0)
				{
					Microsoft.Xna.Framework.Rectangle value = character?.GetBoundingBox() ?? Microsoft.Xna.Framework.Rectangle.Empty;
					foreach (ResourceClump resourceClump in resourceClumps)
					{
						Microsoft.Xna.Framework.Rectangle boundingBox = resourceClump.getBoundingBox(resourceClump.tile);
						if (boundingBox.Intersects(position) && (!isFarmer || character == null || !boundingBox.Intersects(value)))
						{
							return true;
						}
					}
				}
				if (character != null && !(character is FarmAnimal))
				{
					Microsoft.Xna.Framework.Rectangle boundingBox2 = Game1.player.GetBoundingBox();
					Farmer farmer = (isFarmer ? (character as Farmer) : null);
					foreach (FarmAnimal value2 in animals.Values)
					{
						if (position.Intersects(value2.GetBoundingBox()) && (!isFarmer || !boundingBox2.Intersects(value2.GetBoundingBox())))
						{
							if (farmer != null && farmer.TemporaryPassableTiles.Intersects(position))
							{
								break;
							}
							value2.farmerPushing();
							return true;
						}
					}
				}
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding, projectile, ignoreCharacterRequirement);
		}

		public bool CheckPetAnimal(Vector2 position, Farmer who)
		{
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				if (!pair.Value.wasPet && pair.Value.GetCursorPetBoundingBox().Contains((int)position.X, (int)position.Y))
				{
					pair.Value.pet(who);
					return true;
				}
			}
			return false;
		}

		public bool CheckPetAnimal(Microsoft.Xna.Framework.Rectangle rect, Farmer who)
		{
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				if (!pair.Value.wasPet && pair.Value.GetBoundingBox().Intersects(rect))
				{
					pair.Value.pet(who);
					return true;
				}
			}
			return false;
		}

		public bool CheckInspectAnimal(Vector2 position, Farmer who)
		{
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				if ((bool)pair.Value.wasPet && pair.Value.GetCursorPetBoundingBox().Contains((int)position.X, (int)position.Y))
				{
					pair.Value.pet(who);
					return true;
				}
			}
			return false;
		}

		public bool CheckInspectAnimal(Microsoft.Xna.Framework.Rectangle rect, Farmer who)
		{
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				if ((bool)pair.Value.wasPet && pair.Value.GetBoundingBox().Intersects(rect))
				{
					pair.Value.pet(who);
					return true;
				}
			}
			return false;
		}

		public virtual void requestGrandpaReevaluation()
		{
			grandpaScore.Value = 0;
			if (Game1.IsMasterGame)
			{
				Game1.player.eventsSeen.Remove(558292);
				Game1.player.eventsSeen.Add(321777);
			}
			removeTemporarySpritesWithID(6666);
		}

		public override void OnMapLoad(Map map)
		{
			CacheOffBasePatioArea();
			base.OnMapLoad(map);
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
			if (!objects.ContainsKey(new Vector2(tileLocation.X, tileLocation.Y)) && CheckPetAnimal(rect, who))
			{
				return true;
			}
			Point grandpaShrinePosition = GetGrandpaShrinePosition();
			if (tileLocation.X >= grandpaShrinePosition.X - 1 && tileLocation.X <= grandpaShrinePosition.X + 1 && tileLocation.Y == grandpaShrinePosition.Y)
			{
				if (!hasSeenGrandpaNote)
				{
					Game1.addMail("hasSeenGrandpaNote", noLetter: true);
					hasSeenGrandpaNote = true;
					Game1.activeClickableMenu = new LetterViewerMenu(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaNote", Game1.player.Name).Replace('\n', '^'));
					return true;
				}
				if (Game1.year >= 3 && (int)grandpaScore > 0 && (int)grandpaScore < 4)
				{
					if (who.ActiveObject != null && (int)who.ActiveObject.parentSheetIndex == 72 && (int)grandpaScore < 4)
					{
						who.reduceActiveItemByOne();
						playSound("stoneStep");
						playSound("fireball");
						DelayedAction.playSoundAfterDelay("yoba", 800, this);
						DelayedAction.showDialogueAfterDelay(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaShrine_PlaceDiamond"), 1200);
						Game1.multiplayer.broadcastGrandpaReevaluation();
						Game1.player.freezePause = 1200;
						return true;
					}
					if (who.ActiveObject == null || (int)who.ActiveObject.parentSheetIndex != 72)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaShrine_DiamondSlot"));
						return true;
					}
				}
				else
				{
					if ((int)grandpaScore >= 4 && !Utility.doesItemWithThisIndexExistAnywhere(160, bigCraftable: true))
					{
						who.addItemByMenuIfNecessaryElseHoldUp(new Object(Vector2.Zero, 160), grandpaStatueCallback);
						return true;
					}
					if ((int)grandpaScore == 0 && Game1.year >= 3)
					{
						Game1.player.eventsSeen.Remove(558292);
						if (!Game1.player.eventsSeen.Contains(321777))
						{
							Game1.player.eventsSeen.Add(321777);
						}
					}
				}
			}
			if (base.checkAction(tileLocation, viewport, who))
			{
				return true;
			}
			if (Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true) && CheckInspectAnimal(rect, who))
			{
				return true;
			}
			return false;
		}

		public void grandpaStatueCallback(Item item, Farmer who)
		{
			if (item != null && item is Object && (bool)(item as Object).bigCraftable && (int)(item as Object).parentSheetIndex == 160)
			{
				who?.mailReceived.Add("grandpaPerfect");
			}
		}

		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			base.TransferDataFromSavedLocation(l);
			housePaintColor.Value = (l as Farm).housePaintColor.Value;
			farmCaveReady.Value = (l as Farm).farmCaveReady.Value;
			if ((l as Farm).hasSeenGrandpaNote)
			{
				Game1.addMail("hasSeenGrandpaNote", noLetter: true);
			}
		}

		public NetCollection<Item> getShippingBin(Farmer who)
		{
			if ((bool)Game1.player.team.useSeparateWallets)
			{
				return who.personalShippingBin;
			}
			return sharedShippingBin;
		}

		public void shipItem(Item i, Farmer who)
		{
			if (i != null)
			{
				who.removeItemFromInventory(i);
				getShippingBin(who).Add(i);
				if (i is Object)
				{
					showShipment(i as Object, playThrowSound: false);
				}
				lastItemShipped = i;
				if (Game1.player.ActiveObject == null)
				{
					Game1.player.showNotCarrying();
					Game1.player.Halt();
				}
			}
		}

		public override bool leftClick(int x, int y, Farmer who)
		{
			return base.leftClick(x, y, who);
		}

		public void showShipment(Object o, bool playThrowSound = true)
		{
			if (playThrowSound)
			{
				localSound("backpackIN");
			}
			DelayedAction.playSoundAfterDelay("Ship", playThrowSound ? 250 : 0);
			int num = Game1.random.Next();
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(524, 218, 34, 22), new Vector2(71f, 13f) * 64f + new Vector2(0f, 5f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = 100f,
				totalNumberOfLoops = 1,
				animationLength = 3,
				pingPong = true,
				scale = 4f,
				layerDepth = 0.09601f,
				id = num,
				extraInfoForEndBehavior = num,
				endFunction = base.removeTemporarySpritesWithID
			});
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(524, 230, 34, 10), new Vector2(71f, 13f) * 64f + new Vector2(0f, 17f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = 100f,
				totalNumberOfLoops = 1,
				animationLength = 3,
				pingPong = true,
				scale = 4f,
				layerDepth = 0.0963f,
				id = num,
				extraInfoForEndBehavior = num
			});
			temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, o.parentSheetIndex, 16, 16), new Vector2(71f, 13f) * 64f + new Vector2(8 + Game1.random.Next(6), 2f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = 9999f,
				scale = 4f,
				alphaFade = 0.045f,
				layerDepth = 0.096225f,
				motion = new Vector2(0f, 0.3f),
				acceleration = new Vector2(0f, 0.2f),
				scaleChange = -0.05f
			});
		}

		public override int getFishingLocation(Vector2 tile)
		{
			switch (Game1.whichFarm)
			{
			case 3:
				return 0;
			case 1:
			case 2:
			case 5:
				return 1;
			default:
				return -1;
			}
		}

		public override bool doesTileSinkDebris(int tileX, int tileY, Debris.DebrisType type)
		{
			if (isTileBuildingFishable(tileX, tileY))
			{
				return true;
			}
			return base.doesTileSinkDebris(tileX, tileY, type);
		}

		public override bool CanRefillWateringCanOnTile(int tileX, int tileY)
		{
			Vector2 tile = new Vector2(tileX, tileY);
			Building buildingAt = getBuildingAt(tile);
			if (buildingAt != null && buildingAt.CanRefillWateringCan())
			{
				return true;
			}
			return base.CanRefillWateringCanOnTile(tileX, tileY);
		}

		public override bool isTileBuildingFishable(int tileX, int tileY)
		{
			Vector2 tile = new Vector2(tileX, tileY);
			foreach (Building building in buildings)
			{
				if (building.isTileFishable(tile))
				{
					return true;
				}
			}
			return base.isTileBuildingFishable(tileX, tileY);
		}

		public override Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string location = null)
		{
			if (location != null && location != base.Name)
			{
				return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, location);
			}
			if (bobberTile != Vector2.Zero)
			{
				foreach (Building building in buildings)
				{
					if (building is FishPond && building.isTileFishable(bobberTile))
					{
						return (building as FishPond).CatchFish();
					}
				}
			}
			if (_fishLocationOverride == null)
			{
				string mapProperty = getMapProperty("FarmFishLocationOverride");
				if (mapProperty == "")
				{
					_fishLocationOverride = "";
					_fishChanceOverride = 0f;
				}
				else
				{
					string[] array = mapProperty.Split(' ');
					try
					{
						_fishLocationOverride = array[0];
						_fishChanceOverride = float.Parse(array[1]);
					}
					catch (Exception)
					{
						_fishLocationOverride = "";
						_fishChanceOverride = 0f;
					}
				}
			}
			if (_fishChanceOverride > 0f)
			{
				if (Game1.random.NextDouble() < (double)_fishChanceOverride)
				{
					return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, _fishLocationOverride);
				}
			}
			else
			{
				if (Game1.whichFarm == 1)
				{
					if (Game1.random.NextDouble() < 0.3)
					{
						return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "Forest");
					}
					return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "Town");
				}
				if (Game1.whichFarm == 6)
				{
					if (who != null && who.getTileLocation().Equals(new Vector2(23f, 98f)) && !who.mailReceived.Contains("gotBoatPainting"))
					{
						who.mailReceived.Add("gotBoatPainting");
						return new Furniture(2421, Vector2.Zero);
					}
					if (!new Microsoft.Xna.Framework.Rectangle(26, 45, 31, 39).Contains((int)bobberTile.X, (int)bobberTile.Y))
					{
						if (Game1.random.NextDouble() < 0.15)
						{
							return new Object(152, 1);
						}
						if (Game1.random.NextDouble() < 0.06)
						{
							int parentSheetIndex = -1;
							switch (Game1.random.Next(4))
							{
							case 0:
								parentSheetIndex = 723;
								break;
							case 1:
								parentSheetIndex = 393;
								break;
							case 2:
								parentSheetIndex = 719;
								break;
							case 3:
								parentSheetIndex = 718;
								break;
							}
							return new Object(parentSheetIndex, 1);
						}
						if (Game1.random.NextDouble() < 0.66)
						{
							return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "Beach");
						}
					}
				}
				else if (Game1.whichFarm == 5)
				{
					if (who != null && who.getTileX() < 40 && who.getTileY() > 54 && Game1.random.NextDouble() <= 0.5)
					{
						if (who.mailReceived.Contains("cursed_doll") && !who.mailReceived.Contains("eric's_prank_1"))
						{
							who.mailReceived.Add("eric's_prank_1");
							return new Object(103, 1);
						}
						return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "Forest");
					}
				}
				else if (Game1.whichFarm == 3)
				{
					if (Game1.random.NextDouble() < 0.5)
					{
						return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "Forest");
					}
				}
				else if (Game1.whichFarm == 2)
				{
					if (Game1.random.NextDouble() < 0.05 + Game1.player.DailyLuck)
					{
						return new Object(734, 1);
					}
					if (Game1.random.NextDouble() < 0.45)
					{
						return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "Forest");
					}
				}
				else if (Game1.whichFarm == 4 && Game1.random.NextDouble() <= 0.35)
				{
					return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "Mountain");
				}
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile);
		}

		public List<FarmAnimal> getAllFarmAnimals()
		{
			List<FarmAnimal> list = animals.Values.ToList();
			foreach (Building building in buildings)
			{
				if (building.indoors.Value != null && building.indoors.Value is AnimalHouse)
				{
					list.AddRange(((AnimalHouse)(GameLocation)building.indoors).animals.Values.ToList());
				}
			}
			return list;
		}

		public override bool isTileOccupied(Vector2 tileLocation, string characterToIgnore = "", bool ignoreAllCharacters = false)
		{
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				if (pair.Value.getTileLocation().Equals(tileLocation))
				{
					return true;
				}
			}
			return base.isTileOccupied(tileLocation, characterToIgnore, ignoreAllCharacters);
		}

		protected override void resetSharedState()
		{
			base.resetSharedState();
			if (!greenhouseUnlocked.Value && Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccPantry"))
			{
				greenhouseUnlocked.Value = true;
			}
			houseSource.Value = new Microsoft.Xna.Framework.Rectangle(0, 144 * (((int)Game1.MasterPlayer.houseUpgradeLevel == 3) ? 2 : ((int)Game1.MasterPlayer.houseUpgradeLevel)), 160, 144);
			for (int num = characters.Count - 1; num >= 0; num--)
			{
				if (Game1.timeOfDay >= 1300 && characters[num].isMarried() && characters[num].controller == null)
				{
					characters[num].Halt();
					characters[num].drawOffset.Value = Vector2.Zero;
					characters[num].Sprite.StopAnimation();
					FarmHouse farmHouse = Game1.getLocationFromName(characters[num].getSpouse().homeLocation.Value) as FarmHouse;
					Game1.warpCharacter(characters[num], characters[num].getSpouse().homeLocation.Value, farmHouse.getKitchenStandingSpot());
					break;
				}
			}
		}

		public virtual void UpdatePatio()
		{
			if (Game1.MasterPlayer.isMarried() && Game1.MasterPlayer.spouse != null)
			{
				addSpouseOutdoorArea(Game1.MasterPlayer.spouse);
			}
			else
			{
				addSpouseOutdoorArea("");
			}
		}

		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			ClearGreenhouseGrassTiles();
			UpdatePatio();
			_UpdateWaterBowl();
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			hasSeenGrandpaNote = Game1.player.hasOrWillReceiveMail("hasSeenGrandpaNote");
			frameHouseColor = null;
			if (!Game1.player.mailReceived.Contains("button_tut_2"))
			{
				Game1.player.mailReceived.Add("button_tut_2");
				Game1.onScreenMenus.Add(new ButtonTutorialMenu(1));
			}
			for (int num = characters.Count - 1; num >= 0; num--)
			{
				if (characters[num] is Child)
				{
					(characters[num] as Child).resetForPlayerEntry(this);
				}
				if (characters[num].isVillager() && characters[num].name.Equals(Game1.player.spouse))
				{
					petBowlWatered.Set(newValue: true);
				}
			}
			if (Game1.timeOfDay >= 1830)
			{
				for (int num2 = animals.Count() - 1; num2 >= 0; num2--)
				{
					animals.Pairs.ElementAt(num2).Value.warpHome(this, animals.Pairs.ElementAt(num2).Value);
				}
			}
			if (isThereABuildingUnderConstruction() && (int)getBuildingUnderConstruction().daysOfConstructionLeft > 0 && Game1.getCharacterFromName("Robin").currentLocation.Equals(this))
			{
				Building buildingUnderConstruction = getBuildingUnderConstruction();
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(399, 262, ((int)buildingUnderConstruction.daysOfConstructionLeft == 1) ? 29 : 9, 43), new Vector2((int)buildingUnderConstruction.tileX + (int)buildingUnderConstruction.tilesWide / 2, (int)buildingUnderConstruction.tileY + (int)buildingUnderConstruction.tilesHigh / 2) * 64f + new Vector2(-16f, -144f), flipped: false, 0f, Color.White)
				{
					id = 16846f,
					scale = 4f,
					interval = 999999f,
					animationLength = 1,
					totalNumberOfLoops = 99999,
					layerDepth = (float)(((int)buildingUnderConstruction.tileY + (int)buildingUnderConstruction.tilesHigh / 2) * 64 + 32) / 10000f
				});
			}
			else
			{
				removeTemporarySpritesWithIDLocal(16846f);
			}
			addGrandpaCandles();
			if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && !Game1.player.mailReceived.Contains("Farm_Eternal_Parrots") && !Game1.IsRainingHere(this))
			{
				for (int i = 0; i < 20; i++)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Microsoft.Xna.Framework.Rectangle(49, 24 * Game1.random.Next(4), 24, 24), new Vector2(Game1.viewport.MaxCorner.X, Game1.viewport.Location.Y + Game1.random.Next(64, Game1.viewport.Height / 2)), flipped: false, 0f, Color.White)
					{
						scale = 4f,
						motion = new Vector2(-5f + (float)Game1.random.Next(-10, 11) / 10f, 4f + (float)Game1.random.Next(-10, 11) / 10f),
						acceleration = new Vector2(0f, -0.02f),
						animationLength = 3,
						interval = 100f,
						pingPong = true,
						totalNumberOfLoops = 999,
						delayBeforeAnimationStart = i * 250,
						drawAboveAlwaysFront = true,
						startSound = "batFlap"
					});
				}
				DelayedAction.playSoundAfterDelay("parrot_squawk", 1000);
				DelayedAction.playSoundAfterDelay("parrot_squawk", 4000);
				DelayedAction.playSoundAfterDelay("parrot", 3000);
				DelayedAction.playSoundAfterDelay("parrot", 5500);
				DelayedAction.playSoundAfterDelay("parrot_squawk", 7000);
				for (int j = 0; j < 20; j++)
				{
					DelayedAction.playSoundAfterDelay("batFlap", 5000 + j * 250);
				}
				Game1.player.mailReceived.Add("Farm_Eternal_Parrots");
			}
		}

		public virtual Vector2 GetSpouseOutdoorAreaCorner()
		{
			if (!mapSpouseAreaCorner.HasValue)
			{
				int default_x = 69;
				int default_y = 6;
				if (Game1.whichFarm == 6)
				{
					default_x = 79;
					default_y = 2;
				}
				Point mapPropertyPosition = GetMapPropertyPosition("SpouseAreaLocation", default_x, default_y);
				mapSpouseAreaCorner = Utility.PointToVector2(mapPropertyPosition);
			}
			return mapSpouseAreaCorner.Value;
		}

		public virtual int GetSpouseOutdoorAreaSpritesheetIndex()
		{
			return 1;
		}

		public virtual void CacheOffBasePatioArea()
		{
			_baseSpouseAreaTiles = new Dictionary<string, Dictionary<Point, Tile>>();
			List<string> list = new List<string>();
			foreach (Layer layer2 in map.Layers)
			{
				list.Add(layer2.Id);
			}
			foreach (string item in list)
			{
				Layer layer = map.GetLayer(item);
				Dictionary<Point, Tile> dictionary = new Dictionary<Point, Tile>();
				_baseSpouseAreaTiles[item] = dictionary;
				Vector2 spouseOutdoorAreaCorner = GetSpouseOutdoorAreaCorner();
				for (int i = (int)spouseOutdoorAreaCorner.X; i < (int)spouseOutdoorAreaCorner.X + 4; i++)
				{
					for (int j = (int)spouseOutdoorAreaCorner.Y; j < (int)spouseOutdoorAreaCorner.Y + 4; j++)
					{
						if (layer == null)
						{
							dictionary[new Point(i, j)] = null;
						}
						else
						{
							dictionary[new Point(i, j)] = layer.Tiles[i, j];
						}
					}
				}
			}
		}

		public virtual void ReapplyBasePatioArea()
		{
			foreach (string key in _baseSpouseAreaTiles.Keys)
			{
				Layer layer = map.GetLayer(key);
				foreach (Point key2 in _baseSpouseAreaTiles[key].Keys)
				{
					Tile value = _baseSpouseAreaTiles[key][key2];
					if (layer != null)
					{
						layer.Tiles[key2.X, key2.Y] = value;
					}
				}
			}
		}

		public void addSpouseOutdoorArea(string spouseName)
		{
			ReapplyBasePatioArea();
			Point point = Utility.Vector2ToPoint(GetSpouseOutdoorAreaCorner());
			int spouseOutdoorAreaSpritesheetIndex = GetSpouseOutdoorAreaSpritesheetIndex();
			spousePatioSpot = new Point(point.X + 2, point.Y + 3);
			if (spouseName == null)
			{
				return;
			}
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\SpousePatios");
			int num = -1;
			string text = "spousePatios";
			if (dictionary != null && dictionary.ContainsKey(spouseName))
			{
				try
				{
					string text2 = dictionary[spouseName];
					string[] array = text2.Split('/');
					text = array[0];
					num = int.Parse(array[1]);
				}
				catch (Exception)
				{
				}
			}
			if (num < 0)
			{
				return;
			}
			int num2 = 4;
			int num3 = 4;
			Point point2 = point;
			Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(point2.X, point2.Y, num2, num3);
			Map map = Game1.game1.xTileContent.Load<Map>("Maps\\" + text);
			int num4 = map.Layers[0].LayerWidth / num2;
			int num5 = map.Layers[0].LayerHeight / num3;
			Point point3 = new Point(num % num4 * num2, num / num4 * num3);
			if (_appliedMapOverrides.Contains("spouse_patio"))
			{
				_appliedMapOverrides.Remove("spouse_patio");
			}
			ApplyMapOverride(text, "spouse_patio", new Microsoft.Xna.Framework.Rectangle(point3.X, point3.Y, value.Width, value.Height), value);
			bool flag = false;
			for (int i = value.Left; i < value.Right; i++)
			{
				for (int j = value.Top; j < value.Bottom; j++)
				{
					if (getTileIndexAt(new Point(i, j), "Paths") == 7)
					{
						spousePatioSpot = new Point(i, j);
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}

		public void addGrandpaCandles()
		{
			Point grandpaShrinePosition = GetGrandpaShrinePosition();
			if ((int)grandpaScore > 0)
			{
				Microsoft.Xna.Framework.Rectangle sourceRect = new Microsoft.Xna.Framework.Rectangle(577, 1985, 2, 5);
				removeTemporarySpritesWithIDLocal(6666f);
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, 99999f, 1, 9999, new Vector2((grandpaShrinePosition.X - 1) * 64 + 20, (grandpaShrinePosition.Y - 1) * 64 + 20), flicker: false, flipped: false, (float)((grandpaShrinePosition.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((grandpaShrinePosition.X - 1) * 64 + 12, (grandpaShrinePosition.Y - 1) * 64 - 4), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 7,
					light = true,
					id = 6666f,
					lightRadius = 1f,
					scale = 3f,
					layerDepth = 0.038500004f,
					delayBeforeAnimationStart = 0
				});
				if ((int)grandpaScore > 1)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, 99999f, 1, 9999, new Vector2((grandpaShrinePosition.X - 1) * 64 + 40, (grandpaShrinePosition.Y - 2) * 64 + 24), flicker: false, flipped: false, (float)((grandpaShrinePosition.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((grandpaShrinePosition.X - 1) * 64 + 36, (grandpaShrinePosition.Y - 2) * 64), flipped: false, 0f, Color.White)
					{
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 7,
						light = true,
						id = 6666f,
						lightRadius = 1f,
						scale = 3f,
						layerDepth = 0.038500004f,
						delayBeforeAnimationStart = 50
					});
				}
				if ((int)grandpaScore > 2)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, 99999f, 1, 9999, new Vector2((grandpaShrinePosition.X + 1) * 64 + 20, (grandpaShrinePosition.Y - 2) * 64 + 24), flicker: false, flipped: false, (float)((grandpaShrinePosition.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((grandpaShrinePosition.X + 1) * 64 + 16, (grandpaShrinePosition.Y - 2) * 64), flipped: false, 0f, Color.White)
					{
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 7,
						light = true,
						id = 6666f,
						lightRadius = 1f,
						scale = 3f,
						layerDepth = 0.038500004f,
						delayBeforeAnimationStart = 100
					});
				}
				if ((int)grandpaScore > 3)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, 99999f, 1, 9999, new Vector2((grandpaShrinePosition.X + 1) * 64 + 40, (grandpaShrinePosition.Y - 1) * 64 + 20), flicker: false, flipped: false, (float)((grandpaShrinePosition.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((grandpaShrinePosition.X + 1) * 64 + 36, (grandpaShrinePosition.Y - 1) * 64 - 4), flipped: false, 0f, Color.White)
					{
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 7,
						light = true,
						id = 6666f,
						lightRadius = 1f,
						scale = 3f,
						layerDepth = 0.038500004f,
						delayBeforeAnimationStart = 150
					});
				}
			}
			if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(176, 157, 15, 16), 99999f, 1, 9999, new Vector2(grandpaShrinePosition.X * 64 + 4, (grandpaShrinePosition.Y - 2) * 64 - 24), flicker: false, flipped: false, (float)((grandpaShrinePosition.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
			}
		}

		private void openShippingBinLid()
		{
			if (shippingBinLid != null)
			{
				if (shippingBinLid.pingPongMotion != 1 && Game1.currentLocation == this)
				{
					localSound("doorCreak");
				}
				shippingBinLid.pingPongMotion = 1;
				shippingBinLid.paused = false;
			}
		}

		private void closeShippingBinLid()
		{
			if (shippingBinLid != null && shippingBinLid.currentParentTileIndex > 0)
			{
				if (shippingBinLid.pingPongMotion != -1 && Game1.currentLocation == this)
				{
					localSound("doorCreakReverse");
				}
				shippingBinLid.pingPongMotion = -1;
				shippingBinLid.paused = false;
			}
		}

		private void updateShippingBinLid(GameTime time)
		{
			if (isShippingBinLidOpen(requiredToBeFullyOpen: true) && shippingBinLid.pingPongMotion == 1)
			{
				shippingBinLid.paused = true;
			}
			else if (shippingBinLid.currentParentTileIndex == 0 && shippingBinLid.pingPongMotion == -1)
			{
				if (!shippingBinLid.paused && Game1.currentLocation == this)
				{
					localSound("woodyStep");
				}
				shippingBinLid.paused = true;
			}
			shippingBinLid.update(time);
		}

		private bool isShippingBinLidOpen(bool requiredToBeFullyOpen = false)
		{
			if (shippingBinLid != null && shippingBinLid.currentParentTileIndex >= ((!requiredToBeFullyOpen) ? 1 : (shippingBinLid.animationLength - 1)))
			{
				return true;
			}
			return false;
		}

		public override void pokeTileForConstruction(Vector2 tile)
		{
			base.pokeTileForConstruction(tile);
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				if (pair.Value.getTileLocation().Equals(tile))
				{
					pair.Value.Poke();
				}
			}
			foreach (NPC character in characters)
			{
				if (character is Pet && character.getTileLocation() == tile)
				{
					Pet pet = character as Pet;
					pet.FacingDirection = Game1.random.Next(0, 4);
					pet.faceDirection(pet.FacingDirection);
					pet.CurrentBehavior = 0;
					pet.forceUpdateTimer = 2000;
					pet.setMovingInFacingDirection();
				}
			}
		}

		public override bool isTileOccupiedForPlacement(Vector2 tileLocation, Object toPlace = null)
		{
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				if (pair.Value.getTileLocation().Equals(tileLocation))
				{
					return true;
				}
			}
			return base.isTileOccupiedForPlacement(tileLocation, toPlace);
		}

		public override bool shouldShadowBeDrawnAboveBuildingsLayer(Vector2 p)
		{
			if (doesTileHaveProperty((int)p.X, (int)p.Y, "NoSpawn", "Back") == "All" && doesTileHaveProperty((int)p.X, (int)p.Y, "Type", "Back") == "Wood")
			{
				return true;
			}
			foreach (Building building in buildings)
			{
				if (building.occupiesTile(p) && building.isTilePassable(p))
				{
					return true;
				}
			}
			return base.shouldShadowBeDrawnAboveBuildingsLayer(p);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				pair.Value.draw(b);
			}
			Point mainFarmHouseEntry = GetMainFarmHouseEntry();
			Vector2 vector = Utility.PointToVector2(mainFarmHouseEntry) * 64f;
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(mainFarmHouseEntry.X - 5, mainFarmHouseEntry.Y + 2) * 64f), Building.leftShadow, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
			for (int i = 1; i < 8; i++)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(mainFarmHouseEntry.X - 5 + i, mainFarmHouseEntry.Y + 2) * 64f), Building.middleShadow, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
			}
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(mainFarmHouseEntry.X + 3, mainFarmHouseEntry.Y + 2) * 64f), Building.rightShadow, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
			Texture2D texture = houseTextures;
			if (paintedHouseTexture != null)
			{
				texture = paintedHouseTexture;
			}
			Color color = Color.White;
			if (frameHouseColor.HasValue)
			{
				color = frameHouseColor.Value;
				frameHouseColor = null;
			}
			Vector2 globalPosition = new Vector2(vector.X - 384f, vector.Y - 440f);
			b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, globalPosition), houseSource, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, (globalPosition.Y + 230f) / 10000f);
			if (Game1.mailbox.Count > 0)
			{
				float num = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				Point mailboxPosition = Game1.player.getMailboxPosition();
				float num2 = (float)((mailboxPosition.X + 1) * 64) / 10000f + (float)(mailboxPosition.Y * 64) / 10000f;
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(mailboxPosition.X * 64, (float)(mailboxPosition.Y * 64 - 96 - 48) + num)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, num2 + 1E-06f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(mailboxPosition.X * 64 + 32 + 4, (float)(mailboxPosition.Y * 64 - 64 - 24 - 8) + num)), new Microsoft.Xna.Framework.Rectangle(189, 423, 15, 13), Color.White, 0f, new Vector2(7f, 6f), 4f, SpriteEffects.None, num2 + 1E-05f);
			}
			if (shippingBinLid != null)
			{
				shippingBinLid.draw(b);
			}
			if (!hasSeenGrandpaNote)
			{
				Point grandpaShrinePosition = GetGrandpaShrinePosition();
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((grandpaShrinePosition.X + 1) * 64, grandpaShrinePosition.Y * 64)), new Microsoft.Xna.Framework.Rectangle(575, 1972, 11, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.044800997f);
			}
		}

		public virtual Point GetMainMailboxPosition()
		{
			if (!mapMainMailboxPosition.HasValue)
			{
				mapMainMailboxPosition = GetMapPropertyPosition("MailboxLocation", 68, 16);
			}
			return mapMainMailboxPosition.Value;
		}

		public virtual Point GetGrandpaShrinePosition()
		{
			if (!mapGrandpaShrinePosition.HasValue)
			{
				mapGrandpaShrinePosition = GetMapPropertyPosition("GrandpaShrineLocation", 8, 7);
			}
			return mapGrandpaShrinePosition.Value;
		}

		public virtual Point GetMainFarmHouseEntry()
		{
			if (!mainFarmhouseEntry.HasValue)
			{
				mainFarmhouseEntry = GetMapPropertyPosition("FarmHouseEntry", 64, 15);
			}
			return mainFarmhouseEntry.Value;
		}

		public override void startEvent(Event evt)
		{
			if (evt.id != -2)
			{
				Point mainFarmHouseEntry = GetMainFarmHouseEntry();
				int num = mainFarmHouseEntry.X - 64;
				int num2 = mainFarmHouseEntry.Y - 15;
				evt.eventPositionTileOffset = new Vector2(num, num2);
			}
			base.startEvent(evt);
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
		}

		public virtual void ApplyHousePaint()
		{
			if (paintedHouseTexture != null)
			{
				paintedHouseTexture.Dispose();
				paintedHouseTexture = null;
			}
			paintedHouseTexture = BuildingPainter.Apply(houseTextures, "Buildings\\houses_PaintMask", housePaintColor);
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
		{
			spawnCrowEvent.Poll();
			lightningStrikeEvent.Poll();
			housePaintColor.Value.Poll(_ApplyHousePaint);
			base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
			if (!Game1.currentLocation.Equals(this))
			{
				NetDictionary<long, FarmAnimal, NetRef<FarmAnimal>, SerializableDictionary<long, FarmAnimal>, NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>>.PairsCollection pairs = animals.Pairs;
				for (int num = pairs.Count() - 1; num >= 0; num--)
				{
					pairs.ElementAt(num).Value.updateWhenNotCurrentLocation(null, time, this);
				}
			}
		}

		public bool isTileOpenBesidesTerrainFeatures(Vector2 tile)
		{
			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64);
			foreach (Building building in buildings)
			{
				if (building.intersects(rectangle))
				{
					return false;
				}
			}
			foreach (ResourceClump resourceClump in resourceClumps)
			{
				if (resourceClump.getBoundingBox(resourceClump.tile).Intersects(rectangle))
				{
					return false;
				}
			}
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				if (pair.Value.getTileLocation().Equals(tile))
				{
					return true;
				}
			}
			if (!objects.ContainsKey(tile))
			{
				return isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport);
			}
			return false;
		}

		private void doLightningStrike(LightningStrikeEvent lightning)
		{
			if (lightning.smallFlash)
			{
				if (Game1.currentLocation.IsOutdoors && !(Game1.currentLocation is Desert) && !Game1.newDay && Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext()).isLightning.Value)
				{
					Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
					if (Game1.random.NextDouble() < 0.5)
					{
						DelayedAction.screenFlashAfterDelay((float)(0.3 + Game1.random.NextDouble()), Game1.random.Next(500, 1000));
					}
					DelayedAction.playSoundAfterDelay("thunder_small", Game1.random.Next(500, 1500));
				}
			}
			else if (lightning.bigFlash && Game1.currentLocation.IsOutdoors && !(Game1.currentLocation is Desert) && Game1.netWorldState.Value.GetWeatherForLocation(Game1.currentLocation.GetLocationContext()).isLightning.Value && !Game1.newDay)
			{
				Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
				Game1.playSound("thunder");
			}
			if (lightning.createBolt && Game1.currentLocation.name.Equals("Farm"))
			{
				if (lightning.destroyedTerrainFeature)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite(362, 75f, 6, 1, lightning.boltPosition, flicker: false, flipped: false));
				}
				Utility.drawLightningBolt(lightning.boltPosition, this);
			}
		}

		public override bool CanBeRemotedlyViewed()
		{
			return true;
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (wasUpdated && Game1.gameMode != 0)
			{
				return;
			}
			base.UpdateWhenCurrentLocation(time);
			chimneyTimer -= time.ElapsedGameTime.Milliseconds;
			if (chimneyTimer <= 0)
			{
				FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(Game1.MasterPlayer);
				if (homeOfFarmer != null && homeOfFarmer.hasActiveFireplace())
				{
					Point porchStandingSpot = homeOfFarmer.getPorchStandingSpot();
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(porchStandingSpot.X * 64 + 4 * (((int)Game1.MasterPlayer.houseUpgradeLevel >= 2) ? 9 : (-5)), porchStandingSpot.Y * 64 - 420), flipped: false, 0.002f, Color.Gray)
					{
						alpha = 0.75f,
						motion = new Vector2(0f, -0.5f),
						acceleration = new Vector2(0.002f, 0f),
						interval = 99999f,
						layerDepth = 1f,
						scale = 2f,
						scaleChange = 0.02f,
						rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f
					});
				}
				for (int i = 0; i < buildings.Count; i++)
				{
					if (buildings[i].indoors.Value is Cabin && (buildings[i].indoors.Value as Cabin).hasActiveFireplace())
					{
						temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(((int)buildings[i].tileX + 4) * 64 + -20, ((int)buildings[i].tileY + 3) * 64 - 420), flipped: false, 0.002f, Color.Gray)
						{
							alpha = 0.75f,
							motion = new Vector2(0f, -0.5f),
							acceleration = new Vector2(0.002f, 0f),
							interval = 99999f,
							layerDepth = 1f,
							scale = 2f,
							scaleChange = 0.02f,
							rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f
						});
					}
				}
				chimneyTimer = 500;
			}
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				_tempAnimals.Add(pair);
			}
			foreach (KeyValuePair<long, FarmAnimal> tempAnimal in _tempAnimals)
			{
				if (tempAnimal.Value.updateWhenCurrentLocation(time, this))
				{
					animals.Remove(tempAnimal.Key);
				}
			}
			_tempAnimals.Clear();
			if (shippingBinLid == null)
			{
				return;
			}
			bool flag = false;
			foreach (Farmer farmer in farmers)
			{
				if (farmer.GetBoundingBox().Intersects(shippingBinLidOpenArea))
				{
					openShippingBinLid();
					flag = true;
				}
			}
			if (!flag)
			{
				closeShippingBinLid();
			}
			updateShippingBinLid(time);
		}

		public int getTotalCrops()
		{
			int num = 0;
			foreach (TerrainFeature value in terrainFeatures.Values)
			{
				if (value is HoeDirt && (value as HoeDirt).crop != null && !(value as HoeDirt).crop.dead)
				{
					num++;
				}
			}
			return num;
		}

		public int getTotalCropsReadyForHarvest()
		{
			int num = 0;
			foreach (TerrainFeature value in terrainFeatures.Values)
			{
				if (value is HoeDirt && (value as HoeDirt).readyForHarvest())
				{
					num++;
				}
			}
			return num;
		}

		public int getTotalUnwateredCrops()
		{
			int num = 0;
			foreach (TerrainFeature value in terrainFeatures.Values)
			{
				if (value is HoeDirt && (value as HoeDirt).crop != null && (value as HoeDirt).needsWatering() && (int)(value as HoeDirt).state != 1)
				{
					num++;
				}
			}
			return num;
		}

		public int getTotalGreenhouseCropsReadyForHarvest()
		{
			if (Game1.MasterPlayer.mailReceived.Contains("ccPantry"))
			{
				int num = 0;
				{
					foreach (TerrainFeature value in Game1.getLocationFromName("Greenhouse").terrainFeatures.Values)
					{
						if (value is HoeDirt && (value as HoeDirt).readyForHarvest())
						{
							num++;
						}
					}
					return num;
				}
			}
			return -1;
		}

		private GreenhouseBuilding getGreenhouseBuilding()
		{
			foreach (Building building in buildings)
			{
				if (building is GreenhouseBuilding)
				{
					return building as GreenhouseBuilding;
				}
			}
			return null;
		}

		public int getTotalOpenHoeDirt()
		{
			int num = 0;
			foreach (TerrainFeature value in terrainFeatures.Values)
			{
				if (value is HoeDirt && (value as HoeDirt).crop == null && !objects.ContainsKey(value.currentTileLocation))
				{
					num++;
				}
			}
			return num;
		}

		public bool ShouldSpawnMountainOres()
		{
			if (!_mountainForageRectangle.HasValue)
			{
				Microsoft.Xna.Framework.Rectangle value = default(Microsoft.Xna.Framework.Rectangle);
				string mapProperty = getMapProperty("SpawnMountainFarmOreRect");
				string[] array = mapProperty.Split(' ');
				if (array.Length == 4)
				{
					try
					{
						value.X = int.Parse(array[0]);
						value.Y = int.Parse(array[1]);
						value.Width = int.Parse(array[2]);
						value.Height = int.Parse(array[3]);
					}
					catch (Exception)
					{
						value.X = 0;
						value.Y = 0;
						value.Width = 0;
						value.Height = 0;
					}
				}
				_mountainForageRectangle = value;
			}
			return _mountainForageRectangle.Value.Width > 0;
		}

		public bool ShouldSpawnForestFarmForage()
		{
			if (map != null)
			{
				if (!_shouldSpawnForestFarmForage.HasValue)
				{
					_shouldSpawnForestFarmForage = map.Properties.ContainsKey("SpawnForestFarmForage");
				}
				if (_shouldSpawnForestFarmForage.Value)
				{
					return true;
				}
			}
			return Game1.whichFarm == 2;
		}

		public bool ShouldSpawnBeachFarmForage()
		{
			if (map != null)
			{
				if (!_shouldSpawnBeachFarmForage.HasValue)
				{
					_shouldSpawnBeachFarmForage = map.Properties.ContainsKey("SpawnBeachFarmForage");
				}
				if (_shouldSpawnBeachFarmForage.Value)
				{
					return true;
				}
			}
			return Game1.whichFarm == 6;
		}

		public bool SpawnsForage()
		{
			if (!ShouldSpawnForestFarmForage())
			{
				return ShouldSpawnBeachFarmForage();
			}
			return true;
		}

		public int getTotalForageItems()
		{
			int num = 0;
			foreach (Object value in objects.Values)
			{
				if ((bool)value.isSpawnedObject)
				{
					num++;
				}
			}
			return num;
		}

		public int getNumberOfMachinesReadyForHarvest()
		{
			int num = 0;
			foreach (Object value in objects.Values)
			{
				if (value.IsConsideredReadyMachineForComputer())
				{
					num++;
				}
			}
			foreach (Object value2 in Game1.getLocationFromName("FarmHouse").objects.Values)
			{
				if (value2.IsConsideredReadyMachineForComputer())
				{
					num++;
				}
			}
			foreach (Building building in buildings)
			{
				if (building.indoors.Value == null)
				{
					continue;
				}
				foreach (Object value3 in building.indoors.Value.objects.Values)
				{
					if (value3.IsConsideredReadyMachineForComputer())
					{
						num++;
					}
				}
			}
			return num;
		}

		public bool doesFarmCaveNeedHarvesting()
		{
			return farmCaveReady.Value;
		}
	}
}
