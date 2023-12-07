using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Quests;

namespace StardewValley
{
	[XmlInclude(typeof(SpecialOrder))]
	[XmlInclude(typeof(OrderObjective))]
	[XmlInclude(typeof(ShipObjective))]
	[XmlInclude(typeof(SlayObjective))]
	[XmlInclude(typeof(DeliverObjective))]
	[XmlInclude(typeof(FishObjective))]
	[XmlInclude(typeof(GiftObjective))]
	[XmlInclude(typeof(JKScoreObjective))]
	[XmlInclude(typeof(ReachMineFloorObjective))]
	[XmlInclude(typeof(CollectObjective))]
	[XmlInclude(typeof(DonateObjective))]
	[XmlInclude(typeof(MailReward))]
	[XmlInclude(typeof(MoneyReward))]
	[XmlInclude(typeof(GemsReward))]
	[XmlInclude(typeof(ResetEventReward))]
	[XmlInclude(typeof(OrderReward))]
	[XmlInclude(typeof(FriendshipReward))]
	public class SpecialOrder : INetObject<NetFields>, IQuest
	{
		public enum QuestState
		{
			InProgress,
			Complete,
			Failed
		}

		public enum QuestDuration
		{
			Week,
			Month,
			TwoWeeks,
			TwoDays,
			ThreeDays
		}

		[XmlIgnore]
		public Action<Farmer, Item, int> onItemShipped;

		[XmlIgnore]
		public Action<Farmer, Monster> onMonsterSlain;

		[XmlIgnore]
		public Action<Farmer, Item> onFishCaught;

		[XmlIgnore]
		public Action<Farmer, NPC, Item> onGiftGiven;

		[XmlIgnore]
		public Func<Farmer, NPC, Item, int> onItemDelivered;

		[XmlIgnore]
		public Action<Farmer, Item> onItemCollected;

		[XmlIgnore]
		public Action<Farmer, int> onMineFloorReached;

		[XmlIgnore]
		public Action<Farmer, int> onJKScoreAchieved;

		[XmlIgnore]
		protected bool _objectiveRegistrationDirty;

		[XmlElement("preSelectedItems")]
		public NetStringDictionary<int, NetInt> preSelectedItems = new NetStringDictionary<int, NetInt>();

		[XmlElement("selectedRandomElements")]
		public NetStringDictionary<int, NetInt> selectedRandomElements = new NetStringDictionary<int, NetInt>();

		[XmlElement("objectives")]
		public NetList<OrderObjective, NetRef<OrderObjective>> objectives = new NetList<OrderObjective, NetRef<OrderObjective>>();

		[XmlElement("generationSeed")]
		public NetInt generationSeed = new NetInt();

		[XmlElement("seenParticipantsIDs")]
		public NetLongDictionary<bool, NetBool> seenParticipants = new NetLongDictionary<bool, NetBool>();

		[XmlElement("participantsIDs")]
		public NetLongDictionary<bool, NetBool> participants = new NetLongDictionary<bool, NetBool>();

		[XmlElement("unclaimedRewardsIDs")]
		public NetLongDictionary<bool, NetBool> unclaimedRewards = new NetLongDictionary<bool, NetBool>();

		[XmlElement("donatedItems")]
		public readonly NetCollection<Item> donatedItems = new NetCollection<Item>();

		[XmlElement("appliedSpecialRules")]
		public bool appliedSpecialRules;

		[XmlIgnore]
		public readonly NetMutex donateMutex = new NetMutex();

		[XmlIgnore]
		protected int _isIslandOrder = -1;

		[XmlElement("rewards")]
		public NetList<OrderReward, NetRef<OrderReward>> rewards = new NetList<OrderReward, NetRef<OrderReward>>();

		[XmlIgnore]
		protected int _moneyReward = -1;

		[XmlElement("questKey")]
		public NetString questKey = new NetString();

		[XmlElement("questName")]
		public NetString questName = new NetString("Strings\\SpecialOrders:PlaceholderName");

		[XmlElement("questDescription")]
		public NetString questDescription = new NetString("Strings\\SpecialOrders:PlaceholderDescription");

		[XmlElement("requester")]
		public NetString requester = new NetString();

		[XmlElement("orderType")]
		public NetString orderType = new NetString("");

		[XmlElement("specialRule")]
		public NetString specialRule = new NetString("");

		[XmlElement("readyForRemoval")]
		public NetBool readyForRemoval = new NetBool(value: false);

		[XmlElement("itemToRemoveOnEnd")]
		public NetInt itemToRemoveOnEnd = new NetInt(-1);

