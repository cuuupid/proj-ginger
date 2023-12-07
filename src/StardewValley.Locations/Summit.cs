using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Monsters;
using StardewValley.Projectiles;

namespace StardewValley.Locations
{
	public class Summit : GameLocation
	{
		private ICue wind;

		private float windGust;

		private float globalWind = -0.25f;

		[XmlIgnore]
		public bool isShowingEndSlideshow;

		public Summit()
		{
		}

		public Summit(string map, string name)
			: base(map, name)
		{
		}

		public override void checkForMusic(GameTime time)
		{
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (Game1.random.NextDouble() < 0.005 || globalWind >= 1f || globalWind <= 0.35f)
			{
				if (globalWind < 0.35f)
				{
					windGust = (float)Game1.random.Next(3, 6) / 2000f;
				}
				else if (globalWind > 0.75f)
				{
					windGust = (float)(-Game1.random.Next(2, 6)) / 2000f;
				}
				else
				{
					windGust = (float)(((!(Game1.random.NextDouble() < 0.5)) ? 1 : (-1)) * Game1.random.Next(4, 6)) / 2000f;
				}
			}
			if (wind != null)
			{
				globalWind += windGust;
				globalWind = Utility.Clamp(globalWind, -0.5f, 1f);
				wind.SetVariable("Volume", Math.Abs(globalWind) * 60f);
				wind.SetVariable("Frequency", globalWind * 100f);
				wind.SetVariable("Pitch", 1200f + Math.Abs(globalWind) * 1200f);
			}
			base.UpdateWhenCurrentLocation(time);
			if (temporarySprites.Count == 0 && Game1.random.NextDouble() < ((Game1.timeOfDay < 1800) ? 0.0006 : ((Game1.currentSeason.Equals("summer") && Game1.dayOfMonth == 20) ? 1.0 : 0.001)))
			{
				Rectangle sourceRect = Rectangle.Empty;
				Vector2 vector = new Vector2(Game1.viewport.Width, Game1.random.Next(10, Game1.viewport.Height / 2));
				float x = -4f;
				int numberOfLoops = 200;
				float animationInterval = 100f;
				if (Game1.timeOfDay < 1800)
				{
					if (Game1.currentSeason.Equals("spring") || Game1.currentSeason.Equals("fall"))
					{
						sourceRect = new Rectangle(640, 736, 16, 16);
						int num = Game1.random.Next(1, 4);
						x = -1f;
						for (int i = 0; i < num; i++)
						{
							TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, Game1.random.Next(80, 121), 4, 200, vector + new Vector2((i + 1) * Game1.random.Next(15, 18), (i + 1) * -20), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
							{
								layerDepth = 0f
							};
							temporaryAnimatedSprite.motion = new Vector2(-1f, 0f);
							temporarySprites.Add(temporaryAnimatedSprite);
							temporaryAnimatedSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, Game1.random.Next(80, 121), 4, 200, vector + new Vector2((i + 1) * Game1.random.Next(15, 18), (i + 1) * 20), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
							{
								layerDepth = 0f
							};
							temporaryAnimatedSprite.motion = new Vector2(-1f, 0f);
							temporarySprites.Add(temporaryAnimatedSprite);
						}
					}
					else if (Game1.currentSeason.Equals("summer"))
					{
						sourceRect = new Rectangle(640, 752 + ((Game1.random.NextDouble() < 0.5) ? 16 : 0), 16, 16);
						x = -0.5f;
						animationInterval = 150f;
					}
					if (Game1.random.NextDouble() < 1.25)
					{
						TemporaryAnimatedSprite temporaryAnimatedSprite2 = null;
						temporaryAnimatedSprite2 = Game1.currentSeason switch
						{
							"spring" => new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(0, 302, 26, 18), Game1.random.Next(80, 121), 4, 200, vector, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
							{
								layerDepth = 0f,
								pingPong = true
							}, 
							"summer" => new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(1, 165, 24, 21), Game1.random.Next(60, 80), 6, 200, vector, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
							{
								layerDepth = 0f
							}, 
							"fall" => new TemporaryAnimatedSprite("TileSheets\\critters", new Rectangle(0, 64, 32, 32), Game1.random.Next(60, 80), 5, 200, vector, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
							{
								layerDepth = 0f,
								pingPong = true
							}, 
							"winter" => new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(104, 302, 26, 18), Game1.random.Next(80, 121), 4, 200, vector, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
							{
								layerDepth = 0f,
								pingPong = true
							}, 
							_ => new TemporaryAnimatedSprite(), 
						};
						temporaryAnimatedSprite2.motion = new Vector2(-3f, 0f);
						temporarySprites.Add(temporaryAnimatedSprite2);
					}
					else if (Game1.random.NextDouble() < 0.15 && Game1.stats.getStat("childrenTurnedToDoves") > 1)
					{
						for (int j = 0; j < Game1.stats.getStat("childrenTurnedToDoves"); j++)
						{
							sourceRect = Rectangle.Empty;
							TemporaryAnimatedSprite temporaryAnimatedSprite3 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(388, 1894, 24, 21), Game1.random.Next(80, 121), 6, 200, vector + new Vector2((j + 1) * (Game1.random.Next(25, 27) * 4), Game1.random.Next(-32, 33) * 4), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
							{
								layerDepth = 0f
							};
							temporaryAnimatedSprite3.motion = new Vector2(-3f, 0f);
							temporarySprites.Add(temporaryAnimatedSprite3);
						}
					}
					if (Game1.MasterPlayer.eventsSeen.Contains(571102) && Game1.random.NextDouble() < 0.1)
					{
						sourceRect = Rectangle.Empty;
						TemporaryAnimatedSprite temporaryAnimatedSprite4 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(222, 1890, 20, 9), 30f, 2, 99900, vector, flicker: false, flipped: false, 0.01f, 0f, Color.White, 2f, 0f, 0f, 0f, local: true)
						{
							yPeriodic = true,
							yPeriodicLoopTime = 4000f,
							yPeriodicRange = 8f,
							layerDepth = 0f
						};
						temporaryAnimatedSprite4.motion = new Vector2(-3f, 0f);
						temporarySprites.Add(temporaryAnimatedSprite4);
					}
					if (Game1.MasterPlayer.eventsSeen.Contains(10) && Game1.random.NextDouble() < 0.05)
					{
						sourceRect = Rectangle.Empty;
						TemporaryAnimatedSprite temporaryAnimatedSprite5 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(206, 1827, 15, 25), 30f, 4, 99900, vector, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							rotation = -(float)Math.PI / 3f,
							layerDepth = 0f
						};
						temporaryAnimatedSprite5.motion = new Vector2(-4f, -0.5f);
						temporarySprites.Add(temporaryAnimatedSprite5);
					}
				}
				else if (Game1.timeOfDay >= 1900)
				{
					sourceRect = new Rectangle(640, 816, 16, 16);
					x = -2f;
					numberOfLoops = 0;
					vector.X -= Game1.random.Next(64, Game1.viewport.Width);
					if (Game1.currentSeason.Equals("summer") && Game1.dayOfMonth == 20)
					{
						int num2 = Game1.random.Next(3);
						for (int k = 0; k < num2; k++)
						{
							TemporaryAnimatedSprite temporaryAnimatedSprite6 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, Game1.random.Next(80, 121), Game1.currentSeason.Equals("winter") ? 2 : 4, numberOfLoops, vector, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
							{
								layerDepth = 0f
							};
							temporaryAnimatedSprite6.motion = new Vector2(x, 0f);
							temporarySprites.Add(temporaryAnimatedSprite6);
							vector.X -= Game1.random.Next(64, Game1.viewport.Width);
							vector.Y = Game1.random.Next(0, 200);
						}
					}
					else if (Game1.currentSeason.Equals("winter") && Game1.timeOfDay >= 1700 && Game1.random.NextDouble() < 0.1)
					{
						sourceRect = new Rectangle(640, 800, 32, 16);
						numberOfLoops = 1000;
						vector.X = Game1.viewport.Width;
					}
					else if (Game1.currentSeason.Equals("winter"))
					{
						sourceRect = Rectangle.Empty;
					}
				}
				if (Game1.timeOfDay >= 2200 && !Game1.currentSeason.Equals("winter") && Game1.currentSeason.Equals("summer") && Game1.dayOfMonth == 20 && Game1.random.NextDouble() < 0.05)
				{
					sourceRect = new Rectangle(640, 784, 16, 16);
					numberOfLoops = 200;
					vector.X = Game1.viewport.Width;
					x = -3f;
				}
				if (!sourceRect.Equals(Rectangle.Empty) && Game1.viewport.X > -10000)
				{
					TemporaryAnimatedSprite temporaryAnimatedSprite7 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, animationInterval, Game1.currentSeason.Equals("winter") ? 2 : 4, numberOfLoops, vector, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						layerDepth = 0f
					};
					temporaryAnimatedSprite7.motion = new Vector2(x, 0f);
					temporarySprites.Add(temporaryAnimatedSprite7);
				}
			}
			if (Game1.viewport.X > -10000)
			{
				foreach (TemporaryAnimatedSprite temporarySprite in temporarySprites)
				{
					temporarySprite.position.Y -= ((float)Game1.viewport.Y - Game1.previousViewportPosition.Y) / 8f;
					temporarySprite.drawAboveAlwaysFront = true;
				}
			}
			if (Game1.eventUp)
			{
				foreach (TemporaryAnimatedSprite temporarySprite2 in temporarySprites)
				{
					if (temporarySprite2.attachedCharacter != null)
					{
						temporarySprite2.attachedCharacter.animateInFacingDirection(time);
					}
				}
				return;
			}
			isShowingEndSlideshow = false;
		}

		public override void cleanupBeforePlayerExit()
		{
			isShowingEndSlideshow = false;
			base.cleanupBeforePlayerExit();
			Game1.background = null;
			Game1.displayHUD = true;
			if (wind != null)
			{
				wind.Stop(AudioStopOptions.Immediate);
			}
		}

		protected override void resetLocalState()
		{
			isShowingEndSlideshow = false;
			isOutdoors.Value = false;
			base.resetLocalState();
			Game1.background = new Background();
			temporarySprites.Clear();
			Game1.displayHUD = false;
			Game1.changeMusicTrack("winter_day_ambient", track_interruptable: true, Game1.MusicContext.SubLocation);
			wind = Game1.soundBank.GetCue("wind");
			wind.Play();
			globalWind = 0f;
			windGust = 0.001f;
			if (Game1.player.mailReceived.Contains("Summit_event") || !Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
			{
				return;
			}
			string summitEvent = getSummitEvent();
			if (summitEvent != "")
			{
				if (!Game1.player.songsHeard.Contains("end_credits"))
				{
					Game1.player.songsHeard.Add("end_credits");
				}
				Game1.player.mailReceived.Add("Summit_event");
				startEvent(new Event(getSummitEvent()));
			}
		}

		public string GetSummitDialogue(string file, string key)
		{
			string path = "Data\\" + file + ":" + key;
			if (Game1.player.getSpouse() != null && Game1.player.getSpouse().Name == "Penny")
			{
				return Game1.content.LoadString(path, "요");
			}
			return Game1.content.LoadString(path, "");
		}

		private string getSummitEvent()
		{
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				stringBuilder.Append("winter_day_ambient/-1000 -1000/farmer 9 23 0 ");
				if (Game1.player.isMarried() && Game1.player.getSpouse() != null && Game1.player.getSpouse().Name != "Krobus")
				{
					stringBuilder.Append(Game1.player.getSpouse().Name + " 11 13 0/skippable/viewport 10 17 clamp true/pause 2000/viewport move 0 -1 4000/move farmer 0 -10 0/move farmer 1 0 0/pause 2000/speak " + Game1.player.getSpouse().Name + " \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Intro_Spouse") + "\"/viewport move 0 -1 4000/pause 5000/speak " + Game1.player.getSpouse().Name + " \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Intro2_Spouse" + (sayGrufferSummitIntro(Game1.player.getSpouse()) ? "_Gruff" : "")) + "\"/pause 400/emote farmer 56/pause 2000/speak " + Game1.player.getSpouse().Name + " \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue1_Spouse") + "\"/pause 2000/faceDirection " + Game1.player.getSpouse().Name + " 3/faceDirection farmer 1/pause 1000/speak " + Game1.player.getSpouse().Name + " \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue2_Spouse") + "\"/pause 2000/faceDirection " + Game1.player.getSpouse().Name + " 0/faceDirection farmer 0/pause 2000/speak " + Game1.player.getSpouse().Name + " \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue3_" + Game1.player.getSpouse().Name) + "\"/emote farmer 20/pause 500/faceDirection farmer 1/faceDirection " + Game1.player.getSpouse().Name + " 3/pause 1500/animate farmer false true 100 101/showKissFrame " + Game1.player.getSpouse().Name + "/playSound dwop/positionOffset farmer 8 0/positionOffset " + Game1.player.getSpouse().Name + " -4 0/specificTemporarySprite heart 11 12/pause 10");
				}
				else if (Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
				{
					stringBuilder.Append("Morris 11 13 0/skippable/viewport 10 17 clamp true/pause 2000/viewport move 0 -1 4000/move farmer 0 -10 0/pause 2000/speak Morris \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Intro_Morris") + "\"/viewport move 0 -1 4000/pause 5000/speak Morris \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue1_Morris") + "\"/pause 2000/faceDirection Morris 3/speak Morris \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue2_Morris") + "\"/pause 2000/faceDirection Morris 0/speak Morris \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Outro_Morris") + "\"/emote farmer 20/pause 10");
				}
				else
				{
					stringBuilder.Append("Lewis 11 13 0/skippable/viewport 10 17 clamp true/pause 2000/viewport move 0 -1 4000/move farmer 0 -10 0/pause 2000/speak Lewis \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Intro_Lewis") + "\"/viewport move 0 -1 4000/pause 5000/speak Lewis \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue1_Lewis") + "\"/pause 2000/faceDirection Lewis 3/speak Lewis \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue2_Lewis") + "\"/pause 2000/faceDirection Lewis 0/speak Lewis \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Outro_Lewis") + "\"/pause 10");
				}
				int num = 35000;
				if (Game1.player.mailReceived.Contains("Broken_Capsule"))
				{
					num += 8000;
				}
				if (Game1.player.totalMoneyEarned >= 100000000)
				{
					num += 8000;
				}
				if (Game1.year <= 2)
				{
					num += 8000;
				}
				stringBuilder.Append("/playMusic moonlightJellies/pause 2000/specificTemporarySprite krobusraven/viewport move 0 -1 12000/pause 10/pause " + num + "/pause 2000/playMusic none/viewport move 0 -1 5000/fade/playMusic end_credits/viewport -8000 -8000 true/removeTemporarySprites/specificTemporarySprite getEndSlideshow/pause 1000/playMusic none/pause 500");
				stringBuilder.Append("/playMusic grandpas_theme/pause 2000/fade/viewport -3000 -2000/specificTemporarySprite doneWithSlideShow/removeTemporarySprites/pause 3000/addTemporaryActor MrQi 16 32 -998 -1000 2 true/addTemporaryActor Grandpa 1 1 -100 -100 2 true/specificTemporarySprite grandpaSpirit/viewport -1000 -1000 true/pause 6000/spriteText 3 \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_closingmessage") + " \"/spriteText 3 \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_closingmessage2") + " \"/spriteText 3 \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_closingmessage3") + " \"/spriteText 3 \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_closingmessage4") + " \"/spriteText 7 \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_closingmessage5") + " \"/pause 400/playSound dwop/showFrame MrQi 1/pause 100/showFrame MrQi 2/pause 100/showFrame MrQi 3/pause 400/specificTemporarySprite grandpaThumbsUp/pause 10000/end");
			}
			catch (Exception)
			{
				return "";
			}
			return stringBuilder.ToString();
		}

		public string getEndSlideshow()
		{
			StringBuilder stringBuilder = new StringBuilder();
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			int num = 0;
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				try
				{
					if (!(item.Key == "Marlon") && !(item.Key == "Krobus") && !(item.Key == "Dwarf") && !(item.Key == "Sandy") && !(item.Key == "Wizard"))
					{
						string text = item.Key;
						if (item.Key == "Leo")
						{
							text = "ParrotBoy";
						}
						base.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\" + text, new Rectangle(0, 96, 16, 32), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.4f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = num
						});
						num += 500;
					}
				}
				catch (Exception)
				{
				}
			}
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\night_market_tilesheet_objects", new Rectangle(586, 119, 122, 28), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 2000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\night_market_tilesheet_objects", new Rectangle(586, 119, 122, 28), 900f, 1, 999999, new Vector2(Game1.viewport.Width + 488, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 2000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\night_market_tilesheet_objects", new Rectangle(586, 119, 122, 28), 900f, 1, 999999, new Vector2(Game1.viewport.Width + 976, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 2000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\night_market_tilesheet_objects", new Rectangle(586, 119, 122, 28), 900f, 1, 999999, new Vector2(Game1.viewport.Width + 1464, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 2000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(324, 1936, 12, 20), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.4f + 192f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 14000,
				startSound = "dogWhining"
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(43, 80, 51, 56), 90f, 1, 999999, new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-1f, -4f),
				delayBeforeAnimationStart = 27000,
				startSound = "trashbear",
				drawAboveAlwaysFront = true
			});
			stringBuilder.Append("pause 10/spriteText 5 \"" + Utility.loadStringShort("UI", "EndCredit_Neighbors") + " \"/pause 30000/");
			num += 4000;
			int num2 = num;
			foreach (KeyValuePair<string, string> item2 in dictionary)
			{
				if (item2.Key == "Krobus" || item2.Key == "Dwarf" || item2.Key == "Sandy" || item2.Key == "Wizard")
				{
					int num3 = 32;
					if (item2.Key == "Krobus" || item2.Key == "Dwarf")
					{
						num3 = 24;
					}
					base.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\" + item2.Key, new Rectangle(0, num3 * 3, 16, num3), 120f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.4f + (float)((32 - num3) * 4)), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = num
					});
					num += 500;
				}
			}
			num += 5000;
			stringBuilder.Append("spriteText 4 \"" + Utility.loadStringShort("UI", "EndCredit_Animals") + " \"/pause " + (num - num2 + 22000));
			num2 = num;
			Dictionary<string, string> dictionary2 = Game1.content.Load<Dictionary<string, string>>("Data\\FarmAnimals");
			foreach (KeyValuePair<string, string> item3 in dictionary2)
			{
				if (item3.Key == "Hog" || item3.Key == "Brown Cow")
				{
					continue;
				}
				int num4 = Convert.ToInt32(item3.Value.Split('/')[16]);
				int num5 = Convert.ToInt32(item3.Value.Split('/')[17]);
				int num6 = 0;
				base.TemporarySprites.Add(new TemporaryAnimatedSprite("Animals\\" + item3.Key, new Rectangle(0, num5, num4, num5), 120f, 4, 999999, new Vector2(Game1.viewport.Width, (int)((float)Game1.viewport.Height * 0.5f - (float)(num5 * 4))), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = num
				});
				num6 += num4 * 4;
				int num7 = ((num4 > 16) ? 4 : 0);
				try
				{
					string text2 = "Baby" + item3.Key;
					if (item3.Key == "Duck")
					{
						text2 = "BabyWhite Chicken";
					}
					else if (item3.Key == "Dinosaur")
					{
						text2 = "Dinosaur";
					}
					Game1.content.Load<Texture2D>("Animals\\" + text2);
					base.TemporarySprites.Add(new TemporaryAnimatedSprite("Animals\\" + text2, new Rectangle(0, num5, num4, num5), 90f, 4, 999999, new Vector2(Game1.viewport.Width + (num4 + 2 + num7) * 4, (int)((float)Game1.viewport.Height * 0.5f - (float)(num5 * 4))), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = num
					});
					base.TemporarySprites.Add(new TemporaryAnimatedSprite("Animals\\" + text2, new Rectangle(0, num5, num4, num5), 90f, 4, 999999, new Vector2(Game1.viewport.Width + (num4 + 2 + num7) * 2 * 4, (int)((float)Game1.viewport.Height * 0.5f - (float)(num5 * 4))), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = num
					});
					num6 += (num4 + 2 + num7) * 4 * 2;
				}
				catch (Exception)
				{
				}
				base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(0, num5, num4, num5), 120f, 1, 999999, new Vector2((float)(Game1.viewport.Width + num6 / 2) - Game1.dialogueFont.MeasureString(item3.Value.Split('/')[25]).X / 2f, (int)((float)Game1.viewport.Height * 0.5f + 12f)), flicker: false, flipped: true, 0.9f, 0f, Color.White, 1f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = num,
					text = item3.Value.Split('/')[25]
				});
				num += 2000 + num7 * 300;
			}
			if (Game1.player.catPerson)
			{
				base.TemporarySprites.Add(new TemporaryAnimatedSprite("Animals\\cat" + ((Game1.player.whichPetBreed != 0) ? (Game1.player.whichPetBreed.ToString() ?? "") : ""), new Rectangle(0, 96, 32, 32), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 320f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-4f, 0f),
					delayBeforeAnimationStart = 38000,
					startSound = "cat"
				});
			}
			else
			{
				base.TemporarySprites.Add(new TemporaryAnimatedSprite("Animals\\dog" + ((Game1.player.whichPetBreed != 0) ? (Game1.player.whichPetBreed.ToString() ?? "") : ""), new Rectangle(0, 256, 32, 32), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 320f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-5f, 0f),
					delayBeforeAnimationStart = 38000,
					startSound = "dog_bark",
					pingPong = true
				});
			}
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(64, 192, 32, 32), 90f, 6, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 128f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 45000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(128, 160, 32, 32), 90f, 6, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 128f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 47000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(128, 224, 32, 32), 90f, 6, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 128f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 48000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(32, 160, 32, 32), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 320f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 49000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(32, 160, 32, 32), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 288f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 49500,
				pingPong = true
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(34, 98, 32, 32), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 352f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 50000,
				pingPong = true
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(0, 32, 32, 32), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 352f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 50500,
				pingPong = true
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(128, 96, 16, 16), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 352f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 55000,
				pingPong = true,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 3000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(192, 96, 16, 16), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 358.4f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 55300,
				pingPong = true,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 3000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(256, 96, 16, 16), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 345.6f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 55600,
				pingPong = true,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 3000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(0, 128, 16, 16), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 352f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 57000,
				pingPong = true,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 3000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(48, 144, 16, 16), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 358.4f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 57300,
				pingPong = true,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 3000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(96, 144, 16, 16), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 345.6f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 57600,
				pingPong = true,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 3000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(192, 288, 16, 16), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 345.6f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 58000,
				pingPong = true,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 3000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(128, 288, 16, 16), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 358.4f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 58300,
				pingPong = true,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 3000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(0, 224, 16, 16), 90f, 5, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 64f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 54000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(0, 240, 16, 16), 90f, 5, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 64f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 55000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(67, 190, 24, 51), 90f, 3, 999999, new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, -4f),
				delayBeforeAnimationStart = 68000,
				rotation = -(float)Math.PI / 16f,
				pingPong = true,
				drawAboveAlwaysFront = true
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(0, 0, 57, 70), 150f, 2, 999999, new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, -4f),
				delayBeforeAnimationStart = 69000,
				rotation = -(float)Math.PI / 16f,
				drawAboveAlwaysFront = true
			});
			stringBuilder.Append("/spriteText 1 \"" + Utility.loadStringShort("UI", "EndCredit_Fish") + " \"/pause " + (num - num2 + 18000));
			num += 6000;
			num2 = num;
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(257, 98, 182, 18), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 72f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 70000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(257, 98, 182, 18), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 72f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 86000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(257, 98, 182, 18), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 72f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 91000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(140, 78, 28, 38), 250f, 2, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 152f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 102000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(257, 98, 182, 18), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 72f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 75000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\AquariumFish", new Rectangle(0, 287, 47, 14), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 56f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 82000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\AquariumFish", new Rectangle(0, 287, 47, 14), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 56f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 80000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\AquariumFish", new Rectangle(0, 287, 47, 14), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 56f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 84000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(132, 20, 8, 8), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 48f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 81500,
				yPeriodic = true,
				yPeriodicRange = 21f,
				yPeriodicLoopTime = 5000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(140, 20, 8, 8), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 48f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 83500,
				yPeriodic = true,
				yPeriodicRange = 21f,
				yPeriodicLoopTime = 5000f
			});
			Dictionary<int, string> dictionary3 = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
			Dictionary<int, string> dictionary4 = Game1.content.Load<Dictionary<int, string>>("Data\\AquariumFish");
			int num8 = 0;
			foreach (KeyValuePair<int, string> item4 in dictionary3)
			{
				try
				{
					int num9 = Convert.ToInt32(dictionary4[item4.Key].Split('/')[0]);
					Rectangle sourceRect = new Rectangle(24 * num9 % 480, 24 * num9 / 480 * 48, 24, 24);
					float x = Game1.dialogueFont.MeasureString(Game1.IsEnglish() ? item4.Value.Split('/')[0] : item4.Value.Split('/')[13]).X;
					base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\AquariumFish", sourceRect, 9999f, 1, 999999, new Vector2(Game1.viewport.Width + 192, (int)((float)Game1.viewport.Height * 0.53f - (float)(num8 * 64) * 2f)), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = num,
						yPeriodic = true,
						yPeriodicLoopTime = Game1.random.Next(1500, 2100),
						yPeriodicRange = 4f
					});
					base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\AquariumFish", sourceRect, 9999f, 1, 999999, new Vector2((float)(Game1.viewport.Width + 192 + 48) - x / 2f, (int)((float)Game1.viewport.Height * 0.53f - (float)(num8 * 64) * 2f + 64f + 16f)), flicker: false, flipped: true, 0.9f, 0f, Color.White, 1f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = num,
						text = (Game1.IsEnglish() ? item4.Value.Split('/')[0] : item4.Value.Split('/')[13])
					});
					num8++;
					if (num8 == 4)
					{
						num += 2000;
						num8 = 0;
					}
				}
				catch (Exception)
				{
				}
			}
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\projectiles", new Rectangle(64, 0, 16, 16), 909f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 352f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-6f, 0f),
				delayBeforeAnimationStart = 123000,
				rotationChange = -0.1f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\projectiles", new Rectangle(64, 0, 16, 16), 909f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 339.2f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-6f, 0f),
				delayBeforeAnimationStart = 123300,
				rotationChange = -0.1f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(0, 1452, 640, 69), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.2f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 108000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(0, 1452, 640, 69), 900f, 1, 999999, new Vector2(Game1.viewport.Width + 2564, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.2f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 108000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(0, 1452, 640, 69), 900f, 1, 999999, new Vector2(Game1.viewport.Width + 5128, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.2f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 108000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(0, 1452, 300, 69), 900f, 1, 999999, new Vector2(Game1.viewport.Width + 7692, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.2f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 108000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(0, 0, 31, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 110000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(65, 0, 31, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 115000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(96, 90, 31, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 118000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(0, 176, 104, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 121000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(32, 320, 32, 23), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 92f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 124000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(31, 58, 67, 23), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 92f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 127000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(0, 98, 32, 23), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 92f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 132000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(49, 131, 47, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 137000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 0, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 113000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 20, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 116000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 40, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 119000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 60, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 126000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 120, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 129000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 100, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 134000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 120, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 139000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\upperCavePlants", new Rectangle(0, 0, 48, 21), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 84f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 142000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\upperCavePlants", new Rectangle(96, 0, 48, 21), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 84f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 146000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(2, 123, 19, 24), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 352f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 145000,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 2500f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(2, 123, 19, 24), 100f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 358.4f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 142500,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 2000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(0, 0, 31, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 149000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(65, 0, 31, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 151000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(96, 90, 31, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 154000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(0, 176, 104, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 156000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 0, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 155000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 20, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 152500
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 40, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 158000
			});
			if (Game1.player.favoriteThing.Value != null && Game1.player.favoriteThing.Value.ToLower().Equals("concernedape"))
			{
				base.TemporarySprites.Add(new TemporaryAnimatedSprite("Minigames\\Clouds", new Rectangle(210, 842, 138, 130), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 240f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = 160500,
					startSound = "discoverMineral"
				});
			}
			stringBuilder.Append("/spriteText 2 \"" + Utility.loadStringShort("UI", "EndCredit_Monsters") + " \"/pause " + (num - num2 + 19000));
			num += 6000;
			num2 = num;
			Dictionary<string, string> dictionary5 = Game1.content.Load<Dictionary<string, string>>("Data\\Monsters");
			foreach (KeyValuePair<string, string> item5 in dictionary5)
			{
				if (item5.Key == "Fireball" || item5.Key == "Skeleton Warrior")
				{
					continue;
				}
				int num10 = 16;
				int num11 = 16;
				int num12 = 0;
				int num13 = 4;
				bool pingPong = false;
				int num14 = 0;
				Character character = null;
				if (item5.Key.Contains("Bat") || item5.Key.Contains("Ghost"))
				{
					num10 = 24;
				}
				string key = item5.Key;
				if (key != null)
				{
					Texture2D texture2D2;
					Texture2D texture2D3;
					switch (key.Length)
					{
					case 4:
						switch (key[0])
						{
						case 'G':
							break;
						case 'F':
							goto IL_3c1a;
						case 'C':
							goto IL_3c30;
						default:
							goto end_IL_3985;
						}
						if (!(key == "Grub"))
						{
							break;
						}
						goto IL_3fbc;
					case 9:
					{
						char c = key[0];
						if ((uint)c <= 70u)
						{
							switch (c)
							{
							case 'F':
								if (key == "Frost Bat")
								{
									continue;
								}
								break;
							case 'B':
								if (key == "Big Slime")
								{
									num10 = 32;
									num11 = 32;
									num14 = 64;
									character = new BigSlime(Vector2.Zero, 0);
								}
								break;
							}
							break;
						}
						if (c != 'L')
						{
							if (c != 'R' || !(key == "Rock Crab"))
							{
								break;
							}
						}
						else if (!(key == "Lava Crab"))
						{
							if (key == "Lava Lurk")
							{
								num12 = 4;
								pingPong = true;
							}
							break;
						}
						goto IL_3fbc;
					}
					case 12:
					{
						char c = key[0];
						if ((uint)c <= 73u)
						{
							if (c != 'C')
							{
								if (c != 'I' || !(key == "Iridium Crab"))
								{
									break;
								}
								goto IL_3fbc;
							}
							if (!(key == "Carbon Ghost"))
							{
								break;
							}
						}
						else
						{
							if (c == 'M')
							{
								if (!(key == "Magma Sprite"))
								{
									break;
								}
								goto IL_3fec;
							}
							if (c != 'P')
							{
								if (c == 'S' && key == "Shadow Brute")
								{
									continue;
								}
								break;
							}
							if (!(key == "Putrid Ghost"))
							{
								break;
							}
						}
						goto IL_4045;
					}
					case 11:
						switch (key[0])
						{
						case 'S':
							break;
						case 'M':
							goto IL_3d33;
						case 'D':
							goto IL_3d49;
						case 'F':
							goto IL_3d5f;
						case 'T':
							goto IL_3d75;
						case 'I':
							goto IL_3d8b;
						case 'G':
							goto IL_3da1;
						default:
							goto end_IL_3985;
						}
						if (!(key == "Stone Golem"))
						{
							break;
						}
						goto IL_3fbc;
					case 3:
						switch (key[0])
						{
						case 'F':
							break;
						case 'C':
							goto IL_3dcd;
						case 'B':
							goto IL_3de3;
						default:
							goto end_IL_3985;
						}
						if (!(key == "Fly"))
						{
							break;
						}
						goto IL_3fbc;
					case 5:
					{
						char c = key[0];
						if (c != 'D')
						{
							if (c != 'G')
							{
								if (c != 'M' || !(key == "Mummy"))
								{
									break;
								}
								goto IL_404d;
							}
							if (!(key == "Ghost"))
							{
								break;
							}
							goto IL_4045;
						}
						if (!(key == "Duggy"))
						{
							break;
						}
						goto IL_3fbc;
					}
					case 15:
					{
						char c = key[0];
						if (c != 'D')
						{
							if (c != 'F' || !(key == "False Magma Cap"))
							{
								break;
							}
							goto IL_3fc8;
						}
						if (!(key == "Dwarvish Sentry"))
						{
							break;
						}
						goto IL_4045;
					}
					case 13:
					{
						char c = key[8];
						if ((uint)c <= 97u)
						{
							if (c != ' ')
							{
								if (c != 'S')
								{
									if (c != 'a' || !(key == "Magma Sparker"))
									{
										break;
									}
									goto IL_3fec;
								}
								if (key == "Iridium Slime")
								{
									continue;
								}
								break;
							}
							if (!(key == "Skeleton Mage"))
							{
								break;
							}
							goto IL_404d;
						}
						switch (c)
						{
						case 'h':
							if (key == "Shadow Shaman")
							{
								continue;
							}
							break;
						case 'n':
							if (key == "Shadow Sniper")
							{
								continue;
							}
							break;
						case 'r':
							if (key == "Royal Serpent")
							{
								continue;
							}
							break;
						}
						break;
					}
					case 6:
					{
						char c = key[1];
						if (c != 'l')
						{
							if (c != 'p')
							{
								break;
							}
							if (!(key == "Spider"))
							{
								if (!(key == "Spiker"))
								{
									break;
								}
								goto IL_4045;
							}
							num11 = 32;
							num10 = 32;
							num13 = 2;
							break;
						}
						if (key == "Sludge")
						{
							continue;
						}
						break;
					}
					case 8:
					{
						char c = key[0];
						if (c != 'L')
						{
							if (c != 'S' || !(key == "Skeleton"))
							{
								break;
							}
							goto IL_404d;
						}
						if (key == "Lava Bat")
						{
							continue;
						}
						break;
					}
					case 10:
						switch (key[0])
						{
						case 'P':
							if (key == "Pepper Rex")
							{
								num11 = 32;
								num10 = 32;
							}
							break;
						case 'B':
							if (key == "Blue Squid")
							{
								num11 = 24;
								num10 = 24;
								num13 = 5;
							}
							break;
						case 'S':
						{
							if (!(key == "Shadow Guy"))
							{
								break;
							}
							num10 = 32;
							num12 = 4;
							Texture2D texture2D = Game1.content.Load<Texture2D>("Characters\\Monsters\\Shadow Brute");
							base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(num11 * num12 % texture2D.Width, num11 * num12 / texture2D.Width * num10, num11, num10), 100f, num13, 999999, new Vector2(Game1.viewport.Width + 192, (float)Game1.viewport.Height * 0.5f - (float)(num10 * 4) - 16f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
							{
								motion = new Vector2(-3f, 0f),
								delayBeforeAnimationStart = num,
								yPeriodic = (num13 == 1),
								yPeriodicRange = 16f,
								yPeriodicLoopTime = 3000f,
								attachedCharacter = character,
								texture = texture2D
							});
							texture2D = Game1.content.Load<Texture2D>("Characters\\Monsters\\Shadow Shaman");
							num10 = 24;
							base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(num11 * num12 % texture2D.Width, num11 * num12 / texture2D.Width * num10, num11, num10), 100f, num13, 999999, new Vector2((float)Game1.viewport.Width + 96f, (float)Game1.viewport.Height * 0.5f - (float)(num10 * 4) - 16f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
							{
								motion = new Vector2(-3f, 0f),
								delayBeforeAnimationStart = num,
								yPeriodic = (num13 == 1),
								yPeriodicRange = 16f,
								yPeriodicLoopTime = 3000f,
								attachedCharacter = character,
								texture = texture2D
							});
							texture2D = Game1.content.Load<Texture2D>("Characters\\Monsters\\Shadow Sniper");
							num10 = 32;
							num11 = 32;
							base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(num11 * num12 % texture2D.Width, num11 * num12 / texture2D.Width * num10, num11, num10), 100f, num13, 999999, new Vector2((float)Game1.viewport.Width + 288f, (float)Game1.viewport.Height * 0.5f - (float)(num10 * 4) - 16f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
							{
								motion = new Vector2(-3f, 0f),
								delayBeforeAnimationStart = num,
								yPeriodic = (num13 == 1),
								yPeriodicRange = 16f,
								yPeriodicLoopTime = 3000f,
								attachedCharacter = character,
								texture = texture2D
							});
							base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(num11 * num12 % texture2D.Width, num11 * num12 / texture2D.Width * num10, num11, num10), 100f, num13, 999999, new Vector2((float)(Game1.viewport.Width + 128 + num11 * 4 / 2) - Game1.dialogueFont.MeasureString(item5.Value.Split('/')[14]).X / 2f, (float)Game1.viewport.Height * 0.5f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 1f, 0f, 0f, 0f, local: true)
							{
								motion = new Vector2(-3f, 0f),
								delayBeforeAnimationStart = num,
								text = Utility.loadStringShort("UI", "EndCredit_ShadowPeople")
							});
							num += 1500;
							continue;
						}
						}
						break;
					case 16:
						if (!(key == "Wilderness Golem"))
						{
							break;
						}
						goto IL_3fbc;
					case 7:
						{
							if (key == "Serpent")
							{
								num11 = 32;
								num10 = 32;
								num13 = 5;
							}
							break;
						}
						IL_3c30:
						if (key == "Crow")
						{
							continue;
						}
						break;
						IL_404d:
						num10 = 32;
						num12 = 4;
						break;
						IL_3d33:
						if (!(key == "Magma Duggy"))
						{
							break;
						}
						goto IL_3fbc;
						IL_3de3:
						if (!(key == "Bat"))
						{
							break;
						}
						texture2D2 = Game1.content.Load<Texture2D>("Characters\\Monsters\\Frost Bat");
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(num11 * num12 % texture2D2.Width, num11 * num12 / texture2D2.Width * num10, num11, num10), 100f, num13, 999999, new Vector2(Game1.viewport.Width + 192, (float)Game1.viewport.Height * 0.5f - (float)(num10 * 4) - 16f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = num,
							yPeriodic = (num13 == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = character,
							texture = texture2D2
						});
						texture2D2 = Game1.content.Load<Texture2D>("Characters\\Monsters\\Lava Bat");
						num10 = 24;
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(num11 * num12 % texture2D2.Width, num11 * num12 / texture2D2.Width * num10, num11, num10), 100f, num13, 999999, new Vector2((float)Game1.viewport.Width + 96f, (float)Game1.viewport.Height * 0.5f - (float)(num10 * 4) - 16f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = num,
							yPeriodic = (num13 == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = character,
							texture = texture2D2
						});
						texture2D2 = Game1.content.Load<Texture2D>("Characters\\Monsters\\Iridium Bat");
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(num11 * num12 % texture2D2.Width, num11 * num12 / texture2D2.Width * num10, num11, num10), 100f, num13, 999999, new Vector2((float)Game1.viewport.Width + 288f, (float)Game1.viewport.Height * 0.5f - (float)(num10 * 4) - 16f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = num,
							yPeriodic = (num13 == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = character,
							texture = texture2D2
						});
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(num11 * num12 % texture2D2.Width, num11 * num12 / texture2D2.Width * num10, num11, num10), 100f, num13, 999999, new Vector2((float)(Game1.viewport.Width + 128 + num11 * 4 / 2) - Game1.dialogueFont.MeasureString(item5.Value.Split('/')[14]).X / 2f, (float)Game1.viewport.Height * 0.5f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 1f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = num,
							text = Utility.loadStringShort("UI", "EndCredit_Bats")
						});
						num += 1500;
						continue;
						IL_3fec:
						num13 = 7;
						num12 = 7;
						break;
						IL_3dcd:
						if (key == "Cat")
						{
							continue;
						}
						break;
						IL_4045:
						num13 = 1;
						break;
						IL_3da1:
						if (!(key == "Green Slime"))
						{
							break;
						}
						texture2D3 = null;
						if (character == null)
						{
							texture2D3 = Game1.content.Load<Texture2D>("Characters\\Monsters\\Green Slime");
						}
						num10 = 32;
						num12 = 4;
						character = new GreenSlime(Vector2.Zero, 0);
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(num11 * num12 % texture2D3.Width, num11 * num12 / texture2D3.Width * num10, num11, num10), 100f, num13, 999999, new Vector2(Game1.viewport.Width + 192 - 64, (float)Game1.viewport.Height * 0.5f - (float)(num10 * 4) + 32f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = num,
							yPeriodic = (num13 == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = character,
							texture = null
						});
						character = new GreenSlime(Vector2.Zero, 41);
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(num11 * num12 % texture2D3.Width, num11 * num12 / texture2D3.Width * num10, num11, num10), 100f, num13, 999999, new Vector2((float)Game1.viewport.Width + 96f - 64f, (float)Game1.viewport.Height * 0.5f - (float)(num10 * 4) + 32f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = num,
							yPeriodic = (num13 == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = character,
							texture = null
						});
						character = new GreenSlime(Vector2.Zero, 81);
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(num11 * num12 % texture2D3.Width, num11 * num12 / texture2D3.Width * num10, num11, num10), 100f, num13, 999999, new Vector2((float)Game1.viewport.Width + 288f - 64f, (float)Game1.viewport.Height * 0.5f - (float)(num10 * 4) + 32f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = num,
							yPeriodic = (num13 == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = character,
							texture = null
						});
						character = new GreenSlime(Vector2.Zero, 121);
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(num11 * num12 % texture2D3.Width, num11 * num12 / texture2D3.Width * num10, num11, num10), 100f, num13, 999999, new Vector2((float)Game1.viewport.Width + 240f - 64f, (float)Game1.viewport.Height * 0.5f - (float)(num10 * 4 * 2) + 32f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = num,
							yPeriodic = (num13 == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = character,
							texture = null
						});
						character = new GreenSlime(Vector2.Zero, 0);
						(character as GreenSlime).makeTigerSlime();
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(num11 * num12 % texture2D3.Width, num11 * num12 / texture2D3.Width * num10, num11, num10), 100f, num13, 999999, new Vector2((float)Game1.viewport.Width + 144f - 64f, (float)Game1.viewport.Height * 0.5f - (float)(num10 * 4 * 2) + 32f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = num,
							yPeriodic = (num13 == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = character,
							texture = null
						});
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(num11 * num12 % texture2D3.Width, num11 * num12 / texture2D3.Width * num10, num11, num10), 100f, num13, 999999, new Vector2((float)(Game1.viewport.Width + 192 + num11 * 4 / 2) - Game1.dialogueFont.MeasureString(item5.Value.Split('/')[14]).X / 2f, (float)Game1.viewport.Height * 0.5f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 1f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = num,
							text = Utility.loadStringShort("UI", "EndCredit_Slimes")
						});
						num += 1500;
						continue;
						IL_3c1a:
						if (key == "Frog")
						{
							continue;
						}
						break;
						IL_3d8b:
						if (key == "Iridium Bat")
						{
							continue;
						}
						break;
						IL_3fbc:
						num10 = 24;
						num12 = 4;
						break;
						IL_3d75:
						if (key == "Tiger Slime")
						{
							continue;
						}
						break;
						IL_3d49:
						if (!(key == "Dust Spirit"))
						{
							break;
						}
						goto IL_3fc8;
						IL_3d5f:
						if (key == "Frost Jelly")
						{
							continue;
						}
						break;
						IL_3fc8:
						num10 = 24;
						num12 = 0;
						break;
						end_IL_3985:
						break;
					}
				}
				try
				{
					Texture2D texture2D4 = null;
					texture2D4 = ((character != null) ? character.Sprite.Texture : Game1.content.Load<Texture2D>("Characters\\Monsters\\" + item5.Key));
					base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(num11 * num12 % texture2D4.Width, num11 * num12 / texture2D4.Width * num10 + 1, num11, num10 - 1), 100f, num13, 999999, new Vector2(Game1.viewport.Width + 192, (float)Game1.viewport.Height * 0.5f - (float)(num10 * 4) - 16f + (float)num14), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = num,
						yPeriodic = (num13 == 1),
						yPeriodicRange = 16f,
						yPeriodicLoopTime = 3000f,
						attachedCharacter = character,
						texture = ((character == null) ? texture2D4 : null),
						pingPong = pingPong
					});
					base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(num11 * num12 % texture2D4.Width, num11 * num12 / texture2D4.Width * num10, num11, num10), 100f, num13, 999999, new Vector2((float)(Game1.viewport.Width + 192 + num11 * 4 / 2) - Game1.dialogueFont.MeasureString(Game1.parseText(item5.Value.Split('/')[14], Game1.dialogueFont, 256)).X / 2f, (float)Game1.viewport.Height * 0.5f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 1f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = num,
						text = Game1.parseText(item5.Value.Split('/')[14], Game1.dialogueFont, 256)
					});
					num += 1500;
				}
				catch (Exception)
				{
				}
			}
			return stringBuilder.ToString();
		}

		private bool sayGrufferSummitIntro(NPC spouse)
		{
			switch ((string)spouse.name)
			{
			case "Harvey":
			case "Elliott":
				return false;
			case "Abigail":
			case "Maru":
				return true;
			default:
				return spouse.Gender == 0;
			}
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
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
			foreach (Projectile projectile in projectiles)
			{
				projectile.draw(b);
			}
			if (!Game1.eventUp)
			{
				return;
			}
			if (isShowingEndSlideshow)
			{
				b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f - 400f), Game1.viewport.Width, 8), Utility.GetPrismaticColor());
				b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f - 412f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.8f);
				b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f - 432f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.6f);
				b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f - 468f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.4f);
				b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f - 536f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.2f);
				b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f + 240f), Game1.viewport.Width, 8), Utility.GetPrismaticColor());
				b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f + 256f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.8f);
				b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f + 276f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.6f);
				b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f + 312f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.4f);
				b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f + 380f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.2f);
			}
			foreach (TemporaryAnimatedSprite temporarySprite in base.TemporarySprites)
			{
				if (temporarySprite.drawAboveAlwaysFront)
				{
					temporarySprite.draw(b);
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
		}
	}
}
