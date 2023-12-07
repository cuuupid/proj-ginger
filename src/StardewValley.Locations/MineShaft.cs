using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Mobile;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley.Locations
{
	public class MineShaft : GameLocation
	{
		public const int mineFrostLevel = 40;

		public const int mineLavaLevel = 80;

		public const int upperArea = 0;

		public const int jungleArea = 10;

		public const int frostArea = 40;

		public const int lavaArea = 80;

		public const int desertArea = 121;

		public const int bottomOfMineLevel = 120;

		public const int quarryMineShaft = 77377;

		public const int numberOfLevelsPerArea = 40;

		public const int mineFeature_barrels = 0;

		public const int mineFeature_chests = 1;

		public const int mineFeature_coalCart = 2;

		public const int mineFeature_elevator = 3;

		public const double chanceForColoredGemstone = 0.008;

		public const double chanceForDiamond = 0.0005;

		public const double chanceForPrismaticShard = 0.0005;

		public const int monsterLimit = 30;

		public static SerializableDictionary<int, MineInfo> permanentMineChanges = new SerializableDictionary<int, MineInfo>();

		public static int numberOfCraftedStairsUsedThisRun;

		private Random mineRandom = new Random();

		private LocalizedContentManager mineLoader = Game1.content.CreateTemporary();

		private int timeUntilElevatorLightUp;

		[XmlIgnore]
		public int loadedMapNumber;

		private int fogTime;

		private NetBool isFogUp = new NetBool();

		public static int timeSinceLastMusic = 200000;

		private bool ladderHasSpawned;

		private bool ghostAdded;

		private bool loadedDarkArea;

		private bool isFallingDownShaft;

		private Vector2 fogPos;

		private readonly NetBool elevatorShouldDing = new NetBool();

		public readonly NetString mapImageSource = new NetString();

		private readonly NetInt netMineLevel = new NetInt();

		private readonly NetIntDelta netStonesLeftOnThisLevel = new NetIntDelta();

		private readonly NetVector2 netTileBeneathLadder = new NetVector2();

		private readonly NetVector2 netTileBeneathElevator = new NetVector2();

		private readonly NetPoint netElevatorLightSpot = new NetPoint();

		private readonly NetBool netIsSlimeArea = new NetBool();

		private readonly NetBool netIsMonsterArea = new NetBool();

		private readonly NetBool netIsTreasureRoom = new NetBool();

		private readonly NetBool netIsDinoArea = new NetBool();

		private readonly NetBool netIsQuarryArea = new NetBool();

		private readonly NetBool netAmbientFog = new NetBool();

		private readonly NetColor netLighting = new NetColor(Color.White);

		private readonly NetColor netFogColor = new NetColor();

		private readonly NetVector2Dictionary<bool, NetBool> createLadderAtEvent = new NetVector2Dictionary<bool, NetBool>();

		private readonly NetPointDictionary<bool, NetBool> createLadderDownEvent = new NetPointDictionary<bool, NetBool>();

		private float fogAlpha;

		[XmlIgnore]
		public static ICue bugLevelLoop;

		private readonly NetBool rainbowLights = new NetBool(value: false);

		private readonly NetBool isLightingDark = new NetBool(value: false);

		private LocalizedContentManager mapContent;

		public static List<MineShaft> activeMines = new List<MineShaft>();

		public static HashSet<int> mushroomLevelsGeneratedToday = new HashSet<int>();

		private int lastLevelsDownFallen;

		private Microsoft.Xna.Framework.Rectangle fogSource = new Microsoft.Xna.Framework.Rectangle(640, 0, 64, 64);

		private List<Vector2> brownSpots = new List<Vector2>();

		private int lifespan;

		public static int lowestLevelReached
		{
			get
			{
				if (Game1.netWorldState.Value.LowestMineLevelForOrder >= 0)
				{
					if (Game1.netWorldState.Value.LowestMineLevelForOrder == 120)
					{
						return Math.Max(Game1.netWorldState.Value.LowestMineLevelForOrder, Game1.netWorldState.Value.LowestMineLevelForOrder);
					}
					return Game1.netWorldState.Value.LowestMineLevelForOrder;
				}
				return Game1.netWorldState.Value.LowestMineLevel;
			}
			set
			{
				if (Game1.netWorldState.Value.LowestMineLevelForOrder >= 0 && value <= 120)
				{
					Game1.netWorldState.Value.LowestMineLevelForOrder = value;
				}
				else
				{
					Game1.netWorldState.Value.LowestMineLevel = value;
				}
			}
		}

		public int mineLevel
		{
			get
			{
				return netMineLevel;
			}
			set
			{
				netMineLevel.Value = value;
			}
		}

		private int stonesLeftOnThisLevel
		{
			get
			{
				return netStonesLeftOnThisLevel;
			}
			set
			{
				netStonesLeftOnThisLevel.Value = value;
			}
		}

		private Vector2 tileBeneathLadder
		{
			get
			{
				return netTileBeneathLadder;
			}
			set
			{
				netTileBeneathLadder.Value = value;
			}
		}

		private Vector2 tileBeneathElevator
		{
			get
			{
				return netTileBeneathElevator;
			}
			set
			{
				netTileBeneathElevator.Value = value;
			}
		}

		private Point ElevatorLightSpot
		{
			get
			{
				return netElevatorLightSpot;
			}
			set
			{
				netElevatorLightSpot.Value = value;
			}
		}

		private bool isSlimeArea
		{
			get
			{
				return netIsSlimeArea;
			}
			set
			{
				netIsSlimeArea.Value = value;
			}
		}

		private bool isDinoArea
		{
			get
			{
				return netIsDinoArea;
			}
			set
			{
				netIsDinoArea.Value = value;
			}
		}

		private bool isMonsterArea
		{
			get
			{
				return netIsMonsterArea;
			}
			set
			{
				netIsMonsterArea.Value = value;
			}
		}

		private bool isQuarryArea
		{
			get
			{
				return netIsQuarryArea;
			}
			set
			{
				netIsQuarryArea.Value = value;
			}
		}

		private bool ambientFog
		{
			get
			{
				return netAmbientFog;
			}
			set
			{
				netAmbientFog.Value = value;
			}
		}

		private Color lighting
		{
			get
			{
				return netLighting;
			}
			set
			{
				netLighting.Value = value;
			}
		}

		private Color fogColor
		{
			get
			{
				return netFogColor;
			}
			set
			{
				netFogColor.Value = value;
			}
		}

		public int EnemyCount => characters.OfType<Monster>().Count();

		public MineShaft()
		{
			name.Value = "UndergroundMine" + mineLevel;
			mapContent = Game1.game1.xTileContent.CreateTemporary();
		}

		public override bool CanPlaceThisFurnitureHere(Furniture furniture)
		{
			return false;
		}

		public MineShaft(int level)
			: this()
		{
			mineLevel = level;
			name.Value = "UndergroundMine" + level;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(netMineLevel, netStonesLeftOnThisLevel, netTileBeneathLadder, netTileBeneathElevator, netElevatorLightSpot, netIsSlimeArea, netIsMonsterArea, netIsTreasureRoom, netIsDinoArea, netIsQuarryArea, netAmbientFog, netLighting, netFogColor, createLadderAtEvent, createLadderDownEvent, mapImageSource, rainbowLights, isLightingDark, elevatorShouldDing, isFogUp);
			isFogUp.fieldChangeEvent += delegate(NetBool field, bool oldValue, bool newValue)
			{
				if (!oldValue && newValue)
				{
					if (Game1.currentLocation == this)
					{
						Game1.changeMusicTrack("none");
					}
					if (Game1.IsClient)
					{
						fogTime = 35000;
					}
				}
				else if (!newValue)
				{
					fogTime = 0;
				}
			};
			createLadderAtEvent.OnValueAdded += delegate(Vector2 v, bool b)
			{
				doCreateLadderAt(v);
			};
			createLadderDownEvent.OnValueAdded += delegate(Point p, bool b)
			{
				doCreateLadderDown(p, b);
			};
			mapImageSource.fieldChangeEvent += delegate(NetString field, string oldValue, string newValue)
			{
				if (newValue != null && newValue != oldValue)
				{
					base.Map.TileSheets[0].ImageSource = newValue;
					base.Map.LoadTileSheets(Game1.mapDisplayDevice);
				}
			};
		}

		public override bool AllowMapModificationsInResetState()
		{
			return true;
		}

		protected override LocalizedContentManager getMapLoader()
		{
			return mapContent;
		}

		private void setElevatorLit()
		{
			setMapTileIndex(ElevatorLightSpot.X, ElevatorLightSpot.Y, 48, "Buildings");
			Game1.currentLightSources.Add(new LightSource(4, new Vector2(ElevatorLightSpot.X, ElevatorLightSpot.Y) * 64f, 2f, Color.Black, ElevatorLightSpot.X + ElevatorLightSpot.Y * 1000, LightSource.LightContext.None, 0L));
			elevatorShouldDing.Value = false;
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			bool flag = Game1.currentLocation == this;
			if ((Game1.isMusicContextActiveButNotPlaying() || Game1.getMusicTrackName().Contains("Ambient")) && Game1.random.NextDouble() < 0.00195)
			{
				localSound("cavedrip");
			}
			if (timeUntilElevatorLightUp > 0)
			{
				timeUntilElevatorLightUp -= time.ElapsedGameTime.Milliseconds;
				if (timeUntilElevatorLightUp <= 0)
				{
					localSound("crystal");
					setElevatorLit();
				}
			}
			if (flag)
			{
				if ((bool)isFogUp && Game1.shouldTimePass())
				{
					if (Game1.soundBank != null && (bugLevelLoop == null || bugLevelLoop.IsStopped))
					{
						bugLevelLoop = Game1.soundBank.GetCue("bugLevelLoop");
						bugLevelLoop.Play();
					}
					if (fogAlpha < 1f)
					{
						if (Game1.shouldTimePass())
						{
							fogAlpha += 0.01f;
						}
						if (bugLevelLoop != null && Game1.soundBank != null)
						{
							bugLevelLoop.SetVariable("Volume", fogAlpha * 100f);
							bugLevelLoop.SetVariable("Frequency", fogAlpha * 25f);
						}
					}
					else if (bugLevelLoop != null && Game1.soundBank != null)
					{
						float num = (float)Math.Max(0.0, Math.Min(100.0, Math.Sin((double)((float)fogTime / 10000f) % (Math.PI * 200.0))));
						bugLevelLoop.SetVariable("Frequency", Math.Max(0f, Math.Min(100f, fogAlpha * 25f + num * 10f)));
					}
				}
				else if (fogAlpha > 0f)
				{
					if (Game1.shouldTimePass())
					{
						fogAlpha -= 0.01f;
					}
					if (bugLevelLoop != null)
					{
						bugLevelLoop.SetVariable("Volume", fogAlpha * 100f);
						bugLevelLoop.SetVariable("Frequency", Math.Max(0f, bugLevelLoop.GetVariable("Frequency") - 0.01f));
						if (fogAlpha <= 0f)
						{
							bugLevelLoop.Stop(AudioStopOptions.Immediate);
							bugLevelLoop = null;
						}
					}
				}
				else
				{
					_ = ambientFog;
				}
				if (fogAlpha > 0f || ambientFog)
				{
					fogPos = Game1.updateFloatingObjectPositionForMovement(current: new Vector2(Game1.viewport.X, Game1.viewport.Y), w: fogPos, previous: Game1.previousViewportPosition, speed: -1f);
					fogPos.X = (fogPos.X + 0.5f) % 256f;
					fogPos.Y = (fogPos.Y + 0.5f) % 256f;
				}
			}
			base.UpdateWhenCurrentLocation(time);
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			if (bugLevelLoop != null)
			{
				bugLevelLoop.Stop(AudioStopOptions.Immediate);
				bugLevelLoop = null;
			}
			if (!Game1.IsMultiplayer && mineLevel == 20)
			{
				Game1.changeMusicTrack("none");
			}
		}

		public override int getExtraMillisecondsPerInGameMinuteForThisLocation()
		{
			if (Game1.IsMultiplayer)
			{
				return base.getExtraMillisecondsPerInGameMinuteForThisLocation();
			}
			if (getMineArea() != 121)
			{
				return 0;
			}
			return 2000;
		}

		public Vector2 mineEntrancePosition(Farmer who)
		{
			if (!who.ridingMineElevator || tileBeneathElevator.Equals(Vector2.Zero))
			{
				return tileBeneathLadder;
			}
			return tileBeneathElevator;
		}

		private void generateContents()
		{
			ladderHasSpawned = false;
			loadLevel(mineLevel);
			chooseLevelType();
			findLadder();
			populateLevel();
		}

		public void chooseLevelType()
		{
			fogTime = 0;
			if (bugLevelLoop != null)
			{
				bugLevelLoop.Stop(AudioStopOptions.Immediate);
				bugLevelLoop = null;
			}
			ambientFog = false;
			rainbowLights.Value = false;
			isLightingDark.Value = false;
			Random random = new Random((int)Game1.stats.DaysPlayed * mineLevel + 4 * mineLevel + (int)Game1.uniqueIDForThisGame / 2);
			lighting = new Color(80, 80, 40);
			if (getMineArea() == 80)
			{
				lighting = new Color(100, 100, 50);
			}
			if (GetAdditionalDifficulty() > 0)
			{
				if (getMineArea() == 40)
				{
					lighting = new Color(230, 200, 90);
					ambientFog = true;
					fogColor = new Color(0, 80, 255) * 0.55f;
					if (mineLevel < 50)
					{
						lighting = new Color(100, 80, 40);
						ambientFog = false;
					}
				}
			}
			else if (random.NextDouble() < 0.3 && mineLevel > 2)
			{
				isLightingDark.Value = true;
				lighting = new Color(120, 120, 40);
				if (random.NextDouble() < 0.3)
				{
					lighting = new Color(150, 150, 60);
				}
			}
			if (random.NextDouble() < 0.15 && mineLevel > 5 && mineLevel != 120)
			{
				isLightingDark.Value = true;
				switch (getMineArea())
				{
				case 0:
				case 10:
					lighting = new Color(110, 110, 70);
					break;
				case 40:
					lighting = Color.Black;
					if (GetAdditionalDifficulty() > 0)
					{
						lighting = new Color(237, 212, 185);
					}
					break;
				case 80:
					lighting = new Color(90, 130, 70);
					break;
				}
			}
			if (random.NextDouble() < 0.035 && getMineArea() == 80 && mineLevel % 5 != 0 && !mushroomLevelsGeneratedToday.Contains(mineLevel))
			{
				rainbowLights.Value = true;
				mushroomLevelsGeneratedToday.Add(mineLevel);
			}
			if (isDarkArea() && mineLevel < 120)
			{
				isLightingDark.Value = true;
				lighting = ((getMineArea() == 80) ? new Color(70, 100, 100) : new Color(150, 150, 120));
				if (getMineArea() == 0)
				{
					ambientFog = true;
					fogColor = Color.Black;
				}
			}
			if (mineLevel == 100)
			{
				lighting = new Color(140, 140, 80);
			}
			if (getMineArea() == 121)
			{
				lighting = new Color(110, 110, 40);
				if (random.NextDouble() < 0.05)
				{
					lighting = ((random.NextDouble() < 0.5) ? new Color(30, 30, 0) : new Color(150, 150, 50));
				}
			}
			if (getMineArea() == 77377)
			{
				isLightingDark.Value = false;
				rainbowLights.Value = false;
				ambientFog = true;
				fogColor = Color.White * 0.4f;
				lighting = new Color(80, 80, 30);
			}
		}

		private bool canAdd(int typeOfFeature, int numberSoFar)
		{
			if (permanentMineChanges.ContainsKey(mineLevel))
			{
				switch (typeOfFeature)
				{
				case 0:
					return permanentMineChanges[mineLevel].platformContainersLeft > numberSoFar;
				case 1:
					return permanentMineChanges[mineLevel].chestsLeft > numberSoFar;
				case 2:
					return permanentMineChanges[mineLevel].coalCartsLeft > numberSoFar;
				case 3:
					return permanentMineChanges[mineLevel].elevator == 0;
				}
			}
			return true;
		}

		public void updateMineLevelData(int feature, int amount = 1)
		{
			if (!permanentMineChanges.ContainsKey(mineLevel))
			{
				permanentMineChanges.Add(mineLevel, new MineInfo());
			}
			switch (feature)
			{
			case 0:
				permanentMineChanges[mineLevel].platformContainersLeft += amount;
				break;
			case 1:
				permanentMineChanges[mineLevel].chestsLeft += amount;
				break;
			case 2:
				permanentMineChanges[mineLevel].coalCartsLeft += amount;
				break;
			case 3:
				permanentMineChanges[mineLevel].elevator += amount;
				break;
			}
		}

		public void chestConsumed()
		{
			Game1.player.chestConsumedMineLevels[mineLevel] = true;
		}

		public bool isLevelSlimeArea()
		{
			return isSlimeArea;
		}

		public void checkForMapAlterations(int x, int y)
		{
			Tile tile = map.GetLayer("Buildings").Tiles[x, y];
			if (tile != null)
			{
				int tileIndex = tile.TileIndex;
				if (tileIndex == 194 && !canAdd(2, 0))
				{
					setMapTileIndex(x, y, 195, "Buildings");
					setMapTileIndex(x, y - 1, 179, "Front");
				}
			}
		}

		public void findLadder()
		{
			int num = 0;
			int num2 = -1;
			tileBeneathElevator = Vector2.Zero;
			bool flag = mineLevel % 20 == 0;
			lightGlows.Clear();
			for (int i = 0; i < map.GetLayer("Buildings").LayerHeight; i++)
			{
				for (int j = 0; j < map.GetLayer("Buildings").LayerWidth; j++)
				{
					if (map.GetLayer("Buildings").Tiles[j, i] != null)
					{
						num2 = map.GetLayer("Buildings").Tiles[j, i].TileIndex;
						switch (num2)
						{
						case 115:
							tileBeneathLadder = new Vector2(j, i + 1);
							sharedLights[j + i * 999] = new LightSource(4, new Vector2(j, i - 2) * 64f + new Vector2(32f, 0f), 0.25f, new Color(0, 20, 50), j + i * 999, LightSource.LightContext.None, 0L);
							sharedLights[j + i * 998] = new LightSource(4, new Vector2(j, i - 1) * 64f + new Vector2(32f, 0f), 0.5f, new Color(0, 20, 50), j + i * 998, LightSource.LightContext.None, 0L);
							sharedLights[j + i * 997] = new LightSource(4, new Vector2(j, i) * 64f + new Vector2(32f, 0f), 0.75f, new Color(0, 20, 50), j + i * 997, LightSource.LightContext.None, 0L);
							sharedLights[j + i * 1000] = new LightSource(4, new Vector2(j, i + 1) * 64f + new Vector2(32f, 0f), 1f, new Color(0, 20, 50), j + i * 1000, LightSource.LightContext.None, 0L);
							num++;
							break;
						case 112:
							tileBeneathElevator = new Vector2(j, i + 1);
							num++;
							break;
						}
						if (lighting.Equals(Color.White) && num == 2 && !flag)
						{
							return;
						}
						if (!lighting.Equals(Color.White) && (num2 == 97 || num2 == 113 || num2 == 65 || num2 == 66 || num2 == 81 || num2 == 82 || num2 == 48))
						{
							sharedLights[j + i * 1000] = new LightSource(4, new Vector2(j, i) * 64f, 2.5f, new Color(0, 50, 100), j + i * 1000, LightSource.LightContext.None, 0L);
							switch (num2)
							{
							case 66:
								lightGlows.Add(new Vector2(j, i) * 64f + new Vector2(0f, 64f));
								break;
							case 97:
							case 113:
								lightGlows.Add(new Vector2(j, i) * 64f + new Vector2(32f, 32f));
								break;
							}
						}
					}
					if (Game1.IsMasterGame && doesTileHaveProperty(j, i, "Water", "Back") != null && getMineArea() == 80 && Game1.random.NextDouble() < 0.1)
					{
						sharedLights[j + i * 1000] = new LightSource(4, new Vector2(j, i) * 64f, 2f, new Color(0, 220, 220), j + i * 1000, LightSource.LightContext.None, 0L);
					}
				}
			}
			if (isFallingDownShaft)
			{
				Vector2 v = default(Vector2);
				while (!isTileClearForMineObjects(v))
				{
					v.X = Game1.random.Next(1, map.Layers[0].LayerWidth);
					v.Y = Game1.random.Next(1, map.Layers[0].LayerHeight);
				}
				tileBeneathLadder = v;
				Game1.player.showFrame(5);
			}
			isFallingDownShaft = false;
		}

		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			if (mustKillAllMonstersToAdvance() && EnemyCount == 0)
			{
				Vector2 vector = new Vector2((int)tileBeneathLadder.X, (int)tileBeneathLadder.Y);
				if (getTileIndexAt(Utility.Vector2ToPoint(vector), "Buildings") == -1)
				{
					createLadderAt(vector, "newArtifact");
					if (mustKillAllMonstersToAdvance() && Game1.player.currentLocation == this)
					{
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MineShaft.cs.9484"));
					}
				}
			}
			if ((bool)isFogUp || map == null || mineLevel % 5 == 0 || !(Game1.random.NextDouble() < 0.1) || AnyOnlineFarmerHasBuff(23))
			{
				return;
			}
			if (mineLevel > 10 && !mustKillAllMonstersToAdvance() && Game1.random.NextDouble() < 0.11 && getMineArea() != 77377)
			{
				isFogUp.Value = true;
				fogTime = 35000 + Game1.random.Next(-5, 6) * 1000;
				switch (getMineArea())
				{
				case 121:
					fogColor = Color.BlueViolet * 1f;
					break;
				case 0:
				case 10:
					if (GetAdditionalDifficulty() > 0)
					{
						fogColor = (isDarkArea() ? new Color(255, 150, 0) : (Color.Cyan * 0.75f));
					}
					else
					{
						fogColor = (isDarkArea() ? Color.Khaki : (Color.Green * 0.75f));
					}
					break;
				case 40:
					fogColor = Color.Blue * 0.75f;
					break;
				case 80:
					fogColor = Color.Red * 0.5f;
					break;
				}
			}
			else
			{
				spawnFlyingMonsterOffScreen();
			}
		}

		public void spawnFlyingMonsterOffScreen()
		{
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
				zero.X -= Game1.viewport.Width / 64;
			}
			switch (getMineArea())
			{
			case 0:
				if (mineLevel > 10 && isDarkArea())
				{
					characters.Add(BuffMonsterIfNecessary(new Bat(zero * 64f, mineLevel)
					{
						focusedOnFarmers = true
					}));
					playSound("batScreech");
				}
				break;
			case 10:
				if (GetAdditionalDifficulty() > 0)
				{
					characters.Add(BuffMonsterIfNecessary(new BlueSquid(zero * 64f)
					{
						focusedOnFarmers = true
					}));
				}
				else
				{
					characters.Add(BuffMonsterIfNecessary(new Fly(zero * 64f)
					{
						focusedOnFarmers = true
					}));
				}
				break;
			case 40:
				characters.Add(BuffMonsterIfNecessary(new Bat(zero * 64f, mineLevel)
				{
					focusedOnFarmers = true
				}));
				playSound("batScreech");
				break;
			case 80:
				characters.Add(BuffMonsterIfNecessary(new Bat(zero * 64f, mineLevel)
				{
					focusedOnFarmers = true
				}));
				playSound("batScreech");
				break;
			case 121:
				if (mineLevel < 171 || Game1.random.NextDouble() < 0.5)
				{
					characters.Add(BuffMonsterIfNecessary((GetAdditionalDifficulty() > 0) ? new Serpent(zero * 64f, "Royal Serpent")
					{
						focusedOnFarmers = true
					} : new Serpent(zero * 64f)
					{
						focusedOnFarmers = true
					}));
					playSound("serpentDie");
				}
				else
				{
					characters.Add(BuffMonsterIfNecessary(new Bat(zero * 64f, mineLevel)
					{
						focusedOnFarmers = true
					}));
					playSound("batScreech");
				}
				break;
			case 77377:
				characters.Add(new Bat(zero * 64f, 77377)
				{
					focusedOnFarmers = true
				});
				playSound("rockGolemHit");
				break;
			}
		}

		public override void drawLightGlows(SpriteBatch b)
		{
			Color color;
			switch (getMineArea())
			{
			case 0:
				color = (isDarkArea() ? (Color.PaleGoldenrod * 0.5f) : (Color.PaleGoldenrod * 0.33f));
				break;
			case 80:
				color = (isDarkArea() ? (Color.Pink * 0.4f) : (Color.Red * 0.33f));
				break;
			case 40:
				color = Color.White * 0.65f;
				if (GetAdditionalDifficulty() > 0)
				{
					color = ((mineLevel % 40 >= 30) ? (new Color(220, 240, 255) * 0.8f) : (new Color(230, 225, 100) * 0.8f));
				}
				break;
			case 121:
				color = Color.White * 0.8f;
				if (isDinoArea)
				{
					color = Color.Orange * 0.5f;
				}
				break;
			default:
				color = Color.PaleGoldenrod * 0.33f;
				break;
			}
			foreach (Vector2 lightGlow in lightGlows)
			{
				if ((bool)rainbowLights)
				{
					switch ((int)(lightGlow.X / 64f + lightGlow.Y / 64f) % 4)
					{
					case 0:
						color = Color.Red * 0.5f;
						break;
					case 1:
						color = Color.Yellow * 0.5f;
						break;
					case 2:
						color = Color.Cyan * 0.33f;
						break;
					case 3:
						color = Color.Lime * 0.45f;
						break;
					}
				}
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, lightGlow), new Microsoft.Xna.Framework.Rectangle(88, 1779, 30, 30), color, 0f, new Vector2(15f, 15f), 8f + (float)(96.0 * Math.Sin((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(lightGlow.X * 777f) + (double)(lightGlow.Y * 9746f)) % 3140.0 / 1000.0) / 50.0), SpriteEffects.None, 1f);
			}
		}

		public Monster BuffMonsterIfNecessary(Monster monster)
		{
			if (monster != null && monster.GetBaseDifficultyLevel() < GetAdditionalDifficulty())
			{
				monster.BuffForAdditionalDifficulty(GetAdditionalDifficulty() - monster.GetBaseDifficultyLevel());
				if (monster is GreenSlime)
				{
					if (mineLevel < 40)
					{
						(monster as GreenSlime).color.Value = new Color(Game1.random.Next(40, 70), Game1.random.Next(100, 190), 255);
					}
					else if (mineLevel < 80)
					{
						(monster as GreenSlime).color.Value = new Color(0, 180, 120);
					}
					else if (mineLevel < 120)
					{
						(monster as GreenSlime).color.Value = new Color(Game1.random.Next(180, 250), 20, 120);
					}
					else
					{
						(monster as GreenSlime).color.Value = new Color(Game1.random.Next(120, 180), 20, 255);
					}
				}
				try
				{
					string text = string.Concat(monster.Sprite.textureName, "_dangerous");
					if (Game1.content.Load<Texture2D>(text) != null)
					{
						monster.Sprite.LoadTexture(text);
						return monster;
					}
					return monster;
				}
				catch (Exception)
				{
					return monster;
				}
			}
			return monster;
		}

		public override Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			int num = -1;
			double num2 = 1.0;
			num2 += 0.4 * (double)who.FishingLevel;
			num2 += (double)waterDepth * 0.1;
			if (who != null && who.CurrentTool is FishingRod && (who.CurrentTool as FishingRod).getBobberAttachmentIndex() == 856)
			{
				num2 += 5.0;
			}
			switch (getMineArea())
			{
			case 0:
			case 10:
				num2 += (double)((bait == 689) ? 3 : 0);
				if (Game1.random.NextDouble() < 0.02 + 0.01 * num2)
				{
					num = 158;
				}
				break;
			case 40:
				num2 += (double)((bait == 682) ? 3 : 0);
				if (Game1.random.NextDouble() < 0.015 + 0.009 * num2)
				{
					num = 161;
				}
				break;
			case 80:
				num2 += (double)((bait == 684) ? 3 : 0);
				if (Game1.random.NextDouble() < 0.01 + 0.008 * num2)
				{
					num = 162;
				}
				break;
			}
			int quality = 0;
			if (Game1.random.NextDouble() < (double)((float)who.FishingLevel / 10f))
			{
				quality = 1;
			}
			if (Game1.random.NextDouble() < (double)((float)who.FishingLevel / 50f + (float)who.LuckLevel / 100f))
			{
				quality = 2;
			}
			if (num != -1)
			{
				return new Object(num, 1, isRecipe: false, -1, quality);
			}
			if (getMineArea() == 80)
			{
				return new Object(Game1.random.Next(167, 173), 1);
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "UndergroundMine");
		}

		private void adjustLevelChances(ref double stoneChance, ref double monsterChance, ref double itemChance, ref double gemStoneChance)
		{
			if (mineLevel == 1)
			{
				monsterChance = 0.0;
				itemChance = 0.0;
				gemStoneChance = 0.0;
			}
			else if (mineLevel % 5 == 0 && getMineArea() != 121)
			{
				itemChance = 0.0;
				gemStoneChance = 0.0;
				if (mineLevel % 10 == 0)
				{
					monsterChance = 0.0;
				}
			}
			if (mustKillAllMonstersToAdvance())
			{
				monsterChance = 0.025;
				itemChance = 0.001;
				stoneChance = 0.0;
				gemStoneChance = 0.0;
				if (isDinoArea)
				{
					itemChance *= 4.0;
				}
			}
			monsterChance += 0.02 * (double)GetAdditionalDifficulty();
			bool flag = false;
			bool flag2 = false;
			flag = AnyOnlineFarmerHasBuff(23);
			flag2 = AnyOnlineFarmerHasBuff(24);
			if (flag && getMineArea() != 121)
			{
				if (!flag2)
				{
					monsterChance = 0.0;
				}
			}
			else if (flag2)
			{
				monsterChance *= 2.0;
			}
			gemStoneChance /= 2.0;
			if (isQuarryArea || getMineArea() == 77377)
			{
				gemStoneChance = 0.001;
				itemChance = 0.0001;
				stoneChance *= 2.0;
				monsterChance = 0.02;
			}
			if (GetAdditionalDifficulty() > 0 && getMineArea() == 40)
			{
				monsterChance *= 0.6600000262260437;
			}
		}

		public bool AnyOnlineFarmerHasBuff(int which_buff)
		{
			if (which_buff == 23 && GetAdditionalDifficulty() > 0)
			{
				return false;
			}
			foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
			{
				if (onlineFarmer.hasBuff(which_buff))
				{
					return true;
				}
			}
			return false;
		}

		private void populateLevel()
		{
			objects.Clear();
			terrainFeatures.Clear();
			resourceClumps.Clear();
			debris.Clear();
			characters.Clear();
			ghostAdded = false;
			stonesLeftOnThisLevel = 0;
			double stoneChance = (double)mineRandom.Next(10, 30) / 100.0;
			double monsterChance = 0.002 + (double)mineRandom.Next(200) / 10000.0;
			double itemChance = 0.0025;
			double gemStoneChance = 0.003;
			adjustLevelChances(ref stoneChance, ref monsterChance, ref itemChance, ref gemStoneChance);
			int num = 0;
			bool flag = !permanentMineChanges.ContainsKey(mineLevel);
			if (mineLevel > 1 && mineLevel % 5 != 0 && (mineRandom.NextDouble() < 0.5 || isDinoArea))
			{
				int num2 = mineRandom.Next(5) + (int)(Game1.player.team.AverageDailyLuck(Game1.currentLocation) * 20.0);
				if (isDinoArea)
				{
					num2 += map.Layers[0].LayerWidth * map.Layers[0].LayerHeight / 40;
				}
				for (int i = 0; i < num2; i++)
				{
					Point point;
					Point point2;
					if (mineRandom.NextDouble() < 0.33)
					{
						point = new Point(mineRandom.Next(map.GetLayer("Back").LayerWidth), 0);
						point2 = new Point(0, 1);
					}
					else if (mineRandom.NextDouble() < 0.5)
					{
						point = new Point(0, mineRandom.Next(map.GetLayer("Back").LayerHeight));
						point2 = new Point(1, 0);
					}
					else
					{
						point = new Point(map.GetLayer("Back").LayerWidth - 1, mineRandom.Next(map.GetLayer("Back").LayerHeight));
						point2 = new Point(-1, 0);
					}
					while (isTileOnMap(point.X, point.Y))
					{
						point.X += point2.X;
						point.Y += point2.Y;
						if (isTileClearForMineObjects(point.X, point.Y))
						{
							Vector2 vector = new Vector2(point.X, point.Y);
							if (isDinoArea)
							{
								terrainFeatures.Add(vector, new CosmeticPlant(mineRandom.Next(3)));
							}
							else if (!mustKillAllMonstersToAdvance())
							{
								objects.Add(vector, new BreakableContainer(vector, 118, this));
							}
							break;
						}
					}
				}
			}
			bool flag2 = false;
			if (mineLevel % 10 != 0 || (getMineArea() == 121 && mineLevel != 220 && !netIsTreasureRoom.Value))
			{
				for (int j = 0; j < map.GetLayer("Back").LayerWidth; j++)
				{
					for (int k = 0; k < map.GetLayer("Back").LayerHeight; k++)
					{
						checkForMapAlterations(j, k);
						if (isTileClearForMineObjects(j, k))
						{
							if (mineRandom.NextDouble() <= stoneChance)
							{
								Vector2 vector2 = new Vector2(j, k);
								if (base.Objects.ContainsKey(vector2))
								{
									continue;
								}
								if (getMineArea() == 40 && mineRandom.NextDouble() < 0.15)
								{
									int parentSheetIndex = mineRandom.Next(319, 322);
									if (GetAdditionalDifficulty() > 0 && mineLevel % 40 < 30)
									{
										parentSheetIndex = mineRandom.Next(313, 316);
									}
									base.Objects.Add(vector2, new Object(vector2, parentSheetIndex, "Weeds", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
									{
										Fragility = 2,
										CanBeGrabbed = true
									});
								}
								else if ((bool)rainbowLights && mineRandom.NextDouble() < 0.55)
								{
									if (mineRandom.NextDouble() < 0.25)
									{
										int parentSheetIndex2 = 404;
										switch (mineRandom.Next(5))
										{
										case 0:
											parentSheetIndex2 = 422;
											break;
										case 1:
											parentSheetIndex2 = 420;
											break;
										case 2:
											parentSheetIndex2 = 420;
											break;
										case 3:
											parentSheetIndex2 = 420;
											break;
										case 4:
											parentSheetIndex2 = 420;
											break;
										}
										base.Objects.Add(vector2, new Object(parentSheetIndex2, 1)
										{
											IsSpawnedObject = true
										});
									}
								}
								else
								{
									Object @object = chooseStoneType(0.001, 5E-05, gemStoneChance, vector2);
									if (@object != null)
									{
										base.Objects.Add(vector2, @object);
										stonesLeftOnThisLevel++;
									}
								}
							}
							else if (mineRandom.NextDouble() <= monsterChance && getDistanceFromStart(j, k) > 5f)
							{
								Monster monster = BuffMonsterIfNecessary(getMonsterForThisLevel(mineLevel, j, k));
								if (monster is GreenSlime && !flag2 && Game1.random.NextDouble() <= 0.012 + Game1.player.team.AverageDailyLuck() / 10.0 && Game1.player.team.SpecialOrderActive("Wizard2"))
								{
									(monster as GreenSlime).makePrismatic();
									flag2 = true;
								}
								if (monster is GreenSlime && GetAdditionalDifficulty() > 0 && mineRandom.NextDouble() < (double)Math.Min((float)GetAdditionalDifficulty() * 0.1f, 0.5f))
								{
									if (mineRandom.NextDouble() < 0.009999999776482582)
									{
										(monster as GreenSlime).stackedSlimes.Value = 4;
									}
									else
									{
										(monster as GreenSlime).stackedSlimes.Value = 2;
									}
								}
								if (monster is Leaper)
								{
									float num3 = (float)(GetAdditionalDifficulty() + 1) * 0.3f;
									if (mineRandom.NextDouble() < (double)num3)
									{
										tryToAddMonster(BuffMonsterIfNecessary(new Leaper(Vector2.Zero)), j - 1, k);
									}
									if (mineRandom.NextDouble() < (double)num3)
									{
										tryToAddMonster(BuffMonsterIfNecessary(new Leaper(Vector2.Zero)), j + 1, k);
									}
									if (mineRandom.NextDouble() < (double)num3)
									{
										tryToAddMonster(BuffMonsterIfNecessary(new Leaper(Vector2.Zero)), j, k - 1);
									}
									if (mineRandom.NextDouble() < (double)num3)
									{
										tryToAddMonster(BuffMonsterIfNecessary(new Leaper(Vector2.Zero)), j, k + 1);
									}
								}
								if (monster is Grub)
								{
									if (mineRandom.NextDouble() < 0.4)
									{
										tryToAddMonster(BuffMonsterIfNecessary(new Grub(Vector2.Zero)), j - 1, k);
									}
									if (mineRandom.NextDouble() < 0.4)
									{
										tryToAddMonster(BuffMonsterIfNecessary(new Grub(Vector2.Zero)), j + 1, k);
									}
									if (mineRandom.NextDouble() < 0.4)
									{
										tryToAddMonster(BuffMonsterIfNecessary(new Grub(Vector2.Zero)), j, k - 1);
									}
									if (mineRandom.NextDouble() < 0.4)
									{
										tryToAddMonster(BuffMonsterIfNecessary(new Grub(Vector2.Zero)), j, k + 1);
									}
								}
								else if (monster is DustSpirit)
								{
									if (mineRandom.NextDouble() < 0.6)
									{
										tryToAddMonster(BuffMonsterIfNecessary(new DustSpirit(Vector2.Zero)), j - 1, k);
									}
									if (mineRandom.NextDouble() < 0.6)
									{
										tryToAddMonster(BuffMonsterIfNecessary(new DustSpirit(Vector2.Zero)), j + 1, k);
									}
									if (mineRandom.NextDouble() < 0.6)
									{
										tryToAddMonster(BuffMonsterIfNecessary(new DustSpirit(Vector2.Zero)), j, k - 1);
									}
									if (mineRandom.NextDouble() < 0.6)
									{
										tryToAddMonster(BuffMonsterIfNecessary(new DustSpirit(Vector2.Zero)), j, k + 1);
									}
								}
								if (mineRandom.NextDouble() < 0.00175)
								{
									monster.hasSpecialItem.Value = true;
								}
								if (monster.GetBoundingBox().Width > 64 && !isTileClearForMineObjects(j + 1, k))
								{
									continue;
								}
								if (monster != null && monster is GreenSlime && (bool)(monster as GreenSlime).prismatic)
								{
									foreach (NPC character in characters)
									{
										if (character is GreenSlime && (bool)(character as GreenSlime).prismatic)
										{
											break;
										}
									}
								}
								characters.Add(monster);
							}
							else if (mineRandom.NextDouble() <= itemChance)
							{
								Vector2 key = new Vector2(j, k);
								base.Objects.Add(key, getRandomItemForThisLevel(mineLevel));
							}
							else if (mineRandom.NextDouble() <= 0.005 && !isDarkArea() && !mustKillAllMonstersToAdvance() && (GetAdditionalDifficulty() <= 0 || (getMineArea() == 40 && mineLevel % 40 < 30)))
							{
								if (!isTileClearForMineObjects(j + 1, k) || !isTileClearForMineObjects(j, k + 1) || !isTileClearForMineObjects(j + 1, k + 1))
								{
									continue;
								}
								Vector2 tile = new Vector2(j, k);
								int parentSheetIndex3 = ((mineRandom.NextDouble() < 0.5) ? 752 : 754);
								int mineArea = getMineArea();
								if (mineArea == 40)
								{
									if (GetAdditionalDifficulty() > 0)
									{
										parentSheetIndex3 = 600;
										if (mineRandom.NextDouble() < 0.1)
										{
											parentSheetIndex3 = 602;
										}
									}
									else
									{
										parentSheetIndex3 = ((mineRandom.NextDouble() < 0.5) ? 756 : 758);
									}
								}
								resourceClumps.Add(new ResourceClump(parentSheetIndex3, 2, 2, tile));
							}
							else if (GetAdditionalDifficulty() > 0)
							{
								if (getMineArea() == 40 && mineLevel % 40 < 30 && mineRandom.NextDouble() < 0.01 && getTileIndexAt(j, k - 1, "Buildings") != -1)
								{
									terrainFeatures.Add(new Vector2(j, k), new Tree(8, 5));
								}
								else if (getMineArea() == 40 && mineLevel % 40 < 30 && mineRandom.NextDouble() < 0.1 && (getTileIndexAt(j, k - 1, "Buildings") != -1 || getTileIndexAt(j - 1, k, "Buildings") != -1 || getTileIndexAt(j, k + 1, "Buildings") != -1 || getTileIndexAt(j + 1, k, "Buildings") != -1 || terrainFeatures.ContainsKey(new Vector2(j - 1, k)) || terrainFeatures.ContainsKey(new Vector2(j + 1, k)) || terrainFeatures.ContainsKey(new Vector2(j, k - 1)) || terrainFeatures.ContainsKey(new Vector2(j, k + 1))))
								{
									terrainFeatures.Add(new Vector2(j, k), new Grass((mineLevel >= 50) ? 6 : 5, (mineLevel >= 50) ? 1 : mineRandom.Next(1, 5)));
								}
								else if (getMineArea() == 80 && !isDarkArea() && mineRandom.NextDouble() < 0.1 && (getTileIndexAt(j, k - 1, "Buildings") != -1 || getTileIndexAt(j - 1, k, "Buildings") != -1 || getTileIndexAt(j, k + 1, "Buildings") != -1 || getTileIndexAt(j + 1, k, "Buildings") != -1 || terrainFeatures.ContainsKey(new Vector2(j - 1, k)) || terrainFeatures.ContainsKey(new Vector2(j + 1, k)) || terrainFeatures.ContainsKey(new Vector2(j, k - 1)) || terrainFeatures.ContainsKey(new Vector2(j, k + 1))))
								{
									terrainFeatures.Add(new Vector2(j, k), new Grass(4, mineRandom.Next(1, 5)));
								}
							}
						}
						else if (isContainerPlatform(j, k) && isTileLocationTotallyClearAndPlaceable(j, k) && mineRandom.NextDouble() < 0.4 && (flag || canAdd(0, num)))
						{
							Vector2 vector3 = new Vector2(j, k);
							objects.Add(vector3, new BreakableContainer(vector3, 118, this));
							num++;
							if (flag)
							{
								updateMineLevelData(0);
							}
						}
						else if (mineRandom.NextDouble() <= monsterChance && isTileLocationTotallyClearAndPlaceable(j, k) && isTileOnClearAndSolidGround(j, k) && getDistanceFromStart(j, k) > 5f && (!AnyOnlineFarmerHasBuff(23) || getMineArea() == 121))
						{
							Monster monster2 = BuffMonsterIfNecessary(getMonsterForThisLevel(mineLevel, j, k));
							if (mineRandom.NextDouble() < 0.01)
							{
								monster2.hasSpecialItem.Value = true;
							}
							characters.Add(monster2);
						}
					}
				}
				if (stonesLeftOnThisLevel > 35)
				{
					int num4 = stonesLeftOnThisLevel / 35;
					for (int l = 0; l < num4; l++)
					{
						Vector2 key2 = objects.Keys.ElementAt(mineRandom.Next(objects.Count()));
						if (!objects[key2].name.Equals("Stone"))
						{
							continue;
						}
						int num5 = mineRandom.Next(3, 8);
						bool flag3 = mineRandom.NextDouble() < 0.1;
						for (int m = (int)key2.X - num5 / 2; (float)m < key2.X + (float)(num5 / 2); m++)
						{
							for (int n = (int)key2.Y - num5 / 2; (float)n < key2.Y + (float)(num5 / 2); n++)
							{
								Vector2 key3 = new Vector2(m, n);
								if (objects.ContainsKey(key3) && objects[key3].name.Equals("Stone"))
								{
									objects.Remove(key3);
									stonesLeftOnThisLevel--;
									if (getDistanceFromStart(m, n) > 5f && flag3 && mineRandom.NextDouble() < 0.12)
									{
										Monster item = BuffMonsterIfNecessary(getMonsterForThisLevel(mineLevel, m, n));
										characters.Add(item);
									}
								}
							}
						}
					}
				}
				tryToAddAreaUniques();
				if (mineRandom.NextDouble() < 0.95 && !mustKillAllMonstersToAdvance() && mineLevel > 1 && mineLevel % 5 != 0 && shouldCreateLadderOnThisLevel())
				{
					Vector2 v = new Vector2(mineRandom.Next(map.GetLayer("Back").LayerWidth), mineRandom.Next(map.GetLayer("Back").LayerHeight));
					if (isTileClearForMineObjects(v))
					{
						createLadderDown((int)v.X, (int)v.Y);
					}
				}
				if (mustKillAllMonstersToAdvance() && EnemyCount <= 1)
				{
					characters.Add(new Bat(tileBeneathLadder * 64f + new Vector2(256f, 256f)));
				}
			}
			if ((!mustKillAllMonstersToAdvance() || isDinoArea) && mineLevel % 5 != 0 && mineLevel > 2 && mineLevel != 220 && !netIsTreasureRoom.Value)
			{
				tryToAddOreClumps();
				if ((bool)isLightingDark)
				{
					tryToAddOldMinerPath();
				}
			}
		}

		public void placeAppropriateOreAt(Vector2 tile)
		{
			if (isTileLocationTotallyClearAndPlaceable(tile))
			{
				objects.Add(tile, getAppropriateOre(tile));
			}
		}

		public Object getAppropriateOre(Vector2 tile)
		{
			Object @object = new Object(tile, 751, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false);
			@object.minutesUntilReady.Value = 3;
			switch (getMineArea())
			{
			case 0:
			case 10:
				if (GetAdditionalDifficulty() > 0)
				{
					@object.parentSheetIndex.Value = 849;
					@object.minutesUntilReady.Value = 6;
				}
				break;
			case 40:
				if (GetAdditionalDifficulty() > 0)
				{
					@object = new ColoredObject(290, 1, new Color(150, 225, 160))
					{
						MinutesUntilReady = 6,
						CanBeSetDown = true,
						name = "Stone",
						TileLocation = tile,
						ColorSameIndexAsParentSheetIndex = true,
						Flipped = (mineRandom.NextDouble() < 0.5)
					};
				}
				else if (mineRandom.NextDouble() < 0.8)
				{
					@object = new Object(tile, 290, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false);
					@object.minutesUntilReady.Value = 4;
				}
				break;
			case 80:
				if (mineRandom.NextDouble() < 0.8)
				{
					@object = new Object(tile, 764, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false);
					@object.minutesUntilReady.Value = 8;
				}
				break;
			case 121:
				@object = new Object(tile, 764, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false);
				@object.minutesUntilReady.Value = 8;
				if (mineRandom.NextDouble() < 0.02)
				{
					@object = new Object(tile, 765, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false);
					@object.minutesUntilReady.Value = 16;
				}
				break;
			}
			if (mineRandom.NextDouble() < 0.25 && getMineArea() != 40 && GetAdditionalDifficulty() <= 0)
			{
				@object = new Object(tile, (mineRandom.NextDouble() < 0.5) ? 668 : 670, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false);
				@object.minutesUntilReady.Value = 2;
			}
			return @object;
		}

		public void tryToAddOreClumps()
		{
			if (!(mineRandom.NextDouble() < 0.55 + Game1.player.team.AverageDailyLuck(Game1.currentLocation)))
			{
				return;
			}
			Vector2 randomTile = getRandomTile();
			for (int i = 0; i < 1 || mineRandom.NextDouble() < 0.25 + Game1.player.team.AverageDailyLuck(Game1.currentLocation); i++)
			{
				if (isTileLocationTotallyClearAndPlaceable(randomTile) && isTileOnClearAndSolidGround(randomTile) && doesTileHaveProperty((int)randomTile.X, (int)randomTile.Y, "Diggable", "Back") == null)
				{
					Object appropriateOre = getAppropriateOre(randomTile);
					if ((int)appropriateOre.parentSheetIndex == 670)
					{
						appropriateOre.ParentSheetIndex = 668;
					}
					Utility.recursiveObjectPlacement(appropriateOre, (int)randomTile.X, (int)randomTile.Y, 0.949999988079071, 0.30000001192092896, this, "Dirt", ((int)appropriateOre.parentSheetIndex == 668) ? 1 : 0, 0.05000000074505806, ((int)appropriateOre.parentSheetIndex != 668) ? 1 : 2);
				}
				randomTile = getRandomTile();
			}
		}

		public void tryToAddOldMinerPath()
		{
			Vector2 randomTile = getRandomTile();
			int num = 0;
			while (!isTileOnClearAndSolidGround(randomTile) && num < 8)
			{
				randomTile = getRandomTile();
				num++;
			}
			if (!isTileOnClearAndSolidGround(randomTile))
			{
				return;
			}
			Stack<Point> stack = PathFindController.findPath(Utility.Vector2ToPoint(tileBeneathLadder), Utility.Vector2ToPoint(randomTile), PathFindController.isAtEndPoint, this, Game1.player, 500);
			if (stack == null)
			{
				return;
			}
			while (stack.Count > 0)
			{
				Point point = stack.Pop();
				removeEverythingExceptCharactersFromThisTile(point.X, point.Y);
				if (stack.Count <= 0 || !(mineRandom.NextDouble() < 0.2))
				{
					continue;
				}
				Vector2 vector = Vector2.Zero;
				vector = ((stack.Peek().X != point.X) ? new Vector2(point.X, point.Y + ((!(mineRandom.NextDouble() < 0.5)) ? 1 : (-1))) : new Vector2(point.X + ((!(mineRandom.NextDouble() < 0.5)) ? 1 : (-1)), point.Y));
				if (!vector.Equals(Vector2.Zero) && isTileLocationTotallyClearAndPlaceable(vector) && isTileOnClearAndSolidGround(vector))
				{
					if (mineRandom.NextDouble() < 0.5)
					{
						Torch torch = new Torch(vector, 1);
						torch.placementAction(this, (int)vector.X * 64, (int)vector.Y * 64, null);
					}
					else
					{
						placeAppropriateOreAt(vector);
					}
				}
			}
		}

		public void tryToAddAreaUniques()
		{
			if ((getMineArea() != 10 && getMineArea() != 80 && (getMineArea() != 40 || !(mineRandom.NextDouble() < 0.1))) || isDarkArea() || mustKillAllMonstersToAdvance())
			{
				return;
			}
			int num = mineRandom.Next(7, 24);
			int parentSheetIndex = ((getMineArea() == 80) ? 316 : ((getMineArea() == 40) ? 319 : 313));
			Color color = Color.White;
			int objectIndexAddRange = 2;
			if (GetAdditionalDifficulty() > 0)
			{
				if (getMineArea() == 10)
				{
					parentSheetIndex = 674;
					color = new Color(30, 120, 255);
				}
				else if (getMineArea() == 40)
				{
					if (mineLevel % 40 >= 30)
					{
						parentSheetIndex = 319;
					}
					else
					{
						parentSheetIndex = 882;
						color = new Color(100, 180, 220);
					}
				}
				else if (getMineArea() == 80)
				{
					return;
				}
			}
			for (int i = 0; i < num; i++)
			{
				Vector2 tileLocation = new Vector2(mineRandom.Next(map.GetLayer("Back").LayerWidth), mineRandom.Next(map.GetLayer("Back").LayerHeight));
				if (color.Equals(Color.White))
				{
					Utility.recursiveObjectPlacement(new Object(tileLocation, parentSheetIndex, "Weeds", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
					{
						Fragility = 2,
						CanBeGrabbed = true
					}, (int)tileLocation.X, (int)tileLocation.Y, 1.0, (float)mineRandom.Next(10, 40) / 100f, this, "Dirt", objectIndexAddRange, 0.29);
					continue;
				}
				Utility.recursiveObjectPlacement(new ColoredObject(parentSheetIndex, 1, color)
				{
					Fragility = 2,
					CanBeGrabbed = true,
					CanBeSetDown = true,
					Name = "Weeds",
					TileLocation = tileLocation,
					ColorSameIndexAsParentSheetIndex = true
				}, (int)tileLocation.X, (int)tileLocation.Y, 1.0, (float)mineRandom.Next(10, 40) / 100f, this, "Dirt", objectIndexAddRange, 0.29);
			}
		}

		public void tryToAddMonster(Monster m, int tileX, int tileY)
		{
			if (isTileClearForMineObjects(tileX, tileY) && !isTileOccupied(new Vector2(tileX, tileY)))
			{
				m.setTilePosition(tileX, tileY);
				characters.Add(m);
			}
		}

		public bool isContainerPlatform(int x, int y)
		{
			if (map.GetLayer("Back").Tiles[x, y] != null && map.GetLayer("Back").Tiles[x, y].TileIndex == 257)
			{
				return true;
			}
			return false;
		}

		public bool mustKillAllMonstersToAdvance()
		{
			if (!isSlimeArea && !isMonsterArea)
			{
				return isDinoArea;
			}
			return true;
		}

		public void createLadderAt(Vector2 p, string sound = "hoeHit")
		{
			if (shouldCreateLadderOnThisLevel())
			{
				playSound(sound);
				createLadderAtEvent[p] = true;
			}
		}

		public bool shouldCreateLadderOnThisLevel()
		{
			if (mineLevel != 77377)
			{
				return mineLevel != 120;
			}
			return false;
		}

		private void doCreateLadderAt(Vector2 p)
		{
			string startSound = ((Game1.currentLocation == this) ? "sandyStep" : null);
			updateMap();
			setMapTileIndex((int)p.X, (int)p.Y, 173, "Buildings");
			temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f, Color.White * 0.5f)
			{
				interval = 80f
			});
			temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f - new Vector2(16f, 16f), Color.White * 0.5f)
			{
				delayBeforeAnimationStart = 150,
				interval = 80f,
				scale = 0.75f,
				startSound = startSound
			});
			temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f + new Vector2(32f, 16f), Color.White * 0.5f)
			{
				delayBeforeAnimationStart = 300,
				interval = 80f,
				scale = 0.75f,
				startSound = startSound
			});
			temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f - new Vector2(32f, -16f), Color.White * 0.5f)
			{
				delayBeforeAnimationStart = 450,
				interval = 80f,
				scale = 0.75f,
				startSound = startSound
			});
			temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f - new Vector2(-16f, 16f), Color.White * 0.5f)
			{
				delayBeforeAnimationStart = 600,
				interval = 80f,
				scale = 0.75f,
				startSound = startSound
			});
			if (Game1.player.currentLocation == this)
			{
				Game1.player.TemporaryPassableTiles.Add(new Microsoft.Xna.Framework.Rectangle((int)p.X * 64, (int)p.Y * 64, 64, 64));
			}
		}

		public bool recursiveTryToCreateLadderDown(Vector2 centerTile, string sound = "hoeHit", int maxIterations = 16)
		{
			int i = 0;
			Queue<Vector2> queue = new Queue<Vector2>();
			queue.Enqueue(centerTile);
			List<Vector2> list = new List<Vector2>();
			for (; i < maxIterations; i++)
			{
				if (queue.Count <= 0)
				{
					break;
				}
				Vector2 vector = queue.Dequeue();
				list.Add(vector);
				if (!isTileOccupied(vector, "ignoreMe") && isTileOnClearAndSolidGround(vector) && isTileOccupiedByFarmer(vector) == null && doesTileHaveProperty((int)vector.X, (int)vector.Y, "Type", "Back") != null && doesTileHaveProperty((int)vector.X, (int)vector.Y, "Type", "Back").Equals("Stone"))
				{
					createLadderAt(vector);
					return true;
				}
				Vector2[] directionsTileVectors = Utility.DirectionsTileVectors;
				foreach (Vector2 vector2 in directionsTileVectors)
				{
					if (!list.Contains(vector + vector2))
					{
						queue.Enqueue(vector + vector2);
					}
				}
			}
			return false;
		}

		public override void monsterDrop(Monster monster, int x, int y, Farmer who)
		{
			if ((bool)monster.hasSpecialItem)
			{
				Game1.createItemDebris(getSpecialItemForThisMineLevel(mineLevel, x / 64, y / 64), monster.Position, Game1.random.Next(4), monster.currentLocation);
			}
			else if (mineLevel > 121 && who != null && who.getFriendshipHeartLevelForNPC("Krobus") >= 10 && (int)who.houseUpgradeLevel >= 1 && !who.isMarried() && !who.isEngaged() && Game1.random.NextDouble() < 0.001)
			{
				Game1.createItemDebris(new Object(808, 1), monster.Position, Game1.random.Next(4), monster.currentLocation);
			}
			else
			{
				base.monsterDrop(monster, x, y, who);
			}
			if ((mustKillAllMonstersToAdvance() || !(Game1.random.NextDouble() < 0.15)) && (!mustKillAllMonstersToAdvance() || EnemyCount > 1))
			{
				return;
			}
			Vector2 vector = new Vector2(x, y) / 64f;
			vector.X = (int)vector.X;
			vector.Y = (int)vector.Y;
			monster.Name = "ignoreMe";
			Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle((int)vector.X * 64, (int)vector.Y * 64, 64, 64);
			if (!isTileOccupied(vector, "ignoreMe") && isTileOnClearAndSolidGround(vector) && !Game1.player.GetBoundingBox().Intersects(value) && doesTileHaveProperty((int)vector.X, (int)vector.Y, "Type", "Back") != null && doesTileHaveProperty((int)vector.X, (int)vector.Y, "Type", "Back").Equals("Stone"))
			{
				createLadderAt(vector);
			}
			else if (mustKillAllMonstersToAdvance() && EnemyCount <= 1)
			{
				vector = new Vector2((int)tileBeneathLadder.X, (int)tileBeneathLadder.Y);
				createLadderAt(vector, "newArtifact");
				if (mustKillAllMonstersToAdvance() && Game1.player.currentLocation == this)
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MineShaft.cs.9484"));
				}
			}
		}

		public Item GetReplacementChestItem(int floor)
		{
			List<Item> list = null;
			if (Game1.netWorldState.Value.ShuffleMineChests == MineChestType.Remixed)
			{
				list = new List<Item>();
				switch (floor)
				{
				case 10:
					list.Add(new Boots(506));
					list.Add(new Boots(507));
					list.Add(new MeleeWeapon(12));
					list.Add(new MeleeWeapon(17));
					list.Add(new MeleeWeapon(22));
					list.Add(new MeleeWeapon(31));
					break;
				case 20:
					list.Add(new MeleeWeapon(11));
					list.Add(new MeleeWeapon(24));
					list.Add(new MeleeWeapon(20));
					list.Add(new Ring(517));
					list.Add(new Ring(519));
					break;
				case 50:
					list.Add(new Boots(509));
					list.Add(new Boots(510));
					list.Add(new Boots(508));
					list.Add(new MeleeWeapon(1));
					list.Add(new MeleeWeapon(43));
					break;
				case 60:
					list.Add(new MeleeWeapon(21));
					list.Add(new MeleeWeapon(44));
					list.Add(new MeleeWeapon(6));
					list.Add(new MeleeWeapon(18));
					list.Add(new MeleeWeapon(27));
					break;
				case 80:
					list.Add(new Boots(512));
					list.Add(new Boots(511));
					list.Add(new MeleeWeapon(10));
					list.Add(new MeleeWeapon(7));
					list.Add(new MeleeWeapon(46));
					list.Add(new MeleeWeapon(19));
					break;
				case 90:
					list.Add(new MeleeWeapon(8));
					list.Add(new MeleeWeapon(52));
					list.Add(new MeleeWeapon(45));
					list.Add(new MeleeWeapon(5));
					list.Add(new MeleeWeapon(60));
					break;
				case 110:
					list.Add(new Boots(514));
					list.Add(new Boots(878));
					list.Add(new MeleeWeapon(50));
					list.Add(new MeleeWeapon(28));
					break;
				}
			}
			if (list != null && list.Count > 0)
			{
				Random random = new Random((int)(Game1.uniqueIDForThisGame * 512) + floor);
				return Utility.GetRandom(list, random);
			}
			return null;
		}

		private void addLevelChests()
		{
			List<Item> list = new List<Item>();
			Vector2 vector = new Vector2(9f, 9f);
			Color tint = Color.White;
			if (mineLevel < 121 && mineLevel % 20 == 0 && mineLevel % 40 != 0)
			{
				vector.Y += 4f;
			}
			Item replacementChestItem = GetReplacementChestItem(mineLevel);
			bool flag = false;
			if (replacementChestItem != null)
			{
				list.Add(replacementChestItem);
			}
			else
			{
				switch (mineLevel)
				{
				case 5:
					Game1.player.completeQuest(14);
					if (!Game1.player.hasOrWillReceiveMail("guildQuest"))
					{
						Game1.addMailForTomorrow("guildQuest");
					}
					break;
				case 10:
					list.Add(new Boots(506));
					break;
				case 20:
					list.Add(new MeleeWeapon(11));
					break;
				case 40:
					Game1.player.completeQuest(17);
					list.Add(new Slingshot());
					break;
				case 50:
					list.Add(new Boots(509));
					break;
				case 60:
					list.Add(new MeleeWeapon(21));
					break;
				case 70:
					list.Add(new Slingshot(33));
					break;
				case 80:
					list.Add(new Boots(512));
					break;
				case 90:
					list.Add(new MeleeWeapon(8));
					break;
				case 100:
					list.Add(new Object(434, 1));
					break;
				case 110:
					list.Add(new Boots(514));
					break;
				case 120:
					Game1.player.completeQuest(18);
					Game1.getSteamAchievement("Achievement_TheBottom");
					if (!Game1.player.hasSkullKey)
					{
						list.Add(new SpecialItem(4));
					}
					tint = Color.Pink;
					break;
				case 220:
					if (Game1.player.secretNotesSeen.Contains(10) && !Game1.player.mailReceived.Contains("qiCave"))
					{
						Game1.eventUp = true;
						Game1.displayHUD = false;
						Game1.player.CanMove = false;
						Game1.player.showNotCarrying();
						currentEvent = new Event(Game1.content.LoadString((numberOfCraftedStairsUsedThisRun <= 10) ? "Data\\ExtraDialogue:SkullCavern_100_event_honorable" : "Data\\ExtraDialogue:SkullCavern_100_event"));
						currentEvent.exitLocation = new LocationRequest(base.Name, isStructure: false, this);
						Game1.player.chestConsumedMineLevels[mineLevel] = true;
					}
					else
					{
						flag = true;
					}
					break;
				}
			}
			if (netIsTreasureRoom.Value || flag)
			{
				list.Add(getTreasureRoomItem());
			}
			if (list.Count > 0 && !Game1.player.chestConsumedMineLevels.ContainsKey(mineLevel))
			{
				overlayObjects[vector] = new Chest(0, list, vector)
				{
					Tint = tint
				};
			}
		}

		public static Item getTreasureRoomItem()
		{
			return Game1.random.Next(26) switch
			{
				0 => new Object(288, 5), 
				1 => new Object(287, 10), 
				2 => new Object(802, 15), 
				3 => new Object(773, Game1.random.Next(2, 5)), 
				4 => new Object(749, 5), 
				5 => new Object(688, 5), 
				6 => new Object(681, Game1.random.Next(1, 4)), 
				7 => new Object(Game1.random.Next(628, 634), 1), 
				8 => new Object(645, Game1.random.Next(1, 3)), 
				9 => new Object(621, 4), 
				10 => new Object(Game1.random.Next(472, 499), Game1.random.Next(1, 5) * 5), 
				11 => new Object(286, 15), 
				12 => new Object(437, 1), 
				13 => new Object(439, 1), 
				14 => new Object(349, Game1.random.Next(2, 5)), 
				15 => new Object(337, Game1.random.Next(2, 4)), 
				16 => new Object(Game1.random.Next(235, 245), 5), 
				17 => new Object(74, 1), 
				18 => new Object(Vector2.Zero, 21), 
				19 => new Object(Vector2.Zero, 25), 
				20 => new Object(Vector2.Zero, 165), 
				21 => new Hat(37), 
				22 => new Hat(38), 
				23 => new Hat(65), 
				24 => new Object(Vector2.Zero, 272), 
				25 => new Hat(83), 
				_ => new Object(288, 5), 
			};
		}

		public static Item getSpecialItemForThisMineLevel(int level, int x, int y)
		{
			Random random = new Random(level + (int)Game1.stats.DaysPlayed + x + y * 10000);
			if (Game1.mine == null)
			{
				return new Object(388, 1);
			}
			if (Game1.mine.GetAdditionalDifficulty() > 0)
			{
				if (random.NextDouble() < 0.02)
				{
					return new Object(Vector2.Zero, 272);
				}
				switch (random.Next(7))
				{
				case 0:
					return new MeleeWeapon(61);
				case 1:
					return new Object(910, 1);
				case 2:
					return new Object(913, 1);
				case 3:
					return new Object(915, 1);
				case 4:
					return new Ring(527);
				case 5:
					return new Object(858, 1);
				case 6:
				{
					Item treasureRoomItem = getTreasureRoomItem();
					treasureRoomItem.Stack = 1;
					return treasureRoomItem;
				}
				}
			}
			if (level < 20)
			{
				switch (random.Next(6))
				{
				case 0:
					return new MeleeWeapon(16);
				case 1:
					return new MeleeWeapon(24);
				case 2:
					return new Boots(504);
				case 3:
					return new Boots(505);
				case 4:
					return new Ring(516);
				case 5:
					return new Ring(518);
				}
			}
			else if (level < 40)
			{
				switch (random.Next(7))
				{
				case 0:
					return new MeleeWeapon(22);
				case 1:
					return new MeleeWeapon(24);
				case 2:
					return new Boots(504);
				case 3:
					return new Boots(505);
				case 4:
					return new Ring(516);
				case 5:
					return new Ring(518);
				case 6:
					return new MeleeWeapon(15);
				}
			}
			else if (level < 60)
			{
				switch (random.Next(7))
				{
				case 0:
					return new MeleeWeapon(6);
				case 1:
					return new MeleeWeapon(26);
				case 2:
					return new MeleeWeapon(15);
				case 3:
					return new Boots(510);
				case 4:
					return new Ring(517);
				case 5:
					return new Ring(519);
				case 6:
					return new MeleeWeapon(27);
				}
			}
			else if (level < 80)
			{
				switch (random.Next(7))
				{
				case 0:
					return new MeleeWeapon(26);
				case 1:
					return new MeleeWeapon(27);
				case 2:
					return new Boots(508);
				case 3:
					return new Boots(510);
				case 4:
					return new Ring(517);
				case 5:
					return new Ring(519);
				case 6:
					return new MeleeWeapon(19);
				}
			}
			else if (level < 100)
			{
				switch (random.Next(7))
				{
				case 0:
					return new MeleeWeapon(48);
				case 1:
					return new MeleeWeapon(48);
				case 2:
					return new Boots(511);
				case 3:
					return new Boots(513);
				case 4:
					return new MeleeWeapon(18);
				case 5:
					return new MeleeWeapon(28);
				case 6:
					return new MeleeWeapon(52);
				}
			}
			else if (level < 120)
			{
				switch (random.Next(7))
				{
				case 0:
					return new MeleeWeapon(19);
				case 1:
					return new MeleeWeapon(50);
				case 2:
					return new Boots(511);
				case 3:
					return new Boots(513);
				case 4:
					return new MeleeWeapon(18);
				case 5:
					return new MeleeWeapon(46);
				case 6:
					return new Ring(887);
				}
			}
			else
			{
				switch (random.Next(12))
				{
				case 0:
					return new MeleeWeapon(45);
				case 1:
					return new MeleeWeapon(50);
				case 2:
					return new Boots(511);
				case 3:
					return new Boots(513);
				case 4:
					return new MeleeWeapon(18);
				case 5:
					return new MeleeWeapon(28);
				case 6:
					return new MeleeWeapon(52);
				case 7:
					return new Object(787, 1);
				case 8:
					return new Boots(878);
				case 9:
					return new Object(856, 1);
				case 10:
					return new Ring(859);
				case 11:
					return new Ring(887);
				}
			}
			return new Object(78, 1);
		}

		public override bool isTileOccupied(Vector2 tileLocation, string characterToIgnore = "", bool ignoreAllCharacters = false)
		{
			if (tileBeneathLadder.Equals(tileLocation))
			{
				return true;
			}
			if (tileBeneathElevator != Vector2.Zero && tileBeneathElevator.Equals(tileLocation))
			{
				return true;
			}
			return base.isTileOccupied(tileLocation, characterToIgnore, ignoreAllCharacters);
		}

		public bool isDarkArea()
		{
			if (loadedDarkArea || mineLevel % 40 > 30)
			{
				return getMineArea() != 40;
			}
			return false;
		}

		public bool isTileClearForMineObjects(Vector2 v)
		{
			if (tileBeneathLadder.Equals(v) || tileBeneathElevator.Equals(v))
			{
				return false;
			}
			if (!isTileLocationTotallyClearAndPlaceable(v))
			{
				return false;
			}
			string text = doesTileHaveProperty((int)v.X, (int)v.Y, "Type", "Back");
			if (text == null || !text.Equals("Stone"))
			{
				return false;
			}
			if (!isTileOnClearAndSolidGround(v))
			{
				return false;
			}
			if (objects.ContainsKey(v))
			{
				return false;
			}
			return true;
		}

		public override string getFootstepSoundReplacement(string footstep)
		{
			if (GetAdditionalDifficulty() > 0 && getMineArea() == 40 && mineLevel % 40 < 30 && footstep == "stoneStep")
			{
				return "grassyStep";
			}
			return base.getFootstepSoundReplacement(footstep);
		}

		public bool isTileOnClearAndSolidGround(Vector2 v)
		{
			if (map.GetLayer("Back").Tiles[(int)v.X, (int)v.Y] == null)
			{
				return false;
			}
			if (map.GetLayer("Front").Tiles[(int)v.X, (int)v.Y] != null || map.GetLayer("Buildings").Tiles[(int)v.X, (int)v.Y] != null)
			{
				return false;
			}
			if (getTileIndexAt((int)v.X, (int)v.Y, "Back") == 77)
			{
				return false;
			}
			return true;
		}

		public bool isTileOnClearAndSolidGround(int x, int y)
		{
			if (map.GetLayer("Back").Tiles[x, y] == null)
			{
				return false;
			}
			if (map.GetLayer("Front").Tiles[x, y] != null)
			{
				return false;
			}
			if (getTileIndexAt(x, y, "Back") == 77)
			{
				return false;
			}
			return true;
		}

		public bool isTileClearForMineObjects(int x, int y)
		{
			return isTileClearForMineObjects(new Vector2(x, y));
		}

		public void loadLevel(int level)
		{
			isMonsterArea = false;
			isSlimeArea = false;
			loadedDarkArea = false;
			isQuarryArea = false;
			isDinoArea = false;
			mineLoader.Unload();
			mineLoader.Dispose();
			mineLoader = Game1.content.CreateTemporary();
			int num = ((level % 40 % 20 == 0 && level % 40 != 0) ? 20 : ((level % 10 == 0) ? 10 : level));
			num %= 40;
			if (level == 120)
			{
				num = 120;
			}
			if (getMineArea(level) == 121)
			{
				MineShaft mineShaft = null;
				foreach (MineShaft activeMine in activeMines)
				{
					if (activeMine != null && activeMine.mineLevel > 120 && activeMine.mineLevel < level && (mineShaft == null || activeMine.mineLevel > mineShaft.mineLevel))
					{
						mineShaft = activeMine;
					}
				}
				num = mineRandom.Next(40);
				while (mineShaft != null && num == mineShaft.loadedMapNumber)
				{
					num = mineRandom.Next(40);
				}
				while (num % 5 == 0)
				{
					num = mineRandom.Next(40);
				}
				if (level == 220)
				{
					num = 10;
				}
				else if (level >= 130)
				{
					double num2 = 0.01;
					num2 += Game1.player.team.AverageDailyLuck(Game1.currentLocation) / 10.0 + Game1.player.team.AverageLuckLevel(Game1.currentLocation) / 100.0;
					if (Game1.random.NextDouble() < num2)
					{
						netIsTreasureRoom.Value = true;
						num = 10;
					}
				}
			}
			else if (getMineArea() == 77377 && mineLevel == 77377)
			{
				num = 77377;
			}
			mapPath.Value = "Maps\\Mines\\" + num;
			loadedMapNumber = num;
			updateMap();
			Random random = new Random((int)Game1.stats.DaysPlayed + level * 100 + (int)Game1.uniqueIDForThisGame / 2);
			if ((!AnyOnlineFarmerHasBuff(23) || getMineArea() == 121) && random.NextDouble() < 0.044 && num % 5 != 0 && num % 40 > 5 && num % 40 < 30 && num % 40 != 19)
			{
				if (random.NextDouble() < 0.5)
				{
					isMonsterArea = true;
				}
				else
				{
					isSlimeArea = true;
				}
				if (getMineArea() == 121 && mineLevel > 126 && random.NextDouble() < 0.5)
				{
					isDinoArea = true;
					isSlimeArea = false;
					isMonsterArea = false;
				}
			}
			else if (mineLevel < 121 && random.NextDouble() < 0.044 && Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccCraftsRoom") && Game1.MasterPlayer.hasOrWillReceiveMail("VisitedQuarryMine") && num % 40 > 1 && num % 5 != 0)
			{
				isQuarryArea = true;
				if (random.NextDouble() < 0.25)
				{
					isMonsterArea = true;
				}
			}
			if (isQuarryArea || getMineArea(level) == 77377)
			{
				mapImageSource.Value = "Maps\\Mines\\mine_quarryshaft";
				int num3 = map.Layers[0].LayerWidth * map.Layers[0].LayerHeight / 100;
				isQuarryArea = true;
				isSlimeArea = false;
				isMonsterArea = false;
				isDinoArea = false;
				for (int i = 0; i < num3; i++)
				{
					brownSpots.Add(new Vector2(mineRandom.Next(0, map.Layers[0].LayerWidth), mineRandom.Next(0, map.Layers[0].LayerHeight)));
				}
			}
			else if (isDinoArea)
			{
				mapImageSource.Value = "Maps\\Mines\\mine_dino";
			}
			else if (isSlimeArea)
			{
				mapImageSource.Value = "Maps\\Mines\\mine_slime";
			}
			else if (getMineArea() == 0 || getMineArea() == 10 || (getMineArea(level) != 0 && getMineArea(level) != 10))
			{
				if (getMineArea(level) == 40)
				{
					mapImageSource.Value = "Maps\\Mines\\mine_frost";
					if (level >= 70)
					{
						mapImageSource.Value += "_dark";
						loadedDarkArea = true;
					}
				}
				else if (getMineArea(level) == 80)
				{
					mapImageSource.Value = "Maps\\Mines\\mine_lava";
					if (level >= 110 && level != 120)
					{
						mapImageSource.Value += "_dark";
						loadedDarkArea = true;
					}
				}
				else if (getMineArea(level) == 121)
				{
					mapImageSource.Value = "Maps\\Mines\\mine_desert";
					if (num % 40 >= 30)
					{
						mapImageSource.Value += "_dark";
						loadedDarkArea = true;
					}
				}
			}
			if (GetAdditionalDifficulty() > 0)
			{
				string text = "Maps\\Mines\\mine";
				if (mapImageSource.Value != null)
				{
					text = mapImageSource.Value;
				}
				if (text.EndsWith("_dark"))
				{
					text = text.Remove(text.Length - "_dark".Length);
				}
				string text2 = text;
				if (level % 40 >= 30)
				{
					loadedDarkArea = true;
				}
				if (loadedDarkArea)
				{
					text += "_dark";
				}
				text += "_dangerous";
				try
				{
					mapImageSource.Value = text;
					Game1.temporaryContent.Load<Texture2D>(mapImageSource.Value);
				}
				catch (ContentLoadException)
				{
					text = text2 + "_dangerous";
					try
					{
						mapImageSource.Value = text;
						Game1.temporaryContent.Load<Texture2D>(mapImageSource.Value);
					}
					catch (ContentLoadException)
					{
						text = text2;
						if (loadedDarkArea)
						{
							text += "_dark";
						}
						try
						{
							mapImageSource.Value = text;
							Game1.temporaryContent.Load<Texture2D>(mapImageSource.Value);
							goto end_IL_062c;
						}
						catch (ContentLoadException)
						{
							mapImageSource.Value = text2;
							goto end_IL_062c;
						}
						end_IL_062c:;
					}
				}
			}
			ApplyDiggableTileFixes();
			if (!isSideBranch())
			{
				lowestLevelReached = Math.Max(lowestLevelReached, level);
				if (mineLevel % 5 == 0 && getMineArea() != 121)
				{
					prepareElevator();
				}
			}
			base.tapToMove = new TapToMove(this);
		}

		private void addBlueFlamesToChallengeShrine()
		{
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
			Game1.playSound("fireball");
		}

		public static void CheckForQiChallengeCompletion()
		{
			if (Game1.player.deepestMineLevel >= 145 && Game1.player.hasQuest(20) && !Game1.player.hasOrWillReceiveMail("QiChallengeComplete"))
			{
				Game1.player.completeQuest(20);
				Game1.addMailForTomorrow("QiChallengeComplete");
			}
		}

		private void prepareElevator()
		{
			Point point2 = (ElevatorLightSpot = Utility.findTile(this, 80, "Buildings"));
			if (point2.X >= 0)
			{
				if (canAdd(3, 0))
				{
					elevatorShouldDing.Value = true;
					updateMineLevelData(3);
				}
				else
				{
					setMapTileIndex(point2.X, point2.Y, 48, "Buildings");
				}
			}
		}

		public void enterMineShaft()
		{
			DelayedAction.playSoundAfterDelay("fallDown", 1200);
			DelayedAction.playSoundAfterDelay("clubSmash", 2200);
			Random random = new Random(mineLevel + (int)Game1.uniqueIDForThisGame + Game1.Date.TotalDays);
			int num = random.Next(3, 9);
			if (random.NextDouble() < 0.1)
			{
				num = num * 2 - 1;
			}
			if (mineLevel < 220 && mineLevel + num > 220)
			{
				num = 220 - mineLevel;
			}
			lastLevelsDownFallen = num;
			Game1.player.health = Math.Max(1, Game1.player.health - num * 3);
			isFallingDownShaft = true;
			Game1.player.CanMove = false;
			Game1.player.jump();
			afterFall();
		}

		private void afterFall()
		{
			Game1.drawObjectDialogue(Game1.content.LoadString((lastLevelsDownFallen > 7) ? "Strings\\Locations:Mines_FallenFar" : "Strings\\Locations:Mines_Fallen", lastLevelsDownFallen));
			Game1.messagePause = true;
			Game1.enterMine(mineLevel + lastLevelsDownFallen);
			Game1.fadeToBlackAlpha = 1f;
			Game1.player.faceDirection(2);
			Game1.player.showFrame(5);
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			Tile tile = map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
			if (tile != null && who.IsLocalPlayer)
			{
				switch (tile.TileIndex)
				{
				case 112:
					if (mineLevel <= 120)
					{
						Game1.activeClickableMenu = new MineElevatorMenu();
						return true;
					}
					break;
				case 115:
				{
					Response[] answerChoices2 = new Response[2]
					{
						new Response("Leave", Game1.content.LoadString("Strings\\Locations:Mines_LeaveMine")).SetHotKey(Keys.Y),
						new Response("Do", Game1.content.LoadString("Strings\\Locations:Mines_DoNothing")).SetHotKey(Keys.Escape)
					};
					createQuestionDialogue(" ", answerChoices2, "ExitMine");
					return true;
				}
				case 173:
					Game1.enterMine(mineLevel + 1);
					playSound("stairsdown");
					return true;
				case 174:
				{
					Response[] answerChoices = new Response[2]
					{
						new Response("Jump", Game1.content.LoadString("Strings\\Locations:Mines_ShaftJumpIn")).SetHotKey(Keys.Y),
						new Response("Do", Game1.content.LoadString("Strings\\Locations:Mines_DoNothing")).SetHotKey(Keys.Escape)
					};
					createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Mines_Shaft"), answerChoices, "Shaft");
					return true;
				}
				case 194:
					playSound("openBox");
					playSound("Ship");
					map.GetLayer("Buildings").Tiles[tileLocation].TileIndex++;
					map.GetLayer("Front").Tiles[tileLocation.X, tileLocation.Y - 1].TileIndex++;
					Game1.createRadialDebris(this, 382, tileLocation.X, tileLocation.Y, 6, resource: false, -1, item: true);
					updateMineLevelData(2, -1);
					return true;
				case 315:
				case 316:
				case 317:
					if (Game1.player.team.SpecialOrderRuleActive("MINE_HARD") || Game1.player.team.specialRulesRemovedToday.Contains("MINE_HARD"))
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ChallengeShrine_OnQiChallenge"));
					}
					else if (Game1.player.team.toggleMineShrineOvernight.Value)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ChallengeShrine_AlreadyActive"));
					}
					else
					{
						createQuestionDialogue(Game1.player.team.mineShrineActivated.Value ? Game1.content.LoadString("Strings\\Locations:ChallengeShrine_AlreadyHard") : Game1.content.LoadString("Strings\\Locations:ChallengeShrine_NotYetHard"), createYesNoResponses(), "ShrineOfChallenge");
					}
					break;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
		{
			if (isQuarryArea)
			{
				return "";
			}
			if (Game1.random.NextDouble() < 0.15)
			{
				int objectIndex = 330;
				if (Game1.random.NextDouble() < 0.07)
				{
					if (Game1.random.NextDouble() < 0.75)
					{
						switch (Game1.random.Next(5))
						{
						case 0:
							objectIndex = 96;
							break;
						case 1:
							objectIndex = ((!who.hasOrWillReceiveMail("lostBookFound")) ? 770 : (((int)Game1.netWorldState.Value.LostBooksFound < 21) ? 102 : 770));
							break;
						case 2:
							objectIndex = 110;
							break;
						case 3:
							objectIndex = 112;
							break;
						case 4:
							objectIndex = 585;
							break;
						}
					}
					else if (Game1.random.NextDouble() < 0.75)
					{
						switch (getMineArea())
						{
						case 0:
						case 10:
							objectIndex = ((Game1.random.NextDouble() < 0.5) ? 121 : 97);
							break;
						case 40:
							objectIndex = ((Game1.random.NextDouble() < 0.5) ? 122 : 336);
							break;
						case 80:
							objectIndex = 99;
							break;
						}
					}
					else
					{
						objectIndex = ((Game1.random.NextDouble() < 0.5) ? 126 : 127);
					}
				}
				else if (Game1.random.NextDouble() < 0.19)
				{
					objectIndex = ((Game1.random.NextDouble() < 0.5) ? 390 : getOreIndexForLevel(mineLevel, Game1.random));
				}
				else
				{
					if (Game1.random.NextDouble() < 0.08)
					{
						Game1.createRadialDebris(this, 8, xLocation, yLocation, Game1.random.Next(1, 5), resource: true);
						return "";
					}
					if (Game1.random.NextDouble() < 0.45)
					{
						objectIndex = 330;
					}
					else if (Game1.random.NextDouble() < 0.12)
					{
						if (Game1.random.NextDouble() < 0.25)
						{
							objectIndex = 749;
						}
						else
						{
							switch (getMineArea())
							{
							case 0:
							case 10:
								objectIndex = 535;
								break;
							case 40:
								objectIndex = 536;
								break;
							case 80:
								objectIndex = 537;
								break;
							}
						}
					}
					else
					{
						objectIndex = 78;
					}
				}
				Game1.createObjectDebris(objectIndex, xLocation, yLocation, who.UniqueMultiplayerID, this);
				bool flag = who != null && who.CurrentTool != null && who.CurrentTool is Hoe && who.CurrentTool.hasEnchantmentOfType<GenerousEnchantment>();
				float num = 0.25f;
				if (flag && Game1.random.NextDouble() < (double)num)
				{
					Game1.createObjectDebris(objectIndex, xLocation, yLocation, who.UniqueMultiplayerID, this);
				}
				return "";
			}
			return "";
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
			foreach (NPC character in characters)
			{
				if (character is Monster)
				{
					(character as Monster).drawAboveAllLayers(b);
				}
			}
			b.End();
			Game1.PushUIMode();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (fogAlpha > 0f || ambientFog)
			{
				Vector2 position = default(Vector2);
				for (float num = -256 + (int)(fogPos.X % 256f); num < (float)Game1.graphics.GraphicsDevice.Viewport.Width; num += 256f)
				{
					for (float num2 = -256 + (int)(fogPos.Y % 256f); num2 < (float)Game1.graphics.GraphicsDevice.Viewport.Height; num2 += 256f)
					{
						position.X = (int)num;
						position.Y = (int)num2;
						b.Draw(Game1.mouseCursors, position, fogSource, (fogAlpha > 0f) ? (fogColor * fogAlpha) : fogColor, 0f, Vector2.Zero, 4.001f, SpriteEffects.None, 1f);
					}
				}
			}
			if (!Game1.game1.takingMapScreenshot && !isSideBranch())
			{
				int color = ((getMineArea() == 0 || (isDarkArea() && getMineArea() != 121)) ? 4 : ((getMineArea() == 10) ? 6 : ((getMineArea() == 40) ? 7 : ((getMineArea() == 80) ? 2 : 3))));
				string s = (mineLevel + ((getMineArea() == 121) ? (-120) : 0)).ToString() ?? "";
				Microsoft.Xna.Framework.Rectangle titleSafeArea = Game1.game1.GraphicsDevice.Viewport.GetTitleSafeArea();
				if (Game1.options.verticalToolbar)
				{
					titleSafeArea.X += Math.Max(0, Game1.toolbarPaddingX) + Game1.toolbar.itemSlotSize + 20;
				}
				int num3 = 0;
				SpriteText.drawString(b, s, titleSafeArea.Left + 16, titleSafeArea.Top + 16, 999999, -1, 999999, 1f, 1f, junimoText: false, 2, "", color);
				int widthOfString = SpriteText.getWidthOfString(s);
				if (mustKillAllMonstersToAdvance())
				{
					b.Draw(Game1.mouseCursors, new Vector2(titleSafeArea.Left + 16 + widthOfString + 16, titleSafeArea.Top + 16) + new Vector2(4f, 6f) * 4f, new Microsoft.Xna.Framework.Rectangle(192, 324, 7, 10), Color.White, 0f, new Vector2(3f, 5f), 4f + Game1.dialogueButtonScale / 25f, SpriteEffects.None, 1f);
				}
			}
			b.End();
			Game1.PopUIMode();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		}

		public override void checkForMusic(GameTime time)
		{
			if (Game1.player.freezePause <= 0 && !isFogUp && mineLevel != 120)
			{
				string text = "";
				switch (getMineArea())
				{
				case 0:
				case 10:
				case 121:
				case 77377:
					text = "Upper";
					break;
				case 40:
					text = "Frost";
					break;
				case 80:
					text = "Lava";
					break;
				}
				text += "_Ambient";
				if (GetAdditionalDifficulty() > 0 && getMineArea() == 40 && mineLevel < 70)
				{
					text = "jungle_ambience";
				}
				if (Game1.getMusicTrackName() == "none" || Game1.isMusicContextActiveButNotPlaying() || (Game1.getMusicTrackName().EndsWith("_Ambient") && Game1.getMusicTrackName() != text))
				{
					Game1.changeMusicTrack(text);
				}
				timeSinceLastMusic = Math.Min(335000, timeSinceLastMusic + time.ElapsedGameTime.Milliseconds);
			}
		}

		public string getMineSong()
		{
			if (mineLevel < 40)
			{
				return "EarthMine";
			}
			if (mineLevel < 80)
			{
				return "FrostMine";
			}
			return "LavaMine";
		}

		public int GetAdditionalDifficulty()
		{
			if (mineLevel == 77377)
			{
				return 0;
			}
			if (mineLevel > 120)
			{
				return Game1.netWorldState.Value.SkullCavesDifficulty;
			}
			return Game1.netWorldState.Value.MinesDifficulty;
		}

		public bool isPlayingSongFromDifferentArea()
		{
			if (Game1.getMusicTrackName() != getMineSong())
			{
				return Game1.getMusicTrackName().EndsWith("Mine");
			}
			return false;
		}

		public void playMineSong()
		{
			string mineSong = getMineSong();
			if ((Game1.getMusicTrackName() == "none" || Game1.isMusicContextActiveButNotPlaying() || Game1.getMusicTrackName().Contains("Ambient")) && !isDarkArea() && mineLevel != 77377)
			{
				Game1.changeMusicTrack(mineSong);
				timeSinceLastMusic = 0;
			}
		}

		protected override void resetLocalState()
		{
			addLevelChests();
			base.resetLocalState();
			if ((bool)elevatorShouldDing)
			{
				timeUntilElevatorLightUp = 1500;
			}
			else if (mineLevel % 5 == 0 && getMineArea() != 121)
			{
				setElevatorLit();
			}
			if (!isSideBranch(mineLevel))
			{
				Game1.player.deepestMineLevel = Math.Max(Game1.player.deepestMineLevel, mineLevel);
				if (Game1.player.team.specialOrders != null)
				{
					foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
					{
						if (specialOrder.onMineFloorReached != null)
						{
							specialOrder.onMineFloorReached(Game1.player, mineLevel);
						}
					}
				}
			}
			if (mineLevel == 77377)
			{
				Game1.addMailForTomorrow("VisitedQuarryMine", noLetter: true, sendToEveryone: true);
			}
			CheckForQiChallengeCompletion();
			if (mineLevel == 120)
			{
				Game1.player.timesReachedMineBottom++;
			}
			Vector2 vector = mineEntrancePosition(Game1.player);
			Game1.xLocationAfterWarp = (int)vector.X;
			Game1.yLocationAfterWarp = (int)vector.Y;
			if (Game1.IsClient)
			{
				Game1.player.Position = new Vector2(Game1.xLocationAfterWarp * 64, Game1.yLocationAfterWarp * 64 - (Game1.player.Sprite.getHeight() - 32) + 16);
			}
			forceViewportPlayerFollow = true;
			if (mineLevel == 20 && !Game1.IsMultiplayer && Game1.isRaining && Game1.player.eventsSeen.Contains(901756) && !Game1.IsMultiplayer)
			{
				characters.Clear();
				NPC nPC = new NPC(new AnimatedSprite("Characters\\Abigail", 0, 16, 32), new Vector2(896f, 644f), "SeedShop", 3, "AbigailMine", datable: true, null, Game1.content.Load<Texture2D>("Portraits\\Abigail"))
				{
					displayName = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions")["Abigail"].Split('/')[11]
				};
				Random random = new Random((int)Game1.stats.DaysPlayed);
				if (!Game1.player.mailReceived.Contains("AbigailInMineFirst"))
				{
					nPC.setNewDialogue(Game1.content.LoadString("Strings\\Characters:AbigailInMineFirst"));
					nPC.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(0, 300),
						new FarmerSprite.AnimationFrame(1, 300),
						new FarmerSprite.AnimationFrame(2, 300),
						new FarmerSprite.AnimationFrame(3, 300)
					});
					Game1.player.mailReceived.Add("AbigailInMineFirst");
				}
				else if (random.NextDouble() < 0.15)
				{
					nPC.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(16, 500),
						new FarmerSprite.AnimationFrame(17, 500),
						new FarmerSprite.AnimationFrame(18, 500),
						new FarmerSprite.AnimationFrame(19, 500)
					});
					nPC.setNewDialogue(Game1.content.LoadString("Strings\\Characters:AbigailInMineFlute"));
					Game1.changeMusicTrack("AbigailFlute");
				}
				else
				{
					nPC.setNewDialogue(Game1.content.LoadString("Strings\\Characters:AbigailInMine" + random.Next(5)));
					nPC.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(0, 300),
						new FarmerSprite.AnimationFrame(1, 300),
						new FarmerSprite.AnimationFrame(2, 300),
						new FarmerSprite.AnimationFrame(3, 300)
					});
				}
				characters.Add(nPC);
			}
			if (mineLevel == 120 && GetAdditionalDifficulty() > 0 && !Game1.player.hasOrWillReceiveMail("reachedBottomOfHardMines"))
			{
				Game1.addMailForTomorrow("reachedBottomOfHardMines", noLetter: true, sendToEveryone: true);
			}
			if (mineLevel == 120 && Game1.player.hasOrWillReceiveMail("reachedBottomOfHardMines"))
			{
				setMapTileIndex(9, 6, 315, "Buildings");
				setMapTileIndex(10, 6, 316, "Buildings");
				setMapTileIndex(11, 6, 317, "Buildings");
				setTileProperty(9, 6, "Buildings", "Action", "");
				setTileProperty(10, 6, "Buildings", "Action", "");
				setTileProperty(11, 6, "Buildings", "Action", "");
				setMapTileIndex(9, 5, 299, "Front");
				setMapTileIndex(10, 5, 300, "Front");
				setMapTileIndex(11, 5, 301, "Front");
				if ((Game1.player.team.mineShrineActivated.Value && !Game1.player.team.toggleMineShrineOvernight.Value) || (!Game1.player.team.mineShrineActivated.Value && Game1.player.team.toggleMineShrineOvernight.Value))
				{
					DelayedAction.functionAfterDelay(delegate
					{
						addBlueFlamesToChallengeShrine();
					}, 1000);
				}
			}
			ApplyDiggableTileFixes();
			if (isMonsterArea || isSlimeArea)
			{
				Random random2 = new Random((int)Game1.stats.DaysPlayed);
				Game1.showGlobalMessage(Game1.content.LoadString((random2.NextDouble() < 0.5) ? "Strings\\Locations:Mines_Infested" : "Strings\\Locations:Mines_Overrun"));
			}
			bool flag = mineLevel % 20 == 0;
			bool flag2 = false;
			if (flag)
			{
				waterTiles = new bool[map.Layers[0].LayerWidth, map.Layers[0].LayerHeight];
				waterColor.Value = ((getMineArea() == 80) ? (Color.Red * 0.8f) : (new Color(50, 100, 200) * 0.5f));
				for (int i = 0; i < map.GetLayer("Buildings").LayerHeight; i++)
				{
					for (int j = 0; j < map.GetLayer("Buildings").LayerWidth; j++)
					{
						string text = doesTileHaveProperty(j, i, "Water", "Back");
						if (text != null)
						{
							flag2 = true;
							if (text == "I")
							{
								waterTiles.waterTiles[j, i] = new WaterTiles.WaterTileData(is_water: true, is_visible: false);
							}
							else
							{
								waterTiles[j, i] = true;
							}
							if (getMineArea() == 80 && Game1.random.NextDouble() < 0.1)
							{
								sharedLights[j + i * 1000] = new LightSource(4, new Vector2(j, i) * 64f, 2f, new Color(0, 220, 220), j + i * 1000, LightSource.LightContext.None, 0L);
							}
						}
					}
				}
			}
			if (!flag2)
			{
				waterTiles = null;
			}
			if (getMineArea(mineLevel) != getMineArea(mineLevel - 1) || mineLevel == 120 || isPlayingSongFromDifferentArea())
			{
				Game1.changeMusicTrack("none");
			}
			if (GetAdditionalDifficulty() > 0 && mineLevel == 70)
			{
				Game1.changeMusicTrack("none");
			}
			if (mineLevel == 77377 && Game1.player.mailReceived.Contains("gotGoldenScythe"))
			{
				setMapTileIndex(29, 4, 245, "Front");
				setMapTileIndex(30, 4, 246, "Front");
				setMapTileIndex(29, 5, 261, "Front");
				setMapTileIndex(30, 5, 262, "Front");
				setMapTileIndex(29, 6, 277, "Buildings");
				setMapTileIndex(30, 56, 278, "Buildings");
			}
			if (mineLevel > 1 && (mineLevel == 2 || (mineLevel % 5 != 0 && timeSinceLastMusic > 150000 && Game1.random.NextDouble() < 0.5)))
			{
				playMineSong();
			}
		}

		public virtual void ApplyDiggableTileFixes()
		{
			if (map != null && (GetAdditionalDifficulty() <= 0 || getMineArea() == 40 || !isDarkArea()))
			{
				if (!map.TileSheets[0].TileIndexProperties[165].ContainsKey("Diggable"))
				{
					map.TileSheets[0].TileIndexProperties[165].Add("Diggable", new PropertyValue("true"));
				}
				if (!map.TileSheets[0].TileIndexProperties[181].ContainsKey("Diggable"))
				{
					map.TileSheets[0].TileIndexProperties[181].Add("Diggable", new PropertyValue("true"));
				}
				if (!map.TileSheets[0].TileIndexProperties[183].ContainsKey("Diggable"))
				{
					map.TileSheets[0].TileIndexProperties[183].Add("Diggable", new PropertyValue("true"));
				}
			}
		}

		public void createLadderDown(int x, int y, bool forceShaft = false)
		{
			createLadderDownEvent[new Point(x, y)] = forceShaft || (getMineArea() == 121 && !mustKillAllMonstersToAdvance() && mineRandom.NextDouble() < 0.2);
		}

		private void doCreateLadderDown(Point point, bool shaft)
		{
			updateMap();
			int x = point.X;
			int y = point.Y;
			if (shaft)
			{
				map.GetLayer("Buildings").Tiles[x, y] = new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 174);
			}
			else
			{
				ladderHasSpawned = true;
				map.GetLayer("Buildings").Tiles[x, y] = new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 173);
			}
			if (Game1.player.currentLocation == this)
			{
				Game1.player.TemporaryPassableTiles.Add(new Microsoft.Xna.Framework.Rectangle(x * 64, y * 64, 64, 64));
			}
		}

		public void checkStoneForItems(int tileIndexOfStone, int x, int y, Farmer who)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			double num = who.DailyLuck / 2.0 + (double)who.MiningLevel * 0.005 + (double)who.LuckLevel * 0.001;
			Random random = new Random(x * 1000 + y + mineLevel + (int)Game1.uniqueIDForThisGame / 2);
			double num2 = random.NextDouble();
			double num3 = ((tileIndexOfStone == 40 || tileIndexOfStone == 42) ? 1.2 : 0.8);
			double num4 = ((tileIndexOfStone == 34 || tileIndexOfStone == 36 || tileIndexOfStone == 50 || tileIndexOfStone == 52) ? 1.2 : 0.8);
			stonesLeftOnThisLevel--;
			double num5 = 0.02 + 1.0 / (double)Math.Max(1, stonesLeftOnThisLevel) + (double)who.LuckLevel / 100.0 + Game1.player.DailyLuck / 5.0;
			if (EnemyCount == 0)
			{
				num5 += 0.04;
			}
			if (!ladderHasSpawned && !mustKillAllMonstersToAdvance() && (stonesLeftOnThisLevel == 0 || random.NextDouble() < num5) && shouldCreateLadderOnThisLevel())
			{
				createLadderDown(x, y);
			}
			if (breakStone(tileIndexOfStone, x, y, who, random))
			{
				return;
			}
			if (tileIndexOfStone == 44)
			{
				int num6 = random.Next(59, 70);
				num6 += num6 % 2;
				if (who.timesReachedMineBottom == 0)
				{
					if (mineLevel < 40 && num6 != 66 && num6 != 68)
					{
						num6 = ((random.NextDouble() < 0.5) ? 66 : 68);
					}
					else if (mineLevel < 80 && (num6 == 64 || num6 == 60))
					{
						num6 = ((!(random.NextDouble() < 0.5)) ? ((random.NextDouble() < 0.5) ? 68 : 62) : ((random.NextDouble() < 0.5) ? 66 : 70));
					}
				}
				Game1.createObjectDebris(num6, x, y, who.uniqueMultiplayerID, this);
				Game1.stats.OtherPreciousGemsFound++;
				return;
			}
			if (random.NextDouble() < 0.022 * (1.0 + num) * (double)((!who.professions.Contains(22)) ? 1 : 2))
			{
				int objectIndex = 535 + ((getMineArea() == 40) ? 1 : ((getMineArea() == 80) ? 2 : 0));
				if (getMineArea() == 121)
				{
					objectIndex = 749;
				}
				if (who.professions.Contains(19) && random.NextDouble() < 0.5)
				{
					Game1.createObjectDebris(objectIndex, x, y, who.UniqueMultiplayerID, this);
				}
				Game1.createObjectDebris(objectIndex, x, y, who.UniqueMultiplayerID, this);
				who.gainExperience(5, 20 * getMineArea());
			}
			if (mineLevel > 20 && random.NextDouble() < 0.005 * (1.0 + num) * (double)((!who.professions.Contains(22)) ? 1 : 2))
			{
				if (who.professions.Contains(19) && random.NextDouble() < 0.5)
				{
					Game1.createObjectDebris(749, x, y, who.UniqueMultiplayerID, this);
				}
				Game1.createObjectDebris(749, x, y, who.UniqueMultiplayerID, this);
				who.gainExperience(5, 40 * getMineArea());
			}
			if (random.NextDouble() < 0.05 * (1.0 + num) * num3)
			{
				int num7 = random.Next(1, 3) + ((random.NextDouble() < 0.1 * (1.0 + num)) ? 1 : 0);
				if (random.NextDouble() < 0.25 * (double)((!who.professions.Contains(21)) ? 1 : 2))
				{
					Game1.createObjectDebris(382, x, y, who.UniqueMultiplayerID, this);
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(25, new Vector2(64 * x, 64 * y), Color.White, 8, Game1.random.NextDouble() < 0.5, 80f, 0, -1, -1f, 128));
				}
				Game1.createObjectDebris(getOreIndexForLevel(mineLevel, random), x, y, who.UniqueMultiplayerID, this);
				who.gainExperience(3, 5);
			}
			else if (random.NextDouble() < 0.5)
			{
				Game1.createDebris(14, x, y, 1, this);
			}
		}

		public int getOreIndexForLevel(int mineLevel, Random r)
		{
			if (getMineArea(mineLevel) == 77377)
			{
				return 380;
			}
			if (mineLevel < 40)
			{
				if (mineLevel >= 20 && r.NextDouble() < 0.1)
				{
					return 380;
				}
				return 378;
			}
			if (mineLevel < 80)
			{
				if (mineLevel >= 60 && r.NextDouble() < 0.1)
				{
					return 384;
				}
				if (!(r.NextDouble() < 0.75))
				{
					return 378;
				}
				return 380;
			}
			if (mineLevel < 120)
			{
				if (!(r.NextDouble() < 0.75))
				{
					if (!(r.NextDouble() < 0.75))
					{
						return 378;
					}
					return 380;
				}
				return 384;
			}
			if (r.NextDouble() < 0.01 + (double)((float)(mineLevel - 120) / 2000f))
			{
				return 386;
			}
			if (!(r.NextDouble() < 0.75))
			{
				if (!(r.NextDouble() < 0.75))
				{
					return 378;
				}
				return 380;
			}
			return 384;
		}

		public bool shouldUseSnowTextureHoeDirt()
		{
			if (isSlimeArea)
			{
				return false;
			}
			if (GetAdditionalDifficulty() > 0 && (mineLevel < 40 || (mineLevel >= 70 && mineLevel < 80)))
			{
				return true;
			}
			if (GetAdditionalDifficulty() <= 0 && getMineArea() == 40)
			{
				return true;
			}
			return false;
		}

		public int getMineArea(int level = -1)
		{
			if (level == -1)
			{
				level = mineLevel;
			}
			if (!isQuarryArea)
			{
				switch (level)
				{
				case 77377:
					break;
				case 80:
				case 81:
				case 82:
				case 83:
				case 84:
				case 85:
				case 86:
				case 87:
				case 88:
				case 89:
				case 90:
				case 91:
				case 92:
				case 93:
				case 94:
				case 95:
				case 96:
				case 97:
				case 98:
				case 99:
				case 100:
				case 101:
				case 102:
				case 103:
				case 104:
				case 105:
				case 106:
				case 107:
				case 108:
				case 109:
				case 110:
				case 111:
				case 112:
				case 113:
				case 114:
				case 115:
				case 116:
				case 117:
				case 118:
				case 119:
				case 120:
					return 80;
				default:
					if (level > 120)
					{
						return 121;
					}
					if (level >= 40)
					{
						return 40;
					}
					if (level > 10 && mineLevel < 30)
					{
						return 10;
					}
					return 0;
				}
			}
			return 77377;
		}

		public bool isSideBranch(int level = -1)
		{
			if (level == -1)
			{
				level = mineLevel;
			}
			return level == 77377;
		}

		public byte getWallAt(int x, int y)
		{
			return 255;
		}

		public Color getLightingColor(GameTime time)
		{
			return lighting;
		}

		public Object getRandomItemForThisLevel(int level)
		{
			int parentSheetIndex = 80;
			if (mineRandom.NextDouble() < 0.05 && level > 80)
			{
				parentSheetIndex = 422;
			}
			else if (mineRandom.NextDouble() < 0.1 && level > 20 && getMineArea() != 40)
			{
				parentSheetIndex = 420;
			}
			else if (mineRandom.NextDouble() < 0.25 || GetAdditionalDifficulty() > 0)
			{
				switch (getMineArea())
				{
				case 0:
				case 10:
					if (GetAdditionalDifficulty() > 0 && !isDarkArea())
					{
						switch (mineRandom.Next(6))
						{
						case 0:
						case 6:
							parentSheetIndex = 152;
							break;
						case 1:
							parentSheetIndex = 393;
							break;
						case 2:
							parentSheetIndex = 397;
							break;
						case 3:
							parentSheetIndex = 372;
							break;
						case 4:
							parentSheetIndex = 392;
							break;
						}
						if (mineRandom.NextDouble() < 0.005)
						{
							parentSheetIndex = 797;
						}
						else if (mineRandom.NextDouble() < 0.08)
						{
							parentSheetIndex = 394;
						}
					}
					else
					{
						parentSheetIndex = 86;
					}
					break;
				case 40:
					if (GetAdditionalDifficulty() > 0 && mineLevel % 40 < 30)
					{
						switch (mineRandom.Next(4))
						{
						case 0:
						case 3:
							parentSheetIndex = 259;
							break;
						case 1:
							parentSheetIndex = 404;
							break;
						case 2:
							parentSheetIndex = 420;
							break;
						}
						if (mineRandom.NextDouble() < 0.08)
						{
							parentSheetIndex = 422;
						}
					}
					else
					{
						parentSheetIndex = 84;
					}
					break;
				case 80:
					parentSheetIndex = 82;
					break;
				case 121:
					parentSheetIndex = ((mineRandom.NextDouble() < 0.3) ? 86 : ((mineRandom.NextDouble() < 0.3) ? 84 : 82));
					break;
				}
			}
			else
			{
				parentSheetIndex = 80;
			}
			if (isDinoArea)
			{
				parentSheetIndex = 259;
				if (mineRandom.NextDouble() < 0.06)
				{
					parentSheetIndex = 107;
				}
			}
			return new Object(parentSheetIndex, 1)
			{
				IsSpawnedObject = true
			};
		}

		public bool shouldShowDarkHoeDirt()
		{
			if (getMineArea() == 121 && !isDinoArea)
			{
				return false;
			}
			return true;
		}

		public int getRandomGemRichStoneForThisLevel(int level)
		{
			int num = mineRandom.Next(59, 70);
			num += num % 2;
			if (Game1.player.timesReachedMineBottom == 0)
			{
				if (level < 40 && num != 66 && num != 68)
				{
					num = ((mineRandom.NextDouble() < 0.5) ? 66 : 68);
				}
				else if (level < 80 && (num == 64 || num == 60))
				{
					num = ((!(mineRandom.NextDouble() < 0.5)) ? ((mineRandom.NextDouble() < 0.5) ? 68 : 62) : ((mineRandom.NextDouble() < 0.5) ? 66 : 70));
				}
			}
			return num switch
			{
				66 => 8, 
				68 => 10, 
				60 => 12, 
				70 => 6, 
				64 => 4, 
				62 => 14, 
				_ => 40, 
			};
		}

		public float getDistanceFromStart(int xTile, int yTile)
		{
			float num = Utility.distance(xTile, tileBeneathLadder.X, yTile, tileBeneathLadder.Y);
			if (tileBeneathElevator != Vector2.Zero)
			{
				num = Math.Min(num, Utility.distance(xTile, tileBeneathElevator.X, yTile, tileBeneathElevator.Y));
			}
			return num;
		}

		public Monster getMonsterForThisLevel(int level, int xTile, int yTile)
		{
			Vector2 vector = new Vector2(xTile, yTile) * 64f;
			float distanceFromStart = getDistanceFromStart(xTile, yTile);
			if (isSlimeArea)
			{
				if (GetAdditionalDifficulty() <= 0)
				{
					if (mineRandom.NextDouble() < 0.2)
					{
						return new BigSlime(vector, getMineArea());
					}
					return new GreenSlime(vector, mineLevel);
				}
				if (mineLevel < 20)
				{
					return new GreenSlime(vector, mineLevel);
				}
				if (mineLevel < 30)
				{
					return new BlueSquid(vector);
				}
				if (mineLevel < 40)
				{
					return new RockGolem(vector, this);
				}
				if (mineLevel < 50)
				{
					if (mineRandom.NextDouble() < 0.15 && distanceFromStart >= 10f)
					{
						return new Fly(vector);
					}
					return new Grub(vector);
				}
				if (mineLevel < 70)
				{
					return new Leaper(vector);
				}
			}
			else if (isDinoArea)
			{
				if (mineRandom.NextDouble() < 0.1)
				{
					return new Bat(vector, 999);
				}
				if (mineRandom.NextDouble() < 0.1)
				{
					return new Fly(vector, hard: true);
				}
				return new DinoMonster(vector);
			}
			if (getMineArea() == 0 || getMineArea() == 10)
			{
				if (mineRandom.NextDouble() < 0.25 && !mustKillAllMonstersToAdvance())
				{
					return new Bug(vector, mineRandom.Next(4), this);
				}
				if (level < 15)
				{
					if (doesTileHaveProperty(xTile, yTile, "Diggable", "Back") != null)
					{
						return new Duggy(vector);
					}
					if (mineRandom.NextDouble() < 0.15)
					{
						return new RockCrab(vector);
					}
					return new GreenSlime(vector, level);
				}
				if (level <= 30)
				{
					if (doesTileHaveProperty(xTile, yTile, "Diggable", "Back") != null)
					{
						return new Duggy(vector);
					}
					if (mineRandom.NextDouble() < 0.15)
					{
						return new RockCrab(vector);
					}
					if (mineRandom.NextDouble() < 0.05 && distanceFromStart > 10f && GetAdditionalDifficulty() <= 0)
					{
						return new Fly(vector);
					}
					if (mineRandom.NextDouble() < 0.45)
					{
						return new GreenSlime(vector, level);
					}
					if (GetAdditionalDifficulty() <= 0)
					{
						return new Grub(vector);
					}
					if (distanceFromStart > 9f)
					{
						return new BlueSquid(vector);
					}
					if (mineRandom.NextDouble() < 0.01)
					{
						return new RockGolem(vector, this);
					}
					return new GreenSlime(vector, level);
				}
				if (level <= 40)
				{
					if (mineRandom.NextDouble() < 0.1 && distanceFromStart > 10f)
					{
						return new Bat(vector, level);
					}
					if (GetAdditionalDifficulty() > 0 && mineRandom.NextDouble() < 0.1)
					{
						return new Ghost(vector, "Carbon Ghost");
					}
					return new RockGolem(vector, this);
				}
			}
			else if (getMineArea() == 40)
			{
				if (mineLevel >= 70 && (mineRandom.NextDouble() < 0.75 || GetAdditionalDifficulty() > 0))
				{
					if (mineRandom.NextDouble() < 0.75 || GetAdditionalDifficulty() <= 0)
					{
						return new Skeleton(vector, GetAdditionalDifficulty() > 0 && mineRandom.NextDouble() < 0.5);
					}
					return new Bat(vector, 77377);
				}
				if (mineRandom.NextDouble() < 0.3)
				{
					return new DustSpirit(vector, mineRandom.NextDouble() < 0.8);
				}
				if (mineRandom.NextDouble() < 0.3 && distanceFromStart > 10f)
				{
					return new Bat(vector, mineLevel);
				}
				if (!ghostAdded && mineLevel > 50 && mineRandom.NextDouble() < 0.3 && distanceFromStart > 10f)
				{
					ghostAdded = true;
					if (GetAdditionalDifficulty() > 0)
					{
						return new Ghost(vector, "Putrid Ghost");
					}
					return new Ghost(vector);
				}
				if (GetAdditionalDifficulty() > 0)
				{
					if (mineRandom.NextDouble() < 0.01)
					{
						RockCrab rockCrab = new RockCrab(vector);
						rockCrab.makeStickBug();
						return rockCrab;
					}
					if (mineLevel >= 50)
					{
						return new Leaper(vector);
					}
					if (mineRandom.NextDouble() < 0.7)
					{
						return new Grub(vector);
					}
					return new GreenSlime(vector, mineLevel);
				}
			}
			else if (getMineArea() == 80)
			{
				if (isDarkArea() && mineRandom.NextDouble() < 0.25)
				{
					return new Bat(vector, mineLevel);
				}
				if (mineRandom.NextDouble() < ((GetAdditionalDifficulty() > 0) ? 0.05 : 0.15))
				{
					return new GreenSlime(vector, getMineArea());
				}
				if (mineRandom.NextDouble() < 0.15)
				{
					return new MetalHead(vector, getMineArea());
				}
				if (mineRandom.NextDouble() < 0.25)
				{
					return new ShadowBrute(vector);
				}
				if (GetAdditionalDifficulty() > 0 && mineRandom.NextDouble() < 0.25)
				{
					return new Shooter(vector, "Shadow Sniper");
				}
				if (mineRandom.NextDouble() < 0.25)
				{
					return new ShadowShaman(vector);
				}
				if (mineRandom.NextDouble() < 0.25)
				{
					return new RockCrab(vector, "Lava Crab");
				}
				if (mineRandom.NextDouble() < 0.2 && distanceFromStart > 8f && mineLevel >= 90 && getTileIndexAt(xTile, yTile, "Back") != -1 && getTileIndexAt(xTile, yTile, "Front") == -1)
				{
					return new SquidKid(vector);
				}
			}
			else
			{
				if (getMineArea() == 121)
				{
					if (loadedDarkArea)
					{
						if (mineRandom.NextDouble() < 0.18 && distanceFromStart > 8f)
						{
							return new Ghost(vector, "Carbon Ghost");
						}
						return new Mummy(vector);
					}
					if (mineLevel % 20 == 0 && distanceFromStart > 10f)
					{
						return new Bat(vector, mineLevel);
					}
					if (mineLevel % 16 == 0 && !mustKillAllMonstersToAdvance())
					{
						return new Bug(vector, mineRandom.Next(4), this);
					}
					if (mineRandom.NextDouble() < 0.33 && distanceFromStart > 10f)
					{
						if (GetAdditionalDifficulty() <= 0)
						{
							return new Serpent(vector);
						}
						return new Serpent(vector, "Royal Serpent");
					}
					if (mineRandom.NextDouble() < 0.33 && distanceFromStart > 10f && mineLevel >= 171)
					{
						return new Bat(vector, mineLevel);
					}
					if (mineLevel >= 126 && distanceFromStart > 10f && mineRandom.NextDouble() < 0.04 && !mustKillAllMonstersToAdvance())
					{
						return new DinoMonster(vector);
					}
					if (mineRandom.NextDouble() < 0.33 && !mustKillAllMonstersToAdvance())
					{
						return new Bug(vector, mineRandom.Next(4), this);
					}
					if (mineRandom.NextDouble() < 0.25)
					{
						return new GreenSlime(vector, level);
					}
					if (mineLevel >= 146 && mineRandom.NextDouble() < 0.25)
					{
						return new RockCrab(vector, "Iridium Crab");
					}
					if (GetAdditionalDifficulty() > 0 && mineRandom.NextDouble() < 0.2 && distanceFromStart > 8f && getTileIndexAt(xTile, yTile, "Back") != -1 && getTileIndexAt(xTile, yTile, "Front") == -1)
					{
						return new SquidKid(vector);
					}
					return new BigSlime(vector, this);
				}
				if (getMineArea() == 77377)
				{
					if ((mineLevel == 77377 && yTile > 59) || (mineLevel != 77377 && mineLevel % 2 == 0))
					{
						GreenSlime greenSlime = new GreenSlime(vector, 77377);
						Vector2 value = new Vector2(xTile, yTile);
						bool flag = false;
						for (int i = 0; i < brownSpots.Count; i++)
						{
							if (Vector2.Distance(value, brownSpots[i]) < 4f)
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							int num = Game1.random.Next(120, 200);
							greenSlime.color.Value = new Color(num, num / 2, num / 4);
							while (Game1.random.NextDouble() < 0.33)
							{
								greenSlime.objectsToDrop.Add(378);
							}
							greenSlime.Health = (int)((float)greenSlime.Health * 0.5f);
							greenSlime.Speed += 2;
						}
						else
						{
							int num2 = Game1.random.Next(120, 200);
							greenSlime.color.Value = new Color(num2, num2, num2);
							while (Game1.random.NextDouble() < 0.33)
							{
								greenSlime.objectsToDrop.Add(380);
							}
							greenSlime.Speed = 1;
						}
						return greenSlime;
					}
					if (yTile < 51 || mineLevel != 77377)
					{
						return new Bat(vector, 77377);
					}
					return new Bat(vector, 77377)
					{
						focusedOnFarmers = true
					};
				}
			}
			return new GreenSlime(vector, level);
		}

		public Color getCrystalColorForThisLevel()
		{
			Random random = new Random(mineLevel + Game1.player.timesReachedMineBottom);
			if (random.NextDouble() < 0.04 && mineLevel < 80)
			{
				Color result = new Color(mineRandom.Next(256), mineRandom.Next(256), mineRandom.Next(256));
				while (result.R + result.G + result.B < 500)
				{
					result.R = (byte)Math.Min(255, result.R + 10);
					result.G = (byte)Math.Min(255, result.G + 10);
					result.B = (byte)Math.Min(255, result.B + 10);
				}
				return result;
			}
			if (random.NextDouble() < 0.07)
			{
				return new Color(255 - mineRandom.Next(20), 255 - mineRandom.Next(20), 255 - mineRandom.Next(20));
			}
			if (mineLevel < 40)
			{
				switch (mineRandom.Next(2))
				{
				case 0:
					return new Color(58, 145, 72);
				case 1:
					return new Color(255, 255, 255);
				}
			}
			else if (mineLevel < 80)
			{
				switch (mineRandom.Next(4))
				{
				case 0:
					return new Color(120, 0, 210);
				case 1:
					return new Color(0, 100, 170);
				case 2:
					return new Color(0, 220, 255);
				case 3:
					return new Color(0, 255, 220);
				}
			}
			else
			{
				switch (mineRandom.Next(2))
				{
				case 0:
					return new Color(200, 100, 0);
				case 1:
					return new Color(220, 60, 0);
				}
			}
			return Color.White;
		}

		private Object chooseStoneType(double chanceForPurpleStone, double chanceForMysticStone, double gemStoneChance, Vector2 tile)
		{
			Color color = Color.White;
			int num = 32;
			int minutesUntilReady = 1;
			if (GetAdditionalDifficulty() > 0 && mineLevel % 5 != 0 && mineRandom.NextDouble() < (double)GetAdditionalDifficulty() * 0.001 + (double)((float)mineLevel / 100000f) + Game1.player.team.AverageDailyLuck() / 13.0 + Game1.player.team.AverageLuckLevel() * 0.0001500000071246177)
			{
				return new Object(tile, 95, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
				{
					MinutesUntilReady = 25
				};
			}
			if (getMineArea() == 0 || getMineArea() == 10)
			{
				num = mineRandom.Next(31, 42);
				if (mineLevel % 40 < 30 && num >= 33 && num < 38)
				{
					num = ((mineRandom.NextDouble() < 0.5) ? 32 : 38);
				}
				else if (mineLevel % 40 >= 30)
				{
					num = ((mineRandom.NextDouble() < 0.5) ? 34 : 36);
				}
				if (GetAdditionalDifficulty() > 0)
				{
					num = mineRandom.Next(33, 37);
					minutesUntilReady = 5;
					if (Game1.random.NextDouble() < 0.33)
					{
						num = 846;
					}
					else
					{
						color = new Color(Game1.random.Next(60, 90), Game1.random.Next(150, 200), Game1.random.Next(190, 240));
					}
					if (isDarkArea())
					{
						num = mineRandom.Next(32, 39);
						int num2 = Game1.random.Next(130, 160);
						color = new Color(num2, num2, num2);
					}
					if (mineLevel != 1 && mineLevel % 5 != 0 && mineRandom.NextDouble() < 0.029)
					{
						return new Object(tile, 849, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
						{
							MinutesUntilReady = 6
						};
					}
					if (color.Equals(Color.White))
					{
						return new Object(tile, num, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
						{
							MinutesUntilReady = minutesUntilReady
						};
					}
				}
				else if (mineLevel != 1 && mineLevel % 5 != 0 && mineRandom.NextDouble() < 0.029)
				{
					return new Object(tile, 751, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
					{
						MinutesUntilReady = 3
					};
				}
			}
			else if (getMineArea() == 40)
			{
				num = mineRandom.Next(47, 54);
				minutesUntilReady = 3;
				if (GetAdditionalDifficulty() > 0 && mineLevel % 40 < 30)
				{
					num = mineRandom.Next(39, 42);
					minutesUntilReady = 5;
					color = new Color(170, 255, 160);
					if (isDarkArea())
					{
						num = mineRandom.Next(32, 39);
						int num3 = Game1.random.Next(130, 160);
						color = new Color(num3, num3, num3);
					}
					if (mineRandom.NextDouble() < 0.15)
					{
						return new ColoredObject(294 + ((mineRandom.NextDouble() < 0.5) ? 1 : 0), 1, new Color(170, 140, 155))
						{
							MinutesUntilReady = 6,
							CanBeSetDown = true,
							name = "Twig",
							TileLocation = tile,
							ColorSameIndexAsParentSheetIndex = true,
							Flipped = (mineRandom.NextDouble() < 0.5)
						};
					}
					if (mineLevel != 1 && mineLevel % 5 != 0 && mineRandom.NextDouble() < 0.029)
					{
						return new ColoredObject(290, 1, new Color(150, 225, 160))
						{
							MinutesUntilReady = 6,
							CanBeSetDown = true,
							name = "Stone",
							TileLocation = tile,
							ColorSameIndexAsParentSheetIndex = true,
							Flipped = (mineRandom.NextDouble() < 0.5)
						};
					}
					if (color.Equals(Color.White))
					{
						return new Object(tile, num, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
						{
							MinutesUntilReady = minutesUntilReady
						};
					}
				}
				else if (mineLevel % 5 != 0 && mineRandom.NextDouble() < 0.029)
				{
					return new Object(tile, 290, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
					{
						MinutesUntilReady = 4
					};
				}
			}
			else if (getMineArea() == 80)
			{
				minutesUntilReady = 4;
				num = ((mineRandom.NextDouble() < 0.3 && !isDarkArea()) ? ((!(mineRandom.NextDouble() < 0.5)) ? 32 : 38) : ((mineRandom.NextDouble() < 0.3) ? mineRandom.Next(55, 58) : ((!(mineRandom.NextDouble() < 0.5)) ? 762 : 760)));
				if (GetAdditionalDifficulty() > 0)
				{
					num = ((!(mineRandom.NextDouble() < 0.5)) ? 32 : 38);
					minutesUntilReady = 5;
					color = new Color(Game1.random.Next(140, 190), Game1.random.Next(90, 120), Game1.random.Next(210, 255));
					if (isDarkArea())
					{
						num = mineRandom.Next(32, 39);
						int num4 = Game1.random.Next(130, 160);
						color = new Color(num4, num4, num4);
					}
					if (mineLevel != 1 && mineLevel % 5 != 0 && mineRandom.NextDouble() < 0.029)
					{
						return new Object(tile, 764, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
						{
							MinutesUntilReady = 7
						};
					}
					if (color.Equals(Color.White))
					{
						return new Object(tile, num, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
						{
							MinutesUntilReady = minutesUntilReady
						};
					}
				}
				else if (mineLevel % 5 != 0 && mineRandom.NextDouble() < 0.029)
				{
					return new Object(tile, 764, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
					{
						MinutesUntilReady = 8
					};
				}
			}
			else
			{
				if (getMineArea() == 77377)
				{
					minutesUntilReady = 5;
					bool flag = false;
					foreach (Vector2 adjacentTileLocation in Utility.getAdjacentTileLocations(tile))
					{
						if (objects.ContainsKey(adjacentTileLocation))
						{
							flag = true;
							break;
						}
					}
					if (!flag && mineRandom.NextDouble() < 0.45)
					{
						return null;
					}
					bool flag2 = false;
					for (int i = 0; i < brownSpots.Count; i++)
					{
						if (Vector2.Distance(tile, brownSpots[i]) < 4f)
						{
							flag2 = true;
							break;
						}
						if (Vector2.Distance(tile, brownSpots[i]) < 6f)
						{
							return null;
						}
					}
					if (flag2)
					{
						num = ((mineRandom.NextDouble() < 0.5) ? 32 : 38);
						if (mineRandom.NextDouble() < 0.01)
						{
							return new Object(tile, 751, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
							{
								MinutesUntilReady = 3
							};
						}
					}
					else
					{
						num = ((mineRandom.NextDouble() < 0.5) ? 34 : 36);
						if (mineRandom.NextDouble() < 0.01)
						{
							return new Object(tile, 290, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
							{
								MinutesUntilReady = 3
							};
						}
					}
					return new Object(tile, num, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
					{
						MinutesUntilReady = minutesUntilReady
					};
				}
				minutesUntilReady = 5;
				num = ((mineRandom.NextDouble() < 0.5) ? ((!(mineRandom.NextDouble() < 0.5)) ? 32 : 38) : ((!(mineRandom.NextDouble() < 0.5)) ? 42 : 40));
				int num5 = mineLevel - 120;
				double num6 = 0.02 + (double)num5 * 0.0005;
				if (mineLevel >= 130)
				{
					num6 += 0.01 * (double)((float)(Math.Min(100, num5) - 10) / 10f);
				}
				double num7 = 0.0;
				if (mineLevel >= 130)
				{
					num7 += 0.001 * (double)((float)(num5 - 10) / 10f);
				}
				num7 = Math.Min(num7, 0.004);
				if (num5 > 100)
				{
					num7 += (double)num5 / 1000000.0;
				}
				if (!netIsTreasureRoom.Value && mineRandom.NextDouble() < num6)
				{
					double num8 = (double)Math.Min(100, num5) * (0.0003 + num7);
					double num9 = 0.01 + (double)(mineLevel - Math.Min(150, num5)) * 0.0005;
					double num10 = Math.Min(0.5, 0.1 + (double)(mineLevel - Math.Min(200, num5)) * 0.005);
					if (mineRandom.NextDouble() < num8)
					{
						return new Object(tile, 765, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
						{
							MinutesUntilReady = 16
						};
					}
					if (mineRandom.NextDouble() < num9)
					{
						return new Object(tile, 764, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
						{
							MinutesUntilReady = 8
						};
					}
					if (mineRandom.NextDouble() < num10)
					{
						return new Object(tile, 290, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
						{
							MinutesUntilReady = 4
						};
					}
					return new Object(tile, 751, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
					{
						MinutesUntilReady = 2
					};
				}
			}
			double num11 = Game1.player.team.AverageDailyLuck(Game1.currentLocation);
			double num12 = Game1.player.team.AverageSkillLevel(3, Game1.currentLocation);
			double num13 = num11 + num12 * 0.005;
			if (mineLevel > 50 && mineRandom.NextDouble() < 0.00025 + (double)mineLevel / 120000.0 + 0.0005 * num13 / 2.0)
			{
				num = 2;
				minutesUntilReady = 10;
			}
			else if (gemStoneChance != 0.0 && mineRandom.NextDouble() < gemStoneChance + gemStoneChance * num13 + (double)mineLevel / 24000.0)
			{
				return new Object(tile, getRandomGemRichStoneForThisLevel(mineLevel), "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
				{
					MinutesUntilReady = 5
				};
			}
			if (mineRandom.NextDouble() < chanceForPurpleStone / 2.0 + chanceForPurpleStone * num12 * 0.008 + chanceForPurpleStone * (num11 / 2.0))
			{
				num = 44;
			}
			if (mineLevel > 100 && mineRandom.NextDouble() < chanceForMysticStone + chanceForMysticStone * num12 * 0.008 + chanceForMysticStone * (num11 / 2.0))
			{
				num = 46;
			}
			num += num % 2;
			if (mineRandom.NextDouble() < 0.1 && getMineArea() != 40)
			{
				if (!color.Equals(Color.White))
				{
					return new ColoredObject((mineRandom.NextDouble() < 0.5) ? 668 : 670, 1, color)
					{
						MinutesUntilReady = 2,
						CanBeSetDown = true,
						name = "Stone",
						TileLocation = tile,
						ColorSameIndexAsParentSheetIndex = true,
						Flipped = (mineRandom.NextDouble() < 0.5)
					};
				}
				return new Object(tile, (mineRandom.NextDouble() < 0.5) ? 668 : 670, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
				{
					MinutesUntilReady = 2,
					Flipped = (mineRandom.NextDouble() < 0.5)
				};
			}
			if (!color.Equals(Color.White))
			{
				return new ColoredObject(num, 1, color)
				{
					MinutesUntilReady = minutesUntilReady,
					CanBeSetDown = true,
					name = "Stone",
					TileLocation = tile,
					ColorSameIndexAsParentSheetIndex = true,
					Flipped = (mineRandom.NextDouble() < 0.5)
				};
			}
			return new Object(tile, num, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
			{
				MinutesUntilReady = minutesUntilReady
			};
		}

		public static void OnLeftMines()
		{
			if (!Game1.IsClient && !Game1.IsMultiplayer)
			{
				clearInactiveMines(keepUntickedLevels: false);
			}
		}

		public static void clearActiveMines()
		{
			activeMines.RemoveAll(delegate(MineShaft mine)
			{
				mine.mapContent.Dispose();
				return true;
			});
		}

		private static void clearInactiveMines(bool keepUntickedLevels = true)
		{
			activeMines.RemoveAll(delegate(MineShaft mine)
			{
				if (mine.mineLevel == 77377)
				{
					return false;
				}
				if (mine.farmers.Count() > 0)
				{
					return false;
				}
				if (mine.lifespan == 0 && keepUntickedLevels)
				{
					return false;
				}
				mine.mapContent.Dispose();
				return true;
			});
		}

		public static void UpdateMines10Minutes(int timeOfDay)
		{
			clearInactiveMines();
			if (Game1.IsClient)
			{
				return;
			}
			foreach (MineShaft activeMine in activeMines)
			{
				if (activeMine.farmers.Any())
				{
					activeMine.performTenMinuteUpdate(timeOfDay);
				}
				activeMine.lifespan++;
			}
		}

		protected override void updateCharacters(GameTime time)
		{
			if (farmers.Any())
			{
				base.updateCharacters(time);
			}
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
			if (!Game1.shouldTimePass() || !isFogUp)
			{
				return;
			}
			int num = fogTime;
			fogTime -= (int)time.ElapsedGameTime.TotalMilliseconds;
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (fogTime > 5000 && num % 4000 < fogTime % 4000)
			{
				spawnFlyingMonsterOffScreen();
			}
			if (fogTime <= 0)
			{
				isFogUp.Value = false;
				if (isDarkArea())
				{
					netFogColor.Value = Color.Black;
				}
				else if (GetAdditionalDifficulty() > 0 && getMineArea() == 40 && !isDarkArea())
				{
					netFogColor.Value = default(Color);
				}
			}
		}

		public static void UpdateMines(GameTime time)
		{
			foreach (MineShaft activeMine in activeMines)
			{
				if (activeMine.farmers.Any())
				{
					activeMine.UpdateWhenCurrentLocation(time);
				}
				activeMine.updateEvenIfFarmerIsntHere(time);
			}
		}

		public static MineShaft GetMine(string name)
		{
			foreach (MineShaft activeMine in activeMines)
			{
				if (activeMine.Name.Equals(name))
				{
					return activeMine;
				}
			}
			int level = Convert.ToInt32(name.Substring("UndergroundMine".Length));
			MineShaft mineShaft = new MineShaft(level);
			activeMines.Add(mineShaft);
			mineShaft.generateContents();
			return mineShaft;
		}

		public static void ForEach(Action<MineShaft> action)
		{
			foreach (MineShaft activeMine in activeMines)
			{
				action(activeMine);
			}
		}
	}
}
