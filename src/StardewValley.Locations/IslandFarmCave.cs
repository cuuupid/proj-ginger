using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class IslandFarmCave : IslandLocation
	{
		[XmlIgnore]
		public NPC gourmand;

		[XmlElement("gourmandRequestsFulfilled")]
		public NetInt gourmandRequestsFulfilled = new NetInt();

		[XmlIgnore]
		public NetEvent0 requestGourmandCheckEvent = new NetEvent0();

		[XmlIgnore]
		public NetEvent1Field<string, NetString> gourmandResponseEvent = new NetEvent1Field<string, NetString>();

		[XmlIgnore]
		public bool triggeredGourmand;

		[XmlIgnore]
		public static int TOTAL_GOURMAND_REQUESTS = 3;

		[XmlIgnore]
		private NetMutex gourmandMutex = new NetMutex();

		private Texture2D smokeTexture;

		private float smokeTimer;

		public IslandFarmCave()
		{
		}

		public IslandFarmCave(string map, string name)
			: base(map, name)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(gourmandRequestsFulfilled, requestGourmandCheckEvent, gourmandResponseEvent, gourmandMutex.NetFields);
			requestGourmandCheckEvent.onEvent += OnRequestGourmandCheck;
			gourmandResponseEvent.onEvent += OnGourmandResponse;
		}

		public virtual void OnRequestGourmandCheck()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			string arg = "";
			IslandWest islandWest = Game1.getLocationFromName("IslandWest") as IslandWest;
			foreach (Vector2 key in islandWest.terrainFeatures.Keys)
			{
				TerrainFeature terrainFeature = islandWest.terrainFeatures[key];
				if (!(terrainFeature is HoeDirt))
				{
					continue;
				}
				HoeDirt hoeDirt = terrainFeature as HoeDirt;
				if (hoeDirt.crop == null)
				{
					continue;
				}
				bool flag = (int)hoeDirt.crop.currentPhase >= hoeDirt.crop.phaseDays.Count - 1 && (!hoeDirt.crop.fullyGrown || (int)hoeDirt.crop.dayOfCurrentPhase <= 0);
				if (hoeDirt.crop.indexOfHarvest.Value == IndexForRequest(gourmandRequestsFulfilled.Value))
				{
					if (flag)
					{
						Point destination = new Point((int)key.X, (int)key.Y);
						Point point = FindNearbyUnoccupiedTileThatFitsCharacter(islandWest, destination.X, destination.Y);
						Point point2 = FindNearbyUnoccupiedTileThatFitsCharacter(islandWest, destination.X, destination.Y, 2, point);
						int relativeDirection = GetRelativeDirection(point, destination);
						gourmandResponseEvent.Fire(key.X + " " + key.Y + " " + point.X + " " + point.Y + " " + relativeDirection + " " + point2.X + " " + point2.Y + " 2");
						return;
					}
					arg = "inProgress";
				}
			}
			gourmandResponseEvent.Fire(arg);
		}

		public int GetRelativeDirection(Point source, Point destination)
		{
			Point point = new Point(destination.X - source.X, destination.Y - source.Y);
			if (Math.Abs(point.Y) > Math.Abs(point.X))
			{
				if (point.Y < 0)
				{
					return 0;
				}
				return 2;
			}
			if (point.X < 0)
			{
				return 3;
			}
			return 1;
		}

		public override Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			if (Game1.random.NextDouble() < 0.1)
			{
				Game1.createItemDebris(new Hat(78), new Vector2(bobberTile.X * 64f + 32f, bobberTile.Y * 64f), 0, this, who.getStandingY());
				return null;
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
		}

		public Point FindNearbyUnoccupiedTileThatFitsCharacter(GameLocation location, int target_x, int target_y, int width = 1, Point? invalid_tile = null)
		{
			HashSet<Point> hashSet = new HashSet<Point>();
			List<Point> list = new List<Point>();
			list.Add(new Point(target_x, target_y));
			hashSet.Add(new Point(target_x, target_y));
			Point[] array = new Point[4]
			{
				new Point(-1, 0),
				new Point(1, 0),
				new Point(0, -1),
				new Point(0, 1)
			};
			for (int i = 0; i < 500; i++)
			{
				if (list.Count == 0)
				{
					break;
				}
				Point point = list[0];
				list.RemoveAt(0);
				Point[] array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					Point point2 = array2[j];
					Point item = new Point(point.X + point2.X, point.Y + point2.Y);
					if (!hashSet.Contains(item))
					{
						list.Add(item);
					}
				}
				if (hashSet.Contains(point) || (invalid_tile.HasValue && point.X == invalid_tile.Value.X && point.Y == invalid_tile.Value.Y))
				{
					continue;
				}
				hashSet.Add(point);
				bool flag = false;
				int num = 1;
				for (int k = 0; k < width; k++)
				{
					for (int l = 0; l < num; l++)
					{
						Point point3 = new Point(point.X + k, point.Y + l);
						new Microsoft.Xna.Framework.Rectangle(point3.X * 64, point3.Y * 64, 64, 64).Inflate(-4, -4);
						if (point3.X == target_x && point3.Y == target_y + 1)
						{
							flag = true;
							break;
						}
						if (invalid_tile.HasValue && invalid_tile.Value == point3)
						{
							flag = true;
							break;
						}
						if (!location.isTileLocationOpenIgnoreFrontLayers(new Location(point3.X, point3.Y)))
						{
							flag = true;
							break;
						}
						if (location.isObjectAtTile(point3.X, point3.Y))
						{
							flag = true;
							break;
						}
						if (location.isTerrainFeatureAt(point3.X, point3.Y))
						{
							flag = true;
							break;
						}
						if (flag)
						{
							continue;
						}
						Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(point3.X * 64, point3.Y * 64, 64, 64);
						foreach (ResourceClump resourceClump in location.resourceClumps)
						{
							if (resourceClump.getBoundingBox(resourceClump.tile).Intersects(value))
							{
								flag = true;
								break;
							}
						}
					}
				}
				if (!flag)
				{
					return point;
				}
			}
			return new Point(target_x, target_y);
		}

		public virtual void OnGourmandResponse(string response)
		{
			if (Game1.currentLocation != this)
			{
				return;
			}
			if (response == "")
			{
				if (triggeredGourmand)
				{
					Game1.player.freezePause = 0;
					ShowGourmandUnhappy();
				}
			}
			else if (response == "inProgress")
			{
				Game1.player.freezePause = 0;
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Gourmand_InProgress"));
			}
			else
			{
				string[] array = response.Split(' ');
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("none/-1000 -1000/");
				stringBuilder.Append("farmer " + array[2] + " " + array[3] + " " + array[4] + "/");
				stringBuilder.Append("changeLocation IslandWest/");
				stringBuilder.Append("viewport " + array[0] + " " + array[1] + "/");
				stringBuilder.Append("playMusic none/addTemporaryActor Gourmand 32 32 " + array[5] + " " + array[6] + " " + array[7] + " true character/positionOffset Gourmand 0 1/positionOffset farmer 0 1/animate Gourmand false true 500 2 3/");
				stringBuilder.Append("viewport " + array[0] + " " + array[1] + " true/");
				stringBuilder.Append("pause 3000/playSound croak/");
				string text = Game1.content.LoadString("Strings\\Locations:Gourmand_Request_" + gourmandRequestsFulfilled.Value + "_Success");
				string[] array2 = text.Split('|');
				foreach (string text2 in array2)
				{
					stringBuilder.Append("message \"" + text2 + "\"/pause 250/");
				}
				stringBuilder.Append("pause 1000/end");
				Event evt = new Event(stringBuilder.ToString());
				if (triggeredGourmand)
				{
					Event @event = evt;
					@event.onEventFinished = (Action)Delegate.Combine(@event.onEventFinished, (Action)delegate
					{
						if (Game1.locationRequest != null)
						{
							Game1.locationRequest.OnWarp += delegate
							{
								CompleteGourmandRequest();
							};
						}
						else
						{
							CompleteGourmandRequest();
						}
					});
				}
				Game1.globalFadeToBlack(delegate
				{
					Game1.currentLocation.startEvent(evt);
				});
				Game1.player.freezePause = 0;
			}
			triggeredGourmand = false;
		}

		public virtual void CompleteGourmandRequest()
		{
			if (!gourmandMutex.IsLockHeld())
			{
				return;
			}
			Game1.player.freezePause = 1250;
			DelayedAction.functionAfterDelay(delegate
			{
				Game1.playSound("croak");
				gourmand.shake(1000);
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(GiveReward));
				if (gourmandRequestsFulfilled.Value < TOTAL_GOURMAND_REQUESTS - 1)
				{
					Game1.multipleDialogues(Game1.content.LoadString("Strings\\Locations:Gourmand_Reward").Split('|'));
				}
				else
				{
					Game1.multipleDialogues(Game1.content.LoadString("Strings\\Locations:Gourmand_LastReward").Split('|'));
				}
			}, 1000);
		}

		public virtual void GiveReward()
		{
			Game1.createItemDebris(new Object(73, 1), new Vector2(4.5f, 4f) * 64f, 3, this);
			Game1.createItemDebris(new Object(73, 1), new Vector2(4.5f, 4f) * 64f, 1, this);
			Game1.createItemDebris(new Object(73, 1), new Vector2(4.5f, 4f) * 64f, 1, this);
			Game1.createItemDebris(new Object(73, 1), new Vector2(4.5f, 4f) * 64f, 1, this);
			Game1.createItemDebris(new Object(73, 1), new Vector2(4.5f, 4f) * 64f, 1, this);
			gourmandRequestsFulfilled.Value++;
			Game1.player.team.MarkCollectedNut("IslandGourmand" + gourmandRequestsFulfilled.Value);
			gourmandMutex.ReleaseLock();
		}

		public void ShowGourmandUnhappy()
		{
			Game1.playSound("croak");
			gourmand.shake(1000);
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Gourmand_RequestFailed"));
			if (gourmandMutex.IsLockHeld())
			{
				gourmandMutex.ReleaseLock();
			}
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			gourmand = new NPC(new AnimatedSprite("Characters\\Gourmand", 0, 32, 32), new Vector2(4f, 4f) * 64f, "IslandFarmCave", 2, "Gourmand", datable: false, null, Game1.content.Load<Texture2D>("Portraits\\SafariGuy"));
			smokeTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
			waterColor.Value = new Color(10, 250, 120);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (gourmand != null && !Game1.eventUp)
			{
				gourmand.draw(b);
			}
			if ((int)gourmandRequestsFulfilled < TOTAL_GOURMAND_REQUESTS)
			{
				float num = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(gourmand.getStandingX(), (float)(gourmand.getStandingY() - 128 - 8) + num)), new Microsoft.Xna.Framework.Rectangle(114, 53, 6, 10), Color.White, 0f, new Vector2(1f, 4f), 4f, SpriteEffects.None, 1f);
			}
		}

		public override void DayUpdate(int dayOfMonth)
		{
			gourmandMutex.ReleaseLock();
			base.DayUpdate(dayOfMonth);
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			if (gourmand != null)
			{
				gourmand.update(time, this);
				if (time.TotalGameTime.TotalMilliseconds % 1000.0 < 500.0)
				{
					gourmand.Sprite.CurrentFrame = 1;
				}
				else
				{
					gourmand.Sprite.CurrentFrame = 0;
				}
			}
			requestGourmandCheckEvent.Poll();
			gourmandResponseEvent.Poll();
			smokeTimer -= time.ElapsedGameTime.Milliseconds;
			if (smokeTimer <= 0f && smokeTexture != null)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = smokeTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 180, 9, 11),
					sourceRectStartingPos = new Vector2(0f, 180f),
					layerDepth = 1f,
					interval = 250f,
					position = new Vector2(2f, 4f) * 64f + new Vector2(5f, 5f) * 4f,
					scale = 4f,
					scaleChange = 0.005f,
					alpha = 0.75f,
					alphaFade = 0.005f,
					motion = new Vector2(0f, -0.5f),
					acceleration = new Vector2((float)(Game1.random.NextDouble() - 0.5) / 100f, 0f),
					animationLength = 3,
					holdLastFrame = true
				});
				temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = smokeTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 180, 9, 11),
					sourceRectStartingPos = new Vector2(0f, 180f),
					layerDepth = 1f,
					interval = 250f,
					position = new Vector2(7f, 4f) * 64f + new Vector2(5f, 5f) * 4f,
					scale = 4f,
					scaleChange = 0.005f,
					alpha = 0.75f,
					alphaFade = 0.005f,
					motion = new Vector2(0f, -0.5f),
					acceleration = new Vector2((float)(Game1.random.NextDouble() - 0.5) / 100f, 0f),
					animationLength = 3,
					holdLastFrame = true
				});
				smokeTimer = 1250f;
			}
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
			gourmandMutex.Update(Game1.getOnlineFarmers());
		}

		public override bool isTileOccupiedForPlacement(Vector2 tileLocation, Object toPlace = null)
		{
			return base.isTileOccupiedForPlacement(tileLocation, toPlace);
		}

		public override void seasonUpdate(string season, bool onLoad = false)
		{
		}

		public override void updateSeasonalTileSheets(Map map = null)
		{
		}

		public virtual void TalkToGourmand()
		{
			List<string> list = new List<string>();
			if (gourmandRequestsFulfilled.Value >= TOTAL_GOURMAND_REQUESTS)
			{
				list.AddRange(Game1.content.LoadString("Strings\\Locations:Gourmand_Finished").Split('|'));
			}
			else
			{
				if (!Game1.player.hasOrWillReceiveMail("talkedToGourmand"))
				{
					Game1.addMailForTomorrow("talkedToGourmand", noLetter: true);
					list.Add(Game1.content.LoadString("Strings\\Locations:Gourmand_Intro"));
				}
				Game1.playSound("croak");
				gourmand.shake(1000);
				list.Add(Game1.content.LoadString("Strings\\Locations:Gourmand_RequestIntro"));
				list.Add(Game1.content.LoadString("Strings\\Locations:Gourmand_Request_" + gourmandRequestsFulfilled.Value));
				Response[] responses = createYesNoResponses();
				Game1.afterDialogues = delegate
				{
					Game1.afterDialogues = null;
					createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Gourmand_RequestQuestion"), responses, "Gourmand");
				};
			}
			Game1.multipleDialogues(list.ToArray());
		}

		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (!(questionAndAnswer == "Gourmand_Yes"))
			{
				if (questionAndAnswer == "Gourmand_No")
				{
					ShowGourmandUnhappy();
					return true;
				}
				return base.answerDialogueAction(questionAndAnswer, questionParams);
			}
			triggeredGourmand = true;
			Game1.player.freezePause = 3000;
			requestGourmandCheckEvent.Fire();
			return true;
		}

		public int IndexForRequest(int request_number)
		{
			return request_number switch
			{
				0 => 254, 
				1 => 262, 
				2 => 248, 
				_ => -1, 
			};
		}

		public override bool performAction(string action, Farmer who, Location tileLocation)
		{
			if (action == "Gourmand")
			{
				gourmandMutex.RequestLock(delegate
				{
					TalkToGourmand();
				});
				return true;
			}
			return base.performAction(action, who, tileLocation);
		}

		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			base.TransferDataFromSavedLocation(l);
			if (l is IslandFarmCave)
			{
				IslandFarmCave islandFarmCave = l as IslandFarmCave;
				gourmandRequestsFulfilled.Value = islandFarmCave.gourmandRequestsFulfilled.Value;
			}
		}
	}
}
