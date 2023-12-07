using System;
using System.Linq;

namespace StardewValley.BellsAndWhistles
{
	public class Lexicon
	{
		public static string getRandomNegativeItemSlanderNoun()
		{
			Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			string[] array = Game1.content.LoadString("Strings\\Lexicon:RandomNegativeItemNoun").Split('#');
			return array[random.Next(array.Length)];
		}

		public static string getProperArticleForWord(string word)
		{
			if (LocalizedContentManager.CurrentLanguageCode != 0)
			{
				return "";
			}
			string text = "a";
			if (word != null && word.Length > 0)
			{
				switch (word.ToLower()[0])
				{
				case 'a':
					text += "n";
					break;
				case 'e':
					text += "n";
					break;
				case 'i':
					text += "n";
					break;
				case 'o':
					text += "n";
					break;
				case 'u':
					text += "n";
					break;
				}
			}
			return text;
		}

		public static string capitalize(string text)
		{
			if (text == null || text == "" || LocalizedContentManager.CurrentLanguageCode != 0)
			{
				return text;
			}
			int num = 0;
			for (int i = 0; i < text.Length; i++)
			{
				if (char.IsLetter(text[i]))
				{
					num = i;
					break;
				}
			}
			if (num == 0)
			{
				return text.First().ToString().ToUpper() + text.Substring(1);
			}
			return text.Substring(0, num) + text[num].ToString().ToUpper() + text.Substring(num + 1);
		}

