using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using StardewValley.GameData.Movies;
using StardewValley.Locations;

namespace StardewValley.Events
{
	public class MovieTheaterScreeningEvent
	{
		public int currentResponse;

		public List<List<Character>> playerAndGuestAudienceGroups;

		public Dictionary<int, Character> _responseOrder = new Dictionary<int, Character>();

		protected Dictionary<Character, Character> _whiteListDependencyLookup;

		protected Dictionary<Character, string> _characterResponses;

		public MovieData movieData;

		protected List<Farmer> _farmers;

		protected Dictionary<Character, MovieConcession> _concessionsData;

		public Event getMovieEvent(string movieID, List<List<Character>> player_and_guest_audience_groups, List<List<Character>> npcOnlyAudienceGroups, Dictionary<Character, MovieConcession> concessions_data = null)
		{
			_concessionsData = concessions_data;
			_responseOrder = new Dictionary<int, Character>();
			_whiteListDependencyLookup = new Dictionary<Character, Character>();
			_characterResponses = new Dictionary<Character, string>();
			movieData = MovieTheater.GetMovieData()[movieID];
			playerAndGuestAudienceGroups = player_and_guest_audience_groups;
			currentResponse = 0;
			StringBuilder stringBuilder = new StringBuilder();
			Random theaterRandom = new Random((int)(Game1.stats.DaysPlayed + Game1.uniqueIDForThisGame / 2uL));
			stringBuilder.Append("movieScreenAmbience/-2000 -2000/");
			string text = "farmer" + Utility.getFarmerNumberFromFarmer(Game1.player);
			string text2 = "";
			foreach (List<Character> playerAndGuestAudienceGroup in playerAndGuestAudienceGroups)
			{
				if (!playerAndGuestAudienceGroup.Contains(Game1.player))
				{
					continue;
				}
				for (int i = 0; i < playerAndGuestAudienceGroup.Count; i++)
				{
					if (!(playerAndGuestAudienceGroup[i] is Farmer))
					{
						text2 = playerAndGuestAudienceGroup[i].name;
					}
				}
			}
			_farmers = new List<Farmer>();
			foreach (List<Character> playerAndGuestAudienceGroup2 in playerAndGuestAudienceGroups)
			{
				foreach (Character item in playerAndGuestAudienceGroup2)
				{
					if (item is Farmer && !_farmers.Contains(item))
					{
						_farmers.Add(item as Farmer);
					}
				}
			}
			List<Character> list = playerAndGuestAudienceGroups.SelectMany((List<Character> x) => x).ToList();
			list.AddRange(npcOnlyAudienceGroups.SelectMany((List<Character> x) => x).ToList());
			bool flag = true;
			foreach (Character item2 in list)
			{
				if (item2 != null)
				{
					if (!flag)
					{
						stringBuilder.Append(" ");
					}
					if (item2 is Farmer)
					{
						Farmer who = item2 as Farmer;
						stringBuilder.Append("farmer" + Utility.getFarmerNumberFromFarmer(who));
					}
					else if (item2.name == "Krobus")
					{
						stringBuilder.Append("Krobus_Trenchcoat");
					}
					else
					{
						stringBuilder.Append(item2.name);
					}
					stringBuilder.Append(" -1000 -1000 0");
					flag = false;
				}
			}
			stringBuilder.Append("/changeToTemporaryMap MovieTheaterScreen false/specificTemporarySprite movieTheater_setup/ambientLight 0 0 0/");
			string[] array = new string[8];
			playerAndGuestAudienceGroups = playerAndGuestAudienceGroups.OrderBy((List<Character> x) => theaterRandom.Next()).ToList();
			int num = theaterRandom.Next(8 - playerAndGuestAudienceGroups.SelectMany((List<Character> x) => x).Count() + 1);
			int num2 = 0;
			for (int j = 0; j < 8; j++)
			{
				int num3 = (j + num) % 8;
				if (playerAndGuestAudienceGroups[num2].Count == 2 && (num3 == 3 || num3 == 7))
				{
					j++;
					num3++;
					num3 %= 8;
				}
				for (int k = 0; k < playerAndGuestAudienceGroups[num2].Count && num3 + k < array.Length; k++)
				{
					array[num3 + k] = ((playerAndGuestAudienceGroups[num2][k] is Farmer) ? ("farmer" + Utility.getFarmerNumberFromFarmer(playerAndGuestAudienceGroups[num2][k] as Farmer)) : ((string)playerAndGuestAudienceGroups[num2][k].name));
					if (k > 0)
					{
						j++;
					}
				}
				num2++;
				if (num2 >= playerAndGuestAudienceGroups.Count)
				{
					break;
				}
			}
			string[] array2 = new string[6];
			for (int l = 0; l < npcOnlyAudienceGroups.Count; l++)
			{
				int num4 = theaterRandom.Next(3 - npcOnlyAudienceGroups[l].Count + 1) + l * 3;
				for (int m = 0; m < npcOnlyAudienceGroups[l].Count; m++)
				{
					array2[num4 + m] = npcOnlyAudienceGroups[l][m].name;
				}
			}
			int num5 = 0;
			int num6 = 0;
			for (int n = 0; n < array.Length; n++)
			{
				if (array[n] == null || !(array[n] != "") || !(array[n] != text) || !(array[n] != text2))
				{
					continue;
				}
				num5++;
				if (num5 < 2)
				{
					continue;
				}
				num6++;
				Point backRowSeatTileFromIndex = getBackRowSeatTileFromIndex(n);
				stringBuilder.Append("warp ").Append(array[n]).Append(" ")
					.Append(backRowSeatTileFromIndex.X)
					.Append(" ")
					.Append(backRowSeatTileFromIndex.Y)
					.Append("/positionOffset ")
					.Append(array[n])
					.Append(" 0 -10/");
				if (num6 == 2)
				{
					num6 = 0;
					if (theaterRandom.NextDouble() < 0.5 && array[n] != text2 && array[n - 1] != text2)
					{
						stringBuilder.Append("faceDirection " + array[n] + " 3 true/");
						stringBuilder.Append("faceDirection " + array[n - 1] + " 1 true/");
					}
				}
			}
			num5 = 0;
			num6 = 0;
			for (int num7 = 0; num7 < array2.Length; num7++)
			{
				if (array2[num7] == null || !(array2[num7] != ""))
				{
					continue;
				}
				num5++;
				if (num5 < 2)
				{
					continue;
				}
				num6++;
				Point midRowSeatTileFromIndex = getMidRowSeatTileFromIndex(num7);
				stringBuilder.Append("warp ").Append(array2[num7]).Append(" ")
					.Append(midRowSeatTileFromIndex.X)
					.Append(" ")
					.Append(midRowSeatTileFromIndex.Y)
					.Append("/positionOffset ")
					.Append(array2[num7])
					.Append(" 0 -10/");
				if (num6 == 2)
				{
					num6 = 0;
					if (num7 != 3 && theaterRandom.NextDouble() < 0.5)
					{
						stringBuilder.Append("faceDirection " + array2[num7] + " 3 true/");
						stringBuilder.Append("faceDirection " + array2[num7 - 1] + " 1 true/");
					}
				}
			}
			Point point = new Point(1, 15);
			num5 = 0;
			for (int num8 = 0; num8 < array.Length; num8++)
			{
				if (array[num8] != null && array[num8] != "" && array[num8] != text && array[num8] != text2)
				{
					Point backRowSeatTileFromIndex2 = getBackRowSeatTileFromIndex(num8);
					if (num5 == 1)
					{
						stringBuilder.Append("warp ").Append(array[num8]).Append(" ")
							.Append(backRowSeatTileFromIndex2.X - 1)
							.Append(" 10")
							.Append("/advancedMove ")
							.Append(array[num8])
							.Append(" false 1 " + 200 + " 1 0 4 1000/")
							.Append("positionOffset ")
							.Append(array[num8])
							.Append(" 0 -10/");
					}
					else
					{
						stringBuilder.Append("warp ").Append(array[num8]).Append(" 1 12")
							.Append("/advancedMove ")
							.Append(array[num8])
							.Append(" false 1 200 ")
							.Append("0 -2 ")
							.Append(backRowSeatTileFromIndex2.X - 1)
							.Append(" 0 4 1000/")
							.Append("positionOffset ")
							.Append(array[num8])
							.Append(" 0 -10/");
					}
					num5++;
				}
				if (num5 >= 2)
				{
					break;
				}
			}
			num5 = 0;
			for (int num9 = 0; num9 < array2.Length; num9++)
			{
				if (array2[num9] != null && array2[num9] != "")
				{
					Point midRowSeatTileFromIndex2 = getMidRowSeatTileFromIndex(num9);
					if (num5 == 1)
					{
						stringBuilder.Append("warp ").Append(array2[num9]).Append(" ")
							.Append(midRowSeatTileFromIndex2.X - 1)
							.Append(" 8")
							.Append("/advancedMove ")
							.Append(array2[num9])
							.Append(" false 1 " + 400 + " 1 0 4 1000/");
					}
					else
					{
						stringBuilder.Append("warp ").Append(array2[num9]).Append(" 2 9")
							.Append("/advancedMove ")
							.Append(array2[num9])
							.Append(" false 1 300 ")
							.Append("0 -1 ")
							.Append(midRowSeatTileFromIndex2.X - 2)
							.Append(" 0 4 1000/");
					}
					num5++;
				}
				if (num5 >= 2)
				{
					break;
				}
			}
			stringBuilder.Append("viewport 6 8 true/pause 500/");
			for (int num10 = 0; num10 < array.Length; num10++)
			{
				if (array[num10] != null && array[num10] != "")
				{
					Point backRowSeatTileFromIndex3 = getBackRowSeatTileFromIndex(num10);
					if (array[num10] == text || array[num10] == text2)
					{
						stringBuilder.Append("warp ").Append(array[num10]).Append(" ")
							.Append(point.X)
							.Append(" ")
							.Append(point.Y)
							.Append("/advancedMove ")
							.Append(array[num10])
							.Append(" false 0 -5 ")
							.Append(backRowSeatTileFromIndex3.X - point.X)
							.Append(" 0 4 1000/")
							.Append("pause ")
							.Append(1000)
							.Append("/");
					}
				}
			}
			stringBuilder.Append("pause 3000/proceedPosition ").Append(text2).Append("/pause 1000");
			if (text2.Equals(""))
			{
				stringBuilder.Append("/proceedPosition farmer");
			}
			stringBuilder.Append("/waitForAllStationary/pause 100");
			foreach (Character item3 in list)
			{
				if (getEventName(item3) != text && getEventName(item3) != text2)
				{
					if (item3 is Farmer)
					{
						stringBuilder.Append("/faceDirection ").Append(getEventName(item3)).Append(" 0 true/positionOffset ")
							.Append(getEventName(item3))
							.Append(" 0 42 true");
					}
					else
					{
						stringBuilder.Append("/faceDirection ").Append(getEventName(item3)).Append(" 0 true/positionOffset ")
							.Append(getEventName(item3))
							.Append(" 0 12 true");
					}
					if (theaterRandom.NextDouble() < 0.2)
					{
						stringBuilder.Append("/pause 100");
					}
				}
			}
			stringBuilder.Append("/positionOffset ").Append(text).Append(" 0 32/positionOffset ")
				.Append(text2)
				.Append(" 0 8/ambientLight 210 210 120 true/pause 500/viewport move 0 -1 4000/pause 5000");
			List<Character> list2 = new List<Character>();
			foreach (List<Character> playerAndGuestAudienceGroup3 in playerAndGuestAudienceGroups)
			{
				foreach (Character item4 in playerAndGuestAudienceGroup3)
				{
					if (!(item4 is Farmer) && !list2.Contains(item4))
					{
						list2.Add(item4);
					}
				}
			}
			for (int num11 = 0; num11 < list2.Count; num11++)
			{
				int index = theaterRandom.Next(list2.Count);
				Character value = list2[num11];
				list2[num11] = list2[index];
				list2[index] = value;
			}
			int num12 = 0;
			foreach (MovieScene scene in movieData.Scenes)
			{
				if (scene.ResponsePoint == null)
				{
					continue;
				}
				bool flag2 = false;
				for (int num13 = 0; num13 < list2.Count; num13++)
				{
					MovieCharacterReaction reactionsForCharacter = MovieTheater.GetReactionsForCharacter(list2[num13] as NPC);
					if (reactionsForCharacter == null)
					{
						continue;
					}
					foreach (MovieReaction reaction in reactionsForCharacter.Reactions)
					{
						if (!reaction.ShouldApplyToMovie(movieData, MovieTheater.GetPatronNames(), MovieTheater.GetResponseForMovie(list2[num13] as NPC)) || reaction.SpecialResponses == null || reaction.SpecialResponses.DuringMovie == null || (!(reaction.SpecialResponses.DuringMovie.ResponsePoint == scene.ResponsePoint) && reaction.Whitelist.Count <= 0))
						{
							continue;
						}
						if (!_whiteListDependencyLookup.ContainsKey(list2[num13]))
						{
							_responseOrder[num12] = list2[num13];
							if (reaction.Whitelist != null)
							{
								for (int num14 = 0; num14 < reaction.Whitelist.Count; num14++)
								{
									Character characterFromName = Game1.getCharacterFromName(reaction.Whitelist[num14]);
									if (characterFromName == null)
									{
										continue;
									}
									_whiteListDependencyLookup[characterFromName] = list2[num13];
									foreach (int key in _responseOrder.Keys)
									{
										if (_responseOrder[key] == characterFromName)
										{
											_responseOrder.Remove(key);
										}
									}
								}
							}
						}
						list2.RemoveAt(num13);
						num13--;
						flag2 = true;
						break;
					}
					if (flag2)
					{
						break;
					}
				}
				if (!flag2)
				{
					for (int num15 = 0; num15 < list2.Count; num15++)
					{
						MovieCharacterReaction reactionsForCharacter2 = MovieTheater.GetReactionsForCharacter(list2[num15] as NPC);
						if (reactionsForCharacter2 == null)
						{
							continue;
						}
						foreach (MovieReaction reaction2 in reactionsForCharacter2.Reactions)
						{
							if (!reaction2.ShouldApplyToMovie(movieData, MovieTheater.GetPatronNames(), MovieTheater.GetResponseForMovie(list2[num15] as NPC)) || reaction2.SpecialResponses == null || reaction2.SpecialResponses.DuringMovie == null || !(reaction2.SpecialResponses.DuringMovie.ResponsePoint == num12.ToString()))
							{
								continue;
							}
							if (!_whiteListDependencyLookup.ContainsKey(list2[num15]))
							{
								_responseOrder[num12] = list2[num15];
								if (reaction2.Whitelist != null)
								{
									for (int num16 = 0; num16 < reaction2.Whitelist.Count; num16++)
									{
										Character characterFromName2 = Game1.getCharacterFromName(reaction2.Whitelist[num16]);
										if (characterFromName2 == null)
										{
											continue;
										}
										_whiteListDependencyLookup[characterFromName2] = list2[num15];
										foreach (int key2 in _responseOrder.Keys)
										{
											if (_responseOrder[key2] == characterFromName2)
											{
												_responseOrder.Remove(key2);
											}
										}
									}
								}
							}
							list2.RemoveAt(num15);
							num15--;
							flag2 = true;
							break;
						}
						if (flag2)
						{
							break;
						}
					}
				}
				num12++;
			}
			num12 = 0;
			for (int num17 = 0; num17 < list2.Count; num17++)
			{
				if (!_whiteListDependencyLookup.ContainsKey(list2[num17]))
				{
					for (; _responseOrder.ContainsKey(num12); num12++)
					{
					}
					_responseOrder[num12] = list2[num17];
					num12++;
				}
			}
			list2 = null;
			foreach (MovieScene scene2 in movieData.Scenes)
			{
				_ParseScene(stringBuilder, scene2);
			}
			while (currentResponse < _responseOrder.Count)
			{
				_ParseResponse(stringBuilder);
			}
			stringBuilder.Append("/stopMusic");
			stringBuilder.Append("/fade/viewport -1000 -1000");
			stringBuilder.Append("/pause 500/message \"" + Game1.content.LoadString("Strings\\Locations:Theater_MovieEnd") + "\"/pause 500");
			stringBuilder.Append("/requestMovieEnd");
			Console.WriteLine(stringBuilder.ToString());
			return new Event(stringBuilder.ToString());
		}

