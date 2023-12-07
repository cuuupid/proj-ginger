using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Dimensions;
using xTile.Tiles;

namespace StardewValley.Locations
{
	public class Town : GameLocation
	{
		private TemporaryAnimatedSprite minecartSteam;

		private bool ccRefurbished;

		private bool ccJoja;

		private bool playerCheckedBoard;

		private bool isShowingDestroyedJoja;

		private bool isShowingUpgradedPamHouse;

		private bool isShowingSpecialOrdersBoard;

		private LocalizedContentManager mapLoader;

		[XmlElement("daysUntilCommunityUpgrade")]
		public readonly NetInt daysUntilCommunityUpgrade = new NetInt(0);

		private NetArray<bool, NetBool> garbageChecked = new NetArray<bool, NetBool>(8);

		private Vector2 clockCenter = new Vector2(3392f, 1056f);

		private Vector2 ccFacadePosition = new Vector2(3044f, 940f);

		private Vector2 ccFacadePositionBottom = new Vector2(3044f, 1140f);

		public static Microsoft.Xna.Framework.Rectangle minuteHandSource = new Microsoft.Xna.Framework.Rectangle(363, 395, 5, 13);

		public static Microsoft.Xna.Framework.Rectangle hourHandSource = new Microsoft.Xna.Framework.Rectangle(369, 399, 5, 9);

		public static Microsoft.Xna.Framework.Rectangle clockNub = new Microsoft.Xna.Framework.Rectangle(375, 404, 4, 4);

		public static Microsoft.Xna.Framework.Rectangle jojaFacadeTop = new Microsoft.Xna.Framework.Rectangle(424, 1275, 174, 50);

		public static Microsoft.Xna.Framework.Rectangle jojaFacadeBottom = new Microsoft.Xna.Framework.Rectangle(424, 1325, 174, 51);

		public static Microsoft.Xna.Framework.Rectangle jojaFacadeWinterOverlay = new Microsoft.Xna.Framework.Rectangle(66, 1678, 174, 25);

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(daysUntilCommunityUpgrade, garbageChecked);
		}

		public Town()
		{
		}

		public Town(string map, string name)
			: base(map, name)
		{
		}

		protected override LocalizedContentManager getMapLoader()
		{
			if (mapLoader == null)
			{
				mapLoader = Game1.game1.xTileContent.CreateTemporary();
			}
			return mapLoader;
		}

