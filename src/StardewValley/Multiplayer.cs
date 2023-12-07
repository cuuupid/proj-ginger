using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

namespace StardewValley
{
	public class Multiplayer
	{
		public enum PartyWideMessageQueue
		{
			MailForTomorrow,
			SeenMail
		}

		[StructLayout(0, Size = 1)]
		private struct FarmerRoots : IEnumerable<NetFarmerRoot>, IEnumerable
		{
			public struct Enumerator : IEnumerator<NetFarmerRoot>, IDisposable, IEnumerator
			{
				private Dictionary<long, NetRoot<Farmer>>.Enumerator _enumerator;

				private NetFarmerRoot _current;

				private int _step;

				private bool _done;

				public NetFarmerRoot Current => _current;

				object IEnumerator.Current
				{
					get
					{
						if (_done)
						{
							throw new InvalidOperationException();
						}
						return _current;
					}
				}

				public Enumerator(bool dummy)
				{
					_enumerator = Game1.otherFarmers.Roots.GetEnumerator();
					_current = null;
					_step = 0;
					_done = false;
				}

				public bool MoveNext()
				{
					if (_step == 0)
					{
						_step++;
						if (Game1.serverHost != null)
						{
							_current = Game1.serverHost;
							return true;
						}
					}
					while (_enumerator.MoveNext())
					{
						NetRoot<Farmer> value = _enumerator.Current.Value;
						if (Game1.serverHost == null || value != Game1.serverHost)
						{
							_current = value as NetFarmerRoot;
							return true;
						}
					}
					_done = true;
					_current = null;
					return false;
				}

				public void Dispose()
				{
				}

				void IEnumerator.Reset()
				{
					_enumerator = Game1.otherFarmers.Roots.GetEnumerator();
					_current = null;
					_step = 0;
					_done = false;
				}
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(dummy: true);
			}

			IEnumerator<NetFarmerRoot> IEnumerable<NetFarmerRoot>.GetEnumerator()
			{
				return new Enumerator(dummy: true);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new Enumerator(dummy: true);
			}
		}

		[StructLayout(0, Size = 1)]
		public struct ActiveLocations : IEnumerable<GameLocation>, IEnumerable
		{
			public struct Enumerator : IEnumerator<GameLocation>, IDisposable, IEnumerator
			{
				private List<Building>.Enumerator _enumerator;

				private GameLocation _current;

				private int _step;

				private bool _done;

				public GameLocation Current => _current;

				object IEnumerator.Current
				{
					get
					{
						if (_done)
						{
							throw new InvalidOperationException();
						}
						return _current;
					}
				}

				public bool MoveNext()
				{
					if (_step == 0)
					{
						_step++;
						if (Game1.currentLocation != null)
						{
							_current = Game1.currentLocation;
							return true;
						}
					}
					if (_step == 1)
					{
						_step++;
						Farm farm = Game1.getFarm();
						if (farm != null && farm != Game1.currentLocation)
						{
							_current = farm;
							return true;
						}
					}
					if (_step == 2)
					{
						_step++;
						GameLocation locationFromName = Game1.getLocationFromName("FarmHouse");
						if (locationFromName != null && locationFromName != Game1.currentLocation)
						{
							_current = locationFromName;
							return true;
						}
					}
					if (_step == 3)
					{
						_step++;
						GameLocation locationFromName2 = Game1.getLocationFromName("Greenhouse");
						if (locationFromName2 != null && locationFromName2 != Game1.currentLocation)
						{
							_current = locationFromName2;
							return true;
						}
					}
					if (_step == 4)
					{
						_step++;
						Farm farm2 = Game1.getFarm();
						_enumerator = farm2.buildings.GetEnumerator();
					}
					while (_enumerator.MoveNext())
					{
						Building current = _enumerator.Current;
						GameLocation value = current.indoors.Value;
						if (value != null && value != Game1.currentLocation)
						{
							_current = value;
							return true;
						}
					}
					_done = true;
					_current = null;
					return false;
				}

				public void Dispose()
				{
				}

				void IEnumerator.Reset()
				{
					_current = null;
					_step = 0;
					_done = false;
				}
			}

			public Enumerator GetEnumerator()
			{
				return default(Enumerator);
			}

			IEnumerator<GameLocation> IEnumerable<GameLocation>.GetEnumerator()
			{
				return default(Enumerator);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return default(Enumerator);
			}
		}

		public enum DisconnectType
		{
			None,
			ClosedGame,
			ExitedToMainMenu,
			ExitedToMainMenu_FromFarmhandSelect,
			HostLeft,
			ServerOfflineMode,
			ServerFull,
			Kicked,
			AcceptedOtherInvite,
			ClientTimeout,
			LidgrenTimeout,
			GalaxyTimeout,
			Timeout_FarmhandSelection,
			LidgrenDisconnect_Unknown
		}

		public static readonly long AllPlayers = 0L;

		public const byte farmerDelta = 0;

		public const byte serverIntroduction = 1;

		public const byte playerIntroduction = 2;

		public const byte locationIntroduction = 3;

		public const byte forceEvent = 4;

		public const byte warpFarmer = 5;

		public const byte locationDelta = 6;

		public const byte locationSprites = 7;

		public const byte characterWarp = 8;

		public const byte availableFarmhands = 9;

		public const byte chatMessage = 10;

		public const byte connectionMessage = 11;

		public const byte worldDelta = 12;

		public const byte teamDelta = 13;

		public const byte newDaySync = 14;

		public const byte chatInfoMessage = 15;

		public const byte userNameUpdate = 16;

		public const byte farmerGainExperience = 17;

		public const byte serverToClientsMessage = 18;

		public const byte disconnecting = 19;

		public const byte sharedAchievement = 20;

		public const byte globalMessage = 21;

		public const byte partyWideMail = 22;

		public const byte forceKick = 23;

		public const byte removeLocationFromLookup = 24;

		public const byte farmerKilledMonster = 25;

		public const byte requestGrandpaReevaluation = 26;

		public const byte digBuriedNut = 27;

		public const byte requestPassout = 28;

		public const byte passout = 29;

		public int defaultInterpolationTicks = 15;

		public int farmerDeltaBroadcastPeriod = 3;

		public int locationDeltaBroadcastPeriod = 3;

		public int worldStateDeltaBroadcastPeriod = 3;

		public int playerLimit = 4;

		public static string kicked = "KICKED";

		public const string protocolVersion = "1.5.5";

		public readonly NetLogger logging = new NetLogger();

		protected List<long> disconnectingFarmers = new List<long>();

		public ulong latestID;

		public Dictionary<string, CachedMultiplayerMap> cachedMultiplayerMaps = new Dictionary<string, CachedMultiplayerMap>();

		public const string MSG_START_FESTIVAL_EVENT = "festivalEvent";

		public const string MSG_END_FESTIVAL = "endFest";

		public const string MSG_TRAIN_APPROACH = "trainApproach";

		public const string MSG_PLACEHOLDER = "[replace me]";

		public virtual int MaxPlayers
		{
			get
			{
				if (Game1.server == null)
				{
					return 1;
				}
				return playerLimit;
			}
		}

