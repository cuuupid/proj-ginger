using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;

namespace StardewValley.Buildings
{
	public class JumpingFish
	{
		public Vector2 startPosition;

		public Vector2 endPosition;

		protected float _age;

		public float jumpTime = 1f;

		protected FishPond _pond;

		protected Object _fishObject;

		protected bool _flipped;

		public Vector2 position;

		public float jumpHeight;

		public float angularVelocity;

		public float angle;

		public JumpingFish(FishPond pond, Vector2 start_position, Vector2 end_position)
		{
			angularVelocity = Utility.RandomFloat(20f, 40f) * (float)Math.PI / 180f;
			startPosition = start_position;
			endPosition = end_position;
			position = startPosition;
			_pond = pond;
			_fishObject = pond.GetFishObject();
			if (startPosition.X > endPosition.X)
			{
				_flipped = true;
			}
			jumpHeight = Utility.RandomFloat(75f, 100f);
			Splash();
		}

		public void Splash()
		{
			if (_pond != null && Game1.currentLocation is BuildableGameLocation && (Game1.currentLocation as BuildableGameLocation).buildings.Contains(_pond))
			{
				Game1.playSound("dropItemInWater");
				Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(28, 100f, 2, 1, position + new Vector2(-0.5f, -0.5f) * 64f, flicker: false, flipped: false)
				{
					delayBeforeAnimationStart = 0,
					layerDepth = startPosition.Y / 10000f
				});
			}
		}

		public bool Update(float time)
		{
			_age += time;
			angle += angularVelocity * time;
			if (_age >= jumpTime)
			{
				_age = time;
				Splash();
				return true;
			}
			position.X = Utility.Lerp(startPosition.X, endPosition.X, _age / jumpTime);
			position.Y = Utility.Lerp(startPosition.Y, endPosition.Y, _age / jumpTime);
			return false;
		}

		public void Draw(SpriteBatch b)
		{
			float num = angle;
			SpriteEffects effects = SpriteEffects.None;
			if (_flipped)
			{
				effects = SpriteEffects.FlipHorizontally;
				num *= -1f;
			}
			float num2 = 1f;
			Vector2 globalPosition = position + new Vector2(0f, (float)Math.Sin((double)(_age / jumpTime) * Math.PI) * (0f - jumpHeight));
			b.Draw(origin: new Vector2(8f, 8f), texture: Game1.objectSpriteSheet, position: Game1.GlobalToLocal(Game1.viewport, globalPosition), sourceRectangle: Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, _fishObject.ParentSheetIndex, 16, 16), color: Color.White, rotation: num, scale: 4f * num2, effects: effects, layerDepth: position.Y / 10000f + 1E-06f);
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, position), Game1.shadowTexture.Bounds, Color.White * 0.5f, 0f, new Vector2(Game1.shadowTexture.Bounds.Width / 2, Game1.shadowTexture.Bounds.Height / 2), 2f, effects, position.Y / 10000f + 1E-06f);
		}
	}
}
