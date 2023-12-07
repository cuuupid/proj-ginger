using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.Tools;
using StardewValley.Util;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley
{
	public class Farmer : Character, IComparable
	{
		public class EmoteType
		{
			public string emoteString = "";

			public int emoteIconIndex = -1;

			public FarmerSprite.AnimationFrame[] animationFrames;

			public bool hidden;

			public int facingDirection = 2;

			public string displayNameKey;

			public string displayName => Game1.content.LoadString(displayNameKey);

			public EmoteType(string emote_string = "", string display_name_key = "", int icon_index = -1, FarmerSprite.AnimationFrame[] frames = null, int facing_direction = 2, bool is_hidden = false)
			{
				emoteString = emote_string;
				emoteIconIndex = icon_index;
				animationFrames = frames;
				facingDirection = facing_direction;
				hidden = is_hidden;
				displayNameKey = "Strings\\UI:" + display_name_key;
			}
		}

		public const int millisecondsPerSpeedUnit = 64;

		public const byte halt = 64;

		public const byte up = 1;

		public const byte right = 2;

		public const byte down = 4;

		public const byte left = 8;

		public const byte run = 16;

		public const byte release = 32;

		public const int farmingSkill = 0;

		public const int miningSkill = 3;

		public const int fishingSkill = 1;

		public const int foragingSkill = 2;

		public const int combatSkill = 4;

		public const int luckSkill = 5;

		public const float interpolationConstant = 0.5f;

		public const int runningSpeed = 5;

		public const int walkingSpeed = 2;

		public const int caveNothing = 0;

		public const int caveBats = 1;

		public const int caveMushrooms = 2;

		public const int millisecondsInvincibleAfterDamage = 1200;

		public const int millisecondsPerFlickerWhenInvincible = 50;

		public const int startingStamina = 270;

		public const int totalLevels = 35;

		public static int tileSlideThreshold = 32;

		public const int maxInventorySpace = 36;

		public const int hotbarSize = 12;

		public const int eyesOpen = 0;

		public const int eyesHalfShut = 4;

		public const int eyesClosed = 1;

		public const int eyesRight = 2;

		public const int eyesLeft = 3;

		public const int eyesWide = 5;

		public const int rancher = 0;

		public const int tiller = 1;

		public const int butcher = 2;

		public const int shepherd = 3;

		public const int artisan = 4;

		public const int agriculturist = 5;

		public const int fisher = 6;

		public const int trapper = 7;

		public const int angler = 8;

		public const int pirate = 9;

		public const int baitmaster = 10;

		public const int mariner = 11;

		public const int forester = 12;

		public const int gatherer = 13;

		public const int lumberjack = 14;

		public const int tapper = 15;

		public const int botanist = 16;

		public const int tracker = 17;

		public const int miner = 18;

		public const int geologist = 19;

		public const int blacksmith = 20;

		public const int burrower = 21;

		public const int excavator = 22;

		public const int gemologist = 23;

		public const int fighter = 24;

		public const int scout = 25;

		public const int brute = 26;

		public const int defender = 27;

		public const int acrobat = 28;

		public const int desperado = 29;

		public readonly NetObjectList<Quest> questLog = new NetObjectList<Quest>();

		public readonly NetIntList professions = new NetIntList();

		public readonly NetList<Point, NetPoint> newLevels = new NetList<Point, NetPoint>();

		private Queue<int> newLevelSparklingTexts = new Queue<int>();

		private SparklingText sparklingText;

		public readonly NetArray<int, NetInt> experiencePoints = new NetArray<int, NetInt>(6);

		public readonly NetObjectList<Item> items = new NetObjectList<Item>();

		public readonly NetIntList dialogueQuestionsAnswered = new NetIntList();

		public List<string> furnitureOwned = new List<string>();

		[XmlElement("cookingRecipes")]
		public readonly NetStringDictionary<int, NetInt> cookingRecipes = new NetStringDictionary<int, NetInt>();

		[XmlElement("craftingRecipes")]
		public readonly NetStringDictionary<int, NetInt> craftingRecipes = new NetStringDictionary<int, NetInt>();

		[XmlElement("activeDialogueEvents")]
		public readonly NetStringDictionary<int, NetInt> activeDialogueEvents = new NetStringDictionary<int, NetInt>();

		public readonly NetIntList eventsSeen = new NetIntList();

		public readonly NetIntList secretNotesSeen = new NetIntList();

		public List<string> songsHeard = new List<string>();

		public readonly NetIntList achievements = new NetIntList();

		public readonly NetIntList specialItems = new NetIntList();

		public readonly NetIntList specialBigCraftables = new NetIntList();

		public readonly NetStringList mailReceived = new NetStringList();

		public readonly NetStringList mailForTomorrow = new NetStringList();

		public readonly NetStringList mailbox = new NetStringList();

		public readonly NetInt timeWentToBed = new NetInt();

		[XmlIgnore]
		public bool hasMoved;

		public readonly NetBool sleptInTemporaryBed = new NetBool();

		[XmlIgnore]
		public readonly NetBool requestingTimePause = new NetBool();

		public Stats stats = new Stats();

		[XmlIgnore]
		public readonly NetCollection<Item> personalShippingBin = new NetCollection<Item>();

		[XmlIgnore]
		public IList<Item> displayedShippedItems = new List<Item>();

		public List<string> blueprints = new List<string>();

		[XmlElement("biteChime")]
		public NetInt biteChime = new NetInt(-1);

		[XmlIgnore]
		public float usernameDisplayTime;

		[XmlIgnore]
		protected NetRef<Item> _recoveredItem = new NetRef<Item>();

		public NetObjectList<Item> itemsLostLastDeath = new NetObjectList<Item>();

		public List<int> movementDirections = new List<int>();

		[XmlElement("farmName")]
		public readonly NetString farmName = new NetString("");

		[XmlElement("favoriteThing")]
		public readonly NetString favoriteThing = new NetString();

		[XmlElement("horseName")]
		public readonly NetString horseName = new NetString();

		public string slotName;

		public bool slotCanHost;

		[XmlIgnore]
		public bool hasReceivedToolUpgradeMessageYet;

		[XmlIgnore]
		private readonly NetArray<int, NetInt> appliedBuffs = new NetArray<int, NetInt>(12);

		[XmlIgnore]
		public readonly NetIntDictionary<int, NetInt> appliedSpecialBuffs = new NetIntDictionary<int, NetInt>();

		[XmlIgnore]
		public IList<OutgoingMessage> messageQueue = new List<OutgoingMessage>();

		[XmlIgnore]
		public readonly NetLong uniqueMultiplayerID = new NetLong(Utility.RandomLong());

		[XmlElement("userID")]
		public readonly NetString userID = new NetString("");

		[XmlIgnore]
		public string previousLocationName = "";

		[XmlIgnore]
		public readonly NetString platformType = new NetString("");

		[XmlIgnore]
		public readonly NetString platformID = new NetString("");

		[XmlIgnore]
		public readonly NetBool hasMenuOpen = new NetBool(value: false);

		[XmlIgnore]
		public readonly Color DEFAULT_SHIRT_COLOR = Color.White;

		[XmlIgnore]
		public readonly Color DEFAULT_PANTS_COLOR = new Color(46, 85, 183);

		public string defaultChatColor;

		public bool catPerson = true;

		public int whichPetBreed;

		[XmlIgnore]
		public bool isAnimatingMount;

		[XmlElement("acceptedDailyQuest")]
		public readonly NetBool acceptedDailyQuest = new NetBool(value: false);

		[XmlIgnore]
		public Item mostRecentlyGrabbedItem;

		[XmlIgnore]
		public Item itemToEat;

		[XmlElement("farmerRenderer")]
		private readonly NetRef<FarmerRenderer> farmerRenderer = new NetRef<FarmerRenderer>();

		[XmlIgnore]
		public int toolPower;

		[XmlIgnore]
		public int toolHold;

		public Vector2 mostRecentBed;

		public static Dictionary<int, string> hairStyleMetadataFile = null;

		public static List<int> allHairStyleIndices = null;

		public static int lastHairStyle = -1;

		[XmlIgnore]
		public static Dictionary<int, HairStyleMetadata> hairStyleMetadata = new Dictionary<int, HairStyleMetadata>();

		[XmlElement("emoteFavorites")]
		public readonly NetStringList emoteFavorites = new NetStringList();

		[XmlElement("performedEmotes")]
		public readonly NetStringDictionary<bool, NetBool> performedEmotes = new NetStringDictionary<bool, NetBool>();

		[XmlElement("shirt")]
		public readonly NetInt shirt = new NetInt(0);

		[XmlElement("hair")]
		public readonly NetInt hair = new NetInt(0);

		[XmlElement("skin")]
		public readonly NetInt skin = new NetInt(0);

		[XmlElement("shoes")]
		public readonly NetInt shoes = new NetInt(2);

		[XmlElement("accessory")]
		public readonly NetInt accessory = new NetInt(-1);

		[XmlElement("facialHair")]
		public readonly NetInt facialHair = new NetInt(-1);

		[XmlElement("pants")]
		public readonly NetInt pants = new NetInt(0);

		[XmlIgnore]
		public int currentEyes;

		[XmlIgnore]
		public int blinkTimer;

		[XmlIgnore]
		public readonly NetInt netFestivalScore = new NetInt();

		[XmlIgnore]
		public float temporarySpeedBuff;

		[XmlElement("hairstyleColor")]
		public readonly NetColor hairstyleColor = new NetColor(new Color(193, 90, 50));

		[XmlElement("pantsColor")]
		public readonly NetColor pantsColor = new NetColor(new Color(46, 85, 183));

		[XmlElement("newEyeColor")]
		public readonly NetColor newEyeColor = new NetColor(new Color(122, 68, 52));

		[XmlElement("hat")]
		public readonly NetRef<Hat> hat = new NetRef<Hat>();

		[XmlElement("boots")]
		public readonly NetRef<Boots> boots = new NetRef<Boots>();

		[XmlElement("leftRing")]
		public readonly NetRef<Ring> leftRing = new NetRef<Ring>();

		[XmlElement("rightRing")]
		public readonly NetRef<Ring> rightRing = new NetRef<Ring>();

		[XmlElement("shirtItem")]
		public readonly NetRef<Clothing> shirtItem = new NetRef<Clothing>();

		[XmlElement("pantsItem")]
		public readonly NetRef<Clothing> pantsItem = new NetRef<Clothing>();

		[XmlIgnore]
		public readonly NetDancePartner dancePartner = new NetDancePartner();

		[XmlIgnore]
		public bool ridingMineElevator;

		[XmlIgnore]
		public bool mineMovementDirectionWasUp;

		[XmlIgnore]
		public bool cameFromDungeon;

		[XmlIgnore]
		public readonly NetBool exhausted = new NetBool();

		[XmlElement("divorceTonight")]
		public readonly NetBool divorceTonight = new NetBool();

		[XmlElement("changeWalletTypeTonight")]
		public readonly NetBool changeWalletTypeTonight = new NetBool();

		[XmlIgnore]
		public AnimatedSprite.endOfAnimationBehavior toolOverrideFunction;

		[XmlIgnore]
		public NetBool onBridge = new NetBool();

		[XmlIgnore]
		public SuspensionBridge bridge;

		private readonly NetInt netDeepestMineLevel = new NetInt();

		[XmlElement("currentToolIndex")]
		private readonly NetInt currentToolIndex = new NetInt(0);

		[XmlIgnore]
		private readonly NetRef<Item> temporaryItem = new NetRef<Item>();

		[XmlIgnore]
		private readonly NetRef<Item> cursorSlotItem = new NetRef<Item>();

		[XmlIgnore]
		public readonly NetBool netItemStowed = new NetBool(value: false);

		protected bool _itemStowed;

		public int woodPieces;

		public int stonePieces;

		public int copperPieces;

		public int ironPieces;

		public int coalPieces;

		public int goldPieces;

		public int iridiumPieces;

		public int quartzPieces;

		public string gameVersion = "-1";

		public string gameVersionLabel;

		[XmlIgnore]
		public bool isFakeEventActor;

		[XmlElement("caveChoice")]
		public readonly NetInt caveChoice = new NetInt();

		public int feed;

		[XmlElement("farmingLevel")]
		public readonly NetInt farmingLevel = new NetInt();

		[XmlElement("miningLevel")]
		public readonly NetInt miningLevel = new NetInt();

		[XmlElement("combatLevel")]
		public readonly NetInt combatLevel = new NetInt();

		[XmlElement("foragingLevel")]
		public readonly NetInt foragingLevel = new NetInt();

		[XmlElement("fishingLevel")]
		public readonly NetInt fishingLevel = new NetInt();

		[XmlElement("luckLevel")]
		public readonly NetInt luckLevel = new NetInt();

		[XmlElement("newSkillPointsToSpend")]
		public readonly NetInt newSkillPointsToSpend = new NetInt();

		[XmlElement("addedFarmingLevel")]
		public readonly NetInt addedFarmingLevel = new NetInt();

		[XmlElement("addedMiningLevel")]
		public readonly NetInt addedMiningLevel = new NetInt();

		[XmlElement("addedCombatLevel")]
		public readonly NetInt addedCombatLevel = new NetInt();

		[XmlElement("addedForagingLevel")]
		public readonly NetInt addedForagingLevel = new NetInt();

		[XmlElement("addedFishingLevel")]
		public readonly NetInt addedFishingLevel = new NetInt();

		[XmlElement("addedLuckLevel")]
		public readonly NetInt addedLuckLevel = new NetInt();

		[XmlElement("maxStamina")]
		public readonly NetInt maxStamina = new NetInt(270);

		[XmlElement("maxItems")]
		public readonly NetInt maxItems = new NetInt(12);

		[XmlElement("lastSeenMovieWeek")]
		public readonly NetInt lastSeenMovieWeek = new NetInt(-1);

		[XmlIgnore]
		public readonly NetString viewingLocation = new NetString(null);

		private readonly NetFloat netStamina = new NetFloat(270f);

		public int resilience;

		public int attack;

		public int immunity;

		public float attackIncreaseModifier;

		public float knockbackModifier;

		public float weaponSpeedModifier;

		public float critChanceModifier;

		public float critPowerModifier;

		public float weaponPrecisionModifier;

		[XmlIgnore]
		public NetRoot<FarmerTeam> teamRoot = new NetRoot<FarmerTeam>(new FarmerTeam());

		public int clubCoins;

		public int trashCanLevel;

		private NetLong netMillisecondsPlayed = new NetLong();

		[XmlElement("toolBeingUpgraded")]
		public readonly NetRef<Tool> toolBeingUpgraded = new NetRef<Tool>();

		[XmlElement("daysLeftForToolUpgrade")]
		public readonly NetInt daysLeftForToolUpgrade = new NetInt();

		[XmlIgnore]
		private float timeOfLastPositionPacket;

		private int numUpdatesSinceLastDraw;

		[XmlElement("houseUpgradeLevel")]
		public readonly NetInt houseUpgradeLevel = new NetInt(0);

		[XmlElement("daysUntilHouseUpgrade")]
		public readonly NetInt daysUntilHouseUpgrade = new NetInt(-1);

		public int coopUpgradeLevel;

		public int barnUpgradeLevel;

		public bool hasGreenhouse;

		public bool hasUnlockedSkullDoor;

		public bool hasDarkTalisman;

		public bool hasMagicInk;

		public bool showChestColorPicker = true;

		public bool hasMagnifyingGlass;

		public bool hasWateringCanEnchantment;

		[XmlIgnore]
		public List<BaseEnchantment> enchantments = new List<BaseEnchantment>();

		protected NetBool hasTownKey = new NetBool(value: false);

		[XmlElement("magneticRadius")]
		public readonly NetInt magneticRadius = new NetInt(128);

		public int temporaryInvincibilityTimer;

		public int currentTemporaryInvincibilityDuration = 1200;

		[XmlIgnore]
		public float rotation;

		private int craftingTime = 1000;

		private int raftPuddleCounter = 250;

		private int raftBobCounter = 1000;

		public int health = 100;

		public int maxHealth = 100;

		private readonly NetInt netTimesReachedMineBottom = new NetInt(0);

		public float difficultyModifier = 1f;

		[XmlIgnore]
		public Vector2 jitter = Vector2.Zero;

		[XmlIgnore]
		public Vector2 lastPosition;

		[XmlIgnore]
		public Vector2 lastGrabTile = Vector2.Zero;

		[XmlIgnore]
		public float jitterStrength;

		[XmlIgnore]
		public float xOffset;

		[XmlElement("isMale")]
		public readonly NetBool isMale = new NetBool(value: true);

		[XmlIgnore]
		public bool canMove = true;

		[XmlIgnore]
		public bool running;

		[XmlIgnore]
		public bool ignoreCollisions;

		[XmlIgnore]
		public readonly NetBool usingTool = new NetBool(value: false);

		[XmlIgnore]
		public bool isEating;

		[XmlIgnore]
		public readonly NetBool isInBed = new NetBool(value: false);

		[XmlIgnore]
		public bool forceTimePass;

		[XmlIgnore]
		public bool isRafting;

		[XmlIgnore]
		public bool usingSlingshot;

		[XmlIgnore]
		public readonly NetBool bathingClothes = new NetBool(value: false);

		[XmlIgnore]
		public bool canOnlyWalk;

		[XmlIgnore]
		public bool temporarilyInvincible;

		public bool hasBusTicket;

		public bool stardewHero;

		public bool hasClubCard;

		public bool hasSpecialCharm;

		[XmlIgnore]
		public bool canReleaseTool;

		[XmlIgnore]
		public bool isCrafting;

		[XmlIgnore]
		public bool isEmoteAnimating;

		[XmlIgnore]
		public bool passedOut;

		[XmlIgnore]
		public bool hasNutPickupQueued;

		[XmlIgnore]
		protected int _emoteGracePeriod;

		[XmlIgnore]
		private BoundingBoxGroup temporaryPassableTiles = new BoundingBoxGroup();

		[XmlIgnore]
		public readonly NetBool hidden = new NetBool();

		[XmlElement("basicShipped")]
		public readonly NetIntDictionary<int, NetInt> basicShipped = new NetIntDictionary<int, NetInt>();

		[XmlElement("mineralsFound")]
		public readonly NetIntDictionary<int, NetInt> mineralsFound = new NetIntDictionary<int, NetInt>();

		[XmlElement("recipesCooked")]
		public readonly NetIntDictionary<int, NetInt> recipesCooked = new NetIntDictionary<int, NetInt>();

		[XmlElement("fishCaught")]
		public readonly NetIntIntArrayDictionary fishCaught = new NetIntIntArrayDictionary();

		[XmlElement("archaeologyFound")]
		public readonly NetIntIntArrayDictionary archaeologyFound = new NetIntIntArrayDictionary();

		[XmlElement("callsReceived")]
		public readonly NetIntDictionary<int, NetInt> callsReceived = new NetIntDictionary<int, NetInt>();

		public SerializableDictionary<string, SerializableDictionary<int, int>> giftedItems;

		[XmlElement("tailoredItems")]
		public readonly NetStringDictionary<int, NetInt> tailoredItems = new NetStringDictionary<int, NetInt>();

		public SerializableDictionary<string, int[]> friendships;

		[XmlElement("friendshipData")]
		public readonly NetStringDictionary<Friendship, NetRef<Friendship>> friendshipData = new NetStringDictionary<Friendship, NetRef<Friendship>>();

		[XmlIgnore]
		public NetString locationBeforeForcedEvent = new NetString(null);

		[XmlIgnore]
		public Vector2 positionBeforeEvent;

		[XmlIgnore]
		public int orientationBeforeEvent;

		[XmlIgnore]
		public int swimTimer;

		[XmlIgnore]
		public int regenTimer;

		[XmlIgnore]
		public int timerSinceLastMovement;

		[XmlIgnore]
		public int noMovementPause;

		[XmlIgnore]
		public int freezePause;

		[XmlIgnore]
		public float yOffset;

		public BuildingUpgrade currentUpgrade;

		[XmlElement("spouse")]
		protected readonly NetString netSpouse = new NetString();

		public string dateStringForSaveGame;

		public int? dayOfMonthForSaveGame;

		public int? seasonForSaveGame;

		public int? yearForSaveGame;

		public int overallsColor;

		public int shirtColor;

		public int skinColor;

		public int hairColor;

		public int eyeColor;

		[XmlIgnore]
		public Vector2 armOffset;

		public string bobber = "";

		private readonly NetRef<Horse> netMount = new NetRef<Horse>();

		[XmlIgnore]
		public ISittable sittingFurniture;

		[XmlIgnore]
		public NetBool isSitting = new NetBool();

		[XmlIgnore]
		public NetVector2 mapChairSitPosition = new NetVector2(new Vector2(-1f, -1f));

		[XmlIgnore]
		public NetBool hasCompletedAllMonsterSlayerQuests = new NetBool(value: false);

		[XmlIgnore]
		public bool isStopSitting;

		[XmlIgnore]
		protected bool _wasSitting;

		[XmlIgnore]
		public Vector2 lerpStartPosition;

		[XmlIgnore]
		public Vector2 lerpEndPosition;

		[XmlIgnore]
		public float lerpPosition = -1f;

		[XmlIgnore]
		public float lerpDuration = -1f;

		[XmlIgnore]
		protected Item _lastSelectedItem;

		[XmlElement("qiGems")]
		public NetIntDelta netQiGems = new NetIntDelta();

		[XmlElement("JOTPKProgress")]
		public NetRef<AbigailGame.JOTPKProgress> jotpkProgress = new NetRef<AbigailGame.JOTPKProgress>();

		[XmlElement("hasUsedDailyRevive")]
		public NetBool hasUsedDailyRevive = new NetBool(value: false);

		private readonly NetEvent0 fireToolEvent = new NetEvent0(interpolate: true);

		private readonly NetEvent0 beginUsingToolEvent = new NetEvent0(interpolate: true);

		private readonly NetEvent0 endUsingToolEvent = new NetEvent0(interpolate: true);

		private readonly NetEvent0 sickAnimationEvent = new NetEvent0();

		private readonly NetEvent0 passOutEvent = new NetEvent0();

		private readonly NetEvent0 haltAnimationEvent = new NetEvent0();

		private readonly NetEvent1Field<Object, NetRef<Object>> drinkAnimationEvent = new NetEvent1Field<Object, NetRef<Object>>();

		private readonly NetEvent1Field<Object, NetRef<Object>> eatAnimationEvent = new NetEvent1Field<Object, NetRef<Object>>();

		private readonly NetEvent1Field<string, NetString> doEmoteEvent = new NetEvent1Field<string, NetString>();

		private readonly NetEvent1Field<long, NetLong> kissFarmerEvent = new NetEvent1Field<long, NetLong>();

		private readonly NetEvent1Field<float, NetFloat> synchronizedJumpEvent = new NetEvent1Field<float, NetFloat>();

		public readonly NetEvent1Field<string, NetString> renovateEvent = new NetEvent1Field<string, NetString>();

		[XmlElement("chestConsumedLevels")]
		public readonly NetIntDictionary<bool, NetBool> chestConsumedMineLevels = new NetIntDictionary<bool, NetBool>();

		public int saveTime;

		[XmlIgnore]
		public float drawLayerDisambiguator;

		[XmlElement("isCustomized")]
		public readonly NetBool isCustomized = new NetBool(value: false);

		[XmlElement("homeLocation")]
		public readonly NetString homeLocation = new NetString("FarmHouse");

		[XmlElement("lastSleepLocation")]
		public readonly NetString lastSleepLocation = new NetString();

		[XmlElement("lastSleepPoint")]
		public readonly NetPoint lastSleepPoint = new NetPoint();

		public static readonly EmoteType[] EMOTES = new EmoteType[22]
		{
			new EmoteType("happy", "Emote_Happy", 32),
			new EmoteType("sad", "Emote_Sad", 28),
			new EmoteType("heart", "Emote_Heart", 20),
			new EmoteType("exclamation", "Emote_Exclamation", 16),
			new EmoteType("note", "Emote_Note", 56),
			new EmoteType("sleep", "Emote_Sleep", 24),
			new EmoteType("game", "Emote_Game", 52),
			new EmoteType("question", "Emote_Question", 8),
			new EmoteType("x", "Emote_X", 36),
			new EmoteType("pause", "Emote_Pause", 40),
			new EmoteType("blush", "Emote_Blush", 60, null, 2, is_hidden: true),
			new EmoteType("angry", "Emote_Angry", 12),
			new EmoteType("yes", "Emote_Yes", 56, new FarmerSprite.AnimationFrame[7]
			{
				new FarmerSprite.AnimationFrame(0, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.currentLocation.localSound("jingle1");
					}
				}),
				new FarmerSprite.AnimationFrame(16, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(0, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(16, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(0, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(16, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(0, 250, secondaryArm: false, flip: false)
			}),
			new EmoteType("no", "Emote_No", 36, new FarmerSprite.AnimationFrame[5]
			{
				new FarmerSprite.AnimationFrame(25, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.currentLocation.localSound("cancel");
					}
				}),
				new FarmerSprite.AnimationFrame(27, 250, secondaryArm: true, flip: false),
				new FarmerSprite.AnimationFrame(25, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(27, 250, secondaryArm: true, flip: false),
				new FarmerSprite.AnimationFrame(25, 250, secondaryArm: false, flip: false)
			}),
			new EmoteType("sick", "Emote_Sick", 12, new FarmerSprite.AnimationFrame[8]
			{
				new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.currentLocation.localSound("croak");
					}
				}),
				new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false)
			}),
			new EmoteType("laugh", "Emote_Laugh", 56, new FarmerSprite.AnimationFrame[8]
			{
				new FarmerSprite.AnimationFrame(102, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.currentLocation.localSound("dustMeep");
					}
				}),
				new FarmerSprite.AnimationFrame(103, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(102, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.currentLocation.localSound("dustMeep");
					}
				}),
				new FarmerSprite.AnimationFrame(103, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(102, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.currentLocation.localSound("dustMeep");
					}
				}),
				new FarmerSprite.AnimationFrame(103, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(102, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.currentLocation.localSound("dustMeep");
					}
				}),
				new FarmerSprite.AnimationFrame(103, 150, secondaryArm: false, flip: false)
			}),
			new EmoteType("surprised", "Emote_Surprised", 16, new FarmerSprite.AnimationFrame[1] { new FarmerSprite.AnimationFrame(94, 1500, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.currentLocation.localSound("batScreech");
				}
				who.jumpWithoutSound(4f);
				who.jitterStrength = 1f;
			}) }),
			new EmoteType("hi", "Emote_Hi", 56, new FarmerSprite.AnimationFrame[4]
			{
				new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.currentLocation.localSound("give_gift");
					}
				}),
				new FarmerSprite.AnimationFrame(85, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(85, 250, secondaryArm: false, flip: false)
			}),
			new EmoteType("taunt", "Emote_Taunt", 12, new FarmerSprite.AnimationFrame[10]
			{
				new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(102, 50, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(10, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.currentLocation.localSound("hitEnemy");
					}
					who.jitterStrength = 1f;
				}).AddFrameEndAction(delegate(Farmer who)
				{
					who.stopJittering();
				}),
				new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(102, 50, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(10, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.currentLocation.localSound("hitEnemy");
					}
					who.jitterStrength = 1f;
				}).AddFrameEndAction(delegate(Farmer who)
				{
					who.stopJittering();
				}),
				new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(102, 50, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(10, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.currentLocation.localSound("hitEnemy");
					}
					who.jitterStrength = 1f;
				}).AddFrameEndAction(delegate(Farmer who)
				{
					who.stopJittering();
				}),
				new FarmerSprite.AnimationFrame(3, 500, secondaryArm: false, flip: false)
			}, 2, is_hidden: true),
			new EmoteType("uh", "Emote_Uh", 40, new FarmerSprite.AnimationFrame[1] { new FarmerSprite.AnimationFrame(10, 1500, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.currentLocation.localSound("clam_tone");
				}
			}) }),
			new EmoteType("music", "Emote_Music", 56, new FarmerSprite.AnimationFrame[9]
			{
				new FarmerSprite.AnimationFrame(98, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					who.playHarpEmoteSound();
				}),
				new FarmerSprite.AnimationFrame(99, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(100, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(98, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(99, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(100, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(98, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(99, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(100, 150, secondaryArm: false, flip: false)
			}, 2, is_hidden: true),
			new EmoteType("jar", "Emote_Jar", -1, new FarmerSprite.AnimationFrame[6]
			{
				new FarmerSprite.AnimationFrame(111, 150, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(111, 300, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.currentLocation.localSound("fishingRodBend");
					}
					who.jitterStrength = 1f;
				}).AddFrameEndAction(delegate(Farmer who)
				{
					who.stopJittering();
				}),
				new FarmerSprite.AnimationFrame(111, 500, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(111, 300, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.currentLocation.localSound("fishingRodBend");
					}
					who.jitterStrength = 1f;
				}).AddFrameEndAction(delegate(Farmer who)
				{
					who.stopJittering();
				}),
				new FarmerSprite.AnimationFrame(111, 500, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(112, 1000, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
				{
					if (who.ShouldHandleAnimationSound())
					{
						who.currentLocation.localSound("coin");
					}
					who.jumpWithoutSound(4f);
				})
			}, 1, is_hidden: true)
		};

		[XmlIgnore]
		public int emoteFacingDirection = 2;

		public int daysMarried;

		private int toolPitchAccumulator;

		private int charactercollisionTimer;

		private NPC collisionNPC;

		public float movementMultiplier = 0.01f;

		public int visibleQuestCount
		{
			get
			{
				int num = 0;
				foreach (SpecialOrder specialOrder in team.specialOrders)
				{
					if (!specialOrder.IsHidden())
					{
						num++;
					}
				}
				foreach (Quest item in questLog)
				{
					if (!item.IsHidden())
					{
						num++;
					}
				}
				return num;
			}
		}

		public Item recoveredItem
		{
			get
			{
				return _recoveredItem.Value;
			}
			set
			{
				_recoveredItem.Value = value;
			}
		}

		[XmlElement("theaterBuildDate")]
		public long theaterBuildDate
		{
			get
			{
				return teamRoot.Value.theaterBuildDate;
			}
			set
			{
				teamRoot.Value.theaterBuildDate.Value = value;
			}
		}

		[XmlIgnore]
		public int festivalScore
		{
			get
			{
				return netFestivalScore;
			}
			set
			{
				if (Game1.player != null && Game1.player.team != null && Game1.player.team.festivalScoreStatus != null)
				{
					Game1.player.team.festivalScoreStatus.UpdateState(Game1.player.festivalScore.ToString() ?? "");
				}
				netFestivalScore.Value = value;
			}
		}

		public int deepestMineLevel
		{
			get
			{
				return netDeepestMineLevel;
			}
			set
			{
				netDeepestMineLevel.Value = value;
			}
		}

		public float stamina
		{
			get
			{
				return netStamina;
			}
			set
			{
				netStamina.Value = value;
			}
		}

		[XmlIgnore]
		public FarmerTeam team
		{
			get
			{
				if (Game1.player != null && this != Game1.player)
				{
					return Game1.player.team;
				}
				return teamRoot.Value;
			}
		}

		public uint totalMoneyEarned
		{
			get
			{
				return (uint)teamRoot.Value.totalMoneyEarned.Value;
			}
			set
			{
				if (teamRoot.Value.totalMoneyEarned.Value != 0)
				{
					if (value >= 15000 && teamRoot.Value.totalMoneyEarned.Value < 15000)
					{
						Game1.multiplayer.globalChatInfoMessage("Earned15k", farmName);
					}
					if (value >= 50000 && teamRoot.Value.totalMoneyEarned.Value < 50000)
					{
						Game1.multiplayer.globalChatInfoMessage("Earned50k", farmName);
					}
					if (value >= 250000 && teamRoot.Value.totalMoneyEarned.Value < 250000)
					{
						Game1.multiplayer.globalChatInfoMessage("Earned250k", farmName);
					}
					if (value >= 1000000 && teamRoot.Value.totalMoneyEarned.Value < 1000000)
					{
						Game1.multiplayer.globalChatInfoMessage("Earned1m", farmName);
					}
					if (value >= 10000000 && teamRoot.Value.totalMoneyEarned.Value < 10000000)
					{
						Game1.multiplayer.globalChatInfoMessage("Earned10m", farmName);
					}
					if (value >= 100000000 && teamRoot.Value.totalMoneyEarned.Value < 100000000)
					{
						Game1.multiplayer.globalChatInfoMessage("Earned100m", farmName);
					}
				}
				teamRoot.Value.totalMoneyEarned.Value = (int)value;
			}
		}

		public ulong millisecondsPlayed
		{
			get
			{
				return (ulong)netMillisecondsPlayed.Value;
			}
			set
			{
				netMillisecondsPlayed.Value = (long)value;
			}
		}

		public bool hasRustyKey
		{
			get
			{
				return teamRoot.Value.hasRustyKey;
			}
			set
			{
				teamRoot.Value.hasRustyKey.Value = value;
			}
		}

		public bool hasSkullKey
		{
			get
			{
				return teamRoot.Value.hasSkullKey;
			}
			set
			{
				teamRoot.Value.hasSkullKey.Value = value;
			}
		}

		public bool canUnderstandDwarves
		{
			get
			{
				return teamRoot.Value.canUnderstandDwarves;
			}
			set
			{
				teamRoot.Value.canUnderstandDwarves.Value = value;
			}
		}

		public bool HasTownKey
		{
			get
			{
				return hasTownKey.Value;
			}
			set
			{
				hasTownKey.Value = value;
			}
		}

		[XmlIgnore]
		public bool hasPendingCompletedQuests
		{
			get
			{
				foreach (SpecialOrder specialOrder in team.specialOrders)
				{
					if (specialOrder.participants.ContainsKey(UniqueMultiplayerID) && specialOrder.ShouldDisplayAsComplete())
					{
						return true;
					}
				}
				foreach (Quest item in questLog)
				{
					if (!item.IsHidden() && item.ShouldDisplayAsComplete() && !item.destroy.Value)
					{
						return true;
					}
				}
				return false;
			}
		}

		[XmlElement("useSeparateWallets")]
		public bool useSeparateWallets
		{
			get
			{
				return teamRoot.Value.useSeparateWallets;
			}
			set
			{
				teamRoot.Value.useSeparateWallets.Value = value;
			}
		}

		public int timesReachedMineBottom
		{
			get
			{
				return netTimesReachedMineBottom;
			}
			set
			{
				netTimesReachedMineBottom.Value = value;
			}
		}

		public string spouse
		{
			get
			{
				if (netSpouse.Value != null && netSpouse.Value.Length != 0)
				{
					return netSpouse.Value;
				}
				return null;
			}
			set
			{
				if (value == null)
				{
					netSpouse.Value = "";
				}
				else
				{
					netSpouse.Value = value;
				}
			}
		}

		[XmlIgnore]
		public bool isUnclaimedFarmhand
		{
			get
			{
				if (!IsMainPlayer)
				{
					return !isCustomized;
				}
				return false;
			}
		}

		[XmlIgnore]
		public Horse mount
		{
			get
			{
				return netMount.Value;
			}
			set
			{
				setMount(value);
			}
		}

		[XmlIgnore]
		public int MaxItems
		{
			get
			{
				return maxItems;
			}
			set
			{
				maxItems.Value = value;
			}
		}

		[XmlIgnore]
		public int Level => ((int)farmingLevel + (int)fishingLevel + (int)foragingLevel + (int)combatLevel + (int)miningLevel + (int)luckLevel) / 2;

		[XmlIgnore]
		public int CraftingTime
		{
			get
			{
				return craftingTime;
			}
			set
			{
				craftingTime = value;
			}
		}

		[XmlIgnore]
		public int NewSkillPointsToSpend
		{
			get
			{
				return newSkillPointsToSpend;
			}
			set
			{
				newSkillPointsToSpend.Value = value;
			}
		}

		[XmlIgnore]
		public int FarmingLevel
		{
			get
			{
				return (int)farmingLevel + (int)addedFarmingLevel;
			}
			set
			{
				farmingLevel.Value = value;
			}
		}

		[XmlIgnore]
		public int MiningLevel
		{
			get
			{
				return (int)miningLevel + (int)addedMiningLevel;
			}
			set
			{
				miningLevel.Value = value;
			}
		}

		[XmlIgnore]
		public int CombatLevel
		{
			get
			{
				return (int)combatLevel + (int)addedCombatLevel;
			}
			set
			{
				combatLevel.Value = value;
			}
		}

		[XmlIgnore]
		public int ForagingLevel
		{
			get
			{
				return (int)foragingLevel + (int)addedForagingLevel;
			}
			set
			{
				foragingLevel.Value = value;
			}
		}

		[XmlIgnore]
		public int FishingLevel
		{
			get
			{
				return (int)fishingLevel + (int)addedFishingLevel + ((CurrentTool != null && CurrentTool.hasEnchantmentOfType<MasterEnchantment>()) ? 1 : 0);
			}
			set
			{
				fishingLevel.Value = value;
			}
		}

		[XmlIgnore]
		public int LuckLevel
		{
			get
			{
				return (int)luckLevel + (int)addedLuckLevel;
			}
			set
			{
				luckLevel.Value = value;
			}
		}

		[XmlIgnore]
		public double DailyLuck => team.sharedDailyLuck.Value + (double)(hasSpecialCharm ? 0.025f : 0f);

		[XmlIgnore]
		public int HouseUpgradeLevel
		{
			get
			{
				return houseUpgradeLevel;
			}
			set
			{
				houseUpgradeLevel.Value = value;
			}
		}

		[XmlIgnore]
		public int CoopUpgradeLevel
		{
			get
			{
				return coopUpgradeLevel;
			}
			set
			{
				coopUpgradeLevel = value;
			}
		}

		[XmlIgnore]
		public int BarnUpgradeLevel
		{
			get
			{
				return barnUpgradeLevel;
			}
			set
			{
				barnUpgradeLevel = value;
			}
		}

		[XmlIgnore]
		public BoundingBoxGroup TemporaryPassableTiles
		{
			get
			{
				return temporaryPassableTiles;
			}
			set
			{
				temporaryPassableTiles = value;
			}
		}

		[XmlIgnore]
		public IList<Item> Items
		{
			get
			{
				return items;
			}
			set
			{
				items.CopyFrom(value);
			}
		}

		[XmlIgnore]
		public int MagneticRadius
		{
			get
			{
				return magneticRadius.Value;
			}
			set
			{
				magneticRadius.Value = value;
			}
		}

		[XmlIgnore]
		public Object ActiveObject
		{
			get
			{
				if (TemporaryItem != null)
				{
					if (TemporaryItem is Object)
					{
						return (Object)TemporaryItem;
					}
					return null;
				}
				if (_itemStowed)
				{
					return null;
				}
				if ((int)currentToolIndex >= 0 && (int)currentToolIndex < items.Count && items[currentToolIndex] != null && items[currentToolIndex] is Object)
				{
					return (Object)items[currentToolIndex];
				}
				return null;
			}
			set
			{
				netItemStowed.Set(newValue: false);
				if (value == null)
				{
					removeItemFromInventory(ActiveObject);
				}
				else
				{
					addItemToInventory(value, CurrentToolIndex);
				}
			}
		}

		[XmlIgnore]
		public bool IsMale
		{
			get
			{
				return isMale;
			}
			set
			{
				isMale.Set(value);
			}
		}

		[XmlIgnore]
		public IList<int> DialogueQuestionsAnswered => dialogueQuestionsAnswered;

		[XmlIgnore]
		public int WoodPieces
		{
			get
			{
				return woodPieces;
			}
			set
			{
				woodPieces = value;
			}
		}

		[XmlIgnore]
		public int StonePieces
		{
			get
			{
				return stonePieces;
			}
			set
			{
				stonePieces = value;
			}
		}

		[XmlIgnore]
		public int CopperPieces
		{
			get
			{
				return copperPieces;
			}
			set
			{
				copperPieces = value;
			}
		}

		[XmlIgnore]
		public int IronPieces
		{
			get
			{
				return ironPieces;
			}
			set
			{
				ironPieces = value;
			}
		}

		[XmlIgnore]
		public int CoalPieces
		{
			get
			{
				return coalPieces;
			}
			set
			{
				coalPieces = value;
			}
		}

		[XmlIgnore]
		public int GoldPieces
		{
			get
			{
				return goldPieces;
			}
			set
			{
				goldPieces = value;
			}
		}

		[XmlIgnore]
		public int IridiumPieces
		{
			get
			{
				return iridiumPieces;
			}
			set
			{
				iridiumPieces = value;
			}
		}

		[XmlIgnore]
		public int QuartzPieces
		{
			get
			{
				return quartzPieces;
			}
			set
			{
				quartzPieces = value;
			}
		}

		[XmlIgnore]
		public int Feed
		{
			get
			{
				return feed;
			}
			set
			{
				feed = value;
			}
		}

		[XmlIgnore]
		public bool CanMove
		{
			get
			{
				return canMove;
			}
			set
			{
				canMove = value;
			}
		}

		[XmlIgnore]
		public bool UsingTool
		{
			get
			{
				return usingTool;
			}
			set
			{
				usingTool.Set(value);
			}
		}

		[XmlIgnore]
		public Tool CurrentTool
		{
			get
			{
				if (CurrentItem != null && CurrentItem is Tool)
				{
					return (Tool)CurrentItem;
				}
				return null;
			}
			set
			{
				while (CurrentToolIndex >= items.Count)
				{
					items.Add(null);
				}
				items[CurrentToolIndex] = value;
			}
		}

		[XmlIgnore]
		public Item TemporaryItem
		{
			get
			{
				return temporaryItem.Value;
			}
			set
			{
				temporaryItem.Value = value;
			}
		}

		public Item CursorSlotItem
		{
			get
			{
				return cursorSlotItem.Value;
			}
			set
			{
				cursorSlotItem.Value = value;
			}
		}

		[XmlIgnore]
		public Item CurrentItem
		{
			get
			{
				if (TemporaryItem != null)
				{
					return TemporaryItem;
				}
				if (_itemStowed)
				{
					return null;
				}
				if ((int)currentToolIndex < 0 || (int)currentToolIndex >= items.Count)
				{
					return null;
				}
				return items[currentToolIndex];
			}
		}

		[XmlIgnore]
		public int CurrentToolIndex
		{
			get
			{
				return currentToolIndex;
			}
			set
			{
				netItemStowed.Set(newValue: false);
				if ((int)currentToolIndex >= 0 && CurrentItem != null && value != (int)currentToolIndex)
				{
					CurrentItem.actionWhenStopBeingHeld(this);
				}
				currentToolIndex.Set(value);
			}
		}

		[XmlIgnore]
		public float Stamina
		{
			get
			{
				return stamina;
			}
			set
			{
				stamina = Math.Min((int)maxStamina, Math.Max(value, -16f));
			}
		}

		[XmlIgnore]
		public int MaxStamina
		{
			get
			{
				return maxStamina;
			}
			set
			{
				maxStamina.Value = value;
			}
		}

		public long UniqueMultiplayerID
		{
			get
			{
				return uniqueMultiplayerID;
			}
			set
			{
				uniqueMultiplayerID.Value = value;
			}
		}

		[XmlIgnore]
		public bool IsLocalPlayer
		{
			get
			{
				if (UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
				{
					if (Game1.CurrentEvent != null)
					{
						return Game1.CurrentEvent.farmer == this;
					}
					return false;
				}
				return true;
			}
		}

		[XmlIgnore]
		public bool IsMainPlayer
		{
			get
			{
				if (!(Game1.serverHost == null) || !IsLocalPlayer)
				{
					if (Game1.serverHost != null)
					{
						return UniqueMultiplayerID == Game1.serverHost.Value.UniqueMultiplayerID;
					}
					return false;
				}
				return true;
			}
		}

		[XmlIgnore]
		public override AnimatedSprite Sprite
		{
			get
			{
				return base.Sprite;
			}
			set
			{
				base.Sprite = value;
				if (base.Sprite != null)
				{
					(base.Sprite as FarmerSprite).setOwner(this);
				}
			}
		}

		[XmlIgnore]
		public FarmerSprite FarmerSprite
		{
			get
			{
				return (FarmerSprite)Sprite;
			}
			set
			{
				Sprite = value;
			}
		}

		[XmlIgnore]
		public FarmerRenderer FarmerRenderer
		{
			get
			{
				return farmerRenderer;
			}
			set
			{
				farmerRenderer.Set(value);
			}
		}

		[XmlElement("money")]
		public int _money
		{
			get
			{
				return teamRoot.Value.GetMoney(this);
			}
			set
			{
				teamRoot.Value.GetMoney(this).Value = value;
			}
		}

		[XmlIgnore]
		public int QiGems
		{
			get
			{
				return netQiGems.Value;
			}
			set
			{
				netQiGems.Value = value;
			}
		}

		[XmlIgnore]
		public int Money
		{
			get
			{
				return _money;
			}
			set
			{
				if (Game1.player != this)
				{
					throw new Exception("Cannot change another farmer's money. Use Game1.player.team.SetIndividualMoney");
				}
				int money = _money;
				_money = value;
				if (value > money)
				{
					uint num = (uint)(value - money);
					totalMoneyEarned += num;
					if (Game1.player.useSeparateWallets)
					{
						stats.IndividualMoneyEarned += num;
					}
					Game1.stats.checkForMoneyAchievements();
				}
			}
		}

		public override int FacingDirection
		{
			get
			{
				if (isEmoteAnimating)
				{
					return emoteFacingDirection;
				}
				return facingDirection;
			}
			set
			{
				facingDirection.Set(value);
			}
		}

		public void addUnearnedMoney(int money)
		{
			_money += money;
		}

		public NetStringList GetEmoteFavorites()
		{
			if (emoteFavorites.Count == 0)
			{
				emoteFavorites.Add("question");
				emoteFavorites.Add("heart");
				emoteFavorites.Add("yes");
				emoteFavorites.Add("happy");
				emoteFavorites.Add("pause");
				emoteFavorites.Add("sad");
				emoteFavorites.Add("no");
				emoteFavorites.Add("angry");
			}
			return emoteFavorites;
		}

		public Farmer()
		{
			farmerInit();
			Sprite = new FarmerSprite(null);
		}

		public Farmer(FarmerSprite sprite, Vector2 position, int speed, string name, List<Item> initialTools, bool isMale)
			: base(sprite, position, speed, name)
		{
			farmerInit();
			base.Name = name;
			displayName = name;
			IsMale = isMale;
			stamina = (int)maxStamina;
			items.CopyFrom(initialTools);
			for (int i = items.Count; i < (int)maxItems; i++)
			{
				items.Add(null);
			}
			activeDialogueEvents.Add("Introduction", 6);
			if (base.currentLocation != null)
			{
				mostRecentBed = Utility.PointToVector2((base.currentLocation as FarmHouse).GetPlayerBedSpot()) * 64f;
			}
			else
			{
				mostRecentBed = new Vector2(9f, 9f) * 64f;
			}
		}

		private void farmerInit()
		{
			base.NetFields.AddFields(uniqueMultiplayerID, userID, platformType, platformID, farmerRenderer, isMale, bathingClothes, shirt, pants, hair, skin, shoes, accessory, facialHair, hairstyleColor, pantsColor, newEyeColor, items, currentToolIndex, temporaryItem, cursorSlotItem, fireToolEvent, beginUsingToolEvent, endUsingToolEvent, hat, boots, leftRing, rightRing, hidden, isInBed, caveChoice, houseUpgradeLevel, daysUntilHouseUpgrade, magneticRadius, netSpouse, mailReceived, mailForTomorrow, mailbox, eventsSeen, secretNotesSeen, netMount.NetFields, dancePartner.NetFields, divorceTonight, isCustomized, homeLocation, farmName, favoriteThing, horseName, netMillisecondsPlayed, netFestivalScore, friendshipData, drinkAnimationEvent, eatAnimationEvent, sickAnimationEvent, passOutEvent, doEmoteEvent, questLog, professions, newLevels, experiencePoints, dialogueQuestionsAnswered, cookingRecipes, craftingRecipes, activeDialogueEvents, achievements, specialItems, specialBigCraftables, farmingLevel, miningLevel, combatLevel, foragingLevel, fishingLevel, luckLevel, newSkillPointsToSpend, addedFarmingLevel, addedMiningLevel, addedCombatLevel, addedForagingLevel, addedFishingLevel, addedLuckLevel, maxStamina, netStamina, maxItems, chestConsumedMineLevels, toolBeingUpgraded, daysLeftForToolUpgrade, exhausted, appliedBuffs, netDeepestMineLevel, netTimesReachedMineBottom, netItemStowed, acceptedDailyQuest, lastSeenMovieWeek, shirtItem, pantsItem, personalShippingBin, viewingLocation, kissFarmerEvent, haltAnimationEvent, synchronizedJumpEvent, tailoredItems, basicShipped, mineralsFound, recipesCooked, archaeologyFound, fishCaught, _recoveredItem, itemsLostLastDeath, renovateEvent, callsReceived, onBridge, lastSleepLocation, lastSleepPoint, sleptInTemporaryBed, timeWentToBed, hasUsedDailyRevive, jotpkProgress, requestingTimePause, isSitting, mapChairSitPosition, netQiGems, locationBeforeForcedEvent, appliedSpecialBuffs, hasTownKey, hasCompletedAllMonsterSlayerQuests);
			requestingTimePause.InterpolationWait = false;
			if (Sprite != null)
			{
				FarmerSprite.setOwner(this);
			}
			netQiGems.Minimum = 0;
			netMillisecondsPlayed.DeltaAggregateTicks = 60;
			fireToolEvent.onEvent += performFireTool;
			beginUsingToolEvent.onEvent += performBeginUsingTool;
			endUsingToolEvent.onEvent += performEndUsingTool;
			drinkAnimationEvent.onEvent += performDrinkAnimation;
			eatAnimationEvent.onEvent += performEatAnimation;
			sickAnimationEvent.onEvent += performSickAnimation;
			passOutEvent.onEvent += performPassOut;
			doEmoteEvent.onEvent += performPlayerEmote;
			kissFarmerEvent.onEvent += performKissFarmer;
			haltAnimationEvent.onEvent += performHaltAnimation;
			synchronizedJumpEvent.onEvent += performSynchronizedJump;
			renovateEvent.onEvent += performRenovation;
			FarmerRenderer = new FarmerRenderer("Characters\\Farmer\\farmer_" + (IsMale ? "" : "girl_") + "base", this);
			base.currentLocation = Game1.getLocationFromName(homeLocation);
			items.Clear();
			giftedItems = new SerializableDictionary<string, SerializableDictionary<int, int>>();
			craftingRecipes.Add("Chest", 0);
			craftingRecipes.Add("Wood Fence", 0);
			craftingRecipes.Add("Gate", 0);
			craftingRecipes.Add("Torch", 0);
			craftingRecipes.Add("Campfire", 0);
			craftingRecipes.Add("Wood Path", 0);
			craftingRecipes.Add("Cobblestone Path", 0);
			craftingRecipes.Add("Gravel Path", 0);
			craftingRecipes.Add("Wood Sign", 0);
			craftingRecipes.Add("Stone Sign", 0);
			cookingRecipes.Add("Fried Egg", 0);
			songsHeard.Add("title_day");
			songsHeard.Add("title_night");
			changeShirt(0);
			changeSkinColor(0);
			changeShoeColor(2);
			shirtItem.fieldChangeVisibleEvent += delegate
			{
				UpdateClothing();
			};
			pantsItem.fieldChangeVisibleEvent += delegate
			{
				UpdateClothing();
			};
			farmName.FilterStringEvent += Utility.FilterDirtyWords;
			name.FilterStringEvent += Utility.FilterDirtyWords;
		}

		public bool CanEmote()
		{
			if (Game1.farmEvent != null)
			{
				return false;
			}
			if (Game1.eventUp && Game1.CurrentEvent != null && !Game1.CurrentEvent.playerControlSequence && IsLocalPlayer)
			{
				return false;
			}
			if (usingSlingshot)
			{
				return false;
			}
			if (isEating)
			{
				return false;
			}
			if (UsingTool)
			{
				return false;
			}
			if (!CanMove && IsLocalPlayer)
			{
				return false;
			}
			if (IsSitting())
			{
				return false;
			}
			if (isRidingHorse())
			{
				return false;
			}
			if (bathingClothes.Value)
			{
				return false;
			}
			return true;
		}

		public void performRenovation(string location_name)
		{
			GameLocation locationFromName = Game1.getLocationFromName(location_name);
			if (locationFromName != null && locationFromName is FarmHouse farmHouse)
			{
				farmHouse.UpdateForRenovation();
			}
		}

		public void performPlayerEmote(string emote_string)
		{
			for (int i = 0; i < EMOTES.Length; i++)
			{
				EmoteType emoteType = EMOTES[i];
				if (!(emoteType.emoteString == emote_string))
				{
					continue;
				}
				performedEmotes[emote_string] = true;
				if (emoteType.animationFrames != null)
				{
					if (!CanEmote())
					{
						break;
					}
					if (isEmoteAnimating)
					{
						EndEmoteAnimation();
					}
					else if (FarmerSprite.PauseForSingleAnimation)
					{
						break;
					}
					isEmoteAnimating = true;
					_emoteGracePeriod = 200;
					if (this == Game1.player)
					{
						Game1.player.noMovementPause = Math.Max(Game1.player.noMovementPause, 200);
					}
					emoteFacingDirection = emoteType.facingDirection;
					FarmerSprite.animateOnce(emoteType.animationFrames, OnEmoteAnimationEnd);
				}
				if (emoteType.emoteIconIndex >= 0)
				{
					isEmoting = false;
					doEmote(emoteType.emoteIconIndex, nextEventCommand: false);
				}
			}
		}

		public bool ShouldHandleAnimationSound()
		{
			if (!LocalMultiplayer.IsLocalMultiplayer(is_local_only: true))
			{
				return true;
			}
			if (IsLocalPlayer)
			{
				return true;
			}
			return false;
		}

		public static List<Item> initialTools()
		{
			List<Item> list = new List<Item>();
			list.Add(new Axe());
			list.Add(new Hoe());
			list.Add(new WateringCan());
			list.Add(new Pickaxe());
			list.Add(new MeleeWeapon(47));
			return list;
		}

		private void playHarpEmoteSound()
		{
			int[] array = new int[4] { 1200, 1600, 1900, 2400 };
			switch (Game1.random.Next(5))
			{
			case 0:
				array = new int[4] { 1200, 1600, 1900, 2400 };
				break;
			case 1:
				array = new int[4] { 1200, 1700, 2100, 2400 };
				break;
			case 2:
				array = new int[4] { 1100, 1400, 1900, 2300 };
				break;
			case 3:
				array = new int[3] { 1600, 1900, 2400 };
				break;
			case 4:
				array = new int[3] { 700, 1200, 1900 };
				break;
			}
			if (!IsLocalPlayer)
			{
				return;
			}
			if (Game1.IsMultiplayer && (long)uniqueMultiplayerID % 111 == 0L)
			{
				array = new int[4]
				{
					800 + Game1.random.Next(4) * 100,
					1200 + Game1.random.Next(4) * 100,
					1600 + Game1.random.Next(4) * 100,
					2000 + Game1.random.Next(4) * 100
				};
				for (int i = 0; i < array.Length; i++)
				{
					DelayedAction.playSoundAfterDelay("miniharp_note", Game1.random.Next(60, 150) * i, base.currentLocation, array[i]);
					if (i > 1 && Game1.random.NextDouble() < 0.25)
					{
						break;
					}
				}
			}
			else
			{
				for (int j = 0; j < array.Length; j++)
				{
					DelayedAction.playSoundAfterDelay("miniharp_note", (j > 0) ? (150 + Game1.random.Next(35, 51) * j) : 0, base.currentLocation, array[j]);
				}
			}
		}

		private static void removeLowestUpgradeLevelTool(List<Item> items, Type toolType)
		{
			Tool tool = null;
			foreach (Item item in items)
			{
				if (item is Tool && item.GetType() == toolType && (tool == null || (int)(item as Tool).upgradeLevel < (int)tool.upgradeLevel))
				{
					tool = item as Tool;
				}
			}
			if (tool != null)
			{
				items.Remove(tool);
			}
		}

		public static void removeInitialTools(List<Item> items)
		{
			removeLowestUpgradeLevelTool(items, typeof(Axe));
			removeLowestUpgradeLevelTool(items, typeof(Hoe));
			removeLowestUpgradeLevelTool(items, typeof(WateringCan));
			removeLowestUpgradeLevelTool(items, typeof(Pickaxe));
			Item item2 = items.FirstOrDefault((Item item) => item is MeleeWeapon && (item as Tool).InitialParentTileIndex == 47);
			if (item2 != null)
			{
				items.Remove(item2);
			}
		}

		public Point getMailboxPosition()
		{
			foreach (Building building in Game1.getFarm().buildings)
			{
				if (building.isCabin && building.nameOfIndoors == homeLocation)
				{
					return building.getMailboxPosition();
				}
			}
			return Game1.getFarm().GetMainMailboxPosition();
		}

		public void ClearBuffs()
		{
			Game1.buffsDisplay.clearAllBuffs();
			stopGlowing();
			addedCombatLevel.Value = 0;
			addedFarmingLevel.Value = 0;
			addedFishingLevel.Value = 0;
			addedForagingLevel.Value = 0;
			addedLuckLevel.Value = 0;
			addedMiningLevel.Value = 0;
			base.addedSpeed = 0;
			attack = 0;
			Game1.player.appliedSpecialBuffs.Clear();
		}

		public void addBuffAttributes(int[] buffAttributes)
		{
			for (int i = 0; i < appliedBuffs.Length; i++)
			{
				appliedBuffs[i] += buffAttributes[i];
			}
			addedFarmingLevel.Value += buffAttributes[0];
			addedFishingLevel.Value += buffAttributes[1];
			addedMiningLevel.Value += buffAttributes[2];
			addedLuckLevel.Value += buffAttributes[4];
			addedForagingLevel.Value += buffAttributes[5];
			CraftingTime -= buffAttributes[6];
			MaxStamina += buffAttributes[7];
			MagneticRadius += buffAttributes[8];
			resilience += buffAttributes[10];
			attack += buffAttributes[11];
			base.addedSpeed += buffAttributes[9];
		}

		public void removeBuffAttributes(int[] buffAttributes)
		{
			for (int i = 0; i < appliedBuffs.Length; i++)
			{
				appliedBuffs[i] -= buffAttributes[i];
			}
			if (buffAttributes[0] != 0)
			{
				addedFarmingLevel.Value = Math.Max(0, (int)addedFarmingLevel - buffAttributes[0]);
			}
			if (buffAttributes[1] != 0)
			{
				addedFishingLevel.Value = Math.Max(0, (int)addedFishingLevel - buffAttributes[1]);
			}
			if (buffAttributes[2] != 0)
			{
				addedMiningLevel.Value = Math.Max(0, (int)addedMiningLevel - buffAttributes[2]);
			}
			if (buffAttributes[4] != 0)
			{
				addedLuckLevel.Value = Math.Max(0, (int)addedLuckLevel - buffAttributes[4]);
			}
			if (buffAttributes[5] != 0)
			{
				addedForagingLevel.Value = Math.Max(0, (int)addedForagingLevel - buffAttributes[5]);
			}
			if (buffAttributes[6] != 0)
			{
				CraftingTime = Math.Max(0, CraftingTime - buffAttributes[6]);
			}
			if (buffAttributes[7] != 0)
			{
				MaxStamina = Math.Max(0, MaxStamina - buffAttributes[7]);
				stamina = Math.Min(stamina, MaxStamina);
			}
			if (buffAttributes[8] != 0)
			{
				MagneticRadius = Math.Max(0, MagneticRadius - buffAttributes[8]);
			}
			if (buffAttributes[10] != 0)
			{
				resilience = Math.Max(0, resilience - buffAttributes[10]);
			}
			if (buffAttributes[9] != 0)
			{
				if (buffAttributes[9] < 0)
				{
					base.addedSpeed += Math.Abs(buffAttributes[9]);
				}
				else
				{
					base.addedSpeed -= buffAttributes[9];
				}
			}
			if (buffAttributes[11] != 0)
			{
				if (buffAttributes[11] < 0)
				{
					attack += Math.Abs(buffAttributes[11]);
				}
				else
				{
					attack -= buffAttributes[11];
				}
			}
		}

		public void removeBuffAttributes()
		{
			removeBuffAttributes(appliedBuffs.ToArray());
		}

		public bool isActive()
		{
			if (this != Game1.player)
			{
				return Game1.otherFarmers.ContainsKey(UniqueMultiplayerID);
			}
			return true;
		}

		public string getTexture()
		{
			return "Characters\\Farmer\\farmer_" + (IsMale ? "" : "girl_") + "base" + (isBald() ? "_bald" : "");
		}

		public void checkForLevelTenStatus()
		{
		}

		public void unload()
		{
			if (FarmerRenderer != null)
			{
				FarmerRenderer.unload();
			}
		}

		public void setInventory(List<Item> newInventory)
		{
			items.CopyFrom(newInventory);
			for (int i = items.Count; i < (int)maxItems; i++)
			{
				items.Add(null);
			}
		}

		public void makeThisTheActiveObject(Object o)
		{
			if (freeSpotsInInventory() > 0)
			{
				Item currentItem = CurrentItem;
				ActiveObject = o;
				addItemToInventory(currentItem);
			}
		}

		public int getNumberOfChildren()
		{
			return getChildrenCount();
		}

		private void setMount(Horse mount)
		{
			if (mount != null)
			{
				netMount.Value = mount;
				xOffset = -11f;
				base.Position = Utility.PointToVector2(mount.GetBoundingBox().Location);
				position.Y -= 16f;
				position.X -= 8f;
				base.speed = 2;
				showNotCarrying();
				return;
			}
			netMount.Value = null;
			collisionNPC = null;
			running = false;
			base.speed = ((Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.runButton) && !Game1.options.autoRun) ? 5 : 2);
			bool flag = (running = base.speed == 5);
			if (running)
			{
				base.speed = 5;
			}
			else
			{
				base.speed = 2;
			}
			completelyStopAnimatingOrDoingAction();
			xOffset = 0f;
		}

		public bool isRidingHorse()
		{
			if (mount != null)
			{
				return !Game1.eventUp;
			}
			return false;
		}

		public List<Child> getChildren()
		{
			return Utility.getHomeOfFarmer(this).getChildren();
		}

		public int getChildrenCount()
		{
			return Utility.getHomeOfFarmer(this).getChildrenCount();
		}

		public Tool getToolFromName(string name)
		{
			foreach (Item item in items)
			{
				if (item != null && item is Tool && item.Name.Contains(name))
				{
					return (Tool)item;
				}
			}
			return null;
		}

		public override void SetMovingDown(bool b)
		{
			setMoving((byte)(4 + ((!b) ? 32 : 0)));
		}

		public override void SetMovingRight(bool b)
		{
			setMoving((byte)(2 + ((!b) ? 32 : 0)));
		}

		public override void SetMovingUp(bool b)
		{
			setMoving((byte)(1 + ((!b) ? 32 : 0)));
		}

		public override void SetMovingLeft(bool b)
		{
			setMoving((byte)(8 + ((!b) ? 32 : 0)));
		}

		public int? tryGetFriendshipLevelForNPC(string name)
		{
			if (friendshipData.TryGetValue(name, out var value))
			{
				return value.Points;
			}
			return null;
		}

		public int getFriendshipLevelForNPC(string name)
		{
			if (friendshipData.TryGetValue(name, out var value))
			{
				return value.Points;
			}
			return 0;
		}

		public int getFriendshipHeartLevelForNPC(string name)
		{
			return getFriendshipLevelForNPC(name) / 250;
		}

		public bool isRoommate(string name)
		{
			if (name == null)
			{
				return false;
			}
			if (friendshipData.TryGetValue(name, out var value))
			{
				return value.IsRoommate();
			}
			return false;
		}

		public bool hasCurrentOrPendingRoommate()
		{
			if (spouse == null)
			{
				return false;
			}
			if (friendshipData.TryGetValue(spouse, out var value))
			{
				return value.RoommateMarriage;
			}
			return false;
		}

		public bool hasRoommate()
		{
			if (spouse != null && isRoommate(spouse))
			{
				return true;
			}
			return false;
		}

		public bool hasAFriendWithHeartLevel(int heartLevel, bool datablesOnly)
		{
			foreach (NPC allCharacter in Utility.getAllCharacters())
			{
				if ((!datablesOnly || (bool)allCharacter.datable) && getFriendshipHeartLevelForNPC(allCharacter.Name) >= heartLevel)
				{
					return true;
				}
			}
			return false;
		}

		public int getTallyOfObject(int index, bool bigCraftable)
		{
			int num = 0;
			foreach (Item item in items)
			{
				if (item is Object && (item as Object).ParentSheetIndex == index && (bool)(item as Object).bigCraftable == bigCraftable)
				{
					num += item.Stack;
				}
			}
			return num;
		}

		public bool areAllItemsNull()
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] != null)
				{
					return false;
				}
			}
			return true;
		}

		public void shippedBasic(int index, int number)
		{
			if (basicShipped.ContainsKey(index))
			{
				basicShipped[index] += number;
			}
			else
			{
				basicShipped.Add(index, number);
			}
		}

		public void shiftToolbar(bool right)
		{
			if (!Game1.player.UsingTool)
			{
				Game1.playSound("shwip");
			}
			Game1.toolbar.ScrollToolbarOne(right);
		}

		public void foundWalnut(int stack = 1)
		{
			Game1.netWorldState.Value.GoldenWalnuts.Value += stack;
			Game1.netWorldState.Value.GoldenWalnutsFound.Value += stack;
			if (!IsBusyDoingSomething())
			{
				showNutPickup();
			}
			else
			{
				hasNutPickupQueued = true;
			}
		}

		public virtual void RemoveMail(string mail_key, bool from_broadcast_list = false)
		{
			mail_key = mail_key.Replace("%&NL&%", "");
			mailReceived.Remove(mail_key);
			mailbox.Remove(mail_key);
			mailForTomorrow.Remove(mail_key);
			mailForTomorrow.Remove(mail_key + "%&NL&%");
			if (!from_broadcast_list)
			{
				return;
			}
			for (int i = 0; i < team.broadcastedMail.Count; i++)
			{
				string text = team.broadcastedMail[i];
				if (text == "%&SM&%" + mail_key || text == "%&MFT&%" + mail_key || text == "%&MB&%" + mail_key)
				{
					team.broadcastedMail.RemoveAt(i);
					i--;
				}
			}
		}

		public virtual void showNutPickup()
		{
			if (!hasOrWillReceiveMail("lostWalnutFound") && !Game1.eventUp)
			{
				Game1.addMailForTomorrow("lostWalnutFound", noLetter: true);
				holdUpItemThenMessage(new Object(73, 1));
			}
			else if (hasOrWillReceiveMail("lostWalnutFound") && !Game1.eventUp)
			{
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(0, 240, 16, 16), 100f, 4, 2, new Vector2(0f, -96f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -6f),
					acceleration = new Vector2(0f, 0.2f),
					stopAcceleratingWhenVelocityIsZero = true,
					attachedCharacter = this,
					positionFollowsAttachedCharacter = true
				});
			}
		}

		public void foundArtifact(int index, int number)
		{
			bool flag = false;
			if (index == 102)
			{
				if (!hasOrWillReceiveMail("lostBookFound"))
				{
					Game1.addMailForTomorrow("lostBookFound", noLetter: true);
					flag = true;
				}
				else
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14100"));
				}
				Game1.playSound("newRecipe");
				Game1.netWorldState.Value.LostBooksFound.Value++;
				Game1.multiplayer.globalChatInfoMessage("LostBook", displayName);
			}
			if (archaeologyFound.ContainsKey(index))
			{
				int[] array = archaeologyFound[index];
				array[0] += number;
				array[1] += number;
				archaeologyFound[index] = array;
			}
			else
			{
				if (archaeologyFound.Count() == 0)
				{
					if (!eventsSeen.Contains(0) && index != 102)
					{
						addQuest(23);
					}
					mailReceived.Add("artifactFound");
					flag = true;
				}
				archaeologyFound.Add(index, new int[2] { number, number });
			}
			if (flag)
			{
				holdUpItemThenMessage(new Object(index, 1));
			}
		}

		public void cookedRecipe(int index)
		{
			if (recipesCooked.ContainsKey(index))
			{
				recipesCooked[index]++;
			}
			else
			{
				recipesCooked.Add(index, 1);
			}
		}

		public bool caughtFish(int index, int size, bool from_fish_pond = false, int numberCaught = 1)
		{
			if (index >= 167 && index < 173)
			{
				return false;
			}
			bool result = false;
			if (!from_fish_pond)
			{
				if (fishCaught.ContainsKey(index))
				{
					int[] array = fishCaught[index];
					array[0] += numberCaught;
					Game1.stats.checkForFishingAchievements();
					if (size > fishCaught[index][1])
					{
						array[1] = size;
						result = true;
					}
					fishCaught[index] = array;
				}
				else
				{
					fishCaught.Add(index, new int[2] { numberCaught, size });
					Game1.stats.checkForFishingAchievements();
				}
				checkForQuestComplete(null, index, numberCaught, null, null, 7);
			}
			return result;
		}

		public void gainExperience(int which, int howMuch)
		{
			if (which == 5 || howMuch <= 0)
			{
				return;
			}
			if (!IsLocalPlayer)
			{
				queueMessage(17, Game1.player, which, howMuch);
				return;
			}
			int num = checkForLevelGain(experiencePoints[which], experiencePoints[which] + howMuch);
			experiencePoints[which] += howMuch;
			int num2 = -1;
			if (num != -1)
			{
				switch (which)
				{
				case 0:
					num2 = farmingLevel;
					farmingLevel.Value = num;
					break;
				case 3:
					num2 = miningLevel;
					miningLevel.Value = num;
					break;
				case 1:
					num2 = fishingLevel;
					fishingLevel.Value = num;
					break;
				case 2:
					num2 = foragingLevel;
					foragingLevel.Value = num;
					break;
				case 5:
					num2 = luckLevel;
					luckLevel.Value = num;
					break;
				case 4:
					num2 = combatLevel;
					combatLevel.Value = num;
					break;
				}
			}
			if (num > num2)
			{
				for (int i = num2 + 1; i <= num; i++)
				{
					newLevels.Add(new Point(which, i));
					_ = newLevels.Count;
					_ = 1;
				}
			}
		}

		public int getEffectiveSkillLevel(int whichSkill)
		{
			if (whichSkill < 0 || whichSkill > 5)
			{
				return -1;
			}
			int[] array = new int[6] { farmingLevel, fishingLevel, foragingLevel, miningLevel, combatLevel, luckLevel };
			for (int i = 0; i < newLevels.Count; i++)
			{
				array[newLevels[i].X]--;
			}
			return array[whichSkill];
		}

		public static int checkForLevelGain(int oldXP, int newXP)
		{
			int result = -1;
			if (oldXP < 100 && newXP >= 100)
			{
				result = 1;
			}
			if (oldXP < 380 && newXP >= 380)
			{
				result = 2;
			}
			if (oldXP < 770 && newXP >= 770)
			{
				result = 3;
			}
			if (oldXP < 1300 && newXP >= 1300)
			{
				result = 4;
			}
			if (oldXP < 2150 && newXP >= 2150)
			{
				result = 5;
			}
			if (oldXP < 3300 && newXP >= 3300)
			{
				result = 6;
			}
			if (oldXP < 4800 && newXP >= 4800)
			{
				result = 7;
			}
			if (oldXP < 6900 && newXP >= 6900)
			{
				result = 8;
			}
			if (oldXP < 10000 && newXP >= 10000)
			{
				result = 9;
			}
			if (oldXP < 15000 && newXP >= 15000)
			{
				result = 10;
			}
			return result;
		}

		public void revealGiftTaste(NPC npc, int parent_sheet_index)
		{
			if (!giftedItems.ContainsKey(npc.name))
			{
				giftedItems[npc.name] = new SerializableDictionary<int, int>();
			}
			if (!giftedItems[npc.name].ContainsKey(parent_sheet_index))
			{
				giftedItems[npc.name][parent_sheet_index] = 0;
			}
		}

		public void revealGiftTaste(NPC npc, Object item)
		{
			if (!item.bigCraftable)
			{
				revealGiftTaste(npc, item.ParentSheetIndex);
			}
		}

		public void onGiftGiven(NPC npc, Object item)
		{
			if ((bool)item.bigCraftable)
			{
				return;
			}
			if (!giftedItems.ContainsKey(npc.name))
			{
				giftedItems[npc.name] = new SerializableDictionary<int, int>();
			}
			if (!giftedItems[npc.name].ContainsKey(item.ParentSheetIndex))
			{
				giftedItems[npc.name][item.ParentSheetIndex] = 0;
			}
			giftedItems[npc.name][item.ParentSheetIndex] = giftedItems[npc.name][item.ParentSheetIndex] + 1;
			if (Game1.player.team.specialOrders == null)
			{
				return;
			}
			foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
			{
				if (specialOrder.onGiftGiven != null)
				{
					specialOrder.onGiftGiven(Game1.player, npc, item);
				}
			}
		}

		public bool hasGiftTasteBeenRevealed(NPC npc, int item_index)
		{
			if (hasItemBeenGifted(npc, item_index))
			{
				return true;
			}
			if (!giftedItems.ContainsKey(npc.name))
			{
				return false;
			}
			if (!giftedItems[npc.name].ContainsKey(item_index))
			{
				return false;
			}
			return true;
		}

		public bool hasItemBeenGifted(NPC npc, int item_index)
		{
			if (!giftedItems.ContainsKey(npc.name))
			{
				return false;
			}
			if (!giftedItems[npc.name].ContainsKey(item_index))
			{
				return false;
			}
			return giftedItems[npc.name][item_index] > 0;
		}

		public void MarkItemAsTailored(Item item)
		{
			if (item != null)
			{
				string standardDescriptionFromItem = Utility.getStandardDescriptionFromItem(item, 1);
				if (!tailoredItems.ContainsKey(standardDescriptionFromItem))
				{
					tailoredItems[standardDescriptionFromItem] = 0;
				}
				tailoredItems[standardDescriptionFromItem]++;
			}
		}

		public bool HasTailoredThisItem(Item item)
		{
			if (item == null)
			{
				return false;
			}
			string standardDescriptionFromItem = Utility.getStandardDescriptionFromItem(item, 1);
			if (tailoredItems.ContainsKey(standardDescriptionFromItem))
			{
				return true;
			}
			return false;
		}

		public void foundMineral(int index)
		{
			if (mineralsFound.ContainsKey(index))
			{
				mineralsFound[index]++;
			}
			else
			{
				mineralsFound.Add(index, 1);
			}
			if (!hasOrWillReceiveMail("artifactFound"))
			{
				mailReceived.Add("artifactFound");
			}
		}

		public void increaseBackpackSize(int howMuch)
		{
			MaxItems += howMuch;
			for (int i = 0; i < howMuch; i++)
			{
				items.Add(null);
			}
		}

		public void consumeObject(int index, int quantity)
		{
			for (int num = items.Count - 1; num >= 0; num--)
			{
				if (items[num] != null && items[num] is Object && (int)((Object)items[num]).parentSheetIndex == index)
				{
					int num2 = quantity;
					quantity -= items[num].Stack;
					items[num].Stack -= num2;
					if (items[num].Stack <= 0)
					{
						items[num] = null;
					}
					if (quantity <= 0)
					{
						break;
					}
				}
			}
		}

		public int getItemCount(int item_index, int min_price = 0)
		{
			return getItemCountInList(items, item_index, min_price);
		}

		public bool hasItemInInventory(int itemIndex, int quantity, int minPrice = 0)
		{
			switch (itemIndex)
			{
			case 858:
				return QiGems >= quantity;
			case 73:
				return (int)Game1.netWorldState.Value.GoldenWalnuts >= quantity;
			default:
				if (getItemCount(itemIndex, minPrice) >= quantity)
				{
					return true;
				}
				return false;
			}
		}

		public bool hasItemInInventoryNamed(string name)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] != null && items[i].Name != null && items[i].Name.Equals(name))
				{
					return true;
				}
			}
			return false;
		}

		public int howManyOfItemInInventory(int itemIndex)
		{
			int num = 0;
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] != null && ((items[i] is Object && !(items[i] is Furniture) && !(items[i] as Object).bigCraftable && ((Object)items[i]).ParentSheetIndex == itemIndex) || (items[i] is Object && ((Object)items[i]).Category == itemIndex)))
				{
					num += items[i].Stack;
				}
			}
			return num;
		}

		public int howManyOfItemInList(IList<Item> list, int itemIndex)
		{
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != null && ((list[i] is Object && !(list[i] is Furniture) && !(list[i] as Object).bigCraftable && ((Object)list[i]).ParentSheetIndex == itemIndex) || (list[i] is Object && ((Object)list[i]).Category == itemIndex)))
				{
					num += list[i].Stack;
				}
			}
			return num;
		}

		public int getItemCountInList(IList<Item> list, int item_index, int min_price = 0)
		{
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != null && list[i] is Object && !(list[i] is Furniture) && !(list[i] is Wallpaper) && !(list[i] as Object).bigCraftable && (((Object)list[i]).ParentSheetIndex == item_index || (list[i] is Object && ((Object)list[i]).Category == item_index) || CraftingRecipe.isThereSpecialIngredientRule((Object)list[i], item_index)))
				{
					num += list[i].Stack;
				}
			}
			return num;
		}

		public bool hasItemInList(IList<Item> list, int itemIndex, int quantity, int minPrice = 0)
		{
			if (getItemCountInList(list, itemIndex, minPrice) >= quantity)
			{
				return true;
			}
			return false;
		}

		public void addItemByMenuIfNecessaryElseHoldUp(Item item, ItemGrabMenu.behaviorOnItemSelect itemSelectedCallback = null)
		{
			mostRecentlyGrabbedItem = item;
			addItemsByMenuIfNecessary(new List<Item> { item }, itemSelectedCallback);
			if (Game1.activeClickableMenu == null && !Utility.IsNormalObjectAtParentSheetIndex(mostRecentlyGrabbedItem, 434))
			{
				holdUpItemThenMessage(item);
			}
		}

		public void addItemByMenuIfNecessary(Item item, ItemGrabMenu.behaviorOnItemSelect itemSelectedCallback = null)
		{
			addItemsByMenuIfNecessary(new List<Item> { item }, itemSelectedCallback);
		}

		public void addItemsByMenuIfNecessary(List<Item> itemsToAdd, ItemGrabMenu.behaviorOnItemSelect itemSelectedCallback = null)
		{
			if (itemsToAdd == null || !IsLocalPlayer)
			{
				return;
			}
			if (itemsToAdd.Count > 0 && itemsToAdd[0] is Object && Utility.IsNormalObjectAtParentSheetIndex(itemsToAdd[0], 434))
			{
				eatObject(itemsToAdd[0] as Object, overrideFullness: true);
				if (Game1.activeClickableMenu != null)
				{
					Game1.activeClickableMenu.exitThisMenu(playSound: false);
				}
				return;
			}
			for (int num = itemsToAdd.Count - 1; num >= 0; num--)
			{
				if (addItemToInventoryBool(itemsToAdd[num]))
				{
					itemSelectedCallback?.Invoke(itemsToAdd[num], this);
					itemsToAdd.Remove(itemsToAdd[num]);
				}
			}
			if (itemsToAdd.Count > 0)
			{
				Game1.activeClickableMenu = new ItemGrabMenu(itemsToAdd).setEssential(essential: true);
				(Game1.activeClickableMenu as ItemGrabMenu).inventory.showGrayedOutSlots = true;
				(Game1.activeClickableMenu as ItemGrabMenu).inventory.onAddItem = itemSelectedCallback;
				(Game1.activeClickableMenu as ItemGrabMenu).source = 2;
			}
		}

		public void ShowSitting()
		{
			if (!IsSitting())
			{
				return;
			}
			if (sittingFurniture != null)
			{
				FacingDirection = sittingFurniture.GetSittingDirection();
			}
			if (yJumpOffset != 0)
			{
				switch (FacingDirection)
				{
				case 0:
					FarmerSprite.setCurrentSingleFrame(12, 32000);
					break;
				case 1:
					FarmerSprite.setCurrentSingleFrame(6, 32000);
					break;
				case 3:
					FarmerSprite.setCurrentSingleFrame(6, 32000, secondaryArm: false, flip: true);
					break;
				case 2:
					FarmerSprite.setCurrentSingleFrame(0, 32000);
					break;
				}
				return;
			}
			switch (FacingDirection)
			{
			case 0:
				FarmerSprite.setCurrentSingleFrame(113, 32000);
				xOffset = 0f;
				yOffset = -40f;
				break;
			case 1:
				FarmerSprite.setCurrentSingleFrame(117, 32000);
				xOffset = -4f;
				yOffset = -32f;
				break;
			case 3:
				FarmerSprite.setCurrentSingleFrame(117, 32000, secondaryArm: false, flip: true);
				xOffset = 4f;
				yOffset = -32f;
				break;
			case 2:
				FarmerSprite.setCurrentSingleFrame(107, 32000, secondaryArm: true);
				xOffset = 0f;
				yOffset = -48f;
				break;
			}
		}

		public void showRiding()
		{
			if (!isRidingHorse())
			{
				return;
			}
			xOffset = -6f;
			switch (FacingDirection)
			{
			case 0:
				FarmerSprite.setCurrentSingleFrame(113, 32000);
				break;
			case 1:
				FarmerSprite.setCurrentSingleFrame(106, 32000);
				xOffset += 2f;
				break;
			case 3:
				FarmerSprite.setCurrentSingleFrame(106, 32000, secondaryArm: false, flip: true);
				xOffset = -12f;
				break;
			case 2:
				FarmerSprite.setCurrentSingleFrame(107, 32000);
				break;
			}
			if (isMoving())
			{
				switch (mount.Sprite.currentAnimationIndex)
				{
				case 0:
					yOffset = 0f;
					break;
				case 1:
					yOffset = -4f;
					break;
				case 2:
					yOffset = -4f;
					break;
				case 3:
					yOffset = 0f;
					break;
				case 4:
					yOffset = 4f;
					break;
				case 5:
					yOffset = 4f;
					break;
				}
			}
			else
			{
				yOffset = 0f;
			}
		}

		public void showCarrying()
		{
			if (Game1.eventUp || isRidingHorse() || Game1.killScreen || IsSitting())
			{
				return;
			}
			if ((bool)bathingClothes || onBridge.Value)
			{
				showNotCarrying();
				return;
			}
			if (!FarmerSprite.PauseForSingleAnimation && !isMoving())
			{
				switch (FacingDirection)
				{
				case 0:
					FarmerSprite.setCurrentFrame(144);
					break;
				case 1:
					FarmerSprite.setCurrentFrame(136);
					break;
				case 2:
					FarmerSprite.setCurrentFrame(128);
					break;
				case 3:
					FarmerSprite.setCurrentFrame(152);
					break;
				}
			}
			if (ActiveObject != null)
			{
				mostRecentlyGrabbedItem = ActiveObject;
			}
			if (IsLocalPlayer && mostRecentlyGrabbedItem != null && mostRecentlyGrabbedItem is Object && Utility.IsNormalObjectAtParentSheetIndex(mostRecentlyGrabbedItem, 434))
			{
				eatHeldObject();
			}
		}

		public void showNotCarrying()
		{
			if (!FarmerSprite.PauseForSingleAnimation && !isMoving())
			{
				bool flag = canOnlyWalk || (bool)bathingClothes || onBridge.Value;
				switch (FacingDirection)
				{
				case 0:
					FarmerSprite.setCurrentFrame(flag ? 16 : 48, flag ? 1 : 0);
					break;
				case 1:
					FarmerSprite.setCurrentFrame(flag ? 8 : 40, flag ? 1 : 0);
					break;
				case 2:
					FarmerSprite.setCurrentFrame((!flag) ? 32 : 0, flag ? 1 : 0);
					break;
				case 3:
					FarmerSprite.setCurrentFrame(flag ? 24 : 56, flag ? 1 : 0);
					break;
				}
			}
		}

		public int GetDaysMarried()
		{
			if (spouse == null || spouse == "")
			{
				return 0;
			}
			return friendshipData[spouse].DaysMarried;
		}

		public Friendship GetSpouseFriendship()
		{
			if (Game1.player.team.GetSpouse(UniqueMultiplayerID).HasValue)
			{
				long value = Game1.player.team.GetSpouse(UniqueMultiplayerID).Value;
				return Game1.player.team.GetFriendship(UniqueMultiplayerID, value);
			}
			if (spouse == null || spouse == "")
			{
				return null;
			}
			return friendshipData[spouse];
		}

		public bool hasDailyQuest()
		{
			for (int num = questLog.Count - 1; num >= 0; num--)
			{
				if ((bool)questLog[num].dailyQuest)
				{
					return true;
				}
			}
			return false;
		}

		public void showToolUpgradeAvailability()
		{
			int dayOfMonth = Game1.dayOfMonth;
			if (!(toolBeingUpgraded != null) || (int)daysLeftForToolUpgrade > 0 || toolBeingUpgraded.Value == null || Utility.isFestivalDay(dayOfMonth, Game1.currentSeason) || (!(Game1.shortDayNameFromDayOfSeason(dayOfMonth) != "Fri") && hasCompletedCommunityCenter() && !Game1.isRaining) || hasReceivedToolUpgradeMessageYet)
			{
				return;
			}
			if (Game1.newDay)
			{
				Game1.morningQueue.Enqueue(delegate
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:ToolReady", toolBeingUpgraded.Value.DisplayName));
				});
			}
			else
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:ToolReady", toolBeingUpgraded.Value.DisplayName));
			}
			hasReceivedToolUpgradeMessageYet = true;
		}

		public void dayupdate()
		{
			if (IsSitting())
			{
				StopSitting(animate: false);
			}
			resetFriendshipsForNewDay();
			hasUsedDailyRevive.Value = false;
			acceptedDailyQuest.Set(newValue: false);
			dancePartner.Value = null;
			festivalScore = 0;
			forceTimePass = false;
			if ((int)daysLeftForToolUpgrade > 0)
			{
				daysLeftForToolUpgrade.Value--;
			}
			if ((int)daysUntilHouseUpgrade > 0)
			{
				daysUntilHouseUpgrade.Value--;
				if ((int)daysUntilHouseUpgrade <= 0)
				{
					FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(this);
					homeOfFarmer.moveObjectsForHouseUpgrade((int)houseUpgradeLevel + 1);
					houseUpgradeLevel.Value++;
					daysUntilHouseUpgrade.Value = -1;
					homeOfFarmer.setMapForUpgradeLevel(houseUpgradeLevel);
					Game1.stats.checkForBuildingUpgradeAchievements();
				}
			}
			for (int num = questLog.Count - 1; num >= 0; num--)
			{
				if ((bool)questLog[num].dailyQuest)
				{
					questLog[num].daysLeft.Value--;
					if ((int)questLog[num].daysLeft <= 0 && !questLog[num].completed)
					{
						questLog.RemoveAt(num);
					}
				}
			}
			ClearBuffs();
			if (MaxStamina >= 508 && !mailReceived.Contains("gotMaxStamina"))
			{
				mailReceived.Add("gotMaxStamina");
			}
			if (leftRing.Value != null)
			{
				leftRing.Value.onDayUpdate(this, base.currentLocation);
			}
			if (rightRing.Value != null)
			{
				rightRing.Value.onDayUpdate(this, base.currentLocation);
			}
			bobber = "";
			float num2 = Stamina;
			Stamina = MaxStamina;
			if ((bool)exhausted)
			{
				exhausted.Value = false;
				Stamina = MaxStamina / 2 + 1;
			}
			int num3 = (((int)timeWentToBed == 0) ? Game1.timeOfDay : ((int)timeWentToBed));
			if (num3 > 2400)
			{
				float num4 = 1f - (float)(2600 - Math.Min(2600, num3)) / 200f;
				float num5 = num4 * (float)(MaxStamina / 2);
				Stamina -= num5;
				if (Game1.timeOfDay > 2700)
				{
					Stamina /= 2f;
				}
			}
			if (Game1.timeOfDay < 2700 && num2 > Stamina && !exhausted)
			{
				Stamina = num2;
			}
			health = maxHealth;
			List<string> list = new List<string>();
			List<string> list2 = activeDialogueEvents.Keys.ToList();
			foreach (string item in list2)
			{
				activeDialogueEvents[item]--;
				if (activeDialogueEvents[item] < 0)
				{
					if (item == "pennyRedecorating" && Utility.getHomeOfFarmer(this).GetSpouseBed() == null)
					{
						activeDialogueEvents[item] = 0;
					}
					else
					{
						list.Add(item);
					}
				}
			}
			foreach (string item2 in list)
			{
				activeDialogueEvents.Remove(item2);
			}
			hasMoved = false;
			if (Game1.random.NextDouble() < 0.905 && !hasOrWillReceiveMail("RarecrowSociety") && Utility.doesItemWithThisIndexExistAnywhere(136, bigCraftable: true) && Utility.doesItemWithThisIndexExistAnywhere(137, bigCraftable: true) && Utility.doesItemWithThisIndexExistAnywhere(138, bigCraftable: true) && Utility.doesItemWithThisIndexExistAnywhere(139, bigCraftable: true) && Utility.doesItemWithThisIndexExistAnywhere(140, bigCraftable: true) && Utility.doesItemWithThisIndexExistAnywhere(126, bigCraftable: true) && Utility.doesItemWithThisIndexExistAnywhere(110, bigCraftable: true) && Utility.doesItemWithThisIndexExistAnywhere(113, bigCraftable: true))
			{
				mailbox.Add("RarecrowSociety");
			}
			timeWentToBed.Value = 0;
		}

		public void doDivorce()
		{
			divorceTonight.Value = false;
			if (!isMarried())
			{
				return;
			}
			if (spouse != null)
			{
				NPC nPC = getSpouse();
				if (nPC == null)
				{
					return;
				}
				spouse = null;
				for (int num = specialItems.Count - 1; num >= 0; num--)
				{
					if (specialItems[num] == 460)
					{
						specialItems.RemoveAt(num);
					}
				}
				if (friendshipData.ContainsKey(nPC.name))
				{
					friendshipData[nPC.name].Points = 0;
					friendshipData[nPC.name].RoommateMarriage = false;
					friendshipData[nPC.name].Status = FriendshipStatus.Divorced;
				}
				Utility.getHomeOfFarmer(this).showSpouseRoom();
				Game1.getFarm().UpdatePatio();
				removeQuest(126);
			}
			else if (team.GetSpouse(UniqueMultiplayerID).HasValue)
			{
				long value = team.GetSpouse(UniqueMultiplayerID).Value;
				Friendship friendship = team.GetFriendship(UniqueMultiplayerID, value);
				friendship.Points = 0;
				friendship.RoommateMarriage = false;
				friendship.Status = FriendshipStatus.Divorced;
			}
		}

		public static void showReceiveNewItemMessage(Farmer who)
		{
			string text = who.mostRecentlyGrabbedItem.checkForSpecialItemHoldUpMeessage();
			if (text != null)
			{
				Game1.drawObjectDialogue(text);
			}
			else if ((int)who.mostRecentlyGrabbedItem.parentSheetIndex == 472 && who.mostRecentlyGrabbedItem.Stack == 15)
			{
				TutorialManager.Instance.completeTutorial(tutorialType.GET_PARSNIPS);
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1918"));
			}
			else
			{
				Game1.drawObjectDialogue((who.mostRecentlyGrabbedItem.Stack > 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1922", who.mostRecentlyGrabbedItem.Stack, who.mostRecentlyGrabbedItem.DisplayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1919", who.mostRecentlyGrabbedItem.DisplayName, Lexicon.getProperArticleForWord(who.mostRecentlyGrabbedItem.DisplayName)));
			}
			who.completelyStopAnimatingOrDoingAction();
		}

		public static void showEatingItem(Farmer who)
		{
			TemporaryAnimatedSprite temporaryAnimatedSprite = null;
			if (who.itemToEat == null)
			{
				return;
			}
			switch (who.FarmerSprite.currentAnimationIndex)
			{
			case 1:
				temporaryAnimatedSprite = ((!who.IsLocalPlayer || who.itemToEat == null || !(who.itemToEat is Object) || !Utility.IsNormalObjectAtParentSheetIndex(who.itemToEat, 434)) ? new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (who.itemToEat as Object).parentSheetIndex, 16, 16), 254f, 1, 0, who.Position + new Vector2(-21f, -112f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f) : new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(368, 16, 16, 16), 62.75f, 8, 2, who.Position + new Vector2(-21f, -112f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f));
				break;
			case 2:
				if (who.IsLocalPlayer && who.itemToEat != null && who.itemToEat is Object && Utility.IsNormalObjectAtParentSheetIndex(who.itemToEat, 434))
				{
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(368, 16, 16, 16), 81.25f, 8, 0, who.Position + new Vector2(-21f, -108f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, -0.01f, 0f, 0f)
					{
						motion = new Vector2(0.8f, -11f),
						acceleration = new Vector2(0f, 0.5f)
					};
					break;
				}
				if (Game1.currentLocation == who.currentLocation)
				{
					Game1.playSound("dwop");
				}
				temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (who.itemToEat as Object).parentSheetIndex, 16, 16), 650f, 1, 0, who.Position + new Vector2(-21f, -108f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, -0.01f, 0f, 0f)
				{
					motion = new Vector2(0.8f, -11f),
					acceleration = new Vector2(0f, 0.5f)
				};
				break;
			case 3:
				who.yJumpVelocity = 6f;
				who.yJumpOffset = 1;
				break;
			case 4:
			{
				if (Game1.currentLocation == who.currentLocation && who.ShouldHandleAnimationSound())
				{
					Game1.playSound("eat");
				}
				for (int i = 0; i < 8; i++)
				{
					Microsoft.Xna.Framework.Rectangle sourceRectForStandardTileSheet = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (who.itemToEat as Object).parentSheetIndex, 16, 16);
					sourceRectForStandardTileSheet.X += 8;
					sourceRectForStandardTileSheet.Y += 8;
					sourceRectForStandardTileSheet.Width = 4;
					sourceRectForStandardTileSheet.Height = 4;
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", sourceRectForStandardTileSheet, 400f, 1, 0, who.Position + new Vector2(24f, -48f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, Game1.random.Next(-6, -3)),
						acceleration = new Vector2(0f, 0.5f)
					};
					who.currentLocation.temporarySprites.Add(temporaryAnimatedSprite);
				}
				return;
			}
			default:
				who.freezePause = 0;
				break;
			}
			if (temporaryAnimatedSprite != null)
			{
				who.currentLocation.temporarySprites.Add(temporaryAnimatedSprite);
			}
		}

		public static void eatItem(Farmer who)
		{
		}

		public bool hasBuff(int whichBuff)
		{
			return appliedSpecialBuffs.ContainsKey(whichBuff);
		}

		public bool hasOrWillReceiveMail(string id)
		{
			if (!mailReceived.Contains(id) && !mailForTomorrow.Contains(id) && !Game1.mailbox.Contains(id))
			{
				return mailForTomorrow.Contains(id + "%&NL&%");
			}
			return true;
		}

		public static void showHoldingItem(Farmer who)
		{
			if (who.mostRecentlyGrabbedItem is SpecialItem)
			{
				TemporaryAnimatedSprite temporarySpriteForHoldingUp = (who.mostRecentlyGrabbedItem as SpecialItem).getTemporarySpriteForHoldingUp(who.Position + new Vector2(0f, -124f));
				temporarySpriteForHoldingUp.motion = new Vector2(0f, -0.1f);
				temporarySpriteForHoldingUp.scale = 4f;
				temporarySpriteForHoldingUp.interval = 2500f;
				temporarySpriteForHoldingUp.totalNumberOfLoops = 0;
				temporarySpriteForHoldingUp.animationLength = 1;
				Game1.currentLocation.temporarySprites.Add(temporarySpriteForHoldingUp);
			}
			else if (who.mostRecentlyGrabbedItem is Slingshot)
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\weapons", Game1.getSquareSourceRectForNonStandardTileSheet(Tool.weaponsTexture, 16, 16, (who.mostRecentlyGrabbedItem as Slingshot).IndexOfMenuItemView), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -0.1f)
				});
			}
			else if (who.mostRecentlyGrabbedItem is MeleeWeapon)
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\weapons", Game1.getSquareSourceRectForNonStandardTileSheet(Tool.weaponsTexture, 16, 16, (who.mostRecentlyGrabbedItem as MeleeWeapon).IndexOfMenuItemView), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -0.1f)
				});
			}
			else if (who.mostRecentlyGrabbedItem is Boots)
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSquareSourceRectForNonStandardTileSheet(Game1.objectSpriteSheet, 16, 16, (who.mostRecentlyGrabbedItem as Boots).indexInTileSheet), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -0.1f)
				});
			}
			else if (who.mostRecentlyGrabbedItem is Hat)
			{
				Hat hat = who.mostRecentlyGrabbedItem as Hat;
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Farmer\\hats", new Microsoft.Xna.Framework.Rectangle((int)hat.which * 20 % FarmerRenderer.hatsTexture.Width, (int)hat.which * 20 / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20), 2500f, 1, 0, who.Position + new Vector2(-8f, -124f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -0.1f)
				});
			}
			else if (who.mostRecentlyGrabbedItem is Tool)
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\tools", Game1.getSquareSourceRectForNonStandardTileSheet(Game1.toolSpriteSheet, 16, 16, (who.mostRecentlyGrabbedItem as Tool).IndexOfMenuItemView), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -0.1f)
				});
			}
			else if (who.mostRecentlyGrabbedItem is Furniture)
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\furniture", (who.mostRecentlyGrabbedItem as Furniture).sourceRect, 2500f, 1, 0, who.Position + new Vector2(32 - (who.mostRecentlyGrabbedItem as Furniture).sourceRect.Width / 2 * 4, -188f), flicker: false, flipped: false)
				{
					motion = new Vector2(0f, -0.1f),
					scale = 4f,
					layerDepth = 1f
				});
			}
			else if (who.mostRecentlyGrabbedItem is Object && !(who.mostRecentlyGrabbedItem as Object).bigCraftable)
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, who.mostRecentlyGrabbedItem.parentSheetIndex, 16, 16), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false)
				{
					motion = new Vector2(0f, -0.1f),
					scale = 4f,
					layerDepth = 1f
				});
				if (who.IsLocalPlayer && Utility.IsNormalObjectAtParentSheetIndex(who.mostRecentlyGrabbedItem, 434))
				{
					who.eatHeldObject();
				}
			}
			else if (who.mostRecentlyGrabbedItem is Object)
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\Craftables", Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, who.mostRecentlyGrabbedItem.parentSheetIndex, 16, 32), 2500f, 1, 0, who.Position + new Vector2(0f, -188f), flicker: false, flipped: false)
				{
					motion = new Vector2(0f, -0.1f),
					scale = 4f,
					layerDepth = 1f
				});
			}
			else if (who.mostRecentlyGrabbedItem is Ring)
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, who.mostRecentlyGrabbedItem.parentSheetIndex, 16, 16), 2500f, 1, 0, who.Position + new Vector2(-4f, -124f), flicker: false, flipped: false)
				{
					motion = new Vector2(0f, -0.1f),
					scale = 4f,
					layerDepth = 1f
				});
			}
			if (who.mostRecentlyGrabbedItem == null)
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(420, 489, 25, 18), 2500f, 1, 0, who.Position + new Vector2(-20f, -152f), flicker: false, flipped: false)
				{
					motion = new Vector2(0f, -0.1f),
					scale = 4f,
					layerDepth = 1f
				});
			}
			else
			{
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(10, who.Position + new Vector2(32f, -96f), Color.White)
				{
					motion = new Vector2(0f, -0.1f)
				});
			}
		}

		public void holdUpItemThenMessage(Item item, bool showMessage = true)
		{
			completelyStopAnimatingOrDoingAction();
			if (showMessage)
			{
				DelayedAction.playSoundAfterDelay("getNewSpecialItem", 750);
			}
			faceDirection(2);
			freezePause = 4000;
			FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[3]
			{
				new FarmerSprite.AnimationFrame(57, 0),
				new FarmerSprite.AnimationFrame(57, 2500, secondaryArm: false, flip: false, showHoldingItem),
				showMessage ? new FarmerSprite.AnimationFrame((short)FarmerSprite.CurrentFrame, 500, secondaryArm: false, flip: false, showReceiveNewItemMessage, behaviorAtEndOfFrame: true) : new FarmerSprite.AnimationFrame((short)FarmerSprite.CurrentFrame, 500, secondaryArm: false, flip: false)
			});
			mostRecentlyGrabbedItem = item;
			canMove = false;
		}

		private void checkForLevelUp()
		{
			int num = 600;
			int num2 = 0;
			int level = Level;
			for (int i = 0; i <= 35; i++)
			{
				if (level <= i && totalMoneyEarned >= num)
				{
					NewSkillPointsToSpend += 2;
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1925"), Color.Violet, 3500f));
				}
				else if (totalMoneyEarned < num)
				{
					break;
				}
				int num3 = num;
				num += (int)((double)(num - num2) * 1.2);
				num2 = num3;
			}
		}

		public void resetState()
		{
			mount = null;
			removeBuffAttributes();
			TemporaryItem = null;
			swimming.Value = false;
			bathingClothes.Value = false;
			ignoreCollisions = false;
			resetItemStates();
			fireToolEvent.Clear();
			beginUsingToolEvent.Clear();
			endUsingToolEvent.Clear();
			sickAnimationEvent.Clear();
			passOutEvent.Clear();
			drinkAnimationEvent.Clear();
			eatAnimationEvent.Clear();
		}

		public void resetItemStates()
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] != null)
				{
					items[i].resetState();
				}
			}
		}

		public void clearBackpack()
		{
			for (int i = 0; i < items.Count; i++)
			{
				items[i] = null;
			}
		}

		public int numberOfItemsInInventory()
		{
			return numberOfItemsInInventory<Object>();
		}

		public int numberOfItemsInInventory<T>() where T : Item
		{
			int num = 0;
			foreach (Item item in items)
			{
				if (item != null && item is T)
				{
					num++;
				}
			}
			return num;
		}

		public void resetFriendshipsForNewDay()
		{
			foreach (string key in friendshipData.Keys)
			{
				bool flag = false;
				NPC characterFromName = Game1.getCharacterFromName(key);
				if (characterFromName == null)
				{
					characterFromName = Game1.getCharacterFromName<Child>(key, mustBeVillager: false);
				}
				if (characterFromName != null)
				{
					if (characterFromName != null && (bool)characterFromName.datable && !friendshipData[key].IsDating() && !characterFromName.isMarried())
					{
						flag = true;
					}
					if (spouse != null && key.Equals(spouse) && !hasPlayerTalkedToNPC(key))
					{
						changeFriendship(-20, characterFromName);
					}
					else if (characterFromName != null && friendshipData[key].IsDating() && !hasPlayerTalkedToNPC(key) && friendshipData[key].Points < 2500)
					{
						changeFriendship(-8, characterFromName);
					}
					if (hasPlayerTalkedToNPC(key))
					{
						friendshipData[key].TalkedToToday = false;
					}
					else if ((!flag && friendshipData[key].Points < 2500) || (flag && friendshipData[key].Points < 2000))
					{
						changeFriendship(-2, characterFromName);
					}
				}
			}
			WorldDate worldDate = new WorldDate(Game1.Date);
			worldDate.TotalDays++;
			updateFriendshipGifts(worldDate);
		}

		public virtual int GetAppliedMagneticRadius()
		{
			return Math.Max(128, magneticRadius);
		}

		public void updateFriendshipGifts(WorldDate date)
		{
			foreach (string key in friendshipData.Keys)
			{
				if (friendshipData[key].LastGiftDate == null || date.TotalDays != friendshipData[key].LastGiftDate.TotalDays)
				{
					friendshipData[key].GiftsToday = 0;
				}
				if (friendshipData[key].LastGiftDate == null || date.TotalSundayWeeks != friendshipData[key].LastGiftDate.TotalSundayWeeks)
				{
					if (friendshipData[key].GiftsThisWeek >= 2)
					{
						changeFriendship(10, Game1.getCharacterFromName(key));
					}
					friendshipData[key].GiftsThisWeek = 0;
				}
			}
		}

		public bool hasPlayerTalkedToNPC(string name)
		{
			if (!friendshipData.ContainsKey(name) && Game1.NPCGiftTastes.ContainsKey(name))
			{
				friendshipData.Add(name, new Friendship());
			}
			if (friendshipData.ContainsKey(name) && friendshipData[name].TalkedToToday)
			{
				return true;
			}
			return false;
		}

		public void fuelLantern(int units)
		{
			Tool toolFromName = getToolFromName("Lantern");
			if (toolFromName != null)
			{
				((Lantern)toolFromName).fuelLeft = Math.Min(100, ((Lantern)toolFromName).fuelLeft + units);
			}
		}

		public bool tryToCraftItem(List<int[]> ingredients, double successRate, int itemToCraft, bool bigCraftable, string craftingOrCooking)
		{
			List<int[]> list = new List<int[]>();
			foreach (int[] ingredient in ingredients)
			{
				if (ingredient[0] <= -100)
				{
					int num = 0;
					switch (ingredient[0])
					{
					case -100:
						num = WoodPieces;
						break;
					case -101:
						num = stonePieces;
						break;
					case -102:
						num = CopperPieces;
						break;
					case -103:
						num = IronPieces;
						break;
					case -104:
						num = CoalPieces;
						break;
					case -105:
						num = GoldPieces;
						break;
					case -106:
						num = IridiumPieces;
						break;
					}
					if (num < ingredient[1])
					{
						return false;
					}
					list.Add(ingredient);
					continue;
				}
				for (int i = 0; i < ingredient[1]; i++)
				{
					int[] array = new int[2] { 99999, -1 };
					for (int j = 0; j < items.Count; j++)
					{
						if (items[j] != null && items[j] is Object && ((Object)items[j]).ParentSheetIndex == ingredient[0] && !containsIndex(list, j))
						{
							list.Add(new int[2] { j, 1 });
							break;
						}
						if (items[j] != null && items[j] is Object && ((Object)items[j]).Category == ingredient[0] && !containsIndex(list, j) && ((Object)items[j]).Price < array[0])
						{
							array[0] = ((Object)items[j]).Price;
							array[1] = j;
						}
						if (j == items.Count - 1)
						{
							if (array[1] != -1)
							{
								list.Add(new int[2]
								{
									array[1],
									ingredient[1]
								});
								break;
							}
							return false;
						}
					}
				}
			}
			string text = "";
			switch (itemToCraft)
			{
			case 291:
				text = ((Object)items[list[0][0]]).Name;
				break;
			case 216:
				if (Game1.random.NextDouble() < 0.5)
				{
					itemToCraft++;
				}
				break;
			}
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1927", craftingOrCooking));
			isCrafting = true;
			Game1.playSound("crafting");
			int num2 = -1;
			string message = Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1930");
			if (bigCraftable)
			{
				Game1.player.ActiveObject = new Object(Vector2.Zero, itemToCraft);
				Game1.player.showCarrying();
			}
			else if (itemToCraft < 0)
			{
				if (1 == 0)
				{
					message = Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1935");
				}
			}
			else
			{
				num2 = list[0][0];
				if (list[0][0] < 0)
				{
					for (int k = 0; k < items.Count; k++)
					{
						if (items[k] == null)
						{
							num2 = k;
							break;
						}
						if (k == (int)maxItems - 1)
						{
							Game1.pauseThenMessage(craftingTime + ingredients.Count() * 500, Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1936"), showProgressBar: true);
							return false;
						}
					}
				}
				if (!text.Equals(""))
				{
					items[num2] = new Object(Vector2.Zero, itemToCraft, text + " Bobber", canBeSetDown: true, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
				}
				else
				{
					items[num2] = new Object(Vector2.Zero, itemToCraft, null, canBeSetDown: true, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
				}
			}
			Game1.pauseThenMessage(craftingTime + ingredients.Count * 500, message, showProgressBar: true);
			string text2 = craftingOrCooking.ToLower();
			if (!(text2 == "crafting"))
			{
				if (text2 == "cooking")
				{
					Game1.stats.ItemsCooked++;
				}
			}
			else
			{
				Game1.stats.ItemsCrafted++;
			}
			foreach (int[] item in list)
			{
				if (item[0] <= -100)
				{
					switch (item[0])
					{
					case -100:
						WoodPieces -= item[1];
						break;
					case -101:
						stonePieces -= item[1];
						break;
					case -102:
						CopperPieces -= item[1];
						break;
					case -103:
						IronPieces -= item[1];
						break;
					case -104:
						CoalPieces -= item[1];
						break;
					case -105:
						GoldPieces -= item[1];
						break;
					case -106:
						IridiumPieces -= item[1];
						break;
					}
				}
				else if (item[0] != num2)
				{
					items[item[0]] = null;
				}
			}
			return true;
		}

		private static bool containsIndex(List<int[]> locationOfIngredients, int index)
		{
			for (int i = 0; i < locationOfIngredients.Count; i++)
			{
				if (locationOfIngredients[i][0] == index)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsEquippedItem(Item item)
		{
			if (item == null)
			{
				return false;
			}
			if (item == Game1.player.hat.Value)
			{
				return true;
			}
			if (item == Game1.player.shirtItem.Value)
			{
				return true;
			}
			if (item == Game1.player.pantsItem.Value)
			{
				return true;
			}
			if (item == Game1.player.leftRing.Value)
			{
				return true;
			}
			if (item == Game1.player.rightRing.Value)
			{
				return true;
			}
			if (item == Game1.player.boots.Value)
			{
				return true;
			}
			return false;
		}

		public override bool collideWith(Object o)
		{
			base.collideWith(o);
			if (isRidingHorse() && o is Fence)
			{
				mount.squeezeForGate();
				switch (FacingDirection)
				{
				case 3:
					if (o.tileLocation.X > (float)getTileX())
					{
						return false;
					}
					break;
				case 1:
					if (o.tileLocation.X < (float)getTileX())
					{
						return false;
					}
					break;
				}
			}
			return true;
		}

		public void changeIntoSwimsuit()
		{
			bathingClothes.Value = true;
			Halt();
			setRunning(isRunning: false);
			canOnlyWalk = true;
		}

		public void changeOutOfSwimSuit()
		{
			bathingClothes.Value = false;
			canOnlyWalk = false;
			Halt();
			FarmerSprite.StopAnimation();
			if (Game1.options.autoRun)
			{
				setRunning(isRunning: true);
			}
		}

		public bool ownsFurniture(string name)
		{
			foreach (string item in furnitureOwned)
			{
				if (item.Equals(name))
				{
					return true;
				}
			}
			return false;
		}

		public void showFrame(int frame, bool flip = false)
		{
			List<FarmerSprite.AnimationFrame> list = new List<FarmerSprite.AnimationFrame>();
			list.Add(new FarmerSprite.AnimationFrame(Convert.ToInt32(frame), 100, secondaryArm: false, flip));
			FarmerSprite.setCurrentAnimation(list.ToArray());
			FarmerSprite.loop = true;
			FarmerSprite.PauseForSingleAnimation = true;
			Sprite.currentFrame = Convert.ToInt32(frame);
		}

		public void stopShowingFrame()
		{
			FarmerSprite.loop = false;
			FarmerSprite.PauseForSingleAnimation = false;
			completelyStopAnimatingOrDoingAction();
		}

		public Item addItemToInventory(Item item, List<Item> affected_items_list)
		{
			if (item == null)
			{
				return null;
			}
			if (item is SpecialItem)
			{
				return item;
			}
			for (int i = 0; i < (int)maxItems; i++)
			{
				if (i < items.Count && items[i] != null && items[i].maximumStackSize() != -1 && items[i].Stack < items[i].maximumStackSize() && items[i].Name.Equals(item.Name) && (!(item is Object) || !(items[i] is Object) || ((item as Object).quality.Value == (items[i] as Object).quality.Value && (item as Object).parentSheetIndex.Value == (items[i] as Object).parentSheetIndex.Value)) && item.canStackWith(items[i]))
				{
					int num = items[i].addToStack(item);
					affected_items_list?.Add(items[i]);
					if (num <= 0)
					{
						return null;
					}
					item.Stack = num;
				}
			}
			for (int j = 0; j < (int)maxItems; j++)
			{
				if (items.Count > j && items[j] == null)
				{
					items[j] = item;
					affected_items_list?.Add(items[j]);
					return null;
				}
			}
			return item;
		}

		public virtual void BeginSitting(ISittable furniture)
		{
			if (furniture == null || bathingClothes.Value || swimming.Value || isRidingHorse() || !CanMove || UsingTool || base.IsEmoting)
			{
				return;
			}
			Vector2? vector = furniture.AddSittingFarmer(this);
			if (!vector.HasValue)
			{
				return;
			}
			base.currentLocation.playSound("woodyStep");
			Halt();
			synchronizedJump(4f);
			FarmerSprite.StopAnimation();
			sittingFurniture = furniture;
			mapChairSitPosition.Value = new Vector2(-1f, -1f);
			if (sittingFurniture is MapSeat)
			{
				Vector2? sittingPosition = sittingFurniture.GetSittingPosition(this, ignore_offsets: true);
				if (sittingPosition.HasValue)
				{
					mapChairSitPosition.Value = sittingPosition.Value;
				}
			}
			isSitting.Value = true;
			LerpPosition(base.Position, new Vector2(vector.Value.X * 64f, vector.Value.Y * 64f), 0.15f);
			freezePause += 100;
		}

		public virtual void LerpPosition(Vector2 start_position, Vector2 end_position, float duration)
		{
			freezePause = (int)(duration * 1000f);
			lerpStartPosition = start_position;
			lerpEndPosition = end_position;
			lerpPosition = 0f;
			lerpDuration = duration;
		}

		public virtual void StopSitting(bool animate = true)
		{
			if (sittingFurniture == null)
			{
				return;
			}
			ISittable sittable = sittingFurniture;
			if (!animate)
			{
				mapChairSitPosition.Value = new Vector2(-1f, -1f);
				sittable.RemoveSittingFarmer(this);
			}
			bool flag = false;
			bool flag2 = false;
			Vector2 vector = base.Position;
			if (sittable.IsSeatHere(base.currentLocation))
			{
				flag = true;
				List<Vector2> list = new List<Vector2>();
				Vector2 vector2 = new Vector2(sittable.GetSeatBounds().Left, sittable.GetSeatBounds().Top);
				if (sittable.IsSittingHere(this))
				{
					vector2 = sittable.GetSittingPosition(this, ignore_offsets: true).Value;
				}
				if (sittable.GetSittingDirection() == 2)
				{
					list.Add(vector2 + new Vector2(0f, 1f));
					SortSeatExitPositions(list, vector2 + new Vector2(1f, 0f), vector2 + new Vector2(-1f, 0f), vector2 + new Vector2(0f, -1f));
				}
				else if (sittable.GetSittingDirection() == 1)
				{
					list.Add(vector2 + new Vector2(1f, 0f));
					SortSeatExitPositions(list, vector2 + new Vector2(0f, -1f), vector2 + new Vector2(0f, 1f), vector2 + new Vector2(-1f, 0f));
				}
				else if (sittable.GetSittingDirection() == 3)
				{
					list.Add(vector2 + new Vector2(-1f, 0f));
					SortSeatExitPositions(list, vector2 + new Vector2(0f, 1f), vector2 + new Vector2(0f, -1f), vector2 + new Vector2(1f, 0f));
				}
				else if (sittable.GetSittingDirection() == 0)
				{
					list.Add(vector2 + new Vector2(0f, -1f));
					SortSeatExitPositions(list, vector2 + new Vector2(-1f, 0f), vector2 + new Vector2(1f, 0f), vector2 + new Vector2(0f, 1f));
				}
				Microsoft.Xna.Framework.Rectangle seatBounds = sittable.GetSeatBounds();
				seatBounds.Inflate(1, 1);
				foreach (Vector2 item in Utility.getBorderOfThisRectangle(seatBounds))
				{
					list.Add(item);
				}
				foreach (Vector2 item2 in list)
				{
					setTileLocation(item2);
					Object objectAtTile = base.currentLocation.getObjectAtTile((int)item2.X, (int)item2.Y);
					if (!base.currentLocation.isCollidingPosition(GetBoundingBox(), Game1.viewport, isFarmer: true, 0, glider: false, this) && (objectAtTile == null || objectAtTile.isPassable()))
					{
						if (animate)
						{
							base.currentLocation.playSound("coin");
							synchronizedJump(4f);
							LerpPosition(vector2 * 64f, item2 * 64f, 0.15f);
						}
						flag2 = true;
						break;
					}
				}
			}
			if (!flag2)
			{
				if (animate)
				{
					base.currentLocation.playSound("coin");
				}
				base.Position = vector;
				if (flag)
				{
					Microsoft.Xna.Framework.Rectangle seatBounds2 = sittable.GetSeatBounds();
					seatBounds2.X *= 64;
					seatBounds2.Y *= 64;
					seatBounds2.Width *= 64;
					seatBounds2.Height *= 64;
					temporaryPassableTiles.Add(seatBounds2);
				}
			}
			if (!animate)
			{
				sittingFurniture = null;
				isSitting.Value = false;
				Halt();
				showNotCarrying();
			}
			else
			{
				isStopSitting = true;
			}
			Game1.haltAfterCheck = false;
			yOffset = 0f;
			xOffset = 0f;
		}

		public void SortSeatExitPositions(List<Vector2> list, Vector2 a, Vector2 b, Vector2 c)
		{
			Vector2 mouse_pos = Utility.PointToVector2(Game1.getMousePosition(ui_scale: false)) + new Vector2(Game1.viewport.X, Game1.viewport.Y);
			Vector2 zero = Vector2.Zero;
			if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveUpButton) || (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.Y > 0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadUp))))
			{
				zero.Y -= 1f;
			}
			else if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveDownButton) || (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.Y < -0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadDown))))
			{
				zero.Y += 1f;
			}
			if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveLeftButton) || (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.X < -0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadLeft))))
			{
				zero.X -= 1f;
			}
			else if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveRightButton) || (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.X > 0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadRight))))
			{
				zero.X += 1f;
			}
			if (zero != Vector2.Zero)
			{
				mouse_pos = getStandingPosition() + zero * 64f;
			}
			mouse_pos /= 64f;
			List<Vector2> list2 = new List<Vector2>();
			list2.Add(a);
			list2.Add(b);
			list2.Add(c);
			list2.Sort((Vector2 d, Vector2 e) => (d + new Vector2(0.5f, 0.5f) - mouse_pos).Length().CompareTo((e + new Vector2(0.5f, 0.5f) - mouse_pos).Length()));
			list.AddRange(list2);
		}

		public virtual bool IsSitting()
		{
			return isSitting.Value;
		}

		public Item addItemToInventory(Item item)
		{
			return addItemToInventory(item, null);
		}

		public bool isInventoryFull()
		{
			for (int i = 0; i < (int)maxItems; i++)
			{
				if (items.Count > i && items[i] == null)
				{
					return false;
				}
			}
			return true;
		}

		public bool couldInventoryAcceptThisItem(Item item, bool message_if_full = true)
		{
			if (item is Object && (item as Object).IsRecipe)
			{
				return true;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(item, 102))
			{
				return true;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(item, 73))
			{
				return true;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(item, 930))
			{
				return true;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(item, 858))
			{
				return true;
			}
			for (int i = 0; i < (int)maxItems; i++)
			{
				if (items.Count > i && (items[i] == null || (item is Object && items[i] is Object && items[i].Stack + item.Stack <= items[i].maximumStackSize() && (items[i] as Object).canStackWith(item))))
				{
					return true;
				}
			}
			if (IsLocalPlayer && isInventoryFull() && Game1.hudMessages.Count() == 0 && message_if_full)
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
			}
			return false;
		}

		public bool couldInventoryAcceptThisObject(int index, int stack, int quality = 0)
		{
			switch (index)
			{
			case 102:
				return true;
			case 73:
				return true;
			case 858:
				return true;
			default:
			{
				for (int i = 0; i < (int)maxItems; i++)
				{
					if (items.Count > i && (items[i] == null || (items[i] is Object && items[i].Stack + stack <= items[i].maximumStackSize() && (items[i] as Object).ParentSheetIndex == index && (int)(items[i] as Object).quality == quality)))
					{
						return true;
					}
				}
				if (IsLocalPlayer && isInventoryFull() && Game1.hudMessages.Count() == 0)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
				}
				return false;
			}
			}
		}

		public bool hasItemOfType(string type)
		{
			for (int i = 0; i < (int)maxItems; i++)
			{
				if (items.Count > i && items[i] is Object && (items[i] as Object).type.Equals(type))
				{
					return true;
				}
			}
			return false;
		}

		public NPC getSpouse()
		{
			if (isMarried() && spouse != null)
			{
				return Game1.getCharacterFromName(spouse);
			}
			return null;
		}

		public int freeSpotsInInventory()
		{
			int num = 0;
			for (int i = 0; i < (int)maxItems; i++)
			{
				if (i < items.Count && items[i] == null)
				{
					num++;
				}
			}
			return num;
		}

		public Item hasItemWithNameThatContains(string name)
		{
			for (int i = 0; i < (int)maxItems; i++)
			{
				if (i < items.Count && items[i] != null && items[i].netName != null && items[i].Name.Contains(name))
				{
					return items[i];
				}
			}
			return null;
		}

		public bool fakeAddItemToInventoryBool(Item item)
		{
			if (item == null)
			{
				return false;
			}
			int stack = item.Stack;
			Item item2 = null;
			bool flag = item2 == null || item2.Stack != item.Stack || item is SpecialItem;
			if (item is Object)
			{
				(item as Object).reloadSprite();
			}
			if (flag && IsLocalPlayer)
			{
				if (item != null)
				{
					if (IsLocalPlayer && !item.HasBeenInInventory)
					{
						if (item is SpecialItem)
						{
							(item as SpecialItem).actionWhenReceived(this);
							return true;
						}
						if (item is Object && (item as Object).specialItem)
						{
							if ((bool)(item as Object).bigCraftable || item is Furniture)
							{
								if (!specialBigCraftables.Contains((item as Object).parentSheetIndex))
								{
									specialBigCraftables.Add((item as Object).parentSheetIndex);
								}
							}
							else if (!specialItems.Contains((item as Object).parentSheetIndex))
							{
								specialItems.Add((item as Object).parentSheetIndex);
							}
						}
						if (item is Object && (item as Object).Category == -2 && !(item as Object).hasBeenPickedUpByFarmer)
						{
							foundMineral((item as Object).parentSheetIndex);
						}
						else if (!(item is Furniture) && item is Object && (item as Object).type != null && (item as Object).type.Contains("Arch") && !(item as Object).hasBeenPickedUpByFarmer)
						{
							foundArtifact((item as Object).parentSheetIndex, 1);
						}
						if (Utility.IsNormalObjectAtParentSheetIndex(item, 102))
						{
							foundArtifact((item as Object).parentSheetIndex, 1);
							removeItemFromInventory(item);
						}
						else
						{
							switch ((int)item.parentSheetIndex)
							{
							case 384:
								Game1.stats.GoldFound += (uint)item.Stack;
								break;
							case 378:
								Game1.stats.CopperFound += (uint)item.Stack;
								break;
							case 380:
								Game1.stats.IronFound += (uint)item.Stack;
								break;
							case 386:
								Game1.stats.IridiumFound += (uint)item.Stack;
								break;
							}
						}
					}
					if (item is Object && !item.HasBeenInInventory)
					{
						if (!(item is Furniture) && !(item as Object).bigCraftable && !(item as Object).hasBeenPickedUpByFarmer)
						{
							checkForQuestComplete(null, (item as Object).parentSheetIndex, (item as Object).stack, item, null, 9);
						}
						(item as Object).hasBeenPickedUpByFarmer.Value = true;
						if ((bool)(item as Object).questItem)
						{
							return true;
						}
						if (Game1.activeClickableMenu == null)
						{
							switch ((int)(item as Object).parentSheetIndex)
							{
							case 535:
								if (!Game1.player.hasOrWillReceiveMail("geodeFound"))
								{
									mailReceived.Add("geodeFound");
									holdUpItemThenMessage(item);
								}
								break;
							case 378:
								if (!Game1.player.hasOrWillReceiveMail("copperFound"))
								{
									Game1.addMailForTomorrow("copperFound", noLetter: true);
								}
								break;
							case 102:
								Game1.stats.NotesFound++;
								Game1.playSound("newRecipe");
								holdUpItemThenMessage(item);
								return true;
							case 390:
								Game1.stats.StoneGathered++;
								if (Game1.stats.StoneGathered >= 100 && !Game1.player.hasOrWillReceiveMail("robinWell"))
								{
									Game1.addMailForTomorrow("robinWell");
								}
								break;
							}
						}
					}
					Color color = Color.WhiteSmoke;
					string text = item.DisplayName;
					if (item is Object)
					{
						switch ((string)(item as Object).type)
						{
						case "Arch":
							color = Color.Tan;
							text += Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1954");
							break;
						case "Fish":
							color = Color.SkyBlue;
							break;
						case "Mineral":
							color = Color.PaleVioletRed;
							break;
						case "Vegetable":
							color = Color.PaleGreen;
							break;
						case "Fruit":
							color = Color.Pink;
							break;
						}
					}
					if (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is ItemGrabMenu))
					{
						Game1.addHUDMessage(new HUDMessage(text, Math.Max(1, item.Stack), add: true, color, item));
					}
					if (freezePause <= 0)
					{
						mostRecentlyGrabbedItem = item;
					}
				}
				if (item is Object && !item.HasBeenInInventory)
				{
					checkForQuestComplete(null, item.parentSheetIndex, item.Stack, item, "", 10);
				}
				item.HasBeenInInventory = true;
				return flag;
			}
			return false;
		}

		public bool addItemToInventoryBool(Item item, bool makeActiveObject = false)
		{
			if (item == null)
			{
				return false;
			}
			int stack = item.Stack;
			Item item2 = (IsLocalPlayer ? addItemToInventory(item) : null);
			bool flag = item2 == null || item2.Stack != item.Stack || item is SpecialItem;
			if (item is Object)
			{
				(item as Object).reloadSprite();
			}
			bool flag2 = true;
			if (Utility.IsNormalObjectAtParentSheetIndex(item, 73))
			{
				flag = true;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(item, 858))
			{
				flag = true;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(item, 930))
			{
				flag = true;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(item, 102))
			{
				flag = true;
			}
			if (flag && IsLocalPlayer)
			{
				if (item != null)
				{
					if (IsLocalPlayer && !item.HasBeenInInventory)
					{
						if (item is SpecialItem)
						{
							(item as SpecialItem).actionWhenReceived(this);
							return true;
						}
						if (item is Object && (item as Object).specialItem)
						{
							if ((bool)(item as Object).bigCraftable || item is Furniture)
							{
								if (!specialBigCraftables.Contains((item as Object).parentSheetIndex))
								{
									specialBigCraftables.Add((item as Object).parentSheetIndex);
								}
							}
							else if (!specialItems.Contains((item as Object).parentSheetIndex))
							{
								specialItems.Add((item as Object).parentSheetIndex);
							}
						}
						if (item is Object && ((item as Object).Category == -2 || ((item as Object).Type != null && (item as Object).Type.Contains("Mineral"))) && !(item as Object).hasBeenPickedUpByFarmer)
						{
							foundMineral((item as Object).parentSheetIndex);
						}
						else if (!(item is Furniture) && item is Object && (item as Object).type != null && (item as Object).type.Contains("Arch") && !(item as Object).hasBeenPickedUpByFarmer)
						{
							foundArtifact((item as Object).parentSheetIndex, 1);
						}
						if (Utility.IsNormalObjectAtParentSheetIndex(item, 73))
						{
							foundWalnut(stack);
							removeItemFromInventory(item);
						}
						if (Utility.IsNormalObjectAtParentSheetIndex(item, 858))
						{
							QiGems += stack;
							Game1.playSound("qi_shop_purchase");
							base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 858, 16, 16), 100f, 1, 8, new Vector2(0f, -96f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								motion = new Vector2(0f, -6f),
								acceleration = new Vector2(0f, 0.2f),
								stopAcceleratingWhenVelocityIsZero = true,
								attachedCharacter = this,
								positionFollowsAttachedCharacter = true
							});
							removeItemFromInventory(item);
						}
						if (Utility.IsNormalObjectAtParentSheetIndex(item, 930))
						{
							int num = 10;
							health = Math.Min(maxHealth, Game1.player.health + num);
							base.currentLocation.debris.Add(new Debris(num, new Vector2(Game1.player.getStandingX(), Game1.player.getStandingY()), Color.Lime, 1f, this));
							Game1.playSound("healSound");
							removeItemFromInventory(item);
							flag2 = false;
						}
						if (Utility.IsNormalObjectAtParentSheetIndex(item, 102))
						{
							foundArtifact((item as Object).parentSheetIndex, 1);
							removeItemFromInventory(item);
						}
						else
						{
							switch ((int)item.parentSheetIndex)
							{
							case 384:
								Game1.stats.GoldFound += (uint)item.Stack;
								break;
							case 378:
								Game1.stats.CopperFound += (uint)item.Stack;
								break;
							case 380:
								Game1.stats.IronFound += (uint)item.Stack;
								break;
							case 386:
								Game1.stats.IridiumFound += (uint)item.Stack;
								break;
							}
						}
					}
					if (item is Object && !item.HasBeenInInventory)
					{
						Utility.checkItemFirstInventoryAdd(item);
					}
					Color color = Color.WhiteSmoke;
					string text = item.DisplayName;
					if (item is Object)
					{
						switch ((string)(item as Object).type)
						{
						case "Arch":
							color = Color.Tan;
							text += Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1954");
							break;
						case "Fish":
							color = Color.SkyBlue;
							break;
						case "Mineral":
							color = Color.PaleVioletRed;
							break;
						case "Vegetable":
							color = Color.PaleGreen;
							break;
						case "Fruit":
							color = Color.Pink;
							break;
						}
					}
					if (flag2 && (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is ItemGrabMenu)))
					{
						Game1.addHUDMessage(new HUDMessage(text, Math.Max(1, item.Stack), add: true, color, item));
					}
					if (freezePause <= 0)
					{
						mostRecentlyGrabbedItem = item;
					}
					if (item2 != null && makeActiveObject && item.Stack <= 1)
					{
						int indexOfInventoryItem = getIndexOfInventoryItem(item);
						Item value = items[currentToolIndex];
						items[currentToolIndex] = items[indexOfInventoryItem];
						items[indexOfInventoryItem] = value;
					}
				}
				return flag;
			}
			return false;
		}

		public int getIndexOfInventoryItem(Item item)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] == item || (items[i] != null && item != null && item.canStackWith(items[i])))
				{
					return i;
				}
			}
			return -1;
		}

		public void reduceActiveItemByOne()
		{
			if (CurrentItem != null && --CurrentItem.Stack <= 0)
			{
				removeItemFromInventory(CurrentItem);
				showNotCarrying();
			}
		}

		public bool removeItemsFromInventory(int index, int stack)
		{
			if (hasItemInInventory(index, stack))
			{
				if (index == 858 && QiGems >= stack)
				{
					QiGems -= stack;
					return true;
				}
				if (index == 73 && Game1.netWorldState.Value.GoldenWalnuts.Value >= stack)
				{
					Game1.netWorldState.Value.GoldenWalnuts.Value -= stack;
					return true;
				}
				for (int i = 0; i < items.Count; i++)
				{
					if (items[i] != null && items[i] is Object && (int)(items[i] as Object).parentSheetIndex == index)
					{
						if (items[i].Stack > stack)
						{
							items[i].Stack -= stack;
							return true;
						}
						stack -= items[i].Stack;
						items[i] = null;
					}
					if (stack <= 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		public void ReequipEnchantments()
		{
			if (Game1.player.CurrentTool == null || Game1.player.CurrentTool == null)
			{
				return;
			}
			Tool currentTool = Game1.player.CurrentTool;
			foreach (BaseEnchantment enchantment in currentTool.enchantments)
			{
				enchantment.OnEquip(this);
			}
		}

		public Item addItemToInventory(Item item, int position)
		{
			if (item != null && item is Object && (item as Object).specialItem)
			{
				if ((bool)(item as Object).bigCraftable)
				{
					if (!specialBigCraftables.Contains((item as Object).parentSheetIndex))
					{
						specialBigCraftables.Add((item as Object).parentSheetIndex);
					}
				}
				else if (!specialItems.Contains((item as Object).parentSheetIndex))
				{
					specialItems.Add((item as Object).parentSheetIndex);
				}
			}
			if (position >= 0 && position < items.Count)
			{
				if (items[position] == null)
				{
					items[position] = item;
					return null;
				}
				if (item != null && items[position].maximumStackSize() != -1 && items[position].Name.Equals(item.Name) && items[position].ParentSheetIndex == item.ParentSheetIndex && (!(item is Object) || !(items[position] is Object) || (item as Object).quality == (items[position] as Object).quality))
				{
					int num = items[position].addToStack(item);
					if (num <= 0)
					{
						return null;
					}
					item.Stack = num;
					return item;
				}
				Item result = items[position];
				items[position] = item;
				return result;
			}
			return item;
		}

		public void removeItemFromInventory(Item which)
		{
			int num = items.IndexOf(which);
			if (num >= 0 && num < items.Count)
			{
				items[num].actionWhenStopBeingHeld(this);
				items[num] = null;
			}
		}

		public Item removeItemFromInventory(int whichItemIndex)
		{
			if (whichItemIndex >= 0 && whichItemIndex < items.Count && items[whichItemIndex] != null)
			{
				Item item = items[whichItemIndex];
				items[whichItemIndex] = null;
				item.actionWhenStopBeingHeld(this);
				return item;
			}
			return null;
		}

		public bool isMarried()
		{
			if (team.IsMarried(UniqueMultiplayerID))
			{
				return true;
			}
			if (spouse != null && friendshipData.ContainsKey(spouse))
			{
				return friendshipData[spouse].IsMarried();
			}
			return false;
		}

		public bool isEngaged()
		{
			if (team.IsEngaged(UniqueMultiplayerID))
			{
				return true;
			}
			if (spouse != null && friendshipData.ContainsKey(spouse))
			{
				return friendshipData[spouse].IsEngaged();
			}
			return false;
		}

		public void removeFirstOfThisItemFromInventory(int parentSheetIndexOfItem)
		{
			if (ActiveObject != null && ActiveObject.ParentSheetIndex == parentSheetIndexOfItem)
			{
				ActiveObject.Stack--;
				if (ActiveObject.Stack <= 0)
				{
					ActiveObject = null;
					showNotCarrying();
				}
				return;
			}
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] != null && items[i] is Object && ((Object)items[i]).ParentSheetIndex == parentSheetIndexOfItem)
				{
					items[i].Stack--;
					if (items[i].Stack <= 0)
					{
						items[i] = null;
					}
					break;
				}
			}
		}

		public void changeShirt(int whichShirt, bool is_customization_screen = false)
		{
			if (is_customization_screen)
			{
				int num = whichShirt - (int)shirt;
				if (whichShirt < 0)
				{
					whichShirt = 111;
				}
				else if (whichShirt > 111)
				{
					whichShirt = 0;
				}
				if (whichShirt == 127 && !eventsSeen.Contains(3917601))
				{
					whichShirt = 127 + num;
					if (whichShirt > FarmerRenderer.shirtsTexture.Height / 32 * 16 - 1)
					{
						whichShirt = 0;
					}
				}
			}
			shirt.Set(whichShirt);
			FarmerRenderer.changeShirt(whichShirt);
		}

		public void changePantStyle(int whichPants, bool is_customization_screen = false)
		{
			if (is_customization_screen)
			{
				int num = whichPants - (int)pants;
				if (whichPants < 0)
				{
					whichPants = 3;
				}
				else if (whichPants > 3)
				{
					whichPants = 0;
				}
			}
			pants.Set(whichPants);
			FarmerRenderer.changePants(whichPants);
		}

		public void ConvertClothingOverrideToClothesItems()
		{
			if (pants.Value >= 0)
			{
				Clothing clothing = new Clothing(pants.Value);
				clothing.clothesColor.Set(pantsColor);
				pantsItem.Value = clothing;
				pants.Value = -1;
			}
			if (shirt.Value >= 0)
			{
				Clothing value = new Clothing(shirt.Value + 1000);
				shirtItem.Value = value;
				shirt.Value = -1;
			}
		}

		public static Dictionary<int, string> GetHairStyleMetadataFile()
		{
			if (hairStyleMetadataFile == null)
			{
				hairStyleMetadataFile = Game1.content.Load<Dictionary<int, string>>("Data\\HairData");
			}
			return hairStyleMetadataFile;
		}

		public static HairStyleMetadata GetHairStyleMetadata(int hair_index)
		{
			GetHairStyleMetadataFile();
			if (Farmer.hairStyleMetadata.ContainsKey(hair_index))
			{
				return Farmer.hairStyleMetadata[hair_index];
			}
			HairStyleMetadata hairStyleMetadata = null;
			try
			{
				if (hairStyleMetadataFile.ContainsKey(hair_index))
				{
					string text = hairStyleMetadataFile[hair_index];
					string[] array = text.Split('/');
					HairStyleMetadata hairStyleMetadata2 = new HairStyleMetadata();
					hairStyleMetadata2.texture = Game1.content.Load<Texture2D>("Characters\\Farmer\\" + array[0]);
					hairStyleMetadata2.tileX = int.Parse(array[1]);
					hairStyleMetadata2.tileY = int.Parse(array[2]);
					if (array.Length > 3 && array[3].ToLower() == "true")
					{
						hairStyleMetadata2.usesUniqueLeftSprite = true;
					}
					else
					{
						hairStyleMetadata2.usesUniqueLeftSprite = false;
					}
					if (array.Length > 4)
					{
						hairStyleMetadata2.coveredIndex = int.Parse(array[4]);
					}
					if (array.Length > 5 && array[5].ToLower() == "true")
					{
						hairStyleMetadata2.isBaldStyle = true;
					}
					else
					{
						hairStyleMetadata2.isBaldStyle = false;
					}
					hairStyleMetadata = hairStyleMetadata2;
				}
			}
			catch (Exception)
			{
			}
			Farmer.hairStyleMetadata[hair_index] = hairStyleMetadata;
			return hairStyleMetadata;
		}

		public static List<int> GetAllHairstyleIndices()
		{
			if (allHairStyleIndices != null)
			{
				return allHairStyleIndices;
			}
			GetHairStyleMetadataFile();
			allHairStyleIndices = new List<int>();
			int num = FarmerRenderer.hairStylesTexture.Height / 96 * 8;
			for (int i = 0; i < num; i++)
			{
				allHairStyleIndices.Add(i);
			}
			foreach (int key in hairStyleMetadataFile.Keys)
			{
				if (key >= 0 && !allHairStyleIndices.Contains(key))
				{
					allHairStyleIndices.Add(key);
				}
			}
			allHairStyleIndices.Sort();
			return allHairStyleIndices;
		}

		public static int GetLastHairStyle()
		{
			return GetAllHairstyleIndices()[GetAllHairstyleIndices().Count() - 1];
		}

		public void changeHairStyle(int whichHair)
		{
			bool flag = isBald();
			HairStyleMetadata hairStyleMetadata = GetHairStyleMetadata(whichHair);
			if (hairStyleMetadata != null)
			{
				hair.Set(whichHair);
			}
			else
			{
				if (whichHair < 0)
				{
					whichHair = GetLastHairStyle();
				}
				else if (whichHair > GetLastHairStyle())
				{
					whichHair = 0;
				}
				hair.Set(whichHair);
			}
			if (IsBaldHairStyle(whichHair))
			{
				FarmerRenderer.textureName.Set(getTexture());
			}
			if (flag && !isBald())
			{
				FarmerRenderer.textureName.Set(getTexture());
			}
		}

		public virtual bool IsBaldHairStyle(int style)
		{
			if (GetHairStyleMetadata(hair.Value) != null)
			{
				return GetHairStyleMetadata(hair.Value).isBaldStyle;
			}
			if ((uint)(style - 49) <= 6u)
			{
				return true;
			}
			return false;
		}

		private bool isBald()
		{
			return IsBaldHairStyle(getHair());
		}

		public void changeShoeColor(int which)
		{
			FarmerRenderer.recolorShoes(which);
			shoes.Set(which);
		}

		public void changeHairColor(Color c)
		{
			hairstyleColor.Set(c);
		}

		public void changePants(Color color)
		{
			pantsColor.Set(color);
		}

		public void changeHat(int newHat)
		{
			if (newHat < 0)
			{
				hat.Value = null;
			}
			else
			{
				hat.Value = new Hat(newHat);
			}
		}

		public void changeAccessory(int which)
		{
			if (which < -1)
			{
				which = 18;
			}
			if (which >= -1)
			{
				if (which >= 19)
				{
					which = -1;
				}
				accessory.Set(which);
			}
		}

		public void changeSkinColor(int which, bool force = false)
		{
			if (which < 0)
			{
				which = 23;
			}
			else if (which >= 24)
			{
				which = 0;
			}
			skin.Set(FarmerRenderer.recolorSkin(which, force));
		}

		public bool hasDarkSkin()
		{
			if (((int)skin >= 4 && (int)skin <= 8) || (int)skin == 14)
			{
				return true;
			}
			return false;
		}

		public void changeEyeColor(Color c)
		{
			newEyeColor.Set(c);
			FarmerRenderer.recolorEyes(c);
		}

		public int getHair(bool ignore_hat = false)
		{
			if (hat.Value != null && !bathingClothes && !ignore_hat)
			{
				switch (hat.Value.hairDrawType.Value)
				{
				case 2:
					return -1;
				case 1:
					switch ((int)hair)
					{
					case 50:
					case 51:
					case 52:
					case 53:
					case 54:
					case 55:
						return hair;
					case 48:
						return 6;
					case 49:
						return 52;
					case 3:
						return 11;
					case 1:
					case 5:
					case 6:
					case 9:
					case 11:
					case 17:
					case 20:
					case 23:
					case 24:
					case 25:
					case 27:
					case 28:
					case 29:
					case 30:
					case 32:
					case 33:
					case 34:
					case 36:
					case 39:
					case 41:
					case 43:
					case 44:
					case 45:
					case 46:
					case 47:
						return hair;
					case 18:
					case 19:
					case 21:
					case 31:
						return 23;
					case 42:
						return 46;
					default:
						if ((int)hair >= 16)
						{
							return 30;
						}
						return 7;
					}
				}
			}
			return hair;
		}

		public void changeGender(bool male)
		{
			if (male)
			{
				IsMale = true;
				FarmerRenderer.textureName.Set(getTexture());
				FarmerRenderer.heightOffset.Set(0);
			}
			else
			{
				IsMale = false;
				FarmerRenderer.heightOffset.Set(4);
				FarmerRenderer.textureName.Set(getTexture());
			}
			changeShirt(shirt);
		}

		public void changeFriendship(int amount, NPC n)
		{
			if (n == null || (!(n is Child) && !n.isVillager()) || (amount > 0 && n.Name.Equals("Dwarf") && !canUnderstandDwarves))
			{
				return;
			}
			if (friendshipData.ContainsKey(n.Name))
			{
				if (n.isDivorcedFrom(this) && amount > 0)
				{
					return;
				}
				friendshipData[n.Name].Points = Math.Max(0, Math.Min(friendshipData[n.Name].Points + amount, (Utility.GetMaximumHeartsForCharacter(n) + 1) * 250 - 1));
				if ((bool)n.datable && friendshipData[n.Name].Points >= 2000 && !hasOrWillReceiveMail("Bouquet"))
				{
					Game1.addMailForTomorrow("Bouquet");
				}
				if ((bool)n.datable && friendshipData[n.Name].Points >= 2500 && !hasOrWillReceiveMail("SeaAmulet"))
				{
					Game1.addMailForTomorrow("SeaAmulet");
				}
				if (friendshipData[n.Name].Points < 0)
				{
					friendshipData[n.Name].Points = 0;
				}
			}
			else
			{
				Game1.debugOutput = "Tried to change friendship for a friend that wasn't there.";
			}
			Game1.stats.checkForFriendshipAchievements();
		}

		public bool knowsRecipe(string name)
		{
			if (!craftingRecipes.Keys.Contains(name.Replace(" Recipe", "")))
			{
				return cookingRecipes.Keys.Contains(name.Replace(" Recipe", ""));
			}
			return true;
		}

		public Vector2 getUniformPositionAwayFromBox(int direction, int distance)
		{
			return FacingDirection switch
			{
				0 => new Vector2(GetBoundingBox().Center.X, GetBoundingBox().Y - distance), 
				1 => new Vector2(GetBoundingBox().Right + distance, GetBoundingBox().Center.Y), 
				2 => new Vector2(GetBoundingBox().Center.X, GetBoundingBox().Bottom + distance), 
				3 => new Vector2(GetBoundingBox().X - distance, GetBoundingBox().Center.Y), 
				_ => Vector2.Zero, 
			};
		}

		public bool hasTalkedToFriendToday(string npcName)
		{
			if (friendshipData.ContainsKey(npcName))
			{
				return friendshipData[npcName].TalkedToToday;
			}
			return false;
		}

		public void talkToFriend(NPC n, int friendshipPointChange = 20)
		{
			if (friendshipData.ContainsKey(n.Name) && !friendshipData[n.Name].TalkedToToday)
			{
				changeFriendship(friendshipPointChange, n);
				friendshipData[n.Name].TalkedToToday = true;
			}
		}

		public void moveRaft(GameLocation currentLocation, GameTime time)
		{
			float num = 0.2f;
			if (CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveUpButton))
			{
				yVelocity = Math.Max(yVelocity - num, -3f + Math.Abs(xVelocity) / 2f);
				faceDirection(0);
			}
			if (CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveRightButton))
			{
				xVelocity = Math.Min(xVelocity + num, 3f - Math.Abs(yVelocity) / 2f);
				faceDirection(1);
			}
			if (CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveDownButton))
			{
				yVelocity = Math.Min(yVelocity + num, 3f - Math.Abs(xVelocity) / 2f);
				faceDirection(2);
			}
			if (CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveLeftButton))
			{
				xVelocity = Math.Max(xVelocity - num, -3f + Math.Abs(yVelocity) / 2f);
				faceDirection(3);
			}
			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle((int)base.Position.X, (int)(base.Position.Y + 64f + 16f), 64, 64);
			rectangle.X += (int)Math.Ceiling(xVelocity);
			if (!currentLocation.isCollidingPosition(rectangle, Game1.viewport, isFarmer: true))
			{
				position.X += xVelocity;
			}
			rectangle.X -= (int)Math.Ceiling(xVelocity);
			rectangle.Y += (int)Math.Floor(yVelocity);
			if (!currentLocation.isCollidingPosition(rectangle, Game1.viewport, isFarmer: true))
			{
				position.Y += yVelocity;
			}
			if (xVelocity != 0f || yVelocity != 0f)
			{
				raftPuddleCounter -= time.ElapsedGameTime.Milliseconds;
				if (raftPuddleCounter <= 0)
				{
					raftPuddleCounter = 250;
					currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(xVelocity) + Math.Abs(yVelocity)) * 3f, 8, 0, new Vector2(rectangle.X, rectangle.Y - 64), flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
					if (Game1.random.NextDouble() < 0.6)
					{
						Game1.playSound("wateringCan");
					}
					if (Game1.random.NextDouble() < 0.6)
					{
						raftBobCounter /= 2;
					}
				}
			}
			raftBobCounter -= time.ElapsedGameTime.Milliseconds;
			if (raftBobCounter <= 0)
			{
				raftBobCounter = Game1.random.Next(15, 28) * 100;
				if (yOffset <= 0f)
				{
					yOffset = 4f;
					currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(xVelocity) + Math.Abs(yVelocity)) * 3f, 8, 0, new Vector2(rectangle.X, rectangle.Y - 64), flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
				}
				else
				{
					yOffset = 0f;
				}
			}
			if (xVelocity > 0f)
			{
				xVelocity = Math.Max(0f, xVelocity - num / 2f);
			}
			else if (xVelocity < 0f)
			{
				xVelocity = Math.Min(0f, xVelocity + num / 2f);
			}
			if (yVelocity > 0f)
			{
				yVelocity = Math.Max(0f, yVelocity - num / 2f);
			}
			else if (yVelocity < 0f)
			{
				yVelocity = Math.Min(0f, yVelocity + num / 2f);
			}
		}

		public void warpFarmer(Warp w, int warp_collide_direction)
		{
			if (w == null || Game1.eventUp)
			{
				return;
			}
			Halt();
			int targetX = w.TargetX;
			int targetY = w.TargetY;
			if (isRidingHorse())
			{
				if (warp_collide_direction == 3)
				{
					Game1.nextFarmerWarpOffsetX = -1;
				}
				if (warp_collide_direction == 0)
				{
					Game1.nextFarmerWarpOffsetY = -1;
				}
			}
			Game1.warpFarmer(w.TargetName, targetX, targetY, w.flipFarmer);
		}

		public void warpFarmer(Warp w)
		{
			warpFarmer(w, -1);
		}

		public void startToPassOut()
		{
			passOutEvent.Fire();
		}

		private void performPassOut()
		{
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			if (!swimming.Value && bathingClothes.Value)
			{
				bathingClothes.Value = false;
			}
			if (!passedOut && !FarmerSprite.isPassingOut())
			{
				faceDirection(2);
				completelyStopAnimatingOrDoingAction();
				animateOnce(293);
			}
		}

		public static void passOutFromTired(Farmer who)
		{
			if (!who.IsLocalPlayer)
			{
				return;
			}
			if (who.IsSitting())
			{
				who.StopSitting(animate: false);
			}
			if (who.isRidingHorse())
			{
				who.mount.dismount();
			}
			if (Game1.activeClickableMenu != null)
			{
				Game1.activeClickableMenu.emergencyShutDown();
				Game1.exitActiveMenu();
			}
			who.completelyStopAnimatingOrDoingAction();
			if ((bool)who.bathingClothes)
			{
				who.changeOutOfSwimSuit();
			}
			who.swimming.Value = false;
			who.CanMove = false;
			who.FarmerSprite.setCurrentSingleFrame(5, 3000);
			who.FarmerSprite.PauseForSingleAnimation = true;
			if (who == Game1.player && (FarmerTeam.SleepAnnounceModes)Game1.player.team.sleepAnnounceMode != FarmerTeam.SleepAnnounceModes.Off)
			{
				string text = "PassedOut";
				string text2 = "PassedOut_" + who.currentLocation.Name.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
				if (Game1.content.LoadStringReturnNullIfNotFound("Strings\\UI:Chat_" + text2) != null)
				{
					Game1.multiplayer.globalChatInfoMessage(text2, Game1.player.displayName);
				}
				else
				{
					int num = 0;
					for (int i = 0; i < 2; i++)
					{
						if (Game1.random.NextDouble() < 0.25)
						{
							num++;
						}
					}
					Game1.multiplayer.globalChatInfoMessage(text + num, Game1.player.displayName);
				}
			}
			if (Game1.currentLocation is FarmHouse)
			{
				who.lastSleepLocation.Value = Game1.currentLocation.NameOrUniqueName;
				who.lastSleepPoint.Value = (Game1.currentLocation as FarmHouse).GetPlayerBedSpot();
			}
			Game1.multiplayer.sendPassoutRequest();
		}

		public static void performPassoutWarp(Farmer who, string bed_location_name, Point bed_point, bool has_bed)
		{
			GameLocation passOutLocation = who.currentLocationRef.Value;
			Vector2 vector = Utility.PointToVector2(bed_point) * 64f;
			Vector2 bed_tile = new Vector2((int)vector.X / 64, (int)vector.Y / 64);
			Vector2 bed_sleep_position = vector;
			LocationRequest.Callback callback = delegate
			{
				who.Position = bed_sleep_position;
				who.currentLocation.lastTouchActionLocation = bed_tile;
				if (who.NetFields.Root != null)
				{
					(who.NetFields.Root as NetRoot<Farmer>).CancelInterpolation();
				}
				if (!Game1.IsMultiplayer || Game1.timeOfDay >= 2600)
				{
					Game1.PassOutNewDay();
				}
				Game1.changeMusicTrack("none");
				if (!(passOutLocation is FarmHouse) && !(passOutLocation is IslandFarmHouse) && !(passOutLocation is Cellar))
				{
					int num = Math.Min(1000, who.Money / 10);
					string text = "";
					Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + (int)who.UniqueMultiplayerID);
					if (passOutLocation is IslandLocation)
					{
						num = Math.Min(2500, who.Money);
						if (bed_location_name == "FarmHouse")
						{
							text = "passedOutIsland";
							if (Game1.player.friendshipData.ContainsKey("Leo") && Game1.random.NextDouble() < 0.5)
							{
								text = "passedOutIsland_Leo";
								num = 0;
							}
						}
					}
					else if (random.Next(0, 3) == 0 && Game1.MasterPlayer.hasCompletedCommunityCenter() && !Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
					{
						text = "passedOut4";
						num = 0;
					}
					else
					{
						Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
						List<int> list = new List<int>(new int[3] { 1, 2, 3 });
						if (who.getSpouse() != null && who.getSpouse().Name.Equals("Harvey"))
						{
							list.Remove(3);
						}
						if (Game1.MasterPlayer.hasCompletedCommunityCenter() && !Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
						{
							list.Remove(1);
						}
						int random2 = Utility.GetRandom(list, random);
						text = (dictionary.ContainsKey("passedOut" + random2 + "_" + ((num > 0) ? "Billed" : "NotBilled") + "_" + (who.IsMale ? "Male" : "Female")) ? ("passedOut" + random2 + "_" + ((num > 0) ? "Billed" : "NotBilled") + "_" + (who.IsMale ? "Male" : "Female") + " " + num) : (dictionary.ContainsKey("passedOut" + random2 + "_" + ((num > 0) ? "Billed" : "NotBilled")) ? ("passedOut" + random2 + "_" + ((num > 0) ? "Billed" : "NotBilled") + " " + num) : ((!dictionary.ContainsKey("passedOut" + random2)) ? ("passedOut2 " + num) : ("passedOut" + random2 + " " + num))));
					}
					if (num > 0)
					{
						who.Money -= num;
					}
					if (text != "")
					{
						who.mailForTomorrow.Add(text);
					}
				}
			};
			if (!who.isInBed)
			{
				LocationRequest locationRequest = Game1.getLocationRequest(bed_location_name);
				Game1.warpFarmer(locationRequest, (int)vector.X / 64, (int)vector.Y / 64, 2);
				locationRequest.OnWarp += callback;
				who.FarmerSprite.setCurrentSingleFrame(5, 3000);
				who.FarmerSprite.PauseForSingleAnimation = true;
			}
			else
			{
				callback();
			}
		}

		public static void doSleepEmote(Farmer who)
		{
			who.doEmote(24);
			who.yJumpVelocity = -2f;
		}

		public override Microsoft.Xna.Framework.Rectangle GetBoundingBox()
		{
			if (mount != null && !mount.dismounting)
			{
				return mount.GetBoundingBox();
			}
			Vector2 vector = base.Position;
			return new Microsoft.Xna.Framework.Rectangle((int)vector.X + 8, (int)vector.Y + Sprite.getHeight() - 32, 48, 32);
		}

		public string getPetName()
		{
			foreach (NPC character in Game1.getFarm().characters)
			{
				if (character is Pet)
				{
					return character.Name;
				}
			}
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				foreach (NPC character2 in Utility.getHomeOfFarmer(allFarmer).characters)
				{
					if (character2 is Pet)
					{
						return character2.Name;
					}
				}
			}
			return "the Farm";
		}

		public Pet getPet()
		{
			foreach (NPC character in Game1.getFarm().characters)
			{
				if (character is Pet)
				{
					return character as Pet;
				}
			}
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				foreach (NPC character2 in Utility.getHomeOfFarmer(allFarmer).characters)
				{
					if (character2 is Pet)
					{
						return character2 as Pet;
					}
				}
			}
			return null;
		}

		public string getPetDisplayName()
		{
			foreach (NPC character in Game1.getFarm().characters)
			{
				if (character is Pet)
				{
					return character.displayName;
				}
			}
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				foreach (NPC character2 in Utility.getHomeOfFarmer(allFarmer).characters)
				{
					if (character2 is Pet)
					{
						return character2.displayName;
					}
				}
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1972");
		}

		public bool hasPet()
		{
			foreach (NPC character in Game1.getFarm().characters)
			{
				if (character is Pet)
				{
					return true;
				}
			}
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				foreach (NPC character2 in Utility.getHomeOfFarmer(allFarmer).characters)
				{
					if (character2 is Pet)
					{
						return true;
					}
				}
			}
			return false;
		}

		public void UpdateClothing()
		{
			FarmerRenderer.MarkSpriteDirty();
		}

		public int GetPantsIndex()
		{
			if (pants.Value >= 0)
			{
				return pants.Value;
			}
			if (pantsItem.Value != null)
			{
				if ((bool)isMale || (int)pantsItem.Value.indexInTileSheetFemale < 0)
				{
					return pantsItem.Value.indexInTileSheetMale;
				}
				return pantsItem.Value.indexInTileSheetFemale;
			}
			return 14;
		}

		public int GetShirtIndex()
		{
			if (shirt.Value >= 0)
			{
				return shirt.Value;
			}
			if (shirtItem.Value != null)
			{
				if ((bool)isMale || (int)shirtItem.Value.indexInTileSheetFemale < 0)
				{
					return shirtItem.Value.indexInTileSheetMale;
				}
				return shirtItem.Value.indexInTileSheetFemale;
			}
			if (IsMale)
			{
				return 209;
			}
			return 41;
		}

		public List<string> GetShirtExtraData()
		{
			if (shirt.Value > 0)
			{
				return new Clothing(shirt.Value + 1000).GetOtherData();
			}
			if (shirtItem.Value != null)
			{
				return shirtItem.Value.GetOtherData();
			}
			return new List<string>();
		}

		public Color GetShirtColor()
		{
			if (shirt.Value >= 0)
			{
				return Color.White;
			}
			if (shirtItem.Value != null)
			{
				if ((bool)shirtItem.Value.isPrismatic)
				{
					return Utility.GetPrismaticColor();
				}
				return shirtItem.Value.clothesColor.Value;
			}
			return DEFAULT_SHIRT_COLOR;
		}

		public Color GetPantsColor()
		{
			if (pants.Value >= 0)
			{
				return pantsColor.Value;
			}
			if (pantsItem.Value != null)
			{
				if ((bool)pantsItem.Value.isPrismatic)
				{
					return Utility.GetPrismaticColor();
				}
				return pantsItem.Value.clothesColor.Value;
			}
			return Color.White;
		}

		public bool movedDuringLastTick()
		{
			return !base.Position.Equals(lastPosition);
		}

		public int CompareTo(object obj)
		{
			return ((Farmer)obj).saveTime - saveTime;
		}

		public virtual void SetOnBridge(bool val)
		{
			if (Game1.player.onBridge.Value != val)
			{
				Game1.player.onBridge.Value = val;
				if ((bool)Game1.player.onBridge)
				{
					Game1.player.showNotCarrying();
				}
			}
		}

		public float getDrawLayer()
		{
			if (onBridge.Value)
			{
				return (float)getStandingY() / 10000f + drawLayerDisambiguator + 0.0256f;
			}
			if (IsSitting() && mapChairSitPosition.Value.X != -1f && mapChairSitPosition.Value.Y != -1f)
			{
				return (mapChairSitPosition.Value.Y + 1f) * 64f / 10000f;
			}
			return (float)getStandingY() / 10000f + drawLayerDisambiguator;
		}

		public override void draw(SpriteBatch b)
		{
			if (base.currentLocation == null || (!base.currentLocation.Equals(Game1.currentLocation) && !IsLocalPlayer && !Game1.currentLocation.Name.Equals("Temp") && !isFakeEventActor) || ((bool)hidden && (base.currentLocation.currentEvent == null || this != base.currentLocation.currentEvent.farmer) && (!IsLocalPlayer || Game1.locationRequest == null)) || (viewingLocation.Value != null && IsLocalPlayer))
			{
				return;
			}
			if (isRidingHorse())
			{
				mount.SyncPositionToRider();
				mount.draw(b);
			}
			float drawLayer = getDrawLayer();
			Vector2 origin = new Vector2(xOffset, (yOffset + 128f - (float)(GetBoundingBox().Height / 2)) / 4f + 4f);
			numUpdatesSinceLastDraw = 0;
			PropertyValue value = null;
			Tile tile = Game1.currentLocation.Map.GetLayer("Buildings").PickTile(new Location(getStandingX(), getStandingY()), Game1.viewport.Size);
			if (isGlowing && coloredBorder)
			{
				b.Draw(Sprite.Texture, new Vector2(getLocalPosition(Game1.viewport).X - 4f, getLocalPosition(Game1.viewport).Y - 4f), Sprite.SourceRect, glowingColor * glowingTransparency, 0f, Vector2.Zero, 1.1f, SpriteEffects.None, Math.Max(0f, drawLayer - 0.001f));
			}
			else if (isGlowing && !coloredBorder)
			{
				FarmerRenderer.draw(b, FarmerSprite, FarmerSprite.SourceRect, getLocalPosition(Game1.viewport) + jitter + new Vector2(0f, yJumpOffset), origin, Math.Max(0f, drawLayer + 0.00011f), glowingColor * glowingTransparency, rotation, this);
			}
			tile?.TileIndexProperties.TryGetValue("Shadow", out value);
			if (value == null)
			{
				if (IsSitting() || !Game1.shouldTimePass() || !temporarilyInvincible || temporaryInvincibilityTimer % 100 < 50)
				{
					farmerRenderer.Value.draw(b, FarmerSprite, FarmerSprite.SourceRect, getLocalPosition(Game1.viewport) + jitter + new Vector2(0f, yJumpOffset), origin, Math.Max(0f, drawLayer + 0.0001f), Color.White, rotation, this);
				}
			}
			else
			{
				farmerRenderer.Value.draw(b, FarmerSprite, FarmerSprite.SourceRect, getLocalPosition(Game1.viewport), origin, Math.Max(0f, drawLayer + 0.0001f), Color.White, rotation, this);
				farmerRenderer.Value.draw(b, FarmerSprite, FarmerSprite.SourceRect, getLocalPosition(Game1.viewport), origin, Math.Max(0f, drawLayer + 0.0002f), Color.Black * 0.25f, rotation, this);
			}
			if (isRafting)
			{
				b.Draw(Game1.toolSpriteSheet, getLocalPosition(Game1.viewport) + new Vector2(0f, yOffset), Game1.getSourceRectForStandardTileSheet(Game1.toolSpriteSheet, 1), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, drawLayer - 0.001f);
			}
			if (Game1.activeClickableMenu == null && !Game1.eventUp && IsLocalPlayer && CurrentTool != null && (Game1.oldKBState.IsKeyDown(Keys.LeftShift) || Game1.options.alwaysShowToolHitLocation) && CurrentTool.doesShowTileLocationMarker() && (!Game1.options.hideToolHitLocationWhenInMotion || !isMoving()))
			{
				Vector2 target_position = Utility.PointToVector2(Game1.getMousePosition()) + new Vector2(Game1.viewport.X, Game1.viewport.Y);
				Vector2 vector = Game1.GlobalToLocal(Game1.viewport, Utility.clampToTile(GetToolLocation(target_position)));
				b.Draw(Game1.mouseCursors, vector, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 29), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, vector.Y / 10000f);
			}
			if (base.IsEmoting)
			{
				Vector2 localPosition = getLocalPosition(Game1.viewport);
				localPosition.Y -= 160f;
				b.Draw(Game1.emoteSpriteSheet, localPosition, new Microsoft.Xna.Framework.Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, drawLayer);
			}
			if (ActiveObject != null && IsCarrying())
			{
				Game1.drawPlayerHeldObject(this);
			}
			if (sparklingText != null)
			{
				sparklingText.draw(b, Game1.GlobalToLocal(Game1.viewport, base.Position + new Vector2(32f - sparklingText.textWidth / 2f, -128f)));
			}
			bool flag = IsLocalPlayer && Game1.pickingTool;
			if ((UsingTool || flag) && CurrentTool != null && (!CurrentTool.Name.Equals("Seeds") || flag))
			{
				Game1.drawTool(this);
			}
		}

		public virtual void DrawUsername(SpriteBatch b)
		{
			if (!Game1.IsMultiplayer || Game1.multiplayer == null || LocalMultiplayer.IsLocalMultiplayer(is_local_only: true) || usernameDisplayTime <= 0f)
			{
				return;
			}
			string text = null;
			text = Game1.multiplayer.getUserName(UniqueMultiplayerID);
			if (text == null)
			{
				return;
			}
			Vector2 vector = Game1.smallFont.MeasureString(text);
			Vector2 vector2 = getLocalPosition(Game1.viewport) + new Vector2(32f, -104f) - vector / 2f;
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					if (i != 0 || j != 0)
					{
						b.DrawString(Game1.smallFont, text, vector2 + new Vector2(i, j) * 2f, Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999f);
					}
				}
			}
			b.DrawString(Game1.smallFont, text, vector2, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
		}

		public static void drinkGlug(Farmer who)
		{
			Color color = Color.LightBlue;
			if (who.itemToEat != null)
			{
				string text = who.itemToEat.Name.Split(' ').Last();
				if (text != null)
				{
					switch (text.Length)
					{
					case 5:
					{
						char c = text[0];
						if (c != 'J')
						{
							if (c == 'T' && text == "Tonic")
							{
								color = Color.Red;
							}
							break;
						}
						if (!(text == "Juice"))
						{
							break;
						}
						goto IL_019c;
					}
					case 6:
					{
						char c = text[0];
						if (c != 'C')
						{
							if (c == 'R' && text == "Remedy")
							{
								color = Color.LimeGreen;
							}
							break;
						}
						if (!(text == "Coffee"))
						{
							break;
						}
						goto IL_0176;
					}
					case 4:
					{
						char c = text[0];
						if ((uint)c <= 67u)
						{
							if (c != 'B')
							{
								if (c != 'C' || !(text == "Cola"))
								{
									break;
								}
								goto IL_0176;
							}
							if (text == "Beer")
							{
								color = Color.Orange;
							}
							break;
						}
						switch (c)
						{
						case 'W':
							if (text == "Wine")
							{
								color = Color.Purple;
							}
							break;
						case 'M':
							if (text == "Milk")
							{
								color = Color.White;
							}
							break;
						}
						break;
					}
					case 8:
						if (!(text == "Espresso"))
						{
							break;
						}
						goto IL_0176;
					case 3:
						{
							if (!(text == "Tea"))
							{
								break;
							}
							goto IL_019c;
						}
						IL_019c:
						color = Color.LightGreen;
						break;
						IL_0176:
						color = new Color(46, 20, 0);
						break;
					}
				}
			}
			if (Game1.currentLocation == who.currentLocation)
			{
				Game1.playSound("gulp");
			}
			who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(653, 858, 1, 1), 9999f, 1, 1, who.Position + new Vector2(32 + Game1.random.Next(-2, 3) * 4, -48f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.001f, 0.04f, color, 5f, 0f, 0f, 0f)
			{
				acceleration = new Vector2(0f, 0.5f)
			});
		}

		public void handleDisconnect()
		{
			if (base.currentLocation != null)
			{
				if (rightRing.Value != null)
				{
					rightRing.Value.onLeaveLocation(this, base.currentLocation);
				}
				if (leftRing.Value != null)
				{
					leftRing.Value.onLeaveLocation(this, base.currentLocation);
				}
			}
		}

		public bool isDivorced()
		{
			foreach (Friendship value in friendshipData.Values)
			{
				if (value.IsDivorced())
				{
					return true;
				}
			}
			return false;
		}

		public void wipeExMemories()
		{
			foreach (string key in friendshipData.Keys)
			{
				Friendship friendship = friendshipData[key];
				if (friendship.IsDivorced())
				{
					friendship.Clear();
					NPC characterFromName = Game1.getCharacterFromName(key);
					if (characterFromName != null)
					{
						characterFromName.CurrentDialogue.Clear();
						characterFromName.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:WipedMemory"), characterFromName));
						Game1.stats.incrementStat("exMemoriesWiped", 1);
					}
				}
			}
		}

		public void getRidOfChildren()
		{
			FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(this);
			List<Child> list = new List<Child>();
			for (int num = homeOfFarmer.characters.Count - 1; num >= 0; num--)
			{
				if (homeOfFarmer.characters[num] is Child)
				{
					homeOfFarmer.GetChildBed(homeOfFarmer.characters[num].Gender)?.mutex.ReleaseLock();
					if ((homeOfFarmer.characters[num] as Child).hat.Value != null)
					{
						Hat value = (homeOfFarmer.characters[num] as Child).hat.Value;
						(homeOfFarmer.characters[num] as Child).hat.Value = null;
						Game1.player.team.returnedDonations.Add(value);
						Game1.player.team.newLostAndFoundItems.Value = true;
					}
					homeOfFarmer.characters.RemoveAt(num);
					Game1.stats.incrementStat("childrenTurnedToDoves", 1);
				}
			}
		}

		public void animateOnce(int whichAnimation)
		{
			FarmerSprite.animateOnce(whichAnimation, 100f, 6);
			CanMove = false;
		}

		public static void showItemIntake(Farmer who)
		{
			TemporaryAnimatedSprite temporaryAnimatedSprite = null;
			Object @object = ((who.mostRecentlyGrabbedItem != null && who.mostRecentlyGrabbedItem is Object) ? ((Object)who.mostRecentlyGrabbedItem) : ((who.ActiveObject == null) ? null : who.ActiveObject));
			if (@object == null)
			{
				return;
			}
			switch (who.FacingDirection)
			{
			case 2:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 1:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(0f, -32f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 2:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(0f, -43f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 3:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 4:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 200f, 1, 0, who.Position + new Vector2(0f, -120f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 5:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 200f, 1, 0, who.Position + new Vector2(0f, -120f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f);
					break;
				}
				break;
			case 1:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 1:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(28f, -64f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 2:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(24f, -72f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 3:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(4f, -128f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 4:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 200f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 5:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 200f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f);
					break;
				}
				break;
			case 0:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 1:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(0f, -32f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 2:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(0f, -43f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 3:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 4:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 200f, 1, 0, who.Position + new Vector2(0f, -120f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 5:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 200f, 1, 0, who.Position + new Vector2(0f, -120f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f - 0.001f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f);
					break;
				}
				break;
			case 3:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 1:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(-32f, -64f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 2:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(-28f, -76f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 3:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 100f, 1, 0, who.Position + new Vector2(-16f, -128f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 4:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 200f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
					break;
				case 5:
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, @object.parentSheetIndex, 16, 16), 200f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, (float)who.getStandingY() / 10000f + 0.01f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f);
					break;
				}
				break;
			}
			if ((@object.Equals(who.ActiveObject) || (who.ActiveObject != null && @object != null && @object.ParentSheetIndex == (int)who.ActiveObject.parentSheetIndex)) && who.FarmerSprite.currentAnimationIndex == 5)
			{
				temporaryAnimatedSprite = null;
			}
			if (temporaryAnimatedSprite != null)
			{
				who.currentLocation.temporarySprites.Add(temporaryAnimatedSprite);
			}
			if (who.mostRecentlyGrabbedItem is ColoredObject && temporaryAnimatedSprite != null)
			{
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (int)@object.parentSheetIndex + 1, 16, 16), temporaryAnimatedSprite.interval, 1, 0, temporaryAnimatedSprite.Position, flicker: false, flipped: false, temporaryAnimatedSprite.layerDepth + 0.0001f, temporaryAnimatedSprite.alphaFade, (who.mostRecentlyGrabbedItem as ColoredObject).color, 4f, temporaryAnimatedSprite.scaleChange, 0f, 0f));
			}
			if (who.FarmerSprite.currentAnimationIndex == 5)
			{
				who.Halt();
				who.FarmerSprite.CurrentAnimation = null;
			}
		}

		public static void showSwordSwipe(Farmer who)
		{
			TemporaryAnimatedSprite temporaryAnimatedSprite = null;
			bool flag = who.CurrentTool != null && who.CurrentTool is MeleeWeapon && (int)(who.CurrentTool as MeleeWeapon).type == 1;
			Vector2 toolLocation = who.GetToolLocation(ignoreClick: true);
			if (who.CurrentTool != null && who.CurrentTool is MeleeWeapon && !flag)
			{
				(who.CurrentTool as MeleeWeapon).DoDamage(who.currentLocation, (int)toolLocation.X, (int)toolLocation.Y, who.FacingDirection, 1, who);
			}
			int val = 20;
			switch (who.FacingDirection)
			{
			case 2:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 0:
					if (flag)
					{
						who.yVelocity = -0.6f;
					}
					break;
				case 1:
					who.yVelocity = (flag ? 0.5f : (-0.5f));
					break;
				case 5:
					who.yVelocity = 0.3f;
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(503, 256, 42, 17), who.Position + new Vector2(-16f, -2f) * 4f, flipped: false, 0.07f, Color.White)
					{
						scale = 4f,
						animationLength = 1,
						interval = Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, val),
						alpha = 0.5f,
						layerDepth = (who.Position.Y + 64f) / 10000f
					};
					break;
				}
				break;
			case 1:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 0:
					if (flag)
					{
						who.xVelocity = 0.6f;
					}
					break;
				case 1:
					who.xVelocity = (flag ? (-0.5f) : 0.5f);
					break;
				case 5:
					who.xVelocity = -0.3f;
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(518, 274, 23, 31), who.Position + new Vector2(4f, -12f) * 4f, flipped: false, 0.07f, Color.White)
					{
						scale = 4f,
						animationLength = 1,
						interval = Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, val),
						alpha = 0.5f
					};
					break;
				}
				break;
			case 3:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 0:
					if (flag)
					{
						who.xVelocity = -0.6f;
					}
					break;
				case 1:
					who.xVelocity = (flag ? 0.5f : (-0.5f));
					break;
				case 5:
					who.xVelocity = 0.3f;
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(518, 274, 23, 31), who.Position + new Vector2(-15f, -12f) * 4f, flipped: false, 0.07f, Color.White)
					{
						scale = 4f,
						animationLength = 1,
						interval = Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, val),
						flipped = true,
						alpha = 0.5f
					};
					break;
				}
				break;
			case 0:
				switch (who.FarmerSprite.currentAnimationIndex)
				{
				case 0:
					if (flag)
					{
						who.yVelocity = 0.6f;
					}
					break;
				case 1:
					who.yVelocity = (flag ? (-0.5f) : 0.5f);
					break;
				case 5:
					who.yVelocity = -0.3f;
					temporaryAnimatedSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(518, 274, 23, 31), who.Position + new Vector2(0f, -32f) * 4f, flipped: false, 0.07f, Color.White)
					{
						scale = 4f,
						animationLength = 1,
						interval = Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, val),
						alpha = 0.5f,
						rotation = 3.926991f
					};
					break;
				}
				break;
			}
			if (temporaryAnimatedSprite != null)
			{
				if (who.CurrentTool != null && who.CurrentTool is MeleeWeapon && who.CurrentTool.InitialParentTileIndex == 4)
				{
					temporaryAnimatedSprite.color = Color.HotPink;
				}
				who.currentLocation.temporarySprites.Add(temporaryAnimatedSprite);
			}
		}

		public static void showToolSwipeEffect(Farmer who)
		{
			if (who.CurrentTool != null && who.CurrentTool is WateringCan)
			{
				int num = who.FacingDirection;
				return;
			}
			switch (who.FacingDirection)
			{
			case 1:
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(15, who.Position + new Vector2(20f, -132f), Color.White, 4, flipped: false, (who.stamina <= 0f) ? 80f : 40f, 0, 128, 1f, 128)
				{
					layerDepth = (float)(who.GetBoundingBox().Bottom + 1) / 10000f
				});
				break;
			case 3:
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(15, who.Position + new Vector2(-92f, -132f), Color.White, 4, flipped: true, (who.stamina <= 0f) ? 80f : 40f, 0, 128, 1f, 128)
				{
					layerDepth = (float)(who.GetBoundingBox().Bottom + 1) / 10000f
				});
				break;
			case 2:
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(19, who.Position + new Vector2(-4f, -128f), Color.White, 4, flipped: false, (who.stamina <= 0f) ? 80f : 40f, 0, 128, 1f, 128)
				{
					layerDepth = (float)(who.GetBoundingBox().Bottom + 1) / 10000f
				});
				break;
			case 0:
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(18, who.Position + new Vector2(0f, -132f), Color.White, 4, flipped: false, (who.stamina <= 0f) ? 100f : 50f, 0, 64, 1f, 64)
				{
					layerDepth = (float)(who.getStandingY() - 9) / 10000f
				});
				break;
			}
		}

		public static void canMoveNow(Farmer who)
		{
			who.CanMove = true;
			who.UsingTool = false;
			who.usingSlingshot = false;
			who.FarmerSprite.PauseForSingleAnimation = false;
			who.yVelocity = 0f;
			who.xVelocity = 0f;
		}

		public void FireTool()
		{
			fireToolEvent.Fire();
		}

		public void synchronizedJump(float velocity)
		{
			if (IsLocalPlayer)
			{
				synchronizedJumpEvent.Fire(velocity);
				synchronizedJumpEvent.Poll();
			}
		}

		protected void performSynchronizedJump(float velocity)
		{
			yJumpVelocity = velocity;
			yJumpOffset = -1;
		}

		private void performFireTool()
		{
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			if (CurrentTool != null)
			{
				CurrentTool.leftClick(this);
			}
		}

		public static void useTool(Farmer who)
		{
			if (who.toolOverrideFunction != null)
			{
				who.toolOverrideFunction(who);
			}
			else if (who.CurrentTool != null)
			{
				float oldStamina = who.stamina;
				if (who.IsLocalPlayer)
				{
					who.CurrentTool.DoFunction(who.currentLocation, (int)who.GetToolLocation().X, (int)who.GetToolLocation().Y, 1, who);
				}
				who.lastClick = Vector2.Zero;
				who.checkForExhaustion(oldStamina);
				Game1.toolHold = 0f;
			}
		}

		public void BeginUsingTool()
		{
			beginUsingToolEvent.Fire();
		}

		private void performBeginUsingTool()
		{
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			if (CurrentTool != null)
			{
				FarmerSprite.setOwner(this);
				CanMove = false;
				UsingTool = true;
				canReleaseTool = true;
				CurrentTool.beginUsing(base.currentLocation, (int)lastClick.X, (int)lastClick.Y, this);
			}
		}

		public void EndUsingTool()
		{
			if (this == Game1.player)
			{
				endUsingToolEvent.Fire();
			}
			else
			{
				performEndUsingTool();
			}
		}

		private void performEndUsingTool()
		{
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			if (CurrentTool != null)
			{
				CurrentTool.endUsing(base.currentLocation, this);
			}
		}

		public void checkForExhaustion(float oldStamina)
		{
			if (stamina <= 0f && oldStamina > 0f)
			{
				if (!exhausted && IsLocalPlayer)
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1986"));
				}
				setRunning(isRunning: false);
				doEmote(36);
			}
			else if (stamina <= 15f && oldStamina > 15f && IsLocalPlayer)
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1987"));
			}
			if (stamina <= 0f)
			{
				exhausted.Value = true;
			}
		}

		public void setMoving(byte command)
		{
			bool flag = false;
			if (movementDirections.Count < 2)
			{
				if (command == 1 && !movementDirections.Contains(0) && !movementDirections.Contains(2))
				{
					movementDirections.Insert(0, 0);
					flag = true;
				}
				if (command == 2 && !movementDirections.Contains(1) && !movementDirections.Contains(3))
				{
					movementDirections.Insert(0, 1);
					flag = true;
				}
				if (command == 4 && !movementDirections.Contains(2) && !movementDirections.Contains(0))
				{
					movementDirections.Insert(0, 2);
					flag = true;
				}
				if (command == 8 && !movementDirections.Contains(3) && !movementDirections.Contains(1))
				{
					movementDirections.Insert(0, 3);
					flag = true;
				}
			}
			if (command == 33)
			{
				movementDirections.Remove(0);
				flag = true;
			}
			if (command == 34)
			{
				movementDirections.Remove(1);
				flag = true;
			}
			if (command == 36)
			{
				movementDirections.Remove(2);
				flag = true;
			}
			if (command == 40)
			{
				movementDirections.Remove(3);
				flag = true;
			}
			switch (command)
			{
			case 16:
				setRunning(isRunning: true);
				flag = true;
				break;
			case 48:
				setRunning(isRunning: false);
				flag = true;
				break;
			}
			if ((command & 0x40) == 64)
			{
				Halt();
				running = false;
				flag = true;
			}
		}

		public void toolPowerIncrease()
		{
			if (toolPower == 0)
			{
				toolPitchAccumulator = 0;
			}
			toolPower++;
			if (CurrentTool is Pickaxe && toolPower == 1)
			{
				toolPower += 2;
			}
			Color color = Color.White;
			int num = ((FacingDirection == 0) ? 4 : ((FacingDirection == 2) ? 2 : 0));
			switch (toolPower)
			{
			case 1:
				color = Color.Orange;
				if (!(CurrentTool is WateringCan))
				{
					FarmerSprite.CurrentFrame = 72 + num;
				}
				jitterStrength = 0.25f;
				break;
			case 2:
				color = Color.LightSteelBlue;
				if (!(CurrentTool is WateringCan))
				{
					FarmerSprite.CurrentFrame++;
				}
				jitterStrength = 0.5f;
				break;
			case 3:
				color = Color.Gold;
				jitterStrength = 1f;
				break;
			case 4:
				color = Color.Violet;
				jitterStrength = 2f;
				break;
			case 5:
				color = Color.BlueViolet;
				jitterStrength = 3f;
				break;
			}
			int num2 = ((FacingDirection == 1) ? 40 : ((FacingDirection == 3) ? (-40) : ((FacingDirection == 2) ? 32 : 0)));
			int num3 = 192;
			if (CurrentTool is WateringCan)
			{
				if ((int)facingDirection == 3)
				{
					num2 = 48;
				}
				else if ((int)facingDirection == 1)
				{
					num2 = -48;
				}
				num3 = 128;
				if (FacingDirection == 2)
				{
					num2 = 0;
				}
			}
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(21, base.Position - new Vector2(num2, num3), color, 8, flipped: false, 70f, 0, 64, (float)getStandingY() / 10000f + 0.005f, 128));
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(192, 1152, 64, 64), 50f, 4, 0, base.Position - new Vector2((FacingDirection != 1) ? (-64) : 0, 128f), flicker: false, FacingDirection == 1, (float)getStandingY() / 10000f, 0.01f, Color.White, 1f, 0f, 0f, 0f));
			if (Game1.soundBank != null)
			{
				ICue cue = Game1.soundBank.GetCue("toolCharge");
				Random random = new Random(Game1.dayOfMonth + (int)base.Position.X * 1000 + (int)base.Position.Y);
				cue.SetVariable("Pitch", random.Next(12, 16) * 100 + toolPower * 100);
				cue.Play();
			}
		}

		public void UpdateIfOtherPlayer(GameTime time)
		{
			if (base.currentLocation == null)
			{
				return;
			}
			position.UpdateExtrapolation(getMovementSpeed());
			position.Field.InterpolationEnabled = !currentLocationRef.IsChanging();
			if (Game1.ShouldShowOnscreenUsernames() && Game1.mouseCursorTransparency > 0f && base.currentLocation == Game1.currentLocation && Game1.currentMinigame == null && Game1.activeClickableMenu == null)
			{
				Vector2 localPosition = getLocalPosition(Game1.viewport);
				Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, 128, 192);
				rectangle.X = (int)(localPosition.X + 32f - (float)(rectangle.Width / 2));
				rectangle.Y = (int)(localPosition.Y - (float)rectangle.Height + 48f);
				if (rectangle.Contains(Game1.getMouseX(ui_scale: false), Game1.getMouseY(ui_scale: false)))
				{
					usernameDisplayTime = 1f;
				}
			}
			if (_lastSelectedItem != CurrentItem)
			{
				if (_lastSelectedItem != null)
				{
					_lastSelectedItem.actionWhenStopBeingHeld(this);
				}
				_lastSelectedItem = CurrentItem;
			}
			FarmerSprite.setOwner(this);
			fireToolEvent.Poll();
			beginUsingToolEvent.Poll();
			endUsingToolEvent.Poll();
			drinkAnimationEvent.Poll();
			eatAnimationEvent.Poll();
			sickAnimationEvent.Poll();
			passOutEvent.Poll();
			doEmoteEvent.Poll();
			kissFarmerEvent.Poll();
			haltAnimationEvent.Poll();
			synchronizedJumpEvent.Poll();
			renovateEvent.Poll();
			FarmerSprite.checkForSingleAnimation(time);
			updateCommon(time, base.currentLocation);
		}

		public void forceCanMove()
		{
			forceTimePass = false;
			movementDirections.Clear();
			isEating = false;
			CanMove = true;
			Game1.freezeControls = false;
			freezePause = 0;
			UsingTool = false;
			usingSlingshot = false;
			FarmerSprite.PauseForSingleAnimation = false;
			if (CurrentTool is FishingRod)
			{
				(CurrentTool as FishingRod).isFishing = false;
			}
		}

		public void dropItem(Item i)
		{
			if (i != null && i.canBeDropped())
			{
				Game1.createItemDebris(i.getOne(), getStandingPosition(), FacingDirection);
			}
		}

		public bool addEvent(string eventName, int daysActive)
		{
			if (!activeDialogueEvents.ContainsKey(eventName))
			{
				activeDialogueEvents.Add(eventName, daysActive);
				return true;
			}
			return false;
		}

		public void dropObjectFromInventory(int parentSheetIndex, int quantity)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] == null || !(items[i] is Object) || (int)(items[i] as Object).parentSheetIndex != parentSheetIndex)
				{
					continue;
				}
				while (quantity > 0)
				{
					dropItem(items[i].getOne());
					items[i].Stack--;
					quantity--;
					if (items[i].Stack <= 0)
					{
						items[i] = null;
						break;
					}
				}
				if (quantity <= 0)
				{
					break;
				}
			}
		}

		public Vector2 getMostRecentMovementVector()
		{
			return new Vector2(base.Position.X - lastPosition.X, base.Position.Y - lastPosition.Y);
		}

		public void dropActiveItem()
		{
			if (CurrentItem != null && CurrentItem.canBeDropped())
			{
				Game1.createItemDebris(CurrentItem.getOne(), getStandingPosition(), FacingDirection);
				reduceActiveItemByOne();
			}
		}

		public int GetSkillLevel(int index)
		{
			return index switch
			{
				0 => FarmingLevel, 
				3 => MiningLevel, 
				1 => FishingLevel, 
				2 => ForagingLevel, 
				5 => LuckLevel, 
				4 => CombatLevel, 
				_ => 0, 
			};
		}

		public int GetUnmodifiedSkillLevel(int index)
		{
			return index switch
			{
				0 => farmingLevel.Value, 
				3 => miningLevel.Value, 
				1 => fishingLevel.Value, 
				2 => foragingLevel.Value, 
				5 => luckLevel.Value, 
				4 => combatLevel.Value, 
				_ => 0, 
			};
		}

		public static string getSkillNameFromIndex(int index)
		{
			return index switch
			{
				0 => "Farming", 
				3 => "Mining", 
				1 => "Fishing", 
				2 => "Foraging", 
				5 => "Luck", 
				4 => "Combat", 
				_ => "", 
			};
		}

		public static string getSkillDisplayNameFromIndex(int index)
		{
			return index switch
			{
				0 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1991"), 
				3 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1992"), 
				1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1993"), 
				2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1994"), 
				5 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1995"), 
				4 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1996"), 
				_ => "", 
			};
		}

		public bool hasCompletedCommunityCenter()
		{
			if (mailReceived.Contains("ccBoilerRoom") && mailReceived.Contains("ccCraftsRoom") && mailReceived.Contains("ccPantry") && mailReceived.Contains("ccFishTank") && mailReceived.Contains("ccVault"))
			{
				return mailReceived.Contains("ccBulletin");
			}
			return false;
		}

		private bool localBusMoving()
		{
			if (base.currentLocation is Desert)
			{
				Desert desert = base.currentLocation as Desert;
				if (!desert.drivingOff)
				{
					return desert.drivingBack;
				}
				return true;
			}
			if (base.currentLocation is BusStop)
			{
				BusStop busStop = base.currentLocation as BusStop;
				if (!busStop.drivingOff)
				{
					return busStop.drivingBack;
				}
				return true;
			}
			return false;
		}

		public virtual bool CanBeDamaged()
		{
			if (!temporarilyInvincible && !Game1.player.isEating && !Game1.fadeToBlack)
			{
				return !Game1.buffsDisplay.hasBuff(21);
			}
			return false;
		}

		public void takeDamage(int damage, bool overrideParry, Monster damager)
		{
			if (Game1.eventUp || FarmerSprite.isPassingOut())
			{
				return;
			}
			bool flag = damager != null && !damager.isInvincible() && !overrideParry;
			bool flag2 = (damager == null || !damager.isInvincible()) && (damager == null || (!(damager is GreenSlime) && !(damager is BigSlime)) || !isWearingRing(520));
			bool flag3 = CurrentTool != null && CurrentTool is MeleeWeapon && ((MeleeWeapon)CurrentTool).isOnSpecial && (int)((MeleeWeapon)CurrentTool).type == 3;
			bool flag4 = CanBeDamaged();
			if (flag && flag3)
			{
				Rumble.rumble(0.75f, 150f);
				base.currentLocation.playSound("parry");
				damager.parried(damage, this);
			}
			else
			{
				if (!(flag2 && flag4))
				{
					return;
				}
				damager?.onDealContactDamage(this);
				damage += Game1.random.Next(Math.Min(-1, -damage / 8), Math.Max(1, damage / 8));
				int num = resilience;
				if (CurrentTool is MeleeWeapon)
				{
					num += (int)(CurrentTool as MeleeWeapon).addedDefense;
				}
				if ((float)num >= (float)damage * 0.5f)
				{
					num -= (int)((float)num * (float)Game1.random.Next(3) / 10f);
				}
				if (damager != null && isWearingRing(839))
				{
					Microsoft.Xna.Framework.Rectangle boundingBox = damager.GetBoundingBox();
					Vector2 awayFromPlayerTrajectory = Utility.getAwayFromPlayerTrajectory(boundingBox, this);
					awayFromPlayerTrajectory /= 2f;
					int num2 = damage;
					int num3 = Math.Max(1, damage - num);
					if (num3 < 10)
					{
						num2 = (int)Math.Ceiling((double)(num2 + num3) / 2.0);
					}
					damager.takeDamage(num2, (int)awayFromPlayerTrajectory.X, (int)awayFromPlayerTrajectory.Y, isBomb: false, 1.0, this);
					damager.currentLocation.debris.Add(new Debris(num2, new Vector2(boundingBox.Center.X + 16, boundingBox.Center.Y), new Color(255, 130, 0), 1f, damager));
				}
				if (isWearingRing(524) && !Game1.buffsDisplay.hasBuff(21) && Game1.random.NextDouble() < (0.9 - (double)((float)health / 100f)) / (double)(3 - LuckLevel / 10) + ((health <= 15) ? 0.2 : 0.0))
				{
					base.currentLocation.playSound("yoba");
					Game1.buffsDisplay.addOtherBuff(new Buff(21));
					return;
				}
				Rumble.rumble(0.75f, 150f);
				damage = Math.Max(1, damage - num);
				health = Math.Max(0, health - damage);
				if (health <= 0 && GetEffectsOfRingMultiplier(863) > 0 && !hasUsedDailyRevive.Value)
				{
					Game1.player.startGlowing(new Color(255, 255, 0), border: false, 0.25f);
					DelayedAction.functionAfterDelay(delegate
					{
						stopGlowing();
					}, 500);
					Game1.playSound("yoba");
					for (int i = 0; i < 13; i++)
					{
						float num4 = Game1.random.Next(-32, 33);
						base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(114, 46, 2, 2), 200f, 5, 1, new Vector2(num4 + 32f, -96f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							attachedCharacter = this,
							positionFollowsAttachedCharacter = true,
							motion = new Vector2(num4 / 32f, -3f),
							delayBeforeAnimationStart = i * 50,
							alphaFade = 0.001f,
							acceleration = new Vector2(0f, 0.1f)
						});
					}
					base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(157, 280, 28, 19), 2000f, 1, 1, new Vector2(-20f, -16f), flicker: false, flipped: false, 1E-06f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						attachedCharacter = this,
						positionFollowsAttachedCharacter = true,
						alpha = 0.1f,
						alphaFade = -0.01f,
						alphaFadeFade = -0.00025f
					});
					health = (int)Math.Min(maxHealth, (float)maxHealth * 0.5f + (float)GetEffectsOfRingMultiplier(863));
					hasUsedDailyRevive.Value = true;
				}
				temporarilyInvincible = true;
				temporaryInvincibilityTimer = 0;
				currentTemporaryInvincibilityDuration = 1200 + GetEffectsOfRingMultiplier(861) * 400;
				base.currentLocation.debris.Add(new Debris(damage, new Vector2(getStandingX() + 8, getStandingY()), Color.Red, 1f, this));
				base.currentLocation.playSound("ow");
				Game1.hitShakeTimer = 100 * damage;
			}
		}

		public int GetEffectsOfRingMultiplier(int ring_index)
		{
			int num = 0;
			if (leftRing.Value != null)
			{
				num += leftRing.Value.GetEffectsOfRingMultiplier(ring_index);
			}
			if (rightRing.Value != null)
			{
				num += rightRing.Value.GetEffectsOfRingMultiplier(ring_index);
			}
			return num;
		}

		private void checkDamage(GameLocation location)
		{
			if (Game1.eventUp)
			{
				return;
			}
			for (int num = location.characters.Count - 1; num >= 0; num--)
			{
				if (num < location.characters.Count)
				{
					NPC nPC = location.characters[num];
					if (nPC is Monster)
					{
						Monster monster = nPC as Monster;
						if (monster.OverlapsFarmerForDamage(this))
						{
							monster.currentLocation = location;
							monster.collisionWithFarmerBehavior();
							if (monster.DamageToFarmer > 0)
							{
								bool flag = false;
								if (CurrentTool != null && CurrentTool is MeleeWeapon && ((MeleeWeapon)CurrentTool).isOnSpecial && (int)((MeleeWeapon)CurrentTool).type == 3)
								{
									takeDamage(monster.DamageToFarmer, overrideParry: false, nPC as Monster);
								}
								else
								{
									takeDamage(Math.Max(1, monster.DamageToFarmer + Game1.random.Next(-monster.DamageToFarmer / 4, monster.DamageToFarmer / 4)), overrideParry: false, nPC as Monster);
								}
							}
						}
					}
				}
			}
		}

		public bool checkAction(Farmer who, GameLocation location)
		{
			if (who.isRidingHorse())
			{
				who.Halt();
			}
			if ((bool)hidden)
			{
				return false;
			}
			if (Game1.CurrentEvent != null)
			{
				if (Game1.CurrentEvent.isSpecificFestival("spring24") && who.dancePartner.Value == null)
				{
					who.Halt();
					who.faceGeneralDirection(getStandingPosition(), 0, opposite: false, useTileCalculations: false);
					string question = Game1.content.LoadString("Strings\\UI:AskToDance_" + (IsMale ? "Male" : "Female"), base.Name);
					location.createQuestionDialogue(question, location.createYesNoResponses(), delegate(Farmer _, string answer)
					{
						if (answer == "Yes")
						{
							who.team.SendProposal(this, ProposalType.Dance);
							Game1.activeClickableMenu = new PendingProposalDialog();
						}
					});
					return true;
				}
				return false;
			}
			if (who.CurrentItem != null && (int)who.CurrentItem.parentSheetIndex == 801 && !isMarried() && !isEngaged() && !who.isMarried() && !who.isEngaged())
			{
				who.Halt();
				who.faceGeneralDirection(getStandingPosition(), 0, opposite: false, useTileCalculations: false);
				string question2 = Game1.content.LoadString("Strings\\UI:AskToMarry_" + (IsMale ? "Male" : "Female"), base.Name);
				location.createQuestionDialogue(question2, location.createYesNoResponses(), delegate(Farmer _, string answer)
				{
					if (answer == "Yes")
					{
						who.team.SendProposal(this, ProposalType.Marriage, who.CurrentItem.getOne());
						Game1.activeClickableMenu = new PendingProposalDialog();
					}
				});
				return true;
			}
			if (who.CanMove && who.ActiveObject != null && who.ActiveObject.canBeGivenAsGift() && !who.ActiveObject.questItem)
			{
				who.Halt();
				who.faceGeneralDirection(getStandingPosition(), 0, opposite: false, useTileCalculations: false);
				string question3 = Game1.content.LoadString("Strings\\UI:GiftPlayerItem_" + (IsMale ? "Male" : "Female"), who.ActiveObject.DisplayName, base.Name);
				location.createQuestionDialogue(question3, location.createYesNoResponses(), delegate(Farmer _, string answer)
				{
					if (answer == "Yes")
					{
						who.team.SendProposal(this, ProposalType.Gift, who.ActiveObject.getOne());
						Game1.activeClickableMenu = new PendingProposalDialog();
					}
				});
				return true;
			}
			long? num = team.GetSpouse(UniqueMultiplayerID);
			if ((num.HasValue & (who.UniqueMultiplayerID == num)) && who.CanMove && !who.isMoving() && !isMoving())
			{
				int generalDirectionTowards = getGeneralDirectionTowards(who.getStandingPosition(), -10, opposite: false, useTileCalculations: false);
				if (Utility.IsHorizontalDirection(generalDirectionTowards))
				{
					who.Halt();
					who.faceGeneralDirection(getStandingPosition(), 0, opposite: false, useTileCalculations: false);
					who.kissFarmerEvent.Fire(UniqueMultiplayerID);
					Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(211, 428, 7, 6), 2000f, 1, 0, new Vector2(getTileX(), getTileY()) * 64f + new Vector2(16f, -64f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(0f, -0.5f),
						alphaFade = 0.01f
					});
					base.currentLocation.playSound("dwop", NetAudio.SoundContext.NPC);
					return true;
				}
			}
			return false;
		}

		public void Update(GameTime time, GameLocation location)
		{
			FarmerSprite.setOwner(this);
			position.UpdateExtrapolation(getMovementSpeed());
			fireToolEvent.Poll();
			beginUsingToolEvent.Poll();
			endUsingToolEvent.Poll();
			drinkAnimationEvent.Poll();
			eatAnimationEvent.Poll();
			sickAnimationEvent.Poll();
			passOutEvent.Poll();
			doEmoteEvent.Poll();
			kissFarmerEvent.Poll();
			synchronizedJumpEvent.Poll();
			renovateEvent.Poll();
			if (IsLocalPlayer)
			{
				if (base.currentLocation == null)
				{
					return;
				}
				hidden.Value = localBusMoving() || (location.currentEvent != null && !location.currentEvent.isFestival) || (location.currentEvent != null && location.currentEvent.doingSecretSanta) || Game1.locationRequest != null || !Game1.displayFarmer;
				isInBed.Value = base.currentLocation.doesTileHaveProperty((int)getTileLocation().X, (int)getTileLocation().Y, "Bed", "Back") != null;
				if (!Game1.options.allowStowing)
				{
					netItemStowed.Value = false;
				}
				hasMenuOpen.Value = Game1.activeClickableMenu != null;
			}
			if (IsSitting())
			{
				movementDirections.Clear();
				if (IsSitting() && !isStopSitting)
				{
					if (!sittingFurniture.IsSeatHere(base.currentLocation))
					{
						StopSitting(animate: false);
					}
					else if (sittingFurniture is MapSeat)
					{
						if (!base.currentLocation.mapSeats.Contains(sittingFurniture))
						{
							StopSitting(animate: false);
						}
						else if ((sittingFurniture as MapSeat).IsBlocked(base.currentLocation))
						{
							StopSitting();
						}
					}
				}
			}
			if (Game1.CurrentEvent == null && !bathingClothes && !onBridge.Value)
			{
				canOnlyWalk = false;
			}
			if (noMovementPause > 0)
			{
				CanMove = false;
				noMovementPause -= time.ElapsedGameTime.Milliseconds;
				if (noMovementPause <= 0)
				{
					CanMove = true;
				}
			}
			if (freezePause > 0)
			{
				CanMove = false;
				freezePause -= time.ElapsedGameTime.Milliseconds;
				if (freezePause <= 0)
				{
					CanMove = true;
				}
			}
			if (sparklingText != null && sparklingText.update(time))
			{
				sparklingText = null;
			}
			if (newLevelSparklingTexts.Count > 0 && sparklingText == null && !UsingTool && CanMove && Game1.activeClickableMenu == null)
			{
				sparklingText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2003", getSkillDisplayNameFromIndex(newLevelSparklingTexts.Peek())), Color.White, Color.White, rainbow: true);
				newLevelSparklingTexts.Dequeue();
			}
			if (lerpPosition >= 0f)
			{
				lerpPosition += (float)time.ElapsedGameTime.TotalSeconds;
				if (lerpPosition >= lerpDuration)
				{
					lerpPosition = lerpDuration;
				}
				base.Position = new Vector2(Utility.Lerp(lerpStartPosition.X, lerpEndPosition.X, lerpPosition / lerpDuration), Utility.Lerp(lerpStartPosition.Y, lerpEndPosition.Y, lerpPosition / lerpDuration));
				if (lerpPosition >= lerpDuration)
				{
					lerpPosition = -1f;
				}
			}
			if (isStopSitting && lerpPosition < 0f)
			{
				isStopSitting = false;
				if (sittingFurniture != null)
				{
					mapChairSitPosition.Value = new Vector2(-1f, -1f);
					sittingFurniture.RemoveSittingFarmer(this);
					sittingFurniture = null;
					isSitting.Value = false;
				}
			}
			if ((bool)isInBed && Game1.IsMultiplayer && Game1.shouldTimePass())
			{
				regenTimer -= time.ElapsedGameTime.Milliseconds;
				if (regenTimer < 0)
				{
					regenTimer = 500;
					if (stamina < (float)(int)maxStamina)
					{
						stamina++;
					}
					if (health < maxHealth)
					{
						health++;
					}
				}
			}
			FarmerSprite.setOwner(this);
			FarmerSprite.checkForSingleAnimation(time);
			if (CanMove)
			{
				rotation = 0f;
				if (health <= 0 && !Game1.killScreen && Game1.timeOfDay < 2600)
				{
					if (IsSitting())
					{
						StopSitting(animate: false);
					}
					CanMove = false;
					Game1.screenGlowOnce(Color.Red, hold: true);
					Game1.killScreen = true;
					faceDirection(2);
					FarmerSprite.setCurrentFrame(5);
					jitterStrength = 1f;
					Game1.pauseTime = 3000f;
					Rumble.rumbleAndFade(0.75f, 1500f);
					freezePause = 8000;
					if (Game1.currentSong != null && Game1.currentSong.IsPlaying)
					{
						Game1.currentSong.Stop(AudioStopOptions.Immediate);
					}
					base.currentLocation.playSound("death");
					Game1.dialogueUp = false;
					Game1.stats.TimesUnconscious++;
					if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is GameMenu)
					{
						Game1.activeClickableMenu.emergencyShutDown();
						Game1.activeClickableMenu = null;
					}
				}
				Warp warp = null;
				switch (getDirection())
				{
				case 0:
					warp = location.isCollidingWithWarp(nextPosition(0), this);
					break;
				case 1:
					warp = location.isCollidingWithWarp(nextPosition(1), this);
					break;
				case 2:
					warp = location.isCollidingWithWarp(nextPosition(2), this);
					break;
				case 3:
					warp = location.isCollidingWithWarp(nextPosition(3), this);
					break;
				}
				if (collisionNPC != null)
				{
					collisionNPC.farmerPassesThrough = true;
				}
				if (movementDirections.Count > 0 && !isRidingHorse() && location.isCollidingWithCharacter(nextPosition(FacingDirection)) != null)
				{
					charactercollisionTimer += time.ElapsedGameTime.Milliseconds;
					if (charactercollisionTimer > 400)
					{
						location.isCollidingWithCharacter(nextPosition(FacingDirection)).shake(50);
					}
					if (charactercollisionTimer >= 1500 && collisionNPC == null)
					{
						collisionNPC = location.isCollidingWithCharacter(nextPosition(FacingDirection));
						if (collisionNPC.Name.Equals("Bouncer") && base.currentLocation != null && base.currentLocation.name.Equals("SandyHouse"))
						{
							collisionNPC.showTextAboveHead(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2010"));
							collisionNPC = null;
							charactercollisionTimer = 0;
						}
						else if (collisionNPC.name.Equals("Henchman") && base.currentLocation != null && base.currentLocation.name.Equals("WitchSwamp"))
						{
							collisionNPC = null;
							charactercollisionTimer = 0;
						}
					}
				}
				else
				{
					charactercollisionTimer = 0;
					if (collisionNPC != null && location.isCollidingWithCharacter(nextPosition(FacingDirection)) == null)
					{
						collisionNPC.farmerPassesThrough = false;
						collisionNPC = null;
					}
				}
			}
			if (Game1.shouldTimePass())
			{
				MeleeWeapon.weaponsTypeUpdate(time);
			}
			if (!Game1.eventUp || movementDirections.Count <= 0 || base.currentLocation.currentEvent == null || base.currentLocation.currentEvent.playerControlSequence || (controller != null && controller.allowPlayerPathingInEvent))
			{
				lastPosition = base.Position;
				if (controller != null)
				{
					if (controller.update(time))
					{
						controller = null;
					}
				}
				else if (controller == null)
				{
					MovePosition(time, Game1.viewport, location);
				}
			}
			if (hasNutPickupQueued && IsLocalPlayer && !IsBusyDoingSomething())
			{
				hasNutPickupQueued = false;
				showNutPickup();
			}
			updateCommon(time, location);
			position.Paused = FarmerSprite.PauseForSingleAnimation || UsingTool || isEating;
			checkDamage(location);
		}

		private void updateCommon(GameTime time, GameLocation location)
		{
			if (usernameDisplayTime > 0f)
			{
				usernameDisplayTime -= (float)time.ElapsedGameTime.TotalSeconds;
				if (usernameDisplayTime < 0f)
				{
					usernameDisplayTime = 0f;
				}
			}
			if (jitterStrength > 0f)
			{
				jitter = new Vector2((float)Game1.random.Next(-(int)(jitterStrength * 100f), (int)((jitterStrength + 1f) * 100f)) / 100f, (float)Game1.random.Next(-(int)(jitterStrength * 100f), (int)((jitterStrength + 1f) * 100f)) / 100f);
			}
			if (_wasSitting != isSitting.Value)
			{
				if (_wasSitting)
				{
					yOffset = 0f;
					xOffset = 0f;
				}
				_wasSitting = isSitting.Value;
			}
			if (yJumpOffset != 0)
			{
				yJumpVelocity -= 0.5f;
				yJumpOffset -= (int)yJumpVelocity;
				if (yJumpOffset >= 0)
				{
					yJumpOffset = 0;
					yJumpVelocity = 0f;
				}
			}
			updateMovementAnimation(time);
			updateEmote(time);
			updateGlow();
			currentLocationRef.Update();
			if ((bool)exhausted && stamina <= 1f)
			{
				currentEyes = 4;
				blinkTimer = -1000;
			}
			blinkTimer += time.ElapsedGameTime.Milliseconds;
			if (blinkTimer > 2200 && Game1.random.NextDouble() < 0.01)
			{
				blinkTimer = -150;
				currentEyes = 4;
			}
			else if (blinkTimer > -100)
			{
				if (blinkTimer < -50)
				{
					currentEyes = 1;
				}
				else if (blinkTimer < 0)
				{
					currentEyes = 4;
				}
				else
				{
					currentEyes = 0;
				}
			}
			if (isCustomized.Value && isInBed.Value && !Game1.eventUp && ((timerSinceLastMovement >= 3000 && Game1.timeOfDay >= 630) || timeWentToBed.Value != 0))
			{
				currentEyes = 1;
				blinkTimer = -10;
			}
			UpdateItemStow();
			if ((bool)swimming)
			{
				yOffset = (float)(Math.Cos(time.TotalGameTime.TotalMilliseconds / 2000.0) * 4.0);
				int num = swimTimer;
				swimTimer -= time.ElapsedGameTime.Milliseconds;
				if (timerSinceLastMovement == 0)
				{
					if (num > 400 && swimTimer <= 400 && IsLocalPlayer)
					{
						Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(xVelocity) + Math.Abs(yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, getStandingY() - 32), flicker: false, Game1.random.NextDouble() < 0.5, 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
					}
					if (swimTimer < 0)
					{
						swimTimer = 800;
						if (IsLocalPlayer)
						{
							base.currentLocation.playSound("slosh");
							Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(xVelocity) + Math.Abs(yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, getStandingY() - 32), flicker: false, Game1.random.NextDouble() < 0.5, 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
						}
					}
				}
				else if (!Game1.eventUp && (Game1.activeClickableMenu == null || Game1.IsMultiplayer) && !Game1.paused)
				{
					if (timerSinceLastMovement > 800)
					{
						currentEyes = 1;
					}
					else if (timerSinceLastMovement > 700)
					{
						currentEyes = 4;
					}
					if (swimTimer < 0)
					{
						swimTimer = 100;
						if (stamina < (float)(int)maxStamina)
						{
							stamina++;
						}
						if (health < maxHealth)
						{
							health++;
						}
					}
				}
			}
			if (!isMoving())
			{
				timerSinceLastMovement += time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				timerSinceLastMovement = 0;
			}
			for (int num2 = items.Count - 1; num2 >= 0; num2--)
			{
				if (items[num2] != null && items[num2] is Tool)
				{
					((Tool)items[num2]).tickUpdate(time, this);
				}
			}
			if (TemporaryItem is Tool)
			{
				(TemporaryItem as Tool).tickUpdate(time, this);
			}
			if (rightRing.Value != null)
			{
				rightRing.Value.update(time, location, this);
			}
			if (leftRing.Value != null)
			{
				leftRing.Value.update(time, location, this);
			}
			if (mount != null)
			{
				mount.update(time, location);
				if (mount != null)
				{
					mount.SyncPositionToRider();
				}
			}
		}

		public virtual bool IsBusyDoingSomething()
		{
			if (Game1.eventUp)
			{
				return true;
			}
			if (Game1.fadeToBlack)
			{
				return true;
			}
			if (Game1.currentMinigame != null)
			{
				return true;
			}
			if (Game1.activeClickableMenu != null)
			{
				return true;
			}
			if (Game1.isWarping)
			{
				return true;
			}
			if (UsingTool)
			{
				return true;
			}
			if (Game1.killScreen)
			{
				return true;
			}
			if (freezePause > 0)
			{
				return true;
			}
			if (!CanMove)
			{
				return false;
			}
			if (FarmerSprite.PauseForSingleAnimation)
			{
				return false;
			}
			_ = usingSlingshot;
			return false;
		}

		public void UpdateItemStow()
		{
			if (_itemStowed != netItemStowed.Value)
			{
				if (netItemStowed.Value && ActiveObject != null)
				{
					ActiveObject.actionWhenStopBeingHeld(this);
				}
				_itemStowed = netItemStowed.Value;
				if (!netItemStowed.Value && ActiveObject != null)
				{
					ActiveObject.actionWhenBeingHeld(this);
				}
			}
		}

		public void addQuest(int questID)
		{
			if (hasQuest(questID))
			{
				return;
			}
			Quest questFromId = Quest.getQuestFromId(questID);
			if (questFromId != null)
			{
				questLog.Add(questFromId);
				if (!questFromId.IsHidden())
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2011"), 2));
				}
			}
		}

		public void removeQuest(int questID)
		{
			for (int num = questLog.Count - 1; num >= 0; num--)
			{
				if ((int)questLog[num].id == questID)
				{
					questLog.RemoveAt(num);
				}
			}
		}

		public void completeQuest(int questID)
		{
			for (int num = questLog.Count - 1; num >= 0; num--)
			{
				if ((int)questLog[num].id == questID)
				{
					questLog[num].questComplete();
				}
			}
		}

		public bool hasQuest(int id)
		{
			for (int num = questLog.Count - 1; num >= 0; num--)
			{
				if ((int)questLog[num].id == id)
				{
					return true;
				}
			}
			return false;
		}

		public bool hasNewQuestActivity()
		{
			foreach (SpecialOrder specialOrder in team.specialOrders)
			{
				if (!specialOrder.IsHidden() && (specialOrder.ShouldDisplayAsNew() || specialOrder.ShouldDisplayAsComplete()))
				{
					return true;
				}
			}
			foreach (Quest item in questLog)
			{
				if (!item.IsHidden() && ((bool)item.showNew || ((bool)item.completed && !item.destroy)))
				{
					return true;
				}
			}
			return false;
		}

		public float getMovementSpeed()
		{
			float num = 1f;
			if (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence)
			{
				movementMultiplier = 0.066f;
				num = Math.Max(1f, ((float)base.speed + (Game1.eventUp ? 0f : ((float)base.addedSpeed + (isRidingHorse() ? 4.6f : temporarySpeedBuff)))) * movementMultiplier * (float)Game1.currentGameTime.ElapsedGameTime.Milliseconds);
				if (movementDirections.Count > 1)
				{
					num = 0.7f * num;
				}
				if (Game1.CurrentEvent == null && hasBuff(19))
				{
					num = 0f;
				}
			}
			else
			{
				num = Math.Max(1f, (float)base.speed + (Game1.eventUp ? ((float)Math.Max(0, Game1.CurrentEvent.farmerAddedSpeed - 2)) : ((float)base.addedSpeed + (isRidingHorse() ? 5f : temporarySpeedBuff))));
				if (movementDirections.Count > 1)
				{
					num = Math.Max(1, (int)Math.Sqrt(2f * (num * num)) / 2);
				}
			}
			return num;
		}

		public bool isWearingRing(int ringIndex)
		{
			if (rightRing.Value == null || !rightRing.Value.GetsEffectOfRing(ringIndex))
			{
				if (leftRing.Value != null)
				{
					return leftRing.Value.GetsEffectOfRing(ringIndex);
				}
				return false;
			}
			return true;
		}

		public override void Halt()
		{
			if (!FarmerSprite.PauseForSingleAnimation && !isRidingHorse() && !UsingTool)
			{
				base.Halt();
			}
			movementDirections.Clear();
			if (!isEmoteAnimating && !UsingTool)
			{
				stopJittering();
			}
			armOffset = Vector2.Zero;
			if (isRidingHorse())
			{
				mount.Halt();
				mount.Sprite.CurrentAnimation = null;
			}
			if (IsSitting())
			{
				ShowSitting();
			}
		}

		public void stopJittering()
		{
			jitterStrength = 0f;
			jitter = Vector2.Zero;
		}

		public override Microsoft.Xna.Framework.Rectangle nextPosition(int direction)
		{
			Microsoft.Xna.Framework.Rectangle boundingBox = GetBoundingBox();
			switch (direction)
			{
			case 0:
				boundingBox.Y -= (int)Math.Ceiling(getMovementSpeed());
				break;
			case 1:
				boundingBox.X += (int)Math.Ceiling(getMovementSpeed());
				break;
			case 2:
				boundingBox.Y += (int)Math.Ceiling(getMovementSpeed());
				break;
			case 3:
				boundingBox.X -= (int)Math.Ceiling(getMovementSpeed());
				break;
			}
			return boundingBox;
		}

		public Microsoft.Xna.Framework.Rectangle nextPositionHalf(int direction)
		{
			Microsoft.Xna.Framework.Rectangle boundingBox = GetBoundingBox();
			switch (direction)
			{
			case 0:
				boundingBox.Y -= (int)Math.Ceiling((double)getMovementSpeed() / 2.0);
				break;
			case 1:
				boundingBox.X += (int)Math.Ceiling((double)getMovementSpeed() / 2.0);
				break;
			case 2:
				boundingBox.Y += (int)Math.Ceiling((double)getMovementSpeed() / 2.0);
				break;
			case 3:
				boundingBox.X -= (int)Math.Ceiling((double)getMovementSpeed() / 2.0);
				break;
			}
			return boundingBox;
		}

		public int getProfessionForSkill(int skillType, int skillLevel)
		{
			switch (skillLevel)
			{
			case 5:
				switch (skillType)
				{
				case 0:
					if (professions.Contains(0))
					{
						return 0;
					}
					if (professions.Contains(1))
					{
						return 1;
					}
					break;
				case 1:
					if (professions.Contains(6))
					{
						return 6;
					}
					if (professions.Contains(7))
					{
						return 7;
					}
					break;
				case 2:
					if (professions.Contains(12))
					{
						return 12;
					}
					if (professions.Contains(13))
					{
						return 13;
					}
					break;
				case 3:
					if (professions.Contains(18))
					{
						return 18;
					}
					if (professions.Contains(19))
					{
						return 19;
					}
					break;
				case 4:
					if (professions.Contains(24))
					{
						return 24;
					}
					if (professions.Contains(25))
					{
						return 25;
					}
					break;
				}
				break;
			case 10:
				switch (skillType)
				{
				case 0:
					if (professions.Contains(1))
					{
						if (professions.Contains(4))
						{
							return 4;
						}
						if (professions.Contains(5))
						{
							return 5;
						}
					}
					else
					{
						if (professions.Contains(2))
						{
							return 2;
						}
						if (professions.Contains(3))
						{
							return 3;
						}
					}
					break;
				case 1:
					if (professions.Contains(6))
					{
						if (professions.Contains(8))
						{
							return 8;
						}
						if (professions.Contains(9))
						{
							return 9;
						}
					}
					else
					{
						if (professions.Contains(10))
						{
							return 10;
						}
						if (professions.Contains(11))
						{
							return 11;
						}
					}
					break;
				case 2:
					if (professions.Contains(12))
					{
						if (professions.Contains(14))
						{
							return 14;
						}
						if (professions.Contains(15))
						{
							return 15;
						}
					}
					else
					{
						if (professions.Contains(16))
						{
							return 16;
						}
						if (professions.Contains(17))
						{
							return 17;
						}
					}
					break;
				case 3:
					if (professions.Contains(18))
					{
						if (professions.Contains(20))
						{
							return 20;
						}
						if (professions.Contains(21))
						{
							return 21;
						}
					}
					else
					{
						if (professions.Contains(23))
						{
							return 23;
						}
						if (professions.Contains(22))
						{
							return 22;
						}
					}
					break;
				case 4:
					if (professions.Contains(24))
					{
						if (professions.Contains(26))
						{
							return 26;
						}
						if (professions.Contains(27))
						{
							return 27;
						}
					}
					else
					{
						if (professions.Contains(28))
						{
							return 28;
						}
						if (professions.Contains(29))
						{
							return 29;
						}
					}
					break;
				}
				break;
			}
			return -1;
		}

		public void behaviorOnMovement(int direction)
		{
			hasMoved = true;
		}

		public void OnEmoteAnimationEnd(Farmer farmer)
		{
			if (farmer == this && isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
		}

		public void EndEmoteAnimation()
		{
			if (isEmoteAnimating)
			{
				if (jitterStrength > 0f)
				{
					stopJittering();
				}
				if (yJumpOffset != 0)
				{
					yJumpOffset = 0;
					yJumpVelocity = 0f;
				}
				FarmerSprite.PauseForSingleAnimation = false;
				FarmerSprite.StopAnimation();
				isEmoteAnimating = false;
			}
		}

		private void broadcastHaltAnimation(Farmer who)
		{
			if (IsLocalPlayer)
			{
				haltAnimationEvent.Fire();
			}
			else
			{
				completelyStopAnimating(who);
			}
		}

		private void performHaltAnimation()
		{
			completelyStopAnimatingOrDoingAction();
		}

		public void performKissFarmer(long otherPlayerID)
		{
			Farmer farmer = Game1.getFarmer(otherPlayerID);
			if (farmer != null)
			{
				bool flag = getStandingX() < farmer.getStandingX();
				PerformKiss(flag ? 1 : 3);
				farmer.PerformKiss((!flag) ? 1 : 3);
			}
		}

		public void PerformKiss(int facingDirection)
		{
			if (!Game1.eventUp && !UsingTool && (!IsLocalPlayer || Game1.activeClickableMenu == null) && !isRidingHorse() && !IsSitting() && !base.IsEmoting && CanMove)
			{
				CanMove = false;
				FarmerSprite.PauseForSingleAnimation = false;
				faceDirection(facingDirection);
				FarmerSprite.animateOnce(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(101, 1000, 0, secondaryArm: false, FacingDirection == 3),
					new FarmerSprite.AnimationFrame(6, 1, secondaryArm: false, FacingDirection == 3, broadcastHaltAnimation)
				}.ToArray());
			}
		}

		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			if (IsSitting())
			{
				return;
			}
			if (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence)
			{
				if (Game1.shouldTimePass() && temporarilyInvincible)
				{
					if (temporaryInvincibilityTimer < 0)
					{
						currentTemporaryInvincibilityDuration = 1200;
					}
					temporaryInvincibilityTimer += time.ElapsedGameTime.Milliseconds;
					if (temporaryInvincibilityTimer > currentTemporaryInvincibilityDuration)
					{
						temporarilyInvincible = false;
						temporaryInvincibilityTimer = 0;
					}
				}
			}
			else if (temporarilyInvincible)
			{
				temporarilyInvincible = false;
				temporaryInvincibilityTimer = 0;
			}
			if (Game1.activeClickableMenu != null && (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence))
			{
				return;
			}
			if (isRafting)
			{
				moveRaft(currentLocation, time);
				return;
			}
			if (xVelocity != 0f || yVelocity != 0f)
			{
				if (double.IsNaN(xVelocity) || double.IsNaN(yVelocity))
				{
					xVelocity = 0f;
					yVelocity = 0f;
				}
				Microsoft.Xna.Framework.Rectangle boundingBox = GetBoundingBox();
				boundingBox.X += (int)Math.Floor(xVelocity);
				boundingBox.Y -= (int)Math.Floor(yVelocity);
				Microsoft.Xna.Framework.Rectangle boundingBox2 = GetBoundingBox();
				boundingBox2.X += (int)Math.Ceiling(xVelocity);
				boundingBox2.Y -= (int)Math.Ceiling(yVelocity);
				Microsoft.Xna.Framework.Rectangle rectangle = Microsoft.Xna.Framework.Rectangle.Union(boundingBox, boundingBox2);
				if (!currentLocation.isCollidingPosition(rectangle, viewport, isFarmer: true, -1, glider: false, this))
				{
					position.X += xVelocity;
					position.Y -= yVelocity;
					xVelocity -= xVelocity / 16f;
					yVelocity -= yVelocity / 16f;
					if (Math.Abs(xVelocity) <= 0.05f)
					{
						xVelocity = 0f;
					}
					if (Math.Abs(yVelocity) <= 0.05f)
					{
						yVelocity = 0f;
					}
				}
				else
				{
					xVelocity -= xVelocity / 16f;
					yVelocity -= yVelocity / 16f;
					if (Math.Abs(xVelocity) <= 0.05f)
					{
						xVelocity = 0f;
					}
					if (Math.Abs(yVelocity) <= 0.05f)
					{
						yVelocity = 0f;
					}
				}
			}
			if (CanMove || Game1.eventUp || controller != null)
			{
				temporaryPassableTiles.ClearNonIntersecting(GetBoundingBox());
				float movementSpeed = getMovementSpeed();
				temporarySpeedBuff = 0f;
				if (movementDirections.Contains(0))
				{
					Warp warp = Game1.currentLocation.isCollidingWithWarp(nextPosition(0), this);
					if (warp != null && IsLocalPlayer)
					{
						warpFarmer(warp, 0);
						return;
					}
					if (!currentLocation.isCollidingPosition(nextPosition(0), viewport, isFarmer: true, 0, glider: false, this) || ignoreCollisions)
					{
						position.Y -= movementSpeed;
						behaviorOnMovement(0);
					}
					else if (!currentLocation.isCollidingPosition(nextPositionHalf(0), viewport, isFarmer: true, 0, glider: false, this))
					{
						position.Y -= movementSpeed / 2f;
						behaviorOnMovement(0);
					}
					else if (movementDirections.Count == 1)
					{
						Microsoft.Xna.Framework.Rectangle rectangle2 = nextPosition(0);
						rectangle2.Width /= 4;
						bool flag = currentLocation.isCollidingPosition(rectangle2, viewport, isFarmer: true, 0, glider: false, this);
						rectangle2.X += rectangle2.Width * 3;
						bool flag2 = currentLocation.isCollidingPosition(rectangle2, viewport, isFarmer: true, 0, glider: false, this);
						if (flag && !flag2 && !currentLocation.isCollidingPosition(nextPosition(1), viewport, isFarmer: true, 0, glider: false, this))
						{
							position.X += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
						}
						else if (flag2 && !flag && !currentLocation.isCollidingPosition(nextPosition(3), viewport, isFarmer: true, 0, glider: false, this))
						{
							position.X -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
						}
					}
				}
				if (movementDirections.Contains(2))
				{
					Warp warp2 = Game1.currentLocation.isCollidingWithWarp(nextPosition(2), this);
					if (warp2 != null && IsLocalPlayer)
					{
						warpFarmer(warp2, 2);
						return;
					}
					if (!currentLocation.isCollidingPosition(nextPosition(2), viewport, isFarmer: true, 0, glider: false, this) || ignoreCollisions)
					{
						position.Y += movementSpeed;
						behaviorOnMovement(2);
					}
					else if (!currentLocation.isCollidingPosition(nextPositionHalf(2), viewport, isFarmer: true, 0, glider: false, this))
					{
						position.Y += movementSpeed / 2f;
						behaviorOnMovement(2);
					}
					else if (movementDirections.Count == 1)
					{
						Microsoft.Xna.Framework.Rectangle rectangle3 = nextPosition(2);
						rectangle3.Width /= 4;
						bool flag3 = currentLocation.isCollidingPosition(rectangle3, viewport, isFarmer: true, 0, glider: false, this);
						rectangle3.X += rectangle3.Width * 3;
						bool flag4 = currentLocation.isCollidingPosition(rectangle3, viewport, isFarmer: true, 0, glider: false, this);
						if (flag3 && !flag4 && !currentLocation.isCollidingPosition(nextPosition(1), viewport, isFarmer: true, 0, glider: false, this))
						{
							position.X += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
						}
						else if (flag4 && !flag3 && !currentLocation.isCollidingPosition(nextPosition(3), viewport, isFarmer: true, 0, glider: false, this))
						{
							position.X -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
						}
					}
				}
				if (movementDirections.Contains(1))
				{
					Warp warp3 = Game1.currentLocation.isCollidingWithWarp(nextPosition(1), this);
					if (warp3 != null && IsLocalPlayer)
					{
						warpFarmer(warp3, 1);
						return;
					}
					if (!currentLocation.isCollidingPosition(nextPosition(1), viewport, isFarmer: true, 0, glider: false, this) || ignoreCollisions)
					{
						position.X += movementSpeed;
						behaviorOnMovement(1);
					}
					else if (!currentLocation.isCollidingPosition(nextPositionHalf(1), viewport, isFarmer: true, 0, glider: false, this))
					{
						position.X += movementSpeed / 2f;
						behaviorOnMovement(1);
					}
					else if (movementDirections.Count == 1)
					{
						Microsoft.Xna.Framework.Rectangle rectangle4 = nextPosition(1);
						rectangle4.Height /= 4;
						bool flag5 = currentLocation.isCollidingPosition(rectangle4, viewport, isFarmer: true, 0, glider: false, this);
						rectangle4.Y += rectangle4.Height * 3;
						bool flag6 = currentLocation.isCollidingPosition(rectangle4, viewport, isFarmer: true, 0, glider: false, this);
						if (flag5 && !flag6 && !currentLocation.isCollidingPosition(nextPosition(2), viewport, isFarmer: true, 0, glider: false, this))
						{
							position.Y += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
						}
						else if (flag6 && !flag5 && !currentLocation.isCollidingPosition(nextPosition(0), viewport, isFarmer: true, 0, glider: false, this))
						{
							position.Y -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
						}
					}
				}
				if (movementDirections.Contains(3))
				{
					Warp warp4 = Game1.currentLocation.isCollidingWithWarp(nextPosition(3), this);
					if (warp4 != null && IsLocalPlayer)
					{
						warpFarmer(warp4, 3);
						return;
					}
					if (!currentLocation.isCollidingPosition(nextPosition(3), viewport, isFarmer: true, 0, glider: false, this) || ignoreCollisions)
					{
						position.X -= movementSpeed;
						behaviorOnMovement(3);
					}
					else if (!currentLocation.isCollidingPosition(nextPositionHalf(3), viewport, isFarmer: true, 0, glider: false, this))
					{
						position.X -= movementSpeed / 2f;
						behaviorOnMovement(3);
					}
					else if (movementDirections.Count == 1)
					{
						Microsoft.Xna.Framework.Rectangle rectangle5 = nextPosition(3);
						rectangle5.Height /= 4;
						bool flag7 = currentLocation.isCollidingPosition(rectangle5, viewport, isFarmer: true, 0, glider: false, this);
						rectangle5.Y += rectangle5.Height * 3;
						bool flag8 = currentLocation.isCollidingPosition(rectangle5, viewport, isFarmer: true, 0, glider: false, this);
						if (flag7 && !flag8 && !currentLocation.isCollidingPosition(nextPosition(2), viewport, isFarmer: true, 0, glider: false, this))
						{
							position.Y += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
						}
						else if (flag8 && !flag7 && !currentLocation.isCollidingPosition(nextPosition(0), viewport, isFarmer: true, 0, glider: false, this))
						{
							position.Y -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
						}
					}
				}
			}
			if (movementDirections.Count > 0 && !UsingTool)
			{
				FarmerSprite.intervalModifier = 1f - (running ? 0.03f : 0.025f) * (Math.Max(1f, ((float)base.speed + (Game1.eventUp ? 0f : ((float)base.addedSpeed + (isRidingHorse() ? 4.6f : 0f)))) * movementMultiplier * (float)Game1.currentGameTime.ElapsedGameTime.Milliseconds) * 1.25f);
			}
			else
			{
				FarmerSprite.intervalModifier = 1f;
			}
			if (currentLocation != null && currentLocation.isFarmerCollidingWithAnyCharacter())
			{
				temporaryPassableTiles.Add(new Microsoft.Xna.Framework.Rectangle((int)getTileLocation().X * 64, (int)getTileLocation().Y * 64, 64, 64));
			}
		}

		public void updateMovementAnimation(GameTime time)
		{
			if (_emoteGracePeriod > 0)
			{
				_emoteGracePeriod -= time.ElapsedGameTime.Milliseconds;
			}
			if (isEmoteAnimating)
			{
				bool flag = false;
				flag = ((!IsLocalPlayer) ? IsRemoteMoving() : (movementDirections.Count > 0));
				if ((flag && _emoteGracePeriod <= 0) || !FarmerSprite.PauseForSingleAnimation)
				{
					EndEmoteAnimation();
				}
			}
			bool flag2 = IsCarrying();
			if (!isRidingHorse())
			{
				xOffset = 0f;
			}
			if (CurrentTool is FishingRod)
			{
				FishingRod fishingRod = CurrentTool as FishingRod;
				if (fishingRod.isTimingCast || fishingRod.isCasting)
				{
					fishingRod.setTimingCastAnimation(this);
					return;
				}
			}
			if (FarmerSprite.PauseForSingleAnimation || UsingTool)
			{
				return;
			}
			if (IsSitting())
			{
				ShowSitting();
				return;
			}
			if (IsLocalPlayer && !CanMove && !Game1.eventUp)
			{
				if (isRidingHorse() && mount != null && !isAnimatingMount)
				{
					showRiding();
				}
				else if (flag2)
				{
					showCarrying();
				}
				return;
			}
			if (IsLocalPlayer || isFakeEventActor)
			{
				moveUp = movementDirections.Contains(0);
				moveRight = movementDirections.Contains(1);
				moveDown = movementDirections.Contains(2);
				moveLeft = movementDirections.Contains(3);
				bool flag3 = moveUp || moveRight || moveDown || moveLeft;
				if (moveLeft)
				{
					FacingDirection = 3;
				}
				else if (moveRight)
				{
					FacingDirection = 1;
				}
				else if (moveUp)
				{
					FacingDirection = 0;
				}
				else if (moveDown)
				{
					FacingDirection = 2;
				}
				if (isRidingHorse() && !mount.dismounting)
				{
					base.speed = 2;
				}
			}
			else
			{
				moveLeft = IsRemoteMoving() && FacingDirection == 3;
				moveRight = IsRemoteMoving() && FacingDirection == 1;
				moveUp = IsRemoteMoving() && FacingDirection == 0;
				moveDown = IsRemoteMoving() && FacingDirection == 2;
				bool flag4 = moveUp || moveRight || moveDown || moveLeft;
				float num = position.CurrentInterpolationSpeed() / ((float)Game1.currentGameTime.ElapsedGameTime.Milliseconds * 0.066f);
				running = Math.Abs(num - 5f) < Math.Abs(num - 2f) && !bathingClothes && !onBridge.Value;
				if (!flag4)
				{
					FarmerSprite.StopAnimation();
				}
			}
			if (hasBuff(19))
			{
				running = false;
				moveUp = false;
				moveDown = false;
				moveLeft = false;
				moveRight = false;
			}
			if (!FarmerSprite.PauseForSingleAnimation && !UsingTool)
			{
				if (isRidingHorse() && !mount.dismounting)
				{
					showRiding();
				}
				else if (moveLeft && running && !flag2)
				{
					FarmerSprite.animate(56, time);
				}
				else if (moveRight && running && !flag2)
				{
					FarmerSprite.animate(40, time);
				}
				else if (moveUp && running && !flag2)
				{
					FarmerSprite.animate(48, time);
				}
				else if (moveDown && running && !flag2)
				{
					FarmerSprite.animate(32, time);
				}
				else if (moveLeft && running)
				{
					FarmerSprite.animate(152, time);
				}
				else if (moveRight && running)
				{
					FarmerSprite.animate(136, time);
				}
				else if (moveUp && running)
				{
					FarmerSprite.animate(144, time);
				}
				else if (moveDown && running)
				{
					FarmerSprite.animate(128, time);
				}
				else if (moveLeft && !flag2)
				{
					FarmerSprite.animate(24, time);
				}
				else if (moveRight && !flag2)
				{
					FarmerSprite.animate(8, time);
				}
				else if (moveUp && !flag2)
				{
					FarmerSprite.animate(16, time);
				}
				else if (moveDown && !flag2)
				{
					FarmerSprite.animate(0, time);
				}
				else if (moveLeft)
				{
					FarmerSprite.animate(120, time);
				}
				else if (moveRight)
				{
					FarmerSprite.animate(104, time);
				}
				else if (moveUp)
				{
					FarmerSprite.animate(112, time);
				}
				else if (moveDown)
				{
					FarmerSprite.animate(96, time);
				}
				else if (flag2)
				{
					showCarrying();
				}
				else
				{
					showNotCarrying();
				}
			}
		}

		public bool IsCarrying()
		{
			if (mount != null || isAnimatingMount)
			{
				return false;
			}
			if (IsSitting())
			{
				return false;
			}
			if (onBridge.Value)
			{
				return false;
			}
			if (ActiveObject == null || Game1.eventUp || Game1.killScreen)
			{
				return false;
			}
			if (ActiveObject is Furniture)
			{
				return false;
			}
			return true;
		}

		public void doneEating()
		{
			isEating = false;
			completelyStopAnimatingOrDoingAction();
			forceCanMove();
			if (mostRecentlyGrabbedItem == null)
			{
				return;
			}
			Object @object = itemToEat as Object;
			if (IsLocalPlayer && @object.ParentSheetIndex == 434)
			{
				if (Utility.foundAllStardrops())
				{
					Game1.getSteamAchievement("Achievement_Stardrop");
				}
				yOffset = 0f;
				yJumpOffset = 0;
				Game1.changeMusicTrack("none");
				Game1.playSound("stardrop");
				string text = ((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3094") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3095"));
				DelayedAction.showDialogueAfterDelay(string.Concat(str1: favoriteThing.Contains("Stardew") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3097") : ((!favoriteThing.Equals("ConcernedApe")) ? (text + favoriteThing) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3099")), str0: Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3100"), str2: Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3101")), 6000);
				MaxStamina += 34;
				Stamina = MaxStamina;
				FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
				{
					new FarmerSprite.AnimationFrame(57, 6000)
				});
				startGlowing(new Color(200, 0, 255), border: false, 0.1f);
				jitterStrength = 1f;
				Game1.staminaShakeTimer = 12000;
				Game1.screenGlowOnce(new Color(200, 0, 255), hold: true);
				CanMove = false;
				freezePause = 8000;
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(368, 16, 16, 16), 60f, 8, 40, base.Position + new Vector2(-8f, -128f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0.0075f, 0f, 0f)
				{
					alpha = 0.75f,
					alphaFade = 0.0025f,
					motion = new Vector2(0f, -0.25f)
				});
				if (Game1.displayHUD && !Game1.eventUp)
				{
					for (int i = 0; i < 40; i++)
					{
						Game1.uiOverlayTempSprites.Add(new TemporaryAnimatedSprite(Game1.random.Next(10, 12), new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Right / Game1.options.uiScale - 48f - 8f - (float)Game1.random.Next(64), (float)Game1.random.Next(-64, 64) + (float)Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Bottom / Game1.options.uiScale - 224f - 16f - (float)(int)((double)(Game1.player.MaxStamina - 270) * 0.715)), (Game1.random.NextDouble() < 0.5) ? Color.White : Color.Lime, 8, flipped: false, 50f)
						{
							layerDepth = 1f,
							delayBeforeAnimationStart = 200 * i,
							interval = 100f,
							local = true
						});
					}
				}
				Utility.addSprinklesToLocation(base.currentLocation, getTileX(), getTileY(), 9, 9, 6000, 100, new Color(200, 0, 255), null, motionTowardCenter: true);
				DelayedAction.stopFarmerGlowing(6000);
				Utility.addSprinklesToLocation(base.currentLocation, getTileX(), getTileY(), 9, 9, 6000, 300, Color.Cyan, null, motionTowardCenter: true);
				mostRecentlyGrabbedItem = null;
			}
			else if (IsLocalPlayer)
			{
				if (@object != null && @object.HasContextTag("ginger_item") && hasBuff(25))
				{
					Game1.buffsDisplay.removeOtherBuff(25);
				}
				string[] array = Game1.objectInformation[@object.ParentSheetIndex].Split('/');
				if (Convert.ToInt32(array[2]) > 0)
				{
					string[] array2 = ((array.Length > 7) ? array[7].Split(' ') : new string[12]
					{
						"0", "0", "0", "0", "0", "0", "0", "0", "0", "0",
						"0", "0"
					});
					@object.ModifyItemBuffs(array2);
					int num = ((array.Length > 8) ? Convert.ToInt32(array[8]) : (-1));
					if (@object.Quality != 0)
					{
						num = (int)((float)num * 1.5f);
					}
					Buff buff = new Buff(Convert.ToInt32(array2[0]), Convert.ToInt32(array2[1]), Convert.ToInt32(array2[2]), Convert.ToInt32(array2[3]), Convert.ToInt32(array2[4]), Convert.ToInt32(array2[5]), Convert.ToInt32(array2[6]), Convert.ToInt32(array2[7]), Convert.ToInt32(array2[8]), Convert.ToInt32(array2[9]), Convert.ToInt32(array2[10]), (array2.Length > 11) ? Convert.ToInt32(array2[11]) : 0, num, array[0], array[4]);
					if (Utility.IsNormalObjectAtParentSheetIndex(@object, 921))
					{
						buff.which = 28;
					}
					if (array.Length > 6 && array[6].Equals("drink"))
					{
						Game1.buffsDisplay.tryToAddDrinkBuff(buff);
					}
					else if (Convert.ToInt32(array[2]) > 0)
					{
						Game1.buffsDisplay.tryToAddFoodBuff(buff, Math.Min(120000, (int)((float)Convert.ToInt32(array[2]) / 20f * 30000f)));
					}
				}
				float num2 = Stamina;
				int num3 = health;
				int num4 = @object.staminaRecoveredOnConsumption();
				Stamina = Math.Min(MaxStamina, Stamina + (float)num4);
				health = Math.Min(maxHealth, health + @object.healthRecoveredOnConsumption());
				if (num2 < Stamina)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3116", (int)(Stamina - num2)), 4));
				}
				if (num3 < health)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3118", health - num3), 5));
				}
			}
			if (@object != null && @object.Edibility < 0 && IsLocalPlayer)
			{
				CanMove = false;
				sickAnimationEvent.Fire();
			}
		}

		public bool checkForQuestComplete(NPC n, int number1, int number2, Item item, string str, int questType = -1, int questTypeToIgnore = -1)
		{
			bool result = false;
			for (int num = questLog.Count - 1; num >= 0; num--)
			{
				if (questLog[num] != null && (questType == -1 || (int)questLog[num].questType == questType) && (questTypeToIgnore == -1 || (int)questLog[num].questType != questTypeToIgnore) && questLog[num].checkIfComplete(n, number1, number2, item, str))
				{
					result = true;
				}
			}
			return result;
		}

		public static void completelyStopAnimating(Farmer who)
		{
			who.completelyStopAnimatingOrDoingAction();
		}

		public void completelyStopAnimatingOrDoingAction()
		{
			CanMove = !Game1.eventUp;
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			if (UsingTool)
			{
				EndUsingTool();
				if (CurrentTool is FishingRod)
				{
					(CurrentTool as FishingRod).resetState();
				}
			}
			if (usingSlingshot && CurrentTool is Slingshot)
			{
				(CurrentTool as Slingshot).finish();
			}
			UsingTool = false;
			isEating = false;
			FarmerSprite.PauseForSingleAnimation = false;
			usingSlingshot = false;
			canReleaseTool = false;
			Halt();
			Sprite.StopAnimation();
			if (CurrentTool is MeleeWeapon)
			{
				(CurrentTool as MeleeWeapon).isOnSpecial = false;
			}
			stopJittering();
		}

		public void doEmote(int whichEmote)
		{
			if (!Game1.eventUp && !isEmoting)
			{
				isEmoting = true;
				currentEmote = whichEmote;
				currentEmoteFrame = 0;
				emoteInterval = 0f;
			}
		}

		public void performTenMinuteUpdate()
		{
			if (base.addedSpeed > 0 && Game1.buffsDisplay.otherBuffs.Count == 0 && Game1.buffsDisplay.food == null && Game1.buffsDisplay.drink == null)
			{
				base.addedSpeed = 0;
			}
		}

		public void setRunning(bool isRunning, bool force = false)
		{
			if (canOnlyWalk || ((bool)bathingClothes && !running) || (Game1.CurrentEvent != null && isRunning && !Game1.CurrentEvent.isFestival && !Game1.CurrentEvent.playerControlSequence && (controller == null || !controller.allowPlayerPathingInEvent)))
			{
				return;
			}
			if (isRidingHorse())
			{
				running = true;
			}
			else if (stamina <= 0f)
			{
				base.speed = 2;
				if (running)
				{
					Halt();
				}
				running = false;
			}
			else if (force || (CanMove && !isEating && (Game1.currentLocation.currentEvent == null || Game1.currentLocation.currentEvent.playerControlSequence) && (isRunning || !UsingTool) && (isRunning || !Game1.pickingTool) && (Sprite == null || !((FarmerSprite)Sprite).PauseForSingleAnimation)))
			{
				running = isRunning;
				if (running)
				{
					base.speed = 5;
				}
				else
				{
					base.speed = 2;
				}
			}
			else if (UsingTool)
			{
				running = isRunning;
				if (running)
				{
					base.speed = 5;
				}
				else
				{
					base.speed = 2;
				}
			}
		}

		public void addSeenResponse(int id)
		{
			dialogueQuestionsAnswered.Add(id);
		}

		public void eatObject(Object o, bool overrideFullness = false)
		{
			if (o == null || !Game1.objectInformation.ContainsKey(o.ParentSheetIndex))
			{
				forceCanMove();
				completelyStopAnimatingOrDoingAction();
				return;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(o, 434))
			{
				Game1.changeMusicTrack("none");
				Game1.multiplayer.globalChatInfoMessage("Stardrop", base.Name);
			}
			if (getFacingDirection() != 2)
			{
				faceDirection(2);
			}
			itemToEat = o;
			mostRecentlyGrabbedItem = o;
			string[] array = Game1.objectInformation[o.ParentSheetIndex].Split('/');
			forceCanMove();
			completelyStopAnimatingOrDoingAction();
			if (array.Length > 6 && array[6].Equals("drink"))
			{
				if (IsLocalPlayer && Game1.buffsDisplay.hasBuff(7) && !overrideFullness)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2898"), Color.OrangeRed, 3500f));
					return;
				}
				drinkAnimationEvent.Fire(o.getOne() as Object);
			}
			else if (Convert.ToInt32(array[2]) != -300)
			{
				if (Game1.buffsDisplay.hasBuff(6) && !overrideFullness)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2899"), Color.OrangeRed, 3500f));
					return;
				}
				eatAnimationEvent.Fire(o.getOne() as Object);
			}
			freezePause = 20000;
			CanMove = false;
			isEating = true;
		}

		private void performDrinkAnimation(Object item)
		{
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			if (!IsLocalPlayer)
			{
				itemToEat = item;
			}
			FarmerSprite.animateOnce(294, 80f, 8);
			isEating = true;
		}

		public Farmer CreateFakeEventFarmer()
		{
			Farmer farmer = new Farmer(new FarmerSprite(FarmerSprite.textureName.Value), new Vector2(192f, 192f), 1, "", new List<Item>(), IsMale);
			farmer.Name = base.Name;
			farmer.displayName = displayName;
			farmer.isFakeEventActor = true;
			farmer.changeGender(IsMale);
			farmer.changeHairStyle(hair);
			farmer.UniqueMultiplayerID = UniqueMultiplayerID;
			farmer.shirtItem.Set(shirtItem);
			farmer.pantsItem.Set(pantsItem);
			farmer.shirt.Set(shirt.Value);
			farmer.pants.Set(pants.Value);
			farmer.changeShoeColor(shoes.Value);
			farmer.boots.Set(boots.Value);
			farmer.leftRing.Set(leftRing.Value);
			farmer.rightRing.Set(rightRing.Value);
			farmer.hat.Set(hat.Value);
			farmer.shirtColor = shirtColor;
			farmer.pantsColor.Set(pantsColor.Value);
			farmer.changeHairColor(hairstyleColor);
			farmer.changeSkinColor(skin.Value);
			farmer.accessory.Set(accessory.Value);
			farmer.changeEyeColor(newEyeColor.Value);
			farmer.UpdateClothing();
			return farmer;
		}

		private void performEatAnimation(Object item)
		{
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			if (!IsLocalPlayer)
			{
				itemToEat = item;
			}
			FarmerSprite.animateOnce(216, 80f, 8);
			isEating = true;
		}

		public void netDoEmote(string emote_type)
		{
			doEmoteEvent.Fire(emote_type);
		}

		private void performSickAnimation()
		{
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			isEating = false;
			FarmerSprite.animateOnce(224, 350f, 4);
			doEmote(12);
		}

		public void eatHeldObject()
		{
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			if (!Game1.fadeToBlack)
			{
				if (ActiveObject == null)
				{
					ActiveObject = (Object)mostRecentlyGrabbedItem;
				}
				eatObject(ActiveObject);
				if (isEating)
				{
					reduceActiveItemByOne();
					CanMove = false;
				}
			}
		}

		public void grabObject(Object obj)
		{
			if (isEmoteAnimating)
			{
				EndEmoteAnimation();
			}
			if (obj != null)
			{
				CanMove = false;
				switch (FacingDirection)
				{
				case 2:
					((FarmerSprite)Sprite).animateOnce(64, 50f, 8);
					break;
				case 1:
					((FarmerSprite)Sprite).animateOnce(72, 50f, 8);
					break;
				case 0:
					((FarmerSprite)Sprite).animateOnce(80, 50f, 8);
					break;
				case 3:
					((FarmerSprite)Sprite).animateOnce(88, 50f, 8);
					break;
				}
				Game1.playSound("pickUpItem");
			}
		}

		public virtual void PlayFishBiteChime()
		{
			int num = biteChime.Value;
			if (num < 0)
			{
				num = Game1.game1.instanceIndex;
			}
			if (num > 3)
			{
				num = 3;
			}
			if (num == 0)
			{
				base.currentLocation.localSound("fishBite");
			}
			else
			{
				base.currentLocation.localSound("fishBite_alternate_" + (num - 1));
			}
		}

		public string getTitle()
		{
			int level = Level;
			if (level >= 30)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2016");
			}
			if (level > 28)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2017");
			}
			if (level > 26)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2018");
			}
			if (level > 24)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2019");
			}
			if (level > 22)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2020");
			}
			if (level > 20)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2021");
			}
			if (level > 18)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2022");
			}
			if (level > 16)
			{
				if (!IsMale)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2024");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2023");
			}
			if (level > 14)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2025");
			}
			if (level > 12)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2026");
			}
			if (level > 10)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2027");
			}
			if (level > 8)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2028");
			}
			if (level > 6)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2029");
			}
			if (level > 4)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2030");
			}
			if (level > 2)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2031");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2032");
		}

		public void queueMessage(byte messageType, Farmer sourceFarmer, params object[] data)
		{
			queueMessage(new OutgoingMessage(messageType, sourceFarmer, data));
		}

		public void queueMessage(OutgoingMessage message)
		{
			messageQueue.Add(message);
		}
	}
}