		public static string makePlural(string word, bool ignore = false)
		{
			if (ignore || LocalizedContentManager.CurrentLanguageCode != 0 || word == null)
			{
				return word;
			}
			if (word.EndsWith(" Seeds"))
			{
				return word;
			}
			if (word.EndsWith(" Shorts"))
			{
				return word;
			}
			if (word.EndsWith(" Bass"))
			{
				return word;
			}
			if (word.EndsWith(" Flowers"))
			{
				return word;
			}
			if (word != null)
			{
				switch (word.Length)
				{
				case 12:
				{
					char c = word[0];
					if (c != 'D')
					{
						if (c != 'G')
						{
							if (c != 'R' || !(word == "Rice Pudding"))
							{
								break;
							}
							return "bowls of Rice Pudding";
						}
						if (!(word == "Glass Shards"))
						{
							break;
						}
						goto IL_0574;
					}
					if (!(word == "Dragon Tooth"))
					{
						break;
					}
					return "Dragon Teeth";
				}
				case 10:
				{
					char c = word[0];
					if ((uint)c <= 67u)
					{
						if (c == 'A')
						{
							if (!(word == "Algae Soup"))
							{
								break;
							}
							return "bowls of Algae Soup";
						}
						if (c != 'C' || !(word == "Crab Cakes"))
						{
							break;
						}
					}
					else if (c != 'H')
					{
						if (c != 'T' || !(word == "Tea Leaves"))
						{
							break;
						}
					}
					else if (!(word == "Hashbrowns"))
					{
						break;
					}
					goto IL_0574;
				}
				case 4:
					switch (word[3])
					{
					case 'l':
						if (!(word == "Coal"))
						{
							goto end_IL_005b;
						}
						return "lumps of Coal";
					case 't':
						if (!(word == "Salt"))
						{
							goto end_IL_005b;
						}
						return "pieces of Salt";
					case 'p':
						break;
					case 'b':
						goto IL_0325;
					case 's':
						goto IL_033a;
					case 'y':
						goto IL_034f;
					default:
						goto end_IL_005b;
					}
					if (!(word == "Carp"))
					{
						break;
					}
					goto IL_0574;
				case 5:
				{
					char c = word[4];
					if ((uint)c <= 115u)
					{
						if (c != 'm')
						{
							if (c != 's' || !(word == "Weeds"))
							{
								break;
							}
						}
						else if (!(word == "Bream"))
						{
							break;
						}
						goto IL_0574;
					}
					switch (c)
					{
					case 'y':
						if (!(word == "Jelly"))
						{
							break;
						}
						return "Jellies";
					case 't':
						if (!(word == "Wheat"))
						{
							break;
						}
						return "bushels of Wheat";
					}
					break;
				}
				case 6:
					switch (word[1])
					{
					case 'i':
						if (!(word == "Ginger"))
						{
							break;
						}
						return "pieces of Ginger";
					case 'a':
						if (!(word == "Garlic"))
						{
							break;
						}
						return "bulbs of Garlic";
					}
					break;
				case 9:
				{
					char c = word[0];
					if (c != 'D')
					{
						if (c != 'G')
						{
							if (c != 'R' || !(word == "Red Canes"))
							{
								break;
							}
						}
						else if (!(word == "Ghostfish"))
						{
							break;
						}
					}
					else if (!(word == "Driftwood"))
					{
						break;
					}
					goto IL_0574;
				}
				case 11:
					switch (word[4])
					{
					case 'd':
						break;
					case 'b':
						goto IL_0436;
					case 'e':
						goto IL_044b;
					case 'n':
						goto IL_0460;
					case ' ':
						goto IL_0475;
					default:
						goto end_IL_005b;
					}
					if (!(word == "Mixed Seeds"))
					{
						break;
					}
					goto IL_0574;
				case 15:
				{
					char c = word[0];
					if (c != 'F')
					{
						if (c != 'L')
						{
							if (c != 'S' || !(word == "Smallmouth Bass"))
							{
								break;
							}
						}
						else if (!(word == "Largemouth Bass"))
						{
							break;
						}
					}
					else if (!(word == "Fossilized Ribs"))
					{
						break;
					}
					goto IL_0574;
				}
				case 8:
				{
					char c = word[0];
					if (c != 'P')
					{
						if (c != 'S' || !(word == "Sandfish"))
						{
							break;
						}
					}
					else if (!(word == "Pancakes"))
					{
						break;
					}
					goto IL_0574;
				}
				case 14:
				{
					char c = word[0];
					if (c != 'B')
					{
						if (c != 'P' || !(word == "Pepper Poppers"))
						{
							break;
						}
					}
					else if (!(word == "Broken Glasses"))
					{
						break;
					}
					goto IL_0574;
				}
				case 16:
					if (!(word == "Dried Sunflowers"))
					{
						break;
					}
					goto IL_0574;
				case 17:
					if (!(word == "Roasted Hazelnuts"))
					{
						break;
					}
					goto IL_0574;
				case 7:
					{
						if (!(word == "Pickles"))
						{
							break;
						}
						goto IL_0574;
					}
					IL_0436:
					if (!(word == "Cranberries"))
					{
						break;
					}
					goto IL_0574;
					IL_0574:
					return word;
					IL_034f:
					if (!(word == "Clay"))
					{
						break;
					}
					goto IL_0574;
					IL_0325:
					if (!(word == "Chub"))
					{
						break;
					}
					goto IL_0574;
					IL_033a:
					if (!(word == "Hops"))
					{
						break;
					}
					goto IL_0574;
					IL_0475:
					if (!(word == "Star Shards"))
					{
						break;
					}
					goto IL_0574;
					IL_0460:
					if (!(word == "Green Canes"))
					{
						break;
					}
					goto IL_0574;
					IL_044b:
					if (!(word == "Glazed Yams"))
					{
						break;
					}
					goto IL_0574;
					end_IL_005b:
					break;
				}
			}
			if (word.Last() == 'y')
			{
				return word.Substring(0, word.Length - 1) + "ies";
			}
			if (word.Last() == 's' || word.Last() == 'z' || word.Last() == 'x' || (word.Length > 2 && word.Substring(word.Length - 2) == "sh") || (word.Length > 2 && word.Substring(word.Length - 2) == "ch"))
			{
				return word + "es";
			}
			return word + "s";
		}

