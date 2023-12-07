using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Minigames;

namespace StardewValley.Network
{
	public class GameServer : IGameServer, IBandwidthMonitor
	{
		protected List<Server> servers = new List<Server>();

		private Dictionary<Action, Func<bool>> pendingGameAvailableActions = new Dictionary<Action, Func<bool>>();

		protected Dictionary<string, Action> _pendingFarmhandSelections = new Dictionary<string, Action>();

		private List<Action> completedPendingActions = new List<Action>();

		private List<string> bannedUsers = new List<string>();

		protected bool _wasConnected;

		protected bool _isLocalMultiplayerInitiatedServer;

		public int connectionsCount => servers.Sum((Server s) => s.connectionsCount);

		public BandwidthLogger BandwidthLogger
		{
			get
			{
				foreach (Server server in servers)
				{
					if (server.connectionsCount > 0)
					{
						return server.BandwidthLogger;
					}
				}
				return null;
			}
		}

		public bool LogBandwidth
		{
			get
			{
				foreach (Server server in servers)
				{
					if (server.connectionsCount > 0)
					{
						return server.LogBandwidth;
					}
				}
				return false;
			}
			set
			{
				foreach (Server server in servers)
				{
					if (server.connectionsCount > 0)
					{
						server.LogBandwidth = value;
						break;
					}
				}
			}
		}

		public GameServer(bool local_multiplayer = false)
		{
			_isLocalMultiplayerInitiatedServer = local_multiplayer;
			if (!_isLocalMultiplayerInitiatedServer && Program.sdk.Networking != null)
			{
				servers.Add(Program.sdk.Networking.CreateServer(this));
			}
		}

		public bool isConnectionActive(string connectionId)
		{
			foreach (Server server in servers)
			{
				if (server.isConnectionActive(connectionId))
				{
					return true;
				}
			}
			return false;
		}

		public virtual void onConnect(string connectionID)
		{
			UpdateLocalOnlyFlag();
		}

		public virtual void onDisconnect(string connectionID)
		{
			if (_pendingFarmhandSelections.ContainsKey(connectionID))
			{
				Console.WriteLine("Removed pending farmhand selection for invalidated connection " + connectionID);
				if (pendingGameAvailableActions.ContainsKey(_pendingFarmhandSelections[connectionID]))
				{
					pendingGameAvailableActions.Remove(_pendingFarmhandSelections[connectionID]);
				}
				_pendingFarmhandSelections.Remove(connectionID);
			}
			UpdateLocalOnlyFlag();
		}

		public bool IsLocalMultiplayerInitiatedServer()
		{
			return _isLocalMultiplayerInitiatedServer;
		}

		public virtual void UpdateLocalOnlyFlag()
		{
			if (!Game1.game1.IsMainInstance)
			{
				return;
			}
			bool flag = IsLocalMultiplayerInitiatedServer();
			if (Game1.hasLocalClientsOnly != flag)
			{
				Game1.hasLocalClientsOnly = flag;
				if (Game1.hasLocalClientsOnly)
				{
					Console.WriteLine("Game has only local clients.");
				}
				else
				{
					Console.WriteLine("Game has remote clients.");
				}
			}
		}

		public string getInviteCode()
		{
			foreach (Server server in servers)
			{
				string inviteCode = server.getInviteCode();
				if (inviteCode != null)
				{
					return inviteCode;
				}
			}
			return null;
		}

		public string getUserName(long farmerId)
		{
			foreach (Server server in servers)
			{
				string userName = server.getUserName(farmerId);
				if (userName != null)
				{
					return userName;
				}
			}
			return null;
		}

		public float getPingToClient(long farmerId)
		{
			foreach (Server server in servers)
			{
				if (server.getPingToClient(farmerId) != -1f)
				{
					return server.getPingToClient(farmerId);
				}
			}
			return -1f;
		}

		protected void initialize()
		{
			foreach (Server server in servers)
			{
				server.initialize();
			}
			whenGameAvailable(updateLobbyData);
		}

		public void setPrivacy(ServerPrivacy privacy)
		{
			foreach (Server server in servers)
			{
				server.setPrivacy(privacy);
			}
			if (Game1.netWorldState != null && Game1.netWorldState.Value != null)
			{
				Game1.netWorldState.Value.ServerPrivacy = privacy;
			}
		}

