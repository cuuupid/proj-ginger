using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class IslandSouth : IslandLocation
	{
		public class IslandActivityAssigments
		{
			public int activityTime;

			public List<NPC> visitors;

			public Dictionary<Character, string> currentAssignments;

			public Dictionary<Character, string> currentAnimationAssignments;

			public Random random;

			public Dictionary<string, string> animationDescriptions;

			public List<Point> shoreLoungePoints = new List<Point>(new Point[6]
			{
				new Point(9, 33),
				new Point(13, 33),
				new Point(17, 33),
				new Point(24, 33),
				new Point(28, 32),
				new Point(32, 31)
			});

			public List<Point> chairPoints = new List<Point>(new Point[2]
			{
				new Point(20, 24),
				new Point(30, 29)
			});

			public List<Point> umbrellaPoints = new List<Point>(new Point[3]
			{
				new Point(26, 26),
				new Point(28, 29),
				new Point(10, 27)
			});

			public List<Point> dancePoints = new List<Point>(new Point[2]
			{
				new Point(22, 21),
				new Point(23, 21)
			});

			public List<Point> towelLoungePoints = new List<Point>(new Point[4]
			{
				new Point(14, 27),
				new Point(17, 28),
				new Point(20, 27),
				new Point(23, 28)
			});

			public List<Point> drinkPoints = new List<Point>(new Point[2]
			{
				new Point(12, 23),
				new Point(15, 23)
			});

			public List<Point> wanderPoints = new List<Point>(new Point[3]
			{
				new Point(7, 16),
				new Point(31, 24),
				new Point(18, 13)
			});

			public IslandActivityAssigments(int time, List<NPC> visitors, Random seeded_random, Dictionary<Character, string> last_activity_assignments)
			{
				activityTime = time;
				this.visitors = new List<NPC>(visitors);
				random = seeded_random;
				Utility.Shuffle(random, this.visitors);
				animationDescriptions = Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions");
				FindActivityForCharacters(last_activity_assignments);
			}

			public virtual void FindActivityForCharacters(Dictionary<Character, string> last_activity_assignments)
			{
				currentAssignments = new Dictionary<Character, string>();
				currentAnimationAssignments = new Dictionary<Character, string>();
				foreach (NPC visitor in visitors)
				{
					if (currentAssignments.ContainsKey(visitor))
					{
						continue;
					}
					if (visitor.Name == "Gus")
					{
						currentAssignments[visitor] = "14 21 2";
						foreach (NPC visitor2 in visitors)
						{
							if (!currentAssignments.ContainsKey(visitor2) && visitor2.Age != 2)
							{
								TryAssignment(visitor2, drinkPoints, "Resort_Bar", visitor2.name.Value.ToLower() + "_beach_drink", animation_required: false, 0.5, last_activity_assignments);
							}
						}
					}
					if (!(visitor.Name == "Sam") || !TryAssignment(visitor, towelLoungePoints, "Resort_Towel", visitor.name.Value.ToLower() + "_beach_towel", animation_required: true, 0.5, last_activity_assignments))
					{
						continue;
					}
					foreach (NPC visitor3 in visitors)
					{
						if (!currentAssignments.ContainsKey(visitor3) && animationDescriptions.ContainsKey(visitor3.Name.ToLower() + "_beach_dance"))
						{
							int num = int.Parse(currentAssignments[visitor].Split(' ')[0]);
							int num2 = int.Parse(currentAssignments[visitor].Split(' ')[1]);
							currentAssignments.Remove(visitor3);
							TryAssignment(visitor3, new List<Point>(new Point[1]
							{
								new Point(num + 1, num2 + 1)
							}), "Resort_Dance", visitor3.Name.ToLower() + "_beach_dance", animation_required: true, 1.0, last_activity_assignments);
							visitor3.currentScheduleDelay = 0f;
							visitor.currentScheduleDelay = 0f;
							break;
						}
					}
				}
				foreach (NPC visitor4 in visitors)
				{
					if (!currentAssignments.ContainsKey(visitor4) && !TryAssignment(visitor4, towelLoungePoints, "Resort_Towel", visitor4.name.Value.ToLower() + "_beach_towel", animation_required: true, 0.5, last_activity_assignments) && !TryAssignment(visitor4, wanderPoints, "Resort_Wander", "square_3_3", animation_required: false, 0.4, last_activity_assignments) && !TryAssignment(visitor4, umbrellaPoints, "Resort_Umbrella", visitor4.name.Value.ToLower() + "_beach_umbrella", animation_required: true, (visitor4.Name == "Abigail") ? 0.5 : 0.1) && (visitor4.Age != 0 || !TryAssignment(visitor4, chairPoints, "Resort_Chair", "_beach_chair", animation_required: false, 0.4, last_activity_assignments)))
					{
						TryAssignment(visitor4, shoreLoungePoints, "Resort_Shore", null, animation_required: false, 1.0, last_activity_assignments);
					}
				}
				last_activity_assignments.Clear();
				foreach (Character key in currentAnimationAssignments.Keys)
				{
					last_activity_assignments[key] = currentAnimationAssignments[key];
				}
			}

			public bool TryAssignment(Character character, List<Point> points, string dialogue_key, string animation_name = null, bool animation_required = false, double chance = 1.0, Dictionary<Character, string> last_activity_assignments = null)
			{
				if (last_activity_assignments != null && animation_name != "" && animation_name != null && !animation_name.StartsWith("square_") && last_activity_assignments.ContainsKey(character) && last_activity_assignments[character] == animation_name)
				{
					return false;
				}
				if (points.Count > 0 && (random.NextDouble() < chance || chance >= 1.0))
				{
					Point item = Utility.GetRandom(points, random);
					if (animation_name != null && animation_name != "" && !animation_name.StartsWith("square_") && !animationDescriptions.ContainsKey(animation_name))
					{
						if (animation_required)
						{
							return false;
						}
						animation_name = null;
					}
					string text = "";
					text = ((!(animation_name == "") && animation_name != null) ? (item.X + " " + item.Y + " " + animation_name) : (item.X + " " + item.Y + " 2"));
					if (dialogue_key != null)
					{
						dialogue_key = GetRandomDialogueKey("Characters\\Dialogue\\" + character.Name + ":" + dialogue_key, random);
						if (dialogue_key == null)
						{
							dialogue_key = GetRandomDialogueKey("Characters\\Dialogue\\" + character.Name + ":Resort", random);
						}
						if (dialogue_key != null)
						{
							text = text + " \"" + dialogue_key + "\"";
						}
					}
					currentAssignments[character] = text;
					points.Remove(item);
					currentAnimationAssignments[character] = animation_name;
					return true;
				}
				return false;
			}

			public string GetRandomDialogueKey(string dialogue_key, Random random)
			{
				if (Game1.content.LoadStringReturnNullIfNotFound(dialogue_key) != null)
				{
					bool flag = false;
					int num = 0;
					while (!flag)
					{
						num++;
						if (Game1.content.LoadStringReturnNullIfNotFound(dialogue_key + "_" + (num + 1)) == null)
						{
							flag = true;
						}
					}
					int num2 = random.Next(num) + 1;
					if (num2 == 1)
					{
						return dialogue_key;
					}
					return dialogue_key + "_" + num2;
				}
				return null;
			}

			public string GetScheduleStringForCharacter(NPC character)
			{
				if (currentAssignments.ContainsKey(character))
				{
					return "/" + activityTime + " IslandSouth " + currentAssignments[character];
				}
				return "";
			}
		}

		[XmlIgnore]
		public const int ISLAND_DEPART_EVENT_ID = -157039427;

		[XmlIgnore]
		protected int _boatDirection;

		[XmlIgnore]
		public Texture2D boatTexture;

		[XmlIgnore]
		public Vector2 boatPosition;

		[XmlIgnore]
		protected int _boatOffset;

		[XmlIgnore]
		protected float _nextBubble;

		[XmlIgnore]
		protected float _nextSlosh;

		[XmlIgnore]
		protected float _nextSmoke;

		[XmlIgnore]
		public LightSource boatLight;

		[XmlIgnore]
		public LightSource boatStringLight;

		[XmlElement("shouldToggleResort")]
		public readonly NetBool shouldToggleResort = new NetBool(value: false);

		[XmlElement("resortOpenToday")]
		public readonly NetBool resortOpenToday = new NetBool(value: true);

		[XmlElement("resortRestored")]
		public readonly NetBool resortRestored = new NetBool();

		[XmlElement("westernTurtleMoved")]
		public readonly NetBool westernTurtleMoved = new NetBool();

		[XmlIgnore]
		protected bool _parrotBoyHiding;

		[XmlIgnore]
		protected bool _isFirstVisit;

		[XmlIgnore]
		protected bool _exitsBlocked;

		[XmlIgnore]
		protected bool _sawFlameSprite;

		[XmlIgnore]
		public NetEvent0 moveTurtleEvent = new NetEvent0();

		private Microsoft.Xna.Framework.Rectangle turtle1Spot = new Microsoft.Xna.Framework.Rectangle(1088, 0, 192, 192);

		private Microsoft.Xna.Framework.Rectangle turtle2Spot = new Microsoft.Xna.Framework.Rectangle(0, 640, 256, 256);

		public IslandSouth()
		{
		}

		public IslandSouth(string map, string name)
			: base(map, name)
		{
			largeTerrainFeatures.Add(new Bush(new Vector2(31f, 5f), 4, this));
			parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(17, 22), new Microsoft.Xna.Framework.Rectangle(12, 18, 14, 7), 20, delegate
			{
				Game1.addMailForTomorrow("Island_Resort", noLetter: true, sendToEveryone: true);
				resortRestored.Value = true;
			}, () => resortRestored.Value, "Resort", "Island_UpgradeHouse"));
			parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(5, 9), new Microsoft.Xna.Framework.Rectangle(1, 10, 3, 4), 10, delegate
			{
				Game1.addMailForTomorrow("Island_Turtle", noLetter: true, sendToEveryone: true);
				westernTurtleMoved.Value = true;
				moveTurtleEvent.Fire();
			}, () => westernTurtleMoved.Value, "Turtle", "Island_FirstParrot"));
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(resortRestored, westernTurtleMoved, shouldToggleResort, resortOpenToday, moveTurtleEvent);
			resortRestored.InterpolationWait = false;
			resortRestored.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && mapPath.Value != null)
				{
					ApplyResortRestore();
				}
			};
			moveTurtleEvent.onEvent += ApplyWesternTurtleMove;
		}

		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			if (l is IslandSouth)
			{
				IslandSouth islandSouth = l as IslandSouth;
				resortRestored.Value = islandSouth.resortRestored.Value;
				westernTurtleMoved.Value = islandSouth.westernTurtleMoved.Value;
				shouldToggleResort.Value = islandSouth.shouldToggleResort.Value;
				resortOpenToday.Value = islandSouth.resortOpenToday.Value;
			}
			base.TransferDataFromSavedLocation(l);
		}

		public override void DayUpdate(int dayOfMonth)
		{
			if (shouldToggleResort.Value)
			{
				resortOpenToday.Value = !resortOpenToday.Value;
				shouldToggleResort.Value = false;
				ApplyResortRestore();
			}
			base.DayUpdate(dayOfMonth);
		}

		public void ApplyResortRestore()
		{
			if (map != null)
			{
				ApplyUnsafeMapOverride("Island_Resort", null, new Microsoft.Xna.Framework.Rectangle(9, 15, 26, 16));
			}
			removeTile(new Location(41, 28), "Buildings");
			removeTile(new Location(42, 28), "Buildings");
			removeTile(new Location(42, 29), "Buildings");
			removeTile(new Location(42, 30), "Front");
			removeTileProperty(42, 30, "Back", "Passable");
			if (resortRestored.Value)
			{
				if (resortOpenToday.Value)
				{
					removeTile(new Location(22, 21), "Buildings");
					removeTile(new Location(22, 22), "Buildings");
					removeTile(new Location(24, 21), "Buildings");
					removeTile(new Location(24, 22), "Buildings");
				}
				else
				{
					setMapTile(22, 21, 1405, "Buildings", null);
					setMapTile(22, 22, 1437, "Buildings", null);
					setMapTile(24, 21, 1405, "Buildings", null);
					setMapTile(24, 22, 1437, "Buildings", null);
				}
			}
		}

		public void ApplyWesternTurtleMove()
		{
			TemporaryAnimatedSprite temporarySpriteByID = getTemporarySpriteByID(789);
			if (temporarySpriteByID != null)
			{
				temporarySpriteByID.motion = new Vector2(-2f, 0f);
				temporarySpriteByID.yPeriodic = true;
				temporarySpriteByID.yPeriodicRange = 8f;
				temporarySpriteByID.yPeriodicLoopTime = 300f;
				temporarySpriteByID.shakeIntensity = 1f;
			}
			localSound("shadowDie");
		}

		public override void seasonUpdate(string season, bool onLoad = false)
		{
		}

		public override void updateSeasonalTileSheets(Map map = null)
		{
		}

		private void parrotBoyLands(int extra)
		{
			TemporaryAnimatedSprite temporarySpriteByID = getTemporarySpriteByID(888);
			if (temporarySpriteByID != null)
			{
				temporarySpriteByID.sourceRect.X = 0;
				temporarySpriteByID.sourceRect.Y = 32;
				temporarySpriteByID.sourceRectStartingPos.X = 0f;
				temporarySpriteByID.sourceRectStartingPos.Y = 32f;
				temporarySpriteByID.motion = new Vector2(4f, 0f);
				temporarySpriteByID.acceleration = Vector2.Zero;
				temporarySpriteByID.id = 888f;
				temporarySpriteByID.animationLength = 4;
				temporarySpriteByID.interval = 100f;
				temporarySpriteByID.totalNumberOfLoops = 10;
				temporarySpriteByID.drawAboveAlwaysFront = false;
				temporarySpriteByID.layerDepth = 0.1f;
				temporarySprites.Add(temporarySpriteByID);
			}
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			moveTurtleEvent.Poll();
			if (boatLight != null)
			{
				boatLight.position.Value = new Vector2(3f, 1f) * 64f + GetBoatPosition();
			}
			if (boatStringLight != null)
			{
				boatStringLight.position.Value = new Vector2(3f, 4f) * 64f + GetBoatPosition();
			}
			if (_parrotBoyHiding && Utility.isThereAFarmerWithinDistance(new Vector2(29f, 16f), 4, this) == Game1.player)
			{
				TemporaryAnimatedSprite temporarySpriteByID = getTemporarySpriteByID(777);
				if (temporarySpriteByID != null)
				{
					temporarySpriteByID.sourceRect.X = 0;
					temporarySpriteByID.sourceRectStartingPos.X = 0f;
					temporarySpriteByID.motion = new Vector2(3f, -10f);
					temporarySpriteByID.acceleration = new Vector2(0f, 0.4f);
					temporarySpriteByID.yStopCoordinate = 992;
					temporarySpriteByID.shakeIntensity = 2f;
					temporarySpriteByID.id = 888f;
					temporarySpriteByID.reachedStopCoordinate = parrotBoyLands;
					localSound("parrot_squawk");
				}
			}
			if (!_exitsBlocked && !_sawFlameSprite && Utility.isThereAFarmerWithinDistance(new Vector2(18f, 11f), 5, this) == Game1.player)
			{
				Game1.addMailForTomorrow("Saw_Flame_Sprite_South", noLetter: true);
				TemporaryAnimatedSprite temporarySpriteByID2 = getTemporarySpriteByID(999);
				if (temporarySpriteByID2 != null)
				{
					temporarySpriteByID2.yPeriodic = false;
					temporarySpriteByID2.xPeriodic = false;
					temporarySpriteByID2.sourceRect.Y = 0;
					temporarySpriteByID2.sourceRectStartingPos.Y = 0f;
					temporarySpriteByID2.motion = new Vector2(0f, -4f);
					temporarySpriteByID2.acceleration = new Vector2(0f, -0.04f);
				}
				localSound("magma_sprite_spot");
				temporarySpriteByID2 = getTemporarySpriteByID(998);
				if (temporarySpriteByID2 != null)
				{
					temporarySpriteByID2.yPeriodic = false;
					temporarySpriteByID2.xPeriodic = false;
					temporarySpriteByID2.motion = new Vector2(0f, -4f);
					temporarySpriteByID2.acceleration = new Vector2(0f, -0.04f);
				}
				_sawFlameSprite = true;
			}
			if (currentEvent == null || currentEvent.id != -157039427)
			{
				return;
			}
			if (_boatDirection != 0)
			{
				_boatOffset += _boatDirection;
				foreach (NPC actor in currentEvent.actors)
				{
					actor.shouldShadowBeOffset = true;
					actor.drawOffset.Y = _boatOffset;
				}
				foreach (Farmer farmerActor in currentEvent.farmerActors)
				{
					farmerActor.shouldShadowBeOffset = true;
					farmerActor.drawOffset.Y = _boatOffset;
				}
			}
			if ((float)_boatDirection != 0f)
			{
				if (_nextBubble > 0f)
				{
					_nextBubble -= (float)time.ElapsedGameTime.TotalSeconds;
				}
				else
				{
					Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle(64, 256, 192, 64);
					r.X += (int)GetBoatPosition().X;
					r.Y += (int)GetBoatPosition().Y;
					Vector2 randomPositionInThisRectangle = Utility.getRandomPositionInThisRectangle(r, Game1.random);
					TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 50f, 9, 1, randomPositionInThisRectangle, flicker: false, flipped: false, 0f, 0.025f, Color.White, 1f, 0f, 0f, 0f);
					temporaryAnimatedSprite.acceleration = new Vector2(0f, -0.25f * (float)Math.Sign(_boatDirection));
					temporarySprites.Add(temporaryAnimatedSprite);
					_nextBubble = 0.01f;
				}
				if (_nextSlosh > 0f)
				{
					_nextSlosh -= (float)time.ElapsedGameTime.TotalSeconds;
				}
				else
				{
					Game1.playSound("waterSlosh");
					_nextSlosh = 0.5f;
				}
			}
			if (_nextSmoke > 0f)
			{
				_nextSmoke -= (float)time.ElapsedGameTime.TotalSeconds;
				return;
			}
			Vector2 position = new Vector2(2f, 2.5f) * 64f + GetBoatPosition();
			TemporaryAnimatedSprite temporaryAnimatedSprite2 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 1600, 64, 128), 200f, 9, 1, position, flicker: false, flipped: false, 1f, 0.025f, Color.White, 1f, 0.025f, 0f, 0f);
			temporaryAnimatedSprite2.acceleration = new Vector2(-0.25f, -0.15f);
			temporarySprites.Add(temporaryAnimatedSprite2);
			_nextSmoke = 0.2f;
		}

		public override void cleanupBeforePlayerExit()
		{
			boatLight = null;
			boatStringLight = null;
			base.cleanupBeforePlayerExit();
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			if (_exitsBlocked && position.Intersects(turtle1Spot))
			{
				return true;
			}
			if (!westernTurtleMoved && position.Intersects(turtle2Spot))
			{
				return true;
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
		}

		public override bool isTilePlaceable(Vector2 tile_location, Item item = null)
		{
			Point value = Utility.Vector2ToPoint((tile_location + new Vector2(0.5f, 0.5f)) * 64f);
			if (_exitsBlocked && turtle1Spot.Contains(value))
			{
				return false;
			}
			if (!westernTurtleMoved && turtle2Spot.Contains(value))
			{
				return false;
			}
			return base.isTilePlaceable(tile_location, item);
		}

		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (resortRestored.Value)
			{
				ApplyResortRestore();
			}
		}

		protected override void resetLocalState()
		{
			_isFirstVisit = false;
			if (!Game1.player.hasOrWillReceiveMail("Visited_Island"))
			{
				Game1.addMailForTomorrow("Visited_Island", noLetter: true);
				_isFirstVisit = true;
			}
			if (Game1.player.hasOrWillReceiveMail("Saw_Flame_Sprite_South"))
			{
				_sawFlameSprite = true;
			}
			_exitsBlocked = !Game1.MasterPlayer.hasOrWillReceiveMail("Island_FirstParrot");
			boatLight = new LightSource(4, new Vector2(0f, 0f), 1f, LightSource.LightContext.None, 0L);
			boatStringLight = new LightSource(4, new Vector2(0f, 0f), 1f, LightSource.LightContext.None, 0L);
			Game1.currentLightSources.Add(boatLight);
			Game1.currentLightSources.Add(boatStringLight);
			if (!Game1.player.previousLocationName.Contains("Island") || Game1.player.getTileY() > 35)
			{
				if (Game1.isDarkOut() || Game1.isStartingToGetDarkOut() || Game1.IsRainingHere(this))
				{
					Game1.changeMusicTrack("none");
				}
				else if (_exitsBlocked)
				{
					Game1.changeMusicTrack("tropical_island_day_ambient", track_interruptable: true);
				}
				else
				{
					Game1.changeMusicTrack("IslandMusic", track_interruptable: true);
				}
			}
			base.resetLocalState();
			boatTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\WillysBoat");
			if (Game1.random.NextDouble() < 0.25 || _isFirstVisit)
			{
				addCritter(new CrabCritter(new Vector2(37f, 30f) * 64f));
			}
			if (_isFirstVisit)
			{
				addCritter(new CrabCritter(new Vector2(21f, 35f) * 64f));
				addCritter(new CrabCritter(new Vector2(21f, 36f) * 64f));
				addCritter(new CrabCritter(new Vector2(35f, 31f) * 64f));
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("addedParrotBoy"))
				{
					_parrotBoyHiding = true;
					temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\ParrotBoy", new Microsoft.Xna.Framework.Rectangle(32, 128, 16, 32), new Vector2(29f, 15.5f) * 64f, flipped: false, 0f, Color.White)
					{
						id = 777f,
						scale = 4f,
						totalNumberOfLoops = 99999,
						interval = 9999f,
						animationLength = 1,
						layerDepth = 1f,
						drawAboveAlwaysFront = true
					});
				}
			}
			if (_exitsBlocked)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(208, 94, 48, 53), new Vector2(17f, 0f) * 64f, flipped: false, 0f, Color.White)
				{
					id = 555f,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 9999f,
					animationLength = 1,
					layerDepth = 0.001f
				});
			}
			else if (!_sawFlameSprite)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Monsters\\Magma Sprite", new Microsoft.Xna.Framework.Rectangle(0, 16, 16, 16), new Vector2(18f, 11f) * 64f, flipped: false, 0f, Color.White)
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
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\shadow", new Microsoft.Xna.Framework.Rectangle(0, 0, 12, 7), new Vector2(18.2f, 12.4f) * 64f, flipped: false, 0f, Color.White)
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
			if (!westernTurtleMoved)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(152, 101, 56, 40), new Vector2(0.5f, 10f) * 64f, flipped: false, 0f, Color.White)
				{
					id = 789f,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 9999f,
					animationLength = 1,
					layerDepth = 0.001f
				});
			}
			if (Game1.currentSeason == "winter" && !Game1.IsRainingHere(this) && Game1.isDarkOut())
			{
				addMoonlightJellies(50, new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame - 24917), new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
			}
			ResetBoat();
		}

		public override void digUpArtifactSpot(int xLocation, int yLocation, Farmer who)
		{
			Random random = new Random(xLocation * 2000 + yLocation * 767 + (int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + (int)Game1.stats.DirtHoed);
			if (Game1.player.hasOrWillReceiveMail("islandNorthCaveOpened") && random.NextDouble() < 0.25)
			{
				Game1.createItemDebris(new Object(824, 1), new Vector2(xLocation, yLocation) * 64f, -1, this);
			}
			else
			{
				base.digUpArtifactSpot(xLocation, yLocation, who);
			}
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (tileLocation.X == 14 && tileLocation.Y == 22 && getCharacterFromName("Gus") != null && getCharacterFromName("Gus").getTileLocation().Equals(new Vector2(14f, 21f)))
			{
				Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
				Utility.AddStock(dictionary, new Object(Vector2.Zero, 873, 2147483647), 300);
				Utility.AddStock(dictionary, new Object(Vector2.Zero, 346, 2147483647), 250);
				Utility.AddStock(dictionary, new Object(Vector2.Zero, 303, 2147483647), 500);
				Utility.AddStock(dictionary, new Object(Vector2.Zero, 459, 2147483647), 400);
				Utility.AddStock(dictionary, new Object(Vector2.Zero, 612, 2147483647), 200);
				Object @object = new Object(Vector2.Zero, 348, 2147483647);
				Object object2 = new Object(834, 1);
				@object.Price = object2.Price * 3;
				@object.Name = object2.Name + " Wine";
				@object.preserve.Value = Object.PreserveType.Wine;
				@object.preservedParentSheetIndex.Value = object2.ParentSheetIndex;
				@object.Quality = 2;
				Utility.AddStock(dictionary, @object, 2500);
				if (!Game1.player.cookingRecipes.ContainsKey("Tropical Curry"))
				{
					Utility.AddStock(dictionary, new Object(907, 1, isRecipe: true), 1000);
				}
				Game1.activeClickableMenu = new ShopMenu(dictionary, 0, "Gus", null, null, "ResortBar");
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public static bool CanVisitIslandToday(NPC npc)
		{
			if (!npc.isVillager() || !npc.CanSocialize)
			{
				return false;
			}
			if (npc.daysUntilNotInvisible > 0 || npc.IsInvisible)
			{
				return false;
			}
			if ((npc.Name == "Pam" || npc.Name == "Emily") && Game1.dayOfMonth == 15 && Game1.currentSeason == "fall")
			{
				return false;
			}
			string text = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
			if (npc.currentLocation != null && npc.currentLocation.NameOrUniqueName == "Farm")
			{
				return false;
			}
			switch (text)
			{
			case "Tue":
			case "Fri":
			case "Wed":
				if (npc.Name == "Vincent" || npc.Name == "Jas" || npc.Name == "Penny")
				{
					return false;
				}
				break;
			}
			if ((text == "Tue" || text == "Thu") && (npc.Name == "Harvey" || npc.Name == "Maru"))
			{
				return false;
			}
			if (Utility.IsHospitalVisitDay(npc.Name))
			{
				return false;
			}
			if (npc.Name == "Clint" && text != "Fri")
			{
				return false;
			}
			if (npc.Name == "Robin" && text != "Tue")
			{
				return false;
			}
			if (npc.Name == "Marnie" && text != "Tue" && text != "Mon")
			{
				return false;
			}
			if (npc.Name == "Sandy" || npc.Name == "Dwarf" || npc.Name == "Krobus" || npc.Name == "Wizard" || npc.Name == "Linus")
			{
				return false;
			}
			if (npc.Name == "Willy" || npc.Name == "Evelyn" || npc.Name == "George")
			{
				return false;
			}
			return true;
		}

		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (!(questionAndAnswer == "LeaveIsland_Yes"))
			{
				if (questionAndAnswer == "ToggleResort_Yes")
				{
					shouldToggleResort.Value = !shouldToggleResort.Value;
					bool flag = resortOpenToday.Value;
					if (shouldToggleResort.Value)
					{
						flag = !flag;
					}
					if (flag)
					{
						Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\Locations:IslandSouth_ResortWillOpenSign"));
					}
					else
					{
						Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\Locations:IslandSouth_ResortWillCloseSign"));
					}
					return true;
				}
				return base.answerDialogueAction(questionAndAnswer, questionParams);
			}
			Depart();
			return true;
		}

		public override bool performAction(string action, Farmer who, Location tileLocation)
		{
			if (action == "ResortSign")
			{
				string text = "";
				text = (resortOpenToday.Value ? (shouldToggleResort.Value ? "Strings\\Locations:IslandSouth_ResortOpenWillCloseSign" : "Strings\\Locations:IslandSouth_ResortOpenSign") : (shouldToggleResort.Value ? "Strings\\Locations:IslandSouth_ResortClosedWillOpenSign" : "Strings\\Locations:IslandSouth_ResortClosedSign"));
				createQuestionDialogue(Game1.content.LoadString(text), createYesNoResponses(), "ToggleResort");
				return true;
			}
			return base.performAction(action, who, tileLocation);
		}

		public override void performTouchAction(string fullActionString, Vector2 playerStandingPosition)
		{
			string text = fullActionString.Split(' ')[0];
			if (text == "LeaveIsland")
			{
				Response[] answerChoices = new Response[2]
				{
					new Response("Yes", Game1.content.LoadString("Strings\\Locations:Desert_Return_Yes")),
					new Response("Not", Game1.content.LoadString("Strings\\Locations:Desert_Return_No"))
				};
				createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Desert_Return_Question"), answerChoices, "LeaveIsland");
			}
			else
			{
				base.performTouchAction(fullActionString, playerStandingPosition);
			}
		}

		public void Depart()
		{
			Game1.globalFadeToBlack(delegate
			{
				currentEvent = new Event(Game1.content.LoadString("Data\\Events\\IslandSouth:IslandDepart"), -157039427, Game1.player);
				Game1.eventUp = true;
			});
		}

		public static Point GetDressingRoomPoint(NPC character)
		{
			if ((int)character.gender == 1)
			{
				return new Point(22, 19);
			}
			return new Point(24, 19);
		}

		public override bool HasLocationOverrideDialogue(NPC character)
		{
			if (Game1.player.friendshipData.ContainsKey(character.Name) && Game1.player.friendshipData[character.Name].IsDivorced())
			{
				return false;
			}
			return character.islandScheduleName.Value != null;
		}

		public override string GetLocationOverrideDialogue(NPC character)
		{
			string text = "";
			if (Game1.timeOfDay < 1200 || (!character.shouldWearIslandAttire.Value && Game1.timeOfDay < 1730 && HasIslandAttire(character)))
			{
				text = "Characters\\Dialogue\\" + character.Name + ":Resort_Entering";
				if (Game1.content.LoadStringReturnNullIfNotFound(text) != null)
				{
					return text;
				}
			}
			if (Game1.timeOfDay >= 1800)
			{
				text = "Characters\\Dialogue\\" + character.Name + ":Resort_Leaving";
				if (Game1.content.LoadStringReturnNullIfNotFound(text) != null)
				{
					return text;
				}
			}
			return "Characters\\Dialogue\\" + character.Name + ":Resort";
		}

		public static bool HasIslandAttire(NPC character)
		{
			try
			{
				Game1.temporaryContent.Load<Texture2D>("Characters\\" + NPC.getTextureNameForCharacter(character.name.Value) + "_Beach");
				if (character != null && character.Name == "Lewis")
				{
					foreach (Farmer allFarmer in Game1.getAllFarmers())
					{
						if (allFarmer != null && allFarmer.activeDialogueEvents != null && allFarmer.activeDialogueEvents.ContainsKey("lucky_pants_lewis"))
						{
							return true;
						}
					}
					return false;
				}
				return true;
			}
			catch (Exception)
			{
			}
			return false;
		}

		public static void SetupIslandSchedules()
		{
			Game1.netWorldState.Value.IslandVisitors.Clear();
			if (Utility.isFestivalDay(Game1.Date.DayOfMonth, Game1.Date.Season) || (Game1.Date.Season == "winter" && Game1.Date.DayOfMonth >= 15 && Game1.Date.DayOfMonth <= 17) || !(Game1.getLocationFromName("IslandSouth") is IslandSouth islandSouth) || !islandSouth.resortRestored.Value || Game1.IsRainingHere(islandSouth) || !islandSouth.resortOpenToday.Value)
			{
				return;
			}
			Random random = new Random((int)(Game1.uniqueIDForThisGame * 121 / 100uL) + (int)(Game1.stats.DaysPlayed * 25 / 10u));
			List<NPC> list = new List<NPC>();
			foreach (NPC allCharacter in Utility.getAllCharacters())
			{
				if (CanVisitIslandToday(allCharacter))
				{
					list.Add(allCharacter);
				}
			}
			List<NPC> list2 = new List<NPC>();
			if (random.NextDouble() < 0.4)
			{
				for (int i = 0; i < 5; i++)
				{
					NPC random2 = Utility.GetRandom(list, random);
					if (random2 != null && (int)random2.age != 2)
					{
						list.Remove(random2);
						list2.Add(random2);
						random2.scheduleDelaySeconds = Math.Min((float)i * 0.6f, 7f);
					}
				}
			}
			else
			{
				List<List<string>> list3 = new List<List<string>>();
				list3.Add(new List<string> { "Sebastian", "Sam", "Abigail" });
				list3.Add(new List<string> { "Jodi", "Kent", "Vincent", "Sam" });
				list3.Add(new List<string> { "Jodi", "Vincent", "Sam" });
				list3.Add(new List<string> { "Pierre", "Caroline", "Abigail" });
				list3.Add(new List<string> { "Robin", "Demetrius", "Maru", "Sebastian" });
				list3.Add(new List<string> { "Lewis", "Marnie" });
				list3.Add(new List<string> { "Marnie", "Shane", "Jas" });
				list3.Add(new List<string> { "Penny", "Jas", "Vincent" });
				list3.Add(new List<string> { "Pam", "Penny" });
				list3.Add(new List<string> { "Caroline", "Marnie", "Robin", "Jodi" });
				list3.Add(new List<string> { "Haley", "Penny", "Leah", "Emily", "Maru", "Abigail" });
				list3.Add(new List<string> { "Alex", "Sam", "Sebastian", "Elliott", "Shane", "Harvey" });
				List<string> list4 = list3[random.Next(list3.Count)];
				bool flag = false;
				foreach (string item in list4)
				{
					if (!list.Contains(Game1.getCharacterFromName(item)))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					int num = 0;
					foreach (string item2 in list4)
					{
						NPC characterFromName = Game1.getCharacterFromName(item2);
						list.Remove(characterFromName);
						list2.Add(characterFromName);
						characterFromName.scheduleDelaySeconds = Math.Min((float)num * 0.6f, 7f);
						num++;
					}
				}
				for (int j = 0; j < 5 - list2.Count; j++)
				{
					NPC random3 = Utility.GetRandom(list, random);
					if (random3 != null && (int)random3.age != 2)
					{
						list.Remove(random3);
						list2.Add(random3);
						random3.scheduleDelaySeconds = Math.Min((float)j * 0.6f, 7f);
					}
				}
			}
			List<IslandActivityAssigments> list5 = new List<IslandActivityAssigments>();
			Dictionary<Character, string> last_activity_assignments = new Dictionary<Character, string>();
			list5.Add(new IslandActivityAssigments(1200, list2, random, last_activity_assignments));
			list5.Add(new IslandActivityAssigments(1400, list2, random, last_activity_assignments));
			list5.Add(new IslandActivityAssigments(1600, list2, random, last_activity_assignments));
			last_activity_assignments = null;
			foreach (NPC item3 in list2)
			{
				StringBuilder stringBuilder = new StringBuilder("");
				bool flag2 = HasIslandAttire(item3);
				bool flag3 = false;
				if (flag2)
				{
					Point dressingRoomPoint = GetDressingRoomPoint(item3);
					stringBuilder.Append("/a1150 IslandSouth " + dressingRoomPoint.X + " " + dressingRoomPoint.Y + " change_beach");
					flag3 = true;
				}
				foreach (IslandActivityAssigments item4 in list5)
				{
					string text = item4.GetScheduleStringForCharacter(item3);
					if (text != "")
					{
						if (!flag3)
						{
							text = "/a" + text.Substring(1);
							flag3 = true;
						}
						stringBuilder.Append(text);
					}
				}
				if (flag2)
				{
					Point dressingRoomPoint2 = GetDressingRoomPoint(item3);
					stringBuilder.Append("/a1730 IslandSouth " + dressingRoomPoint2.X + " " + dressingRoomPoint2.Y + " change_normal");
				}
				if (item3.Name == "Gus")
				{
					stringBuilder.Append("/1800 Saloon 10 18 2/2430 bed");
				}
				else
				{
					stringBuilder.Append("/1800 bed");
				}
				stringBuilder.Remove(0, 1);
				item3.islandScheduleName.Value = "island";
				item3.Schedule = item3.parseMasterSchedule(stringBuilder.ToString());
				Game1.netWorldState.Value.IslandVisitors[item3.Name] = true;
			}
		}

		public virtual void ResetBoat()
		{
			boatPosition = new Vector2(14f, 37f) * 64f;
			_boatOffset = 0;
			_boatDirection = 0;
			_nextBubble = 0f;
			_nextSmoke = 0f;
			_nextSlosh = 0f;
		}

		public Vector2 GetBoatPosition()
		{
			return boatPosition + new Vector2(0f, _boatOffset);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			Vector2 globalPosition = GetBoatPosition();
			b.Draw(boatTexture, Game1.GlobalToLocal(globalPosition), new Microsoft.Xna.Framework.Rectangle(192, 0, 96, 208), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (boatPosition.Y + 320f) / 10000f);
			b.Draw(boatTexture, Game1.GlobalToLocal(globalPosition), new Microsoft.Xna.Framework.Rectangle(288, 0, 96, 208), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (boatPosition.Y + 616f) / 10000f);
			if (currentEvent == null || currentEvent.id != -157039427)
			{
				b.Draw(boatTexture, Game1.GlobalToLocal(new Vector2(1184f, 2752f)), new Microsoft.Xna.Framework.Rectangle(192, 208, 32, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.272f);
			}
		}

		public override bool RunLocationSpecificEventCommand(Event current_event, string command_string, bool first_run, params string[] args)
		{
			if (command_string == "boat_reset")
			{
				ResetBoat();
				return true;
			}
			if (command_string == "boat_depart")
			{
				_boatDirection = 1;
				if (_boatOffset >= 100)
				{
					return true;
				}
				return false;
			}
			return false;
		}
	}
}
