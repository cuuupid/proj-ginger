using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Locations;
using xTile.Dimensions;

namespace StardewValley.Monsters
{
	public class Monster : NPC
	{
		protected delegate void collisionBehavior(GameLocation location);

		public const int defaultInvincibleCountdown = 450;

		public const int seekPlayerIterationLimit = 80;

		[XmlElement("damageToFarmer")]
		public readonly NetInt damageToFarmer = new NetInt();

		[XmlElement("health")]
		public readonly NetInt health = new NetInt();

		[XmlElement("maxHealth")]
		public readonly NetInt maxHealth = new NetInt();

		[XmlElement("coinsToDrop")]
		public readonly NetInt coinsToDrop = new NetInt();

		[XmlElement("durationOfRandomMovements")]
		public readonly NetInt durationOfRandomMovements = new NetInt();

		[XmlElement("resilience")]
		public readonly NetInt resilience = new NetInt();

		[XmlElement("slipperiness")]
		public readonly NetInt slipperiness = new NetInt(2);

		[XmlElement("experienceGained")]
		public readonly NetInt experienceGained = new NetInt();

		[XmlElement("jitteriness")]
		public readonly NetDouble jitteriness = new NetDouble();

		[XmlElement("missChance")]
		public readonly NetDouble missChance = new NetDouble();

		[XmlElement("isGlider")]
		public readonly NetBool isGlider = new NetBool();

		[XmlElement("mineMonster")]
		public readonly NetBool mineMonster = new NetBool();

		[XmlElement("hasSpecialItem")]
		public readonly NetBool hasSpecialItem = new NetBool();

		[XmlIgnore]
		public readonly NetFloat synchedRotation = new NetFloat().Interpolated(interpolate: true, wait: true);

		public readonly NetIntList objectsToDrop = new NetIntList();

		protected int skipHorizontal;

		protected int invincibleCountdown;

		[XmlIgnore]
		private bool skipHorizontalUp;

		protected readonly NetInt defaultAnimationInterval = new NetInt(175);

		public int stunTime;

		[XmlElement("initializedForLocation")]
		public bool initializedForLocation;

		[XmlIgnore]
		public readonly NetBool netFocusedOnFarmers = new NetBool();

		[XmlIgnore]
		public readonly NetBool netWildernessFarmMonster = new NetBool();

		private readonly NetEvent1<ParryEventArgs> parryEvent = new NetEvent1<ParryEventArgs>();

		private readonly NetEvent1Field<Vector2, NetVector2> trajectoryEvent = new NetEvent1Field<Vector2, NetVector2>();

		[XmlIgnore]
		private readonly NetEvent0 deathAnimEvent = new NetEvent0();

		[XmlElement("ignoreDamageLOS")]
		public readonly NetBool ignoreDamageLOS = new NetBool();

		protected collisionBehavior onCollision;

		[XmlElement("isHardModeMonster")]
		public NetBool isHardModeMonster = new NetBool(value: false);

		private int slideAnimationTimer;

		[XmlIgnore]
		public Farmer Player => findPlayer();

		[XmlIgnore]
		public int DamageToFarmer
		{
			get
			{
				return damageToFarmer;
			}
			set
			{
				damageToFarmer.Value = value;
			}
		}

		[XmlIgnore]
		public int Health
		{
			get
			{
				return health;
			}
			set
			{
				health.Value = value;
			}
		}

		[XmlIgnore]
		public int MaxHealth
		{
			get
			{
				return maxHealth;
			}
			set
			{
				maxHealth.Value = value;
			}
		}

		[XmlIgnore]
		public int ExperienceGained
		{
			get
			{
				return experienceGained;
			}
			set
			{
				experienceGained.Value = value;
			}
		}

		[XmlIgnore]
		public int Slipperiness
		{
			get
			{
				return slipperiness;
			}
			set
			{
				slipperiness.Value = value;
			}
		}

		[XmlIgnore]
		public bool focusedOnFarmers
		{
			get
			{
				return netFocusedOnFarmers;
			}
			set
			{
				netFocusedOnFarmers.Value = value;
			}
		}

		[XmlIgnore]
		public bool wildernessFarmMonster
		{
			get
			{
				return netWildernessFarmMonster;
			}
			set
			{
				netWildernessFarmMonster.Value = value;
			}
		}

		public override bool IsMonster => true;

		public Monster()
		{
		}

		public Monster(string name, Vector2 position)
			: this(name, position, 2)
		{
			base.Breather = false;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(damageToFarmer, health, maxHealth, coinsToDrop, durationOfRandomMovements, resilience, slipperiness, experienceGained, jitteriness, missChance, isGlider, mineMonster, hasSpecialItem, objectsToDrop, defaultAnimationInterval, netFocusedOnFarmers, netWildernessFarmMonster, deathAnimEvent, parryEvent, trajectoryEvent, ignoreDamageLOS, synchedRotation, isHardModeMonster);
			position.Field.AxisAlignedMovement = false;
			parryEvent.onEvent += handleParried;
			parryEvent.InterpolationWait = false;
			deathAnimEvent.onEvent += localDeathAnimation;
			trajectoryEvent.onEvent += doSetTrajectory;
			trajectoryEvent.InterpolationWait = false;
		}