		public virtual long getNewID()
		{
			ulong num = ((latestID & 0xFF) + 1) & 0xFF;
			ulong uniqueMultiplayerID = (ulong)Game1.player.UniqueMultiplayerID;
			uniqueMultiplayerID = (uniqueMultiplayerID >> 32) ^ (uniqueMultiplayerID & 0xFFFFFFFFu);
			uniqueMultiplayerID = ((uniqueMultiplayerID >> 16) ^ (uniqueMultiplayerID & 0xFFFF)) & 0xFFFF;
			ulong num2 = (ulong)(DateTime.Now.Ticks / 10000);
			latestID = (num2 << 24) | (uniqueMultiplayerID << 8) | num;
			return (long)latestID;
		}

		public virtual bool isDisconnecting(Farmer farmer)
		{
			return isDisconnecting(farmer.UniqueMultiplayerID);
		}

		public virtual bool isDisconnecting(long uid)
		{
			return disconnectingFarmers.Contains(uid);
		}

		public virtual bool isClientBroadcastType(byte messageType)
		{
			switch (messageType)
			{
			case 0:
			case 2:
			case 4:
			case 6:
			case 7:
			case 12:
			case 13:
			case 14:
			case 15:
			case 19:
			case 20:
			case 21:
			case 22:
			case 24:
			case 26:
				return true;
			default:
				return false;
			}
		}

		public virtual bool allowSyncDelay()
		{
			return Game1.newDaySync == null;
		}

		public virtual int interpolationTicks()
		{
			if (!allowSyncDelay())
			{
				return 0;
			}
			if (LocalMultiplayer.IsLocalMultiplayer(is_local_only: true))
			{
				return 4;
			}
			return defaultInterpolationTicks;
		}

		private FarmerRoots farmerRoots()
		{
			return default(FarmerRoots);
		}

		public virtual NetFarmerRoot farmerRoot(long id)
		{
			if (Game1.serverHost != null && id == Game1.serverHost.Value.UniqueMultiplayerID)
			{
				return Game1.serverHost;
			}
			if (Game1.otherFarmers.ContainsKey(id))
			{
				return Game1.otherFarmers.Roots[id] as NetFarmerRoot;
			}
			return null;
		}

		public virtual void broadcastFarmerDeltas()
		{
			foreach (NetFarmerRoot item in farmerRoots())
			{
				if (item.Dirty && Game1.player.UniqueMultiplayerID == item.Value.UniqueMultiplayerID)
				{
					broadcastFarmerDelta(item.Value, writeObjectDeltaBytes(item));
				}
			}
			if (Game1.player.teamRoot.Dirty)
			{
				broadcastTeamDelta(writeObjectDeltaBytes(Game1.player.teamRoot));
			}
		}

		protected virtual void broadcastTeamDelta(byte[] delta)
		{
			if (Game1.IsServer)
			{
				foreach (Farmer value in Game1.otherFarmers.Values)
				{
					if (value != Game1.player)
					{
						Game1.server.sendMessage(value.UniqueMultiplayerID, 13, Game1.player, delta);
					}
				}
				return;
			}
			if (Game1.IsClient && Game1.client != null)
			{
				Game1.client.sendMessage(13, delta);
			}
		}

		protected virtual void broadcastFarmerDelta(Farmer farmer, byte[] delta)
		{
			foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
			{
				if (otherFarmer.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
				{
					otherFarmer.Value.queueMessage(0, farmer, farmer.UniqueMultiplayerID, delta);
				}
			}
		}

		public void updateRoot<T>(T root) where T : INetRoot
		{
			foreach (long disconnectingFarmer in disconnectingFarmers)
			{
				root.Disconnect(disconnectingFarmer);
			}
			root.TickTree();
		}

		public void updateRoots()
		{
			updateRoot(Game1.netWorldState);
			foreach (NetFarmerRoot item in farmerRoots())
			{
				item.Clock.InterpolationTicks = interpolationTicks();
				updateRoot(item);
			}
			Game1.player.teamRoot.Clock.InterpolationTicks = interpolationTicks();
			updateRoot(Game1.player.teamRoot);
			if (Game1.IsClient)
			{
				foreach (GameLocation item2 in activeLocations())
				{
					if (item2.Root != null && item2.Root.Value == item2)
					{
						item2.Root.Clock.InterpolationTicks = interpolationTicks();
						updateRoot(item2.Root);
					}
				}
				return;
			}
			foreach (GameLocation location in Game1.locations)
			{
				if (location.Root != null)
				{
					location.Root.Clock.InterpolationTicks = interpolationTicks();
					updateRoot(location.Root);
				}
			}
			foreach (MineShaft activeMine in MineShaft.activeMines)
			{
				if (activeMine.Root != null)
				{
					activeMine.Root.Clock.InterpolationTicks = interpolationTicks();
					updateRoot(activeMine.Root);
				}
			}
			foreach (VolcanoDungeon activeLevel in VolcanoDungeon.activeLevels)
			{
				if (activeLevel.Root != null)
				{
					activeLevel.Root.Clock.InterpolationTicks = interpolationTicks();
					updateRoot(activeLevel.Root);
				}
			}
		}

		public virtual void broadcastLocationDeltas()
		{
			if (Game1.IsClient)
			{
				foreach (GameLocation item in activeLocations())
				{
					if (!(item.Root == null) && item.Root.Dirty)
					{
						broadcastLocationDelta(item);
					}
				}
				return;
			}
			foreach (GameLocation location in Game1.locations)
			{
				if (location.Root != null && location.Root.Dirty)
				{
					broadcastLocationDelta(location);
				}
			}
			foreach (MineShaft activeMine in MineShaft.activeMines)
			{
				if (activeMine.Root != null && activeMine.Root.Dirty)
				{
					broadcastLocationDelta(activeMine);
				}
			}
			foreach (VolcanoDungeon activeLevel in VolcanoDungeon.activeLevels)
			{
				if (activeLevel.Root != null && activeLevel.Root.Dirty)
				{
					broadcastLocationDelta(activeLevel);
				}
			}
		}

		public virtual void broadcastLocationDelta(GameLocation loc)
		{
			if (!(loc.Root == null) && loc.Root.Dirty)
			{
				byte[] bytes = writeObjectDeltaBytes(loc.Root);
				broadcastLocationBytes(loc, 6, bytes);
			}
		}

		protected virtual void broadcastLocationBytes(GameLocation loc, byte messageType, byte[] bytes)
		{
			OutgoingMessage message = new OutgoingMessage(messageType, Game1.player, loc.isStructure.Value, loc.isStructure ? loc.uniqueName.Value : loc.name.Value, bytes);
			broadcastLocationMessage(loc, message);
		}

		protected virtual void broadcastLocationMessage(GameLocation loc, OutgoingMessage message)
		{
			if (Game1.IsClient)
			{
				if (Game1.client != null)
				{
					Game1.client.sendMessage(message);
				}
				return;
			}
			Action<Farmer> action = delegate(Farmer f)
			{
				if (f != Game1.player)
				{
					Game1.server.sendMessage(f.UniqueMultiplayerID, message);
				}
			};
			if (isAlwaysActiveLocation(loc))
			{
				foreach (Farmer value in Game1.otherFarmers.Values)
				{
					action(value);
				}
				return;
			}
			foreach (Farmer farmer in loc.farmers)
			{
				action(farmer);
			}
			if (!(loc is BuildableGameLocation))
			{
				return;
			}
			foreach (Building building in (loc as BuildableGameLocation).buildings)
			{
				if (building.indoors.Value == null)
				{
					continue;
				}
				foreach (Farmer farmer2 in building.indoors.Value.farmers)
				{
					action(farmer2);
				}
			}
		}

		public virtual void broadcastSprites(GameLocation location, List<TemporaryAnimatedSprite> sprites)
		{
			broadcastSprites(location, sprites.ToArray());
		}

		public virtual void broadcastSprites(GameLocation location, params TemporaryAnimatedSprite[] sprites)
		{
			location.temporarySprites.AddRange(sprites);
			if (sprites.Length == 0 || !Game1.IsMultiplayer)
			{
				return;
			}
			using MemoryStream memoryStream = new MemoryStream();
			using (BinaryWriter binaryWriter = createWriter(memoryStream))
			{
				binaryWriter.Push("TemporaryAnimatedSprites");
				binaryWriter.Write(sprites.Length);
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in sprites)
				{
					temporaryAnimatedSprite.Write(binaryWriter, location);
				}
				binaryWriter.Pop();
			}
			broadcastLocationBytes(location, 7, memoryStream.ToArray());
		}

