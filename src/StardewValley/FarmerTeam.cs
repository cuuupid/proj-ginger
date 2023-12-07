using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Util;

namespace StardewValley
{
	public class FarmerTeam : INetObject<NetFields>
	{
		public enum RemoteBuildingPermissions
		{
			Off,
			OwnedBuildings,
			On
		}

		public enum SleepAnnounceModes
		{
			All,
			First,
			Off
		}

		public readonly NetIntDelta money = new NetIntDelta(500);

		public readonly NetLongDictionary<NetIntDelta, NetRef<NetIntDelta>> individualMoney = new NetLongDictionary<NetIntDelta, NetRef<NetIntDelta>>();

		public readonly NetIntDelta totalMoneyEarned = new NetIntDelta(0);

		public readonly NetBool hasRustyKey = new NetBool();

		public readonly NetBool hasSkullKey = new NetBool();

		public readonly NetBool canUnderstandDwarves = new NetBool();

		public readonly NetBool useSeparateWallets = new NetBool();

		public readonly NetBool newLostAndFoundItems = new NetBool();

		public readonly NetBool toggleMineShrineOvernight = new NetBool();

		public readonly NetBool mineShrineActivated = new NetBool();

		public readonly NetBool farmPerfect = new NetBool();

		public readonly NetList<string, NetString> specialRulesRemovedToday = new NetList<string, NetString>();

		public readonly NetList<int, NetInt> itemsToRemoveOvernight = new NetList<int, NetInt>();

		public readonly NetList<string, NetString> mailToRemoveOvernight = new NetList<string, NetString>();

		public NetIntDictionary<long, NetLong> cellarAssignments = new NetIntDictionary<long, NetLong>();

		public NetStringList broadcastedMail = new NetStringList();

		public NetStringDictionary<bool, NetBool> collectedNutTracker = new NetStringDictionary<bool, NetBool>();

		public NetStringDictionary<bool, NetBool> completedSpecialOrders = new NetStringDictionary<bool, NetBool>();

		public NetList<SpecialOrder, NetRef<SpecialOrder>> specialOrders = new NetList<SpecialOrder, NetRef<SpecialOrder>>();

		public NetList<SpecialOrder, NetRef<SpecialOrder>> availableSpecialOrders = new NetList<SpecialOrder, NetRef<SpecialOrder>>();

		public NetList<string, NetString> acceptedSpecialOrderTypes = new NetList<string, NetString>();

		public readonly NetCollection<Item> returnedDonations = new NetCollection<Item>();

		public readonly NetObjectList<Item> junimoChest = new NetObjectList<Item>();

		public readonly NetFarmerCollection announcedSleepingFarmers = new NetFarmerCollection();

		public readonly NetEnum<SleepAnnounceModes> sleepAnnounceMode = new NetEnum<SleepAnnounceModes>(SleepAnnounceModes.All);

		public readonly NetEnum<RemoteBuildingPermissions> farmhandsCanMoveBuildings = new NetEnum<RemoteBuildingPermissions>(RemoteBuildingPermissions.Off);

		private readonly NetStringDictionary<ReadyCheck, NetRef<ReadyCheck>> readyChecks = new NetStringDictionary<ReadyCheck, NetRef<ReadyCheck>>();

		private readonly NetLongDictionary<Proposal, NetRef<Proposal>> proposals = new NetLongDictionary<Proposal, NetRef<Proposal>>();

		public readonly NetList<MovieInvitation, NetRef<MovieInvitation>> movieInvitations = new NetList<MovieInvitation, NetRef<MovieInvitation>>();

		public readonly NetCollection<Item> luauIngredients = new NetCollection<Item>();

		public readonly NetCollection<Item> grangeDisplay = new NetCollection<Item>();

		public readonly NetMutex grangeMutex = new NetMutex();

		public readonly NetMutex returnedDonationsMutex = new NetMutex();

		public readonly NetMutex ordersBoardMutex = new NetMutex();

		public readonly NetMutex qiChallengeBoardMutex = new NetMutex();

		public readonly NetMutex junimoChestMutex = new NetMutex();

		private readonly NetEvent1Field<Rectangle, NetRectangle> festivalPropRemovalEvent = new NetEvent1Field<Rectangle, NetRectangle>();

		public readonly NetEvent1Field<int, NetInt> addQiGemsToTeam = new NetEvent1Field<int, NetInt>();

		public readonly NetEvent1Field<string, NetString> addCharacterEvent = new NetEvent1Field<string, NetString>();

		public readonly NetEvent1Field<string, NetString> requestAddCharacterEvent = new NetEvent1Field<string, NetString>();

		public readonly NetEvent0 requestLeoMove = new NetEvent0();

		public readonly NetEvent0 kickOutOfMinesEvent = new NetEvent0();

		public readonly NetEvent1Field<long, NetLong> requestSpouseSleepEvent = new NetEvent1Field<long, NetLong>();

		public readonly NetEvent1Field<int, NetInt> ringPhoneEvent = new NetEvent1Field<int, NetInt>();

		public readonly NetEvent1Field<long, NetLong> requestHorseWarpEvent = new NetEvent1Field<long, NetLong>();

		public readonly NetEvent1Field<long, NetLong> requestPetWarpHomeEvent = new NetEvent1Field<long, NetLong>();

		public readonly NetEvent1Field<long, NetLong> requestMovieEndEvent = new NetEvent1Field<long, NetLong>();

