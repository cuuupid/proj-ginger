using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TinyTween;

namespace StardewValley.Menus
{
	public class tweeningSprite
	{
		private Tween<Vector2> posTween;

		public bool tweening;

		public bool isInGameworld;

		private ClickableTextureComponent sprite;

		private Vector2 startPosition;

		private Vector2 endPosition;

		private float duration;

		private float scale;

		private Item item;

		private int worldStartX;

		private int worldStartY;

		private ClickableTextureComponent buttonTarget;

		private bool hold;

		public tweeningSprite(Item i, ClickableTextureComponent spriteToCopy, Vector2 startPosition, Vector2 endPosition, float durationInMilliseconds, bool isInGameworld = false, float scale = 4f, ClickableTextureComponent buttonTarget = null)
		{
			hold = false;
			this.buttonTarget = buttonTarget;
			this.scale = scale;
			this.isInGameworld = isInGameworld;
			tweening = false;
			if (spriteToCopy != null)
			{
				sprite = new ClickableTextureComponent(spriteToCopy.bounds, spriteToCopy.texture, spriteToCopy.sourceRect, scale);
			}
			if (i != null)
			{
				item = i;
			}
			if (buttonTarget == null)
			{
				setUp(startPosition, endPosition, durationInMilliseconds);
			}
			else
			{
				setUp(buttonTarget, durationInMilliseconds);
			}
		}

		public void setUp(ClickableTextureComponent buttonTarget, float durationInMilliseconds, bool hold = false)
		{
			this.hold = hold;
			endPosition = new Vector2(buttonTarget.bounds.X + buttonTarget.bounds.Width / 2, buttonTarget.bounds.Y + buttonTarget.bounds.Height / 2);
			if (hold)
			{
				startPosition = endPosition;
			}
			else
			{
				startPosition = new Vector2(buttonTarget.bounds.X + buttonTarget.bounds.Width / 2 - 64, buttonTarget.bounds.Y + buttonTarget.bounds.Height / 2 + 64);
			}
			duration = durationInMilliseconds;
			tweening = false;
			posTween = new Tween<Vector2>(Vector2.Lerp);
		}

		public void setUp(Vector2 startPosition, Vector2 endPosition, float durationInMilliseconds)
		{
			this.startPosition = startPosition;
			this.endPosition = endPosition;
			duration = durationInMilliseconds;
			tweening = false;
			posTween = new Tween<Vector2>(Vector2.Lerp);
			buttonTarget = null;
		}

		public void resetVector(Vector2 startPosition, Vector2 endPosition)
		{
			if (buttonTarget == null)
			{
				this.startPosition = startPosition;
				this.endPosition = endPosition;
				return;
			}
			this.endPosition = new Vector2(buttonTarget.bounds.X + buttonTarget.bounds.Width / 2, buttonTarget.bounds.Y + buttonTarget.bounds.Height / 2);
			if (hold)
			{
				this.startPosition = this.endPosition;
			}
			else
			{
				this.startPosition = new Vector2(buttonTarget.bounds.X + buttonTarget.bounds.Width / 2 - 64, buttonTarget.bounds.Y + buttonTarget.bounds.Height / 2 + 64);
			}
		}

		public void start()
		{
			tweening = true;
			if (posTween != null)
			{
				if (isInGameworld)
				{
					posTween.Start(new Vector2(0f, 0f), new Vector2(1f, 1f), duration, ScaleFuncs.QuadraticEaseInOut);
				}
				else
				{
					posTween.Start(startPosition, endPosition, duration, ScaleFuncs.QuadraticEaseInOut);
				}
			}
		}

		public void stop()
		{
			tweening = false;
			if (posTween != null)
			{
				posTween.Stop(StopBehavior.AsIs);
			}
		}

		public void update(GameTime t)
		{
			if (tweening && posTween != null)
			{
				posTween.Update(t.ElapsedGameTime.Milliseconds);
				if (!isInGameworld && sprite != null)
				{
					sprite.bounds.X = (int)posTween.CurrentValue.X;
					sprite.bounds.Y = (int)posTween.CurrentValue.Y;
				}
				else if (sprite != null)
				{
					sprite.bounds.X = (int)(startPosition.X + posTween.CurrentValue.X * (endPosition.X - startPosition.X));
					sprite.bounds.Y = (int)(startPosition.Y + posTween.CurrentValue.Y * (endPosition.Y - startPosition.Y));
				}
			}
			if (posTween != null && posTween.State != 0)
			{
				tweening = false;
			}
		}

		public void draw(SpriteBatch b)
		{
			if (sprite != null)
			{
				if (isInGameworld)
				{
					sprite.draw(b, Color.White, 0.008f);
				}
				else
				{
					if (buttonTarget != null)
					{
						resetVector(startPosition, endPosition);
					}
					sprite.draw(b);
				}
			}
			if (item != null)
			{
				item.drawInMenu(b, posTween.CurrentValue, 1f);
			}
		}
	}
}
