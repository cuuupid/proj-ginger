using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using xTile.Dimensions;

namespace StardewValley.Characters
{
	public class Horse : NPC
	{
		private readonly NetGuid horseId = new NetGuid();

		private readonly NetFarmerRef netRider = new NetFarmerRef();

		public readonly NetLong ownerId = new NetLong();

		[XmlIgnore]
		public readonly NetBool mounting = new NetBool();

		[XmlIgnore]
		public readonly NetBool dismounting = new NetBool();

		private Vector2 dismountTile;

		private int ridingAnimationDirection;

		private bool roomForHorseAtDismountTile;

		[XmlElement("hat")]
		public readonly NetRef<Hat> hat = new NetRef<Hat>();

		public readonly NetMutex mutex = new NetMutex();

		[XmlIgnore]
		public Action<string> onFootstepAction;

		private bool squeezingThroughGate;

		public bool checkActionEnabled = true;

		public Guid HorseId
		{
			get
			{
				return horseId.Value;
			}
			set
			{
				horseId.Value = value;
			}
		}

		[XmlIgnore]
		public Farmer rider
		{
			get
			{
				return netRider.Value;
			}
			set
			{
				netRider.Value = value;
			}
		}

		public Horse()
		{
			Sprite = new AnimatedSprite("Animals\\horse", 0, 32, 32);
			base.Breather = false;
			base.willDestroyObjectsUnderfoot = false;
			base.HideShadow = true;
			Sprite.textureUsesFlippedRightForLeft = true;
			Sprite.loop = true;
			drawOffset.Set(new Vector2(-16f, 0f));
			faceDirection(3);
			onFootstepAction = PerformDefaultHorseFootstep;
		}

		public Horse(Guid horseId, int xTile, int yTile)
			: this()
		{
			base.Name = "";
			displayName = base.Name;
			base.Position = new Vector2(xTile, yTile) * 64f;
			base.currentLocation = Game1.currentLocation;
			HorseId = horseId;
		}

		public override bool canTalk()
		{
			return false;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(horseId, netRider.NetFields, mounting, dismounting, hat, mutex.NetFields, ownerId);
			position.Field.AxisAlignedMovement = false;
		}

		public Farmer getOwner()
		{
			if (ownerId.Value == 0L)
			{
				return null;
			}
			return Game1.getFarmerMaybeOffline(ownerId.Value);
		}

		public override void reloadSprite()
		{
		}

		public override void dayUpdate(int dayOfMonth)
		{
			faceDirection(3);
		}

		public Microsoft.Xna.Framework.Rectangle GetAlternativeBoundingBox()
		{
			if (FacingDirection == 0 || FacingDirection == 2)
			{
				return new Microsoft.Xna.Framework.Rectangle((int)base.Position.X, (int)base.Position.Y - 128, 64, 192);
			}
			return new Microsoft.Xna.Framework.Rectangle((int)base.Position.X - 32, (int)base.Position.Y, 128, 64);
		}

		public override Microsoft.Xna.Framework.Rectangle GetBoundingBox()
		{
			int num = (int)Math.Min(48.0, (double)(Sprite.SpriteWidth * 4) * 0.75);
			return new Microsoft.Xna.Framework.Rectangle((int)base.Position.X + (64 - num) / 2, (int)base.Position.Y + 16, num, 32);
		}

		public override Vector2 getLocalPosition(xTile.Dimensions.Rectangle viewport)
		{
			return new Vector2(base.Position.X - (float)viewport.X - 18f, base.Position.Y - (float)viewport.Y + (float)yJumpOffset) + drawOffset;
		}

		public override bool canPassThroughActionTiles()
		{
			return false;
		}

		public void squeezeForGate()
		{
			squeezingThroughGate = true;
			if (rider != null)
			{
				rider.TemporaryPassableTiles.Add(GetBoundingBox());
			}
		}

