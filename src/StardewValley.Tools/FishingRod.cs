using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

namespace StardewValley.Tools
{
	public class FishingRod : Tool
	{
		public const int sizeOfLandCheckRectangle = 11;

		[XmlElement("bobber")]
		public readonly NetPosition bobber = new NetPosition();

		public static int minFishingBiteTime = 600;

		public static int maxFishingBiteTime = 30000;

		public static int minTimeToNibble = 340;

		public static int maxTimeToNibble = 800;

		public static int maxTackleUses = 20;

		protected Vector2 _lastAppliedMotion = Vector2.Zero;

		protected Vector2[] _totalMotionBuffer = new Vector2[4];

		protected int _totalMotionBufferIndex;

		protected NetVector2 _totalMotion = new NetVector2(Vector2.Zero);

		public static double baseChanceForTreasure = 0.15;

		private int bobberBob;

		[XmlIgnore]
		public float bobberTimeAccumulator;

		[XmlIgnore]
		public float timePerBobberBob = 2000f;

		[XmlIgnore]
		public float timeUntilFishingBite = -1f;

		[XmlIgnore]
		public float fishingBiteAccumulator;

		[XmlIgnore]
		public float fishingNibbleAccumulator;

		[XmlIgnore]
		public float timeUntilFishingNibbleDone = -1f;

		[XmlIgnore]
		public float castingPower;

		[XmlIgnore]
		public float castingChosenCountdown;

		[XmlIgnore]
		public float castingTimerSpeed = 0.001f;

		[XmlIgnore]
		public float fishWiggle;

		[XmlIgnore]
		public float fishWiggleIntensity;

		[XmlIgnore]
		public bool isFishing;

		[XmlIgnore]
		public bool hit;

		[XmlIgnore]
		public bool isNibbling;

		[XmlIgnore]
		public bool favBait;

		[XmlIgnore]
		public bool isTimingCast;

		[XmlIgnore]
		public bool isCasting;

		[XmlIgnore]
		public bool castedButBobberStillInAir;

		[XmlIgnore]
		protected bool _hasPlayerAdjustedBobber;

		private bool lastCatchWasJunk;

		[XmlIgnore]
		public bool doneWithAnimation;

		[XmlIgnore]
		public bool pullingOutOfWater;

		[XmlIgnore]
		public bool isReeling;

		[XmlIgnore]
		public bool hasDoneFucntionYet;

		[XmlIgnore]
		public bool fishCaught;

		[XmlIgnore]
		public bool recordSize;

		[XmlIgnore]
		public bool treasureCaught;

		[XmlIgnore]
		public bool showingTreasure;

		[XmlIgnore]
		public bool hadBobber;

		[XmlIgnore]
		public bool bossFish;

		[XmlIgnore]
		public bool fromFishPond;

		[XmlIgnore]
		public bool caughtDoubleFish;

		[XmlIgnore]
		public List<TemporaryAnimatedSprite> animations = new List<TemporaryAnimatedSprite>();

		[XmlIgnore]
		public SparklingText sparklingText;

		[XmlIgnore]
		private int fishSize;

		[XmlIgnore]
		private int whichFish;

		[XmlIgnore]
		private int fishQuality;

		[XmlIgnore]
		private int clearWaterDistance;

		[XmlIgnore]
		private int originalFacingDirection;

		[XmlIgnore]
		private string itemCategory;

		[XmlIgnore]
		private int recastTimerMs;

		protected const int RECAST_DELAY_MS = 200;

		[XmlIgnore]
		private readonly NetEventBinary pullFishFromWaterEvent = new NetEventBinary();

		[XmlIgnore]
		private readonly NetEvent1Field<bool, NetBool> doneFishingEvent = new NetEvent1Field<bool, NetBool>();

		[XmlIgnore]
		private readonly NetEvent0 startCastingEvent = new NetEvent0();

		[XmlIgnore]
		private readonly NetEvent0 castingEndEnableMovementEvent = new NetEvent0();

		[XmlIgnore]
		private readonly NetEvent0 putAwayEvent = new NetEvent0();

		[XmlIgnore]
		private readonly NetEvent0 beginReelingEvent = new NetEvent0();

		public static ICue chargeSound;

		public static ICue reelSound;

		private bool usedGamePadToCast;

		public override string DisplayName => (int)upgradeLevel switch
		{
			0 => Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14045"), 
			1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14046"), 
			2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14047"), 
			3 => Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14048"), 
			_ => displayName, 
		};

		public override string Name
		{
			get
			{
				return (int)upgradeLevel switch
				{
					0 => "Bamboo Pole", 
					1 => "Training Rod", 
					2 => "Fiberglass Rod", 
					3 => "Iridium Rod", 
					_ => base.BaseName, 
				};
			}
			set
			{
				base.BaseName = value;
			}
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(bobber.NetFields, pullFishFromWaterEvent, doneFishingEvent, startCastingEvent, castingEndEnableMovementEvent, putAwayEvent, _totalMotion, beginReelingEvent);
			_totalMotion.InterpolationEnabled = false;
			_totalMotion.InterpolationWait = false;
			pullFishFromWaterEvent.AddReaderHandler(doPullFishFromWater);
			doneFishingEvent.onEvent += doDoneFishing;
			startCastingEvent.onEvent += doStartCasting;
			castingEndEnableMovementEvent.onEvent += doCastingEndEnableMovement;
			beginReelingEvent.onEvent += beginReeling;
			putAwayEvent.onEvent += resetState;
		}

		public override void actionWhenStopBeingHeld(Farmer who)
		{
			putAwayEvent.Fire();
			base.actionWhenStopBeingHeld(who);
		}

		public FishingRod()
			: base("Fishing Rod", 0, 189, 8, stackable: false)
		{
			numAttachmentSlots.Value = 2;
			attachments.SetCount(numAttachmentSlots);
			base.IndexOfMenuItemView = 8 + (int)upgradeLevel;
		}

		public override void resetState()
		{
			isNibbling = false;
			fishCaught = false;
			isFishing = false;
			isReeling = false;
			isCasting = false;
			isTimingCast = false;
			doneWithAnimation = false;
			pullingOutOfWater = false;
			fromFishPond = false;
			caughtDoubleFish = false;
			fishingBiteAccumulator = 0f;
			showingTreasure = false;
			fishingNibbleAccumulator = 0f;
			timeUntilFishingBite = -1f;
			timeUntilFishingNibbleDone = -1f;
			bobberTimeAccumulator = 0f;
			castingChosenCountdown = 0f;
			_totalMotionBufferIndex = 0;
			for (int i = 0; i < _totalMotionBuffer.Length; i++)
			{
				_totalMotionBuffer[i] = Vector2.Zero;
			}
			if (lastUser != null && Game1.player == lastUser)
			{
				for (int num = Game1.screenOverlayTempSprites.Count - 1; num >= 0; num--)
				{
					if (Game1.screenOverlayTempSprites[num].id == 987654340f)
					{
						Game1.screenOverlayTempSprites.RemoveAt(num);
					}
				}
			}
			_totalMotion.Value = Vector2.Zero;
			_lastAppliedMotion = Vector2.Zero;
			pullFishFromWaterEvent.Clear();
			doneFishingEvent.Clear();
			startCastingEvent.Clear();
			castingEndEnableMovementEvent.Clear();
			beginReelingEvent.Clear();
			bobber.Set(Vector2.Zero);
		}

		public override Item getOne()
		{
			FishingRod fishingRod = new FishingRod();
			fishingRod.UpgradeLevel = base.UpgradeLevel;
			fishingRod.numAttachmentSlots.Value = numAttachmentSlots.Value;
			fishingRod.IndexOfMenuItemView = base.IndexOfMenuItemView;
			CopyEnchantments(this, fishingRod);
			fishingRod._GetOneFrom(this);
			return fishingRod;
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14041");
		}

		protected override string loadDescription()
		{
			if ((int)upgradeLevel != 1)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14042");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.trainingRodDescription");
		}

		public override int salePrice()
		{
			return (int)upgradeLevel switch
			{
				0 => 500, 
				1 => 2000, 
				2 => 5000, 
				3 => 15000, 
				_ => 500, 
			};
		}

		public override int attachmentSlots()
		{
			if ((int)upgradeLevel <= 2)
			{
				if ((int)upgradeLevel <= 1)
				{
					return 0;
				}
				return 1;
			}
			return 2;
		}

		public FishingRod(int upgradeLevel)
			: base("Fishing Rod", upgradeLevel, 189, 8, stackable: false)
		{
			numAttachmentSlots.Value = 2;
			attachments.SetCount(numAttachmentSlots);
			base.IndexOfMenuItemView = 8 + upgradeLevel;
			base.UpgradeLevel = upgradeLevel;
		}

		internal static int getAddedDistance(Farmer who)
		{
			if (who.FishingLevel >= 15)
			{
				return 4;
			}
			if (who.FishingLevel >= 8)
			{
				return 3;
			}
			if (who.FishingLevel >= 4)
			{
				return 2;
			}
			if (who.FishingLevel >= 1)
			{
				return 1;
			}
			return 0;
		}

