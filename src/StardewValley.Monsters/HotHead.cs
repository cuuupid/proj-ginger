using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley.Monsters
{
	public class HotHead : MetalHead
	{
		[XmlIgnore]
		public NetFarmerRef lastAttacker = new NetFarmerRef();

		[XmlIgnore]
		public NetFloat timeUntilExplode = new NetFloat(-1f);

		[XmlIgnore]
		public NetBool angry = new NetBool();

		public HotHead()
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(lastAttacker.NetFields, angry, timeUntilExplode);
		}

		public HotHead(Vector2 position)
			: base("Hot Head", position)
		{
			base.Slipperiness *= 2;
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			lastAttacker.Value = who;
			int result = base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
			if ((float)timeUntilExplode == -1f && base.Health < 25)
			{
				base.currentLocation.netAudio.StartPlaying("fuse");
				timeUntilExplode.Value = 2.4f;
				base.Speed = 5;
				angry.Value = true;
			}
			return result;
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			if (Game1.IsMasterGame && (float)timeUntilExplode > 0f)
			{
				timeUntilExplode.Value -= (float)time.ElapsedGameTime.TotalSeconds;
				if ((float)timeUntilExplode <= 0f)
				{
					base.currentLocation.netAudio.StopPlaying("fuse");
					timeUntilExplode.Value = 0f;
					DropBomb();
					base.Health = -9999;
					return;
				}
			}
			base.behaviorAtGameTick(time);
		}

		public virtual void DropBomb()
		{
			base.currentLocation.netAudio.StopPlaying("fuse");
			if (lastAttacker.Value != null)
			{
				Farmer value = lastAttacker.Value;
				int num = 0;
				int num2 = 287;
				num = Game1.random.Next();
				base.currentLocation.playSound("thudStep");
				Vector2 tileLocation = getTileLocation();
				float y = base.Position.Y;
				float num3 = 2.4f;
				if ((float)timeUntilExplode >= 0f)
				{
					num3 = timeUntilExplode;
					base.currentLocation.netAudio.StartPlaying("fuse");
				}
				int numberOfLoops = Math.Max(1, (int)(num3 * 1000f / 100f));
				Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("Characters\\Monsters\\Hot Head", new Rectangle(16, 64, 16, 16), 25f, 3, numberOfLoops, tileLocation * 64f, flicker: false, Game1.random.NextDouble() < 0.5)
				{
					shakeIntensity = 0.5f,
					shakeIntensityChange = 0.002f,
					extraInfoForEndBehavior = num,
					endFunction = base.currentLocation.removeTemporarySpritesWithID,
					bombRadius = 2,
					bombDamage = base.DamageToFarmer,
					Parent = base.currentLocation,
					scale = 4f,
					owner = value
				});
				Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(598, 1279, 3, 4), 53f, 5, 9, tileLocation * 64f, flicker: true, flipped: false, (y + 7f) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f)
				{
					id = num
				});
				Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(598, 1279, 3, 4), 53f, 5, 9, tileLocation * 64f, flicker: true, flipped: false, (y + 7f) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f)
				{
					delayBeforeAnimationStart = 100,
					id = num
				});
				Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(598, 1279, 3, 4), 53f, 5, 9, tileLocation * 64f, flicker: true, flipped: false, (y + 7f) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f)
				{
					delayBeforeAnimationStart = 200,
					id = num
				});
			}
		}

		protected override void sharedDeathAnimation()
		{
			base.sharedDeathAnimation();
			DropBomb();
		}

		public override void draw(SpriteBatch b)
		{
			if (angry.Value)
			{
				if (!base.IsInvisible && Utility.isOnScreen(base.Position, 128))
				{
					Rectangle sourceRect = Sprite.SourceRect;
					sourceRect.Y += 80;
					b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 42f + yOffset), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3.5f + (float)scale + yOffset / 30f, SpriteEffects.None, (float)(getStandingY() - 1) / 10000f);
					b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, 48 + yJumpOffset), sourceRect, c, rotation, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f)));
				}
			}
			else
			{
				base.draw(b);
			}
		}
	}
}
