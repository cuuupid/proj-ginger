using System.Collections.Generic;
using System.IO;

namespace StardewValley.Network
{
	public class LoopbackClient : Client
	{
		private static uint _nextClientId;

		private readonly string _clientId;

		private LoopbackServer _host;

		private readonly BinaryReader _packetReader;

		private readonly BinaryWriter _packetWriter;

		private readonly MemoryStream _packetStream;

		private readonly List<IncomingMessage> _incoming = new List<IncomingMessage>();

		public string id => _clientId;

		public bool isConnected => _host != null;

		public LoopbackClient()
		{
			_clientId = "player_" + ++_nextClientId;
			_packetStream = new MemoryStream(1048576);
			_packetReader = new BinaryReader(_packetStream);
			_packetWriter = new BinaryWriter(_packetStream);
		}

		public override string getUserID()
		{
			return "";
		}

		public override float GetPingToHost()
		{
			return 0f;
		}

		protected override string getHostUserName()
		{
			return "host";
		}

		protected override void connectImpl()
		{
			_host = LoopbackServer.Instance;
			_host.clientConnect(this);
		}

		public override void disconnect(bool neatly = true)
		{
			if (_host != null)
			{
				if (neatly)
				{
					sendMessage(new OutgoingMessage(19, Game1.player));
				}
				_host.clientDisconnect(this);
				_host = null;
			}
			_incoming.Clear();
			connectionMessage = null;
		}

		protected virtual bool validateProtocol(string version)
		{
			return version == "1.5.5";
		}

		protected override void receiveMessagesImpl()
		{
			foreach (IncomingMessage item in _incoming)
			{
				processIncomingMessage(item);
			}
			_incoming.Clear();
		}

		public override void sendMessage(OutgoingMessage message)
		{
			if (_host != null)
			{
				_host.clientMessage(this, message);
			}
		}

		public void serverMessage(OutgoingMessage message)
		{
			_packetStream.Position = 0L;
			message.Write(_packetWriter);
			_packetWriter.Flush();
			_packetStream.Position = 0L;
			IncomingMessage incomingMessage = new IncomingMessage();
			incomingMessage.Read(_packetReader);
			_incoming.Add(incomingMessage);
		}

		public void serverDisconnect()
		{
			pendingDisconnect = Multiplayer.DisconnectType.LidgrenTimeout;
			_incoming.Clear();
			_host = null;
			connectionMessage = null;
		}

		public void serverKicked()
		{
			pendingDisconnect = Multiplayer.DisconnectType.Kicked;
			disconnect(neatly: false);
		}
	}
}
