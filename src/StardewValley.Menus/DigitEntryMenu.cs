using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	internal class DigitEntryMenu : NumberSelectionMenu
	{
		public List<ClickableComponent> digits = new List<ClickableComponent>();

		private int calculatorX;

		private int calculatorY;

		private int calculatorWidth;

		private int calculatorHeight;

		internal static string clear = "c";

		protected override Vector2 centerPosition => new Vector2(Game1.uiViewport.Width / 2, Game1.uiViewport.Height / 2 + 128);

		public DigitEntryMenu(string message, behaviorOnNumberSelect behaviorOnSelection, int price = -1, int minValue = 0, int maxValue = 99, int defaultNumber = 0)
			: base(message, behaviorOnSelection, price, minValue, maxValue, defaultNumber)
		{
			int num = (int)Game1.dialogueFont.MeasureString(message).Y + IClickableMenu.borderWidth * 2 + 384;
			int num2 = 3;
			int num3 = 44;
			int num4 = num3;
			int num5 = 8;
			int num6 = num5;
			int num7 = num2 * num3 + (num2 - 1) * num5;
			calculatorWidth = num3 * num2 + num5 * (num2 - 1) + IClickableMenu.spaceToClearSideBorder * 2 + 128;
			calculatorHeight = num4 * 4 + num6 * 3 + IClickableMenu.spaceToClearTopBorder * 2;
			calculatorX = Game1.uiViewport.Width / 2 - calculatorWidth / 2;
			calculatorY = Game1.uiViewport.Height / 2 - calculatorHeight;
			int num8 = Game1.uiViewport.Width / 2;
			int num9 = Game1.uiViewport.Height / 2 - 384 + 24 + IClickableMenu.spaceToClearTopBorder;
			for (int i = 0; i < 11; i++)
			{
				string name = (i + 1).ToString();
				switch (i)
				{
				case 9:
					name = clear;
					break;
				case 10:
					name = "0";
					break;
				}
				digits.Add(new ClickableComponent(new Rectangle(num8 - num7 / 2 + i % num2 * (num5 + num3), num9 + i / num2 * (num6 + num4), num3, num4), name)
				{
					myID = i,
					rightNeighborID = -99998,
					leftNeighborID = -99998,
					downNeighborID = -99998,
					upNeighborID = -99998
				});
			}
			populateClickableComponentList();
		}

		private void onDigitPressed(string digit)
		{
			if (digit == clear)
			{
				currentValue = 0;
				numberSelectedBox.Text = currentValue.ToString();
			}
			else
			{
				string text = currentValue.ToString();
				currentValue = Math.Min(val2: Convert.ToInt32((!(text == "0")) ? (text + digit) : digit.ToString()), val1: maxValue);
				numberSelectedBox.Text = currentValue.ToString();
			}
		}

		public override bool isWithinBounds(int x, int y)
		{
			if (!base.isWithinBounds(x, y))
			{
				if (x - calculatorX < calculatorWidth && x - calculatorX >= 0 && y - calculatorY < calculatorHeight)
				{
					return y - calculatorY >= 0;
				}
				return false;
			}
			return true;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			foreach (ClickableComponent digit in digits)
			{
				if (digit.containsPoint(x, y))
				{
					Game1.playSound("smallSelect");
					onDigitPressed(digit.name);
				}
			}
			base.receiveLeftClick(x, y, playSound: true);
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			foreach (ClickableComponent digit in digits)
			{
				if (digit.containsPoint(x, y))
				{
					digit.scale = 2f;
				}
				else
				{
					digit.scale = 1f;
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			Game1.drawDialogueBox(calculatorX, calculatorY, calculatorWidth, calculatorHeight, speaker: false, drawOnlyBox: true, null, objectDialogueWithPortrait: false, ignoreTitleSafe: true);
			foreach (ClickableComponent digit in digits)
			{
				if (digit.name == clear)
				{
					b.Draw(Game1.mouseCursors, new Vector2(digit.bounds.X - 4, digit.bounds.Y + 4), new Rectangle((digit.scale > 1f) ? 267 : 256, 256, 10, 10), Color.Black * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
					b.Draw(Game1.mouseCursors, new Vector2(digit.bounds.X, digit.bounds.Y), new Rectangle((digit.scale > 1f) ? 267 : 256, 256, 10, 10), Color.White * 0.6f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.868f);
					Vector2 vector = new Vector2(digit.bounds.X + digit.bounds.Width / 2 - SpriteText.getWidthOfString(digit.name) / 2, digit.bounds.Y + digit.bounds.Height / 2 - SpriteText.getHeightOfString(digit.name) / 2 - 4);
					SpriteText.drawString(b, digit.name, (int)vector.X, (int)vector.Y);
				}
				else
				{
					b.Draw(Game1.mouseCursors, new Vector2(digit.bounds.X - 4, digit.bounds.Y + 4), new Rectangle((digit.scale > 1f) ? 267 : 256, 256, 10, 10), Color.Black * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
					b.Draw(Game1.mouseCursors, new Vector2(digit.bounds.X, digit.bounds.Y), new Rectangle((digit.scale > 1f) ? 267 : 256, 256, 10, 10), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.868f);
					NumberSprite.draw(position: new Vector2(digit.bounds.X + 16 + NumberSprite.numberOfDigits(Convert.ToInt32(digit.name)) * 6, digit.bounds.Y + 24 - NumberSprite.getHeight() / 4), number: Convert.ToInt32(digit.name), b: b, c: Color.Gold, scale: 0.5f, layerDepth: 0.86f, alpha: 1f, secondDigitOffset: 0);
				}
			}
			drawMouse(b);
		}
	}
}
