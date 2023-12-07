using System;
using System.Collections.Generic;
using System.Linq;
using BmFont;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.BellsAndWhistles
{
	public class SpriteText
	{
		public enum ScrollTextAlignment
		{
			Left,
			Center,
			Right
		}

		public const int scrollStyle_scroll = 0;

		public const int scrollStyle_scrollleftjustified = 0;

		public const int scrollStyle_speechBubble = 1;

		public const int scrollStyle_darkMetal = 2;

		public const int maxCharacter = 999999;

		public const int maxHeight = 999999;

		public const int characterWidth = 8;

		public const int characterHeight = 16;

		public const int horizontalSpaceBetweenCharacters = 0;

		public const int verticalSpaceBetweenCharacters = 2;

		public const char newLine = '^';

		public static float fontPixelZoom = 3f;

		public static float shadowAlpha = 0.15f;

		private static Dictionary<char, FontChar> _characterMap;

		private static FontFile FontFile = null;

		private static List<Texture2D> fontPages = null;

		public static Texture2D spriteTexture;

		public static Texture2D coloredTexture;

		private static bool fontShrunk = false;

		public const int color_Black = 0;

		public const int color_Blue = 1;

		public const int color_Red = 2;

		public const int color_Purple = 3;

		public const int color_White = 4;

		public const int color_Orange = 5;

		public const int color_Green = 6;

		public const int color_Cyan = 7;

		public const int color_Gray = 8;

		public static bool forceEnglishFont = false;

		public static void shrinkFont(bool shrink)
		{
			if (fontShrunk != shrink)
			{
				fontShrunk = shrink;
				SetFontPixelZoom();
			}
		}

		private static void SetFontPixelZoom()
		{
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
			{
				fontPixelZoom = 1.75f;
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh)
			{
				fontPixelZoom = 1.5f;
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.th)
			{
				fontPixelZoom = 1.5f;
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
			{
				fontPixelZoom = 1.5f;
			}
			else if (fontShrunk && Game1.clientBounds.Width < 1080)
			{
				fontPixelZoom = 2f;
			}
			else
			{
				fontPixelZoom = 3f;
			}
		}

		public static void drawScroll(SpriteBatch b, int X, int Y, int Width)
		{
			b.Draw(Game1.mouseCursors, new Rectangle(X, Y, 48, 72), new Rectangle(325, 318, 12, 18), Color.White);
			b.Draw(Game1.mouseCursors, new Rectangle(X + 48, Y, Width - 96, 72), new Rectangle(337, 318, 1, 18), Color.White);
			b.Draw(Game1.mouseCursors, new Rectangle(X + Width - 48, Y, 48, 72), new Rectangle(338, 318, 12, 18), Color.White);
		}

		public static void drawScrollText(SpriteBatch b, string text, SpriteFont font, int X, int Y, int Width)
		{
			drawScroll(b, X, Y, Width);
			int num = (int)((float)Width - font.MeasureString(text).X - 48f) / 2;
			Utility.drawTextWithShadow(b, text, font, new Vector2(X + num + 16, Y + 16), Game1.textColor);
		}

		public static void drawStringHorizontallyCenteredAt(SpriteBatch b, string s, int x, int y, int characterPosition = 999999, int width = -1, int height = 999999, float alpha = 1f, float layerDepth = 0.88f, bool junimoText = false, int color = -1, int maxWidth = 99999)
		{
			drawString(b, s, x - getWidthOfString(s, maxWidth) / 2, y, characterPosition, width, height, alpha, layerDepth, junimoText, -1, "", color);
		}

		public static int getWidthOfString(string s, int widthConstraint = 999999)
		{
			setUpCharacterMap();
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < s.Length; i++)
			{
				if (!LocalizedContentManager.CurrentLanguageLatin && !forceEnglishFont)
				{
					if (_characterMap.TryGetValue(s[i], out var value))
					{
						num += value.XAdvance;
					}
					num2 = Math.Max(num, num2);
					if (s[i] == '^' || (float)num * fontPixelZoom > (float)widthConstraint)
					{
						num = 0;
					}
					continue;
				}
				num += 8 + getWidthOffsetForChar(s[i]);
				if (i > 0)
				{
					num += getWidthOffsetForChar(s[Math.Max(0, i - 1)]);
				}
				num2 = Math.Max(num, num2);
				float num3 = positionOfNextSpace(s, i, (int)((float)num * fontPixelZoom), 0);
				if (s[i] == '^' || (float)num * fontPixelZoom >= (float)widthConstraint || num3 >= (float)widthConstraint)
				{
					num = 0;
				}
			}
			return (int)((float)num2 * fontPixelZoom);
		}

		public static bool IsMissingCharacters(string text)
		{
			setUpCharacterMap();
			if (!LocalizedContentManager.CurrentLanguageLatin && !forceEnglishFont)
			{
				for (int i = 0; i < text.Length; i++)
				{
					if (!_characterMap.ContainsKey(text[i]))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static int getHeightOfString(string s, int widthConstraint = 999999)
		{
			if (s.Length == 0)
			{
				return 0;
			}
			Vector2 vector = default(Vector2);
			int num = 0;
			s = s.Replace(Environment.NewLine, "");
			setUpCharacterMap();
			if (!LocalizedContentManager.CurrentLanguageLatin && !forceEnglishFont)
			{
				for (int i = 0; i < s.Length; i++)
				{
					if (s[i] == '^')
					{
						vector.Y += (float)(FontFile.Common.LineHeight + 2) * fontPixelZoom;
						vector.X = 0f;
						continue;
					}
					if (positionOfNextSpace(s, i, (int)vector.X, num) >= widthConstraint)
					{
						vector.Y += (float)(FontFile.Common.LineHeight + 2) * fontPixelZoom;
						num = 0;
						vector.X = 0f;
					}
					if (_characterMap.TryGetValue(s[i], out var value))
					{
						vector.X += (float)value.XAdvance * fontPixelZoom;
					}
				}
				return (int)(vector.Y + (float)(FontFile.Common.LineHeight + 2) * fontPixelZoom);
			}
			for (int j = 0; j < s.Length; j++)
			{
				if (s[j] == '^')
				{
					vector.Y += 18f * fontPixelZoom;
					vector.X = 0f;
					num = 0;
					continue;
				}
				if (positionOfNextSpace(s, j, (int)vector.X, num) >= widthConstraint)
				{
					vector.Y += 18f * fontPixelZoom;
					num = 0;
					vector.X = 0f;
				}
				vector.X += 8f * fontPixelZoom + (float)num + (float)getWidthOffsetForChar(s[j]) * fontPixelZoom;
				if (j > 0)
				{
					vector.X += (float)getWidthOffsetForChar(s[j - 1]) * fontPixelZoom;
				}
				num = (int)(0f * fontPixelZoom);
			}
			return (int)(vector.Y + 16f * fontPixelZoom);
		}

		public static Color getColorFromIndex(int index)
		{
			switch (index)
			{
			case 1:
				return Color.SkyBlue;
			case 2:
				return Color.Red;
			case 3:
				return new Color(110, 43, 255);
			case -1:
				if (LocalizedContentManager.CurrentLanguageLatin)
				{
					return Color.White;
				}
				return new Color(86, 22, 12);
			case 4:
				return Color.White;
			case 5:
				return Color.OrangeRed;
			case 6:
				return Color.LimeGreen;
			case 7:
				return Color.Cyan;
			case 8:
				return new Color(60, 60, 60);
			default:
				return Color.Black;
			}
		}

		public static string getSubstringBeyondHeight(string s, int width, int height)
		{
			Vector2 vector = default(Vector2);
			int num = 0;
			s = s.Replace(Environment.NewLine, "");
			setUpCharacterMap();
			if (!LocalizedContentManager.CurrentLanguageLatin)
			{
				for (int i = 0; i < s.Length; i++)
				{
					if (s[i] == '^')
					{
						vector.Y += (float)(FontFile.Common.LineHeight + 2) * fontPixelZoom;
						vector.X = 0f;
						num = 0;
						continue;
					}
					if (_characterMap.TryGetValue(s[i], out var value))
					{
						if (i > 0)
						{
							vector.X += (float)value.XAdvance * fontPixelZoom;
						}
						if (positionOfNextSpace(s, i, (int)vector.X, num) >= width)
						{
							vector.Y += (float)(FontFile.Common.LineHeight + 2) * fontPixelZoom;
							num = 0;
							vector.X = 0f;
						}
					}
					if (vector.Y >= (float)height - (float)FontFile.Common.LineHeight * fontPixelZoom * 2f)
					{
						return s.Substring(getLastSpace(s, i));
					}
				}
				return "";
			}
			for (int j = 0; j < s.Length; j++)
			{
				if (s[j] == '^')
				{
					vector.Y += 18f * fontPixelZoom;
					vector.X = 0f;
					num = 0;
					continue;
				}
				if (j > 0)
				{
					vector.X += 8f * fontPixelZoom + (float)num + (float)(getWidthOffsetForChar(s[j]) + getWidthOffsetForChar(s[j - 1])) * fontPixelZoom;
				}
				num = (int)(0f * fontPixelZoom);
				if (positionOfNextSpace(s, j, (int)vector.X, num) >= width)
				{
					vector.Y += 18f * fontPixelZoom;
					num = 0;
					vector.X = 0f;
				}
				if (vector.Y >= (float)height - 16f * fontPixelZoom * 2f)
				{
					return s.Substring(getLastSpace(s, j));
				}
			}
			return "";
		}

		public static int getIndexOfSubstringBeyondHeight(string s, int width, int height)
		{
			Vector2 vector = default(Vector2);
			int num = 0;
			s = s.Replace(Environment.NewLine, "");
			setUpCharacterMap();
			if (!LocalizedContentManager.CurrentLanguageLatin)
			{
				for (int i = 0; i < s.Length; i++)
				{
					if (s[i] == '^')
					{
						vector.Y += (float)(FontFile.Common.LineHeight + 2) * fontPixelZoom;
						vector.X = 0f;
						num = 0;
						continue;
					}
					if (_characterMap.TryGetValue(s[i], out var value))
					{
						if (i > 0)
						{
							vector.X += (float)value.XAdvance * fontPixelZoom;
						}
						if (positionOfNextSpace(s, i, (int)vector.X, num) >= width)
						{
							vector.Y += (float)(FontFile.Common.LineHeight + 2) * fontPixelZoom;
							num = 0;
							vector.X = 0f;
						}
					}
					if (vector.Y >= (float)height - (float)FontFile.Common.LineHeight * fontPixelZoom * 2f)
					{
						return i - 1;
					}
				}
				return s.Length - 1;
			}
			for (int j = 0; j < s.Length; j++)
			{
				if (s[j] == '^')
				{
					vector.Y += 18f * fontPixelZoom;
					vector.X = 0f;
					num = 0;
					continue;
				}
				if (j > 0)
				{
					vector.X += 8f * fontPixelZoom + (float)num + (float)(getWidthOffsetForChar(s[j]) + getWidthOffsetForChar(s[j - 1])) * fontPixelZoom;
				}
				num = (int)(0f * fontPixelZoom);
				if (positionOfNextSpace(s, j, (int)vector.X, num) >= width)
				{
					vector.Y += 18f * fontPixelZoom;
					num = 0;
					vector.X = 0f;
				}
				if (vector.Y >= (float)height - 16f * fontPixelZoom)
				{
					return j - 1;
				}
			}
			return s.Length - 1;
		}

		public static List<string> getStringBrokenIntoSectionsOfHeight(string s, int width, int height)
		{
			List<string> list = new List<string>();
			while (s.Length > 0)
			{
				string stringPreviousToThisHeightCutoff = getStringPreviousToThisHeightCutoff(s, width, height);
				if (stringPreviousToThisHeightCutoff.Length <= 0)
				{
					break;
				}
				list.Add(stringPreviousToThisHeightCutoff);
				s = s.Substring(list.Last().Length);
			}
			return list;
		}

		public static string getStringPreviousToThisHeightCutoff(string s, int width, int height)
		{
			return s.Substring(0, getIndexOfSubstringBeyondHeight(s, width, height) + 1);
		}

		private static int getLastSpace(string s, int startIndex)
		{
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.th)
			{
				return startIndex;
			}
			for (int num = startIndex; num >= 0; num--)
			{
				if (s[num] == ' ')
				{
					return num;
				}
			}
			return startIndex;
		}

		public static int getWidthOffsetForChar(char c)
		{
			switch (c)
			{
			case ',':
			case '.':
				return -2;
			case '!':
			case 'j':
			case 'l':
			case '¡':
				return -1;
			case 'i':
			case 'ì':
			case 'í':
			case 'î':
			case 'ï':
			case 'ı':
				return -1;
			case '^':
				return -8;
			case '$':
				return 1;
			default:
				return 0;
			}
		}

		public static void drawStringWithScrollCenteredAt(SpriteBatch b, string s, int x, int y, int width, float alpha = 1f, int color = -1, int scrollType = 0, float layerDepth = 0.88f, bool junimoText = false)
		{
			drawString(b, s, x - width / 2 + 5, y, 999999, width, 999999, alpha, layerDepth, junimoText, scrollType, "", color, ScrollTextAlignment.Center);
		}

		public static void drawStringWithScrollCenteredAt(SpriteBatch b, string s, int x, int y, string placeHolderWidthText = "", float alpha = 1f, int color = -1, int scrollType = 0, float layerDepth = 0.88f, bool junimoText = false)
		{
			drawString(b, s, x - getWidthOfString((placeHolderWidthText.Length > 0) ? placeHolderWidthText : s) / 2, y, 999999, -1, 999999, alpha, layerDepth, junimoText, scrollType, placeHolderWidthText, color, ScrollTextAlignment.Center);
		}

		public static void drawStringWithScrollBackground(SpriteBatch b, string s, int x, int y, string placeHolderWidthText = "", float alpha = 1f, int color = -1, ScrollTextAlignment scroll_text_alignment = ScrollTextAlignment.Left)
		{
			drawString(b, s, x, y, 999999, -1, 999999, alpha, 0.88f, junimoText: false, 0, placeHolderWidthText, color, scroll_text_alignment);
		}

		private static FontFile loadFont(string assetName)
		{
			return FontLoader.Parse(Game1.content.Load<XmlSource>(assetName).Source);
		}

		private static void setUpCharacterMap()
		{
			if (!LocalizedContentManager.CurrentLanguageLatin && _characterMap == null)
			{
				_characterMap = new Dictionary<char, FontChar>();
				fontPages = new List<Texture2D>();
				switch (LocalizedContentManager.CurrentLanguageCode)
				{
				case LocalizedContentManager.LanguageCode.ja:
					FontFile = loadFont("Fonts\\Japanese");
					fontPixelZoom = 1.75f;
					break;
				case LocalizedContentManager.LanguageCode.zh:
					FontFile = loadFont("Fonts\\Chinese");
					fontPixelZoom = 1.5f;
					break;
				case LocalizedContentManager.LanguageCode.ru:
					FontFile = loadFont("Fonts\\Russian");
					fontPixelZoom = 3f;
					break;
				case LocalizedContentManager.LanguageCode.th:
					FontFile = loadFont("Fonts\\Thai");
					fontPixelZoom = 1.5f;
					break;
				case LocalizedContentManager.LanguageCode.ko:
					FontFile = loadFont("Fonts\\Korean");
					fontPixelZoom = 1.5f;
					break;
				case LocalizedContentManager.LanguageCode.mod:
					FontFile = loadFont(LocalizedContentManager.CurrentModLanguage.FontFile);
					fontPixelZoom = LocalizedContentManager.CurrentModLanguage.FontPixelZoom;
					break;
				}
				foreach (FontChar @char in FontFile.Chars)
				{
					char key = (char)@char.ID;
					_characterMap.Add(key, @char);
				}
				foreach (FontPage page in FontFile.Pages)
				{
					fontPages.Add(Game1.content.Load<Texture2D>("Fonts\\" + page.File));
				}
				LocalizedContentManager.OnLanguageChange += OnLanguageChange;
			}
			else if (LocalizedContentManager.CurrentLanguageLatin)
			{
				if (fontShrunk && Game1.clientBounds.Width < 1920)
				{
					fontPixelZoom = 2f;
				}
				else
				{
					fontPixelZoom = 3f;
				}
			}
		}

		public static void drawString(SpriteBatch b, string s, int x, int y, int characterPosition = 999999, int width = -1, int height = 999999, float alpha = 1f, float layerDepth = 0.88f, bool junimoText = false, int drawBGScroll = -1, string placeHolderScrollWidthText = "", int color = -1, ScrollTextAlignment scroll_text_alignment = ScrollTextAlignment.Left)
		{
			setUpCharacterMap();
			bool flag = true;
			if (width == -1)
			{
				flag = false;
				width = Game1.graphics.GraphicsDevice.Viewport.Width - x;
				if (drawBGScroll == 1)
				{
					width = getWidthOfString(s) * 2;
				}
			}
			if (fontPixelZoom < 4f && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ko)
			{
				y += (int)((4f - fontPixelZoom) * 4f);
			}
			Vector2 vector = new Vector2(x, y);
			int num = 0;
			if (drawBGScroll != 1 && vector.X < 0f)
			{
				vector.X = 0f;
			}
			if (drawBGScroll == 0 || drawBGScroll == 0 || drawBGScroll == 2)
			{
				if (fontPixelZoom < 3f && LocalizedContentManager.CurrentLanguageLatin)
				{
					if (drawBGScroll == 2)
					{
						int widthOfString = getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s);
						b.Draw(Game1.mouseCursors, vector + new Vector2(-3f, -3f) * fontPixelZoom, new Rectangle(327, 281, 3, 17), Color.White * alpha, 0f, Vector2.Zero, fontPixelZoom, SpriteEffects.None, layerDepth - 0.001f);
						b.Draw(Game1.mouseCursors, vector + new Vector2(0f, -3f) * fontPixelZoom, new Rectangle(330, 281, 1, 17), Color.White * alpha, 0f, Vector2.Zero, new Vector2((float)widthOfString + fontPixelZoom, fontPixelZoom), SpriteEffects.None, layerDepth - 0.001f);
						b.Draw(Game1.mouseCursors, vector + new Vector2(widthOfString + 4, -3f * fontPixelZoom), new Rectangle(333, 281, 3, 17), Color.White * alpha, 0f, Vector2.Zero, fontPixelZoom, SpriteEffects.None, layerDepth - 0.001f);
					}
					else
					{
						b.Draw(Game1.mouseCursors, vector + new Vector2(-12f, -2f) * fontPixelZoom, new Rectangle(325, 318, 12, 18), Color.White * alpha, 0f, Vector2.Zero, fontPixelZoom, SpriteEffects.None, layerDepth - 0.001f);
						b.Draw(Game1.mouseCursors, vector + new Vector2(0f, -2f) * fontPixelZoom, new Rectangle(337, 318, 1, 18), Color.White * alpha, 0f, Vector2.Zero, new Vector2(getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s), fontPixelZoom), SpriteEffects.None, layerDepth - 0.001f);
						b.Draw(Game1.mouseCursors, vector + new Vector2(getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s), -2f * fontPixelZoom), new Rectangle(338, 318, 12, 18), Color.White * alpha, 0f, Vector2.Zero, fontPixelZoom, SpriteEffects.None, layerDepth - 0.001f);
					}
					if (placeHolderScrollWidthText.Length > 0)
					{
						if (drawBGScroll != 0)
						{
							x += getWidthOfString(placeHolderScrollWidthText) / 2 - getWidthOfString(s) / 2;
						}
						vector.X = x;
					}
					vector.Y += (4f - fontPixelZoom) * fontPixelZoom;
				}
				else
				{
					if (drawBGScroll == 2)
					{
						int widthOfString2 = getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s);
						b.Draw(Game1.mouseCursors, vector + new Vector2(-3f, -3f) * 4f, new Rectangle(327, 281, 3, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
						b.Draw(Game1.mouseCursors, vector + new Vector2(0f, -3f) * 4f, new Rectangle(330, 281, 1, 17), Color.White * alpha, 0f, Vector2.Zero, new Vector2(widthOfString2 + 4, 4f), SpriteEffects.None, layerDepth - 0.001f);
						b.Draw(Game1.mouseCursors, vector + new Vector2(widthOfString2 + 4, -12f), new Rectangle(333, 281, 3, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
					}
					else
					{
						b.Draw(Game1.mouseCursors, vector + new Vector2(-12f, -3f) * 4f, new Rectangle(325, 318, 12, 18), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
						b.Draw(Game1.mouseCursors, vector + new Vector2(0f, -3f) * 4f, new Rectangle(337, 318, 1, 18), Color.White * alpha, 0f, Vector2.Zero, new Vector2(getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s), 4f), SpriteEffects.None, layerDepth - 0.001f);
						b.Draw(Game1.mouseCursors, vector + new Vector2(getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s), -12f), new Rectangle(338, 318, 12, 18), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
					}
					if (placeHolderScrollWidthText.Length > 0)
					{
						if (drawBGScroll != 0)
						{
							x += getWidthOfString(placeHolderScrollWidthText) / 2 - getWidthOfString(s) / 2;
						}
						vector.X = x;
					}
					vector.Y += (4f - fontPixelZoom) * 4f;
				}
			}
			else if (drawBGScroll == 1)
			{
				int widthOfString3 = getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s);
				Vector2 vector2 = vector;
				if (Game1.currentLocation != null && Game1.currentLocation.map != null && Game1.currentLocation.map.Layers[0] != null)
				{
					int num2 = -Game1.viewport.X + 28;
					int num3 = -Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64 - 28;
					if (vector.X < (float)num2)
					{
						vector.X = num2;
					}
					if (vector.X + (float)widthOfString3 > (float)num3)
					{
						vector.X = num3 - widthOfString3;
					}
					vector2.X += widthOfString3 / 2;
					if (vector2.X < vector.X)
					{
						vector.X += vector2.X - vector.X;
					}
					if (vector2.X > vector.X + (float)widthOfString3 - 24f)
					{
						vector.X += vector2.X - (vector.X + (float)widthOfString3 - 24f);
					}
					vector2.X = Utility.Clamp(vector2.X, vector.X, vector.X + (float)widthOfString3 - 24f);
				}
				b.Draw(Game1.mouseCursors, vector + new Vector2(-7f, -3f) * 4f, new Rectangle(324, 299, 7, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
				b.Draw(Game1.mouseCursors, vector + new Vector2(0f, -3f) * 4f, new Rectangle(331, 299, 1, 17), Color.White * alpha, 0f, Vector2.Zero, new Vector2(getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s), 4f), SpriteEffects.None, layerDepth - 0.001f);
				b.Draw(Game1.mouseCursors, vector + new Vector2(widthOfString3, -12f), new Rectangle(332, 299, 7, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
				b.Draw(Game1.mouseCursors, vector2 + new Vector2(0f, 52f), new Rectangle(341, 308, 6, 5), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.0001f);
				x = (int)vector.X;
				if (placeHolderScrollWidthText.Length > 0)
				{
					x += getWidthOfString(placeHolderScrollWidthText) / 2 - getWidthOfString(s) / 2;
					vector.X = x;
				}
				vector.Y += (4f - fontPixelZoom) * 4f;
			}
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
			{
				vector.Y -= 8f;
			}
			s = s.Replace(Environment.NewLine, "");
			if (!junimoText && (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.th || (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod && LocalizedContentManager.CurrentModLanguage.FontApplyYOffset)))
			{
				vector.Y -= (4f - fontPixelZoom) * 4f;
			}
			s = s.Replace('♡', '<');
			for (int i = 0; i < Math.Min(s.Length, characterPosition); i++)
			{
				if (LocalizedContentManager.CurrentLanguageLatin || IsSpecialCharacter(s[i]) || junimoText || forceEnglishFont)
				{
					float num4 = fontPixelZoom;
					if (IsSpecialCharacter(s[i]) || junimoText || forceEnglishFont)
					{
						fontPixelZoom = 3f;
					}
					if (s[i] == '^')
					{
						vector.Y += 18f * fontPixelZoom;
						vector.X = x;
						num = 0;
						fontPixelZoom = num4;
						continue;
					}
					num = (int)(0f * fontPixelZoom);
					bool flag2 = char.IsUpper(s[i]) || s[i] == 'ß';
					Vector2 vector3 = new Vector2(0f, -1 + ((!junimoText && flag2) ? (-3) : 0));
					if (s[i] == 'Ç')
					{
						vector3.Y += 2f;
					}
					if (positionOfNextSpace(s, i, (int)vector.X - x, num) >= width)
					{
						vector.Y += 18f * fontPixelZoom;
						num = 0;
						vector.X = x;
						if (s[i] == ' ')
						{
							fontPixelZoom = num4;
							continue;
						}
					}
					b.Draw((color != -1) ? coloredTexture : spriteTexture, vector + vector3 * fontPixelZoom, getSourceRectForChar(s[i], junimoText), ((IsSpecialCharacter(s[i]) || junimoText) ? Color.White : getColorFromIndex(color)) * alpha, 0f, Vector2.Zero, fontPixelZoom, SpriteEffects.None, layerDepth);
					if (i < s.Length - 1)
					{
						vector.X += 8f * fontPixelZoom + (float)num + (float)getWidthOffsetForChar(s[i + 1]) * fontPixelZoom;
					}
					if (s[i] != '^')
					{
						vector.X += (float)getWidthOffsetForChar(s[i]) * fontPixelZoom;
					}
					fontPixelZoom = num4;
					continue;
				}
				if (s[i] == '^')
				{
					vector.Y += (float)(FontFile.Common.LineHeight + 2) * fontPixelZoom;
					vector.X = x;
					num = 0;
					continue;
				}
				if (i > 0 && IsSpecialCharacter(s[i - 1]))
				{
					vector.X += 24f;
				}
				if (_characterMap.TryGetValue(s[i], out var value))
				{
					Rectangle value2 = new Rectangle(value.X, value.Y, value.Width, value.Height);
					Texture2D texture = fontPages[value.Page];
					if (positionOfNextSpace(s, i, (int)vector.X, num) >= x + width - 4)
					{
						vector.Y += (float)(FontFile.Common.LineHeight + 2) * fontPixelZoom;
						num = 0;
						vector.X = x;
					}
					Vector2 vector4 = new Vector2(vector.X + (float)value.XOffset * fontPixelZoom, vector.Y + (float)value.YOffset * fontPixelZoom);
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru)
					{
						Vector2 vector5 = new Vector2(-1f, 1f) * fontPixelZoom;
						b.Draw(texture, vector4 + vector5, value2, getColorFromIndex(color) * alpha * shadowAlpha, 0f, Vector2.Zero, fontPixelZoom, SpriteEffects.None, layerDepth);
						b.Draw(texture, vector4 + new Vector2(0f, vector5.Y), value2, getColorFromIndex(color) * alpha * shadowAlpha, 0f, Vector2.Zero, fontPixelZoom, SpriteEffects.None, layerDepth);
						b.Draw(texture, vector4 + new Vector2(vector5.X, 0f), value2, getColorFromIndex(color) * alpha * shadowAlpha, 0f, Vector2.Zero, fontPixelZoom, SpriteEffects.None, layerDepth);
					}
					b.Draw(texture, vector4, value2, getColorFromIndex(color) * alpha, 0f, Vector2.Zero, fontPixelZoom, SpriteEffects.None, layerDepth);
					vector.X += (float)value.XAdvance * fontPixelZoom;
				}
			}
		}

		private static bool IsSpecialCharacter(char c)
		{
			if (!c.Equals('<') && !c.Equals('=') && !c.Equals('>') && !c.Equals('@') && !c.Equals('$') && !c.Equals('`'))
			{
				return c.Equals('+');
			}
			return true;
		}

		private static void OnLanguageChange(LocalizedContentManager.LanguageCode code)
		{
			if (_characterMap != null)
			{
				_characterMap.Clear();
			}
			else
			{
				_characterMap = new Dictionary<char, FontChar>();
			}
			if (fontPages != null)
			{
				fontPages.Clear();
			}
			else
			{
				fontPages = new List<Texture2D>();
			}
			switch (code)
			{
			case LocalizedContentManager.LanguageCode.ja:
				FontFile = loadFont("Fonts\\Japanese");
				fontPixelZoom = 1.75f;
				break;
			case LocalizedContentManager.LanguageCode.zh:
				FontFile = loadFont("Fonts\\Chinese");
				fontPixelZoom = 1.5f;
				break;
			case LocalizedContentManager.LanguageCode.ru:
				FontFile = loadFont("Fonts\\Russian");
				fontPixelZoom = 3f;
				break;
			case LocalizedContentManager.LanguageCode.th:
				FontFile = loadFont("Fonts\\Thai");
				fontPixelZoom = 1.5f;
				break;
			case LocalizedContentManager.LanguageCode.ko:
				FontFile = loadFont("Fonts\\Korean");
				fontPixelZoom = 1.5f;
				break;
			}
			foreach (FontChar @char in FontFile.Chars)
			{
				char key = (char)@char.ID;
				_characterMap.Add(key, @char);
			}
			foreach (FontPage page in FontFile.Pages)
			{
				fontPages.Add(Game1.content.Load<Texture2D>("Fonts\\" + page.File));
			}
		}

		public static int positionOfNextSpace(string s, int index, int currentXPosition, int accumulatedHorizontalSpaceBetweenCharacters)
		{
			setUpCharacterMap();
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.th)
			{
				if (_characterMap.TryGetValue(s[index], out var value))
				{
					return currentXPosition + (int)((float)value.XAdvance * fontPixelZoom);
				}
				return currentXPosition + (int)((float)FontFile.Common.LineHeight * fontPixelZoom);
			}
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
			{
				if (_characterMap.TryGetValue(s[index], out var value2))
				{
					return currentXPosition + (int)((float)value2.XAdvance * fontPixelZoom);
				}
				return currentXPosition + (int)((float)FontFile.Common.LineHeight * fontPixelZoom);
			}
			for (int i = index; i < s.Length; i++)
			{
				if (!LocalizedContentManager.CurrentLanguageLatin)
				{
					if (s[i] == ' ' || s[i] == '^')
					{
						return currentXPosition;
					}
					currentXPosition = ((!_characterMap.TryGetValue(s[i], out var value3)) ? (currentXPosition + (int)((float)FontFile.Common.LineHeight * fontPixelZoom)) : (currentXPosition + (int)((float)value3.XAdvance * fontPixelZoom)));
					continue;
				}
				if (s[i] == ' ' || s[i] == '^')
				{
					return currentXPosition;
				}
				currentXPosition += (int)(8f * fontPixelZoom + (float)accumulatedHorizontalSpaceBetweenCharacters + (float)(getWidthOffsetForChar(s[i]) + getWidthOffsetForChar(s[Math.Max(0, i - 1)])) * fontPixelZoom);
				accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * fontPixelZoom);
			}
			return currentXPosition;
		}

		private static Rectangle getSourceRectForChar(char c, bool junimoText)
		{
			int num = c - 32;
			switch (c)
			{
			case 'Œ':
				num = 96;
				break;
			case 'œ':
				num = 97;
				break;
			case 'Ğ':
				num = 102;
				break;
			case 'ğ':
				num = 103;
				break;
			case 'İ':
				num = 98;
				break;
			case 'ı':
				num = 99;
				break;
			case 'Ş':
				num = 100;
				break;
			case 'ş':
				num = 101;
				break;
			case '’':
				num = 104;
				break;
			case 'Ő':
				num = 105;
				break;
			case 'ő':
				num = 106;
				break;
			case 'Ű':
				num = 107;
				break;
			case 'ű':
				num = 108;
				break;
			}
			return new Rectangle(num * 8 % spriteTexture.Width, num * 8 / spriteTexture.Width * 16 + (junimoText ? 224 : 0), 8, 16);
		}
	}
}
