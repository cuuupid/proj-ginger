using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	public class ChatMessage
	{
		public List<ChatSnippet> message = new List<ChatSnippet>();

		public int timeLeftToDisplay;

		public int verticalSize;

		public float alpha = 1f;

		public Color color;

		public LocalizedContentManager.LanguageCode language;

		public void parseMessageForEmoji(string messagePlaintext)
		{
			if (messagePlaintext == null)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < messagePlaintext.Count(); i++)
			{
				int num;
				if (messagePlaintext[i] == '[')
				{
					if (stringBuilder.Length > 0)
					{
						breakNewLines(stringBuilder);
					}
					stringBuilder.Clear();
					num = messagePlaintext.IndexOf(']', i);
					int num2 = -1;
					if (i + 1 < messagePlaintext.Count())
					{
						num2 = messagePlaintext.IndexOf('[', i + 1);
					}
					if (num != -1 && (num2 == -1 || num2 > num))
					{
						string text = messagePlaintext.Substring(i + 1, num - i - 1);
						int result = -1;
						if (int.TryParse(text, out result))
						{
							if (result < EmojiMenu.totalEmojis)
							{
								message.Add(new ChatSnippet(result));
							}
						}
						else
						{
							if (text != null)
							{
								switch (text.Length)
								{
								case 4:
								{
									char c = text[0];
									if ((uint)c <= 98u)
									{
										if (c != 'a')
										{
											if (c != 'b' || !(text == "blue"))
											{
												break;
											}
										}
										else if (!(text == "aqua"))
										{
											break;
										}
									}
									else if (c != 'g')
									{
										if (c != 'j')
										{
											if (c != 'p' || (!(text == "pink") && !(text == "plum")))
											{
												break;
											}
										}
										else if (!(text == "jade"))
										{
											break;
										}
									}
									else if (!(text == "gray"))
									{
										break;
									}
									goto IL_02f6;
								}
								case 6:
									switch (text[0])
									{
									case 'j':
										break;
									case 'y':
										goto IL_025f;
									case 'o':
										goto IL_0275;
									case 'p':
										goto IL_0288;
									case 's':
										goto IL_0298;
									default:
										goto end_IL_00c2;
									}
									if (!(text == "jungle"))
									{
										break;
									}
									goto IL_02f6;
								case 5:
								{
									char c = text[0];
									if ((uint)c <= 99u)
									{
										if (c != 'b')
										{
											if (c != 'c' || !(text == "cream"))
											{
												break;
											}
										}
										else if (!(text == "brown"))
										{
											break;
										}
									}
									else if (c != 'g')
									{
										if (c != 'p' || !(text == "peach"))
										{
											break;
										}
									}
									else if (!(text == "green"))
									{
										break;
									}
									goto IL_02f6;
								}
								case 3:
									if (!(text == "red"))
									{
										break;
									}
									goto IL_02f6;
								case 11:
									{
										if (!(text == "yellowgreen"))
										{
											break;
										}
										goto IL_02f6;
									}
									IL_02f6:
									if (color.Equals(Color.White))
									{
										color = getColorFromName(text);
									}
									goto IL_0338;
									IL_0298:
									if (!(text == "salmon"))
									{
										break;
									}
									goto IL_02f6;
									IL_0275:
									if (!(text == "orange"))
									{
										break;
									}
									goto IL_02f6;
									IL_0288:
									if (!(text == "purple"))
									{
										break;
									}
									goto IL_02f6;
									IL_025f:
									if (!(text == "yellow"))
									{
										break;
									}
									goto IL_02f6;
									end_IL_00c2:
									break;
								}
							}
							stringBuilder.Append("[");
							stringBuilder.Append(text);
							stringBuilder.Append("]");
						}
						goto IL_0338;
					}
					stringBuilder.Append("[");
					continue;
				}
				stringBuilder.Append(messagePlaintext[i]);
				continue;
				IL_0338:
				i = num;
			}
			if (stringBuilder.Length > 0)
			{
				breakNewLines(stringBuilder);
			}
		}

		public static Color getColorFromName(string name)
		{
			if (name != null)
			{
				switch (name.Length)
				{
				case 4:
					switch (name[0])
					{
					case 'a':
						if (!(name == "aqua"))
						{
							break;
						}
						return Color.MediumTurquoise;
					case 'b':
						if (!(name == "blue"))
						{
							break;
						}
						return Color.DodgerBlue;
					case 'j':
						if (!(name == "jade"))
						{
							break;
						}
						return new Color(50, 230, 150);
					case 'p':
						if (!(name == "pink"))
						{
							if (!(name == "plum"))
							{
								break;
							}
							return new Color(190, 0, 190);
						}
						return Color.HotPink;
					case 'g':
						if (!(name == "gray"))
						{
							break;
						}
						return Color.Gray;
					}
					break;
				case 6:
					switch (name[0])
					{
					case 'j':
						if (!(name == "jungle"))
						{
							break;
						}
						return Color.SeaGreen;
					case 'y':
						if (!(name == "yellow"))
						{
							break;
						}
						return new Color(240, 200, 0);
					case 'o':
						if (!(name == "orange"))
						{
							break;
						}
						return new Color(255, 100, 0);
					case 'p':
						if (!(name == "purple"))
						{
							break;
						}
						return new Color(138, 43, 250);
					case 's':
						if (!(name == "salmon"))
						{
							break;
						}
						return Color.Salmon;
					}
					break;
				case 5:
					switch (name[0])
					{
					case 'g':
						if (!(name == "green"))
						{
							break;
						}
						return new Color(0, 180, 10);
					case 'c':
						if (!(name == "cream"))
						{
							break;
						}
						return new Color(255, 255, 180);
					case 'p':
						if (!(name == "peach"))
						{
							break;
						}
						return new Color(255, 180, 120);
					case 'b':
						if (!(name == "brown"))
						{
							break;
						}
						return new Color(160, 80, 30);
					}
					break;
				case 3:
					if (!(name == "red"))
					{
						break;
					}
					return new Color(220, 20, 20);
				case 11:
					if (!(name == "yellowgreen"))
					{
						break;
					}
					return new Color(182, 214, 0);
				}
			}
			return Color.White;
		}

		private void breakNewLines(StringBuilder sb)
		{
			string[] array = sb.ToString().Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				message.Add(new ChatSnippet(array[i], language));
				if (i != array.Length - 1)
				{
					message.Add(new ChatSnippet(Environment.NewLine, language));
				}
			}
		}

		public static string makeMessagePlaintext(List<ChatSnippet> message, bool include_color_information)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ChatSnippet item in message)
			{
				if (item.message != null)
				{
					stringBuilder.Append(item.message);
				}
				else if (item.emojiIndex != -1)
				{
					stringBuilder.Append("[" + item.emojiIndex + "]");
				}
			}
			if (include_color_information && Game1.player.defaultChatColor != null && !getColorFromName(Game1.player.defaultChatColor).Equals(Color.White))
			{
				stringBuilder.Append(" [");
				stringBuilder.Append(Game1.player.defaultChatColor);
				stringBuilder.Append("]");
			}
			return stringBuilder.ToString();
		}

		public void draw(SpriteBatch b, int x, int y)
		{
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < message.Count; i++)
			{
				if (message[i].emojiIndex != -1)
				{
					b.Draw(ChatBox.emojiTexture, new Vector2((float)x + num + 1f, (float)y + num2 - 4f), new Rectangle(message[i].emojiIndex * 9 % ChatBox.emojiTexture.Width, message[i].emojiIndex * 9 / ChatBox.emojiTexture.Width * 9, 9, 9), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				}
				else if (message[i].message != null)
				{
					if (message[i].message.Equals(Environment.NewLine))
					{
						num = 0f;
						num2 += ChatBox.messageFont(language).MeasureString("(").Y;
					}
					else
					{
						b.DrawString(ChatBox.messageFont(language), message[i].message, new Vector2((float)x + num, (float)y + num2), color * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
					}
				}
				num += message[i].myLength;
				if (num >= 888f)
				{
					num = 0f;
					num2 += ChatBox.messageFont(language).MeasureString("(").Y;
					if (message.Count > i + 1 && message[i + 1].message != null && message[i + 1].message.Equals(Environment.NewLine))
					{
						i++;
					}
				}
			}
		}
	}
}