		public void stopServer()
		{
			if (Game1.chatBox != null)
			{
				Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_DisablingServer"));
			}
			foreach (Server server in servers)
			{
				server.stopServer();
			}
		}

		public void receiveMessages()
		{
			foreach (Server server in servers)
			{
				server.receiveMessages();
			}
			completedPendingActions.Clear();
			foreach (KeyValuePair<Action, Func<bool>> pendingGameAvailableAction in pendingGameAvailableActions)
			{
				Action key = pendingGameAvailableAction.Key;
				if (pendingGameAvailableActions[key]())
				{
					key();
					completedPendingActions.Add(key);
				}
			}
			foreach (Action completedPendingAction in completedPendingActions)
			{
				pendingGameAvailableActions.Remove(completedPendingAction);
			}
			completedPendingActions.Clear();
			if (Game1.chatBox == null)
			{
				return;
			}
			bool flag = anyServerConnected();
			if (_wasConnected != flag)
			{
				_wasConnected = flag;
				if (_wasConnected)
				{
					Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_StartingServer"));
				}
			}
		}

		public void sendMessage(long peerId, OutgoingMessage message)
		{
			foreach (Server server in servers)
			{
				server.sendMessage(peerId, message);
			}
		}

		public bool canAcceptIPConnections()
		{
			return servers.Select((Server s) => s.canAcceptIPConnections()).Aggregate(seed: false, (bool a, bool b) => a || b);
		}

		public bool canOfferInvite()
		{
			return servers.Select((Server s) => s.canOfferInvite()).Aggregate(seed: false, (bool a, bool b) => a || b);
		}

		public void offerInvite()
		{
			foreach (Server server in servers)
			{
				if (server.canOfferInvite())
				{
					server.offerInvite();
				}
			}
		}

		public bool anyServerConnected()
		{
			foreach (Server server in servers)
			{
				if (server.connected())
				{
					return true;
				}
			}
			return false;
		}

		public bool connected()
		{
			foreach (Server server in servers)
			{
				if (!server.connected())
				{
					return false;
				}
			}
			return true;
		}

		public void sendMessage(long peerId, byte messageType, Farmer sourceFarmer, params object[] data)
		{
			sendMessage(peerId, new OutgoingMessage(messageType, sourceFarmer, data));
		}

		public void sendMessages()
		{
			foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
			{
				Farmer value = otherFarmer.Value;
				foreach (OutgoingMessage item in value.messageQueue)
				{
					sendMessage(value.UniqueMultiplayerID, item);
				}
				value.messageQueue.Clear();
			}
		}

		public void startServer()
		{
			_wasConnected = false;
			Console.WriteLine("Starting server. Protocol version: 1.5.5");
			initialize();
			if (Game1.netWorldState == null)
			{
				Game1.netWorldState = new NetRoot<IWorldState>(new NetWorldState());
			}
			Game1.netWorldState.Clock.InterpolationTicks = 0;
			Game1.netWorldState.Value.UpdateFromGame1();
		}

		public void initializeHost()
		{
			if (Game1.serverHost == null)
			{
				Game1.serverHost = new NetFarmerRoot();
			}
			Game1.serverHost.Value = Game1.player;
			foreach (Server server in servers)
			{
				if (server.PopulatePlatformData(Game1.player))
				{
					break;
				}
			}
			Game1.serverHost.MarkClean();
			Game1.serverHost.Clock.InterpolationTicks = Game1.multiplayer.defaultInterpolationTicks;
		}

		public void sendServerIntroduction(long peer)
		{
			sendMessage(peer, new OutgoingMessage(1, Game1.serverHost.Value, Game1.multiplayer.writeObjectFullBytes(Game1.serverHost, peer), Game1.multiplayer.writeObjectFullBytes(Game1.player.teamRoot, peer), Game1.multiplayer.writeObjectFullBytes(Game1.netWorldState, peer)));
			foreach (KeyValuePair<long, NetRoot<Farmer>> root in Game1.otherFarmers.Roots)
			{
				if (root.Key != Game1.player.UniqueMultiplayerID && root.Key != peer)
				{
					sendMessage(peer, new OutgoingMessage(2, root.Value.Value, getUserName(root.Value.Value.UniqueMultiplayerID), Game1.multiplayer.writeObjectFullBytes(root.Value, peer)));
				}
			}
		}

