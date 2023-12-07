using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class LibraryMuseum : GameLocation
	{
		public const int dwarvenGuide = 0;

		public const int totalArtifacts = 95;

		public const int totalNotes = 21;

		[Obsolete]
		[XmlIgnore]
		private Dictionary<int, Vector2> lostBooksLocations = new Dictionary<int, Vector2>();

		private readonly NetMutex mutex = new NetMutex();

		[XmlElement("museumPieces")]
		public NetVector2Dictionary<int, NetInt> museumPieces => Game1.netWorldState.Value.MuseumPieces;

		public LibraryMuseum()
		{
		}

		public LibraryMuseum(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(mutex.NetFields);
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
		{
			mutex.Update(this);
			base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
		}

		public bool museumAlreadyHasArtifact(int index)
		{
			foreach (KeyValuePair<Vector2, int> pair in museumPieces.Pairs)
			{
				if (pair.Value == index)
				{
					return true;
				}
			}
			return false;
		}

		public bool isItemSuitableForDonation(Item i)
		{
			if (i is Object && (i as Object).type != null && ((i as Object).type.Equals("Arch") || (i as Object).type.Equals("Minerals")))
			{
				int num = (i as Object).parentSheetIndex;
				bool flag = false;
				foreach (KeyValuePair<Vector2, int> pair in museumPieces.Pairs)
				{
					if (pair.Value == num)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return true;
				}
			}
			return false;
		}

		public bool doesFarmerHaveAnythingToDonate(Farmer who)
		{
			for (int i = 0; i < (int)who.maxItems; i++)
			{
				if (i >= who.items.Count || !(who.items[i] is Object) || !((who.items[i] as Object).type != null) || (!(who.items[i] as Object).type.Equals("Arch") && !(who.items[i] as Object).type.Equals("Minerals")))
				{
					continue;
				}
				int num = (who.items[i] as Object).parentSheetIndex;
				bool flag = false;
				foreach (KeyValuePair<Vector2, int> pair in museumPieces.Pairs)
				{
					if (pair.Value == num)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return true;
				}
			}
			return false;
		}

		private bool museumContainsTheseItems(int[] items, HashSet<int> museumItems)
		{
			for (int i = 0; i < items.Length; i++)
			{
				if (!museumItems.Contains(items[i]))
				{
					return false;
				}
			}
			return true;
		}

		private int numberOfMuseumItemsOfType(string type)
		{
			int num = 0;
			foreach (KeyValuePair<Vector2, int> pair in museumPieces.Pairs)
			{
				if (Game1.objectInformation[pair.Value].Split('/')[3].Contains(type))
				{
					num++;
				}
			}
			return num;
		}

		private Dictionary<int, Vector2> getLostBooksLocations()
		{
			Dictionary<int, Vector2> dictionary = new Dictionary<int, Vector2>();
			for (int i = 0; i < map.Layers[0].LayerWidth; i++)
			{
				for (int j = 0; j < map.Layers[0].LayerHeight; j++)
				{
					if (doesTileHaveProperty(i, j, "Action", "Buildings") != null && doesTileHaveProperty(i, j, "Action", "Buildings").Contains("Notes"))
					{
						dictionary.Add(Convert.ToInt32(doesTileHaveProperty(i, j, "Action", "Buildings").Split(' ')[1]), new Vector2(i, j));
					}
				}
			}
			return dictionary;
		}

		protected override void resetLocalState()
		{
			if (!Game1.player.eventsSeen.Contains(0) && doesFarmerHaveAnythingToDonate(Game1.player) && !Game1.player.mailReceived.Contains("somethingToDonate"))
			{
				Game1.player.mailReceived.Add("somethingToDonate");
			}
			if (museumPieces.Count() > 0 && !Game1.player.mailReceived.Contains("somethingWasDonated"))
			{
				Game1.player.mailReceived.Add("somethingWasDonated");
			}
			base.resetLocalState();
			if (!Game1.isRaining)
			{
				Game1.changeMusicTrack("libraryTheme");
			}
			int num = Game1.netWorldState.Value.LostBooksFound;
			Dictionary<int, Vector2> dictionary = getLostBooksLocations();
			for (int i = 0; i < dictionary.Count; i++)
			{
				if (dictionary.ElementAt(i).Key <= num && !Game1.player.mailReceived.Contains("lb_" + dictionary.ElementAt(i).Key))
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(144, 447, 15, 15), new Vector2(dictionary.ElementAt(i).Value.X * 64f, dictionary.ElementAt(i).Value.Y * 64f - 96f - 16f), flipped: false, 0f, Color.White)
					{
						interval = 99999f,
						animationLength = 1,
						totalNumberOfLoops = 9999,
						yPeriodic = true,
						yPeriodicLoopTime = 4000f,
						yPeriodicRange = 16f,
						layerDepth = 1f,
						scale = 4f,
						id = dictionary.ElementAt(i).Key
					});
				}
			}
			foreach (KeyValuePair<Vector2, int> pair in museumPieces.Pairs)
			{
				if (Game1.objectInformation.ContainsKey(pair.Value))
				{
					string text = Game1.objectInformation[pair.Value].Split('/')[3];
					if (text.Contains("Arch") && !Game1.player.archaeologyFound.ContainsKey(pair.Value))
					{
						Game1.player.foundArtifact(pair.Value, 1);
					}
					else if ((text.Contains("Mineral") || text.Substring(text.Length - 3).Equals("-2")) && !Game1.player.mineralsFound.ContainsKey(pair.Value))
					{
						Game1.player.foundMineral(pair.Value);
					}
				}
			}
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			if (!Game1.isRaining)
			{
				Game1.changeMusicTrack("none");
			}
		}

		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			switch (questionAndAnswer)
			{
			case "Museum_Collect":
				Game1.activeClickableMenu = new ItemGrabMenu(getRewardsForPlayer(Game1.player), reverseGrab: false, showReceivingMenu: true, null, null, "Rewards", collectedReward, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: false, allowRightClick: false, showOrganizeButton: false, 0, null, -1, null, -1, 3, null, allowStack: true, null, rearrangeGrangeOnExit: false, null, this);
				break;
			case "Museum_Donate":
				mutex.RequestLock(delegate
				{
					Game1.activeClickableMenu = new MuseumMenu(isItemSuitableForDonation)
					{
						exitFunction = delegate
						{
							mutex.ReleaseLock();
						}
					};
				});
				break;
			case "Museum_Rearrange_Yes":
				if (mutex.IsLocked())
				{
					break;
				}
				mutex.RequestLock(delegate
				{
					Game1.activeClickableMenu = new MuseumMenu(InventoryMenu.highlightNoItems)
					{
						exitFunction = delegate
						{
							mutex.ReleaseLock();
						}
					};
				});
				break;
			}
			return base.answerDialogueAction(questionAndAnswer, questionParams);
		}

		public string getRewardItemKey(Item item)
		{
			return "museumCollectedReward" + Utility.getStandardDescriptionFromItem(item, 1, '_');
		}

		public override bool performAction(string action, Farmer who, Location tileLocation)
		{
			if (action != null && who.IsLocalPlayer)
			{
				string[] array = action.Split(' ');
				string text = array[0];
				if (text == "Gunther")
				{
					gunther();
					return true;
				}
				if (text == "Rearrange" && !doesFarmerHaveAnythingToDonate(Game1.player))
				{
					rearrange();
					return true;
				}
			}
			return base.performAction(action, who, tileLocation);
		}

		public void rearrange()
		{
			if (museumPieces.Count() > 0)
			{
				createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Rearrange"), createYesNoResponses(), "Museum_Rearrange");
			}
		}

		public List<Item> getRewardsForPlayer(Farmer who)
		{
			List<Item> list = new List<Item>();
			HashSet<int> hashSet = new HashSet<int>(museumPieces.Values);
			int num = numberOfMuseumItemsOfType("Arch");
			int num2 = numberOfMuseumItemsOfType("Minerals");
			int num3 = num + num2;
			if (!who.canUnderstandDwarves && hashSet.Contains(96) && hashSet.Contains(97) && hashSet.Contains(98) && hashSet.Contains(99))
			{
				AddRewardIfUncollected(who, list, new Object(326, 1));
			}
			if (!who.specialBigCraftables.Contains(1305) && hashSet.Contains(113) && num > 4)
			{
				AddRewardIfUncollected(who, list, new Furniture(1305, Vector2.Zero));
			}
			if (!who.specialBigCraftables.Contains(1304) && num >= 15)
			{
				AddRewardIfUncollected(who, list, new Furniture(1304, Vector2.Zero));
			}
			if (!who.specialBigCraftables.Contains(139) && num >= 20)
			{
				AddRewardIfUncollected(who, list, new Object(Vector2.Zero, 139));
			}
			if (!who.specialBigCraftables.Contains(1545) && museumContainsTheseItems(new int[2] { 108, 122 }, hashSet) && num > 10)
			{
				AddRewardIfUncollected(who, list, new Furniture(1545, Vector2.Zero));
			}
			if (!who.specialItems.Contains(464) && hashSet.Contains(119) && num > 2)
			{
				AddRewardIfUncollected(who, list, new Object(464, 1));
			}
			if (!who.specialItems.Contains(463) && hashSet.Contains(123) && num > 2)
			{
				AddRewardIfUncollected(who, list, new Object(463, 1));
			}
			if (!who.specialItems.Contains(499) && hashSet.Contains(114))
			{
				AddRewardIfUncollected(who, list, new Object(499, 1));
			}
			if (!who.knowsRecipe("Ancient Seeds") && hashSet.Contains(114))
			{
				AddRewardIfUncollected(who, list, new Object(499, 1, isRecipe: true));
			}
			if (!who.specialBigCraftables.Contains(1301) && museumContainsTheseItems(new int[3] { 579, 581, 582 }, hashSet))
			{
				AddRewardIfUncollected(who, list, new Furniture(1301, Vector2.Zero));
			}
			if (!who.specialBigCraftables.Contains(1302) && museumContainsTheseItems(new int[2] { 583, 584 }, hashSet))
			{
				AddRewardIfUncollected(who, list, new Furniture(1302, Vector2.Zero));
			}
			if (!who.specialBigCraftables.Contains(1303) && museumContainsTheseItems(new int[2] { 580, 585 }, hashSet))
			{
				AddRewardIfUncollected(who, list, new Furniture(1303, Vector2.Zero));
			}
			if (!who.specialBigCraftables.Contains(1298) && num2 > 10)
			{
				AddRewardIfUncollected(who, list, new Furniture(1298, Vector2.Zero));
			}
			if (!who.specialBigCraftables.Contains(1299) && num2 > 30)
			{
				AddRewardIfUncollected(who, list, new Furniture(1299, Vector2.Zero));
			}
			if (!who.specialBigCraftables.Contains(94) && num2 > 20)
			{
				AddRewardIfUncollected(who, list, new Object(Vector2.Zero, 94));
			}
			if (!who.specialBigCraftables.Contains(21) && num2 >= 50)
			{
				AddRewardIfUncollected(who, list, new Object(Vector2.Zero, 21));
			}
			if (!who.specialBigCraftables.Contains(131) && num2 > 40)
			{
				AddRewardIfUncollected(who, list, new Furniture(131, Vector2.Zero));
			}
			foreach (Item item in list)
			{
				item.specialItem = true;
			}
			if (!who.mailReceived.Contains("museum5") && num3 >= 5)
			{
				AddRewardIfUncollected(who, list, new Object(474, 9));
			}
			if (!who.mailReceived.Contains("museum10") && num3 >= 10)
			{
				AddRewardIfUncollected(who, list, new Object(479, 9));
			}
			if (!who.mailReceived.Contains("museum15") && num3 >= 15)
			{
				AddRewardIfUncollected(who, list, new Object(486, 1));
			}
			if (!who.mailReceived.Contains("museum20") && num3 >= 20)
			{
				AddRewardIfUncollected(who, list, new Furniture(1541, Vector2.Zero));
			}
			if (!who.mailReceived.Contains("museum25") && num3 >= 25)
			{
				AddRewardIfUncollected(who, list, new Furniture(1554, Vector2.Zero));
			}
			if (!who.mailReceived.Contains("museum30") && num3 >= 30)
			{
				AddRewardIfUncollected(who, list, new Furniture(1669, Vector2.Zero));
			}
			if (!who.mailReceived.Contains("museum35") && num3 >= 35)
			{
				AddRewardIfUncollected(who, list, new Object(490, 9));
			}
			if (!who.mailReceived.Contains("museum40") && num3 >= 40)
			{
				AddRewardIfUncollected(who, list, new Object(Vector2.Zero, 140));
			}
			if (!who.mailReceived.Contains("museum50") && num3 >= 50)
			{
				AddRewardIfUncollected(who, list, new Furniture(1671, Vector2.Zero));
			}
			if (!who.mailReceived.Contains("museum70") && num3 >= 70)
			{
				AddRewardIfUncollected(who, list, new Object(253, 3));
			}
			if (!who.mailReceived.Contains("museum80") && num3 >= 80)
			{
				AddRewardIfUncollected(who, list, new Object(688, 5));
			}
			if (!who.mailReceived.Contains("museum90") && num3 >= 90)
			{
				AddRewardIfUncollected(who, list, new Object(279, 1));
			}
			if (!who.mailReceived.Contains("museumComplete") && num3 >= 95)
			{
				AddRewardIfUncollected(who, list, new Object(434, 1));
			}
			if (num3 >= 60)
			{
				if (!Game1.player.eventsSeen.Contains(295672))
				{
					Game1.player.eventsSeen.Add(295672);
				}
				else if (!Game1.player.hasRustyKey)
				{
					Game1.player.eventsSeen.Remove(66);
				}
			}
			return list;
		}

		public void AddRewardIfUncollected(Farmer farmer, List<Item> rewards, Item reward_item)
		{
			if (!farmer.mailReceived.Contains(getRewardItemKey(reward_item)))
			{
				rewards.Add(reward_item);
			}
		}

		public void collectedReward(Item item, Farmer who)
		{
			if (item == null)
			{
				return;
			}
			if (item is Object)
			{
				(item as Object).specialItem = true;
				switch ((item as Object).ParentSheetIndex)
				{
				case 434:
					who.mailReceived.Add("museumComplete");
					break;
				case 474:
					who.mailReceived.Add("museum5");
					break;
				case 479:
					who.mailReceived.Add("museum10");
					break;
				case 486:
					who.mailReceived.Add("museum15");
					break;
				case 1541:
					who.mailReceived.Add("museum20");
					break;
				case 1554:
					who.mailReceived.Add("museum25");
					break;
				case 1669:
					who.mailReceived.Add("museum30");
					break;
				case 490:
					who.mailReceived.Add("museum35");
					break;
				case 140:
					who.mailReceived.Add("museum40");
					break;
				case 1671:
					who.mailReceived.Add("museum50");
					break;
				case 253:
					who.mailReceived.Add("museum70");
					break;
				case 688:
					who.mailReceived.Add("museum80");
					break;
				case 279:
					who.mailReceived.Add("museum90");
					break;
				}
			}
			if (!who.hasOrWillReceiveMail(getRewardItemKey(item)))
			{
				who.mailReceived.Add(getRewardItemKey(item));
			}
		}

		private void gunther()
		{
			if (doesFarmerHaveAnythingToDonate(Game1.player) && !mutex.IsLocked())
			{
				Response[] answerChoices = ((getRewardsForPlayer(Game1.player).Count <= 0) ? new Response[2]
				{
					new Response("Donate", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Donate")),
					new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave"))
				} : new Response[3]
				{
					new Response("Donate", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Donate")),
					new Response("Collect", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Collect")),
					new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave"))
				});
				createQuestionDialogue("", answerChoices, "Museum");
			}
			else if (getRewardsForPlayer(Game1.player).Count > 0)
			{
				createQuestionDialogue("", new Response[2]
				{
					new Response("Collect", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Collect")),
					new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave"))
				}, "Museum");
			}
			else if (doesFarmerHaveAnythingToDonate(Game1.player) && mutex.IsLocked())
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NPC_Busy", Game1.getCharacterFromName("Gunther").displayName));
			}
			else if (Game1.player.achievements.Contains(5))
			{
				Game1.drawDialogue(Game1.getCharacterFromName("Gunther"), Game1.parseText(Game1.content.LoadString("Data\\ExtraDialogue:Gunther_MuseumComplete")));
			}
			else
			{
				Game1.drawDialogue(Game1.getCharacterFromName("Gunther"), Game1.player.mailReceived.Contains("artifactFound") ? Game1.parseText(Game1.content.LoadString("Data\\ExtraDialogue:Gunther_NothingToDonate")) : Game1.content.LoadString("Data\\ExtraDialogue:Gunther_NoArtifactsFound"));
			}
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			foreach (KeyValuePair<Vector2, int> pair in museumPieces.Pairs)
			{
				if (pair.Key.X == (float)tileLocation.X && (pair.Key.Y == (float)tileLocation.Y || pair.Key.Y == (float)(tileLocation.Y - 1)))
				{
					string text = Game1.objectInformation[pair.Value].Split('/')[4];
					Game1.drawObjectDialogue(Game1.parseText(" - " + text + " - " + Environment.NewLine + Game1.objectInformation[pair.Value].Split('/')[5]));
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public bool isTileSuitableForMuseumPiece(int x, int y)
		{
			Vector2 key = new Vector2(x, y);
			if (!museumPieces.ContainsKey(key))
			{
				int tileIndexAt = getTileIndexAt(new Point(x, y), "Buildings");
				if (tileIndexAt == 1073 || tileIndexAt == 1074 || tileIndexAt == 1072 || tileIndexAt == 1237 || tileIndexAt == 1238)
				{
					return true;
				}
			}
			return false;
		}

		public Microsoft.Xna.Framework.Rectangle getMuseumDonationBounds()
		{
			return new Microsoft.Xna.Framework.Rectangle(26, 5, 22, 13);
		}

		public Vector2 getFreeDonationSpot()
		{
			Microsoft.Xna.Framework.Rectangle museumDonationBounds = getMuseumDonationBounds();
			for (int i = museumDonationBounds.X; i <= museumDonationBounds.Right; i++)
			{
				for (int j = museumDonationBounds.Y; j <= museumDonationBounds.Bottom; j++)
				{
					if (isTileSuitableForMuseumPiece(i, j))
					{
						return new Vector2(i, j);
					}
				}
			}
			return new Vector2(26f, 5f);
		}

		public Vector2 findMuseumPieceLocationInDirection(Vector2 startingPoint, int direction, int distanceToCheck = 8, bool ignoreExistingItems = true)
		{
			Vector2 vector = startingPoint;
			Vector2 vector2 = Vector2.Zero;
			switch (direction)
			{
			case 0:
				vector2 = new Vector2(0f, -1f);
				break;
			case 1:
				vector2 = new Vector2(1f, 0f);
				break;
			case 2:
				vector2 = new Vector2(0f, 1f);
				break;
			case 3:
				vector2 = new Vector2(-1f, 0f);
				break;
			}
			for (int i = 0; i < distanceToCheck; i++)
			{
				for (int j = 0; j < distanceToCheck; j++)
				{
					vector += vector2;
					if (isTileSuitableForMuseumPiece((int)vector.X, (int)vector.Y) || (!ignoreExistingItems && museumPieces.ContainsKey(vector)))
					{
						return vector;
					}
				}
				vector = startingPoint;
				int num = ((i % 2 != 0) ? 1 : (-1));
				switch (direction)
				{
				case 0:
				case 2:
					vector.X += num * (i / 2 + 1);
					break;
				case 1:
				case 3:
					vector.Y += num * (i / 2 + 1);
					break;
				}
			}
			return startingPoint;
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			foreach (TemporaryAnimatedSprite temporarySprite in temporarySprites)
			{
				if (temporarySprite.layerDepth >= 1f)
				{
					temporarySprite.draw(b);
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (KeyValuePair<Vector2, int> pair in museumPieces.Pairs)
			{
				b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, pair.Key * 64f + new Vector2(32f, 52f)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (pair.Key.Y * 64f - 2f) / 10000f);
				b.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, pair.Key * 64f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, pair.Value, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, pair.Key.Y * 64f / 10000f);
			}
		}
	}
}