		public override void update(GameTime time, GameLocation location)
		{
			base.currentLocation = location;
			mutex.Update(location);
			squeezingThroughGate = false;
			faceTowardFarmer = false;
			faceTowardFarmerTimer = -1;
			Sprite.loop = rider != null && !rider.hidden;
			if (rider != null && (bool)rider.hidden)
			{
				return;
			}
			if (rider != null && rider.isAnimatingMount)
			{
				rider.showNotCarrying();
			}
			if ((bool)mounting)
			{
				if (rider == null || !rider.IsLocalPlayer)
				{
					return;
				}
				if (rider.mount != null)
				{
					mounting.Value = false;
					rider.isAnimatingMount = false;
					rider = null;
					Halt();
					farmerPassesThrough = false;
					return;
				}
				if (rider.Position.X < (float)(GetBoundingBox().X + 16 - 4))
				{
					rider.position.X += 4f;
				}
				else if (rider.Position.X > (float)(GetBoundingBox().X + 16 + 4))
				{
					rider.position.X -= 4f;
				}
				if (rider.getStandingY() < GetBoundingBox().Y - 4)
				{
					rider.position.Y += 4f;
				}
				else if (rider.getStandingY() > GetBoundingBox().Y + 4)
				{
					rider.position.Y -= 4f;
				}
				if (rider.yJumpOffset >= -8 && rider.yJumpVelocity <= 0f)
				{
					Halt();
					Sprite.loop = true;
					base.currentLocation.characters.Remove(this);
					rider.mount = this;
					rider.freezePause = -1;
					mounting.Value = false;
					rider.isAnimatingMount = false;
					rider.canMove = true;
					if (FacingDirection == 1)
					{
						rider.xOffset += 8f;
					}
				}
			}
			else if ((bool)dismounting)
			{
				if (rider == null || !rider.IsLocalPlayer)
				{
					Halt();
					return;
				}
				if (rider.isAnimatingMount)
				{
					rider.faceDirection(FacingDirection);
				}
				Vector2 vector = new Vector2(dismountTile.X * 64f + 32f - (float)(rider.GetBoundingBox().Width / 2), dismountTile.Y * 64f + 4f);
				if (Math.Abs(rider.Position.X - vector.X) > 4f)
				{
					if (rider.Position.X < vector.X)
					{
						rider.position.X += Math.Min(4f, vector.X - rider.Position.X);
					}
					else if (rider.Position.X > vector.X)
					{
						rider.position.X += Math.Max(-4f, vector.X - rider.Position.X);
					}
				}
				if (Math.Abs(rider.Position.Y - vector.Y) > 4f)
				{
					if (rider.Position.Y < vector.Y)
					{
						rider.position.Y += Math.Min(4f, vector.Y - rider.Position.Y);
					}
					else if (rider.Position.Y > vector.Y)
					{
						rider.position.Y += Math.Max(-4f, vector.Y - rider.Position.Y);
					}
				}
				if (rider.yJumpOffset >= 0 && rider.yJumpVelocity <= 0f)
				{
					rider.position.Y += 8f;
					rider.position.X = vector.X;
					int num = 0;
					while (rider.currentLocation.isCollidingPosition(rider.GetBoundingBox(), Game1.viewport, isFarmer: true, 0, glider: false, rider) && num < 6)
					{
						num++;
						rider.position.Y -= 4f;
					}
					if (num == 6)
					{
						rider.Position = base.Position;
						dismounting.Value = false;
						rider.isAnimatingMount = false;
						rider.freezePause = -1;
						rider.canMove = true;
						return;
					}
					dismount();
				}
			}
			else if (rider == null && FacingDirection != 2 && Sprite.CurrentAnimation == null && Game1.random.NextDouble() < 0.002)
			{
				Sprite.loop = false;
				switch (FacingDirection)
				{
				case 0:
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(25, Game1.random.Next(250, 750)),
						new FarmerSprite.AnimationFrame(14, 10)
					});
					break;
				case 1:
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(21, 100),
						new FarmerSprite.AnimationFrame(22, 100),
						new FarmerSprite.AnimationFrame(23, 400),
						new FarmerSprite.AnimationFrame(24, 400),
						new FarmerSprite.AnimationFrame(23, 400),
						new FarmerSprite.AnimationFrame(24, 400),
						new FarmerSprite.AnimationFrame(23, 400),
						new FarmerSprite.AnimationFrame(24, 400),
						new FarmerSprite.AnimationFrame(23, 400),
						new FarmerSprite.AnimationFrame(22, 100),
						new FarmerSprite.AnimationFrame(21, 100)
					});
					break;
				case 3:
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(21, 100, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(22, 100, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(23, 100, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(24, 400, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(23, 400, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(24, 400, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(23, 400, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(24, 400, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(23, 400, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(22, 100, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(21, 100, secondaryArm: false, flip: true)
					});
					break;
				}
			}
			else if (rider != null)
			{
				if (FacingDirection != rider.FacingDirection || ridingAnimationDirection != FacingDirection)
				{
					Sprite.StopAnimation();
					faceDirection(rider.FacingDirection);
				}
				bool flag = (rider.movementDirections.Any() && rider.CanMove) || rider.position.Field.IsInterpolating();
				SyncPositionToRider();
				if (flag && Sprite.CurrentAnimation == null)
				{
					if (FacingDirection == 1)
					{
						Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(8, 70),
							new FarmerSprite.AnimationFrame(9, 70, secondaryArm: false, flip: false, OnMountFootstep),
							new FarmerSprite.AnimationFrame(10, 70, secondaryArm: false, flip: false, OnMountFootstep),
							new FarmerSprite.AnimationFrame(11, 70, secondaryArm: false, flip: false, OnMountFootstep),
							new FarmerSprite.AnimationFrame(12, 70),
							new FarmerSprite.AnimationFrame(13, 70)
						});
					}
					else if (FacingDirection == 3)
					{
						Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(8, 70, secondaryArm: false, flip: true),
							new FarmerSprite.AnimationFrame(9, 70, secondaryArm: false, flip: true, OnMountFootstep),
							new FarmerSprite.AnimationFrame(10, 70, secondaryArm: false, flip: true, OnMountFootstep),
							new FarmerSprite.AnimationFrame(11, 70, secondaryArm: false, flip: true, OnMountFootstep),
							new FarmerSprite.AnimationFrame(12, 70, secondaryArm: false, flip: true),
							new FarmerSprite.AnimationFrame(13, 70, secondaryArm: false, flip: true)
						});
					}
					else if (FacingDirection == 0)
					{
						Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(15, 70),
							new FarmerSprite.AnimationFrame(16, 70, secondaryArm: false, flip: false, OnMountFootstep),
							new FarmerSprite.AnimationFrame(17, 70, secondaryArm: false, flip: false, OnMountFootstep),
							new FarmerSprite.AnimationFrame(18, 70, secondaryArm: false, flip: false, OnMountFootstep),
							new FarmerSprite.AnimationFrame(19, 70),
							new FarmerSprite.AnimationFrame(20, 70)
						});
					}
					else if (FacingDirection == 2)
					{
						Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(1, 70),
							new FarmerSprite.AnimationFrame(2, 70, secondaryArm: false, flip: false, OnMountFootstep),
							new FarmerSprite.AnimationFrame(3, 70, secondaryArm: false, flip: false, OnMountFootstep),
							new FarmerSprite.AnimationFrame(4, 70, secondaryArm: false, flip: false, OnMountFootstep),
							new FarmerSprite.AnimationFrame(5, 70),
							new FarmerSprite.AnimationFrame(6, 70)
						});
					}
					ridingAnimationDirection = FacingDirection;
				}
				if (!flag)
				{
					Sprite.StopAnimation();
					faceDirection(rider.FacingDirection);
				}
			}
			if (FacingDirection == 3)
			{
				drawOffset.Set(Vector2.Zero);
			}
			else
			{
				drawOffset.Set(new Vector2(-16f, 0f));
			}
			flip = FacingDirection == 3;
			base.update(time, location);
		}

		public virtual void OnMountFootstep(Farmer who)
		{
			if (onFootstepAction != null && rider != null)
			{
				string obj = rider.currentLocation.doesTileHaveProperty((int)rider.getTileLocation().X, (int)rider.getTileLocation().Y, "Type", "Back");
				onFootstepAction(obj);
			}
		}

		public virtual void PerformDefaultHorseFootstep(string step_type)
		{
			if (rider == null)
			{
				return;
			}
			if (!(step_type == "Stone"))
			{
				if (step_type == "Wood")
				{
					if (rider.ShouldHandleAnimationSound())
					{
						rider.currentLocation.localSoundAt("woodyStep", getTileLocation());
					}
					_ = rider;
					_ = Game1.player;
				}
				else
				{
					if (rider.ShouldHandleAnimationSound())
					{
						rider.currentLocation.localSoundAt("thudStep", getTileLocation());
					}
					_ = rider;
					_ = Game1.player;
				}
			}
			else
			{
				if (rider.ShouldHandleAnimationSound())
				{
					rider.currentLocation.localSoundAt("stoneStep", getTileLocation());
				}
				_ = rider;
				_ = Game1.player;
			}
		}

		public override void collisionWithFarmerBehavior()
		{
			base.collisionWithFarmerBehavior();
		}

		public void dismount(bool from_demolish = false)
		{
			mutex.ReleaseLock();
			rider.mount = null;
			if (base.currentLocation == null)
			{
				return;
			}
			Stable stable = null;
			foreach (Building building in Game1.getFarm().buildings)
			{
				if (building is Stable && (building as Stable).HorseId == HorseId)
				{
					stable = building as Stable;
					break;
				}
			}
			if (stable != null && !from_demolish && !base.currentLocation.characters.Where((NPC c) => c is Horse && (c as Horse).HorseId == HorseId).Any())
			{
				base.currentLocation.characters.Add(this);
			}
			SyncPositionToRider();
			rider.TemporaryPassableTiles.Add(new Microsoft.Xna.Framework.Rectangle((int)dismountTile.X * 64, (int)dismountTile.Y * 64, 64, 64));
			rider.freezePause = -1;
			dismounting.Value = false;
			rider.isAnimatingMount = false;
			rider.canMove = true;
			rider.forceCanMove();
			rider.xOffset = 0f;
			rider = null;
			Halt();
			farmerPassesThrough = false;
		}

		public void nameHorse(string name)
		{
			if (name.Length <= 0)
			{
				return;
			}
			Game1.multiplayer.globalChatInfoMessage("HorseNamed", Game1.player.Name, name);
			foreach (NPC allCharacter in Utility.getAllCharacters())
			{
				if (allCharacter.isVillager() && allCharacter.Name.Equals(name))
				{
					name += " ";
				}
			}
			base.Name = name;
			displayName = name;
			if (Game1.player.horseName.Value == null || Game1.player.horseName == "")
			{
				Game1.player.horseName.Value = name;
			}
			Game1.exitActiveMenu();
			Game1.playSound("newArtifact");
			if (mutex.IsLockHeld())
			{
				mutex.ReleaseLock();
			}
		}

		public override bool checkAction(Farmer who, GameLocation l)
		{
			if (who != null && !who.canMove)
			{
				return false;
			}
			if (!checkActionEnabled)
			{
				checkActionEnabled = true;
				return false;
			}
			if (Game1.locationRequest != null && !Game1.locationRequest.Name.Equals(Game1.currentLocation.Name))
			{
				return false;
			}
			if (rider == null)
			{
				mutex.RequestLock(delegate
				{
					if (who.mount != null || rider != null || who.FarmerSprite.PauseForSingleAnimation)
					{
						mutex.ReleaseLock();
					}
					else if ((getOwner() == Game1.player || (getOwner() == null && (Game1.player.horseName.Value == null || Game1.player.horseName.Value.Length == 0 || Utility.findHorseForPlayer(Game1.player.UniqueMultiplayerID) == null))) && base.Name.Length <= 0)
					{
						Farm farm = Game1.getLocationFromName("Farm") as Farm;
						foreach (Building building in farm.buildings)
						{
							if ((int)building.daysOfConstructionLeft <= 0 && building is Stable)
							{
								Stable stable = building as Stable;
								if (stable.getStableHorse() == this)
								{
									stable.owner.Value = who.UniqueMultiplayerID;
									stable.updateHorseOwnership();
								}
								else if ((long)stable.owner == who.UniqueMultiplayerID)
								{
									stable.owner.Value = 0L;
									stable.updateHorseOwnership();
								}
							}
						}
						if (Game1.player.horseName.Value == null || Game1.player.horseName.Value.Length == 0)
						{
							Game1.activeClickableMenu = new NamingMenu(nameHorse, Game1.content.LoadString("Strings\\Characters:NameYourHorse"), Game1.content.LoadString("Strings\\Characters:DefaultHorseName"));
						}
					}
					else if (who.CurrentToolIndex >= 0 && who.items.Count > who.CurrentToolIndex && who.items[who.CurrentToolIndex] != null && who.Items[who.CurrentToolIndex] is Hat)
					{
						if (hat.Value != null)
						{
							Game1.createItemDebris((Hat)hat, position, facingDirection);
							hat.Value = null;
						}
						else
						{
							Hat value2 = who.Items[who.CurrentToolIndex] as Hat;
							who.Items[who.CurrentToolIndex] = null;
							hat.Value = value2;
							Game1.playSound("dirtyHit");
						}
						mutex.ReleaseLock();
					}
					else if (!Game1.currentLocation.tapToMove.preventMountingHorse)
					{
						rider = who;
						rider.freezePause = 5000;
						rider.synchronizedJump(6f);
						rider.Halt();
						if (rider.Position.X < base.Position.X)
						{
							rider.faceDirection(1);
						}
						l.playSound("dwop");
						mounting.Value = true;
						rider.isAnimatingMount = true;
						rider.completelyStopAnimatingOrDoingAction();
						rider.faceGeneralDirection(Utility.PointToVector2(GetBoundingBox().Center), 0, opposite: false, useTileCalculations: false);
					}
				});
				return true;
			}
			dismounting.Value = true;
			rider.isAnimatingMount = true;
			farmerPassesThrough = false;
			rider.TemporaryPassableTiles.Clear();
			Vector2 value = Utility.recursiveFindOpenTileForCharacter(rider, rider.currentLocation, rider.getTileLocation(), 8, allowOffMap: false);
			base.Position = new Vector2(value.X * 64f + 32f - (float)(GetBoundingBox().Width / 2), value.Y * 64f + 4f);
			roomForHorseAtDismountTile = !base.currentLocation.isCollidingPosition(GetBoundingBox(), Game1.viewport, isFarmer: true, 0, glider: false, this);
			base.Position = rider.Position;
			dismounting.Value = false;
			rider.isAnimatingMount = false;
			Halt();
			if (!value.Equals(Vector2.Zero) && Vector2.Distance(value, rider.getTileLocation()) < 2f)
			{
				rider.synchronizedJump(6f);
				l.playSound("dwop");
				rider.freezePause = 5000;
				rider.Halt();
				rider.xOffset = 0f;
				dismounting.Value = true;
				rider.isAnimatingMount = true;
				dismountTile = value;
				Game1.debugOutput = "dismount tile: " + value.ToString();
			}
			else
			{
				dismount();
			}
			return true;
		}

		public void SyncPositionToRider()
		{
			if (rider != null && (!dismounting || roomForHorseAtDismountTile))
			{
				base.Position = rider.position;
			}
		}

		public override void draw(SpriteBatch b)
		{
			flip = FacingDirection == 3;
			Sprite.UpdateSourceRect();
			base.draw(b);
			if (FacingDirection == 2 && rider != null)
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(48f, -24f - rider.yOffset), new Microsoft.Xna.Framework.Rectangle(160, 96, 9, 15), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (base.Position.Y + 64f) / 10000f);
			}
			bool flag = true;
			if (hat.Value == null)
			{
				return;
			}
			Vector2 zero = Vector2.Zero;
			switch ((int)hat.Value.which)
			{
			case 14:
				if ((int)facingDirection == 0)
				{
					zero.X = -100f;
				}
				break;
			case 6:
				zero.Y += 2f;
				if (FacingDirection == 2)
				{
					zero.Y -= 1f;
				}
				break;
			case 10:
				zero.Y += 3f;
				if ((int)facingDirection == 0)
				{
					flag = false;
				}
				break;
			case 9:
			case 32:
				if (FacingDirection == 0 || FacingDirection == 2)
				{
					zero.Y += 1f;
				}
				break;
			case 31:
				zero.Y += 1f;
				break;
			case 11:
			case 39:
				if (FacingDirection == 3 || FacingDirection == 1)
				{
					if (flip)
					{
						zero.X += 2f;
					}
					else
					{
						zero.X -= 2f;
					}
				}
				break;
			case 26:
				if (FacingDirection == 3 || FacingDirection == 1)
				{
					if (flip)
					{
						zero.X += 1f;
					}
					else
					{
						zero.X -= 1f;
					}
				}
				break;
			case 56:
			case 67:
				if (FacingDirection == 0)
				{
					flag = false;
				}
				break;
			}
			zero *= 4f;
			if (shakeTimer > 0)
			{
				zero += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
			}
			if (zero.X <= -100f)
			{
				return;
			}
			float num = (float)GetBoundingBox().Center.Y / 10000f;
			if (rider != null)
			{
				num = ((FacingDirection == 0) ? ((position.Y + 64f - 32f) / 10000f) : ((FacingDirection != 2) ? ((position.Y + 64f - 1f) / 10000f) : ((position.Y + 64f + (float)((rider != null) ? 1 : 1)) / 10000f)));
			}
			if (!flag)
			{
				return;
			}
			num += 1E-07f;
			switch (Sprite.CurrentFrame)
			{
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
				hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + zero + new Vector2(30f, -42f - ((rider != null) ? rider.yOffset : 0f))), 1.3333334f, 1f, num, 2);
				break;
			case 7:
			case 11:
				if (flip)
				{
					hat.Value.draw(b, getLocalPosition(Game1.viewport) + zero + new Vector2(-14f, -74f), 1.3333334f, 1f, num, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + zero + new Vector2(66f, -74f)), 1.3333334f, 1f, num, 1);
				}
				break;
			case 8:
				if (flip)
				{
					hat.Value.draw(b, getLocalPosition(Game1.viewport) + zero + new Vector2(-18f, -74f), 1.3333334f, 1f, num, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + zero + new Vector2(70f, -74f)), 1.3333334f, 1f, num, 1);
				}
				break;
			case 9:
				if (flip)
				{
					hat.Value.draw(b, getLocalPosition(Game1.viewport) + zero + new Vector2(-18f, -70f), 1.3333334f, 1f, num, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + zero + new Vector2(70f, -70f)), 1.3333334f, 1f, num, 1);
				}
				break;
			case 10:
				if (flip)
				{
					hat.Value.draw(b, getLocalPosition(Game1.viewport) + zero + new Vector2(-14f, -70f), 1.3333334f, 1f, num, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + zero + new Vector2(66f, -70f)), 1.3333334f, 1f, num, 1);
				}
				break;
			case 12:
				if (flip)
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + zero + new Vector2(-14f, -78f)), 1.3333334f, 1f, num, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + zero + new Vector2(66f, -78f)), 1.3333334f, 1f, num, 1);
				}
				break;
			case 13:
				if (flip)
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + zero + new Vector2(-18f, -78f)), 1.3333334f, 1f, num, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + zero + new Vector2(70f, -78f)), 1.3333334f, 1f, num, 1);
				}
				break;
			case 21:
				if (flip)
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + zero + new Vector2(-14f, -66f)), 1.3333334f, 1f, num, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + zero + new Vector2(66f, -66f)), 1.3333334f, 1f, num, 1);
				}
				break;
			case 22:
				if (flip)
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + zero + new Vector2(-18f, -54f)), 1.3333334f, 1f, num, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + zero + new Vector2(70f, -54f)), 1.3333334f, 1f, num, 1);
				}
				break;
			case 23:
				if (flip)
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + zero + new Vector2(-18f, -42f)), 1.3333334f, 1f, num, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + zero + new Vector2(70f, -42f)), 1.3333334f, 1f, num, 1);
				}
				break;
			case 24:
				if (flip)
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + zero + new Vector2(-18f, -42f)), 1.3333334f, 1f, num, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + zero + new Vector2(70f, -42f)), 1.3333334f, 1f, num, 1);
				}
				break;
			case 14:
			case 15:
			case 16:
			case 17:
			case 18:
			case 19:
			case 20:
			case 25:
				hat.Value.draw(b, getLocalPosition(Game1.viewport) + zero + new Vector2(28f, -106f - ((rider != null) ? rider.yOffset : 0f)), 1.3333334f, 1f, num, 0);
				break;
			}
		}
	}
}
