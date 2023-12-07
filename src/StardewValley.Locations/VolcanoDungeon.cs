using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Mobile;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley.Locations
{
	public class VolcanoDungeon : IslandLocation
	{
		public enum TileNeighbors
		{
			N = 1,
			S = 2,
			E = 4,
			W = 8,
			NW = 0x10,
			NE = 0x20
		}

		public NetInt level = new NetInt();

		public NetEvent1Field<Point, NetPoint> coolLavaEvent = new NetEvent1Field<Point, NetPoint>();

		public static List<VolcanoDungeon> activeLevels = new List<VolcanoDungeon>();

		public NetVector2Dictionary<bool, NetBool> cooledLavaTiles = new NetVector2Dictionary<bool, NetBool>();

		public Dictionary<Vector2, Point> localCooledLavaTiles = new Dictionary<Vector2, Point>();

		public HashSet<Point> dirtTiles = new HashSet<Point>();

		public NetInt generationSeed = new NetInt();

		public NetInt layoutIndex = new NetInt();

		public Random generationRandom;

		private LocalizedContentManager mapContent;

		[XmlIgnore]
		public int mapWidth;

		[XmlIgnore]
		public int mapHeight;

		public const int WALL_HEIGHT = 4;

		public Layer backLayer;

		public Layer buildingsLayer;

		public Layer frontLayer;

		public Layer alwaysFrontLayer;

		[XmlIgnore]
		public Point? startPosition;

		[XmlIgnore]
		public Point? endPosition;

		public const int LAYOUT_WIDTH = 64;

		public const int LAYOUT_HEIGHT = 64;

		[XmlIgnore]
		public Texture2D mapBaseTilesheet;

		public static List<Microsoft.Xna.Framework.Rectangle> setPieceAreas = new List<Microsoft.Xna.Framework.Rectangle>();

		internal static Dictionary<int, Point> _blobIndexLookup = null;

		internal static Dictionary<int, Point> _lavaBlobIndexLookup = null;

		protected bool generated;

		[NonInstancedStatic]
		protected static Point shortcutOutPosition = new Point(29, 34);

		[XmlIgnore]
		protected NetBool shortcutOutUnlocked = new NetBool(value: false);

		[XmlIgnore]
		protected NetBool bridgeUnlocked = new NetBool(value: false);

		public Color[] pixelMap;

		public int[] heightMap;

		public Dictionary<int, List<Point>> possibleSwitchPositions = new Dictionary<int, List<Point>>();

		public Dictionary<int, List<Point>> possibleGatePositions = new Dictionary<int, List<Point>>();

		public NetList<DwarfGate, NetRef<DwarfGate>> dwarfGates = new NetList<DwarfGate, NetRef<DwarfGate>>();

		[XmlIgnore]
		protected bool _sawFlameSprite;

		private int lavaSoundsPlayedThisTick;

		private float steamTimer = 6000f;

		public VolcanoDungeon()
		{
			mapContent = Game1.game1.xTileContent.CreateTemporary();
			mapPath.Value = "Maps\\Mines\\VolcanoTemplate";
		}

		public VolcanoDungeon(int level)
			: this()
		{
			this.level.Value = level;
			name.Value = "VolcanoDungeon" + level;
		}

		public override bool BlocksDamageLOS(int x, int y)
		{
			if (cooledLavaTiles.ContainsKey(new Vector2(x, y)))
			{
				return false;
			}
			return base.BlocksDamageLOS(x, y);
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(level, coolLavaEvent, cooledLavaTiles.NetFields, generationSeed, layoutIndex, dwarfGates, shortcutOutUnlocked, bridgeUnlocked);
			coolLavaEvent.onEvent += OnCoolLavaEvent;
			bridgeUnlocked.InterpolationWait = false;
			bridgeUnlocked.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && mapPath.Value != null)
				{
					UpdateBridge();
				}
			};
			shortcutOutUnlocked.InterpolationWait = false;
			shortcutOutUnlocked.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && mapPath.Value != null)
				{
					UpdateShortcutOut();
				}
			};
		}

		protected override LocalizedContentManager getMapLoader()
		{
			return mapContent;
		}

		public override bool CanPlaceThisFurnitureHere(Furniture furniture)
		{
			return false;
		}

		public virtual void OnCoolLavaEvent(Point point)
		{
			CoolLava(point.X, point.Y);
			UpdateLavaNeighbor(point.X, point.Y);
			UpdateLavaNeighbor(point.X - 1, point.Y);
			UpdateLavaNeighbor(point.X + 1, point.Y);
			UpdateLavaNeighbor(point.X, point.Y - 1);
			UpdateLavaNeighbor(point.X, point.Y + 1);
		}

		public virtual void CoolLava(int x, int y, bool playSound = true)
		{
			if (Game1.currentLocation == this)
			{
				for (int i = 0; i < 5; i++)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(x, (float)y - 0.5f) * 64f + new Vector2(Game1.random.Next(64), Game1.random.Next(64)), flipped: false, 0.007f, Color.White)
					{
						alpha = 0.75f,
						motion = new Vector2(0f, -1f),
						acceleration = new Vector2(0.002f, 0f),
						interval = 99999f,
						layerDepth = 1f,
						scale = 4f,
						scaleChange = 0.02f,
						rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
						delayBeforeAnimationStart = i * 35
					});
				}
				if (playSound && lavaSoundsPlayedThisTick < 3)
				{
					DelayedAction.playSoundAfterDelay("steam", lavaSoundsPlayedThisTick * 300);
					lavaSoundsPlayedThisTick++;
				}
			}
			if (!cooledLavaTiles.ContainsKey(new Vector2(x, y)))
			{
				cooledLavaTiles[new Vector2(x, y)] = true;
			}
		}

		public virtual void UpdateLavaNeighbor(int x, int y)
		{
			if (IsCooledLava(x, y))
			{
				setTileProperty(x, y, "Buildings", "Passable", "T");
				int num = 0;
				if (IsCooledLava(x, y - 1))
				{
					num++;
				}
				if (IsCooledLava(x, y + 1))
				{
					num += 2;
				}
				if (IsCooledLava(x - 1, y))
				{
					num += 8;
				}
				if (IsCooledLava(x + 1, y))
				{
					num += 4;
				}
				if (GetBlobLookup().ContainsKey(num))
				{
					localCooledLavaTiles[new Vector2(x, y)] = GetBlobLookup()[num];
				}
			}
		}

		public virtual bool IsCooledLava(int x, int y)
		{
			if (x < 0 || x >= mapWidth)
			{
				return false;
			}
			if (y < 0 || y >= mapHeight)
			{
				return false;
			}
			if (cooledLavaTiles.ContainsKey(new Vector2(x, y)))
			{
				return true;
			}
			return false;
		}

		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (questionAndAnswer == "LeaveVolcano_Yes")
			{
				UseVolcanoShortcut();
				return true;
			}
			return base.answerDialogueAction(questionAndAnswer, questionParams);
		}

		public void UseVolcanoShortcut()
		{
			DelayedAction.playSoundAfterDelay("fallDown", 200);
			DelayedAction.playSoundAfterDelay("clubSmash", 900);
			Game1.player.CanMove = false;
			Game1.player.jump();
			Game1.warpFarmer("IslandNorth", 56, 17, 1);
		}

		public virtual void GenerateContents(bool use_level_level_as_layout = false)
		{
			generated = true;
			if (Game1.IsMasterGame)
			{
				generationSeed.Value = (int)Game1.stats.DaysPlayed * level.Value + level.Value * 5152 + (int)Game1.uniqueIDForThisGame / 2;
				if (level.Value == 0)
				{
					layoutIndex.Value = 0;
					bridgeUnlocked.Value = Game1.MasterPlayer.hasOrWillReceiveMail("Island_VolcanoBridge");
					parrotUpgradePerches.Clear();
					parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(27, 39), new Microsoft.Xna.Framework.Rectangle(28, 34, 5, 4), 5, delegate
					{
						Game1.addMailForTomorrow("Island_VolcanoBridge", noLetter: true, sendToEveryone: true);
						bridgeUnlocked.Value = true;
					}, () => bridgeUnlocked.Value, "VolcanoBridge", "reachedCaldera, Island_Turtle"));
				}
				else if (level.Value == 5)
				{
					layoutIndex.Value = 31;
					waterColor.Value = Color.DeepSkyBlue * 0.6f;
					shortcutOutUnlocked.Value = Game1.MasterPlayer.hasOrWillReceiveMail("Island_VolcanoShortcutOut");
					parrotUpgradePerches.Clear();
					parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(shortcutOutPosition.X, shortcutOutPosition.Y), new Microsoft.Xna.Framework.Rectangle(shortcutOutPosition.X - 1, shortcutOutPosition.Y - 1, 3, 3), 5, delegate
					{
						Game1.addMailForTomorrow("Island_VolcanoShortcutOut", noLetter: true, sendToEveryone: true);
						shortcutOutUnlocked.Value = true;
					}, () => shortcutOutUnlocked.Value, "VolcanoShortcutOut", "Island_Turtle"));
				}
				else if (level.Value == 9)
				{
					layoutIndex.Value = 30;
				}
				else
				{
					List<int> list = new List<int>();
					for (int i = 1; i < GetMaxRoomLayouts(); i++)
					{
						list.Add(i);
					}
					Random random = new Random(generationSeed.Value);
					float num = 1f + (float)Game1.player.team.AverageLuckLevel() * 0.035f + (float)Game1.player.team.AverageDailyLuck() / 2f;
					if ((int)level > 1 && random.NextDouble() < 0.5 * (double)num)
					{
						bool flag = false;
						for (int j = 0; j < activeLevels.Count; j++)
						{
							if (activeLevels[j].layoutIndex.Value >= 32)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							for (int k = 32; k < 38; k++)
							{
								list.Add(k);
							}
						}
					}
					for (int l = 0; l < activeLevels.Count; l++)
					{
						if (activeLevels[l].level.Value == level.Value - 1)
						{
							list.Remove(activeLevels[l].layoutIndex.Value);
							break;
						}
					}
					layoutIndex.Value = Utility.GetRandom(list, random);
				}
			}
			GenerateLevel(use_level_level_as_layout);
			if (level.Value != 5)
			{
				return;
			}
			ApplyMapOverride("Mines\\Volcano_Well", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(25, 29, 6, 4));
			for (int m = 27; m < 31; m++)
			{
				for (int n = 29; n < 33; n++)
				{
					waterTiles[m, n] = true;
				}
			}
			ApplyMapOverride("Mines\\Volcano_DwarfShop", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(34, 29, 5, 4));
			setMapTileIndex(36, 30, 77, "Buildings");
			setMapTileIndex(36, 29, 61, "Front");
			setTileProperty(36, 30, "Buildings", "Action", "asedf");
			setMapTileIndex(35, 31, 78, "Back");
			setMapTileIndex(36, 31, 79, "Back");
			setMapTileIndex(37, 31, 62, "Back");
			if (Game1.IsMasterGame)
			{
				objects.Add(new Vector2(34f, 29f), new BreakableContainer(new Vector2(34f, 29f), isVolcano: true));
				objects.Add(new Vector2(26f, 32f), new BreakableContainer(new Vector2(26f, 32f), isVolcano: true));
				objects.Add(new Vector2(38f, 33f), new BreakableContainer(new Vector2(38f, 33f), isVolcano: true));
			}
		}

		public bool isMushroomLevel()
		{
			if ((int)layoutIndex >= 32)
			{
				return (int)layoutIndex <= 34;
			}
			return false;
		}

		public bool isMonsterLevel()
		{
			if ((int)layoutIndex >= 35)
			{
				return (int)layoutIndex <= 37;
			}
			return false;
		}

		public override void checkForMusic(GameTime time)
		{
			if (Game1.getMusicTrackName() == "rain" || Game1.getMusicTrackName() == "none" || Game1.getMusicTrackName() == "IslandMusic" || Game1.isMusicContextActiveButNotPlaying() || (Game1.getMusicTrackName().EndsWith("_Ambient") && Game1.getMusicTrackName() != "Volcano_Ambient"))
			{
				Game1.changeMusicTrack("Volcano_Ambient");
			}
		}

		public virtual void UpdateShortcutOut()
		{
			if (this == Game1.currentLocation)
			{
				if (shortcutOutUnlocked.Value)
				{
					setMapTileIndex(shortcutOutPosition.X, shortcutOutPosition.Y, 367, "Buildings");
					removeTile(shortcutOutPosition.X, shortcutOutPosition.Y - 1, "Front");
				}
				else
				{
					setMapTileIndex(shortcutOutPosition.X, shortcutOutPosition.Y, 399, "Buildings");
					setMapTileIndex(shortcutOutPosition.X, shortcutOutPosition.Y - 1, 383, "Front");
				}
			}
		}

		public virtual void UpdateBridge()
		{
			if (this != Game1.currentLocation)
			{
				return;
			}
			if (Game1.MasterPlayer.hasOrWillReceiveMail("reachedCaldera"))
			{
				setMapTileIndex(27, 39, 399, "Buildings");
				setMapTileIndex(27, 38, 383, "Front");
			}
			if (!bridgeUnlocked.Value)
			{
				return;
			}
			for (int i = 28; i <= 32; i++)
			{
				for (int j = 34; j <= 37; j++)
				{
					int num = 0;
					setMapTileIndex(i, j, i switch
					{
						28 => j switch
						{
							34 => 189, 
							37 => 221, 
							_ => 205, 
						}, 
						32 => j switch
						{
							34 => 191, 
							37 => 223, 
							_ => 207, 
						}, 
						_ => j switch
						{
							34 => 190, 
							37 => 222, 
							_ => 206, 
						}, 
					}, "Buildings");
					setTileProperty(i, j, "Buildings", "Passable", "T");
					removeTileProperty(i, j, "Back", "Water");
					NPC nPC = isCharacterAtTile(new Vector2(i, j));
					if (nPC != null && nPC is Monster)
					{
						characters.Remove(nPC);
					}
					if (waterTiles != null && i != 28 && i != 32)
					{
						waterTiles[i, j] = false;
					}
					if (cooledLavaTiles.ContainsKey(new Vector2(i, j)))
					{
						cooledLavaTiles.Remove(new Vector2(i, j));
					}
				}
			}
		}

		protected override void resetLocalState()
		{
			if (!generated)
			{
				GenerateContents();
				generated = true;
			}
			foreach (Vector2 key in cooledLavaTiles.Keys)
			{
				UpdateLavaNeighbor((int)key.X, (int)key.Y);
			}
			if ((int)level == 0)
			{
				UpdateBridge();
			}
			if (level.Value == 5)
			{
				UpdateShortcutOut();
			}
			base.resetLocalState();
			Game1.ambientLight = Color.White;
			int num = (int)(Game1.player.Position.Y / 64f);
			if (level.Value == 0 && Game1.player.previousLocationName == "Caldera")
			{
				Game1.player.Position = new Vector2(44f, 50f) * 64f;
			}
			else if (num == 0 && endPosition.HasValue)
			{
				if (endPosition.HasValue)
				{
					Game1.player.Position = new Vector2(endPosition.Value.X, endPosition.Value.Y) * 64f;
				}
			}
			else if (num == 1 && startPosition.HasValue)
			{
				Game1.player.Position = new Vector2(startPosition.Value.X, startPosition.Value.Y) * 64f;
			}
			mapBaseTilesheet = Game1.temporaryContent.Load<Texture2D>(map.TileSheets[0].ImageSource);
			_ = Game1.locationRequest;
			foreach (DwarfGate dwarfGate in dwarfGates)
			{
				dwarfGate.ResetLocalState();
			}
			if ((int)level == 5)
			{
				Game1.changeMusicTrack("Volcano_Ambient");
				AmbientLocationSounds.addSound(new Vector2(29f, 31f), 0);
			}
			else if ((int)level == 1 || ((Game1.random.NextDouble() < 0.25 || (int)level == 6) && (Game1.getMusicTrackName() == "none" || Game1.isMusicContextActiveButNotPlaying() || Game1.getMusicTrackName().EndsWith("_Ambient"))))
			{
				Game1.changeMusicTrack("VolcanoMines");
			}
			if (level.Value != 0)
			{
				return;
			}
			if (Game1.player.hasOrWillReceiveMail("Saw_Flame_Sprite_Volcano"))
			{
				_sawFlameSprite = true;
			}
			if (!_sawFlameSprite)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Monsters\\Magma Sprite", new Microsoft.Xna.Framework.Rectangle(0, 32, 16, 16), new Vector2(30f, 38f) * 64f, flipped: false, 0f, Color.White)
				{
					id = 999f,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 70f,
					light = true,
					lightRadius = 1f,
					animationLength = 7,
					layerDepth = 1f,
					yPeriodic = true,
					yPeriodicRange = 12f,
					yPeriodicLoopTime = 1000f,
					xPeriodic = true,
					xPeriodicRange = 16f,
					xPeriodicLoopTime = 1800f
				});
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\shadow", new Microsoft.Xna.Framework.Rectangle(0, 0, 12, 7), new Vector2(30.2f, 39.4f) * 64f, flipped: false, 0f, Color.White)
				{
					id = 998f,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 1000f,
					animationLength = 1,
					layerDepth = 0.001f,
					yPeriodic = true,
					yPeriodicRange = 1f,
					yPeriodicLoopTime = 1000f,
					xPeriodic = true,
					xPeriodicRange = 16f,
					xPeriodicLoopTime = 1800f
				});
			}
			ApplyMapOverride("Mines\\Volcano_Well", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(22, 43, 6, 4));
			for (int i = 24; i < 28; i++)
			{
				for (int j = 43; j < 47; j++)
				{
					waterTiles[i, j] = true;
				}
			}
		}

		protected override void resetSharedState()
		{
			base.resetSharedState();
			if ((int)level != 5)
			{
				waterColor.Value = Color.White;
			}
		}

		public override bool CanRefillWateringCanOnTile(int tileX, int tileY)
		{
			if ((int)level == 5 && new Microsoft.Xna.Framework.Rectangle(27, 29, 4, 4).Contains(tileX, tileY))
			{
				return true;
			}
			if ((int)level == 0 && tileX > 23 && tileX < 28 && tileY > 42 && tileY < 47)
			{
				return true;
			}
			return false;
		}

		public virtual void GenerateLevel(bool use_level_level_as_layout = false)
		{
			generationRandom = new Random(generationSeed.Value);
			generationRandom.Next();
			mapPath.Value = "Maps\\Mines\\VolcanoTemplate";
			updateMap();
			loadedMapPath = mapPath;
			Texture2D texture2D = Game1.temporaryContent.Load<Texture2D>("VolcanoLayouts\\Layouts");
			mapWidth = 64;
			mapHeight = 64;
			waterTiles = new bool[mapWidth, mapHeight];
			for (int i = 0; i < map.Layers.Count; i++)
			{
				Layer layer = map.Layers[i];
				map.RemoveLayer(layer);
				map.InsertLayer(new Layer(layer.Id, map, new Size(mapWidth, mapHeight), layer.TileSize), i);
			}
			backLayer = map.GetLayer("Back");
			buildingsLayer = map.GetLayer("Buildings");
			frontLayer = map.GetLayer("Front");
			alwaysFrontLayer = map.GetLayer("AlwaysFront");
			map.TileSheets[0].TileIndexProperties[1].Add("Type", new PropertyValue("Stone"));
			map.TileSheets[0].TileIndexProperties[2].Add("Type", new PropertyValue("Stone"));
			map.TileSheets[0].TileIndexProperties[3].Add("Type", new PropertyValue("Stone"));
			map.TileSheets[0].TileIndexProperties[17].Add("Type", new PropertyValue("Stone"));
			map.TileSheets[0].TileIndexProperties[18].Add("Type", new PropertyValue("Stone"));
			map.TileSheets[0].TileIndexProperties[19].Add("Type", new PropertyValue("Stone"));
			map.TileSheets[0].TileIndexProperties[528].Add("Type", new PropertyValue("Stone"));
			map.TileSheets[0].TileIndexProperties[544].Add("Type", new PropertyValue("Stone"));
			map.TileSheets[0].TileIndexProperties[560].Add("Type", new PropertyValue("Stone"));
			map.TileSheets[0].TileIndexProperties[545].Add("Type", new PropertyValue("Stone"));
			map.TileSheets[0].TileIndexProperties[561].Add("Type", new PropertyValue("Stone"));
			map.TileSheets[0].TileIndexProperties[564].Add("Type", new PropertyValue("Stone"));
			map.TileSheets[0].TileIndexProperties[565].Add("Type", new PropertyValue("Stone"));
			map.TileSheets[0].TileIndexProperties[555].Add("Type", new PropertyValue("Stone"));
			map.TileSheets[0].TileIndexProperties[571].Add("Type", new PropertyValue("Stone"));
			pixelMap = new Color[mapWidth * mapHeight];
			heightMap = new int[mapWidth * mapHeight];
			int num = texture2D.Width / 64;
			int value = layoutIndex.Value;
			int source_x = value % num * 64;
			int source_y = value / num * 64;
			bool flip_x = generationRandom.Next(2) == 1;
			if (layoutIndex.Value == 0 || layoutIndex.Value == 31)
			{
				flip_x = false;
			}
			ApplyPixels("VolcanoLayouts\\Layouts", source_x, source_y, mapWidth, mapHeight, 0, 0, flip_x);
			for (int j = 0; j < mapWidth; j++)
			{
				for (int k = 0; k < mapHeight; k++)
				{
					PlaceGroundTile(j, k);
				}
			}
			ApplyToColor(new Color(0, 255, 0), delegate(int x, int y)
			{
				if (!startPosition.HasValue)
				{
					startPosition = new Point(x, y);
				}
				if (level.Value == 0)
				{
					warps.Add(new Warp(x, y + 2, "IslandNorth", 40, 24, flipFarmer: false));
				}
				else
				{
					warps.Add(new Warp(x, y + 2, "VolcanoDungeon" + (level.Value - 1), x - startPosition.Value.X, 0, flipFarmer: false));
				}
			});
			ApplyToColor(new Color(255, 0, 0), delegate(int x, int y)
			{
				if (!endPosition.HasValue)
				{
					endPosition = new Point(x, y);
				}
				if (level.Value == 9)
				{
					warps.Add(new Warp(x, y - 2, "Caldera", 21, 39, flipFarmer: false));
				}
				else
				{
					warps.Add(new Warp(x, y - 2, "VolcanoDungeon" + (level.Value + 1), x - endPosition.Value.X, 1, flipFarmer: false));
				}
			});
			setPieceAreas.Clear();
			Color set_piece_color = new Color(255, 255, 0);
			ApplyToColor(set_piece_color, delegate(int x, int y)
			{
				int num2 = 0;
				for (num2 = 0; num2 < 32 && !(GetPixel(x + num2, y, Color.Black) != set_piece_color) && !(GetPixel(x, y + num2, Color.Black) != set_piece_color); num2++)
				{
				}
				setPieceAreas.Add(new Microsoft.Xna.Framework.Rectangle(x, y, num2, num2));
				for (int l = 0; l < num2; l++)
				{
					for (int m = 0; m < num2; m++)
					{
						SetPixelMap(x + l, y + m, Color.White);
					}
				}
			});
			possibleSwitchPositions = new Dictionary<int, List<Point>>();
			possibleGatePositions = new Dictionary<int, List<Point>>();
			ApplyToColor(new Color(128, 128, 128), delegate(int x, int y)
			{
				AddPossibleSwitchLocation(0, x, y);
			});
			ApplySetPieces();
			GenerateWalls(Color.Black, 0, 4, 4, 4, start_in_wall: true, delegate(int x, int y)
			{
				SetPixelMap(x, y, Color.Chartreuse);
			}, use_corner_hack: true);
			GenerateWalls(Color.Chartreuse, 0, 13, 1);
			ApplyToColor(Color.Blue, delegate(int x, int y)
			{
				waterTiles[x, y] = true;
				SetTile(backLayer, x, y, 4);
				setTileProperty(x, y, "Back", "Water", "T");
				if (generationRandom.NextDouble() < 0.1)
				{
					sharedLights[x + y * 1000] = new LightSource(4, new Vector2(x, y) * 64f, 2f, new Color(0, 50, 50), x + y * 1000, LightSource.LightContext.None, 0L);
				}
			});
			GenerateBlobs(Color.Blue, 0, 16, fill_center: true, is_lava_pool: true);
			if (startPosition.HasValue)
			{
				CreateEntrance(startPosition.Value);
			}
			if (endPosition.HasValue)
			{
				CreateExit(endPosition);
			}
			if ((int)level != 0)
			{
				GenerateDirtTiles();
			}
			if ((level.Value == 9 || generationRandom.NextDouble() < (isMonsterLevel() ? 1.0 : 0.2)) && possibleSwitchPositions.ContainsKey(0) && possibleSwitchPositions[0].Count > 0)
			{
				AddPossibleGateLocation(0, endPosition.Value.X, endPosition.Value.Y);
			}
			foreach (int key in possibleGatePositions.Keys)
			{
				if (possibleGatePositions[key].Count > 0 && possibleSwitchPositions.ContainsKey(key) && possibleSwitchPositions[key].Count > 0)
				{
					Point random = Utility.GetRandom(possibleGatePositions[key], generationRandom);
					CreateDwarfGate(key, random);
				}
			}
			if ((int)level == 0)
			{
				CreateExit(new Point(40, 48), draw_stairs: false);
				removeTile(40, 46, "Buildings");
				removeTile(40, 45, "Buildings");
				removeTile(40, 44, "Buildings");
				setMapTileIndex(40, 45, 266, "AlwaysFront");
				setMapTileIndex(40, 44, 76, "AlwaysFront");
				setMapTileIndex(39, 44, 76, "AlwaysFront");
				setMapTileIndex(41, 44, 76, "AlwaysFront");
				removeTile(40, 43, "Front");
				setMapTileIndex(40, 43, 70, "AlwaysFront");
				removeTile(39, 43, "Front");
				setMapTileIndex(39, 43, 69, "AlwaysFront");
				removeTile(41, 43, "Front");
				setMapTileIndex(41, 43, 69, "AlwaysFront");
				setMapTileIndex(39, 45, 265, "AlwaysFront");
				setMapTileIndex(41, 45, 267, "AlwaysFront");
				setMapTileIndex(40, 45, 60, "Back");
				setMapTileIndex(40, 46, 60, "Back");
				setMapTileIndex(40, 47, 60, "Back");
				setMapTileIndex(40, 48, 555, "Back");
				AddPossibleSwitchLocation(-1, 40, 51);
				CreateDwarfGate(-1, new Point(40, 48));
				setMapTileIndex(34, 30, 90, "Buildings");
				setMapTileIndex(34, 29, 148, "Buildings");
				setMapTileIndex(34, 31, 180, "Buildings");
				setMapTileIndex(34, 32, 196, "Buildings");
				CoolLava(34, 34, playSound: false);
				if (Game1.MasterPlayer.hasOrWillReceiveMail("volcanoShortcutUnlocked"))
				{
					foreach (DwarfGate dwarfGate in dwarfGates)
					{
						if (dwarfGate.gateIndex.Value != -1)
						{
							continue;
						}
						dwarfGate.opened.Value = true;
						dwarfGate.triggeredOpen = true;
						foreach (Point key2 in dwarfGate.switches.Keys)
						{
							dwarfGate.switches[key2] = true;
						}
					}
				}
				CreateExit(new Point(44, 50));
				warps.Add(new Warp(44, 48, "Caldera", 11, 36, flipFarmer: false));
				CreateEntrance(new Point(6, 48));
				warps.Add(new Warp(6, 50, "IslandNorth", 12, 31, flipFarmer: false));
			}
			if (Game1.IsMasterGame)
			{
				GenerateEntities();
			}
			pixelMap = null;
			base.tapToMove = new TapToMove(this);
		}

		public virtual void GenerateDirtTiles()
		{
			if ((int)level == 5)
			{
				return;
			}
			for (int i = 0; i < 8; i++)
			{
				int num = generationRandom.Next(0, 64);
				int num2 = generationRandom.Next(0, 64);
				int num3 = generationRandom.Next(2, 8);
				int num4 = generationRandom.Next(1, 3);
				int num5 = ((generationRandom.Next(2) != 0) ? 1 : (-1));
				int num6 = ((generationRandom.Next(2) != 0) ? 1 : (-1));
				bool flag = generationRandom.Next(2) == 0;
				for (int j = 0; j < num3; j++)
				{
					for (int k = num - num4; k <= num + num4; k++)
					{
						for (int l = num2 - num4; l <= num2 + num4; l++)
						{
							if (!(GetPixel(k, l, Color.Black) != Color.White))
							{
								dirtTiles.Add(new Point(k, l));
							}
						}
					}
					if (flag)
					{
						num6 += ((generationRandom.Next(2) != 0) ? 1 : (-1));
					}
					else
					{
						num5 += ((generationRandom.Next(2) != 0) ? 1 : (-1));
					}
					num += num5;
					num2 += num6;
					num4 += ((generationRandom.Next(2) != 0) ? 1 : (-1));
					if (num4 < 1)
					{
						num4 = 1;
					}
					if (num4 > 4)
					{
						num4 = 4;
					}
				}
			}
			for (int m = 0; m < 2; m++)
			{
				ErodeInvalidDirtTiles();
			}
			HashSet<Point> hashSet = new HashSet<Point>();
			Point[] array = new Point[8]
			{
				new Point(-1, -1),
				new Point(0, -1),
				new Point(1, -1),
				new Point(-1, 0),
				new Point(1, 0),
				new Point(-1, 1),
				new Point(0, 1),
				new Point(1, 1)
			};
			foreach (Point dirtTile in dirtTiles)
			{
				SetTile(backLayer, dirtTile.X, dirtTile.Y, GetTileIndex(9, 1));
				if (generationRandom.NextDouble() < 0.015)
				{
					characters.Add(new Duggy(Utility.PointToVector2(dirtTile) * 64f, magmaDuggy: true));
				}
				Point[] array2 = array;
				for (int n = 0; n < array2.Length; n++)
				{
					Point point = array2[n];
					Point item = new Point(dirtTile.X + point.X, dirtTile.Y + point.Y);
					if (!dirtTiles.Contains(item) && !hashSet.Contains(item))
					{
						hashSet.Add(item);
						Point? dirtNeighborTile = GetDirtNeighborTile(item.X, item.Y);
						if (dirtNeighborTile.HasValue)
						{
							SetTile(backLayer, item.X, item.Y, GetTileIndex(8 + dirtNeighborTile.Value.X, dirtNeighborTile.Value.Y));
						}
					}
				}
			}
		}

		public virtual void CreateEntrance(Point? position)
		{
			for (int i = -1; i <= 1; i++)
			{
				for (int j = 0; j <= 3; j++)
				{
					if (isTileOnMap(new Vector2(position.Value.X + i, position.Value.Y + j)))
					{
						removeTile(position.Value.X + i, position.Value.Y + j, "Back");
						removeTile(position.Value.X + i, position.Value.Y + j, "Buildings");
						removeTile(position.Value.X + i, position.Value.Y + j, "Front");
					}
				}
			}
			if (getTileIndexAt(position.Value.X - 1, position.Value.Y - 1, "Front") != -1)
			{
				SetTile(frontLayer, position.Value.X - 1, position.Value.Y - 1, GetTileIndex(13, 16));
			}
			removeTile(position.Value.X, position.Value.Y - 1, "Front");
			SetTile(buildingsLayer, position.Value.X - 1, position.Value.Y, GetTileIndex(13, 17));
			SetTile(buildingsLayer, position.Value.X - 1, position.Value.Y + 1, GetTileIndex(13, 18));
			SetTile(buildingsLayer, position.Value.X - 1, position.Value.Y + 2, GetTileIndex(13, 19));
			if (getTileIndexAt(position.Value.X + 1, position.Value.Y - 1, "Front") != -1)
			{
				SetTile(frontLayer, position.Value.X + 1, position.Value.Y - 1, GetTileIndex(15, 16));
			}
			SetTile(buildingsLayer, position.Value.X + 1, position.Value.Y, GetTileIndex(15, 17));
			SetTile(buildingsLayer, position.Value.X + 1, position.Value.Y + 1, GetTileIndex(15, 18));
			SetTile(buildingsLayer, position.Value.X + 1, position.Value.Y + 2, GetTileIndex(15, 19));
			SetTile(backLayer, position.Value.X, position.Value.Y, GetTileIndex(14, 17));
			SetTile(backLayer, position.Value.X, position.Value.Y + 1, GetTileIndex(14, 18));
			SetTile(backLayer, position.Value.X, position.Value.Y + 2, GetTileIndex(14, 19));
			SetTile(frontLayer, position.Value.X, position.Value.Y + 2, GetTileIndex(14, 19));
			SetTile(buildingsLayer, position.Value.X - 1, position.Value.Y + 3, GetTileIndex(12, 4));
			SetTile(buildingsLayer, position.Value.X, position.Value.Y + 3, GetTileIndex(12, 4));
			SetTile(buildingsLayer, position.Value.X + 1, position.Value.Y + 3, GetTileIndex(12, 4));
		}

		private void CreateExit(Point? position, bool draw_stairs = true)
		{
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -4; j <= 0; j++)
				{
					if (isTileOnMap(new Vector2(position.Value.X + i, position.Value.Y + j)))
					{
						if (draw_stairs)
						{
							removeTile(position.Value.X + i, position.Value.Y + j, "Back");
						}
						removeTile(position.Value.X + i, position.Value.Y + j, "Buildings");
						removeTile(position.Value.X + i, position.Value.Y + j, "Front");
					}
				}
			}
			SetTile(buildingsLayer, position.Value.X - 1, position.Value.Y, GetTileIndex(9, 19));
			SetTile(buildingsLayer, position.Value.X - 1, position.Value.Y - 1, GetTileIndex(9, 18));
			SetTile(buildingsLayer, position.Value.X - 1, position.Value.Y - 2, GetTileIndex(9, 17));
			SetTile(buildingsLayer, position.Value.X - 1, position.Value.Y - 3, GetTileIndex(9, 16));
			SetTile(alwaysFrontLayer, position.Value.X - 1, position.Value.Y - 4, GetTileIndex(12, 4));
			SetTile(alwaysFrontLayer, position.Value.X, position.Value.Y - 4, GetTileIndex(12, 4));
			SetTile(alwaysFrontLayer, position.Value.X + 1, position.Value.Y - 4, GetTileIndex(12, 4));
			SetTile(buildingsLayer, position.Value.X, position.Value.Y - 3, GetTileIndex(10, 16));
			SetTile(buildingsLayer, position.Value.X + 1, position.Value.Y, GetTileIndex(11, 19));
			SetTile(buildingsLayer, position.Value.X + 1, position.Value.Y - 1, GetTileIndex(11, 18));
			SetTile(buildingsLayer, position.Value.X + 1, position.Value.Y - 2, GetTileIndex(11, 17));
			SetTile(buildingsLayer, position.Value.X + 1, position.Value.Y - 3, GetTileIndex(11, 16));
			if (draw_stairs)
			{
				SetTile(backLayer, position.Value.X, position.Value.Y, GetTileIndex(12, 19));
				SetTile(backLayer, position.Value.X, position.Value.Y - 1, GetTileIndex(12, 18));
				SetTile(backLayer, position.Value.X, position.Value.Y - 2, GetTileIndex(12, 17));
				SetTile(backLayer, position.Value.X, position.Value.Y - 3, GetTileIndex(12, 16));
			}
			SetTile(buildingsLayer, position.Value.X - 1, position.Value.Y - 4, GetTileIndex(12, 4));
			SetTile(buildingsLayer, position.Value.X, position.Value.Y - 4, GetTileIndex(12, 4));
			SetTile(buildingsLayer, position.Value.X + 1, position.Value.Y - 4, GetTileIndex(12, 4));
		}

		public virtual void ErodeInvalidDirtTiles()
		{
			Point[] array = new Point[8]
			{
				new Point(-1, -1),
				new Point(0, -1),
				new Point(1, -1),
				new Point(-1, 0),
				new Point(1, 0),
				new Point(-1, 1),
				new Point(0, 1),
				new Point(1, 1)
			};
			Dictionary<Point, bool> dictionary = new Dictionary<Point, bool>();
			List<Point> list = new List<Point>();
			foreach (Point dirtTile in dirtTiles)
			{
				bool flag = false;
				foreach (Microsoft.Xna.Framework.Rectangle setPieceArea in setPieceAreas)
				{
					if (setPieceArea.Contains(dirtTile))
					{
						flag = true;
						break;
					}
				}
				if (!flag && getTileIndexAt(dirtTile, "Buildings") >= 0)
				{
					flag = true;
				}
				if (!flag)
				{
					Point[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						Point point = array2[i];
						Point point2 = new Point(dirtTile.X + point.X, dirtTile.Y + point.Y);
						if (dictionary.ContainsKey(point2))
						{
							if (!dictionary[point2])
							{
								flag = true;
								break;
							}
						}
						else if (!dirtTiles.Contains(point2))
						{
							if (!GetDirtNeighborTile(point2.X, point2.Y).HasValue)
							{
								flag = true;
							}
							dictionary[point2] = !flag;
							if (flag)
							{
								break;
							}
						}
					}
				}
				if (flag)
				{
					list.Add(dirtTile);
				}
			}
			foreach (Point item in list)
			{
				dirtTiles.Remove(item);
			}
		}

		public override void monsterDrop(Monster monster, int x, int y, Farmer who)
		{
			base.monsterDrop(monster, x, y, who);
			if (Game1.random.NextDouble() < 0.05)
			{
				Game1.player.team.RequestLimitedNutDrops("VolcanoMonsterDrop", this, x, y, 5);
			}
		}

		public Point? GetDirtNeighborTile(int tile_x, int tile_y)
		{
			if (GetPixel(tile_x, tile_y, Color.Black) != Color.White)
			{
				return null;
			}
			if (getTileIndexAt(new Point(tile_x, tile_y), "Buildings") >= 0)
			{
				return null;
			}
			if (dirtTiles.Contains(new Point(tile_x, tile_y - 1)) && dirtTiles.Contains(new Point(tile_x, tile_y + 1)))
			{
				return null;
			}
			if (dirtTiles.Contains(new Point(tile_x - 1, tile_y)) && dirtTiles.Contains(new Point(tile_x + 1, tile_y)))
			{
				return null;
			}
			if (dirtTiles.Contains(new Point(tile_x - 1, tile_y)) && !dirtTiles.Contains(new Point(tile_x + 1, tile_y)))
			{
				if (dirtTiles.Contains(new Point(tile_x, tile_y - 1)))
				{
					return new Point(3, 3);
				}
				if (dirtTiles.Contains(new Point(tile_x, tile_y + 1)))
				{
					return new Point(3, 1);
				}
				return new Point(2, 1);
			}
			if (dirtTiles.Contains(new Point(tile_x + 1, tile_y)) && !dirtTiles.Contains(new Point(tile_x - 1, tile_y)))
			{
				if (dirtTiles.Contains(new Point(tile_x, tile_y - 1)))
				{
					return new Point(3, 2);
				}
				if (dirtTiles.Contains(new Point(tile_x, tile_y + 1)))
				{
					return new Point(3, 0);
				}
				return new Point(0, 1);
			}
			if (dirtTiles.Contains(new Point(tile_x, tile_y - 1)) && !dirtTiles.Contains(new Point(tile_x, tile_y + 1)))
			{
				return new Point(1, 2);
			}
			if (dirtTiles.Contains(new Point(tile_x, tile_y + 1)) && !dirtTiles.Contains(new Point(tile_x, tile_y - 1)))
			{
				return new Point(1, 0);
			}
			if (dirtTiles.Contains(new Point(tile_x - 1, tile_y - 1)))
			{
				return new Point(2, 2);
			}
			if (dirtTiles.Contains(new Point(tile_x + 1, tile_y - 1)))
			{
				return new Point(0, 2);
			}
			if (dirtTiles.Contains(new Point(tile_x - 1, tile_y + 1)))
			{
				return new Point(0, 2);
			}
			if (dirtTiles.Contains(new Point(tile_x + 1, tile_y + 1)))
			{
				return new Point(2, 2);
			}
			return null;
		}

		public virtual void ErodeBadBorderedTiles()
		{
			List<Point> list = new List<Point>();
			foreach (Point dirtTile in dirtTiles)
			{
				bool flag = dirtTiles.Contains(new Point(dirtTile.X, dirtTile.Y - 1));
				bool flag2 = dirtTiles.Contains(new Point(dirtTile.X, dirtTile.Y + 1));
				bool flag3 = dirtTiles.Contains(new Point(dirtTile.X + 1, dirtTile.Y));
				bool flag4 = dirtTiles.Contains(new Point(dirtTile.X - 1, dirtTile.Y));
				if (!(flag && flag2 && flag3 && flag4))
				{
					if ((flag && flag2) || (flag && !flag2) || (flag2 && !flag))
					{
						list.Add(dirtTile);
					}
					else if ((flag3 && flag4) || (flag3 && !flag4) || (flag4 && !flag3))
					{
						list.Add(dirtTile);
					}
				}
			}
			foreach (Point item in list)
			{
				dirtTiles.Remove(item);
			}
		}

		public HashSet<Point> GetDirtBorders()
		{
			HashSet<Point> hashSet = new HashSet<Point>();
			foreach (Point dirtTile in dirtTiles)
			{
				AddBorders(dirtTile, hashSet);
			}
			return hashSet;
		}

		public void AddBorders(Point point, HashSet<Point> borders)
		{
			Point[] array = new Point[8]
			{
				new Point(-1, 0),
				new Point(-1, -1),
				new Point(0, -1),
				new Point(1, -1),
				new Point(1, 0),
				new Point(1, 1),
				new Point(0, 1),
				new Point(1, 1)
			};
			Point[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				Point point2 = array2[i];
				Point item = new Point(point.X + point2.X, point.Y + point2.Y);
				if (!dirtTiles.Contains(item) && !borders.Contains(item))
				{
					borders.Add(item);
				}
			}
		}

		public virtual void CreateDwarfGate(int gate_index, Point tile_position)
		{
			SetTile(backLayer, tile_position.X, tile_position.Y + 1, GetTileIndex(3, 34));
			SetTile(buildingsLayer, tile_position.X - 1, tile_position.Y + 1, GetTileIndex(2, 34));
			SetTile(buildingsLayer, tile_position.X + 1, tile_position.Y + 1, GetTileIndex(4, 34));
			SetTile(buildingsLayer, tile_position.X - 1, tile_position.Y, GetTileIndex(2, 33));
			SetTile(buildingsLayer, tile_position.X + 1, tile_position.Y, GetTileIndex(4, 33));
			SetTile(frontLayer, tile_position.X - 1, tile_position.Y - 1, GetTileIndex(2, 32));
			SetTile(frontLayer, tile_position.X + 1, tile_position.Y - 1, GetTileIndex(4, 32));
			SetTile(alwaysFrontLayer, tile_position.X - 1, tile_position.Y - 1, GetTileIndex(2, 32));
			SetTile(alwaysFrontLayer, tile_position.X, tile_position.Y - 1, GetTileIndex(3, 32));
			SetTile(alwaysFrontLayer, tile_position.X + 1, tile_position.Y - 1, GetTileIndex(4, 32));
			if (gate_index == 0)
			{
				SetTile(alwaysFrontLayer, tile_position.X - 1, tile_position.Y - 2, GetTileIndex(0, 32));
				SetTile(alwaysFrontLayer, tile_position.X + 1, tile_position.Y - 2, GetTileIndex(0, 32));
			}
			else
			{
				SetTile(alwaysFrontLayer, tile_position.X - 1, tile_position.Y - 2, GetTileIndex(9, 25));
				SetTile(alwaysFrontLayer, tile_position.X + 1, tile_position.Y - 2, GetTileIndex(10, 25));
			}
			int seed = generationRandom.Next();
			if (Game1.IsMasterGame)
			{
				DwarfGate item = new DwarfGate(this, gate_index, tile_position.X, tile_position.Y, seed);
				dwarfGates.Add(item);
			}
		}

		public virtual void AddPossibleSwitchLocation(int switch_index, int x, int y)
		{
			if (!possibleSwitchPositions.ContainsKey(switch_index))
			{
				possibleSwitchPositions[switch_index] = new List<Point>();
			}
			possibleSwitchPositions[switch_index].Add(new Point(x, y));
		}

		public virtual void AddPossibleGateLocation(int gate_index, int x, int y)
		{
			if (!possibleGatePositions.ContainsKey(gate_index))
			{
				possibleGatePositions[gate_index] = new List<Point>();
			}
			possibleGatePositions[gate_index].Add(new Point(x, y));
		}

		private void adjustLevelChances(ref double stoneChance, ref double monsterChance, ref double itemChance, ref double gemStoneChance)
		{
			if ((int)level == 0 || (int)level == 5)
			{
				monsterChance = 0.0;
				itemChance = 0.0;
				gemStoneChance = 0.0;
				stoneChance = 0.0;
			}
			if (isMushroomLevel())
			{
				monsterChance = 0.025;
				itemChance *= 35.0;
				stoneChance = 0.0;
			}
			else if (isMonsterLevel())
			{
				stoneChance = 0.0;
				itemChance = 0.0;
				monsterChance *= 2.0;
			}
			bool flag = false;
			bool flag2 = false;
			foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
			{
				if (onlineFarmer.hasBuff(23))
				{
					flag = true;
				}
				if (onlineFarmer.hasBuff(24))
				{
					flag2 = true;
				}
				if (flag2 && flag)
				{
					break;
				}
			}
			if (flag2)
			{
				monsterChance *= 2.0;
			}
			gemStoneChance /= 2.0;
		}

		public bool isTileClearForMineObjects(Vector2 v, bool ignoreRuins = false)
		{
			if ((Math.Abs((float)startPosition.Value.X - v.X) <= 2f && Math.Abs((float)startPosition.Value.Y - v.Y) <= 2f) || (Math.Abs((float)endPosition.Value.X - v.X) <= 2f && Math.Abs((float)endPosition.Value.Y - v.Y) <= 2f))
			{
				return false;
			}
			if (pixelMap != null && GetPixel((int)v.X, (int)v.Y, Color.Black) == new Color(128, 128, 128))
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
			if (ignoreRuins)
			{
				if (map.GetLayer("Back").Tiles[(int)v.X, (int)v.Y] == null)
				{
					return false;
				}
				if (map.GetLayer("Back").Tiles[(int)v.X, (int)v.Y].TileIndex >= 384)
				{
					return false;
				}
			}
			return true;
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
			return true;
		}

		public virtual void GenerateEntities()
		{
			List<Point> spawn_points = new List<Point>();
			ApplyToColor(new Color(0, 255, 255), delegate(int x, int y)
			{
				spawn_points.Add(new Point(x, y));
			});
			List<Point> spiker_spawn_points = new List<Point>();
			ApplyToColor(new Color(0, 128, 255), delegate(int x, int y)
			{
				spiker_spawn_points.Add(new Point(x, y));
			});
			double stoneChance = (double)generationRandom.Next(11, 18) / 150.0;
			double monsterChance = 0.0008 + (double)generationRandom.Next(70) / 10000.0;
			double itemChance = 0.001;
			double gemStoneChance = 0.003;
			adjustLevelChances(ref stoneChance, ref monsterChance, ref itemChance, ref gemStoneChance);
			int num = 0;
			if ((int)level > 0 && (int)level != 5 && (generationRandom.NextDouble() < 0.5 || isMushroomLevel()))
			{
				int num2 = generationRandom.Next(5) + (int)(Game1.player.team.AverageDailyLuck(Game1.currentLocation) * 20.0);
				if (isMushroomLevel())
				{
					num2 += 50;
				}
				for (int i = 0; i < num2; i++)
				{
					Point point;
					Point point2;
					if (generationRandom.NextDouble() < 0.33)
					{
						point = new Point(generationRandom.Next(map.GetLayer("Back").LayerWidth), 0);
						point2 = new Point(0, 1);
					}
					else if (generationRandom.NextDouble() < 0.5)
					{
						point = new Point(0, generationRandom.Next(map.GetLayer("Back").LayerHeight));
						point2 = new Point(1, 0);
					}
					else
					{
						point = new Point(map.GetLayer("Back").LayerWidth - 1, generationRandom.Next(map.GetLayer("Back").LayerHeight));
						point2 = new Point(-1, 0);
					}
					while (isTileOnMap(point.X, point.Y))
					{
						point.X += point2.X;
						point.Y += point2.Y;
						if (isTileClearForMineObjects(new Vector2(point.X, point.Y)))
						{
							Vector2 vector = new Vector2(point.X, point.Y);
							if (isMushroomLevel())
							{
								terrainFeatures.Add(vector, new CosmeticPlant(6 + generationRandom.Next(3)));
							}
							else
							{
								objects.Add(vector, new BreakableContainer(vector, isVolcano: true));
							}
							break;
						}
					}
				}
			}
			if ((int)level != 5)
			{
				for (int j = 0; j < map.Layers[0].LayerWidth; j++)
				{
					for (int k = 0; k < map.Layers[0].LayerHeight; k++)
					{
						Vector2 vector2 = new Vector2(j, k);
						if ((Math.Abs((float)startPosition.Value.X - vector2.X) <= 5f && Math.Abs((float)startPosition.Value.Y - vector2.Y) <= 5f) || (Math.Abs((float)endPosition.Value.X - vector2.X) <= 5f && Math.Abs((float)endPosition.Value.Y - vector2.Y) <= 5f))
						{
							continue;
						}
						if (isTileLocationTotallyClearAndPlaceable(vector2) && generationRandom.NextDouble() < monsterChance)
						{
							if (getTileIndexAt((int)vector2.X, (int)vector2.Y, "Back") == 25)
							{
								if (!isMushroomLevel())
								{
									characters.Add(new Duggy(vector2 * 64f, magmaDuggy: true));
								}
							}
							else if (isMushroomLevel())
							{
								characters.Add(new RockCrab(vector2 * 64f, "False Magma Cap"));
							}
							else
							{
								characters.Add(new Bat(vector2 * 64f, ((int)level > 5 && generationRandom.NextDouble() < 0.5) ? (-556) : (-555)));
							}
						}
						else
						{
							if (!isTileClearForMineObjects(vector2, ignoreRuins: true))
							{
								continue;
							}
							double num3 = stoneChance;
							if (num3 > 0.0)
							{
								foreach (Vector2 adjacentTileLocation in Utility.getAdjacentTileLocations(vector2))
								{
									if (objects.ContainsKey(adjacentTileLocation))
									{
										num3 += 0.1;
									}
								}
							}
							int num4 = chooseStoneTypeIndexOnly(vector2);
							bool flag = num4 >= 845 && num4 <= 847;
							if (num3 > 0.0 && (!flag || generationRandom.NextDouble() < num3))
							{
								Object @object = createStone(num4, vector2);
								if (@object != null)
								{
									base.Objects.Add(vector2, @object);
								}
							}
							else if (generationRandom.NextDouble() < itemChance)
							{
								Object object2 = new Object(851, 1);
								object2.IsSpawnedObject = true;
								object2.CanBeGrabbed = true;
								base.Objects.Add(vector2, object2);
							}
						}
					}
				}
				while (generationRandom.NextDouble() < 0.2)
				{
					tryToAddOreClumps();
				}
			}
			for (int l = 0; l < 7; l++)
			{
				if (spawn_points.Count == 0)
				{
					break;
				}
				int index = generationRandom.Next(0, spawn_points.Count);
				Point point3 = spawn_points[index];
				if (isTileLocationTotallyClearAndPlaceable(point3.X, point3.Y))
				{
					Monster monster = null;
					if (generationRandom.NextDouble() <= 0.25)
					{
						for (int m = 0; m < 20; m++)
						{
							Point p = point3;
							p.X += generationRandom.Next(-10, 11);
							p.Y += generationRandom.Next(-10, 11);
							bool flag2 = false;
							for (int n = -1; n <= 1; n++)
							{
								for (int num5 = -1; num5 <= 1; num5++)
								{
									if (!LavaLurk.IsLavaTile(this, p.X + n, p.Y + num5))
									{
										flag2 = true;
										break;
									}
								}
							}
							if (!flag2)
							{
								monster = new LavaLurk(Utility.PointToVector2(p) * 64f);
								break;
							}
						}
					}
					if (monster == null && generationRandom.NextDouble() <= 0.20000000298023224)
					{
						monster = new HotHead(Utility.PointToVector2(point3) * 64f);
					}
					if (monster == null)
					{
						monster = new GreenSlime(Utility.PointToVector2(point3) * 64f, 0);
						(monster as GreenSlime).makeTigerSlime();
					}
					if (monster != null)
					{
						characters.Add(monster);
					}
				}
				spawn_points.RemoveAt(index);
			}
			foreach (Point item in spiker_spawn_points)
			{
				if (isTileLocationTotallyClearAndPlaceable(item.X, item.Y))
				{
					int direction = 1;
					switch (getTileIndexAt(item, "Back"))
					{
					case 537:
					case 538:
						direction = 2;
						break;
					case 552:
					case 569:
						direction = 3;
						break;
					case 553:
					case 570:
						direction = 0;
						break;
					}
					characters.Add(new Spiker(new Vector2(item.X, item.Y) * 64f, direction));
				}
			}
		}

		private Object createStone(int stone, Vector2 tile)
		{
			int num = chooseStoneTypeIndexOnly(tile);
			int minutesUntilReady = 1;
			switch (num)
			{
			case 845:
			case 846:
			case 847:
				minutesUntilReady = 6;
				break;
			case 843:
			case 844:
				minutesUntilReady = 12;
				break;
			case 765:
				minutesUntilReady = 16;
				break;
			case 764:
				minutesUntilReady = 8;
				break;
			case 290:
				minutesUntilReady = 8;
				break;
			case 751:
				minutesUntilReady = 8;
				break;
			case 819:
				minutesUntilReady = 8;
				break;
			}
			return new Object(tile, num, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
			{
				MinutesUntilReady = minutesUntilReady
			};
		}

		private int chooseStoneTypeIndexOnly(Vector2 tile)
		{
			int num = 32;
			int num2 = 1;
			num = generationRandom.Next(845, 848);
			num2 = 8;
			float num3 = 1f + (float)(int)level / 7f;
			float num4 = 0.8f;
			float num5 = 1f + (float)Game1.player.team.AverageLuckLevel() * 0.035f + (float)Game1.player.team.AverageDailyLuck() / 2f;
			double num6 = 0.008 * (double)num3 * (double)num4 * (double)num5;
			foreach (Vector2 adjacentTileLocation in Utility.getAdjacentTileLocations(tile))
			{
				if (objects.ContainsKey(adjacentTileLocation) && ((int)objects[adjacentTileLocation].parentSheetIndex == 843 || (int)objects[adjacentTileLocation].parentSheetIndex == 844))
				{
					num6 += 0.15;
				}
			}
			if (generationRandom.NextDouble() < num6)
			{
				num = generationRandom.Next(843, 845);
				num2 = 12;
			}
			else
			{
				num6 = 0.0025 * (double)num3 * (double)num4 * (double)num5;
				foreach (Vector2 adjacentTileLocation2 in Utility.getAdjacentTileLocations(tile))
				{
					if (objects.ContainsKey(adjacentTileLocation2) && (int)objects[adjacentTileLocation2].parentSheetIndex == 765)
					{
						num6 += 0.1;
					}
				}
				if (generationRandom.NextDouble() < num6)
				{
					num = 765;
					num2 = 16;
				}
				else
				{
					num6 = 0.01 * (double)num3 * (double)num4;
					foreach (Vector2 adjacentTileLocation3 in Utility.getAdjacentTileLocations(tile))
					{
						if (objects.ContainsKey(adjacentTileLocation3) && (int)objects[adjacentTileLocation3].parentSheetIndex == 764)
						{
							num6 += 0.2;
						}
					}
					if (generationRandom.NextDouble() < num6)
					{
						num = 764;
						num2 = 12;
					}
					else
					{
						num6 = 0.015 * (double)num3 * (double)num4;
						foreach (Vector2 adjacentTileLocation4 in Utility.getAdjacentTileLocations(tile))
						{
							if (objects.ContainsKey(adjacentTileLocation4) && (int)objects[adjacentTileLocation4].parentSheetIndex == 850)
							{
								num6 += 0.25;
							}
						}
						if (generationRandom.NextDouble() < num6)
						{
							num = 850;
							num2 = 12;
						}
						else
						{
							num6 = 0.018 * (double)num3 * (double)num4;
							foreach (Vector2 adjacentTileLocation5 in Utility.getAdjacentTileLocations(tile))
							{
								if (objects.ContainsKey(adjacentTileLocation5) && (int)objects[adjacentTileLocation5].parentSheetIndex == 849)
								{
									num6 += 0.25;
								}
							}
							if (generationRandom.NextDouble() < num6)
							{
								num = 849;
								num2 = 12;
							}
						}
					}
				}
			}
			if (generationRandom.NextDouble() < 0.0005)
			{
				num = 819;
				num2 = 12;
			}
			if (generationRandom.NextDouble() < 0.0007)
			{
				num = 44;
				num2 = 12;
			}
			if ((int)level > 2 && generationRandom.NextDouble() < 0.0002)
			{
				num = 46;
				num2 = 16;
			}
			return num;
		}

		public void tryToAddOreClumps()
		{
			if (!(generationRandom.NextDouble() < 0.55 + Game1.player.team.AverageDailyLuck(Game1.currentLocation)))
			{
				return;
			}
			Vector2 randomTile = getRandomTile();
			for (int i = 0; i < 1 || generationRandom.NextDouble() < 0.25 + Game1.player.team.AverageDailyLuck(Game1.currentLocation); i++)
			{
				if (isTileLocationTotallyClearAndPlaceable(randomTile) && isTileOnClearAndSolidGround(randomTile) && doesTileHaveProperty((int)randomTile.X, (int)randomTile.Y, "Diggable", "Back") == null)
				{
					Object @object = new Object(randomTile, generationRandom.Next(843, 845), "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
					{
						MinutesUntilReady = 12
					};
					if ((int)@object.parentSheetIndex == 670)
					{
						@object.ParentSheetIndex = 668;
					}
					Utility.recursiveObjectPlacement(@object, (int)randomTile.X, (int)randomTile.Y, 0.949999988079071, 0.30000001192092896, this, "Dirt", ((int)@object.parentSheetIndex == 668) ? 1 : 0, 0.05000000074505806, ((int)@object.parentSheetIndex != 668) ? 1 : 2);
				}
				randomTile = getRandomTile();
			}
		}

		public virtual void ApplySetPieces()
		{
			for (int i = 0; i < setPieceAreas.Count; i++)
			{
				Microsoft.Xna.Framework.Rectangle value = setPieceAreas[i];
				int num = 3;
				if (value.Width >= 32)
				{
					num = 32;
				}
				else if (value.Width >= 16)
				{
					num = 16;
				}
				else if (value.Width >= 8)
				{
					num = 8;
				}
				else if (value.Width >= 4)
				{
					num = 4;
				}
				Map map = Game1.game1.xTileContent.Load<Map>("Maps\\Mines\\Volcano_SetPieces_" + num);
				int maxValue = map.Layers[0].LayerWidth / num;
				int maxValue2 = map.Layers[0].LayerHeight / num;
				int num2 = generationRandom.Next(0, maxValue);
				int num3 = generationRandom.Next(0, maxValue2);
				ApplyMapOverride(map, "area_" + i, new Microsoft.Xna.Framework.Rectangle(num2 * num, num3 * num, num, num), value);
				Layer layer = map.GetLayer("Paths");
				if (layer == null)
				{
					continue;
				}
				for (int j = 0; j < num; j++)
				{
					for (int k = 0; k <= num; k++)
					{
						int num4 = num2 * num + j;
						int num5 = num3 * num + k;
						int num6 = value.Left + j;
						int num7 = value.Top + k;
						if (!layer.IsValidTileLocation(num4, num5))
						{
							continue;
						}
						Tile tile = layer.Tiles[num4, num5];
						int num8 = -1;
						if (tile != null)
						{
							num8 = tile.TileIndex;
						}
						if (num8 >= GetTileIndex(10, 14) && num8 <= GetTileIndex(15, 14))
						{
							int num9 = num8 - GetTileIndex(10, 14);
							if (num9 > 0)
							{
								num9 += i * 10;
							}
							double result = 1.0;
							if (tile.Properties.ContainsKey("Chance") && !double.TryParse(tile.Properties["Chance"].ToString(), out result))
							{
								result = 1.0;
							}
							if (generationRandom.NextDouble() < result)
							{
								AddPossibleGateLocation(num9, num6, num7);
							}
						}
						else if (num8 >= GetTileIndex(10, 15) && num8 <= GetTileIndex(15, 15))
						{
							int num10 = num8 - GetTileIndex(10, 15);
							if (num10 > 0)
							{
								num10 += i * 10;
							}
							AddPossibleSwitchLocation(num10, num6, num7);
						}
						else if (num8 == GetTileIndex(10, 20))
						{
							SetPixelMap(num6, num7, new Color(0, 255, 255));
						}
						else if (num8 == GetTileIndex(11, 20))
						{
							SetPixelMap(num6, num7, new Color(0, 0, 255));
						}
						else if (num8 == GetTileIndex(12, 20))
						{
							SpawnChest(num6, num7);
						}
						else if (num8 == GetTileIndex(13, 20))
						{
							SetPixelMap(num6, num7, new Color(0, 0, 0));
						}
						else if (num8 == GetTileIndex(14, 20) && generationRandom.NextDouble() < 0.5)
						{
							if (Game1.IsMasterGame)
							{
								objects.Add(new Vector2(num6, num7), new BreakableContainer(new Vector2(num6, num7), isVolcano: true));
							}
						}
						else if (num8 == GetTileIndex(15, 20) && generationRandom.NextDouble() < 0.5)
						{
							if (Game1.IsMasterGame)
							{
								Object @object = new Object(852, 1);
								@object.IsSpawnedObject = true;
								@object.CanBeGrabbed = true;
								objects.Add(new Vector2(num6, num7), @object);
							}
						}
						else if (num8 == GetTileIndex(10, 21))
						{
							SetPixelMap(num6, num7, new Color(0, 128, 255));
						}
					}
				}
			}
		}

		public virtual void SpawnChest(int tile_x, int tile_y)
		{
			Random random = new Random(generationRandom.Next());
			float num = (float)Game1.player.team.AverageLuckLevel() * 0.035f + (float)Game1.player.team.AverageDailyLuck() / 2f;
			if (Game1.IsMasterGame)
			{
				Vector2 vector = new Vector2(tile_x, tile_y);
				Chest chest = new Chest(playerChest: false, vector);
				chest.dropContents.Value = true;
				chest.synchronized.Value = true;
				chest.type.Value = "interactive";
				if (random.NextDouble() < (double)(((int)level == 9) ? (0.5f + num) : (0.1f + num)))
				{
					chest.SetBigCraftableSpriteIndex(227);
					PopulateChest(chest.items, random, 1);
				}
				else
				{
					chest.SetBigCraftableSpriteIndex(223);
					PopulateChest(chest.items, random, 0);
				}
				setObject(vector, chest);
			}
		}

		protected override bool breakStone(int indexOfStone, int x, int y, Farmer who, Random r)
		{
			if (indexOfStone >= 845 && indexOfStone <= 847 && Game1.random.NextDouble() < 0.005)
			{
				Game1.createObjectDebris(827, x, y, who.uniqueMultiplayerID, this);
			}
			if (r.NextDouble() < 0.03)
			{
				Game1.player.team.RequestLimitedNutDrops("VolcanoMining", this, x * 64, y * 64, 5);
			}
			return base.breakStone(indexOfStone, x, y, who, r);
		}

		public virtual void PopulateChest(NetObjectList<Item> items, Random chest_random, int chest_type)
		{
			if (chest_type == 0)
			{
				int maxValue = 7;
				int num = chest_random.Next(maxValue);
				if (!Game1.netWorldState.Value.GoldenCoconutCracked.Value)
				{
					while (num == 1)
					{
						num = chest_random.Next(maxValue);
					}
				}
				if (Game1.random.NextDouble() <= 0.5 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
				{
					int num2 = chest_random.Next(2, 6);
					for (int i = 0; i < num2; i++)
					{
						items.Add(new Object(890, 1));
					}
				}
				switch (num)
				{
				case 0:
				{
					for (int l = 0; l < 3; l++)
					{
						items.Add(new Object(848, 1));
					}
					break;
				}
				case 1:
					items.Add(new Object(791, 1));
					break;
				case 2:
				{
					for (int k = 0; k < 8; k++)
					{
						items.Add(new Object(831, 1));
					}
					break;
				}
				case 3:
				{
					for (int j = 0; j < 5; j++)
					{
						items.Add(new Object(833, 1));
					}
					break;
				}
				case 4:
					items.Add(new Ring(861));
					break;
				case 5:
					items.Add(new Ring(862));
					break;
				case 6:
					items.Add(new MeleeWeapon(chest_random.Next(54, 57)));
					break;
				}
			}
			if (chest_type != 1)
			{
				return;
			}
			int maxValue2 = 9;
			int num3 = chest_random.Next(maxValue2);
			if (!Game1.netWorldState.Value.GoldenCoconutCracked.Value)
			{
				while (num3 == 3)
				{
					num3 = chest_random.Next(maxValue2);
				}
			}
			if (Game1.random.NextDouble() <= 1.0 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
			{
				int num4 = chest_random.Next(4, 6);
				for (int m = 0; m < num4; m++)
				{
					items.Add(new Object(890, 1));
				}
			}
			switch (num3)
			{
			case 0:
			{
				for (int num5 = 0; num5 < 10; num5++)
				{
					items.Add(new Object(848, 1));
				}
				break;
			}
			case 1:
				items.Add(new Boots(854));
				break;
			case 2:
				items.Add(new Boots(855));
				break;
			case 3:
			{
				for (int n = 0; n < 3; n++)
				{
					items.Add(new Object(Vector2.Zero, 791, 1));
				}
				break;
			}
			case 4:
				items.Add(new Ring(863));
				break;
			case 5:
				items.Add(new Ring(860));
				break;
			case 6:
				items.Add(new MeleeWeapon(chest_random.Next(57, 60)));
				break;
			case 7:
				items.Add(new Hat(76));
				break;
			case 8:
				items.Add(new Object(289, 1));
				break;
			}
		}

		public virtual void ApplyToColor(Color match, Action<int, int> action)
		{
			for (int i = 0; i < mapWidth; i++)
			{
				for (int j = 0; j < mapHeight; j++)
				{
					if (GetPixel(i, j, match) == match)
					{
						action?.Invoke(i, j);
					}
				}
			}
		}

		public override bool sinkDebris(Debris debris, Vector2 chunkTile, Vector2 chunkPosition)
		{
			if (cooledLavaTiles.ContainsKey(chunkTile))
			{
				return false;
			}
			return base.sinkDebris(debris, chunkTile, chunkPosition);
		}

		public override bool performToolAction(Tool t, int tileX, int tileY)
		{
			if ((int)level != 5 && t is WateringCan && isTileOnMap(new Vector2(tileX, tileY)) && waterTiles[tileX, tileY] && !cooledLavaTiles.ContainsKey(new Vector2(tileX, tileY)))
			{
				coolLavaEvent.Fire(new Point(tileX, tileY));
			}
			return base.performToolAction(t, tileX, tileY);
		}

		public virtual void GenerateBlobs(Color match, int tile_x, int tile_y, bool fill_center = true, bool is_lava_pool = false)
		{
			for (int i = 0; i < mapWidth; i++)
			{
				for (int j = 0; j < mapHeight; j++)
				{
					if (!(GetPixel(i, j, match) == match))
					{
						continue;
					}
					int neighborValue = GetNeighborValue(i, j, match, is_lava_pool);
					if (fill_center || neighborValue != 15)
					{
						Dictionary<int, Point> dictionary = GetBlobLookup();
						if (is_lava_pool)
						{
							dictionary = GetLavaBlobLookup();
						}
						if (dictionary.ContainsKey(neighborValue))
						{
							Point point = dictionary[neighborValue];
							SetTile(buildingsLayer, i, j, GetTileIndex(tile_x + point.X, tile_y + point.Y));
						}
					}
				}
			}
		}

		public Dictionary<int, Point> GetBlobLookup()
		{
			if (_blobIndexLookup == null)
			{
				_blobIndexLookup = new Dictionary<int, Point>();
				_blobIndexLookup[0] = new Point(0, 0);
				_blobIndexLookup[6] = new Point(1, 0);
				_blobIndexLookup[14] = new Point(2, 0);
				_blobIndexLookup[10] = new Point(3, 0);
				_blobIndexLookup[7] = new Point(1, 1);
				_blobIndexLookup[11] = new Point(3, 1);
				_blobIndexLookup[5] = new Point(1, 2);
				_blobIndexLookup[13] = new Point(2, 2);
				_blobIndexLookup[9] = new Point(3, 2);
				_blobIndexLookup[2] = new Point(0, 1);
				_blobIndexLookup[3] = new Point(0, 2);
				_blobIndexLookup[1] = new Point(0, 3);
				_blobIndexLookup[4] = new Point(1, 3);
				_blobIndexLookup[12] = new Point(2, 3);
				_blobIndexLookup[8] = new Point(3, 3);
				_blobIndexLookup[15] = new Point(2, 1);
			}
			return _blobIndexLookup;
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false)
		{
			if (isFarmer && !glider && (position.Left < 0 || position.Right > map.DisplayWidth || position.Top < 0 || position.Bottom > map.DisplayHeight))
			{
				return true;
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding, projectile, ignoreCharacterRequirement);
		}

		public Dictionary<int, Point> GetLavaBlobLookup()
		{
			if (_lavaBlobIndexLookup == null)
			{
				_lavaBlobIndexLookup = new Dictionary<int, Point>(GetBlobLookup());
				_lavaBlobIndexLookup[63] = new Point(2, 1);
				_lavaBlobIndexLookup[47] = new Point(4, 3);
				_lavaBlobIndexLookup[31] = new Point(4, 2);
				_lavaBlobIndexLookup[15] = new Point(4, 1);
			}
			return _lavaBlobIndexLookup;
		}

		public virtual void GenerateWalls(Color match, int source_x, int source_y, int wall_height = 4, int random_wall_variants = 1, bool start_in_wall = false, Action<int, int> on_insufficient_wall_height = null, bool use_corner_hack = false)
		{
			heightMap = new int[mapWidth * mapHeight];
			for (int i = 0; i < heightMap.Length; i++)
			{
				heightMap[i] = -1;
			}
			for (int j = 0; j < 2; j++)
			{
				for (int k = 0; k < mapWidth; k++)
				{
					int num = -1;
					int num2 = 0;
					if (start_in_wall)
					{
						num2 = wall_height;
					}
					for (int l = 0; l <= mapHeight; l++)
					{
						Color pixel = GetPixel(k, l, match);
						if (pixel != match || l >= mapHeight)
						{
							int num3 = 0;
							int num4 = 0;
							if (random_wall_variants > 1 && generationRandom.NextDouble() < 0.5)
							{
								num4 = generationRandom.Next(1, random_wall_variants);
							}
							if (l >= mapHeight)
							{
								num3 = wall_height;
								num2 = wall_height;
							}
							for (int num5 = l - 1; num5 > num; num5--)
							{
								if (num2 < wall_height)
								{
									if (on_insufficient_wall_height != null)
									{
										on_insufficient_wall_height(k, num5);
									}
									else
									{
										SetPixelMap(k, num5, Color.White);
										PlaceSingleWall(k, num5);
									}
									num3--;
								}
								else if (j == 0 && GetPixelClearance(k - 1, num5, wall_height, match) < wall_height && GetPixelClearance(k + 1, num5, wall_height, match) < wall_height)
								{
									if (on_insufficient_wall_height != null)
									{
										on_insufficient_wall_height(k, num5);
									}
									else
									{
										SetPixelMap(k, num5, Color.White);
										PlaceSingleWall(k, num5);
									}
									num3--;
								}
								else if (j == 1)
								{
									heightMap[k + num5 * mapWidth] = num3 + 1;
									if (num3 < wall_height || wall_height == 0)
									{
										if (wall_height > 0)
										{
											SetTile(buildingsLayer, k, num5, GetTileIndex(source_x + random_wall_variants + num4, source_y + 1 + random_wall_variants + wall_height - num3 - 1));
										}
									}
									else
									{
										SetTile(buildingsLayer, k, num5, GetTileIndex(source_x + random_wall_variants * 3, source_y));
									}
								}
								if (num3 < wall_height)
								{
									num3++;
								}
							}
							num = l;
							num2 = 0;
						}
						else
						{
							num2++;
						}
					}
				}
			}
			List<Point> list = new List<Point>();
			for (int m = 0; m < mapHeight; m++)
			{
				for (int n = 0; n < mapWidth; n++)
				{
					int height = GetHeight(n, m, wall_height);
					int height2 = GetHeight(n - 1, m, wall_height);
					int height3 = GetHeight(n + 1, m, wall_height);
					int height4 = GetHeight(n, m - 1, wall_height);
					int num6 = generationRandom.Next(0, random_wall_variants);
					if (height3 < height)
					{
						if (height3 == wall_height)
						{
							if (use_corner_hack)
							{
								list.Add(new Point(n, m));
								SetTile(buildingsLayer, n, m, GetTileIndex(source_x + random_wall_variants * 3, source_y));
							}
							else
							{
								SetTile(buildingsLayer, n, m, GetTileIndex(source_x + random_wall_variants * 3, source_y + 1));
							}
						}
						else
						{
							Layer layer = buildingsLayer;
							if (height3 >= 0)
							{
								SetTile(buildingsLayer, n, m, GetTileIndex(source_x + random_wall_variants, source_y + 1 + random_wall_variants + wall_height - height3));
								layer = frontLayer;
							}
							if (height > wall_height)
							{
								SetTile(layer, n, m, GetTileIndex(source_x + random_wall_variants * 3 - 1, source_y + 1 + num6));
							}
							else
							{
								SetTile(layer, n, m, GetTileIndex(source_x + random_wall_variants * 2 + num6, source_y + 1 + random_wall_variants * 2 + 1 - height - 1));
							}
							if (wall_height > 0 && m + 1 < mapHeight && height3 == -1 && GetHeight(n + 1, m + 1, wall_height) >= 0 && GetHeight(n, m + 1, wall_height) >= 0)
							{
								if (use_corner_hack)
								{
									list.Add(new Point(n, m));
									SetTile(buildingsLayer, n, m, GetTileIndex(source_x + random_wall_variants * 3, source_y));
								}
								else
								{
									SetTile(frontLayer, n, m, GetTileIndex(source_x + random_wall_variants * 3, source_y + 2));
								}
							}
						}
					}
					else if (height2 < height)
					{
						if (height2 == wall_height)
						{
							if (use_corner_hack)
							{
								list.Add(new Point(n, m));
								SetTile(buildingsLayer, n, m, GetTileIndex(source_x + random_wall_variants * 3, source_y));
							}
							else
							{
								SetTile(buildingsLayer, n, m, GetTileIndex(source_x + random_wall_variants * 3 + 1, source_y + 1));
							}
						}
						else
						{
							Layer layer2 = buildingsLayer;
							if (height2 >= 0)
							{
								SetTile(buildingsLayer, n, m, GetTileIndex(source_x + random_wall_variants, source_y + 1 + random_wall_variants + wall_height - height2));
								layer2 = frontLayer;
							}
							if (height > wall_height)
							{
								SetTile(layer2, n, m, GetTileIndex(source_x, source_y + 1 + num6));
							}
							else
							{
								SetTile(layer2, n, m, GetTileIndex(source_x + num6, source_y + 1 + random_wall_variants * 2 + 1 - height - 1));
							}
							if (wall_height > 0 && m + 1 < mapHeight && height2 == -1 && GetHeight(n - 1, m + 1, wall_height) >= 0 && GetHeight(n, m + 1, wall_height) >= 0)
							{
								if (use_corner_hack)
								{
									list.Add(new Point(n, m));
									SetTile(buildingsLayer, n, m, GetTileIndex(source_x + random_wall_variants * 3, source_y));
								}
								else
								{
									SetTile(frontLayer, n, m, GetTileIndex(source_x + random_wall_variants * 3 + 1, source_y + 2));
								}
							}
						}
					}
					if (height < 0 || height4 != -1)
					{
						continue;
					}
					if (wall_height > 0)
					{
						if (height3 == -1)
						{
							SetTile(frontLayer, n, m - 1, GetTileIndex(source_x + random_wall_variants * 2 + num6, source_y));
						}
						else if (height2 == -1)
						{
							SetTile(frontLayer, n, m - 1, GetTileIndex(source_x + num6, source_y));
						}
						else
						{
							SetTile(frontLayer, n, m - 1, GetTileIndex(source_x + random_wall_variants + num6, source_y));
						}
					}
					else if (height3 == -1)
					{
						SetTile(buildingsLayer, n, m, GetTileIndex(source_x + random_wall_variants * 2 + num6, source_y));
					}
					else if (height2 == -1)
					{
						SetTile(buildingsLayer, n, m, GetTileIndex(source_x + num6, source_y));
					}
					else
					{
						SetTile(buildingsLayer, n, m, GetTileIndex(source_x + random_wall_variants + num6, source_y));
					}
				}
			}
			if (use_corner_hack)
			{
				foreach (Point item in list)
				{
					if (GetHeight(item.X - 1, item.Y, wall_height) == -1)
					{
						SetTile(frontLayer, item.X, item.Y, GetTileIndex(source_x + random_wall_variants * 3 + 1, source_y + 2));
					}
					else if (GetHeight(item.X + 1, item.Y, wall_height) == -1)
					{
						SetTile(frontLayer, item.X, item.Y, GetTileIndex(source_x + random_wall_variants * 3, source_y + 2));
					}
					if (GetHeight(item.X - 1, item.Y, wall_height) == wall_height)
					{
						SetTile(alwaysFrontLayer, item.X, item.Y, GetTileIndex(source_x + random_wall_variants * 3 + 1, source_y + 1));
					}
					else if (GetHeight(item.X + 1, item.Y, wall_height) == wall_height)
					{
						SetTile(alwaysFrontLayer, item.X, item.Y, GetTileIndex(source_x + random_wall_variants * 3, source_y + 1));
					}
				}
			}
			heightMap = null;
		}

		public int GetPixelClearance(int x, int y, int wall_height, Color match)
		{
			int num = 0;
			if (GetPixel(x, y, Color.White) == match)
			{
				num++;
				for (int i = 1; i < wall_height; i++)
				{
					if (num >= wall_height)
					{
						break;
					}
					if (y + i >= mapHeight)
					{
						return wall_height;
					}
					if (!(GetPixel(x, y + i, Color.White) == match))
					{
						break;
					}
					num++;
				}
				for (int j = 1; j < wall_height; j++)
				{
					if (num >= wall_height)
					{
						break;
					}
					if (y - j < 0)
					{
						return wall_height;
					}
					if (!(GetPixel(x, y - j, Color.White) == match))
					{
						break;
					}
					num++;
				}
				return num;
			}
			return 0;
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			coolLavaEvent.Poll();
			lavaSoundsPlayedThisTick = 0;
			if ((int)level == 0 && Game1.currentLocation == this)
			{
				steamTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
				if (steamTimer < 0f)
				{
					steamTimer = 5000f;
					Game1.playSound("cavedrip");
					temporarySprites.Add(new TemporaryAnimatedSprite(null, new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1), new Vector2(34.5f, 30.75f) * 64f, flipped: false, 0f, Color.White)
					{
						texture = Game1.staminaRect,
						color = new Color(100, 150, 255),
						alpha = 0.75f,
						motion = new Vector2(0f, 1f),
						acceleration = new Vector2(0f, 0.1f),
						interval = 99999f,
						layerDepth = 1f,
						scale = 8f,
						id = 89898f,
						yStopCoordinate = 2208,
						reachedStopCoordinate = delegate
						{
							removeTemporarySpritesWithID(89898);
							Game1.playSound("steam");
							for (int i = 0; i < 4; i++)
							{
								temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(33.75f, 33.5f) * 64f + new Vector2(Game1.random.Next(64), Game1.random.Next(64)), flipped: false, 0.007f, Color.White)
								{
									alpha = 0.75f,
									motion = new Vector2(0f, -1f),
									acceleration = new Vector2(0.002f, 0f),
									interval = 99999f,
									layerDepth = 1f,
									scale = 4f,
									scaleChange = 0.02f,
									rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f
								});
							}
						}
					});
				}
			}
			foreach (DwarfGate dwarfGate in dwarfGates)
			{
				dwarfGate.UpdateWhenCurrentLocation(time, this);
			}
			if (!_sawFlameSprite && Utility.isThereAFarmerWithinDistance(new Vector2(30f, 38f), 3, this) != null)
			{
				Game1.addMailForTomorrow("Saw_Flame_Sprite_Volcano", noLetter: true);
				TemporaryAnimatedSprite temporarySpriteByID = getTemporarySpriteByID(999);
				if (temporarySpriteByID != null)
				{
					temporarySpriteByID.yPeriodic = false;
					temporarySpriteByID.xPeriodic = false;
					temporarySpriteByID.sourceRect.Y = 0;
					temporarySpriteByID.sourceRectStartingPos.Y = 0f;
					temporarySpriteByID.motion = new Vector2(0f, -4f);
					temporarySpriteByID.acceleration = new Vector2(0f, -0.04f);
				}
				localSound("magma_sprite_spot");
				temporarySpriteByID = getTemporarySpriteByID(998);
				if (temporarySpriteByID != null)
				{
					temporarySpriteByID.yPeriodic = false;
					temporarySpriteByID.xPeriodic = false;
					temporarySpriteByID.motion = new Vector2(0f, -4f);
					temporarySpriteByID.acceleration = new Vector2(0f, -0.04f);
				}
				_sawFlameSprite = true;
			}
		}

		public virtual void PlaceGroundTile(int x, int y)
		{
			if (generationRandom.NextDouble() < 0.30000001192092896)
			{
				SetTile(backLayer, x, y, GetTileIndex(1 + generationRandom.Next(0, 3), generationRandom.Next(0, 2)));
			}
			else
			{
				SetTile(backLayer, x, y, GetTileIndex(1, 0));
			}
		}

		public override void drawFloorDecorations(SpriteBatch b)
		{
			base.drawFloorDecorations(b);
			Vector2 key = default(Vector2);
			Point value = default(Point);
			for (int i = Game1.viewport.Y / 64 - 1; i < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 1; i++)
			{
				for (int j = Game1.viewport.X / 64 - 1; j < (Game1.viewport.X + Game1.viewport.Width) / 64 + 1; j++)
				{
					key.X = j;
					key.Y = i;
					if (localCooledLavaTiles.TryGetValue(key, out value))
					{
						value.X += 5;
						value.Y += 16;
						b.Draw(mapBaseTilesheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(j * 64, i * 64)), new Microsoft.Xna.Framework.Rectangle(value.X * 16, value.Y * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.55f);
					}
				}
			}
		}

		public override void drawWaterTile(SpriteBatch b, int x, int y)
		{
			if ((int)level == 5)
			{
				base.drawWaterTile(b, x, y);
				return;
			}
			if ((int)level == 0 && x > 23 && x < 28 && y > 42 && y < 47)
			{
				drawWaterTile(b, x, y, Color.DeepSkyBlue * 0.8f);
				return;
			}
			bool flag = y == map.Layers[0].LayerHeight - 1 || !waterTiles[x, y + 1];
			bool flag2 = y == 0 || !waterTiles[x, y - 1];
			int num = 0;
			int num2 = 320;
			b.Draw(mapBaseTilesheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - (int)((!flag2) ? waterPosition : 0f))), new Microsoft.Xna.Framework.Rectangle(num + waterAnimationIndex * 16, num2 + (((x + y) % 2 != 0) ? ((!waterTileFlip) ? 32 : 0) : (waterTileFlip ? 32 : 0)) + (flag2 ? ((int)waterPosition / 4) : 0), 16, 16 + (flag2 ? ((int)(0f - waterPosition) / 4) : 0)), waterColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.56f);
			if (flag)
			{
				b.Draw(mapBaseTilesheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y + 1) * 64 - (int)waterPosition)), new Microsoft.Xna.Framework.Rectangle(num + waterAnimationIndex * 16, num2 + (((x + (y + 1)) % 2 != 0) ? ((!waterTileFlip) ? 32 : 0) : (waterTileFlip ? 32 : 0)), 16, 16 - (int)(16f - waterPosition / 4f) - 1), waterColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.56f);
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (DwarfGate dwarfGate in dwarfGates)
			{
				dwarfGate.Draw(b);
			}
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			if (!Game1.game1.takingMapScreenshot && (int)level > 0)
			{
				int color = 2;
				string s = level?.ToString() ?? "";
				Microsoft.Xna.Framework.Rectangle titleSafeArea = Game1.game1.GraphicsDevice.Viewport.GetTitleSafeArea();
				SpriteText.drawString(b, s, titleSafeArea.Left + 16, titleSafeArea.Top + 16, 999999, -1, 999999, 1f, 1f, junimoText: false, 2, "", color);
			}
		}

		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			if (!(Game1.random.NextDouble() < 0.1) || (int)level <= 0 || (int)level == 5)
			{
				return;
			}
			int num = 0;
			foreach (NPC character in characters)
			{
				if (character is Bat)
				{
					num++;
				}
			}
			if (num < farmers.Count * 4)
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
			playSound("magma_sprite_spot");
			characters.Add(new Bat(zero, ((int)level > 5 && Game1.random.NextDouble() < 0.5) ? (-556) : (-555))
			{
				focusedOnFarmers = true
			});
		}

		public virtual void PlaceSingleWall(int x, int y)
		{
			int x2 = generationRandom.Next(0, 4);
			SetTile(frontLayer, x, y - 1, GetTileIndex(x2, 2));
			SetTile(buildingsLayer, x, y, GetTileIndex(x2, 3));
		}

		public virtual void ApplyPixels(string layout_texture_name, int source_x = 0, int source_y = 0, int width = 64, int height = 64, int x_offset = 0, int y_offset = 0, bool flip_x = false)
		{
			Texture2D texture2D = Game1.temporaryContent.Load<Texture2D>(layout_texture_name);
			Color[] array = new Color[width * height];
			texture2D.GetData(0, new Microsoft.Xna.Framework.Rectangle(source_x, source_y, width, height), array, 0, width * height);
			for (int i = 0; i < width; i++)
			{
				int num = i + x_offset;
				if (flip_x)
				{
					num = x_offset + width - 1 - i;
				}
				if (num < 0 || num >= mapWidth)
				{
					continue;
				}
				for (int j = 0; j < height; j++)
				{
					int num2 = j + y_offset;
					if (num2 >= 0 && num2 < mapHeight)
					{
						Color pixelColor = GetPixelColor(width, height, array, i, j);
						SetPixelMap(num, num2, pixelColor);
					}
				}
			}
		}

		public int GetHeight(int x, int y, int max_height)
		{
			if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
			{
				return max_height + 1;
			}
			return heightMap[x + y * mapWidth];
		}

		public Color GetPixel(int x, int y, Color out_of_bounds_color)
		{
			if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
			{
				return out_of_bounds_color;
			}
			return pixelMap[x + y * mapWidth];
		}

		public void SetPixelMap(int x, int y, Color color)
		{
			if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
			{
				pixelMap[x + y * mapWidth] = color;
			}
		}

		public int GetNeighborValue(int x, int y, Color matched_color, bool is_lava_pool = false)
		{
			int num = 0;
			if (GetPixel(x, y - 1, matched_color) == matched_color)
			{
				num++;
			}
			if (GetPixel(x, y + 1, matched_color) == matched_color)
			{
				num += 2;
			}
			if (GetPixel(x + 1, y, matched_color) == matched_color)
			{
				num += 4;
			}
			if (GetPixel(x - 1, y, matched_color) == matched_color)
			{
				num += 8;
			}
			if (is_lava_pool && num == 15)
			{
				if (GetPixel(x - 1, y - 1, matched_color) == matched_color)
				{
					num += 16;
				}
				if (GetPixel(x + 1, y - 1, matched_color) == matched_color)
				{
					num += 32;
				}
			}
			return num;
		}

		public Color GetPixelColor(int width, int height, Color[] pixels, int x, int y)
		{
			if (x < 0 || x >= width)
			{
				return Color.Black;
			}
			if (y < 0 || y >= height)
			{
				return Color.Black;
			}
			int num = x + y * width;
			return pixels[num];
		}

		public static int GetTileIndex(int x, int y)
		{
			return x + y * 16;
		}

		public void SetTile(Layer layer, int x, int y, int index)
		{
			if (x >= 0 && x < layer.LayerWidth && y >= 0 && y < layer.LayerHeight)
			{
				Location location = new Location(x, y);
				layer.Tiles[location] = new StaticTile(layer, map.TileSheets[0], BlendMode.Alpha, index);
			}
		}

		public int GetMaxRoomLayouts()
		{
			return 30;
		}

		public static VolcanoDungeon GetLevel(string name, bool use_level_level_as_layout = false)
		{
			foreach (VolcanoDungeon activeLevel in activeLevels)
			{
				if (activeLevel.Name.Equals(name))
				{
					return activeLevel;
				}
			}
			int num = Convert.ToInt32(name.Substring("VolcanoDungeon".Length));
			VolcanoDungeon volcanoDungeon = new VolcanoDungeon(num);
			activeLevels.Add(volcanoDungeon);
			if (Game1.IsMasterGame)
			{
				volcanoDungeon.GenerateContents(use_level_level_as_layout);
			}
			else
			{
				volcanoDungeon.reloadMap();
			}
			return volcanoDungeon;
		}

		public static void UpdateLevels(GameTime time)
		{
			foreach (VolcanoDungeon activeLevel in activeLevels)
			{
				if (activeLevel.farmers.Count() > 0)
				{
					activeLevel.UpdateWhenCurrentLocation(time);
				}
				activeLevel.updateEvenIfFarmerIsntHere(time);
			}
		}

		public static void UpdateLevels10Minutes(int timeOfDay)
		{
			ClearInactiveLevels();
			if (Game1.IsClient)
			{
				return;
			}
			foreach (VolcanoDungeon activeLevel in activeLevels)
			{
				if (activeLevel.farmers.Count() > 0)
				{
					activeLevel.performTenMinuteUpdate(timeOfDay);
				}
			}
		}

		private static void ClearInactiveLevels(bool keepUntickedLevels = true)
		{
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			if (Game1.locationRequest != null && !Game1.locationRequest.Name.Contains("Volcano"))
			{
				Game1.changeMusicTrack("none");
			}
		}

		public static void ClearAllLevels()
		{
			activeLevels.RemoveAll(delegate(VolcanoDungeon level)
			{
				level.CleanUp();
				level.mapContent.Dispose();
				return true;
			});
		}

		public virtual void CleanUp()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			int num = 0;
			while (num < base.debris.Count)
			{
				Debris debris = base.debris[num];
				if (debris.isEssentialItem() && Game1.IsMasterGame && debris.collect(Game1.player))
				{
					base.debris.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
		}

		public static void ForEach(Action<VolcanoDungeon> action)
		{
			foreach (VolcanoDungeon activeLevel in activeLevels)
			{
				action(activeLevel);
			}
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings") == 367)
			{
				createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Volcano_ShortcutOut"), createYesNoResponses(), "LeaveVolcano");
				return true;
			}
			if (getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings") == 77)
			{
				if (Game1.player.canUnderstandDwarves)
				{
					Random random = new Random((int)(Game1.stats.DaysPlayed + 898 + Game1.uniqueIDForThisGame));
					Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
					dictionary.Add(new Boots(853), new int[4] { 0, 2147483647, 848, 100 });
					Utility.AddStock(dictionary, new Object(Vector2.Zero, 286, 2147483647), 150);
					Utility.AddStock(dictionary, new Object(Vector2.Zero, 287, 2147483647), 300);
					Utility.AddStock(dictionary, new Object(Vector2.Zero, 288, 2147483647), 500);
					if (random.NextDouble() < 0.5)
					{
						Utility.AddStock(dictionary, new Object(Vector2.Zero, 244, 2147483647), 600);
					}
					else
					{
						Utility.AddStock(dictionary, new Object(Vector2.Zero, 237, 2147483647), 600);
					}
					if (random.NextDouble() < 0.25)
					{
						Utility.AddStock(dictionary, new Hat(77), 5000);
					}
					if (!Game1.player.craftingRecipes.ContainsKey("Warp Totem: Island"))
					{
						Utility.AddStock(dictionary, new Object(886, 1, isRecipe: true), 5000);
					}
					if (!Game1.player.cookingRecipes.ContainsKey("Ginger Ale"))
					{
						Utility.AddStock(dictionary, new Object(903, 1, isRecipe: true), 500);
					}
					Game1.activeClickableMenu = new ShopMenu(dictionary, 0, "VolcanoShop", null, null, "VolcanoShop");
				}
				else
				{
					Game1.player.doEmote(8);
				}
				return true;
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public override void performTouchAction(string full_action_string, Vector2 player_standing_position)
		{
			if (Game1.eventUp)
			{
				return;
			}
			string text = full_action_string.Split(' ')[0];
			if (text == "DwarfSwitch")
			{
				Point point = new Point((int)player_standing_position.X, (int)player_standing_position.Y);
				foreach (DwarfGate dwarfGate in dwarfGates)
				{
					if (dwarfGate.switches.ContainsKey(point) && !dwarfGate.switches[point])
					{
						dwarfGate.pressEvent.Fire(point);
					}
				}
			}
			base.performTouchAction(full_action_string, player_standing_position);
		}
	}
}
