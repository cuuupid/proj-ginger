using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Mobile;

namespace StardewValley.Minigames
{
	[XmlInclude(typeof(JOTPKProgress))]
	[InstanceStatics]
	public class AbigailGame : IMinigame
	{
		public delegate void behaviorAfterMotionPause();

		public enum GameKeys
		{
			MoveLeft,
			MoveRight,
			MoveUp,
			MoveDown,
			ShootLeft,
			ShootRight,
			ShootUp,
			ShootDown,
			UsePowerup,
			SelectOption,
			Exit,
			MAX
		}

		public class CowboyPowerup
		{
			public int which;

			public Point position;

			public int duration;

			public float yOffset;

			public CowboyPowerup(int which, Point position, int duration)
			{
				this.which = which;
				this.position = position;
				this.duration = duration;
			}

			public void draw(SpriteBatch b)
			{
				if (duration > 2000 || duration / 200 % 2 == 0)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, (float)position.Y + yOffset), new Rectangle(272 + which * 16, 1808, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f + 0.001f);
				}
			}
		}

		public class JOTPKProgress : INetObject<NetFields>
		{
			public NetInt bulletDamage = new NetInt();

			public NetInt fireSpeedLevel = new NetInt();

			public NetInt ammoLevel = new NetInt();

			public NetBool spreadPistol = new NetBool();

			public NetInt runSpeedLevel = new NetInt();

			public NetInt lives = new NetInt();

			public NetInt coins = new NetInt();

			public NetInt score = new NetInt();

			public NetBool died = new NetBool();

			public NetInt whichRound = new NetInt();

			public NetInt whichWave = new NetInt();

			public NetInt heldItem = new NetInt(-100);

			public NetInt world = new NetInt();

			public NetInt waveTimer = new NetInt();

			public NetList<Vector2, NetVector2> monsterChances = new NetList<Vector2, NetVector2>();

			public NetFields NetFields { get; } = new NetFields();


			public JOTPKProgress()
			{
				NetFields.AddFields(bulletDamage, runSpeedLevel, ammoLevel, lives, coins, score, died, spreadPistol, whichRound, whichWave, heldItem, world, waveTimer, monsterChances);
			}
		}

		public class CowboyBullet
		{
			public Point position;

			public Point motion;

			public int damage;

			public CowboyBullet(Point position, Point motion, int damage)
			{
				this.position = position;
				this.motion = motion;
				this.damage = damage;
			}

			public CowboyBullet(Point position, int direction, int damage)
			{
				this.position = position;
				switch (direction)
				{
				case 0:
					motion = new Point(0, -8);
					break;
				case 1:
					motion = new Point(8, 0);
					break;
				case 2:
					motion = new Point(0, 8);
					break;
				case 3:
					motion = new Point(-8, 0);
					break;
				}
				this.damage = damage;
			}
		}

		public class CowboyMonster
		{
			public const int MonsterAnimationDelay = 500;

			public int health;

			public int type;

			public int speed;

			public float movementAnimationTimer;

			public Rectangle position;

			public int movementDirection;

			public bool movedLastTurn;

			public bool oppositeMotionGuy;

			public bool invisible;

			public bool special;

			public bool uninterested;

			public bool flyer;

			public Color tint = Color.White;

			public Color flashColor = Color.Red;

			public float flashColorTimer;

			public int ticksSinceLastMovement;

			public Vector2 acceleration;

			private Point targetPosition;

			public CowboyMonster(int which, int health, int speed, Point position)
			{
				this.health = health;
				type = which;
				this.speed = speed;
				this.position = new Rectangle(position.X, position.Y, TileSize, TileSize);
				uninterested = Game1.random.NextDouble() < 0.25;
			}

			public CowboyMonster(int which, Point position)
			{
				type = which;
				this.position = new Rectangle(position.X, position.Y, TileSize, TileSize);
				switch (type)
				{
				case 0:
					speed = 2;
					health = 1;
					uninterested = Game1.random.NextDouble() < 0.25;
					if (uninterested)
					{
						targetPosition = new Point(Game1.random.Next(2, 14) * TileSize, Game1.random.Next(2, 14) * TileSize);
					}
					break;
				case 2:
					speed = 1;
					health = 3;
					break;
				case 5:
					speed = 3;
					health = 2;
					break;
				case 1:
					speed = 2;
					health = 1;
					flyer = true;
					break;
				case 3:
					health = 6;
					speed = 1;
					uninterested = Game1.random.NextDouble() < 0.25;
					if (uninterested)
					{
						targetPosition = new Point(Game1.random.Next(2, 14) * TileSize, Game1.random.Next(2, 14) * TileSize);
					}
					break;
				case 4:
					health = 3;
					speed = 3;
					flyer = true;
					break;
				case 6:
				{
					speed = 3;
					health = 2;
					int num = 0;
					do
					{
						targetPosition = new Point(Game1.random.Next(2, 14) * TileSize, Game1.random.Next(2, 14) * TileSize);
						num++;
					}
					while (isCollidingWithMap(targetPosition) && num < 10);
					break;
				}
				}
				oppositeMotionGuy = Game1.random.NextDouble() < 0.5;
			}

			public virtual void draw(SpriteBatch b)
			{
				if (type == 6 && special)
				{
					if (flashColorTimer > 0f)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(480, 1696, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f + 0.001f);
					}
					else
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(576, 1712, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f + 0.001f);
					}
				}
				else if (!invisible)
				{
					if (flashColorTimer > 0f)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(352 + type * 16, 1696, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f + 0.001f);
					}
					else
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(352 + (type * 2 + ((movementAnimationTimer < 250f) ? 1 : 0)) * 16, 1712, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f + 0.001f);
					}
					if (monsterConfusionTimer > 0)
					{
						b.DrawString(Game1.smallFont, "?", topLeftScreenCoordinate + new Vector2((float)(position.X + TileSize / 2) - Game1.smallFont.MeasureString("?").X / 2f, position.Y - TileSize / 2), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)position.Y / 10000f);
						b.DrawString(Game1.smallFont, "?", topLeftScreenCoordinate + new Vector2((float)(position.X + TileSize / 2) - Game1.smallFont.MeasureString("?").X / 2f + 1f, position.Y - TileSize / 2), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)position.Y / 10000f);
						b.DrawString(Game1.smallFont, "?", topLeftScreenCoordinate + new Vector2((float)(position.X + TileSize / 2) - Game1.smallFont.MeasureString("?").X / 2f - 1f, position.Y - TileSize / 2), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)position.Y / 10000f);
					}
				}
			}

			public virtual bool takeDamage(int damage)
			{
				health -= damage;
				health = Math.Max(0, health);
				if (health <= 0)
				{
					return true;
				}
				Game1.playSound("cowboy_monsterhit");
				flashColor = Color.Red;
				flashColorTimer = 100f;
				return false;
			}

			public virtual int getLootDrop()
			{
				if (type == 6 && special)
				{
					return -1;
				}
				if (Game1.random.NextDouble() < 0.05)
				{
					if (type != 0 && Game1.random.NextDouble() < 0.1)
					{
						return 1;
					}
					if (Game1.random.NextDouble() < 0.01)
					{
						return 1;
					}
					return 0;
				}
				if (Game1.random.NextDouble() < 0.05)
				{
					if (Game1.random.NextDouble() < 0.15)
					{
						return Game1.random.Next(6, 8);
					}
					if (Game1.random.NextDouble() < 0.07)
					{
						return 10;
					}
					int num = Game1.random.Next(2, 10);
					if (num == 5 && Game1.random.NextDouble() < 0.4)
					{
						num = Game1.random.Next(2, 10);
					}
					return num;
				}
				return -1;
			}

			public virtual bool move(Vector2 playerPosition, GameTime time)
			{
				movementAnimationTimer -= time.ElapsedGameTime.Milliseconds;
				if (movementAnimationTimer <= 0f)
				{
					movementAnimationTimer = Math.Max(100, 500 - speed * 50);
				}
				if (flashColorTimer > 0f)
				{
					flashColorTimer -= time.ElapsedGameTime.Milliseconds;
					return false;
				}
				if (monsterConfusionTimer > 0)
				{
					return false;
				}
				if (shopping)
				{
					shoppingTimer -= time.ElapsedGameTime.Milliseconds;
					if (shoppingTimer <= 0)
					{
						shoppingTimer = 100;
					}
				}
				ticksSinceLastMovement++;
				switch (type)
				{
				case 0:
				case 2:
				case 3:
				case 5:
				case 6:
				{
					if (type == 6)
					{
						if (special || invisible)
						{
							break;
						}
						if (ticksSinceLastMovement > 20)
						{
							int num3 = 0;
							do
							{
								targetPosition = new Point(Game1.random.Next(2, 14) * TileSize, Game1.random.Next(2, 14) * TileSize);
								num3++;
							}
							while (isCollidingWithMap(targetPosition) && num3 < 5);
						}
					}
					else if (ticksSinceLastMovement > 20)
					{
						int num4 = 0;
						do
						{
							oppositeMotionGuy = !oppositeMotionGuy;
							targetPosition = new Point(Game1.random.Next(position.X - TileSize * 2, position.X + TileSize * 2), Game1.random.Next(position.Y - TileSize * 2, position.Y + TileSize * 2));
							num4++;
						}
						while (isCollidingWithMap(targetPosition) && num4 < 5);
					}
					_ = targetPosition;
					Vector2 vector = ((!targetPosition.Equals(Point.Zero)) ? new Vector2(targetPosition.X, targetPosition.Y) : playerPosition);
					if (playingWithAbigail && vector.Equals(playerPosition))
					{
						double num5 = Math.Sqrt(Math.Pow((float)position.X - vector.X, 2.0) - Math.Pow((float)position.Y - vector.Y, 2.0));
						double num6 = Math.Sqrt(Math.Pow((float)position.X - player2Position.X, 2.0) - Math.Pow((float)position.Y - player2Position.Y, 2.0));
						if (num6 < num5)
						{
							vector = player2Position;
						}
					}
					if (gopherRunning)
					{
						vector = new Vector2(gopherBox.X, gopherBox.Y);
					}
					if (Game1.random.NextDouble() < 0.001)
					{
						oppositeMotionGuy = !oppositeMotionGuy;
					}
					if ((type == 6 && !oppositeMotionGuy) || Math.Abs(vector.X - (float)position.X) > Math.Abs(vector.Y - (float)position.Y))
					{
						if (vector.X + (float)speed < (float)position.X && (movedLastTurn || movementDirection != 3))
						{
							movementDirection = 3;
						}
						else if (vector.X > (float)(position.X + speed) && (movedLastTurn || movementDirection != 1))
						{
							movementDirection = 1;
						}
						else if (vector.Y > (float)(position.Y + speed) && (movedLastTurn || movementDirection != 2))
						{
							movementDirection = 2;
						}
						else if (vector.Y + (float)speed < (float)position.Y && (movedLastTurn || movementDirection != 0))
						{
							movementDirection = 0;
						}
					}
					else if (vector.Y > (float)(position.Y + speed) && (movedLastTurn || movementDirection != 2))
					{
						movementDirection = 2;
					}
					else if (vector.Y + (float)speed < (float)position.Y && (movedLastTurn || movementDirection != 0))
					{
						movementDirection = 0;
					}
					else if (vector.X + (float)speed < (float)position.X && (movedLastTurn || movementDirection != 3))
					{
						movementDirection = 3;
					}
					else if (vector.X > (float)(position.X + speed) && (movedLastTurn || movementDirection != 1))
					{
						movementDirection = 1;
					}
					movedLastTurn = false;
					Rectangle rectangle = position;
					switch (movementDirection)
					{
					case 0:
						rectangle.Y -= speed;
						break;
					case 1:
						rectangle.X += speed;
						break;
					case 2:
						rectangle.Y += speed;
						break;
					case 3:
						rectangle.X -= speed;
						break;
					}
					if (zombieModeTimer > 0)
					{
						rectangle.X = position.X - (rectangle.X - position.X);
						rectangle.Y = position.Y - (rectangle.Y - position.Y);
					}
					if (type == 2)
					{
						for (int num7 = monsters.Count - 1; num7 >= 0; num7--)
						{
							if (monsters[num7].type == 6 && monsters[num7].special && monsters[num7].position.Intersects(rectangle))
							{
								addGuts(monsters[num7].position.Location, monsters[num7].type);
								Game1.playSound("Cowboy_monsterDie");
								monsters.RemoveAt(num7);
							}
						}
					}
					if (isCollidingWithMapForMonsters(rectangle) || isCollidingWithMonster(rectangle, this) || !(deathTimer <= 0f))
					{
						break;
					}
					ticksSinceLastMovement = 0;
					position = rectangle;
					movedLastTurn = true;
					if (!position.Contains((int)vector.X + TileSize / 2, (int)vector.Y + TileSize / 2))
					{
						break;
					}
					targetPosition = Point.Zero;
					if ((type == 0 || type == 3) && uninterested)
					{
						targetPosition = new Point(Game1.random.Next(2, 14) * TileSize, Game1.random.Next(2, 14) * TileSize);
						if (Game1.random.NextDouble() < 0.5)
						{
							uninterested = false;
							targetPosition = Point.Zero;
						}
					}
					if (type == 6 && !invisible)
					{
						temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(352, 1728, 16, 16), 60f, 3, 0, new Vector2(position.X, position.Y) + topLeftScreenCoordinate, flicker: false, flipped: false, (float)position.Y / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
						{
							endFunction = spikeyEndBehavior
						});
						invisible = true;
					}
					break;
				}
				case 1:
				case 4:
				{
					if (ticksSinceLastMovement > 20)
					{
						int num = 0;
						do
						{
							oppositeMotionGuy = !oppositeMotionGuy;
							targetPosition = new Point(Game1.random.Next(position.X - TileSize * 2, position.X + TileSize * 2), Game1.random.Next(position.Y - TileSize * 2, position.Y + TileSize * 2));
							num++;
						}
						while (isCollidingWithMap(targetPosition) && num < 5);
					}
					_ = targetPosition;
					Vector2 vector = ((!targetPosition.Equals(Point.Zero)) ? new Vector2(targetPosition.X, targetPosition.Y) : playerPosition);
					Vector2 velocityTowardPoint = Utility.getVelocityTowardPoint(position.Location, vector + new Vector2(TileSize / 2, TileSize / 2), speed);
					float num2 = ((velocityTowardPoint.X != 0f && velocityTowardPoint.Y != 0f) ? 1.5f : 1f);
					if (velocityTowardPoint.X > acceleration.X)
					{
						acceleration.X += 0.1f * num2;
					}
					if (velocityTowardPoint.X < acceleration.X)
					{
						acceleration.X -= 0.1f * num2;
					}
					if (velocityTowardPoint.Y > acceleration.Y)
					{
						acceleration.Y += 0.1f * num2;
					}
					if (velocityTowardPoint.Y < acceleration.Y)
					{
						acceleration.Y -= 0.1f * num2;
					}
					Rectangle r = new Rectangle(position.X + (int)Math.Ceiling(acceleration.X), position.Y + (int)Math.Ceiling(acceleration.Y), TileSize, TileSize);
					if (!isCollidingWithMonster(r, this) && deathTimer <= 0f)
					{
						ticksSinceLastMovement = 0;
						position.X += (int)Math.Ceiling(acceleration.X);
						position.Y += (int)Math.Ceiling(acceleration.Y);
						if (position.Contains((int)vector.X + TileSize / 2, (int)vector.Y + TileSize / 2))
						{
							targetPosition = Point.Zero;
						}
					}
					break;
				}
				}
				return false;
			}

			public void spikeyEndBehavior(int extraInfo)
			{
				invisible = false;
				health += 5;
				special = true;
			}
		}

		public class Dracula : CowboyMonster
		{
			public const int gloatingPhase = -1;

			public const int walkRandomlyAndShootPhase = 0;

			public const int spreadShotPhase = 1;

			public const int summonDemonPhase = 2;

			public const int summonMummyPhase = 3;

			public int phase = -1;

			public int phaseInternalTimer;

			public int phaseInternalCounter;

			public int shootTimer;

			public int fullHealth;

			public Point homePosition;

			public Dracula()
				: base(-2, new Point(8 * TileSize, 8 * TileSize))
			{
				homePosition = position.Location;
				position.Y += TileSize * 4;
				health = 350;
				fullHealth = health;
				phase = -1;
				phaseInternalTimer = 4000;
				speed = 2;
			}

			public override void draw(SpriteBatch b)
			{
				if (phase != -1)
				{
					b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y + 16 * TileSize + 3, (int)((float)(16 * TileSize) * ((float)health / (float)fullHealth)), TileSize / 3), new Color(188, 51, 74));
				}
				if (flashColorTimer > 0f)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(464, 1696, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f);
					return;
				}
				int num = phase;
				if (num == -1 || (uint)(num - 1) <= 2u)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(592 + phaseInternalTimer / 100 % 3 * 16, 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f);
					if (phase == -1)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, (float)(position.Y + TileSize) + (float)Math.Sin((float)phaseInternalTimer / 1000f) * 3f), new Rectangle(528, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X - TileSize / 2, position.Y - TileSize * 2), new Rectangle(608, 1728, 32, 32), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f);
					}
				}
				else
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(592 + phaseInternalTimer / 100 % 2 * 16, 1712, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f);
				}
			}

			public override int getLootDrop()
			{
				return -1;
			}

			public override bool takeDamage(int damage)
			{
				if (phase == -1)
				{
					return false;
				}
				health -= damage;
				if (health < 0)
				{
					return true;
				}
				flashColorTimer = 100f;
				Game1.playSound("cowboy_monsterhit");
				return false;
			}

			public override bool move(Vector2 playerPosition, GameTime time)
			{
				if (flashColorTimer > 0f)
				{
					flashColorTimer -= time.ElapsedGameTime.Milliseconds;
				}
				phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
				switch (phase)
				{
				case -1:
					if (phaseInternalTimer <= 0)
					{
						phaseInternalCounter = 0;
						if (Game1.soundBank != null && Game1.musicWaveBankState == Game1.MusicWaveBankState.Created)
						{
							outlawSong = Game1.soundBank.GetCue("cowboy_boss");
							outlawSong.Play();
						}
						phase = 0;
					}
					break;
				case 0:
				{
					if (phaseInternalCounter == 0)
					{
						phaseInternalCounter++;
						phaseInternalTimer = Game1.random.Next(3000, 7000);
					}
					if (phaseInternalTimer < 0)
					{
						phaseInternalCounter = 0;
						phase = Game1.random.Next(1, 4);
						phaseInternalTimer = 9999;
					}
					Vector2 vector = playerPosition;
					if (!(deathTimer <= 0f))
					{
						break;
					}
					int num = -1;
					if (Math.Abs(vector.X - (float)position.X) > Math.Abs(vector.Y - (float)position.Y))
					{
						if (vector.X + (float)speed < (float)position.X)
						{
							num = 3;
						}
						else if (vector.X > (float)(position.X + speed))
						{
							num = 1;
						}
						else if (vector.Y > (float)(position.Y + speed))
						{
							num = 2;
						}
						else if (vector.Y + (float)speed < (float)position.Y)
						{
							num = 0;
						}
					}
					else if (vector.Y > (float)(position.Y + speed))
					{
						num = 2;
					}
					else if (vector.Y + (float)speed < (float)position.Y)
					{
						num = 0;
					}
					else if (vector.X + (float)speed < (float)position.X)
					{
						num = 3;
					}
					else if (vector.X > (float)(position.X + speed))
					{
						num = 1;
					}
					Rectangle rectangle = position;
					switch (num)
					{
					case 0:
						rectangle.Y -= speed;
						break;
					case 1:
						rectangle.X += speed;
						break;
					case 2:
						rectangle.Y += speed;
						break;
					case 3:
						rectangle.X -= speed;
						break;
					}
					rectangle.X = position.X - (rectangle.X - position.X);
					rectangle.Y = position.Y - (rectangle.Y - position.Y);
					if (!isCollidingWithMapForMonsters(rectangle) && !isCollidingWithMonster(rectangle, this))
					{
						position = rectangle;
					}
					shootTimer -= time.ElapsedGameTime.Milliseconds;
					if (shootTimer < 0)
					{
						Vector2 p = Utility.getVelocityTowardPoint(new Point(position.X + TileSize / 2, position.Y), playerPosition + new Vector2(TileSize / 2, TileSize / 2), 8f);
						if (playerMovementDirections.Count > 0)
						{
							p = Utility.getTranslatedVector2(p, playerMovementDirections.Last(), 3f);
						}
						enemyBullets.Add(new CowboyBullet(new Point(position.X + TileSize / 2, position.Y + TileSize / 2), new Point((int)p.X, (int)p.Y), 1));
						shootTimer = 250;
						Game1.playSound("Cowboy_gunshot");
					}
					break;
				}
				case 2:
				case 3:
					if (phaseInternalCounter == 0)
					{
						Point location2 = position.Location;
						if (position.X > homePosition.X + 6)
						{
							position.X -= 6;
						}
						else if (position.X < homePosition.X - 6)
						{
							position.X += 6;
						}
						if (position.Y > homePosition.Y + 6)
						{
							position.Y -= 6;
						}
						else if (position.Y < homePosition.Y - 6)
						{
							position.Y += 6;
						}
						if (position.Location.Equals(location2))
						{
							phaseInternalCounter++;
							phaseInternalTimer = 1500;
						}
					}
					else if (phaseInternalCounter == 1 && phaseInternalTimer < 0)
					{
						summonEnemies(new Point(position.X + TileSize / 2, position.Y + TileSize / 2), Game1.random.Next(0, 5));
						if (Game1.random.NextDouble() < 0.4)
						{
							phase = 0;
							phaseInternalCounter = 0;
						}
						else
						{
							phaseInternalTimer = 2000;
						}
					}
					break;
				case 1:
					if (phaseInternalCounter == 0)
					{
						Point location = position.Location;
						if (position.X > homePosition.X + 6)
						{
							position.X -= 6;
						}
						else if (position.X < homePosition.X - 6)
						{
							position.X += 6;
						}
						if (position.Y > homePosition.Y + 6)
						{
							position.Y -= 6;
						}
						else if (position.Y < homePosition.Y - 6)
						{
							position.Y += 6;
						}
						if (position.Location.Equals(location))
						{
							phaseInternalCounter++;
							phaseInternalTimer = 1500;
						}
					}
					else if (phaseInternalCounter == 1)
					{
						if (phaseInternalTimer < 0)
						{
							phaseInternalCounter++;
							phaseInternalTimer = 2000;
							shootTimer = 200;
							fireSpread(new Point(position.X + TileSize / 2, position.Y + TileSize / 2), 0.0);
						}
					}
					else if (phaseInternalCounter == 2)
					{
						shootTimer -= time.ElapsedGameTime.Milliseconds;
						if (shootTimer < 0)
						{
							fireSpread(new Point(position.X + TileSize / 2, position.Y + TileSize / 2), 0.0);
							shootTimer = 200;
						}
						if (phaseInternalTimer < 0)
						{
							phaseInternalCounter++;
							phaseInternalTimer = 500;
						}
					}
					else if (phaseInternalCounter == 3)
					{
						if (phaseInternalTimer < 0)
						{
							phaseInternalTimer = 2000;
							shootTimer = 200;
							phaseInternalCounter++;
							Vector2 velocityTowardPoint = Utility.getVelocityTowardPoint(new Point(position.X + TileSize / 2, position.Y), playerPosition + new Vector2(TileSize / 2, TileSize / 2), 8f);
							enemyBullets.Add(new CowboyBullet(new Point(position.X + TileSize / 2, position.Y + TileSize / 2), new Point((int)velocityTowardPoint.X, (int)velocityTowardPoint.Y), 1));
							Game1.playSound("Cowboy_gunshot");
						}
					}
					else
					{
						if (phaseInternalCounter != 4)
						{
							break;
						}
						shootTimer -= time.ElapsedGameTime.Milliseconds;
						if (shootTimer < 0)
						{
							Vector2 velocityTowardPoint2 = Utility.getVelocityTowardPoint(new Point(position.X + TileSize / 2, position.Y), playerPosition + new Vector2(TileSize / 2, TileSize / 2), 8f);
							velocityTowardPoint2.X += Game1.random.Next(-1, 2);
							velocityTowardPoint2.Y += Game1.random.Next(-1, 2);
							enemyBullets.Add(new CowboyBullet(new Point(position.X + TileSize / 2, position.Y + TileSize / 2), new Point((int)velocityTowardPoint2.X, (int)velocityTowardPoint2.Y), 1));
							Game1.playSound("Cowboy_gunshot");
							shootTimer = 200;
						}
						if (phaseInternalTimer < 0)
						{
							if (Game1.random.NextDouble() < 0.4)
							{
								phase = 0;
								phaseInternalCounter = 0;
							}
							else
							{
								phaseInternalTimer = 500;
								phaseInternalCounter = 1;
							}
						}
					}
					break;
				}
				return false;
			}

			public void fireSpread(Point origin, double offsetAngle)
			{
				Vector2[] surroundingTileLocationsArray = Utility.getSurroundingTileLocationsArray(new Vector2(origin.X, origin.Y));
				for (int i = 0; i < surroundingTileLocationsArray.Length; i++)
				{
					Vector2 endingPoint = surroundingTileLocationsArray[i];
					Vector2 velocityTowardPoint = Utility.getVelocityTowardPoint(origin, endingPoint, 6f);
					if (offsetAngle > 0.0)
					{
						offsetAngle /= 2.0;
						velocityTowardPoint.X = (float)(Math.Cos(offsetAngle) * (double)(endingPoint.X - (float)origin.X) - Math.Sin(offsetAngle) * (double)(endingPoint.Y - (float)origin.Y) + (double)origin.X);
						velocityTowardPoint.Y = (float)(Math.Sin(offsetAngle) * (double)(endingPoint.X - (float)origin.X) + Math.Cos(offsetAngle) * (double)(endingPoint.Y - (float)origin.Y) + (double)origin.Y);
						velocityTowardPoint = Utility.getVelocityTowardPoint(origin, velocityTowardPoint, 8f);
					}
					enemyBullets.Add(new CowboyBullet(origin, new Point((int)velocityTowardPoint.X, (int)velocityTowardPoint.Y), 1));
				}
				Game1.playSound("Cowboy_gunshot");
			}

			public void summonEnemies(Point origin, int which)
			{
				if (!isCollidingWithMonster(new Rectangle(origin.X - TileSize - TileSize / 2, origin.Y, TileSize, TileSize), null))
				{
					monsters.Add(new CowboyMonster(which, new Point(origin.X - TileSize - TileSize / 2, origin.Y)));
				}
				if (!isCollidingWithMonster(new Rectangle(origin.X + TileSize + TileSize / 2, origin.Y, TileSize, TileSize), null))
				{
					monsters.Add(new CowboyMonster(which, new Point(origin.X + TileSize + TileSize / 2, origin.Y)));
				}
				if (!isCollidingWithMonster(new Rectangle(origin.X, origin.Y + TileSize + TileSize / 2, TileSize, TileSize), null))
				{
					monsters.Add(new CowboyMonster(which, new Point(origin.X, origin.Y + TileSize + TileSize / 2)));
				}
				if (!isCollidingWithMonster(new Rectangle(origin.X, origin.Y - TileSize - TileSize * 3 / 4, TileSize, TileSize), null))
				{
					monsters.Add(new CowboyMonster(which, new Point(origin.X, origin.Y - TileSize - TileSize * 3 / 4)));
				}
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, topLeftScreenCoordinate + new Vector2(origin.X - TileSize - TileSize / 2, origin.Y), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					delayBeforeAnimationStart = Game1.random.Next(800)
				});
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, topLeftScreenCoordinate + new Vector2(origin.X + TileSize + TileSize / 2, origin.Y), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					delayBeforeAnimationStart = Game1.random.Next(800)
				});
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, topLeftScreenCoordinate + new Vector2(origin.X, origin.Y - TileSize - TileSize * 3 / 4), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					delayBeforeAnimationStart = Game1.random.Next(800)
				});
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, topLeftScreenCoordinate + new Vector2(origin.X, origin.Y + TileSize + TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					delayBeforeAnimationStart = Game1.random.Next(800)
				});
				Game1.playSound("Cowboy_monsterDie");
			}
		}

		public class Outlaw : CowboyMonster
		{
			public const int talkingPhase = -1;

			public const int hidingPhase = 0;

			public const int dartOutAndShootPhase = 1;

			public const int runAndGunPhase = 2;

			public const int runGunAndPantPhase = 3;

			public const int shootAtPlayerPhase = 4;

			public int phase;

			public int phaseCountdown;

			public int shootTimer;

			public int phaseInternalTimer;

			public int phaseInternalCounter;

			public bool dartLeft;

			public int fullHealth;

			public Point homePosition;

			public Outlaw(Point position, int health)
				: base(-1, position)
			{
				homePosition = position;
				base.health = health;
				fullHealth = health;
				phaseCountdown = 4000;
				phase = -1;
			}

			public override void draw(SpriteBatch b)
			{
				b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y + 16 * TileSize + 3, (int)((float)(16 * TileSize) * ((float)health / (float)fullHealth)), TileSize / 3), new Color(188, 51, 74));
				if (flashColorTimer > 0f)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(496, 1696, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f + 0.001f);
					return;
				}
				int num = phase;
				if ((uint)(num - -1) <= 1u)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(560 + ((phaseCountdown / 250 % 2 == 0) ? 16 : 0), 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f + 0.001f);
					if (phase == -1 && phaseCountdown > 1000)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X - TileSize / 2, position.Y - TileSize * 2), new Rectangle(576 + ((whichWave > 5) ? 32 : 0), 1792, 32, 32), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f + 0.001f);
					}
				}
				else if (phase == 3 && phaseInternalCounter == 2)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(560 + ((phaseCountdown / 250 % 2 == 0) ? 16 : 0), 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f + 0.001f);
				}
				else
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(592 + ((phaseCountdown / 80 % 2 == 0) ? 16 : 0), 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f + 0.001f);
				}
			}

			public override bool move(Vector2 playerPosition, GameTime time)
			{
				if (flashColorTimer > 0f)
				{
					flashColorTimer -= time.ElapsedGameTime.Milliseconds;
				}
				phaseCountdown -= time.ElapsedGameTime.Milliseconds;
				if (position.X > 17 * TileSize || position.X < -TileSize)
				{
					position.X = 16 * TileSize / 2;
				}
				switch (phase)
				{
				case -1:
				case 0:
					if (phaseCountdown >= 0)
					{
						break;
					}
					phase = Game1.random.Next(1, 5);
					dartLeft = playerPosition.X < (float)position.X;
					if (playerPosition.X > (float)(7 * TileSize) && playerPosition.X < (float)(9 * TileSize))
					{
						if (Game1.random.NextDouble() < 0.66 || phase == 2)
						{
							phase = 4;
						}
					}
					else if (phase == 4)
					{
						phase = 3;
					}
					phaseInternalCounter = 0;
					phaseInternalTimer = 0;
					break;
				case 4:
				{
					int num = (dartLeft ? (-3) : 3);
					if (phaseInternalCounter == 0 && (!(playerPosition.X > (float)(7 * TileSize)) || !(playerPosition.X < (float)(9 * TileSize))))
					{
						phaseInternalCounter = 1;
						phaseInternalTimer = Game1.random.Next(500, 1500);
						break;
					}
					if (Math.Abs(position.Location.X - homePosition.X + TileSize / 2) < TileSize * 7 + 12 && phaseInternalCounter == 0)
					{
						position.X += num;
						break;
					}
					if (phaseInternalCounter == 2)
					{
						num = (dartLeft ? (-4) : 4);
						position.X -= num;
						if (Math.Abs(position.X - homePosition.X) < 4)
						{
							position.X = homePosition.X;
							phase = 0;
							phaseCountdown = Game1.random.Next(1000, 2000);
						}
						break;
					}
					if (phaseInternalCounter == 0)
					{
						phaseInternalCounter++;
						phaseInternalTimer = Game1.random.Next(1000, 2000);
					}
					phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
					shootTimer -= time.ElapsedGameTime.Milliseconds;
					if (shootTimer < 0)
					{
						Vector2 velocityTowardPoint = Utility.getVelocityTowardPoint(new Point(position.X + TileSize / 2, position.Y), playerPosition + new Vector2(TileSize / 2, TileSize / 2), 8f);
						enemyBullets.Add(new CowboyBullet(new Point(position.X + TileSize / 2, position.Y - TileSize / 2), new Point((int)velocityTowardPoint.X, (int)velocityTowardPoint.Y), 1));
						shootTimer = 120;
						Game1.playSound("Cowboy_gunshot");
					}
					if (phaseInternalTimer <= 0)
					{
						phaseInternalCounter++;
					}
					break;
				}
				case 1:
				{
					int num = (dartLeft ? (-3) : 3);
					if (Math.Abs(position.Location.X - homePosition.X + TileSize / 2) < TileSize * 2 + 12 && phaseInternalCounter == 0)
					{
						position.X += num;
						if (position.X > 256)
						{
							phaseInternalCounter = 2;
						}
						break;
					}
					if (phaseInternalCounter == 2)
					{
						position.X -= num;
						if (Math.Abs(position.X - homePosition.X) < 4)
						{
							position.X = homePosition.X;
							phase = 0;
							phaseCountdown = Game1.random.Next(1000, 2000);
						}
						break;
					}
					if (phaseInternalCounter == 0)
					{
						phaseInternalCounter++;
						phaseInternalTimer = Game1.random.Next(1000, 2000);
					}
					phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
					shootTimer -= time.ElapsedGameTime.Milliseconds;
					if (shootTimer < 0)
					{
						enemyBullets.Add(new CowboyBullet(new Point(position.X + TileSize / 2, position.Y - TileSize / 2), new Point(Game1.random.Next(-2, 3), -8), 1));
						shootTimer = 150;
						Game1.playSound("Cowboy_gunshot");
					}
					if (phaseInternalTimer <= 0)
					{
						phaseInternalCounter++;
					}
					break;
				}
				case 2:
					if (phaseInternalCounter == 2)
					{
						if (position.X < homePosition.X)
						{
							position.X += 4;
						}
						else
						{
							position.X -= 4;
						}
						if (Math.Abs(position.X - homePosition.X) < 5)
						{
							position.X = homePosition.X;
							phase = 0;
							phaseCountdown = Game1.random.Next(1000, 2000);
						}
						return false;
					}
					if (phaseInternalCounter == 0)
					{
						phaseInternalCounter++;
						phaseInternalTimer = Game1.random.Next(4000, 7000);
					}
					phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
					if ((float)position.X > playerPosition.X && (float)position.X - playerPosition.X > 3f)
					{
						position.X -= 2;
					}
					else if ((float)position.X < playerPosition.X && playerPosition.X - (float)position.X > 3f)
					{
						position.X += 2;
					}
					shootTimer -= time.ElapsedGameTime.Milliseconds;
					if (shootTimer < 0)
					{
						enemyBullets.Add(new CowboyBullet(new Point(position.X + TileSize / 2, position.Y - TileSize / 2), new Point(Game1.random.Next(-1, 2), -8), 1));
						shootTimer = 250;
						if (fullHealth > 50)
						{
							shootTimer -= 50;
						}
						if (Game1.random.NextDouble() < 0.2)
						{
							shootTimer = 150;
						}
						Game1.playSound("Cowboy_gunshot");
					}
					if (phaseInternalTimer <= 0)
					{
						phaseInternalCounter++;
					}
					break;
				case 3:
				{
					if (phaseInternalCounter == 0)
					{
						phaseInternalCounter++;
						phaseInternalTimer = Game1.random.Next(3000, 6500);
						break;
					}
					if (phaseInternalCounter == 2)
					{
						phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
						if (phaseInternalTimer <= 0)
						{
							phaseInternalCounter++;
						}
						break;
					}
					if (phaseInternalCounter == 3)
					{
						if (position.X < homePosition.X)
						{
							position.X += 4;
						}
						else
						{
							position.X -= 4;
						}
						if (Math.Abs(position.X - homePosition.X) < 5)
						{
							position.X = homePosition.X;
							phase = 0;
							phaseCountdown = Game1.random.Next(1000, 2000);
						}
						break;
					}
					int num = (dartLeft ? (-3) : 3);
					position.X += num;
					if (position.X < TileSize || position.X > 15 * TileSize)
					{
						dartLeft = !dartLeft;
					}
					shootTimer -= time.ElapsedGameTime.Milliseconds;
					if (shootTimer < 0)
					{
						enemyBullets.Add(new CowboyBullet(new Point(position.X + TileSize / 2, position.Y - TileSize / 2), new Point(Game1.random.Next(-1, 2), -8), 1));
						shootTimer = 250;
						if (fullHealth > 50)
						{
							shootTimer -= 50;
						}
						if (Game1.random.NextDouble() < 0.2)
						{
							shootTimer = 150;
						}
						Game1.playSound("Cowboy_gunshot");
					}
					phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
					if (phaseInternalTimer <= 0)
					{
						if (phase == 2)
						{
							phaseInternalCounter = 3;
							break;
						}
						phaseInternalTimer = 3000;
						phaseInternalCounter++;
					}
					break;
				}
				}
				if (position.X <= 16 * TileSize)
				{
					_ = position.X;
					_ = 0;
				}
				return false;
			}

			public override int getLootDrop()
			{
				return 8;
			}

			public override bool takeDamage(int damage)
			{
				if (Math.Abs(position.X - homePosition.X) < 5)
				{
					return false;
				}
				health -= damage;
				if (health < 0)
				{
					return true;
				}
				flashColorTimer = 150f;
				Game1.playSound("cowboy_monsterhit");
				return false;
			}
		}

		private const int joystickPixelScale = 4;

		public ClickableTextureComponent leftJoystick;

		public ClickableTextureComponent rightJoystick;

		public ClickableTextureComponent upperRightCloseButton;

		public ClickableComponent buttonRestart;

		public ClickableComponent buttonQuit;

		public ClickableComponent buttonPowerup;

		private int _maxJoystickMoveDistance = 80;

		private const int MAX_JOYSTICK_HITAREA_RADIUS = 350;

		private const int MIN_JOYSTICK_MOVE_THRESHOLD = 5;

		private bool _lastLeftJoystickHeld;

		private bool _lastRightJoystickHeld;

		private bool _leftJoystickHeld;

		private bool _rightJoystickHeld;

		private Vector2 _joystickLeftStartPosition;

		private Vector2 _joystickRightStartPosition;

		private Vector2 _joystickLeftTapStartPoint = Vector2.Zero;

		private Vector2 _joystickRightTapStartPoint = Vector2.Zero;

		private Vector2 _joystickLeftTapPoint = Vector2.Zero;

		private Vector2 _joystickRightTapPoint = Vector2.Zero;

		private Vector2 _joystickLeftLastTapPoint = Vector2.Zero;

		private Vector2 _joystickRightLastTapPoint = Vector2.Zero;

		public const int mapWidth = 16;

		public const int mapHeight = 16;

		public const int pixelZoom = 3;

		public const int bulletSpeed = 8;

		public const double lootChance = 0.05;

		public const double coinChance = 0.05;

		public int lootDuration = 7500;

		public int powerupDuration = 10000;

		public const int abigailPortraitDuration = 6000;

		public const float playerSpeed = 3f;

		public const int baseTileSize = 16;

		public const int orcSpeed = 2;

		public const int ogreSpeed = 1;

		public const int ghostSpeed = 3;

		public const int spikeySpeed = 3;

		public const int orcHealth = 1;

		public const int ghostHealth = 1;

		public const int ogreHealth = 3;

		public const int spikeyHealth = 2;

		public const int cactusDanceDelay = 800;

		public const int playerMotionDelay = 100;

		public const int playerFootStepDelay = 200;

		public const int deathDelay = 3000;

		public const int MAP_BARRIER1 = 0;

		public const int MAP_BARRIER2 = 1;

		public const int MAP_ROCKY1 = 2;

		public const int MAP_DESERT = 3;

		public const int MAP_GRASSY = 4;

		public const int MAP_CACTUS = 5;

		public const int MAP_FENCE = 7;

		public const int MAP_TRENCH1 = 8;

		public const int MAP_TRENCH2 = 9;

		public const int MAP_BRIDGE = 10;

		public const int orc = 0;

		public const int ghost = 1;

		public const int ogre = 2;

		public const int mummy = 3;

		public const int devil = 4;

		public const int mushroom = 5;

		public const int spikey = 6;

		public const int dracula = 7;

		public const int desert = 0;

		public const int woods = 2;

		public const int graveyard = 1;

		public const int POWERUP_LOG = -1;

		public const int POWERUP_SKULL = -2;

		public const int coin1 = 0;

		public const int coin5 = 1;

		public const int POWERUP_SPREAD = 2;

		public const int POWERUP_RAPIDFIRE = 3;

		public const int POWERUP_NUKE = 4;

		public const int POWERUP_ZOMBIE = 5;

		public const int POWERUP_SPEED = 6;

		public const int POWERUP_SHOTGUN = 7;

		public const int POWERUP_LIFE = 8;

		public const int POWERUP_TELEPORT = 9;

		public const int POWERUP_SHERRIFF = 10;

		public const int POWERUP_HEART = -3;

		public const int ITEM_FIRESPEED1 = 0;

		public const int ITEM_FIRESPEED2 = 1;

		public const int ITEM_FIRESPEED3 = 2;

		public const int ITEM_RUNSPEED1 = 3;

		public const int ITEM_RUNSPEED2 = 4;

		public const int ITEM_LIFE = 5;

		public const int ITEM_AMMO1 = 6;

		public const int ITEM_AMMO2 = 7;

		public const int ITEM_AMMO3 = 8;

		public const int ITEM_SPREADPISTOL = 9;

		public const int ITEM_STAR = 10;

		public const int ITEM_SKULL = 11;

		public const int ITEM_LOG = 12;

		public const int option_retry = 0;

		public const int option_quit = 1;

		public int runSpeedLevel;

		public int fireSpeedLevel;

		public int ammoLevel;

		public int whichRound;

		public bool spreadPistol;

		public const int waveDuration = 80000;

		public const int betweenWaveDuration = 5000;

		public static List<CowboyMonster> monsters = new List<CowboyMonster>();

		protected HashSet<Vector2> _borderTiles = new HashSet<Vector2>();

		public Vector2 playerPosition;

		public static Vector2 player2Position = default(Vector2);

		public Rectangle playerBoundingBox;

		public Rectangle merchantBox;

		public Rectangle player2BoundingBox;

		public Rectangle noPickUpBox;

		public static List<int> playerMovementDirections = new List<int>();

		public static List<int> playerShootingDirections = new List<int>();

		public List<int> player2MovementDirections = new List<int>();

		public List<int> player2ShootingDirections = new List<int>();

		public int shootingDelay = 300;

		public int shotTimer;

		public int motionPause;

		public int bulletDamage;

		public int speedBonus;

		public int fireRateBonus;

		public int lives = 3;

		public int coins;

		public int score;

		public int player2deathtimer;

		public int player2invincibletimer;

		public List<CowboyBullet> bullets = new List<CowboyBullet>();

		public static List<CowboyBullet> enemyBullets = new List<CowboyBullet>();

		public static int[,] map = new int[16, 16];

		public static int[,] nextMap = new int[16, 16];

		public List<Point>[] spawnQueue = new List<Point>[4];

		public static Vector2 topLeftScreenCoordinate;

		public float cactusDanceTimer;

		public float playerMotionAnimationTimer;

		public float playerFootstepSoundTimer = 200f;

		public behaviorAfterMotionPause behaviorAfterPause;

		public List<Vector2> monsterChances = new List<Vector2>
		{
			new Vector2(0.014f, 0.4f),
			Vector2.Zero,
			Vector2.Zero,
			Vector2.Zero,
			Vector2.Zero,
			Vector2.Zero,
			Vector2.Zero
		};

		public Rectangle shoppingCarpetNoPickup;

		public Dictionary<int, int> activePowerups = new Dictionary<int, int>();

		public static List<CowboyPowerup> powerups = new List<CowboyPowerup>();

		public string AbigailDialogue = "";

		public static List<TemporaryAnimatedSprite> temporarySprites = new List<TemporaryAnimatedSprite>();

		public CowboyPowerup heldItem;

		public static int world = 0;

		public int gameOverOption;

		public int gamerestartTimer;

		public int player2TargetUpdateTimer;

		public int player2shotTimer;

		public int player2AnimationTimer;

		public int fadethenQuitTimer;

		public int abigailPortraitYposition;

		public int abigailPortraitTimer;

		public int abigailPortraitExpression;

		public static int waveTimer = 80000;

		public static int betweenWaveTimer = 5000;

		public static int whichWave;

		public static int monsterConfusionTimer;

		public static int zombieModeTimer;

		public static int shoppingTimer;

		public static int holdItemTimer;

		public static int itemToHold;

		public static int newMapPosition;

		public static int playerInvincibleTimer;

		public static int screenFlash;

		public static int gopherTrainPosition;

		public static int endCutsceneTimer;

		public static int endCutscenePhase;

		public static int startTimer;

		public static float deathTimer;

		public static bool onStartMenu;

		public static bool shopping;

		public static bool gopherRunning;

		public static bool store;

		public static bool merchantLeaving;

		public static bool merchantArriving;

		public static bool merchantShopOpen;

		public static bool waitingForPlayerToMoveDownAMap;

		public static bool scrollingMap;

		public static bool hasGopherAppeared;

		public static bool shootoutLevel;

		public static bool gopherTrain;

		public static bool playerJumped;

		public static bool endCutscene;

		public static bool gameOver;

		public static bool playingWithAbigail;

		public static bool beatLevelWithAbigail;

		public Dictionary<Rectangle, int> storeItems = new Dictionary<Rectangle, int>();

		public bool quit;

		public bool died;

		public static Rectangle gopherBox;

		public Point gopherMotion;

		internal static ICue overworldSong;

		internal static ICue outlawSong;

		internal static ICue zombieSong;

		protected Dictionary<GameKeys, List<Keys>> _binds;

		protected HashSet<GameKeys> _buttonHeldState = new HashSet<GameKeys>();

		protected Dictionary<GameKeys, int> _buttonHeldFrames;

		private int player2FootstepSoundTimer;

		public CowboyMonster targetMonster;

		public static int TileSize => 48;

		public bool LoadGame()
		{
			if (playingWithAbigail)
			{
				return false;
			}
			if (Game1.player.jotpkProgress.Value == null)
			{
				return false;
			}
			JOTPKProgress value = Game1.player.jotpkProgress.Value;
			ammoLevel = value.ammoLevel.Value;
			bulletDamage = value.bulletDamage.Value;
			coins = value.coins.Value;
			died = value.died.Value;
			fireSpeedLevel = value.fireSpeedLevel.Value;
			lives = value.lives.Value;
			score = value.score.Value;
			runSpeedLevel = value.runSpeedLevel.Value;
			spreadPistol = value.spreadPistol.Value;
			whichRound = value.whichRound.Value;
			whichWave = value.whichWave.Value;
			waveTimer = value.waveTimer.Value;
			world = value.world.Value;
			if (value.heldItem.Value != -100)
			{
				heldItem = new CowboyPowerup(value.heldItem.Value, Point.Zero, 9999);
			}
			monsterChances = new List<Vector2>(value.monsterChances);
			ApplyLevelSpecificStates();
			if (shootoutLevel)
			{
				playerPosition = new Vector2(8 * TileSize, 3 * TileSize);
				playerBoundingBox.X = (int)playerPosition.X + TileSize / 4;
				playerBoundingBox.Y = (int)playerPosition.Y + TileSize / 4;
				playerBoundingBox.Width = TileSize / 2;
				playerBoundingBox.Height = TileSize / 2;
			}
			return true;
		}

		public void SaveGame()
		{
			if (!playingWithAbigail)
			{
				if (Game1.player.jotpkProgress.Value == null)
				{
					Game1.player.jotpkProgress.Value = new JOTPKProgress();
				}
				JOTPKProgress value = Game1.player.jotpkProgress.Value;
				value.ammoLevel.Value = ammoLevel;
				value.bulletDamage.Value = bulletDamage;
				value.coins.Value = coins;
				value.died.Value = died;
				value.fireSpeedLevel.Value = fireSpeedLevel;
				value.lives.Value = lives;
				value.score.Value = score;
				value.runSpeedLevel.Value = runSpeedLevel;
				value.spreadPistol.Value = spreadPistol;
				value.whichRound.Value = whichRound;
				value.whichWave.Value = whichWave;
				value.waveTimer.Value = waveTimer;
				value.world.Value = world;
				value.monsterChances.Clear();
				value.monsterChances.AddRange(monsterChances);
				if (heldItem == null)
				{
					value.heldItem.Value = -100;
				}
				else
				{
					value.heldItem.Value = heldItem.which;
				}
			}
		}

		public AbigailGame(bool playingWithAbby = false)
		{
			reset(playingWithAbby);
			if (!playingWithAbigail && LoadGame())
			{
				map = getMap(whichWave);
			}
			_InitializeMobileControls();
		}

		public AbigailGame(int coins, int ammoLevel, int bulletDamage, int fireSpeedLevel, int runSpeedLevel, int lives, bool spreadPistol, int whichRound)
		{
			reset(playingWithAbby: false);
			this.coins = coins;
			this.ammoLevel = ammoLevel;
			this.bulletDamage = bulletDamage;
			this.fireSpeedLevel = fireSpeedLevel;
			this.runSpeedLevel = runSpeedLevel;
			this.lives = lives;
			this.spreadPistol = spreadPistol;
			this.whichRound = whichRound;
			ApplyNewGamePlus();
			SaveGame();
			onStartMenu = false;
			_InitializeMobileControls();
		}

		public void ApplyNewGamePlus()
		{
			monsterChances[0] = new Vector2(0.014f + (float)whichRound * 0.005f, 0.41f + (float)whichRound * 0.05f);
			monsterChances[4] = new Vector2(0.002f, 0.1f);
		}

		public void reset(bool playingWithAbby)
		{
			Rectangle r = new Rectangle(0, 0, 16, 16);
			_borderTiles = new HashSet<Vector2>(Utility.getBorderOfThisRectangle(r));
			died = false;
			topLeftScreenCoordinate = new Vector2(Game1.viewport.Width / 2 - 384, Game1.viewport.Height / 2 - 384);
			enemyBullets.Clear();
			holdItemTimer = 0;
			itemToHold = -1;
			merchantArriving = false;
			merchantLeaving = false;
			merchantShopOpen = false;
			monsterConfusionTimer = 0;
			monsters.Clear();
			newMapPosition = 16 * TileSize;
			scrollingMap = false;
			shopping = false;
			store = false;
			temporarySprites.Clear();
			waitingForPlayerToMoveDownAMap = false;
			waveTimer = 80000;
			whichWave = 0;
			zombieModeTimer = 0;
			bulletDamage = 1;
			deathTimer = 0f;
			shootoutLevel = false;
			betweenWaveTimer = 5000;
			gopherRunning = false;
			hasGopherAppeared = false;
			playerMovementDirections.Clear();
			outlawSong = null;
			overworldSong = null;
			endCutscene = false;
			endCutscenePhase = 0;
			endCutsceneTimer = 0;
			gameOver = false;
			deathTimer = 0f;
			playerInvincibleTimer = 0;
			playingWithAbigail = playingWithAbby;
			beatLevelWithAbigail = false;
			onStartMenu = true;
			startTimer = 0;
			powerups.Clear();
			world = 0;
			Game1.changeMusicTrack("none", track_interruptable: false, Game1.MusicContext.MiniGame);
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					if ((i == 0 || i == 15 || j == 0 || j == 15) && (i <= 6 || i >= 10) && (j <= 6 || j >= 10))
					{
						map[i, j] = 5;
					}
					else if (i == 0 || i == 15 || j == 0 || j == 15)
					{
						map[i, j] = ((Game1.random.NextDouble() < 0.15) ? 1 : 0);
					}
					else if (i == 1 || i == 14 || j == 1 || j == 14)
					{
						map[i, j] = 2;
					}
					else
					{
						map[i, j] = ((Game1.random.NextDouble() < 0.1) ? 4 : 3);
					}
				}
			}
			playerPosition = new Vector2(384f, 384f);
			playerBoundingBox.X = (int)playerPosition.X + TileSize / 4;
			playerBoundingBox.Y = (int)playerPosition.Y + TileSize / 4;
			playerBoundingBox.Width = TileSize / 2;
			playerBoundingBox.Height = TileSize / 2;
			if (playingWithAbigail)
			{
				onStartMenu = false;
				player2Position = new Vector2(432f, 384f);
				player2BoundingBox = new Rectangle(9 * TileSize, 8 * TileSize, TileSize, TileSize);
				betweenWaveTimer += 1500;
			}
			for (int k = 0; k < 4; k++)
			{
				spawnQueue[k] = new List<Point>();
			}
			noPickUpBox = new Rectangle(0, 0, TileSize, TileSize);
			merchantBox = new Rectangle(8 * TileSize, 0, TileSize, TileSize);
			newMapPosition = 16 * TileSize;
		}

		public float getMovementSpeed(float speed, int directions)
		{
			float num = speed;
			if (directions > 1)
			{
				num = Math.Max(1, (int)Math.Sqrt(2f * (num * num)) / 2);
			}
			return num;
		}

		public bool getPowerUp(CowboyPowerup c)
		{
			switch (c.which)
			{
			case -3:
				usePowerup(-3);
				break;
			case -2:
				usePowerup(-2);
				break;
			case -1:
				usePowerup(-1);
				break;
			case 0:
				coins++;
				Game1.playSound("Pickup_Coin15");
				break;
			case 1:
				coins += 5;
				Game1.playSound("Pickup_Coin15");
				break;
			case 8:
				lives++;
				Game1.playSound("cowboy_powerup");
				break;
			default:
			{
				if (heldItem == null)
				{
					heldItem = c;
					Game1.playSound("cowboy_powerup");
					break;
				}
				CowboyPowerup cowboyPowerup = heldItem;
				heldItem = c;
				noPickUpBox.Location = c.position;
				cowboyPowerup.position = c.position;
				powerups.Add(cowboyPowerup);
				Game1.playSound("cowboy_powerup");
				return true;
			}
			}
			return true;
		}

		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		public void usePowerup(int which)
		{
			if (activePowerups.ContainsKey(which))
			{
				activePowerups[which] = powerupDuration + 2000;
				return;
			}
			int num;
			switch (which)
			{
			case -3:
				itemToHold = 13;
				holdItemTimer = 4000;
				Game1.playSound("Cowboy_Secret");
				endCutscene = true;
				endCutsceneTimer = 4000;
				world = 0;
				if (!Game1.player.hasOrWillReceiveMail("Beat_PK"))
				{
					Game1.addMailForTomorrow("Beat_PK");
				}
				break;
			case -2:
				num = 11;
				goto IL_00cd;
			case -1:
				num = 12;
				goto IL_00cd;
			case 10:
			{
				usePowerup(7);
				usePowerup(3);
				usePowerup(6);
				for (int j = 0; j < activePowerups.Count; j++)
				{
					activePowerups[activePowerups.ElementAt(j).Key] *= 2;
				}
				break;
			}
			case 5:
				if (overworldSong != null && overworldSong.IsPlaying)
				{
					overworldSong.Stop(AudioStopOptions.Immediate);
				}
				if (zombieSong != null && zombieSong.IsPlaying)
				{
					zombieSong.Stop(AudioStopOptions.Immediate);
					zombieSong = null;
				}
				if (Game1.musicWaveBankState == Game1.MusicWaveBankState.Created)
				{
					zombieSong = Game1.soundBank.GetCue("Cowboy_undead");
					zombieSong.Play();
				}
				motionPause = 1800;
				zombieModeTimer = 10000;
				break;
			case 9:
			{
				Point position = Point.Zero;
				int num2 = 0;
				while ((Math.Abs((float)position.X - playerPosition.X) < 8f || Math.Abs((float)position.Y - playerPosition.Y) < 8f || isCollidingWithMap(position) || isCollidingWithMonster(new Rectangle(position.X, position.Y, TileSize, TileSize), null)) && num2 < 10)
				{
					position = new Point(Game1.random.Next(TileSize, 16 * TileSize - TileSize), Game1.random.Next(TileSize, 16 * TileSize - TileSize));
					num2++;
				}
				if (num2 < 10)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, playerPosition + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2(position.X, position.Y) + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2(position.X - TileSize / 2, position.Y) + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
					{
						delayBeforeAnimationStart = 200
					});
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2(position.X + TileSize / 2, position.Y) + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
					{
						delayBeforeAnimationStart = 400
					});
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2(position.X, position.Y - TileSize / 2) + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
					{
						delayBeforeAnimationStart = 600
					});
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2(position.X, position.Y + TileSize / 2) + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
					{
						delayBeforeAnimationStart = 800
					});
					playerPosition = new Vector2(position.X, position.Y);
					monsterConfusionTimer = 4000;
					playerInvincibleTimer = 4000;
					Game1.playSound("cowboy_powerup");
				}
				break;
			}
			case 8:
				lives++;
				Game1.playSound("cowboy_powerup");
				break;
			case 4:
			{
				Game1.playSound("cowboy_explosion");
				if (!shootoutLevel)
				{
					foreach (CowboyMonster monster in monsters)
					{
						addGuts(monster.position.Location, monster.type);
					}
					monsters.Clear();
				}
				else
				{
					foreach (CowboyMonster monster2 in monsters)
					{
						monster2.takeDamage(30);
						bullets.Add(new CowboyBullet(monster2.position.Center, 2, 1));
					}
				}
				for (int i = 0; i < 30; i++)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, new Vector2(Game1.random.Next(1, 16), Game1.random.Next(1, 16)) * TileSize + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
					{
						delayBeforeAnimationStart = Game1.random.Next(800)
					});
				}
				break;
			}
			case 2:
			case 3:
			case 7:
				shotTimer = 0;
				Game1.playSound("cowboy_gunload");
				activePowerups.Add(which, powerupDuration + 2000);
				break;
			case 0:
				coins++;
				Game1.playSound("Pickup_Coin15");
				break;
			case 1:
				coins += 5;
				Game1.playSound("Pickup_Coin15");
				Game1.playSound("Pickup_Coin15");
				break;
			default:
				{
					activePowerups.Add(which, powerupDuration);
					Game1.playSound("cowboy_powerup");
					break;
				}
				IL_00cd:
				itemToHold = num;
				holdItemTimer = 2000;
				Game1.playSound("Cowboy_Secret");
				gopherTrain = true;
				gopherTrainPosition = -TileSize * 2;
				break;
			}
			if (whichRound > 0 && activePowerups.ContainsKey(which))
			{
				activePowerups[which] /= 2;
			}
		}

		public static void addGuts(Point position, int whichGuts)
		{
			switch (whichGuts)
			{
			case 0:
			case 2:
			case 5:
			case 6:
			case 7:
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(512, 1696, 16, 16), 80f, 6, 0, topLeftScreenCoordinate + new Vector2(position.X, position.Y), flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(592, 1696, 16, 16), 10000f, 1, 0, topLeftScreenCoordinate + new Vector2(position.X, position.Y), flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					delayBeforeAnimationStart = 480
				});
				break;
			case 3:
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, topLeftScreenCoordinate + new Vector2(position.X, position.Y), flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
				break;
			case 1:
			case 4:
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(544, 1728, 16, 16), 80f, 4, 0, topLeftScreenCoordinate + new Vector2(position.X, position.Y), flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
				break;
			}
		}

		public void endOfGopherAnimationBehavior2(int extraInfo)
		{
			Game1.playSound("cowboy_gopher");
			if (Math.Abs(gopherBox.X - 8 * TileSize) > Math.Abs(gopherBox.Y - 8 * TileSize))
			{
				if (gopherBox.X > 8 * TileSize)
				{
					gopherMotion = new Point(-2, 0);
				}
				else
				{
					gopherMotion = new Point(2, 0);
				}
			}
			else if (gopherBox.Y > 8 * TileSize)
			{
				gopherMotion = new Point(0, -2);
			}
			else
			{
				gopherMotion = new Point(0, 2);
			}
			gopherRunning = true;
		}

		public void endOfGopherAnimationBehavior(int extrainfo)
		{
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(384, 1792, 16, 16), 120f, 4, 2, topLeftScreenCoordinate + new Vector2(gopherBox.X + TileSize / 2, gopherBox.Y + TileSize / 2), flicker: false, flipped: false, (float)gopherBox.Y / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
			{
				endFunction = endOfGopherAnimationBehavior2
			});
			Game1.playSound("cowboy_gopher");
		}

		public static void killOutlaw()
		{
			powerups.Add(new CowboyPowerup((world == 0) ? (-1) : (-2), new Point(8 * TileSize, 10 * TileSize), 9999999));
			if (outlawSong != null && outlawSong.IsPlaying)
			{
				outlawSong.Stop(AudioStopOptions.Immediate);
			}
			map[8, 8] = 10;
			screenFlash = 200;
			Game1.playSound("Cowboy_monsterDie");
			for (int i = 0; i < 15; i++)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, new Vector2(monsters[0].position.X + Game1.random.Next(-TileSize, TileSize), monsters[0].position.Y + Game1.random.Next(-TileSize, TileSize)) + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					delayBeforeAnimationStart = i * 75
				});
			}
			monsters.Clear();
		}

		public void updateBullets(GameTime time)
		{
			for (int num = bullets.Count - 1; num >= 0; num--)
			{
				bullets[num].position.X += bullets[num].motion.X;
				bullets[num].position.Y += bullets[num].motion.Y;
				if (bullets[num].position.X <= 0 || bullets[num].position.Y <= 0 || bullets[num].position.X >= 768 || bullets[num].position.Y >= 768)
				{
					bullets.RemoveAt(num);
				}
				else if (map[bullets[num].position.X / 16 / 3, bullets[num].position.Y / 16 / 3] == 7)
				{
					bullets.RemoveAt(num);
				}
				else
				{
					for (int num2 = monsters.Count - 1; num2 >= 0; num2--)
					{
						if (monsters[num2].position.Intersects(new Rectangle(bullets[num].position.X, bullets[num].position.Y, 12, 12)))
						{
							int health = monsters[num2].health;
							int num3 = 0;
							if (monsters[num2].takeDamage(bullets[num].damage))
							{
								num3 = monsters[num2].health;
								addGuts(monsters[num2].position.Location, monsters[num2].type);
								int num4 = monsters[num2].getLootDrop();
								if (whichRound == 1 && Game1.random.NextDouble() < 0.5)
								{
									num4 = -1;
								}
								if (whichRound > 0 && (num4 == 5 || num4 == 8) && Game1.random.NextDouble() < 0.4)
								{
									num4 = -1;
								}
								if (num4 != -1 && whichWave != 12)
								{
									powerups.Add(new CowboyPowerup(num4, monsters[num2].position.Location, lootDuration));
								}
								if (shootoutLevel)
								{
									if (whichWave == 12 && monsters[num2].type == -2)
									{
										Game1.playSound("cowboy_explosion");
										powerups.Add(new CowboyPowerup(-3, new Point(8 * TileSize, 10 * TileSize), 9999999));
										noPickUpBox = new Rectangle(8 * TileSize, 10 * TileSize, TileSize, TileSize);
										if (outlawSong != null && outlawSong.IsPlaying)
										{
											outlawSong.Stop(AudioStopOptions.Immediate);
										}
										screenFlash = 200;
										for (int i = 0; i < 30; i++)
										{
											temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(512, 1696, 16, 16), 70f, 6, 0, new Vector2(monsters[num2].position.X + Game1.random.Next(-TileSize, TileSize), monsters[num2].position.Y + Game1.random.Next(-TileSize, TileSize)) + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
											{
												delayBeforeAnimationStart = i * 75
											});
											if (i % 4 == 0)
											{
												addGuts(new Point(monsters[num2].position.X + Game1.random.Next(-TileSize, TileSize), monsters[num2].position.Y + Game1.random.Next(-TileSize, TileSize)), 7);
											}
											if (i % 4 == 0)
											{
												temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, new Vector2(monsters[num2].position.X + Game1.random.Next(-TileSize, TileSize), monsters[num2].position.Y + Game1.random.Next(-TileSize, TileSize)) + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
												{
													delayBeforeAnimationStart = i * 75
												});
											}
											if (i % 3 == 0)
											{
												temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(544, 1728, 16, 16), 100f, 4, 0, new Vector2(monsters[num2].position.X + Game1.random.Next(-TileSize, TileSize), monsters[num2].position.Y + Game1.random.Next(-TileSize, TileSize)) + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
												{
													delayBeforeAnimationStart = i * 75
												});
											}
										}
									}
									else if (whichWave != 12)
									{
										powerups.Add(new CowboyPowerup((world == 0) ? (-1) : (-2), new Point(8 * TileSize, 10 * TileSize), 9999999));
										if (outlawSong != null && outlawSong.IsPlaying)
										{
											outlawSong.Stop(AudioStopOptions.Immediate);
										}
										map[8, 8] = 10;
										screenFlash = 200;
										for (int j = 0; j < 15; j++)
										{
											temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, new Vector2(monsters[num2].position.X + Game1.random.Next(-TileSize, TileSize), monsters[num2].position.Y + Game1.random.Next(-TileSize, TileSize)) + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
											{
												delayBeforeAnimationStart = j * 75
											});
										}
									}
								}
								monsters.RemoveAt(num2);
								Game1.playSound("Cowboy_monsterDie");
							}
							else
							{
								num3 = monsters[num2].health;
							}
							bullets[num].damage -= health - num3;
							if (bullets[num].damage <= 0)
							{
								bullets.RemoveAt(num);
							}
							break;
						}
					}
				}
			}
			for (int num5 = enemyBullets.Count - 1; num5 >= 0; num5--)
			{
				enemyBullets[num5].position.X += enemyBullets[num5].motion.X;
				enemyBullets[num5].position.Y += enemyBullets[num5].motion.Y;
				if (enemyBullets[num5].position.X <= 0 || enemyBullets[num5].position.Y <= 0 || enemyBullets[num5].position.X >= 762 || enemyBullets[num5].position.Y >= 762)
				{
					enemyBullets.RemoveAt(num5);
				}
				else if (map[(enemyBullets[num5].position.X + 6) / 16 / 3, (enemyBullets[num5].position.Y + 6) / 16 / 3] == 7)
				{
					enemyBullets.RemoveAt(num5);
				}
				else if (playerInvincibleTimer <= 0 && deathTimer <= 0f && playerBoundingBox.Intersects(new Rectangle(enemyBullets[num5].position.X, enemyBullets[num5].position.Y, 15, 15)))
				{
					playerDie();
					break;
				}
			}
		}

		public void playerDie()
		{
			gopherRunning = false;
			hasGopherAppeared = false;
			spawnQueue = new List<Point>[4];
			for (int i = 0; i < 4; i++)
			{
				spawnQueue[i] = new List<Point>();
			}
			enemyBullets.Clear();
			if (!shootoutLevel)
			{
				powerups.Clear();
				monsters.Clear();
			}
			died = true;
			activePowerups.Clear();
			deathTimer = 3000f;
			if (overworldSong != null && overworldSong.IsPlaying)
			{
				overworldSong.Stop(AudioStopOptions.Immediate);
			}
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1808, 16, 16), 120f, 5, 0, playerPosition + topLeftScreenCoordinate, flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
			waveTimer = Math.Min(80000, waveTimer + 10000);
			betweenWaveTimer = 4000;
			lives--;
			playerInvincibleTimer = 5000;
			if (shootoutLevel)
			{
				playerPosition = new Vector2(8 * TileSize, 3 * TileSize);
				playerBoundingBox.X = (int)playerPosition.X + TileSize / 4;
				playerBoundingBox.Y = (int)playerPosition.Y + TileSize / 4;
				playerBoundingBox.Width = TileSize / 2;
				playerBoundingBox.Height = TileSize / 2;
				Game1.playSound("Cowboy_monsterDie");
			}
			else
			{
				playerPosition = new Vector2(8 * TileSize - TileSize, 8 * TileSize);
				playerBoundingBox.X = (int)playerPosition.X + TileSize / 4;
				playerBoundingBox.Y = (int)playerPosition.Y + TileSize / 4;
				playerBoundingBox.Width = TileSize / 2;
				playerBoundingBox.Height = TileSize / 2;
				if (playerBoundingBox.Intersects(player2BoundingBox))
				{
					playerPosition.X -= TileSize * 3 / 2;
					player2deathtimer = (int)deathTimer;
					playerBoundingBox.X = (int)playerPosition.X + TileSize / 4;
					playerBoundingBox.Y = (int)playerPosition.Y + TileSize / 4;
					playerBoundingBox.Width = TileSize / 2;
					playerBoundingBox.Height = TileSize / 2;
				}
				Game1.playSound("cowboy_dead");
			}
			if (lives < 0)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1808, 16, 16), 550f, 5, 0, playerPosition + topLeftScreenCoordinate, flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					alpha = 0.001f,
					endFunction = afterPlayerDeathFunction
				});
				deathTimer *= 3f;
				Game1.player.jotpkProgress.Value = null;
			}
			else if (!shootoutLevel)
			{
				SaveGame();
			}
		}

		public void afterPlayerDeathFunction(int extra)
		{
			if (lives < 0)
			{
				gameOver = true;
				if (overworldSong != null && !overworldSong.IsPlaying)
				{
					overworldSong.Stop(AudioStopOptions.Immediate);
				}
				if (outlawSong != null && !outlawSong.IsPlaying)
				{
					overworldSong.Stop(AudioStopOptions.Immediate);
				}
				monsters.Clear();
				powerups.Clear();
				died = false;
				Game1.playSound("Cowboy_monsterDie");
				if (playingWithAbigail && Game1.currentLocation.currentEvent != null)
				{
					unload();
					Game1.currentMinigame = null;
					Game1.currentLocation.currentEvent.CurrentCommand++;
				}
			}
		}

		public void startAbigailPortrait(int whichExpression, string sayWhat)
		{
			if (abigailPortraitTimer <= 0)
			{
				abigailPortraitTimer = 6000;
				AbigailDialogue = sayWhat;
				abigailPortraitExpression = whichExpression;
				abigailPortraitYposition = (int)((float)Game1.viewport.Height / Game1.options.zoomLevel);
				Game1.playSound("dwop");
			}
		}

		public void startNewRound()
		{
			gamerestartTimer = 2000;
			Game1.playSound("Cowboy_monsterDie");
			whichRound++;
		}

		protected void _UpdateInput()
		{
			UpdateMobile();
			if (Game1.options.gamepadControls)
			{
				GamePadState padState = Game1.input.GetGamePadState();
				ButtonCollection buttonCollection = new ButtonCollection(ref padState);
				if ((double)padState.ThumbSticks.Left.X < -0.2)
				{
					_buttonHeldState.Add(GameKeys.MoveLeft);
				}
				if ((double)padState.ThumbSticks.Left.X > 0.2)
				{
					_buttonHeldState.Add(GameKeys.MoveRight);
				}
				if ((double)padState.ThumbSticks.Left.Y < -0.2)
				{
					_buttonHeldState.Add(GameKeys.MoveDown);
				}
				if ((double)padState.ThumbSticks.Left.Y > 0.2)
				{
					_buttonHeldState.Add(GameKeys.MoveUp);
				}
				if ((double)padState.ThumbSticks.Right.X < -0.2)
				{
					_buttonHeldState.Add(GameKeys.ShootLeft);
				}
				if ((double)padState.ThumbSticks.Right.X > 0.2)
				{
					_buttonHeldState.Add(GameKeys.ShootRight);
				}
				if ((double)padState.ThumbSticks.Right.Y < -0.2)
				{
					_buttonHeldState.Add(GameKeys.ShootDown);
				}
				if ((double)padState.ThumbSticks.Right.Y > 0.2)
				{
					_buttonHeldState.Add(GameKeys.ShootUp);
				}
				ButtonCollection.ButtonEnumerator enumerator = buttonCollection.GetEnumerator();
				while (enumerator.MoveNext())
				{
					switch (enumerator.Current)
					{
					case Buttons.A:
						if (gameOver)
						{
							_buttonHeldState.Add(GameKeys.SelectOption);
						}
						else if (Program.sdk.IsEnterButtonAssignmentFlipped)
						{
							_buttonHeldState.Add(GameKeys.ShootRight);
						}
						else
						{
							_buttonHeldState.Add(GameKeys.ShootDown);
						}
						break;
					case Buttons.Y:
						_buttonHeldState.Add(GameKeys.ShootUp);
						break;
					case Buttons.X:
						_buttonHeldState.Add(GameKeys.ShootLeft);
						break;
					case Buttons.B:
						if (gameOver)
						{
							_buttonHeldState.Add(GameKeys.Exit);
						}
						else if (Program.sdk.IsEnterButtonAssignmentFlipped)
						{
							_buttonHeldState.Add(GameKeys.ShootDown);
						}
						else
						{
							_buttonHeldState.Add(GameKeys.ShootRight);
						}
						break;
					case Buttons.DPadUp:
						_buttonHeldState.Add(GameKeys.MoveUp);
						break;
					case Buttons.DPadDown:
						_buttonHeldState.Add(GameKeys.MoveDown);
						break;
					case Buttons.DPadLeft:
						_buttonHeldState.Add(GameKeys.MoveLeft);
						break;
					case Buttons.DPadRight:
						_buttonHeldState.Add(GameKeys.MoveRight);
						break;
					case Buttons.Start:
					case Buttons.LeftShoulder:
					case Buttons.RightShoulder:
					case Buttons.RightTrigger:
					case Buttons.LeftTrigger:
						_buttonHeldState.Add(GameKeys.UsePowerup);
						break;
					case Buttons.Back:
						_buttonHeldState.Add(GameKeys.Exit);
						break;
					}
				}
			}
			if (_binds == null)
			{
				SetupBinds();
			}
			if (IsBoundButtonDown(GameKeys.MoveUp))
			{
				_buttonHeldState.Add(GameKeys.MoveUp);
			}
			if (IsBoundButtonDown(GameKeys.MoveDown))
			{
				_buttonHeldState.Add(GameKeys.MoveDown);
			}
			if (IsBoundButtonDown(GameKeys.MoveLeft))
			{
				_buttonHeldState.Add(GameKeys.MoveLeft);
			}
			if (IsBoundButtonDown(GameKeys.MoveRight))
			{
				_buttonHeldState.Add(GameKeys.MoveRight);
			}
			if (IsBoundButtonDown(GameKeys.ShootUp))
			{
				if (gameOver)
				{
					_buttonHeldState.Add(GameKeys.MoveUp);
				}
				else
				{
					_buttonHeldState.Add(GameKeys.ShootUp);
				}
			}
			if (IsBoundButtonDown(GameKeys.ShootDown))
			{
				if (gameOver)
				{
					_buttonHeldState.Add(GameKeys.MoveDown);
				}
				else
				{
					_buttonHeldState.Add(GameKeys.ShootDown);
				}
			}
			if (IsBoundButtonDown(GameKeys.ShootLeft))
			{
				_buttonHeldState.Add(GameKeys.ShootLeft);
			}
			if (IsBoundButtonDown(GameKeys.ShootRight))
			{
				_buttonHeldState.Add(GameKeys.ShootRight);
			}
			if (IsBoundButtonDown(GameKeys.UsePowerup))
			{
				if (gameOver)
				{
					_buttonHeldState.Add(GameKeys.SelectOption);
				}
				else
				{
					_buttonHeldState.Add(GameKeys.UsePowerup);
				}
			}
			if (IsBoundButtonDown(GameKeys.Exit))
			{
				_buttonHeldState.Add(GameKeys.Exit);
			}
		}

		public virtual void SetupBinds()
		{
			_binds = new Dictionary<GameKeys, List<Keys>>();
			_binds[GameKeys.MoveUp] = new List<Keys>(new Keys[1] { Keys.W });
			_binds[GameKeys.MoveDown] = new List<Keys>(new Keys[1] { Keys.S });
			_binds[GameKeys.MoveLeft] = new List<Keys>(new Keys[1] { Keys.A });
			_binds[GameKeys.MoveRight] = new List<Keys>(new Keys[1] { Keys.D });
			_binds[GameKeys.ShootUp] = new List<Keys>(new Keys[1] { Keys.Up });
			_binds[GameKeys.ShootDown] = new List<Keys>(new Keys[1] { Keys.Down });
			_binds[GameKeys.ShootLeft] = new List<Keys>(new Keys[1] { Keys.Left });
			_binds[GameKeys.ShootRight] = new List<Keys>(new Keys[1] { Keys.Right });
			_binds[GameKeys.UsePowerup] = new List<Keys>(new Keys[2]
			{
				Keys.Enter,
				Keys.Space
			});
			_binds[GameKeys.Exit] = new List<Keys>(new Keys[1] { Keys.Escape });
			Keys keys = Keys.None;
			keys = GetBoundKey(Game1.options.moveUpButton);
			if (keys != 0 && keys != Keys.Up && keys != Keys.Down && keys != Keys.Left && keys != Keys.Right)
			{
				_binds[GameKeys.MoveUp] = new List<Keys>(new Keys[1] { keys });
			}
			keys = GetBoundKey(Game1.options.moveDownButton);
			if (keys != 0 && keys != Keys.Up && keys != Keys.Down && keys != Keys.Left && keys != Keys.Right)
			{
				_binds[GameKeys.MoveDown] = new List<Keys>(new Keys[1] { keys });
			}
			keys = GetBoundKey(Game1.options.moveLeftButton);
			if (keys != 0 && keys != Keys.Up && keys != Keys.Down && keys != Keys.Left && keys != Keys.Right)
			{
				_binds[GameKeys.MoveLeft] = new List<Keys>(new Keys[1] { keys });
			}
			keys = GetBoundKey(Game1.options.moveRightButton);
			if (keys != 0 && keys != Keys.Up && keys != Keys.Down && keys != Keys.Left && keys != Keys.Right)
			{
				_binds[GameKeys.MoveRight] = new List<Keys>(new Keys[1] { keys });
			}
			bool flag = false;
			foreach (List<Keys> value in _binds.Values)
			{
				if (value.Contains(Keys.X))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_binds[GameKeys.UsePowerup].Add(Keys.X);
			}
		}

		public Keys GetBoundKey(InputButton[] button)
		{
			if (button == null || button.Length == 0)
			{
				return Keys.None;
			}
			for (int i = 0; i < button.Length; i++)
			{
				if (button[i].key != 0)
				{
					return button[i].key;
				}
			}
			return Keys.None;
		}

		public bool IsBoundButtonDown(GameKeys game_key)
		{
			List<Keys> value = null;
			if (_binds.TryGetValue(game_key, out value))
			{
				foreach (Keys item in value)
				{
					if (Game1.input.GetKeyboardState().IsKeyDown(item))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool tick(GameTime time)
		{
			if (_buttonHeldFrames == null)
			{
				_buttonHeldFrames = new Dictionary<GameKeys, int>();
				for (int i = 0; i < 11; i++)
				{
					_buttonHeldFrames[(GameKeys)i] = 0;
				}
			}
			_buttonHeldState.Clear();
			if (startTimer <= 0)
			{
				_UpdateInput();
			}
			for (int j = 0; j < 11; j++)
			{
				if (_buttonHeldState.Contains((GameKeys)j))
				{
					_buttonHeldFrames[(GameKeys)j]++;
				}
				else
				{
					_buttonHeldFrames[(GameKeys)j] = 0;
				}
			}
			_ProcessInputs();
			if (quit)
			{
				Game1.stopMusicTrack(Game1.MusicContext.MiniGame);
				return true;
			}
			if (gameOver)
			{
				startTimer = 0;
				return false;
			}
			if (onStartMenu)
			{
				if (startTimer > 0)
				{
					startTimer -= time.ElapsedGameTime.Milliseconds;
					if (startTimer <= 0)
					{
						shotTimer = 100;
						onStartMenu = false;
					}
				}
				else
				{
					Game1.playSound("Pickup_Coin15");
					startTimer = 1500;
				}
				return false;
			}
			if (gamerestartTimer > 0)
			{
				gamerestartTimer -= time.ElapsedGameTime.Milliseconds;
				if (gamerestartTimer <= 0)
				{
					unload();
					if (whichRound == 0 || !endCutscene)
					{
						Game1.currentMinigame = new AbigailGame();
					}
					else
					{
						Game1.currentMinigame = new AbigailGame(coins, ammoLevel, bulletDamage, fireSpeedLevel, runSpeedLevel, lives, spreadPistol, whichRound);
					}
				}
			}
			if (fadethenQuitTimer > 0 && (float)abigailPortraitTimer <= 0f)
			{
				fadethenQuitTimer -= time.ElapsedGameTime.Milliseconds;
				if (fadethenQuitTimer <= 0)
				{
					if (Game1.currentLocation.currentEvent != null)
					{
						Game1.currentLocation.currentEvent.CurrentCommand++;
						if (beatLevelWithAbigail)
						{
							Game1.currentLocation.currentEvent.specialEventVariable1 = true;
						}
					}
					return true;
				}
			}
			if (abigailPortraitTimer > 0)
			{
				abigailPortraitTimer -= time.ElapsedGameTime.Milliseconds;
				if (abigailPortraitTimer > 1000 && abigailPortraitYposition > (int)((float)Game1.viewport.Height / Game1.options.zoomLevel) - 256)
				{
					abigailPortraitYposition -= 16;
				}
				else if (abigailPortraitTimer <= 1000)
				{
					abigailPortraitYposition += 16;
				}
			}
			if (endCutscene)
			{
				endCutsceneTimer -= time.ElapsedGameTime.Milliseconds;
				if (endCutsceneTimer < 0)
				{
					endCutscenePhase++;
					if (endCutscenePhase > 5)
					{
						endCutscenePhase = 5;
					}
					switch (endCutscenePhase)
					{
					case 1:
						Game1.getSteamAchievement("Achievement_PrairieKing");
						if (!died)
						{
							Game1.getSteamAchievement("Achievement_FectorsChallenge");
						}
						Game1.multiplayer.globalChatInfoMessage("PrairieKing", Game1.player.Name);
						endCutsceneTimer = 15500;
						Game1.playSound("Cowboy_singing");
						map = getMap(-1);
						break;
					case 2:
						playerPosition = new Vector2(0f, 8 * TileSize);
						endCutsceneTimer = 12000;
						break;
					case 3:
						endCutsceneTimer = 5000;
						break;
					case 4:
						endCutsceneTimer = 1000;
						break;
					case 5:
						if (!_lastLeftJoystickHeld && !_lastRightJoystickHeld && Game1.input.GetKeyboardState().GetPressedKeys().Length == 0)
						{
							Game1.input.GetGamePadState();
							if (Game1.input.GetGamePadState().Buttons.X != ButtonState.Pressed && Game1.input.GetGamePadState().Buttons.Start != ButtonState.Pressed && Game1.input.GetGamePadState().Buttons.A != ButtonState.Pressed)
							{
								break;
							}
						}
						if (gamerestartTimer <= 0)
						{
							startNewRound();
						}
						break;
					}
				}
				if (endCutscenePhase == 2 && playerPosition.X < (float)(9 * TileSize))
				{
					playerPosition.X += 1f;
					playerMotionAnimationTimer += time.ElapsedGameTime.Milliseconds;
					playerMotionAnimationTimer %= 400f;
				}
				return false;
			}
			if (motionPause > 0)
			{
				motionPause -= time.ElapsedGameTime.Milliseconds;
				if (motionPause <= 0 && behaviorAfterPause != null)
				{
					behaviorAfterPause();
					behaviorAfterPause = null;
				}
			}
			else if (monsterConfusionTimer > 0)
			{
				monsterConfusionTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (zombieModeTimer > 0)
			{
				zombieModeTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (holdItemTimer > 0)
			{
				holdItemTimer -= time.ElapsedGameTime.Milliseconds;
				return false;
			}
			if (screenFlash > 0)
			{
				screenFlash -= time.ElapsedGameTime.Milliseconds;
			}
			if (gopherTrain)
			{
				gopherTrainPosition += 3;
				if (gopherTrainPosition % 30 == 0)
				{
					Game1.playSound("Cowboy_Footstep");
				}
				if (playerJumped)
				{
					playerPosition.Y += 3f;
				}
				if (Math.Abs(playerPosition.Y - (float)(gopherTrainPosition - TileSize)) <= 16f)
				{
					playerJumped = true;
					playerPosition.Y = gopherTrainPosition - TileSize;
				}
				if (gopherTrainPosition > 16 * TileSize + TileSize)
				{
					gopherTrain = false;
					playerJumped = false;
					whichWave++;
					map = getMap(whichWave);
					playerPosition = new Vector2(8 * TileSize, 8 * TileSize);
					world = ((world != 0) ? 1 : 2);
					waveTimer = 80000;
					betweenWaveTimer = 5000;
					waitingForPlayerToMoveDownAMap = false;
					shootoutLevel = false;
					SaveGame();
				}
			}
			if ((shopping || merchantArriving || merchantLeaving || waitingForPlayerToMoveDownAMap) && holdItemTimer <= 0)
			{
				int num = shoppingTimer;
				shoppingTimer += time.ElapsedGameTime.Milliseconds;
				shoppingTimer %= 500;
				if (!merchantShopOpen && shopping && ((num < 250 && shoppingTimer >= 250) || num > shoppingTimer))
				{
					Game1.playSound("Cowboy_Footstep");
				}
			}
			if (playerInvincibleTimer > 0)
			{
				playerInvincibleTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (scrollingMap)
			{
				newMapPosition -= TileSize / 8;
				playerPosition.Y -= TileSize / 8;
				playerPosition.Y += 3f;
				playerBoundingBox.X = (int)playerPosition.X + TileSize / 4;
				playerBoundingBox.Y = (int)playerPosition.Y + TileSize / 4;
				playerBoundingBox.Width = TileSize / 2;
				playerBoundingBox.Height = TileSize / 2;
				playerMovementDirections = new List<int> { 2 };
				playerMotionAnimationTimer += time.ElapsedGameTime.Milliseconds;
				playerMotionAnimationTimer %= 400f;
				if (newMapPosition <= 0)
				{
					scrollingMap = false;
					map = nextMap;
					newMapPosition = 16 * TileSize;
					shopping = false;
					betweenWaveTimer = 5000;
					waitingForPlayerToMoveDownAMap = false;
					playerMovementDirections.Clear();
					ApplyLevelSpecificStates();
				}
			}
			if (gopherRunning)
			{
				gopherBox.X += gopherMotion.X;
				gopherBox.Y += gopherMotion.Y;
				for (int num2 = monsters.Count - 1; num2 >= 0; num2--)
				{
					if (gopherBox.Intersects(monsters[num2].position))
					{
						addGuts(monsters[num2].position.Location, monsters[num2].type);
						monsters.RemoveAt(num2);
						Game1.playSound("Cowboy_monsterDie");
					}
				}
				if (gopherBox.X < 0 || gopherBox.Y < 0 || gopherBox.X > 16 * TileSize || gopherBox.Y > 16 * TileSize)
				{
					gopherRunning = false;
				}
			}
			for (int num3 = temporarySprites.Count - 1; num3 >= 0; num3--)
			{
				if (temporarySprites[num3].update(time))
				{
					temporarySprites.RemoveAt(num3);
				}
			}
			if (motionPause <= 0)
			{
				for (int num4 = powerups.Count - 1; num4 >= 0; num4--)
				{
					if (Utility.distance(playerBoundingBox.Center.X, powerups[num4].position.X + TileSize / 2, playerBoundingBox.Center.Y, powerups[num4].position.Y + TileSize / 2) <= (float)(TileSize + 3) && (powerups[num4].position.X < TileSize || powerups[num4].position.X >= 16 * TileSize - TileSize || powerups[num4].position.Y < TileSize || powerups[num4].position.Y >= 16 * TileSize - TileSize))
					{
						if (powerups[num4].position.X + TileSize / 2 < playerBoundingBox.Center.X)
						{
							powerups[num4].position.X++;
						}
						if (powerups[num4].position.X + TileSize / 2 > playerBoundingBox.Center.X)
						{
							powerups[num4].position.X--;
						}
						if (powerups[num4].position.Y + TileSize / 2 < playerBoundingBox.Center.Y)
						{
							powerups[num4].position.Y++;
						}
						if (powerups[num4].position.Y + TileSize / 2 > playerBoundingBox.Center.Y)
						{
							powerups[num4].position.Y--;
						}
					}
					powerups[num4].duration -= time.ElapsedGameTime.Milliseconds;
					if (powerups[num4].duration <= 0)
					{
						powerups.RemoveAt(num4);
					}
				}
				for (int num5 = activePowerups.Count - 1; num5 >= 0; num5--)
				{
					activePowerups[activePowerups.ElementAt(num5).Key] -= time.ElapsedGameTime.Milliseconds;
					if (activePowerups[activePowerups.ElementAt(num5).Key] <= 0)
					{
						activePowerups.Remove(activePowerups.ElementAt(num5).Key);
					}
				}
				if (deathTimer <= 0f && playerMovementDirections.Count > 0 && !scrollingMap)
				{
					int num6 = playerMovementDirections.Count;
					if (num6 >= 2 && playerMovementDirections.Last() == (playerMovementDirections.ElementAt(playerMovementDirections.Count - 2) + 2) % 4)
					{
						num6 = 1;
					}
					float num7 = getMovementSpeed(3f, num6);
					if (activePowerups.Keys.Contains(6))
					{
						num7 *= 1.5f;
					}
					if (zombieModeTimer > 0)
					{
						num7 *= 1.5f;
					}
					for (int k = 0; k < runSpeedLevel; k++)
					{
						num7 *= 1.25f;
					}
					for (int l = Math.Max(0, playerMovementDirections.Count - 2); l < playerMovementDirections.Count; l++)
					{
						if (l != 0 || playerMovementDirections.Count < 2 || playerMovementDirections.Last() != (playerMovementDirections.ElementAt(playerMovementDirections.Count - 2) + 2) % 4)
						{
							Vector2 vector = playerPosition;
							switch (playerMovementDirections.ElementAt(l))
							{
							case 0:
								vector.Y -= num7;
								break;
							case 3:
								vector.X -= num7;
								break;
							case 2:
								vector.Y += num7;
								break;
							case 1:
								vector.X += num7;
								break;
							}
							Rectangle rectangle = new Rectangle((int)vector.X + TileSize / 4, (int)vector.Y + TileSize / 4, TileSize / 2, TileSize / 2);
							if (!isCollidingWithMap(rectangle) && (!merchantBox.Intersects(rectangle) || merchantBox.Intersects(playerBoundingBox)) && (!playingWithAbigail || !rectangle.Intersects(player2BoundingBox)))
							{
								playerPosition = vector;
							}
						}
					}
					playerBoundingBox.X = (int)playerPosition.X + TileSize / 4;
					playerBoundingBox.Y = (int)playerPosition.Y + TileSize / 4;
					playerBoundingBox.Width = TileSize / 2;
					playerBoundingBox.Height = TileSize / 2;
					playerMotionAnimationTimer += time.ElapsedGameTime.Milliseconds;
					playerMotionAnimationTimer %= 400f;
					playerFootstepSoundTimer -= time.ElapsedGameTime.Milliseconds;
					if (playerFootstepSoundTimer <= 0f)
					{
						Game1.playSound("Cowboy_Footstep");
						playerFootstepSoundTimer = 200f;
					}
					for (int num8 = powerups.Count - 1; num8 >= 0; num8--)
					{
						if (playerBoundingBox.Intersects(new Rectangle(powerups[num8].position.X, powerups[num8].position.Y, TileSize, TileSize)) && !playerBoundingBox.Intersects(noPickUpBox))
						{
							if (heldItem != null)
							{
								usePowerup(powerups[num8].which);
								powerups.RemoveAt(num8);
							}
							else if (getPowerUp(powerups[num8]))
							{
								powerups.RemoveAt(num8);
							}
						}
					}
					if (!playerBoundingBox.Intersects(noPickUpBox))
					{
						noPickUpBox.Location = new Point(0, 0);
					}
					if (waitingForPlayerToMoveDownAMap && playerBoundingBox.Bottom >= 16 * TileSize - TileSize / 2)
					{
						SaveGame();
						shopping = false;
						merchantArriving = false;
						merchantLeaving = false;
						merchantShopOpen = false;
						merchantBox.Y = -TileSize;
						scrollingMap = true;
						nextMap = getMap(whichWave);
						newMapPosition = 16 * TileSize;
						temporarySprites.Clear();
						powerups.Clear();
					}
					if (!shoppingCarpetNoPickup.Intersects(playerBoundingBox))
					{
						shoppingCarpetNoPickup.X = -1000;
					}
				}
				if (shopping)
				{
					if (merchantBox.Y < 8 * TileSize - TileSize * 3 && merchantArriving)
					{
						merchantBox.Y += 2;
						if (merchantBox.Y >= 8 * TileSize - TileSize * 3)
						{
							merchantShopOpen = true;
							Game1.playSound("cowboy_monsterhit");
							map[8, 15] = 3;
							map[7, 15] = 3;
							map[7, 15] = 3;
							map[8, 14] = 3;
							map[7, 14] = 3;
							map[7, 14] = 3;
							shoppingCarpetNoPickup = new Rectangle(merchantBox.X - TileSize, merchantBox.Y + TileSize, TileSize * 3, TileSize * 2);
						}
					}
					else if (merchantLeaving)
					{
						merchantBox.Y -= 2;
						if (merchantBox.Y <= -TileSize)
						{
							shopping = false;
							merchantLeaving = false;
							merchantArriving = true;
						}
					}
					else if (merchantShopOpen)
					{
						for (int num9 = storeItems.Count - 1; num9 >= 0; num9--)
						{
							if (!playerBoundingBox.Intersects(shoppingCarpetNoPickup) && playerBoundingBox.Intersects(storeItems.ElementAt(num9).Key) && coins >= getPriceForItem(storeItems.ElementAt(num9).Value))
							{
								Game1.playSound("Cowboy_Secret");
								holdItemTimer = 2500;
								motionPause = 2500;
								itemToHold = storeItems.ElementAt(num9).Value;
								storeItems.Remove(storeItems.ElementAt(num9).Key);
								merchantLeaving = true;
								merchantArriving = false;
								merchantShopOpen = false;
								coins -= getPriceForItem(itemToHold);
								switch (itemToHold)
								{
								case 6:
								case 7:
								case 8:
									ammoLevel++;
									bulletDamage++;
									break;
								case 0:
								case 1:
								case 2:
									fireSpeedLevel++;
									break;
								case 3:
								case 4:
									runSpeedLevel++;
									break;
								case 5:
									lives++;
									break;
								case 9:
									spreadPistol = true;
									break;
								case 10:
									heldItem = new CowboyPowerup(10, Point.Zero, 9999);
									break;
								}
							}
						}
					}
				}
				cactusDanceTimer += time.ElapsedGameTime.Milliseconds;
				cactusDanceTimer %= 1600f;
				if (shotTimer > 0)
				{
					shotTimer -= time.ElapsedGameTime.Milliseconds;
				}
				if (deathTimer <= 0f && playerShootingDirections.Count > 0 && shotTimer <= 0)
				{
					if (activePowerups.ContainsKey(2))
					{
						spawnBullets(new int[1], playerPosition);
						spawnBullets(new int[1] { 1 }, playerPosition);
						spawnBullets(new int[1] { 2 }, playerPosition);
						spawnBullets(new int[1] { 3 }, playerPosition);
						spawnBullets(new int[2] { 0, 1 }, playerPosition);
						spawnBullets(new int[2] { 1, 2 }, playerPosition);
						spawnBullets(new int[2] { 2, 3 }, playerPosition);
						spawnBullets(new int[2] { 3, 0 }, playerPosition);
					}
					else if (playerShootingDirections.Count == 1 || playerShootingDirections.Last() == (playerShootingDirections.ElementAt(playerShootingDirections.Count - 2) + 2) % 4)
					{
						spawnBullets(new int[1] { (playerShootingDirections.Count == 2 && playerShootingDirections.Last() == (playerShootingDirections.ElementAt(playerShootingDirections.Count - 2) + 2) % 4) ? playerShootingDirections.ElementAt(1) : playerShootingDirections.ElementAt(0) }, playerPosition);
					}
					else
					{
						spawnBullets(playerShootingDirections.ToArray(), playerPosition);
					}
					Game1.playSound("Cowboy_gunshot");
					shotTimer = shootingDelay;
					if (activePowerups.ContainsKey(3))
					{
						shotTimer /= 4;
					}
					for (int m = 0; m < fireSpeedLevel; m++)
					{
						shotTimer = shotTimer * 3 / 4;
					}
					if (activePowerups.ContainsKey(7))
					{
						shotTimer = shotTimer * 3 / 2;
					}
					shotTimer = Math.Max(shotTimer, 20);
				}
				updateBullets(time);
				foreach (CowboyPowerup powerup in powerups)
				{
					Vector2 item = new Vector2((powerup.position.X + TileSize / 2) / TileSize, (powerup.position.Y + TileSize / 2) / TileSize);
					Vector2 item2 = new Vector2(powerup.position.X / TileSize, powerup.position.Y / TileSize);
					Vector2 item3 = new Vector2((powerup.position.X + TileSize) / TileSize, powerup.position.Y / TileSize);
					Vector2 item4 = new Vector2(powerup.position.X / TileSize, powerup.position.Y / TileSize);
					Vector2 item5 = new Vector2(powerup.position.X / TileSize, (powerup.position.Y + 64) / TileSize);
					if (_borderTiles.Contains(item) || _borderTiles.Contains(item2) || _borderTiles.Contains(item3) || _borderTiles.Contains(item4) || _borderTiles.Contains(item5))
					{
						Point point = default(Point);
						if (Math.Abs(item.X - 8f) > Math.Abs(item.Y - 8f))
						{
							point.X = Math.Sign(item.X - 8f);
						}
						else
						{
							point.Y = Math.Sign(item.Y - 8f);
						}
						powerup.position.X -= point.X;
						powerup.position.Y -= point.Y;
					}
				}
				if (waveTimer > 0 && betweenWaveTimer <= 0 && zombieModeTimer <= 0 && !shootoutLevel && (overworldSong == null || !overworldSong.IsPlaying) && Game1.soundBank != null && Game1.musicWaveBankState == Game1.MusicWaveBankState.Created)
				{
					overworldSong = Game1.soundBank.GetCue("Cowboy_OVERWORLD");
					overworldSong.Play();
					Game1.musicPlayerVolume = Game1.options.musicVolumeLevel;
					Game1.musicCategory.SetVolume(Game1.musicPlayerVolume);
				}
				if (deathTimer > 0f)
				{
					deathTimer -= time.ElapsedGameTime.Milliseconds;
				}
				if (betweenWaveTimer > 0 && monsters.Count == 0 && isSpawnQueueEmpty() && !shopping && !waitingForPlayerToMoveDownAMap)
				{
					betweenWaveTimer -= time.ElapsedGameTime.Milliseconds;
					if (betweenWaveTimer <= 0 && playingWithAbigail)
					{
						startAbigailPortrait(7, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11896"));
					}
				}
				else if (deathTimer <= 0f && !waitingForPlayerToMoveDownAMap && !shopping && !shootoutLevel)
				{
					if (waveTimer > 0)
					{
						int num10 = waveTimer;
						waveTimer -= time.ElapsedGameTime.Milliseconds;
						if (playingWithAbigail && num10 > 40000 && waveTimer <= 40000)
						{
							startAbigailPortrait(0, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11897"));
						}
						int num11 = 0;
						foreach (Vector2 monsterChance in monsterChances)
						{
							if (Game1.random.NextDouble() < (double)(monsterChance.X * (float)((monsters.Count != 0) ? 1 : 2)))
							{
								int num12 = 1;
								while (Game1.random.NextDouble() < (double)monsterChance.Y && num12 < 15)
								{
									num12++;
								}
								spawnQueue[(whichWave == 11) ? (Game1.random.Next(1, 3) * 2 - 1) : Game1.random.Next(4)].Add(new Point(num11, num12));
							}
							num11++;
						}
						if (!hasGopherAppeared && monsters.Count > 6 && Game1.random.NextDouble() < 0.0004 && waveTimer > 7000 && waveTimer < 50000)
						{
							hasGopherAppeared = true;
							gopherBox = new Rectangle(Game1.random.Next(16 * TileSize), Game1.random.Next(16 * TileSize), TileSize, TileSize);
							int num13 = 0;
							while ((isCollidingWithMap(gopherBox) || isCollidingWithMonster(gopherBox, null) || Math.Abs((float)gopherBox.X - playerPosition.X) < (float)(TileSize * 6) || Math.Abs((float)gopherBox.Y - playerPosition.Y) < (float)(TileSize * 6) || Math.Abs(gopherBox.X - 8 * TileSize) < TileSize * 4 || Math.Abs(gopherBox.Y - 8 * TileSize) < TileSize * 4) && num13 < 10)
							{
								gopherBox.X = Game1.random.Next(16 * TileSize);
								gopherBox.Y = Game1.random.Next(16 * TileSize);
								num13++;
							}
							if (num13 < 10)
							{
								temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(256, 1664, 16, 32), 80f, 5, 0, topLeftScreenCoordinate + new Vector2(gopherBox.X + TileSize / 2, gopherBox.Y - TileSize + TileSize / 2), flicker: false, flipped: false, (float)gopherBox.Y / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
								{
									endFunction = endOfGopherAnimationBehavior
								});
							}
						}
					}
					for (int n = 0; n < 4; n++)
					{
						if (spawnQueue[n].Count <= 0)
						{
							continue;
						}
						if (spawnQueue[n][0].X == 1 || spawnQueue[n][0].X == 4)
						{
							Rectangle r = new Rectangle(0, 0, 16, 16);
							List<Vector2> borderOfThisRectangle = Utility.getBorderOfThisRectangle(r);
							Vector2 vector2 = borderOfThisRectangle.ElementAt(Game1.random.Next(borderOfThisRectangle.Count));
							int num14 = 0;
							while (isCollidingWithMonster(new Rectangle((int)vector2.X * TileSize, (int)vector2.Y * TileSize, TileSize, TileSize), null) && num14 < 10)
							{
								vector2 = borderOfThisRectangle.ElementAt(Game1.random.Next(borderOfThisRectangle.Count));
								num14++;
							}
							if (num14 < 10)
							{
								monsters.Add(new CowboyMonster(spawnQueue[n][0].X, new Point((int)vector2.X * TileSize, (int)vector2.Y * TileSize)));
								if (whichRound > 0)
								{
									monsters.Last().health += whichRound * 2;
								}
								spawnQueue[n][0] = new Point(spawnQueue[n][0].X, spawnQueue[n][0].Y - 1);
								if (spawnQueue[n][0].Y <= 0)
								{
									spawnQueue[n].RemoveAt(0);
								}
							}
							continue;
						}
						switch (n)
						{
						case 0:
						{
							for (int num18 = 7; num18 < 10; num18++)
							{
								if (Game1.random.NextDouble() < 0.5 && !isCollidingWithMonster(new Rectangle(num18 * 16 * 3, 0, 48, 48), null))
								{
									monsters.Add(new CowboyMonster(spawnQueue[n].First().X, new Point(num18 * TileSize, 0)));
									if (whichRound > 0)
									{
										monsters.Last().health += whichRound * 2;
									}
									spawnQueue[n][0] = new Point(spawnQueue[n][0].X, spawnQueue[n][0].Y - 1);
									if (spawnQueue[n][0].Y <= 0)
									{
										spawnQueue[n].RemoveAt(0);
									}
									break;
								}
							}
							break;
						}
						case 1:
						{
							for (int num16 = 7; num16 < 10; num16++)
							{
								if (Game1.random.NextDouble() < 0.5 && !isCollidingWithMonster(new Rectangle(720, num16 * TileSize, 48, 48), null))
								{
									monsters.Add(new CowboyMonster(spawnQueue[n].First().X, new Point(15 * TileSize, num16 * TileSize)));
									if (whichRound > 0)
									{
										monsters.Last().health += whichRound * 2;
									}
									spawnQueue[n][0] = new Point(spawnQueue[n][0].X, spawnQueue[n][0].Y - 1);
									if (spawnQueue[n][0].Y <= 0)
									{
										spawnQueue[n].RemoveAt(0);
									}
									break;
								}
							}
							break;
						}
						case 2:
						{
							for (int num17 = 7; num17 < 10; num17++)
							{
								if (Game1.random.NextDouble() < 0.5 && !isCollidingWithMonster(new Rectangle(num17 * 16 * 3, 15 * TileSize, 48, 48), null))
								{
									monsters.Add(new CowboyMonster(spawnQueue[n].First().X, new Point(num17 * TileSize, 15 * TileSize)));
									if (whichRound > 0)
									{
										monsters.Last().health += whichRound * 2;
									}
									spawnQueue[n][0] = new Point(spawnQueue[n][0].X, spawnQueue[n][0].Y - 1);
									if (spawnQueue[n][0].Y <= 0)
									{
										spawnQueue[n].RemoveAt(0);
									}
									break;
								}
							}
							break;
						}
						case 3:
						{
							for (int num15 = 7; num15 < 10; num15++)
							{
								if (Game1.random.NextDouble() < 0.5 && !isCollidingWithMonster(new Rectangle(0, num15 * TileSize, 48, 48), null))
								{
									monsters.Add(new CowboyMonster(spawnQueue[n].First().X, new Point(0, num15 * TileSize)));
									if (whichRound > 0)
									{
										monsters.Last().health += whichRound * 2;
									}
									spawnQueue[n][0] = new Point(spawnQueue[n][0].X, spawnQueue[n][0].Y - 1);
									if (spawnQueue[n][0].Y <= 0)
									{
										spawnQueue[n].RemoveAt(0);
									}
									break;
								}
							}
							break;
						}
						}
					}
					if (waveTimer <= 0 && monsters.Count > 0 && isSpawnQueueEmpty())
					{
						bool flag = true;
						foreach (CowboyMonster monster in monsters)
						{
							if (monster.type != 6)
							{
								flag = false;
								break;
							}
						}
						if (flag)
						{
							foreach (CowboyMonster monster2 in monsters)
							{
								monster2.health = 1;
							}
						}
					}
					if (waveTimer <= 0 && monsters.Count == 0 && isSpawnQueueEmpty())
					{
						hasGopherAppeared = false;
						if (playingWithAbigail)
						{
							startAbigailPortrait(1, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11898"));
						}
						waveTimer = 80000;
						betweenWaveTimer = 3333;
						whichWave++;
						if (playingWithAbigail)
						{
							beatLevelWithAbigail = true;
							fadethenQuitTimer = 2000;
						}
						switch (whichWave)
						{
						case 1:
						case 2:
						case 3:
							monsterChances[0] = new Vector2(monsterChances[0].X + 0.001f, monsterChances[0].Y + 0.02f);
							if (whichWave > 1)
							{
								monsterChances[2] = new Vector2(monsterChances[2].X + 0.001f, monsterChances[2].Y + 0.01f);
							}
							monsterChances[6] = new Vector2(monsterChances[6].X + 0.001f, monsterChances[6].Y + 0.01f);
							if (whichRound > 0)
							{
								monsterChances[4] = new Vector2(0.002f, 0.1f);
							}
							break;
						case 4:
						case 5:
						case 6:
						case 7:
							if (monsterChances[5].Equals(Vector2.Zero))
							{
								monsterChances[5] = new Vector2(0.01f, 0.15f);
								if (whichRound > 0)
								{
									monsterChances[5] = new Vector2(0.01f + (float)whichRound * 0.004f, 0.15f + (float)whichRound * 0.04f);
								}
							}
							monsterChances[0] = Vector2.Zero;
							monsterChances[6] = Vector2.Zero;
							monsterChances[2] = new Vector2(monsterChances[2].X + 0.002f, monsterChances[2].Y + 0.02f);
							monsterChances[5] = new Vector2(monsterChances[5].X + 0.001f, monsterChances[5].Y + 0.02f);
							monsterChances[1] = new Vector2(monsterChances[1].X + 0.0018f, monsterChances[1].Y + 0.08f);
							if (whichRound > 0)
							{
								monsterChances[4] = new Vector2(0.001f, 0.1f);
							}
							break;
						case 8:
						case 9:
						case 10:
						case 11:
							monsterChances[5] = Vector2.Zero;
							monsterChances[1] = Vector2.Zero;
							monsterChances[2] = Vector2.Zero;
							if (monsterChances[3].Equals(Vector2.Zero))
							{
								monsterChances[3] = new Vector2(0.012f, 0.4f);
								if (whichRound > 0)
								{
									monsterChances[3] = new Vector2(0.012f + (float)whichRound * 0.005f, 0.4f + (float)whichRound * 0.075f);
								}
							}
							if (monsterChances[4].Equals(Vector2.Zero))
							{
								monsterChances[4] = new Vector2(0.003f, 0.1f);
							}
							monsterChances[3] = new Vector2(monsterChances[3].X + 0.002f, monsterChances[3].Y + 0.05f);
							monsterChances[4] = new Vector2(monsterChances[4].X + 0.0015f, monsterChances[4].Y + 0.04f);
							if (whichWave == 11)
							{
								monsterChances[4] = new Vector2(monsterChances[4].X + 0.01f, monsterChances[4].Y + 0.04f);
								monsterChances[3] = new Vector2(monsterChances[3].X - 0.01f, monsterChances[3].Y + 0.04f);
							}
							break;
						}
						if (whichRound > 0)
						{
							for (int num19 = 0; num19 < monsterChances.Count; num19++)
							{
								_ = monsterChances[num19];
								monsterChances[num19] *= 1.1f;
							}
						}
						if (whichWave > 0 && whichWave % 2 == 0)
						{
							startShoppingLevel();
						}
						else if (whichWave > 0)
						{
							waitingForPlayerToMoveDownAMap = true;
							if (!playingWithAbigail)
							{
								map[8, 15] = 3;
								map[7, 15] = 3;
								map[9, 15] = 3;
							}
						}
					}
				}
				if (playingWithAbigail)
				{
					updateAbigail(time);
				}
				for (int num20 = monsters.Count - 1; num20 >= 0; num20--)
				{
					monsters[num20].move(playerPosition, time);
					if (num20 < monsters.Count && monsters[num20].position.Intersects(playerBoundingBox) && playerInvincibleTimer <= 0)
					{
						if (zombieModeTimer <= 0)
						{
							playerDie();
							break;
						}
						if (monsters[num20].type != -2)
						{
							addGuts(monsters[num20].position.Location, monsters[num20].type);
							monsters.RemoveAt(num20);
							Game1.playSound("Cowboy_monsterDie");
						}
					}
					if (playingWithAbigail && num20 < monsters.Count && monsters[num20].position.Intersects(player2BoundingBox) && player2invincibletimer <= 0)
					{
						Game1.playSound("Cowboy_monsterDie");
						player2deathtimer = 3000;
						temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1808, 16, 16), 120f, 5, 0, player2Position + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
						player2invincibletimer = 4000;
						player2Position = new Vector2(8f, 8f) * TileSize;
						player2BoundingBox.X = (int)player2Position.X + TileSize / 4;
						player2BoundingBox.Y = (int)player2Position.Y + TileSize / 4;
						player2BoundingBox.Width = TileSize / 2;
						player2BoundingBox.Height = TileSize / 2;
						if (playerBoundingBox.Intersects(player2BoundingBox))
						{
							player2Position.X = playerBoundingBox.Right + 2;
						}
						player2BoundingBox.X = (int)player2Position.X + TileSize / 4;
						player2BoundingBox.Y = (int)player2Position.Y + TileSize / 4;
						player2BoundingBox.Width = TileSize / 2;
						player2BoundingBox.Height = TileSize / 2;
						startAbigailPortrait(5, (Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11901") : Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11902"));
					}
				}
			}
			return false;
		}

		protected void _ProcessInputs()
		{
			if (_buttonHeldFrames[GameKeys.MoveUp] > 0)
			{
				if (_buttonHeldFrames[GameKeys.MoveUp] == 1 && gameOver)
				{
					gameOverOption = Math.Max(0, gameOverOption - 1);
					Game1.playSound("Cowboy_gunshot");
				}
				addPlayerMovementDirection(0);
			}
			else if (playerMovementDirections.Contains(0))
			{
				playerMovementDirections.Remove(0);
			}
			if (_buttonHeldFrames[GameKeys.MoveDown] > 0)
			{
				if (_buttonHeldFrames[GameKeys.MoveDown] == 1 && gameOver)
				{
					gameOverOption = Math.Min(1, gameOverOption + 1);
					Game1.playSound("Cowboy_gunshot");
				}
				addPlayerMovementDirection(2);
			}
			else if (playerMovementDirections.Contains(2))
			{
				playerMovementDirections.Remove(2);
			}
			if (_buttonHeldFrames[GameKeys.MoveLeft] > 0)
			{
				addPlayerMovementDirection(3);
			}
			else if (playerMovementDirections.Contains(3))
			{
				playerMovementDirections.Remove(3);
			}
			if (_buttonHeldFrames[GameKeys.MoveRight] > 0)
			{
				addPlayerMovementDirection(1);
			}
			else if (playerMovementDirections.Contains(1))
			{
				playerMovementDirections.Remove(1);
			}
			if (_buttonHeldFrames[GameKeys.ShootUp] > 0)
			{
				addPlayerShootingDirection(0);
			}
			else if (playerShootingDirections.Contains(0))
			{
				playerShootingDirections.Remove(0);
			}
			if (_buttonHeldFrames[GameKeys.ShootDown] > 0)
			{
				addPlayerShootingDirection(2);
			}
			else if (playerShootingDirections.Contains(2))
			{
				playerShootingDirections.Remove(2);
			}
			if (_buttonHeldFrames[GameKeys.ShootLeft] > 0)
			{
				addPlayerShootingDirection(3);
			}
			else if (playerShootingDirections.Contains(3))
			{
				playerShootingDirections.Remove(3);
			}
			if (_buttonHeldFrames[GameKeys.ShootRight] > 0)
			{
				addPlayerShootingDirection(1);
			}
			else if (playerShootingDirections.Contains(1))
			{
				playerShootingDirections.Remove(1);
			}
			if (_buttonHeldFrames[GameKeys.SelectOption] == 1 && gameOver)
			{
				if (gameOverOption == 1)
				{
					quit = true;
				}
				else
				{
					gamerestartTimer = 1500;
					gameOver = false;
					gameOverOption = 0;
					Game1.playSound("Pickup_Coin15");
				}
			}
			if (_buttonHeldFrames[GameKeys.UsePowerup] == 1 && !gameOver && heldItem != null && deathTimer <= 0f && zombieModeTimer <= 0)
			{
				usePowerup(heldItem.which);
				heldItem = null;
			}
			if (_buttonHeldFrames[GameKeys.Exit] == 1 && !playingWithAbigail)
			{
				quit = true;
			}
		}

		public virtual void ApplyLevelSpecificStates()
		{
			if (whichWave == 12)
			{
				shootoutLevel = true;
				monsters.Add(new Dracula());
				if (whichRound > 0)
				{
					monsters.Last().health *= 2;
				}
			}
			else if (whichWave > 0 && whichWave % 4 == 0)
			{
				shootoutLevel = true;
				monsters.Add(new Outlaw(new Point(8 * TileSize, 13 * TileSize), (world == 0) ? 50 : 100));
				if (Game1.soundBank != null && Game1.musicWaveBankState == Game1.MusicWaveBankState.Created)
				{
					outlawSong = Game1.soundBank.GetCue("cowboy_outlawsong");
					outlawSong.Play();
				}
			}
		}

		public void updateAbigail(GameTime time)
		{
			player2TargetUpdateTimer -= time.ElapsedGameTime.Milliseconds;
			if (player2deathtimer > 0)
			{
				player2deathtimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (player2invincibletimer > 0)
			{
				player2invincibletimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (player2deathtimer > 0)
			{
				return;
			}
			if (player2TargetUpdateTimer < 0)
			{
				player2TargetUpdateTimer = 500;
				CowboyMonster cowboyMonster = null;
				double num = 99999.0;
				foreach (CowboyMonster monster in monsters)
				{
					double num2 = Math.Sqrt(Math.Pow((float)monster.position.X - player2Position.X, 2.0) - Math.Pow((float)monster.position.Y - player2Position.Y, 2.0));
					if (cowboyMonster == null || num2 < num)
					{
						cowboyMonster = monster;
						num = Math.Sqrt(Math.Pow((float)cowboyMonster.position.X - player2Position.X, 2.0) - Math.Pow((float)cowboyMonster.position.Y - player2Position.Y, 2.0));
					}
				}
				targetMonster = cowboyMonster;
			}
			player2ShootingDirections.Clear();
			player2MovementDirections.Clear();
			if (targetMonster != null)
			{
				double num3 = Math.Sqrt(Math.Pow((float)targetMonster.position.X - player2Position.X, 2.0) - Math.Pow((float)targetMonster.position.Y - player2Position.Y, 2.0));
				if (num3 < (double)(TileSize * 3))
				{
					if ((float)targetMonster.position.X > player2Position.X)
					{
						addPlayer2MovementDirection(3);
					}
					else if ((float)targetMonster.position.X < player2Position.X)
					{
						addPlayer2MovementDirection(1);
					}
					if ((float)targetMonster.position.Y > player2Position.Y)
					{
						addPlayer2MovementDirection(0);
					}
					else if ((float)targetMonster.position.Y < player2Position.Y)
					{
						addPlayer2MovementDirection(2);
					}
					foreach (int player2MovementDirection in player2MovementDirections)
					{
						player2ShootingDirections.Add((player2MovementDirection + 2) % 4);
					}
				}
				else
				{
					if (Math.Abs((float)targetMonster.position.X - player2Position.X) > Math.Abs((float)targetMonster.position.Y - player2Position.Y) && Math.Abs((float)targetMonster.position.Y - player2Position.Y) > 4f)
					{
						if ((float)targetMonster.position.Y > player2Position.Y + 3f)
						{
							addPlayer2MovementDirection(2);
						}
						else if ((float)targetMonster.position.Y < player2Position.Y - 3f)
						{
							addPlayer2MovementDirection(0);
						}
					}
					else if (Math.Abs((float)targetMonster.position.X - player2Position.X) > 4f)
					{
						if ((float)targetMonster.position.X > player2Position.X + 3f)
						{
							addPlayer2MovementDirection(1);
						}
						else if ((float)targetMonster.position.X < player2Position.X - 3f)
						{
							addPlayer2MovementDirection(3);
						}
					}
					if ((float)targetMonster.position.X > player2Position.X + 3f)
					{
						addPlayer2ShootingDirection(1);
					}
					else if ((float)targetMonster.position.X < player2Position.X - 3f)
					{
						addPlayer2ShootingDirection(3);
					}
					if ((float)targetMonster.position.Y > player2Position.Y + 3f)
					{
						addPlayer2ShootingDirection(2);
					}
					else if ((float)targetMonster.position.Y < player2Position.Y - 3f)
					{
						addPlayer2ShootingDirection(0);
					}
				}
			}
			if (player2MovementDirections.Count > 0)
			{
				float movementSpeed = getMovementSpeed(3f, player2MovementDirections.Count);
				for (int i = 0; i < player2MovementDirections.Count; i++)
				{
					Vector2 vector = player2Position;
					switch (player2MovementDirections[i])
					{
					case 0:
						vector.Y -= movementSpeed;
						break;
					case 3:
						vector.X -= movementSpeed;
						break;
					case 2:
						vector.Y += movementSpeed;
						break;
					case 1:
						vector.X += movementSpeed;
						break;
					}
					Rectangle rectangle = new Rectangle((int)vector.X + TileSize / 4, (int)vector.Y + TileSize / 4, TileSize / 2, TileSize / 2);
					if (!isCollidingWithMap(rectangle) && (!merchantBox.Intersects(rectangle) || merchantBox.Intersects(player2BoundingBox)) && !rectangle.Intersects(playerBoundingBox))
					{
						player2Position = vector;
					}
				}
				player2BoundingBox.X = (int)player2Position.X + TileSize / 4;
				player2BoundingBox.Y = (int)player2Position.Y + TileSize / 4;
				player2BoundingBox.Width = TileSize / 2;
				player2BoundingBox.Height = TileSize / 2;
				player2AnimationTimer += time.ElapsedGameTime.Milliseconds;
				player2AnimationTimer %= 400;
				player2FootstepSoundTimer -= time.ElapsedGameTime.Milliseconds;
				if (player2FootstepSoundTimer <= 0)
				{
					Game1.playSound("Cowboy_Footstep");
					player2FootstepSoundTimer = 200;
				}
				for (int num4 = powerups.Count - 1; num4 >= 0; num4--)
				{
					if (player2BoundingBox.Intersects(new Rectangle(powerups[num4].position.X, powerups[num4].position.Y, TileSize, TileSize)) && !player2BoundingBox.Intersects(noPickUpBox))
					{
						powerups.RemoveAt(num4);
					}
				}
			}
			player2shotTimer -= time.ElapsedGameTime.Milliseconds;
			if (player2ShootingDirections.Count > 0 && player2shotTimer <= 0)
			{
				if (player2ShootingDirections.Count == 1)
				{
					spawnBullets(new int[1] { player2ShootingDirections[0] }, player2Position);
				}
				else
				{
					spawnBullets(player2ShootingDirections.ToArray(), player2Position);
				}
				Game1.playSound("Cowboy_gunshot");
				player2shotTimer = shootingDelay;
			}
		}

		public int[,] getMap(int wave)
		{
			int[,] array = new int[16, 16];
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					if ((i == 0 || i == 15 || j == 0 || j == 15) && (i <= 6 || i >= 10) && (j <= 6 || j >= 10))
					{
						array[i, j] = 5;
					}
					else if (i == 0 || i == 15 || j == 0 || j == 15)
					{
						array[i, j] = ((Game1.random.NextDouble() < 0.15) ? 1 : 0);
					}
					else if (i == 1 || i == 14 || j == 1 || j == 14)
					{
						array[i, j] = 2;
					}
					else
					{
						array[i, j] = ((Game1.random.NextDouble() < 0.1) ? 4 : 3);
					}
				}
			}
			switch (wave)
			{
			case -1:
			{
				for (int num = 0; num < 16; num++)
				{
					for (int num2 = 0; num2 < 16; num2++)
					{
						if (array[num, num2] == 0 || array[num, num2] == 1 || array[num, num2] == 2 || array[num, num2] == 5)
						{
							array[num, num2] = 3;
						}
					}
				}
				array[3, 1] = 5;
				array[8, 2] = 5;
				array[13, 1] = 5;
				array[5, 0] = 0;
				array[10, 2] = 2;
				array[15, 2] = 1;
				array[14, 12] = 5;
				array[10, 6] = 7;
				array[11, 6] = 7;
				array[12, 6] = 7;
				array[13, 6] = 7;
				array[14, 6] = 7;
				array[14, 7] = 7;
				array[14, 8] = 7;
				array[14, 9] = 7;
				array[14, 10] = 7;
				array[14, 11] = 7;
				array[14, 12] = 7;
				array[14, 13] = 7;
				for (int num3 = 0; num3 < 16; num3++)
				{
					array[num3, 3] = ((num3 % 2 == 0) ? 9 : 8);
				}
				array[3, 3] = 10;
				array[7, 8] = 2;
				array[8, 8] = 2;
				array[4, 11] = 2;
				array[11, 12] = 2;
				array[9, 11] = 2;
				array[3, 9] = 2;
				array[2, 12] = 5;
				array[8, 13] = 5;
				array[12, 11] = 5;
				array[7, 14] = 0;
				array[6, 14] = 2;
				array[8, 14] = 2;
				array[7, 13] = 2;
				array[7, 15] = 2;
				break;
			}
			case 1:
				array[4, 4] = 7;
				array[4, 5] = 7;
				array[5, 4] = 7;
				array[12, 4] = 7;
				array[11, 4] = 7;
				array[12, 5] = 7;
				array[4, 12] = 7;
				array[5, 12] = 7;
				array[4, 11] = 7;
				array[12, 12] = 7;
				array[11, 12] = 7;
				array[12, 11] = 7;
				break;
			case 2:
				array[8, 4] = 7;
				array[12, 8] = 7;
				array[8, 12] = 7;
				array[4, 8] = 7;
				array[1, 1] = 5;
				array[14, 1] = 5;
				array[14, 14] = 5;
				array[1, 14] = 5;
				array[2, 1] = 5;
				array[13, 1] = 5;
				array[13, 14] = 5;
				array[2, 14] = 5;
				array[1, 2] = 5;
				array[14, 2] = 5;
				array[14, 13] = 5;
				array[1, 13] = 5;
				break;
			case 3:
				array[5, 5] = 7;
				array[6, 5] = 7;
				array[7, 5] = 7;
				array[9, 5] = 7;
				array[10, 5] = 7;
				array[11, 5] = 7;
				array[5, 11] = 7;
				array[6, 11] = 7;
				array[7, 11] = 7;
				array[9, 11] = 7;
				array[10, 11] = 7;
				array[11, 11] = 7;
				array[5, 6] = 7;
				array[5, 7] = 7;
				array[5, 9] = 7;
				array[5, 10] = 7;
				array[11, 6] = 7;
				array[11, 7] = 7;
				array[11, 9] = 7;
				array[11, 10] = 7;
				break;
			case 4:
			case 8:
			{
				for (int num5 = 0; num5 < 16; num5++)
				{
					for (int num6 = 0; num6 < 16; num6++)
					{
						if (array[num5, num6] == 5)
						{
							array[num5, num6] = ((!(Game1.random.NextDouble() < 0.5)) ? 1 : 0);
						}
					}
				}
				for (int num7 = 0; num7 < 16; num7++)
				{
					array[num7, 8] = ((Game1.random.NextDouble() < 0.5) ? 8 : 9);
				}
				array[8, 4] = 7;
				array[8, 12] = 7;
				array[9, 12] = 7;
				array[7, 12] = 7;
				array[5, 6] = 5;
				array[10, 6] = 5;
				break;
			}
			case 5:
				array[1, 1] = 5;
				array[14, 1] = 5;
				array[14, 14] = 5;
				array[1, 14] = 5;
				array[2, 1] = 5;
				array[13, 1] = 5;
				array[13, 14] = 5;
				array[2, 14] = 5;
				array[1, 2] = 5;
				array[14, 2] = 5;
				array[14, 13] = 5;
				array[1, 13] = 5;
				array[3, 1] = 5;
				array[13, 1] = 5;
				array[13, 13] = 5;
				array[1, 13] = 5;
				array[1, 3] = 5;
				array[13, 3] = 5;
				array[12, 13] = 5;
				array[3, 14] = 5;
				array[3, 3] = 5;
				array[13, 12] = 5;
				array[13, 12] = 5;
				array[3, 12] = 5;
				break;
			case 6:
				array[4, 5] = 2;
				array[12, 10] = 5;
				array[10, 9] = 5;
				array[5, 12] = 2;
				array[5, 9] = 5;
				array[12, 12] = 5;
				array[3, 4] = 5;
				array[2, 3] = 5;
				array[11, 3] = 5;
				array[10, 6] = 5;
				array[5, 9] = 7;
				array[10, 12] = 7;
				array[3, 12] = 7;
				array[10, 8] = 7;
				break;
			case 7:
			{
				for (int n = 0; n < 16; n++)
				{
					array[n, 5] = ((n % 2 == 0) ? 9 : 8);
					array[n, 10] = ((n % 2 == 0) ? 9 : 8);
				}
				array[4, 5] = 10;
				array[8, 5] = 10;
				array[12, 5] = 10;
				array[4, 10] = 10;
				array[8, 10] = 10;
				array[12, 10] = 10;
				break;
			}
			case 9:
				array[4, 4] = 5;
				array[5, 4] = 5;
				array[10, 4] = 5;
				array[12, 4] = 5;
				array[4, 5] = 5;
				array[5, 5] = 5;
				array[10, 5] = 5;
				array[12, 5] = 5;
				array[4, 10] = 5;
				array[5, 10] = 5;
				array[10, 10] = 5;
				array[12, 10] = 5;
				array[4, 12] = 5;
				array[5, 12] = 5;
				array[10, 12] = 5;
				array[12, 12] = 5;
				break;
			case 10:
			{
				for (int num8 = 0; num8 < 16; num8++)
				{
					array[num8, 1] = ((num8 % 2 == 0) ? 9 : 8);
					array[num8, 14] = ((num8 % 2 == 0) ? 9 : 8);
				}
				array[8, 1] = 10;
				array[7, 1] = 10;
				array[9, 1] = 10;
				array[8, 14] = 10;
				array[7, 14] = 10;
				array[9, 14] = 10;
				array[6, 8] = 5;
				array[10, 8] = 5;
				array[8, 6] = 5;
				array[8, 9] = 5;
				break;
			}
			case 11:
			{
				for (int num4 = 0; num4 < 16; num4++)
				{
					array[num4, 0] = 7;
					array[num4, 15] = 7;
					if (num4 % 2 == 0)
					{
						array[num4, 1] = 5;
						array[num4, 14] = 5;
					}
				}
				break;
			}
			case 12:
			{
				for (int k = 0; k < 16; k++)
				{
					for (int l = 0; l < 16; l++)
					{
						if (array[k, l] == 0 || array[k, l] == 1)
						{
							array[k, l] = 5;
						}
					}
				}
				for (int m = 0; m < 16; m++)
				{
					array[m, 0] = ((m % 2 == 0) ? 9 : 8);
					array[m, 15] = ((m % 2 == 0) ? 9 : 8);
				}
				Rectangle r = new Rectangle(1, 1, 14, 14);
				foreach (Vector2 item in Utility.getBorderOfThisRectangle(r))
				{
					array[(int)item.X, (int)item.Y] = 10;
				}
				r.Inflate(-1, -1);
				{
					foreach (Vector2 item2 in Utility.getBorderOfThisRectangle(r))
					{
						array[(int)item2.X, (int)item2.Y] = 2;
					}
					return array;
				}
			}
			default:
				array[4, 4] = 5;
				array[12, 4] = 5;
				array[4, 12] = 5;
				array[12, 12] = 5;
				break;
			}
			return array;
		}

		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!quit && upperRightCloseButton.containsPoint(x, y) && !endCutscene && !gopherTrain && !gameOver && gamerestartTimer <= 0 && !onStartMenu)
			{
				quit = true;
			}
			if (gameOver)
			{
				if (gamerestartTimer <= 0 && buttonRestart.containsPoint(x, y))
				{
					gamerestartTimer = 1500;
					gameOver = false;
					gameOverOption = 0;
					Game1.playSound("Pickup_Coin15");
				}
				else if (!quit && buttonQuit.containsPoint(x, y))
				{
					quit = true;
				}
			}
			else if (heldItem != null && buttonPowerup.containsPoint(x, y))
			{
				usePowerup(heldItem.which);
				heldItem = null;
			}
		}

		public void leftClickHeld(int x, int y)
		{
		}

		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public void releaseLeftClick(int x, int y)
		{
		}

		public void releaseRightClick(int x, int y)
		{
		}

		public void spawnBullets(int[] directions, Vector2 spawn)
		{
			Point position = new Point((int)spawn.X + 24, (int)spawn.Y + 24 - 6);
			int num = (int)getMovementSpeed(8f, 2);
			if (directions.Length == 1)
			{
				int num2 = directions[0];
				switch (num2)
				{
				case 0:
					position.Y -= 22;
					break;
				case 1:
					position.X += 16;
					position.Y -= 6;
					break;
				case 2:
					position.Y += 10;
					break;
				case 3:
					position.X -= 16;
					position.Y -= 6;
					break;
				}
				bullets.Add(new CowboyBullet(position, num2, bulletDamage));
				if (activePowerups.ContainsKey(7) || spreadPistol)
				{
					switch (num2)
					{
					case 0:
						bullets.Add(new CowboyBullet(new Point(position.X, position.Y), new Point(-2, -8), bulletDamage));
						bullets.Add(new CowboyBullet(new Point(position.X, position.Y), new Point(2, -8), bulletDamage));
						break;
					case 1:
						bullets.Add(new CowboyBullet(new Point(position.X, position.Y), new Point(8, -2), bulletDamage));
						bullets.Add(new CowboyBullet(new Point(position.X, position.Y), new Point(8, 2), bulletDamage));
						break;
					case 2:
						bullets.Add(new CowboyBullet(new Point(position.X, position.Y), new Point(-2, 8), bulletDamage));
						bullets.Add(new CowboyBullet(new Point(position.X, position.Y), new Point(2, 8), bulletDamage));
						break;
					case 3:
						bullets.Add(new CowboyBullet(new Point(position.X, position.Y), new Point(-8, -2), bulletDamage));
						bullets.Add(new CowboyBullet(new Point(position.X, position.Y), new Point(-8, 2), bulletDamage));
						break;
					}
				}
			}
			else if (directions.Contains(0) && directions.Contains(1))
			{
				position.X += TileSize / 2;
				position.Y -= TileSize / 2;
				bullets.Add(new CowboyBullet(position, new Point(num, -num), bulletDamage));
				if (activePowerups.ContainsKey(7) || spreadPistol)
				{
					int num3 = -2;
					bullets.Add(new CowboyBullet(position, new Point(num + num3, -num + num3), bulletDamage));
					num3 = 2;
					bullets.Add(new CowboyBullet(position, new Point(num + num3, -num + num3), bulletDamage));
				}
			}
			else if (directions.Contains(0) && directions.Contains(3))
			{
				position.X -= TileSize / 2;
				position.Y -= TileSize / 2;
				bullets.Add(new CowboyBullet(position, new Point(-num, -num), bulletDamage));
				if (activePowerups.ContainsKey(7) || spreadPistol)
				{
					int num4 = -2;
					bullets.Add(new CowboyBullet(position, new Point(-num - num4, -num + num4), bulletDamage));
					num4 = 2;
					bullets.Add(new CowboyBullet(position, new Point(-num - num4, -num + num4), bulletDamage));
				}
			}
			else if (directions.Contains(2) && directions.Contains(1))
			{
				position.X += TileSize / 2;
				position.Y += TileSize / 4;
				bullets.Add(new CowboyBullet(position, new Point(num, num), bulletDamage));
				if (activePowerups.ContainsKey(7) || spreadPistol)
				{
					int num5 = -2;
					bullets.Add(new CowboyBullet(position, new Point(num - num5, num + num5), bulletDamage));
					num5 = 2;
					bullets.Add(new CowboyBullet(position, new Point(num - num5, num + num5), bulletDamage));
				}
			}
			else if (directions.Contains(2) && directions.Contains(3))
			{
				position.X -= TileSize / 2;
				position.Y += TileSize / 4;
				bullets.Add(new CowboyBullet(position, new Point(-num, num), bulletDamage));
				if (activePowerups.ContainsKey(7) || spreadPistol)
				{
					int num6 = -2;
					bullets.Add(new CowboyBullet(position, new Point(-num + num6, num + num6), bulletDamage));
					num6 = 2;
					bullets.Add(new CowboyBullet(position, new Point(-num + num6, num + num6), bulletDamage));
				}
			}
		}

		public bool isSpawnQueueEmpty()
		{
			for (int i = 0; i < 4; i++)
			{
				if (spawnQueue[i].Count > 0)
				{
					return false;
				}
			}
			return true;
		}

		public static bool isMapTilePassable(int tileType)
		{
			if ((uint)tileType <= 1u || (uint)(tileType - 5) <= 4u)
			{
				return false;
			}
			return true;
		}

		public static bool isMapTilePassableForMonsters(int tileType)
		{
			if (tileType == 5 || (uint)(tileType - 7) <= 2u)
			{
				return false;
			}
			return true;
		}

		public static bool isCollidingWithMonster(Rectangle r, CowboyMonster subject)
		{
			foreach (CowboyMonster monster in monsters)
			{
				if ((subject == null || !subject.Equals(monster)) && Math.Abs(monster.position.X - r.X) < 48 && Math.Abs(monster.position.Y - r.Y) < 48 && r.Intersects(new Rectangle(monster.position.X, monster.position.Y, 48, 48)))
				{
					return true;
				}
			}
			return false;
		}

		public static bool isCollidingWithMapForMonsters(Rectangle positionToCheck)
		{
			for (int i = 0; i < 4; i++)
			{
				Vector2 cornersOfThisRectangle = Utility.getCornersOfThisRectangle(ref positionToCheck, i);
				if (cornersOfThisRectangle.X < 0f || cornersOfThisRectangle.Y < 0f || cornersOfThisRectangle.X >= 768f || cornersOfThisRectangle.Y >= 768f || !isMapTilePassableForMonsters(map[(int)cornersOfThisRectangle.X / 16 / 3, (int)cornersOfThisRectangle.Y / 16 / 3]))
				{
					return true;
				}
			}
			return false;
		}

		public static bool isCollidingWithMap(Rectangle positionToCheck)
		{
			for (int i = 0; i < 4; i++)
			{
				Vector2 cornersOfThisRectangle = Utility.getCornersOfThisRectangle(ref positionToCheck, i);
				if (cornersOfThisRectangle.X < 0f || cornersOfThisRectangle.Y < 0f || cornersOfThisRectangle.X >= 768f || cornersOfThisRectangle.Y >= 768f || !isMapTilePassable(map[(int)cornersOfThisRectangle.X / 16 / 3, (int)cornersOfThisRectangle.Y / 16 / 3]))
				{
					return true;
				}
			}
			return false;
		}

		public static bool isCollidingWithMap(Point position)
		{
			Rectangle r = new Rectangle(position.X, position.Y, 48, 48);
			for (int i = 0; i < 4; i++)
			{
				Vector2 cornersOfThisRectangle = Utility.getCornersOfThisRectangle(ref r, i);
				if (cornersOfThisRectangle.X < 0f || cornersOfThisRectangle.Y < 0f || cornersOfThisRectangle.X >= 768f || cornersOfThisRectangle.Y >= 768f || !isMapTilePassable(map[(int)cornersOfThisRectangle.X / 16 / 3, (int)cornersOfThisRectangle.Y / 16 / 3]))
				{
					return true;
				}
			}
			return false;
		}

		public static bool isCollidingWithMap(Vector2 position)
		{
			Rectangle r = new Rectangle((int)position.X, (int)position.Y, 48, 48);
			for (int i = 0; i < 4; i++)
			{
				Vector2 cornersOfThisRectangle = Utility.getCornersOfThisRectangle(ref r, i);
				if (cornersOfThisRectangle.X < 0f || cornersOfThisRectangle.Y < 0f || cornersOfThisRectangle.X >= 768f || cornersOfThisRectangle.Y >= 768f || !isMapTilePassable(map[(int)cornersOfThisRectangle.X / 16 / 3, (int)cornersOfThisRectangle.Y / 16 / 3]))
				{
					return true;
				}
			}
			return false;
		}

		private void addPlayer2MovementDirection(int direction)
		{
			if (!player2MovementDirections.Contains(direction))
			{
				if (player2MovementDirections.Count == 1 && direction == (player2MovementDirections[0] + 2) % 4)
				{
					player2MovementDirections.Clear();
				}
				player2MovementDirections.Add(direction);
				if (player2MovementDirections.Count > 2)
				{
					player2MovementDirections.RemoveAt(0);
				}
			}
		}

		private void addPlayerMovementDirection(int direction)
		{
			if (!gopherTrain && !playerMovementDirections.Contains(direction))
			{
				if (playerMovementDirections.Count == 1)
				{
					_ = (playerMovementDirections.ElementAt(0) + 2) % 4;
				}
				playerMovementDirections.Add(direction);
			}
		}

		private void addPlayer2ShootingDirection(int direction)
		{
			if (!player2ShootingDirections.Contains(direction))
			{
				if (player2ShootingDirections.Count == 1 && direction == (player2ShootingDirections[0] + 2) % 4)
				{
					player2ShootingDirections.Clear();
				}
				player2ShootingDirections.Add(direction);
				if (player2ShootingDirections.Count > 2)
				{
					player2ShootingDirections.RemoveAt(0);
				}
			}
		}

		private void addPlayerShootingDirection(int direction)
		{
			if (!playerShootingDirections.Contains(direction))
			{
				playerShootingDirections.Add(direction);
			}
		}

		public void startShoppingLevel()
		{
			merchantBox.Y = -TileSize;
			shopping = true;
			merchantArriving = true;
			merchantLeaving = false;
			merchantShopOpen = false;
			if (overworldSong != null)
			{
				overworldSong.Stop(AudioStopOptions.Immediate);
			}
			monsters.Clear();
			waitingForPlayerToMoveDownAMap = true;
			storeItems.Clear();
			int num = whichWave;
			if (num == 2)
			{
				storeItems.Add(new Rectangle(7 * TileSize + 12, 8 * TileSize - TileSize * 2, TileSize, TileSize), 3);
				storeItems.Add(new Rectangle(8 * TileSize + 24, 8 * TileSize - TileSize * 2, TileSize, TileSize), 0);
				storeItems.Add(new Rectangle(9 * TileSize + 36, 8 * TileSize - TileSize * 2, TileSize, TileSize), 6);
			}
			else
			{
				storeItems.Add(new Rectangle(7 * TileSize + 12, 8 * TileSize - TileSize * 2, TileSize, TileSize), (runSpeedLevel >= 2) ? 5 : (3 + runSpeedLevel));
				storeItems.Add(new Rectangle(8 * TileSize + 24, 8 * TileSize - TileSize * 2, TileSize, TileSize), (fireSpeedLevel < 3) ? fireSpeedLevel : ((ammoLevel >= 3 && !spreadPistol) ? 9 : 10));
				storeItems.Add(new Rectangle(9 * TileSize + 36, 8 * TileSize - TileSize * 2, TileSize, TileSize), (ammoLevel < 3) ? (6 + ammoLevel) : 10);
			}
			if (whichRound > 0)
			{
				storeItems.Clear();
				storeItems.Add(new Rectangle(7 * TileSize + 12, 8 * TileSize - TileSize * 2, TileSize, TileSize), (runSpeedLevel >= 2) ? 5 : (3 + runSpeedLevel));
				storeItems.Add(new Rectangle(8 * TileSize + 24, 8 * TileSize - TileSize * 2, TileSize, TileSize), (fireSpeedLevel < 3) ? fireSpeedLevel : ((ammoLevel >= 3 && !spreadPistol) ? 9 : 10));
				storeItems.Add(new Rectangle(9 * TileSize + 36, 8 * TileSize - TileSize * 2, TileSize, TileSize), (ammoLevel < 3) ? (6 + ammoLevel) : 10);
			}
		}

		public void receiveKeyPress(Keys k)
		{
			if (onStartMenu)
			{
				startTimer = 1;
			}
		}

		public void receiveKeyRelease(Keys k)
		{
		}

		public int getPriceForItem(int whichItem)
		{
			return whichItem switch
			{
				6 => 15, 
				7 => 30, 
				8 => 45, 
				0 => 10, 
				1 => 20, 
				2 => 30, 
				5 => 10, 
				3 => 8, 
				4 => 20, 
				9 => 99, 
				10 => 10, 
				_ => 5, 
			};
		}

		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (onStartMenu)
			{
				b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y, 16 * TileSize, 16 * TileSize), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.97f);
				b.Draw(Game1.mouseCursors, new Vector2(Game1.viewport.Width / 2 - 3 * TileSize, topLeftScreenCoordinate.Y + (float)(5 * TileSize)), new Rectangle(128, 1744, 96, 56), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
			}
			else if ((gameOver || gamerestartTimer > 0) && !endCutscene)
			{
				b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y, 16 * TileSize, 16 * TileSize), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.0001f);
				Vector2 vector = topLeftScreenCoordinate + new Vector2(6f, 9f) * TileSize;
				Vector2 vector2 = topLeftScreenCoordinate + new Vector2(6f, 9f) * TileSize + new Vector2(0f, 50f);
				buttonRestart.bounds = new Rectangle((int)topLeftScreenCoordinate.X, (int)vector.Y - 10, 16 * TileSize, 50);
				buttonQuit.bounds = new Rectangle((int)topLeftScreenCoordinate.X, (int)vector2.Y - 10, 16 * TileSize, 50);
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11914"), topLeftScreenCoordinate + new Vector2(6f, 7f) * TileSize, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11914"), topLeftScreenCoordinate + new Vector2(6f, 7f) * TileSize + new Vector2(-1f, 0f), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11914"), topLeftScreenCoordinate + new Vector2(6f, 7f) * TileSize + new Vector2(1f, 0f), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
				string text = Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11919");
				string text2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11917");
				if (Game1.options.gamepadControls)
				{
					if (gameOverOption == 0)
					{
						text2 = "> " + text2;
					}
					if (gameOverOption == 1)
					{
						text = "> " + text;
					}
				}
				if (gamerestartTimer <= 0 || gamerestartTimer / 500 % 2 == 0)
				{
					b.DrawString(Game1.smallFont, text2, topLeftScreenCoordinate + new Vector2(6f, 9f) * TileSize, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
				}
				b.DrawString(Game1.smallFont, text, topLeftScreenCoordinate + new Vector2(6f, 9f) * TileSize + new Vector2(0f, TileSize * 2 / 3), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
			}
			else if (endCutscene)
			{
				switch (endCutscenePhase)
				{
				case 0:
					b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y, 16 * TileSize, 16 * TileSize), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.0001f);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize / 4), new Rectangle(384, 1760, 16, 16), Color.White * ((endCutsceneTimer < 2000) ? (1f * ((float)endCutsceneTimer / 2000f)) : 1f), 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.001f);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize * 2 / 3) + new Vector2(0f, -TileSize / 4), new Rectangle(320 + itemToHold * 16, 1776, 16, 16), Color.White * ((endCutsceneTimer < 2000) ? (1f * ((float)endCutsceneTimer / 2000f)) : 1f), 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.002f);
					break;
				case 4:
				case 5:
					b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y, 16 * TileSize, 16 * TileSize), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.97f);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(6 * TileSize, 3 * TileSize), new Rectangle(224, 1744, 64, 48), Color.White * ((endCutsceneTimer > 0) ? (1f - ((float)endCutsceneTimer - 2000f) / 2000f) : 1f), 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
					if (endCutscenePhase == 5 && gamerestartTimer <= 0)
					{
						b.DrawString(Game1.smallFont, Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_PK_NewGame+"), topLeftScreenCoordinate + new Vector2(3f, 10f) * TileSize, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
					}
					break;
				case 1:
				case 2:
				case 3:
				{
					for (int i = 0; i < 16; i++)
					{
						for (int j = 0; j < 16; j++)
						{
							b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(i, j) * 16f * 3f + new Vector2(0f, newMapPosition - 16 * TileSize), new Rectangle(464 + 16 * map[i, j] + ((map[i, j] == 5 && cactusDanceTimer > 800f) ? 16 : 0), 1680 - world * 16, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
						}
					}
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(6 * TileSize, 3 * TileSize), new Rectangle(288, 1697, 64, 80), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.01f);
					if (endCutscenePhase == 3)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(9 * TileSize, 7 * TileSize), new Rectangle(544, 1792, 32, 32), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.05f);
						if (endCutsceneTimer < 3000)
						{
							b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y, 16 * TileSize, 16 * TileSize), Game1.staminaRect.Bounds, Color.Black * (1f - (float)endCutsceneTimer / 3000f), 0f, Vector2.Zero, SpriteEffects.None, 1f);
						}
						break;
					}
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(10 * TileSize, 8 * TileSize), new Rectangle(272 - endCutsceneTimer / 300 % 4 * 16, 1792, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.02f);
					if (endCutscenePhase == 2)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(4f, 13f) * 3f, new Rectangle(484, 1760 + (int)(playerMotionAnimationTimer / 100f) * 3, 8, 3), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.001f + 0.001f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition, new Rectangle(384, 1760, 16, 13), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.002f + 0.001f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize * 2 / 3 - TileSize / 4), new Rectangle(320 + itemToHold * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.005f);
					}
					b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y, 16 * TileSize, 16 * TileSize), Game1.staminaRect.Bounds, Color.Black * ((endCutscenePhase == 1 && endCutsceneTimer > 12500) ? ((float)((endCutsceneTimer - 12500) / 3000)) : 0f), 0f, Vector2.Zero, SpriteEffects.None, 1f);
					break;
				}
				}
			}
			else
			{
				if (zombieModeTimer > 8200)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition, new Rectangle(384 + ((zombieModeTimer / 200 % 2 == 0) ? 16 : 0), 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
					for (int num = (int)(playerPosition.Y - (float)TileSize); num > -TileSize; num -= TileSize)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(playerPosition.X, num), new Rectangle(368 + ((num / TileSize % 3 == 0) ? 16 : 0), 1744, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
					}
					b.End();
					return;
				}
				for (int k = 0; k < 16; k++)
				{
					for (int l = 0; l < 16; l++)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(k, l) * 16f * 3f + new Vector2(0f, newMapPosition - 16 * TileSize), new Rectangle(464 + 16 * map[k, l] + ((map[k, l] == 5 && cactusDanceTimer > 800f) ? 16 : 0), 1680 - world * 16, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
					}
				}
				if (scrollingMap)
				{
					for (int m = 0; m < 16; m++)
					{
						for (int n = 0; n < 16; n++)
						{
							b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(m, n) * 16f * 3f + new Vector2(0f, newMapPosition), new Rectangle(464 + 16 * nextMap[m, n] + ((nextMap[m, n] == 5 && cactusDanceTimer > 800f) ? 16 : 0), 1680 - world * 16, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
						}
					}
					b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, -1, 16 * TileSize, (int)topLeftScreenCoordinate.Y), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 1f);
					b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y + 16 * TileSize, 16 * TileSize, (int)topLeftScreenCoordinate.Y + 2), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 1f);
				}
				if (deathTimer <= 0f && (playerInvincibleTimer <= 0 || playerInvincibleTimer / 100 % 2 == 0))
				{
					if (holdItemTimer > 0)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize / 4), new Rectangle(384, 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.001f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize * 2 / 3) + new Vector2(0f, -TileSize / 4), new Rectangle(320 + itemToHold * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.002f);
					}
					else if (zombieModeTimer > 0)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize / 4), new Rectangle(352 + ((zombieModeTimer / 50 % 2 == 0) ? 16 : 0), 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.001f);
					}
					else if (playerMovementDirections.Count == 0 && playerShootingDirections.Count == 0)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize / 4), new Rectangle(496, 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.001f);
					}
					else
					{
						int num2 = ((playerShootingDirections.Count == 0) ? playerMovementDirections.ElementAt(0) : playerShootingDirections.Last());
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize / 4) + new Vector2(4f, 13f) * 3f, new Rectangle(483, 1760 + (int)(playerMotionAnimationTimer / 100f) * 3, 10, 3), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.001f + 0.001f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(3f, -TileSize / 4), new Rectangle(464 + num2 * 16, 1744, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.002f + 0.001f);
					}
				}
				if (playingWithAbigail && player2deathtimer <= 0 && (player2invincibletimer <= 0 || player2invincibletimer / 100 % 2 == 0))
				{
					if (player2MovementDirections.Count == 0 && player2ShootingDirections.Count == 0)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + player2Position + new Vector2(0f, -TileSize / 4), new Rectangle(256, 1728, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.001f);
					}
					else
					{
						int num3 = ((player2ShootingDirections.Count == 0) ? player2MovementDirections[0] : player2ShootingDirections[0]);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + player2Position + new Vector2(0f, -TileSize / 4) + new Vector2(4f, 13f) * 3f, new Rectangle(243, 1728 + player2AnimationTimer / 100 * 3, 10, 3), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, player2Position.Y / 10000f + 0.001f + 0.001f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + player2Position + new Vector2(0f, -TileSize / 4), new Rectangle(224 + num3 * 16, 1712, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, player2Position.Y / 10000f + 0.002f + 0.001f);
					}
				}
				foreach (TemporaryAnimatedSprite temporarySprite in temporarySprites)
				{
					temporarySprite.draw(b, localPosition: true);
				}
				foreach (CowboyPowerup powerup in powerups)
				{
					powerup.draw(b);
				}
				foreach (CowboyBullet bullet in bullets)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(bullet.position.X, bullet.position.Y), new Rectangle(518, 1760 + (bulletDamage - 1) * 4, 4, 4), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.9f);
				}
				foreach (CowboyBullet enemyBullet in enemyBullets)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(enemyBullet.position.X, enemyBullet.position.Y), new Rectangle(523, 1760, 5, 5), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.9f);
				}
				if (shopping)
				{
					if ((merchantArriving || merchantLeaving) && !merchantShopOpen)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(merchantBox.Location.X, merchantBox.Location.Y), new Rectangle(464 + ((shoppingTimer / 100 % 2 == 0) ? 16 : 0), 1728, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)merchantBox.Y / 10000f + 0.001f);
					}
					else
					{
						int num4 = ((playerBoundingBox.X - merchantBox.X > TileSize) ? 2 : ((merchantBox.X - playerBoundingBox.X > TileSize) ? 1 : 0));
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(merchantBox.Location.X, merchantBox.Location.Y), new Rectangle(496 + num4 * 16, 1728, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)merchantBox.Y / 10000f + 0.001f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(merchantBox.Location.X - TileSize, merchantBox.Location.Y + TileSize), new Rectangle(529, 1744, 63, 32), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)merchantBox.Y / 10000f + 0.001f);
						foreach (KeyValuePair<Rectangle, int> storeItem in storeItems)
						{
							b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(storeItem.Key.Location.X, storeItem.Key.Location.Y), new Rectangle(320 + storeItem.Value * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)storeItem.Key.Location.Y / 10000f);
							b.DrawString(Game1.smallFont, getPriceForItem(storeItem.Value).ToString() ?? "", topLeftScreenCoordinate + new Vector2((float)(storeItem.Key.Location.X + TileSize / 2) - Game1.smallFont.MeasureString(getPriceForItem(storeItem.Value).ToString() ?? "").X / 2f, storeItem.Key.Location.Y + TileSize + 3), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)storeItem.Key.Location.Y / 10000f + 0.002f);
							b.DrawString(Game1.smallFont, getPriceForItem(storeItem.Value).ToString() ?? "", topLeftScreenCoordinate + new Vector2((float)(storeItem.Key.Location.X + TileSize / 2) - Game1.smallFont.MeasureString(getPriceForItem(storeItem.Value).ToString() ?? "").X / 2f - 1f, storeItem.Key.Location.Y + TileSize + 3), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)storeItem.Key.Location.Y / 10000f + 0.002f);
							b.DrawString(Game1.smallFont, getPriceForItem(storeItem.Value).ToString() ?? "", topLeftScreenCoordinate + new Vector2((float)(storeItem.Key.Location.X + TileSize / 2) - Game1.smallFont.MeasureString(getPriceForItem(storeItem.Value).ToString() ?? "").X / 2f + 1f, storeItem.Key.Location.Y + TileSize + 3), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)storeItem.Key.Location.Y / 10000f + 0.002f);
						}
					}
				}
				if (waitingForPlayerToMoveDownAMap && (merchantShopOpen || merchantLeaving || !shopping) && shoppingTimer < 250)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(8.5f, 15f) * TileSize + new Vector2(-12f, 0f), new Rectangle(355, 1750, 8, 8), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.001f);
				}
				foreach (CowboyMonster monster in monsters)
				{
					monster.draw(b);
				}
				if (gopherRunning)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(gopherBox.X, gopherBox.Y), new Rectangle(320 + waveTimer / 100 % 4 * 16, 1792, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)gopherBox.Y / 10000f + 0.001f);
				}
				if (gopherTrain && gopherTrainPosition > -TileSize)
				{
					b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y, 16 * TileSize, 16 * TileSize), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.95f);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(playerPosition.X - (float)(TileSize / 2), gopherTrainPosition), new Rectangle(384 + gopherTrainPosition / 30 % 4 * 16, 1792, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.96f);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(playerPosition.X + (float)(TileSize / 2), gopherTrainPosition), new Rectangle(384 + gopherTrainPosition / 30 % 4 * 16, 1792, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.96f);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(playerPosition.X, gopherTrainPosition - TileSize * 3), new Rectangle(320 + gopherTrainPosition / 30 % 4 * 16, 1792, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.96f);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(playerPosition.X - (float)(TileSize / 2), gopherTrainPosition - TileSize), new Rectangle(400, 1728, 32, 32), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.97f);
					if (holdItemTimer > 0)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize / 4), new Rectangle(384, 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.98f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize * 2 / 3) + new Vector2(0f, -TileSize / 4), new Rectangle(320 + itemToHold * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.99f);
					}
					else
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize / 4), new Rectangle(464, 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.98f);
					}
				}
				else
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate - new Vector2(TileSize + 27, 0f), new Rectangle(294, 1782, 22, 22), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.25f);
					if (heldItem != null)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate - new Vector2(TileSize + 18, -9f), new Rectangle(272 + heldItem.which * 16, 1808, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
					Vector2 position = topLeftScreenCoordinate - new Vector2(TileSize + 27, 0f);
					b.Draw(Game1.mouseCursors, position, new Rectangle(294, 1782, 22, 22), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.25f);
					buttonPowerup.bounds = new Rectangle((int)position.X - 32, (int)position.Y - 32, 130, 130);
					upperRightCloseButton.bounds.X = Game1.viewport.Width - 68 - Game1.xEdge;
					upperRightCloseButton.draw(b);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate - new Vector2(TileSize * 2, -TileSize - 18), new Rectangle(400, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					b.DrawString(Game1.smallFont, "x" + Math.Max(lives, 0), topLeftScreenCoordinate - new Vector2(TileSize, -TileSize - TileSize / 4 - 18), Color.White);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate - new Vector2(TileSize * 2, -TileSize * 2 - 18), new Rectangle(272, 1808, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					b.DrawString(Game1.smallFont, "x" + coins, topLeftScreenCoordinate - new Vector2(TileSize, -TileSize * 2 - TileSize / 4 - 18), Color.White);
					for (int num5 = 0; num5 < whichWave + whichRound * 12; num5++)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(TileSize * 16 + 3, num5 * 3 * 6), new Rectangle(512, 1760, 5, 5), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
					b.Draw(Game1.mouseCursors, new Vector2((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y - TileSize / 2 - 12), new Rectangle(595, 1748, 9, 11), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					if (!shootoutLevel)
					{
						b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X + 30, (int)topLeftScreenCoordinate.Y - TileSize / 2 + 3, (int)((float)(16 * TileSize - 30) * ((float)waveTimer / 80000f)), TileSize / 4), (waveTimer < 8000) ? new Color(188, 51, 74) : new Color(147, 177, 38));
					}
					if (betweenWaveTimer > 0 && whichWave == 0 && !scrollingMap)
					{
						Vector2 vector3 = new Vector2(Game1.viewport.Width / 2 - 120, Game1.viewport.Height - 144 - 3);
					}
					if (bulletDamage > 1)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(-TileSize - 3, 16 * TileSize - TileSize), new Rectangle(416 + (ammoLevel - 1) * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
					if (fireSpeedLevel > 0)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(-TileSize - 3, 16 * TileSize - TileSize * 2), new Rectangle(320 + (fireSpeedLevel - 1) * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
					if (runSpeedLevel > 0)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(-TileSize - 3, 16 * TileSize - TileSize * 3), new Rectangle(368 + (runSpeedLevel - 1) * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
					if (spreadPistol)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(-TileSize - 3, 16 * TileSize - TileSize * 4), new Rectangle(464, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
				}
				drawJoysticks(b);
				if (screenFlash > 0)
				{
					b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y, 16 * TileSize, 16 * TileSize), Game1.staminaRect.Bounds, new Color(255, 214, 168), 0f, Vector2.Zero, SpriteEffects.None, 1f);
				}
			}
			if (fadethenQuitTimer > 0)
			{
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Game1.staminaRect.Bounds, Color.Black * (1f - (float)fadethenQuitTimer / 2000f), 0f, Vector2.Zero, SpriteEffects.None, 1f);
			}
			if (abigailPortraitTimer > 0)
			{
				b.Draw(Game1.getCharacterFromName("Abigail").Portrait, new Vector2(topLeftScreenCoordinate.X + (float)(16 * TileSize), abigailPortraitYposition), new Rectangle(64 * (abigailPortraitExpression % 2), 64 * (abigailPortraitExpression / 2), 64, 64), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				if (abigailPortraitTimer < 5500 && abigailPortraitTimer > 500)
				{
					int widthOfString = SpriteText.getWidthOfString("0" + AbigailDialogue + "0");
					int num6 = (int)(topLeftScreenCoordinate.X + (float)(16 * TileSize) + 128f - (float)(widthOfString / 2));
					SpriteText.drawString(x: (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.zh && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ru) ? ((int)(topLeftScreenCoordinate.X + (float)(16 * TileSize))) : ((int)(topLeftScreenCoordinate.X + (float)(16 * TileSize)) + widthOfString / 4), b: b, s: AbigailDialogue, y: (int)((double)abigailPortraitYposition - 80.0), characterPosition: 999999, width: widthOfString, height: 999999, alpha: 1f, layerDepth: 0.88f, junimoText: false, drawBGScroll: -1, placeHolderScrollWidthText: "", color: 3);
				}
			}
			b.End();
			b.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (Game1.IsMultiplayer)
			{
				string timeOfDayString = Game1.getTimeOfDayString(Game1.timeOfDay);
				Vector2 vector4 = new Vector2((float)Game1.viewport.Width - Game1.dialogueFont.MeasureString(timeOfDayString).X - 16f, 16f);
				Color white = Color.White;
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), vector4, white, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.01f);
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), vector4 + new Vector2(1f, 1f) + new Vector2(-3f, -3f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), vector4 + new Vector2(1f, 1f) + new Vector2(-2f, -2f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), vector4 + new Vector2(1f, 1f) + new Vector2(-1f, -1f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), vector4 + new Vector2(1f, 1f) + new Vector2(-3.5f, -3.5f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), vector4 + new Vector2(1f, 1f) + new Vector2(-1.5f, -1.5f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), vector4 + new Vector2(1f, 1f) + new Vector2(-2.5f, -2.5f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.02f);
			}
			b.End();
		}

		public void changeScreenSize()
		{
			topLeftScreenCoordinate = new Vector2(Game1.viewport.Width / 2 - 384, Game1.viewport.Height / 2 - 384);
		}

		public void unload()
		{
			if (overworldSong != null && overworldSong.IsPlaying)
			{
				overworldSong.Stop(AudioStopOptions.Immediate);
			}
			if (outlawSong != null && outlawSong.IsPlaying)
			{
				outlawSong.Stop(AudioStopOptions.Immediate);
			}
			lives = 3;
			Game1.stopMusicTrack(Game1.MusicContext.MiniGame);
		}

		public void receiveEventPoke(int data)
		{
		}

		public string minigameId()
		{
			return "PrairieKing";
		}

		public bool doMainGameUpdates()
		{
			return false;
		}

		public bool forceQuit()
		{
			if (playingWithAbigail)
			{
				return false;
			}
			unload();
			return true;
		}

		private void drawJoysticks(SpriteBatch b)
		{
			leftJoystick.draw(b);
			rightJoystick.draw(b);
		}

		private void SetJoystickStartPositions()
		{
			int num = 148;
			int num2 = (_maxJoystickMoveDistance = (Game1.viewport.Width - 16 * TileSize) / 4 - num / 2);
			_joystickLeftStartPosition = new Vector2(num2, Game1.viewport.Height - num2 - num);
			_joystickRightStartPosition = new Vector2(Game1.viewport.Width - num2 - num, Game1.viewport.Height - num2 - num);
		}

		private void UpdateMobile()
		{
			SetJoystickStartPositions();
			try
			{
				TouchCollection getTouchState = Game1.input.GetTouchState;
				for (int i = 0; i < getTouchState.Count; i++)
				{
					if (getTouchState[i].State == TouchLocationState.Pressed || getTouchState[i].State == TouchLocationState.Moved)
					{
						OnTap((int)getTouchState[i].Position.X, (int)getTouchState[i].Position.Y);
					}
				}
			}
			catch (Exception)
			{
			}
			if (_leftJoystickHeld)
			{
				if (!_lastLeftJoystickHeld)
				{
					_joystickLeftLastTapPoint = _joystickLeftTapPoint;
					_joystickLeftTapStartPoint = _joystickLeftTapPoint;
				}
				OnTapHeldJoystickLeft((int)_joystickLeftTapPoint.X, (int)_joystickLeftTapPoint.Y);
			}
			else
			{
				if (leftJoystick.bounds.X != (int)_joystickLeftStartPosition.X)
				{
					leftJoystick.bounds.X += (int)((_joystickLeftStartPosition.X - (float)leftJoystick.bounds.X) / 2f);
					if ((float)Math.Abs(leftJoystick.bounds.X) - Math.Abs(_joystickLeftStartPosition.X) < 2f)
					{
						leftJoystick.bounds.X = (int)_joystickLeftStartPosition.X;
					}
				}
				if (leftJoystick.bounds.Y != (int)_joystickLeftStartPosition.Y)
				{
					leftJoystick.bounds.Y += (int)((_joystickLeftStartPosition.Y - (float)leftJoystick.bounds.Y) / 2f);
					if ((float)Math.Abs(leftJoystick.bounds.Y) - Math.Abs(_joystickLeftStartPosition.Y) < 2f)
					{
						leftJoystick.bounds.Y = (int)_joystickLeftStartPosition.Y;
					}
				}
			}
			if (_rightJoystickHeld)
			{
				if (!_lastRightJoystickHeld)
				{
					_joystickRightTapStartPoint = _joystickRightTapPoint;
					_joystickRightLastTapPoint = _joystickRightTapPoint;
				}
				OnTapHeldJoystickRight((int)_joystickRightTapPoint.X, (int)_joystickRightTapPoint.Y);
			}
			else
			{
				if (rightJoystick.bounds.X != (int)_joystickRightStartPosition.X)
				{
					rightJoystick.bounds.X += (int)((_joystickRightStartPosition.X - (float)rightJoystick.bounds.X) / 2f);
					if ((float)Math.Abs(rightJoystick.bounds.X) - Math.Abs(_joystickRightStartPosition.X) < 2f)
					{
						rightJoystick.bounds.X = (int)_joystickRightStartPosition.X;
					}
				}
				if (rightJoystick.bounds.Y != (int)_joystickRightStartPosition.Y)
				{
					rightJoystick.bounds.Y += (int)((_joystickRightStartPosition.Y - (float)rightJoystick.bounds.Y) / 2f);
					if ((float)Math.Abs(rightJoystick.bounds.Y) - Math.Abs(_joystickRightStartPosition.Y) < 2f)
					{
						rightJoystick.bounds.Y = (int)_joystickRightStartPosition.Y;
					}
				}
			}
			_lastLeftJoystickHeld = _leftJoystickHeld;
			_lastRightJoystickHeld = _rightJoystickHeld;
			_leftJoystickHeld = false;
			_rightJoystickHeld = false;
		}

		public void OnTap(int x, int y, bool playSound = true)
		{
			OnTapLeftJoystick(x, y);
			OnTapRightJoystick(x, y);
		}

		private void OnTapLeftJoystick(int x, int y)
		{
			if (x < Game1.viewport.Width / 2)
			{
				_joystickLeftTapPoint.X = x;
				_joystickLeftTapPoint.Y = y;
				_leftJoystickHeld = true;
			}
		}

		private void OnTapRightJoystick(int x, int y)
		{
			if (x > Game1.viewport.Width / 2)
			{
				_joystickRightTapPoint.X = x;
				_joystickRightTapPoint.Y = y;
				_rightJoystickHeld = true;
			}
		}

		private void OnTapHeldJoystickLeft(int x, int y)
		{
			float num = Vector2.Distance(_joystickLeftTapStartPoint, new Vector2(x, y));
			float num2 = Vector2.Distance(_joystickLeftLastTapPoint, new Vector2(x, y));
			if (num2 > 5f && num <= 350f)
			{
				float num3 = (float)Math.Atan2(_joystickLeftTapStartPoint.Y - (float)y, _joystickLeftTapStartPoint.X - (float)x);
				num = Math.Min(num, _maxJoystickMoveDistance);
				leftJoystick.bounds.X = (int)_joystickLeftStartPosition.X - (int)(Math.Cos(num3) * (double)num);
				leftJoystick.bounds.Y = (int)_joystickLeftStartPosition.Y - (int)(Math.Sin(num3) * (double)num);
				num3 = (float)Math.Atan2(_joystickLeftStartPosition.Y - (float)leftJoystick.bounds.Y, _joystickLeftStartPosition.X - (float)leftJoystick.bounds.X);
				float angleDegrees = num3 / ((float)Math.PI * 2f) * 360f;
				WalkDirection walkDirection = TapToMoveUtils.WalkDirectionForAngle(angleDegrees);
				if (walkDirection == WalkDirection.Up || walkDirection == WalkDirection.UpLeft || walkDirection == WalkDirection.UpRight)
				{
					_buttonHeldState.Add(GameKeys.MoveDown);
				}
				if (walkDirection == WalkDirection.Down || walkDirection == WalkDirection.DownLeft || walkDirection == WalkDirection.DownRight)
				{
					_buttonHeldState.Add(GameKeys.MoveUp);
				}
				if (walkDirection == WalkDirection.Left || walkDirection == WalkDirection.UpLeft || walkDirection == WalkDirection.DownLeft)
				{
					_buttonHeldState.Add(GameKeys.MoveRight);
				}
				if (walkDirection == WalkDirection.Right || walkDirection == WalkDirection.UpRight || walkDirection == WalkDirection.DownRight)
				{
					_buttonHeldState.Add(GameKeys.MoveLeft);
				}
			}
		}

		private void OnTapHeldJoystickRight(int x, int y)
		{
			float num = Vector2.Distance(_joystickRightTapStartPoint, new Vector2(x, y));
			float num2 = Vector2.Distance(_joystickRightLastTapPoint, new Vector2(x, y));
			if (num2 > 5f && num <= 350f)
			{
				float num3 = (float)Math.Atan2(_joystickRightTapStartPoint.Y - (float)y, _joystickRightTapStartPoint.X - (float)x);
				num = Math.Min(num, _maxJoystickMoveDistance);
				rightJoystick.bounds.X = (int)_joystickRightStartPosition.X - (int)(Math.Cos(num3) * (double)num);
				rightJoystick.bounds.Y = (int)_joystickRightStartPosition.Y - (int)(Math.Sin(num3) * (double)num);
				num3 = (float)Math.Atan2(_joystickRightStartPosition.Y - (float)rightJoystick.bounds.Y, _joystickRightStartPosition.X - (float)rightJoystick.bounds.X);
				float angleDegrees = num3 / ((float)Math.PI * 2f) * 360f;
				WalkDirection walkDirection = TapToMoveUtils.WalkDirectionForAngle(angleDegrees);
				if (walkDirection == WalkDirection.Up || walkDirection == WalkDirection.UpLeft || walkDirection == WalkDirection.UpRight)
				{
					_buttonHeldState.Add(GameKeys.ShootDown);
				}
				if (walkDirection == WalkDirection.Down || walkDirection == WalkDirection.DownLeft || walkDirection == WalkDirection.DownRight)
				{
					_buttonHeldState.Add(GameKeys.ShootUp);
				}
				if (walkDirection == WalkDirection.Left || walkDirection == WalkDirection.UpLeft || walkDirection == WalkDirection.DownLeft)
				{
					_buttonHeldState.Add(GameKeys.ShootRight);
				}
				if (walkDirection == WalkDirection.Right || walkDirection == WalkDirection.UpRight || walkDirection == WalkDirection.DownRight)
				{
					_buttonHeldState.Add(GameKeys.ShootLeft);
				}
			}
		}

		protected virtual void _InitializeMobileControls()
		{
			SetJoystickStartPositions();
			int num = 148;
			leftJoystick = new ClickableTextureComponent(new Rectangle((int)_joystickLeftStartPosition.X, (int)_joystickLeftStartPosition.Y, num, num), Game1.mobileSpriteSheet, new Rectangle(0, 116, 37, 37), 4f);
			rightJoystick = new ClickableTextureComponent(new Rectangle((int)_joystickRightStartPosition.X, (int)_joystickRightStartPosition.Y, num, num), Game1.mobileSpriteSheet, new Rectangle(0, 116, 37, 37), 4f);
			buttonRestart = new ClickableComponent(new Rectangle(0, 0, 1, 1), "Restart");
			buttonQuit = new ClickableComponent(new Rectangle(0, 0, 1, 1), "Quit");
			buttonPowerup = new ClickableComponent(new Rectangle(0, 0, 100, 100), "Powerup");
			upperRightCloseButton = new ClickableTextureComponent(new Rectangle(Game1.viewport.Width - 68 - Game1.xEdge, 0, 68 + Game1.xEdge, 80), Game1.mobileSpriteSheet, new Rectangle(62, 0, 17, 17), 4f, drawShadow: true);
		}

		public float GetForcedScaleFactor()
		{
			return (float)Game1.uiViewport.Height / ((float)TileSize * 16f);
		}
	}
}
