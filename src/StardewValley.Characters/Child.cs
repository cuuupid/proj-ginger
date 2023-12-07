using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using xTile.Dimensions;

namespace StardewValley.Characters
{
	public class Child : NPC
	{
		public const int newborn = 0;

		public const int baby = 1;

		public const int crawler = 2;

		public const int toddler = 3;

		[XmlElement("daysOld")]
		public readonly NetInt daysOld = new NetInt(0);

		[XmlElement("idOfParent")]
		public NetLong idOfParent = new NetLong(0L);

		[XmlElement("darkSkinned")]
		public readonly NetBool darkSkinned = new NetBool(value: false);

		private readonly NetEvent1Field<int, NetInt> setStateEvent = new NetEvent1Field<int, NetInt>();

		[XmlElement("hat")]
		public readonly NetRef<Hat> hat = new NetRef<Hat>();

		[XmlIgnore]
		public readonly NetMutex mutex = new NetMutex();

		private int previousState;

		public Child()
		{
		}

		public Child(string name, bool isMale, bool isDarkSkinned, Farmer parent)
		{
			base.Age = 2;
			base.Gender = ((!isMale) ? 1 : 0);
			darkSkinned.Value = isDarkSkinned;
			reloadSprite();
			base.Name = name;
			displayName = name;
			base.DefaultMap = "FarmHouse";
			base.HideShadow = true;
			base.speed = 1;
			idOfParent.Value = parent.UniqueMultiplayerID;
			base.Breather = false;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(setStateEvent, darkSkinned, daysOld, idOfParent, mutex.NetFields, hat);
			age.fieldChangeVisibleEvent += delegate
			{
				reloadSprite();
			};
			setStateEvent.onEvent += doSetState;
			name.FilterStringEvent += Utility.FilterDirtyWords;
		}

		public override void reloadSprite()
		{
			if (Game1.IsMasterGame)
			{
				Farmer farmerMaybeOffline = Game1.getFarmerMaybeOffline(idOfParent.Value);
				if (idOfParent.Value == 0L || farmerMaybeOffline == null)
				{
					long uniqueMultiplayerID = Game1.MasterPlayer.UniqueMultiplayerID;
					if (base.currentLocation is FarmHouse)
					{
						FarmHouse farmHouse = base.currentLocation as FarmHouse;
						foreach (Farmer allFarmer in Game1.getAllFarmers())
						{
							if (Utility.getHomeOfFarmer(allFarmer) == base.currentLocation)
							{
								uniqueMultiplayerID = allFarmer.UniqueMultiplayerID;
								break;
							}
						}
					}
					idOfParent.Value = uniqueMultiplayerID;
				}
			}
			if (Sprite == null)
			{
				Sprite = new AnimatedSprite("Characters\\Baby" + (darkSkinned.Value ? "_dark" : ""), 0, 22, 16);
			}
			if (base.Age >= 3)
			{
				Sprite.textureName.Value = "Characters\\Toddler" + ((base.Gender == 0) ? "" : "_girl") + (darkSkinned.Value ? "_dark" : "");
				Sprite.SpriteWidth = 16;
				Sprite.SpriteHeight = 32;
				Sprite.currentFrame = 0;
				base.HideShadow = false;
			}
			else
			{
				Sprite.textureName.Value = "Characters\\Baby" + (darkSkinned.Value ? "_dark" : "");
				Sprite.SpriteWidth = 22;
				Sprite.SpriteHeight = ((base.Age == 1) ? 32 : 16);
				Sprite.currentFrame = 0;
				if (base.Age == 1)
				{
					Sprite.currentFrame = 4;
				}
				else if (base.Age == 2)
				{
					Sprite.currentFrame = 32;
				}
				base.HideShadow = true;
			}
			Sprite.UpdateSourceRect();
			base.Breather = false;
		}

