using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	public class ChatBox : IClickableMenu
	{
		public const int chatMessage = 0;

		public const int errorMessage = 1;

		public const int userNotificationMessage = 2;

		public const int privateMessage = 3;

		public const int defaultMaxMessages = 10;

		public const int timeToDisplayMessages = 600;

		public const int chatboxWidth = 896;

		public const int chatboxHeight = 56;

		public const int region_chatBox = 101;

		public const int region_emojiButton = 102;

		public ChatTextBox chatBox;

		public ClickableComponent chatBoxCC;

		private TextBoxEvent e;

		private TextBoxEvent e_backspace;

		private List<ChatMessage> messages = new List<ChatMessage>();

		private KeyboardState oldKBState;

		private List<string> cheatHistory = new List<string>();

		private int cheatHistoryPosition = -1;

		public int maxMessages = 10;

		public static Texture2D emojiTexture;

		public ClickableTextureComponent emojiMenuIcon;

		public EmojiMenu emojiMenu;

		public bool choosingEmoji;

		public bool enableCheats;

		private long lastReceivedPrivateMessagePlayerId;

		private bool _justShownKeyboard;

		public ChatBox()
		{
			enableCheats = true;
			Texture2D texture2D = Game1.content.Load<Texture2D>("LooseSprites\\chatBox");
			chatBox = new ChatTextBox(texture2D, null, Game1.smallFont, Color.White);
			e = textBoxEnter;
			chatBox.OnEnterPressed += e;
			chatBox.TitleText = "Chat";
			chatBoxCC = new ClickableComponent(new Rectangle(chatBox.X, chatBox.Y, chatBox.Width, chatBox.Height), "")
			{
				myID = 101
			};
			Game1.keyboardDispatcher.Subscriber = chatBox;
			emojiTexture = Game1.content.Load<Texture2D>("LooseSprites\\emojis");
			emojiMenuIcon = new ClickableTextureComponent(new Rectangle(0, 0, 40, 36), emojiTexture, new Rectangle(0, 0, 9, 9), 4f)
			{
				myID = 102,
				leftNeighborID = 101
			};
			emojiMenu = new EmojiMenu(this, emojiTexture, texture2D);
			emojiMenuIcon.visible = false;
			updatePosition();
			chatBox.Selected = false;
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(101);
			snapCursorToCurrentSnappedComponent();
		}

		private void updatePosition()
		{
			chatBox.Width = 896;
			chatBox.Height = 56;
			width = chatBox.Width;
			height = chatBox.Height;
			xPositionOnScreen = 0;
			yPositionOnScreen = Game1.uiViewport.Height - chatBox.Height;
			Utility.makeSafe(ref xPositionOnScreen, ref yPositionOnScreen, chatBox.Width, chatBox.Height);
			chatBox.X = xPositionOnScreen;
			chatBox.Y = yPositionOnScreen;
			chatBoxCC.bounds = new Rectangle(chatBox.X, chatBox.Y, chatBox.Width, chatBox.Height);
			emojiMenuIcon.bounds.Y = chatBox.Y + 8;
			emojiMenuIcon.bounds.X = chatBox.Width - emojiMenuIcon.bounds.Width - 8;
			if (emojiMenu != null)
			{
				emojiMenu.xPositionOnScreen = emojiMenuIcon.bounds.Center.X - 146;
				emojiMenu.yPositionOnScreen = emojiMenuIcon.bounds.Y - 248;
			}
		}

		public virtual void textBoxEnter(string text_to_send)
		{
			if (text_to_send.Length >= 1)
			{
				if (text_to_send[0] == '/' && text_to_send.Split(' ')[0].Length > 1)
				{
					runCommand(text_to_send.Substring(1));
					return;
				}
				text_to_send = Program.sdk.FilterDirtyWords(text_to_send);
				Game1.multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, text_to_send, Multiplayer.AllPlayers);
				receiveChatMessage(Game1.player.UniqueMultiplayerID, 0, LocalizedContentManager.CurrentLanguageCode, text_to_send);
			}
		}

		public virtual void textBoxEnter(TextBox sender)
		{
			if (sender is ChatTextBox)
			{
				ChatTextBox chatTextBox = sender as ChatTextBox;
				if (chatTextBox.finalText.Count > 0)
				{
					bool include_color_information = true;
					if (chatTextBox.finalText[0].message != null && chatTextBox.finalText[0].message.Length > 0 && chatTextBox.finalText[0].message.ToString()[0] == '/' && chatTextBox.finalText[0].message.Split(' ')[0].Length > 1)
					{
						include_color_information = false;
					}
					if (chatTextBox.finalText.Count != 1 || ((chatTextBox.finalText[0].message != null || chatTextBox.finalText[0].emojiIndex != -1) && (chatTextBox.finalText[0].message == null || chatTextBox.finalText[0].message.Trim().Length != 0)))
					{
						string text_to_send = ChatMessage.makeMessagePlaintext(chatTextBox.finalText, include_color_information);
						textBoxEnter(text_to_send);
					}
				}
				chatTextBox.reset();
				cheatHistoryPosition = -1;
			}
			sender.Text = "";
			clickAway();
		}

		public virtual void addInfoMessage(string message)
		{
			receiveChatMessage(0L, 2, LocalizedContentManager.CurrentLanguageCode, message);
		}

		public virtual void globalInfoMessage(string messageKey, params string[] args)
		{
			if (Game1.IsMultiplayer)
			{
				Game1.multiplayer.globalChatInfoMessage(messageKey, args);
			}
			else
			{
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_" + messageKey, args));
			}
		}

		public virtual void addErrorMessage(string message)
		{
			receiveChatMessage(0L, 1, LocalizedContentManager.CurrentLanguageCode, message);
		}

		public virtual void listPlayers(bool otherPlayersOnly = false)
		{
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_UserList"));
			foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
			{
				if (!otherPlayersOnly || onlineFarmer.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
				{
					addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_UserListUser", formattedUserNameLong(onlineFarmer)));
				}
			}
		}

		public virtual void showHelp()
		{
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_Help"));
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpClear", "clear"));
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpList", "list"));
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpColor", "color"));
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpColorList", "color-list"));
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpPause", "pause"));
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpResume", "resume"));
			if (Game1.IsMultiplayer)
			{
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpMessage", "message"));
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpReply", "reply"));
			}
			if (Game1.IsServer)
			{
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpKick", "kick"));
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpBan", "ban"));
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpUnban", "unban"));
			}
		}

		protected virtual void runCommand(string command)
		{
			string[] array = command.Split(' ');
			string text = array[0];
			if (text != null)
			{
				bool flag;
				string text3;
				StringBuilder stringBuilder2;
				switch (text.Length)
				{
				case 2:
				{
					char c = text[0];
					if ((uint)c <= 100u)
					{
						if (c == 'c')
						{
							if (!(text == "ca"))
							{
								break;
							}
							goto IL_06a1;
						}
						if (c != 'd' || !(text == "dm"))
						{
							break;
						}
					}
					else
					{
						if (c != 'p')
						{
							if (c != 'q' || !(text == "qi"))
							{
								break;
							}
							if (!Game1.player.mailReceived.Contains("QiChat1"))
							{
								Game1.player.mailReceived.Add("QiChat1");
								addMessage(Game1.content.LoadString("Strings\\UI:Chat_Qi1"), new Color(100, 50, 255));
							}
							else if (!Game1.player.mailReceived.Contains("QiChat2"))
							{
								Game1.player.mailReceived.Add("QiChat2");
								addMessage(Game1.content.LoadString("Strings\\UI:Chat_Qi2"), new Color(100, 50, 255));
								addMessage(Game1.content.LoadString("Strings\\UI:Chat_Qi3"), Color.Yellow);
							}
							return;
						}
						if (!(text == "pm"))
						{
							break;
						}
					}
					goto IL_070d;
				}
				case 3:
				{
					char c = text[0];
					if (c != 'a')
					{
						if (c != 'b')
						{
							if (c != 'm' || !(text == "mbp"))
							{
								break;
							}
							goto IL_09ad;
						}
						if (!(text == "ban"))
						{
							break;
						}
						if (Game1.IsMultiplayer && Game1.IsServer)
						{
							banPlayer(command);
						}
						return;
					}
					if (!(text == "ape"))
					{
						break;
					}
					goto IL_06a1;
				}
				case 12:
				{
					char c = text[0];
					if (c != 'C')
					{
						if (c != 'c' || !(text == "concernedape"))
						{
							break;
						}
					}
					else if (!(text == "ConcernedApe"))
					{
						break;
					}
					goto IL_06a1;
				}
				case 7:
				{
					char c = text[0];
					if ((uint)c <= 112u)
					{
						if (c != 'm')
						{
							if (c != 'p' || !(text == "players"))
							{
								break;
							}
							goto IL_085c;
						}
						if (!(text == "message"))
						{
							break;
						}
					}
					else
					{
						if (c == 'r')
						{
							if (!(text == "rosebud"))
							{
								break;
							}
							goto IL_071d;
						}
						if (c != 'w' || !(text == "whisper"))
						{
							break;
						}
					}
					goto IL_070d;
				}
				case 5:
					switch (text[3])
					{
					case 'l':
						break;
					case 'a':
						goto IL_03a6;
					case 'u':
						goto IL_03db;
					case 's':
						goto IL_03f0;
					case 'o':
						if (!(text == "color"))
						{
							goto end_IL_001e;
						}
						if (array.Length > 1)
						{
							Game1.player.defaultChatColor = array[1];
						}
						return;
					case 'r':
						goto IL_041a;
					case 'e':
						goto IL_042f;
					case 't':
						goto IL_0444;
					default:
						goto end_IL_001e;
					}
					if (!(text == "reply"))
					{
						break;
					}
					goto IL_0715;
				case 1:
				{
					char c = text[0];
					if (c != 'e')
					{
						if (c != 'h')
						{
							if (c != 'r')
							{
								break;
							}
							goto IL_0715;
						}
						goto IL_0864;
					}
					goto IL_0bb9;
				}
				case 14:
				{
					char c = text[0];
					if (c != 'm')
					{
						if (c != 's' || !(text == "showmethemoney"))
						{
							break;
						}
						goto IL_071d;
					}
					if (!(text == "movepermission"))
					{
						break;
					}
					goto IL_09ad;
				}
				case 8:
				{
					char c = text[5];
					if ((uint)c <= 97u)
					{
						if (c != 'A')
						{
							if (c != 'a' || !(text == "unbanall"))
							{
								break;
							}
						}
						else if (!(text == "unbanAll"))
						{
							break;
						}
						if (Game1.IsServer)
						{
							if (Game1.bannedUsers.Count == 0)
							{
								addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_BannedPlayersList_None"));
							}
							else
							{
								unbanAll();
							}
						}
						return;
					}
					if (c != 'e')
					{
						if (c != 'o' || !(text == "freegold"))
						{
							break;
						}
					}
					else if (!(text == "imacheat"))
					{
						break;
					}
					goto IL_071d;
				}
				case 6:
				{
					char c = text[0];
					if (c != 'c')
					{
						if (c != 'r' || !(text == "resume"))
						{
							break;
						}
						if (!Game1.IsMasterGame)
						{
							addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_HostOnlyCommand"));
						}
						else if (Game1.netWorldState.Value.IsPaused)
						{
							Game1.netWorldState.Value.IsPaused = false;
							globalInfoMessage("Resumed");
						}
						return;
					}
					if (!(text == "cheats"))
					{
						break;
					}
					goto IL_071d;
				}
				case 10:
					switch (text[0])
					{
					case 'c':
						if (!(text == "color-list"))
						{
							break;
						}
						addMessage("white, red, blue, green, jade, yellowgreen, pink, purple, yellow, orange, brown, gray, cream, salmon, peach, aqua, jungle, plum", Color.White);
						return;
					case 'f':
						if (!(text == "fixweapons"))
						{
							break;
						}
						Game1.applySaveFix(SaveGame.SaveFixes.ResetForges);
						addMessage("Reset forged weapon attributes.", Color.White);
						return;
					}
					break;
				case 4:
					switch (text[0])
					{
					case 'l':
						break;
					case 'h':
						goto IL_0555;
					case 'k':
						if (!(text == "kick"))
						{
							goto end_IL_001e;
						}
						if (Game1.IsMultiplayer && Game1.IsServer)
						{
							kickPlayer(command);
						}
						return;
					case 'p':
						goto IL_057f;
					default:
						goto end_IL_001e;
					}
					if (!(text == "list"))
					{
						break;
					}
					goto IL_085c;
				case 11:
					switch (text[2])
					{
					case 'c':
						if (!(text == "recountnuts"))
						{
							break;
						}
						Game1.game1.RecountWalnuts();
						return;
					case 's':
						if (!(text == "resetisland"))
						{
							break;
						}
						Game1.game1.ResetIslandLocations();
						return;
					}
					break;
				case 9:
				{
					if (!(text == "printdiag"))
					{
						break;
					}
					StringBuilder stringBuilder = new StringBuilder();
					addInfoMessage(stringBuilder.ToString());
					Console.WriteLine(stringBuilder.ToString());
					return;
				}
				case 22:
					if (!(text == "movebuildingpermission"))
					{
						break;
					}
					goto IL_09ad;
				case 17:
					{
						if (!(text == "sleepannouncemode"))
						{
							break;
						}
						if (!Game1.IsMasterGame)
						{
							return;
						}
						if (array.Count() > 1)
						{
							switch (array[1])
							{
							case "all":
								Game1.player.team.sleepAnnounceMode.Value = FarmerTeam.SleepAnnounceModes.All;
								break;
							case "first":
								Game1.player.team.sleepAnnounceMode.Value = FarmerTeam.SleepAnnounceModes.First;
								break;
							case "off":
								Game1.player.team.sleepAnnounceMode.Value = FarmerTeam.SleepAnnounceModes.Off;
								break;
							}
						}
						Game1.multiplayer.globalChatInfoMessage("SleepAnnounceModeSet", Game1.content.LoadString("Strings\\UI:SleepAnnounceMode_" + Game1.player.team.sleepAnnounceMode.Value));
						return;
					}
					IL_06a1:
					if (!Game1.player.mailReceived.Contains("apeChat1"))
					{
						Game1.player.mailReceived.Add("apeChat1");
						addMessage(Game1.content.LoadString("Strings\\UI:Chat_ConcernedApe"), new Color(104, 214, 255));
					}
					else
					{
						addMessage(Game1.content.LoadString("Strings\\UI:Chat_ConcernedApe2"), Color.Yellow);
					}
					return;
					IL_071d:
					addMessage(Game1.content.LoadString("Strings\\UI:Chat_ConcernedApeNiceTry"), new Color(104, 214, 255));
					return;
					IL_0bb9:
					if (!Game1.player.CanEmote())
					{
						return;
					}
					flag = false;
					if (array.Count() > 1)
					{
						string text2 = array[1];
						text2 = text2.Substring(0, Math.Min(text2.Length, 16));
						text2.Trim();
						text2.ToLower();
						for (int i = 0; i < Farmer.EMOTES.Length; i++)
						{
							if (text2 == Farmer.EMOTES[i].emoteString)
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							Game1.player.netDoEmote(text2);
						}
					}
					if (flag)
					{
						return;
					}
					text3 = "";
					for (int j = 0; j < Farmer.EMOTES.Length; j++)
					{
						if (!Farmer.EMOTES[j].hidden)
						{
							text3 += Farmer.EMOTES[j].emoteString;
							if (j < Farmer.EMOTES.Length - 1)
							{
								text3 += ", ";
							}
						}
					}
					addMessage(text3, Color.White);
					return;
					IL_057f:
					if (!(text == "ping"))
					{
						break;
					}
					if (!Game1.IsMultiplayer)
					{
						return;
					}
					stringBuilder2 = new StringBuilder();
					if (Game1.IsServer)
					{
						foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
						{
							stringBuilder2.Clear();
							stringBuilder2.AppendFormat("Ping({0}) {1}ms ", otherFarmer.Value.Name, (int)Game1.server.getPingToClient(otherFarmer.Key));
							addMessage(stringBuilder2.ToString(), Color.White);
						}
						return;
					}
					stringBuilder2.AppendFormat("Ping: {0}ms", (int)Game1.client.GetPingToHost());
					addMessage(stringBuilder2.ToString(), Color.White);
					return;
					IL_042f:
					if (!(text == "money"))
					{
						break;
					}
					if (enableCheats)
					{
						cheat(command);
					}
					else
					{
						addMessage(Game1.content.LoadString("Strings\\UI:Chat_ConcernedApeNiceTry"), new Color(104, 214, 255));
					}
					return;
					IL_03db:
					if (!(text == "debug"))
					{
						break;
					}
					goto IL_071d;
					IL_0555:
					if (!(text == "help"))
					{
						break;
					}
					goto IL_0864;
					IL_070d:
					sendPrivateMessage(command);
					return;
					IL_0715:
					replyPrivateMessage(command);
					return;
					IL_041a:
					if (!(text == "users"))
					{
						break;
					}
					goto IL_085c;
					IL_03f0:
					if (!(text == "pause"))
					{
						break;
					}
					if (!Game1.IsMasterGame)
					{
						addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_HostOnlyCommand"));
						return;
					}
					Game1.netWorldState.Value.IsPaused = !Game1.netWorldState.Value.IsPaused;
					if (Game1.netWorldState.Value.IsPaused)
					{
						globalInfoMessage("Paused");
					}
					else
					{
						globalInfoMessage("Resumed");
					}
					return;
					IL_0864:
					showHelp();
					return;
					IL_03a6:
					switch (text)
					{
					case "cheat":
						break;
					case "clear":
						messages.Clear();
						return;
					case "unban":
						if (Game1.IsServer)
						{
							unbanPlayer(command);
						}
						return;
					default:
						goto end_IL_001e;
					}
					goto IL_071d;
					IL_0444:
					if (!(text == "emote"))
					{
						break;
					}
					goto IL_0bb9;
					IL_085c:
					listPlayers();
					return;
					IL_09ad:
					if (!Game1.IsMasterGame)
					{
						return;
					}
					if (array.Count() > 1)
					{
						switch (array[1])
						{
						case "off":
							Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.Off;
							break;
						case "owned":
							Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.OwnedBuildings;
							break;
						case "on":
							Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.On;
							break;
						}
						addMessage("movebuildingpermission " + Game1.player.team.farmhandsCanMoveBuildings.Value, Color.White);
					}
					else
					{
						addMessage("off, owned, on", Color.White);
					}
					return;
					end_IL_001e:
					break;
				}
			}
			if (enableCheats || Game1.isRunningMacro)
			{
				cheat(command);
			}
		}

		public virtual void cheat(string command)
		{
			Game1.debugOutput = null;
			addInfoMessage("/" + command);
			if (!Game1.isRunningMacro)
			{
				cheatHistory.Insert(0, "/" + command);
			}
			if (Game1.game1.parseDebugInput(command))
			{
				if (Game1.debugOutput != null && Game1.debugOutput != "")
				{
					addInfoMessage(Game1.debugOutput);
				}
			}
			else if (Game1.debugOutput != null && Game1.debugOutput != "")
			{
				addErrorMessage(Game1.debugOutput);
			}
			else
			{
				addErrorMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:ChatBox.cs.10261") + " " + command.Split(' ')[0]);
			}
		}

		private void replyPrivateMessage(string command)
		{
			if (!Game1.IsMultiplayer)
			{
				return;
			}
			if (lastReceivedPrivateMessagePlayerId == 0L)
			{
				addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_NoPlayerToReplyTo"));
				return;
			}
			bool flag = !Game1.otherFarmers.ContainsKey(lastReceivedPrivateMessagePlayerId);
			if (!flag)
			{
				Farmer farmer = Game1.otherFarmers[lastReceivedPrivateMessagePlayerId];
				flag = !farmer.isActive();
			}
			if (flag)
			{
				addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_CouldNotReply"));
				return;
			}
			string[] array = command.Split(' ');
			if (array.Length <= 1)
			{
				return;
			}
			string text = "";
			for (int i = 1; i < array.Length; i++)
			{
				text += array[i];
				if (i < array.Length - 1)
				{
					text += " ";
				}
			}
			text = Program.sdk.FilterDirtyWords(text);
			Game1.multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, text, lastReceivedPrivateMessagePlayerId);
			receiveChatMessage(Game1.player.UniqueMultiplayerID, 3, LocalizedContentManager.CurrentLanguageCode, text);
		}

		private void kickPlayer(string command)
		{
			int matchingIndex = 0;
			Farmer farmer = findMatchingFarmer(command, ref matchingIndex, allowMatchingByUserName: true);
			if (farmer != null)
			{
				Game1.server.kick(farmer.UniqueMultiplayerID);
				return;
			}
			addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_NoPlayerWithThatName"));
			listPlayers(otherPlayersOnly: true);
		}

		private void banPlayer(string command)
		{
			int matchingIndex = 0;
			Farmer farmer = findMatchingFarmer(command, ref matchingIndex, allowMatchingByUserName: true);
			if (farmer != null)
			{
				string text = Game1.server.ban(farmer.UniqueMultiplayerID);
				if (text == null || !Game1.bannedUsers.ContainsKey(text))
				{
					addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_BannedPlayerFailed"));
					return;
				}
				string text2 = Game1.bannedUsers[text];
				string sub = ((text2 != null) ? (text2 + " (" + text + ")") : text);
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_BannedPlayer", sub));
			}
			else
			{
				addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_NoPlayerWithThatName"));
				listPlayers(otherPlayersOnly: true);
			}
		}

		private void unbanAll()
		{
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_UnbannedAllPlayers"));
			Game1.bannedUsers.Clear();
		}

		private void unbanPlayer(string command)
		{
			if (Game1.bannedUsers.Count == 0)
			{
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_BannedPlayersList_None"));
				return;
			}
			bool flag = false;
			string[] array = command.Split(' ');
			if (array.Length > 1)
			{
				string text = array[1];
				string text2 = null;
				if (Game1.bannedUsers.ContainsKey(text))
				{
					text2 = text;
				}
				else
				{
					foreach (KeyValuePair<string, string> bannedUser in Game1.bannedUsers)
					{
						if (bannedUser.Value == text)
						{
							text2 = bannedUser.Key;
							break;
						}
					}
				}
				if (text2 != null)
				{
					string sub = ((Game1.bannedUsers[text2] == null) ? text2 : (Game1.bannedUsers[text2] + " (" + text2 + ")"));
					addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_UnbannedPlayer", sub));
					Game1.bannedUsers.Remove(text2);
				}
				else
				{
					flag = true;
					addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_UnbanPlayer_NotFound"));
				}
			}
			else
			{
				flag = true;
			}
			if (!flag)
			{
				return;
			}
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_BannedPlayersList"));
			foreach (KeyValuePair<string, string> bannedUser2 in Game1.bannedUsers)
			{
				string message = "- " + bannedUser2.Key;
				if (bannedUser2.Value != null)
				{
					message = "- " + bannedUser2.Value + " (" + bannedUser2.Key + ")";
				}
				addInfoMessage(message);
			}
		}

		private Farmer findMatchingFarmer(string command, ref int matchingIndex, bool allowMatchingByUserName = false)
		{
			string[] array = command.Split(' ');
			Farmer result = null;
			foreach (Farmer value in Game1.otherFarmers.Values)
			{
				string[] array2 = value.displayName.Split(' ');
				bool flag = true;
				int i;
				for (i = 0; i < array2.Length; i++)
				{
					if (array.Length > i + 1)
					{
						if (array[i + 1].ToLowerInvariant() != array2[i].ToLowerInvariant())
						{
							flag = false;
							break;
						}
						continue;
					}
					flag = false;
					break;
				}
				if (flag)
				{
					result = value;
					matchingIndex = i;
					return result;
				}
				if (!allowMatchingByUserName)
				{
					continue;
				}
				flag = true;
				string[] array3 = Game1.multiplayer.getUserName(value.UniqueMultiplayerID).Split(' ');
				for (i = 0; i < array3.Length; i++)
				{
					if (array.Length > i + 1)
					{
						if (array[i + 1].ToLowerInvariant() != array3[i].ToLowerInvariant())
						{
							flag = false;
							break;
						}
						continue;
					}
					flag = false;
					break;
				}
				if (flag)
				{
					result = value;
					matchingIndex = i;
					return result;
				}
			}
			return result;
		}

		private void sendPrivateMessage(string command)
		{
			if (!Game1.IsMultiplayer)
			{
				return;
			}
			string[] array = command.Split(' ');
			int matchingIndex = 0;
			Farmer farmer = findMatchingFarmer(command, ref matchingIndex);
			if (farmer == null)
			{
				addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_NoPlayerWithThatName"));
				return;
			}
			string text = "";
			for (int i = matchingIndex + 1; i < array.Length; i++)
			{
				text += array[i];
				if (i < array.Length - 1)
				{
					text += " ";
				}
			}
			text = Program.sdk.FilterDirtyWords(text);
			Game1.multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, text, farmer.UniqueMultiplayerID);
			receiveChatMessage(Game1.player.UniqueMultiplayerID, 3, LocalizedContentManager.CurrentLanguageCode, text);
		}

		public bool isActive()
		{
			return chatBox.Selected;
		}

		public void activate()
		{
			chatBox.Selected = true;
			setText("");
			updatePosition();
		}

		public override void clickAway()
		{
			base.clickAway();
			if (isActive() && isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
			{
				_justShownKeyboard = true;
				chatBox.Update();
				return;
			}
			if (_justShownKeyboard)
			{
				Game.Activity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)2;
				_justShownKeyboard = false;
			}
			else
			{
				chatBox.HideStatusBar();
			}
			if (!choosingEmoji || !emojiMenu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()) || Game1.input.GetKeyboardState().IsKeyDown(Keys.Escape))
			{
				bool selected = chatBox.Selected;
				chatBox.Selected = false;
				choosingEmoji = false;
				setText("");
				cheatHistoryPosition = -1;
				if (selected)
				{
					Game1.oldKBState = Game1.GetKeyboardState();
				}
			}
		}

		public override bool isWithinBounds(int x, int y)
		{
			if (x - xPositionOnScreen >= width || x - xPositionOnScreen < 0 || y - yPositionOnScreen >= height || y - yPositionOnScreen < -getOldMessagesBoxHeight())
			{
				if (choosingEmoji)
				{
					return emojiMenu.isWithinBounds(x, y);
				}
				return false;
			}
			return true;
		}

		public virtual void setText(string text)
		{
			chatBox.setText(text);
		}

		public override void receiveKeyPress(Keys key)
		{
			switch (key)
			{
			case Keys.Up:
				if (cheatHistoryPosition < cheatHistory.Count - 1)
				{
					cheatHistoryPosition++;
					string text2 = cheatHistory[cheatHistoryPosition];
					chatBox.setText(text2);
				}
				break;
			case Keys.Down:
				if (cheatHistoryPosition > 0)
				{
					cheatHistoryPosition--;
					string text = cheatHistory[cheatHistoryPosition];
					chatBox.setText(text);
				}
				break;
			}
			if (!Game1.options.doesInputListContain(Game1.options.moveUpButton, key) && !Game1.options.doesInputListContain(Game1.options.moveRightButton, key) && !Game1.options.doesInputListContain(Game1.options.moveDownButton, key) && !Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
			{
				base.receiveKeyPress(key);
			}
		}

		public override bool readyToClose()
		{
			return false;
		}

		public override void receiveGamePadButton(Buttons b)
		{
		}

		public bool isHoveringOverClickable(int x, int y)
		{
			if (emojiMenuIcon.containsPoint(x, y) || (choosingEmoji && emojiMenu.isWithinBounds(x, y)))
			{
				return true;
			}
			return false;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.activeClickableMenu == null && chatBox.Selected)
			{
				chatBox.Update();
				if (choosingEmoji)
				{
					choosingEmoji = false;
					emojiMenuIcon.scale = 4f;
				}
				if (isWithinBounds(x, y))
				{
					chatBox.Selected = true;
				}
			}
		}

		public static string formattedUserName(Farmer farmer)
		{
			string text = farmer.Name;
			if (text == null || text.Trim() == "")
			{
				text = Game1.content.LoadString("Strings\\UI:Chat_PlayerJoinedNewName");
			}
			return text;
		}

		public static string formattedUserNameLong(Farmer farmer)
		{
			string sub = formattedUserName(farmer);
			return Game1.content.LoadString("Strings\\UI:Chat_PlayerName", sub, Game1.multiplayer.getUserName(farmer.UniqueMultiplayerID));
		}

		private string formatMessage(long sourceFarmer, int chatKind, string message)
		{
			string sub = Game1.content.LoadString("Strings\\UI:Chat_UnknownUserName");
			Farmer farmer = null;
			if (sourceFarmer == Game1.player.UniqueMultiplayerID)
			{
				farmer = Game1.player;
			}
			if (Game1.otherFarmers.ContainsKey(sourceFarmer))
			{
				farmer = Game1.otherFarmers[sourceFarmer];
			}
			if (farmer != null)
			{
				sub = formattedUserName(farmer);
			}
			return chatKind switch
			{
				0 => Game1.content.LoadString("Strings\\UI:Chat_ChatMessageFormat", sub, message), 
				2 => Game1.content.LoadString("Strings\\UI:Chat_UserNotificationMessageFormat", message), 
				3 => Game1.content.LoadString("Strings\\UI:Chat_PrivateMessageFormat", sub, message), 
				_ => Game1.content.LoadString("Strings\\UI:Chat_ErrorMessageFormat", message), 
			};
		}

		protected virtual Color messageColor(int chatKind)
		{
			return chatKind switch
			{
				0 => chatBox.TextColor, 
				3 => Color.DarkCyan, 
				2 => Color.Yellow, 
				_ => Color.Red, 
			};
		}

		public virtual void receiveChatMessage(long sourceFarmer, int chatKind, LocalizedContentManager.LanguageCode language, string message)
		{
			string text = formatMessage(sourceFarmer, chatKind, message);
			ChatMessage chatMessage = new ChatMessage();
			string text2 = Game1.parseText(text, chatBox.Font, chatBox.Width - 16);
			chatMessage.timeLeftToDisplay = 600;
			chatMessage.verticalSize = (int)chatBox.Font.MeasureString(text2).Y + 4;
			chatMessage.color = messageColor(chatKind);
			chatMessage.language = language;
			chatMessage.parseMessageForEmoji(text2);
			messages.Add(chatMessage);
			if (messages.Count > maxMessages)
			{
				messages.RemoveAt(0);
			}
			if (chatKind == 3 && sourceFarmer != Game1.player.UniqueMultiplayerID)
			{
				lastReceivedPrivateMessagePlayerId = sourceFarmer;
			}
		}

		public virtual void addMessage(string message, Color color)
		{
			ChatMessage chatMessage = new ChatMessage();
			string text = Game1.parseText(message, chatBox.Font, chatBox.Width - 8);
			chatMessage.timeLeftToDisplay = 600;
			chatMessage.verticalSize = (int)chatBox.Font.MeasureString(text).Y + 4;
			chatMessage.color = color;
			chatMessage.language = LocalizedContentManager.CurrentLanguageCode;
			chatMessage.parseMessageForEmoji(text);
			messages.Add(chatMessage);
			if (messages.Count > maxMessages)
			{
				messages.RemoveAt(0);
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			x = Game1.input.GetMouseState().X;
			y = Game1.input.GetMouseState().Y;
			emojiMenuIcon.tryHover(x, y, 1f);
			emojiMenuIcon.tryHover(x, y, 1f);
		}

		public override void update(GameTime time)
		{
			KeyboardState keyboardState = Game1.input.GetKeyboardState();
			Keys[] pressedKeys = keyboardState.GetPressedKeys();
			foreach (Keys key in pressedKeys)
			{
				if (!oldKBState.IsKeyDown(key))
				{
					receiveKeyPress(key);
				}
			}
			oldKBState = keyboardState;
			for (int j = 0; j < messages.Count; j++)
			{
				if (messages[j].timeLeftToDisplay > 0)
				{
					messages[j].timeLeftToDisplay--;
				}
				if (messages[j].timeLeftToDisplay < 75)
				{
					messages[j].alpha = (float)messages[j].timeLeftToDisplay / 75f;
				}
			}
			if (!chatBox.Selected)
			{
				return;
			}
			foreach (ChatMessage message in messages)
			{
				message.alpha = 1f;
			}
		}

		public override void receiveScrollWheelAction(int direction)
		{
			if (choosingEmoji)
			{
				emojiMenu.receiveScrollWheelAction(direction);
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
		}

		public static SpriteFont messageFont(LocalizedContentManager.LanguageCode language)
		{
			return Game1.content.Load<SpriteFont>("Fonts\\SmallFont", language);
		}

		public int getOldMessagesBoxHeight()
		{
			int num = 20;
			for (int num2 = messages.Count - 1; num2 >= 0; num2--)
			{
				ChatMessage chatMessage = messages[num2];
				if (chatBox.Selected || chatMessage.alpha > 0.01f)
				{
					num += chatMessage.verticalSize;
				}
			}
			return num;
		}

		public override void draw(SpriteBatch b)
		{
			int num = 0;
			bool flag = false;
			for (int num2 = messages.Count - 1; num2 >= 0; num2--)
			{
				ChatMessage chatMessage = messages[num2];
				if (chatBox.Selected || chatMessage.alpha > 0.01f)
				{
					num += chatMessage.verticalSize;
					flag = true;
				}
			}
			if (flag)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(301, 288, 15, 15), xPositionOnScreen, yPositionOnScreen - num - 20 + ((!chatBox.Selected) ? chatBox.Height : 0), chatBox.Width, num + 20, Color.White, 4f, drawShadow: false);
			}
			num = 0;
			for (int num3 = messages.Count - 1; num3 >= 0; num3--)
			{
				ChatMessage chatMessage2 = messages[num3];
				num += chatMessage2.verticalSize;
				chatMessage2.draw(b, xPositionOnScreen + 12, yPositionOnScreen - num - 8 + ((!chatBox.Selected) ? chatBox.Height : 0));
			}
			if (chatBox.Selected)
			{
				chatBox.Draw(b, drawShadow: false);
				if (isWithinBounds(Game1.getMouseX(), Game1.getMouseY()) && !Game1.options.hardwareCursor)
				{
					Game1.mouseCursor = (Game1.options.gamepadControls ? 44 : 0);
				}
			}
		}
	}
}
