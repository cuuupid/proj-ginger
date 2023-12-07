using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Objects;

namespace StardewValley.Menus
{
	public class DiscreteColorPicker : IClickableMenu
	{
		public static int sizeOfEachSwatch;

		private int rows;

		private int columns;

		private int swatchGapX;

		private int swatchGapY;

		public Item itemToDrawColored;

		public bool showExample;

		public bool visible = true;

		public int colorSelection;

		public int totalColors;

		public DiscreteColorPicker(int xPosition, int yPosition, int startingColor = 0, Item itemToDrawColored = null, int rows = 1, int w = 1280, int h = 720)
		{
			totalColors = 21;
			xPositionOnScreen = xPosition;
			yPositionOnScreen = yPosition;
			initializeUpperRightCloseButton();
			this.rows = rows;
			columns = totalColors / rows;
			width = w;
			height = h;
			sizeOfEachSwatch = Math.Min((int)(((float)width - 32f) / (float)totalColors / 4f * (float)rows) - 2, (int)(((float)height - 32f) / (float)totalColors / 4f * (float)columns) - 2);
			swatchGapX = Math.Max(2, (int)(((float)width - 128f) / (float)columns / (float)sizeOfEachSwatch));
			swatchGapY = Math.Max(2, (int)((float)height / (float)columns / (float)sizeOfEachSwatch));
			this.itemToDrawColored = itemToDrawColored;
			visible = Game1.player.showChestColorPicker;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public int getSelectionFromColor(Color c)
		{
			for (int i = 0; i < totalColors; i++)
			{
				if (getColorFromSelection(i).Equals(c))
				{
					return i;
				}
			}
			return -1;
		}

		public Color getCurrentColor()
		{
			return getColorFromSelection(colorSelection);
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void update(GameTime time)
		{
			base.update(time);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!visible)
			{
				return;
			}
			base.receiveLeftClick(x, y, playSound);
			Rectangle rectangle = new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth / 2, yPositionOnScreen + IClickableMenu.borderWidth / 2 + swatchGapY * 2, (sizeOfEachSwatch + swatchGapX) * 4 * totalColors / rows, (sizeOfEachSwatch + swatchGapY) * totalColors * 4 / columns);
			if (rectangle.Contains(x, y))
			{
				int num = (y - rectangle.Y) / ((sizeOfEachSwatch + 2) * 4);
				colorSelection = (x - rectangle.X) / ((sizeOfEachSwatch + swatchGapX) * 4) + num * columns;
				try
				{
					Game1.playSound("coin");
				}
				catch (Exception)
				{
				}
				if (itemToDrawColored is Chest)
				{
					(itemToDrawColored as Chest).playerChoiceColor.Value = getColorFromSelection(colorSelection);
					(itemToDrawColored as Chest).resetLidFrame();
				}
			}
		}

		public Color getColorFromSelection(int selection)
		{
			return selection switch
			{
				2 => new Color(119, 191, 255), 
				1 => new Color(85, 85, 255), 
				3 => new Color(0, 170, 170), 
				4 => new Color(0, 234, 175), 
				5 => new Color(0, 170, 0), 
				6 => new Color(159, 236, 0), 
				7 => new Color(255, 234, 18), 
				8 => new Color(255, 167, 18), 
				9 => new Color(255, 105, 18), 
				10 => new Color(255, 0, 0), 
				11 => new Color(135, 0, 35), 
				12 => new Color(255, 173, 199), 
				13 => new Color(255, 117, 195), 
				14 => new Color(172, 0, 198), 
				15 => new Color(143, 0, 255), 
				16 => new Color(89, 11, 142), 
				17 => new Color(64, 64, 64), 
				18 => new Color(100, 100, 100), 
				19 => new Color(200, 200, 200), 
				20 => new Color(254, 254, 254), 
				_ => Color.Black, 
			};
		}

		public override void receiveGamePadButton(Buttons b)
		{
			int num = colorSelection;
			int num2 = num % columns;
			int num3 = num / columns;
			if (b == Buttons.A || b == Buttons.X || b == Buttons.Y)
			{
				Game1.player.showChestColorPicker = !Game1.player.showChestColorPicker;
				visible = false;
			}
			switch (b)
			{
			case Buttons.DPadUp:
			case Buttons.LeftThumbstickUp:
				num3 = Math.Max(0, num3 - 1);
				break;
			case Buttons.DPadDown:
			case Buttons.LeftThumbstickDown:
				num3 = Math.Min(rows - 1, num3 + 1);
				break;
			case Buttons.DPadLeft:
			case Buttons.LeftThumbstickLeft:
				num2 = Math.Max(0, num2 - 1);
				break;
			case Buttons.DPadRight:
			case Buttons.LeftThumbstickRight:
				num2 = Math.Min(columns - 1, num2 + 1);
				break;
			}
			num = (colorSelection = num3 * columns + num2);
			if (itemToDrawColored is Chest)
			{
				(itemToDrawColored as Chest).playerChoiceColor.Value = getColorFromSelection(colorSelection);
				(itemToDrawColored as Chest).resetLidFrame();
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (!visible)
			{
				return;
			}
			IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.LightGray);
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < totalColors; i++)
			{
				if (i == 0)
				{
					b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth / 2, yPositionOnScreen + swatchGapY * 2 + IClickableMenu.borderWidth / 2), new Rectangle(295, 503, 7, 7), Color.White, 0f, Vector2.Zero, 4f * (float)sizeOfEachSwatch / 7f, SpriteEffects.None, 0.088f);
				}
				else
				{
					b.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth / 2 + i % columns * (sizeOfEachSwatch + swatchGapX) * 4, yPositionOnScreen + IClickableMenu.borderWidth / 2 + swatchGapY * 2 + (sizeOfEachSwatch + swatchGapY) * num * 4, sizeOfEachSwatch * 4, sizeOfEachSwatch * 4), getColorFromSelection(i));
				}
				if (i == colorSelection)
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), xPositionOnScreen + IClickableMenu.borderWidth / 2 - 4 + i % columns * (sizeOfEachSwatch + swatchGapX) * 4, yPositionOnScreen + IClickableMenu.borderWidth / 2 + swatchGapY * 2 - 4 + (sizeOfEachSwatch + swatchGapY) * num * 4, (sizeOfEachSwatch + 2) * 4, (sizeOfEachSwatch + 2) * 4, Color.Black, 4f, drawShadow: false);
				}
				num2++;
				if (num2 >= columns)
				{
					num2 = 0;
					num++;
				}
			}
			if (itemToDrawColored != null)
			{
				int num3 = xPositionOnScreen + columns * (sizeOfEachSwatch + swatchGapX) * 4 - swatchGapX;
				if (itemToDrawColored is Chest)
				{
					(itemToDrawColored as Chest).resetLidFrame();
					(itemToDrawColored as Chest).draw(b, num3 + (xPositionOnScreen + width - num3 - 64) / 2, yPositionOnScreen + (height - 32) / 2, 1f, local: true);
				}
			}
		}
	}
}
