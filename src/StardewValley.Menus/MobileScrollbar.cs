using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	public class MobileScrollbar
	{
		public Rectangle Bounds;

		public ClickableTextureComponent upArrow;

		public ClickableTextureComponent downArrow;

		public ClickableTextureComponent slider;

		public ClickableTextureComponent top;

		public ClickableTextureComponent bottom;

		public Rectangle middle;

		private Texture2D middleTex;

		public int sliderMin;

		public int sliderMax;

		public float percentage;

		private int addWL;

		private int addWR;

		public bool showArrows;

		public MobileScrollbar(int x, int y, int width, int height, int additionalWidthLeft = 0, int additionalWidthRight = 0, bool showArrows = false)
		{
			Bounds.X = x;
			Bounds.Y = y;
			Bounds.Height = height;
			Bounds.Width = width;
			percentage = 0f;
			addWL = additionalWidthLeft;
			addWR = additionalWidthRight;
			this.showArrows = showArrows;
			if (showArrows)
			{
				upArrow = new ClickableTextureComponent(new Rectangle(x + 4, y, 40, 40), Game1.mobileSpriteSheet, new Rectangle(30, 76, 10, 10), 4f);
				downArrow = new ClickableTextureComponent(new Rectangle(x + 4, y + height - 40, 40, 40), Game1.mobileSpriteSheet, new Rectangle(30, 86, 10, 10), 4f);
				top = new ClickableTextureComponent(new Rectangle(x, y, 48, 60), Game1.mobileSpriteSheet, new Rectangle(40, 78, 12, 15), 4f);
				bottom = new ClickableTextureComponent(new Rectangle(x, y + height - 60, 48, 60), Game1.mobileSpriteSheet, new Rectangle(52, 79, 12, 16), 4f);
				middle = new Rectangle(x, y + 60, 48, bottom.bounds.Y - y - 60);
				sliderMin = upArrow.bounds.Y + upArrow.bounds.Height + 8;
				sliderMax = downArrow.bounds.Y - 88;
				slider = new ClickableTextureComponent(new Rectangle(middle.X + 4, sliderMin, 40, 76), Game1.mobileSpriteSheet, new Rectangle(20, 76, 10, 20), 4f);
			}
			else
			{
				top = new ClickableTextureComponent(new Rectangle(x, y, 48, 24), Game1.mobileSpriteSheet, new Rectangle(40, 88, 12, 6), 4f);
				bottom = new ClickableTextureComponent(new Rectangle(x, y + height - 20, 48, 24), Game1.mobileSpriteSheet, new Rectangle(52, 79, 12, 6), 4f);
				middle = new Rectangle(x, y + 24, 48, bottom.bounds.Y - y - 24);
				sliderMin = top.bounds.Y;
				sliderMax = bottom.bounds.Y - 56;
				slider = new ClickableTextureComponent(new Rectangle(middle.X + 4, sliderMin, 40, 76), Game1.mobileSpriteSheet, new Rectangle(20, 76, 10, 20), 4f);
			}
		}

		public void draw(SpriteBatch b)
		{
			top.draw(b);
			bottom.draw(b);
			if (showArrows)
			{
				upArrow.draw(b);
				downArrow.draw(b);
			}
			b.Draw(bottom.texture, new Vector2(middle.X, middle.Y), new Rectangle(40, 92, 12, 1), Color.White, 0f, Vector2.Zero, new Vector2(4f, middle.Height), SpriteEffects.None, 0f);
			slider.draw(b);
		}

		public void setPercentage(float newPercent)
		{
			if (newPercent > 100f)
			{
				newPercent = 100f;
			}
			else if (newPercent < 0f)
			{
				newPercent = 0f;
			}
			slider.bounds.Y = (int)((float)(sliderMax - sliderMin) * newPercent / 100f) + sliderMin;
		}

		public float setY(int newY)
		{
			if (newY <= sliderMax && newY >= sliderMin)
			{
				float num = 100 * (newY - sliderMin) / (sliderMax - sliderMin);
				if (num > 100f)
				{
					num = 100f;
				}
				if (num < 0f)
				{
					num = 0f;
				}
				setPercentage(num);
				return num;
			}
			if (newY > sliderMax)
			{
				setPercentage(100f);
				return 100f;
			}
			if (newY < sliderMin)
			{
				setPercentage(0f);
				return 0f;
			}
			return -1f;
		}

		public bool upArrowContains(int x, int y)
		{
			if (showArrows && upArrow.bounds.X - addWL <= x && upArrow.bounds.X + upArrow.bounds.Width + addWR >= x && upArrow.bounds.Y <= y && upArrow.bounds.Y + upArrow.bounds.Height >= y)
			{
				return true;
			}
			return false;
		}

		public bool downArrowContains(int x, int y)
		{
			if (showArrows && downArrow.bounds.X - addWL <= x && downArrow.bounds.X + downArrow.bounds.Width + addWR >= x && downArrow.bounds.Y <= y && downArrow.bounds.Y + downArrow.bounds.Height >= y)
			{
				return true;
			}
			return false;
		}

		public bool sliderContains(int x, int y)
		{
			if (slider.bounds.X - addWL <= x && slider.bounds.X + slider.bounds.Width + addWR >= x && slider.bounds.Y <= y && slider.bounds.Y + slider.bounds.Height >= y)
			{
				return true;
			}
			return false;
		}

		public bool sliderRunnerContains(int x, int y)
		{
			if (Bounds.X - addWL <= x && Bounds.X + middle.Width + addWR >= x && Bounds.Y <= y && Bounds.Y + Bounds.Height >= y)
			{
				return true;
			}
			return false;
		}
	}
}
