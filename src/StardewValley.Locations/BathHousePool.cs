using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Locations
{
	public class BathHousePool : GameLocation
	{
		public const float steamZoom = 4f;

		public const float steamYMotionPerMillisecond = 0.1f;

		public const float millisecondsPerSteamFrame = 50f;

		private Texture2D steamAnimation;

		private Texture2D swimShadow;

		private Vector2 steamPosition;

		private float steamYOffset;

		private int swimShadowTimer;

		private int swimShadowFrame;

		private int _counter;

		public BathHousePool()
		{
		}

		public BathHousePool(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			Game1.changeMusicTrack("pool_ambient");
			steamPosition = new Vector2(-Game1.viewport.X, -Game1.viewport.Y);
			steamAnimation = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\steamAnimation");
			swimShadow = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\swimShadow");
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			if (Game1.player.swimming.Value)
			{
				Game1.player.swimming.Value = false;
			}
			if (Game1.locationRequest != null && !Game1.locationRequest.Name.Contains("BathHouse"))
			{
				Game1.player.bathingClothes.Value = false;
			}
			Game1.changeMusicTrack("none");
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (currentEvent != null)
			{
				foreach (NPC actor in currentEvent.actors)
				{
					if ((bool)actor.swimming)
					{
						b.Draw(swimShadow, Game1.GlobalToLocal(Game1.viewport, actor.Position + new Vector2(0f, actor.Sprite.SpriteHeight / 3 * 4 + 4)), new Rectangle(swimShadowFrame * 16, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
					}
				}
			}
			else
			{
				foreach (NPC character in characters)
				{
					if ((bool)character.swimming)
					{
						b.Draw(swimShadow, Game1.GlobalToLocal(Game1.viewport, character.Position + new Vector2(0f, character.Sprite.SpriteHeight / 3 * 4 + 4)), new Rectangle(swimShadowFrame * 16, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
					}
				}
				foreach (Farmer farmer in farmers)
				{
					if ((bool)farmer.swimming)
					{
						b.Draw(swimShadow, Game1.GlobalToLocal(Game1.viewport, farmer.Position + new Vector2(0f, farmer.Sprite.SpriteHeight / 4 * 4)), new Rectangle(swimShadowFrame * 16, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
					}
				}
			}
			_ = (bool)Game1.player.swimming;
		}

		public override void checkForMusic(GameTime time)
		{
			base.checkForMusic(time);
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			int num = 256;
			int num2 = (int)Math.Ceiling(Game1.viewport.Width / num) + 2;
			int num3 = (int)Math.Ceiling(Game1.viewport.Height / num) + 2;
			int num4 = Game1.viewport.X % num;
			int num5 = Game1.viewport.Y % num;
			Rectangle value = new Rectangle(0, 0, 64, 64);
			for (int i = -1; i < num2; i++)
			{
				for (int j = -1; j <= num3; j++)
				{
					b.Draw(position: new Vector2((float)(i * num) - (float)num4, (float)(j * num) - (float)num5 - (float)_counter), texture: steamAnimation, sourceRectangle: value, color: Color.White * 0.8f, rotation: 0f, origin: Vector2.Zero, scale: 4f, effects: SpriteEffects.None, layerDepth: 1f);
				}
			}
			_counter++;
			if (_counter >= num)
			{
				_counter = 0;
			}
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			steamYOffset -= (float)time.ElapsedGameTime.Milliseconds * 0.1f;
			steamYOffset %= -256f;
			steamPosition -= Game1.getMostRecentViewportMotion();
			swimShadowTimer -= time.ElapsedGameTime.Milliseconds;
			if (swimShadowTimer <= 0)
			{
				swimShadowTimer = 70;
				swimShadowFrame++;
				swimShadowFrame %= 10;
			}
		}
	}
}