		public virtual void broadcastWorldStateDeltas()
		{
			if (!Game1.netWorldState.Dirty)
			{
				return;
			}
			byte[] array = writeObjectDeltaBytes(Game1.netWorldState);
			foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
			{
				if (otherFarmer.Value != Game1.player)
				{
					otherFarmer.Value.queueMessage(12, Game1.player, array);
				}
			}
		}

		public virtual void receiveWorldState(BinaryReader msg)
		{
			Game1.netWorldState.Clock.InterpolationTicks = 0;
			readObjectDelta(msg, Game1.netWorldState);
			Game1.netWorldState.TickTree();
			int timeOfDay = Game1.timeOfDay;
			Game1.netWorldState.Value.WriteToGame1();
			if (!Game1.IsServer && timeOfDay != Game1.timeOfDay && Game1.currentLocation != null && Game1.newDaySync == null)
			{
				Game1.performTenMinuteClockUpdate();
			}
		}

		public virtual void requestCharacterWarp(NPC character, GameLocation targetLocation, Vector2 position)
		{
			if (Game1.IsClient)
			{
				GameLocation currentLocation = character.currentLocation;
				if (currentLocation == null)
				{
					throw new ArgumentException("In warpCharacter, the character's currentLocation must not be null");
				}
				Guid guid = currentLocation.characters.GuidOf(character);
				if (guid == Guid.Empty)
				{
					throw new ArgumentException("In warpCharacter, the character must be in its currentLocation");
				}
				OutgoingMessage message = new OutgoingMessage(8, Game1.player, currentLocation.isStructure.Value, currentLocation.isStructure ? currentLocation.uniqueName.Value : currentLocation.name.Value, guid, targetLocation.isStructure.Value, targetLocation.isStructure ? targetLocation.uniqueName.Value : targetLocation.name.Value, position);
				Game1.serverHost.Value.queueMessage(message);
			}
		}

		public virtual NetRoot<GameLocation> locationRoot(GameLocation location)
		{
			if (location.Root == null && Game1.IsMasterGame)
			{
				new NetRoot<GameLocation>().Set(location);
				location.Root.Clock.InterpolationTicks = interpolationTicks();
				location.Root.MarkClean();
			}
			return location.Root;
		}

		public virtual void sendPassoutRequest()
		{
			object[] data = new object[1] { Game1.player.UniqueMultiplayerID };
			if (Game1.IsMasterGame)
			{
				_receivePassoutRequest(Game1.player);
			}
			else
			{
				Game1.client.sendMessage(28, data);
			}
		}

		public virtual void receivePassoutRequest(IncomingMessage msg)
		{
			if (Game1.IsServer)
			{
				long id = msg.Reader.ReadInt64();
				Farmer farmer = Game1.getFarmer(id);
				if (farmer != null)
				{
					_receivePassoutRequest(farmer);
				}
			}
		}

		protected virtual void _receivePassoutRequest(Farmer farmer)
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (farmer.lastSleepLocation.Value != null && Game1.isLocationAccessible(farmer.lastSleepLocation) && Game1.getLocationFromName(farmer.lastSleepLocation) != null && Game1.getLocationFromName(farmer.lastSleepLocation).GetLocationContext() == farmer.currentLocation.GetLocationContext() && BedFurniture.IsBedHere(Game1.getLocationFromName(farmer.lastSleepLocation), farmer.lastSleepPoint.Value.X, farmer.lastSleepPoint.Value.Y))
			{
				if (Game1.IsServer && farmer != Game1.player)
				{
					object[] source = new object[4]
					{
						farmer.lastSleepLocation.Value,
						farmer.lastSleepPoint.X,
						farmer.lastSleepPoint.Y,
						true
					};
					Game1.server.sendMessage(farmer.UniqueMultiplayerID, 29, Game1.player, source.ToArray());
				}
				else
				{
					Farmer.performPassoutWarp(farmer, farmer.lastSleepLocation, farmer.lastSleepPoint, has_bed: true);
				}
				return;
			}
			string nameOrUniqueName = Utility.getHomeOfFarmer(farmer).NameOrUniqueName;
			Point bed_point = Utility.getHomeOfFarmer(farmer).GetPlayerBedSpot();
			bool flag = Utility.getHomeOfFarmer(farmer).GetPlayerBed() != null;
			if (farmer.currentLocation.GetLocationContext() == GameLocation.LocationContext.Island && Game1.getLocationFromName("IslandWest") is IslandWest islandWest && islandWest.farmhouseRestored.Value && Game1.getLocationFromName("IslandFarmHouse") is IslandFarmHouse islandFarmHouse)
			{
				nameOrUniqueName = islandFarmHouse.NameOrUniqueName;
				bed_point = new Point(14, 17);
				flag = false;
				foreach (Furniture item in islandFarmHouse.furniture)
				{
					if (item is BedFurniture && (item as BedFurniture).bedType != BedFurniture.BedType.Child)
					{
						bed_point = (item as BedFurniture).GetBedSpot();
						flag = true;
						break;
					}
				}
			}
			if (Game1.IsServer && farmer != Game1.player)
			{
				object[] source2 = new object[4] { nameOrUniqueName, bed_point.X, bed_point.Y, flag };
				Game1.server.sendMessage(farmer.UniqueMultiplayerID, 29, Game1.player, source2.ToArray());
			}
			else
			{
				Farmer.performPassoutWarp(farmer, nameOrUniqueName, bed_point, flag);
			}
		}