		public override void UpdateMapSeats()
		{
			base.UpdateMapSeats();
			if (!Game1.IsMasterGame)
			{
				return;
			}
			for (int num = mapSeats.Count - 1; num >= 0; num--)
			{
				if (mapSeats[num].tilePosition.Value.X == 24f && mapSeats[num].tilePosition.Value.Y == 13f && mapSeats[num].seatType.Value == "swings")
				{
					mapSeats.RemoveAt(num);
				}
			}
		}

		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			if (!Game1.isStartingToGetDarkOut())
			{
				addClintMachineGraphics();
			}
			else
			{
				AmbientLocationSounds.removeSound(new Vector2(100f, 79f));
			}
		}

		public void checkedBoard()
		{
			playerCheckedBoard = true;
		}

		private void addClintMachineGraphics()
		{
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(302, 1946, 15, 16), 7000 - Game1.gameTimeInterval, 1, 1, new Vector2(100f, 79f) * 64f + new Vector2(9f, 6f) * 4f, flicker: false, flipped: false, 0.5188f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				shakeIntensity = 1f
			});
			for (int i = 0; i < 10; i++)
			{
				Utility.addSmokePuff(this, new Vector2(101f, 78f) * 64f + new Vector2(4f, 4f) * 4f, i * ((7000 - Game1.gameTimeInterval) / 16));
			}
			Vector2 vector = new Vector2(643f, 1305f);
			if (Game1.currentSeason.Equals("fall"))
			{
				vector = new Vector2(304f, 256f);
			}
			for (int j = 0; j < Game1.random.Next(1, 4); j++)
			{
				for (int k = 0; k < 16; k++)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle((int)vector.X, (int)vector.Y, 5, 18), 50f, 4, 1, new Vector2(100f, 78f) * 64f + new Vector2(-5 - k * 4, 0f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = j * 1500 + 100 * k
					});
				}
				Utility.addSmokePuff(this, new Vector2(100f, 78f) * 64f + new Vector2(-70f, -6f) * 4f, j * 1500 + 1600);
			}
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			for (int i = 0; i < garbageChecked.Length; i++)
			{
				garbageChecked[i] = false;
			}
			if (Game1.dayOfMonth == 2 && Game1.currentSeason.Equals("spring") && !Game1.MasterPlayer.mailReceived.Contains("JojaMember") && !isTileOccupiedForPlacement(new Vector2(57f, 16f)))
			{
				objects.Add(new Vector2(57f, 16f), new Object(Vector2.Zero, 55));
			}
			if (daysUntilCommunityUpgrade.Value <= 0)
			{
				return;
			}
			daysUntilCommunityUpgrade.Value--;
			if (daysUntilCommunityUpgrade.Value <= 0)
			{
				if (!Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					Game1.MasterPlayer.mailReceived.Add("pamHouseUpgrade");
					Game1.player.changeFriendship(1000, Game1.getCharacterFromName("Pam"));
				}
				else
				{
					Game1.MasterPlayer.mailReceived.Add("communityUpgradeShortcuts");
				}
			}
		}

		public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
		{
			if (who.secretNotesSeen.Contains(17) && xLocation == 98 && yLocation == 4 && !who.mailReceived.Contains("SecretNote17_done"))
			{
				who.mailReceived.Add("SecretNote17_done");
				Game1.createObjectDebris(126, xLocation, yLocation, who.UniqueMultiplayerID, this);
				return "";
			}
			return base.checkForBuriedItem(xLocation, yLocation, explosion, detectOnly, who);
		}

		public override bool CanPlantTreesHere(int sapling_index, int tile_x, int tile_y)
		{
			if (Object.isWildTreeSeed(sapling_index))
			{
				return doesTileHavePropertyNoNull(tile_x, tile_y, "Type", "Back") == "Dirt";
			}
			return false;
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (map.GetLayer("Buildings").Tiles[tileLocation] != null && who.mount == null)
			{
				switch (map.GetLayer("Buildings").Tiles[tileLocation].TileIndex)
				{
				case 78:
				{
					string text = doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings");
					int num = ((text != null) ? Convert.ToInt32(text.Split(' ')[1]) : (-1));
					if (num < 0 || num >= garbageChecked.Length)
					{
						break;
					}
					if (!garbageChecked[num])
					{
						garbageChecked[num] = true;
						Random random = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + 777 + num * 77);
						int num2 = random.Next(0, 100);
						for (int i = 0; i < num2; i++)
						{
							random.NextDouble();
						}
						num2 = random.Next(0, 100);
						for (int j = 0; j < num2; j++)
						{
							random.NextDouble();
						}
						Game1.stats.incrementStat("trashCansChecked", 1);
						int num3 = Utility.getSeasonNumber(Game1.currentSeason) * 17;
						bool flag = Game1.stats.getStat("trashCansChecked") > 20 && random.NextDouble() < 0.01;
						bool flag2 = Game1.stats.getStat("trashCansChecked") > 20 && random.NextDouble() < 0.002;
						if (flag2)
						{
							playSound("explosion");
						}
						else if (flag)
						{
							playSound("crit");
						}
						List<TemporaryAnimatedSprite> list = new List<TemporaryAnimatedSprite>();
						list.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(22 + num3, 0, 16, 10), new Vector2(tileLocation.X, tileLocation.Y) * 64f + new Vector2(0f, -6f) * 4f, flipped: false, 0f, Color.White)
						{
							interval = (flag2 ? 4000 : 1000),
							motion = (flag2 ? new Vector2(4f, -20f) : new Vector2(0f, -8f + (flag ? (-7f) : ((float)(Game1.random.Next(-1, 3) + ((Game1.random.NextDouble() < 0.1) ? (-2) : 0)))))),
							rotationChange = (flag2 ? 0.4f : 0f),
							acceleration = new Vector2(0f, 0.7f),
							yStopCoordinate = tileLocation.Y * 64 + -24,
							layerDepth = (flag2 ? 1f : ((float)((tileLocation.Y + 1) * 64 + 2) / 10000f)),
							scale = 4f,
							Parent = this,
							shakeIntensity = (flag2 ? 0f : 1f),
							reachedStopCoordinate = delegate
							{
								removeTemporarySpritesWithID(97654);
								playSound("thudStep");
								for (int l = 0; l < 3; l++)
								{
									temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(tileLocation.X, tileLocation.Y) * 64f + new Vector2(l * 6, -3 + Game1.random.Next(3)) * 4f, flipped: false, 0.02f, Color.DimGray)
									{
										alpha = 0.85f,
										motion = new Vector2(-0.6f + (float)l * 0.3f, -1f),
										acceleration = new Vector2(0.002f, 0f),
										interval = 99999f,
										layerDepth = (float)((tileLocation.Y + 1) * 64 + 3) / 10000f,
										scale = 3f,
										scaleChange = 0.02f,
										rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
										delayBeforeAnimationStart = 50
									});
								}
							},
							id = 97654f
						});
						if (flag2)
						{
							list.Last().reachedStopCoordinate = list.Last().bounce;
						}
						list.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(22 + num3, 11, 16, 16), new Vector2(tileLocation.X, tileLocation.Y) * 64f + new Vector2(0f, -5f) * 4f, flipped: false, 0f, Color.White)
						{
							interval = (flag2 ? 999999 : 1000),
							layerDepth = (float)((tileLocation.Y + 1) * 64 + 1) / 10000f,
							scale = 4f,
							id = 97654f
						});
						for (int k = 0; k < 5; k++)
						{
							list.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(22 + Game1.random.Next(4) * 4, 32, 4, 4), new Vector2(tileLocation.X, tileLocation.Y) * 64f + new Vector2(Game1.random.Next(13), -3 + Game1.random.Next(3)) * 4f, flipped: false, 0f, Color.White)
							{
								interval = 500f,
								motion = new Vector2(Game1.random.Next(-2, 3), -5f),
								acceleration = new Vector2(0f, 0.4f),
								layerDepth = (float)((tileLocation.Y + 1) * 64 + 3) / 10000f,
								scale = 4f,
								color = Utility.getRandomRainbowColor(),
								delayBeforeAnimationStart = Game1.random.Next(100)
							});
						}
						Game1.multiplayer.broadcastSprites(this, list);
						playSound("trashcan");
						Character character = Utility.isThereAFarmerOrCharacterWithinDistance(new Vector2(tileLocation.X, tileLocation.Y), 7, this);
						if (character != null && character is NPC && !(character is Horse))
						{
							Game1.multiplayer.globalChatInfoMessage("TrashCan", Game1.player.Name, character.name);
							if (character.name.Equals("Linus"))
							{
								character.doEmote(32);
								(character as NPC).setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Linus"), add: true, clearOnMovement: true);
								who.changeFriendship(5, character as NPC);
								Game1.multiplayer.globalChatInfoMessage("LinusTrashCan");
							}
							else if ((character as NPC).Age == 2)
							{
								character.doEmote(28);
								(character as NPC).setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Child"), add: true, clearOnMovement: true);
								who.changeFriendship(-25, character as NPC);
							}
							else if ((character as NPC).Age == 1)
							{
								character.doEmote(8);
								(character as NPC).setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Teen"), add: true, clearOnMovement: true);
								who.changeFriendship(-25, character as NPC);
							}
							else
							{
								character.doEmote(12);
								(character as NPC).setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Adult"), add: true, clearOnMovement: true);
								who.changeFriendship(-25, character as NPC);
							}
							Game1.drawDialogue(character as NPC);
						}
						if (flag2)
						{
							who.addItemByMenuIfNecessary(new Hat(66));
						}
						else if (flag || random.NextDouble() < 0.2 + who.DailyLuck)
						{
							int parentSheetIndex = 168;
							int num4 = 1;
							switch (random.Next(10))
							{
							case 0:
								parentSheetIndex = 168;
								break;
							case 1:
								parentSheetIndex = 167;
								break;
							case 2:
								parentSheetIndex = 170;
								break;
							case 3:
								parentSheetIndex = 171;
								break;
							case 4:
								parentSheetIndex = 172;
								break;
							case 5:
								parentSheetIndex = 216;
								break;
							case 6:
								parentSheetIndex = Utility.getRandomItemFromSeason(Game1.currentSeason, tileLocation.X * 653 + tileLocation.Y * 777, forQuest: false);
								break;
							case 7:
								parentSheetIndex = 403;
								break;
							case 8:
								parentSheetIndex = 309 + random.Next(3);
								break;
							case 9:
								parentSheetIndex = 153;
								break;
							}
							if (num == 3 && random.NextDouble() < 0.2 + who.DailyLuck)
							{
								parentSheetIndex = 535;
								if (random.NextDouble() < 0.05)
								{
									parentSheetIndex = 749;
								}
							}
							if (num == 4 && random.NextDouble() < 0.2 + who.DailyLuck)
							{
								parentSheetIndex = 378 + random.Next(3) * 2;
								num4 = random.Next(1, 5);
							}
							if (num == 5 && random.NextDouble() < 0.2 + who.DailyLuck && Game1.dishOfTheDay != null)
							{
								parentSheetIndex = (((int)Game1.dishOfTheDay.parentSheetIndex != 217) ? ((int)Game1.dishOfTheDay.parentSheetIndex) : 216);
							}
							if (num == 6 && random.NextDouble() < 0.2 + who.DailyLuck)
							{
								parentSheetIndex = 223;
							}
							if (num == 7 && random.NextDouble() < 0.2)
							{
								if (!Utility.HasAnyPlayerSeenEvent(191393))
								{
									parentSheetIndex = 167;
								}
								if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater") && !Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheaterJoja"))
								{
									parentSheetIndex = ((!(random.NextDouble() < 0.25)) ? 270 : 809);
								}
							}
							if (Game1.random.NextDouble() <= 0.25 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
							{
								parentSheetIndex = 890;
							}
							Vector2 origin = new Vector2((float)tileLocation.X + 0.5f, tileLocation.Y - 1) * 64f;
							Game1.createItemDebris(new Object(parentSheetIndex, 1), origin, 2, this, (int)origin.Y + 64);
						}
					}
					Game1.haltAfterCheck = false;
					return true;
				}
				case 620:
					if (Utility.HasAnyPlayerSeenEvent(191393))
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_SeedShopSign").Replace('\n', '^'));
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_SeedShopSign").Split('\n').First() + "^" + Game1.content.LoadString("Strings\\Locations:SeedShop_LockedWed"));
					}
					return true;
				case 1935:
				case 2270:
					if (Game1.player.secretNotesSeen.Contains(20) && !Game1.player.mailReceived.Contains("SecretNote20_done"))
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Town_SpecialCharmQuestion"), createYesNoResponses(), "specialCharmQuestion");
					}
					break;
				case 1913:
				case 1914:
				case 1945:
				case 1946:
					if (isShowingDestroyedJoja)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_JojaSign_Destroyed"));
						return true;
					}
					break;
				case 2000:
				case 2001:
				case 2032:
				case 2033:
					if (isShowingDestroyedJoja)
					{
						Rumble.rumble(0.15f, 200f);
						Game1.player.completelyStopAnimatingOrDoingAction();
						playSoundAt("stairsdown", Game1.player.getTileLocation());
						Game1.warpFarmer("AbandonedJojaMart", 9, 13, flip: false);
						return true;
					}
					break;
				case 599:
					if (Game1.player.secretNotesSeen.Contains(19) && !Game1.player.mailReceived.Contains("SecretNote19_done"))
					{
						DelayedAction.playSoundAfterDelay("newArtifact", 250);
						Game1.player.mailReceived.Add("SecretNote19_done");
						Game1.player.addItemByMenuIfNecessary(new Object(Vector2.Zero, 164));
					}
					break;
				case 958:
				case 1080:
				case 1081:
					if (Game1.player.mount != null)
					{
						return true;
					}
					if (currentEvent != null && currentEvent.isFestival && currentEvent.checkAction(tileLocation, viewport, who))
					{
						return true;
					}
					if (Game1.player.getTileX() <= 70)
					{
						if (Game1.CurrentEvent == null || !(Game1.CurrentEvent.FestivalName == "Egg Festival"))
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_PickupTruck"));
						}
						return true;
					}
					if (Game1.MasterPlayer.mailReceived.Contains("ccBoilerRoom"))
					{
						if (Game1.player.isRidingHorse() && Game1.player.mount != null)
						{
							Game1.player.mount.checkAction(Game1.player, this);
							break;
						}
						Game1.currentLocation.tapToMove.preventMountingHorse = true;
						createQuestionDialogue(answerChoices: (!Game1.MasterPlayer.mailReceived.Contains("ccCraftsRoom")) ? new Response[3]
						{
							new Response("Mines", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Mines")),
							new Response("Bus", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_BusStop")),
							new Response("Cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel"))
						} : new Response[4]
						{
							new Response("Mines", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Mines")),
							new Response("Bus", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_BusStop")),
							new Response("Quarry", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Quarry")),
							new Response("Cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel"))
						}, question: Game1.content.LoadString("Strings\\Locations:MineCart_ChooseDestination"), dialogKey: "Minecart");
						break;
					}
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:MineCart_OutOfOrder"));
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public void crackOpenAbandonedJojaMartDoor()
		{
			setMapTileIndex(95, 49, 2000, "Buildings");
			setMapTileIndex(96, 49, 2001, "Buildings");
			setMapTileIndex(95, 50, 2032, "Buildings");
			setMapTileIndex(96, 50, 2033, "Buildings");
		}

		private void refurbishCommunityCenter()
		{
			if (ccRefurbished)
			{
				return;
			}
			ccRefurbished = true;
			if (Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
			{
				ccJoja = true;
			}
			if (_appliedMapOverrides != null)
			{
				if (_appliedMapOverrides.Contains("ccRefurbished"))
				{
					return;
				}
				_appliedMapOverrides.Add("ccRefurbished");
			}
			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(47, 11, 11, 9);
			for (int i = rectangle.X; i <= rectangle.Right; i++)
			{
				for (int j = rectangle.Y; j <= rectangle.Bottom; j++)
				{
					if (map.GetLayer("Back").Tiles[i, j] != null && map.GetLayer("Back").Tiles[i, j].TileSheet.Id.Equals("Town") && map.GetLayer("Back").Tiles[i, j].TileIndex > 1200)
					{
						map.GetLayer("Back").Tiles[i, j].TileIndex += 12;
					}
					if (map.GetLayer("Buildings").Tiles[i, j] != null && map.GetLayer("Buildings").Tiles[i, j].TileSheet.Id.Equals("Town") && map.GetLayer("Buildings").Tiles[i, j].TileIndex > 1200)
					{
						map.GetLayer("Buildings").Tiles[i, j].TileIndex += 12;
					}
					if (map.GetLayer("Front").Tiles[i, j] != null && map.GetLayer("Front").Tiles[i, j].TileSheet.Id.Equals("Town") && map.GetLayer("Front").Tiles[i, j].TileIndex > 1200)
					{
						map.GetLayer("Front").Tiles[i, j].TileIndex += 12;
					}
					if (map.GetLayer("AlwaysFront").Tiles[i, j] != null && map.GetLayer("AlwaysFront").Tiles[i, j].TileSheet.Id.Equals("Town") && map.GetLayer("AlwaysFront").Tiles[i, j].TileIndex > 1200)
					{
						map.GetLayer("AlwaysFront").Tiles[i, j].TileIndex += 12;
					}
				}
			}
		}

		private void showDestroyedJoja()
		{
			if (isShowingDestroyedJoja)
			{
				return;
			}
			isShowingDestroyedJoja = true;
			if (_appliedMapOverrides != null && _appliedMapOverrides.Contains("isShowingDestroyedJoja"))
			{
				return;
			}
			_appliedMapOverrides.Add("isShowingDestroyedJoja");
			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(90, 42, 11, 9);
			for (int i = rectangle.X; i <= rectangle.Right; i++)
			{
				for (int j = rectangle.Y; j <= rectangle.Bottom; j++)
				{
					bool flag = false;
					bool flag2 = false;
					if (i > rectangle.X + 6 || j < rectangle.Y + 9)
					{
						flag = true;
					}
					if (flag && map.GetLayer("Back").Tiles[i, j] != null && map.GetLayer("Back").Tiles[i, j].TileSheet.Id.Equals("Town") && map.GetLayer("Back").Tiles[i, j].TileIndex > 1200)
					{
						map.GetLayer("Back").Tiles[i, j].TileIndex += 20;
					}
					if (flag && map.GetLayer("Buildings").Tiles[i, j] != null && map.GetLayer("Buildings").Tiles[i, j].TileSheet.Id.Equals("Town") && map.GetLayer("Buildings").Tiles[i, j].TileIndex > 1200)
					{
						map.GetLayer("Buildings").Tiles[i, j].TileIndex += 20;
					}
					if (flag && ((i != 93 && j != 50) || (i != 94 && j != 50)) && map.GetLayer("Front").Tiles[i, j] != null && map.GetLayer("Front").Tiles[i, j].TileSheet.Id.Equals("Town") && map.GetLayer("Front").Tiles[i, j].TileIndex > 1200)
					{
						map.GetLayer("Front").Tiles[i, j].TileIndex += 20;
					}
					if (flag && map.GetLayer("AlwaysFront").Tiles[i, j] != null && map.GetLayer("AlwaysFront").Tiles[i, j].TileSheet.Id.Equals("Town") && map.GetLayer("AlwaysFront").Tiles[i, j].TileIndex > 1200)
					{
						map.GetLayer("AlwaysFront").Tiles[i, j].TileIndex += 20;
					}
				}
			}
		}

		public override bool isTileFishable(int tileX, int tileY)
		{
			if ((GetSeasonForLocation() != "winter" && tileY == 26 && (tileX == 25 || tileX == 26 || tileX == 27)) || (tileX == 25 && tileY == 25) || (tileX == 27 && tileY == 25))
			{
				return true;
			}
			return base.isTileFishable(tileX, tileY);
		}

		public void showImprovedPamHouse()
		{
			if (isShowingUpgradedPamHouse)
			{
				return;
			}
			isShowingUpgradedPamHouse = true;
			if (_appliedMapOverrides != null)
			{
				if (_appliedMapOverrides.Contains("isShowingUpgradedPamHouse"))
				{
					return;
				}
				_appliedMapOverrides.Add("isShowingUpgradedPamHouse");
			}
			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(69, 66, 8, 3);
			Microsoft.Xna.Framework.Rectangle rectangle2 = new Microsoft.Xna.Framework.Rectangle(69, 60, 8, 6);
			for (int i = rectangle.X; i < rectangle.Right; i++)
			{
				for (int j = rectangle.Y; j < rectangle.Bottom; j++)
				{
					if (map.GetLayer("Buildings").Tiles[i, j] != null)
					{
						map.GetLayer("Buildings").Tiles[i, j].TileIndex += 842;
						if (map.GetLayer("Buildings").Tiles[i, j].TileIndex == 1568)
						{
							map.GetLayer("Buildings").Tiles[i, j].TileIndex = 1562;
						}
					}
					if (map.GetLayer("Front").Tiles[i, j] != null && j < rectangle.Bottom - 1)
					{
						map.GetLayer("Front").Tiles[i, j].TileIndex += 842;
					}
				}
			}
			for (int k = rectangle2.X; k < rectangle2.Right; k++)
			{
				for (int l = rectangle2.Y; l < rectangle2.Bottom; l++)
				{
					if (map.GetLayer("AlwaysFront").Tiles[k, l] == null)
					{
						map.GetLayer("AlwaysFront").Tiles[k, l] = new StaticTile(map.GetLayer("AlwaysFront"), map.GetTileSheet("Town"), BlendMode.Alpha, 1336 + (k - rectangle2.X) + (l - rectangle2.Y) * 32);
					}
				}
			}
			if (!Game1.eventUp)
			{
				removeTile(63, 68, "Buildings");
				removeTile(62, 72, "Buildings");
				removeTile(74, 71, "Buildings");
			}
		}

		public static Point GetTheaterTileOffset()
		{
			if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheaterJoja"))
			{
				return new Point(-43, -31);
			}
			return new Point(0, 0);
		}

		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (force)
			{
				isShowingSpecialOrdersBoard = false;
				isShowingUpgradedPamHouse = false;
				isShowingDestroyedJoja = false;
				ccRefurbished = false;
			}
			if (Game1.MasterPlayer.mailReceived.Contains("ccIsComplete") || Game1.MasterPlayer.mailReceived.Contains("JojaMember") || Game1.MasterPlayer.hasCompletedCommunityCenter())
			{
				refurbishCommunityCenter();
			}
			if (!isShowingSpecialOrdersBoard && SpecialOrder.IsSpecialOrdersBoardUnlocked())
			{
				isShowingSpecialOrdersBoard = true;
				LargeTerrainFeature largeTerrainFeature = null;
				do
				{
					largeTerrainFeature = getLargeTerrainFeatureAt(61, 93);
					if (largeTerrainFeature != null)
					{
						largeTerrainFeatures.Remove(largeTerrainFeature);
					}
				}
				while (largeTerrainFeature != null);
				int whichTileSheet = 2;
				TileSheet tileSheet = map.GetTileSheet("Town");
				if (tileSheet != null)
				{
					whichTileSheet = map.TileSheets.IndexOf(tileSheet);
				}
				setMapTileIndex(61, 93, 2045, "Buildings", whichTileSheet);
				setMapTileIndex(62, 93, 2046, "Buildings", whichTileSheet);
				setMapTileIndex(63, 93, 2047, "Buildings", whichTileSheet);
				setTileProperty(61, 93, "Buildings", "Action", "SpecialOrders");
				setTileProperty(62, 93, "Buildings", "Action", "SpecialOrders");
				setTileProperty(63, 93, "Buildings", "Action", "SpecialOrders");
				setMapTileIndex(61, 92, 2013, "Front", whichTileSheet);
				setMapTileIndex(62, 92, 2014, "Front", whichTileSheet);
				setMapTileIndex(63, 92, 2015, "Front", whichTileSheet);
			}
			if (NetWorldState.checkAnywhereForWorldStateID("trashBearDone") && (currentEvent == null || currentEvent.id != 777111))
			{
				if (!Game1.eventUp || mapPath.Value == null || !mapPath.Value.EndsWith("Town-Fair", StringComparison.Ordinal))
				{
					ApplyMapOverride("Town-TrashGone", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(57, 68, 17, 5));
				}
				ApplyMapOverride("Town-DogHouse", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(51, 65, 5, 6));
			}
			if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater"))
			{
				if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheaterJoja"))
				{
					Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(46, 10, 15, 16);
					ApplyMapOverride("Town-TheaterCC", value, value);
				}
				else
				{
					Microsoft.Xna.Framework.Rectangle value2 = new Microsoft.Xna.Framework.Rectangle(84, 41, 27, 15);
					ApplyMapOverride("Town-Theater", value2, value2);
				}
			}
			else if (Utility.HasAnyPlayerSeenEvent(191393))
			{
				showDestroyedJoja();
				if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("abandonedJojaMartAccessible"))
				{
					crackOpenAbandonedJojaMartDoor();
				}
			}
			if (Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
			{
				showImprovedPamHouse();
			}
			if (Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
			{
				showTownCommunityUpgradeShortcuts();
			}
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccBoilerRoom"))
			{
				minecartSteam = new TemporaryAnimatedSprite(27, new Vector2(6856f, 5008f), Color.White)
				{
					totalNumberOfLoops = 999999,
					interval = 60f,
					flipped = true
				};
			}
			if (NetWorldState.checkAnywhereForWorldStateID("trashBearDone") && (currentEvent == null || currentEvent.id != 777111) && !Game1.isRaining)
			{
				Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
				if (random.NextDouble() < 0.2)
				{
					base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(348, 1916, 12, 20), 999f, 1, 999999, new Vector2(53f, 67f) * 64f + new Vector2(3f, 2f) * 4f, flicker: false, flipped: false, 0.98f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 1f
					});
				}
			}
			if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater"))
			{
				if ((long)Game1.player.team.theaterBuildDate < 0)
				{
					Game1.player.team.theaterBuildDate.Value = Game1.Date.TotalDays;
				}
				Point theaterTileOffset = GetTheaterTileOffset();
				MovieTheater.AddMoviePoster(this, (91 + theaterTileOffset.X) * 64 + 32, (48 + theaterTileOffset.Y) * 64 + 64);
				MovieTheater.AddMoviePoster(this, (93 + theaterTileOffset.X) * 64 + 24, (48 + theaterTileOffset.Y) * 64 + 64, 1);
				Vector2 vector = new Vector2(theaterTileOffset.X, theaterTileOffset.Y);
				Game1.currentLightSources.Add(new LightSource(4, (new Vector2(91f, 46f) + vector) * 64f, 1f, LightSource.LightContext.None, 0L));
				Game1.currentLightSources.Add(new LightSource(4, (new Vector2(96f, 47f) + vector) * 64f, 1f, LightSource.LightContext.None, 0L));
				Game1.currentLightSources.Add(new LightSource(4, (new Vector2(100f, 47f) + vector) * 64f, 1f, LightSource.LightContext.None, 0L));
				Game1.currentLightSources.Add(new LightSource(4, (new Vector2(96f, 45f) + vector) * 64f, 1f, LightSource.LightContext.None, 0L));
				Game1.currentLightSources.Add(new LightSource(4, (new Vector2(100f, 45f) + vector) * 64f, 1f, LightSource.LightContext.None, 0L));
				Game1.currentLightSources.Add(new LightSource(4, (new Vector2(97f, 43f) + vector) * 64f, 1f, LightSource.LightContext.None, 0L));
				Game1.currentLightSources.Add(new LightSource(4, (new Vector2(99f, 43f) + vector) * 64f, 1f, LightSource.LightContext.None, 0L));
				Game1.currentLightSources.Add(new LightSource(4, (new Vector2(98f, 49f) + vector) * 64f, 1f, LightSource.LightContext.None, 0L));
				Game1.currentLightSources.Add(new LightSource(4, (new Vector2(92f, 49f) + vector) * 64f, 1f, LightSource.LightContext.None, 0L));
				Game1.currentLightSources.Add(new LightSource(4, (new Vector2(94f, 49f) + vector) * 64f, 1f, LightSource.LightContext.None, 0L));
				Game1.currentLightSources.Add(new LightSource(4, (new Vector2(98f, 51f) + vector) * 64f, 1f, LightSource.LightContext.None, 0L));
				Game1.currentLightSources.Add(new LightSource(4, (new Vector2(92f, 51f) + vector) * 64f, 1f, LightSource.LightContext.None, 0L));
				Game1.currentLightSources.Add(new LightSource(4, (new Vector2(94f, 51f) + vector) * 64f, 1f, LightSource.LightContext.None, 0L));
			}
			if (!Game1.currentSeason.Equals("winter"))
			{
				AmbientLocationSounds.addSound(new Vector2(26f, 26f), 0);
				AmbientLocationSounds.addSound(new Vector2(26f, 28f), 0);
			}
			if (!Game1.isStartingToGetDarkOut())
			{
				AmbientLocationSounds.addSound(new Vector2(100f, 79f), 2);
				addClintMachineGraphics();
			}
			if (Game1.player.mailReceived.Contains("checkedBulletinOnce"))
			{
				playerCheckedBoard = true;
			}
			if (Game1.player.eventsSeen.Contains(520702) && !Game1.player.hasMagnifyingGlass && Game1.currentSeason.Equals("winter"))
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(14.5f, 52.75f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(13.5f, 53f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(15.5f, 53f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(16f, 52.25f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(17f, 52f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(17f, 51f) * 64f + new Vector2(8f, 0f) * 4f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(18f, 51f) * 64f + new Vector2(5f, -7f) * 4f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(18f, 50f) * 64f + new Vector2(12f, -2f) * 4f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(21.75f, 39.5f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(21f, 39f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(21.75f, 38.25f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(22.5f, 37.5f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(21.75f, 36.75f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(23f, 36f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(22.25f, 35.25f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(23.5f, 34.6f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(23.5f, 33.6f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(24.25f, 32.6f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(26.75f, 26.75f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(27.5f, 26f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(30f, 23f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(31f, 22f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(30.5f, 21f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(31f, 20f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(30f, 19f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(29f, 18f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(29.1f, 17f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(30f, 17.7f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(31.5f, 18.2f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(30.5f, 16.8f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			}
			if (Game1.MasterPlayer.mailReceived.Contains("Capsule_Broken") && Game1.isDarkOut() && Game1.random.NextDouble() < 0.01)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Microsoft.Xna.Framework.Rectangle(448, 546, 16, 25), new Vector2(3f, 59f) * 64f, flipped: false, 0f, Color.White)
				{
					scale = 4f,
					motion = new Vector2(3f, 0f),
					animationLength = 4,
					interval = 80f,
					totalNumberOfLoops = 200,
					layerDepth = 0.384f,
					xStopCoordinate = 384
				});
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Microsoft.Xna.Framework.Rectangle(448, 546, 16, 25), new Vector2(58f, 108f) * 64f, flipped: false, 0f, Color.White)
				{
					scale = 4f,
					motion = new Vector2(3f, 0f),
					animationLength = 4,
					interval = 80f,
					totalNumberOfLoops = 200,
					layerDepth = 0.384f,
					xStopCoordinate = 4800
				});
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Microsoft.Xna.Framework.Rectangle(448, 546, 16, 25), new Vector2(20f, 92.5f) * 64f, flipped: false, 0f, Color.White)
				{
					scale = 4f,
					motion = new Vector2(3f, 0f),
					animationLength = 4,
					interval = 80f,
					totalNumberOfLoops = 200,
					layerDepth = 0.384f,
					xStopCoordinate = 1664,
					delayBeforeAnimationStart = 1000
				});
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Microsoft.Xna.Framework.Rectangle(448, 546, 16, 25), new Vector2(75f, 1f) * 64f, flipped: true, 0f, Color.White)
				{
					scale = 4f,
					motion = new Vector2(-4f, 0f),
					animationLength = 4,
					interval = 60f,
					totalNumberOfLoops = 200,
					layerDepth = 0.0064f,
					xStopCoordinate = 4352
				});
			}
		}

		private void showTownCommunityUpgradeShortcuts()
		{
			removeTile(90, 2, "Buildings");
			removeTile(90, 1, "Front");
			removeTile(90, 1, "Buildings");
			removeTile(90, 0, "Buildings");
			setMapTileIndex(89, 1, 360, "Front");
			setMapTileIndex(89, 2, 385, "Buildings");
			setMapTileIndex(89, 1, 436, "Buildings");
			setMapTileIndex(89, 0, 411, "Buildings");
			removeTile(98, 3, "Buildings");
			removeTile(98, 2, "Buildings");
			removeTile(98, 1, "Buildings");
			removeTile(98, 0, "Buildings");
			setMapTileIndex(98, 3, 588, "Back");
			setMapTileIndex(98, 2, 588, "Back");
			setMapTileIndex(98, 1, 588, "Back");
			setMapTileIndex(98, 0, 588, "Back");
			setMapTileIndex(99, 3, 416, "Buildings");
			setMapTileIndex(99, 2, 391, "Buildings");
			setMapTileIndex(99, 1, 416, "Buildings");
			setMapTileIndex(99, 0, 391, "Buildings");
			removeTile(92, 104, "Buildings");
			removeTile(93, 104, "Buildings");
			removeTile(94, 104, "Buildings");
			removeTile(92, 105, "Buildings");
			removeTile(93, 105, "Buildings");
			removeTile(94, 105, "Buildings");
			removeTile(93, 106, "Buildings");
			removeTile(94, 106, "Buildings");
			removeTile(92, 103, "Front");
			removeTile(93, 103, "Front");
			removeTile(94, 103, "Front");
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			minecartSteam = null;
			if ((Game1.locationRequest == null || Game1.locationRequest.Location == null || Game1.locationRequest.Location.IsOutdoors) && Game1.getMusicTrackName().Contains("town"))
			{
				Game1.changeMusicTrack("none");
			}
			if (mapLoader != null)
			{
				mapLoader.Dispose();
				mapLoader = null;
			}
		}

		public void initiateMarnieLewisBush()
		{
			Game1.player.freezePause = 3000;
			temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Marnie", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 32), new Vector2(48f, 98f) * 64f, flipped: false, 0f, Color.White)
			{
				scale = 4f,
				animationLength = 4,
				interval = 200f,
				totalNumberOfLoops = 99999,
				motion = new Vector2(-3f, -12f),
				acceleration = new Vector2(0f, 0.4f),
				xStopCoordinate = 2880,
				yStopCoordinate = 6336,
				layerDepth = 0.64f,
				reachedStopCoordinate = marnie_landed,
				id = 888f
			});
			temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Lewis", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 32), new Vector2(48f, 98f) * 64f, flipped: false, 0f, Color.White)
			{
				scale = 4f,
				animationLength = 4,
				interval = 200f,
				totalNumberOfLoops = 99999,
				motion = new Vector2(3f, -12f),
				acceleration = new Vector2(0f, 0.4f),
				xStopCoordinate = 3264,
				yStopCoordinate = 6336,
				layerDepth = 0.64f,
				id = 777f
			});
			Game1.playSound("dwop");
		}

		private void marnie_landed(int extra)
		{
			Game1.player.freezePause = 2000;
			TemporaryAnimatedSprite temporarySpriteByID = getTemporarySpriteByID(777);
			if (temporarySpriteByID != null)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Lewis", new Microsoft.Xna.Framework.Rectangle(0, 32, 16, 32), temporarySpriteByID.position, flipped: false, 0f, Color.White)
				{
					scale = 4f,
					animationLength = 4,
					interval = 60f,
					totalNumberOfLoops = 50,
					layerDepth = 0.64f,
					id = 0f,
					motion = new Vector2(8f, 0f)
				});
			}
			temporarySpriteByID = getTemporarySpriteByID(888);
			if (temporarySpriteByID != null)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Marnie", new Microsoft.Xna.Framework.Rectangle(0, 32, 16, 32), temporarySpriteByID.position, flipped: true, 0f, Color.White)
				{
					scale = 4f,
					animationLength = 4,
					interval = 60f,
					totalNumberOfLoops = 50,
					layerDepth = 0.64f,
					id = 1f,
					motion = new Vector2(-8f, 0f)
				});
			}
			removeTemporarySpritesWithID(777);
			removeTemporarySpritesWithID(888);
			for (int i = 0; i < 3200; i += 200)
			{
				DelayedAction.playSoundAfterDelay("grassyStep", 100 + i);
			}
		}

		public void initiateMagnifyingGlassGet()
		{
			Game1.displayHUD = false;
			Game1.player.freezePause = 3000;
			if (Game1.player.getTileX() >= 31)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Krobus", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 24), new Vector2(29f, 13f) * 64f, flipped: false, 0f, Color.White)
				{
					scale = 4f,
					animationLength = 4,
					interval = 200f,
					totalNumberOfLoops = 99999,
					motion = new Vector2(3f, -12f),
					acceleration = new Vector2(0f, 0.4f),
					xStopCoordinate = 2048,
					yStopCoordinate = 960,
					layerDepth = 1f,
					reachedStopCoordinate = mgThief_landed,
					id = 777f
				});
			}
			else
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Krobus", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 24), new Vector2(29f, 13f) * 64f, flipped: false, 0f, Color.White)
				{
					scale = 4f,
					animationLength = 4,
					interval = 200f,
					totalNumberOfLoops = 99999,
					motion = new Vector2(2f, -12f),
					acceleration = new Vector2(0f, 0.4f),
					xStopCoordinate = 1984,
					yStopCoordinate = 832,
					layerDepth = 0.0896f,
					reachedStopCoordinate = mgThief_landed,
					id = 777f
				});
			}
			Game1.playSound("dwop");
		}

		private void mgThief_landed(int extra)
		{
			TemporaryAnimatedSprite temporarySpriteByID = getTemporarySpriteByID(777);
			if (temporarySpriteByID != null)
			{
				temporarySpriteByID.animationLength = 1;
				temporarySpriteByID.shakeIntensity = 1f;
				temporarySpriteByID.interval = 1500f;
				temporarySpriteByID.timer = 0f;
				temporarySpriteByID.totalNumberOfLoops = 1;
				temporarySpriteByID.currentNumberOfLoops = 0;
				temporarySpriteByID.endFunction = mgThief_speech;
				Game1.playSound("snowyStep");
			}
		}

		private void mgThief_speech(int extra)
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_mgThiefMessage"));
			Game1.afterDialogues = mgThief_afterSpeech;
			TemporaryAnimatedSprite temporarySpriteByID = getTemporarySpriteByID(777);
			if (temporarySpriteByID != null)
			{
				temporarySpriteByID.animationLength = 4;
				temporarySpriteByID.shakeIntensity = 0f;
				temporarySpriteByID.interval = 200f;
				temporarySpriteByID.timer = 0f;
				temporarySpriteByID.totalNumberOfLoops = 9999;
				temporarySpriteByID.currentNumberOfLoops = 0;
				temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Krobus", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 24), temporarySpriteByID.position, flipped: false, 0f, Color.White)
				{
					scale = 4f,
					animationLength = 4,
					interval = 200f,
					totalNumberOfLoops = 99999,
					layerDepth = 0.0896f,
					id = 777f
				});
			}
		}

		private void mgThief_afterSpeech()
		{
			Game1.player.holdUpItemThenMessage(new SpecialItem(5));
			Game1.afterDialogues = mgThief_afterGlass;
			Game1.player.hasMagnifyingGlass = true;
			Game1.player.removeQuest(31);
		}

		private void mgThief_afterGlass()
		{
			Game1.player.freezePause = 1500;
			TemporaryAnimatedSprite temporarySpriteByID = getTemporarySpriteByID(777);
			if (temporarySpriteByID != null)
			{
				temporarySpriteByID.animationLength = 1;
				temporarySpriteByID.shakeIntensity = 1f;
				temporarySpriteByID.interval = 500f;
				temporarySpriteByID.timer = 0f;
				temporarySpriteByID.totalNumberOfLoops = 1;
				temporarySpriteByID.currentNumberOfLoops = 0;
				temporarySpriteByID.endFunction = mg_disappear;
			}
		}

		private void mg_disappear(int extra)
		{
			Game1.player.freezePause = 1000;
			TemporaryAnimatedSprite temporarySpriteByID = getTemporarySpriteByID(777);
			if (temporarySpriteByID != null)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Krobus", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 24), temporarySpriteByID.position, flipped: false, 0f, Color.White)
				{
					scale = 4f,
					animationLength = 4,
					interval = 60f,
					totalNumberOfLoops = 50,
					layerDepth = 0.0896f,
					id = 777f,
					motion = new Vector2(0f, 8f)
				});
				for (int i = 0; i < 3200; i += 200)
				{
					DelayedAction.playSoundAfterDelay("snowyStep", 100 + i);
				}
			}
			Game1.displayHUD = true;
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			if (minecartSteam != null)
			{
				minecartSteam.update(time);
			}
		}

		public override void draw(SpriteBatch spriteBatch)
		{
			base.draw(spriteBatch);
			if (minecartSteam != null)
			{
				minecartSteam.draw(spriteBatch);
			}
			if (ccJoja && !_appliedMapOverrides.Contains("Town-TheaterCC"))
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, ccFacadePositionBottom), jojaFacadeBottom, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.128f);
			}
			if (!playerCheckedBoard)
			{
				float num = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2616f, 3472f + num)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.98f);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2656f, 3512f + num)), new Microsoft.Xna.Framework.Rectangle(175, 425, 12, 12), Color.White * 0.75f, 0f, new Vector2(6f, 6f), 4f, SpriteEffects.None, 1f);
			}
			if (Game1.CanAcceptDailyQuest())
			{
				float num2 = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2692f, 3528f + num2)), new Microsoft.Xna.Framework.Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - num2 / 16f), SpriteEffects.None, 1f);
			}
			if (SpecialOrder.IsSpecialOrdersBoardUnlocked() && !Game1.player.team.acceptedSpecialOrderTypes.Contains("") && !Game1.eventUp)
			{
				float num3 = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3997.6f, 5908.8f + num3)), new Microsoft.Xna.Framework.Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - num3 / 8f), SpriteEffects.None, 1f);
			}
		}

		public override Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			bool flag = IsUsingMagicBait(who);
			if (bobberTile.X < 30f && bobberTile.Y < 30f)
			{
				if (Game1.random.NextDouble() < 0.1)
				{
					return new Furniture(2427, Vector2.Zero);
				}
				return new Object((Game1.random.NextDouble() < 0.5) ? 388 : 390, 1);
			}
			float num = 0f;
			if (who != null && who.CurrentTool is FishingRod && (who.CurrentTool as FishingRod).getBobberAttachmentIndex() == 856)
			{
				num += 0.05f;
			}
			if (who.getTileLocation().Y < 15f && who.FishingLevel >= 3 && Game1.random.NextDouble() < 0.2 + (double)num)
			{
				if (Game1.player.team.SpecialOrderRuleActive("LEGENDARY_FAMILY"))
				{
					return new Object(899, 1);
				}
				if (!who.fishCaught.ContainsKey(160) && (Game1.currentSeason.Equals("fall") || flag))
				{
					return new Object(160, 1);
				}
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			if (ccJoja && !_appliedMapOverrides.Contains("Town-TheaterCC"))
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, ccFacadePosition), jojaFacadeTop, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.128f);
				if (Game1.IsWinter)
				{
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, ccFacadePosition), jojaFacadeWinterOverlay, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1281f);
				}
			}
			else if (!ccJoja && ccRefurbished)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, clockCenter), hourHandSource, Color.White, (float)(Math.PI * 2.0 * (double)((float)(Game1.timeOfDay % 1200) / 1200f) + (double)((float)Game1.gameTimeInterval / 7000f / 23f)), new Vector2(2.5f, 8f), 4f, SpriteEffects.None, 0.98f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, clockCenter), minuteHandSource, Color.White, (float)(Math.PI * 2.0 * (double)((float)(Game1.timeOfDay % 1000 % 100 % 60) / 60f) + (double)((float)Game1.gameTimeInterval / 7000f * 1.02f)), new Vector2(2.5f, 12f), 4f, SpriteEffects.None, 0.99f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, clockCenter), clockNub, Color.White, 0f, new Vector2(2f, 2f), 4f, SpriteEffects.None, 1f);
			}
			base.drawAboveAlwaysFrontLayer(b);
		}

		public override bool performAction(string action, Farmer who, Location tileLocation)
		{
			return base.performAction(action, who, tileLocation);
		}
	}
}
