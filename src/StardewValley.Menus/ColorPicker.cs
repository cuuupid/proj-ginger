using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	public class ColorPicker
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

		public ColorPicker(string name, int x, int y)
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
				x -= bounds.X;
				y -= bounds.Y;
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

		public void draw(SpriteBatch b)
		{
			for (int i = 0; i < 24; i++)
			{
				Color color = HsvToRgb((double)i / 24.0 * 360.0, 0.9, 0.9);
				b.Draw(Game1.staminaRect, new Rectangle(bounds.X + bounds.Width / 24 * i, bounds.Y + hueBar.bounds.Center.Y - 2, hueBar.bounds.Width / 24, 4), color);
			}
			b.Draw(Game1.mouseCursors, new Vector2(bounds.X + (int)((float)hueBar.value / 100f * (float)hueBar.bounds.Width), bounds.Y + hueBar.bounds.Center.Y), new Rectangle(64, 256, 32, 32), Color.White, 0f, new Vector2(16f, 9f), 1f, SpriteEffects.None, 0.86f);
			Utility.drawTextWithShadow(b, hueBar.value.ToString() ?? "", Game1.smallFont, new Vector2(bounds.X + bounds.Width + 8, bounds.Y + hueBar.bounds.Y), Game1.textColor);
			for (int j = 0; j < 24; j++)
			{
				Color color2 = HsvToRgb((double)hueBar.value / 100.0 * 360.0, (double)j / 24.0, (double)valueBar.value / 100.0);
				b.Draw(Game1.staminaRect, new Rectangle(bounds.X + bounds.Width / 24 * j, bounds.Y + saturationBar.bounds.Center.Y - 2, saturationBar.bounds.Width / 24, 4), color2);
			}
			b.Draw(Game1.mouseCursors, new Vector2(bounds.X + (int)((float)saturationBar.value / 100f * (float)saturationBar.bounds.Width), bounds.Y + saturationBar.bounds.Center.Y), new Rectangle(64, 256, 32, 32), Color.White, 0f, new Vector2(16f, 9f), 1f, SpriteEffects.None, 0.87f);
			Utility.drawTextWithShadow(b, saturationBar.value.ToString() ?? "", Game1.smallFont, new Vector2(bounds.X + bounds.Width + 8, bounds.Y + saturationBar.bounds.Y), Game1.textColor);
			for (int k = 0; k < 24; k++)
			{
				Color color3 = HsvToRgb((double)hueBar.value / 100.0 * 360.0, (double)saturationBar.value / 100.0, (double)k / 24.0);
				b.Draw(Game1.staminaRect, new Rectangle(bounds.X + bounds.Width / 24 * k, bounds.Y + valueBar.bounds.Center.Y - 2, valueBar.bounds.Width / 24, 4), color3);
			}
			b.Draw(Game1.mouseCursors, new Vector2(bounds.X + (int)((float)valueBar.value / 100f * (float)valueBar.bounds.Width), bounds.Y + valueBar.bounds.Center.Y), new Rectangle(64, 256, 32, 32), Color.White, 0f, new Vector2(16f, 9f), 1f, SpriteEffects.None, 0.86f);
			Utility.drawTextWithShadow(b, valueBar.value.ToString() ?? "", Game1.smallFont, new Vector2(bounds.X + bounds.Width + 8, bounds.Y + valueBar.bounds.Y), Game1.textColor);
		}

		public bool containsPoint(int x, int y)
		{
			return bounds.Contains(x, y);
		}

		public void setColor(Color color)
		{
			RGBtoHSV((int)color.R, (int)color.G, (int)color.B, out var h, out var s, out var v);
			if (float.IsNaN(h))
			{
				h = 0f;
			}
			if (float.IsNaN(s))
			{
				s = 0f;
			}
			if (float.IsNaN(h))
			{
				h = 0f;
			}
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