		[XmlElement("mailToRemoveOnEnd")]
		public NetString mailToRemoveOnEnd = new NetString(null);

		[XmlIgnore]
		protected string _localizedName;

		[XmlIgnore]
		protected string _localizedDescription;

		[XmlElement("dueDate")]
		public NetInt dueDate = new NetInt();

		[XmlElement("duration")]
		public NetEnum<QuestDuration> questDuration = new NetEnum<QuestDuration>();

		[XmlIgnore]
		protected List<OrderObjective> _registeredObjectives = new List<OrderObjective>();

		[XmlIgnore]
		protected Dictionary<Item, bool> _highlightLookup;

		[XmlIgnore]
		protected SpecialOrderData _orderData;

		[XmlElement("questState")]
		public NetEnum<QuestState> questState = new NetEnum<QuestState>(QuestState.InProgress);

		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields();


		public SpecialOrder()
		{
			InitializeNetFields();
		}

		public virtual void SetDuration(QuestDuration duration)
		{
			questDuration.Value = duration;
			WorldDate worldDate = new WorldDate();
			switch (duration)
			{
			case QuestDuration.Week:
				worldDate = new WorldDate(Game1.year, Game1.currentSeason, (Game1.dayOfMonth - 1) / 7 * 7);
				worldDate.TotalDays++;
				worldDate.TotalDays += 7;
				break;
			case QuestDuration.TwoWeeks:
				worldDate = new WorldDate(Game1.year, Game1.currentSeason, (Game1.dayOfMonth - 1) / 7 * 7);
				worldDate.TotalDays++;
				worldDate.TotalDays += 14;
				break;
			case QuestDuration.Month:
				worldDate = new WorldDate(Game1.year, Game1.currentSeason, 0);
				worldDate.TotalDays++;
				worldDate.TotalDays += 28;
				break;
			case QuestDuration.TwoDays:
				worldDate = new WorldDate(Game1.year, Game1.currentSeason, Game1.dayOfMonth);
				worldDate.TotalDays += 2;
				break;
			case QuestDuration.ThreeDays:
				worldDate = new WorldDate(Game1.year, Game1.currentSeason, Game1.dayOfMonth);
				worldDate.TotalDays += 3;
				break;
			}
			dueDate.Value = worldDate.TotalDays;
		}

		public virtual void OnFail()
		{
			foreach (OrderObjective objective in objectives)
			{
				objective.OnFail();
			}
			for (int i = 0; i < donatedItems.Count; i++)
			{
				Item item = donatedItems[i];
				donatedItems[i] = null;
				if (item != null)
				{
					Game1.player.team.returnedDonations.Add(item);
					Game1.player.team.newLostAndFoundItems.Value = true;
				}
			}
			if (Game1.IsMasterGame)
			{
				HostHandleQuestEnd();
			}
			questState.Value = QuestState.Failed;
			_RemoveSpecialRuleIfNecessary();
		}

		public virtual int GetCompleteObjectivesCount()
		{
			int num = 0;
			foreach (OrderObjective objective in objectives)
			{
				if (objective.IsComplete())
				{
					num++;
				}
			}
			return num;
		}

		public virtual void ConfirmCompleteDonations()
		{
			foreach (OrderObjective objective in objectives)
			{
				if (objective is DonateObjective)
				{
					DonateObjective donateObjective = objective as DonateObjective;
					donateObjective.Confirm();
				}
			}
		}

		public virtual void UpdateDonationCounts()
		{
			_highlightLookup = null;
			int num = 0;
			int num2 = 0;
			foreach (OrderObjective objective in objectives)
			{
				if (!(objective is DonateObjective))
				{
					continue;
				}
				DonateObjective donateObjective = objective as DonateObjective;
				int num3 = 0;
				if (donateObjective.GetCount() >= donateObjective.GetMaxCount())
				{
					num++;
				}
				foreach (Item donatedItem in donatedItems)
				{
					if (donateObjective.IsValidItem(donatedItem))
					{
						num3 += donatedItem.Stack;
					}
				}
				donateObjective.SetCount(num3);
				if (donateObjective.GetCount() >= donateObjective.GetMaxCount())
				{
					num2++;
				}
			}
			if (num2 > num)
			{
				Game1.playSound("newArtifact");
			}
		}

