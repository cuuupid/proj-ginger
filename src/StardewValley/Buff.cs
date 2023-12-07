using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using StardewValley.Menus;

namespace StardewValley
{
	public class Buff
	{
		public const float glowRate = 0.05f;

		public const int farming = 0;

		public const int fishing = 1;

		public const int mining = 2;

		public const int luck = 4;

		public const int foraging = 5;

		public const int crafting = 6;

		public const int maxStamina = 7;

		public const int magneticRadius = 8;

		public const int speed = 9;

		public const int defense = 10;

		public const int attack = 11;

		public const int totalNumberOfBuffableAttriutes = 12;

		public const int goblinsCurse = 12;

		public const int slimed = 13;

		public const int evilEye = 14;

		public const int chickenedOut = 15;

		public const int tipsy = 17;

		public const int fear = 18;

		public const int frozen = 19;

		public const int warriorEnergy = 20;

		public const int yobaBlessing = 21;

		public const int adrenalineRush = 22;

		public const int avoidMonsters = 23;

		public const int full = 6;

		public const int quenched = 7;

		public const int spawnMonsters = 24;

		public const int nauseous = 25;

		public const int darkness = 26;

		public const int weakness = 27;

		public const int squidInkRavioli = 28;

		public int millisecondsDuration;

		public int totalMillisecondsDuration;

		public int[] buffAttributes = new int[12];

		public string description;

		public string source;

		public string displaySource;

		public int total;

		public int sheetIndex = -1;

		public int which = -1;

		public Color glow;

		public float displayAlphaTimer;

		public bool alreadyUpdatedIconAlpha;

		public Buff(string description, int millisecondsDuration, string source, int index)
		{
			this.description = description;
			this.millisecondsDuration = millisecondsDuration;
			sheetIndex = index;
			this.source = source;
			totalMillisecondsDuration = millisecondsDuration;
		}

