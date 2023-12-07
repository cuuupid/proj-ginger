using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData.Movies;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Mobile;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewValley.Util;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley
{
	[XmlInclude(typeof(Farm))]
	[XmlInclude(typeof(Beach))]
	[XmlInclude(typeof(AnimalHouse))]
	[XmlInclude(typeof(SlimeHutch))]
	[XmlInclude(typeof(Shed))]
	[XmlInclude(typeof(LibraryMuseum))]
	[XmlInclude(typeof(AdventureGuild))]
	[XmlInclude(typeof(Woods))]
	[XmlInclude(typeof(Railroad))]
	[XmlInclude(typeof(Summit))]
	[XmlInclude(typeof(Forest))]
	[XmlInclude(typeof(ShopLocation))]
	[XmlInclude(typeof(SeedShop))]
	[XmlInclude(typeof(FishShop))]
	[XmlInclude(typeof(BathHousePool))]
	[XmlInclude(typeof(FarmHouse))]
	[XmlInclude(typeof(Cabin))]
	[XmlInclude(typeof(Club))]
	[XmlInclude(typeof(BusStop))]
	[XmlInclude(typeof(CommunityCenter))]
	[XmlInclude(typeof(Desert))]
	[XmlInclude(typeof(FarmCave))]
	[XmlInclude(typeof(JojaMart))]
	[XmlInclude(typeof(MineShaft))]
	[XmlInclude(typeof(Mountain))]
	[XmlInclude(typeof(Sewer))]
	[XmlInclude(typeof(WizardHouse))]
	[XmlInclude(typeof(Town))]
	[XmlInclude(typeof(Cellar))]
	[XmlInclude(typeof(Submarine))]
	[XmlInclude(typeof(MermaidHouse))]
	[XmlInclude(typeof(BeachNightMarket))]
	[XmlInclude(typeof(MovieTheater))]
	[XmlInclude(typeof(ManorHouse))]
	[XmlInclude(typeof(AbandonedJojaMart))]
	[XmlInclude(typeof(Mine))]
	[XmlInclude(typeof(BoatTunnel))]
	[XmlInclude(typeof(IslandWest))]
	[XmlInclude(typeof(IslandEast))]
	[XmlInclude(typeof(IslandSouth))]
	[XmlInclude(typeof(IslandNorth))]
	[XmlInclude(typeof(IslandSouthEast))]
	[XmlInclude(typeof(IslandSouthEastCave))]
	[XmlInclude(typeof(IslandFarmCave))]
	[XmlInclude(typeof(IslandWestCave1))]
	[XmlInclude(typeof(IslandFieldOffice))]
	[XmlInclude(typeof(IslandHut))]
	[XmlInclude(typeof(IslandFarmHouse))]
	[XmlInclude(typeof(IslandSecret))]
	[XmlInclude(typeof(IslandShrine))]
	[XmlInclude(typeof(IslandLocation))]
	[XmlInclude(typeof(IslandForestLocation))]
	[XmlInclude(typeof(Caldera))]
	[XmlInclude(typeof(BugLand))]
	[InstanceStatics]
	public class GameLocation : INetObject<NetFields>, IEquatable<GameLocation>
	{
		public enum LocationContext
		{
			Default,
			Island,
			MAX
		}

		public delegate void afterQuestionBehavior(Farmer who, string whichAnswer);

		private struct DamagePlayersEventArg : NetEventArg
		{
			public Microsoft.Xna.Framework.Rectangle Area;

			public int Damage;

			public void Read(BinaryReader reader)
			{
				Area = reader.ReadRectangle();
				Damage = reader.ReadInt32();
			}

			public void Write(BinaryWriter writer)
			{
				writer.WriteRectangle(Area);
				writer.Write(Damage);
			}
		}

		public const int minDailyWeeds = 5;

		public const int maxDailyWeeds = 12;

		public const int minDailyObjectSpawn = 1;

		public const int maxDailyObjectSpawn = 4;

		public const int maxSpawnedObjectsAtOnce = 6;

		public const int maxTriesForDebrisPlacement = 3;

		public const int maxTriesForObjectSpawn = 6;

		public const double chanceForStumpOrBoulderRespawn = 0.2;

		public const double chanceForClay = 0.03;

		public const string OVERRIDE_MAP_TILESHEET_PREFIX = "zzzzz";

		public const string PHONE_DIAL_SOUND = "telephone_buttonPush";

		public const int PHONE_RING_DURATION = 4950;

		public const string PHONE_PICKUP_SOUND = "bigSelect";

		public const string PHONE_HANGUP_SOUND = "openBox";

		public const int forageDataIndex = 0;

		public const int fishDataIndex = 4;

		public const int diggablesDataIndex = 8;

		[XmlIgnore]
		public string seasonOverride;

		[XmlIgnore]
		public LocationContext locationContext = (LocationContext)(-1);

		[XmlIgnore]
		public afterQuestionBehavior afterQuestion;

		[XmlIgnore]
		public Map map;

		[XmlIgnore]
		public readonly NetString mapPath = new NetString().Interpolated(interpolate: false, wait: false);

		[XmlIgnore]
		protected string loadedMapPath;

		public readonly NetCollection<NPC> characters = new NetCollection<NPC>();

		[XmlIgnore]
		public readonly NetVector2Dictionary<Object, NetRef<Object>> netObjects = new NetVector2Dictionary<Object, NetRef<Object>>();

		[XmlIgnore]
		public readonly Dictionary<Vector2, Object> overlayObjects = new Dictionary<Vector2, Object>(tilePositionComparer);

		[XmlElement("objects")]
		public readonly OverlaidDictionary objects;

		private readonly List<Object> tempObjects = new List<Object>();

		[XmlIgnore]
		public NetList<MapSeat, NetRef<MapSeat>> mapSeats = new NetList<MapSeat, NetRef<MapSeat>>();

		protected bool _mapSeatsDirty;

		[XmlIgnore]
		private List<KeyValuePair<Vector2, Object>> _objectUpdateList = new List<KeyValuePair<Vector2, Object>>();

		[XmlIgnore]
		public List<TemporaryAnimatedSprite> temporarySprites = new List<TemporaryAnimatedSprite>();

		[XmlIgnore]
		public List<Action> postFarmEventOvernightActions = new List<Action>();

		[XmlIgnore]
		public readonly NetObjectList<Warp> warps = new NetObjectList<Warp>();

		[XmlIgnore]
		public readonly NetPointDictionary<string, NetString> doors = new NetPointDictionary<string, NetString>();

		[XmlIgnore]
		public readonly InteriorDoorDictionary interiorDoors;

		[XmlIgnore]
		public readonly FarmerCollection farmers;

		[XmlIgnore]
		public readonly NetCollection<Projectile> projectiles = new NetCollection<Projectile>();

		public readonly NetCollection<ResourceClump> resourceClumps = new NetCollection<ResourceClump>();

		public readonly NetCollection<LargeTerrainFeature> largeTerrainFeatures = new NetCollection<LargeTerrainFeature>();

		[XmlIgnore]
		public List<TerrainFeature> _activeTerrainFeatures = new List<TerrainFeature>();

		[XmlIgnore]
		public List<Critter> critters;

		[XmlElement("terrainFeatures")]
		public readonly NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>> terrainFeatures = new NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>();

		[XmlIgnore]
		public readonly NetCollection<Debris> debris = new NetCollection<Debris>();

		[XmlIgnore]
		public readonly NetPoint fishSplashPoint = new NetPoint(Point.Zero);

		[XmlIgnore]
		public readonly NetPoint orePanPoint = new NetPoint(Point.Zero);

		[XmlIgnore]
		public TemporaryAnimatedSprite fishSplashAnimation;

		[XmlIgnore]
		public TemporaryAnimatedSprite orePanAnimation;

		[XmlIgnore]
		public WaterTiles waterTiles;

		[XmlIgnore]
		protected HashSet<string> _appliedMapOverrides;

		[XmlElement("uniqueName")]
		public readonly NetString uniqueName = new NetString();

		[XmlElement("name")]
		public readonly NetString name = new NetString();

		[XmlElement("waterColor")]
		public readonly NetColor waterColor = new NetColor(Color.White * 0.33f);

		[XmlIgnore]
		public string lastQuestionKey;

		[XmlIgnore]
		public Vector2 lastTouchActionLocation = Vector2.Zero;

		[XmlElement("lightLevel")]
		protected readonly NetFloat lightLevel = new NetFloat(0f);

		[XmlElement("isFarm")]
		public readonly NetBool isFarm = new NetBool();

		[XmlElement("isOutdoors")]
		public readonly NetBool isOutdoors = new NetBool();

		[XmlIgnore]
		public readonly NetBool isGreenhouse = new NetBool();

		[XmlElement("isStructure")]
		public readonly NetBool isStructure = new NetBool();

		[XmlElement("ignoreDebrisWeather")]
		public readonly NetBool ignoreDebrisWeather = new NetBool();

		[XmlElement("ignoreOutdoorLighting")]
		public readonly NetBool ignoreOutdoorLighting = new NetBool();

		[XmlElement("ignoreLights")]
		public readonly NetBool ignoreLights = new NetBool();

		[XmlElement("treatAsOutdoors")]
		public readonly NetBool treatAsOutdoors = new NetBool();

		[XmlIgnore]
		public bool wasUpdated;

		private List<Vector2> terrainFeaturesToRemoveList = new List<Vector2>();

		public int numberOfSpawnedObjectsOnMap;

		[XmlIgnore]
		public bool showDropboxIndicator;

		[XmlIgnore]
		public Vector2 dropBoxIndicatorLocation;

		[XmlElement("miniJukeboxCount")]
		public readonly NetInt miniJukeboxCount = new NetInt();

		[XmlElement("miniJukeboxTrack")]
		public readonly NetString miniJukeboxTrack = new NetString("");

		[XmlIgnore]
		public readonly NetString randomMiniJukeboxTrack = new NetString();

		[XmlIgnore]
		public Event currentEvent;

		[XmlIgnore]
		public Object actionObjectForQuestionDialogue;

		[XmlIgnore]
		public int waterAnimationIndex;

		[XmlIgnore]
		public int waterAnimationTimer;

		[XmlIgnore]
		public bool waterTileFlip;

		[XmlIgnore]
		public bool forceViewportPlayerFollow;

		[XmlIgnore]
		public bool forceLoadPathLayerLights;

		[XmlIgnore]
		public float waterPosition;

		[XmlIgnore]
		public readonly NetAudio netAudio;

		[XmlIgnore]
		public readonly NetIntDictionary<LightSource, NetRef<LightSource>> sharedLights = new NetIntDictionary<LightSource, NetRef<LightSource>>();

		private readonly NetEvent1Field<float, NetFloat> removeTemporarySpritesWithIDEvent = new NetEvent1Field<float, NetFloat>();

		private readonly NetEvent1Field<int, NetInt> rumbleAndFadeEvent = new NetEvent1Field<int, NetInt>();

		private readonly NetEvent1<DamagePlayersEventArg> damagePlayersEvent = new NetEvent1<DamagePlayersEventArg>();

		[XmlIgnore]
		public NetList<Vector2, NetVector2> lightGlows = new NetList<Vector2, NetVector2>();

		public static readonly int JOURNAL_INDEX = 1000;

		public static readonly float FIRST_SECRET_NOTE_CHANCE = 0.8f;

		public static readonly float LAST_SECRET_NOTE_CHANCE = 0.12f;

		public static readonly int NECKLACE_SECRET_NOTE_INDEX = 25;

		public static readonly int CAROLINES_NECKLACE_ITEM = 191;

		public static readonly string CAROLINES_NECKLACE_MAIL = "carolinesNecklace";

		public static TilePositionComparer tilePositionComparer = new TilePositionComparer();

		protected List<Vector2> _startingCabinLocations = new List<Vector2>();

		[XmlIgnore]
		public bool wasInhabited;

		[XmlIgnore]
		protected bool _madeMapModifications;

		[XmlIgnore]
		public ModDataDictionary modData = new ModDataDictionary();

		public readonly NetCollection<Furniture> furniture = new NetCollection<Furniture>();

		protected readonly NetMutexQueue<Guid> furnitureToRemove = new NetMutexQueue<Guid>();

		internal bool ignoreWarps;

		private Vector2 snowPos;

		private const int fireIDBase = 944468;

		private readonly List<Farmer> current_location_farmers = new List<Farmer>();

		internal static HashSet<int> secretNotesSeen = new HashSet<int>();

		internal static List<int> unseenSecretNotes = new List<int>();

		internal static HashSet<int> journalsSeen = new HashSet<int>();

		internal static List<int> unseenJournals = new List<int>();

		private static readonly char[] ForwardSlash = new char[1] { '/' };

		private long ticks = DateTime.Now.Ticks;

		private bool drawFrameOne = true;

		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields();


		[XmlIgnore]
		public NetRoot<GameLocation> Root => NetFields.Root as NetRoot<GameLocation>;

		[XmlIgnore]
		public string NameOrUniqueName
		{
			get
			{
				if (uniqueName.Value != null)
				{
					return uniqueName.Value;
				}
				return name.Value;
			}
		}

		[XmlIgnore]
		public float LightLevel
		{
			get
			{
				return lightLevel;
			}
			set
			{
				lightLevel.Value = value;
			}
		}

		[XmlIgnore]
		public Map Map
		{
			get
			{
				updateMap();
				return map;
			}
			set
			{
				map = value;
			}
		}

		[XmlIgnore]
		public OverlaidDictionary Objects => objects;

		[XmlIgnore]
		public List<TemporaryAnimatedSprite> TemporarySprites => temporarySprites;

		public string Name => name;

		[XmlIgnore]
		public bool IsFarm
		{
			get
			{
				return isFarm;
			}
			set
			{
				isFarm.Value = value;
			}
		}

		[XmlIgnore]
		public bool IsOutdoors
		{
			get
			{
				return isOutdoors;
			}
			set
			{
				isOutdoors.Value = value;
			}
		}

		[XmlIgnore]
		public TapToMove tapToMove { get; set; }

		public bool IsGreenhouse
		{
			get
			{
				return isGreenhouse;
			}
			set
			{
				isGreenhouse.Value = value;
			}
		}

		[XmlElement("modData")]
		public ModDataDictionary modDataForSerialization
		{
			get
			{
				return modData.GetForSerialization();
			}
			set
			{
				modData.SetFromSerialization(value);
			}
		}

		public virtual bool SeedsIgnoreSeasonsHere()
		{
			return IsGreenhouse;
		}

		public virtual bool CanPlantSeedsHere(int crop_index, int tile_x, int tile_y)
		{
			return IsGreenhouse;
		}

		public virtual bool CanPlantTreesHere(int sapling_index, int tile_x, int tile_y)
		{
			if (Object.isWildTreeSeed(sapling_index) && IsOutdoors)
			{
				string text = doesTileHavePropertyNoNull(tile_x, tile_y, "Type", "Back");
				if (text == "Dirt")
				{
					return true;
				}
			}
			bool flag = false;
			if (map != null && map.Properties.ContainsKey("ForceAllowTreePlanting"))
			{
				flag = true;
			}
			return IsGreenhouse || flag;
		}

		protected virtual void initNetFields()
		{
			NetFields.AddFields(mapPath, uniqueName, name, lightLevel, sharedLights, isFarm, isOutdoors, isStructure, ignoreDebrisWeather, ignoreOutdoorLighting, ignoreLights, treatAsOutdoors, warps, doors, interiorDoors, waterColor, netObjects, projectiles, largeTerrainFeatures, terrainFeatures, characters, debris, netAudio.NetFields, removeTemporarySpritesWithIDEvent, rumbleAndFadeEvent, damagePlayersEvent, lightGlows, fishSplashPoint, orePanPoint, isGreenhouse, miniJukeboxCount, miniJukeboxTrack, randomMiniJukeboxTrack, resourceClumps);
			NetFields.AddFields(furniture, furnitureToRemove.NetFields);
			NetFields.AddField(mapSeats);
			NetFields.AddField(modData);
			sharedLights.OnValueAdded += delegate(int identifier, LightSource light)
			{
				if (Game1.currentLocation == this)
				{
					Game1.currentLightSources.Add(light);
				}
			};
			sharedLights.OnValueRemoved += delegate(int identifier, LightSource light)
			{
				if (Game1.currentLocation == this)
				{
					Game1.currentLightSources.Remove(light);
				}
			};
			netObjects.OnConflictResolve += delegate(Vector2 pos, NetRef<Object> rejected, NetRef<Object> accepted)
			{
				if (Game1.IsMasterGame)
				{
					Object value = rejected.Value;
					if (value != null)
					{
						value.NetFields.Parent = null;
						value.dropItem(this, pos * 64f, pos * 64f);
					}
				}
			};
			removeTemporarySpritesWithIDEvent.onEvent += removeTemporarySpritesWithIDLocal;
			rumbleAndFadeEvent.onEvent += performRumbleAndFade;
			damagePlayersEvent.onEvent += performDamagePlayers;
			fishSplashPoint.fieldChangeVisibleEvent += delegate
			{
				updateFishSplashAnimation();
			};
			orePanPoint.fieldChangeVisibleEvent += delegate
			{
				updateOrePanAnimation();
			};
			characters.OnValueRemoved += delegate(NPC npc)
			{
				npc.Removed();
			};
			terrainFeatures.OnValueAdded += delegate(Vector2 pos, TerrainFeature tf)
			{
				if (tf is Flooring)
				{
					(tf as Flooring).OnAdded(this, pos);
				}
				else if (tf is HoeDirt)
				{
					(tf as HoeDirt).OnAdded(this, pos);
				}
				OnTerrainFeatureAdded(tf, pos);
			};
			terrainFeatures.OnValueRemoved += delegate(Vector2 pos, TerrainFeature tf)
			{
				if (tf is Flooring)
				{
					(tf as Flooring).OnRemoved(this, pos);
				}
				else if (tf is HoeDirt)
				{
					(tf as HoeDirt).OnRemoved(this, pos);
				}
				OnTerrainFeatureRemoved(tf);
			};
			largeTerrainFeatures.OnValueAdded += delegate(LargeTerrainFeature tf)
			{
				OnTerrainFeatureAdded(tf, tf.currentTileLocation);
			};
			largeTerrainFeatures.OnValueRemoved += delegate(LargeTerrainFeature tf)
			{
				OnTerrainFeatureRemoved(tf);
			};
			furniture.InterpolationWait = false;
			furniture.OnValueAdded += delegate(Furniture f)
			{
				f.OnAdded(this, f.TileLocation);
			};
			furnitureToRemove.Processor = removeQueuedFurniture;
		}

		public virtual void InvalidateCachedMultiplayerMap(Dictionary<string, CachedMultiplayerMap> cached_data)
		{
			if (!Game1.IsMasterGame && cached_data.ContainsKey(NameOrUniqueName))
			{
				cached_data.Remove(NameOrUniqueName);
			}
		}

		public virtual void MakeMapModifications(bool force = false)
		{
			if (force)
			{
				_appliedMapOverrides.Clear();
			}
			interiorDoors.MakeMapModifications();
			string text = name;
			if (text == null)
			{
				return;
			}
			switch (text.Length)
			{
			case 10:
				switch (text[0])
				{
				case 'W':
					if (text == "WitchSwamp")
					{
						if (Game1.MasterPlayer.mailReceived.Contains("henchmanGone"))
						{
							removeTile(20, 29, "Buildings");
						}
						else
						{
							setMapTileIndex(20, 29, 10, "Buildings");
						}
					}
					break;
				case 'H':
					if (text == "HaleyHouse" && Game1.player.eventsSeen.Contains(463391) && (Game1.player.spouse == null || !Game1.player.spouse.Equals("Emily")))
					{
						setMapTileIndex(14, 4, 2173, "Buildings");
						setMapTileIndex(14, 3, 2141, "Buildings");
						setMapTileIndex(14, 3, 219, "Back");
					}
					break;
				}
				break;
			case 7:
				if (text == "Sunroom")
				{
					string imageSource = map.TileSheets[1].ImageSource;
					string fileName = Path.GetFileName(imageSource);
					string text2 = Path.GetDirectoryName(imageSource);
					if (string.IsNullOrWhiteSpace(text2))
					{
						text2 = "Maps";
					}
					map.TileSheets[1].ImageSource = Path.Combine(text2, "CarolineGreenhouseTiles" + ((Game1.IsRainingHere(this) || Game1.timeOfDay > Game1.getTrulyDarkTime()) ? "_rainy" : ""));
					map.DisposeTileSheets(Game1.mapDisplayDevice);
					map.LoadTileSheets(Game1.mapDisplayDevice);
				}
				break;
			case 17:
				if (text == "AbandonedJojaMart")
				{
					if (!Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
					{
						StaticTile[] junimoNoteTileFrames = CommunityCenter.getJunimoNoteTileFrames(0, map);
						string layerId = "Buildings";
						Point point = new Point(8, 8);
						map.GetLayer(layerId).Tiles[point.X, point.Y] = new AnimatedTile(map.GetLayer(layerId), junimoNoteTileFrames, 70L);
					}
					else
					{
						removeTile(8, 8, "Buildings");
					}
				}
				break;
			case 8:
				if (text == "WitchHut" && Game1.player.mailReceived.Contains("hasPickedUpMagicInk"))
				{
					setMapTileIndex(4, 11, 113, "Buildings");
					map.GetLayer("Buildings").Tiles[4, 11].Properties.Remove("Action");
				}
				break;
			case 6:
				if (text == "Saloon" && NetWorldState.checkAnywhereForWorldStateID("saloonSportsRoom"))
				{
					refurbishMapPortion(new Microsoft.Xna.Framework.Rectangle(32, 1, 7, 9), "RefurbishedSaloonRoom", Point.Zero);
					Game1.currentLightSources.Add(new LightSource(1, new Vector2(33f, 7f) * 64f, 4f, LightSource.LightContext.None, 0L));
					Game1.currentLightSources.Add(new LightSource(1, new Vector2(36f, 7f) * 64f, 4f, LightSource.LightContext.None, 0L));
					Game1.currentLightSources.Add(new LightSource(1, new Vector2(34f, 5f) * 64f, 4f, LightSource.LightContext.None, 0L));
				}
				break;
			case 9:
			{
				if (!(text == "Backwoods"))
				{
					break;
				}
				if (Game1.netWorldState.Value.hasWorldStateID("golemGrave"))
				{
					ApplyMapOverride("Backwoods_GraveSite");
				}
				if (!Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts") || _appliedMapOverrides.Contains("Backwoods_Staircase"))
				{
					break;
				}
				ApplyMapOverride("Backwoods_Staircase");
				LargeTerrainFeature largeTerrainFeature = null;
				foreach (LargeTerrainFeature largeTerrainFeature2 in largeTerrainFeatures)
				{
					if (largeTerrainFeature2.tilePosition.Equals(new Vector2(37f, 16f)))
					{
						largeTerrainFeature = largeTerrainFeature2;
						break;
					}
				}
				if (largeTerrainFeature != null)
				{
					largeTerrainFeatures.Remove(largeTerrainFeature);
				}
				break;
			}
			}
		}

		public virtual bool ApplyCachedMultiplayerMap(Dictionary<string, CachedMultiplayerMap> cached_data, string requested_map_path)
		{
			if (Game1.IsMasterGame)
			{
				return false;
			}
			if (cached_data.ContainsKey(NameOrUniqueName))
			{
				CachedMultiplayerMap cachedMultiplayerMap = cached_data[NameOrUniqueName];
				if (cachedMultiplayerMap.mapPath == requested_map_path)
				{
					_appliedMapOverrides = cachedMultiplayerMap.appliedMapOverrides;
					map = cachedMultiplayerMap.map;
					loadedMapPath = cachedMultiplayerMap.loadedMapPath;
					return true;
				}
				cached_data.Remove(NameOrUniqueName);
				return false;
			}
			return false;
		}

		public virtual void StoreCachedMultiplayerMap(Dictionary<string, CachedMultiplayerMap> cached_data)
		{
			if (!Game1.IsMasterGame && !(this is VolcanoDungeon) && !(this is MineShaft))
			{
				CachedMultiplayerMap cachedMultiplayerMap = new CachedMultiplayerMap();
				cachedMultiplayerMap.map = map;
				cachedMultiplayerMap.appliedMapOverrides = _appliedMapOverrides;
				cachedMultiplayerMap.mapPath = mapPath;
				cachedMultiplayerMap.loadedMapPath = loadedMapPath;
				cached_data[NameOrUniqueName] = cachedMultiplayerMap;
			}
		}

		public virtual void TransferDataFromSavedLocation(GameLocation l)
		{
			modData.Clear();
			if (l.modData != null)
			{
				foreach (string key in l.modData.Keys)
				{
					modData[key] = l.modData[key];
				}
			}
			SelectRandomMiniJukeboxTrack();
			UpdateMapSeats();
		}

		public virtual void OnTerrainFeatureAdded(TerrainFeature feature, Vector2 location)
		{
			if (feature != null)
			{
				feature.currentLocation = this;
				feature.currentTileLocation = location;
				feature.OnAddedToLocation(this, location);
				UpdateTerrainFeatureUpdateSubscription(feature);
			}
		}

		public virtual void OnTerrainFeatureRemoved(TerrainFeature feature)
		{
			if (feature != null)
			{
				if (feature.NeedsUpdate)
				{
					_activeTerrainFeatures.Remove(feature);
				}
				feature.currentLocation = null;
			}
		}

		public virtual void UpdateTerrainFeatureUpdateSubscription(TerrainFeature feature)
		{
			if (feature.NeedsUpdate)
			{
				_activeTerrainFeatures.Add(feature);
			}
			else
			{
				_activeTerrainFeatures.Remove(feature);
			}
		}

		public string GetSeasonForLocation()
		{
			if (seasonOverride == null)
			{
				if (map == null && mapPath.Value != null)
				{
					reloadMap();
				}
				if (map != null)
				{
					seasonOverride = null;
					PropertyValue value = null;
					if (map.Properties.TryGetValue("SeasonOverride", out value))
					{
						seasonOverride = value.ToString();
					}
					else
					{
						seasonOverride = "";
					}
				}
			}
			if (seasonOverride != null && seasonOverride.Length == 0)
			{
				return Game1.currentSeason;
			}
			return seasonOverride;
		}

		public bool isTemp()
		{
			if (!Name.StartsWith("Temp") && !Name.Equals("fishingGame"))
			{
				return Name.Equals("tent");
			}
			return true;
		}

		private void updateFishSplashAnimation()
		{
			if (fishSplashPoint.Value == Point.Zero)
			{
				fishSplashAnimation = null;
			}
			else
			{
				fishSplashAnimation = new TemporaryAnimatedSprite(51, new Vector2(fishSplashPoint.X * 64, fishSplashPoint.Y * 64), Color.White, 10, flipped: false, 80f, 999999);
			}
		}

		private void updateOrePanAnimation()
		{
			if (orePanPoint.Value == Point.Zero)
			{
				orePanAnimation = null;
				return;
			}
			orePanAnimation = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), new Vector2(orePanPoint.X * 64 + 32, orePanPoint.Y * 64 + 32), flipped: false, 0f, Color.White)
			{
				totalNumberOfLoops = 9999999,
				interval = 100f,
				scale = 3f,
				animationLength = 6
			};
		}

		public GameLocation()
		{
			farmers = new FarmerCollection(this);
			interiorDoors = new InteriorDoorDictionary(this);
			netAudio = new NetAudio(this);
			objects = new OverlaidDictionary(netObjects, overlayObjects);
			_appliedMapOverrides = new HashSet<string>();
			terrainFeatures.SetEqualityComparer(tilePositionComparer);
			netObjects.SetEqualityComparer(tilePositionComparer);
			objects.SetEqualityComparer(tilePositionComparer, ref netObjects, ref overlayObjects);
			initNetFields();
		}

		public GameLocation(string mapPath, string name)
			: this()
		{
			this.mapPath.Set(mapPath);
			this.name.Value = name;
			if (name.Contains("Farm") || name.Contains("Coop") || name.Contains("Barn") || name.Equals("SlimeHutch"))
			{
				isFarm.Value = true;
			}
			if (name == "Greenhouse")
			{
				IsGreenhouse = true;
			}
			reloadMap();
			loadObjects();
			tapToMove = new TapToMove(this);
		}

		public void playSound(string audioName, NetAudio.SoundContext soundContext = NetAudio.SoundContext.Default)
		{
			netAudio.Play(audioName, soundContext);
		}

		public void playSoundPitched(string audioName, int pitch, NetAudio.SoundContext soundContext = NetAudio.SoundContext.Default)
		{
			netAudio.PlayPitched(audioName, Vector2.Zero, pitch, soundContext);
		}

		public void playSoundAt(string audioName, Vector2 position, NetAudio.SoundContext soundContext = NetAudio.SoundContext.Default)
		{
			netAudio.PlayAt(audioName, position, soundContext);
		}

		public void localSound(string audioName)
		{
			netAudio.PlayLocal(audioName);
		}

		public void localSoundAt(string audioName, Vector2 position)
		{
			netAudio.PlayLocalAt(audioName, position);
		}

		private bool doorHasStateOpen(Point door)
		{
			bool value;
			return interiorDoors.TryGetValue(door, out value) && value;
		}

		protected virtual LocalizedContentManager getMapLoader()
		{
			return Game1.game1.xTileContent;
		}

		public void ApplyMapOverride(Map override_map, string override_key, Microsoft.Xna.Framework.Rectangle? source_rect = null, Microsoft.Xna.Framework.Rectangle? dest_rect = null)
		{
			if (_appliedMapOverrides.Contains(override_key))
			{
				return;
			}
			_appliedMapOverrides.Add(override_key);
			updateSeasonalTileSheets(override_map);
			Dictionary<TileSheet, TileSheet> dictionary = new Dictionary<TileSheet, TileSheet>();
			foreach (TileSheet tileSheet2 in override_map.TileSheets)
			{
				TileSheet tileSheet = map.GetTileSheet(tileSheet2.Id);
				string text = "";
				string text2 = "";
				if (tileSheet != null)
				{
					text = tileSheet.ImageSource;
				}
				if (text2 != null)
				{
					text2 = tileSheet2.ImageSource;
				}
				if (tileSheet == null || text2 != text)
				{
					string id = "zzzzz_" + override_key + "_" + tileSheet2.Id;
					tileSheet = new TileSheet(id, map, tileSheet2.ImageSource, tileSheet2.SheetSize, tileSheet2.TileSize);
					for (int i = 0; i < tileSheet2.TileCount; i++)
					{
						tileSheet.TileIndexProperties[i].CopyFrom(tileSheet2.TileIndexProperties[i]);
					}
					map.AddTileSheet(tileSheet);
				}
				else if (tileSheet.TileCount < tileSheet2.TileCount)
				{
					int tileCount = tileSheet.TileCount;
					tileSheet.SheetWidth = tileSheet2.SheetWidth;
					tileSheet.SheetHeight = tileSheet2.SheetHeight;
					for (int j = tileCount; j < tileSheet2.TileCount; j++)
					{
						tileSheet.TileIndexProperties[j].CopyFrom(tileSheet2.TileIndexProperties[j]);
					}
				}
				dictionary[tileSheet2] = tileSheet;
			}
			Dictionary<Layer, Layer> dictionary2 = new Dictionary<Layer, Layer>();
			int num = 0;
			int num2 = 0;
			for (int k = 0; k < override_map.Layers.Count; k++)
			{
				num = Math.Max(num, override_map.Layers[k].LayerWidth);
				num2 = Math.Max(num2, override_map.Layers[k].LayerHeight);
			}
			if (!source_rect.HasValue)
			{
				source_rect = new Microsoft.Xna.Framework.Rectangle(0, 0, num, num2);
			}
			num = 0;
			num2 = 0;
			for (int l = 0; l < map.Layers.Count; l++)
			{
				num = Math.Max(num, map.Layers[l].LayerWidth);
				num2 = Math.Max(num2, map.Layers[l].LayerHeight);
			}
			for (int m = 0; m < override_map.Layers.Count; m++)
			{
				Layer layer = map.GetLayer(override_map.Layers[m].Id);
				if (layer == null)
				{
					layer = new Layer(override_map.Layers[m].Id, map, new Size(num, num2), override_map.Layers[m].TileSize);
					map.AddLayer(layer);
				}
				dictionary2[override_map.Layers[m]] = layer;
			}
			if (!dest_rect.HasValue)
			{
				dest_rect = new Microsoft.Xna.Framework.Rectangle(0, 0, num, num2);
			}
			int x = source_rect.Value.X;
			int y = source_rect.Value.Y;
			int x2 = dest_rect.Value.X;
			int y2 = dest_rect.Value.Y;
			for (int n = 0; n < source_rect.Value.Width; n++)
			{
				for (int num3 = 0; num3 < source_rect.Value.Height; num3++)
				{
					Point point = new Point(x + n, y + num3);
					Point point2 = new Point(x2 + n, y2 + num3);
					bool flag = false;
					for (int num4 = 0; num4 < override_map.Layers.Count; num4++)
					{
						Layer layer2 = override_map.Layers[num4];
						Layer layer3 = dictionary2[layer2];
						if (layer3 == null || point2.X >= layer3.LayerWidth || point2.Y >= layer3.LayerHeight || (!flag && override_map.Layers[num4].Tiles[point.X, point.Y] == null))
						{
							continue;
						}
						flag = true;
						if (point.X >= layer2.LayerWidth || point.Y >= layer2.LayerHeight)
						{
							continue;
						}
						if (layer2.Tiles[point.X, point.Y] == null)
						{
							layer3.Tiles[point2.X, point2.Y] = null;
							continue;
						}
						Tile tile = layer2.Tiles[point.X, point.Y];
						Tile tile2 = null;
						if (tile is StaticTile)
						{
							tile2 = new StaticTile(layer3, dictionary[tile.TileSheet], tile.BlendMode, tile.TileIndex);
						}
						else if (tile is AnimatedTile)
						{
							AnimatedTile animatedTile = tile as AnimatedTile;
							StaticTile[] array = new StaticTile[animatedTile.TileFrames.Length];
							for (int num5 = 0; num5 < animatedTile.TileFrames.Length; num5++)
							{
								StaticTile staticTile = animatedTile.TileFrames[num5];
								array[num5] = new StaticTile(layer3, dictionary[staticTile.TileSheet], staticTile.BlendMode, staticTile.TileIndex);
							}
							tile2 = new AnimatedTile(layer3, array, animatedTile.FrameInterval);
						}
						tile2?.Properties.CopyFrom(tile.Properties);
						layer3.Tiles[point2.X, point2.Y] = tile2;
					}
				}
			}
			map.LoadTileSheets(Game1.mapDisplayDevice);
			if (Game1.IsMasterGame)
			{
				_mapSeatsDirty = true;
			}
		}

		public virtual bool RunLocationSpecificEventCommand(Event current_event, string command_string, bool first_run, params string[] args)
		{
			return true;
		}

		public void ApplyMapOverride(string map_name, Microsoft.Xna.Framework.Rectangle? source_rect = null, Microsoft.Xna.Framework.Rectangle? destination_rect = null)
		{
			if (!_appliedMapOverrides.Contains(map_name))
			{
				Map override_map = Game1.game1.xTileContent.Load<Map>("Maps\\" + map_name);
				ApplyMapOverride(override_map, map_name, source_rect, destination_rect);
			}
		}

		public void ApplyMapOverride(string map_name, string override_key_name, Microsoft.Xna.Framework.Rectangle? source_rect = null, Microsoft.Xna.Framework.Rectangle? destination_rect = null)
		{
			if (!_appliedMapOverrides.Contains(override_key_name))
			{
				Map override_map = Game1.game1.xTileContent.Load<Map>("Maps\\" + map_name);
				ApplyMapOverride(override_map, override_key_name, source_rect, destination_rect);
			}
		}

		public virtual void UpdateMapSeats()
		{
			_mapSeatsDirty = false;
			if (!Game1.IsMasterGame)
			{
				return;
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			Dictionary<string, string> dictionary2 = Game1.content.Load<Dictionary<string, string>>("Data\\ChairTiles");
			mapSeats.Clear();
			Layer layer = map.GetLayer("Buildings");
			if (layer == null)
			{
				return;
			}
			for (int i = 0; i < layer.LayerWidth; i++)
			{
				for (int j = 0; j < layer.LayerHeight; j++)
				{
					Tile tile = layer.Tiles[i, j];
					if (tile == null)
					{
						continue;
					}
					string text = Path.GetFileNameWithoutExtension(tile.TileSheet.ImageSource);
					if (dictionary.ContainsKey(text))
					{
						text = dictionary[text];
					}
					else
					{
						if (text.StartsWith("summer_") || text.StartsWith("winter_") || text.StartsWith("fall_"))
						{
							text = "spring_" + text.Substring(text.IndexOf('_') + 1);
						}
						dictionary[text] = text;
					}
					int sheetWidth = tile.TileSheet.SheetWidth;
					int num = tile.TileIndex % sheetWidth;
					int num2 = tile.TileIndex / sheetWidth;
					string key = text + "/" + num + "/" + num2;
					if (dictionary2.ContainsKey(key))
					{
						string data = dictionary2[key];
						MapSeat mapSeat = MapSeat.FromData(data, i, j);
						if (mapSeat != null)
						{
							mapSeats.Add(mapSeat);
						}
					}
				}
			}
		}

		public virtual void OnMapLoad(Map map)
		{
		}

		public void loadMap(string mapPath, bool force_reload = false)
		{
			if (force_reload)
			{
				LocalizedContentManager localizedContentManager = Program.gamePtr.CreateContentManager(Game1.content.ServiceProvider, Game1.content.RootDirectory);
				map = localizedContentManager.Load<Map>(mapPath);
				localizedContentManager.Unload();
				InvalidateCachedMultiplayerMap(Game1.multiplayer.cachedMultiplayerMaps);
			}
			else if (!ApplyCachedMultiplayerMap(Game1.multiplayer.cachedMultiplayerMaps, mapPath))
			{
				map = getMapLoader().Load<Map>(mapPath);
			}
			loadedMapPath = mapPath;
			OnMapLoad(map);
			updateSeasonalTileSheets(map);
			map.LoadTileSheets(Game1.mapDisplayDevice);
			if (Game1.IsMasterGame)
			{
				_mapSeatsDirty = true;
			}
			map.Properties.TryGetValue("Outdoors", out var value);
			if (value != null)
			{
				isOutdoors.Value = true;
			}
			if (map.Properties.TryGetValue("IsFarm", out var value2) && value2 != null)
			{
				isFarm.Value = true;
			}
			if (map.Properties.TryGetValue("IsGreenhouse", out value2) && value2 != null)
			{
				isGreenhouse.Value = true;
			}
			map.Properties.TryGetValue("forceLoadPathLayerLights", out var value3);
			if (value3 != null)
			{
				forceLoadPathLayerLights = true;
			}
			map.Properties.TryGetValue("TreatAsOutdoors", out var value4);
			if (value4 != null)
			{
				treatAsOutdoors.Value = true;
			}
			bool flag = false;
			map.Properties.TryGetValue("indoorWater", out var value5);
			if (value5 != null)
			{
				flag = true;
			}
			if (((bool)isOutdoors || flag || this is Sewer || this is Submarine) && !(this is Desert))
			{
				waterTiles = new bool[map.Layers[0].LayerWidth, map.Layers[0].LayerHeight];
				bool flag2 = false;
				for (int i = 0; i < map.Layers[0].LayerWidth; i++)
				{
					for (int j = 0; j < map.Layers[0].LayerHeight; j++)
					{
						string text = doesTileHaveProperty(i, j, "Water", "Back");
						if (text != null)
						{
							flag2 = true;
							if (text == "I")
							{
								waterTiles.waterTiles[i, j] = new WaterTiles.WaterTileData(is_water: true, is_visible: false);
							}
							else
							{
								waterTiles[i, j] = true;
							}
						}
					}
				}
				if (!flag2)
				{
					waterTiles = null;
				}
			}
			if ((bool)isOutdoors)
			{
				critters = new List<Critter>();
			}
			loadLights();
		}

		public virtual void HandleGrassGrowth(int dayOfMonth)
		{
			if (dayOfMonth == 1)
			{
				if (this is Farm || getMapProperty("ClearEmptyDirtOnNewMonth") != "")
				{
					for (int num = terrainFeatures.Count() - 1; num >= 0; num--)
					{
						if (terrainFeatures.Pairs.ElementAt(num).Value is HoeDirt && (terrainFeatures.Pairs.ElementAt(num).Value as HoeDirt).crop == null && Game1.random.NextDouble() < 0.8)
						{
							terrainFeatures.Remove(terrainFeatures.Pairs.ElementAt(num).Key);
						}
					}
				}
				if (this is Farm || getMapProperty("SpawnDebrisOnNewMonth") != "")
				{
					spawnWeedsAndStones(20, weedsOnly: false, spawnFromOldWeeds: false);
				}
				if (Game1.currentSeason.Equals("spring") && Game1.stats.DaysPlayed > 1)
				{
					if (this is Farm || getMapProperty("SpawnDebrisOnNewYear") != "")
					{
						spawnWeedsAndStones(40, weedsOnly: false, spawnFromOldWeeds: false);
						spawnWeedsAndStones(40, weedsOnly: true, spawnFromOldWeeds: false);
					}
					if (this is Farm || getMapProperty("SpawnRandomGrassOnNewYear") != "")
					{
						for (int i = 0; i < 15; i++)
						{
							int num2 = Game1.random.Next(map.DisplayWidth / 64);
							int num3 = Game1.random.Next(map.DisplayHeight / 64);
							Vector2 vector = new Vector2(num2, num3);
							objects.TryGetValue(vector, out var value);
							if (value == null && doesTileHaveProperty(num2, num3, "Diggable", "Back") != null && doesTileHaveProperty(num2, num3, "NoSpawn", "Back") == null && isTileLocationOpen(new Location(num2, num3)) && !isTileOccupied(vector) && doesTileHaveProperty(num2, num3, "Water", "Back") == null)
							{
								terrainFeatures.Add(vector, new Grass(1, 4));
							}
						}
						growWeedGrass(40);
					}
					if (getMapProperty("SpawnGrassFromPathsOnNewYear") != null)
					{
						Layer layer = map.GetLayer("Paths");
						if (layer != null)
						{
							for (int j = 0; j < layer.LayerWidth; j++)
							{
								for (int k = 0; k < layer.LayerHeight; k++)
								{
									Vector2 vector2 = new Vector2(j, k);
									objects.TryGetValue(vector2, out var value2);
									if (value2 == null && getTileIndexAt(new Point(j, k), "Paths") == 22 && isTileLocationOpen(new Location(j, k)) && !isTileOccupied(vector2))
									{
										terrainFeatures.Add(vector2, new Grass(1, 4));
									}
								}
							}
						}
					}
				}
			}
			if ((this is Farm || getMapProperty("EnableGrassSpread") != "") && (Game1.currentSeason != "winter" || getMapProperty("AllowGrassGrowInWinter") != ""))
			{
				growWeedGrass(1);
			}
		}

		public void reloadMap()
		{
			if (mapPath != null)
			{
				loadMap(mapPath);
			}
			else
			{
				map = null;
			}
			loadedMapPath = mapPath;
			tapToMove?.Init(this);
		}

		public virtual bool canSlimeMateHere()
		{
			return true;
		}

		public virtual bool canSlimeHatchHere()
		{
			return true;
		}

		public void addCharacter(NPC character)
		{
			characters.Add(character);
		}

		public NetCollection<NPC> getCharacters()
		{
			return characters;
		}

		public static Microsoft.Xna.Framework.Rectangle getSourceRectForObject(int tileIndex)
		{
			return new Microsoft.Xna.Framework.Rectangle(tileIndex * 16 % Game1.objectSpriteSheet.Width, tileIndex * 16 / Game1.objectSpriteSheet.Width * 16, 16, 16);
		}

		public Warp isCollidingWithWarp(Microsoft.Xna.Framework.Rectangle position, Character character)
		{
			if (ignoreWarps)
			{
				return null;
			}
			foreach (Warp warp in warps)
			{
				if ((character is NPC || !warp.npcOnly.Value) && (warp.X == (int)Math.Floor((double)position.Left / 64.0) || warp.X == (int)Math.Floor((double)position.Right / 64.0)) && (warp.Y == (int)Math.Floor((double)position.Top / 64.0) || warp.Y == (int)Math.Floor((double)position.Bottom / 64.0)))
				{
					if (warp.TargetName == "BoatTunnel" && character is NPC)
					{
						return new Warp(warp.X, warp.Y, "IslandSouth", 17, 43, flipFarmer: false);
					}
					if (warp.TargetName == "VolcanoEntrance")
					{
						return new Warp(warp.X, warp.Y, "VolcanoDungeon0", warp.TargetX, warp.TargetY, flipFarmer: false);
					}
					return warp;
				}
			}
			return null;
		}

		public Warp isCollidingWithWarpOrDoor(Microsoft.Xna.Framework.Rectangle position, Character character = null)
		{
			Warp warp = isCollidingWithWarp(position, character);
			if (warp == null)
			{
				warp = isCollidingWithDoors(position, character);
			}
			return warp;
		}

		public virtual Warp isCollidingWithDoors(Microsoft.Xna.Framework.Rectangle position, Character character = null)
		{
			for (int i = 0; i < 4; i++)
			{
				Vector2 cornersOfThisRectangle = Utility.getCornersOfThisRectangle(ref position, i);
				Point point = new Point((int)cornersOfThisRectangle.X / 64, (int)cornersOfThisRectangle.Y / 64);
				foreach (KeyValuePair<Point, string> pair in doors.Pairs)
				{
					Point key = pair.Key;
					if (point.Equals(key))
					{
						return getWarpFromDoor(key, character);
					}
				}
			}
			return null;
		}

		public virtual Warp getWarpFromDoor(Point door, Character character = null)
		{
			string text = doesTileHaveProperty(door.X, door.Y, "Action", "Buildings");
			string[] array = text.Split(' ');
			if (array[0].Equals("WarpCommunityCenter"))
			{
				return new Warp(door.X, door.Y, "CommunityCenter", 32, 23, flipFarmer: false);
			}
			if (array[0].Equals("Warp_Sunroom_Door"))
			{
				return new Warp(door.X, door.Y, "Sunroom", 5, 13, flipFarmer: false);
			}
			if ((array[0].Equals("WarpBoatTunnel") || (array.Length > 3 && array[3].Equals("BoatTunnel"))) && character is NPC)
			{
				return new Warp(door.X, door.Y, "IslandSouth", 17, 43, flipFarmer: false);
			}
			return new Warp(door.X, door.Y, array[3], Convert.ToInt32(array[1]), Convert.ToInt32(array[2]), flipFarmer: false);
		}

		public void addResourceClumpAndRemoveUnderlyingTerrain(int resourceClumpIndex, int width, int height, Vector2 tile)
		{
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					removeEverythingExceptCharactersFromThisTile((int)tile.X + i, (int)tile.Y + j);
				}
			}
			resourceClumps.Add(new ResourceClump(resourceClumpIndex, width, height, tile));
		}

		public virtual bool canFishHere()
		{
			return true;
		}

		public virtual bool CanRefillWateringCanOnTile(int tileX, int tileY)
		{
			if (doesTileHaveProperty(tileX, tileY, "Water", "Back") == null && doesTileHaveProperty(tileX, tileY, "WaterSource", "Back") == null)
			{
				if (!isOutdoors && doesTileHavePropertyNoNull(tileX, tileY, "Action", "Buildings").Equals("kitchen"))
				{
					if (getTileIndexAt(tileX, tileY, "Buildings") != 172)
					{
						return getTileIndexAt(tileX, tileY, "Buildings") == 257;
					}
					return true;
				}
				return false;
			}
			return true;
		}

		public virtual bool isTileBuildingFishable(int tileX, int tileY)
		{
			return false;
		}

		public virtual bool isTileFishable(int tileX, int tileY)
		{
			if (isTileBuildingFishable(tileX, tileY))
			{
				return true;
			}
			if (doesTileHaveProperty(tileX, tileY, "Water", "Back") == null || doesTileHaveProperty(tileX, tileY, "NoFishing", "Back") != null || getTileIndexAt(tileX, tileY, "Buildings") != -1)
			{
				return doesTileHaveProperty(tileX, tileY, "Water", "Buildings") != null;
			}
			return true;
		}

		public bool isFarmerCollidingWithAnyCharacter()
		{
			foreach (NPC character in characters)
			{
				if (character != null && Game1.player.GetBoundingBox().Intersects(character.GetBoundingBox()))
				{
					return true;
				}
			}
			return false;
		}

		public bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer)
		{
			return isCollidingPosition(position, viewport, isFarmer, 0, glider: false, null, pathfinding: false);
		}

		public bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, Character character)
		{
			return isCollidingPosition(position, viewport, isFarmer: false, 0, glider: false, character, pathfinding: false);
		}

		public bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider)
		{
			return isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, null, pathfinding: false);
		}

		protected bool _TestCornersWorld(int top, int bottom, int left, int right, Func<int, int, bool> action)
		{
			if (action(right, top))
			{
				return true;
			}
			if (action(right, bottom))
			{
				return true;
			}
			if (action(left, top))
			{
				return true;
			}
			if (action(left, bottom))
			{
				return true;
			}
			return false;
		}

		protected bool _TestCornersWorld(int top, int bottom, int left, int right, int center, bool bigger_than_tile, Func<int, int, bool> action)
		{
			if (_TestCornersWorld(top, bottom, left, right, action))
			{
				return true;
			}
			if (bigger_than_tile)
			{
				if (action(center, top))
				{
					return true;
				}
				if (action(center, bottom))
				{
					return true;
				}
			}
			return false;
		}

		protected bool _TestCornersTiles(Vector2 top_right, Vector2 top_left, Vector2 bottom_right, Vector2 bottom_left, Vector2 top_mid, Vector2 bottom_mid, Vector2 player_top_right, Vector2 player_top_left, Vector2 player_bottom_right, Vector2 player_bottom_left, Vector2 player_top_mid, Vector2 player_bottom_mid, bool bigger_than_tile, Func<Vector2, Vector2, bool> action)
		{
			if (action(top_right, player_top_right))
			{
				return true;
			}
			if (action(top_left, player_top_left))
			{
				return true;
			}
			if (action(bottom_left, player_bottom_left))
			{
				return true;
			}
			if (action(bottom_right, player_bottom_right))
			{
				return true;
			}
			if (bigger_than_tile)
			{
				if (action(top_mid, player_top_mid))
				{
					return true;
				}
				if (action(bottom_mid, player_bottom_mid))
				{
					return true;
				}
			}
			return false;
		}

		public Furniture GetFurnitureAt(Vector2 tile_position)
		{
			Point value = default(Point);
			value.X = (int)((float)(int)tile_position.X + 0.5f) * 64;
			value.Y = (int)((float)(int)tile_position.Y + 0.5f) * 64;
			foreach (Furniture item in furniture)
			{
				if (item.getBoundingBox(item.TileLocation).Contains(value))
				{
					return item;
				}
			}
			return null;
		}

		public virtual bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			if (!Game1.eventUp && (character == null || character.willDestroyObjectsUnderfoot))
			{
				foreach (Furniture item in furniture)
				{
					if ((int)item.furniture_type != 12 && item.IntersectsForCollision(position) && (!isFarmer || !(character as Farmer).TemporaryPassableTiles.Intersects(position)))
					{
						return true;
					}
				}
			}
			return isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding: false);
		}

		public virtual bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false)
		{
			foreach (ResourceClump resourceClump in resourceClumps)
			{
				if (!glider && resourceClump.getBoundingBox(resourceClump.tile).Intersects(position))
				{
					return true;
				}
			}
			bool flag = Game1.eventUp;
			if (flag && Game1.CurrentEvent != null && !Game1.CurrentEvent.ignoreObjectCollisions)
			{
				flag = false;
			}
			updateMap();
			if (!isFarmer && Game1.currentMinigame != null && Game1.currentMinigame is TargetGame)
			{
				return false;
			}
			if (position.Right < 0 || position.X > map.Layers[0].DisplayWidth || position.Bottom < 0 || position.Top > map.Layers[0].DisplayHeight)
			{
				return false;
			}
			if (character == null && !ignoreCharacterRequirement)
			{
				return true;
			}
			Vector2 vector = new Vector2(position.Right / 64, position.Top / 64);
			Vector2 vector2 = new Vector2(position.Left / 64, position.Top / 64);
			Vector2 vector3 = new Vector2(position.Right / 64, position.Bottom / 64);
			Vector2 vector4 = new Vector2(position.Left / 64, position.Bottom / 64);
			bool bigger_than_tile = position.Width > 64;
			Vector2 bottom_mid = new Vector2(position.Center.X / 64, position.Bottom / 64);
			Vector2 top_mid = new Vector2(position.Center.X / 64, position.Top / 64);
			Microsoft.Xna.Framework.Rectangle boundingBox = Game1.player.GetBoundingBox();
			Vector2 player_top_right = new Vector2((boundingBox.Right - 1) / 64, boundingBox.Top / 64);
			Vector2 player_top_left = new Vector2(boundingBox.Left / 64, boundingBox.Top / 64);
			Vector2 player_bottom_right = new Vector2((boundingBox.Right - 1) / 64, (boundingBox.Bottom - 1) / 64);
			Vector2 player_bottom_left = new Vector2(boundingBox.Left / 64, (boundingBox.Bottom - 1) / 64);
			Vector2 player_bottom_mid = new Vector2(boundingBox.Center.X / 64, (boundingBox.Bottom - 1) / 64);
			Vector2 player_top_mid = new Vector2(boundingBox.Center.X / 64, boundingBox.Top / 64);
			if (character != null && isFarmer && character is Farmer)
			{
				Farmer farmer = character as Farmer;
				if (farmer.bridge != null && farmer.onBridge.Value && position.Right >= farmer.bridge.bridgeBounds.X && position.Left <= farmer.bridge.bridgeBounds.Right)
				{
					if (_TestCornersWorld(position.Top, position.Bottom, position.Left, position.Right, (int x, int y) => (y > farmer.bridge.bridgeBounds.Bottom || y < farmer.bridge.bridgeBounds.Top) ? true : false))
					{
						return true;
					}
					return false;
				}
			}
			if (!glider && (!flag || (character != null && !isFarmer && (!pathfinding || !character.willDestroyObjectsUnderfoot))) && _TestCornersTiles(vector, vector2, vector3, vector4, top_mid, bottom_mid, player_top_right, player_top_left, player_bottom_right, player_bottom_left, player_top_mid, player_bottom_mid, bigger_than_tile, delegate(Vector2 corner, Vector2 player_corner)
			{
				objects.TryGetValue(corner, out var value2);
				return (value2 != null && !value2.IsHoeDirt && !value2.isPassable() && !Game1.player.TemporaryPassableTiles.Intersects(value2.getBoundingBox(corner)) && value2.getBoundingBox(corner).Intersects(position) && character != null && (!(character is FarmAnimal) || !value2.isAnimalProduct()) && character.collideWith(value2) && (!isFarmer || corner != player_corner)) ? true : false;
			}))
			{
				return true;
			}
			if (!glider && !flag)
			{
				foreach (Furniture item in furniture)
				{
					if ((int)item.furniture_type != 12 && item.IntersectsForCollision(position) && (!isFarmer || !item.IntersectsForCollision(Game1.player.GetBoundingBox())))
					{
						return true;
					}
				}
			}
			if (largeTerrainFeatures != null && !glider)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					if (largeTerrainFeature.getBoundingBox().Intersects(position))
					{
						return true;
					}
				}
			}
			if (!flag && !glider && _TestCornersTiles(vector, vector2, vector3, vector4, top_mid, bottom_mid, player_top_right, player_top_left, player_bottom_right, player_bottom_left, player_top_mid, player_bottom_mid, bigger_than_tile, delegate(Vector2 corner, Vector2 player_corner)
			{
				if (terrainFeatures.ContainsKey(corner) && terrainFeatures[corner].getBoundingBox(corner).Intersects(position))
				{
					if (!pathfinding)
					{
						terrainFeatures[corner].doCollisionAction(position, Game1.player.speed + Game1.player.addedSpeed, corner, character, this);
					}
					if (terrainFeatures.ContainsKey(corner) && !terrainFeatures[corner].isPassable(character) && (!isFarmer || corner != player_corner))
					{
						return true;
					}
				}
				return false;
			}))
			{
				return true;
			}
			if (character != null && character.hasSpecialCollisionRules() && (character.isColliding(this, vector) || character.isColliding(this, vector2) || character.isColliding(this, vector3) || character.isColliding(this, vector4)))
			{
				return true;
			}
			if (((isFarmer && (currentEvent == null || currentEvent.playerControlSequence)) || (character != null && (bool)character.collidesWithOtherCharacters)) && !pathfinding)
			{
				for (int num = characters.Count - 1; num >= 0; num--)
				{
					NPC nPC = characters[num];
					if (nPC != null && (character == null || !character.Equals(nPC)))
					{
						Microsoft.Xna.Framework.Rectangle boundingBox2 = nPC.GetBoundingBox();
						if (nPC.layingDown)
						{
							boundingBox2.Y -= 64;
							boundingBox2.Height += 64;
						}
						if (boundingBox2.Intersects(position) && !Game1.player.temporarilyInvincible)
						{
							nPC.behaviorOnFarmerPushing();
						}
						if (isFarmer && !flag && !nPC.farmerPassesThrough && boundingBox2.Intersects(position) && !Game1.player.temporarilyInvincible && Game1.player.TemporaryPassableTiles.IsEmpty() && (!nPC.IsMonster || (!((Monster)nPC).isGlider && !Game1.player.GetBoundingBox().Intersects(nPC.GetBoundingBox()))) && !nPC.IsInvisible && !Game1.player.GetBoundingBox().Intersects(boundingBox2))
						{
							return true;
						}
						if (!isFarmer && boundingBox2.Intersects(position))
						{
							return true;
						}
					}
				}
			}
			Layer back_layer = map.GetLayer("Back");
			Layer buildings_layer = map.GetLayer("Buildings");
			if (isFarmer)
			{
				if (currentEvent != null && currentEvent.checkForCollision(position, (character != null) ? (character as Farmer) : Game1.player))
				{
					return true;
				}
				if (Game1.player.currentUpgrade != null && Game1.player.currentUpgrade.daysLeftTillUpgradeDone <= 3 && name.Equals("Farm") && position.Intersects(new Microsoft.Xna.Framework.Rectangle((int)Game1.player.currentUpgrade.positionOfCarpenter.X, (int)Game1.player.currentUpgrade.positionOfCarpenter.Y + 64, 64, 32)))
				{
					return true;
				}
			}
			else
			{
				if (!pathfinding && !(character is Monster) && damagesFarmer == 0 && !glider)
				{
					foreach (Farmer farmer2 in farmers)
					{
						if (position.Intersects(farmer2.GetBoundingBox()))
						{
							return true;
						}
					}
				}
				if (((bool)isFarm || name.Value.StartsWith("UndergroundMine") || this is IslandLocation) && character != null && !character.Name.Contains("NPC") && !character.eventActor && !glider)
				{
					PropertyValue barrier2 = null;
					Tile t3;
					if (_TestCornersWorld(position.Top, position.Bottom, position.Left, position.Right, delegate(int x, int y)
					{
						t3 = back_layer.PickTile(new Location(x, y), viewport.Size);
						if (t3 != null)
						{
							t3.Properties.TryGetValue("NPCBarrier", out barrier2);
							if (barrier2 != null)
							{
								return true;
							}
						}
						return false;
					}))
					{
						return true;
					}
				}
				if (glider && !projectile)
				{
					return false;
				}
			}
			if (!isFarmer || !Game1.player.isRafting)
			{
				PropertyValue barrier = null;
				Tile t2;
				if (_TestCornersWorld(position.Top, position.Bottom, position.Left, position.Right, delegate(int x, int y)
				{
					t2 = back_layer.PickTile(new Location(x, y), viewport.Size);
					if (t2 != null)
					{
						t2.Properties.TryGetValue("TemporaryBarrier", out barrier);
						if (barrier != null)
						{
							return true;
						}
					}
					return false;
				}))
				{
					return true;
				}
			}
			if (!isFarmer || !Game1.player.isRafting)
			{
				PropertyValue passable = null;
				if ((!(character is FarmAnimal) || !(character as FarmAnimal).IsActuallySwimming()) && _TestCornersWorld(position.Top, position.Bottom, position.Left, position.Right, position.Center.X, bigger_than_tile, delegate(int x, int y)
				{
					Tile tile2 = back_layer.PickTile(new Location(x, y), viewport.Size);
					if (tile2 != null)
					{
						tile2.TileIndexProperties.TryGetValue("Passable", out passable);
						if (passable == null)
						{
							tile2.Properties.TryGetValue("Passable", out passable);
						}
						if (passable != null && (!isFarmer || !Game1.player.TemporaryPassableTiles.Contains(x, y)))
						{
							return true;
						}
					}
					return false;
				}))
				{
					return true;
				}
				Tile tmp;
				if (_TestCornersWorld(position.Top, position.Bottom, position.Left, position.Right, position.Center.X, bigger_than_tile, delegate(int x, int y)
				{
					tmp = buildings_layer.PickTile(new Location(x, y), viewport.Size);
					if (tmp != null)
					{
						if (projectile && this is VolcanoDungeon)
						{
							Tile tile = back_layer.PickTile(new Location(x, y), viewport.Size);
							if (tile != null)
							{
								PropertyValue value = null;
								if (value == null)
								{
									tile.TileIndexProperties.TryGetValue("Water", out value);
								}
								if (value == null)
								{
									tile.Properties.TryGetValue("Water", out value);
								}
								if (value != null)
								{
									return false;
								}
							}
						}
						tmp.TileIndexProperties.TryGetValue("Shadow", out passable);
						if (passable == null)
						{
							tmp.TileIndexProperties.TryGetValue("Passable", out passable);
						}
						if (passable == null)
						{
							tmp.Properties.TryGetValue("Passable", out passable);
						}
						if (projectile)
						{
							if (passable == null)
							{
								tmp.TileIndexProperties.TryGetValue("ProjectilePassable", out passable);
							}
							if (passable == null)
							{
								tmp.Properties.TryGetValue("ProjectilePassable", out passable);
							}
						}
						if (passable == null && !isFarmer)
						{
							tmp.TileIndexProperties.TryGetValue("NPCPassable", out passable);
						}
						if (passable == null && !isFarmer)
						{
							tmp.Properties.TryGetValue("NPCPassable", out passable);
						}
						if (passable == null && !isFarmer && character != null && character.canPassThroughActionTiles())
						{
							tmp.Properties.TryGetValue("Action", out passable);
						}
						if ((passable == null || passable.ToString().Length == 0) && (!isFarmer || !Game1.player.TemporaryPassableTiles.Contains(x, y)))
						{
							if (character == null)
							{
								return true;
							}
							return character.shouldCollideWithBuildingLayer(this);
						}
					}
					return false;
				}))
				{
					return true;
				}
				if (!isFarmer && passable != null && (passable.ToString().StartsWith("Door ") || passable.ToString() == "Door"))
				{
					openDoor(new Location(position.Center.X / 64, position.Bottom / 64), playSound: false);
					openDoor(new Location(position.Center.X / 64, position.Top / 64), Game1.currentLocation.Equals(this));
				}
				return false;
			}
			PropertyValue passable2 = null;
			Tile t;
			if (_TestCornersWorld(position.Top, position.Bottom, position.Left, position.Right, delegate(int x, int y)
			{
				t = back_layer.PickTile(new Location(x, y), viewport.Size);
				if (t != null)
				{
					t.TileIndexProperties.TryGetValue("Water", out passable2);
				}
				if (passable2 == null)
				{
					if (isTileLocationOpen(new Location(x / 64, y / 64)) && !isTileOccupiedForPlacement(new Vector2(x / 64, y / 64)))
					{
						Game1.player.isRafting = false;
						Game1.player.Position = new Vector2(x / 64 * 64, y / 64 * 64 - 32);
						Game1.player.setTrajectory(0, 0);
					}
					return true;
				}
				return false;
			}))
			{
				return true;
			}
			return false;
		}

		public bool isTilePassable(Location tileLocation, xTile.Dimensions.Rectangle viewport)
		{
			PropertyValue value = null;
			Tile tile = map.GetLayer("Back").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
			tile?.TileIndexProperties.TryGetValue("Passable", out value);
			Tile tile2 = map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
			if (value == null && tile2 == null)
			{
				return tile != null;
			}
			return false;
		}

		public bool isPointPassable(Location location, xTile.Dimensions.Rectangle viewport)
		{
			PropertyValue value = null;
			PropertyValue value2 = null;
			map.GetLayer("Back").PickTile(new Location(location.X, location.Y), viewport.Size)?.TileIndexProperties.TryGetValue("Passable", out value);
			Tile tile = map.GetLayer("Buildings").PickTile(new Location(location.X, location.Y), viewport.Size);
			tile?.TileIndexProperties.TryGetValue("Shadow", out value2);
			if (value == null)
			{
				if (tile != null)
				{
					return value2 != null;
				}
				return true;
			}
			return false;
		}

		public bool isTilePassable(Microsoft.Xna.Framework.Rectangle nextPosition, xTile.Dimensions.Rectangle viewport)
		{
			if (isPointPassable(new Location(nextPosition.Left, nextPosition.Top), viewport) && isPointPassable(new Location(nextPosition.Left, nextPosition.Bottom), viewport) && isPointPassable(new Location(nextPosition.Right, nextPosition.Top), viewport))
			{
				return isPointPassable(new Location(nextPosition.Right, nextPosition.Bottom), viewport);
			}
			return false;
		}

		public bool isTileOnMap(Vector2 position)
		{
			if (position.X >= 0f && position.X < (float)map.Layers[0].LayerWidth && position.Y >= 0f)
			{
				return position.Y < (float)map.Layers[0].LayerHeight;
			}
			return false;
		}

		public bool isTileOnMap(int x, int y)
		{
			if (x >= 0 && x < map.Layers[0].LayerWidth && y >= 0)
			{
				return y < map.Layers[0].LayerHeight;
			}
			return false;
		}

		public void busLeave()
		{
			NPC nPC = null;
			for (int num = characters.Count - 1; num >= 0; num--)
			{
				if (characters[num].Name.Equals("Pam"))
				{
					nPC = characters[num];
					characters.RemoveAt(num);
					break;
				}
			}
			if (nPC == null)
			{
				return;
			}
			Game1.changeMusicTrack("none");
			localSound("openBox");
			if (name.Equals("BusStop"))
			{
				Game1.warpFarmer("Desert", 32, 27, flip: true);
				nPC.followSchedule = false;
				nPC.Position = new Vector2(1984f, 1752f);
				nPC.faceDirection(2);
				nPC.CurrentDialogue.Peek().temporaryDialogue = Game1.parseText(Game1.content.LoadString("Strings\\Locations:Desert_BusArrived"));
				Game1.getLocationFromName("Desert").characters.Add(nPC);
				return;
			}
			nPC.CurrentDialogue.Peek().temporaryDialogue = null;
			Game1.warpFarmer("BusStop", 9, 9, flip: true);
			if (Game1.timeOfDay >= 2300)
			{
				nPC.Position = new Vector2(1152f, 408f);
				nPC.faceDirection(2);
				Game1.getLocationFromName("Trailer").characters.Add(nPC);
			}
			else if (Game1.timeOfDay >= 1700)
			{
				nPC.Position = new Vector2(448f, 1112f);
				nPC.faceDirection(1);
				Game1.getLocationFromName("Saloon").characters.Add(nPC);
			}
			else
			{
				nPC.Position = new Vector2(512f, 600f);
				nPC.faceDirection(2);
				Game1.getLocationFromName("BusStop").characters.Add(nPC);
				nPC.Sprite.currentFrame = 0;
			}
			nPC.DirectionsToNewLocation = null;
			nPC.followSchedule = true;
		}

		public int numberOfObjectsWithName(string name)
		{
			int num = 0;
			foreach (Object value in objects.Values)
			{
				if (value.Name.Equals(name))
				{
					num++;
				}
			}
			return num;
		}

		public virtual Point getWarpPointTo(string location, Character character = null)
		{
			foreach (Warp warp in warps)
			{
				if (warp.TargetName.Equals(location))
				{
					return new Point(warp.X, warp.Y);
				}
				if (warp.TargetName.Equals("BoatTunnel") && location == "IslandSouth")
				{
					return new Point(warp.X, warp.Y);
				}
			}
			foreach (KeyValuePair<Point, string> pair in doors.Pairs)
			{
				if (pair.Value.Equals("BoatTunnel") && location == "IslandSouth")
				{
					return pair.Key;
				}
				if (pair.Value.Equals(location))
				{
					return pair.Key;
				}
			}
			return Point.Zero;
		}

		public Point getWarpPointTarget(Point warpPointLocation, Character character = null)
		{
			foreach (Warp warp in warps)
			{
				if (warp.X == warpPointLocation.X && warp.Y == warpPointLocation.Y)
				{
					return new Point(warp.TargetX, warp.TargetY);
				}
			}
			foreach (KeyValuePair<Point, string> pair in doors.Pairs)
			{
				if (!pair.Key.Equals(warpPointLocation))
				{
					continue;
				}
				string text = doesTileHaveProperty(warpPointLocation.X, warpPointLocation.Y, "Action", "Buildings");
				if (text != null && text.Contains("Warp"))
				{
					string[] array = text.Split(' ');
					if (array[0].Equals("WarpCommunityCenter"))
					{
						return new Point(32, 23);
					}
					if (array[0].Equals("Warp_Sunroom_Door"))
					{
						return new Point(5, 13);
					}
					if (character is NPC && (array[0].Equals("WarpBoatTunnel") || (array.Length > 3 && array[3].Equals("BoatTunnel"))))
					{
						return new Point(17, 43);
					}
					if (array[3].Equals("Trailer") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
					{
						return new Point(13, 24);
					}
					return new Point(Convert.ToInt32(array[1]), Convert.ToInt32(array[2]));
				}
			}
			return Point.Zero;
		}

		public virtual bool HasLocationOverrideDialogue(NPC character)
		{
			return false;
		}

		public virtual string GetLocationOverrideDialogue(NPC character)
		{
			if (!HasLocationOverrideDialogue(character))
			{
				return null;
			}
			return "";
		}

		public void boardBus(Vector2 playerTileLocation)
		{
			if (Game1.player.hasBusTicket || name.Equals("Desert"))
			{
				bool flag = false;
				for (int num = characters.Count - 1; num >= 0; num--)
				{
				}
				if (flag)
				{
					Game1.player.hasBusTicket = false;
					Game1.player.CanMove = false;
					Game1.viewportFreeze = true;
					Game1.player.position.X = -99999f;
					Game1.boardingBus = true;
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Bus_NoDriver"));
				}
			}
		}

		public NPC doesPositionCollideWithCharacter(float x, float y)
		{
			foreach (NPC character in characters)
			{
				if (character.GetBoundingBox().Contains((int)x, (int)y))
				{
					return character;
				}
			}
			return null;
		}

		public NPC doesPositionCollideWithCharacter(Microsoft.Xna.Framework.Rectangle r, bool ignoreMonsters = false)
		{
			foreach (NPC character in characters)
			{
				if (character.GetBoundingBox().Intersects(r) && (!character.IsMonster || !ignoreMonsters))
				{
					return character;
				}
			}
			return null;
		}

		public void switchOutNightTiles()
		{
			try
			{
				map.Properties.TryGetValue("NightTiles", out var value);
				if (value != null)
				{
					string[] array = value.ToString().Split(' ');
					for (int i = 0; i < array.Length; i += 4)
					{
						if ((array[i + 3].Equals("726") || array[i + 3].Equals("720")) && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
						{
							continue;
						}
						Layer layer = map.GetLayer(array[i]);
						if (layer != null)
						{
							Tile tile = layer.Tiles[Convert.ToInt32(array[i + 1]), Convert.ToInt32(array[i + 2])];
							if (tile != null)
							{
								tile.TileIndex = Convert.ToInt32(array[i + 3]);
							}
						}
					}
				}
			}
			catch (Exception)
			{
			}
			if (!(this is MineShaft) && !(this is Woods))
			{
				lightGlows.Clear();
			}
		}

		public virtual void checkForMusic(GameTime time)
		{
			if (!Game1.startedJukeboxMusic && Game1.getMusicTrackName() == "IslandMusic" && Game1.currentLocation != null && Game1.currentLocation.GetLocationContext() != LocationContext.Island)
			{
				Game1.changeMusicTrack("none", track_interruptable: true);
			}
			if (Game1.getMusicTrackName() == "rain" && !Game1.IsRainingHere())
			{
				Game1.changeMusicTrack("none", track_interruptable: true);
			}
			if (Utility.IsDesertLocation(this))
			{
				return;
			}
			if (Game1.getMusicTrackName() == "sam_acoustic1" && Game1.isMusicContextActiveButNotPlaying())
			{
				Game1.changeMusicTrack("none", track_interruptable: true);
			}
			if (!(this is MineShaft) && Game1.getMusicTrackName().Contains("Ambient") && !Game1.getMusicTrackName().Contains(Game1.currentSeason) && getMapProperty("Music") != Game1.getMusicTrackName())
			{
				Game1.changeMusicTrack("none", track_interruptable: true);
			}
			if (IsOutdoors && Game1.isMusicContextActiveButNotPlaying() && !Game1.IsRainingHere(this) && !Game1.eventUp)
			{
				if (!Game1.isDarkOut())
				{
					switch (Game1.currentSeason)
					{
					case "spring":
						Game1.changeMusicTrack("spring_day_ambient", track_interruptable: true);
						break;
					case "summer":
						Game1.changeMusicTrack("summer_day_ambient", track_interruptable: true);
						break;
					case "fall":
						Game1.changeMusicTrack("fall_day_ambient", track_interruptable: true);
						break;
					case "winter":
						Game1.changeMusicTrack("winter_day_ambient", track_interruptable: true);
						break;
					}
				}
				else if (Game1.isDarkOut() && Game1.timeOfDay < 2500)
				{
					switch (Game1.currentSeason)
					{
					case "spring":
						Game1.changeMusicTrack("spring_night_ambient", track_interruptable: true);
						break;
					case "summer":
						Game1.changeMusicTrack("spring_night_ambient", track_interruptable: true);
						break;
					case "fall":
						Game1.changeMusicTrack("spring_night_ambient", track_interruptable: true);
						break;
					case "winter":
						Game1.changeMusicTrack("none", track_interruptable: true);
						break;
					}
				}
			}
			else if (Game1.isMusicContextActiveButNotPlaying() && Game1.IsRainingHere(this) && !Game1.showingEndOfNightStuff)
			{
				Game1.changeMusicTrack("rain", track_interruptable: true);
			}
			if (!Game1.isRaining && (!Game1.currentSeason.Equals("fall") || !Game1.isDebrisWeather) && !Game1.currentSeason.Equals("winter") && !Game1.eventUp && Game1.timeOfDay < 1800 && name.Equals("Town") && (Game1.isMusicContextActiveButNotPlaying() || Game1.getMusicTrackName().Contains("ambient")))
			{
				Game1.changeMusicTrack("springtown");
			}
			else if ((name.Equals("AnimalShop") || name.Equals("ScienceHouse")) && (Game1.isMusicContextActiveButNotPlaying() || Game1.getMusicTrackName().Contains("ambient")) && currentEvent == null)
			{
				Game1.changeMusicTrack("marnieShop");
			}
		}

		public NPC isCollidingWithCharacter(Microsoft.Xna.Framework.Rectangle box)
		{
			if (Game1.isFestival() && currentEvent != null)
			{
				foreach (NPC actor in currentEvent.actors)
				{
					if (actor.GetBoundingBox().Intersects(box))
					{
						return actor;
					}
				}
			}
			foreach (NPC character in characters)
			{
				if (character.GetBoundingBox().Intersects(box))
				{
					return character;
				}
			}
			return null;
		}

		public virtual void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			if (critters != null && Game1.farmEvent == null)
			{
				for (int i = 0; i < critters.Count; i++)
				{
					critters[i].drawAboveFrontLayer(b);
				}
			}
			foreach (NPC character in characters)
			{
				character.drawAboveAlwaysFrontLayer(b);
			}
			foreach (TemporaryAnimatedSprite temporarySprite in TemporarySprites)
			{
				if (temporarySprite.drawAboveAlwaysFront)
				{
					temporarySprite.draw(b);
				}
			}
			foreach (Projectile projectile in projectiles)
			{
				projectile.draw(b);
			}
		}

		public bool moveObject(int oldX, int oldY, int newX, int newY)
		{
			Vector2 key = new Vector2(oldX, oldY);
			Vector2 vector = new Vector2(newX, newY);
			if (objects.ContainsKey(key) && !objects.ContainsKey(vector))
			{
				Object @object = objects[key];
				@object.tileLocation.Value = vector;
				objects.Remove(key);
				objects.Add(vector, @object);
				return true;
			}
			return false;
		}

		private void getGalaxySword()
		{
			Game1.flashAlpha = 1f;
			Game1.player.holdUpItemThenMessage(new MeleeWeapon(4));
			Game1.player.reduceActiveItemByOne();
			if (!Game1.player.addItemToInventoryBool(new MeleeWeapon(4)))
			{
				Game1.createItemDebris(new MeleeWeapon(4), Game1.player.getStandingPosition(), 1);
			}
			Game1.player.mailReceived.Add("galaxySword");
			Game1.player.jitterStrength = 0f;
			Game1.screenGlowHold = false;
			Game1.multiplayer.globalChatInfoMessage("GalaxySword", Game1.player.Name);
		}

		public virtual void performTouchAction(string fullActionString, Vector2 playerStandingPosition)
		{
			if (Game1.eventUp)
			{
				return;
			}
			try
			{
				string text = fullActionString.Split(' ')[0];
				if (text == null)
				{
					return;
				}
				switch (text.Length)
				{
				case 4:
					switch (text[0])
					{
					case 'W':
						if (text == "Warp")
						{
							string locationName = fullActionString.Split(' ')[1];
							int tileX = Convert.ToInt32(fullActionString.Split(' ')[2]);
							int tileY = Convert.ToInt32(fullActionString.Split(' ')[3]);
							string text4 = ((fullActionString.Split(' ').Length > 4) ? fullActionString.Split(' ')[4] : null);
							if (text4 == null || Game1.player.mailReceived.Contains(text4))
							{
								Game1.warpFarmer(locationName, tileX, tileY, flip: false);
							}
						}
						break;
					case 'D':
					{
						if (!(text == "Door"))
						{
							break;
						}
						for (int k = 1; k < fullActionString.Split(' ').Length; k++)
						{
							if (Game1.player.getFriendshipHeartLevelForNPC(fullActionString.Split(' ')[k]) < 2 && k == fullActionString.Split(' ').Length - 1)
							{
								Game1.player.Position -= Game1.player.getMostRecentMovementVector() * 2f;
								Game1.player.yVelocity = 0f;
								Game1.player.Halt();
								Game1.player.TemporaryPassableTiles.Clear();
								if (Game1.player.getTileLocation().Equals(lastTouchActionLocation))
								{
									if (Game1.player.Position.Y > lastTouchActionLocation.Y * 64f + 32f)
									{
										Game1.player.position.Y += 4f;
									}
									else
									{
										Game1.player.position.Y -= 4f;
									}
									lastTouchActionLocation = Vector2.Zero;
								}
								if ((!Game1.player.mailReceived.Contains("doorUnlock" + fullActionString.Split(' ')[1]) || (fullActionString.Split(' ').Length != 2 && !Game1.player.mailReceived.Contains("doorUnlock" + fullActionString.Split(' ')[2]))) && (fullActionString.Split(' ').Length != 3 || !Game1.player.mailReceived.Contains("doorUnlock" + fullActionString.Split(' ')[2])))
								{
									if (fullActionString.Split(' ').Length == 2)
									{
										tapToMove.Reset();
										NPC characterFromName = Game1.getCharacterFromName(fullActionString.Split(' ')[1]);
										string text3 = ((characterFromName.Gender == 0) ? "Male" : "Female");
										Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:DoorUnlock_NotFriend_" + text3, characterFromName.displayName));
									}
									else
									{
										tapToMove.Reset();
										NPC characterFromName2 = Game1.getCharacterFromName(fullActionString.Split(' ')[1]);
										NPC characterFromName3 = Game1.getCharacterFromName(fullActionString.Split(' ')[2]);
										Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:DoorUnlock_NotFriend_Couple", characterFromName2.displayName, characterFromName3.displayName));
									}
								}
								break;
							}
							if (k != fullActionString.Split(' ').Length - 1 && Game1.player.getFriendshipHeartLevelForNPC(fullActionString.Split(' ')[k]) >= 2)
							{
								if (!Game1.player.mailReceived.Contains("doorUnlock" + fullActionString.Split(' ')[k]))
								{
									Game1.player.mailReceived.Add("doorUnlock" + fullActionString.Split(' ')[k]);
								}
								break;
							}
							if (k == fullActionString.Split(' ').Length - 1 && Game1.player.getFriendshipHeartLevelForNPC(fullActionString.Split(' ')[k]) >= 2)
							{
								if (!Game1.player.mailReceived.Contains("doorUnlock" + fullActionString.Split(' ')[k]))
								{
									Game1.player.mailReceived.Add("doorUnlock" + fullActionString.Split(' ')[k]);
								}
								break;
							}
						}
						break;
					}
					}
					break;
				case 5:
					switch (text[0])
					{
					case 'S':
						if (text == "Sleep" && !Game1.newDay && Game1.shouldTimePass() && Game1.player.hasMoved && !Game1.player.passedOut && !TutorialManager.Instance.ShowingDialogueBox)
						{
							Game1.currentLocation.tapToMove.Reset();
							createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:FarmHouse_Bed_GoToSleep"), createYesNoResponses(), "Sleep", null);
						}
						break;
					case 'E':
						if (text == "Emote")
						{
							getCharacterFromName(fullActionString.Split(' ')[1]).doEmote(Convert.ToInt32(fullActionString.Split(' ')[2]));
						}
						break;
					}
					break;
				case 12:
					switch (text[0])
					{
					case 'W':
						if (text == "WomensLocker" && Game1.player.IsMale)
						{
							Game1.player.position.Y += (Game1.player.Speed + Game1.player.addedSpeed) * 2;
							Game1.player.Halt();
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WomensLocker_WrongGender"));
						}
						break;
					case 'P':
						if (text == "PoolEntrance")
						{
							if (!Game1.player.swimming)
							{
								Game1.player.swimTimer = 800;
								Game1.player.swimming.Value = true;
								Game1.player.position.Y += 16f;
								Game1.player.yVelocity = -8f;
								playSound("pullItemFromWater");
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(27, 100f, 4, 0, new Vector2(Game1.player.Position.X, Game1.player.getStandingY() - 40), flicker: false, flipped: false)
								{
									layerDepth = 1f,
									motion = new Vector2(0f, 2f)
								});
							}
							else
							{
								Game1.player.jump();
								Game1.player.swimTimer = 800;
								Game1.player.position.X = playerStandingPosition.X * 64f;
								playSound("pullItemFromWater");
								Game1.player.yVelocity = 8f;
								Game1.player.swimming.Value = false;
							}
							Game1.player.noMovementPause = 500;
						}
						break;
					}
					break;
				case 11:
				{
					if (!(text == "MagicalSeal") || Game1.player.mailReceived.Contains("krobusUnseal"))
					{
						break;
					}
					Game1.player.Position -= Game1.player.getMostRecentMovementVector() * 2f;
					Game1.player.yVelocity = 0f;
					Game1.player.Halt();
					Game1.player.TemporaryPassableTiles.Clear();
					if (Game1.player.getTileLocation().Equals(lastTouchActionLocation))
					{
						if (Game1.player.position.Y > lastTouchActionLocation.Y * 64f + 32f)
						{
							Game1.player.position.Y += 4f;
						}
						else
						{
							Game1.player.position.Y -= 4f;
						}
						lastTouchActionLocation = Vector2.Zero;
					}
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_MagicSeal"));
					for (int j = 0; j < 40; j++)
					{
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 25f, 4, 2, new Vector2(3f, 19f) * 64f + new Vector2(-8 + j % 4 * 16, -(j / 4) * 64 / 4), flicker: false, flipped: false)
						{
							layerDepth = 0.1152f + (float)j / 10000f,
							color = new Color(100 + j * 4, j * 5, 120 + j * 4),
							pingPong = true,
							delayBeforeAnimationStart = j * 10,
							scale = 4f,
							alphaFade = 0.01f
						});
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 25f, 4, 2, new Vector2(3f, 17f) * 64f + new Vector2(-8 + j % 4 * 16, j / 4 * 64 / 4), flicker: false, flipped: false)
						{
							layerDepth = 0.1152f + (float)j / 10000f,
							color = new Color(232 - j * 4, 192 - j * 6, 255 - j * 4),
							pingPong = true,
							delayBeforeAnimationStart = 320 + j * 10,
							scale = 4f,
							alphaFade = 0.01f
						});
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 25f, 4, 2, new Vector2(3f, 19f) * 64f + new Vector2(-8 + j % 4 * 16, -(j / 4) * 64 / 4), flicker: false, flipped: false)
						{
							layerDepth = 0.1152f + (float)j / 10000f,
							color = new Color(100 + j * 4, j * 6, 120 + j * 4),
							pingPong = true,
							delayBeforeAnimationStart = 640 + j * 10,
							scale = 4f,
							alphaFade = 0.01f
						});
					}
					Game1.player.jitterStrength = 2f;
					Game1.player.freezePause = 500;
					playSound("debuffHit");
					Game1.currentLocation.tapToMove.Reset();
					break;
				}
				case 9:
				{
					if (!(text == "MagicWarp"))
					{
						break;
					}
					string locationToWarp = fullActionString.Split(' ')[1];
					int locationX = Convert.ToInt32(fullActionString.Split(' ')[2]);
					int locationY = Convert.ToInt32(fullActionString.Split(' ')[3]);
					string text2 = ((fullActionString.Split(' ').Length > 4) ? fullActionString.Split(' ')[4] : null);
					if (text2 != null && !Game1.player.mailReceived.Contains(text2))
					{
						break;
					}
					if (Game1.player.mount != null)
					{
						if (fullActionString == "MagicWarp WitchSwamp 20 42")
						{
							locationY -= 2;
						}
						else if (fullActionString == "MagicWarp WitchWarpCave 4 5")
						{
							locationY += 2;
						}
					}
					for (int i = 0; i < 12; i++)
					{
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)Game1.player.position.X - 256, (int)Game1.player.position.X + 192), Game1.random.Next((int)Game1.player.position.Y - 256, (int)Game1.player.position.Y + 192)), flicker: false, Game1.random.NextDouble() < 0.5));
					}
					playSound("wand");
					Game1.freezeControls = true;
					Game1.displayFarmer = false;
					Game1.player.CanMove = false;
					Game1.flashAlpha = 1f;
					DelayedAction.fadeAfterDelay(delegate
					{
						Game1.warpFarmer(locationToWarp, locationX, locationY, flip: false);
						Game1.fadeToBlackAlpha = 0.99f;
						Game1.screenGlow = false;
						Game1.displayFarmer = true;
						Game1.player.CanMove = true;
						Game1.freezeControls = false;
					}, 1000);
					new Microsoft.Xna.Framework.Rectangle(Game1.player.GetBoundingBox().X, Game1.player.GetBoundingBox().Y, 64, 64).Inflate(192, 192);
					int num = 0;
					for (int num2 = Game1.player.getTileX() + 8; num2 >= Game1.player.getTileX() - 8; num2--)
					{
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(6, new Vector2(num2, Game1.player.getTileY()) * 64f, Color.White, 8, flipped: false, 50f)
						{
							layerDepth = 1f,
							delayBeforeAnimationStart = num * 25,
							motion = new Vector2(-0.25f, 0f)
						});
						num++;
					}
					break;
				}
				case 3:
					if (text == "Bus")
					{
						boardBus(playerStandingPosition);
					}
					break;
				case 13:
					if (text == "FaceDirection" && getCharacterFromName(fullActionString.Split(' ')[1]) != null)
					{
						getCharacterFromName(fullActionString.Split(' ')[1]).faceDirection(Convert.ToInt32(fullActionString.Split(' ')[2]));
					}
					break;
				case 14:
					if (!(text == "legendarySword"))
					{
						break;
					}
					if (Game1.player.ActiveObject != null && Utility.IsNormalObjectAtParentSheetIndex(Game1.player.ActiveObject, 74) && !Game1.player.mailReceived.Contains("galaxySword"))
					{
						Game1.player.Halt();
						Game1.player.faceDirection(2);
						Game1.player.showCarrying();
						Game1.player.jitterStrength = 1f;
						Game1.pauseThenDoFunction(7000, getGalaxySword);
						Game1.changeMusicTrack("none", track_interruptable: false, Game1.MusicContext.Event);
						playSound("crit");
						Game1.screenGlowOnce(new Color(30, 0, 150), hold: true, 0.01f, 0.999f);
						DelayedAction.playSoundAfterDelay("stardrop", 1500);
						Game1.screenOverlayTempSprites.AddRange(Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), 500, Color.White, 10, 2000));
						Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
						{
							Game1.stopMusicTrack(Game1.MusicContext.Event);
						});
					}
					else if (!Game1.player.mailReceived.Contains("galaxySword"))
					{
						localSound("SpringBirds");
					}
					break;
				case 10:
					if (text == "MensLocker" && !Game1.player.IsMale)
					{
						Game1.player.position.Y += (Game1.player.Speed + Game1.player.addedSpeed) * 2;
						Game1.player.Halt();
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:MensLocker_WrongGender"));
					}
					break;
				case 18:
					if (text == "ChangeIntoSwimsuit")
					{
						Game1.player.changeIntoSwimsuit();
					}
					break;
				case 19:
					if (text == "ChangeOutOfSwimsuit")
					{
						Game1.player.changeOutOfSwimSuit();
					}
					break;
				case 6:
				case 7:
				case 8:
				case 15:
				case 16:
				case 17:
					break;
				}
			}
			catch (Exception)
			{
			}
		}

		public virtual void updateMap()
		{
			if (!object.Equals(mapPath.Value, loadedMapPath))
			{
				reloadMap();
			}
		}

		public LargeTerrainFeature getLargeTerrainFeatureAt(int tileX, int tileY)
		{
			foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
			{
				if (largeTerrainFeature.getBoundingBox().Contains(tileX * 64 + 32, tileY * 64 + 32))
				{
					return largeTerrainFeature;
				}
			}
			return null;
		}

		public virtual void UpdateWhenCurrentLocation(GameTime time)
		{
			updateMap();
			if (wasUpdated)
			{
				return;
			}
			wasUpdated = true;
			if (_mapSeatsDirty)
			{
				UpdateMapSeats();
			}
			furnitureToRemove.Update(this);
			foreach (Furniture item in furniture)
			{
				item.updateWhenCurrentLocation(time, this);
			}
			AmbientLocationSounds.update(time);
			if (critters != null)
			{
				for (int num = critters.Count - 1; num >= 0; num--)
				{
					if (critters[num].update(time, this))
					{
						critters.RemoveAt(num);
					}
				}
			}
			if (fishSplashAnimation != null)
			{
				fishSplashAnimation.update(time);
				if (Game1.random.NextDouble() < 0.02)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite(0, fishSplashAnimation.position + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), Color.White * 0.3f));
				}
			}
			if (orePanAnimation != null)
			{
				orePanAnimation.update(time);
				if (Game1.random.NextDouble() < 0.05)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), orePanAnimation.position + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), flipped: false, 0.02f, Color.White * 0.8f)
					{
						scale = 2f,
						animationLength = 6,
						interval = 100f
					});
				}
			}
			interiorDoors.Update(time);
			updateWater(time);
			Map.Update(time.ElapsedGameTime.Milliseconds);
			int num2 = 0;
			while (num2 < this.debris.Count)
			{
				Debris debris = this.debris[num2];
				if (debris.updateChunks(time, this))
				{
					this.debris.RemoveAt(num2);
				}
				else
				{
					num2++;
				}
			}
			if (Game1.shouldTimePass() || Game1.isFestival())
			{
				int num3 = 0;
				while (num3 < projectiles.Count)
				{
					Projectile projectile = projectiles[num3];
					if (projectile.update(time, this))
					{
						projectiles.RemoveAt(num3);
					}
					else
					{
						num3++;
					}
				}
			}
			if (true)
			{
				for (int i = 0; i < _activeTerrainFeatures.Count; i++)
				{
					TerrainFeature terrainFeature = _activeTerrainFeatures[i];
					if (terrainFeature.tickUpdate(time, terrainFeature.currentTileLocation, this))
					{
						terrainFeaturesToRemoveList.Add(terrainFeature.currentTileLocation);
					}
				}
			}
			else
			{
				foreach (KeyValuePair<Vector2, TerrainFeature> pair in terrainFeatures.Pairs)
				{
					if (pair.Value.tickUpdate(time, pair.Key, this))
					{
						terrainFeaturesToRemoveList.Add(pair.Key);
					}
				}
			}
			foreach (Vector2 terrainFeaturesToRemove in terrainFeaturesToRemoveList)
			{
				terrainFeatures.Remove(terrainFeaturesToRemove);
			}
			terrainFeaturesToRemoveList.Clear();
			if (largeTerrainFeatures != null)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					largeTerrainFeature.tickUpdate(time, this);
				}
			}
			foreach (ResourceClump resourceClump in resourceClumps)
			{
				resourceClump.tickUpdate(time, resourceClump.tile, this);
			}
			if (Game1.timeOfDay >= 2000 && (float)lightLevel > 0f && name.Equals("FarmHouse"))
			{
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(64f, 448f), 2f, LightSource.LightContext.None, 0L));
			}
			if (currentEvent != null)
			{
				bool flag = false;
				do
				{
					int currentCommand = currentEvent.CurrentCommand;
					currentEvent.checkForNextCommand(this, time);
					if (currentEvent != null)
					{
						flag = currentEvent.simultaneousCommand;
						if (currentCommand == currentEvent.CurrentCommand)
						{
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
				}
				while (flag);
			}
			foreach (Object value2 in objects.Values)
			{
				tempObjects.Add(value2);
			}
			foreach (Object tempObject in tempObjects)
			{
				bool flag2 = tempObject.shakeTimer > 0;
				tempObject.updateWhenCurrentLocation(time, this);
			}
			tempObjects.Clear();
			if (Game1.gameMode != 3 || this != Game1.currentLocation)
			{
				return;
			}
			if (!Utility.IsDesertLocation(Game1.currentLocation))
			{
				if ((bool)isOutdoors && !Game1.IsRainingHere(this) && Game1.random.NextDouble() < 0.002 && Game1.isMusicContextActiveButNotPlaying() && Game1.timeOfDay < 2000 && !Game1.currentSeason.Equals("winter") && !name.Equals("Desert"))
				{
					localSound("SpringBirds");
				}
				else if (!Game1.IsRainingHere(this) && (bool)isOutdoors && Game1.timeOfDay > 2100 && Game1.currentSeason.Equals("summer") && Game1.random.NextDouble() < 0.0005 && !(this is Beach) && !name.Equals("temp"))
				{
					localSound("crickets");
				}
				else if (Game1.IsRainingHere(this) && (bool)isOutdoors && !name.Equals("Town") && !Game1.eventUp && Game1.options.musicVolumeLevel > 0f && Game1.random.NextDouble() < 0.00015)
				{
					localSound("rainsound");
				}
			}
			Vector2 vector = new Vector2(Game1.player.getStandingX() / 64, Game1.player.getStandingY() / 64);
			if (lastTouchActionLocation.Equals(Vector2.Zero))
			{
				string text = doesTileHaveProperty((int)vector.X, (int)vector.Y, "TouchAction", "Back");
				lastTouchActionLocation = new Vector2(Game1.player.getStandingX() / 64, Game1.player.getStandingY() / 64);
				if (text != null)
				{
					performTouchAction(text, vector);
				}
			}
			else if (!lastTouchActionLocation.Equals(vector))
			{
				lastTouchActionLocation = Vector2.Zero;
			}
			foreach (Farmer farmer in farmers)
			{
				Vector2 tileLocation = farmer.getTileLocation();
				Vector2[] adjacentTilesOffsets = Character.AdjacentTilesOffsets;
				foreach (Vector2 vector2 in adjacentTilesOffsets)
				{
					Vector2 key = tileLocation + vector2;
					if (objects.TryGetValue(key, out var value))
					{
						value.farmerAdjacentAction(this);
					}
				}
			}
			if (Game1.boardingBus)
			{
				NPC characterFromName = getCharacterFromName("Pam");
				if (characterFromName != null && doesTileHaveProperty(characterFromName.getStandingX() / 64, characterFromName.getStandingY() / 64, "TouchAction", "Back") != null)
				{
					busLeave();
				}
			}
		}

		public void updateWater(GameTime time)
		{
			waterAnimationTimer -= time.ElapsedGameTime.Milliseconds;
			if (waterAnimationTimer <= 0)
			{
				waterAnimationIndex = (waterAnimationIndex + 1) % 10;
				waterAnimationTimer = 200;
			}
			if (!isFarm)
			{
				waterPosition += (float)((Math.Sin((float)time.TotalGameTime.Milliseconds / 1000f) + 1.0) * 0.15000000596046448);
			}
			else
			{
				waterPosition += 0.1f;
			}
			if (waterPosition >= 64f)
			{
				waterPosition -= 64f;
				waterTileFlip = !waterTileFlip;
			}
		}

		public NPC getCharacterFromName(string name)
		{
			NPC result = null;
			foreach (NPC character in characters)
			{
				if (character.Name.Equals(name))
				{
					return character;
				}
			}
			return result;
		}

		protected virtual void updateCharacters(GameTime time)
		{
			bool flag = Game1.shouldTimePass();
			for (int num = characters.Count - 1; num >= 0; num--)
			{
				NPC nPC = characters[num];
				if (nPC != null && (flag || nPC is Horse || nPC.forceUpdateTimer > 0))
				{
					nPC.currentLocation = this;
					nPC.update(time, this);
					if (num < characters.Count && nPC is Monster && ((Monster)nPC).Health <= 0)
					{
						characters.RemoveAt(num);
					}
				}
				else if (nPC != null)
				{
					if (nPC.hasJustStartedFacingPlayer)
					{
						nPC.updateFaceTowardsFarmer(time, this);
					}
					nPC.updateEmote(time);
				}
			}
		}

		public virtual void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			netAudio.Update();
			removeTemporarySpritesWithIDEvent.Poll();
			rumbleAndFadeEvent.Poll();
			damagePlayersEvent.Poll();
			if (!ignoreWasUpdatedFlush)
			{
				wasUpdated = false;
			}
			updateCharacters(time);
			for (int num = temporarySprites.Count - 1; num >= 0; num--)
			{
				TemporaryAnimatedSprite temporaryAnimatedSprite = ((num < temporarySprites.Count) ? temporarySprites[num] : null);
				if (num < temporarySprites.Count && temporaryAnimatedSprite != null && temporaryAnimatedSprite.update(time) && num < temporarySprites.Count)
				{
					temporarySprites.RemoveAt(num);
				}
			}
		}

		public Response[] createYesNoResponses()
		{
			return new Response[2]
			{
				new Response("Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")).SetHotKey(Keys.Y),
				new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")).SetHotKey(Keys.Escape)
			};
		}

		public void createQuestionDialogue(string question, Response[] answerChoices, string dialogKey)
		{
			lastQuestionKey = dialogKey;
			Game1.drawObjectQuestionDialogue(question, answerChoices.ToList());
		}

		public void createQuestionDialogueWithCustomWidth(string question, Response[] answerChoices, string dialogKey)
		{
			int width = SpriteText.getWidthOfString(question) + 64;
			lastQuestionKey = dialogKey;
			Game1.drawObjectQuestionDialogue(question, answerChoices.ToList(), width);
		}

		public void createQuestionDialogue(string question, Response[] answerChoices, afterQuestionBehavior afterDialogueBehavior, NPC speaker = null)
		{
			afterQuestion = afterDialogueBehavior;
			Game1.drawObjectQuestionDialogue(question, answerChoices.ToList());
			if (speaker != null)
			{
				Game1.objectDialoguePortraitPerson = speaker;
			}
		}

		public void createQuestionDialogue(string question, Response[] answerChoices, string dialogKey, Object actionObject)
		{
			lastQuestionKey = dialogKey;
			Game1.drawObjectQuestionDialogue(question, answerChoices.ToList());
			actionObjectForQuestionDialogue = actionObject;
		}

		public virtual Point GetMapPropertyPosition(string key, int default_x, int default_y)
		{
			string text = "";
			if (Map.Properties.ContainsKey(key))
			{
				text = map.Properties[key].ToString();
			}
			if (text != "")
			{
				int result = -1;
				int result2 = -1;
				string[] array = text.Split(' ');
				if (array.Length >= 2 && int.TryParse(array[0], out result) && int.TryParse(array[1], out result2))
				{
					return new Point(result, result2);
				}
			}
			return new Point(default_x, default_y);
		}

		public virtual void monsterDrop(Monster monster, int x, int y, Farmer who)
		{
			int num = monster.coinsToDrop;
			IList<int> objectsToDrop = monster.objectsToDrop;
			Vector2 vector = new Vector2(Game1.player.GetBoundingBox().Center.X, Game1.player.GetBoundingBox().Center.Y);
			List<Item> extraDropItems = monster.getExtraDropItems();
			if (Game1.player.isWearingRing(526))
			{
				string value = "";
				Game1.content.Load<Dictionary<string, string>>("Data\\Monsters").TryGetValue(monster.Name, out value);
				if (value != null && value.Length > 0)
				{
					string[] array = value.Split('/');
					string[] array2 = array[6].Split(' ');
					for (int i = 0; i < array2.Length; i += 2)
					{
						if (Game1.random.NextDouble() < Convert.ToDouble(array2[i + 1]))
						{
							objectsToDrop.Add(Convert.ToInt32(array2[i]));
						}
					}
				}
			}
			for (int j = 0; j < objectsToDrop.Count; j++)
			{
				int num2 = objectsToDrop[j];
				if (num2 < 0)
				{
					debris.Add(monster.ModifyMonsterLoot(new Debris(Math.Abs(num2), Game1.random.Next(1, 4), new Vector2(x, y), vector)));
				}
				else
				{
					debris.Add(monster.ModifyMonsterLoot(new Debris(num2, new Vector2(x, y), vector)));
				}
			}
			for (int k = 0; k < extraDropItems.Count; k++)
			{
				debris.Add(monster.ModifyMonsterLoot(new Debris(extraDropItems[k], new Vector2(x, y), vector)));
			}
			if (Game1.player.isWearingRing(526))
			{
				extraDropItems = monster.getExtraDropItems();
				for (int l = 0; l < extraDropItems.Count; l++)
				{
					Item one = extraDropItems[l].getOne();
					one.Stack = extraDropItems[l].Stack;
					one.HasBeenInInventory = false;
					debris.Add(monster.ModifyMonsterLoot(new Debris(one, new Vector2(x, y), vector)));
				}
			}
			if (HasUnlockedAreaSecretNotes(Game1.player) && Game1.random.NextDouble() < 0.033)
			{
				Object @object = tryToCreateUnseenSecretNote(Game1.player);
				if (@object != null)
				{
					monster.ModifyMonsterLoot(Game1.createItemDebris(@object, new Vector2(x, y), -1, this));
				}
			}
			if (this is Woods && Game1.random.NextDouble() < 0.1)
			{
				monster.ModifyMonsterLoot(Game1.createItemDebris(new Object(292, 1), new Vector2(x, y), -1, this));
			}
			if ((bool)monster.isHardModeMonster && Game1.stats.getStat("hardModeMonstersKilled") > 50 && Game1.random.NextDouble() < 0.001 + (double)((float)who.LuckLevel * 0.0002f))
			{
				monster.ModifyMonsterLoot(Game1.createItemDebris(new Object(896, 1), new Vector2(x, y), -1, this));
			}
			else if ((bool)monster.isHardModeMonster && Game1.random.NextDouble() < 0.008 + (double)((float)who.LuckLevel * 0.002f))
			{
				monster.ModifyMonsterLoot(Game1.createItemDebris(new Object(858, 1), new Vector2(x, y), -1, this));
			}
		}

		public virtual bool HasUnlockedAreaSecretNotes(Farmer who)
		{
			if (GetLocationContext() == LocationContext.Island)
			{
				return true;
			}
			return who.hasMagnifyingGlass;
		}

		public bool damageMonster(Microsoft.Xna.Framework.Rectangle areaOfEffect, int minDamage, int maxDamage, bool isBomb, Farmer who)
		{
			return damageMonster(areaOfEffect, minDamage, maxDamage, isBomb, 1f, 0, 0f, 1f, triggerMonsterInvincibleTimer: false, who);
		}

		private bool isMonsterDamageApplicable(Farmer who, Monster monster, bool horizontalBias = true)
		{
			if (!monster.isGlider && !(who.CurrentTool is Slingshot) && !monster.ignoreDamageLOS.Value)
			{
				Point tileLocationPoint = who.getTileLocationPoint();
				Point tileLocationPoint2 = monster.getTileLocationPoint();
				if (Math.Abs(tileLocationPoint.X - tileLocationPoint2.X) + Math.Abs(tileLocationPoint.Y - tileLocationPoint2.Y) > 1)
				{
					int num = tileLocationPoint2.X - tileLocationPoint.X;
					int num2 = tileLocationPoint2.Y - tileLocationPoint.Y;
					Vector2 key = new Vector2(tileLocationPoint.X, tileLocationPoint.Y);
					while (num != 0 || num2 != 0)
					{
						if (horizontalBias)
						{
							if (Math.Abs(num) >= Math.Abs(num2))
							{
								key.X += Math.Sign(num);
								num -= Math.Sign(num);
							}
							else
							{
								key.Y += Math.Sign(num2);
								num2 -= Math.Sign(num2);
							}
						}
						else if (Math.Abs(num2) >= Math.Abs(num))
						{
							key.Y += Math.Sign(num2);
							num2 -= Math.Sign(num2);
						}
						else
						{
							key.X += Math.Sign(num);
							num -= Math.Sign(num);
						}
						if ((objects.ContainsKey(key) && !objects[key].isPassable()) || BlocksDamageLOS((int)key.X, (int)key.Y))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		public virtual bool BlocksDamageLOS(int x, int y)
		{
			if (getTileIndexAt(x, y, "Buildings") != -1 && doesTileHaveProperty(x, y, "Passable", "Buildings") == null)
			{
				return true;
			}
			return false;
		}

		public bool damageMonster(Microsoft.Xna.Framework.Rectangle areaOfEffect, int minDamage, int maxDamage, bool isBomb, float knockBackModifier, int addedPrecision, float critChance, float critMultiplier, bool triggerMonsterInvincibleTimer, Farmer who)
		{
			bool result = false;
			for (int num = characters.Count - 1; num >= 0; num--)
			{
				if (num < characters.Count && characters[num] is Monster monster && monster.IsMonster && monster.Health > 0 && monster.TakesDamageFromHitbox(areaOfEffect))
				{
					if (monster.currentLocation == null)
					{
						monster.currentLocation = this;
					}
					if (!monster.IsInvisible && !monster.isInvincible() && (isBomb || isMonsterDamageApplicable(who, monster) || isMonsterDamageApplicable(who, monster, horizontalBias: false)))
					{
						bool flag = !isBomb && who != null && who.CurrentTool != null && who.CurrentTool is MeleeWeapon && (int)(who.CurrentTool as MeleeWeapon).type == 1;
						bool flag2 = false;
						if (flag && MeleeWeapon.daggerHitsLeft > 1)
						{
							flag2 = true;
						}
						if (flag2)
						{
							triggerMonsterInvincibleTimer = false;
						}
						result = true;
						if (Game1.currentLocation == this)
						{
							Rumble.rumble(0.1f + (float)(Game1.random.NextDouble() / 8.0), 200 + Game1.random.Next(-50, 50));
						}
						Microsoft.Xna.Framework.Rectangle boundingBox = monster.GetBoundingBox();
						Vector2 trajectory = Utility.getAwayFromPlayerTrajectory(boundingBox, who);
						if (knockBackModifier > 0f)
						{
							trajectory *= knockBackModifier;
						}
						else
						{
							trajectory = new Vector2(monster.xVelocity, monster.yVelocity);
						}
						if (monster.Slipperiness == -1)
						{
							trajectory = Vector2.Zero;
						}
						bool flag3 = false;
						int num2 = 0;
						if (who != null && who.CurrentTool != null && monster.hitWithTool(who.CurrentTool))
						{
							return false;
						}
						if (who.professions.Contains(25))
						{
							critChance += critChance * 0.5f;
						}
						if (maxDamage >= 0)
						{
							num2 = Game1.random.Next(minDamage, maxDamage + 1);
							if (who != null && Game1.random.NextDouble() < (double)(critChance + (float)who.LuckLevel * (critChance / 40f)))
							{
								flag3 = true;
								playSound("crit");
							}
							num2 = (flag3 ? ((int)((float)num2 * critMultiplier)) : num2);
							num2 = Math.Max(1, num2 + ((who != null) ? (who.attack * 3) : 0));
							if (who != null && who.professions.Contains(24))
							{
								num2 = (int)Math.Ceiling((float)num2 * 1.1f);
							}
							if (who != null && who.professions.Contains(26))
							{
								num2 = (int)Math.Ceiling((float)num2 * 1.15f);
							}
							if (who != null && flag3 && who.professions.Contains(29))
							{
								num2 = (int)((float)num2 * 2f);
							}
							if (who != null)
							{
								foreach (BaseEnchantment enchantment in who.enchantments)
								{
									enchantment.OnCalculateDamage(monster, this, who, ref num2);
								}
							}
							num2 = monster.takeDamage(num2, (int)trajectory.X, (int)trajectory.Y, isBomb, (double)addedPrecision / 10.0, who);
							if (flag2)
							{
								monster.stunTime = 50;
							}
							else
							{
								monster.stunTime = 0;
							}
							if (num2 == -1)
							{
								debris.Add(new Debris("Miss", 1, new Vector2(boundingBox.Center.X, boundingBox.Center.Y), Color.LightGray, 1f, 0f));
							}
							else
							{
								removeDamageDebris(monster);
								debris.Add(new Debris(num2, new Vector2(boundingBox.Center.X + 16, boundingBox.Center.Y), flag3 ? Color.Yellow : new Color(255, 130, 0), flag3 ? (1f + (float)num2 / 300f) : 1f, monster));
								if (who != null)
								{
									foreach (BaseEnchantment enchantment2 in who.enchantments)
									{
										enchantment2.OnDealDamage(monster, this, who, ref num2);
									}
								}
							}
							if (triggerMonsterInvincibleTimer)
							{
								monster.setInvincibleCountdown(450 / (flag ? 3 : 2));
							}
						}
						else
						{
							num2 = -2;
							monster.setTrajectory(trajectory);
							if (monster.Slipperiness > 10)
							{
								monster.xVelocity /= 2f;
								monster.yVelocity /= 2f;
							}
						}
						if (who != null && who.CurrentTool != null && who.CurrentTool.Name.Equals("Galaxy Sword"))
						{
							Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(362, Game1.random.Next(50, 120), 6, 1, new Vector2(boundingBox.Center.X - 32, boundingBox.Center.Y - 32), flicker: false, flipped: false));
						}
						if (monster.Health <= 0)
						{
							if (!isFarm)
							{
								who.checkForQuestComplete(null, 1, 1, null, monster.Name, 4);
							}
							if (!isFarm && Game1.player.team.specialOrders != null)
							{
								foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
								{
									if (specialOrder.onMonsterSlain != null)
									{
										specialOrder.onMonsterSlain(Game1.player, monster);
									}
								}
							}
							if (who != null)
							{
								foreach (BaseEnchantment enchantment3 in who.enchantments)
								{
									enchantment3.OnMonsterSlay(monster, this, who);
								}
							}
							if (who != null && who.leftRing.Value != null)
							{
								who.leftRing.Value.onMonsterSlay(monster, this, who);
							}
							if (who != null && who.rightRing.Value != null)
							{
								who.rightRing.Value.onMonsterSlay(monster, this, who);
							}
							if (who != null && !isFarm && (!(monster is GreenSlime) || (bool)(monster as GreenSlime).firstGeneration))
							{
								if (who.IsLocalPlayer)
								{
									Game1.stats.monsterKilled(monster.Name);
								}
								else if (Game1.IsMasterGame)
								{
									who.queueMessage(25, Game1.player, monster.Name);
								}
							}
							monsterDrop(monster, boundingBox.Center.X, boundingBox.Center.Y, who);
							if (who != null && !isFarm)
							{
								who.gainExperience(4, monster.ExperienceGained);
							}
							if ((bool)monster.isHardModeMonster)
							{
								Game1.stats.incrementStat("hardModeMonstersKilled", 1);
							}
							characters.Remove(monster);
							Game1.stats.MonstersKilled++;
						}
						else if (num2 > 0)
						{
							monster.shedChunks(Game1.random.Next(1, 3));
							if (flag3)
							{
								Vector2 standingPosition = monster.getStandingPosition();
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standingPosition - new Vector2(32f, 32f), flicker: false, Game1.random.NextDouble() < 0.5)
								{
									scale = 0.75f,
									alpha = (flag3 ? 0.75f : 0.5f)
								});
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standingPosition - new Vector2(32 + Game1.random.Next(-21, 21) + 32, 32 + Game1.random.Next(-21, 21)), flicker: false, Game1.random.NextDouble() < 0.5)
								{
									scale = 0.5f,
									delayBeforeAnimationStart = 50,
									alpha = (flag3 ? 0.75f : 0.5f)
								});
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standingPosition - new Vector2(32 + Game1.random.Next(-21, 21) - 32, 32 + Game1.random.Next(-21, 21)), flicker: false, Game1.random.NextDouble() < 0.5)
								{
									scale = 0.5f,
									delayBeforeAnimationStart = 100,
									alpha = (flag3 ? 0.75f : 0.5f)
								});
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standingPosition - new Vector2(32 + Game1.random.Next(-21, 21) + 32, 32 + Game1.random.Next(-21, 21)), flicker: false, Game1.random.NextDouble() < 0.5)
								{
									scale = 0.5f,
									delayBeforeAnimationStart = 150,
									alpha = (flag3 ? 0.75f : 0.5f)
								});
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standingPosition - new Vector2(32 + Game1.random.Next(-21, 21) - 32, 32 + Game1.random.Next(-21, 21)), flicker: false, Game1.random.NextDouble() < 0.5)
								{
									scale = 0.5f,
									delayBeforeAnimationStart = 200,
									alpha = (flag3 ? 0.75f : 0.5f)
								});
							}
						}
					}
				}
			}
			return result;
		}

		public void moveCharacters(GameTime time)
		{
			for (int num = characters.Count - 1; num >= 0; num--)
			{
				NPC nPC = characters[num];
				if (!nPC.IsInvisible)
				{
					nPC.updateMovement(this, time);
				}
			}
		}

		public void growWeedGrass(int iterations)
		{
			for (int i = 0; i < iterations; i++)
			{
				for (int num = terrainFeatures.Count() - 1; num >= 0; num--)
				{
					KeyValuePair<Vector2, TerrainFeature> keyValuePair = terrainFeatures.Pairs.ElementAt(num);
					if (keyValuePair.Value is Grass && Game1.random.NextDouble() < 0.65)
					{
						if ((int)((Grass)keyValuePair.Value).numberOfWeeds < 4)
						{
							((Grass)keyValuePair.Value).numberOfWeeds.Value = Math.Max(0, Math.Min(4, (int)((Grass)keyValuePair.Value).numberOfWeeds + Game1.random.Next(3)));
						}
						else if ((int)((Grass)keyValuePair.Value).numberOfWeeds >= 4)
						{
							int num2 = (int)keyValuePair.Key.X;
							int num3 = (int)keyValuePair.Key.Y;
							if (isTileOnMap(num2, num3) && !isTileOccupied(keyValuePair.Key + new Vector2(-1f, 0f)) && isTileLocationOpenIgnoreFrontLayers(new Location(num2 - 1, num3)) && doesTileHaveProperty(num2 - 1, num3, "Diggable", "Back") != null && doesTileHaveProperty(num2 - 1, num3, "NoSpawn", "Back") == null && Game1.random.NextDouble() < 0.25)
							{
								terrainFeatures.Add(keyValuePair.Key + new Vector2(-1f, 0f), new Grass((byte)((Grass)keyValuePair.Value).grassType, Game1.random.Next(1, 3)));
							}
							if (isTileOnMap(num2, num3) && !isTileOccupied(keyValuePair.Key + new Vector2(1f, 0f)) && isTileLocationOpenIgnoreFrontLayers(new Location(num2 + 1, num3)) && doesTileHaveProperty(num2 + 1, num3, "Diggable", "Back") != null && doesTileHaveProperty(num2 + 1, num3, "NoSpawn", "Back") == null && Game1.random.NextDouble() < 0.25)
							{
								terrainFeatures.Add(keyValuePair.Key + new Vector2(1f, 0f), new Grass((byte)((Grass)keyValuePair.Value).grassType, Game1.random.Next(1, 3)));
							}
							if (isTileOnMap(num2, num3) && !isTileOccupied(keyValuePair.Key + new Vector2(0f, 1f)) && isTileLocationOpenIgnoreFrontLayers(new Location(num2, num3 + 1)) && doesTileHaveProperty(num2, num3 + 1, "Diggable", "Back") != null && doesTileHaveProperty(num2, num3 + 1, "NoSpawn", "Back") == null && Game1.random.NextDouble() < 0.25)
							{
								terrainFeatures.Add(keyValuePair.Key + new Vector2(0f, 1f), new Grass((byte)((Grass)keyValuePair.Value).grassType, Game1.random.Next(1, 3)));
							}
							if (isTileOnMap(num2, num3) && !isTileOccupied(keyValuePair.Key + new Vector2(0f, -1f)) && isTileLocationOpenIgnoreFrontLayers(new Location(num2, num3 - 1)) && doesTileHaveProperty(num2, num3 - 1, "Diggable", "Back") != null && doesTileHaveProperty(num2, num3 - 1, "NoSpawn", "Back") == null && Game1.random.NextDouble() < 0.25)
							{
								terrainFeatures.Add(keyValuePair.Key + new Vector2(0f, -1f), new Grass((byte)((Grass)keyValuePair.Value).grassType, Game1.random.Next(1, 3)));
							}
						}
					}
				}
			}
		}

		public void removeDamageDebris(Monster monster)
		{
			debris.Filter((Debris d) => d.toHover == null || !d.toHover.Equals(monster) || d.nonSpriteChunkColor.Equals(Color.Yellow) || !(d.timeSinceDoneBouncing > 900f));
		}

		public void spawnWeeds(bool weedsOnly)
		{
			int num = Game1.random.Next(isFarm ? 5 : 2, isFarm ? 12 : 6);
			if (Game1.dayOfMonth == 1 && Game1.currentSeason.Equals("spring"))
			{
				num *= 15;
			}
			if (name.Equals("Desert"))
			{
				num = ((Game1.random.NextDouble() < 0.1) ? 1 : 0);
			}
			for (int i = 0; i < num; i++)
			{
				int num2 = 0;
				while (num2 < 3)
				{
					int num3 = Game1.random.Next(map.DisplayWidth / 64);
					int num4 = Game1.random.Next(map.DisplayHeight / 64);
					Vector2 vector = new Vector2(num3, num4);
					objects.TryGetValue(vector, out var value);
					int num5 = 0;
					bool flag = false;
					int num6 = -1;
					int num7 = -1;
					if (name.Equals("Desert"))
					{
						if (Game1.random.NextDouble() < 0.5)
						{
							num5 = 750;
						}
						else
						{
							num5 = 750;
						}
					}
					else if (Game1.random.NextDouble() < 0.15 + (weedsOnly ? 0.05 : 0.0))
					{
						num6 = 1;
					}
					else if (!weedsOnly && Game1.random.NextDouble() < 0.35)
					{
						num7 = 1;
					}
					else if (!weedsOnly && !isFarm && Game1.random.NextDouble() < 0.35)
					{
						num7 = 2;
					}
					if (num7 != -1)
					{
						if (this is Farm && Game1.random.NextDouble() < 0.25)
						{
							return;
						}
					}
					else if (value == null && doesTileHaveProperty(num3, num4, "Diggable", "Back") != null && isTileLocationOpen(new Location(num3, num4)) && !isTileOccupied(vector) && doesTileHaveProperty(num3, num4, "Water", "Back") == null)
					{
						string text = doesTileHaveProperty(num3, num4, "NoSpawn", "Back");
						if (text != null && (text.Equals("Grass") || text.Equals("All") || text.Equals("True")))
						{
							continue;
						}
						if (num6 != -1 && !GetSeasonForLocation().Equals("winter") && name.Equals("Farm"))
						{
							int numberOfWeeds = Game1.random.Next(1, 3);
							terrainFeatures.Add(vector, new Grass(num6, numberOfWeeds));
						}
					}
					num2++;
				}
			}
		}

		public bool addCharacterAtRandomLocation(NPC n)
		{
			Vector2 vector = new Vector2(Game1.random.Next(0, map.GetLayer("Back").LayerWidth), Game1.random.Next(0, map.GetLayer("Back").LayerHeight));
			int i;
			for (i = 0; i < 6; i++)
			{
				if (!isTileOccupied(vector) && isTilePassable(new Location((int)vector.X, (int)vector.Y), Game1.viewport) && map.GetLayer("Back").Tiles[(int)vector.X, (int)vector.Y] != null && !map.GetLayer("Back").Tiles[(int)vector.X, (int)vector.Y].Properties.ContainsKey("NPCBarrier"))
				{
					break;
				}
				vector = new Vector2(Game1.random.Next(0, map.GetLayer("Back").LayerWidth), Game1.random.Next(0, map.GetLayer("Back").LayerHeight));
			}
			if (i < 6)
			{
				n.Position = vector * new Vector2(64f, 64f) - new Vector2(0f, n.Sprite.SpriteHeight - 64);
				addCharacter(n);
				return true;
			}
			return false;
		}

		public virtual void OnMiniJukeboxAdded()
		{
			miniJukeboxCount.Value += 1;
			UpdateMiniJukebox();
		}

		public virtual void OnMiniJukeboxRemoved()
		{
			miniJukeboxCount.Value -= 1;
			UpdateMiniJukebox();
		}

		public virtual void UpdateMiniJukebox()
		{
			if (miniJukeboxCount.Value <= 0)
			{
				miniJukeboxCount.Set(0);
				miniJukeboxTrack.Set("");
			}
		}

		public virtual bool IsMiniJukeboxPlaying()
		{
			if (miniJukeboxCount.Value > 0 && miniJukeboxTrack.Value != "")
			{
				if (IsOutdoors)
				{
					return !Game1.IsRainingHere(this);
				}
				return true;
			}
			return false;
		}

		public virtual void DayUpdate(int dayOfMonth)
		{
			netAudio.StopPlaying("fuse");
			SelectRandomMiniJukeboxTrack();
			if (critters != null)
			{
				critters.Clear();
			}
			int num = 0;
			while (num < this.debris.Count)
			{
				Debris debris = this.debris[num];
				if (debris.isEssentialItem() && Game1.IsMasterGame)
				{
					if (Utility.IsNormalObjectAtParentSheetIndex(debris.item, 73))
					{
						debris.collect(Game1.player);
					}
					else
					{
						Item item = debris.item;
						debris.item = null;
						Game1.player.team.returnedDonations.Add(item);
						Game1.player.team.newLostAndFoundItems.Value = true;
					}
					this.debris.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
			updateMap();
			Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
			temporarySprites.Clear();
			KeyValuePair<Vector2, TerrainFeature>[] array = terrainFeatures.Pairs.ToArray();
			KeyValuePair<Vector2, TerrainFeature>[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				KeyValuePair<Vector2, TerrainFeature> keyValuePair = array2[i];
				if (!isTileOnMap(keyValuePair.Key))
				{
					terrainFeatures.Remove(keyValuePair.Key);
				}
				else
				{
					keyValuePair.Value.dayUpdate(this, keyValuePair.Key);
				}
			}
			KeyValuePair<Vector2, TerrainFeature>[] array3 = array;
			for (int j = 0; j < array3.Length; j++)
			{
				KeyValuePair<Vector2, TerrainFeature> keyValuePair2 = array3[j];
				if (keyValuePair2.Value is HoeDirt hoeDirt)
				{
					hoeDirt.updateNeighbors(this, keyValuePair2.Key);
				}
			}
			if (largeTerrainFeatures != null)
			{
				LargeTerrainFeature[] array4 = largeTerrainFeatures.ToArray();
				LargeTerrainFeature[] array5 = array4;
				foreach (LargeTerrainFeature largeTerrainFeature in array5)
				{
					largeTerrainFeature.dayUpdate(this);
				}
			}
			List<KeyValuePair<Vector2, Object>> list = new List<KeyValuePair<Vector2, Object>>();
			for (int num2 = objects.Count() - 1; num2 >= 0; num2--)
			{
				objects.Pairs.ElementAt(num2).Value.DayUpdate(this);
				if (objects.Pairs.ElementAt(num2).Value.destroyOvernight)
				{
					list.Add(objects.Pairs.ElementAt(num2));
				}
			}
			foreach (KeyValuePair<Vector2, Object> item2 in list)
			{
				if (objects.ContainsKey(item2.Key) && objects[item2.Key] == item2.Value)
				{
					if (item2.Value != null)
					{
						item2.Value.performRemoveAction(item2.Key, this);
					}
					objects.Remove(item2.Key);
				}
			}
			if (!(this is FarmHouse))
			{
				this.debris.Filter((Debris d) => d.item != null);
			}
			if (((bool)isOutdoors || map.Properties.ContainsKey("ForceSpawnForageables")) && !map.Properties.ContainsKey("skipWeedGrowth"))
			{
				if (Game1.dayOfMonth % 7 == 0 && !(this is Farm))
				{
					Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0);
					if (this is IslandWest)
					{
						rectangle = new Microsoft.Xna.Framework.Rectangle(31, 3, 77, 66);
					}
					for (int num3 = objects.Count() - 1; num3 >= 0; num3--)
					{
						if ((bool)objects.Pairs.ElementAt(num3).Value.isSpawnedObject && !rectangle.Contains(Utility.Vector2ToPoint(objects.Pairs.ElementAt(num3).Key)))
						{
							objects.Remove(objects.Pairs.ElementAt(num3).Key);
						}
					}
					numberOfSpawnedObjectsOnMap = 0;
					spawnObjects();
					spawnObjects();
				}
				spawnObjects();
				if (Game1.dayOfMonth == 1)
				{
					spawnObjects();
				}
				if (Game1.stats.DaysPlayed < 4)
				{
					spawnObjects();
				}
				bool flag = false;
				foreach (Layer layer in map.Layers)
				{
					if (layer.Id.Equals("Paths"))
					{
						flag = true;
						break;
					}
				}
				if (flag && !(this is Farm))
				{
					for (int l = 0; l < map.Layers[0].LayerWidth; l++)
					{
						for (int m = 0; m < map.Layers[0].LayerHeight; m++)
						{
							if (map.GetLayer("Paths").Tiles[l, m] == null || !(Game1.random.NextDouble() < 0.5))
							{
								continue;
							}
							Vector2 vector = new Vector2(l, m);
							int num4 = -1;
							switch (map.GetLayer("Paths").Tiles[l, m].TileIndex)
							{
							case 9:
								num4 = 1;
								if (Game1.currentSeason.Equals("winter"))
								{
									num4 += 3;
								}
								break;
							case 10:
								num4 = 2;
								if (Game1.currentSeason.Equals("winter"))
								{
									num4 += 3;
								}
								break;
							case 11:
								num4 = 3;
								break;
							case 12:
								num4 = 6;
								break;
							case 31:
								num4 = 9;
								break;
							case 32:
								num4 = 8;
								break;
							}
							if (num4 != -1 && GetFurnitureAt(vector) == null && !terrainFeatures.ContainsKey(vector) && !objects.ContainsKey(vector))
							{
								terrainFeatures.Add(vector, new Tree(num4, 2));
							}
						}
					}
				}
			}
			if (!isFarm && !SeedsIgnoreSeasonsHere())
			{
				ICollection<Vector2> collection = new List<Vector2>(terrainFeatures.Keys);
				for (int num5 = collection.Count - 1; num5 >= 0; num5--)
				{
					if (terrainFeatures[collection.ElementAt(num5)] is HoeDirt && ((terrainFeatures[collection.ElementAt(num5)] as HoeDirt).crop == null || (bool)(terrainFeatures[collection.ElementAt(num5)] as HoeDirt).crop.forageCrop))
					{
						terrainFeatures.Remove(collection.ElementAt(num5));
					}
				}
			}
			for (int num6 = characters.Count - 1; num6 >= 0; num6--)
			{
			}
			lightLevel.Value = 0f;
			name.Equals("BugLand");
			foreach (Furniture item3 in furniture)
			{
				item3.minutesElapsed(Utility.CalculateMinutesUntilMorning(Game1.timeOfDay), this);
				item3.DayUpdate(this);
			}
			addLightGlows();
			if (!(this is Farm))
			{
				HandleGrassGrowth(dayOfMonth);
			}
		}

		public void addLightGlows()
		{
			int trulyDarkTime = Game1.getTrulyDarkTime();
			if ((bool)isOutdoors || (Game1.timeOfDay >= trulyDarkTime && !Game1.newDay))
			{
				return;
			}
			lightGlows.Clear();
			map.Properties.TryGetValue("DayTiles", out var value);
			if (value == null)
			{
				return;
			}
			string[] array = value.ToString().Trim().Split(' ');
			for (int i = 0; i < array.Length; i += 4)
			{
				if (map.GetLayer(array[i]).PickTile(new Location(Convert.ToInt32(array[i + 1]) * 64, Convert.ToInt32(array[i + 2]) * 64), new Size(Game1.viewport.Width, Game1.viewport.Height)) != null)
				{
					map.GetLayer(array[i]).Tiles[Convert.ToInt32(array[i + 1]), Convert.ToInt32(array[i + 2])].TileIndex = Convert.ToInt32(array[i + 3]);
					switch (Convert.ToInt32(array[i + 3]))
					{
					case 257:
						lightGlows.Add(new Vector2(Convert.ToInt32(array[i + 1]), Convert.ToInt32(array[i + 2])) * 64f + new Vector2(32f, -4f));
						break;
					case 256:
						lightGlows.Add(new Vector2(Convert.ToInt32(array[i + 1]), Convert.ToInt32(array[i + 2])) * 64f + new Vector2(32f, 64f));
						break;
					case 405:
						lightGlows.Add(new Vector2(Convert.ToInt32(array[i + 1]), Convert.ToInt32(array[i + 2])) * 64f + new Vector2(32f, 32f));
						lightGlows.Add(new Vector2(Convert.ToInt32(array[i + 1]), Convert.ToInt32(array[i + 2])) * 64f + new Vector2(96f, 32f));
						break;
					case 469:
						lightGlows.Add(new Vector2(Convert.ToInt32(array[i + 1]), Convert.ToInt32(array[i + 2])) * 64f + new Vector2(32f, 36f));
						break;
					case 1224:
						lightGlows.Add(new Vector2(Convert.ToInt32(array[i + 1]), Convert.ToInt32(array[i + 2])) * 64f + new Vector2(32f, 32f));
						break;
					}
				}
			}
		}

		public NPC isCharacterAtTile(Vector2 tileLocation)
		{
			NPC result = null;
			tileLocation.X = (int)tileLocation.X;
			tileLocation.Y = (int)tileLocation.Y;
			if (currentEvent == null)
			{
				foreach (NPC character in characters)
				{
					if (character.getTileLocation().Equals(tileLocation))
					{
						return character;
					}
				}
				return result;
			}
			foreach (NPC actor in currentEvent.actors)
			{
				if (actor.getTileLocation().Equals(tileLocation))
				{
					return actor;
				}
			}
			return result;
		}

		public void ResetCharacterDialogues()
		{
			for (int num = characters.Count - 1; num >= 0; num--)
			{
				characters[num].resetCurrentDialogue();
			}
		}

		public string getMapProperty(string propertyName)
		{
			PropertyValue value = null;
			Map.Properties.TryGetValue(propertyName, out value);
			if (value == null)
			{
				return "";
			}
			return value.ToString();
		}

		public virtual void tryToAddCritters(bool onlyIfOnScreen = false)
		{
			if (Game1.CurrentEvent != null)
			{
				return;
			}
			double num = map.Layers[0].LayerWidth * map.Layers[0].LayerHeight;
			double num2 = Math.Max(0.15, Math.Min(0.5, num / 15000.0));
			double chance = num2;
			double chance2 = num2;
			double chance3 = num2 / 2.0;
			double chance4 = num2 / 2.0;
			double chance5 = num2 / 8.0;
			double num3 = num2 * 2.0;
			if (Game1.IsRainingHere(this))
			{
				return;
			}
			addClouds(num3 / (double)(onlyIfOnScreen ? 2f : 1f), onlyIfOnScreen);
			if (!(this is Beach) && critters != null && critters.Count <= (Game1.currentSeason.Equals("summer") ? 20 : 10))
			{
				addBirdies(chance, onlyIfOnScreen);
				addButterflies(chance2, onlyIfOnScreen);
				addBunnies(chance3, onlyIfOnScreen);
				addSquirrels(chance4, onlyIfOnScreen);
				addWoodpecker(chance5, onlyIfOnScreen);
				if (Game1.isDarkOut() && Game1.random.NextDouble() < 0.01)
				{
					addOwl();
				}
			}
		}

		public void addClouds(double chance, bool onlyIfOnScreen = false)
		{
			if (!Game1.currentSeason.Equals("summer") || Game1.IsRainingHere(this) || Game1.weatherIcon == 4 || Game1.timeOfDay >= Game1.getStartingToGetDarkTime() - 100)
			{
				return;
			}
			while (Game1.random.NextDouble() < Math.Min(0.9, chance))
			{
				Vector2 vector = getRandomTile();
				if (onlyIfOnScreen)
				{
					vector = ((Game1.random.NextDouble() < 0.5) ? new Vector2(map.Layers[0].LayerWidth, Game1.random.Next(map.Layers[0].LayerHeight)) : new Vector2(Game1.random.Next(map.Layers[0].LayerWidth), map.Layers[0].LayerHeight));
				}
				if (!onlyIfOnScreen && Utility.isOnScreen(vector * 64f, 1280))
				{
					continue;
				}
				Cloud cloud = new Cloud(vector);
				bool flag = true;
				if (critters != null)
				{
					foreach (Critter critter in critters)
					{
						if (critter is Cloud && critter.getBoundingBox(0, 0).Intersects(cloud.getBoundingBox(0, 0)))
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					addCritter(cloud);
				}
			}
		}

		public void addOwl()
		{
			critters.Add(new Owl(new Vector2(Game1.random.Next(64, map.Layers[0].LayerWidth * 64 - 64), -128f)));
		}

		public void setFireplace(bool on, int tileLocationX, int tileLocationY, bool playSound = true)
		{
			int num = 944468 + tileLocationX * 1000 + tileLocationY;
			if (on)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(tileLocationX, tileLocationY) * 64f + new Vector2(32f, -32f), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					light = true,
					lightID = num,
					id = num,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = ((float)tileLocationY + 1.1f) * 64f / 10000f
				});
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(tileLocationX + 1, tileLocationY) * 64f + new Vector2(-16f, -32f), flipped: false, 0f, Color.White)
				{
					delayBeforeAnimationStart = 10,
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					light = true,
					lightID = num,
					id = num,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = ((float)tileLocationY + 1.1f) * 64f / 10000f
				});
				if (playSound && Game1.gameMode != 6)
				{
					localSound("fireball");
				}
				AmbientLocationSounds.addSound(new Vector2(tileLocationX, tileLocationY), 1);
			}
			else
			{
				removeTemporarySpritesWithID(num);
				Utility.removeLightSource(num);
				if (playSound)
				{
					localSound("fireball");
				}
				AmbientLocationSounds.removeSound(new Vector2(tileLocationX, tileLocationY));
			}
		}

		public void addWoodpecker(double chance, bool onlyIfOnScreen = false)
		{
			if (Game1.isStartingToGetDarkOut() || onlyIfOnScreen || this is Town || this is Desert || !(Game1.random.NextDouble() < chance))
			{
				return;
			}
			for (int i = 0; i < 3; i++)
			{
				int index = Game1.random.Next(terrainFeatures.Count());
				if (terrainFeatures.Count() > 0 && terrainFeatures.Pairs.ElementAt(index).Value is Tree && (int)(terrainFeatures.Pairs.ElementAt(index).Value as Tree).treeType != 2 && (int)(terrainFeatures.Pairs.ElementAt(index).Value as Tree).growthStage >= 5)
				{
					critters.Add(new Woodpecker(terrainFeatures.Pairs.ElementAt(index).Value as Tree, terrainFeatures.Pairs.ElementAt(index).Key));
					break;
				}
			}
		}

		public void addSquirrels(double chance, bool onlyIfOnScreen = false)
		{
			if (Game1.isStartingToGetDarkOut() || onlyIfOnScreen || this is Farm || this is Town || this is Desert || !(Game1.random.NextDouble() < chance))
			{
				return;
			}
			for (int i = 0; i < 3; i++)
			{
				int index = Game1.random.Next(terrainFeatures.Count());
				if (terrainFeatures.Count() <= 0 || !(terrainFeatures.Pairs.ElementAt(index).Value is Tree) || (int)(terrainFeatures.Pairs.ElementAt(index).Value as Tree).growthStage < 5 || (bool)(terrainFeatures.Pairs.ElementAt(index).Value as Tree).stump)
				{
					continue;
				}
				Vector2 key = terrainFeatures.Pairs.ElementAt(index).Key;
				int num = Game1.random.Next(4, 7);
				bool flag = Game1.random.NextDouble() < 0.5;
				bool flag2 = true;
				for (int j = 0; j < num; j++)
				{
					key.X += (flag ? 1 : (-1));
					if (!isTileLocationTotallyClearAndPlaceable(key))
					{
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					critters.Add(new Squirrel(key, flag));
					break;
				}
			}
		}

		public void addBunnies(double chance, bool onlyIfOnScreen = false)
		{
			if (onlyIfOnScreen || this is Farm || this is Desert || !(Game1.random.NextDouble() < chance) || largeTerrainFeatures == null)
			{
				return;
			}
			for (int i = 0; i < 3; i++)
			{
				int index = Game1.random.Next(largeTerrainFeatures.Count);
				if (largeTerrainFeatures.Count <= 0 || !(largeTerrainFeatures[index] is Bush))
				{
					continue;
				}
				Vector2 vector = largeTerrainFeatures[index].tilePosition;
				int num = Game1.random.Next(5, 12);
				bool flag = Game1.random.NextDouble() < 0.5;
				bool flag2 = true;
				for (int j = 0; j < num; j++)
				{
					vector.X += (flag ? 1 : (-1));
					if (!largeTerrainFeatures[index].getBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle((int)vector.X * 64, (int)vector.Y * 64, 64, 64)) && !isTileLocationTotallyClearAndPlaceable(vector))
					{
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					critters.Add(new Rabbit(vector, flag));
					break;
				}
			}
		}

		public void instantiateCrittersList()
		{
			if (critters == null)
			{
				critters = new List<Critter>();
			}
		}

		public void addCritter(Critter c)
		{
			if (critters != null)
			{
				critters.Add(c);
			}
		}

		public void addButterflies(double chance, bool onlyIfOnScreen = false)
		{
			bool flag = GetLocationContext() == LocationContext.Island;
			bool flag2 = (Game1.currentSeason.Equals("summer") || flag) && Game1.isDarkOut();
			if ((Game1.timeOfDay >= 1500 && !flag2) || !(Game1.currentSeason.Equals("spring") || Game1.currentSeason.Equals("summer") || flag))
			{
				return;
			}
			chance = Math.Min(0.8, chance * 1.5);
			while (Game1.random.NextDouble() < chance)
			{
				Vector2 randomTile = getRandomTile();
				if (onlyIfOnScreen && Utility.isOnScreen(randomTile * 64f, 64))
				{
					continue;
				}
				if (flag2)
				{
					critters.Add(new Firefly(randomTile));
				}
				else
				{
					critters.Add(new Butterfly(randomTile, flag));
				}
				while (Game1.random.NextDouble() < 0.4)
				{
					if (flag2)
					{
						critters.Add(new Firefly(randomTile + new Vector2(Game1.random.Next(-2, 3), Game1.random.Next(-2, 3))));
					}
					else
					{
						critters.Add(new Butterfly(randomTile + new Vector2(Game1.random.Next(-2, 3), Game1.random.Next(-2, 3)), flag));
					}
				}
			}
		}

		public void addBirdies(double chance, bool onlyIfOnScreen = false)
		{
			if (Game1.timeOfDay >= 1500 || this is Desert || this is Railroad || this is Farm || Game1.currentSeason.Equals("summer"))
			{
				return;
			}
			while (Game1.random.NextDouble() < chance)
			{
				int num = Game1.random.Next(1, 4);
				bool flag = false;
				int num2 = 0;
				while (!flag && num2 < 5)
				{
					Vector2 randomTile = getRandomTile();
					if (!onlyIfOnScreen || !Utility.isOnScreen(randomTile * 64f, 64))
					{
						Microsoft.Xna.Framework.Rectangle area = new Microsoft.Xna.Framework.Rectangle((int)randomTile.X - 2, (int)randomTile.Y - 2, 5, 5);
						if (isAreaClear(area))
						{
							List<Critter> list = new List<Critter>();
							int startingIndex = (Game1.currentSeason.Equals("fall") ? 45 : 25);
							if (Game1.random.NextDouble() < 0.5 && Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
							{
								startingIndex = (Game1.currentSeason.Equals("fall") ? 135 : 125);
							}
							for (int i = 0; i < num; i++)
							{
								list.Add(new Birdie(-100, -100, startingIndex));
							}
							addCrittersStartingAtTile(randomTile, list);
							flag = true;
						}
					}
					num2++;
				}
			}
		}

		public void addJumperFrog(Vector2 tileLocation)
		{
			if (critters != null)
			{
				critters.Add(new Frog(tileLocation));
			}
		}

		public void addFrog()
		{
			if (!Game1.IsRainingHere(this) || Game1.currentSeason.Equals("winter"))
			{
				return;
			}
			for (int i = 0; i < 3; i++)
			{
				Vector2 randomTile = getRandomTile();
				if (doesTileHaveProperty((int)randomTile.X, (int)randomTile.Y, "Water", "Back") == null || doesTileHaveProperty((int)randomTile.X, (int)randomTile.Y - 1, "Water", "Back") == null || doesTileHaveProperty((int)randomTile.X, (int)randomTile.Y, "Passable", "Buildings") != null)
				{
					continue;
				}
				int num = 10;
				bool flag = Game1.random.NextDouble() < 0.5;
				for (int j = 0; j < num; j++)
				{
					randomTile.X += (flag ? 1 : (-1));
					if (isTileOnMap((int)randomTile.X, (int)randomTile.Y) && doesTileHaveProperty((int)randomTile.X, (int)randomTile.Y, "Water", "Back") == null)
					{
						critters.Add(new Frog(randomTile, waterLeaper: true, flag));
						return;
					}
				}
			}
		}

		public void checkForSpecialCharacterIconAtThisTile(Vector2 tileLocation)
		{
			if (currentEvent != null)
			{
				currentEvent.checkForSpecialCharacterIconAtThisTile(tileLocation);
			}
		}

		private void addCrittersStartingAtTile(Vector2 tile, List<Critter> crittersToAdd)
		{
			if (crittersToAdd == null)
			{
				return;
			}
			int num = 0;
			HashSet<Vector2> hashSet = new HashSet<Vector2>();
			while (crittersToAdd.Count > 0 && num < 20)
			{
				if (hashSet.Contains(tile))
				{
					tile = Utility.getTranslatedVector2(tile, Game1.random.Next(4), 1f);
				}
				else
				{
					if (isTileLocationTotallyClearAndPlaceable(tile))
					{
						crittersToAdd.Last().position = tile * 64f;
						crittersToAdd.Last().startingPosition = tile * 64f;
						critters.Add(crittersToAdd.Last());
						crittersToAdd.RemoveAt(crittersToAdd.Count - 1);
					}
					tile = Utility.getTranslatedVector2(tile, Game1.random.Next(4), 1f);
					hashSet.Add(tile);
				}
				num++;
			}
		}

		public bool isAreaClear(Microsoft.Xna.Framework.Rectangle area)
		{
			for (int i = area.Left; i < area.Right; i++)
			{
				for (int j = area.Top; j < area.Bottom; j++)
				{
					if (!isTileLocationTotallyClearAndPlaceable(new Vector2(i, j)))
					{
						return false;
					}
				}
			}
			return true;
		}

		public void refurbishMapPortion(Microsoft.Xna.Framework.Rectangle areaToRefurbish, string refurbishedMapName, Point mapReaderStartPoint)
		{
			Map map = Game1.game1.xTileContent.Load<Map>("Maps\\" + refurbishedMapName);
			Point point = mapReaderStartPoint;
			this.map.Properties.Remove("DayTiles");
			this.map.Properties.Remove("NightTiles");
			for (int i = 0; i < areaToRefurbish.Width; i++)
			{
				for (int j = 0; j < areaToRefurbish.Height; j++)
				{
					if (map.GetLayer("Back").Tiles[point.X + i, point.Y + j] != null)
					{
						this.map.GetLayer("Back").Tiles[areaToRefurbish.X + i, areaToRefurbish.Y + j] = new StaticTile(this.map.GetLayer("Back"), this.map.TileSheets[0], BlendMode.Alpha, map.GetLayer("Back").Tiles[point.X + i, point.Y + j].TileIndex);
						foreach (string key in map.GetLayer("Back").Tiles[point.X + i, point.Y + j].Properties.Keys)
						{
							this.map.GetLayer("Back").Tiles[areaToRefurbish.X + i, areaToRefurbish.Y + j].Properties.Add(key, map.GetLayer("Back").Tiles[point.X + i, point.Y + j].Properties[key]);
						}
					}
					if (map.GetLayer("Buildings").Tiles[point.X + i, point.Y + j] != null)
					{
						this.map.GetLayer("Buildings").Tiles[areaToRefurbish.X + i, areaToRefurbish.Y + j] = new StaticTile(this.map.GetLayer("Buildings"), this.map.TileSheets[0], BlendMode.Alpha, map.GetLayer("Buildings").Tiles[point.X + i, point.Y + j].TileIndex);
						adjustMapLightPropertiesForLamp(map.GetLayer("Buildings").Tiles[point.X + i, point.Y + j].TileIndex, areaToRefurbish.X + i, areaToRefurbish.Y + j, "Buildings");
						foreach (string key2 in map.GetLayer("Buildings").Tiles[point.X + i, point.Y + j].Properties.Keys)
						{
							this.map.GetLayer("Buildings").Tiles[areaToRefurbish.X + i, areaToRefurbish.Y + j].Properties.Add(key2, map.GetLayer("Back").Tiles[point.X + i, point.Y + j].Properties[key2]);
						}
					}
					else
					{
						this.map.GetLayer("Buildings").Tiles[areaToRefurbish.X + i, areaToRefurbish.Y + j] = null;
					}
					if (j < areaToRefurbish.Height - 1 && map.GetLayer("Front").Tiles[point.X + i, point.Y + j] != null)
					{
						this.map.GetLayer("Front").Tiles[areaToRefurbish.X + i, areaToRefurbish.Y + j] = new StaticTile(this.map.GetLayer("Front"), this.map.TileSheets[0], BlendMode.Alpha, map.GetLayer("Front").Tiles[point.X + i, point.Y + j].TileIndex);
						adjustMapLightPropertiesForLamp(map.GetLayer("Front").Tiles[point.X + i, point.Y + j].TileIndex, areaToRefurbish.X + i, areaToRefurbish.Y + j, "Front");
					}
					else if (j < areaToRefurbish.Height - 1)
					{
						this.map.GetLayer("Front").Tiles[areaToRefurbish.X + i, areaToRefurbish.Y + j] = null;
					}
				}
			}
		}

		public Vector2 getRandomTile()
		{
			return new Vector2(Game1.random.Next(Map.Layers[0].LayerWidth), Game1.random.Next(Map.Layers[0].LayerHeight));
		}

		public void setUpLocationSpecificFlair()
		{
			if (!isOutdoors && !(this is FarmHouse) && !(this is IslandFarmHouse))
			{
				map.Properties.TryGetValue("AmbientLight", out var value);
				if (value == null)
				{
					Game1.ambientLight = new Color(100, 120, 30);
				}
			}
			string text = name;
			if (text == null)
			{
				return;
			}
			switch (text.Length)
			{
			case 6:
				switch (text[1])
				{
				case 'u':
					if (text == "Summit")
					{
						Game1.ambientLight = Color.Black;
					}
					break;
				case 'a':
					if (!(text == "Saloon"))
					{
						break;
					}
					if (Game1.timeOfDay >= 1700)
					{
						setFireplace(on: true, 22, 17, playSound: false);
						Game1.changeMusicTrack("Saloon1");
					}
					if (Game1.random.NextDouble() < 0.25)
					{
						NPC characterFromName11 = Game1.getCharacterFromName("Gus");
						if (characterFromName11 != null && characterFromName11.getTileY() == 18 && characterFromName11.currentLocation == this)
						{
							string text6 = "";
							switch (Game1.random.Next(5))
							{
							case 0:
								text6 = "Greeting";
								break;
							case 1:
								text6 = (Game1.IsSummer ? "Summer" : "NotSummer");
								break;
							case 2:
								text6 = (Game1.isSnowing ? "Snowing1" : "NotSnowing1");
								break;
							case 3:
								text6 = (Game1.isRaining ? "Raining" : "NotRaining");
								break;
							case 4:
								text6 = (Game1.isSnowing ? "Snowing2" : "NotSnowing2");
								break;
							}
							if (Game1.random.NextDouble() < 0.001)
							{
								text6 = "RareGreeting";
							}
							characterFromName11.showTextAboveHead(Game1.content.LoadString("Strings\\SpeechBubbles:Saloon_Gus_" + text6));
						}
					}
					if (getCharacterFromName("Gus") == null && Game1.IsVisitingIslandToday("Gus"))
					{
						temporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.mouseCursors2,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(129, 210, 13, 16),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(129f, 210f),
							interval = 50000f,
							totalNumberOfLoops = 9999,
							position = new Vector2(11f, 18f) * 64f + new Vector2(3f, 0f) * 4f,
							scale = 4f,
							layerDepth = 0.1281f,
							id = 777f
						});
					}
					if (Game1.dayOfMonth % 7 == 0 && NetWorldState.checkAnywhereForWorldStateID("saloonSportsRoom") && Game1.timeOfDay < 1500)
					{
						Texture2D texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
						TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 336, 19, 14),
							animationLength = 7,
							sourceRectStartingPos = new Vector2(368f, 336f),
							interval = 5000f,
							totalNumberOfLoops = 99999,
							position = new Vector2(34f, 3f) * 64f + new Vector2(7f, 13f) * 4f,
							scale = 4f,
							layerDepth = 0.0401f,
							id = 2400f
						});
					}
					break;
				}
				break;
			case 9:
				switch (text[0])
				{
				case 'Q':
					if (text == "QiNutRoom")
					{
						Game1.changeMusicTrack("clubloop", track_interruptable: false, Game1.MusicContext.SubLocation);
						Game1.ambientLight = new Color(100, 120, 30);
					}
					break;
				case 'L':
				{
					if (!(text == "LeahHouse"))
					{
						break;
					}
					Game1.changeMusicTrack("distantBanjo");
					NPC characterFromName7 = Game1.getCharacterFromName("Leah");
					if (Game1.IsFall || Game1.IsWinter || Game1.isRaining)
					{
						setFireplace(on: true, 11, 4, playSound: false);
					}
					if (characterFromName7 != null && characterFromName7.currentLocation == this && !characterFromName7.isDivorcedFrom(Game1.player))
					{
						string path5 = "";
						switch (Game1.random.Next(3))
						{
						case 0:
							path5 = "Strings\\SpeechBubbles:LeahHouse_Leah_Greeting1";
							break;
						case 1:
							path5 = "Strings\\SpeechBubbles:LeahHouse_Leah_Greeting2";
							break;
						case 2:
							path5 = "Strings\\SpeechBubbles:LeahHouse_Leah_Greeting3";
							break;
						}
						characterFromName7.faceTowardFarmerForPeriod(3000, 15, faceAway: false, Game1.player);
						characterFromName7.showTextAboveHead(Game1.content.LoadString(path5, Game1.player.Name));
					}
					break;
				}
				}
				break;
			case 12:
				switch (text[0])
				{
				case 'L':
					if (text == "LeoTreeHouse")
					{
						temporarySprites.Add(new EmilysParrot(new Vector2(88f, 224f))
						{
							layerDepth = 1f,
							id = 5858585f
						});
						temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(71, 334, 12, 11), new Vector2(304f, 32f), flipped: false, 0f, Color.White)
						{
							layerDepth = 0.001f,
							interval = 700f,
							animationLength = 3,
							totalNumberOfLoops = 999999,
							scale = 4f
						});
						temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(47, 334, 12, 11), new Vector2(112f, -25.6f), flipped: true, 0f, Color.White)
						{
							layerDepth = 0.001f,
							interval = 300f,
							animationLength = 3,
							totalNumberOfLoops = 999999,
							scale = 4f
						});
						temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(71, 334, 12, 11), new Vector2(224f, -25.6f), flipped: false, 0f, Color.White)
						{
							layerDepth = 0.001f,
							interval = 800f,
							animationLength = 3,
							totalNumberOfLoops = 999999,
							scale = 4f
						});
						Game1.changeMusicTrack("sad_kid", track_interruptable: true, Game1.MusicContext.SubLocation);
					}
					break;
				case 'S':
					if (!(text == "ScienceHouse"))
					{
						break;
					}
					if (Game1.random.NextDouble() < 0.5 && Game1.player.currentLocation != null && (bool)Game1.player.currentLocation.isOutdoors)
					{
						NPC characterFromName6 = Game1.getCharacterFromName("Robin");
						if (characterFromName6 != null && characterFromName6.getTileY() == 18)
						{
							string path4 = "";
							switch (Game1.random.Next(4))
							{
							case 0:
								path4 = (Game1.isRaining ? "Strings\\SpeechBubbles:ScienceHouse_Robin_Raining1" : "Strings\\SpeechBubbles:ScienceHouse_Robin_NotRaining1");
								break;
							case 1:
								path4 = (Game1.isSnowing ? "Strings\\SpeechBubbles:ScienceHouse_Robin_Snowing" : "Strings\\SpeechBubbles:ScienceHouse_Robin_NotSnowing");
								break;
							case 2:
								path4 = ((Game1.player.getFriendshipHeartLevelForNPC("Robin") > 4) ? "Strings\\SpeechBubbles:ScienceHouse_Robin_CloseFriends" : "Strings\\SpeechBubbles:ScienceHouse_Robin_NotCloseFriends");
								break;
							case 3:
								path4 = (Game1.isRaining ? "Strings\\SpeechBubbles:ScienceHouse_Robin_Raining2" : "Strings\\SpeechBubbles:ScienceHouse_Robin_NotRaining2");
								break;
							case 4:
								path4 = "Strings\\SpeechBubbles:ScienceHouse_Robin_Greeting";
								break;
							}
							if (Game1.random.NextDouble() < 0.001)
							{
								path4 = "Strings\\SpeechBubbles:ScienceHouse_Robin_RareGreeting";
							}
							characterFromName6.showTextAboveHead(Game1.content.LoadString(path4, Game1.player.Name));
						}
					}
					if (getCharacterFromName("Robin") == null && Game1.IsVisitingIslandToday("Robin"))
					{
						temporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.mouseCursors2,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(129, 210, 13, 16),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(129f, 210f),
							interval = 50000f,
							totalNumberOfLoops = 9999,
							position = new Vector2(7f, 18f) * 64f + new Vector2(3f, 0f) * 4f,
							scale = 4f,
							layerDepth = 0.1281f,
							id = 777f
						});
					}
					break;
				case 'E':
				{
					if (!(text == "ElliottHouse"))
					{
						break;
					}
					Game1.changeMusicTrack("communityCenter");
					NPC characterFromName5 = Game1.getCharacterFromName("Elliott");
					if (characterFromName5 != null && characterFromName5.currentLocation == this && !characterFromName5.isDivorcedFrom(Game1.player))
					{
						string path3 = "";
						switch (Game1.random.Next(3))
						{
						case 0:
							path3 = "Strings\\SpeechBubbles:ElliottHouse_Elliott_Greeting1";
							break;
						case 1:
							path3 = "Strings\\SpeechBubbles:ElliottHouse_Elliott_Greeting2";
							break;
						case 2:
							path3 = "Strings\\SpeechBubbles:ElliottHouse_Elliott_Greeting3";
							break;
						}
						characterFromName5.faceTowardFarmerForPeriod(3000, 15, faceAway: false, Game1.player);
						characterFromName5.showTextAboveHead(Game1.content.LoadString(path3, Game1.player.Name));
					}
					break;
				}
				}
				break;
			case 7:
				switch (text[0])
				{
				case 'S':
					if (!(text == "Sunroom"))
					{
						break;
					}
					Game1.ambientLight = new Color(0, 0, 0);
					AmbientLocationSounds.addSound(new Vector2(3f, 4f), 0);
					if (largeTerrainFeatures.Count == 0)
					{
						Bush bush = new Bush(new Vector2(6f, 7f), 3, this, -999);
						bush.greenhouseBush.Value = true;
						bush.loadSprite();
						bush.health = 99f;
						largeTerrainFeatures.Add(bush);
					}
					if (!Game1.IsRainingHere(this))
					{
						Game1.changeMusicTrack("SunRoom", track_interruptable: false, Game1.MusicContext.SubLocation);
						critters = new List<Critter>();
						critters.Add(new Butterfly(getRandomTile()).setStayInbounds(stayInbounds: true));
						while (Game1.random.NextDouble() < 0.5)
						{
							critters.Add(new Butterfly(getRandomTile()).setStayInbounds(stayInbounds: true));
						}
					}
					break;
				case 'B':
					if (!(text == "BugLand"))
					{
						break;
					}
					if (!Game1.player.hasDarkTalisman && isTileLocationTotallyClearAndPlaceable(31, 5))
					{
						overlayObjects.Add(new Vector2(31f, 5f), new Chest(0, new List<Item>
						{
							new SpecialItem(6)
						}, new Vector2(31f, 5f))
						{
							Tint = Color.Gray
						});
					}
					{
						foreach (NPC character in characters)
						{
							if (character is Grub)
							{
								(character as Grub).setHard();
							}
							else if (character is Fly)
							{
								(character as Fly).setHard();
							}
						}
						break;
					}
				}
				break;
			case 8:
				switch (text[0])
				{
				case 'W':
					if (text == "WitchHut" && Game1.player.mailReceived.Contains("cursed_doll") && !farmers.Any())
					{
						characters.Clear();
						addCharacter(new Bat(new Vector2(7f, 6f) * 64f, -666));
						if (Game1.stats.getStat("childrenTurnedToDoves") > 1)
						{
							addCharacter(new Bat(new Vector2(4f, 7f) * 64f, -666));
						}
						if (Game1.stats.getStat("childrenTurnedToDoves") > 2)
						{
							addCharacter(new Bat(new Vector2(10f, 7f) * 64f, -666));
						}
						for (int i = 4; i <= Game1.stats.getStat("childrenTurnedToDoves"); i++)
						{
							addCharacter(new Bat(Utility.getRandomPositionInThisRectangle(new Microsoft.Xna.Framework.Rectangle(1, 4, 13, 4), Game1.random) * 64f + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), -666));
						}
					}
					break;
				case 'H':
				{
					if (!(text == "Hospital"))
					{
						break;
					}
					if (!Game1.isRaining)
					{
						Game1.changeMusicTrack("distantBanjo");
					}
					Game1.ambientLight = new Color(100, 100, 60);
					if (!(Game1.random.NextDouble() < 0.5))
					{
						break;
					}
					NPC characterFromName10 = Game1.getCharacterFromName("Maru");
					if (characterFromName10 != null && characterFromName10.currentLocation == this && !characterFromName10.isDivorcedFrom(Game1.player))
					{
						string path7 = "";
						switch (Game1.random.Next(5))
						{
						case 0:
							path7 = "Strings\\SpeechBubbles:Hospital_Maru_Greeting1";
							break;
						case 1:
							path7 = "Strings\\SpeechBubbles:Hospital_Maru_Greeting2";
							break;
						case 2:
							path7 = "Strings\\SpeechBubbles:Hospital_Maru_Greeting3";
							break;
						case 3:
							path7 = "Strings\\SpeechBubbles:Hospital_Maru_Greeting4";
							break;
						case 4:
							path7 = "Strings\\SpeechBubbles:Hospital_Maru_Greeting5";
							break;
						}
						if (Game1.player.spouse != null && Game1.player.spouse.Equals("Maru"))
						{
							path7 = "Strings\\SpeechBubbles:Hospital_Maru_Spouse";
							characterFromName10.showTextAboveHead(Game1.content.LoadString(path7), 2);
						}
						else
						{
							characterFromName10.showTextAboveHead(Game1.content.LoadString(path7));
						}
					}
					break;
				}
				case 'J':
					if (!(text == "JojaMart"))
					{
						break;
					}
					Game1.changeMusicTrack("Hospital_Ambient");
					Game1.ambientLight = new Color(0, 0, 0);
					if (Game1.random.NextDouble() < 0.5)
					{
						NPC characterFromName9 = Game1.getCharacterFromName("Morris");
						if (characterFromName9 != null && characterFromName9.currentLocation == this)
						{
							string path6 = "Strings\\SpeechBubbles:JojaMart_Morris_Greeting";
							characterFromName9.showTextAboveHead(Game1.content.LoadString(path6));
						}
					}
					break;
				case 'S':
					if (!(text == "SeedShop"))
					{
						break;
					}
					setFireplace(on: true, 25, 13, playSound: false);
					if (Game1.random.NextDouble() < 0.5 && Game1.player.getTileY() > 10)
					{
						NPC characterFromName8 = Game1.getCharacterFromName("Pierre");
						if (characterFromName8 != null && characterFromName8.getTileY() == 17 && characterFromName8.currentLocation == this)
						{
							string text4 = "";
							switch (Game1.random.Next(5))
							{
							case 0:
								text4 = (Game1.IsWinter ? "Winter" : "NotWinter");
								break;
							case 1:
								text4 = (Game1.IsSummer ? "Summer" : "NotSummer");
								break;
							case 2:
								text4 = "Greeting1";
								break;
							case 3:
								text4 = "Greeting2";
								break;
							case 4:
								text4 = (Game1.isRaining ? "Raining" : "NotRaining");
								break;
							}
							if (Game1.random.NextDouble() < 0.001)
							{
								text4 = "RareGreeting";
							}
							string text5 = Game1.content.LoadString("Strings\\SpeechBubbles:SeedShop_Pierre_" + text4);
							if (text5.Contains('^'))
							{
								text5 = ((!Game1.player.IsMale) ? text5.Split('^')[1] : text5.Split('^')[0]);
							}
							characterFromName8.showTextAboveHead(string.Format(text5, Game1.player.Name));
						}
					}
					if (getCharacterFromName("Pierre") == null && Game1.IsVisitingIslandToday("Pierre"))
					{
						temporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.mouseCursors2,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(129, 210, 13, 16),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(129f, 210f),
							interval = 50000f,
							totalNumberOfLoops = 9999,
							position = new Vector2(5f, 17f) * 64f + new Vector2(3f, 0f) * 4f,
							scale = 4f,
							layerDepth = 0.1217f,
							id = 777f
						});
					}
					break;
				}
				break;
			case 10:
				switch (text[0])
				{
				case 'H':
					if (text == "HaleyHouse" && Game1.player.eventsSeen.Contains(463391) && (Game1.player.spouse == null || !Game1.player.spouse.Equals("Emily")))
					{
						temporarySprites.Add(new EmilysParrot(new Vector2(912f, 160f)));
					}
					break;
				case 'A':
					if (!(text == "AnimalShop"))
					{
						break;
					}
					setFireplace(on: true, 3, 14, playSound: false);
					if (Game1.random.NextDouble() < 0.5)
					{
						NPC characterFromName4 = Game1.getCharacterFromName("Marnie");
						if (characterFromName4 != null && characterFromName4.getTileY() == 14)
						{
							string path2 = "";
							switch (Game1.random.Next(4))
							{
							case 0:
								path2 = "Strings\\SpeechBubbles:AnimalShop_Marnie_Greeting1";
								break;
							case 1:
								path2 = "Strings\\SpeechBubbles:AnimalShop_Marnie_Greeting2";
								break;
							case 2:
								path2 = ((Game1.player.getFriendshipHeartLevelForNPC("Marnie") > 4) ? "Strings\\SpeechBubbles:AnimalShop_Marnie_CloseFriends" : "Strings\\SpeechBubbles:AnimalShop_Marnie_NotCloseFriends");
								break;
							case 3:
								path2 = (Game1.isRaining ? "Strings\\SpeechBubbles:AnimalShop_Marnie_Raining" : "Strings\\SpeechBubbles:AnimalShop_Marnie_NotRaining");
								break;
							case 4:
								path2 = "Strings\\SpeechBubbles:AnimalShop_Marnie_Greeting3";
								break;
							}
							if (Game1.random.NextDouble() < 0.001)
							{
								path2 = "Strings\\SpeechBubbles:AnimalShop_Marnie_RareGreeting";
							}
							characterFromName4.showTextAboveHead(Game1.content.LoadString(path2, Game1.player.Name, Game1.player.farmName));
						}
					}
					if (getCharacterFromName("Marnie") == null && Game1.IsVisitingIslandToday("Marnie"))
					{
						temporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.mouseCursors2,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(129, 210, 13, 16),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(129f, 210f),
							interval = 50000f,
							totalNumberOfLoops = 9999,
							position = new Vector2(13f, 14f) * 64f + new Vector2(3f, 0f) * 4f,
							scale = 4f,
							layerDepth = 0.1025f,
							id = 777f
						});
					}
					if (Game1.netWorldState.Value.hasWorldStateID("m_painting0"))
					{
						temporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.mouseCursors,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(25, 1925, 25, 23),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(25f, 1925f),
							interval = 5000f,
							totalNumberOfLoops = 9999,
							position = new Vector2(16f, 1f) * 64f + new Vector2(3f, 1f) * 4f,
							scale = 4f,
							layerDepth = 0.1f,
							id = 777f
						});
					}
					else if (Game1.netWorldState.Value.hasWorldStateID("m_painting1"))
					{
						temporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.mouseCursors,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 1925, 25, 23),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(0f, 1925f),
							interval = 5000f,
							totalNumberOfLoops = 9999,
							position = new Vector2(16f, 1f) * 64f + new Vector2(3f, 1f) * 4f,
							scale = 4f,
							layerDepth = 0.1f,
							id = 777f
						});
					}
					else if (Game1.netWorldState.Value.hasWorldStateID("m_painting2"))
					{
						temporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.mouseCursors,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 1948, 25, 24),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(0f, 1948f),
							interval = 5000f,
							totalNumberOfLoops = 9999,
							position = new Vector2(16f, 1f) * 64f + new Vector2(3f, 1f) * 4f,
							scale = 4f,
							layerDepth = 0.1f,
							id = 777f
						});
					}
					break;
				case 'B':
					if (text == "Blacksmith")
					{
						AmbientLocationSounds.addSound(new Vector2(9f, 10f), 2);
						AmbientLocationSounds.changeSpecificVariable("Frequency", 2f, 2);
						Game1.changeMusicTrack("none");
					}
					break;
				case 'S':
				{
					if (!(text == "SandyHouse"))
					{
						break;
					}
					Game1.changeMusicTrack("distantBanjo");
					Game1.ambientLight = new Color(0, 0, 0);
					if (!(Game1.random.NextDouble() < 0.5))
					{
						break;
					}
					NPC characterFromName2 = Game1.getCharacterFromName("Sandy");
					if (characterFromName2 != null && characterFromName2.currentLocation == this)
					{
						string path = "";
						switch (Game1.random.Next(5))
						{
						case 0:
							path = "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting1";
							break;
						case 1:
							path = "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting2";
							break;
						case 2:
							path = "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting3";
							break;
						case 3:
							path = "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting4";
							break;
						case 4:
							path = "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting5";
							break;
						}
						characterFromName2.showTextAboveHead(Game1.content.LoadString(path));
					}
					break;
				}
				case 'M':
					if (text == "ManorHouse")
					{
						Game1.ambientLight = new Color(150, 120, 50);
						NPC characterFromName3 = Game1.getCharacterFromName("Lewis");
						if (characterFromName3 != null && characterFromName3.currentLocation == this)
						{
							string text3 = "";
							text3 = ((Game1.timeOfDay < 1200) ? "Morning" : ((Game1.timeOfDay < 1700) ? "Afternoon" : "Evening"));
							characterFromName3.faceTowardFarmerForPeriod(3000, 15, faceAway: false, Game1.player);
							characterFromName3.showTextAboveHead(Game1.content.LoadString("Strings\\SpeechBubbles:ManorHouse_Lewis_" + text3));
						}
					}
					break;
				case 'G':
					if (text == "Greenhouse" && Game1.isDarkOut())
					{
						Game1.ambientLight = Game1.outdoorLight;
					}
					break;
				}
				break;
			case 17:
				if (text == "AbandonedJojaMart" && !Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
				{
					Point point = new Point(8, 8);
					Game1.currentLightSources.Add(new LightSource(4, new Vector2(point.X * 64, point.Y * 64), 1f, LightSource.LightContext.None, 0L));
					temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(point.X * 64, point.Y * 64), Color.White)
					{
						layerDepth = 1f,
						interval = 50f,
						motion = new Vector2(1f, 0f),
						acceleration = new Vector2(-0.005f, 0f)
					});
					temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(point.X * 64 - 12, point.Y * 64 - 12), Color.White)
					{
						scale = 0.75f,
						layerDepth = 1f,
						interval = 50f,
						motion = new Vector2(1f, 0f),
						acceleration = new Vector2(-0.005f, 0f),
						delayBeforeAnimationStart = 50
					});
					temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(point.X * 64 - 12, point.Y * 64 + 12), Color.White)
					{
						layerDepth = 1f,
						interval = 50f,
						motion = new Vector2(1f, 0f),
						acceleration = new Vector2(-0.005f, 0f),
						delayBeforeAnimationStart = 100
					});
					temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(point.X * 64, point.Y * 64), Color.White)
					{
						layerDepth = 1f,
						scale = 0.75f,
						interval = 50f,
						motion = new Vector2(1f, 0f),
						acceleration = new Vector2(-0.005f, 0f),
						delayBeforeAnimationStart = 150
					});
					if (characters.Count == 0)
					{
						characters.Add(new Junimo(new Vector2(8f, 7f) * 64f, 6));
					}
				}
				break;
			case 15:
				if (text == "CommunityCenter" && this is CommunityCenter && (Game1.isLocationAccessible("CommunityCenter") || (currentEvent != null && currentEvent.id == 191393)))
				{
					setFireplace(on: true, 31, 8, playSound: false);
					setFireplace(on: true, 32, 8, playSound: false);
					setFireplace(on: true, 33, 8, playSound: false);
				}
				break;
			case 14:
			{
				if (!(text == "AdventureGuild"))
				{
					break;
				}
				setFireplace(on: true, 9, 11, playSound: false);
				if (!(Game1.random.NextDouble() < 0.5))
				{
					break;
				}
				NPC characterFromName12 = Game1.getCharacterFromName("Marlon");
				if (characterFromName12 != null)
				{
					string path8 = "";
					switch (Game1.random.Next(5))
					{
					case 0:
						path8 = "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting_" + (Game1.player.IsMale ? "Male" : "Female");
						break;
					case 1:
						path8 = "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting1";
						break;
					case 2:
						path8 = "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting2";
						break;
					case 3:
						path8 = "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting3";
						break;
					case 4:
						path8 = "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting4";
						break;
					}
					characterFromName12.showTextAboveHead(Game1.content.LoadString(path8));
				}
				break;
			}
			case 16:
			{
				if (!(text == "ArchaeologyHouse"))
				{
					break;
				}
				setFireplace(on: true, 43, 4, playSound: false);
				if (!(Game1.random.NextDouble() < 0.5) || !Game1.player.hasOrWillReceiveMail("artifactFound"))
				{
					break;
				}
				NPC characterFromName = Game1.getCharacterFromName("Gunther");
				if (characterFromName != null && characterFromName.currentLocation == this)
				{
					string text2 = "";
					switch (Game1.random.Next(5))
					{
					case 0:
						text2 = "Greeting1";
						break;
					case 1:
						text2 = "Greeting2";
						break;
					case 2:
						text2 = "Greeting3";
						break;
					case 3:
						text2 = "Greeting4";
						break;
					case 4:
						text2 = "Greeting5";
						break;
					}
					if (Game1.random.NextDouble() < 0.001)
					{
						text2 = "RareGreeting";
					}
					characterFromName.showTextAboveHead(Game1.content.LoadString("Strings\\SpeechBubbles:ArchaeologyHouse_Gunther_" + text2));
				}
				break;
			}
			case 11:
			case 13:
				break;
			}
		}

		public virtual void hostSetup()
		{
			if (Game1.IsMasterGame && !farmers.Any() && !HasFarmerWatchingBroadcastEventReturningHere())
			{
				interiorDoors.ResetSharedState();
			}
		}

		public virtual bool HasFarmerWatchingBroadcastEventReturningHere()
		{
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer.locationBeforeForcedEvent.Value != null && allFarmer.locationBeforeForcedEvent.Value == NameOrUniqueName)
				{
					return true;
				}
			}
			return false;
		}

		public void resetForPlayerEntry()
		{
			Game1.updateWeatherIcon();
			bool flag = false;
			Game1.hooks.OnGameLocation_ResetForPlayerEntry(this, delegate
			{
				_madeMapModifications = false;
				if (!farmers.Any() && !HasFarmerWatchingBroadcastEventReturningHere())
				{
					resetSharedState();
				}
				resetLocalState();
				if (!_madeMapModifications)
				{
					_madeMapModifications = true;
					MakeMapModifications();
				}
			});
			Microsoft.Xna.Framework.Rectangle boundingBox = Game1.player.GetBoundingBox();
			foreach (Furniture item in furniture)
			{
				if (item.getBoundingBox(item.TileLocation).Intersects(boundingBox) && item.IntersectsForCollision(boundingBox) && !item.isPassable())
				{
					Game1.player.TemporaryPassableTiles.Add(item.getBoundingBox(item.TileLocation));
				}
			}
			Game1.options.platformClampValues();
			Program.gamePtr.refreshWindowSettings();
		}

		protected virtual void resetLocalState()
		{
			Game1.elliottPiano = 0;
			Game1.crabPotOverlayTiles.Clear();
			Utility.killAllStaticLoopingSoundCues();
			Game1.player.bridge = null;
			Game1.player.SetOnBridge(val: false);
			if (Game1.CurrentEvent == null && !Name.ToLower().Contains("bath"))
			{
				Game1.player.canOnlyWalk = false;
			}
			if (!(this is Farm))
			{
				for (int num = temporarySprites.Count - 1; num >= 0; num--)
				{
					if (temporarySprites[num].clearOnAreaEntry())
					{
						temporarySprites.RemoveAt(num);
					}
				}
			}
			if (Game1.options != null)
			{
				if (Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.runButton))
				{
					Game1.player.setRunning(!Game1.options.autoRun, force: true);
				}
				else
				{
					Game1.player.setRunning(Game1.options.autoRun, force: true);
				}
			}
			Game1.UpdateViewPort(overrideFreeze: false, new Point(Game1.player.getStandingX(), Game1.player.getStandingY()));
			Game1.previousViewportPosition = new Vector2(Game1.viewport.X, Game1.viewport.Y);
			lock (Game1.RenderLock)
			{
				Game1.PushUIMode();
				foreach (IClickableMenu onScreenMenu in Game1.onScreenMenus)
				{
					onScreenMenu.gameWindowSizeChanged(new Microsoft.Xna.Framework.Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height), new Microsoft.Xna.Framework.Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height));
				}
				Game1.PopUIMode();
			}
			ignoreWarps = false;
			if (Game1.newDaySync == null || Game1.newDaySync.hasFinished())
			{
				if (Game1.player.rightRing.Value != null)
				{
					Game1.player.rightRing.Value.onNewLocation(Game1.player, this);
				}
				if (Game1.player.leftRing.Value != null)
				{
					Game1.player.leftRing.Value.onNewLocation(Game1.player, this);
				}
			}
			forceViewportPlayerFollow = Map.Properties.ContainsKey("ViewportFollowPlayer");
			lastTouchActionLocation = Game1.player.getTileLocation();
			for (int num2 = Game1.player.questLog.Count - 1; num2 >= 0; num2--)
			{
				Game1.player.questLog[num2].adjustGameLocation(this);
			}
			if (!isOutdoors)
			{
				Game1.player.FarmerSprite.currentStep = "thudStep";
			}
			_updateAmbientLighting();
			setUpLocationSpecificFlair();
			map.Properties.TryGetValue("UniquePortrait", out var value);
			if (value != null)
			{
				string[] array = value.ToString().Split(' ');
				string[] array2 = array;
				foreach (string text in array2)
				{
					NPC characterFromName = Game1.getCharacterFromName(text);
					if (characters.Contains(characterFromName))
					{
						try
						{
							characterFromName.Portrait = Game1.content.Load<Texture2D>("Portraits\\" + characterFromName.Name + "_" + name);
							characterFromName.uniquePortraitActive = true;
						}
						catch (Exception)
						{
						}
					}
				}
			}
			map.Properties.TryGetValue("Light", out var value2);
			if (value2 != null && !ignoreLights)
			{
				string[] array3 = value2.ToString().Split(' ');
				for (int j = 0; j < array3.Length; j += 3)
				{
					Game1.currentLightSources.Add(new LightSource(Convert.ToInt32(array3[j + 2]), new Vector2(Convert.ToInt32(array3[j]) * 64 + 32, Convert.ToInt32(array3[j + 1]) * 64 + 32), 1f, LightSource.LightContext.MapLight, 0L));
				}
			}
			value2 = null;
			map.Properties.TryGetValue("WindowLight", out value2);
			if (value2 != null && !ignoreLights)
			{
				string[] array4 = value2.ToString().Split(' ');
				for (int k = 0; k < array4.Length; k += 3)
				{
					Game1.currentLightSources.Add(new LightSource(Convert.ToInt32(array4[k + 2]), new Vector2(Convert.ToInt32(array4[k]) * 64 + 32, Convert.ToInt32(array4[k + 1]) * 64 + 32), 1f, LightSource.LightContext.WindowLight, 0L));
				}
			}
			if ((bool)isOutdoors || (bool)treatAsOutdoors)
			{
				map.Properties.TryGetValue("BrookSounds", out var value3);
				if (value3 != null)
				{
					string[] array5 = value3.ToString().Split(' ');
					for (int l = 0; l < array5.Length; l += 3)
					{
						AmbientLocationSounds.addSound(new Vector2(Convert.ToInt32(array5[l]), Convert.ToInt32(array5[l + 1])), Convert.ToInt32(array5[l + 2]));
					}
				}
				Game1.randomizeRainPositions();
				Game1.randomizeDebrisWeatherPositions(Game1.debrisWeather);
			}
			foreach (KeyValuePair<Vector2, TerrainFeature> pair in terrainFeatures.Pairs)
			{
				pair.Value.performPlayerEntryAction(pair.Key);
			}
			if (largeTerrainFeatures != null)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					largeTerrainFeature.performPlayerEntryAction(largeTerrainFeature.tilePosition);
				}
			}
			foreach (KeyValuePair<Vector2, Object> pair2 in objects.Pairs)
			{
				pair2.Value.actionOnPlayerEntry();
			}
			if ((bool)isOutdoors && Game1.shouldPlayMorningSong())
			{
				Game1.playMorningSong();
			}
			PropertyValue value4 = null;
			map.Properties.TryGetValue("Music", out value4);
			if (value4 != null)
			{
				string[] array6 = value4.ToString().Split(' ');
				if (array6.Length > 1)
				{
					if (Game1.timeOfDay >= Convert.ToInt32(array6[0]) && Game1.timeOfDay < Convert.ToInt32(array6[1]) && !array6[2].Equals(Game1.getMusicTrackName()))
					{
						Game1.changeMusicTrack(array6[2]);
					}
				}
				else if (Game1.getMusicTrackName() == "none" || Game1.isMusicContextActiveButNotPlaying() || !array6[0].Equals(Game1.getMusicTrackName()))
				{
					Game1.changeMusicTrack(array6[0]);
				}
			}
			if ((bool)isOutdoors)
			{
				((FarmerSprite)Game1.player.Sprite).currentStep = "sandyStep";
				tryToAddCritters();
			}
			interiorDoors.ResetLocalState();
			int trulyDarkTime = Game1.getTrulyDarkTime();
			if (Game1.timeOfDay < trulyDarkTime && (!Game1.IsRainingHere(this) || name.Equals("SandyHouse")))
			{
				map.Properties.TryGetValue("DayTiles", out var value5);
				if (value5 != null)
				{
					string[] array7 = value5.ToString().Trim().Split(' ');
					for (int m = 0; m < array7.Length; m += 4)
					{
						if ((!array7[m + 3].Equals("720") || !Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade")) && map.GetLayer(array7[m]).Tiles[Convert.ToInt32(array7[m + 1]), Convert.ToInt32(array7[m + 2])] != null)
						{
							map.GetLayer(array7[m]).Tiles[Convert.ToInt32(array7[m + 1]), Convert.ToInt32(array7[m + 2])].TileIndex = Convert.ToInt32(array7[m + 3]);
						}
					}
				}
			}
			else if (Game1.timeOfDay >= trulyDarkTime || (Game1.IsRainingHere(this) && !name.Equals("SandyHouse")))
			{
				switchOutNightTiles();
			}
			if (name.Equals("Coop"))
			{
				string[] array8 = getMapProperty("Feed").Split(' ');
				if (Game1.MasterPlayer.Feed <= 0)
				{
					map.GetLayer("Buildings").Tiles[Convert.ToInt32(array8[0]), Convert.ToInt32(array8[1])].TileIndex = 35;
				}
				else
				{
					map.GetLayer("Buildings").Tiles[Convert.ToInt32(array8[0]), Convert.ToInt32(array8[1])].TileIndex = 31;
				}
			}
			else if (name.Equals("Barn"))
			{
				string[] array9 = getMapProperty("Feed").Split(' ');
				if (Game1.MasterPlayer.Feed <= 0)
				{
					map.GetLayer("Buildings").Tiles[Convert.ToInt32(array9[0]), Convert.ToInt32(array9[1])].TileIndex = 35;
				}
				else
				{
					map.GetLayer("Buildings").Tiles[Convert.ToInt32(array9[0]), Convert.ToInt32(array9[1])].TileIndex = 31;
				}
			}
			if (name.Equals("Club"))
			{
				Game1.changeMusicTrack("clubloop");
			}
			else if (Game1.getMusicTrackName().Equals("clubloop"))
			{
				Game1.changeMusicTrack("none");
			}
			if (Game1.killScreen && Game1.activeClickableMenu != null && !Game1.dialogueUp)
			{
				Game1.activeClickableMenu.emergencyShutDown();
				Game1.exitActiveMenu();
			}
			if (Game1.activeClickableMenu == null && !Game1.warpingForForcedRemoteEvent)
			{
				checkForEvents();
			}
			Game1.currentLightSources.UnionWith(sharedLights.Values);
			foreach (NPC character in characters)
			{
				character.behaviorOnLocalFarmerLocationEntry(this);
			}
			foreach (Furniture item in furniture)
			{
				item.resetOnPlayerEntry(this);
			}
			updateFishSplashAnimation();
			updateOrePanAnimation();
			showDropboxIndicator = false;
			foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
			{
				if (specialOrder.ShouldDisplayAsComplete())
				{
					continue;
				}
				foreach (OrderObjective objective in specialOrder.objectives)
				{
					if (objective is DonateObjective)
					{
						DonateObjective donateObjective = objective as DonateObjective;
						if (donateObjective.dropBoxGameLocation != null && donateObjective.GetDropboxLocationName() == Name)
						{
							showDropboxIndicator = true;
							dropBoxIndicatorLocation = donateObjective.dropBoxTileLocation.Value * 64f + new Vector2(7f, 0f) * 4f;
						}
					}
				}
			}
			if (tapToMove != null && tapToMove.aStarGraph != null)
			{
				tapToMove.aStarGraph.RefreshBubbles();
			}
		}

		protected void _updateAmbientLighting()
		{
			if (!isOutdoors || (bool)ignoreOutdoorLighting)
			{
				map.Properties.TryGetValue("AmbientLight", out var value);
				if (value != null)
				{
					string[] array = value.ToString().Split(' ');
					Game1.ambientLight = new Color(Convert.ToInt32(array[0]), Convert.ToInt32(array[1]), Convert.ToInt32(array[2]));
				}
				else if (Game1.isDarkOut() || (float)lightLevel > 0f)
				{
					Game1.ambientLight = new Color(180, 180, 0);
				}
				else
				{
					Game1.ambientLight = Color.White;
				}
				if (Game1.getMusicTrackName().Contains("ambient"))
				{
					Game1.changeMusicTrack("none", Game1.currentTrackOverrideable);
				}
			}
			else if (!(this is Desert))
			{
				Game1.ambientLight = (Game1.IsRainingHere(this) ? new Color(255, 200, 80) : Color.White);
			}
		}

		public void SelectRandomMiniJukeboxTrack()
		{
			if (!(miniJukeboxTrack.Value != "random"))
			{
				Farmer farmer = Game1.player;
				if (this is FarmHouse && (this as FarmHouse).owner != null)
				{
					farmer = (this as FarmHouse).owner;
				}
				List<string> list = farmer.songsHeard.Distinct().ToList();
				ChooseFromListMenu.FilterJukeboxTracks(list);
				string random = Utility.GetRandom(list);
				randomMiniJukeboxTrack.Value = random;
			}
		}

		protected virtual void resetSharedState()
		{
			SelectRandomMiniJukeboxTrack();
			for (int num = characters.Count - 1; num >= 0; num--)
			{
				characters[num].behaviorOnFarmerLocationEntry(this, Game1.player);
			}
			Map.Properties.TryGetValue("UniqueSprite", out var value);
			if (value != null)
			{
				string[] array = value.ToString().Split(' ');
				string[] array2 = array;
				foreach (string text in array2)
				{
					NPC characterFromName = Game1.getCharacterFromName(text);
					if (characters.Contains(characterFromName))
					{
						try
						{
							characterFromName.Sprite.LoadTexture("Characters\\" + NPC.getTextureNameForCharacter(characterFromName.Name) + "_" + name);
							characterFromName.uniqueSpriteActive = true;
						}
						catch (Exception)
						{
						}
					}
				}
			}
			if (!(this is MineShaft))
			{
				switch (Game1.currentSeason.ToLower())
				{
				case "spring":
					waterColor.Value = new Color(120, 200, 255) * 0.5f;
					break;
				case "summer":
					waterColor.Value = new Color(60, 240, 255) * 0.5f;
					break;
				case "fall":
					waterColor.Value = new Color(255, 130, 200) * 0.5f;
					break;
				case "winter":
					waterColor.Value = new Color(130, 80, 255) * 0.5f;
					break;
				}
			}
			if (name.Equals("Mountain") && (Game1.timeOfDay < 2000 || !Game1.currentSeason.Equals("summer") || !(Game1.random.NextDouble() < 0.3)) && Game1.IsRainingHere(this) && !Game1.currentSeason.Equals("winter"))
			{
				Game1.random.NextDouble();
				_ = 0.2;
			}
		}

		public LightSource getLightSource(int identifier)
		{
			sharedLights.TryGetValue(identifier, out var value);
			return value;
		}

		public bool hasLightSource(int identifier)
		{
			return sharedLights.ContainsKey(identifier);
		}

		public void removeLightSource(int identifier)
		{
			sharedLights.Remove(identifier);
		}

		public void repositionLightSource(int identifier, Vector2 position)
		{
			sharedLights.TryGetValue(identifier, out var value);
			if (value != null)
			{
				value.position.Value = position;
			}
		}

		public virtual bool isTileOccupiedForPlacement(Vector2 tileLocation, Object toPlace = null)
		{
			foreach (ResourceClump resourceClump in resourceClumps)
			{
				if (resourceClump.occupiesTile((int)tileLocation.X, (int)tileLocation.Y))
				{
					return true;
				}
			}
			objects.TryGetValue(tileLocation, out var value);
			Microsoft.Xna.Framework.Rectangle value2 = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
			for (int i = 0; i < characters.Count; i++)
			{
				if (characters[i] != null && characters[i].GetBoundingBox().Intersects(value2))
				{
					return true;
				}
			}
			if (isTileOccupiedByFarmer(tileLocation) != null && (toPlace == null || !toPlace.isPassable()) && (toPlace == null || toPlace.Category == -24 || toPlace.Category == 0))
			{
				return true;
			}
			if (largeTerrainFeatures != null)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					if (largeTerrainFeature.getBoundingBox().Intersects(value2))
					{
						return true;
					}
				}
			}
			if (toPlace != null && toPlace.Category == -19)
			{
				if (toPlace.Category == -19 && terrainFeatures.ContainsKey(tileLocation) && terrainFeatures[tileLocation] is HoeDirt)
				{
					HoeDirt hoeDirt = terrainFeatures[tileLocation] as HoeDirt;
					if ((int)(terrainFeatures[tileLocation] as HoeDirt).fertilizer != 0)
					{
						return true;
					}
					if (((int)toPlace.parentSheetIndex == 368 || (int)toPlace.parentSheetIndex == 368) && hoeDirt.crop != null && (int)hoeDirt.crop.currentPhase != 0)
					{
						return true;
					}
				}
			}
			else if (terrainFeatures.ContainsKey(tileLocation) && value2.Intersects(terrainFeatures[tileLocation].getBoundingBox(tileLocation)) && (!terrainFeatures[tileLocation].isPassable() || (terrainFeatures[tileLocation] is HoeDirt && ((HoeDirt)terrainFeatures[tileLocation]).crop != null) || (toPlace != null && toPlace.isSapling())))
			{
				return true;
			}
			if ((toPlace == null || !(toPlace is BedFurniture) || isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y), Game1.viewport) || !isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y + 1), Game1.viewport)) && !isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y), Game1.viewport) && (toPlace == null || !(toPlace is Wallpaper)))
			{
				return true;
			}
			if (toPlace != null && (toPlace.Category == -74 || toPlace.Category == -19) && value != null && value is IndoorPot)
			{
				if ((int)toPlace.parentSheetIndex == 251)
				{
					if ((value as IndoorPot).bush.Value == null && (value as IndoorPot).hoeDirt.Value.crop == null)
					{
						return false;
					}
				}
				else if ((value as IndoorPot).hoeDirt.Value.canPlantThisSeedHere(toPlace.parentSheetIndex, (int)tileLocation.X, (int)tileLocation.Y, toPlace.Category == -19) && (value as IndoorPot).bush.Value == null)
				{
					return false;
				}
			}
			return value != null;
		}

		public Farmer isTileOccupiedByFarmer(Vector2 tileLocation)
		{
			foreach (Farmer farmer in farmers)
			{
				if (farmer.getTileLocation().Equals(tileLocation))
				{
					return farmer;
				}
			}
			return null;
		}

		public virtual bool isTileOccupied(Vector2 tileLocation, string characterToIgnore = "", bool ignoreAllCharacters = false)
		{
			foreach (ResourceClump resourceClump in resourceClumps)
			{
				if (resourceClump.occupiesTile((int)tileLocation.X, (int)tileLocation.Y))
				{
					return true;
				}
			}
			objects.TryGetValue(tileLocation, out var value);
			Microsoft.Xna.Framework.Rectangle value2 = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64 + 1, (int)tileLocation.Y * 64 + 1, 62, 62);
			if (!ignoreAllCharacters)
			{
				for (int i = 0; i < characters.Count; i++)
				{
					if (characters[i] != null && !characters[i].Name.Equals(characterToIgnore) && characters[i].GetBoundingBox().Intersects(value2))
					{
						return true;
					}
				}
			}
			if (terrainFeatures.ContainsKey(tileLocation) && value2.Intersects(terrainFeatures[tileLocation].getBoundingBox(tileLocation)) && (!NPC.isCheckingSpouseTileOccupancy || !terrainFeatures[tileLocation].isPassable()))
			{
				return true;
			}
			if (largeTerrainFeatures != null)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					if (largeTerrainFeature.getBoundingBox().Intersects(value2))
					{
						return true;
					}
				}
			}
			Furniture furnitureAt = GetFurnitureAt(tileLocation);
			if (furnitureAt != null && !furnitureAt.isPassable())
			{
				return true;
			}
			if (NPC.isCheckingSpouseTileOccupancy && value != null && value.isPassable())
			{
				return false;
			}
			return value != null;
		}

		public virtual bool isTileOccupiedIgnoreFloors(Vector2 tileLocation, string characterToIgnore = "")
		{
			objects.TryGetValue(tileLocation, out var value);
			Microsoft.Xna.Framework.Rectangle value2 = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64 + 1, (int)tileLocation.Y * 64 + 1, 62, 62);
			for (int i = 0; i < characters.Count; i++)
			{
				if (characters[i] != null && !characters[i].name.Equals(characterToIgnore) && characters[i].GetBoundingBox().Intersects(value2))
				{
					return true;
				}
			}
			if (terrainFeatures.ContainsKey(tileLocation) && value2.Intersects(terrainFeatures[tileLocation].getBoundingBox(tileLocation)) && !terrainFeatures[tileLocation].isPassable())
			{
				return true;
			}
			if (largeTerrainFeatures != null)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					if (largeTerrainFeature.getBoundingBox().Intersects(value2))
					{
						return true;
					}
				}
			}
			Furniture furnitureAt = GetFurnitureAt(tileLocation);
			if (furnitureAt != null && !furnitureAt.isPassable())
			{
				return true;
			}
			return value != null;
		}

		public virtual bool isTileOccupiedIgnoreFloorsAndHorse(Vector2 tileLocation)
		{
			if (objects.ContainsKey(tileLocation))
			{
				objects.TryGetValue(tileLocation, out var value);
				if (value != null)
				{
					return !value.isPassable();
				}
			}
			Microsoft.Xna.Framework.Rectangle value2 = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64 + 1, (int)tileLocation.Y * 64 + 1, 62, 62);
			for (int i = 0; i < characters.Count; i++)
			{
				if ((characters[i] == null || !(characters[i] is Pet) || !((Pet)characters[i]).isSleepingOnFarmerBed) && characters[i] != null && characters[i].GetBoundingBox().Intersects(value2))
				{
					if (characters[i] is Horse && Game1.player.isRidingHorse())
					{
						return false;
					}
					return true;
				}
			}
			if (terrainFeatures.ContainsKey(tileLocation) && value2.Intersects(terrainFeatures[tileLocation].getBoundingBox(tileLocation)) && !terrainFeatures[tileLocation].isPassable())
			{
				return true;
			}
			if (largeTerrainFeatures != null)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					if (largeTerrainFeature.getBoundingBox().Intersects(value2))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool isTileHoeDirt(Vector2 tileLocation)
		{
			if (terrainFeatures.ContainsKey(tileLocation) && terrainFeatures[tileLocation] is HoeDirt)
			{
				return true;
			}
			if (objects.ContainsKey(tileLocation) && objects[tileLocation] is IndoorPot)
			{
				return true;
			}
			return false;
		}

		public void playTerrainSound(Vector2 tileLocation, Character who = null, bool showTerrainDisturbAnimation = true)
		{
			string text = "thudStep";
			if (Game1.currentLocation.IsOutdoors || Game1.currentLocation.Name.ToLower().Contains("mine"))
			{
				switch (Game1.currentLocation.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Type", "Back"))
				{
				case "Dirt":
					text = "sandyStep";
					break;
				case "Stone":
					text = "stoneStep";
					break;
				case "Grass":
					text = (GetSeasonForLocation().Equals("winter") ? "snowyStep" : "grassyStep");
					break;
				case "Wood":
					text = "woodyStep";
					break;
				case null:
				{
					string text2 = Game1.currentLocation.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Water", "Back");
					if (text2 != null)
					{
						text = "waterSlosh";
					}
					break;
				}
				}
			}
			else
			{
				text = "thudStep";
			}
			if (Game1.currentLocation.terrainFeatures.ContainsKey(tileLocation) && Game1.currentLocation.terrainFeatures[tileLocation] is Flooring)
			{
				text = ((Flooring)Game1.currentLocation.terrainFeatures[tileLocation]).getFootstepSound();
			}
			if (who != null && showTerrainDisturbAnimation && text.Equals("sandyStep"))
			{
				Vector2 vector = Vector2.Zero;
				if (who.shouldShadowBeOffset)
				{
					vector = who.drawOffset.Value;
				}
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 64, 64, 64), 50f, 4, 1, new Vector2(who.Position.X + (float)Game1.random.Next(-8, 8), who.Position.Y + (float)Game1.random.Next(-16, 0)) + vector, flicker: false, Game1.random.NextDouble() < 0.5, 0.0001f, 0f, Color.White, 1f, 0.01f, 0f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 128f));
			}
			else if (who != null && showTerrainDisturbAnimation && GetSeasonForLocation().Equals("winter") && text.Equals("grassyStep"))
			{
				Vector2 vector2 = Vector2.Zero;
				if (who.shouldShadowBeOffset)
				{
					vector2 = who.drawOffset.Value;
				}
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(who.Position.X, who.Position.Y) + vector2, flicker: false, flipped: false, 0.0001f, 0.001f, Color.White, 1f, 0.01f, 0f, 0f));
			}
			if (text.Length > 0)
			{
				localSound(text);
			}
		}

		public bool checkTileIndexAction(int tileIndex)
		{
			if ((tileIndex == 1799 || (uint)(tileIndex - 1824) <= 9u) && Name.Equals("AbandonedJojaMart"))
			{
				(Game1.getLocationFromName("AbandonedJojaMart") as AbandonedJojaMart).checkBundle();
				return true;
			}
			return false;
		}

		public virtual bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (who.IsSitting())
			{
				who.StopSitting();
				return true;
			}
			Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
			Microsoft.Xna.Framework.Rectangle value2 = new Microsoft.Xna.Framework.Rectangle((tileLocation.X - 1) * 64, (tileLocation.Y - 1) * 64, 192, 192);
			foreach (Farmer farmer in farmers)
			{
				if (farmer != Game1.player && farmer.GetBoundingBox().Intersects(value) && farmer.checkAction(who, this))
				{
					return true;
				}
			}
			if (currentEvent != null && currentEvent.isFestival)
			{
				return currentEvent.checkAction(tileLocation, viewport, who);
			}
			foreach (NPC character in characters)
			{
				if (character != null && ((!character.IsMonster && (!who.isRidingHorse() || !(character is Horse)) && character.GetBoundingBox().Intersects(value)) || (!who.isRidingHorse() && character is Horse && character.GetBoundingBox().Intersects(value2))) && character.checkAction(who, this))
				{
					if (who.FarmerSprite.IsPlayingBasicAnimation(who.FacingDirection, carrying: false) || who.FarmerSprite.IsPlayingBasicAnimation(who.FacingDirection, carrying: true))
					{
						who.faceGeneralDirection(character.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
					}
					return true;
				}
			}
			if (who.IsLocalPlayer && who.currentUpgrade != null && name.Equals("Farm") && tileLocation.Equals(new Location((int)(who.currentUpgrade.positionOfCarpenter.X + 32f) / 64, (int)(who.currentUpgrade.positionOfCarpenter.Y + 32f) / 64)))
			{
				if (who.currentUpgrade.daysLeftTillUpgradeDone == 1)
				{
					Game1.drawDialogue(Game1.getCharacterFromName("Robin"), Game1.content.LoadString("Data\\ExtraDialogue:Farm_RobinWorking_ReadyTomorrow"));
				}
				else
				{
					Game1.drawDialogue(Game1.getCharacterFromName("Robin"), Game1.content.LoadString("Data\\ExtraDialogue:Farm_RobinWorking" + (Game1.random.Next(2) + 1)));
				}
			}
			foreach (ResourceClump resourceClump in resourceClumps)
			{
				if (resourceClump.getBoundingBox(resourceClump.tile).Intersects(value) && resourceClump.performUseAction(new Vector2(tileLocation.X, tileLocation.Y), this))
				{
					return true;
				}
			}
			Vector2 vector = new Vector2(tileLocation.X, tileLocation.Y);
			if (objects.ContainsKey(vector) && objects[vector].Type != null)
			{
				if (who.isRidingHorse() && !(objects[vector] is Fence) && !objects[vector].isSpawnedObject)
				{
					return false;
				}
				if (vector.Equals(who.getTileLocation()) && !objects[vector].isPassable())
				{
					Tool tool = new Pickaxe();
					tool.DoFunction(Game1.currentLocation, -1, -1, 0, who);
					if (objects[vector].performToolAction(tool, this))
					{
						objects[vector].performRemoveAction(objects[vector].tileLocation, Game1.currentLocation);
						objects[vector].dropItem(this, who.GetToolLocation(), new Vector2(who.GetBoundingBox().Center.X, who.GetBoundingBox().Center.Y));
						Game1.currentLocation.Objects.Remove(vector);
						return true;
					}
					tool = new Axe();
					tool.DoFunction(Game1.currentLocation, -1, -1, 0, who);
					if (objects.ContainsKey(vector) && objects[vector].performToolAction(tool, this))
					{
						objects[vector].performRemoveAction(objects[vector].tileLocation, Game1.currentLocation);
						objects[vector].dropItem(this, who.GetToolLocation(), new Vector2(who.GetBoundingBox().Center.X, who.GetBoundingBox().Center.Y));
						Game1.currentLocation.Objects.Remove(vector);
						return true;
					}
					if (!objects.ContainsKey(vector))
					{
						return true;
					}
				}
				if (objects.ContainsKey(vector) && (objects[vector].Type.Equals("Crafting") || objects[vector].Type.Equals("interactive")))
				{
					if (who.ActiveObject == null && objects[vector].checkForAction(who))
					{
						return true;
					}
					if (objects.ContainsKey(vector))
					{
						if (who.CurrentItem != null)
						{
							Object value3 = objects[vector].heldObject.Value;
							objects[vector].heldObject.Value = null;
							bool flag = objects[vector].performObjectDropInAction(who.CurrentItem, probe: true, who);
							objects[vector].heldObject.Value = value3;
							bool flag2 = objects[vector].performObjectDropInAction(who.CurrentItem, probe: false, who);
							if ((flag || flag2) && who.isMoving())
							{
								Game1.haltAfterCheck = false;
							}
							if (flag2)
							{
								who.reduceActiveItemByOne();
								return true;
							}
							return objects[vector].checkForAction(who) || flag;
						}
						return objects[vector].checkForAction(who);
					}
				}
				else if (objects.ContainsKey(vector) && (bool)objects[vector].isSpawnedObject)
				{
					int quality = objects[vector].quality;
					Random random = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + (int)vector.X + (int)vector.Y * 777);
					if (who.professions.Contains(16) && objects[vector].isForage(this))
					{
						objects[vector].Quality = 4;
					}
					else if (objects[vector].isForage(this))
					{
						if (random.NextDouble() < (double)((float)who.ForagingLevel / 30f))
						{
							objects[vector].Quality = 2;
						}
						else if (random.NextDouble() < (double)((float)who.ForagingLevel / 15f))
						{
							objects[vector].Quality = 1;
						}
					}
					if ((bool)objects[vector].questItem && objects[vector].questId.Value != 0 && !who.hasQuest(objects[vector].questId))
					{
						return false;
					}
					if (who.couldInventoryAcceptThisItem(objects[vector]))
					{
						if (who.IsLocalPlayer)
						{
							localSound("pickUpItem");
							DelayedAction.playSoundAfterDelay("coin", 300);
						}
						who.animateOnce(279 + who.FacingDirection);
						if (!isFarmBuildingInterior())
						{
							if (objects[vector].isForage(this))
							{
								who.gainExperience(2, 7);
							}
						}
						else
						{
							who.gainExperience(0, 5);
						}
						who.addItemToInventoryBool(objects[vector].getOne());
						Game1.stats.ItemsForaged++;
						if (who.professions.Contains(13) && random.NextDouble() < 0.2 && !objects[vector].questItem && who.couldInventoryAcceptThisItem(objects[vector]) && !isFarmBuildingInterior())
						{
							who.addItemToInventoryBool(objects[vector].getOne());
							who.gainExperience(2, 7);
						}
						objects.Remove(vector);
						return true;
					}
					objects[vector].Quality = quality;
				}
			}
			if (who.isRidingHorse())
			{
				who.mount.checkAction(who, this);
				return true;
			}
			foreach (KeyValuePair<Vector2, TerrainFeature> pair in terrainFeatures.Pairs)
			{
				if (pair.Value.getBoundingBox(pair.Key).Intersects(value) && pair.Value.performUseAction(pair.Key, this))
				{
					Game1.haltAfterCheck = false;
					return true;
				}
			}
			if (largeTerrainFeatures != null)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					if (largeTerrainFeature.getBoundingBox().Intersects(value) && largeTerrainFeature.performUseAction(largeTerrainFeature.tilePosition, this))
					{
						Game1.haltAfterCheck = false;
						return true;
					}
				}
			}
			string text = null;
			Tile tile = map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
			if (tile != null)
			{
				tile.Properties.TryGetValue("Action", out var value4);
				if (value4 != null)
				{
					text = value4.ToString();
				}
			}
			if (text == null)
			{
				text = doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings");
			}
			NPC nPC = isCharacterAtTile(vector + new Vector2(0f, 1f));
			if (text != null)
			{
				if (currentEvent == null && nPC != null && !nPC.IsInvisible && !nPC.IsMonster && (!who.isRidingHorse() || !(nPC is Horse)) && Utility.withinRadiusOfPlayer(nPC.getStandingX(), nPC.getStandingY(), 1, who) && nPC.checkAction(who, this))
				{
					if (who.FarmerSprite.IsPlayingBasicAnimation(who.FacingDirection, who.IsCarrying()))
					{
						who.faceGeneralDirection(nPC.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
					}
					return true;
				}
				return performAction(text, who, tileLocation);
			}
			if (tile != null && checkTileIndexAction(tile.TileIndex))
			{
				return true;
			}
			foreach (MapSeat mapSeat in mapSeats)
			{
				if (mapSeat.OccupiesTile(tileLocation.X, tileLocation.Y) && !mapSeat.IsBlocked(this))
				{
					who.BeginSitting(mapSeat);
					return true;
				}
			}
			Point value5 = new Point(tileLocation.X * 64, (tileLocation.Y - 1) * 64);
			bool flag3 = Game1.didPlayerJustRightClick();
			foreach (Furniture item in furniture)
			{
				if (item.boundingBox.Value.Contains((int)(vector.X * 64f), (int)(vector.Y * 64f)) && (int)item.furniture_type != 12)
				{
					if (flag3)
					{
						if (who.ActiveObject != null && item.performObjectDropInAction(who.ActiveObject, probe: false, who))
						{
							return true;
						}
						return item.checkForAction(who);
					}
					return item.clicked(who);
				}
				if ((int)item.furniture_type != 6 || !item.boundingBox.Value.Contains(value5))
				{
					continue;
				}
				if (flag3)
				{
					if (who.ActiveObject != null && item.performObjectDropInAction(who.ActiveObject, probe: false, who))
					{
						return true;
					}
					return item.checkForAction(who);
				}
				return item.clicked(who);
			}
			return false;
		}

		public virtual bool CanFreePlaceFurniture()
		{
			return false;
		}

		public virtual bool LowPriorityLeftClick(int x, int y, Farmer who)
		{
			if (Game1.activeClickableMenu != null)
			{
				return false;
			}
			for (int num = this.furniture.Count - 1; num >= 0; num--)
			{
				Furniture furniture = this.furniture[num];
				if (CanFreePlaceFurniture() || furniture.IsCloseEnoughToFarmer(who))
				{
					if (!furniture.isPassable() && furniture.boundingBox.Value.Contains(x, y) && furniture.canBeRemoved(who))
					{
						furniture.AttemptRemoval(delegate(Furniture f)
						{
							Guid job3 = this.furniture.GuidOf(f);
							if (!furnitureToRemove.Contains(job3))
							{
								furnitureToRemove.Add(job3);
							}
						});
						return true;
					}
					if (furniture.boundingBox.Value.Contains(x, y) && furniture.heldObject.Value != null)
					{
						furniture.clicked(who);
						return true;
					}
					if (!furniture.isGroundFurniture() && furniture.canBeRemoved(who))
					{
						int num2 = y;
						if (this is DecoratableLocation)
						{
							num2 = (this as DecoratableLocation).GetWallTopY(x / 64, y / 64);
							if (num2 == -1)
							{
								num2 = y * 64;
							}
						}
						if (furniture.boundingBox.Value.Contains(x, num2))
						{
							furniture.AttemptRemoval(delegate(Furniture f)
							{
								Guid job2 = this.furniture.GuidOf(f);
								if (!furnitureToRemove.Contains(job2))
								{
									furnitureToRemove.Add(job2);
								}
							});
							return true;
						}
					}
				}
			}
			for (int num3 = this.furniture.Count - 1; num3 >= 0; num3--)
			{
				Furniture furniture2 = this.furniture[num3];
				if ((CanFreePlaceFurniture() || furniture2.IsCloseEnoughToFarmer(who)) && furniture2.isPassable() && furniture2.boundingBox.Value.Contains(x, y) && furniture2.canBeRemoved(who))
				{
					furniture2.AttemptRemoval(delegate(Furniture f)
					{
						Guid job = this.furniture.GuidOf(f);
						if (!furnitureToRemove.Contains(job))
						{
							furnitureToRemove.Add(job);
						}
					});
					return true;
				}
			}
			return false;
		}

		public virtual bool CanPlaceThisFurnitureHere(Furniture furniture)
		{
			if (furniture == null)
			{
				return false;
			}
			if (furniture.furniture_type.Value == 15 && !(this is FarmHouse) && !(this is IslandFarmHouse))
			{
				return false;
			}
			int placementRestriction = furniture.placementRestriction;
			if ((placementRestriction == 0 || placementRestriction == 2) && (this is DecoratableLocation || !IsOutdoors))
			{
				return true;
			}
			if ((placementRestriction == 1 || placementRestriction == 2) && !(this is DecoratableLocation) && IsOutdoors)
			{
				return true;
			}
			return false;
		}

		[Obsolete("These values returned by this function are no longer used by the game (except for rare, backwards compatibility related cases.) Check DecoratableLocation's wallpaper/flooring related functionality instead.")]
		public virtual List<Microsoft.Xna.Framework.Rectangle> getWalls()
		{
			return new List<Microsoft.Xna.Framework.Rectangle>();
		}

		protected virtual void removeQueuedFurniture(Guid guid)
		{
			Farmer player = Game1.player;
			if (!this.furniture.ContainsGuid(guid))
			{
				return;
			}
			Furniture furniture = this.furniture[guid];
			if (!player.couldInventoryAcceptThisItem(furniture))
			{
				return;
			}
			furniture.performRemoveAction(furniture.tileLocation, this);
			this.furniture.Remove(guid);
			bool flag = false;
			for (int i = 0; i < 12; i++)
			{
				if (player.items[i] == null)
				{
					player.items[i] = furniture;
					player.CurrentToolIndex = i;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Item item = player.addItemToInventory(furniture, 11);
				player.addItemToInventory(item);
				player.CurrentToolIndex = 11;
			}
			localSound("coin");
		}

		public virtual bool leftClick(int x, int y, Farmer who)
		{
			Vector2 key = new Vector2(x / 64, y / 64);
			if (objects.ContainsKey(key) && objects[key].clicked(who))
			{
				objects.Remove(key);
				return true;
			}
			return false;
		}

		public virtual int getExtraMillisecondsPerInGameMinuteForThisLocation()
		{
			return 0;
		}

		public bool isTileLocationTotallyClearAndPlaceable(int x, int y)
		{
			return isTileLocationTotallyClearAndPlaceable(new Vector2(x, y));
		}

		public virtual bool isTileLocationTotallyClearAndPlaceableIgnoreFloors(Vector2 v)
		{
			if (isTileOnMap(v) && !isTileOccupiedIgnoreFloors(v) && isTilePassable(new Location((int)v.X, (int)v.Y), Game1.viewport))
			{
				return isTilePlaceable(v);
			}
			return false;
		}

		public void ActivateKitchen(NetRef<Chest> fridge)
		{
			List<NetMutex> list = new List<NetMutex>();
			List<Chest> mini_fridges = new List<Chest>();
			foreach (Object value in objects.Values)
			{
				if (value != null && (bool)value.bigCraftable && value is Chest && value.ParentSheetIndex == 216)
				{
					mini_fridges.Add(value as Chest);
					list.Add((value as Chest).mutex);
				}
			}
			if (fridge != null && fridge.Value.mutex.IsLocked())
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Kitchen_InUse"));
				return;
			}
			MultipleMutexRequest multiple_mutex_request = null;
			multiple_mutex_request = new MultipleMutexRequest(list, delegate
			{
				fridge.Value.mutex.RequestLock(delegate
				{
					List<Chest> list2 = new List<Chest>();
					if (fridge != null)
					{
						list2.Add(fridge);
					}
					list2.AddRange(mini_fridges);
					Game1.activeClickableMenu = new CraftingPageMobile(0, 0, 0, 0, cooking: true, 300, list2);
					Game1.activeClickableMenu.exitFunction = delegate
					{
						fridge.Value.mutex.ReleaseLock();
						multiple_mutex_request.ReleaseLocks();
					};
				}, delegate
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Kitchen_InUse"));
					multiple_mutex_request.ReleaseLocks();
				});
			}, delegate
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Kitchen_InUse"));
			});
		}

		public virtual bool isTileLocationTotallyClearAndPlaceable(Vector2 v)
		{
			Vector2 vector = v * 64f;
			vector.X += 32f;
			vector.Y += 32f;
			foreach (Furniture item in furniture)
			{
				if ((int)item.furniture_type != 12 && !item.isPassable() && item.getBoundingBox(item.tileLocation).Contains((int)vector.X, (int)vector.Y) && !item.AllowPlacementOnThisTile((int)v.X, (int)v.Y))
				{
					return false;
				}
			}
			if (isTileOnMap(v) && !isTileOccupied(v) && isTilePassable(new Location((int)v.X, (int)v.Y), Game1.viewport))
			{
				return isTilePlaceable(v);
			}
			return false;
		}

		public virtual bool isTilePlaceable(Vector2 v, Item item = null)
		{
			if (doesTileHaveProperty((int)v.X, (int)v.Y, "NoFurniture", "Back") == null || (item != null && item is Object && (item as Object).isPassable() && Game1.currentLocation.IsOutdoors && !doesTileHavePropertyNoNull((int)v.X, (int)v.Y, "NoFurniture", "Back").Equals("total")))
			{
				if (doesTileHaveProperty((int)v.X, (int)v.Y, "Water", "Back") != null)
				{
					return item?.canBePlacedInWater() ?? false;
				}
				return true;
			}
			return false;
		}

		public virtual bool shouldShadowBeDrawnAboveBuildingsLayer(Vector2 p)
		{
			if (doesTileHaveProperty((int)p.X, (int)p.Y, "Passable", "Buildings") != null)
			{
				return true;
			}
			if (terrainFeatures.TryGetValue(p, out var value) && value is HoeDirt)
			{
				return true;
			}
			if (doesTileHaveProperty((int)p.X, (int)p.Y, "Water", "Back") != null && (!(getTileSheetIDAt((int)p.X, (int)p.Y, "Buildings") == "Town") || getTileIndexAt((int)p.X, (int)p.Y, "Buildings") < 1004 || getTileIndexAt((int)p.X, (int)p.Y, "Buildings") > 1013))
			{
				return true;
			}
			return false;
		}

		public void openDoor(Location tileLocation, bool playSound)
		{
			try
			{
				int tileIndexAt = getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings");
				Point key = new Point(tileLocation.X, tileLocation.Y);
				if (!interiorDoors.ContainsKey(key))
				{
					return;
				}
				interiorDoors[key] = true;
				if (playSound)
				{
					Vector2 position = new Vector2(tileLocation.X, tileLocation.Y);
					if (tileIndexAt == 120)
					{
						playSoundAt("doorOpen", position);
					}
					else
					{
						playSoundAt("doorCreak", position);
					}
				}
			}
			catch (Exception)
			{
			}
		}

		public void doStarpoint(string which)
		{
			if (!(which == "3"))
			{
				if (which == "4" && Game1.player.ActiveObject != null && Game1.player.ActiveObject != null && !Game1.player.ActiveObject.bigCraftable && (int)Game1.player.ActiveObject.parentSheetIndex == 203)
				{
					Object @object = new Object(Vector2.Zero, 162);
					if (!Game1.player.couldInventoryAcceptThisItem(@object) && (int)Game1.player.ActiveObject.stack > 1)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
						return;
					}
					Game1.player.reduceActiveItemByOne();
					Game1.player.makeThisTheActiveObject(@object);
					localSound("croak");
					Game1.flashAlpha = 1f;
				}
			}
			else if (Game1.player.ActiveObject != null && Game1.player.ActiveObject != null && !Game1.player.ActiveObject.bigCraftable && (int)Game1.player.ActiveObject.parentSheetIndex == 307)
			{
				Object object2 = new Object(Vector2.Zero, 161);
				if (!Game1.player.couldInventoryAcceptThisItem(object2) && (int)Game1.player.ActiveObject.stack > 1)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
					return;
				}
				Game1.player.reduceActiveItemByOne();
				Game1.player.makeThisTheActiveObject(object2);
				localSound("discoverMineral");
				Game1.flashAlpha = 1f;
			}
		}

		public virtual string FormatCompletionLine(Func<Farmer, float> check)
		{
			KeyValuePair<Farmer, float> farmCompletion = Utility.GetFarmCompletion(check);
			if (farmCompletion.Key == Game1.player)
			{
				return farmCompletion.Value.ToString();
			}
			return "(" + farmCompletion.Key.Name + ") " + farmCompletion.Value;
		}

		public virtual string FormatCompletionLine(Func<Farmer, bool> check, string true_value, string false_value)
		{
			KeyValuePair<Farmer, bool> farmCompletion = Utility.GetFarmCompletion(check);
			if (farmCompletion.Key == Game1.player)
			{
				if (!farmCompletion.Value)
				{
					return false_value;
				}
				return true_value;
			}
			return "(" + farmCompletion.Key.Name + ") " + (farmCompletion.Value ? true_value : false_value);
		}

		public virtual void ShowQiCat()
		{
			if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && !Game1.MasterPlayer.mailReceived.Contains("GotPerfectionStatue"))
			{
				Game1.MasterPlayer.mailReceived.Add("GotPerfectionStatue");
				Game1.player.addItemByMenuIfNecessaryElseHoldUp(new Object(Vector2.Zero, 280));
				return;
			}
			Game1.playSound("qi_shop");
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = Game1.viewport.Height < 850;
			string[] array = new string[2];
			stringBuilder.AppendLine(Utility.loadStringShort("UI", "PT_Title") + "^");
			stringBuilder.AppendLine("----------------^");
			stringBuilder.AppendLine(Utility.loadStringShort("UI", "PT_Shipped") + ": " + FormatCompletionLine((Farmer farmer) => (float)Math.Floor(Utility.getFarmerItemsShippedPercent(farmer) * 100f)) + "%^");
			stringBuilder.AppendLine(Utility.loadStringShort("UI", "PT_Obelisks") + ": " + Math.Min(Utility.numObelisksOnFarm(), 4) + "/4^");
			stringBuilder.AppendLine(Utility.loadStringShort("UI", "PT_GoldClock") + ": " + (Game1.getFarm().isBuildingConstructed("Gold Clock") ? Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes") : Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")) + "^");
			stringBuilder.AppendLine(Utility.loadStringShort("UI", "PT_MonsterSlayer") + ": " + FormatCompletionLine((Farmer farmer) => farmer.hasCompletedAllMonsterSlayerQuests.Value, Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes"), Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")) + "^");
			stringBuilder.AppendLine(Utility.loadStringShort("UI", "PT_GreatFriends") + ": " + FormatCompletionLine((Farmer farmer) => (float)Math.Floor(Utility.getMaxedFriendshipPercent(farmer) * 100f)) + "%^");
			stringBuilder.AppendLine(Utility.loadStringShort("UI", "PT_FarmerLevel") + ": " + FormatCompletionLine((Farmer farmer) => Math.Min(farmer.Level, 25)) + "/25^");
			if (flag)
			{
				stringBuilder.AppendLine("...");
				array[0] = stringBuilder.ToString();
				stringBuilder.Clear();
			}
			stringBuilder.AppendLine(Utility.loadStringShort("UI", "PT_Stardrops") + ": " + FormatCompletionLine((Farmer farmer) => Utility.foundAllStardrops(farmer), Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes"), Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")) + "^");
			stringBuilder.AppendLine(Utility.loadStringShort("UI", "PT_Cooking") + ": " + FormatCompletionLine((Farmer farmer) => (float)Math.Floor(Utility.getCookedRecipesPercent(farmer) * 100f)) + "%^");
			stringBuilder.AppendLine(Utility.loadStringShort("UI", "PT_Crafting") + ": " + FormatCompletionLine((Farmer farmer) => (float)Math.Floor(Utility.getCraftedRecipesPercent(farmer) * 100f)) + "%^");
			stringBuilder.AppendLine(Utility.loadStringShort("UI", "PT_Fish") + ": " + FormatCompletionLine((Farmer farmer) => (float)Math.Floor(Utility.getFishCaughtPercent(farmer) * 100f)) + "%^");
			stringBuilder.AppendLine(Utility.loadStringShort("UI", "PT_GoldenWalnut") + ": " + Math.Min(Game1.netWorldState.Value.GoldenWalnutsFound, 130) + "/" + 130 + "^");
			stringBuilder.AppendLine("----------------^");
			stringBuilder.AppendLine(Utility.loadStringShort("UI", "PT_Total") + ": " + Math.Floor(Utility.percentGameComplete() * 100f) + "%^");
			if (flag)
			{
				array[1] = stringBuilder.ToString();
				Game1.multipleDialogues(array);
			}
			else
			{
				Game1.drawDialogueNoTyping(stringBuilder.ToString());
			}
		}

		public virtual bool performAction(string action, Farmer who, Location tileLocation)
		{
			if (action != null && who.IsLocalPlayer)
			{
				string[] array = action.Split(' ');
				string text = array[0];
				if (text != null)
				{
					Response response;
					Response response2;
					Response response3;
					NPC characterFromName6;
					bool dailyQuest;
					Response[] answerChoices3;
					TemporaryAnimatedSprite temporarySpriteByID2;
					switch (text.Length)
					{
					case 13:
						switch (text[7])
						{
						case 'o':
							break;
						case 'O':
							goto IL_051a;
						case 'm':
							goto IL_0530;
						case 'r':
							goto IL_0546;
						case 'S':
							goto IL_055c;
						case 'e':
							goto IL_0572;
						case 'l':
							goto IL_0588;
						default:
							goto end_IL_0034;
						}
						if (!(text == "SummitBoulder"))
						{
							break;
						}
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:SummitBoulder"));
						goto IL_3253;
					case 5:
					{
						char c = text[0];
						if ((uint)c <= 72u)
						{
							if (c != 'C')
							{
								if (c != 'H' || !(text == "HMTGF"))
								{
									break;
								}
								if (who.ActiveObject != null && who.ActiveObject != null && !who.ActiveObject.bigCraftable && (int)who.ActiveObject.parentSheetIndex == 155)
								{
									Object @object = new Object(Vector2.Zero, 155);
									if (!Game1.player.couldInventoryAcceptThisItem(@object) && (int)Game1.player.ActiveObject.stack > 1)
									{
										Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
									}
									else
									{
										Game1.player.reduceActiveItemByOne();
										Game1.player.makeThisTheActiveObject(@object);
										localSound("discoverMineral");
										Game1.flashAlpha = 1f;
									}
								}
							}
							else
							{
								if (!(text == "Craft"))
								{
									break;
								}
								openCraftingMenu(actionParamsToString(array));
							}
						}
						else if (c != 'N')
						{
							if (c != 'Q' || !(text == "QiCat"))
							{
								break;
							}
							ShowQiCat();
						}
						else
						{
							if (!(text == "Notes"))
							{
								break;
							}
							readNote(Convert.ToInt32(array[1]));
						}
						goto IL_3253;
					}
					case 9:
						switch (text[3])
						{
						case 'e':
							if (!(text == "QiGemShop"))
							{
								goto end_IL_0034;
							}
							Game1.playSound("qi_shop");
							Game1.activeClickableMenu = new ShopMenu(Utility.GetQiChallengeRewardStock(Game1.player), 4, null, null, null, "QiGemShop")
							{
								purchaseSound = "qi_shop_purchase"
							};
							return true;
						case 'S':
							break;
						case 'l':
							goto IL_0622;
						case 'P':
							goto IL_065a;
						case 'r':
							goto IL_0670;
						case 'y':
							goto IL_0686;
						case 'L':
							goto IL_069c;
						case 'p':
							if (!(text == "Carpenter"))
							{
								goto end_IL_0034;
							}
							return carpenters(tileLocation);
						case 'm':
							goto IL_06c8;
						case 'u':
							goto IL_06de;
						case 'b':
							goto IL_06f4;
						case 'c':
							goto IL_071b;
						default:
							goto end_IL_0034;
						}
						if (!(text == "DogStatue"))
						{
							break;
						}
						if (canRespec(0) || canRespec(3) || canRespec(2) || canRespec(4) || canRespec(1))
						{
							createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatue"), createYesNoResponses(), "dogStatue");
						}
						else
						{
							string text2 = Game1.content.LoadString("Strings\\Locations:Sewer_DogStatue");
							text2 = text2.Substring(0, text2.LastIndexOf('^'));
							Game1.drawObjectDialogue(text2);
						}
						goto IL_3253;
					case 16:
					{
						char c = text[0];
						if ((uint)c <= 81u)
						{
							if (c != 'E')
							{
								if (c != 'Q' || !(text == "QiChallengeBoard"))
								{
									break;
								}
								Game1.player.team.qiChallengeBoardMutex.RequestLock(delegate
								{
									Game1.activeClickableMenu = new SpecialOrdersBoard("Qi")
									{
										behaviorBeforeCleanup = delegate
										{
											Game1.player.team.qiChallengeBoardMutex.ReleaseLock();
										}
									};
								});
							}
							else
							{
								if (!(text == "EvilShrineCenter"))
								{
									break;
								}
								if (who.isDivorced())
								{
									createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineCenter"), createYesNoResponses(), "evilShrineCenter");
								}
								else
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineCenterInactive"));
								}
							}
						}
						else if (c != 'T')
						{
							if (c != 'W' || !(text == "WarpWomensLocker"))
							{
								break;
							}
							if (who.IsMale)
							{
								if (who.IsLocalPlayer)
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WomensLocker_WrongGender"));
								}
								return true;
							}
							who.faceGeneralDirection(new Vector2(tileLocation.X, tileLocation.Y) * 64f);
							if (array.Length < 5)
							{
								playSoundAt("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
							}
							Game1.warpFarmer(array[3], Convert.ToInt32(array[1]), Convert.ToInt32(array[2]), flip: false);
						}
						else
						{
							if (!(text == "Theater_Entrance"))
							{
								break;
							}
							if (Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
							{
								if (Game1.player.team.movieMutex.IsLocked())
								{
									Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieTheater_CurrentlyShowing")));
								}
								else if (Game1.isFestival())
								{
									Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieTheater_ClosedFestival")));
								}
								else if (Game1.timeOfDay > 2100 || Game1.timeOfDay < 900)
								{
									string sub = Game1.getTimeOfDayString(900).Replace(" ", "");
									string sub2 = Game1.getTimeOfDayString(2100).Replace(" ", "");
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor_OpenRange", sub, sub2));
								}
								else if ((int)Game1.player.lastSeenMovieWeek >= Game1.Date.TotalWeeks)
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_AlreadySeen"));
								}
								else
								{
									NPC nPC3 = null;
									foreach (MovieInvitation movieInvitation in Game1.player.team.movieInvitations)
									{
										if (movieInvitation.farmer == Game1.player && !movieInvitation.fulfilled && MovieTheater.GetFirstInvitedPlayer(movieInvitation.invitedNPC) == Game1.player)
										{
											nPC3 = movieInvitation.invitedNPC;
											break;
										}
									}
									if (nPC3 != null && Game1.player.hasItemInInventory(809, 1))
									{
										Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_WatchWithFriendPrompt", nPC3.displayName), Game1.currentLocation.createYesNoResponses(), "EnterTheaterSpendTicket");
									}
									else if (Game1.player.hasItemInInventory(809, 1))
									{
										Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_WatchAlonePrompt"), Game1.currentLocation.createYesNoResponses(), "EnterTheaterSpendTicket");
									}
									else
									{
										Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieTheater_NoTicket")));
									}
								}
							}
						}
						goto IL_3253;
					}
					case 12:
						switch (text[3])
						{
						case 's':
							break;
						case 'a':
							goto IL_079f;
						case 'p':
							goto IL_07b5;
						case 'i':
							goto IL_07cb;
						case 'e':
							goto IL_07e1;
						case 'd':
							goto IL_07f7;
						case 'b':
							goto IL_080d;
						default:
							goto end_IL_0034;
						}
						if (!(text == "MonsterGrave"))
						{
							break;
						}
						if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en && who.eventsSeen.Contains(6963327))
						{
							Game1.multipleDialogues(new string[2] { "Abigail took a life to save mine...", "I'll never forget that." });
						}
						goto IL_3253;
					case 17:
					{
						char c = text[0];
						if (c != 'M')
						{
							if (c != 'T')
							{
								if (c != 'W' || !(text == "Warp_Sunroom_Door"))
								{
									break;
								}
								if (who.getFriendshipHeartLevelForNPC("Caroline") >= 2)
								{
									playSoundAt("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
									Game1.warpFarmer("Sunroom", 5, 13, flip: false);
								}
								else
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Caroline_Sunroom_Door"));
								}
							}
							else
							{
								if (!(text == "Theater_BoxOffice"))
								{
									break;
								}
								if (Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
								{
									if (Game1.isFestival())
									{
										Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieTheater_ClosedFestival")));
									}
									else if (Game1.timeOfDay > 2100)
									{
										Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_BoxOfficeClosed"));
									}
									else
									{
										MovieData movieForDate2 = MovieTheater.GetMovieForDate(Game1.Date);
										if (movieForDate2 != null)
										{
											Dictionary<ISalable, int[]> dictionary2 = new Dictionary<ISalable, int[]>();
											Object object2 = new Object(809, 1);
											dictionary2.Add(object2, new int[2]
											{
												object2.salePrice(),
												2147483647
											});
											Game1.activeClickableMenu = new ShopMenu(dictionary2, 0, "boxOffice");
										}
									}
								}
							}
						}
						else
						{
							if (!(text == "MinecartTransport"))
							{
								break;
							}
							if (Game1.MasterPlayer.mailReceived.Contains("ccBoilerRoom"))
							{
								Game1.currentLocation.tapToMove.preventMountingHorse = true;
								createQuestionDialogue(answerChoices: (!Game1.MasterPlayer.mailReceived.Contains("ccCraftsRoom")) ? new Response[3]
								{
									new Response("Town", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Town")),
									new Response("Bus", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_BusStop")),
									new Response("Cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel"))
								} : new Response[4]
								{
									new Response("Town", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Town")),
									new Response("Bus", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_BusStop")),
									new Response("Quarry", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Quarry")),
									new Response("Cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel"))
								}, question: Game1.content.LoadString("Strings\\Locations:MineCart_ChooseDestination"), dialogKey: "Minecart");
							}
							else
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:MineCart_OutOfOrder"));
							}
						}
						goto IL_3253;
					}
					case 10:
						switch (text[0])
						{
						case 'W':
							break;
						case 'B':
							goto IL_087b;
						case 'L':
							goto IL_08a2;
						case 'S':
							goto IL_08b8;
						case 'T':
							goto IL_08df;
						case 'N':
							goto IL_08f5;
						case 'A':
							if (!(text == "AnimalShop"))
							{
								goto end_IL_0034;
							}
							return animalShop(tileLocation);
						case 'F':
							goto IL_0921;
						case 'C':
							goto IL_0937;
						case 'E':
							goto IL_094d;
						case 'D':
							goto IL_0963;
						default:
							goto end_IL_0034;
						}
						if (!(text == "WizardBook"))
						{
							break;
						}
						if (who.mailReceived.Contains("hasPickedUpMagicInk") || who.hasMagicInk)
						{
							TutorialManager.Instance.SeenShop(TutorialShopLocation.ScienceHouse);
							Game1.activeClickableMenu = new CarpenterMenu(magicalConstruction: true);
						}
						goto IL_3253;
					case 14:
					{
						char c = text[4];
						if ((uint)c <= 83u)
						{
							if (c != 'G')
							{
								if (c != 'M')
								{
									if (c != 'S' || !(text == "EvilShrineLeft"))
									{
										break;
									}
									if (who.getChildrenCount() == 0)
									{
										Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineLeftInactive"));
									}
									else
									{
										createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineLeft"), createYesNoResponses(), "evilShrineLeft");
									}
								}
								else
								{
									if (!(text == "WarpMensLocker"))
									{
										break;
									}
									if (!who.IsMale)
									{
										if (who.IsLocalPlayer)
										{
											Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:MensLocker_WrongGender"));
										}
										return true;
									}
									who.faceGeneralDirection(new Vector2(tileLocation.X, tileLocation.Y) * 64f);
									if (array.Length < 5)
									{
										playSoundAt("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
									}
									Game1.warpFarmer(array[3], Convert.ToInt32(array[1]), Convert.ToInt32(array[2]), flip: false);
								}
							}
							else
							{
								if (!(text == "WarpGreenhouse"))
								{
									break;
								}
								if (Game1.MasterPlayer.mailReceived.Contains("ccPantry"))
								{
									who.faceGeneralDirection(new Vector2(tileLocation.X, tileLocation.Y) * 64f);
									playSoundAt("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
									GameLocation locationFromName = Game1.getLocationFromName("Greenhouse");
									int tileX = 10;
									int tileY = 23;
									if (locationFromName != null)
									{
										foreach (Warp warp in locationFromName.warps)
										{
											if (warp.TargetName == "Farm")
											{
												tileX = warp.X;
												tileY = warp.Y - 1;
												break;
											}
										}
									}
									Game1.warpFarmer("Greenhouse", tileX, tileY, flip: false);
								}
								else
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Farm_GreenhouseRuins"));
								}
							}
						}
						else if (c != 'd')
						{
							if (c != 'e')
							{
								if (c != 't' || !(text == "Theater_Poster"))
								{
									break;
								}
								if (Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
								{
									MovieData movieForDate3 = MovieTheater.GetMovieForDate(Game1.Date);
									if (movieForDate3 != null)
									{
										Game1.multipleDialogues(new string[2]
										{
											Game1.content.LoadString("Strings\\Locations:Theater_Poster_0", movieForDate3.Title),
											Game1.content.LoadString("Strings\\Locations:Theater_Poster_1", movieForDate3.Description)
										});
									}
								}
							}
							else
							{
								if (!(text == "LockedDoorWarp"))
								{
									break;
								}
								who.faceGeneralDirection(new Vector2(tileLocation.X, tileLocation.Y) * 64f);
								lockedDoorWarp(array);
							}
						}
						else
						{
							if (!(text == "Arcade_Prairie"))
							{
								break;
							}
							showPrairieKingMenu();
						}
						goto IL_3253;
					}
					case 15:
					{
						char c = text[1];
						if (c != 'm')
						{
							if (c != 'r')
							{
								if (c != 'v' || !(text == "EvilShrineRight"))
								{
									break;
								}
								if (Game1.spawnMonstersAtNight)
								{
									createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineRightDeActivate"), createYesNoResponses(), "evilShrineRightDeActivate");
								}
								else
								{
									createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineRightActivate"), createYesNoResponses(), "evilShrineRightActivate");
								}
							}
							else
							{
								if (!(text == "Arcade_Minecart"))
								{
									break;
								}
								if (who.hasSkullKey)
								{
									Response[] answerChoices = new Response[3]
									{
										new Response("Progress", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_ProgressMode")),
										new Response("Endless", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_EndlessMode")),
										new Response("Exit", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Exit"))
									};
									createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Menu"), answerChoices, "MinecartGame");
								}
								else
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Inactive"));
								}
							}
						}
						else
						{
							if (!(text == "EmilyRoomObject"))
							{
								break;
							}
							if (Game1.player.eventsSeen.Contains(463391) && (Game1.player.spouse == null || !Game1.player.spouse.Equals("Emily")))
							{
								TemporaryAnimatedSprite temporarySpriteByID = getTemporarySpriteByID(5858585);
								if (temporarySpriteByID != null && temporarySpriteByID is EmilysParrot)
								{
									(temporarySpriteByID as EmilysParrot).doAction();
								}
							}
							else
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:HaleyHouse_EmilyRoomObject"));
							}
						}
						goto IL_3253;
					}
					case 6:
					{
						char c = text[0];
						if (c != 'D')
						{
							if (c != 'L')
							{
								if (c != 'S' || !(text == "Saloon"))
								{
									break;
								}
								return saloon(tileLocation);
							}
							if (!(text == "Letter"))
							{
								break;
							}
							Game1.drawLetterMessage(Game1.content.LoadString("Strings\\StringsFromMaps:" + array[1].Replace("\"", "")));
						}
						else
						{
							if (!(text == "DyePot"))
							{
								break;
							}
							if (who.eventsSeen.Contains(992559))
							{
								if (!DyeMenu.IsWearingDyeable())
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:DyePot_NoDyeable"));
								}
								else
								{
									Game1.activeClickableMenu = new DyeMenu();
								}
							}
							else
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:HaleyHouse_DyePot"));
							}
						}
						goto IL_3253;
					}
					case 8:
						switch (text[3])
						{
						case 'i':
							break;
						case 'a':
							goto IL_0a97;
						case 'o':
							goto IL_0aad;
						case 'l':
							goto IL_0ac3;
						case 'e':
							goto IL_0ad9;
						case 't':
							goto IL_0b11;
						case 'b':
							goto IL_0b27;
						default:
							goto end_IL_0034;
						}
						if (!(text == "MagicInk"))
						{
							break;
						}
						if (!who.mailReceived.Contains("hasPickedUpMagicInk"))
						{
							who.mailReceived.Add("hasPickedUpMagicInk");
							who.hasMagicInk = true;
							setMapTileIndex(4, 11, 113, "Buildings");
							who.addItemByMenuIfNecessaryElseHoldUp(new SpecialItem(7));
						}
						goto IL_3253;
					case 11:
						switch (text[6])
						{
						case 'c':
							break;
						case 'k':
							goto IL_0b53;
						case 'a':
							goto IL_0b69;
						case 'e':
							goto IL_0b7f;
						case 'C':
							goto IL_0b95;
						case 'A':
							goto IL_0bab;
						case 'H':
							goto IL_0bc1;
						case 't':
							goto IL_0bd7;
						default:
							goto end_IL_0034;
						}
						if (!(text == "ColaMachine"))
						{
							break;
						}
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_ColaMachine_Question"), createYesNoResponses(), "buyJojaCola");
						goto IL_3253;
					case 4:
					{
						char c = text[0];
						if ((uint)c <= 76u)
						{
							if (c == 'C')
							{
								if (!(text == "Crib"))
								{
									break;
								}
								foreach (NPC character in characters)
								{
									if (character is Child)
									{
										if ((character as Child).Age == 1)
										{
											(character as Child).toss(who);
											return true;
										}
										if ((character as Child).Age == 0)
										{
											Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:FarmHouse_Crib_NewbornSleeping", character.displayName)));
											return true;
										}
										if ((character as Child).isInCrib() && (character as Child).Age == 2)
										{
											return character.checkAction(who, this);
										}
									}
								}
								return false;
							}
							if (c != 'D')
							{
								if (c != 'L' || !(text == "Lamp"))
								{
									break;
								}
								if ((float)lightLevel == 0f)
								{
									lightLevel.Value = 0.6f;
								}
								else
								{
									lightLevel.Value = 0f;
								}
								playSound("openBox");
							}
							else
							{
								if (!(text == "Door"))
								{
									break;
								}
								if (array.Length <= 1 || Game1.eventUp)
								{
									openDoor(tileLocation, playSound: true);
									return true;
								}
								for (int i = 1; i < array.Length; i++)
								{
									if (who.getFriendshipHeartLevelForNPC(array[i]) >= 2 || Game1.player.mailReceived.Contains("doorUnlock" + array[i]))
									{
										Rumble.rumble(0.1f, 100f);
										if (!Game1.player.mailReceived.Contains("doorUnlock" + array[i]))
										{
											Game1.player.mailReceived.Add("doorUnlock" + array[i]);
										}
										openDoor(tileLocation, playSound: true);
										return true;
									}
								}
								if (array.Length == 2 && Game1.getCharacterFromName(array[1]) != null)
								{
									tapToMove.Reset();
									NPC characterFromName2 = Game1.getCharacterFromName(array[1]);
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:DoorUnlock_NotFriend_" + ((characterFromName2.Gender == 0) ? "Male" : "Female"), characterFromName2.displayName));
								}
								else if (Game1.getCharacterFromName(array[1]) != null && Game1.getCharacterFromName(array[2]) != null)
								{
									tapToMove.Reset();
									NPC characterFromName3 = Game1.getCharacterFromName(array[1]);
									NPC characterFromName4 = Game1.getCharacterFromName(array[2]);
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:DoorUnlock_NotFriend_Couple", characterFromName3.displayName, characterFromName4.displayName));
								}
								else if (Game1.getCharacterFromName(array[1]) != null)
								{
									tapToMove.Reset();
									NPC characterFromName5 = Game1.getCharacterFromName(array[1]);
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:DoorUnlock_NotFriend_" + ((characterFromName5.Gender == 0) ? "Male" : "Female"), characterFromName5.displayName));
								}
							}
						}
						else
						{
							if (c == 'M')
							{
								if (!(text == "Mine"))
								{
									break;
								}
								goto IL_2661;
							}
							if (c != 'W')
							{
								if (c != 'Y' || !(text == "Yoba"))
								{
									break;
								}
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:SeedShop_Yoba"));
							}
							else
							{
								if (!(text == "Warp"))
								{
									break;
								}
								who.faceGeneralDirection(new Vector2(tileLocation.X, tileLocation.Y) * 64f);
								Rumble.rumble(0.15f, 200f);
								if (array.Length < 5)
								{
									playSoundAt("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
								}
								Game1.warpFarmer(array[3], Convert.ToInt32(array[1]), Convert.ToInt32(array[2]), flip: false);
							}
						}
						goto IL_3253;
					}
					case 7:
					{
						char c = text[1];
						if ((uint)c <= 101u)
						{
							if (c != 'a')
							{
								if (c != 'e' || !(text == "Message"))
								{
									break;
								}
								goto IL_2057;
							}
							if (!(text == "Mailbox"))
							{
								break;
							}
							if (this is Farm)
							{
								Point mailboxPosition = Game1.player.getMailboxPosition();
								if (tileLocation.X != mailboxPosition.X || tileLocation.Y != mailboxPosition.Y)
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Farm_OtherPlayerMailbox"));
									goto IL_3253;
								}
							}
							mailbox();
						}
						else if (c != 'i')
						{
							if (c == 'r')
							{
								if (!(text == "DropBox"))
								{
									break;
								}
								string box_id = array[1];
								int minimum_capacity = 0;
								foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
								{
									if (specialOrder.UsesDropBox(box_id))
									{
										minimum_capacity = Math.Max(minimum_capacity, specialOrder.GetMinimumDropBoxCapacity(box_id));
									}
								}
								foreach (SpecialOrder order in Game1.player.team.specialOrders)
								{
									if (!order.UsesDropBox(box_id))
									{
										continue;
									}
									order.donateMutex.RequestLock(delegate
									{
										while (order.donatedItems.Count < minimum_capacity)
										{
											order.donatedItems.Add(null);
										}
										Game1.activeClickableMenu = new QuestContainerMenu(order.donatedItems, 3, order.HighlightAcceptableItems, order.GetAcceptCount, order.UpdateDonationCounts, order.ConfirmCompleteDonations);
									});
									return true;
								}
								return false;
							}
							if (c != 'u' || !(text == "Jukebox"))
							{
								break;
							}
							Game1.activeClickableMenu = new ChooseFromListMenu(Game1.player.songsHeard.Distinct().ToList(), ChooseFromListMenu.playSongAction, isJukebox: true);
						}
						else
						{
							if (!(text == "QiCoins"))
							{
								break;
							}
							if (who.clubCoins > 0)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Club_QiCoins", who.clubCoins));
							}
							else
							{
								createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_QiCoins_BuyStarter"), createYesNoResponses(), "BuyClubCoins");
							}
						}
						goto IL_3253;
					}
					case 24:
					{
						char c = text[0];
						if (c != 'N')
						{
							if (c != 'T' || !(text == "Theater_PosterComingSoon"))
							{
								break;
							}
							if (Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
							{
								WorldDate worldDate = new WorldDate(Game1.Date);
								worldDate.TotalDays += 28;
								MovieData movieForDate = MovieTheater.GetMovieForDate(worldDate);
								if (movieForDate != null)
								{
									Game1.multipleDialogues(new string[1] { Game1.content.LoadString("Strings\\Locations:Theater_Poster_Coming_Soon", movieForDate.Title) });
								}
							}
							goto IL_3253;
						}
						if (!(text == "NPCSpeechMessageNoRadius"))
						{
							break;
						}
						NPC characterFromName = Game1.getCharacterFromName(array[1]);
						if (characterFromName != null)
						{
							try
							{
								characterFromName.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromMaps:" + array[2]), add: true);
								Game1.drawDialogue(characterFromName);
								return true;
							}
							catch (Exception)
							{
								return false;
							}
						}
						try
						{
							NPC nPC = new NPC(null, Vector2.Zero, "", 0, array[1], datable: false, null, Game1.temporaryContent.Load<Texture2D>("Portraits\\" + array[1]));
							nPC.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromMaps:" + array[2]), add: true);
							Game1.drawDialogue(nPC);
							return true;
						}
						catch (Exception)
						{
							return false;
						}
					}
					case 19:
						if (!(text == "WarpCommunityCenter"))
						{
							break;
						}
						if (Game1.MasterPlayer.mailReceived.Contains("ccDoorUnlock") || Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
						{
							playSoundAt("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
							Game1.warpFarmer("CommunityCenter", 32, 23, flip: false);
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8175"));
						}
						goto IL_3253;
					case 3:
						{
							if (!(text == "Buy"))
							{
								break;
							}
							return openShopMenu(array[1]);
						}
						IL_0530:
						if (!(text == "IceCreamStand"))
						{
							break;
						}
						if (isCharacterAtTile(new Vector2(tileLocation.X, tileLocation.Y - 2)) != null || isCharacterAtTile(new Vector2(tileLocation.X, tileLocation.Y - 1)) != null || isCharacterAtTile(new Vector2(tileLocation.X, tileLocation.Y - 3)) != null)
						{
							TutorialManager.Instance.SeenShop(TutorialShopLocation.IceCreamStand);
							Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
							dictionary.Add(new Object(233, 1), new int[2] { 250, 2147483647 });
							Game1.activeClickableMenu = new ShopMenu(dictionary);
						}
						else if (Game1.currentSeason.Equals("summer"))
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:IceCreamStand_ComeBackLater"));
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:IceCreamStand_NotSummer"));
						}
						goto IL_3253;
						IL_0546:
						if (!(text == "AdventureShop"))
						{
							break;
						}
						adventureShop();
						goto IL_3253;
						IL_06f4:
						if (!(text == "ClubSlots"))
						{
							if (!(text == "ClubCards"))
							{
								break;
							}
							goto IL_2962;
						}
						Game1.currentMinigame = new Slots();
						goto IL_3253;
						IL_08df:
						if (!(text == "TunnelSafe"))
						{
							break;
						}
						if (who.ActiveObject != null && (int)who.ActiveObject.parentSheetIndex == 787 && !who.hasOrWillReceiveMail("TH_Tunnel"))
						{
							who.reduceActiveItemByOne();
							Game1.player.CanMove = false;
							playSound("openBox");
							DelayedAction.playSoundAfterDelay("doorCreakReverse", 500);
							Game1.player.mailReceived.Add("TH_Tunnel");
							Game1.multipleDialogues(new string[2]
							{
								Game1.content.LoadString("Strings\\Locations:Tunnel_TunnelSafe_ConsumeBattery"),
								Game1.content.LoadString("Strings\\Locations:Tunnel_TunnelSafe_MrQiNote")
							});
							Game1.player.addQuest(2);
						}
						else if (who.hasOrWillReceiveMail("TH_Tunnel"))
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Tunnel_TunnelSafe_MrQiNote"));
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Tunnel_TunnelSafe_Initial"));
						}
						goto IL_3253;
						IL_07b5:
						if (!(text == "HospitalShop"))
						{
							break;
						}
						if (isCharacterAtTile(who.getTileLocation() + new Vector2(0f, -2f)) != null || isCharacterAtTile(who.getTileLocation() + new Vector2(-1f, -2f)) != null || isCharacterAtTile(who.getTileLocation() + new Vector2(1f, -2f)) != null)
						{
							TutorialManager.Instance.SeenShop(TutorialShopLocation.Hospital);
							Game1.activeClickableMenu = new ShopMenu(Utility.getHospitalStock());
						}
						goto IL_3253;
						IL_0670:
						if (!(text == "Starpoint"))
						{
							break;
						}
						try
						{
							doStarpoint(array[1]);
						}
						catch (Exception)
						{
						}
						goto IL_3253;
						IL_2057:
						Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\StringsFromMaps:" + array[1].Replace("\"", "")));
						goto IL_3253;
						IL_08b8:
						if (!(text == "SandDragon"))
						{
							if (!(text == "StorageBox"))
							{
								break;
							}
							openStorageBox(actionParamsToString(array));
						}
						else if (who.ActiveObject != null && (int)who.ActiveObject.parentSheetIndex == 768 && !who.hasOrWillReceiveMail("TH_SandDragon") && who.hasOrWillReceiveMail("TH_MayorFridge"))
						{
							who.reduceActiveItemByOne();
							Game1.player.CanMove = false;
							localSound("eat");
							Game1.player.mailReceived.Add("TH_SandDragon");
							Game1.multipleDialogues(new string[2]
							{
								Game1.content.LoadString("Strings\\Locations:Desert_SandDragon_ConsumeEssence"),
								Game1.content.LoadString("Strings\\Locations:Desert_SandDragon_MrQiNote")
							});
							Game1.player.removeQuest(4);
							Game1.player.addQuest(5);
						}
						else if (who.hasOrWillReceiveMail("TH_SandDragon"))
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Desert_SandDragon_MrQiNote"));
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Desert_SandDragon_Initial"));
						}
						goto IL_3253;
						IL_079f:
						if (!(text == "WizardShrine"))
						{
							break;
						}
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WizardTower_WizardShrine").Replace('\n', '^'), createYesNoResponses(), "WizardShrine");
						goto IL_3253;
						IL_0bd7:
						if (!(text == "ElliottBook"))
						{
							break;
						}
						if (who.eventsSeen.Contains(41))
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ElliottHouse_ElliottBook_Filled", Game1.elliottBookName, who.displayName)));
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ElliottHouse_ElliottBook_Blank"));
						}
						goto IL_3253;
						IL_06c8:
						if (!(text == "ItemChest"))
						{
							break;
						}
						openItemChest(tileLocation, Convert.ToInt32(array[1]));
						goto IL_3253;
						IL_0bc1:
						if (!(text == "WizardHatch"))
						{
							break;
						}
						if (who.friendshipData.ContainsKey("Wizard") && who.friendshipData["Wizard"].Points >= 1000)
						{
							playSoundAt("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
							Game1.warpFarmer("WizardHouseBasement", 4, 4, flip: true);
						}
						else
						{
							NPC nPC2 = characters[0];
							nPC2.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Data\\ExtraDialogue:Wizard_Hatch"), nPC2));
							Game1.drawDialogue(nPC2);
						}
						goto IL_3253;
						IL_051a:
						if (!(text == "SpecialOrders"))
						{
							break;
						}
						Game1.player.team.ordersBoardMutex.RequestLock(delegate
						{
							Game1.activeClickableMenu = new SpecialOrdersBoard
							{
								behaviorBeforeCleanup = delegate
								{
									Game1.player.team.ordersBoardMutex.ReleaseLock();
								}
							};
						});
						goto IL_3253;
						IL_07f7:
						if (!(text == "GoldenScythe"))
						{
							break;
						}
						if (!Game1.player.mailReceived.Contains("gotGoldenScythe"))
						{
							if (!Game1.player.isInventoryFull())
							{
								Game1.playSound("parry");
								Game1.player.mailReceived.Add("gotGoldenScythe");
								setMapTileIndex(29, 4, 245, "Front");
								setMapTileIndex(30, 4, 246, "Front");
								setMapTileIndex(29, 5, 261, "Front");
								setMapTileIndex(30, 5, 262, "Front");
								setMapTileIndex(29, 6, 277, "Buildings");
								setMapTileIndex(30, 56, 278, "Buildings");
								Game1.player.addItemByMenuIfNecessaryElseHoldUp(new MeleeWeapon(53));
							}
							else
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
							}
						}
						else
						{
							Game1.changeMusicTrack("none");
							performTouchAction("MagicWarp Mine 67 10", Game1.player.getStandingPosition());
						}
						goto IL_3253;
						IL_0686:
						if (!(text == "playSound"))
						{
							break;
						}
						localSound(array[1]);
						goto IL_3253;
						IL_0bab:
						if (!(text == "SpiritAltar"))
						{
							break;
						}
						if (who.ActiveObject != null && (double)Game1.player.team.sharedDailyLuck != -0.12 && (double)Game1.player.team.sharedDailyLuck != 0.12)
						{
							if (who.ActiveObject.Price >= 60)
							{
								temporarySprites.Add(new TemporaryAnimatedSprite(352, 70f, 2, 2, new Vector2(tileLocation.X * 64, tileLocation.Y * 64), flicker: false, flipped: false));
								Game1.player.team.sharedDailyLuck.Value = 0.12;
								playSound("money");
							}
							else
							{
								temporarySprites.Add(new TemporaryAnimatedSprite(362, 50f, 6, 1, new Vector2(tileLocation.X * 64, tileLocation.Y * 64), flicker: false, flipped: false));
								Game1.player.team.sharedDailyLuck.Value = -0.12;
								playSound("thunder");
							}
							who.ActiveObject = null;
							who.showNotCarrying();
						}
						goto IL_3253;
						IL_08a2:
						if (!(text == "LumberPile"))
						{
							break;
						}
						if (!who.hasOrWillReceiveMail("TH_LumberPile") && who.hasOrWillReceiveMail("TH_SandDragon"))
						{
							Game1.player.hasClubCard = true;
							Game1.player.CanMove = false;
							Game1.player.mailReceived.Add("TH_LumberPile");
							Game1.player.addItemByMenuIfNecessaryElseHoldUp(new SpecialItem(2));
							Game1.player.removeQuest(5);
						}
						goto IL_3253;
						IL_080d:
						if (!(text == "ClubComputer"))
						{
							break;
						}
						goto IL_2a9f;
						IL_2a9f:
						farmerFile();
						goto IL_3253;
						IL_087b:
						if (!(text == "BuyQiCoins"))
						{
							if (!(text == "Blacksmith"))
							{
								break;
							}
							return blacksmith(tileLocation);
						}
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_Buy100Coins"), createYesNoResponses(), "BuyQiCoins");
						goto IL_3253;
						IL_07e1:
						if (!(text == "MineElevator"))
						{
							break;
						}
						if (MineShaft.lowestLevelReached < 5)
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Mines_MineElevator_NotWorking")));
						}
						else
						{
							Game1.activeClickableMenu = new MineElevatorMenu();
						}
						goto IL_3253;
						IL_0963:
						if (!(text == "DwarfGrave"))
						{
							break;
						}
						if (who.canUnderstandDwarves)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_DwarfGrave_Translated").Replace('\n', '^'));
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8214"));
						}
						goto IL_3253;
						IL_071b:
						if (!(text == "BlackJack"))
						{
							break;
						}
						goto IL_2962;
						IL_0b95:
						if (!(text == "RemoveChest"))
						{
							break;
						}
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:RemoveChest"));
						map.GetLayer("Buildings").Tiles[tileLocation.X, tileLocation.Y] = null;
						goto IL_3253;
						IL_2962:
						if (array.Length > 1 && array[1].Equals("1000"))
						{
							createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_HS"), new Response[2]
							{
								new Response("Play", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Play")),
								new Response("Leave", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Leave"))
							}, "CalicoJackHS");
						}
						else
						{
							createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_CalicoJack"), new Response[3]
							{
								new Response("Play", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Play")),
								new Response("Leave", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Leave")),
								new Response("Rules", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Rules"))
							}, "CalicoJack");
						}
						goto IL_3253;
						IL_069c:
						if (!(text == "GetLumber"))
						{
							break;
						}
						GetLumber();
						goto IL_3253;
						IL_0b7f:
						if (!(text == "MessageOnce"))
						{
							break;
						}
						if (!who.eventsSeen.Contains(Convert.ToInt32(array[1])))
						{
							who.eventsSeen.Add(Convert.ToInt32(array[1]));
							Game1.drawObjectDialogue(Game1.parseText(actionParamsToString(array).Substring(actionParamsToString(array).IndexOf(' '))));
						}
						goto IL_3253;
						IL_094d:
						if (!(text == "EnterSewer"))
						{
							break;
						}
						if (who.hasRustyKey && !who.mailReceived.Contains("OpenedSewer"))
						{
							playSound("openBox");
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Forest_OpenedSewer")));
							who.mailReceived.Add("OpenedSewer");
						}
						else if (who.mailReceived.Contains("OpenedSewer"))
						{
							playSoundAt("stairsdown", new Vector2(tileLocation.X, tileLocation.Y));
							Game1.warpFarmer("Sewer", 16, 11, 2);
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
						}
						goto IL_3253;
						IL_0b69:
						if (!(text == "RailroadBox"))
						{
							break;
						}
						if (who.ActiveObject != null && (int)who.ActiveObject.parentSheetIndex == 394 && !who.hasOrWillReceiveMail("TH_Railroad") && who.hasOrWillReceiveMail("TH_Tunnel"))
						{
							who.reduceActiveItemByOne();
							Game1.player.CanMove = false;
							localSound("Ship");
							Game1.player.mailReceived.Add("TH_Railroad");
							Game1.multipleDialogues(new string[2]
							{
								Game1.content.LoadString("Strings\\Locations:Railroad_Box_ConsumeShell"),
								Game1.content.LoadString("Strings\\Locations:Railroad_Box_MrQiNote")
							});
							Game1.player.removeQuest(2);
							Game1.player.addQuest(3);
						}
						else if (who.hasOrWillReceiveMail("TH_Railroad"))
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Railroad_Box_MrQiNote"));
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Railroad_Box_Initial"));
						}
						goto IL_3253;
						IL_0572:
						if (!(text == "NextMineLevel"))
						{
							break;
						}
						goto IL_2661;
						IL_18f0:
						if (who.hasSkullKey)
						{
							if (!who.hasUnlockedSkullDoor)
							{
								Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:SkullCave_SkullDoor_Unlock")));
								DelayedAction.playSoundAfterDelay("openBox", 500);
								DelayedAction.playSoundAfterDelay("openBox", 700);
								Game1.addMailForTomorrow("skullCave");
								who.hasUnlockedSkullDoor = true;
								who.completeQuest(19);
							}
							else
							{
								who.completelyStopAnimatingOrDoingAction();
								playSound("doorClose");
								DelayedAction.playSoundAfterDelay("stairsdown", 500, this);
								Game1.enterMine(121);
								MineShaft.numberOfCraftedStairsUsedThisRun = 0;
							}
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:SkullCave_SkullDoor_Locked"));
						}
						goto IL_3253;
						IL_0b53:
						if (!(text == "BuyBackpack"))
						{
							break;
						}
						response = new Response("Purchase", Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Response2000"));
						response2 = new Response("Purchase", Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Response10000"));
						response3 = new Response("Not", Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_ResponseNo"));
						if ((int)Game1.player.maxItems == 12)
						{
							createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Question24"), new Response[2] { response, response3 }, "Backpack");
						}
						else if ((int)Game1.player.maxItems < 36)
						{
							createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Question36"), new Response[2] { response2, response3 }, "Backpack");
						}
						goto IL_3253;
						IL_0921:
						if (!(text == "FarmerFile"))
						{
							break;
						}
						goto IL_2a9f;
						IL_0937:
						if (!(text == "ClubSeller"))
						{
							break;
						}
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_ClubSeller"), new Response[2]
						{
							new Response("I'll", Game1.content.LoadString("Strings\\Locations:Club_ClubSeller_Yes")),
							new Response("No", Game1.content.LoadString("Strings\\Locations:Club_ClubSeller_No"))
						}, "ClubSeller");
						goto IL_3253;
						IL_3253:
						return true;
						IL_0622:
						switch (text)
						{
						case "Tailoring":
							break;
						case "SkullDoor":
							goto IL_18f0;
						case "Billboard":
							goto IL_24b2;
						default:
							goto end_IL_0034;
						}
						if (who.eventsSeen.Contains(992559))
						{
							Game1.activeClickableMenu = new TailoringMenu();
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:HaleyHouse_SewingMachine"));
						}
						goto IL_3253;
						IL_055c:
						if (!(text == "MessageSpeech"))
						{
							break;
						}
						goto IL_2057;
						IL_08f5:
						if (!(text == "NPCMessage"))
						{
							break;
						}
						characterFromName6 = Game1.getCharacterFromName(array[1]);
						if (characterFromName6 != null && characterFromName6.currentLocation == who.currentLocation && Utility.tileWithinRadiusOfPlayer(characterFromName6.getTileX(), characterFromName6.getTileY(), 14, who))
						{
							try
							{
								string text3 = action.Substring(action.IndexOf('"') + 1).Split('/')[0];
								string text4 = text3.Substring(text3.IndexOf(':') + 1);
								characterFromName6.setNewDialogue(Game1.content.LoadString(text3), add: true);
								Game1.drawDialogue(characterFromName6);
								switch (text4)
								{
								case "AnimalShop_Marnie_Trash":
								case "JoshHouse_Alex_Trash":
								case "SamHouse_Sam_Trash":
								case "SeedShop_Abigail_Drawers":
									if (who != null)
									{
										Game1.multiplayer.globalChatInfoMessage("Caught_Snooping", who.name, characterFromName6.displayName);
									}
									break;
								}
								return true;
							}
							catch (Exception)
							{
								return false;
							}
						}
						try
						{
							Game1.drawDialogueNoTyping(Game1.content.LoadString(action.Substring(action.IndexOf('"')).Split('/')[1].Replace("\"", "")));
							return false;
						}
						catch (Exception)
						{
							return false;
						}
						IL_24b2:
						dailyQuest = array[1].Equals("3");
						Game1.activeClickableMenu = new Billboard(dailyQuest);
						goto IL_3253;
						IL_0588:
						if (!(text == "MineWallDecor"))
						{
							break;
						}
						getWallDecorItem(tileLocation);
						goto IL_3253;
						IL_0b11:
						if (!(text == "ExitMine"))
						{
							break;
						}
						answerChoices3 = new Response[3]
						{
							new Response("Leave", Game1.content.LoadString("Strings\\Locations:Mines_LeaveMine")),
							new Response("Go", Game1.content.LoadString("Strings\\Locations:Mines_GoUp")),
							new Response("Do", Game1.content.LoadString("Strings\\Locations:Mines_DoNothing"))
						};
						createQuestionDialogue(" ", answerChoices3, "ExitMine");
						goto IL_3253;
						IL_0ad9:
						switch (text)
						{
						case "Material":
							break;
						case "MineSign":
							goto IL_28ce;
						case "Minecart":
							goto IL_28f0;
						default:
							goto end_IL_0034;
						}
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8205", who.WoodPieces, who.StonePieces).Replace("\n", "^"));
						goto IL_3253;
						IL_28f0:
						openChest(tileLocation, 4, Game1.random.Next(3, 7));
						goto IL_3253;
						IL_065a:
						if (!(text == "LeoParrot"))
						{
							break;
						}
						temporarySpriteByID2 = getTemporarySpriteByID(5858585);
						if (temporarySpriteByID2 != null && temporarySpriteByID2 is EmilysParrot)
						{
							(temporarySpriteByID2 as EmilysParrot).doAction();
						}
						goto IL_3253;
						IL_0b27:
						if (!(text == "ClubShop"))
						{
							break;
						}
						TutorialManager.Instance.SeenShop(TutorialShopLocation.Club);
						Game1.activeClickableMenu = new ShopMenu(Utility.getQiShopStock(), 2);
						goto IL_3253;
						IL_2661:
						playSound("stairsdown");
						Game1.enterMine((array.Length == 1) ? 1 : Convert.ToInt32(array[1]));
						goto IL_3253;
						IL_0ac3:
						if (!(text == "Dialogue"))
						{
							break;
						}
						Game1.drawDialogueNoTyping(actionParamsToString(array));
						goto IL_3253;
						IL_28ce:
						Game1.drawObjectDialogue(Game1.parseText(actionParamsToString(array)));
						goto IL_3253;
						IL_07cb:
						if (!(text == "ElliottPiano"))
						{
							break;
						}
						playElliottPiano(int.Parse(array[1]));
						goto IL_3253;
						IL_0a97:
						if (!(text == "JojaShop"))
						{
							break;
						}
						TutorialManager.Instance.SeenShop(TutorialShopLocation.JojaMart);
						Game1.activeClickableMenu = new ShopMenu(Utility.getJojaStock());
						goto IL_3253;
						IL_0aad:
						if (!(text == "Tutorial"))
						{
							break;
						}
						Game1.activeClickableMenu = new TutorialMenu();
						goto IL_3253;
						IL_06de:
						if (!(text == "Incubator"))
						{
							break;
						}
						(this as AnimalHouse).incubator();
						goto IL_3253;
						end_IL_0034:
						break;
					}
				}
				return false;
			}
			if (action != null && !who.IsLocalPlayer)
			{
				string[] array2 = action.ToString().Split(' ');
				switch (array2[0])
				{
				case "Minecart":
					openChest(tileLocation, 4, Game1.random.Next(3, 7));
					break;
				case "RemoveChest":
					map.GetLayer("Buildings").Tiles[tileLocation.X, tileLocation.Y] = null;
					break;
				case "Door":
					openDoor(tileLocation, playSound: true);
					break;
				case "TV":
					Game1.tvStation = Game1.random.Next(2);
					break;
				}
			}
			return false;
		}

		public void showPrairieKingMenu()
		{
			if (Game1.player.jotpkProgress.Value == null)
			{
				Game1.currentMinigame = new AbigailGame();
				return;
			}
			Response[] answerChoices = new Response[3]
			{
				new Response("Continue", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_Continue")),
				new Response("NewGame", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_NewGame")),
				new Response("Exit", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Exit"))
			};
			createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_Menu"), answerChoices, "CowboyGame");
		}

		public Vector2 findNearestObject(Vector2 startingPoint, int objectIndex, bool bigCraftable)
		{
			int num = 0;
			Queue<Vector2> queue = new Queue<Vector2>();
			queue.Enqueue(startingPoint);
			HashSet<Vector2> hashSet = new HashSet<Vector2>();
			List<Vector2> list = new List<Vector2>();
			while (num < 1000)
			{
				if (objects.ContainsKey(startingPoint) && (int)objects[startingPoint].parentSheetIndex == objectIndex && (bool)objects[startingPoint].bigCraftable == bigCraftable)
				{
					queue = null;
					hashSet = null;
					return startingPoint;
				}
				num++;
				hashSet.Add(startingPoint);
				list = Utility.getAdjacentTileLocations(startingPoint);
				for (int i = 0; i < list.Count; i++)
				{
					if (!hashSet.Contains(list[i]))
					{
						queue.Enqueue(list[i]);
					}
				}
				startingPoint = queue.Dequeue();
			}
			queue = null;
			hashSet = null;
			return Vector2.Zero;
		}

		public void lockedDoorWarp(string[] actionParams)
		{
			bool flag = Game1.player.HasTownKey;
			if (AreStoresClosedForFestival() && Name != "Desert" && GetLocationContext() == LocationContext.Default)
			{
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:FestivalDay_DoorLocked")));
				return;
			}
			if (actionParams[3].Equals("SeedShop") && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Wed") && !Utility.HasAnyPlayerSeenEvent(191393) && !flag)
			{
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:SeedShop_LockedWed")));
				return;
			}
			int num = Convert.ToInt32(actionParams[4]);
			if (actionParams[3] == "FishShop" && Game1.player.mailReceived.Contains("willyHours"))
			{
				num = 800;
			}
			if (flag)
			{
				if (flag && Name == "Desert")
				{
					flag = false;
				}
				if (flag && GetLocationContext() != 0)
				{
					flag = false;
				}
				if (flag && this is BeachNightMarket && actionParams[3] != "FishShop")
				{
					flag = false;
				}
			}
			if ((flag || (Game1.timeOfDay >= num && Game1.timeOfDay < Convert.ToInt32(actionParams[5]))) && (actionParams.Length < 7 || Game1.currentSeason.Equals("winter") || (Game1.player.friendshipData.ContainsKey(actionParams[6]) && Game1.player.friendshipData[actionParams[6]].Points >= Convert.ToInt32(actionParams[7]))))
			{
				Rumble.rumble(0.15f, 200f);
				Game1.player.completelyStopAnimatingOrDoingAction();
				playSoundAt("doorClose", Game1.player.getTileLocation());
				Game1.warpFarmer(actionParams[3], Convert.ToInt32(actionParams[1]), Convert.ToInt32(actionParams[2]), flip: false);
			}
			else if (actionParams.Length < 7)
			{
				string sub = Game1.getTimeOfDayString(Convert.ToInt32(actionParams[4])).Replace(" ", "");
				if (actionParams[3] == "FishShop" && Game1.player.mailReceived.Contains("willyHours"))
				{
					sub = Game1.getTimeOfDayString(800).Replace(" ", "");
				}
				string sub2 = Game1.getTimeOfDayString(Convert.ToInt32(actionParams[5])).Replace(" ", "");
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor_OpenRange", sub, sub2));
			}
			else if (Game1.timeOfDay < Convert.ToInt32(actionParams[4]) || Game1.timeOfDay >= Convert.ToInt32(actionParams[5]))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
			}
			else
			{
				NPC characterFromName = Game1.getCharacterFromName(actionParams[6]);
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor_FriendsOnly", characterFromName.displayName));
			}
		}

		public void playElliottPiano(int key)
		{
			if (Game1.IsMultiplayer && (long)Game1.player.uniqueMultiplayerID % 111 == 0L)
			{
				switch (key)
				{
				case 1:
					playSoundPitched("toyPiano", 500);
					break;
				case 2:
					playSoundPitched("toyPiano", 1200);
					break;
				case 3:
					playSoundPitched("toyPiano", 1400);
					break;
				case 4:
					playSoundPitched("toyPiano", 2000);
					break;
				}
				return;
			}
			switch (key)
			{
			case 1:
				playSoundPitched("toyPiano", 1100);
				break;
			case 2:
				playSoundPitched("toyPiano", 1500);
				break;
			case 3:
				playSoundPitched("toyPiano", 1600);
				break;
			case 4:
				playSoundPitched("toyPiano", 1800);
				break;
			}
			if (Game1.elliottPiano == 0)
			{
				if (key == 2)
				{
					Game1.elliottPiano++;
				}
				else
				{
					Game1.elliottPiano = 0;
				}
			}
			else if (Game1.elliottPiano == 1)
			{
				if (key == 4)
				{
					Game1.elliottPiano++;
				}
				else
				{
					Game1.elliottPiano = 0;
				}
			}
			else if (Game1.elliottPiano == 2)
			{
				if (key == 3)
				{
					Game1.elliottPiano++;
				}
				else
				{
					Game1.elliottPiano = 0;
				}
			}
			else if (Game1.elliottPiano == 3)
			{
				if (key == 2)
				{
					Game1.elliottPiano++;
				}
				else
				{
					Game1.elliottPiano = 0;
				}
			}
			else if (Game1.elliottPiano == 4)
			{
				if (key == 3)
				{
					Game1.elliottPiano++;
				}
				else
				{
					Game1.elliottPiano = 0;
				}
			}
			else if (Game1.elliottPiano == 5)
			{
				if (key == 4)
				{
					Game1.elliottPiano++;
				}
				else
				{
					Game1.elliottPiano = 0;
				}
			}
			else if (Game1.elliottPiano == 6)
			{
				if (key == 2)
				{
					Game1.elliottPiano++;
				}
				else
				{
					Game1.elliottPiano = 0;
				}
			}
			else
			{
				if (Game1.elliottPiano != 7)
				{
					return;
				}
				if (key == 1)
				{
					Game1.elliottPiano = 0;
					NPC characterFromName = getCharacterFromName("Elliott");
					if (!Game1.eventUp && characterFromName != null && !characterFromName.isMoving())
					{
						characterFromName.faceTowardFarmerForPeriod(1000, 100, faceAway: false, Game1.player);
						characterFromName.doEmote(20);
					}
				}
				else
				{
					Game1.elliottPiano = 0;
				}
			}
		}

		public void readNote(int which)
		{
			if ((int)Game1.netWorldState.Value.LostBooksFound >= which)
			{
				string message = Game1.content.LoadString("Strings\\Notes:" + which).Replace('\n', '^');
				if (!Game1.player.mailReceived.Contains("lb_" + which))
				{
					Game1.player.mailReceived.Add("lb_" + which);
				}
				removeTemporarySpritesWithIDLocal(which);
				Game1.drawLetterMessage(message);
			}
			else
			{
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Notes:Missing")));
			}
		}

		public void mailbox()
		{
			if (Game1.mailbox.Count > 0)
			{
				if (!Game1.player.mailReceived.Contains(Game1.mailbox.First()) && !Game1.mailbox.First().Contains("passedOut") && !Game1.mailbox.First().Contains("Cooking"))
				{
					Game1.player.mailReceived.Add(Game1.mailbox.First());
				}
				string text = Game1.mailbox.First();
				Game1.mailbox.RemoveAt(0);
				Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
				string text2 = (dictionary.ContainsKey(text) ? dictionary[text] : "");
				string text3 = "";
				string text4 = "";
				if (text.StartsWith("passedOut "))
				{
					string[] array = text.Split(' ');
					int num = ((array.Length > 1) ? Convert.ToInt32(array[1]) : 0);
					Random random = new Random(num);
					switch (random.Next((Game1.player.getSpouse() != null && Game1.player.getSpouse().Name.Equals("Harvey")) ? 2 : 3))
					{
					case 0:
						text2 = ((!Game1.MasterPlayer.hasCompletedCommunityCenter() || Game1.MasterPlayer.mailReceived.Contains("JojaMember")) ? string.Format(dictionary["passedOut1_" + ((num > 0) ? "Billed" : "NotBilled") + "_" + (Game1.player.IsMale ? "Male" : "Female")], num) : string.Format(dictionary["passedOut4"], num));
						break;
					case 1:
						text2 = string.Format(dictionary["passedOut2"], num);
						break;
					case 2:
						text2 = string.Format(dictionary["passedOut3_" + ((num > 0) ? "Billed" : "NotBilled")], num);
						break;
					}
				}
				else if (text.StartsWith("passedOut"))
				{
					string[] array2 = text.Split(' ');
					if (array2.Length > 1)
					{
						int num2 = Convert.ToInt32(array2[1]);
						text2 = string.Format(dictionary[array2[0]], num2);
					}
				}
				if (text2.Length != 0)
				{
					Game1.activeClickableMenu = new LetterViewerMenu(text2, text);
				}
			}
			else if (Game1.mailbox.Count == 0)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8429"));
			}
		}

		public void farmerFile()
		{
			Game1.multipleDialogues(new string[2]
			{
				Game1.content.LoadString("Strings\\UI:FarmerFile_1", Game1.player.Name, Game1.stats.StepsTaken, Game1.stats.GiftsGiven, Game1.stats.DaysPlayed, Game1.stats.DirtHoed, Game1.stats.ItemsCrafted, Game1.stats.ItemsCooked, Game1.stats.PiecesOfTrashRecycled).Replace('\n', '^'),
				Game1.content.LoadString("Strings\\UI:FarmerFile_2", Game1.stats.MonstersKilled, Game1.stats.FishCaught, Game1.stats.TimesFished, Game1.stats.SeedsSown, Game1.stats.ItemsShipped).Replace('\n', '^')
			});
		}

		public void openItemChest(Location location, int whichObject)
		{
			playSound("openBox");
			if (Game1.player.ActiveObject == null)
			{
				if (whichObject == 434)
				{
					Game1.player.ActiveObject = new Object(Vector2.Zero, 434, "Cosmic Fruit", canBeSetDown: false, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false);
					Game1.player.eatHeldObject();
				}
				else
				{
					debris.Add(new Debris(whichObject, new Vector2(location.X * 64, location.Y * 64), Game1.player.Position));
				}
				map.GetLayer("Buildings").Tiles[location.X, location.Y].TileIndex++;
				map.GetLayer("Buildings").Tiles[location].Properties["Action"] = new PropertyValue("RemoveChest");
			}
		}

		public void getWallDecorItem(Location location)
		{
		}

		public static string getFavoriteItemName(string character)
		{
			string result = "???";
			if (Game1.NPCGiftTastes.ContainsKey(character))
			{
				string[] array = Game1.NPCGiftTastes[character].Split('/')[1].Split(' ');
				result = Game1.objectInformation[Convert.ToInt32(array[Game1.random.Next(array.Length)])].Split('/')[0];
			}
			return result;
		}

		public static void openCraftingMenu(string nameOfCraftingDevice)
		{
			Game1.activeClickableMenu = new GameMenu(3);
		}

		private void openStorageBox(string which)
		{
		}

		public virtual bool openShopMenu(string which)
		{
			if (which.Equals("Fish"))
			{
				if (getCharacterFromName("Willy") != null)
				{
					TutorialManager.Instance.SeenShop(TutorialShopLocation.FishShop);
					Game1.activeClickableMenu = new ShopMenu(Utility.getFishShopStock(Game1.player), 0, "Willy");
					return true;
				}
				return false;
			}
			if (this is SeedShop)
			{
				if (getCharacterFromName("Pierre") != null && getCharacterFromName("Pierre").getTileLocation().Equals(new Vector2(4f, 17f)) && Game1.player.getTileY() > getCharacterFromName("Pierre").getTileY())
				{
					Game1.activeClickableMenu = new ShopMenu((this as SeedShop).shopStock(), 0, "Pierre");
				}
				else if (getCharacterFromName("Pierre") == null && Game1.IsVisitingIslandToday("Pierre"))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:SeedShop_MoneyBox"));
					Game1.afterDialogues = delegate
					{
						Game1.activeClickableMenu = new ShopMenu((this as SeedShop).shopStock());
					};
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8525"));
				}
				return true;
			}
			if (name.Equals("SandyHouse"))
			{
				NPC characterFromName = getCharacterFromName("Sandy");
				if (characterFromName != null && characterFromName.currentLocation == this)
				{
					TutorialManager.Instance.SeenShop(TutorialShopLocation.SandyHouse);
					Game1.activeClickableMenu = new ShopMenu(sandyShopStock(), 0, "Sandy", onSandyShopPurchase);
				}
				return true;
			}
			return false;
		}

		public virtual bool isObjectAt(int x, int y)
		{
			Vector2 key = new Vector2(x / 64, y / 64);
			foreach (Furniture item in furniture)
			{
				if (item.boundingBox.Value.Contains(x, y))
				{
					return true;
				}
			}
			if (objects.ContainsKey(key))
			{
				return true;
			}
			return false;
		}

		public virtual bool isObjectAtTile(int tileX, int tileY)
		{
			Vector2 key = new Vector2(tileX, tileY);
			foreach (Furniture item in furniture)
			{
				if (item.boundingBox.Value.Contains(tileX * 64, tileY * 64))
				{
					return true;
				}
			}
			if (objects.ContainsKey(key))
			{
				return true;
			}
			return false;
		}

		public virtual Object getObjectAt(int x, int y)
		{
			Vector2 key = new Vector2(x / 64, y / 64);
			foreach (Furniture item in furniture)
			{
				if (item.boundingBox.Value.Contains(x, y))
				{
					return item;
				}
			}
			if (objects.ContainsKey(key))
			{
				return objects[key];
			}
			return null;
		}

		public Object getObjectAtTile(int x, int y)
		{
			return getObjectAt(x * 64, y * 64);
		}

		private bool onSandyShopPurchase(ISalable item, Farmer who, int amount)
		{
			Game1.player.team.synchronizedShopStock.OnItemPurchased(SynchronizedShopStock.SynchedShop.Sandy, item, amount);
			return false;
		}

		private Dictionary<ISalable, int[]> sandyShopStock()
		{
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			Utility.AddStock(dictionary, new Object(802, 2147483647), (int)(75f * Game1.MasterPlayer.difficultyModifier));
			Utility.AddStock(dictionary, new Object(478, 2147483647));
			Utility.AddStock(dictionary, new Object(486, 2147483647));
			Utility.AddStock(dictionary, new Object(494, 2147483647));
			Utility.AddStock(dictionary, new Object(Vector2.Zero, 196)
			{
				Stack = 2147483647
			});
			switch (Game1.dayOfMonth % 7)
			{
			case 0:
				Utility.AddStock(dictionary, new Object(233, 2147483647));
				break;
			case 1:
				Utility.AddStock(dictionary, new Object(88, 1), 200, 10);
				break;
			case 2:
				Utility.AddStock(dictionary, new Object(90, 2147483647));
				break;
			case 3:
				Utility.AddStock(dictionary, new Object(749, 1), 500, 3);
				break;
			case 4:
				Utility.AddStock(dictionary, new Object(466, 2147483647));
				break;
			case 5:
				Utility.AddStock(dictionary, new Object(340, 2147483647));
				break;
			case 6:
				Utility.AddStock(dictionary, new Object(371, 2147483647), 100);
				break;
			}
			Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			Clothing key = new Clothing(1000 + random.Next(127));
			dictionary.Add(key, new int[2] { 1000, 2147483647 });
			dictionary.Add(new Furniture(2655, Vector2.Zero), new int[2] { 700, 2147483647 });
			switch (Game1.dayOfMonth % 7)
			{
			case 0:
				dictionary.Add(new Furniture(2720, Vector2.Zero), new int[2] { 3000, 2147483647 });
				break;
			case 1:
				dictionary.Add(new Furniture(2802, Vector2.Zero), new int[2] { 2000, 2147483647 });
				break;
			case 2:
				dictionary.Add(new Furniture(2734 + random.Next(4) * 2, Vector2.Zero), new int[2] { 500, 2147483647 });
				break;
			case 3:
				dictionary.Add(new Furniture(2584, Vector2.Zero), new int[2] { 5000, 2147483647 });
				break;
			case 4:
				dictionary.Add(new Furniture(2794, Vector2.Zero), new int[2] { 2500, 2147483647 });
				break;
			case 5:
				dictionary.Add(new Furniture(2784, Vector2.Zero), new int[2] { 2500, 2147483647 });
				break;
			case 6:
				dictionary.Add(new Furniture(2748, Vector2.Zero), new int[2] { 500, 2147483647 });
				dictionary.Add(new Furniture(2812, Vector2.Zero), new int[2] { 500, 2147483647 });
				break;
			}
			Game1.player.team.synchronizedShopStock.UpdateLocalStockWithSyncedQuanitities(SynchronizedShopStock.SynchedShop.Sandy, dictionary);
			return dictionary;
		}

		public virtual bool saloon(Location tileLocation)
		{
			foreach (NPC character in characters)
			{
				if (character.Name.Equals("Gus"))
				{
					if (character.getTileY() != Game1.player.getTileY() - 1 && character.getTileY() != Game1.player.getTileY() - 2)
					{
						return false;
					}
					character.facePlayer(Game1.player);
					Game1.activeClickableMenu = new ShopMenu(Utility.getSaloonStock(), 0, "Gus", delegate(ISalable item, Farmer farmer, int amount)
					{
						Game1.player.team.synchronizedShopStock.OnItemPurchased(SynchronizedShopStock.SynchedShop.Saloon, item, amount);
						return false;
					});
					return true;
				}
			}
			if (getCharacterFromName("Gus") == null && Game1.IsVisitingIslandToday("Gus"))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_MoneyBox"));
				Game1.afterDialogues = delegate
				{
					Game1.activeClickableMenu = new ShopMenu(Utility.getSaloonStock(), 0, null, delegate(ISalable item, Farmer farmer, int amount)
					{
						Game1.player.team.synchronizedShopStock.OnItemPurchased(SynchronizedShopStock.SynchedShop.Saloon, item, amount);
						return false;
					});
				};
				return true;
			}
			return false;
		}

		private void adventureShop()
		{
			if (Game1.player.itemsLostLastDeath.Count > 0)
			{
				List<Response> list = new List<Response>();
				list.Add(new Response("Shop", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Shop")));
				list.Add(new Response("Recovery", Game1.content.LoadString("Strings\\Locations:AdventureGuild_ItemRecovery")));
				list.Add(new Response("Leave", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Leave")));
				createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:AdventureGuild_Greeting"), list.ToArray(), "adventureGuild");
			}
			else
			{
				Game1.activeClickableMenu = new ShopMenu(Utility.getAdventureShopStock(), 0, "Marlon");
			}
			TutorialManager.Instance.SeenShop(TutorialShopLocation.AdventureGuild);
		}

		public virtual bool carpenters(Location tileLocation)
		{
			if (Game1.player.currentUpgrade == null)
			{
				foreach (NPC character in characters)
				{
					if (!character.Name.Equals("Robin"))
					{
						continue;
					}
					if (Vector2.Distance(character.getTileLocation(), new Vector2(tileLocation.X, tileLocation.Y)) > 3f)
					{
						return false;
					}
					character.faceDirection(2);
					if ((int)Game1.player.daysUntilHouseUpgrade < 0 && !Game1.getFarm().isThereABuildingUnderConstruction())
					{
						List<Response> list = new List<Response>();
						list.Add(new Response("Shop", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Shop")));
						if (Game1.IsMasterGame)
						{
							if ((int)Game1.player.houseUpgradeLevel < 3)
							{
								list.Add(new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_UpgradeHouse")));
							}
							else if ((Game1.MasterPlayer.mailReceived.Contains("ccIsComplete") || Game1.MasterPlayer.mailReceived.Contains("JojaMember") || Game1.MasterPlayer.hasCompletedCommunityCenter()) && (int)(Game1.getLocationFromName("Town") as Town).daysUntilCommunityUpgrade <= 0)
							{
								if (!Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
								{
									list.Add(new Response("CommunityUpgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_CommunityUpgrade")));
								}
								else if (!Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
								{
									list.Add(new Response("CommunityUpgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_CommunityUpgrade")));
								}
							}
						}
						else if ((int)Game1.player.houseUpgradeLevel < 3)
						{
							list.Add(new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_UpgradeCabin")));
						}
						if ((int)Game1.player.houseUpgradeLevel >= 2)
						{
							if (Game1.IsMasterGame)
							{
								list.Add(new Response("Renovate", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_RenovateHouse")));
							}
							else
							{
								list.Add(new Response("Renovate", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_RenovateCabin")));
							}
						}
						list.Add(new Response("Construct", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Construct")));
						list.Add(new Response("Leave", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Leave")));
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu"), list.ToArray(), "carpenter");
					}
					else
					{
						TutorialManager.Instance.SeenShop(TutorialShopLocation.ScienceHouse);
						Game1.activeClickableMenu = new ShopMenu(Utility.getCarpenterStock(), 0, "Robin");
					}
					return true;
				}
				if (getCharacterFromName("Robin") == null && Game1.IsVisitingIslandToday("Robin"))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_MoneyBox"));
					Game1.afterDialogues = delegate
					{
						Game1.activeClickableMenu = new ShopMenu(Utility.getCarpenterStock());
					};
					return true;
				}
				if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Tue"))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_RobinAbsent").Replace('\n', '^'));
					return true;
				}
				return false;
			}
			return false;
		}

		public virtual bool blacksmith(Location tileLocation)
		{
			foreach (NPC character in characters)
			{
				if (!character.Name.Equals("Clint"))
				{
					continue;
				}
				if (!character.getTileLocation().Equals(new Vector2(tileLocation.X, tileLocation.Y - 1)))
				{
					character.getTileLocation().Equals(new Vector2(tileLocation.X - 1, tileLocation.Y - 1));
				}
				character.faceDirection(2);
				if (Game1.player.toolBeingUpgraded.Value == null)
				{
					Response[] answerChoices = ((!Game1.player.hasItemInInventory(535, 1) && !Game1.player.hasItemInInventory(536, 1) && !Game1.player.hasItemInInventory(537, 1) && !Game1.player.hasItemInInventory(749, 1) && !Game1.player.hasItemInInventory(275, 1) && !Game1.player.hasItemInInventory(791, 1)) ? new Response[3]
					{
						new Response("Shop", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Shop")),
						new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Upgrade")),
						new Response("Leave", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Leave"))
					} : new Response[4]
					{
						new Response("Shop", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Shop")),
						new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Upgrade")),
						new Response("Process", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Geodes")),
						new Response("Leave", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Leave"))
					});
					createQuestionDialogue("", answerChoices, "Blacksmith");
				}
				else if ((int)Game1.player.daysLeftForToolUpgrade <= 0)
				{
					if (Game1.player.freeSpotsInInventory() > 0 || Game1.player.toolBeingUpgraded.Value is GenericTool)
					{
						Tool value = Game1.player.toolBeingUpgraded.Value;
						Game1.player.toolBeingUpgraded.Value = null;
						Game1.player.hasReceivedToolUpgradeMessageYet = false;
						Game1.player.holdUpItemThenMessage(value);
						if (value is GenericTool)
						{
							value.actionWhenClaimed();
						}
						else
						{
							Game1.player.addItemToInventoryBool(value);
						}
						if (Game1.player.team.useSeparateWallets.Value && value.UpgradeLevel == 4)
						{
							Game1.multiplayer.globalChatInfoMessage("IridiumToolUpgrade", Game1.player.Name, value.DisplayName);
						}
					}
					else
					{
						Game1.drawDialogue(character, Game1.content.LoadString("Data\\ExtraDialogue:Clint_NoInventorySpace"));
					}
				}
				else
				{
					Game1.drawDialogue(character, Game1.content.LoadString("Data\\ExtraDialogue:Clint_StillWorking", Game1.player.toolBeingUpgraded.Value.DisplayName));
				}
				return true;
			}
			return false;
		}

		public virtual bool animalShop(Location tileLocation)
		{
			foreach (NPC character in characters)
			{
				if (character.Name.Equals("Marnie"))
				{
					if (!character.getTileLocation().Equals(new Vector2(tileLocation.X, tileLocation.Y - 1)))
					{
						return false;
					}
					character.faceDirection(2);
					Response[] answerChoices = new Response[3]
					{
						new Response("Supplies", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Supplies")),
						new Response("Purchase", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Animals")),
						new Response("Leave", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Leave"))
					};
					createQuestionDialogue("", answerChoices, "Marnie");
					return true;
				}
			}
			if (getCharacterFromName("Marnie") == null && Game1.IsVisitingIslandToday("Marnie"))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:AnimalShop_MoneyBox"));
				Game1.afterDialogues = delegate
				{
					Game1.activeClickableMenu = new ShopMenu(Utility.getAnimalShopStock());
				};
				return true;
			}
			if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Tue"))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Absent").Replace('\n', '^'));
				return true;
			}
			return false;
		}

		private void gunther()
		{
			if ((this as LibraryMuseum).doesFarmerHaveAnythingToDonate(Game1.player))
			{
				Response[] answerChoices = (((this as LibraryMuseum).getRewardsForPlayer(Game1.player).Count <= 0) ? new Response[2]
				{
					new Response("Donate", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Donate")),
					new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave"))
				} : new Response[3]
				{
					new Response("Donate", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Donate")),
					new Response("Collect", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Collect")),
					new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave"))
				});
				createQuestionDialogue("", answerChoices, "Museum");
			}
			else if ((this as LibraryMuseum).getRewardsForPlayer(Game1.player).Count > 0)
			{
				createQuestionDialogue("", new Response[2]
				{
					new Response("Collect", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Collect")),
					new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave"))
				}, "Museum");
			}
			else if (Game1.player.achievements.Contains(5))
			{
				Game1.drawDialogue(Game1.getCharacterFromName("Gunther"), Game1.parseText(Game1.content.LoadString("Data\\ExtraDialogue:Gunther_MuseumComplete")));
			}
			else
			{
				Game1.drawDialogue(Game1.getCharacterFromName("Gunther"), Game1.player.mailReceived.Contains("artifactFound") ? Game1.parseText(Game1.content.LoadString("Data\\ExtraDialogue:Gunther_NothingToDonate")) : Game1.content.LoadString("Data\\ExtraDialogue:Gunther_NoArtifactsFound"));
			}
		}

		public void openChest(Location location, int debrisType, int numberOfChunks)
		{
			int[] debrisType2 = new int[1] { debrisType };
			openChest(location, debrisType2, numberOfChunks);
		}

		public void openChest(Location location, int[] debrisType, int numberOfChunks)
		{
			playSound("openBox");
			map.GetLayer("Buildings").Tiles[location.X, location.Y].TileIndex++;
			for (int i = 0; i < debrisType.Length; i++)
			{
				Game1.createDebris(debrisType[i], location.X, location.Y, numberOfChunks);
			}
			map.GetLayer("Buildings").Tiles[location].Properties["Action"] = new PropertyValue("RemoveChest");
		}

		public string actionParamsToString(string[] actionparams)
		{
			string text = actionparams[1];
			for (int i = 2; i < actionparams.Length; i++)
			{
				text = text + " " + actionparams[i];
			}
			return text;
		}

		private void GetLumber()
		{
			if (Game1.player.ActiveObject == null && Game1.player.WoodPieces > 0)
			{
				Game1.player.grabObject(new Object(Vector2.Zero, 30, "Lumber", canBeSetDown: true, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false));
				Game1.player.WoodPieces--;
			}
			else if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.Name.Equals("Lumber"))
			{
				Game1.player.CanMove = false;
				switch (Game1.player.FacingDirection)
				{
				case 2:
					((FarmerSprite)Game1.player.Sprite).animateBackwardsOnce(64, 75f);
					break;
				case 1:
					((FarmerSprite)Game1.player.Sprite).animateBackwardsOnce(72, 75f);
					break;
				case 0:
					((FarmerSprite)Game1.player.Sprite).animateBackwardsOnce(80, 75f);
					break;
				case 3:
					((FarmerSprite)Game1.player.Sprite).animateBackwardsOnce(88, 75f);
					break;
				}
				Game1.player.ActiveObject = null;
				Game1.player.WoodPieces++;
			}
		}

		public void removeTile(Location tileLocation, string layer)
		{
			Map.GetLayer(layer).Tiles[tileLocation.X, tileLocation.Y] = null;
		}

		public void removeTile(int x, int y, string layer)
		{
			Map.GetLayer(layer).Tiles[x, y] = null;
		}

		public void characterTrampleTile(Vector2 tile)
		{
			if (!(this is FarmHouse) && !(this is IslandFarmHouse) && !(this is Farm))
			{
				terrainFeatures.TryGetValue(tile, out var value);
				if (value != null && value is Tree && (int)(value as Tree).growthStage < 1 && (value as Tree).instantDestroy(tile, this))
				{
					terrainFeatures.Remove(tile);
				}
			}
		}

		public bool characterDestroyObjectWithinRectangle(Microsoft.Xna.Framework.Rectangle rect, bool showDestroyedObject)
		{
			if (this is FarmHouse || this is IslandFarmHouse)
			{
				return false;
			}
			foreach (Farmer farmer in farmers)
			{
				if (rect.Intersects(farmer.GetBoundingBox()))
				{
					return false;
				}
			}
			Vector2 vector = new Vector2(rect.X / 64, rect.Y / 64);
			objects.TryGetValue(vector, out var value);
			if (checkDestroyItem(value, vector, showDestroyedObject))
			{
				return true;
			}
			terrainFeatures.TryGetValue(vector, out var value2);
			if (checkDestroyTerrainFeature(value2, vector))
			{
				return true;
			}
			vector.X = rect.Right / 64;
			objects.TryGetValue(vector, out value);
			if (checkDestroyItem(value, vector, showDestroyedObject))
			{
				return true;
			}
			terrainFeatures.TryGetValue(vector, out value2);
			if (checkDestroyTerrainFeature(value2, vector))
			{
				return true;
			}
			vector.X = rect.X / 64;
			vector.Y = rect.Bottom / 64;
			objects.TryGetValue(vector, out value);
			if (checkDestroyItem(value, vector, showDestroyedObject))
			{
				return true;
			}
			terrainFeatures.TryGetValue(vector, out value2);
			if (checkDestroyTerrainFeature(value2, vector))
			{
				return true;
			}
			vector.X = rect.Right / 64;
			objects.TryGetValue(vector, out value);
			if (checkDestroyItem(value, vector, showDestroyedObject))
			{
				return true;
			}
			terrainFeatures.TryGetValue(vector, out value2);
			if (checkDestroyTerrainFeature(value2, vector))
			{
				return true;
			}
			return false;
		}

		private bool checkDestroyTerrainFeature(TerrainFeature tf, Vector2 tilePositionToTry)
		{
			if (tf != null && tf is Tree && (tf as Tree).instantDestroy(tilePositionToTry, this))
			{
				terrainFeatures.Remove(tilePositionToTry);
			}
			return false;
		}

		private bool checkDestroyItem(Object o, Vector2 tilePositionToTry, bool showDestroyedObject)
		{
			if (o != null && !o.IsHoeDirt && !o.isPassable() && !map.GetLayer("Back").Tiles[(int)tilePositionToTry.X, (int)tilePositionToTry.Y].Properties.ContainsKey("NPCBarrier"))
			{
				if (!o.IsHoeDirt)
				{
					if (o.IsSpawnedObject)
					{
						numberOfSpawnedObjectsOnMap--;
					}
					if (showDestroyedObject && !o.bigCraftable)
					{
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(o.ParentSheetIndex, 150f, 1, 3, new Vector2(tilePositionToTry.X * 64f, tilePositionToTry.Y * 64f), flicker: false, o.flipped)
						{
							alphaFade = 0.01f
						});
					}
					o.performToolAction(null, this);
					if (objects.ContainsKey(tilePositionToTry))
					{
						if (o is Chest)
						{
							Chest chest = o as Chest;
							if ((chest.SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin || chest.SpecialChestType == Chest.SpecialChestTypes.JunimoChest) && chest.MoveToSafePosition(this, tilePositionToTry))
							{
								return true;
							}
							chest.destroyAndDropContents(tilePositionToTry * 64f, this);
						}
						objects.Remove(tilePositionToTry);
					}
				}
				return true;
			}
			return false;
		}

		public Object removeObject(Vector2 location, bool showDestroyedObject)
		{
			objects.TryGetValue(location, out var value);
			if (value != null && (value.CanBeGrabbed || showDestroyedObject))
			{
				if (value.IsSpawnedObject)
				{
					numberOfSpawnedObjectsOnMap--;
				}
				Object @object = objects[location];
				objects.Remove(location);
				if (showDestroyedObject)
				{
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(@object.Type.Equals("Crafting") ? @object.ParentSheetIndex : (@object.ParentSheetIndex + 1), 150f, 1, 3, new Vector2(location.X * 64f, location.Y * 64f), flicker: true, @object.bigCraftable, @object.flipped));
				}
				if (value.Name.Contains("Weed"))
				{
					Game1.stats.WeedsEliminated++;
				}
				return @object;
			}
			return null;
		}

		public void removeTileProperty(int tileX, int tileY, string layer, string key)
		{
			try
			{
				if (map == null)
				{
					return;
				}
				Layer layer2 = map.GetLayer(layer);
				if (layer2 == null)
				{
					return;
				}
				Tile tile = layer2.Tiles[tileX, tileY];
				if (tile != null)
				{
					IPropertyCollection properties = tile.Properties;
					if (properties.ContainsKey(key))
					{
						properties.Remove(key);
					}
				}
			}
			catch (Exception)
			{
			}
		}

		public void setTileProperty(int tileX, int tileY, string layer, string key, string value)
		{
			try
			{
				if (map == null)
				{
					return;
				}
				Layer layer2 = map.GetLayer(layer);
				if (layer2 == null)
				{
					return;
				}
				Tile tile = layer2.Tiles[tileX, tileY];
				if (tile != null)
				{
					IPropertyCollection properties = tile.Properties;
					if (!properties.ContainsKey(key))
					{
						properties.Add(key, new PropertyValue(value));
					}
					else
					{
						properties[key] = value;
					}
				}
			}
			catch (Exception)
			{
			}
		}

		private void removeDirt(Vector2 location)
		{
			objects.TryGetValue(location, out var value);
			if (value != null && value.IsHoeDirt)
			{
				objects.Remove(location);
			}
		}

		public void removeBatch(List<Vector2> locations)
		{
			foreach (Vector2 location in locations)
			{
				objects.Remove(location);
			}
		}

		public void setObjectAt(float x, float y, Object o)
		{
			Vector2 key = new Vector2(x, y);
			if (objects.ContainsKey(key))
			{
				objects[key] = o;
			}
			else
			{
				objects.Add(key, o);
			}
		}

		public virtual void cleanupBeforeSave()
		{
			for (int num = characters.Count - 1; num >= 0; num--)
			{
				if (characters[num] is Junimo)
				{
					characters.RemoveAt(num);
				}
			}
			if (name.Equals("WitchHut"))
			{
				characters.Clear();
			}
		}

		public virtual void cleanupForVacancy()
		{
			int num = 0;
			while (num < this.debris.Count)
			{
				Debris debris = this.debris[num];
				if (debris.isEssentialItem() && Game1.IsMasterGame && debris.collect(Game1.player))
				{
					this.debris.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
		}

		public virtual void cleanupBeforePlayerExit()
		{
			int num = 0;
			while (num < this.debris.Count)
			{
				Debris debris = this.debris[num];
				if (debris.isEssentialItem() && debris.player.Value != null && debris.player.Value == Game1.player && debris.collect(debris.player))
				{
					this.debris.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
			Game1.currentLightSources.Clear();
			if (critters != null)
			{
				critters.Clear();
			}
			for (int num2 = Game1.onScreenMenus.Count - 1; num2 >= 0; num2--)
			{
				IClickableMenu clickableMenu = Game1.onScreenMenus[num2];
				if (clickableMenu.destroy)
				{
					Game1.onScreenMenus.RemoveAt(num2);
					if (clickableMenu is IDisposable)
					{
						(clickableMenu as IDisposable).Dispose();
					}
				}
			}
			if (Game1.getMusicTrackName() == "sam_acoustic1")
			{
				Game1.changeMusicTrack("none");
			}
			bool flag = Game1.locationRequest == null || Game1.locationRequest.Location == null || Game1.locationRequest.Location.IsOutdoors;
			if (!Game1.getMusicTrackName().Contains(Game1.currentSeason) && !Game1.getMusicTrackName().Contains("night_ambient") && !Game1.getMusicTrackName().Contains("day_ambient") && !Game1.getMusicTrackName().Equals("rain") && !Game1.eventUp && (bool)isOutdoors && flag && GetLocationContext() == LocationContext.Default)
			{
				Game1.changeMusicTrack("none");
			}
			AmbientLocationSounds.onLocationLeave();
			if ((name.Equals("AnimalShop") || name.Equals("ScienceHouse")) && Game1.getMusicTrackName().Equals("marnieShop") && flag)
			{
				Game1.changeMusicTrack("none");
			}
			if (name.Equals("Saloon") && (Game1.getMusicTrackName().Contains("Saloon") || Game1.startedJukeboxMusic))
			{
				Game1.changeMusicTrack("none");
			}
			if (name.Equals("LeahHouse"))
			{
				Game1.changeMusicTrack("none");
			}
			if (name.Equals("ElliottHouse"))
			{
				Game1.changeMusicTrack("none");
			}
			if (name.Equals("IslandSouthEastCave"))
			{
				Game1.changeMusicTrack("none");
			}
			if (name.Equals("IslandFarmCave"))
			{
				Game1.changeMusicTrack("none");
			}
			if (name.Equals("IslandNorthCave1"))
			{
				Game1.changeMusicTrack("none");
			}
			if (name.Equals("IslandWestCave1"))
			{
				Game1.changeMusicTrack("none");
			}
			if (name.Equals("IslandFieldOffice"))
			{
				Game1.changeMusicTrack("none");
			}
			if (this is LibraryMuseum || this is JojaMart)
			{
				Game1.changeMusicTrack("none");
			}
			Game1.startedJukeboxMusic = false;
			if (name.Equals("Hospital") && Game1.getMusicTrackName().Equals("distantBanjo") && flag)
			{
				Game1.changeMusicTrack("none");
			}
			if (Game1.player.rightRing.Value != null)
			{
				Game1.player.rightRing.Value.onLeaveLocation(Game1.player, this);
			}
			if (Game1.player.leftRing.Value != null)
			{
				Game1.player.leftRing.Value.onLeaveLocation(Game1.player, this);
			}
			if (Game1.locationRequest == null || name != Game1.locationRequest.Name)
			{
				foreach (NPC character in characters)
				{
					if (character.uniqueSpriteActive)
					{
						character.Sprite.LoadTexture("Characters\\" + NPC.getTextureNameForCharacter(character.Name));
						character.uniqueSpriteActive = false;
					}
					if (character.uniquePortraitActive)
					{
						character.Portrait = Game1.content.Load<Texture2D>("Portraits\\" + NPC.getTextureNameForCharacter(character.Name));
						character.uniquePortraitActive = false;
					}
				}
			}
			if (name.Equals("AbandonedJojaMart"))
			{
				if (farmers.Count <= 1)
				{
					for (int num3 = characters.Count - 1; num3 >= 0; num3--)
					{
						if (characters[num3] is Junimo)
						{
							characters.RemoveAt(num3);
						}
					}
				}
				Game1.changeMusicTrack("none");
			}
			furnitureToRemove.Clear();
			Game1.stopMusicTrack(Game1.MusicContext.SubLocation);
			interiorDoors.CleanUpLocalState();
			tapToMove.Reset();
			Game1.temporaryContent.Unload();
			Utility.CollectGarbage();
		}

		public static int getWeedForSeason(Random r, string season)
		{
			switch (season)
			{
			case "spring":
				if (!(r.NextDouble() < 0.33))
				{
					if (!(r.NextDouble() < 0.5))
					{
						return 675;
					}
					return 674;
				}
				return 784;
			case " summer":
				if (!(r.NextDouble() < 0.33))
				{
					if (!(r.NextDouble() < 0.5))
					{
						return 677;
					}
					return 676;
				}
				return 785;
			case "fall":
				if (!(r.NextDouble() < 0.33))
				{
					if (!(r.NextDouble() < 0.5))
					{
						return 679;
					}
					return 678;
				}
				return 786;
			default:
				return 674;
			}
		}

		private void startSleep()
		{
			Game1.player.timeWentToBed.Value = Game1.timeOfDay;
			if (Game1.IsMultiplayer)
			{
				Game1.player.team.SetLocalReady("sleep", ready: true);
				Game1.dialogueUp = false;
				Game1.activeClickableMenu = new ReadyCheckDialog("sleep", allowCancel: true, delegate
				{
					doSleep();
				}, delegate(Farmer who)
				{
					if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ReadyCheckDialog)
					{
						(Game1.activeClickableMenu as ReadyCheckDialog).closeDialog(who);
					}
					who.timeWentToBed.Value = 0;
				});
			}
			else
			{
				doSleep();
			}
			if (Game1.player.team.announcedSleepingFarmers.Contains(Game1.player))
			{
				return;
			}
			Game1.player.team.announcedSleepingFarmers.Add(Game1.player);
			if (!Game1.IsMultiplayer || ((FarmerTeam.SleepAnnounceModes)Game1.player.team.sleepAnnounceMode != 0 && ((FarmerTeam.SleepAnnounceModes)Game1.player.team.sleepAnnounceMode != FarmerTeam.SleepAnnounceModes.First || Game1.player.team.announcedSleepingFarmers.Count() != 1)))
			{
				return;
			}
			string text = "GoneToBed";
			if (Game1.random.NextDouble() < 0.75)
			{
				if (Game1.timeOfDay < 1800)
				{
					text += "Early";
				}
				else if (Game1.timeOfDay > 2530)
				{
					text += "Late";
				}
			}
			int num = 0;
			for (int i = 0; i < 2; i++)
			{
				if (Game1.random.NextDouble() < 0.25)
				{
					num++;
				}
			}
			Game1.multiplayer.globalChatInfoMessage(text + num, Game1.player.displayName);
		}

		private void doSleep()
		{
			if ((float)lightLevel == 0f && Game1.timeOfDay < 2000)
			{
				lightLevel.Value = 0.6f;
				localSound("openBox");
				if (Game1.IsMasterGame)
				{
					Game1.NewDay(600f);
				}
			}
			else if ((float)lightLevel > 0f && Game1.timeOfDay >= 2000)
			{
				lightLevel.Value = 0f;
				localSound("openBox");
				if (Game1.IsMasterGame)
				{
					Game1.NewDay(600f);
				}
			}
			else if (Game1.IsMasterGame)
			{
				Game1.NewDay(0f);
			}
			Game1.player.lastSleepLocation.Value = Game1.currentLocation.NameOrUniqueName;
			Game1.player.lastSleepPoint.Value = Game1.player.getTileLocationPoint();
			Game1.player.mostRecentBed = Game1.player.Position;
			Game1.player.doEmote(24);
			Game1.player.freezePause = 2000;
		}

		public virtual bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (questionParams != null && questionParams.Length != 0 && questionParams[0] == "Minecart")
			{
				Game1.currentLocation.tapToMove.preventMountingHorse = false;
			}
			if (questionAndAnswer != null)
			{
				Vector2 tileLocation;
				Vector2[] adjacentTilesOffsets;
				switch (questionAndAnswer.Length)
				{
				case 27:
				{
					char c = questionAndAnswer[0];
					if (c != 'E')
					{
						if (c != 'e' || !(questionAndAnswer == "evilShrineRightActivate_Yes"))
						{
							break;
						}
						if (Game1.player.removeItemsFromInventory(203, 1))
						{
							Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(780f, 388f), flipped: false, 0f, Color.White)
							{
								interval = 50f,
								totalNumberOfLoops = 99999,
								animationLength = 7,
								layerDepth = 0.038500004f,
								scale = 4f
							});
							playSound("fireball");
							DelayedAction.playSoundAfterDelay("batScreech", 500, this);
							for (int j = 0; j < 20; j++)
							{
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(12f, 6f) * 64f + new Vector2(Game1.random.Next(-32, 64), Game1.random.Next(16)), flipped: false, 0.002f, Color.DarkSlateBlue)
								{
									alpha = 0.75f,
									motion = new Vector2(-0.1f, -0.5f),
									acceleration = new Vector2(-0.002f, 0f),
									interval = 99999f,
									layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
									scale = 3f,
									scaleChange = 0.01f,
									rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
									delayBeforeAnimationStart = j * 60
								});
							}
							Game1.player.freezePause = 1501;
							for (int k = 0; k < 28; k++)
							{
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(540, 347, 13, 13), 50f, 4, 9999, new Vector2(12f, 5f) * 64f, flicker: false, flipped: true, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
								{
									delayBeforeAnimationStart = 500 + k * 25,
									motion = new Vector2(Game1.random.Next(1, 5) * ((!(Game1.random.NextDouble() < 0.5)) ? 1 : (-1)), Game1.random.Next(1, 5) * ((!(Game1.random.NextDouble() < 0.5)) ? 1 : (-1)))
								});
							}
							Game1.spawnMonstersAtNight = true;
							Game1.multiplayer.globalChatInfoMessage("MonstersActivated", Game1.player.name);
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_NoOffering"));
						}
					}
					else
					{
						if (!(questionAndAnswer == "EnterTheaterSpendTicket_Yes"))
						{
							break;
						}
						Game1.player.removeItemsFromInventory(809, 1);
						Rumble.rumble(0.15f, 200f);
						Game1.player.completelyStopAnimatingOrDoingAction();
						playSoundAt("doorClose", Game1.player.getTileLocation());
						Game1.warpFarmer("MovieTheater", 13, 15, 0);
					}
					goto IL_3375;
				}
				case 16:
					switch (questionAndAnswer[0])
					{
					case 'E':
						break;
					case 'W':
						goto IL_056a;
					case 'C':
						goto IL_057f;
					case 'B':
						goto IL_0594;
					case 't':
						goto IL_05a9;
					default:
						goto end_IL_003d;
					}
					if (!(questionAndAnswer == "EnterTheater_Yes"))
					{
						break;
					}
					Rumble.rumble(0.15f, 200f);
					Game1.player.completelyStopAnimatingOrDoingAction();
					playSoundAt("doorClose", Game1.player.getTileLocation());
					Game1.warpFarmer("MovieTheater", 13, 15, 0);
					goto IL_3375;
				case 13:
				{
					char c = questionAndAnswer[0];
					if ((uint)c <= 77u)
					{
						if (c == 'C')
						{
							if (!(questionAndAnswer == "ClubCard_Yes."))
							{
								break;
							}
							goto IL_2b3e;
						}
						if (c != 'M' || !(questionAndAnswer == "Minecart_Town"))
						{
							break;
						}
						Game1.player.Halt();
						Game1.player.freezePause = 700;
						Game1.warpFarmer("Town", 105, 80, 1);
					}
					else if (c != 'S')
					{
						if (c != 'd' || !(questionAndAnswer == "dogStatue_Yes"))
						{
							break;
						}
						if (Game1.player.Money < 10000)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
						}
						else
						{
							List<Response> list = new List<Response>();
							if (canRespec(0))
							{
								list.Add(new Response("farming", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11604")));
							}
							if (canRespec(3))
							{
								list.Add(new Response("mining", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11605")));
							}
							if (canRespec(2))
							{
								list.Add(new Response("foraging", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11606")));
							}
							if (canRespec(1))
							{
								list.Add(new Response("fishing", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11607")));
							}
							if (canRespec(4))
							{
								list.Add(new Response("combat", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11608")));
							}
							list.Add(new Response("cancel", Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueCancel")));
							createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueQuestion"), list.ToArray(), "professionForget");
						}
					}
					else
					{
						if (!(questionAndAnswer == "Smelt_Iridium"))
						{
							break;
						}
						Game1.player.IridiumPieces -= 10;
						smeltBar(new Object(Vector2.Zero, 337, "Iridium Bar", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false), 1440);
					}
					goto IL_3375;
				}
				case 24:
				{
					char c = questionAndAnswer[18];
					if ((uint)c <= 101u)
					{
						if (c != 'a')
						{
							if (c != 'e' || !(questionAndAnswer == "telephone_AdventureGuild"))
							{
								break;
							}
							playShopPhoneNumberSounds(questionAndAnswer);
							Game1.player.freezePause = 4950;
							DelayedAction.functionAfterDelay(delegate
							{
								Game1.playSound("bigSelect");
								NPC character = Game1.getCharacterFromName("Marlon");
								string text3 = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
								if (Game1.player.mailForTomorrow.Contains("MarlonRecovery"))
								{
									Game1.drawDialogue(character, Game1.content.LoadString("Strings\\Characters:Phone_Marlon_AlreadyRecovering"));
								}
								else
								{
									Game1.drawDialogue(character, Game1.content.LoadString("Strings\\Characters:Phone_Marlon_Open"));
									Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
									{
										if (Game1.player.itemsLostLastDeath.Count > 0)
										{
											Game1.player.forceCanMove();
											Game1.activeClickableMenu = new ShopMenu(Utility.getAdventureRecoveryStock(), 0, "Marlon_Recovery");
										}
										else
										{
											Game1.drawDialogue(character, Game1.content.LoadString("Strings\\Characters:Phone_Marlon_NoDeathItems"));
										}
									});
								}
							}, 4950);
						}
						else
						{
							if (!(questionAndAnswer == "professionForget_farming"))
							{
								break;
							}
							if (Game1.player.newLevels.Contains(new Point(0, 5)) || Game1.player.newLevels.Contains(new Point(0, 10)))
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
							}
							else
							{
								Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
								RemoveProfession(0);
								RemoveProfession(1);
								RemoveProfession(3);
								RemoveProfession(5);
								RemoveProfession(2);
								RemoveProfession(4);
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
								int num8 = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[0]);
								if (num8 >= 5)
								{
									Game1.player.newLevels.Add(new Point(0, 5));
								}
								if (num8 >= 10)
								{
									Game1.player.newLevels.Add(new Point(0, 10));
								}
								DelayedAction.playSoundAfterDelay("dog_bark", 300);
								DelayedAction.playSoundAfterDelay("dog_bark", 900);
							}
						}
					}
					else if (c != 'i')
					{
						if (c != 'o' || !(questionAndAnswer == "specialCharmQuestion_Yes"))
						{
							break;
						}
						if (Game1.player.hasItemInInventory(446, 1))
						{
							Game1.player.holdUpItemThenMessage(new SpecialItem(3));
							Game1.player.removeFirstOfThisItemFromInventory(446);
							Game1.player.hasSpecialCharm = true;
							Game1.player.mailReceived.Add("SecretNote20_done");
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_specialCharmNoFoot"));
						}
					}
					else
					{
						if (!(questionAndAnswer == "professionForget_fishing"))
						{
							break;
						}
						if (Game1.player.newLevels.Contains(new Point(1, 5)) || Game1.player.newLevels.Contains(new Point(1, 10)))
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
						}
						else
						{
							Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
							RemoveProfession(8);
							RemoveProfession(11);
							RemoveProfession(10);
							RemoveProfession(6);
							RemoveProfession(9);
							RemoveProfession(7);
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
							int num9 = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[1]);
							if (num9 >= 5)
							{
								Game1.player.newLevels.Add(new Point(1, 5));
							}
							if (num9 >= 10)
							{
								Game1.player.newLevels.Add(new Point(1, 10));
							}
							DelayedAction.playSoundAfterDelay("dog_bark", 300);
							DelayedAction.playSoundAfterDelay("dog_bark", 900);
						}
					}
					goto IL_3375;
				}
				case 23:
				{
					char c = questionAndAnswer[19];
					if ((uint)c <= 109u)
					{
						if (c != '_')
						{
							if (c != 'm' || !(questionAndAnswer == "professionForget_combat"))
							{
								break;
							}
							if (Game1.player.newLevels.Contains(new Point(4, 5)) || Game1.player.newLevels.Contains(new Point(4, 10)))
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
							}
							else
							{
								Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
								RemoveProfession(26);
								RemoveProfession(27);
								RemoveProfession(29);
								RemoveProfession(25);
								RemoveProfession(28);
								RemoveProfession(24);
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
								int num = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[4]);
								if (num >= 5)
								{
									Game1.player.newLevels.Add(new Point(4, 5));
								}
								if (num >= 10)
								{
									Game1.player.newLevels.Add(new Point(4, 10));
								}
								DelayedAction.playSoundAfterDelay("dog_bark", 300);
								DelayedAction.playSoundAfterDelay("dog_bark", 900);
							}
						}
						else
						{
							if (!(questionAndAnswer == "RemoveIncubatingEgg_Yes"))
							{
								break;
							}
							Game1.player.ActiveObject = new Object(Vector2.Zero, (this as AnimalHouse).incubatingEgg.Y, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
							Game1.player.showCarrying();
							(this as AnimalHouse).incubatingEgg.Y = -1;
							map.GetLayer("Front").Tiles[1, 2].TileIndex = 45;
						}
					}
					else if (c != 'n')
					{
						if (c != 'v' || !(questionAndAnswer == "adventureGuild_Recovery"))
						{
							break;
						}
						Game1.player.forceCanMove();
						Game1.activeClickableMenu = new ShopMenu(Utility.getAdventureRecoveryStock(), 0, "Marlon_Recovery");
					}
					else
					{
						if (!(questionAndAnswer == "professionForget_mining"))
						{
							break;
						}
						if (Game1.player.newLevels.Contains(new Point(3, 5)) || Game1.player.newLevels.Contains(new Point(3, 10)))
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
						}
						else
						{
							Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
							RemoveProfession(23);
							RemoveProfession(21);
							RemoveProfession(18);
							RemoveProfession(19);
							RemoveProfession(22);
							RemoveProfession(20);
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
							int num2 = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[3]);
							if (num2 >= 5)
							{
								Game1.player.newLevels.Add(new Point(3, 5));
							}
							if (num2 >= 10)
							{
								Game1.player.newLevels.Add(new Point(3, 10));
							}
							DelayedAction.playSoundAfterDelay("dog_bark", 300);
							DelayedAction.playSoundAfterDelay("dog_bark", 900);
						}
					}
					goto IL_3375;
				}
				case 18:
				{
					char c = questionAndAnswer[14];
					if ((uint)c <= 95u)
					{
						if (c != 'G')
						{
							if (c != 'S')
							{
								if (c != '_' || !(questionAndAnswer == "evilShrineLeft_Yes"))
								{
									break;
								}
								if (Game1.player.removeItemsFromInventory(74, 1))
								{
									Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(156f, 388f), flipped: false, 0f, Color.White)
									{
										interval = 50f,
										totalNumberOfLoops = 99999,
										animationLength = 7,
										layerDepth = 0.038500004f,
										scale = 4f
									});
									for (int num4 = 0; num4 < 20; num4++)
									{
										Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(2f, 6f) * 64f + new Vector2(Game1.random.Next(-32, 64), Game1.random.Next(16)), flipped: false, 0.002f, Color.LightGray)
										{
											alpha = 0.75f,
											motion = new Vector2(1f, -0.5f),
											acceleration = new Vector2(-0.002f, 0f),
											interval = 99999f,
											layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
											scale = 3f,
											scaleChange = 0.01f,
											rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
											delayBeforeAnimationStart = num4 * 25
										});
									}
									playSound("fireball");
									Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(2f, 5f) * 64f, flicker: false, flipped: true, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
									{
										motion = new Vector2(4f, -2f)
									});
									if (Game1.player.getChildrenCount() > 1)
									{
										Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(2f, 5f) * 64f, flicker: false, flipped: true, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
										{
											motion = new Vector2(4f, -1.5f),
											delayBeforeAnimationStart = 50
										});
									}
									string text2 = "";
									foreach (Child child in Game1.player.getChildren())
									{
										text2 += Game1.content.LoadString("Strings\\Locations:WitchHut_Goodbye", child.getName());
									}
									Game1.showGlobalMessage(text2);
									Game1.player.getRidOfChildren();
									Game1.multiplayer.globalChatInfoMessage("EvilShrine", Game1.player.name);
								}
								else
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_NoOffering"));
								}
							}
							else
							{
								if (!(questionAndAnswer == "telephone_SeedShop"))
								{
									break;
								}
								playShopPhoneNumberSounds(questionAndAnswer);
								Game1.player.freezePause = 4950;
								DelayedAction.functionAfterDelay(delegate
								{
									Game1.playSound("bigSelect");
									NPC characterFromName3 = Game1.getCharacterFromName("Pierre");
									string text4 = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
									if (AreStoresClosedForFestival())
									{
										Game1.drawDialogue(characterFromName3, Game1.content.LoadString("Strings\\Characters:Phone_Pierre_Festival"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
									}
									else if ((Game1.isLocationAccessible("CommunityCenter") || text4 != "Wed") && Game1.timeOfDay >= 900 && Game1.timeOfDay < 1700)
									{
										Game1.drawDialogue(characterFromName3, Game1.content.LoadString("Strings\\Characters:Phone_Pierre_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : "")));
									}
									else
									{
										Game1.drawDialogue(characterFromName3, Game1.content.LoadString("Strings\\Characters:Phone_Pierre_Closed"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
									}
									Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
									{
										List<Response> list2 = new List<Response>
										{
											new Response("SeedShop_CheckSeedStock", Game1.content.LoadString("Strings\\Characters:Phone_CheckSeedStock")),
											new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp"))
										};
										Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), list2.ToArray(), "telephone");
									});
								}, 4950);
							}
						}
						else
						{
							if (!(questionAndAnswer == "CowboyGame_NewGame"))
							{
								break;
							}
							Game1.player.jotpkProgress.Value = null;
							Game1.currentMinigame = new AbigailGame();
						}
					}
					else if (c != 'c')
					{
						if (c != 'r')
						{
							if (c != 'v' || !(questionAndAnswer == "carpenter_Renovate"))
							{
								break;
							}
							Game1.player.forceCanMove();
							HouseRenovation.ShowRenovationMenu();
						}
						else
						{
							if (!(questionAndAnswer == "Blacksmith_Upgrade"))
							{
								break;
							}
							Game1.activeClickableMenu = new ShopMenu(Utility.getBlacksmithUpgradeStock(Game1.player), 0, "ClintUpgrade");
						}
					}
					else
					{
						if (!(questionAndAnswer == "Blacksmith_Process"))
						{
							break;
						}
						Game1.activeClickableMenu = new GeodeMenu();
					}
					goto IL_3375;
				}
				case 20:
					switch (questionAndAnswer[10])
					{
					case 'C':
						break;
					case 'p':
						goto IL_0772;
					case 'm':
						goto IL_0787;
					case 'A':
						goto IL_079c;
					case 'B':
						goto IL_07b1;
					default:
						goto end_IL_003d;
					}
					if (!(questionAndAnswer == "evilShrineCenter_Yes"))
					{
						break;
					}
					if (Game1.player.Money >= 30000)
					{
						Game1.player.Money -= 30000;
						Game1.player.wipeExMemories();
						Game1.multiplayer.globalChatInfoMessage("EvilShrine", Game1.player.name);
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(468f, 328f), flipped: false, 0f, Color.White)
						{
							interval = 50f,
							totalNumberOfLoops = 99999,
							animationLength = 7,
							layerDepth = 0.038500004f,
							scale = 4f
						});
						playSound("fireball");
						DelayedAction.playSoundAfterDelay("debuffHit", 500, this);
						int num5 = 0;
						Game1.player.faceDirection(2);
						Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
						{
							new FarmerSprite.AnimationFrame(94, 1500),
							new FarmerSprite.AnimationFrame(0, 1)
						});
						Game1.player.freezePause = 1500;
						Game1.player.jitterStrength = 1f;
						for (int num6 = 0; num6 < 20; num6++)
						{
							Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(7f, 5f) * 64f + new Vector2(Game1.random.Next(-32, 64), Game1.random.Next(16)), flipped: false, 0.002f, Color.SlateGray)
							{
								alpha = 0.75f,
								motion = new Vector2(0f, -0.5f),
								acceleration = new Vector2(-0.002f, 0f),
								interval = 99999f,
								layerDepth = 0.032f + (float)Game1.random.Next(100) / 10000f,
								scale = 3f,
								scaleChange = 0.01f,
								rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
								delayBeforeAnimationStart = num6 * 25
							});
						}
						for (int num7 = 0; num7 < 16; num7++)
						{
							foreach (Vector2 item in Utility.getBorderOfThisRectangle(Utility.getRectangleCenteredAt(new Vector2(7f, 5f), 2 + num7 * 2)))
							{
								if (num5 % 2 == 0)
								{
									Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(692, 1853, 4, 4), 25f, 1, 16, item * 64f + new Vector2(32f, 32f), flicker: false, flipped: false)
									{
										layerDepth = 1f,
										delayBeforeAnimationStart = num7 * 50,
										scale = 4f,
										scaleChange = 1f,
										color = new Color(255 - Utility.getRedToGreenLerpColor(1f / (float)(num7 + 1)).R, 255 - Utility.getRedToGreenLerpColor(1f / (float)(num7 + 1)).G, 255 - Utility.getRedToGreenLerpColor(1f / (float)(num7 + 1)).B),
										acceleration = new Vector2(-0.1f, 0f)
									});
								}
								num5++;
							}
						}
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_NoOffering"));
					}
					goto IL_3375;
				case 29:
				{
					char c = questionAndAnswer[20];
					if (c != 'H')
					{
						if (c != 'S')
						{
							if (c != 'i' || !(questionAndAnswer == "evilShrineRightDeActivate_Yes"))
							{
								break;
							}
							if (Game1.player.removeItemsFromInventory(203, 1))
							{
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(780f, 388f), flipped: false, 0f, Color.White)
								{
									interval = 50f,
									totalNumberOfLoops = 99999,
									animationLength = 7,
									layerDepth = 0.038500004f,
									scale = 4f
								});
								playSound("fireball");
								for (int l = 0; l < 20; l++)
								{
									Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(12f, 6f) * 64f + new Vector2(Game1.random.Next(-32, 64), Game1.random.Next(16)), flipped: false, 0.002f, Color.DarkSlateBlue)
									{
										alpha = 0.75f,
										motion = new Vector2(0f, -0.5f),
										acceleration = new Vector2(-0.002f, 0f),
										interval = 99999f,
										layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
										scale = 3f,
										scaleChange = 0.01f,
										rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
										delayBeforeAnimationStart = l * 25
									});
								}
								Game1.spawnMonstersAtNight = false;
								Game1.multiplayer.globalChatInfoMessage("MonstersDeActivated", Game1.player.name);
							}
							else
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_NoOffering"));
							}
						}
						else
						{
							if (!(questionAndAnswer == "telephone_Carpenter_ShopStock"))
							{
								break;
							}
							Game1.activeClickableMenu = new ShopMenu(Utility.getCarpenterStock());
							if (Game1.activeClickableMenu is ShopMenu)
							{
								ShopMenu shopMenu3 = Game1.activeClickableMenu as ShopMenu;
								shopMenu3.readOnly = true;
								shopMenu3.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(shopMenu3.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
								{
									answerDialogueAction("HangUp", new string[0]);
								});
							}
						}
					}
					else
					{
						if (!(questionAndAnswer == "telephone_Carpenter_HouseCost"))
						{
							break;
						}
						NPC characterFromName = Game1.getCharacterFromName("Robin");
						string text = Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse" + ((int)Game1.player.houseUpgradeLevel + 1));
						if (text.Contains('.'))
						{
							text = text.Substring(0, text.LastIndexOf('.') + 1);
						}
						else if (text.Contains('。'))
						{
							text = text.Substring(0, text.LastIndexOf('。') + 1);
						}
						Game1.drawDialogue(characterFromName, text, Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
						Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
						{
							answerDialogueAction("HangUp", new string[0]);
						});
					}
					goto IL_3375;
				}
				case 15:
				{
					char c = questionAndAnswer[7];
					if ((uint)c <= 95u)
					{
						if ((uint)c <= 80u)
						{
							if (c != 'C')
							{
								if (c != 'P' || !(questionAndAnswer == "Marnie_Purchase"))
								{
									break;
								}
								Game1.player.forceCanMove();
								Game1.activeClickableMenu = new PurchaseAnimalsMenu(Utility.getPurchaseAnimalStock());
							}
							else
							{
								if (!(questionAndAnswer == "buyJojaCola_Yes"))
								{
									break;
								}
								if (Game1.player.Money >= 75)
								{
									Game1.player.Money -= 75;
									Game1.player.addItemByMenuIfNecessary(new Object(167, 1));
								}
								else
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
								}
							}
						}
						else if (c != 'S')
						{
							if (c != '_' || !(questionAndAnswer == "taxvote_Against"))
							{
								break;
							}
							Game1.addMailForTomorrow("taxRejected");
							currentEvent.currentCommand++;
						}
						else
						{
							if (!(questionAndAnswer == "Marnie_Supplies"))
							{
								break;
							}
							Game1.activeClickableMenu = new ShopMenu(Utility.getAnimalShopStock(), 0, "Marnie");
						}
					}
					else if ((uint)c <= 100u)
					{
						if (c != 'a')
						{
							if (c != 'd' || !(questionAndAnswer == "ClubCard_That's"))
							{
								break;
							}
							goto IL_2b3e;
						}
						if (!(questionAndAnswer == "CalicoJack_Play"))
						{
							break;
						}
						if (Game1.player.clubCoins >= 100)
						{
							Game1.currentMinigame = new CalicoJack();
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_NotEnoughCoins"));
						}
					}
					else if (c != 'i')
					{
						if (c != 'l')
						{
							if (c != 't' || !(questionAndAnswer == "Minecart_Quarry"))
							{
								break;
							}
							Game1.player.Halt();
							Game1.player.freezePause = 700;
							Game1.warpFarmer("Mountain", 124, 12, 2);
						}
						else
						{
							if (!(questionAndAnswer == "ClubSeller_I'll"))
							{
								break;
							}
							if (Game1.player.Money >= 1000000)
							{
								Game1.player.Money -= 1000000;
								Game1.exitActiveMenu();
								Game1.player.forceCanMove();
								Game1.player.addItemByMenuIfNecessaryElseHoldUp(new Object(Vector2.Zero, 127));
							}
							else
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Club_ClubSeller_NotEnoughMoney"));
							}
						}
					}
					else if (!(questionAndAnswer == "Blacksmith_Shop"))
					{
						if (!(questionAndAnswer == "ExitToTitle_Yes"))
						{
							break;
						}
						Game1.fadeScreenToBlack();
						Game1.exitToTitle = true;
					}
					else
					{
						Game1.activeClickableMenu = new ShopMenu(Utility.getBlacksmithStock(), 0, "Clint");
					}
					goto IL_3375;
				}
				case 17:
				{
					char c = questionAndAnswer[0];
					if (c != 'B')
					{
						if (c != 'C')
						{
							if (c != 'c' || !(questionAndAnswer == "carpenter_Upgrade"))
							{
								break;
							}
							houseUpgradeOffer();
						}
						else
						{
							if (!(questionAndAnswer == "CalicoJackHS_Play"))
							{
								break;
							}
							if (Game1.player.clubCoins >= 1000)
							{
								Game1.currentMinigame = new CalicoJack(-1, highStakes: true);
							}
							else
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Club_CalicoJackHS_NotEnoughCoins"));
							}
						}
					}
					else
					{
						if (!(questionAndAnswer == "Backpack_Purchase"))
						{
							break;
						}
						if ((int)Game1.player.maxItems == 12 && Game1.player.Money >= 2000)
						{
							Game1.player.Money -= 2000;
							Game1.player.maxItems.Value += 12;
							for (int m = 0; m < (int)Game1.player.maxItems; m++)
							{
								if (Game1.player.items.Count <= m)
								{
									Game1.player.items.Add(null);
								}
							}
							Game1.player.holdUpItemThenMessage(new SpecialItem(99, Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8708")));
							Game1.multiplayer.globalChatInfoMessage("BackpackLarge", Game1.player.Name);
						}
						else if ((int)Game1.player.maxItems < 36 && Game1.player.Money >= 10000)
						{
							Game1.player.Money -= 10000;
							Game1.player.maxItems.Value += 12;
							Game1.player.holdUpItemThenMessage(new SpecialItem(99, Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8709")));
							for (int n = 0; n < (int)Game1.player.maxItems; n++)
							{
								if (Game1.player.items.Count <= n)
								{
									Game1.player.items.Add(null);
								}
							}
							Game1.multiplayer.globalChatInfoMessage("BackpackDeluxe", Game1.player.Name);
						}
						else if ((int)Game1.player.maxItems != 36)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney2"));
						}
					}
					goto IL_3375;
				}
				case 14:
					switch (questionAndAnswer[0])
					{
					case 'B':
						break;
					case 'c':
						goto IL_0926;
					case 'E':
						goto IL_093b;
					case 'M':
						goto IL_0950;
					case 'C':
						goto IL_0965;
					default:
						goto end_IL_003d;
					}
					if (!(questionAndAnswer == "BuyQiCoins_Yes"))
					{
						break;
					}
					if (Game1.player.Money >= 1000)
					{
						Game1.player.Money -= 1000;
						localSound("Pickup_Coin15");
						Game1.player.clubCoins += 100;
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8715"));
					}
					goto IL_3375;
				case 10:
					switch (questionAndAnswer[7])
					{
					case 'u':
						break;
					case '_':
						goto IL_098f;
					case 't':
						goto IL_09a4;
					case 'r':
						goto IL_09b9;
					case 'o':
						goto IL_09ce;
					default:
						goto end_IL_003d;
					}
					if (!(questionAndAnswer == "Shaft_Jump"))
					{
						break;
					}
					if (this is MineShaft)
					{
						(this as MineShaft).enterMineShaft();
					}
					goto IL_3375;
				case 11:
					switch (questionAndAnswer[0])
					{
					case 'm':
						break;
					case 'u':
						goto IL_09f8;
					case 'M':
						goto IL_0a0d;
					case 'E':
						goto IL_0a32;
					case 'j':
						goto IL_0a47;
					case 'B':
						goto IL_0a5c;
					case 'D':
						goto IL_0a71;
					case 't':
						goto IL_0a86;
					default:
						goto end_IL_003d;
					}
					if (!(questionAndAnswer == "mariner_Buy"))
					{
						break;
					}
					if (Game1.player.Money >= 5000)
					{
						Game1.player.Money -= 5000;
						Game1.player.addItemByMenuIfNecessary(new Object(460, 1)
						{
							specialItem = true
						});
						if (Game1.activeClickableMenu == null)
						{
							Game1.player.holdUpItemThenMessage(new Object(460, 1));
						}
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
					}
					goto IL_3375;
				case 19:
				{
					char c = questionAndAnswer[0];
					if ((uint)c <= 97u)
					{
						if (c != 'C')
						{
							if (c != 'a' || !(questionAndAnswer == "adventureGuild_Shop"))
							{
								break;
							}
							Game1.player.forceCanMove();
							Game1.activeClickableMenu = new ShopMenu(Utility.getAdventureShopStock(), 0, "Marlon");
						}
						else
						{
							if (!(questionAndAnswer == "CowboyGame_Continue"))
							{
								break;
							}
							Game1.currentMinigame = new AbigailGame();
						}
					}
					else if (c != 'c')
					{
						if (c != 't' || !(questionAndAnswer == "telephone_Carpenter"))
						{
							break;
						}
						playShopPhoneNumberSounds(questionAndAnswer);
						Game1.player.freezePause = 4950;
						DelayedAction.functionAfterDelay(delegate
						{
							Game1.playSound("bigSelect");
							NPC characterFromName6 = Game1.getCharacterFromName("Robin");
							if (AreStoresClosedForFestival())
							{
								Game1.drawDialogue(characterFromName6, Game1.content.LoadString("Strings\\Characters:Phone_Robin_Festival"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
							}
							else if (Game1.getLocationFromName("Town") is Town town && town.daysUntilCommunityUpgrade.Value > 0)
							{
								int value3 = town.daysUntilCommunityUpgrade.Value;
								if (value3 == 1)
								{
									Game1.drawDialogue(characterFromName6, Game1.content.LoadString("Strings\\Characters:Phone_Robin_Working_OneDay"));
								}
								else
								{
									Game1.drawDialogue(characterFromName6, Game1.content.LoadString("Strings\\Characters:Phone_Robin_Working", value3));
								}
							}
							else if (Game1.getFarm().isThereABuildingUnderConstruction())
							{
								Building buildingUnderConstruction = Game1.getFarm().getBuildingUnderConstruction();
								int num10 = 0;
								if (buildingUnderConstruction != null)
								{
									num10 = buildingUnderConstruction.daysUntilUpgrade;
								}
								if (num10 == 1)
								{
									Game1.drawDialogue(characterFromName6, Game1.content.LoadString("Strings\\Characters:Phone_Robin_Working_OneDay"));
								}
								else
								{
									Game1.drawDialogue(characterFromName6, Game1.content.LoadString("Strings\\Characters:Phone_Robin_Working", num10));
								}
							}
							else
							{
								string value4 = characterFromName6.dayScheduleName.Value;
								if (!(value4 == "summer_18"))
								{
									if (value4 == "Tue")
									{
										Game1.drawDialogue(characterFromName6, Game1.content.LoadString("Strings\\Characters:Phone_Robin_Workout"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
									}
									else if (Game1.timeOfDay >= 900 && Game1.timeOfDay < 1700)
									{
										Game1.drawDialogue(characterFromName6, Game1.content.LoadString("Strings\\Characters:Phone_Robin_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : "")));
									}
									else
									{
										Game1.drawDialogue(characterFromName6, Game1.content.LoadString("Strings\\Characters:Phone_Robin_Closed"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
									}
								}
								else
								{
									Game1.drawDialogue(characterFromName6, Game1.content.LoadString("Strings\\Characters:Phone_Robin_Festival"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
								}
							}
							Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
							{
								List<Response> list5 = new List<Response>
								{
									new Response("Carpenter_ShopStock", Game1.content.LoadString("Strings\\Characters:Phone_CheckSeedStock"))
								};
								if ((int)Game1.player.houseUpgradeLevel < 3)
								{
									list5.Add(new Response("Carpenter_HouseCost", Game1.content.LoadString("Strings\\Characters:Phone_CheckHouseCost")));
								}
								list5.Add(new Response("Carpenter_BuildingCost", Game1.content.LoadString("Strings\\Characters:Phone_CheckBuildingCost")));
								list5.Add(new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp")));
								Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), list5.ToArray(), "telephone");
							});
						}, 4950);
					}
					else
					{
						if (!(questionAndAnswer == "carpenter_Construct"))
						{
							break;
						}
						Game1.activeClickableMenu = new CarpenterMenu();
					}
					goto IL_3375;
				}
				case 7:
				{
					char c = questionAndAnswer[0];
					if (c != 'E')
					{
						if (c != 'M' || !(questionAndAnswer == "Mine_No"))
						{
							break;
						}
						Response[] answerChoices = new Response[2]
						{
							new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")),
							new Response("Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes"))
						};
						createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Mines_ResetMine")), answerChoices, "ResetMine");
					}
					else
					{
						if (!(questionAndAnswer == "Eat_Yes"))
						{
							break;
						}
						Game1.player.isEating = false;
						Game1.player.eatHeldObject();
						Game1.currentLocation.tapToMove.Reset();
					}
					goto IL_3375;
				}
				case 9:
				{
					char c = questionAndAnswer[0];
					if (c != 'Q')
					{
						if (c != 'S' || !(questionAndAnswer == "Sleep_Yes"))
						{
							break;
						}
						startSleep();
					}
					else
					{
						if (!(questionAndAnswer == "Quest_Yes"))
						{
							break;
						}
						Game1.questOfTheDay.dailyQuest.Value = true;
						Game1.questOfTheDay.accept();
						Game1.currentBillboard = 0;
						Game1.player.questLog.Add(Game1.questOfTheDay);
					}
					goto IL_3375;
				}
				case 8:
				{
					char c = questionAndAnswer[0];
					if (c != 'M')
					{
						if (c != 'Q' || !(questionAndAnswer == "Quest_No"))
						{
							break;
						}
						Game1.currentBillboard = 0;
					}
					else
					{
						if (!(questionAndAnswer == "Mine_Yes"))
						{
							break;
						}
						if (Game1.CurrentMineLevel > 120)
						{
							Game1.warpFarmer("SkullCave", 3, 4, 2);
						}
						else
						{
							Game1.warpFarmer("UndergroundMine", 16, 16, flip: false);
						}
					}
					goto IL_3375;
				}
				case 12:
					switch (questionAndAnswer[0])
					{
					case 'E':
						break;
					case 'M':
						goto IL_0bac;
					case 'B':
						goto IL_0bc1;
					case 'S':
						goto IL_0bd6;
					case 'F':
						goto IL_0beb;
					default:
						goto end_IL_003d;
					}
					if (!(questionAndAnswer == "ExitMine_Yes"))
					{
						break;
					}
					goto IL_27f6;
				case 21:
				{
					char c = questionAndAnswer[0];
					if (c != 'M')
					{
						if (c != 'S' || !(questionAndAnswer == "ShrineOfChallenge_Yes"))
						{
							break;
						}
						Game1.player.team.toggleMineShrineOvernight.Value = true;
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ChallengeShrine_Activated"));
						Game1.multiplayer.globalChatInfoMessage((!Game1.player.team.mineShrineActivated.Value) ? "HardModeMinesActivated" : "HardModeMinesDeactivated", Game1.player.Name);
						DelayedAction.functionAfterDelay(delegate
						{
							if (!Game1.player.team.mineShrineActivated.Value)
							{
								Game1.playSound("fireball");
								temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(8.75f, 5.8f) * 64f + new Vector2(32f, -32f), flipped: false, 0f, Color.White)
								{
									interval = 50f,
									totalNumberOfLoops = 99999,
									animationLength = 4,
									light = true,
									lightID = 888,
									id = 888f,
									lightRadius = 2f,
									scale = 4f,
									yPeriodic = true,
									lightcolor = new Color(100, 0, 0),
									yPeriodicLoopTime = 1000f,
									yPeriodicRange = 4f,
									layerDepth = 0.04544f
								});
								temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(10.75f, 5.8f) * 64f + new Vector2(32f, -32f), flipped: false, 0f, Color.White)
								{
									interval = 50f,
									totalNumberOfLoops = 99999,
									animationLength = 4,
									light = true,
									lightID = 889,
									id = 889f,
									lightRadius = 2f,
									scale = 4f,
									lightcolor = new Color(100, 0, 0),
									yPeriodic = true,
									yPeriodicLoopTime = 1100f,
									yPeriodicRange = 4f,
									layerDepth = 0.04544f
								});
							}
							else
							{
								removeTemporarySpritesWithID(888);
								removeTemporarySpritesWithID(889);
								Game1.playSound("fireball");
							}
						}, 500);
					}
					else
					{
						if (!(questionAndAnswer == "MinecartGame_Progress"))
						{
							break;
						}
						Game1.currentMinigame = new MineCart(0, 3);
					}
					goto IL_3375;
				}
				case 32:
				{
					char c = questionAndAnswer[10];
					if (c != 'B')
					{
						if (c != 'C' || !(questionAndAnswer == "telephone_Carpenter_BuildingCost"))
						{
							break;
						}
						answerDialogueAction("carpenter_Construct", new string[0]);
						if (Game1.activeClickableMenu is CarpenterMenu carpenterMenu)
						{
							carpenterMenu.readOnly = true;
							carpenterMenu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(carpenterMenu.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
							{
								answerDialogueAction("HangUp", new string[0]);
							});
						}
					}
					else
					{
						if (!(questionAndAnswer == "telephone_Blacksmith_UpgradeCost"))
						{
							break;
						}
						answerDialogueAction("Blacksmith_Upgrade", new string[0]);
						if (Game1.activeClickableMenu is ShopMenu shopMenu2)
						{
							shopMenu2.readOnly = true;
							shopMenu2.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(shopMenu2.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
							{
								answerDialogueAction("HangUp", new string[0]);
							});
						}
					}
					goto IL_3375;
				}
				case 25:
					if (!(questionAndAnswer == "professionForget_foraging"))
					{
						break;
					}
					if (Game1.player.newLevels.Contains(new Point(2, 5)) || Game1.player.newLevels.Contains(new Point(2, 10)))
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
					}
					else
					{
						Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
						RemoveProfession(16);
						RemoveProfession(14);
						RemoveProfession(17);
						RemoveProfession(12);
						RemoveProfession(13);
						RemoveProfession(15);
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
						int num3 = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[2]);
						if (num3 >= 5)
						{
							Game1.player.newLevels.Add(new Point(2, 5));
						}
						if (num3 >= 10)
						{
							Game1.player.newLevels.Add(new Point(2, 10));
						}
						DelayedAction.playSoundAfterDelay("dog_bark", 300);
						DelayedAction.playSoundAfterDelay("dog_bark", 900);
					}
					goto IL_3375;
				case 26:
					if (!(questionAndAnswer == "carpenter_CommunityUpgrade"))
					{
						break;
					}
					communityUpgradeOffer();
					goto IL_3375;
				case 6:
					if (!(questionAndAnswer == "Eat_No"))
					{
						break;
					}
					Game1.player.isEating = false;
					Game1.player.completelyStopAnimatingOrDoingAction();
					Game1.currentLocation.tapToMove.Reset();
					goto IL_3375;
				case 33:
					if (!(questionAndAnswer == "telephone_SeedShop_CheckSeedStock"))
					{
						break;
					}
					if (Game1.getLocationFromName("SeedShop") is SeedShop seedShop)
					{
						Game1.activeClickableMenu = new ShopMenu(seedShop.shopStock(), 0, "Pierre");
						if (Game1.activeClickableMenu is ShopMenu shopMenu)
						{
							shopMenu.readOnly = true;
							shopMenu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(shopMenu.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
							{
								answerDialogueAction("HangUp", new string[0]);
							});
						}
					}
					else
					{
						answerDialogueAction("HangUp", new string[0]);
					}
					goto IL_3375;
				case 38:
					{
						if (!(questionAndAnswer == "telephone_AnimalShop_CheckAnimalPrices"))
						{
							break;
						}
						Game1.activeClickableMenu = new PurchaseAnimalsMenu(Utility.getPurchaseAnimalStock());
						if (Game1.activeClickableMenu is PurchaseAnimalsMenu purchaseAnimalsMenu)
						{
							purchaseAnimalsMenu.readOnly = true;
							purchaseAnimalsMenu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(purchaseAnimalsMenu.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
							{
								answerDialogueAction("HangUp", new string[0]);
							});
						}
						goto IL_3375;
					}
					IL_0a0d:
					if (!(questionAndAnswer == "Mine_Return"))
					{
						if (!(questionAndAnswer == "Mariner_Buy"))
						{
							break;
						}
						if (Game1.player.Money >= 5000)
						{
							Game1.player.Money -= 5000;
							Game1.player.grabObject(new Object(Vector2.Zero, 460, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false));
							return true;
						}
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
					}
					else
					{
						Game1.enterMine(Game1.player.deepestMineLevel);
					}
					goto IL_3375;
					IL_0772:
					if (!(questionAndAnswer == "communityUpgrade_Yes"))
					{
						break;
					}
					communityUpgradeAccept();
					goto IL_3375;
					IL_3375:
					return true;
					IL_0926:
					if (!(questionAndAnswer == "carpenter_Shop"))
					{
						break;
					}
					Game1.player.forceCanMove();
					Game1.activeClickableMenu = new ShopMenu(Utility.getCarpenterStock(), 0, "Robin");
					goto IL_3375;
					IL_09f8:
					if (!(questionAndAnswer == "upgrade_Yes"))
					{
						break;
					}
					houseUpgradeAccept();
					goto IL_3375;
					IL_0965:
					if (!(questionAndAnswer == "ClearHouse_Yes"))
					{
						break;
					}
					tileLocation = Game1.player.getTileLocation();
					adjacentTilesOffsets = Character.AdjacentTilesOffsets;
					foreach (Vector2 vector in adjacentTilesOffsets)
					{
						Vector2 key = tileLocation + vector;
						if (objects.ContainsKey(key))
						{
							objects.Remove(key);
						}
					}
					goto IL_3375;
					IL_056a:
					if (!(questionAndAnswer == "WizardShrine_Yes"))
					{
						break;
					}
					if (Game1.player.Money >= 500)
					{
						Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.Wizard);
						Game1.player.Money -= 500;
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney2"));
					}
					goto IL_3375;
					IL_0950:
					if (!(questionAndAnswer == "Minecart_Mines"))
					{
						break;
					}
					Game1.player.Halt();
					Game1.player.freezePause = 700;
					Game1.warpFarmer("Mine", 13, 9, 1);
					if (Game1.getMusicTrackName() == "springtown")
					{
						Game1.changeMusicTrack("none");
					}
					goto IL_3375;
					IL_09ce:
					if (!(questionAndAnswer == "Smelt_Gold"))
					{
						break;
					}
					Game1.player.GoldPieces -= 10;
					smeltBar(new Object(Vector2.Zero, 336, "Gold Bar", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false), 300);
					goto IL_3375;
					IL_0594:
					if (!(questionAndAnswer == "BuyClubCoins_Yes"))
					{
						break;
					}
					if (Game1.player.Money >= 1000)
					{
						Game1.player.Money -= 1000;
						Game1.player.clubCoins += 10;
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
					}
					goto IL_3375;
					IL_09b9:
					if (!(questionAndAnswer == "Smelt_Iron"))
					{
						break;
					}
					Game1.player.IronPieces -= 10;
					smeltBar(new Object(Vector2.Zero, 335, "Iron Bar", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false), 120);
					goto IL_3375;
					IL_09a4:
					if (!(questionAndAnswer == "Mine_Enter"))
					{
						break;
					}
					Game1.enterMine(1);
					goto IL_3375;
					IL_098f:
					if (!(questionAndAnswer == "Dungeon_Go"))
					{
						break;
					}
					Game1.enterMine(Game1.CurrentMineLevel + 1);
					goto IL_3375;
					IL_057f:
					if (!(questionAndAnswer == "CalicoJack_Rules"))
					{
						break;
					}
					Game1.multipleDialogues(new string[2]
					{
						Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Rules1"),
						Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Rules2")
					});
					goto IL_3375;
					IL_2b3e:
					playSound("explosion");
					Game1.flashAlpha = 5f;
					characters.Remove(getCharacterFromName("Bouncer"));
					if (characters.Count > 0)
					{
						characters[0].faceDirection(1);
						characters[0].setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Sandy_PlayerClubMember"));
						characters[0].doEmote(16);
					}
					Game1.pauseThenMessage(500, Game1.content.LoadString("Strings\\Locations:Club_Bouncer_PlayerClubMember"), showProgressBar: false);
					Game1.player.Halt();
					Game1.getCharacterFromName("Mister Qi").setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:MisterQi_PlayerClubMember"));
					goto IL_3375;
					IL_0a86:
					if (!(questionAndAnswer == "taxvote_For"))
					{
						break;
					}
					Game1.shippingTax = true;
					Game1.addMailForTomorrow("taxPassed");
					currentEvent.currentCommand++;
					goto IL_3375;
					IL_0787:
					if (!(questionAndAnswer == "MinecartGame_Endless"))
					{
						break;
					}
					Game1.currentMinigame = new MineCart(0, 2);
					goto IL_3375;
					IL_079c:
					if (!(questionAndAnswer == "telephone_AnimalShop"))
					{
						break;
					}
					playShopPhoneNumberSounds(questionAndAnswer);
					Game1.player.freezePause = 4950;
					DelayedAction.functionAfterDelay(delegate
					{
						Game1.playSound("bigSelect");
						NPC characterFromName5 = Game1.getCharacterFromName("Marnie");
						string text5 = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
						if (AreStoresClosedForFestival())
						{
							Game1.drawDialogue(characterFromName5, Game1.content.LoadString("Strings\\Characters:Phone_Marnie_ClosedDay"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
						}
						else if (characterFromName5.dayScheduleName.Value == "fall_18" || characterFromName5.dayScheduleName.Value == "winter_18" || characterFromName5.dayScheduleName.Value == "Tue" || characterFromName5.dayScheduleName.Value == "Mon")
						{
							Game1.drawDialogue(characterFromName5, Game1.content.LoadString("Strings\\Characters:Phone_Marnie_ClosedDay"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
						}
						else if (Game1.timeOfDay >= 900 && Game1.timeOfDay < 1600)
						{
							Game1.drawDialogue(characterFromName5, Game1.content.LoadString("Strings\\Characters:Phone_Marnie_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : "")));
						}
						else
						{
							Game1.drawDialogue(characterFromName5, Game1.content.LoadString("Strings\\Characters:Phone_Marnie_Closed"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
						}
						Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
						{
							List<Response> list4 = new List<Response>
							{
								new Response("AnimalShop_CheckAnimalPrices", Game1.content.LoadString("Strings\\Characters:Phone_CheckAnimalPrices")),
								new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp"))
							};
							Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), list4.ToArray(), "telephone");
						});
					}, 4950);
					goto IL_3375;
					IL_0beb:
					if (!(questionAndAnswer == "Flute_Change"))
					{
						break;
					}
					Game1.drawItemNumberSelection("flutePitch", -1);
					goto IL_3375;
					IL_07b1:
					if (!(questionAndAnswer == "telephone_Blacksmith"))
					{
						break;
					}
					playShopPhoneNumberSounds(questionAndAnswer);
					Game1.player.freezePause = 4950;
					DelayedAction.functionAfterDelay(delegate
					{
						Game1.playSound("bigSelect");
						NPC characterFromName4 = Game1.getCharacterFromName("Clint");
						if (AreStoresClosedForFestival())
						{
							Game1.drawDialogue(characterFromName4, Game1.content.LoadString("Strings\\Characters:Phone_Clint_Festival"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
						}
						else if (Game1.player.daysLeftForToolUpgrade.Value > 0)
						{
							int value = Game1.player.daysLeftForToolUpgrade.Value;
							if (value == 1)
							{
								Game1.drawDialogue(characterFromName4, Game1.content.LoadString("Strings\\Characters:Phone_Clint_Working_OneDay"));
							}
							else
							{
								Game1.drawDialogue(characterFromName4, Game1.content.LoadString("Strings\\Characters:Phone_Clint_Working", value));
							}
						}
						else
						{
							string value2 = characterFromName4.dayScheduleName.Value;
							if (!(value2 == "winter_16"))
							{
								if (value2 == "Fri")
								{
									Game1.drawDialogue(characterFromName4, Game1.content.LoadString("Strings\\Characters:Phone_Clint_Festival"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
								}
								else if (Game1.timeOfDay >= 900 && Game1.timeOfDay < 1600)
								{
									Game1.drawDialogue(characterFromName4, Game1.content.LoadString("Strings\\Characters:Phone_Clint_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : "")));
								}
								else
								{
									Game1.drawDialogue(characterFromName4, Game1.content.LoadString("Strings\\Characters:Phone_Clint_Closed"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
								}
							}
							else
							{
								Game1.drawDialogue(characterFromName4, Game1.content.LoadString("Strings\\Characters:Phone_Clint_Festival"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
							}
						}
						Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
						{
							List<Response> list3 = new List<Response>
							{
								new Response("Blacksmith_UpgradeCost", Game1.content.LoadString("Strings\\Characters:Phone_CheckToolCost")),
								new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp"))
							};
							Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), list3.ToArray(), "telephone");
						});
					}, 4950);
					goto IL_3375;
					IL_0bd6:
					if (!(questionAndAnswer == "Smelt_Copper"))
					{
						break;
					}
					Game1.player.CopperPieces -= 10;
					smeltBar(new Object(Vector2.Zero, 334, "Copper Bar", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false), 60);
					goto IL_3375;
					IL_0a5c:
					if (!(questionAndAnswer == "Bouquet_Yes"))
					{
						break;
					}
					if (Game1.player.Money >= 500)
					{
						if (Game1.player.ActiveObject == null)
						{
							Game1.player.Money -= 500;
							Game1.player.grabObject(new Object(Vector2.Zero, 458, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false));
							return true;
						}
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
					}
					goto IL_3375;
					IL_0bc1:
					if (!(questionAndAnswer == "Backpack_Yes"))
					{
						break;
					}
					tryToBuyNewBackpack();
					goto IL_3375;
					IL_093b:
					if (!(questionAndAnswer == "ExitMine_Leave"))
					{
						break;
					}
					goto IL_27f6;
					IL_0bac:
					if (!(questionAndAnswer == "Minecart_Bus"))
					{
						break;
					}
					Game1.player.Halt();
					Game1.player.freezePause = 700;
					Game1.warpFarmer("BusStop", 4, 4, 2);
					goto IL_3375;
					IL_27f6:
					if (Game1.CurrentMineLevel == 77377)
					{
						Game1.warpFarmer("Mine", 67, 10, flip: true);
					}
					else if (Game1.CurrentMineLevel > 120)
					{
						Game1.warpFarmer("SkullCave", 3, 4, 2);
					}
					else
					{
						Game1.warpFarmer("Mine", 23, 8, flip: false);
					}
					Game1.changeMusicTrack("none");
					goto IL_3375;
					IL_0a71:
					if (!(questionAndAnswer == "Drum_Change"))
					{
						break;
					}
					Game1.drawItemNumberSelection("drumTone", -1);
					goto IL_3375;
					IL_0a47:
					if (!(questionAndAnswer == "jukebox_Yes"))
					{
						break;
					}
					Game1.drawItemNumberSelection("jukebox", -1);
					Game1.jukeboxPlaying = true;
					goto IL_3375;
					IL_0a32:
					if (!(questionAndAnswer == "ExitMine_Go"))
					{
						break;
					}
					Game1.enterMine(Game1.CurrentMineLevel - 1);
					goto IL_3375;
					IL_05a9:
					if (!(questionAndAnswer == "telephone_Saloon"))
					{
						if (!(questionAndAnswer == "telephone_HangUp"))
						{
							break;
						}
						playSound("openBox");
					}
					else
					{
						playShopPhoneNumberSounds(questionAndAnswer);
						Game1.player.freezePause = 4950;
						DelayedAction.functionAfterDelay(delegate
						{
							Game1.playSound("bigSelect");
							NPC characterFromName2 = Game1.getCharacterFromName("Gus");
							if (AreStoresClosedForFestival())
							{
								Game1.drawDialogue(characterFromName2, Game1.content.LoadString("Strings\\Characters:Phone_Gus_Festival"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
							}
							else if (Game1.timeOfDay >= 1200 && Game1.timeOfDay < 2400 && (characterFromName2.dayScheduleName.Value != "fall_4" || Game1.timeOfDay >= 1700))
							{
								if (Game1.dishOfTheDay != null)
								{
									Game1.drawDialogue(characterFromName2, Game1.content.LoadString("Strings\\Characters:Phone_Gus_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : ""), Game1.dishOfTheDay.DisplayName));
								}
								else
								{
									Game1.drawDialogue(characterFromName2, Game1.content.LoadString("Strings\\Characters:Phone_Gus_Open_NoDishOfTheDay"));
								}
							}
							else if (Game1.dishOfTheDay != null && Game1.timeOfDay < 2400)
							{
								Game1.drawDialogue(characterFromName2, Game1.content.LoadString("Strings\\Characters:Phone_Gus_Closed", Game1.dishOfTheDay.DisplayName), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
							}
							else
							{
								Game1.drawDialogue(characterFromName2, Game1.content.LoadString("Strings\\Characters:Phone_Gus_Closed_NoDishOfTheDay"), Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine"));
							}
							answerDialogueAction("HangUp", new string[0]);
						}, 4950);
					}
					goto IL_3375;
					end_IL_003d:
					break;
				}
			}
			return false;
		}

		private void playShopPhoneNumberSounds(string whichShop)
		{
			Random random = new Random(whichShop.GetHashCode());
			DelayedAction.playSoundAfterDelay("telephone_dialtone", 495, null, 1200);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1200, null, 1200 + random.Next(-4, 5) * 100);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1370, null, 1200 + random.Next(-4, 5) * 100);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1600, null, 1200 + random.Next(-4, 5) * 100);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1850, null, 1200 + random.Next(-4, 5) * 100);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 2030, null, 1200 + random.Next(-4, 5) * 100);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 2250, null, 1200 + random.Next(-4, 5) * 100);
			DelayedAction.playSoundAfterDelay("telephone_buttonPush", 2410, null, 1200 + random.Next(-4, 5) * 100);
			DelayedAction.playSoundAfterDelay("telephone_ringingInEar", 3150);
		}

		public virtual bool answerDialogue(Response answer)
		{
			string[] array = ((lastQuestionKey != null) ? lastQuestionKey.Split(' ') : null);
			string text = ((array != null) ? (array[0] + "_" + answer.responseKey) : null);
			if (answer.responseKey.Equals("Move"))
			{
				Game1.player.grabObject(actionObjectForQuestionDialogue);
				removeObject(actionObjectForQuestionDialogue.TileLocation, showDestroyedObject: false);
				actionObjectForQuestionDialogue = null;
				return true;
			}
			if (afterQuestion != null)
			{
				afterQuestion(Game1.player, answer.responseKey);
				afterQuestion = null;
				Game1.objectDialoguePortraitPerson = null;
				return true;
			}
			if (text == null)
			{
				return false;
			}
			return answerDialogueAction(text, array);
		}

		public static bool AreStoresClosedForFestival()
		{
			if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
			{
				return Utility.getStartTimeOfFestival() < 1900;
			}
			return false;
		}

		public static void RemoveProfession(int profession)
		{
			if (Game1.player.professions.Contains(profession))
			{
				LevelUpMenu.removeImmediateProfessionPerk(profession);
				Game1.player.professions.Remove(profession);
			}
		}

		public static bool canRespec(int skill_index)
		{
			if (Game1.player.GetUnmodifiedSkillLevel(skill_index) < 5)
			{
				return false;
			}
			if (Game1.player.newLevels.Contains(new Point(skill_index, 5)) || Game1.player.newLevels.Contains(new Point(skill_index, 10)))
			{
				return false;
			}
			return true;
		}

		public void setObject(Vector2 v, Object o)
		{
			if (objects.ContainsKey(v))
			{
				objects[v] = o;
			}
			else
			{
				objects.Add(v, o);
			}
		}

		private void houseUpgradeOffer()
		{
			switch ((int)Game1.player.houseUpgradeLevel)
			{
			case 0:
				createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse1")), createYesNoResponses(), "upgrade");
				break;
			case 1:
				createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse2")), createYesNoResponses(), "upgrade");
				break;
			case 2:
				createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse3")), createYesNoResponses(), "upgrade");
				break;
			}
		}

		private void communityUpgradeOffer()
		{
			if (!Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
			{
				createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_CommunityUpgrade1")), createYesNoResponses(), "communityUpgrade");
				if (!Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgradeAsked"))
				{
					Game1.MasterPlayer.mailReceived.Add("pamHouseUpgradeAsked");
				}
			}
			else if (!Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
			{
				createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_CommunityUpgrade2")), createYesNoResponses(), "communityUpgrade");
			}
		}

		public virtual bool catchOceanCrabPotFishFromThisSpot(int x, int y)
		{
			return false;
		}

		public virtual float getExtraTrashChanceForCrabPot(int x, int y)
		{
			return 0f;
		}

		private void communityUpgradeAccept()
		{
			if (!Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
			{
				if (Game1.player.Money >= 500000 && Game1.player.hasItemInInventory(388, 950))
				{
					Game1.player.Money -= 500000;
					Game1.player.removeItemsFromInventory(388, 950);
					Game1.getCharacterFromName("Robin").setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Robin_PamUpgrade_Accepted"));
					Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
					(Game1.getLocationFromName("Town") as Town).daysUntilCommunityUpgrade.Value = 3;
					Game1.multiplayer.globalChatInfoMessage("CommunityUpgrade", Game1.player.Name);
				}
				else if (Game1.player.Money < 500000)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughWood3"));
				}
			}
			else if (!Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
			{
				if (Game1.player.Money >= 300000)
				{
					Game1.player.Money -= 300000;
					Game1.getCharacterFromName("Robin").setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted"));
					Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
					(Game1.getLocationFromName("Town") as Town).daysUntilCommunityUpgrade.Value = 3;
					Game1.multiplayer.globalChatInfoMessage("CommunityUpgrade", Game1.player.Name);
				}
				else if (Game1.player.Money < 300000)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
				}
			}
		}

		private void houseUpgradeAccept()
		{
			switch ((int)Game1.player.houseUpgradeLevel)
			{
			case 0:
				if (Game1.player.Money >= 10000 && Game1.player.hasItemInInventory(388, 450))
				{
					Game1.player.daysUntilHouseUpgrade.Value = 3;
					Game1.player.Money -= 10000;
					Game1.player.removeItemsFromInventory(388, 450);
					Game1.getCharacterFromName("Robin").setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted"));
					Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
					Game1.multiplayer.globalChatInfoMessage("HouseUpgrade", Game1.player.Name, Lexicon.getPossessivePronoun(Game1.player.isMale));
				}
				else if (Game1.player.Money < 10000)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughWood1"));
				}
				break;
			case 1:
				if (Game1.player.Money >= 50000 && Game1.player.hasItemInInventory(709, 150))
				{
					Game1.player.daysUntilHouseUpgrade.Value = 3;
					Game1.player.Money -= 50000;
					Game1.player.removeItemsFromInventory(709, 150);
					Game1.getCharacterFromName("Robin").setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted"));
					Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
					Game1.multiplayer.globalChatInfoMessage("HouseUpgrade", Game1.player.Name, Lexicon.getPossessivePronoun(Game1.player.isMale));
				}
				else if (Game1.player.Money < 50000)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughWood2"));
				}
				break;
			case 2:
				if (Game1.player.Money >= 100000)
				{
					Game1.player.daysUntilHouseUpgrade.Value = 3;
					Game1.player.Money -= 100000;
					Game1.getCharacterFromName("Robin").setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted"));
					Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
					Game1.multiplayer.globalChatInfoMessage("HouseUpgrade", Game1.player.Name, Lexicon.getPossessivePronoun(Game1.player.isMale));
				}
				else if (Game1.player.Money < 100000)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
				}
				break;
			}
		}

		private void smeltBar(Object bar, int minutesUntilReady)
		{
			Game1.player.CoalPieces--;
			actionObjectForQuestionDialogue.heldObject.Value = bar;
			actionObjectForQuestionDialogue.minutesUntilReady.Value = minutesUntilReady;
			actionObjectForQuestionDialogue.showNextIndex.Value = true;
			actionObjectForQuestionDialogue = null;
			playSound("openBox");
			playSound("furnace");
			Game1.stats.BarsSmelted++;
		}

		public void tryToBuyNewBackpack()
		{
			int num = 0;
			switch (Game1.player.MaxItems)
			{
			case 4:
				num = 3500;
				break;
			case 9:
				num = 7500;
				break;
			case 14:
				num = 15000;
				break;
			}
			if (Game1.player.Money >= num)
			{
				Game1.player.increaseBackpackSize(5);
				Game1.player.Money -= num;
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:Backpack_Bought", Game1.player.MaxItems));
				checkForMapChanges();
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
			}
		}

		public void checkForMapChanges()
		{
			if (name.Equals("SeedShop") && Game1.player.MaxItems == 19)
			{
				map.GetLayer("Front").Tiles[10, 21] = new StaticTile(map.GetLayer("Front"), map.GetTileSheet("TownHouseIndoors"), BlendMode.Alpha, 203);
			}
		}

		public void removeStumpOrBoulder(int tileX, int tileY, Object o)
		{
			List<Vector2> list = new List<Vector2>();
			string text = o.Name;
			if (text != null)
			{
				int length = text.Length;
				if (length != 8)
				{
					if (length == 10)
					{
						switch (text[7])
						{
						case '1':
							break;
						case '2':
							goto IL_00de;
						case '3':
							goto IL_00f0;
						case '4':
							goto IL_0105;
						default:
							goto IL_0236;
						}
						if (text == "Boulder1/4")
						{
							goto IL_011a;
						}
					}
				}
				else
				{
					switch (text[5])
					{
					case '1':
						break;
					case '2':
						goto IL_008d;
					case '3':
						goto IL_00a2;
					case '4':
						goto IL_00b7;
					default:
						goto IL_0236;
					}
					if (text == "Stump1/4")
					{
						goto IL_011a;
					}
				}
			}
			goto IL_0236;
			IL_01ac:
			list.Add(new Vector2(tileX, tileY));
			list.Add(new Vector2(tileX + 1, tileY));
			list.Add(new Vector2(tileX, tileY - 1));
			list.Add(new Vector2(tileX + 1, tileY - 1));
			goto IL_0236;
			IL_008d:
			if (text == "Stump2/4")
			{
				goto IL_0163;
			}
			goto IL_0236;
			IL_0236:
			int num = (o.Name.Contains("Stump") ? 5 : 3);
			if (Game1.currentSeason.Equals("winter"))
			{
				num += 376;
			}
			for (int i = 0; i < list.Count; i++)
			{
				Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(num, Game1.random.Next(150, 400), 1, 3, new Vector2(list[i].X * 64f, list[i].Y * 64f), flicker: true, o.flipped));
			}
			removeBatch(list);
			return;
			IL_0105:
			if (text == "Boulder4/4")
			{
				goto IL_01f2;
			}
			goto IL_0236;
			IL_00f0:
			if (text == "Boulder3/4")
			{
				goto IL_01ac;
			}
			goto IL_0236;
			IL_00de:
			if (text == "Boulder2/4")
			{
				goto IL_0163;
			}
			goto IL_0236;
			IL_011a:
			list.Add(new Vector2(tileX, tileY));
			list.Add(new Vector2(tileX + 1, tileY));
			list.Add(new Vector2(tileX, tileY + 1));
			list.Add(new Vector2(tileX + 1, tileY + 1));
			goto IL_0236;
			IL_0163:
			list.Add(new Vector2(tileX, tileY));
			list.Add(new Vector2(tileX - 1, tileY));
			list.Add(new Vector2(tileX, tileY + 1));
			list.Add(new Vector2(tileX - 1, tileY + 1));
			goto IL_0236;
			IL_00b7:
			if (text == "Stump4/4")
			{
				goto IL_01f2;
			}
			goto IL_0236;
			IL_01f2:
			list.Add(new Vector2(tileX, tileY));
			list.Add(new Vector2(tileX - 1, tileY));
			list.Add(new Vector2(tileX, tileY - 1));
			list.Add(new Vector2(tileX - 1, tileY - 1));
			goto IL_0236;
			IL_00a2:
			if (text == "Stump3/4")
			{
				goto IL_01ac;
			}
			goto IL_0236;
		}

		public void destroyObject(Vector2 tileLocation, Farmer who)
		{
			destroyObject(tileLocation, hardDestroy: false, who);
		}

		public void destroyObject(Vector2 tileLocation, bool hardDestroy, Farmer who)
		{
			if (objects.ContainsKey(tileLocation) && !objects[tileLocation].IsHoeDirt && (int)objects[tileLocation].fragility != 2 && !(objects[tileLocation] is Chest) && (int)objects[tileLocation].parentSheetIndex != 165)
			{
				Object @object = objects[tileLocation];
				bool flag = false;
				if (@object.Type != null && (@object.Type.Equals("Fish") || @object.Type.Equals("Cooking") || @object.Type.Equals("Crafting")))
				{
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(@object.ParentSheetIndex, 150f, 1, 3, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), flicker: true, @object.bigCraftable, @object.flipped));
					flag = true;
				}
				else if (@object.Name.Contains("Stump") || @object.Name.Contains("Boulder"))
				{
					flag = true;
					removeStumpOrBoulder((int)tileLocation.X, (int)tileLocation.Y, @object);
				}
				else if (@object.CanBeGrabbed || hardDestroy)
				{
					flag = true;
				}
				if (this is MineShaft && !@object.Name.Contains("Lumber"))
				{
					flag = true;
				}
				if (@object.Name.Contains("Stone") && !@object.bigCraftable && !(@object is Fence))
				{
					flag = true;
					OnStoneDestroyed(@object.parentSheetIndex, (int)tileLocation.X, (int)tileLocation.Y, who);
				}
				if (flag)
				{
					objects.Remove(tileLocation);
				}
			}
		}

		public LocationContext GetLocationContext()
		{
			if (locationContext == (LocationContext)(-1))
			{
				if (map == null)
				{
					reloadMap();
				}
				locationContext = LocationContext.Default;
				string text = null;
				PropertyValue value = null;
				if (map == null)
				{
					return LocationContext.Default;
				}
				text = ((!map.Properties.TryGetValue("LocationContext", out value)) ? "" : value.ToString());
				if (text != "" && !Enum.TryParse<LocationContext>(text, out locationContext))
				{
					locationContext = LocationContext.Default;
				}
			}
			return locationContext;
		}

		public virtual bool sinkDebris(Debris debris, Vector2 chunkTile, Vector2 chunkPosition)
		{
			if (debris.isEssentialItem())
			{
				return false;
			}
			if ((Debris.DebrisType)debris.debrisType == Debris.DebrisType.OBJECT && (int)debris.chunkType == 74)
			{
				return false;
			}
			if ((Debris.DebrisType)debris.debrisType == Debris.DebrisType.CHUNKS)
			{
				localSound("quickSlosh");
				TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f, 3, 0, chunkPosition, flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0.02f, Color.White, 0.75f, 0.003f, 0f, 0f));
			}
			else
			{
				TemporarySprites.Add(new TemporaryAnimatedSprite(28, 300f, 2, 1, chunkPosition, flicker: false, flipped: false));
				localSound("dropItemInWater");
			}
			return true;
		}

		public virtual bool doesTileSinkDebris(int xTile, int yTile, Debris.DebrisType type)
		{
			if (type == Debris.DebrisType.CHUNKS)
			{
				if (doesTileHaveProperty(xTile, yTile, "Water", "Back") != null)
				{
					return getTileIndexAt(xTile, yTile, "Buildings") == -1;
				}
				return false;
			}
			if (doesTileHaveProperty(xTile, yTile, "Water", "Back") != null && !isTileUpperWaterBorder(getTileIndexAt(xTile, yTile, "Buildings")))
			{
				return doesTileHaveProperty(xTile, yTile, "Passable", "Buildings") == null;
			}
			return false;
		}

		private bool isTileUpperWaterBorder(int index)
		{
			switch (index)
			{
			case 183:
			case 184:
			case 185:
			case 211:
			case 1182:
			case 1183:
			case 1184:
			case 1210:
				return true;
			default:
				return false;
			}
		}

		public virtual bool doesEitherTileOrTileIndexPropertyEqual(int xTile, int yTile, string propertyName, string layerName, string propertyValue)
		{
			PropertyValue value = null;
			if (map != null && map.GetLayer(layerName) != null)
			{
				Tile tile = map.GetLayer(layerName).PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size);
				if (tile != null && tile.TileIndexProperties.TryGetValue(propertyName, out value) && value.ToString() == propertyValue)
				{
					return true;
				}
				if (tile != null && map.GetLayer(layerName).PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size).Properties.TryGetValue(propertyName, out value) && value.ToString() == propertyValue)
				{
					return true;
				}
			}
			return propertyValue == null;
		}

		public virtual string doesTileHaveProperty(int xTile, int yTile, string propertyName, string layerName)
		{
			foreach (Furniture item in furniture)
			{
				if ((float)xTile >= item.tileLocation.X - (float)item.GetAdditionalTilePropertyRadius() && (float)xTile < item.tileLocation.X + (float)item.getTilesWide() + (float)item.GetAdditionalTilePropertyRadius() && (float)yTile >= item.tileLocation.Y - (float)item.GetAdditionalTilePropertyRadius() && (float)yTile < item.tileLocation.Y + (float)item.getTilesHigh() + (float)item.GetAdditionalTilePropertyRadius())
				{
					string property_value = null;
					if (item.DoesTileHaveProperty(xTile, yTile, propertyName, layerName, ref property_value))
					{
						return property_value;
					}
				}
			}
			PropertyValue value = null;
			if (map != null && map.GetLayer(layerName) != null)
			{
				Tile tile = map.GetLayer(layerName).PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size);
				tile?.TileIndexProperties.TryGetValue(propertyName, out value);
				if (value == null && tile != null)
				{
					map.GetLayer(layerName).PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size).Properties.TryGetValue(propertyName, out value);
				}
			}
			return value?.ToString();
		}

		public virtual string doesTileHavePropertyNoNull(int xTile, int yTile, string propertyName, string layerName)
		{
			foreach (Furniture item in furniture)
			{
				if (!((float)xTile >= item.tileLocation.X) || !((float)xTile < item.tileLocation.X + (float)item.getTilesWide()) || !((float)yTile >= item.tileLocation.Y) || !((float)yTile < item.tileLocation.Y + (float)item.getTilesHigh()))
				{
					continue;
				}
				string property_value = null;
				if (item.DoesTileHaveProperty(xTile, yTile, propertyName, layerName, ref property_value))
				{
					if (property_value == null)
					{
						return "";
					}
					return property_value;
				}
			}
			PropertyValue value = null;
			PropertyValue value2 = null;
			if (map != null && map.GetLayer(layerName) != null)
			{
				Tile tile = map.GetLayer(layerName).PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size);
				tile?.TileIndexProperties.TryGetValue(propertyName, out value);
				if (tile != null)
				{
					map.GetLayer(layerName).PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size).Properties.TryGetValue(propertyName, out value2);
				}
				if (value2 != null)
				{
					value = value2;
				}
			}
			if (value != null)
			{
				return value.ToString();
			}
			return "";
		}

		public bool isWaterTile(int xTile, int yTile)
		{
			return doesTileHaveProperty(xTile, yTile, "Water", "Back") != null;
		}

		public bool IsWaterTile(Location location)
		{
			Tile tile = map.GetLayer("Back").PickTile(new Location(location.X * 64, location.Y * 64), Game1.viewport.Size);
			if (tile != null)
			{
				tile.TileIndexProperties.TryGetValue("Water", out var value);
				if (value != null)
				{
					if (doesTileHaveProperty(location.X, location.Y, "Passable", "Buildings") != null)
					{
						return false;
					}
					return true;
				}
			}
			return false;
		}

		public bool isOpenWater(int xTile, int yTile)
		{
			if (!isWaterTile(xTile, yTile))
			{
				return false;
			}
			int tileIndexAt = getTileIndexAt(xTile, yTile, "Buildings");
			if (tileIndexAt != -1)
			{
				bool flag = true;
				string tileSheetIDAt = getTileSheetIDAt(xTile, yTile, "Buildings");
				if (tileSheetIDAt == "outdoors" && (tileIndexAt == 759 || tileIndexAt == 628 || tileIndexAt == 629 || tileIndexAt == 734))
				{
					flag = false;
				}
				if (flag)
				{
					return false;
				}
			}
			return !objects.ContainsKey(new Vector2(xTile, yTile));
		}

		public bool isCropAtTile(int tileX, int tileY)
		{
			Vector2 key = new Vector2(tileX, tileY);
			if (terrainFeatures.ContainsKey(key) && terrainFeatures[key] is HoeDirt)
			{
				return ((HoeDirt)terrainFeatures[key]).crop != null;
			}
			return false;
		}

		public virtual void HandleViewportSizeChange(xTile.Dimensions.Rectangle old_viewport, xTile.Dimensions.Rectangle new_viewport)
		{
		}

		public virtual void UpdateLocationSpecificWeatherDebris()
		{
		}

		public virtual bool dropObject(Object obj, Vector2 dropLocation, xTile.Dimensions.Rectangle viewport, bool initialPlacement, Farmer who = null)
		{
			obj.isSpawnedObject.Value = true;
			Vector2 vector = new Vector2((int)dropLocation.X / 64, (int)dropLocation.Y / 64);
			if (!isTileOnMap(vector) || map.GetLayer("Back").PickTile(new Location((int)dropLocation.X, (int)dropLocation.Y), Game1.viewport.Size) == null || map.GetLayer("Back").Tiles[(int)vector.X, (int)vector.Y].TileIndexProperties.ContainsKey("Unplaceable"))
			{
				return false;
			}
			if ((bool)obj.bigCraftable)
			{
				obj.tileLocation.Value = vector;
				if (!isFarm)
				{
					return false;
				}
				if (!obj.setOutdoors && (bool)isOutdoors)
				{
					return false;
				}
				if (!obj.setIndoors && !isOutdoors)
				{
					return false;
				}
				if (obj.performDropDownAction(who))
				{
					return false;
				}
			}
			else if (obj.Type != null && obj.Type.Equals("Crafting") && obj.performDropDownAction(who))
			{
				obj.CanBeSetDown = false;
			}
			bool flag = isTilePassable(new Location((int)vector.X, (int)vector.Y), viewport) && !isTileOccupiedForPlacement(vector);
			if ((obj.CanBeSetDown || initialPlacement) && flag && !isTileHoeDirt(vector))
			{
				obj.TileLocation = vector;
				if (objects.ContainsKey(vector))
				{
					return false;
				}
				objects.Add(vector, obj);
			}
			else if (doesTileHaveProperty((int)vector.X, (int)vector.Y, "Water", "Back") != null)
			{
				Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(28, 300f, 2, 1, dropLocation, flicker: false, obj.flipped));
				playSound("dropItemInWater");
			}
			else
			{
				if (obj.CanBeSetDown && !flag)
				{
					return false;
				}
				if (obj.ParentSheetIndex >= 0 && obj.Type != null)
				{
					if (obj.Type.Equals("Fish") || obj.Type.Equals("Cooking") || obj.Type.Equals("Crafting"))
					{
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(obj.ParentSheetIndex, 150f, 1, 3, dropLocation, flicker: true, obj.flipped));
					}
					else
					{
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(obj.ParentSheetIndex + 1, 150f, 1, 3, dropLocation, flicker: true, obj.flipped));
					}
				}
			}
			return true;
		}

		private void rumbleAndFade(int milliseconds)
		{
			rumbleAndFadeEvent.Fire(milliseconds);
		}

		private void performRumbleAndFade(int milliseconds)
		{
			if (Game1.currentLocation == this)
			{
				Rumble.rumbleAndFade(1f, milliseconds);
			}
		}

		private void damagePlayers(Microsoft.Xna.Framework.Rectangle area, int damage)
		{
			damagePlayersEvent.Fire(new DamagePlayersEventArg
			{
				Area = area,
				Damage = damage
			});
		}

		private void performDamagePlayers(DamagePlayersEventArg arg)
		{
			if (Game1.player.currentLocation == this && Game1.player.GetBoundingBox().Intersects(arg.Area) && !Game1.player.onBridge.Value)
			{
				Game1.player.takeDamage(arg.Damage, overrideParry: true, null);
			}
		}

		public void explode(Vector2 tileLocation, int radius, Farmer who, bool damageFarmers = true, int damage_amount = -1)
		{
			bool flag = false;
			updateMap();
			Vector2 vector = new Vector2(Math.Min(map.Layers[0].LayerWidth - 1, Math.Max(0f, tileLocation.X - (float)radius)), Math.Min(map.Layers[0].LayerHeight - 1, Math.Max(0f, tileLocation.Y - (float)radius)));
			bool[,] circleOutlineGrid = Game1.getCircleOutlineGrid(radius);
			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle((int)(tileLocation.X - (float)radius) * 64, (int)(tileLocation.Y - (float)radius) * 64, (radius * 2 + 1) * 64, (radius * 2 + 1) * 64);
			if (damage_amount > 0)
			{
				damageMonster(rectangle, damage_amount, damage_amount, isBomb: true, who);
			}
			else
			{
				damageMonster(rectangle, radius * 6, radius * 8, isBomb: true, who);
			}
			List<TemporaryAnimatedSprite> list = new List<TemporaryAnimatedSprite>();
			list.Add(new TemporaryAnimatedSprite(23, 9999f, 6, 1, new Vector2(vector.X * 64f, vector.Y * 64f), flicker: false, Game1.random.NextDouble() < 0.5)
			{
				light = true,
				lightRadius = radius,
				lightcolor = Color.Black,
				alphaFade = 0.03f - (float)radius * 0.003f,
				Parent = this
			});
			rumbleAndFade(300 + radius * 100);
			if (damageFarmers)
			{
				if (damage_amount > 0)
				{
					damagePlayers(rectangle, damage_amount);
				}
				else
				{
					damagePlayers(rectangle, radius * 3);
				}
			}
			for (int num = terrainFeatures.Count() - 1; num >= 0; num--)
			{
				KeyValuePair<Vector2, TerrainFeature> keyValuePair = terrainFeatures.Pairs.ElementAt(num);
				if (keyValuePair.Value.getBoundingBox(keyValuePair.Key).Intersects(rectangle) && keyValuePair.Value.performToolAction(null, radius / 2, keyValuePair.Key, this))
				{
					terrainFeatures.Remove(keyValuePair.Key);
				}
			}
			for (int i = 0; i < radius * 2 + 1; i++)
			{
				for (int j = 0; j < radius * 2 + 1; j++)
				{
					if (i == 0 || j == 0 || i == radius * 2 || j == radius * 2)
					{
						flag = circleOutlineGrid[i, j];
					}
					else if (circleOutlineGrid[i, j])
					{
						flag = !flag;
						if (!flag)
						{
							if (objects.ContainsKey(vector) && objects[vector].onExplosion(who, this))
							{
								destroyObject(vector, who);
							}
							if (Game1.random.NextDouble() < 0.45)
							{
								if (Game1.random.NextDouble() < 0.5)
								{
									list.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(vector.X * 64f, vector.Y * 64f), flicker: false, Game1.random.NextDouble() < 0.5)
									{
										delayBeforeAnimationStart = Game1.random.Next(700)
									});
								}
								else
								{
									list.Add(new TemporaryAnimatedSprite(5, new Vector2(vector.X * 64f, vector.Y * 64f), Color.White, 8, flipped: false, 50f)
									{
										delayBeforeAnimationStart = Game1.random.Next(200),
										scale = (float)Game1.random.Next(5, 15) / 10f
									});
								}
							}
						}
					}
					if (flag)
					{
						explosionAt(vector.X, vector.Y);
						if (objects.ContainsKey(vector) && objects[vector].onExplosion(who, this))
						{
							destroyObject(vector, who);
						}
						if (Game1.random.NextDouble() < 0.45)
						{
							if (Game1.random.NextDouble() < 0.5)
							{
								list.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(vector.X * 64f, vector.Y * 64f), flicker: false, Game1.random.NextDouble() < 0.5)
								{
									delayBeforeAnimationStart = Game1.random.Next(700)
								});
							}
							else
							{
								list.Add(new TemporaryAnimatedSprite(5, new Vector2(vector.X * 64f, vector.Y * 64f), Color.White, 8, flipped: false, 50f)
								{
									delayBeforeAnimationStart = Game1.random.Next(200),
									scale = (float)Game1.random.Next(5, 15) / 10f
								});
							}
						}
						list.Add(new TemporaryAnimatedSprite(6, new Vector2(vector.X * 64f, vector.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, Vector2.Distance(vector, tileLocation) * 20f));
					}
					vector.Y += 1f;
					vector.Y = Math.Min(map.Layers[0].LayerHeight - 1, Math.Max(0f, vector.Y));
				}
				vector.X += 1f;
				vector.Y = Math.Min(map.Layers[0].LayerWidth - 1, Math.Max(0f, vector.X));
				vector.Y = tileLocation.Y - (float)radius;
				vector.Y = Math.Min(map.Layers[0].LayerHeight - 1, Math.Max(0f, vector.Y));
			}
			Game1.multiplayer.broadcastSprites(this, list);
			radius /= 2;
			circleOutlineGrid = Game1.getCircleOutlineGrid(radius);
			vector = new Vector2((int)(tileLocation.X - (float)radius), (int)(tileLocation.Y - (float)radius));
			for (int k = 0; k < radius * 2 + 1; k++)
			{
				for (int l = 0; l < radius * 2 + 1; l++)
				{
					if (k == 0 || l == 0 || k == radius * 2 || l == radius * 2)
					{
						flag = circleOutlineGrid[k, l];
					}
					else if (circleOutlineGrid[k, l])
					{
						flag = !flag;
						if (!flag && !objects.ContainsKey(vector) && Game1.random.NextDouble() < 0.9 && doesTileHaveProperty((int)vector.X, (int)vector.Y, "Diggable", "Back") != null && !isTileHoeDirt(vector))
						{
							checkForBuriedItem((int)vector.X, (int)vector.Y, explosion: true, detectOnly: false, who);
							makeHoeDirt(vector);
						}
					}
					if (flag && !objects.ContainsKey(vector) && Game1.random.NextDouble() < 0.9 && doesTileHaveProperty((int)vector.X, (int)vector.Y, "Diggable", "Back") != null && !isTileHoeDirt(vector))
					{
						checkForBuriedItem((int)vector.X, (int)vector.Y, explosion: true, detectOnly: false, who);
						makeHoeDirt(vector);
					}
					vector.Y += 1f;
					vector.Y = Math.Min(map.Layers[0].LayerHeight - 1, Math.Max(0f, vector.Y));
				}
				vector.X += 1f;
				vector.Y = Math.Min(map.Layers[0].LayerWidth - 1, Math.Max(0f, vector.X));
				vector.Y = tileLocation.Y - (float)radius;
				vector.Y = Math.Min(map.Layers[0].LayerHeight - 1, Math.Max(0f, vector.Y));
			}
		}

		public virtual void explosionAt(float x, float y)
		{
		}

		public void removeTemporarySpritesWithID(int id)
		{
			removeTemporarySpritesWithID((float)id);
		}

		public void removeTemporarySpritesWithID(float id)
		{
			removeTemporarySpritesWithIDEvent.Fire(id);
		}

		public void removeTemporarySpritesWithIDLocal(float id)
		{
			for (int num = temporarySprites.Count - 1; num >= 0; num--)
			{
				if (temporarySprites[num].id == id)
				{
					if (temporarySprites[num].hasLit)
					{
						Utility.removeLightSource(temporarySprites[num].lightID);
					}
					temporarySprites.RemoveAt(num);
				}
			}
		}

		public void makeHoeDirt(Vector2 tileLocation, bool ignoreChecks = false)
		{
			if ((ignoreChecks || (doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Diggable", "Back") != null && !isTileOccupied(tileLocation) && isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y), Game1.viewport))) && (!(this is MineShaft) || (this as MineShaft).getMineArea() != 77377))
			{
				terrainFeatures.Add(tileLocation, new HoeDirt((Game1.IsRainingHere(this) && (bool)isOutdoors && !Name.Equals("Desert")) ? 1 : 0, this));
			}
		}

		public int numberOfObjectsOfType(int index, bool bigCraftable)
		{
			int num = 0;
			foreach (KeyValuePair<Vector2, Object> pair in Objects.Pairs)
			{
				if ((int)pair.Value.parentSheetIndex == index && (bool)pair.Value.bigCraftable == bigCraftable)
				{
					num++;
				}
			}
			return num;
		}

		public void passTimeForObjects(int timeElapsed)
		{
			lock (_objectUpdateList)
			{
				_objectUpdateList.Clear();
				foreach (KeyValuePair<Vector2, Object> pair in objects.Pairs)
				{
					_objectUpdateList.Add(pair);
				}
				for (int num = _objectUpdateList.Count - 1; num >= 0; num--)
				{
					KeyValuePair<Vector2, Object> keyValuePair = _objectUpdateList[num];
					Object value = keyValuePair.Value;
					if (value.minutesElapsed(timeElapsed, this))
					{
						Vector2 key = keyValuePair.Key;
						objects.Remove(key);
					}
				}
				_objectUpdateList.Clear();
			}
		}

		public virtual void performTenMinuteUpdate(int timeOfDay)
		{
			foreach (Furniture item in furniture)
			{
				item.minutesElapsed(10, this);
			}
			for (int i = 0; i < characters.Count; i++)
			{
				NPC nPC = characters[i];
				if (!nPC.IsInvisible)
				{
					nPC.checkSchedule(timeOfDay);
					nPC.performTenMinuteUpdate(timeOfDay, this);
				}
			}
			passTimeForObjects(10);
			if ((bool)isOutdoors)
			{
				Random random = new Random(timeOfDay + (int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed);
				if (Equals(Game1.currentLocation))
				{
					tryToAddCritters(onlyIfOnScreen: true);
				}
				if (Game1.IsMasterGame)
				{
					if (fishSplashPoint.Value.Equals(Point.Zero) && random.NextDouble() < 0.5 && (!(this is Farm) || Game1.whichFarm == 1))
					{
						for (int j = 0; j < 2; j++)
						{
							Point value = new Point(random.Next(0, map.GetLayer("Back").LayerWidth), random.Next(0, map.GetLayer("Back").LayerHeight));
							if (!isOpenWater(value.X, value.Y) || doesTileHaveProperty(value.X, value.Y, "NoFishing", "Back") != null)
							{
								continue;
							}
							int num = FishingRod.distanceToLand(value.X, value.Y, this);
							if (num > 1 && num < 5)
							{
								if (Game1.player.currentLocation.Equals(this))
								{
									playSound("waterSlosh");
								}
								fishSplashPoint.Value = value;
								break;
							}
						}
					}
					else if (!fishSplashPoint.Value.Equals(Point.Zero) && random.NextDouble() < 0.1)
					{
						fishSplashPoint.Value = Point.Zero;
					}
					performOrePanTenMinuteUpdate(random);
				}
			}
			if (Game1.dayOfMonth % 7 == 0 && name.Equals("Saloon") && Game1.timeOfDay >= 1200 && Game1.timeOfDay <= 1500 && NetWorldState.checkAnywhereForWorldStateID("saloonSportsRoom"))
			{
				if (Game1.timeOfDay == 1500)
				{
					removeTemporarySpritesWithID(2400);
				}
				else
				{
					bool flag = Game1.random.NextDouble() < 0.25;
					bool flag2 = Game1.random.NextDouble() < 0.25;
					List<NPC> list = new List<NPC>();
					foreach (NPC character in characters)
					{
						if (character.getTileY() < 12 && character.getTileX() > 26 && Game1.random.NextDouble() < ((flag || flag2) ? 0.66 : 0.25))
						{
							list.Add(character);
						}
					}
					foreach (NPC item2 in list)
					{
						item2.showTextAboveHead(Game1.content.LoadString("Strings\\Characters:Saloon_" + (flag ? "goodEvent" : (flag2 ? "badEvent" : "neutralEvent")) + "_" + Game1.random.Next(5)));
						if (flag && Game1.random.NextDouble() < 0.55)
						{
							item2.jump();
						}
					}
				}
			}
			if (name.Equals("BugLand") && Game1.random.NextDouble() <= 0.2 && Game1.currentLocation.Equals(this))
			{
				characters.Add(new Fly(getRandomTile() * 64f, hard: true));
			}
		}

		public virtual void performOrePanTenMinuteUpdate(Random r)
		{
			if (Game1.MasterPlayer.mailReceived.Contains("ccFishTank") && !(this is Beach) && orePanPoint.Value.Equals(Point.Zero) && r.NextDouble() < 0.5)
			{
				for (int i = 0; i < 6; i++)
				{
					Point point = new Point(r.Next(0, Map.GetLayer("Back").LayerWidth), r.Next(0, Map.GetLayer("Back").LayerHeight));
					if (!isOpenWater(point.X, point.Y))
					{
						continue;
					}
					int num = FishingRod.distanceToLand(point.X, point.Y, this);
					if (num <= 1 && getTileIndexAt(point, "Buildings") == -1)
					{
						if (Game1.player.currentLocation.Equals(this))
						{
							playSound("slosh");
						}
						orePanPoint.Value = point;
						break;
					}
				}
			}
			else if (!orePanPoint.Value.Equals(Point.Zero) && r.NextDouble() < 0.1)
			{
				orePanPoint.Value = Point.Zero;
			}
		}

		public bool dropObject(Object obj)
		{
			return dropObject(obj, obj.TileLocation, Game1.viewport, initialPlacement: false);
		}

		public virtual int getFishingLocation(Vector2 tile)
		{
			return -1;
		}

		public virtual bool IsUsingMagicBait(Farmer who)
		{
			if (who != null && who.CurrentTool != null && who.CurrentTool is FishingRod)
			{
				return (who.CurrentTool as FishingRod).getBaitAttachmentIndex() == 908;
			}
			return false;
		}

		public virtual Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			Object @object = null;
			int num = -1;
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
			bool flag = false;
			bool flag2 = IsUsingMagicBait(who);
			string text = ((locationName == null) ? ((string)name) : locationName);
			if (text == "BeachNightMarket")
			{
				text = "Beach";
			}
			if (name.Equals("WitchSwamp") && !Game1.MasterPlayer.mailReceived.Contains("henchmanGone") && Game1.random.NextDouble() < 0.25 && !Game1.player.hasItemInInventory(308, 1))
			{
				return new Object(308, 1);
			}
			if (dictionary.ContainsKey(text))
			{
				string[] array = dictionary[text].Split('/')[4 + Utility.getSeasonNumber(Game1.currentSeason)].Split(' ');
				if (flag2)
				{
					List<string> list = new List<string>();
					for (int i = 0; i < 4; i++)
					{
						string[] array2 = dictionary[text].Split('/')[4 + i].Split(' ');
						if (array2.Length > 1)
						{
							list.AddRange(dictionary[text].Split('/')[4 + i].Split(' '));
						}
					}
					array = list.ToArray();
				}
				Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
				if (array.Length > 1)
				{
					for (int j = 0; j < array.Length; j += 2)
					{
						dictionary2[array[j]] = array[j + 1];
					}
				}
				string[] array3 = dictionary2.Keys.ToArray();
				Dictionary<int, string> dictionary3 = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
				Utility.Shuffle(Game1.random, array3);
				for (int k = 0; k < array3.Length; k++)
				{
					bool flag3 = true;
					string[] array4 = dictionary3[Convert.ToInt32(array3[k])].Split('/');
					string[] array5 = array4[5].Split(' ');
					int num2 = Convert.ToInt32(dictionary2[array3[k]]);
					if (num2 == -1 || getFishingLocation(who.getTileLocation()) == num2)
					{
						for (int l = 0; l < array5.Length; l += 2)
						{
							if (Game1.timeOfDay >= Convert.ToInt32(array5[l]) && Game1.timeOfDay < Convert.ToInt32(array5[l + 1]))
							{
								flag3 = false;
								break;
							}
						}
					}
					if (!array4[7].Equals("both"))
					{
						if (array4[7].Equals("rainy") && !Game1.IsRainingHere(this))
						{
							flag3 = true;
						}
						else if (array4[7].Equals("sunny") && Game1.IsRainingHere(this))
						{
							flag3 = true;
						}
					}
					if (flag2)
					{
						flag3 = false;
					}
					bool flag4 = who != null && who.CurrentTool != null && who.CurrentTool is FishingRod && (int)who.CurrentTool.upgradeLevel == 1;
					if (Convert.ToInt32(array4[1]) >= 50 && flag4)
					{
						flag3 = true;
					}
					if (who.FishingLevel < Convert.ToInt32(array4[12]))
					{
						flag3 = true;
					}
					if (!flag3)
					{
						double num3 = Convert.ToDouble(array4[10]);
						double num4 = Convert.ToDouble(array4[11]) * num3;
						num3 -= (double)Math.Max(0, Convert.ToInt32(array4[9]) - waterDepth) * num4;
						num3 += (double)((float)who.FishingLevel / 50f);
						if (flag4)
						{
							num3 *= 1.1;
						}
						num3 = Math.Min(num3, 0.8999999761581421);
						if (num3 < 0.25 && who != null && who.CurrentTool != null && who.CurrentTool is FishingRod && (who.CurrentTool as FishingRod).getBobberAttachmentIndex() == 856)
						{
							float num5 = 0.25f;
							float num6 = 0.08f;
							num3 = (double)((num5 - num6) / num5) * num3 + (double)((num5 - num6) / 2f);
						}
						if (Game1.random.NextDouble() <= num3)
						{
							num = Convert.ToInt32(array3[k]);
							break;
						}
					}
				}
			}
			bool flag5 = false;
			if (num == -1)
			{
				num = Game1.random.Next(167, 173);
				flag5 = true;
			}
			if ((who.fishCaught == null || who.fishCaught.Count() == 0) && num >= 152)
			{
				num = 145;
			}
			if (!Game1.isFestival() && Game1.random.NextDouble() <= 0.15 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
			{
				num = 890;
			}
			if (who.currentLocation.HasUnlockedAreaSecretNotes(who) && flag5 && Game1.random.NextDouble() < 0.08)
			{
				Object object2 = tryToCreateUnseenSecretNote(who);
				if (object2 != null)
				{
					return object2;
				}
			}
			@object = new Object(num, 1);
			if (flag)
			{
				@object.scale.X = 1f;
			}
			return @object;
		}

		public virtual bool isActionableTile(int xTile, int yTile, Farmer who)
		{
			bool flag = false;
			string text = doesTileHaveProperty(xTile, yTile, "Action", "Buildings");
			if (text != null)
			{
				flag = true;
				if (text.StartsWith("DropBox"))
				{
					flag = false;
					string[] array = text.Split(' ');
					if (array.Length >= 2 && Game1.player.team.specialOrders != null)
					{
						foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
						{
							if (specialOrder.UsesDropBox(array[1]))
							{
								flag = true;
								break;
							}
						}
					}
				}
				if (text.Contains("Message"))
				{
					if (text.Contains("Speech"))
					{
						Game1.isSpeechAtCurrentCursorTile = true;
					}
					else
					{
						Game1.isInspectionAtCurrentCursorTile = true;
					}
				}
			}
			if (objects.ContainsKey(new Vector2(xTile, yTile)) && objects[new Vector2(xTile, yTile)].isActionable(who))
			{
				flag = true;
			}
			if (!Game1.isFestival() && terrainFeatures.ContainsKey(new Vector2(xTile, yTile)) && terrainFeatures[new Vector2(xTile, yTile)].isActionable())
			{
				flag = true;
			}
			if (flag && !Utility.tileWithinRadiusOfPlayer(xTile, yTile, 1, who))
			{
				Game1.mouseCursorTransparency = 0.5f;
			}
			return flag;
		}

		public virtual void digUpArtifactSpot(int xLocation, int yLocation, Farmer who)
		{
			Random random = new Random(xLocation * 2000 + yLocation + (int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed);
			int num = -1;
			bool flag = who != null && who.CurrentTool != null && who.CurrentTool is Hoe && who.CurrentTool.hasEnchantmentOfType<ArchaeologistEnchantment>();
			string[] array = null;
			foreach (KeyValuePair<int, string> item in Game1.objectInformation)
			{
				array = item.Value.Split('/');
				if (array[3].Contains("Arch"))
				{
					string[] array2 = array[6].Split(' ');
					for (int i = 0; i < array2.Length; i += 2)
					{
						if (array2[i].Equals(name) && random.NextDouble() < (double)((!flag) ? 1 : 2) * Convert.ToDouble(array2[i + 1], CultureInfo.InvariantCulture))
						{
							num = item.Key;
							break;
						}
					}
				}
				if (num != -1)
				{
					break;
				}
			}
			if (random.NextDouble() < 0.2 && !(this is Farm))
			{
				num = 102;
			}
			if (num == 102 && (int)Game1.netWorldState.Value.LostBooksFound >= 21)
			{
				num = 770;
			}
			if (num != -1)
			{
				Game1.createObjectDebris(num, xLocation, yLocation, who.UniqueMultiplayerID);
				who.gainExperience(5, 25);
				return;
			}
			bool flag2 = who != null && who.CurrentTool != null && who.CurrentTool is Hoe && who.CurrentTool.hasEnchantmentOfType<GenerousEnchantment>();
			float num2 = 0.5f;
			if (Game1.GetSeasonForLocation(this).Equals("winter") && random.NextDouble() < 0.5 && !(this is Desert))
			{
				if (random.NextDouble() < 0.4)
				{
					Game1.createObjectDebris(416, xLocation, yLocation, who.UniqueMultiplayerID);
					if (flag2 && random.NextDouble() < (double)num2)
					{
						Game1.createObjectDebris(416, xLocation, yLocation, who.UniqueMultiplayerID);
					}
				}
				else
				{
					Game1.createObjectDebris(412, xLocation, yLocation, who.UniqueMultiplayerID);
					if (flag2 && random.NextDouble() < (double)num2)
					{
						Game1.createObjectDebris(412, xLocation, yLocation, who.UniqueMultiplayerID);
					}
				}
				return;
			}
			if (Game1.random.NextDouble() <= 0.25 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
			{
				Game1.createMultipleObjectDebris(890, xLocation, yLocation, random.Next(2, 6), who.UniqueMultiplayerID, this);
			}
			if (Game1.GetSeasonForLocation(this).Equals("spring") && random.NextDouble() < 0.0625 && !(this is Desert) && !(this is Beach))
			{
				Game1.createMultipleObjectDebris(273, xLocation, yLocation, random.Next(2, 6), who.UniqueMultiplayerID, this);
				if (flag2 && random.NextDouble() < (double)num2)
				{
					Game1.createMultipleObjectDebris(273, xLocation, yLocation, random.Next(2, 6), who.UniqueMultiplayerID, this);
				}
				return;
			}
			if (Game1.random.NextDouble() <= 0.2 && (Game1.MasterPlayer.mailReceived.Contains("guntherBones") || (Game1.player.team.specialOrders.Where((SpecialOrder x) => x.questKey == "Gunther") != null && Game1.player.team.specialOrders.Where((SpecialOrder x) => x.questKey == "Gunther").Count() > 0)))
			{
				Game1.createMultipleObjectDebris(881, xLocation, yLocation, random.Next(2, 6), who.UniqueMultiplayerID, this);
			}
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
			if (!dictionary.ContainsKey(name))
			{
				return;
			}
			string[] array3 = dictionary[name].Split('/')[8].Split(' ');
			if (array3.Length == 0 || array3[0].Equals("-1"))
			{
				return;
			}
			for (int j = 0; j < array3.Length; j += 2)
			{
				if (!(random.NextDouble() <= Convert.ToDouble(array3[j + 1])))
				{
					continue;
				}
				num = Convert.ToInt32(array3[j]);
				if (Game1.objectInformation.ContainsKey(num) && (Game1.objectInformation[num].Split('/')[3].Contains("Arch") || num == 102))
				{
					if (num == 102 && (int)Game1.netWorldState.Value.LostBooksFound >= 21)
					{
						num = 770;
					}
					Game1.createObjectDebris(num, xLocation, yLocation, who.UniqueMultiplayerID);
					break;
				}
				if (num == 330 && HasUnlockedAreaSecretNotes(who) && Game1.random.NextDouble() < 0.11)
				{
					Object @object = tryToCreateUnseenSecretNote(who);
					if (@object != null)
					{
						Game1.createItemDebris(@object, new Vector2((float)xLocation + 0.5f, (float)yLocation + 0.5f) * 64f, -1, this);
						break;
					}
				}
				else if (num == 330 && Game1.stats.DaysPlayed > 28 && Game1.random.NextDouble() < 0.1)
				{
					Game1.createMultipleObjectDebris(688 + Game1.random.Next(3), xLocation, yLocation, 1, who.UniqueMultiplayerID);
				}
				Game1.createMultipleObjectDebris(num, xLocation, yLocation, random.Next(1, 4), who.UniqueMultiplayerID);
				if (flag2 && random.NextDouble() < (double)num2)
				{
					Game1.createMultipleObjectDebris(num, xLocation, yLocation, random.Next(1, 4), who.UniqueMultiplayerID);
				}
				break;
			}
		}

		public virtual string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
		{
			Random random = new Random(xLocation * 2000 + yLocation * 77 + (int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + (int)Game1.stats.DirtHoed);
			string text = doesTileHaveProperty(xLocation, yLocation, "Treasure", "Back");
			if (text != null)
			{
				string[] array = text.Split(' ');
				if (detectOnly)
				{
					return array[0];
				}
				string text2 = array[0];
				if (text2 != null)
				{
					switch (text2.Length)
					{
					case 6:
						switch (text2[0])
						{
						case 'C':
							if (text2 == "Copper")
							{
								Game1.createDebris(0, xLocation, yLocation, Convert.ToInt32(array[1]));
							}
							break;
						case 'O':
							if (text2 == "Object")
							{
								Game1.createObjectDebris(Convert.ToInt32(array[1]), xLocation, yLocation);
								if (Convert.ToInt32(array[1]) == 78)
								{
									Game1.stats.CaveCarrotsFound++;
								}
							}
							break;
						}
						break;
					case 4:
						switch (text2[0])
						{
						case 'C':
							if (text2 == "Coal")
							{
								Game1.createDebris(4, xLocation, yLocation, Convert.ToInt32(array[1]));
							}
							break;
						case 'I':
							if (text2 == "Iron")
							{
								Game1.createDebris(2, xLocation, yLocation, Convert.ToInt32(array[1]));
							}
							break;
						case 'G':
							if (text2 == "Gold")
							{
								Game1.createDebris(6, xLocation, yLocation, Convert.ToInt32(array[1]));
							}
							break;
						case 'A':
							if (text2 == "Arch")
							{
								Game1.createObjectDebris(Convert.ToInt32(array[1]), xLocation, yLocation);
							}
							break;
						}
						break;
					case 5:
						if (text2 == "Coins")
						{
							Game1.createObjectDebris(330, xLocation, yLocation);
						}
						break;
					case 7:
						if (text2 == "Iridium")
						{
							Game1.createDebris(10, xLocation, yLocation, Convert.ToInt32(array[1]));
						}
						break;
					case 10:
						if (text2 == "CaveCarrot")
						{
							Game1.createObjectDebris(78, xLocation, yLocation);
						}
						break;
					}
				}
				map.GetLayer("Back").Tiles[xLocation, yLocation].Properties["Treasure"] = null;
			}
			else
			{
				bool flag = who != null && who.CurrentTool != null && who.CurrentTool is Hoe && who.CurrentTool.hasEnchantmentOfType<GenerousEnchantment>();
				float num = 0.5f;
				if (!isFarm && (bool)isOutdoors && Game1.GetSeasonForLocation(this).Equals("winter") && random.NextDouble() < 0.08 && !explosion && !detectOnly && !(this is Desert))
				{
					Game1.createObjectDebris((random.NextDouble() < 0.5) ? 412 : 416, xLocation, yLocation);
					if (flag && random.NextDouble() < (double)num)
					{
						Game1.createObjectDebris((random.NextDouble() < 0.5) ? 412 : 416, xLocation, yLocation);
					}
					return "";
				}
				if ((bool)isOutdoors && random.NextDouble() < 0.03 && !explosion)
				{
					if (detectOnly)
					{
						map.GetLayer("Back").Tiles[xLocation, yLocation].Properties.Add("Treasure", new PropertyValue("Object " + 330));
						return "Object";
					}
					Game1.createObjectDebris(330, xLocation, yLocation);
					if (flag && random.NextDouble() < (double)num)
					{
						Game1.createObjectDebris(330, xLocation, yLocation);
					}
					return "";
				}
			}
			return "";
		}

		public void setAnimatedMapTile(int tileX, int tileY, int[] animationTileIndexes, long interval, string layer, string action, int whichTileSheet = 0)
		{
			StaticTile[] array = new StaticTile[animationTileIndexes.Count()];
			for (int i = 0; i < animationTileIndexes.Count(); i++)
			{
				array[i] = new StaticTile(map.GetLayer(layer), map.TileSheets[whichTileSheet], BlendMode.Alpha, animationTileIndexes[i]);
			}
			map.GetLayer(layer).Tiles[tileX, tileY] = new AnimatedTile(map.GetLayer(layer), array, interval);
			if (action != null && layer != null && layer.Equals("Buildings"))
			{
				map.GetLayer("Buildings").Tiles[tileX, tileY].Properties.Add("Action", new PropertyValue(action));
			}
		}

		public virtual bool AllowMapModificationsInResetState()
		{
			return false;
		}

		public void setMapTile(int tileX, int tileY, int index, string layer, string action, int whichTileSheet = 0)
		{
			map.GetLayer(layer).Tiles[tileX, tileY] = new StaticTile(map.GetLayer(layer), map.TileSheets[whichTileSheet], BlendMode.Alpha, index);
			if (action != null && layer != null && layer.Equals("Buildings"))
			{
				map.GetLayer("Buildings").Tiles[tileX, tileY].Properties.Add("Action", new PropertyValue(action));
			}
		}

		public void setMapTileIndex(int tileX, int tileY, int index, string layer, int whichTileSheet = 0)
		{
			if (map == null)
			{
				return;
			}
			try
			{
				if (map.GetLayer(layer).Tiles[tileX, tileY] != null)
				{
					if (index == -1)
					{
						map.GetLayer(layer).Tiles[tileX, tileY] = null;
					}
					else
					{
						map.GetLayer(layer).Tiles[tileX, tileY].TileIndex = index;
					}
				}
				else if (index != -1)
				{
					map.GetLayer(layer).Tiles[tileX, tileY] = new StaticTile(map.GetLayer(layer), map.TileSheets[whichTileSheet], BlendMode.Alpha, index);
				}
			}
			catch (Exception)
			{
			}
		}

		public virtual void shiftObjects(int dx, int dy)
		{
			List<KeyValuePair<Vector2, Object>> list = new List<KeyValuePair<Vector2, Object>>(objects.Pairs);
			objects.Clear();
			foreach (KeyValuePair<Vector2, Object> item in list)
			{
				if (item.Value.lightSource != null)
				{
					removeLightSource(item.Value.lightSource.identifier);
				}
				item.Value.tileLocation.Value = new Vector2(item.Key.X + (float)dx, item.Key.Y + (float)dy);
				objects.Add(item.Value.tileLocation, item.Value);
				item.Value.initializeLightSource(item.Value.tileLocation);
			}
		}

		public int getTileIndexAt(Point p, string layer)
		{
			return getTileIndexAt(p.X, p.Y, layer);
		}

		public int getTileIndexAt(int x, int y, string layer)
		{
			Layer layer2 = map.GetLayer(layer);
			if (layer2 != null)
			{
				return layer2.Tiles[x, y]?.TileIndex ?? (-1);
			}
			return -1;
		}

		public string getTileSheetIDAt(int x, int y, string layer)
		{
			if (map.GetLayer(layer) != null)
			{
				Tile tile = map.GetLayer(layer).Tiles[x, y];
				if (tile != null)
				{
					return tile.TileSheet.Id;
				}
				return "";
			}
			return "";
		}

		public void OnStoneDestroyed(int indexOfStone, int x, int y, Farmer who)
		{
			if (who != null && Game1.random.NextDouble() <= 0.02 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
			{
				Game1.createMultipleObjectDebris(890, x, y, 1, who.UniqueMultiplayerID, this);
			}
			if (!Name.StartsWith("UndergroundMine"))
			{
				if (indexOfStone == 343 || indexOfStone == 450)
				{
					Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + x * 2000 + y);
					if (random.NextDouble() < 0.035 && Game1.stats.DaysPlayed > 1)
					{
						Game1.createObjectDebris(535 + ((Game1.stats.DaysPlayed > 60 && random.NextDouble() < 0.2) ? 1 : ((Game1.stats.DaysPlayed > 120 && random.NextDouble() < 0.2) ? 2 : 0)), x, y, who.UniqueMultiplayerID, this);
					}
					if (random.NextDouble() < 0.035 * (double)((!who.professions.Contains(21)) ? 1 : 2) && Game1.stats.DaysPlayed > 1)
					{
						Game1.createObjectDebris(382, x, y, who.UniqueMultiplayerID, this);
					}
					if (random.NextDouble() < 0.01 && Game1.stats.DaysPlayed > 1)
					{
						Game1.createObjectDebris(390, x, y, who.UniqueMultiplayerID, this);
					}
				}
				breakStone(indexOfStone, x, y, who, new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + x * 4000 + y));
			}
			else
			{
				(this as MineShaft).checkStoneForItems(indexOfStone, x, y, who);
			}
		}

		protected virtual bool breakStone(int indexOfStone, int x, int y, Farmer who, Random r)
		{
			int num = 0;
			int num2 = (who.professions.Contains(18) ? 1 : 0);
			if (indexOfStone == 44)
			{
				indexOfStone = Game1.random.Next(1, 8) * 2;
			}
			switch (indexOfStone)
			{
			case 95:
				Game1.createMultipleObjectDebris(909, x, y, num2 + r.Next(1, 3) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 200f)) ? 1 : 0), who.uniqueMultiplayerID, this);
				num = 18;
				break;
			case 843:
			case 844:
				Game1.createMultipleObjectDebris(848, x, y, num2 + r.Next(1, 3) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 200f)) ? 1 : 0), who.uniqueMultiplayerID, this);
				num = 12;
				break;
			case 25:
				Game1.createMultipleObjectDebris(719, x, y, r.Next(2, 5), who.uniqueMultiplayerID, this);
				num = 5;
				if (this is IslandLocation && r.NextDouble() < 0.1)
				{
					Game1.player.team.RequestLimitedNutDrops("MusselStone", this, x * 64, y * 64, 5);
				}
				break;
			case 75:
				Game1.createObjectDebris(535, x, y, who.uniqueMultiplayerID, this);
				num = 8;
				break;
			case 76:
				Game1.createObjectDebris(536, x, y, who.uniqueMultiplayerID, this);
				num = 16;
				break;
			case 77:
				Game1.createObjectDebris(537, x, y, who.uniqueMultiplayerID, this);
				num = 32;
				break;
			case 816:
			case 817:
				if (r.NextDouble() < 0.1)
				{
					Game1.createObjectDebris(823, x, y, who.uniqueMultiplayerID, this);
				}
				else if (r.NextDouble() < 0.015)
				{
					Game1.createObjectDebris(824, x, y, who.uniqueMultiplayerID, this);
				}
				else if (r.NextDouble() < 0.1)
				{
					Game1.createObjectDebris(579 + r.Next(11), x, y, who.uniqueMultiplayerID, this);
				}
				Game1.createMultipleObjectDebris(881, x, y, num2 + r.Next(1, 3) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 100f)) ? 1 : 0), who.uniqueMultiplayerID, this);
				num = 6;
				break;
			case 818:
				Game1.createMultipleObjectDebris(330, x, y, num2 + r.Next(1, 3) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 100f)) ? 1 : 0), who.uniqueMultiplayerID, this);
				num = 6;
				break;
			case 819:
				Game1.createObjectDebris(749, x, y, who.uniqueMultiplayerID, this);
				num = 64;
				break;
			case 8:
				Game1.createObjectDebris(66, x, y, who.uniqueMultiplayerID, this);
				num = 16;
				break;
			case 10:
				Game1.createObjectDebris(68, x, y, who.uniqueMultiplayerID, this);
				num = 16;
				break;
			case 12:
				Game1.createObjectDebris(60, x, y, who.uniqueMultiplayerID, this);
				num = 80;
				break;
			case 14:
				Game1.createObjectDebris(62, x, y, who.uniqueMultiplayerID, this);
				num = 40;
				break;
			case 6:
				Game1.createObjectDebris(70, x, y, who.uniqueMultiplayerID, this);
				num = 40;
				break;
			case 4:
				Game1.createObjectDebris(64, x, y, who.uniqueMultiplayerID, this);
				num = 80;
				break;
			case 2:
				Game1.createObjectDebris(72, x, y, who.uniqueMultiplayerID, this);
				num = 150;
				break;
			case 668:
			case 670:
			case 845:
			case 846:
			case 847:
				Game1.createMultipleObjectDebris(390, x, y, num2 + r.Next(1, 3) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 100f)) ? 1 : 0), who.uniqueMultiplayerID, this);
				num = 3;
				if (r.NextDouble() < 0.08)
				{
					Game1.createMultipleObjectDebris(382, x, y, 1 + num2, who.uniqueMultiplayerID, this);
					num = 4;
				}
				break;
			case 751:
			case 849:
				Game1.createMultipleObjectDebris(378, x, y, num2 + r.Next(1, 4) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 100f)) ? 1 : 0), who.uniqueMultiplayerID, this);
				num = 5;
				Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.Orange * 0.5f, 175, 100));
				break;
			case 290:
			case 850:
				Game1.createMultipleObjectDebris(380, x, y, num2 + r.Next(1, 4) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 100f)) ? 1 : 0), who.uniqueMultiplayerID, this);
				num = 12;
				Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.White * 0.5f, 175, 100));
				break;
			case 764:
				Game1.createMultipleObjectDebris(384, x, y, num2 + r.Next(1, 4) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 100f)) ? 1 : 0), who.uniqueMultiplayerID, this);
				num = 18;
				Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.Yellow * 0.5f, 175, 100));
				break;
			case 765:
				Game1.createMultipleObjectDebris(386, x, y, num2 + r.Next(1, 4) + ((r.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)who.MiningLevel / 100f)) ? 1 : 0), who.uniqueMultiplayerID, this);
				num = 50;
				Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 6, Color.BlueViolet * 0.5f, 175, 100));
				if (r.NextDouble() < 0.04)
				{
					Game1.createMultipleObjectDebris(74, x, y, 1);
				}
				num = 50;
				break;
			}
			if (who.professions.Contains(19) && r.NextDouble() < 0.5)
			{
				switch (indexOfStone)
				{
				case 8:
					Game1.createObjectDebris(66, x, y, who.uniqueMultiplayerID, this);
					num = 8;
					break;
				case 10:
					Game1.createObjectDebris(68, x, y, who.uniqueMultiplayerID, this);
					num = 8;
					break;
				case 12:
					Game1.createObjectDebris(60, x, y, who.uniqueMultiplayerID, this);
					num = 50;
					break;
				case 14:
					Game1.createObjectDebris(62, x, y, who.uniqueMultiplayerID, this);
					num = 20;
					break;
				case 6:
					Game1.createObjectDebris(70, x, y, who.uniqueMultiplayerID, this);
					num = 20;
					break;
				case 4:
					Game1.createObjectDebris(64, x, y, who.uniqueMultiplayerID, this);
					num = 50;
					break;
				case 2:
					Game1.createObjectDebris(72, x, y, who.uniqueMultiplayerID, this);
					num = 100;
					break;
				}
			}
			if (indexOfStone == 46)
			{
				Game1.createDebris(10, x, y, r.Next(1, 4), this);
				Game1.createDebris(6, x, y, r.Next(1, 5), this);
				if (r.NextDouble() < 0.25)
				{
					Game1.createMultipleObjectDebris(74, x, y, 1, who.uniqueMultiplayerID, this);
				}
				num = 50;
				Game1.stats.MysticStonesCrushed++;
			}
			if (((bool)isOutdoors || (bool)treatAsOutdoors) && num == 0)
			{
				double num3 = who.DailyLuck / 2.0 + (double)who.MiningLevel * 0.005 + (double)who.LuckLevel * 0.001;
				Random random = new Random(x * 1000 + y + (int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
				Game1.createDebris(14, x, y, 1, this);
				who.gainExperience(3, 1);
				if (who.professions.Contains(21) && random.NextDouble() < 0.05 * (1.0 + num3))
				{
					Game1.createObjectDebris(382, x, y, who.UniqueMultiplayerID, this);
				}
				if (random.NextDouble() < 0.05 * (1.0 + num3))
				{
					int num4 = random.Next(1, 3) + ((random.NextDouble() < 0.1 * (1.0 + num3)) ? 1 : 0);
					Game1.createObjectDebris(382, x, y, who.UniqueMultiplayerID, this);
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(25, new Vector2(64 * x, 64 * y), Color.White, 8, Game1.random.NextDouble() < 0.5, 80f, 0, -1, -1f, 128));
					who.gainExperience(3, 5);
				}
			}
			if (HasUnlockedAreaSecretNotes(who) && r.NextDouble() < 0.0075)
			{
				Object @object = tryToCreateUnseenSecretNote(who);
				if (@object != null)
				{
					Game1.createItemDebris(@object, new Vector2((float)x + 0.5f, (float)y + 0.75f) * 64f, Game1.player.facingDirection, this);
				}
			}
			who.gainExperience(3, num);
			return num > 0;
		}

		public bool isBehindBush(Vector2 Tile)
		{
			if (largeTerrainFeatures != null)
			{
				Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle((int)Tile.X * 64, (int)(Tile.Y + 1f) * 64, 64, 128);
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					if (largeTerrainFeature.getBoundingBox().Intersects(value))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool isBehindTree(Vector2 Tile)
		{
			if (terrainFeatures != null)
			{
				Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle((int)(Tile.X - 1f) * 64, (int)Tile.Y * 64, 192, 256);
				foreach (KeyValuePair<Vector2, TerrainFeature> pair in terrainFeatures.Pairs)
				{
					if (pair.Value is Tree && pair.Value.getBoundingBox(pair.Key).Intersects(value))
					{
						return true;
					}
				}
			}
			return false;
		}

		public virtual void spawnObjects()
		{
			Random random = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed);
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
			if (dictionary.ContainsKey(name))
			{
				string text = dictionary[name].Split('/')[Utility.getSeasonNumber(GetSeasonForLocation())];
				if (!text.Equals("-1") && numberOfSpawnedObjectsOnMap < 6)
				{
					string[] array = text.Split(' ');
					int num = random.Next(1, Math.Min(5, 7 - numberOfSpawnedObjectsOnMap));
					for (int i = 0; i < num; i++)
					{
						for (int j = 0; j < 11; j++)
						{
							int num2 = random.Next(map.DisplayWidth / 64);
							int num3 = random.Next(map.DisplayHeight / 64);
							Vector2 vector = new Vector2(num2, num3);
							objects.TryGetValue(vector, out var value);
							int num4 = random.Next(array.Length / 2) * 2;
							if (value == null && doesTileHaveProperty(num2, num3, "Spawnable", "Back") != null && !doesEitherTileOrTileIndexPropertyEqual(num2, num3, "Spawnable", "Back", "F") && random.NextDouble() < Convert.ToDouble(array[num4 + 1], CultureInfo.InvariantCulture) && isTileLocationTotallyClearAndPlaceable(num2, num3) && getTileIndexAt(num2, num3, "AlwaysFront") == -1 && getTileIndexAt(num2, num3, "Front") == -1 && !isBehindBush(vector) && (Game1.random.NextDouble() < 0.1 || !isBehindTree(vector)) && dropObject(new Object(vector, Convert.ToInt32(array[num4]), null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: true), new Vector2(num2 * 64, num3 * 64), Game1.viewport, initialPlacement: true))
							{
								numberOfSpawnedObjectsOnMap++;
								break;
							}
						}
					}
				}
			}
			List<Vector2> list = new List<Vector2>();
			foreach (KeyValuePair<Vector2, Object> pair in objects.Pairs)
			{
				if ((int)pair.Value.parentSheetIndex == 590)
				{
					list.Add(pair.Key);
				}
			}
			if (!(this is Farm) && !(this is IslandWest))
			{
				spawnWeedsAndStones();
			}
			for (int num5 = list.Count - 1; num5 >= 0; num5--)
			{
				if ((!(this is IslandNorth) || !(list[num5].X < 26f)) && Game1.random.NextDouble() < 0.15)
				{
					objects.Remove(list.ElementAt(num5));
					list.RemoveAt(num5);
				}
			}
			if (list.Count > ((!(this is Farm)) ? 1 : 0) && (!GetSeasonForLocation().Equals("winter") || list.Count > 4))
			{
				return;
			}
			double num6 = 1.0;
			while (random.NextDouble() < num6)
			{
				int num7 = random.Next(map.DisplayWidth / 64);
				int num8 = random.Next(map.DisplayHeight / 64);
				Vector2 vector2 = new Vector2(num7, num8);
				if (isTileLocationTotallyClearAndPlaceable(vector2) && getTileIndexAt(num7, num8, "AlwaysFront") == -1 && getTileIndexAt(num7, num8, "Front") == -1 && !isBehindBush(vector2) && (doesTileHaveProperty(num7, num8, "Diggable", "Back") != null || (GetSeasonForLocation().Equals("winter") && doesTileHaveProperty(num7, num8, "Type", "Back") != null && doesTileHaveProperty(num7, num8, "Type", "Back").Equals("Grass"))))
				{
					if (name.Equals("Forest") && num7 >= 93 && num8 <= 22)
					{
						continue;
					}
					objects.Add(vector2, new Object(vector2, 590, 1));
				}
				num6 *= 0.75;
				if (GetSeasonForLocation().Equals("winter"))
				{
					num6 += 0.10000000149011612;
				}
			}
		}

		public bool isTileLocationOpen(Location location)
		{
			if (map.GetLayer("Buildings").Tiles[location.X, location.Y] == null && doesTileHaveProperty(location.X, location.Y, "Water", "Back") == null && map.GetLayer("Front").Tiles[location.X, location.Y] == null)
			{
				if (map.GetLayer("AlwaysFront") != null)
				{
					return map.GetLayer("AlwaysFront").Tiles[location.X, location.Y] == null;
				}
				return true;
			}
			return false;
		}

		public bool isTileLocationOpenIgnoreFrontLayers(Location location)
		{
			if (map.GetLayer("Buildings").Tiles[location.X, location.Y] == null)
			{
				return doesTileHaveProperty(location.X, location.Y, "Water", "Back") == null;
			}
			return false;
		}

		public bool IsNotWaterTileAndNotNullTile(Location location)
		{
			if (!IsWaterTile(location))
			{
				Tile tile = map.GetLayer("Back").PickTile(new Location(location.X * 64, location.Y * 64), Game1.viewport.Size);
				return tile != null;
			}
			return false;
		}

		public bool IsOnMap(Location location)
		{
			return map.GetLayer("Back").PickTile(new Location(location.X * 64, location.Y * 64), Game1.viewport.Size) != null;
		}

		public bool NeighboursLand(Location location)
		{
			Location location2 = new Location(location.X, location.Y - 1);
			if (!IsWaterTile(location2) && IsOnMap(location2))
			{
				return true;
			}
			location2.X = location.X;
			location2.Y = location.Y + 1;
			if (!IsWaterTile(location2) && IsOnMap(location2))
			{
				return true;
			}
			location2.X = location.X + 1;
			location2.Y = location.Y;
			if (!IsWaterTile(location2) && IsOnMap(location2))
			{
				return true;
			}
			location2.X = location.X - 1;
			location2.Y = location.Y;
			if (!IsWaterTile(location2) && IsOnMap(location2))
			{
				return true;
			}
			location2.X = location.X + 1;
			location2.Y = location.Y - 1;
			if (!IsWaterTile(location2) && IsOnMap(location2))
			{
				return true;
			}
			location2.X = location.X + 1;
			location2.Y = location.Y + 1;
			if (!IsWaterTile(location2) && IsOnMap(location2))
			{
				return true;
			}
			location2.X = location.X - 1;
			location2.Y = location.Y - 1;
			if (!IsWaterTile(location2) && IsOnMap(location2))
			{
				return true;
			}
			location2.X = location.X - 1;
			location2.Y = location.Y + 1;
			if (!IsWaterTile(location2) && IsOnMap(location2))
			{
				return true;
			}
			return false;
		}

		public bool DistanceToNeighboursLand(Location location, int distance = 2)
		{
			if (IsNotWaterTileAndNotNullTile(new Location(location.X, location.Y - distance)))
			{
				return true;
			}
			if (IsNotWaterTileAndNotNullTile(new Location(location.X, location.Y + distance)))
			{
				return true;
			}
			if (IsNotWaterTileAndNotNullTile(new Location(location.X + distance, location.Y)))
			{
				return true;
			}
			if (IsNotWaterTileAndNotNullTile(new Location(location.X - distance, location.Y)))
			{
				return true;
			}
			if (IsNotWaterTileAndNotNullTile(new Location(location.X + distance, location.Y - distance)))
			{
				return true;
			}
			if (IsNotWaterTileAndNotNullTile(new Location(location.X + distance, location.Y + distance)))
			{
				return true;
			}
			if (IsNotWaterTileAndNotNullTile(new Location(location.X - distance, location.Y - distance)))
			{
				return true;
			}
			if (IsNotWaterTileAndNotNullTile(new Location(location.X - distance, location.Y + distance)))
			{
				return true;
			}
			return false;
		}

		public void spawnWeedsAndStones(int numDebris = -1, bool weedsOnly = false, bool spawnFromOldWeeds = true)
		{
			if ((this is Farm || this is IslandWest) && Game1.getFarm().isBuildingConstructed("Gold Clock"))
			{
				return;
			}
			bool flag = false;
			if (this is Beach || GetSeasonForLocation().Equals("winter") || this is Desert)
			{
				return;
			}
			int num = ((numDebris != -1) ? numDebris : ((Game1.random.NextDouble() < 0.95) ? ((Game1.random.NextDouble() < 0.25) ? Game1.random.Next(10, 21) : Game1.random.Next(5, 11)) : 0));
			if (Game1.IsRainingHere(this))
			{
				num *= 2;
			}
			if (Game1.dayOfMonth == 1)
			{
				num *= 5;
			}
			if (objects.Count() <= 0 && spawnFromOldWeeds)
			{
				return;
			}
			if (!(this is Farm))
			{
				num /= 2;
			}
			for (int i = 0; i < num; i++)
			{
				Vector2 vector = (spawnFromOldWeeds ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : new Vector2(Game1.random.Next(map.Layers[0].LayerWidth), Game1.random.Next(map.Layers[0].LayerHeight)));
				if (!spawnFromOldWeeds && this is IslandWest)
				{
					vector = new Vector2(Game1.random.Next(57, 97), Game1.random.Next(44, 68));
				}
				while (spawnFromOldWeeds && vector.Equals(Vector2.Zero))
				{
					vector = new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
				}
				KeyValuePair<Vector2, Object> keyValuePair = new KeyValuePair<Vector2, Object>(Vector2.Zero, null);
				if (spawnFromOldWeeds)
				{
					keyValuePair = objects.Pairs.ElementAt(Game1.random.Next(objects.Count()));
				}
				Vector2 vector2 = (spawnFromOldWeeds ? keyValuePair.Key : Vector2.Zero);
				if ((this is Mountain && vector.X + vector2.X > 100f) || this is IslandNorth)
				{
					continue;
				}
				bool flag2 = this is Farm || this is IslandWest;
				int xTile = (int)(vector.X + vector2.X);
				int yTile = (int)(vector.Y + vector2.Y);
				Vector2 vector3 = vector + vector2;
				bool flag3 = false;
				bool flag4 = doesTileHaveProperty(xTile, yTile, "Diggable", "Back") != null;
				if (flag2 == flag4 && doesTileHaveProperty(xTile, yTile, "NoSpawn", "Back") == null)
				{
					string text = doesTileHaveProperty(xTile, yTile, "Type", "Back");
					if (text != "Wood")
					{
						bool flag5 = false;
						if (isTileLocationTotallyClearAndPlaceable(vector3))
						{
							flag5 = true;
						}
						else if (spawnFromOldWeeds)
						{
							if (objects.ContainsKey(vector3))
							{
								Object @object = objects[vector3];
								if (!@object.bigCraftable.Value || (@object.ParentSheetIndex != 105 && @object.ParentSheetIndex != 264))
								{
									flag5 = true;
								}
							}
							if (!flag5 && terrainFeatures.ContainsKey(vector3))
							{
								TerrainFeature terrainFeature = terrainFeatures[vector3];
								if (terrainFeature is HoeDirt || terrainFeature is Flooring)
								{
									flag5 = true;
								}
							}
						}
						if (flag5)
						{
							if (spawnFromOldWeeds)
							{
								flag3 = true;
							}
							else if (!objects.ContainsKey(vector3))
							{
								flag3 = true;
							}
						}
					}
				}
				if (!flag3)
				{
					continue;
				}
				int num2 = -1;
				if (this is Desert)
				{
					num2 = 750;
				}
				else
				{
					if (Game1.random.NextDouble() < 0.5 && !weedsOnly && (!spawnFromOldWeeds || keyValuePair.Value.Name.Equals("Stone") || keyValuePair.Value.Name.Contains("Twig")))
					{
						num2 = ((!(Game1.random.NextDouble() < 0.5)) ? ((Game1.random.NextDouble() < 0.5) ? 343 : 450) : ((Game1.random.NextDouble() < 0.5) ? 294 : 295));
					}
					else if (!spawnFromOldWeeds || keyValuePair.Value.Name.Contains("Weed"))
					{
						num2 = getWeedForSeason(Game1.random, GetSeasonForLocation());
					}
					if (this is Farm && !spawnFromOldWeeds && Game1.random.NextDouble() < 0.05)
					{
						terrainFeatures.Add(vector3, new Tree(Game1.random.Next(3) + 1, Game1.random.Next(3)));
						continue;
					}
				}
				if (num2 == -1)
				{
					continue;
				}
				bool flag6 = false;
				if (objects.ContainsKey(vector + vector2))
				{
					Object object2 = objects[vector + vector2];
					if (object2 is Fence || object2 is Chest)
					{
						continue;
					}
					if (object2.name != null && !object2.Name.Contains("Weed") && !object2.Name.Equals("Stone") && !object2.name.Contains("Twig") && object2.name.Length > 0)
					{
						flag6 = true;
						Game1.debugOutput = object2.Name + " was destroyed";
					}
					objects.Remove(vector + vector2);
				}
				if (terrainFeatures.ContainsKey(vector + vector2))
				{
					try
					{
						flag6 = terrainFeatures[vector + vector2] is HoeDirt || terrainFeatures[vector + vector2] is Flooring;
					}
					catch (Exception)
					{
					}
					if (!flag6)
					{
						break;
					}
					terrainFeatures.Remove(vector + vector2);
				}
				if (flag6 && this is Farm && Game1.stats.DaysPlayed > 1 && !flag)
				{
					flag = true;
					Game1.multiplayer.broadcastGlobalMessage("Strings\\Locations:Farm_WeedsDestruction", false);
				}
				objects.Add(vector + vector2, new Object(vector + vector2, num2, 1));
			}
		}

		public virtual void removeEverythingExceptCharactersFromThisTile(int x, int y)
		{
			Vector2 key = new Vector2(x, y);
			if (terrainFeatures.ContainsKey(key))
			{
				terrainFeatures.Remove(key);
			}
			if (objects.ContainsKey(key))
			{
				objects.Remove(key);
			}
		}

		public virtual string getFootstepSoundReplacement(string footstep)
		{
			return footstep;
		}

		public virtual void removeEverythingFromThisTile(int x, int y)
		{
			for (int num = resourceClumps.Count - 1; num >= 0; num--)
			{
				if (resourceClumps[num].tile.X == (float)x && resourceClumps[num].tile.Y == (float)y)
				{
					resourceClumps.RemoveAt(num);
				}
			}
			Vector2 vector = new Vector2(x, y);
			if (terrainFeatures.ContainsKey(vector))
			{
				terrainFeatures.Remove(vector);
			}
			if (objects.ContainsKey(vector))
			{
				objects.Remove(vector);
			}
			for (int num2 = characters.Count - 1; num2 >= 0; num2--)
			{
				if (characters[num2].getTileLocation().Equals(vector) && characters[num2] is Monster)
				{
					characters.RemoveAt(num2);
				}
			}
		}

		public virtual Dictionary<string, string> GetLocationEvents()
		{
			string text = name;
			if (uniqueName != null && uniqueName.Value != null && uniqueName.Value.Equals(Game1.player.homeLocation.Value))
			{
				text = "FarmHouse";
			}
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + text);
			if (Name == "IslandFarmHouse")
			{
				dictionary = new Dictionary<string, string>(dictionary);
				Dictionary<string, string> dictionary2 = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\FarmHouse");
				{
					foreach (string key in dictionary2.Keys)
					{
						string value = dictionary2[key];
						if (key.StartsWith("558291/") || key.StartsWith("558292/"))
						{
							dictionary[key] = value;
						}
					}
					return dictionary;
				}
			}
			if (Name == "Trailer_Big")
			{
				dictionary = new Dictionary<string, string>(dictionary);
				Dictionary<string, string> dictionary3 = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\Trailer");
				if (dictionary3 != null)
				{
					foreach (string key2 in dictionary3.Keys)
					{
						string text2 = dictionary3[key2];
						if (!(text == "Trailer_Big") || !dictionary.ContainsKey(key2))
						{
							if (key2.StartsWith("36/"))
							{
								text2 = text2.Replace("/farmer -30 30 0", "/farmer 12 19 0");
								text2 = text2.Replace("/playSound doorClose/warp farmer 12 9", "/move farmer 0 -10 0");
							}
							else if (key2.StartsWith("35/"))
							{
								text2 = text2.Replace("/farmer -30 30 0", "/farmer 12 19 0");
								text2 = text2.Replace("/warp farmer 12 9/playSound doorClose", "/move farmer 0 -10 0");
								text2 = text2.Replace("/warp farmer -40 -40/playSound doorClose", "/move farmer 0 10 0/warp farmer -40 -40");
							}
							dictionary[key2] = text2;
						}
					}
					return dictionary;
				}
			}
			return dictionary;
		}

		public virtual void checkForEvents()
		{
			if (Game1.killScreen && !Game1.eventUp)
			{
				if (name.Equals("Mine"))
				{
					string text = "Linus";
					string text2 = "Data\\ExtraDialogue:Mines_PlayerKilled_Linus";
					switch (Game1.random.Next(7))
					{
					case 0:
						text = "Robin";
						text2 = "Data\\ExtraDialogue:Mines_PlayerKilled_Robin";
						break;
					case 1:
						text = "Clint";
						text2 = "Data\\ExtraDialogue:Mines_PlayerKilled_Clint";
						break;
					case 2:
						text = "Maru";
						text2 = ((Game1.player.spouse != null && Game1.player.spouse.Equals("Maru")) ? "Data\\ExtraDialogue:Mines_PlayerKilled_Maru_Spouse" : "Data\\ExtraDialogue:Mines_PlayerKilled_Maru_NotSpouse");
						break;
					default:
						text = "Linus";
						text2 = "Data\\ExtraDialogue:Mines_PlayerKilled_Linus";
						break;
					}
					if (Game1.random.NextDouble() < 0.1 && Game1.player.spouse != null && !Game1.player.isEngaged() && Game1.player.spouse.Length > 1)
					{
						text = Game1.player.spouse;
						text2 = (Game1.player.IsMale ? "Data\\ExtraDialogue:Mines_PlayerKilled_Spouse_PlayerMale" : "Data\\ExtraDialogue:Mines_PlayerKilled_Spouse_PlayerFemale");
					}
					currentEvent = new Event(Game1.content.LoadString("Data\\Events\\Mine:PlayerKilled", text, text2, Game1.player.Name));
				}
				else if (this is IslandLocation)
				{
					string sub = "Willy";
					string sub2 = "Data\\ExtraDialogue:Island_willy_rescue";
					if (Game1.player.friendshipData.ContainsKey("Leo") && Game1.random.NextDouble() < 0.5)
					{
						sub = "Leo";
						sub2 = "Data\\ExtraDialogue:Island_leo_rescue";
					}
					currentEvent = new Event(Game1.content.LoadString("Data\\Events\\IslandSouth:PlayerKilled", sub, sub2, Game1.player.Name));
				}
				else if (name.Equals("Hospital"))
				{
					currentEvent = new Event(Game1.content.LoadString("Data\\Events\\Hospital:PlayerKilled", Game1.player.Name));
				}
				Game1.eventUp = true;
				Game1.killScreen = false;
				Game1.player.health = 10;
			}
			else if (!Game1.eventUp && Game1.weddingsToday.Count > 0 && (Game1.CurrentEvent == null || Game1.CurrentEvent.id != -2) && Game1.currentLocation != null && Game1.currentLocation.Name != "Temp")
			{
				currentEvent = Game1.getAvailableWeddingEvent();
				if (currentEvent != null)
				{
					startEvent(currentEvent);
				}
			}
			else
			{
				if (Game1.eventUp || Game1.farmEvent != null)
				{
					return;
				}
				string festival = Game1.currentSeason + Game1.dayOfMonth;
				try
				{
					Event @event = new Event();
					if (@event.tryToLoadFestival(festival))
					{
						currentEvent = @event;
					}
				}
				catch (Exception)
				{
				}
				if (!Game1.eventUp && currentEvent == null && Game1.farmEvent == null)
				{
					Dictionary<string, string> dictionary = null;
					try
					{
						string text3 = name;
						dictionary = GetLocationEvents();
					}
					catch (Exception)
					{
						return;
					}
					if (dictionary != null)
					{
						string[] array = dictionary.Keys.ToArray();
						for (int i = 0; i < array.Length; i++)
						{
							int num = checkEventPrecondition(array[i]);
							if (num != -1)
							{
								currentEvent = new Event(dictionary[array[i]], num);
								break;
							}
						}
						if (currentEvent == null && name.Equals("Farm") && Game1.IsMasterGame && !Game1.player.mailReceived.Contains("rejectedPet") && Game1.stats.DaysPlayed >= 20 && !Game1.player.hasPet())
						{
							for (int j = 0; j < array.Length; j++)
							{
								if ((array[j].Contains("dog") && !Game1.player.catPerson) || (array[j].Contains("cat") && Game1.player.catPerson))
								{
									currentEvent = new Event(dictionary[array[j]]);
									Game1.player.eventsSeen.Add(Convert.ToInt32(array[j].Split('/')[0]));
									break;
								}
							}
						}
					}
				}
				if (currentEvent != null)
				{
					startEvent(currentEvent);
				}
			}
		}

		public Event findEventById(int id, Farmer farmerActor = null)
		{
			if (id == -2)
			{
				long? spouse = Game1.player.team.GetSpouse(farmerActor.UniqueMultiplayerID);
				if (farmerActor == null || !spouse.HasValue)
				{
					return Utility.getWeddingEvent(farmerActor);
				}
				if (Game1.otherFarmers.ContainsKey(spouse.Value))
				{
					Farmer spouse2 = Game1.otherFarmers[spouse.Value];
					return Utility.getPlayerWeddingEvent(farmerActor, spouse2);
				}
			}
			Dictionary<string, string> dictionary = null;
			try
			{
				dictionary = GetLocationEvents();
			}
			catch (Exception)
			{
				return null;
			}
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				if (item.Key.Split('/')[0] == id.ToString())
				{
					return new Event(item.Value, id, farmerActor);
				}
			}
			return null;
		}

		public virtual void startEvent(Event evt)
		{
			if (Game1.eventUp || Game1.eventOver)
			{
				return;
			}
			currentEvent = evt;
			if (evt.exitLocation == null)
			{
				evt.exitLocation = Game1.getLocationRequest(isStructure ? uniqueName.Value : Name, isStructure);
			}
			if (Game1.player.mount != null)
			{
				Horse mount = Game1.player.mount;
				mount.currentLocation = this;
				mount.dismount();
				Microsoft.Xna.Framework.Rectangle boundingBox = mount.GetBoundingBox();
				Vector2 position = mount.Position;
				if (mount.currentLocation != null && mount.currentLocation.isCollidingPosition(boundingBox, Game1.viewport, isFarmer: false, 0, glider: false, mount, pathfinding: true))
				{
					boundingBox.X -= 64;
					if (!mount.currentLocation.isCollidingPosition(boundingBox, Game1.viewport, isFarmer: false, 0, glider: false, mount, pathfinding: true))
					{
						position.X -= 64f;
						mount.Position = position;
					}
					else
					{
						boundingBox.X += 128;
						if (!mount.currentLocation.isCollidingPosition(boundingBox, Game1.viewport, isFarmer: false, 0, glider: false, mount, pathfinding: true))
						{
							position.X += 64f;
							mount.Position = position;
						}
					}
				}
			}
			foreach (NPC character in characters)
			{
				character.clearTextAboveHead();
			}
			Game1.eventUp = true;
			Game1.displayHUD = false;
			Game1.player.CanMove = false;
			Game1.player.showNotCarrying();
			if (critters != null)
			{
				critters.Clear();
			}
		}

		public virtual void drawBackground(SpriteBatch b)
		{
		}

		public virtual void drawWater(SpriteBatch b)
		{
			if (currentEvent != null)
			{
				currentEvent.drawUnderWater(b);
			}
			if (waterTiles == null)
			{
				return;
			}
			for (int i = Math.Max(0, Game1.viewport.Y / 64 - 1); i < Math.Min(map.Layers[0].LayerHeight, (Game1.viewport.Y + Game1.viewport.Height) / 64 + 2); i++)
			{
				for (int j = Math.Max(0, Game1.viewport.X / 64 - 1); j < Math.Min(map.Layers[0].LayerWidth, (Game1.viewport.X + Game1.viewport.Width) / 64 + 1); j++)
				{
					if (waterTiles.waterTiles[j, i].isWater && waterTiles.waterTiles[j, i].isVisible)
					{
						drawWaterTile(b, j, i);
					}
				}
			}
		}

		public virtual void drawWaterTile(SpriteBatch b, int x, int y)
		{
			drawWaterTile(b, x, y, waterColor);
		}

		public void drawWaterTile(SpriteBatch b, int x, int y, Color color)
		{
			bool flag = y == map.Layers[0].LayerHeight - 1 || !waterTiles[x, y + 1];
			bool flag2 = y == 0 || !waterTiles[x, y - 1];
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - (int)((!flag2) ? waterPosition : 0f))), new Microsoft.Xna.Framework.Rectangle(waterAnimationIndex * 64, 2064 + (((x + y) % 2 != 0) ? ((!waterTileFlip) ? 128 : 0) : (waterTileFlip ? 128 : 0)) + (flag2 ? ((int)waterPosition) : 0), 64, 64 + (flag2 ? ((int)(0f - waterPosition)) : 0)), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.56f);
			if (flag)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y + 1) * 64 - (int)waterPosition)), new Microsoft.Xna.Framework.Rectangle(waterAnimationIndex * 64, 2064 + (((x + (y + 1)) % 2 != 0) ? ((!waterTileFlip) ? 128 : 0) : (waterTileFlip ? 128 : 0)), 64, 64 - (int)(64f - waterPosition) - 1), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.56f);
			}
		}

		public virtual void drawFloorDecorations(SpriteBatch b)
		{
			if (!Game1.isFestival())
			{
				Vector2 vector = default(Vector2);
				for (int i = Game1.viewport.Y / 64 - 1; i < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 7; i++)
				{
					for (int j = Game1.viewport.X / 64 - 1; j < (Game1.viewport.X + Game1.viewport.Width) / 64 + 3; j++)
					{
						vector.X = j;
						vector.Y = i;
						if (terrainFeatures.TryGetValue(vector, out var value) && value is Flooring flooring)
						{
							flooring.draw(b, vector);
						}
					}
				}
			}
			if (Game1.eventUp && !(this is Farm) && !(this is FarmHouse))
			{
				return;
			}
			Furniture.isDrawingLocationFurniture = true;
			foreach (Furniture item in furniture)
			{
				if (item.furniture_type.Value == 12)
				{
					item.draw(b, -1, -1);
				}
			}
			Furniture.isDrawingLocationFurniture = false;
		}

		public TemporaryAnimatedSprite getTemporarySpriteByID(int id)
		{
			for (int i = 0; i < temporarySprites.Count; i++)
			{
				if (temporarySprites[i].id == (float)id)
				{
					return temporarySprites[i];
				}
			}
			return null;
		}

		protected void drawDebris(SpriteBatch b)
		{
			int num = 0;
			foreach (Debris debri in debris)
			{
				num++;
				if (debri.item != null)
				{
					Chunk chunk = debri.Chunks[0];
					if (debri.item is Object && (bool)(debri.item as Object).bigCraftable)
					{
						debri.item.drawInMenu(b, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, chunk.position + new Vector2(32f, 32f))), 1.6f, 1f, ((float)(debri.chunkFinalYLevel + 64 + 8) + chunk.position.X / 10000f) / 10000f, StackDrawType.Hide, Color.White, drawShadow: true);
					}
					else
					{
						debri.item.drawInMenu(b, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, chunk.position + new Vector2(32f, 32f))), 0.8f + (float)debri.itemQuality * 0.1f, 1f, ((float)(debri.chunkFinalYLevel + 64 + 8) + chunk.position.X / 10000f) / 10000f, StackDrawType.Hide, Color.White, drawShadow: true);
					}
					continue;
				}
				if ((Debris.DebrisType)debri.debrisType == Debris.DebrisType.LETTERS)
				{
					Chunk chunk2 = debri.Chunks[0];
					Game1.drawWithBorder(debri.debrisMessage, Color.Black, debri.nonSpriteChunkColor, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, chunk2.position)), chunk2.rotation, chunk2.scale, (chunk2.position.Y + 64f) / 10000f);
					continue;
				}
				if ((Debris.DebrisType)debri.debrisType == Debris.DebrisType.NUMBERS)
				{
					Chunk chunk3 = debri.Chunks[0];
					NumberSprite.draw(debri.chunkType, b, Game1.GlobalToLocal(Game1.viewport, Utility.snapDrawPosition(new Vector2(chunk3.position.X, (float)debri.chunkFinalYLevel - ((float)debri.chunkFinalYLevel - chunk3.position.Y)))), debri.nonSpriteChunkColor, chunk3.scale * 0.75f, 0.98f + 0.0001f * (float)num, chunk3.alpha, -1 * (int)((float)debri.chunkFinalYLevel - chunk3.position.Y) / 2);
					continue;
				}
				if ((Debris.DebrisType)debri.debrisType == Debris.DebrisType.SPRITECHUNKS)
				{
					foreach (Chunk chunk4 in debri.Chunks)
					{
						b.Draw(debri.spriteChunkSheet, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, chunk4.position)), new Microsoft.Xna.Framework.Rectangle(chunk4.xSpriteSheet, chunk4.ySpriteSheet, Math.Min(debri.sizeOfSourceRectSquares, debri.spriteChunkSheet.Bounds.Width), Math.Min(debri.sizeOfSourceRectSquares, debri.spriteChunkSheet.Bounds.Height)), debri.nonSpriteChunkColor.Value * chunk4.alpha, chunk4.rotation, new Vector2((int)debri.sizeOfSourceRectSquares / 2, (int)debri.sizeOfSourceRectSquares / 2), chunk4.scale, SpriteEffects.None, ((float)(debri.chunkFinalYLevel + 16) + chunk4.position.X / 10000f) / 10000f);
					}
					continue;
				}
				if ((Debris.DebrisType)debri.debrisType == Debris.DebrisType.SQUARES)
				{
					foreach (Chunk chunk5 in debri.Chunks)
					{
						b.Draw(Game1.littleEffect, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, chunk5.position)), new Microsoft.Xna.Framework.Rectangle(0, 0, 4, 4), debri.nonSpriteChunkColor, 0f, Vector2.Zero, 1f + (float)chunk5.yVelocity / 2f, SpriteEffects.None, (chunk5.position.Y + 64f) / 10000f);
					}
					continue;
				}
				if ((Debris.DebrisType)debri.debrisType != 0)
				{
					foreach (Chunk chunk6 in debri.Chunks)
					{
						if (chunk6.debrisType <= 0)
						{
							b.Draw(Game1.bigCraftableSpriteSheet, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, chunk6.position + new Vector2(32f, 64f))), Game1.getArbitrarySourceRect(Game1.bigCraftableSpriteSheet, 16, 32, -chunk6.debrisType), Color.White, 0f, new Vector2(8f, 32f), 3.2f, SpriteEffects.None, ((float)(debri.chunkFinalYLevel + 48) + chunk6.position.X / 10000f) / 10000f);
							b.Draw(Game1.shadowTexture, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, new Vector2(chunk6.position.X + 25.6f, (debri.chunksMoveTowardPlayer ? (chunk6.position.Y + 8f) : ((float)debri.chunkFinalYLevel)) + 32f))), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), Math.Min(3f, 3f - (debri.chunksMoveTowardPlayer ? 0f : (((float)debri.chunkFinalYLevel - chunk6.position.Y) / 128f))), SpriteEffects.None, (float)debri.chunkFinalYLevel / 10000f);
						}
						else
						{
							b.Draw(Game1.objectSpriteSheet, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, chunk6.position)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, chunk6.debrisType, 16, 16), Color.White, 0f, Vector2.Zero, ((Debris.DebrisType)debri.debrisType == Debris.DebrisType.RESOURCE || (bool)debri.floppingFish) ? 4f : (4f * (0.8f + (float)debri.itemQuality * 0.1f)), ((bool)debri.floppingFish && chunk6.bounces % 2 == 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((float)(debri.chunkFinalYLevel + 32) + chunk6.position.X / 10000f) / 10000f);
							b.Draw(Game1.shadowTexture, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, new Vector2(chunk6.position.X + 25.6f, (debri.chunksMoveTowardPlayer ? (chunk6.position.Y + 8f) : ((float)debri.chunkFinalYLevel)) + 32f + (float)(12 * debri.itemQuality)))), Game1.shadowTexture.Bounds, Color.White * 0.75f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), Math.Min(3f, 3f - (debri.chunksMoveTowardPlayer ? 0f : (((float)debri.chunkFinalYLevel - chunk6.position.Y) / 96f))), SpriteEffects.None, (float)debri.chunkFinalYLevel / 10000f);
						}
					}
					continue;
				}
				foreach (Chunk chunk7 in debri.Chunks)
				{
					b.Draw(Game1.debrisSpriteSheet, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, chunk7.position)), Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, chunk7.debrisType, 16, 16), debri.chunksColor, 0f, Vector2.Zero, 4f * (float)debri.scale, SpriteEffects.None, (chunk7.position.Y + 128f + chunk7.position.X / 10000f) / 10000f);
				}
			}
		}

		public virtual bool shouldHideCharacters()
		{
			return false;
		}

		protected virtual void drawCharacters(SpriteBatch b)
		{
			if (shouldHideCharacters() || (Game1.eventUp && (Game1.CurrentEvent == null || !Game1.CurrentEvent.showWorldCharacters)))
			{
				return;
			}
			for (int i = 0; i < characters.Count; i++)
			{
				if (characters[i] != null)
				{
					characters[i].draw(b);
				}
			}
		}

		protected virtual void drawFarmers(SpriteBatch b)
		{
			if (shouldHideCharacters() || Game1.currentMinigame != null)
			{
				return;
			}
			if (currentEvent == null || currentEvent.isFestival || currentEvent.farmerActors.Count == 0)
			{
				foreach (Farmer farmer in farmers)
				{
					if (!Game1.multiplayer.isDisconnecting(farmer.UniqueMultiplayerID))
					{
						farmer.draw(b);
					}
				}
				return;
			}
			currentEvent.drawFarmers(b);
		}

		public virtual void DrawFarmerUsernames(SpriteBatch b)
		{
			if (shouldHideCharacters() || Game1.currentMinigame != null || (currentEvent != null && !currentEvent.isFestival && currentEvent.farmerActors.Count != 0))
			{
				return;
			}
			foreach (Farmer farmer in farmers)
			{
				if (!Game1.multiplayer.isDisconnecting(farmer.UniqueMultiplayerID))
				{
					farmer.DrawUsername(b);
				}
			}
		}

		public virtual void draw(SpriteBatch b)
		{
			foreach (MapSeat mapSeat in mapSeats)
			{
				mapSeat.Draw(b);
			}
			foreach (ResourceClump resourceClump in resourceClumps)
			{
				resourceClump.draw(b, resourceClump.tile);
			}
			current_location_farmers.Clear();
			foreach (Farmer farmer3 in farmers)
			{
				farmer3.drawLayerDisambiguator = 0f;
				current_location_farmers.Add(farmer3);
			}
			if (current_location_farmers.Contains(Game1.player))
			{
				current_location_farmers.Remove(Game1.player);
				current_location_farmers.Insert(0, Game1.player);
			}
			float num = 0.0001f;
			for (int i = 0; i < current_location_farmers.Count; i++)
			{
				for (int j = i + 1; j < current_location_farmers.Count; j++)
				{
					Farmer farmer = current_location_farmers[i];
					Farmer farmer2 = current_location_farmers[j];
					if (!farmer2.IsSitting() && Math.Abs(farmer.getDrawLayer() - farmer2.getDrawLayer()) < num && Math.Abs(farmer.position.X - farmer2.position.X) < 64f)
					{
						farmer2.drawLayerDisambiguator += farmer.getDrawLayer() - num - farmer2.getDrawLayer();
					}
				}
			}
			current_location_farmers.Clear();
			drawCharacters(b);
			drawFarmers(b);
			if (critters != null && Game1.farmEvent == null)
			{
				for (int k = 0; k < critters.Count; k++)
				{
					critters[k].draw(b);
				}
			}
			drawDebris(b);
			if (!Game1.eventUp || (currentEvent != null && currentEvent.showGroundObjects))
			{
				Vector2 key = default(Vector2);
				for (int l = Game1.viewport.Y / 64 - 1; l < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 3; l++)
				{
					for (int m = Game1.viewport.X / 64 - 1; m < (Game1.viewport.X + Game1.viewport.Width) / 64 + 1; m++)
					{
						key.X = m;
						key.Y = l;
						if (objects.TryGetValue(key, out var value))
						{
							value.draw(b, (int)key.X, (int)key.Y);
						}
					}
				}
			}
			foreach (TemporaryAnimatedSprite temporarySprite in TemporarySprites)
			{
				if (!temporarySprite.drawAboveAlwaysFront)
				{
					temporarySprite.draw(b);
				}
			}
			interiorDoors.Draw(b);
			if (largeTerrainFeatures != null)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					largeTerrainFeature.draw(b);
				}
			}
			testToDrawShopIconAnim(b);
			if (fishSplashAnimation != null)
			{
				fishSplashAnimation.draw(b);
			}
			if (orePanAnimation != null)
			{
				orePanAnimation.draw(b);
			}
			if (!Game1.eventUp || this is Farm || this is FarmHouse)
			{
				Furniture.isDrawingLocationFurniture = true;
				foreach (Furniture item in furniture)
				{
					if (item.furniture_type.Value != 12)
					{
						item.draw(b, -1, -1);
					}
				}
				Furniture.isDrawingLocationFurniture = false;
			}
			if (showDropboxIndicator && !Game1.eventUp)
			{
				float num2 = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(dropBoxIndicatorLocation.X, dropBoxIndicatorLocation.Y + num2)), new Microsoft.Xna.Framework.Rectangle(114, 53, 6, 10), Color.White, 0f, new Vector2(1f, 4f), 4f, SpriteEffects.None, 1f);
			}
		}

		public virtual void drawAboveFrontLayer(SpriteBatch b)
		{
			if (!Game1.isFestival())
			{
				Vector2 vector = default(Vector2);
				for (int i = Game1.viewport.Y / 64 - 1; i < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 7; i++)
				{
					for (int j = Game1.viewport.X / 64 - 1; j < (Game1.viewport.X + Game1.viewport.Width) / 64 + 3; j++)
					{
						vector.X = j;
						vector.Y = i;
						if (terrainFeatures.TryGetValue(vector, out var value) && !(value is Flooring))
						{
							value.draw(b, vector);
						}
					}
				}
			}
			if (!(this is MineShaft))
			{
				foreach (NPC character in characters)
				{
					if (character is Monster monster)
					{
						monster.drawAboveAllLayers(b);
					}
				}
			}
			if (lightGlows.Count > 0)
			{
				drawLightGlows(b);
			}
			DrawFarmerUsernames(b);
		}

		public virtual void drawLightGlows(SpriteBatch b)
		{
			foreach (Vector2 lightGlow in lightGlows)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, lightGlow), new Microsoft.Xna.Framework.Rectangle(21, 1695, 41, 67), Color.White, 0f, new Vector2(19f, 22f), 4f, SpriteEffects.None, 1f);
			}
		}

		public Object tryToCreateUnseenSecretNote(Farmer who)
		{
			bool flag = GetLocationContext() == LocationContext.Island;
			if (who != null && ((who.hasMagnifyingGlass && !flag) || flag))
			{
				Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\SecretNotes");
				int num = 0;
				foreach (int key in dictionary.Keys)
				{
					if (flag)
					{
						if (key >= JOURNAL_INDEX)
						{
							num++;
						}
					}
					else if (key < JOURNAL_INDEX)
					{
						num++;
					}
				}
				secretNotesSeen.Clear();
				foreach (int item in who.secretNotesSeen)
				{
					if (item < JOURNAL_INDEX)
					{
						secretNotesSeen.Add(item);
					}
					else
					{
						journalsSeen.Add(item);
					}
				}
				int num2 = 0;
				num2 = ((!flag) ? secretNotesSeen.Count : journalsSeen.Count);
				if (num2 == num)
				{
					return null;
				}
				float num3 = (float)(num - 1 - num2) / (float)Math.Max(1, num - 1);
				float num4 = LAST_SECRET_NOTE_CHANCE + (FIRST_SECRET_NOTE_CHANCE - LAST_SECRET_NOTE_CHANCE) * num3;
				if (Game1.random.NextDouble() < (double)num4)
				{
					unseenJournals.Clear();
					unseenSecretNotes.Clear();
					foreach (int key2 in dictionary.Keys)
					{
						if (key2 < JOURNAL_INDEX && !secretNotesSeen.Contains(key2) && !who.hasItemInInventoryNamed("Secret Note #" + key2) && (key2 != 10 || who.mailReceived.Contains("QiChallengeComplete")))
						{
							unseenSecretNotes.Add(key2);
						}
						else if (key2 >= JOURNAL_INDEX && !journalsSeen.Contains(key2) && !who.hasItemInInventoryNamed("Journal Scrap #" + (key2 - JOURNAL_INDEX)))
						{
							unseenJournals.Add(key2);
						}
					}
					if (!flag && unseenSecretNotes.Count > 0)
					{
						int num5 = unseenSecretNotes[Game1.random.Next(unseenSecretNotes.Count)];
						Object @object = new Object(79, 1);
						@object.name = @object.name + " #" + num5;
						return @object;
					}
					if (flag && unseenJournals.Count > 0)
					{
						int num6 = unseenJournals.First();
						Object object2 = new Object(842, 1);
						object2.name = object2.name + " #" + (num6 - JOURNAL_INDEX);
						return object2;
					}
				}
			}
			return null;
		}

		public virtual bool performToolAction(Tool t, int tileX, int tileY)
		{
			for (int num = resourceClumps.Count - 1; num >= 0; num--)
			{
				if (resourceClumps[num] != null && resourceClumps[num].getBoundingBox(resourceClumps[num].tile).Contains(tileX * 64, tileY * 64) && resourceClumps[num].performToolAction(t, 1, resourceClumps[num].tile, this))
				{
					resourceClumps.RemoveAt(num);
					return true;
				}
			}
			return false;
		}

		public virtual void seasonUpdate(string season, bool onLoad = false)
		{
			for (int num = terrainFeatures.Count() - 1; num >= 0; num--)
			{
				if (terrainFeatures.Values.ElementAt(num).seasonUpdate(onLoad))
				{
					terrainFeatures.Remove(terrainFeatures.Keys.ElementAt(num));
				}
			}
			if (largeTerrainFeatures != null)
			{
				for (int num2 = largeTerrainFeatures.Count - 1; num2 >= 0; num2--)
				{
					if (largeTerrainFeatures.ElementAt(num2).seasonUpdate(onLoad))
					{
						largeTerrainFeatures.Remove(largeTerrainFeatures.ElementAt(num2));
					}
				}
			}
			foreach (NPC character in getCharacters())
			{
				if (!character.IsMonster)
				{
					character.resetSeasonalDialogue();
				}
			}
			if (IsOutdoors && !onLoad)
			{
				for (int num3 = objects.Count() - 1; num3 >= 0; num3--)
				{
					if (objects.Pairs.ElementAt(num3).Value.IsSpawnedObject && !objects.Pairs.ElementAt(num3).Value.Name.Equals("Stone"))
					{
						objects.Remove(objects.Pairs.ElementAt(num3).Key);
					}
					else if ((int)objects.Pairs.ElementAt(num3).Value.parentSheetIndex == 590 && doesTileHavePropertyNoNull((int)objects.Pairs.ElementAt(num3).Key.X, (int)objects.Pairs.ElementAt(num3).Key.Y, "Diggable", "Back") == "")
					{
						objects.Remove(objects.Pairs.ElementAt(num3).Key);
					}
				}
				numberOfSpawnedObjectsOnMap = 0;
			}
			switch (season.ToLower())
			{
			case "spring":
				waterColor.Value = new Color(120, 200, 255) * 0.5f;
				break;
			case "summer":
				waterColor.Value = new Color(60, 240, 255) * 0.5f;
				break;
			case "fall":
				waterColor.Value = new Color(255, 130, 200) * 0.5f;
				break;
			case "winter":
				waterColor.Value = new Color(130, 80, 255) * 0.5f;
				break;
			}
			if (!onLoad && season == "spring" && Game1.stats.daysPlayed > 1 && !(this is Farm))
			{
				loadWeeds();
			}
		}

		public virtual void updateSeasonalTileSheets(Map map = null)
		{
			if (map == null)
			{
				map = Map;
			}
			if (this is Summit || (IsOutdoors && !Name.Equals("Desert")))
			{
				map.DisposeTileSheets(Game1.mapDisplayDevice);
				for (int i = 0; i < map.TileSheets.Count; i++)
				{
					map.TileSheets[i].ImageSource = GetSeasonalTilesheetName(map.TileSheets[i].ImageSource, GetSeasonForLocation());
				}
				map.LoadTileSheets(Game1.mapDisplayDevice);
			}
		}

		public static string GetSeasonalTilesheetName(string sheet_path, string current_season)
		{
			string fileName = Path.GetFileName(sheet_path);
			if (fileName.StartsWith("spring_") || fileName.StartsWith("summer_") || fileName.StartsWith("fall_") || fileName.StartsWith("winter_"))
			{
				string directoryName = Path.GetDirectoryName(sheet_path);
				sheet_path = Path.Combine(directoryName, current_season + fileName.Substring(fileName.IndexOf('_')));
			}
			return sheet_path;
		}

		public virtual int checkEventPrecondition(string precondition)
		{
			string[] array = precondition.Split(ForwardSlash);
			if (!int.TryParse(array[0], out var result))
			{
				return -1;
			}
			if (Game1.player.eventsSeen.Contains(result))
			{
				return -1;
			}
			for (int i = 1; i < array.Length; i++)
			{
				if (array[i][0] == 'e')
				{
					if (checkEventsSeenPreconditions(array[i].Split(' ')))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'h')
				{
					if (Game1.player.hasPet())
					{
						return -1;
					}
					if ((Game1.player.catPerson && !array[i].Split(' ')[1].ToString().ToLower().Equals("cat")) || (!Game1.player.catPerson && !array[i].Split(' ')[1].ToString().ToLower().Equals("dog")))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'H')
				{
					string[] array2 = array[i].Split(' ');
					if (array2[0] == "H")
					{
						if (!Game1.IsMasterGame)
						{
							return -1;
						}
					}
					else if (array2[0] == "Hn")
					{
						if (!Game1.MasterPlayer.mailReceived.Contains(array2[1]))
						{
							return -1;
						}
					}
					else if (array2[0] == "Hl" && Game1.MasterPlayer.mailReceived.Contains(array2[1]))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == '*')
				{
					string[] array3 = array[i].Split(' ');
					if (array3[0] == "*")
					{
						if (!NetWorldState.checkAnywhereForWorldStateID(array3[1]))
						{
							return -1;
						}
					}
					else if (array3[0] == "*n")
					{
						if (!Game1.MasterPlayer.mailReceived.Contains(array3[1]) && !Game1.player.mailReceived.Contains(array3[1]))
						{
							return -1;
						}
					}
					else if (array3[0] == "*l" && (Game1.MasterPlayer.mailReceived.Contains(array3[1]) || Game1.player.mailReceived.Contains(array3[1])))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'm')
				{
					string[] array4 = array[i].Split(' ');
					if (Game1.player.totalMoneyEarned < Convert.ToInt32(array4[1]))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'M')
				{
					string[] array5 = array[i].Split(' ');
					if (Game1.player.Money < Convert.ToInt32(array5[1]))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'c')
				{
					if (Game1.player.freeSpotsInInventory() < Convert.ToInt32(array[i].Split(' ')[1]))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'C')
				{
					if (!Game1.MasterPlayer.eventsSeen.Contains(191393) && !Game1.MasterPlayer.eventsSeen.Contains(502261) && !Game1.MasterPlayer.hasCompletedCommunityCenter())
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'X')
				{
					if (Game1.MasterPlayer.eventsSeen.Contains(191393) || Game1.MasterPlayer.eventsSeen.Contains(502261) || Game1.MasterPlayer.hasCompletedCommunityCenter())
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'D')
				{
					string key = array[i].Split(' ')[1];
					if (!Game1.player.friendshipData.ContainsKey(key))
					{
						return -1;
					}
					if (!Game1.player.friendshipData[key].IsDating())
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'j')
				{
					if (Game1.stats.DaysPlayed <= Convert.ToInt32(array[i].Split(' ')[1]))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'J')
				{
					if (!checkJojaCompletePrerequisite())
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'f')
				{
					if (!checkFriendshipPrecondition(array[i].Split(' ')))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'F')
				{
					if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'r')
				{
					string[] array6 = array[i].Split(' ');
					if (Game1.random.NextDouble() > Convert.ToDouble(array6[1]))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 's')
				{
					if (!checkItemsPrecondition(array[i].Split(' ')))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'S')
				{
					if (!Game1.player.secretNotesSeen.Contains(Convert.ToInt32(array[i].Split(' ')[1])))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'q')
				{
					if (!checkDialoguePrecondition(array[i].Split(' ')))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'n')
				{
					if (!Game1.player.mailReceived.Contains(array[i].Split(' ')[1]))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'N')
				{
					if (Game1.netWorldState.Value.GoldenWalnutsFound.Value < Convert.ToInt32(array[i].Split(' ')[1]))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'l')
				{
					if (Game1.player.mailReceived.Contains(array[i].Split(' ')[1]))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'L')
				{
					if (!(this is FarmHouse) || (this as FarmHouse).upgradeLevel < 2)
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 't')
				{
					string[] array7 = array[i].Split(' ');
					if (Game1.timeOfDay < Convert.ToInt32(array7[1]) || Game1.timeOfDay > Convert.ToInt32(array7[2]))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'w')
				{
					string[] array8 = array[i].Split(' ');
					if ((array8[1].Equals("rainy") && !Game1.IsRainingHere(this)) || (array8[1].Equals("sunny") && Game1.IsRainingHere(this)))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'd')
				{
					string[] source = array[i].Split(' ');
					if (source.Contains(Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'o')
				{
					if (Game1.player.spouse != null && Game1.player.spouse.Equals(array[i].Split(' ')[1]))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'O')
				{
					if (Game1.player.spouse == null || !Game1.player.spouse.Equals(array[i].Split(' ')[1]))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'R')
				{
					if (array[i] == "Rf")
					{
						if (Game1.player.hasCurrentOrPendingRoommate())
						{
							return -1;
						}
					}
					else if (!Game1.player.hasCurrentOrPendingRoommate())
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'v')
				{
					string[] array9 = array[i].Split(' ');
					if (Game1.getCharacterFromName(array9[1]).IsInvisible)
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'p')
				{
					string[] array10 = array[i].Split(' ');
					if (!isCharacterHere(array10[1]))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'z')
				{
					string[] array11 = array[i].Split(' ');
					if (Game1.currentSeason.Equals(array11[1]))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'B')
				{
					if (Utility.getHomeOfFarmer(Game1.player).GetSpouseBed() == null)
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'b')
				{
					string[] array12 = array[i].Split(' ');
					if (Game1.player.timesReachedMineBottom < Convert.ToInt32(array12[1]))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'y')
				{
					if (Game1.year < Convert.ToInt32(array[i].Split(' ')[1]) || (Convert.ToInt32(array[i].Split(' ')[1]) == 1 && Game1.year != 1))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'g')
				{
					string text = (Game1.player.IsMale ? "male" : "female");
					if (!text.Equals(array[i].Split(' ')[1].ToLower()))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'i')
				{
					if (!Game1.player.hasItemInInventory(Convert.ToInt32(array[i].Split(' ')[1]), 1) && (Game1.player.ActiveObject == null || Game1.player.ActiveObject.ParentSheetIndex != Convert.ToInt32(array[i].Split(' ')[1])))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'k')
				{
					if (!checkEventsSeenPreconditions(array[i].Split(' ')))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'a')
				{
					bool flag = false;
					string[] array13 = array[i].Split(' ');
					for (int j = 1; j < array13.Length - 1; j += 2)
					{
						if (Game1.xLocationAfterWarp == Convert.ToInt32(array13[j]) && Game1.yLocationAfterWarp == Convert.ToInt32(array13[j + 1]))
						{
							flag = true;
						}
					}
					if (!flag)
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'A')
				{
					if (Game1.player.activeDialogueEvents.ContainsKey(array[i].Split(' ')[1]))
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'x')
				{
					string[] array14 = array[i].Split(' ');
					if (array14.Length == 2)
					{
						Game1.addMailForTomorrow(array14[1]);
					}
					else
					{
						Game1.player.mailbox.Add(array14[1]);
					}
					Game1.player.eventsSeen.Add(Convert.ToInt32(array[0]));
					return -1;
				}
				if (array[i][0] == 'u')
				{
					bool flag2 = false;
					string[] array15 = array[i].Split(' ');
					for (int k = 1; k < array15.Length; k++)
					{
						if (Game1.dayOfMonth == Convert.ToInt32(array15[k]))
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						return -1;
					}
					continue;
				}
				if (array[i][0] == 'U')
				{
					int num = Convert.ToInt32(array[i].Split(' ')[1]);
					string text2 = Game1.currentSeason;
					int num2 = Game1.dayOfMonth;
					for (int l = 0; l < num; l++)
					{
						if (Utility.isFestivalDay(num2, text2))
						{
							return -1;
						}
						num2++;
						if (num2 > 28)
						{
							num2 = 1;
							text2 = Utility.getSeasonNameFromNumber((Utility.getSeasonNumber(text2) + 1) % 4).ToLower();
						}
					}
					continue;
				}
				return -1;
			}
			return result;
		}

		private bool isCharacterHere(string name)
		{
			foreach (NPC character in characters)
			{
				if (character.Name.Equals(name) && !character.IsInvisible)
				{
					return true;
				}
			}
			return false;
		}

		private bool checkJojaCompletePrerequisite()
		{
			return Utility.hasFinishedJojaRoute();
		}

		private bool checkEventsSeenPreconditions(string[] eventIDs)
		{
			for (int i = 1; i < eventIDs.Length; i++)
			{
				if (int.TryParse(eventIDs[i], out var _) && Game1.player.eventsSeen.Contains(Convert.ToInt32(eventIDs[i])))
				{
					return false;
				}
			}
			return true;
		}

		private bool checkFriendshipPrecondition(string[] friendshipString)
		{
			for (int i = 1; i < friendshipString.Length; i += 2)
			{
				if (!Game1.player.friendshipData.ContainsKey(friendshipString[i]) || Game1.player.friendshipData[friendshipString[i]].Points < Convert.ToInt32(friendshipString[i + 1]))
				{
					return false;
				}
			}
			return true;
		}

		private bool checkItemsPrecondition(string[] itemString)
		{
			for (int i = 1; i < itemString.Length; i += 2)
			{
				if (!Game1.player.basicShipped.ContainsKey(Convert.ToInt32(itemString[i])) || Game1.player.basicShipped[Convert.ToInt32(itemString[i])] < Convert.ToInt32(itemString[i + 1]))
				{
					return false;
				}
			}
			return true;
		}

		private bool checkDialoguePrecondition(string[] dialogueString)
		{
			for (int i = 1; i < dialogueString.Length; i += 2)
			{
				if (!Game1.player.DialogueQuestionsAnswered.Contains(Convert.ToInt32(dialogueString[i])))
				{
					return false;
				}
			}
			return true;
		}

		public virtual void updateWarps()
		{
			warps.Clear();
			map.Properties.TryGetValue("NPCWarp", out var value);
			if (value != null)
			{
				string[] array = value.ToString().Split(' ');
				for (int i = 0; i < array.Length; i += 5)
				{
					warps.Add(new Warp(Convert.ToInt32(array[i]), Convert.ToInt32(array[i + 1]), array[i + 2], Convert.ToInt32(array[i + 3]), Convert.ToInt32(array[i + 4]), flipFarmer: false, npcOnly: true));
				}
			}
			value = null;
			map.Properties.TryGetValue("Warp", out value);
			if (value != null)
			{
				string[] array2 = value.ToString().Split(' ');
				for (int j = 0; j < array2.Length; j += 5)
				{
					warps.Add(new Warp(Convert.ToInt32(array2[j]), Convert.ToInt32(array2[j + 1]), array2[j + 2], Convert.ToInt32(array2[j + 3]), Convert.ToInt32(array2[j + 4]), flipFarmer: false));
				}
			}
		}

		public void loadWeeds()
		{
			if (map == null)
			{
				return;
			}
			bool flag = false;
			foreach (Layer layer in map.Layers)
			{
				if (layer.Id.Equals("Paths"))
				{
					flag = true;
					break;
				}
			}
			if (!(((bool)isOutdoors || (bool)treatAsOutdoors) && flag))
			{
				return;
			}
			for (int i = 0; i < map.Layers[0].LayerWidth; i++)
			{
				for (int j = 0; j < map.Layers[0].LayerHeight; j++)
				{
					Tile tile = map.GetLayer("Paths").Tiles[i, j];
					if (tile == null)
					{
						continue;
					}
					Vector2 vector = new Vector2(i, j);
					switch (tile.TileIndex)
					{
					case 13:
					case 14:
					case 15:
						if (CanLoadPathObjectHere(vector))
						{
							objects.Add(vector, new Object(vector, getWeedForSeason(Game1.random, GetSeasonForLocation()), 1));
						}
						break;
					case 16:
						if (CanLoadPathObjectHere(vector))
						{
							objects.Add(vector, new Object(vector, (Game1.random.NextDouble() < 0.5) ? 343 : 450, 1));
						}
						break;
					case 17:
						if (CanLoadPathObjectHere(vector))
						{
							objects.Add(vector, new Object(vector, (Game1.random.NextDouble() < 0.5) ? 343 : 450, 1));
						}
						break;
					case 18:
						if (CanLoadPathObjectHere(vector))
						{
							objects.Add(vector, new Object(vector, (Game1.random.NextDouble() < 0.5) ? 294 : 295, 1));
						}
						break;
					}
				}
			}
		}

		public bool CanLoadPathObjectHere(Vector2 tile)
		{
			if (objects.ContainsKey(tile))
			{
				return false;
			}
			if (terrainFeatures.ContainsKey(tile))
			{
				return false;
			}
			if (getLargeTerrainFeatureAt((int)tile.X, (int)tile.Y) != null)
			{
				return false;
			}
			Vector2 vector = tile * 64f;
			vector.X += 32f;
			vector.Y += 32f;
			foreach (Furniture item in furniture)
			{
				if ((int)item.furniture_type != 12 && !item.isPassable() && item.getBoundingBox(item.tileLocation).Contains((int)vector.X, (int)vector.Y) && !item.AllowPlacementOnThisTile((int)tile.X, (int)tile.Y))
				{
					return false;
				}
			}
			return true;
		}

		public void loadObjects()
		{
			_startingCabinLocations.Clear();
			if (map == null)
			{
				return;
			}
			updateWarps();
			map.Properties.TryGetValue(Game1.currentSeason.Substring(0, 1).ToUpper() + Game1.currentSeason.Substring(1) + "_Objects", out var value);
			bool flag = false;
			foreach (Layer layer in map.Layers)
			{
				if (layer.Id.Equals("Paths"))
				{
					flag = true;
					break;
				}
			}
			map.Properties.TryGetValue("Trees", out var value2);
			if (value2 != null)
			{
				string[] array = value2.ToString().Split(' ');
				for (int i = 0; i < array.Length; i += 3)
				{
					int num = Convert.ToInt32(array[i]);
					int num2 = Convert.ToInt32(array[i + 1]);
					int which = Convert.ToInt32(array[i + 2]) + 1;
					terrainFeatures.Add(new Vector2(num, num2), new Tree(which, 5));
				}
			}
			if (this is FishShop || this is SeedShop)
			{
				updateDoors();
			}
			if (!(((bool)isOutdoors || name.Equals("BathHouse_Entry") || (bool)treatAsOutdoors || map.Properties.ContainsKey("forceLoadObjects")) && flag))
			{
				return;
			}
			for (int j = 0; j < map.Layers[0].LayerWidth; j++)
			{
				for (int k = 0; k < map.Layers[0].LayerHeight; k++)
				{
					Tile tile = map.GetLayer("Paths").Tiles[j, k];
					if (tile == null)
					{
						continue;
					}
					Vector2 vector = new Vector2(j, k);
					int num3 = -1;
					switch (tile.TileIndex)
					{
					case 9:
						num3 = 1;
						if (Game1.currentSeason.Equals("winter"))
						{
							num3 += 3;
						}
						break;
					case 10:
						num3 = 2;
						if (Game1.currentSeason.Equals("winter"))
						{
							num3 += 3;
						}
						break;
					case 11:
						num3 = 3;
						break;
					case 12:
						num3 = 6;
						break;
					case 31:
						num3 = 9;
						break;
					case 32:
						num3 = 8;
						break;
					}
					if (num3 != -1)
					{
						if (GetFurnitureAt(vector) == null && !terrainFeatures.ContainsKey(vector) && !objects.ContainsKey(vector))
						{
							terrainFeatures.Add(vector, new Tree(num3, 5));
						}
						continue;
					}
					switch (tile.TileIndex)
					{
					case 13:
					case 14:
					case 15:
						if (!objects.ContainsKey(vector))
						{
							objects.Add(vector, new Object(vector, getWeedForSeason(Game1.random, GetSeasonForLocation()), 1));
						}
						break;
					case 16:
						if (!objects.ContainsKey(vector))
						{
							objects.Add(vector, new Object(vector, (Game1.random.NextDouble() < 0.5) ? 343 : 450, 1));
						}
						break;
					case 17:
						if (!objects.ContainsKey(vector))
						{
							objects.Add(vector, new Object(vector, (Game1.random.NextDouble() < 0.5) ? 343 : 450, 1));
						}
						break;
					case 18:
						if (!objects.ContainsKey(vector))
						{
							objects.Add(vector, new Object(vector, (Game1.random.NextDouble() < 0.5) ? 294 : 295, 1));
						}
						break;
					case 19:
						addResourceClumpAndRemoveUnderlyingTerrain(602, 2, 2, vector);
						break;
					case 20:
						addResourceClumpAndRemoveUnderlyingTerrain(672, 2, 2, vector);
						break;
					case 21:
						addResourceClumpAndRemoveUnderlyingTerrain(600, 2, 2, vector);
						break;
					case 22:
						if (!terrainFeatures.ContainsKey(vector))
						{
							terrainFeatures.Add(vector, new Grass(1, 3));
						}
						break;
					case 23:
						if (!terrainFeatures.ContainsKey(vector))
						{
							terrainFeatures.Add(vector, new Tree(Game1.random.Next(1, 4), Game1.random.Next(2, 4)));
						}
						break;
					case 24:
						if (!terrainFeatures.ContainsKey(vector))
						{
							largeTerrainFeatures.Add(new Bush(vector, 2, this));
						}
						break;
					case 25:
						if (!terrainFeatures.ContainsKey(vector))
						{
							largeTerrainFeatures.Add(new Bush(vector, 1, this));
						}
						break;
					case 26:
						if (!terrainFeatures.ContainsKey(vector))
						{
							largeTerrainFeatures.Add(new Bush(vector, 0, this));
						}
						break;
					case 33:
						if (!terrainFeatures.ContainsKey(vector))
						{
							largeTerrainFeatures.Add(new Bush(vector, 4, this));
						}
						break;
					case 27:
						changeMapProperties("BrookSounds", vector.X + " " + vector.Y + " 0");
						break;
					case 28:
					{
						string text = name;
						if (text == "BugLand")
						{
						}
						break;
					}
					case 29:
					case 30:
					{
						if (Game1.startingCabins <= 0)
						{
							break;
						}
						PropertyValue value3 = null;
						tile.Properties.TryGetValue("Order", out value3);
						if (value3 != null)
						{
							int num4 = int.Parse(value3.ToString());
							if (num4 <= Game1.startingCabins && ((tile.TileIndex == 29 && !Game1.cabinsSeparate) || (tile.TileIndex == 30 && Game1.cabinsSeparate)))
							{
								_startingCabinLocations.Add(vector);
							}
						}
						break;
					}
					}
				}
			}
			if (value != null && !Game1.eventUp)
			{
				spawnObjects();
			}
			updateDoors();
		}

		public void BuildStartingCabins()
		{
			if (_startingCabinLocations.Count > 0)
			{
				List<string> list = new List<string>();
				switch (Game1.whichFarm)
				{
				case 3:
				case 4:
					list.Add("Stone Cabin");
					list.Add("Log Cabin");
					list.Add("Plank Cabin");
					break;
				case 1:
					list.Add("Plank Cabin");
					list.Add("Log Cabin");
					list.Add("Stone Cabin");
					break;
				default:
				{
					bool flag = Game1.random.NextDouble() < 0.5;
					list.Add(flag ? "Log Cabin" : "Plank Cabin");
					list.Add("Stone Cabin");
					list.Add(flag ? "Plank Cabin" : "Log Cabin");
					break;
				}
				}
				for (int i = 0; i < _startingCabinLocations.Count; i++)
				{
					if (this is BuildableGameLocation)
					{
						clearArea((int)_startingCabinLocations[i].X, (int)_startingCabinLocations[i].Y, 5, 3);
						clearArea((int)_startingCabinLocations[i].X + 2, (int)_startingCabinLocations[i].Y + 3, 1, 1);
						setTileProperty((int)_startingCabinLocations[i].X + 2, (int)_startingCabinLocations[i].Y + 3, "Back", "NoSpawn", "All");
						BluePrint bluePrint = new BluePrint(list[i]);
						bluePrint.magical = true;
						Building building = new Building(bluePrint, _startingCabinLocations[i]);
						building.daysOfConstructionLeft.Value = 0;
						building.load();
						(this as BuildableGameLocation).buildStructure(building, _startingCabinLocations[i], Game1.player, skipSafetyChecks: true);
						building.removeOverlappingBushes(this);
					}
				}
			}
			_startingCabinLocations.Clear();
		}

		public void updateDoors()
		{
			doors.Clear();
			Layer layer = map.GetLayer("Buildings");
			for (int i = 0; i < layer.LayerWidth; i++)
			{
				for (int j = 0; j < layer.LayerHeight; j++)
				{
					if (layer.Tiles[i, j] == null)
					{
						continue;
					}
					PropertyValue value = null;
					layer.Tiles[i, j].Properties.TryGetValue("Action", out value);
					if (value != null && value.ToString().Contains("Warp"))
					{
						string[] array = value.ToString().Split(' ');
						if (array[0].Equals("WarpBoatTunnel"))
						{
							doors.Add(new Point(i, j), new NetString("BoatTunnel"));
						}
						if (array[0].Equals("WarpCommunityCenter"))
						{
							doors.Add(new Point(i, j), new NetString("CommunityCenter"));
						}
						else if (array[0].Equals("Warp_Sunroom_Door"))
						{
							doors.Add(new Point(i, j), new NetString("Sunroom"));
						}
						else if ((!name.Equals("Mountain") || i != 8 || j != 20) && array.Length > 3)
						{
							doors.Add(new Point(i, j), new NetString(array[3]));
						}
					}
				}
			}
		}

		private void clearArea(int startingX, int startingY, int width, int height)
		{
			for (int i = startingX; i < startingX + width; i++)
			{
				for (int j = startingY; j < startingY + height; j++)
				{
					removeEverythingExceptCharactersFromThisTile(i, j);
				}
			}
		}

		public bool isTerrainFeatureAt(int x, int y)
		{
			Vector2 key = new Vector2(x, y);
			if (terrainFeatures.ContainsKey(key) && !terrainFeatures[key].isPassable())
			{
				return true;
			}
			if (largeTerrainFeatures != null)
			{
				Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(x * 64, y * 64, 64, 64);
				foreach (LargeTerrainFeature largeTerrainFeature in largeTerrainFeatures)
				{
					if (largeTerrainFeature.getBoundingBox().Intersects(value))
					{
						return true;
					}
				}
			}
			return false;
		}

		public void loadLights()
		{
			if (((bool)isOutdoors && !Game1.isFestival() && !forceLoadPathLayerLights) || this is FarmHouse || this is IslandFarmHouse)
			{
				return;
			}
			bool flag = false;
			foreach (Layer layer in map.Layers)
			{
				if (layer.Id.Equals("Paths"))
				{
					flag = true;
					break;
				}
			}
			for (int i = 0; i < map.Layers[0].LayerWidth; i++)
			{
				for (int j = 0; j < map.Layers[0].LayerHeight; j++)
				{
					if (!isOutdoors && !map.Properties.ContainsKey("IgnoreLightingTiles"))
					{
						Tile tile = map.GetLayer("Front").Tiles[i, j];
						if (tile != null)
						{
							adjustMapLightPropertiesForLamp(tile.TileIndex, i, j, "Front");
						}
						tile = map.GetLayer("Buildings").Tiles[i, j];
						if (tile != null)
						{
							adjustMapLightPropertiesForLamp(tile.TileIndex, i, j, "Buildings");
						}
					}
					if (flag)
					{
						Tile tile = map.GetLayer("Paths").Tiles[i, j];
						if (tile != null)
						{
							adjustMapLightPropertiesForLamp(tile.TileIndex, i, j, "Paths");
						}
					}
				}
			}
		}

		public bool isFarmBuildingInterior()
		{
			if (this is AnimalHouse)
			{
				return true;
			}
			return false;
		}

		public virtual bool CanBeRemotedlyViewed()
		{
			return false;
		}

		protected void adjustMapLightPropertiesForLamp(int tile, int x, int y, string layer)
		{
			string tileSheetIDAt = getTileSheetIDAt(x, y, layer);
			if (isFarmBuildingInterior())
			{
				if (tileSheetIDAt == "Coop" || tileSheetIDAt == "barn")
				{
					switch (tile)
					{
					case 24:
						changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
						changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 26);
						changeMapProperties("WindowLight", x + " " + (y + 1) + " 4");
						changeMapProperties("WindowLight", x + " " + (y + 3) + " 4");
						break;
					case 25:
						changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
						changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 12);
						break;
					case 46:
						changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
						changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 53);
						break;
					}
				}
			}
			else if (tile == 8 && layer == "Paths")
			{
				changeMapProperties("Light", x + " " + y + " 4");
			}
			else
			{
				if (!(tileSheetIDAt == "indoor"))
				{
					return;
				}
				switch (tile)
				{
				case 1346:
					changeMapProperties("DayTiles", "Front " + x + " " + y + " " + tile);
					changeMapProperties("NightTiles", "Front " + x + " " + y + " " + 1347);
					changeMapProperties("DayTiles", "Buildings " + x + " " + (y + 1) + " " + 452);
					changeMapProperties("NightTiles", "Buildings " + x + " " + (y + 1) + " " + 453);
					changeMapProperties("Light", x + " " + y + " 4");
					break;
				case 480:
					changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
					changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 809);
					changeMapProperties("Light", x + " " + y + " 4");
					break;
				case 826:
					changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
					changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 827);
					changeMapProperties("Light", x + " " + y + " 4");
					break;
				case 1344:
					changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
					changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 1345);
					changeMapProperties("Light", x + " " + y + " 4");
					break;
				case 256:
					changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
					changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 1253);
					changeMapProperties("DayTiles", layer + " " + x + " " + (y + 1) + " " + 288);
					changeMapProperties("NightTiles", layer + " " + x + " " + (y + 1) + " " + 1285);
					changeMapProperties("WindowLight", x + " " + y + " 4");
					changeMapProperties("WindowLight", x + " " + (y + 1) + " 4");
					break;
				case 225:
					if (!name.Contains("BathHouse") && !name.Contains("Club") && (!name.Equals("SeedShop") || (x != 36 && x != 37)))
					{
						changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
						changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 1222);
						changeMapProperties("DayTiles", layer + " " + x + " " + (y + 1) + " " + 257);
						changeMapProperties("NightTiles", layer + " " + x + " " + (y + 1) + " " + 1254);
						changeMapProperties("WindowLight", x + " " + y + " 4");
						changeMapProperties("WindowLight", x + " " + (y + 1) + " 4");
					}
					break;
				}
			}
		}

		private void changeMapProperties(string propertyName, string toAdd)
		{
			try
			{
				bool flag = true;
				if (!map.Properties.ContainsKey(propertyName))
				{
					map.Properties.Add(propertyName, new PropertyValue(string.Empty));
					flag = false;
				}
				if (!map.Properties[propertyName].ToString().Contains(toAdd))
				{
					StringBuilder stringBuilder = new StringBuilder(map.Properties[propertyName].ToString());
					if (flag)
					{
						stringBuilder.Append(" ");
					}
					stringBuilder.Append(toAdd);
					map.Properties[propertyName] = new PropertyValue(stringBuilder.ToString());
				}
			}
			catch (Exception)
			{
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is GameLocation)
			{
				return Equals(obj as GameLocation);
			}
			return false;
		}

		public bool Equals(GameLocation other)
		{
			if (isStructure.Get() == other.isStructure.Get())
			{
				return string.Equals(NameOrUniqueName, other.NameOrUniqueName, StringComparison.Ordinal);
			}
			return false;
		}

		private void testToDrawShopIconAnim(SpriteBatch b)
		{
			if (TutorialManager.Instance == null || TutorialManager.Instance.shopLocationsVisited == null || Game1.CurrentEvent != null)
			{
				return;
			}
			if (!TutorialManager.Instance.shopLocationsVisited.Contains(TutorialShopLocation.AdventureGuild) && name == "AdventureGuild")
			{
				if (npcAtCounter("Marlon", 5, 11))
				{
					drawShopIconAnim(b, 5, 12);
				}
			}
			else if (!TutorialManager.Instance.shopLocationsVisited.Contains(TutorialShopLocation.AnimalShop) && name == "AnimalShop")
			{
				if (npcAtCounter("Marnie", 12, 14))
				{
					drawShopIconAnim(b, 12, 15);
				}
			}
			else if (!TutorialManager.Instance.shopLocationsVisited.Contains(TutorialShopLocation.ArchaeologyHouse) && name == "ArchaeologyHouse")
			{
				if (npcAtCounter("Gunther", 3, 8))
				{
					drawShopIconAnim(b, 3, 9);
				}
			}
			else if (!TutorialManager.Instance.shopLocationsVisited.Contains(TutorialShopLocation.Blacksmith) && name == "Blacksmith")
			{
				if (npcAtCounter("Clint", 3, 13))
				{
					drawShopIconAnim(b, 3, 14);
				}
			}
			else if (!TutorialManager.Instance.shopLocationsVisited.Contains(TutorialShopLocation.Club) && name == "Club" && Game1.player.hasClubCard)
			{
				drawShopIconAnim(b, 3, 10);
				drawShopIconAnim(b, 25, 3);
				drawShopIconAnim(b, 23, 10);
			}
			else if (!TutorialManager.Instance.shopLocationsVisited.Contains(TutorialShopLocation.SandyHouse) && name == "SandyHouse")
			{
				if (npcAtCounter("Sandy", 2, 5))
				{
					drawShopIconAnim(b, 2, 6);
				}
			}
			else if (!TutorialManager.Instance.shopLocationsVisited.Contains(TutorialShopLocation.FishShop) && name == "FishShop")
			{
				if (npcAtCounter("Willy", 5, 4))
				{
					drawShopIconAnim(b, 5, 5);
				}
			}
			else if (!TutorialManager.Instance.shopLocationsVisited.Contains(TutorialShopLocation.Hospital) && name == "Hospital")
			{
				if (npcAtCounter("Maru", 5, 15) || npcAtCounter("Harvey", 5, 15))
				{
					drawShopIconAnim(b, 5, 16);
				}
			}
			else if (!TutorialManager.Instance.shopLocationsVisited.Contains(TutorialShopLocation.JojaMart) && name == "JojaMart")
			{
				drawShopIconAnim(b, 10, 25);
				if (Game1.player.mailReceived.Contains("ccDoorUnlock"))
				{
					drawShopIconAnim(b, 21, 25);
				}
			}
			else if (!TutorialManager.Instance.shopLocationsVisited.Contains(TutorialShopLocation.Saloon) && name == "Saloon")
			{
				if (npcAtCounter("Gus", 9, 18) || npcAtCounter("Gus", 10, 18) || npcAtCounter("Gus", 11, 18) || npcAtCounter("Gus", 12, 18) || npcAtCounter("Gus", 13, 18) || npcAtCounter("Gus", 14, 18) || npcAtCounter("Gus", 15, 18) || npcAtCounter("Gus", 16, 18) || npcAtCounter("Gus", 17, 18) || npcAtCounter("Gus", 18, 18))
				{
					drawShopIconAnim(b, 10, 19);
				}
			}
			else if (!TutorialManager.Instance.shopLocationsVisited.Contains(TutorialShopLocation.ScienceHouse) && name == "ScienceHouse")
			{
				if (npcAtCounter("Robin", 8, 18))
				{
					drawShopIconAnim(b, 8, 19);
				}
			}
			else if (!TutorialManager.Instance.shopLocationsVisited.Contains(TutorialShopLocation.SeedShop) && name == "SeedShop")
			{
				if (npcAtCounter("Pierre", 4, 17))
				{
					drawShopIconAnim(b, 4, 18);
				}
			}
			else
			{
				if (TutorialManager.Instance.shopLocationsVisited.Contains(TutorialShopLocation.IceCreamStand) || !(name == "Town") || !(Game1.currentSeason == "summer"))
				{
					return;
				}
				NPC nPC = isCharacterAtTile(new Vector2(88f, 89f));
				if (nPC != null && !(nPC is Horse))
				{
					drawShopIconAnim(b, 88, 92);
					return;
				}
				nPC = isCharacterAtTile(new Vector2(88f, 90f));
				if (nPC != null && !(nPC is Horse))
				{
					drawShopIconAnim(b, 88, 92);
					return;
				}
				nPC = isCharacterAtTile(new Vector2(88f, 91f));
				if (nPC != null && !(nPC is Horse))
				{
					drawShopIconAnim(b, 88, 92);
				}
			}
		}

		private bool npcAtCounter(string name, int tileX, int tileY)
		{
			foreach (NPC character in characters)
			{
				if (character.name == name)
				{
					Vector2 tileLocation = character.getTileLocation();
					if ((int)tileLocation.X == tileX && (int)tileLocation.Y == tileY)
					{
						return true;
					}
				}
			}
			return false;
		}

		private void drawShopIconAnim(SpriteBatch b, int tileX, int tileY)
		{
			long num = DateTime.Now.Ticks - ticks;
			if (num > 4000000)
			{
				ticks = DateTime.Now.Ticks;
				drawFrameOne = !drawFrameOne;
			}
			if (drawFrameOne)
			{
				b.Draw(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(tileX * 64 - Game1.viewport.X, tileY * 64 - Game1.viewport.Y, 64, 64), new Microsoft.Xna.Framework.Rectangle(128, 208, 16, 16), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1E-06f);
			}
			else
			{
				b.Draw(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(tileX * 64 - Game1.viewport.X, tileY * 64 - Game1.viewport.Y, 64, 64), new Microsoft.Xna.Framework.Rectangle(144, 208, 16, 16), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1E-06f);
			}
		}

		public bool containsNPCAlready(NPC npc)
		{
			for (int i = 0; i < characters.Count; i++)
			{
				if (!(npc is Monster) && npc.Name == characters[i].Name)
				{
					return true;
				}
			}
			return false;
		}

		public void removeCharactersWithNullLocation()
		{
			for (int i = characters.Count - 1; i >= 0; i++)
			{
				if (characters[i] != null && characters[i].currentLocation == null)
				{
					characters.RemoveAt(i);
				}
			}
		}
	}
}
