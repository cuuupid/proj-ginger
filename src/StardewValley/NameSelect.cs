using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley
{
	[InstanceStatics]
	public class NameSelect
	{
		public const int maxNameLength = 9;

		public const int charactersPerRow = 15;

		public static string name = "";

		internal static int selection = 0;

		internal static List<char> namingCharacters;

		public static void load()
		{
			namingCharacters = new List<char>();
			for (int i = 0; i < 25; i += 5)
			{
				for (int j = 0; j < 5; j++)
				{
					namingCharacters.Add((char)(97 + i + j));
				}
				for (int k = 0; k < 5; k++)
				{
					namingCharacters.Add((char)(65 + i + k));
				}
				if (i < 10)
				{
					for (int l = 0; l < 5; l++)
					{
						namingCharacters.Add((char)(48 + i + l));
					}
				}
				else if (i < 15)
				{
					namingCharacters.Add('?');
					namingCharacters.Add('$');
					namingCharacters.Add('\'');
					namingCharacters.Add('#');
					namingCharacters.Add('[');
				}
				else if (i < 20)
				{
					namingCharacters.Add('-');
					namingCharacters.Add('=');
					namingCharacters.Add('~');
					namingCharacters.Add('&');
					namingCharacters.Add('!');
				}
				else
				{
					namingCharacters.Add('Z');
					namingCharacters.Add('z');
					namingCharacters.Add('<');
					namingCharacters.Add('"');
					namingCharacters.Add(']');
				}
			}
		}

		public static void draw()
		{
			int num = Math.Min(Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width - Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width % 64, Game1.graphics.GraphicsDevice.Viewport.Width - Game1.graphics.GraphicsDevice.Viewport.Width % 64 - 128);
			int num2 = Math.Min(Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Height - Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Height % 64, Game1.graphics.GraphicsDevice.Viewport.Height - Game1.graphics.GraphicsDevice.Viewport.Height % 64 - 64);
			int num3 = Game1.graphics.GraphicsDevice.Viewport.Width / 2 - num / 2;
			int num4 = Game1.graphics.GraphicsDevice.Viewport.Height / 2 - num2 / 2;
			int num5 = (num - 128) / 15;
			int num6 = (num2 - 256) / 5;
			Game1.drawDialogueBox(num3, num4, num, num2, speaker: false, drawOnlyBox: true);
			string text = "";
			switch (Game1.nameSelectType)
			{
			case "samBand":
				text = Game1.content.LoadString("Strings\\StringsFromCSFiles:NameSelect.cs.3856");
				break;
			case "Animal":
			case "playerName":
			case "coopDwellerBorn":
				text = Game1.content.LoadString("Strings\\StringsFromCSFiles:NameSelect.cs.3860");
				break;
			}
			Game1.spriteBatch.DrawString(Game1.dialogueFont, text, new Vector2(num3 + 128, num4 + 128), Game1.textColor);
			int num7 = (int)Game1.dialogueFont.MeasureString(text).X;
			string text2 = "";
			for (int i = 0; i < 9; i++)
			{
				if (name.Length > i)
				{
					Game1.spriteBatch.DrawString(Game1.dialogueFont, name[i].ToString() ?? "", new Vector2((float)(num3 + 128 + num7) + Game1.dialogueFont.MeasureString(text2).X + (Game1.dialogueFont.MeasureString("_").X - Game1.dialogueFont.MeasureString(name[i].ToString() ?? "").X) / 2f - 2f, num4 + 128 - 6), Game1.textColor);
				}
				text2 += "_ ";
			}
			Game1.spriteBatch.DrawString(Game1.dialogueFont, "_ _ _ _ _ _ _ _ _", new Vector2(num3 + 128 + num7, num4 + 128), Game1.textColor);
			Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:NameSelect.cs.3864"), new Vector2(num3 + num - 192, num4 + num2 - 96), Game1.textColor);
			for (int j = 0; j < 5; j++)
			{
				int num8 = 0;
				for (int k = 0; k < 15; k++)
				{
					if (k != 0 && k % 5 == 0)
					{
						num8 += num5 / 3;
					}
					Game1.spriteBatch.DrawString(Game1.dialogueFont, namingCharacters[j * 15 + k].ToString() ?? "", new Vector2(num8 + num3 + 64 + num5 * k, num4 + 192 + num6 * j), Game1.textColor);
					if (selection == j * 15 + k)
					{
						Game1.spriteBatch.Draw(Game1.objectSpriteSheet, new Vector2(num8 + num3 + num5 * k - 6, num4 + 192 + num6 * j - 8), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 26), Color.White);
					}
				}
			}
			if (selection == -1)
			{
				Game1.spriteBatch.Draw(Game1.objectSpriteSheet, new Vector2(num3 + num - 192 - 64 - 6, num4 + num2 - 96 - 8), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 26), Color.White);
			}
		}

		public static bool select()
		{
			if (selection == -1)
			{
				if (name.Length > 0)
				{
					return true;
				}
			}
			else if (name.Length < 9)
			{
				name += namingCharacters[selection];
				Game1.playSound("smallSelect");
			}
			return false;
		}

		public static void startButton()
		{
			if (name.Length > 0)
			{
				selection = -1;
				Game1.playSound("smallSelect");
			}
		}

		public static bool isOnDone()
		{
			return selection == -1;
		}

		public static void backspace()
		{
			if (name.Length > 0)
			{
				name = name.Remove(name.Length - 1);
				Game1.playSound("toolSwap");
			}
		}

		public static bool cancel()
		{
			if ((Game1.nameSelectType.Equals("samBand") || Game1.nameSelectType.Equals("coopDwellerBorn")) && name.Length > 0)
			{
				Game1.playSound("toolSwap");
				name = name.Remove(name.Length - 1);
				return false;
			}
			selection = 0;
			name = "";
			return true;
		}

		public static void moveSelection(int direction)
		{
			Game1.playSound("toolSwap");
			if (direction.Equals(0))
			{
				if (selection == -1)
				{
					selection = namingCharacters.Count - 2;
				}
				else if (selection - 15 < 0)
				{
					selection = namingCharacters.Count - 15 + selection;
				}
				else
				{
					selection -= 15;
				}
			}
			else if (direction.Equals(1))
			{
				selection++;
				if (selection % 15 == 0)
				{
					selection -= 15;
				}
			}
			else if (direction.Equals(2))
			{
				if (selection >= namingCharacters.Count - 2)
				{
					selection = -1;
					return;
				}
				selection += 15;
				if (selection >= namingCharacters.Count)
				{
					selection -= namingCharacters.Count;
				}
			}
			else if (direction.Equals(3))
			{
				if (selection % 15 == 0)
				{
					selection += 14;
				}
				else
				{
					selection--;
				}
			}
		}
	}
}