		protected override void updateSlaveAnimation(GameTime time)
		{
			if (base.Age >= 2 && (Sprite.currentFrame > 7 || Sprite.SpriteHeight != 16))
			{
				base.updateSlaveAnimation(time);
			}
		}

		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			if (Game1.eventUp && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
			{
				base.MovePosition(time, viewport, currentLocation);
				return;
			}
			if (!Game1.IsMasterGame)
			{
				moveLeft = IsRemoteMoving() && FacingDirection == 3;
				moveRight = IsRemoteMoving() && FacingDirection == 1;
				moveUp = IsRemoteMoving() && FacingDirection == 0;
				moveDown = IsRemoteMoving() && FacingDirection == 2;
			}
			if (moveUp)
			{
				if (currentLocation == null || !currentLocation.isCollidingPosition(nextPosition(0), viewport, isFarmer: false, 0, glider: false, this) || isCharging)
				{
					if (Game1.IsMasterGame)
					{
						position.Y -= base.speed + base.addedSpeed;
					}
					if (base.Age == 3)
					{
						Sprite.AnimateUp(time);
						FacingDirection = 0;
					}
				}
				else if (!currentLocation.isTilePassable(nextPosition(0), viewport) || !base.willDestroyObjectsUnderfoot)
				{
					moveUp = false;
					Sprite.currentFrame = ((Sprite.CurrentAnimation != null) ? Sprite.CurrentAnimation[0].frame : Sprite.currentFrame);
					Sprite.CurrentAnimation = null;
					if (Game1.IsMasterGame && base.Age == 2 && Game1.timeOfDay < 1800)
					{
						setCrawlerInNewDirection();
					}
				}
			}
			else if (moveRight)
			{
				if (currentLocation == null || !currentLocation.isCollidingPosition(nextPosition(1), viewport, isFarmer: false, 0, glider: false, this) || isCharging)
				{
					if (Game1.IsMasterGame)
					{
						position.X += base.speed + base.addedSpeed;
					}
					if (base.Age == 3)
					{
						Sprite.AnimateRight(time);
						FacingDirection = 1;
					}
				}
				else if (!currentLocation.isTilePassable(nextPosition(1), viewport) || !base.willDestroyObjectsUnderfoot)
				{
					moveRight = false;
					Sprite.currentFrame = ((Sprite.CurrentAnimation != null) ? Sprite.CurrentAnimation[0].frame : Sprite.currentFrame);
					Sprite.CurrentAnimation = null;
					if (Game1.IsMasterGame && base.Age == 2 && Game1.timeOfDay < 1800)
					{
						setCrawlerInNewDirection();
					}
				}
			}
			else if (moveDown)
			{
				if (currentLocation == null || !currentLocation.isCollidingPosition(nextPosition(2), viewport, isFarmer: false, 0, glider: false, this) || isCharging)
				{
					if (Game1.IsMasterGame)
					{
						position.Y += base.speed + base.addedSpeed;
					}
					if (base.Age == 3)
					{
						Sprite.AnimateDown(time);
						FacingDirection = 2;
					}
				}
				else if (!currentLocation.isTilePassable(nextPosition(2), viewport) || !base.willDestroyObjectsUnderfoot)
				{
					moveDown = false;
					Sprite.currentFrame = ((Sprite.CurrentAnimation != null) ? Sprite.CurrentAnimation[0].frame : Sprite.currentFrame);
					Sprite.CurrentAnimation = null;
					if (Game1.IsMasterGame && base.Age == 2 && Game1.timeOfDay < 1800)
					{
						setCrawlerInNewDirection();
					}
				}
			}
			else if (moveLeft)
			{
				if (currentLocation == null || !currentLocation.isCollidingPosition(nextPosition(3), viewport, isFarmer: false, 0, glider: false, this) || isCharging)
				{
					if (Game1.IsMasterGame)
					{
						position.X -= base.speed + base.addedSpeed;
					}
					if (base.Age == 3)
					{
						Sprite.AnimateLeft(time);
						FacingDirection = 3;
					}
				}
				else if (!currentLocation.isTilePassable(nextPosition(3), viewport) || !base.willDestroyObjectsUnderfoot)
				{
					moveLeft = false;
					Sprite.currentFrame = ((Sprite.CurrentAnimation != null) ? Sprite.CurrentAnimation[0].frame : Sprite.currentFrame);
					Sprite.CurrentAnimation = null;
					if (Game1.IsMasterGame && base.Age == 2 && Game1.timeOfDay < 1800)
					{
						setCrawlerInNewDirection();
					}
				}
			}
			if (blockedInterval >= 3000 && (float)blockedInterval <= 3750f && !Game1.eventUp)
			{
				doEmote((Game1.random.NextDouble() < 0.5) ? 8 : 40);
				blockedInterval = 3750;
			}
			else if (blockedInterval >= 5000)
			{
				base.speed = 1;
				isCharging = true;
				blockedInterval = 0;
			}
		}