		public readonly NetEvent1Field<long, NetLong> endMovieEvent = new NetEvent1Field<long, NetLong>();

		public readonly NetEvent1Field<Guid, NetGuid> demolishStableEvent = new NetEvent1Field<Guid, NetGuid>();

		public readonly NetStringDictionary<int, NetInt> limitedNutDrops = new NetStringDictionary<int, NetInt>();

		private readonly NetEvent1<NutDropRequest> requestNutDrop = new NetEvent1<NutDropRequest>();

		public readonly NetFarmerPairDictionary<Friendship, NetRef<Friendship>> friendshipData = new NetFarmerPairDictionary<Friendship, NetRef<Friendship>>();

		public readonly NetWitnessedLock demolishLock = new NetWitnessedLock();

		public readonly NetMutex buildLock = new NetMutex();

		public readonly NetMutex movieMutex = new NetMutex();

		public readonly NetMutex goldenCoconutMutex = new NetMutex();

		public readonly SynchronizedShopStock synchronizedShopStock = new SynchronizedShopStock();

		public readonly NetLong theaterBuildDate = new NetLong(-1L);

		public readonly NetInt lastDayQueenOfSauceRerunUpdated = new NetInt(0);

		public readonly NetInt queenOfSauceRerunWeek = new NetInt(1);

		public readonly NetDouble sharedDailyLuck = new NetDouble(0.0010000000474974513);

		public readonly NetBool spawnMonstersAtNight = new NetBool(value: false);

		public readonly NetLeaderboards junimoKartScores = new NetLeaderboards();

		public PlayerStatusList junimoKartStatus = new PlayerStatusList();

		public PlayerStatusList endOfNightStatus = new PlayerStatusList();

		public PlayerStatusList festivalScoreStatus = new PlayerStatusList();

		public PlayerStatusList sleepStatus = new PlayerStatusList();

		public NetFields NetFields { get; } = new NetFields();


		public FarmerTeam()
		{
			NetFields.AddFields(money, totalMoneyEarned, hasRustyKey, hasSkullKey, canUnderstandDwarves, readyChecks, proposals, luauIngredients, grangeDisplay, grangeMutex.NetFields, festivalPropRemovalEvent, friendshipData, demolishLock.NetFields, buildLock.NetFields, movieInvitations, movieMutex.NetFields, requestMovieEndEvent, endMovieEvent, requestSpouseSleepEvent, useSeparateWallets, individualMoney, announcedSleepingFarmers.NetFields, sleepAnnounceMode, theaterBuildDate, demolishStableEvent, queenOfSauceRerunWeek, lastDayQueenOfSauceRerunUpdated, broadcastedMail, sharedDailyLuck, spawnMonstersAtNight, junimoKartScores.NetFields, cellarAssignments, synchronizedShopStock.NetFields, junimoKartStatus.NetFields, endOfNightStatus.NetFields, festivalScoreStatus.NetFields, sleepStatus.NetFields, farmhandsCanMoveBuildings, requestPetWarpHomeEvent, ringPhoneEvent, specialOrders, returnedDonations, returnedDonationsMutex.NetFields, goldenCoconutMutex.NetFields, requestNutDrop, limitedNutDrops, availableSpecialOrders, acceptedSpecialOrderTypes, ordersBoardMutex.NetFields, qiChallengeBoardMutex.NetFields, completedSpecialOrders, addCharacterEvent, requestAddCharacterEvent, requestLeoMove, collectedNutTracker, itemsToRemoveOvernight, mailToRemoveOvernight, newLostAndFoundItems, junimoChest, junimoChestMutex.NetFields, requestHorseWarpEvent, kickOutOfMinesEvent, toggleMineShrineOvernight, mineShrineActivated, specialRulesRemovedToday, addQiGemsToTeam, farmPerfect);
			newLostAndFoundItems.Interpolated(interpolate: false, wait: false);
			junimoKartStatus.sortMode = PlayerStatusList.SortMode.NumberSortDescending;
			festivalScoreStatus.sortMode = PlayerStatusList.SortMode.NumberSortDescending;
			endOfNightStatus.displayMode = PlayerStatusList.DisplayMode.Icons;
			endOfNightStatus.AddSpriteDefinition("sleep", "LooseSprites\\PlayerStatusList", 0, 0, 16, 16);
			endOfNightStatus.AddSpriteDefinition("level", "LooseSprites\\PlayerStatusList", 16, 0, 16, 16);
			endOfNightStatus.AddSpriteDefinition("shipment", "LooseSprites\\PlayerStatusList", 32, 0, 16, 16);
			endOfNightStatus.AddSpriteDefinition("ready", "LooseSprites\\PlayerStatusList", 48, 0, 16, 16);
			endOfNightStatus.iconAnimationFrames = 4;
			money.Minimum = 0;
			festivalPropRemovalEvent.onEvent += delegate(Rectangle rect)
			{
				if (Game1.CurrentEvent != null)
				{
					Game1.CurrentEvent.removeFestivalProps(rect);
				}
			};
			requestSpouseSleepEvent.onEvent += OnRequestSpouseSleepEvent;
			requestPetWarpHomeEvent.onEvent += OnRequestPetWarpHomeEvent;
			requestMovieEndEvent.onEvent += OnRequestMovieEndEvent;
			endMovieEvent.onEvent += OnEndMovieEvent;
			demolishStableEvent.onEvent += OnDemolishStableEvent;
			ringPhoneEvent.onEvent += OnRingPhoneEvent;
			requestNutDrop.onEvent += OnRequestNutDrop;
			requestAddCharacterEvent.onEvent += OnRequestAddCharacterEvent;
			addCharacterEvent.onEvent += OnAddCharacterEvent;
			requestLeoMove.onEvent += OnRequestLeoMoveEvent;
			requestHorseWarpEvent.onEvent += OnRequestHorseWarp;
			kickOutOfMinesEvent.onEvent += OnKickOutOfMinesEvent;
			addQiGemsToTeam.onEvent += _AddQiGemsToTeam;
			requestHorseWarpEvent.InterpolationWait = false;
			requestSpouseSleepEvent.InterpolationWait = false;
			requestPetWarpHomeEvent.InterpolationWait = false;
		}

