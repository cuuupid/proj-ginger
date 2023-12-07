using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Objects
{
	public class TankFish
	{
		public enum FishType
		{
			Normal,
			Eel,
			Cephalopod,
			Float,
			Ground,
			Crawl,
			Static
		}

		protected FishTankFurniture _tank;

		public Vector2 position;

		public float zPosition;

		public bool facingLeft;

		public Vector2 velocity = Vector2.Zero;

		protected Texture2D _texture;

		public float nextSwim;

		public int fishIndex;

		public int currentFrame;

		public int numberOfDarts;

		public FishType fishType;

		public float minimumVelocity;

		public float fishScale = 1f;

		public List<int> currentAnimation;

		public List<int> idleAnimation;

		public List<int> dartStartAnimation;

		public List<int> dartHoldAnimation;

		public List<int> dartEndAnimation;

		public int currentAnimationFrame;

		public float currentFrameTime;

		public float nextBubble;

		public TankFish(FishTankFurniture tank, Item item)
		{
			_tank = tank;
			_texture = _tank.GetAquariumTexture();
			Dictionary<int, string> aquariumData = _tank.GetAquariumData();
			string text = aquariumData[item.ParentSheetIndex];
			string[] array = text.Split('/');
			fishIndex = int.Parse(array[0]);
			currentFrame = fishIndex;
			zPosition = Utility.RandomFloat(4f, 10f);
			Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
			fishScale = 0.75f;
			if (dictionary.ContainsKey(item.ParentSheetIndex))
			{
				string text2 = dictionary[item.ParentSheetIndex];
				string[] array2 = text2.Split('/');
				if (!(array2[1] == "trap"))
				{
					minimumVelocity = Utility.RandomFloat(0.25f, 0.35f);
					if (array2[2] == "smooth")
					{
						minimumVelocity = Utility.RandomFloat(0.5f, 0.6f);
					}
					if (array2[2] == "dart")
					{
						minimumVelocity = 0f;
					}
				}
			}
			if (array.Length > 1)
			{
				switch (array[1])
				{
				case "eel":
					fishType = FishType.Eel;
					minimumVelocity = Utility.Clamp(fishScale, 0.3f, 0.4f);
					break;
				case "cephalopod":
					fishType = FishType.Cephalopod;
					minimumVelocity = 0f;
					break;
				case "ground":
					fishType = FishType.Ground;
					zPosition = 4f;
					minimumVelocity = 0f;
					break;
				case "static":
					fishType = FishType.Static;
					break;
				case "crawl":
					fishType = FishType.Crawl;
					minimumVelocity = 0f;
					break;
				case "front_crawl":
					fishType = FishType.Crawl;
					zPosition = 3f;
					minimumVelocity = 0f;
					break;
				case "float":
					fishType = FishType.Float;
					break;
				}
			}
			if (array.Length > 2)
			{
				string text3 = array[2];
				string[] array3 = text3.Split(' ');
				idleAnimation = new List<int>();
				string[] array4 = array3;
				foreach (string s in array4)
				{
					idleAnimation.Add(int.Parse(s));
				}
				SetAnimation(idleAnimation);
			}
			if (array.Length > 3)
			{
				string text4 = array[3];
				string[] array5 = text4.Split(' ');
				dartStartAnimation = new List<int>();
				if (text4 != "")
				{
					string[] array6 = array5;
					foreach (string s2 in array6)
					{
						dartStartAnimation.Add(int.Parse(s2));
					}
				}
			}
			if (array.Length > 4)
			{
				string text5 = array[4];
				string[] array7 = text5.Split(' ');
				dartHoldAnimation = new List<int>();
				if (text5 != "")
				{
					string[] array8 = array7;
					foreach (string s3 in array8)
					{
						dartHoldAnimation.Add(int.Parse(s3));
					}
				}
			}
			if (array.Length > 5)
			{
				string text6 = array[5];
				string[] array9 = text6.Split(' ');
				dartEndAnimation = new List<int>();
				if (text6 != "")
				{
					string[] array10 = array9;
					foreach (string s4 in array10)
					{
						dartEndAnimation.Add(int.Parse(s4));
					}
				}
			}
			Rectangle tankBounds = _tank.GetTankBounds();
			tankBounds.X = 0;
			tankBounds.Y = 0;
			position = Vector2.Zero;
			position = Utility.getRandomPositionInThisRectangle(tankBounds, Game1.random);
			nextSwim = Utility.RandomFloat(0.1f, 10f);
			nextBubble = Utility.RandomFloat(0.1f, 10f);
			facingLeft = Game1.random.Next(2) == 1;
			if (facingLeft)
			{
				velocity = new Vector2(-1f, 0f);
			}
			else
			{
				velocity = new Vector2(1f, 0f);
			}
			velocity *= minimumVelocity;
			if (fishType == FishType.Ground || fishType == FishType.Crawl || fishType == FishType.Static)
			{
				position.Y = 0f;
			}
			ConstrainToTank();
		}

		public void SetAnimation(List<int> frames)
		{
			if (currentAnimation != frames)
			{
				currentAnimation = frames;
				currentAnimationFrame = 0;
				currentFrameTime = 0f;
				if (currentAnimation != null && currentAnimation.Count > 0)
				{
					currentFrame = frames[0];
				}
			}
		}

		public virtual void Draw(SpriteBatch b, float alpha, float draw_layer)
		{
			SpriteEffects effects = SpriteEffects.None;
			int num = -12;
			int num2 = 8;
			if (fishType == FishType.Eel)
			{
				num2 = 4;
			}
			int num3 = num2;
			if (facingLeft)
			{
				effects = SpriteEffects.FlipHorizontally;
				num3 *= -1;
				num = -num - num2 + 1;
			}
			float num4 = (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 1.25 + (double)(position.X / 32f)) * 2f;
			if (fishType == FishType.Crawl || fishType == FishType.Ground || fishType == FishType.Static)
			{
				num4 = 0f;
			}
			float scale = GetScale();
			int num5 = _texture.Width / 24;
			int num6 = currentFrame % num5 * 24;
			int y = currentFrame / num5 * 48;
			int num7 = 10;
			float num8 = 1f;
			if (fishType == FishType.Eel)
			{
				num7 = 20;
				num4 *= 0f;
			}
			if (fishType == FishType.Ground || fishType == FishType.Crawl || fishType == FishType.Static)
			{
				float rotation = 0f;
				b.Draw(_texture, Game1.GlobalToLocal(GetWorldPosition() + new Vector2(0f, num4) * 4f * scale), new Rectangle(num6, y, 24, 24), Color.White * alpha, rotation, new Vector2(12f, 12f), 4f * scale, effects, draw_layer);
			}
			else if (fishType == FishType.Cephalopod || fishType == FishType.Float)
			{
				float rotation2 = Utility.Clamp(velocity.X, -0.5f, 0.5f);
				b.Draw(_texture, Game1.GlobalToLocal(GetWorldPosition() + new Vector2(0f, num4) * 4f * scale), new Rectangle(num6, y, 24, 24), Color.White * alpha, rotation2, new Vector2(12f, 12f), 4f * scale, effects, draw_layer);
			}
			else
			{
				for (int i = 0; i < 24 / num2; i++)
				{
					float num9 = (float)(i * num2) / (float)num7;
					num9 = 1f - num9;
					float value = velocity.Length() / 1f;
					float num10 = 1f;
					float num11 = 0f;
					value = Utility.Clamp(value, 0.2f, 1f);
					num9 = Utility.Clamp(num9, 0f, 1f);
					if (fishType == FishType.Eel)
					{
						num9 = 1f;
						value = 1f;
						num10 = 0.1f;
						num11 = 4f;
					}
					if (facingLeft)
					{
						num11 *= -1f;
					}
					b.Draw(_texture, Game1.GlobalToLocal(GetWorldPosition() + new Vector2(num + i * num3, num4 + (float)(Math.Sin((double)(i * 20) + Game1.currentGameTime.TotalGameTime.TotalSeconds * 25.0 * (double)num10 + (double)(num11 * position.X / 16f)) * (double)num8 * (double)num9 * (double)value)) * 4f * scale), new Rectangle(num6 + i * num2, y, num2, 24), Color.White * alpha, 0f, new Vector2(0f, 12f), 4f * scale, effects, draw_layer);
				}
			}
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(new Vector2(GetWorldPosition().X, (float)_tank.GetTankBounds().Bottom - zPosition * 4f)), null, Color.White * alpha * 0.75f, 0f, new Vector2(Game1.shadowTexture.Width / 2, Game1.shadowTexture.Height / 2), new Vector2(4f * scale, 1f), SpriteEffects.None, _tank.GetFishSortRegion().X - 1E-07f);
		}

		public Vector2 GetWorldPosition()
		{
			return new Vector2((float)_tank.GetTankBounds().X + position.X, (float)_tank.GetTankBounds().Bottom - position.Y - zPosition * 4f);
		}

		public void ConstrainToTank()
		{
			Rectangle tankBounds = _tank.GetTankBounds();
			Rectangle bounds = GetBounds();
			tankBounds.X = 0;
			tankBounds.Y = 0;
			if (bounds.X < tankBounds.X)
			{
				position.X += tankBounds.X - bounds.X;
				bounds = GetBounds();
			}
			if (bounds.Y < tankBounds.Y)
			{
				position.Y -= tankBounds.Y - bounds.Y;
				bounds = GetBounds();
			}
			if (bounds.Right > tankBounds.Right)
			{
				position.X += tankBounds.Right - bounds.Right;
				bounds = GetBounds();
			}
			if (fishType == FishType.Crawl || fishType == FishType.Ground || fishType == FishType.Static)
			{
				if (position.Y > (float)tankBounds.Bottom)
				{
					position.Y -= (float)tankBounds.Bottom - position.Y;
					bounds = GetBounds();
				}
			}
			else if (bounds.Bottom > tankBounds.Bottom)
			{
				position.Y -= tankBounds.Bottom - bounds.Bottom;
				bounds = GetBounds();
			}
		}

		public virtual float GetScale()
		{
			return fishScale;
		}

		public Rectangle GetBounds()
		{
			Vector2 vector = new Vector2(24f, 18f);
			vector *= 4f * GetScale();
			if (fishType == FishType.Crawl || fishType == FishType.Ground || fishType == FishType.Static)
			{
				return new Rectangle((int)(position.X - vector.X / 2f), (int)((float)_tank.GetTankBounds().Height - position.Y - vector.Y), (int)vector.X, (int)vector.Y);
			}
			return new Rectangle((int)(position.X - vector.X / 2f), (int)((float)_tank.GetTankBounds().Height - position.Y - vector.Y / 2f), (int)vector.X, (int)vector.Y);
		}

		public virtual void Update(GameTime time)
		{
			if (currentAnimation != null && currentAnimation.Count > 0)
			{
				currentFrameTime += (float)time.ElapsedGameTime.TotalSeconds;
				float num = 0.125f;
				if (currentFrameTime > num)
				{
					currentAnimationFrame += (int)(currentFrameTime / num);
					currentFrameTime %= num;
					if (currentAnimationFrame >= currentAnimation.Count)
					{
						if (currentAnimation == idleAnimation)
						{
							currentAnimationFrame %= currentAnimation.Count;
							currentFrame = currentAnimation[currentAnimationFrame];
						}
						else if (currentAnimation == dartStartAnimation)
						{
							if (dartHoldAnimation != null)
							{
								SetAnimation(dartHoldAnimation);
							}
							else
							{
								SetAnimation(idleAnimation);
							}
						}
						else if (currentAnimation == dartHoldAnimation)
						{
							currentAnimationFrame %= currentAnimation.Count;
							currentFrame = currentAnimation[currentAnimationFrame];
						}
						else if (currentAnimation == dartEndAnimation)
						{
							SetAnimation(idleAnimation);
						}
					}
					else
					{
						currentFrame = currentAnimation[currentAnimationFrame];
					}
				}
			}
			if (fishType != FishType.Static)
			{
				Rectangle tankBounds = _tank.GetTankBounds();
				tankBounds.X = 0;
				tankBounds.Y = 0;
				float num2 = velocity.X;
				if (fishType == FishType.Crawl)
				{
					num2 = Utility.Clamp(num2, -0.5f, 0.5f);
				}
				position.X += num2;
				Rectangle bounds = GetBounds();
				if (bounds.Left < tankBounds.Left || bounds.Right > tankBounds.Right)
				{
					ConstrainToTank();
					bounds = GetBounds();
					velocity.X *= -1f;
					facingLeft = !facingLeft;
				}
				position.Y += velocity.Y;
				bounds = GetBounds();
				if (bounds.Top < tankBounds.Top || bounds.Bottom > tankBounds.Bottom)
				{
					ConstrainToTank();
					velocity.Y *= 0f;
				}
				float num3 = velocity.Length();
				if (num3 > minimumVelocity)
				{
					float t = 0.015f;
					if (fishType == FishType.Crawl || fishType == FishType.Ground)
					{
						t = 0.03f;
					}
					num3 = Utility.Lerp(num3, minimumVelocity, t);
					if (num3 < 0.0001f)
					{
						num3 = 0f;
					}
					velocity.Normalize();
					velocity *= num3;
					if (currentAnimation == dartHoldAnimation && num3 <= minimumVelocity + 0.5f)
					{
						if (dartEndAnimation != null && dartEndAnimation.Count > 0)
						{
							SetAnimation(dartEndAnimation);
						}
						else if (idleAnimation != null && idleAnimation.Count > 0)
						{
							SetAnimation(idleAnimation);
						}
					}
				}
				nextSwim -= (float)time.ElapsedGameTime.TotalSeconds;
				if (nextSwim <= 0f)
				{
					if (numberOfDarts == 0)
					{
						numberOfDarts = Game1.random.Next(1, 4);
						nextSwim = Utility.RandomFloat(6f, 12f);
						if (fishType == FishType.Cephalopod)
						{
							nextSwim = Utility.RandomFloat(2f, 5f);
						}
						if (Game1.random.NextDouble() < 0.30000001192092896)
						{
							facingLeft = !facingLeft;
						}
					}
					else
					{
						nextSwim = Utility.RandomFloat(0.1f, 0.5f);
						numberOfDarts--;
						if (Game1.random.NextDouble() < 0.05000000074505806)
						{
							facingLeft = !facingLeft;
						}
					}
					if (dartStartAnimation != null && dartStartAnimation.Count > 0)
					{
						SetAnimation(dartStartAnimation);
					}
					else if (dartHoldAnimation != null && dartHoldAnimation.Count > 0)
					{
						SetAnimation(dartHoldAnimation);
					}
					velocity.X = 1.5f;
					if (_tank.getTilesWide() <= 2)
					{
						velocity.X *= 0.5f;
					}
					if (facingLeft)
					{
						velocity.X *= -1f;
					}
					if (fishType == FishType.Cephalopod)
					{
						velocity.Y = Utility.RandomFloat(0.5f, 0.75f);
					}
					else if (fishType == FishType.Ground)
					{
						velocity.X *= 0.5f;
						velocity.Y = Utility.RandomFloat(0.5f, 0.25f);
					}
					else
					{
						velocity.Y = Utility.RandomFloat(-0.5f, 0.5f);
					}
					if (fishType == FishType.Crawl)
					{
						velocity.Y = 0f;
					}
				}
			}
			if (fishType == FishType.Cephalopod || fishType == FishType.Ground || fishType == FishType.Crawl || fishType == FishType.Static)
			{
				float num4 = 0.2f;
				if (fishType == FishType.Static)
				{
					num4 = 0.6f;
				}
				if (position.Y > 0f)
				{
					position.Y -= num4;
				}
			}
			nextBubble -= (float)time.ElapsedGameTime.TotalSeconds;
			if (nextBubble <= 0f)
			{
				nextBubble = Utility.RandomFloat(1f, 10f);
				float num5 = 0f;
				if (fishType == FishType.Ground || fishType == FishType.Normal || fishType == FishType.Eel)
				{
					num5 = 32f;
				}
				if (facingLeft)
				{
					num5 *= -1f;
				}
				num5 *= fishScale;
				_tank.bubbles.Add(new Vector4(position.X + num5, position.Y + zPosition, zPosition, 0.25f));
			}
			ConstrainToTank();
		}
	}
}
