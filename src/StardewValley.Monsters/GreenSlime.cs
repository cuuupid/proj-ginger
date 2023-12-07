using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Objects;
using StardewValley.Projectiles;

namespace StardewValley.Monsters
{
	public class GreenSlime : Monster
	{
		public const float mutationFactor = 0.25f;

		public const int matingInterval = 120000;

		public const int childhoodLength = 120000;

		public const int durationOfMating = 2000;

		public const double chanceToMate = 0.001;

		public static int matingRange = 192;

		public const int AQUA_SLIME = 9999899;

		public NetIntDelta stackedSlimes = new NetIntDelta(0);

		public float randomStackOffset;

		[XmlIgnore]
		public NetEvent1Field<Vector2, NetVector2> attackedEvent = new NetEvent1Field<Vector2, NetVector2>();

		[XmlElement("leftDrift")]
		public readonly NetBool leftDrift = new NetBool();

		[XmlElement("cute")]
		public readonly NetBool cute = new NetBool(value: true);

		private int readyToJump = -1;

		private int matingCountdown;

		private new int yOffset;

		private int wagTimer;

		public int readyToMate = 120000;

		[XmlElement("ageUntilFullGrown")]
		public readonly NetInt ageUntilFullGrown = new NetInt();

		public int animateTimer;

		public int timeSinceLastJump;

		[XmlElement("specialNumber")]
		public readonly NetInt specialNumber = new NetInt();

		[XmlElement("firstGeneration")]
		public readonly NetBool firstGeneration = new NetBool();

		[XmlElement("color")]
		public readonly NetColor color = new NetColor();

		private readonly NetBool pursuingMate = new NetBool();

		private readonly NetBool avoidingMate = new NetBool();

		private GreenSlime mate;

		public readonly NetBool prismatic = new NetBool();

		private readonly NetVector2 facePosition = new NetVector2();

		private readonly NetEvent1Field<Vector2, NetVector2> jumpEvent = new NetEvent1Field<Vector2, NetVector2>();

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(leftDrift, cute, ageUntilFullGrown, specialNumber, firstGeneration, color, pursuingMate, avoidingMate, facePosition, jumpEvent, prismatic, stackedSlimes, attackedEvent.NetFields);
			stackedSlimes.Minimum = 0;
			attackedEvent.onEvent += OnAttacked;
			jumpEvent.onEvent += doJump;
			jumpEvent.InterpolationWait = false;
		}

		public GreenSlime()
		{
		}

		public GreenSlime(Vector2 position)
			: base("Green Slime", position)
		{
			if (Game1.random.NextDouble() < 0.5)
			{
				leftDrift.Value = true;
			}
			base.Slipperiness = 4;
			readyToMate = Game1.random.Next(1000, 120000);
			int num = Game1.random.Next(200, 256);
			color.Value = new Color(num / Game1.random.Next(2, 10), Game1.random.Next(180, 256), (Game1.random.NextDouble() < 0.1) ? 255 : (255 - num));
			firstGeneration.Value = true;
			flip = Game1.random.NextDouble() < 0.5;
			cute.Value = Game1.random.NextDouble() < 0.49;
			base.HideShadow = true;
		}

