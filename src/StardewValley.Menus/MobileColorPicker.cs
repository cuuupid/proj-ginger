using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	public class MobileColorPicker : IClickableMenu
	{
		public const int sliderChunks = 24;

		private int colorIndexSelection;

		private Rectangle bounds;

		public SliderBar hueBar;

		public SliderBar valueBar;

		public SliderBar saturationBar;

		public SliderBar recentSliderBar;

		public string Name;

		public Color LastColor;

		public bool Dirty;

		public int barWidth;

		private int barHeight;

		private int barY;

		public MobileColorPicker(string name, Rectangle bounds)
		{
			int num = bounds.Width / 17;
			barWidth = (bounds.Width - num * 5) / 3;
			barWidth = barWidth / 24 * 24;
			barY = bounds.Height / 4 + bounds.Y;
			barHeight = bounds.Height / 2;
			this.bounds = bounds;
			Name = name;
			hueBar = new SliderBar(num + bounds.X, barY, 50);
			hueBar.bounds.Width = barWidth;
			hueBar.bounds.Height = barHeight;
			saturationBar = new SliderBar(num * 2 + barWidth + bounds.X, barY, 50);
			saturationBar.bounds.Width = barWidth;
			saturationBar.bounds.Height = barHeight;
			valueBar = new SliderBar(num * 3 + barWidth * 2 + bounds.X, barY, 50);
			valueBar.bounds.Width = barWidth;
			valueBar.bounds.Height = barHeight;
		}

		public MobileColorPicker(string name, int x, int y)
		{
			Name = name;
			hueBar = new SliderBar(0, 0, 50);
			saturationBar = new SliderBar(0, 20, 50);
			valueBar = new SliderBar(0, 40, 50);
			bounds = new Rectangle(x, y, SliderBar.defaultWidth, 60);
		}

		public Color getSelectedColor()
		{
			return HsvToRgb((double)hueBar.value / 100.0 * 360.0, (double)saturationBar.value / 100.0, (double)valueBar.value / 100.0);
		}

		public Color click(int x, int y)
		{
			if (bounds.Contains(x, y))
			{
				if (hueBar.bounds.Contains(x, y))
				{
					hueBar.click(x, y);
					recentSliderBar = hueBar;
				}
				if (saturationBar.bounds.Contains(x, y))
				{
					recentSliderBar = saturationBar;
					saturationBar.click(x, y);
				}
				if (valueBar.bounds.Contains(x, y))
				{
					recentSliderBar = valueBar;
					valueBar.click(x, y);
				}
			}
			return getSelectedColor();
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public void changeHue(int amount)
		{
			hueBar.changeValueBy(amount);
			recentSliderBar = hueBar;
		}

		public void changeSaturation(int amount)
		{
			saturationBar.changeValueBy(amount);
			recentSliderBar = saturationBar;
		}

		public void changeValue(int amount)
		{
			valueBar.changeValueBy(amount);
			recentSliderBar = valueBar;
		}

		public Color clickHeld(int x, int y)
		{
			if (recentSliderBar != null)
			{
				x = Math.Max(x, bounds.X);
				x = Math.Min(x, bounds.Right - 1);
				y = recentSliderBar.bounds.Center.Y;
				x -= bounds.X;
				if (recentSliderBar.Equals(hueBar))
				{
					hueBar.click(x, y);
				}
				if (recentSliderBar.Equals(saturationBar))
				{
					saturationBar.click(x, y);
				}
				if (recentSliderBar.Equals(valueBar))
				{
					valueBar.click(x, y);
				}
			}
			return getSelectedColor();
		}

		public void releaseClick()
		{
			hueBar.release(0, 0);
			saturationBar.release(0, 0);
			valueBar.release(0, 0);
			recentSliderBar = null;
		}

		public new void draw(SpriteBatch b)
		{
			Color color;
			for (int i = 0; i < 24; i++)
			{
				color = HsvToRgb((double)i / 24.0 * 360.0, 0.9, 0.9);
				b.Draw(Game1.staminaRect, new Rectangle(hueBar.bounds.X + barWidth / 24 * i, barY + 4, hueBar.bounds.Width / 24, barHeight - 8), color);
			}
			IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(56, 101, 15, 15), hueBar.bounds.X - 12, hueBar.bounds.Y, hueBar.bounds.Width + 24, hueBar.bounds.Height, Color.White, 4f, drawShadow: false);
			color = HsvToRgb((float)hueBar.value / 100f * 360f, 1.0, 1.0);
			b.Draw(Game1.staminaRect, new Rectangle(hueBar.bounds.X + (int)((float)hueBar.value / 100f * (float)hueBar.bounds.Width) - 12, barY - 8, 24, barHeight + 16), color);
			IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(56, 101, 15, 15), hueBar.bounds.X + (int)((float)hueBar.value / 100f * (float)hueBar.bounds.Width) - 18, barY - 12, 36, barHeight + 24, Color.White, 4f, drawShadow: false);
			if (recentSliderBar != null && recentSliderBar.Equals(hueBar))
			{
				b.Draw(Game1.staminaRect, new Rectangle(hueBar.bounds.X + (int)((float)hueBar.value / 100f * (float)hueBar.bounds.Width) - 20 - 12, barY - 8 - 96, barHeight + 16, barHeight + 16), color);
				IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(56, 101, 15, 15), hueBar.bounds.X + (int)((float)hueBar.value / 100f * (float)hueBar.bounds.Width) - 6 - 18 - 12, barY - 12 - 96, barHeight + 24, barHeight + 24, Color.White, 4f, drawShadow: false);
			}
			for (int j = 0; j < 24; j++)
			{
				color = HsvToRgb((double)hueBar.value / 100.0 * 360.0, (double)j / 24.0, (double)valueBar.value / 100.0);
				b.Draw(Game1.staminaRect, new Rectangle(saturationBar.bounds.X + barWidth / 24 * j, barY + 4, saturationBar.bounds.Width / 24, barHeight - 8), color);
			}
			IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(56, 101, 15, 15), saturationBar.bounds.X - 12, saturationBar.bounds.Y, saturationBar.bounds.Width + 24, saturationBar.bounds.Height, Color.White, 4f, drawShadow: false);
			color = HsvToRgb((float)hueBar.value / 100f * 360f, (float)saturationBar.value / 100f, (float)valueBar.value / 100f);
			b.Draw(Game1.staminaRect, new Rectangle(saturationBar.bounds.X + (int)((float)saturationBar.value / 100f * (float)saturationBar.bounds.Width) - 12, barY - 8, 24, barHeight + 16), color);
			IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(56, 101, 15, 15), saturationBar.bounds.X + (int)((float)saturationBar.value / 100f * (float)saturationBar.bounds.Width) - 18, barY - 12, 36, barHeight + 24, Color.White, 4f, drawShadow: false);
			if (recentSliderBar != null && recentSliderBar.Equals(saturationBar))
			{
				b.Draw(Game1.staminaRect, new Rectangle(saturationBar.bounds.X + (int)((float)saturationBar.value / 100f * (float)saturationBar.bounds.Width) - 20 - 12, barY - 8 - 96, barHeight + 16, barHeight + 16), color);
				IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(56, 101, 15, 15), saturationBar.bounds.X + (int)((float)saturationBar.value / 100f * (float)saturationBar.bounds.Width) - 6 - 18 - 12, barY - 12 - 96, barHeight + 24, barHeight + 24, Color.White, 4f, drawShadow: false);
			}
			for (int k = 0; k < 24; k++)
			{
				color = HsvToRgb((double)hueBar.value / 100.0 * 360.0, (double)saturationBar.value / 100.0, (double)k / 24.0);
				b.Draw(Game1.staminaRect, new Rectangle(valueBar.bounds.X + barWidth / 24 * k, barY + 4, valueBar.bounds.Width / 24, barHeight - 8), color);
			}
			IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(56, 101, 15, 15), valueBar.bounds.X - 12, valueBar.bounds.Y, valueBar.bounds.Width + 24, valueBar.bounds.Height, Color.White, 4f, drawShadow: false);
			color = HsvToRgb((float)hueBar.value / 100f * 360f, (float)saturationBar.value / 100f, (float)valueBar.value / 100f);
			b.Draw(Game1.staminaRect, new Rectangle(valueBar.bounds.X + (int)((float)valueBar.value / 100f * (float)valueBar.bounds.Width) - 12, barY - 8, 24, barHeight + 16), color);
			IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(56, 101, 15, 15), valueBar.bounds.X + (int)((float)valueBar.value / 100f * (float)valueBar.bounds.Width) - 18, barY - 12, 36, barHeight + 24, Color.White, 4f, drawShadow: false);
			if (recentSliderBar != null && recentSliderBar.Equals(valueBar))
			{
				b.Draw(Game1.staminaRect, new Rectangle(valueBar.bounds.X + (int)((float)valueBar.value / 100f * (float)valueBar.bounds.Width) - 20 - 12, barY - 8 - 96, barHeight + 16, barHeight + 16), color);
				IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(56, 101, 15, 15), valueBar.bounds.X + (int)((float)valueBar.value / 100f * (float)valueBar.bounds.Width) - 6 - 18 - 12, barY - 12 - 96, barHeight + 24, barHeight + 24, Color.White, 4f, drawShadow: false);
			}
		}

		public bool containsPoint(int x, int y)
		{
			return bounds.Contains(x, y);
		}

		public void setColor(Color color)
		{
			RGBtoHSV((int)color.R, (int)color.G, (int)color.B, out var h, out var s, out var v);
			hueBar.value = (int)(h / 360f * 100f);
			saturationBar.value = (int)(s * 100f);
			valueBar.value = (int)(v / 255f * 100f);
		}

		private void RGBtoHSV(float r, float g, float b, out float h, out float s, out float v)
		{
			float num = Math.Min(Math.Min(r, g), b);
			float num2 = (v = Math.Max(Math.Max(r, g), b));
			float num3 = num2 - num;
			if (num2 != 0f)
			{
				s = num3 / num2;
				if (r == num2)
				{
					h = (g - b) / num3;
				}
				else if (g == num2)
				{
					h = 2f + (b - r) / num3;
				}
				else
				{
					h = 4f + (r - g) / num3;
				}
				h *= 60f;
				if (h < 0f)
				{
					h += 360f;
				}
			}
			else
			{
				s = 0f;
				h = -1f;
			}
		}

		private Color HsvToRgb(double h, double S, double V)
		{
			double num = h;
			while (num < 0.0)
			{
				num += 1.0;
				if (num < -1000000.0)
				{
					num = 0.0;
				}
			}
			while (num >= 360.0)
			{
				num -= 1.0;
			}
			double num4;
			double num3;
			double num2;
			if (V <= 0.0)
			{
				num4 = (num3 = (num2 = 0.0));
			}
			else if (S <= 0.0)
			{
				num4 = (num3 = (num2 = V));
			}
			else
			{
				double num5 = num / 60.0;
				int num6 = (int)Math.Floor(num5);
				double num7 = num5 - (double)num6;
				double num8 = V * (1.0 - S);
				double num9 = V * (1.0 - S * num7);
				double num10 = V * (1.0 - S * (1.0 - num7));
				switch (num6)
				{
				case 0:
					num4 = V;
					num3 = num10;
					num2 = num8;
					break;
				case 1:
					num4 = num9;
					num3 = V;
					num2 = num8;
					break;
				case 2:
					num4 = num8;
					num3 = V;
					num2 = num10;
					break;
				case 3:
					num4 = num8;
					num3 = num9;
					num2 = V;
					break;
				case 4:
					num4 = num10;
					num3 = num8;
					num2 = V;
					break;
				case 5:
					num4 = V;
					num3 = num8;
					num2 = num9;
					break;
				case 6:
					num4 = V;
					num3 = num10;
					num2 = num8;
					break;
				case -1:
					num4 = V;
					num3 = num8;
					num2 = num9;
					break;
				default:
					num4 = (num3 = (num2 = V));
					break;
				}
			}
			return new Color(Clamp((int)(num4 * 255.0)), Clamp((int)(num3 * 255.0)), Clamp((int)(num2 * 255.0)));
		}

		private int Clamp(int i)
		{
			if (i < 0)
			{
				return 0;
			}
			if (i > 255)
			{
				return 255;
			}
			return i;
		}
	}
}
