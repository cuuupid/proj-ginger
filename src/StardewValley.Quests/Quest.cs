using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Quests
{
	[XmlInclude(typeof(SocializeQuest))]
	[XmlInclude(typeof(SlayMonsterQuest))]
	[XmlInclude(typeof(ResourceCollectionQuest))]
	[XmlInclude(typeof(ItemDeliveryQuest))]
	[XmlInclude(typeof(ItemHarvestQuest))]
	[XmlInclude(typeof(CraftingQuest))]
	[XmlInclude(typeof(FishingQuest))]
	[XmlInclude(typeof(GoSomewhereQuest))]
	[XmlInclude(typeof(LostItemQuest))]
	[XmlInclude(typeof(DescriptionElement))]
	[XmlInclude(typeof(SecretLostItemQuest))]
	public class Quest : INetObject<NetFields>, IQuest
	{
		public const int type_basic = 1;

		public const int type_crafting = 2;

		public const int type_itemDelivery = 3;

		public const int type_monster = 4;

		public const int type_socialize = 5;

		public const int type_location = 6;

		public const int type_fishing = 7;

		public const int type_building = 8;

		public const int type_harvest = 9;

		public const int type_resource = 10;

		public const int type_weeding = 11;

		public string _currentObjective = "";

		public string _questDescription = "";

		public string _questTitle = "";

		[XmlElement("rewardDescription")]
		public readonly NetString rewardDescription = new NetString();

		[XmlElement("completionString")]
		public readonly NetString completionString = new NetString();

		protected Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);

		[XmlElement("accepted")]
		public readonly NetBool accepted = new NetBool();

		[XmlElement("completed")]
		public readonly NetBool completed = new NetBool();

		[XmlElement("dailyQuest")]
		public readonly NetBool dailyQuest = new NetBool();

		[XmlElement("showNew")]
		public readonly NetBool showNew = new NetBool();

		[XmlElement("canBeCancelled")]
		public readonly NetBool canBeCancelled = new NetBool();

		[XmlElement("destroy")]
		public readonly NetBool destroy = new NetBool();

		[XmlElement("id")]
		public readonly NetInt id = new NetInt();

		[XmlElement("moneyReward")]
		public readonly NetInt moneyReward = new NetInt();

		[XmlElement("questType")]
		public readonly NetInt questType = new NetInt();

		[XmlElement("daysLeft")]
		public readonly NetInt daysLeft = new NetInt();

		[XmlElement("dayQuestAccepted")]
		public readonly NetInt dayQuestAccepted = new NetInt(-1);

		public readonly NetIntList nextQuests = new NetIntList();

		private bool _loadedDescription;

		private bool _loadedTitle;

		public NetFields NetFields { get; } = new NetFields();


		public string questTitle
		{
			get
			{
				if (!_loadedTitle)
				{
					switch (questType.Value)
					{
					case 3:
						_questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13285");
						break;
					case 4:
						_questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13696");
						break;
					case 5:
						_questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SocializeQuest.cs.13785");
						break;
					case 7:
						_questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingQuest.cs.13227");
						break;
					case 10:
						_questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13640");
						break;
					}
					Dictionary<int, string> dictionary = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Quests");
					if (dictionary != null && dictionary.ContainsKey(id))
					{
						string[] array = dictionary[id].Split('/');
						_questTitle = array[1];
					}
					_loadedTitle = true;
				}
				if (_questTitle == null)
				{
					_questTitle = "";
				}
				return _questTitle;
			}
			set
			{
				_questTitle = value;
			}
		}

		[XmlIgnore]
		public string questDescription
		{
			get
			{
				if (!_loadedDescription)
				{
					reloadDescription();
					Dictionary<int, string> dictionary = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Quests");
					if (dictionary != null && dictionary.ContainsKey(id))
					{
						string[] array = dictionary[id].Split('/');
						_questDescription = array[2];
					}
					_loadedDescription = true;
				}
				if (_questDescription == null)
				{
					_questDescription = "";
				}
				return _questDescription;
			}
			set
			{
				_questDescription = value;
			}
		}

		[XmlIgnore]
		public string currentObjective
		{
			get
			{
				Dictionary<int, string> dictionary = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Quests");
				if (dictionary != null && dictionary.ContainsKey(id))
				{
					string[] array = dictionary[id].Split('/');
					if (array[3].Length > 1)
					{
						_currentObjective = array[3];
					}
				}
				reloadObjective();
				if (_currentObjective == null)
				{
					_currentObjective = "";
				}
				return _currentObjective;
			}
			set
			{
				_currentObjective = value;
			}
		}

		public Quest()
		{
			initNetFields();
		}

		protected virtual void initNetFields()
		{
			NetFields.AddFields(rewardDescription, completionString, accepted, completed, dailyQuest, showNew, canBeCancelled, destroy, id, moneyReward, questType, daysLeft, nextQuests, dayQuestAccepted);
		}

		public static Quest getQuestFromId(int id)
		{
			Dictionary<int, string> dictionary = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Quests");
			if (dictionary != null && dictionary.ContainsKey(id))
			{
				string[] array = dictionary[id].Split('/');
				string text = array[0];
				Quest quest = null;
				string[] array2 = array[4].Split(' ');
				if (text != null)
				{
					switch (text.Length)
					{
					case 8:
						switch (text[2])
						{
						case 'a':
							if (text == "Crafting")
							{
								quest = new CraftingQuest(Convert.ToInt32(array2[0]), array2[1].ToLower().Equals("true"));
								quest.questType.Value = 2;
							}
							break;
						case 'c':
							if (text == "Location")
							{
								quest = new GoSomewhereQuest(array2[0]);
								quest.questType.Value = 6;
							}
							break;
						case 'i':
							if (text == "Building")
							{
								quest = new Quest();
								quest.questType.Value = 8;
								quest.completionString.Value = array2[0];
							}
							break;
						case 's':
							if (text == "LostItem")
							{
								quest = new LostItemQuest(array2[0], array2[2], Convert.ToInt32(array2[1]), Convert.ToInt32(array2[3]), Convert.ToInt32(array2[4]));
							}
							break;
						}
						break;
					case 12:
						if (text == "ItemDelivery")
						{
							quest = new ItemDeliveryQuest();
							(quest as ItemDeliveryQuest).target.Value = array2[0];
							(quest as ItemDeliveryQuest).item.Value = Convert.ToInt32(array2[1]);
							(quest as ItemDeliveryQuest).targetMessage = array[9];
							if (array2.Length > 2)
							{
								(quest as ItemDeliveryQuest).number.Value = Convert.ToInt32(array2[2]);
							}
							quest.questType.Value = 3;
						}
						break;
					case 7:
						if (text == "Monster")
						{
							quest = new SlayMonsterQuest();
							(quest as SlayMonsterQuest).loadQuestInfo();
							(quest as SlayMonsterQuest).monster.Value.Name = array2[0].Replace('_', ' ');
							(quest as SlayMonsterQuest).monsterName.Value = (quest as SlayMonsterQuest).monster.Value.Name;
							(quest as SlayMonsterQuest).numberToKill.Value = Convert.ToInt32(array2[1]);
							if (array2.Length > 2)
							{
								(quest as SlayMonsterQuest).target.Value = array2[2];
							}
							else
							{
								(quest as SlayMonsterQuest).target.Value = "null";
							}
							quest.questType.Value = 4;
						}
						break;
					case 5:
						if (text == "Basic")
						{
							quest = new Quest();
							quest.questType.Value = 1;
						}
						break;
					case 6:
						if (text == "Social")
						{
							quest = new SocializeQuest();
							(quest as SocializeQuest).loadQuestInfo();
						}
						break;
					case 11:
						if (text == "ItemHarvest")
						{
							quest = new ItemHarvestQuest(Convert.ToInt32(array2[0]), (array2.Length <= 1) ? 1 : Convert.ToInt32(array2[1]));
						}
						break;
					case 14:
						if (text == "SecretLostItem")
						{
							quest = new SecretLostItemQuest(array2[0], Convert.ToInt32(array2[1]), Convert.ToInt32(array2[2]), Convert.ToInt32(array2[3]));
						}
						break;
					}
				}
				quest.id.Value = id;
				quest.questTitle = array[1];
				quest.questDescription = array[2];
				if (array[3].Length > 1)
				{
					quest.currentObjective = array[3];
				}
				string[] array3 = array[5].Split(' ');
				for (int i = 0; i < array3.Length; i++)
				{
					string text2 = array3[i];
					if (text2.StartsWith("h"))
					{
						if (!Game1.IsMasterGame)
						{
							continue;
						}
						text2 = text2.Substring(1);
					}
					quest.nextQuests.Add(Convert.ToInt32(text2));
				}
				quest.showNew.Value = true;
				quest.moneyReward.Value = Convert.ToInt32(array[6]);
				quest.rewardDescription.Value = (array[6].Equals("-1") ? null : array[7]);
				if (array.Length > 8)
				{
					quest.canBeCancelled.Value = array[8].Equals("true");
				}
				return quest;
			}
			return null;
		}

		public virtual void reloadObjective()
		{
		}

		public virtual void reloadDescription()
		{
		}

		public virtual void adjustGameLocation(GameLocation location)
		{
		}

		public virtual void accept()
		{
			accepted.Value = true;
		}

		public virtual bool checkIfComplete(NPC n = null, int number1 = -1, int number2 = -2, Item item = null, string str = null)
		{
			if (completionString.Value != null && str != null && str.Equals(completionString.Value))
			{
				questComplete();
				return true;
			}
			return false;
		}

		public bool hasReward()
		{
			if ((int)moneyReward <= 0)
			{
				if (rewardDescription.Value != null)
				{
					return rewardDescription.Value.Length > 2;
				}
				return false;
			}
			return true;
		}

		public virtual bool isSecretQuest()
		{
			return false;
		}

		public virtual void questComplete()
		{
			if ((bool)completed)
			{
				return;
			}
			if ((bool)dailyQuest || (int)questType == 7)
			{
				Game1.stats.QuestsCompleted++;
			}
			completed.Value = true;
			if (nextQuests.Count > 0)
			{
				int num = -1;
				foreach (int nextQuest in nextQuests)
				{
					if (nextQuest > 0)
					{
						Game1.player.questLog.Add(getQuestFromId(nextQuest));
						if (num == -1)
						{
							num = nextQuest;
						}
					}
				}
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Quest.cs.13636"), 2, num));
			}
			if ((int)moneyReward <= 0 && (rewardDescription.Value == null || rewardDescription.Value.Length <= 2))
			{
				Game1.player.questLog.Remove(this);
			}
			else
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Quest.cs.13636"), 2));
			}
			Game1.playSound("questcomplete");
			if (id.Value == 126)
			{
				Game1.player.mailReceived.Add("emilyFiber");
				Game1.player.activeDialogueEvents.Add("emilyFiber", 2);
			}
			Game1.dayTimeMoneyBox.questsDirty = true;
		}

		public string GetName()
		{
			return questTitle;
		}

		public string GetDescription()
		{
			return questDescription;
		}

		public bool IsHidden()
		{
			return isSecretQuest();
		}

		public List<string> GetObjectiveDescriptions()
		{
			List<string> list = new List<string>();
			list.Add(currentObjective);
			return list;
		}

		public bool CanBeCancelled()
		{
			return canBeCancelled.Value;
		}

		public bool HasReward()
		{
			if (!HasMoneyReward())
			{
				if (rewardDescription.Value != null)
				{
					return rewardDescription.Value.Length > 2;
				}
				return false;
			}
			return true;
		}

		public bool HasMoneyReward()
		{
			if (completed.Value)
			{
				return moneyReward.Value > 0;
			}
			return false;
		}

		public void MarkAsViewed()
		{
			showNew.Value = false;
		}

		public bool ShouldDisplayAsNew()
		{
			return showNew.Value;
		}

		public bool ShouldDisplayAsComplete()
		{
			if (completed.Value)
			{
				return !IsHidden();
			}
			return false;
		}

		public bool IsTimedQuest()
		{
			return dailyQuest.Value;
		}

		public int GetDaysLeft()
		{
			return daysLeft;
		}

		public int GetMoneyReward()
		{
			return moneyReward.Value;
		}

		public void OnMoneyRewardClaimed()
		{
			moneyReward.Value = 0;
			destroy.Value = true;
		}

		public bool OnLeaveQuestPage()
		{
			if ((bool)completed && (int)moneyReward <= 0)
			{
				destroy.Value = true;
			}
			if (destroy.Value)
			{
				Game1.player.questLog.Remove(this);
				return true;
			}
			return false;
		}
	}
}