		protected virtual void _AddQiGemsToTeam(int amount)
		{
			Game1.player.QiGems += amount;
		}

		public virtual void OnKickOutOfMinesEvent()
		{
			if (Game1.currentLocation is MineShaft)
			{
				MineShaft mineShaft = Game1.currentLocation as MineShaft;
				if (mineShaft.getMineArea() == 77377)
				{
					Game1.player.completelyStopAnimatingOrDoingAction();
					LocationRequest locationRequest = Game1.getLocationRequest("Mine");
					Game1.warpFarmer(locationRequest, 67, 10, 2);
				}
				else if (mineShaft.getMineArea() == 121)
				{
					Game1.player.completelyStopAnimatingOrDoingAction();
					LocationRequest locationRequest2 = Game1.getLocationRequest("SkullCave");
					Game1.warpFarmer(locationRequest2, 3, 4, 2);
				}
				else
				{
					Game1.player.completelyStopAnimatingOrDoingAction();
					LocationRequest locationRequest3 = Game1.getLocationRequest("Mine");
					Game1.warpFarmer(locationRequest3, 18, 4, 2);
				}
			}
		}

		public virtual void OnRequestHorseWarp(long uid)
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			Farmer farmer = Game1.getFarmer(uid);
			Horse horse = null;
			foreach (Building building in Game1.getFarm().buildings)
			{
				if (building is Stable)
				{
					Stable stable = building as Stable;
					if (stable.getStableHorse() != null && stable.getStableHorse().getOwner() == farmer)
					{
						horse = stable.getStableHorse();
						break;
					}
				}
			}
			if (horse == null || Utility.GetHorseWarpRestrictionsForFarmer(farmer).Any())
			{
				return;
			}
			horse.mutex.RequestLock(delegate
			{
				horse.mutex.ReleaseLock();
				GameLocation currentLocation = horse.currentLocation;
				Vector2 tileLocation = horse.getTileLocation();
				for (int i = 0; i < 8; i++)
				{
					Game1.multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite(10, new Vector2(tileLocation.X + Utility.RandomFloat(-1f, 1f), tileLocation.Y + Utility.RandomFloat(-1f, 0f)) * 64f, Color.White, 8, flipped: false, 50f)
					{
						layerDepth = 1f,
						motion = new Vector2(Utility.RandomFloat(-0.5f, 0.5f), Utility.RandomFloat(-0.5f, 0.5f))
					});
				}
				currentLocation.playSoundAt("wand", horse.getTileLocation());
				currentLocation = farmer.currentLocation;
				tileLocation = farmer.getTileLocation();
				currentLocation.playSoundAt("wand", tileLocation);
				for (int j = 0; j < 8; j++)
				{
					Game1.multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite(10, new Vector2(tileLocation.X + Utility.RandomFloat(-1f, 1f), tileLocation.Y + Utility.RandomFloat(-1f, 0f)) * 64f, Color.White, 8, flipped: false, 50f)
					{
						layerDepth = 1f,
						motion = new Vector2(Utility.RandomFloat(-0.5f, 0.5f), Utility.RandomFloat(-0.5f, 0.5f))
					});
				}
				Game1.warpCharacter(horse, farmer.currentLocation, tileLocation);
				int num = 0;
				for (int num2 = (int)tileLocation.X + 3; num2 >= (int)tileLocation.X - 3; num2--)
				{
					Game1.multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite(6, new Vector2(num2, tileLocation.Y) * 64f, Color.White, 8, flipped: false, 50f)
					{
						layerDepth = 1f,
						delayBeforeAnimationStart = num * 25,
						motion = new Vector2(-0.25f, 0f)
					});
					num++;
				}
			});
		}

		public virtual void OnRequestLeoMoveEvent()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			Game1.player.team.requestAddCharacterEvent.Fire("Leo");
			NPC characterFromName = Game1.getCharacterFromName("Leo");
			if (characterFromName != null)
			{
				characterFromName.DefaultMap = "LeoTreeHouse";
				characterFromName.DefaultPosition = new Vector2(5f, 4f) * 64f;
				characterFromName.faceDirection(2);
				characterFromName.InvalidateMasterSchedule();
				if (characterFromName.Schedule != null)
				{
					characterFromName.Schedule = null;
				}
				characterFromName.controller = null;
				characterFromName.temporaryController = null;
				Game1.warpCharacter(characterFromName, Game1.getLocationFromName("Mountain"), new Vector2(16f, 8f));
				characterFromName.Halt();
				characterFromName.ignoreScheduleToday = false;
			}
		}

		public virtual void MarkCollectedNut(string key)
		{
			collectedNutTracker[key] = true;
		}

		public int GetIndividualMoney(Farmer who)
		{
			return GetMoney(who).Value;
		}

		public void AddIndividualMoney(Farmer who, int value)
		{
			GetMoney(who).Value += value;
		}

		public void SetIndividualMoney(Farmer who, int value)
		{
			GetMoney(who).Value = value;
		}

		public NetIntDelta GetMoney(Farmer who)
		{
			if ((bool)useSeparateWallets)
			{
				if (!individualMoney.ContainsKey(who.UniqueMultiplayerID))
				{
					individualMoney[who.uniqueMultiplayerID] = new NetIntDelta(500);
					individualMoney[who.uniqueMultiplayerID].Minimum = 0;
				}
				return individualMoney[who.uniqueMultiplayerID];
			}
			return money;
		}

		public bool SpecialOrderActive(string special_order_key)
		{
			foreach (SpecialOrder specialOrder in specialOrders)
			{
				if (specialOrder.questKey == special_order_key && specialOrder.questState.Value == SpecialOrder.QuestState.InProgress)
				{
					return true;
				}
			}
			return false;
		}

		public bool SpecialOrderRuleActive(string special_rule, SpecialOrder order_to_ignore = null)
		{
			foreach (SpecialOrder specialOrder in specialOrders)
			{
				if (specialOrder == order_to_ignore || specialOrder.questState.Value != 0 || specialOrder.specialRule.Value == null)
				{
					continue;
				}
				string[] array = specialOrder.specialRule.Value.Split(',');
				string[] array2 = array;
				foreach (string text in array2)
				{
					if (text.Trim() == special_rule)
					{
						return true;
					}
				}
			}
			return false;
		}

		public SpecialOrder GetAvailableSpecialOrder(int index = 0, string type = "")
		{
			foreach (SpecialOrder availableSpecialOrder in availableSpecialOrders)
			{
				if (availableSpecialOrder.orderType.Value == type)
				{
					if (index <= 0)
					{
						return availableSpecialOrder;
					}
					index--;
				}
			}
			return null;
		}

		public void CheckReturnedDonations()
		{
			returnedDonationsMutex.RequestLock(delegate
			{
				Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
				foreach (Item returnedDonation in returnedDonations)
				{
					dictionary[returnedDonation] = new int[2] { 0, returnedDonation.Stack };
				}
				Game1.activeClickableMenu = new ShopMenu(dictionary, 0, null, OnDonatedItemWithdrawn, OnReturnedDonationDeposited, "ReturnedDonations")
				{
					source = this,
					behaviorBeforeCleanup = delegate
					{
						returnedDonationsMutex.ReleaseLock();
					}
				};
			});
		}

		public bool OnDonatedItemWithdrawn(ISalable salable, Farmer who, int amount)
		{
			if (salable is Item && (salable.Stack <= 0 || salable.maximumStackSize() <= 1))
			{
				returnedDonations.Remove(salable as Item);
			}
			return false;
		}

		public bool OnReturnedDonationDeposited(ISalable deposited_salable)
		{
			return false;
		}

		public void OnRequestMovieEndEvent(long uid)
		{
			if (Game1.IsMasterGame)
			{
				(Game1.getLocationFromName("MovieTheater") as MovieTheater).RequestEndMovie(uid);
			}
		}

		public void OnRequestPetWarpHomeEvent(long uid)
		{
			if (Game1.IsMasterGame)
			{
				Farmer farmer = Game1.getFarmerMaybeOffline(uid);
				if (farmer == null)
				{
					farmer = Game1.MasterPlayer;
				}
				Pet characterFromName = Game1.getCharacterFromName<Pet>(farmer.getPetName(), mustBeVillager: false);
				if (characterFromName == null || !(characterFromName.currentLocation is FarmHouse))
				{
					characterFromName?.warpToFarmHouse(farmer);
				}
			}
		}

		public void OnRequestSpouseSleepEvent(long uid)
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			Farmer farmerMaybeOffline = Game1.getFarmerMaybeOffline(uid);
			if (farmerMaybeOffline == null)
			{
				return;
			}
			NPC characterFromName = Game1.getCharacterFromName(farmerMaybeOffline.spouse);
			if (characterFromName != null && !characterFromName.isSleeping.Value)
			{
				FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(farmerMaybeOffline);
				Game1.warpCharacter(characterFromName, homeOfFarmer, new Vector2(homeOfFarmer.getSpouseBedSpot(farmerMaybeOffline.spouse).X, homeOfFarmer.getSpouseBedSpot(farmerMaybeOffline.spouse).Y));
				characterFromName.NetFields.CancelInterpolation();
				characterFromName.Halt();
				characterFromName.faceDirection(0);
				characterFromName.controller = null;
				characterFromName.temporaryController = null;
				characterFromName.ignoreScheduleToday = true;
				if (homeOfFarmer.GetSpouseBed() != null)
				{
					FarmHouse.spouseSleepEndFunction(characterFromName, homeOfFarmer);
				}
			}
		}

		public virtual void OnRequestAddCharacterEvent(string character_name)
		{
			if (Game1.IsMasterGame && character_name == "Leo" && !Game1.player.hasOrWillReceiveMail("addedParrotBoy"))
			{
				Game1.player.mailReceived.Add("addedParrotBoy");
				addCharacterEvent.Fire(character_name);
			}
		}

		public virtual void OnAddCharacterEvent(string character_name)
		{
			if (character_name == "Leo")
			{
				Game1.addParrotBoyIfNecessary();
			}
		}

		public void RequestLimitedNutDrops(string key, GameLocation location, int x, int y, int limit, int reward_amount = 1)
		{
			if (!limitedNutDrops.ContainsKey(key) || limitedNutDrops[key] < limit)
			{
				if (location == null)
				{
					requestNutDrop.Fire(new NutDropRequest(key, null, new Point(x, y), limit, reward_amount));
				}
				else
				{
					requestNutDrop.Fire(new NutDropRequest(key, location.NameOrUniqueName, new Point(x, y), limit, reward_amount));
				}
			}
		}

		public int GetDroppedLimitedNutCount(string key)
		{
			if (limitedNutDrops.ContainsKey(key))
			{
				return limitedNutDrops[key];
			}
			return 0;
		}

		protected void OnRequestNutDrop(NutDropRequest request)
		{
			if (!Game1.IsMasterGame || (limitedNutDrops.ContainsKey(request.key) && limitedNutDrops[request.key] >= request.limit))
			{
				return;
			}
			int rewardAmount = request.rewardAmount;
			if (!limitedNutDrops.ContainsKey(request.key))
			{
				rewardAmount = Math.Min(request.limit, rewardAmount);
				limitedNutDrops[request.key] = rewardAmount;
			}
			else
			{
				rewardAmount = Math.Min(request.limit - limitedNutDrops[request.key], rewardAmount);
				limitedNutDrops[request.key] += rewardAmount;
			}
			GameLocation gameLocation = null;
			if (request.locationName != "null")
			{
				gameLocation = Game1.getLocationFromName(request.locationName);
			}
			if (gameLocation != null)
			{
				for (int i = 0; i < rewardAmount; i++)
				{
					Game1.createItemDebris(new Object(73, 1), new Vector2(request.position.X, request.position.Y), -1, gameLocation);
				}
			}
			else
			{
				Game1.netWorldState.Value.GoldenWalnutsFound.Value += rewardAmount;
				Game1.netWorldState.Value.GoldenWalnuts.Value += rewardAmount;
			}
		}

		public void OnRingPhoneEvent(int which_call)
		{
			Phone.Ring(which_call);
		}

		public void OnEndMovieEvent(long uid)
		{
			if (Game1.player.UniqueMultiplayerID != uid)
			{
				return;
			}
			Game1.player.lastSeenMovieWeek.Set(Game1.Date.TotalWeeks);
			if (Game1.CurrentEvent != null)
			{
				Event currentEvent = Game1.CurrentEvent;
				currentEvent.onEventFinished = (Action)Delegate.Combine(currentEvent.onEventFinished, (Action)delegate
				{
					LocationRequest locationRequest = Game1.getLocationRequest("MovieTheater");
					locationRequest.OnWarp += delegate
					{
					};
					Game1.warpFarmer(locationRequest, 13, 4, 2);
					Game1.fadeToBlackAlpha = 1f;
				});
				Game1.CurrentEvent.endBehaviors(new string[1] { "end" }, Game1.currentLocation);
			}
		}

		public void OnDemolishStableEvent(Guid stable_guid)
		{
			if (Game1.player.mount != null && Game1.player.mount.HorseId == stable_guid)
			{
				Game1.player.mount.dismount(from_demolish: true);
			}
		}

		public void DeleteFarmhand(Farmer farmhand)
		{
			friendshipData.Filter((KeyValuePair<FarmerPair, Friendship> pair) => !pair.Key.Contains(farmhand.UniqueMultiplayerID));
		}

		public Friendship GetFriendship(long farmer1, long farmer2)
		{
			FarmerPair key = FarmerPair.MakePair(farmer1, farmer2);
			if (!friendshipData.ContainsKey(key))
			{
				friendshipData.Add(key, new Friendship());
			}
			return friendshipData[key];
		}

		public void AddAnyBroadcastedMail()
		{
			foreach (string item2 in broadcastedMail)
			{
				Multiplayer.PartyWideMessageQueue partyWideMessageQueue = Multiplayer.PartyWideMessageQueue.SeenMail;
				string text = item2;
				if (text.StartsWith("%&SM&%"))
				{
					text = text.Substring("%&SM&%".Length);
					partyWideMessageQueue = Multiplayer.PartyWideMessageQueue.SeenMail;
				}
				else if (text.StartsWith("%&MFT&%"))
				{
					text = text.Substring("%&MFT&%".Length);
					partyWideMessageQueue = Multiplayer.PartyWideMessageQueue.MailForTomorrow;
				}
				if (partyWideMessageQueue == Multiplayer.PartyWideMessageQueue.SeenMail)
				{
					if (text.Contains("%&NL&%") || text.StartsWith("NightMarketYear"))
					{
						text = text.Replace("%&NL&%", "");
						if (!Game1.player.mailReceived.Contains(text))
						{
							Game1.player.mailReceived.Add(text);
						}
					}
					else if (!Game1.player.hasOrWillReceiveMail(text))
					{
						Game1.player.mailbox.Add(text);
					}
				}
				else if (!Game1.MasterPlayer.mailForTomorrow.Contains(text))
				{
					if (Game1.player.hasOrWillReceiveMail(text))
					{
						continue;
					}
					if (text.Contains("%&NL&%"))
					{
						string item = text.Replace("%&NL&%", "");
						if (!Game1.player.mailReceived.Contains(item))
						{
							Game1.player.mailReceived.Add(item);
						}
					}
					else if (!Game1.player.mailbox.Contains(text))
					{
						Game1.player.mailbox.Add(text);
					}
				}
				else if (!Game1.player.hasOrWillReceiveMail(text))
				{
					Game1.player.mailForTomorrow.Add(text);
				}
			}
		}

		public bool IsMarried(long farmer)
		{
			foreach (KeyValuePair<FarmerPair, Friendship> pair in friendshipData.Pairs)
			{
				if (pair.Key.Contains(farmer) && pair.Value.IsMarried())
				{
					return true;
				}
			}
			return false;
		}

		public bool IsEngaged(long farmer)
		{
			foreach (KeyValuePair<FarmerPair, Friendship> pair in friendshipData.Pairs)
			{
				if (pair.Key.Contains(farmer) && pair.Value.IsEngaged())
				{
					return true;
				}
			}
			return false;
		}

		public long? GetSpouse(long farmer)
		{
			foreach (KeyValuePair<FarmerPair, Friendship> pair in friendshipData.Pairs)
			{
				if (pair.Key.Contains(farmer) && (pair.Value.IsEngaged() || pair.Value.IsMarried()))
				{
					return pair.Key.GetOther(farmer);
				}
			}
			return null;
		}

		public void FestivalPropsRemoved(Rectangle rect)
		{
			festivalPropRemovalEvent.Fire(rect);
		}

		public void SendProposal(Farmer receiver, ProposalType proposalType, Item gift = null)
		{
			Proposal proposal = new Proposal();
			proposal.sender.Value = Game1.player;
			proposal.receiver.Value = receiver;
			proposal.proposalType.Value = proposalType;
			proposal.gift.Value = gift;
			proposals[Game1.player.UniqueMultiplayerID] = proposal;
		}

		public Proposal GetOutgoingProposal()
		{
			if (proposals.TryGetValue(Game1.player.UniqueMultiplayerID, out var value))
			{
				return value;
			}
			return null;
		}

		public void RemoveOutgoingProposal()
		{
			proposals.Remove(Game1.player.UniqueMultiplayerID);
		}

		public Proposal GetIncomingProposal()
		{
			foreach (Proposal value in proposals.Values)
			{
				if (value.receiver.Value == Game1.player && value.response.Value == ProposalResponse.None)
				{
					return value;
				}
			}
			return null;
		}

		private bool locationsMatch(GameLocation location1, GameLocation location2)
		{
			if (location1 == null || location2 == null)
			{
				return false;
			}
			if (location1.Name == location2.Name)
			{
				return true;
			}
			if ((location1 is Mine || (location1 is MineShaft && Convert.ToInt32(location1.Name.Substring("UndergroundMine".Length)) < 121)) && (location2 is Mine || (location2 is MineShaft && Convert.ToInt32(location2.Name.Substring("UndergroundMine".Length)) < 121)))
			{
				return true;
			}
			if ((location1.Name.Equals("SkullCave") || (location1 is MineShaft && Convert.ToInt32(location1.Name.Substring("UndergroundMine".Length)) >= 121)) && (location2.Name.Equals("SkullCave") || (location2 is MineShaft && Convert.ToInt32(location2.Name.Substring("UndergroundMine".Length)) >= 121)))
			{
				return true;
			}
			return false;
		}

		public double AverageDailyLuck(GameLocation inThisLocation = null)
		{
			double num = 0.0;
			int num2 = 0;
			foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
			{
				if (inThisLocation == null || locationsMatch(inThisLocation, onlineFarmer.currentLocation))
				{
					num += onlineFarmer.DailyLuck;
					num2++;
				}
			}
			return num / (double)Math.Max(num2, 1);
		}

		public double AverageLuckLevel(GameLocation inThisLocation = null)
		{
			double num = 0.0;
			int num2 = 0;
			foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
			{
				if (inThisLocation == null || locationsMatch(inThisLocation, onlineFarmer.currentLocation))
				{
					num += (double)onlineFarmer.LuckLevel;
					num2++;
				}
			}
			return num / (double)Math.Max(num2, 1);
		}

		public double AverageSkillLevel(int skillIndex, GameLocation inThisLocation = null)
		{
			double num = 0.0;
			int num2 = 0;
			foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
			{
				if (inThisLocation == null || locationsMatch(inThisLocation, onlineFarmer.currentLocation))
				{
					num += (double)onlineFarmer.GetSkillLevel(skillIndex);
					num2++;
				}
			}
			return num / (double)Math.Max(num2, 1);
		}

		public void Update()
		{
			requestLeoMove.Poll();
			requestMovieEndEvent.Poll();
			endMovieEvent.Poll();
			ringPhoneEvent.Poll();
			festivalPropRemovalEvent.Poll();
			demolishStableEvent.Poll();
			requestSpouseSleepEvent.Poll();
			requestHorseWarpEvent.Poll();
			kickOutOfMinesEvent.Poll();
			requestPetWarpHomeEvent.Poll();
			requestNutDrop.Poll();
			requestAddCharacterEvent.Poll();
			addCharacterEvent.Poll();
			addQiGemsToTeam.Poll();
			grangeMutex.Update(Game1.getOnlineFarmers());
			returnedDonationsMutex.Update(Game1.getOnlineFarmers());
			ordersBoardMutex.Update(Game1.getOnlineFarmers());
			qiChallengeBoardMutex.Update(Game1.getOnlineFarmers());
			junimoChestMutex.Update(Game1.getOnlineFarmers());
			demolishLock.Update();
			buildLock.Update(Game1.getOnlineFarmers());
			movieMutex.Update(Game1.getOnlineFarmers());
			goldenCoconutMutex.Update(Game1.getOnlineFarmers());
			if (grangeMutex.IsLockHeld() && Game1.activeClickableMenu == null)
			{
				grangeMutex.ReleaseLock();
			}
			foreach (SpecialOrder specialOrder in specialOrders)
			{
				specialOrder.Update();
			}
			foreach (ReadyCheck value in readyChecks.Values)
			{
				value.Update();
			}
			if (Game1.IsMasterGame && proposals.Count() > 0)
			{
				proposals.Filter((KeyValuePair<long, Proposal> pair) => playerIsOnline(pair.Key) && playerIsOnline(pair.Value.receiver.UID));
			}
			Proposal incomingProposal = GetIncomingProposal();
			if (incomingProposal != null && incomingProposal.canceled.Value)
			{
				incomingProposal.cancelConfirmed.Value = true;
			}
			if (Game1.dialogueUp)
			{
				return;
			}
			if (incomingProposal != null)
			{
				if (!handleIncomingProposal(incomingProposal))
				{
					incomingProposal.responseMessageKey.Value = genderedKey("Strings\\UI:Proposal_PlayerBusy", Game1.player);
					incomingProposal.response.Value = ProposalResponse.Rejected;
				}
			}
			else if (Game1.activeClickableMenu == null)
			{
				Proposal outgoingProposal = GetOutgoingProposal();
				if (outgoingProposal != null)
				{
					Game1.activeClickableMenu = new PendingProposalDialog();
				}
			}
		}

		private string genderedKey(string baseKey, Farmer farmer)
		{
			return baseKey + (farmer.IsMale ? "_Male" : "_Female");
		}

		private bool handleIncomingProposal(Proposal proposal)
		{
			if (Game1.gameMode != 3 || Game1.activeClickableMenu != null || Game1.currentMinigame != null)
			{
				return (ProposalType)proposal.proposalType == ProposalType.Baby;
			}
			if (Game1.currentLocation == null)
			{
				return false;
			}
			if (proposal.proposalType.Value != ProposalType.Dance && Game1.CurrentEvent != null)
			{
				return false;
			}
			string sub = "";
			string responseYes = null;
			string responseNo = null;
			string baseKey;
			if ((ProposalType)proposal.proposalType == ProposalType.Dance)
			{
				if (Game1.CurrentEvent == null || !Game1.CurrentEvent.isSpecificFestival("spring24"))
				{
					return false;
				}
				baseKey = "Strings\\UI:AskedToDance";
				responseYes = "Strings\\UI:AskedToDance_Accepted";
				responseNo = "Strings\\UI:AskedToDance_Rejected";
				if (Game1.player.dancePartner.Value != null)
				{
					return false;
				}
			}
			else if ((ProposalType)proposal.proposalType == ProposalType.Marriage)
			{
				if (Game1.player.isMarried() || Game1.player.isEngaged())
				{
					proposal.response.Value = ProposalResponse.Rejected;
					proposal.responseMessageKey.Value = genderedKey("Strings\\UI:AskedToMarry_NotSingle", Game1.player);
					return true;
				}
				baseKey = "Strings\\UI:AskedToMarry";
				responseYes = "Strings\\UI:AskedToMarry_Accepted";
				responseNo = "Strings\\UI:AskedToMarry_Rejected";
			}
			else if ((ProposalType)proposal.proposalType == ProposalType.Gift && proposal.gift != null)
			{
				if (!Game1.player.couldInventoryAcceptThisItem(proposal.gift))
				{
					proposal.response.Value = ProposalResponse.Rejected;
					proposal.responseMessageKey.Value = genderedKey("Strings\\UI:GiftPlayerItem_NoInventorySpace", Game1.player);
					return true;
				}
				baseKey = "Strings\\UI:GivenGift";
				sub = proposal.gift.Value.DisplayName;
			}
			else
			{
				if ((ProposalType)proposal.proposalType != ProposalType.Baby)
				{
					return false;
				}
				if (proposal.sender.Value.IsMale != Game1.player.IsMale)
				{
					baseKey = "Strings\\UI:AskedToHaveBaby";
					responseYes = "Strings\\UI:AskedToHaveBaby_Accepted";
					responseNo = "Strings\\UI:AskedToHaveBaby_Rejected";
				}
				else
				{
					baseKey = "Strings\\UI:AskedToAdoptBaby";
					responseYes = "Strings\\UI:AskedToAdoptBaby_Accepted";
					responseNo = "Strings\\UI:AskedToAdoptBaby_Rejected";
				}
			}
			baseKey = genderedKey(baseKey, proposal.sender);
			if (responseYes != null)
			{
				responseYes = genderedKey(responseYes, Game1.player);
			}
			if (responseNo != null)
			{
				responseNo = genderedKey(responseNo, Game1.player);
			}
			string question = Game1.content.LoadString(baseKey, proposal.sender.Value.Name, sub);
			Game1.currentLocation.createQuestionDialogue(question, Game1.currentLocation.createYesNoResponses(), delegate(Farmer _, string answer)
			{
				if (proposal.canceled.Value)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:ProposalWithdrawn", proposal.sender.Value.Name));
					proposal.response.Value = ProposalResponse.Rejected;
					proposal.responseMessageKey.Value = responseNo;
				}
				else if (answer == "Yes")
				{
					proposal.response.Value = ProposalResponse.Accepted;
					proposal.responseMessageKey.Value = responseYes;
					if (proposal.proposalType.Value == ProposalType.Gift || proposal.proposalType.Value == ProposalType.Marriage)
					{
						Item value = proposal.gift.Value;
						proposal.gift.Value = null;
						value = Game1.player.addItemToInventory(value);
						if (value != null)
						{
							Game1.currentLocation.debris.Add(new Debris(value, Game1.player.Position));
						}
					}
					if (proposal.proposalType.Value == ProposalType.Dance)
					{
						Game1.player.dancePartner.Value = proposal.sender.Value;
					}
					if (proposal.proposalType.Value == ProposalType.Marriage)
					{
						Friendship friendship = GetFriendship(proposal.sender.Value.UniqueMultiplayerID, Game1.player.UniqueMultiplayerID);
						friendship.Status = FriendshipStatus.Engaged;
						friendship.Proposer = proposal.sender.Value.UniqueMultiplayerID;
						WorldDate worldDate = new WorldDate(Game1.Date);
						worldDate.TotalDays += 3;
						while (!Game1.canHaveWeddingOnDay(worldDate.DayOfMonth, worldDate.Season))
						{
							worldDate.TotalDays++;
						}
						friendship.WeddingDate = worldDate;
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:PlayerWeddingArranged"));
						Game1.multiplayer.globalChatInfoMessage("Engaged", Game1.player.Name, proposal.sender.Value.Name);
					}
					if (proposal.proposalType.Value == ProposalType.Baby)
					{
						Friendship friendship2 = GetFriendship(proposal.sender.Value.UniqueMultiplayerID, Game1.player.UniqueMultiplayerID);
						WorldDate worldDate2 = new WorldDate(Game1.Date);
						worldDate2.TotalDays += 14;
						friendship2.NextBirthingDate = worldDate2;
					}
					Game1.player.doEmote(20);
				}
				else
				{
					proposal.response.Value = ProposalResponse.Rejected;
					proposal.responseMessageKey.Value = responseNo;
				}
			});
			return true;
		}

		public bool playerIsOnline(long uid)
		{
			if (Game1.MasterPlayer.UniqueMultiplayerID == uid)
			{
				return true;
			}
			if (Game1.serverHost != null && Game1.serverHost.Value.UniqueMultiplayerID == uid)
			{
				return true;
			}
			if (Game1.otherFarmers.ContainsKey(uid))
			{
				return !Game1.multiplayer.isDisconnecting(uid);
			}
			return false;
		}

		public void SetLocalRequiredFarmers(string checkName, IEnumerable<Farmer> required_farmers)
		{
			if (!readyChecks.ContainsKey(checkName))
			{
				readyChecks.Add(checkName, new ReadyCheck(checkName));
			}
			readyChecks[checkName].SetRequiredFarmers(required_farmers);
		}

		public void SetLocalReady(string checkName, bool ready)
		{
			if (!readyChecks.ContainsKey(checkName))
			{
				readyChecks.Add(checkName, new ReadyCheck(checkName));
			}
			readyChecks[checkName].SetLocalReady(ready);
		}

		public bool IsReady(string checkName)
		{
			if (readyChecks.TryGetValue(checkName, out var value))
			{
				return value.IsReady();
			}
			return false;
		}

		public bool IsReadyCheckCancelable(string checkName)
		{
			if (readyChecks.TryGetValue(checkName, out var value))
			{
				return value.IsCancelable();
			}
			return false;
		}

		public bool IsOtherFarmerReady(string checkName, Farmer farmer)
		{
			if (readyChecks.TryGetValue(checkName, out var value))
			{
				return value.IsOtherFarmerReady(farmer);
			}
			return false;
		}

		public int GetNumberReady(string checkName)
		{
			if (!readyChecks.TryGetValue(checkName, out var value))
			{
				return 0;
			}
			return value.GetNumberReady();
		}

		public int GetNumberRequired(string checkName)
		{
			if (!readyChecks.TryGetValue(checkName, out var value))
			{
				return 0;
			}
			return value.GetNumberRequired();
		}

		public void NewDay()
		{
			if (Game1.IsClient)
			{
				return;
			}
			readyChecks.Clear();
			luauIngredients.Clear();
			if (grangeDisplay.Count > 0)
			{
				for (int i = 0; i < grangeDisplay.Count; i++)
				{
					Item item = grangeDisplay[i];
					grangeDisplay[i] = null;
					if (item != null)
					{
						returnedDonations.Add(item);
						newLostAndFoundItems.Value = true;
					}
				}
			}
			grangeDisplay.Clear();
			movieInvitations.Clear();
		}
	}
}
