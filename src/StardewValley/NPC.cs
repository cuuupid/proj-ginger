using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley
{
	public class NPC : Character, IComparable
	{
		public const int minimum_square_pause = 6000;

		public const int maximum_square_pause = 12000;

		public const int portrait_width = 64;

		public const int portrait_height = 64;

		public const int portrait_neutral_index = 0;

		public const int portrait_happy_index = 1;

		public const int portrait_sad_index = 2;

		public const int portrait_custom_index = 3;

		public const int portrait_blush_index = 4;

		public const int portrait_angry_index = 5;

		public const int startingFriendship = 0;

		public const int defaultSpeed = 2;

		public const int maxGiftsPerWeek = 2;

		public const int friendshipPointsPerHeartLevel = 250;

		public const int maxFriendshipPoints = 2500;

		public const int gift_taste_love = 0;

		public const int gift_taste_like = 2;

		public const int gift_taste_neutral = 8;

		public const int gift_taste_dislike = 4;

		public const int gift_taste_hate = 6;

		public const int textStyle_shake = 0;

		public const int textStyle_fade = 1;

		public const int textStyle_none = 2;

		public const int adult = 0;

		public const int teen = 1;

		public const int child = 2;

		public const int neutral = 0;

		public const int polite = 1;

		public const int rude = 2;

		public const int outgoing = 0;

		public const int shy = 1;

		public const int positive = 0;

		public const int negative = 1;

		public const int male = 0;

		public const int female = 1;

		public const int undefined = 2;

		public const int other = 0;

		public const int desert = 1;

		public const int town = 2;

		public static bool isCheckingSpouseTileOccupancy = false;

		internal static List<List<string>> routesFromLocationToLocation;

		private Dictionary<int, SchedulePathDescription> schedule;

		private Dictionary<string, string> dialogue;

		private SchedulePathDescription directionsToNewLocation;

		private int directionIndex;

		private int lengthOfWalkingSquareX;

		private int lengthOfWalkingSquareY;

		private int squarePauseAccumulation;

		private int squarePauseTotal;

		private int squarePauseOffset;

		public Microsoft.Xna.Framework.Rectangle lastCrossroad;

		private Texture2D portrait;

		private Vector2 nextSquarePosition;

		protected int shakeTimer;

		private bool isWalkingInSquare;

		private readonly NetBool isWalkingTowardPlayer = new NetBool();

		protected string textAboveHead;

		protected int textAboveHeadPreTimer;

		protected int textAboveHeadTimer;

		protected int textAboveHeadStyle;

		protected int textAboveHeadColor;

		protected float textAboveHeadAlpha;

		public int daysAfterLastBirth = -1;

		private string extraDialogueMessageToAddThisMorning;

		[XmlElement("birthday_Season")]
		public readonly NetString birthday_Season = new NetString();

		[XmlElement("birthday_Day")]
		public readonly NetInt birthday_Day = new NetInt();

		[XmlElement("age")]
		public readonly NetInt age = new NetInt();

		[XmlElement("manners")]
		public readonly NetInt manners = new NetInt();

		[XmlElement("socialAnxiety")]
		public readonly NetInt socialAnxiety = new NetInt();

		[XmlElement("optimism")]
		public readonly NetInt optimism = new NetInt();

		[XmlElement("gender")]
		public readonly NetInt gender = new NetInt();

		[XmlIgnore]
		public readonly NetBool breather = new NetBool(value: true);

		[XmlIgnore]
		public readonly NetBool isSleeping = new NetBool(value: false);

		[XmlElement("sleptInBed")]
		public readonly NetBool sleptInBed = new NetBool(value: true);

		[XmlIgnore]
		public readonly NetBool hideShadow = new NetBool();

		[XmlElement("isInvisible")]
		public readonly NetBool isInvisible = new NetBool(value: false);

		[XmlElement("lastSeenMovieWeek")]
		public readonly NetInt lastSeenMovieWeek = new NetInt(-1);

		[XmlIgnore]
		public readonly NetString syncedPortraitPath = new NetString();

		public bool datingFarmer;

		public bool divorcedFromFarmer;

		[XmlElement("datable")]
		public readonly NetBool datable = new NetBool();

		[XmlIgnore]
		public bool uniqueSpriteActive;

		[XmlIgnore]
		public bool uniquePortraitActive;

		[XmlIgnore]
		public bool updatedDialogueYet;

		[XmlIgnore]
		public bool immediateSpeak;

		[XmlIgnore]
		public bool ignoreScheduleToday;

		protected int defaultFacingDirection;

		[XmlElement("defaultPosition")]
		private readonly NetVector2 defaultPosition = new NetVector2();

		[XmlElement("defaultMap")]
		public readonly NetString defaultMap = new NetString();

		public string loveInterest;

		protected int idForClones = -1;

		public int id = -1;

		public int homeRegion;

		public int daysUntilNotInvisible;

		public bool followSchedule = true;

		[XmlIgnore]
		public PathFindController temporaryController;

		[XmlElement("moveTowardPlayerThreshold")]
		public readonly NetInt moveTowardPlayerThreshold = new NetInt();

		[XmlIgnore]
		public float rotation;

		[XmlIgnore]
		public float yOffset;

		[XmlIgnore]
		public float swimTimer;

		[XmlIgnore]
		public float timerSinceLastMovement;

		[XmlIgnore]
		public string mapBeforeEvent;

		[XmlIgnore]
		public Vector2 positionBeforeEvent;

		[XmlIgnore]
		public Vector2 lastPosition;

		[XmlIgnore]
		public float currentScheduleDelay;

		[XmlIgnore]
		public float scheduleDelaySeconds;

		[XmlIgnore]
		public bool layingDown;

		[XmlIgnore]
		public Vector2 appliedRouteAnimationOffset = Vector2.Zero;

		[XmlIgnore]
		public string[] routeAnimationMetadata;

		[XmlElement("hasSaidAfternoonDialogue")]
		private NetBool hasSaidAfternoonDialogue = new NetBool(value: false);

		[XmlIgnore]
		public static bool hasSomeoneWateredCrops;

		[XmlIgnore]
		public static bool hasSomeoneFedThePet;

		[XmlIgnore]
		public static bool hasSomeoneFedTheAnimals;

		[XmlIgnore]
		public static bool hasSomeoneRepairedTheFences = false;

		[XmlIgnore]
		protected bool _skipRouteEndIntro;

		[NonInstancedStatic]
		public static HashSet<string> invalidDialogueFiles = new HashSet<string>();

		[XmlIgnore]
		protected bool _hasLoadedMasterScheduleData;

		[XmlIgnore]
		protected Dictionary<string, string> _masterScheduleData;

		[XmlIgnore]
		protected string _lastLoadedScheduleKey;

		[XmlIgnore]
		public readonly NetList<MarriageDialogueReference, NetRef<MarriageDialogueReference>> currentMarriageDialogue = new NetList<MarriageDialogueReference, NetRef<MarriageDialogueReference>>();

		public readonly NetBool hasBeenKissedToday = new NetBool(value: false);

		[XmlIgnore]
		public readonly NetRef<MarriageDialogueReference> marriageDefaultDialogue = new NetRef<MarriageDialogueReference>(null);

		[XmlIgnore]
		public readonly NetBool shouldSayMarriageDialogue = new NetBool(value: false);

		[XmlIgnore]
		public readonly NetBool exploreFarm = new NetBool(value: false);

		[XmlIgnore]
		public float nextFarmActivityScan;

		[XmlIgnore]
		protected List<FarmActivity> _farmActivities;

		[XmlIgnore]
		protected float _farmActivityWeightTotal;

		[XmlIgnore]
		protected FarmActivity _currentFarmActivity;

		public readonly NetEvent0 removeHenchmanEvent = new NetEvent0();

		private bool isPlayingSleepingAnimation;

		public readonly NetBool shouldPlayRobinHammerAnimation = new NetBool();

		private bool isPlayingRobinHammerAnimation;

		public readonly NetBool shouldPlaySpousePatioAnimation = new NetBool();

		private bool isPlayingSpousePatioAnimation = new NetBool();

		public readonly NetBool shouldWearIslandAttire = new NetBool();

		private bool isWearingIslandAttire;

		public readonly NetBool isMovingOnPathFindPath = new NetBool();

		public List<KeyValuePair<int, SchedulePathDescription>> queuedSchedulePaths = new List<KeyValuePair<int, SchedulePathDescription>>();

		public int lastAttemptedSchedule = -1;

		[XmlIgnore]
		public readonly NetBool doingEndOfRouteAnimation = new NetBool();

		private bool currentlyDoingEndOfRouteAnimation;

		[XmlIgnore]
		public readonly NetBool goingToDoEndOfRouteAnimation = new NetBool();

		[XmlIgnore]
		public readonly NetString endOfRouteMessage = new NetString();

		[XmlElement("dayScheduleName")]
		public readonly NetString dayScheduleName = new NetString();

		[XmlElement("islandScheduleName")]
		public readonly NetString islandScheduleName = new NetString();

		private int[] routeEndIntro;

		private int[] routeEndAnimation;

		private int[] routeEndOutro;

		[XmlIgnore]
		public string nextEndOfRouteMessage;

		private string loadedEndOfRouteBehavior;

		[XmlIgnore]
		protected string _startedEndOfRouteBehavior;

		[XmlIgnore]
		protected string _finishingEndOfRouteBehavior;

		[XmlIgnore]
		protected int _beforeEndOfRouteAnimationFrame;

		public readonly NetString endOfRouteBehaviorName = new NetString();

		private Point previousEndPoint;

		protected int scheduleTimeToTry = 9999999;

		public int squareMovementFacingPreference;

		public const int NO_TRY = 9999999;

		private bool returningToEndPoint;

		private string nameOfTodaysSchedule = "";

		[XmlIgnore]
		public SchedulePathDescription DirectionsToNewLocation
		{
			get
			{
				return directionsToNewLocation;
			}
			set
			{
				directionsToNewLocation = value;
			}
		}

		[XmlIgnore]
		public int DirectionIndex
		{
			get
			{
				return directionIndex;
			}
			set
			{
				directionIndex = value;
			}
		}

		public int DefaultFacingDirection
		{
			get
			{
				return defaultFacingDirection;
			}
			set
			{
				defaultFacingDirection = value;
			}
		}

		[XmlIgnore]
		public Dictionary<string, string> Dialogue
		{
			get
			{
				if (this is Monster)
				{
					return null;
				}
				if (this is Pet)
				{
					return null;
				}
				if (this is Horse)
				{
					return null;
				}
				if (this is Child)
				{
					return null;
				}
				if (dialogue == null)
				{
					string text = "Characters\\Dialogue\\" + GetDialogueSheetName();
					if (invalidDialogueFiles.Contains(text))
					{
						dialogue = new Dictionary<string, string>();
					}
					try
					{
						dialogue = Game1.content.Load<Dictionary<string, string>>(text).Select(delegate(KeyValuePair<string, string> pair)
						{
							string key = pair.Key;
							string text2 = pair.Value;
							if (text2.Contains("¦"))
							{
								text2 = ((!Game1.player.IsMale) ? text2.Substring(text2.IndexOf("¦") + 1) : text2.Substring(0, text2.IndexOf("¦")));
							}
							return new KeyValuePair<string, string>(key, text2);
						}).ToDictionary((KeyValuePair<string, string> p) => p.Key, (KeyValuePair<string, string> p) => p.Value);
					}
					catch (ContentLoadException)
					{
						invalidDialogueFiles.Add(text);
						dialogue = new Dictionary<string, string>();
					}
				}
				return dialogue;
			}
		}

		public string DefaultMap
		{
			get
			{
				return defaultMap.Value;
			}
			set
			{
				defaultMap.Value = value;
			}
		}

		public Vector2 DefaultPosition
		{
			get
			{
				return defaultPosition.Value;
			}
			set
			{
				defaultPosition.Value = value;
			}
		}

		[XmlIgnore]
		public Texture2D Portrait
		{
			get
			{
				if (portrait == null)
				{
					try
					{
						string text = ((!string.IsNullOrEmpty(syncedPortraitPath.Value)) ? ((string)syncedPortraitPath) : ("Portraits\\" + getTextureName()));
						if (isWearingIslandAttire)
						{
							try
							{
								portrait = Game1.content.Load<Texture2D>(text + "_Beach");
							}
							catch (ContentLoadException)
							{
								portrait = null;
							}
						}
						if (portrait == null)
						{
							portrait = Game1.content.Load<Texture2D>(text);
						}
					}
					catch (ContentLoadException)
					{
						portrait = null;
					}
				}
				return portrait;
			}
			set
			{
				portrait = value;
			}
		}

		[XmlIgnore]
		public Dictionary<int, SchedulePathDescription> Schedule
		{
			get
			{
				return schedule;
			}
			set
			{
				schedule = value;
			}
		}

		public bool IsWalkingInSquare
		{
			get
			{
				return isWalkingInSquare;
			}
			set
			{
				isWalkingInSquare = value;
			}
		}

		public bool IsWalkingTowardPlayer
		{
			get
			{
				return isWalkingTowardPlayer;
			}
			set
			{
				isWalkingTowardPlayer.Value = value;
			}
		}

		[XmlIgnore]
		public Stack<Dialogue> CurrentDialogue
		{
			get
			{
				Stack<Dialogue> value = null;
				if (Game1.npcDialogues == null)
				{
					Game1.npcDialogues = new Dictionary<string, Stack<Dialogue>>();
				}
				Game1.npcDialogues.TryGetValue(base.Name, out value);
				if (value == null)
				{
					Stack<Dialogue> stack2 = (Game1.npcDialogues[base.Name] = loadCurrentDialogue());
					value = stack2;
				}
				return value;
			}
			set
			{
				if (Game1.npcDialogues != null)
				{
					Game1.npcDialogues[base.Name] = value;
				}
			}
		}

		[XmlIgnore]
		public string Birthday_Season
		{
			get
			{
				return birthday_Season;
			}
			set
			{
				birthday_Season.Value = value;
			}
		}

		[XmlIgnore]
		public int Birthday_Day
		{
			get
			{
				return birthday_Day;
			}
			set
			{
				birthday_Day.Value = value;
			}
		}

		[XmlIgnore]
		public int Age
		{
			get
			{
				return age;
			}
			set
			{
				age.Value = value;
			}
		}

		[XmlIgnore]
		public int Manners
		{
			get
			{
				return manners;
			}
			set
			{
				manners.Value = value;
			}
		}

		[XmlIgnore]
		public int SocialAnxiety
		{
			get
			{
				return socialAnxiety;
			}
			set
			{
				socialAnxiety.Value = value;
			}
		}

		[XmlIgnore]
		public int Optimism
		{
			get
			{
				return optimism;
			}
			set
			{
				optimism.Value = value;
			}
		}

		[XmlIgnore]
		public int Gender
		{
			get
			{
				return gender;
			}
			set
			{
				gender.Value = value;
			}
		}

		[XmlIgnore]
		public bool Breather
		{
			get
			{
				return breather;
			}
			set
			{
				breather.Value = value;
			}
		}

		[XmlIgnore]
		public bool HideShadow
		{
			get
			{
				return hideShadow;
			}
			set
			{
				hideShadow.Value = value;
			}
		}

		[XmlIgnore]
		public bool HasPartnerForDance
		{
			get
			{
				foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
				{
					if (onlineFarmer.dancePartner.TryGetVillager() == this)
					{
						return true;
					}
				}
				return false;
			}
		}

		[XmlIgnore]
		public bool IsInvisible
		{
			get
			{
				return isInvisible;
			}
			set
			{
				isInvisible.Value = value;
			}
		}

		public virtual bool CanSocialize
		{
			get
			{
				if (base.Name.Equals("Leo") && !Game1.MasterPlayer.mailReceived.Contains("addedParrotBoy"))
				{
					return false;
				}
				if (base.Name.Equals("Sandy") && !Game1.MasterPlayer.mailReceived.Contains("ccVault"))
				{
					return false;
				}
				if (base.Name.Equals("???") || base.Name.Equals("Bouncer") || base.Name.Equals("Marlon") || base.Name.Equals("Gil") || base.Name.Equals("Gunther") || base.Name.Equals("Henchman") || base.Name.Equals("Birdie") || IsMonster || this is Horse || this is Pet || this is JunimoHarvester)
				{
					return false;
				}
				if (base.Name.Equals("Dwarf") || base.Name.Contains("Qi") || this is Pet || this is Horse || this is Junimo)
				{
					return false;
				}
				if (base.Name.Equals("Krobus"))
				{
					return Game1.player.friendshipData.ContainsKey("Krobus");
				}
				return true;
			}
		}

		public NPC()
		{
		}

		public NPC(AnimatedSprite sprite, Vector2 position, int facingDir, string name, LocalizedContentManager content = null)
			: base(sprite, position, 2, name)
		{
			faceDirection(facingDir);
			defaultPosition.Value = position;
			defaultFacingDirection = facingDir;
			lastCrossroad = new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y + 64, 64, 64);
			if (content != null)
			{
				try
				{
					portrait = content.Load<Texture2D>("Portraits\\" + name);
				}
				catch (Exception)
				{
				}
			}
		}

		public NPC(AnimatedSprite sprite, Vector2 position, string defaultMap, int facingDirection, string name, bool datable, Dictionary<int, int[]> schedule, Texture2D portrait)
			: this(sprite, position, defaultMap, facingDirection, name, schedule, portrait, eventActor: false)
		{
			this.datable.Value = datable;
		}

		public NPC(AnimatedSprite sprite, Vector2 position, string defaultMap, int facingDir, string name, Dictionary<int, int[]> schedule, Texture2D portrait, bool eventActor, string syncedPortraitPath = null)
			: base(sprite, position, 2, name)
		{
			this.portrait = portrait;
			this.syncedPortraitPath.Value = syncedPortraitPath;
			faceDirection(facingDir);
			if (!eventActor)
			{
				lastCrossroad = new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y + 64, 64, 64);
			}
			reloadData();
			defaultPosition.Value = position;
			this.defaultMap.Value = defaultMap;
			base.currentLocation = Game1.getLocationFromName(defaultMap);
			defaultFacingDirection = facingDir;
		}

		public virtual void reloadData()
		{
			try
			{
				Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
				if (this is Child || !dictionary.ContainsKey(name))
				{
					return;
				}
				string[] array = dictionary[name].Split('/');
				string text = array[0];
				if (!(text == "teen"))
				{
					if (text == "child")
					{
						Age = 2;
					}
				}
				else
				{
					Age = 1;
				}
				string text2 = array[1];
				if (!(text2 == "rude"))
				{
					if (text2 == "polite")
					{
						Manners = 1;
					}
				}
				else
				{
					Manners = 2;
				}
				string text3 = array[2];
				if (!(text3 == "shy"))
				{
					if (text3 == "outgoing")
					{
						SocialAnxiety = 0;
					}
				}
				else
				{
					SocialAnxiety = 1;
				}
				string text4 = array[3];
				if (!(text4 == "positive"))
				{
					if (text4 == "negative")
					{
						Optimism = 1;
					}
				}
				else
				{
					Optimism = 0;
				}
				string text5 = array[4];
				if (!(text5 == "female"))
				{
					if (text5 == "undefined")
					{
						Gender = 2;
					}
				}
				else
				{
					Gender = 1;
				}
				string text6 = array[5];
				if (!(text6 == "datable"))
				{
					if (text6 == "not-datable")
					{
						datable.Value = false;
					}
				}
				else
				{
					datable.Value = true;
				}
				loveInterest = array[6];
				switch (array[7])
				{
				case "Desert":
					homeRegion = 1;
					break;
				case "Other":
					homeRegion = 0;
					break;
				case "Town":
					homeRegion = 2;
					break;
				}
				if (array.Length > 8 && array[8].Length > 0)
				{
					Birthday_Season = array[8].Split(' ')[0];
					Birthday_Day = Convert.ToInt32(array[8].Split(' ')[1]);
				}
				for (int i = 0; i < dictionary.Count; i++)
				{
					if (dictionary.ElementAt(i).Key.Equals(name))
					{
						id = i;
						break;
					}
				}
				if (!isMarried())
				{
					reloadDefaultLocation();
				}
				displayName = array[11];
			}
			catch (Exception)
			{
			}
		}

		public virtual void reloadDefaultLocation()
		{
			try
			{
				Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
				if (dictionary.ContainsKey(name))
				{
					string[] array = dictionary[base.Name].Split('/');
					string[] array2 = array[10].Split(' ');
					DefaultMap = array2[0];
					DefaultPosition = new Vector2(Convert.ToInt32(array2[1]) * 64, Convert.ToInt32(array2[2]) * 64);
				}
			}
			catch (Exception)
			{
			}
		}

		public virtual bool canTalk()
		{
			return true;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(birthday_Season, birthday_Day, datable, shouldPlayRobinHammerAnimation, shouldPlaySpousePatioAnimation, isWalkingTowardPlayer, moveTowardPlayerThreshold, age, manners, socialAnxiety, optimism, gender, breather, isSleeping, hideShadow, isInvisible, defaultMap, defaultPosition, removeHenchmanEvent, doingEndOfRouteAnimation, goingToDoEndOfRouteAnimation, endOfRouteMessage, endOfRouteBehaviorName, lastSeenMovieWeek, currentMarriageDialogue, marriageDefaultDialogue, shouldSayMarriageDialogue, hasBeenKissedToday, syncedPortraitPath, hasSaidAfternoonDialogue, dayScheduleName, islandScheduleName, sleptInBed, shouldWearIslandAttire, isMovingOnPathFindPath);
			position.Field.AxisAlignedMovement = true;
			removeHenchmanEvent.onEvent += performRemoveHenchman;
		}

		protected override string translateName(string name)
		{
			if (name != null)
			{
				switch (name.Length)
				{
				case 7:
					switch (name[1])
					{
					case 'u':
						if (!(name == "Gunther"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\NPCNames:Gunther");
					case 'o':
						if (!(name == "Bouncer"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\NPCNames:Bouncer");
					case 'e':
						if (!(name == "Welwick"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\NPCNames:Welwick");
					case 'r':
						if (!(name == "Grandpa"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\NPCNames:Grandpa");
					}
					break;
				case 3:
					switch (name[0])
					{
					case 'G':
						if (!(name == "Gil"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\NPCNames:Gil");
					case 'K':
						if (!(name == "Kel"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\NPCNames:Kel");
					}
					break;
				case 6:
					switch (name[1])
					{
					case 'o':
						if (!(name == "Morris"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\NPCNames:Morris");
					case 'a':
						if (!(name == "Marlon"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\NPCNames:Marlon");
					case 'i':
						if (!(name == "Birdie"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\NPCNames:Birdie");
					}
					break;
				case 8:
					switch (name[0])
					{
					case 'H':
						if (!(name == "Henchman"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\NPCNames:Henchman");
					case 'G':
						if (!(name == "Governor"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\NPCNames:Governor");
					}
					break;
				case 11:
					if (!(name == "Old Mariner"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\NPCNames:OldMariner");
				case 9:
					if (!(name == "Mister Qi"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\NPCNames:MisterQi");
				case 4:
					if (!(name == "Bear"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\NPCNames:Bear");
				}
			}
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			if (dictionary.ContainsKey(name))
			{
				string[] array = dictionary[name].Split('/');
				return array[array.Length - 1];
			}
			return name;
		}

		public string getName()
		{
			if (displayName != null && displayName.Length > 0)
			{
				return displayName;
			}
			return base.Name;
		}

		public string getTextureName()
		{
			return getTextureNameForCharacter(base.Name);
		}

		public static string getTextureNameForCharacter(string character_name)
		{
			string result = character_name switch
			{
				"Old Mariner" => "Mariner", 
				"Dwarf King" => "DwarfKing", 
				"Mister Qi" => "MrQi", 
				"???" => "Monsters\\Shadow Guy", 
				"Leo" => "ParrotBoy", 
				_ => character_name, 
			};
			if (character_name.Equals(Utility.getOtherFarmerNames()[0]))
			{
				result = (Game1.player.IsMale ? "maleRival" : "femaleRival");
			}
			return result;
		}

		public virtual bool PathToOnFarm(Point destination, PathFindController.endBehavior on_path_success = null)
		{
			controller = null;
			Stack<Point> stack = PathFindController.FindPathOnFarm(getTileLocationPoint(), destination, base.currentLocation, 2000);
			if (stack != null)
			{
				controller = new PathFindController(stack, this, base.currentLocation);
				controller.nonDestructivePathing = true;
				ignoreScheduleToday = true;
				PathFindController pathFindController = controller;
				pathFindController.endBehaviorFunction = (PathFindController.endBehavior)Delegate.Combine(pathFindController.endBehaviorFunction, on_path_success);
				return true;
			}
			return false;
		}

		public virtual void OnFinishPathForActivity(Character c, GameLocation location)
		{
			_currentFarmActivity.BeginActivity();
		}

		public void resetPortrait()
		{
			portrait = null;
		}

		public void resetSeasonalDialogue()
		{
			dialogue = null;
		}

		public void findRightSchedule()
		{
			bool flag = false;
			SchedulePathDescription value = null;
			int timeOfDay = Game1.timeOfDay;
			if (schedule != null)
			{
				for (int num = timeOfDay; num > 600; num -= 10)
				{
					schedule.TryGetValue(num, out value);
					if (value != null)
					{
						flag = true;
						break;
					}
				}
			}
			if (value == null || value.route == null)
			{
				return;
			}
			int num2 = 0;
			Stack<string> stack = new Stack<string>(value.locationNames);
			string[] array = new string[stack.Count];
			for (int num3 = value.locationNames.Count - 1; num3 >= 0; num3--)
			{
				array[num3] = stack.Pop();
			}
			float x = getTileLocation().X;
			float y = getTileLocation().Y;
			string text = base.currentLocation.Name;
			if (isMarried() && text == "FarmHouse")
			{
				base.currentLocation = Game1.getLocationFromName("BusStop");
			}
			foreach (Point item in value.route)
			{
				string text2 = "";
				if (array.Count() > num2)
				{
					text2 = array[num2];
				}
				if (text2 != null && text2 != "" && text2 == base.currentLocation.Name && (float)item.X == getTileLocation().X && (float)item.Y == getTileLocation().Y)
				{
					break;
				}
				num2++;
			}
			if (num2 == value.route.Count)
			{
				num2--;
			}
			Point point = default(Point);
			if (num2 > 0)
			{
				for (int i = 0; i < num2; i++)
				{
					point = value.route.Pop();
					if (value.locationNames.Count > 0)
					{
						value.locationNames.Pop();
					}
				}
				setTileLocation(new Vector2(point.X, point.Y));
			}
			directionsToNewLocation = value;
			prepareToDisembarkOnNewSchedulePath();
			if (schedule != null)
			{
				if (isWalkingInSquare)
				{
					returnToEndPoint();
				}
				controller = new PathFindController(directionsToNewLocation.route, this, Utility.getGameLocationOfCharacter(this))
				{
					finalFacingDirection = directionsToNewLocation.facingDirection,
					endBehaviorFunction = getRouteEndBehaviorFunction(directionsToNewLocation.endOfRouteBehavior, directionsToNewLocation.endOfRouteMessage)
				};
				lastCrossroad = new Microsoft.Xna.Framework.Rectangle(getStandingX() - getStandingX() % 64, getStandingY() - getStandingY() % 64, 64, 64);
				scheduleTimeToTry = 9999999;
				if (directionsToNewLocation != null && directionsToNewLocation.route != null)
				{
					previousEndPoint = ((directionsToNewLocation.route.Count > 0) ? directionsToNewLocation.route.Last() : Point.Zero);
				}
			}
		}

		public virtual void reloadSprite()
		{
			string textureName = getTextureName();
			if (!IsMonster)
			{
				Sprite = new AnimatedSprite("Characters\\" + textureName);
				if (!base.Name.Contains("Dwarf") && !base.Name.Equals("Krobus"))
				{
					Sprite.SpriteHeight = 32;
				}
			}
			else
			{
				Sprite = new AnimatedSprite("Monsters\\" + textureName);
			}
			resetPortrait();
			_ = IsInvisible;
			if (!Game1.newDay && Game1.gameMode != 6)
			{
				return;
			}
			faceDirection(DefaultFacingDirection);
			previousEndPoint = new Point((int)defaultPosition.X / 64, (int)defaultPosition.Y / 64);
			if (SaveGame.emergencyBackupRestore)
			{
				SaveGame.setEmergencyDayAndTime();
				Schedule = getSchedule(Game1.dayOfMonth);
				previousEndPoint = getTileLocationPoint();
				findRightSchedule();
				string text = base.Name;
				bool flag = false;
				if (base.Name.Equals("Maru"))
				{
					if (base.currentLocation.Name.Equals("Hospital"))
					{
						text = "Maru_Hospital";
						flag = true;
					}
					else if (base.currentLocation.Name.Equals("Town"))
					{
						text = "Maru";
						flag = true;
					}
				}
				else if (base.Name.Equals("Shane"))
				{
					if (base.currentLocation.Name.Equals("JojaMart"))
					{
						text = "Shane_JojaMart";
						flag = true;
					}
					else if (base.currentLocation.Name.Equals("Town"))
					{
						text = "Shane";
						flag = true;
					}
				}
				if (flag)
				{
					Sprite.LoadTexture("Characters\\" + text);
				}
			}
			else
			{
				Schedule = getSchedule(Game1.dayOfMonth);
			}
			faceDirection(defaultFacingDirection);
			resetSeasonalDialogue();
			resetCurrentDialogue();
			if (isMarried() && !getSpouse().divorceTonight && !IsInvisible)
			{
				marriageDuties();
			}
			updateConstructionAnimation();
			try
			{
				displayName = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions")[base.Name].Split('/')[11];
			}
			catch (Exception)
			{
			}
		}

		private void updateConstructionAnimation()
		{
			bool flag = Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason);
			if (Game1.IsMasterGame && base.Name == "Robin" && !flag)
			{
				if ((int)Game1.player.daysUntilHouseUpgrade > 0)
				{
					Farm farm = Game1.getFarm();
					Game1.warpCharacter(this, "Farm", new Vector2(farm.GetMainFarmHouseEntry().X + 4, farm.GetMainFarmHouseEntry().Y - 1));
					isPlayingRobinHammerAnimation = false;
					shouldPlayRobinHammerAnimation.Value = true;
					return;
				}
				if (Game1.getFarm().isThereABuildingUnderConstruction())
				{
					Building buildingUnderConstruction = Game1.getFarm().getBuildingUnderConstruction();
					if ((int)buildingUnderConstruction.daysUntilUpgrade > 0 && buildingUnderConstruction.indoors.Value != null)
					{
						if (base.currentLocation != null)
						{
							base.currentLocation.characters.Remove(this);
						}
						base.currentLocation = buildingUnderConstruction.indoors;
						if (base.currentLocation != null && !base.currentLocation.characters.Contains(this))
						{
							base.currentLocation.addCharacter(this);
						}
						if (buildingUnderConstruction.nameOfIndoorsWithoutUnique.Contains("Shed"))
						{
							setTilePosition(2, 2);
							position.X -= 28f;
						}
						else
						{
							setTilePosition(1, 5);
						}
					}
					else
					{
						Game1.warpCharacter(this, "Farm", new Vector2((int)buildingUnderConstruction.tileX + (int)buildingUnderConstruction.tilesWide / 2, (int)buildingUnderConstruction.tileY + (int)buildingUnderConstruction.tilesHigh / 2));
						position.X += 16f;
						position.Y -= 32f;
					}
					isPlayingRobinHammerAnimation = false;
					shouldPlayRobinHammerAnimation.Value = true;
					return;
				}
				if ((Game1.getLocationFromName("Town") as Town).daysUntilCommunityUpgrade.Value > 0)
				{
					if (Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
					{
						Game1.warpCharacter(this, "Backwoods", new Vector2(41f, 23f));
						isPlayingRobinHammerAnimation = false;
						shouldPlayRobinHammerAnimation.Value = true;
					}
					else
					{
						Game1.warpCharacter(this, "Town", new Vector2(77f, 68f));
						isPlayingRobinHammerAnimation = false;
						shouldPlayRobinHammerAnimation.Value = true;
					}
					return;
				}
			}
			shouldPlayRobinHammerAnimation.Value = false;
		}

		private void doPlayRobinHammerAnimation()
		{
			Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(24, 75),
				new FarmerSprite.AnimationFrame(25, 75),
				new FarmerSprite.AnimationFrame(26, 300, secondaryArm: false, flip: false, robinHammerSound),
				new FarmerSprite.AnimationFrame(27, 1000, secondaryArm: false, flip: false, robinVariablePause)
			});
			ignoreScheduleToday = true;
			bool flag = (int)Game1.player.daysUntilHouseUpgrade == 1 || (int)(Game1.getLocationFromName("Town") as Town).daysUntilCommunityUpgrade == 1;
			CurrentDialogue.Clear();
			CurrentDialogue.Push(new Dialogue(flag ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3927") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3926"), this));
		}

		public void showTextAboveHead(string Text, int spriteTextColor = -1, int style = 2, int duration = 3000, int preTimer = 0)
		{
			textAboveHeadAlpha = 0f;
			textAboveHead = Text;
			textAboveHeadPreTimer = preTimer;
			textAboveHeadTimer = duration;
			textAboveHeadStyle = style;
			textAboveHeadColor = spriteTextColor;
		}

		public void moveToNewPlaceForEvent(int xTile, int yTile, string oldMap)
		{
			mapBeforeEvent = oldMap;
			positionBeforeEvent = base.Position;
			base.Position = new Vector2(xTile * 64, yTile * 64 - 96);
		}

		public virtual bool hitWithTool(Tool t)
		{
			return false;
		}

		public bool canReceiveThisItemAsGift(Item i)
		{
			if (i is Object || i is Ring || i is Hat || i is Boots || i is MeleeWeapon || i is Clothing)
			{
				return true;
			}
			return false;
		}

		public int getGiftTasteForThisItem(Item item)
		{
			int num = 8;
			if (item is Object)
			{
				Object @object = item as Object;
				Game1.NPCGiftTastes.TryGetValue(base.Name, out var value);
				string[] array = value.Split('/');
				int parentSheetIndex = @object.ParentSheetIndex;
				int category = @object.Category;
				string value2 = parentSheetIndex.ToString() ?? "";
				string value3 = category.ToString() ?? "";
				if (Game1.NPCGiftTastes["Universal_Love"].Split(' ').Contains(value3))
				{
					num = 0;
				}
				else if (Game1.NPCGiftTastes["Universal_Hate"].Split(' ').Contains(value3))
				{
					num = 6;
				}
				else if (Game1.NPCGiftTastes["Universal_Like"].Split(' ').Contains(value3))
				{
					num = 2;
				}
				else if (Game1.NPCGiftTastes["Universal_Dislike"].Split(' ').Contains(value3))
				{
					num = 4;
				}
				if (CheckTasteContextTags(item, Game1.NPCGiftTastes["Universal_Love"].Split(' ')))
				{
					num = 0;
				}
				else if (CheckTasteContextTags(item, Game1.NPCGiftTastes["Universal_Hate"].Split(' ')))
				{
					num = 6;
				}
				else if (CheckTasteContextTags(item, Game1.NPCGiftTastes["Universal_Like"].Split(' ')))
				{
					num = 2;
				}
				else if (CheckTasteContextTags(item, Game1.NPCGiftTastes["Universal_Dislike"].Split(' ')))
				{
					num = 4;
				}
				bool flag = false;
				bool flag2 = false;
				if (Game1.NPCGiftTastes["Universal_Love"].Split(' ').Contains(value2))
				{
					num = 0;
					flag = true;
				}
				else if (Game1.NPCGiftTastes["Universal_Hate"].Split(' ').Contains(value2))
				{
					num = 6;
					flag = true;
				}
				else if (Game1.NPCGiftTastes["Universal_Like"].Split(' ').Contains(value2))
				{
					num = 2;
					flag = true;
				}
				else if (Game1.NPCGiftTastes["Universal_Dislike"].Split(' ').Contains(value2))
				{
					num = 4;
					flag = true;
				}
				else if (Game1.NPCGiftTastes["Universal_Neutral"].Split(' ').Contains(value2))
				{
					num = 8;
					flag = true;
					flag2 = true;
				}
				if (@object.type.Contains("Arch"))
				{
					num = 4;
					if (base.Name.Equals("Penny") || name.Equals("Dwarf"))
					{
						num = 2;
					}
				}
				if (num == 8 && !flag2)
				{
					if ((int)@object.edibility != -300 && (int)@object.edibility < 0)
					{
						num = 6;
					}
					else if ((int)@object.price < 20)
					{
						num = 4;
					}
				}
				if (value != null)
				{
					List<string[]> list = new List<string[]>();
					for (int i = 0; i < 10; i += 2)
					{
						string[] array2 = array[i + 1].Split(' ');
						string[] array3 = new string[array2.Length];
						for (int j = 0; j < array2.Length; j++)
						{
							if (array2[j].Length > 0)
							{
								array3[j] = array2[j];
							}
						}
						list.Add(array3);
					}
					if (list[0].Contains(value2))
					{
						return 0;
					}
					if (list[3].Contains(value2))
					{
						return 6;
					}
					if (list[1].Contains(value2))
					{
						return 2;
					}
					if (list[2].Contains(value2))
					{
						return 4;
					}
					if (list[4].Contains(value2))
					{
						return 8;
					}
					if (CheckTasteContextTags(item, list[0]))
					{
						return 0;
					}
					if (CheckTasteContextTags(item, list[3]))
					{
						return 6;
					}
					if (CheckTasteContextTags(item, list[1]))
					{
						return 2;
					}
					if (CheckTasteContextTags(item, list[2]))
					{
						return 4;
					}
					if (CheckTasteContextTags(item, list[4]))
					{
						return 8;
					}
					if (!flag)
					{
						if (category != 0 && list[0].Contains(value3))
						{
							return 0;
						}
						if (category != 0 && list[3].Contains(value3))
						{
							return 6;
						}
						if (category != 0 && list[1].Contains(value3))
						{
							return 2;
						}
						if (category != 0 && list[2].Contains(value3))
						{
							return 4;
						}
						if (category != 0 && list[4].Contains(value3))
						{
							return 8;
						}
					}
				}
			}
			return num;
		}

		public virtual bool CheckTasteContextTags(Item item, string[] list)
		{
			foreach (string text in list)
			{
				if (text != null && text.Length > 0 && !char.IsNumber(text[0]) && text[0] != '-' && item.HasContextTag(text))
				{
					return true;
				}
			}
			return false;
		}

		private void goblinDoorEndBehavior(Character c, GameLocation l)
		{
			l.characters.Remove(this);
			l.playSound("doorClose");
		}

		private void performRemoveHenchman()
		{
			Sprite.CurrentFrame = 4;
			Game1.netWorldState.Value.IsGoblinRemoved = true;
			Game1.player.removeQuest(27);
			Stack<Point> stack = new Stack<Point>();
			stack.Push(new Point(20, 21));
			stack.Push(new Point(20, 22));
			stack.Push(new Point(20, 23));
			stack.Push(new Point(20, 24));
			stack.Push(new Point(20, 25));
			stack.Push(new Point(20, 26));
			stack.Push(new Point(20, 27));
			stack.Push(new Point(20, 28));
			base.addedSpeed = 2;
			controller = new PathFindController(stack, this, base.currentLocation);
			controller.endBehaviorFunction = goblinDoorEndBehavior;
			showTextAboveHead(Game1.content.LoadString("Strings\\Characters:Henchman6"));
			Game1.player.mailReceived.Add("henchmanGone");
			base.currentLocation.removeTile(20, 29, "Buildings");
		}

		private void engagementResponse(Farmer who, bool asRoommate = false)
		{
			Game1.changeMusicTrack("none");
			who.spouse = base.Name;
			if (!asRoommate)
			{
				Game1.multiplayer.globalChatInfoMessage("Engaged", Game1.player.Name, displayName);
			}
			Friendship friendship = who.friendshipData[base.Name];
			friendship.Status = FriendshipStatus.Engaged;
			friendship.RoommateMarriage = asRoommate;
			WorldDate worldDate = new WorldDate(Game1.Date);
			worldDate.TotalDays += 3;
			while (!Game1.canHaveWeddingOnDay(worldDate.DayOfMonth, worldDate.Season))
			{
				worldDate.TotalDays++;
			}
			friendship.WeddingDate = worldDate;
			CurrentDialogue.Clear();
			if (asRoommate && Game1.content.Load<Dictionary<string, string>>("Data\\EngagementDialogue").ContainsKey(base.Name + "Roommate0"))
			{
				CurrentDialogue.Push(new Dialogue(Game1.content.Load<Dictionary<string, string>>("Data\\EngagementDialogue")[base.Name + "Roommate0"], this));
				string text = Game1.content.LoadStringReturnNullIfNotFound("Strings\\StringsFromCSFiles:" + base.Name + "_EngagedRoommate");
				if (text != null)
				{
					CurrentDialogue.Push(new Dialogue(text, this));
				}
				else
				{
					text = Game1.content.LoadStringReturnNullIfNotFound("Strings\\StringsFromCSFiles:" + base.Name + "_Engaged");
					if (text != null)
					{
						CurrentDialogue.Push(new Dialogue(text, this));
					}
					else
					{
						CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3980"), this));
					}
				}
			}
			else
			{
				if (Game1.content.Load<Dictionary<string, string>>("Data\\EngagementDialogue").ContainsKey(base.Name + "0"))
				{
					CurrentDialogue.Push(new Dialogue(Game1.content.Load<Dictionary<string, string>>("Data\\EngagementDialogue")[base.Name + "0"], this));
				}
				string text2 = Game1.content.LoadStringReturnNullIfNotFound("Strings\\StringsFromCSFiles:" + base.Name + "_Engaged");
				if (text2 != null)
				{
					CurrentDialogue.Push(new Dialogue(text2, this));
				}
				else
				{
					CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3980"), this));
				}
			}
			who.changeFriendship(1, this);
			who.reduceActiveItemByOne();
			who.completelyStopAnimatingOrDoingAction();
			Game1.drawDialogue(this);
		}

		public virtual void tryToReceiveActiveObject(Farmer who)
		{
			who.Halt();
			who.faceGeneralDirection(getStandingPosition(), 0, opposite: false, useTileCalculations: false);
			if (name.Equals("Henchman") && Game1.currentLocation.name.Equals("WitchSwamp"))
			{
				if (who.ActiveObject != null && (int)who.ActiveObject.parentSheetIndex == 308)
				{
					if (controller == null)
					{
						who.currentLocation.localSound("coin");
						who.reduceActiveItemByOne();
						CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Henchman5"), this));
						Game1.drawDialogue(this);
						who.freezePause = 2000;
						removeHenchmanEvent.Fire();
					}
				}
				else if (who.ActiveObject != null)
				{
					if ((int)who.ActiveObject.parentSheetIndex == 684)
					{
						CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Henchman4"), this));
					}
					else
					{
						CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Henchman3"), this));
					}
					Game1.drawDialogue(this);
				}
				return;
			}
			if (Game1.player.team.specialOrders != null)
			{
				foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
				{
					if (specialOrder.onItemDelivered == null)
					{
						continue;
					}
					Delegate[] invocationList = specialOrder.onItemDelivered.GetInvocationList();
					for (int i = 0; i < invocationList.Length; i++)
					{
						Func<Farmer, NPC, Item, int> func = (Func<Farmer, NPC, Item, int>)invocationList[i];
						int num = func(Game1.player, this, who.ActiveObject);
						if (num > 0)
						{
							if (who.ActiveObject.Stack <= 0)
							{
								who.ActiveObject = null;
								who.showNotCarrying();
							}
							return;
						}
					}
				}
			}
			if (Game1.questOfTheDay != null && (bool)Game1.questOfTheDay.accepted && !Game1.questOfTheDay.completed && Game1.questOfTheDay is ItemDeliveryQuest && ((ItemDeliveryQuest)Game1.questOfTheDay).checkIfComplete(this, -1, -1, who.ActiveObject))
			{
				who.reduceActiveItemByOne();
				who.completelyStopAnimatingOrDoingAction();
				if (Game1.random.NextDouble() < 0.3 && !base.Name.Equals("Wizard"))
				{
					doEmote(32);
				}
			}
			else if (Game1.questOfTheDay != null && Game1.questOfTheDay is FishingQuest && ((FishingQuest)Game1.questOfTheDay).checkIfComplete(this, who.ActiveObject.ParentSheetIndex, 1))
			{
				who.reduceActiveItemByOne();
				who.completelyStopAnimatingOrDoingAction();
				if (Game1.random.NextDouble() < 0.3 && !base.Name.Equals("Wizard"))
				{
					doEmote(32);
				}
			}
			else if (who.ActiveObject != null && Utility.IsNormalObjectAtParentSheetIndex(who.ActiveObject, 897))
			{
				if (base.Name.Equals("Pierre") && !Game1.player.hasOrWillReceiveMail("PierreStocklist"))
				{
					Game1.addMail("PierreStocklist", noLetter: true, sendToEveryone: true);
					who.reduceActiveItemByOne();
					who.completelyStopAnimatingOrDoingAction();
					who.currentLocation.localSound("give_gift");
					Game1.player.team.itemsToRemoveOvernight.Add(897);
					setNewDialogue(Game1.content.LoadString("Strings\\Characters:PierreStockListDialogue"), add: true);
					Game1.drawDialogue(this);
					Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
					{
						Game1.multiplayer.globalChatInfoMessage("StockList");
					});
				}
				else
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", displayName)));
				}
			}
			else if (who.ActiveObject != null && !who.ActiveObject.bigCraftable && (int)who.ActiveObject.parentSheetIndex == 71 && base.Name.Equals("Lewis") && who.hasQuest(102))
			{
				if (who.currentLocation != null && who.currentLocation.Name == "IslandSouth")
				{
					Game1.player.activeDialogueEvents["lucky_pants_lewis"] = 28;
				}
				who.completeQuest(102);
				Dictionary<int, string> dictionary = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Quests");
				string s = ((dictionary[102].Length > 9) ? dictionary[102].Split('/')[9] : Game1.content.LoadString("Data\\ExtraDialogue:LostItemQuest_DefaultThankYou"));
				setNewDialogue(s);
				Game1.drawDialogue(this);
				Game1.player.changeFriendship(250, this);
				who.ActiveObject = null;
			}
			else if (who.ActiveObject != null && Dialogue.ContainsKey("reject_" + who.ActiveObject.ParentSheetIndex))
			{
				setNewDialogue(Dialogue["reject_" + who.ActiveObject.ParentSheetIndex]);
				Game1.drawDialogue(this);
			}
			else if (who.ActiveObject != null && (bool)who.ActiveObject.questItem)
			{
				if (who.hasQuest(130) && Dialogue.ContainsKey("accept_" + who.ActiveObject.ParentSheetIndex))
				{
					setNewDialogue(Dialogue["accept_" + who.ActiveObject.ParentSheetIndex]);
					Game1.drawDialogue(this);
					CurrentDialogue.Peek().onFinish = delegate
					{
						int num2 = who.ActiveObject.ParentSheetIndex + 1;
						Object o = new Object(who.ActiveObject.ParentSheetIndex + 1, 1)
						{
							specialItem = true
						};
						o.questItem.Value = true;
						who.reduceActiveItemByOne();
						DelayedAction.playSoundAfterDelay("coin", 200);
						DelayedAction.functionAfterDelay(delegate
						{
							who.addItemByMenuIfNecessary(o);
						}, 200);
						Game1.player.freezePause = 550;
						DelayedAction.functionAfterDelay(delegate
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1919", o.DisplayName, Lexicon.getProperArticleForWord(o.DisplayName)));
						}, 550);
					};
				}
				else if (!who.checkForQuestComplete(this, -1, -1, who.ActiveObject, "", 9, 3) && name != "Birdie")
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3954"));
				}
			}
			else
			{
				if (who.checkForQuestComplete(this, -1, -1, null, "", 10))
				{
					return;
				}
				if ((int)who.ActiveObject.parentSheetIndex == 809 && !who.ActiveObject.bigCraftable)
				{
					if (!Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater"))
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", displayName)));
						return;
					}
					if (base.Name.Equals("Dwarf") && !who.canUnderstandDwarves)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", displayName)));
						return;
					}
					if (base.Name.Equals("Krobus") && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri")
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", displayName)));
						return;
					}
					if ((!CanSocialize && !base.Name.Equals("Dwarf")) || !isVillager())
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_CantInvite", displayName)));
						return;
					}
					if (!who.friendshipData.ContainsKey(base.Name))
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", displayName)));
						return;
					}
					if (who.friendshipData[base.Name].IsDivorced())
					{
						if (who == Game1.player)
						{
							Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", Game1.player.displayName, displayName);
						}
						CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Divorced_gift"), this));
						Game1.drawDialogue(this);
						return;
					}
					if (who.lastSeenMovieWeek.Value >= Game1.Date.TotalWeeks)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_FarmerAlreadySeen")));
						return;
					}
					if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_Festival")));
						return;
					}
					if (Game1.timeOfDay > 2100)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_Closed")));
						return;
					}
					foreach (MovieInvitation movieInvitation in who.team.movieInvitations)
					{
						if (movieInvitation.farmer == who)
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_AlreadyInvitedSomeone", movieInvitation.invitedNPC.displayName)));
							return;
						}
					}
					faceTowardFarmerForPeriod(4000, 3, faceAway: false, who);
					foreach (MovieInvitation movieInvitation2 in who.team.movieInvitations)
					{
						if (movieInvitation2.invitedNPC == this)
						{
							if (who == Game1.player)
							{
								Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", Game1.player.displayName, displayName);
							}
							CurrentDialogue.Push(new Dialogue(GetDispositionModifiedString("Strings\\Characters:MovieInvite_InvitedBySomeoneElse", movieInvitation2.farmer.displayName), this));
							Game1.drawDialogue(this);
							return;
						}
					}
					if (lastSeenMovieWeek.Value >= Game1.Date.TotalWeeks)
					{
						if (who == Game1.player)
						{
							Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", Game1.player.displayName, displayName);
						}
						CurrentDialogue.Push(new Dialogue(GetDispositionModifiedString("Strings\\Characters:MovieInvite_AlreadySeen"), this));
						Game1.drawDialogue(this);
						return;
					}
					if (MovieTheater.GetResponseForMovie(this) == "reject")
					{
						if (who == Game1.player)
						{
							Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", Game1.player.displayName, displayName);
						}
						CurrentDialogue.Push(new Dialogue(GetDispositionModifiedString("Strings\\Characters:MovieInvite_Reject"), this));
						Game1.drawDialogue(this);
						return;
					}
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en && getSpouse() != null && getSpouse().Equals(who) && name != "Krobus")
					{
						CurrentDialogue.Push(new Dialogue(GetDispositionModifiedString("Strings\\Characters:MovieInvite_Spouse_" + name), this));
					}
					else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en && dialogue != null && dialogue.ContainsKey("MovieInvitation"))
					{
						CurrentDialogue.Push(new Dialogue(dialogue["MovieInvitation"], this));
					}
					else
					{
						CurrentDialogue.Push(new Dialogue(GetDispositionModifiedString("Strings\\Characters:MovieInvite_Invited"), this));
					}
					Game1.drawDialogue(this);
					who.reduceActiveItemByOne();
					who.completelyStopAnimatingOrDoingAction();
					who.currentLocation.localSound("give_gift");
					MovieTheater.Invite(who, this);
					if (who == Game1.player)
					{
						Game1.multiplayer.globalChatInfoMessage("MovieInviteAccept", Game1.player.displayName, displayName);
					}
				}
				else
				{
					if (!Game1.NPCGiftTastes.ContainsKey(base.Name))
					{
						return;
					}
					foreach (string key in who.activeDialogueEvents.Keys)
					{
						if (key.Contains("dumped") && Dialogue.ContainsKey(key))
						{
							doEmote(12);
							return;
						}
					}
					who.completeQuest(25);
					string text = base.Name.ToLower().Replace(' ', '_');
					if (who.ActiveObject.HasContextTag("propose_roommate_" + text))
					{
						if (who.getFriendshipHeartLevelForNPC(base.Name) >= 10 && (int)who.houseUpgradeLevel >= 1 && !who.isMarried() && !who.isEngaged())
						{
							engagementResponse(who, asRoommate: true);
						}
						else
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", displayName)));
						}
					}
					else if (who.ActiveObject.ParentSheetIndex == 808 && base.Name.Equals("Krobus"))
					{
						if (who.getFriendshipHeartLevelForNPC(base.Name) >= 10 && (int)who.houseUpgradeLevel >= 1 && !who.isMarried() && !who.isEngaged())
						{
							engagementResponse(who, asRoommate: true);
						}
					}
					else if (who.ActiveObject.ParentSheetIndex == 458)
					{
						if (!datable || (who.spouse != base.Name && isMarriedOrEngaged()))
						{
							if (Game1.random.NextDouble() < 0.5)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3955", displayName));
								return;
							}
							CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3956") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.3957"), this));
							Game1.drawDialogue(this);
							return;
						}
						if ((bool)datable && who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].IsDating())
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:AlreadyDatingBouquet", displayName));
							return;
						}
						if ((bool)datable && who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].IsDivorced())
						{
							CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Divorced_bouquet"), this));
							Game1.drawDialogue(this);
							return;
						}
						if ((bool)datable && who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].Points < 1000)
						{
							CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3958") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.3959"), this));
							Game1.drawDialogue(this);
							return;
						}
						if ((bool)datable && who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].Points < 2000)
						{
							CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3960") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3961"), this));
							Game1.drawDialogue(this);
							return;
						}
						Friendship friendship = who.friendshipData[base.Name];
						if (!friendship.IsDating())
						{
							friendship.Status = FriendshipStatus.Dating;
							Game1.multiplayer.globalChatInfoMessage("Dating", Game1.player.Name, displayName);
						}
						CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.3962") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.3963"), this));
						who.changeFriendship(25, this);
						who.reduceActiveItemByOne();
						who.completelyStopAnimatingOrDoingAction();
						doEmote(20);
						Game1.drawDialogue(this);
					}
					else if (who.ActiveObject.ParentSheetIndex == 277)
					{
						if (!datable || (who.friendshipData.ContainsKey(base.Name) && !who.friendshipData[base.Name].IsDating()) || (who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].IsMarried()))
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Wilted_Bouquet_Meaningless", displayName));
						}
						else if (who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].IsDating())
						{
							Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Wilted_Bouquet_Effect", displayName));
							Game1.multiplayer.globalChatInfoMessage("BreakUp", Game1.player.Name, displayName);
							who.reduceActiveItemByOne();
							who.friendshipData[base.Name].Status = FriendshipStatus.Friendly;
							who.completelyStopAnimatingOrDoingAction();
							who.friendshipData[base.Name].Points = Math.Min(who.friendshipData[base.Name].Points, 1250);
							switch ((string)name)
							{
							case "Maru":
							case "Haley":
								doEmote(12);
								break;
							default:
								doEmote(28);
								break;
							case "Shane":
							case "Alex":
								break;
							}
							CurrentDialogue.Clear();
							CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Characters\\Dialogue\\" + GetDialogueSheetName() + ":breakUp"), this));
							Game1.drawDialogue(this);
						}
					}
					else if (who.ActiveObject.ParentSheetIndex == 460)
					{
						if (who.isMarried() || who.isEngaged())
						{
							if (who.hasCurrentOrPendingRoommate())
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:TriedToMarryButKrobus"));
							}
							else if (who.isEngaged())
							{
								CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.3965") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.3966"), this));
								Game1.drawDialogue(this);
							}
							else
							{
								CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3967") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.3968"), this));
								Game1.drawDialogue(this);
							}
						}
						else if (!datable || isMarriedOrEngaged() || (who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].IsDivorced()) || (who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].Points < 1500))
						{
							if (Game1.random.NextDouble() < 0.5)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", displayName));
								return;
							}
							CurrentDialogue.Push(new Dialogue((Gender == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3970") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3971"), this));
							Game1.drawDialogue(this);
						}
						else if ((bool)datable && who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].Points < 2500)
						{
							if (!who.friendshipData[base.Name].ProposalRejected)
							{
								CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3972") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3973"), this));
								Game1.drawDialogue(this);
								who.changeFriendship(-20, this);
								who.friendshipData[base.Name].ProposalRejected = true;
							}
							else
							{
								CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.3974") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.3975"), this));
								Game1.drawDialogue(this);
								who.changeFriendship(-50, this);
							}
						}
						else if ((bool)datable && (int)who.houseUpgradeLevel < 1)
						{
							if (Game1.random.NextDouble() < 0.5)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", displayName));
								return;
							}
							CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3972"), this));
							Game1.drawDialogue(this);
						}
						else
						{
							engagementResponse(who);
						}
					}
					else if ((who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].GiftsThisWeek < 2) || (who.spouse != null && who.spouse.Equals(base.Name)) || this is Child || isBirthday(Game1.currentSeason, Game1.dayOfMonth))
					{
						if (who.friendshipData[base.Name].IsDivorced())
						{
							CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Divorced_gift"), this));
							Game1.drawDialogue(this);
							return;
						}
						if (who.friendshipData[base.Name].GiftsToday == 1)
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3981", displayName)));
							return;
						}
						receiveGift(who.ActiveObject, who);
						who.reduceActiveItemByOne();
						who.completelyStopAnimatingOrDoingAction();
						faceTowardFarmerForPeriod(4000, 3, faceAway: false, who);
						if ((bool)datable && who.spouse != null && who.spouse != base.Name && !who.hasCurrentOrPendingRoommate() && Utility.isMale(who.spouse) == Utility.isMale(base.Name) && Game1.random.NextDouble() < 0.3 - (double)((float)who.LuckLevel / 100f) - who.DailyLuck && !isBirthday(Game1.currentSeason, Game1.dayOfMonth) && who.friendshipData[base.Name].IsDating())
						{
							NPC characterFromName = Game1.getCharacterFromName(who.spouse);
							who.changeFriendship(-30, characterFromName);
							characterFromName.CurrentDialogue.Clear();
							characterFromName.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3985", displayName), characterFromName));
						}
					}
					else
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3987", displayName, 2)));
					}
				}
			}
		}

		public string GetDispositionModifiedString(string path, params object[] substitutions)
		{
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			List<string> list = new List<string>();
			string value = "";
			list.Add(name.Value);
			if (Game1.player.isMarried() && Game1.player.getSpouse() == this)
			{
				list.Add("spouse");
			}
			if (dictionary.TryGetValue(base.Name, out value))
			{
				string[] array = value.Split('/');
				if (array.Length > 4)
				{
					list.Add(array[1]);
					list.Add(array[2]);
					list.Add(array[3]);
					list.Add(array[0]);
				}
			}
			foreach (string item in list)
			{
				string text = path + "_" + Utility.capitalizeFirstLetter(item);
				string text2 = Game1.content.LoadString(text, substitutions);
				if (!(text2 == text))
				{
					return text2;
				}
			}
			return Game1.content.LoadString(path, substitutions);
		}

		public void haltMe(Farmer who)
		{
			Halt();
		}

		public virtual bool checkAction(Farmer who, GameLocation l)
		{
			if (IsInvisible)
			{
				return false;
			}
			if (isSleeping.Value)
			{
				if (!isEmoting)
				{
					doEmote(24);
				}
				shake(250);
				return false;
			}
			if (!who.CanMove)
			{
				return false;
			}
			if (base.Name.Equals("Henchman") && l.Name.Equals("WitchSwamp"))
			{
				if (!Game1.player.mailReceived.Contains("Henchman1"))
				{
					Game1.player.mailReceived.Add("Henchman1");
					CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Henchman1"), this));
					Game1.drawDialogue(this);
					Game1.player.addQuest(27);
					if (!Game1.player.friendshipData.ContainsKey("Henchman"))
					{
						Game1.player.friendshipData.Add("Henchman", new Friendship());
					}
				}
				else
				{
					if (who.ActiveObject != null && who.ActiveObject.canBeGivenAsGift() && !who.isRidingHorse())
					{
						tryToReceiveActiveObject(who);
						return true;
					}
					if (controller == null)
					{
						CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Henchman2"), this));
						Game1.drawDialogue(this);
					}
				}
				return true;
			}
			bool flag = false;
			if (who.pantsItem.Value != null && (int)who.pantsItem.Value.parentSheetIndex == 15 && (base.Name.Equals("Lewis") || base.Name.Equals("Marnie")))
			{
				flag = true;
			}
			if (Game1.NPCGiftTastes.ContainsKey(base.Name) && !Game1.player.friendshipData.ContainsKey(base.Name))
			{
				Game1.player.friendshipData.Add(base.Name, new Friendship(0));
				if (base.Name.Equals("Krobus"))
				{
					CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3990"), this));
					Game1.drawDialogue(this);
					return true;
				}
			}
			if (who.checkForQuestComplete(this, -1, -1, who.ActiveObject, null, -1, 5))
			{
				faceTowardFarmerForPeriod(6000, 3, faceAway: false, who);
				return true;
			}
			if (base.Name.Equals("Krobus") && who.hasQuest(28))
			{
				CurrentDialogue.Push(new Dialogue((l is Sewer) ? Game1.content.LoadString("Strings\\Characters:KrobusDarkTalisman") : Game1.content.LoadString("Strings\\Characters:KrobusDarkTalisman_elsewhere"), this));
				Game1.drawDialogue(this);
				who.removeQuest(28);
				who.mailReceived.Add("krobusUnseal");
				if (l is Sewer)
				{
					DelayedAction.addTemporarySpriteAfterDelay(new TemporaryAnimatedSprite("TileSheets\\Projectiles", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16), 3000f, 1, 0, new Vector2(31f, 17f) * 64f, flicker: false, flipped: false)
					{
						scale = 4f,
						delayBeforeAnimationStart = 1,
						startSound = "debuffSpell",
						motion = new Vector2(-9f, 1f),
						rotationChange = (float)Math.PI / 64f,
						light = true,
						lightRadius = 1f,
						lightcolor = new Color(150, 0, 50),
						layerDepth = 1f,
						alphaFade = 0.003f
					}, l, 200, waitUntilMenusGone: true);
					DelayedAction.addTemporarySpriteAfterDelay(new TemporaryAnimatedSprite("TileSheets\\Projectiles", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16), 3000f, 1, 0, new Vector2(31f, 17f) * 64f, flicker: false, flipped: false)
					{
						startSound = "debuffSpell",
						delayBeforeAnimationStart = 1,
						scale = 4f,
						motion = new Vector2(-9f, 1f),
						rotationChange = (float)Math.PI / 64f,
						light = true,
						lightRadius = 1f,
						lightcolor = new Color(150, 0, 50),
						layerDepth = 1f,
						alphaFade = 0.003f
					}, l, 700, waitUntilMenusGone: true);
				}
				return true;
			}
			if (base.Name.Equals(who.spouse) && who.IsLocalPlayer)
			{
				_ = Game1.timeOfDay;
				_ = 2200;
				if (Sprite.CurrentAnimation == null)
				{
					faceDirection(-3);
				}
				if (Sprite.CurrentAnimation == null && who.friendshipData.ContainsKey(name) && who.friendshipData[name].Points >= 3125 && !who.mailReceived.Contains("CF_Spouse"))
				{
					CurrentDialogue.Push(new Dialogue(Game1.content.LoadString(Game1.player.isRoommate(who.spouse) ? "Strings\\StringsFromCSFiles:Krobus_Stardrop" : "Strings\\StringsFromCSFiles:NPC.cs.4001"), this));
					Game1.player.addItemByMenuIfNecessary(new Object(Vector2.Zero, 434, "Cosmic Fruit", canBeSetDown: false, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false));
					shouldSayMarriageDialogue.Value = false;
					currentMarriageDialogue.Clear();
					who.mailReceived.Add("CF_Spouse");
					return true;
				}
				if (Sprite.CurrentAnimation == null && !hasTemporaryMessageAvailable() && currentMarriageDialogue.Count == 0 && CurrentDialogue.Count == 0 && Game1.timeOfDay < 2200 && !isMoving() && who.ActiveObject == null)
				{
					faceGeneralDirection(who.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
					who.faceGeneralDirection(getStandingPosition(), 0, opposite: false, useTileCalculations: false);
					if (FacingDirection == 3 || FacingDirection == 1)
					{
						int frame = 28;
						bool flag2 = true;
						string text = base.Name;
						if (text != null)
						{
							switch (text.Length)
							{
							case 4:
								switch (text[0])
								{
								case 'M':
									if (text == "Maru")
									{
										frame = 28;
										flag2 = false;
									}
									break;
								case 'L':
									if (text == "Leah")
									{
										frame = 25;
										flag2 = true;
									}
									break;
								case 'A':
									if (text == "Alex")
									{
										frame = 42;
										flag2 = true;
									}
									break;
								}
								break;
							case 6:
								switch (text[0])
								{
								case 'H':
									if (text == "Harvey")
									{
										frame = 31;
										flag2 = false;
									}
									break;
								case 'K':
									if (text == "Krobus")
									{
										frame = 16;
										flag2 = true;
									}
									break;
								}
								break;
							case 7:
								switch (text[0])
								{
								case 'E':
									if (text == "Elliott")
									{
										frame = 35;
										flag2 = false;
									}
									break;
								case 'A':
									if (text == "Abigail")
									{
										frame = 33;
										flag2 = false;
									}
									break;
								}
								break;
							case 5:
								switch (text[0])
								{
								case 'P':
									if (text == "Penny")
									{
										frame = 35;
										flag2 = true;
									}
									break;
								case 'S':
									if (text == "Shane")
									{
										frame = 34;
										flag2 = false;
									}
									break;
								case 'E':
									if (text == "Emily")
									{
										frame = 33;
										flag2 = false;
									}
									break;
								}
								break;
							case 9:
								if (text == "Sebastian")
								{
									frame = 40;
									flag2 = false;
								}
								break;
							case 3:
								if (text == "Sam")
								{
									frame = 36;
									flag2 = true;
								}
								break;
							}
						}
						bool flag3 = (flag2 && FacingDirection == 3) || (!flag2 && FacingDirection == 1);
						if (who.getFriendshipHeartLevelForNPC(base.Name) > 9 && sleptInBed.Value)
						{
							int milliseconds = (movementPause = (Game1.IsMultiplayer ? 1000 : 10));
							Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
							{
								new FarmerSprite.AnimationFrame(frame, milliseconds, secondaryArm: false, flag3, haltMe, behaviorAtEndOfFrame: true)
							});
							if (!hasBeenKissedToday.Value)
							{
								who.changeFriendship(10, this);
								if (who.hasCurrentOrPendingRoommate())
								{
									Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\emojis", new Microsoft.Xna.Framework.Rectangle(0, 0, 9, 9), 2000f, 1, 0, new Vector2(getTileX(), getTileY()) * 64f + new Vector2(16f, -64f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
									{
										motion = new Vector2(0f, -0.5f),
										alphaFade = 0.01f
									});
								}
								else
								{
									Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(211, 428, 7, 6), 2000f, 1, 0, new Vector2(getTileX(), getTileY()) * 64f + new Vector2(16f, -64f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
									{
										motion = new Vector2(0f, -0.5f),
										alphaFade = 0.01f
									});
								}
								l.playSound("dwop", NetAudio.SoundContext.NPC);
								who.exhausted.Value = false;
							}
							hasBeenKissedToday.Value = true;
							Sprite.UpdateSourceRect();
						}
						else
						{
							faceDirection((Game1.random.NextDouble() < 0.5) ? 2 : 0);
							doEmote(12);
						}
						int num = 1;
						if ((flag2 && !flag3) || (!flag2 && flag3))
						{
							num = 3;
						}
						who.PerformKiss(num);
						return true;
					}
				}
			}
			bool flag4 = false;
			if (who.friendshipData.ContainsKey(base.Name) || base.Name == "Mister Qi")
			{
				if (getSpouse() == Game1.player && shouldSayMarriageDialogue.Value && currentMarriageDialogue.Count > 0 && currentMarriageDialogue.Count > 0)
				{
					while (currentMarriageDialogue.Count > 0)
					{
						MarriageDialogueReference marriageDialogueReference = currentMarriageDialogue[currentMarriageDialogue.Count - 1];
						if (marriageDialogueReference == marriageDefaultDialogue.Value)
						{
							marriageDefaultDialogue.Value = null;
						}
						currentMarriageDialogue.RemoveAt(currentMarriageDialogue.Count - 1);
						CurrentDialogue.Push(marriageDialogueReference.GetDialogue(this));
					}
					flag4 = true;
				}
				if (!flag4)
				{
					flag4 = checkForNewCurrentDialogue(who.friendshipData.ContainsKey(base.Name) ? (who.friendshipData[base.Name].Points / 250) : 0);
					if (!flag4)
					{
						flag4 = checkForNewCurrentDialogue(who.friendshipData.ContainsKey(base.Name) ? (who.friendshipData[base.Name].Points / 250) : 0, noPreface: true);
					}
				}
			}
			if (who.IsLocalPlayer && who.friendshipData.ContainsKey(base.Name) && (endOfRouteMessage.Value != null || flag4 || (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this))))
			{
				if (!flag4 && setTemporaryMessages(who))
				{
					Game1.player.checkForQuestComplete(this, -1, -1, null, null, 5);
					return false;
				}
				if (Sprite.Texture.Bounds.Height > 32)
				{
					faceTowardFarmerForPeriod(5000, 4, faceAway: false, who);
				}
				if (who.ActiveObject != null && who.ActiveObject.canBeGivenAsGift() && !who.isRidingHorse())
				{
					tryToReceiveActiveObject(who);
					faceTowardFarmerForPeriod(3000, 4, faceAway: false, who);
					return true;
				}
				grantConversationFriendship(who);
				Game1.drawDialogue(this);
				return true;
			}
			if (canTalk() && who.hasClubCard && base.Name.Equals("Bouncer") && who.IsLocalPlayer)
			{
				Response[] answerChoices = new Response[2]
				{
					new Response("Yes.", Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4018")),
					new Response("That's", Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4020"))
				};
				l.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4021"), answerChoices, "ClubCard");
			}
			else if (canTalk() && CurrentDialogue.Count > 0)
			{
				if (!base.Name.Contains("King") && who.ActiveObject != null && who.ActiveObject.canBeGivenAsGift() && !who.isRidingHorse())
				{
					if (who.IsLocalPlayer)
					{
						tryToReceiveActiveObject(who);
					}
					else
					{
						faceTowardFarmerForPeriod(3000, 4, faceAway: false, who);
					}
					return true;
				}
				if (CurrentDialogue.Count >= 1 || endOfRouteMessage.Value != null || (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this)))
				{
					if (setTemporaryMessages(who))
					{
						Game1.player.checkForQuestComplete(this, -1, -1, null, null, 5);
						return false;
					}
					if (Sprite.Texture.Bounds.Height > 32)
					{
						faceTowardFarmerForPeriod(5000, 4, faceAway: false, who);
					}
					if (who.IsLocalPlayer)
					{
						grantConversationFriendship(who);
						if (!flag)
						{
							Game1.drawDialogue(this);
							return true;
						}
					}
				}
				else if (!doingEndOfRouteAnimation)
				{
					try
					{
						if (who.friendshipData.ContainsKey(base.Name))
						{
							faceTowardFarmerForPeriod(who.friendshipData[base.Name].Points / 125 * 1000 + 1000, 4, faceAway: false, who);
						}
					}
					catch (Exception)
					{
					}
					if (Game1.random.NextDouble() < 0.1)
					{
						doEmote(8);
					}
				}
			}
			else if (canTalk() && !Game1.game1.wasAskedLeoMemory && Game1.CurrentEvent == null && name == "Leo" && base.currentLocation != null && (base.currentLocation.NameOrUniqueName == "LeoTreeHouse" || base.currentLocation.NameOrUniqueName == "Mountain") && Game1.MasterPlayer.hasOrWillReceiveMail("leoMoved") && GetUnseenLeoEvent().HasValue && CanRevisitLeoMemory(GetUnseenLeoEvent()))
			{
				Game1.drawDialogue(this, Game1.content.LoadString("Strings\\Characters:Leo_Memory"));
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(AskLeoMemoryPrompt));
			}
			else
			{
				if (who.ActiveObject != null && who.ActiveObject.canBeGivenAsGift() && !who.isRidingHorse())
				{
					if (base.Name.Equals("Bouncer"))
					{
						return true;
					}
					tryToReceiveActiveObject(who);
					faceTowardFarmerForPeriod(3000, 4, faceAway: false, who);
					return true;
				}
				if (base.Name.Equals("Krobus"))
				{
					if (l is Sewer)
					{
						Game1.activeClickableMenu = new ShopMenu((l as Sewer).getShadowShopStock(), 0, "Krobus", (l as Sewer).onShopPurchase);
						return true;
					}
				}
				else if (base.Name.Equals("Dwarf") && who.canUnderstandDwarves && l is Mine)
				{
					Game1.activeClickableMenu = new ShopMenu(Utility.getDwarfShopStock(), 0, "Dwarf");
					return true;
				}
			}
			if (flag)
			{
				if (yJumpVelocity != 0f || Sprite.CurrentAnimation != null)
				{
					return true;
				}
				if (base.Name.Equals("Lewis"))
				{
					faceTowardFarmerForPeriod(1000, 3, faceAway: false, who);
					jump();
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(26, 1000, secondaryArm: false, flip: false, delegate
						{
							doEmote(12);
						}, behaviorAtEndOfFrame: true)
					});
					Sprite.loop = false;
					shakeTimer = 1000;
					l.playSound("batScreech");
				}
				else if (base.Name.Equals("Marnie"))
				{
					faceTowardFarmerForPeriod(1000, 3, faceAway: false, who);
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(33, 150, secondaryArm: false, flip: false, delegate
						{
							l.playSound("dustMeep");
						}),
						new FarmerSprite.AnimationFrame(34, 180),
						new FarmerSprite.AnimationFrame(33, 180, secondaryArm: false, flip: false, delegate
						{
							l.playSound("dustMeep");
						}),
						new FarmerSprite.AnimationFrame(34, 180),
						new FarmerSprite.AnimationFrame(33, 180, secondaryArm: false, flip: false, delegate
						{
							l.playSound("dustMeep");
						}),
						new FarmerSprite.AnimationFrame(34, 180),
						new FarmerSprite.AnimationFrame(33, 180, secondaryArm: false, flip: false, delegate
						{
							l.playSound("dustMeep");
						}),
						new FarmerSprite.AnimationFrame(34, 180)
					});
					Sprite.loop = false;
				}
				return true;
			}
			if (setTemporaryMessages(who))
			{
				return false;
			}
			if (((bool)doingEndOfRouteAnimation || !goingToDoEndOfRouteAnimation) && endOfRouteMessage.Value != null)
			{
				Game1.drawDialogue(this);
				return true;
			}
			return false;
		}

		public void grantConversationFriendship(Farmer who, int amount = 20)
		{
			if (!base.Name.Contains("King") && !who.hasPlayerTalkedToNPC(base.Name) && who.friendshipData.ContainsKey(base.Name))
			{
				who.friendshipData[base.Name].TalkedToToday = true;
				Game1.player.checkForQuestComplete(this, -1, -1, null, null, 5);
				if (!isDivorcedFrom(who))
				{
					who.changeFriendship(amount, this);
				}
			}
		}

		public virtual void AskLeoMemoryPrompt()
		{
			GameLocation gameLocation = base.currentLocation;
			Response[] answerChoices = new Response[2]
			{
				new Response("Yes", Game1.content.LoadString("Strings\\Characters:Leo_Memory_Answer_Yes")),
				new Response("No", Game1.content.LoadString("Strings\\Characters:Leo_Memory_Answer_No"))
			};
			string text = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Characters:Leo_Memory_" + GetUnseenLeoEvent().Value.Value);
			if (text == null)
			{
				text = "";
			}
			gameLocation.createQuestionDialogue(text, answerChoices, OnLeoMemoryResponse, this);
		}

		public bool CanRevisitLeoMemory(KeyValuePair<string, int>? event_data)
		{
			if (!event_data.HasValue)
			{
				return false;
			}
			string key = event_data.Value.Key;
			int value = event_data.Value.Value;
			Dictionary<string, string> dictionary = null;
			try
			{
				dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + key);
			}
			catch (Exception)
			{
				return false;
			}
			if (dictionary == null)
			{
				return false;
			}
			foreach (string key2 in dictionary.Keys)
			{
				string[] array = key2.Split('/');
				if (array[0] == value.ToString())
				{
					GameLocation locationFromName = Game1.getLocationFromName(key);
					string text = key2;
					text = text.Replace("/e 1039573", "");
					text = text.Replace("/Hl leoMoved", "");
					if (locationFromName != null && locationFromName.checkEventPrecondition(text) != -1)
					{
						return true;
					}
				}
			}
			return false;
		}

		public KeyValuePair<string, int>? GetUnseenLeoEvent()
		{
			List<int> list = new List<int>();
			if (!Game1.player.eventsSeen.Contains(6497423))
			{
				return new KeyValuePair<string, int>("IslandWest", 6497423);
			}
			if (!Game1.player.eventsSeen.Contains(6497421))
			{
				return new KeyValuePair<string, int>("IslandNorth", 6497421);
			}
			if (!Game1.player.eventsSeen.Contains(6497428))
			{
				return new KeyValuePair<string, int>("IslandSouth", 6497428);
			}
			return null;
		}

		public void OnLeoMemoryResponse(Farmer who, string whichAnswer)
		{
			if (whichAnswer.ToLower() == "yes")
			{
				KeyValuePair<string, int>? unseenLeoEvent = GetUnseenLeoEvent();
				if (!unseenLeoEvent.HasValue)
				{
					return;
				}
				string key2 = unseenLeoEvent.Value.Key;
				int event_id = unseenLeoEvent.Value.Value;
				Dictionary<string, string> location_events = null;
				try
				{
					location_events = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + key2);
				}
				catch (Exception)
				{
					return;
				}
				if (location_events == null)
				{
					return;
				}
				int old_x = Game1.player.getTileX();
				int old_y = Game1.player.getTileY();
				string old_location = Game1.player.currentLocation.NameOrUniqueName;
				int old_direction = Game1.player.FacingDirection;
				{
					foreach (string key in location_events.Keys)
					{
						string[] array = key.Split('/');
						if (array[0] == event_id.ToString())
						{
							LocationRequest location_request = Game1.getLocationRequest(key2);
							Game1.warpingForForcedRemoteEvent = true;
							location_request.OnWarp += delegate
							{
								Event @event = new Event(location_events[key], event_id);
								@event.isMemory = true;
								@event.setExitLocation(old_location, old_x, old_y);
								Game1.player.orientationBeforeEvent = old_direction;
								location_request.Location.currentEvent = @event;
								location_request.Location.startEvent(@event);
								Game1.warpingForForcedRemoteEvent = false;
							};
							int x = 8;
							int y = 8;
							Utility.getDefaultWarpLocation(location_request.Name, ref x, ref y);
							Game1.warpFarmer(location_request, x, y, Game1.player.FacingDirection);
						}
					}
					return;
				}
			}
			Game1.game1.wasAskedLeoMemory = true;
		}

		public bool isDivorcedFrom(Farmer who)
		{
			if (who == null)
			{
				return false;
			}
			if (who.friendshipData.ContainsKey(base.Name) && who.friendshipData[base.Name].IsDivorced())
			{
				return true;
			}
			return false;
		}

		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			if (movementPause <= 0)
			{
				faceTowardFarmerTimer = 0;
				base.MovePosition(time, viewport, currentLocation);
			}
		}

		public GameLocation getHome()
		{
			if (isMarried() && getSpouse() != null)
			{
				return Utility.getHomeOfFarmer(getSpouse());
			}
			return Game1.getLocationFromName(defaultMap);
		}

		public override bool canPassThroughActionTiles()
		{
			return true;
		}

		public virtual void behaviorOnFarmerPushing()
		{
		}

		public virtual void behaviorOnFarmerLocationEntry(GameLocation location, Farmer who)
		{
			if (Sprite != null && Sprite.CurrentAnimation == null && Sprite.SourceRect.Height > 32)
			{
				Sprite.SpriteWidth = 16;
				Sprite.SpriteHeight = 16;
				Sprite.currentFrame = 0;
			}
		}

		public virtual void behaviorOnLocalFarmerLocationEntry(GameLocation location)
		{
			shouldPlayRobinHammerAnimation.CancelInterpolation();
			shouldPlaySpousePatioAnimation.CancelInterpolation();
			shouldWearIslandAttire.CancelInterpolation();
			isSleeping.CancelInterpolation();
			doingEndOfRouteAnimation.CancelInterpolation();
			if (doingEndOfRouteAnimation.Value)
			{
				_skipRouteEndIntro = true;
			}
			else
			{
				_skipRouteEndIntro = false;
			}
			endOfRouteBehaviorName.CancelInterpolation();
			if (isSleeping.Value)
			{
				drawOffset.CancelInterpolation();
				position.Field.CancelInterpolation();
			}
		}

		public override void updateMovement(GameLocation location, GameTime time)
		{
			lastPosition = base.Position;
			if (DirectionsToNewLocation != null && !Game1.newDay)
			{
				Point standingXY = getStandingXY();
				if (standingXY.X < -64 || standingXY.X > location.map.DisplayWidth + 64 || standingXY.Y < -64 || standingXY.Y > location.map.DisplayHeight + 64)
				{
					IsWalkingInSquare = false;
					Game1.warpCharacter(this, DefaultMap, DefaultPosition);
					location.characters.Remove(this);
				}
				else if (IsWalkingInSquare)
				{
					returnToEndPoint();
					MovePosition(time, Game1.viewport, location);
				}
				else
				{
					if (!followSchedule)
					{
						return;
					}
					MovePosition(time, Game1.viewport, location);
					Warp warp = location.isCollidingWithWarp(GetBoundingBox(), this);
					PropertyValue value = null;
					location.map.GetLayer("Buildings").PickTile(nextPositionPoint(), Game1.viewport.Size)?.Properties.TryGetValue("Action", out value);
					string[] array = value?.ToString().Split(Utility.CharSpace);
					if (array == null)
					{
						standingXY = getStandingXY();
						location.map.GetLayer("Buildings").PickTile(new Location(standingXY.X, standingXY.Y), Game1.viewport.Size)?.Properties.TryGetValue("Action", out value);
						array = value?.ToString().Split(Utility.CharSpace);
					}
					if (warp != null)
					{
						if (location is BusStop && warp.TargetName.Equals("Farm"))
						{
							Point entryLocation = ((isMarried() ? (getHome() as FarmHouse) : Game1.getLocationFromName(getSpouse().homeLocation.Value)) as FarmHouse).getEntryLocation();
							warp = new Warp(warp.X, warp.Y, getSpouse().homeLocation.Value, entryLocation.X, entryLocation.Y, flipFarmer: false);
						}
						else if (location is FarmHouse && warp.TargetName.Equals("Farm"))
						{
							warp = new Warp(warp.X, warp.Y, "BusStop", 0, 23, flipFarmer: false);
						}
						Game1.warpCharacter(this, warp.TargetName, new Vector2(warp.TargetX * 64, warp.TargetY * 64 - Sprite.getHeight() / 2 - 16));
						location.characters.Remove(this);
					}
					else if (array != null && array.Length >= 1 && array[0].Contains("Warp"))
					{
						Game1.warpCharacter(this, array[3], new Vector2(Convert.ToInt32(array[1]), Convert.ToInt32(array[2])));
						if (Game1.currentLocation.name.Equals(location.name) && Utility.isOnScreen(getStandingPosition(), 192) && !Game1.eventUp)
						{
							location.playSound("doorClose", NetAudio.SoundContext.NPC);
						}
						location.characters.Remove(this);
					}
					else if (array != null && array.Length >= 1 && array[0].Contains("Door"))
					{
						location.openDoor(new Location(nextPositionPoint().X / 64, nextPositionPoint().Y / 64), Game1.player.currentLocation.Equals(location));
					}
					else
					{
						if (location.map.GetLayer("Paths") == null)
						{
							return;
						}
						standingXY = getStandingXY();
						Tile tile = location.map.GetLayer("Paths").PickTile(new Location(standingXY.X, standingXY.Y), Game1.viewport.Size);
						Microsoft.Xna.Framework.Rectangle boundingBox = GetBoundingBox();
						boundingBox.Inflate(2, 2);
						if (tile == null || !new Microsoft.Xna.Framework.Rectangle(standingXY.X - standingXY.X % 64, standingXY.Y - standingXY.Y % 64, 64, 64).Contains(boundingBox))
						{
							return;
						}
						switch (tile.TileIndex)
						{
						case 0:
							if (getDirection() == 3)
							{
								SetMovingOnlyUp();
							}
							else if (getDirection() == 2)
							{
								SetMovingOnlyRight();
							}
							break;
						case 1:
							if (getDirection() == 3)
							{
								SetMovingOnlyDown();
							}
							else if (getDirection() == 0)
							{
								SetMovingOnlyRight();
							}
							break;
						case 2:
							if (getDirection() == 1)
							{
								SetMovingOnlyDown();
							}
							else if (getDirection() == 0)
							{
								SetMovingOnlyLeft();
							}
							break;
						case 3:
							if (getDirection() == 1)
							{
								SetMovingOnlyUp();
							}
							else if (getDirection() == 2)
							{
								SetMovingOnlyLeft();
							}
							break;
						case 4:
							changeSchedulePathDirection();
							moveCharacterOnSchedulePath();
							break;
						case 7:
							ReachedEndPoint();
							break;
						case 5:
						case 6:
							break;
						}
					}
				}
			}
			else if (IsWalkingInSquare)
			{
				randomSquareMovement(time);
				MovePosition(time, Game1.viewport, location);
			}
		}

		public void facePlayer(Farmer who)
		{
			if ((int)facingDirectionBeforeSpeakingToPlayer == -1)
			{
				facingDirectionBeforeSpeakingToPlayer.Value = getFacingDirection();
			}
			faceDirection((who.FacingDirection + 2) % 4);
		}

		public void doneFacingPlayer(Farmer who)
		{
		}

		public virtual void UpdateFarmExploration(GameTime time, GameLocation location)
		{
			if (_farmActivities == null)
			{
				InitializeFarmActivities();
			}
			if (_currentFarmActivity != null)
			{
				if (_currentFarmActivity.IsPerformingActivity() && _currentFarmActivity.Update(time))
				{
					_currentFarmActivity.EndActivity();
					_currentFarmActivity = null;
				}
				return;
			}
			nextFarmActivityScan -= (float)time.ElapsedGameTime.TotalSeconds;
			if (nextFarmActivityScan <= 0f)
			{
				bool flag = false;
				if (FindFarmActivity())
				{
					flag = true;
				}
				if (!flag)
				{
					nextFarmActivityScan = 3f;
				}
			}
		}

		public virtual void InitializeFarmActivities()
		{
			_farmActivities = new List<FarmActivity>();
			_farmActivities.Add(new CropWatchActivity().Initialize(this));
			_farmActivities.Add(new FlowerWatchActivity().Initialize(this));
			_farmActivities.Add(new ArtifactSpotWatchActivity().Initialize(this, 0.5f));
			_farmActivities.Add(new TreeActivity().Initialize(this));
			_farmActivities.Add(new ClearingActivity().Initialize(this));
			_farmActivities.Add(new ShrineActivity().Initialize(this, 0.1f));
			_farmActivities.Add(new MailActivity().Initialize(this, 0.1f));
			_farmActivityWeightTotal = 0f;
			foreach (FarmActivity farmActivity in _farmActivities)
			{
				_farmActivityWeightTotal += farmActivity.weight;
			}
		}

		public virtual bool FindFarmActivity()
		{
			if (!(base.currentLocation is Farm))
			{
				return false;
			}
			Farm farm = base.currentLocation as Farm;
			float num = Utility.RandomFloat(0f, _farmActivityWeightTotal);
			FarmActivity farmActivity = null;
			foreach (FarmActivity farmActivity2 in _farmActivities)
			{
				num -= farmActivity2.weight;
				if (num <= 0f)
				{
					if (farmActivity2.AttemptActivity(farm))
					{
						farmActivity = farmActivity2;
					}
					break;
				}
			}
			if (farmActivity != null)
			{
				if (farmActivity.IsTileBlockedFromSight(farmActivity.activityPosition))
				{
					return false;
				}
				if (PathToOnFarm(Utility.Vector2ToPoint(farmActivity.activityPosition), OnFinishPathForActivity))
				{
					_currentFarmActivity = farmActivity;
					return true;
				}
			}
			return false;
		}

		public override void update(GameTime time, GameLocation location)
		{
			if (Game1.IsMasterGame && currentScheduleDelay > 0f)
			{
				currentScheduleDelay -= (float)time.ElapsedGameTime.TotalSeconds;
				if (currentScheduleDelay <= 0f)
				{
					currentScheduleDelay = -1f;
					checkSchedule(Game1.timeOfDay);
					currentScheduleDelay = 0f;
				}
			}
			removeHenchmanEvent.Poll();
			if (Game1.IsMasterGame && (bool)exploreFarm)
			{
				UpdateFarmExploration(time, location);
			}
			if (Game1.IsMasterGame && shouldWearIslandAttire.Value && (base.currentLocation == null || base.currentLocation.GetLocationContext() == GameLocation.LocationContext.Default))
			{
				shouldWearIslandAttire.Value = false;
			}
			if (_startedEndOfRouteBehavior == null && _finishingEndOfRouteBehavior == null && loadedEndOfRouteBehavior != endOfRouteBehaviorName.Value)
			{
				loadEndOfRouteBehavior(endOfRouteBehaviorName);
			}
			if (!currentlyDoingEndOfRouteAnimation && string.Equals(loadedEndOfRouteBehavior, endOfRouteBehaviorName.Value, StringComparison.Ordinal) && (bool)doingEndOfRouteAnimation)
			{
				reallyDoAnimationAtEndOfScheduleRoute();
			}
			else if (currentlyDoingEndOfRouteAnimation && !doingEndOfRouteAnimation)
			{
				finishEndOfRouteAnimation();
			}
			currentlyDoingEndOfRouteAnimation = doingEndOfRouteAnimation;
			if (shouldWearIslandAttire.Value && !isWearingIslandAttire)
			{
				wearIslandAttire();
			}
			else if (!shouldWearIslandAttire.Value && isWearingIslandAttire)
			{
				wearNormalClothes();
			}
			bool value = isSleeping.Value;
			if (value && !isPlayingSleepingAnimation)
			{
				playSleepingAnimation();
			}
			else if (!value && isPlayingSleepingAnimation)
			{
				Sprite.StopAnimation();
				isPlayingSleepingAnimation = false;
			}
			bool value2 = shouldPlayRobinHammerAnimation.Value;
			if (value2 && !isPlayingRobinHammerAnimation)
			{
				doPlayRobinHammerAnimation();
				isPlayingRobinHammerAnimation = true;
			}
			else if (!value2 && isPlayingRobinHammerAnimation)
			{
				Sprite.StopAnimation();
				isPlayingRobinHammerAnimation = false;
			}
			bool value3 = shouldPlaySpousePatioAnimation.Value;
			if (value3 && !isPlayingSpousePatioAnimation)
			{
				doPlaySpousePatioAnimation();
				isPlayingSpousePatioAnimation = true;
			}
			else if (!value3 && isPlayingSpousePatioAnimation)
			{
				Sprite.StopAnimation();
				isPlayingSpousePatioAnimation = false;
			}
			if (returningToEndPoint)
			{
				returnToEndPoint();
				MovePosition(time, Game1.viewport, location);
			}
			else if (temporaryController != null)
			{
				if (temporaryController.update(time))
				{
					bool nPCSchedule = temporaryController.NPCSchedule;
					temporaryController = null;
					if (nPCSchedule)
					{
						currentScheduleDelay = -1f;
						checkSchedule(Game1.timeOfDay);
						currentScheduleDelay = 0f;
					}
				}
				updateEmote(time);
			}
			else
			{
				base.update(time, location);
			}
			if (textAboveHeadTimer > 0)
			{
				if (textAboveHeadPreTimer > 0)
				{
					textAboveHeadPreTimer -= time.ElapsedGameTime.Milliseconds;
				}
				else
				{
					textAboveHeadTimer -= time.ElapsedGameTime.Milliseconds;
					if (textAboveHeadTimer > 500)
					{
						textAboveHeadAlpha = Math.Min(1f, textAboveHeadAlpha + 0.1f);
					}
					else
					{
						textAboveHeadAlpha = Math.Max(0f, textAboveHeadAlpha - 0.04f);
					}
				}
			}
			if (isWalkingInSquare && !returningToEndPoint)
			{
				randomSquareMovement(time);
			}
			if (Sprite != null && Sprite.CurrentAnimation != null && !Game1.eventUp && Game1.IsMasterGame && Sprite.animateOnce(time))
			{
				Sprite.CurrentAnimation = null;
			}
			if (movementPause > 0 && (!Game1.dialogueUp || controller != null))
			{
				freezeMotion = true;
				movementPause -= time.ElapsedGameTime.Milliseconds;
				if (movementPause <= 0)
				{
					freezeMotion = false;
				}
			}
			if (shakeTimer > 0)
			{
				shakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (lastPosition.Equals(base.Position))
			{
				timerSinceLastMovement += time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				timerSinceLastMovement = 0f;
			}
			if ((bool)swimming)
			{
				yOffset = (float)(Math.Cos(time.TotalGameTime.TotalMilliseconds / 2000.0) * 4.0);
				float num = swimTimer;
				swimTimer -= time.ElapsedGameTime.Milliseconds;
				if (timerSinceLastMovement == 0f)
				{
					if (num > 400f && swimTimer <= 400f && location.Equals(Game1.currentLocation))
					{
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(xVelocity) + Math.Abs(yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, getStandingY() - 32), flicker: false, Game1.random.NextDouble() < 0.5, 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
						location.playSound("slosh", NetAudio.SoundContext.NPC);
					}
					if (swimTimer < 0f)
					{
						swimTimer = 800f;
						if (location.Equals(Game1.currentLocation))
						{
							location.playSound("slosh", NetAudio.SoundContext.NPC);
							Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(xVelocity) + Math.Abs(yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, getStandingY() - 32), flicker: false, Game1.random.NextDouble() < 0.5, 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
						}
					}
				}
				else if (swimTimer < 0f)
				{
					swimTimer = 100f;
				}
			}
			if (Game1.IsMasterGame)
			{
				isMovingOnPathFindPath.Value = controller != null && temporaryController != null;
			}
		}

		public virtual void wearIslandAttire()
		{
			try
			{
				Sprite.LoadTexture("Characters\\" + getTextureNameForCharacter(name.Value) + "_Beach");
			}
			catch (ContentLoadException)
			{
				Sprite.LoadTexture("Characters\\" + getTextureNameForCharacter(name.Value));
			}
			isWearingIslandAttire = true;
			resetPortrait();
		}

		public virtual void wearNormalClothes()
		{
			Sprite.LoadTexture("Characters\\" + getTextureNameForCharacter(name.Value));
			isWearingIslandAttire = false;
			resetPortrait();
		}

		public virtual void performTenMinuteUpdate(int timeOfDay, GameLocation l)
		{
			if (Game1.eventUp)
			{
				return;
			}
			if (Game1.random.NextDouble() < 0.1 && Dialogue != null && Dialogue.ContainsKey(string.Concat(l.name, "_Ambient")))
			{
				string[] array = Dialogue[string.Concat(l.name, "_Ambient")].Split('/');
				int preTimer = Game1.random.Next(4) * 1000;
				showTextAboveHead(array[Game1.random.Next(array.Length)], -1, 2, 3000, preTimer);
			}
			else
			{
				if (!isMoving() || !l.isOutdoors || timeOfDay >= 1800 || !(Game1.random.NextDouble() < 0.3 + ((SocialAnxiety == 0) ? 0.25 : ((SocialAnxiety != 1) ? 0.0 : ((Manners == 2) ? (-1.0) : (-0.2))))) || (Age == 1 && (Manners != 1 || SocialAnxiety != 0)) || isMarried())
				{
					return;
				}
				Character character = Utility.isThereAFarmerOrCharacterWithinDistance(getTileLocation(), 4, l);
				if (!character.Name.Equals(base.Name) && !(character is Horse))
				{
					Dictionary<string, string> dictionary = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\NPCDispositions");
					if (dictionary.ContainsKey(base.Name) && !dictionary[base.Name].Split('/')[9].Split(' ').Contains(character.Name) && isFacingToward(character.getTileLocation()))
					{
						sayHiTo(character);
					}
				}
			}
		}

		public void sayHiTo(Character c)
		{
			string hi = getHi(c.displayName);
			if (hi == null)
			{
				return;
			}
			showTextAboveHead(getHi(c.displayName));
			if (c is NPC && Game1.random.NextDouble() < 0.66)
			{
				string hi2 = (c as NPC).getHi(displayName);
				if (hi2 != null)
				{
					(c as NPC).showTextAboveHead((c as NPC).getHi(displayName), -1, 2, 3000, 1000 + Game1.random.Next(500));
				}
			}
		}

		public string getHi(string nameToGreet)
		{
			if (Age == 2)
			{
				if (SocialAnxiety != 1)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4059");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4058");
			}
			if (SocialAnxiety == 1)
			{
				if (!(Game1.random.NextDouble() < 0.5))
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4061");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4060");
			}
			if (SocialAnxiety == 0)
			{
				if (!(Game1.random.NextDouble() < 0.33))
				{
					if (!(Game1.random.NextDouble() < 0.5))
					{
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4068", nameToGreet);
					}
					return ((Game1.timeOfDay < 1200) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4063") : ((Game1.timeOfDay < 1700) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4064") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4065"))) + ", " + Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4066", nameToGreet);
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4062");
			}
			if (!(Game1.random.NextDouble() < 0.33))
			{
				if (!(Game1.random.NextDouble() < 0.5))
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4072");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4071", nameToGreet);
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4060");
		}

		public bool isFacingToward(Vector2 tileLocation)
		{
			return FacingDirection switch
			{
				0 => (float)getTileY() > tileLocation.Y, 
				1 => (float)getTileX() < tileLocation.X, 
				2 => (float)getTileY() < tileLocation.Y, 
				3 => (float)getTileX() > tileLocation.X, 
				_ => false, 
			};
		}

		public virtual void arriveAt(GameLocation l)
		{
			if (!Game1.eventUp && Game1.random.NextDouble() < 0.5 && Dialogue != null && Dialogue.ContainsKey(string.Concat(l.name, "_Entry")))
			{
				string[] array = Dialogue[string.Concat(l.name, "_Entry")].Split('/');
				showTextAboveHead(array[Game1.random.Next(array.Length)]);
			}
		}

		public override void Halt()
		{
			base.Halt();
			shouldPlaySpousePatioAnimation.Value = false;
			isPlayingSleepingAnimation = false;
			isCharging = false;
			base.speed = 2;
			base.addedSpeed = 0;
			if (isSleeping.Value)
			{
				playSleepingAnimation();
				Sprite.UpdateSourceRect();
			}
		}

		public void addExtraDialogues(string dialogues)
		{
			if (updatedDialogueYet)
			{
				if (dialogues != null)
				{
					CurrentDialogue.Push(new Dialogue(dialogues, this));
				}
			}
			else
			{
				extraDialogueMessageToAddThisMorning = dialogues;
			}
		}

		public void PerformDivorce()
		{
			reloadDefaultLocation();
			Game1.warpCharacter(this, defaultMap, DefaultPosition / 64f);
		}

		public string tryToGetMarriageSpecificDialogueElseReturnDefault(string dialogueKey, string defaultMessage = "")
		{
			Dictionary<string, string> dictionary = null;
			bool flag = false;
			if (isRoommate())
			{
				try
				{
					if (Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\MarriageDialogue" + GetDialogueSheetName() + "Roommate") != null)
					{
						flag = true;
						dictionary = Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\MarriageDialogue" + GetDialogueSheetName() + "Roommate");
						if (dictionary != null && dictionary.ContainsKey(dialogueKey))
						{
							return dictionary[dialogueKey];
						}
					}
				}
				catch (Exception)
				{
				}
			}
			if (!flag)
			{
				try
				{
					dictionary = Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\MarriageDialogue" + GetDialogueSheetName());
				}
				catch (Exception)
				{
				}
			}
			if (dictionary != null && dictionary.ContainsKey(dialogueKey))
			{
				return dictionary[dialogueKey];
			}
			dictionary = Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\MarriageDialogue");
			if (isRoommate() && dictionary != null && dictionary.ContainsKey(dialogueKey + "Roommate"))
			{
				return dictionary[dialogueKey + "Roommate"];
			}
			if (dictionary != null && dictionary.ContainsKey(dialogueKey))
			{
				return dictionary[dialogueKey];
			}
			return defaultMessage;
		}

		public void resetCurrentDialogue()
		{
			CurrentDialogue = null;
			shouldSayMarriageDialogue.Value = false;
			currentMarriageDialogue.Clear();
		}

		private Stack<Dialogue> loadCurrentDialogue()
		{
			updatedDialogueYet = true;
			Friendship value;
			int num = (Game1.player.friendshipData.TryGetValue(base.Name, out value) ? (value.Points / 250) : 0);
			Stack<Dialogue> stack = new Stack<Dialogue>();
			Random random = new Random((int)(Game1.stats.DaysPlayed * 77) + (int)Game1.uniqueIDForThisGame / 2 + 2 + (int)defaultPosition.X * 77 + (int)defaultPosition.Y * 777);
			if (random.NextDouble() < 0.025 && num >= 1)
			{
				Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
				if (dictionary.TryGetValue(base.Name, out var value2))
				{
					string[] array = value2.Split('/')[9].Split(' ');
					if (array.Length > 1)
					{
						int num2 = random.Next(array.Length / 2) * 2;
						string text = array[num2];
						string text2 = text;
						if (LocalizedContentManager.CurrentLanguageCode != 0 && Game1.getCharacterFromName(text) != null)
						{
							text2 = Game1.getCharacterFromName(text).displayName;
						}
						string text3 = array[num2 + 1].Replace("'", "").Replace("_", " ");
						string value3;
						bool flag = dictionary.TryGetValue(text, out value3) && value3.Split('/')[4].Equals("male");
						Dictionary<string, string> dictionary2 = Game1.content.Load<Dictionary<string, string>>("Data\\NPCGiftTastes");
						if (dictionary2.ContainsKey(text))
						{
							string text4 = null;
							string text5 = ((text3.Length <= 2 || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja) ? text2 : (flag ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4079", text3) : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4080", text3)));
							string text6 = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4083", text5);
							int num3;
							if (random.NextDouble() < 0.5)
							{
								string[] array2 = dictionary2[text].Split('/')[1].Split(' ');
								num3 = Convert.ToInt32(array2[random.Next(array2.Length)]);
								if (base.Name == "Penny" && text == "Pam")
								{
									while (num3 == 303 || num3 == 346 || num3 == 348 || num3 == 459)
									{
										num3 = Convert.ToInt32(array2[random.Next(array2.Length)]);
									}
								}
								if (Game1.objectInformation.TryGetValue(num3, out var value4))
								{
									text4 = value4.Split('/')[4];
									text6 += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4084", text4);
									if (Age == 2)
									{
										text6 = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4086", text2, text4) + (flag ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4088") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4089"));
									}
									else
									{
										switch (random.Next(5))
										{
										case 0:
											text6 = Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4091", text5, text4);
											break;
										case 1:
											text6 = (flag ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4094", text5, text4) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4097", text5, text4));
											break;
										case 2:
											text6 = (flag ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4100", text5, text4) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4103", text5, text4));
											break;
										case 3:
											text6 = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4106", text5, text4);
											break;
										}
										if (random.NextDouble() < 0.65)
										{
											switch (random.Next(5))
											{
											case 0:
												text6 += (flag ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4109") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4111"));
												break;
											case 1:
												text6 += ((!flag) ? ((random.NextDouble() < 0.5) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4115") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4116")) : ((random.NextDouble() < 0.5) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4113") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4114")));
												break;
											case 2:
												text6 += (flag ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4118") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4120"));
												break;
											case 3:
												text6 += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4125");
												break;
											case 4:
												text6 += (flag ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4126") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4128"));
												break;
											}
											if (text.Equals("Abigail") && random.NextDouble() < 0.5)
											{
												text6 = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4128", text2, text4);
											}
										}
									}
								}
							}
							else
							{
								try
								{
									num3 = Convert.ToInt32(dictionary2[text].Split('/')[7].Split(' ')[random.Next(dictionary2[text].Split('/')[7].Split(' ').Length)]);
								}
								catch (Exception)
								{
									num3 = Convert.ToInt32(dictionary2["Universal_Hate"].Split(' ')[random.Next(dictionary2["Universal_Hate"].Split(' ').Length)]);
								}
								if (Game1.objectInformation.ContainsKey(num3))
								{
									text4 = Game1.objectInformation[num3].Split('/')[4];
									text6 += (flag ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4135", text4, Lexicon.getRandomNegativeFoodAdjective()) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4138", text4, Lexicon.getRandomNegativeFoodAdjective()));
									if (Age == 2)
									{
										text6 = (flag ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4141", text2, text4) : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4144", text2, text4));
									}
									else
									{
										switch (random.Next(4))
										{
										case 0:
											text6 = ((random.NextDouble() < 0.5) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4146") : "") + Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4147", text5, text4);
											break;
										case 1:
											text6 = ((!flag) ? ((random.NextDouble() < 0.5) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4153", text5, text4) : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4154", text5, text4)) : ((random.NextDouble() < 0.5) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4149", text5, text4) : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4152", text5, text4)));
											break;
										case 2:
											text6 = (flag ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4161", text5, text4) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4164", text5, text4));
											break;
										}
										if (random.NextDouble() < 0.65)
										{
											switch (random.Next(5))
											{
											case 0:
												text6 += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4170");
												break;
											case 1:
												text6 += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4171");
												break;
											case 2:
												text6 += (flag ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4172") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4174"));
												break;
											case 3:
												text6 += (flag ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4176") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4178"));
												break;
											case 4:
												text6 += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4180");
												break;
											}
											if (base.Name.Equals("Lewis") && random.NextDouble() < 0.5)
											{
												text6 = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4182", text2, text4);
											}
										}
									}
								}
							}
							if (text4 != null)
							{
								NPC characterFromName = Game1.getCharacterFromName(text);
								if (characterFromName != null)
								{
									text6 = text6 + "%revealtaste" + text + num3;
								}
								stack.Clear();
								if (text6.Length > 0)
								{
									try
									{
										text6 = text6.Substring(0, 1).ToUpper() + text6.Substring(1, text6.Length - 1);
									}
									catch (Exception)
									{
									}
								}
								stack.Push(new Dialogue(text6, this));
								return stack;
							}
						}
					}
				}
			}
			if (Dialogue != null && Dialogue.Count != 0)
			{
				string value5 = "";
				stack.Clear();
				if (Game1.player.spouse != null && Game1.player.spouse == base.Name)
				{
					if (Game1.player.spouse.Equals(base.Name) && Game1.player.isEngaged())
					{
						if (Game1.player.hasCurrentOrPendingRoommate() && Game1.content.Load<Dictionary<string, string>>("Data\\EngagementDialogue").ContainsKey(base.Name + "Roommate0"))
						{
							stack.Push(new Dialogue(Game1.content.Load<Dictionary<string, string>>("Data\\EngagementDialogue")[base.Name + "Roommate" + random.Next(2)], this));
						}
						else if (Game1.content.Load<Dictionary<string, string>>("Data\\EngagementDialogue").ContainsKey(base.Name + "0"))
						{
							stack.Push(new Dialogue(Game1.content.Load<Dictionary<string, string>>("Data\\EngagementDialogue")[base.Name + random.Next(2)], this));
						}
					}
					else if (!Game1.newDay && marriageDefaultDialogue.Value != null && !shouldSayMarriageDialogue.Value)
					{
						stack.Push(marriageDefaultDialogue.Value.GetDialogue(this));
						marriageDefaultDialogue.Value = null;
					}
				}
				else if (idForClones == -1)
				{
					if (Game1.player.friendshipData.ContainsKey(base.Name) && Game1.player.friendshipData[base.Name].IsDivorced())
					{
						try
						{
							stack.Push(new Dialogue(Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\" + GetDialogueSheetName())["divorced"], this));
							return stack;
						}
						catch (Exception)
						{
						}
					}
					if (Game1.isRaining && random.NextDouble() < 0.5 && (base.currentLocation == null || base.currentLocation.GetLocationContext() == GameLocation.LocationContext.Default) && (!base.Name.Equals("Krobus") || !(Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri")) && (!base.Name.Equals("Penny") || !Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade")))
					{
						try
						{
							stack.Push(new Dialogue(Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\rainy")[GetDialogueSheetName()], this));
							return stack;
						}
						catch (Exception)
						{
						}
					}
					Dialogue dialogue = tryToRetrieveDialogue(Game1.currentSeason + "_", num);
					if (dialogue == null)
					{
						dialogue = tryToRetrieveDialogue("", num);
					}
					if (dialogue != null)
					{
						stack.Push(dialogue);
					}
				}
				else
				{
					Dialogue.TryGetValue(idForClones.ToString() ?? "", out value5);
					stack.Push(new Dialogue(value5, this));
				}
			}
			else if (base.Name.Equals("Bouncer"))
			{
				stack.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4192"), this));
			}
			if (extraDialogueMessageToAddThisMorning != null)
			{
				stack.Push(new Dialogue(extraDialogueMessageToAddThisMorning, this));
			}
			return stack;
		}

		public bool checkForNewCurrentDialogue(int heartLevel, bool noPreface = false)
		{
			string text = "";
			foreach (string key in Game1.player.activeDialogueEvents.Keys)
			{
				if (!Dialogue.ContainsKey(key))
				{
					continue;
				}
				text = key;
				if (!text.Equals("") && !Game1.player.mailReceived.Contains(base.Name + "_" + text))
				{
					CurrentDialogue.Clear();
					CurrentDialogue.Push(new Dialogue(Dialogue[text], this));
					if (!key.Contains("dumped"))
					{
						Game1.player.mailReceived.Add(base.Name + "_" + text);
					}
					return true;
				}
			}
			string text2 = ((!Game1.currentSeason.Equals("spring") && !noPreface) ? Game1.currentSeason : "");
			if (Dialogue.ContainsKey(string.Concat(text2, Game1.currentLocation.name, "_", getTileX().ToString(), "_", getTileY().ToString())))
			{
				CurrentDialogue.Push(new Dialogue(Dialogue[string.Concat(text2, Game1.currentLocation.name, "_", getTileX().ToString(), "_", getTileY().ToString())], this)
				{
					removeOnNextMove = true
				});
				return true;
			}
			if (Dialogue.ContainsKey(string.Concat(text2, Game1.currentLocation.name, "_", Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth))))
			{
				CurrentDialogue.Push(new Dialogue(Dialogue[string.Concat(text2, Game1.currentLocation.name, "_", Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth))], this)
				{
					removeOnNextMove = true
				});
				return true;
			}
			if (heartLevel >= 10 && Dialogue.ContainsKey(string.Concat(text2, Game1.currentLocation.name, "10")))
			{
				CurrentDialogue.Push(new Dialogue(Dialogue[string.Concat(text2, Game1.currentLocation.name, "10")], this)
				{
					removeOnNextMove = true
				});
				return true;
			}
			if (heartLevel >= 8 && Dialogue.ContainsKey(string.Concat(text2, Game1.currentLocation.name, "8")))
			{
				CurrentDialogue.Push(new Dialogue(Dialogue[string.Concat(text2, Game1.currentLocation.name, "8")], this)
				{
					removeOnNextMove = true
				});
				return true;
			}
			if (heartLevel >= 6 && Dialogue.ContainsKey(string.Concat(text2, Game1.currentLocation.name, "6")))
			{
				CurrentDialogue.Push(new Dialogue(Dialogue[string.Concat(text2, Game1.currentLocation.name, "6")], this)
				{
					removeOnNextMove = true
				});
				return true;
			}
			if (heartLevel >= 4 && Dialogue.ContainsKey(string.Concat(text2, Game1.currentLocation.name, "4")))
			{
				CurrentDialogue.Push(new Dialogue(Dialogue[string.Concat(text2, Game1.currentLocation.name, "4")], this)
				{
					removeOnNextMove = true
				});
				return true;
			}
			if (heartLevel >= 2 && Dialogue.ContainsKey(string.Concat(text2, Game1.currentLocation.name, "2")))
			{
				CurrentDialogue.Push(new Dialogue(Dialogue[string.Concat(text2, Game1.currentLocation.name, "2")], this)
				{
					removeOnNextMove = true
				});
				return true;
			}
			if (Dialogue.ContainsKey(text2 + Game1.currentLocation.name))
			{
				CurrentDialogue.Push(new Dialogue(Dialogue[text2 + Game1.currentLocation.name], this)
				{
					removeOnNextMove = true
				});
				return true;
			}
			return false;
		}

		public Dialogue tryToRetrieveDialogue(string preface, int heartLevel, string appendToEnd = "")
		{
			int num = Game1.year;
			if (Game1.year > 2)
			{
				num = 2;
			}
			if (Game1.player.spouse != null && Game1.player.spouse.Length > 0 && appendToEnd.Equals(""))
			{
				if (Game1.player.hasCurrentOrPendingRoommate())
				{
					Dialogue dialogue = tryToRetrieveDialogue(preface, heartLevel, "_roommate_" + Game1.player.spouse);
					if (dialogue != null)
					{
						return dialogue;
					}
				}
				else
				{
					Dialogue dialogue2 = tryToRetrieveDialogue(preface, heartLevel, "_inlaw_" + Game1.player.spouse);
					if (dialogue2 != null)
					{
						return dialogue2;
					}
				}
			}
			string text = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
			if (base.Name == "Pierre" && (Game1.isLocationAccessible("CommunityCenter") || Game1.player.HasTownKey) && text == "Wed")
			{
				text = "Sat";
			}
			if (Dialogue.ContainsKey(preface + Game1.dayOfMonth + appendToEnd) && num == 1)
			{
				return new Dialogue(Dialogue[preface + Game1.dayOfMonth + appendToEnd], this);
			}
			if (Dialogue.ContainsKey(preface + Game1.dayOfMonth + "_" + num + appendToEnd))
			{
				return new Dialogue(Dialogue[preface + Game1.dayOfMonth + "_" + num + appendToEnd], this);
			}
			if (heartLevel >= 10 && Dialogue.ContainsKey(preface + text + "10" + appendToEnd))
			{
				if (!Dialogue.ContainsKey(preface + text + "10_" + num + appendToEnd))
				{
					return new Dialogue(Dialogue[preface + text + "10" + appendToEnd], this);
				}
				return new Dialogue(Dialogue[preface + text + "10_" + num + appendToEnd], this);
			}
			if (heartLevel >= 8 && Dialogue.ContainsKey(preface + text + "8" + appendToEnd))
			{
				if (!Dialogue.ContainsKey(preface + text + "8_" + num + appendToEnd))
				{
					return new Dialogue(Dialogue[preface + text + "8" + appendToEnd], this);
				}
				return new Dialogue(Dialogue[preface + text + "8_" + num + appendToEnd], this);
			}
			if (heartLevel >= 6 && Dialogue.ContainsKey(preface + text + "6" + appendToEnd))
			{
				if (!Dialogue.ContainsKey(preface + text + "6_" + num))
				{
					return new Dialogue(Dialogue[preface + text + "6" + appendToEnd], this);
				}
				return new Dialogue(Dialogue[preface + text + "6_" + num + appendToEnd], this);
			}
			if (heartLevel >= 4 && Dialogue.ContainsKey(preface + text + "4" + appendToEnd))
			{
				if (preface == "fall_" && text == "Mon" && base.Name.Equals("Penny") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					if (!Dialogue.ContainsKey(preface + text + "_" + num + appendToEnd))
					{
						return new Dialogue(Dialogue["fall_Mon"], this);
					}
					return new Dialogue(Dialogue[preface + text + "_" + num + appendToEnd], this);
				}
				if (!Dialogue.ContainsKey(preface + text + "4_" + num))
				{
					return new Dialogue(Dialogue[preface + text + "4" + appendToEnd], this);
				}
				return new Dialogue(Dialogue[preface + text + "4_" + num + appendToEnd], this);
			}
			if (heartLevel >= 2 && Dialogue.ContainsKey(preface + text + "2" + appendToEnd))
			{
				if (!Dialogue.ContainsKey(preface + text + "2_" + num))
				{
					return new Dialogue(Dialogue[preface + text + "2" + appendToEnd], this);
				}
				return new Dialogue(Dialogue[preface + text + "2_" + num + appendToEnd], this);
			}
			if (Dialogue.ContainsKey(preface + text + appendToEnd))
			{
				if (base.Name.Equals("Caroline") && Game1.isLocationAccessible("CommunityCenter") && preface == "summer_" && text == "Mon")
				{
					return new Dialogue(Dialogue["summer_Wed"], this);
				}
				if (!Dialogue.ContainsKey(preface + text + "_" + num + appendToEnd))
				{
					return new Dialogue(Dialogue[preface + text + appendToEnd], this);
				}
				return new Dialogue(Dialogue[preface + text + "_" + num + appendToEnd], this);
			}
			return null;
		}

		public void clearSchedule()
		{
			schedule = null;
		}

		public void checkSchedule(int timeOfDay)
		{
			if (currentScheduleDelay == 0f && scheduleDelaySeconds > 0f)
			{
				currentScheduleDelay = scheduleDelaySeconds;
			}
			else
			{
				if (returningToEndPoint)
				{
					return;
				}
				updatedDialogueYet = false;
				extraDialogueMessageToAddThisMorning = null;
				if (ignoreScheduleToday || schedule == null)
				{
					return;
				}
				SchedulePathDescription value = null;
				if (lastAttemptedSchedule < timeOfDay)
				{
					lastAttemptedSchedule = timeOfDay;
					schedule.TryGetValue(timeOfDay, out value);
					if (value != null)
					{
						queuedSchedulePaths.Add(new KeyValuePair<int, SchedulePathDescription>(timeOfDay, value));
					}
					value = null;
				}
				if (controller != null && controller.pathToEndPoint != null && controller.pathToEndPoint.Count > 0)
				{
					return;
				}
				if (queuedSchedulePaths.Count > 0 && timeOfDay >= queuedSchedulePaths[0].Key)
				{
					value = queuedSchedulePaths[0].Value;
				}
				if (value == null)
				{
					return;
				}
				prepareToDisembarkOnNewSchedulePath();
				if (returningToEndPoint || temporaryController != null)
				{
					return;
				}
				directionsToNewLocation = value;
				if (queuedSchedulePaths.Count > 0)
				{
					queuedSchedulePaths.RemoveAt(0);
				}
				controller = new PathFindController(directionsToNewLocation.route, this, Utility.getGameLocationOfCharacter(this))
				{
					finalFacingDirection = directionsToNewLocation.facingDirection,
					endBehaviorFunction = getRouteEndBehaviorFunction(directionsToNewLocation.endOfRouteBehavior, directionsToNewLocation.endOfRouteMessage)
				};
				if (controller.pathToEndPoint == null || controller.pathToEndPoint.Count == 0)
				{
					if (controller.endBehaviorFunction != null)
					{
						controller.endBehaviorFunction(this, base.currentLocation);
					}
					controller = null;
				}
				if (directionsToNewLocation != null && directionsToNewLocation.route != null)
				{
					previousEndPoint = ((directionsToNewLocation.route.Count > 0) ? directionsToNewLocation.route.Last() : Point.Zero);
				}
			}
		}

		private void finishEndOfRouteAnimation()
		{
			_finishingEndOfRouteBehavior = _startedEndOfRouteBehavior;
			_startedEndOfRouteBehavior = null;
			if (_finishingEndOfRouteBehavior == "change_beach")
			{
				shouldWearIslandAttire.Value = true;
				currentlyDoingEndOfRouteAnimation = false;
			}
			else if (_finishingEndOfRouteBehavior == "change_normal")
			{
				shouldWearIslandAttire.Value = false;
				currentlyDoingEndOfRouteAnimation = false;
			}
			while (CurrentDialogue.Count > 0 && CurrentDialogue.Peek().removeOnNextMove)
			{
				CurrentDialogue.Pop();
			}
			shouldSayMarriageDialogue.Value = false;
			currentMarriageDialogue.Clear();
			nextEndOfRouteMessage = null;
			endOfRouteMessage.Value = null;
			if (currentlyDoingEndOfRouteAnimation && routeEndOutro != null)
			{
				List<FarmerSprite.AnimationFrame> list = new List<FarmerSprite.AnimationFrame>();
				for (int i = 0; i < routeEndOutro.Length; i++)
				{
					if (i == routeEndOutro.Length - 1)
					{
						list.Add(new FarmerSprite.AnimationFrame(routeEndOutro[i], 100, 0, secondaryArm: false, flip: false, routeEndAnimationFinished, behaviorAtEndOfFrame: true));
					}
					else
					{
						list.Add(new FarmerSprite.AnimationFrame(routeEndOutro[i], 100, 0, secondaryArm: false, flip: false));
					}
				}
				if (list.Count > 0)
				{
					Sprite.setCurrentAnimation(list);
				}
				else
				{
					routeEndAnimationFinished(null);
				}
				if (_finishingEndOfRouteBehavior != null)
				{
					finishRouteBehavior(_finishingEndOfRouteBehavior);
				}
			}
			else
			{
				routeEndAnimationFinished(null);
			}
		}

		private void prepareToDisembarkOnNewSchedulePath()
		{
			finishEndOfRouteAnimation();
			doingEndOfRouteAnimation.Value = false;
			currentlyDoingEndOfRouteAnimation = false;
			if (!isMarried())
			{
				return;
			}
			if (temporaryController == null && Utility.getGameLocationOfCharacter(this) is FarmHouse)
			{
				temporaryController = new PathFindController(this, getHome(), new Point(getHome().warps[0].X, getHome().warps[0].Y), 2, eraseOldPathController: true)
				{
					NPCSchedule = true
				};
				if (temporaryController.pathToEndPoint == null || temporaryController.pathToEndPoint.Count <= 0)
				{
					temporaryController = null;
					schedule = null;
				}
				else
				{
					followSchedule = true;
				}
			}
			else if (Utility.getGameLocationOfCharacter(this) is Farm)
			{
				temporaryController = null;
				schedule = null;
			}
		}

		public void checkForMarriageDialogue(int timeOfDay, GameLocation location)
		{
			if (base.Name == "Krobus" && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri")
			{
				return;
			}
			switch (timeOfDay)
			{
			case 1100:
				setRandomAfternoonMarriageDialogue(1100, location);
				break;
			case 1800:
				if (location is FarmHouse)
				{
					Random random = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + timeOfDay + (int)getSpouse().UniqueMultiplayerID);
					string text = "";
					int num = random.Next(Game1.isRaining ? 7 : 6) - 1;
					text = ((num < 0) ? base.Name : (num.ToString() ?? ""));
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", (Game1.isRaining ? "Rainy" : "Indoor") + "_Night_" + text, false);
				}
				break;
			}
		}

		private void routeEndAnimationFinished(Farmer who)
		{
			doingEndOfRouteAnimation.Value = false;
			freezeMotion = false;
			Sprite.SpriteHeight = 32;
			Sprite.oldFrame = _beforeEndOfRouteAnimationFrame;
			Sprite.StopAnimation();
			endOfRouteMessage.Value = null;
			isCharging = false;
			base.speed = 2;
			base.addedSpeed = 0;
			goingToDoEndOfRouteAnimation.Value = false;
			if (isWalkingInSquare)
			{
				returningToEndPoint = true;
			}
			if (_finishingEndOfRouteBehavior == "penny_dishes")
			{
				drawOffset.Value = new Vector2(0f, 0f);
			}
			if (appliedRouteAnimationOffset != Vector2.Zero)
			{
				drawOffset.Value = new Vector2(0f, 0f);
				appliedRouteAnimationOffset = Vector2.Zero;
			}
			_finishingEndOfRouteBehavior = null;
		}

		public bool isOnSilentTemporaryMessage()
		{
			if (((bool)doingEndOfRouteAnimation || !goingToDoEndOfRouteAnimation) && endOfRouteMessage.Value != null && endOfRouteMessage.Value.ToLower().Equals("silent"))
			{
				return true;
			}
			return false;
		}

		public bool hasTemporaryMessageAvailable()
		{
			if (isDivorcedFrom(Game1.player))
			{
				return false;
			}
			if (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this))
			{
				return true;
			}
			if (endOfRouteMessage.Value != null && ((bool)doingEndOfRouteAnimation || !goingToDoEndOfRouteAnimation))
			{
				return true;
			}
			return false;
		}

		public bool setTemporaryMessages(Farmer who)
		{
			if (isOnSilentTemporaryMessage())
			{
				return true;
			}
			if (endOfRouteMessage.Value != null && ((bool)doingEndOfRouteAnimation || !goingToDoEndOfRouteAnimation))
			{
				if (!isDivorcedFrom(Game1.player) && (!endOfRouteMessage.Value.Contains("marriage") || getSpouse() == Game1.player))
				{
					_PushTemporaryDialogue(endOfRouteMessage);
					return false;
				}
			}
			else if (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this))
			{
				_PushTemporaryDialogue(base.currentLocation.GetLocationOverrideDialogue(this));
				return false;
			}
			return false;
		}

		protected void _PushTemporaryDialogue(string dialogue_key)
		{
			if (dialogue_key.StartsWith("Resort"))
			{
				string text = "Resort_Marriage" + dialogue_key.Substring(6);
				if (Game1.content.LoadStringReturnNullIfNotFound(text) != null)
				{
					dialogue_key = text;
				}
			}
			if (CurrentDialogue.Count == 0 || CurrentDialogue.Peek().temporaryDialogueKey != dialogue_key)
			{
				Dialogue item = new Dialogue(Game1.content.LoadString(dialogue_key), this)
				{
					removeOnNextMove = true,
					temporaryDialogueKey = dialogue_key
				};
				CurrentDialogue.Push(item);
			}
		}

		private void walkInSquareAtEndOfRoute(Character c, GameLocation l)
		{
			startRouteBehavior(endOfRouteBehaviorName);
		}

		private void doAnimationAtEndOfScheduleRoute(Character c, GameLocation l)
		{
			doingEndOfRouteAnimation.Value = true;
			reallyDoAnimationAtEndOfScheduleRoute();
			currentlyDoingEndOfRouteAnimation = true;
		}

		private void reallyDoAnimationAtEndOfScheduleRoute()
		{
			_startedEndOfRouteBehavior = loadedEndOfRouteBehavior;
			bool flag = false;
			if (_startedEndOfRouteBehavior == "change_beach")
			{
				flag = true;
			}
			else if (_startedEndOfRouteBehavior == "change_normal")
			{
				flag = true;
			}
			if (!flag)
			{
				if (_startedEndOfRouteBehavior == "penny_dishes")
				{
					drawOffset.Value = new Vector2(0f, 16f);
				}
				if (_startedEndOfRouteBehavior.EndsWith("_sleep"))
				{
					layingDown = true;
					HideShadow = true;
				}
				if (routeAnimationMetadata != null)
				{
					for (int i = 0; i < routeAnimationMetadata.Length; i++)
					{
						string[] array = routeAnimationMetadata[i].Split(' ');
						if (array[0] == "laying_down")
						{
							layingDown = true;
							HideShadow = true;
						}
						else if (array[0] == "offset")
						{
							appliedRouteAnimationOffset = new Vector2(int.Parse(array[1]), int.Parse(array[2]));
						}
					}
				}
				if (appliedRouteAnimationOffset != Vector2.Zero)
				{
					drawOffset.Value = appliedRouteAnimationOffset;
				}
				if (_skipRouteEndIntro)
				{
					doMiddleAnimation(null);
				}
				else
				{
					List<FarmerSprite.AnimationFrame> list = new List<FarmerSprite.AnimationFrame>();
					for (int j = 0; j < routeEndIntro.Length; j++)
					{
						if (j == routeEndIntro.Length - 1)
						{
							list.Add(new FarmerSprite.AnimationFrame(routeEndIntro[j], 100, 0, secondaryArm: false, flip: false, doMiddleAnimation, behaviorAtEndOfFrame: true));
						}
						else
						{
							list.Add(new FarmerSprite.AnimationFrame(routeEndIntro[j], 100, 0, secondaryArm: false, flip: false));
						}
					}
					Sprite.setCurrentAnimation(list);
				}
			}
			_skipRouteEndIntro = false;
			doingEndOfRouteAnimation.Value = true;
			freezeMotion = true;
			_beforeEndOfRouteAnimationFrame = Sprite.oldFrame;
		}

		private void doMiddleAnimation(Farmer who)
		{
			List<FarmerSprite.AnimationFrame> list = new List<FarmerSprite.AnimationFrame>();
			for (int i = 0; i < routeEndAnimation.Length; i++)
			{
				list.Add(new FarmerSprite.AnimationFrame(routeEndAnimation[i], 100, 0, secondaryArm: false, flip: false));
			}
			Sprite.setCurrentAnimation(list);
			Sprite.loop = true;
			if (_startedEndOfRouteBehavior != null)
			{
				startRouteBehavior(_startedEndOfRouteBehavior);
			}
		}

		private void startRouteBehavior(string behaviorName)
		{
			if (behaviorName.Length > 0 && behaviorName[0] == '"')
			{
				if (Game1.IsMasterGame)
				{
					endOfRouteMessage.Value = behaviorName.Replace("\"", "");
				}
				return;
			}
			if (behaviorName.Contains("square_") && Game1.IsMasterGame)
			{
				lastCrossroad = new Microsoft.Xna.Framework.Rectangle(getTileX() * 64, getTileY() * 64, 64, 64);
				string[] array = behaviorName.Split('_');
				walkInSquare(Convert.ToInt32(array[1]), Convert.ToInt32(array[2]), 6000);
				if (array.Length > 3)
				{
					squareMovementFacingPreference = Convert.ToInt32(array[3]);
				}
				else
				{
					squareMovementFacingPreference = -1;
				}
			}
			if (behaviorName.Contains("sleep"))
			{
				isPlayingSleepingAnimation = true;
				playSleepingAnimation();
			}
			switch (behaviorName)
			{
			case "abigail_videogames":
				if (Game1.IsMasterGame)
				{
					Game1.multiplayer.broadcastSprites(Utility.getGameLocationOfCharacter(this), new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(167, 1714, 19, 14), 100f, 3, 999999, new Vector2(2f, 3f) * 64f + new Vector2(7f, 12f) * 4f, flicker: false, flipped: false, 0.0002f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 688f
					});
					doEmote(52);
				}
				break;
			case "dick_fish":
				extendSourceRect(0, 32);
				Sprite.tempSpriteHeight = 64;
				drawOffset.Value = new Vector2(0f, 96f);
				Sprite.ignoreSourceRectUpdates = false;
				if (Utility.isOnScreen(Utility.Vector2ToPoint(base.Position), 64, base.currentLocation))
				{
					base.currentLocation.playSoundAt("slosh", getTileLocation());
				}
				break;
			case "clint_hammer":
				extendSourceRect(16, 0);
				Sprite.SpriteWidth = 32;
				Sprite.ignoreSourceRectUpdates = false;
				Sprite.currentFrame = 8;
				Sprite.CurrentAnimation[14] = new FarmerSprite.AnimationFrame(9, 100, 0, secondaryArm: false, flip: false, clintHammerSound);
				break;
			case "birdie_fish":
				extendSourceRect(16, 0);
				Sprite.SpriteWidth = 32;
				Sprite.ignoreSourceRectUpdates = false;
				Sprite.currentFrame = 8;
				break;
			}
		}

		public void playSleepingAnimation()
		{
			isSleeping.Value = true;
			Vector2 value = new Vector2(0f, name.Equals("Sebastian") ? 12 : (-4));
			if (isMarried())
			{
				value.X = -12f;
			}
			drawOffset.Value = value;
			if (!isPlayingSleepingAnimation)
			{
				Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions");
				if (dictionary.ContainsKey(name.Value.ToLower() + "_sleep"))
				{
					int frame = Convert.ToInt32(dictionary[name.Value.ToLower() + "_sleep"].Split('/')[0]);
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(frame, 100, secondaryArm: false, flip: false)
					});
					Sprite.loop = true;
				}
				isPlayingSleepingAnimation = true;
			}
		}

		private void finishRouteBehavior(string behaviorName)
		{
			switch (behaviorName)
			{
			case "abigail_videogames":
				Utility.getGameLocationOfCharacter(this).removeTemporarySpritesWithID(688);
				break;
			case "birdie_fish":
			case "clint_hammer":
			case "dick_fish":
				reloadSprite();
				Sprite.SpriteWidth = 16;
				Sprite.SpriteHeight = 32;
				Sprite.UpdateSourceRect();
				drawOffset.Value = Vector2.Zero;
				Halt();
				movementPause = 1;
				break;
			}
			if (layingDown)
			{
				layingDown = false;
				HideShadow = false;
			}
		}

		public bool IsReturningToEndPoint()
		{
			return returningToEndPoint;
		}

		public void StartActivityWalkInSquare(int square_width, int square_height, int pause_offset)
		{
			lastCrossroad = new Microsoft.Xna.Framework.Rectangle(getTileX() * 64, getTileY() * 64, 64, 64);
			walkInSquare(square_height, square_height, pause_offset);
		}

		public void EndActivityRouteEndBehavior()
		{
			finishEndOfRouteAnimation();
		}

		public void StartActivityRouteEndBehavior(string behavior_name, string end_message)
		{
			getRouteEndBehaviorFunction(behavior_name, end_message)?.Invoke(this, base.currentLocation);
		}

		private PathFindController.endBehavior getRouteEndBehaviorFunction(string behaviorName, string endMessage)
		{
			if (endMessage != null || (behaviorName != null && behaviorName.Length > 0 && behaviorName[0] == '"'))
			{
				nextEndOfRouteMessage = endMessage.Replace("\"", "");
			}
			if (behaviorName != null)
			{
				if (behaviorName.Length > 0 && behaviorName.Contains("square_"))
				{
					endOfRouteBehaviorName.Value = behaviorName;
					return walkInSquareAtEndOfRoute;
				}
				Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions");
				if (behaviorName == "change_beach" || behaviorName == "change_normal")
				{
					endOfRouteBehaviorName.Value = behaviorName;
					goingToDoEndOfRouteAnimation.Value = true;
				}
				else
				{
					if (!dictionary.ContainsKey(behaviorName))
					{
						return null;
					}
					endOfRouteBehaviorName.Value = behaviorName;
					loadEndOfRouteBehavior(endOfRouteBehaviorName);
					goingToDoEndOfRouteAnimation.Value = true;
				}
				return doAnimationAtEndOfScheduleRoute;
			}
			return null;
		}

		private void loadEndOfRouteBehavior(string name)
		{
			loadedEndOfRouteBehavior = name;
			if (name.Length > 0 && name.Contains("square_"))
			{
				return;
			}
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions");
			if (dictionary.ContainsKey(name))
			{
				string[] array = dictionary[name].Split('/');
				routeEndIntro = Utility.parseStringToIntArray(array[0]);
				routeEndAnimation = Utility.parseStringToIntArray(array[1]);
				routeEndOutro = Utility.parseStringToIntArray(array[2]);
				if (array.Length > 3 && array[3] != "")
				{
					nextEndOfRouteMessage = array[3];
				}
				if (array.Length > 4)
				{
					routeAnimationMetadata = array.Skip(4).ToArray();
				}
				else
				{
					routeAnimationMetadata = null;
				}
			}
		}

		public void warp(bool wasOutdoors)
		{
		}

		public void shake(int duration)
		{
			shakeTimer = duration;
		}

		public void setNewDialogue(string s, bool add = false, bool clearOnMovement = false)
		{
			if (!add)
			{
				CurrentDialogue.Clear();
			}
			CurrentDialogue.Push(new Dialogue(s, this)
			{
				removeOnNextMove = clearOnMovement
			});
		}

		public void setNewDialogue(string dialogueSheetName, string dialogueSheetKey, int numberToAppend = -1, bool add = false, bool clearOnMovement = false)
		{
			if (!add)
			{
				CurrentDialogue.Clear();
			}
			string text = ((numberToAppend == -1) ? base.Name : "");
			if (dialogueSheetName.Contains("Marriage"))
			{
				if (getSpouse() == Game1.player)
				{
					CurrentDialogue.Push(new Dialogue(tryToGetMarriageSpecificDialogueElseReturnDefault(dialogueSheetKey + ((numberToAppend != -1) ? (numberToAppend.ToString() ?? "") : "") + text), this)
					{
						removeOnNextMove = clearOnMovement
					});
				}
			}
			else if (Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\" + dialogueSheetName).ContainsKey(dialogueSheetKey + ((numberToAppend != -1) ? (numberToAppend.ToString() ?? "") : "") + text))
			{
				CurrentDialogue.Push(new Dialogue(Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\" + dialogueSheetName)[dialogueSheetKey + ((numberToAppend != -1) ? (numberToAppend.ToString() ?? "") : "") + text], this)
				{
					removeOnNextMove = clearOnMovement
				});
			}
		}

		public string GetDialogueSheetName()
		{
			if (base.Name == "Leo" && DefaultMap != "IslandHut")
			{
				return base.Name + "Mainland";
			}
			return base.Name;
		}

		public void setSpouseRoomMarriageDialogue()
		{
			currentMarriageDialogue.Clear();
			addMarriageDialogue("MarriageDialogue", "spouseRoom_" + base.Name, false);
		}

		public void setRandomAfternoonMarriageDialogue(int time, GameLocation location, bool countAsDailyAfternoon = false)
		{
			if ((base.Name == "Krobus" && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri") || hasSaidAfternoonDialogue.Value)
			{
				return;
			}
			if (countAsDailyAfternoon)
			{
				hasSaidAfternoonDialogue.Value = true;
			}
			Random random = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + time);
			int friendshipHeartLevelForNPC = getSpouse().getFriendshipHeartLevelForNPC(base.Name);
			if (location is FarmHouse && random.NextDouble() < 0.5)
			{
				if (friendshipHeartLevelForNPC < 9)
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", (random.NextDouble() < (double)((float)friendshipHeartLevelForNPC / 11f)) ? "Neutral_" : ("Bad_" + random.Next(10)), false);
				}
				else if (random.NextDouble() < 0.05)
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", Game1.currentSeason + "_" + base.Name, false);
				}
				else if ((friendshipHeartLevelForNPC >= 10 && random.NextDouble() < 0.5) || (friendshipHeartLevelForNPC >= 11 && random.NextDouble() < 0.75))
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", "Good_" + random.Next(10), false);
				}
				else
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", "Neutral_" + random.Next(10), false);
				}
			}
			else if (location is Farm)
			{
				if (random.NextDouble() < 0.2)
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", "Outdoor_" + base.Name, false);
				}
				else
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", "Outdoor_" + random.Next(5), false);
				}
			}
		}

		public bool isBirthday(string season, int day)
		{
			if (Birthday_Season == null)
			{
				return false;
			}
			if (Birthday_Season.Equals(season) && Birthday_Day == day)
			{
				return true;
			}
			return false;
		}

		public Object getFavoriteItem()
		{
			Game1.NPCGiftTastes.TryGetValue(base.Name, out var value);
			if (value != null)
			{
				string[] array = value.Split('/');
				int parentSheetIndex = Convert.ToInt32(array[1].Split(' ')[0]);
				return new Object(parentSheetIndex, 1);
			}
			return null;
		}

		public void receiveGift(Object o, Farmer giver, bool updateGiftLimitInfo = true, float friendshipChangeMultiplier = 1f, bool showResponse = true)
		{
			Game1.NPCGiftTastes.TryGetValue(base.Name, out var value);
			string[] array = value.Split('/');
			float num = 1f;
			switch (o.Quality)
			{
			case 1:
				num = 1.1f;
				break;
			case 2:
				num = 1.25f;
				break;
			case 4:
				num = 1.5f;
				break;
			}
			if (Birthday_Season != null && Game1.currentSeason.Equals(Birthday_Season) && Game1.dayOfMonth == Birthday_Day)
			{
				friendshipChangeMultiplier = 8f;
				string text = ((Manners == 2) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4274") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4275"));
				if (Game1.random.NextDouble() < 0.5)
				{
					text = ((Manners == 2) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4276") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4277"));
				}
				string text2 = ((Manners == 2) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4278") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4279"));
				array[0] = text;
				array[2] = text;
				array[4] = text2;
				array[6] = text2;
				array[8] = ((Manners == 2) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4280") : Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4281"));
			}
			giver?.onGiftGiven(this, o);
			if (getSpouse() != null && getSpouse().Equals(giver))
			{
				friendshipChangeMultiplier /= 2f;
			}
			if (value == null)
			{
				return;
			}
			Game1.stats.GiftsGiven++;
			giver.currentLocation.localSound("give_gift");
			if (updateGiftLimitInfo)
			{
				giver.friendshipData[base.Name].GiftsToday++;
				giver.friendshipData[base.Name].GiftsThisWeek++;
				giver.friendshipData[base.Name].LastGiftDate = new WorldDate(Game1.Date);
			}
			int giftTasteForThisItem = getGiftTasteForThisItem(o);
			switch (giver.FacingDirection)
			{
			case 0:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(80, 50f);
				break;
			case 1:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(72, 50f);
				break;
			case 2:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(64, 50f);
				break;
			case 3:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(88, 50f);
				break;
			}
			List<string> list = new List<string>();
			for (int i = 0; i < 8; i += 2)
			{
				list.Add(array[i]);
			}
			if (base.Name.Equals("Krobus") && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri")
			{
				for (int j = 0; j < list.Count; j++)
				{
					list[j] = "...";
				}
			}
			switch (giftTasteForThisItem)
			{
			case 0:
				if (base.Name.Contains("Dwarf"))
				{
					if (showResponse)
					{
						Game1.drawDialogue(this, giver.canUnderstandDwarves ? list[0] : StardewValley.Dialogue.convertToDwarvish(list[0]));
					}
				}
				else if (showResponse)
				{
					Game1.drawDialogue(this, list[0] + "$h");
				}
				giver.changeFriendship((int)(80f * friendshipChangeMultiplier * num), this);
				doEmote(20);
				faceTowardFarmerForPeriod(15000, 4, faceAway: false, giver);
				return;
			case 6:
				if (base.Name.Contains("Dwarf"))
				{
					if (showResponse)
					{
						Game1.drawDialogue(this, giver.canUnderstandDwarves ? list[3] : StardewValley.Dialogue.convertToDwarvish(list[3]));
					}
				}
				else if (showResponse)
				{
					Game1.drawDialogue(this, list[3] + "$s");
				}
				giver.changeFriendship((int)(-40f * friendshipChangeMultiplier), this);
				faceTowardFarmerForPeriod(15000, 4, faceAway: true, giver);
				doEmote(12);
				return;
			case 2:
				if (base.Name.Contains("Dwarf"))
				{
					if (showResponse)
					{
						Game1.drawDialogue(this, giver.canUnderstandDwarves ? list[1] : StardewValley.Dialogue.convertToDwarvish(list[1]));
					}
				}
				else if (showResponse)
				{
					Game1.drawDialogue(this, list[1] + "$h");
				}
				giver.changeFriendship((int)(45f * friendshipChangeMultiplier * num), this);
				faceTowardFarmerForPeriod(7000, 3, faceAway: true, giver);
				return;
			case 4:
				if (base.Name.Contains("Dwarf"))
				{
					if (showResponse)
					{
						Game1.drawDialogue(this, giver.canUnderstandDwarves ? list[2] : StardewValley.Dialogue.convertToDwarvish(list[2]));
					}
				}
				else if (showResponse)
				{
					Game1.drawDialogue(this, list[2] + "$s");
				}
				giver.changeFriendship((int)(-20f * friendshipChangeMultiplier), this);
				return;
			}
			if (base.Name.Contains("Dwarf"))
			{
				if (showResponse)
				{
					Game1.drawDialogue(this, giver.canUnderstandDwarves ? array[8] : StardewValley.Dialogue.convertToDwarvish(array[8]));
				}
			}
			else if (showResponse)
			{
				Game1.drawDialogue(this, array[8]);
			}
			giver.changeFriendship((int)(20f * friendshipChangeMultiplier), this);
		}

		public override void draw(SpriteBatch b, float alpha = 1f)
		{
			if (Sprite == null || IsInvisible || (!Utility.isOnScreen(base.Position, 128) && (!eventActor || !(base.currentLocation is Summit))))
			{
				return;
			}
			if ((bool)swimming)
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, 80 + yJumpOffset * 2) + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero) - new Vector2(0f, yOffset), new Microsoft.Xna.Framework.Rectangle(Sprite.SourceRect.X, Sprite.SourceRect.Y, Sprite.SourceRect.Width, Sprite.SourceRect.Height / 2 - (int)(yOffset / 4f)), Color.White, rotation, new Vector2(32f, 96f) / 4f, Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f)));
				Vector2 localPosition = getLocalPosition(Game1.viewport);
				b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((int)localPosition.X + (int)yOffset + 8, (int)localPosition.Y - 128 + Sprite.SourceRect.Height * 4 + 48 + yJumpOffset * 2 - (int)yOffset, Sprite.SourceRect.Width * 4 - (int)yOffset * 2 - 16, 4), Game1.staminaRect.Bounds, Color.White * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, (float)getStandingY() / 10000f + 0.001f);
			}
			else
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(GetSpriteWidthForPositioning() * 4 / 2, GetBoundingBox().Height / 2) + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), Sprite.SourceRect, Color.White * alpha, rotation, new Vector2(Sprite.SpriteWidth / 2, (float)Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, scale) * 4f, (flip || (Sprite.CurrentAnimation != null && Sprite.CurrentAnimation[Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f)));
			}
			if (Breather && shakeTimer <= 0 && !swimming && Sprite.currentFrame < 16 && !farmerPassesThrough)
			{
				Microsoft.Xna.Framework.Rectangle sourceRect = Sprite.SourceRect;
				sourceRect.Y += Sprite.SpriteHeight / 2 + Sprite.SpriteHeight / 32;
				sourceRect.Height = Sprite.SpriteHeight / 4;
				sourceRect.X += Sprite.SpriteWidth / 4;
				sourceRect.Width = Sprite.SpriteWidth / 2;
				Vector2 vector = new Vector2(Sprite.SpriteWidth * 4 / 2, 8f);
				if (Age == 2)
				{
					sourceRect.Y += Sprite.SpriteHeight / 6 + 1;
					sourceRect.Height /= 2;
					vector.Y += Sprite.SpriteHeight / 8 * 4;
					if (this is Child)
					{
						if ((this as Child).Age == 0)
						{
							vector.X -= 12f;
						}
						else if ((this as Child).Age == 1)
						{
							vector.X -= 4f;
						}
					}
				}
				else if (Gender == 1)
				{
					sourceRect.Y++;
					vector.Y -= 4f;
					sourceRect.Height /= 2;
				}
				float num = Math.Max(0f, (float)Math.Ceiling(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 600.0 + (double)(defaultPosition.X * 20f))) / 4f);
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + vector + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), sourceRect, Color.White * alpha, rotation, new Vector2(sourceRect.Width / 2, sourceRect.Height / 2 + 1), Math.Max(0.2f, scale) * 4f + num, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.992f : ((float)getStandingY() / 10000f + 0.001f)));
			}
			if (isGlowing)
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(GetSpriteWidthForPositioning() * 4 / 2, GetBoundingBox().Height / 2) + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), Sprite.SourceRect, glowingColor * glowingTransparency, rotation, new Vector2(Sprite.SpriteWidth / 2, (float)Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.99f : ((float)getStandingY() / 10000f + 0.001f)));
			}
			if (base.IsEmoting && !Game1.eventUp && !(this is Child) && !(this is Pet))
			{
				Vector2 localPosition2 = getLocalPosition(Game1.viewport);
				localPosition2.Y -= 32 + Sprite.SpriteHeight * 4;
				b.Draw(Game1.emoteSpriteSheet, localPosition2, new Microsoft.Xna.Framework.Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)getStandingY() / 10000f);
			}
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			if (textAboveHeadTimer > 0 && textAboveHead != null)
			{
				Vector2 vector = Game1.GlobalToLocal(new Vector2(getStandingX(), getStandingY() - Sprite.SpriteHeight * 4 - 64 + yJumpOffset));
				if (textAboveHeadStyle == 0)
				{
					vector += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
				}
				if (NeedsBirdieEmoteHack())
				{
					vector.X += -GetBoundingBox().Width / 4 + 64;
				}
				if (shouldShadowBeOffset)
				{
					vector += drawOffset.Value;
				}
				SpriteText.drawStringWithScrollCenteredAt(b, textAboveHead, (int)vector.X, (int)vector.Y, "", textAboveHeadAlpha, textAboveHeadColor, 1, (float)(getTileY() * 64) / 10000f + 0.001f + (float)getTileX() / 10000f);
			}
		}

		public bool NeedsBirdieEmoteHack()
		{
			if (Game1.eventUp && Sprite.SpriteWidth == 32 && base.Name == "Birdie")
			{
				return true;
			}
			return false;
		}

		public void warpToPathControllerDestination()
		{
			if (controller != null)
			{
				while (controller.pathToEndPoint.Count > 2)
				{
					controller.pathToEndPoint.Pop();
					controller.handleWarps(new Microsoft.Xna.Framework.Rectangle(controller.pathToEndPoint.Peek().X * 64, controller.pathToEndPoint.Peek().Y * 64, 64, 64));
					base.Position = new Vector2(controller.pathToEndPoint.Peek().X * 64, controller.pathToEndPoint.Peek().Y * 64 + 16);
					Halt();
				}
			}
		}

		public virtual Microsoft.Xna.Framework.Rectangle getMugShotSourceRect()
		{
			return new Microsoft.Xna.Framework.Rectangle(0, (Age == 2) ? 4 : 0, 16, 24);
		}

		public void getHitByPlayer(Farmer who, GameLocation location)
		{
			doEmote(12);
			if (who == null)
			{
				if (Game1.IsMultiplayer)
				{
					return;
				}
				who = Game1.player;
			}
			if (who.friendshipData.ContainsKey(base.Name))
			{
				who.changeFriendship(-30, this);
				if (who.IsLocalPlayer)
				{
					CurrentDialogue.Clear();
					CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4293") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4294"), this));
				}
				location.debris.Add(new Debris(Sprite.textureName, Game1.random.Next(3, 8), new Vector2(GetBoundingBox().Center.X, GetBoundingBox().Center.Y)));
			}
			if (base.Name.Equals("Bouncer"))
			{
				location.localSound("crafting");
			}
			else
			{
				location.localSound("hitEnemy");
			}
		}

		public void walkInSquare(int squareWidth, int squareHeight, int squarePauseOffset)
		{
			isWalkingInSquare = true;
			lengthOfWalkingSquareX = squareWidth;
			lengthOfWalkingSquareY = squareHeight;
			this.squarePauseOffset = squarePauseOffset;
		}

		public void moveTowardPlayer(int threshold)
		{
			isWalkingTowardPlayer.Value = true;
			moveTowardPlayerThreshold.Value = threshold;
		}

		protected virtual Farmer findPlayer()
		{
			return Game1.MasterPlayer;
		}

		public virtual bool withinPlayerThreshold()
		{
			return withinPlayerThreshold(moveTowardPlayerThreshold);
		}

		public virtual bool withinPlayerThreshold(int threshold)
		{
			if (base.currentLocation != null && !base.currentLocation.farmers.Any())
			{
				return false;
			}
			Vector2 tileLocation = findPlayer().getTileLocation();
			Vector2 tileLocation2 = getTileLocation();
			if (Math.Abs(tileLocation2.X - tileLocation.X) <= (float)threshold && Math.Abs(tileLocation2.Y - tileLocation.Y) <= (float)threshold)
			{
				return true;
			}
			return false;
		}

		private Stack<Point> addToStackForSchedule(Stack<Point> original, Stack<Point> toAdd, string location, Stack<string> originalLocNames, out Stack<string> locNames)
		{
			if (toAdd == null)
			{
				locNames = new Stack<string>(originalLocNames);
				return original;
			}
			original = new Stack<Point>(original);
			originalLocNames = new Stack<string>(originalLocNames);
			locNames = new Stack<string>();
			foreach (Point item in toAdd)
			{
				locNames.Push(location);
			}
			while (original.Count > 0)
			{
				toAdd.Push(original.Pop());
				if (originalLocNames.Count > 0)
				{
					locNames.Push(originalLocNames.Pop());
				}
			}
			return toAdd;
		}

		private SchedulePathDescription pathfindToNextScheduleLocation(string startingLocation, int startingX, int startingY, string endingLocation, int endingX, int endingY, int finalFacingDirection, string endBehavior, string endMessage)
		{
			Stack<Point> stack = new Stack<Point>();
			Stack<string> stack2 = new Stack<string>();
			Stack<string> locNames = new Stack<string>();
			Point startPoint = new Point(startingX, startingY);
			List<string> list = ((!startingLocation.Equals(endingLocation, StringComparison.Ordinal)) ? getLocationRoute(startingLocation, endingLocation) : null);
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					GameLocation locationFromName = Game1.getLocationFromName(list[i]);
					if (locationFromName.Name.Equals("Trailer") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
					{
						locationFromName = Game1.getLocationFromName("Trailer_Big");
					}
					if (i < list.Count - 1)
					{
						Point warpPointTo = locationFromName.getWarpPointTo(list[i + 1]);
						if (warpPointTo.Equals(Point.Zero) || startPoint.Equals(Point.Zero))
						{
							throw new Exception("schedule pathing tried to find a warp point that doesn't exist.");
						}
						stack = addToStackForSchedule(stack, PathFindController.findPathForNPCSchedules(startPoint, warpPointTo, locationFromName, 30000), locationFromName.Name, stack2, out locNames);
						stack2 = locNames;
						startPoint = locationFromName.getWarpPointTarget(warpPointTo, this);
					}
					else
					{
						stack = addToStackForSchedule(stack, PathFindController.findPathForNPCSchedules(startPoint, new Point(endingX, endingY), locationFromName, 30000), locationFromName.Name, stack2, out locNames);
						stack2 = locNames;
					}
				}
			}
			else if (startingLocation.Equals(endingLocation, StringComparison.Ordinal))
			{
				GameLocation locationFromName2 = Game1.getLocationFromName(startingLocation);
				if (locationFromName2.Name.Equals("Trailer") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					locationFromName2 = Game1.getLocationFromName("Trailer_Big");
				}
				stack = PathFindController.findPathForNPCSchedules(startPoint, new Point(endingX, endingY), locationFromName2, 30000);
				stack2 = addLocationNamesToPath(stack, startingLocation);
			}
			return new SchedulePathDescription(stack, finalFacingDirection, endBehavior, endMessage, stack2);
		}

		private Stack<string> addLocationNamesToPath(Stack<Point> path, string startingLocation)
		{
			Stack<string> stack = new Stack<string>();
			if (path != null)
			{
				foreach (Point item in path)
				{
					stack.Push(startingLocation);
				}
				return stack;
			}
			return stack;
		}

		private List<string> getLocationRoute(string startingLocation, string endingLocation)
		{
			foreach (List<string> item in routesFromLocationToLocation)
			{
				if (item.First().Equals(startingLocation, StringComparison.Ordinal) && item.Last().Equals(endingLocation, StringComparison.Ordinal) && ((int)gender == 0 || !item.Contains("BathHouse_MensLocker", StringComparer.Ordinal)) && ((int)gender != 0 || !item.Contains("BathHouse_WomensLocker", StringComparer.Ordinal)))
				{
					return item;
				}
			}
			return null;
		}

		private bool changeScheduleForLocationAccessibility(ref string locationName, ref int tileX, ref int tileY, ref int facingDirection)
		{
			switch (locationName)
			{
			case "JojaMart":
			case "Railroad":
				if (!Game1.isLocationAccessible(locationName))
				{
					if (!hasMasterScheduleEntry(locationName + "_Replacement"))
					{
						return true;
					}
					string[] array = getMasterScheduleEntry(locationName + "_Replacement").Split(' ');
					locationName = array[0];
					tileX = Convert.ToInt32(array[1]);
					tileY = Convert.ToInt32(array[2]);
					facingDirection = Convert.ToInt32(array[3]);
				}
				break;
			case "CommunityCenter":
				return !Game1.isLocationAccessible(locationName);
			}
			return false;
		}

		public Dictionary<int, SchedulePathDescription> parseMasterSchedule(string rawData)
		{
			string[] array = rawData.Split('/');
			Dictionary<int, SchedulePathDescription> dictionary = new Dictionary<int, SchedulePathDescription>();
			int num = 0;
			if (array[0].Contains("GOTO"))
			{
				string text = array[0].Split(' ')[1];
				if (text.ToLower().Equals("season"))
				{
					text = Game1.currentSeason;
				}
				try
				{
					array = getMasterScheduleRawData()[text].Split('/');
				}
				catch (Exception)
				{
					return parseMasterSchedule(getMasterScheduleEntry("spring"));
				}
			}
			if (array[0].Contains("NOT"))
			{
				string[] array2 = array[0].Split(' ');
				string text2 = array2[1];
				string text3 = text2.ToLower();
				if (text3 == "friendship")
				{
					int i = 2;
					bool flag = false;
					for (; i < array2.Length; i += 2)
					{
						string text4 = array2[i];
						int result = 0;
						if (int.TryParse(array2[i + 1], out result))
						{
							foreach (Farmer allFarmer in Game1.getAllFarmers())
							{
								if (allFarmer.getFriendshipHeartLevelForNPC(text4) >= result)
								{
									flag = true;
									break;
								}
							}
						}
						if (flag)
						{
							break;
						}
					}
					if (flag)
					{
						return parseMasterSchedule(getMasterScheduleEntry("spring"));
					}
					num++;
				}
			}
			else if (array[0].Contains("MAIL"))
			{
				string[] array3 = array[0].Split(' ');
				string item = array3[1];
				num = ((!Game1.MasterPlayer.mailReceived.Contains(item) && !NetWorldState.checkAnywhereForWorldStateID(item)) ? (num + 1) : (num + 2));
			}
			if (array[num].Contains("GOTO"))
			{
				string text5 = array[num].Split(' ')[1];
				if (text5.ToLower().Equals("season"))
				{
					text5 = Game1.currentSeason;
				}
				else if (text5.ToLower().Equals("no_schedule"))
				{
					followSchedule = false;
					return null;
				}
				return parseMasterSchedule(getMasterScheduleEntry(text5));
			}
			Point point = (isMarried() ? new Point(0, 23) : new Point((int)defaultPosition.X / 64, (int)defaultPosition.Y / 64));
			string text6 = (isMarried() ? "BusStop" : ((string)defaultMap));
			int val = 610;
			string text7 = DefaultMap;
			int num2 = (int)(defaultPosition.X / 64f);
			int num3 = (int)(defaultPosition.Y / 64f);
			bool flag2 = false;
			for (int j = num; j < array.Length; j++)
			{
				if (array.Length <= 1)
				{
					break;
				}
				int num4 = 0;
				string[] array4 = array[j].Split(' ');
				int num5 = 0;
				bool flag3 = false;
				string text8 = array4[num4];
				if (text8.Length > 0 && array4[num4][0] == 'a')
				{
					flag3 = true;
					text8 = text8.Substring(1);
				}
				num5 = Convert.ToInt32(text8);
				num4++;
				string locationName = array4[num4];
				string endBehavior = null;
				string endMessage = null;
				int result2 = 0;
				int result3 = 0;
				int result4 = 2;
				if (locationName == "bed")
				{
					if (isMarried())
					{
						locationName = "BusStop";
						result2 = -1;
						result3 = 23;
						result4 = 3;
					}
					else
					{
						string text9 = null;
						if (hasMasterScheduleEntry("default"))
						{
							text9 = getMasterScheduleEntry("default");
						}
						else if (hasMasterScheduleEntry("spring"))
						{
							text9 = getMasterScheduleEntry("spring");
						}
						if (text9 != null)
						{
							try
							{
								string[] array5 = text9.Split('/');
								string text10 = array5[array5.Length - 1];
								string[] array6 = text10.Split(' ');
								locationName = array6[1];
								if (array6.Length > 3)
								{
									if (!int.TryParse(array6[2], out result2) || !int.TryParse(array6[3], out result3))
									{
										text9 = null;
									}
								}
								else
								{
									text9 = null;
								}
							}
							catch (Exception)
							{
								text9 = null;
							}
						}
						if (text9 == null)
						{
							locationName = text7;
							result2 = num2;
							result3 = num3;
						}
					}
					num4++;
					Dictionary<string, string> dictionary2 = Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions");
					string text11 = name.Value.ToLower() + "_sleep";
					if (dictionary2.ContainsKey(text11))
					{
						endBehavior = text11;
					}
				}
				else
				{
					if (int.TryParse(locationName, out var _))
					{
						locationName = text6;
						num4--;
					}
					num4++;
					result2 = Convert.ToInt32(array4[num4]);
					num4++;
					result3 = Convert.ToInt32(array4[num4]);
					num4++;
					try
					{
						if (array4.Length > num4)
						{
							if (int.TryParse(array4[num4], out result4))
							{
								num4++;
							}
							else
							{
								result4 = 2;
							}
						}
					}
					catch (Exception)
					{
						result4 = 2;
					}
				}
				if (changeScheduleForLocationAccessibility(ref locationName, ref result2, ref result3, ref result4))
				{
					if (getMasterScheduleRawData().ContainsKey("default"))
					{
						return parseMasterSchedule(getMasterScheduleEntry("default"));
					}
					return parseMasterSchedule(getMasterScheduleEntry("spring"));
				}
				if (num4 < array4.Length)
				{
					if (array4[num4].Length > 0 && array4[num4][0] == '"')
					{
						endMessage = array[j].Substring(array[j].IndexOf('"'));
					}
					else
					{
						string text12 = array4[num4];
						endBehavior = text12;
						num4++;
						if (num4 < array4.Length && array4[num4].Length > 0 && array4[num4][0] == '"')
						{
							endMessage = array[j].Substring(array[j].IndexOf('"')).Replace("\"", "");
						}
					}
				}
				if (num5 == 0)
				{
					flag2 = true;
					text7 = locationName;
					num2 = result2;
					num3 = result3;
					text6 = locationName;
					point.X = result2;
					point.Y = result3;
					previousEndPoint = new Point(result2, result3);
					continue;
				}
				SchedulePathDescription schedulePathDescription = pathfindToNextScheduleLocation(text6, point.X, point.Y, locationName, result2, result3, result4, endBehavior, endMessage);
				if (flag3)
				{
					int num6 = 0;
					Point? point2 = null;
					foreach (Point item2 in schedulePathDescription.route)
					{
						if (!point2.HasValue)
						{
							point2 = item2;
							continue;
						}
						if (Math.Abs(point2.Value.X - item2.X) + Math.Abs(point2.Value.Y - item2.Y) == 1)
						{
							num6 += 64;
						}
						point2 = item2;
					}
					int num7 = num6 / 2;
					int num8 = 420;
					int num9 = (int)Math.Round((float)num7 / (float)num8) * 10;
					num5 = Math.Max(Utility.ConvertMinutesToTime(Utility.ConvertTimeToMinutes(num5) - num9), val);
				}
				dictionary.Add(num5, schedulePathDescription);
				point.X = result2;
				point.Y = result3;
				text6 = locationName;
				val = num5;
			}
			if (Game1.IsMasterGame && flag2)
			{
				Game1.warpCharacter(this, text7, new Point(num2, num3));
			}
			if (_lastLoadedScheduleKey != null && Game1.IsMasterGame)
			{
				dayScheduleName.Value = _lastLoadedScheduleKey;
			}
			return dictionary;
		}

		public Dictionary<int, SchedulePathDescription> getSchedule(int dayOfMonth)
		{
			if (!base.Name.Equals("Robin") || Game1.player.currentUpgrade != null)
			{
				IsInvisible = false;
			}
			if ((base.Name.Equals("Willy") && Game1.stats.DaysPlayed < 2) || daysUntilNotInvisible > 0)
			{
				IsInvisible = true;
			}
			else if (Schedule != null)
			{
				followSchedule = true;
			}
			if (getMasterScheduleRawData() == null)
			{
				return null;
			}
			if (islandScheduleName != null && islandScheduleName.Value != null && islandScheduleName.Value != "")
			{
				nameOfTodaysSchedule = islandScheduleName.Value;
				return Schedule;
			}
			if (isMarried())
			{
				if (hasMasterScheduleEntry("marriage_" + Game1.currentSeason + "_" + Game1.dayOfMonth))
				{
					nameOfTodaysSchedule = "marriage_" + Game1.currentSeason + "_" + Game1.dayOfMonth;
					return parseMasterSchedule(getMasterScheduleEntry("marriage_" + Game1.currentSeason + "_" + Game1.dayOfMonth));
				}
				string text = Game1.shortDayNameFromDayOfSeason(dayOfMonth);
				if ((base.Name.Equals("Penny") && (text.Equals("Tue") || text.Equals("Wed") || text.Equals("Fri"))) || (base.Name.Equals("Maru") && (text.Equals("Tue") || text.Equals("Thu"))) || (base.Name.Equals("Harvey") && (text.Equals("Tue") || text.Equals("Thu"))))
				{
					nameOfTodaysSchedule = "marriageJob";
					return parseMasterSchedule(getMasterScheduleEntry("marriageJob"));
				}
				if (!Game1.isRaining && hasMasterScheduleEntry("marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
				{
					nameOfTodaysSchedule = "marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
					return parseMasterSchedule(getMasterScheduleEntry("marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)));
				}
				followSchedule = false;
				return null;
			}
			if (hasMasterScheduleEntry(Game1.currentSeason + "_" + Game1.dayOfMonth))
			{
				return parseMasterSchedule(getMasterScheduleEntry(Game1.currentSeason + "_" + Game1.dayOfMonth));
			}
			int num = Utility.GetAllPlayerFriendshipLevel(this);
			if (num >= 0)
			{
				num /= 250;
			}
			while (num > 0)
			{
				if (hasMasterScheduleEntry(Game1.dayOfMonth + "_" + num))
				{
					return parseMasterSchedule(getMasterScheduleEntry(Game1.dayOfMonth + "_" + num));
				}
				num--;
			}
			if (hasMasterScheduleEntry(string.Empty + Game1.dayOfMonth))
			{
				return parseMasterSchedule(getMasterScheduleEntry(string.Empty + Game1.dayOfMonth));
			}
			if (base.Name.Equals("Pam") && Game1.player.mailReceived.Contains("ccVault"))
			{
				return parseMasterSchedule(getMasterScheduleEntry("bus"));
			}
			if (Game1.IsRainingHere(base.currentLocation))
			{
				if (Game1.random.NextDouble() < 0.5 && hasMasterScheduleEntry("rain2"))
				{
					return parseMasterSchedule(getMasterScheduleEntry("rain2"));
				}
				if (hasMasterScheduleEntry("rain"))
				{
					return parseMasterSchedule(getMasterScheduleEntry("rain"));
				}
			}
			List<string> list = new List<string>
			{
				Game1.currentSeason,
				Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)
			};
			num = Utility.GetAllPlayerFriendshipLevel(this);
			if (num >= 0)
			{
				num /= 250;
			}
			while (num > 0)
			{
				list.Add(string.Empty + num);
				if (hasMasterScheduleEntry(string.Join("_", list)))
				{
					return parseMasterSchedule(getMasterScheduleEntry(string.Join("_", list)));
				}
				num--;
				list.RemoveAt(list.Count - 1);
			}
			if (hasMasterScheduleEntry(string.Join("_", list)))
			{
				return parseMasterSchedule(getMasterScheduleEntry(string.Join("_", list)));
			}
			if (hasMasterScheduleEntry(Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
			{
				return parseMasterSchedule(getMasterScheduleEntry(Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)));
			}
			if (hasMasterScheduleEntry(Game1.currentSeason))
			{
				return parseMasterSchedule(getMasterScheduleEntry(Game1.currentSeason));
			}
			if (hasMasterScheduleEntry("spring_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
			{
				return parseMasterSchedule(getMasterScheduleEntry("spring_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)));
			}
			list.RemoveAt(list.Count - 1);
			list.Add("spring");
			num = Utility.GetAllPlayerFriendshipLevel(this);
			if (num >= 0)
			{
				num /= 250;
			}
			while (num > 0)
			{
				list.Add(string.Empty + num);
				if (hasMasterScheduleEntry(string.Join("_", list)))
				{
					return parseMasterSchedule(getMasterScheduleEntry(string.Join("_", list)));
				}
				num--;
				list.RemoveAt(list.Count - 1);
			}
			if (hasMasterScheduleEntry("spring"))
			{
				return parseMasterSchedule(getMasterScheduleEntry("spring"));
			}
			return null;
		}

		public virtual void handleMasterScheduleFileLoadError(Exception e)
		{
		}

		public virtual void InvalidateMasterSchedule()
		{
			_hasLoadedMasterScheduleData = false;
		}

		public Dictionary<string, string> getMasterScheduleRawData()
		{
			if (!_hasLoadedMasterScheduleData)
			{
				_hasLoadedMasterScheduleData = true;
				try
				{
					if (base.Name == "Leo")
					{
						if (DefaultMap == "IslandHut")
						{
							_masterScheduleData = Game1.content.Load<Dictionary<string, string>>("Characters\\schedules\\" + base.Name);
						}
						else
						{
							_masterScheduleData = Game1.content.Load<Dictionary<string, string>>("Characters\\schedules\\" + base.Name + "Mainland");
						}
					}
					else
					{
						_masterScheduleData = Game1.content.Load<Dictionary<string, string>>("Characters\\schedules\\" + base.Name);
					}
				}
				catch (Exception e)
				{
					handleMasterScheduleFileLoadError(e);
				}
			}
			return _masterScheduleData;
		}

		public string getMasterScheduleEntry(string schedule_key)
		{
			if (getMasterScheduleRawData() == null)
			{
				throw new KeyNotFoundException("The schedule file for NPC '" + base.Name + "' could not be loaded...");
			}
			if (_masterScheduleData.TryGetValue(schedule_key, out var value))
			{
				_lastLoadedScheduleKey = schedule_key;
				return value;
			}
			throw new KeyNotFoundException("The schedule file for NPC '" + base.Name + "' has no schedule named '" + schedule_key + "'.");
		}

		public bool hasMasterScheduleEntry(string key)
		{
			if (getMasterScheduleRawData() == null)
			{
				return false;
			}
			return getMasterScheduleRawData().ContainsKey(key);
		}

		public virtual bool isRoommate()
		{
			if (!isVillager())
			{
				return false;
			}
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer.spouse != null && allFarmer.spouse.Equals(base.Name) && !allFarmer.isEngaged() && allFarmer.isRoommate(base.Name))
				{
					return true;
				}
			}
			return false;
		}

		public bool isMarried()
		{
			if (!isVillager())
			{
				return false;
			}
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer.spouse != null && allFarmer.spouse.Equals(base.Name) && !allFarmer.isEngaged())
				{
					return true;
				}
			}
			return false;
		}

		public bool isMarriedOrEngaged()
		{
			if (!isVillager())
			{
				return false;
			}
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer.spouse != null && allFarmer.spouse.Equals(base.Name))
				{
					return true;
				}
			}
			return false;
		}

		public virtual void dayUpdate(int dayOfMonth)
		{
			isMovingOnPathFindPath.Value = false;
			queuedSchedulePaths.Clear();
			lastAttemptedSchedule = -1;
			if (layingDown)
			{
				layingDown = false;
				HideShadow = false;
			}
			exploreFarm.Value = false;
			shouldWearIslandAttire.Value = false;
			if (isWearingIslandAttire)
			{
				wearNormalClothes();
			}
			layingDown = false;
			if (appliedRouteAnimationOffset != Vector2.Zero)
			{
				drawOffset.Value = Vector2.Zero;
				appliedRouteAnimationOffset = Vector2.Zero;
			}
			if (base.currentLocation != null && defaultMap.Value != null)
			{
				Game1.warpCharacter(this, defaultMap, defaultPosition.Value / 64f);
			}
			if (base.Name.Equals("Maru") || base.Name.Equals("Shane"))
			{
				Sprite.LoadTexture("Characters\\" + getTextureNameForCharacter(base.Name));
			}
			if (base.Name.Equals("Willy") || base.Name.Equals("Clint"))
			{
				Sprite.SpriteWidth = 16;
				Sprite.SpriteHeight = 32;
				Sprite.ignoreSourceRectUpdates = false;
				Sprite.UpdateSourceRect();
				IsInvisible = false;
			}
			if (Game1.IsMasterGame && base.Name.Equals("Elliott") && Game1.netWorldState.Value.hasWorldStateID("elliottGone"))
			{
				daysUntilNotInvisible = 7;
				Game1.netWorldState.Value.removeWorldStateID("elliottGone");
				Game1.worldStateIDs.Remove("elliottGone");
			}
			drawOffset.Value = Vector2.Zero;
			if (Game1.IsMasterGame && daysUntilNotInvisible > 0)
			{
				IsInvisible = true;
				daysUntilNotInvisible--;
				if (daysUntilNotInvisible <= 0)
				{
					IsInvisible = false;
				}
			}
			resetForNewDay(dayOfMonth);
			updateConstructionAnimation();
			if (isMarried() && !getSpouse().divorceTonight && !IsInvisible)
			{
				marriageDuties();
			}
		}

		public virtual void resetForNewDay(int dayOfMonth)
		{
			sleptInBed.Value = true;
			if (isMarried() && !isRoommate())
			{
				FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(getSpouse());
				if (homeOfFarmer != null && homeOfFarmer.GetSpouseBed() == null)
				{
					sleptInBed.Value = false;
				}
			}
			doingEndOfRouteAnimation.Value = false;
			Halt();
			hasBeenKissedToday.Value = false;
			currentMarriageDialogue.Clear();
			marriageDefaultDialogue.Value = null;
			shouldSayMarriageDialogue.Value = false;
			isSleeping.Value = false;
			drawOffset.Value = Vector2.Zero;
			faceTowardFarmer = false;
			faceTowardFarmerTimer = 0;
			drawOffset.Value = Vector2.Zero;
			hasSaidAfternoonDialogue.Value = false;
			isPlayingSleepingAnimation = false;
			ignoreScheduleToday = false;
			Halt();
			controller = null;
			temporaryController = null;
			directionsToNewLocation = null;
			faceDirection(DefaultFacingDirection);
			previousEndPoint = new Point((int)defaultPosition.X / 64, (int)defaultPosition.Y / 64);
			isWalkingInSquare = false;
			returningToEndPoint = false;
			lastCrossroad = Microsoft.Xna.Framework.Rectangle.Empty;
			_startedEndOfRouteBehavior = null;
			_finishingEndOfRouteBehavior = null;
			if (isVillager())
			{
				Schedule = getSchedule(dayOfMonth);
			}
			endOfRouteMessage.Value = null;
		}

		public void returnHomeFromFarmPosition(Farm farm)
		{
			Farmer spouse = getSpouse();
			if (spouse == null)
			{
				return;
			}
			FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(spouse);
			Point porchStandingSpot = homeOfFarmer.getPorchStandingSpot();
			if (exploreFarm.Value)
			{
				drawOffset.Value = Vector2.Zero;
				GameLocation home = getHome();
				string value = home.Name;
				if (home.uniqueName.Value != null)
				{
					value = home.uniqueName.Value;
				}
				base.willDestroyObjectsUnderfoot = true;
				Point warpPointTo = farm.getWarpPointTo(value, this);
				Game1.player.getSpouse().PathToOnFarm(warpPointTo);
			}
			else if (getTileLocationPoint().Equals(porchStandingSpot))
			{
				drawOffset.Value = Vector2.Zero;
				GameLocation home2 = getHome();
				string value2 = home2.Name;
				if (home2.uniqueName.Value != null)
				{
					value2 = home2.uniqueName.Value;
				}
				base.willDestroyObjectsUnderfoot = true;
				Point warpPointTo2 = farm.getWarpPointTo(value2, this);
				controller = new PathFindController(this, farm, warpPointTo2, 0)
				{
					NPCSchedule = true
				};
			}
			else if (!shouldPlaySpousePatioAnimation.Value || !farm.farmers.Any())
			{
				drawOffset.Value = Vector2.Zero;
				Halt();
				controller = null;
				temporaryController = null;
				ignoreScheduleToday = true;
				Game1.warpCharacter(this, homeOfFarmer, Utility.PointToVector2(homeOfFarmer.getKitchenStandingSpot()));
			}
		}

		public virtual Vector2 GetSpousePatioPosition()
		{
			return Utility.PointToVector2(Game1.getFarm().spousePatioSpot);
		}

		public void setUpForOutdoorPatioActivity()
		{
			Vector2 spousePatioPosition = GetSpousePatioPosition();
			if (!checkTileOccupancyForSpouse(Game1.getFarm(), spousePatioPosition))
			{
				Game1.warpCharacter(this, "Farm", spousePatioPosition);
				popOffAnyNonEssentialItems();
				currentMarriageDialogue.Clear();
				addMarriageDialogue("MarriageDialogue", "patio_" + base.Name, false);
				setTilePosition((int)spousePatioPosition.X, (int)spousePatioPosition.Y);
				shouldPlaySpousePatioAnimation.Value = true;
			}
		}

		private void doPlaySpousePatioAnimation()
		{
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\SpousePatios");
			if (dictionary != null && dictionary.ContainsKey(base.Name))
			{
				string text = dictionary[base.Name];
				string[] array = text.Split('/');
				if (array.Length > 2)
				{
					int[] array2 = Utility.parseStringToIntArray(array[2]);
					Point zero = Point.Zero;
					if (array.Length > 3)
					{
						string[] array3 = array[3].Split(' ');
						if (array3.Length > 1)
						{
							zero.X = int.Parse(array3[0]) * 4;
							zero.Y = int.Parse(array3[1]) * 4;
						}
					}
					drawOffset.Value = Utility.PointToVector2(zero);
					List<FarmerSprite.AnimationFrame> list = new List<FarmerSprite.AnimationFrame>();
					for (int i = 0; i < array2.Length; i++)
					{
						list.Add(new FarmerSprite.AnimationFrame(array2[i], 100, 0, secondaryArm: false, flip: false));
					}
					Sprite.setCurrentAnimation(list);
					return;
				}
			}
			string text2 = name;
			if (text2 == null)
			{
				return;
			}
			switch (text2.Length)
			{
			case 5:
				switch (text2[0])
				{
				case 'E':
					if (text2 == "Emily")
					{
						Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(54, 4000, 64, secondaryArm: false, flip: false)
						});
					}
					break;
				case 'S':
					if (text2 == "Shane")
					{
						Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(28, 4000, 64, secondaryArm: false, flip: false),
							new FarmerSprite.AnimationFrame(29, 800, 64, secondaryArm: false, flip: false)
						});
					}
					break;
				case 'P':
					if (text2 == "Penny")
					{
						Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(18, 6000),
							new FarmerSprite.AnimationFrame(19, 500)
						});
					}
					break;
				case 'H':
					if (text2 == "Haley")
					{
						Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(30, 2000),
							new FarmerSprite.AnimationFrame(31, 200),
							new FarmerSprite.AnimationFrame(24, 2000),
							new FarmerSprite.AnimationFrame(25, 1000),
							new FarmerSprite.AnimationFrame(32, 200),
							new FarmerSprite.AnimationFrame(33, 2000),
							new FarmerSprite.AnimationFrame(32, 200),
							new FarmerSprite.AnimationFrame(25, 2000),
							new FarmerSprite.AnimationFrame(32, 200),
							new FarmerSprite.AnimationFrame(33, 2000)
						});
					}
					break;
				}
				break;
			case 7:
				switch (text2[0])
				{
				case 'E':
					if (text2 == "Elliott")
					{
						Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(33, 3000),
							new FarmerSprite.AnimationFrame(32, 500),
							new FarmerSprite.AnimationFrame(33, 3000),
							new FarmerSprite.AnimationFrame(32, 500),
							new FarmerSprite.AnimationFrame(33, 2000),
							new FarmerSprite.AnimationFrame(34, 1500)
						});
					}
					break;
				case 'A':
					if (text2 == "Abigail")
					{
						Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(16, 500),
							new FarmerSprite.AnimationFrame(17, 500),
							new FarmerSprite.AnimationFrame(18, 500),
							new FarmerSprite.AnimationFrame(19, 500)
						});
					}
					break;
				}
				break;
			case 4:
				switch (text2[0])
				{
				case 'A':
					if (text2 == "Alex")
					{
						Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(34, 4000),
							new FarmerSprite.AnimationFrame(33, 300),
							new FarmerSprite.AnimationFrame(28, 200),
							new FarmerSprite.AnimationFrame(29, 100),
							new FarmerSprite.AnimationFrame(30, 100),
							new FarmerSprite.AnimationFrame(31, 100),
							new FarmerSprite.AnimationFrame(32, 100),
							new FarmerSprite.AnimationFrame(31, 100),
							new FarmerSprite.AnimationFrame(30, 100),
							new FarmerSprite.AnimationFrame(29, 100),
							new FarmerSprite.AnimationFrame(28, 800),
							new FarmerSprite.AnimationFrame(29, 100),
							new FarmerSprite.AnimationFrame(30, 100),
							new FarmerSprite.AnimationFrame(31, 100),
							new FarmerSprite.AnimationFrame(32, 100),
							new FarmerSprite.AnimationFrame(31, 100),
							new FarmerSprite.AnimationFrame(30, 100),
							new FarmerSprite.AnimationFrame(29, 100),
							new FarmerSprite.AnimationFrame(28, 800),
							new FarmerSprite.AnimationFrame(33, 200)
						});
					}
					break;
				case 'M':
					if (text2 == "Maru")
					{
						Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(16, 4000),
							new FarmerSprite.AnimationFrame(17, 200),
							new FarmerSprite.AnimationFrame(18, 200),
							new FarmerSprite.AnimationFrame(19, 200),
							new FarmerSprite.AnimationFrame(20, 200),
							new FarmerSprite.AnimationFrame(21, 200),
							new FarmerSprite.AnimationFrame(22, 200),
							new FarmerSprite.AnimationFrame(23, 200)
						});
					}
					break;
				case 'L':
					if (text2 == "Leah")
					{
						Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(16, 100),
							new FarmerSprite.AnimationFrame(17, 100),
							new FarmerSprite.AnimationFrame(18, 100),
							new FarmerSprite.AnimationFrame(19, 300),
							new FarmerSprite.AnimationFrame(16, 100),
							new FarmerSprite.AnimationFrame(17, 100),
							new FarmerSprite.AnimationFrame(18, 100),
							new FarmerSprite.AnimationFrame(19, 1000),
							new FarmerSprite.AnimationFrame(16, 100),
							new FarmerSprite.AnimationFrame(17, 100),
							new FarmerSprite.AnimationFrame(18, 100),
							new FarmerSprite.AnimationFrame(19, 300),
							new FarmerSprite.AnimationFrame(16, 100),
							new FarmerSprite.AnimationFrame(17, 100),
							new FarmerSprite.AnimationFrame(18, 100),
							new FarmerSprite.AnimationFrame(19, 300),
							new FarmerSprite.AnimationFrame(16, 100),
							new FarmerSprite.AnimationFrame(17, 100),
							new FarmerSprite.AnimationFrame(18, 100),
							new FarmerSprite.AnimationFrame(19, 2000)
						});
					}
					break;
				}
				break;
			case 9:
				if (text2 == "Sebastian")
				{
					drawOffset.Value = new Vector2(16f, 40f);
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(32, 500, 64, secondaryArm: false, flip: false),
						new FarmerSprite.AnimationFrame(36, 500, 64, secondaryArm: false, flip: false),
						new FarmerSprite.AnimationFrame(32, 500, 64, secondaryArm: false, flip: false),
						new FarmerSprite.AnimationFrame(36, 500, 64, secondaryArm: false, flip: false),
						new FarmerSprite.AnimationFrame(32, 500, 64, secondaryArm: false, flip: false),
						new FarmerSprite.AnimationFrame(36, 500, 64, secondaryArm: false, flip: false),
						new FarmerSprite.AnimationFrame(32, 500, 64, secondaryArm: false, flip: false),
						new FarmerSprite.AnimationFrame(36, 2000, 64, secondaryArm: false, flip: false),
						new FarmerSprite.AnimationFrame(33, 100, 64, secondaryArm: false, flip: false),
						new FarmerSprite.AnimationFrame(34, 100, 64, secondaryArm: false, flip: false),
						new FarmerSprite.AnimationFrame(35, 3000, 64, secondaryArm: false, flip: false),
						new FarmerSprite.AnimationFrame(34, 100, 64, secondaryArm: false, flip: false),
						new FarmerSprite.AnimationFrame(33, 100, 64, secondaryArm: false, flip: false),
						new FarmerSprite.AnimationFrame(32, 1500, 64, secondaryArm: false, flip: false)
					});
				}
				break;
			case 3:
				if (text2 == "Sam")
				{
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(25, 3000),
						new FarmerSprite.AnimationFrame(27, 500),
						new FarmerSprite.AnimationFrame(26, 100),
						new FarmerSprite.AnimationFrame(28, 100),
						new FarmerSprite.AnimationFrame(27, 500),
						new FarmerSprite.AnimationFrame(25, 2000),
						new FarmerSprite.AnimationFrame(27, 500),
						new FarmerSprite.AnimationFrame(26, 100),
						new FarmerSprite.AnimationFrame(29, 100),
						new FarmerSprite.AnimationFrame(30, 100),
						new FarmerSprite.AnimationFrame(32, 500),
						new FarmerSprite.AnimationFrame(31, 1000),
						new FarmerSprite.AnimationFrame(30, 100),
						new FarmerSprite.AnimationFrame(29, 100)
					});
				}
				break;
			case 6:
				if (text2 == "Harvey")
				{
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(42, 6000),
						new FarmerSprite.AnimationFrame(43, 1000),
						new FarmerSprite.AnimationFrame(39, 100),
						new FarmerSprite.AnimationFrame(43, 500),
						new FarmerSprite.AnimationFrame(39, 100),
						new FarmerSprite.AnimationFrame(43, 1000),
						new FarmerSprite.AnimationFrame(42, 5000),
						new FarmerSprite.AnimationFrame(43, 3000)
					});
				}
				break;
			case 8:
				break;
			}
		}

		public bool isGaySpouse()
		{
			Farmer spouse = getSpouse();
			if (spouse != null)
			{
				if (Gender != 0 || !spouse.IsMale)
				{
					if (Gender == 1)
					{
						return !spouse.IsMale;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		public bool canGetPregnant()
		{
			if (this is Horse || base.Name.Equals("Krobus") || isRoommate())
			{
				return false;
			}
			Farmer spouse = getSpouse();
			if (spouse == null || (bool)spouse.divorceTonight)
			{
				return false;
			}
			int friendshipHeartLevelForNPC = spouse.getFriendshipHeartLevelForNPC(base.Name);
			Friendship spouseFriendship = spouse.GetSpouseFriendship();
			List<Child> children = spouse.getChildren();
			defaultMap.Value = spouse.homeLocation.Value;
			FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(spouse);
			if (homeOfFarmer.cribStyle.Value <= 0)
			{
				return false;
			}
			if (homeOfFarmer.upgradeLevel >= 2 && spouseFriendship.DaysUntilBirthing < 0 && friendshipHeartLevelForNPC >= 10 && spouse.GetDaysMarried() >= 7)
			{
				if (children.Count != 0)
				{
					if (children.Count < 2)
					{
						return children[0].Age > 2;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		public void marriageDuties()
		{
			if (!Game1.newDay && Game1.gameMode != 6)
			{
				return;
			}
			Farmer spouse = getSpouse();
			if (spouse == null)
			{
				return;
			}
			shouldSayMarriageDialogue.Value = true;
			DefaultMap = spouse.homeLocation.Value;
			FarmHouse farmHouse = Game1.getLocationFromName(spouse.homeLocation.Value) as FarmHouse;
			Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + (int)spouse.UniqueMultiplayerID);
			int friendshipHeartLevelForNPC = spouse.getFriendshipHeartLevelForNPC(base.Name);
			if (Game1.IsMasterGame && (base.currentLocation == null || !base.currentLocation.Equals(farmHouse)))
			{
				Game1.warpCharacter(this, spouse.homeLocation.Value, farmHouse.getSpouseBedSpot(base.Name));
			}
			if (Game1.isRaining)
			{
				marriageDefaultDialogue.Value = new MarriageDialogueReference("MarriageDialogue", "Rainy_Day_" + random.Next(5), false);
			}
			else
			{
				marriageDefaultDialogue.Value = new MarriageDialogueReference("MarriageDialogue", "Indoor_Day_" + random.Next(5), false);
			}
			currentMarriageDialogue.Add(new MarriageDialogueReference(marriageDefaultDialogue.Value.DialogueFile, marriageDefaultDialogue.Value.DialogueKey, marriageDefaultDialogue.Value.IsGendered, marriageDefaultDialogue.Value.Substitutions));
			if (spouse.GetSpouseFriendship().DaysUntilBirthing == 0)
			{
				setTilePosition(farmHouse.getKitchenStandingSpot());
				currentMarriageDialogue.Clear();
				return;
			}
			if (daysAfterLastBirth >= 0)
			{
				daysAfterLastBirth--;
				switch (getSpouse().getChildrenCount())
				{
				case 1:
					setTilePosition(farmHouse.getKitchenStandingSpot());
					if (!spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false), farmHouse))
					{
						currentMarriageDialogue.Clear();
						addMarriageDialogue("MarriageDialogue", "OneKid_" + random.Next(4), false);
					}
					return;
				case 2:
					setTilePosition(farmHouse.getKitchenStandingSpot());
					if (!spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false), farmHouse))
					{
						currentMarriageDialogue.Clear();
						addMarriageDialogue("MarriageDialogue", "TwoKids_" + random.Next(4), false);
					}
					return;
				}
			}
			setTilePosition(farmHouse.getKitchenStandingSpot());
			if (!sleptInBed.Value)
			{
				currentMarriageDialogue.Clear();
				addMarriageDialogue("MarriageDialogue", "NoBed_" + random.Next(4), false);
				return;
			}
			if (tryToGetMarriageSpecificDialogueElseReturnDefault(Game1.currentSeason + "_" + Game1.dayOfMonth).Length > 0)
			{
				if (spouse != null)
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", Game1.currentSeason + "_" + Game1.dayOfMonth, false);
				}
				return;
			}
			if (schedule != null)
			{
				if (nameOfTodaysSchedule.Equals("marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", "funLeave_" + base.Name, false);
				}
				else if (nameOfTodaysSchedule.Equals("marriageJob"))
				{
					currentMarriageDialogue.Clear();
					addMarriageDialogue("MarriageDialogue", "jobLeave_" + base.Name, false);
				}
				return;
			}
			if (!Game1.isRaining && !Game1.IsWinter && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Sat") && spouse == Game1.MasterPlayer && !base.Name.Equals("Krobus"))
			{
				setUpForOutdoorPatioActivity();
				return;
			}
			if (spouse.GetDaysMarried() >= 1 && random.NextDouble() < (double)(1f - (float)Math.Max(1, friendshipHeartLevelForNPC) / 12f))
			{
				Furniture randomFurniture = farmHouse.getRandomFurniture(random);
				if (randomFurniture != null && randomFurniture.isGroundFurniture() && randomFurniture.furniture_type.Value != 15 && randomFurniture.furniture_type.Value != 12)
				{
					Point tilePosition = new Point((int)randomFurniture.tileLocation.X - 1, (int)randomFurniture.tileLocation.Y);
					if (farmHouse.isTileLocationTotallyClearAndPlaceable(tilePosition.X, tilePosition.Y))
					{
						setTilePosition(tilePosition);
						faceDirection(1);
						switch (random.Next(10))
						{
						case 0:
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4420", false);
							break;
						case 1:
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4421", false);
							break;
						case 2:
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4422", true);
							break;
						case 3:
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4423", false);
							break;
						case 4:
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4424", false);
							break;
						case 5:
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4425", false);
							break;
						case 6:
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4426", false);
							break;
						case 7:
							if (Gender == 1)
							{
								if (random.NextDouble() < 0.5)
								{
									currentMarriageDialogue.Clear();
									addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4427", false);
								}
								else
								{
									currentMarriageDialogue.Clear();
									addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4429", false);
								}
							}
							else
							{
								currentMarriageDialogue.Clear();
								addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4431", false);
							}
							break;
						case 8:
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4432", false);
							break;
						case 9:
							currentMarriageDialogue.Clear();
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4433", false);
							break;
						}
						return;
					}
				}
				MarriageDialogueReference marriageDialogueReference = null;
				switch (random.Next(5))
				{
				case 0:
					marriageDialogueReference = new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4434", false);
					break;
				case 1:
					marriageDialogueReference = new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4435", false);
					break;
				case 2:
					marriageDialogueReference = new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4436", false);
					break;
				case 3:
					marriageDialogueReference = new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4437", true);
					break;
				case 4:
					marriageDialogueReference = new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4438", false);
					break;
				}
				spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false), farmHouse, force: true);
				return;
			}
			Friendship spouseFriendship = spouse.GetSpouseFriendship();
			if (spouseFriendship.DaysUntilBirthing != -1 && spouseFriendship.DaysUntilBirthing <= 7 && random.NextDouble() < 0.5)
			{
				if (isGaySpouse())
				{
					setTilePosition(farmHouse.getKitchenStandingSpot());
					if (!spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4439", false), farmHouse))
					{
						if (random.NextDouble() < 0.5)
						{
							currentMarriageDialogue.Clear();
						}
						if (random.NextDouble() < 0.5)
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4440", false, getSpouse().displayName);
						}
						else
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4441", false, "%endearment");
						}
					}
					return;
				}
				if (Gender == 1)
				{
					setTilePosition(farmHouse.getKitchenStandingSpot());
					if (!spouseObstacleCheck((random.NextDouble() < 0.5) ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4442", false) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4443", false), farmHouse))
					{
						if (random.NextDouble() < 0.5)
						{
							currentMarriageDialogue.Clear();
						}
						currentMarriageDialogue.Add((random.NextDouble() < 0.5) ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4444", false, getSpouse().displayName) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4445", false, "%endearment"));
					}
					return;
				}
				setTilePosition(farmHouse.getKitchenStandingSpot());
				if (!spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4446", true), farmHouse))
				{
					if (random.NextDouble() < 0.5)
					{
						currentMarriageDialogue.Clear();
					}
					currentMarriageDialogue.Add((random.NextDouble() < 0.5) ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4447", true, getSpouse().displayName) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4448", false, "%endearment"));
				}
				return;
			}
			if (random.NextDouble() < 0.07)
			{
				switch (getSpouse().getChildrenCount())
				{
				case 1:
					setTilePosition(farmHouse.getKitchenStandingSpot());
					if (!spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4449", true), farmHouse))
					{
						currentMarriageDialogue.Clear();
						addMarriageDialogue("MarriageDialogue", "OneKid_" + random.Next(4), false);
					}
					return;
				case 2:
					setTilePosition(farmHouse.getKitchenStandingSpot());
					if (!spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4452", true), farmHouse))
					{
						currentMarriageDialogue.Clear();
						addMarriageDialogue("MarriageDialogue", "TwoKids_" + random.Next(4), false);
					}
					return;
				}
			}
			Farm farm = Game1.getFarm();
			if (currentMarriageDialogue.Count > 0 && currentMarriageDialogue[0].IsItemGrabDialogue(this))
			{
				setTilePosition(farmHouse.getKitchenStandingSpot());
				spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4455", true), farmHouse);
			}
			else if (!Game1.isRaining && random.NextDouble() < 0.4 && !checkTileOccupancyForSpouse(farm, Utility.PointToVector2(farmHouse.getPorchStandingSpot())) && !base.Name.Equals("Krobus"))
			{
				bool flag = false;
				if (!farm.petBowlWatered.Value && !hasSomeoneFedThePet)
				{
					flag = true;
					farm.petBowlWatered.Set(newValue: true);
					hasSomeoneFedThePet = true;
				}
				if (random.NextDouble() < 0.6 && !Game1.currentSeason.Equals("winter") && !hasSomeoneWateredCrops)
				{
					Vector2 vector = Vector2.Zero;
					int i = 0;
					bool flag2 = false;
					for (; i < Math.Min(50, farm.terrainFeatures.Count()); i++)
					{
						if (!vector.Equals(Vector2.Zero))
						{
							break;
						}
						int index = random.Next(farm.terrainFeatures.Count());
						if (farm.terrainFeatures.Pairs.ElementAt(index).Value is HoeDirt)
						{
							if ((farm.terrainFeatures.Pairs.ElementAt(index).Value as HoeDirt).needsWatering())
							{
								vector = farm.terrainFeatures.Pairs.ElementAt(index).Key;
							}
							else if ((farm.terrainFeatures.Pairs.ElementAt(index).Value as HoeDirt).crop != null)
							{
								flag2 = true;
							}
						}
					}
					if (!vector.Equals(Vector2.Zero))
					{
						Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle((int)vector.X - 30, (int)vector.Y - 30, 60, 60);
						Vector2 key = default(Vector2);
						for (int j = rectangle.X; j < rectangle.Right; j++)
						{
							for (int k = rectangle.Y; k < rectangle.Bottom; k++)
							{
								key.X = j;
								key.Y = k;
								if (farm.isTileOnMap(key) && farm.terrainFeatures.ContainsKey(key) && farm.terrainFeatures[key] is HoeDirt && Game1.IsMasterGame && (farm.terrainFeatures[key] as HoeDirt).needsWatering())
								{
									(farm.terrainFeatures[key] as HoeDirt).state.Value = 1;
								}
							}
						}
						faceDirection(2);
						currentMarriageDialogue.Clear();
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4462", true);
						if (flag)
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
						}
						addMarriageDialogue("MarriageDialogue", "Outdoor_" + random.Next(5), false);
						hasSomeoneWateredCrops = true;
					}
					else
					{
						faceDirection(2);
						if (flag2)
						{
							currentMarriageDialogue.Clear();
							if (Game1.gameMode == 6)
							{
								if (random.NextDouble() < 0.5)
								{
									addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4465", false, "%endearment");
								}
								else
								{
									addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4466", false, "%endearment");
									addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4462", true);
									if (flag)
									{
										addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
									}
								}
							}
							else
							{
								currentMarriageDialogue.Clear();
								addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4470", true);
							}
						}
						else
						{
							currentMarriageDialogue.Clear();
							addMarriageDialogue("MarriageDialogue", "Outdoor_" + random.Next(5), false);
						}
					}
				}
				else if (random.NextDouble() < 0.6 && !hasSomeoneFedTheAnimals)
				{
					bool flag3 = false;
					foreach (Building building in farm.buildings)
					{
						if ((building is Barn || building is Coop) && (int)building.daysOfConstructionLeft <= 0)
						{
							if (Game1.IsMasterGame)
							{
								(building.indoors.Value as AnimalHouse).feedAllAnimals();
							}
							flag3 = true;
						}
					}
					faceDirection(2);
					if (flag3)
					{
						hasSomeoneFedTheAnimals = true;
						currentMarriageDialogue.Clear();
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4474", true);
						if (flag)
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
						}
						addMarriageDialogue("MarriageDialogue", "Outdoor_" + random.Next(5), false);
					}
					else
					{
						currentMarriageDialogue.Clear();
						addMarriageDialogue("MarriageDialogue", "Outdoor_" + random.Next(5), false);
					}
					if (Game1.IsMasterGame)
					{
						farm.petBowlWatered.Set(newValue: true);
					}
				}
				else if (!hasSomeoneRepairedTheFences)
				{
					int l = 0;
					faceDirection(2);
					Vector2 vector2 = Vector2.Zero;
					for (; l < Math.Min(50, farm.objects.Count()); l++)
					{
						if (!vector2.Equals(Vector2.Zero))
						{
							break;
						}
						int index2 = random.Next(farm.objects.Count());
						if (farm.objects.Pairs.ElementAt(index2).Value is Fence)
						{
							vector2 = farm.objects.Pairs.ElementAt(index2).Key;
						}
					}
					if (!vector2.Equals(Vector2.Zero))
					{
						Microsoft.Xna.Framework.Rectangle rectangle2 = new Microsoft.Xna.Framework.Rectangle((int)vector2.X - 10, (int)vector2.Y - 10, 20, 20);
						Vector2 key2 = default(Vector2);
						for (int m = rectangle2.X; m < rectangle2.Right; m++)
						{
							for (int n = rectangle2.Y; n < rectangle2.Bottom; n++)
							{
								key2.X = m;
								key2.Y = n;
								if (farm.isTileOnMap(key2) && farm.objects.ContainsKey(key2) && farm.objects[key2] is Fence && Game1.IsMasterGame)
								{
									(farm.objects[key2] as Fence).repair();
								}
							}
						}
						currentMarriageDialogue.Clear();
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4481", true);
						if (flag)
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
						}
						addMarriageDialogue("MarriageDialogue", "Outdoor_" + random.Next(5), false);
						hasSomeoneRepairedTheFences = true;
					}
					else
					{
						currentMarriageDialogue.Clear();
						addMarriageDialogue("MarriageDialogue", "Outdoor_" + random.Next(5), false);
					}
				}
				Game1.warpCharacter(this, "Farm", farmHouse.getPorchStandingSpot());
				popOffAnyNonEssentialItems();
				faceDirection(2);
			}
			else if (base.Name.Equals("Krobus") && Game1.isRaining && random.NextDouble() < 0.4 && !checkTileOccupancyForSpouse(farm, Utility.PointToVector2(farmHouse.getPorchStandingSpot())))
			{
				addMarriageDialogue("MarriageDialogue", "Outdoor_" + random.Next(5), false);
				Game1.warpCharacter(this, "Farm", farmHouse.getPorchStandingSpot());
				popOffAnyNonEssentialItems();
				faceDirection(2);
			}
			else if (spouse.GetDaysMarried() >= 1 && random.NextDouble() < 0.045)
			{
				if (random.NextDouble() < 0.75)
				{
					Point randomOpenPointInHouse = farmHouse.getRandomOpenPointInHouse(random, 1);
					Furniture furniture = null;
					try
					{
						furniture = new Furniture(Utility.getRandomSingleTileFurniture(random), new Vector2(randomOpenPointInHouse.X, randomOpenPointInHouse.Y));
					}
					catch (Exception)
					{
						furniture = null;
					}
					if (furniture != null && randomOpenPointInHouse.X > 0 && farmHouse.isTileLocationOpen(new Location(randomOpenPointInHouse.X - 1, randomOpenPointInHouse.Y)))
					{
						farmHouse.furniture.Add(furniture);
						setTilePosition(randomOpenPointInHouse.X - 1, randomOpenPointInHouse.Y);
						faceDirection(1);
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4486", false, "%endearmentlower");
						if (Game1.random.NextDouble() < 0.5)
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4488", true);
						}
						else
						{
							addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4489", false);
						}
					}
					else
					{
						setTilePosition(farmHouse.getKitchenStandingSpot());
						spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4490", false), farmHouse);
					}
					return;
				}
				Point randomOpenPointInHouse2 = farmHouse.getRandomOpenPointInHouse(random);
				if (randomOpenPointInHouse2.X <= 0)
				{
					return;
				}
				setTilePosition(randomOpenPointInHouse2.X, randomOpenPointInHouse2.Y);
				faceDirection(0);
				if (random.NextDouble() < 0.5)
				{
					string wallpaperID = farmHouse.GetWallpaperID(randomOpenPointInHouse2.X, randomOpenPointInHouse2.Y);
					if (wallpaperID == null)
					{
						return;
					}
					int num = random.Next(112);
					List<int> list = new List<int>();
					string text = base.Name;
					if (text != null)
					{
						switch (text.Length)
						{
						case 5:
							switch (text[0])
							{
							case 'H':
								if (text == "Haley")
								{
									list.AddRange(new int[7] { 1, 7, 10, 35, 49, 84, 99 });
								}
								break;
							case 'S':
								if (text == "Shane")
								{
									list.AddRange(new int[3] { 6, 21, 60 });
								}
								break;
							}
							break;
						case 4:
							switch (text[0])
							{
							case 'L':
								if (text == "Leah")
								{
									list.AddRange(new int[7] { 44, 108, 43, 45, 92, 37, 29 });
								}
								break;
							case 'A':
								if (text == "Alex")
								{
									list.AddRange(new int[1] { 6 });
								}
								break;
							}
							break;
						case 9:
							if (text == "Sebastian")
							{
								list.AddRange(new int[11]
								{
									3, 4, 12, 14, 30, 46, 47, 56, 58, 59,
									107
								});
							}
							break;
						case 7:
							if (text == "Abigail")
							{
								list.AddRange(new int[10] { 2, 13, 23, 26, 46, 45, 64, 77, 106, 107 });
							}
							break;
						case 6:
							if (text == "Krobus")
							{
								list.AddRange(new int[2] { 23, 24 });
							}
							break;
						}
					}
					if (list.Count > 0)
					{
						num = list[random.Next(list.Count)];
					}
					farmHouse.SetWallpaper(num.ToString(), wallpaperID);
					addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4496", false);
				}
				else
				{
					int floorAt = farmHouse.getFloorAt(randomOpenPointInHouse2);
					if (floorAt != -1)
					{
						farmHouse.setFloor(random.Next(40), floorAt, persist: true);
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4497", false);
					}
				}
			}
			else if (Game1.isRaining && random.NextDouble() < 0.08 && friendshipHeartLevelForNPC < 11 && base.Name != "Krobus")
			{
				foreach (Furniture item in farmHouse.furniture)
				{
					if ((int)item.furniture_type == 13 && farmHouse.isTileLocationTotallyClearAndPlaceable((int)item.tileLocation.X, (int)item.tileLocation.Y + 1))
					{
						setTilePosition((int)item.tileLocation.X, (int)item.tileLocation.Y + 1);
						faceDirection(0);
						currentMarriageDialogue.Clear();
						addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4498", true);
						return;
					}
				}
				spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4499", false), farmHouse, force: true);
			}
			else if (random.NextDouble() < 0.45)
			{
				Vector2 vector3 = Utility.PointToVector2(farmHouse.GetSpouseRoomSpot());
				setTilePosition((int)vector3.X, (int)vector3.Y);
				faceDirection(0);
				setSpouseRoomMarriageDialogue();
				if (name == "Sebastian" && Game1.netWorldState.Value.hasWorldStateID("sebastianFrog"))
				{
					Point spouseRoomCorner = farmHouse.GetSpouseRoomCorner();
					spouseRoomCorner.X += 2;
					spouseRoomCorner.Y += 5;
					setTilePosition(spouseRoomCorner);
					faceDirection(2);
				}
			}
			else
			{
				setTilePosition(farmHouse.getKitchenStandingSpot());
				faceDirection(0);
				if (random.NextDouble() < 0.2)
				{
					setRandomAfternoonMarriageDialogue(Game1.timeOfDay, farmHouse);
				}
			}
		}

		public virtual void popOffAnyNonEssentialItems()
		{
			if (!Game1.IsMasterGame || base.currentLocation == null)
			{
				return;
			}
			Object objectAtTile = base.currentLocation.getObjectAtTile(getTileX(), getTileY());
			if (objectAtTile != null)
			{
				bool flag = false;
				if (Utility.IsNormalObjectAtParentSheetIndex(objectAtTile, 93) || objectAtTile is Torch)
				{
					flag = true;
				}
				if (flag)
				{
					Vector2 tileLocation = objectAtTile.TileLocation;
					objectAtTile.performRemoveAction(tileLocation, base.currentLocation);
					base.currentLocation.objects.Remove(tileLocation);
					objectAtTile.dropItem(base.currentLocation, tileLocation * 64f, tileLocation * 64f);
				}
			}
		}

		public static bool checkTileOccupancyForSpouse(GameLocation location, Vector2 point, string characterToIgnore = "")
		{
			if (location == null)
			{
				return true;
			}
			isCheckingSpouseTileOccupancy = true;
			bool result = location.isTileOccupied(point, characterToIgnore);
			isCheckingSpouseTileOccupancy = false;
			return result;
		}

		public void addMarriageDialogue(string dialogue_file, string dialogue_key, bool gendered = false, params string[] substitutions)
		{
			shouldSayMarriageDialogue.Value = true;
			currentMarriageDialogue.Add(new MarriageDialogueReference(dialogue_file, dialogue_key, gendered, substitutions));
		}

		public void clearTextAboveHead()
		{
			textAboveHead = null;
			textAboveHeadPreTimer = -1;
			textAboveHeadTimer = -1;
		}

		public bool isVillager()
		{
			if (!IsMonster && !(this is Child) && !(this is Pet) && !(this is Horse) && !(this is Junimo))
			{
				return !(this is JunimoHarvester);
			}
			return false;
		}

		public override bool shouldCollideWithBuildingLayer(GameLocation location)
		{
			if (isMarried() && (Schedule == null || location is FarmHouse))
			{
				return true;
			}
			return base.shouldCollideWithBuildingLayer(location);
		}

		public void arriveAtFarmHouse(FarmHouse farmHouse)
		{
			if (Game1.newDay || !isMarried() || Game1.timeOfDay <= 630 || getTileLocationPoint().Equals(farmHouse.getSpouseBedSpot(name)))
			{
				return;
			}
			setTilePosition(farmHouse.getEntryLocation());
			ignoreScheduleToday = true;
			temporaryController = null;
			controller = null;
			if (Game1.timeOfDay >= 2130)
			{
				Point spouseBedSpot = farmHouse.getSpouseBedSpot(name);
				bool flag = farmHouse.GetSpouseBed() != null;
				PathFindController.endBehavior endBehaviorFunction = null;
				if (flag)
				{
					endBehaviorFunction = FarmHouse.spouseSleepEndFunction;
				}
				controller = new PathFindController(this, farmHouse, spouseBedSpot, 0, endBehaviorFunction);
				if (controller.pathToEndPoint != null && flag)
				{
					foreach (Furniture item in farmHouse.furniture)
					{
						if (item is BedFurniture && item.getBoundingBox(item.TileLocation).Intersects(new Microsoft.Xna.Framework.Rectangle(spouseBedSpot.X * 64, spouseBedSpot.Y * 64, 64, 64)))
						{
							(item as BedFurniture).ReserveForNPC();
							break;
						}
					}
				}
			}
			else
			{
				controller = new PathFindController(this, farmHouse, farmHouse.getKitchenStandingSpot(), 0);
			}
			if (controller.pathToEndPoint == null)
			{
				base.willDestroyObjectsUnderfoot = true;
				controller = new PathFindController(this, farmHouse, farmHouse.getKitchenStandingSpot(), 0);
				setNewDialogue(Game1.LoadStringByGender(gender, "Strings\\StringsFromCSFiles:NPC.cs.4500"));
			}
			else if (Game1.timeOfDay > 1300)
			{
				if (nameOfTodaysSchedule.Equals("marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
				{
					setNewDialogue("MarriageDialogue", "funReturn_", -1, add: false, clearOnMovement: true);
				}
				else if (nameOfTodaysSchedule.Equals("marriageJob"))
				{
					setNewDialogue("MarriageDialogue", "jobReturn_");
				}
				else if (Game1.timeOfDay < 1800)
				{
					setRandomAfternoonMarriageDialogue(Game1.timeOfDay, base.currentLocation, countAsDailyAfternoon: true);
				}
			}
			if (Game1.currentLocation == farmHouse)
			{
				Game1.currentLocation.playSound("doorClose", NetAudio.SoundContext.NPC);
			}
		}

		public Farmer getSpouse()
		{
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer.spouse != null && allFarmer.spouse.Equals(base.Name))
				{
					return allFarmer;
				}
			}
			return null;
		}

		public string getTermOfSpousalEndearment(bool happy = true)
		{
			Farmer spouse = getSpouse();
			if (spouse != null)
			{
				if (isRoommate())
				{
					return spouse.displayName;
				}
				int friendshipHeartLevelForNPC = spouse.getFriendshipHeartLevelForNPC(base.Name);
				if (friendshipHeartLevelForNPC < 9)
				{
					return spouse.displayName;
				}
				if (happy && Game1.random.NextDouble() < 0.08)
				{
					switch (Game1.random.Next(8))
					{
					case 0:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4507");
					case 1:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4508");
					case 2:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4509");
					case 3:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4510");
					case 4:
						if (!spouse.IsMale)
						{
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4512");
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4511");
					case 5:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4513");
					case 6:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4514");
					case 7:
						if (!spouse.IsMale)
						{
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4516");
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4515");
					}
				}
				if (!happy)
				{
					switch (Game1.random.Next(2))
					{
					case 0:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4517");
					case 1:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4518");
					case 2:
						return spouse.displayName;
					}
				}
				switch (Game1.random.Next(5))
				{
				case 0:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4519");
				case 1:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4518");
				case 2:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4517");
				case 3:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4522");
				case 4:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4523");
				}
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4517");
		}

		public bool spouseObstacleCheck(MarriageDialogueReference backToBedMessage, GameLocation currentLocation, bool force = false)
		{
			if (force || checkTileOccupancyForSpouse(currentLocation, getTileLocation(), base.Name))
			{
				Game1.warpCharacter(this, defaultMap, (Game1.getLocationFromName(defaultMap) as FarmHouse).getSpouseBedSpot(name));
				faceDirection(1);
				currentMarriageDialogue.Clear();
				currentMarriageDialogue.Add(backToBedMessage);
				shouldSayMarriageDialogue.Value = true;
				return true;
			}
			return false;
		}

		public void setTilePosition(Point p)
		{
			setTilePosition(p.X, p.Y);
		}

		public void setTilePosition(int x, int y)
		{
			base.Position = new Vector2(x * 64, y * 64);
		}

		private void clintHammerSound(Farmer who)
		{
			base.currentLocation.playSoundAt("hammer", getTileLocation());
		}

		private void robinHammerSound(Farmer who)
		{
			if (Game1.currentLocation.Equals(base.currentLocation) && Utility.isOnScreen(base.Position, 256))
			{
				Game1.playSound((Game1.random.NextDouble() < 0.1) ? "clank" : "axchop");
				shakeTimer = 250;
			}
		}

		private void robinVariablePause(Farmer who)
		{
			if (Game1.random.NextDouble() < 0.4)
			{
				Sprite.CurrentAnimation[Sprite.currentAnimationIndex] = new FarmerSprite.AnimationFrame(27, 300, secondaryArm: false, flip: false, robinVariablePause);
			}
			else if (Game1.random.NextDouble() < 0.25)
			{
				Sprite.CurrentAnimation[Sprite.currentAnimationIndex] = new FarmerSprite.AnimationFrame(23, Game1.random.Next(500, 4000), secondaryArm: false, flip: false, robinVariablePause);
			}
			else
			{
				Sprite.CurrentAnimation[Sprite.currentAnimationIndex] = new FarmerSprite.AnimationFrame(27, Game1.random.Next(1000, 4000), secondaryArm: false, flip: false, robinVariablePause);
			}
		}

		public void ReachedEndPoint()
		{
		}

		public void changeSchedulePathDirection()
		{
			Microsoft.Xna.Framework.Rectangle boundingBox = GetBoundingBox();
			boundingBox.Inflate(2, 2);
			_ = lastCrossroad;
			if (!lastCrossroad.Intersects(boundingBox))
			{
				isCharging = false;
				base.speed = 2;
				directionIndex++;
				lastCrossroad = new Microsoft.Xna.Framework.Rectangle(getStandingX() - getStandingX() % 64, getStandingY() - getStandingY() % 64, 64, 64);
				moveCharacterOnSchedulePath();
			}
		}

		public void moveCharacterOnSchedulePath()
		{
		}

		public void randomSquareMovement(GameTime time)
		{
			Microsoft.Xna.Framework.Rectangle boundingBox = GetBoundingBox();
			boundingBox.Inflate(2, 2);
			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle((int)nextSquarePosition.X * 64, (int)nextSquarePosition.Y * 64, 64, 64);
			_ = nextSquarePosition;
			if (nextSquarePosition.Equals(Vector2.Zero))
			{
				squarePauseAccumulation = 0;
				squarePauseTotal = Game1.random.Next(6000 + squarePauseOffset, 12000 + squarePauseOffset);
				nextSquarePosition = new Vector2(lastCrossroad.X / 64 - lengthOfWalkingSquareX / 2 + Game1.random.Next(lengthOfWalkingSquareX), lastCrossroad.Y / 64 - lengthOfWalkingSquareY / 2 + Game1.random.Next(lengthOfWalkingSquareY));
			}
			else if (rectangle.Contains(boundingBox))
			{
				Halt();
				if (squareMovementFacingPreference != -1)
				{
					faceDirection(squareMovementFacingPreference);
				}
				isCharging = false;
				base.speed = 2;
			}
			else if (boundingBox.Left <= rectangle.Left)
			{
				SetMovingOnlyRight();
			}
			else if (boundingBox.Right >= rectangle.Right)
			{
				SetMovingOnlyLeft();
			}
			else if (boundingBox.Top <= rectangle.Top)
			{
				SetMovingOnlyDown();
			}
			else if (boundingBox.Bottom >= rectangle.Bottom)
			{
				SetMovingOnlyUp();
			}
			squarePauseAccumulation += time.ElapsedGameTime.Milliseconds;
			if (squarePauseAccumulation >= squarePauseTotal && rectangle.Contains(boundingBox))
			{
				nextSquarePosition = Vector2.Zero;
				isCharging = false;
				base.speed = 2;
			}
		}

		public void returnToEndPoint()
		{
			Microsoft.Xna.Framework.Rectangle boundingBox = GetBoundingBox();
			boundingBox.Inflate(2, 2);
			if (boundingBox.Left <= lastCrossroad.Left)
			{
				SetMovingOnlyRight();
			}
			else if (boundingBox.Right >= lastCrossroad.Right)
			{
				SetMovingOnlyLeft();
			}
			else if (boundingBox.Top <= lastCrossroad.Top)
			{
				SetMovingOnlyDown();
			}
			else if (boundingBox.Bottom >= lastCrossroad.Bottom)
			{
				SetMovingOnlyUp();
			}
			boundingBox.Inflate(-2, -2);
			if (lastCrossroad.Contains(boundingBox))
			{
				isWalkingInSquare = false;
				nextSquarePosition = Vector2.Zero;
				returningToEndPoint = false;
				Halt();
			}
		}

		public void SetMovingOnlyUp()
		{
			moveUp = true;
			moveDown = false;
			moveLeft = false;
			moveRight = false;
		}

		public void SetMovingOnlyRight()
		{
			moveUp = false;
			moveDown = false;
			moveLeft = false;
			moveRight = true;
		}

		public void SetMovingOnlyDown()
		{
			moveUp = false;
			moveDown = true;
			moveLeft = false;
			moveRight = false;
		}

		public void SetMovingOnlyLeft()
		{
			moveUp = false;
			moveDown = false;
			moveLeft = true;
			moveRight = false;
		}

		public static void populateRoutesFromLocationToLocationList()
		{
			routesFromLocationToLocation = new List<List<string>>();
			foreach (GameLocation location in Game1.locations)
			{
				if (!(location is Farm) && !location.name.Equals("Backwoods"))
				{
					List<string> route = new List<string>();
					exploreWarpPoints(location, route);
				}
			}
		}

		private static bool exploreWarpPoints(GameLocation l, List<string> route)
		{
			bool result = false;
			if (l != null && !route.Contains(l.name, StringComparer.Ordinal))
			{
				route.Add(l.name);
				if (route.Count == 1 || !doesRoutesListContain(route))
				{
					if (route.Count > 1)
					{
						routesFromLocationToLocation.Add(route.ToList());
						result = true;
					}
					foreach (Warp warp in l.warps)
					{
						string text = warp.TargetName;
						if (text == "BoatTunnel")
						{
							text = "IslandSouth";
						}
						if (!text.Equals("Farm", StringComparison.Ordinal) && !text.Equals("Woods", StringComparison.Ordinal) && !text.Equals("Backwoods", StringComparison.Ordinal) && !text.Equals("Tunnel", StringComparison.Ordinal) && !text.Contains("Volcano"))
						{
							exploreWarpPoints(Game1.getLocationFromName(text), route);
						}
					}
					foreach (Point key in l.doors.Keys)
					{
						string text2 = l.doors[key];
						if (text2 == "BoatTunnel")
						{
							text2 = "IslandSouth";
						}
						exploreWarpPoints(Game1.getLocationFromName(text2), route);
					}
				}
				if (route.Count > 0)
				{
					route.RemoveAt(route.Count - 1);
				}
			}
			return result;
		}

		private static bool doesRoutesListContain(List<string> route)
		{
			foreach (List<string> item in routesFromLocationToLocation)
			{
				if (item.Count != route.Count)
				{
					continue;
				}
				bool flag = true;
				for (int i = 0; i < route.Count; i++)
				{
					if (!item[i].Equals(route[i], StringComparison.Ordinal))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return true;
				}
			}
			return false;
		}

		public int CompareTo(object obj)
		{
			if (obj is NPC)
			{
				return (obj as NPC).id - id;
			}
			return 0;
		}

		public virtual void Removed()
		{
		}
	}
}