		protected override Farmer findPlayer()
		{
			if (base.currentLocation == null)
			{
				return Game1.player;
			}
			Farmer result = Game1.player;
			double num = 1.7976931348623157E+308;
			foreach (Farmer farmer in base.currentLocation.farmers)
			{
				if (!farmer.hidden)
				{
					double num2 = findPlayerPriority(farmer);
					if (num2 < num)
					{
						num = num2;
						result = farmer;
					}
				}
			}
			return result;
		}

		protected virtual double findPlayerPriority(Farmer f)
		{
			return (f.Position - base.Position).LengthSquared();
		}

		public virtual void onDealContactDamage(Farmer who)
		{
		}

		public virtual List<Item> getExtraDropItems()
		{
			return new List<Item>();
		}

		public override bool withinPlayerThreshold()
		{
			if (!focusedOnFarmers)
			{
				return withinPlayerThreshold(moveTowardPlayerThreshold);
			}
			return true;
		}

		public Monster(string name, Vector2 position, int facingDir)
			: base(new AnimatedSprite("Characters\\Monsters\\" + name), position, facingDir, name)
		{
			parseMonsterInfo(name);
			base.Breather = false;
		}

		public virtual void drawAboveAllLayers(SpriteBatch b)
		{
		}

		public override void draw(SpriteBatch b)
		{
			if (!isGlider)
			{
				base.draw(b);
			}
		}

		public virtual bool isInvincible()
		{
			return invincibleCountdown > 0;
		}

		public void setInvincibleCountdown(int time)
		{
			invincibleCountdown = time;
			startGlowing(new Color(255, 0, 0), border: false, 0.25f);
			glowingTransparency = 1f;
		}

		protected int maxTimesReachedMineBottom()
		{
			int num = 0;
			foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
			{
				num = Math.Max(num, onlineFarmer.timesReachedMineBottom);
			}
			return num;
		}

		public virtual Debris ModifyMonsterLoot(Debris debris)
		{
			return debris;
		}

		public virtual int GetBaseDifficultyLevel()
		{
			return 0;
		}

		public virtual void BuffForAdditionalDifficulty(int additional_difficulty)
		{
			int num = 0;
			if (DamageToFarmer != 0)
			{
				DamageToFarmer = (int)((float)DamageToFarmer * (1f + (float)additional_difficulty * 0.25f));
				num = 20 + (additional_difficulty - 1) * 20;
				if (DamageToFarmer < num)
				{
					DamageToFarmer = (int)Utility.Lerp(DamageToFarmer, num, 0.5f);
				}
			}
			MaxHealth = (int)((float)MaxHealth * (1f + (float)additional_difficulty * 0.5f));
			num = 500 + (additional_difficulty - 1) * 300;
			if (MaxHealth < num)
			{
				MaxHealth = (int)Utility.Lerp(MaxHealth, num, 0.5f);
			}
			Health = MaxHealth;
			resilience.Value += additional_difficulty * resilience.Value;
			isHardModeMonster.Value = true;
		}

		protected void parseMonsterInfo(string name)
		{
			string[] array = Game1.content.Load<Dictionary<string, string>>("Data\\Monsters")[name].Split('/');
			Health = Convert.ToInt32(array[0]);
			MaxHealth = Health;
			DamageToFarmer = Convert.ToInt32(array[1]);
			coinsToDrop.Value = Game1.random.Next(Convert.ToInt32(array[2]), Convert.ToInt32(array[3]) + 1);
			isGlider.Value = Convert.ToBoolean(array[4]);
			durationOfRandomMovements.Value = Convert.ToInt32(array[5]);
			string[] array2 = array[6].Split(' ');
			objectsToDrop.Clear();
			for (int i = 0; i < array2.Length; i += 2)
			{
				if (Game1.random.NextDouble() < Convert.ToDouble(array2[i + 1]))
				{
					objectsToDrop.Add(Convert.ToInt32(array2[i]));
				}
			}
			resilience.Value = Convert.ToInt32(array[7]);
			jitteriness.Value = Convert.ToDouble(array[8]);
			base.willDestroyObjectsUnderfoot = false;
			moveTowardPlayer(Convert.ToInt32(array[9]));
			base.speed = Convert.ToInt32(array[10]);
			missChance.Value = Convert.ToDouble(array[11]);
			mineMonster.Value = Convert.ToBoolean(array[12]);
			if (maxTimesReachedMineBottom() >= 1 && (bool)mineMonster)
			{
				resilience.Value += resilience.Value / 2;
				missChance.Value *= 2.0;
				Health += Game1.random.Next(0, Health);
				DamageToFarmer += Game1.random.Next(0, DamageToFarmer / 2);
				coinsToDrop.Value += Game1.random.Next(0, (int)coinsToDrop + 1);
			}
			try
			{
				ExperienceGained = Convert.ToInt32(array[13]);
			}
			catch (Exception)
			{
				ExperienceGained = 1;
			}
			if (LocalizedContentManager.CurrentLanguageCode != 0)
			{
				displayName = array[array.Length - 1];
			}
		}

