using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewValley.Util;
using xTile.Dimensions;

namespace StardewValley
{
	public class Utility
	{
		public static readonly char[] CharSpace = new char[1] { ' ' };

		public static Color[] PRISMATIC_COLORS = new Color[6]
		{
			Color.Red,
			new Color(255, 120, 0),
			new Color(255, 217, 0),
			Color.Lime,
			Color.Cyan,
			Color.Violet
		};

		public static List<VertexPositionColor[]> straightLineVertex = new List<VertexPositionColor[]>
		{
			new VertexPositionColor[2],
			new VertexPositionColor[2],
			new VertexPositionColor[2],
			new VertexPositionColor[2]
		};

		private static readonly ListPool<NPC> _pool = new ListPool<NPC>();

		public static readonly Vector2[] DirectionsTileVectors = new Vector2[4]
		{
			new Vector2(0f, -1f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(-1f, 0f)
		};

		public static readonly RasterizerState ScissorEnabled = new RasterizerState
		{
			ScissorTestEnable = true
		};

		[NonInstancedStatic]
		private static readonly RenderTargetBinding[] _bindings = new RenderTargetBinding[4];

		public static Microsoft.Xna.Framework.Rectangle controllerMapSourceRect(Microsoft.Xna.Framework.Rectangle xboxSourceRect)
		{
			return xboxSourceRect;
		}

		public static char getRandomSlotCharacter()
		{
			return getRandomSlotCharacter('o');
		}

		public static List<Vector2> removeDuplicates(List<Vector2> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				for (int num = list.Count - 1; num >= 0; num--)
				{
					if (num != i && list[i].Equals(list[num]))
					{
						list.RemoveAt(num);
					}
				}
			}
			return list;
		}

		public static IEnumerable<int> GetHorseWarpRestrictionsForFarmer(Farmer who)
		{
			if (who.horseName.Value == null)
			{
				yield return 1;
			}
			GameLocation location = who.currentLocation;
			if (!location.IsOutdoors)
			{
				yield return 2;
			}
			Microsoft.Xna.Framework.Rectangle position = new Microsoft.Xna.Framework.Rectangle(who.getTileX() * 64, who.getTileY() * 64, 128, 64);
			if (location.isCollidingPosition(position, Game1.viewport, isFarmer: true, 0, glider: false, who))
			{
				yield return 3;
			}
			foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
			{
				if (onlineFarmer.mount != null && onlineFarmer.mount.getOwner() == who)
				{
					yield return 4;
				}
			}
		}

		public static Microsoft.Xna.Framework.Rectangle ConstrainScissorRectToScreen(Microsoft.Xna.Framework.Rectangle scissor_rect)
		{
			int num = 0;
			if (scissor_rect.Top < 0)
			{
				num = -scissor_rect.Top;
				scissor_rect.Height -= num;
				scissor_rect.Y += num;
			}
			if (scissor_rect.Bottom > Game1.viewport.Height)
			{
				num = scissor_rect.Bottom - Game1.viewport.Height;
				scissor_rect.Height -= num;
			}
			if (scissor_rect.Left < 0)
			{
				num = -scissor_rect.Left;
				scissor_rect.Width -= num;
				scissor_rect.X += num;
			}
			if (scissor_rect.Right > Game1.viewport.Width)
			{
				num = scissor_rect.Right - Game1.viewport.Width;
				scissor_rect.Width -= num;
			}
			return scissor_rect;
		}

		public static void RecordAnimalProduce(FarmAnimal animal, int produce)
		{
			if (animal.type.Contains("Cow"))
			{
				Game1.stats.CowMilkProduced++;
			}
			else if (animal.type.Contains("Sheep"))
			{
				Game1.stats.SheepWoolProduced++;
			}
			else if (animal.type.Contains("Goat"))
			{
				Game1.stats.GoatMilkProduced++;
			}
		}

		public static Point Vector2ToPoint(Vector2 v)
		{
			return new Point((int)v.X, (int)v.Y);
		}

		public static Vector2 PointToVector2(Point p)
		{
			return new Vector2(p.X, p.Y);
		}

		public static int getStartTimeOfFestival()
		{
			if (Game1.weatherIcon == 1)
			{
				return Convert.ToInt32(Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + Game1.dayOfMonth)["conditions"].Split('/')[1].Split(' ')[0]);
			}
			return -1;
		}

		public static bool doesMasterPlayerHaveMailReceivedButNotMailForTomorrow(string mailID)
		{
			if (Game1.MasterPlayer.mailReceived.Contains(mailID) || Game1.MasterPlayer.mailReceived.Contains(mailID + "%&NL&%"))
			{
				if (!Game1.MasterPlayer.mailForTomorrow.Contains(mailID))
				{
					return !Game1.MasterPlayer.mailForTomorrow.Contains(mailID + "%&NL&%");
				}
				return false;
			}
			return false;
		}

		public static bool isFestivalDay(int day, string season)
		{
			string key = season + day;
			return Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\FestivalDates").ContainsKey(key);
		}

		public static void ForAllLocations(Action<GameLocation> action)
		{
			foreach (GameLocation location in Game1.locations)
			{
				action(location);
				if (!(location is BuildableGameLocation))
				{
					continue;
				}
				foreach (Building building in (location as BuildableGameLocation).buildings)
				{
					if (building.indoors.Value != null)
					{
						action(building.indoors.Value);
					}
				}
			}
		}

		public static int getNumObjectsOfIndexWithinRectangle(Microsoft.Xna.Framework.Rectangle r, int[] indexes, GameLocation location)
		{
			int num = 0;
			Vector2 zero = Vector2.Zero;
			for (int i = r.Y; i < r.Bottom + 1; i++)
			{
				zero.Y = i;
				for (int j = r.X; j < r.Right + 1; j++)
				{
					zero.X = j;
					if (!location.objects.ContainsKey(zero))
					{
						continue;
					}
					for (int k = 0; k < indexes.Length; k++)
					{
						if (indexes[k] == (int)location.objects[zero].parentSheetIndex || indexes[k] == -1)
						{
							num++;
							break;
						}
					}
				}
			}
			return num;
		}

		public static string fuzzySearch(string query, List<string> word_bank)
		{
			foreach (string item in word_bank)
			{
				if (query.Trim() == item.Trim())
				{
					return item;
				}
			}
			foreach (string item2 in word_bank)
			{
				if (_formatForFuzzySearch(query) == _formatForFuzzySearch(item2))
				{
					return item2;
				}
			}
			foreach (string item3 in word_bank)
			{
				if (_formatForFuzzySearch(item3).StartsWith(_formatForFuzzySearch(query)))
				{
					return item3;
				}
			}
			foreach (string item4 in word_bank)
			{
				if (_formatForFuzzySearch(item4).Contains(_formatForFuzzySearch(query)))
				{
					return item4;
				}
			}
			return null;
		}

		protected static string _formatForFuzzySearch(string term)
		{
			string text = term.Trim().ToLowerInvariant().Replace(" ", "")
				.Replace("(", "")
				.Replace(")", "")
				.Replace("'", "")
				.Replace(".", "")
				.Replace("!", "")
				.Replace("?", "")
				.Replace("-", "");
			if (text.Length == 0)
			{
				return term.Trim().ToLowerInvariant().Replace(" ", "");
			}
			return text;
		}

		public static Item fuzzyItemSearch(string query, int stack_count = 1)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (int key7 in Game1.objectInformation.Keys)
			{
				string text = Game1.objectInformation[key7];
				if (text == null)
				{
					continue;
				}
				string[] array = text.Split('/');
				string text2 = array[0];
				if (!(text2 == "Stone") || key7 == 390)
				{
					if (array[3] == "Ring")
					{
						dictionary[text2] = "R " + key7 + " " + stack_count;
					}
					else
					{
						dictionary[text2] = "O " + key7 + " " + stack_count;
					}
				}
			}
			foreach (int key8 in Game1.bigCraftablesInformation.Keys)
			{
				string text3 = Game1.bigCraftablesInformation[key8];
				if (text3 != null)
				{
					string key = text3.Substring(0, text3.IndexOf('/'));
					dictionary[key] = "BO " + key8 + " " + stack_count;
				}
			}
			Dictionary<int, string> dictionary2 = Game1.content.Load<Dictionary<int, string>>("Data\\Furniture");
			foreach (int key9 in dictionary2.Keys)
			{
				string text4 = dictionary2[key9];
				if (text4 != null)
				{
					string key2 = text4.Substring(0, text4.IndexOf('/'));
					dictionary[key2] = "F " + key9 + " " + stack_count;
				}
			}
			Dictionary<int, string> dictionary3 = Game1.content.Load<Dictionary<int, string>>("Data\\weapons");
			foreach (int key10 in dictionary3.Keys)
			{
				string text5 = dictionary3[key10];
				if (text5 != null)
				{
					string key3 = text5.Substring(0, text5.IndexOf('/'));
					dictionary[key3] = "W " + key10 + " " + stack_count;
				}
			}
			Dictionary<int, string> dictionary4 = Game1.content.Load<Dictionary<int, string>>("Data\\Boots");
			foreach (int key11 in dictionary4.Keys)
			{
				string text6 = dictionary4[key11];
				if (text6 != null)
				{
					string key4 = text6.Substring(0, text6.IndexOf('/'));
					dictionary[key4] = "B " + key11 + " " + stack_count;
				}
			}
			Dictionary<int, string> dictionary5 = Game1.content.Load<Dictionary<int, string>>("Data\\hats");
			foreach (int key12 in dictionary5.Keys)
			{
				string text7 = dictionary5[key12];
				if (text7 != null)
				{
					string key5 = text7.Substring(0, text7.IndexOf('/'));
					dictionary[key5] = "H " + key12 + " " + stack_count;
				}
			}
			foreach (int key13 in Game1.clothingInformation.Keys)
			{
				string text8 = Game1.clothingInformation[key13];
				if (text8 != null)
				{
					string key6 = text8.Substring(0, text8.IndexOf('/'));
					dictionary[key6] = "C " + key13 + " " + stack_count;
				}
			}
			string text9 = fuzzySearch(query, dictionary.Keys.ToList());
			if (text9 != null)
			{
				return getItemFromStandardTextDescription(dictionary[text9], null);
			}
			return null;
		}

		public static GameLocation fuzzyLocationSearch(string query)
		{
			Dictionary<string, GameLocation> dictionary = new Dictionary<string, GameLocation>();
			foreach (GameLocation location in Game1.locations)
			{
				dictionary[location.NameOrUniqueName] = location;
				if (!(location is BuildableGameLocation))
				{
					continue;
				}
				foreach (Building building in (location as BuildableGameLocation).buildings)
				{
					if (building.indoors.Value != null)
					{
						dictionary[building.indoors.Value.NameOrUniqueName] = building.indoors.Value;
					}
				}
			}
			string text = fuzzySearch(query, dictionary.Keys.ToList());
			if (text != null)
			{
				return dictionary[text];
			}
			return null;
		}

		public static string AOrAn(string text)
		{
			if (text != null && text.Length > 0)
			{
				char c = text.ToLowerInvariant()[0];
				if (c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u')
				{
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.hu)
					{
						return "az";
					}
					return "an";
				}
			}
			return "a";
		}

		public static void getDefaultWarpLocation(string location_name, ref int x, ref int y)
		{
			if (location_name == null)
			{
				return;
			}
			switch (location_name.Length)
			{
			case 12:
				switch (location_name[5])
				{
				case 'e':
					if (location_name == "LeoTreeHouse")
					{
						x = 3;
						y = 4;
					}
					break;
				case 'd':
					if (!(location_name == "IslandShrine"))
					{
						if (location_name == "IslandSecret")
						{
							x = 80;
							y = 68;
						}
					}
					else
					{
						x = 16;
						y = 28;
					}
					break;
				case 't':
					if (location_name == "ElliottHouse")
					{
						x = 3;
						y = 9;
					}
					break;
				case 'i':
					if (location_name == "MermaidHouse")
					{
						x = 4;
						y = 9;
					}
					break;
				case 'T':
					if (location_name == "MovieTheater")
					{
						x = 8;
						y = 9;
					}
					break;
				case 'c':
					if (location_name == "ScienceHouse")
					{
						x = 8;
						y = 20;
					}
					break;
				}
				break;
			case 9:
				switch (location_name[1])
				{
				case 'i':
					if (location_name == "QiNutRoom")
					{
						x = 7;
						y = 7;
					}
					break;
				case 's':
					if (location_name == "IslandHut")
					{
						x = 7;
						y = 11;
					}
					break;
				case 'a':
					if (location_name == "Backwoods")
					{
						x = 18;
						y = 18;
					}
					break;
				case 'o':
					if (location_name == "JoshHouse")
					{
						x = 9;
						y = 20;
					}
					break;
				case 'e':
					if (location_name == "LeahHouse")
					{
						x = 7;
						y = 9;
					}
					break;
				case 'k':
					if (location_name == "SkullCave")
					{
						x = 3;
						y = 4;
					}
					break;
				case 'u':
					if (location_name == "Submarine")
					{
						x = 14;
						y = 14;
					}
					break;
				}
				break;
			case 10:
				switch (location_name[6])
				{
				case 'o':
					switch (location_name)
					{
					case "Greenhouse":
						x = 9;
						y = 8;
						break;
					case "HaleyHouse":
						x = 2;
						y = 23;
						break;
					case "ManorHouse":
						x = 4;
						y = 10;
						break;
					case "SandyHouse":
						x = 2;
						y = 7;
						break;
					}
					break;
				case 'W':
					if (location_name == "IslandWest")
					{
						x = 77;
						y = 40;
					}
					break;
				case 'E':
					if (location_name == "IslandEast")
					{
						x = 21;
						y = 37;
					}
					break;
				case 'S':
					if (location_name == "AnimalShop")
					{
						x = 12;
						y = 16;
					}
					break;
				case 'n':
					if (location_name == "BoatTunnel")
					{
						x = 6;
						y = 11;
					}
					break;
				case 'm':
					if (location_name == "Blacksmith")
					{
						x = 3;
						y = 15;
					}
					break;
				case 'R':
					if (location_name == "HarveyRoom")
					{
						x = 6;
						y = 11;
					}
					break;
				case 'u':
					if (location_name == "SlimeHutch")
					{
						x = 8;
						y = 18;
					}
					break;
				case 'w':
					if (location_name == "WitchSwamp")
					{
						x = 20;
						y = 30;
					}
					break;
				}
				break;
			case 7:
				switch (location_name[2])
				{
				case 'l':
					if (location_name == "Caldera")
					{
						x = 21;
						y = 30;
					}
					break;
				case 'g':
					if (location_name == "BugLand")
					{
						x = 14;
						y = 52;
					}
					break;
				case 's':
					if (location_name == "BusStop")
					{
						x = 14;
						y = 23;
					}
					break;
				case 'n':
					if (location_name == "Sunroom")
					{
						x = 5;
						y = 12;
					}
					break;
				case 'a':
					if (location_name == "Trailer")
					{
						x = 12;
						y = 9;
					}
					break;
				}
				break;
			case 14:
				switch (location_name[0])
				{
				case 'I':
					if (location_name == "IslandFarmCave")
					{
						x = 4;
						y = 10;
					}
					break;
				case 'A':
					if (location_name == "AdventureGuild")
					{
						x = 4;
						y = 13;
					}
					break;
				case 'B':
					if (location_name == "BathHouse_Pool")
					{
						x = 13;
						y = 5;
					}
					break;
				}
				break;
			case 19:
				switch (location_name[0])
				{
				case 'I':
					if (location_name == "IslandSouthEastCave")
					{
						x = 2;
						y = 7;
					}
					break;
				case 'W':
					if (location_name == "WizardHouseBasement")
					{
						x = 4;
						y = 4;
					}
					break;
				}
				break;
			case 15:
				switch (location_name[6])
				{
				case 'F':
					if (location_name == "IslandFarmHouse")
					{
						x = 14;
						y = 17;
					}
					break;
				case 'S':
					if (location_name == "IslandSouthEast")
					{
						x = 0;
						y = 28;
					}
					break;
				case 'u':
					if (location_name == "BathHouse_Entry")
					{
						x = 5;
						y = 8;
					}
					break;
				case 'i':
					if (location_name == "CommunityCenter")
					{
						x = 32;
						y = 13;
					}
					break;
				}
				break;
			case 17:
				switch (location_name[0])
				{
				case 'I':
					if (location_name == "IslandFieldOffice")
					{
						x = 8;
						y = 8;
					}
					break;
				case 'A':
					if (location_name == "AbandonedJojaMart")
					{
						x = 9;
						y = 12;
					}
					break;
				}
				break;
			case 11:
				switch (location_name[6])
				{
				case 'N':
					if (location_name == "IslandNorth")
					{
						x = 36;
						y = 89;
					}
					break;
				case 'S':
					if (location_name == "IslandSouth")
					{
						x = 21;
						y = 43;
					}
					break;
				case 'r':
					if (location_name == "Trailer_Big")
					{
						x = 13;
						y = 23;
					}
					break;
				case 'H':
					if (location_name == "WizardHouse")
					{
						x = 6;
						y = 18;
					}
					break;
				}
				break;
			case 4:
				switch (location_name[3])
				{
				default:
					return;
				case 'n':
					break;
				case 'b':
					if (location_name == "Club")
					{
						x = 8;
						y = 11;
					}
					return;
				case 'p':
					goto IL_066e;
				case 'm':
					if (location_name == "Farm")
					{
						x = 64;
						y = 15;
					}
					return;
				case 'e':
					if (location_name == "Mine")
					{
						x = 13;
						y = 10;
					}
					return;
				case 't':
					if (location_name == "Tent")
					{
						x = 2;
						y = 4;
					}
					return;
				}
				if (!(location_name == "Barn"))
				{
					if (location_name == "Town")
					{
						x = 29;
						y = 67;
					}
					break;
				}
				goto IL_08d5;
			case 5:
				switch (location_name[3])
				{
				default:
					return;
				case 'n':
					break;
				case 'c':
					if (location_name == "Beach")
					{
						x = 39;
						y = 1;
					}
					return;
				case 'p':
					goto IL_06e4;
				case 'e':
					if (location_name == "Sewer")
					{
						x = 31;
						y = 18;
					}
					return;
				case 'd':
					if (location_name == "Woods")
					{
						x = 8;
						y = 9;
					}
					return;
				}
				if (!(location_name == "Barn2") && !(location_name == "Barn3"))
				{
					break;
				}
				goto IL_08d5;
			case 6:
				switch (location_name[0])
				{
				case 'D':
					if (location_name == "Desert")
					{
						x = 35;
						y = 43;
					}
					break;
				case 'F':
					if (location_name == "Forest")
					{
						x = 27;
						y = 12;
					}
					break;
				case 'S':
					if (location_name == "Saloon")
					{
						x = 18;
						y = 20;
					}
					break;
				case 'T':
					if (location_name == "Tunnel")
					{
						x = 17;
						y = 7;
					}
					break;
				}
				break;
			case 8:
				switch (location_name[3])
				{
				case 'h':
					if (location_name == "FishShop")
					{
						x = 6;
						y = 6;
					}
					break;
				case 'p':
					if (location_name == "Hospital")
					{
						x = 10;
						y = 18;
					}
					break;
				case 'a':
					if (location_name == "JojaMart")
					{
						x = 13;
						y = 28;
					}
					break;
				case 'n':
					if (location_name == "Mountain")
					{
						x = 40;
						y = 13;
					}
					break;
				case 'l':
					if (location_name == "Railroad")
					{
						x = 29;
						y = 58;
					}
					break;
				case 'H':
					if (location_name == "SamHouse")
					{
						x = 4;
						y = 15;
					}
					break;
				case 'd':
					if (location_name == "SeedShop")
					{
						x = 4;
						y = 19;
					}
					break;
				case 'c':
					if (location_name == "WitchHut")
					{
						x = 7;
						y = 14;
					}
					break;
				}
				break;
			case 16:
				if (location_name == "ArchaeologyHouse")
				{
					x = 3;
					y = 10;
				}
				break;
			case 20:
				if (location_name == "BathHouse_MensLocker")
				{
					x = 15;
					y = 16;
				}
				break;
			case 22:
				if (location_name == "BathHouse_WomensLocker")
				{
					x = 2;
					y = 14;
				}
				break;
			case 13:
				if (location_name == "WitchWarpCave")
				{
					x = 4;
					y = 8;
				}
				break;
			case 18:
			case 21:
				break;
				IL_08d5:
				x = 11;
				y = 13;
				break;
				IL_06e4:
				if (!(location_name == "Coop2") && !(location_name == "Coop3"))
				{
					break;
				}
				goto IL_0939;
				IL_066e:
				if (!(location_name == "Coop"))
				{
					break;
				}
				goto IL_0939;
				IL_0939:
				x = 2;
				y = 8;
				break;
			}
		}

		public static FarmAnimal fuzzyAnimalSearch(string query)
		{
			List<FarmAnimal> list = new List<FarmAnimal>();
			foreach (GameLocation location in Game1.locations)
			{
				if (location is IAnimalLocation)
				{
					list.AddRange((location as IAnimalLocation).Animals.Values);
				}
				if (!(location is BuildableGameLocation))
				{
					continue;
				}
				foreach (Building building in (location as BuildableGameLocation).buildings)
				{
					if (building.indoors.Value != null && building.indoors.Value is IAnimalLocation)
					{
						list.AddRange((building.indoors.Value as IAnimalLocation).Animals.Values);
					}
				}
			}
			Dictionary<string, FarmAnimal> dictionary = new Dictionary<string, FarmAnimal>();
			foreach (FarmAnimal item in list)
			{
				dictionary[item.Name] = item;
			}
			string text = fuzzySearch(query, dictionary.Keys.ToList());
			if (text != null)
			{
				return dictionary[text];
			}
			return null;
		}

		public static NPC fuzzyCharacterSearch(string query, bool must_be_villager = true)
		{
			List<NPC> list = new List<NPC>();
			getAllCharacters(list);
			Dictionary<string, NPC> dictionary = new Dictionary<string, NPC>();
			foreach (NPC item in list)
			{
				if (!must_be_villager || item.isVillager())
				{
					dictionary[item.Name] = item;
				}
			}
			string text = fuzzySearch(query, dictionary.Keys.ToList());
			if (text != null)
			{
				return dictionary[text];
			}
			return null;
		}

		public static Color GetPrismaticColor(int offset = 0, float speedMultiplier = 1f)
		{
			float num = 1500f;
			int num2 = ((int)((float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds * speedMultiplier / num) + offset) % PRISMATIC_COLORS.Length;
			int num3 = (num2 + 1) % PRISMATIC_COLORS.Length;
			float t = (float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds * speedMultiplier / num % 1f;
			Color result = default(Color);
			result.R = (byte)(Lerp((float)(int)PRISMATIC_COLORS[num2].R / 255f, (float)(int)PRISMATIC_COLORS[num3].R / 255f, t) * 255f);
			result.G = (byte)(Lerp((float)(int)PRISMATIC_COLORS[num2].G / 255f, (float)(int)PRISMATIC_COLORS[num3].G / 255f, t) * 255f);
			result.B = (byte)(Lerp((float)(int)PRISMATIC_COLORS[num2].B / 255f, (float)(int)PRISMATIC_COLORS[num3].B / 255f, t) * 255f);
			result.A = (byte)(Lerp((float)(int)PRISMATIC_COLORS[num2].A / 255f, (float)(int)PRISMATIC_COLORS[num3].A / 255f, t) * 255f);
			return result;
		}

		public static Color Get2PhaseColor(Color color1, Color color2, int offset = 0, float speedMultiplier = 1f, float timeOffset = 0f)
		{
			float num = 1500f;
			int num2 = ((int)((float)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)timeOffset) * speedMultiplier / num) + offset) % 2;
			int num3 = (num2 + 1) % 2;
			float t = (float)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)timeOffset) * speedMultiplier / num % 1f;
			Color result = default(Color);
			Color color3 = ((num2 == 0) ? color1 : color2);
			Color color4 = ((num2 == 0) ? color2 : color1);
			result.R = (byte)(Lerp((float)(int)color3.R / 255f, (float)(int)color4.R / 255f, t) * 255f);
			result.G = (byte)(Lerp((float)(int)color3.G / 255f, (float)(int)color4.G / 255f, t) * 255f);
			result.B = (byte)(Lerp((float)(int)color3.B / 255f, (float)(int)color4.B / 255f, t) * 255f);
			result.A = (byte)(Lerp((float)(int)color3.A / 255f, (float)(int)color4.A / 255f, t) * 255f);
			return result;
		}

		public static bool IsNormalObjectAtParentSheetIndex(Item item, int index)
		{
			if (item == null)
			{
				return false;
			}
			if (item.GetType() != typeof(Object))
			{
				return false;
			}
			Object @object = item as Object;
			if (@object.bigCraftable.Value)
			{
				return false;
			}
			if (item.ParentSheetIndex != index)
			{
				return false;
			}
			return true;
		}

		public static bool isObjectOffLimitsForSale(int index)
		{
			switch (index)
			{
			case 69:
			case 73:
			case 79:
			case 91:
			case 158:
			case 159:
			case 160:
			case 161:
			case 162:
			case 163:
			case 261:
			case 277:
			case 279:
			case 289:
			case 292:
			case 305:
			case 308:
			case 326:
			case 341:
			case 413:
			case 417:
			case 437:
			case 439:
			case 447:
			case 454:
			case 460:
			case 645:
			case 680:
			case 681:
			case 682:
			case 688:
			case 689:
			case 690:
			case 774:
			case 775:
			case 797:
			case 798:
			case 799:
			case 800:
			case 801:
			case 802:
			case 803:
			case 807:
			case 812:
				return true;
			default:
				return false;
			}
		}

		public static Microsoft.Xna.Framework.Rectangle xTileToMicrosoftRectangle(xTile.Dimensions.Rectangle rect)
		{
			return new Microsoft.Xna.Framework.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static Microsoft.Xna.Framework.Rectangle getSafeArea()
		{
			Viewport viewport = Game1.game1.GraphicsDevice.Viewport;
			Microsoft.Xna.Framework.Rectangle titleSafeArea = viewport.GetTitleSafeArea();
			if (Game1.game1.GraphicsDevice.GetRenderTargets().Length == 0)
			{
				float num = 1f / Game1.options.zoomLevel;
				if (Game1.uiMode)
				{
					num = 1f / Game1.options.uiScale;
				}
				titleSafeArea.X = (int)((float)titleSafeArea.X * num);
				titleSafeArea.Y = (int)((float)titleSafeArea.Y * num);
				titleSafeArea.Width = (int)((float)titleSafeArea.Width * num);
				titleSafeArea.Height = (int)((float)titleSafeArea.Height * num);
			}
			return titleSafeArea;
		}

		public static Vector2 makeSafe(Vector2 renderPos, Vector2 renderSize)
		{
			Microsoft.Xna.Framework.Rectangle safeArea = getSafeArea();
			int x = (int)renderPos.X;
			int y = (int)renderPos.Y;
			int width = (int)renderSize.X;
			int height = (int)renderSize.Y;
			makeSafe(ref x, ref y, width, height);
			return new Vector2(x, y);
		}

		public static void makeSafe(ref Vector2 position, int width, int height)
		{
			int x = (int)position.X;
			int y = (int)position.Y;
			makeSafe(ref x, ref y, width, height);
			position.X = x;
			position.Y = y;
		}

		public static void makeSafe(ref Microsoft.Xna.Framework.Rectangle bounds)
		{
			makeSafe(ref bounds.X, ref bounds.Y, bounds.Width, bounds.Height);
		}

		public static void makeSafe(ref int x, ref int y, int width, int height)
		{
			Microsoft.Xna.Framework.Rectangle safeArea = getSafeArea();
			if (x < safeArea.Left)
			{
				x = safeArea.Left;
			}
			if (y < safeArea.Top)
			{
				y = safeArea.Top;
			}
			if (x + width > safeArea.Right)
			{
				x = safeArea.Right - width;
			}
			if (y + height > safeArea.Bottom)
			{
				y = safeArea.Bottom - height;
			}
		}

		internal static void makeSafeY(ref int y, int height)
		{
			Vector2 renderPos = new Vector2(0f, y);
			Vector2 renderSize = new Vector2(0f, height);
			y = (int)makeSafe(renderPos, renderSize).Y;
		}

		public static int makeSafeMarginX(int marginx)
		{
			Viewport viewport = Game1.game1.GraphicsDevice.Viewport;
			Microsoft.Xna.Framework.Rectangle safeArea = getSafeArea();
			if (safeArea.Left > viewport.Bounds.Left)
			{
				marginx = safeArea.Left;
			}
			int num = safeArea.Right - viewport.Bounds.Right;
			if (num > marginx)
			{
				marginx = num;
			}
			return marginx;
		}

		public static int makeSafeMarginY(int marginy)
		{
			Viewport viewport = Game1.game1.GraphicsDevice.Viewport;
			Microsoft.Xna.Framework.Rectangle safeArea = getSafeArea();
			int num = safeArea.Top - viewport.Bounds.Top;
			if (num > marginy)
			{
				marginy = num;
			}
			num = viewport.Bounds.Bottom - safeArea.Bottom;
			if (num > marginy)
			{
				marginy = num;
			}
			return marginy;
		}

		public static bool onTravelingMerchantShopPurchase(ISalable item, Farmer farmer, int amount)
		{
			SynchronizedShopStock synchronizedShopStock = Game1.player.team.synchronizedShopStock;
			synchronizedShopStock.OnItemPurchased(SynchronizedShopStock.SynchedShop.TravelingMerchant, item, amount);
			return false;
		}

		private static Dictionary<ISalable, int[]> generateLocalTravelingMerchantStock(int seed)
		{
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			HashSet<int> stockIndices = new HashSet<int>();
			Random random = new Random(seed);
			bool flag = false;
			if (Game1.netWorldState.Value.VisitsUntilY1Guarantee == 0)
			{
				flag = true;
			}
			for (int i = 0; i < 10; i++)
			{
				int num = random.Next(2, 790);
				while (true)
				{
					num++;
					num %= 790;
					if (Game1.objectInformation.ContainsKey(num) && !isObjectOffLimitsForSale(num))
					{
						if (num == 266 || num == 485)
						{
							flag = false;
						}
						string[] array = Game1.objectInformation[num].Split('/');
						if (array[3].Contains('-') && Convert.ToInt32(array[1]) > 0 && !array[3].Contains("-13") && !array[3].Equals("Quest") && !array[0].Equals("Weeds") && !array[3].Contains("Minerals") && !array[3].Contains("Arch") && addToStock(dictionary, stockIndices, new Object(num, 1), new int[2]
						{
							Math.Max(random.Next(1, 11) * 100, Convert.ToInt32(array[1]) * random.Next(3, 6)),
							(!(random.NextDouble() < 0.1)) ? 1 : 5
						}))
						{
							break;
						}
					}
				}
			}
			if (flag)
			{
				string[] array2 = Game1.objectInformation[485].Split('/');
				addToStock(dictionary, stockIndices, new Object(485, 1), new int[2]
				{
					Math.Max(random.Next(1, 11) * 100, Convert.ToInt32(array2[1]) * random.Next(3, 6)),
					(!(random.NextDouble() < 0.1)) ? 1 : 5
				});
			}
			addToStock(dictionary, stockIndices, getRandomFurniture(random, null, 0, 1613), new int[2]
			{
				random.Next(1, 11) * 250,
				1
			});
			if (getSeasonNumber(Game1.currentSeason) < 2)
			{
				addToStock(dictionary, stockIndices, new Object(347, 1), new int[2]
				{
					1000,
					(!(random.NextDouble() < 0.1)) ? 1 : 5
				});
			}
			else if (random.NextDouble() < 0.4)
			{
				addToStock(dictionary, stockIndices, new Object(Vector2.Zero, 136), new int[2] { 4000, 1 });
			}
			if (random.NextDouble() < 0.25)
			{
				addToStock(dictionary, stockIndices, new Object(433, 1), new int[2] { 2500, 1 });
			}
			return dictionary;
		}

		public static Dictionary<ISalable, int[]> getTravelingMerchantStock(int seed)
		{
			Dictionary<ISalable, int[]> dictionary = generateLocalTravelingMerchantStock(seed);
			SynchronizedShopStock synchronizedShopStock = Game1.player.team.synchronizedShopStock;
			synchronizedShopStock.UpdateLocalStockWithSyncedQuanitities(SynchronizedShopStock.SynchedShop.TravelingMerchant, dictionary);
			if (Game1.IsMultiplayer && !Game1.player.craftingRecipes.ContainsKey("Wedding Ring"))
			{
				Object key = new Object(801, 1, isRecipe: true);
				dictionary.Add(key, new int[2] { 500, 1 });
			}
			return dictionary;
		}

		private static bool addToStock(Dictionary<ISalable, int[]> stock, HashSet<int> stockIndices, Object objectToAdd, int[] listing)
		{
			int parentSheetIndex = objectToAdd.ParentSheetIndex;
			if (!stockIndices.Contains(parentSheetIndex))
			{
				stock.Add(objectToAdd, listing);
				stockIndices.Add(parentSheetIndex);
				return true;
			}
			return false;
		}

		public static Dictionary<ISalable, int[]> getDwarfShopStock()
		{
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			dictionary.Add(new Object(773, 1), new int[2] { 2000, 2147483647 });
			dictionary.Add(new Object(772, 1), new int[2] { 3000, 2147483647 });
			dictionary.Add(new Object(286, 1), new int[2] { 300, 2147483647 });
			dictionary.Add(new Object(287, 1), new int[2] { 600, 2147483647 });
			dictionary.Add(new Object(288, 1), new int[2] { 1000, 2147483647 });
			dictionary.Add(new Object(243, 1), new int[2] { 1000, 2147483647 });
			dictionary.Add(new Object(Vector2.Zero, 138), new int[2] { 2500, 2147483647 });
			dictionary.Add(new Object(Vector2.Zero, 32), new int[2] { 200, 2147483647 });
			if (!Game1.player.craftingRecipes.ContainsKey("Weathered Floor"))
			{
				dictionary.Add(new Object(331, 1, isRecipe: true), new int[2] { 500, 1 });
			}
			return dictionary;
		}

		public static Dictionary<ISalable, int[]> getHospitalStock()
		{
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			dictionary.Add(new Object(349, 1), new int[2] { 1000, 2147483647 });
			dictionary.Add(new Object(351, 1), new int[2] { 1000, 2147483647 });
			return dictionary;
		}

		public static int CompareGameVersions(string version, string other_version, bool ignore_platform_specific = false, bool major_only = false)
		{
			string[] array = version.Split('.');
			string[] array2 = other_version.Split('.');
			if (major_only && array[0] == array2[0] && array[1] == array2[1])
			{
				return 0;
			}
			for (int i = 0; i < Math.Max(array.Length, array2.Length); i++)
			{
				float result = 0f;
				float result2 = 0f;
				if (i < array.Length)
				{
					float.TryParse(array[i], out result);
				}
				if (i < array2.Length)
				{
					float.TryParse(array2[i], out result2);
				}
				if (result != result2 || (i == 2 && ignore_platform_specific))
				{
					return result.CompareTo(result2);
				}
			}
			return 0;
		}

		public static float getFarmerItemsShippedPercent(Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			int num = 0;
			int num2 = 0;
			foreach (KeyValuePair<int, string> item in Game1.objectInformation)
			{
				string text = item.Value.Split('/')[3];
				if (!text.Contains("Arch") && !text.Contains("Fish") && !text.Contains("Mineral") && !text.Substring(text.Length - 3).Equals("-2") && !text.Contains("Cooking") && !text.Substring(text.Length - 3).Equals("-7") && Object.isPotentialBasicShippedCategory(item.Key, text.Substring(text.Length - 3)))
				{
					num2++;
					if (who.basicShipped.ContainsKey(item.Key))
					{
						num++;
					}
				}
			}
			return (float)num / (float)num2;
		}

		public static bool hasFarmerShippedAllItems()
		{
			return getFarmerItemsShippedPercent() >= 1f;
		}

		public static Dictionary<ISalable, int[]> getQiShopStock()
		{
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			dictionary.Add(new Furniture(1552, Vector2.Zero), new int[2] { 5000, 2147483647 });
			dictionary.Add(new Furniture(1545, Vector2.Zero), new int[2] { 4000, 2147483647 });
			dictionary.Add(new Furniture(1563, Vector2.Zero), new int[2] { 4000, 2147483647 });
			dictionary.Add(new Furniture(1561, Vector2.Zero), new int[2] { 3000, 2147483647 });
			dictionary.Add(new Hat(2), new int[2] { 8000, 2147483647 });
			dictionary.Add(new Object(Vector2.Zero, 126), new int[2] { 10000, 2147483647 });
			dictionary.Add(new Object(298, 1), new int[2] { 100, 2147483647 });
			dictionary.Add(new Object(703, 1), new int[2] { 1000, 2147483647 });
			dictionary.Add(new Object(688, 1), new int[2] { 500, 2147483647 });
			dictionary.Add(new BedFurniture(2192, Vector2.Zero), new int[2] { 8000, 2147483647 });
			return dictionary;
		}

		public static Dictionary<ISalable, int[]> getJojaStock()
		{
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			if (Game1.MasterPlayer.eventsSeen.Contains(502261))
			{
				dictionary.Add(new Object(Vector2.Zero, 272), new int[2] { 50000, 2147483647 });
			}
			dictionary.Add(new Object(Vector2.Zero, 167, 2147483647), new int[2] { 75, 2147483647 });
			dictionary.Add(new Wallpaper(21)
			{
				Stack = 2147483647
			}, new int[2] { 20, 2147483647 });
			dictionary.Add(new Furniture(1609, Vector2.Zero)
			{
				Stack = 2147483647
			}, new int[2] { 500, 2147483647 });
			float num = (Game1.player.hasOrWillReceiveMail("JojaMember") ? 2f : 2.5f);
			num *= Game1.MasterPlayer.difficultyModifier;
			if (Game1.currentSeason.Equals("spring"))
			{
				dictionary.Add(new Object(Vector2.Zero, 472, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[472].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 473, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[473].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 474, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[474].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 475, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[475].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 427, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[427].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 429, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[429].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 477, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[477].Split('/')[1]) * num),
					2147483647
				});
			}
			if (Game1.currentSeason.Equals("summer"))
			{
				dictionary.Add(new Object(Vector2.Zero, 480, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[480].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 482, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[482].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 483, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[483].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 484, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[484].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 479, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[479].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 302, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[302].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 453, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[453].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 455, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[455].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(431, 2147483647, isRecipe: false, 100), new int[2]
				{
					(int)(50f * num),
					2147483647
				});
			}
			if (Game1.currentSeason.Equals("fall"))
			{
				dictionary.Add(new Object(Vector2.Zero, 487, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[487].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 488, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[488].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 483, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[483].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 490, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[490].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 299, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[299].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 301, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[301].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 492, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[492].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 491, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[491].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 493, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[493].Split('/')[1]) * num),
					2147483647
				});
				dictionary.Add(new Object(431, 2147483647, isRecipe: false, 100), new int[2]
				{
					(int)(50f * num),
					2147483647
				});
				dictionary.Add(new Object(Vector2.Zero, 425, 2147483647), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[425].Split('/')[1]) * num),
					2147483647
				});
			}
			dictionary.Add(new Object(Vector2.Zero, 297, 2147483647), new int[2]
			{
				(int)((float)Convert.ToInt32(Game1.objectInformation[297].Split('/')[1]) * num),
				2147483647
			});
			dictionary.Add(new Object(Vector2.Zero, 245, 2147483647), new int[2]
			{
				(int)((float)Convert.ToInt32(Game1.objectInformation[245].Split('/')[1]) * num),
				2147483647
			});
			dictionary.Add(new Object(Vector2.Zero, 246, 2147483647), new int[2]
			{
				(int)((float)Convert.ToInt32(Game1.objectInformation[246].Split('/')[1]) * num),
				2147483647
			});
			dictionary.Add(new Object(Vector2.Zero, 423, 2147483647), new int[2]
			{
				(int)((float)Convert.ToInt32(Game1.objectInformation[423].Split('/')[1]) * num),
				2147483647
			});
			Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + 1);
			int num2 = random.Next(112);
			if (num2 == 21)
			{
				num2 = 22;
			}
			dictionary.Add(new Wallpaper(num2)
			{
				Stack = 2147483647
			}, new int[2] { 250, 2147483647 });
			dictionary.Add(new Wallpaper(random.Next(40), isFloor: true)
			{
				Stack = 2147483647
			}, new int[2] { 250, 2147483647 });
			return dictionary;
		}

		public static Dictionary<ISalable, int[]> getHatStock()
		{
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			Dictionary<int, string> dictionary2 = Game1.content.Load<Dictionary<int, string>>("Data\\Achievements");
			bool flag = true;
			foreach (KeyValuePair<int, string> item in dictionary2)
			{
				if (Game1.player.achievements.Contains(item.Key))
				{
					dictionary.Add(new Hat(Convert.ToInt32(item.Value.Split('^')[4])), new int[2] { 1000, 2147483647 });
				}
				else
				{
					flag = false;
				}
			}
			if (Game1.player.mailReceived.Contains("Egg Festival"))
			{
				dictionary.Add(new Hat(4), new int[2] { 1000, 2147483647 });
			}
			if (Game1.player.mailReceived.Contains("Ice Festival"))
			{
				dictionary.Add(new Hat(17), new int[2] { 1000, 2147483647 });
			}
			if (Game1.player.achievements.Contains(17))
			{
				dictionary.Add(new Hat(61), new int[2] { 1000, 2147483647 });
			}
			if (flag)
			{
				dictionary.Add(new Hat(64), new int[2] { 1000, 2147483647 });
			}
			return dictionary;
		}

		public static NPC getTodaysBirthdayNPC(string season, int day)
		{
			foreach (NPC allCharacter in getAllCharacters())
			{
				if (allCharacter.isBirthday(season, day))
				{
					return allCharacter;
				}
			}
			return null;
		}

		public static bool highlightEdibleItems(Item i)
		{
			if (i is Object)
			{
				return (int)(i as Object).edibility != -300;
			}
			return false;
		}

		public static T GetRandom<T>(List<T> list, Random random = null)
		{
			if (list == null || list.Count == 0)
			{
				return default(T);
			}
			if (random == null)
			{
				random = Game1.random;
			}
			return list[random.Next(list.Count)];
		}

		public static int getRandomSingleTileFurniture(Random r)
		{
			return r.Next(3) switch
			{
				0 => r.Next(10) * 3, 
				1 => r.Next(1376, 1391), 
				2 => r.Next(7) * 2 + 1391, 
				_ => 0, 
			};
		}

		public static void improveFriendshipWithEveryoneInRegion(Farmer who, int amount, int region)
		{
			foreach (GameLocation location in Game1.locations)
			{
				foreach (NPC character in location.characters)
				{
					if (character.homeRegion == region && who.friendshipData.ContainsKey(character.Name))
					{
						who.changeFriendship(amount, character);
					}
				}
			}
		}

		public static Item getGiftFromNPC(NPC who)
		{
			Random random = new Random((int)Game1.uniqueIDForThisGame / 2 + Game1.year + Game1.dayOfMonth + getSeasonNumber(Game1.currentSeason) + who.getTileX());
			List<Item> list = new List<Item>();
			switch (who.Name)
			{
			case "Clint":
				list.Add(new Object(337, 1));
				list.Add(new Object(336, 5));
				list.Add(new Object(random.Next(535, 538), 5));
				break;
			case "Marnie":
				list.Add(new Object(176, 12));
				break;
			case "Robin":
				list.Add(new Object(388, 99));
				list.Add(new Object(390, 50));
				list.Add(new Object(709, 25));
				break;
			case "Willy":
				list.Add(new Object(690, 25));
				list.Add(new Object(687, 1));
				list.Add(new Object(703, 1));
				break;
			case "Evelyn":
				list.Add(new Object(223, 1));
				break;
			default:
			{
				int age = who.Age;
				if (age == 2)
				{
					list.Add(new Object(330, 1));
					list.Add(new Object(103, 1));
					list.Add(new Object(394, 1));
					list.Add(new Object(random.Next(535, 538), 1));
					break;
				}
				list.Add(new Object(608, 1));
				list.Add(new Object(651, 1));
				list.Add(new Object(611, 1));
				list.Add(new Ring(517));
				list.Add(new Object(466, 10));
				list.Add(new Object(422, 1));
				list.Add(new Object(392, 1));
				list.Add(new Object(348, 1));
				list.Add(new Object(346, 1));
				list.Add(new Object(341, 1));
				list.Add(new Object(221, 1));
				list.Add(new Object(64, 1));
				list.Add(new Object(60, 1));
				list.Add(new Object(70, 1));
				break;
			}
			}
			return list[random.Next(list.Count)];
		}

		public static NPC getTopRomanticInterest(Farmer who)
		{
			NPC result = null;
			int num = -1;
			foreach (NPC allCharacter in getAllCharacters())
			{
				if (who.friendshipData.ContainsKey(allCharacter.Name) && (bool)allCharacter.datable && who.getFriendshipLevelForNPC(allCharacter.Name) > num)
				{
					result = allCharacter;
					num = who.getFriendshipLevelForNPC(allCharacter.Name);
				}
			}
			return result;
		}

		public static Color getRandomRainbowColor(Random r = null)
		{
			return (r?.Next(8) ?? Game1.random.Next(8)) switch
			{
				0 => Color.Red, 
				1 => Color.Orange, 
				2 => Color.Yellow, 
				3 => Color.Lime, 
				4 => Color.Cyan, 
				5 => new Color(0, 100, 255), 
				6 => new Color(152, 96, 255), 
				7 => new Color(255, 100, 255), 
				_ => Color.White, 
			};
		}

		public static NPC getTopNonRomanticInterest(Farmer who)
		{
			NPC result = null;
			int num = -1;
			foreach (NPC allCharacter in getAllCharacters())
			{
				if (who.friendshipData.ContainsKey(allCharacter.Name) && !allCharacter.datable && who.getFriendshipLevelForNPC(allCharacter.Name) > num)
				{
					result = allCharacter;
					num = who.getFriendshipLevelForNPC(allCharacter.Name);
				}
			}
			return result;
		}

		public static int getHighestSkill(Farmer who)
		{
			int num = 0;
			int result = 0;
			for (int i = 0; i < who.experiencePoints.Length; i++)
			{
				if (who.experiencePoints[i] > num)
				{
					result = i;
				}
			}
			return result;
		}

		public static int getNumberOfFriendsWithinThisRange(Farmer who, int minFriendshipPoints, int maxFriendshipPoints, bool romanceOnly = false)
		{
			int num = 0;
			foreach (NPC allCharacter in getAllCharacters())
			{
				int? num2 = who.tryGetFriendshipLevelForNPC(allCharacter.Name);
				if (num2.HasValue && num2.Value >= minFriendshipPoints && num2.Value <= maxFriendshipPoints && (!romanceOnly || (bool)allCharacter.datable))
				{
					num++;
				}
			}
			return num;
		}

		public static bool highlightLuauSoupItems(Item i)
		{
			if (i is Object)
			{
				if (((int)(i as Object).edibility == -300 || (i as Object).Category == -7) && !IsNormalObjectAtParentSheetIndex(i, 789))
				{
					return IsNormalObjectAtParentSheetIndex(i, 71);
				}
				return true;
			}
			return false;
		}

		public static bool highlightEdibleNonCookingItems(Item i)
		{
			if (i is Object && (int)(i as Object).edibility != -300)
			{
				return (i as Object).Category != -7;
			}
			return false;
		}

		public static bool highlightSmallObjects(Item i)
		{
			if (i is Object)
			{
				return !(i as Object).bigCraftable;
			}
			return false;
		}

		public static bool highlightSantaObjects(Item i)
		{
			if (!i.canBeTrashed() || !i.canBeGivenAsGift())
			{
				return false;
			}
			return highlightSmallObjects(i);
		}

		public static bool highlightShippableObjects(Item i)
		{
			if (i is Object)
			{
				return (i as Object).canBeShipped();
			}
			return false;
		}

		public static Farmer getFarmerFromFarmerNumberString(string s, Farmer defaultFarmer)
		{
			if (s.Equals("farmer"))
			{
				return defaultFarmer;
			}
			if (s.StartsWith("farmer"))
			{
				return getFarmerFromFarmerNumber(Convert.ToInt32(s[s.Length - 1].ToString() ?? ""));
			}
			return defaultFarmer;
		}

		public static int getFarmerNumberFromFarmer(Farmer who)
		{
			for (int i = 1; i <= Game1.CurrentPlayerLimit; i++)
			{
				if (getFarmerFromFarmerNumber(i).UniqueMultiplayerID == who.UniqueMultiplayerID)
				{
					return i;
				}
			}
			return -1;
		}

		public static Farmer getFarmerFromFarmerNumber(int number)
		{
			if (!Game1.IsMultiplayer)
			{
				if (number == 1)
				{
					return Game1.player;
				}
				return null;
			}
			if (number <= 1 && Game1.serverHost != null)
			{
				return Game1.serverHost;
			}
			if (number <= Game1.numberOfPlayers())
			{
				IEnumerable<Farmer> source = from f in Game1.otherFarmers.Values
					where f != Game1.serverHost.Value
					orderby f.UniqueMultiplayerID
					select f;
				return source.ElementAt(number - 2);
			}
			return null;
		}

		public static string getLoveInterest(string who)
		{
			if (who != null)
			{
				switch (who.Length)
				{
				case 5:
					switch (who[0])
					{
					case 'H':
						if (!(who == "Haley"))
						{
							break;
						}
						return "Alex";
					case 'P':
						if (!(who == "Penny"))
						{
							break;
						}
						return "Sam";
					case 'E':
						if (!(who == "Emily"))
						{
							break;
						}
						return "Shane";
					case 'S':
						if (!(who == "Shane"))
						{
							break;
						}
						return "Emily";
					}
					break;
				case 4:
					switch (who[0])
					{
					case 'A':
						if (!(who == "Alex"))
						{
							break;
						}
						return "Haley";
					case 'L':
						if (!(who == "Leah"))
						{
							break;
						}
						return "Elliott";
					case 'M':
						if (!(who == "Maru"))
						{
							break;
						}
						return "Harvey";
					}
					break;
				case 7:
					switch (who[0])
					{
					case 'E':
						if (!(who == "Elliott"))
						{
							break;
						}
						return "Leah";
					case 'A':
						if (!(who == "Abigail"))
						{
							break;
						}
						return "Sebastian";
					}
					break;
				case 3:
					if (!(who == "Sam"))
					{
						break;
					}
					return "Penny";
				case 6:
					if (!(who == "Harvey"))
					{
						break;
					}
					return "Maru";
				case 9:
					if (!(who == "Sebastian"))
					{
						break;
					}
					return "Abigail";
				}
			}
			return "";
		}

		public static Dictionary<ISalable, int[]> getFishShopStock(Farmer who)
		{
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			dictionary.Add(new Object(219, 1), new int[2] { 250, 2147483647 });
			if ((int)Game1.player.fishingLevel >= 2)
			{
				dictionary.Add(new Object(685, 1), new int[2] { 5, 2147483647 });
			}
			if ((int)Game1.player.fishingLevel >= 3)
			{
				dictionary.Add(new Object(710, 1), new int[2] { 1500, 2147483647 });
			}
			if ((int)Game1.player.fishingLevel >= 6)
			{
				dictionary.Add(new Object(686, 1), new int[2] { 500, 2147483647 });
				dictionary.Add(new Object(694, 1), new int[2] { 500, 2147483647 });
				dictionary.Add(new Object(692, 1), new int[2] { 200, 2147483647 });
			}
			if ((int)Game1.player.fishingLevel >= 7)
			{
				dictionary.Add(new Object(693, 1), new int[2] { 750, 2147483647 });
				dictionary.Add(new Object(695, 1), new int[2] { 750, 2147483647 });
			}
			if ((int)Game1.player.fishingLevel >= 8)
			{
				dictionary.Add(new Object(691, 1), new int[2] { 1000, 2147483647 });
				dictionary.Add(new Object(687, 1), new int[2] { 1000, 2147483647 });
			}
			if ((int)Game1.player.fishingLevel >= 9)
			{
				dictionary.Add(new Object(703, 1), new int[2] { 1000, 2147483647 });
			}
			dictionary.Add(new FishingRod(0), new int[2] { 500, 2147483647 });
			dictionary.Add(new FishingRod(1), new int[2] { 25, 2147483647 });
			if ((int)Game1.player.fishingLevel >= 2)
			{
				dictionary.Add(new FishingRod(2), new int[2] { 1800, 2147483647 });
			}
			if ((int)Game1.player.fishingLevel >= 6)
			{
				dictionary.Add(new FishingRod(3), new int[2] { 7500, 2147483647 });
			}
			if (Game1.MasterPlayer.mailReceived.Contains("ccFishTank"))
			{
				dictionary.Add(new Pan(), new int[2] { 2500, 2147483647 });
			}
			dictionary.Add(new FishTankFurniture(2304, Vector2.Zero), new int[2] { 2000, 2147483647 });
			dictionary.Add(new FishTankFurniture(2322, Vector2.Zero), new int[2] { 500, 2147483647 });
			if (Game1.player.mailReceived.Contains("WillyTropicalFish"))
			{
				dictionary.Add(new FishTankFurniture(2312, Vector2.Zero), new int[2] { 5000, 2147483647 });
			}
			dictionary.Add(new BedFurniture(2502, Vector2.Zero), new int[2] { 25000, 2147483647 });
			GameLocation locationFromName = Game1.getLocationFromName("FishShop");
			if (locationFromName is ShopLocation)
			{
				foreach (Item item in (locationFromName as ShopLocation).itemsFromPlayerToSell)
				{
					if (item.Stack > 0)
					{
						int num = item.salePrice();
						if (item is Object)
						{
							num = (item as Object).sellToStorePrice(-1L);
						}
						dictionary.Add(item, new int[2] { num, item.Stack });
					}
				}
				return dictionary;
			}
			return dictionary;
		}

		public static string ParseGiftReveals(string str)
		{
			try
			{
				while (str.Contains("%revealtaste"))
				{
					int num = str.IndexOf("%revealtaste");
					int num2 = num + "%revealtaste".Length;
					int i = num + 1;
					if (i >= str.Length)
					{
						i = str.Length - 1;
					}
					for (; i < str.Length && (str[i] < '0' || str[i] > '9'); i++)
					{
					}
					string name = str.Substring(num2, i - num2);
					num2 = i;
					for (; i < str.Length && str[i] >= '0' && str[i] <= '9'; i++)
					{
					}
					string s = str.Substring(num2, i - num2);
					int parent_sheet_index = int.Parse(s);
					str = str.Remove(num, i - num);
					NPC characterFromName = Game1.getCharacterFromName(name);
					if (characterFromName != null)
					{
						Game1.player.revealGiftTaste(characterFromName, parent_sheet_index);
					}
				}
				return str;
			}
			catch (Exception)
			{
				return str;
			}
		}

		public static void Shuffle<T>(Random rng, List<T> list)
		{
			int count = list.Count;
			while (count > 1)
			{
				int index = rng.Next(count--);
				T value = list[count];
				list[count] = list[index];
				list[index] = value;
			}
		}

		public static void Shuffle<T>(Random rng, T[] array)
		{
			int num = array.Length;
			while (num > 1)
			{
				int num2 = rng.Next(num--);
				T val = array[num];
				array[num] = array[num2];
				array[num2] = val;
			}
		}

		public static int getSeasonNumber(string whichSeason)
		{
			if (whichSeason.Equals("spring", StringComparison.OrdinalIgnoreCase))
			{
				return 0;
			}
			if (whichSeason.Equals("summer", StringComparison.OrdinalIgnoreCase))
			{
				return 1;
			}
			if (whichSeason.Equals("autumn", StringComparison.OrdinalIgnoreCase) || whichSeason.Equals("fall", StringComparison.OrdinalIgnoreCase))
			{
				return 2;
			}
			if (whichSeason.Equals("winter", StringComparison.OrdinalIgnoreCase))
			{
				return 3;
			}
			return -1;
		}

		public static char getRandomSlotCharacter(char current)
		{
			char c = 'o';
			while (c == 'o' || c == current)
			{
				switch (Game1.random.Next(8))
				{
				case 0:
					c = '=';
					break;
				case 1:
					c = '\\';
					break;
				case 2:
					c = ']';
					break;
				case 3:
					c = '[';
					break;
				case 4:
					c = '<';
					break;
				case 5:
					c = '*';
					break;
				case 6:
					c = '$';
					break;
				case 7:
					c = '}';
					break;
				}
			}
			return c;
		}

		public static List<Vector2> getPositionsInClusterAroundThisTile(Vector2 startTile, int number)
		{
			Queue<Vector2> queue = new Queue<Vector2>();
			List<Vector2> list = new List<Vector2>();
			Vector2 item = startTile;
			queue.Enqueue(item);
			while (list.Count < number)
			{
				item = queue.Dequeue();
				list.Add(item);
				if (!list.Contains(new Vector2(item.X + 1f, item.Y)))
				{
					queue.Enqueue(new Vector2(item.X + 1f, item.Y));
				}
				if (!list.Contains(new Vector2(item.X - 1f, item.Y)))
				{
					queue.Enqueue(new Vector2(item.X - 1f, item.Y));
				}
				if (!list.Contains(new Vector2(item.X, item.Y + 1f)))
				{
					queue.Enqueue(new Vector2(item.X, item.Y + 1f));
				}
				if (!list.Contains(new Vector2(item.X, item.Y - 1f)))
				{
					queue.Enqueue(new Vector2(item.X, item.Y - 1f));
				}
			}
			return list;
		}

		public static bool doesPointHaveLineOfSightInMine(GameLocation mine, Vector2 start, Vector2 end, int visionDistance)
		{
			if (Vector2.Distance(start, end) > (float)visionDistance)
			{
				return false;
			}
			foreach (Point item in GetPointsOnLine((int)start.X, (int)start.Y, (int)end.X, (int)end.Y))
			{
				if (mine.getTileIndexAt(item, "Buildings") != -1)
				{
					return false;
				}
			}
			return true;
		}

		public static void addSprinklesToLocation(GameLocation l, int sourceXTile, int sourceYTile, int tilesWide, int tilesHigh, int totalSprinkleDuration, int millisecondsBetweenSprinkles, Color sprinkleColor, string sound = null, bool motionTowardCenter = false)
		{
			Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle(sourceXTile - tilesWide / 2, sourceYTile - tilesHigh / 2, tilesWide, tilesHigh);
			Random random = new Random();
			int num = totalSprinkleDuration / millisecondsBetweenSprinkles;
			for (int i = 0; i < num; i++)
			{
				Vector2 vector = getRandomPositionInThisRectangle(r, random) * 64f;
				l.temporarySprites.Add(new TemporaryAnimatedSprite(random.Next(10, 12), vector, sprinkleColor, 8, flipped: false, 50f)
				{
					layerDepth = 1f,
					delayBeforeAnimationStart = millisecondsBetweenSprinkles * i,
					interval = 100f,
					startSound = sound,
					motion = (motionTowardCenter ? getVelocityTowardPoint(vector, new Vector2(sourceXTile, sourceYTile) * 64f, Vector2.Distance(new Vector2(sourceXTile, sourceYTile) * 64f, vector) / 64f) : Vector2.Zero),
					xStopCoordinate = sourceXTile,
					yStopCoordinate = sourceYTile
				});
			}
		}

		public static List<TemporaryAnimatedSprite> getStarsAndSpirals(GameLocation l, int sourceXTile, int sourceYTile, int tilesWide, int tilesHigh, int totalSprinkleDuration, int millisecondsBetweenSprinkles, Color sprinkleColor, string sound = null, bool motionTowardCenter = false)
		{
			Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle(sourceXTile - tilesWide / 2, sourceYTile - tilesHigh / 2, tilesWide, tilesHigh);
			Random random = new Random();
			int num = totalSprinkleDuration / millisecondsBetweenSprinkles;
			List<TemporaryAnimatedSprite> list = new List<TemporaryAnimatedSprite>();
			for (int i = 0; i < num; i++)
			{
				Vector2 position = getRandomPositionInThisRectangle(r, random) * 64f;
				list.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", (random.NextDouble() < 0.5) ? new Microsoft.Xna.Framework.Rectangle(359, 1437, 14, 14) : new Microsoft.Xna.Framework.Rectangle(377, 1438, 9, 9), position, flipped: false, 0.01f, sprinkleColor)
				{
					xPeriodic = true,
					xPeriodicLoopTime = random.Next(2000, 3000),
					xPeriodicRange = random.Next(-64, 64),
					motion = new Vector2(0f, -2f),
					rotationChange = (float)Math.PI / (float)random.Next(4, 64),
					delayBeforeAnimationStart = millisecondsBetweenSprinkles * i,
					layerDepth = 1f,
					scaleChange = 0.04f,
					scaleChangeChange = -0.0008f,
					scale = 4f
				});
			}
			return list;
		}

		public static void addStarsAndSpirals(GameLocation l, int sourceXTile, int sourceYTile, int tilesWide, int tilesHigh, int totalSprinkleDuration, int millisecondsBetweenSprinkles, Color sprinkleColor, string sound = null, bool motionTowardCenter = false)
		{
			l.temporarySprites.AddRange(getStarsAndSpirals(l, sourceXTile, sourceYTile, tilesWide, tilesHigh, totalSprinkleDuration, millisecondsBetweenSprinkles, sprinkleColor, sound, motionTowardCenter));
		}

		public static Vector2 snapDrawPosition(Vector2 draw_position)
		{
			return new Vector2((int)draw_position.X, (int)draw_position.Y);
		}

		public static Vector2 clampToTile(Vector2 nonTileLocation)
		{
			nonTileLocation.X -= nonTileLocation.X % 64f;
			nonTileLocation.Y -= nonTileLocation.Y % 64f;
			return nonTileLocation;
		}

		public static float distance(float x1, float x2, float y1, float y2)
		{
			return (float)Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
		}

		public static void facePlayerEndBehavior(Character c, GameLocation location)
		{
			c.faceGeneralDirection(new Vector2(Game1.player.GetBoundingBox().Center.X, Game1.player.GetBoundingBox().Center.Y), 0, opposite: false, useTileCalculations: false);
		}

		public static bool couldSeePlayerInPeripheralVision(Farmer player, Character c)
		{
			switch (c.FacingDirection)
			{
			case 0:
				if (player.GetBoundingBox().Center.Y < c.GetBoundingBox().Center.Y + 32)
				{
					return true;
				}
				break;
			case 1:
				if (player.GetBoundingBox().Center.X > c.GetBoundingBox().Center.X - 32)
				{
					return true;
				}
				break;
			case 2:
				if (player.GetBoundingBox().Center.Y > c.GetBoundingBox().Center.Y - 32)
				{
					return true;
				}
				break;
			case 3:
				if (player.GetBoundingBox().Center.X < c.GetBoundingBox().Center.X + 32)
				{
					return true;
				}
				break;
			}
			return false;
		}

		public static List<Microsoft.Xna.Framework.Rectangle> divideThisRectangleIntoQuarters(Microsoft.Xna.Framework.Rectangle rect)
		{
			List<Microsoft.Xna.Framework.Rectangle> list = new List<Microsoft.Xna.Framework.Rectangle>();
			list.Add(new Microsoft.Xna.Framework.Rectangle(rect.X, rect.Y, rect.Width / 2, rect.Height / 2));
			list.Add(new Microsoft.Xna.Framework.Rectangle(rect.X + rect.Width / 2, rect.Y, rect.Width / 2, rect.Height / 2));
			list.Add(new Microsoft.Xna.Framework.Rectangle(rect.X, rect.Y + rect.Height / 2, rect.Width / 2, rect.Height / 2));
			list.Add(new Microsoft.Xna.Framework.Rectangle(rect.X + rect.Width / 2, rect.Y + rect.Height / 2, rect.Width / 2, rect.Height / 2));
			return list;
		}

		public static Item getUncommonItemForThisMineLevel(int level, Point location)
		{
			Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\weapons");
			List<int> list = new List<int>();
			int num = -1;
			int num2 = -1;
			int num3 = 12;
			Random random = new Random(location.X * 1000 + location.Y + (int)Game1.uniqueIDForThisGame + level);
			foreach (KeyValuePair<int, string> item in dictionary)
			{
				if (Game1.CurrentMineLevel >= Convert.ToInt32(item.Value.Split('/')[10]) && Convert.ToInt32(item.Value.Split('/')[9]) != -1)
				{
					int num4 = Convert.ToInt32(item.Value.Split('/')[9]);
					if (num == -1 || num2 > Math.Abs(Game1.CurrentMineLevel - num4))
					{
						num = item.Key;
						num2 = Convert.ToInt32(item.Value.Split('/')[9]);
					}
					double num5 = Math.Pow(Math.E, (0.0 - Math.Pow(Game1.CurrentMineLevel - num4, 2.0)) / (double)(2 * (num3 * num3)));
					if (random.NextDouble() < num5)
					{
						list.Add(item.Key);
					}
				}
			}
			list.Add(num);
			return new MeleeWeapon(list.ElementAt(random.Next(list.Count)));
		}

		public static IEnumerable<Point> GetPointsOnLine(int x0, int y0, int x1, int y1)
		{
			return GetPointsOnLine(x0, y0, x1, y1, ignoreSwap: false);
		}

		public static List<Vector2> getBorderOfThisRectangle(Microsoft.Xna.Framework.Rectangle r)
		{
			List<Vector2> list = new List<Vector2>();
			for (int i = r.X; i < r.Right; i++)
			{
				list.Add(new Vector2(i, r.Y));
			}
			for (int j = r.Y + 1; j < r.Bottom; j++)
			{
				list.Add(new Vector2(r.Right - 1, j));
			}
			for (int num = r.Right - 2; num >= r.X; num--)
			{
				list.Add(new Vector2(num, r.Bottom - 1));
			}
			for (int num2 = r.Bottom - 2; num2 >= r.Y + 1; num2--)
			{
				list.Add(new Vector2(r.X, num2));
			}
			return list;
		}

		public static Point getTranslatedPoint(Point p, int direction, int movementAmount)
		{
			return direction switch
			{
				0 => new Point(p.X, p.Y - movementAmount), 
				2 => new Point(p.X, p.Y + movementAmount), 
				1 => new Point(p.X + movementAmount, p.Y), 
				3 => new Point(p.X - movementAmount, p.Y), 
				_ => p, 
			};
		}

		public static Vector2 getTranslatedVector2(Vector2 p, int direction, float movementAmount)
		{
			return direction switch
			{
				0 => new Vector2(p.X, p.Y - movementAmount), 
				2 => new Vector2(p.X, p.Y + movementAmount), 
				1 => new Vector2(p.X + movementAmount, p.Y), 
				3 => new Vector2(p.X - movementAmount, p.Y), 
				_ => p, 
			};
		}

		public static IEnumerable<Point> GetPointsOnLine(int x0, int y0, int x1, int y1, bool ignoreSwap)
		{
			bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
			if (steep)
			{
				int num = x0;
				x0 = y0;
				y0 = num;
				num = x1;
				x1 = y1;
				y1 = num;
			}
			if (!ignoreSwap && x0 > x1)
			{
				int num2 = x0;
				x0 = x1;
				x1 = num2;
				num2 = y0;
				y0 = y1;
				y1 = num2;
			}
			int dx = x1 - x0;
			int dy = Math.Abs(y1 - y0);
			int error = dx / 2;
			int ystep = ((y0 < y1) ? 1 : (-1));
			int y2 = y0;
			for (int x2 = x0; x2 <= x1; x2++)
			{
				yield return new Point(steep ? y2 : x2, steep ? x2 : y2);
				error -= dy;
				if (error < 0)
				{
					y2 += ystep;
					error += dx;
				}
			}
		}

		public static Vector2 getRandomAdjacentOpenTile(Vector2 tile, GameLocation location)
		{
			List<Vector2> adjacentTileLocations = getAdjacentTileLocations(tile);
			int i = 0;
			int num = Game1.random.Next(adjacentTileLocations.Count);
			Vector2 vector = adjacentTileLocations[num];
			for (; i < 4; i++)
			{
				if (!location.isTileOccupiedForPlacement(vector) && location.isTilePassable(new Location((int)vector.X, (int)vector.Y), Game1.viewport))
				{
					break;
				}
				num = (num + 1) % adjacentTileLocations.Count;
				vector = adjacentTileLocations[num];
			}
			if (i >= 4)
			{
				return Vector2.Zero;
			}
			return vector;
		}

		public static int getObjectIndexFromSlotCharacter(char character)
		{
			return character switch
			{
				'=' => 72, 
				'\\' => 336, 
				']' => 221, 
				'[' => 276, 
				'<' => 400, 
				'$' => 398, 
				'}' => 184, 
				'*' => 176, 
				_ => 0, 
			};
		}

		private static string farmerAccomplishments()
		{
			string text = (Game1.player.IsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5229") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5230"));
			if (Game1.player.hasRustyKey)
			{
				text += Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5235");
			}
			if (Game1.player.achievements.Contains(71))
			{
				text += Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5236");
			}
			if (Game1.player.achievements.Contains(45))
			{
				text += Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5237");
			}
			if (text.Length > 115)
			{
				text += "#$b#";
			}
			if (Game1.player.achievements.Contains(63))
			{
				text += Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5239");
			}
			if (Game1.player.timesReachedMineBottom > 0)
			{
				text += Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5240");
			}
			return text + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5241", Game1.player.totalMoneyEarned - Game1.player.totalMoneyEarned % 1000u);
		}

		public static string getCreditsString()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5243") + Environment.NewLine + " " + Environment.NewLine + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5244") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5245") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5246") + Environment.NewLine + Environment.NewLine + "-Eric Barone" + Environment.NewLine + " " + Environment.NewLine + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5248") + Environment.NewLine + Environment.NewLine + "-Amber Hageman" + Environment.NewLine + "-Shane Waletzko" + Environment.NewLine + "-Fiddy, Nuns, Kappy &" + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5252") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5253") + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5254");
		}

		public static string getStardewHeroCelebrationEventString(int finalFarmerScore)
		{
			string text = "";
			if (finalFarmerScore >= Game1.percentageToWinStardewHero)
			{
				return "title_day/-100 -100/farmer 18 20 1 rival 27 20 2" + getCelebrationPositionsForDatables(Game1.player.spouse) + ((Game1.player.spouse != null && !Game1.player.isEngaged()) ? (Game1.player.spouse + " 17 21 1 ") : "") + "Lewis 22 19 2 Marnie 21 22 0 Caroline 24 22 0 Pierre 25 22 0 Gus 26 22 0 Clint 26 23 0 Emily 25 23 0 Shane 27 23 0 " + ((Game1.player.friendshipData.ContainsKey("Sandy") && Game1.player.friendshipData["Sandy"].Points > 0) ? "Sandy 24 23 0 " : "") + "George 21 23 0 Evelyn 20 23 0 Pam 19 23 0 Jodi 27 24 0 " + ((Game1.getCharacterFromName("Kent") != null) ? "Kent 26 24 0 " : "") + "Linus 24 24 0 Robin 21 24 0 Demetrius 20 24 0" + ((Game1.player.timesReachedMineBottom > 0) ? " Dwarf 19 24 0" : "") + "/addObject 18 19 " + Game1.random.Next(313, 320) + "/addObject 19 19 " + Game1.random.Next(313, 320) + "/addObject 20 19 " + Game1.random.Next(313, 320) + "/addObject 25 19 " + Game1.random.Next(313, 320) + "/addObject 26 19 " + Game1.random.Next(313, 320) + "/addObject 27 19 " + Game1.random.Next(313, 320) + "/addObject 23 19 468/viewport 22 20 true/pause 4000/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5256") + "\"/pause 400/faceDirection Lewis 3/pause 500/faceDirection Lewis 1/pause 600/faceDirection Lewis 2/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5259") + "\"/pause 200/showRivalFrame 16/pause 600/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5260") + "\"/pause 700/move Lewis 0 1 3/stopMusic/move Lewis -2 0 3/playMusic musicboxsong/faceDirection farmer 1/showRivalFrame 12/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5263", farmerAccomplishments()) + "\"/pause 800/move Lewis 5 0 1/showRivalFrame 12/playMusic rival/pause 500/speak Lewis \"" + (Game1.player.isMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5306") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5307")) + "\"/pause 500/speak rival \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5308") + "\"/move rival 0 1 2/showRivalFrame 17/pause 500/speak rival \"" + ((!Game1.player.isMale) ? ((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5312") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5313")) : ((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5310") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5311"))) + "\"/pause 600/emote farmer 40/showRivalFrame 16/pause 900/move rival 0 -1 2/showRivalFrame 16/move Lewis -3 0 2/stopMusic/pause 500/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5314") + "\"/stopMusic/move Lewis 0 -1 2/pause 600/faceDirection Lewis 1/pause 600/faceDirection Lewis 3/pause 600/faceDirection Lewis 2/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5315") + "\"/pause 300/move rival -2 0 2/showRivalFrame 16/pause 1500/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5316") + "\"/pause 500/showRivalFrame 18/pause 400/playMusic happy/emote farmer 16/move farmer 5 0 2/move Lewis 0 1 1/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5317", finalFarmerScore) + "\"/speak Emily \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5318") + "\"/speak Gus \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5319") + "\"/speak Pierre \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5320") + "\"/showRivalFrame 12/pause 500/speak rival \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5321") + "\"/speed rival 4/move rival 6 0 0/faceDirection farmer 1 true/speed rival 4/move rival 0 -10 1/warp rival -100 -100/move farmer 0 1 2/emote farmer 20/fade/viewport -1000 -1000/message \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5322", getOtherFarmerNames()[0]) + "\"/end credits";
			}
			return "title_day/-100 -100/farmer 18 20 1 rival 27 20 2" + getCelebrationPositionsForDatables(Game1.player.spouse) + ((Game1.player.spouse != null && !Game1.player.isEngaged()) ? (Game1.player.spouse + " 17 21 1 ") : "") + "Lewis 22 19 2 Marnie 21 22 0 Caroline 24 22 0 Pierre 25 22 0 Gus 26 22 0 Clint 26 23 0 Emily 25 23 0 Shane 27 23 0 " + ((Game1.player.friendshipData.ContainsKey("Sandy") && Game1.player.friendshipData["Sandy"].Points > 0) ? "Sandy 24 23 0 " : "") + "George 21 23 0 Evelyn 20 23 0 Pam 19 23 0 Jodi 27 24 0 " + ((Game1.getCharacterFromName("Kent") != null) ? "Kent 26 24 0 " : "") + "Linus 24 24 0 Robin 21 24 0 Demetrius 20 24 0" + ((Game1.player.timesReachedMineBottom > 0) ? " Dwarf 19 24 0" : "") + "/addObject 18 19 " + Game1.random.Next(313, 320) + "/addObject 19 19 " + Game1.random.Next(313, 320) + "/addObject 20 19 " + Game1.random.Next(313, 320) + "/addObject 25 19 " + Game1.random.Next(313, 320) + "/addObject 26 19 " + Game1.random.Next(313, 320) + "/addObject 27 19 " + Game1.random.Next(313, 320) + "/addObject 23 19 468/viewport 22 20 true/pause 4000/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5256") + "\"/pause 400/faceDirection Lewis 3/pause 500/faceDirection Lewis 1/pause 600/faceDirection Lewis 2/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5259") + "\"/pause 200/showRivalFrame 16/pause 600/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5260") + "\"/pause 700/move Lewis 0 1 3/stopMusic/move Lewis -2 0 3/playMusic musicboxsong/faceDirection farmer 1/showRivalFrame 12/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5263", farmerAccomplishments()) + "\"/pause 800/move Lewis 5 0 1/showRivalFrame 12/playMusic rival/pause 500/speak Lewis \"" + (Game1.player.isMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5306") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5307")) + "\"/pause 500/speak rival \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5308") + "\"/move rival 0 1 2/showRivalFrame 17/pause 500/speak rival \"" + ((!Game1.player.isMale) ? ((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5312") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5313")) : ((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5310") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5311"))) + "\"/pause 600/emote farmer 40/showRivalFrame 16/pause 900/move rival 0 -1 2/showRivalFrame 16/move Lewis -3 0 2/stopMusic/pause 500/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5314") + "\"/stopMusic/move Lewis 0 -1 2/pause 600/faceDirection Lewis 1/pause 600/faceDirection Lewis 3/pause 600/faceDirection Lewis 2/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5315") + "\"/pause 300/move rival -2 0 2/showRivalFrame 16/pause 1500/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5323") + "\"/pause 200/showFrame 32/move rival -2 0 2/showRivalFrame 19/pause 400/playSound death/emote farmer 28/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5324", Game1.percentageToWinStardewHero - finalFarmerScore) + "\"/speak rival \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5325") + "\"/pause 600/faceDirection Lewis 3/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5326") + "\"/speak Emily \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5327") + "\"/fade/viewport -1000 -1000/message \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5328", finalFarmerScore) + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5329") + "\"/end credits";
		}

		public static void CollectSingleItemOrShowChestMenu(Chest chest, object context = null)
		{
			int num = 0;
			Item item = null;
			for (int i = 0; i < chest.items.Count; i++)
			{
				if (chest.items[i] != null)
				{
					num++;
					if (num == 1)
					{
						item = chest.items[i];
					}
					if (num == 2)
					{
						item = null;
						break;
					}
				}
			}
			if (num == 0)
			{
				return;
			}
			if (item != null)
			{
				int stack = item.Stack;
				if (Game1.player.addItemToInventory(item) == null)
				{
					Game1.playSound("coin");
					chest.items.Remove(item);
					chest.clearNulls();
					return;
				}
				if (item.Stack != stack)
				{
					Game1.playSound("coin");
				}
			}
			if (context != null && context is Mill)
			{
				chest.createSlotsForCapacity(force: true);
				chest.SpecialChestType = Chest.SpecialChestTypes.Mill;
			}
			Game1.activeClickableMenu = new ItemGrabMenu(chest.items, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, chest.grabItemFromInventory, null, chest.grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, null, -1, null, -1, 3, null, allowStack: true, null, rearrangeGrangeOnExit: false, null, context);
		}

		public static bool CollectOrDrop(Item item, int direction)
		{
			if (item != null)
			{
				item = Game1.player.addItemToInventory(item);
				if (item != null)
				{
					if (direction != -1)
					{
						Game1.createItemDebris(item, Game1.player.getStandingPosition(), direction);
					}
					else
					{
						Game1.createItemDebris(item, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
					}
					return false;
				}
				return true;
			}
			return true;
		}

		public static bool CollectOrDrop(Item item)
		{
			return CollectOrDrop(item, -1);
		}

		public static void perpareDayForStardewCelebration(int finalFarmerScore)
		{
			bool flag = finalFarmerScore >= Game1.percentageToWinStardewHero;
			foreach (GameLocation location in Game1.locations)
			{
				foreach (NPC character in location.characters)
				{
					string masterDialogue = "";
					if (flag)
					{
						switch (Game1.random.Next(6))
						{
						case 0:
							masterDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5348");
							break;
						case 1:
							masterDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5349");
							break;
						case 2:
							masterDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5350");
							break;
						case 3:
							masterDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5351");
							break;
						case 4:
							masterDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5352");
							break;
						case 5:
							masterDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5353");
							break;
						}
						if (character.Name.Equals("Sebastian") || character.Name.Equals("Abigail"))
						{
							masterDialogue = (Game1.player.IsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5356") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5357"));
						}
						else if (character.Name.Equals("George"))
						{
							masterDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5360");
						}
					}
					else
					{
						switch (Game1.random.Next(4))
						{
						case 0:
							masterDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5361");
							break;
						case 1:
							masterDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5362");
							break;
						case 2:
							masterDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5363");
							break;
						case 3:
							masterDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5364");
							break;
						}
						if (character.Name.Equals("George"))
						{
							masterDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5360");
						}
					}
					character.CurrentDialogue.Push(new Dialogue(masterDialogue, character));
				}
			}
			if (flag)
			{
				Game1.player.stardewHero = true;
			}
		}

		public static List<string> getExes(Farmer farmer)
		{
			List<string> list = new List<string>();
			foreach (string key in farmer.friendshipData.Keys)
			{
				if (farmer.friendshipData[key].IsDivorced())
				{
					list.Add(key);
				}
			}
			return list;
		}

		public static string getCelebrationPositionsForDatables(List<string> people_to_exclude)
		{
			string text = " ";
			if (!people_to_exclude.Contains("Sam"))
			{
				text += "Sam 25 65 0 ";
			}
			if (!people_to_exclude.Contains("Sebastian"))
			{
				text += "Sebastian 24 65 0 ";
			}
			if (!people_to_exclude.Contains("Alex"))
			{
				text += "Alex 25 69 0 ";
			}
			if (!people_to_exclude.Contains("Harvey"))
			{
				text += "Harvey 23 67 0 ";
			}
			if (!people_to_exclude.Contains("Elliott"))
			{
				text += "Elliott 32 65 0 ";
			}
			if (!people_to_exclude.Contains("Haley"))
			{
				text += "Haley 26 69 0 ";
			}
			if (!people_to_exclude.Contains("Penny"))
			{
				text += "Penny 23 66 0 ";
			}
			if (!people_to_exclude.Contains("Maru"))
			{
				text += "Maru 24 68 0 ";
			}
			if (!people_to_exclude.Contains("Leah"))
			{
				text += "Leah 33 65 0 ";
			}
			if (!people_to_exclude.Contains("Abigail"))
			{
				text += "Abigail 23 65 0 ";
			}
			return text;
		}

		public static string getCelebrationPositionsForDatables(string personToExclude)
		{
			List<string> list = new List<string>();
			if (personToExclude != null)
			{
				list.Add(personToExclude);
			}
			return getCelebrationPositionsForDatables(list);
		}

		public static void fixAllAnimals()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			Farm farm = Game1.getFarm();
			foreach (Building building in farm.buildings)
			{
				if (building.indoors.Value == null || !(building.indoors.Value is AnimalHouse))
				{
					continue;
				}
				foreach (long item in (building.indoors.Value as AnimalHouse).animalsThatLiveHere)
				{
					FarmAnimal animal = getAnimal(item);
					if (animal != null)
					{
						animal.home = building;
						animal.homeLocation.Value = new Vector2((int)building.tileX, (int)building.tileY);
					}
				}
			}
			List<FarmAnimal> list = new List<FarmAnimal>();
			List<FarmAnimal> allFarmAnimals = farm.getAllFarmAnimals();
			foreach (FarmAnimal item2 in allFarmAnimals)
			{
				if (item2.home == null)
				{
					list.Add(item2);
				}
			}
			foreach (FarmAnimal item3 in list)
			{
				foreach (Building building2 in farm.buildings)
				{
					if (building2.indoors.Value == null || !(building2.indoors.Value is AnimalHouse))
					{
						continue;
					}
					for (int num = (building2.indoors.Value as AnimalHouse).animals.Count() - 1; num >= 0; num--)
					{
						if ((building2.indoors.Value as AnimalHouse).animals.Pairs.ElementAt(num).Value.Equals(item3))
						{
							(building2.indoors.Value as AnimalHouse).animals.Remove((building2.indoors.Value as AnimalHouse).animals.Pairs.ElementAt(num).Key);
						}
					}
				}
				for (int num2 = farm.animals.Count() - 1; num2 >= 0; num2--)
				{
					if (farm.animals.Pairs.ElementAt(num2).Value.Equals(item3))
					{
						farm.animals.Remove(farm.animals.Pairs.ElementAt(num2).Key);
					}
				}
			}
			foreach (Building building3 in farm.buildings)
			{
				if (building3.indoors.Value == null || !(building3.indoors.Value is AnimalHouse))
				{
					continue;
				}
				for (int num3 = (building3.indoors.Value as AnimalHouse).animalsThatLiveHere.Count - 1; num3 >= 0; num3--)
				{
					FarmAnimal animal2 = getAnimal((building3.indoors.Value as AnimalHouse).animalsThatLiveHere[num3]);
					if (animal2.home != building3)
					{
						(building3.indoors.Value as AnimalHouse).animalsThatLiveHere.RemoveAt(num3);
					}
				}
			}
			foreach (FarmAnimal item4 in list)
			{
				foreach (Building building4 in farm.buildings)
				{
					if (building4.buildingType.Contains(item4.buildingTypeILiveIn) && building4.indoors.Value != null && building4.indoors.Value is AnimalHouse && !(building4.indoors.Value as AnimalHouse).isFull())
					{
						item4.home = building4;
						item4.homeLocation.Value = new Vector2((int)building4.tileX, (int)building4.tileY);
						item4.setRandomPosition(item4.home.indoors);
						(item4.home.indoors.Value as AnimalHouse).animals.Add(item4.myID, item4);
						(item4.home.indoors.Value as AnimalHouse).animalsThatLiveHere.Add(item4.myID);
						break;
					}
				}
			}
			List<FarmAnimal> list2 = new List<FarmAnimal>();
			foreach (FarmAnimal item5 in list)
			{
				if (item5.home == null)
				{
					list2.Add(item5);
				}
			}
			foreach (FarmAnimal item6 in list2)
			{
				item6.Position = recursiveFindOpenTileForCharacter(item6, farm, new Vector2(40f, 40f), 200) * 64f;
				if (!farm.animals.ContainsKey(item6.myID))
				{
					farm.animals.Add(item6.myID, item6);
				}
			}
		}

		public static Event getWeddingEvent(Farmer farmer)
		{
			List<string> exes = getExes(farmer);
			exes.Add(farmer.spouse);
			string text = "";
			text = "sweet/-1000 -100/farmer 27 63 2 spouse 28 63 2" + getCelebrationPositionsForDatables(exes) + "Lewis 27 64 2 Marnie 26 65 0 Caroline 29 65 0 Pierre 30 65 0 Gus 31 65 0 Clint 31 66 0 " + ((farmer.spouse.Contains("Emily") || exes.Contains("Emily")) ? "" : "Emily 30 66 0 ") + ((farmer.spouse.Contains("Shane") || exes.Contains("Shane")) ? "" : "Shane 32 66 0 ") + ((farmer.friendshipData.ContainsKey("Sandy") && farmer.friendshipData["Sandy"].Points > 0) ? "Sandy 29 66 0 " : "") + "George 26 66 0 Evelyn 25 66 0 Pam 24 66 0 Jodi 32 67 0 " + ((Game1.getCharacterFromName("Kent") != null) ? "Kent 31 67 0 " : "") + "otherFarmers 29 69 0 Linus 29 67 0 Robin 25 67 0 Demetrius 26 67 0 Vincent 26 68 3 Jas 25 68 1" + ((farmer.friendshipData.ContainsKey("Dwarf") && farmer.friendshipData["Dwarf"].Points > 0) ? " Dwarf 30 67 0" : "") + "/changeLocation Town/showFrame spouse 36/specificTemporarySprite wedding/viewport 27 64 true/pause 4000/speak Lewis \"" + (farmer.IsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5367", Game1.dayOfMonth, Game1.CurrentSeasonDisplayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5369", Game1.dayOfMonth, Game1.CurrentSeasonDisplayName)) + "\"/faceDirection farmer 1/showFrame spouse 37/pause 500/faceDirection Lewis 0/pause 2000/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5370") + "\"/move Lewis 0 1 0/playMusic none/pause 1000/showFrame Lewis 20/speak Lewis \"" + ((!farmer.IsMale) ? (isMale(farmer.spouse) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5377") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5375")) : (isMale(farmer.spouse) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5371") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5373"))) + "\"/pause 500/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5379") + "\"/pause 1000/showFrame 101/showFrame spouse 38/specificTemporarySprite heart 28 62/playSound dwop/pause 2000/specificTemporarySprite wed/warp Marnie -2000 -2000/faceDirection farmer 2/showFrame spouse 36/faceDirection Pam 1 true/faceDirection Evelyn 3 true/faceDirection Pierre 3 true/faceDirection Caroline 1 true/animate Robin false true 500 20 21 20 22/animate Demetrius false true 500 24 25 24 26/move Lewis 0 3 3 true/move Caroline 0 -1 3 false/pause 4000/faceDirection farmer 1/showFrame spouse 37/globalFade/viewport -1000 -1000/pause 1000/message \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5381") + "\"/pause 500/message \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5383") + "\"/pause 4000/waitForOtherPlayers weddingEnd" + farmer.uniqueMultiplayerID?.ToString() + "/end wedding";
			return new Event(text, -2, farmer);
		}

		public static Event getPlayerWeddingEvent(Farmer farmer, Farmer spouse)
		{
			List<string> exes = getExes(farmer);
			exes.AddRange(getExes(spouse));
			string text = "";
			text = "sweet/-1000 -100/farmer 27 63 2" + getCelebrationPositionsForDatables(exes) + "Lewis 27 64 2 Marnie 26 65 0 Caroline 29 65 0 Pierre 30 65 0 Gus 31 65 0 Clint 31 66 0 " + (exes.Contains("Emily") ? "" : "Emily 30 66 0 ") + (exes.Contains("Shane") ? "" : "Shane 32 66 0 ") + ((farmer.friendshipData.ContainsKey("Sandy") && farmer.friendshipData["Sandy"].Points > 0) ? "Sandy 29 66 0 " : "") + "George 26 66 0 Evelyn 25 66 0 Pam 24 66 0 Jodi 32 67 0 " + ((Game1.getCharacterFromName("Kent") != null) ? "Kent 31 67 0 " : "") + "otherFarmers 29 69 0 Linus 29 67 0 Robin 25 67 0 Demetrius 26 67 0 Vincent 26 68 3 Jas 25 68 1" + ((farmer.friendshipData.ContainsKey("Dwarf") && farmer.friendshipData["Dwarf"].Points > 0) ? " Dwarf 30 67 0" : "") + "/changeLocation Town/faceDirection spouseFarmer 2/warp spouseFarmer 28 63/specificTemporarySprite wedding/viewport 27 64 true/pause 4000/speak Lewis \"" + (farmer.IsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5367", Game1.dayOfMonth, Game1.CurrentSeasonDisplayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5369", Game1.dayOfMonth, Game1.CurrentSeasonDisplayName)) + "\"/faceDirection farmer 1/faceDirection spouseFarmer 3/pause 500/faceDirection Lewis 0/pause 2000/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5370") + "\"/move Lewis 0 1 0/playMusic none/pause 1000/showFrame Lewis 20/speak Lewis \"" + ((!farmer.IsMale) ? (spouse.IsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5377") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5375")) : (spouse.IsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5371") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5373"))) + "\"/pause 500/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5379") + "\"/pause 1000/showFrame 101/showFrame spouseFarmer 101/specificTemporarySprite heart 28 62/playSound dwop/pause 2000/specificTemporarySprite wed/warp Marnie -2000 -2000/faceDirection farmer 2/faceDirection spouseFarmer 2/faceDirection Pam 1 true/faceDirection Evelyn 3 true/faceDirection Pierre 3 true/faceDirection Caroline 1 true/animate Robin false true 500 20 21 20 22/animate Demetrius false true 500 24 25 24 26/move Lewis 0 3 3 true/move Caroline 0 -1 3 false/pause 4000/faceDirection farmer 1/showFrame spouseFarmer 3/globalFade/viewport -1000 -1000/pause 1000/message \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5381") + "\"/pause 500/message \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5383") + "\"/pause 4000/waitForOtherPlayers weddingEnd" + farmer.uniqueMultiplayerID?.ToString() + "/end wedding";
			text = text.Replace("spouseFarmer", "farmer" + getFarmerNumberFromFarmer(spouse));
			return new Event(text, -2, farmer);
		}

		public static void drawTinyDigits(int toDraw, SpriteBatch b, Vector2 position, float scale, float layerDepth, Color c)
		{
			if (Game1.options.bigNumbers)
			{
				scale *= 1.5f;
				position.Y -= 8f;
			}
			int num = 0;
			int num2 = toDraw;
			int num3 = 0;
			do
			{
				num3++;
			}
			while ((toDraw /= 10) >= 1);
			int num4 = (int)Math.Pow(10.0, num3 - 1);
			bool flag = false;
			for (int i = 0; i < num3; i++)
			{
				int num5 = num2 / num4 % 10;
				if (num5 > 0 || i == num3 - 1)
				{
					flag = true;
				}
				if (flag)
				{
					b.Draw(Game1.mouseCursors, position + new Vector2(num, 0f), new Microsoft.Xna.Framework.Rectangle(368 + num5 * 5, 56, 5, 7), c, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
				}
				num += (int)(5f * scale) - 1;
				num4 /= 10;
			}
		}

		public static int getWidthOfTinyDigitString(int toDraw, float scale)
		{
			if (Game1.options.bigNumbers)
			{
				scale *= 1.5f;
			}
			int num = 0;
			do
			{
				num++;
			}
			while ((toDraw /= 10) >= 1);
			return (int)((float)(num * 5) * scale);
		}

		public static bool isMale(string who)
		{
			if (who != null)
			{
				switch (who.Length)
				{
				case 4:
				{
					char c = who[0];
					if (c != 'L')
					{
						if (c != 'M' || !(who == "Maru"))
						{
							break;
						}
					}
					else if (!(who == "Leah"))
					{
						break;
					}
					goto IL_00cd;
				}
				case 5:
				{
					char c = who[0];
					if ((uint)c <= 72u)
					{
						if (c != 'E')
						{
							if (c != 'H' || !(who == "Haley"))
							{
								break;
							}
						}
						else if (!(who == "Emily"))
						{
							break;
						}
					}
					else if (c != 'P')
					{
						if (c != 'S' || !(who == "Sandy"))
						{
							break;
						}
					}
					else if (!(who == "Penny"))
					{
						break;
					}
					goto IL_00cd;
				}
				case 7:
					{
						if (!(who == "Abigail"))
						{
							break;
						}
						goto IL_00cd;
					}
					IL_00cd:
					return false;
				}
			}
			return true;
		}

		public static int GetMaximumHeartsForCharacter(Character character)
		{
			if (character == null)
			{
				return 0;
			}
			int result = 10;
			if (character is NPC && (bool)((NPC)character).datable)
			{
				result = 8;
			}
			Friendship friendship = null;
			if (Game1.player.friendshipData.ContainsKey(character.Name))
			{
				friendship = Game1.player.friendshipData[character.Name];
			}
			if (friendship != null)
			{
				if (friendship.IsMarried())
				{
					result = 14;
				}
				else if (friendship.IsDating())
				{
					result = 10;
				}
			}
			return result;
		}

		public static bool doesItemWithThisIndexExistAnywhere(int index, bool bigCraftable = false)
		{
			bool item_found = false;
			iterateAllItems(delegate(Item item)
			{
				if (item is Object && (bool)(item as Object).bigCraftable == bigCraftable && (int)item.parentSheetIndex == index)
				{
					item_found = true;
				}
			});
			return item_found;
		}

		public static int getSwordUpgradeLevel()
		{
			foreach (Item item in Game1.player.items)
			{
				if (item != null && item is Sword)
				{
					return ((Tool)item).upgradeLevel;
				}
			}
			return 0;
		}

		public static bool tryToAddObjectToHome(Object o)
		{
			GameLocation locationFromName = Game1.getLocationFromName("FarmHouse");
			for (int num = locationFromName.map.GetLayer("Back").LayerWidth - 1; num >= 0; num--)
			{
				for (int num2 = locationFromName.map.GetLayer("Back").LayerHeight - 1; num2 >= 0; num2--)
				{
					if (locationFromName.map.GetLayer("Back").Tiles[num, num2] != null && locationFromName.dropObject(o, new Vector2(num * 64, num2 * 64), Game1.viewport, initialPlacement: false))
					{
						if (o.ParentSheetIndex == 468)
						{
							Object @object = new Object(new Vector2(num, num2), 308, null, canBeSetDown: true, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
							@object.heldObject.Value = o;
							locationFromName.objects[new Vector2(num, num2)] = @object;
						}
						return true;
					}
				}
			}
			return false;
		}

		internal static void CollectGarbage(string filePath = "", int lineNumber = 0)
		{
			GC.Collect(0, GCCollectionMode.Forced);
		}

		public static string InvokeSimpleReturnTypeMethod(object toBeCalled, string methodName, object[] parameters)
		{
			Type type = toBeCalled.GetType();
			string text = "";
			try
			{
				return ((string)type.InvokeMember(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, toBeCalled, parameters)) ?? "";
			}
			catch (Exception ex)
			{
				return Game1.parseText("Didn't work - " + ex.Message);
			}
		}

		public static List<int> possibleCropsAtThisTime(string season, bool firstWeek)
		{
			List<int> list = null;
			List<int> list2 = null;
			if (season.Equals("spring"))
			{
				list = new List<int> { 24, 192 };
				if (Game1.year > 1)
				{
					list.Add(250);
				}
				if (doesAnyFarmerHaveMail("ccVault"))
				{
					list.Add(248);
				}
				list2 = new List<int> { 190, 188 };
				if (doesAnyFarmerHaveMail("ccVault"))
				{
					list2.Add(252);
				}
				list2.AddRange(list);
			}
			else if (season.Equals("summer"))
			{
				list = new List<int> { 264, 262, 260 };
				list2 = new List<int> { 254, 256 };
				if (Game1.year > 1)
				{
					list.Add(266);
				}
				if (doesAnyFarmerHaveMail("ccVault"))
				{
					list2.AddRange(new int[2] { 258, 268 });
				}
				list2.AddRange(list);
			}
			else if (season.Equals("fall"))
			{
				list = new List<int> { 272, 278 };
				list2 = new List<int> { 270, 276, 280 };
				if (Game1.year > 1)
				{
					list2.Add(274);
				}
				if (doesAnyFarmerHaveMail("ccVault"))
				{
					list.Add(284);
					list2.Add(282);
				}
				list2.AddRange(list);
			}
			if (firstWeek)
			{
				return list;
			}
			return list2;
		}

		public static int[] cropsOfTheWeek()
		{
			Random random = new Random((int)Game1.uniqueIDForThisGame + (int)(Game1.stats.DaysPlayed / 29u));
			int[] array = new int[4];
			List<int> list = possibleCropsAtThisTime(Game1.currentSeason, firstWeek: true);
			List<int> list2 = possibleCropsAtThisTime(Game1.currentSeason, firstWeek: false);
			if (list != null)
			{
				array[0] = list.ElementAt(random.Next(list.Count));
				for (int i = 1; i < 4; i++)
				{
					array[i] = list2.ElementAt(random.Next(list2.Count));
					while (array[i] == array[i - 1])
					{
						array[i] = list2.ElementAt(random.Next(list2.Count));
					}
				}
			}
			return array;
		}

		public static float RandomFloat(float min, float max, Random random = null)
		{
			if (random == null)
			{
				random = Game1.random;
			}
			return Lerp(min, max, (float)random.NextDouble());
		}

		public static float Clamp(float value, float min, float max)
		{
			if (max < min)
			{
				float num = min;
				min = max;
				max = num;
			}
			if (value < min)
			{
				value = min;
			}
			if (value > max)
			{
				value = max;
			}
			return value;
		}

		public static Color MakeCompletelyOpaque(Color color)
		{
			if (color.A >= 255)
			{
				return color;
			}
			color.A = 255;
			return color;
		}

		public static int Clamp(int value, int min, int max)
		{
			if (max < min)
			{
				int num = min;
				min = max;
				max = num;
			}
			if (value < min)
			{
				value = min;
			}
			if (value > max)
			{
				value = max;
			}
			return value;
		}

		public static float Lerp(float a, float b, float t)
		{
			return a + t * (b - a);
		}

		public static float MoveTowards(float from, float to, float delta)
		{
			if (Math.Abs(to - from) <= delta)
			{
				return to;
			}
			return from + (float)Math.Sign(to - from) * delta;
		}

		public static Color MultiplyColor(Color a, Color b)
		{
			return new Color((float)(int)a.R / 255f * ((float)(int)b.R / 255f), (float)(int)a.G / 255f * ((float)(int)b.G / 255f), (float)(int)a.B / 255f * ((float)(int)b.B / 255f), (float)(int)a.A / 255f * ((float)(int)b.A / 255f));
		}

		public static int CalculateMinutesUntilMorning(int currentTime)
		{
			return CalculateMinutesUntilMorning(currentTime, 1);
		}

		public static int CalculateMinutesUntilMorning(int currentTime, int daysElapsed)
		{
			if (daysElapsed <= 0)
			{
				return 0;
			}
			daysElapsed--;
			int num = ConvertTimeToMinutes(2600) - ConvertTimeToMinutes(currentTime) + 400;
			return num + daysElapsed * 1600;
		}

		public static int CalculateMinutesBetweenTimes(int startTime, int endTime)
		{
			return ConvertTimeToMinutes(endTime) - ConvertTimeToMinutes(startTime);
		}

		public static int ModifyTime(int timestamp, int minutes_to_add)
		{
			timestamp = ConvertTimeToMinutes(timestamp);
			timestamp += minutes_to_add;
			return ConvertMinutesToTime(timestamp);
		}

		public static int ConvertMinutesToTime(int minutes)
		{
			return minutes / 60 * 100 + minutes % 60;
		}

		public static int ConvertTimeToMinutes(int time_stamp)
		{
			int num = time_stamp / 100 * 60;
			return num + time_stamp % 100;
		}

		public static int getSellToStorePriceOfItem(Item i, bool countStack = true)
		{
			if (i != null)
			{
				return ((i is Object) ? (i as Object).sellToStorePrice(-1L) : (i.salePrice() / 2)) * ((!countStack) ? 1 : i.Stack);
			}
			return 0;
		}

		public static bool HasAnyPlayerSeenSecretNote(int note_number)
		{
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer.secretNotesSeen.Contains(note_number))
				{
					return true;
				}
			}
			return false;
		}

		public static bool HasAnyPlayerSeenEvent(int event_number)
		{
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer.eventsSeen.Contains(event_number))
				{
					return true;
				}
			}
			return false;
		}

		public static bool HaveAllPlayersSeenEvent(int event_number)
		{
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (!allFarmer.eventsSeen.Contains(event_number))
				{
					return false;
				}
			}
			return true;
		}

		public static List<string> GetAllPlayerUnlockedCookingRecipes()
		{
			List<string> list = new List<string>();
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				foreach (string key in allFarmer.cookingRecipes.Keys)
				{
					if (!list.Contains(key))
					{
						list.Add(key);
					}
				}
			}
			return list;
		}

		public static List<string> GetAllPlayerUnlockedCraftingRecipes()
		{
			List<string> list = new List<string>();
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				foreach (string key in allFarmer.craftingRecipes.Keys)
				{
					if (!list.Contains(key))
					{
						list.Add(key);
					}
				}
			}
			return list;
		}

		public static int GetAllPlayerFriendshipLevel(NPC npc)
		{
			int num = -1;
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer.friendshipData.ContainsKey(npc.Name) && allFarmer.friendshipData[npc.Name].Points > num)
				{
					num = allFarmer.friendshipData[npc.Name].Points;
				}
			}
			return num;
		}

		public static int GetAllPlayerReachedBottomOfMines()
		{
			int num = 0;
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer.timesReachedMineBottom > num)
				{
					num = allFarmer.timesReachedMineBottom;
				}
			}
			return num;
		}

		public static int GetAllPlayerDeepestMineLevel()
		{
			int num = 0;
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer.deepestMineLevel > num)
				{
					num = allFarmer.deepestMineLevel;
				}
			}
			return num;
		}

		public static int getRandomBasicSeasonalForageItem(string season, int randomSeedAddition = -1)
		{
			Random random = new Random((int)Game1.uniqueIDForThisGame + randomSeedAddition);
			int num = 0;
			List<int> list = new List<int>();
			if (season.Equals("spring"))
			{
				list.AddRange(new int[4] { 16, 18, 20, 22 });
			}
			else if (season.Equals("summer"))
			{
				list.AddRange(new int[3] { 396, 398, 402 });
			}
			else if (season.Equals("fall"))
			{
				list.AddRange(new int[4] { 404, 406, 408, 410 });
			}
			else if (season.Equals("winter"))
			{
				list.AddRange(new int[4] { 412, 414, 416, 418 });
			}
			return list.ElementAt(random.Next(list.Count));
		}

		public static int getRandomPureSeasonalItem(string season, int randomSeedAddition)
		{
			Random random = new Random((int)Game1.uniqueIDForThisGame + randomSeedAddition);
			int num = 0;
			List<int> list = new List<int>();
			if (season.Equals("spring"))
			{
				list.AddRange(new int[15]
				{
					16, 18, 20, 22, 129, 131, 132, 136, 137, 142,
					143, 145, 147, 148, 152
				});
			}
			else if (season.Equals("summer"))
			{
				list.AddRange(new int[16]
				{
					128, 130, 131, 132, 136, 138, 142, 144, 145, 146,
					149, 150, 155, 396, 398, 402
				});
			}
			else if (season.Equals("fall"))
			{
				list.AddRange(new int[17]
				{
					404, 406, 408, 410, 129, 131, 132, 136, 137, 139,
					140, 142, 143, 148, 150, 154, 155
				});
			}
			else if (season.Equals("winter"))
			{
				list.AddRange(new int[17]
				{
					412, 414, 416, 418, 130, 131, 132, 136, 140, 141,
					143, 144, 146, 147, 150, 151, 154
				});
			}
			return list.ElementAt(random.Next(list.Count));
		}

		public static int getRandomItemFromSeason(string season, int randomSeedAddition, bool forQuest, bool changeDaily = true)
		{
			Random random = new Random((int)Game1.uniqueIDForThisGame + (int)(changeDaily ? Game1.stats.DaysPlayed : 0) + randomSeedAddition);
			int num = 0;
			List<int> list = new List<int> { 68, 66, 78, 80, 86, 152, 167, 153, 420 };
			List<string> list2 = new List<string>(Game1.player.craftingRecipes.Keys);
			List<string> list3 = new List<string>(Game1.player.cookingRecipes.Keys);
			if (forQuest)
			{
				list2 = GetAllPlayerUnlockedCraftingRecipes();
				list3 = GetAllPlayerUnlockedCookingRecipes();
			}
			if ((forQuest && (MineShaft.lowestLevelReached > 40 || GetAllPlayerReachedBottomOfMines() >= 1)) || (!forQuest && (Game1.player.deepestMineLevel > 40 || Game1.player.timesReachedMineBottom >= 1)))
			{
				list.AddRange(new int[5] { 62, 70, 72, 84, 422 });
			}
			if ((forQuest && (MineShaft.lowestLevelReached > 80 || GetAllPlayerReachedBottomOfMines() >= 1)) || (!forQuest && (Game1.player.deepestMineLevel > 80 || Game1.player.timesReachedMineBottom >= 1)))
			{
				list.AddRange(new int[3] { 64, 60, 82 });
			}
			if (doesAnyFarmerHaveMail("ccVault"))
			{
				list.AddRange(new int[4] { 88, 90, 164, 165 });
			}
			if (list2.Contains("Furnace"))
			{
				list.AddRange(new int[4] { 334, 335, 336, 338 });
			}
			if (list2.Contains("Quartz Globe"))
			{
				list.Add(339);
			}
			if (season.Equals("spring"))
			{
				list.AddRange(new int[17]
				{
					16, 18, 20, 22, 129, 131, 132, 136, 137, 142,
					143, 145, 147, 148, 152, 167, 267
				});
			}
			else if (season.Equals("summer"))
			{
				list.AddRange(new int[16]
				{
					128, 130, 132, 136, 138, 142, 144, 145, 146, 149,
					150, 155, 396, 398, 402, 267
				});
			}
			else if (season.Equals("fall"))
			{
				list.AddRange(new int[18]
				{
					404, 406, 408, 410, 129, 131, 132, 136, 137, 139,
					140, 142, 143, 148, 150, 154, 155, 269
				});
			}
			else if (season.Equals("winter"))
			{
				list.AddRange(new int[17]
				{
					412, 414, 416, 418, 130, 131, 132, 136, 140, 141,
					144, 146, 147, 150, 151, 154, 269
				});
			}
			if (forQuest)
			{
				foreach (string item in list3)
				{
					if (random.NextDouble() < 0.4)
					{
						continue;
					}
					List<int> list4 = possibleCropsAtThisTime(Game1.currentSeason, Game1.dayOfMonth <= 7);
					Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data//CookingRecipes");
					if (!dictionary.ContainsKey(item))
					{
						continue;
					}
					string[] array = dictionary[item].Split('/')[0].Split(' ');
					bool flag = true;
					for (int i = 0; i < array.Length; i++)
					{
						if (!list.Contains(Convert.ToInt32(array[i])) && !isCategoryIngredientAvailable(Convert.ToInt32(array[i])) && (list4 == null || !list4.Contains(Convert.ToInt32(array[i]))))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						list.Add(Convert.ToInt32(dictionary[item].Split('/')[2]));
					}
				}
			}
			return list.ElementAt(random.Next(list.Count));
		}

		private static bool isCategoryIngredientAvailable(int category)
		{
			if (category < 0)
			{
				return category switch
				{
					-5 => false, 
					-6 => false, 
					_ => true, 
				};
			}
			return false;
		}

		public static int weatherDebrisOffsetForSeason(string season)
		{
			return season switch
			{
				"spring" => 16, 
				"summer" => 24, 
				"fall" => 18, 
				"winter" => 20, 
				_ => 0, 
			};
		}

		public static Color getSkyColorForSeason(string season)
		{
			return season switch
			{
				"spring" => new Color(92, 170, 255), 
				"summer" => new Color(24, 163, 255), 
				"fall" => new Color(255, 184, 151), 
				"winter" => new Color(165, 207, 255), 
				_ => new Color(92, 170, 255), 
			};
		}

		public static void farmerHeardSong(string trackName)
		{
			List<string> list = new List<string>();
			if (trackName.Equals("EarthMine"))
			{
				if (!Game1.player.songsHeard.Contains("Crystal Bells"))
				{
					list.Add("Crystal Bells");
				}
				if (!Game1.player.songsHeard.Contains("Cavern"))
				{
					list.Add("Cavern");
				}
				if (!Game1.player.songsHeard.Contains("Secret Gnomes"))
				{
					list.Add("Secret Gnomes");
				}
			}
			else if (trackName.Equals("FrostMine"))
			{
				if (!Game1.player.songsHeard.Contains("Cloth"))
				{
					list.Add("Cloth");
				}
				if (!Game1.player.songsHeard.Contains("Icicles"))
				{
					list.Add("Icicles");
				}
				if (!Game1.player.songsHeard.Contains("XOR"))
				{
					list.Add("XOR");
				}
			}
			else if (trackName.Equals("LavaMine"))
			{
				if (!Game1.player.songsHeard.Contains("Of Dwarves"))
				{
					list.Add("Of Dwarves");
				}
				if (!Game1.player.songsHeard.Contains("Near The Planet Core"))
				{
					list.Add("Near The Planet Core");
				}
				if (!Game1.player.songsHeard.Contains("Overcast"))
				{
					list.Add("Overcast");
				}
				if (!Game1.player.songsHeard.Contains("tribal"))
				{
					list.Add("tribal");
				}
			}
			else if (trackName.Equals("VolcanoMines"))
			{
				if (!Game1.player.songsHeard.Contains("VolcanoMines1"))
				{
					list.Add("VolcanoMines1");
				}
				if (!Game1.player.songsHeard.Contains("VolcanoMines2"))
				{
					list.Add("VolcanoMines2");
				}
			}
			else if (!trackName.Equals("none") && !trackName.Equals("rain"))
			{
				list.Add(trackName);
			}
			foreach (string item in list)
			{
				if (!Game1.player.songsHeard.Contains(item))
				{
					Game1.player.songsHeard.Add(item);
				}
			}
		}

		public static float getMaxedFriendshipPercent(Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			float num = 0f;
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			foreach (KeyValuePair<string, Friendship> pair in who.friendshipData.Pairs)
			{
				if (dictionary.ContainsKey(pair.Key))
				{
					Friendship value = pair.Value;
					bool flag = dictionary[pair.Key].Split('/')[5] == "datable";
					if (value.Points >= 250 * (flag ? 8 : 10))
					{
						num += 1f;
					}
				}
			}
			int num2 = dictionary.Count - 1;
			return num / (float)num2;
		}

		public static float getCookedRecipesPercent(Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\CookingRecipes");
			float num = 0f;
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				if (who.cookingRecipes.ContainsKey(item.Key))
				{
					int key = Convert.ToInt32(item.Value.Split('/')[2].Split(' ')[0]);
					if (who.recipesCooked.ContainsKey(key))
					{
						num += 1f;
					}
				}
			}
			return num / (float)dictionary.Count;
		}

		public static float getCraftedRecipesPercent(Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\CraftingRecipes");
			float num = 0f;
			foreach (string key in dictionary.Keys)
			{
				if (!(key == "Wedding Ring") && who.craftingRecipes.ContainsKey(key) && who.craftingRecipes[key] > 0)
				{
					num += 1f;
				}
			}
			return num / ((float)dictionary.Count - 1f);
		}

		public static float getFishCaughtPercent(Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			float num = 0f;
			float num2 = 0f;
			foreach (KeyValuePair<int, string> item in Game1.objectInformation)
			{
				if (item.Value.Split('/')[3].Contains("Fish") && (item.Key < 167 || item.Key > 172) && (item.Key < 898 || item.Key > 902))
				{
					num2 += 1f;
					if (who.fishCaught.ContainsKey(item.Key))
					{
						num += 1f;
					}
				}
			}
			return num / num2;
		}

		public static KeyValuePair<Farmer, bool> GetFarmCompletion(Func<Farmer, bool> check)
		{
			if (check(Game1.player))
			{
				return new KeyValuePair<Farmer, bool>(Game1.player, value: true);
			}
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer != Game1.player && allFarmer.isCustomized.Value && check(allFarmer))
				{
					return new KeyValuePair<Farmer, bool>(allFarmer, value: true);
				}
			}
			return new KeyValuePair<Farmer, bool>(Game1.player, value: false);
		}

		public static KeyValuePair<Farmer, float> GetFarmCompletion(Func<Farmer, float> check)
		{
			Farmer key = Game1.player;
			float num = check(Game1.player);
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer != Game1.player && allFarmer.isCustomized.Value)
				{
					float num2 = check(allFarmer);
					if (num2 > num)
					{
						key = allFarmer;
						num = num2;
					}
				}
			}
			return new KeyValuePair<Farmer, float>(key, num);
		}

		public static float percentGameComplete()
		{
			float num = 0f;
			float num2 = 0f;
			num2 += GetFarmCompletion((Farmer farmer) => getFarmerItemsShippedPercent(farmer)).Value * 15f;
			num += 15f;
			num2 += Math.Min(numObelisksOnFarm(), 4f);
			num += 4f;
			num2 += (float)(Game1.getFarm().isBuildingConstructed("Gold Clock") ? 10 : 0);
			num += 10f;
			num2 += (float)(GetFarmCompletion((Farmer farmer) => farmer.hasCompletedAllMonsterSlayerQuests.Value).Value ? 10 : 0);
			num += 10f;
			float value = GetFarmCompletion((Farmer farmer) => getMaxedFriendshipPercent(farmer)).Value;
			num2 += value * 11f;
			num += 11f;
			float value2 = GetFarmCompletion((Farmer farmer) => Math.Min(farmer.Level, 25f) / 25f).Value;
			num2 += value2 * 5f;
			num += 5f;
			num2 += (float)(GetFarmCompletion((Farmer farmer) => foundAllStardrops(farmer)).Value ? 10 : 0);
			num += 10f;
			num2 += GetFarmCompletion((Farmer farmer) => getCookedRecipesPercent(farmer)).Value * 10f;
			num += 10f;
			num2 += GetFarmCompletion((Farmer farmer) => getCraftedRecipesPercent(farmer)).Value * 10f;
			num += 10f;
			num2 += GetFarmCompletion((Farmer farmer) => getFishCaughtPercent(farmer)).Value * 10f;
			num += 10f;
			float num3 = 130f;
			float num4 = Math.Min((int)Game1.netWorldState.Value.GoldenWalnutsFound, num3);
			num2 += num4 / num3 * 5f;
			num += 5f;
			return num2 / num;
		}

		public static int numObelisksOnFarm()
		{
			return (Game1.getFarm().isBuildingConstructed("Water Obelisk") ? 1 : 0) + (Game1.getFarm().isBuildingConstructed("Earth Obelisk") ? 1 : 0) + (Game1.getFarm().isBuildingConstructed("Desert Obelisk") ? 1 : 0) + (Game1.getFarm().isBuildingConstructed("Island Obelisk") ? 1 : 0);
		}

		public static bool IsDesertLocation(GameLocation location)
		{
			if (location.Name == "Desert" || location.Name == "SkullCave" || location.Name == "Club" || location.Name == "SandyHouse" || location.Name == "SandyShop")
			{
				return true;
			}
			return false;
		}

		public static List<string> getOtherFarmerNames()
		{
			List<string> list = new List<string>();
			Random random = new Random((int)Game1.uniqueIDForThisGame);
			Random random2 = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
			string[] array = new string[33]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5499"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5500"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5501"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5502"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5503"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5504"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5505"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5506"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5507"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5508"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5509"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5510"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5511"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5512"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5513"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5514"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5515"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5516"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5517"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5518"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5519"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5520"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5521"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5522"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5523"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5524"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5525"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5526"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5527"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5528"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5529"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5530"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5531")
			};
			string[] array2 = new string[29]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5532"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5533"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5534"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5535"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5536"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5537"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5538"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5539"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5540"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5541"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5542"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5543"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5544"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5545"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5546"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5547"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5548"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5549"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5550"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5551"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5552"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5553"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5554"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5555"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5556"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5557"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5558"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5559"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5560")
			};
			string[] array3 = new string[17]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5561"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5562"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5563"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5564"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5565"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5566"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5567"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5568"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5569"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5570"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5571"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5572"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5573"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5574"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5575"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5576"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5577")
			};
			string[] array4 = new string[12]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5561"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5562"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5573"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5581"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5582"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5583"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5568"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5585"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5586"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5587"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5588"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5589")
			};
			string[] array5 = new string[28]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5590"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5591"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5592"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5593"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5594"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5595"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5596"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5597"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5598"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5599"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5600"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5601"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5602"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5603"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5604"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5605"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5606"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5607"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5608"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5609"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5610"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5611"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5612"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5613"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5614"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5615"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5616"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5617")
			};
			string[] array6 = new string[21]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5618"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5619"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5620"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5607"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5622"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5623"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5624"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5625"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5626"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5627"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5628"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5629"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5630"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5631"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5632"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5633"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5634"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5635"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5636"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5637"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5638")
			};
			string[] array7 = new string[9]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5639"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5640"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5641"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5642"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5643"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5644"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5645"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5646"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5647")
			};
			string[] array8 = new string[4]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5561"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5568"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5569"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5651")
			};
			string[] array9 = new string[4]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5561"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5568"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5585"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5655")
			};
			string text = "";
			if (Game1.player.IsMale)
			{
				text = array[random.Next(array.Length)];
				for (int i = 0; i < 2; i++)
				{
					while (list.Contains(text) || Game1.player.Name.Equals(text))
					{
						text = ((i != 0) ? array[random2.Next(array.Length)] : array[random.Next(array.Length)]);
					}
					text = ((i != 0) ? (array3[random2.Next(array3.Length)] + " " + text) : (array8[random.Next(array8.Length)] + " " + text));
					list.Add(text);
				}
			}
			else
			{
				text = array2[random.Next(array2.Length)];
				for (int j = 0; j < 2; j++)
				{
					while (list.Contains(text) || Game1.player.Name.Equals(text))
					{
						text = ((j != 0) ? array2[random2.Next(array2.Length)] : array2[random.Next(array2.Length)]);
					}
					text = ((j != 0) ? (array4[random2.Next(array4.Length)] + " " + text) : (array9[random.Next(array9.Length)] + " " + text));
					list.Add(text);
				}
			}
			if (random2.NextDouble() < 0.5)
			{
				text = array[random2.Next(array.Length)];
				while (Game1.player.Name.Equals(text))
				{
					text = array[random2.Next(array.Length)];
				}
				text = ((!(random2.NextDouble() < 0.5)) ? (text + " " + array7[random2.Next(array7.Length)]) : (array5[random2.Next(array5.Length)] + " " + text));
			}
			else
			{
				text = array2[random2.Next(array2.Length)];
				while (Game1.player.Name.Equals(text))
				{
					text = array2[random2.Next(array2.Length)];
				}
				text = array6[random2.Next(array6.Length)] + " " + text;
			}
			list.Add(text);
			return list;
		}

		public static string getStardewHeroStandingsString()
		{
			string text = "";
			Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
			List<string> otherFarmerNames = getOtherFarmerNames();
			int[] array = new int[otherFarmerNames.Count];
			array[0] = (int)((float)Game1.stats.DaysPlayed / 208f * (float)Game1.percentageToWinStardewHero);
			array[1] = (int)((float)array[0] * 0.75f + (float)random.Next(-5, 5));
			array[2] = Math.Max(0, array[1] / 2 + random.Next(-10, 0));
			if (Game1.stats.DaysPlayed < 30)
			{
				array[0] += 3;
			}
			else if (Game1.stats.DaysPlayed < 60)
			{
				array[0] += 7;
			}
			float num = percentGameComplete();
			bool flag = false;
			for (int i = 0; i < 3; i++)
			{
				if (num > (float)array[i] && !flag)
				{
					flag = true;
					text = text + Game1.player.getTitle() + " " + Game1.player.Name + " ....... " + num + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5657") + Environment.NewLine;
				}
				text = text + otherFarmerNames[i] + " ....... " + array[i] + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5657") + Environment.NewLine;
			}
			if (!flag)
			{
				text = text + Game1.player.getTitle() + " " + Game1.player.Name + " ....... " + num + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5657");
			}
			return text;
		}

		private static int cosmicFruitPercent()
		{
			return Math.Max(0, (Game1.player.MaxStamina - 120) / 20);
		}

		private static int minePercentage()
		{
			if (Game1.player.timesReachedMineBottom > 0)
			{
				return 4;
			}
			if (MineShaft.lowestLevelReached >= 80)
			{
				return 2;
			}
			if (MineShaft.lowestLevelReached >= 40)
			{
				return 1;
			}
			return 0;
		}

		private static int cookingPercent()
		{
			int num = 0;
			foreach (string key in Game1.player.cookingRecipes.Keys)
			{
				if (Game1.player.cookingRecipes[key] > 0)
				{
					num++;
				}
			}
			return (int)((float)(num / Game1.content.Load<Dictionary<string, string>>("Data\\CookingRecipes").Count) * 3f);
		}

		private static int craftingPercent()
		{
			int num = 0;
			foreach (string key in Game1.player.craftingRecipes.Keys)
			{
				if (Game1.player.craftingRecipes[key] > 0)
				{
					num++;
				}
			}
			return (int)((float)(num / Game1.content.Load<Dictionary<string, string>>("Data\\CraftingRecipes").Count) * 3f);
		}

		private static int achievementsPercent()
		{
			return (int)((float)(Game1.player.achievements.Count / Game1.content.Load<Dictionary<int, string>>("Data\\achievements").Count) * 15f);
		}

		private static int itemsShippedPercent()
		{
			return (int)((float)Game1.player.basicShipped.Count() / 92f * 5f);
		}

		private static int artifactsPercent()
		{
			return (int)((float)Game1.player.archaeologyFound.Count() / 32f * 3f);
		}

		private static int fishPercent()
		{
			return (int)((float)Game1.player.fishCaught.Count() / 42f * 3f);
		}

		private static int upgradePercent()
		{
			int num = 0;
			foreach (Item item in Game1.player.items)
			{
				if (item != null && item is Tool && (item.Name.Contains("Hoe") || item.Name.Contains("Axe") || item.Name.Contains("Pickaxe") || item.Name.Contains("Can")) && (int)((Tool)item).upgradeLevel == 4)
				{
					num++;
				}
			}
			num += Game1.player.HouseUpgradeLevel;
			num += Game1.player.CoopUpgradeLevel;
			num += Game1.player.BarnUpgradeLevel;
			if (Game1.player.hasGreenhouse)
			{
				num++;
			}
			return num;
		}

		private static int friendshipPercent()
		{
			int num = 0;
			foreach (string key in Game1.player.friendshipData.Keys)
			{
				num += Game1.player.friendshipData[key].Points;
			}
			return Math.Min(10, (int)((float)num / 70000f * 10f));
		}

		private static bool playerHasGalaxySword()
		{
			foreach (Item item in Game1.player.Items)
			{
				if (item != null && item is Sword && item.Name.Contains("Galaxy"))
				{
					return true;
				}
			}
			return false;
		}

		public static int getTrashReclamationPrice(Item i, Farmer f, int stack = -1)
		{
			float num = 0.15f * (float)f.trashCanLevel;
			if (i != null && i.canBeTrashed())
			{
				if (i is Wallpaper || i is Furniture)
				{
					return -1;
				}
				if (i is Object && !(i as Object).bigCraftable)
				{
					if (stack == -1)
					{
						return (int)((float)i.Stack * ((float)(i as Object).sellToStorePrice(-1L) * num));
					}
					return (int)((float)stack * ((float)(i as Object).sellToStorePrice(-1L) * num));
				}
				if (i is MeleeWeapon || i is Ring || i is Boots)
				{
					return (int)((float)i.Stack * ((float)(i.salePrice() / 2) * num));
				}
			}
			return -1;
		}

		public static Quest getQuestOfTheDay()
		{
			if (Game1.stats.DaysPlayed <= 1)
			{
				return null;
			}
			Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
			Quest quest = null;
			double num = random.NextDouble();
			if (num < 0.08)
			{
				return new ResourceCollectionQuest();
			}
			if (num < 0.18 && MineShaft.lowestLevelReached > 0 && Game1.stats.DaysPlayed > 5)
			{
				return new SlayMonsterQuest();
			}
			if (num < 0.53)
			{
				return null;
			}
			if (num < 0.6)
			{
				return new FishingQuest();
			}
			return new ItemDeliveryQuest();
		}

		public static Color getOppositeColor(Color color)
		{
			return new Color(255 - color.R, 255 - color.G, 255 - color.B);
		}

		public static void drawLightningBolt(Vector2 strikePosition, GameLocation l)
		{
			Microsoft.Xna.Framework.Rectangle sourceRect = new Microsoft.Xna.Framework.Rectangle(644, 1078, 37, 57);
			Vector2 position = strikePosition + new Vector2(-sourceRect.Width * 4 / 2, -sourceRect.Height * 4);
			while (position.Y > (float)(-sourceRect.Height * 4))
			{
				l.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, 9999f, 1, 999, position, flicker: false, Game1.random.NextDouble() < 0.5, (strikePosition.Y + 32f) / 10000f + 0.001f, 0.025f, Color.White, 4f, 0f, 0f, 0f)
				{
					light = true,
					lightRadius = 2f,
					delayBeforeAnimationStart = 200,
					lightcolor = Color.Black
				});
				position.Y -= sourceRect.Height * 4;
			}
		}

		public static string getDateStringFor(int currentDay, int currentSeason, int currentYear)
		{
			if (currentDay <= 0)
			{
				currentDay += 28;
				currentSeason--;
				if (currentSeason < 0)
				{
					currentSeason = 3;
					currentYear--;
				}
			}
			else if (currentDay > 28)
			{
				currentDay -= 28;
				currentSeason++;
				if (currentSeason > 3)
				{
					currentSeason = 0;
					currentYear++;
				}
			}
			if (currentYear == 0)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5677");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5678", currentDay, (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es) ? getSeasonNameFromNumber(currentSeason).ToLower() : getSeasonNameFromNumber(currentSeason), currentYear);
		}

		public static string getDateString(int offset = 0)
		{
			int dayOfMonth = Game1.dayOfMonth;
			int seasonNumber = getSeasonNumber(Game1.currentSeason);
			int year = Game1.year;
			dayOfMonth += offset;
			return getDateStringFor(dayOfMonth, seasonNumber, year);
		}

		public static string getYesterdaysDate()
		{
			return getDateString(-1);
		}

		public static string getSeasonNameFromNumber(int number)
		{
			return number switch
			{
				0 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5680"), 
				1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5681"), 
				2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5682"), 
				3 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5683"), 
				_ => "", 
			};
		}

		public static string getNumberEnding(int number)
		{
			if (number % 100 > 10 && number % 100 < 20)
			{
				return "th";
			}
			switch (number % 10)
			{
			case 0:
			case 4:
			case 5:
			case 6:
			case 7:
			case 8:
			case 9:
				return "th";
			case 1:
				return "st";
			case 2:
				return "nd";
			case 3:
				return "rd";
			default:
				return "";
			}
		}

		public static void killAllStaticLoopingSoundCues()
		{
			if (Game1.soundBank != null)
			{
				if (Intro.roadNoise != null)
				{
					Intro.roadNoise.Stop(AudioStopOptions.Immediate);
				}
				if (Railroad.trainLoop != null)
				{
					Railroad.trainLoop.Stop(AudioStopOptions.Immediate);
				}
				if (BobberBar.reelSound != null)
				{
					BobberBar.reelSound.Stop(AudioStopOptions.Immediate);
				}
				if (BobberBar.unReelSound != null)
				{
					BobberBar.unReelSound.Stop(AudioStopOptions.Immediate);
				}
				if (FishingRod.reelSound != null)
				{
					FishingRod.reelSound.Stop(AudioStopOptions.Immediate);
				}
			}
			Game1.locationCues.StopAll();
		}

		public static void consolidateStacks(IList<Item> objects)
		{
			for (int i = 0; i < objects.Count; i++)
			{
				if (objects[i] == null || !(objects[i] is Object))
				{
					continue;
				}
				Object @object = objects[i] as Object;
				for (int j = i + 1; j < objects.Count; j++)
				{
					if (objects[j] != null && @object.canStackWith(objects[j]))
					{
						@object.Stack = objects[j].addToStack(@object);
						if (@object.Stack <= 0)
						{
							break;
						}
					}
				}
			}
			for (int num = objects.Count - 1; num >= 0; num--)
			{
				if (objects[num] != null && objects[num].Stack <= 0)
				{
					objects.RemoveAt(num);
				}
			}
		}

		public static void performLightningUpdate(int time_of_day)
		{
			Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + time_of_day);
			if (random.NextDouble() < 0.125 + Game1.player.team.AverageDailyLuck() + Game1.player.team.AverageLuckLevel() / 100.0)
			{
				Farm.LightningStrikeEvent lightningStrikeEvent = new Farm.LightningStrikeEvent();
				lightningStrikeEvent.bigFlash = true;
				Farm farm = Game1.getLocationFromName("Farm") as Farm;
				List<Vector2> list = new List<Vector2>();
				foreach (KeyValuePair<Vector2, Object> pair in farm.objects.Pairs)
				{
					if ((bool)pair.Value.bigCraftable && pair.Value.ParentSheetIndex == 9)
					{
						list.Add(pair.Key);
					}
				}
				if (list.Count > 0)
				{
					for (int i = 0; i < 2; i++)
					{
						Vector2 vector = list.ElementAt(random.Next(list.Count));
						if (farm.objects[vector].heldObject.Value == null)
						{
							farm.objects[vector].heldObject.Value = new Object(787, 1);
							farm.objects[vector].minutesUntilReady.Value = CalculateMinutesUntilMorning(Game1.timeOfDay);
							farm.objects[vector].shakeTimer = 1000;
							lightningStrikeEvent.createBolt = true;
							lightningStrikeEvent.boltPosition = vector * 64f + new Vector2(32f, 0f);
							farm.lightningStrikeEvent.Fire(lightningStrikeEvent);
							return;
						}
					}
				}
				if (random.NextDouble() < 0.25 - Game1.player.team.AverageDailyLuck() - Game1.player.team.AverageLuckLevel() / 100.0)
				{
					try
					{
						KeyValuePair<Vector2, TerrainFeature> keyValuePair = farm.terrainFeatures.Pairs.ElementAt(random.Next(farm.terrainFeatures.Count()));
						if (!(keyValuePair.Value is FruitTree))
						{
							bool flag = keyValuePair.Value is HoeDirt && (keyValuePair.Value as HoeDirt).crop != null && !(keyValuePair.Value as HoeDirt).crop.dead;
							if (keyValuePair.Value.performToolAction(null, 50, keyValuePair.Key, farm))
							{
								lightningStrikeEvent.destroyedTerrainFeature = true;
								lightningStrikeEvent.createBolt = true;
								farm.terrainFeatures.Remove(keyValuePair.Key);
								lightningStrikeEvent.boltPosition = keyValuePair.Key * 64f + new Vector2(32f, -128f);
							}
							if (flag && keyValuePair.Value is HoeDirt && (keyValuePair.Value as HoeDirt).crop != null && (bool)(keyValuePair.Value as HoeDirt).crop.dead)
							{
								lightningStrikeEvent.createBolt = true;
								lightningStrikeEvent.boltPosition = keyValuePair.Key * 64f + new Vector2(32f, 0f);
							}
						}
						else if (keyValuePair.Value is FruitTree)
						{
							(keyValuePair.Value as FruitTree).struckByLightningCountdown.Value = 4;
							(keyValuePair.Value as FruitTree).shake(keyValuePair.Key, doEvenIfStillShaking: true, farm);
							lightningStrikeEvent.createBolt = true;
							lightningStrikeEvent.boltPosition = keyValuePair.Key * 64f + new Vector2(32f, -128f);
						}
					}
					catch (Exception)
					{
					}
				}
				farm.lightningStrikeEvent.Fire(lightningStrikeEvent);
			}
			else if (random.NextDouble() < 0.1)
			{
				Farm.LightningStrikeEvent lightningStrikeEvent2 = new Farm.LightningStrikeEvent();
				lightningStrikeEvent2.smallFlash = true;
				Farm farm = Game1.getLocationFromName("Farm") as Farm;
				farm.lightningStrikeEvent.Fire(lightningStrikeEvent2);
			}
		}

		public static void overnightLightning()
		{
			if (Game1.IsMasterGame)
			{
				int num = (2300 - Game1.timeOfDay) / 100;
				for (int i = 1; i <= num; i++)
				{
					performLightningUpdate(Game1.timeOfDay + i * 100);
				}
			}
		}

		public static List<Vector2> getAdjacentTileLocations(Vector2 tileLocation)
		{
			List<Vector2> list = new List<Vector2>();
			list.Add(new Vector2(-1f, 0f) + tileLocation);
			list.Add(new Vector2(1f, 0f) + tileLocation);
			list.Add(new Vector2(0f, 1f) + tileLocation);
			list.Add(new Vector2(0f, -1f) + tileLocation);
			return list;
		}

		public static List<Point> getAdjacentTilePoints(float xTile, float yTile)
		{
			List<Point> list = new List<Point>();
			int num = (int)xTile;
			int num2 = (int)yTile;
			list.Add(new Point(-1 + num, num2));
			list.Add(new Point(1 + num, num2));
			list.Add(new Point(num, 1 + num2));
			list.Add(new Point(num, -1 + num2));
			return list;
		}

		public static Vector2[] getAdjacentTileLocationsArray(Vector2 tileLocation)
		{
			return new Vector2[4]
			{
				new Vector2(-1f, 0f) + tileLocation,
				new Vector2(1f, 0f) + tileLocation,
				new Vector2(0f, 1f) + tileLocation,
				new Vector2(0f, -1f) + tileLocation
			};
		}

		public static Vector2[] getDiagonalTileLocationsArray(Vector2 tileLocation)
		{
			return new Vector2[4]
			{
				new Vector2(-1f, -1f) + tileLocation,
				new Vector2(1f, 1f) + tileLocation,
				new Vector2(-1f, 1f) + tileLocation,
				new Vector2(1f, -1f) + tileLocation
			};
		}

		public static Vector2[] getSurroundingTileLocationsArray(Vector2 tileLocation)
		{
			return new Vector2[8]
			{
				new Vector2(-1f, 0f) + tileLocation,
				new Vector2(1f, 0f) + tileLocation,
				new Vector2(0f, 1f) + tileLocation,
				new Vector2(0f, -1f) + tileLocation,
				new Vector2(-1f, -1f) + tileLocation,
				new Vector2(1f, -1f) + tileLocation,
				new Vector2(1f, 1f) + tileLocation,
				new Vector2(-1f, 1f) + tileLocation
			};
		}

		public static Crop findCloseFlower(GameLocation location, Vector2 startTileLocation)
		{
			return findCloseFlower(location, startTileLocation, -1, null);
		}

		public static Crop findCloseFlower(GameLocation location, Vector2 startTileLocation, int range = -1, Func<Crop, bool> additional_check = null)
		{
			Queue<Vector2> queue = new Queue<Vector2>();
			HashSet<Vector2> hashSet = new HashSet<Vector2>();
			queue.Enqueue(startTileLocation);
			for (int i = 0; range >= 0 || (range < 0 && i <= 150); i++)
			{
				if (queue.Count <= 0)
				{
					break;
				}
				Vector2 vector = queue.Dequeue();
				if (location.terrainFeatures.ContainsKey(vector) && location.terrainFeatures[vector] is HoeDirt && (location.terrainFeatures[vector] as HoeDirt).crop != null && new Object((location.terrainFeatures[vector] as HoeDirt).crop.indexOfHarvest.Value, 1).Category == -80 && (int)(location.terrainFeatures[vector] as HoeDirt).crop.currentPhase >= (location.terrainFeatures[vector] as HoeDirt).crop.phaseDays.Count - 1 && !(location.terrainFeatures[vector] as HoeDirt).crop.dead && (additional_check == null || additional_check((location.terrainFeatures[vector] as HoeDirt).crop)))
				{
					return (location.terrainFeatures[vector] as HoeDirt).crop;
				}
				foreach (Vector2 adjacentTileLocation in getAdjacentTileLocations(vector))
				{
					if (!hashSet.Contains(adjacentTileLocation) && (range < 0 || Math.Abs(adjacentTileLocation.X - startTileLocation.X) + Math.Abs(adjacentTileLocation.Y - startTileLocation.Y) <= (float)range))
					{
						queue.Enqueue(adjacentTileLocation);
					}
				}
				hashSet.Add(vector);
			}
			return null;
		}

		public static Point findCloseMatureCrop(Vector2 startTileLocation)
		{
			Queue<Vector2> queue = new Queue<Vector2>();
			HashSet<Vector2> hashSet = new HashSet<Vector2>();
			Farm farm = Game1.getLocationFromName("Farm") as Farm;
			queue.Enqueue(startTileLocation);
			for (int i = 0; i <= 40; i++)
			{
				if (queue.Count() <= 0)
				{
					break;
				}
				Vector2 vector = queue.Dequeue();
				if (farm.terrainFeatures.ContainsKey(vector) && farm.terrainFeatures[vector] is HoeDirt && (farm.terrainFeatures[vector] as HoeDirt).crop != null && (farm.terrainFeatures[vector] as HoeDirt).readyForHarvest())
				{
					return Vector2ToPoint(vector);
				}
				foreach (Vector2 adjacentTileLocation in getAdjacentTileLocations(vector))
				{
					if (!hashSet.Contains(adjacentTileLocation))
					{
						queue.Enqueue(adjacentTileLocation);
					}
				}
				hashSet.Add(vector);
			}
			return Point.Zero;
		}

		public static void recursiveFenceBuild(Vector2 position, int direction, GameLocation location, Random r)
		{
			if (!(r.NextDouble() < 0.04) && !location.objects.ContainsKey(position) && location.isTileLocationOpen(new Location((int)position.X, (int)position.Y)))
			{
				location.objects.Add(position, new Fence(position, 1, isGate: false));
				int num = direction;
				if (r.NextDouble() < 0.16)
				{
					num = r.Next(4);
				}
				if (num == (direction + 2) % 4)
				{
					num = (num + 1) % 4;
				}
				switch (direction)
				{
				case 0:
					recursiveFenceBuild(position + new Vector2(0f, -1f), num, location, r);
					break;
				case 1:
					recursiveFenceBuild(position + new Vector2(1f, 0f), num, location, r);
					break;
				case 3:
					recursiveFenceBuild(position + new Vector2(-1f, 0f), num, location, r);
					break;
				case 2:
					recursiveFenceBuild(position + new Vector2(0f, 1f), num, location, r);
					break;
				}
			}
		}

		public static bool addAnimalToFarm(FarmAnimal animal)
		{
			if (animal == null || animal.Sprite == null)
			{
				return false;
			}
			Farm farm = (Farm)Game1.getLocationFromName("Farm");
			foreach (Building building in farm.buildings)
			{
				if (building.buildingType.Contains(animal.buildingTypeILiveIn) && !((AnimalHouse)(GameLocation)building.indoors).isFull())
				{
					((AnimalHouse)(GameLocation)building.indoors).animals.Add(animal.myID, animal);
					((AnimalHouse)(GameLocation)building.indoors).animalsThatLiveHere.Add(animal.myID);
					animal.home = building;
					animal.setRandomPosition(building.indoors);
					return true;
				}
			}
			return false;
		}

		public static Item getItemFromStandardTextDescription(string description, Farmer who, char delimiter = ' ')
		{
			if (string.IsNullOrEmpty(description))
			{
				return null;
			}
			string[] array = description.Split(delimiter);
			string text = array[0];
			int num = Convert.ToInt32(array[1]);
			int stack = Convert.ToInt32(array[2]);
			Item item = null;
			if (text != null)
			{
				switch (text.Length)
				{
				case 9:
				{
					char c = text[1];
					if (c != 'i')
					{
						if (c != 'l')
						{
							if (c != 'u' || !(text == "Furniture"))
							{
								break;
							}
							goto IL_028f;
						}
						if (!(text == "Blueprint"))
						{
							break;
						}
						goto IL_02df;
					}
					if (!(text == "BigObject"))
					{
						break;
					}
					goto IL_02b1;
				}
				case 1:
					switch (text[0])
					{
					case 'F':
						break;
					case 'O':
						goto IL_02a3;
					case 'R':
						goto IL_02c1;
					case 'B':
						goto IL_02cb;
					case 'W':
						goto IL_02d5;
					case 'H':
						goto IL_02ed;
					case 'C':
						item = new Clothing(num);
						goto end_IL_003e;
					default:
						goto end_IL_003e;
					}
					goto IL_028f;
				case 6:
				{
					char c = text[0];
					if (c != 'O')
					{
						if (c != 'W' || !(text == "Weapon"))
						{
							break;
						}
						goto IL_02d5;
					}
					if (!(text == "Object"))
					{
						break;
					}
					goto IL_02a3;
				}
				case 2:
				{
					char c = text[1];
					if (c != 'L')
					{
						if (c != 'O' || !(text == "BO"))
						{
							break;
						}
						goto IL_02b1;
					}
					if (!(text == "BL"))
					{
						break;
					}
					goto IL_02df;
				}
				case 4:
				{
					char c = text[0];
					if (c != 'B')
					{
						if (c != 'R' || !(text == "Ring"))
						{
							break;
						}
						goto IL_02c1;
					}
					if (!(text == "Boot"))
					{
						break;
					}
					goto IL_02cb;
				}
				case 3:
				{
					char c = text[2];
					if (c != 'L')
					{
						if (c != 'l')
						{
							if (c != 't' || !(text == "Hat"))
							{
								break;
							}
							goto IL_02ed;
						}
						if (!(text == "BBl"))
						{
							break;
						}
					}
					else if (!(text == "BBL"))
					{
						break;
					}
					goto IL_02f7;
				}
				case 12:
					{
						if (!(text == "BigBlueprint"))
						{
							break;
						}
						goto IL_02f7;
					}
					IL_02b1:
					item = new Object(Vector2.Zero, num);
					break;
					IL_02df:
					item = new Object(num, 1, isRecipe: true);
					break;
					IL_028f:
					item = Furniture.GetFurnitureInstance(num, Vector2.Zero);
					break;
					IL_02f7:
					item = new Object(Vector2.Zero, num, isRecipe: true);
					break;
					IL_02ed:
					item = new Hat(num);
					break;
					IL_02cb:
					item = new Boots(num);
					break;
					IL_02d5:
					item = new MeleeWeapon(num);
					break;
					IL_02c1:
					item = new Ring(num);
					break;
					IL_02a3:
					item = new Object(num, 1);
					break;
					end_IL_003e:
					break;
				}
			}
			item.Stack = stack;
			if (who != null && item is Object && (bool)(item as Object).isRecipe && who.knowsRecipe(item.Name))
			{
				return null;
			}
			return item;
		}

		public static string getStandardDescriptionFromItem(Item item, int stack, char delimiter = ' ')
		{
			string text = "";
			int num = item.parentSheetIndex.Value;
			if (item is Furniture)
			{
				text = "F";
			}
			else if (item is Object)
			{
				Object @object = item as Object;
				text = (@object.bigCraftable.Value ? ((!@object.IsRecipe) ? "BO" : "BBL") : ((!@object.IsRecipe) ? "O" : "BL"));
			}
			else if (item is Ring)
			{
				text = "R";
			}
			else if (item is Boots boots)
			{
				text = "B";
				num = boots.indexInTileSheet.Value;
			}
			else if (item is MeleeWeapon meleeWeapon)
			{
				text = "W";
				num = meleeWeapon.CurrentParentTileIndex;
			}
			else if (item is Hat hat)
			{
				text = "H";
				num = hat.which.Value;
			}
			else if (item is Clothing)
			{
				text = "C";
			}
			return text + delimiter + num + delimiter + stack;
		}

		public static List<TemporaryAnimatedSprite> sparkleWithinArea(Microsoft.Xna.Framework.Rectangle bounds, int numberOfSparkles, Color sparkleColor, int delayBetweenSparkles = 100, int delayBeforeStarting = 0, string sparkleSound = "")
		{
			return getTemporarySpritesWithinArea(new int[2] { 10, 11 }, bounds, numberOfSparkles, sparkleColor, delayBetweenSparkles, delayBeforeStarting, sparkleSound);
		}

		public static List<TemporaryAnimatedSprite> getTemporarySpritesWithinArea(int[] temporarySpriteRowNumbers, Microsoft.Xna.Framework.Rectangle bounds, int numberOfsprites, Color color, int delayBetweenSprites = 100, int delayBeforeStarting = 0, string sound = "")
		{
			List<TemporaryAnimatedSprite> list = new List<TemporaryAnimatedSprite>();
			for (int i = 0; i < numberOfsprites; i++)
			{
				list.Add(new TemporaryAnimatedSprite(temporarySpriteRowNumbers[Game1.random.Next(temporarySpriteRowNumbers.Length)], new Vector2(Game1.random.Next(bounds.X, bounds.Right), Game1.random.Next(bounds.Y, bounds.Bottom)), color)
				{
					delayBeforeAnimationStart = delayBeforeStarting + delayBetweenSprites * i,
					startSound = ((sound.Length > 0) ? sound : null)
				});
			}
			return list;
		}

		public static Vector2 getAwayFromPlayerTrajectory(Microsoft.Xna.Framework.Rectangle monsterBox, Farmer who)
		{
			Vector2 result = new Vector2(-(who.GetBoundingBox().Center.X - monsterBox.Center.X), who.GetBoundingBox().Center.Y - monsterBox.Center.Y);
			if (result.Length() <= 0f)
			{
				if (who.FacingDirection == 3)
				{
					result = new Vector2(-1f, 0f);
				}
				else if (who.FacingDirection == 1)
				{
					result = new Vector2(1f, 0f);
				}
				else if (who.FacingDirection == 0)
				{
					result = new Vector2(0f, 1f);
				}
				else if (who.FacingDirection == 2)
				{
					result = new Vector2(0f, -1f);
				}
			}
			result.Normalize();
			result.X *= 50 + Game1.random.Next(-20, 20);
			result.Y *= 50 + Game1.random.Next(-20, 20);
			return result;
		}

		public static string getSongTitleFromCueName(string cueName)
		{
			string text = cueName.ToLower();
			if (text != null)
			{
				switch (text.Length)
				{
				case 8:
					switch (text[2])
					{
					case 'r':
						if (!(text == "turn_off"))
						{
							if (!(text == "aerobics"))
							{
								break;
							}
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5730");
						}
						return Game1.content.LoadString("Strings\\UI:Mini_JukeBox_Off");
					case 'l':
						if (!(text == "fallfest"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5766");
					case 'c':
						if (!(text == "ticktock"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5782");
					case 'd':
						if (!(text == "sadpiano"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5804");
					case 'e':
						if (!(text == "overcast"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5810");
					case 's':
						if (!(text == "desolate"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5845");
					case 'o':
						if (!(text == "frogcave"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:FrogCave");
					}
					break;
				case 7:
					switch (text[6])
					{
					case '1':
						switch (text)
						{
						case "spring1":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5718");
						case "winter1":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5732");
						case "summer1":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5740");
						case "saloon1":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5802");
						}
						break;
					case '2':
						switch (text)
						{
						case "spring2":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5720");
						case "winter2":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5734");
						case "summer2":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5742");
						}
						break;
					case '3':
						switch (text)
						{
						case "spring3":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5722");
						case "winter3":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5736");
						case "summer3":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5744");
						}
						break;
					case 'e':
						if (!(text == "ragtime"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5864_1");
					case 's':
						if (!(text == "icicles"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5768");
					case 'g':
						if (!(text == "wedding"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5776");
					case 'l':
						if (!(text == "playful"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5808");
					case 'm':
						if (!(text == "sunroom"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:SunRoom");
					case 'a':
						if (!(text == "caldera"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:caldera");
					case 'd':
						if (!(text == "sad_kid"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:sad_kid");
					}
					break;
				case 3:
					switch (text[0])
					{
					case '5':
						if (!(text == "50s"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5724");
					case 'x':
						if (!(text == "xor"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5770");
					}
					break;
				case 12:
					switch (text[0])
					{
					case 'a':
						if (!(text == "abigailflute"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5726");
					case 'c':
						if (!(text == "cloudcountry"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5758");
					case 't':
						if (!(text == "tinymusicbox"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5780");
					case 'm':
						switch (text)
						{
						case "marlonstheme":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5816");
						case "musicboxsong":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5864_8");
						case "movie_nature":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
						case "movie_wumbus":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
						case "movietheater":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
						}
						break;
					case 'l':
						if (!(text == "librarytheme"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5823");
					case 'e':
						if (!(text == "elliottpiano"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5839");
					case 'd':
						if (!(text == "distantbanjo"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5843");
					case 'b':
						if (!(text == "buglevelloop"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5864_9");
					case 'n':
						if (!(text == "night_market"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\UI:Billboard_NightMarket");
					case 'p':
						if (!(text == "pirate_theme"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:PIRATE_THEME");
					}
					break;
				case 16:
					switch (text[0])
					{
					case 'a':
						if (!(text == "abigailfluteduet"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5728");
					case 'm':
						if (!(text == "moonlightjellies"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5812");
					case 'c':
						if (!(text == "cowboy_overworld"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5849");
					}
					break;
				case 20:
					switch (text[0])
					{
					case 'n':
						if (!(text == "near the planet core"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5738");
					case 'j':
						if (!(text == "jojaofficesoundscape"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5863");
					case 'f':
						if (!(text == "fieldofficetentmusic"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:fieldOfficeTentMusic");
					}
					break;
				case 5:
					switch (text[4])
					{
					case '1':
						if (!(text == "fall1"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5746");
					case '2':
						if (!(text == "fall2"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5748");
					case '3':
						if (!(text == "fall3"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5750");
					case 'h':
						if (!(text == "cloth"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5756");
					case 'y':
						if (!(text == "heavy"))
						{
							if (!(text == "poppy"))
							{
								break;
							}
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5806");
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5794");
					case 't':
						if (!(text == "sweet"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5864_2");
					case 's':
						if (!(text == "echos"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5841");
					}
					break;
				case 6:
					switch (text[0])
					{
					case 'b':
						if (!(text == "breezy"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5752");
					case 't':
						if (!(text == "tribal"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5778");
					case 'j':
						if (!(text == "jaunty"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5829");
					case 'e':
						if (!(text == "event1"))
						{
							if (!(text == "event2"))
							{
								break;
							}
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5837");
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5835");
					case 'c':
						if (!(text == "cavern"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5859");
					case 'r':
						if (!(text == "random"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:JukeboxRandomTrack");
					}
					break;
				case 14:
					switch (text[2])
					{
					case 'r':
						if (!(text == "christmasTheme"))
						{
							if (!(text == "christmastheme"))
							{
								break;
							}
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5864_7");
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5754");
					case 'a':
						if (!(text == "grandpas_theme"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5762");
					case 'n':
						if (!(text == "junimostarsong"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5825");
					case 'w':
						if (!(text == "cowboy_singing"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5857");
					case 'b':
						if (!(text == "submarine_song"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.721");
					}
					break;
				case 10:
					switch (text[8])
					{
					case 'e':
						if (!(text == "of dwarves"))
						{
							goto end_IL_0017;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5760");
					case 'n':
						if (!(text == "wizardsong"))
						{
							goto end_IL_0017;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5772");
					case 'm':
						break;
					case 'w':
						if (!(text == "springtown"))
						{
							goto end_IL_0017;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5786");
					case 'i':
						goto IL_0a3b;
					case 'o':
						if (!(text == "marnieshop"))
						{
							goto end_IL_0017;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5814");
					case 'k':
						if (!(text == "honkytonky"))
						{
							goto end_IL_0017;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5831");
					case 'c':
						if (!(text == "emilydance"))
						{
							goto end_IL_0017;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5864_5");
					case 'a':
						if (!(text == "emilydream"))
						{
							goto end_IL_0017;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5864_6");
					case 'r':
						goto IL_0ab4;
					default:
						goto end_IL_0017;
					}
					switch (text)
					{
					case "woodstheme":
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5774");
					case "shanetheme":
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5864_3");
					case "emilytheme":
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5864_4");
					case "crane_game":
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
					}
					break;
				case 11:
					switch (text[1])
					{
					case 'l':
						if (!(text == "flowerdance"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5764");
					case 'p':
						if (!(text == "spirits_eve"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5788");
					case 'a':
						if (!(text == "sampractice"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5800");
					case 'i':
						if (!(text == "title_night"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5821");
					case 'o':
						if (!(text == "cowboy_boss"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5855");
					case 'e':
						if (!(text == "mermaidsong"))
						{
							break;
						}
						return Game1.content.LoadString("strings\\StringsFromCSFiles:Dialogue.cs.718");
					case 's':
						if (!(text == "islandmusic"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:IslandMusic");
					case 'n':
						if (!(text == "end_credits"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:EndCredits_SongName");
					}
					break;
				case 9:
				{
					char c = text[0];
					if ((uint)c <= 109u)
					{
						if (c == 'g')
						{
							if (!(text == "gusviolin"))
							{
								break;
							}
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5833");
						}
						if (c != 'm' || !(text == "maintheme"))
						{
							break;
						}
					}
					else
					{
						if (c == 's')
						{
							if (!(text == "starshoot"))
							{
								break;
							}
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5784");
						}
						if (c != 't' || !(text == "title_day"))
						{
							break;
						}
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5819");
				}
				case 17:
					switch (text[0])
					{
					case 's':
						if (!(text == "shimmeringbastion"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5792");
					case 'c':
						if (!(text == "cowboy_outlawsong"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5851");
					case 'm':
						if (!(text == "movietheaterafter"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
					}
					break;
				case 13:
					switch (text[2])
					{
					case 'c':
						if (!(text == "secret gnomes"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5798");
					case 'y':
						if (!(text == "crystal bells"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5847");
					case 'w':
						if (!(text == "cowboy_undead"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5853");
					case 'm':
						if (!(text == "sam_acoustic1"))
						{
							if (!(text == "sam_acoustic2"))
							{
								break;
							}
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
					case 'v':
						if (!(text == "movie_classic"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
					case 'l':
						if (!(text == "volcanomines1"))
						{
							if (!(text == "volcanomines2"))
							{
								break;
							}
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:VolcanoMines2");
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:VolcanoMines1");
					}
					break;
				case 15:
				{
					char c = text[0];
					if (c != 'c')
					{
						if (c != 'k' || !(text == "kindadumbautumn"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5827");
					}
					if (!(text == "crane_game_fast"))
					{
						break;
					}
					goto IL_13b2;
				}
				case 21:
				{
					char c = text[11];
					if ((uint)c <= 103u)
					{
						if (c == 'e')
						{
							if (!(text == "pirate_theme(muffled)"))
							{
								break;
							}
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:PIRATE_THEME_MUFFLED");
						}
						if (c != 'g' || !(text == "junimokart_ghostmusic"))
						{
							break;
						}
					}
					else if (c != 's')
					{
						if (c != 'w' || !(text == "junimokart_whalemusic"))
						{
							break;
						}
					}
					else if (!(text == "junimokart_slimemusic"))
					{
						break;
					}
					goto IL_13b2;
				}
				case 4:
					if (!(text == "wavy"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5861");
				case 18:
					if (!(text == "harveys_theme_jazz"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
				case 24:
					{
						if (!(text == "junimokart_mushroommusic"))
						{
							break;
						}
						goto IL_13b2;
					}
					IL_0a3b:
					if (!(text == "spacemusic"))
					{
						if (!(text == "settlingin"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5796");
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5790");
					IL_13b2:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
					IL_0ab4:
					if (!(text == "junimokart"))
					{
						break;
					}
					goto IL_13b2;
					end_IL_0017:
					break;
				}
			}
			return cueName;
		}

		public static bool isOffScreenEndFunction(PathNode currentNode, Point endPoint, GameLocation location, Character c)
		{
			if (!isOnScreen(new Vector2(currentNode.x * 64, currentNode.y * 64), 32))
			{
				return true;
			}
			return false;
		}

		public static Vector2 getAwayFromPositionTrajectory(Microsoft.Xna.Framework.Rectangle monsterBox, Vector2 position)
		{
			float num = 0f - (position.X - (float)monsterBox.Center.X);
			float num2 = position.Y - (float)monsterBox.Center.Y;
			float num3 = Math.Abs(num) + Math.Abs(num2);
			if (num3 < 1f)
			{
				num3 = 5f;
			}
			num = num / num3 * 20f;
			num2 = num2 / num3 * 20f;
			return new Vector2(num, num2);
		}

		public static bool tileWithinRadiusOfPlayer(int xTile, int yTile, int tileRadius, Farmer f)
		{
			Point point = new Point(xTile, yTile);
			Vector2 tileLocation = f.getTileLocation();
			if (Math.Abs((float)point.X - tileLocation.X) <= (float)tileRadius)
			{
				return Math.Abs((float)point.Y - tileLocation.Y) <= (float)tileRadius;
			}
			return false;
		}

		public static bool withinRadiusOfPlayer(int x, int y, int tileRadius, Farmer f)
		{
			Point point = new Point(x / 64, y / 64);
			Vector2 tileLocation = f.getTileLocation();
			if (Math.Abs((float)point.X - tileLocation.X) <= (float)tileRadius)
			{
				return Math.Abs((float)point.Y - tileLocation.Y) <= (float)tileRadius;
			}
			return false;
		}

		public static bool isThereAnObjectHereWhichAcceptsThisItem(GameLocation location, Item item, int x, int y)
		{
			if (item is Tool)
			{
				return false;
			}
			Vector2 vector = new Vector2(x / 64, y / 64);
			if (location is BuildableGameLocation)
			{
				foreach (Building building in (location as BuildableGameLocation).buildings)
				{
					if (building.occupiesTile(vector) && building.performActiveObjectDropInAction(Game1.player, probe: true))
					{
						return true;
					}
				}
			}
			if (location.Objects.ContainsKey(vector) && location.objects[vector].heldObject.Value == null)
			{
				location.objects[vector].performObjectDropInAction((Object)item, probe: true, Game1.player);
				bool result = location.objects[vector].heldObject.Value != null;
				location.objects[vector].heldObject.Value = null;
				return result;
			}
			return false;
		}

		public static bool buyWallpaper()
		{
			if (Game1.player.Money >= Game1.wallpaperPrice)
			{
				Game1.updateWallpaperInFarmHouse(Game1.currentWallpaper);
				Game1.farmerWallpaper = Game1.currentWallpaper;
				Game1.player.Money -= Game1.wallpaperPrice;
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5865"), Color.Green, 3500f));
				return true;
			}
			return false;
		}

		public static FarmAnimal getAnimal(long id)
		{
			if (Game1.getFarm().animals.ContainsKey(id))
			{
				return Game1.getFarm().animals[id];
			}
			foreach (Building building in Game1.getFarm().buildings)
			{
				if (building.indoors.Value is AnimalHouse && (building.indoors.Value as AnimalHouse).animals.ContainsKey(id))
				{
					return (building.indoors.Value as AnimalHouse).animals[id];
				}
			}
			return null;
		}

		public static bool buyFloor()
		{
			if (Game1.player.Money >= Game1.floorPrice)
			{
				Game1.FarmerFloor = Game1.currentFloor;
				Game1.updateFloorInFarmHouse(Game1.currentFloor);
				Game1.player.Money -= Game1.floorPrice;
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5868"), Color.Green, 3500f));
				return true;
			}
			return false;
		}

		public static int numSilos()
		{
			int num = 0;
			Farm farm = Game1.getLocationFromName("Farm") as Farm;
			foreach (Building building in farm.buildings)
			{
				if (building.buildingType.Equals("Silo") && (int)building.daysOfConstructionLeft <= 0)
				{
					num++;
				}
			}
			return num;
		}

		public static Dictionary<ISalable, int[]> getCarpenterStock()
		{
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			AddStock(dictionary, new Object(Vector2.Zero, 388, 2147483647));
			AddStock(dictionary, new Object(Vector2.Zero, 390, 2147483647));
			Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			AddStock(dictionary, new Furniture(1614, Vector2.Zero));
			AddStock(dictionary, new Furniture(1616, Vector2.Zero));
			switch (Game1.dayOfMonth % 7)
			{
			case 1:
				AddStock(dictionary, new Furniture(0, Vector2.Zero));
				AddStock(dictionary, new Furniture(192, Vector2.Zero));
				AddStock(dictionary, new Furniture(704, Vector2.Zero));
				AddStock(dictionary, new Furniture(1120, Vector2.Zero));
				AddStock(dictionary, new Furniture(1216, Vector2.Zero));
				AddStock(dictionary, new Furniture(1391, Vector2.Zero));
				break;
			case 2:
				AddStock(dictionary, new Furniture(3, Vector2.Zero));
				AddStock(dictionary, new Furniture(197, Vector2.Zero));
				AddStock(dictionary, new Furniture(709, Vector2.Zero));
				AddStock(dictionary, new Furniture(1122, Vector2.Zero));
				AddStock(dictionary, new Furniture(1218, Vector2.Zero));
				AddStock(dictionary, new Furniture(1393, Vector2.Zero));
				break;
			case 3:
				AddStock(dictionary, new Furniture(6, Vector2.Zero));
				AddStock(dictionary, new Furniture(202, Vector2.Zero));
				AddStock(dictionary, new Furniture(714, Vector2.Zero));
				AddStock(dictionary, new Furniture(1124, Vector2.Zero));
				AddStock(dictionary, new Furniture(1220, Vector2.Zero));
				AddStock(dictionary, new Furniture(1395, Vector2.Zero));
				break;
			case 4:
				AddStock(dictionary, getRandomFurniture(random, dictionary, 1296, 1391));
				AddStock(dictionary, getRandomFurniture(random, dictionary, 1296, 1391));
				break;
			case 5:
				AddStock(dictionary, getRandomFurniture(random, dictionary, 1443, 1450));
				AddStock(dictionary, getRandomFurniture(random, dictionary, 288, 313));
				break;
			case 6:
				AddStock(dictionary, getRandomFurniture(random, dictionary, 1565, 1607));
				AddStock(dictionary, getRandomFurniture(random, dictionary, 12, 129));
				break;
			case 0:
				AddStock(dictionary, getRandomFurniture(random, dictionary, 1296, 1391));
				AddStock(dictionary, getRandomFurniture(random, dictionary, 416, 537));
				break;
			}
			AddStock(dictionary, getRandomFurniture(random, dictionary));
			AddStock(dictionary, getRandomFurniture(random, dictionary));
			while (random.NextDouble() < 0.25)
			{
				AddStock(dictionary, getRandomFurniture(random, dictionary, 1673, 1815));
			}
			AddStock(dictionary, new Furniture(1402, Vector2.Zero));
			AddStock(dictionary, new Object(Vector2.Zero, 208)
			{
				Stack = 2147483647
			});
			if (Game1.currentSeason == "winter" || Game1.year >= 2)
			{
				AddStock(dictionary, new Object(Vector2.Zero, 211)
				{
					Stack = 2147483647
				});
			}
			if (getHomeOfFarmer(Game1.player).upgradeLevel > 0)
			{
				AddStock(dictionary, new Object(Vector2.Zero, 216));
			}
			AddStock(dictionary, new Object(Vector2.Zero, 214));
			AddStock(dictionary, new TV(1466, Vector2.Zero));
			AddStock(dictionary, new TV(1680, Vector2.Zero));
			if (getHomeOfFarmer(Game1.player).upgradeLevel > 0)
			{
				AddStock(dictionary, new TV(1468, Vector2.Zero));
			}
			if (getHomeOfFarmer(Game1.player).upgradeLevel > 0)
			{
				AddStock(dictionary, new Furniture(1226, Vector2.Zero));
			}
			AddStock(dictionary, new Object(Vector2.Zero, 200)
			{
				Stack = 2147483647
			});
			AddStock(dictionary, new Object(Vector2.Zero, 35)
			{
				Stack = 2147483647
			});
			AddStock(dictionary, new Object(Vector2.Zero, 46)
			{
				Stack = 2147483647
			});
			AddStock(dictionary, new Furniture(1792, Vector2.Zero));
			AddStock(dictionary, new Furniture(1794, Vector2.Zero));
			AddStock(dictionary, new Furniture(1798, Vector2.Zero));
			if (Game1.player.eventsSeen.Contains(1053978))
			{
				AddStock(dictionary, new BedFurniture(2186, Vector2.Zero));
			}
			AddStock(dictionary, new BedFurniture(2048, Vector2.Zero), 250);
			if (Game1.player.HouseUpgradeLevel > 0)
			{
				AddStock(dictionary, new BedFurniture(2052, Vector2.Zero), 1000);
			}
			if (Game1.player.HouseUpgradeLevel > 1)
			{
				AddStock(dictionary, new BedFurniture(2076, Vector2.Zero), 1000);
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Wooden Brazier"))
			{
				AddStock(dictionary, new Torch(Vector2.Zero, 143, bigCraftable: true)
				{
					IsRecipe = true
				});
			}
			else if (!Game1.player.craftingRecipes.ContainsKey("Stone Brazier"))
			{
				AddStock(dictionary, new Torch(Vector2.Zero, 144, bigCraftable: true)
				{
					IsRecipe = true
				});
			}
			else if (!Game1.player.craftingRecipes.ContainsKey("Barrel Brazier"))
			{
				AddStock(dictionary, new Torch(Vector2.Zero, 150, bigCraftable: true)
				{
					IsRecipe = true
				});
			}
			else if (!Game1.player.craftingRecipes.ContainsKey("Stump Brazier"))
			{
				AddStock(dictionary, new Torch(Vector2.Zero, 147, bigCraftable: true)
				{
					IsRecipe = true
				});
			}
			else if (!Game1.player.craftingRecipes.ContainsKey("Gold Brazier"))
			{
				AddStock(dictionary, new Torch(Vector2.Zero, 145, bigCraftable: true)
				{
					IsRecipe = true
				});
			}
			else if (!Game1.player.craftingRecipes.ContainsKey("Carved Brazier"))
			{
				AddStock(dictionary, new Torch(Vector2.Zero, 148, bigCraftable: true)
				{
					IsRecipe = true
				});
			}
			else if (!Game1.player.craftingRecipes.ContainsKey("Skull Brazier"))
			{
				AddStock(dictionary, new Torch(Vector2.Zero, 149, bigCraftable: true)
				{
					IsRecipe = true
				});
			}
			else if (!Game1.player.craftingRecipes.ContainsKey("Marble Brazier"))
			{
				AddStock(dictionary, new Torch(Vector2.Zero, 151, bigCraftable: true)
				{
					IsRecipe = true
				});
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Wood Lamp-post"))
			{
				AddStock(dictionary, new Object(Vector2.Zero, 152, isRecipe: true)
				{
					IsRecipe = true
				});
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Iron Lamp-post"))
			{
				AddStock(dictionary, new Object(Vector2.Zero, 153, isRecipe: true)
				{
					IsRecipe = true
				});
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Wood Floor"))
			{
				AddStock(dictionary, new Object(328, 1, isRecipe: true), 50);
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Rustic Plank Floor"))
			{
				AddStock(dictionary, new Object(840, 1, isRecipe: true), 100);
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Stone Floor"))
			{
				AddStock(dictionary, new Object(329, 1, isRecipe: true), 50);
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Brick Floor"))
			{
				AddStock(dictionary, new Object(293, 1, isRecipe: true), 250);
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Stone Walkway Floor"))
			{
				AddStock(dictionary, new Object(841, 1, isRecipe: true), 100);
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Stepping Stone Path"))
			{
				AddStock(dictionary, new Object(415, 1, isRecipe: true), 50);
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Straw Floor"))
			{
				AddStock(dictionary, new Object(401, 1, isRecipe: true), 100);
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Crystal Path"))
			{
				AddStock(dictionary, new Object(409, 1, isRecipe: true), 100);
			}
			return dictionary;
		}

		private static bool isFurnitureOffLimitsForSale(int index)
		{
			switch (index)
			{
			case 131:
			case 134:
			case 984:
			case 985:
			case 986:
			case 989:
			case 1226:
			case 1298:
			case 1299:
			case 1300:
			case 1301:
			case 1302:
			case 1303:
			case 1304:
			case 1305:
			case 1306:
			case 1307:
			case 1308:
			case 1309:
			case 1371:
			case 1373:
			case 1375:
			case 1402:
			case 1466:
			case 1468:
			case 1471:
			case 1541:
			case 1545:
			case 1554:
			case 1669:
			case 1671:
			case 1680:
			case 1687:
			case 1692:
			case 1733:
			case 1760:
			case 1761:
			case 1762:
			case 1763:
			case 1764:
			case 1796:
			case 1798:
			case 1800:
			case 1802:
			case 1838:
			case 1840:
			case 1842:
			case 1844:
			case 1846:
			case 1848:
			case 1850:
			case 1852:
			case 1854:
			case 1900:
			case 1902:
			case 1907:
			case 1909:
			case 1914:
			case 1915:
			case 1916:
			case 1917:
			case 1918:
			case 1952:
			case 1953:
			case 1954:
			case 1955:
			case 1956:
			case 1957:
			case 1958:
			case 1959:
			case 1960:
			case 1961:
			case 1971:
			case 2186:
			case 2326:
			case 2329:
			case 2331:
			case 2332:
			case 2334:
			case 2393:
			case 2396:
			case 2400:
			case 2418:
			case 2419:
			case 2421:
			case 2423:
			case 2425:
			case 2426:
			case 2428:
			case 2496:
			case 2502:
			case 2508:
			case 2514:
			case 2624:
			case 2625:
			case 2626:
			case 2653:
			case 2732:
			case 2814:
				return true;
			default:
				return false;
			}
		}

		private static Furniture getRandomFurniture(Random r, Dictionary<ISalable, int[]> stock, int lowerIndexBound = 0, int upperIndexBound = 1462)
		{
			int num = -1;
			Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\Furniture");
			do
			{
				num = r.Next(lowerIndexBound, upperIndexBound);
				if (stock == null)
				{
					continue;
				}
				foreach (Item key in stock.Keys)
				{
					if (key is Furniture && (int)key.parentSheetIndex == num)
					{
						num = -1;
					}
				}
			}
			while (isFurnitureOffLimitsForSale(num) || !dictionary.ContainsKey(num));
			Furniture furniture = new Furniture(num, Vector2.Zero);
			furniture.stack.Value = 2147483647;
			return furniture;
		}

		public static Dictionary<ISalable, int[]> GetQiChallengeRewardStock(Farmer who)
		{
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			Item item = null;
			int num = 2147483647;
			item = new Object(Vector2.Zero, 256);
			if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotFirstJunimoChest"))
			{
				item.Stack = 2;
				num = 1;
			}
			dictionary.Add(item, new int[4]
			{
				0,
				num,
				858,
				15 * item.Stack
			});
			item = new Object(911, 1);
			dictionary.Add(item, new int[4] { 0, 2147483647, 858, 50 });
			if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotMissingStocklist"))
			{
				item = new Object(897, 1);
				(item as Object).questItem.Value = true;
				dictionary.Add(item, new int[4] { 0, 1, 858, 50 });
			}
			item = new Object(Vector2.Zero, 275);
			dictionary.Add(item, new int[4] { 0, 2147483647, 858, 10 });
			item = new Object(913, 4);
			dictionary.Add(item, new int[4] { 0, 2147483647, 858, 20 });
			item = new Object(915, 4);
			dictionary.Add(item, new int[4] { 0, 2147483647, 858, 20 });
			item = new Object(Vector2.Zero, 265);
			dictionary.Add(item, new int[4] { 0, 2147483647, 858, 20 });
			if (!who.HasTownKey)
			{
				PurchaseableKeyItem key = new PurchaseableKeyItem(Game1.content.LoadString("Strings\\StringsFromCSFiles:KeyToTheTown"), Game1.content.LoadString("Strings\\StringsFromCSFiles:Key To The Town_desc"), 912, delegate(Farmer farmer)
				{
					farmer.HasTownKey = true;
				});
				dictionary.Add(key, new int[4] { 0, 1, 858, 20 });
			}
			item = new Object(896, 1);
			dictionary.Add(item, new int[4] { 0, 2147483647, 858, 40 });
			item = new Object(891, 1);
			dictionary.Add(item, new int[4] { 0, 2147483647, 858, 5 });
			item = new Object(908, 20);
			dictionary.Add(item, new int[4] { 0, 2147483647, 858, 5 });
			item = new Object(917, 10);
			dictionary.Add(item, new int[4] { 0, 2147483647, 858, 10 });
			item = new Hat(82);
			dictionary.Add(item, new int[4] { 0, 2147483647, 858, 5 });
			item = new FishTankFurniture(2400, Vector2.Zero);
			dictionary.Add(item, new int[4] { 0, 2147483647, 858, 20 });
			if (!who.craftingRecipes.ContainsKey("Heavy Tapper"))
			{
				item = new Object(Vector2.Zero, 264, isRecipe: true);
				dictionary.Add(item, new int[4] { 0, 1, 858, 20 });
			}
			if (!who.craftingRecipes.ContainsKey("Hyper Speed-Gro"))
			{
				item = new Object(918, 1, isRecipe: true);
				dictionary.Add(item, new int[4] { 0, 1, 858, 30 });
			}
			if (!who.craftingRecipes.ContainsKey("Deluxe Fertilizer"))
			{
				item = new Object(919, 1, isRecipe: true);
				dictionary.Add(item, new int[4] { 0, 1, 858, 20 });
			}
			if (!who.craftingRecipes.ContainsKey("Hopper"))
			{
				item = new Object(Vector2.Zero, 275, isRecipe: true);
				dictionary.Add(item, new int[4] { 0, 1, 858, 50 });
			}
			if (!who.craftingRecipes.ContainsKey("Magic Bait"))
			{
				item = new Object(908, 1, isRecipe: true);
				dictionary.Add(item, new int[4] { 0, 1, 858, 20 });
			}
			if ((int)Game1.netWorldState.Value.GoldenWalnuts > 0 && Game1.player.hasOrWillReceiveMail("Island_FirstParrot") && Game1.player.hasOrWillReceiveMail("Island_Turtle") && Game1.player.hasOrWillReceiveMail("Island_UpgradeBridge") && Game1.player.hasOrWillReceiveMail("Island_UpgradeHouse") && Game1.player.hasOrWillReceiveMail("Island_UpgradeParrotPlatform") && Game1.player.hasOrWillReceiveMail("Island_Resort") && Game1.player.hasOrWillReceiveMail("Island_UpgradeTrader") && Game1.player.hasOrWillReceiveMail("Island_W_Obelisk") && Game1.player.hasOrWillReceiveMail("Island_UpgradeHouse_Mailbox") && Game1.player.hasOrWillReceiveMail("Island_VolcanoBridge") && Game1.player.hasOrWillReceiveMail("Island_VolcanoShortcutOut"))
			{
				item = new Object(858, 2);
				dictionary.Add(item, new int[4] { 0, 2147483647, 73, 1 });
			}
			item = new BedFurniture(2514, Vector2.Zero);
			dictionary.Add(item, new int[4] { 0, 2147483647, 858, 50 });
			if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
			{
				item = new Object(928, 1);
				dictionary.Add(item, new int[4] { 0, 2147483647, 858, 100 });
			}
			return dictionary;
		}

		public static Dictionary<ISalable, int[]> getAdventureRecoveryStock()
		{
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			foreach (Item item in Game1.player.itemsLostLastDeath)
			{
				if (item != null)
				{
					item.isLostItem = true;
					dictionary.Add(item, new int[2]
					{
						getSellToStorePriceOfItem(item),
						item.Stack
					});
				}
			}
			return dictionary;
		}

		public static Dictionary<ISalable, int[]> getAdventureShopStock()
		{
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			int num = 2147483647;
			dictionary.Add(new MeleeWeapon(12), new int[2] { 250, num });
			if (MineShaft.lowestLevelReached >= 15)
			{
				dictionary.Add(new MeleeWeapon(17), new int[2] { 500, num });
			}
			if (MineShaft.lowestLevelReached >= 20)
			{
				dictionary.Add(new MeleeWeapon(1), new int[2] { 750, num });
			}
			if (MineShaft.lowestLevelReached >= 25)
			{
				dictionary.Add(new MeleeWeapon(43), new int[2] { 850, num });
				dictionary.Add(new MeleeWeapon(44), new int[2] { 1500, num });
			}
			if (MineShaft.lowestLevelReached >= 40)
			{
				dictionary.Add(new MeleeWeapon(27), new int[2] { 2000, num });
			}
			if (MineShaft.lowestLevelReached >= 45)
			{
				dictionary.Add(new MeleeWeapon(10), new int[2] { 2000, num });
			}
			if (MineShaft.lowestLevelReached >= 55)
			{
				dictionary.Add(new MeleeWeapon(7), new int[2] { 4000, num });
			}
			if (MineShaft.lowestLevelReached >= 75)
			{
				dictionary.Add(new MeleeWeapon(5), new int[2] { 6000, num });
			}
			if (MineShaft.lowestLevelReached >= 90)
			{
				dictionary.Add(new MeleeWeapon(50), new int[2] { 9000, num });
			}
			if (MineShaft.lowestLevelReached >= 120)
			{
				dictionary.Add(new MeleeWeapon(9), new int[2] { 25000, num });
			}
			if (Game1.player.mailReceived.Contains("galaxySword"))
			{
				dictionary.Add(new MeleeWeapon(4), new int[2] { 50000, num });
				dictionary.Add(new MeleeWeapon(23), new int[2] { 35000, num });
				dictionary.Add(new MeleeWeapon(29), new int[2] { 75000, num });
			}
			dictionary.Add(new Boots(504), new int[2] { 500, num });
			if (MineShaft.lowestLevelReached >= 10)
			{
				dictionary.Add(new Boots(506), new int[2] { 500, num });
			}
			if (MineShaft.lowestLevelReached >= 50)
			{
				dictionary.Add(new Boots(509), new int[2] { 750, num });
			}
			if (MineShaft.lowestLevelReached >= 40)
			{
				dictionary.Add(new Boots(508), new int[2] { 1250, num });
			}
			if (MineShaft.lowestLevelReached >= 80)
			{
				dictionary.Add(new Boots(512), new int[2] { 2000, num });
				dictionary.Add(new Boots(511), new int[2] { 2500, num });
			}
			if (MineShaft.lowestLevelReached >= 110)
			{
				dictionary.Add(new Boots(514), new int[2] { 5000, num });
			}
			dictionary.Add(new Ring(529), new int[2] { 1000, num });
			dictionary.Add(new Ring(530), new int[2] { 1000, num });
			if (MineShaft.lowestLevelReached >= 40)
			{
				dictionary.Add(new Ring(531), new int[2] { 2500, num });
				dictionary.Add(new Ring(532), new int[2] { 2500, num });
			}
			if (MineShaft.lowestLevelReached >= 80)
			{
				dictionary.Add(new Ring(533), new int[2] { 5000, num });
				dictionary.Add(new Ring(534), new int[2] { 5000, num });
			}
			_ = MineShaft.lowestLevelReached;
			_ = 120;
			if (MineShaft.lowestLevelReached >= 40)
			{
				dictionary.Add(new Slingshot(32), new int[2] { 500, num });
			}
			if (MineShaft.lowestLevelReached >= 70)
			{
				dictionary.Add(new Slingshot(33), new int[2] { 1000, num });
			}
			if (Game1.player.craftingRecipes.ContainsKey("Explosive Ammo"))
			{
				dictionary.Add(new Object(441, 2147483647), new int[2] { 300, num });
			}
			if (Game1.player.mailReceived.Contains("Gil_Slime Charmer Ring"))
			{
				dictionary.Add(new Ring(520), new int[2] { 25000, num });
			}
			if (Game1.player.mailReceived.Contains("Gil_Savage Ring"))
			{
				dictionary.Add(new Ring(523), new int[2] { 25000, num });
			}
			if (Game1.player.mailReceived.Contains("Gil_Burglar's Ring"))
			{
				dictionary.Add(new Ring(526), new int[2] { 20000, num });
			}
			if (Game1.player.mailReceived.Contains("Gil_Vampire Ring"))
			{
				dictionary.Add(new Ring(522), new int[2] { 15000, num });
			}
			if (Game1.player.mailReceived.Contains("Gil_Crabshell Ring"))
			{
				dictionary.Add(new Ring(810), new int[2] { 15000, num });
			}
			if (Game1.player.mailReceived.Contains("Gil_Napalm Ring"))
			{
				dictionary.Add(new Ring(811), new int[2] { 30000, num });
			}
			if (Game1.player.mailReceived.Contains("Gil_Skeleton Mask"))
			{
				dictionary.Add(new Hat(8), new int[2] { 20000, num });
			}
			if (Game1.player.mailReceived.Contains("Gil_Hard Hat"))
			{
				dictionary.Add(new Hat(27), new int[2] { 20000, num });
			}
			if (Game1.player.mailReceived.Contains("Gil_Arcane Hat"))
			{
				dictionary.Add(new Hat(60), new int[2] { 20000, num });
			}
			if (Game1.player.mailReceived.Contains("Gil_Knight's Helmet"))
			{
				dictionary.Add(new Hat(50), new int[2] { 20000, num });
			}
			if (Game1.player.mailReceived.Contains("Gil_Insect Head"))
			{
				dictionary.Add(new MeleeWeapon(13), new int[2] { 10000, num });
			}
			return dictionary;
		}

		public static void AddStock(Dictionary<ISalable, int[]> stock, Item obj, int buyPrice = -1, int limitedQuantity = -1)
		{
			int num = 2 * buyPrice;
			if (buyPrice == -1)
			{
				num = obj.salePrice();
			}
			int num2 = 2147483647;
			if (obj is Object && (obj as Object).IsRecipe)
			{
				num2 = 1;
			}
			else if (limitedQuantity != -1)
			{
				num2 = limitedQuantity;
			}
			stock.Add(obj, new int[2] { num, num2 });
		}

		public static Dictionary<ISalable, int[]> getSaloonStock()
		{
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			AddStock(dictionary, new Object(Vector2.Zero, 346, 2147483647));
			AddStock(dictionary, new Object(Vector2.Zero, 196, 2147483647));
			AddStock(dictionary, new Object(Vector2.Zero, 216, 2147483647));
			AddStock(dictionary, new Object(Vector2.Zero, 224, 2147483647));
			AddStock(dictionary, new Object(Vector2.Zero, 206, 2147483647));
			AddStock(dictionary, new Object(Vector2.Zero, 395, 2147483647));
			if (!Game1.player.cookingRecipes.ContainsKey("Hashbrowns"))
			{
				AddStock(dictionary, new Object(210, 1, isRecipe: true), 25);
			}
			if (!Game1.player.cookingRecipes.ContainsKey("Omelet"))
			{
				AddStock(dictionary, new Object(195, 1, isRecipe: true), 50);
			}
			if (!Game1.player.cookingRecipes.ContainsKey("Pancakes"))
			{
				AddStock(dictionary, new Object(211, 1, isRecipe: true), 50);
			}
			if (!Game1.player.cookingRecipes.ContainsKey("Bread"))
			{
				AddStock(dictionary, new Object(216, 1, isRecipe: true), 50);
			}
			if (!Game1.player.cookingRecipes.ContainsKey("Tortilla"))
			{
				AddStock(dictionary, new Object(229, 1, isRecipe: true), 50);
			}
			if (!Game1.player.cookingRecipes.ContainsKey("Pizza"))
			{
				AddStock(dictionary, new Object(206, 1, isRecipe: true), 75);
			}
			if (!Game1.player.cookingRecipes.ContainsKey("Maki Roll"))
			{
				AddStock(dictionary, new Object(228, 1, isRecipe: true), 150);
			}
			if (!Game1.player.cookingRecipes.ContainsKey("Cookies") && Game1.player.eventsSeen.Contains(19))
			{
				AddStock(dictionary, new Object(223, 1, isRecipe: true)
				{
					name = "Cookies"
				}, 150);
			}
			if (!Game1.player.cookingRecipes.ContainsKey("Triple Shot Espresso"))
			{
				AddStock(dictionary, new Object(253, 1, isRecipe: true), 2500);
			}
			if ((int)Game1.dishOfTheDay.stack > 0 && !getForbiddenDishesOfTheDay().Contains(Game1.dishOfTheDay.ParentSheetIndex))
			{
				AddStock(dictionary, Game1.dishOfTheDay.getOne() as Object, Game1.dishOfTheDay.Price, Game1.dishOfTheDay.stack);
			}
			SynchronizedShopStock synchronizedShopStock = Game1.player.team.synchronizedShopStock;
			synchronizedShopStock.UpdateLocalStockWithSyncedQuanitities(SynchronizedShopStock.SynchedShop.Saloon, dictionary);
			if (Game1.player.activeDialogueEvents.ContainsKey("willyCrabs"))
			{
				AddStock(dictionary, new Object(Vector2.Zero, 732, 2147483647));
			}
			return dictionary;
		}

		public static int[] getForbiddenDishesOfTheDay()
		{
			return new int[7] { 346, 196, 216, 224, 206, 395, 217 };
		}

		public static bool removeLightSource(int identifier)
		{
			bool result = false;
			for (int num = Game1.currentLightSources.Count - 1; num >= 0; num--)
			{
				if ((int)Game1.currentLightSources.ElementAt(num).identifier == identifier)
				{
					Game1.currentLightSources.Remove(Game1.currentLightSources.ElementAt(num));
					result = true;
				}
			}
			return result;
		}

		public static Horse findHorseForPlayer(long uid)
		{
			foreach (GameLocation location in Game1.locations)
			{
				foreach (NPC character in location.characters)
				{
					if (character is Horse horse && (long)horse.ownerId == uid)
					{
						return horse;
					}
				}
			}
			return null;
		}

		public static Horse findHorse(Guid horseId)
		{
			foreach (GameLocation location in Game1.locations)
			{
				foreach (NPC character in location.characters)
				{
					if (character is Horse horse && horse.HorseId == horseId)
					{
						return horse;
					}
				}
			}
			return null;
		}

		public static void addDirtPuffs(GameLocation location, int tileX, int tileY, int tilesWide, int tilesHigh, int number = 5)
		{
			for (int i = tileX; i < tileX + tilesWide; i++)
			{
				for (int j = tileY; j < tileY + tilesHigh; j++)
				{
					for (int k = 0; k < number; k++)
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite((Game1.random.NextDouble() < 0.5) ? 46 : 12, new Vector2(i, j) * 64f + new Vector2(Game1.random.Next(-16, 32), Game1.random.Next(-16, 32)), Color.White, 10, Game1.random.NextDouble() < 0.5)
						{
							delayBeforeAnimationStart = Math.Max(0, Game1.random.Next(-200, 400)),
							motion = new Vector2(0f, -1f),
							interval = Game1.random.Next(50, 80)
						});
					}
					location.temporarySprites.Add(new TemporaryAnimatedSprite(14, new Vector2(i, j) * 64f + new Vector2(Game1.random.Next(-16, 32), Game1.random.Next(-16, 32)), Color.White, 10, Game1.random.NextDouble() < 0.5));
				}
			}
		}

		public static void addSmokePuff(GameLocation l, Vector2 v, int delay = 0, float baseScale = 2f, float scaleChange = 0.02f, float alpha = 0.75f, float alphaFade = 0.002f)
		{
			l.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), v, flipped: false, alphaFade, Color.Gray)
			{
				alpha = alpha,
				motion = new Vector2(0f, -0.5f),
				acceleration = new Vector2(0.002f, 0f),
				interval = 99999f,
				layerDepth = 1f,
				scale = baseScale,
				scaleChange = scaleChange,
				rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
				delayBeforeAnimationStart = delay
			});
		}

		public static LightSource getLightSource(int identifier)
		{
			foreach (LightSource currentLightSource in Game1.currentLightSources)
			{
				if ((int)currentLightSource.identifier == identifier)
				{
					return currentLightSource;
				}
			}
			return null;
		}

		public static Dictionary<ISalable, int[]> getAllWallpapersAndFloorsForFree()
		{
			List<ModWallpaperOrFlooring> list = Game1.content.Load<List<ModWallpaperOrFlooring>>("Data\\AdditionalWallpaperFlooring");
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			for (int i = 0; i < 112; i++)
			{
				dictionary.Add(new Wallpaper(i)
				{
					Stack = 2147483647
				}, new int[2] { 0, 2147483647 });
			}
			foreach (ModWallpaperOrFlooring item in list)
			{
				for (int j = 0; j < item.Count; j++)
				{
					if (!item.IsFlooring)
					{
						Wallpaper key = new Wallpaper(item.ID, j)
						{
							Stack = 2147483647
						};
						dictionary.Add(key, new int[2] { 0, 2147483647 });
					}
				}
			}
			for (int k = 0; k < 56; k++)
			{
				dictionary.Add(new Wallpaper(k, isFloor: true)
				{
					Stack = 2147483647
				}, new int[2] { 0, 2147483647 });
			}
			foreach (ModWallpaperOrFlooring item2 in list)
			{
				for (int l = 0; l < item2.Count; l++)
				{
					if (item2.IsFlooring)
					{
						Wallpaper key2 = new Wallpaper(item2.ID, l)
						{
							Stack = 2147483647
						};
						dictionary.Add(key2, new int[2] { 0, 2147483647 });
					}
				}
			}
			return dictionary;
		}

		public static Dictionary<ISalable, int[]> getAllFurnituresForFree()
		{
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			Dictionary<int, string> dictionary2 = Game1.content.Load<Dictionary<int, string>>("Data\\Furniture");
			List<Furniture> list = new List<Furniture>();
			foreach (KeyValuePair<int, string> item in dictionary2)
			{
				if (!isFurnitureOffLimitsForSale(item.Key))
				{
					list.Add(Furniture.GetFurnitureInstance(item.Key));
				}
			}
			list.Sort(SortAllFurnitures);
			foreach (Furniture item2 in list)
			{
				dictionary.Add(item2, new int[2] { 0, 2147483647 });
			}
			dictionary.Add(new Furniture(1402, Vector2.Zero), new int[2] { 0, 2147483647 });
			dictionary.Add(new TV(1680, Vector2.Zero), new int[2] { 0, 2147483647 });
			dictionary.Add(new TV(1466, Vector2.Zero), new int[2] { 0, 2147483647 });
			dictionary.Add(new TV(1468, Vector2.Zero), new int[2] { 0, 2147483647 });
			return dictionary;
		}

		public static int SortAllFurnitures(Furniture a, Furniture b)
		{
			if (a.furniture_type != b.furniture_type)
			{
				return a.furniture_type.Value.CompareTo(b.furniture_type.Value);
			}
			if ((int)a.furniture_type == 12 && (int)b.furniture_type == 12)
			{
				bool flag = a.Name.StartsWith("Floor Divider ");
				bool flag2 = b.Name.StartsWith("Floor Divider ");
				if (flag != flag2)
				{
					if (flag2)
					{
						return -1;
					}
					return 1;
				}
			}
			return a.ParentSheetIndex.CompareTo(b.ParentSheetIndex);
		}

		public static bool doesAnyFarmerHaveOrWillReceiveMail(string id)
		{
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer.hasOrWillReceiveMail(id))
				{
					return true;
				}
			}
			return false;
		}

		public static string loadStringShort(string fileWithinStringsFolder, string key)
		{
			return Game1.content.LoadString("Strings\\" + fileWithinStringsFolder + ":" + key);
		}

		public static string loadStringDataShort(string fileWithinStringsFolder, string key)
		{
			return Game1.content.LoadString("Data\\" + fileWithinStringsFolder + ":" + key);
		}

		public static bool doesAnyFarmerHaveMail(string id)
		{
			if (Game1.player.mailReceived.Contains(id))
			{
				return true;
			}
			foreach (Farmer value in Game1.otherFarmers.Values)
			{
				if (value.mailReceived.Contains(id))
				{
					return true;
				}
			}
			return false;
		}

		public static FarmEvent pickFarmEvent()
		{
			return Game1.hooks.OnUtility_PickFarmEvent(delegate
			{
				Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
				if (Game1.weddingToday)
				{
					return null;
				}
				foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
				{
					Friendship spouseFriendship = onlineFarmer.GetSpouseFriendship();
					if (spouseFriendship != null && spouseFriendship.IsMarried() && spouseFriendship.WeddingDate == Game1.Date)
					{
						return null;
					}
				}
				if (Game1.stats.DaysPlayed == 31)
				{
					return new SoundInTheNightEvent(4);
				}
				if (Game1.MasterPlayer.mailForTomorrow.Contains("leoMoved%&NL&%") || Game1.MasterPlayer.mailForTomorrow.Contains("leoMoved"))
				{
					return new WorldChangeEvent(14);
				}
				if (Game1.player.mailForTomorrow.Contains("jojaPantry%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaPantry"))
				{
					return new WorldChangeEvent(0);
				}
				if (Game1.player.mailForTomorrow.Contains("ccPantry%&NL&%") || Game1.player.mailForTomorrow.Contains("ccPantry"))
				{
					return new WorldChangeEvent(1);
				}
				if (Game1.player.mailForTomorrow.Contains("jojaVault%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaVault"))
				{
					return new WorldChangeEvent(6);
				}
				if (Game1.player.mailForTomorrow.Contains("ccVault%&NL&%") || Game1.player.mailForTomorrow.Contains("ccVault"))
				{
					return new WorldChangeEvent(7);
				}
				if (Game1.player.mailForTomorrow.Contains("jojaBoilerRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaBoilerRoom"))
				{
					return new WorldChangeEvent(2);
				}
				if (Game1.player.mailForTomorrow.Contains("ccBoilerRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("ccBoilerRoom"))
				{
					return new WorldChangeEvent(3);
				}
				if (Game1.player.mailForTomorrow.Contains("jojaCraftsRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaCraftsRoom"))
				{
					return new WorldChangeEvent(4);
				}
				if (Game1.player.mailForTomorrow.Contains("ccCraftsRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("ccCraftsRoom"))
				{
					return new WorldChangeEvent(5);
				}
				if (Game1.player.mailForTomorrow.Contains("jojaFishTank%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaFishTank"))
				{
					return new WorldChangeEvent(8);
				}
				if (Game1.player.mailForTomorrow.Contains("ccFishTank%&NL&%") || Game1.player.mailForTomorrow.Contains("ccFishTank"))
				{
					return new WorldChangeEvent(9);
				}
				if (Game1.player.mailForTomorrow.Contains("ccMovieTheaterJoja%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaMovieTheater"))
				{
					return new WorldChangeEvent(10);
				}
				if (Game1.player.mailForTomorrow.Contains("ccMovieTheater%&NL&%") || Game1.player.mailForTomorrow.Contains("ccMovieTheater"))
				{
					return new WorldChangeEvent(11);
				}
				if (Game1.MasterPlayer.eventsSeen.Contains(191393) && (Game1.isRaining || Game1.isLightning) && !Game1.MasterPlayer.mailReceived.Contains("abandonedJojaMartAccessible") && !Game1.MasterPlayer.mailReceived.Contains("ccMovieTheater"))
				{
					return new WorldChangeEvent(12);
				}
				if (Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatTicketMachine") && Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatHull") && Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatAnchor") && !Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed"))
				{
					return new WorldChangeEvent(13);
				}
				if (random.NextDouble() < 0.01 && !Game1.currentSeason.Equals("winter"))
				{
					return new FairyEvent();
				}
				if (random.NextDouble() < 0.01)
				{
					return new WitchEvent();
				}
				if (random.NextDouble() < 0.01)
				{
					return new SoundInTheNightEvent(1);
				}
				if (random.NextDouble() < 0.005)
				{
					return new SoundInTheNightEvent(3);
				}
				if (random.NextDouble() < 0.008 && Game1.year > 1 && !Game1.MasterPlayer.mailReceived.Contains("Got_Capsule"))
				{
					Game1.MasterPlayer.mailReceived.Add("Got_Capsule");
					return new SoundInTheNightEvent(0);
				}
				return null;
			});
		}

		public static bool hasFinishedJojaRoute()
		{
			bool flag = false;
			if (Game1.player.mailReceived.Contains("jojaVault"))
			{
				flag = true;
			}
			else if (!Game1.player.mailReceived.Contains("ccVault"))
			{
				return false;
			}
			if (Game1.player.mailReceived.Contains("jojaPantry"))
			{
				flag = true;
			}
			else if (!Game1.player.mailReceived.Contains("ccPantry"))
			{
				return false;
			}
			if (Game1.player.mailReceived.Contains("jojaBoilerRoom"))
			{
				flag = true;
			}
			else if (!Game1.player.mailReceived.Contains("ccBoilerRoom"))
			{
				return false;
			}
			if (Game1.player.mailReceived.Contains("jojaCraftsRoom"))
			{
				flag = true;
			}
			else if (!Game1.player.mailReceived.Contains("ccCraftsRoom"))
			{
				return false;
			}
			if (Game1.player.mailReceived.Contains("jojaFishTank"))
			{
				flag = true;
			}
			else if (!Game1.player.mailReceived.Contains("ccFishTank"))
			{
				return false;
			}
			if (flag || Game1.player.mailReceived.Contains("JojaMember"))
			{
				return true;
			}
			return false;
		}

		public static FarmEvent pickPersonalFarmEvent()
		{
			Random random = new Random(((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2) ^ (470124797 + (int)Game1.player.UniqueMultiplayerID));
			if (Game1.weddingToday)
			{
				return null;
			}
			if (Game1.player.isMarried() && Game1.player.GetSpouseFriendship().DaysUntilBirthing <= 0 && Game1.player.GetSpouseFriendship().NextBirthingDate != null)
			{
				if (Game1.player.spouse != null)
				{
					return new BirthingEvent();
				}
				long value = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
				if (Game1.otherFarmers.ContainsKey(value))
				{
					return new PlayerCoupleBirthingEvent();
				}
			}
			else
			{
				if (Game1.player.isMarried() && Game1.player.spouse != null && Game1.getCharacterFromName(Game1.player.spouse).canGetPregnant() && Game1.player.currentLocation == Game1.getLocationFromName(Game1.player.homeLocation) && random.NextDouble() < 0.05)
				{
					return new QuestionEvent(1);
				}
				if (Game1.player.isMarried() && Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).HasValue && Game1.player.GetSpouseFriendship().NextBirthingDate == null && random.NextDouble() < 0.05)
				{
					long value2 = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
					if (Game1.otherFarmers.ContainsKey(value2))
					{
						Farmer farmer = Game1.otherFarmers[value2];
						if (farmer.currentLocation == Game1.player.currentLocation && (farmer.currentLocation == Game1.getLocationFromName(farmer.homeLocation) || farmer.currentLocation == Game1.getLocationFromName(Game1.player.homeLocation)))
						{
							FarmHouse farmHouse = farmer.currentLocation as FarmHouse;
							if (playersCanGetPregnantHere(farmHouse))
							{
								return new QuestionEvent(3);
							}
						}
					}
				}
			}
			if (random.NextDouble() < 0.5)
			{
				return new QuestionEvent(2);
			}
			return new SoundInTheNightEvent(2);
		}

		private static bool playersCanGetPregnantHere(FarmHouse farmHouse)
		{
			List<Child> children = farmHouse.getChildren();
			if (farmHouse.cribStyle.Value <= 0)
			{
				return false;
			}
			if (farmHouse.getChildrenCount() < 2 && farmHouse.upgradeLevel >= 2 && children.Count < 2)
			{
				if (children.Count != 0)
				{
					return children[0].Age > 2;
				}
				return true;
			}
			return false;
		}

		public static string capitalizeFirstLetter(string s)
		{
			if (s == null || s.Length < 1)
			{
				return "";
			}
			return s[0].ToString().ToUpper() + ((s.Length > 1) ? s.Substring(1) : "");
		}

		public static Dictionary<ISalable, int[]> getBlacksmithStock()
		{
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			dictionary.Add(new Object(Vector2.Zero, 378, 2147483647), new int[2]
			{
				(Game1.year > 1) ? 150 : 75,
				2147483647
			});
			dictionary.Add(new Object(Vector2.Zero, 380, 2147483647), new int[2]
			{
				(Game1.year > 1) ? 250 : 150,
				2147483647
			});
			dictionary.Add(new Object(Vector2.Zero, 382, 2147483647), new int[2]
			{
				(Game1.year > 1) ? 250 : 150,
				2147483647
			});
			dictionary.Add(new Object(Vector2.Zero, 384, 2147483647), new int[2]
			{
				(Game1.year > 1) ? 750 : 400,
				2147483647
			});
			return dictionary;
		}

		public static bool alreadyHasLightSourceWithThisID(int identifier)
		{
			foreach (LightSource currentLightSource in Game1.currentLightSources)
			{
				if ((int)currentLightSource.identifier == identifier)
				{
					return true;
				}
			}
			return false;
		}

		public static void repositionLightSource(int identifier, Vector2 position)
		{
			foreach (LightSource currentLightSource in Game1.currentLightSources)
			{
				if ((int)currentLightSource.identifier == identifier)
				{
					currentLightSource.position.Value = position;
				}
			}
		}

		public static Dictionary<ISalable, int[]> getAnimalShopStock()
		{
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			dictionary.Add(new Object(178, 1), new int[2] { 50, 2147483647 });
			Object @object = new Object(Vector2.Zero, 104);
			@object.price.Value = 2000;
			@object.Stack = 1;
			dictionary.Add(@object, new int[2] { 2000, 2147483647 });
			if (Game1.player.hasItemWithNameThatContains("Milk Pail") == null)
			{
				dictionary.Add(new MilkPail(), new int[2] { 1000, 1 });
			}
			if (Game1.player.hasItemWithNameThatContains("Shears") == null)
			{
				dictionary.Add(new Shears(), new int[2] { 1000, 1 });
			}
			if ((int)Game1.player.farmingLevel >= 10)
			{
				dictionary.Add(new Object(Vector2.Zero, 165), new int[2] { 25000, 2147483647 });
			}
			dictionary.Add(new Object(Vector2.Zero, 45), new int[2] { 250, 2147483647 });
			if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
			{
				dictionary.Add(new Object(928, 1), new int[2] { 100000, 2147483647 });
			}
			return dictionary;
		}

		public static bool areThereAnyOtherAnimalsWithThisName(string name)
		{
			Farm farm = Game1.getLocationFromName("Farm") as Farm;
			foreach (Building building in farm.buildings)
			{
				if (!(building.indoors.Value is AnimalHouse))
				{
					continue;
				}
				foreach (FarmAnimal value in (building.indoors.Value as AnimalHouse).animals.Values)
				{
					if (value.displayName != null && value.displayName.Equals(name))
					{
						return true;
					}
				}
			}
			foreach (FarmAnimal value2 in farm.animals.Values)
			{
				if (value2.displayName != null && value2.displayName.Equals(name))
				{
					return true;
				}
			}
			return false;
		}

		public static string getNumberWithCommas(int number)
		{
			StringBuilder stringBuilder = new StringBuilder(number.ToString() ?? "");
			string value = ",";
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt)
			{
				value = ".";
			}
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod && LocalizedContentManager.CurrentModLanguage != null)
			{
				value = LocalizedContentManager.CurrentModLanguage.NumberComma;
			}
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru)
			{
				value = " ";
			}
			for (int num = stringBuilder.Length - 4; num >= 0; num -= 3)
			{
				stringBuilder.Insert(num + 1, value);
			}
			return stringBuilder.ToString();
		}

		public static List<Object> getPurchaseAnimalStock()
		{
			List<Object> list = new List<Object>();
			Object item = new Object(100, 1, isRecipe: false, 400)
			{
				Name = "Chicken",
				Type = ((Game1.getFarm().isBuildingConstructed("Coop") || Game1.getFarm().isBuildingConstructed("Deluxe Coop") || Game1.getFarm().isBuildingConstructed("Big Coop")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5926")),
				displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5922")
			};
			list.Add(item);
			item = new Object(100, 1, isRecipe: false, 750)
			{
				Name = "Dairy Cow",
				Type = ((Game1.getFarm().isBuildingConstructed("Barn") || Game1.getFarm().isBuildingConstructed("Deluxe Barn") || Game1.getFarm().isBuildingConstructed("Big Barn")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5931")),
				displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5927")
			};
			list.Add(item);
			item = new Object(100, 1, isRecipe: false, 2000)
			{
				Name = "Goat",
				Type = ((Game1.getFarm().isBuildingConstructed("Big Barn") || Game1.getFarm().isBuildingConstructed("Deluxe Barn")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5936")),
				displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5933")
			};
			list.Add(item);
			item = new Object(100, 1, isRecipe: false, 600)
			{
				Name = "Duck",
				Type = ((Game1.getFarm().isBuildingConstructed("Big Coop") || Game1.getFarm().isBuildingConstructed("Deluxe Coop")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5940")),
				displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5937")
			};
			list.Add(item);
			item = new Object(100, 1, isRecipe: false, 4000)
			{
				Name = "Sheep",
				Type = (Game1.getFarm().isBuildingConstructed("Deluxe Barn") ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5944")),
				displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5942")
			};
			list.Add(item);
			item = new Object(100, 1, isRecipe: false, 4000)
			{
				Name = "Rabbit",
				Type = (Game1.getFarm().isBuildingConstructed("Deluxe Coop") ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5947")),
				displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5945")
			};
			list.Add(item);
			item = new Object(100, 1, isRecipe: false, 8000)
			{
				Name = "Pig",
				Type = (Game1.getFarm().isBuildingConstructed("Deluxe Barn") ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5950")),
				displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5948")
			};
			list.Add(item);
			return list;
		}

		public static void FixChildNameCollisions()
		{
			List<NPC> list = new List<NPC>();
			getAllCharacters(list);
			bool flag = false;
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			foreach (NPC item in list)
			{
				if (!(item is Child))
				{
					continue;
				}
				string name = item.Name;
				string text = item.Name;
				do
				{
					flag = false;
					if (dictionary.ContainsKey(text))
					{
						text += " ";
						flag = true;
						continue;
					}
					foreach (NPC item2 in list)
					{
						if (item2 != item && item2.name.Equals(text))
						{
							text += " ";
							flag = true;
						}
					}
				}
				while (flag);
				if (!(text != item.Name))
				{
					continue;
				}
				item.Name = text;
				item.displayName = null;
				string displayName = item.displayName;
				foreach (Farmer allFarmer in Game1.getAllFarmers())
				{
					if (allFarmer.friendshipData != null && allFarmer.friendshipData.ContainsKey(name))
					{
						allFarmer.friendshipData[text] = allFarmer.friendshipData[name];
						allFarmer.friendshipData.Remove(name);
					}
				}
			}
		}

		public static List<Item> getShopStock(bool Pierres)
		{
			List<Item> list = new List<Item>();
			if (Pierres)
			{
				if (Game1.currentSeason.Equals("spring"))
				{
					list.Add(new Object(Vector2.Zero, 472, 2147483647));
					list.Add(new Object(Vector2.Zero, 473, 2147483647));
					list.Add(new Object(Vector2.Zero, 474, 2147483647));
					list.Add(new Object(Vector2.Zero, 475, 2147483647));
					list.Add(new Object(Vector2.Zero, 427, 2147483647));
					list.Add(new Object(Vector2.Zero, 429, 2147483647));
					list.Add(new Object(Vector2.Zero, 477, 2147483647));
					list.Add(new Object(628, 2147483647, isRecipe: false, 1700));
					list.Add(new Object(629, 2147483647, isRecipe: false, 1000));
					if (Game1.year > 1)
					{
						list.Add(new Object(Vector2.Zero, 476, 2147483647));
					}
				}
				if (Game1.currentSeason.Equals("summer"))
				{
					list.Add(new Object(Vector2.Zero, 480, 2147483647));
					list.Add(new Object(Vector2.Zero, 482, 2147483647));
					list.Add(new Object(Vector2.Zero, 483, 2147483647));
					list.Add(new Object(Vector2.Zero, 484, 2147483647));
					list.Add(new Object(Vector2.Zero, 479, 2147483647));
					list.Add(new Object(Vector2.Zero, 302, 2147483647));
					list.Add(new Object(Vector2.Zero, 453, 2147483647));
					list.Add(new Object(Vector2.Zero, 455, 2147483647));
					list.Add(new Object(630, 2147483647, isRecipe: false, 2000));
					list.Add(new Object(631, 2147483647, isRecipe: false, 3000));
					if (Game1.year > 1)
					{
						list.Add(new Object(Vector2.Zero, 485, 2147483647));
					}
				}
				if (Game1.currentSeason.Equals("fall"))
				{
					list.Add(new Object(Vector2.Zero, 487, 2147483647));
					list.Add(new Object(Vector2.Zero, 488, 2147483647));
					list.Add(new Object(Vector2.Zero, 490, 2147483647));
					list.Add(new Object(Vector2.Zero, 299, 2147483647));
					list.Add(new Object(Vector2.Zero, 301, 2147483647));
					list.Add(new Object(Vector2.Zero, 492, 2147483647));
					list.Add(new Object(Vector2.Zero, 491, 2147483647));
					list.Add(new Object(Vector2.Zero, 493, 2147483647));
					list.Add(new Object(431, 2147483647, isRecipe: false, 100));
					list.Add(new Object(Vector2.Zero, 425, 2147483647));
					list.Add(new Object(632, 2147483647, isRecipe: false, 3000));
					list.Add(new Object(633, 2147483647, isRecipe: false, 2000));
					if (Game1.year > 1)
					{
						list.Add(new Object(Vector2.Zero, 489, 2147483647));
					}
				}
				list.Add(new Object(Vector2.Zero, 297, 2147483647));
				list.Add(new Object(Vector2.Zero, 245, 2147483647));
				list.Add(new Object(Vector2.Zero, 246, 2147483647));
				list.Add(new Object(Vector2.Zero, 423, 2147483647));
				Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
				list.Add(new Wallpaper(random.Next(112))
				{
					Stack = 2147483647
				});
				list.Add(new Wallpaper(random.Next(40), isFloor: true)
				{
					Stack = 2147483647
				});
				list.Add(new Clothing(1000 + random.Next(128))
				{
					Stack = 2147483647,
					Price = 1000
				});
				if (Game1.player.achievements.Contains(38))
				{
					list.Add(new Object(Vector2.Zero, 458, 2147483647));
				}
			}
			else
			{
				if (Game1.currentSeason.Equals("spring"))
				{
					list.Add(new Object(Vector2.Zero, 478, 2147483647));
				}
				if (Game1.currentSeason.Equals("summer"))
				{
					list.Add(new Object(Vector2.Zero, 486, 2147483647));
					list.Add(new Object(Vector2.Zero, 481, 2147483647));
				}
				if (Game1.currentSeason.Equals("fall"))
				{
					list.Add(new Object(Vector2.Zero, 493, 2147483647));
					list.Add(new Object(Vector2.Zero, 494, 2147483647));
				}
				list.Add(new Object(Vector2.Zero, 88, 2147483647));
				list.Add(new Object(Vector2.Zero, 90, 2147483647));
			}
			return list;
		}

		public static Vector2 getCornersOfThisRectangle(ref Microsoft.Xna.Framework.Rectangle r, int corner)
		{
			return corner switch
			{
				1 => new Vector2(r.Right - 1, r.Y), 
				2 => new Vector2(r.Right - 1, r.Bottom - 1), 
				3 => new Vector2(r.X, r.Bottom - 1), 
				_ => new Vector2(r.X, r.Y), 
			};
		}

		private static int priceForToolUpgradeLevel(int level)
		{
			return level switch
			{
				1 => 2000, 
				2 => 5000, 
				3 => 10000, 
				4 => 25000, 
				_ => 2000, 
			};
		}

		private static int indexOfExtraMaterialForToolUpgrade(int level)
		{
			return level switch
			{
				1 => 334, 
				2 => 335, 
				3 => 336, 
				4 => 337, 
				_ => 334, 
			};
		}

		public static Dictionary<ISalable, int[]> getBlacksmithUpgradeStock(Farmer who)
		{
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			Tool toolFromName = who.getToolFromName("Axe");
			Tool toolFromName2 = who.getToolFromName("Watering Can");
			Tool toolFromName3 = who.getToolFromName("Pickaxe");
			Tool toolFromName4 = who.getToolFromName("Hoe");
			if (toolFromName != null && (int)toolFromName.upgradeLevel < 4)
			{
				Tool tool = new Axe();
				tool.UpgradeLevel = (int)toolFromName.upgradeLevel + 1;
				dictionary.Add(tool, new int[3]
				{
					priceForToolUpgradeLevel(tool.UpgradeLevel),
					1,
					indexOfExtraMaterialForToolUpgrade(tool.upgradeLevel)
				});
			}
			if (toolFromName2 != null && (int)toolFromName2.upgradeLevel < 4)
			{
				Tool tool2 = new WateringCan();
				tool2.UpgradeLevel = (int)toolFromName2.upgradeLevel + 1;
				dictionary.Add(tool2, new int[3]
				{
					priceForToolUpgradeLevel(tool2.UpgradeLevel),
					1,
					indexOfExtraMaterialForToolUpgrade(tool2.upgradeLevel)
				});
			}
			if (toolFromName3 != null && (int)toolFromName3.upgradeLevel < 4)
			{
				Tool tool3 = new Pickaxe();
				tool3.UpgradeLevel = (int)toolFromName3.upgradeLevel + 1;
				dictionary.Add(tool3, new int[3]
				{
					priceForToolUpgradeLevel(tool3.UpgradeLevel),
					1,
					indexOfExtraMaterialForToolUpgrade(tool3.upgradeLevel)
				});
			}
			if (toolFromName4 != null && (int)toolFromName4.upgradeLevel < 4)
			{
				Tool tool4 = new Hoe();
				tool4.UpgradeLevel = (int)toolFromName4.upgradeLevel + 1;
				dictionary.Add(tool4, new int[3]
				{
					priceForToolUpgradeLevel(tool4.UpgradeLevel),
					1,
					indexOfExtraMaterialForToolUpgrade(tool4.upgradeLevel)
				});
			}
			if (who.trashCanLevel < 4)
			{
				string name = "";
				switch (who.trashCanLevel + 1)
				{
				case 1:
					name = Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14299", Game1.content.LoadString("Strings\\StringsFromCSFiles:TrashCan"));
					break;
				case 2:
					name = Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14300", Game1.content.LoadString("Strings\\StringsFromCSFiles:TrashCan"));
					break;
				case 3:
					name = Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14301", Game1.content.LoadString("Strings\\StringsFromCSFiles:TrashCan"));
					break;
				case 4:
					name = Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14302", Game1.content.LoadString("Strings\\StringsFromCSFiles:TrashCan"));
					break;
				}
				Tool key = new GenericTool(name, Game1.content.LoadString("Strings\\StringsFromCSFiles:TrashCan_Description", ((who.trashCanLevel + 1) * 15).ToString() ?? ""), who.trashCanLevel + 1, 13 + who.trashCanLevel, 13 + who.trashCanLevel);
				dictionary.Add(key, new int[3]
				{
					priceForToolUpgradeLevel(who.trashCanLevel + 1) / 2,
					1,
					indexOfExtraMaterialForToolUpgrade(who.trashCanLevel + 1)
				});
			}
			return dictionary;
		}

		public static Vector2 GetCurvePoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
		{
			float num = 3f * (p1.X - p0.X);
			float num2 = 3f * (p1.Y - p0.Y);
			float num3 = 3f * (p2.X - p1.X) - num;
			float num4 = 3f * (p2.Y - p1.Y) - num2;
			float num5 = p3.X - p0.X - num - num3;
			float num6 = p3.Y - p0.Y - num2 - num4;
			float num7 = t * t * t;
			float num8 = t * t;
			float x = num5 * num7 + num3 * num8 + num * t + p0.X;
			float y = num6 * num7 + num4 * num8 + num2 * t + p0.Y;
			return new Vector2(x, y);
		}

		public static GameLocation getGameLocationOfCharacter(NPC n)
		{
			return n.currentLocation;
		}

		public static int[] parseStringToIntArray(string s, char delimiter = ' ')
		{
			string[] array = s.Split(delimiter);
			int[] array2 = new int[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = Convert.ToInt32(array[i]);
			}
			return array2;
		}

		public static void drawLineWithScreenCoordinates(int x1, int y1, int x2, int y2, SpriteBatch b, Color color1, float layerDepth = 1f)
		{
			Vector2 vector = new Vector2(x2, y2);
			Vector2 vector2 = new Vector2(x1, y1);
			Vector2 vector3 = vector - vector2;
			float rotation = (float)Math.Atan2(vector3.Y, vector3.X);
			b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((int)vector2.X, (int)vector2.Y, (int)vector3.Length(), 1), null, color1, rotation, new Vector2(0f, 0f), SpriteEffects.None, layerDepth);
			b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((int)vector2.X, (int)vector2.Y + 1, (int)vector3.Length(), 1), null, color1, rotation, new Vector2(0f, 0f), SpriteEffects.None, layerDepth);
		}

		public static string getRandomNonLoopingSong()
		{
			return Game1.random.Next(7) switch
			{
				0 => "springsongs", 
				1 => "summersongs", 
				2 => "fallsongs", 
				3 => "wintersongs", 
				4 => "EarthMine", 
				5 => "FrostMine", 
				6 => "LavaMine", 
				_ => "fallsongs", 
			};
		}

		public static Farmer isThereAFarmerWithinDistance(Vector2 tileLocation, int tilesAway, GameLocation location)
		{
			foreach (Farmer farmer in location.farmers)
			{
				if (Math.Abs(tileLocation.X - farmer.getTileLocation().X) <= (float)tilesAway && Math.Abs(tileLocation.Y - farmer.getTileLocation().Y) <= (float)tilesAway)
				{
					return farmer;
				}
			}
			return null;
		}

		public static Character isThereAFarmerOrCharacterWithinDistance(Vector2 tileLocation, int tilesAway, GameLocation environment)
		{
			foreach (NPC character in environment.characters)
			{
				if (Vector2.Distance(character.getTileLocation(), tileLocation) <= (float)tilesAway)
				{
					return character;
				}
			}
			return isThereAFarmerWithinDistance(tileLocation, tilesAway, environment);
		}

		public static Color getRedToGreenLerpColor(float power)
		{
			return new Color((int)((power <= 0.5f) ? 255f : ((1f - power) * 2f * 255f)), (int)Math.Min(255f, power * 2f * 255f), 0);
		}

		public static FarmHouse getHomeOfFarmer(Farmer who)
		{
			return Game1.getLocationFromName(who.homeLocation) as FarmHouse;
		}

		public static Vector2 getRandomPositionOnScreen()
		{
			return new Vector2(Game1.random.Next(Game1.viewport.Width), Game1.random.Next(Game1.viewport.Height));
		}

		public static Vector2 getRandomPositionOnScreenNotOnMap()
		{
			Vector2 vector = Vector2.Zero;
			int i;
			for (i = 0; i < 30; i++)
			{
				if (!vector.Equals(Vector2.Zero) && !Game1.currentLocation.isTileOnMap((vector + new Vector2(Game1.viewport.X, Game1.viewport.Y)) / 64f))
				{
					break;
				}
				vector = getRandomPositionOnScreen();
			}
			if (i >= 30)
			{
				return new Vector2(-1000f, -1000f);
			}
			return vector;
		}

		public static Microsoft.Xna.Framework.Rectangle getRectangleCenteredAt(Vector2 v, int size)
		{
			return new Microsoft.Xna.Framework.Rectangle((int)v.X - size / 2, (int)v.Y - size / 2, size, size);
		}

		public static bool checkForCharacterInteractionAtTile(Vector2 tileLocation, Farmer who)
		{
			NPC nPC = Game1.currentLocation.isCharacterAtTile(tileLocation);
			if (nPC != null && !nPC.IsMonster && !nPC.IsInvisible)
			{
				if (Game1.currentLocation is MovieTheater)
				{
					Game1.mouseCursor = 4;
				}
				else if (who.ActiveObject != null && who.ActiveObject.canBeGivenAsGift() && !who.isRidingHorse() && nPC.isVillager() && ((who.friendshipData.ContainsKey(nPC.Name) && who.friendshipData[nPC.Name].GiftsToday != 1) || Game1.NPCGiftTastes.ContainsKey(nPC.Name)) && !Game1.eventUp)
				{
					Game1.mouseCursor = 3;
				}
				else if (nPC.canTalk() && ((nPC.CurrentDialogue != null && nPC.CurrentDialogue.Count > 0) || (Game1.player.spouse != null && nPC.Name != null && nPC.Name == Game1.player.spouse && nPC.shouldSayMarriageDialogue.Value && nPC.currentMarriageDialogue != null && nPC.currentMarriageDialogue.Count > 0) || nPC.hasTemporaryMessageAvailable() || (who.hasClubCard && nPC.Name.Equals("Bouncer") && who.IsLocalPlayer) || (nPC.Name.Equals("Henchman") && nPC.currentLocation.Name.Equals("WitchSwamp") && !who.hasOrWillReceiveMail("henchmanGone"))) && !nPC.isOnSilentTemporaryMessage())
				{
					Game1.mouseCursor = 4;
				}
				if (Game1.eventUp && Game1.CurrentEvent != null && !Game1.CurrentEvent.playerControlSequence)
				{
					Game1.mouseCursor = 0;
				}
				Game1.currentLocation.checkForSpecialCharacterIconAtThisTile(tileLocation);
				if (Game1.mouseCursor == 3 || Game1.mouseCursor == 4)
				{
					if (tileWithinRadiusOfPlayer((int)tileLocation.X, (int)tileLocation.Y, 1, who))
					{
						Game1.mouseCursorTransparency = 1f;
					}
					else
					{
						Game1.mouseCursorTransparency = 0.5f;
					}
				}
				return true;
			}
			return false;
		}

		public static bool canGrabSomethingFromHere(int x, int y, Farmer who)
		{
			if (Game1.currentLocation == null)
			{
				return false;
			}
			Vector2 vector = new Vector2(x / 64, y / 64);
			if (Game1.currentLocation.isObjectAt(x, y))
			{
				Game1.currentLocation.getObjectAt(x, y).hoverAction();
			}
			if (checkForCharacterInteractionAtTile(vector, who))
			{
				return false;
			}
			if (checkForCharacterInteractionAtTile(vector + new Vector2(0f, 1f), who))
			{
				return false;
			}
			if (who.IsLocalPlayer)
			{
				if (who.onBridge.Value)
				{
					return false;
				}
				if (Game1.currentLocation != null)
				{
					foreach (Furniture item in Game1.currentLocation.furniture)
					{
						if (item.getBoundingBox(item.TileLocation).Contains(Vector2ToPoint(vector * 64f)) && item.Name.Contains("Table") && item.heldObject.Value != null)
						{
							return true;
						}
					}
				}
				if (Game1.currentLocation.Objects.ContainsKey(vector))
				{
					if ((bool)Game1.currentLocation.Objects[vector].readyForHarvest || (bool)Game1.currentLocation.Objects[vector].isSpawnedObject || (Game1.currentLocation.Objects[vector] is IndoorPot && (Game1.currentLocation.Objects[vector] as IndoorPot).hoeDirt.Value.readyForHarvest()))
					{
						Game1.mouseCursor = 6;
						if (!withinRadiusOfPlayer(x, y, 1, who))
						{
							Game1.mouseCursorTransparency = 0.5f;
							return false;
						}
						return true;
					}
				}
				else if (Game1.currentLocation.terrainFeatures.ContainsKey(vector) && Game1.currentLocation.terrainFeatures[vector] is HoeDirt && ((HoeDirt)Game1.currentLocation.terrainFeatures[vector]).readyForHarvest())
				{
					Game1.mouseCursor = 6;
					if (!withinRadiusOfPlayer(x, y, 1, who))
					{
						Game1.mouseCursorTransparency = 0.5f;
						return false;
					}
					return true;
				}
			}
			return false;
		}

		public static Microsoft.Xna.Framework.Rectangle getSourceRectWithinRectangularRegion(int regionX, int regionY, int regionWidth, int sourceIndex, int sourceWidth, int sourceHeight)
		{
			int num = regionWidth / sourceWidth;
			return new Microsoft.Xna.Framework.Rectangle(regionX + sourceIndex % num * sourceWidth, regionY + sourceIndex / num * sourceHeight, sourceWidth, sourceHeight);
		}

		public static void drawWithShadow(SpriteBatch b, Texture2D texture, Vector2 position, Microsoft.Xna.Framework.Rectangle sourceRect, Color color, float rotation, Vector2 origin, float scale = -1f, bool flipped = false, float layerDepth = -1f, int horizontalShadowOffset = -1, int verticalShadowOffset = -1, float shadowIntensity = 0.35f)
		{
			if (scale == -1f)
			{
				scale = 4f;
			}
			if (layerDepth == -1f)
			{
				layerDepth = position.Y / 10000f;
			}
			if (horizontalShadowOffset == -1)
			{
				horizontalShadowOffset = -4;
			}
			if (verticalShadowOffset == -1)
			{
				verticalShadowOffset = 4;
			}
			b.Draw(texture, position + new Vector2(horizontalShadowOffset, verticalShadowOffset), sourceRect, Color.Black * shadowIntensity, rotation, origin, scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth - 0.0001f);
			b.Draw(texture, position, sourceRect, color, rotation, origin, scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
		}

		public static int To4(int v)
		{
			int num = v % 4;
			int num2 = v - num;
			if (num > 2)
			{
				num2 += 4;
			}
			return num2;
		}

		public static Vector2 To4(Vector2 v)
		{
			int num = To4((int)v.X);
			int num2 = To4((int)v.Y);
			return new Vector2(num, num2);
		}

		public static xTile.Dimensions.Rectangle To4(xTile.Dimensions.Rectangle v)
		{
			int x = To4(v.X);
			int y = To4(v.Y);
			int width = To4(v.Width);
			int height = To4(v.Height);
			return new xTile.Dimensions.Rectangle(x, y, width, height);
		}

		public static void drawTextWithShadow(SpriteBatch b, StringBuilder text, SpriteFont font, Vector2 position, Color color, float scale = 1f, float layerDepth = -1f, int horizontalShadowOffset = -1, int verticalShadowOffset = -1, float shadowIntensity = 1f, int numShadows = 3)
		{
			position = To4(position);
			if (layerDepth == -1f)
			{
				layerDepth = position.Y / 10000f;
			}
			bool flag = Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.de;
			if (flag && font == Game1.dialogueFont)
			{
				position = new Vector2(position.X, position.Y + 8f);
			}
			if (horizontalShadowOffset == -1)
			{
				horizontalShadowOffset = ((font.Equals(Game1.smallFont) || flag) ? (-2) : (-3));
			}
			if (verticalShadowOffset == -1)
			{
				verticalShadowOffset = ((font.Equals(Game1.smallFont) || flag) ? 2 : 3);
			}
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, verticalShadowOffset), new Color(221, 148, 84) * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0001f);
			if (numShadows == 2)
			{
				b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, 0f), new Color(221, 148, 84) * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0002f);
			}
			if (numShadows == 3)
			{
				b.DrawString(font, text, position + new Vector2(0f, verticalShadowOffset), new Color(221, 148, 84) * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0003f);
			}
			b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
		}

		public static void drawTextWithShadow(SpriteBatch b, string text, SpriteFont font, Vector2 position, Color color, float scale = 1f, float layerDepth = -1f, int horizontalShadowOffset = -1, int verticalShadowOffset = -1, float shadowIntensity = 1f, int numShadows = 3)
		{
			if (layerDepth == -1f)
			{
				layerDepth = position.Y / 10000f;
			}
			bool flag = Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.de || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ko;
			if (horizontalShadowOffset == -1)
			{
				horizontalShadowOffset = ((font.Equals(Game1.smallFont) || flag) ? (-2) : (-3));
			}
			if (verticalShadowOffset == -1)
			{
				verticalShadowOffset = ((font.Equals(Game1.smallFont) || flag) ? 2 : 3);
			}
			if (text == null)
			{
				text = "";
			}
			b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, verticalShadowOffset), new Color(221, 148, 84) * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0001f);
			if (numShadows == 2)
			{
				b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, 0f), new Color(221, 148, 84) * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0002f);
			}
			if (numShadows == 3)
			{
				b.DrawString(font, text, position + new Vector2(0f, verticalShadowOffset), new Color(221, 148, 84) * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0003f);
			}
			b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
		}

		public static void drawTextWithColoredShadow(SpriteBatch b, string text, SpriteFont font, Vector2 position, Color color, Color shadowColor, float scale = 1f, float layerDepth = -1f, int horizontalShadowOffset = -1, int verticalShadowOffset = -1, int numShadows = 3)
		{
			position = To4(position);
			if (layerDepth == -1f)
			{
				layerDepth = position.Y / 10000f;
			}
			bool flag = Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.de;
			if (flag && font == Game1.dialogueFont)
			{
				position = new Vector2(position.X, position.Y + 8f);
			}
			if (horizontalShadowOffset == -1)
			{
				horizontalShadowOffset = ((font.Equals(Game1.smallFont) || flag) ? (-2) : (-3));
			}
			if (verticalShadowOffset == -1)
			{
				verticalShadowOffset = ((font.Equals(Game1.smallFont) || flag) ? 2 : 3);
			}
			if (text == null)
			{
				text = "";
			}
			b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, verticalShadowOffset), shadowColor, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0001f);
			if (numShadows == 2)
			{
				b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, 0f), shadowColor, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0002f);
			}
			if (numShadows == 3)
			{
				b.DrawString(font, text, position + new Vector2(0f, verticalShadowOffset), shadowColor, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0003f);
			}
			b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
		}

		public static void drawBoldText(SpriteBatch b, string text, SpriteFont font, Vector2 position, Color color, float scale = 1f, float layerDepth = -1f, int boldnessOffset = 1)
		{
			position = To4(position);
			if (layerDepth == -1f)
			{
				layerDepth = position.Y / 10000f;
			}
			b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
			b.DrawString(font, text, position + new Vector2(boldnessOffset, 0f), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
			b.DrawString(font, text, position + new Vector2(boldnessOffset, boldnessOffset), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
			b.DrawString(font, text, position + new Vector2(0f, boldnessOffset), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
		}

		protected static bool _HasNonMousePlacementLeeway(int x, int y, Item item, Farmer f)
		{
			if (!Game1.isCheckingNonMousePlacement)
			{
				return false;
			}
			Point tileLocationPoint = f.getTileLocationPoint();
			if (!withinRadiusOfPlayer(x, y, 2, f))
			{
				return false;
			}
			if (item is Object && ((Object)item).Category == -74)
			{
				return true;
			}
			foreach (Point item2 in GetPointsOnLine(tileLocationPoint.X, tileLocationPoint.Y, x / 64, y / 64))
			{
				if (!(item2 == tileLocationPoint) && !item.canBePlacedHere(f.currentLocation, new Vector2(item2.X, item2.Y)))
				{
					return false;
				}
			}
			return true;
		}

		public static bool isPlacementForbiddenHere(GameLocation location)
		{
			if (location == null)
			{
				return true;
			}
			return isPlacementForbiddenHere(location.name);
		}

		public static bool isPlacementForbiddenHere(string location_name)
		{
			if (location_name == "AbandonedJojaMart")
			{
				return true;
			}
			if (location_name == "BeachNightMarket")
			{
				return true;
			}
			return false;
		}

		public static void transferPlacedObjectsFromOneLocationToAnother(GameLocation source, GameLocation destination, Vector2? overflow_chest_position = null, GameLocation overflow_chest_location = null)
		{
			if (source == null)
			{
				return;
			}
			List<Item> list = new List<Item>();
			List<Vector2> list2 = new List<Vector2>(source.objects.Keys);
			foreach (Vector2 item in list2)
			{
				if (source.objects[item] == null)
				{
					continue;
				}
				Object @object = source.objects[item];
				bool flag = true;
				if (destination == null)
				{
					flag = false;
				}
				if (flag && destination.objects.ContainsKey(item))
				{
					flag = false;
				}
				if (flag && !destination.isTileLocationTotallyClearAndPlaceable(item))
				{
					flag = false;
				}
				source.objects.Remove(item);
				if (flag && destination != null)
				{
					destination.objects[item] = @object;
					continue;
				}
				list.Add(@object);
				if (!(@object is Chest))
				{
					continue;
				}
				Chest chest = @object as Chest;
				List<Item> list3 = new List<Item>(chest.items);
				chest.items.Clear();
				foreach (Item item2 in list3)
				{
					if (item2 != null)
					{
						list.Add(item2);
					}
				}
			}
			if (overflow_chest_position.HasValue)
			{
				if (overflow_chest_location != null)
				{
					createOverflowChest(overflow_chest_location, overflow_chest_position.Value, list);
				}
				else if (destination != null)
				{
					createOverflowChest(destination, overflow_chest_position.Value, list);
				}
			}
		}

		public static void createOverflowChest(GameLocation destination, Vector2 overflow_chest_location, List<Item> overflow_items)
		{
			List<Chest> list = new List<Chest>();
			foreach (Item overflow_item in overflow_items)
			{
				if (overflow_item == null)
				{
					continue;
				}
				if (list.Count == 0)
				{
					list.Add(new Chest(playerChest: true));
				}
				bool flag = false;
				foreach (Chest item in list)
				{
					if (item.addItem(overflow_item) == null)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					Chest chest = new Chest(playerChest: true);
					chest.addItem(overflow_item);
					list.Add(chest);
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				Chest o = list[i];
				_placeOverflowChestInNearbySpace(destination, overflow_chest_location, o);
			}
		}

		protected static void _placeOverflowChestInNearbySpace(GameLocation location, Vector2 tileLocation, Object o)
		{
			if (o == null || tileLocation.Equals(Vector2.Zero))
			{
				return;
			}
			int i = 0;
			Queue<Vector2> queue = new Queue<Vector2>();
			HashSet<Vector2> hashSet = new HashSet<Vector2>();
			queue.Enqueue(tileLocation);
			Vector2 vector = Vector2.Zero;
			for (; i < 100; i++)
			{
				vector = queue.Dequeue();
				if (!location.isTileOccupiedForPlacement(vector) && location.isTileLocationTotallyClearAndPlaceable(vector) && !location.isOpenWater((int)vector.X, (int)vector.Y))
				{
					break;
				}
				hashSet.Add(vector);
				foreach (Vector2 adjacentTileLocation in getAdjacentTileLocations(vector))
				{
					if (!hashSet.Contains(adjacentTileLocation))
					{
						queue.Enqueue(adjacentTileLocation);
					}
				}
			}
			if (!vector.Equals(Vector2.Zero) && !location.isTileOccupiedForPlacement(vector) && !location.isOpenWater((int)vector.X, (int)vector.Y) && location.isTileLocationTotallyClearAndPlaceable(vector))
			{
				o.tileLocation.Value = vector;
				location.objects.Add(vector, o);
			}
		}

		public static bool isWithinTileWithLeeway(int x, int y, Item item, Farmer f)
		{
			if (!withinRadiusOfPlayer(x, y, 1, f))
			{
				return _HasNonMousePlacementLeeway(x, y, item, f);
			}
			return true;
		}

		public static bool playerCanPlaceItemHere(GameLocation location, Item item, int x, int y, Farmer f)
		{
			if (isPlacementForbiddenHere(location))
			{
				return false;
			}
			if (item == null || item is Tool || Game1.eventUp || (bool)f.bathingClothes || f.onBridge.Value)
			{
				return false;
			}
			if (isWithinTileWithLeeway(x, y, item, f) || (item is Wallpaper && location is DecoratableLocation) || (item is Furniture && location.CanPlaceThisFurnitureHere(item as Furniture)))
			{
				if (item is Furniture)
				{
					Furniture furniture = item as Furniture;
					if (!location.CanFreePlaceFurniture() && !furniture.IsCloseEnoughToFarmer(f, x / 64, y / 64))
					{
						return false;
					}
				}
				Vector2 vector = new Vector2(x / 64, y / 64);
				Object objectAtTile = location.getObjectAtTile((int)vector.X, (int)vector.Y);
				if (objectAtTile != null && objectAtTile is Fence && (objectAtTile as Fence).CanRepairWithThisItem(item))
				{
					return true;
				}
				if (item.canBePlacedHere(location, vector))
				{
					if (item is Wallpaper)
					{
						return true;
					}
					if (!((Object)item).isPassable())
					{
						foreach (Farmer farmer in location.farmers)
						{
							if (farmer.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle((int)vector.X * 64, (int)vector.Y * 64, 64, 64)))
							{
								return false;
							}
						}
					}
					if (itemCanBePlaced(location, vector, item) || isViableSeedSpot(location, vector, item))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static int GetDoubleWideVersionOfBed(int bed_index)
		{
			return bed_index + 4;
		}

		private static bool itemCanBePlaced(GameLocation location, Vector2 tileLocation, Item item)
		{
			if (location.isTilePlaceable(tileLocation, item) && item.isPlaceable() && (item.Category != -74 || (item is Object && (item as Object).isSapling())))
			{
				if (!((Object)item).isPassable())
				{
					return !new Microsoft.Xna.Framework.Rectangle((int)(tileLocation.X * 64f), (int)(tileLocation.Y * 64f), 64, 64).Intersects(Game1.player.GetBoundingBox());
				}
				return true;
			}
			return false;
		}

		public static bool isViableSeedSpot(GameLocation location, Vector2 tileLocation, Item item)
		{
			if (((Object)item).Category == -74)
			{
				if ((!location.terrainFeatures.ContainsKey(tileLocation) || !(location.terrainFeatures[tileLocation] is HoeDirt) || !((HoeDirt)location.terrainFeatures[tileLocation]).canPlantThisSeedHere((item as Object).ParentSheetIndex, (int)tileLocation.X, (int)tileLocation.Y)) && (!location.objects.ContainsKey(tileLocation) || !(location.objects[tileLocation] is IndoorPot) || !(location.objects[tileLocation] as IndoorPot).hoeDirt.Value.canPlantThisSeedHere((item as Object).ParentSheetIndex, (int)tileLocation.X, (int)tileLocation.Y) || (item as Object).ParentSheetIndex == 499))
				{
					if (location.isTileHoeDirt(tileLocation) || !location.terrainFeatures.ContainsKey(tileLocation))
					{
						return Object.isWildTreeSeed(item.parentSheetIndex);
					}
					return false;
				}
				return true;
			}
			return false;
		}

		public static int getDirectionFromChange(Vector2 current, Vector2 previous, bool yBias = false)
		{
			if (!yBias && current.X > previous.X)
			{
				return 1;
			}
			if (!yBias && current.X < previous.X)
			{
				return 3;
			}
			if (current.Y > previous.Y)
			{
				return 2;
			}
			if (current.Y < previous.Y)
			{
				return 0;
			}
			if (current.X > previous.X)
			{
				return 1;
			}
			if (current.X < previous.X)
			{
				return 3;
			}
			return -1;
		}

		public static bool doesRectangleIntersectTile(Microsoft.Xna.Framework.Rectangle r, int tileX, int tileY)
		{
			Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(tileX * 64, tileY * 64, 64, 64);
			return r.Intersects(value);
		}

		public static List<NPC> getPooledList()
		{
			lock (_pool)
			{
				return _pool.Get();
			}
		}

		public static bool IsHospitalVisitDay(string character_name)
		{
			try
			{
				Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Characters\\schedules\\" + character_name);
				string key = Game1.currentSeason + "_" + Game1.dayOfMonth;
				if (dictionary.ContainsKey(key) && dictionary[key].Contains("Hospital"))
				{
					return true;
				}
			}
			catch (Exception)
			{
			}
			return false;
		}

		public static void returnPooledList(List<NPC> list)
		{
			lock (_pool)
			{
				_pool.Return(list);
			}
		}

		public static List<NPC> getAllCharacters(List<NPC> list)
		{
			list.AddRange(Game1.currentLocation.characters);
			foreach (GameLocation location in Game1.locations)
			{
				if (!location.Equals(Game1.currentLocation))
				{
					list.AddRange(location.characters);
				}
			}
			Farm farm = Game1.getFarm();
			if (farm != null)
			{
				foreach (Building building in farm.buildings)
				{
					if (building.indoors.Value != null)
					{
						foreach (NPC character in building.indoors.Value.characters)
						{
							character.currentLocation = building.indoors;
						}
						list.AddRange(building.indoors.Value.characters);
					}
				}
				return list;
			}
			return list;
		}

		public static DisposableList<NPC> getAllCharacters()
		{
			List<NPC> list;
			lock (_pool)
			{
				list = _pool.Get();
			}
			getAllCharacters(list);
			return new DisposableList<NPC>(list, _pool);
		}

		private static void _recursiveIterateItem(Item i, Action<Item> action)
		{
			if (i == null)
			{
				return;
			}
			if (i is Object)
			{
				Object @object = i as Object;
				if (@object is StorageFurniture)
				{
					foreach (Item heldItem in (@object as StorageFurniture).heldItems)
					{
						if (heldItem != null)
						{
							_recursiveIterateItem(heldItem, action);
						}
					}
				}
				if (@object is Chest)
				{
					foreach (Item item in (@object as Chest).items)
					{
						if (item != null)
						{
							_recursiveIterateItem(item, action);
						}
					}
				}
				if (@object.heldObject.Value != null)
				{
					_recursiveIterateItem((Object)@object.heldObject, action);
				}
			}
			action(i);
		}

		protected static void _recursiveIterateLocation(GameLocation l, Action<Item> action)
		{
			if (l == null)
			{
				return;
			}
			if (l != null)
			{
				foreach (Furniture item in l.furniture)
				{
					_recursiveIterateItem(item, action);
				}
			}
			if (l is IslandFarmHouse)
			{
				foreach (Item item2 in (l as IslandFarmHouse).fridge.Value.items)
				{
					if (item2 != null)
					{
						_recursiveIterateItem(item2, action);
					}
				}
			}
			if (l is FarmHouse)
			{
				foreach (Item item3 in (l as FarmHouse).fridge.Value.items)
				{
					if (item3 != null)
					{
						_recursiveIterateItem(item3, action);
					}
				}
			}
			foreach (NPC character in l.characters)
			{
				if (character is Child && (character as Child).hat.Value != null)
				{
					_recursiveIterateItem((character as Child).hat.Value, action);
				}
				if (character is Horse && (character as Horse).hat.Value != null)
				{
					_recursiveIterateItem((character as Horse).hat.Value, action);
				}
			}
			if (l is BuildableGameLocation)
			{
				foreach (Building building in (l as BuildableGameLocation).buildings)
				{
					if (building.indoors.Value != null)
					{
						_recursiveIterateLocation(building.indoors.Value, action);
					}
					if (building is Mill)
					{
						foreach (Item item4 in (building as Mill).output.Value.items)
						{
							if (item4 != null)
							{
								_recursiveIterateItem(item4, action);
							}
						}
					}
					else
					{
						if (!(building is JunimoHut))
						{
							continue;
						}
						foreach (Item item5 in (building as JunimoHut).output.Value.items)
						{
							if (item5 != null)
							{
								_recursiveIterateItem(item5, action);
							}
						}
					}
				}
			}
			foreach (Object value in l.objects.Values)
			{
				_recursiveIterateItem(value, action);
			}
			foreach (Debris debri in l.debris)
			{
				if (debri.item != null)
				{
					_recursiveIterateItem(debri.item, action);
				}
			}
		}

		public static Item PerformSpecialItemPlaceReplacement(Item placedItem)
		{
			if (placedItem != null && placedItem is Pan)
			{
				return new Hat(71);
			}
			if (placedItem != null && placedItem is Object && (int)(placedItem as Object).parentSheetIndex == 71)
			{
				return new Clothing(15);
			}
			return placedItem;
		}

		public static Item PerformSpecialItemGrabReplacement(Item heldItem)
		{
			if (heldItem != null && heldItem is Clothing && (int)(heldItem as Clothing).parentSheetIndex == 15)
			{
				heldItem = new Object(71, 1);
				Object @object = heldItem as Object;
				@object.questItem.Value = true;
				@object.questId.Value = 102;
			}
			if (heldItem != null && heldItem is Hat && (int)(heldItem as Hat).which == 71)
			{
				heldItem = new Pan();
			}
			return heldItem;
		}

		public static void iterateAllItemsHere(GameLocation location, Action<Item> action)
		{
			_recursiveIterateLocation(location, action);
		}

		public static void iterateAllItems(Action<Item> action)
		{
			foreach (GameLocation location in Game1.locations)
			{
				_recursiveIterateLocation(location, action);
			}
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				foreach (Item item in allFarmer.Items)
				{
					_recursiveIterateItem(item, action);
				}
				_recursiveIterateItem(allFarmer.shirtItem.Value, action);
				_recursiveIterateItem(allFarmer.pantsItem.Value, action);
				_recursiveIterateItem(allFarmer.boots.Value, action);
				_recursiveIterateItem(allFarmer.hat.Value, action);
				_recursiveIterateItem(allFarmer.leftRing.Value, action);
				_recursiveIterateItem(allFarmer.rightRing.Value, action);
				foreach (Item item2 in allFarmer.itemsLostLastDeath)
				{
					_recursiveIterateItem(item2, action);
				}
				if (allFarmer.recoveredItem != null)
				{
					_recursiveIterateItem(allFarmer.recoveredItem, action);
				}
			}
			foreach (Item returnedDonation in Game1.player.team.returnedDonations)
			{
				if (returnedDonation != null)
				{
					action(returnedDonation);
				}
			}
			foreach (Item item3 in Game1.player.team.junimoChest)
			{
				if (item3 != null)
				{
					action(item3);
				}
			}
			foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
			{
				foreach (Item donatedItem in specialOrder.donatedItems)
				{
					if (donatedItem != null)
					{
						action(donatedItem);
					}
				}
			}
		}

		public static void iterateChestsAndStorage(Action<Item> action)
		{
			foreach (GameLocation location in Game1.locations)
			{
				foreach (Object value in location.objects.Values)
				{
					if (value is Chest)
					{
						foreach (Item item in (value as Chest).items)
						{
							if (item != null)
							{
								action(item);
							}
						}
					}
					if (value.heldObject.Value == null || !(value.heldObject.Value is Chest))
					{
						continue;
					}
					foreach (Item item2 in (value.heldObject.Value as Chest).items)
					{
						if (item2 != null)
						{
							action(item2);
						}
					}
				}
				if (location is FarmHouse)
				{
					foreach (Item item3 in (location as FarmHouse).fridge.Value.items)
					{
						if (item3 != null)
						{
							action(item3);
						}
					}
				}
				else if (location is IslandFarmHouse)
				{
					foreach (Item item4 in (location as IslandFarmHouse).fridge.Value.items)
					{
						if (item4 != null)
						{
							action(item4);
						}
					}
				}
				if (location != null)
				{
					foreach (Furniture item5 in location.furniture)
					{
						if (!(item5 is StorageFurniture))
						{
							continue;
						}
						foreach (Item heldItem in (item5 as StorageFurniture).heldItems)
						{
							if (heldItem != null)
							{
								action(heldItem);
							}
						}
					}
				}
				if (!(location is BuildableGameLocation))
				{
					continue;
				}
				foreach (Building building in (location as BuildableGameLocation).buildings)
				{
					if (building.indoors.Value != null)
					{
						foreach (Object value2 in building.indoors.Value.objects.Values)
						{
							if (value2 is Chest)
							{
								foreach (Item item6 in (value2 as Chest).items)
								{
									if (item6 != null)
									{
										action(item6);
									}
								}
							}
							if (value2.heldObject.Value == null || !(value2.heldObject.Value is Chest))
							{
								continue;
							}
							foreach (Item item7 in (value2.heldObject.Value as Chest).items)
							{
								if (item7 != null)
								{
									action(item7);
								}
							}
						}
						if (building.indoors.Value == null)
						{
							continue;
						}
						foreach (Furniture item8 in building.indoors.Value.furniture)
						{
							if (!(item8 is StorageFurniture))
							{
								continue;
							}
							foreach (Item heldItem2 in (item8 as StorageFurniture).heldItems)
							{
								if (heldItem2 != null)
								{
									action(heldItem2);
								}
							}
						}
					}
					else if (building is Mill)
					{
						foreach (Item item9 in (building as Mill).output.Value.items)
						{
							if (item9 != null)
							{
								action(item9);
							}
						}
					}
					else
					{
						if (!(building is JunimoHut))
						{
							continue;
						}
						foreach (Item item10 in (building as JunimoHut).output.Value.items)
						{
							if (item10 != null)
							{
								action(item10);
							}
						}
					}
				}
			}
			foreach (Item returnedDonation in Game1.player.team.returnedDonations)
			{
				if (returnedDonation != null)
				{
					action(returnedDonation);
				}
			}
			foreach (Item item11 in Game1.player.team.junimoChest)
			{
				if (item11 != null)
				{
					action(item11);
				}
			}
			foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
			{
				foreach (Item donatedItem in specialOrder.donatedItems)
				{
					if (donatedItem != null)
					{
						action(donatedItem);
					}
				}
			}
		}

		public static Item removeItemFromInventory(int whichItemIndex, IList<Item> items)
		{
			if (whichItemIndex >= 0 && whichItemIndex < items.Count && items[whichItemIndex] != null)
			{
				Item item = items[whichItemIndex];
				if (whichItemIndex == Game1.player.CurrentToolIndex && items.Equals(Game1.player.items))
				{
					item?.actionWhenStopBeingHeld(Game1.player);
				}
				items[whichItemIndex] = null;
				return item;
			}
			return null;
		}

		public static void iterateAllCrops(Action<Crop> action)
		{
			foreach (GameLocation location in Game1.locations)
			{
				_recursiveIterateLocationCrops(location, action);
			}
		}

		protected static void _recursiveIterateLocationCrops(GameLocation l, Action<Crop> action)
		{
			if (l == null)
			{
				return;
			}
			if (l is BuildableGameLocation)
			{
				foreach (Building building in (l as BuildableGameLocation).buildings)
				{
					if (building.indoors.Value != null)
					{
						_recursiveIterateLocationCrops(building.indoors.Value, action);
					}
				}
			}
			foreach (TerrainFeature value in l.terrainFeatures.Values)
			{
				if (value is HoeDirt && (value as HoeDirt).crop != null)
				{
					action((value as HoeDirt).crop);
				}
			}
			foreach (Object value2 in l.objects.Values)
			{
				if (value2 is IndoorPot && (value2 as IndoorPot).hoeDirt.Value != null && (value2 as IndoorPot).hoeDirt.Value.crop != null)
				{
					action((value2 as IndoorPot).hoeDirt.Value.crop);
				}
			}
		}

		public static void checkItemFirstInventoryAdd(Item item)
		{
			if (!(item is Object) || item.HasBeenInInventory)
			{
				return;
			}
			if (!(item is Furniture) && !(item as Object).bigCraftable && !(item as Object).hasBeenPickedUpByFarmer)
			{
				Game1.player.checkForQuestComplete(null, (item as Object).parentSheetIndex, (item as Object).stack, item, null, 9);
			}
			if (Game1.player.team.specialOrders != null)
			{
				foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
				{
					if (specialOrder.onItemCollected != null)
					{
						specialOrder.onItemCollected(Game1.player, item);
					}
				}
			}
			item.HasBeenInInventory = true;
			(item as Object).hasBeenPickedUpByFarmer.Value = true;
			if ((bool)(item as Object).questItem)
			{
				if (IsNormalObjectAtParentSheetIndex(item, 875) && !Game1.MasterPlayer.hasOrWillReceiveMail("ectoplasmDrop") && Game1.player.team.SpecialOrderActive("Wizard"))
				{
					Game1.addMailForTomorrow("ectoplasmDrop", noLetter: true, sendToEveryone: true);
				}
				else if (IsNormalObjectAtParentSheetIndex(item, 876) && !Game1.MasterPlayer.hasOrWillReceiveMail("prismaticJellyDrop") && Game1.player.team.SpecialOrderActive("Wizard2"))
				{
					Game1.addMailForTomorrow("prismaticJellyDrop", noLetter: true, sendToEveryone: true);
				}
				if (IsNormalObjectAtParentSheetIndex(item, 897) && !Game1.MasterPlayer.hasOrWillReceiveMail("gotMissingStocklist"))
				{
					Game1.addMailForTomorrow("gotMissingStocklist", noLetter: true, sendToEveryone: true);
				}
				return;
			}
			if (item is Object && (item as Object).bigCraftable.Value && item.ParentSheetIndex == 256 && !Game1.MasterPlayer.hasOrWillReceiveMail("gotFirstJunimoChest"))
			{
				Game1.addMailForTomorrow("gotFirstJunimoChest", noLetter: true, sendToEveryone: true);
			}
			if (IsNormalObjectAtParentSheetIndex(item, item.ParentSheetIndex))
			{
				switch ((int)(item as Object).parentSheetIndex)
				{
				case 535:
					if (Game1.activeClickableMenu == null && !Game1.player.hasOrWillReceiveMail("geodeFound"))
					{
						Game1.player.mailReceived.Add("geodeFound");
						Game1.player.holdUpItemThenMessage(item);
					}
					break;
				case 378:
					if (!Game1.player.hasOrWillReceiveMail("copperFound"))
					{
						Game1.addMailForTomorrow("copperFound", noLetter: true);
					}
					break;
				case 428:
					if (!Game1.player.hasOrWillReceiveMail("clothFound"))
					{
						Game1.addMailForTomorrow("clothFound", noLetter: true);
					}
					break;
				case 102:
					Game1.stats.NotesFound++;
					break;
				case 390:
					Game1.stats.StoneGathered++;
					if (Game1.stats.StoneGathered >= 100 && !Game1.player.hasOrWillReceiveMail("robinWell"))
					{
						Game1.addMailForTomorrow("robinWell");
					}
					break;
				case 74:
					Game1.stats.PrismaticShardsFound++;
					break;
				case 72:
					Game1.stats.DiamondsFound++;
					break;
				}
			}
			else if (item is Object && (item as Object).bigCraftable.Value)
			{
				int parentSheetIndex = (item as Object).ParentSheetIndex;
				if (parentSheetIndex == 248)
				{
					Game1.netWorldState.Value.MiniShippingBinsObtained.Value = Game1.netWorldState.Value.MiniShippingBinsObtained.Value + 1;
				}
			}
			if (item is Object)
			{
				Game1.player.checkForQuestComplete(null, item.parentSheetIndex, item.Stack, item, "", 10);
			}
		}

		public static NPC getRandomTownNPC()
		{
			return getRandomTownNPC(Game1.random);
		}

		public static NPC getRandomTownNPC(Random r)
		{
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			int index = r.Next(dictionary.Count);
			NPC characterFromName = Game1.getCharacterFromName(dictionary.ElementAt(index).Key);
			while (dictionary.ElementAt(index).Key.Equals("Wizard") || dictionary.ElementAt(index).Key.Equals("Krobus") || dictionary.ElementAt(index).Key.Equals("Sandy") || dictionary.ElementAt(index).Key.Equals("Dwarf") || dictionary.ElementAt(index).Key.Equals("Marlon") || (dictionary.ElementAt(index).Key.Equals("Leo") && !Game1.MasterPlayer.mailReceived.Contains("addedParrotBoy")) || characterFromName == null)
			{
				index = r.Next(dictionary.Count);
				characterFromName = Game1.getCharacterFromName(dictionary.ElementAt(index).Key);
			}
			return characterFromName;
		}

		public static NPC getTownNPCByGiftTasteIndex(int index)
		{
			Dictionary<string, string> source = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			NPC characterFromName = Game1.getCharacterFromName(source.ElementAt(index).Key);
			int num = (index += 10);
			num %= 25;
			while (characterFromName == null)
			{
				characterFromName = Game1.getCharacterFromName(source.ElementAt(num).Key);
				num++;
				num %= 30;
			}
			return characterFromName;
		}

		public static bool foundAllStardrops(Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			if (who.mailReceived.Contains("gotMaxStamina"))
			{
				return true;
			}
			if (who.hasOrWillReceiveMail("CF_Fair") && who.hasOrWillReceiveMail("CF_Fish") && who.hasOrWillReceiveMail("CF_Mines") && who.hasOrWillReceiveMail("CF_Sewer") && who.hasOrWillReceiveMail("museumComplete") && who.hasOrWillReceiveMail("CF_Spouse"))
			{
				return who.hasOrWillReceiveMail("CF_Statue");
			}
			return false;
		}

		public static int getGrandpaScore()
		{
			int num = 0;
			if (Game1.player.totalMoneyEarned >= 50000)
			{
				num++;
			}
			if (Game1.player.totalMoneyEarned >= 100000)
			{
				num++;
			}
			if (Game1.player.totalMoneyEarned >= 200000)
			{
				num++;
			}
			if (Game1.player.totalMoneyEarned >= 300000)
			{
				num++;
			}
			if (Game1.player.totalMoneyEarned >= 500000)
			{
				num++;
			}
			if (Game1.player.totalMoneyEarned >= 1000000)
			{
				num += 2;
			}
			if (Game1.player.achievements.Contains(5))
			{
				num++;
			}
			if (Game1.player.hasSkullKey)
			{
				num++;
			}
			bool flag = Game1.isLocationAccessible("CommunityCenter");
			if (flag || Game1.player.hasCompletedCommunityCenter())
			{
				num++;
			}
			if (flag)
			{
				num += 2;
			}
			if (Game1.player.isMarried() && getHomeOfFarmer(Game1.player).upgradeLevel >= 2)
			{
				num++;
			}
			if (Game1.player.hasRustyKey)
			{
				num++;
			}
			if (Game1.player.achievements.Contains(26))
			{
				num++;
			}
			if (Game1.player.achievements.Contains(34))
			{
				num++;
			}
			int numberOfFriendsWithinThisRange = getNumberOfFriendsWithinThisRange(Game1.player, 1975, 999999);
			if (numberOfFriendsWithinThisRange >= 5)
			{
				num++;
			}
			if (numberOfFriendsWithinThisRange >= 10)
			{
				num++;
			}
			int level = Game1.player.Level;
			if (level >= 15)
			{
				num++;
			}
			if (level >= 25)
			{
				num++;
			}
			string petName = Game1.player.getPetName();
			if (petName != null)
			{
				Pet characterFromName = Game1.getCharacterFromName<Pet>(petName, mustBeVillager: false);
				if (characterFromName != null && (int)characterFromName.friendshipTowardFarmer >= 999)
				{
					num++;
				}
			}
			return num;
		}

		public static int getGrandpaCandlesFromScore(int score)
		{
			if (score >= 12)
			{
				return 4;
			}
			if (score >= 8)
			{
				return 3;
			}
			if (score >= 4)
			{
				return 2;
			}
			return 1;
		}

		public static bool canItemBeAddedToThisInventoryList(Item i, IList<Item> list, int listMaxSpace = -1)
		{
			if (listMaxSpace != -1 && list.Count < listMaxSpace)
			{
				return true;
			}
			int num = i.Stack;
			foreach (Item item in list)
			{
				if (item == null)
				{
					return true;
				}
				if (item.canStackWith(i) && item.getRemainingStackSpace() > 0)
				{
					num -= item.getRemainingStackSpace();
					if (num <= 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool ifPossibleAddItemToThisInventoryList(Item i, IList<Item> list, int capacity = -1)
		{
			for (int j = 0; j < ((capacity == -1) ? list.Count : capacity); j++)
			{
				if (list[j] == null)
				{
					list[j] = i;
					return true;
				}
			}
			return false;
		}

		public static Item addItemToThisInventoryList(Item i, IList<Item> list, int listMaxSpace = -1)
		{
			if (i.Stack == 0)
			{
				i.Stack = 1;
			}
			foreach (Item item in list)
			{
				if (item != null && item.canStackWith(i) && item.getRemainingStackSpace() > 0)
				{
					if (i is Object)
					{
						(i as Object).stack.Value = item.addToStack(i);
					}
					else
					{
						i.Stack = item.addToStack(i);
					}
					if (i.Stack <= 0)
					{
						return null;
					}
				}
			}
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (list[num] == null)
				{
					if (i.Stack <= i.maximumStackSize())
					{
						list[num] = i;
						return null;
					}
					list[num] = i.getOne();
					list[num].Stack = i.maximumStackSize();
					if (i is Object)
					{
						(i as Object).stack.Value -= i.maximumStackSize();
					}
					else
					{
						i.Stack -= i.maximumStackSize();
					}
				}
			}
			while (listMaxSpace != -1 && list.Count < listMaxSpace)
			{
				if (i.Stack > i.maximumStackSize())
				{
					Item one = i.getOne();
					one.Stack = i.maximumStackSize();
					if (i is Object)
					{
						(i as Object).stack.Value -= i.maximumStackSize();
					}
					else
					{
						i.Stack -= i.maximumStackSize();
					}
					list.Add(one);
					continue;
				}
				list.Add(i);
				return null;
			}
			return i;
		}

		public static Item addItemToInventory(Item item, int position, IList<Item> items, ItemGrabMenu.behaviorOnItemSelect onAddFunction = null, bool allowStack = true)
		{
			if (items.Equals(Game1.player.items) && item is Object && (item as Object).specialItem)
			{
				if ((bool)(item as Object).bigCraftable)
				{
					if (!Game1.player.specialBigCraftables.Contains((item as Object).isRecipe ? (-(int)(item as Object).parentSheetIndex) : ((int)(item as Object).parentSheetIndex)))
					{
						Game1.player.specialBigCraftables.Add((item as Object).isRecipe ? (-(int)(item as Object).parentSheetIndex) : ((int)(item as Object).parentSheetIndex));
					}
				}
				else if (!Game1.player.specialItems.Contains((item as Object).isRecipe ? (-(int)(item as Object).parentSheetIndex) : ((int)(item as Object).parentSheetIndex)))
				{
					Game1.player.specialItems.Add((item as Object).isRecipe ? (-(int)(item as Object).parentSheetIndex) : ((int)(item as Object).parentSheetIndex));
				}
			}
			if (position >= 0 && position < items.Count)
			{
				if (items[position] == null)
				{
					if (!allowStack)
					{
						if (item.Stack >= 1)
						{
							int stack = item.Stack;
							addItemToInventory(item.getOne(), position, items);
							item.Stack = stack - 1;
						}
						onAddFunction?.Invoke(item, null);
						if (item.Stack >= 1)
						{
							return item;
						}
						return null;
					}
					items[position] = item;
					checkItemFirstInventoryAdd(item);
					onAddFunction?.Invoke(item, null);
					return null;
				}
				if (allowStack && items[position].maximumStackSize() != -1 && items[position].Name.Equals(item.Name) && (!(item is Object) || !(items[position] is Object) || ((item as Object).quality == (items[position] as Object).quality && (item as Object).parentSheetIndex == (items[position] as Object).parentSheetIndex)) && item.canStackWith(items[position]))
				{
					checkItemFirstInventoryAdd(item);
					int num = items[position].addToStack(item);
					if (num <= 0)
					{
						return null;
					}
					item.Stack = num;
					onAddFunction?.Invoke(item, null);
					return item;
				}
				if (!allowStack)
				{
					Item item2 = items[position];
					if (position == Game1.player.CurrentToolIndex && items.Equals(Game1.player.items) && item2 != null)
					{
						item2.actionWhenStopBeingHeld(Game1.player);
						item.actionWhenBeingHeld(Game1.player);
					}
					if (item.Stack > 1)
					{
						checkItemFirstInventoryAdd(item);
						items[position] = item;
						items[position].Stack = 1;
						item.Stack--;
					}
					onAddFunction?.Invoke(item, null);
					return item2;
				}
				Item item3 = items[position];
				if (position == Game1.player.CurrentToolIndex && items.Equals(Game1.player.items) && item3 != null)
				{
					item3.actionWhenStopBeingHeld(Game1.player);
					item.actionWhenBeingHeld(Game1.player);
				}
				checkItemFirstInventoryAdd(item);
				items[position] = item;
				onAddFunction?.Invoke(item, null);
				return item3;
			}
			return item;
		}

		public static bool spawnObjectAround(Vector2 tileLocation, Object o, GameLocation l, bool playSound = true, Action<Object> modifyObject = null)
		{
			if (o == null || l == null || tileLocation.Equals(Vector2.Zero))
			{
				return false;
			}
			int i = 0;
			Queue<Vector2> queue = new Queue<Vector2>();
			HashSet<Vector2> hashSet = new HashSet<Vector2>();
			queue.Enqueue(tileLocation);
			Vector2 vector = Vector2.Zero;
			for (; i < 100; i++)
			{
				vector = queue.Dequeue();
				if (!l.isTileOccupiedForPlacement(vector) && !l.isOpenWater((int)vector.X, (int)vector.Y))
				{
					break;
				}
				hashSet.Add(vector);
				List<Vector2> list = (from a in getAdjacentTileLocations(vector)
					orderby GuidHelper.NewGuid()
					select a).ToList();
				foreach (Vector2 item in list)
				{
					if (!hashSet.Contains(item))
					{
						queue.Enqueue(item);
					}
				}
			}
			o.isSpawnedObject.Value = true;
			o.canBeGrabbed.Value = true;
			o.tileLocation.Value = vector;
			modifyObject?.Invoke(o);
			if (!vector.Equals(Vector2.Zero) && !l.isTileOccupiedForPlacement(vector) && !l.isOpenWater((int)vector.X, (int)vector.Y))
			{
				l.objects.Add(vector, o);
				if (playSound)
				{
					l.playSound("coin");
				}
				if (l.Equals(Game1.currentLocation))
				{
					l.temporarySprites.Add(new TemporaryAnimatedSprite(5, vector * 64f, Color.White));
				}
				return true;
			}
			return false;
		}

		public static bool IsGeode(Item item, bool disallow_special_geodes = false)
		{
			if (item == null)
			{
				return false;
			}
			if (!IsNormalObjectAtParentSheetIndex(item, item.ParentSheetIndex))
			{
				return false;
			}
			int num = (item as Object).parentSheetIndex;
			if (num == 275 || num == 791)
			{
				return !disallow_special_geodes;
			}
			try
			{
				if (Game1.objectInformation.ContainsKey(num))
				{
					string text = Game1.objectInformation[num];
					string[] array = text.Split('/');
					if (array.Length > 6)
					{
						string[] array2 = array[6].Split(' ');
						if (array2 == null || array2.Length == 0 || !int.TryParse(array2[0], out var _))
						{
							return false;
						}
						return true;
					}
				}
			}
			catch (Exception)
			{
			}
			return false;
		}

		public static Item getTreasureFromGeode(Item geode)
		{
			bool flag = IsGeode(geode);
			if (flag)
			{
				try
				{
					Random random = new Random((int)Game1.stats.GeodesCracked + (int)Game1.uniqueIDForThisGame / 2);
					int num = random.Next(1, 10);
					for (int i = 0; i < num; i++)
					{
						random.NextDouble();
					}
					num = random.Next(1, 10);
					for (int j = 0; j < num; j++)
					{
						random.NextDouble();
					}
					int num2 = (geode as Object).parentSheetIndex;
					if (random.NextDouble() <= 0.1 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
					{
						bool flag2 = random.NextDouble() < 0.25;
						return new Object(890, (!flag2) ? 1 : 5);
					}
					if (num2 == 791)
					{
						if (random.NextDouble() < 0.05 && !Game1.player.hasOrWillReceiveMail("goldenCoconutHat"))
						{
							Game1.player.mailReceived.Add("goldenCoconutHat");
							return new Hat(75);
						}
						switch (random.Next(7))
						{
						case 0:
							return new Object(69, 1);
						case 1:
							return new Object(835, 1);
						case 2:
							return new Object(833, 5);
						case 3:
							return new Object(831, 5);
						case 4:
							return new Object(820, 1);
						case 5:
							return new Object(292, 1);
						case 6:
							return new Object(386, 5);
						}
					}
					else
					{
						if (num2 == 275 || !(random.NextDouble() < 0.5))
						{
							string[] array = Game1.objectInformation[num2].Split('/')[6].Split(' ');
							int parentSheetIndex = Convert.ToInt32(array[random.Next(array.Length)]);
							if (num2 == 749 && random.NextDouble() < 0.008 && (int)Game1.stats.GeodesCracked > 15)
							{
								return new Object(74, 1);
							}
							return new Object(parentSheetIndex, 1);
						}
						int num3 = random.Next(3) * 2 + 1;
						if (random.NextDouble() < 0.1)
						{
							num3 = 10;
						}
						if (random.NextDouble() < 0.01)
						{
							num3 = 20;
						}
						if (random.NextDouble() < 0.5)
						{
							switch (random.Next(4))
							{
							case 0:
							case 1:
								return new Object(390, num3);
							case 2:
								return new Object(330, 1);
							case 3:
							{
								int parentSheetIndex2;
								switch (num2)
								{
								case 749:
									return new Object(82 + random.Next(3) * 2, 1);
								default:
									parentSheetIndex2 = 82;
									break;
								case 536:
									parentSheetIndex2 = 84;
									break;
								case 535:
									parentSheetIndex2 = 86;
									break;
								}
								return new Object(parentSheetIndex2, 1);
							}
							}
						}
						else
						{
							switch (num2)
							{
							case 535:
								switch (random.Next(3))
								{
								case 0:
									return new Object(378, num3);
								case 1:
									return new Object((Game1.player.deepestMineLevel > 25) ? 380 : 378, num3);
								case 2:
									return new Object(382, num3);
								}
								break;
							case 536:
								switch (random.Next(4))
								{
								case 0:
									return new Object(378, num3);
								case 1:
									return new Object(380, num3);
								case 2:
									return new Object(382, num3);
								case 3:
									return new Object((Game1.player.deepestMineLevel > 75) ? 384 : 380, num3);
								}
								break;
							default:
								switch (random.Next(5))
								{
								case 0:
									return new Object(378, num3);
								case 1:
									return new Object(380, num3);
								case 2:
									return new Object(382, num3);
								case 3:
									return new Object(384, num3);
								case 4:
									return new Object(386, num3 / 2 + 1);
								}
								break;
							}
						}
					}
					return new Object(Vector2.Zero, 390, 1);
				}
				catch (Exception)
				{
				}
			}
			if (flag)
			{
				return new Object(Vector2.Zero, 390, 1);
			}
			return null;
		}

		public static Vector2 snapToInt(Vector2 v)
		{
			v.X = (int)v.X;
			v.Y = (int)v.Y;
			return v;
		}

		public static Vector2 GetNearbyValidPlacementPosition(Farmer who, GameLocation location, Item item, int x, int y)
		{
			if (!Game1.isCheckingNonMousePlacement)
			{
				return new Vector2(x, y);
			}
			int num = 1;
			int num2 = 1;
			Point point = default(Point);
			Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(0, 0, num * 64, num2 * 64);
			if (item is Furniture)
			{
				Furniture furniture = item as Furniture;
				num = furniture.getTilesWide();
				num2 = furniture.getTilesHigh();
				value.Width = furniture.boundingBox.Value.Width;
				value.Height = furniture.boundingBox.Value.Height;
			}
			switch (who.FacingDirection)
			{
			case 0:
				point.X = 0;
				point.Y = -1;
				y -= (num2 - 1) * 64;
				break;
			case 2:
				point.X = 0;
				point.Y = 1;
				break;
			case 3:
				point.X = -1;
				point.Y = 0;
				x -= (num - 1) * 64;
				break;
			case 1:
				point.X = 1;
				point.Y = 0;
				break;
			}
			int num3 = 2;
			if (item is Object && (item as Object).isPassable() && ((item as Object).Category == -74 || (item as Object).isSapling() || (int)(item as Object).category == -19))
			{
				x = (int)who.GetToolLocation().X / 64 * 64;
				y = (int)who.GetToolLocation().Y / 64 * 64;
				point.X = who.getTileX() - x / 64;
				point.Y = who.getTileY() - y / 64;
				int num4 = (int)Math.Sqrt(Math.Pow(point.X, 2.0) + Math.Pow(point.Y, 2.0));
				if (num4 > 0)
				{
					point.X /= num4;
					point.Y /= num4;
				}
				num3 = num4 + 1;
			}
			bool flag = item is Object && (item as Object).isPassable();
			x = x / 64 * 64;
			y = y / 64 * 64;
			for (int i = 0; i < num3; i++)
			{
				int num5 = x + point.X * i * 64;
				int num6 = y + point.Y * i * 64;
				value.X = num5;
				value.Y = num6;
				if ((!who.GetBoundingBox().Intersects(value) && !flag) || playerCanPlaceItemHere(location, item, num5, num6, who))
				{
					return new Vector2(num5, num6);
				}
			}
			return new Vector2(x, y);
		}

		public static bool tryToPlaceItem(GameLocation location, Item item, int x, int y)
		{
			if (item == null)
			{
				return false;
			}
			if (item is Tool)
			{
				return false;
			}
			Vector2 key = new Vector2(x / 64, y / 64);
			if (playerCanPlaceItemHere(location, item, x, y, Game1.player))
			{
				if (item is Furniture)
				{
					Game1.player.ActiveObject = null;
				}
				if (((Object)item).placementAction(location, x, y, Game1.player))
				{
					Game1.player.reduceActiveItemByOne();
					if (Game1.player.ActiveObject != null && (Game1.player.ActiveObject.ParentSheetIndex == 286 || Game1.player.ActiveObject.ParentSheetIndex == 287 || Game1.player.ActiveObject.ParentSheetIndex == 288))
					{
						Game1.player.CurrentToolIndex = -1;
					}
				}
				else if (item is Furniture)
				{
					Game1.player.ActiveObject = item as Furniture;
				}
				else if (item is Wallpaper)
				{
					return false;
				}
				return true;
			}
			if (isPlacementForbiddenHere(location) && item != null && item.isPlaceable())
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
			}
			else if (item is Furniture)
			{
				switch ((item as Furniture).GetAdditionalFurniturePlacementStatus(location, x, y, Game1.player))
				{
				case 1:
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12629"));
					break;
				case 2:
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12632"));
					break;
				case 3:
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12633"));
					break;
				case 4:
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12632"));
					break;
				}
			}
			if (item.Category == -19 && location.terrainFeatures.ContainsKey(key) && location.terrainFeatures[key] is HoeDirt)
			{
				HoeDirt hoeDirt = location.terrainFeatures[key] as HoeDirt;
				if ((int)(location.terrainFeatures[key] as HoeDirt).fertilizer != 0)
				{
					if ((location.terrainFeatures[key] as HoeDirt).fertilizer != item.parentSheetIndex)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13916-2"));
					}
					return false;
				}
				if (((int)item.parentSheetIndex == 368 || (int)item.parentSheetIndex == 368) && hoeDirt.crop != null && (int)hoeDirt.crop.currentPhase != 0)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13916"));
					return false;
				}
			}
			return false;
		}

		public static int showLanternBar()
		{
			foreach (Item item in Game1.player.Items)
			{
				if (item != null && item is Lantern && ((Lantern)item).on)
				{
					return ((Lantern)item).fuelLeft;
				}
			}
			return -1;
		}

		public static void plantCrops(GameLocation farm, int seedType, int x, int y, int width, int height, int daysOld)
		{
			for (int i = x; i < x + width; i++)
			{
				for (int j = y; j < y + height; j++)
				{
					Vector2 vector = new Vector2(i, j);
					farm.makeHoeDirt(vector);
					if (farm.terrainFeatures.ContainsKey(vector) && farm.terrainFeatures[vector] is HoeDirt)
					{
						((HoeDirt)farm.terrainFeatures[vector]).crop = new Crop(seedType, x, y);
					}
				}
			}
		}

		public static bool pointInRectangles(List<Microsoft.Xna.Framework.Rectangle> rectangles, int x, int y)
		{
			foreach (Microsoft.Xna.Framework.Rectangle rectangle in rectangles)
			{
				if (rectangle.Contains(x, y))
				{
					return true;
				}
			}
			return false;
		}

		public static Keys mapGamePadButtonToKey(Buttons b)
		{
			return b switch
			{
				Buttons.A => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.actionButton), 
				Buttons.X => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.useToolButton), 
				Buttons.B => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.menuButton), 
				Buttons.Back => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.journalButton), 
				Buttons.Start => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.journalButton), 
				Buttons.Y => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.journalButton), 
				Buttons.DPadUp => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveUpButton), 
				Buttons.DPadRight => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveRightButton), 
				Buttons.DPadDown => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveDownButton), 
				Buttons.DPadLeft => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveLeftButton), 
				Buttons.LeftThumbstickUp => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveUpButton), 
				Buttons.LeftThumbstickRight => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveRightButton), 
				Buttons.LeftThumbstickDown => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveDownButton), 
				Buttons.LeftThumbstickLeft => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveLeftButton), 
				_ => Keys.None, 
			};
		}

		public static ButtonCollection getPressedButtons(GamePadState padState, GamePadState oldPadState)
		{
			return new ButtonCollection(ref padState, ref oldPadState);
		}

		public static ButtonCollection getReleasedButtons(GamePadState padState, GamePadState oldPadState)
		{
			return new ButtonCollection(ref padState, ref oldPadState, released: true);
		}

		public static bool thumbstickIsInDirection(int direction, GamePadState padState)
		{
			if (Game1.currentMinigame != null)
			{
				return true;
			}
			if (direction == 0 && Math.Abs(padState.ThumbSticks.Left.X) < padState.ThumbSticks.Left.Y)
			{
				return true;
			}
			if (direction == 1 && padState.ThumbSticks.Left.X > Math.Abs(padState.ThumbSticks.Left.Y))
			{
				return true;
			}
			if (direction == 2 && Math.Abs(padState.ThumbSticks.Left.X) < Math.Abs(padState.ThumbSticks.Left.Y))
			{
				return true;
			}
			if (direction == 3 && Math.Abs(padState.ThumbSticks.Left.X) > Math.Abs(padState.ThumbSticks.Left.Y))
			{
				return true;
			}
			return false;
		}

		public static ButtonCollection getHeldButtons(GamePadState padState)
		{
			return new ButtonCollection(ref padState);
		}

		public static bool toggleMuteMusic()
		{
			if (Game1.soundBank != null)
			{
				if (Game1.options.musicVolumeLevel != 0f)
				{
					return true;
				}
				enableMusic();
			}
			return false;
		}

		public static void enableMusic()
		{
			if (Game1.soundBank != null)
			{
				Game1.options.musicVolumeLevel = 0.75f;
				if (Game1.musicCategory != null)
				{
					Game1.musicCategory.SetVolume(0.75f);
				}
				Game1.musicPlayerVolume = 0.75f;
				Game1.options.ambientVolumeLevel = 0.75f;
				Game1.ambientCategory.SetVolume(0.75f);
				Game1.ambientPlayerVolume = 0.75f;
			}
		}

		public static void disableMusic()
		{
			if (Game1.soundBank != null)
			{
				Game1.options.musicVolumeLevel = 0f;
				if (Game1.musicCategory != null)
				{
					Game1.musicCategory.SetVolume(0f);
				}
				Game1.options.ambientVolumeLevel = 0f;
				Game1.ambientCategory.SetVolume(0f);
				Game1.ambientPlayerVolume = 0f;
				Game1.musicPlayerVolume = 0f;
			}
		}

		public static Vector2 getVelocityTowardPlayer(Point startingPoint, float speed, Farmer f)
		{
			return getVelocityTowardPoint(startingPoint, new Vector2(f.GetBoundingBox().X, f.GetBoundingBox().Y), speed);
		}

		public static string getHoursMinutesStringFromMilliseconds(ulong milliseconds)
		{
			return milliseconds / 3600000uL + ":" + ((milliseconds % 3600000uL / 60000uL < 10) ? "0" : "") + milliseconds % 3600000uL / 60000uL;
		}

		public static string getMinutesSecondsStringFromMilliseconds(int milliseconds)
		{
			return milliseconds / 60000 + ":" + ((milliseconds % 60000 / 1000 < 10) ? "0" : "") + milliseconds % 60000 / 1000;
		}

		public static Vector2 getVelocityTowardPoint(Vector2 startingPoint, Vector2 endingPoint, float speed)
		{
			double num = endingPoint.X - startingPoint.X;
			double num2 = endingPoint.Y - startingPoint.Y;
			if (Math.Abs(num) < 0.1 && Math.Abs(num2) < 0.1)
			{
				return new Vector2(0f, 0f);
			}
			double num3 = Math.Sqrt(Math.Pow(num, 2.0) + Math.Pow(num2, 2.0));
			num /= num3;
			num2 /= num3;
			return new Vector2((float)(num * (double)speed), (float)(num2 * (double)speed));
		}

		public static Vector2 getVelocityTowardPoint(Point startingPoint, Vector2 endingPoint, float speed)
		{
			return getVelocityTowardPoint(new Vector2(startingPoint.X, startingPoint.Y), endingPoint, speed);
		}

		public static Vector2 getRandomPositionInThisRectangle(Microsoft.Xna.Framework.Rectangle r, Random random)
		{
			return new Vector2(random.Next(r.X, r.X + r.Width), random.Next(r.Y, r.Y + r.Height));
		}

		public static Vector2 getTopLeftPositionForCenteringOnScreen(xTile.Dimensions.Rectangle viewport, int width, int height, int xOffset = 0, int yOffset = 0)
		{
			return new Vector2(viewport.Width / 2 - width / 2 + xOffset, viewport.Height / 2 - height / 2 + yOffset);
		}

		public static Vector2 getTopLeftPositionForCenteringOnScreen(int width, int height, int xOffset = 0, int yOffset = 0)
		{
			return getTopLeftPositionForCenteringOnScreen(Game1.uiViewport, width, height, xOffset, yOffset);
		}

		public static void recursiveFindPositionForCharacter(NPC c, GameLocation l, Vector2 tileLocation, int maxIterations)
		{
			int i = 0;
			Queue<Vector2> queue = new Queue<Vector2>();
			queue.Enqueue(tileLocation);
			List<Vector2> list = new List<Vector2>();
			for (; i < maxIterations; i++)
			{
				if (queue.Count <= 0)
				{
					break;
				}
				Vector2 vector = queue.Dequeue();
				list.Add(vector);
				c.Position = new Vector2(vector.X * 64f + 32f - (float)(c.GetBoundingBox().Width / 2), vector.Y * 64f - (float)c.GetBoundingBox().Height);
				if (!l.isCollidingPosition(c.GetBoundingBox(), Game1.viewport, isFarmer: false, 0, glider: false, c, pathfinding: true))
				{
					if (!l.characters.Contains(c))
					{
						l.characters.Add(c);
						c.currentLocation = l;
					}
					break;
				}
				Vector2[] directionsTileVectors = DirectionsTileVectors;
				foreach (Vector2 vector2 in directionsTileVectors)
				{
					if (!list.Contains(vector + vector2))
					{
						queue.Enqueue(vector + vector2);
					}
				}
			}
		}

		public static Vector2 recursiveFindOpenTileForCharacter(Character c, GameLocation l, Vector2 tileLocation, int maxIterations, bool allowOffMap = true)
		{
			int i = 0;
			Queue<Vector2> queue = new Queue<Vector2>();
			queue.Enqueue(tileLocation);
			List<Vector2> list = new List<Vector2>();
			Vector2 position = c.Position;
			for (; i < maxIterations; i++)
			{
				if (queue.Count <= 0)
				{
					break;
				}
				Vector2 vector = queue.Dequeue();
				list.Add(vector);
				c.Position = new Vector2(vector.X * 64f + 32f - (float)(c.GetBoundingBox().Width / 2), vector.Y * 64f + 4f);
				if (!l.isCollidingPosition(c.GetBoundingBox(), Game1.viewport, c is Farmer, 0, glider: false, c, pathfinding: true) && (allowOffMap || l.isTileOnMap(vector)))
				{
					c.Position = position;
					return vector;
				}
				Vector2[] directionsTileVectors = DirectionsTileVectors;
				foreach (Vector2 vector2 in directionsTileVectors)
				{
					if (!list.Contains(vector + vector2))
					{
						queue.Enqueue(vector + vector2);
					}
				}
			}
			c.Position = position;
			return Vector2.Zero;
		}

		public static List<Vector2> recursiveFindOpenTiles(GameLocation l, Vector2 tileLocation, int maxOpenTilesToFind = 24, int maxIterations = 50)
		{
			int i = 0;
			Queue<Vector2> queue = new Queue<Vector2>();
			queue.Enqueue(tileLocation);
			List<Vector2> list = new List<Vector2>();
			List<Vector2> list2 = new List<Vector2>();
			for (; i < maxIterations; i++)
			{
				if (queue.Count <= 0)
				{
					break;
				}
				if (list2.Count >= maxOpenTilesToFind)
				{
					break;
				}
				Vector2 vector = queue.Dequeue();
				list.Add(vector);
				if (l.isTileLocationTotallyClearAndPlaceable(vector))
				{
					list2.Add(vector);
				}
				Vector2[] directionsTileVectors = DirectionsTileVectors;
				foreach (Vector2 vector2 in directionsTileVectors)
				{
					if (!list.Contains(vector + vector2))
					{
						queue.Enqueue(vector + vector2);
					}
				}
			}
			return list2;
		}

		public static void spreadAnimalsAround(Building b, Farm environment)
		{
			try
			{
			}
			catch (Exception)
			{
			}
		}

		public static void spreadAnimalsAround(Building b, Farm environment, List<FarmAnimal> animalsList)
		{
			if (b.indoors.Value == null || !(b.indoors.Value is AnimalHouse))
			{
				return;
			}
			Queue<FarmAnimal> queue = new Queue<FarmAnimal>(animalsList);
			int num = 0;
			Queue<Vector2> queue2 = new Queue<Vector2>();
			queue2.Enqueue(new Vector2((int)b.tileX + b.animalDoor.X, (int)b.tileY + b.animalDoor.Y + 1));
			while (queue.Count > 0 && num < 40 && queue2.Count > 0)
			{
				Vector2 vector = queue2.Dequeue();
				queue.Peek().Position = new Vector2(vector.X * 64f + 32f - (float)(queue.Peek().GetBoundingBox().Width / 2), vector.Y * 64f - 32f - (float)(queue.Peek().GetBoundingBox().Height / 2));
				if (!environment.isCollidingPosition(queue.Peek().GetBoundingBox(), Game1.viewport, isFarmer: false, 0, glider: false, queue.Peek(), pathfinding: true))
				{
					FarmAnimal farmAnimal = queue.Dequeue();
					environment.animals.Add(farmAnimal.myID, farmAnimal);
				}
				if (queue.Count > 0)
				{
					Vector2[] directionsTileVectors = DirectionsTileVectors;
					for (int i = 0; i < directionsTileVectors.Length; i++)
					{
						Vector2 vector2 = directionsTileVectors[i];
						queue.Peek().Position = new Vector2((vector.X + vector2.X) * 64f + 32f - (float)(queue.Peek().GetBoundingBox().Width / 2), (vector.Y + vector2.Y) * 64f - 32f - (float)(queue.Peek().GetBoundingBox().Height / 2));
						if (!environment.isCollidingPosition(queue.Peek().GetBoundingBox(), Game1.viewport, isFarmer: false, 0, glider: false, queue.Peek(), pathfinding: true))
						{
							queue2.Enqueue(vector + vector2);
						}
					}
				}
				num++;
			}
		}

		public static bool[] horizontalOrVerticalCollisionDirections(Microsoft.Xna.Framework.Rectangle boundingBox, bool projectile = false)
		{
			return horizontalOrVerticalCollisionDirections(boundingBox, null, projectile);
		}

		public static Point findTile(GameLocation location, int tileIndex, string layer)
		{
			for (int i = 0; i < location.map.GetLayer(layer).LayerHeight; i++)
			{
				for (int j = 0; j < location.map.GetLayer(layer).LayerWidth; j++)
				{
					if (location.getTileIndexAt(j, i, layer) == tileIndex)
					{
						return new Point(j, i);
					}
				}
			}
			return new Point(-1, -1);
		}

		public static bool[] horizontalOrVerticalCollisionDirections(Microsoft.Xna.Framework.Rectangle boundingBox, Character c, bool projectile = false)
		{
			bool[] array = new bool[2];
			Microsoft.Xna.Framework.Rectangle position = new Microsoft.Xna.Framework.Rectangle(boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height);
			position.Width = 1;
			position.X = boundingBox.Center.X;
			if (c != null)
			{
				if (Game1.currentLocation.isCollidingPosition(position, Game1.viewport, isFarmer: false, -1, projectile, c, pathfinding: false, projectile))
				{
					array[1] = true;
				}
			}
			else if (Game1.currentLocation.isCollidingPosition(position, Game1.viewport, isFarmer: false, -1, projectile, c, pathfinding: false, projectile))
			{
				array[1] = true;
			}
			position.Width = boundingBox.Width;
			position.X = boundingBox.X;
			position.Height = 1;
			position.Y = boundingBox.Center.Y;
			if (c != null)
			{
				if (Game1.currentLocation.isCollidingPosition(position, Game1.viewport, isFarmer: false, -1, projectile, c, pathfinding: false, projectile))
				{
					array[0] = true;
				}
			}
			else if (Game1.currentLocation.isCollidingPosition(position, Game1.viewport, isFarmer: false, -1, projectile, c, pathfinding: false, projectile))
			{
				array[0] = true;
			}
			return array;
		}

		public static Color getBlendedColor(Color c1, Color c2)
		{
			return new Color((Game1.random.NextDouble() < 0.5) ? Math.Max(c1.R, c2.R) : ((c1.R + c2.R) / 2), (Game1.random.NextDouble() < 0.5) ? Math.Max(c1.G, c2.G) : ((c1.G + c2.G) / 2), (Game1.random.NextDouble() < 0.5) ? Math.Max(c1.B, c2.B) : ((c1.B + c2.B) / 2));
		}

		public static Character checkForCharacterWithinArea(Type kindOfCharacter, Vector2 positionToAvoid, GameLocation location, Microsoft.Xna.Framework.Rectangle area)
		{
			foreach (NPC character in location.characters)
			{
				if (character.GetType().Equals(kindOfCharacter) && character.GetBoundingBox().Intersects(area) && !character.Position.Equals(positionToAvoid))
				{
					return character;
				}
			}
			return null;
		}

		public static int getNumberOfCharactersInRadius(GameLocation l, Point position, int tileRadius)
		{
			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(position.X - tileRadius * 64, position.Y - tileRadius * 64, (tileRadius * 2 + 1) * 64, (tileRadius * 2 + 1) * 64);
			int num = 0;
			foreach (NPC character in l.characters)
			{
				if (rectangle.Contains(Vector2ToPoint(character.Position)))
				{
					num++;
				}
			}
			return num;
		}

		public static List<Vector2> getListOfTileLocationsForBordersOfNonTileRectangle(Microsoft.Xna.Framework.Rectangle rectangle)
		{
			List<Vector2> list = new List<Vector2>();
			list.Add(new Vector2(rectangle.Left / 64, rectangle.Top / 64));
			list.Add(new Vector2(rectangle.Right / 64, rectangle.Top / 64));
			list.Add(new Vector2(rectangle.Left / 64, rectangle.Bottom / 64));
			list.Add(new Vector2(rectangle.Right / 64, rectangle.Bottom / 64));
			list.Add(new Vector2(rectangle.Left / 64, rectangle.Center.Y / 64));
			list.Add(new Vector2(rectangle.Right / 64, rectangle.Center.Y / 64));
			list.Add(new Vector2(rectangle.Center.X / 64, rectangle.Bottom / 64));
			list.Add(new Vector2(rectangle.Center.X / 64, rectangle.Top / 64));
			list.Add(new Vector2(rectangle.Center.X / 64, rectangle.Center.Y / 64));
			return list;
		}

		public static void makeTemporarySpriteJuicier(TemporaryAnimatedSprite t, GameLocation l, int numAddOns = 4, int xRange = 64, int yRange = 64)
		{
			t.position.Y -= 8f;
			l.temporarySprites.Add(t);
			for (int i = 0; i < numAddOns; i++)
			{
				TemporaryAnimatedSprite clone = t.getClone();
				clone.delayBeforeAnimationStart = i * 100;
				clone.position += new Vector2(Game1.random.Next(-xRange / 2, xRange / 2 + 1), Game1.random.Next(-yRange / 2, yRange / 2 + 1));
				l.temporarySprites.Add(clone);
			}
		}

		public static void recursiveObjectPlacement(Object o, int tileX, int tileY, double growthRate, double decay, GameLocation location, string terrainToExclude = "", int objectIndexAddRange = 0, double failChance = 0.0, int objectIndeAddRangeMultiplier = 1)
		{
			if (!location.isTileLocationOpen(new Location(tileX, tileY)) || location.isTileOccupied(new Vector2(tileX, tileY)) || location.getTileIndexAt(tileX, tileY, "Back") == -1 || (!terrainToExclude.Equals("") && (location.doesTileHaveProperty(tileX, tileY, "Type", "Back") == null || location.doesTileHaveProperty(tileX, tileY, "Type", "Back").Equals(terrainToExclude))))
			{
				return;
			}
			Vector2 vector = new Vector2(tileX, tileY);
			if (Game1.random.NextDouble() > failChance * 2.0)
			{
				if (o is ColoredObject)
				{
					location.objects.Add(vector, new ColoredObject((int)o.parentSheetIndex + Game1.random.Next(objectIndexAddRange + 1) * objectIndeAddRangeMultiplier, 1, (o as ColoredObject).color)
					{
						Fragility = o.fragility,
						MinutesUntilReady = o.minutesUntilReady,
						Name = o.name,
						CanBeSetDown = o.CanBeSetDown,
						CanBeGrabbed = o.CanBeGrabbed,
						IsSpawnedObject = o.IsSpawnedObject,
						TileLocation = vector,
						ColorSameIndexAsParentSheetIndex = (o as ColoredObject).ColorSameIndexAsParentSheetIndex
					});
				}
				else
				{
					location.objects.Add(vector, new Object(vector, (int)o.parentSheetIndex + Game1.random.Next(objectIndexAddRange + 1) * objectIndeAddRangeMultiplier, o.name, o.canBeSetDown, o.canBeGrabbed, o.isHoedirt, o.isSpawnedObject)
					{
						Fragility = o.fragility,
						MinutesUntilReady = o.minutesUntilReady
					});
				}
			}
			growthRate -= decay;
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveObjectPlacement(o, tileX + 1, tileY, growthRate, decay, location, terrainToExclude, objectIndexAddRange, failChance, objectIndeAddRangeMultiplier);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveObjectPlacement(o, tileX - 1, tileY, growthRate, decay, location, terrainToExclude, objectIndexAddRange, failChance, objectIndeAddRangeMultiplier);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveObjectPlacement(o, tileX, tileY + 1, growthRate, decay, location, terrainToExclude, objectIndexAddRange, failChance, objectIndeAddRangeMultiplier);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveObjectPlacement(o, tileX, tileY - 1, growthRate, decay, location, terrainToExclude, objectIndexAddRange, failChance, objectIndeAddRangeMultiplier);
			}
		}

		public static void recursiveFarmGrassPlacement(int tileX, int tileY, double growthRate, double decay, GameLocation farm)
		{
			if (farm.isTileLocationOpen(new Location(tileX, tileY)) && !farm.isTileOccupied(new Vector2(tileX, tileY)) && farm.doesTileHaveProperty(tileX, tileY, "Diggable", "Back") != null)
			{
				Vector2 key = new Vector2(tileX, tileY);
				if (Game1.random.NextDouble() < 0.05)
				{
					farm.objects.Add(new Vector2(tileX, tileY), new Object(new Vector2(tileX, tileY), (Game1.random.NextDouble() < 0.5) ? 674 : 675, 1));
				}
				else
				{
					farm.terrainFeatures.Add(key, new Grass(1, 4 - (int)((1.0 - growthRate) * 4.0)));
				}
				growthRate -= decay;
				if (Game1.random.NextDouble() < growthRate)
				{
					recursiveFarmGrassPlacement(tileX + 1, tileY, growthRate, decay, farm);
				}
				if (Game1.random.NextDouble() < growthRate)
				{
					recursiveFarmGrassPlacement(tileX - 1, tileY, growthRate, decay, farm);
				}
				if (Game1.random.NextDouble() < growthRate)
				{
					recursiveFarmGrassPlacement(tileX, tileY + 1, growthRate, decay, farm);
				}
				if (Game1.random.NextDouble() < growthRate)
				{
					recursiveFarmGrassPlacement(tileX, tileY - 1, growthRate, decay, farm);
				}
			}
		}

		public static void recursiveTreePlacement(int tileX, int tileY, double growthRate, int growthStage, double skipChance, GameLocation l, Microsoft.Xna.Framework.Rectangle clearPatch, bool sparse)
		{
			if (clearPatch.Contains(tileX, tileY))
			{
				return;
			}
			Vector2 vector = new Vector2(tileX, tileY);
			if (l.doesTileHaveProperty((int)vector.X, (int)vector.Y, "Diggable", "Back") == null || l.doesTileHaveProperty((int)vector.X, (int)vector.Y, "NoSpawn", "Back") != null || !l.isTileLocationOpen(new Location((int)vector.X, (int)vector.Y)) || l.isTileOccupied(vector) || (sparse && (l.isTileOccupied(new Vector2(tileX, tileY + -1)) || l.isTileOccupied(new Vector2(tileX, tileY + 1)) || l.isTileOccupied(new Vector2(tileX + 1, tileY)) || l.isTileOccupied(new Vector2(tileX + -1, tileY)) || l.isTileOccupied(new Vector2(tileX + 1, tileY + 1)))))
			{
				return;
			}
			if (Game1.random.NextDouble() > skipChance)
			{
				if (sparse && vector.X < 70f && (vector.X < 48f || vector.Y > 26f) && Game1.random.NextDouble() < 0.07)
				{
					(l as Farm).resourceClumps.Add(new ResourceClump((Game1.random.NextDouble() < 0.5) ? 672 : ((Game1.random.NextDouble() < 0.5) ? 600 : 602), 2, 2, vector));
				}
				else
				{
					l.terrainFeatures.Add(vector, new Tree(Game1.random.Next(1, 4), (growthStage < 5) ? Game1.random.Next(5) : 5));
				}
				growthRate -= 0.05;
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveTreePlacement(tileX + Game1.random.Next(1, 3), tileY, growthRate, growthStage, skipChance, l, clearPatch, sparse);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveTreePlacement(tileX - Game1.random.Next(1, 3), tileY, growthRate, growthStage, skipChance, l, clearPatch, sparse);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveTreePlacement(tileX, tileY + Game1.random.Next(1, 3), growthRate, growthStage, skipChance, l, clearPatch, sparse);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveTreePlacement(tileX, tileY - Game1.random.Next(1, 3), growthRate, growthStage, skipChance, l, clearPatch, sparse);
			}
		}

		public static void recursiveRemoveTerrainFeatures(int tileX, int tileY, double growthRate, double decay, GameLocation l)
		{
			Vector2 key = new Vector2(tileX, tileY);
			l.terrainFeatures.Remove(key);
			growthRate -= decay;
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveRemoveTerrainFeatures(tileX + 1, tileY, growthRate, decay, l);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveRemoveTerrainFeatures(tileX - 1, tileY, growthRate, decay, l);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveRemoveTerrainFeatures(tileX, tileY + 1, growthRate, decay, l);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveRemoveTerrainFeatures(tileX, tileY - 1, growthRate, decay, l);
			}
		}

		public static IEnumerator<int> generateNewFarm(bool skipFarmGeneration)
		{
			return generateNewFarm(skipFarmGeneration, loadForNewGame: true);
		}

		public static IEnumerator<int> generateNewFarm(bool skipFarmGeneration, bool loadForNewGame)
		{
			Game1.fadeToBlack = false;
			Game1.fadeToBlackAlpha = 1f;
			Game1.ClearDebrisWeather(Game1.debrisWeather);
			Game1.viewport.X = -9999;
			Game1.changeMusicTrack("none");
			if (loadForNewGame)
			{
				Game1.loadForNewGame();
			}
			Game1.currentLocation = Game1.getLocationFromName("Farmhouse");
			Game1.currentLocation.currentEvent = new Event("none/-600 -600/farmer 4 8 2/warp farmer 4 8/end beginGame");
			Game1.gameMode = 2;
			yield return 100;
		}

		public static bool isObjectOnScreen(Object o)
		{
			Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height);
			return new Microsoft.Xna.Framework.Rectangle(o.boundingBox.Value.X, o.boundingBox.Value.Y, o.boundingBox.Value.Width, o.boundingBox.Value.Height).Intersects(value);
		}

		public static bool isOnScreen(Vector2 positionNonTile, int acceptableDistanceFromScreen)
		{
			positionNonTile.X -= Game1.viewport.X;
			positionNonTile.Y -= Game1.viewport.Y;
			if (positionNonTile.X > (float)(-acceptableDistanceFromScreen) && positionNonTile.X < (float)(Game1.viewport.Width + acceptableDistanceFromScreen) && positionNonTile.Y > (float)(-acceptableDistanceFromScreen))
			{
				return positionNonTile.Y < (float)(Game1.viewport.Height + acceptableDistanceFromScreen);
			}
			return false;
		}

		public static bool isOnScreen(Point positionTile, int acceptableDistanceFromScreenNonTile, GameLocation location = null)
		{
			if (location != null && !location.Equals(Game1.currentLocation))
			{
				return false;
			}
			if (positionTile.X * 64 > Game1.viewport.X - acceptableDistanceFromScreenNonTile && positionTile.X * 64 < Game1.viewport.X + Game1.viewport.Width + acceptableDistanceFromScreenNonTile && positionTile.Y * 64 > Game1.viewport.Y - acceptableDistanceFromScreenNonTile)
			{
				return positionTile.Y * 64 < Game1.viewport.Y + Game1.viewport.Height + acceptableDistanceFromScreenNonTile;
			}
			return false;
		}

		public static void createPotteryTreasure(int tileX, int tileY)
		{
		}

		public static void clearObjectsInArea(Microsoft.Xna.Framework.Rectangle r, GameLocation l)
		{
			for (int i = r.Left; i < r.Right; i += 64)
			{
				for (int j = r.Top; j < r.Bottom; j += 64)
				{
					l.removeEverythingFromThisTile(i / 64, j / 64);
				}
			}
		}

		public static void trashItem(Item item, int stack = -1)
		{
			if (item is Object && Game1.player.specialItems.Contains((item as Object).parentSheetIndex))
			{
				Game1.player.specialItems.Remove((item as Object).parentSheetIndex);
			}
			if (getTrashReclamationPrice(item, Game1.player) > 0)
			{
				Game1.player.Money += getTrashReclamationPrice(item, Game1.player, stack);
			}
			Game1.playSound("trashcan");
		}

		public static FarmAnimal GetBestHarvestableFarmAnimal(IEnumerable<FarmAnimal> animals, Tool tool, Microsoft.Xna.Framework.Rectangle toolRect)
		{
			FarmAnimal result = null;
			foreach (FarmAnimal animal in animals)
			{
				if (animal.GetHarvestBoundingBox().Intersects(toolRect))
				{
					if (animal.toolUsedForHarvest.Equals(tool.BaseName) && (int)animal.currentProduce > 0 && (int)animal.age >= (byte)animal.ageWhenMature)
					{
						return animal;
					}
					result = animal;
				}
			}
			return result;
		}

		public static void recolorDialogueAndMenu(string theme)
		{
			Color color = Color.White;
			Color white = Color.White;
			Color color2 = Color.White;
			Color color3 = Color.White;
			Color white2 = Color.White;
			Color color4 = Color.White;
			Color color5 = Color.White;
			Color color6 = Color.White;
			Color color7 = Color.White;
			if (theme != null)
			{
				switch (theme.Length)
				{
				case 6:
					switch (theme[0])
					{
					case 'E':
						if (theme == "Earthy")
						{
							color = new Color(44, 35, 0);
							white = new Color(115, 147, 102);
							color2 = new Color(91, 65, 0);
							color3 = new Color(122, 83, 0);
							white2 = new Color(179, 181, 125);
							color4 = new Color(144, 96, 0);
							color5 = new Color(234, 227, 190);
							color6 = new Color(255, 255, 227);
							color7 = new Color(193, 187, 156);
						}
						break;
					case 'B':
						if (theme == "Biomes")
						{
							color = new Color(17, 36, 0);
							white = new Color(color.R + 60, color.G + 60, color.B + 60);
							color2 = new Color(white.R + 30, white.G + 30, white.B + 30);
							color3 = new Color(color2.R + 30, color2.G + 30, color2.B + 30);
							white2 = new Color(color3.R + 15, color3.G + 15, color3.B + 15);
							color4 = new Color(white2.R + 15, white2.G + 15, white2.B + 15);
							color5 = new Color(192, 255, 183);
							color6 = new Color(Math.Min(255, color5.R + 30), Math.Min(255, color5.G + 30), Math.Min(255, color5.B + 30));
							color7 = new Color(color5.R - 30, color5.G - 30, color5.B - 30);
						}
						break;
					case 'S':
						if (theme == "Sports")
						{
							color = new Color(110, 45, 0);
							white = new Color(color.R + 60, color.G + 60, color.B + 60);
							color2 = new Color(white.R + 30, white.G + 30, white.B + 30);
							color3 = new Color(color2.R + 30, color2.G + 30, color2.B + 30);
							white2 = new Color(color3.R + 15, color3.G + 15, color3.B + 15);
							color4 = new Color(white2.R + 15, white2.G + 15, white2.B + 15);
							color5 = new Color(255, 214, 168);
							color6 = new Color(Math.Min(255, color5.R + 30), Math.Min(255, color5.G + 30), Math.Min(255, color5.B + 30));
							color7 = new Color(color5.R - 30, color5.G - 30, color5.B - 30);
						}
						break;
					}
					break;
				case 8:
					switch (theme[1])
					{
					case 'k':
						if (theme == "Skyscape")
						{
							color = new Color(15, 31, 57);
							white = new Color(color.R + 60, color.G + 60, color.B + 60);
							color2 = new Color(white.R + 30, white.G + 30, white.B + 30);
							color3 = new Color(color2.R + 30, color2.G + 30, color2.B + 30);
							white2 = new Color(color3.R + 15, color3.G + 15, color3.B + 15);
							color4 = new Color(white2.R + 15, white2.G + 15, white2.B + 15);
							color5 = new Color(206, 237, 254);
							color6 = new Color(Math.Min(255, color5.R + 30), Math.Min(255, color5.G + 30), Math.Min(255, color5.B + 30));
							color7 = new Color(color5.R - 30, color5.G - 30, color5.B - 30);
						}
						break;
					case 'w':
						if (theme == "Sweeties")
						{
							color = new Color(120, 60, 60);
							white = new Color(color.R + 60, color.G + 60, color.B + 60);
							color2 = new Color(white.R + 30, white.G + 30, white.B + 30);
							color3 = new Color(color2.R + 30, color2.G + 30, color2.B + 30);
							white2 = new Color(color3.R + 15, color3.G + 15, color3.B + 15);
							color4 = new Color(white2.R + 15, white2.G + 15, white2.B + 15);
							color5 = new Color(255, 213, 227);
							color6 = new Color(Math.Min(255, color5.R + 30), Math.Min(255, color5.G + 30), Math.Min(255, color5.B + 30));
							color7 = new Color(color5.R - 30, color5.G - 30, color5.B - 30);
						}
						break;
					}
					break;
				case 10:
					switch (theme[0])
					{
					case 'B':
						if (theme == "Bombs Away")
						{
							color = new Color(50, 20, 0);
							white = new Color(color.R + 60, color.G + 60, color.B + 60);
							color2 = new Color(white.R + 30, white.G + 30, white.B + 30);
							color3 = new Color(color2.R + 30, color2.G + 30, color2.B + 30);
							white2 = Color.Tan;
							color4 = new Color(color3.R + 30, color3.G + 30, color3.B + 30);
							color5 = new Color(192, 167, 143);
							color6 = new Color(Math.Min(255, color5.R + 30), Math.Min(255, color5.G + 30), Math.Min(255, color5.B + 30));
							color7 = new Color(color5.R - 30, color5.G - 30, color5.B - 30);
						}
						break;
					case 'P':
						if (theme == "Polynomial")
						{
							color = new Color(60, 60, 60);
							white = new Color(color.R + 60, color.G + 60, color.B + 60);
							color2 = new Color(white.R + 30, white.G + 30, white.B + 30);
							color3 = new Color(color2.R + 30, color2.G + 30, color2.B + 30);
							color4 = new Color(254, 254, 254);
							white2 = new Color(color3.R + 30, color3.G + 30, color3.B + 30);
							color5 = new Color(225, 225, 225);
							color6 = new Color(Math.Min(255, color5.R + 30), Math.Min(255, color5.G + 30), Math.Min(255, color5.B + 30));
							color7 = new Color(color5.R - 30, color5.G - 30, color5.B - 30);
						}
						break;
					}
					break;
				case 5:
					if (theme == "Basic")
					{
						color = new Color(47, 46, 36);
						white = new Color(color.R + 60, color.G + 60, color.B + 60);
						color2 = new Color(white.R + 30, white.G + 30, white.B + 30);
						color3 = new Color(color2.R + 30, color2.G + 30, color2.B + 30);
						white2 = new Color(color3.R + 15, color3.G + 15, color3.B + 15);
						color4 = new Color(white2.R + 15, white2.G + 15, white2.B + 15);
						color5 = new Color(220, 215, 194);
						color6 = new Color(Math.Min(255, color5.R + 30), Math.Min(255, color5.G + 30), Math.Min(255, color5.B + 30));
						color7 = new Color(color5.R - 30, color5.G - 30, color5.B - 30);
					}
					break;
				case 11:
					if (theme == "Outer Space")
					{
						color = new Color(20, 20, 20);
						white = new Color(color.R + 60, color.G + 60, color.B + 60);
						color2 = new Color(white.R + 30, white.G + 30, white.B + 30);
						color3 = new Color(color2.R + 30, color2.G + 30, color2.B + 30);
						white2 = new Color(color3.R + 15, color3.G + 15, color3.B + 15);
						color4 = new Color(white2.R + 15, white2.G + 15, white2.B + 15);
						color5 = new Color(194, 189, 202);
						color6 = new Color(Math.Min(255, color5.R + 30), Math.Min(255, color5.G + 30), Math.Min(255, color5.B + 30));
						color7 = new Color(color5.R - 30, color5.G - 30, color5.B - 30);
					}
					break;
				case 17:
					if (theme == "Ghosts N' Goblins")
					{
						color = new Color(55, 0, 0);
						white = new Color(color.R + 60, color.G + 60, color.B + 60);
						color2 = new Color(white.R + 30, white.G + 30, white.B + 30);
						color3 = new Color(color2.R + 30, color2.G + 30, color2.B + 30);
						white2 = new Color(color3.R + 15, color3.G + 15, color3.B + 15);
						color4 = new Color(white2.R + 15, white2.G + 15, white2.B + 15);
						color5 = new Color(196, 197, 230);
						color6 = new Color(Math.Min(255, color5.R + 30), Math.Min(255, color5.G + 30), Math.Min(255, color5.B + 30));
						color7 = new Color(color5.R - 30, color5.G - 30, color5.B - 30);
					}
					break;
				case 9:
					if (theme == "Wasteland")
					{
						color = new Color(14, 12, 10);
						white = new Color(color.R + 60, color.G + 60, color.B + 60);
						color2 = new Color(white.R + 30, white.G + 30, white.B + 30);
						color3 = new Color(color2.R + 30, color2.G + 30, color2.B + 30);
						white2 = new Color(color3.R + 15, color3.G + 15, color3.B + 15);
						color4 = new Color(white2.R + 15, white2.G + 15, white2.B + 15);
						color5 = new Color(185, 178, 165);
						color6 = new Color(Math.Min(255, color5.R + 30), Math.Min(255, color5.G + 30), Math.Min(255, color5.B + 30));
						color7 = new Color(color5.R - 30, color5.G - 30, color5.B - 30);
					}
					break;
				case 7:
					if (theme == "Duchess")
					{
						color = new Color(69, 45, 0);
						white = new Color(color.R + 60, color.G + 60, color.B + 30);
						color2 = new Color(white.R + 30, white.G + 30, white.B + 20);
						color3 = new Color(color2.R + 30, color2.G + 30, color2.B + 20);
						white2 = new Color(color3.R + 15, color3.G + 15, color3.B + 10);
						color4 = new Color(white2.R + 15, white2.G + 15, white2.B + 10);
						color5 = new Color(227, 221, 174);
						color6 = new Color(Math.Min(255, color5.R + 30), Math.Min(255, color5.G + 30), Math.Min(255, color5.B + 30));
						color7 = new Color(color5.R - 30, color5.G - 30, color5.B - 30);
					}
					break;
				}
			}
			Game1.menuTexture = ColorChanger.swapColor(Game1.menuTexture, 15633, color.R, color.G, color.B);
			Game1.menuTexture = ColorChanger.swapColor(Game1.menuTexture, 15645, color4.R, color4.G, color4.B);
			Game1.menuTexture = ColorChanger.swapColor(Game1.menuTexture, 15649, color3.R, color3.G, color3.B);
			Game1.menuTexture = ColorChanger.swapColor(Game1.menuTexture, 15641, color3.R, color3.G, color3.B);
			Game1.menuTexture = ColorChanger.swapColor(Game1.menuTexture, 15637, color2.R, color2.G, color2.B);
			Game1.menuTexture = ColorChanger.swapColor(Game1.menuTexture, 15666, color5.R, color5.G, color5.B);
			Game1.menuTexture = ColorChanger.swapColor(Game1.menuTexture, 40577, color6.R, color6.G, color6.B);
			Game1.menuTexture = ColorChanger.swapColor(Game1.menuTexture, 40637, color7.R, color7.G, color7.B);
			Game1.toolIconBox = ColorChanger.swapColor(Game1.toolIconBox, 1760, color.R, color.G, color.B);
			Game1.toolIconBox = ColorChanger.swapColor(Game1.toolIconBox, 1764, color2.R, color2.G, color2.B);
			Game1.toolIconBox = ColorChanger.swapColor(Game1.toolIconBox, 1768, color3.R, color3.G, color3.B);
			Game1.toolIconBox = ColorChanger.swapColor(Game1.toolIconBox, 1841, color4.R, color4.G, color4.B);
			Game1.toolIconBox = ColorChanger.swapColor(Game1.toolIconBox, 1792, color5.R, color5.G, color5.B);
			Game1.toolIconBox = ColorChanger.swapColor(Game1.toolIconBox, 1834, color6.R, color6.G, color6.B);
			Game1.toolIconBox = ColorChanger.swapColor(Game1.toolIconBox, 1773, color7.R, color7.G, color7.B);
		}

		public static long RandomLong(Random r = null)
		{
			if (r == null)
			{
				r = new Random();
			}
			byte[] array = new byte[8];
			r.NextBytes(array);
			return BitConverter.ToInt64(array, 0);
		}

		public static ulong NewUniqueIdForThisGame()
		{
			DateTime dateTime = new DateTime(2012, 6, 22);
			return (ulong)(long)(DateTime.UtcNow - dateTime).TotalSeconds;
		}

		public static string FilterDirtyWords(string words)
		{
			return Program.sdk.FilterDirtyWords(words);
		}

		public static string FilterUserName(string name)
		{
			return name;
		}

		public static bool IsHorizontalDirection(int direction)
		{
			if (direction != 3)
			{
				return direction == 1;
			}
			return true;
		}

		public static bool IsVerticalDirection(int direction)
		{
			if (direction != 0)
			{
				return direction == 2;
			}
			return true;
		}

		public static Microsoft.Xna.Framework.Rectangle ExpandRectangle(Microsoft.Xna.Framework.Rectangle rect, int pixels)
		{
			rect.Height += 2 * pixels;
			rect.Width += 2 * pixels;
			rect.X -= pixels;
			rect.Y -= pixels;
			return rect;
		}

		public static Microsoft.Xna.Framework.Rectangle ExpandRectangle(Microsoft.Xna.Framework.Rectangle rect, int facingDirection, int pixels)
		{
			switch (facingDirection)
			{
			case 0:
				rect.Height += pixels;
				rect.Y -= pixels;
				break;
			case 1:
				rect.Width += pixels;
				break;
			case 2:
				rect.Height += pixels;
				break;
			case 3:
				rect.Width += pixels;
				rect.X -= pixels;
				break;
			}
			return rect;
		}

		public static int GetOppositeFacingDirection(int facingDirection)
		{
			return facingDirection switch
			{
				0 => 2, 
				1 => 3, 
				2 => 0, 
				3 => 1, 
				_ => 0, 
			};
		}

		public static string getHeaderFromString(string original)
		{
			return new StringReader(original).ReadLine();
		}

		public static string getBodyFromString(string input, int lines)
		{
			string text = input;
			for (int i = 0; i < lines; i++)
			{
				int num = text.IndexOf('\n');
				if (num < 0)
				{
					return string.Empty;
				}
				text = text.Substring(num + 1);
			}
			return text;
		}

		public static string removeReturnsFromString(string input)
		{
			return Regex.Replace(input, "\\r\\n?|\\n", " ");
		}

		public static int drawMultiLineTextWithShadow(SpriteBatch b, string text, SpriteFont font, Vector2 position, int width, int height, Color col, bool centreY = true, bool actuallyDrawIt = true, bool drawShadows = true, bool centerX = true, bool bold = false, bool close = false, float scale = 1f)
		{
			char[] trimChars = new char[3] { ' ', '\n', '\r' };
			text = text.Trim(trimChars);
			List<string> list = new List<string>();
			string text2 = "";
			int num = (int)position.Y;
			int num2 = 0;
			int num3 = 0;
			string[] array = Regex.Split(text, "\r\n|\r|\n");
			int num4 = (int)(font.MeasureString(array[0]).Y * scale);
			string[] array2 = array;
			foreach (string input in array2)
			{
				string text3 = removeReturnsFromString(input);
				if (!(text3 != ""))
				{
					continue;
				}
				if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
				{
					string[] array3 = Regex.Split(text3, string.Empty);
					num3 += num4;
					string[] array4 = array3;
					foreach (string text4 in array4)
					{
						if (text4 != "")
						{
							if (font.MeasureString(text2 + text4).X * scale < (float)width)
							{
								text2 += text4;
								continue;
							}
							list.Add(text2);
							text2 = text4;
							num3 += num4;
						}
					}
				}
				else
				{
					string[] array3 = text3.Split(' ');
					num3 += num4;
					string[] array5 = array3;
					foreach (string text5 in array5)
					{
						if (text5 != "")
						{
							if (font.MeasureString(text2 + " " + text5).X * scale < (float)width)
							{
								text2 = text2 + ((text2 != "") ? " " : "") + text5;
								continue;
							}
							list.Add(text2);
							text2 = text5;
							num3 += num4;
						}
					}
				}
				if (text2 != "")
				{
					list.Add(text2);
					text2 = "";
				}
			}
			int num5 = 0;
			int num6 = 0;
			foreach (string item in list)
			{
				num6 = (centreY ? ((int)(position.Y + (float)((height - num3) / 2) + (float)num5)) : ((int)(position.Y + (float)num5)));
				if (actuallyDrawIt)
				{
					if (centerX)
					{
						int num7 = (int)(position.X + ((float)width - font.MeasureString(item).X * scale) / 2f);
						if (drawShadows)
						{
							drawTextWithShadow(b, item, font, new Vector2(num7, num6), col, scale);
						}
						else
						{
							drawTextWithShadow(b, item, font, new Vector2(num7, num6), col, scale, -1f, 0, 0, 0f, 0);
						}
						if (bold)
						{
							drawBoldText(b, item, font, To4(new Vector2(num7, num6)), col, scale);
						}
					}
					else
					{
						if (drawShadows)
						{
							drawTextWithShadow(b, item, font, new Vector2(position.X, num6), col, scale);
						}
						else
						{
							drawTextWithShadow(b, item, font, new Vector2(position.X, num6), col, scale, -1f, 0, 0, 0f, 0);
						}
						if (bold)
						{
							drawBoldText(b, item, font, To4(new Vector2(position.X, num6)), col, scale);
						}
					}
				}
				num5 += (close ? (num4 * 2 / 3 + 4) : num4);
			}
			return num6 + (close ? (num4 * 2 / 3 + 4) : num4);
		}

		public static void RGBtoHSL(int r, int g, int b, out double h, out double s, out double l)
		{
			double num = (double)r / 255.0;
			double num2 = (double)g / 255.0;
			double num3 = (double)b / 255.0;
			double num4 = num;
			if (num4 < num2)
			{
				num4 = num2;
			}
			if (num4 < num3)
			{
				num4 = num3;
			}
			double num5 = num;
			if (num5 > num2)
			{
				num5 = num2;
			}
			if (num5 > num3)
			{
				num5 = num3;
			}
			double num6 = num4 - num5;
			l = (num4 + num5) / 2.0;
			if (Math.Abs(num6) < 1E-05)
			{
				s = 0.0;
				h = 0.0;
				return;
			}
			if (l <= 0.5)
			{
				s = num6 / (num4 + num5);
			}
			else
			{
				s = num6 / (2.0 - num4 - num5);
			}
			double num7 = (num4 - num) / num6;
			double num8 = (num4 - num2) / num6;
			double num9 = (num4 - num3) / num6;
			if (num == num4)
			{
				h = num9 - num8;
			}
			else if (num2 == num4)
			{
				h = 2.0 + num7 - num9;
			}
			else
			{
				h = 4.0 + num8 - num7;
			}
			h *= 60.0;
			if (h < 0.0)
			{
				h += 360.0;
			}
		}

		public static void HSLtoRGB(double h, double s, double l, out int r, out int g, out int b)
		{
			double num = ((!(l <= 0.5)) ? (l + s - l * s) : (l * (1.0 + s)));
			double q = 2.0 * l - num;
			double num2;
			double num3;
			double num4;
			if (s == 0.0)
			{
				num2 = l;
				num3 = l;
				num4 = l;
			}
			else
			{
				num2 = QQHtoRGB(q, num, h + 120.0);
				num3 = QQHtoRGB(q, num, h);
				num4 = QQHtoRGB(q, num, h - 120.0);
			}
			r = (int)(num2 * 255.0);
			g = (int)(num3 * 255.0);
			b = (int)(num4 * 255.0);
		}

		private static double QQHtoRGB(double q1, double q2, double hue)
		{
			if (hue > 360.0)
			{
				hue -= 360.0;
			}
			else if (hue < 0.0)
			{
				hue += 360.0;
			}
			if (hue < 60.0)
			{
				return q1 + (q2 - q1) * hue / 60.0;
			}
			if (hue < 180.0)
			{
				return q2;
			}
			if (hue < 240.0)
			{
				return q1 + (q2 - q1) * (240.0 - hue) / 60.0;
			}
			return q1;
		}

		public static float ModifyCoordinateFromUIScale(float coordinate)
		{
			return coordinate * Game1.options.uiScale / Game1.options.zoomLevel;
		}

		public static Vector2 ModifyCoordinatesFromUIScale(Vector2 coordinates)
		{
			return coordinates * Game1.options.uiScale / Game1.options.zoomLevel;
		}

		public static float ModifyCoordinateForUIScale(float coordinate)
		{
			return coordinate / Game1.options.uiScale * Game1.options.zoomLevel;
		}

		public static Vector2 ModifyCoordinatesForUIScale(Vector2 coordinates)
		{
			return coordinates / Game1.options.uiScale * Game1.options.zoomLevel;
		}

		public static bool ShouldIgnoreValueChangeCallback()
		{
			if (Game1.gameMode != 3)
			{
				return true;
			}
			if (Game1.client != null && !Game1.client.readyToPlay)
			{
				return true;
			}
			if (Game1.client != null && Game1.locationRequest != null)
			{
				return true;
			}
			return false;
		}

		public static RenderTarget2D GetFirstRenderTarget2D(GraphicsDevice device)
		{
			if (device.RenderTargetCount > 0)
			{
				device.GetRenderTargets(_bindings);
				return _bindings[0].RenderTarget as RenderTarget2D;
			}
			return null;
		}

		public static string RemoveDangerousChars(string s)
		{
			s = s.Replace("[", "");
			s = s.Replace("]", "");
			s = s.Replace("{", "");
			s = s.Replace("}", "");
			s = s.Replace("\\", "");
			s = s.Replace(":", "");
			return s;
		}

		public static string RemoveDodgyChars(string s)
		{
			if (s == null)
			{
				return null;
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			if (s == null)
			{
				return "";
			}
			for (int i = 0; i < s.Length; i++)
			{
				switch (s[i])
				{
				case '[':
					num++;
					break;
				case ']':
					num2++;
					break;
				case '{':
					num3++;
					break;
				case '}':
					num4++;
					break;
				}
			}
			if (num == num2 && num3 == num4)
			{
				return s;
			}
			return RemoveDangerousChars(s);
		}

		public static double Distance(int x1, int y1, int x2, int y2)
		{
			return Math.Sqrt(Math.Pow(x2 - x1, 2.0) + Math.Pow(y2 - y1, 2.0));
		}
	}
}