		public static string prependArticle(string word)
		{
			if (LocalizedContentManager.CurrentLanguageCode != 0)
			{
				return word;
			}
			return getProperArticleForWord(word) + " " + word;
		}

		public static string getRandomPositiveAdjectiveForEventOrPerson(NPC n = null)
		{
			Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			string[] array = ((n != null && n.Age != 0) ? Game1.content.LoadString("Strings\\Lexicon:RandomPositiveAdjective_Child").Split('#') : ((n != null && n.Gender == 0) ? Game1.content.LoadString("Strings\\Lexicon:RandomPositiveAdjective_AdultMale").Split('#') : ((n == null || n.Gender != 1) ? Game1.content.LoadString("Strings\\Lexicon:RandomPositiveAdjective_PlaceOrEvent").Split('#') : Game1.content.LoadString("Strings\\Lexicon:RandomPositiveAdjective_AdultFemale").Split('#'))));
			return array[random.Next(array.Length)];
		}

		public static string getRandomNegativeAdjectiveForEventOrPerson(NPC n = null)
		{
			Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			string[] array = ((n != null && n.Age != 0) ? Game1.content.LoadString("Strings\\Lexicon:RandomNegativeAdjective_Child").Split('#') : ((n != null && n.Gender == 0) ? Game1.content.LoadString("Strings\\Lexicon:RandomNegativeAdjective_AdultMale").Split('#') : ((n == null || n.Gender != 1) ? Game1.content.LoadString("Strings\\Lexicon:RandomNegativeAdjective_PlaceOrEvent").Split('#') : Game1.content.LoadString("Strings\\Lexicon:RandomNegativeAdjective_AdultFemale").Split('#'))));
			return array[random.Next(array.Length)];
		}

		public static string getRandomDeliciousAdjective(NPC n = null)
		{
			Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			string[] array = ((n == null || n.Age != 2) ? Game1.content.LoadString("Strings\\Lexicon:RandomDeliciousAdjective").Split('#') : Game1.content.LoadString("Strings\\Lexicon:RandomDeliciousAdjective_Child").Split('#'));
			return array[random.Next(array.Length)];
		}

		public static string getRandomNegativeFoodAdjective(NPC n = null)
		{
			Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			string[] array = ((n != null && n.Age == 2) ? Game1.content.LoadString("Strings\\Lexicon:RandomNegativeFoodAdjective_Child").Split('#') : ((n == null || n.Manners != 1) ? Game1.content.LoadString("Strings\\Lexicon:RandomNegativeFoodAdjective").Split('#') : Game1.content.LoadString("Strings\\Lexicon:RandomNegativeFoodAdjective_Polite").Split('#')));
			return array[random.Next(array.Length)];
		}

		public static string getRandomSlightlyPositiveAdjectiveForEdibleNoun(NPC n = null)
		{
			Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			string[] array = Game1.content.LoadString("Strings\\Lexicon:RandomSlightlyPositiveFoodAdjective").Split('#');
			return array[random.Next(array.Length)];
		}

		public static string getGenderedChildTerm(bool isMale)
		{
			if (isMale)
			{
				return Game1.content.LoadString("Strings\\Lexicon:ChildTerm_Male");
			}
			return Game1.content.LoadString("Strings\\Lexicon:ChildTerm_Female");
		}

		public static string getPronoun(bool isMale)
		{
			if (isMale)
			{
				return Game1.content.LoadString("Strings\\Lexicon:Pronoun_Male");
			}
			return Game1.content.LoadString("Strings\\Lexicon:Pronoun_Female");
		}

		public static string getPossessivePronoun(bool isMale)
		{
			if (isMale)
			{
				return Game1.content.LoadString("Strings\\Lexicon:Possessive_Pronoun_Male");
			}
			return Game1.content.LoadString("Strings\\Lexicon:Possessive_Pronoun_Female");
		}
	}
}