		public Buff(int which)
		{
			this.which = which;
			sheetIndex = which;
			bool flag = true;
			switch (which)
			{
			case 12:
				description = Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.453") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.454") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.455");
				buffAttributes[9] = -2;
				buffAttributes[10] = -3;
				buffAttributes[11] = -3;
				glow = Color.Yellow;
				millisecondsDuration = 6000;
				break;
			case 6:
				description = Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.456");
				millisecondsDuration = 180000;
				flag = false;
				break;
			case 7:
				description = Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.457");
				millisecondsDuration = 60000;
				flag = false;
				break;
			case 17:
				description = Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.458") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.459");
				buffAttributes[9] = -1;
				glow = Color.OrangeRed * 0.5f;
				millisecondsDuration = 30000;
				break;
			case 13:
				description = Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.460") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.461");
				buffAttributes[9] = -4;
				glow = Color.Green;
				millisecondsDuration = 2500 + Game1.random.Next(500);
				break;
			case 18:
				description = Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.462") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.463");
				buffAttributes[11] = -8;
				glow = new Color(50, 0, 30);
				millisecondsDuration = 8000;
				break;
			case 14:
				description = Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.464") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.465");
				buffAttributes[10] = -8;
				glow = Color.HotPink;
				millisecondsDuration = 8000;
				break;
			case 19:
				description = Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.466") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.467");
				buffAttributes[9] = -8;
				glow = Color.LightBlue;
				millisecondsDuration = 2000;
				break;
			case 20:
				description = Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.468") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.469");
				buffAttributes[11] = 10;
				glow = Color.Red;
				millisecondsDuration = 5000;
				flag = false;
				break;
			case 21:
				description = Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.470") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.471");
				glow = Color.Orange;
				millisecondsDuration = 5000;
				flag = false;
				break;
			case 22:
				description = Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.472") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.473");
				glow = Color.Cyan;
				millisecondsDuration = 3000;
				sheetIndex = 9;
				buffAttributes[9] = 2;
				flag = false;
				break;
			case 23:
				description = Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.474") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.475");
				glow = Color.LightGreen * 0.25f;
				millisecondsDuration = 600000;
				flag = false;
				break;
			case 24:
				description = Game1.content.LoadString("Strings\\StringsFromCSFiles:MonsterMusk_BuffName") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MonsterMusk_BuffDescription");
				glow = Color.Purple * 0.25f;
				millisecondsDuration = 600000;
				flag = false;
				break;
			case 25:
				description = Game1.content.LoadString("Strings\\StringsFromCSFiles:Nauseous_BuffName") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Nauseous_BuffDescription");
				glow = Color.Green;
				millisecondsDuration = 120000;
				break;
			case 26:
				description = Game1.content.LoadString("Strings\\StringsFromCSFiles:Darkness_BuffName") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Darkness_BuffDescription");
				glow = Color.Purple;
				millisecondsDuration = 5000;
				break;
			case 27:
				description = Game1.content.LoadString("Strings\\StringsFromCSFiles:Debuff_Weakness") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Debuff_Weakness_Description");
				glow = new Color(0, 150, 255);
				millisecondsDuration = 10000;
				buffAttributes[11] = -20;
				break;
			case 28:
				description = Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff_Ravioli_Title") + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff_Ravioli_Description");
				flag = false;
				millisecondsDuration = 180000;
				break;
			}
			if (flag && Game1.player.isWearingRing(525))
			{
				millisecondsDuration /= 2;
			}
			totalMillisecondsDuration = millisecondsDuration;
		}

		public Buff(int farming, int fishing, int mining, int digging, int luck, int foraging, int crafting, int maxStamina, int magneticRadius, int speed, int defense, int attack, int minutesDuration, string source, string displaySource)
		{
			buffAttributes[0] = farming;
			buffAttributes[1] = fishing;
			buffAttributes[2] = mining;
			buffAttributes[4] = luck;
			buffAttributes[5] = foraging;
			buffAttributes[6] = crafting;
			buffAttributes[7] = maxStamina;
			buffAttributes[8] = magneticRadius;
			buffAttributes[9] = speed;
			buffAttributes[10] = defense;
			buffAttributes[11] = attack;
			total = Math.Abs(buffAttributes[0]) + Math.Abs(buffAttributes[2]) + Math.Abs(buffAttributes[1]) + Math.Abs(buffAttributes[4]) + Math.Abs(buffAttributes[5]) + Math.Abs(buffAttributes[6]) + Math.Abs(buffAttributes[7]) + Math.Abs(buffAttributes[8]) + Math.Abs(buffAttributes[9]) + Math.Abs(buffAttributes[10]) + Math.Abs(buffAttributes[11]);
			millisecondsDuration = minutesDuration / 10 * 7000;
			this.source = source;
			this.displaySource = displaySource;
			totalMillisecondsDuration = millisecondsDuration;
		}

		public string getTimeLeft()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.476") + millisecondsDuration / 60000 + ":" + millisecondsDuration % 60000 / 10000 + millisecondsDuration % 60000 % 10000 / 1000;
		}

		public bool update(GameTime time)
		{
			int num = millisecondsDuration;
			millisecondsDuration -= time.ElapsedGameTime.Milliseconds;
			if (which == 13 && num % 500 < millisecondsDuration % 500 && num < 3000)
			{
				Game1.multiplayer.broadcastSprites(Game1.player.currentLocation, new TemporaryAnimatedSprite(44, Game1.player.getStandingPosition() + new Vector2(-40 + Game1.random.Next(-8, 12), Game1.random.Next(-32, -16)), Color.Green * 0.5f, 8, Game1.random.NextDouble() < 0.5, 70f)
				{
					scale = 1f
				});
			}
			if (millisecondsDuration <= 0)
			{
				return true;
			}
			return false;
		}

		public void addBuff()
		{
			Game1.player.addBuffAttributes(buffAttributes);
			if (which != -1)
			{
				int value = 0;
				value = ((!Game1.player.appliedSpecialBuffs.TryGetValue(which, out value)) ? 1 : (value + 1));
				Game1.player.appliedSpecialBuffs[which] = value;
			}
			_ = glow;
			if (!glow.Equals(Color.White))
			{
				Game1.player.startGlowing(glow, border: false, 0.05f);
			}
			int num = which;
			if (num == 19)
			{
				Game1.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(118, 227, 16, 13), Game1.player.getStandingPosition() + new Vector2(-32f, -21f), flipped: false, 0f, Color.White)
				{
					layerDepth = (float)(Game1.player.getStandingY() + 1) / 10000f,
					animationLength = 1,
					interval = 2000f,
					scale = 4f
				});
			}
		}

		public string getDescription(int which)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (description != null && description.Length > 1)
			{
				stringBuilder.AppendLine(description);
			}
			else
			{
				if (which == 0)
				{
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es)
					{
						stringBuilder.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.480") + ((buffAttributes[0] > 0) ? "+" : "-") + buffAttributes[0]);
					}
					else
					{
						stringBuilder.AppendLine(((buffAttributes[0] > 0) ? "+" : "-") + buffAttributes[0] + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.480"));
					}
				}
				if (which == 1)
				{
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es)
					{
						stringBuilder.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.483") + ((buffAttributes[1] > 0) ? "+" : "-") + buffAttributes[1]);
					}
					else
					{
						stringBuilder.AppendLine(((buffAttributes[1] > 0) ? "+" : "-") + buffAttributes[1] + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.483"));
					}
				}
				if (which == 2)
				{
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es)
					{
						stringBuilder.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.486") + ((buffAttributes[2] > 0) ? "+" : "-") + buffAttributes[2]);
					}
					else
					{
						stringBuilder.AppendLine(((buffAttributes[2] > 0) ? "+" : "-") + buffAttributes[2] + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.486"));
					}
				}
				if (which == 4)
				{
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es)
					{
						stringBuilder.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.489") + ((buffAttributes[4] > 0) ? "+" : "-") + buffAttributes[4]);
					}
					else
					{
						stringBuilder.AppendLine(((buffAttributes[4] > 0) ? "+" : "-") + buffAttributes[4] + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.489"));
					}
				}
				if (which == 5)
				{
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es)
					{
						stringBuilder.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.492") + ((buffAttributes[5] > 0) ? "+" : "-") + buffAttributes[5]);
					}
					else
					{
						stringBuilder.AppendLine(((buffAttributes[5] > 0) ? "+" : "-") + buffAttributes[5] + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.492"));
					}
				}
				if (which == 7)
				{
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es)
					{
						stringBuilder.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.495") + ((buffAttributes[7] > 0) ? "+" : "-") + buffAttributes[7]);
					}
					else
					{
						stringBuilder.AppendLine(((buffAttributes[7] > 0) ? "+" : "-") + buffAttributes[7] + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.495"));
					}
				}
				if (which == 8)
				{
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es)
					{
						stringBuilder.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.498") + ((buffAttributes[8] > 0) ? "+" : "-") + buffAttributes[8]);
					}
					else
					{
						stringBuilder.AppendLine(((buffAttributes[8] > 0) ? "+" : "-") + buffAttributes[8] + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.498"));
					}
				}
				if (which == 10)
				{
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es)
					{
						stringBuilder.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.501") + ((buffAttributes[10] > 0) ? "+" : "-") + buffAttributes[10]);
					}
					else
					{
						stringBuilder.AppendLine(((buffAttributes[10] > 0) ? "+" : "-") + buffAttributes[10] + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.501"));
					}
				}
				if (which == 11)
				{
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es)
					{
						stringBuilder.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.504") + ((buffAttributes[11] > 0) ? "+" : "-") + buffAttributes[11]);
					}
					else
					{
						stringBuilder.AppendLine(((buffAttributes[11] > 0) ? "+" : "-") + buffAttributes[11] + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.504"));
					}
				}
				if (which == 9)
				{
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es)
					{
						stringBuilder.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.507") + ((buffAttributes[9] > 0) ? "+" : "-") + buffAttributes[9]);
					}
					else
					{
						stringBuilder.AppendLine(((buffAttributes[9] > 0) ? "+" : "-") + buffAttributes[9] + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.507"));
					}
				}
			}
			if (source != null && !source.Equals(""))
			{
				stringBuilder.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.508") + displaySource);
			}
			return stringBuilder.ToString();
		}

		public bool betterThan(Buff other)
		{
			if (total <= 0)
			{
				return false;
			}
			if (other == null)
			{
				return true;
			}
			if (total > other.total)
			{
				return true;
			}
			return false;
		}

		public void removeBuff()
		{
			Game1.player.removeBuffAttributes(buffAttributes);
			if (which != -1)
			{
				int value = 0;
				if (Game1.player.appliedSpecialBuffs.TryGetValue(which, out value))
				{
					value--;
					if (value > 1)
					{
						value--;
						Game1.player.appliedSpecialBuffs[which] = value;
					}
					else
					{
						Game1.player.appliedSpecialBuffs.Remove(which);
					}
				}
			}
			_ = glow;
			if (glow.Equals(Color.White))
			{
				return;
			}
			Game1.player.stopGlowing();
			foreach (Buff otherBuff in Game1.buffsDisplay.otherBuffs)
			{
				if (!otherBuff.Equals(this))
				{
					_ = otherBuff.glow;
					if (!otherBuff.glow.Equals(Color.White))
					{
						Game1.player.startGlowing(otherBuff.glow, border: false, 0.05f);
					}
				}
			}
		}

		public List<ClickableTextureComponent> getClickableComponents()
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			if (sheetIndex != -1)
			{
				dictionary.Add(sheetIndex, 0);
			}
			else
			{
				if (buffAttributes[0] != 0)
				{
					dictionary.Add(0, buffAttributes[0]);
				}
				if (buffAttributes[1] != 0)
				{
					dictionary.Add(1, buffAttributes[1]);
				}
				if (buffAttributes[2] != 0)
				{
					dictionary.Add(2, buffAttributes[2]);
				}
				if (buffAttributes[4] != 0)
				{
					dictionary.Add(4, buffAttributes[4]);
				}
				if (buffAttributes[5] != 0)
				{
					dictionary.Add(5, buffAttributes[5]);
				}
				if (buffAttributes[7] != 0)
				{
					dictionary.Add(16, buffAttributes[7]);
				}
				if (buffAttributes[11] != 0)
				{
					dictionary.Add(11, buffAttributes[11]);
				}
				if (buffAttributes[8] != 0)
				{
					dictionary.Add(8, buffAttributes[8]);
				}
				if (buffAttributes[10] != 0)
				{
					dictionary.Add(10, buffAttributes[10]);
				}
				if (buffAttributes[9] != 0)
				{
					dictionary.Add(9, buffAttributes[9]);
				}
			}
			List<ClickableTextureComponent> list = new List<ClickableTextureComponent>();
			foreach (KeyValuePair<int, int> item in dictionary)
			{
				list.Add(new ClickableTextureComponent("", Rectangle.Empty, null, getDescription(getAttributeIndexFromSourceRectIndex(item.Key)), Game1.buffsIcons, Game1.getSourceRectForStandardTileSheet(Game1.buffsIcons, item.Key, 16, 16), 4f));
			}
			return list;
		}

		public static int getAttributeIndexFromSourceRectIndex(int index)
		{
			if (index == 16)
			{
				return 7;
			}
			return index;
		}

		public static string getBuffTypeFromBuffDescriptionIndex(int index)
		{
			string result = "";
			switch (index)
			{
			case 0:
				result = "farming";
				break;
			case 1:
				result = "fishing";
				break;
			case 2:
				result = "mining";
				break;
			case 3:
				result = "digging";
				break;
			case 4:
				result = "luck";
				break;
			case 5:
				result = "foraging";
				break;
			case 6:
				result = "crafting speed";
				break;
			case 7:
				result = "max energy";
				break;
			case 8:
				result = "magnetism";
				break;
			case 9:
				result = "speed";
				break;
			case 10:
				result = "defense";
				break;
			case 11:
				result = "attack";
				break;
			}
			return result;
		}
	}
}