		public virtual void InitializeForLocation(GameLocation location)
		{
			if (initializedForLocation)
			{
				return;
			}
			if ((bool)mineMonster && maxTimesReachedMineBottom() >= 1)
			{
				double num = 0.0;
				if (location is MineShaft)
				{
					MineShaft mineShaft = location as MineShaft;
					num = (double)mineShaft.GetAdditionalDifficulty() * 0.001;
				}
				if (Game1.random.NextDouble() < 0.001 + num)
				{
					objectsToDrop.Add((Game1.random.NextDouble() < 0.5) ? 72 : 74);
				}
			}
			if (Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS") && Game1.random.NextDouble() < ((name == "Dust Spirit") ? 0.02 : 0.05))
			{
				objectsToDrop.Add(890);
			}
			initializedForLocation = true;
		}

		public override void reloadSprite()
		{
			Sprite = new AnimatedSprite("Characters\\Monsters\\" + base.Name, 0, 16, 16);
		}

		public virtual void shedChunks(int number)
		{
			shedChunks(number, 0.75f);
		}

		public virtual void shedChunks(int number, float scale)
		{
			if (Sprite.Texture.Height > Sprite.getHeight() * 4)
			{
				Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Microsoft.Xna.Framework.Rectangle(0, Sprite.getHeight() * 4 + 16, 16, 16), 8, GetBoundingBox().Center.X, GetBoundingBox().Center.Y, number, (int)getTileLocation().Y, Color.White, 4f * scale);
			}
		}

		public void deathAnimation()
		{
			sharedDeathAnimation();
			deathAnimEvent.Fire();
		}

		protected virtual void sharedDeathAnimation()
		{
			shedChunks(Game1.random.Next(4, 9), 0.75f);
		}

		protected virtual void localDeathAnimation()
		{
		}

		public void parried(int damage, Farmer who)
		{
			parryEvent.Fire(new ParryEventArgs(damage, who));
		}

		private void handleParried(ParryEventArgs args)
		{
			int damage = args.damage;
			Farmer who = args.who;
			if (Game1.IsMasterGame)
			{
				float num = xVelocity;
				float num2 = yVelocity;
				if (xVelocity != 0f || yVelocity != 0f)
				{
					base.currentLocation.damageMonster(GetBoundingBox(), damage / 2, damage / 2 + 1, isBomb: false, 0f, 0, 0f, 0f, triggerMonsterInvincibleTimer: false, who);
				}
				xVelocity = 0f - num;
				yVelocity = 0f - num2;
				xVelocity *= (isGlider ? 2f : 3.5f);
				yVelocity *= (isGlider ? 2f : 3.5f);
			}
			setInvincibleCountdown(450);
		}

