using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.GameData.Movies;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Network;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;

namespace StardewValley.Locations
{
	public class MovieTheater : GameLocation
	{
		public enum MovieStates
		{
			Preshow,
			Show,
			PostShow
		}

		protected bool _startedMovie;

		protected bool _isJojaTheater;

		internal static Dictionary<string, MovieData> _movieData;

		internal static List<MovieCharacterReaction> _genericReactions;

		internal static List<ConcessionTaste> _concessionTastes;

		protected readonly NetStringDictionary<int, NetInt> _spawnedMoviePatrons = new NetStringDictionary<int, NetInt>();

		protected readonly NetStringDictionary<int, NetInt> _purchasedConcessions = new NetStringDictionary<int, NetInt>();

		protected readonly NetStringDictionary<int, NetInt> _playerInvitedPatrons = new NetStringDictionary<int, NetInt>();

		protected readonly NetStringDictionary<bool, NetBool> _characterGroupLookup = new NetStringDictionary<bool, NetBool>();

		protected Dictionary<int, List<Point>> _hangoutPoints;

		protected Dictionary<int, List<Point>> _availableHangoutPoints;

		protected int _maxHangoutGroups;

		protected int _movieStartTime = -1;

		[XmlElement("dayFirstEntered")]
		public readonly NetInt dayFirstEntered = new NetInt(-1);

		internal static Dictionary<int, MovieConcession> _concessions;

		public const int LOVE_MOVIE_FRIENDSHIP = 200;

		public const int LIKE_MOVIE_FRIENDSHIP = 100;

		public const int DISLIKE_MOVIE_FRIENDSHIP = 0;

		public const int LOVE_CONCESSION_FRIENDSHIP = 50;

		public const int LIKE_CONCESSION_FRIENDSHIP = 25;

		public const int DISLIKE_CONCESSION_FRIENDSHIP = 0;

		public const int OPEN_TIME = 900;

		public const int CLOSE_TIME = 2100;

		public int nextRepathTime;

		public int repathTimeInterval = 1000;

		[XmlIgnore]
		protected Dictionary<string, KeyValuePair<Point, int>> _destinationPositions = new Dictionary<string, KeyValuePair<Point, int>>();

		[XmlIgnore]
		public PerchingBirds birds;

		protected int _exitX;

		protected int _exitY;

		private NetEvent1<MovieViewerLockEvent> movieViewerLockEvent = new NetEvent1<MovieViewerLockEvent>();

		private NetEvent1<StartMovieEvent> startMovieEvent = new NetEvent1<StartMovieEvent>();

		private NetEvent1Field<long, NetLong> requestStartMovieEvent = new NetEvent1Field<long, NetLong>();

		private NetEvent1Field<long, NetLong> endMovieEvent = new NetEvent1Field<long, NetLong>();

		protected List<Farmer> _viewingFarmers = new List<Farmer>();

		protected List<List<Character>> _viewingGroups = new List<List<Character>>();

		protected List<List<Character>> _playerGroups = new List<List<Character>>();

		protected List<List<Character>> _npcGroups = new List<List<Character>>();

		internal static bool _hasRequestedMovieStart = false;

		internal static int _playerHangoutGroup = -1;

		protected int _farmerCount;

		protected NetInt _currentState = new NetInt();

		public static string[][][][] possibleNPCGroups = new string[7][][][]
		{
			new string[3][][]
			{
				new string[1][] { new string[1] { "Lewis" } },
				new string[3][]
				{
					new string[3] { "Jas", "Vincent", "Marnie" },
					new string[3] { "Abigail", "Sebastian", "Sam" },
					new string[2] { "Penny", "Maru" }
				},
				new string[1][] { new string[2] { "Lewis", "Marnie" } }
			},
			new string[3][][]
			{
				new string[3][]
				{
					new string[1] { "Clint" },
					new string[2] { "Demetrius", "Robin" },
					new string[1] { "Lewis" }
				},
				new string[2][]
				{
					new string[2] { "Caroline", "Jodi" },
					new string[3] { "Abigail", "Sebastian", "Sam" }
				},
				new string[2][]
				{
					new string[1] { "Lewis" },
					new string[3] { "Abigail", "Sebastian", "Sam" }
				}
			},
			new string[3][][]
			{
				new string[2][]
				{
					new string[2] { "Evelyn", "George" },
					new string[1] { "Lewis" }
				},
				new string[2][]
				{
					new string[2] { "Penny", "Pam" },
					new string[3] { "Abigail", "Sebastian", "Sam" }
				},
				new string[2][]
				{
					new string[2] { "Sandy", "Emily" },
					new string[1] { "Elliot" }
				}
			},
			new string[3][][]
			{
				new string[3][]
				{
					new string[2] { "Penny", "Pam" },
					new string[3] { "Abigail", "Sebastian", "Sam" },
					new string[1] { "Lewis" }
				},
				new string[2][]
				{
					new string[3] { "Alex", "Haley", "Emily" },
					new string[3] { "Abigail", "Sebastian", "Sam" }
				},
				new string[2][]
				{
					new string[2] { "Pierre", "Caroline" },
					new string[3] { "Shane", "Jas", "Marnie" }
				}
			},
			new string[3][][]
			{
				null,
				new string[3][]
				{
					new string[2] { "Haley", "Emily" },
					new string[3] { "Abigail", "Sebastian", "Sam" },
					new string[1] { "Lewis" }
				},
				new string[2][]
				{
					new string[2] { "Penny", "Pam" },
					new string[3] { "Abigail", "Sebastian", "Sam" }
				}
			},
			new string[3][][]
			{
				new string[1][] { new string[1] { "Lewis" } },
				new string[2][]
				{
					new string[2] { "Penny", "Pam" },
					new string[3] { "Abigail", "Sebastian", "Sam" }
				},
				new string[2][]
				{
					new string[3] { "Harvey", "Maru", "Penny" },
					new string[1] { "Leah" }
				}
			},
			new string[3][][]
			{
				new string[3][]
				{
					new string[2] { "Penny", "Pam" },
					new string[3] { "George", "Evelyn", "Alex" },
					new string[1] { "Lewis" }
				},
				new string[2][]
				{
					new string[2] { "Gus", "Willy" },
					new string[2] { "Maru", "Sebastian" }
				},
				new string[2][]
				{
					new string[2] { "Penny", "Pam" },
					new string[2] { "Sandy", "Emily" }
				}
			}
		};

		public MovieTheater()
		{
		}