		public bool HighlightAcceptableItems(Item item)
		{
			if (_highlightLookup != null && _highlightLookup.ContainsKey(item))
			{
				return _highlightLookup[item];
			}
			if (_highlightLookup == null)
			{
				_highlightLookup = new Dictionary<Item, bool>();
			}
			foreach (OrderObjective objective in objectives)
			{
				if (objective is DonateObjective)
				{
					DonateObjective donateObjective = objective as DonateObjective;
					if (donateObjective.GetAcceptCount(item, 1) > 0)
					{
						_highlightLookup[item] = true;
						return true;
					}
				}
			}
			_highlightLookup[item] = false;
			return false;
		}

		public virtual int GetAcceptCount(Item item)
		{
			int num = 0;
			int num2 = item.Stack;
			foreach (OrderObjective objective in objectives)
			{
				if (objective is DonateObjective)
				{
					DonateObjective donateObjective = objective as DonateObjective;
					int acceptCount = donateObjective.GetAcceptCount(item, num2);
					num2 -= acceptCount;
					num += acceptCount;
				}
			}
			return num;
		}

		public static bool CheckTags(string tag_list)
		{
			if (tag_list == null)
			{
				return true;
			}
			List<string> list = new List<string>();
			string[] array = tag_list.Split(',');
			foreach (string text in array)
			{
				list.Add(text.Trim());
			}
			foreach (string item in list)
			{
				string text2 = item;
				if (text2.Length != 0)
				{
					bool flag = true;
					if (text2[0] == '!')
					{
						flag = false;
						text2 = text2.Substring(1);
					}
					if (CheckTag(text2) != flag)
					{
						return false;
					}
				}
			}
			return true;
		}