		public virtual void receivePassout(IncomingMessage msg)
		{
			if (msg.SourceFarmer == Game1.serverHost.Value)
			{
				string bed_location_name = msg.Reader.ReadString();
				Point bed_point = new Point(msg.Reader.ReadInt32(), msg.Reader.ReadInt32());
				bool has_bed = msg.Reader.ReadBoolean();
				Farmer.performPassoutWarp(Game1.player, bed_location_name, bed_point, has_bed);
			}
		}

		public virtual void broadcastEvent(Event evt, GameLocation location, Vector2 positionBeforeEvent, bool use_local_farmer = true)
		{
			if (evt.id == -1)
			{
				return;
			}
			object[] data = new object[6]
			{
				evt.id,
				use_local_farmer,
				(int)positionBeforeEvent.X,
				(int)positionBeforeEvent.Y,
				(byte)(location.isStructure ? 1u : 0u),
				location.isStructure ? location.uniqueName.Value : location.Name
			};
			if (Game1.IsServer)
			{
				foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
				{
					if (otherFarmer.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
					{
						Game1.server.sendMessage(otherFarmer.Value.UniqueMultiplayerID, 4, Game1.player, data);
					}
				}
				return;
			}
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(4, data);
			}
		}

		protected virtual void receiveRequestGrandpaReevaluation(IncomingMessage msg)
		{
			Game1.getFarm()?.requestGrandpaReevaluation();
		}

		protected virtual void receiveFarmerKilledMonster(IncomingMessage msg)
		{
			if (msg.SourceFarmer == Game1.serverHost.Value)
			{
				string text = msg.Reader.ReadString();
				if (text != null)
				{
					Game1.stats.monsterKilled(text);
				}
			}
		}

		public virtual void broadcastRemoveLocationFromLookup(GameLocation location)
		{
			List<object> list = new List<object>();
			list.Add(location.NameOrUniqueName);
			if (Game1.IsServer)
			{
				foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
				{
					if (otherFarmer.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
					{
						Game1.server.sendMessage(otherFarmer.Value.UniqueMultiplayerID, 24, Game1.player, list.ToArray());
					}
				}
				return;
			}
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(24, list.ToArray());
			}
		}

		public virtual void broadcastNutDig(GameLocation location, Point point)
		{
			if (Game1.IsMasterGame)
			{
				_performNutDig(location, point);
				return;
			}
			List<object> list = new List<object>();
			list.Add(location.NameOrUniqueName);
			list.Add(point.X);
			list.Add(point.Y);
			Game1.client.sendMessage(27, list.ToArray());
		}

		protected virtual void receiveNutDig(IncomingMessage msg)
		{
			if (Game1.IsMasterGame)
			{
				string name = msg.Reader.ReadString();
				Point point = new Point(msg.Reader.ReadInt32(), msg.Reader.ReadInt32());
				GameLocation locationFromName = Game1.getLocationFromName(name);
				_performNutDig(locationFromName, point);
			}
		}

		protected virtual void _performNutDig(GameLocation location, Point point)
		{
			if (!(location is IslandLocation))
			{
				return;
			}
			IslandLocation islandLocation = location as IslandLocation;
			if (islandLocation.IsBuriedNutLocation(point))
			{
				string key = location.NameOrUniqueName + "_" + point.X + "_" + point.Y;
				if (!Game1.netWorldState.Value.FoundBuriedNuts.ContainsKey(key))
				{
					Game1.netWorldState.Value.FoundBuriedNuts[key] = true;
					Game1.createItemDebris(new Object(73, 1), new Vector2(point.X, point.Y) * 64f, -1, islandLocation);
				}
			}
		}

		public virtual void broadcastPartyWideMail(string mail_key, PartyWideMessageQueue message_queue = PartyWideMessageQueue.MailForTomorrow, bool no_letter = false)
		{
			mail_key = mail_key.Trim();
			mail_key = mail_key.Replace(Environment.NewLine, "");
			List<object> list = new List<object>();
			list.Add(mail_key);
			list.Add((int)message_queue);
			list.Add(no_letter);
			_performPartyWideMail(mail_key, message_queue, no_letter);
			if (Game1.IsServer)
			{
				foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
				{
					if (otherFarmer.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
					{
						Game1.server.sendMessage(otherFarmer.Value.UniqueMultiplayerID, 22, Game1.player, list.ToArray());
					}
				}
				return;
			}
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(22, list.ToArray());
			}
		}

		public virtual void broadcastGrandpaReevaluation()
		{
			Farm farm = Game1.getFarm();
			farm.requestGrandpaReevaluation();
			if (Game1.IsServer)
			{
				foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
				{
					if (otherFarmer.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
					{
						Game1.server.sendMessage(otherFarmer.Value.UniqueMultiplayerID, 26, Game1.player);
					}
				}
				return;
			}
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(26);
			}
		}

		public virtual void broadcastGlobalMessage(string localization_string_key, bool only_show_if_empty = false, params string[] substitutions)
		{
			if (!only_show_if_empty || Game1.hudMessages.Count == 0)
			{
				Game1.showGlobalMessage(Game1.content.LoadString(localization_string_key, substitutions));
			}
			List<object> list = new List<object>();
			list.Add(localization_string_key);
			list.Add(only_show_if_empty);
			list.Add(substitutions.Length);
			for (int i = 0; i < substitutions.Length; i++)
			{
				list.Add(substitutions[i]);
			}
			if (Game1.IsServer)
			{
				foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
				{
					if (otherFarmer.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
					{
						Game1.server.sendMessage(otherFarmer.Value.UniqueMultiplayerID, 21, Game1.player, list.ToArray());
					}
				}
				return;
			}
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(21, list.ToArray());
			}
		}

		public virtual NetRoot<T> readObjectFull<T>(BinaryReader reader) where T : class, INetObject<INetSerializable>
		{
			NetRoot<T> netRoot = NetRoot<T>.Connect(reader);
			netRoot.Clock.InterpolationTicks = defaultInterpolationTicks;
			return netRoot;
		}

		protected virtual BinaryWriter createWriter(Stream stream)
		{
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			if (logging.IsLogging)
			{
				binaryWriter = new LoggingBinaryWriter(binaryWriter);
			}
			return binaryWriter;
		}

		public virtual void writeObjectFull<T>(BinaryWriter writer, NetRoot<T> root, long? peer) where T : class, INetObject<INetSerializable>
		{
			root.CreateConnectionPacket(writer, peer);
		}

		public virtual byte[] writeObjectFullBytes<T>(NetRoot<T> root, long? peer) where T : class, INetObject<INetSerializable>
		{
			using MemoryStream memoryStream = new MemoryStream();
			using BinaryWriter writer = createWriter(memoryStream);
			root.CreateConnectionPacket(writer, peer);
			return memoryStream.ToArray();
		}

		public virtual void readObjectDelta<T>(BinaryReader reader, NetRoot<T> root) where T : class, INetObject<INetSerializable>
		{
			root.Read(reader);
		}

