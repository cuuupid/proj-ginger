using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	public class SliderBar
	{
		public static int defaultWidth = 128;

		public const int defaultHeight = 20;

		public const int fullHeight = 80;

		public int value;

		public Rectangle bounds;

		public Rectangle expandedBounds;

		public SliderBar(int x, int y, int initialValue)
		{
			bounds = new Rectangle(x, y, defaultWidth, 80);
			value = initialValue;
		}

		public int click(int x, int y, bool wasAlreadyHeld = false)
		{
			if (bounds.Contains(x, y))
			{
				x -= bounds.X;
				value = (int)((float)x / (float)bounds.Width * 100f);
			}
			else
			{
				expandedBounds = new Rectangle(bounds.X, bounds.Y - 100, bounds.Width, bounds.Height + 200);
				if (wasAlreadyHeld && expandedBounds.Contains(x, y))
				{
					x -= bounds.X;
					value = (int)((float)x / (float)bounds.Width * 100f);
					Log.It("expanded:" + x + "," + y + "    " + value);
				}
				else if (wasAlreadyHeld && x > bounds.X + bounds.Width)
				{
					value = 100;
				}
				else if (wasAlreadyHeld && x < bounds.X)
				{
					value = 0;
				}
			}
			return value;
		}

		public void changeValueBy(int amount)
		{
			value += amount;
			value = Math.Max(0, Math.Min(100, value));
		}

		public void release(int x, int y)
		{
		}

		public void draw(SpriteBatch b)
		{
			b.Draw(Game1.mobileSpriteSheet, new Rectangle(bounds.X, bounds.Center.Y + 12, 24, bounds.Width), new Rectangle(57, 79, 6, 5), Color.White, -(float)Math.PI / 2f, new Vector2(0f, 0f), SpriteEffects.None, 0.086f);
			b.Draw(Game1.mobileSpriteSheet, new Rectangle(bounds.X, bounds.Center.Y + 12, 24, 24), new Rectangle(45, 88, 6, 6), Color.White, -(float)Math.PI / 2f, new Vector2(0f, 0f), SpriteEffects.None, 0.086f);
			b.Draw(Game1.mobileSpriteSheet, new Rectangle(bounds.X + bounds.Width - 24, bounds.Center.Y + 12, 24, 24), new Rectangle(57, 79, 6, 6), Color.White, -(float)Math.PI / 2f, new Vector2(0f, 0f), SpriteEffects.None, 0.086f);
			b.Draw(Game1.mobileSpriteSheet, new Vector2(bounds.X + (int)((float)value / 100f * (float)(bounds.Width - 24)), bounds.Center.Y), new Rectangle(24, 76, 6, 20), Color.White, 0f, new Vector2(0f, 10f), 4f, SpriteEffects.None, 0.086f);
		}
	}
}