		protected static bool CheckTag(string tag)
		{
			if (tag == "NOT_IMPLEMENTED")
			{
				return false;
			}
			if (tag.StartsWith("dropbox_"))
			{
				string box_id = tag.Substring("dropbox_".Length);
				foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
				{
					if (specialOrder.UsesDropBox(box_id))
					{
						return true;
					}
				}
			}
			if (tag.StartsWith("rule_"))
			{
				string special_rule = tag.Substring("rule_".Length);
				if (Game1.player.team.SpecialOrderRuleActive(special_rule))
				{
					return true;
				}
			}
			if (tag.StartsWith("completed_"))
			{
				string key = tag.Substring("season_".Length);
				if (Game1.player.team.completedSpecialOrders.ContainsKey(key))
				{
					return true;
				}
			}
			if (tag.StartsWith("season_"))
			{
				string text = tag.Substring("season_".Length);
				if (Game1.currentSeason == text)
				{
					return true;
				}
			}
			else if (tag.StartsWith("mail_"))
			{
				string id = tag.Substring("mail_".Length);
				if (Game1.MasterPlayer.hasOrWillReceiveMail(id))
				{
					return true;
				}
			}
			else if (tag.StartsWith("event_"))
			{
				int item = Convert.ToInt32(tag.Substring("event_".Length));
				if (Game1.MasterPlayer.eventsSeen.Contains(item))
				{
					return true;
				}
			}
			else
			{
				if (tag == "island")
				{
					if (Utility.doesAnyFarmerHaveOrWillReceiveMail("seenBoatJourney"))
					{
						return true;
					}
					return false;
				}
				if (tag.StartsWith("knows_"))
				{
					string key2 = tag.Substring("knows_".Length);
					foreach (Farmer allFarmer in Game1.getAllFarmers())
					{
						if (allFarmer.friendshipData.ContainsKey(key2))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public bool IsIslandOrder()
		{
			if (_isIslandOrder == -1)
			{
				Dictionary<string, SpecialOrderData> dictionary = Game1.content.Load<Dictionary<string, SpecialOrderData>>("Data\\SpecialOrders");
				if (dictionary.ContainsKey(questKey.Value))
				{
					if (dictionary[questKey.Value].RequiredTags.Contains("island"))
					{
						_isIslandOrder = 1;
					}
					else
					{
						_isIslandOrder = 0;
					}
				}
			}
			return _isIslandOrder == 1;
		}

		public static bool IsSpecialOrdersBoardUnlocked()
		{
			return Game1.stats.DaysPlayed >= 58;
		}

		public static void UpdateAvailableSpecialOrders(bool force_refresh)
		{
			if (Game1.player.team.availableSpecialOrders != null)
			{
				foreach (SpecialOrder availableSpecialOrder in Game1.player.team.availableSpecialOrders)
				{
					if ((availableSpecialOrder.questDuration.Value == QuestDuration.TwoDays || availableSpecialOrder.questDuration.Value == QuestDuration.ThreeDays) && !Game1.player.team.acceptedSpecialOrderTypes.Contains(availableSpecialOrder.orderType.Value))
					{
						availableSpecialOrder.SetDuration(availableSpecialOrder.questDuration);
					}
				}
			}
			if (Game1.player.team.availableSpecialOrders.Count > 0 && !force_refresh)
			{
				return;
			}
			Game1.player.team.availableSpecialOrders.Clear();
			Game1.player.team.acceptedSpecialOrderTypes.Clear();
			Dictionary<string, SpecialOrderData> dictionary = Game1.content.Load<Dictionary<string, SpecialOrderData>>("Data\\SpecialOrders");
			List<string> list = new List<string>(dictionary.Keys);
			for (int i = 0; i < list.Count; i++)
			{
				string text = list[i];
				bool flag = false;
				if (!flag && dictionary[text].Repeatable != "True" && Game1.MasterPlayer.team.completedSpecialOrders.ContainsKey(text))
				{
					flag = true;
				}
				if (Game1.dayOfMonth >= 16 && dictionary[text].Duration == "Month")
				{
					flag = true;
				}
				if (!flag && !CheckTags(dictionary[text].RequiredTags))
				{
					flag = true;
				}
				if (!flag)
				{
					foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
					{
						if (specialOrder.questKey == text)
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					list.RemoveAt(i);
					i--;
				}
			}
			Random random = new Random((int)Game1.uniqueIDForThisGame + (int)((float)Game1.stats.DaysPlayed * 1.3f));
			Game1.player.team.availableSpecialOrders.Clear();
			string[] array = new string[2] { "", "Qi" };
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				List<string> list2 = new List<string>();
				foreach (string item in list)
				{
					if (dictionary[item].OrderType == text2)
					{
						list2.Add(item);
					}
				}
				List<string> list3 = new List<string>(list2);
				if (text2 != "Qi")
				{
					for (int k = 0; k < list2.Count; k++)
					{
						if (Game1.player.team.completedSpecialOrders.ContainsKey(list2[k]))
						{
							list2.RemoveAt(k);
							k--;
						}
					}
				}
				for (int l = 0; l < 2; l++)
				{
					if (list2.Count == 0)
					{
						if (list3.Count == 0)
						{
							break;
						}
						list2 = new List<string>(list3);
					}
					int index = random.Next(list2.Count);
					string text3 = list2[index];
					Game1.player.team.availableSpecialOrders.Add(GetSpecialOrder(text3, random.Next()));
					list2.Remove(text3);
					list3.Remove(text3);
				}
			}
		}

		public static SpecialOrder GetSpecialOrder(string key, int? generation_seed)
		{
			Dictionary<string, SpecialOrderData> dictionary = Game1.content.Load<Dictionary<string, SpecialOrderData>>("Data\\SpecialOrders");
			if (!generation_seed.HasValue)
			{
				generation_seed = Game1.random.Next();
			}
			if (dictionary.ContainsKey(key))
			{
				Random random = new Random(generation_seed.Value);
				SpecialOrderData specialOrderData = dictionary[key];
				SpecialOrder specialOrder = new SpecialOrder();
				specialOrder.generationSeed.Value = generation_seed.Value;
				specialOrder._orderData = specialOrderData;
				specialOrder.questKey.Value = key;
				specialOrder.questName.Value = specialOrderData.Name;
				specialOrder.requester.Value = specialOrderData.Requester;
				specialOrder.orderType.Value = specialOrderData.OrderType.Trim();
				specialOrder.specialRule.Value = specialOrderData.SpecialRule.Trim();
				if (specialOrderData.ItemToRemoveOnEnd != null)
				{
					int result = -1;
					if (int.TryParse(specialOrderData.ItemToRemoveOnEnd, out result))
					{
						specialOrder.itemToRemoveOnEnd.Value = result;
					}
				}
				if (specialOrderData.MailToRemoveOnEnd != null)
				{
					specialOrder.mailToRemoveOnEnd.Value = specialOrderData.MailToRemoveOnEnd;
				}
				specialOrder.selectedRandomElements.Clear();
				if (specialOrderData.RandomizedElements != null)
				{
					foreach (RandomizedElement randomizedElement in specialOrderData.RandomizedElements)
					{
						List<int> list = new List<int>();
						for (int i = 0; i < randomizedElement.Values.Count; i++)
						{
							if (CheckTags(randomizedElement.Values[i].RequiredTags))
							{
								list.Add(i);
							}
						}
						int random2 = Utility.GetRandom(list, random);
						specialOrder.selectedRandomElements[randomizedElement.Name] = random2;
						string value = randomizedElement.Values[random2].Value;
						if (!value.StartsWith("PICK_ITEM"))
						{
							continue;
						}
						value = value.Substring("PICK_ITEM".Length);
						string[] array = value.Split(',');
						List<int> list2 = new List<int>();
						string[] array2 = array;
						foreach (string text in array2)
						{
							string text2 = text.Trim();
							if (text2.Length == 0)
							{
								continue;
							}
							if (char.IsDigit(text2[0]))
							{
								int result2 = -1;
								if (int.TryParse(text2, out result2))
								{
									list2.Add(result2);
								}
							}
							else
							{
								Item item = Utility.fuzzyItemSearch(text2);
								if (Utility.IsNormalObjectAtParentSheetIndex(item, item.ParentSheetIndex))
								{
									list2.Add(item.ParentSheetIndex);
								}
							}
						}
						specialOrder.preSelectedItems[randomizedElement.Name] = Utility.GetRandom(list2, random);
					}
				}
				if (specialOrderData.Duration == "Month")
				{
					specialOrder.SetDuration(QuestDuration.Month);
				}
				else if (specialOrderData.Duration == "TwoWeeks")
				{
					specialOrder.SetDuration(QuestDuration.TwoWeeks);
				}
				else if (specialOrderData.Duration == "TwoDays")
				{
					specialOrder.SetDuration(QuestDuration.TwoDays);
				}
				else if (specialOrderData.Duration == "ThreeDays")
				{
					specialOrder.SetDuration(QuestDuration.ThreeDays);
				}
				else
				{
					specialOrder.SetDuration(QuestDuration.Week);
				}
				specialOrder.questDescription.Value = specialOrderData.Text;
				foreach (SpecialOrderObjectiveData objective in specialOrderData.Objectives)
				{
					OrderObjective orderObjective = null;
					Type type = Type.GetType("StardewValley." + objective.Type.Trim() + "Objective");
					if (!(type == null) && type.IsSubclassOf(typeof(OrderObjective)))
					{
						orderObjective = (OrderObjective)Activator.CreateInstance(type);
						if (orderObjective != null)
						{
							orderObjective.description.Value = specialOrder.Parse(objective.Text);
							orderObjective.maxCount.Value = int.Parse(specialOrder.Parse(objective.RequiredCount));
							orderObjective.Load(specialOrder, objective.Data);
							specialOrder.objectives.Add(orderObjective);
						}
					}
				}
				{
					foreach (SpecialOrderRewardData reward in specialOrderData.Rewards)
					{
						OrderReward orderReward = null;
						Type type2 = Type.GetType("StardewValley." + reward.Type.Trim() + "Reward");
						if (!(type2 == null) && type2.IsSubclassOf(typeof(OrderReward)))
						{
							orderReward = (OrderReward)Activator.CreateInstance(type2);
							if (orderReward != null)
							{
								orderReward.Load(specialOrder, reward.Data);
								specialOrder.rewards.Add(orderReward);
							}
						}
					}
					return specialOrder;
				}
			}
			return null;
		}

		public virtual string MakeLocalizationReplacements(string data)
		{
			data = data.Trim();
			int num = 0;
			do
			{
				num = data.LastIndexOf('[');
				if (num >= 0)
				{
					int num2 = data.IndexOf(']', num);
					if (num2 == -1)
					{
						return data;
					}
					string text = data.Substring(num + 1, num2 - num - 1);
					string value = Game1.content.LoadString("Strings\\SpecialOrderStrings:" + text);
					data = data.Remove(num, num2 - num + 1);
					data = data.Insert(num, value);
				}
			}
			while (num >= 0);
			return data;
		}

		public virtual string Parse(string data)
		{
			data = data.Trim();
			GetData();
			data = MakeLocalizationReplacements(data);
			int num = 0;
			do
			{
				num = data.LastIndexOf('{');
				if (num < 0)
				{
					continue;
				}
				int num2 = data.IndexOf('}', num);
				if (num2 == -1)
				{
					return data;
				}
				string text = data.Substring(num + 1, num2 - num - 1);
				string text2 = text;
				string text3 = text;
				string text4 = null;
				if (text.Contains(":"))
				{
					string[] array = text.Split(':');
					text3 = array[0];
					if (array.Length > 1)
					{
						text4 = array[1];
					}
				}
				if (_orderData.RandomizedElements != null)
				{
					if (preSelectedItems.ContainsKey(text3))
					{
						Object @object = new Object(Vector2.Zero, preSelectedItems[text3], 0);
						switch (text4)
						{
						case "Text":
							text2 = @object.DisplayName;
							break;
						case "TextPlural":
							text2 = Lexicon.makePlural(@object.DisplayName);
							break;
						case "TextPluralCapitalized":
							text2 = Utility.capitalizeFirstLetter(Lexicon.makePlural(@object.DisplayName));
							break;
						case "Tags":
						{
							string text5 = "id_" + Utility.getStandardDescriptionFromItem(@object, 0, '_');
							text5 = text5.Substring(0, text5.Length - 2).ToLower();
							text2 = text5;
							break;
						}
						case "Price":
							text2 = @object.sellToStorePrice(-1L).ToString() ?? "";
							break;
						}
					}
					else if (selectedRandomElements.ContainsKey(text3))
					{
						foreach (RandomizedElement randomizedElement in _orderData.RandomizedElements)
						{
							if (randomizedElement.Name == text3)
							{
								text2 = MakeLocalizationReplacements(randomizedElement.Values[selectedRandomElements[text3]].Value);
								break;
							}
						}
					}
				}
				if (text4 != null)
				{
					string[] array2 = text2.Split('|');
					for (int i = 0; i < array2.Length; i += 2)
					{
						if (i + 1 <= array2.Length && array2[i] == text4)
						{
							text2 = array2[i + 1];
							break;
						}
					}
				}
				data = data.Remove(num, num2 - num + 1);
				data = data.Insert(num, text2);
			}
			while (num >= 0);
			return data;
		}

		public virtual SpecialOrderData GetData()
		{
			if (_orderData == null)
			{
				Dictionary<string, SpecialOrderData> dictionary = Game1.content.Load<Dictionary<string, SpecialOrderData>>("Data\\SpecialOrders");
				if (dictionary.ContainsKey(questKey.Value))
				{
					_orderData = dictionary[questKey.Value];
				}
			}
			return _orderData;
		}

		public virtual void InitializeNetFields()
		{
			NetFields.AddFields(questName, questDescription, dueDate, objectives, rewards, questState, donatedItems, questKey, requester, generationSeed, selectedRandomElements, preSelectedItems, orderType, specialRule, participants, seenParticipants, unclaimedRewards, donateMutex.NetFields, itemToRemoveOnEnd, mailToRemoveOnEnd, questDuration, readyForRemoval);
		}

		protected virtual void _UpdateObjectiveRegistration()
		{
			for (int i = 0; i < _registeredObjectives.Count; i++)
			{
				OrderObjective orderObjective = _registeredObjectives[i];
				if (!objectives.Contains(orderObjective))
				{
					orderObjective.Unregister();
				}
			}
			foreach (OrderObjective objective in objectives)
			{
				if (!_registeredObjectives.Contains(objective))
				{
					objective.Register(this);
					_registeredObjectives.Add(objective);
				}
			}
		}

		public bool UsesDropBox(string box_id)
		{
			if (questState.Value != 0)
			{
				return false;
			}
			foreach (OrderObjective objective in objectives)
			{
				if (objective is DonateObjective && (objective as DonateObjective).dropBox.Value == box_id)
				{
					return true;
				}
			}
			return false;
		}

		public int GetMinimumDropBoxCapacity(string box_id)
		{
			int num = 9;
			foreach (OrderObjective objective in objectives)
			{
				if (objective is DonateObjective && (objective as DonateObjective).dropBox.Value == box_id && (objective as DonateObjective).minimumCapacity.Value > 0)
				{
					num = Math.Max(num, (objective as DonateObjective).minimumCapacity);
				}
			}
			return num;
		}

		public virtual void Update()
		{
			_AddSpecialRulesIfNecessary();
			_objectiveRegistrationDirty = true;
			if (_objectiveRegistrationDirty)
			{
				_objectiveRegistrationDirty = false;
				_UpdateObjectiveRegistration();
			}
			if (!readyForRemoval.Value)
			{
				if (questState.Value == QuestState.InProgress && !participants.ContainsKey(Game1.player.UniqueMultiplayerID))
				{
					participants[Game1.player.UniqueMultiplayerID] = true;
				}
				else if (questState.Value == QuestState.Complete)
				{
					if (unclaimedRewards.ContainsKey(Game1.player.UniqueMultiplayerID))
					{
						unclaimedRewards.Remove(Game1.player.UniqueMultiplayerID);
						Game1.stats.QuestsCompleted++;
						Game1.playSound("questcomplete");
						Game1.dayTimeMoneyBox.questsDirty = true;
						foreach (OrderReward reward in rewards)
						{
							reward.Grant();
						}
					}
					if (participants.ContainsKey(Game1.player.UniqueMultiplayerID) && GetMoneyReward() <= 0)
					{
						RemoveFromParticipants();
					}
				}
			}
			donateMutex.Update(Game1.getOnlineFarmers());
			if (donateMutex.IsLockHeld() && Game1.activeClickableMenu == null)
			{
				donateMutex.ReleaseLock();
			}
			if (Game1.activeClickableMenu == null)
			{
				_highlightLookup = null;
			}
			if (Game1.IsMasterGame && questState.Value != 0)
			{
				MarkForRemovalIfEmpty();
				if (readyForRemoval.Value)
				{
					_RemoveSpecialRuleIfNecessary();
					Game1.player.team.specialOrders.Remove(this);
				}
			}
		}

		public virtual void RemoveFromParticipants()
		{
			participants.Remove(Game1.player.UniqueMultiplayerID);
			MarkForRemovalIfEmpty();
		}

		public virtual void MarkForRemovalIfEmpty()
		{
			if (participants.Count() == 0)
			{
				readyForRemoval.Value = true;
			}
		}

		public virtual void HostHandleQuestEnd()
		{
			if (Game1.IsMasterGame)
			{
				if (itemToRemoveOnEnd.Value >= 0 && !Game1.player.team.itemsToRemoveOvernight.Contains(itemToRemoveOnEnd.Value))
				{
					Game1.player.team.itemsToRemoveOvernight.Add(itemToRemoveOnEnd.Value);
				}
				if (mailToRemoveOnEnd.Value != null && !Game1.player.team.mailToRemoveOvernight.Contains(mailToRemoveOnEnd.Value))
				{
					Game1.player.team.mailToRemoveOvernight.Add(mailToRemoveOnEnd.Value);
				}
			}
		}

		protected void _AddSpecialRulesIfNecessary()
		{
			if (!Game1.IsMasterGame || appliedSpecialRules || questState.Value != 0)
			{
				return;
			}
			appliedSpecialRules = true;
			string[] array = specialRule.Value.Split(',');
			string[] array2 = array;
			foreach (string text in array2)
			{
				string text2 = text.Trim();
				if (!Game1.player.team.SpecialOrderRuleActive(text2, this))
				{
					AddSpecialRule(text2);
					if (Game1.player.team.specialRulesRemovedToday.Contains(text2))
					{
						Game1.player.team.specialRulesRemovedToday.Remove(text2);
					}
				}
			}
		}

		protected void _RemoveSpecialRuleIfNecessary()
		{
			if (!Game1.IsMasterGame || !appliedSpecialRules)
			{
				return;
			}
			appliedSpecialRules = false;
			string[] array = specialRule.Value.Split(',');
			string[] array2 = array;
			foreach (string text in array2)
			{
				string text2 = text.Trim();
				if (!Game1.player.team.SpecialOrderRuleActive(text2, this))
				{
					RemoveSpecialRule(text2);
					if (!Game1.player.team.specialRulesRemovedToday.Contains(text2))
					{
						Game1.player.team.specialRulesRemovedToday.Add(text2);
					}
				}
			}
		}

		public virtual void AddSpecialRule(string rule)
		{
			if (rule == "MINE_HARD")
			{
				Game1.netWorldState.Value.MinesDifficulty++;
				Game1.player.team.kickOutOfMinesEvent.Fire();
				Game1.netWorldState.Value.LowestMineLevelForOrder = 0;
			}
			else if (rule == "SC_HARD")
			{
				Game1.netWorldState.Value.SkullCavesDifficulty++;
				Game1.player.team.kickOutOfMinesEvent.Fire();
			}
		}

		public static void RemoveSpecialRuleAtEndOfDay(string rule)
		{
			switch (rule)
			{
			case "MINE_HARD":
				if (Game1.netWorldState.Value.MinesDifficulty > 0)
				{
					Game1.netWorldState.Value.MinesDifficulty--;
				}
				Game1.netWorldState.Value.LowestMineLevelForOrder = -1;
				break;
			case "SC_HARD":
				if (Game1.netWorldState.Value.SkullCavesDifficulty > 0)
				{
					Game1.netWorldState.Value.SkullCavesDifficulty--;
				}
				break;
			case "QI_COOKING":
				Utility.iterateAllItems(delegate(Item item)
				{
					if (item is Object && (item as Object).orderData.Value == "QI_COOKING")
					{
						(item as Object).orderData.Value = null;
						item.MarkContextTagsDirty();
					}
				});
				break;
			}
		}

		public virtual void RemoveSpecialRule(string rule)
		{
			if (rule == "QI_BEANS")
			{
				Game1.player.team.itemsToRemoveOvernight.Add(890);
				Game1.player.team.itemsToRemoveOvernight.Add(889);
			}
		}

		public virtual bool HasMoneyReward()
		{
			if (questState.Value == QuestState.Complete && GetMoneyReward() > 0)
			{
				return participants.ContainsKey(Game1.player.UniqueMultiplayerID);
			}
			return false;
		}

		public virtual void Fail()
		{
		}

		public virtual void AddObjective(OrderObjective objective)
		{
			objectives.Add(objective);
		}

		public void CheckCompletion()
		{
			if (questState.Value != 0)
			{
				return;
			}
			foreach (OrderObjective objective in objectives)
			{
				if ((bool)objective.failOnCompletion && objective.IsComplete())
				{
					OnFail();
					return;
				}
			}
			foreach (OrderObjective objective2 in objectives)
			{
				if (!objective2.failOnCompletion && !objective2.IsComplete())
				{
					return;
				}
			}
			if (!Game1.IsMasterGame)
			{
				return;
			}
			foreach (long key in participants.Keys)
			{
				if (!unclaimedRewards.ContainsKey(key))
				{
					unclaimedRewards[key] = true;
				}
			}
			Game1.multiplayer.globalChatInfoMessage("CompletedSpecialOrder", GetName());
			HostHandleQuestEnd();
			Game1.player.team.completedSpecialOrders[questKey.Value] = true;
			questState.Value = QuestState.Complete;
			_RemoveSpecialRuleIfNecessary();
		}

		public override string ToString()
		{
			string text = "";
			foreach (OrderObjective objective in objectives)
			{
				text += objective.description;
				if (objective.GetMaxCount() > 1)
				{
					text = text + " (" + objective.GetCount() + "/" + objective.GetMaxCount() + ")";
				}
				text += "\n";
			}
			return text.Trim();
		}

		public string GetName()
		{
			if (_localizedName == null)
			{
				_localizedName = MakeLocalizationReplacements(questName.Value);
			}
			return _localizedName;
		}

		public string GetDescription()
		{
			if (_localizedDescription == null)
			{
				_localizedDescription = Parse(questDescription.Value).Trim();
			}
			return _localizedDescription;
		}

		public List<string> GetObjectiveDescriptions()
		{
			List<string> list = new List<string>();
			foreach (OrderObjective objective in objectives)
			{
				list.Add(Parse(objective.GetDescription()));
			}
			return list;
		}

		public bool CanBeCancelled()
		{
			return false;
		}

		public void MarkAsViewed()
		{
			if (!seenParticipants.ContainsKey(Game1.player.UniqueMultiplayerID))
			{
				seenParticipants[Game1.player.UniqueMultiplayerID] = true;
			}
		}

		public bool IsHidden()
		{
			return !participants.ContainsKey(Game1.player.UniqueMultiplayerID);
		}

		public bool ShouldDisplayAsNew()
		{
			return !seenParticipants.ContainsKey(Game1.player.UniqueMultiplayerID);
		}

		public bool HasReward()
		{
			return HasMoneyReward();
		}

		public int GetMoneyReward()
		{
			if (_moneyReward == -1)
			{
				_moneyReward = 0;
				foreach (OrderReward reward in rewards)
				{
					if (reward is MoneyReward)
					{
						_moneyReward += (reward as MoneyReward).GetRewardMoneyAmount();
					}
				}
			}
			return _moneyReward;
		}

		public bool ShouldDisplayAsComplete()
		{
			return questState.Value != QuestState.InProgress;
		}

		public bool IsTimedQuest()
		{
			return true;
		}

		public int GetDaysLeft()
		{
			if (questState.Value != 0)
			{
				return 0;
			}
			return (int)dueDate - Game1.Date.TotalDays;
		}

		public void OnMoneyRewardClaimed()
		{
			participants.Remove(Game1.player.UniqueMultiplayerID);
			MarkForRemovalIfEmpty();
		}

		public bool OnLeaveQuestPage()
		{
			if (!participants.ContainsKey(Game1.player.UniqueMultiplayerID))
			{
				MarkForRemovalIfEmpty();
				return true;
			}
			return false;
		}
	}
}
