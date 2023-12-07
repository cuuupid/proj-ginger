using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Objects
{
	public class TV : Furniture
	{
		public const int customChannel = 1;

		public const int weatherChannel = 2;

		public const int fortuneTellerChannel = 3;

		public const int tipsChannel = 4;

		public const int cookingChannel = 5;

		public const int fishingChannel = 6;

		private int currentChannel;

		private TemporaryAnimatedSprite screen;

		private TemporaryAnimatedSprite screenOverlay;

		internal static Dictionary<int, string> weekToRecipeMap;

		public TV()
		{
		}

		public TV(int which, Vector2 tile)
			: base(which, tile)
		{
		}

		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			List<Response> list = new List<Response>();
			list.Add(new Response("Weather", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13105")));
			list.Add(new Response("Fortune", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13107")));
			string text = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
			if (text.Equals("Mon") || text.Equals("Thu"))
			{
				list.Add(new Response("Livin'", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13111")));
			}
			if (text.Equals("Sun"))
			{
				list.Add(new Response("The", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13114")));
			}
			if (text.Equals("Wed") && Game1.stats.DaysPlayed > 7)
			{
				list.Add(new Response("The", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13117")));
			}
			if (Game1.Date.Season == "fall" && Game1.Date.DayOfMonth == 26 && Game1.stats.getStat("childrenTurnedToDoves") != 0 && !who.mailReceived.Contains("cursed_doll"))
			{
				list.Add(new Response("???", "???"));
			}
			if (Game1.player.mailReceived.Contains("pamNewChannel"))
			{
				list.Add(new Response("Fishing", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel")));
			}
			list.Add(new Response("(Leave)", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13118")));
			Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13120"), list.ToArray(), selectChannel);
			Game1.player.Halt();
			return true;
		}

		public override Item getOne()
		{
			TV tV = new TV(parentSheetIndex, tileLocation);
			tV.drawPosition.Value = drawPosition;
			tV.defaultBoundingBox.Value = defaultBoundingBox;
			tV.boundingBox.Value = boundingBox;
			tV.currentRotation.Value = (int)currentRotation - 1;
			tV.rotations.Value = rotations;
			tV.rotate();
			tV._GetOneFrom(this);
			return tV;
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			base.updateWhenCurrentLocation(time, environment);
		}

		public virtual void selectChannel(Farmer who, string answer)
		{
			switch (answer.Split(' ')[0])
			{
			case "Weather":
				currentChannel = 2;
				screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(413, 305, 42, 28), 150f, 2, 999999, getScreenPosition(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				Game1.drawObjectDialogue(Game1.parseText(getWeatherChannelOpening()));
				Game1.afterDialogues = proceedToNextScene;
				break;
			case "Fortune":
				currentChannel = 3;
				screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(540, 305, 42, 28), 150f, 2, 999999, getScreenPosition(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				Game1.drawObjectDialogue(Game1.parseText(getFortuneTellerOpening()));
				Game1.afterDialogues = proceedToNextScene;
				break;
			case "Livin'":
				currentChannel = 4;
				screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(517, 361, 42, 28), 150f, 2, 999999, getScreenPosition(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13124")));
				Game1.afterDialogues = proceedToNextScene;
				break;
			case "The":
				currentChannel = 5;
				screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(602, 361, 42, 28), 150f, 2, 999999, getScreenPosition(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13127")));
				Game1.afterDialogues = proceedToNextScene;
				break;
			case "???":
				Game1.changeMusicTrack("none");
				currentChannel = 666;
				screen = new TemporaryAnimatedSprite("Maps\\springobjects", new Rectangle(112, 64, 16, 16), 150f, 1, 999999, getScreenPosition() + (((int)parentSheetIndex == 1468) ? new Vector2(56f, 32f) : new Vector2(8f, 8f)), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, 3f, 0f, 0f, 0f);
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Cursed_Doll")));
				Game1.afterDialogues = proceedToNextScene;
				break;
			case "Fishing":
				currentChannel = 6;
				screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(172, 33, 42, 28), 150f, 2, 999999, getScreenPosition(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Fishing_Channel_Intro")));
				Game1.afterDialogues = proceedToNextScene;
				break;
			}
		}

		protected virtual string getFortuneTellerOpening()
		{
			switch (Game1.random.Next(5))
			{
			case 0:
				if (!Game1.player.IsMale)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13130");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13128");
			case 1:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13132");
			case 2:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13133");
			case 3:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13134");
			case 4:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13135");
			default:
				return "";
			}
		}

		protected virtual string getWeatherChannelOpening()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13136");
		}

		public virtual float getScreenSizeModifier()
		{
			if ((int)parentSheetIndex != 1468 && (int)parentSheetIndex != 2326)
			{
				return 2f;
			}
			return 4f;
		}

		public virtual Vector2 getScreenPosition()
		{
			if ((int)parentSheetIndex == 1466)
			{
				return new Vector2(boundingBox.X + 24, boundingBox.Y);
			}
			if ((int)parentSheetIndex == 1468)
			{
				return new Vector2(boundingBox.X + 12, boundingBox.Y - 128 + 32);
			}
			if ((int)parentSheetIndex == 2326)
			{
				return new Vector2(boundingBox.X + 12, boundingBox.Y - 128 + 40);
			}
			if ((int)parentSheetIndex == 1680)
			{
				return new Vector2(boundingBox.X + 24, boundingBox.Y - 12);
			}
			return Vector2.Zero;
		}

		public virtual void proceedToNextScene()
		{
			if (currentChannel == 2)
			{
				if (screenOverlay == null)
				{
					screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(497, 305, 42, 28), 9999f, 1, 999999, getScreenPosition(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f)
					{
						id = 777f
					};
					Game1.drawObjectDialogue(Game1.parseText(getWeatherForecast()));
					setWeatherOverlay();
					Game1.afterDialogues = proceedToNextScene;
				}
				else if (Game1.player.hasOrWillReceiveMail("Visited_Island") && screen.id == 777f)
				{
					screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(148, 62, 42, 28), 9999f, 1, 999999, getScreenPosition(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
					Game1.drawObjectDialogue(Game1.parseText(getIslandWeatherForecast()));
					setWeatherOverlay(island: true);
					Game1.afterDialogues = proceedToNextScene;
				}
				else
				{
					turnOffTV();
				}
			}
			else if (currentChannel == 3)
			{
				if (screenOverlay == null)
				{
					screen = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(624, 305, 42, 28), 9999f, 1, 999999, getScreenPosition(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 1E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
					Game1.drawObjectDialogue(Game1.parseText(getFortuneForecast(Game1.player)));
					setFortuneOverlay(Game1.player);
					Game1.afterDialogues = proceedToNextScene;
				}
				else
				{
					turnOffTV();
				}
			}
			else if (currentChannel == 4)
			{
				if (screenOverlay == null)
				{
					Game1.drawObjectDialogue(Game1.parseText(getTodaysTip()));
					Game1.afterDialogues = proceedToNextScene;
					screenOverlay = new TemporaryAnimatedSprite
					{
						alpha = 1E-07f
					};
				}
				else
				{
					turnOffTV();
				}
			}
			else if (currentChannel == 5)
			{
				if (screenOverlay == null)
				{
					Game1.multipleDialogues(getWeeklyRecipe());
					Game1.afterDialogues = proceedToNextScene;
					screenOverlay = new TemporaryAnimatedSprite
					{
						alpha = 1E-07f
					};
				}
				else
				{
					turnOffTV();
				}
			}
			else if (currentChannel == 666)
			{
				Game1.flashAlpha = 1f;
				Game1.soundBank.PlayCue("batScreech");
				Game1.createItemDebris(new Object(103, 1), Game1.player.getStandingPosition(), 1, Game1.currentLocation);
				Game1.player.mailReceived.Add("cursed_doll");
				turnOffTV();
			}
			else if (currentChannel == 6)
			{
				if (screenOverlay == null)
				{
					Game1.multipleDialogues(getFishingInfo());
					Game1.afterDialogues = proceedToNextScene;
					screenOverlay = new TemporaryAnimatedSprite
					{
						alpha = 1E-07f
					};
				}
				else
				{
					turnOffTV();
				}
			}
		}

		public virtual void turnOffTV()
		{
			screen = null;
			screenOverlay = null;
		}

		protected virtual void setWeatherOverlay(bool island = false)
		{
			WorldDate worldDate = new WorldDate(Game1.Date);
			worldDate.TotalDays++;
			switch (island ? Game1.netWorldState.Value.GetWeatherForLocation(Game1.getLocationFromName("IslandSouth").GetLocationContext()).weatherForTomorrow.Value : ((!Game1.IsMasterGame) ? Game1.getWeatherModificationsForDate(worldDate, Game1.netWorldState.Value.WeatherForTomorrow) : Game1.getWeatherModificationsForDate(worldDate, Game1.weatherForTomorrow)))
			{
			case 0:
			case 6:
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(413, 333, 13, 13), 100f, 4, 999999, getScreenPosition() + new Vector2(3f, 3f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				break;
			case 5:
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(465, 346, 13, 13), 100f, 4, 999999, getScreenPosition() + new Vector2(3f, 3f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				break;
			case 1:
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(465, 333, 13, 13), 70f, 4, 999999, getScreenPosition() + new Vector2(3f, 3f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				break;
			case 2:
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", Game1.currentSeason.Equals("spring") ? new Rectangle(465, 359, 13, 13) : (Game1.currentSeason.Equals("fall") ? new Rectangle(413, 359, 13, 13) : new Rectangle(465, 346, 13, 13)), 70f, 4, 999999, getScreenPosition() + new Vector2(3f, 3f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				break;
			case 3:
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(413, 346, 13, 13), 120f, 4, 999999, getScreenPosition() + new Vector2(3f, 3f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				break;
			case 4:
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(413, 372, 13, 13), 120f, 4, 999999, getScreenPosition() + new Vector2(3f, 3f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
				break;
			}
		}

		private string[] getFishingInfo()
		{
			List<string> list = new List<string>();
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			int seasonNumber = Utility.getSeasonNumber(Game1.currentSeason);
			stringBuilder.AppendLine("---" + Utility.getSeasonNameFromNumber(seasonNumber) + "---^^");
			Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
			Dictionary<string, string> dictionary2 = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
			List<string> list2 = new List<string>();
			int num = 0;
			foreach (KeyValuePair<int, string> item in dictionary)
			{
				if (item.Value.Contains("spring summer fall winter"))
				{
					continue;
				}
				list2.Clear();
				foreach (KeyValuePair<string, string> item2 in dictionary2)
				{
					string[] array = item2.Value.Split('/');
					if (array[4 + seasonNumber].Contains(item.Key.ToString() ?? "") && !list2.Contains(getSanitizedFishingLocation(item2.Key)))
					{
						list2.Add(getSanitizedFishingLocation(item2.Key));
					}
				}
				if (list2.Count <= 0)
				{
					continue;
				}
				string[] array2 = item.Value.Split('/');
				string value = ((array2.Count() > 13) ? array2[13] : array2[0]);
				string text = array2[7];
				string value2 = array2[5].Split(' ')[0];
				string value3 = array2[5].Split(' ')[1];
				stringBuilder2.Append(value);
				stringBuilder2.Append("...... ");
				stringBuilder2.Append(Game1.getTimeOfDayString(Convert.ToInt32(value2)).Replace(" ", ""));
				stringBuilder2.Append("-");
				stringBuilder2.Append(Game1.getTimeOfDayString(Convert.ToInt32(value3)).Replace(" ", ""));
				if (text != "both")
				{
					stringBuilder2.Append(", " + Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel_" + text));
				}
				bool flag = false;
				foreach (string item3 in list2)
				{
					if (item3 != "")
					{
						flag = true;
						stringBuilder2.Append(", ");
						stringBuilder2.Append(item3);
					}
				}
				if (flag)
				{
					stringBuilder2.Append("^^");
					stringBuilder.Append(stringBuilder2.ToString());
					num++;
				}
				stringBuilder2.Clear();
				if (num > 3)
				{
					list.Add(stringBuilder.ToString());
					stringBuilder.Clear();
					num = 0;
				}
			}
			return list.ToArray();
		}

		private string getSanitizedFishingLocation(string rawLocationName)
		{
			switch (rawLocationName)
			{
			case "Town":
			case "Forest":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel_River");
			case "Beach":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel_Ocean");
			case "Mountain":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel_Lake");
			default:
				return "";
			}
		}

		protected virtual string getTodaysTip()
		{
			Dictionary<string, string> dictionary = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\TV\\TipChannel");
			return dictionary.ContainsKey((Game1.stats.DaysPlayed % 224u).ToString() ?? "") ? dictionary[(Game1.stats.DaysPlayed % 224u).ToString() ?? ""] : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13148");
		}

		protected int getRerunWeek()
		{
			int num = (int)(Game1.stats.DaysPlayed - 3);
			int num2 = Math.Min(num / 7, 32);
			if (weekToRecipeMap == null)
			{
				weekToRecipeMap = new Dictionary<int, string>();
				Dictionary<string, string> dictionary = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\TV\\CookingChannel");
				foreach (string key in dictionary.Keys)
				{
					weekToRecipeMap[Convert.ToInt32(key)] = dictionary[key].Split('/')[0];
				}
			}
			List<int> list = new List<int>();
			IEnumerable<Farmer> allFarmers = Game1.getAllFarmers();
			for (int i = 1; i <= num2; i++)
			{
				foreach (Farmer item in allFarmers)
				{
					if (!item.cookingRecipes.ContainsKey(weekToRecipeMap[i]))
					{
						list.Add(i);
						break;
					}
				}
			}
			Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			if (list.Count == 0)
			{
				return Math.Max(1, 1 + random.Next(num2));
			}
			return list[random.Next(list.Count)];
		}

		protected virtual string[] getWeeklyRecipe()
		{
			string[] array = new string[2];
			int num = (int)(Game1.stats.DaysPlayed % 224u / 7u);
			if (Game1.stats.DaysPlayed % 224u == 0)
			{
				num = 32;
			}
			Dictionary<string, string> dictionary = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\TV\\CookingChannel");
			FarmerTeam team = Game1.player.team;
			if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Wed"))
			{
				if ((int)team.lastDayQueenOfSauceRerunUpdated != Game1.Date.TotalDays)
				{
					team.lastDayQueenOfSauceRerunUpdated.Set(Game1.Date.TotalDays);
					team.queenOfSauceRerunWeek.Set(getRerunWeek());
				}
				num = team.queenOfSauceRerunWeek.Value;
			}
			try
			{
				string text = dictionary[num.ToString() ?? ""].Split('/')[0];
				array[0] = dictionary[num.ToString() ?? ""].Split('/')[1];
				if (CraftingRecipe.cookingRecipes.ContainsKey(text))
				{
					string[] array2 = CraftingRecipe.cookingRecipes[text].Split('/');
					array[1] = ((LocalizedContentManager.CurrentLanguageCode != 0) ? (Game1.player.cookingRecipes.ContainsKey(dictionary[num.ToString() ?? ""].Split('/')[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", array2[array2.Length - 1]) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", array2[array2.Length - 1])) : (Game1.player.cookingRecipes.ContainsKey(dictionary[num.ToString() ?? ""].Split('/')[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", text) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", text)));
				}
				else
				{
					array[1] = ((LocalizedContentManager.CurrentLanguageCode != 0) ? (Game1.player.cookingRecipes.ContainsKey(dictionary[num.ToString() ?? ""].Split('/')[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", dictionary[num.ToString() ?? ""].Split('/').Last()) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", dictionary[num.ToString() ?? ""].Split('/').Last())) : (Game1.player.cookingRecipes.ContainsKey(dictionary[num.ToString() ?? ""].Split('/')[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", text) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", text)));
				}
				if (!Game1.player.cookingRecipes.ContainsKey(text))
				{
					Game1.player.cookingRecipes.Add(text, 0);
					return array;
				}
				return array;
			}
			catch (Exception)
			{
				string text2 = dictionary["1"].Split('/')[0];
				array[0] = dictionary["1"].Split('/')[1];
				array[1] = ((LocalizedContentManager.CurrentLanguageCode != 0) ? (Game1.player.cookingRecipes.ContainsKey(dictionary["1"].Split('/')[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", dictionary["1"].Split('/').Last()) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", dictionary["1"].Split('/').Last())) : (Game1.player.cookingRecipes.ContainsKey(dictionary["1"].Split('/')[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", text2) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", text2)));
				if (!Game1.player.cookingRecipes.ContainsKey(text2))
				{
					Game1.player.cookingRecipes.Add(text2, 0);
					return array;
				}
				return array;
			}
		}

		private string getIslandWeatherForecast()
		{
			new WorldDate(Game1.Date).TotalDays++;
			int value = Game1.netWorldState.Value.GetWeatherForLocation(Game1.getLocationFromName("IslandSouth").GetLocationContext()).weatherForTomorrow.Value;
			string text = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_IslandWeatherIntro");
			return value switch
			{
				0 => text + ((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13182") : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13183")), 
				1 => text + Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13184"), 
				3 => text + Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13185"), 
				_ => text + "???", 
			};
		}

		protected virtual string getWeatherForecast()
		{
			WorldDate worldDate = new WorldDate(Game1.Date);
			worldDate.TotalDays++;
			switch ((!Game1.IsMasterGame) ? Game1.getWeatherModificationsForDate(worldDate, Game1.netWorldState.Value.WeatherForTomorrow) : Game1.getWeatherModificationsForDate(worldDate, Game1.weatherForTomorrow))
			{
			case 4:
			{
				Dictionary<string, string> dictionary;
				try
				{
					dictionary = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + (Game1.dayOfMonth + 1));
				}
				catch (Exception)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13164");
				}
				string text = dictionary["name"];
				string text2 = dictionary["conditions"].Split('/')[0];
				int time = Convert.ToInt32(dictionary["conditions"].Split('/')[1].Split(' ')[0]);
				int time2 = Convert.ToInt32(dictionary["conditions"].Split('/')[1].Split(' ')[1]);
				string text3 = "";
				switch (text2)
				{
				case "Town":
					text3 = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13170");
					break;
				case "Beach":
					text3 = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13172");
					break;
				case "Forest":
					text3 = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13174");
					break;
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13175", text, text3, Game1.getTimeOfDayString(time), Game1.getTimeOfDayString(time2));
			}
			case 5:
				if (!(Game1.random.NextDouble() < 0.5))
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13181");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13180");
			case 0:
			case 6:
				if (!(Game1.random.NextDouble() < 0.5))
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13183");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13182");
			case 1:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13184");
			case 3:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13185");
			case 2:
				if (!Game1.currentSeason.Equals("spring"))
				{
					if (!Game1.currentSeason.Equals("fall"))
					{
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13190");
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13189");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13187");
			default:
				return "";
			}
		}

		public virtual void setFortuneOverlay(Farmer who)
		{
			if (who.DailyLuck < -0.07)
			{
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(592, 346, 13, 13), 100f, 4, 999999, getScreenPosition() + new Vector2(15f, 1f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
			}
			else if (who.DailyLuck < -0.02)
			{
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(540, 346, 13, 13), 100f, 4, 999999, getScreenPosition() + new Vector2(15f, 1f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
			}
			else if (who.DailyLuck > 0.07)
			{
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(644, 333, 13, 13), 100f, 4, 999999, getScreenPosition() + new Vector2(15f, 1f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
			}
			else if (who.DailyLuck > 0.02)
			{
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(592, 333, 13, 13), 100f, 4, 999999, getScreenPosition() + new Vector2(15f, 1f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
			}
			else
			{
				screenOverlay = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(540, 333, 13, 13), 100f, 4, 999999, getScreenPosition() + new Vector2(15f, 1f) * getScreenSizeModifier(), flicker: false, flipped: false, (float)(boundingBox.Bottom - 1) / 10000f + 2E-05f, 0f, Color.White, getScreenSizeModifier(), 0f, 0f, 0f);
			}
		}

		public virtual string getFortuneForecast(Farmer who)
		{
			string text = "";
			Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			text = (((double)who.team.sharedDailyLuck == -0.12) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13191") : ((who.DailyLuck < -0.07) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13192") : ((who.DailyLuck < -0.02) ? ((random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13193") : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13195")) : (((double)who.team.sharedDailyLuck == 0.12) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13197") : ((who.DailyLuck > 0.07) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13198") : ((!(who.DailyLuck > 0.02)) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13200") : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13199")))))));
			if (who.DailyLuck == 0.0)
			{
				text = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13201");
			}
			return text;
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			base.draw(spriteBatch, x, y, alpha);
			if (screen != null)
			{
				screen.update(Game1.currentGameTime);
				screen.draw(spriteBatch);
				if (screenOverlay != null)
				{
					screenOverlay.update(Game1.currentGameTime);
					screenOverlay.draw(spriteBatch);
				}
			}
		}
	}
}
