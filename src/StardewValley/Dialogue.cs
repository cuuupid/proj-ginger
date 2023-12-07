using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley
{
	public class Dialogue
	{
		public delegate bool onAnswerQuestion(int whichResponse);

		public const string dialogueHappy = "$h";

		public const string dialogueSad = "$s";

		public const string dialogueUnique = "$u";

		public const string dialogueNeutral = "$neutral";

		public const string dialogueLove = "$l";

		public const string dialogueAngry = "$a";

		public const string dialogueEnd = "$e";

		public const string dialogueBreak = "$b";

		public const string dialogueKill = "$k";

		public const string dialogueChance = "$c";

		public const string dialogueDependingOnWorldState = "$d";

		public const string dialogueQuickResponse = "$y";

		public const string dialoguePrerequisite = "$p";

		public const string dialogueSingle = "$1";

		public const string dialogueQuestion = "$q";

		public const string dialogueResponse = "$r";

		public const string breakSpecialCharacter = "{";

		public const string playerNameSpecialCharacter = "@";

		public const string genderDialogueSplitCharacter = "^";

		public const string genderDialogueSplitCharacter2 = "¦";

		public const string quickResponseDelineator = "*";

		public const string randomAdjectiveSpecialCharacter = "%adj";

		public const string randomNounSpecialCharacter = "%noun";

		public const string randomPlaceSpecialCharacter = "%place";

		public const string spouseSpecialCharacter = "%spouse";

		public const string randomNameSpecialCharacter = "%name";

		public const string firstNameLettersSpecialCharacter = "%firstnameletter";

		public const string timeSpecialCharacter = "%time";

		public const string bandNameSpecialCharacter = "%band";

		public const string bookNameSpecialCharacter = "%book";

		public const string rivalSpecialCharacter = "%rival";

		public const string petSpecialCharacter = "%pet";

		public const string farmNameSpecialCharacter = "%farm";

		public const string favoriteThingSpecialCharacter = "%favorite";

		public const string eventForkSpecialCharacter = "%fork";

		public const string yearSpecialCharacter = "%year";

		public const string kid1specialCharacter = "%kid1";

		public const string kid2SpecialCharacter = "%kid2";

		public const string revealTasteCharacter = "%revealtaste";

		private static bool nameArraysTranslated = false;

		public static string[] adjectives = new string[20]
		{
			"Purple", "Gooey", "Chalky", "Green", "Plush", "Chunky", "Gigantic", "Greasy", "Gloomy", "Practical",
			"Lanky", "Dopey", "Crusty", "Fantastic", "Rubbery", "Silly", "Courageous", "Reasonable", "Lonely", "Bitter"
		};

		public static string[] nouns = new string[23]
		{
			"Dragon", "Buffet", "Biscuit", "Robot", "Planet", "Pepper", "Tomb", "Hyena", "Lip", "Quail",
			"Cheese", "Disaster", "Raincoat", "Shoe", "Castle", "Elf", "Pump", "Chip", "Wig", "Mermaid",
			"Drumstick", "Puppet", "Submarine"
		};

		public static string[] verbs = new string[13]
		{
			"ran", "danced", "spoke", "galloped", "ate", "floated", "stood", "flowed", "smelled", "swam",
			"grilled", "cracked", "melted"
		};

		public static string[] positional = new string[13]
		{
			"atop", "near", "with", "alongside", "away from", "too close to", "dangerously close to", "far, far away from", "uncomfortably close to", "way above the",
			"miles below", "on a different planet from", "in a different century than"
		};

		public static string[] places = new string[12]
		{
			"Castle Village", "Basket Town", "Pine Mesa City", "Point Drake", "Minister Valley", "Grampleton", "Zuzu City", "a small island off the coast", "Fort Josa", "Chestervale",
			"Fern Islands", "Tanker Grove"
		};

		public static string[] colors = new string[16]
		{
			"/crimson", "/green", "/tan", "/purple", "/deep blue", "/neon pink", "/pale/yellow", "/chocolate/brown", "/sky/blue", "/bubblegum/pink",
			"/blood/red", "/bright/orange", "/aquamarine", "/silvery", "/glimmering/gold", "/rainbow"
		};

		public List<string> dialogues = new List<string>();

		private List<NPCDialogueResponse> playerResponses;

		private List<string> quickResponses;

		private bool isLastDialogueInteractive;

		private bool quickResponse;

		public bool isCurrentStringContinuedOnNextScreen;

		private bool dialogueToBeKilled;

		private bool finishedLastDialogue;

		public bool showPortrait;

		public bool removeOnNextMove;

		public string temporaryDialogueKey;

		public int currentDialogueIndex;

		private string currentEmotion;

		public string temporaryDialogue;

		public NPC speaker;

		public onAnswerQuestion answerQuestionBehavior;

		public Texture2D overridePortrait;

		public Action onFinish;

		public string CurrentEmotion
		{
			get
			{
				return currentEmotion;
			}
			set
			{
				currentEmotion = value;
			}
		}

		public Farmer farmer
		{
			get
			{
				if (Game1.CurrentEvent != null)
				{
					return Game1.CurrentEvent.farmer;
				}
				return Game1.player;
			}
		}

		private static void TranslateArraysOfStrings()
		{
			colors = new string[16]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.795"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.796"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.797"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.798"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.799"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.800"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.801"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.802"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.803"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.804"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.805"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.806"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.807"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.808"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.809"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.810")
			};
			adjectives = new string[20]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.679"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.680"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.681"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.682"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.683"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.684"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.685"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.686"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.687"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.688"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.689"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.690"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.691"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.692"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.693"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.694"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.695"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.696"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.697"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.698")
			};
			nouns = new string[23]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.699"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.700"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.701"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.702"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.703"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.704"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.705"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.706"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.707"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.708"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.709"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.710"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.711"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.712"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.713"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.714"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.715"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.716"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.717"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.718"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.719"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.720"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.721")
			};
			verbs = new string[13]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.722"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.723"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.724"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.725"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.726"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.727"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.728"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.729"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.730"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.731"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.732"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.733"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.734")
			};
			positional = new string[13]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.735"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.736"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.737"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.738"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.739"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.740"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.741"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.742"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.743"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.744"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.745"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.746"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.747")
			};
			places = new string[12]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.748"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.749"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.750"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.751"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.752"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.753"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.754"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.755"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.756"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.757"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.758"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.759")
			};
			nameArraysTranslated = true;
		}

		public Dialogue(string masterDialogue, NPC speaker)
		{
			if (!nameArraysTranslated)
			{
				TranslateArraysOfStrings();
			}
			this.speaker = speaker;
			parseDialogueString(masterDialogue);
			checkForSpecialDialogueAttributes();
		}

		public void setCurrentDialogue(string dialogue)
		{
			dialogues.Clear();
			currentDialogueIndex = 0;
			parseDialogueString(dialogue);
		}

		public void addMessageToFront(string dialogue)
		{
			currentDialogueIndex = 0;
			List<string> list = new List<string>();
			list.AddRange(dialogues);
			dialogues.Clear();
			parseDialogueString(dialogue);
			dialogues.AddRange(list);
			checkForSpecialDialogueAttributes();
		}

		public static string getRandomVerb()
		{
			if (!nameArraysTranslated)
			{
				TranslateArraysOfStrings();
			}
			return verbs[Game1.random.Next(verbs.Length)];
		}

		public static string getRandomAdjective()
		{
			if (!nameArraysTranslated)
			{
				TranslateArraysOfStrings();
			}
			return adjectives[Game1.random.Next(adjectives.Length)];
		}

		public static string getRandomNoun()
		{
			if (!nameArraysTranslated)
			{
				TranslateArraysOfStrings();
			}
			return nouns[Game1.random.Next(nouns.Length)];
		}

		public static string getRandomPositional()
		{
			if (!nameArraysTranslated)
			{
				TranslateArraysOfStrings();
			}
			return positional[Game1.random.Next(positional.Length)];
		}

		public int getPortraitIndex()
		{
			switch (currentEmotion)
			{
			case "$neutral":
				return 0;
			case "$h":
				return 1;
			case "$s":
				return 2;
			case "$u":
				return 3;
			case "$l":
				return 4;
			case "$a":
				return 5;
			default:
			{
				if (int.TryParse(currentEmotion.Substring(1), out var _))
				{
					return Convert.ToInt32(currentEmotion.Substring(1));
				}
				return 0;
			}
			}
		}

		private void parseDialogueString(string masterString)
		{
			if (masterString == null)
			{
				masterString = "...";
			}
			temporaryDialogue = null;
			if (playerResponses != null)
			{
				playerResponses.Clear();
			}
			string[] array = masterString.Split('#');
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Length < 2)
				{
					continue;
				}
				array[i] = checkForSpecialCharacters(array[i]);
				string text;
				try
				{
					text = array[i].Substring(0, 2);
				}
				catch (Exception)
				{
					text = "     ";
				}
				if (text.Equals("$e"))
				{
					continue;
				}
				if (text.Equals("$b"))
				{
					if (dialogues.Count > 0)
					{
						dialogues[dialogues.Count - 1] += "{";
					}
				}
				else if (text.Equals("$k"))
				{
					dialogueToBeKilled = true;
				}
				else if (text.Equals("$1") && array[i].Split(' ').Length > 1)
				{
					string text2 = array[i].Split(' ')[1];
					if (!farmer.mailReceived.Contains(text2))
					{
						array[i + 1] = checkForSpecialCharacters(array[i + 1]);
						dialogues.Add(text2 + "}" + array[i + 1]);
						i = 99999;
						break;
					}
					i += 3;
					if (i < array.Length)
					{
						array[i] = checkForSpecialCharacters(array[i]);
						dialogues.Add(array[i]);
					}
				}
				else if (text.Equals("$c") && array[i].Split(' ').Length > 1)
				{
					double num = Convert.ToDouble(array[i].Split(' ')[1]);
					if (Game1.random.NextDouble() > num)
					{
						i++;
						continue;
					}
					dialogues.Add(array[i + 1]);
					i += 3;
				}
				else if (text.Equals("$q"))
				{
					if (dialogues.Count > 0)
					{
						dialogues[dialogues.Count - 1] += "{";
					}
					string[] array2 = array[i].Split(' ');
					string[] array3 = array2[1].Split('/');
					bool flag = false;
					for (int j = 0; j < array3.Length; j++)
					{
						if (farmer.DialogueQuestionsAnswered.Contains(Convert.ToInt32(array3[j])))
						{
							flag = true;
							break;
						}
					}
					if (flag && Convert.ToInt32(array3[0]) != -1)
					{
						if (!array2[2].Equals("null"))
						{
							array = array.Take(i).ToArray().Concat(speaker.Dialogue[array2[2]].Split('#'))
								.ToArray();
							i--;
						}
					}
					else
					{
						isLastDialogueInteractive = true;
					}
				}
				else if (text.Equals("$r"))
				{
					string[] array4 = array[i].Split(' ');
					if (playerResponses == null)
					{
						playerResponses = new List<NPCDialogueResponse>();
					}
					isLastDialogueInteractive = true;
					playerResponses.Add(new NPCDialogueResponse(Convert.ToInt32(array4[1]), Convert.ToInt32(array4[2]), array4[3], array[i + 1]));
					i++;
				}
				else if (text.Equals("$p"))
				{
					string[] array5 = array[i].Split(' ');
					string[] array6 = array[i + 1].Split('|');
					bool flag2 = false;
					for (int k = 1; k < array5.Length; k++)
					{
						if (farmer.DialogueQuestionsAnswered.Contains(Convert.ToInt32(array5[1])))
						{
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						array = array6[0].Split('#');
						i = -1;
					}
					else
					{
						array[i + 1] = array[i + 1].Split('|').Last();
					}
				}
				else if (text.Equals("$d"))
				{
					string[] array7 = array[i].Split(' ');
					string text3 = masterString.Substring(masterString.IndexOf('#') + 1);
					bool flag3 = false;
					switch (array7[1].ToLower())
					{
					case "joja":
						flag3 = Game1.isLocationAccessible("JojaMart");
						break;
					case "cc":
					case "communitycenter":
						flag3 = Game1.isLocationAccessible("CommunityCenter");
						break;
					case "bus":
						flag3 = farmer.mailReceived.Contains("ccVault");
						break;
					case "kent":
						flag3 = Game1.year >= 2;
						break;
					}
					char separator = (text3.Contains('|') ? '|' : '#');
					array = ((!flag3) ? text3.Split(separator)[1].Split('#') : text3.Split(separator)[0].Split('#'));
					i--;
				}
				else if (text.Equals("$y"))
				{
					quickResponse = true;
					isLastDialogueInteractive = true;
					if (quickResponses == null)
					{
						quickResponses = new List<string>();
					}
					if (playerResponses == null)
					{
						playerResponses = new List<NPCDialogueResponse>();
					}
					string text4 = array[i].Substring(array[i].IndexOf('\'') + 1);
					text4 = text4.Substring(0, text4.Length - 1);
					string[] array8 = text4.Split('_');
					dialogues.Add(array8[0]);
					for (int l = 1; l < array8.Length; l += 2)
					{
						playerResponses.Add(new NPCDialogueResponse(-1, -1, "quickResponse" + l, Game1.parseText(array8[l])));
						quickResponses.Add(array8[l + 1].Replace("*", "#$b#"));
					}
				}
				else if (array[i].Contains("^"))
				{
					if (farmer.IsMale)
					{
						dialogues.Add(array[i].Substring(0, array[i].IndexOf("^")));
					}
					else
					{
						dialogues.Add(array[i].Substring(array[i].IndexOf("^") + 1));
					}
				}
				else if (array[i].Contains("¦"))
				{
					if (farmer.IsMale)
					{
						dialogues.Add(array[i].Substring(0, array[i].IndexOf("¦")));
					}
					else
					{
						dialogues.Add(array[i].Substring(array[i].IndexOf("¦") + 1));
					}
				}
				else
				{
					dialogues.Add(array[i]);
				}
			}
		}

		public void prepareDialogueForDisplay()
		{
			if (dialogues.Count > 0)
			{
				if (speaker != null && (bool)speaker.shouldWearIslandAttire && Game1.player.friendshipData.ContainsKey(speaker.Name) && Game1.player.friendshipData[speaker.Name].IsDivorced() && currentEmotion == "$u")
				{
					currentEmotion = "$neutral";
				}
				dialogues[currentDialogueIndex] = Utility.ParseGiftReveals(dialogues[currentDialogueIndex]);
			}
		}

		public string getCurrentDialogue()
		{
			if (currentDialogueIndex >= dialogues.Count || finishedLastDialogue)
			{
				return "";
			}
			showPortrait = true;
			if (speaker.Name.Equals("Dwarf") && !farmer.canUnderstandDwarves)
			{
				return convertToDwarvish(dialogues[currentDialogueIndex]);
			}
			if (temporaryDialogue != null)
			{
				return temporaryDialogue;
			}
			if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains("}"))
			{
				farmer.mailReceived.Add(dialogues[currentDialogueIndex].Split('}')[0]);
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Substring(dialogues[currentDialogueIndex].IndexOf("}") + 1);
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace("$k", "");
			}
			if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains('['))
			{
				int num = dialogues[currentDialogueIndex].IndexOf('[');
				int num2 = dialogues[currentDialogueIndex].IndexOf(']');
				if (num > 0 && num2 > 0)
				{
					string text = dialogues[currentDialogueIndex].Substring(num + 1, num2 - num - 1);
					string[] array = text.Split(' ');
					int result = -1;
					if (int.TryParse(array[Game1.random.Next(array.Length)], out result) && Game1.objectInformation.ContainsKey(result))
					{
						farmer.addItemToInventoryBool(new Object(Vector2.Zero, result, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false), makeActiveObject: true);
						farmer.showCarrying();
					}
					dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace("[" + text + "]", "");
				}
			}
			if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains("$k"))
			{
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace("$k", "");
				dialogues.RemoveRange(currentDialogueIndex + 1, dialogues.Count - 1 - currentDialogueIndex);
				finishedLastDialogue = true;
			}
			if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Length > 1 && dialogues[currentDialogueIndex][0] == '%')
			{
				showPortrait = false;
				return dialogues[currentDialogueIndex].Substring(1);
			}
			if (dialogues.Count <= 0)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.792");
			}
			return dialogues[currentDialogueIndex].Replace("%time", Game1.getTimeOfDayString(Game1.timeOfDay));
		}

		public bool isItemGrabDialogue()
		{
			if (dialogues.Count <= 0)
			{
				return false;
			}
			return dialogues[currentDialogueIndex].Contains('[');
		}

		public bool isOnFinalDialogue()
		{
			return currentDialogueIndex == dialogues.Count - 1;
		}

		public bool isDialogueFinished()
		{
			return finishedLastDialogue;
		}

		public string checkForSpecialCharacters(string str)
		{
			string text = Utility.FilterUserName(farmer.Name);
			str = str.Replace("@", text);
			str = str.Replace("%adj", adjectives[Game1.random.Next(adjectives.Length)].ToLower());
			if (str.Contains("%noun"))
			{
				str = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de) ? (str.Substring(0, str.IndexOf("%noun") + "%noun".Length).Replace("%noun", nouns[Game1.random.Next(nouns.Length)]) + str.Substring(str.IndexOf("%noun") + "%noun".Length).Replace("%noun", nouns[Game1.random.Next(nouns.Length)])) : (str.Substring(0, str.IndexOf("%noun") + "%noun".Length).Replace("%noun", nouns[Game1.random.Next(nouns.Length)].ToLower()) + str.Substring(str.IndexOf("%noun") + "%noun".Length).Replace("%noun", nouns[Game1.random.Next(nouns.Length)].ToLower())));
			}
			str = str.Replace("%place", places[Game1.random.Next(places.Length)]);
			str = str.Replace("%name", randomName());
			str = str.Replace("%firstnameletter", text.Substring(0, Math.Max(0, text.Length / 2)));
			str = str.Replace("%band", Game1.samBandName);
			if (str.Contains("%book"))
			{
				str = str.Replace("%book", Game1.elliottBookName);
			}
			if (!string.IsNullOrEmpty(str) && str.Contains("%spouse"))
			{
				if (farmer.spouse != null)
				{
					string[] array = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions")[farmer.spouse].Split('/');
					str = str.Replace("%spouse", array[array.Length - 1]);
				}
				else if (farmer.team.GetSpouse(farmer.UniqueMultiplayerID).HasValue)
				{
					Farmer farmerMaybeOffline = Game1.getFarmerMaybeOffline(farmer.team.GetSpouse(farmer.UniqueMultiplayerID).Value);
					str = str.Replace("%spouse", farmerMaybeOffline.Name);
				}
			}
			string newValue = Utility.FilterUserName(farmer.farmName);
			str = str.Replace("%farm", newValue);
			string newValue2 = Utility.FilterUserName(farmer.favoriteThing);
			str = str.Replace("%favorite", newValue2);
			str = str.Replace("%year", Game1.year.ToString() ?? "");
			int numberOfChildren = farmer.getNumberOfChildren();
			str = str.Replace("%kid1", (numberOfChildren > 0) ? farmer.getChildren()[0].displayName : Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.793"));
			str = str.Replace("%kid2", (numberOfChildren > 1) ? farmer.getChildren()[1].displayName : Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.794"));
			str = str.Replace("%pet", farmer.getPetDisplayName());
			if (str.Contains("¦"))
			{
				str = (farmer.IsMale ? str.Substring(0, str.IndexOf("¦")) : str.Substring(str.IndexOf("¦") + 1));
			}
			if (str.Contains("%fork"))
			{
				str = str.Replace("%fork", "");
				if (Game1.currentLocation.currentEvent != null)
				{
					Game1.currentLocation.currentEvent.specialEventVariable1 = true;
				}
			}
			str = str.Replace("%rival", Utility.getOtherFarmerNames()[0].Split(' ')[1]);
			return str;
		}

		public static string randomName()
		{
			string text = "";
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
			{
				string[] array = new string[38]
				{
					"ローゼン", "ミルド", "ココ", "ナミ", "こころ", "サルコ", "ハンゾー", "クッキー", "ココナツ", "せん",
					"ハル", "ラン", "オサム", "ヨシ", "ソラ", "ホシ", "まこと", "マサ", "ナナ", "リオ",
					"リン", "フジ", "うどん", "ミント", "さくら", "ボンボン", "レオ", "モリ", "コーヒー", "ミルク",
					"マロン", "クルミ", "サムライ", "カミ", "ゴロ", "マル", "チビ", "ユキダマ"
				};
				text = array[new Random().Next(0, array.Length)];
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh)
			{
				string[] array2 = new string[183]
				{
					"雨果", "蛋挞", "小百合", "毛毛", "小雨", "小溪", "精灵", "安琪儿", "小糕", "玫瑰",
					"小黄", "晓雨", "阿江", "铃铛", "马琪", "果粒", "郁金香", "小黑", "雨露", "小江",
					"灵力", "萝拉", "豆豆", "小莲", "斑点", "小雾", "阿川", "丽丹", "玛雅", "阿豆",
					"花花", "琉璃", "滴答", "阿山", "丹麦", "梅西", "橙子", "花儿", "晓璃", "小夕",
					"山大", "咪咪", "卡米", "红豆", "花朵", "洋洋", "太阳", "小岩", "汪汪", "玛利亚",
					"小菜", "花瓣", "阳阳", "小夏", "石头", "阿狗", "邱洁", "苹果", "梨花", "小希",
					"天天", "浪子", "阿猫", "艾薇儿", "雪梨", "桃花", "阿喜", "云朵", "风儿", "狮子",
					"绮丽", "雪莉", "樱花", "小喜", "朵朵", "田田", "小红", "宝娜", "梅子", "小樱",
					"嘻嘻", "云儿", "小草", "小黄", "纳香", "阿梅", "茶花", "哈哈", "芸儿", "东东",
					"小羽", "哈豆", "桃子", "茶叶", "双双", "沫沫", "楠楠", "小爱", "麦当娜", "杏仁",
					"椰子", "小王", "泡泡", "小林", "小灰", "马格", "鱼蛋", "小叶", "小李", "晨晨",
					"小琳", "小慧", "布鲁", "晓梅", "绿叶", "甜豆", "小雪", "晓林", "康康", "安妮",
					"樱桃", "香板", "甜甜", "雪花", "虹儿", "美美", "葡萄", "薇儿", "金豆", "雪玲",
					"瑶瑶", "龙眼", "丁香", "晓云", "雪豆", "琪琪", "麦子", "糖果", "雪丽", "小艺",
					"小麦", "小圆", "雨佳", "小火", "麦茶", "圆圆", "春儿", "火灵", "板子", "黑点",
					"冬冬", "火花", "米粒", "喇叭", "晓秋", "跟屁虫", "米果", "欢欢", "爱心", "松子",
					"丫头", "双子", "豆芽", "小子", "彤彤", "棉花糖", "阿贵", "仙儿", "冰淇淋", "小彬",
					"贤儿", "冰棒", "仔仔", "格子", "水果", "悠悠", "莹莹", "巧克力", "梦洁", "汤圆",
					"静香", "茄子", "珍珠"
				};
				text = array2[new Random().Next(0, array2.Length)];
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru)
			{
				string[] array3 = new string[50]
				{
					"Августина", "Альф", "Анфиса", "Ариша", "Афоня", "Баламут", "Балкан", "Бандит", "Бланка", "Бобик",
					"Боня", "Борька", "Буренка", "Бусинка", "Вася", "Гаврюша", "Глаша", "Гоша", "Дуня", "Дуся",
					"Зорька", "Ивонна", "Игнат", "Кеша", "Клара", "Кузя", "Лада", "Максимус", "Маня", "Марта",
					"Маруся", "Моня", "Мотя", "Мурзик", "Мурка", "Нафаня", "Ника", "Нюша", "Проша", "Пятнушка",
					"Сеня", "Сивка", "Тихон", "Тоша", "Фунтик", "Шайтан", "Юнона", "Юпитер", "Ягодка", "Яшка"
				};
				text = array3[new Random().Next(0, array3.Length)];
			}
			else
			{
				int num = Game1.random.Next(3, 6);
				string[] array4 = new string[24]
				{
					"B", "Br", "J", "F", "S", "M", "C", "Ch", "L", "P",
					"K", "W", "G", "Z", "Tr", "T", "Gr", "Fr", "Pr", "N",
					"Sn", "R", "Sh", "St"
				};
				string[] array5 = new string[12]
				{
					"ll", "tch", "l", "m", "n", "p", "r", "s", "t", "c",
					"rt", "ts"
				};
				string[] array6 = new string[5] { "a", "e", "i", "o", "u" };
				string[] array7 = new string[5] { "ie", "o", "a", "ers", "ley" };
				Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
				Dictionary<string, string[]> dictionary2 = new Dictionary<string, string[]>();
				dictionary.Add("a", new string[6] { "nie", "bell", "bo", "boo", "bella", "s" });
				dictionary.Add("e", new string[4] { "ll", "llo", "", "o" });
				dictionary.Add("i", new string[18]
				{
					"ck", "e", "bo", "ba", "lo", "la", "to", "ta", "no", "na",
					"ni", "a", "o", "zor", "que", "ca", "co", "mi"
				});
				dictionary.Add("o", new string[12]
				{
					"nie", "ze", "dy", "da", "o", "ver", "la", "lo", "s", "ny",
					"mo", "ra"
				});
				dictionary.Add("u", new string[4] { "rt", "mo", "", "s" });
				dictionary2.Add("a", new string[12]
				{
					"nny", "sper", "trina", "bo", "-bell", "boo", "lbert", "sko", "sh", "ck",
					"ishe", "rk"
				});
				dictionary2.Add("e", new string[9] { "lla", "llo", "rnard", "cardo", "ffe", "ppo", "ppa", "tch", "x" });
				dictionary2.Add("i", new string[18]
				{
					"llard", "lly", "lbo", "cky", "card", "ne", "nnie", "lbert", "nono", "nano",
					"nana", "ana", "nsy", "msy", "skers", "rdo", "rda", "sh"
				});
				dictionary2.Add("o", new string[17]
				{
					"nie", "zzy", "do", "na", "la", "la", "ver", "ng", "ngus", "ny",
					"-mo", "llo", "ze", "ra", "ma", "cco", "z"
				});
				dictionary2.Add("u", new string[11]
				{
					"ssie", "bbie", "ffy", "bba", "rt", "s", "mby", "mbo", "mbus", "ngus",
					"cky"
				});
				text += array4[Game1.random.Next(array4.Length - 1)];
				for (int i = 1; i < num - 1; i++)
				{
					text = ((i % 2 != 0) ? (text + array6[Game1.random.Next(array6.Length)]) : (text + array5[Game1.random.Next(array5.Length)]));
					if (text.Length >= num)
					{
						break;
					}
				}
				if (Game1.random.NextDouble() < 0.5 && !array6.Contains(text.ElementAt(text.Length - 1).ToString() ?? ""))
				{
					text += array7[Game1.random.Next(array7.Length)];
				}
				else if (array6.Contains(text.ElementAt(text.Length - 1).ToString() ?? ""))
				{
					if (Game1.random.NextDouble() < 0.8)
					{
						text = ((text.Length > 3) ? (text + dictionary[text.ElementAt(text.Length - 1).ToString() ?? ""].ElementAt(Game1.random.Next(dictionary[text.ElementAt(text.Length - 1).ToString() ?? ""].Length - 1))) : (text + dictionary2[text.ElementAt(text.Length - 1).ToString() ?? ""].ElementAt(Game1.random.Next(dictionary2[text.ElementAt(text.Length - 1).ToString() ?? ""].Length - 1))));
					}
				}
				else
				{
					text += array6[Game1.random.Next(array6.Length)];
				}
				for (int num2 = text.Length - 1; num2 > 2; num2--)
				{
					if (array6.Contains(text[num2].ToString()) && array6.Contains(text[num2 - 2].ToString()))
					{
						switch (text[num2 - 1])
						{
						case 'c':
							text = text.Substring(0, num2) + "k" + text.Substring(num2);
							num2--;
							break;
						case 'r':
							text = text.Substring(0, num2 - 1) + "k" + text.Substring(num2);
							num2--;
							break;
						case 'l':
							text = text.Substring(0, num2 - 1) + "n" + text.Substring(num2);
							num2--;
							break;
						}
					}
				}
				if (text.Length <= 3 && Game1.random.NextDouble() < 0.1)
				{
					text = ((Game1.random.NextDouble() < 0.5) ? (text + text) : (text + "-" + text));
				}
				if (text.Length <= 2 && text.Last() == 'e')
				{
					text += ((Game1.random.NextDouble() < 0.3) ? "m" : ((Game1.random.NextDouble() < 0.5) ? "p" : "b"));
				}
				if (text.ToLower().Contains("sex") || text.ToLower().Contains("taboo") || text.ToLower().Contains("fuck") || text.ToLower().Contains("rape") || text.ToLower().Contains("cock") || text.ToLower().Contains("willy") || text.ToLower().Contains("cum") || text.ToLower().Contains("goock") || text.ToLower().Contains("trann") || text.ToLower().Contains("gook") || text.ToLower().Contains("bitch") || text.ToLower().Contains("shit") || text.ToLower().Contains("pusie") || text.ToLower().Contains("kike") || text.ToLower().Contains("nigg") || text.ToLower().Contains("puss"))
				{
					text = ((Game1.random.NextDouble() < 0.5) ? "Bobo" : "Wumbus");
				}
			}
			return text;
		}

		public string exitCurrentDialogue()
		{
			if (isOnFinalDialogue() && onFinish != null)
			{
				onFinish();
			}
			if (temporaryDialogue != null)
			{
				return null;
			}
			bool flag = isCurrentStringContinuedOnNextScreen;
			if (currentDialogueIndex < dialogues.Count - 1)
			{
				currentDialogueIndex++;
				checkForSpecialDialogueAttributes();
			}
			else
			{
				finishedLastDialogue = true;
			}
			if (flag)
			{
				return getCurrentDialogue();
			}
			return null;
		}

		private void checkForSpecialDialogueAttributes()
		{
			if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains("{"))
			{
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace("{", "");
				isCurrentStringContinuedOnNextScreen = true;
			}
			else
			{
				isCurrentStringContinuedOnNextScreen = false;
			}
			checkEmotions();
		}

		private void checkEmotions()
		{
			if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains("$h"))
			{
				currentEmotion = "$h";
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace("$h", "");
			}
			else if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains("$s"))
			{
				currentEmotion = "$s";
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace("$s", "");
			}
			else if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains("$u"))
			{
				currentEmotion = "$u";
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace("$u", "");
			}
			else if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains("$l"))
			{
				currentEmotion = "$l";
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace("$l", "");
			}
			else if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains("$a"))
			{
				currentEmotion = "$a";
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace("$a", "");
			}
			else if (dialogues.Count > 0 && dialogues[currentDialogueIndex].Contains("$"))
			{
				int num = ((dialogues[currentDialogueIndex].Length <= dialogues[currentDialogueIndex].IndexOf("$") + 2 || !char.IsDigit(dialogues[currentDialogueIndex][dialogues[currentDialogueIndex].IndexOf("$") + 2])) ? 1 : 2);
				string oldValue = (currentEmotion = dialogues[currentDialogueIndex].Substring(dialogues[currentDialogueIndex].IndexOf("$"), num + 1));
				dialogues[currentDialogueIndex] = dialogues[currentDialogueIndex].Replace(oldValue, "");
			}
			else
			{
				currentEmotion = "$neutral";
			}
		}

		public List<NPCDialogueResponse> getNPCResponseOptions()
		{
			return playerResponses;
		}

		public List<Response> getResponseOptions()
		{
			return new List<Response>(((IEnumerable<NPCDialogueResponse>)playerResponses).Select((Func<NPCDialogueResponse, Response>)((NPCDialogueResponse x) => x)));
		}

		public bool isCurrentDialogueAQuestion()
		{
			if (isLastDialogueInteractive)
			{
				return currentDialogueIndex == dialogues.Count - 1;
			}
			return false;
		}

		public bool chooseResponse(Response response)
		{
			for (int i = 0; i < playerResponses.Count; i++)
			{
				if (playerResponses[i].responseKey == null || response.responseKey == null || !playerResponses[i].responseKey.Equals(response.responseKey))
				{
					continue;
				}
				if (answerQuestionBehavior != null)
				{
					if (answerQuestionBehavior(i))
					{
						Game1.currentSpeaker = null;
					}
					isLastDialogueInteractive = false;
					finishedLastDialogue = true;
					answerQuestionBehavior = null;
					return true;
				}
				if (quickResponse)
				{
					isLastDialogueInteractive = false;
					finishedLastDialogue = true;
					isCurrentStringContinuedOnNextScreen = true;
					speaker.setNewDialogue(quickResponses[i]);
					Game1.drawDialogue(speaker);
					speaker.faceTowardFarmerForPeriod(4000, 3, faceAway: false, farmer);
					return true;
				}
				if (Game1.isFestival())
				{
					Game1.currentLocation.currentEvent.answerDialogueQuestion(speaker, playerResponses[i].responseKey);
					isLastDialogueInteractive = false;
					finishedLastDialogue = true;
					return false;
				}
				farmer.changeFriendship(playerResponses[i].friendshipChange, speaker);
				if (playerResponses[i].id != -1)
				{
					farmer.addSeenResponse(playerResponses[i].id);
				}
				isLastDialogueInteractive = false;
				finishedLastDialogue = false;
				parseDialogueString(speaker.Dialogue[playerResponses[i].responseKey]);
				isCurrentStringContinuedOnNextScreen = true;
				return false;
			}
			return false;
		}

		public static string convertToDwarvish(string str)
		{
			string text = "";
			for (int i = 0; i < str.Length; i++)
			{
				switch (str[i])
				{
				case 'a':
					text += "o";
					continue;
				case 'e':
					text += "u";
					continue;
				case 'i':
					text += "e";
					continue;
				case 'o':
					text += "a";
					continue;
				case 'u':
					text += "i";
					continue;
				case 'y':
					text += "ol";
					continue;
				case 'z':
					text += "b";
					continue;
				case 'A':
					text += "O";
					continue;
				case 'E':
					text += "U";
					continue;
				case 'I':
					text += "E";
					continue;
				case 'O':
					text += "A";
					continue;
				case 'U':
					text += "I";
					continue;
				case 'Y':
					text += "Ol";
					continue;
				case 'Z':
					text += "B";
					continue;
				case '1':
					text += "M";
					continue;
				case '5':
					text += "X";
					continue;
				case '9':
					text += "V";
					continue;
				case '0':
					text += "Q";
					continue;
				case 'g':
					text += "l";
					continue;
				case 'c':
					text += "t";
					continue;
				case 't':
					text += "n";
					continue;
				case 'd':
					text += "p";
					continue;
				case ' ':
				case '!':
				case '"':
				case '\'':
				case ',':
				case '.':
				case '?':
				case 'h':
				case 'm':
				case 's':
					text += str[i];
					continue;
				case '\n':
				case 'n':
				case 'p':
					continue;
				}
				if (char.IsLetterOrDigit(str[i]))
				{
					text += (char)(str[i] + 2);
				}
			}
			return text.Replace("nhu", "doo");
		}
	}
}