		public virtual void writeObjectDelta<T>(BinaryWriter writer, NetRoot<T> root) where T : class, INetObject<INetSerializable>
		{
			root.Write(writer);
		}

		public virtual byte[] writeObjectDeltaBytes<T>(NetRoot<T> root) where T : class, INetObject<INetSerializable>
		{
			using MemoryStream memoryStream = new MemoryStream();
			using BinaryWriter writer = createWriter(memoryStream);
			root.Write(writer);
			return memoryStream.ToArray();
		}

		public virtual NetFarmerRoot readFarmer(BinaryReader reader)
		{
			NetFarmerRoot netFarmerRoot = new NetFarmerRoot();
			netFarmerRoot.ReadConnectionPacket(reader);
			netFarmerRoot.Clock.InterpolationTicks = defaultInterpolationTicks;
			return netFarmerRoot;
		}

		public virtual void addPlayer(NetFarmerRoot f)
		{
			long uniqueMultiplayerID = f.Value.UniqueMultiplayerID;
			f.Value.teamRoot = Game1.player.teamRoot;
			Game1.otherFarmers.Roots[uniqueMultiplayerID] = f;
			disconnectingFarmers.Remove(uniqueMultiplayerID);
			if (Game1.chatBox != null)
			{
				string sub = ChatBox.formattedUserNameLong(f.Value);
				Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_PlayerJoined", sub));
			}
		}

		public virtual void receivePlayerIntroduction(BinaryReader reader)
		{
			addPlayer(readFarmer(reader));
		}

		public virtual void broadcastPlayerIntroduction(NetFarmerRoot farmerRoot)
		{
			if (Game1.server == null)
			{
				return;
			}
			foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
			{
				if (farmerRoot.Value.UniqueMultiplayerID != otherFarmer.Value.UniqueMultiplayerID)
				{
					Game1.server.sendMessage(otherFarmer.Value.UniqueMultiplayerID, 2, farmerRoot.Value, Game1.server.getUserName(farmerRoot.Value.UniqueMultiplayerID), writeObjectFullBytes(farmerRoot, otherFarmer.Value.UniqueMultiplayerID));
				}
			}
		}

		public virtual void broadcastUserName(long farmerId, string userName)
		{
			if (Game1.server != null)
			{
				return;
			}
			foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
			{
				Farmer value = otherFarmer.Value;
				if (value.UniqueMultiplayerID != farmerId)
				{
					Game1.server.sendMessage(value.UniqueMultiplayerID, 16, Game1.serverHost.Value, farmerId, userName);
				}
			}
		}

		public virtual string getUserName(long id)
		{
			if (id == Game1.player.UniqueMultiplayerID)
			{
				return Game1.content.LoadString("Strings\\UI:Chat_SelfPlayerID");
			}
			if (Game1.server != null)
			{
				return Game1.server.getUserName(id);
			}
			if (Game1.client != null)
			{
				return Game1.client.getUserName(id);
			}
			return "?";
		}

		public virtual void playerDisconnected(long id)
		{
			if (Game1.otherFarmers.ContainsKey(id) && !disconnectingFarmers.Contains(id))
			{
				NetFarmerRoot netFarmerRoot = Game1.otherFarmers.Roots[id] as NetFarmerRoot;
				if (netFarmerRoot.Value.mount != null && Game1.IsMasterGame)
				{
					netFarmerRoot.Value.mount.dismount();
				}
				if (Game1.IsMasterGame)
				{
					saveFarmhand(netFarmerRoot);
					netFarmerRoot.Value.handleDisconnect();
				}
				if (Game1.player.dancePartner.Value is Farmer && ((Farmer)Game1.player.dancePartner.Value).UniqueMultiplayerID == netFarmerRoot.Value.UniqueMultiplayerID)
				{
					Game1.player.dancePartner.Value = null;
				}
				if (Game1.chatBox != null)
				{
					Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_PlayerLeft", ChatBox.formattedUserNameLong(Game1.otherFarmers[id])));
				}
				disconnectingFarmers.Add(id);
			}
		}

		protected virtual void removeDisconnectedFarmers()
		{
			foreach (long disconnectingFarmer in disconnectingFarmers)
			{
				Game1.otherFarmers.Remove(disconnectingFarmer);
			}
			disconnectingFarmers.Clear();
		}

		public virtual void sendFarmhand()
		{
			(Game1.player.NetFields.Root as NetFarmerRoot).MarkReassigned();
		}

		protected virtual void saveFarmhand(NetFarmerRoot farmhand)
		{
			FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(farmhand);
			if (homeOfFarmer is Cabin)
			{
				Cabin cabin = homeOfFarmer as Cabin;
				cabin.saveFarmhand(farmhand);
			}
		}