		public static void AddMoviePoster(GameLocation location, float x, float y, int month_offset = 0)
		{
			WorldDate worldDate = new WorldDate(Game1.Date);
			worldDate.TotalDays += 28 * month_offset;
			MovieData movieForDate = GetMovieForDate(worldDate);
			if (movieForDate != null)
			{
				location.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Movies"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, movieForDate.SheetIndex * 128, 13, 19),
					sourceRectStartingPos = new Vector2(0f, movieForDate.SheetIndex * 128),
					animationLength = 1,
					totalNumberOfLoops = 9999,
					interval = 9999f,
					scale = 4f,
					position = new Vector2(x, y),
					layerDepth = 0.01f
				});
			}
		}

		public MovieTheater(string map, string name)
			: base(map, name)
		{
			_currentState.Set(0);
			GetMovieData();
			_InitializeMap();
			GetMovieReactions();
		}

		public static List<MovieCharacterReaction> GetMovieReactions()
		{
			if (_genericReactions == null)
			{
				_genericReactions = Game1.content.Load<List<MovieCharacterReaction>>("Data\\MoviesReactions");
			}
			return _genericReactions;
		}

		public static string GetConcessionTasteForCharacter(Character character, MovieConcession concession)
		{
			if (_concessionTastes == null)
			{
				_concessionTastes = Game1.content.Load<List<ConcessionTaste>>("Data\\ConcessionTastes");
			}
			ConcessionTaste concessionTaste = null;
			foreach (ConcessionTaste concessionTaste2 in _concessionTastes)
			{
				if (concessionTaste2.Name == "*")
				{
					concessionTaste = concessionTaste2;
					break;
				}
			}
			foreach (ConcessionTaste concessionTaste3 in _concessionTastes)
			{
				if (!(concessionTaste3.Name == character.Name))
				{
					continue;
				}
				if (concessionTaste3.LovedTags.Contains(concession.Name))
				{
					return "love";
				}
				if (concessionTaste3.LikedTags.Contains(concession.Name))
				{
					return "like";
				}
				if (concessionTaste3.DislikedTags.Contains(concession.Name))
				{
					return "dislike";
				}
				if (concessionTaste != null)
				{
					if (concessionTaste.LovedTags.Contains(concession.Name))
					{
						return "love";
					}
					if (concessionTaste.LikedTags.Contains(concession.Name))
					{
						return "like";
					}
					if (concessionTaste.DislikedTags.Contains(concession.Name))
					{
						return "dislike";
					}
				}
				if (concession.tags == null)
				{
					break;
				}
				foreach (string tag in concession.tags)
				{
					if (concessionTaste3.LovedTags.Contains(tag))
					{
						return "love";
					}
					if (concessionTaste3.LikedTags.Contains(tag))
					{
						return "like";
					}
					if (concessionTaste3.DislikedTags.Contains(tag))
					{
						return "dislike";
					}
					if (concessionTaste != null)
					{
						if (concessionTaste.LovedTags.Contains(tag))
						{
							return "love";
						}
						if (concessionTaste.LikedTags.Contains(tag))
						{
							return "like";
						}
						if (concessionTaste.DislikedTags.Contains(tag))
						{
							return "dislike";
						}
					}
				}
				break;
			}
			return "like";
		}

		public static IEnumerable<string> GetPatronNames()
		{
			if (!(Game1.getLocationFromName("MovieTheater") is MovieTheater movieTheater))
			{
				return null;
			}
			if (movieTheater._spawnedMoviePatrons == null)
			{
				return null;
			}
			return movieTheater._spawnedMoviePatrons.Keys;
		}

		protected void _InitializeMap()
		{
			_hangoutPoints = new Dictionary<int, List<Point>>();
			_maxHangoutGroups = 0;
			if (map.GetLayer("Paths") != null)
			{
				Layer layer = map.GetLayer("Paths");
				for (int i = 0; i < layer.LayerWidth; i++)
				{
					for (int j = 0; j < layer.LayerHeight; j++)
					{
						if (layer.Tiles[i, j] == null || layer.Tiles[i, j].TileIndex != 7)
						{
							continue;
						}
						int result = -1;
						if (map.GetLayer("Paths").Tiles[i, j].Properties.ContainsKey("group") && int.TryParse(map.GetLayer("Paths").Tiles[i, j].Properties["group"], out result))
						{
							if (!_hangoutPoints.ContainsKey(result))
							{
								_hangoutPoints[result] = new List<Point>();
							}
							_hangoutPoints[result].Add(new Point(i, j));
							_maxHangoutGroups = Math.Max(_maxHangoutGroups, result);
						}
					}
				}
			}
			ResetTheater();
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(_spawnedMoviePatrons, _purchasedConcessions, _currentState, movieViewerLockEvent, requestStartMovieEvent, startMovieEvent, endMovieEvent, _playerInvitedPatrons, _characterGroupLookup, dayFirstEntered);
			movieViewerLockEvent.onEvent += OnMovieViewerLockEvent;
			requestStartMovieEvent.onEvent += OnRequestStartMovieEvent;
			startMovieEvent.onEvent += OnStartMovieEvent;
		}

		public void OnStartMovieEvent(StartMovieEvent e)
		{
			if (e.uid == Game1.player.UniqueMultiplayerID)
			{
				if (Game1.activeClickableMenu is ReadyCheckDialog)
				{
					(Game1.activeClickableMenu as ReadyCheckDialog).closeDialog(Game1.player);
				}
				MovieTheaterScreeningEvent movieTheaterScreeningEvent = new MovieTheaterScreeningEvent();
				Event viewing_event = movieTheaterScreeningEvent.getMovieEvent(GetMovieForDate(Game1.Date).ID, e.playerGroups, e.npcGroups, GetConcessionsDictionary());
				Rumble.rumble(0.15f, 200f);
				Game1.player.completelyStopAnimatingOrDoingAction();
				playSoundAt("doorClose", Game1.player.getTileLocation());
				Game1.globalFadeToBlack(delegate
				{
					Game1.changeMusicTrack("none");
					startEvent(viewing_event);
				});
			}
		}

		public void OnRequestStartMovieEvent(long uid)
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (_currentState.Value == 0)
			{
				if (Game1.player.team.movieMutex.IsLocked())
				{
					Game1.player.team.movieMutex.ReleaseLock();
				}
				Game1.player.team.movieMutex.RequestLock();
				_playerGroups = new List<List<Character>>();
				_npcGroups = new List<List<Character>>();
				List<Character> list = new List<Character>();
				foreach (string patronName in GetPatronNames())
				{
					Character characterFromName = Game1.getCharacterFromName(patronName);
					list.Add(characterFromName);
				}
				foreach (Farmer viewingFarmer in _viewingFarmers)
				{
					List<Character> list2 = new List<Character>();
					list2.Add(viewingFarmer);
					for (int i = 0; i < Game1.player.team.movieInvitations.Count; i++)
					{
						MovieInvitation movieInvitation = Game1.player.team.movieInvitations[i];
						if (movieInvitation.farmer == viewingFarmer && GetFirstInvitedPlayer(movieInvitation.invitedNPC) == viewingFarmer && list.Contains(movieInvitation.invitedNPC))
						{
							list.Remove(movieInvitation.invitedNPC);
							list2.Add(movieInvitation.invitedNPC);
						}
					}
					_playerGroups.Add(list2);
				}
				foreach (List<Character> playerGroup in _playerGroups)
				{
					foreach (Character item in playerGroup)
					{
						if (item is NPC)
						{
							(item as NPC).lastSeenMovieWeek.Set(Game1.Date.TotalWeeks);
						}
					}
				}
				_npcGroups.Add(new List<Character>(list));
				_PopulateNPCOnlyGroups(_playerGroups, _npcGroups);
				_viewingGroups = new List<List<Character>>();
				List<Character> list3 = new List<Character>();
				foreach (List<Character> playerGroup2 in _playerGroups)
				{
					foreach (Character item2 in playerGroup2)
					{
						list3.Add(item2);
					}
				}
				_viewingGroups.Add(list3);
				foreach (List<Character> npcGroup in _npcGroups)
				{
					_viewingGroups.Add(new List<Character>(npcGroup));
				}
				_currentState.Set(1);
			}
			startMovieEvent.Fire(new StartMovieEvent(uid, _playerGroups, _npcGroups));
		}

		public void OnMovieViewerLockEvent(MovieViewerLockEvent e)
		{
			_viewingFarmers = new List<Farmer>();
			_movieStartTime = e.movieStartTime;
			foreach (long uid in e.uids)
			{
				Farmer farmer = Game1.getFarmer(uid);
				if (farmer != null)
				{
					_viewingFarmers.Add(farmer);
				}
			}
			if (_viewingFarmers.Count > 0 && Game1.IsMultiplayer)
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\UI:MovieStartRequest"));
			}
			if (Game1.player.team.movieMutex.IsLockHeld())
			{
				_ShowMovieStartReady();
			}
		}

		public void _ShowMovieStartReady()
		{
			if (!Game1.IsMultiplayer)
			{
				requestStartMovieEvent.Fire(Game1.player.UniqueMultiplayerID);
				return;
			}
			Game1.player.team.SetLocalRequiredFarmers("start_movie", _viewingFarmers);
			Game1.player.team.SetLocalReady("start_movie", ready: true);
			Game1.dialogueUp = false;
			_hasRequestedMovieStart = true;
			Game1.activeClickableMenu = new ReadyCheckDialog("start_movie", allowCancel: true, delegate(Farmer farmer)
			{
				if (_hasRequestedMovieStart)
				{
					_hasRequestedMovieStart = false;
					requestStartMovieEvent.Fire(farmer.UniqueMultiplayerID);
				}
			}, delegate(Farmer farmer)
			{
				if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ReadyCheckDialog)
				{
					(Game1.activeClickableMenu as ReadyCheckDialog).closeDialog(farmer);
				}
				if (Game1.player.team.movieMutex.IsLockHeld())
				{
					Game1.player.team.movieMutex.ReleaseLock();
				}
			});
		}

		public static Dictionary<string, MovieData> GetMovieData()
		{
			if (_movieData == null)
			{
				_movieData = Game1.content.Load<Dictionary<string, MovieData>>("Data\\Movies");
				foreach (KeyValuePair<string, MovieData> movieDatum in _movieData)
				{
					movieDatum.Value.ID = movieDatum.Key;
				}
			}
			return _movieData;
		}

		public NPC GetMoviePatron(string name)
		{
			for (int i = 0; i < characters.Count; i++)
			{
				if (characters[i].name == name)
				{
					return characters[i];
				}
			}
			return null;
		}

		protected NPC AddMoviePatronNPC(string name, int x, int y, int facingDirection)
		{
			if (_spawnedMoviePatrons.ContainsKey(name))
			{
				return GetMoviePatron(name);
			}
			string text = (name.Equals("Krobus") ? "Krobus_Trenchcoat" : NPC.getTextureNameForCharacter(name));
			string syncedPortraitPath = "Portraits\\" + NPC.getTextureNameForCharacter(name);
			int num = ((name.Contains("Dwarf") || name.Equals("Krobus")) ? 96 : 128);
			NPC nPC = new NPC(new AnimatedSprite("Characters\\" + text, 0, 16, num / 4), new Vector2(x * 64, y * 64), base.Name, facingDirection, name, null, null, eventActor: true, syncedPortraitPath);
			nPC.eventActor = true;
			nPC.collidesWithOtherCharacters.Set(newValue: false);
			addCharacter(nPC);
			_spawnedMoviePatrons.Add(name, 1);
			Dialogue dialogueForCharacter = GetDialogueForCharacter(nPC);
			return nPC;
		}

		public void RemoveAllPatrons()
		{
			if (_spawnedMoviePatrons == null)
			{
				return;
			}
			for (int i = 0; i < characters.Count; i++)
			{
				if (_spawnedMoviePatrons.ContainsKey(characters[i].Name))
				{
					characters.RemoveAt(i);
					i--;
				}
			}
			_spawnedMoviePatrons.Clear();
		}

		protected override void resetSharedState()
		{
			base.resetSharedState();
			if (_currentState.Value == 0)
			{
				MovieData movieForDate = GetMovieForDate(Game1.Date);
				Game1.multiplayer.globalChatInfoMessage("MovieStart", movieForDate.Title);
			}
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			birds = new PerchingBirds(Game1.birdsSpriteSheet, 2, 16, 16, new Vector2(8f, 14f), new Point[14]
			{
				new Point(19, 5),
				new Point(21, 4),
				new Point(16, 3),
				new Point(10, 13),
				new Point(2, 13),
				new Point(2, 6),
				new Point(9, 2),
				new Point(18, 12),
				new Point(21, 11),
				new Point(3, 11),
				new Point(4, 2),
				new Point(12, 12),
				new Point(11, 5),
				new Point(13, 13)
			}, new Point[6]
			{
				new Point(19, 5),
				new Point(21, 4),
				new Point(16, 3),
				new Point(9, 2),
				new Point(21, 11),
				new Point(4, 2)
			});
			if (!_isJojaTheater && Game1.MasterPlayer.mailReceived.Contains("ccMovieTheaterJoja"))
			{
				_isJojaTheater = true;
			}
			if (dayFirstEntered.Value == -1)
			{
				dayFirstEntered.Value = Game1.Date.TotalDays;
			}
			if (!_isJojaTheater)
			{
				birds.roosting = _currentState.Value == 2;
				for (int i = 0; i < Game1.random.Next(2, 5); i++)
				{
					int bird_type = Game1.random.Next(0, 4);
					if (Game1.currentSeason == "fall")
					{
						bird_type = 10;
					}
					birds.AddBird(bird_type);
				}
				if (Game1.timeOfDay > 2100 && Game1.random.NextDouble() < 0.5)
				{
					birds.AddBird(11);
				}
			}
			AddMoviePoster(this, 1104f, 292f);
			loadMap(mapPath, force_reload: true);
			if (_isJojaTheater)
			{
				string text = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? "" : "_international");
				base.Map.TileSheets[0].ImageSource = "Maps\\MovieTheaterJoja_TileSheet" + text;
				base.Map.LoadTileSheets(Game1.mapDisplayDevice);
			}
			if (_currentState.Value == 0)
			{
				addRandomNPCs();
			}
			else if (_currentState.Value == 2)
			{
				Game1.changeMusicTrack("movieTheaterAfter");
				Game1.ambientLight = new Color(150, 170, 80);
				addSpecificRandomNPC(0);
			}
		}

		private void addRandomNPCs()
		{
			Random random = new Random((int)Game1.uniqueIDForThisGame + Game1.Date.TotalDays);
			critters = new List<Critter>();
			if (dayFirstEntered.Value == Game1.Date.TotalDays || random.NextDouble() < 0.25)
			{
				addSpecificRandomNPC(0);
			}
			if (!_isJojaTheater && random.NextDouble() < 0.28)
			{
				addSpecificRandomNPC(4);
				addSpecificRandomNPC(11);
			}
			else if (_isJojaTheater && random.NextDouble() < 0.33)
			{
				addSpecificRandomNPC(13);
			}
			if (random.NextDouble() < 0.1)
			{
				addSpecificRandomNPC(9);
				addSpecificRandomNPC(7);
			}
			if (Game1.currentSeason.Equals("fall") && random.NextDouble() < 0.5)
			{
				addSpecificRandomNPC(1);
			}
			if (Game1.currentSeason.Equals("spring") && random.NextDouble() < 0.5)
			{
				addSpecificRandomNPC(3);
			}
			if (random.NextDouble() < 0.25)
			{
				addSpecificRandomNPC(2);
			}
			if (random.NextDouble() < 0.25)
			{
				addSpecificRandomNPC(6);
			}
			if (random.NextDouble() < 0.25)
			{
				addSpecificRandomNPC(8);
			}
			if (random.NextDouble() < 0.2)
			{
				addSpecificRandomNPC(10);
			}
			if (random.NextDouble() < 0.2)
			{
				addSpecificRandomNPC(12);
			}
			if (random.NextDouble() < 0.2)
			{
				addSpecificRandomNPC(5);
			}
			if (!_isJojaTheater)
			{
				if (random.NextDouble() < 0.75)
				{
					addCritter(new Butterfly(new Vector2(13f, 7f)).setStayInbounds(stayInbounds: true));
				}
				if (random.NextDouble() < 0.75)
				{
					addCritter(new Butterfly(new Vector2(4f, 8f)).setStayInbounds(stayInbounds: true));
				}
				if (random.NextDouble() < 0.75)
				{
					addCritter(new Butterfly(new Vector2(17f, 10f)).setStayInbounds(stayInbounds: true));
				}
			}
		}

		private void addSpecificRandomNPC(int whichRandomNPC)
		{
			Random random = new Random((int)Game1.uniqueIDForThisGame + Game1.Date.TotalDays + whichRandomNPC);
			switch (whichRandomNPC)
			{
			case 0:
				setMapTile(2, 9, 215, "Buildings", "MessageSpeech MovieTheater_CraneMan" + ((random.NextDouble() < 0.5) ? "2" : ""));
				setMapTile(2, 8, 199, "Front", null);
				break;
			case 1:
				setMapTile(19, 7, 216, "Buildings", "MessageSpeech MovieTheater_Welwick" + ((random.NextDouble() < 0.5) ? "2" : ""));
				setMapTile(19, 6, 200, "Front", null);
				break;
			case 2:
				setAnimatedMapTile(21, 7, new int[4] { 217, 217, 217, 218 }, 700L, "Buildings", "MessageSpeech MovieTheater_ShortsMan" + ((random.NextDouble() < 0.5) ? "2" : ""));
				setAnimatedMapTile(21, 6, new int[4] { 201, 201, 201, 202 }, 700L, "Front", null);
				break;
			case 3:
				setMapTile(5, 9, 219, "Buildings", "MessageSpeech MovieTheater_Mother" + ((random.NextDouble() < 0.5) ? "2" : ""));
				setMapTile(6, 9, 220, "Buildings", "MessageSpeech MovieTheater_Child" + ((random.NextDouble() < 0.5) ? "2" : ""));
				setAnimatedMapTile(5, 8, new int[6] { 203, 203, 203, 204, 204, 204 }, 1000L, "Front", null);
				break;
			case 4:
				setMapTileIndex(20, 9, 222, "Front");
				setMapTileIndex(21, 9, 223, "Front");
				setMapTile(20, 10, 238, "Buildings", null);
				setMapTile(21, 10, 239, "Buildings", null);
				setMapTileIndex(20, 11, 254, "Buildings");
				setMapTileIndex(21, 11, 255, "Buildings");
				break;
			case 5:
				setAnimatedMapTile(10, 7, new int[4] { 251, 251, 251, 252 }, 900L, "Buildings", "MessageSpeech MovieTheater_Lupini" + ((random.NextDouble() < 0.5) ? "2" : ""));
				setAnimatedMapTile(10, 6, new int[4] { 235, 235, 235, 236 }, 900L, "Front", null);
				break;
			case 6:
				setAnimatedMapTile(5, 7, new int[4] { 249, 249, 249, 250 }, 600L, "Buildings", "MessageSpeech MovieTheater_ConcessionMan" + ((random.NextDouble() < 0.5) ? "2" : ""));
				setAnimatedMapTile(5, 6, new int[4] { 233, 233, 233, 234 }, 600L, "Front", null);
				break;
			case 7:
				setMapTile(1, 12, 248, "Buildings", "MessageSpeech MovieTheater_PurpleHairLady");
				setMapTile(1, 11, 232, "Front", null);
				break;
			case 8:
				setMapTile(3, 8, 247, "Buildings", "MessageSpeech MovieTheater_RedCapGuy" + ((random.NextDouble() < 0.5) ? "2" : ""));
				setMapTile(3, 7, 231, "Front", null);
				break;
			case 9:
				setMapTile(2, 11, 253, "Buildings", "MessageSpeech MovieTheater_Governor" + ((random.NextDouble() < 0.5) ? "2" : ""));
				setMapTile(2, 10, 237, "Front", null);
				break;
			case 10:
				setMapTile(9, 7, 221, "Buildings", "NPCSpeechMessageNoRadius Gunther MovieTheater_Gunther" + ((random.NextDouble() < 0.5) ? "2" : ""));
				setMapTile(9, 6, 205, "Front", null);
				break;
			case 11:
				setMapTile(19, 10, 208, "Buildings", "NPCSpeechMessageNoRadius Marlon MovieTheater_Marlon" + ((random.NextDouble() < 0.5) ? "2" : ""));
				setMapTile(19, 9, 192, "Front", null);
				break;
			case 12:
				setMapTile(12, 4, 209, "Buildings", "MessageSpeech MovieTheater_Marcello" + ((random.NextDouble() < 0.5) ? "2" : ""));
				setMapTile(12, 3, 193, "Front", null);
				break;
			case 13:
				setMapTile(17, 12, 241, "Buildings", "NPCSpeechMessageNoRadius Morris MovieTheater_Morris" + ((random.NextDouble() < 0.5) ? "2" : ""));
				setMapTile(17, 11, 225, "Front", null);
				break;
			}
		}

		public static MovieData GetMovieForDate(WorldDate date)
		{
			GetMovieData();
			string text = (date.Season + "_movie_").ToString();
			long num = Game1.player.team.theaterBuildDate;
			long num2 = date.TotalDays;
			long num3 = num2 / 112 - num / 112;
			if (num2 / 28 % 4 < num / 28 % 4)
			{
				num3--;
			}
			int num4 = 0;
			if (_movieData.ContainsKey(text + num3))
			{
				return _movieData[text + num3];
			}
			foreach (MovieData value in _movieData.Values)
			{
				if (value == null || !value.ID.StartsWith(text))
				{
					continue;
				}
				string[] array = value.ID.Split('_');
				if (array.Length >= 3)
				{
					int result = 0;
					if (int.TryParse(array[2], out result) && result > num4)
					{
						num4 = result;
					}
				}
			}
			foreach (MovieData value2 in _movieData.Values)
			{
				if (value2.ID == text + num3 % (num4 + 1))
				{
					return value2;
				}
			}
			return _movieData.Values.FirstOrDefault();
		}

		public override void DayUpdate(int dayOfMonth)
		{
			ResetTheater();
			_ResetHangoutPoints();
			base.DayUpdate(dayOfMonth);
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (_farmerCount != farmers.Count)
			{
				_farmerCount = farmers.Count;
				if (Game1.activeClickableMenu is ReadyCheckDialog)
				{
					(Game1.activeClickableMenu as ReadyCheckDialog).closeDialog(Game1.player);
					if (Game1.player.team.movieMutex.IsLockHeld())
					{
						Game1.player.team.movieMutex.ReleaseLock();
					}
				}
			}
			if (birds != null)
			{
				birds.Update(time);
			}
			base.UpdateWhenCurrentLocation(time);
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			if (birds != null)
			{
				birds.Draw(b);
			}
			base.drawAboveAlwaysFrontLayer(b);
		}

		public static bool Invite(Farmer farmer, NPC invited_npc)
		{
			if (farmer == null || invited_npc == null)
			{
				return false;
			}
			MovieInvitation movieInvitation = new MovieInvitation();
			movieInvitation.farmer = farmer;
			movieInvitation.invitedNPC = invited_npc;
			farmer.team.movieInvitations.Add(movieInvitation);
			return true;
		}

		public void ResetTheater()
		{
			_playerHangoutGroup = -1;
			RemoveAllPatrons();
			_playerGroups.Clear();
			_npcGroups.Clear();
			_viewingGroups.Clear();
			_viewingFarmers.Clear();
			_purchasedConcessions.Clear();
			_playerInvitedPatrons.Clear();
			_characterGroupLookup.Clear();
			_ResetHangoutPoints();
			Game1.player.team.movieMutex.ReleaseLock();
			_currentState.Set(0);
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
			movieViewerLockEvent.Poll();
			requestStartMovieEvent.Poll();
			startMovieEvent.Poll();
			endMovieEvent.Poll();
			if (!Game1.IsMasterGame)
			{
				return;
			}
			for (int i = 0; i < _viewingFarmers.Count; i++)
			{
				Farmer farmer = _viewingFarmers[i];
				if (!Game1.getOnlineFarmers().Contains(farmer))
				{
					_viewingFarmers.RemoveAt(i);
					i--;
				}
				else if (_currentState.Value == 2 && !farmers.Contains(farmer) && !HasFarmerWatchingBroadcastEventReturningHere() && farmer.currentLocation != null && farmer.currentLocation.Name != "Temp")
				{
					_viewingFarmers.RemoveAt(i);
					i--;
				}
			}
			if (_currentState.Value != 0 && _viewingFarmers.Count == 0)
			{
				MovieData movieForDate = GetMovieForDate(Game1.Date);
				Game1.multiplayer.globalChatInfoMessage("MovieEnd", movieForDate.Title);
				ResetTheater();
			}
			if (Game1.player.team.movieInvitations == null || _playerInvitedPatrons.Count() >= 4)
			{
				return;
			}
			foreach (Farmer farmer2 in farmers)
			{
				for (int j = 0; j < Game1.player.team.movieInvitations.Count; j++)
				{
					MovieInvitation movieInvitation = Game1.player.team.movieInvitations[j];
					if (movieInvitation.fulfilled || _spawnedMoviePatrons.ContainsKey(movieInvitation.invitedNPC.displayName))
					{
						continue;
					}
					if (_playerHangoutGroup < 0)
					{
						_playerHangoutGroup = Game1.random.Next(_maxHangoutGroups);
					}
					int playerHangoutGroup = _playerHangoutGroup;
					if (movieInvitation.farmer == farmer2 && GetFirstInvitedPlayer(movieInvitation.invitedNPC) == farmer2)
					{
						Point random = Utility.GetRandom(_availableHangoutPoints[playerHangoutGroup]);
						NPC nPC = AddMoviePatronNPC(movieInvitation.invitedNPC.name, 14, 15, 0);
						_playerInvitedPatrons.Add(nPC.name, 1);
						_availableHangoutPoints[playerHangoutGroup].Remove(random);
						int result = 2;
						if (map.GetLayer("Paths").Tiles[random.X, random.Y].Properties != null && map.GetLayer("Paths").Tiles[random.X, random.Y].Properties.ContainsKey("direction"))
						{
							int.TryParse(map.GetLayer("Paths").Tiles[random.X, random.Y].Properties["direction"], out result);
						}
						_destinationPositions[nPC.Name] = new KeyValuePair<Point, int>(random, result);
						PathCharacterToLocation(nPC, random, result);
						movieInvitation.fulfilled = true;
					}
				}
			}
		}

		public static MovieCharacterReaction GetReactionsForCharacter(NPC character)
		{
			if (character == null)
			{
				return null;
			}
			foreach (MovieCharacterReaction movieReaction in GetMovieReactions())
			{
				if (!(movieReaction.NPCName != character.Name))
				{
					return movieReaction;
				}
			}
			return null;
		}

		public override void checkForMusic(GameTime time)
		{
		}

		public static string GetResponseForMovie(NPC character)
		{
			string result = "like";
			MovieData movieForDate = GetMovieForDate(Game1.Date);
			if (movieForDate == null)
			{
				return null;
			}
			if (movieForDate != null)
			{
				foreach (MovieCharacterReaction movieReaction in GetMovieReactions())
				{
					if (!(movieReaction.NPCName != character.Name))
					{
						foreach (MovieReaction reaction in movieReaction.Reactions)
						{
							if (reaction.ShouldApplyToMovie(movieForDate, GetPatronNames()) && reaction.Response != null && reaction.Response.Length > 0)
							{
								result = reaction.Response;
								break;
							}
						}
					}
				}
				return result;
			}
			return result;
		}

		public Dialogue GetDialogueForCharacter(NPC character)
		{
			MovieData movieForDate = GetMovieForDate(Game1.Date);
			if (movieForDate != null)
			{
				foreach (MovieCharacterReaction genericReaction in _genericReactions)
				{
					if (genericReaction.NPCName != character.Name)
					{
						continue;
					}
					foreach (MovieReaction reaction in genericReaction.Reactions)
					{
						if (reaction.ShouldApplyToMovie(movieForDate, GetPatronNames(), GetResponseForMovie(character)) && reaction.Response != null && reaction.Response.Length > 0 && reaction.SpecialResponses != null)
						{
							if (_currentState.Value == 0 && reaction.SpecialResponses.BeforeMovie != null)
							{
								return new Dialogue(FormatString(reaction.SpecialResponses.BeforeMovie.Text), character);
							}
							if (_currentState.Value == 1 && reaction.SpecialResponses.DuringMovie != null)
							{
								return new Dialogue(FormatString(reaction.SpecialResponses.DuringMovie.Text), character);
							}
							if (_currentState.Value == 2 && reaction.SpecialResponses.AfterMovie != null)
							{
								return new Dialogue(FormatString(reaction.SpecialResponses.AfterMovie.Text), character);
							}
							break;
						}
					}
					break;
				}
			}
			return null;
		}

		public string FormatString(string text, params string[] args)
		{
			return string.Format(text, GetMovieForDate(Game1.Date).Title, Game1.player.displayName, args);
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
			PropertyValue value2 = null;
			map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size)?.Properties.TryGetValue("Action", out value2);
			if (value2 != null)
			{
				return performAction(value2, who, tileLocation);
			}
			foreach (NPC character in characters)
			{
				if (character == null || character.IsMonster || (who.isRidingHorse() && character is Horse) || !character.GetBoundingBox().Intersects(value))
				{
					continue;
				}
				if (!character.isMoving())
				{
					if (_playerInvitedPatrons.ContainsKey(character.Name))
					{
						character.faceTowardFarmerForPeriod(5000, 4, faceAway: false, who);
						Dialogue dialogueForCharacter = GetDialogueForCharacter(character);
						if (dialogueForCharacter != null)
						{
							character.CurrentDialogue.Push(dialogueForCharacter);
							Game1.drawDialogue(character);
							character.grantConversationFriendship(Game1.player);
						}
					}
					else if (_characterGroupLookup.ContainsKey(character.Name))
					{
						if (!_characterGroupLookup[character.Name])
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_AfterMovieAlone", character.Name));
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_AfterMovie", character.Name));
						}
					}
				}
				return true;
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		protected void _PopulateNPCOnlyGroups(List<List<Character>> player_groups, List<List<Character>> groups)
		{
			HashSet<string> hashSet = new HashSet<string>();
			foreach (List<Character> player_group in player_groups)
			{
				foreach (Character item2 in player_group)
				{
					if (item2 is NPC)
					{
						hashSet.Add(item2.name);
					}
				}
			}
			foreach (List<Character> group in groups)
			{
				foreach (Character item3 in group)
				{
					if (item3 is NPC)
					{
						hashSet.Add(item3.name);
					}
				}
			}
			Random random = new Random((int)Game1.uniqueIDForThisGame + Game1.Date.TotalDays);
			int num = 0;
			for (int i = 0; i < 2; i++)
			{
				if (random.NextDouble() < 0.75)
				{
					num++;
				}
			}
			int num2 = 0;
			if (_movieStartTime >= 1200)
			{
				num2 = 1;
			}
			if (_movieStartTime >= 1800)
			{
				num2 = 2;
			}
			string[][] array = possibleNPCGroups[(int)Game1.Date.DayOfWeek][num2];
			if (array == null)
			{
				return;
			}
			if (groups.Count > 0 && groups[0].Count == 0)
			{
				groups.RemoveAt(0);
			}
			for (int j = 0; j < num; j++)
			{
				if (groups.Count >= 2)
				{
					break;
				}
				int num3 = random.Next(array.Length);
				bool flag = true;
				string[] array2 = array[num3];
				foreach (string text in array2)
				{
					bool flag2 = false;
					foreach (Farmer allFarmer in Game1.getAllFarmers())
					{
						if (allFarmer.friendshipData.ContainsKey(text))
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						flag = false;
						break;
					}
					if (hashSet.Contains(text))
					{
						flag = false;
						break;
					}
					if (GetResponseForMovie(Game1.getCharacterFromName(text)) == "dislike" || GetResponseForMovie(Game1.getCharacterFromName(text)) == "reject")
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					List<Character> list = new List<Character>();
					string[] array3 = array[num3];
					foreach (string text2 in array3)
					{
						NPC item = AddMoviePatronNPC(text2, 1000, 1000, 2);
						list.Add(item);
						hashSet.Add(text2);
						_characterGroupLookup[text2] = array[num3].Length > 1;
					}
					groups.Add(list);
				}
			}
		}

		public Dictionary<Character, MovieConcession> GetConcessionsDictionary()
		{
			Dictionary<Character, MovieConcession> dictionary = new Dictionary<Character, MovieConcession>();
			foreach (string key in _purchasedConcessions.Keys)
			{
				Character characterFromName = Game1.getCharacterFromName(key);
				if (characterFromName != null && GetConcessions().ContainsKey(_purchasedConcessions[key]))
				{
					dictionary[characterFromName] = GetConcessions()[_purchasedConcessions[key]];
				}
			}
			return dictionary;
		}

		protected void _ResetHangoutPoints()
		{
			_destinationPositions.Clear();
			_availableHangoutPoints = new Dictionary<int, List<Point>>();
			foreach (int key in _hangoutPoints.Keys)
			{
				_availableHangoutPoints[key] = new List<Point>(_hangoutPoints[key]);
			}
		}

		public override void cleanupBeforePlayerExit()
		{
			if (!Game1.eventUp)
			{
				Game1.changeMusicTrack("none");
			}
			birds = null;
			base.cleanupBeforePlayerExit();
		}

		public void RequestEndMovie(long uid)
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (_currentState.Value == 1)
			{
				_currentState.Set(2);
				for (int i = 0; i < _viewingGroups.Count; i++)
				{
					int index = Game1.random.Next(_viewingGroups.Count);
					List<Character> value = _viewingGroups[i];
					_viewingGroups[i] = _viewingGroups[index];
					_viewingGroups[index] = value;
				}
				_ResetHangoutPoints();
				int num = 0;
				for (int j = 0; j < _viewingGroups.Count; j++)
				{
					for (int k = 0; k < _viewingGroups[j].Count; k++)
					{
						if (!(_viewingGroups[j][k] is NPC))
						{
							continue;
						}
						NPC moviePatron = GetMoviePatron(_viewingGroups[j][k].Name);
						if (moviePatron != null)
						{
							moviePatron.setTileLocation(new Vector2(14f, 4f + (float)num * 1f));
							Point random = Utility.GetRandom(_availableHangoutPoints[j]);
							int result = 2;
							if (map.GetLayer("Paths").Tiles[random.X, random.Y].Properties.ContainsKey("direction"))
							{
								int.TryParse(map.GetLayer("Paths").Tiles[random.X, random.Y].Properties["direction"], out result);
							}
							_destinationPositions[moviePatron.Name] = new KeyValuePair<Point, int>(random, result);
							PathCharacterToLocation(moviePatron, random, result);
							_availableHangoutPoints[j].Remove(random);
							num++;
						}
					}
				}
			}
			Game1.getFarmer(uid).team.endMovieEvent.Fire(uid);
		}

		public void PathCharacterToLocation(NPC character, Point point, int direction)
		{
			if (character.currentLocation == this)
			{
				PathFindController pathFindController = new PathFindController(character, this, character.getTileLocationPoint(), direction);
				pathFindController.pathToEndPoint = PathFindController.findPathForNPCSchedules(character.getTileLocationPoint(), point, this, 30000);
				character.temporaryController = pathFindController;
				character.followSchedule = true;
				character.ignoreScheduleToday = true;
			}
		}

		public Dictionary<int, MovieConcession> GetConcessions()
		{
			if (_concessions == null)
			{
				_concessions = new Dictionary<int, MovieConcession>();
				List<ConcessionItemData> list = Game1.content.Load<List<ConcessionItemData>>("Data\\Concessions");
				List<MovieConcession> list2 = new List<MovieConcession>();
				foreach (ConcessionItemData item in list)
				{
					_concessions[item.ID] = new MovieConcession(item);
				}
			}
			return _concessions;
		}

		public bool OnPurchaseConcession(ISalable salable, Farmer who, int amount)
		{
			foreach (MovieInvitation movieInvitation in who.team.movieInvitations)
			{
				if (movieInvitation.farmer == who && GetFirstInvitedPlayer(movieInvitation.invitedNPC) == Game1.player && _spawnedMoviePatrons.ContainsKey(movieInvitation.invitedNPC.Name))
				{
					_purchasedConcessions[movieInvitation.invitedNPC.Name] = (salable as MovieConcession).id;
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_ConcessionPurchased", (salable as MovieConcession).DisplayName, movieInvitation.invitedNPC.displayName));
					return true;
				}
			}
			return false;
		}

		public bool HasInvitedSomeone(Farmer who)
		{
			foreach (MovieInvitation movieInvitation in who.team.movieInvitations)
			{
				if (movieInvitation.farmer == who && GetFirstInvitedPlayer(movieInvitation.invitedNPC) == Game1.player && _spawnedMoviePatrons.ContainsKey(movieInvitation.invitedNPC.Name))
				{
					return true;
				}
			}
			return false;
		}

		public bool HasPurchasedConcession(Farmer who)
		{
			if (!HasInvitedSomeone(who))
			{
				return false;
			}
			foreach (MovieInvitation movieInvitation in who.team.movieInvitations)
			{
				if (movieInvitation.farmer != who || GetFirstInvitedPlayer(movieInvitation.invitedNPC) != Game1.player)
				{
					continue;
				}
				foreach (string key in _purchasedConcessions.Keys)
				{
					if (key == movieInvitation.invitedNPC.Name && _spawnedMoviePatrons.ContainsKey(movieInvitation.invitedNPC.Name))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static Farmer GetFirstInvitedPlayer(NPC npc)
		{
			foreach (MovieInvitation movieInvitation in Game1.player.team.movieInvitations)
			{
				if (movieInvitation.invitedNPC.Name == npc.Name)
				{
					return movieInvitation.farmer;
				}
			}
			return null;
		}

		public override void performTouchAction(string fullActionString, Vector2 playerStandingPosition)
		{
			string text = fullActionString.Split(' ')[0];
			if (text == "Theater_Exit")
			{
				_exitX = int.Parse(fullActionString.Split(' ')[1]) + Town.GetTheaterTileOffset().X;
				_exitY = int.Parse(fullActionString.Split(' ')[2]) + Town.GetTheaterTileOffset().Y;
				if ((int)Game1.player.lastSeenMovieWeek >= Game1.Date.TotalWeeks)
				{
					_Leave();
					return;
				}
				Game1.player.position.Y -= (Game1.player.Speed + Game1.player.addedSpeed) * 2;
				Game1.player.Halt();
				Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_LeavePrompt"), Game1.currentLocation.createYesNoResponses(), "LeaveMovie");
			}
			else
			{
				base.performTouchAction(fullActionString, playerStandingPosition);
			}
		}

		public List<MovieConcession> GetConcessionsForGuest(string npc_name)
		{
			List<MovieConcession> list = new List<MovieConcession>();
			List<MovieConcession> list2 = GetConcessions().Values.ToList();
			Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			Utility.Shuffle(random, list2);
			NPC characterFromName = Game1.getCharacterFromName(npc_name);
			if (characterFromName == null)
			{
				return list;
			}
			int num = 1;
			int num2 = 2;
			int num3 = 1;
			int num4 = 5;
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < list2.Count; j++)
				{
					MovieConcession movieConcession = list2[j];
					if (GetConcessionTasteForCharacter(characterFromName, movieConcession) == "love" && (!movieConcession.Name.Equals("Stardrop Sorbet") || random.NextDouble() < 0.33))
					{
						list.Add(movieConcession);
						list2.RemoveAt(j);
						j--;
						break;
					}
				}
			}
			for (int k = 0; k < num2; k++)
			{
				for (int l = 0; l < list2.Count; l++)
				{
					MovieConcession movieConcession2 = list2[l];
					if (GetConcessionTasteForCharacter(characterFromName, movieConcession2) == "like")
					{
						list.Add(movieConcession2);
						list2.RemoveAt(l);
						l--;
						break;
					}
				}
			}
			for (int m = 0; m < num3; m++)
			{
				for (int n = 0; n < list2.Count; n++)
				{
					MovieConcession movieConcession3 = list2[n];
					if (GetConcessionTasteForCharacter(characterFromName, movieConcession3) == "dislike")
					{
						list.Add(movieConcession3);
						list2.RemoveAt(n);
						n--;
						break;
					}
				}
			}
			for (int num5 = list.Count; num5 < num4; num5++)
			{
				int num6 = 0;
				if (num6 < list2.Count)
				{
					MovieConcession item = list2[num6];
					list.Add(item);
					list2.RemoveAt(num6);
					num6--;
				}
			}
			if (_isJojaTheater && !list.Exists((MovieConcession x) => x.Name.Equals("JojaCorn")))
			{
				MovieConcession movieConcession4 = list2.Find((MovieConcession x) => x.Name.Equals("JojaCorn"));
				if (movieConcession4 != null)
				{
					list.Add(movieConcession4);
				}
			}
			Utility.Shuffle(random, list);
			return list;
		}

		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (!(questionAndAnswer == "LeaveMovie_Yes"))
			{
				if (questionAndAnswer == "Concession_Yes")
				{
					string npc_name = "";
					foreach (MovieInvitation movieInvitation in Game1.player.team.movieInvitations)
					{
						if (movieInvitation.farmer == Game1.player && GetFirstInvitedPlayer(movieInvitation.invitedNPC) == Game1.player)
						{
							npc_name = movieInvitation.invitedNPC.Name;
						}
					}
					Game1.activeClickableMenu = new ShopMenu(((IEnumerable<ISalable>)GetConcessionsForGuest(npc_name)).ToList(), 0, "Concessions", OnPurchaseConcession);
					return true;
				}
				return base.answerDialogueAction(questionAndAnswer, questionParams);
			}
			_Leave();
			return true;
		}

		protected void _Leave()
		{
			Game1.player.completelyStopAnimatingOrDoingAction();
			Game1.warpFarmer("Town", _exitX, _exitY, 2);
		}

		public override bool performAction(string action, Farmer who, Location tileLocation)
		{
			switch (action)
			{
			case "Concessions":
				if (_currentState.Value > 0)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_ConcessionAfterMovie"));
					return true;
				}
				if (!HasInvitedSomeone(who))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_ConcessionAlone"));
					return true;
				}
				if (HasPurchasedConcession(who))
				{
					foreach (MovieInvitation movieInvitation in who.team.movieInvitations)
					{
						if (movieInvitation.farmer != who || GetFirstInvitedPlayer(movieInvitation.invitedNPC) != Game1.player)
						{
							continue;
						}
						foreach (string key in _purchasedConcessions.Keys)
						{
							if (key == movieInvitation.invitedNPC.Name)
							{
								MovieConcession movieConcession = GetConcessionsDictionary()[Game1.getCharacterFromName(key)];
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_ConcessionPurchased", movieConcession.DisplayName, Game1.getCharacterFromName(key).displayName));
								return true;
							}
						}
					}
					return true;
				}
				Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_Concession"), Game1.currentLocation.createYesNoResponses(), "Concession");
				break;
			case "Theater_Doors":
				if (_currentState.Value > 0)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Theater_MovieEndReEntry"));
					return true;
				}
				if (Game1.player.team.movieMutex.IsLocked())
				{
					_ShowMovieStartReady();
					return true;
				}
				Game1.player.team.movieMutex.RequestLock(delegate
				{
					List<Farmer> list = new List<Farmer>();
					foreach (Farmer farmer in farmers)
					{
						if (farmer.isActive() && farmer.currentLocation == this)
						{
							list.Add(farmer);
						}
					}
					movieViewerLockEvent.Fire(new MovieViewerLockEvent(list, Game1.timeOfDay));
				});
				return true;
			case "CraneGame":
				if (getTileIndexAt(2, 9, "Buildings") == -1)
				{
					createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromMaps:MovieTheater_CranePlay", 500), createYesNoResponses(), tryToStartCraneGame);
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromMaps:MovieTheater_CraneOccupied"));
				}
				break;
			}
			return base.performAction(action, who, tileLocation);
		}

		private void tryToStartCraneGame(Farmer who, string whichAnswer)
		{
			if (!(whichAnswer.ToLower() == "yes"))
			{
				return;
			}
			if (Game1.player.Money >= 500)
			{
				Game1.player.Money -= 500;
				Game1.changeMusicTrack("none", track_interruptable: false, Game1.MusicContext.MiniGame);
				Game1.globalFadeToBlack(delegate
				{
					Game1.currentMinigame = new CraneGame();
				}, 0.008f);
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11325"));
			}
		}

		public static void ClearCachedLocalizedData()
		{
			_concessions = null;
			_genericReactions = null;
			_movieData = null;
		}
	}
}