		protected void _ParseScene(StringBuilder sb, MovieScene scene)
		{
			if (scene.Sound != "")
			{
				sb.Append("/playSound " + scene.Sound);
			}
			if (scene.Music != "")
			{
				sb.Append("/playMusic " + scene.Music);
			}
			if (scene.MessageDelay > 0)
			{
				sb.Append("/pause " + scene.MessageDelay);
			}
			if (scene.Image >= 0)
			{
				sb.Append("/specificTemporarySprite movieTheater_screen " + movieData.SheetIndex + " " + scene.Image + " " + scene.Shake);
			}
			if (scene.Script != "")
			{
				sb.Append(scene.Script);
			}
			if (scene.Text != "")
			{
				sb.Append("/message \"" + scene.Text + "\"");
			}
			if (scene.ResponsePoint != null)
			{
				_ParseResponse(sb, scene);
			}
		}

		protected void _ParseResponse(StringBuilder sb, MovieScene scene = null)
		{
			if (_responseOrder.ContainsKey(currentResponse))
			{
				sb.Append("/pause 500");
				Character character = _responseOrder[currentResponse];
				bool ignoreScript = false;
				if (!_whiteListDependencyLookup.ContainsKey(character))
				{
					MovieCharacterReaction reactionsForCharacter = MovieTheater.GetReactionsForCharacter(character as NPC);
					if (reactionsForCharacter != null)
					{
						foreach (MovieReaction reaction in reactionsForCharacter.Reactions)
						{
							if (reaction.ShouldApplyToMovie(movieData, MovieTheater.GetPatronNames(), MovieTheater.GetResponseForMovie(character as NPC)) && reaction.SpecialResponses != null && reaction.SpecialResponses.DuringMovie != null && (reaction.SpecialResponses.DuringMovie.ResponsePoint == null || reaction.SpecialResponses.DuringMovie.ResponsePoint == "" || (scene != null && reaction.SpecialResponses.DuringMovie.ResponsePoint == scene.ResponsePoint) || reaction.SpecialResponses.DuringMovie.ResponsePoint == currentResponse.ToString() || reaction.Whitelist.Count > 0))
							{
								if (reaction.SpecialResponses.DuringMovie.Script != "")
								{
									sb.Append(reaction.SpecialResponses.DuringMovie.Script);
									ignoreScript = true;
								}
								if (reaction.SpecialResponses.DuringMovie.Text != "")
								{
									sb.Append(string.Concat("/speak ", character.name, " \"", reaction.SpecialResponses.DuringMovie.Text, "\""));
								}
								break;
							}
						}
					}
				}
				_ParseCharacterResponse(sb, character, ignoreScript);
				foreach (Character key in _whiteListDependencyLookup.Keys)
				{
					if (_whiteListDependencyLookup[key] == character)
					{
						_ParseCharacterResponse(sb, key);
					}
				}
			}
			currentResponse++;
		}