		public override bool canPassThroughActionTiles()
		{
			return false;
		}

		public override void resetForNewDay(int dayOfMonth)
		{
			base.resetForNewDay(dayOfMonth);
			if (base.currentLocation is FarmHouse && (base.currentLocation as FarmHouse).GetChildBed(GetChildIndex()) == null)
			{
				sleptInBed.Value = false;
			}
		}

		protected override string translateName(string name)
		{
			return name.TrimEnd();
		}

		public override void dayUpdate(int dayOfMonth)
		{
			resetForNewDay(dayOfMonth);
			mutex.ReleaseLock();
			moveUp = false;
			moveDown = false;
			moveLeft = false;
			moveRight = false;
			int num = (int)Game1.MasterPlayer.UniqueMultiplayerID;
			if (Game1.currentLocation is FarmHouse)
			{
				FarmHouse farmHouse = Game1.currentLocation as FarmHouse;
				if (farmHouse.owner != null)
				{
					num = (int)farmHouse.owner.UniqueMultiplayerID;
				}
			}
			Random r = new Random(Game1.Date.TotalDays + (int)Game1.uniqueIDForThisGame / 2 + num * 2);
			daysOld.Value += 1;
			if (daysOld.Value >= 55)
			{
				base.Age = 3;
				base.speed = 4;
			}
			else if (daysOld.Value >= 27)
			{
				base.Age = 2;
			}
			else if (daysOld.Value >= 13)
			{
				base.Age = 1;
			}
			if ((int)age == 0 || (int)age == 1)
			{
				base.Position = new Vector2(16f, 4f) * 64f + new Vector2(0f, -24f);
			}
			if (base.Age == 2)
			{
				base.speed = 1;
				Point randomOpenPointInHouse = (base.currentLocation as FarmHouse).getRandomOpenPointInHouse(r, 1, 200);
				if (!randomOpenPointInHouse.Equals(Point.Zero))
				{
					setTilePosition(randomOpenPointInHouse);
				}
				else
				{
					base.Position = new Vector2(16f, 4f) * 64f + new Vector2(0f, -24f);
				}
				Sprite.CurrentAnimation = null;
			}
			if (base.Age == 3)
			{
				Point randomOpenPointInHouse2 = (base.currentLocation as FarmHouse).getRandomOpenPointInHouse(r, 1, 200);
				if (!randomOpenPointInHouse2.Equals(Point.Zero))
				{
					setTilePosition(randomOpenPointInHouse2);
				}
				else
				{
					FarmHouse farmHouse2 = base.currentLocation as FarmHouse;
					BedFurniture childBed = farmHouse2.GetChildBed(GetChildIndex());
					randomOpenPointInHouse2 = farmHouse2.GetChildBedSpot(GetChildIndex());
					if (!randomOpenPointInHouse2.Equals(Point.Zero))
					{
						setTilePosition(randomOpenPointInHouse2);
					}
				}
				Sprite.CurrentAnimation = null;
			}
			reloadSprite();
			if (base.Age == 2)
			{
				setCrawlerInNewDirection();
			}
		}

		public bool isInCrib()
		{
			if (getTileX() >= 15 && getTileX() <= 17 && getTileY() >= 3)
			{
				return getTileY() <= 4;
			}
			return false;
		}

		public void toss(Farmer who)
		{
			if (Game1.timeOfDay >= 1800 || Sprite.SpriteHeight <= 16)
			{
				return;
			}
			if (who == Game1.player)
			{
				mutex.RequestLock(delegate
				{
					performToss(who);
				});
			}
			else
			{
				performToss(who);
			}
		}

