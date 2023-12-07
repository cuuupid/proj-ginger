using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley.Monsters
{
	public class Serpent : Monster
	{
		public const float rotationIncrement = (float)Math.PI / 64f;

		private int wasHitCounter;

		private float targetRotation;

		private bool turningRight;

		private readonly NetFarmerRef killer = new NetFarmerRef().Delayed(interpolationWait: false);

		public List<Vector3> segments = new List<Vector3>();

		public NetInt segmentCount = new NetInt(0);

		public Serpent()
		{
		}

		public Serpent(Vector2 position)
			: base("Serpent", position)
		{
			InitializeAttributes();
		}

		public Serpent(Vector2 position, string name)
			: base(name, position)
		{
			InitializeAttributes();
			if (name == "Royal Serpent")
			{
				segmentCount.Value = Game1.random.Next(3, 7);
				if (Game1.random.NextDouble() < 0.1)
				{
					segmentCount.Value = Game1.random.Next(5, 10);
				}
				else if (Game1.random.NextDouble() < 0.01)
				{
					segmentCount.Value *= 3;
				}
				reloadSprite();
				base.MaxHealth += segmentCount.Value * 50;
				base.Health = base.MaxHealth;
			}
		}

		public virtual void InitializeAttributes()
		{
			base.Slipperiness = 24 + Game1.random.Next(10);
			Halt();
			base.IsWalkingTowardPlayer = false;
			Sprite.SpriteWidth = 32;
			Sprite.SpriteHeight = 32;
			base.Scale = 0.75f;
			base.HideShadow = true;
		}

		public bool IsRoyalSerpent()
		{
			return segmentCount.Value > 1;
		}

		public override bool TakesDamageFromHitbox(Rectangle area_of_effect)
		{
			if (base.TakesDamageFromHitbox(area_of_effect))
			{
				return true;
			}
			if (IsRoyalSerpent())
			{
				Rectangle boundingBox = GetBoundingBox();
				Vector2 vector = new Vector2((float)boundingBox.X - base.Position.X, (float)boundingBox.Y - base.Position.Y);
				foreach (Vector3 segment in segments)
				{
					boundingBox.X = (int)(segment.X + vector.X);
					boundingBox.Y = (int)(segment.Y + vector.Y);
					if (boundingBox.Intersects(area_of_effect))
					{
						return true;
					}
				}
			}
			return false;
		}

		public override bool OverlapsFarmerForDamage(Farmer who)
		{
			if (base.OverlapsFarmerForDamage(who))
			{
				return true;
			}
			if (IsRoyalSerpent())
			{
				Rectangle boundingBox = who.GetBoundingBox();
				Rectangle boundingBox2 = GetBoundingBox();
				Vector2 vector = new Vector2((float)boundingBox2.X - base.Position.X, (float)boundingBox2.Y - base.Position.Y);
				foreach (Vector3 segment in segments)
				{
					boundingBox2.X = (int)(segment.X + vector.X);
					boundingBox2.Y = (int)(segment.Y + vector.Y);
					if (boundingBox2.Intersects(boundingBox))
					{
						return true;
					}
				}
			}
			return false;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(killer.NetFields, segmentCount);
			segmentCount.fieldChangeVisibleEvent += delegate(NetInt field, int old_value, int new_value)
			{
				if (new_value > 0)
				{
					reloadSprite();
				}
			};
		}

		public override void reloadSprite()
		{
			if (IsRoyalSerpent())
			{
				Sprite = new AnimatedSprite("Characters\\Monsters\\Royal Serpent");
				base.Scale = 1f;
			}
			else
			{
				Sprite = new AnimatedSprite("Characters\\Monsters\\Serpent");
				base.Scale = 0.75f;
			}
			Sprite.SpriteWidth = 32;
			Sprite.SpriteHeight = 32;
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
				base.currentLocation.playSound("serpentHit");
				if (base.Health <= 0)
				{
					killer.Value = who;
					deathAnimation();
				}
			}
			base.addedSpeed = Game1.random.Next(-1, 1);
			return num;
		}

		protected override void sharedDeathAnimation()
		{
		}

		protected override void localDeathAnimation()
		{
			if (killer.Value == null)
			{
				return;
			}
			Rectangle boundingBox = GetBoundingBox();
			boundingBox.Inflate(-boundingBox.Width / 2 + 1, -boundingBox.Height / 2 + 1);
			Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(boundingBox.Center, 4f, killer.Value);
			int num = -(int)velocityTowardPlayer.X;
			int num2 = -(int)velocityTowardPlayer.Y;
			if (IsRoyalSerpent())
			{
				base.currentLocation.localSound("serpentDie");
				for (int i = -1; i < segments.Count; i++)
				{
					Vector2 zero = Vector2.Zero;
					Rectangle rectangle = new Rectangle(0, 0, 32, 32);
					float z = rotation;
					float t = 0f;
					if (i == -1)
					{
						zero = base.Position;
						rectangle = new Rectangle(0, 64, 32, 32);
					}
					else
					{
						if (segments.Count <= 0 || i >= segments.Count)
						{
							break;
						}
						t = (float)(i + 1) / (float)segments.Count;
						zero = new Vector2(segments[i].X, segments[i].Y);
						boundingBox.X = (int)(zero.X - (float)(boundingBox.Width / 2));
						boundingBox.Y = (int)(zero.Y - (float)(boundingBox.Height / 2));
						rectangle = new Rectangle(32, 64, 32, 32);
						if (i == segments.Count - 1)
						{
							rectangle = new Rectangle(64, 64, 32, 32);
						}
						z = segments[i].Z;
					}
					Color color = default(Color);
					color.R = (byte)Utility.Lerp(255f, 255f, t);
					color.G = (byte)Utility.Lerp(0f, 166f, t);
					color.B = (byte)Utility.Lerp(0f, 0f, t);
					color.A = 255;
					TemporaryAnimatedSprite temporaryAnimatedSprite = null;
					temporaryAnimatedSprite = new TemporaryAnimatedSprite(Sprite.textureName, rectangle, 800f, 1, 0, zero, flicker: false, flipped: false, 0.9f, 0.001f, color, 4f * (float)scale, 0.01f, z + (float)Math.PI, (float)((double)Game1.random.Next(3, 5) * Math.PI / 64.0))
					{
						motion = new Vector2(num, num2),
						layerDepth = 1f
					};
					temporaryAnimatedSprite.alphaFade = 0.025f;
					base.currentLocation.temporarySprites.Add(temporaryAnimatedSprite);
					temporaryAnimatedSprite = new TemporaryAnimatedSprite(5, Utility.PointToVector2(boundingBox.Center) + new Vector2(-32f, 0f), Color.LightGreen * 0.9f, 10, flipped: false, 70f)
					{
						delayBeforeAnimationStart = 50,
						motion = new Vector2(num, num2),
						layerDepth = 1f
					};
					if (i == -1)
					{
						temporaryAnimatedSprite.startSound = "cowboy_monsterhit";
					}
					base.currentLocation.temporarySprites.Add(temporaryAnimatedSprite);
					temporaryAnimatedSprite = new TemporaryAnimatedSprite(5, Utility.PointToVector2(boundingBox.Center) + new Vector2(32f, 0f), Color.LightGreen * 0.8f, 10, flipped: false, 70f)
					{
						delayBeforeAnimationStart = 100,
						startSound = "cowboy_monsterhit",
						motion = new Vector2(num, num2) * 0.8f,
						layerDepth = 1f
					};
					if (i == -1)
					{
						temporaryAnimatedSprite.startSound = "cowboy_monsterhit";
					}
					base.currentLocation.temporarySprites.Add(temporaryAnimatedSprite);
					temporaryAnimatedSprite = new TemporaryAnimatedSprite(5, Utility.PointToVector2(boundingBox.Center) + new Vector2(0f, -32f), Color.LightGreen * 0.7f, 10)
					{
						delayBeforeAnimationStart = 150,
						startSound = "cowboy_monsterhit",
						motion = new Vector2(num, num2) * 0.6f,
						layerDepth = 1f
					};
					if (i == -1)
					{
						temporaryAnimatedSprite.startSound = "cowboy_monsterhit";
					}
					base.currentLocation.temporarySprites.Add(temporaryAnimatedSprite);
					temporaryAnimatedSprite = new TemporaryAnimatedSprite(5, Utility.PointToVector2(boundingBox.Center), Color.LightGreen * 0.6f, 10, flipped: false, 70f)
					{
						delayBeforeAnimationStart = 200,
						startSound = "cowboy_monsterhit",
						motion = new Vector2(num, num2) * 0.4f,
						layerDepth = 1f
					};
					if (i == -1)
					{
						temporaryAnimatedSprite.startSound = "cowboy_monsterhit";
					}
					base.currentLocation.temporarySprites.Add(temporaryAnimatedSprite);
					temporaryAnimatedSprite = new TemporaryAnimatedSprite(5, Utility.PointToVector2(boundingBox.Center) + new Vector2(0f, 32f), Color.LightGreen * 0.5f, 10)
					{
						delayBeforeAnimationStart = 250,
						startSound = "cowboy_monsterhit",
						motion = new Vector2(num, num2) * 0.2f,
						layerDepth = 1f
					};
					if (i == -1)
					{
						temporaryAnimatedSprite.startSound = "cowboy_monsterhit";
					}
					base.currentLocation.temporarySprites.Add(temporaryAnimatedSprite);
				}
			}
			else
			{
				base.currentLocation.localSound("serpentDie");
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(Sprite.textureName, new Rectangle(0, 64, 32, 32), 200f, 4, 0, base.Position, flicker: false, flipped: false, 0.9f, 0.001f, Color.White, 4f * (float)scale, 0.01f, rotation + (float)Math.PI, (float)((double)Game1.random.Next(3, 5) * Math.PI / 64.0))
				{
					motion = new Vector2(num, num2),
					layerDepth = 1f
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.PointToVector2(GetBoundingBox().Center) + new Vector2(-32f, 0f), Color.LightGreen * 0.9f, 10, flipped: false, 70f)
				{
					delayBeforeAnimationStart = 50,
					startSound = "cowboy_monsterhit",
					motion = new Vector2(num, num2),
					layerDepth = 1f
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.PointToVector2(GetBoundingBox().Center) + new Vector2(32f, 0f), Color.LightGreen * 0.8f, 10, flipped: false, 70f)
				{
					delayBeforeAnimationStart = 100,
					startSound = "cowboy_monsterhit",
					motion = new Vector2(num, num2) * 0.8f,
					layerDepth = 1f
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.PointToVector2(GetBoundingBox().Center) + new Vector2(0f, -32f), Color.LightGreen * 0.7f, 10)
				{
					delayBeforeAnimationStart = 150,
					startSound = "cowboy_monsterhit",
					motion = new Vector2(num, num2) * 0.6f,
					layerDepth = 1f
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.PointToVector2(GetBoundingBox().Center), Color.LightGreen * 0.6f, 10, flipped: false, 70f)
				{
					delayBeforeAnimationStart = 200,
					startSound = "cowboy_monsterhit",
					motion = new Vector2(num, num2) * 0.4f,
					layerDepth = 1f
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.PointToVector2(GetBoundingBox().Center) + new Vector2(0f, 32f), Color.LightGreen * 0.5f, 10)
				{
					delayBeforeAnimationStart = 250,
					startSound = "cowboy_monsterhit",
					motion = new Vector2(num, num2) * 0.2f,
					layerDepth = 1f
				});
			}
		}

		public override List<Item> getExtraDropItems()
		{
			List<Item> list = new List<Item>();
			if (Game1.random.NextDouble() < 0.002)
			{
				list.Add(new Object(485, 1));
			}
			return list;
		}

		public override void drawAboveAllLayers(SpriteBatch b)
		{
			Vector2 vector = base.Position;
			bool flag = IsRoyalSerpent();
			for (int i = -1; i < segmentCount.Value; i++)
			{
				Vector2 zero = Vector2.Zero;
				float num = 0f;
				float num2 = (float)(i + 1) * -0.25f / 10000f;
				float num3 = (float)(int)segmentCount * -0.25f / 10000f - 5E-05f;
				if ((float)(getStandingY() - 1) / 10000f + num3 < 0f)
				{
					num2 += 0f - ((float)(getStandingY() - 1) / 10000f + num3);
				}
				Rectangle value = Sprite.SourceRect;
				Vector2 globalPosition = base.Position;
				if (i == -1)
				{
					if (flag)
					{
						value = new Rectangle(0, 0, 32, 32);
					}
					zero = base.Position;
					num = rotation;
				}
				else
				{
					if (i >= segments.Count)
					{
						break;
					}
					Vector3 vector2 = segments[i];
					zero = new Vector2(vector2.X, vector2.Y);
					value = new Rectangle(32, 0, 32, 32);
					if (i == segments.Count - 1)
					{
						value = new Rectangle(64, 0, 32, 32);
					}
					num = vector2.Z;
					globalPosition = (vector + zero) / 2f;
				}
				if (Utility.isOnScreen(zero, 128))
				{
					Vector2 vector3 = Game1.GlobalToLocal(Game1.viewport, zero) + drawOffset + new Vector2(0f, yJumpOffset);
					Vector2 vector4 = Game1.GlobalToLocal(Game1.viewport, globalPosition) + drawOffset + new Vector2(0f, yJumpOffset);
					b.Draw(Game1.shadowTexture, vector4 + new Vector2(64f, GetBoundingBox().Height), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)(getStandingY() - 1) / 10000f + num2);
					b.Draw(Sprite.Texture, vector3 + new Vector2(64f, GetBoundingBox().Height / 2), value, Color.White, num, new Vector2(16f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)(getStandingY() + 8) / 10000f + num2)));
					if (isGlowing)
					{
						b.Draw(Sprite.Texture, vector3 + new Vector2(64f, GetBoundingBox().Height / 2), value, glowingColor * glowingTransparency, num, new Vector2(16f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)(getStandingY() + 8) / 10000f + 0.0001f + num2)));
					}
					if (flag)
					{
						num2 += -5E-05f;
						num = 0f;
						value = new Rectangle(96, 0, 32, 32);
						vector3 = Game1.GlobalToLocal(Game1.viewport, vector) + drawOffset + new Vector2(0f, yJumpOffset);
						if (i > 0)
						{
							b.Draw(Game1.shadowTexture, vector3 + new Vector2(64f, GetBoundingBox().Height), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)(getStandingY() - 1) / 10000f + num2);
						}
						b.Draw(Sprite.Texture, vector3 + new Vector2(64f, GetBoundingBox().Height / 2), value, Color.White, num, new Vector2(16f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)(getStandingY() + 8) / 10000f + num2)));
						if (isGlowing)
						{
							b.Draw(Sprite.Texture, vector3 + new Vector2(64f, GetBoundingBox().Height / 2), value, glowingColor * glowingTransparency, num, new Vector2(16f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)(getStandingY() + 8) / 10000f + 0.0001f + num2)));
						}
					}
				}
				vector = zero;
			}
		}

		public override Rectangle GetBoundingBox()
		{
			Vector2 vector = base.Position;
			return new Rectangle((int)vector.X + 8, (int)vector.Y, Sprite.SpriteWidth * 4 * 3 / 4, 96);
		}

		protected override void updateAnimation(GameTime time)
		{
			if (IsRoyalSerpent())
			{
				if (segments.Count < segmentCount.Value)
				{
					for (int i = 0; i < segmentCount.Value; i++)
					{
						Vector2 vector = base.Position;
						segments.Add(new Vector3(vector.X, vector.Y, 0f));
					}
				}
				Vector2 vector2 = base.Position;
				for (int j = 0; j < segments.Count; j++)
				{
					Vector2 vector3 = new Vector2(segments[j].X, segments[j].Y);
					Vector2 vector4 = vector3 - vector2;
					int num = 64;
					int num2 = (int)vector4.Length();
					vector4.Normalize();
					if (num2 > num)
					{
						vector3 = vector4 * num + vector2;
					}
					double num3 = Math.Atan2(vector4.Y, vector4.X) - Math.PI / 2.0;
					segments[j] = new Vector3(vector3.X, vector3.Y, (float)num3);
					vector2 = vector3;
				}
			}
			base.updateAnimation(time);
			if (wasHitCounter >= 0)
			{
				wasHitCounter -= time.ElapsedGameTime.Milliseconds;
			}
			if (!IsRoyalSerpent())
			{
				Sprite.Animate(time, 0, 9, 40f);
			}
			if (withinPlayerThreshold() && invincibleCountdown <= 0)
			{
				float num4 = -(base.Player.GetBoundingBox().Center.X - GetBoundingBox().Center.X);
				float num5 = base.Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y;
				float num6 = Math.Max(1f, Math.Abs(num4) + Math.Abs(num5));
				if (num6 < 64f)
				{
					xVelocity = Math.Max(-7f, Math.Min(7f, xVelocity * 1.1f));
					yVelocity = Math.Max(-7f, Math.Min(7f, yVelocity * 1.1f));
				}
				num4 /= num6;
				num5 /= num6;
				if (wasHitCounter <= 0)
				{
					targetRotation = (float)Math.Atan2(0f - num5, num4) - (float)Math.PI / 2f;
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
				float num7 = Math.Min(7f, Math.Max(2f, 7f - num6 / 64f / 2f));
				num4 = (float)Math.Cos((double)rotation + Math.PI / 2.0);
				num5 = 0f - (float)Math.Sin((double)rotation + Math.PI / 2.0);
				xVelocity += (0f - num4) * num7 / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				yVelocity += (0f - num5) * num7 / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				if (Math.Abs(xVelocity) > Math.Abs((0f - num4) * 7f))
				{
					xVelocity -= (0f - num4) * num7 / 6f;
				}
				if (Math.Abs(yVelocity) > Math.Abs((0f - num5) * 7f))
				{
					yVelocity -= (0f - num5) * num7 / 6f;
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
			if (withinPlayerThreshold() && invincibleCountdown <= 0)
			{
				faceDirection(2);
			}
		}
	}
}
