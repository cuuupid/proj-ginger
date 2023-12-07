using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class IslandNorth : IslandLocation
	{
		[XmlElement("bridgeFixed")]
		public readonly NetBool bridgeFixed = new NetBool();

		[XmlElement("traderActivated")]
		public readonly NetBool traderActivated = new NetBool();

		[XmlElement("boulderRemoved")]
		public readonly NetBool boulderRemoved = new NetBool();

		[XmlElement("caveOpened")]
		public readonly NetBool caveOpened = new NetBool();

		[XmlElement("treeNutShot")]
		public readonly NetBool treeNutShot = new NetBool();

		[XmlIgnore]
		public List<SuspensionBridge> suspensionBridges = new List<SuspensionBridge>();

		[XmlIgnore]
		protected bool _sawFlameSpriteSouth;

		[XmlIgnore]
		protected bool _sawFlameSpriteNorth;

		[XmlIgnore]
		protected bool hasTriedFirstEntryDigSiteLoad;

		private float boulderKnockTimer;

		private float boulderTextTimer;

		private string boulderTextString;

		private int boulderKnocksLeft;

		private Microsoft.Xna.Framework.Rectangle boulderPosition = new Microsoft.Xna.Framework.Rectangle(1344, 3008, 128, 64);

		private float doneHittingBoulderWithToolTimer;

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(bridgeFixed);
			bridgeFixed.InterpolationWait = false;
			bridgeFixed.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && mapPath.Value != null)
				{
					ApplyFixedBridge();
				}
			};
			base.NetFields.AddField(traderActivated);
			traderActivated.InterpolationWait = false;
			traderActivated.fieldChangeEvent += delegate
			{
				if (!Utility.ShouldIgnoreValueChangeCallback())
				{
					ApplyIslandTraderHut();
				}
			};
			base.NetFields.AddField(caveOpened);
			caveOpened.InterpolationWait = false;
			caveOpened.fieldChangeEvent += delegate
			{
				if (!Utility.ShouldIgnoreValueChangeCallback())
				{
					ApplyCaveOpened();
				}
			};
			base.NetFields.AddField(treeNutShot);
			treeNutShot.InterpolationWait = false;
		}

		public override void SetBuriedNutLocations()
		{
			buriedNutPoints.Add(new Point(57, 79));
			buriedNutPoints.Add(new Point(19, 39));
			buriedNutPoints.Add(new Point(19, 13));
			buriedNutPoints.Add(new Point(54, 21));
			buriedNutPoints.Add(new Point(42, 77));
			buriedNutPoints.Add(new Point(62, 54));
			buriedNutPoints.Add(new Point(26, 81));
			base.SetBuriedNutLocations();
		}

		public virtual void ApplyFixedBridge()
		{
			if (map != null)
			{
				ApplyMapOverride("Island_Bridge_Repaired", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(31, 52, 4, 3));
			}
		}

		public virtual void ApplyBoulderRemove()
		{
			if (map != null)
			{
				ApplyMapOverride("Island_Boulder_Removed", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(38, 19, 6, 5));
			}
		}

		public virtual void ApplyIslandTraderHut()
		{
			if (map != null)
			{
				ApplyMapOverride("Island_N_Trader", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(32, 64, 9, 10));
				removeTemporarySpritesWithIDLocal(8989f);
				removeTemporarySpritesWithIDLocal(8988f);
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(33.45f, 70.33f) * 64f + new Vector2(-16f, -32f), flipped: false, 0f, Color.White)
				{
					delayBeforeAnimationStart = 10,
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					light = true,
					lightID = 8989,
					id = 8989f,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = 0.46144f
				});
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(39.45f, 70.33f) * 64f + new Vector2(-16f, -32f), flipped: false, 0f, Color.White)
				{
					delayBeforeAnimationStart = 10,
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					light = true,
					lightID = 8988,
					id = 8988f,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = 0.46144f
				});
			}
		}

		public virtual void ApplyCaveOpened()
		{
			if (Game1.player.currentLocation == null || !Game1.player.currentLocation.Equals(this))
			{
				return;
			}
			for (int i = 0; i < 12; i++)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(146, 229 + Game1.random.Next(3) * 9, 9, 9), Utility.getRandomPositionInThisRectangle(boulderPosition, Game1.random), Game1.random.NextDouble() < 0.5, 0f, Color.White)
				{
					scale = 4f,
					motion = new Vector2(Game1.random.Next(-3, 1), Game1.random.Next(-15, -9)),
					acceleration = new Vector2(0f, 0.4f),
					rotationChange = (float)Game1.random.Next(-2, 3) * 0.01f,
					drawAboveAlwaysFront = true,
					yStopCoordinate = boulderPosition.Bottom + 1 + Game1.random.Next(64),
					delayBeforeAnimationStart = i * 15
				});
				temporarySprites[temporarySprites.Count - 1].initialPosition.Y = temporarySprites[temporarySprites.Count - 1].yStopCoordinate;
				temporarySprites[temporarySprites.Count - 1].reachedStopCoordinate = temporarySprites[temporarySprites.Count - 1].bounce;
			}
			for (int j = 0; j < 8; j++)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), Utility.getRandomPositionInThisRectangle(boulderPosition, Game1.random) + new Vector2(-32f, -32f), flipped: false, 0.007f, Color.White)
				{
					alpha = 0.75f,
					motion = new Vector2(0f, -1f),
					acceleration = new Vector2(0.002f, 0f),
					interval = 99999f,
					layerDepth = 1f,
					scale = 4f,
					scaleChange = 0.02f,
					rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
					delayBeforeAnimationStart = j * 40
				});
			}
			Game1.playSound("boulderBreak");
			Game1.player.freezePause = 3000;
			DelayedAction.functionAfterDelay(delegate
			{
				Game1.globalFadeToBlack(delegate
				{
					startEvent(new Event(Game1.content.LoadString("Strings\\Locations:IslandNorth_Event_SafariManAppear")));
				});
			}, 1000);
		}

		public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
		{
			if (xLocation == 27 && yLocation == 28 && who.secretNotesSeen.Contains(1010))
			{
				Game1.player.team.RequestLimitedNutDrops("Island_N_BuriedTreasureNut", this, xLocation * 64, yLocation * 64, 1);
				if (!Game1.player.hasOrWillReceiveMail("Island_N_BuriedTreasure"))
				{
					Game1.createItemDebris(new Object(289, 1), new Vector2(xLocation, yLocation) * 64f, 1);
					Game1.addMailForTomorrow("Island_N_BuriedTreasure", noLetter: true);
				}
			}
			if (xLocation == 26 && yLocation == 81 && !Game1.player.team.collectedNutTracker.ContainsKey("Buried_IslandNorth_26_81"))
			{
				DelayedAction.functionAfterDelay(delegate
				{
					TemporaryAnimatedSprite t = getTemporarySpriteByID(79797);
					if (t != null)
					{
						t.sourceRectStartingPos.X += 40f;
						t.sourceRect.X = 181;
						t.interval = 100f;
						t.shakeIntensity = 1f;
						playSound("monkey1");
						t.motion = new Vector2(-3f, -10f);
						t.acceleration = new Vector2(0f, 0.3f);
						t.yStopCoordinate = (int)t.position.Y + 1;
						t.reachedStopCoordinate = delegate
						{
							temporarySprites.Add(new TemporaryAnimatedSprite(50, t.position, Color.Green)
							{
								drawAboveAlwaysFront = true
							});
							removeTemporarySpritesWithID(79797);
							playSound("leafrustle");
						};
					}
				}, 700);
			}
			return base.checkForBuriedItem(xLocation, yLocation, explosion, detectOnly, who);
		}

		public IslandNorth()
		{
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false)
		{
			if (projectile && damagesFarmer == 0 && position.Bottom < 832)
			{
				if (position.Intersects(new Microsoft.Xna.Framework.Rectangle(3648, 576, 256, 64)))
				{
					if (Game1.IsMasterGame && !treeNutShot.Value)
					{
						Game1.player.team.MarkCollectedNut("TreeNutShot");
						treeNutShot.Value = true;
						Game1.createItemDebris(new Object(73, 1), new Vector2(58.5f, 11f) * 64f, 0, this, 0);
					}
					return true;
				}
				return false;
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding, projectile, ignoreCharacterRequirement);
		}

		public IslandNorth(string map, string name)
			: base(map, name)
		{
			parrotUpgradePerches.Clear();
			parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(35, 52), new Microsoft.Xna.Framework.Rectangle(31, 52, 4, 4), 10, delegate
			{
				Game1.addMailForTomorrow("Island_UpgradeBridge", noLetter: true, sendToEveryone: true);
				bridgeFixed.Value = true;
			}, () => bridgeFixed.Value, "Bridge", "Island_Turtle"));
			parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(32, 72), new Microsoft.Xna.Framework.Rectangle(33, 68, 5, 5), 10, delegate
			{
				Game1.addMailForTomorrow("Island_UpgradeTrader", noLetter: true, sendToEveryone: true);
				traderActivated.Value = true;
			}, () => traderActivated.Value, "Trader", "Island_UpgradeHouse"));
			largeTerrainFeatures.Add(new Bush(new Vector2(45f, 38f), 4, this));
			largeTerrainFeatures.Add(new Bush(new Vector2(47f, 40f), 4, this));
			largeTerrainFeatures.Add(new Bush(new Vector2(13f, 33f), 4, this));
			largeTerrainFeatures.Add(new Bush(new Vector2(5f, 30f), 4, this));
		}

		protected override void resetSharedState()
		{
			base.resetSharedState();
		}

		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			if (l is IslandNorth)
			{
				IslandNorth islandNorth = l as IslandNorth;
				bridgeFixed.Value = islandNorth.bridgeFixed;
				boulderRemoved.Value = islandNorth.boulderRemoved;
				treeNutShot.Value = islandNorth.treeNutShot.Value;
				caveOpened.Value = islandNorth.caveOpened.Value;
				traderActivated.Value = islandNorth.traderActivated.Value;
			}
			base.TransferDataFromSavedLocation(l);
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			int tileIndexAt = getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings");
			if ((uint)(tileIndexAt - 2074) <= 4u)
			{
				Game1.activeClickableMenu = new ShopMenu(getIslandMerchantTradeStock(Game1.player), 0, "IslandTrade");
				return true;
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public static Dictionary<ISalable, int[]> getIslandMerchantTradeStock(Farmer who)
		{
			Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
			Item key = new Object(688, 1);
			dictionary.Add(key, new int[4] { 0, 2147483647, 830, 5 });
			key = new Object(831, 1);
			dictionary.Add(key, new int[4] { 0, 2147483647, 881, 2 });
			key = new Object(833, 1);
			dictionary.Add(key, new int[4] { 0, 2147483647, 851, 1 });
			if (Game1.netWorldState.Value.GoldenCoconutCracked.Value)
			{
				key = new Object(791, 1);
				dictionary.Add(key, new int[4] { 0, 2147483647, 88, 10 });
			}
			key = new TV(2326, Vector2.Zero);
			dictionary.Add(key, new int[4] { 0, 2147483647, 830, 30 });
			key = new Furniture(2331, Vector2.Zero);
			dictionary.Add(key, new int[4] { 0, 2147483647, 848, 5 });
			if (Game1.dayOfMonth % 2 == 0)
			{
				key = new Furniture(134, Vector2.Zero);
				dictionary.Add(key, new int[4] { 0, 2147483647, 837, 1 });
			}
			key = new Object(69, 1);
			dictionary.Add(key, new int[4] { 0, 2147483647, 852, 5 });
			key = new Object(835, 1);
			dictionary.Add(key, new int[4] { 0, 2147483647, 719, 75 });
			if (Game1.dayOfMonth % 7 == 1)
			{
				key = new Hat(79);
				dictionary.Add(key, new int[4] { 0, 2147483647, 830, 30 });
			}
			if (Game1.dayOfMonth % 7 == 3)
			{
				key = new Hat(80);
				dictionary.Add(key, new int[4] { 0, 2147483647, 830, 30 });
			}
			if (Game1.dayOfMonth % 7 == 5)
			{
				key = new Hat(81);
				dictionary.Add(key, new int[4] { 0, 2147483647, 830, 30 });
			}
			key = new BedFurniture(2496, Vector2.Zero);
			dictionary.Add(key, new int[4] { 0, 2147483647, 848, 100 });
			key = new BedFurniture(2176, Vector2.Zero);
			dictionary.Add(key, new int[4] { 0, 2147483647, 829, 20 });
			if (Game1.dayOfMonth % 7 == 0)
			{
				key = new BedFurniture(2180, Vector2.Zero);
				dictionary.Add(key, new int[4] { 0, 2147483647, 91, 5 });
			}
			if (Game1.dayOfMonth % 7 == 2)
			{
				key = new Furniture(2393, Vector2.Zero);
				dictionary.Add(key, new int[4] { 0, 2147483647, 832, 1 });
			}
			if (Game1.dayOfMonth % 7 == 4)
			{
				key = new Furniture(2329, Vector2.Zero);
				dictionary.Add(key, new int[4] { 0, 2147483647, 834, 5 });
			}
			if (Game1.dayOfMonth % 7 == 6)
			{
				key = new Furniture(1228, Vector2.Zero);
				dictionary.Add(key, new int[4] { 0, 2147483647, 838, 3 });
			}
			key = new Object(292, 1);
			dictionary.Add(key, new int[4] { 0, 2147483647, 836, 1 });
			key = new Clothing(7);
			dictionary.Add(key, new int[4] { 0, 2147483647, 830, 50 });
			if (!Game1.player.cookingRecipes.ContainsKey("Banana Pudding"))
			{
				key = new Object(904, 1, isRecipe: true);
				dictionary.Add(key, new int[4] { 0, 1, 881, 30 });
			}
			if (!Game1.player.cookingRecipes.ContainsKey("Deluxe Retaining Soil"))
			{
				key = new Object(920, 1, isRecipe: true);
				dictionary.Add(key, new int[4] { 0, 1, 848, 50 });
			}
			if (Game1.dayOfMonth == 28 && Game1.stats.getStat("hardModeMonstersKilled") > 50)
			{
				key = new Object(896, 1);
				dictionary.Add(key, new int[4] { 0, 2147483647, 910, 10 });
			}
			return dictionary;
		}

		public override List<Vector2> GetAdditionalWalnutBushes()
		{
			List<Vector2> list = new List<Vector2>();
			list.Add(new Vector2(56f, 27f));
			return list;
		}

		public override void digUpArtifactSpot(int xLocation, int yLocation, Farmer who)
		{
			Random random = new Random(xLocation * 2000 + yLocation * 777 + (int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + (int)Game1.stats.DirtHoed);
			if (bridgeFixed.Value)
			{
				if (random.NextDouble() < 0.1)
				{
					Game1.createItemDebris(new Object(825, 1), new Vector2(xLocation, yLocation) * 64f, -1, this);
					return;
				}
				if (random.NextDouble() < 0.25)
				{
					Game1.createMultipleObjectDebris(881, xLocation, yLocation, random.Next(1, 3) + ((random.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? 1 : 0) + ((random.NextDouble() < (double)((float)who.MiningLevel / 100f)) ? 1 : 0), who.uniqueMultiplayerID, this);
				}
			}
			base.digUpArtifactSpot(xLocation, yLocation, who);
		}

		public override bool catchOceanCrabPotFishFromThisSpot(int x, int y)
		{
			return false;
		}

		public override Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			Random random = new Random((int)bobberTile.X * 2000 + (int)bobberTile.Y * 777 + (int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + (int)Game1.stats.TimesFished);
			if ((bool)(Game1.getLocationFromName("IslandNorth") as IslandNorth).bridgeFixed && random.NextDouble() < 0.1)
			{
				return new Object(821, 1);
			}
			if (who != null && who.getTileY() >= 72)
			{
				if (!who.mailReceived.Contains("gotSecretIslandNPainting"))
				{
					who.mailReceived.Add("gotSecretIslandNPainting");
					return new Furniture(2419, Vector2.Zero);
				}
				if (random.NextDouble() < 0.1)
				{
					return new Furniture(2419, Vector2.Zero);
				}
			}
			if (who != null && bobberTile.Y < 35f && bobberTile.X < 4f)
			{
				if (!who.mailReceived.Contains("gotSecretIslandNSquirrel"))
				{
					who.mailReceived.Add("gotSecretIslandNSquirrel");
					return new Furniture(2814, Vector2.Zero);
				}
				if (random.NextDouble() < 0.1)
				{
					return new Furniture(2814, Vector2.Zero);
				}
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			foreach (SuspensionBridge suspensionBridge in suspensionBridges)
			{
				suspensionBridge.Update(time);
			}
			if (!caveOpened && Utility.isOnScreen(Utility.PointToVector2(boulderPosition.Location), 1))
			{
				boulderKnockTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
				boulderTextTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
				if (doneHittingBoulderWithToolTimer > 0f)
				{
					doneHittingBoulderWithToolTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
					if (doneHittingBoulderWithToolTimer <= 0f)
					{
						boulderTextTimer = 2000f;
						boulderTextString = Game1.content.LoadString("Strings\\Locations:IslandNorth_CaveTool_" + Game1.random.Next(4));
					}
				}
				if (boulderKnocksLeft > 0)
				{
					if (boulderKnockTimer < 0f)
					{
						Game1.playSound("hammer");
						boulderKnocksLeft--;
						boulderKnockTimer = 500f;
						if (boulderKnocksLeft == 0 && Game1.random.NextDouble() < 0.5)
						{
							DelayedAction.functionAfterDelay(delegate
							{
								boulderTextTimer = 2000f;
								boulderTextString = Game1.content.LoadString("Strings\\Locations:IslandNorth_CaveHelp_" + Game1.random.Next(4));
							}, 1000);
						}
					}
				}
				else if (Game1.random.NextDouble() < 0.002 && boulderTextTimer < -500f)
				{
					boulderKnocksLeft = Game1.random.Next(3, 6);
				}
			}
			if (!_sawFlameSpriteSouth && Utility.isThereAFarmerWithinDistance(new Vector2(36f, 79f), 5, this) == Game1.player)
			{
				Game1.addMailForTomorrow("Saw_Flame_Sprite_North_South", noLetter: true);
				TemporaryAnimatedSprite temporarySpriteByID = getTemporarySpriteByID(999);
				if (temporarySpriteByID != null)
				{
					temporarySpriteByID.yPeriodic = false;
					temporarySpriteByID.xPeriodic = false;
					temporarySpriteByID.sourceRect.Y = 0;
					temporarySpriteByID.sourceRectStartingPos.Y = 0f;
					temporarySpriteByID.motion = new Vector2(1f, -4f);
					temporarySpriteByID.acceleration = new Vector2(0f, -0.04f);
					temporarySpriteByID.drawAboveAlwaysFront = true;
				}
				localSound("magma_sprite_spot");
				temporarySpriteByID = getTemporarySpriteByID(998);
				if (temporarySpriteByID != null)
				{
					temporarySpriteByID.yPeriodic = false;
					temporarySpriteByID.xPeriodic = false;
					temporarySpriteByID.motion = new Vector2(1f, -4f);
					temporarySpriteByID.acceleration = new Vector2(0f, -0.04f);
				}
				_sawFlameSpriteSouth = true;
			}
			if (!_sawFlameSpriteNorth && Utility.isThereAFarmerWithinDistance(new Vector2(41f, 30f), 5, this) == Game1.player)
			{
				Game1.addMailForTomorrow("Saw_Flame_Sprite_North_North", noLetter: true);
				TemporaryAnimatedSprite temporarySpriteByID2 = getTemporarySpriteByID(9999);
				if (temporarySpriteByID2 != null)
				{
					temporarySpriteByID2.yPeriodic = false;
					temporarySpriteByID2.xPeriodic = false;
					temporarySpriteByID2.sourceRect.Y = 0;
					temporarySpriteByID2.sourceRectStartingPos.Y = 0f;
					temporarySpriteByID2.motion = new Vector2(0f, -4f);
					temporarySpriteByID2.acceleration = new Vector2(0f, -0.04f);
					temporarySpriteByID2.yStopCoordinate = 1216;
					temporarySpriteByID2.reachedStopCoordinate = delegate
					{
						removeTemporarySpritesWithID(9999);
					};
				}
				localSound("magma_sprite_spot");
				temporarySpriteByID2 = getTemporarySpriteByID(9998);
				if (temporarySpriteByID2 != null)
				{
					temporarySpriteByID2.yPeriodic = false;
					temporarySpriteByID2.xPeriodic = false;
					temporarySpriteByID2.motion = new Vector2(0f, -4f);
					temporarySpriteByID2.acceleration = new Vector2(0f, -0.04f);
					temporarySpriteByID2.yStopCoordinate = 1280;
					temporarySpriteByID2.reachedStopCoordinate = delegate
					{
						removeTemporarySpritesWithID(9998);
					};
				}
				_sawFlameSpriteNorth = true;
			}
			if (hasTriedFirstEntryDigSiteLoad)
			{
				return;
			}
			if (Game1.IsMasterGame && !Game1.player.hasOrWillReceiveMail("ISLAND_NORTH_DIGSITE_LOAD"))
			{
				Game1.addMail("ISLAND_NORTH_DIGSITE_LOAD", noLetter: true);
				for (int i = 0; i < 40; i++)
				{
					digSiteUpdate();
				}
			}
			hasTriedFirstEntryDigSiteLoad = true;
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			if (!caveOpened && boulderPosition.Intersects(position))
			{
				return true;
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
		}

		public override bool isTilePlaceable(Vector2 tile_location, Item item = null)
		{
			Point value = Utility.Vector2ToPoint((tile_location + new Vector2(0.5f, 0.5f)) * 64f);
			if (!caveOpened && boulderPosition.Contains(value))
			{
				return false;
			}
			return base.isTilePlaceable(tile_location, item);
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			digSiteUpdate();
			List<Vector2> list = new List<Vector2>();
			foreach (Vector2 key in terrainFeatures.Keys)
			{
				if (terrainFeatures[key] is HoeDirt && (terrainFeatures[key] as HoeDirt).crop != null && (bool)(terrainFeatures[key] as HoeDirt).crop.forageCrop)
				{
					list.Add(key);
				}
			}
			foreach (Vector2 item in list)
			{
				terrainFeatures.Remove(item);
			}
			List<Microsoft.Xna.Framework.Rectangle> list2 = new List<Microsoft.Xna.Framework.Rectangle>();
			list2.Add(new Microsoft.Xna.Framework.Rectangle(10, 51, 1, 8));
			list2.Add(new Microsoft.Xna.Framework.Rectangle(15, 59, 1, 4));
			list2.Add(new Microsoft.Xna.Framework.Rectangle(18, 34, 1, 1));
			list2.Add(new Microsoft.Xna.Framework.Rectangle(40, 48, 6, 6));
			for (int i = 0; i < 1; i++)
			{
				Microsoft.Xna.Framework.Rectangle rectangle = list2[Game1.random.Next(list2.Count)];
				Vector2 vector = new Vector2(Game1.random.Next(rectangle.X, rectangle.Right), Game1.random.Next(rectangle.Y, rectangle.Bottom));
				List<Vector2> list3 = Utility.recursiveFindOpenTiles(this, vector, 16);
				foreach (Vector2 item2 in list3)
				{
					string text = doesTileHaveProperty((int)item2.X, (int)item2.Y, "Diggable", "Back");
					if (!terrainFeatures.ContainsKey(item2) && text != null && Game1.random.NextDouble() < (double)(1f - Vector2.Distance(vector, item2) * 0.35f))
					{
						HoeDirt hoeDirt = new HoeDirt(0, new Crop(forageCrop: true, 2, (int)item2.X, (int)item2.Y));
						hoeDirt.state.Value = 2;
						terrainFeatures.Add(item2, hoeDirt);
					}
				}
			}
		}

		private bool isTileOpenForDigSiteStone(int tileX, int tileY)
		{
			if (doesTileHaveProperty(tileX, tileY, "Diggable", "Back") != null && doesTileHaveProperty(tileX, tileY, "Diggable", "Back") == "T")
			{
				return isTileLocationTotallyClearAndPlaceable(new Vector2(tileX, tileY));
			}
			return false;
		}

		public void digSiteUpdate()
		{
			bool flag = false;
			Random random = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + 78);
			Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle(4, 47, 22, 20);
			int num = 20;
			Vector2[] array = new Vector2[8]
			{
				new Vector2(18f, 49f),
				new Vector2(15f, 54f),
				new Vector2(21f, 52f),
				new Vector2(18f, 61f),
				new Vector2(23f, 57f),
				new Vector2(9f, 63f),
				new Vector2(7f, 51f),
				new Vector2(7f, 57f)
			};
			int numObjectsOfIndexWithinRectangle = Utility.getNumObjectsOfIndexWithinRectangle(r, new int[9] { 816, 817, 818, 819, 32, 38, 40, 42, 590 }, this);
			if (numObjectsOfIndexWithinRectangle < 60)
			{
				for (int i = 0; i < num; i++)
				{
					Vector2 randomPositionInThisRectangle = Utility.getRandomPositionInThisRectangle(r, Game1.random);
					Vector2 tileLocation = array[random.Next(array.Length)];
					if (!isTileOpenForDigSiteStone((int)randomPositionInThisRectangle.X, (int)randomPositionInThisRectangle.Y))
					{
						continue;
					}
					if (!flag || Game1.random.NextDouble() < 0.3)
					{
						flag = true;
						objects.Add(randomPositionInThisRectangle, new Object(randomPositionInThisRectangle, 816 + Game1.random.Next(2), 1)
						{
							MinutesUntilReady = 4
						});
					}
					else if (Game1.random.NextDouble() < 0.1)
					{
						int num2 = (int)randomPositionInThisRectangle.X;
						int num3 = (int)randomPositionInThisRectangle.Y;
						if (isTileLocationTotallyClearAndPlaceable(randomPositionInThisRectangle) && getTileIndexAt(num2, num3, "AlwaysFront") == -1 && getTileIndexAt(num2, num3, "Front") == -1 && !isBehindBush(randomPositionInThisRectangle) && doesTileHaveProperty(num2, num3, "Diggable", "Back") != null && doesTileHaveProperty(num2, num3, "Diggable", "Back") == "T")
						{
							objects.Add(randomPositionInThisRectangle, new Object(randomPositionInThisRectangle, 590, 1));
						}
					}
					else if (Game1.random.NextDouble() < 0.06)
					{
						terrainFeatures.Add(randomPositionInThisRectangle, new Tree(8, 1));
					}
					else if (Game1.random.NextDouble() < 0.2)
					{
						if (!isTileOpenForDigSiteStone((int)tileLocation.X, (int)tileLocation.Y))
						{
							continue;
						}
						int num4 = Game1.random.Next(2, 5);
						for (int j = 0; j < num4; j++)
						{
							Utility.spawnObjectAround(tileLocation, new Object(tileLocation, 818, 1)
							{
								MinutesUntilReady = 4
							}, this, playSound: false, delegate(Object o)
							{
								o.CanBeGrabbed = false;
								o.IsSpawnedObject = false;
							});
						}
					}
					else if (Game1.random.NextDouble() < 0.25)
					{
						objects.Add(randomPositionInThisRectangle, new Object(randomPositionInThisRectangle, (random.NextDouble() < 0.33) ? 785 : ((random.NextDouble() < 0.5) ? 676 : 677), 1));
					}
					else
					{
						Object @object = new Object(randomPositionInThisRectangle, (Game1.random.NextDouble() < 0.25) ? 32 : ((Game1.random.NextDouble() < 0.33) ? 38 : ((Game1.random.NextDouble() < 0.5) ? 40 : 42)), 1);
						@object.minutesUntilReady.Value = 2;
						@object.Name = "Stone";
						objects.Add(randomPositionInThisRectangle, @object);
					}
				}
				return;
			}
			numObjectsOfIndexWithinRectangle = Utility.getNumObjectsOfIndexWithinRectangle(r, new int[3] { 785, 676, 677 }, this);
			if (numObjectsOfIndexWithinRectangle >= 100)
			{
				return;
			}
			int num5 = random.Next(4);
			for (int k = 0; k < num5; k++)
			{
				Vector2 randomPositionInThisRectangle2 = Utility.getRandomPositionInThisRectangle(r, Game1.random);
				if (isTileOpenForDigSiteStone((int)randomPositionInThisRectangle2.X, (int)randomPositionInThisRectangle2.Y))
				{
					objects.Add(randomPositionInThisRectangle2, new Object(randomPositionInThisRectangle2, (random.NextDouble() < 0.33) ? 785 : ((random.NextDouble() < 0.5) ? 676 : 677), 1));
				}
			}
		}

		public override void performOrePanTenMinuteUpdate(Random r)
		{
			if (Game1.MasterPlayer.mailReceived.Contains("ccFishTank") && orePanPoint.Value.Equals(Point.Zero) && r.NextDouble() < 0.5)
			{
				for (int i = 0; i < 6; i++)
				{
					Point point = new Point(r.Next(4, 15), r.Next(45, 70));
					if (!isOpenWater(point.X, point.Y))
					{
						continue;
					}
					int num = FishingRod.distanceToLand(point.X, point.Y, this);
					if (num <= 1 && getTileIndexAt(point, "Buildings") == -1)
					{
						if (Game1.player.currentLocation.Equals(this))
						{
							playSound("slosh");
						}
						orePanPoint.Value = point;
						break;
					}
				}
			}
			else if (!orePanPoint.Value.Equals(Point.Zero) && r.NextDouble() < 0.1)
			{
				orePanPoint.Value = Point.Zero;
			}
		}

		public override bool performToolAction(Tool t, int tileX, int tileY)
		{
			if (!caveOpened && tileY == 47 && (tileX == 21 || tileX == 22))
			{
				boulderKnockTimer = 500f;
				Game1.playSound("hammer");
				boulderKnocksLeft = 0;
				doneHittingBoulderWithToolTimer = 1200f;
			}
			return base.performToolAction(t, tileX, tileY);
		}

		public override void explosionAt(float x, float y)
		{
			base.explosionAt(x, y);
			if (!caveOpened.Value && y == 47f && (x == 21f || x == 22f))
			{
				caveOpened.Value = true;
				Game1.addMailForTomorrow("islandNorthCaveOpened", noLetter: true, sendToEveryone: true);
			}
		}

		public override void drawBackground(SpriteBatch b)
		{
			base.drawBackground(b);
			DrawParallaxHorizon(b);
			if (!treeNutShot.Value)
			{
				b.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(58.25f, 10f) * 64f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 73, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (SuspensionBridge suspensionBridge in suspensionBridges)
			{
				suspensionBridge.Draw(b);
			}
			if (!caveOpened)
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Utility.PointToVector2(boulderPosition.Location) + new Vector2((boulderKnockTimer > 250f) ? Game1.random.Next(-1, 2) : 0, -64 + ((boulderKnockTimer > 250f) ? Game1.random.Next(-1, 2) : 0))), new Microsoft.Xna.Framework.Rectangle(155, 224, 32, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)boulderPosition.Y / 10000f);
			}
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			if (!caveOpened && boulderTextTimer > 0f)
			{
				SpriteText.drawStringWithScrollCenteredAt(b, boulderTextString, (int)Game1.GlobalToLocal(Utility.PointToVector2(boulderPosition.Location)).X + 64, (int)Game1.GlobalToLocal(Utility.PointToVector2(boulderPosition.Location)).Y - 128 - 32, "", 1f, -1, 1, 1f);
			}
		}

		public override bool isTileOccupiedForPlacement(Vector2 tileLocation, Object toPlace = null)
		{
			foreach (SuspensionBridge suspensionBridge in suspensionBridges)
			{
				if (suspensionBridge.CheckPlacementPrevention(tileLocation))
				{
					return true;
				}
			}
			return base.isTileOccupiedForPlacement(tileLocation, toPlace);
		}

		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (bridgeFixed.Value)
			{
				ApplyFixedBridge();
			}
			else
			{
				ApplyMapOverride("Island_Bridge_Broken", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(31, 52, 4, 3));
			}
			if (traderActivated.Value)
			{
				ApplyIslandTraderHut();
			}
			if (boulderRemoved.Value)
			{
				ApplyBoulderRemove();
			}
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (traderActivated.Value)
			{
				removeTemporarySpritesWithIDLocal(8989f);
				removeTemporarySpritesWithIDLocal(8988f);
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(33.45f, 70.33f) * 64f + new Vector2(-16f, -32f), flipped: false, 0f, Color.White)
				{
					delayBeforeAnimationStart = 10,
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					light = true,
					lightID = 8989,
					id = 8989f,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = 0.46144f
				});
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(39.45f, 70.33f) * 64f + new Vector2(-16f, -32f), flipped: false, 0f, Color.White)
				{
					delayBeforeAnimationStart = 10,
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					light = true,
					lightID = 8988,
					id = 8988f,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = 0.46144f
				});
			}
			if (caveOpened.Value && !Game1.player.hasOrWillReceiveMail("islandNorthCaveOpened"))
			{
				Game1.addMailForTomorrow("islandNorthCaveOpened", noLetter: true);
			}
			suspensionBridges.Clear();
			SuspensionBridge item = new SuspensionBridge(38, 39);
			suspensionBridges.Add(item);
			if (Game1.player.hasOrWillReceiveMail("Saw_Flame_Sprite_North_South"))
			{
				_sawFlameSpriteSouth = true;
			}
			if (Game1.player.hasOrWillReceiveMail("Saw_Flame_Sprite_North_North"))
			{
				_sawFlameSpriteNorth = true;
			}
			if (!_sawFlameSpriteSouth)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Monsters\\Magma Sprite", new Microsoft.Xna.Framework.Rectangle(0, 32, 16, 16), new Vector2(36f, 79f) * 64f, flipped: false, 0f, Color.White)
				{
					id = 999f,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 70f,
					light = true,
					lightRadius = 1f,
					animationLength = 7,
					layerDepth = 1f,
					yPeriodic = true,
					yPeriodicRange = 12f,
					yPeriodicLoopTime = 1000f,
					xPeriodic = true,
					xPeriodicRange = 16f,
					xPeriodicLoopTime = 1800f
				});
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\shadow", new Microsoft.Xna.Framework.Rectangle(0, 0, 12, 7), new Vector2(36.2f, 80.4f) * 64f, flipped: false, 0f, Color.White)
				{
					id = 998f,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 1000f,
					animationLength = 1,
					layerDepth = 0.001f,
					yPeriodic = true,
					yPeriodicRange = 1f,
					yPeriodicLoopTime = 1000f,
					xPeriodic = true,
					xPeriodicRange = 16f,
					xPeriodicLoopTime = 1800f
				});
			}
			if (!_sawFlameSpriteNorth)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Monsters\\Magma Sprite", new Microsoft.Xna.Framework.Rectangle(0, 32, 16, 16), new Vector2(41f, 30f) * 64f, flipped: false, 0f, Color.White)
				{
					id = 9999f,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 70f,
					light = true,
					lightRadius = 1f,
					animationLength = 7,
					layerDepth = 1f,
					yPeriodic = true,
					yPeriodicRange = 12f,
					yPeriodicLoopTime = 1000f,
					xPeriodic = true,
					xPeriodicRange = 16f,
					xPeriodicLoopTime = 1800f
				});
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\shadow", new Microsoft.Xna.Framework.Rectangle(0, 0, 12, 7), new Vector2(41.2f, 31.4f) * 64f, flipped: false, 0f, Color.White)
				{
					id = 9998f,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 1000f,
					animationLength = 1,
					layerDepth = 0.001f,
					yPeriodic = true,
					yPeriodicRange = 1f,
					yPeriodicLoopTime = 1000f,
					xPeriodic = true,
					xPeriodicRange = 16f,
					xPeriodicLoopTime = 1800f
				});
			}
			Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + 978);
			if (!Game1.player.team.collectedNutTracker.ContainsKey("Buried_IslandNorth_26_81"))
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(141, 310, 20, 23), new Vector2(23.75f, 77.15f) * 64f, flipped: false, 0f, Color.White)
				{
					totalNumberOfLoops = 999999,
					animationLength = 2,
					interval = 200f,
					id = 79797f,
					layerDepth = 1f,
					scale = 4f,
					drawAboveAlwaysFront = true
				});
			}
			else if (!Game1.IsRainingHere(this) && random.NextDouble() < 0.1)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(141, 310, 20, 23), new Vector2(23.75f, 77.15f) * 64f, flipped: false, 0f, Color.White)
				{
					totalNumberOfLoops = 999999,
					animationLength = 2,
					interval = 200f,
					layerDepth = 1f,
					scale = 4f,
					drawAboveAlwaysFront = true
				});
			}
		}

		public override void seasonUpdate(string season, bool onLoad = false)
		{
		}

		public override void updateSeasonalTileSheets(Map map = null)
		{
		}
	}
}
