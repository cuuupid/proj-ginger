using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	public class HandPointer
	{
		public const int TAP_SCREEN_X_Y = 0;

		public const int TAP_GRID_X_Y = 1;

		public const int TAP_HOLD_SCREEN_X_Y = 2;

		public const int TAP_HOLD_GRID_X_Y = 3;

		public const int DRAG_SCREEN = 4;

		public int X;

		public int Y;

		public int destX;

		public int destY;

		private ClickableTextureComponent hand;

		private tweeningSprite handSprite;

		private const int xOffset = 32;

		private const int yOffset = 32;

		private const float transitionTimeTap = 500f;

		private const float holdTimeTap = 500f;

		private const float transitionTimeTapHold = 500f;

		private const float holdTimeTapHold = 1000f;

		private const float transitionTimeDrag = 700f;

		private const float holdTimeDrag = 50f;

		private bool isHolding;

		public int mode;

		private ClickableTextureComponent buttonTarget;

		public HandPointer(int x, int y, int mode = 0, int destX = -1, int destY = -1, ClickableTextureComponent buttonTarget = null)
		{
			X = x;
			Y = y;
			this.destX = destX;
			this.destY = destY;
			this.mode = mode;
			this.buttonTarget = buttonTarget;
			hand = new ClickableTextureComponent(new Rectangle(-100, -100, 40, 40), Game1.mobileSpriteSheet, new Rectangle(88, 100, 15, 16), 4f, drawShadow: true);
			switch (mode)
			{
			case 0:
				handSprite = new tweeningSprite(null, hand, new Vector2(X - 100, Y + 100), new Vector2(X, Y), 500f, isInGameworld: false, 4f, buttonTarget);
				break;
			case 1:
			{
				int num = X * 64 - Game1.viewport.X + 32;
				int num2 = Y * 64 - Game1.viewport.Y + 32;
				handSprite = new tweeningSprite(null, hand, new Vector2(num + 32, num2 + ((num2 < Game1.viewport.Height - 128) ? 32 : (-32))), new Vector2(num, num2), 500f, isInGameworld: true);
				break;
			}
			case 3:
			{
				int num = X * 64 - Game1.viewport.X + 32;
				int num2 = Y * 64 - Game1.viewport.Y + 32;
				handSprite = new tweeningSprite(null, hand, new Vector2(num + 32, num2 + ((num2 < Game1.viewport.Height - 128) ? 32 : (-32))), new Vector2(num, num2), 500f, isInGameworld: true);
				break;
			}
			case 2:
				handSprite = new tweeningSprite(null, hand, new Vector2(X + ((X + 32 < Game1.viewport.Width - 128) ? 32 : (-32)), Y + 32), new Vector2(X, Y), 500f, isInGameworld: false, 4f, buttonTarget);
				break;
			case 4:
				handSprite = new tweeningSprite(null, hand, new Vector2(X, Y), new Vector2(destX, destY), 700f, isInGameworld: false, 4f, buttonTarget);
				break;
			}
			isHolding = false;
		}

		public void start()
		{
			isHolding = false;
			if (handSprite != null)
			{
				handSprite.start();
			}
		}

		public void update(GameTime time)
		{
			float num = 1f / Game1.options.desiredUIScale * Game1.options.zoomLevel;
			if (mode == 1 || mode == 3)
			{
				float num2 = X * 64 - Game1.viewport.X + 32;
				float num3 = Y * 64 - Game1.viewport.Y + 32;
				if (isHolding)
				{
					handSprite.resetVector(new Vector2(num2, num3) * num, new Vector2(num2, num3) * num);
				}
				else
				{
					handSprite.resetVector(new Vector2(num2 - 32f, num3 + 32f) * num, new Vector2(num2, num3) * num);
				}
			}
			if (handSprite.tweening)
			{
				handSprite.update(time);
				return;
			}
			if (isHolding)
			{
				isHolding = false;
				switch (mode)
				{
				case 0:
					if (buttonTarget == null)
					{
						handSprite.setUp(new Vector2(X - 32, Y + 32), new Vector2(X, Y), 500f);
					}
					else
					{
						handSprite.setUp(buttonTarget, 500f);
					}
					break;
				case 2:
					if (buttonTarget == null)
					{
						handSprite.setUp(new Vector2(X + ((X + 32 < Game1.viewport.Width - 128) ? 32 : (-32)), Y + 32), new Vector2(X, Y), 500f);
					}
					else
					{
						handSprite.setUp(buttonTarget, 500f);
					}
					break;
				case 4:
					handSprite.setUp(new Vector2(X, Y), new Vector2(destX, destY), 700f);
					break;
				case 1:
				{
					int num4 = X * 64 - Game1.viewport.X + 32;
					int num5 = Y * 64 - Game1.viewport.Y + 32;
					handSprite.setUp(new Vector2(num4 + 32, num5 + ((num5 < Game1.viewport.Height - 128) ? 32 : (-32))) * num, new Vector2(num4, num5) * num, 500f);
					break;
				}
				case 3:
				{
					int num4 = X * 64 - Game1.viewport.X + 32;
					int num5 = Y * 64 - Game1.viewport.Y + 32;
					handSprite.setUp(new Vector2(num4 + 32, num5 + ((num5 < Game1.viewport.Height - 128) ? 32 : (-32))) * num, new Vector2(num4, num5) * num, 500f);
					break;
				}
				}
				handSprite.start();
				return;
			}
			isHolding = true;
			switch (mode)
			{
			case 0:
				if (buttonTarget == null)
				{
					handSprite.setUp(new Vector2(X, Y), new Vector2(X, Y), 500f);
				}
				else
				{
					handSprite.setUp(buttonTarget, 500f, hold: true);
				}
				break;
			case 2:
				if (buttonTarget == null)
				{
					handSprite.setUp(new Vector2(X, Y), new Vector2(X, Y), 1000f);
				}
				else
				{
					handSprite.setUp(buttonTarget, 1000f, hold: true);
				}
				break;
			case 4:
				handSprite.setUp(new Vector2(destX, destY), new Vector2(destX, destY), 50f);
				break;
			case 1:
			{
				int num6 = X * 64 - Game1.viewport.X + 32;
				int num7 = Y * 64 - Game1.viewport.Y + 32;
				handSprite.setUp(new Vector2(num6, num7) * num, new Vector2(num6, num7) * num, 500f);
				break;
			}
			case 3:
			{
				int num6 = X * 64 - Game1.viewport.X + 32;
				int num7 = Y * 64 - Game1.viewport.Y + 32;
				handSprite.setUp(new Vector2(num6, num7) * num, new Vector2(num6, num7) * num, 1000f);
				break;
			}
			}
			handSprite.start();
		}

		public void resetVector(Vector2 startPosition, Vector2 endPosition)
		{
			handSprite.resetVector(startPosition, endPosition);
		}

		public void draw(SpriteBatch b)
		{
			handSprite.draw(b);
		}
	}
}