		public virtual void saveFarmhands()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			foreach (NetRoot<Farmer> value in Game1.otherFarmers.Roots.Values)
			{
				saveFarmhand(value as NetFarmerRoot);
			}
		}

		public virtual void clientRemotelyDisconnected(DisconnectType disconnectType)
		{
			LogDisconnect(disconnectType);
			returnToMainMenu();
		}

		private void returnToMainMenu()
		{
			if (!Game1.game1.IsMainInstance)
			{
				GameRunner.instance.RemoveGameInstance(Game1.game1);
				return;
			}
			Game1.ExitToTitle(delegate
			{
				if (Game1.activeClickableMenu is TitleMenu titleMenu)
				{
					titleMenu.skipToTitleButtons();
					TitleMenu.subMenu = new ConfirmationDialog(Game1.content.LoadString("Strings\\UI:Client_RemotelyDisconnected"), null)
					{
						okButton = 
						{
							visible = false
						}
					};
				}
			});
		}

		public static bool ShouldLogDisconnect(DisconnectType disconnectType)
		{
			switch (disconnectType)
			{
			case DisconnectType.ClosedGame:
			case DisconnectType.ExitedToMainMenu:
			case DisconnectType.ExitedToMainMenu_FromFarmhandSelect:
			case DisconnectType.ServerOfflineMode:
			case DisconnectType.ServerFull:
			case DisconnectType.AcceptedOtherInvite:
				return false;
			default:
				return true;
			}
		}

		public static bool IsTimeout(DisconnectType disconnectType)
		{
			if ((uint)(disconnectType - 9) <= 2u)
			{
				return true;
			}
			return false;
		}

		public static void LogDisconnect(DisconnectType disconnectType)
		{
			ShouldLogDisconnect(disconnectType);
			Console.WriteLine("Disconnected: " + disconnectType);
		}

		public virtual void sendSharedAchievementMessage(int achievement)
		{
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(20, achievement);
			}
			else
			{
				if (!Game1.IsServer)
				{
					return;
				}
				foreach (long key in Game1.otherFarmers.Keys)
				{
					Game1.server.sendMessage(key, 20, Game1.player, achievement);
				}
			}
		}

		public virtual void sendServerToClientsMessage(string message)
		{
			if (!Game1.IsServer)
			{
				return;
			}
			foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
			{
				otherFarmer.Value.queueMessage(18, Game1.player, message);
			}
		}

		public virtual void sendChatMessage(LocalizedContentManager.LanguageCode language, string message, long recipientID)
		{
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(10, recipientID, language, message);
			}
			else
			{
				if (!Game1.IsServer)
				{
					return;
				}
				if (recipientID == AllPlayers)
				{
					foreach (long key in Game1.otherFarmers.Keys)
					{
						Game1.server.sendMessage(key, 10, Game1.player, recipientID, language, message);
					}
					return;
				}
				Game1.server.sendMessage(recipientID, 10, Game1.player, recipientID, language, message);
			}
		}

		public virtual void receiveChatMessage(Farmer sourceFarmer, long recipientID, LocalizedContentManager.LanguageCode language, string message)
		{
			if (Game1.chatBox != null)
			{
				int chatKind = 0;
				message = Program.sdk.FilterDirtyWords(message);
				if (recipientID != AllPlayers)
				{
					chatKind = 3;
				}
				Game1.chatBox.receiveChatMessage(sourceFarmer.UniqueMultiplayerID, chatKind, language, message);
			}
		}

		public virtual void globalChatInfoMessage(string messageKey, params string[] args)
		{
			if (Game1.IsMultiplayer || Game1.multiplayerMode != 0)
			{
				receiveChatInfoMessage(Game1.player, messageKey, args);
				sendChatInfoMessage(messageKey, args);
			}
		}

		public void globalChatInfoMessageEvenInSinglePlayer(string messageKey, params string[] args)
		{
			receiveChatInfoMessage(Game1.player, messageKey, args);
			sendChatInfoMessage(messageKey, args);
		}

		protected virtual void sendChatInfoMessage(string messageKey, params string[] args)
		{
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(15, messageKey, args);
			}
			else
			{
				if (!Game1.IsServer)
				{
					return;
				}
				foreach (long key in Game1.otherFarmers.Keys)
				{
					Game1.server.sendMessage(key, 15, Game1.player, messageKey, args);
				}
			}
		}

		protected virtual void receiveChatInfoMessage(Farmer sourceFarmer, string messageKey, string[] args)
		{
			if (Game1.chatBox == null)
			{
				return;
			}
			try
			{
				string[] array = args.Select(delegate(string arg)
				{
					if (arg.StartsWith("achievement:"))
					{
						int key = Convert.ToInt32(arg.Substring("achievement:".Length));
						Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\Achievements");
						return dictionary[key].Split('^')[0];
					}
					if (arg.StartsWith("object:"))
					{
						int parentSheetIndex = Convert.ToInt32(arg.Substring("object:".Length));
						Object @object = new Object(parentSheetIndex, 1);
						return @object.DisplayName;
					}
					return arg;
				}).ToArray();
				ChatBox chatBox = Game1.chatBox;
				LocalizedContentManager content = Game1.content;
				string path = "Strings\\UI:Chat_" + messageKey;
				object[] substitutions = array;
				chatBox.addInfoMessage(content.LoadString(path, substitutions));
			}
			catch (ContentLoadException)
			{
			}
			catch (FormatException)
			{
			}
			catch (OverflowException)
			{
			}
			catch (KeyNotFoundException)
			{
			}
		}

		public virtual void parseServerToClientsMessage(string message)
		{
			if (!Game1.IsClient)
			{
				return;
			}
			switch (message)
			{
			case "festivalEvent":
				if (Game1.currentLocation.currentEvent != null)
				{
					Game1.currentLocation.currentEvent.forceFestivalContinue();
				}
				break;
			case "endFest":
				if (Game1.CurrentEvent != null)
				{
					Game1.CurrentEvent.forceEndFestival(Game1.player);
				}
				break;
			case "trainApproach":
			{
				GameLocation locationFromName = Game1.getLocationFromName("Railroad");
				if (locationFromName != null && locationFromName is Railroad)
				{
					((Railroad)locationFromName).PlayTrainApproach();
				}
				break;
			}
			}
		}

		public ActiveLocations activeLocations()
		{
			return default(ActiveLocations);
		}

		public virtual bool isAlwaysActiveLocation(GameLocation location)
		{
			if (!(location.Name == "Farm") && !(location.Name == "FarmHouse") && !(location.Name == "Greenhouse"))
			{
				if (location.Root != null)
				{
					return location.Root.Value.Equals(Game1.getFarm());
				}
				return false;
			}
			return true;
		}

		protected virtual void readActiveLocation(IncomingMessage msg)
		{
			bool flag = msg.Reader.ReadBoolean();
			NetRoot<GameLocation> netRoot = readObjectFull<GameLocation>(msg.Reader);
			if (isAlwaysActiveLocation(netRoot.Value))
			{
				for (int i = 0; i < Game1.locations.Count; i++)
				{
					if (!Game1.locations[i].Equals(netRoot.Value))
					{
						continue;
					}
					if (Game1.locations[i] == netRoot.Value)
					{
						break;
					}
					if (Game1.locations[i] != null)
					{
						if (Game1.currentLocation == Game1.locations[i])
						{
							Game1.currentLocation = netRoot.Value;
						}
						if (Game1.player.currentLocation == Game1.locations[i])
						{
							Game1.player.currentLocation = netRoot.Value;
						}
						Game1.removeLocationFromLocationLookup(Game1.locations[i]);
					}
					Game1.locations[i] = netRoot.Value;
					break;
				}
			}
			if (!(Game1.locationRequest != null || flag))
			{
				return;
			}
			if (Game1.locationRequest != null)
			{
				Game1.currentLocation = Game1.findStructure(netRoot.Value, Game1.locationRequest.Name);
				if (Game1.currentLocation == null)
				{
					Game1.currentLocation = netRoot.Value;
				}
			}
			else if (flag)
			{
				Game1.currentLocation = netRoot.Value;
			}
			if (Game1.locationRequest != null)
			{
				Game1.locationRequest.Location = netRoot.Value;
				Game1.locationRequest.Loaded(netRoot.Value);
			}
			Game1.currentLocation.resetForPlayerEntry();
			Game1.player.currentLocation = Game1.currentLocation;
			if (Game1.locationRequest != null)
			{
				Game1.locationRequest.Warped(netRoot.Value);
			}
			Game1.currentLocation.updateSeasonalTileSheets();
			if (Game1.IsDebrisWeatherHere())
			{
				Game1.populateDebrisWeatherArray(Game1.debrisWeather);
			}
			Game1.locationRequest = null;
		}

		public virtual bool isActiveLocation(GameLocation location)
		{
			if (Game1.IsMasterGame)
			{
				return true;
			}
			if (Game1.currentLocation != null && Game1.currentLocation.Root != null && Game1.currentLocation.Root.Value == location.Root.Value)
			{
				return true;
			}
			if (isAlwaysActiveLocation(location))
			{
				return true;
			}
			return false;
		}

		protected virtual GameLocation readLocation(BinaryReader reader)
		{
			bool isStructure = reader.ReadByte() != 0;
			string name = reader.ReadString();
			GameLocation locationFromName = Game1.getLocationFromName(name, isStructure);
			if (locationFromName == null || locationRoot(locationFromName) == null)
			{
				return null;
			}
			if (!isActiveLocation(locationFromName))
			{
				return null;
			}
			return locationFromName;
		}

		protected virtual LocationRequest readLocationRequest(BinaryReader reader)
		{
			bool isStructure = reader.ReadByte() != 0;
			string locationName = reader.ReadString();
			return Game1.getLocationRequest(locationName, isStructure);
		}

		protected virtual void readWarp(BinaryReader reader, int tileX, int tileY, Action afterWarp)
		{
			LocationRequest locationRequest = readLocationRequest(reader);
			if (afterWarp != null)
			{
				locationRequest.OnWarp += afterWarp.Invoke;
			}
			Game1.warpFarmer(locationRequest, tileX, tileY, Game1.player.FacingDirection);
		}

		protected virtual NPC readNPC(BinaryReader reader)
		{
			GameLocation gameLocation = readLocation(reader);
			Guid guid = reader.ReadGuid();
			if (!gameLocation.characters.ContainsGuid(guid))
			{
				return null;
			}
			return gameLocation.characters[guid];
		}

		public virtual TemporaryAnimatedSprite[] readSprites(BinaryReader reader, GameLocation location)
		{
			int num = reader.ReadInt32();
			TemporaryAnimatedSprite[] array = new TemporaryAnimatedSprite[num];
			for (int i = 0; i < num; i++)
			{
				TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite();
				temporaryAnimatedSprite.Read(reader, location);
				temporaryAnimatedSprite.ticksBeforeAnimationStart += interpolationTicks();
				array[i] = temporaryAnimatedSprite;
			}
			return array;
		}

		protected virtual void receiveTeamDelta(BinaryReader msg)
		{
			readObjectDelta(msg, Game1.player.teamRoot);
		}

		protected virtual void receiveNewDaySync(IncomingMessage msg)
		{
			if (Game1.newDaySync == null && msg.SourceFarmer == Game1.serverHost.Value)
			{
				Game1.NewDay(0f);
			}
			if (Game1.newDaySync != null)
			{
				Game1.newDaySync.receiveMessage(msg);
			}
		}

		protected virtual void receiveFarmerGainExperience(IncomingMessage msg)
		{
			if (msg.SourceFarmer == Game1.serverHost.Value)
			{
				int which = msg.Reader.ReadInt32();
				int howMuch = msg.Reader.ReadInt32();
				Game1.player.gainExperience(which, howMuch);
			}
		}

		protected virtual void receiveSharedAchievement(IncomingMessage msg)
		{
			int which = msg.Reader.ReadInt32();
			Game1.getAchievement(which, allowBroadcasting: false);
		}

		protected virtual void receiveRemoveLocationFromLookup(IncomingMessage msg)
		{
			string name_or_unique_name = msg.Reader.ReadString();
			Game1.removeLocationFromLocationLookup(name_or_unique_name);
		}

		protected virtual void receivePartyWideMail(IncomingMessage msg)
		{
			string mail_key = msg.Reader.ReadString();
			PartyWideMessageQueue message_queue = (PartyWideMessageQueue)msg.Reader.ReadInt32();
			bool no_letter = msg.Reader.ReadBoolean();
			_performPartyWideMail(mail_key, message_queue, no_letter);
		}

		protected void _performPartyWideMail(string mail_key, PartyWideMessageQueue message_queue, bool no_letter)
		{
			switch (message_queue)
			{
			case PartyWideMessageQueue.MailForTomorrow:
				Game1.addMailForTomorrow(mail_key, no_letter);
				break;
			case PartyWideMessageQueue.SeenMail:
				Game1.addMail(mail_key, no_letter);
				break;
			}
			if (no_letter)
			{
				mail_key += "%&NL&%";
			}
			switch (message_queue)
			{
			case PartyWideMessageQueue.MailForTomorrow:
				mail_key = "%&MFT&%" + mail_key;
				break;
			case PartyWideMessageQueue.SeenMail:
				mail_key = "%&SM&%" + mail_key;
				break;
			}
			if (Game1.IsMasterGame && !Game1.player.team.broadcastedMail.Contains(mail_key))
			{
				Game1.player.team.broadcastedMail.Add(mail_key);
			}
		}

		protected void receiveForceKick()
		{
			if (!Game1.IsServer)
			{
				Disconnect(DisconnectType.Kicked);
				returnToMainMenu();
			}
		}

		protected virtual void receiveGlobalMessage(IncomingMessage msg)
		{
			string path = msg.Reader.ReadString();
			if (!msg.Reader.ReadBoolean() || Game1.hudMessages.Count <= 0)
			{
				int num = msg.Reader.ReadInt32();
				object[] array = new object[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = msg.Reader.ReadString();
				}
				Game1.showGlobalMessage(Game1.content.LoadString(path, array));
			}
		}

		public virtual void processIncomingMessage(IncomingMessage msg)
		{
			switch (msg.MessageType)
			{
			case 0:
			{
				long id = msg.Reader.ReadInt64();
				NetFarmerRoot netFarmerRoot = farmerRoot(id);
				if (netFarmerRoot != null)
				{
					readObjectDelta(msg.Reader, netFarmerRoot);
				}
				break;
			}
			case 3:
				readActiveLocation(msg);
				break;
			case 6:
			{
				GameLocation gameLocation = readLocation(msg.Reader);
				if (gameLocation != null)
				{
					readObjectDelta(msg.Reader, gameLocation.Root);
				}
				break;
			}
			case 7:
			{
				GameLocation gameLocation = readLocation(msg.Reader);
				gameLocation?.temporarySprites.AddRange(readSprites(msg.Reader, gameLocation));
				break;
			}
			case 8:
			{
				NPC nPC = readNPC(msg.Reader);
				GameLocation gameLocation = readLocation(msg.Reader);
				if (nPC != null && gameLocation != null)
				{
					Game1.warpCharacter(nPC, gameLocation, msg.Reader.ReadVector2());
				}
				break;
			}
			case 4:
			{
				int eventId = msg.Reader.ReadInt32();
				bool flag = msg.Reader.ReadBoolean();
				int tileX = msg.Reader.ReadInt32();
				int tileY = msg.Reader.ReadInt32();
				LocationRequest request = readLocationRequest(msg.Reader);
				GameLocation locationFromName = Game1.getLocationFromName(request.Name);
				if (locationFromName == null || locationFromName.findEventById(eventId) == null)
				{
					Console.WriteLine("Couldn't find event " + eventId + " for broadcast event!");
					break;
				}
				Farmer farmerActor = null;
				if (flag)
				{
					farmerActor = (Game1.player.NetFields.Root as NetRoot<Farmer>).Clone().Value;
				}
				else
				{
					farmerActor = (msg.SourceFarmer.NetFields.Root as NetRoot<Farmer>).Clone().Value;
				}
				int old_x = (int)Game1.player.getTileLocation().X;
				int old_y = (int)Game1.player.getTileLocation().Y;
				string old_location = Game1.player.currentLocation.NameOrUniqueName;
				int direction = Game1.player.facingDirection.Value;
				Game1.player.locationBeforeForcedEvent.Value = old_location;
				request.OnWarp += delegate
				{
					farmerActor.currentLocation = Game1.currentLocation;
					farmerActor.completelyStopAnimatingOrDoingAction();
					farmerActor.UsingTool = false;
					farmerActor.items.Clear();
					farmerActor.hidden.Value = false;
					Event @event = Game1.currentLocation.findEventById(eventId, farmerActor);
					Game1.currentLocation.startEvent(@event);
					farmerActor.Position = Game1.player.Position;
					Game1.warpingForForcedRemoteEvent = false;
					string value = Game1.player.locationBeforeForcedEvent.Value;
					Game1.player.locationBeforeForcedEvent.Value = null;
					@event.setExitLocation(old_location, old_x, old_y);
					Game1.player.locationBeforeForcedEvent.Value = value;
					Game1.player.orientationBeforeEvent = direction;
				};
				Action item = delegate
				{
					Game1.warpingForForcedRemoteEvent = true;
					Game1.player.completelyStopAnimatingOrDoingAction();
					Game1.warpFarmer(request, tileX, tileY, Game1.player.FacingDirection);
				};
				Game1.remoteEventQueue.Add(item);
				break;
			}
			case 10:
			{
				long recipientID = msg.Reader.ReadInt64();
				LocalizedContentManager.LanguageCode language = msg.Reader.ReadEnum<LocalizedContentManager.LanguageCode>();
				string message = msg.Reader.ReadString();
				receiveChatMessage(msg.SourceFarmer, recipientID, language, message);
				break;
			}
			case 15:
			{
				string messageKey = msg.Reader.ReadString();
				string[] array = new string[msg.Reader.ReadByte()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = msg.Reader.ReadString();
				}
				receiveChatInfoMessage(msg.SourceFarmer, messageKey, array);
				break;
			}
			case 2:
				receivePlayerIntroduction(msg.Reader);
				break;
			case 12:
				receiveWorldState(msg.Reader);
				break;
			case 13:
				receiveTeamDelta(msg.Reader);
				break;
			case 14:
				receiveNewDaySync(msg);
				break;
			case 18:
				parseServerToClientsMessage(msg.Reader.ReadString());
				break;
			case 19:
				playerDisconnected(msg.SourceFarmer.UniqueMultiplayerID);
				break;
			case 17:
				receiveFarmerGainExperience(msg);
				break;
			case 25:
				receiveFarmerKilledMonster(msg);
				break;
			case 20:
				receiveSharedAchievement(msg);
				break;
			case 21:
				receiveGlobalMessage(msg);
				break;
			case 22:
				receivePartyWideMail(msg);
				break;
			case 27:
				receiveNutDig(msg);
				break;
			case 23:
				receiveForceKick();
				break;
			case 24:
				receiveRemoveLocationFromLookup(msg);
				break;
			case 26:
				receiveRequestGrandpaReevaluation(msg);
				break;
			case 28:
				receivePassoutRequest(msg);
				break;
			case 29:
				receivePassout(msg);
				break;
			case 1:
			case 5:
			case 9:
			case 11:
			case 16:
				break;
			}
		}

		public virtual void StartLocalMultiplayerServer()
		{
			Game1.server = new GameServer(local_multiplayer: true);
			Game1.server.startServer();
		}

		public virtual void StartServer()
		{
			Game1.server = new GameServer();
			Game1.server.startServer();
		}

		public virtual void Disconnect(DisconnectType disconnectType)
		{
			if (Game1.server != null)
			{
				Game1.server.stopServer();
				Game1.server = null;
				foreach (long key in Game1.otherFarmers.Keys)
				{
					playerDisconnected(key);
				}
			}
			if (Game1.client != null)
			{
				sendFarmhand();
				UpdateLate(forceSync: true);
				Game1.client.disconnect();
				Game1.client = null;
			}
			Game1.otherFarmers.Clear();
			LogDisconnect(disconnectType);
		}

		protected virtual void updatePendingConnections()
		{
			if (Game1.multiplayerMode == 2)
			{
				if (Game1.server == null && Game1.options.enableServer)
				{
					StartServer();
				}
			}
			else if (Game1.multiplayerMode == 1 && Game1.client != null && !Game1.client.readyToPlay)
			{
				Game1.client.receiveMessages();
			}
		}

		public void UpdateLoading()
		{
			updatePendingConnections();
			if (Game1.server != null)
			{
				Game1.server.receiveMessages();
			}
		}

		public virtual void UpdateEarly()
		{
			updatePendingConnections();
			if (Game1.multiplayerMode == 2 && Game1.serverHost == null && Game1.options.enableServer)
			{
				Game1.server.initializeHost();
			}
			if (Game1.server != null)
			{
				Game1.server.receiveMessages();
			}
			else if (Game1.client != null)
			{
				Game1.client.receiveMessages();
			}
			updateRoots();
			if (Game1.CurrentEvent == null)
			{
				removeDisconnectedFarmers();
			}
		}

		public virtual void UpdateLate(bool forceSync = false)
		{
			if (Game1.multiplayerMode != 0)
			{
				if (!allowSyncDelay() || forceSync || Game1.ticks % farmerDeltaBroadcastPeriod == 0)
				{
					broadcastFarmerDeltas();
				}
				if (!allowSyncDelay() || forceSync || Game1.ticks % locationDeltaBroadcastPeriod == 0)
				{
					broadcastLocationDeltas();
				}
				if (!allowSyncDelay() || forceSync || Game1.ticks % worldStateDeltaBroadcastPeriod == 0)
				{
					broadcastWorldStateDeltas();
				}
			}
			if (Game1.server != null)
			{
				Game1.server.sendMessages();
			}
			if (Game1.client != null)
			{
				Game1.client.sendMessages();
			}
		}

		public virtual void inviteAccepted()
		{
		}

		public virtual Client InitClient(Client client)
		{
			return client;
		}

		public virtual Server InitServer(Server server)
		{
			return server;
		}

		public static string MessageTypeToString(byte type)
		{
			return type switch
			{
				0 => "farmerDelta", 
				1 => "serverIntroduction", 
				2 => "playerIntroduction", 
				3 => "locationIntroduction", 
				4 => "forceEvent", 
				5 => "warpFarmer", 
				6 => "locationDelta", 
				7 => "locationSprites", 
				8 => "characterWarp", 
				9 => "availableFarmhands", 
				10 => "chatMessage", 
				11 => "connectionMessage", 
				12 => "worldDelta", 
				13 => "teamDelta", 
				14 => "newDaySync", 
				15 => "chatInfoMessage", 
				16 => "userNameUpdate", 
				17 => "farmerGainExperience", 
				18 => "serverToClientsMessage", 
				19 => "disconnecting", 
				20 => "sharedAchievement", 
				21 => "globalMessage", 
				22 => "partyWideMail", 
				23 => "forceKick", 
				24 => "removeLocationFromLookup", 
				25 => "farmerKilledMonster", 
				26 => "requestGrandpaReevaluation", 
				_ => type.ToString(), 
			};
		}
	}
}