		private Vector2 calculateBobberTile()
		{
			Vector2 result = bobber;
			result.X = bobber.X / 64f;
			result.Y = bobber.Y / 64f;
			return result;
		}

		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			if (fishCaught || (!who.IsLocalPlayer && (isReeling || isFishing || pullingOutOfWater)))
			{
				return;
			}
			hasDoneFucntionYet = true;
			Vector2 bobberTile = calculateBobberTile();
			int tileX = (int)bobberTile.X;
			int tileY = (int)bobberTile.Y;
			base.DoFunction(location, x, y, power, who);
			if (doneWithAnimation)
			{
				who.canReleaseTool = true;
			}
			if (Game1.isAnyGamePadButtonBeingPressed())
			{
				Game1.lastCursorMotionWasMouse = false;
			}
			if (!isFishing && !castedButBobberStillInAir && !pullingOutOfWater && !isNibbling && !hit && !showingTreasure)
			{
				if (!Game1.eventUp && who.IsLocalPlayer && !hasEnchantmentOfType<EfficientToolEnchantment>())
				{
					float stamina = who.Stamina;
					who.Stamina -= 8f - (float)who.FishingLevel * 0.1f;
					who.checkForExhaustion(stamina);
				}
				if (location.canFishHere() && location.isTileFishable(tileX, tileY))
				{
					clearWaterDistance = distanceToLand((int)(bobber.X / 64f), (int)(bobber.Y / 64f), lastUser.currentLocation);
					isFishing = true;
					location.temporarySprites.Add(new TemporaryAnimatedSprite(28, 100f, 2, 1, new Vector2(bobber.X - 32f, bobber.Y - 32f), flicker: false, flipped: false));
					if (who.IsLocalPlayer)
					{
						location.playSound("dropItemInWater");
					}
					timeUntilFishingBite = calculateTimeUntilFishingBite(bobberTile, isFirstCast: true, who);
					Game1.stats.TimesFished++;
					float num = (bobber.X - 32f) / 64f;
					float num2 = (bobber.Y - 32f) / 64f;
					if (location.fishSplashPoint != null)
					{
						Rectangle value = new Rectangle(location.fishSplashPoint.X * 64, location.fishSplashPoint.Y * 64, 64, 64);
						if (new Rectangle((int)bobber.X - 32, (int)bobber.Y - 32, 64, 64).Intersects(value))
						{
							timeUntilFishingBite /= 4f;
							location.temporarySprites.Add(new TemporaryAnimatedSprite(10, bobber - new Vector2(32f, 32f), Color.Cyan));
						}
					}
					who.UsingTool = true;
					who.canMove = false;
				}
				else
				{
					if (doneWithAnimation)
					{
						who.UsingTool = false;
					}
					if (doneWithAnimation)
					{
						who.canMove = true;
					}
				}
			}
			else
			{
				if (isCasting || pullingOutOfWater)
				{
					return;
				}
				bool flag = location.isTileBuildingFishable((int)bobberTile.X, (int)bobberTile.Y);
				who.FarmerSprite.PauseForSingleAnimation = false;
				switch (who.FacingDirection)
				{
				case 0:
					who.FarmerSprite.animateBackwardsOnce(299, 35f);
					break;
				case 1:
					who.FarmerSprite.animateBackwardsOnce(300, 35f);
					break;
				case 2:
					who.FarmerSprite.animateBackwardsOnce(301, 35f);
					break;
				case 3:
					who.FarmerSprite.animateBackwardsOnce(302, 35f);
					break;
				}
				if (isNibbling)
				{
					double num3 = ((attachments[0] != null) ? ((float)attachments[0].Price / 10f) : 0f);
					bool flag2 = false;
					if (location.fishSplashPoint != null)
					{
						Rectangle rectangle = new Rectangle(location.fishSplashPoint.X * 64, location.fishSplashPoint.Y * 64, 64, 64);
						Rectangle value2 = new Rectangle((int)bobber.X - 80, (int)bobber.Y - 80, 64, 64);
						flag2 = rectangle.Intersects(value2);
					}
					Object @object = location.getFish(fishingNibbleAccumulator, (attachments[0] != null) ? attachments[0].ParentSheetIndex : (-1), clearWaterDistance + (flag2 ? 1 : 0), lastUser, num3 + (flag2 ? 0.4 : 0.0), bobberTile);
					if (@object == null || @object.ParentSheetIndex <= 0)
					{
						@object = new Object(Game1.random.Next(167, 173), 1);
					}
					if (@object.scale.X == 1f)
					{
						favBait = true;
					}
					Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
					bool flag3 = false;
					if (@object is Furniture)
					{
						flag3 = true;
					}
					else if (Utility.IsNormalObjectAtParentSheetIndex(@object, @object.ParentSheetIndex) && dictionary.ContainsKey(@object.ParentSheetIndex))
					{
						string[] array = dictionary[@object.ParentSheetIndex].Split('/');
						int result = -1;
						if (!int.TryParse(array[1], out result))
						{
							flag3 = true;
						}
					}
					else
					{
						flag3 = true;
					}
					lastCatchWasJunk = false;
					if (@object.Category == -20 || @object.ParentSheetIndex == 152 || @object.ParentSheetIndex == 153 || (int)@object.parentSheetIndex == 157 || (int)@object.parentSheetIndex == 797 || (int)@object.parentSheetIndex == 79 || (int)@object.parentSheetIndex == 73 || @object.ParentSheetIndex == 842 || (@object.ParentSheetIndex >= 820 && @object.ParentSheetIndex <= 828) || (int)@object.parentSheetIndex == GameLocation.CAROLINES_NECKLACE_ITEM || @object.ParentSheetIndex == 890 || flag || flag3)
					{
						lastCatchWasJunk = true;
						string text = "Object";
						if (@object is Furniture)
						{
							text = "Furniture";
						}
						pullFishFromWater(@object.ParentSheetIndex, -1, 0, 0, treasureCaught: false, wasPerfect: false, flag, caughtDouble: false, text);
					}
					else if (!hit && who.IsLocalPlayer)
					{
						hit = true;
						Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(612, 1913, 74, 30), 1500f, 1, 0, Game1.GlobalToLocal(Game1.viewport, bobber + new Vector2(-140f, -160f)), flicker: false, flipped: false, 1f, 0.005f, Color.White, 4f, 0.075f, 0f, 0f, local: true)
						{
							scaleChangeChange = -0.005f,
							motion = new Vector2(0f, -0.1f),
							endFunction = startMinigameEndFunction,
							extraInfoForEndBehavior = @object.ParentSheetIndex,
							id = 987654340f
						});
						location.localSound("FishHit");
					}
					return;
				}
				if (flag)
				{
					Object fish = location.getFish(-1f, -1, -1, lastUser, -1.0, bobberTile);
					if (fish != null)
					{
						pullFishFromWater(fish.ParentSheetIndex, -1, 0, 0, treasureCaught: false, wasPerfect: false, flag);
						return;
					}
				}
				if (who.IsLocalPlayer)
				{
					location.playSound("pullItemFromWater");
				}
				isFishing = false;
				pullingOutOfWater = true;
				if (lastUser.FacingDirection == 1 || lastUser.FacingDirection == 3)
				{
					float num4 = Math.Abs(bobber.X - (float)lastUser.getStandingX());
					float num5 = 0.005f;
					float num6 = 0f - (float)Math.Sqrt(num4 * num5 / 2f);
					float num7 = 2f * (Math.Abs(num6 - 0.5f) / num5);
					num7 *= 1.2f;
					animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(170, 1903, 7, 8), num7, 1, 0, bobber + new Vector2(-32f, -48f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f, 0f, Color.White, 4f, 0f, 0f, (float)Game1.random.Next(-20, 20) / 100f)
					{
						motion = new Vector2((float)((who.FacingDirection != 3) ? 1 : (-1)) * (num6 + 0.2f), num6 - 0.8f),
						acceleration = new Vector2(0f, num5),
						endFunction = donefishingEndFunction,
						timeBasedMotion = true,
						alphaFade = 0.001f
					});
				}
				else
				{
					float num8 = bobber.Y - (float)lastUser.getStandingY();
					float num9 = Math.Abs(num8 + 256f);
					float num10 = 0.005f;
					float num11 = (float)Math.Sqrt(2f * num10 * num9);
					float animationInterval = (float)(Math.Sqrt(2f * (num9 - num8) / num10) + (double)(num11 / num10));
					animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(170, 1903, 7, 8), animationInterval, 1, 0, bobber + new Vector2(-32f, -48f), flicker: false, flipped: false, bobber.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, (float)Game1.random.Next(-20, 20) / 100f)
					{
						motion = new Vector2(0f, 0f - num11),
						acceleration = new Vector2(0f, num10),
						endFunction = donefishingEndFunction,
						timeBasedMotion = true,
						alphaFade = 0.001f
					});
				}
				who.UsingTool = true;
				who.canReleaseTool = false;
			}
		}

		private float calculateTimeUntilFishingBite(Vector2 bobberTile, bool isFirstCast, Farmer who)
		{
			if (Game1.currentLocation.isTileBuildingFishable((int)bobberTile.X, (int)bobberTile.Y) && Game1.currentLocation is BuildableGameLocation)
			{
				Building buildingAt = (Game1.currentLocation as BuildableGameLocation).getBuildingAt(bobberTile);
				if (buildingAt != null && buildingAt is FishPond && (int)(buildingAt as FishPond).currentOccupants > 0)
				{
					return FishPond.FISHING_MILLISECONDS;
				}
			}
			float num = Game1.random.Next(minFishingBiteTime, maxFishingBiteTime - 250 * who.FishingLevel - ((attachments[1] != null && attachments[1].ParentSheetIndex == 686) ? 5000 : ((attachments[1] != null && attachments[1].ParentSheetIndex == 687) ? 10000 : 0)));
			if (isFirstCast)
			{
				num *= 0.75f;
			}
			if (attachments[0] != null)
			{
				num *= 0.5f;
				if ((int)attachments[0].parentSheetIndex == 774)
				{
					num *= 0.75f;
				}
			}
			return Math.Max(500f, num);
		}

		public Color getColor()
		{
			return (int)upgradeLevel switch
			{
				0 => Color.Goldenrod, 
				1 => Color.OliveDrab, 
				2 => Color.White, 
				3 => Color.Violet, 
				_ => Color.White, 
			};
		}

		public static int distanceToLand(int tileX, int tileY, GameLocation location)
		{
			Rectangle r = new Rectangle(tileX - 1, tileY - 1, 3, 3);
			bool flag = false;
			int num = 1;
			while (!flag && r.Width <= 11)
			{
				foreach (Vector2 item in Utility.getBorderOfThisRectangle(r))
				{
					if (location.isTileOnMap(item) && location.doesTileHaveProperty((int)item.X, (int)item.Y, "Water", "Back") == null)
					{
						flag = true;
						num = r.Width / 2;
						break;
					}
				}
				r.Inflate(1, 1);
			}
			if (r.Width > 11)
			{
				num = 6;
			}
			return num - 1;
		}

		public void startMinigameEndFunction(int extra)
		{
			beginReelingEvent.Fire();
			isReeling = true;
			hit = false;
			switch (lastUser.FacingDirection)
			{
			case 1:
				lastUser.FarmerSprite.setCurrentSingleFrame(48, 32000);
				break;
			case 3:
				lastUser.FarmerSprite.setCurrentSingleFrame(48, 32000, secondaryArm: false, flip: true);
				break;
			}
			float num = 1f;
			num *= (float)clearWaterDistance / 5f;
			int num2 = 1 + lastUser.FishingLevel / 2;
			num *= (float)Game1.random.Next(num2, Math.Max(6, num2)) / 5f;
			if (favBait)
			{
				num *= 1.2f;
			}
			num *= 1f + (float)Game1.random.Next(-10, 11) / 100f;
			num = Math.Max(0f, Math.Min(1f, num));
			bool treasure = !Game1.isFestival() && lastUser.fishCaught != null && lastUser.fishCaught.Count() > 1 && Game1.random.NextDouble() < baseChanceForTreasure + (double)lastUser.LuckLevel * 0.005 + ((getBaitAttachmentIndex() == 703) ? baseChanceForTreasure : 0.0) + ((getBobberAttachmentIndex() == 693) ? (baseChanceForTreasure / 3.0) : 0.0) + lastUser.DailyLuck / 2.0 + (lastUser.professions.Contains(9) ? baseChanceForTreasure : 0.0);
			Game1.activeClickableMenu = new BobberBar(extra, num, treasure, (attachments[1] != null) ? attachments[1].ParentSheetIndex : (-1));
		}

		public int getBobberAttachmentIndex()
		{
			if (attachments[1] == null)
			{
				return -1;
			}
			return attachments[1].ParentSheetIndex;
		}

		public int getBaitAttachmentIndex()
		{
			if (attachments[0] == null)
			{
				return -1;
			}
			return attachments[0].ParentSheetIndex;
		}

		public bool inUse()
		{
			if (!isFishing && !isCasting && !isTimingCast && !isNibbling && !isReeling)
			{
				return fishCaught;
			}
			return true;
		}

		public void donefishingEndFunction(int extra)
		{
			isFishing = false;
			isReeling = false;
			lastUser.canReleaseTool = true;
			lastUser.canMove = true;
			lastUser.UsingTool = false;
			lastUser.FarmerSprite.PauseForSingleAnimation = false;
			pullingOutOfWater = false;
			doneFishing(lastUser);
		}

		public static void endOfAnimationBehavior(Farmer f)
		{
		}

		public override Object attach(Object o)
		{
			if (o != null && o.Category == -21 && (int)upgradeLevel > 1)
			{
				Object @object = attachments[0];
				if (@object != null && @object.canStackWith(o))
				{
					@object.Stack = o.addToStack(@object);
					if (@object.Stack <= 0)
					{
						@object = null;
					}
				}
				attachments[0] = o;
				Game1.playSound("button1");
				return @object;
			}
			if (o != null && o.Category == -22 && (int)upgradeLevel > 2)
			{
				Object result = attachments[1];
				attachments[1] = o;
				Game1.playSound("button1");
				return result;
			}
			if (o == null)
			{
				if (attachments[0] != null)
				{
					Object result2 = attachments[0];
					attachments[0] = null;
					Game1.playSound("dwop");
					return result2;
				}
				if (attachments[1] != null)
				{
					Object result3 = attachments[1];
					attachments[1] = null;
					Game1.playSound("dwop");
					return result3;
				}
			}
			return null;
		}

		public override void drawAttachments(SpriteBatch b, int x, int y)
		{
			y += ((enchantments.Count() > 0) ? 8 : 4);
			if ((int)upgradeLevel > 1)
			{
				if (attachments[0] == null)
				{
					b.Draw(Game1.menuTexture, new Vector2(x, y), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 36), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
				}
				else
				{
					b.Draw(Game1.menuTexture, new Vector2(x, y), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
					attachments[0].drawInMenu(b, new Vector2(x, y), 1f);
				}
			}
			if ((int)upgradeLevel > 2)
			{
				if (attachments[1] == null)
				{
					b.Draw(Game1.menuTexture, new Vector2(x, y + 64 + 4), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 37), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
					return;
				}
				b.Draw(Game1.menuTexture, new Vector2(x, y + 64 + 4), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
				attachments[1].drawInMenu(b, new Vector2(x, y + 64 + 4), 1f);
			}
		}

		public override bool canThisBeAttached(Object o)
		{
			if (o != null)
			{
				if (o.Category != -21 || (int)upgradeLevel <= 1)
				{
					if (o.Category == -22)
					{
						return (int)upgradeLevel > 2;
					}
					return false;
				}
				return true;
			}
			return true;
		}

		public void playerCaughtFishEndFunction(int extraData)
		{
			lastUser.Halt();
			lastUser.armOffset = Vector2.Zero;
			castedButBobberStillInAir = false;
			fishCaught = true;
			isReeling = false;
			isFishing = false;
			pullingOutOfWater = false;
			lastUser.canReleaseTool = false;
			if (lastUser.IsLocalPlayer)
			{
				if (!Game1.isFestival())
				{
					recordSize = lastUser.caughtFish(whichFish, fishSize, fromFishPond, (!caughtDoubleFish) ? 1 : 2);
					lastUser.faceDirection(2);
				}
				else
				{
					Game1.currentLocation.currentEvent.caughtFish(whichFish, fishSize, lastUser);
					fishCaught = false;
					doneFishing(lastUser);
				}
				if (isFishBossFish(whichFish))
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14068"));
					string text = Game1.objectInformation[whichFish].Split('/')[4];
					Game1.multiplayer.globalChatInfoMessage("CaughtLegendaryFish", Game1.player.Name, text);
				}
				else if (recordSize)
				{
					sparklingText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14069"), Color.LimeGreen, Color.Azure);
					lastUser.currentLocation.localSound("newRecord");
				}
				else
				{
					lastUser.currentLocation.localSound("fishSlap");
				}
			}
		}

		public static bool isFishBossFish(int index)
		{
			switch (index)
			{
			case 159:
			case 160:
			case 163:
			case 682:
			case 775:
				return true;
			default:
				return false;
			}
		}

		public void pullFishFromWater(int whichFish, int fishSize, int fishQuality, int fishDifficulty, bool treasureCaught, bool wasPerfect, bool fromFishPond, bool caughtDouble = false, string itemCategory = "Object")
		{
			pullFishFromWaterEvent.Fire(delegate(BinaryWriter writer)
			{
				writer.Write(whichFish);
				writer.Write(fishSize);
				writer.Write(fishQuality);
				writer.Write(fishDifficulty);
				writer.Write(treasureCaught);
				writer.Write(wasPerfect);
				writer.Write(fromFishPond);
				writer.Write(caughtDouble);
				writer.Write(itemCategory);
			});
		}

		private void doPullFishFromWater(BinaryReader argReader)
		{
			int num = argReader.ReadInt32();
			int num2 = argReader.ReadInt32();
			int num3 = argReader.ReadInt32();
			int num4 = argReader.ReadInt32();
			bool flag = argReader.ReadBoolean();
			bool flag2 = argReader.ReadBoolean();
			bool flag3 = argReader.ReadBoolean();
			bool flag4 = argReader.ReadBoolean();
			string text = argReader.ReadString();
			treasureCaught = flag;
			fishSize = num2;
			fishQuality = num3;
			whichFish = num;
			fromFishPond = flag3;
			caughtDoubleFish = flag4;
			itemCategory = text;
			if (num3 >= 2 && flag2)
			{
				fishQuality = 4;
			}
			else if (num3 >= 1 && flag2)
			{
				fishQuality = 2;
			}
			if (lastUser == null)
			{
				return;
			}
			if (!Game1.isFestival() && lastUser.IsLocalPlayer && !flag3 && text == "Object")
			{
				bossFish = isFishBossFish(num);
				int num5 = Math.Max(1, (num3 + 1) * 3 + num4 / 3);
				if (flag)
				{
					num5 += (int)((float)num5 * 1.2f);
				}
				if (flag2)
				{
					num5 += (int)((float)num5 * 1.4f);
				}
				if (bossFish)
				{
					num5 *= 5;
				}
				lastUser.gainExperience(1, num5);
			}
			if (fishQuality < 0)
			{
				fishQuality = 0;
			}
			string text2 = "";
			Rectangle rectangle = default(Rectangle);
			if (text == "Object")
			{
				text2 = "Maps\\springobjects";
				rectangle = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, num, 16, 16);
			}
			else
			{
				text2 = "LooseSprites\\Cursors";
				rectangle = new Rectangle(228, 408, 16, 16);
			}
			float num6 = 35f;
			if (lastUser.FacingDirection == 1 || lastUser.FacingDirection == 3)
			{
				float num7 = Vector2.Distance(bobber, lastUser.Position);
				float num8 = 0.001f;
				float num9 = 128f - (lastUser.Position.Y - bobber.Y + 10f);
				double a = 1.1423973285781066;
				float num10 = (float)((double)(num7 * num8) * Math.Tan(a) / Math.Sqrt((double)(2f * num7 * num8) * Math.Tan(a) - (double)(2f * num8 * num9)));
				if (float.IsNaN(num10))
				{
					num10 = 0.6f;
				}
				float num11 = (float)((double)num10 * (1.0 / Math.Tan(a)));
				num6 = num7 / num11;
				animations.Add(new TemporaryAnimatedSprite(text2, rectangle, num6, 1, 0, bobber, flicker: false, flipped: false, bobber.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2((float)((lastUser.FacingDirection != 3) ? 1 : (-1)) * (0f - num11), 0f - num10),
					acceleration = new Vector2(0f, num8),
					timeBasedMotion = true,
					endFunction = playerCaughtFishEndFunction,
					extraInfoForEndBehavior = num,
					endSound = "tinyWhip"
				});
				if (caughtDoubleFish)
				{
					num7 = Vector2.Distance(bobber, lastUser.Position);
					num8 = 0.0008f;
					num9 = 128f - (lastUser.Position.Y - bobber.Y + 10f);
					a = 1.1423973285781066;
					num10 = (float)((double)(num7 * num8) * Math.Tan(a) / Math.Sqrt((double)(2f * num7 * num8) * Math.Tan(a) - (double)(2f * num8 * num9)));
					if (float.IsNaN(num10))
					{
						num10 = 0.6f;
					}
					num11 = (float)((double)num10 * (1.0 / Math.Tan(a)));
					num6 = num7 / num11;
					animations.Add(new TemporaryAnimatedSprite(text2, rectangle, num6, 1, 0, bobber, flicker: false, flipped: false, bobber.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2((float)((lastUser.FacingDirection != 3) ? 1 : (-1)) * (0f - num11), 0f - num10),
						acceleration = new Vector2(0f, num8),
						timeBasedMotion = true,
						endSound = "fishSlap",
						Parent = lastUser.currentLocation
					});
				}
			}
			else
			{
				float num12 = bobber.Y - (float)(lastUser.getStandingY() - 64);
				float num13 = Math.Abs(num12 + 256f + 32f);
				if (lastUser.FacingDirection == 0)
				{
					num13 += 96f;
				}
				float num14 = 0.003f;
				float num15 = (float)Math.Sqrt(2f * num14 * num13);
				num6 = (float)(Math.Sqrt(2f * (num13 - num12) / num14) + (double)(num15 / num14));
				float x = 0f;
				if (num6 != 0f)
				{
					x = (lastUser.Position.X - bobber.X) / num6;
				}
				animations.Add(new TemporaryAnimatedSprite(text2, rectangle, num6, 1, 0, new Vector2(bobber.X, bobber.Y), flicker: false, flipped: false, bobber.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(x, 0f - num15),
					acceleration = new Vector2(0f, num14),
					timeBasedMotion = true,
					endFunction = playerCaughtFishEndFunction,
					extraInfoForEndBehavior = num,
					endSound = "tinyWhip"
				});
				if (caughtDoubleFish)
				{
					num12 = bobber.Y - (float)(lastUser.getStandingY() - 64);
					num13 = Math.Abs(num12 + 256f + 32f);
					if (lastUser.FacingDirection == 0)
					{
						num13 += 96f;
					}
					num14 = 0.004f;
					num15 = (float)Math.Sqrt(2f * num14 * num13);
					num6 = (float)(Math.Sqrt(2f * (num13 - num12) / num14) + (double)(num15 / num14));
					x = 0f;
					if (num6 != 0f)
					{
						x = (lastUser.Position.X - bobber.X) / num6;
					}
					animations.Add(new TemporaryAnimatedSprite(text2, rectangle, num6, 1, 0, new Vector2(bobber.X, bobber.Y), flicker: false, flipped: false, bobber.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(x, 0f - num15),
						acceleration = new Vector2(0f, num14),
						timeBasedMotion = true,
						endSound = "fishSlap",
						Parent = lastUser.currentLocation
					});
				}
			}
			if (lastUser.IsLocalPlayer)
			{
				lastUser.currentLocation.playSound("pullItemFromWater");
				lastUser.currentLocation.playSound("dwop");
			}
			castedButBobberStillInAir = false;
			pullingOutOfWater = true;
			isFishing = false;
			isReeling = false;
			lastUser.FarmerSprite.PauseForSingleAnimation = false;
			switch (lastUser.FacingDirection)
			{
			case 0:
				lastUser.FarmerSprite.animateBackwardsOnce(299, num6);
				break;
			case 1:
				lastUser.FarmerSprite.animateBackwardsOnce(300, num6);
				break;
			case 2:
				lastUser.FarmerSprite.animateBackwardsOnce(301, num6);
				break;
			case 3:
				lastUser.FarmerSprite.animateBackwardsOnce(302, num6);
				break;
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			float num = 4f;
			if (!bobber.Equals(Vector2.Zero) && isFishing)
			{
				Vector2 globalPosition = bobber;
				if (bobberTimeAccumulator > timePerBobberBob)
				{
					if ((!isNibbling && !isReeling) || Game1.random.NextDouble() < 0.05)
					{
						lastUser.currentLocation.localSound("waterSlosh");
						lastUser.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 150f, 8, 0, new Vector2(bobber.X - 32f, bobber.Y - 32f), flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f));
					}
					timePerBobberBob = ((bobberBob == 0) ? Game1.random.Next(1500, 3500) : Game1.random.Next(350, 750));
					bobberTimeAccumulator = 0f;
					if (isNibbling || isReeling)
					{
						timePerBobberBob = Game1.random.Next(25, 75);
						globalPosition.X += Game1.random.Next(-5, 5);
						globalPosition.Y += Game1.random.Next(-5, 5);
						if (!isReeling)
						{
							num += (float)Game1.random.Next(-20, 20) / 100f;
						}
					}
					else if (Game1.random.NextDouble() < 0.1)
					{
						lastUser.currentLocation.localSound("bob");
					}
				}
				float layerDepth = globalPosition.Y / 10000f;
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, globalPosition), new Rectangle(179 + bobberBob * 9, 1903, 9, 9), Color.White, 0f, new Vector2(4f, 4f), num, SpriteEffects.None, layerDepth);
			}
			else if ((isTimingCast || castingChosenCountdown > 0f) && lastUser.IsLocalPlayer)
			{
				int num2 = (int)((0f - Math.Abs(castingChosenCountdown / 2f - castingChosenCountdown)) / 50f);
				float num3 = ((castingChosenCountdown > 0f && castingChosenCountdown < 100f) ? (castingChosenCountdown / 100f) : 1f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, getLastFarmerToUse().Position + new Vector2(-48f, -160 + num2)), new Rectangle(193, 1868, 47, 12), Color.White * num3, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.885f);
				b.Draw(Game1.staminaRect, new Rectangle((int)Game1.GlobalToLocal(Game1.viewport, getLastFarmerToUse().Position).X - 32 - 4, (int)Game1.GlobalToLocal(Game1.viewport, getLastFarmerToUse().Position).Y + num2 - 128 - 32 + 12, (int)(164f * castingPower), 25), Game1.staminaRect.Bounds, Utility.getRedToGreenLerpColor(castingPower) * num3, 0f, Vector2.Zero, SpriteEffects.None, 0.887f);
			}
			for (int num4 = animations.Count - 1; num4 >= 0; num4--)
			{
				animations[num4].draw(b);
			}
			if (sparklingText != null && !fishCaught)
			{
				sparklingText.draw(b, Game1.GlobalToLocal(Game1.viewport, getLastFarmerToUse().Position + new Vector2(-24f, -192f)));
			}
			else if (sparklingText != null && fishCaught)
			{
				sparklingText.draw(b, Game1.GlobalToLocal(Game1.viewport, getLastFarmerToUse().Position + new Vector2(-64f, -352f)));
			}
			if (!bobber.Value.Equals(Vector2.Zero) && (isFishing || pullingOutOfWater || castedButBobberStillInAir) && lastUser.FarmerSprite.CurrentFrame != 57 && (lastUser.FacingDirection != 0 || !pullingOutOfWater || whichFish == -1))
			{
				Vector2 vector = (isFishing ? ((Vector2)bobber) : ((animations.Count > 0) ? (animations[0].position + new Vector2(0f, 4f * num)) : Vector2.Zero));
				if (whichFish != -1)
				{
					vector += new Vector2(32f, 32f);
				}
				Vector2 vector2 = Vector2.Zero;
				if (castedButBobberStillInAir)
				{
					switch (lastUser.FacingDirection)
					{
					case 2:
						vector2 = lastUser.FarmerSprite.currentAnimationIndex switch
						{
							0 => Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(8f, lastUser.armOffset.Y - 96f + 4f)), 
							1 => Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(22f, lastUser.armOffset.Y - 96f + 4f)), 
							2 => Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(28f, lastUser.armOffset.Y - 64f + 40f)), 
							3 => Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(28f, lastUser.armOffset.Y - 8f)), 
							4 => Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(28f, lastUser.armOffset.Y + 32f)), 
							5 => Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(28f, lastUser.armOffset.Y + 32f)), 
							_ => Vector2.Zero, 
						};
						break;
					case 0:
						vector2 = lastUser.FarmerSprite.currentAnimationIndex switch
						{
							0 => Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(22f, lastUser.armOffset.Y - 96f + 4f)), 
							1 => Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(32f, lastUser.armOffset.Y - 96f + 4f)), 
							2 => Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(36f, lastUser.armOffset.Y - 64f + 40f)), 
							3 => Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(36f, lastUser.armOffset.Y - 16f)), 
							4 => Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(36f, lastUser.armOffset.Y - 32f)), 
							5 => Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(36f, lastUser.armOffset.Y - 32f)), 
							_ => Vector2.Zero, 
						};
						break;
					case 1:
						vector2 = lastUser.FarmerSprite.currentAnimationIndex switch
						{
							0 => Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(-48f, lastUser.armOffset.Y - 96f - 8f)), 
							1 => Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(-16f, lastUser.armOffset.Y - 96f - 20f)), 
							2 => Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(84f, lastUser.armOffset.Y - 96f - 20f)), 
							3 => Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(112f, lastUser.armOffset.Y - 32f - 20f)), 
							4 => Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(120f, lastUser.armOffset.Y - 32f + 8f)), 
							5 => Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(120f, lastUser.armOffset.Y - 32f + 8f)), 
							_ => Vector2.Zero, 
						};
						break;
					case 3:
						switch (lastUser.FarmerSprite.currentAnimationIndex)
						{
						case 0:
							vector2 = Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(112f, lastUser.armOffset.Y - 96f - 8f));
							break;
						case 1:
							vector2 = Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(80f, lastUser.armOffset.Y - 96f - 20f));
							break;
						case 2:
							vector2 = Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(-20f, lastUser.armOffset.Y - 96f - 20f));
							break;
						case 3:
							vector2 = Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(-48f, lastUser.armOffset.Y - 32f - 20f));
							break;
						case 4:
							vector2 = Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(-56f, lastUser.armOffset.Y - 32f + 8f));
							break;
						case 5:
							vector2 = Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(-56f, lastUser.armOffset.Y - 32f + 8f));
							break;
						}
						break;
					default:
						vector2 = Vector2.Zero;
						break;
					}
				}
				else if (!isReeling)
				{
					vector2 = lastUser.FacingDirection switch
					{
						0 => pullingOutOfWater ? Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(22f, lastUser.armOffset.Y - 96f + 4f)) : Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(28f, lastUser.armOffset.Y - 64f - 12f)), 
						2 => pullingOutOfWater ? Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(8f, lastUser.armOffset.Y - 96f + 4f)) : Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(28f, lastUser.armOffset.Y + 64f - 12f)), 
						1 => pullingOutOfWater ? Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(-48f, lastUser.armOffset.Y - 96f - 8f)) : Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(120f, lastUser.armOffset.Y - 64f + 16f)), 
						3 => pullingOutOfWater ? Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(112f, lastUser.armOffset.Y - 96f - 8f)) : Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(-56f, lastUser.armOffset.Y - 64f + 16f)), 
						_ => Vector2.Zero, 
					};
				}
				else if (lastUser != null && lastUser.IsLocalPlayer && Game1.didPlayerJustClickAtAll())
				{
					switch (lastUser.FacingDirection)
					{
					case 0:
						vector2 = Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(24f, lastUser.armOffset.Y - 96f + 12f));
						break;
					case 3:
						vector2 = Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(48f, lastUser.armOffset.Y - 96f - 12f));
						break;
					case 2:
						vector2 = Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(12f, lastUser.armOffset.Y - 96f + 8f));
						break;
					case 1:
						vector2 = Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(20f, lastUser.armOffset.Y - 96f - 12f));
						break;
					}
				}
				else
				{
					switch (lastUser.FacingDirection)
					{
					case 2:
						vector2 = Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(12f, lastUser.armOffset.Y - 96f + 4f));
						break;
					case 0:
						vector2 = Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(25f, lastUser.armOffset.Y - 96f + 4f));
						break;
					case 3:
						vector2 = Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(36f, lastUser.armOffset.Y - 96f - 8f));
						break;
					case 1:
						vector2 = Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(28f, lastUser.armOffset.Y - 96f - 8f));
						break;
					}
				}
				Vector2 vector3 = Game1.GlobalToLocal(Game1.viewport, vector + new Vector2(0f, -2f * num + (float)((bobberBob == 1) ? 4 : 0)));
				if (isTimingCast || (isCasting && !lastUser.IsLocalPlayer))
				{
					return;
				}
				if (isReeling)
				{
					Utility.drawLineWithScreenCoordinates((int)vector2.X, (int)vector2.Y, (int)vector3.X, (int)vector3.Y, b, Color.White * 0.5f);
					return;
				}
				Vector2 p = vector2;
				Vector2 p2 = new Vector2(vector2.X + (vector3.X - vector2.X) / 3f, vector2.Y + (vector3.Y - vector2.Y) * 2f / 3f);
				Vector2 p3 = new Vector2(vector2.X + (vector3.X - vector2.X) * 2f / 3f, vector2.Y + (vector3.Y - vector2.Y) * (float)(isFishing ? 6 : 2) / 5f);
				Vector2 p4 = vector3;
				for (float num5 = 0f; num5 < 1f; num5 += 0.025f)
				{
					Vector2 curvePoint = Utility.GetCurvePoint(num5, p, p2, p3, p4);
					Utility.drawLineWithScreenCoordinates((int)vector2.X, (int)vector2.Y, (int)curvePoint.X, (int)curvePoint.Y, b, Color.White * 0.5f, (float)lastUser.getStandingY() / 10000f + ((lastUser.FacingDirection != 0) ? 0.005f : (-0.001f)));
					vector2 = curvePoint;
				}
			}
			else
			{
				if (!fishCaught)
				{
					return;
				}
				float num6 = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(-120f, -288f + num6)), new Rectangle(31, 1870, 73, 49), Color.White * 0.8f, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)lastUser.getStandingY() / 10000f + 0.06f);
				if (itemCategory == "Object")
				{
					b.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(-124f, -284f + num6) + new Vector2(44f, 68f)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, whichFish, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)lastUser.getStandingY() / 10000f + 0.0001f + 0.06f);
					if (caughtDoubleFish)
					{
						Utility.drawTinyDigits(2, b, Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(-120f, -284f + num6) + new Vector2(23f, 29f) * 4f), 3f, (float)lastUser.getStandingY() / 10000f + 0.0001f + 0.061f, Color.White);
					}
					b.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(0f, -56f)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, whichFish, 16, 16), Color.White, (fishSize == -1 || whichFish == 800 || whichFish == 798 || whichFish == 149 || whichFish == 151) ? 0f : ((float)Math.PI * 3f / 4f), new Vector2(8f, 8f), 3f, SpriteEffects.None, (float)lastUser.getStandingY() / 10000f + 0.002f + 0.06f);
					if (caughtDoubleFish)
					{
						b.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(-8f, -56f)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, whichFish, 16, 16), Color.White, (fishSize == -1 || whichFish == 800 || whichFish == 798 || whichFish == 149 || whichFish == 151) ? 0f : ((float)Math.PI * 4f / 5f), new Vector2(8f, 8f), 3f, SpriteEffects.None, (float)lastUser.getStandingY() / 10000f + 0.002f + 0.058f);
					}
				}
				else
				{
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(-124f, -284f + num6) + new Vector2(44f, 68f)), new Rectangle(228, 408, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)lastUser.getStandingY() / 10000f + 0.0001f + 0.06f);
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(0f, -56f)), new Rectangle(228, 408, 16, 16), Color.White, 0f, new Vector2(8f, 8f), 3f, SpriteEffects.None, (float)lastUser.getStandingY() / 10000f + 0.002f + 0.06f);
				}
				string text = "???";
				if (itemCategory == "Object")
				{
					text = Game1.objectInformation[whichFish].Split('/')[4];
				}
				b.DrawString(Game1.smallFont, text, Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(26f - Game1.smallFont.MeasureString(text).X / 2f, -278f + num6)), bossFish ? new Color(126, 61, 237) : Game1.textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)lastUser.getStandingY() / 10000f + 0.002f + 0.06f);
				if (fishSize != -1)
				{
					b.DrawString(Game1.smallFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14082"), Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(20f, -214f + num6)), Game1.textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)lastUser.getStandingY() / 10000f + 0.002f + 0.06f);
					b.DrawString(Game1.smallFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14083", (LocalizedContentManager.CurrentLanguageCode != 0) ? Math.Round((double)fishSize * 2.54) : ((double)fishSize)), Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(85f - Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14083", (LocalizedContentManager.CurrentLanguageCode != 0) ? Math.Round((double)fishSize * 2.54) : ((double)fishSize))).X / 2f, -179f + num6)), recordSize ? (Color.Blue * Math.Min(1f, num6 / 8f + 1.5f)) : Game1.textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)lastUser.getStandingY() / 10000f + 0.002f + 0.06f);
				}
			}
		}

		public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			if (who.Stamina <= 1f && who.IsLocalPlayer)
			{
				if (!who.isEmoting)
				{
					who.doEmote(36);
				}
				who.CanMove = !Game1.eventUp;
				who.UsingTool = false;
				who.canReleaseTool = false;
				doneFishing(null);
				return true;
			}
			usedGamePadToCast = false;
			if (Game1.input.GetGamePadState().IsButtonDown(Buttons.X))
			{
				usedGamePadToCast = true;
			}
			bossFish = false;
			originalFacingDirection = who.FacingDirection;
			who.Halt();
			treasureCaught = false;
			showingTreasure = false;
			isFishing = false;
			hit = false;
			favBait = false;
			if (attachments != null && attachments.Length > 1 && attachments[1] != null)
			{
				hadBobber = true;
			}
			isNibbling = false;
			lastUser = who;
			isTimingCast = true;
			_totalMotionBufferIndex = 0;
			for (int i = 0; i < _totalMotionBuffer.Length; i++)
			{
				_totalMotionBuffer[i] = Vector2.Zero;
			}
			_totalMotion.Value = Vector2.Zero;
			_lastAppliedMotion = Vector2.Zero;
			who.UsingTool = true;
			whichFish = -1;
			itemCategory = "";
			recastTimerMs = 0;
			who.canMove = false;
			fishCaught = false;
			doneWithAnimation = false;
			who.canReleaseTool = false;
			hasDoneFucntionYet = false;
			isReeling = false;
			pullingOutOfWater = false;
			castingPower = 0f;
			castingChosenCountdown = 0f;
			animations.Clear();
			sparklingText = null;
			setTimingCastAnimation(who);
			return true;
		}

		public void setTimingCastAnimation(Farmer who)
		{
			if (who.CurrentTool != null)
			{
				switch (who.FacingDirection)
				{
				case 0:
					who.FarmerSprite.setCurrentFrame(295);
					who.CurrentTool.Update(0, 0, who);
					break;
				case 1:
					who.FarmerSprite.setCurrentFrame(296);
					who.CurrentTool.Update(1, 0, who);
					break;
				case 2:
					who.FarmerSprite.setCurrentFrame(297);
					who.CurrentTool.Update(2, 0, who);
					break;
				case 3:
					who.FarmerSprite.setCurrentFrame(298);
					who.CurrentTool.Update(3, 0, who);
					break;
				}
			}
		}

		public void doneFishing(Farmer who, bool consumeBaitAndTackle = false)
		{
			doneFishingEvent.Fire(consumeBaitAndTackle);
		}

		private void doDoneFishing(bool consumeBaitAndTackle)
		{
			if (consumeBaitAndTackle && lastUser != null && lastUser.IsLocalPlayer)
			{
				float num = 1f;
				if (hasEnchantmentOfType<PreservingEnchantment>())
				{
					num = 0.5f;
				}
				if (attachments[0] != null && Game1.random.NextDouble() < (double)num)
				{
					attachments[0].Stack--;
					if (attachments[0].Stack <= 0)
					{
						attachments[0] = null;
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14085"));
					}
				}
				if (attachments[1] != null && !lastCatchWasJunk && Game1.random.NextDouble() < (double)num)
				{
					attachments[1].uses.Value++;
					if (attachments[1].uses.Value >= maxTackleUses)
					{
						attachments[1] = null;
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14086"));
					}
				}
			}
			if (lastUser != null && lastUser.IsLocalPlayer)
			{
				bobber.Set(Vector2.Zero);
			}
			isNibbling = false;
			fishCaught = false;
			isFishing = false;
			isReeling = false;
			isCasting = false;
			isTimingCast = false;
			treasureCaught = false;
			showingTreasure = false;
			doneWithAnimation = false;
			pullingOutOfWater = false;
			fromFishPond = false;
			caughtDoubleFish = false;
			fishingBiteAccumulator = 0f;
			fishingNibbleAccumulator = 0f;
			timeUntilFishingBite = -1f;
			timeUntilFishingNibbleDone = -1f;
			bobberTimeAccumulator = 0f;
			if (chargeSound != null && chargeSound.IsPlaying && lastUser.IsLocalPlayer)
			{
				chargeSound.Stop(AudioStopOptions.Immediate);
				chargeSound = null;
			}
			if (reelSound != null && reelSound.IsPlaying)
			{
				reelSound.Stop(AudioStopOptions.Immediate);
				reelSound = null;
			}
			if (lastUser != null)
			{
				lastUser.UsingTool = false;
				lastUser.CanMove = true;
				lastUser.completelyStopAnimatingOrDoingAction();
				if (lastUser == Game1.player)
				{
					lastUser.faceDirection(originalFacingDirection);
				}
			}
			Game1.currentLocation.tapToMove.Reset();
		}

		public static void doneWithCastingAnimation(Farmer who)
		{
			if (who.CurrentTool != null && who.CurrentTool is FishingRod)
			{
				(who.CurrentTool as FishingRod).doneWithAnimation = true;
				if ((who.CurrentTool as FishingRod).hasDoneFucntionYet)
				{
					who.canReleaseTool = true;
					who.UsingTool = false;
					who.canMove = true;
					Farmer.canMoveNow(who);
				}
			}
		}

		public void castingEndFunction(int extraInfo)
		{
			castedButBobberStillInAir = false;
			if (lastUser != null)
			{
				float stamina = lastUser.Stamina;
				if (lastUser.CurrentTool != null)
				{
					lastUser.CurrentTool.DoFunction(lastUser.currentLocation, (int)bobber.X, (int)bobber.Y, 1, lastUser);
				}
				lastUser.lastClick = Vector2.Zero;
				if (reelSound != null)
				{
					reelSound.Stop(AudioStopOptions.Immediate);
				}
				reelSound = null;
				if (lastUser.Stamina <= 0f && stamina > 0f)
				{
					lastUser.doEmote(36);
				}
				Game1.toolHold = 0f;
				if (!isFishing && doneWithAnimation)
				{
					castingEndEnableMovement();
				}
			}
		}

		private void castingEndEnableMovement()
		{
			castingEndEnableMovementEvent.Fire();
		}

		private void doCastingEndEnableMovement()
		{
			Farmer.canMoveNow(lastUser);
		}

		public override void tickUpdate(GameTime time, Farmer who)
		{
			lastUser = who;
			beginReelingEvent.Poll();
			putAwayEvent.Poll();
			startCastingEvent.Poll();
			pullFishFromWaterEvent.Poll();
			doneFishingEvent.Poll();
			castingEndEnableMovementEvent.Poll();
			if (recastTimerMs > 0 && who.IsLocalPlayer)
			{
				if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed || Game1.didPlayerJustClickAtAll() || Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.useToolButton))
				{
					recastTimerMs -= time.ElapsedGameTime.Milliseconds;
					if (recastTimerMs <= 0)
					{
						recastTimerMs = 0;
						if (Game1.activeClickableMenu == null)
						{
							who.BeginUsingTool();
						}
					}
				}
				else
				{
					recastTimerMs = 0;
				}
			}
			if (isFishing && !Game1.shouldTimePass() && Game1.activeClickableMenu != null && !(Game1.activeClickableMenu is BobberBar))
			{
				return;
			}
			if (who.CurrentTool != null && who.CurrentTool.Equals(this) && who.UsingTool)
			{
				who.CanMove = false;
			}
			else if (Game1.currentMinigame == null && (who.CurrentTool == null || !(who.CurrentTool is FishingRod) || !who.UsingTool))
			{
				if (chargeSound != null && chargeSound.IsPlaying && who.IsLocalPlayer)
				{
					chargeSound.Stop(AudioStopOptions.Immediate);
					chargeSound = null;
				}
				return;
			}
			for (int num = animations.Count - 1; num >= 0; num--)
			{
				if (animations[num].update(time))
				{
					animations.RemoveAt(num);
				}
			}
			if (sparklingText != null && sparklingText.update(time))
			{
				sparklingText = null;
			}
			if (castingChosenCountdown > 0f)
			{
				castingChosenCountdown -= time.ElapsedGameTime.Milliseconds;
				if (castingChosenCountdown <= 0f && who.CurrentTool != null)
				{
					switch (who.FacingDirection)
					{
					case 0:
						who.FarmerSprite.animateOnce(295, 1f, 1);
						who.CurrentTool.Update(0, 0, who);
						break;
					case 1:
						who.FarmerSprite.animateOnce(296, 1f, 1);
						who.CurrentTool.Update(1, 0, who);
						break;
					case 2:
						who.FarmerSprite.animateOnce(297, 1f, 1);
						who.CurrentTool.Update(2, 0, who);
						break;
					case 3:
						who.FarmerSprite.animateOnce(298, 1f, 1);
						who.CurrentTool.Update(3, 0, who);
						break;
					}
					if (who.FacingDirection == 1 || who.FacingDirection == 3)
					{
						float num2 = Math.Max(128f, castingPower * (float)(getAddedDistance(who) + 4) * 64f);
						num2 -= 8f;
						float num3 = 0.005f;
						float num4 = (float)((double)num2 * Math.Sqrt(num3 / (2f * (num2 + 96f))));
						float animationInterval = 2f * (num4 / num3) + (float)((Math.Sqrt(num4 * num4 + 2f * num3 * 96f) - (double)num4) / (double)num3);
						if (lastUser.IsLocalPlayer)
						{
							bobber.Set(new Vector2((float)who.getStandingX() + (float)((who.FacingDirection != 3) ? 1 : (-1)) * num2, who.getStandingY()));
						}
						animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(170, 1903, 7, 8), animationInterval, 1, 0, who.Position + new Vector2(0f, -96f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f, 0f, Color.White, 4f, 0f, 0f, (float)Game1.random.Next(-20, 20) / 100f)
						{
							motion = new Vector2((float)((who.FacingDirection != 3) ? 1 : (-1)) * num4, 0f - num4),
							acceleration = new Vector2(0f, num3),
							endFunction = castingEndFunction,
							timeBasedMotion = true
						});
					}
					else
					{
						float num5 = 0f - Math.Max(128f, castingPower * (float)(getAddedDistance(who) + 3) * 64f);
						float num6 = Math.Abs(num5 - 64f);
						if (lastUser.FacingDirection == 0)
						{
							num5 = 0f - num5;
							num6 += 64f;
						}
						float num7 = 0.005f;
						float num8 = (float)Math.Sqrt(2f * num7 * num6);
						float num9 = (float)(Math.Sqrt(2f * (num6 - num5) / num7) + (double)(num8 / num7));
						num9 *= 1.05f;
						if (lastUser.FacingDirection == 0)
						{
							num9 *= 1.05f;
						}
						if (lastUser.IsLocalPlayer)
						{
							bobber.Set(new Vector2(who.getStandingX(), (float)who.getStandingY() - num5));
						}
						animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(170, 1903, 7, 8), num9, 1, 0, who.Position + new Vector2(24f, -96f), flicker: false, flipped: false, bobber.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, (float)Game1.random.Next(-20, 20) / 100f)
						{
							alphaFade = 0.0001f,
							motion = new Vector2(0f, 0f - num8),
							acceleration = new Vector2(0f, num7),
							endFunction = castingEndFunction,
							timeBasedMotion = true
						});
					}
					_hasPlayerAdjustedBobber = false;
					castedButBobberStillInAir = true;
					isCasting = false;
					if (who.IsLocalPlayer)
					{
						who.currentLocation.playSound("cast");
					}
					if (who.IsLocalPlayer && Game1.soundBank != null)
					{
						reelSound = Game1.soundBank.GetCue("slowReel");
						reelSound.SetVariable("Pitch", 1600);
						reelSound.Play();
					}
				}
			}
			else if (!isTimingCast && castingChosenCountdown <= 0f)
			{
				who.jitterStrength = 0f;
			}
			if (isTimingCast)
			{
				if (chargeSound == null && Game1.soundBank != null)
				{
					chargeSound = Game1.soundBank.GetCue("SinWave");
				}
				if (who.IsLocalPlayer && chargeSound != null && !chargeSound.IsPlaying)
				{
					chargeSound.Play();
				}
				castingPower = Math.Max(0f, Math.Min(1f, castingPower + castingTimerSpeed * (float)time.ElapsedGameTime.Milliseconds));
				if (who.IsLocalPlayer && chargeSound != null)
				{
					chargeSound.SetVariable("Pitch", 2400f * castingPower);
				}
				if (castingPower == 1f || castingPower == 0f)
				{
					castingTimerSpeed = 0f - castingTimerSpeed;
				}
				who.armOffset.Y = 2f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				who.jitterStrength = Math.Max(0f, castingPower - 0.5f);
				if (who.IsLocalPlayer && ((!usedGamePadToCast && Game1.input.GetMouseState().LeftButton == ButtonState.Released) || (usedGamePadToCast && Game1.options.gamepadControls && Game1.input.GetGamePadState().IsButtonUp(Buttons.X))) && Game1.areAllOfTheseKeysUp(Game1.GetKeyboardState(), Game1.options.useToolButton))
				{
					startCasting();
				}
				return;
			}
			if (isReeling)
			{
				if (who.IsLocalPlayer && Game1.didPlayerJustClickAtAll())
				{
					if (Game1.isAnyGamePadButtonBeingPressed())
					{
						Game1.lastCursorMotionWasMouse = false;
					}
					switch (who.FacingDirection)
					{
					case 0:
						who.FarmerSprite.setCurrentSingleFrame(76, 32000);
						break;
					case 1:
						who.FarmerSprite.setCurrentSingleFrame(72, 100);
						break;
					case 2:
						who.FarmerSprite.setCurrentSingleFrame(75, 32000);
						break;
					case 3:
						who.FarmerSprite.setCurrentSingleFrame(72, 100, secondaryArm: false, flip: true);
						break;
					}
					who.armOffset.Y = (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
					who.jitterStrength = 1f;
				}
				else
				{
					switch (who.FacingDirection)
					{
					case 0:
						who.FarmerSprite.setCurrentSingleFrame(36, 32000);
						break;
					case 1:
						who.FarmerSprite.setCurrentSingleFrame(48, 100);
						break;
					case 2:
						who.FarmerSprite.setCurrentSingleFrame(66, 32000);
						break;
					case 3:
						who.FarmerSprite.setCurrentSingleFrame(48, 100, secondaryArm: false, flip: true);
						break;
					}
					who.stopJittering();
				}
				who.armOffset = new Vector2((float)Game1.random.Next(-10, 11) / 10f, (float)Game1.random.Next(-10, 11) / 10f);
				bobberTimeAccumulator += time.ElapsedGameTime.Milliseconds;
				return;
			}
			if (isFishing)
			{
				if (lastUser.IsLocalPlayer)
				{
					bobber.Y += (float)(0.10000000149011612 * Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0));
				}
				who.canReleaseTool = true;
				bobberTimeAccumulator += time.ElapsedGameTime.Milliseconds;
				switch (who.FacingDirection)
				{
				case 0:
					who.FarmerSprite.setCurrentFrame(44);
					break;
				case 1:
					who.FarmerSprite.setCurrentFrame(89);
					break;
				case 2:
					who.FarmerSprite.setCurrentFrame(70);
					break;
				case 3:
					who.FarmerSprite.setCurrentFrame(89, 0, 10, 1, flip: true, secondaryArm: false);
					break;
				}
				who.armOffset.Y = (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2) + (float)((who.FacingDirection == 1 || who.FacingDirection == 3) ? 1 : (-1));
				if (!who.IsLocalPlayer)
				{
					return;
				}
				if (timeUntilFishingBite != -1f)
				{
					fishingBiteAccumulator += time.ElapsedGameTime.Milliseconds;
					if (fishingBiteAccumulator > timeUntilFishingBite)
					{
						fishingBiteAccumulator = 0f;
						timeUntilFishingBite = -1f;
						isNibbling = true;
						if (hasEnchantmentOfType<AutoHookEnchantment>())
						{
							timePerBobberBob = 1f;
							timeUntilFishingNibbleDone = maxTimeToNibble;
							DoFunction(who.currentLocation, (int)bobber.X, (int)bobber.Y, 1, who);
							Rumble.rumble(0.95f, 200f);
							return;
						}
						who.PlayFishBiteChime();
						Rumble.rumble(0.75f, 250f);
						timeUntilFishingNibbleDone = maxTimeToNibble;
						if (Game1.currentMinigame == null)
						{
							Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(395, 497, 3, 8), new Vector2(lastUser.getStandingX() - Game1.viewport.X, lastUser.getStandingY() - 128 - 8 - Game1.viewport.Y), flipped: false, 0.02f, Color.White)
							{
								scale = 5f,
								scaleChange = -0.01f,
								motion = new Vector2(0f, -0.5f),
								shakeIntensityChange = -0.005f,
								shakeIntensity = 1f
							});
						}
						timePerBobberBob = 1f;
					}
				}
				if (timeUntilFishingNibbleDone != -1f && !hit)
				{
					fishingNibbleAccumulator += time.ElapsedGameTime.Milliseconds;
					if (fishingNibbleAccumulator > timeUntilFishingNibbleDone)
					{
						fishingNibbleAccumulator = 0f;
						timeUntilFishingNibbleDone = -1f;
						isNibbling = false;
						timeUntilFishingBite = calculateTimeUntilFishingBite(calculateBobberTile(), isFirstCast: false, who);
					}
				}
				return;
			}
			Vector2 zero;
			if (who.UsingTool && castedButBobberStillInAir)
			{
				zero = Vector2.Zero;
				if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveDownButton))
				{
					goto IL_0da3;
				}
				if (Game1.options.gamepadControls)
				{
					_ = Game1.oldPadState;
					if (Game1.oldPadState.IsButtonDown(Buttons.DPadDown) || Game1.input.GetGamePadState().ThumbSticks.Left.Y < 0f)
					{
						goto IL_0da3;
					}
				}
				goto IL_0dcb;
			}
			if (showingTreasure)
			{
				who.FarmerSprite.setCurrentSingleFrame(0, 32000);
			}
			else if (fishCaught)
			{
				if (!Game1.isFestival())
				{
					who.faceDirection(2);
					who.FarmerSprite.setCurrentFrame(84);
				}
				if (Game1.random.NextDouble() < 0.025)
				{
					who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(653, 858, 1, 1), 9999f, 1, 1, who.Position + new Vector2(Game1.random.Next(-3, 2) * 4, -32f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.002f, 0.04f, Color.LightBlue, 5f, 0f, 0f, 0f)
					{
						acceleration = new Vector2(0f, 0.25f)
					});
				}
				if (!who.IsLocalPlayer || (Game1.input.GetMouseState().LeftButton != ButtonState.Pressed && !Game1.didPlayerJustClickAtAll() && !Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.useToolButton)))
				{
					return;
				}
				who.currentLocation.localSound("coin");
				if (!treasureCaught)
				{
					recastTimerMs = 200;
					Object @object = null;
					if (itemCategory == "Object")
					{
						@object = new Object(whichFish, 1, isRecipe: false, -1, fishQuality);
						if (whichFish == GameLocation.CAROLINES_NECKLACE_ITEM)
						{
							@object.questItem.Value = true;
						}
						if (whichFish == 79 || whichFish == 842)
						{
							@object = who.currentLocation.tryToCreateUnseenSecretNote(lastUser);
							if (@object == null)
							{
								return;
							}
						}
						if (caughtDoubleFish)
						{
							@object.Stack = 2;
						}
					}
					else if (itemCategory == "Furniture")
					{
						@object = new Furniture(whichFish, Vector2.Zero);
					}
					bool flag = fromFishPond;
					lastUser.completelyStopAnimatingOrDoingAction();
					doneFishing(lastUser, !flag);
					if (!Game1.isFestival() && !flag && itemCategory == "Object" && Game1.player.team.specialOrders != null)
					{
						foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
						{
							if (specialOrder.onFishCaught != null)
							{
								specialOrder.onFishCaught(Game1.player, @object);
							}
						}
					}
					if (!Game1.isFestival() && !lastUser.addItemToInventoryBool(@object))
					{
						Game1.activeClickableMenu = new ItemGrabMenu(new List<Item> { @object }, this).setEssential(essential: true);
					}
					return;
				}
				fishCaught = false;
				showingTreasure = true;
				who.UsingTool = true;
				int initialStack = 1;
				if (caughtDoubleFish)
				{
					initialStack = 2;
				}
				Object object2 = new Object(whichFish, initialStack, isRecipe: false, -1, fishQuality);
				if (Game1.player.team.specialOrders != null)
				{
					foreach (SpecialOrder specialOrder2 in Game1.player.team.specialOrders)
					{
						if (specialOrder2.onFishCaught != null)
						{
							specialOrder2.onFishCaught(Game1.player, object2);
						}
					}
				}
				bool flag2 = lastUser.addItemToInventoryBool(object2);
				animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(64, 1920, 32, 32), 500f, 1, 0, lastUser.Position + new Vector2(-32f, -160f), flicker: false, flipped: false, (float)lastUser.getStandingY() / 10000f + 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -0.128f),
					timeBasedMotion = true,
					endFunction = openChestEndFunction,
					extraInfoForEndBehavior = ((!flag2) ? 1 : 0),
					alpha = 0f,
					alphaFade = -0.002f
				});
			}
			else if (who.UsingTool && castedButBobberStillInAir && doneWithAnimation)
			{
				switch (who.FacingDirection)
				{
				case 0:
					who.FarmerSprite.setCurrentFrame(39);
					break;
				case 1:
					who.FarmerSprite.setCurrentFrame(89);
					break;
				case 2:
					who.FarmerSprite.setCurrentFrame(28);
					break;
				case 3:
					who.FarmerSprite.setCurrentFrame(89, 0, 10, 1, flip: true, secondaryArm: false);
					break;
				}
				who.armOffset.Y = (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			}
			else if (!castedButBobberStillInAir && whichFish != -1 && animations.Count > 0 && animations[0].timer > 500f && !Game1.eventUp)
			{
				lastUser.faceDirection(2);
				lastUser.FarmerSprite.setCurrentFrame(57);
			}
			return;
			IL_0f5c:
			if (!_hasPlayerAdjustedBobber)
			{
				Vector2 vector = calculateBobberTile();
				if (!lastUser.currentLocation.isTileFishable((int)vector.X, (int)vector.Y))
				{
					if (lastUser.FacingDirection == 3 || lastUser.FacingDirection == 1)
					{
						int num10 = 1;
						if (vector.Y % 1f < 0.5f)
						{
							num10 = -1;
						}
						if (lastUser.currentLocation.isTileFishable((int)vector.X, (int)vector.Y + num10))
						{
							zero.Y += (float)num10 * 4f;
						}
						else if (lastUser.currentLocation.isTileFishable((int)vector.X, (int)vector.Y - num10))
						{
							zero.Y -= (float)num10 * 4f;
						}
					}
					if (lastUser.FacingDirection == 0 || lastUser.FacingDirection == 2)
					{
						int num11 = 1;
						if (vector.X % 1f < 0.5f)
						{
							num11 = -1;
						}
						if (lastUser.currentLocation.isTileFishable((int)vector.X + num11, (int)vector.Y))
						{
							zero.X += (float)num11 * 4f;
						}
						else if (lastUser.currentLocation.isTileFishable((int)vector.X - num11, (int)vector.Y))
						{
							zero.X -= (float)num11 * 4f;
						}
					}
				}
			}
			if (who.IsLocalPlayer)
			{
				bobber.Set(bobber + zero);
				_totalMotion.Set(_totalMotion.Value + zero);
			}
			if (animations.Count <= 0)
			{
				return;
			}
			Vector2 vector2 = Vector2.Zero;
			if (who.IsLocalPlayer)
			{
				vector2 = _totalMotion.Value;
			}
			else
			{
				_totalMotionBuffer[_totalMotionBufferIndex] = _totalMotion.Value;
				for (int i = 0; i < _totalMotionBuffer.Length; i++)
				{
					vector2 += _totalMotionBuffer[i];
				}
				vector2 /= (float)_totalMotionBuffer.Length;
				_totalMotionBufferIndex = (_totalMotionBufferIndex + 1) % _totalMotionBuffer.Length;
			}
			animations[0].position -= _lastAppliedMotion;
			_lastAppliedMotion = vector2;
			animations[0].position += vector2;
			return;
			IL_0e51:
			if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveUpButton))
			{
				goto IL_0eae;
			}
			if (Game1.options.gamepadControls)
			{
				_ = Game1.oldPadState;
				if (Game1.oldPadState.IsButtonDown(Buttons.DPadUp) || Game1.input.GetGamePadState().ThumbSticks.Left.Y > 0f)
				{
					goto IL_0eae;
				}
			}
			goto IL_0ed6;
			IL_0ed6:
			if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveLeftButton))
			{
				goto IL_0f33;
			}
			if (Game1.options.gamepadControls)
			{
				_ = Game1.oldPadState;
				if (Game1.oldPadState.IsButtonDown(Buttons.DPadLeft) || Game1.input.GetGamePadState().ThumbSticks.Left.X < 0f)
				{
					goto IL_0f33;
				}
			}
			goto IL_0f5c;
			IL_0e28:
			if (who.FacingDirection != 1 && who.FacingDirection != 3)
			{
				zero.X += 2f;
				_hasPlayerAdjustedBobber = true;
			}
			goto IL_0e51;
			IL_0dcb:
			if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveRightButton))
			{
				goto IL_0e28;
			}
			if (Game1.options.gamepadControls)
			{
				_ = Game1.oldPadState;
				if (Game1.oldPadState.IsButtonDown(Buttons.DPadRight) || Game1.input.GetGamePadState().ThumbSticks.Left.X > 0f)
				{
					goto IL_0e28;
				}
			}
			goto IL_0e51;
			IL_0eae:
			if (who.FacingDirection != 0 && who.FacingDirection != 2)
			{
				zero.Y -= 4f;
				_hasPlayerAdjustedBobber = true;
			}
			goto IL_0ed6;
			IL_0da3:
			if (who.FacingDirection != 2 && who.FacingDirection != 0)
			{
				zero.Y += 4f;
				_hasPlayerAdjustedBobber = true;
			}
			goto IL_0dcb;
			IL_0f33:
			if (who.FacingDirection != 3 && who.FacingDirection != 1)
			{
				zero.X -= 2f;
				_hasPlayerAdjustedBobber = true;
			}
			goto IL_0f5c;
		}

		private void startCasting()
		{
			startCastingEvent.Fire();
		}

		public void beginReeling()
		{
			isReeling = true;
		}

		private void doStartCasting()
		{
			if (chargeSound != null && lastUser.IsLocalPlayer)
			{
				chargeSound.Stop(AudioStopOptions.Immediate);
				chargeSound = null;
			}
			if (lastUser.currentLocation != null)
			{
				if (lastUser.IsLocalPlayer)
				{
					lastUser.currentLocation.localSound("button1");
					Rumble.rumble(0.5f, 150f);
				}
				lastUser.UsingTool = true;
				isTimingCast = false;
				isCasting = true;
				castingChosenCountdown = 350f;
				lastUser.armOffset.Y = 0f;
				if (castingPower > 0.99f && lastUser.IsLocalPlayer)
				{
					Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(545, 1921, 53, 19), 800f, 1, 0, Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(0f, -192f)), flicker: false, flipped: false, 1f, 0.01f, Color.White, 2f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(0f, -4f),
						acceleration = new Vector2(0f, 0.2f),
						delayBeforeAnimationStart = 200
					});
					DelayedAction.playSoundAfterDelay("crit", 200);
				}
			}
		}

		public void openChestEndFunction(int extra)
		{
			lastUser.currentLocation.localSound("openChest");
			animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(64, 1920, 32, 32), 200f, 4, 0, lastUser.Position + new Vector2(-32f, -228f), flicker: false, flipped: false, (float)lastUser.getStandingY() / 10000f + 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				endFunction = openTreasureMenuEndFunction,
				extraInfoForEndBehavior = extra
			});
			sparklingText = null;
		}

		public override bool doesShowTileLocationMarker()
		{
			return false;
		}

		public void openTreasureMenuEndFunction(int extra)
		{
			lastUser.gainExperience(5, 10 * (clearWaterDistance + 1));
			lastUser.UsingTool = false;
			int initialStack = 1;
			if (caughtDoubleFish)
			{
				initialStack = 2;
			}
			lastUser.completelyStopAnimatingOrDoingAction();
			doneFishing(lastUser, consumeBaitAndTackle: true);
			List<Item> list = new List<Item>();
			if (extra == 1)
			{
				list.Add(new Object(whichFish, initialStack, isRecipe: false, -1, fishQuality));
			}
			float num = 1f;
			while (Game1.random.NextDouble() <= (double)num)
			{
				num *= 0.4f;
				if (Game1.currentSeason.Equals("spring") && !(lastUser.currentLocation is Beach) && Game1.random.NextDouble() < 0.1)
				{
					list.Add(new Object(273, Game1.random.Next(2, 6) + ((Game1.random.NextDouble() < 0.25) ? 5 : 0)));
				}
				if (caughtDoubleFish && Game1.random.NextDouble() < 0.5)
				{
					list.Add(new Object(774, 2 + ((Game1.random.NextDouble() < 0.25) ? 2 : 0)));
				}
				if (Game1.random.NextDouble() <= 0.33 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
				{
					list.Add(new Object(890, Game1.random.Next(1, 3) + ((Game1.random.NextDouble() < 0.25) ? 2 : 0)));
				}
				switch (Game1.random.Next(4))
				{
				case 0:
				{
					if (clearWaterDistance >= 5 && Game1.random.NextDouble() < 0.03)
					{
						list.Add(new Object(386, Game1.random.Next(1, 3)));
						break;
					}
					List<int> list2 = new List<int>();
					if (clearWaterDistance >= 4)
					{
						list2.Add(384);
					}
					if (clearWaterDistance >= 3 && (list2.Count == 0 || Game1.random.NextDouble() < 0.6))
					{
						list2.Add(380);
					}
					if (list2.Count == 0 || Game1.random.NextDouble() < 0.6)
					{
						list2.Add(378);
					}
					if (list2.Count == 0 || Game1.random.NextDouble() < 0.6)
					{
						list2.Add(388);
					}
					if (list2.Count == 0 || Game1.random.NextDouble() < 0.6)
					{
						list2.Add(390);
					}
					list2.Add(382);
					list.Add(new Object(list2.ElementAt(Game1.random.Next(list2.Count)), Game1.random.Next(2, 7) * ((!(Game1.random.NextDouble() < 0.05 + (double)(int)lastUser.luckLevel * 0.015)) ? 1 : 2)));
					if (Game1.random.NextDouble() < 0.05 + (double)lastUser.LuckLevel * 0.03)
					{
						list.Last().Stack *= 2;
					}
					break;
				}
				case 1:
					if (clearWaterDistance >= 4 && Game1.random.NextDouble() < 0.1 && lastUser.FishingLevel >= 6)
					{
						list.Add(new Object(687, 1));
					}
					else if (Game1.random.NextDouble() < 0.25 && lastUser.craftingRecipes.ContainsKey("Wild Bait"))
					{
						list.Add(new Object(774, 5 + ((Game1.random.NextDouble() < 0.25) ? 5 : 0)));
					}
					else if (lastUser.FishingLevel >= 6)
					{
						list.Add(new Object(685, 1));
					}
					else
					{
						list.Add(new Object(685, 10));
					}
					break;
				case 2:
					if (Game1.random.NextDouble() < 0.1 && (int)Game1.netWorldState.Value.LostBooksFound < 21 && lastUser != null && lastUser.hasOrWillReceiveMail("lostBookFound"))
					{
						list.Add(new Object(102, 1));
					}
					else if (lastUser.archaeologyFound.Count() > 0)
					{
						if (Game1.random.NextDouble() < 0.25 && lastUser.FishingLevel > 1)
						{
							list.Add(new Object(Game1.random.Next(585, 588), 1));
						}
						else if (Game1.random.NextDouble() < 0.5 && lastUser.FishingLevel > 1)
						{
							list.Add(new Object(Game1.random.Next(103, 120), 1));
						}
						else
						{
							list.Add(new Object(535, 1));
						}
					}
					else
					{
						list.Add(new Object(382, Game1.random.Next(1, 3)));
					}
					break;
				case 3:
					switch (Game1.random.Next(3))
					{
					case 0:
						if (clearWaterDistance >= 4)
						{
							list.Add(new Object(537 + ((Game1.random.NextDouble() < 0.4) ? Game1.random.Next(-2, 0) : 0), Game1.random.Next(1, 4)));
						}
						else if (clearWaterDistance >= 3)
						{
							list.Add(new Object(536 + ((Game1.random.NextDouble() < 0.4) ? (-1) : 0), Game1.random.Next(1, 4)));
						}
						else
						{
							list.Add(new Object(535, Game1.random.Next(1, 4)));
						}
						if (Game1.random.NextDouble() < 0.05 + (double)lastUser.LuckLevel * 0.03)
						{
							list.Last().Stack *= 2;
						}
						break;
					case 1:
						if (lastUser.FishingLevel < 2)
						{
							list.Add(new Object(382, Game1.random.Next(1, 4)));
							break;
						}
						if (clearWaterDistance >= 4)
						{
							list.Add(new Object((Game1.random.NextDouble() < 0.3) ? 82 : ((Game1.random.NextDouble() < 0.5) ? 64 : 60), Game1.random.Next(1, 3)));
						}
						else if (clearWaterDistance >= 3)
						{
							list.Add(new Object((Game1.random.NextDouble() < 0.3) ? 84 : ((Game1.random.NextDouble() < 0.5) ? 70 : 62), Game1.random.Next(1, 3)));
						}
						else
						{
							list.Add(new Object((Game1.random.NextDouble() < 0.3) ? 86 : ((Game1.random.NextDouble() < 0.5) ? 66 : 68), Game1.random.Next(1, 3)));
						}
						if (Game1.random.NextDouble() < 0.028 * (double)((float)clearWaterDistance / 5f))
						{
							list.Add(new Object(72, 1));
						}
						if (Game1.random.NextDouble() < 0.05)
						{
							list.Last().Stack *= 2;
						}
						break;
					case 2:
					{
						if (lastUser.FishingLevel < 2)
						{
							list.Add(new Object(770, Game1.random.Next(1, 4)));
							break;
						}
						float num2 = (1f + (float)lastUser.DailyLuck) * ((float)clearWaterDistance / 5f);
						if (Game1.random.NextDouble() < 0.05 * (double)num2 && !lastUser.specialItems.Contains(14))
						{
							list.Add(new MeleeWeapon(14)
							{
								specialItem = true
							});
						}
						if (Game1.random.NextDouble() < 0.05 * (double)num2 && !lastUser.specialItems.Contains(51))
						{
							list.Add(new MeleeWeapon(51)
							{
								specialItem = true
							});
						}
						if (Game1.random.NextDouble() < 0.07 * (double)num2)
						{
							switch (Game1.random.Next(3))
							{
							case 0:
								list.Add(new Ring(516 + ((Game1.random.NextDouble() < (double)((float)lastUser.LuckLevel / 11f)) ? 1 : 0)));
								break;
							case 1:
								list.Add(new Ring(518 + ((Game1.random.NextDouble() < (double)((float)lastUser.LuckLevel / 11f)) ? 1 : 0)));
								break;
							case 2:
								list.Add(new Ring(Game1.random.Next(529, 535)));
								break;
							}
						}
						if (Game1.random.NextDouble() < 0.02 * (double)num2)
						{
							list.Add(new Object(166, 1));
						}
						if (lastUser.FishingLevel > 5 && Game1.random.NextDouble() < 0.001 * (double)num2)
						{
							list.Add(new Object(74, 1));
						}
						if (Game1.random.NextDouble() < 0.01 * (double)num2)
						{
							list.Add(new Object(127, 1));
						}
						if (Game1.random.NextDouble() < 0.01 * (double)num2)
						{
							list.Add(new Object(126, 1));
						}
						if (Game1.random.NextDouble() < 0.01 * (double)num2)
						{
							list.Add(new Ring(527));
						}
						if (Game1.random.NextDouble() < 0.01 * (double)num2)
						{
							list.Add(new Boots(Game1.random.Next(504, 514)));
						}
						if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && Game1.random.NextDouble() < 0.01 * (double)num2)
						{
							list.Add(new Object(928, 1));
						}
						if (list.Count == 1)
						{
							list.Add(new Object(72, 1));
						}
						break;
					}
					}
					break;
				}
			}
			if (list.Count == 0)
			{
				list.Add(new Object(685, Game1.random.Next(1, 4) * 5));
			}
			Game1.activeClickableMenu = new ItemGrabMenu(list, this).setEssential(essential: true);
			(Game1.activeClickableMenu as ItemGrabMenu).source = 3;
			lastUser.completelyStopAnimatingOrDoingAction();
		}
	}
}