		public void kick(long disconnectee)
		{
			foreach (Server server in servers)
			{
				server.kick(disconnectee);
			}
		}

		public string ban(long farmerId)
		{
			string text = null;
			foreach (Server server in servers)
			{
				text = server.getUserId(farmerId);
				if (text != null)
				{
					break;
				}
			}
			if (text != null && !Game1.bannedUsers.ContainsKey(text))
			{
				string text2 = Game1.multiplayer.getUserName(farmerId);
				if (text2 == "" || text2 == text)
				{
					text2 = null;
				}
				Game1.bannedUsers.Add(text, text2);
				kick(farmerId);
				return text;
			}
			return null;
		}

		public void playerDisconnected(long disconnectee)
		{
			Farmer value = null;
			Game1.otherFarmers.TryGetValue(disconnectee, out value);
			Game1.multiplayer.playerDisconnected(disconnectee);
			if (value == null)
			{
				return;
			}
			OutgoingMessage message = new OutgoingMessage(19, value);
			foreach (long key in Game1.otherFarmers.Keys)
			{
				if (key != disconnectee)
				{
					sendMessage(key, message);
				}
			}
		}

		public bool isGameAvailable()
		{
			bool flag = Game1.currentMinigame is Intro || Game1.Date.DayOfMonth == 0;
			bool flag2 = Game1.CurrentEvent != null && Game1.CurrentEvent.isWedding;
			bool flag3 = Game1.newDaySync != null && !Game1.newDaySync.hasFinished();
			bool flag4 = Game1.player.team.demolishLock.IsLocked();
			bool flag5 = Game1.gameMode == 6;
			if (!Game1.isFestival() && !flag2 && !flag && !flag3 && !flag4 && !flag5)
			{
				return Game1.weddingsToday.Count == 0;
			}
			return false;
		}

		public bool whenGameAvailable(Action action, Func<bool> customAvailabilityCheck = null)
		{
			Func<bool> func = ((customAvailabilityCheck != null) ? customAvailabilityCheck : new Func<bool>(isGameAvailable));
			if (func())
			{
				action();
				return true;
			}
			pendingGameAvailableActions.Add(action, func);
			return false;
		}

		private void rejectFarmhandRequest(string userID, NetFarmerRoot farmer, Action<OutgoingMessage> sendMessage)
		{
			sendAvailableFarmhands(userID, sendMessage);
			Console.WriteLine("Rejected request for farmhand " + ((farmer.Value != null) ? farmer.Value.UniqueMultiplayerID.ToString() : "???"));
		}

		private IEnumerable<Cabin> cabins()
		{
			Farm farm = Game1.getFarm();
			if (farm == null)
			{
				yield break;
			}
			NetCollection<Building> buildings = farm.buildings;
			foreach (Building item in buildings)
			{
				if ((int)item.daysOfConstructionLeft <= 0 && item.indoors.Value is Cabin cabin)
				{
					yield return cabin;
				}
			}
		}

		public bool isUserBanned(string userID)
		{
			return Game1.bannedUsers.ContainsKey(userID);
		}

		private bool authCheck(string userID, Farmer farmhand)
		{
			if (!Game1.options.enableFarmhandCreation && !IsLocalMultiplayerInitiatedServer() && !farmhand.isCustomized)
			{
				return false;
			}
			if (!(userID == "") && !(farmhand.userID.Value == ""))
			{
				return farmhand.userID.Value == userID;
			}
			return true;
		}

		private Cabin findCabin(Farmer farmhand)
		{
			foreach (Cabin item in cabins())
			{
				if (item.getFarmhand().Value.UniqueMultiplayerID == farmhand.UniqueMultiplayerID)
				{
					return item;
				}
			}
			return null;
		}

		private Farmer findOriginalFarmhand(Farmer farmhand)
		{
			return findCabin(farmhand)?.getFarmhand().Value;
		}