		public virtual int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			return takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, "hitEnemy");
		}

		public int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, string hitSound)
		{
			int num = Math.Max(1, damage - (int)resilience);
			slideAnimationTimer = 0;
			if (Game1.random.NextDouble() < (double)missChance - (double)missChance * addedPrecision)
			{
				num = -1;
			}
			else
			{
				Health -= num;
				base.currentLocation.playSound(hitSound);
				setTrajectory(xTrajectory / 3, yTrajectory / 3);
				if (Health <= 0)
				{
					deathAnimation();
				}
			}
			return num;
		}

		public override void setTrajectory(Vector2 trajectory)
		{
			trajectoryEvent.Fire(trajectory);
		}

		private void doSetTrajectory(Vector2 trajectory)
		{
			if (Game1.IsMasterGame)
			{
				if (Math.Abs(trajectory.X) > Math.Abs(xVelocity))
				{
					xVelocity = trajectory.X;
				}
				if (Math.Abs(trajectory.Y) > Math.Abs(yVelocity))
				{
					yVelocity = trajectory.Y;
				}
			}
		}

		public virtual void behaviorAtGameTick(GameTime time)
		{
			if (timeBeforeAIMovementAgain > 0f)
			{
				timeBeforeAIMovementAgain -= time.ElapsedGameTime.Milliseconds;
			}
			if (!Player.isRafting || !withinPlayerThreshold(4))
			{
				return;
			}
			if (Math.Abs(Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y) > 192)
			{
				if (Player.GetBoundingBox().Center.X - GetBoundingBox().Center.X > 0)
				{
					SetMovingLeft(b: true);
				}
				else
				{
					SetMovingRight(b: true);
				}
			}
			else if (Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y > 0)
			{
				SetMovingUp(b: true);
			}
			else
			{
				SetMovingDown(b: true);
			}
			MovePosition(time, Game1.viewport, base.currentLocation);
		}

		public virtual bool passThroughCharacters()
		{
			return false;
		}

		public override bool shouldCollideWithBuildingLayer(GameLocation location)
		{
			return true;
		}

		public override void update(GameTime time, GameLocation location)
		{
			if (Game1.IsMasterGame && !initializedForLocation && location != null)
			{
				InitializeForLocation(location);
				initializedForLocation = true;
			}
			parryEvent.Poll();
			trajectoryEvent.Poll();
			deathAnimEvent.Poll();
			position.UpdateExtrapolation(base.speed + base.addedSpeed);
			if (invincibleCountdown > 0)
			{
				invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
				if (invincibleCountdown <= 0)
				{
					stopGlowing();
				}
			}
			if (!location.farmers.Any())
			{
				return;
			}
			if (!Player.isRafting || !withinPlayerThreshold(4))
			{
				base.update(time, location);
			}
			if (Game1.IsMasterGame)
			{
				if (stunTime <= 0)
				{
					behaviorAtGameTick(time);
				}
				else
				{
					stunTime -= (int)time.ElapsedGameTime.TotalMilliseconds;
					if (stunTime < 0)
					{
						stunTime = 0;
					}
				}
			}
			updateAnimation(time);
			if (Game1.IsMasterGame)
			{
				synchedRotation.Value = rotation;
			}
			else
			{
				rotation = synchedRotation.Value;
			}
			if (controller != null && withinPlayerThreshold(3))
			{
				controller = null;
			}
			if (!isGlider && (base.Position.X < 0f || base.Position.X > (float)(location.Map.GetLayer("Back").LayerWidth * 64) || base.Position.Y < 0f || base.Position.Y > (float)(location.map.GetLayer("Back").LayerHeight * 64)))
			{
				location.characters.Remove(this);
			}
			else if ((bool)isGlider && base.Position.X < -2000f)
			{
				Health = -500;
			}
		}

		protected void resetAnimationSpeed()
		{
			if (!ignoreMovementAnimations)
			{
				Sprite.interval = (float)(int)defaultAnimationInterval - (float)(base.speed + base.addedSpeed - 2) * 20f;
			}
		}

		protected virtual void updateAnimation(GameTime time)
		{
			if (!Game1.IsMasterGame)
			{
				updateMonsterSlaveAnimation(time);
			}
			resetAnimationSpeed();
		}

		protected override void updateSlaveAnimation(GameTime time)
		{
		}

		protected virtual void updateMonsterSlaveAnimation(GameTime time)
		{
			Sprite.animateOnce(time);
		}

		private bool doHorizontalMovement(GameLocation location)
		{
			bool result = false;
			if (base.Position.X > Player.Position.X + 8f || (skipHorizontal > 0 && Player.getStandingX() < getStandingX() - 8))
			{
				SetMovingOnlyLeft();
				if (!location.isCollidingPosition(nextPosition(3), Game1.viewport, isFarmer: false, DamageToFarmer, isGlider, this))
				{
					MovePosition(Game1.currentGameTime, Game1.viewport, location);
					result = true;
				}
				else
				{
					faceDirection(3);
					if ((int)durationOfRandomMovements > 0 && Game1.random.NextDouble() < (double)jitteriness)
					{
						if (Game1.random.NextDouble() < 0.5)
						{
							tryToMoveInDirection(2, isFarmer: false, DamageToFarmer, isGlider);
						}
						else
						{
							tryToMoveInDirection(0, isFarmer: false, DamageToFarmer, isGlider);
						}
						timeBeforeAIMovementAgain = (int)durationOfRandomMovements;
					}
				}
			}
			else if (base.Position.X < Player.Position.X - 8f)
			{
				SetMovingOnlyRight();
				if (!location.isCollidingPosition(nextPosition(1), Game1.viewport, isFarmer: false, DamageToFarmer, isGlider, this))
				{
					MovePosition(Game1.currentGameTime, Game1.viewport, location);
					result = true;
				}
				else
				{
					faceDirection(1);
					if ((int)durationOfRandomMovements > 0 && Game1.random.NextDouble() < (double)jitteriness)
					{
						if (Game1.random.NextDouble() < 0.5)
						{
							tryToMoveInDirection(2, isFarmer: false, DamageToFarmer, isGlider);
						}
						else
						{
							tryToMoveInDirection(0, isFarmer: false, DamageToFarmer, isGlider);
						}
						timeBeforeAIMovementAgain = (int)durationOfRandomMovements;
					}
				}
			}
			else
			{
				faceGeneralDirection(Player.getStandingPosition());
				setMovingInFacingDirection();
				skipHorizontal = 500;
			}
			return result;
		}

		public virtual bool ShouldActuallyMoveAwayFromPlayer()
		{
			return false;
		}

		private void checkHorizontalMovement(ref bool success, ref bool setMoving, ref bool scootSuccess, Farmer who, GameLocation location)
		{
			if (who.Position.X > base.Position.X + 16f)
			{
				if (ShouldActuallyMoveAwayFromPlayer())
				{
					SetMovingOnlyLeft();
				}
				else
				{
					SetMovingOnlyRight();
				}
				setMoving = true;
				if (!location.isCollidingPosition(nextPosition(1), Game1.viewport, isFarmer: false, DamageToFarmer, isGlider, this))
				{
					success = true;
				}
				else
				{
					MovePosition(Game1.currentGameTime, Game1.viewport, location);
					if (!base.Position.Equals(lastPosition))
					{
						scootSuccess = true;
					}
				}
			}
			if (success || !(who.Position.X < base.Position.X - 16f))
			{
				return;
			}
			if (ShouldActuallyMoveAwayFromPlayer())
			{
				SetMovingOnlyRight();
			}
			else
			{
				SetMovingOnlyLeft();
			}
			setMoving = true;
			if (!location.isCollidingPosition(nextPosition(3), Game1.viewport, isFarmer: false, DamageToFarmer, isGlider, this))
			{
				success = true;
				return;
			}
			MovePosition(Game1.currentGameTime, Game1.viewport, location);
			if (!base.Position.Equals(lastPosition))
			{
				scootSuccess = true;
			}
		}

		private void checkVerticalMovement(ref bool success, ref bool setMoving, ref bool scootSuccess, Farmer who, GameLocation location)
		{
			if (!success && who.Position.Y < base.Position.Y - 16f)
			{
				if (ShouldActuallyMoveAwayFromPlayer())
				{
					SetMovingOnlyDown();
				}
				else
				{
					SetMovingOnlyUp();
				}
				setMoving = true;
				if (!location.isCollidingPosition(nextPosition(0), Game1.viewport, isFarmer: false, DamageToFarmer, isGlider, this))
				{
					success = true;
				}
				else
				{
					MovePosition(Game1.currentGameTime, Game1.viewport, location);
					if (!base.Position.Equals(lastPosition))
					{
						scootSuccess = true;
					}
				}
			}
			if (success || !(who.Position.Y > base.Position.Y + 16f))
			{
				return;
			}
			if (ShouldActuallyMoveAwayFromPlayer())
			{
				SetMovingOnlyUp();
			}
			else
			{
				SetMovingOnlyDown();
			}
			setMoving = true;
			if (!location.isCollidingPosition(nextPosition(2), Game1.viewport, isFarmer: false, DamageToFarmer, isGlider, this))
			{
				success = true;
				return;
			}
			MovePosition(Game1.currentGameTime, Game1.viewport, location);
			if (!base.Position.Equals(lastPosition))
			{
				scootSuccess = true;
			}
		}

		public override void updateMovement(GameLocation location, GameTime time)
		{
			if (base.IsWalkingTowardPlayer)
			{
				if (((int)moveTowardPlayerThreshold == -1 || withinPlayerThreshold()) && timeBeforeAIMovementAgain <= 0f && IsMonster && !isGlider && location.map.GetLayer("Back").Tiles[(int)Player.getTileLocation().X, (int)Player.getTileLocation().Y] != null && !location.map.GetLayer("Back").Tiles[(int)Player.getTileLocation().X, (int)Player.getTileLocation().Y].Properties.ContainsKey("NPCBarrier"))
				{
					if (skipHorizontal <= 0)
					{
						if (lastPosition.Equals(base.Position) && Game1.random.NextDouble() < 0.001)
						{
							switch (FacingDirection)
							{
							case 1:
							case 3:
								if (Game1.random.NextDouble() < 0.5)
								{
									SetMovingOnlyUp();
								}
								else
								{
									SetMovingOnlyDown();
								}
								break;
							case 0:
							case 2:
								if (Game1.random.NextDouble() < 0.5)
								{
									SetMovingOnlyRight();
								}
								else
								{
									SetMovingOnlyLeft();
								}
								break;
							}
							skipHorizontal = 700;
							return;
						}
						bool success = false;
						bool setMoving = false;
						bool scootSuccess = false;
						if (lastPosition.X == base.Position.X)
						{
							checkHorizontalMovement(ref success, ref setMoving, ref scootSuccess, Player, location);
							checkVerticalMovement(ref success, ref setMoving, ref scootSuccess, Player, location);
						}
						else
						{
							checkVerticalMovement(ref success, ref setMoving, ref scootSuccess, Player, location);
							checkHorizontalMovement(ref success, ref setMoving, ref scootSuccess, Player, location);
						}
						if (!success && !setMoving)
						{
							Halt();
							faceGeneralDirection(Player.getStandingPosition());
						}
						if (success)
						{
							skipHorizontal = 500;
						}
						if (scootSuccess)
						{
							return;
						}
					}
					else
					{
						skipHorizontal -= time.ElapsedGameTime.Milliseconds;
					}
				}
			}
			else
			{
				defaultMovementBehavior(time);
			}
			MovePosition(time, Game1.viewport, location);
			if (base.Position.Equals(lastPosition) && base.IsWalkingTowardPlayer && withinPlayerThreshold())
			{
				noMovementProgressNearPlayerBehavior();
			}
		}

		public virtual void noMovementProgressNearPlayerBehavior()
		{
			Halt();
			faceGeneralDirection(Player.getStandingPosition());
		}

		public virtual void defaultMovementBehavior(GameTime time)
		{
			if (Game1.random.NextDouble() < (double)jitteriness * 1.8 && skipHorizontal <= 0)
			{
				switch (Game1.random.Next(6))
				{
				case 0:
					SetMovingOnlyUp();
					break;
				case 1:
					SetMovingOnlyRight();
					break;
				case 2:
					SetMovingOnlyDown();
					break;
				case 3:
					SetMovingOnlyLeft();
					break;
				default:
					Halt();
					break;
				}
			}
		}

		public virtual bool TakesDamageFromHitbox(Microsoft.Xna.Framework.Rectangle area_of_effect)
		{
			return GetBoundingBox().Intersects(area_of_effect);
		}

		public virtual bool OverlapsFarmerForDamage(Farmer who)
		{
			return GetBoundingBox().Intersects(who.GetBoundingBox());
		}

		public override void Halt()
		{
			int num = base.speed;
			base.Halt();
			base.speed = num;
		}

		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			if (stunTime > 0)
			{
				return;
			}
			lastPosition = base.Position;
			if (xVelocity != 0f || yVelocity != 0f)
			{
				if (double.IsNaN(xVelocity) || double.IsNaN(yVelocity))
				{
					xVelocity = 0f;
					yVelocity = 0f;
				}
				else if (xVelocity < -1000f || xVelocity > 1000f || yVelocity < -1000f || yVelocity > 1000f)
				{
					xVelocity = 0f;
					yVelocity = 0f;
				}
				Microsoft.Xna.Framework.Rectangle boundingBox = GetBoundingBox();
				int x = boundingBox.X;
				int y = boundingBox.Y;
				int num = boundingBox.X + (int)xVelocity;
				int num2 = boundingBox.Y - (int)yVelocity;
				int num3 = 1;
				bool flag = false;
				bool flag2 = false;
				if (this is SquidKid)
				{
					flag2 = true;
				}
				if (!isGlider.Value || flag2)
				{
					if (boundingBox.Width > 0 && Math.Abs((int)xVelocity) > boundingBox.Width)
					{
						num3 = (int)Math.Max(num3, Math.Ceiling((float)Math.Abs((int)xVelocity) / (float)boundingBox.Width));
					}
					if (boundingBox.Height > 0 && Math.Abs((int)yVelocity) > boundingBox.Height)
					{
						num3 = (int)Math.Max(num3, Math.Ceiling((float)Math.Abs((int)yVelocity) / (float)boundingBox.Height));
					}
				}
				for (int i = 1; i <= num3; i++)
				{
					boundingBox.X = (int)Utility.Lerp(x, num, (float)i / (float)num3);
					boundingBox.Y = (int)Utility.Lerp(y, num2, (float)i / (float)num3);
					bool glider = isGlider;
					if (flag2)
					{
						glider = false;
					}
					if (currentLocation != null && currentLocation.isCollidingPosition(boundingBox, viewport, isFarmer: false, DamageToFarmer, glider, this))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					position.X += xVelocity;
					position.Y -= yVelocity;
					if (Slipperiness < 1000)
					{
						xVelocity -= xVelocity / (float)Slipperiness;
						yVelocity -= yVelocity / (float)Slipperiness;
						if (Math.Abs(xVelocity) <= 0.05f)
						{
							xVelocity = 0f;
						}
						if (Math.Abs(yVelocity) <= 0.05f)
						{
							yVelocity = 0f;
						}
					}
					if (!isGlider && invincibleCountdown > 0)
					{
						slideAnimationTimer -= time.ElapsedGameTime.Milliseconds;
						if (slideAnimationTimer < 0 && (Math.Abs(xVelocity) >= 3f || Math.Abs(yVelocity) >= 3f))
						{
							slideAnimationTimer = 100 - (int)(Math.Abs(xVelocity) * 2f + Math.Abs(yVelocity) * 2f);
							Game1.multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite(6, getStandingPosition() + new Vector2(-32f, -32f), Color.White * 0.75f, 8, Game1.random.NextDouble() < 0.5, 20f)
							{
								scale = 0.75f
							});
						}
					}
				}
				else if ((bool)isGlider || Slipperiness >= 8)
				{
					if ((bool)isGlider)
					{
						bool[] array = Utility.horizontalOrVerticalCollisionDirections(boundingBox, this);
						if (array[0])
						{
							xVelocity = 0f - xVelocity;
							position.X += Math.Sign(xVelocity);
							rotation += (float)(Math.PI + (double)Game1.random.Next(-10, 11) * Math.PI / 500.0);
						}
						if (array[1])
						{
							yVelocity = 0f - yVelocity;
							position.Y -= Math.Sign(yVelocity);
							rotation += (float)(Math.PI + (double)Game1.random.Next(-10, 11) * Math.PI / 500.0);
						}
					}
					if (Slipperiness < 1000)
					{
						xVelocity -= xVelocity / (float)Slipperiness / 4f;
						yVelocity -= yVelocity / (float)Slipperiness / 4f;
						if (Math.Abs(xVelocity) <= 0.05f)
						{
							xVelocity = 0f;
						}
						if (Math.Abs(yVelocity) <= 0.051f)
						{
							yVelocity = 0f;
						}
					}
				}
				else
				{
					xVelocity -= xVelocity / (float)Slipperiness;
					yVelocity -= yVelocity / (float)Slipperiness;
					if (Math.Abs(xVelocity) <= 0.05f)
					{
						xVelocity = 0f;
					}
					if (Math.Abs(yVelocity) <= 0.05f)
					{
						yVelocity = 0f;
					}
				}
				if ((bool)isGlider)
				{
					return;
				}
			}
			if (moveUp)
			{
				if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(nextPosition(0), viewport, isFarmer: false, DamageToFarmer, isGlider, this)) || isCharging)
				{
					position.Y -= base.speed + base.addedSpeed;
					if (!ignoreMovementAnimations)
					{
						Sprite.AnimateUp(time);
					}
					FacingDirection = 0;
					faceDirection(0);
				}
				else
				{
					Microsoft.Xna.Framework.Rectangle rectangle = nextPosition(0);
					rectangle.Width /= 4;
					bool flag3 = currentLocation.isCollidingPosition(rectangle, viewport, isFarmer: false, DamageToFarmer, isGlider, this);
					rectangle.X += rectangle.Width * 3;
					bool flag4 = currentLocation.isCollidingPosition(rectangle, viewport, isFarmer: false, DamageToFarmer, isGlider, this);
					if (flag3 && !flag4 && !currentLocation.isCollidingPosition(nextPosition(1), viewport, isFarmer: false, DamageToFarmer, isGlider, this))
					{
						position.X += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					else if (flag4 && !flag3 && !currentLocation.isCollidingPosition(nextPosition(3), viewport, isFarmer: false, DamageToFarmer, isGlider, this))
					{
						position.X -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					if (!currentLocation.isTilePassable(nextPosition(0), viewport) || !base.willDestroyObjectsUnderfoot)
					{
						Halt();
					}
					else if (base.willDestroyObjectsUnderfoot)
					{
						Vector2 vector = new Vector2(getStandingX() / 64, getStandingY() / 64 - 1);
						if (currentLocation.characterDestroyObjectWithinRectangle(nextPosition(0), showDestroyedObject: true))
						{
							currentLocation.playSound("stoneCrack");
							position.Y -= base.speed + base.addedSpeed;
						}
						else
						{
							blockedInterval += time.ElapsedGameTime.Milliseconds;
						}
					}
					if (onCollision != null)
					{
						onCollision(currentLocation);
					}
				}
			}
			else if (moveRight)
			{
				if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(nextPosition(1), viewport, isFarmer: false, DamageToFarmer, isGlider, this)) || isCharging)
				{
					position.X += base.speed + base.addedSpeed;
					if (!ignoreMovementAnimations)
					{
						Sprite.AnimateRight(time);
					}
					FacingDirection = 1;
					faceDirection(1);
				}
				else
				{
					Microsoft.Xna.Framework.Rectangle rectangle2 = nextPosition(1);
					rectangle2.Height /= 4;
					bool flag5 = currentLocation.isCollidingPosition(rectangle2, viewport, isFarmer: false, DamageToFarmer, isGlider, this);
					rectangle2.Y += rectangle2.Height * 3;
					bool flag6 = currentLocation.isCollidingPosition(rectangle2, viewport, isFarmer: false, DamageToFarmer, isGlider, this);
					if (flag5 && !flag6 && !currentLocation.isCollidingPosition(nextPosition(2), viewport, isFarmer: false, DamageToFarmer, isGlider, this))
					{
						position.Y += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					else if (flag6 && !flag5 && !currentLocation.isCollidingPosition(nextPosition(0), viewport, isFarmer: false, DamageToFarmer, isGlider, this))
					{
						position.Y -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					if (!currentLocation.isTilePassable(nextPosition(1), viewport) || !base.willDestroyObjectsUnderfoot)
					{
						Halt();
					}
					else if (base.willDestroyObjectsUnderfoot)
					{
						Vector2 vector2 = new Vector2(getStandingX() / 64 + 1, getStandingY() / 64);
						if (currentLocation.characterDestroyObjectWithinRectangle(nextPosition(1), showDestroyedObject: true))
						{
							currentLocation.playSound("stoneCrack");
							position.X += base.speed + base.addedSpeed;
						}
						else
						{
							blockedInterval += time.ElapsedGameTime.Milliseconds;
						}
					}
					if (onCollision != null)
					{
						onCollision(currentLocation);
					}
				}
			}
			else if (moveDown)
			{
				if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(nextPosition(2), viewport, isFarmer: false, DamageToFarmer, isGlider, this)) || isCharging)
				{
					position.Y += base.speed + base.addedSpeed;
					if (!ignoreMovementAnimations)
					{
						Sprite.AnimateDown(time);
					}
					FacingDirection = 2;
					faceDirection(2);
				}
				else
				{
					Microsoft.Xna.Framework.Rectangle rectangle3 = nextPosition(2);
					rectangle3.Width /= 4;
					bool flag7 = currentLocation.isCollidingPosition(rectangle3, viewport, isFarmer: false, DamageToFarmer, isGlider, this);
					rectangle3.X += rectangle3.Width * 3;
					bool flag8 = currentLocation.isCollidingPosition(rectangle3, viewport, isFarmer: false, DamageToFarmer, isGlider, this);
					if (flag7 && !flag8 && !currentLocation.isCollidingPosition(nextPosition(1), viewport, isFarmer: false, DamageToFarmer, isGlider, this))
					{
						position.X += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					else if (flag8 && !flag7 && !currentLocation.isCollidingPosition(nextPosition(3), viewport, isFarmer: false, DamageToFarmer, isGlider, this))
					{
						position.X -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					if (!currentLocation.isTilePassable(nextPosition(2), viewport) || !base.willDestroyObjectsUnderfoot)
					{
						Halt();
					}
					else if (base.willDestroyObjectsUnderfoot)
					{
						Vector2 vector3 = new Vector2(getStandingX() / 64, getStandingY() / 64 + 1);
						if (currentLocation.characterDestroyObjectWithinRectangle(nextPosition(2), showDestroyedObject: true))
						{
							currentLocation.playSound("stoneCrack");
							position.Y += base.speed + base.addedSpeed;
						}
						else
						{
							blockedInterval += time.ElapsedGameTime.Milliseconds;
						}
					}
					if (onCollision != null)
					{
						onCollision(currentLocation);
					}
				}
			}
			else if (moveLeft)
			{
				if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(nextPosition(3), viewport, isFarmer: false, DamageToFarmer, isGlider, this)) || isCharging)
				{
					position.X -= base.speed + base.addedSpeed;
					FacingDirection = 3;
					if (!ignoreMovementAnimations)
					{
						Sprite.AnimateLeft(time);
					}
					faceDirection(3);
				}
				else
				{
					Microsoft.Xna.Framework.Rectangle rectangle4 = nextPosition(3);
					rectangle4.Height /= 4;
					bool flag9 = currentLocation.isCollidingPosition(rectangle4, viewport, isFarmer: false, DamageToFarmer, isGlider, this);
					rectangle4.Y += rectangle4.Height * 3;
					bool flag10 = currentLocation.isCollidingPosition(rectangle4, viewport, isFarmer: false, DamageToFarmer, isGlider, this);
					if (flag9 && !flag10 && !currentLocation.isCollidingPosition(nextPosition(2), viewport, isFarmer: false, DamageToFarmer, isGlider, this))
					{
						position.Y += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					else if (flag10 && !flag9 && !currentLocation.isCollidingPosition(nextPosition(0), viewport, isFarmer: false, DamageToFarmer, isGlider, this))
					{
						position.Y -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					if (!currentLocation.isTilePassable(nextPosition(3), viewport) || !base.willDestroyObjectsUnderfoot)
					{
						Halt();
					}
					else if (base.willDestroyObjectsUnderfoot)
					{
						Vector2 vector4 = new Vector2(getStandingX() / 64 - 1, getStandingY() / 64);
						if (currentLocation.characterDestroyObjectWithinRectangle(nextPosition(3), showDestroyedObject: true))
						{
							currentLocation.playSound("stoneCrack");
							position.X -= base.speed + base.addedSpeed;
						}
						else
						{
							blockedInterval += time.ElapsedGameTime.Milliseconds;
						}
					}
					if (onCollision != null)
					{
						onCollision(currentLocation);
					}
				}
			}
			else if (!ignoreMovementAnimations)
			{
				if (moveUp)
				{
					Sprite.AnimateUp(time);
				}
				else if (moveRight)
				{
					Sprite.AnimateRight(time);
				}
				else if (moveDown)
				{
					Sprite.AnimateDown(time);
				}
				else if (moveLeft)
				{
					Sprite.AnimateLeft(time);
				}
			}
			if ((blockedInterval < 3000 || !((float)blockedInterval <= 3750f)) && blockedInterval >= 5000)
			{
				base.speed = 4;
				isCharging = true;
				blockedInterval = 0;
			}
			if (DamageToFarmer <= 0 || !(Game1.random.NextDouble() < 0.0003333333333333333))
			{
				return;
			}
			if (base.Name.Equals("Shadow Guy") && Game1.random.NextDouble() < 0.3)
			{
				if (Game1.random.NextDouble() < 0.5)
				{
					currentLocation.playSound("grunt");
				}
				else
				{
					currentLocation.playSound("shadowpeep");
				}
			}
			else if (!base.Name.Equals("Shadow Girl"))
			{
				if (base.Name.Equals("Ghost"))
				{
					currentLocation.playSound("ghost");
				}
				else if (!base.Name.Contains("Slime"))
				{
					base.Name.Contains("Jelly");
				}
			}
		}
	}
}
