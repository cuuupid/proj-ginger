using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Quests
{
	public class ItemDeliveryQuest : Quest
	{
		public string targetMessage;

		[XmlElement("target")]
		public readonly NetString target = new NetString();

		[XmlElement("item")]
		public readonly NetInt item = new NetInt();

		[XmlElement("number")]
		public readonly NetInt number = new NetInt(1);

		[XmlElement("deliveryItem")]
		public readonly NetRef<Object> deliveryItem = new NetRef<Object>();

		public readonly NetDescriptionElementList parts = new NetDescriptionElementList();

		public readonly NetDescriptionElementList dialogueparts = new NetDescriptionElementList();

		[XmlElement("objective")]
		public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

		[XmlIgnore]
		[Obsolete]
		public NPC actualTarget;

		public ItemDeliveryQuest()
		{
			questType.Value = 3;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(target, item, number, deliveryItem, parts, dialogueparts, objective);
		}

		public List<NPC> GetValidTargetList()
		{
			List<NPC> list = new List<NPC>();
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer == null)
				{
					continue;
				}
				foreach (string key in allFarmer.friendshipData.Keys)
				{
					NPC characterFromName = Game1.getCharacterFromName(key);
					if (characterFromName != null && !list.Contains(characterFromName))
					{
						list.Add(characterFromName);
					}
				}
			}
			list.OrderBy((NPC n) => n.Name);
			for (int i = 0; i < list.Count; i++)
			{
				NPC nPC = list[i];
				if (nPC.IsInvisible)
				{
					list.RemoveAt(i);
					i--;
				}
				else if (nPC.Age == 2)
				{
					list.RemoveAt(i);
					i--;
				}
				else if (!nPC.isVillager())
				{
					list.RemoveAt(i);
					i--;
				}
				else if (nPC.Name.Equals("Krobus") || nPC.Name.Equals("Qi") || nPC.Name.Equals("Dwarf") || nPC.Name.Equals("Gunther") || nPC.Name.Equals("Bouncer") || nPC.Name.Equals("Henchman") || nPC.Name.Equals("Marlon") || nPC.Name.Equals("Mariner"))
				{
					list.RemoveAt(i);
					i--;
				}
			}
			foreach (Farmer allFarmer2 in Game1.getAllFarmers())
			{
				for (int j = 0; j < list.Count; j++)
				{
					NPC nPC2 = list[j];
					if (nPC2.Name.Equals(allFarmer2.spouse))
					{
						list.RemoveAt(j);
						j--;
					}
				}
			}
			for (int k = 0; k < list.Count; k++)
			{
				NPC nPC3 = list[k];
				if (!nPC3.Name.Equals("Sandy"))
				{
					continue;
				}
				bool flag = false;
				foreach (Farmer allFarmer3 in Game1.getAllFarmers())
				{
					if (allFarmer3.eventsSeen.Contains(67))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.RemoveAt(k);
					k--;
				}
				break;
			}
			return list;
		}

		public void loadQuestInfo()
		{
			if (target.Value != null)
			{
				return;
			}
			base.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13285");
			NPC nPC = null;
			List<NPC> validTargetList = GetValidTargetList();
			if (Game1.player.friendshipData == null || Game1.player.friendshipData.Count() <= 0 || validTargetList.Count <= 0)
			{
				return;
			}
			nPC = validTargetList[random.Next(validTargetList.Count)];
			if (nPC == null)
			{
				return;
			}
			target.Value = nPC.name;
			if (target.Value.Equals("Wizard") && !Game1.player.mailReceived.Contains("wizardJunimoNote") && !Game1.player.mailReceived.Contains("JojaMember"))
			{
				target.Value = "Demetrius";
				nPC = Game1.getCharacterFromName(target.Value);
			}
			if (!Game1.currentSeason.Equals("winter") && random.NextDouble() < 0.15)
			{
				List<int> list = Utility.possibleCropsAtThisTime(Game1.currentSeason, Game1.dayOfMonth <= 7);
				item.Value = list.ElementAt(random.Next(list.Count));
				deliveryItem.Value = new Object(Vector2.Zero, item.Value, 1);
				parts.Clear();
				parts.Add((random.NextDouble() < 0.3) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13299" : ((random.NextDouble() < 0.5) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13300" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13301"));
				parts.Add((random.NextDouble() < 0.3) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13302", deliveryItem.Value) : ((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13303", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13304", deliveryItem.Value)));
				parts.Add((random.NextDouble() < 0.25) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13306" : ((random.NextDouble() < 0.33) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13307" : ((random.NextDouble() < 0.5) ? "" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13308")));
				parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", nPC));
				if (target.Value.Equals("Demetrius"))
				{
					parts.Clear();
					parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13311", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13314", deliveryItem.Value));
				}
				if (target.Value.Equals("Marnie"))
				{
					parts.Clear();
					parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13317", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13320", deliveryItem.Value));
				}
				if (target.Value.Equals("Sebastian"))
				{
					parts.Clear();
					parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13324", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13327", deliveryItem.Value));
				}
			}
			else
			{
				item.Value = Utility.getRandomItemFromSeason(Game1.currentSeason, 1000, forQuest: true);
				if ((int)item == -5)
				{
					item.Value = 176;
				}
				if ((int)item == -6)
				{
					item.Value = 184;
				}
				deliveryItem.Value = new Object(Vector2.Zero, item, 1);
				DescriptionElement[] array = null;
				DescriptionElement[] array2 = null;
				DescriptionElement[] array3 = null;
				if (Game1.objectInformation[item].Split('/')[3].Split(' ')[0].Equals("Cooking") && !target.Value.Equals("Wizard"))
				{
					if (random.NextDouble() < 0.33)
					{
						DescriptionElement[] source = new DescriptionElement[12]
						{
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13336",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13337",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13338",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13339",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13340",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13341",
							(!Game1.samBandName.Equals(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156"))) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13347", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2156")) : ((!Game1.elliottBookName.Equals(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2157"))) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13342", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2157")) : ((DescriptionElement)"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13346")),
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13349",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13350",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13351",
							Game1.currentSeason.Equals("winter") ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13353" : (Game1.currentSeason.Equals("summer") ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13355" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13356"),
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13357"
						};
						parts.Clear();
						parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13333", deliveryItem.Value, source.ElementAt(random.Next(12))) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13334", deliveryItem.Value, source.ElementAt(random.Next(12))));
						parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", nPC));
					}
					else
					{
						DescriptionElement param = new DescriptionElement();
						switch (Game1.dayOfMonth % 7)
						{
						case 0:
							param = "Strings\\StringsFromCSFiles:Game1.cs.3042";
							break;
						case 1:
							param = "Strings\\StringsFromCSFiles:Game1.cs.3043";
							break;
						case 2:
							param = "Strings\\StringsFromCSFiles:Game1.cs.3044";
							break;
						case 3:
							param = "Strings\\StringsFromCSFiles:Game1.cs.3045";
							break;
						case 4:
							param = "Strings\\StringsFromCSFiles:Game1.cs.3046";
							break;
						case 5:
							param = "Strings\\StringsFromCSFiles:Game1.cs.3047";
							break;
						case 6:
							param = "Strings\\StringsFromCSFiles:Game1.cs.3048";
							break;
						}
						array = new DescriptionElement[5]
						{
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13360", deliveryItem.Value),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13364", deliveryItem.Value),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13367", deliveryItem.Value),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13370", deliveryItem.Value),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13373", param, deliveryItem.Value, nPC)
						};
						array2 = new DescriptionElement[5]
						{
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", nPC),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", nPC),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", nPC),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", nPC),
							""
						};
						array3 = new DescriptionElement[5] { "", "", "", "", "" };
					}
					parts.Clear();
					int num = random.Next(array.Count());
					parts.Add(array[num]);
					parts.Add(array2[num]);
					parts.Add(array3[num]);
					if (target.Value.Equals("Sebastian"))
					{
						parts.Clear();
						parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13378", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13381", deliveryItem.Value));
					}
				}
				else if (random.NextDouble() < 0.5 && Convert.ToInt32(Game1.objectInformation[item].Split('/')[2]) > 0)
				{
					array = new DescriptionElement[2]
					{
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13383", deliveryItem.Value, new DescriptionElement[12]
						{
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13385", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13386", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13387", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13388", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13389", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13390", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13391", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13392", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13393", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13394",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13395", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13396"
						}.ElementAt(random.Next(12))),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13400", deliveryItem.Value)
					};
					array2 = new DescriptionElement[2]
					{
						new DescriptionElement((random.NextDouble() < 0.5) ? "" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13398"),
						new DescriptionElement((random.NextDouble() < 0.5) ? "" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13402")
					};
					array3 = new DescriptionElement[2]
					{
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", nPC),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", nPC)
					};
					if (random.NextDouble() < 0.33)
					{
						DescriptionElement[] source2 = new DescriptionElement[12]
						{
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13336",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13337",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13338",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13339",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13340",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13341",
							(!Game1.samBandName.Equals(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156"))) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13347", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2156")) : ((!Game1.elliottBookName.Equals(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2157"))) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13342", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2157")) : ((DescriptionElement)"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13346")),
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13420",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13421",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13422",
							Game1.currentSeason.Equals("winter") ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13424" : (Game1.currentSeason.Equals("summer") ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13426" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13427"),
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13357"
						};
						parts.Clear();
						parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13333", deliveryItem.Value, source2.ElementAt(random.Next(12))) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13334", deliveryItem.Value, source2.ElementAt(random.Next(12))));
						parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", nPC));
					}
					else
					{
						parts.Clear();
						int num2 = random.Next(array.Count());
						parts.Add(array[num2]);
						parts.Add(array2[num2]);
						parts.Add(array3[num2]);
					}
					if (target.Value.Equals("Demetrius"))
					{
						parts.Clear();
						parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13311", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13314", deliveryItem.Value));
					}
					if (target.Value.Equals("Marnie"))
					{
						parts.Clear();
						parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13317", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13320", deliveryItem.Value));
					}
					if (target.Value.Equals("Harvey"))
					{
						DescriptionElement[] source3 = new DescriptionElement[12]
						{
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13448", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13449", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13450", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13451", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13452", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13453", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13454", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13455", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13456", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13457",
							"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13458", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13459"
						};
						parts.Clear();
						parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13446", deliveryItem.Value, source3.ElementAt(random.Next(12))));
					}
					if (target.Value.Equals("Gus") && random.NextDouble() < 0.6)
					{
						parts.Clear();
						parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13462", deliveryItem.Value));
					}
				}
				else if (random.NextDouble() < 0.5 && Convert.ToInt32(Game1.objectInformation[item].Split('/')[2]) < 0)
				{
					parts.Clear();
					parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13464", deliveryItem.Value, new DescriptionElement[5] { "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13465", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13466", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13467", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13468", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13469" }.ElementAt(random.Next(5))));
					parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", nPC));
					if (target.Value.Equals("Emily"))
					{
						parts.Clear();
						parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13473", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13476", deliveryItem.Value));
					}
				}
				else
				{
					DescriptionElement[] source4 = new DescriptionElement[12]
					{
						"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13502", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13503", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13504", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13505", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13506", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13507", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13508", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13509", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13510", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13511",
						"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13512", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13513"
					};
					array = new DescriptionElement[9]
					{
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13480", nPC, deliveryItem.Value),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13481", deliveryItem.Value),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13485", deliveryItem.Value),
						(random.NextDouble() < 0.4) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13491", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13492", deliveryItem.Value),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13494", deliveryItem.Value),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13497", deliveryItem.Value),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13500", deliveryItem.Value, source4.ElementAt(random.Next(12))),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13518", nPC, deliveryItem.Value),
						(random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13520", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13523", deliveryItem.Value)
					};
					array2 = new DescriptionElement[9]
					{
						"",
						(random.NextDouble() < 0.3) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13482" : ((random.NextDouble() < 0.5) ? "" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13483"),
						(random.NextDouble() < 0.25) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13487" : ((random.NextDouble() < 0.33) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13488" : ((random.NextDouble() < 0.5) ? "" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13489")),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", nPC),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", nPC),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", nPC),
						(random.NextDouble() < 0.5) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13514" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13516",
						"",
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", nPC)
					};
					array3 = new DescriptionElement[9]
					{
						"",
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", nPC),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", nPC),
						"",
						"",
						"",
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", nPC),
						"",
						""
					};
					parts.Clear();
					int num3 = random.Next(array.Count());
					parts.Add(array[num3]);
					parts.Add(array2[num3]);
					parts.Add(array3[num3]);
				}
			}
			dialogueparts.Clear();
			dialogueparts.Add((random.NextDouble() < 0.3 || target.Value.Equals("Evelyn")) ? ((DescriptionElement)"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13526") : ((random.NextDouble() < 0.5) ? ((DescriptionElement)"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13527") : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13528", Game1.player.Name)));
			dialogueparts.Add((random.NextDouble() < 0.3) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13530", deliveryItem.Value) : ((random.NextDouble() < 0.5) ? ((DescriptionElement)"Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13532") : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13533", (random.NextDouble() < 0.3) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13534") : ((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13535") : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13536")))));
			dialogueparts.Add((random.NextDouble() < 0.3) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13538" : ((random.NextDouble() < 0.5) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13539" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13540"));
			dialogueparts.Add((random.NextDouble() < 0.3) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13542" : ((random.NextDouble() < 0.5) ? "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13543" : "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13544"));
			if (target.Value.Equals("Wizard"))
			{
				parts.Clear();
				if (random.NextDouble() < 0.5)
				{
					parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13546", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13548", deliveryItem.Value));
				}
				else
				{
					parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13551", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13553", deliveryItem.Value));
				}
				dialogueparts.Clear();
				dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13555");
			}
			if (target.Value.Equals("Haley"))
			{
				parts.Clear();
				parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13557", deliveryItem.Value) : (Game1.player.isMale ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13560", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13563", deliveryItem.Value)));
				dialogueparts.Clear();
				dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13566");
			}
			if (target.Value.Equals("Sam"))
			{
				parts.Clear();
				parts.Add((random.NextDouble() < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13568", deliveryItem.Value) : (Game1.player.isMale ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13571", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13574", deliveryItem.Value)));
				dialogueparts.Clear();
				dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13577", Game1.player.Name));
			}
			if (target.Value.Equals("Maru"))
			{
				parts.Clear();
				double num4 = random.NextDouble();
				parts.Add((num4 < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13580", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13583", deliveryItem.Value));
				dialogueparts.Clear();
				dialogueparts.Add((num4 < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13585", Game1.player.Name) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13587", Game1.player.Name));
			}
			if (target.Value.Equals("Abigail"))
			{
				parts.Clear();
				double num5 = random.NextDouble();
				parts.Add((num5 < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13590", deliveryItem.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13593", deliveryItem.Value));
				dialogueparts.Add((num5 < 0.5) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13597", Game1.player.Name) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13599", Game1.player.Name));
			}
			if (target.Value.Equals("Sebastian"))
			{
				dialogueparts.Clear();
				dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13602");
			}
			if (target.Value.Equals("Elliott"))
			{
				dialogueparts.Clear();
				dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13604", deliveryItem.Value, Game1.player.Name));
			}
			DescriptionElement descriptionElement = ((random.NextDouble() < 0.3) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13608", nPC) : ((!(random.NextDouble() < 0.5)) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13612", nPC) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13610", nPC)));
			parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13607", (int)deliveryItem.Value.price * 3));
			parts.Add(descriptionElement);
			objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13614", nPC, deliveryItem.Value);
		}

		public override void reloadDescription()
		{
			if (_questDescription == "")
			{
				loadQuestInfo();
			}
			string text = "";
			string text2 = "";
			if (parts != null && parts.Count != 0)
			{
				foreach (DescriptionElement part in parts)
				{
					text += part.loadDescriptionElement();
				}
				base.questDescription = text;
			}
			if (dialogueparts != null && dialogueparts.Count != 0)
			{
				foreach (DescriptionElement dialoguepart in dialogueparts)
				{
					text2 += dialoguepart.loadDescriptionElement();
				}
				targetMessage = text2;
			}
			else
			{
				if ((int)id == 0)
				{
					return;
				}
				Dictionary<int, string> dictionary = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Quests");
				if (dictionary != null && dictionary.ContainsKey(id))
				{
					string[] array = dictionary[id].Split('/');
					if (array != null && array.Length > 9)
					{
						targetMessage = array[9];
					}
				}
			}
		}

		public override void reloadObjective()
		{
			if (objective.Value != null)
			{
				base.currentObjective = objective.Value.loadDescriptionElement();
			}
		}

		public override bool checkIfComplete(NPC n = null, int number1 = -1, int number2 = -1, Item item = null, string monsterName = null)
		{
			if ((bool)completed)
			{
				return false;
			}
			if (item != null && item is Object && n != null && n.isVillager() && n.Name.Equals(target.Value) && ((item as Object).ParentSheetIndex == (int)this.item || (item as Object).Category == (int)this.item))
			{
				if (item.Stack >= (int)number)
				{
					Game1.player.ActiveObject.Stack -= (int)number - 1;
					reloadDescription();
					n.CurrentDialogue.Push(new Dialogue(targetMessage, n));
					Game1.drawDialogue(n);
					Game1.player.reduceActiveItemByOne();
					if ((bool)dailyQuest)
					{
						Game1.player.changeFriendship(150, n);
						if (deliveryItem.Value == null)
						{
							deliveryItem.Value = new Object(Vector2.Zero, this.item, 1);
						}
						moneyReward.Value = (int)deliveryItem.Value.price * 3;
					}
					else
					{
						Game1.player.changeFriendship(255, n);
					}
					questComplete();
					return true;
				}
				n.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13615", number.Value), n));
				Game1.drawDialogue(n);
				return false;
			}
			return false;
		}
	}
}