		public void checkFarmhandRequest(string userID, string connectionID, NetFarmerRoot farmer, Action<OutgoingMessage> sendMessage, Action approve)
		{
			if (farmer.Value == null)
			{
				rejectFarmhandRequest(userID, farmer, sendMessage);
				return;
			}
			long id = farmer.Value.UniqueMultiplayerID;
			Action action = delegate
			{
				if (_pendingFarmhandSelections.ContainsKey(connectionID))
				{
					_pendingFarmhandSelections.Remove(connectionID);
				}
				Farmer farmer2 = findOriginalFarmhand(farmer.Value);
				if (!isConnectionActive(connectionID))
				{
					Console.WriteLine("Rejected request for connection ID " + connectionID + ": Connection not active.");
				}
				else if (farmer2 == null)
				{
					Console.WriteLine("Rejected request for farmhand " + id + ": doesn't exist");
					rejectFarmhandRequest(userID, farmer, sendMessage);
				}
				else if (!authCheck(userID, farmer2))
				{
					Console.WriteLine("Rejected request for farmhand " + id + ": authorization failure " + userID + " " + farmer2.userID.Value);
					rejectFarmhandRequest(userID, farmer, sendMessage);
				}
				else if ((Game1.otherFarmers.ContainsKey(id) && !Game1.multiplayer.isDisconnecting(id)) || Game1.serverHost.Value.UniqueMultiplayerID == id)
				{
					Console.WriteLine("Rejected request for farmhand " + id + ": already in use");
					rejectFarmhandRequest(userID, farmer, sendMessage);
				}
				else
				{
					Cabin cabin = findCabin(farmer.Value);
					if (cabin.isInventoryOpen())
					{
						Console.WriteLine("Rejected request for farmhand " + id + ": inventory in use");
						rejectFarmhandRequest(userID, farmer, sendMessage);
					}
					else
					{
						Console.WriteLine("Approved request for farmhand " + id);
						approve();
						Game1.updateCellarAssignments();
						Game1.multiplayer.addPlayer(farmer);
						Game1.multiplayer.broadcastPlayerIntroduction(farmer);
						sendLocation(id, Game1.getFarm());
						sendLocation(id, Game1.getLocationFromName("FarmHouse"));
						sendLocation(id, Game1.getLocationFromName("Greenhouse"));
						if (farmer.Value.lastSleepLocation != null)
						{
							GameLocation locationFromName = Game1.getLocationFromName(farmer.Value.lastSleepLocation);
							if (locationFromName != null && Game1.isLocationAccessible(locationFromName.Name) && !Game1.multiplayer.isAlwaysActiveLocation(locationFromName))
							{
								sendLocation(id, locationFromName, force_current: true);
							}
						}
						sendServerIntroduction(id);
						updateLobbyData();
					}
				}
			};
			if (!whenGameAvailable(action))
			{
				_pendingFarmhandSelections[connectionID] = action;
				Console.WriteLine("Postponing request for farmhand " + id + " from connection: " + connectionID);
				sendMessage(new OutgoingMessage(11, Game1.player, "Strings\\UI:Client_WaitForHostAvailability"));
			}
		}

		public void sendAvailableFarmhands(string userID, Action<OutgoingMessage> sendMessage)
		{
			List<NetRef<Farmer>> list = new List<NetRef<Farmer>>();
			Farm farm = Game1.getFarm();
			foreach (Cabin item in cabins())
			{
				NetRef<Farmer> farmhand = item.getFarmhand();
				if ((!farmhand.Value.isActive() || Game1.multiplayer.isDisconnecting(farmhand.Value.UniqueMultiplayerID)) && !item.isInventoryOpen())
				{
					list.Add(farmhand);
				}
			}
			using MemoryStream memoryStream = new MemoryStream();
			using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(Game1.year);
			binaryWriter.Write(Utility.getSeasonNumber(Game1.currentSeason));
			binaryWriter.Write(Game1.dayOfMonth);
			binaryWriter.Write((byte)list.Count);
			foreach (NetRef<Farmer> item2 in list)
			{
				try
				{
					item2.Serializer = SaveGame.farmerSerializer;
					item2.WriteFull(binaryWriter);
				}
				finally
				{
					item2.Serializer = null;
				}
			}
			memoryStream.Seek(0L, SeekOrigin.Begin);
			sendMessage(new OutgoingMessage(9, Game1.player, memoryStream.ToArray()));
		}