		protected void _ParseCharacterResponse(StringBuilder sb, Character responding_character, bool ignoreScript = false)
		{
			string responseForMovie = MovieTheater.GetResponseForMovie(responding_character as NPC);
			if (_whiteListDependencyLookup.ContainsKey(responding_character))
			{
				responseForMovie = MovieTheater.GetResponseForMovie(_whiteListDependencyLookup[responding_character] as NPC);
			}
			switch (responseForMovie)
			{
			case "love":
				sb.Append("/friendship " + responding_character.Name + " " + 200);
				if (!ignoreScript)
				{
					sb.Append(string.Concat("/playSound reward/emote ", responding_character.name, " ", 20.ToString(), "/message \"", Game1.content.LoadString("Strings\\Characters:MovieTheater_LoveMovie", responding_character.displayName), "\""));
				}
				break;
			case "like":
				sb.Append("/friendship " + responding_character.Name + " " + 100);
				if (!ignoreScript)
				{
					sb.Append(string.Concat("/playSound give_gift/emote ", responding_character.name, " ", 56.ToString(), "/message \"", Game1.content.LoadString("Strings\\Characters:MovieTheater_LikeMovie", responding_character.displayName), "\""));
				}
				break;
			case "dislike":
				sb.Append("/friendship " + responding_character.Name + " " + 0);
				if (!ignoreScript)
				{
					sb.Append(string.Concat("/playSound newArtifact/emote ", responding_character.name, " ", 24.ToString(), "/message \"", Game1.content.LoadString("Strings\\Characters:MovieTheater_DislikeMovie", responding_character.displayName), "\""));
				}
				break;
			}
			if (_concessionsData != null && _concessionsData.ContainsKey(responding_character))
			{
				MovieConcession movieConcession = _concessionsData[responding_character];
				string concessionTasteForCharacter = MovieTheater.GetConcessionTasteForCharacter(responding_character, movieConcession);
				string text = "";
				Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
				if (dictionary.ContainsKey(responding_character.name))
				{
					string[] array = dictionary[responding_character.name].Split('/');
					if (array[4] == "female")
					{
						text = "_Female";
					}
					else if (array[4] == "male")
					{
						text = "_Male";
					}
				}
				string text2 = "eat";
				if (movieConcession.tags != null && movieConcession.tags.Contains("Drink"))
				{
					text2 = "gulp";
				}
				switch (concessionTasteForCharacter)
				{
				case "love":
					sb.Append("/friendship " + responding_character.Name + " " + 50);
					sb.Append("/tossConcession " + responding_character.Name + " " + movieConcession.id + "/pause 1000");
					sb.Append("/playSound " + text2 + "/shake " + responding_character.Name + " 500/pause 1000");
					sb.Append(string.Concat("/playSound reward/emote ", responding_character.name, " ", 20.ToString(), "/message \"", Game1.content.LoadString("Strings\\Characters:MovieTheater_LoveConcession" + text, responding_character.displayName, movieConcession.DisplayName), "\""));
					break;
				case "like":
					sb.Append("/friendship " + responding_character.Name + " " + 25);
					sb.Append("/tossConcession " + responding_character.Name + " " + movieConcession.id + "/pause 1000");
					sb.Append("/playSound " + text2 + "/shake " + responding_character.Name + " 500/pause 1000");
					sb.Append(string.Concat("/playSound give_gift/emote ", responding_character.name, " ", 56.ToString(), "/message \"", Game1.content.LoadString("Strings\\Characters:MovieTheater_LikeConcession" + text, responding_character.displayName, movieConcession.DisplayName), "\""));
					break;
				case "dislike":
					sb.Append("/friendship " + responding_character.Name + " " + 0);
					sb.Append("/playSound croak/pause 1000");
					sb.Append(string.Concat("/playSound newArtifact/emote ", responding_character.name, " ", 40.ToString(), "/message \"", Game1.content.LoadString("Strings\\Characters:MovieTheater_DislikeConcession" + text, responding_character.displayName, movieConcession.DisplayName), "\""));
					break;
				}
			}
			_characterResponses[responding_character] = responseForMovie;
		}

		public Dictionary<Character, string> GetCharacterResponses()
		{
			return _characterResponses;
		}

		private static string getEventName(Character c)
		{
			if (c is Farmer)
			{
				return "farmer" + Utility.getFarmerNumberFromFarmer(c as Farmer);
			}
			return c.name;
		}

		private Point getBackRowSeatTileFromIndex(int index)
		{
			return index switch
			{
				0 => new Point(2, 10), 
				1 => new Point(3, 10), 
				2 => new Point(4, 10), 
				3 => new Point(5, 10), 
				4 => new Point(8, 10), 
				5 => new Point(9, 10), 
				6 => new Point(10, 10), 
				7 => new Point(11, 10), 
				_ => new Point(4, 12), 
			};
		}

		private Point getMidRowSeatTileFromIndex(int index)
		{
			return index switch
			{
				0 => new Point(3, 8), 
				1 => new Point(4, 8), 
				2 => new Point(5, 8), 
				3 => new Point(8, 8), 
				4 => new Point(9, 8), 
				5 => new Point(10, 8), 
				_ => new Point(4, 12), 
			};
		}
	}
}
