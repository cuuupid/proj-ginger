using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class IslandWest : IslandLocation
	{
		[XmlElement("addedSlimesToday")]
		private readonly NetBool addedSlimesToday = new NetBool();

		[XmlElement("sandDuggy")]
		public NetRef<SandDuggy> sandDuggy = new NetRef<SandDuggy>();

		[XmlElement("farmhouseRestored")]
		public readonly NetBool farmhouseRestored = new NetBool();

		[XmlElement("farmhouseMailbox")]
		public readonly NetBool farmhouseMailbox = new NetBool();

		[XmlElement("farmObelisk")]
		public readonly NetBool farmObelisk = new NetBool();

		public Point shippingBinPosition = new Point(90, 39);

		private TemporaryAnimatedSprite shippingBinLid;

		private Microsoft.Xna.Framework.Rectangle shippingBinLidOpenArea;

		public override void SetBuriedNutLocations()
		{
			buriedNutPoints.Add(new Point(21, 81));
			buriedNutPoints.Add(new Point(62, 76));
			buriedNutPoints.Add(new Point(39, 24));
			buriedNutPoints.Add(new Point(88, 14));
			buriedNutPoints.Add(new Point(43, 74));
			buriedNutPoints.Add(new Point(30, 75));
			base.SetBuriedNutLocations();
		}

		public override bool CanPlantSeedsHere(int crop_index, int tile_x, int tile_y)
		{
			if (getTileSheetIDAt(tile_x, tile_y, "Back") == "untitled tile sheet2")
			{
				return true;
			}
			return base.CanPlantSeedsHere(crop_index, tile_x, tile_y);
		}

		public override bool SeedsIgnoreSeasonsHere()
		{
			return true;
		}

		public override bool CanPlantTreesHere(int sapling_index, int tile_x, int tile_y)
		{
			if (getTileSheetIDAt(tile_x, tile_y, "Back") == "untitled tile sheet2" || Object.isWildTreeSeed(sapling_index))
			{
				switch (doesTileHavePropertyNoNull(tile_x, tile_y, "Type", "Back"))
				{
				case "Dirt":
				case "Grass":
				case "":
					return true;
				}
			}
			return base.CanPlantTreesHere(sapling_index, tile_x, tile_y);
		}

		public IslandWest()
		{
		}

		public override int getFishingLocation(Vector2 tile)
		{
			if (tile.X > 35f && tile.Y < 81f)
			{
				return 2;
			}
			return 1;
		}

		public override bool catchOceanCrabPotFishFromThisSpot(int x, int y)
		{
			if (x > 38 && y < 85)
			{
				return false;
			}
			return true;
		}

		public override void digUpArtifactSpot(int xLocation, int yLocation, Farmer who)
		{
			Random random = new Random(xLocation * 2000 + yLocation * 77 + (int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + (int)Game1.stats.DirtHoed);
			if (Game1.player.hasOrWillReceiveMail("islandNorthCaveOpened"))
			{
				if (random.NextDouble() < 0.1)
				{
					Game1.createItemDebris(new Object(825, 1), new Vector2(xLocation, yLocation) * 64f, -1, this);
					return;
				}
				if (random.NextDouble() < 0.25)
				{
					Game1.createItemDebris(new Object(826, 1), new Vector2(xLocation, yLocation) * 64f, -1, this);
					return;
				}
			}
			base.digUpArtifactSpot(xLocation, yLocation, who);
		}

		public override Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			Random random = new Random((int)bobberTile.X * 2000 + (int)bobberTile.Y * 777 + (int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + (int)Game1.stats.TimesFished);
			if (Game1.player.hasOrWillReceiveMail("islandNorthCaveOpened") && random.NextDouble() < 0.1)
			{
				return new Object(825, 1);
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
		}

		public override bool performToolAction(Tool t, int tileX, int tileY)
		{
			if (sandDuggy.Value != null)
			{
				sandDuggy.Value.PerformToolAction(t, tileX, tileY);
			}
			return base.performToolAction(t, tileX, tileY);
		}

		public override List<Vector2> GetAdditionalWalnutBushes()
		{
			List<Vector2> list = new List<Vector2>();
			list.Add(new Vector2(54f, 18f));
			list.Add(new Vector2(25f, 30f));
			list.Add(new Vector2(15f, 3f));
			return list;
		}

		public override void draw(SpriteBatch b)
		{
			if (sandDuggy.Value != null)
			{
				sandDuggy.Value.Draw(b);
			}
			if (farmhouseRestored.Value && shippingBinLid != null)
			{
				shippingBinLid.draw(b);
			}
			if (farmhouseMailbox.Value && Game1.mailbox.Count > 0)
			{
				float num = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				Point point = new Point(81, 40);
				float num2 = (float)((point.X + 1) * 64) / 10000f + (float)(point.Y * 64) / 10000f;
				float num3 = -8f;
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(point.X * 64) + num3, (float)(point.Y * 64 - 96 - 48) + num)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, num2 + 1E-06f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(point.X * 64 + 32 + 4) + num3, (float)(point.Y * 64 - 64 - 24 - 8) + num)), new Microsoft.Xna.Framework.Rectangle(189, 423, 15, 13), Color.White, 0f, new Vector2(7f, 6f), 4f, SpriteEffects.None, num2 + 1E-05f);
			}
			base.draw(b);
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (sandDuggy.Value != null)
			{
				sandDuggy.Value.Update(time);
			}
			if (farmhouseRestored.Value && shippingBinLid != null)
			{
				bool flag = false;
				foreach (Farmer farmer in farmers)
				{
					if (farmer.GetBoundingBox().Intersects(shippingBinLidOpenArea))
					{
						openShippingBinLid();
						flag = true;
					}
				}
				if (!flag)
				{
					closeShippingBinLid();
				}
				updateShippingBinLid(time);
			}
			base.UpdateWhenCurrentLocation(time);
		}

		public IslandWest(string map, string name)
			: base(map, name)
		{
			sandDuggy.Value = new SandDuggy(this, new Point[4]
			{
				new Point(37, 87),
				new Point(41, 86),
				new Point(45, 86),
				new Point(48, 87)
			});
			parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(72, 37), new Microsoft.Xna.Framework.Rectangle(71, 29, 3, 8), 20, delegate
			{
				Game1.addMailForTomorrow("Island_W_Obelisk", noLetter: true, sendToEveryone: true);
				farmObelisk.Value = true;
			}, () => farmObelisk.Value, "Obelisk", "Island_UpgradeHouse_Mailbox"));
			parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(81, 40), new Microsoft.Xna.Framework.Rectangle(80, 39, 3, 2), 5, delegate
			{
				Game1.addMailForTomorrow("Island_UpgradeHouse_Mailbox", noLetter: true, sendToEveryone: true);
				farmhouseMailbox.Value = true;
			}, () => farmhouseMailbox.Value, "House_Mailbox", "Island_UpgradeHouse"));
			parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(81, 40), new Microsoft.Xna.Framework.Rectangle(74, 36, 7, 4), 20, delegate
			{
				Game1.addMailForTomorrow("Island_UpgradeHouse", noLetter: true, sendToEveryone: true);
				farmhouseRestored.Value = true;
			}, () => farmhouseRestored.Value, "House"));
			parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(72, 10), new Microsoft.Xna.Framework.Rectangle(73, 5, 3, 5), 10, delegate
			{
				Game1.addMailForTomorrow("Island_UpgradeParrotPlatform", noLetter: true, sendToEveryone: true);
				Game1.netWorldState.Value.ParrotPlatformsUnlocked.Value = true;
			}, () => Game1.netWorldState.Value.ParrotPlatformsUnlocked.Value, "ParrotPlatforms"));
		}

		public override bool performAction(string action, Farmer who, Location tileLocation)
		{
			if (action != null && action == "FarmObelisk")
			{
				for (int i = 0; i < 12; i++)
				{
					who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)who.Position.X - 256, (int)who.Position.X + 192), Game1.random.Next((int)who.Position.Y - 256, (int)who.Position.Y + 192)), flicker: false, Game1.random.NextDouble() < 0.5));
				}
				who.currentLocation.playSound("wand");
				Game1.displayFarmer = false;
				Game1.player.temporarilyInvincible = true;
				Game1.player.temporaryInvincibilityTimer = -2000;
				Game1.player.freezePause = 1000;
				Game1.flashAlpha = 1f;
				DelayedAction.fadeAfterDelay(delegate
				{
					int default_x = 48;
					int default_y = 7;
					if (Game1.whichFarm == 5)
					{
						default_x = 48;
						default_y = 39;
					}
					else if (Game1.whichFarm == 6)
					{
						default_x = 82;
						default_y = 29;
					}
					Point mapPropertyPosition = Game1.getFarm().GetMapPropertyPosition("WarpTotemEntry", default_x, default_y);
					Game1.warpFarmer("Farm", mapPropertyPosition.X, mapPropertyPosition.Y, flip: false);
					Game1.fadeToBlackAlpha = 0.99f;
					Game1.screenGlow = false;
					Game1.player.temporarilyInvincible = false;
					Game1.player.temporaryInvincibilityTimer = 0;
					Game1.displayFarmer = true;
				}, 1000);
				new Microsoft.Xna.Framework.Rectangle(who.GetBoundingBox().X, who.GetBoundingBox().Y, 64, 64).Inflate(192, 192);
				int num = 0;
				for (int num2 = who.getTileX() + 8; num2 >= who.getTileX() - 8; num2--)
				{
					who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(num2, who.getTileY()) * 64f, Color.White, 8, flipped: false, 50f)
					{
						layerDepth = 1f,
						delayBeforeAnimationStart = num * 25,
						motion = new Vector2(-0.25f, 0f)
					});
					num++;
				}
				return true;
			}
			return base.performAction(action, who, tileLocation);
		}

		public override bool leftClick(int x, int y, Farmer who)
		{
			if (farmhouseRestored.Value && who.ActiveObject != null && x / 64 >= shippingBinPosition.X && x / 64 <= shippingBinPosition.X + 1 && y / 64 >= shippingBinPosition.Y - 1 && y / 64 <= shippingBinPosition.Y && who.ActiveObject.canBeShipped() && Vector2.Distance(who.getTileLocation(), new Vector2((float)shippingBinPosition.X + 0.5f, shippingBinPosition.Y)) <= 2f)
			{
				Game1.getFarm().getShippingBin(who).Add(who.ActiveObject);
				Game1.getFarm().lastItemShipped = who.ActiveObject;
				who.showNotCarrying();
				showShipment(who.ActiveObject);
				who.ActiveObject = null;
				return true;
			}
			return base.leftClick(x, y, who);
		}

		public void showShipment(Object o, bool playThrowSound = true)
		{
			if (playThrowSound)
			{
				localSound("backpackIN");
			}
			DelayedAction.playSoundAfterDelay("Ship", playThrowSound ? 250 : 0);
			int num = Game1.random.Next();
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(524, 218, 34, 22), new Vector2(90f, 38f) * 64f + new Vector2(0f, 5f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = 100f,
				totalNumberOfLoops = 1,
				animationLength = 3,
				pingPong = true,
				scale = 4f,
				layerDepth = 0.25601003f,
				id = num,
				extraInfoForEndBehavior = num,
				endFunction = base.removeTemporarySpritesWithID
			});
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(524, 230, 34, 10), new Vector2(90f, 38f) * 64f + new Vector2(0f, 17f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = 100f,
				totalNumberOfLoops = 1,
				animationLength = 3,
				pingPong = true,
				scale = 4f,
				layerDepth = 0.2563f,
				id = num,
				extraInfoForEndBehavior = num
			});
			temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, o.parentSheetIndex, 16, 16), new Vector2(90f, 38f) * 64f + new Vector2(8 + Game1.random.Next(6), 2f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = 9999f,
				scale = 4f,
				alphaFade = 0.045f,
				layerDepth = 0.25622502f,
				motion = new Vector2(0f, 0.3f),
				acceleration = new Vector2(0f, 0.2f),
				scaleChange = -0.05f
			});
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (farmhouseRestored.Value && tileLocation.X >= shippingBinPosition.X && tileLocation.X <= shippingBinPosition.X + 1 && tileLocation.Y >= shippingBinPosition.Y - 1 && tileLocation.Y <= shippingBinPosition.Y)
			{
				ItemGrabMenu itemGrabMenu = new ItemGrabMenu(null, reverseGrab: true, showReceivingMenu: false, Utility.highlightShippableObjects, Game1.getFarm().shipItem, "", null, snapToBottom: true, canBeExitedWithKey: true, playRightClickSound: false, allowRightClick: true, showOrganizeButton: false, 0, null, -1, null, -1, 3, null, allowStack: true, null, rearrangeGrangeOnExit: false, null, this);
				itemGrabMenu.initializeUpperRightCloseButton();
				itemGrabMenu.setBackgroundTransparency(b: false);
				itemGrabMenu.setDestroyItemOnClick(b: true);
				itemGrabMenu.initializeShippingBin();
				Game1.activeClickableMenu = itemGrabMenu;
				playSound("shwip");
				if (Game1.player.FacingDirection == 1)
				{
					Game1.player.Halt();
				}
				Game1.player.showCarrying();
				return true;
			}
			if (getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings") != -1)
			{
				int tileIndexAt = getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings");
				if (tileIndexAt == 1470)
				{
					int num = Math.Max(0, Game1.netWorldState.Value.GoldenWalnutsFound.Value - 1);
					if (num < 100)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:qiNutDoor", num));
					}
					else
					{
						Game1.playSound("doorClose");
						Game1.warpFarmer("QiNutRoom", 7, 8, 0);
					}
					return true;
				}
			}
			if (getCharacterFromName("Birdie") != null && !getCharacterFromName("Birdie").IsInvisible && (getCharacterFromName("Birdie").getTileLocation().Equals(new Vector2(tileLocation.X, tileLocation.Y)) || getCharacterFromName("Birdie").getTileLocation().Equals(new Vector2(tileLocation.X - 1, tileLocation.Y))))
			{
				if (!who.mailReceived.Contains("birdieQuestBegun"))
				{
					who.Halt();
					Game1.globalFadeToBlack(delegate
					{
						startEvent(new Event(Game1.content.LoadString("Strings\\Locations:IslandSecret_Event_BirdieIntro"), -888999));
					});
					who.mailReceived.Add("birdieQuestBegun");
					return true;
				}
				if (who.hasQuest(130) && !who.mailReceived.Contains("birdieQuestFinished") && who.ActiveObject != null && Utility.IsNormalObjectAtParentSheetIndex(who.ActiveObject, 870))
				{
					who.Halt();
					Game1.globalFadeToBlack(delegate
					{
						who.reduceActiveItemByOne();
						startEvent(new Event(Game1.content.LoadString("Strings\\Locations:IslandSecret_Event_BirdieFinished"), -666777));
					});
					who.mailReceived.Add("birdieQuestFinished");
					return true;
				}
				if (who.mailReceived.Contains("birdieQuestFinished"))
				{
					if (who.ActiveObject != null)
					{
						Game1.drawDialogue(getCharacterFromName("Birdie"), Utility.loadStringDataShort("ExtraDialogue", "Birdie_NoGift"));
					}
					else
					{
						string text = null;
						try
						{
							text = Game1.content.LoadStringReturnNullIfNotFound("Data\\ExtraDialogue:Birdie" + Game1.dayOfMonth);
						}
						catch (Exception)
						{
						}
						if (text != null && text.Length > 0)
						{
							Game1.drawDialogue(getCharacterFromName("Birdie"), text);
						}
						else
						{
							Game1.drawDialogue(getCharacterFromName("Birdie"), Utility.loadStringDataShort("ExtraDialogue", "Birdie" + Game1.dayOfMonth % 7));
						}
					}
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public override bool isActionableTile(int xTile, int yTile, Farmer who)
		{
			if (!Game1.eventUp)
			{
				NPC characterFromName = getCharacterFromName("Birdie");
				if (characterFromName != null && !characterFromName.IsInvisible && characterFromName.getTileLocation().Equals(new Vector2(xTile - 1, yTile)) && (!who.mailReceived.Contains("birdieQuestBegun") || who.mailReceived.Contains("birdieQuestFinished")))
				{
					Game1.isSpeechAtCurrentCursorTile = true;
					return true;
				}
			}
			return base.isActionableTile(xTile, yTile, who);
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(addedSlimesToday);
			base.NetFields.AddField(farmhouseRestored);
			base.NetFields.AddField(sandDuggy);
			base.NetFields.AddField(farmhouseMailbox);
			base.NetFields.AddField(farmObelisk);
			farmhouseRestored.InterpolationWait = false;
			farmhouseRestored.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && mapPath.Value != null)
				{
					ApplyFarmHouseRestore();
				}
			};
			farmhouseMailbox.InterpolationWait = false;
			farmhouseMailbox.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && mapPath.Value != null)
				{
					ApplyFarmHouseRestore();
				}
			};
			farmObelisk.InterpolationWait = false;
			farmObelisk.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (newValue && mapPath.Value != null)
				{
					ApplyFarmObeliskBuild();
				}
			};
		}

		public void ApplyFarmObeliskBuild()
		{
			if (map != null && !_appliedMapOverrides.Contains("Island_W_Obelisk"))
			{
				ApplyMapOverride("Island_W_Obelisk", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(71, 29, 3, 9));
			}
		}

		public void ApplyFarmHouseRestore()
		{
			if (map != null)
			{
				if (!_appliedMapOverrides.Contains("Island_House_Restored"))
				{
					ApplyMapOverride("Island_House_Restored", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(74, 33, 7, 9));
					ApplyMapOverride("Island_House_Bin", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(shippingBinPosition.X, shippingBinPosition.Y - 1, 2, 2));
					ApplyMapOverride("Island_House_Cave", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(95, 30, 3, 4));
				}
				if (farmhouseMailbox.Value)
				{
					setMapTileIndex(81, 40, 771, "Buildings");
					setMapTileIndex(81, 39, 739, "Front");
					setTileProperty(81, 40, "Buildings", "Action", "Mailbox");
				}
			}
		}

		public override void seasonUpdate(string season, bool onLoad = false)
		{
		}

		public override void updateSeasonalTileSheets(Map map = null)
		{
		}

		public override void monsterDrop(Monster monster, int x, int y, Farmer who)
		{
			base.monsterDrop(monster, x, y, who);
			if (Game1.MasterPlayer.hasOrWillReceiveMail("tigerSlimeNut"))
			{
				return;
			}
			int num = 0;
			foreach (NPC character in characters)
			{
				if (character is GreenSlime && character.name == "Tiger Slime")
				{
					num++;
				}
			}
			if (num == 1)
			{
				Game1.addMailForTomorrow("tigerSlimeNut", noLetter: true, sendToEveryone: true);
				Game1.player.team.RequestLimitedNutDrops("TigerSlimeNut", this, x, y, 1);
			}
		}

		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			if (l is IslandWest)
			{
				IslandWest islandWest = l as IslandWest;
				farmhouseRestored.Value = islandWest.farmhouseRestored;
				farmhouseMailbox.Value = islandWest.farmhouseMailbox;
				farmObelisk.Value = islandWest.farmObelisk.Value;
				sandDuggy.Value.whacked.Value = islandWest.sandDuggy.Value.whacked.Value;
				List<ResourceClump> other = new List<ResourceClump>(islandWest.resourceClumps);
				islandWest.resourceClumps.Clear();
				resourceClumps.Set(other);
			}
			base.TransferDataFromSavedLocation(l);
		}

		public override void spawnObjects()
		{
			base.spawnObjects();
			Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle(57, 78, 43, 8);
			if (Utility.getNumObjectsOfIndexWithinRectangle(r, new int[1] { 25 }, this) < 10)
			{
				Vector2 randomPositionInThisRectangle = Utility.getRandomPositionInThisRectangle(r, Game1.random);
				if (isTileLocationTotallyClearAndPlaceable((int)randomPositionInThisRectangle.X, (int)randomPositionInThisRectangle.Y))
				{
					objects.Add(randomPositionInThisRectangle, new Object(randomPositionInThisRectangle, 25, "Stone", canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
					{
						MinutesUntilReady = 8,
						Flipped = (Game1.random.NextDouble() < 0.5)
					});
				}
			}
			Microsoft.Xna.Framework.Rectangle r2 = new Microsoft.Xna.Framework.Rectangle(20, 71, 28, 16);
			if (Utility.getNumObjectsOfIndexWithinRectangle(r2, new int[2] { 393, 397 }, this) < 5)
			{
				Vector2 randomPositionInThisRectangle2 = Utility.getRandomPositionInThisRectangle(r2, Game1.random);
				if (isTileLocationTotallyClearAndPlaceable((int)randomPositionInThisRectangle2.X, (int)randomPositionInThisRectangle2.Y))
				{
					objects.Add(randomPositionInThisRectangle2, new Object(randomPositionInThisRectangle2, (Game1.random.NextDouble() < 0.1) ? 397 : 393, 1)
					{
						IsSpawnedObject = true,
						CanBeGrabbed = true
					});
				}
			}
		}

		public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
		{
			if (xLocation == 18 && yLocation == 42 && who.secretNotesSeen.Contains(1004))
			{
				Game1.player.team.RequestLimitedNutDrops("Island_W_BuriedTreasureNut", this, xLocation * 64, yLocation * 64, 1);
				if (!Game1.player.hasOrWillReceiveMail("Island_W_BuriedTreasure"))
				{
					Game1.createItemDebris(new Object(877, 1), new Vector2(xLocation, yLocation) * 64f, 1);
					Game1.addMailForTomorrow("Island_W_BuriedTreasure", noLetter: true);
				}
			}
			else if (xLocation == 104 && yLocation == 74 && who.secretNotesSeen.Contains(1006))
			{
				Game1.player.team.RequestLimitedNutDrops("Island_W_BuriedTreasureNut2", this, xLocation * 64, yLocation * 64, 1);
				if (!Game1.player.hasOrWillReceiveMail("Island_W_BuriedTreasure2"))
				{
					Game1.createItemDebris(new Object(797, 1), new Vector2(xLocation, yLocation) * 64f, 1);
					Game1.addMailForTomorrow("Island_W_BuriedTreasure2", noLetter: true);
				}
			}
			return base.checkForBuriedItem(xLocation, yLocation, explosion, detectOnly, who);
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			ICollection<Vector2> collection = new List<Vector2>(terrainFeatures.Keys);
			for (int num = collection.Count - 1; num >= 0; num--)
			{
				if (terrainFeatures[collection.ElementAt(num)] is HoeDirt && (terrainFeatures[collection.ElementAt(num)] as HoeDirt).crop == null && Game1.random.NextDouble() <= 0.1)
				{
					terrainFeatures.Remove(collection.ElementAt(num));
				}
			}
			for (int i = 0; i < characters.Count; i++)
			{
				if (characters[i] != null && characters[i] is Monster)
				{
					characters.RemoveAt(i);
					i--;
				}
			}
			addedSlimesToday.Value = false;
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
			list2.Add(new Microsoft.Xna.Framework.Rectangle(31, 43, 7, 6));
			list2.Add(new Microsoft.Xna.Framework.Rectangle(37, 62, 6, 5));
			list2.Add(new Microsoft.Xna.Framework.Rectangle(48, 42, 5, 4));
			list2.Add(new Microsoft.Xna.Framework.Rectangle(71, 12, 5, 4));
			list2.Add(new Microsoft.Xna.Framework.Rectangle(50, 59, 1, 1));
			list2.Add(new Microsoft.Xna.Framework.Rectangle(47, 64, 1, 1));
			list2.Add(new Microsoft.Xna.Framework.Rectangle(36, 58, 1, 1));
			list2.Add(new Microsoft.Xna.Framework.Rectangle(56, 48, 1, 1));
			list2.Add(new Microsoft.Xna.Framework.Rectangle(29, 46, 1, 1));
			for (int j = 0; j < 5; j++)
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
			if (Game1.MasterPlayer.mailReceived.Contains("Island_Turtle"))
			{
				spawnWeedsAndStones(20, weedsOnly: true);
				if (Game1.dayOfMonth % 7 == 1)
				{
					spawnWeedsAndStones(20, weedsOnly: true, spawnFromOldWeeds: false);
				}
			}
		}

		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (farmhouseRestored.Value)
			{
				ApplyFarmHouseRestore();
			}
			if (farmObelisk.Value)
			{
				ApplyFarmObeliskBuild();
			}
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			shippingBinLidOpenArea = new Microsoft.Xna.Framework.Rectangle((shippingBinPosition.X - 1) * 64, (shippingBinPosition.Y - 1) * 64, 256, 192);
			shippingBinLid = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(134, 226, 30, 25), new Vector2(shippingBinPosition.X, shippingBinPosition.Y - 1) * 64f + new Vector2(2f, -7f) * 4f, flipped: false, 0f, Color.White)
			{
				holdLastFrame = true,
				destroyable = false,
				interval = 20f,
				animationLength = 13,
				paused = true,
				scale = 4f,
				layerDepth = (float)((shippingBinPosition.Y + 1) * 64) / 10000f + 0.0001f,
				pingPong = true,
				pingPongMotion = 0
			};
			if (sandDuggy.Value != null)
			{
				sandDuggy.Value.ResetForPlayerEntry();
			}
			NPC characterFromName = getCharacterFromName("Birdie");
			if (characterFromName != null)
			{
				if (characterFromName.Sprite.SourceRect.Width < 32)
				{
					characterFromName.extendSourceRect(16, 0);
				}
				characterFromName.Sprite.SpriteWidth = 32;
				characterFromName.Sprite.ignoreSourceRectUpdates = false;
				characterFromName.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(8, 1000, 0, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(9, 1000, 0, secondaryArm: false, flip: false)
				});
				characterFromName.Sprite.loop = true;
				characterFromName.HideShadow = true;
				characterFromName.IsInvisible = Game1.IsRainingHere(this);
			}
			if (Game1.timeOfDay > 1700)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(23f, 58f) * 64f + new Vector2(-16f, -32f), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					light = true,
					lightID = 987654,
					id = 987654f,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = 0.37824f
				});
				AmbientLocationSounds.addSound(new Vector2(23f, 58f), 1);
			}
			if (Game1.currentSeason == "winter" && !Game1.IsRainingHere(this) && Game1.isDarkOut())
			{
				addMoonlightJellies(100, new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame - 24917), new Microsoft.Xna.Framework.Rectangle(35, 0, 60, 60));
			}
		}

		protected override void resetSharedState()
		{
			base.resetSharedState();
			if ((bool)addedSlimesToday)
			{
				return;
			}
			addedSlimesToday.Value = true;
			Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame + 12);
			Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle(28, 24, 19, 8);
			for (int num = 5; num > 0; num--)
			{
				Vector2 randomPositionInThisRectangle = Utility.getRandomPositionInThisRectangle(r, random);
				if (isTileLocationTotallyClearAndPlaceable(randomPositionInThisRectangle))
				{
					GreenSlime greenSlime = new GreenSlime(randomPositionInThisRectangle * 64f, 0);
					greenSlime.makeTigerSlime();
					characters.Add(greenSlime);
				}
			}
		}

		private void openShippingBinLid()
		{
			if (shippingBinLid != null)
			{
				if (shippingBinLid.pingPongMotion != 1 && Game1.currentLocation == this)
				{
					localSound("doorCreak");
				}
				shippingBinLid.pingPongMotion = 1;
				shippingBinLid.paused = false;
			}
		}

		private void closeShippingBinLid()
		{
			if (shippingBinLid != null && shippingBinLid.currentParentTileIndex > 0)
			{
				if (shippingBinLid.pingPongMotion != -1 && Game1.currentLocation == this)
				{
					localSound("doorCreakReverse");
				}
				shippingBinLid.pingPongMotion = -1;
				shippingBinLid.paused = false;
			}
		}

		private void updateShippingBinLid(GameTime time)
		{
			if (isShippingBinLidOpen(requiredToBeFullyOpen: true) && shippingBinLid.pingPongMotion == 1)
			{
				shippingBinLid.paused = true;
			}
			else if (shippingBinLid.currentParentTileIndex == 0 && shippingBinLid.pingPongMotion == -1)
			{
				if (!shippingBinLid.paused && Game1.currentLocation == this)
				{
					localSound("woodyStep");
				}
				shippingBinLid.paused = true;
			}
			shippingBinLid.update(time);
		}

		private bool isShippingBinLidOpen(bool requiredToBeFullyOpen = false)
		{
			if (shippingBinLid != null && shippingBinLid.currentParentTileIndex >= ((!requiredToBeFullyOpen) ? 1 : (shippingBinLid.animationLength - 1)))
			{
				return true;
			}
			return false;
		}
	}
}