		public void performToss(Farmer who)
		{
			who.forceTimePass = true;
			who.faceDirection(2);
			who.FarmerSprite.PauseForSingleAnimation = false;
			who.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
			{
				new FarmerSprite.AnimationFrame(57, 1500, secondaryArm: false, flip: false, doneTossing, behaviorAtEndOfFrame: true)
			});
			base.Position = who.Position + new Vector2(-16f, -96f);
			yJumpVelocity = Game1.random.Next(12, 19);
			yJumpOffset = -1;
			Game1.playSound("dwop");
			who.CanMove = false;
			who.freezePause = 1500;
			drawOnTop = true;
			Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(4, 100),
				new FarmerSprite.AnimationFrame(5, 100),
				new FarmerSprite.AnimationFrame(6, 100),
				new FarmerSprite.AnimationFrame(7, 100)
			});
		}

		public void doneTossing(Farmer who)
		{
			who.forceTimePass = false;
			resetForPlayerEntry(who.currentLocation);
			who.CanMove = true;
			who.forceCanMove();
			who.faceDirection(0);
			drawOnTop = false;
			doEmote(20);
			if (!who.friendshipData.ContainsKey(base.Name))
			{
				who.friendshipData.Add(base.Name, new Friendship(250));
			}
			who.talkToFriend(this);
			Game1.playSound("tinyWhip");
			if (mutex.IsLockHeld())
			{
				mutex.ReleaseLock();
			}
		}

		public override Microsoft.Xna.Framework.Rectangle getMugShotSourceRect()
		{
			return base.Age switch
			{
				0 => new Microsoft.Xna.Framework.Rectangle(0, 0, 22, 16), 
				1 => new Microsoft.Xna.Framework.Rectangle(0, 42, 22, 24), 
				2 => new Microsoft.Xna.Framework.Rectangle(0, 112, 22, 16), 
				3 => new Microsoft.Xna.Framework.Rectangle(0, 4, 16, 24), 
				_ => Microsoft.Xna.Framework.Rectangle.Empty, 
			};
		}

		private void setState(int state)
		{
			setStateEvent.Fire(state);
		}

		private void doSetState(int state)
		{
			switch (state)
			{
			case 0:
				SetMovingOnlyUp();
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(32, 160),
					new FarmerSprite.AnimationFrame(33, 160),
					new FarmerSprite.AnimationFrame(34, 160),
					new FarmerSprite.AnimationFrame(35, 160)
				});
				break;
			case 1:
				SetMovingOnlyRight();
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(28, 160),
					new FarmerSprite.AnimationFrame(29, 160),
					new FarmerSprite.AnimationFrame(30, 160),
					new FarmerSprite.AnimationFrame(31, 160)
				});
				break;
			case 2:
				SetMovingOnlyDown();
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(24, 160),
					new FarmerSprite.AnimationFrame(25, 160),
					new FarmerSprite.AnimationFrame(26, 160),
					new FarmerSprite.AnimationFrame(27, 160)
				});
				break;
			case 3:
				SetMovingOnlyLeft();
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(36, 160),
					new FarmerSprite.AnimationFrame(37, 160),
					new FarmerSprite.AnimationFrame(38, 160),
					new FarmerSprite.AnimationFrame(39, 160)
				});
				break;
			case 4:
				Halt();
				Sprite.SpriteHeight = 16;
				Sprite.setCurrentAnimation(getRandomCrawlerAnimation(0));
				break;
			case 5:
				Halt();
				Sprite.SpriteHeight = 16;
				Sprite.setCurrentAnimation(getRandomCrawlerAnimation(1));
				break;
			}
		}

		private void setCrawlerInNewDirection()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			base.speed = 1;
			int num = Game1.random.Next(6);
			if (Game1.timeOfDay >= 1800 && isInCrib())
			{
				Sprite.currentFrame = 7;
				Sprite.UpdateSourceRect();
				return;
			}
			if (previousState >= 4 && Game1.random.NextDouble() < 0.6)
			{
				num = previousState;
			}
			if (num < 4)
			{
				while (num == previousState)
				{
					num = Game1.random.Next(6);
				}
			}
			else if (previousState >= 4)
			{
				num = previousState;
			}
			if (isInCrib())
			{
				num = Game1.random.Next(4, 6);
			}
			setState(num);
			previousState = num;
		}

		public override bool hasSpecialCollisionRules()
		{
			return true;
		}

		public override bool isColliding(GameLocation l, Vector2 tile)
		{
			if (!l.isTilePlaceable(tile))
			{
				return true;
			}
			return false;
		}

		public void tenMinuteUpdate()
		{
			if (Game1.IsMasterGame && base.Age == 2)
			{
				setCrawlerInNewDirection();
			}
			else if (Game1.IsMasterGame && Game1.timeOfDay % 100 == 0 && base.Age == 3 && Game1.timeOfDay < 1900)
			{
				base.IsWalkingInSquare = false;
				Halt();
				FarmHouse farmHouse = base.currentLocation as FarmHouse;
				if (farmHouse.characters.Contains(this))
				{
					controller = new PathFindController(this, farmHouse, farmHouse.getRandomOpenPointInHouse(Game1.random, 1), -1, toddlerReachedDestination);
					if (controller.pathToEndPoint == null || !farmHouse.isTileOnMap(controller.pathToEndPoint.Last().X, controller.pathToEndPoint.Last().Y))
					{
						controller = null;
					}
				}
			}
			else
			{
				if (!Game1.IsMasterGame || base.Age != 3 || Game1.timeOfDay != 1900)
				{
					return;
				}
				base.IsWalkingInSquare = false;
				Halt();
				FarmHouse farmHouse2 = base.currentLocation as FarmHouse;
				if (!farmHouse2.characters.Contains(this))
				{
					return;
				}
				int childIndex = GetChildIndex();
				BedFurniture childBed = farmHouse2.GetChildBed(childIndex);
				Point childBedSpot = farmHouse2.GetChildBedSpot(childIndex);
				if (!childBedSpot.Equals(Point.Zero))
				{
					controller = new PathFindController(this, farmHouse2, childBedSpot, -1, toddlerReachedDestination);
					if (controller.pathToEndPoint == null || !farmHouse2.isTileOnMap(controller.pathToEndPoint.Last().X, controller.pathToEndPoint.Last().Y))
					{
						controller = null;
					}
					else
					{
						childBed?.ReserveForNPC();
					}
				}
			}
		}

		public virtual int GetChildIndex()
		{
			Farmer farmerMaybeOffline = Game1.getFarmerMaybeOffline(idOfParent.Value);
			if (farmerMaybeOffline != null)
			{
				List<Child> children = farmerMaybeOffline.getChildren();
				children.Sort((Child a, Child b) => a.daysOld.Value.CompareTo(b.daysOld.Value));
				children.Reverse();
				return children.IndexOf(this);
			}
			return base.Gender;
		}

		public void toddlerReachedDestination(Character c, GameLocation l)
		{
			if (Game1.random.NextDouble() < 0.8 && c.FacingDirection == 2)
			{
				List<FarmerSprite.AnimationFrame> list = new List<FarmerSprite.AnimationFrame>();
				list.Add(new FarmerSprite.AnimationFrame(16, 120, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(17, 120, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(18, 120, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(19, 120, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(18, 120, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(17, 120, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(16, 120, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(0, 1000, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(16, 100, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(17, 100, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(18, 100, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(19, 100, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(18, 300, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(17, 100, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(16, 100, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(0, 2000, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(16, 120, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(17, 180, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(16, 120, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(0, 800, 0, secondaryArm: false, flip: false));
				Sprite.setCurrentAnimation(list);
			}
			else if (Game1.random.NextDouble() < 0.8 && c.FacingDirection == 1)
			{
				List<FarmerSprite.AnimationFrame> list2 = new List<FarmerSprite.AnimationFrame>();
				list2.Add(new FarmerSprite.AnimationFrame(20, 120, 0, secondaryArm: false, flip: false));
				list2.Add(new FarmerSprite.AnimationFrame(21, 70, 0, secondaryArm: false, flip: false));
				list2.Add(new FarmerSprite.AnimationFrame(22, 70, 0, secondaryArm: false, flip: false));
				list2.Add(new FarmerSprite.AnimationFrame(23, 70, 0, secondaryArm: false, flip: false));
				list2.Add(new FarmerSprite.AnimationFrame(22, 999999, 0, secondaryArm: false, flip: false));
				Sprite.setCurrentAnimation(list2);
			}
			else if (Game1.random.NextDouble() < 0.8 && c.FacingDirection == 3)
			{
				List<FarmerSprite.AnimationFrame> list3 = new List<FarmerSprite.AnimationFrame>();
				list3.Add(new FarmerSprite.AnimationFrame(20, 120, 0, secondaryArm: false, flip: true));
				list3.Add(new FarmerSprite.AnimationFrame(21, 70, 0, secondaryArm: false, flip: true));
				list3.Add(new FarmerSprite.AnimationFrame(22, 70, 0, secondaryArm: false, flip: true));
				list3.Add(new FarmerSprite.AnimationFrame(23, 70, 0, secondaryArm: false, flip: true));
				list3.Add(new FarmerSprite.AnimationFrame(22, 999999, 0, secondaryArm: false, flip: true));
				Sprite.setCurrentAnimation(list3);
			}
			else if (c.FacingDirection == 0)
			{
				lastCrossroad = new Microsoft.Xna.Framework.Rectangle(getTileX() * 64, getTileY() * 64, 64, 64);
				squareMovementFacingPreference = -1;
				walkInSquare(4, 4, 2000);
			}
		}

		public override bool canTalk()
		{
			return false;
		}

		public override bool checkAction(Farmer who, GameLocation l)
		{
			if (!who.friendshipData.ContainsKey(base.Name))
			{
				who.friendshipData.Add(base.Name, new Friendship(250));
			}
			if (base.Age >= 2 && !who.hasTalkedToFriendToday(base.Name))
			{
				who.talkToFriend(this);
				doEmote(20);
				if (base.Age == 3)
				{
					faceTowardFarmerForPeriod(4000, 3, faceAway: false, who);
				}
				return true;
			}
			if (Game1.CurrentEvent != null)
			{
				return false;
			}
			if (base.Age >= 3 && who.items.Count > who.CurrentToolIndex && who.CurrentToolIndex >= 0 && who.items[who.CurrentToolIndex] != null && who.Items[who.CurrentToolIndex] is Hat)
			{
				if (hat.Value != null)
				{
					Game1.createItemDebris((Hat)hat, position, facingDirection);
					hat.Value = null;
				}
				else
				{
					Hat value = who.Items[who.CurrentToolIndex] as Hat;
					who.Items[who.CurrentToolIndex] = null;
					hat.Value = value;
					Game1.playSound("dirtyHit");
				}
			}
			return false;
		}

		private List<FarmerSprite.AnimationFrame> getRandomCrawlerAnimation(int which = -1)
		{
			List<FarmerSprite.AnimationFrame> list = new List<FarmerSprite.AnimationFrame>();
			double num = Game1.random.NextDouble();
			if (which == 0 || num < 0.5)
			{
				list.Add(new FarmerSprite.AnimationFrame(40, 500, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(41, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(42, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(43, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(42, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(41, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(40, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(41, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(42, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(43, 1900, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(42, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(41, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(40, 500, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(41, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(40, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(41, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(40, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(40, 1500, 0, secondaryArm: false, flip: false));
			}
			else if (which == 1 || num >= 0.5)
			{
				list.Add(new FarmerSprite.AnimationFrame(44, 1500, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(45, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(44, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(46, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(44, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(45, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(44, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(46, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(44, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(45, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(44, 200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(46, 200, 0, secondaryArm: false, flip: false));
			}
			return list;
		}

		private List<FarmerSprite.AnimationFrame> getRandomNewbornAnimation()
		{
			List<FarmerSprite.AnimationFrame> list = new List<FarmerSprite.AnimationFrame>();
			if (Game1.random.NextDouble() < 0.5)
			{
				list.Add(new FarmerSprite.AnimationFrame(0, 400, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(1, 400, 0, secondaryArm: false, flip: false));
			}
			else
			{
				list.Add(new FarmerSprite.AnimationFrame(1, 3400, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(2, 100, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(3, 100, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(4, 100, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(5, 100, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(6, 4400, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(5, 3400, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(4, 100, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(3, 100, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(2, 100, 0, secondaryArm: false, flip: false));
			}
			return list;
		}

		private List<FarmerSprite.AnimationFrame> getRandomBabyAnimation()
		{
			List<FarmerSprite.AnimationFrame> list = new List<FarmerSprite.AnimationFrame>();
			if (Game1.random.NextDouble() < 0.5)
			{
				list.Add(new FarmerSprite.AnimationFrame(4, 120, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(5, 120, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(6, 120, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(7, 120, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(4, 100, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(5, 100, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(6, 100, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(7, 100, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(4, 150, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(5, 150, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(6, 150, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(7, 150, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(4, 2000, 0, secondaryArm: false, flip: false));
				if (Game1.random.NextDouble() < 0.5)
				{
					list.Add(new FarmerSprite.AnimationFrame(8, 1950, 0, secondaryArm: false, flip: false));
					list.Add(new FarmerSprite.AnimationFrame(9, 1200, 0, secondaryArm: false, flip: false));
					list.Add(new FarmerSprite.AnimationFrame(10, 180, 0, secondaryArm: false, flip: false));
					list.Add(new FarmerSprite.AnimationFrame(11, 1500, 0, secondaryArm: false, flip: false));
					list.Add(new FarmerSprite.AnimationFrame(8, 1500, 0, secondaryArm: false, flip: false));
				}
			}
			else
			{
				list.Add(new FarmerSprite.AnimationFrame(8, 250, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(9, 250, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(10, 250, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(11, 250, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(8, 1950, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(9, 1200, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(10, 180, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(11, 1500, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(8, 1500, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(9, 150, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(10, 150, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(11, 150, 0, secondaryArm: false, flip: false));
				list.Add(new FarmerSprite.AnimationFrame(8, 1500, 0, secondaryArm: false, flip: false));
			}
			return list;
		}

		public override void update(GameTime time, GameLocation location)
		{
			setStateEvent.Poll();
			mutex.Update(location);
			base.update(time, location);
			if (base.Age >= 2 && (Game1.IsMasterGame || base.Age < 3))
			{
				MovePosition(time, Game1.viewport, location);
			}
		}

		public void resetForPlayerEntry(GameLocation l)
		{
			if (base.Age == 0)
			{
				base.Position = new Vector2(16f, 4f) * 64f + new Vector2(0f, -24f);
				if (Game1.timeOfDay >= 1800 && Sprite != null)
				{
					Sprite.StopAnimation();
					Sprite.currentFrame = Game1.random.Next(7);
				}
				else if (Sprite != null)
				{
					Sprite.setCurrentAnimation(getRandomNewbornAnimation());
				}
			}
			else if (base.Age == 1)
			{
				base.Position = new Vector2(16f, 4f) * 64f + new Vector2(0f, -12f);
				if (Game1.timeOfDay >= 1800 && Sprite != null)
				{
					Sprite.StopAnimation();
					Sprite.SpriteHeight = 16;
					Sprite.currentFrame = Game1.random.Next(7);
				}
				else if (Sprite != null)
				{
					Sprite.SpriteHeight = 32;
					Sprite.setCurrentAnimation(getRandomBabyAnimation());
				}
			}
			else if (base.Age == 2)
			{
				if (Sprite != null)
				{
					Sprite.SpriteHeight = 16;
				}
				if (Game1.timeOfDay >= 1800)
				{
					base.Position = new Vector2(16f, 4f) * 64f + new Vector2(0f, -24f);
					if (Sprite != null)
					{
						Sprite.StopAnimation();
						Sprite.SpriteHeight = 16;
						Sprite.currentFrame = 7;
					}
				}
			}
			if (Sprite != null)
			{
				Sprite.loop = true;
			}
			if (drawOnTop && !mutex.IsLocked())
			{
				drawOnTop = false;
			}
			Sprite.UpdateSourceRect();
		}

		public override void draw(SpriteBatch b, float alpha = 1f)
		{
			Microsoft.Xna.Framework.Rectangle sourceRect = Sprite.SourceRect;
			int spriteHeight = Sprite.SpriteHeight;
			int num = yJumpOffset;
			if (hat.Value != null && hat.Value.hairDrawType.Value != 0)
			{
				Microsoft.Xna.Framework.Rectangle sourceRect2 = Sprite.SourceRect;
				int num2 = 17;
				switch (Sprite.CurrentFrame)
				{
				case 0:
					num2 = 17;
					break;
				case 1:
					num2 = 18;
					break;
				case 2:
					num2 = 17;
					break;
				case 3:
					num2 = 16;
					break;
				case 4:
					num2 = 17;
					break;
				case 5:
					num2 = 18;
					break;
				case 6:
					num2 = 17;
					break;
				case 7:
					num2 = 16;
					break;
				case 8:
					num2 = 17;
					break;
				case 9:
					num2 = 18;
					break;
				case 10:
					num2 = 17;
					break;
				case 11:
					num2 = 16;
					break;
				case 12:
					num2 = 17;
					break;
				case 13:
					num2 = 16;
					break;
				case 14:
					num2 = 17;
					break;
				case 15:
					num2 = 18;
					break;
				case 16:
					num2 = 17;
					break;
				case 17:
					num2 = 17;
					break;
				case 18:
					num2 = 16;
					break;
				case 19:
					num2 = 16;
					break;
				case 20:
					num2 = 17;
					break;
				case 21:
					num2 = 16;
					break;
				case 22:
					num2 = 15;
					break;
				case 23:
					num2 = 14;
					break;
				}
				int num3 = sourceRect.Height - num2;
				sourceRect2.Y += sourceRect.Height - num2;
				sourceRect2.Height = num2;
				Sprite.SourceRect = sourceRect2;
				Sprite.SpriteHeight = num2;
				yJumpOffset = num3;
			}
			base.draw(b, 1f);
			Sprite.SpriteHeight = spriteHeight;
			Sprite.SourceRect = sourceRect;
			yJumpOffset = num;
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (base.IsEmoting && !Game1.eventUp)
			{
				Vector2 localPosition = getLocalPosition(Game1.viewport);
				localPosition.Y -= 32 + Sprite.SpriteHeight * 4 - ((base.Age == 1 || base.Age == 3) ? 64 : 0);
				localPosition.X += ((base.Age == 1) ? 8 : 0);
				b.Draw(Game1.emoteSpriteSheet, localPosition, new Microsoft.Xna.Framework.Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)getStandingY() / 10000f);
			}
			bool flag = true;
			if (hat.Value == null)
			{
				return;
			}
			Vector2 zero = Vector2.Zero;
			zero *= 4f;
			if (zero.X <= -100f)
			{
				return;
			}
			float num = (float)GetBoundingBox().Center.Y / 10000f;
			zero.X = -36f;
			zero.Y = -12f;
			if (flag)
			{
				num += 1E-07f;
				int direction = 2;
				bool flag2 = sprite.Value.CurrentAnimation != null && sprite.Value.CurrentAnimation[sprite.Value.currentAnimationIndex].flip;
				switch (Sprite.CurrentFrame)
				{
				case 1:
					zero.Y -= 4f;
					direction = 2;
					break;
				case 3:
					zero.Y += 4f;
					direction = 2;
					break;
				case 4:
					direction = 1;
					break;
				case 5:
					zero.Y -= 4f;
					direction = 1;
					break;
				case 6:
					direction = 1;
					break;
				case 7:
					zero.Y += 4f;
					direction = 1;
					break;
				case 20:
					direction = 1;
					break;
				case 21:
					zero.Y += 4f;
					direction = ((!flag2) ? 1 : 3);
					zero.X += (flag2 ? 1 : (-1)) * 4;
					break;
				case 22:
					zero.Y += 8f;
					direction = ((!flag2) ? 1 : 3);
					zero.X += (flag2 ? 2 : (-2)) * 4;
					break;
				case 23:
					zero.Y += 12f;
					direction = ((!flag2) ? 1 : 3);
					zero.X += (flag2 ? 2 : (-2)) * 4;
					break;
				case 8:
					direction = 0;
					break;
				case 9:
					zero.Y -= 4f;
					direction = 0;
					break;
				case 10:
					direction = 0;
					break;
				case 11:
					zero.Y += 4f;
					direction = 0;
					break;
				case 12:
					direction = 3;
					break;
				case 13:
					zero.Y += 4f;
					direction = 3;
					break;
				case 14:
					direction = 3;
					break;
				case 15:
					zero.Y -= 4f;
					direction = 3;
					break;
				case 18:
				case 19:
					zero.Y += 4f;
					direction = 2;
					break;
				}
				if (shakeTimer > 0)
				{
					zero += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
				}
				hat.Value.draw(b, getLocalPosition(Game1.viewport) + zero + new Vector2(30f, -42f), 1.3333334f, 1f, num, direction);
			}
		}

		public override void behaviorOnLocalFarmerLocationEntry(GameLocation location)
		{
			reloadSprite();
		}
	}
}