		public GreenSlime(Vector2 position, int mineLevel)
			: base("Green Slime", position)
		{
			randomStackOffset = Utility.RandomFloat(0f, 100f);
			cute.Value = Game1.random.NextDouble() < 0.49;
			flip = Game1.random.NextDouble() < 0.5;
			specialNumber.Value = Game1.random.Next(100);
			if (mineLevel < 40)
			{
				parseMonsterInfo("Green Slime");
				int num = Game1.random.Next(200, 256);
				color.Value = new Color(num / Game1.random.Next(2, 10), num, (Game1.random.NextDouble() < 0.01) ? 255 : (255 - num));
				if (Game1.random.NextDouble() < 0.01 && mineLevel % 5 != 0 && mineLevel % 5 != 1)
				{
					color.Value = new Color(205, 255, 0) * 0.7f;
					hasSpecialItem.Value = true;
					base.Health *= 3;
					base.DamageToFarmer *= 2;
				}
				if (Game1.random.NextDouble() < 0.01 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
				{
					objectsToDrop.Add(680);
				}
			}
			else if (mineLevel < 80)
			{
				base.Name = "Frost Jelly";
				parseMonsterInfo("Frost Jelly");
				int num2 = Game1.random.Next(200, 256);
				color.Value = new Color((Game1.random.NextDouble() < 0.01) ? 180 : (num2 / Game1.random.Next(2, 10)), (Game1.random.NextDouble() < 0.1) ? 255 : (255 - num2 / 3), num2);
				if (Game1.random.NextDouble() < 0.01 && mineLevel % 5 != 0 && mineLevel % 5 != 1)
				{
					color.Value = new Color(0, 0, 0) * 0.7f;
					hasSpecialItem.Value = true;
					base.Health *= 3;
					base.DamageToFarmer *= 2;
				}
				if (Game1.random.NextDouble() < 0.01 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
				{
					objectsToDrop.Add(413);
				}
			}
			else if (mineLevel >= 77377 && mineLevel < 77387)
			{
				base.Name = "Sludge";
				parseMonsterInfo("Sludge");
			}
			else if (mineLevel > 120)
			{
				base.Name = "Sludge";
				parseMonsterInfo("Sludge");
				color.Value = Color.BlueViolet;
				base.Health *= 2;
				int r = color.R;
				int g = color.G;
				int b = color.B;
				r += Game1.random.Next(-20, 21);
				g += Game1.random.Next(-20, 21);
				b += Game1.random.Next(-20, 21);
				color.R = (byte)Math.Max(Math.Min(255, r), 0);
				color.G = (byte)Math.Max(Math.Min(255, g), 0);
				color.B = (byte)Math.Max(Math.Min(255, b), 0);
				while (Game1.random.NextDouble() < 0.08)
				{
					objectsToDrop.Add(386);
				}
				if (Game1.random.NextDouble() < 0.009)
				{
					objectsToDrop.Add(337);
				}
				if (Game1.random.NextDouble() < 0.01 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
				{
					objectsToDrop.Add(439);
				}
			}
			else
			{
				base.Name = "Sludge";
				parseMonsterInfo("Sludge");
				int num3 = Game1.random.Next(200, 256);
				color.Value = new Color(num3, (Game1.random.NextDouble() < 0.01) ? 255 : (255 - num3), num3 / Game1.random.Next(2, 10));
				if (Game1.random.NextDouble() < 0.01 && mineLevel % 5 != 0 && mineLevel % 5 != 1)
				{
					color.Value = new Color(50, 10, 50) * 0.7f;
					hasSpecialItem.Value = true;
					base.Health *= 3;
					base.DamageToFarmer *= 2;
				}
				if (Game1.random.NextDouble() < 0.01 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
				{
					objectsToDrop.Add(437);
				}
			}
			if ((bool)cute)
			{
				base.Health += base.Health / 4;
				base.DamageToFarmer++;
			}
			if (Game1.random.NextDouble() < 0.5)
			{
				leftDrift.Value = true;
			}
			base.Slipperiness = 3;
			readyToMate = Game1.random.Next(1000, 120000);
			if (Game1.random.NextDouble() < 0.001)
			{
				color.Value = new Color(255, 255, 50);
				coinsToDrop.Value = 10;
			}
			if (mineLevel == 9999899)
			{
				color.Value = new Color(0, 255, 200);
				base.Health *= 2;
				objectsToDrop.Clear();
				if (Game1.random.NextDouble() < 0.02)
				{
					objectsToDrop.Add(394);
				}
				if (Game1.random.NextDouble() < 0.02)
				{
					objectsToDrop.Add(60);
				}
				if (Game1.random.NextDouble() < 0.02)
				{
					objectsToDrop.Add(62);
				}
				if (Game1.random.NextDouble() < 0.01)
				{
					objectsToDrop.Add(797);
				}
				if (Game1.random.NextDouble() < 0.03 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
				{
					objectsToDrop.Add(413);
				}
				while (Game1.random.NextDouble() < 0.5)
				{
					objectsToDrop.Add(766);
				}
			}
			firstGeneration.Value = true;
			base.HideShadow = true;
		}

		public GreenSlime(Vector2 position, Color color)
			: base("Green Slime", position)
		{
			this.color.Value = color;
			firstGeneration.Value = true;
			base.HideShadow = true;
		}

		public void makeTigerSlime()
		{
			base.Name = "Tiger Slime";
			base.reloadSprite();
			Sprite.SpriteHeight = 24;
			Sprite.UpdateSourceRect();
			parseMonsterInfo("Tiger Slime");
			color.Value = Color.White;
		}

		public void makePrismatic()
		{
			prismatic.Value = true;
			base.Name = "Prismatic Slime";
			base.Health = 1000;
			damageToFarmer.Value = 35;
			hasSpecialItem.Value = false;
		}

		public override void reloadSprite()
		{
			if (base.Name == "Tiger Slime")
			{
				makeTigerSlime();
				return;
			}
			base.HideShadow = true;
			string text = name;
			base.Name = "Green Slime";
			base.reloadSprite();
			base.Name = text;
			Sprite.SpriteHeight = 24;
			Sprite.UpdateSourceRect();
		}

		public virtual void OnAttacked(Vector2 trajectory)
		{
			if (Game1.IsMasterGame && stackedSlimes.Value > 0)
			{
				stackedSlimes.Value--;
				if (trajectory.LengthSquared() == 0f)
				{
					trajectory = new Vector2(0f, -1f);
				}
				else
				{
					trajectory.Normalize();
				}
				trajectory *= 16f;
				BasicProjectile basicProjectile = new BasicProjectile(base.DamageToFarmer / 3 * 2, 13, 3, 0, (float)Math.PI / 16f, trajectory.X, trajectory.Y, base.Position, "", "", explode: true, damagesMonsters: false, base.currentLocation, this);
				basicProjectile.height.Value = 24f;
				basicProjectile.color.Value = color.Value;
				basicProjectile.ignoreMeleeAttacks.Value = true;
				basicProjectile.hostTimeUntilAttackable = 0.1f;
				if (Game1.random.NextDouble() < 0.5)
				{
					basicProjectile.debuff.Value = 13;
				}
				base.currentLocation.projectiles.Add(basicProjectile);
			}
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			if (base.currentLocation == null)
			{
				base.currentLocation = Game1.currentLocation;
			}
			if (stackedSlimes.Value > 0)
			{
				attackedEvent.Fire(new Vector2(xTrajectory, -yTrajectory));
				xTrajectory = 0;
				yTrajectory = 0;
				damage = 1;
			}
			int num = Math.Max(1, damage - (int)resilience);
			if (Game1.random.NextDouble() < (double)missChance - (double)missChance * addedPrecision)
			{
				num = -1;
			}
			else
			{
				if (Game1.random.NextDouble() < 0.025 && (bool)cute)
				{
					if (!base.focusedOnFarmers)
					{
						base.DamageToFarmer += base.DamageToFarmer / 2;
						shake(1000);
					}
					base.focusedOnFarmers = true;
				}
				base.Slipperiness = 3;
				base.Health -= num;
				setTrajectory(xTrajectory, yTrajectory);
				base.currentLocation.playSound("slimeHit");
				readyToJump = -1;
				base.IsWalkingTowardPlayer = true;
				if (base.Health <= 0)
				{
					base.currentLocation.playSound("slimedead");
					Game1.stats.SlimesKilled++;
					if (mate != null)
					{
						mate.mate = null;
					}
					if (Game1.gameMode == 3 && (float)scale > 1.8f)
					{
						base.Health = 10;
						int num2 = ((!((float)scale > 1.8f)) ? 1 : Game1.random.Next(3, 5));
						base.Scale *= 2f / 3f;
						for (int i = 0; i < num2; i++)
						{
							base.currentLocation.characters.Add(new GreenSlime(base.Position + new Vector2(i * GetBoundingBox().Width, 0f), Game1.CurrentMineLevel));
							base.currentLocation.characters[base.currentLocation.characters.Count - 1].setTrajectory(xTrajectory + Game1.random.Next(-20, 20), yTrajectory + Game1.random.Next(-20, 20));
							base.currentLocation.characters[base.currentLocation.characters.Count - 1].willDestroyObjectsUnderfoot = false;
							base.currentLocation.characters[base.currentLocation.characters.Count - 1].moveTowardPlayer(4);
							base.currentLocation.characters[base.currentLocation.characters.Count - 1].Scale = 0.75f + (float)Game1.random.Next(-5, 10) / 100f;
						}
					}
					else
					{
						Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(44, base.Position, color.Value * 0.66f, 10)
						{
							interval = 70f,
							holdLastFrame = true,
							alphaFade = 0.01f
						});
						Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(44, base.Position + new Vector2(-16f, 0f), color.Value * 0.66f, 10)
						{
							interval = 70f,
							delayBeforeAnimationStart = 0,
							holdLastFrame = true,
							alphaFade = 0.01f
						});
						Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(44, base.Position + new Vector2(0f, 16f), color.Value * 0.66f, 10)
						{
							interval = 70f,
							delayBeforeAnimationStart = 100,
							holdLastFrame = true,
							alphaFade = 0.01f
						});
						Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(44, base.Position + new Vector2(16f, 0f), color.Value * 0.66f, 10)
						{
							interval = 70f,
							delayBeforeAnimationStart = 200,
							holdLastFrame = true,
							alphaFade = 0.01f
						});
					}
				}
			}
			return num;
		}

		public override void shedChunks(int number, float scale)
		{
			if (base.currentLocation == null)
			{
				base.currentLocation = Game1.currentLocation;
			}
			Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(0, 120, 16, 16), 8, GetBoundingBox().Center.X + 32, GetBoundingBox().Center.Y, number, (int)getTileLocation().Y, color, 4f * scale);
		}

