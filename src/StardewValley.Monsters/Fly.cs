using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Monsters
{
	public class Fly : Monster
	{
		public const float rotationIncrement = (float)Math.PI / 64f;

		public const int volumeTileRange = 16;

		public const int spawnTime = 1000;

		private int spawningCounter = 1000;

		private int wasHitCounter;

		private float targetRotation;

		public static ICue buzz;

		private bool turningRight;

		public bool hard;

		public Fly()
		{
		}

		public Fly(Vector2 position)
			: this(position, hard: false)
		{
		}

		public Fly(Vector2 position, bool hard)
			: base("Fly", position)
		{
			base.Slipperiness = 24 + Game1.random.Next(-10, 10);
			Halt();
			base.IsWalkingTowardPlayer = false;
			this.hard = hard;
			if (hard)
			{
				base.DamageToFarmer *= 2;
				base.MaxHealth *= 3;
				base.Health = base.MaxHealth;
			}
			base.HideShadow = true;
		}

		public void setHard()
		{
			hard = true;
			if (hard)
			{
				base.DamageToFarmer = 12;
				base.MaxHealth = 66;
				base.Health = base.MaxHealth;
			}
		}

		public override void reloadSprite()
		{
			Sprite = new AnimatedSprite("Characters\\Monsters\\Fly");
			base.HideShadow = true;
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int num = Math.Max(1, damage - (int)resilience);
			if (Game1.random.NextDouble() < (double)missChance - (double)missChance * addedPrecision)
			{
				num = -1;
			}
			else
			{
				base.Health -= num;
				setTrajectory(xTrajectory / 3, yTrajectory / 3);
				wasHitCounter = 500;
				if (base.currentLocation != null)
				{
					base.currentLocation.playSound("hitEnemy");
				}
				if (base.Health <= 0 && base.currentLocation != null)
				{
					base.currentLocation.playSound("monsterdead");
					Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.HotPink, 10)
					{
						interval = 70f
					}, base.currentLocation);
				}
			}
			base.addedSpeed = Game1.random.Next(-1, 1);
			return num;
		}

		public override void drawAboveAllLayers(SpriteBatch b)
		{
			if (Utility.isOnScreen(base.Position, 128))
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, GetBoundingBox().Height / 2 - 32), Sprite.SourceRect, hard ? Color.Lime : Color.White, rotation, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)(getStandingY() + 8) / 10000f)));
				b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, GetBoundingBox().Height / 2), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)(getStandingY() - 1) / 10000f);
				if (isGlowing)
				{
					b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, GetBoundingBox().Height / 2 - 32), Sprite.SourceRect, glowingColor * glowingTransparency, rotation, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.99f : ((float)getStandingY() / 10000f + 0.001f)));
				}
			}
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			if (base.currentLocation != null && (bool)base.currentLocation.treatAsOutdoors)
			{
				drawAboveAllLayers(b);
			}
		}

		protected override void updateAnimation(GameTime time)
		{
			if (wasHitCounter >= 0)
			{
				wasHitCounter -= time.ElapsedGameTime.Milliseconds;
			}
			Sprite.Animate(time, (FacingDirection == 0) ? 8 : ((FacingDirection != 2) ? (FacingDirection * 4) : 0), 4, 75f);
			if (spawningCounter >= 0)
			{
				spawningCounter -= time.ElapsedGameTime.Milliseconds;
				base.Scale = 1f - (float)spawningCounter / 1000f;
			}
			else if ((withinPlayerThreshold() || Utility.isOnScreen(position, 256)) && invincibleCountdown <= 0)
			{
				faceDirection(0);
				float num = -(base.Player.GetBoundingBox().Center.X - GetBoundingBox().Center.X);
				float num2 = base.Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y;
				float num3 = Math.Max(1f, Math.Abs(num) + Math.Abs(num2));
				if (num3 < 64f)
				{
					xVelocity = Math.Max(-7f, Math.Min(7f, xVelocity * 1.1f));
					yVelocity = Math.Max(-7f, Math.Min(7f, yVelocity * 1.1f));
				}
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
					wasHitCounter = 5 + Game1.random.Next(-1, 2);
				}
				float num4 = Math.Min(7f, Math.Max(2f, 7f - num3 / 64f / 2f));
				num = (float)Math.Cos((double)rotation + Math.PI / 2.0);
				num2 = 0f - (float)Math.Sin((double)rotation + Math.PI / 2.0);
				xVelocity += (0f - num) * num4 / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				yVelocity += (0f - num2) * num4 / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				if (Math.Abs(xVelocity) > Math.Abs((0f - num) * 7f))
				{
					xVelocity -= (0f - num) * num4 / 6f;
				}
				if (Math.Abs(yVelocity) > Math.Abs((0f - num2) * 7f))
				{
					yVelocity -= (0f - num2) * num4 / 6f;
				}
			}
			resetAnimationSpeed();
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (double.IsNaN(xVelocity) || double.IsNaN(yVelocity))
			{
				base.Health = -500;
			}
			if (base.Position.X <= -640f || base.Position.Y <= -640f || base.Position.X >= (float)(base.currentLocation.Map.Layers[0].LayerWidth * 64 + 640) || base.Position.Y >= (float)(base.currentLocation.Map.Layers[0].LayerHeight * 64 + 640))
			{
				base.Health = -500;
			}
		}

		public override void Removed()
		{
			base.Removed();
		}
	}
}