		public T GetServer<T>() where T : Server
		{
			foreach (Server server in servers)
			{
				if (server is T)
				{
					return server as T;
				}
			}
			return null;
		}

		private void sendLocation(long peer, GameLocation location, bool force_current = false)
		{
			sendMessage(peer, 3, Game1.serverHost.Value, force_current, Game1.multiplayer.writeObjectFullBytes(Game1.multiplayer.locationRoot(location), peer));
		}

		private void warpFarmer(Farmer farmer, short x, short y, string name, bool isStructure)
		{
			GameLocation locationFromName = Game1.getLocationFromName(name, isStructure);
			if (Game1.IsMasterGame)
			{
				locationFromName.hostSetup();
			}
			farmer.currentLocation = locationFromName;
			farmer.Position = new Vector2(x * 64, y * 64 - (farmer.Sprite.getHeight() - 32) + 16);
			sendLocation(farmer.UniqueMultiplayerID, locationFromName);
		}

		public void processIncomingMessage(IncomingMessage message)
		{
			switch (message.MessageType)
			{
			case 5:
			{
				short x = message.Reader.ReadInt16();
				short y = message.Reader.ReadInt16();
				string name = message.Reader.ReadString();
				bool isStructure = message.Reader.ReadByte() == 1;
				warpFarmer(message.SourceFarmer, x, y, name, isStructure);
				break;
			}
			case 2:
				message.Reader.ReadString();
				Game1.multiplayer.processIncomingMessage(message);
				break;
			case 10:
			{
				long num = message.Reader.ReadInt64();
				message.Reader.BaseStream.Position -= 8L;
				if (num == Multiplayer.AllPlayers || num == Game1.player.UniqueMultiplayerID)
				{
					Game1.multiplayer.processIncomingMessage(message);
				}
				rebroadcastClientMessage(message, num);
				break;
			}
			default:
				Game1.multiplayer.processIncomingMessage(message);
				break;
			}
			if (Game1.multiplayer.isClientBroadcastType(message.MessageType))
			{
				rebroadcastClientMessage(message, Multiplayer.AllPlayers);
			}
		}

		private void rebroadcastClientMessage(IncomingMessage message, long peerID)
		{
			OutgoingMessage message2 = new OutgoingMessage(message);
			foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
			{
				long key = otherFarmer.Key;
				if (key != message.FarmerID && (peerID == Multiplayer.AllPlayers || key == peerID))
				{
					sendMessage(key, message2);
				}
			}
		}

		private void setLobbyData(string key, string value)
		{
			foreach (Server server in servers)
			{
				server.setLobbyData(key, value);
			}
		}

		private bool unclaimedFarmhandsExist()
		{
			foreach (Cabin item in cabins())
			{
				if (item.farmhand.Value == null)
				{
					return true;
				}
				if (item.farmhand.Value.userID.Value == "")
				{
					return true;
				}
			}
			return false;
		}

		public void updateLobbyData()
		{
			setLobbyData("farmName", Game1.player.farmName.Value);
			setLobbyData("farmType", Convert.ToString(Game1.whichFarm));
			if (Game1.whichFarm == 7)
			{
				setLobbyData("modFarmType", Game1.GetFarmTypeID());
			}
			else
			{
				setLobbyData("modFarmType", "");
			}
			WorldDate worldDate = new WorldDate(Game1.year, Game1.currentSeason, Game1.dayOfMonth);
			setLobbyData("date", Convert.ToString(worldDate.TotalDays));
			IEnumerable<string> source = from farmhand in Game1.getAllFarmhands()
				select farmhand.userID.Value;
			setLobbyData("farmhands", string.Join(",", source.Where((string user) => user != "")));
			setLobbyData("newFarmhands", Convert.ToString(Game1.options.enableFarmhandCreation && unclaimedFarmhandsExist()));
		}
	}
}
