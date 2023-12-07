using System;
using System.Collections.Generic;
using System.IO;

namespace StardewValley.Network
{
	public class LoopbackServer : Server
	{
		private struct Packet
		{
			public LoopbackClient client;

			public IncomingMessage message;
		}

		private bool _connected;

		private readonly List<Packet> _incoming = new List<Packet>();

		private readonly BinaryReader _packetReader;

		private readonly BinaryWriter _packetWriter;

		private readonly MemoryStream _packetStream;

		private readonly List<LoopbackClient> _connecting = new List<LoopbackClient>();

		private readonly List<LoopbackClient> _connections = new List<LoopbackClient>();

		private readonly List<LoopbackClient> _disconnections = new List<LoopbackClient>();

		protected readonly Bimap<long, LoopbackClient> peers = new Bimap<long, LoopbackClient>();

		public override int connectionsCount => _connections.Count;

		public static LoopbackServer Instance { get; private set; }

		public LoopbackServer(IGameServer gameServer)
			: base(gameServer)
		{
			Instance = this;
			_packetStream = new MemoryStream(1048576);
			_packetReader = new BinaryReader(_packetStream);
			_packetWriter = new BinaryWriter(_packetStream);
		}

		public override bool isConnectionActive(string connectionID)
		{
			foreach (LoopbackClient connection in _connections)
			{
				if (connection.id == connectionID && connection.isConnected)
				{
					return true;
				}
			}
			return false;
		}

		public override string getUserId(long farmerId)
		{
			if (!peers.ContainsLeft(farmerId))
			{
				return null;
			}
			return peers[farmerId].id;
		}

		public override bool hasUserId(string userId)
		{
			foreach (LoopbackClient rightValue in peers.RightValues)
			{
				if (rightValue.id.Equals(userId))
				{
					return true;
				}
			}
			return false;
		}

		public override string getUserName(long farmerId)
		{
			if (!peers.ContainsLeft(farmerId))
			{
				return null;
			}
			LoopbackClient loopbackClient = peers[farmerId];
			return loopbackClient.id;
		}

		public override float getPingToClient(long farmerId)
		{
			if (!peers.ContainsLeft(farmerId))
			{
				return -1f;
			}
			return 0f;
		}

		public override void setPrivacy(ServerPrivacy privacy)
		{
		}

		public override bool canAcceptIPConnections()
		{
			return true;
		}

		public override bool connected()
		{
			return _connected;
		}

		public override void initialize()
		{
			_connected = true;
		}

		public override void stopServer()
		{
			foreach (LoopbackClient connection in _connections)
			{
				connection.serverDisconnect();
			}
			_incoming.Clear();
			_connections.Clear();
			_connecting.Clear();
			_disconnections.Clear();
			peers.Clear();
			_connected = false;
		}

		public override void receiveMessages()
		{
			if (!_connected)
			{
				return;
			}
			foreach (Packet item in _incoming)
			{
				parseDataMessageFromClient(item.client, item.message);
			}
			_incoming.Clear();
			foreach (LoopbackClient conn in _connecting)
			{
				_connections.Add(conn);
				onConnect(conn.id);
				if (!gameServer.whenGameAvailable(delegate
				{
					gameServer.sendAvailableFarmhands("", delegate(OutgoingMessage msg)
					{
						sendMessage(conn, msg);
					});
				}, () => Game1.gameMode != 6))
				{
					Console.WriteLine("Postponing introduction message");
					sendMessage(conn, new OutgoingMessage(11, Game1.player, "Strings\\UI:Client_WaitForHostLoad"));
				}
			}
			_connecting.Clear();
			foreach (LoopbackClient disconnection in _disconnections)
			{
				if (!peers.ContainsRight(disconnection))
				{
					onDisconnect(disconnection.id);
					_connections.Remove(disconnection);
					_connecting.Remove(disconnection);
				}
				else
				{
					long left = peers.GetLeft(disconnection);
					playerDisconnected(left);
				}
			}
			_disconnections.Clear();
		}

		public override void kick(long disconnectee)
		{
			base.kick(disconnectee);
			if (peers.ContainsLeft(disconnectee))
			{
				peers[disconnectee].serverKicked();
				playerDisconnected(disconnectee);
			}
		}

		public override void playerDisconnected(long disconnectee)
		{
			base.playerDisconnected(disconnectee);
			LoopbackClient loopbackClient = peers[disconnectee];
			peers.RemoveLeft(disconnectee);
			onDisconnect(loopbackClient.id);
			_connecting.Remove(loopbackClient);
			_connections.Remove(loopbackClient);
		}

		private void parseDataMessageFromClient(LoopbackClient peer, IncomingMessage message)
		{
			if (peers.ContainsLeft(message.FarmerID) && peers[message.FarmerID] == peer)
			{
				gameServer.processIncomingMessage(message);
			}
			else if (message.MessageType == 2)
			{
				NetFarmerRoot farmer = Game1.multiplayer.readFarmer(message.Reader);
				gameServer.checkFarmhandRequest("", peer.id, farmer, delegate(OutgoingMessage msg)
				{
					sendMessage(peer, msg);
				}, delegate
				{
					peers[farmer.Value.UniqueMultiplayerID] = peer;
				});
			}
		}

		public override void sendMessage(long peerId, OutgoingMessage message)
		{
			if (_connected && peers.ContainsLeft(peerId))
			{
				sendMessage(peers[peerId], message);
			}
		}

		private void sendMessage(LoopbackClient connection, OutgoingMessage message)
		{
			connection.serverMessage(message);
		}

		public override void setLobbyData(string key, string value)
		{
		}

		public void clientConnect(LoopbackClient client)
		{
			_connecting.Add(client);
		}

		public void clientMessage(LoopbackClient client, OutgoingMessage message)
		{
			_packetStream.Position = 0L;
			message.Write(_packetWriter);
			_packetWriter.Flush();
			_packetStream.Position = 0L;
			IncomingMessage incomingMessage = new IncomingMessage();
			incomingMessage.Read(_packetReader);
			Packet item = default(Packet);
			item.client = client;
			item.message = incomingMessage;
			_incoming.Add(item);
		}

		public void clientDisconnect(LoopbackClient client)
		{
			if (!_disconnections.Contains(client))
			{
				_disconnections.Add(client);
			}
		}
	}
}
