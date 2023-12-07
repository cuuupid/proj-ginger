using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class ManorHouse : GameLocation
	{
		[XmlIgnore]
		private Dictionary<string, Farmer> sendMoneyMapping = new Dictionary<string, Farmer>();

		private static readonly bool changeWalletTypeImmediately;

		public ManorHouse()
		{
		}

		public ManorHouse(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		public override bool performAction(string action, Farmer who, Location tileLocation)
		{
			if (action != null && who.IsLocalPlayer)
			{
				string[] array = action.Split(' ');
				switch (array[0])
				{
				case "LostAndFound":
					CheckLostAndFound();
					break;
				case "MayorFridge":
				{
					int num = 0;
					for (int i = 0; i < who.items.Count; i++)
					{
						if (who.items[i] != null && (int)who.items[i].parentSheetIndex == 284)
						{
							num += who.items[i].Stack;
						}
					}
					if (num >= 10 && !who.hasOrWillReceiveMail("TH_MayorFridge") && who.hasOrWillReceiveMail("TH_Railroad"))
					{
						int num2 = 0;
						for (int j = 0; j < who.items.Count; j++)
						{
							if (who.items[j] == null || (int)who.items[j].parentSheetIndex != 284)
							{
								continue;
							}
							while (num2 < 10)
							{
								who.items[j].Stack--;
								num2++;
								if (who.items[j].Stack == 0)
								{
									who.items[j] = null;
									break;
								}
							}
							if (num2 >= 10)
							{
								break;
							}
						}
						Game1.player.CanMove = false;
						localSound("coin");
						Game1.player.mailReceived.Add("TH_MayorFridge");
						Game1.multipleDialogues(new string[2]
						{
							Game1.content.LoadString("Strings\\Locations:ManorHouse_MayorFridge_ConsumeBeets"),
							Game1.content.LoadString("Strings\\Locations:ManorHouse_MayorFridge_MrQiNote")
						});
						Game1.player.removeQuest(3);
						Game1.player.addQuest(4);
					}
					else if (who.hasOrWillReceiveMail("TH_MayorFridge"))
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_MayorFridge_MrQiNote"));
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_MayorFridge_Initial"));
					}
					break;
				}
				case "DivorceBook":
					if ((bool)Game1.player.divorceTonight)
					{
						string text = null;
						if (Game1.player.hasCurrentOrPendingRoommate())
						{
							text = Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_CancelQuestion_Krobus", Game1.player.getSpouse().displayName);
						}
						if (text == null)
						{
							text = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_CancelQuestion");
						}
						createQuestionDialogue(text, createYesNoResponses(), "divorceCancel");
					}
					else if (Game1.player.isMarried())
					{
						string text2 = null;
						if (Game1.player.hasCurrentOrPendingRoommate())
						{
							text2 = Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_Question_Krobus", Game1.player.getSpouse().displayName);
						}
						if (text2 == null)
						{
							text2 = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Question");
						}
						createQuestionDialogue(text2, createYesNoResponses(), "divorce");
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_NoSpouse"));
					}
					break;
				case "LedgerBook":
					readLedgerBook();
					break;
				}
			}
			return base.performAction(action, who, tileLocation);
		}

		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (Game1.eventUp)
			{
				removeTile(4, 5, "Buildings");
				removeTile(4, 4, "Front");
				removeTile(4, 3, "Front");
				setMapTile(4, 6, 635, "Back", null);
			}
			else
			{
				setMapTile(4, 5, 109, "Buildings", "LostAndFound", 2);
				setMapTile(4, 4, 77, "Front", null, 2);
				setMapTile(4, 3, 110, "Front", null, 2);
				setMapTile(4, 6, 604, "Back", null);
			}
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
		}

		public void CheckLostAndFound()
		{
			string text = "";
			text = ((!SpecialOrder.IsSpecialOrdersBoardUnlocked()) ? Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_Check") : Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_Check_OrdersUnlocked"));
			List<Response> list = new List<Response>();
			if (Game1.player.team.returnedDonations.Count > 0 && !Game1.player.team.returnedDonationsMutex.IsLocked())
			{
				list.Add(new Response("CheckDonations", Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_DonationItems")));
			}
			List<Farmer> retrievableFarmers = GetRetrievableFarmers();
			if (retrievableFarmers.Count > 0)
			{
				list.Add(new Response("RetrieveFarmhandItems", Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_FarmhandItems")));
			}
			if (list.Count > 0)
			{
				list.Add(new Response("Cancel", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_TransferCancel")));
			}
			if (list.Count > 0)
			{
				createQuestionDialogue(text, list.ToArray(), "lostAndFound");
			}
			else
			{
				Game1.drawObjectDialogue(text);
			}
		}

		public List<Farmer> GetRetrievableFarmers()
		{
			List<Farmer> list = new List<Farmer>(Game1.getAllFarmers());
			foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
			{
				list.Remove(onlineFarmer);
			}
			for (int i = 0; i < list.Count; i++)
			{
				Farmer farmer = list[i];
				if (Utility.getHomeOfFarmer(farmer) is Cabin cabin && (farmer.isUnclaimedFarmhand || cabin.inventoryMutex.IsLocked()))
				{
					list.RemoveAt(i);
					i--;
				}
			}
			return list;
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (Game1.player.team.returnedDonations.Count > 0 && !Game1.eventUp)
			{
				float num = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds / 250.0), 2);
				Vector2 vector = new Vector2(4f, 4f) * 64f + new Vector2(7f, 0f) * 4f;
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(vector.X, vector.Y + num)), new Microsoft.Xna.Framework.Rectangle(114, 53, 6, 10), Color.White, 0f, new Vector2(1f, 4f), 4f, SpriteEffects.None, 1f);
			}
		}

		private void readLedgerBook()
		{
			if (Game1.player.useSeparateWallets)
			{
				if (Game1.IsMasterGame)
				{
					List<Response> list = new List<Response>();
					list.Add(new Response("SendMoney", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SendMoney")));
					if ((bool)Game1.player.changeWalletTypeTonight)
					{
						list.Add(new Response("CancelMerge", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_CancelMerge")));
					}
					else
					{
						list.Add(new Response("MergeWallets", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_MergeWallets")));
					}
					list.Add(new Response("Leave", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_Leave")));
					createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SeparateWallets_HostQuestion"), list.ToArray(), "ledgerOptions");
				}
				else
				{
					ChooseRecipient();
				}
			}
			else if (Game1.getAllFarmhands().Count() == 0)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_Singleplayer"));
			}
			else if (Game1.IsMasterGame)
			{
				if ((bool)Game1.player.changeWalletTypeTonight)
				{
					string question = Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SharedWallets_CancelQuestion");
					createQuestionDialogue(question, createYesNoResponses(), "cancelSeparateWallets");
				}
				else
				{
					string question2 = Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SharedWallets_SeparateQuestion");
					createQuestionDialogue(question2, createYesNoResponses(), "separateWallets");
				}
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SharedWallets_Client"));
			}
		}

		public void ShowOfflineFarmhandItemList()
		{
			List<Response> list = new List<Response>();
			List<Farmer> retrievableFarmers = GetRetrievableFarmers();
			foreach (Farmer item in retrievableFarmers)
			{
				string responseKey = item.UniqueMultiplayerID.ToString() ?? "";
				string responseText = item.Name;
				if (item.Name == "")
				{
					responseText = Game1.content.LoadString("Strings\\UI:Chat_PlayerJoinedNewName");
				}
				list.Add(new Response(responseKey, responseText));
			}
			list.Add(new Response("Cancel", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_TransferCancel")));
			Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_FarmhandItemsQuestion"), list.ToArray(), "CheckItems");
		}

		public void ChooseRecipient()
		{
			sendMoneyMapping.Clear();
			List<Response> list = new List<Response>();
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID && !allFarmer.isUnclaimedFarmhand)
				{
					string text = "Transfer" + (list.Count + 1);
					string responseText = allFarmer.Name;
					if (allFarmer.Name == "")
					{
						responseText = Game1.content.LoadString("Strings\\UI:Chat_PlayerJoinedNewName");
					}
					list.Add(new Response(text, responseText));
					sendMoneyMapping.Add(text, allFarmer);
				}
			}
			if (list.Count == 0)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_NoFarmhands"));
				return;
			}
			list.Sort((Response x, Response y) => string.Compare(x.responseKey, y.responseKey));
			list.Add(new Response("Cancel", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_TransferCancel")));
			Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SeparateWallets_TransferQuestion"), list.ToArray(), "chooseRecipient");
		}

		private void beginSendMoney(Farmer recipient)
		{
			Game1.activeClickableMenu = new DigitEntryMenu(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SeparateWallets_HowMuchQuestion"), delegate(int currentValue, int price, Farmer who)
			{
				sendMoney(recipient, currentValue);
			}, -1, 1, Game1.player.Money);
		}

		public void sendMoney(Farmer recipient, int amount)
		{
			Game1.playSound("smallSelect");
			Game1.player.Money -= amount;
			Game1.player.team.AddIndividualMoney(recipient, amount);
			Game1.player.stats.onMoneyGifted((uint)amount);
			if (amount == 1)
			{
				Game1.multiplayer.globalChatInfoMessage("Sent1g", Game1.player.Name, recipient.Name);
			}
			else
			{
				Game1.multiplayer.globalChatInfoMessage("SentMoney", Game1.player.Name, recipient.Name, Utility.getNumberWithCommas(amount));
			}
			Game1.exitActiveMenu();
		}

		public static void SeparateWallets()
		{
			if (Game1.player.useSeparateWallets || !Game1.IsMasterGame)
			{
				return;
			}
			Game1.player.changeWalletTypeTonight.Value = false;
			int money = Game1.player.Money;
			int num = 0;
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (!allFarmer.isUnclaimedFarmhand)
				{
					num++;
				}
			}
			int value = money / Math.Max(num, 1);
			Game1.player.team.useSeparateWallets.Value = true;
			foreach (Farmer allFarmer2 in Game1.getAllFarmers())
			{
				if (!allFarmer2.isUnclaimedFarmhand)
				{
					Game1.player.team.SetIndividualMoney(allFarmer2, value);
				}
			}
			Game1.multiplayer.globalChatInfoMessage("SeparatedWallets", Game1.player.Name, value.ToString());
		}

		public static void MergeWallets()
		{
			if (!Game1.player.useSeparateWallets || !Game1.IsMasterGame)
			{
				return;
			}
			Game1.player.changeWalletTypeTonight.Value = false;
			int num = 0;
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (!allFarmer.isUnclaimedFarmhand)
				{
					num += Game1.player.team.GetIndividualMoney(allFarmer);
				}
			}
			Game1.player.team.useSeparateWallets.Value = false;
			Game1.player.team.money.Value = num;
			Game1.multiplayer.globalChatInfoMessage("MergedWallets", Game1.player.Name);
		}

		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			string text = null;
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (questionAndAnswer != null)
			{
				switch (questionAndAnswer.Length)
				{
				case 25:
					switch (questionAndAnswer[0])
					{
					case 'c':
						if (questionAndAnswer == "cancelSeparateWallets_Yes")
						{
							Game1.player.changeWalletTypeTonight.Value = false;
							Game1.multiplayer.globalChatInfoMessage("SeparateWalletsCancel", Game1.player.Name);
						}
						break;
					case 'l':
						if (questionAndAnswer == "ledgerOptions_CancelMerge")
						{
							text = Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SeparateWallets_CancelQuestion");
							createQuestionDialogue(text, createYesNoResponses(), "cancelMergeWallets");
						}
						break;
					}
					break;
				case 17:
					if (questionAndAnswer == "divorceCancel_Yes" && (bool)Game1.player.divorceTonight)
					{
						Game1.player.divorceTonight.Value = false;
						if (!Game1.player.isRoommate(Game1.player.spouse))
						{
							Game1.player.addUnearnedMoney(50000);
						}
						if (Game1.player.hasCurrentOrPendingRoommate())
						{
							text = Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_Cancelled_Krobus", Game1.player.getSpouse().displayName);
						}
						if (text == null)
						{
							text = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Cancelled");
						}
						Game1.drawObjectDialogue(text);
						if (!Game1.player.isRoommate(Game1.player.spouse))
						{
							Game1.multiplayer.globalChatInfoMessage("DivorceCancel", Game1.player.Name);
						}
					}
					break;
				case 11:
					if (!(questionAndAnswer == "divorce_Yes"))
					{
						break;
					}
					if (Game1.player.Money >= 50000 || Game1.player.hasCurrentOrPendingRoommate())
					{
						if (!Game1.player.isRoommate(Game1.player.spouse))
						{
							Game1.player.Money -= 50000;
						}
						Game1.player.divorceTonight.Value = true;
						if (Game1.player.hasCurrentOrPendingRoommate())
						{
							text = Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_Filed_Krobus", Game1.player.getSpouse().displayName);
						}
						if (text == null)
						{
							text = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed");
						}
						Game1.drawObjectDialogue(text);
						if (!Game1.player.isRoommate(Game1.player.spouse))
						{
							Game1.multiplayer.globalChatInfoMessage("Divorce", Game1.player.Name);
						}
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
					}
					break;
				case 19:
					if (questionAndAnswer == "separateWallets_Yes")
					{
						if (changeWalletTypeImmediately)
						{
							SeparateWallets();
							break;
						}
						Game1.player.changeWalletTypeTonight.Value = true;
						Game1.multiplayer.globalChatInfoMessage("SeparateWallets", Game1.player.Name);
					}
					break;
				case 16:
					if (questionAndAnswer == "mergeWallets_Yes")
					{
						if (changeWalletTypeImmediately)
						{
							MergeWallets();
							break;
						}
						Game1.player.changeWalletTypeTonight.Value = true;
						Game1.multiplayer.globalChatInfoMessage("MergeWallets", Game1.player.Name);
					}
					break;
				case 22:
					if (questionAndAnswer == "cancelMergeWallets_Yes")
					{
						Game1.player.changeWalletTypeTonight.Value = false;
						Game1.multiplayer.globalChatInfoMessage("MergeWalletsCancel", Game1.player.Name);
					}
					break;
				case 23:
					if (questionAndAnswer == "ledgerOptions_SendMoney")
					{
						ChooseRecipient();
					}
					break;
				case 26:
					if (questionAndAnswer == "ledgerOptions_MergeWallets")
					{
						text = Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SeparateWallets_MergeQuestion");
						createQuestionDialogue(text, createYesNoResponses(), "mergeWallets");
					}
					break;
				case 34:
					if (!(questionAndAnswer == "lostAndFound_RetrieveFarmhandItems"))
					{
						break;
					}
					ShowOfflineFarmhandItemList();
					return true;
				case 27:
					if (!(questionAndAnswer == "lostAndFound_CheckDonations"))
					{
						break;
					}
					Game1.player.team.CheckReturnedDonations();
					return true;
				}
			}
			if (questionAndAnswer.StartsWith("CheckItems"))
			{
				string s = questionAndAnswer.Split('_')[1];
				long result = 0L;
				if (long.TryParse(s, out result))
				{
					Farmer farmerMaybeOffline = Game1.getFarmerMaybeOffline(result);
					Cabin home = Utility.getHomeOfFarmer(farmerMaybeOffline) as Cabin;
					if (farmerMaybeOffline != null && home != null && !farmerMaybeOffline.isActive())
					{
						home.inventoryMutex.RequestLock(delegate
						{
							home.openFarmhandInventory();
						});
					}
				}
				return true;
			}
			if (questionAndAnswer.Contains("Transfer"))
			{
				string key = questionAndAnswer.Split('_')[1];
				beginSendMoney(sendMoneyMapping[key]);
				sendMoneyMapping.Clear();
			}
			return base.answerDialogueAction(questionAndAnswer, questionParams);
		}
	}
}