		public override void collisionWithFarmerBehavior()
		{
			farmerPassesThrough = base.Player.isWearingRing(520);
		}

		public override void onDealContactDamage(Farmer who)
		{
			if (Game1.random.NextDouble() < 0.3 && base.Player == Game1.player && !base.Player.temporarilyInvincible && !base.Player.isWearingRing(520) && Game1.random.Next(11) >= who.immunity && !base.Player.hasBuff(28) && Game1.buffsDisplay.addOtherBuff(new Buff(13)))
			{
				base.currentLocation.playSound("slime");
			}
			base.onDealContactDamage(who);
		}

		public override void draw(SpriteBatch b)
		{
			if (base.IsInvisible || !Utility.isOnScreen(base.Position, 128))
			{
				return;
			}
			for (int i = 0; i <= stackedSlimes.Value; i++)
			{
				bool flag = i == stackedSlimes.Value;
				Vector2 vector = Vector2.Zero;
				if (stackedSlimes.Value > 0)
				{
					vector = new Vector2((float)Math.Sin((double)randomStackOffset + Game1.currentGameTime.TotalGameTime.TotalSeconds * Math.PI * 2.0 + (double)(i * 30)) * 8f, -30 * i);
				}
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, GetBoundingBox().Height / 2 + yOffset) + vector, Sprite.SourceRect, prismatic ? Utility.GetPrismaticColor(348 + (int)specialNumber, 5f) : ((Color)color), 0f, new Vector2(8f, 16f), 4f * Math.Max(0.2f, (float)scale - 0.4f * ((float)(int)ageUntilFullGrown / 120000f)), SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)(getStandingY() + i * 2) / 10000f)));
				b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(GetBoundingBox().Height / 2 * 7) / 4f + (float)yOffset + 8f * (float)scale - (float)(((int)ageUntilFullGrown > 0) ? 8 : 0)) + vector, Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + (float)scale - (float)(int)ageUntilFullGrown / 120000f - ((Sprite.currentFrame % 4 % 3 != 0 || i != 0) ? 1f : 0f) + (float)yOffset / 30f, SpriteEffects.None, (float)(getStandingY() - 1 + i * 2) / 10000f);
				if ((int)ageUntilFullGrown <= 0)
				{
					if (flag && ((bool)cute || (bool)hasSpecialItem))
					{
						int x = ((isMoving() || wagTimer > 0) ? (16 * Math.Min(7, Math.Abs(((wagTimer > 0) ? (992 - wagTimer) : (Game1.currentGameTime.TotalGameTime.Milliseconds % 992)) - 496) / 62) % 64) : 48);
						int num = ((isMoving() || wagTimer > 0) ? (24 * Math.Min(1, Math.Max(1, Math.Abs(((wagTimer > 0) ? (992 - wagTimer) : (Game1.currentGameTime.TotalGameTime.Milliseconds % 992)) - 496) / 62) / 4)) : 24);
						if ((bool)hasSpecialItem)
						{
							num += 48;
						}
						b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + vector + new Vector2(32f, GetBoundingBox().Height - 16 + ((readyToJump <= 0) ? (4 * (-2 + Math.Abs(Sprite.currentFrame % 4 - 2))) : (4 + 4 * (Sprite.currentFrame % 4 % 3))) + yOffset) * scale, new Rectangle(x, 168 + num, 16, 24), hasSpecialItem ? Color.White : ((Color)color), 0f, new Vector2(8f, 16f), 4f * Math.Max(0.2f, (float)scale - 0.4f * ((float)(int)ageUntilFullGrown / 120000f)), flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f + 0.0001f)));
					}
					b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + vector + (new Vector2(32f, GetBoundingBox().Height / 2 + ((readyToJump <= 0) ? (4 * (-2 + Math.Abs(Sprite.currentFrame % 4 - 2))) : (4 - 4 * (Sprite.currentFrame % 4 % 3))) + yOffset) + facePosition) * Math.Max(0.2f, (float)scale - 0.4f * ((float)(int)ageUntilFullGrown / 120000f)), new Rectangle(32 + ((readyToJump > 0 || base.focusedOnFarmers) ? 16 : 0), 120 + ((readyToJump < 0 && (base.focusedOnFarmers || invincibleCountdown > 0)) ? 24 : 0), 16, 24), Color.White * ((FacingDirection == 0) ? 0.5f : 1f), 0f, new Vector2(8f, 16f), 4f * Math.Max(0.2f, (float)scale - 0.4f * ((float)(int)ageUntilFullGrown / 120000f)), SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)(getStandingY() + i * 2) / 10000f + 0.0001f)));
				}
				if (isGlowing)
				{
					b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + vector + new Vector2(32f, GetBoundingBox().Height / 2 + yOffset), Sprite.SourceRect, glowingColor * glowingTransparency, 0f, new Vector2(8f, 16f), 4f * Math.Max(0.2f, scale), SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.99f : ((float)getStandingY() / 10000f + 0.001f)));
				}
			}
			if ((bool)pursuingMate)
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, -32 + yOffset), new Rectangle(16, 120, 8, 8), Color.White, 0f, new Vector2(3f, 3f), 4f, SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f)));
			}
			else if ((bool)avoidingMate)
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, -32 + yOffset), new Rectangle(24, 120, 8, 8), Color.White, 0f, new Vector2(4f, 4f), 4f, SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f)));
			}
		}

		public void moveTowardOtherSlime(GreenSlime other, bool moveAway, GameTime time)
		{
			int num = Math.Abs(other.getStandingX() - getStandingX());
			int num2 = Math.Abs(other.getStandingY() - getStandingY());
			if (num > 4 || num2 > 4)
			{
				int num3 = ((other.getStandingX() > getStandingX()) ? 1 : (-1));
				int num4 = ((other.getStandingY() > getStandingY()) ? 1 : (-1));
				if (moveAway)
				{
					num3 = -num3;
					num4 = -num4;
				}
				double num5 = (double)num / (double)(num + num2);
				if (Game1.random.NextDouble() < num5)
				{
					tryToMoveInDirection((num3 > 0) ? 1 : 3, isFarmer: false, base.DamageToFarmer, glider: false);
				}
				else
				{
					tryToMoveInDirection((num4 > 0) ? 2 : 0, isFarmer: false, base.DamageToFarmer, glider: false);
				}
			}
			Sprite.AnimateDown(time);
			if (invincibleCountdown > 0)
			{
				invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
				if (invincibleCountdown <= 0)
				{
					stopGlowing();
				}
			}
		}

		public void doneMating()
		{
			readyToMate = 120000;
			matingCountdown = 2000;
			mate = null;
			pursuingMate.Value = false;
			avoidingMate.Value = false;
		}

		public override void noMovementProgressNearPlayerBehavior()
		{
			faceGeneralDirection(base.Player.getStandingPosition());
		}

		public void mateWith(GreenSlime mateToPursue, GameLocation location)
		{
			if (location.canSlimeMateHere())
			{
				GreenSlime greenSlime = new GreenSlime(Vector2.Zero);
				Utility.recursiveFindPositionForCharacter(greenSlime, location, getTileLocation(), 30);
				Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 10 + (int)((float)scale * 100f) + (int)((float)mateToPursue.scale * 100f));
				switch (random.Next(4))
				{
				case 0:
					greenSlime.color.Value = new Color(Math.Min(255, Math.Max(0, color.R + random.Next((int)((float)(-color.R) * 0.25f), (int)((float)(int)color.R * 0.25f)))), Math.Min(255, Math.Max(0, color.G + random.Next((int)((float)(-color.G) * 0.25f), (int)((float)(int)color.G * 0.25f)))), Math.Min(255, Math.Max(0, color.B + random.Next((int)((float)(-color.B) * 0.25f), (int)((float)(int)color.B * 0.25f)))));
					break;
				case 1:
				case 2:
					greenSlime.color.Value = Utility.getBlendedColor(color, mateToPursue.color);
					break;
				case 3:
					greenSlime.color.Value = new Color(Math.Min(255, Math.Max(0, mateToPursue.color.R + random.Next((int)((float)(-mateToPursue.color.R) * 0.25f), (int)((float)(int)mateToPursue.color.R * 0.25f)))), Math.Min(255, Math.Max(0, mateToPursue.color.G + random.Next((int)((float)(-mateToPursue.color.G) * 0.25f), (int)((float)(int)mateToPursue.color.G * 0.25f)))), Math.Min(255, Math.Max(0, mateToPursue.color.B + random.Next((int)((float)(-mateToPursue.color.B) * 0.25f), (int)((float)(int)mateToPursue.color.B * 0.25f)))));
					break;
				}
				int r = greenSlime.color.R;
				int g = greenSlime.color.G;
				int b = greenSlime.color.B;
				greenSlime.Name = name;
				if (greenSlime.Name == "Tiger Slime")
				{
					greenSlime.makeTigerSlime();
				}
				else if (r > 100 && b > 100 && g < 50)
				{
					greenSlime.parseMonsterInfo("Sludge");
					while (random.NextDouble() < 0.1)
					{
						greenSlime.objectsToDrop.Add(386);
					}
					if (random.NextDouble() < 0.01)
					{
						greenSlime.objectsToDrop.Add(337);
					}
				}
				else if (r >= 200 && g < 75)
				{
					greenSlime.parseMonsterInfo("Sludge");
				}
				else if (b >= 200 && r < 100)
				{
					greenSlime.parseMonsterInfo("Frost Jelly");
				}
				greenSlime.Health = ((random.NextDouble() < 0.5) ? base.Health : mateToPursue.Health);
				greenSlime.Health = Math.Max(1, base.Health + random.Next(-4, 5));
				greenSlime.DamageToFarmer = ((random.NextDouble() < 0.5) ? base.DamageToFarmer : mateToPursue.DamageToFarmer);
				greenSlime.DamageToFarmer = Math.Max(0, base.DamageToFarmer + random.Next(-1, 2));
				greenSlime.resilience.Value = ((random.NextDouble() < 0.5) ? resilience : mateToPursue.resilience);
				greenSlime.resilience.Value = Math.Max(0, (int)resilience + random.Next(-1, 2));
				greenSlime.missChance.Value = ((random.NextDouble() < 0.5) ? missChance : mateToPursue.missChance);
				greenSlime.missChance.Value = Math.Max(0.0, (double)missChance + (double)((float)random.Next(-1, 2) / 100f));
				greenSlime.Scale = ((random.NextDouble() < 0.5) ? scale : mateToPursue.scale);
				greenSlime.Scale = Math.Max(0.6f, Math.Min(1.5f, (float)scale + (float)random.Next(-2, 3) / 100f));
				greenSlime.Slipperiness = 8;
				base.speed = ((random.NextDouble() < 0.5) ? base.speed : mateToPursue.speed);
				if (random.NextDouble() < 0.015)
				{
					base.speed = Math.Max(1, Math.Min(6, base.speed + random.Next(-1, 2)));
				}
				greenSlime.setTrajectory(Utility.getAwayFromPositionTrajectory(greenSlime.GetBoundingBox(), getStandingPosition()) / 2f);
				greenSlime.ageUntilFullGrown.Value = 120000;
				greenSlime.Halt();
				greenSlime.firstGeneration.Value = false;
				if (Utility.isOnScreen(base.Position, 128))
				{
					base.currentLocation.playSound("slime");
				}
			}
			mateToPursue.doneMating();
			doneMating();
		}

		public override List<Item> getExtraDropItems()
		{
			List<Item> list = new List<Item>();
			if (name != "Tiger Slime")
			{
				if (color.R < 80 && color.G < 80 && color.B < 80)
				{
					list.Add(new Object(382, 1));
					Random random = new Random((int)base.Position.X * 777 + (int)base.Position.Y * 77 + (int)Game1.stats.DaysPlayed);
					if (random.NextDouble() < 0.05)
					{
						list.Add(new Object(553, 1));
					}
					if (random.NextDouble() < 0.05)
					{
						list.Add(new Object(539, 1));
					}
				}
				else if (color.R > 200 && color.G > 180 && color.B < 50)
				{
					list.Add(new Object(384, 2));
				}
				else if (color.R > 220 && color.G > 90 && color.G < 150 && color.B < 50)
				{
					list.Add(new Object(378, 2));
				}
				else if (color.R > 230 && color.G > 230 && color.B > 230)
				{
					if ((int)color.R % 2 == 1)
					{
						list.Add(new Object(338, 1));
						if ((int)color.G % 2 == 1)
						{
							list.Add(new Object(338, 1));
						}
					}
					else
					{
						list.Add(new Object(380, 1));
					}
					if (((int)color.R % 2 == 0 && (int)color.G % 2 == 0 && (int)color.B % 2 == 0) || color.Equals(Color.White))
					{
						list.Add(new Object(72, 1));
					}
				}
				else if (color.R > 150 && color.G > 150 && color.B > 150)
				{
					list.Add(new Object(390, 2));
				}
				else if (color.R > 150 && color.B > 180 && color.G < 50 && (int)specialNumber % (firstGeneration ? 4 : 2) == 0)
				{
					list.Add(new Object(386, 2));
					if ((bool)firstGeneration && Game1.random.NextDouble() < 0.005)
					{
						list.Add(new Object(485, 1));
					}
				}
			}
			if (Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt") && (int)specialNumber == 1)
			{
				switch (base.Name)
				{
				case "Green Slime":
					list.Add(new Object(680, 1));
					break;
				case "Frost Jelly":
					list.Add(new Object(413, 1));
					break;
				case "Tiger Slime":
					list.Add(new Object(857, 1));
					break;
				}
			}
			if (base.Name == "Tiger Slime")
			{
				if (Game1.random.NextDouble() < 0.001)
				{
					list.Add(new Hat(91));
				}
				if (Game1.random.NextDouble() < 0.1)
				{
					list.Add(new Object(831, 1));
					while (Game1.random.NextDouble() < 0.5)
					{
						list.Add(new Object(831, 1));
					}
				}
				else if (Game1.random.NextDouble() < 0.1)
				{
					list.Add(new Object(829, 1));
				}
				else if (Game1.random.NextDouble() < 0.02)
				{
					list.Add(new Object(833, 1));
					while (Game1.random.NextDouble() < 0.5)
					{
						list.Add(new Object(833, 1));
					}
				}
				else if (Game1.random.NextDouble() < 0.006)
				{
					list.Add(new Object(835, 1));
				}
			}
			if (prismatic.Value && Game1.player.team.specialOrders.Where((SpecialOrder x) => x.questKey == "Wizard2") != null)
			{
				Object @object = new Object(876, 1)
				{
					specialItem = true
				};
				@object.questItem.Value = true;
				return new List<Item> { @object };
			}
			return list;
		}

		public override void dayUpdate(int dayOfMonth)
		{
			if ((int)ageUntilFullGrown > 0)
			{
				ageUntilFullGrown.Value /= 2;
			}
			if (readyToMate > 0)
			{
				readyToMate /= 2;
			}
			base.dayUpdate(dayOfMonth);
		}

		protected override void updateAnimation(GameTime time)
		{
			if (wagTimer > 0)
			{
				wagTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
			yOffset = Math.Max(yOffset - (int)Math.Abs(xVelocity + yVelocity) / 2, -64);
			if (yOffset < 0)
			{
				yOffset = Math.Min(0, yOffset + 4 + (int)((yOffset <= -64) ? ((float)(-yOffset) / 8f) : ((float)(-yOffset) / 16f)));
			}
			timeSinceLastJump += time.ElapsedGameTime.Milliseconds;
			if (Game1.random.NextDouble() < 0.01 && wagTimer <= 0)
			{
				wagTimer = 992;
			}
			if (Math.Abs(xVelocity) >= 0.5f || Math.Abs(yVelocity) >= 0.5f)
			{
				Sprite.AnimateDown(time);
			}
			else if (!base.Position.Equals(lastPosition))
			{
				animateTimer = 500;
			}
			if (animateTimer > 0 && readyToJump <= 0)
			{
				animateTimer -= time.ElapsedGameTime.Milliseconds;
				Sprite.AnimateDown(time);
			}
			resetAnimationSpeed();
		}

		public override void update(GameTime time, GameLocation location)
		{
			base.update(time, location);
			jumpEvent.Poll();
			attackedEvent.Poll();
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			if (mate == null)
			{
				pursuingMate.Value = false;
				avoidingMate.Value = false;
			}
			switch (FacingDirection)
			{
			case 2:
				if (facePosition.X > 0f)
				{
					facePosition.X -= 2f;
				}
				else if (facePosition.X < 0f)
				{
					facePosition.X += 2f;
				}
				if (facePosition.Y < 0f)
				{
					facePosition.Y += 2f;
				}
				break;
			case 1:
				if (facePosition.X < 8f)
				{
					facePosition.X += 2f;
				}
				if (facePosition.Y < 0f)
				{
					facePosition.Y += 2f;
				}
				break;
			case 3:
				if (facePosition.X > -8f)
				{
					facePosition.X -= 2f;
				}
				if (facePosition.Y < 0f)
				{
					facePosition.Y += 2f;
				}
				break;
			case 0:
				if (facePosition.X > 0f)
				{
					facePosition.X -= 2f;
				}
				else if (facePosition.X < 0f)
				{
					facePosition.X += 2f;
				}
				if (facePosition.Y > -8f)
				{
					facePosition.Y -= 2f;
				}
				break;
			}
			if (stackedSlimes.Value <= 0)
			{
				if ((int)ageUntilFullGrown <= 0)
				{
					readyToMate -= time.ElapsedGameTime.Milliseconds;
				}
				else
				{
					ageUntilFullGrown.Value -= time.ElapsedGameTime.Milliseconds;
				}
			}
			if ((bool)pursuingMate && mate != null)
			{
				if (readyToMate <= -35000)
				{
					mate.doneMating();
					doneMating();
					return;
				}
				moveTowardOtherSlime(mate, moveAway: false, time);
				if (mate.mate != null && (bool)mate.pursuingMate && !mate.mate.Equals(this))
				{
					doneMating();
				}
				else if (Vector2.Distance(getStandingPosition(), mate.getStandingPosition()) < (float)(GetBoundingBox().Width + 4))
				{
					if (mate.mate != null && (bool)mate.avoidingMate && mate.mate.Equals(this))
					{
						mate.avoidingMate.Value = false;
						mate.matingCountdown = 2000;
						mate.pursuingMate.Value = true;
					}
					matingCountdown -= time.ElapsedGameTime.Milliseconds;
					if (base.currentLocation != null && matingCountdown <= 0 && (bool)pursuingMate && (!base.currentLocation.isOutdoors || Utility.getNumberOfCharactersInRadius(base.currentLocation, Utility.Vector2ToPoint(base.Position), 1) <= 4))
					{
						mateWith(mate, base.currentLocation);
					}
				}
				else if (Vector2.Distance(getStandingPosition(), mate.getStandingPosition()) > (float)(matingRange * 2))
				{
					mate.mate = null;
					mate.avoidingMate.Value = false;
					mate = null;
				}
				return;
			}
			if ((bool)avoidingMate && mate != null)
			{
				moveTowardOtherSlime(mate, moveAway: true, time);
				return;
			}
			if (readyToMate < 0 && (bool)cute)
			{
				readyToMate = -1;
				if (Game1.random.NextDouble() < 0.001)
				{
					GreenSlime greenSlime = (GreenSlime)Utility.checkForCharacterWithinArea(GetType(), base.Position, base.currentLocation, new Rectangle(getStandingX() - matingRange, getStandingY() - matingRange, matingRange * 2, matingRange * 2));
					if (greenSlime != null && greenSlime.readyToMate <= 0 && !greenSlime.cute && greenSlime.stackedSlimes.Value <= 0)
					{
						matingCountdown = 2000;
						mate = greenSlime;
						pursuingMate.Value = true;
						greenSlime.mate = this;
						greenSlime.avoidingMate.Value = true;
						base.addedSpeed = 1;
						mate.addedSpeed = 1;
						return;
					}
				}
			}
			else if (!isGlowing)
			{
				base.addedSpeed = 0;
			}
			base.behaviorAtGameTick(time);
			if (readyToJump != -1)
			{
				Halt();
				base.IsWalkingTowardPlayer = false;
				readyToJump -= time.ElapsedGameTime.Milliseconds;
				Sprite.currentFrame = 16 + (800 - readyToJump) / 200;
				if (readyToJump <= 0)
				{
					timeSinceLastJump = timeSinceLastJump;
					base.Slipperiness = 10;
					base.IsWalkingTowardPlayer = true;
					readyToJump = -1;
					invincibleCountdown = 0;
					Vector2 awayFromPlayerTrajectory = Utility.getAwayFromPlayerTrajectory(GetBoundingBox(), base.Player);
					awayFromPlayerTrajectory.X = (0f - awayFromPlayerTrajectory.X) / 2f;
					awayFromPlayerTrajectory.Y = (0f - awayFromPlayerTrajectory.Y) / 2f;
					jumpEvent.Fire(awayFromPlayerTrajectory);
					setTrajectory((int)awayFromPlayerTrajectory.X, (int)awayFromPlayerTrajectory.Y);
				}
			}
			else if (Game1.random.NextDouble() < 0.1 && !base.focusedOnFarmers)
			{
				if (FacingDirection == 0 || FacingDirection == 2)
				{
					if ((bool)leftDrift && !base.currentLocation.isCollidingPosition(nextPosition(3), Game1.viewport, isFarmer: false, 1, glider: false, this))
					{
						position.X -= base.speed;
					}
					else if (!leftDrift && !base.currentLocation.isCollidingPosition(nextPosition(1), Game1.viewport, isFarmer: false, 1, glider: false, this))
					{
						position.X += base.speed;
					}
				}
				else if ((bool)leftDrift && !base.currentLocation.isCollidingPosition(nextPosition(0), Game1.viewport, isFarmer: false, 1, glider: false, this))
				{
					position.Y -= base.speed;
				}
				else if (!leftDrift && !base.currentLocation.isCollidingPosition(nextPosition(2), Game1.viewport, isFarmer: false, 1, glider: false, this))
				{
					position.Y += base.speed;
				}
				if (Game1.random.NextDouble() < 0.08)
				{
					leftDrift.Value = !leftDrift;
				}
			}
			else if (withinPlayerThreshold() && timeSinceLastJump > (base.focusedOnFarmers ? 1000 : 4000) && Game1.random.NextDouble() < 0.01 && stackedSlimes.Value <= 0)
			{
				if (base.Name.Equals("Frost Jelly") && Game1.random.NextDouble() < 0.25)
				{
					base.addedSpeed = 2;
					startGlowing(Color.Cyan, border: false, 0.15f);
				}
				else
				{
					base.addedSpeed = 0;
					stopGlowing();
					readyToJump = 800;
				}
			}
		}

		private void doJump(Vector2 trajectory)
		{
			if (Utility.isOnScreen(position, 128))
			{
				base.currentLocation.localSound("slime");
			}
			Sprite.currentFrame = 1;
		}
	}
}
