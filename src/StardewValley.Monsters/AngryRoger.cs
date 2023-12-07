using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using xTile.Dimensions;

namespace StardewValley.Monsters
{
	public class AngryRoger : Monster
	{
		public const float rotationIncrement = (float)Math.PI / 64f;

		private int wasHitCounter;

		private float targetRotation;

		private bool turningRight;

		private bool seenPlayer;

		private int identifier = Game1.random.Next(-99999, 99999);

		private new int yOffset;

		private int yOffsetExtra;

		public AngryRoger()
		{
		}

		public AngryRoger(Vector2 position)
			: base("Ghost", position)
		{
			base.Slipperiness = 8;
			isGlider.Value = true;
			base.HideShadow = true;
		}

		public AngryRoger(Vector2 position, string name)
			: base(name, position)
		{
			base.Slipperiness = 8;
			isGlider.Value = true;
			base.HideShadow = true;
		}

		public override void reloadSprite()
		{
			Sprite = new AnimatedSprite("Characters\\Monsters\\" + name);
		}

		public override void drawAboveAllLayers(SpriteBatch b)
		{
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, 21 + yOffset), Sprite.SourceRect, Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f)));
			b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + (float)yOffset / 20f, SpriteEffects.None, (float)(getStandingY() - 1) / 10000f);
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int num = Math.Max(1, damage - (int)resilience);
			base.Slipperiness = 8;
			Utility.addSprinklesToLocation(base.currentLocation, getTileX(), getTileY(), 2, 2, 101, 50, Color.LightBlue);
			if (Game1.random.NextDouble() < (double)missChance - (double)missChance * addedPrecision)
			{
				num = -1;
			}
			else
			{
				if (who.CurrentTool != null && who.CurrentTool.Name.Equals("Holy Sword") && !isBomb)
				{
					base.Health -= damage * 3 / 4;
					base.currentLocation.debris.Add(new Debris((damage * 3 / 4).ToString() ?? "", 1, new Vector2(getStandingX(), getStandingY()), Color.LightBlue, 1f, 0f));
				}
				base.Health -= num;
				if (base.Health <= 0)
				{
					deathAnimation();
				}
				setTrajectory(xTrajectory, yTrajectory);
			}
			base.addedSpeed = -1;
			Utility.removeLightSource(identifier);
			return num;
		}

		protected override void localDeathAnimation()
		{
			base.currentLocation.localSound("ghost");
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(Sprite.textureName, new Microsoft.Xna.Framework.Rectangle(0, 96, 16, 24), 100f, 4, 0, base.Position, flicker: false, flipped: false, 0.9f, 0.001f, Color.White, 4f, 0.01f, 0f, (float)Math.PI / 64f));
		}

		protected override void sharedDeathAnimation()
		{
		}

		protected override void updateAnimation(GameTime time)
		{
			yOffset = (int)(Math.Sin((double)((float)time.TotalGameTime.Milliseconds / 1000f) * (Math.PI * 2.0)) * 20.0) - yOffsetExtra;
			if (base.currentLocation == Game1.currentLocation)
			{
				bool flag = false;
				foreach (LightSource currentLightSource in Game1.currentLightSources)
				{
					if ((int)currentLightSource.identifier == identifier)
					{
						currentLightSource.position.Value = new Vector2(base.Position.X + 32f, base.Position.Y + 64f + (float)yOffset);
						flag = true;
					}
				}
				if (!flag)
				{
					Game1.currentLightSources.Add(new LightSource(5, new Vector2(base.Position.X + 8f, base.Position.Y + 64f), 1f, Color.White * 0.7f, identifier, LightSource.LightContext.None, 0L));
				}
			}
			float num = -(base.Player.GetBoundingBox().Center.X - GetBoundingBox().Center.X);
			float num2 = base.Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y;
			float num3 = 400f;
			num /= num3;
			num2 /= num3;
			if (wasHitCounter <= 0)
			{
				targetRotation = (float)Math.Atan2(0f - num2, num) - (float)Math.PI / 2f;
				if ((double)(Math.Abs(targetRotation) - Math.Abs(rotation)) > Math.PI * 7.0 / 8.0 && Game1.random.NextDouble() < 0.5)
				{
					turningRight = true;
				}
				else if ((double)(Math.Abs(targetRotation) - Math.Abs(rotation)) < Math.PI / 8.0)
				{
					turningRight = false;
				}
				if (turningRight)
				{
					rotation -= (float)Math.Sign(targetRotation - rotation) * ((float)Math.PI / 64f);
				}
				else
				{
					rotation += (float)Math.Sign(targetRotation - rotation) * ((float)Math.PI / 64f);
				}
				rotation %= (float)Math.PI * 2f;
				wasHitCounter = 0;
			}
			float num4 = Math.Min(4f, Math.Max(1f, 5f - num3 / 64f / 2f));
			num = (float)Math.Cos((double)rotation + Math.PI / 2.0);
			num2 = 0f - (float)Math.Sin((double)rotation + Math.PI / 2.0);
			xVelocity += (0f - num) * num4 / 6f + (float)Game1.random.Next(-10, 10) / 100f;
			yVelocity += (0f - num2) * num4 / 6f + (float)Game1.random.Next(-10, 10) / 100f;
			if (Math.Abs(xVelocity) > Math.Abs((0f - num) * 5f))
			{
				xVelocity -= (0f - num) * num4 / 6f;
			}
			if (Math.Abs(yVelocity) > Math.Abs((0f - num2) * 5f))
			{
				yVelocity -= (0f - num2) * num4 / 6f;
			}
			faceGeneralDirection(base.Player.getStandingPosition());
			resetAnimationSpeed();
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (!GetBoundingBox().Intersects(base.Player.GetBoundingBox()) || !base.Player.temporarilyInvincible)
			{
				return;
			}
			int i = 0;
			Vector2 vector = new Vector2(base.Player.GetBoundingBox().Center.X / 64 + Game1.random.Next(-12, 12), base.Player.GetBoundingBox().Center.Y / 64 + Game1.random.Next(-12, 12));
			for (; i < 3; i++)
			{
				if (!(vector.X >= (float)base.currentLocation.map.GetLayer("Back").LayerWidth) && !(vector.Y >= (float)base.currentLocation.map.GetLayer("Back").LayerHeight) && !(vector.X < 0f) && !(vector.Y < 0f) && base.currentLocation.map.GetLayer("Back").Tiles[(int)vector.X, (int)vector.Y] != null && base.currentLocation.isTilePassable(new Location((int)vector.X, (int)vector.Y), Game1.viewport) && !vector.Equals(new Vector2(base.Player.getStandingX() / 64, base.Player.getStandingY() / 64)))
				{
					break;
				}
				vector = new Vector2(base.Player.GetBoundingBox().Center.X / 64 + Game1.random.Next(-12, 12), base.Player.GetBoundingBox().Center.Y / 64 + Game1.random.Next(-12, 12));
			}
			if (i < 3)
			{
				base.Position = new Vector2(vector.X * 64f, vector.Y * 64f - 32f);
				Halt();
			}
		}
	}
}
