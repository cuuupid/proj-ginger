using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using SkiaSharp;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Mobile;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Projectiles;
using StardewValley.Quests;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewValley.Util;
using xTile;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley
{
	[InstanceStatics]
	public class Game1 : InstanceGame
	{
		public enum MusicWaveBankState
		{
			NotInitialised,
			NotDownloaded,
			Created
		}

		public enum BundleType
		{
			Default,
			Remixed
		}

		private class MusicContextComparer : IEqualityComparer<MusicContext>
		{
			public static readonly MusicContextComparer Default = new MusicContextComparer();

			public bool Equals(MusicContext x, MusicContext y)
			{
				return x == y;
			}

			public int GetHashCode(MusicContext b)
			{
				return b.GetHashCode();
			}
		}

		public enum MusicContext
		{
			Default,
			SubLocation,
			Event,
			MiniGame,
			ImportantSplitScreenMusic,
			MAX
		}

		public delegate void afterFadeFunction();

		[NonInstancedStatic]
		public static string savesPath;

		[NonInstancedStatic]
		public static string hiddenSavesPath;

		[NonInstancedStatic]
		public static string screenshotsPath;

		[NonInstancedStatic]
		public static int xEdge = 0;

		[NonInstancedStatic]
		public static int toolbarPaddingX = 0;

		[NonInstancedStatic]
		public static Microsoft.Xna.Framework.Rectangle clientBounds;

		private ButtonState _lastBackButtonState;

		public const float tapHoldTime = 0.5f;

		public const int headerOffset = 16;

		[NonInstancedStatic]
		public static int maxItemSlotSize = 80;

		[NonInstancedStatic]
		public static bool skipTutorials = false;

		[NonInstancedStatic]
		public static bool emergencyLoading = false;

		public static bool SeenConcernedApeLogo = false;

		public const string titleButtonsTextureName = "Minigames\\TitleButtons";

		[NonInstancedStatic]
		public static Texture2D titleButtonsTexture;

		[NonInstancedStatic]
		public static int logoFadeTimer;

		private int _greenSquareAnimIndex;

		private long _greenSquareLastUpdateTicks = DateTime.Now.Ticks;

		private Object _lastActiveObject;

		private string _lastLocation = "";

		[NonInstancedStatic]
		private static string _pendingTrackName = null;

		[NonInstancedStatic]
		private static IEnumerator<int> locationLoader;

		[NonInstancedStatic]
		private static IEnumerator<int> farmerLoader;

		[NonInstancedStatic]
		private static IEnumerator<int> wholeBackupLoader;

		public static VirtualJoypad virtualJoypad;

		public static Toolbar toolbar;

		public static Texture2D mobileSpriteSheet;

		[NonInstancedStatic]
		public static MusicWaveBankState musicWaveBankState = MusicWaveBankState.NotInitialised;

		public bool ScreenshotBusy;

		public bool takingMapScreenshot;

		public const int pixelZoom = 4;

		public const int tileSize = 64;

		public const int smallestTileSize = 16;

		public const int up = 0;

		public const int right = 1;

		public const int down = 2;

		public const int left = 3;

		public const int spriteIndexForOveralls = 3854;

		public const int colorToleranceForOveralls = 60;

		public const int spriteIndexForOverallsBorder = 3846;

		public const int colorToloranceForOverallsBorder = 20;

		public const int dialogueBoxTileHeight = 5;

		public const int realMilliSecondsPerGameTenMinutes = 7000;

		public const int rainDensity = 70;

		public const int millisecondsPerDialogueLetterType = 30;

		public const float pickToolDelay = 500f;

		public const int defaultMinFishingBiteTime = 600;

		public const int defaultMaxFishingBiteTime = 30000;

		public const int defaultMinFishingNibbleTime = 340;

		public const int defaultMaxFishingNibbleTime = 800;

		public const int minWallpaperPrice = 75;

		public const int maxWallpaperPrice = 500;

		public const int rainLoopLength = 70;

		public const int weather_sunny = 0;

		public const int weather_rain = 1;

		public const int weather_debris = 2;

		public const int weather_lightning = 3;

		public const int weather_festival = 4;

		public const int weather_snow = 5;

		public const int weather_wedding = 6;

		public const byte singlePlayer = 0;

		public const byte multiplayerClient = 1;

		public const byte multiplayerServer = 2;

		public const byte logoScreenGameMode = 4;

		public const byte titleScreenGameMode = 0;

		public const byte loadScreenGameMode = 1;

		public const byte newGameMode = 2;

		public const byte playingGameMode = 3;

		public const byte loadingMode = 6;

		public const byte saveMode = 7;

		public const byte saveCompleteMode = 8;

		public const byte selectGameScreen = 9;

		public const byte creditsMode = 10;

		public const byte errorLogMode = 11;

		public static readonly string version = "1.5.6";

		public static readonly string versionLabel = "";

		public static readonly int versionBuild = 51;

		public const float keyPollingThreshold = 650f;

		public const float toolHoldPerPowerupLevel = 600f;

		public const float startingMusicVolume = 1f;

		public LocalizedContentManager xTileContent;

		public static DelayedAction morningSongPlayAction;

		internal static LocalizedContentManager _temporaryContent;

		[NonInstancedStatic]
		public static GraphicsDeviceManager graphics;

		[NonInstancedStatic]
		public static LocalizedContentManager content;

		public static SpriteBatch spriteBatch;

		[NonInstancedStatic]
		public static readonly Mutex RenderLock = new Mutex();

		public static GamePadState oldPadState;

		public static float thumbStickSensitivity = 0.1f;

		public static float runThreshold = 0.5f;

		public static int rightStickHoldTime = 0;

		public static int emoteMenuShowTime = 250;

		public static int nextFarmerWarpOffsetX = 0;

		public static int nextFarmerWarpOffsetY = 0;

		public static KeyboardState oldKBState;

		public static MouseState oldMouseState;

		[NonInstancedStatic]
		public static Game1 keyboardFocusInstance = null;

		internal static Farmer _player;

		public static NetFarmerRoot serverHost;

		internal static bool _isWarping = false;

		[NonInstancedStatic]
		public static bool hasLocalClientsOnly = false;

		public static bool isUsingBackToFrontSorting = false;

		internal static StringBuilder _debugStringBuilder = new StringBuilder();

		public static Dictionary<string, GameLocation> _locationLookup = new Dictionary<string, GameLocation>(StringComparer.OrdinalIgnoreCase);

		public readonly List<GameLocation> _locations = new List<GameLocation>();

		public static Viewport defaultDeviceViewport;

		public static LocationRequest locationRequest;

		public static bool warpingForForcedRemoteEvent = false;

		public GameLocation instanceGameLocation;

		public static IDisplayDevice mapDisplayDevice;

		[NonInstancedStatic]
		public static Microsoft.Xna.Framework.Rectangle safeAreaBounds = default(Microsoft.Xna.Framework.Rectangle);

		[NonInstancedStatic]
		public static bool shouldDrawSafeAreaBounds = false;

		public static xTile.Dimensions.Rectangle viewport;

		public static xTile.Dimensions.Rectangle uiViewport;

		public static Texture2D objectSpriteSheet;

		public static Texture2D cropSpriteSheet;

		public static Texture2D mailboxTexture;

		public static Texture2D emoteSpriteSheet;

		public static Texture2D debrisSpriteSheet;

		public static Texture2D toolIconBox;

		public static Texture2D rainTexture;

		public static Texture2D bigCraftableSpriteSheet;

		public static Texture2D swordSwipe;

		public static Texture2D swordSwipeDark;

		public static Texture2D buffsIcons;

		public static Texture2D daybg;

		public static Texture2D nightbg;

		public static Texture2D logoScreenTexture;

		public static Texture2D tvStationTexture;

		public static Texture2D cloud;

		public static Texture2D menuTexture;

		public static Texture2D uncoloredMenuTexture;

		public static Texture2D lantern;

		public static Texture2D windowLight;

		public static Texture2D sconceLight;

		public static Texture2D cauldronLight;

		public static Texture2D shadowTexture;

		public static Texture2D mouseCursors;

		public static Texture2D mouseCursors2;

		public static Texture2D giftboxTexture;

		public static Texture2D controllerMaps;

		public static Texture2D indoorWindowLight;

		public static Texture2D animations;

		public static Texture2D titleScreenBG;

		public static Texture2D logo;

		public static Texture2D concessionsSpriteSheet;

		public static Texture2D birdsSpriteSheet;

		public static Dictionary<string, Stack<Dialogue>> npcDialogues = new Dictionary<string, Stack<Dialogue>>();

		protected readonly List<Farmer> _farmerShadows = new List<Farmer>();

		public static Queue<DelayedAction.delayedBehavior> morningQueue = new Queue<DelayedAction.delayedBehavior>();

		[NonInstancedStatic]
		protected internal static ModHooks hooks = new ModHooks();

		public static InputState input = new InputState();

		internal static IInputSimulator inputSimulator = null;

		public const string objectSpriteSheetName = "Maps\\springobjects";

		public const string animationsName = "TileSheets\\animations";

		public const string mouseCursorsName = "LooseSprites\\Cursors";

		public const string mouseCursors2Name = "LooseSprites\\Cursors2";

		public const string giftboxName = "LooseSprites\\Giftbox";

		public const string toolSpriteSheetName = "TileSheets\\tools";

		public const string bigCraftableSpriteSheetName = "TileSheets\\Craftables";

		public const string debrisSpriteSheetName = "TileSheets\\debris";

		public const string parrotSheetName = "LooseSprites\\parrots";

		public const string hatsSheetName = "Characters\\Farmer\\hats";

		internal static Texture2D _toolSpriteSheet = null;

		public static Dictionary<Vector2, int> crabPotOverlayTiles = new Dictionary<Vector2, int>();

		internal static bool _setSaveName = false;

		internal static string _currentSaveName = "";

		public static string savePathOverride = "";

		public static List<string> mailDeliveredFromMailForTomorrow = new List<string>();

		internal static RenderTarget2D _lightmap;

		public static Texture2D fadeToBlackRect;

		public static Texture2D staminaRect;

		public static Texture2D currentCoopTexture;

		public static Texture2D currentBarnTexture;

		public static Texture2D currentHouseTexture;

		public static Texture2D greenhouseTexture;

		public static Texture2D littleEffect;

		public static SpriteFont dialogueFont;

		public static SpriteFont smallFont;

		public static SpriteFont tinyFont;

		public static SpriteFont tinyFontBorder;

		public static float pickToolInterval;

		public static float screenGlowAlpha = 0f;

		public static float flashAlpha = 0f;

		public static float starCropShimmerPause;

		public static float noteBlockTimer;

		public static int currentGemBirdIndex = 0;

		public Dictionary<string, object> newGameSetupOptions = new Dictionary<string, object>();

		public static bool dialogueUp = false;

		public static bool dialogueTyping = false;

		public static bool pickingTool = false;

		public static bool isQuestion = false;

		public static bool particleRaining = false;

		public static bool newDay = false;

		public static bool inMine = false;

		public static bool menuUp = false;

		public static bool eventUp = false;

		public static bool viewportFreeze = false;

		public static bool eventOver = false;

		public static bool nameSelectUp = false;

		public static bool screenGlow = false;

		public static bool screenGlowHold = false;

		public static bool screenGlowUp;

		public static bool progressBar = false;

		public static bool killScreen = false;

		public static bool coopDwellerBorn;

		public static bool messagePause;

		public static bool boardingBus;

		public static bool listeningForKeyControlDefinitions;

		public static bool weddingToday;

		public static bool exitToTitle;

		public static bool debugMode;

		public static bool displayHUD = true;

		public static bool displayFarmer = true;

		public static bool showKeyHelp;

		public static bool shippingTax;

		public static bool dialogueButtonShrinking;

		public static bool jukeboxPlaying;

		public static bool drawLighting;

		public static bool quit;

		public static bool startedJukeboxMusic;

		public static bool drawGrid;

		public static bool freezeControls;

		public static bool saveOnNewDay;

		public static bool panMode;

		public static bool showingEndOfNightStuff;

		public static bool wasRainingYesterday;

		public static bool hasLoadedGame;

		public static bool isActionAtCurrentCursorTile;

		public static bool isInspectionAtCurrentCursorTile;

		public static bool isSpeechAtCurrentCursorTile;

		public static bool paused;

		public static bool isTimePaused;

		public static bool frameByFrame;

		public static bool lastCursorMotionWasMouse;

		public static bool showingHealth = false;

		public static bool cabinsSeparate = false;

		public static bool hasApplied1_3_UpdateChanges = false;

		public static bool hasApplied1_4_UpdateChanges = false;

		public static bool showingHealthBar = false;

		internal static Action postExitToTitleCallback = null;

		protected int _lastUsedDisplay = -1;

		public bool wasAskedLeoMemory;

		public float controllerSlingshotSafeTime;

		public static BundleType bundleType = BundleType.Default;

		public static bool isRaining = false;

		public static bool isSnowing = false;

		public static bool isLightning = false;

		public static bool isDebrisWeather = false;

		public static int weatherForTomorrow;

		public float zoomModifier = 1f;

		internal static ScreenFade screenFade;

		public static string currentSeason = "spring";

		public static SerializableDictionary<string, string> bannedUsers = new SerializableDictionary<string, string>();

		internal static object _debugOutputLock = new object();

		internal static string _debugOutput;

		public static string requestedMusicTrack = "";

		public static string selectedItemsType;

		public static string nameSelectType;

		public static string messageAfterPause = "";

		public static string fertilizer = "";

		public static string samBandName = "The Alfalfas";

		public static string slotResult;

		public static string keyHelpString = "";

		public static string lastDebugInput = "";

		public static string loadingMessage = "";

		public static string errorMessage = "";

		protected Dictionary<MusicContext, KeyValuePair<string, bool>> _instanceRequestedMusicTracks = new Dictionary<MusicContext, KeyValuePair<string, bool>>(MusicContextComparer.Default);

		protected MusicContext _instanceActiveMusicContext;

		public static bool requestedMusicTrackOverrideable;

		public static bool currentTrackOverrideable;

		public static bool requestedMusicDirty = false;

		protected bool _useUnscaledLighting;

		protected bool _didInitiateItemStow;

		public bool instanceIsOverridingTrack;

		[NonInstancedStatic]
		private static string[] _shortDayDisplayName = new string[7];

		public static Queue<string> currentObjectDialogue = new Queue<string>();

		public static List<string> worldStateIDs = new List<string>();

		public static List<Response> questionChoices = new List<Response>();

		public static int xLocationAfterWarp;

		public static int yLocationAfterWarp;

		public static int gameTimeInterval;

		public static int currentQuestionChoice;

		public static int currentDialogueCharacterIndex;

		public static int dialogueTypingInterval;

		public static int dayOfMonth = 0;

		public static int year = 1;

		public static int timeOfDay = 600;

		public static int timeOfDayAfterFade = -1;

		public static int numberOfSelectedItems = -1;

		public static int priceOfSelectedItem;

		public static int currentWallpaper;

		public static int farmerWallpaper = 22;

		public static int wallpaperPrice = 75;

		public static int currentFloor = 3;

		public static int FarmerFloor = 29;

		public static int floorPrice = 75;

		public static int dialogueWidth;

		public static int menuChoice;

		public static int tvStation = -1;

		public static int currentBillboard;

		public static int facingDirectionAfterWarp;

		public static int tmpTimeOfDay;

		public static int percentageToWinStardewHero = 70;

		public static int mouseClickPolling;

		public static int gamePadXButtonPolling;

		public static int gamePadAButtonPolling;

		public static int weatherIcon;

		public static int hitShakeTimer;

		public static int staminaShakeTimer;

		public static int pauseThenDoFunctionTimer;

		public static int currentSongIndex = 3;

		public static int cursorTileHintCheckTimer;

		public static int timerUntilMouseFade;

		public static int minecartHighScore;

		public static int whichFarm;

		public static int startingCabins;

		public static ModFarmType whichModFarm = null;

		public static ulong? startingGameSeed = null;

		public static int elliottPiano = 0;

		public static SaveGame.SaveFixes lastAppliedSaveFix;

		public static List<int> dealerCalicoJackTotal;

		public static Color morningColor = Color.LightBlue;

		public static Color eveningColor = new Color(255, 255, 0);

		public static Color unselectedOptionColor = new Color(100, 100, 100);

		public static Color screenGlowColor;

		public static NPC currentSpeaker;

		public static Random random = new Random(DateTime.Now.Millisecond);

		public static Random recentMultiplayerRandom = new Random();

		public static IDictionary<int, string> objectInformation;

		public static IDictionary<int, string> bigCraftablesInformation;

		public static IDictionary<int, string> clothingInformation;

		public static IDictionary<string, string> objectContextTags;

		public static List<HUDMessage> hudMessages = new List<HUDMessage>();

		public static IDictionary<string, string> NPCGiftTastes;

		public static float musicPlayerVolume;

		public static float ambientPlayerVolume;

		public static float pauseAccumulator;

		public static float pauseTime;

		public static float upPolling;

		public static float downPolling;

		public static float rightPolling;

		public static float leftPolling;

		public static float debrisSoundInterval;

		public static float toolHold;

		public static float windGust;

		public static float dialogueButtonScale = 1f;

		public static float creditsTimer;

		public static float globalOutdoorLighting;

		public ICue instanceCurrentSong;

		public static IAudioCategory musicCategory;

		public static IAudioCategory soundCategory;

		public static IAudioCategory ambientCategory;

		public static IAudioCategory footstepCategory;

		public PlayerIndex instancePlayerOneIndex;

		[NonInstancedStatic]
		public static IAudioEngine audioEngine;

		[NonInstancedStatic]
		public static WaveBank waveBank;

		[NonInstancedStatic]
		public static WaveBank waveBank1_4;

		[NonInstancedStatic]
		public static ISoundBank soundBank;

		public static Vector2 shiny = Vector2.Zero;

		public static Vector2 previousViewportPosition;

		public static Vector2 currentCursorTile;

		public static Vector2 lastCursorTile = Vector2.Zero;

		public static Vector2 snowPos;

		public Microsoft.Xna.Framework.Rectangle localMultiplayerWindow;

		public static List<RainDrop> rainDrops = new List<RainDrop>();

		public static double chanceToRainTomorrow = 0.0;

		public static ICue chargeUpSound;

		public static ICue wind;

		public static NetAudioCueManager locationCues = new NetAudioCueManager();

		public static int baseDebrisWeatherCount = 0;

		public static List<WeatherDebris> debrisWeatherPool = new List<WeatherDebris>();

		public static List<RainDrop> rainDropPool = new List<RainDrop>();

		public static List<WeatherDebris> debrisWeather = new List<WeatherDebris>();

		public static List<TemporaryAnimatedSprite> screenOverlayTempSprites = new List<TemporaryAnimatedSprite>();

		public static List<TemporaryAnimatedSprite> uiOverlayTempSprites = new List<TemporaryAnimatedSprite>();

		internal static byte _gameMode;

		private bool _isSaving;

		protected internal static Multiplayer multiplayer = new Multiplayer();

		public static byte multiplayerMode;

		public static IEnumerator<int> currentLoader;

		public static ulong uniqueIDForThisGame = Utility.NewUniqueIdForThisGame();

		public static int[] cropsOfTheWeek;

		public static int[] directionKeyPolling = new int[4];

		public static Quest questOfTheDay;

		public static MoneyMadeScreen moneyMadeScreen;

		public static HashSet<LightSource> currentLightSources = new HashSet<LightSource>();

		public static Color ambientLight;

		public static Color outdoorLight = new Color(255, 255, 0);

		public static Color textColor = new Color(34, 17, 34);

		public static Color textShadowColor = new Color(206, 156, 95);

		public static IClickableMenu overlayMenu;

		internal static IClickableMenu _activeClickableMenu;

		public static bool isCheckingNonMousePlacement = false;

		internal static IMinigame _currentMinigame = null;

		protected static float _beforeMinigameScale = 0f;

		public static List<IClickableMenu> onScreenMenus = new List<IClickableMenu>();

		private const int _fpsHistory = 120;

		internal static List<float> _fpsList = new List<float>(120);

		internal static Stopwatch _fpsStopwatch = new Stopwatch();

		internal static float _fps = 0f;

		public static Dictionary<int, string> achievements;

		public static BuffsDisplay buffsDisplay;

		public static DayTimeMoneyBox dayTimeMoneyBox;

		public static NetRootDictionary<long, Farmer> otherFarmers;

		private static readonly FarmerCollection _onlineFarmers = new FarmerCollection();

		public static IGameServer server;

		public static Client client;

		public KeyboardDispatcher instanceKeyboardDispatcher;

		public static Background background;

		public static FarmEvent farmEvent;

		public static afterFadeFunction afterFade;

		public static afterFadeFunction afterDialogues;

		public static afterFadeFunction afterViewport;

		public static afterFadeFunction viewportReachedTarget;

		public static afterFadeFunction afterPause;

		public static GameTime currentGameTime;

		public static IList<DelayedAction> delayedActions = new List<DelayedAction>();

		public static Stack<IClickableMenu> endOfNightMenus = new Stack<IClickableMenu>();

		public Options instanceOptions;

		[NonInstancedStatic]
		public static SerializableDictionary<long, Options> splitscreenOptions = new SerializableDictionary<long, Options>();

		public static Game1 game1;

		public static Point lastMousePositionBeforeFade;

		public static int ticks;

		public static EmoteMenu emoteMenu;

		[NonInstancedStatic]
		public static SerializableDictionary<string, string> CustomData = new SerializableDictionary<string, string>();

		public static NetRoot<IWorldState> netWorldState;

		public static ChatBox chatBox;

		public TextEntryMenu instanceTextEntry;

		public static SpecialCurrencyDisplay specialCurrencyDisplay = null;

		public LocalCoopJoinMenu localCoopJoinMenu;

		public static bool drawbounds;

		internal static string debugPresenceString;

		public static List<Action> remoteEventQueue = new List<Action>();

		public static List<long> weddingsToday = new List<long>();

		public int instanceIndex;

		public int instanceId;

		public static bool overrideGameMenuReset;

		protected bool _windowResizing;

		protected Point _oldMousePosition;

		protected bool _oldGamepadConnectedState;

		protected int _oldScrollWheelValue;

		public static Point viewportCenter;

		public static Vector2 viewportTarget = new Vector2(-2.1474836E+09f, -2.1474836E+09f);

		public static float viewportSpeed = 2f;

		public static int viewportHold;

		internal static bool _cursorDragEnabled = false;

		internal static bool _cursorDragPrevEnabled = false;

		internal static bool _cursorSpeedDirty = true;

		private const float CursorBaseSpeed = 16f;

		internal static float _cursorSpeed = 16f;

		internal static float _cursorSpeedScale = 1f;

		internal static float _cursorUpdateElapsedSec = 0f;

		internal static int thumbstickPollingTimer;

		public static bool toggleFullScreen;

		public static string whereIsTodaysFest;

		public const string NO_LETTER_MAIL = "%&NL&%";

		public const string BROADCAST_MAIL_FOR_TOMORROW_PREFIX = "%&MFT&%";

		public const string BROADCAST_SEEN_MAIL_PREFIX = "%&SM&%";

		public const string BROADCAST_MAILBOX_PREFIX = "%&MB&%";

		public bool isLocalMultiplayerNewDayActive;

		internal static Task _newDayTask;

		internal static Action _afterNewDayAction;

		public static NewDaySynchronizer newDaySync;

		public static bool forceSnapOnNextViewportUpdate = false;

		public static Vector2 currentViewportTarget;

		public static Vector2 viewportPositionLerp;

		public static float screenGlowRate = 0.005f;

		public static float screenGlowMax;

		public static bool haltAfterCheck = false;

		public static bool uiMode = false;

		public static RenderTarget2D nonUIRenderTarget = null;

		public static int uiModeCount = 0;

		internal static int _oldUIModeCount = 0;

		private string panModeString;

		public static bool conventionMode = false;

		private EventTest eventTest;

		private bool panFacingDirectionWait;

		public static bool isRunningMacro = false;

		public static int thumbstickMotionMargin;

		public static float thumbstickMotionAccell = 1f;

		public static int triggerPolling;

		public static int rightClickPolling;

		private RenderTarget2D _screen;

		private RenderTarget2D _uiScreen;

		public static Color bgColor = new Color(5, 3, 4);

		protected readonly BlendState lightingBlend = new BlendState
		{
			ColorBlendFunction = BlendFunction.ReverseSubtract,
			ColorDestinationBlend = Blend.One,
			ColorSourceBlend = Blend.SourceColor
		};

		public bool isDrawing;

		[NonInstancedStatic]
		public static bool isRenderingScreenBuffer = false;

		protected bool _lastDrewMouseCursor;

		internal static int _activatedTick = 0;

		public static int mouseCursor = 0;

		internal static float _mouseCursorTransparency = 1f;

		public static bool wasMouseVisibleThisFrame = true;

		public static NPC objectDialoguePortraitPerson;

		public static bool IsActiveClickableMenuNativeScaled
		{
			get
			{
				if (!(activeClickableMenu is AnimalQueryMenu) && !(activeClickableMenu is Billboard) && !(activeClickableMenu is BuffsDisplay) && !(activeClickableMenu is CarpenterMenu) && !(activeClickableMenu is ChooseFromListMenu) && !(activeClickableMenu is ConfirmationDialog) && !(activeClickableMenu is ItemListMenu) && !(activeClickableMenu is DialogueBox) && !(activeClickableMenu is GeodeMenu) && !(activeClickableMenu is DyeMenu) && !(activeClickableMenu is TailoringMenu) && !(activeClickableMenu is ItemGrabMenu) && !(activeClickableMenu is JojaCDMenu) && !(activeClickableMenu is JunimoNoteMenu) && !(activeClickableMenu is LetterViewerMenu) && !(activeClickableMenu is LevelUpMenu) && !(activeClickableMenu is MineElevatorMenu) && !(activeClickableMenu is MuseumMenu) && !(activeClickableMenu is NamingMenu) && !(activeClickableMenu is NumberSelectionMenu) && !(activeClickableMenu is PurchaseAnimalsMenu) && !(activeClickableMenu is SaveGameMenu) && !(activeClickableMenu is ShippingMenu) && !(activeClickableMenu is PondQueryMenu) && !(activeClickableMenu is VirtualJoypad))
				{
					return activeClickableMenu is WheelSpinGame;
				}
				return true;
			}
		}

		public static bool IsActiveClickableMenuUnscaled
		{
			get
			{
				if (!(activeClickableMenu is BobberBar))
				{
					return activeClickableMenu is TutorialMenu;
				}
				return true;
			}
		}

		public static float NativeZoomLevel => MobileDisplay.ZoomScale;

		public static float DateTimeScale
		{
			get
			{
				if (options != null)
				{
					return options.dateTimeScale;
				}
				return DefaultMenuButtonScale;
			}
		}

		public static float DefaultMenuButtonScale => MobileDisplay.MenuButtonScale;

		public bool IsActiveNoOverlay
		{
			get
			{
				if (!base.IsActive)
				{
					return false;
				}
				if (Program.sdk.HasOverlay)
				{
					return false;
				}
				return true;
			}
		}

		public static LocalizedContentManager temporaryContent
		{
			get
			{
				if (_temporaryContent == null)
				{
					_temporaryContent = content.CreateTemporary();
				}
				return _temporaryContent;
			}
		}

		public static Farmer player
		{
			get
			{
				return _player;
			}
			set
			{
				if (_player != null)
				{
					_player.unload();
					_player = null;
				}
				_player = value;
			}
		}

		public static bool isWarping => _isWarping;

		public static List<GameLocation> locations => game1._locations;

		public static GameLocation currentLocation
		{
			get
			{
				return game1.instanceGameLocation;
			}
			set
			{
				game1.instanceGameLocation = value;
			}
		}

		public static Texture2D toolSpriteSheet
		{
			get
			{
				if (_toolSpriteSheet == null)
				{
					ResetToolSpriteSheet();
				}
				return _toolSpriteSheet;
			}
		}

		public static RenderTarget2D lightmap => _lightmap;

		public static bool spawnMonstersAtNight
		{
			get
			{
				return player.team.spawnMonstersAtNight;
			}
			set
			{
				player.team.spawnMonstersAtNight.Value = value;
			}
		}

		public static bool fadeToBlack
		{
			get
			{
				return screenFade.fadeToBlack;
			}
			set
			{
				screenFade.fadeToBlack = value;
			}
		}

		public static bool fadeIn
		{
			get
			{
				return screenFade.fadeIn;
			}
			set
			{
				screenFade.fadeIn = value;
			}
		}

		public static bool globalFade
		{
			get
			{
				return screenFade.globalFade;
			}
			set
			{
				screenFade.globalFade = value;
			}
		}

		public static bool nonWarpFade
		{
			get
			{
				return screenFade.nonWarpFade;
			}
			set
			{
				screenFade.nonWarpFade = value;
			}
		}

		public static float fadeToBlackAlpha
		{
			get
			{
				return screenFade.fadeToBlackAlpha;
			}
			set
			{
				screenFade.fadeToBlackAlpha = value;
			}
		}

		public static float globalFadeSpeed
		{
			get
			{
				return screenFade.globalFadeSpeed;
			}
			set
			{
				screenFade.globalFadeSpeed = value;
			}
		}

		public static string CurrentSeasonDisplayName => content.LoadString("Strings\\StringsFromCSFiles:" + currentSeason);

		public static string debugOutput
		{
			get
			{
				return _debugOutput;
			}
			set
			{
				lock (_debugOutputLock)
				{
					if (_debugOutput != value)
					{
						_debugOutput = value;
						if (!string.IsNullOrEmpty(_debugOutput))
						{
							Console.WriteLine("DebugOutput: {0}", _debugOutput);
						}
					}
				}
			}
		}

		public static string elliottBookName
		{
			get
			{
				if (player != null && player.DialogueQuestionsAnswered.Contains(958699))
				{
					return content.LoadString("Strings\\Events:ElliottBook_mystery");
				}
				if (player != null && player.DialogueQuestionsAnswered.Contains(958700))
				{
					return content.LoadString("Strings\\Events:ElliottBook_romance");
				}
				return content.LoadString("Strings\\Events:ElliottBook_default");
			}
			set
			{
			}
		}

		protected static Dictionary<MusicContext, KeyValuePair<string, bool>> _requestedMusicTracks
		{
			get
			{
				return game1._instanceRequestedMusicTracks;
			}
			set
			{
				game1._instanceRequestedMusicTracks = value;
			}
		}

		protected static MusicContext _activeMusicContext
		{
			get
			{
				return game1._instanceActiveMusicContext;
			}
			set
			{
				game1._instanceActiveMusicContext = value;
			}
		}

		public static bool isOverridingTrack
		{
			get
			{
				return game1.instanceIsOverridingTrack;
			}
			set
			{
				game1.instanceIsOverridingTrack = value;
			}
		}

		public bool useUnscaledLighting
		{
			get
			{
				return _useUnscaledLighting;
			}
			set
			{
				if (_useUnscaledLighting != value)
				{
					_useUnscaledLighting = value;
					allocateLightmap(localMultiplayerWindow.Width, localMultiplayerWindow.Height);
				}
			}
		}

		public static IList<string> mailbox => player.mailbox;

		public static ICue currentSong
		{
			get
			{
				return game1.instanceCurrentSong;
			}
			set
			{
				game1.instanceCurrentSong = value;
			}
		}

		public static PlayerIndex playerOneIndex
		{
			get
			{
				return game1.instancePlayerOneIndex;
			}
			set
			{
				game1.instancePlayerOneIndex = value;
			}
		}

		public static byte gameMode
		{
			get
			{
				return _gameMode;
			}
			set
			{
				if (_gameMode != value)
				{
					Console.WriteLine("gameMode was '{0}', set to '{1}'.", GameModeToString(_gameMode), GameModeToString(value));
					_gameMode = value;
				}
			}
		}

		public bool IsSaving
		{
			get
			{
				return _isSaving;
			}
			set
			{
				_isSaving = value;
			}
		}

		public static Stats stats => player.stats;

		public static IClickableMenu activeClickableMenu
		{
			get
			{
				return _activeClickableMenu;
			}
			set
			{
				if (_activeClickableMenu is IDisposable && !_activeClickableMenu.HasDependencies())
				{
					(_activeClickableMenu as IDisposable).Dispose();
				}
				if (_activeClickableMenu != null && value == null)
				{
					timerUntilMouseFade = 0;
				}
				if (textEntry != null && _activeClickableMenu != value)
				{
					closeTextEntry();
				}
				_activeClickableMenu = value;
				if (_activeClickableMenu != null && (!eventUp || (CurrentEvent != null && CurrentEvent.playerControlSequence && !player.UsingTool)))
				{
					player.Halt();
				}
			}
		}

		public static IMinigame currentMinigame
		{
			get
			{
				return _currentMinigame;
			}
			set
			{
				if (_currentMinigame != null && value == null && _beforeMinigameScale != 0f)
				{
					PinchZoom.Instance.SetZoomLevel(_beforeMinigameScale);
					float num = (options.baseZoomLevel = (options.desiredBaseZoomLevel = _beforeMinigameScale));
					game1.Window_ClientSizeChanged(null, null);
					_beforeMinigameScale = 0f;
				}
				_currentMinigame = value;
				if (_currentMinigame != null && _beforeMinigameScale == 0f && _currentMinigame.GetForcedScaleFactor() != -1f)
				{
					_beforeMinigameScale = options.desiredBaseZoomLevel;
					PinchZoom.Instance.SetZoomLevel(_currentMinigame.GetForcedScaleFactor());
					float num = (options.baseZoomLevel = (options.desiredBaseZoomLevel = _currentMinigame.GetForcedScaleFactor()));
					game1.Window_ClientSizeChanged(null, null);
				}
				if (value == null)
				{
					if (currentLocation != null)
					{
						setRichPresence("location", currentLocation.Name);
						currentLocation.tapToMove.Reset();
					}
					randomizeDebrisWeatherPositions(debrisWeather);
					randomizeRainPositions();
				}
				else if (value.minigameId() != null)
				{
					setRichPresence("minigame", value.minigameId());
				}
			}
		}

		public static Object dishOfTheDay
		{
			get
			{
				return netWorldState.Value.DishOfTheDay.Value;
			}
			set
			{
				netWorldState.Value.DishOfTheDay.Value = value;
			}
		}

		public static KeyboardDispatcher keyboardDispatcher
		{
			get
			{
				return game1.instanceKeyboardDispatcher;
			}
			set
			{
				game1.instanceKeyboardDispatcher = value;
			}
		}

		public static Options options
		{
			get
			{
				return game1.instanceOptions;
			}
			set
			{
				game1.instanceOptions = value;
			}
		}

		public static TextEntryMenu textEntry
		{
			get
			{
				return game1.instanceTextEntry;
			}
			set
			{
				game1.instanceTextEntry = value;
			}
		}

		public static WorldDate Date => netWorldState.Value.Date;

		public static bool NetTimePaused => netWorldState.Get().IsTimePaused;

		public static bool HostPaused => netWorldState.Get().IsPaused;

		public static bool IsMultiplayer => otherFarmers.Count > 0;

		public static bool IsClient => multiplayerMode == 1;

		public static bool IsServer => multiplayerMode == 2;

		public static bool IsMasterGame
		{
			get
			{
				if (multiplayerMode != 0)
				{
					return multiplayerMode == 2;
				}
				return true;
			}
		}

		public static bool IsOnlineMultiplayer
		{
			get
			{
				if (server != null)
				{
					if (server.IsLocalMultiplayerInitiatedServer())
					{
						return false;
					}
					return true;
				}
				if (client != null)
				{
					if (client is LoopbackClient)
					{
						return false;
					}
					return true;
				}
				return false;
			}
		}

		public static Farmer MasterPlayer
		{
			get
			{
				if (!IsMasterGame)
				{
					return serverHost.Value;
				}
				return player;
			}
		}

		public static bool IsChatting
		{
			get
			{
				if (chatBox != null)
				{
					return chatBox.isActive();
				}
				return false;
			}
			set
			{
				if (value != chatBox.isActive())
				{
					if (value)
					{
						chatBox.activate();
					}
					else
					{
						chatBox.clickAway();
					}
				}
			}
		}

		public static Event CurrentEvent
		{
			get
			{
				if (currentLocation == null)
				{
					return null;
				}
				return currentLocation.currentEvent;
			}
		}

		public static MineShaft mine
		{
			get
			{
				if (locationRequest != null && locationRequest.Location is MineShaft)
				{
					return locationRequest.Location as MineShaft;
				}
				if (currentLocation is MineShaft)
				{
					return currentLocation as MineShaft;
				}
				return null;
			}
		}

		public static int CurrentMineLevel
		{
			get
			{
				if (currentLocation is MineShaft)
				{
					return (currentLocation as MineShaft).mineLevel;
				}
				return 0;
			}
		}

		public static int CurrentPlayerLimit
		{
			get
			{
				if (netWorldState == null || netWorldState.Value == null || netWorldState.Value.CurrentPlayerLimit == null)
				{
					return multiplayer.playerLimit;
				}
				return netWorldState.Value.CurrentPlayerLimit.Value;
			}
		}

		private static float thumbstickToMouseModifier
		{
			get
			{
				if (_cursorSpeedDirty)
				{
					ComputeCursorSpeed();
				}
				return _cursorSpeed / 720f * (float)viewport.Height * (float)currentGameTime.ElapsedGameTime.TotalSeconds;
			}
		}

		public static bool isFullscreen => graphics.IsFullScreen;

		public static bool IsSummer => currentSeason.Equals("summer");

		public static bool IsSpring => currentSeason.Equals("spring");

		public static bool IsFall => currentSeason.Equals("fall");

		public static bool IsWinter => currentSeason.Equals("winter");

		public RenderTarget2D screen
		{
			get
			{
				return _screen;
			}
			set
			{
				if (_screen != null)
				{
					_screen.Dispose();
					_screen = null;
				}
				_screen = value;
			}
		}

		public RenderTarget2D uiScreen
		{
			get
			{
				return _uiScreen;
			}
			set
			{
				if (_uiScreen != null)
				{
					_uiScreen.Dispose();
					_uiScreen = null;
				}
				_uiScreen = value;
			}
		}

		public static float mouseCursorTransparency
		{
			get
			{
				return _mouseCursorTransparency;
			}
			set
			{
				_mouseCursorTransparency = value;
			}
		}

		public static bool ShowJustTheMinimalButtons
		{
			get
			{
				if (activeClickableMenu == null && CurrentEvent != null && !CurrentEvent.skippable && currentMinigame == null && !globalFade && !freezeControls && currentLocation.Name != "Mine")
				{
					return currentLocation.Name != "IslandSouth";
				}
				return false;
			}
		}

		public static bool isGamePadConnected()
		{
			if (input != null)
			{
				return input.GetGamePadState().IsConnected;
			}
			return false;
		}

		public static void InitializeRunner()
		{
			Context context = Application.Context;
			string absolutePath = context.GetExternalFilesDir(null).AbsolutePath;
			savesPath = (hiddenSavesPath = Path.Combine(absolutePath, "Saves"));
			screenshotsPath = Path.Combine(absolutePath, "Screenshots");
			if (!Directory.Exists(savesPath))
			{
				Directory.CreateDirectory(savesPath);
			}
			if (!Directory.Exists(hiddenSavesPath))
			{
				Directory.CreateDirectory(hiddenSavesPath);
			}
			if (!Directory.Exists(screenshotsPath))
			{
				Directory.CreateDirectory(screenshotsPath);
			}
		}

		private void MobileLoadContent()
		{
			mobileSpriteSheet = content.Load<Texture2D>("LooseSprites\\MobileAtlas_manually_made");
			virtualJoypad = new VirtualJoypad();
			clientBounds = viewport.ToXna();
		}

		private void _updateTapToMove(GameTime gameTime, MouseState currentMouseState)
		{
			GameLocation location = currentLocation;
			if (location != null)
			{
				if (currentMinigame is FishingGame)
				{
					location = ((FishingGame)currentMinigame).location;
				}
				else if (currentMinigame is TargetGame)
				{
					location = ((TargetGame)currentMinigame).location;
				}
				if (currentMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
				{
					location.tapToMove.OnTap(getMouseX(), getMouseY(), viewport.X, viewport.Y);
				}
				else if (currentMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Pressed)
				{
					location.tapToMove.OnTapHeld(getMouseX(), getMouseY(), viewport.X, viewport.Y);
				}
				else if (currentMouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
				{
					location.tapToMove.OnTapRelease(getMouseX(), getMouseY(), viewport.X, viewport.Y);
				}
				location.tapToMove.Update();
				if (currentMinigame is FishingGame)
				{
					((FishingGame)currentMinigame).ReceiveMobileKeyStates(location.tapToMove.mobileKeyStates);
				}
				else if (currentMinigame is TargetGame)
				{
					((TargetGame)currentMinigame).ReceiveMobileKeyStates(location.tapToMove.mobileKeyStates);
				}
			}
		}

		private static void SetCurrentViewportTargetToCenterOnPlayer()
		{
			if (currentLocation != null)
			{
				int num = Math.Min(0, (currentLocation.Map.DisplayWidth - viewport.Width) / 2);
				if (options.verticalToolbar && displayHUD && Toolbar.visible)
				{
					float num2 = (float)Toolbar.toolbarWidth * options.uiScale / options.zoomLevel;
					num = Math.Min(-(int)num2, (currentLocation.Map.DisplayWidth - viewport.Width) / 2);
				}
				int num3 = currentLocation.Map.DisplayWidth - viewport.Width;
				int num4 = Math.Min(0, (currentLocation.Map.DisplayHeight - viewport.Height) / 2);
				int num5 = currentLocation.Map.DisplayHeight - viewport.Height;
				currentViewportTarget.X = player.getStandingX() - viewport.Width / 2;
				currentViewportTarget.Y = player.getStandingY() - viewport.Height / 2;
				currentViewportTarget.X = Math.Max(num, Math.Min(currentViewportTarget.X, num3));
				currentViewportTarget.Y = Math.Max(num4, Math.Min(currentViewportTarget.Y, num5));
			}
		}

		private void _mobileUpdateControlInput(MouseState currentMouseState, out TapState tapState, ref bool moveupPressed, ref bool movedownPressed, ref bool moveleftPressed, ref bool moverightPressed, ref bool moveupReleased, ref bool movedownReleased, ref bool moveleftReleased, ref bool moverightReleased, ref bool moveupHeld, ref bool movedownHeld, ref bool moveleftHeld, ref bool moverightHeld, ref bool actionButtonPressed, ref bool useToolButtonPressed, ref bool useToolButtonReleased, ref bool useToolHeld)
		{
			tapState = TapState.None;
			if (currentMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
			{
				tapState = TapState.TapDown;
			}
			else if (currentMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Pressed)
			{
				tapState = TapState.TapHeld;
			}
			else if (currentMouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
			{
				tapState = TapState.TapReleased;
			}
			currentLocation.tapToMove.Update();
			if (moveupPressed | movedownPressed | moveleftPressed | moverightPressed | moveupReleased | movedownReleased | moveleftReleased | moverightReleased | moveupHeld | movedownHeld | moveleftHeld | moverightHeld)
			{
				currentLocation.tapToMove.Reset();
				currentLocation.tapToMove.mobileKeyStates.moveUpPressed = moveupPressed;
				currentLocation.tapToMove.mobileKeyStates.moveDownPressed = movedownPressed;
				currentLocation.tapToMove.mobileKeyStates.moveLeftPressed = moveleftPressed;
				currentLocation.tapToMove.mobileKeyStates.moveRightPressed = moverightPressed;
				currentLocation.tapToMove.mobileKeyStates.moveUpReleased = moveupReleased;
				currentLocation.tapToMove.mobileKeyStates.moveDownReleased = movedownReleased;
				currentLocation.tapToMove.mobileKeyStates.moveLeftReleased = moveleftReleased;
				currentLocation.tapToMove.mobileKeyStates.moveRightReleased = moverightReleased;
				currentLocation.tapToMove.mobileKeyStates.moveUpHeld = moveupHeld;
				currentLocation.tapToMove.mobileKeyStates.moveDownHeld = movedownHeld;
				currentLocation.tapToMove.mobileKeyStates.moveLeftHeld = moveleftHeld;
				currentLocation.tapToMove.mobileKeyStates.moveRightHeld = moverightHeld;
			}
			else if (currentLocation.tapToMove.Moving)
			{
				moveupPressed = currentLocation.tapToMove.mobileKeyStates.moveUpPressed;
				movedownPressed = currentLocation.tapToMove.mobileKeyStates.moveDownPressed;
				moveleftPressed = currentLocation.tapToMove.mobileKeyStates.moveLeftPressed;
				moverightPressed = currentLocation.tapToMove.mobileKeyStates.moveRightPressed;
				moveupReleased = currentLocation.tapToMove.mobileKeyStates.moveUpReleased;
				movedownReleased = currentLocation.tapToMove.mobileKeyStates.moveDownReleased;
				moveleftReleased = currentLocation.tapToMove.mobileKeyStates.moveLeftReleased;
				moverightReleased = currentLocation.tapToMove.mobileKeyStates.moveRightReleased;
				moveupHeld = currentLocation.tapToMove.mobileKeyStates.moveUpHeld;
				movedownHeld = currentLocation.tapToMove.mobileKeyStates.moveDownHeld;
				moveleftHeld = currentLocation.tapToMove.mobileKeyStates.moveLeftHeld;
				moverightHeld = currentLocation.tapToMove.mobileKeyStates.moveRightHeld;
			}
			if (currentLocation.tapToMove.mobileKeyStates.useToolButtonPressed)
			{
				useToolButtonPressed = true;
			}
			if (currentLocation.tapToMove.mobileKeyStates.useToolHeld)
			{
				useToolHeld = true;
			}
			if (currentLocation.tapToMove.mobileKeyStates.useToolButtonReleased)
			{
				useToolButtonReleased = true;
			}
			if (currentLocation.tapToMove.mobileKeyStates.actionButtonPressed)
			{
				actionButtonPressed = true;
			}
		}

		private void _mobileProcessTaps(TapState tapState)
		{
			switch (tapState)
			{
			case TapState.TapDown:
				currentLocation.tapToMove.OnTap(getMouseX(), getMouseY(), viewport.X, viewport.Y);
				break;
			case TapState.TapHeld:
				currentLocation.tapToMove.OnTapHeld(getMouseX(), getMouseY(), viewport.X, viewport.Y);
				break;
			case TapState.TapReleased:
				currentLocation.tapToMove.OnTapRelease(getMouseX(), getMouseY(), viewport.X, viewport.Y);
				break;
			}
		}

		public static bool GetHasRoomAnotherFarm()
		{
			return true;
		}

		private void askedToQuit(Farmer who)
		{
			TutorialManager.Instance.stopTutorialsTemporarily();
			ExitToTitle();
		}

		private void _updateMobileMenus()
		{
			GamePadState gamePadState = input.GetGamePadState();
			if ((gamePadState.IsConnected && gamePadState.Buttons.Back == ButtonState.Pressed && _lastBackButtonState == ButtonState.Released) || (!gamePadState.IsConnected && gamePadState.Buttons.Back == ButtonState.Pressed))
			{
				_lastBackButtonState = ButtonState.Pressed;
				_ = currentMinigame;
			}
			else if (_lastBackButtonState == ButtonState.Pressed && gamePadState.Buttons.Back == ButtonState.Released)
			{
				_lastBackButtonState = ButtonState.Released;
			}
		}

		public static void LoadNewGameFromCharacterCustomization()
		{
			loadForNewGame();
			saveOnNewDay = true;
			player.eventsSeen.Add(60367);
			player.currentLocation = Utility.getHomeOfFarmer(player);
			player.Position = new Vector2(9f, 9f) * 64f;
			player.isInBed.Value = true;
			NewDay(0f);
			exitActiveMenu();
			setGameMode(3);
			displayFarmer = true;
			displayHUD = true;
		}

		private void DrawTapToMoveTarget()
		{
			if (activeClickableMenu != null && currentLocation.tapToMove.TileClicked.X != -1f)
			{
				currentLocation.tapToMove.Reset();
			}
			else if (player.canMove && currentLocation.tapToMove.TileClicked.X != -1f && currentLocation.tapToMove.targetNPC == null && currentLocation.tapToMove.targetFarmAnimal == null)
			{
				Vector2 vector = new Vector2(currentLocation.tapToMove.TileClicked.X * 64f - (float)viewport.X, currentLocation.tapToMove.TileClicked.Y * 64f - (float)viewport.Y);
				long num = DateTime.Now.Ticks;
				if (num - _greenSquareLastUpdateTicks > 1250000)
				{
					_greenSquareLastUpdateTicks = num;
					_greenSquareAnimIndex++;
					if (_greenSquareAnimIndex > 7)
					{
						_greenSquareAnimIndex = 0;
					}
				}
				spriteBatch.Draw(mobileSpriteSheet, vector + new Vector2(0f, -12f), new Microsoft.Xna.Framework.Rectangle(_greenSquareAnimIndex * 16, 201, 16, 20), Color.White * mouseCursorTransparency, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.58f);
			}
			else if (player.canMove && currentLocation.tapToMove.noPathHere.X != -1f && currentLocation.tapToMove.targetNPC == null)
			{
				Vector2 position = new Vector2(currentLocation.tapToMove.noPathHere.X * 64f - (float)viewport.X, currentLocation.tapToMove.noPathHere.Y * 64f - (float)viewport.Y);
				spriteBatch.Draw(mouseCursors, position, new Microsoft.Xna.Framework.Rectangle(210, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
			}
		}

		private void DrawGreenPlacementBounds()
		{
			CheckToClearLastObjectGreenPlacementSquares();
			if (activeClickableMenu == null && mouseCursor > -1 && (input.GetMouseState().X != 0 || input.GetMouseState().Y != 0) && (getOldMouseX() != 0 || getOldMouseY() != 0) && player.ActiveObject != null && mouseCursor != 3 && !eventUp && (mouseCursorTransparency > 0f || options.showPlacementTileForGamepad))
			{
				spriteBatch.End();
				PopUIMode();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				player.ActiveObject.drawPlacementBounds(spriteBatch, currentLocation);
				spriteBatch.End();
				PushUIMode();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			}
		}

		public void CheckToClearLastObjectGreenPlacementSquares()
		{
			if (_lastActiveObject != player.ActiveObject)
			{
				if (_lastActiveObject != null)
				{
					_lastActiveObject.ClearRedGreenSquareDict();
				}
				_lastActiveObject = player.ActiveObject;
				if (_lastActiveObject != null)
				{
					_lastActiveObject.ClearRedGreenSquareDict();
				}
			}
			if (_lastLocation != currentLocation.name.Value)
			{
				_lastLocation = currentLocation.name.Value;
				if (player.ActiveObject != null)
				{
					player.ActiveObject.ClearRedGreenSquareDict();
				}
			}
		}

		public void CreateMusicWaveBank()
		{
			if (musicWaveBankState == MusicWaveBankState.Created || musicWaveBankState == MusicWaveBankState.NotInitialised)
			{
				return;
			}
			string text = FetchMusicXWBPath();
			if (text == null || !(text != string.Empty) || audioEngine == null)
			{
				return;
			}
			try
			{
				WaveBank waveBank = new WaveBank(audioEngine.Engine, text, 0, 2);
				musicWaveBankState = MusicWaveBankState.Created;
				audioEngine.Update();
				musicCategory = audioEngine.GetCategory("Music");
				if (_pendingTrackName != null)
				{
					changeMusicTrack(_pendingTrackName);
					_pendingTrackName = null;
				}
			}
			catch (Exception)
			{
				musicWaveBankState = MusicWaveBankState.NotDownloaded;
			}
		}

		private void InitializeMusicWaveBank()
		{
			string text = FetchMusicXWBPath();
			musicWaveBankState = MusicWaveBankState.NotDownloaded;
			if (!(text != string.Empty))
			{
				return;
			}
			try
			{
				WaveBank waveBank = new WaveBank(audioEngine.Engine, FetchMusicXWBPath(), 0, 2);
				musicWaveBankState = MusicWaveBankState.Created;
			}
			catch (Exception ex)
			{
				Log.It("Game1.Initialize filePath:" + text + ", exception:" + ex);
				musicWaveBankState = MusicWaveBankState.NotDownloaded;
				if (text.Contains("pdalife.ru") || text.Contains("lenov.ru"))
				{
					try
					{
						Intent intent = new Intent("android.intent.action.VIEW", Android.Net.Uri.Parse("https://play.google.com/store/apps/details?id=com.chucklefish.stardewvalley&hl=ru"));
						intent.AddFlags(ActivityFlags.NewTask);
						Application.Context.StartActivity(intent);
						return;
					}
					catch (Exception)
					{
						return;
					}
				}
			}
		}

		private string FetchMusicXWBPath()
		{
			string empty = string.Empty;
			return Path.Combine(base.Content.RootDirectory, "XACT", "music.xwb");
		}

		private void _updateTutorialManager(GameTime gameTime)
		{
			TutorialManager.Instance.TapLeaveHouseCheck();
			TutorialManager.Instance.DummyInteractShopCheck();
			TutorialManager.Instance.update(gameTime);
		}

		public static void loadTitleTexture()
		{
			titleButtonsTexture = temporaryContent.Load<Texture2D>("Minigames\\TitleButtons");
			logoFadeTimer = 4500;
		}

		public static void saveWholeBackup()
		{
			if (SaveGame.saveInProgress || CurrentEvent != null || eventUp || currentLocation is MovieTheater)
			{
				return;
			}
			wholeBackupLoader = SaveGame.Save(wholeBackup: true);
			while (wholeBackupLoader != null)
			{
				if (!wholeBackupLoader.MoveNext())
				{
					wholeBackupLoader = null;
				}
			}
		}

		public static void emergencyBackup()
		{
			if (SaveGame.saveInProgress)
			{
				if (currentLoader != null)
				{
					while (currentLoader != null)
					{
						if (!currentLoader.MoveNext())
						{
							currentLoader = null;
						}
					}
				}
				else
				{
					if (wholeBackupLoader == null)
					{
						return;
					}
					while (wholeBackupLoader != null)
					{
						if (!wholeBackupLoader.MoveNext())
						{
							wholeBackupLoader = null;
						}
					}
				}
			}
			else if (gameMode == 3 && !SaveGame.saveInProgress && timeOfDay > 620 && MainActivity.instance.HasPermissions)
			{
				MakeFullBackup();
			}
		}

		private void _updateWholeBackupLoader(GameTime gameTime)
		{
			if (wholeBackupLoader != null && !wholeBackupLoader.MoveNext())
			{
				wholeBackupLoader = null;
			}
		}

		public static void MakeFullBackup()
		{
			wholeBackupLoader = SaveGame.Save(wholeBackup: false, emergencyBackup: true);
			while (wholeBackupLoader != null)
			{
				try
				{
					if (!wholeBackupLoader.MoveNext())
					{
						wholeBackupLoader = null;
					}
				}
				catch (UnauthorizedAccessException)
				{
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		public void OnAppPause()
		{
			if (currentLocation != null)
			{
				currentLocation.tapToMove.Reset();
			}
			if (player != null)
			{
				player.completelyStopAnimatingOrDoingAction();
			}
		}

		public void OnAppResume()
		{
			if (player != null)
			{
				player.CanMove = true;
			}
		}

		private void DrawVirtualJoypad()
		{
			if (!(activeClickableMenu is VirtualJoypad) && (!eventUp || options.weaponControl != 0) && !(currentMinigame is GrandpaStory) && !(currentMinigame is Intro))
			{
				virtualJoypad.update(currentGameTime);
				if (activeClickableMenu == null || activeClickableMenu is VirtualJoypad || activeClickableMenu is ChatBox || (eventUp && currentLocation != null && currentLocation.currentEvent != null && (currentMinigame == null || currentMinigame is TargetGame || currentMinigame is FishingGame) && activeClickableMenu == null))
				{
					virtualJoypad.draw(spriteBatch);
				}
			}
		}

		public bool CanTakeScreenshots()
		{
			_ = Environment.Is64BitProcess;
			return true;
		}

		public bool CanBrowseScreenshots()
		{
			return false;
		}

		public bool CanZoomScreenshots()
		{
			return false;
		}

		public void BrowseScreenshots()
		{
		}

		public unsafe string takeMapScreenshot(float? in_scale, string screenshot_name, Action onDone)
		{
			if (screenshot_name == null || screenshot_name.Trim() == "")
			{
				screenshot_name = SaveGame.FilterFileName(player.name) + "_" + DateTime.UtcNow.Month + "-" + DateTime.UtcNow.Day + "-" + DateTime.UtcNow.Year + "_" + (int)DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
			}
			if (currentLocation == null)
			{
				return null;
			}
			string text = screenshot_name + ".png";
			int num = 0;
			int num2 = 0;
			int num3 = currentLocation.map.DisplayWidth;
			int num4 = currentLocation.map.DisplayHeight;
			try
			{
				PropertyValue value = null;
				if (currentLocation.map.Properties.TryGetValue("ScreenshotRegion", out value))
				{
					string[] array = value.ToString().Split(' ');
					num = int.Parse(array[0]) * 64;
					num2 = int.Parse(array[1]) * 64;
					num3 = (int.Parse(array[2]) + 1) * 64 - num;
					num4 = (int.Parse(array[3]) + 1) * 64 - num2;
				}
			}
			catch (Exception)
			{
				num = 0;
				num2 = 0;
				num3 = currentLocation.map.DisplayWidth;
				num4 = currentLocation.map.DisplayHeight;
			}
			float num5 = ((!in_scale.HasValue) ? 1f : in_scale.Value);
			int num6 = (int)((float)num3 * num5);
			int num7 = (int)((float)num4 * num5);
			if ((float)num6 > 2048f || (float)num7 > 2048f)
			{
				float val = 1f / (float)num6 * 2048f;
				float val2 = 1f / (float)num7 * 2048f;
				num5 = Math.Min(val, val2);
			}
			bool flag = false;
			SKSurface sKSurface = null;
			do
			{
				flag = false;
				num6 = (int)((float)num3 * num5);
				num7 = (int)((float)num4 * num5);
				try
				{
					sKSurface = SKSurface.Create(num6, num7, SKColorType.Rgb888x, SKAlphaType.Opaque);
				}
				catch (Exception ex2)
				{
					Console.WriteLine("Map Screenshot: Error trying to create Bitmap: " + ex2.ToString());
					flag = true;
				}
				if (flag)
				{
					num5 -= 0.25f;
				}
				if (num5 <= 0f)
				{
					return null;
				}
			}
			while (flag);
			int num8 = 2048;
			int num9 = (int)((float)num8 * num5);
			xTile.Dimensions.Rectangle rectangle = viewport;
			bool flag2 = displayHUD;
			takingMapScreenshot = true;
			float baseZoomLevel = options.baseZoomLevel;
			options.baseZoomLevel = 1f;
			RenderTarget2D renderTarget2D = _lightmap;
			_lightmap = null;
			bool flag3 = false;
			try
			{
				allocateLightmap(num8, num8);
				int num10 = (int)Math.Ceiling((float)num6 / (float)num9);
				int num11 = (int)Math.Ceiling((float)num7 / (float)num9);
				for (int i = 0; i < num11; i++)
				{
					for (int j = 0; j < num10; j++)
					{
						int num12 = num9;
						int num13 = num9;
						int num14 = j * num9;
						int num15 = i * num9;
						if (num14 + num9 > num6)
						{
							num12 += num6 - (num14 + num9);
						}
						if (num15 + num9 > num7)
						{
							num13 += num7 - (num15 + num9);
						}
						if (num13 <= 0 || num12 <= 0)
						{
							continue;
						}
						Microsoft.Xna.Framework.Rectangle rectangle2 = new Microsoft.Xna.Framework.Rectangle(num14, num15, num12, num13);
						RenderTarget2D renderTarget2D2 = new RenderTarget2D(graphics.GraphicsDevice, num8, num8, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
						viewport = new xTile.Dimensions.Rectangle(j * num8 + num, i * num8 + num2, num8, num8);
						_draw(currentGameTime, renderTarget2D2);
						RenderTarget2D renderTarget2D3 = new RenderTarget2D(graphics.GraphicsDevice, num12, num13, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
						base.GraphicsDevice.SetRenderTarget(renderTarget2D3);
						spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
						Color white = Color.White;
						spriteBatch.Draw(renderTarget2D2, Vector2.Zero, renderTarget2D2.Bounds, white, 0f, Vector2.Zero, num5, SpriteEffects.None, 1f);
						spriteBatch.End();
						renderTarget2D2.Dispose();
						base.GraphicsDevice.SetRenderTarget(null);
						Color[] array2 = new Color[num12 * num13];
						renderTarget2D3.GetData(array2);
						SKImageInfo sKImageInfo = new SKImageInfo(num12, num13, SKColorType.Rgb888x);
						SKBitmap sKBitmap = new SKBitmap(rectangle2.Width, rectangle2.Height, SKColorType.Rgb888x, SKAlphaType.Opaque);
						byte* ptr = (byte*)sKBitmap.GetPixels().ToPointer();
						for (int k = 0; k < num13; k++)
						{
							for (int l = 0; l < num12; l++)
							{
								*(ptr++) = array2[l + k * num12].R;
								*(ptr++) = array2[l + k * num12].G;
								*(ptr++) = array2[l + k * num12].B;
								*(ptr++) = 255;
							}
						}
						SKPaint paint = new SKPaint();
						sKSurface.Canvas.DrawBitmap(sKBitmap, SKRect.Create(rectangle2.X, rectangle2.Y, num12, num13), paint);
						sKBitmap.Dispose();
						renderTarget2D3.Dispose();
					}
				}
				string path = Path.Combine(screenshotsPath, text);
				SKImage sKImage = sKSurface.Snapshot();
				SKData sKData = sKImage.Encode(SKEncodedImageFormat.Png, 100);
				sKData.SaveTo(new FileStream(path, FileMode.OpenOrCreate));
				sKSurface.Dispose();
			}
			catch (Exception ex3)
			{
				Console.WriteLine("Map Screenshot: Error taking screenshot: " + ex3.ToString());
				base.GraphicsDevice.SetRenderTarget(null);
				flag3 = true;
			}
			if (_lightmap != null)
			{
				_lightmap.Dispose();
				_lightmap = null;
			}
			_lightmap = renderTarget2D;
			options.baseZoomLevel = baseZoomLevel;
			takingMapScreenshot = false;
			displayHUD = flag2;
			viewport = rectangle;
			if (flag3)
			{
				return null;
			}
			return text;
		}

		public void CleanupReturningToTitle()
		{
			if (!game1.IsMainInstance)
			{
				GameRunner.instance.RemoveGameInstance(this);
			}
			else
			{
				foreach (Game1 gameInstance in GameRunner.instance.gameInstances)
				{
					if (gameInstance != this)
					{
						GameRunner.instance.RemoveGameInstance(gameInstance);
					}
				}
			}
			Console.WriteLine("CleanupReturningToTitle()");
			LocalizedContentManager.localizedAssetNames.Clear();
			Event.invalidFestivals.Clear();
			NPC.invalidDialogueFiles.Clear();
			SaveGame.CancelToTitle = false;
			overlayMenu = null;
			multiplayer.cachedMultiplayerMaps.Clear();
			keyboardFocusInstance = null;
			multiplayer.Disconnect(Multiplayer.DisconnectType.ExitedToMainMenu);
			BuildingPaintMenu.savedColors = null;
			startingGameSeed = null;
			_afterNewDayAction = null;
			_currentMinigame = null;
			gameMode = 0;
			_isSaving = false;
			_mouseCursorTransparency = 1f;
			_newDayTask = null;
			newDaySync = null;
			resetPlayer();
			serverHost = null;
			afterDialogues = null;
			afterFade = null;
			afterPause = null;
			afterViewport = null;
			ambientLight = new Color(0, 0, 0, 0);
			background = null;
			startedJukeboxMusic = false;
			boardingBus = false;
			chanceToRainTomorrow = 0.0;
			chatBox = null;
			if (specialCurrencyDisplay != null)
			{
				specialCurrencyDisplay.Cleanup();
			}
			specialCurrencyDisplay = null;
			client = null;
			cloud = null;
			conventionMode = false;
			coopDwellerBorn = false;
			creditsTimer = 0f;
			cropsOfTheWeek = null;
			currentBarnTexture = null;
			currentBillboard = 0;
			currentCoopTexture = null;
			currentCursorTile = Vector2.Zero;
			currentDialogueCharacterIndex = 0;
			currentFloor = 3;
			currentHouseTexture = null;
			currentLightSources.Clear();
			currentLoader = null;
			currentLocation = null;
			currentObjectDialogue.Clear();
			currentQuestionChoice = 0;
			currentSeason = "spring";
			currentSongIndex = 3;
			currentSpeaker = null;
			currentViewportTarget = Vector2.Zero;
			currentWallpaper = 0;
			cursorTileHintCheckTimer = 0;
			CustomData = new SerializableDictionary<string, string>();
			player.team.sharedDailyLuck.Value = 0.001;
			dayOfMonth = 0;
			dealerCalicoJackTotal = null;
			debrisSoundInterval = 0f;
			ClearDebrisWeather(debrisWeather);
			debugMode = false;
			debugOutput = null;
			debugPresenceString = "In menus";
			delayedActions.Clear();
			morningSongPlayAction = null;
			dialogueButtonScale = 1f;
			dialogueButtonShrinking = false;
			dialogueTyping = false;
			dialogueTypingInterval = 0;
			dialogueUp = false;
			dialogueWidth = 1024;
			displayFarmer = true;
			displayHUD = true;
			downPolling = 0f;
			drawGrid = false;
			drawLighting = false;
			elliottBookName = "Blue Tower";
			endOfNightMenus.Clear();
			errorMessage = "";
			eveningColor = new Color(255, 255, 0, 255);
			eventOver = false;
			eventUp = false;
			exitToTitle = false;
			facingDirectionAfterWarp = 0;
			fadeIn = true;
			fadeToBlack = false;
			fadeToBlackAlpha = 1.02f;
			FarmerFloor = 29;
			farmerWallpaper = 22;
			farmEvent = null;
			fertilizer = "";
			flashAlpha = 0f;
			floorPrice = 75;
			freezeControls = false;
			gamePadAButtonPolling = 0;
			gameTimeInterval = 0;
			globalFade = false;
			globalFadeSpeed = 0f;
			globalOutdoorLighting = 0f;
			greenhouseTexture = null;
			haltAfterCheck = false;
			hasLoadedGame = false;
			hitShakeTimer = 0;
			hudMessages.Clear();
			inMine = false;
			isActionAtCurrentCursorTile = false;
			isDebrisWeather = false;
			isInspectionAtCurrentCursorTile = false;
			isLightning = false;
			isQuestion = false;
			isRaining = false;
			isSnowing = false;
			jukeboxPlaying = false;
			keyHelpString = "";
			killScreen = false;
			lastCursorMotionWasMouse = true;
			lastCursorTile = Vector2.Zero;
			lastDebugInput = "";
			lastMousePositionBeforeFade = Point.Zero;
			leftPolling = 0f;
			listeningForKeyControlDefinitions = false;
			loadingMessage = "";
			locationRequest = null;
			warpingForForcedRemoteEvent = false;
			locations.Clear();
			logo = null;
			logoScreenTexture = null;
			mailbox.Clear();
			mailboxTexture = null;
			mapDisplayDevice = new XnaDisplayDevice(content, base.GraphicsDevice);
			menuChoice = 0;
			menuUp = false;
			messageAfterPause = "";
			messagePause = false;
			minecartHighScore = 0;
			moneyMadeScreen = null;
			mouseClickPolling = 0;
			mouseCursor = 0;
			multiplayerMode = 0;
			nameSelectType = null;
			nameSelectUp = false;
			netWorldState = new NetRoot<IWorldState>(new NetWorldState());
			newDay = false;
			nonWarpFade = false;
			noteBlockTimer = 0f;
			npcDialogues = null;
			numberOfSelectedItems = -1;
			objectDialoguePortraitPerson = null;
			hasApplied1_3_UpdateChanges = false;
			hasApplied1_4_UpdateChanges = false;
			remoteEventQueue.Clear();
			if (bannedUsers != null)
			{
				bannedUsers.Clear();
			}
			onScreenMenus.Clear();
			onScreenMenus.Add(new Toolbar());
			dayTimeMoneyBox = new DayTimeMoneyBox();
			onScreenMenus.Add(dayTimeMoneyBox);
			buffsDisplay = new BuffsDisplay();
			onScreenMenus.Add(buffsDisplay);
			foreach (KeyValuePair<long, Farmer> otherFarmer in otherFarmers)
			{
				otherFarmer.Value.unload();
			}
			otherFarmers.Clear();
			outdoorLight = new Color(255, 255, 0, 255);
			overlayMenu = null;
			overrideGameMenuReset = false;
			panFacingDirectionWait = false;
			panMode = false;
			panModeString = null;
			particleRaining = false;
			pauseAccumulator = 0f;
			paused = false;
			pauseThenDoFunctionTimer = 0;
			pauseTime = 0f;
			percentageToWinStardewHero = 70;
			pickingTool = false;
			pickToolInterval = 0f;
			previousViewportPosition = Vector2.Zero;
			priceOfSelectedItem = 0;
			progressBar = false;
			questionChoices.Clear();
			questOfTheDay = null;
			quit = false;
			rightClickPolling = 0;
			rightPolling = 0f;
			runThreshold = 0.5f;
			samBandName = "The Alfalfas";
			saveOnNewDay = true;
			startingCabins = 0;
			cabinsSeparate = false;
			screenGlow = false;
			screenGlowAlpha = 0f;
			screenGlowColor = new Color(0, 0, 0, 0);
			screenGlowHold = false;
			screenGlowMax = 0f;
			screenGlowRate = 0.005f;
			screenGlowUp = false;
			screenOverlayTempSprites.Clear();
			uiOverlayTempSprites.Clear();
			selectedItemsType = null;
			server = null;
			shiny = Vector2.Zero;
			newGameSetupOptions.Clear();
			shippingTax = false;
			showingEndOfNightStuff = false;
			showKeyHelp = false;
			slotResult = null;
			spawnMonstersAtNight = false;
			staminaShakeTimer = 0;
			starCropShimmerPause = 0f;
			swordSwipe = null;
			swordSwipeDark = null;
			textColor = new Color(34, 17, 34, 255);
			textShadowColor = new Color(206, 156, 95, 255);
			thumbstickMotionAccell = 1f;
			thumbstickMotionMargin = 0;
			thumbstickPollingTimer = 0;
			thumbStickSensitivity = 0.1f;
			timeOfDay = 600;
			timeOfDayAfterFade = -1;
			timerUntilMouseFade = 0;
			titleScreenBG = null;
			tmpTimeOfDay = 0;
			toggleFullScreen = false;
			toolHold = 0f;
			toolIconBox = null;
			ResetToolSpriteSheet();
			triggerPolling = 0;
			tvStation = -1;
			tvStationTexture = null;
			uniqueIDForThisGame = (ulong)(DateTime.UtcNow - new DateTime(2012, 6, 22)).TotalSeconds;
			upPolling = 0f;
			viewportFreeze = false;
			viewportHold = 0;
			viewportPositionLerp = Vector2.Zero;
			viewportReachedTarget = null;
			viewportSpeed = 2f;
			viewportTarget = new Vector2(-2.1474836E+09f, -2.1474836E+09f);
			wallpaperPrice = 75;
			wasMouseVisibleThisFrame = true;
			wasRainingYesterday = false;
			weatherForTomorrow = 0;
			elliottPiano = 0;
			weatherIcon = 0;
			weddingToday = false;
			whereIsTodaysFest = null;
			worldStateIDs.Clear();
			whichFarm = 0;
			windGust = 0f;
			xLocationAfterWarp = 0;
			game1.xTileContent.Dispose();
			game1.xTileContent = CreateContentManager(content.ServiceProvider, content.RootDirectory);
			year = 1;
			yLocationAfterWarp = 0;
			mailDeliveredFromMailForTomorrow.Clear();
			bundleType = BundleType.Default;
			JojaMart.Morris = null;
			AmbientLocationSounds.onLocationLeave();
			WeatherDebris.globalWind = -0.25f;
			Utility.killAllStaticLoopingSoundCues();
			TitleMenu.subMenu = null;
			OptionsDropDown.selected = null;
			JunimoNoteMenu.tempSprites.Clear();
			JunimoNoteMenu.screenSwipe = null;
			JunimoNoteMenu.canClick = true;
			GameMenu.forcePreventClose = false;
			Club.timesPlayedCalicoJack = 0;
			MineShaft.activeMines.Clear();
			MineShaft.permanentMineChanges.Clear();
			MineShaft.numberOfCraftedStairsUsedThisRun = 0;
			MineShaft.mushroomLevelsGeneratedToday.Clear();
			Desert.boughtMagicRockCandy = false;
			VolcanoDungeon.activeLevels.Clear();
			Rumble.stopRumbling();
			game1.refreshWindowSettings();
			if (activeClickableMenu != null && activeClickableMenu is TitleMenu)
			{
				(activeClickableMenu as TitleMenu).applyPreferences();
				activeClickableMenu.gameWindowSizeChanged(graphics.GraphicsDevice.Viewport.Bounds, graphics.GraphicsDevice.Viewport.Bounds);
			}
		}

		public static void GetHasRoomAnotherFarmAsync(ReportHasRoomAnotherFarm callback)
		{
			if (LocalMultiplayer.IsLocalMultiplayer())
			{
				bool hasRoomAnotherFarm = GetHasRoomAnotherFarm();
				callback(hasRoomAnotherFarm);
				return;
			}
			Task task = new Task(delegate
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				bool hasRoomAnotherFarm2 = GetHasRoomAnotherFarm();
				callback(hasRoomAnotherFarm2);
			});
			hooks.StartTask(task, "Farm_SpaceCheck");
		}

		private static string GameModeToString(byte mode)
		{
			return mode switch
			{
				4 => $"logoScreenGameMode ({mode})", 
				0 => $"titleScreenGameMode ({mode})", 
				1 => $"loadScreenGameMode ({mode})", 
				2 => $"newGameMode ({mode})", 
				3 => $"playingGameMode ({mode})", 
				6 => $"loadingMode ({mode})", 
				7 => $"saveMode ({mode})", 
				8 => $"saveCompleteMode ({mode})", 
				9 => $"selectGameScreen ({mode})", 
				10 => $"creditsMode ({mode})", 
				11 => $"errorLogMode ({mode})", 
				_ => $"unknown ({mode})", 
			};
		}

		public static string GetVersionString()
		{
			string text = string.Empty;
			if (versionBuild > 0)
			{
				text = " build " + versionBuild;
			}
			string text2 = string.Empty;
			if (!string.IsNullOrEmpty(versionLabel))
			{
				text2 = " " + versionLabel;
			}
			return version + text + text2;
		}

		public static void ResetToolSpriteSheet()
		{
			if (_toolSpriteSheet != null)
			{
				_toolSpriteSheet.Dispose();
				_toolSpriteSheet = null;
			}
			Texture2D texture2D = content.Load<Texture2D>("TileSheets\\tools");
			int actualWidth = texture2D.GetActualWidth();
			int actualHeight = texture2D.GetActualHeight();
			int levelCount = texture2D.LevelCount;
			Texture2D texture2D2 = new Texture2D(game1.GraphicsDevice, actualWidth, actualHeight, mipmap: false, SurfaceFormat.Color);
			Color[] data = new Color[actualWidth * actualHeight];
			texture2D.GetData(data);
			texture2D2.SetData(data);
			texture2D2.SetImageSize(texture2D.Width, texture2D.Height);
			_toolSpriteSheet = texture2D2;
		}

		public static void SetSaveName(string new_save_name)
		{
			if (new_save_name == null)
			{
				new_save_name = "";
			}
			_currentSaveName = new_save_name;
			_setSaveName = true;
		}

		public static string GetSaveGameName(bool set_value = true)
		{
			if (!_setSaveName && set_value)
			{
				string value = MasterPlayer.farmName.Value;
				string text = SaveGame.FilterFileName(value);
				int num = 2;
				string root = null;
				while (SaveGame.IsNewGameSaveNameCollision(text, root))
				{
					text = value + num;
					num++;
				}
				SetSaveName(text);
			}
			return _currentSaveName;
		}

		private static void allocateLightmap(int width, int height)
		{
			int num = 8;
			float num2 = 1f;
			if (options != null)
			{
				num = options.lightingQuality;
				num2 = ((!game1.useUnscaledLighting) ? options.zoomLevel : 1f);
			}
			int val = (int)((float)width * (1f / num2) + 64f) / (num / 2);
			int val2 = (int)((float)height * (1f / num2) + 64f) / (num / 2);
			val = Math.Max(val, 1);
			val2 = Math.Max(val2, 1);
			if (lightmap == null || lightmap.Width != val || lightmap.Height != val2)
			{
				if (_lightmap != null)
				{
					_lightmap.Dispose();
				}
				_lightmap = new RenderTarget2D(graphics.GraphicsDevice, val, val2, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			}
		}

		public static bool canHaveWeddingOnDay(int day, string season)
		{
			if (Utility.isFestivalDay(day, season))
			{
				return false;
			}
			return true;
		}

		public static void RefreshQuestOfTheDay()
		{
			questOfTheDay = Utility.getQuestOfTheDay();
			if (Utility.isFestivalDay(dayOfMonth, currentSeason) || Utility.isFestivalDay(dayOfMonth + 1, currentSeason))
			{
				questOfTheDay = null;
			}
		}

		public static void ExitToTitle(Action postExitCallback = null)
		{
			_requestedMusicTracks.Clear();
			UpdateRequestedMusicTrack();
			changeMusicTrack("none");
			setGameMode(0);
			exitToTitle = true;
			postExitToTitleCallback = postExitCallback;
		}

		public Game1(PlayerIndex player_index, int index)
			: this()
		{
			instancePlayerOneIndex = player_index;
			instanceIndex = index;
		}

		public Game1()
		{
			instanceId = GameRunner.instance.GetNewInstanceID();
			if (Program.gamePtr == null)
			{
				Program.gamePtr = this;
			}
			_temporaryContent = CreateContentManager(base.Content.ServiceProvider, base.Content.RootDirectory);
		}

		public void TranslateFields()
		{
			LocalizedContentManager.localizedAssetNames.Clear();
			samBandName = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156");
			elliottBookName = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2157");
			objectSpriteSheet = content.Load<Texture2D>("Maps\\springobjects");
			dialogueFont = content.Load<SpriteFont>("Fonts\\SpriteFont1");
			smallFont = content.Load<SpriteFont>("Fonts\\SmallFont");
			smallFont.LineSpacing = 26;
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
			{
				smallFont.LineSpacing += 16;
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr)
			{
				smallFont.LineSpacing += 4;
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
			{
				smallFont.LineSpacing = LocalizedContentManager.CurrentModLanguage.SmallFontLineSpacing;
			}
			tinyFont = content.Load<SpriteFont>("Fonts\\tinyFont");
			tinyFontBorder = content.Load<SpriteFont>("Fonts\\tinyFontBorder");
			objectInformation = content.Load<Dictionary<int, string>>("Data\\ObjectInformation");
			clothingInformation = content.Load<Dictionary<int, string>>("Data\\ClothingInformation");
			bigCraftablesInformation = content.Load<Dictionary<int, string>>("Data\\BigCraftablesInformation");
			achievements = content.Load<Dictionary<int, string>>("Data\\Achievements");
			CraftingRecipe.craftingRecipes = content.Load<Dictionary<string, string>>("Data\\CraftingRecipes");
			CraftingRecipe.cookingRecipes = content.Load<Dictionary<string, string>>("Data\\CookingRecipes");
			MovieTheater.ClearCachedLocalizedData();
			mouseCursors = content.Load<Texture2D>("LooseSprites\\Cursors");
			mouseCursors2 = content.Load<Texture2D>("LooseSprites\\Cursors2");
			giftboxTexture = content.Load<Texture2D>("LooseSprites\\Giftbox");
			controllerMaps = content.Load<Texture2D>("LooseSprites\\ControllerMaps");
			NPCGiftTastes = content.Load<Dictionary<string, string>>("Data\\NPCGiftTastes");
			_shortDayDisplayName[0] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3042");
			_shortDayDisplayName[1] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3043");
			_shortDayDisplayName[2] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3044");
			_shortDayDisplayName[3] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3045");
			_shortDayDisplayName[4] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3046");
			_shortDayDisplayName[5] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3047");
			_shortDayDisplayName[6] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3048");
		}

		public void exitEvent(object sender, EventArgs e)
		{
			multiplayer.Disconnect(Multiplayer.DisconnectType.ClosedGame);
			keyboardDispatcher.Cleanup();
		}

		public void refreshWindowSettings()
		{
			GameRunner.instance.OnWindowSizeChange(null, null);
		}

		public void Window_ClientSizeChanged(object sender, EventArgs e)
		{
			if (_windowResizing)
			{
				return;
			}
			Console.WriteLine("Window_ClientSizeChanged(); Window.ClientBounds={0}", base.Window.ClientBounds);
			if (options == null)
			{
				Console.WriteLine("Window_ClientSizeChanged(); options is null, returning.");
				return;
			}
			_windowResizing = true;
			int w = (graphics.IsFullScreen ? graphics.PreferredBackBufferWidth : base.Window.ClientBounds.Width);
			int h = (graphics.IsFullScreen ? graphics.PreferredBackBufferHeight : base.Window.ClientBounds.Height);
			GameRunner.instance.ExecuteForInstances(delegate(Game1 instance)
			{
				instance.SetWindowSize(w, h);
			});
			_windowResizing = false;
		}

		public virtual void SetWindowSize(int w, int h)
		{
			Microsoft.Xna.Framework.Rectangle oldBounds = new Microsoft.Xna.Framework.Rectangle(viewport.X, viewport.Y, viewport.Width, viewport.Height);
			Microsoft.Xna.Framework.Rectangle rectangle = base.Window.ClientBounds;
			bool flag = false;
			w = graphics.PreferredBackBufferWidth;
			h = graphics.PreferredBackBufferHeight;
			rectangle = base.Window.ClientBounds;
			if (base.Window.ClientBounds.Width < base.Window.ClientBounds.Height)
			{
				w = base.Window.ClientBounds.Height;
				h = base.Window.ClientBounds.Width;
				rectangle = new Microsoft.Xna.Framework.Rectangle(base.Window.ClientBounds.X, base.Window.ClientBounds.Y, base.Window.ClientBounds.Height, base.Window.ClientBounds.Width);
			}
			if (flag)
			{
				rectangle = base.Window.ClientBounds;
			}
			if (base.IsMainInstance && graphics.SynchronizeWithVerticalRetrace != options.vsyncEnabled)
			{
				graphics.SynchronizeWithVerticalRetrace = options.vsyncEnabled;
				Console.WriteLine("Vsync toggled: " + graphics.SynchronizeWithVerticalRetrace);
			}
			graphics.ApplyChanges();
			try
			{
				if (graphics.IsFullScreen)
				{
					localMultiplayerWindow = new Microsoft.Xna.Framework.Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
				}
				else
				{
					localMultiplayerWindow = new Microsoft.Xna.Framework.Rectangle(0, 0, w, h);
				}
			}
			catch (Exception)
			{
			}
			defaultDeviceViewport = new Viewport(localMultiplayerWindow);
			List<Vector4> list = new List<Vector4>();
			if (GameRunner.instance.gameInstances.Count <= 1)
			{
				list.Add(new Vector4(0f, 0f, 1f, 1f));
			}
			else if (GameRunner.instance.gameInstances.Count == 2)
			{
				list.Add(new Vector4(0f, 0f, 0.5f, 1f));
				list.Add(new Vector4(0.5f, 0f, 0.5f, 1f));
			}
			else if (GameRunner.instance.gameInstances.Count == 3)
			{
				list.Add(new Vector4(0f, 0f, 1f, 0.5f));
				list.Add(new Vector4(0f, 0.5f, 0.5f, 0.5f));
				list.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
			}
			else if (GameRunner.instance.gameInstances.Count == 4)
			{
				list.Add(new Vector4(0f, 0f, 0.5f, 0.5f));
				list.Add(new Vector4(0.5f, 0f, 0.5f, 0.5f));
				list.Add(new Vector4(0f, 0.5f, 0.5f, 0.5f));
				list.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
			}
			if (GameRunner.instance.gameInstances.Count <= 1)
			{
				zoomModifier = 1f;
			}
			else
			{
				zoomModifier = 0.5f;
			}
			Vector4 vector = list[game1.instanceIndex];
			Vector2? vector2 = null;
			if (uiScreen != null)
			{
				vector2 = new Vector2(uiScreen.Width, uiScreen.Height);
			}
			localMultiplayerWindow.X = (int)((float)w * vector.X);
			localMultiplayerWindow.Y = (int)((float)h * vector.Y);
			localMultiplayerWindow.Width = (int)Math.Ceiling((float)w * vector.Z);
			localMultiplayerWindow.Height = (int)Math.Ceiling((float)h * vector.W);
			try
			{
				int width = (int)Math.Ceiling((float)localMultiplayerWindow.Width * (1f / options.zoomLevel));
				int height = (int)Math.Ceiling((float)localMultiplayerWindow.Height * (1f / options.zoomLevel));
				screen = new RenderTarget2D(graphics.GraphicsDevice, width, height, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
				screen.Name = "Screen";
				int width2 = (int)Math.Ceiling((float)localMultiplayerWindow.Width / options.uiScale);
				int height2 = (int)Math.Ceiling((float)localMultiplayerWindow.Height / options.uiScale);
				uiScreen = new RenderTarget2D(graphics.GraphicsDevice, width2, height2, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
				uiScreen.Name = "UI Screen";
			}
			catch (Exception)
			{
			}
			updateViewportForScreenSizeChange(fullscreenChange: false, localMultiplayerWindow.Width, localMultiplayerWindow.Height);
			if (currentMinigame != null)
			{
				currentMinigame.changeScreenSize();
			}
			if (vector2.HasValue && vector2.Value.X == (float)uiScreen.Width && vector2.Value.Y == (float)uiScreen.Height)
			{
				return;
			}
			PushUIMode();
			if (textEntry != null)
			{
				textEntry.gameWindowSizeChanged(oldBounds, new Microsoft.Xna.Framework.Rectangle(viewport.X, viewport.Y, viewport.Width, viewport.Height));
			}
			foreach (IClickableMenu onScreenMenu in onScreenMenus)
			{
				onScreenMenu.gameWindowSizeChanged(oldBounds, new Microsoft.Xna.Framework.Rectangle(viewport.X, viewport.Y, viewport.Width, viewport.Height));
			}
			if (activeClickableMenu != null)
			{
				activeClickableMenu.gameWindowSizeChanged(oldBounds, new Microsoft.Xna.Framework.Rectangle(viewport.X, viewport.Y, viewport.Width, viewport.Height));
			}
			if (activeClickableMenu is GameMenu && !overrideGameMenuReset)
			{
				activeClickableMenu = new GameMenu((activeClickableMenu as GameMenu).currentTab);
			}
			PopUIMode();
		}

		private void Game1_Exiting(object sender, EventArgs e)
		{
			Program.sdk.Shutdown();
		}

		public static void setGameMode(byte mode)
		{
			Console.WriteLine("setGameMode( '{0}' )", GameModeToString(mode));
			_gameMode = mode;
			if (temporaryContent != null)
			{
				temporaryContent.Unload();
			}
			switch (mode)
			{
			case 0:
			{
				bool flag = false;
				if (activeClickableMenu != null && currentGameTime != null && currentGameTime.TotalGameTime.TotalSeconds > 10.0)
				{
					flag = true;
				}
				if (game1.instanceIndex <= 0)
				{
					activeClickableMenu = new TitleMenu();
					if (flag)
					{
						(activeClickableMenu as TitleMenu).skipToTitleButtons();
					}
				}
				break;
			}
			case 3:
				hasApplied1_3_UpdateChanges = true;
				hasApplied1_4_UpdateChanges = false;
				break;
			}
		}

		public static void updateViewportForScreenSizeChange(bool fullscreenChange, int width, int height)
		{
			forceSnapOnNextViewportUpdate = true;
			if (graphics.GraphicsDevice != null)
			{
				allocateLightmap(width, height);
			}
			width = (int)Math.Ceiling((float)width / options.zoomLevel);
			height = (int)Math.Ceiling((float)height / options.zoomLevel);
			Point point = new Point(viewport.X + viewport.Width / 2, viewport.Y + viewport.Height / 2);
			bool flag = false;
			if (viewport.Width != width || viewport.Height != height)
			{
				flag = true;
			}
			xTile.Dimensions.Rectangle old_viewport = viewport;
			viewport = new xTile.Dimensions.Rectangle(point.X - width / 2, point.Y - height / 2, width, height);
			if (currentLocation == null)
			{
				return;
			}
			if (eventUp)
			{
				if (!IsFakedBlackScreen() && currentLocation.IsOutdoors)
				{
					clampViewportToGameMap();
				}
				return;
			}
			if (viewport.X >= 0 || !currentLocation.IsOutdoors || fullscreenChange)
			{
				point = new Point(viewport.X + viewport.Width / 2, viewport.Y + viewport.Height / 2);
				viewport = new xTile.Dimensions.Rectangle(point.X - width / 2, point.Y - height / 2, width, height);
				UpdateViewPort(overrideFreeze: true, point);
			}
			if (flag)
			{
				forceSnapOnNextViewportUpdate = true;
				_UpdateDebrisWeatherAndRainDropsForResize(old_viewport, viewport);
				if (currentLocation != null)
				{
					currentLocation.HandleViewportSizeChange(old_viewport, viewport);
				}
			}
		}

		public void Instance_Initialize()
		{
			Initialize();
		}

		public static bool IsFading()
		{
			if (!globalFade && (!fadeIn || !(fadeToBlackAlpha > 0f)))
			{
				if (fadeToBlack)
				{
					return fadeToBlackAlpha < 1f;
				}
				return false;
			}
			return true;
		}

		public static bool IsFakedBlackScreen()
		{
			if (currentMinigame != null)
			{
				return false;
			}
			if (CurrentEvent != null && CurrentEvent.currentCustomEventScript != null)
			{
				return false;
			}
			if (!eventUp)
			{
				return false;
			}
			return (float)(int)Math.Floor((float)new Point(viewport.X + viewport.Width / 2, viewport.Y + viewport.Height / 2).X / 64f) <= -200f;
		}

		protected override void Initialize()
		{
			keyboardDispatcher = new KeyboardDispatcher(base.Window);
			screenFade = new ScreenFade(onFadeToBlackComplete, onFadedBackInComplete);
			if (options == null)
			{
				options = new Options();
				options.musicVolumeLevel = 1f;
				options.soundVolumeLevel = 1f;
				options.greenSquaresGuide = true;
			}
			otherFarmers = new NetRootDictionary<long, Farmer>();
			otherFarmers.Serializer = SaveGame.farmerSerializer;
			viewport = new xTile.Dimensions.Rectangle(new Size(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));
			string rootDirectory = base.Content.RootDirectory;
			if (base.IsMainInstance)
			{
				try
				{
					AudioEngine engine = new AudioEngine(Path.Combine(rootDirectory, "XACT", "FarmerSounds.xgs"));
					audioEngine = new AudioEngineWrapper(engine);
					WaveBank waveBank = new WaveBank(audioEngine.Engine, Path.Combine(rootDirectory, "XACT", "wavebank.xwb"));
					WaveBank waveBank2 = new WaveBank(audioEngine.Engine, Path.Combine(rootDirectory, "XACT", "wavebank_14.xwb"));
					InitializeMusicWaveBank();
					soundBank = new SoundBankWrapper(new SoundBank(audioEngine.Engine, Path.Combine(rootDirectory, "XACT", "Sound Bank.xsb")));
				}
				catch (Exception arg)
				{
					Console.WriteLine("Game.Initialize() caught exception initializing XACT:\n{0}", arg);
					audioEngine = new DummyAudioEngine();
					soundBank = new DummySoundBank();
				}
			}
			audioEngine.Update();
			musicCategory = audioEngine.GetCategory("Music");
			soundCategory = audioEngine.GetCategory("Sound");
			ambientCategory = audioEngine.GetCategory("Ambient");
			footstepCategory = audioEngine.GetCategory("Footsteps");
			currentSong = null;
			if (soundBank != null)
			{
				wind = soundBank.GetCue("wind");
				chargeUpSound = soundBank.GetCue("toolCharge");
			}
			int width = graphics.GraphicsDevice.Viewport.Width;
			int height = graphics.GraphicsDevice.Viewport.Height;
			screen = new RenderTarget2D(graphics.GraphicsDevice, width, height, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			allocateLightmap(width, height);
			AmbientLocationSounds.InitShared();
			previousViewportPosition = Vector2.Zero;
			PushUIMode();
			PopUIMode();
			setRichPresence("menus");
		}

		public static void apply1_3_UpdateChanges()
		{
			if (IsMasterGame)
			{
				if (!player.craftingRecipes.ContainsKey("Wood Sign"))
				{
					player.craftingRecipes.Add("Wood Sign", 0);
				}
				if (!player.craftingRecipes.ContainsKey("Stone Sign"))
				{
					player.craftingRecipes.Add("Stone Sign", 0);
				}
				FarmHouse farmHouse = getLocationFromName("FarmHouse") as FarmHouse;
				farmHouse.furniture.Add(new Furniture(1792, Utility.PointToVector2(farmHouse.getFireplacePoint())));
				if (!MasterPlayer.mailReceived.Contains("JojaMember") && !getLocationFromName("Town").isTileOccupiedForPlacement(new Vector2(57f, 16f)))
				{
					getLocationFromName("Town").objects.Add(new Vector2(57f, 16f), new Object(Vector2.Zero, 55));
				}
				MarkFloorChestAsCollectedIfNecessary(10);
				MarkFloorChestAsCollectedIfNecessary(20);
				MarkFloorChestAsCollectedIfNecessary(40);
				MarkFloorChestAsCollectedIfNecessary(50);
				MarkFloorChestAsCollectedIfNecessary(60);
				MarkFloorChestAsCollectedIfNecessary(70);
				MarkFloorChestAsCollectedIfNecessary(80);
				MarkFloorChestAsCollectedIfNecessary(90);
				MarkFloorChestAsCollectedIfNecessary(100);
				hasApplied1_3_UpdateChanges = true;
			}
		}

		public static void apply1_4_UpdateChanges()
		{
			if (!IsMasterGame)
			{
				return;
			}
			foreach (Farmer allFarmer in getAllFarmers())
			{
				foreach (string key2 in allFarmer.friendshipData.Keys)
				{
					allFarmer.friendshipData[key2].Points = Math.Min(allFarmer.friendshipData[key2].Points, 3125);
				}
				if (allFarmer.ForagingLevel >= 7 && !allFarmer.craftingRecipes.ContainsKey("Tree Fertilizer"))
				{
					allFarmer.craftingRecipes.Add("Tree Fertilizer", 0);
				}
			}
			Dictionary<string, string> bundleData = netWorldState.Value.BundleData;
			foreach (KeyValuePair<string, string> item in bundleData)
			{
				int key = Convert.ToInt32(item.Key.Split('/')[1]);
				if (!netWorldState.Value.Bundles.ContainsKey(key))
				{
					netWorldState.Value.Bundles.Add(key, new NetArray<bool, NetBool>(item.Value.Split('/')[2].Split(' ').Length));
				}
				if (!netWorldState.Value.BundleRewards.ContainsKey(key))
				{
					netWorldState.Value.BundleRewards.Add(key, new NetBool(value: false));
				}
			}
			foreach (Farmer allFarmer2 in getAllFarmers())
			{
				foreach (Item item2 in allFarmer2.items)
				{
					if (item2 != null)
					{
						item2.HasBeenInInventory = true;
					}
				}
			}
			recalculateLostBookCount();
			Utility.iterateChestsAndStorage(delegate(Item item)
			{
				item.HasBeenInInventory = true;
			});
			foreach (TerrainFeature value in getLocationFromName("Greenhouse").terrainFeatures.Values)
			{
				if (value is HoeDirt)
				{
					((HoeDirt)value).isGreenhouseDirt.Value = true;
				}
			}
			hasApplied1_4_UpdateChanges = true;
		}

		public static void applySaveFix(SaveGame.SaveFixes save_fix)
		{
			switch (save_fix)
			{
			case SaveGame.SaveFixes.AddTownBush:
				if (getLocationFromName("Town") is Town town)
				{
					Vector2 tileLocation = new Vector2(61f, 93f);
					if (town.getLargeTerrainFeatureAt((int)tileLocation.X, (int)tileLocation.Y) == null)
					{
						town.largeTerrainFeatures.Add(new Bush(tileLocation, 2, town));
					}
				}
				break;
			case SaveGame.SaveFixes.AddBirdie:
				addBirdieIfNecessary();
				break;
			case SaveGame.SaveFixes.AddBugSteakRecipe:
				if (player.combatLevel.Value >= 2 && !player.craftingRecipes.ContainsKey("Bug Steak"))
				{
					player.craftingRecipes.Add("Bug Steak", 0);
				}
				break;
			case SaveGame.SaveFixes.StoredBigCraftablesStackFix:
				Utility.iterateChestsAndStorage(delegate(Item item)
				{
					if (item is Object)
					{
						Object @object = item as Object;
						if ((bool)@object.bigCraftable && @object.Stack == 0)
						{
							@object.Stack = 1;
						}
					}
				});
				break;
			case SaveGame.SaveFixes.PorchedCabinBushesFix:
			{
				foreach (Building building in getFarm().buildings)
				{
					if ((int)building.daysOfConstructionLeft <= 0 && building.indoors.Value is Cabin)
					{
						building.removeOverlappingBushes(getFarm());
					}
				}
				break;
			}
			case SaveGame.SaveFixes.ChangeObeliskFootprintHeight:
			{
				foreach (Building building2 in getFarm().buildings)
				{
					if (building2.buildingType.Value.Contains("Obelisk"))
					{
						building2.tilesHigh.Value = 2;
						building2.tileY.Value++;
					}
				}
				break;
			}
			case SaveGame.SaveFixes.CreateStorageDressers:
			{
				Utility.iterateChestsAndStorage(delegate(Item item)
				{
					if (item is Clothing)
					{
						item.Category = -100;
					}
				});
				List<DecoratableLocation> list2 = new List<DecoratableLocation>();
				foreach (GameLocation location in locations)
				{
					if (location is DecoratableLocation)
					{
						list2.Add(location as DecoratableLocation);
					}
				}
				foreach (Building building3 in getFarm().buildings)
				{
					if (building3.indoors != null)
					{
						GameLocation gameLocation = building3.indoors;
						if (gameLocation is DecoratableLocation)
						{
							list2.Add(gameLocation as DecoratableLocation);
						}
					}
				}
				{
					foreach (DecoratableLocation item3 in list2)
					{
						List<Furniture> list3 = new List<Furniture>();
						for (int j = 0; j < item3.furniture.Count; j++)
						{
							Furniture furniture = item3.furniture[j];
							if (furniture.ParentSheetIndex == 704 || furniture.ParentSheetIndex == 709 || furniture.ParentSheetIndex == 714 || furniture.ParentSheetIndex == 719)
							{
								StorageFurniture item2 = new StorageFurniture(furniture.ParentSheetIndex, furniture.TileLocation, furniture.currentRotation);
								list3.Add(item2);
								item3.furniture.RemoveAt(j);
								j--;
							}
						}
						for (int k = 0; k < list3.Count; k++)
						{
							item3.furniture.Add(list3[k]);
						}
					}
					break;
				}
			}
			case SaveGame.SaveFixes.InferPreserves:
			{
				int[] preserve_item_indices = new int[4] { 350, 348, 344, 342 };
				string[] suffixes = new string[3] { " Juice", " Wine", " Jelly" };
				Object.PreserveType[] suffix_preserve_types = new Object.PreserveType[3]
				{
					Object.PreserveType.Juice,
					Object.PreserveType.Wine,
					Object.PreserveType.Jelly
				};
				string[] prefixes = new string[1] { "Pickled " };
				Object.PreserveType[] prefix_preserve_types = new Object.PreserveType[1] { Object.PreserveType.Pickle };
				Utility.iterateAllItems(delegate(Item item)
				{
					if (item is Object && Utility.IsNormalObjectAtParentSheetIndex(item, item.ParentSheetIndex) && preserve_item_indices.Contains(item.ParentSheetIndex) && !(item as Object).preserve.Value.HasValue)
					{
						for (int num3 = 0; num3 < suffixes.Length; num3++)
						{
							string text3 = suffixes[num3];
							if (item.Name.EndsWith(text3))
							{
								string value = item.Name.Substring(0, item.Name.Length - text3.Length);
								int num4 = -1;
								foreach (int key in objectInformation.Keys)
								{
									if (objectInformation[key].Substring(0, objectInformation[key].IndexOf('/')).Equals(value))
									{
										num4 = key;
										break;
									}
								}
								if (num4 >= 0)
								{
									(item as Object).preservedParentSheetIndex.Value = num4;
									(item as Object).preserve.Value = suffix_preserve_types[num3];
									return;
								}
							}
						}
						for (int num5 = 0; num5 < prefixes.Length; num5++)
						{
							string text4 = prefixes[num5];
							if (item.Name.StartsWith(text4))
							{
								string value2 = item.Name.Substring(text4.Length);
								int num6 = -1;
								foreach (int key2 in objectInformation.Keys)
								{
									if (objectInformation[key2].Substring(0, objectInformation[key2].IndexOf('/')).Equals(value2))
									{
										num6 = key2;
										break;
									}
								}
								if (num6 >= 0)
								{
									(item as Object).preservedParentSheetIndex.Value = num6;
									(item as Object).preserve.Value = prefix_preserve_types[num5];
									break;
								}
							}
						}
					}
				});
				break;
			}
			case SaveGame.SaveFixes.TransferHatSkipHairFlag:
				Utility.iterateAllItems(delegate(Item item)
				{
					if (item is Hat)
					{
						Hat hat = item as Hat;
						if (hat.skipHairDraw)
						{
							hat.hairDrawType.Set(0);
							hat.skipHairDraw = false;
						}
					}
				});
				break;
			case SaveGame.SaveFixes.RevealSecretNoteItemTastes:
			{
				Dictionary<int, string> dictionary = content.Load<Dictionary<int, string>>("Data\\SecretNotes");
				for (int i = 0; i < 21; i++)
				{
					if (dictionary.ContainsKey(i) && player.secretNotesSeen.Contains(i))
					{
						Utility.ParseGiftReveals(dictionary[i]);
					}
				}
				break;
			}
			case SaveGame.SaveFixes.TransferHoneyTypeToPreserves:
			{
				int[] array = new int[1] { 340 };
				Utility.iterateAllItems(delegate(Item item)
				{
					if (item is Object && Utility.IsNormalObjectAtParentSheetIndex(item, item.ParentSheetIndex) && item.ParentSheetIndex == 340 && (item as Object).preservedParentSheetIndex.Value <= 0)
					{
						if ((item as Object).honeyType.Value.HasValue && (item as Object).honeyType.Value.Value >= (Object.HoneyType)0)
						{
							(item as Object).preservedParentSheetIndex.Value = (int)(item as Object).honeyType.Value.Value;
						}
						else
						{
							(item as Object).honeyType.Value = Object.HoneyType.Wild;
							(item as Object).preservedParentSheetIndex.Value = -1;
						}
					}
				});
				break;
			}
			case SaveGame.SaveFixes.TransferNoteBlockScale:
				Utility.iterateAllItems(delegate(Item item)
				{
					if (item is Object && Utility.IsNormalObjectAtParentSheetIndex(item, item.ParentSheetIndex) && (item.ParentSheetIndex == 363 || item.ParentSheetIndex == 464))
					{
						(item as Object).preservedParentSheetIndex.Value = (int)(item as Object).scale.X;
					}
				});
				break;
			case SaveGame.SaveFixes.FixCropHarvestAmountsAndInferSeedIndex:
				Utility.iterateAllCrops(delegate(Crop crop)
				{
					crop.ResetCropYield();
				});
				break;
			case SaveGame.SaveFixes.Level9PuddingFishingRecipe2:
			case SaveGame.SaveFixes.Level9PuddingFishingRecipe3:
				if (player.cookingRecipes.ContainsKey("Ocean Mineral Pudding"))
				{
					player.cookingRecipes.Remove("Ocean Mineral Pudding");
				}
				if (player.fishingLevel.Value >= 9 && !player.cookingRecipes.ContainsKey("Seafoam Pudding"))
				{
					player.cookingRecipes.Add("Seafoam Pudding", 0);
				}
				break;
			case SaveGame.SaveFixes.quarryMineBushes:
			{
				GameLocation locationFromName = getLocationFromName("Mountain");
				locationFromName.largeTerrainFeatures.Add(new Bush(new Vector2(101f, 18f), 1, locationFromName));
				locationFromName.largeTerrainFeatures.Add(new Bush(new Vector2(104f, 21f), 0, locationFromName));
				locationFromName.largeTerrainFeatures.Add(new Bush(new Vector2(105f, 18f), 0, locationFromName));
				break;
			}
			case SaveGame.SaveFixes.MissingQisChallenge:
			{
				foreach (Farmer allFarmer in getAllFarmers())
				{
					if (allFarmer.mailReceived.Contains("skullCave") && !allFarmer.hasQuest(20) && !allFarmer.hasOrWillReceiveMail("QiChallengeComplete"))
					{
						allFarmer.addQuest(20);
					}
				}
				break;
			}
			case SaveGame.SaveFixes.BedsToFurniture:
			{
				List<GameLocation> list5 = new List<GameLocation>();
				list5.Add(getLocationFromName("FarmHouse"));
				foreach (Building building4 in getFarm().buildings)
				{
					if ((building4.indoors.Value != null) & (building4.indoors.Value is FarmHouse))
					{
						list5.Add(building4.indoors);
					}
				}
				{
					foreach (GameLocation item4 in list5)
					{
						if (!(item4 is FarmHouse farmHouse2))
						{
							continue;
						}
						for (int num = 0; num < farmHouse2.map.Layers[0].TileWidth; num++)
						{
							for (int num2 = 0; num2 < farmHouse2.map.Layers[0].TileHeight; num2++)
							{
								if (farmHouse2.doesTileHaveProperty(num, num2, "DefaultBedPosition", "Back") == null)
								{
									continue;
								}
								if (farmHouse2.upgradeLevel == 0)
								{
									farmHouse2.furniture.Add(new BedFurniture(BedFurniture.DEFAULT_BED_INDEX, new Vector2(num, num2)));
									continue;
								}
								int which = BedFurniture.DOUBLE_BED_INDEX;
								if (!farmHouse2.owner.activeDialogueEvents.ContainsKey("pennyRedecorating"))
								{
									if (farmHouse2.owner.mailReceived.Contains("pennyQuilt0"))
									{
										which = 2058;
									}
									if (farmHouse2.owner.mailReceived.Contains("pennyQuilt1"))
									{
										which = 2064;
									}
									if (farmHouse2.owner.mailReceived.Contains("pennyQuilt2"))
									{
										which = 2070;
									}
								}
								farmHouse2.furniture.Add(new BedFurniture(which, new Vector2(num, num2)));
							}
						}
					}
					break;
				}
			}
			case SaveGame.SaveFixes.ChildBedsToFurniture:
			{
				List<GameLocation> list4 = new List<GameLocation>();
				list4.Add(getLocationFromName("FarmHouse"));
				foreach (Building building5 in getFarm().buildings)
				{
					if ((building5.indoors.Value != null) & (building5.indoors.Value is FarmHouse))
					{
						list4.Add(building5.indoors);
					}
				}
				{
					foreach (GameLocation item5 in list4)
					{
						if (!(item5 is FarmHouse farmHouse))
						{
							continue;
						}
						for (int l = 0; l < farmHouse.map.Layers[0].TileWidth; l++)
						{
							for (int m = 0; m < farmHouse.map.Layers[0].TileHeight; m++)
							{
								if (farmHouse.doesTileHaveProperty(l, m, "DefaultChildBedPosition", "Back") != null)
								{
									farmHouse.furniture.Add(new BedFurniture(BedFurniture.CHILD_BED_INDEX, new Vector2(l, m)));
								}
							}
						}
					}
					break;
				}
			}
			case SaveGame.SaveFixes.ModularizeFarmStructures:
				getFarm().AddModularShippingBin();
				break;
			case SaveGame.SaveFixes.FixFlooringFlags:
				Utility.ForAllLocations(delegate(GameLocation location)
				{
					foreach (TerrainFeature value3 in location.terrainFeatures.Values)
					{
						if (value3 is Flooring)
						{
							(value3 as Flooring).ApplyFlooringFlags();
						}
					}
				});
				break;
			case SaveGame.SaveFixes.AddNewRingRecipes1_5:
				if (player.combatLevel.Value >= 7 && !player.craftingRecipes.ContainsKey("Thorns Ring"))
				{
					player.craftingRecipes.Add("Thorns Ring", 0);
				}
				if (player.miningLevel.Value >= 4 && !player.craftingRecipes.ContainsKey("Glowstone Ring"))
				{
					player.craftingRecipes.Add("Glowstone Ring", 0);
				}
				break;
			case SaveGame.SaveFixes.ResetForges:
				Utility.iterateAllItems(delegate(Item item)
				{
					if (item is MeleeWeapon)
					{
						(item as MeleeWeapon).RecalculateAppliedForges();
					}
				});
				break;
			case SaveGame.SaveFixes.AddSquidInkRavioli:
				if (player.combatLevel.Value >= 9 && !player.cookingRecipes.ContainsKey("Squid Ink Ravioli"))
				{
					player.cookingRecipes.Add("Squid Ink Ravioli", 0);
				}
				break;
			case SaveGame.SaveFixes.MakeDarkSwordVampiric:
				Utility.iterateAllItems(delegate(Item item)
				{
					if (item is MeleeWeapon && (item as MeleeWeapon).InitialParentTileIndex == 2)
					{
						(item as MeleeWeapon).AddEnchantment(new VampiricEnchantment());
					}
				});
				break;
			case SaveGame.SaveFixes.FixRingSheetIndex:
				Utility.iterateAllItems(delegate(Item item)
				{
					if (item is Ring && item.ParentSheetIndex == -1)
					{
						item.ParentSheetIndex = (item as Ring).indexInTileSheet.Value;
					}
				});
				break;
			case SaveGame.SaveFixes.FixBeachFarmBushes:
			{
				if (whichFarm != 6)
				{
					break;
				}
				Farm farm = getFarm();
				Vector2[] array2 = new Vector2[4]
				{
					new Vector2(77f, 4f),
					new Vector2(78f, 3f),
					new Vector2(83f, 4f),
					new Vector2(83f, 3f)
				};
				Vector2[] array3 = array2;
				foreach (Vector2 vector in array3)
				{
					foreach (LargeTerrainFeature largeTerrainFeature in farm.largeTerrainFeatures)
					{
						if (largeTerrainFeature.tilePosition.Value == vector)
						{
							if (largeTerrainFeature is Bush && largeTerrainFeature is Bush bush)
							{
								bush.tilePosition.Value = new Vector2(bush.tilePosition.X, bush.tilePosition.Y + 1f);
							}
							break;
						}
					}
				}
				break;
			}
			case SaveGame.SaveFixes.AddCampfireKit:
				if (player.foragingLevel.Value >= 9 && !player.craftingRecipes.ContainsKey("Cookout Kit"))
				{
					player.craftingRecipes.Add("Cookout Kit", 0);
				}
				break;
			case SaveGame.SaveFixes.OstrichIncubatorFragility:
				Utility.iterateAllItems(delegate(Item item)
				{
					if (item is Object && (item as Object).Fragility == 2 && item.Name == "Ostrich Incubator")
					{
						(item as Object).Fragility = 0;
					}
				});
				break;
			case SaveGame.SaveFixes.FixBotchedBundleData:
			{
				Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
				foreach (string key3 in netWorldState.Value.BundleData.Keys)
				{
					string text = netWorldState.Value.BundleData[key3];
					List<string> list = new List<string>(text.Split('/'));
					int result = 0;
					while (list.Count > 4 && !int.TryParse(list[list.Count - 1], out result))
					{
						string text2 = list[list.Count - 1];
						if (char.IsDigit(text2[text2.Length - 1]) && text2.Contains(":") && text2.Contains("\\"))
						{
							break;
						}
						list.RemoveAt(list.Count - 1);
					}
					dictionary2[key3] = string.Join("/", list);
				}
				netWorldState.Value.SetBundleData(dictionary2);
				break;
			}
			case SaveGame.SaveFixes.LeoChildrenFix:
				Utility.FixChildNameCollisions();
				break;
			case SaveGame.SaveFixes.Leo6HeartGermanFix:
				if (Utility.HasAnyPlayerSeenEvent(6497428) && !MasterPlayer.hasOrWillReceiveMail("leoMoved"))
				{
					addMailForTomorrow("leoMoved", noLetter: true, sendToEveryone: true);
					player.team.requestLeoMove.Fire();
				}
				break;
			case SaveGame.SaveFixes.BirdieQuestRemovedFix:
			{
				foreach (Farmer allFarmer2 in getAllFarmers())
				{
					if (allFarmer2.hasQuest(130))
					{
						foreach (Quest item6 in allFarmer2.questLog)
						{
							if ((int)item6.id == 130)
							{
								item6.canBeCancelled.Value = true;
							}
						}
					}
					if (allFarmer2.hasOrWillReceiveMail("birdieQuestBegun") && !allFarmer2.hasOrWillReceiveMail("birdieQuestFinished") && !allFarmer2.hasQuest(130))
					{
						allFarmer2.addQuest(130);
					}
				}
				break;
			}
			case SaveGame.SaveFixes.SkippedSummit:
				if (!MasterPlayer.mailReceived.Contains("Farm_Eternal"))
				{
					break;
				}
				{
					foreach (Farmer allFarmer3 in getAllFarmers())
					{
						if (allFarmer3.mailReceived.Contains("Summit_event") && !allFarmer3.songsHeard.Contains("end_credits"))
						{
							allFarmer3.mailReceived.Remove("Summit_event");
						}
					}
					break;
				}
			case SaveGame.SaveFixes.MobileHPRecalc:
				player.professions.Set(player.professions.Distinct().ToList());
				player.maxHealth = 100;
				player.maxHealth += player.CombatLevel * 5;
				if ((int)player.combatLevel >= 10)
				{
					player.maxHealth -= 5;
				}
				if ((int)player.combatLevel >= 5)
				{
					player.maxHealth -= 5;
				}
				if (player.professions.Contains(24))
				{
					player.maxHealth += 15;
				}
				if (player.professions.Contains(27))
				{
					player.maxHealth += 25;
				}
				if (player.mailReceived.Contains("qiCave"))
				{
					player.maxHealth += 25;
				}
				player.health = player.maxHealth;
				break;
			case SaveGame.SaveFixes.Level9PuddingFishingRecipe:
				break;
			}
		}

		public static void recalculateLostBookCount()
		{
			int num = 0;
			foreach (Farmer allFarmer in getAllFarmers())
			{
				if (allFarmer.archaeologyFound.ContainsKey(102) && allFarmer.archaeologyFound[102][0] > 0)
				{
					num = Math.Max(num, allFarmer.archaeologyFound[102][0]);
					if (!allFarmer.mailForTomorrow.Contains("lostBookFound%&NL&%"))
					{
						allFarmer.mailForTomorrow.Add("lostBookFound%&NL&%");
					}
				}
			}
			netWorldState.Value.LostBooksFound.Value = num;
		}

		public static void MarkFloorChestAsCollectedIfNecessary(int floor_number)
		{
			if (MineShaft.permanentMineChanges != null && MineShaft.permanentMineChanges.ContainsKey(floor_number) && MineShaft.permanentMineChanges[floor_number].chestsLeft <= 0)
			{
				player.chestConsumedMineLevels[floor_number] = true;
			}
		}

		public static void pauseThenDoFunction(int pauseTime, afterFadeFunction function)
		{
			afterPause = function;
			pauseThenDoFunctionTimer = pauseTime;
		}

		public static string dayOrNight()
		{
			string result = "_day";
			int dayOfYear = DateTime.Now.DayOfYear;
			int num = (int)(1.75 * Math.Sin(Math.PI * 2.0 / 365.0 * (double)dayOfYear - 79.0) + 18.75);
			if (DateTime.Now.TimeOfDay.TotalHours >= (double)num || DateTime.Now.TimeOfDay.TotalHours < 5.0)
			{
				result = "_night";
			}
			return result;
		}

		protected internal virtual LocalizedContentManager CreateContentManager(IServiceProvider serviceProvider, string rootDirectory)
		{
			return new LocalizedContentManager(serviceProvider, rootDirectory);
		}

		public void Instance_LoadContent()
		{
			LoadContent();
			StartupPreferences startupPreferences = new StartupPreferences();
			startupPreferences.loadPreferences(async: false);
			if (startupPreferences.clientOptions != null)
			{
				options = startupPreferences.clientOptions;
				if (startupPreferences.clientOptions.xEdge > 0)
				{
					xEdge = startupPreferences.clientOptions.xEdge;
				}
				if (startupPreferences.clientOptions.toolbarPadding > 0)
				{
					toolbarPaddingX = startupPreferences.clientOptions.toolbarPadding;
				}
				if (soundBank != null)
				{
					soundCategory.SetVolume(options.soundVolumeLevel);
					if (musicCategory != null)
					{
						musicCategory.SetVolume(options.musicVolumeLevel);
					}
					footstepCategory.SetVolume(options.footstepVolumeLevel);
					ambientCategory.SetVolume(options.ambientVolumeLevel);
				}
				if (!options.autoSave)
				{
					SaveGame.deleteBackupIndices();
				}
				if (virtualJoypad != null)
				{
					virtualJoypad.UpdateSettings();
				}
			}
			loadTitleTexture();
		}

		protected override void LoadContent()
		{
			content = CreateContentManager(base.Content.ServiceProvider, base.Content.RootDirectory);
			xTileContent = CreateContentManager(content.ServiceProvider, content.RootDirectory);
			mapDisplayDevice = new XnaDisplayDevice(content, base.GraphicsDevice);
			CraftingRecipe.InitShared();
			Critter.InitShared();
			spriteBatch = new SpriteBatch(base.GraphicsDevice);
			concessionsSpriteSheet = content.Load<Texture2D>("LooseSprites\\Concessions");
			birdsSpriteSheet = content.Load<Texture2D>("LooseSprites\\birds");
			daybg = content.Load<Texture2D>("LooseSprites\\daybg");
			nightbg = content.Load<Texture2D>("LooseSprites\\nightbg");
			menuTexture = content.Load<Texture2D>("Maps\\MenuTiles");
			uncoloredMenuTexture = content.Load<Texture2D>("Maps\\MenuTilesUncolored");
			lantern = content.Load<Texture2D>("LooseSprites\\Lighting\\lantern");
			windowLight = content.Load<Texture2D>("LooseSprites\\Lighting\\windowLight");
			sconceLight = content.Load<Texture2D>("LooseSprites\\Lighting\\sconceLight");
			cauldronLight = content.Load<Texture2D>("LooseSprites\\Lighting\\greenLight");
			indoorWindowLight = content.Load<Texture2D>("LooseSprites\\Lighting\\indoorWindowLight");
			shadowTexture = content.Load<Texture2D>("LooseSprites\\shadow");
			mouseCursors = content.Load<Texture2D>("LooseSprites\\Cursors");
			mouseCursors2 = content.Load<Texture2D>("LooseSprites\\Cursors2");
			giftboxTexture = content.Load<Texture2D>("LooseSprites\\Giftbox");
			controllerMaps = content.Load<Texture2D>("LooseSprites\\ControllerMaps");
			animations = content.Load<Texture2D>("TileSheets\\animations");
			achievements = content.Load<Dictionary<int, string>>("Data\\Achievements");
			fadeToBlackRect = new Texture2D(base.GraphicsDevice, 1, 1, mipmap: false, SurfaceFormat.Color);
			Color[] data = new Color[1] { Color.White };
			fadeToBlackRect.SetData(data);
			dialogueWidth = Math.Min(1024, graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width - 256);
			NameSelect.load();
			NPCGiftTastes = content.Load<Dictionary<string, string>>("Data\\NPCGiftTastes");
			data = new Color[1];
			staminaRect = new Texture2D(base.GraphicsDevice, 1, 1, mipmap: false, SurfaceFormat.Color);
			onScreenMenus.Clear();
			toolbar = new Toolbar();
			onScreenMenus.Add(toolbar);
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = new Color(255, 255, 255, 255);
			}
			staminaRect.SetData(data);
			saveOnNewDay = true;
			littleEffect = new Texture2D(base.GraphicsDevice, 4, 4, mipmap: false, SurfaceFormat.Color);
			data = new Color[16];
			for (int j = 0; j < data.Length; j++)
			{
				data[j] = new Color(255, 255, 255, 255);
			}
			littleEffect.SetData(data);
			for (int k = 0; k < 70; k++)
			{
				rainDrops.Add(GetRainDrop(viewport.X + random.Next(viewport.Width), viewport.Y + random.Next(viewport.Height), random.Next(4), random.Next(70)));
			}
			MobileLoadContent();
			dayTimeMoneyBox = new DayTimeMoneyBox();
			onScreenMenus.Add(dayTimeMoneyBox);
			buffsDisplay = new BuffsDisplay();
			onScreenMenus.Add(buffsDisplay);
			dialogueFont = content.Load<SpriteFont>("Fonts\\SpriteFont1");
			dialogueFont.LineSpacing = 42;
			smallFont = content.Load<SpriteFont>("Fonts\\SmallFont");
			smallFont.LineSpacing = 26;
			tinyFont = content.Load<SpriteFont>("Fonts\\tinyFont");
			tinyFontBorder = content.Load<SpriteFont>("Fonts\\tinyFontBorder");
			objectSpriteSheet = content.Load<Texture2D>("Maps\\springobjects");
			cropSpriteSheet = content.Load<Texture2D>("TileSheets\\crops");
			emoteSpriteSheet = content.Load<Texture2D>("TileSheets\\emotes");
			debrisSpriteSheet = content.Load<Texture2D>("TileSheets\\debris");
			bigCraftableSpriteSheet = content.Load<Texture2D>("TileSheets\\Craftables");
			rainTexture = content.Load<Texture2D>("TileSheets\\rain");
			buffsIcons = content.Load<Texture2D>("TileSheets\\BuffsIcons");
			objectInformation = content.Load<Dictionary<int, string>>("Data\\ObjectInformation");
			clothingInformation = content.Load<Dictionary<int, string>>("Data\\ClothingInformation");
			objectContextTags = content.Load<Dictionary<string, string>>("Data\\ObjectContextTags");
			bigCraftablesInformation = content.Load<Dictionary<int, string>>("Data\\BigCraftablesInformation");
			if (gameMode == 4)
			{
				fadeToBlackAlpha = -0.5f;
				fadeIn = true;
			}
			if (random.NextDouble() < 0.7)
			{
				isDebrisWeather = true;
				populateDebrisWeatherArray(debrisWeather);
			}
			FarmerRenderer.hairStylesTexture = content.Load<Texture2D>("Characters\\Farmer\\hairstyles");
			FarmerRenderer.shirtsTexture = content.Load<Texture2D>("Characters\\Farmer\\shirts");
			FarmerRenderer.pantsTexture = content.Load<Texture2D>("Characters\\Farmer\\pants");
			FarmerRenderer.hatsTexture = content.Load<Texture2D>("Characters\\Farmer\\hats");
			FarmerRenderer.accessoriesTexture = content.Load<Texture2D>("Characters\\Farmer\\accessories");
			Furniture.furnitureTexture = content.Load<Texture2D>("TileSheets\\furniture");
			Furniture.furnitureFrontTexture = content.Load<Texture2D>("TileSheets\\furnitureFront");
			MapSeat.mapChairTexture = content.Load<Texture2D>("TileSheets\\ChairTiles");
			SpriteText.spriteTexture = content.Load<Texture2D>("LooseSprites\\font_bold");
			SpriteText.coloredTexture = content.Load<Texture2D>("LooseSprites\\font_colored");
			Tool.weaponsTexture = content.Load<Texture2D>("TileSheets\\weapons");
			Projectile.projectileSheet = content.Load<Texture2D>("TileSheets\\Projectiles");
			_shortDayDisplayName[0] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3042");
			_shortDayDisplayName[1] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3043");
			_shortDayDisplayName[2] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3044");
			_shortDayDisplayName[3] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3045");
			_shortDayDisplayName[4] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3046");
			_shortDayDisplayName[5] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3047");
			_shortDayDisplayName[6] = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3048");
			netWorldState = new NetRoot<IWorldState>(new NetWorldState());
			resetPlayer();
			setGameMode(0);
		}

		public static void resetPlayer()
		{
			List<Item> initialTools = Farmer.initialTools();
			player = new Farmer(new FarmerSprite(null), new Vector2(192f, 192f), 1, "", initialTools, isMale: true);
		}

		public static void resetVariables()
		{
			xLocationAfterWarp = 0;
			yLocationAfterWarp = 0;
			gameTimeInterval = 0;
			currentQuestionChoice = 0;
			currentDialogueCharacterIndex = 0;
			dialogueTypingInterval = 0;
			dayOfMonth = 0;
			year = 1;
			timeOfDay = 600;
			timeOfDayAfterFade = -1;
			numberOfSelectedItems = -1;
			priceOfSelectedItem = 0;
			currentWallpaper = 0;
			farmerWallpaper = 22;
			wallpaperPrice = 75;
			currentFloor = 3;
			FarmerFloor = 29;
			floorPrice = 75;
			facingDirectionAfterWarp = 0;
			dialogueWidth = 0;
			menuChoice = 0;
			tvStation = -1;
			currentBillboard = 0;
			facingDirectionAfterWarp = 0;
			tmpTimeOfDay = 0;
			percentageToWinStardewHero = 70;
			mouseClickPolling = 0;
			weatherIcon = 0;
			hitShakeTimer = 0;
			staminaShakeTimer = 0;
			pauseThenDoFunctionTimer = 0;
			weatherForTomorrow = 0;
			currentSongIndex = 3;
		}

		public static void playSound(string cueName)
		{
			if (soundBank != null)
			{
				try
				{
					soundBank.PlayCue(cueName);
				}
				catch (Exception ex)
				{
					debugOutput = parseText(ex.Message);
					Console.WriteLine(ex);
				}
			}
		}

		public static void playSoundPitched(string cueName, int pitch)
		{
			if (soundBank != null)
			{
				try
				{
					ICue cue = soundBank.GetCue(cueName);
					cue.SetVariable("Pitch", pitch);
					cue.Play();
				}
				catch (Exception ex)
				{
					debugOutput = parseText(ex.Message);
					Console.WriteLine(ex);
				}
			}
		}

		public static void setRichPresence(string friendlyName, object argument = null)
		{
			if (friendlyName == null)
			{
				return;
			}
			switch (friendlyName.Length)
			{
			case 8:
				switch (friendlyName[0])
				{
				case 'l':
					if (friendlyName == "location")
					{
						debugPresenceString = $"At {argument}";
					}
					break;
				case 'f':
					if (friendlyName == "festival")
					{
						debugPresenceString = $"At {argument}";
					}
					break;
				case 'm':
					if (friendlyName == "minigame")
					{
						debugPresenceString = $"Playing {argument}";
					}
					break;
				case 'e':
					if (friendlyName == "earnings")
					{
						debugPresenceString = $"Made {argument}g last night";
					}
					break;
				}
				break;
			case 7:
				switch (friendlyName[0])
				{
				case 'f':
					if (friendlyName == "fishing")
					{
						debugPresenceString = $"Fishing at {argument}";
					}
					break;
				case 'w':
					if (friendlyName == "wedding")
					{
						debugPresenceString = $"Getting married to {argument}";
					}
					break;
				}
				break;
			case 5:
				if (friendlyName == "menus")
				{
					debugPresenceString = "In menus";
				}
				break;
			case 9:
				if (friendlyName == "giantcrop")
				{
					debugPresenceString = $"Just harvested a Giant {argument}";
				}
				break;
			case 6:
				break;
			}
		}

		public static void GenerateBundles(BundleType bundle_type, bool use_seed = true)
		{
			Random random = null;
			random = ((!use_seed) ? new Random() : new Random((int)uniqueIDForThisGame * 9));
			if (bundle_type == BundleType.Remixed)
			{
				BundleGenerator bundleGenerator = new BundleGenerator();
				Dictionary<string, string> bundleData = bundleGenerator.Generate("Data\\RandomBundles", random);
				netWorldState.Value.SetBundleData(bundleData);
			}
			else
			{
				netWorldState.Value.SetBundleData(content.LoadBase<Dictionary<string, string>>("Data\\Bundles"));
			}
		}

		public void SetNewGameOption<T>(string key, T val)
		{
			newGameSetupOptions[key] = val;
		}

		public T GetNewGameOption<T>(string key)
		{
			if (!newGameSetupOptions.ContainsKey(key))
			{
				return default(T);
			}
			return (T)newGameSetupOptions[key];
		}

		public static void loadForNewGame(bool loadedGame = false)
		{
			if (startingGameSeed.HasValue)
			{
				uniqueIDForThisGame = startingGameSeed.Value;
			}
			specialCurrencyDisplay = new SpecialCurrencyDisplay();
			flushLocationLookup();
			locations.Clear();
			mailbox.Clear();
			currentLightSources.Clear();
			if (dealerCalicoJackTotal != null)
			{
				dealerCalicoJackTotal.Clear();
			}
			questionChoices.Clear();
			hudMessages.Clear();
			weddingToday = false;
			timeOfDay = 600;
			currentSeason = "spring";
			if (!loadedGame)
			{
				year = 1;
			}
			dayOfMonth = 0;
			pickingTool = false;
			isQuestion = false;
			nonWarpFade = false;
			particleRaining = false;
			newDay = false;
			inMine = false;
			menuUp = false;
			eventUp = false;
			viewportFreeze = false;
			eventOver = false;
			nameSelectUp = false;
			screenGlow = false;
			screenGlowHold = false;
			screenGlowUp = false;
			progressBar = false;
			isRaining = false;
			killScreen = false;
			coopDwellerBorn = false;
			messagePause = false;
			isDebrisWeather = false;
			boardingBus = false;
			listeningForKeyControlDefinitions = false;
			weddingToday = false;
			exitToTitle = false;
			isRaining = false;
			dialogueUp = false;
			currentBillboard = 0;
			postExitToTitleCallback = null;
			displayHUD = true;
			messageAfterPause = "";
			fertilizer = "";
			samBandName = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156");
			slotResult = "";
			background = null;
			currentCursorTile = Vector2.Zero;
			if (!loadedGame)
			{
				lastAppliedSaveFix = SaveGame.SaveFixes.MobileHPRecalc;
			}
			resetVariables();
			chanceToRainTomorrow = 0.0;
			player.team.sharedDailyLuck.Value = 0.001;
			if (!loadedGame)
			{
				options = new Options();
				options.LoadDefaultOptions();
				initializeVolumeLevels();
			}
			game1.CheckGamepadMode();
			cropsOfTheWeek = Utility.cropsOfTheWeek();
			onScreenMenus.Add(chatBox = new ChatBox());
			outdoorLight = Color.White;
			ambientLight = Color.White;
			int num = Game1.random.Next(194, 240);
			while (Utility.getForbiddenDishesOfTheDay().Contains(num))
			{
				num = Game1.random.Next(194, 240);
			}
			int initialStack = Game1.random.Next(1, 4 + ((Game1.random.NextDouble() < 0.08) ? 10 : 0));
			netWorldState.Value.DishOfTheDay.Value = new Object(Vector2.Zero, num, initialStack);
			locations.Clear();
			locations.Add(new Farm("Maps\\" + Farm.getMapNameFromTypeInt(whichFarm), "Farm"));
			getFarm().BuildStartingCabins();
			forceSnapOnNextViewportUpdate = true;
			currentLocation = new FarmHouse("Maps\\FarmHouse", "FarmHouse");
			currentLocation.map.LoadTileSheets(mapDisplayDevice);
			locations.Add(currentLocation);
			if (whichFarm == 3 || getFarm().ShouldSpawnMountainOres())
			{
				for (int i = 0; i < 28; i++)
				{
					getFarm().doDailyMountainFarmUpdate();
				}
			}
			else if (whichFarm == 5)
			{
				for (int j = 0; j < 10; j++)
				{
					getFarm().doDailyMountainFarmUpdate();
				}
			}
			locations.Add(new FarmCave("Maps\\FarmCave", "FarmCave"));
			locations.Add(new Town("Maps\\Town", "Town"));
			locations.Add(new GameLocation("Maps\\JoshHouse", "JoshHouse"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\George", 0, 16, 32), new Vector2(1024f, 1408f), "JoshHouse", 0, "George", datable: false, null, content.Load<Texture2D>("Portraits\\George")));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Evelyn", 0, 16, 32), new Vector2(128f, 1088f), "JoshHouse", 1, "Evelyn", datable: false, null, content.Load<Texture2D>("Portraits\\Evelyn")));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Alex", 0, 16, 32), new Vector2(1216f, 320f), "JoshHouse", 3, "Alex", datable: true, null, content.Load<Texture2D>("Portraits\\Alex")));
			locations.Add(new GameLocation("Maps\\HaleyHouse", "HaleyHouse"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Emily", 0, 16, 32), new Vector2(1024f, 320f), "HaleyHouse", 2, "Emily", datable: true, null, content.Load<Texture2D>("Portraits\\Emily")));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Haley", 0, 16, 32), new Vector2(512f, 448f), "HaleyHouse", 1, "Haley", datable: true, null, content.Load<Texture2D>("Portraits\\Haley")));
			locations.Add(new GameLocation("Maps\\SamHouse", "SamHouse"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Jodi", 0, 16, 32), new Vector2(256f, 320f), "SamHouse", 0, "Jodi", datable: false, null, content.Load<Texture2D>("Portraits\\Jodi")));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Sam", 0, 16, 32), new Vector2(1408f, 832f), "SamHouse", 1, "Sam", datable: true, null, content.Load<Texture2D>("Portraits\\Sam")));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Vincent", 0, 16, 32), new Vector2(640f, 1472f), "SamHouse", 2, "Vincent", datable: false, null, content.Load<Texture2D>("Portraits\\Vincent")));
			addKentIfNecessary();
			locations.Add(new GameLocation("Maps\\Blacksmith", "Blacksmith"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Clint", 0, 16, 32), new Vector2(192f, 832f), "Blacksmith", 2, "Clint", datable: false, null, content.Load<Texture2D>("Portraits\\Clint")));
			locations.Add(new ManorHouse("Maps\\ManorHouse", "ManorHouse"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Lewis", 0, 16, 32), new Vector2(512f, 320f), "ManorHouse", 0, "Lewis", datable: false, null, content.Load<Texture2D>("Portraits\\Lewis")));
			locations.Add(new SeedShop("Maps\\SeedShop", "SeedShop"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Caroline", 0, 16, 32), new Vector2(1408f, 320f), "SeedShop", 2, "Caroline", datable: false, null, content.Load<Texture2D>("Portraits\\Caroline")));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Abigail", 0, 16, 32), new Vector2(64f, 580f), "SeedShop", 3, "Abigail", datable: true, null, content.Load<Texture2D>("Portraits\\Abigail")));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Pierre", 0, 16, 32), new Vector2(256f, 1088f), "SeedShop", 2, "Pierre", datable: false, null, content.Load<Texture2D>("Portraits\\Pierre")));
			locations.Add(new GameLocation("Maps\\Saloon", "Saloon"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Gus", 0, 16, 32), new Vector2(1152f, 384f), "Saloon", 2, "Gus", datable: false, null, content.Load<Texture2D>("Portraits\\Gus")));
			locations.Add(new GameLocation("Maps\\Trailer", "Trailer"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Pam", 0, 16, 32), new Vector2(960f, 256f), "Trailer", 2, "Pam", datable: false, null, content.Load<Texture2D>("Portraits\\Pam")));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Penny", 0, 16, 32), new Vector2(256f, 576f), "Trailer", 1, "Penny", datable: true, null, content.Load<Texture2D>("Portraits\\Penny")));
			locations.Add(new GameLocation("Maps\\Hospital", "Hospital"));
			locations.Add(new GameLocation("Maps\\HarveyRoom", "HarveyRoom"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Harvey", 0, 16, 32), new Vector2(832f, 256f), "HarveyRoom", 1, "Harvey", datable: true, null, content.Load<Texture2D>("Portraits\\Harvey")));
			locations.Add(new Beach("Maps\\Beach", "Beach"));
			locations.Add(new GameLocation("Maps\\ElliottHouse", "ElliottHouse"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Elliott", 0, 16, 32), new Vector2(64f, 320f), "ElliottHouse", 0, "Elliott", datable: true, null, content.Load<Texture2D>("Portraits\\Elliott")));
			locations.Add(new Mountain("Maps\\Mountain", "Mountain"));
			locations.Add(new GameLocation("Maps\\ScienceHouse", "ScienceHouse"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Maru", 0, 16, 32), new Vector2(128f, 256f), "ScienceHouse", 3, "Maru", datable: true, null, content.Load<Texture2D>("Portraits\\Maru")));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Robin", 0, 16, 32), new Vector2(1344f, 256f), "ScienceHouse", 1, "Robin", datable: false, null, content.Load<Texture2D>("Portraits\\Robin")));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Demetrius", 0, 16, 32), new Vector2(1216f, 256f), "ScienceHouse", 1, "Demetrius", datable: false, null, content.Load<Texture2D>("Portraits\\Demetrius")));
			locations.Add(new GameLocation("Maps\\SebastianRoom", "SebastianRoom"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Sebastian", 0, 16, 32), new Vector2(640f, 576f), "SebastianRoom", 1, "Sebastian", datable: true, null, content.Load<Texture2D>("Portraits\\Sebastian")));
			GameLocation gameLocation = new GameLocation("Maps\\Tent", "Tent");
			locations.Add(gameLocation);
			gameLocation.addCharacter(new NPC(new AnimatedSprite("Characters\\Linus", 0, 16, 32), new Vector2(2f, 2f) * 64f, "Tent", 2, "Linus", datable: false, null, content.Load<Texture2D>("Portraits\\Linus")));
			locations.Add(new Forest("Maps\\Forest", "Forest"));
			locations.Add(new WizardHouse("Maps\\WizardHouse", "WizardHouse"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Wizard", 0, 16, 32), new Vector2(192f, 1088f), "WizardHouse", 2, "Wizard", datable: false, null, content.Load<Texture2D>("Portraits\\Wizard")));
			locations.Add(new GameLocation("Maps\\AnimalShop", "AnimalShop"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Marnie", 0, 16, 32), new Vector2(768f, 896f), "AnimalShop", 2, "Marnie", datable: false, null, content.Load<Texture2D>("Portraits\\Marnie")));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Shane", 0, 16, 32), new Vector2(1600f, 384f), "AnimalShop", 3, "Shane", datable: true, null, content.Load<Texture2D>("Portraits\\Shane")));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Jas", 0, 16, 32), new Vector2(256f, 384f), "AnimalShop", 2, "Jas", datable: false, null, content.Load<Texture2D>("Portraits\\Jas")));
			locations.Add(new GameLocation("Maps\\LeahHouse", "LeahHouse"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Leah", 0, 16, 32), new Vector2(192f, 448f), "LeahHouse", 3, "Leah", datable: true, null, content.Load<Texture2D>("Portraits\\Leah")));
			locations.Add(new BusStop("Maps\\BusStop", "BusStop"));
			locations.Add(new Mine("Maps\\Mine", "Mine"));
			locations[locations.Count - 1].objects.Add(new Vector2(27f, 8f), new Object(Vector2.Zero, 78));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Dwarf", 0, 16, 24), new Vector2(2752f, 384f), "Mine", 2, "Dwarf", datable: false, null, content.Load<Texture2D>("Portraits\\Dwarf"))
			{
				Breather = false
			});
			locations.Add(new Sewer("Maps\\Sewer", "Sewer"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Krobus", 0, 16, 24), new Vector2(31f, 17f) * 64f, "Sewer", 2, "Krobus", datable: false, null, content.Load<Texture2D>("Portraits\\Krobus")));
			locations.Add(new BugLand("Maps\\BugLand", "BugLand"));
			locations.Add(new Desert("Maps\\Desert", "Desert"));
			locations.Add(new Club("Maps\\Club", "Club"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\MrQi", 0, 16, 32), new Vector2(512f, 256f), "Club", 0, "Mister Qi", datable: false, null, content.Load<Texture2D>("Portraits\\MrQi")));
			locations.Add(new GameLocation("Maps\\SandyHouse", "SandyHouse"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Sandy", 0, 16, 32), new Vector2(128f, 320f), "SandyHouse", 2, "Sandy", datable: false, null, content.Load<Texture2D>("Portraits\\Sandy")));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Bouncer", 0, 16, 32), new Vector2(1088f, 192f), "SandyHouse", 2, "Bouncer", datable: false, null, content.Load<Texture2D>("Portraits\\Bouncer")));
			locations.Add(new LibraryMuseum("Maps\\ArchaeologyHouse", "ArchaeologyHouse"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Gunther", 0, 16, 32), new Vector2(192f, 512f), "ArchaeologyHouse", 2, "Gunther", datable: false, null, content.Load<Texture2D>("Portraits\\Gunther")));
			locations.Add(new GameLocation("Maps\\WizardHouseBasement", "WizardHouseBasement"));
			locations.Add(new AdventureGuild("Maps\\AdventureGuild", "AdventureGuild"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Marlon", 0, 16, 32), new Vector2(320f, 704f), "AdventureGuild", 2, "Marlon", datable: false, null, content.Load<Texture2D>("Portraits\\Marlon")));
			locations.Add(new Woods("Maps\\Woods", "Woods"));
			locations.Add(new Railroad("Maps\\Railroad", "Railroad"));
			locations.Add(new GameLocation("Maps\\WitchSwamp", "WitchSwamp"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Henchman", 0, 16, 32), new Vector2(1280f, 1856f), "WitchSwamp", 2, "Henchman", datable: false, null, content.Load<Texture2D>("Portraits\\Henchman")));
			locations.Add(new GameLocation("Maps\\WitchHut", "WitchHut"));
			locations.Add(new GameLocation("Maps\\WitchWarpCave", "WitchWarpCave"));
			locations.Add(new Summit("Maps\\Summit", "Summit"));
			locations.Add(new FishShop("Maps\\FishShop", "FishShop"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\Willy", 0, 16, 32), new Vector2(320f, 256f), "FishShop", 2, "Willy", datable: false, null, content.Load<Texture2D>("Portraits\\Willy")));
			locations.Add(new GameLocation("Maps\\BathHouse_Entry", "BathHouse_Entry"));
			locations.Add(new GameLocation("Maps\\BathHouse_MensLocker", "BathHouse_MensLocker"));
			locations.Add(new GameLocation("Maps\\BathHouse_WomensLocker", "BathHouse_WomensLocker"));
			locations.Add(new BathHousePool("Maps\\BathHouse_Pool", "BathHouse_Pool"));
			locations.Add(new CommunityCenter("CommunityCenter"));
			locations.Add(new JojaMart("Maps\\JojaMart", "JojaMart"));
			locations.Add(new GameLocation("Maps\\Greenhouse", "Greenhouse"));
			locations.Add(new GameLocation("Maps\\SkullCave", "SkullCave"));
			locations.Add(new GameLocation("Maps\\Backwoods", "Backwoods"));
			locations.Add(new GameLocation("Maps\\Tunnel", "Tunnel"));
			locations.Add(new GameLocation("Maps\\Trailer_big", "Trailer_Big"));
			locations.Add(new Cellar("Maps\\Cellar", "Cellar"));
			for (int k = 1; k < (int)netWorldState.Value.HighestPlayerLimit; k++)
			{
				locations.Add(new Cellar("Maps\\Cellar", "Cellar" + (k + 1)));
			}
			locations.Add(new BeachNightMarket("Maps\\Beach-NightMarket", "BeachNightMarket"));
			locations.Add(new MermaidHouse("Maps\\MermaidHouse", "MermaidHouse"));
			locations.Add(new Submarine("Maps\\Submarine", "Submarine"));
			locations.Add(new AbandonedJojaMart("Maps\\AbandonedJojaMart", "AbandonedJojaMart"));
			locations.Add(new MovieTheater("Maps\\MovieTheater", "MovieTheater"));
			locations.Add(new GameLocation("Maps\\Sunroom", "Sunroom"));
			locations.Add(new BoatTunnel("Maps\\BoatTunnel", "BoatTunnel"));
			locations.Add(new IslandSouth("Maps\\Island_S", "IslandSouth"));
			locations.Add(new IslandSouthEast("Maps\\Island_SE", "IslandSouthEast"));
			locations.Add(new IslandSouthEastCave("Maps\\IslandSouthEastCave", "IslandSouthEastCave"));
			locations.Add(new IslandEast("Maps\\Island_E", "IslandEast"));
			locations.Add(new IslandWest("Maps\\Island_W", "IslandWest"));
			addBirdieIfNecessary();
			locations.Add(new IslandNorth("Maps\\Island_N", "IslandNorth"));
			locations.Add(new IslandHut("Maps\\Island_Hut", "IslandHut"));
			locations.Add(new IslandWestCave1("Maps\\IslandWestCave1", "IslandWestCave1"));
			locations.Add(new IslandLocation("Maps\\IslandNorthCave1", "IslandNorthCave1"));
			locations.Add(new IslandFieldOffice("Maps\\Island_FieldOffice", "IslandFieldOffice"));
			locations.Add(new IslandFarmHouse("Maps\\IslandFarmHouse", "IslandFarmHouse"));
			locations.Add(new IslandLocation("Maps\\Island_CaptainRoom", "CaptainRoom"));
			locations.Add(new IslandShrine("Maps\\Island_Shrine", "IslandShrine"));
			locations.Add(new IslandFarmCave("Maps\\Island_FarmCave", "IslandFarmCave"));
			locations.Add(new Caldera("Maps\\Caldera", "Caldera"));
			locations.Add(new GameLocation("Maps\\LeoTreeHouse", "LeoTreeHouse"));
			locations.Add(new IslandLocation("Maps\\QiNutRoom", "QiNutRoom"));
			locations[locations.Count - 1].addCharacter(new NPC(new AnimatedSprite("Characters\\MrQi", 0, 16, 32), new Vector2(448f, 256f), "QiNutRoom", 0, "Mister Qi", datable: false, null, content.Load<Texture2D>("Portraits\\MrQi")));
			if (!loadedGame)
			{
				foreach (GameLocation location in locations)
				{
					if (location is IslandLocation)
					{
						(location as IslandLocation).AddAdditionalWalnutBushes();
					}
				}
			}
			AddModNPCs();
			NPC.populateRoutesFromLocationToLocationList();
			if (!loadedGame)
			{
				GenerateBundles(bundleType);
				foreach (string value in netWorldState.Value.BundleData.Values)
				{
					string text = value.Split('/')[2];
					string[] array = text.Split(' ');
					if (!game1.GetNewGameOption<bool>("YearOneCompletable"))
					{
						continue;
					}
					for (int l = 0; l < array.Length; l += 3)
					{
						if (array[l] == "266")
						{
							int num2 = 16;
							num2 -= 2;
							int num3 = num2 * 2;
							num3 += 3;
							Random random = new Random((int)uniqueIDForThisGame * 12);
							netWorldState.Value.VisitsUntilY1Guarantee = random.Next(2, num3);
						}
					}
				}
				netWorldState.Value.ShuffleMineChests = game1.GetNewGameOption<MineChestType>("MineChests");
				if (game1.newGameSetupOptions.ContainsKey("SpawnMonstersAtNight"))
				{
					spawnMonstersAtNight = game1.GetNewGameOption<bool>("SpawnMonstersAtNight");
				}
			}
			player.ConvertClothingOverrideToClothesItems();
			player.addQuest(9);
			player.currentLocation = getLocationFromName("FarmHouse");
			player.gameVersion = version;
			hudMessages.Clear();
			hasLoadedGame = true;
			setGraphicsForSeason();
			if (!loadedGame)
			{
				_setSaveName = false;
			}
			game1.newGameSetupOptions.Clear();
			if (virtualJoypad != null)
			{
				virtualJoypad.UpdateSettings();
			}
			updateCellarAssignments();
			if (!loadedGame && netWorldState != null && netWorldState.Value != null)
			{
				netWorldState.Value.RegisterSpecialCurrencies();
			}
		}

		public bool IsFirstInstanceAtThisLocation(GameLocation location, Func<Game1, bool> additional_check = null)
		{
			if (GameRunner.instance.GetFirstInstanceAtThisLocation(location, additional_check) == this)
			{
				return true;
			}
			return false;
		}

		public bool IsLocalCoopJoinable()
		{
			if (GameRunner.instance.gameInstances.Count >= GameRunner.instance.GetMaxSimultaneousPlayers())
			{
				return false;
			}
			if (IsClient)
			{
				return false;
			}
			return true;
		}

		public static void StartLocalMultiplayerIfNecessary()
		{
			if (multiplayerMode == 0)
			{
				Console.WriteLine("Starting multiplayer server for local multiplayer...");
				multiplayerMode = 2;
				if (server == null)
				{
					multiplayer.StartLocalMultiplayerServer();
				}
			}
		}

		public static void EndLocalMultiplayer()
		{
		}

		public static void addParrotBoyIfNecessary()
		{
			if (MasterPlayer.hasOrWillReceiveMail("addedParrotBoy"))
			{
				if (getCharacterFromName("Leo", mustBeVillager: true, useLocationsListOnly: true) == null)
				{
					NPC nPC = new NPC(new AnimatedSprite("Characters\\ParrotBoy", 0, 16, 32), new Vector2(320f, 384f), "IslandHut", 2, "Leo", datable: false, null, content.Load<Texture2D>("Portraits\\ParrotBoy"));
					nPC.Breather = false;
					getLocationFromNameInLocationsList("IslandHut").addCharacter(nPC);
				}
				if (!player.friendshipData.ContainsKey("Leo"))
				{
					player.friendshipData.Add("Leo", new Friendship());
				}
			}
		}

		public static void addBirdieIfNecessary()
		{
			if (getCharacterFromName("Birdie", mustBeVillager: true, useLocationsListOnly: true) == null)
			{
				getLocationFromNameInLocationsList("IslandWest").addCharacter(new NPC(new AnimatedSprite("Characters\\Birdie", 0, 16, 32), new Vector2(1088f, 3712f), "IslandWest", 3, "Birdie", datable: false, null, content.Load<Texture2D>("Portraits\\Birdie")));
			}
		}

		public static void addKentIfNecessary()
		{
			if (year > 1)
			{
				if (getCharacterFromName("Kent", mustBeVillager: true, useLocationsListOnly: true) == null)
				{
					getLocationFromNameInLocationsList("SamHouse").addCharacter(new NPC(new AnimatedSprite("Characters\\Kent", 0, 16, 32), new Vector2(512f, 832f), "SamHouse", 2, "Kent", datable: false, null, content.Load<Texture2D>("Portraits\\Kent")));
				}
				if (!player.friendshipData.ContainsKey("Kent"))
				{
					player.friendshipData.Add("Kent", new Friendship());
				}
			}
		}

		public void Instance_UnloadContent()
		{
			UnloadContent();
		}

		protected override void UnloadContent()
		{
			base.UnloadContent();
			spriteBatch.Dispose();
			content.Unload();
			xTileContent.Unload();
			if (server != null)
			{
				server.stopServer();
			}
		}

		public void errorUpdateLoop()
		{
			if (GetKeyboardState().IsKeyDown(Keys.B))
			{
				Program.GameTesterMode = false;
				gameMode = 3;
			}
			if (GetKeyboardState().IsKeyDown(Keys.Escape))
			{
				Environment.Exit(1);
			}
			Update(new GameTime());
			BeginDraw();
			Draw(new GameTime());
			EndDraw();
		}

		public static void showRedMessage(string message)
		{
			addHUDMessage(new HUDMessage(message, 3));
			if (!message.Contains("Inventory"))
			{
				playSound("cancel");
			}
			else if (!player.mailReceived.Contains("BackpackTip"))
			{
				player.mailReceived.Add("BackpackTip");
				addMailForTomorrow("pierreBackpack");
			}
		}

		public static void showRedMessageUsingLoadString(string loadString)
		{
			showRedMessage(content.LoadString(loadString));
		}

		public static bool didPlayerJustLeftClick(bool ignoreNonMouseHeldInput = false)
		{
			if (input.GetMouseState().LeftButton == ButtonState.Pressed && oldMouseState.LeftButton != ButtonState.Pressed)
			{
				return true;
			}
			if (input.GetGamePadState().Buttons.X == ButtonState.Pressed && (!ignoreNonMouseHeldInput || !oldPadState.IsButtonDown(Buttons.X)))
			{
				return true;
			}
			KeyboardState keyboardState = input.GetKeyboardState();
			if (isOneOfTheseKeysDown(keyboardState, options.useToolButton) && (!ignoreNonMouseHeldInput || areAllOfTheseKeysUp(oldKBState, options.useToolButton)))
			{
				return true;
			}
			if (currentLocation != null)
			{
				return currentLocation.tapToMove.mobileKeyStates.useToolButtonPressed;
			}
			return false;
		}

		public static bool didPlayerJustRightClick(bool ignoreNonMouseHeldInput = false)
		{
			if (input.GetMouseState().RightButton == ButtonState.Pressed && oldMouseState.RightButton != ButtonState.Pressed)
			{
				return true;
			}
			if (input.GetGamePadState().Buttons.A == ButtonState.Pressed && (!ignoreNonMouseHeldInput || !oldPadState.IsButtonDown(Buttons.A)))
			{
				return true;
			}
			KeyboardState keyboardState = input.GetKeyboardState();
			if (isOneOfTheseKeysDown(keyboardState, options.actionButton) && (!ignoreNonMouseHeldInput || !isOneOfTheseKeysDown(oldKBState, options.actionButton)))
			{
				return true;
			}
			if (currentLocation != null)
			{
				return currentLocation.tapToMove.mobileKeyStates.actionButtonPressed;
			}
			return false;
		}

		public static bool didPlayerJustClickAtAll(bool ignoreNonMouseHeldInput = false)
		{
			if (!didPlayerJustLeftClick(ignoreNonMouseHeldInput))
			{
				return didPlayerJustRightClick(ignoreNonMouseHeldInput);
			}
			return true;
		}

		public static void showGlobalMessage(string message)
		{
			addHUDMessage(new HUDMessage(message, ""));
		}

		public static void globalFadeToBlack(afterFadeFunction afterFade = null, float fadeSpeed = 0.02f)
		{
			screenFade.GlobalFadeToBlack(afterFade, fadeSpeed);
		}

		public static void globalFadeToClear(afterFadeFunction afterFade = null, float fadeSpeed = 0.02f)
		{
			screenFade.GlobalFadeToClear(afterFade, fadeSpeed);
		}

		public void CheckGamepadMode()
		{
			bool gamepadControls = options.gamepadControls;
			if (options.gamepadMode == Options.GamepadModes.ForceOn)
			{
				options.gamepadControls = true;
				return;
			}
			if (options.gamepadMode == Options.GamepadModes.ForceOff)
			{
				options.gamepadControls = false;
				return;
			}
			MouseState mouseState = input.GetMouseState();
			KeyboardState keyboardState = GetKeyboardState();
			GamePadState gamePadState = input.GetGamePadState();
			bool flag = false;
			bool flag2 = false;
			if ((mouseState.LeftButton == ButtonState.Pressed || mouseState.MiddleButton == ButtonState.Pressed || mouseState.RightButton == ButtonState.Pressed || mouseState.ScrollWheelValue != _oldScrollWheelValue || ((mouseState.X != _oldMousePosition.X || mouseState.Y != _oldMousePosition.Y) && lastCursorMotionWasMouse) || keyboardState.GetPressedKeys().Length != 0) && (keyboardState.GetPressedKeys().Length != 1 || keyboardState.GetPressedKeys()[0] != Keys.Pause))
			{
				flag = true;
			}
			_oldScrollWheelValue = mouseState.ScrollWheelValue;
			_oldMousePosition.X = mouseState.X;
			_oldMousePosition.Y = mouseState.Y;
			flag2 = isAnyGamePadButtonBeingPressed() || isDPadPressed() || isGamePadThumbstickInMotion() || gamePadState.Triggers.Left != 0f || gamePadState.Triggers.Right != 0f;
			if (_oldGamepadConnectedState != gamePadState.IsConnected)
			{
				_oldGamepadConnectedState = gamePadState.IsConnected;
				if (_oldGamepadConnectedState)
				{
					options.gamepadControls = true;
				}
				else
				{
					options.gamepadControls = false;
					if (instancePlayerOneIndex != (PlayerIndex)(-1) && CanShowPauseMenu() && activeClickableMenu == null)
					{
						activeClickableMenu = new GameMenu();
					}
				}
			}
			if (flag && options.gamepadControls)
			{
				options.gamepadControls = false;
			}
			if (!options.gamepadControls && flag2)
			{
				options.gamepadControls = true;
			}
			if (gamepadControls == options.gamepadControls || !options.gamepadControls)
			{
				return;
			}
			lastMousePositionBeforeFade = new Point(localMultiplayerWindow.Width / 2, localMultiplayerWindow.Height / 2);
			if (activeClickableMenu != null)
			{
				activeClickableMenu.setUpForGamePadMode();
				if (options.SnappyMenus)
				{
					activeClickableMenu.populateClickableComponentList();
					activeClickableMenu.snapToDefaultClickableComponent();
				}
			}
			timerUntilMouseFade = 0;
		}

		public void Instance_Update(GameTime gameTime)
		{
			Update(gameTime);
		}

		protected override void Update(GameTime gameTime)
		{
			input.UpdateStates();
			debugOutput = virtualJoypad.mostRecentlyUsedControlType.ToString();
			if (input.GetGamePadState().IsButtonDown(Buttons.RightStick))
			{
				rightStickHoldTime += gameTime.ElapsedGameTime.Milliseconds;
			}
			_update(gameTime);
			if (IsMultiplayer && player != null)
			{
				player.requestingTimePause.Value = !shouldTimePass(LocalMultiplayer.IsLocalMultiplayer(is_local_only: true));
				if (IsMasterGame)
				{
					bool flag = false;
					if (LocalMultiplayer.IsLocalMultiplayer(is_local_only: true))
					{
						flag = true;
						foreach (Farmer onlineFarmer in getOnlineFarmers())
						{
							if (!onlineFarmer.requestingTimePause.Value)
							{
								flag = false;
								break;
							}
						}
					}
					netWorldState.Value.IsTimePaused = flag;
				}
			}
			Rumble.update(gameTime.ElapsedGameTime.Milliseconds);
			if (options.gamepadControls && thumbstickMotionMargin > 0)
			{
				thumbstickMotionMargin -= gameTime.ElapsedGameTime.Milliseconds;
			}
			if (!input.GetGamePadState().IsButtonDown(Buttons.RightStick))
			{
				rightStickHoldTime = 0;
			}
			base.Update(gameTime);
		}

		public void Instance_OnActivated(object sender, EventArgs args)
		{
			OnActivated(sender, args);
		}

		protected override void OnActivated(object sender, EventArgs args)
		{
			base.OnActivated(sender, args);
			_activatedTick = ticks + 1;
			input.IgnoreKeys(GetKeyboardState().GetPressedKeys());
		}

		public bool HasKeyboardFocus()
		{
			if (keyboardFocusInstance == null)
			{
				return base.IsMainInstance;
			}
			return keyboardFocusInstance == this;
		}

		private void _update(GameTime gameTime)
		{
			if (graphics.GraphicsDevice == null)
			{
				return;
			}
			bool flag = false;
			if (options != null && !takingMapScreenshot && gameMode == 3)
			{
				if (options.baseUIScale != options.desiredUIScale)
				{
					if (options.desiredUIScale < 0f)
					{
						options.desiredUIScale = options.desiredBaseZoomLevel;
					}
					options.baseUIScale = options.desiredUIScale;
					flag = true;
				}
				if (options.desiredBaseZoomLevel != options.baseZoomLevel)
				{
					options.baseZoomLevel = options.desiredBaseZoomLevel;
					forceSnapOnNextViewportUpdate = true;
					flag = true;
				}
			}
			if (flag)
			{
				refreshWindowSettings();
			}
			_updateWholeBackupLoader(gameTime);
			CheckGamepadMode();
			FarmAnimal.NumPathfindingThisTick = 0;
			options.reApplySetOptions();
			if (toggleFullScreen)
			{
				toggleFullscreen();
				toggleFullScreen = false;
			}
			input.Update();
			_updateMobileMenus();
			if (frameByFrame)
			{
				if (GetKeyboardState().IsKeyDown(Keys.Escape) && oldKBState.IsKeyUp(Keys.Escape))
				{
					frameByFrame = false;
				}
				bool flag2 = false;
				if (GetKeyboardState().IsKeyDown(Keys.G) && oldKBState.IsKeyUp(Keys.G))
				{
					flag2 = true;
				}
				if (!flag2)
				{
					oldKBState = GetKeyboardState();
					return;
				}
			}
			if (client != null && client.timedOut)
			{
				multiplayer.clientRemotelyDisconnected(client.pendingDisconnect);
			}
			if (_newDayTask != null)
			{
				if (_newDayTask.Status == TaskStatus.Created)
				{
					hooks.StartTask(_newDayTask, "NewDay");
				}
				if (_newDayTask.Status >= TaskStatus.RanToCompletion)
				{
					if (_newDayTask.IsFaulted)
					{
						Exception baseException = _newDayTask.Exception.GetBaseException();
						Console.WriteLine("_newDayTask failed with an exception");
						Console.WriteLine(baseException);
						throw new Exception("Error on new day: \n---------------\n" + baseException.Message + "\n" + baseException.StackTrace + "\n---------------\n");
					}
					_newDayTask = null;
					Utility.CollectGarbage();
				}
				UpdateChatBox();
				return;
			}
			if (isLocalMultiplayerNewDayActive)
			{
				UpdateChatBox();
				return;
			}
			if (IsSaving)
			{
				PushUIMode();
				activeClickableMenu?.update(gameTime);
				if (overlayMenu != null)
				{
					overlayMenu.update(gameTime);
					if (overlayMenu == null)
					{
						PopUIMode();
						return;
					}
				}
				PopUIMode();
				UpdateChatBox();
				return;
			}
			if (exitToTitle)
			{
				exitToTitle = false;
				CleanupReturningToTitle();
				Utility.CollectGarbage();
				if (postExitToTitleCallback != null)
				{
					postExitToTitleCallback();
				}
			}
			SetFreeCursorElapsed((float)gameTime.ElapsedGameTime.TotalSeconds);
			if (game1.IsMainInstance)
			{
				keyboardFocusInstance = game1;
				foreach (Game1 gameInstance in GameRunner.instance.gameInstances)
				{
					if (gameInstance.instanceKeyboardDispatcher.Subscriber != null && gameInstance.instanceTextEntry != null)
					{
						keyboardFocusInstance = gameInstance;
						break;
					}
				}
			}
			if (HasKeyboardFocus())
			{
				keyboardDispatcher.Poll();
			}
			else
			{
				keyboardDispatcher.Discard();
			}
			if (gameMode == 6)
			{
				multiplayer.UpdateLoading();
			}
			if (gameMode == 3)
			{
				multiplayer.UpdateEarly();
				if (player != null && player.team != null)
				{
					player.team.Update();
				}
			}
			if ((paused || (!IsActiveNoOverlay && Program.releaseBuild)) && (options == null || options.pauseWhenOutOfFocus || paused) && multiplayerMode == 0)
			{
				UpdateChatBox();
				return;
			}
			currentGameTime = gameTime;
			if (gameMode == 11)
			{
				return;
			}
			ticks++;
			if (IsActiveNoOverlay)
			{
				checkForEscapeKeys();
			}
			updateMusic();
			updateRaindropPosition();
			if (globalFade)
			{
				screenFade.UpdateGlobalFade();
			}
			else if (pauseThenDoFunctionTimer > 0)
			{
				freezeControls = true;
				pauseThenDoFunctionTimer -= gameTime.ElapsedGameTime.Milliseconds;
				if (pauseThenDoFunctionTimer <= 0)
				{
					freezeControls = false;
					if (afterPause != null)
					{
						afterPause();
					}
				}
			}
			bool flag3 = false;
			if (options.gamepadControls && activeClickableMenu != null && activeClickableMenu.shouldClampGamePadCursor())
			{
				flag3 = true;
			}
			if (flag3)
			{
				Point mousePositionRaw = getMousePositionRaw();
				Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, localMultiplayerWindow.Width, localMultiplayerWindow.Height);
				if (mousePositionRaw.X < rectangle.X)
				{
					mousePositionRaw.X = rectangle.X;
				}
				else if (mousePositionRaw.X > rectangle.Right)
				{
					mousePositionRaw.X = rectangle.Right;
				}
				if (mousePositionRaw.Y < rectangle.Y)
				{
					mousePositionRaw.Y = rectangle.Y;
				}
				else if (mousePositionRaw.Y > rectangle.Bottom)
				{
					mousePositionRaw.Y = rectangle.Bottom;
				}
				setMousePositionRaw(mousePositionRaw.X, mousePositionRaw.Y);
			}
			if (gameMode == 3 || gameMode == 2)
			{
				if (!warpingForForcedRemoteEvent && !eventUp && remoteEventQueue.Count > 0 && player != null && player.isCustomized.Value && (!fadeIn || !(fadeToBlackAlpha > 0f)))
				{
					if (activeClickableMenu != null)
					{
						activeClickableMenu.emergencyShutDown();
						exitActiveMenu();
					}
					else if (currentMinigame != null && currentMinigame.forceQuit())
					{
						currentMinigame = null;
					}
					if (activeClickableMenu == null && currentMinigame == null && player.freezePause <= 0)
					{
						Action action = remoteEventQueue[0];
						remoteEventQueue.RemoveAt(0);
						action();
					}
				}
				player.millisecondsPlayed += (uint)gameTime.ElapsedGameTime.Milliseconds;
				bool flag4 = true;
				if (currentMinigame != null && !HostPaused)
				{
					if (pauseTime > 0f)
					{
						updatePause(gameTime);
					}
					if (fadeToBlack)
					{
						screenFade.UpdateFadeAlpha(gameTime);
						if (fadeToBlackAlpha >= 1f)
						{
							fadeToBlack = false;
						}
					}
					else
					{
						if (thumbstickMotionMargin > 0)
						{
							thumbstickMotionMargin -= gameTime.ElapsedGameTime.Milliseconds;
						}
						KeyboardState keyboardState = default(KeyboardState);
						MouseState currentMouseState = default(MouseState);
						GamePadState padState = default(GamePadState);
						if (base.IsActive)
						{
							keyboardState = GetKeyboardState();
							currentMouseState = input.GetMouseState();
							padState = input.GetGamePadState();
							_updateTapToMove(gameTime, currentMouseState);
							bool flag5 = false;
							if (chatBox != null && chatBox.isActive())
							{
								flag5 = true;
							}
							else if (textEntry != null)
							{
								flag5 = true;
							}
							if (flag5)
							{
								keyboardState = default(KeyboardState);
								padState = default(GamePadState);
							}
							else
							{
								Keys[] pressedKeys = keyboardState.GetPressedKeys();
								foreach (Keys keys in pressedKeys)
								{
									if (!oldKBState.IsKeyDown(keys) && currentMinigame != null)
									{
										currentMinigame.receiveKeyPress(keys);
									}
								}
								if (options.gamepadControls)
								{
									if (currentMinigame == null)
									{
										oldMouseState = currentMouseState;
										oldKBState = keyboardState;
										oldPadState = padState;
										UpdateChatBox();
										return;
									}
									ButtonCollection.ButtonEnumerator enumerator2 = Utility.getPressedButtons(padState, oldPadState).GetEnumerator();
									while (enumerator2.MoveNext())
									{
										Buttons current2 = enumerator2.Current;
										if (currentMinigame != null)
										{
											currentMinigame.receiveKeyPress(Utility.mapGamePadButtonToKey(current2));
										}
									}
									if (currentMinigame == null)
									{
										oldMouseState = currentMouseState;
										oldKBState = keyboardState;
										oldPadState = padState;
										UpdateChatBox();
										return;
									}
									if (padState.ThumbSticks.Right.Y < -0.2f && oldPadState.ThumbSticks.Right.Y >= -0.2f)
									{
										currentMinigame.receiveKeyPress(Keys.Down);
									}
									if (padState.ThumbSticks.Right.Y > 0.2f && oldPadState.ThumbSticks.Right.Y <= 0.2f)
									{
										currentMinigame.receiveKeyPress(Keys.Up);
									}
									if (padState.ThumbSticks.Right.X < -0.2f && oldPadState.ThumbSticks.Right.X >= -0.2f)
									{
										currentMinigame.receiveKeyPress(Keys.Left);
									}
									if (padState.ThumbSticks.Right.X > 0.2f && oldPadState.ThumbSticks.Right.X <= 0.2f)
									{
										currentMinigame.receiveKeyPress(Keys.Right);
									}
									if (oldPadState.ThumbSticks.Right.Y < -0.2f && padState.ThumbSticks.Right.Y >= -0.2f)
									{
										currentMinigame.receiveKeyRelease(Keys.Down);
									}
									if (oldPadState.ThumbSticks.Right.Y > 0.2f && padState.ThumbSticks.Right.Y <= 0.2f)
									{
										currentMinigame.receiveKeyRelease(Keys.Up);
									}
									if (oldPadState.ThumbSticks.Right.X < -0.2f && padState.ThumbSticks.Right.X >= -0.2f)
									{
										currentMinigame.receiveKeyRelease(Keys.Left);
									}
									if (oldPadState.ThumbSticks.Right.X > 0.2f && padState.ThumbSticks.Right.X <= 0.2f)
									{
										currentMinigame.receiveKeyRelease(Keys.Right);
									}
									if (isGamePadThumbstickInMotion() && currentMinigame != null && !currentMinigame.overrideFreeMouseMovement())
									{
										setMousePosition(getMouseX() + (int)(padState.ThumbSticks.Left.X * thumbstickToMouseModifier), getMouseY() - (int)(padState.ThumbSticks.Left.Y * thumbstickToMouseModifier));
									}
									else if (getMouseX() != getOldMouseX() || getMouseY() != getOldMouseY())
									{
										lastCursorMotionWasMouse = true;
									}
								}
								Keys[] pressedKeys2 = oldKBState.GetPressedKeys();
								foreach (Keys keys2 in pressedKeys2)
								{
									if (!keyboardState.IsKeyDown(keys2) && currentMinigame != null)
									{
										currentMinigame.receiveKeyRelease(keys2);
									}
								}
								if (options.gamepadControls)
								{
									if (currentMinigame == null)
									{
										oldMouseState = currentMouseState;
										oldKBState = keyboardState;
										oldPadState = padState;
										UpdateChatBox();
										return;
									}
									if (padState.IsConnected && padState.IsButtonDown(Buttons.X) && !oldPadState.IsButtonDown(Buttons.X))
									{
										currentMinigame.receiveRightClick(getMouseX(), getMouseY());
									}
									else if (padState.IsConnected && padState.IsButtonDown(Buttons.A) && !oldPadState.IsButtonDown(Buttons.A))
									{
										currentMinigame.receiveLeftClick(getMouseX(), getMouseY());
									}
									else if (padState.IsConnected && !padState.IsButtonDown(Buttons.X) && oldPadState.IsButtonDown(Buttons.X))
									{
										currentMinigame.releaseRightClick(getMouseX(), getMouseY());
									}
									else if (padState.IsConnected && !padState.IsButtonDown(Buttons.A) && oldPadState.IsButtonDown(Buttons.A))
									{
										currentMinigame.releaseLeftClick(getMouseX(), getMouseY());
									}
									ButtonCollection.ButtonEnumerator enumerator3 = Utility.getPressedButtons(oldPadState, padState).GetEnumerator();
									while (enumerator3.MoveNext())
									{
										Buttons current3 = enumerator3.Current;
										if (currentMinigame != null)
										{
											currentMinigame.receiveKeyRelease(Utility.mapGamePadButtonToKey(current3));
										}
									}
									if (padState.IsConnected && padState.IsButtonDown(Buttons.A) && currentMinigame != null)
									{
										currentMinigame.leftClickHeld(0, 0);
									}
								}
								if (currentMinigame == null)
								{
									oldMouseState = currentMouseState;
									oldKBState = keyboardState;
									oldPadState = padState;
									UpdateChatBox();
									return;
								}
								if (currentMinigame != null && currentMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton != ButtonState.Pressed)
								{
									currentMinigame.receiveLeftClick(getMouseX(), getMouseY());
								}
								if (currentMinigame != null && currentMouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton != ButtonState.Pressed)
								{
									currentMinigame.receiveRightClick(getMouseX(), getMouseY());
								}
								if (currentMinigame != null && currentMouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
								{
									currentMinigame.releaseLeftClick(getMouseX(), getMouseY());
								}
								if (currentMinigame != null && currentMouseState.RightButton == ButtonState.Released && oldMouseState.RightButton == ButtonState.Pressed)
								{
									currentMinigame.releaseLeftClick(getMouseX(), getMouseY());
								}
								if (currentMinigame != null && currentMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Pressed)
								{
									currentMinigame.leftClickHeld(getMouseX(), getMouseY());
								}
							}
						}
						if (currentMinigame != null && currentMinigame.tick(gameTime))
						{
							oldMouseState = currentMouseState;
							oldKBState = keyboardState;
							oldPadState = padState;
							if (currentMinigame != null)
							{
								currentMinigame.unload();
							}
							currentMinigame = null;
							fadeIn = true;
							fadeToBlackAlpha = 1f;
							UpdateChatBox();
							return;
						}
						if (currentMinigame == null && IsMusicContextActive(MusicContext.MiniGame))
						{
							stopMusicTrack(MusicContext.MiniGame);
						}
						oldMouseState = currentMouseState;
						oldKBState = keyboardState;
						oldPadState = padState;
					}
					flag4 = IsMultiplayer || currentMinigame == null || currentMinigame.doMainGameUpdates();
				}
				else if (farmEvent != null && !HostPaused && farmEvent.tickUpdate(gameTime))
				{
					farmEvent.makeChangesToLocation();
					timeOfDay = 600;
					UpdateOther(gameTime);
					displayHUD = true;
					farmEvent = null;
					netWorldState.Value.WriteToGame1();
					currentLocation = player.currentLocation;
					if (currentLocation is FarmHouse farmHouse)
					{
						player.Position = Utility.PointToVector2(farmHouse.GetPlayerBedSpot()) * 64f;
						BedFurniture.ShiftPositionForBed(player);
					}
					else
					{
						BedFurniture.ApplyWakeUpPosition(player);
					}
					changeMusicTrack("none");
					currentLocation.resetForPlayerEntry();
					if (player.IsSitting())
					{
						player.StopSitting(animate: false);
					}
					player.forceCanMove();
					freezeControls = false;
					displayFarmer = true;
					outdoorLight = Color.White;
					viewportFreeze = false;
					fadeToBlackAlpha = 0f;
					fadeToBlack = false;
					globalFadeToClear();
					RemoveDeliveredMailForTomorrow();
					handlePostFarmEventActions();
					showEndOfNightStuff();
				}
				if (flag4)
				{
					if (endOfNightMenus.Count > 0 && activeClickableMenu == null)
					{
						activeClickableMenu = endOfNightMenus.Pop();
						if (activeClickableMenu != null && options.SnappyMenus)
						{
							activeClickableMenu.snapToDefaultClickableComponent();
						}
					}
					if (specialCurrencyDisplay != null)
					{
						specialCurrencyDisplay.Update(gameTime);
					}
					if (currentLocation != null && currentMinigame == null)
					{
						if (emoteMenu != null)
						{
							emoteMenu.update(gameTime);
							if (emoteMenu != null)
							{
								PushUIMode();
								emoteMenu.performHoverAction(getMouseX(), getMouseY());
								KeyboardState keyboardState2 = GetKeyboardState();
								if (input.GetMouseState().LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
								{
									emoteMenu.receiveLeftClick(getMouseX(), getMouseY());
								}
								else if (input.GetMouseState().RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released)
								{
									emoteMenu.receiveRightClick(getMouseX(), getMouseY());
								}
								else if (isOneOfTheseKeysDown(keyboardState2, options.menuButton) || (isOneOfTheseKeysDown(keyboardState2, options.emoteButton) && areAllOfTheseKeysUp(oldKBState, options.emoteButton)))
								{
									emoteMenu.exitThisMenu(playSound: false);
								}
								PopUIMode();
								oldKBState = keyboardState2;
								oldMouseState = input.GetMouseState();
							}
						}
						else if (textEntry != null)
						{
							PushUIMode();
							updateTextEntry(gameTime);
							PopUIMode();
						}
						else if (activeClickableMenu != null)
						{
							PushUIMode();
							updateActiveMenu(gameTime);
							PopUIMode();
						}
						else
						{
							if (pauseTime > 0f)
							{
								updatePause(gameTime);
							}
							if (!globalFade && !freezeControls && activeClickableMenu == null && (IsActiveNoOverlay || inputSimulator != null))
							{
								UpdateControlInput(gameTime);
							}
						}
					}
					if (showingEndOfNightStuff && endOfNightMenus.Count == 0 && activeClickableMenu == null)
					{
						if (newDaySync != null)
						{
							newDaySync = null;
						}
						player.team.endOfNightStatus.WithdrawState();
						showingEndOfNightStuff = false;
						Action afterNewDayAction = _afterNewDayAction;
						if (afterNewDayAction != null)
						{
							_afterNewDayAction = null;
							afterNewDayAction();
						}
						player.ReequipEnchantments();
						globalFadeToClear(doMorningStuff);
					}
					if (currentLocation != null)
					{
						if (!HostPaused && !showingEndOfNightStuff)
						{
							if (IsMultiplayer || (activeClickableMenu == null && currentMinigame == null))
							{
								UpdateGameClock(gameTime);
							}
							UpdateCharacters(gameTime);
							UpdateLocations(gameTime);
							if (currentMinigame == null)
							{
								UpdateViewPort(overrideFreeze: false, getViewportCenter());
							}
							else
							{
								previousViewportPosition.X = viewport.X;
								previousViewportPosition.Y = viewport.Y;
							}
							UpdateOther(gameTime);
						}
						if (messagePause)
						{
							KeyboardState keyboardState3 = GetKeyboardState();
							MouseState mouseState = input.GetMouseState();
							GamePadState gamePadState = input.GetGamePadState();
							if (isOneOfTheseKeysDown(keyboardState3, options.actionButton) && !isOneOfTheseKeysDown(oldKBState, options.actionButton))
							{
								pressActionButton(keyboardState3, mouseState, gamePadState);
							}
							oldKBState = keyboardState3;
							oldPadState = gamePadState;
						}
					}
				}
				else if (textEntry != null)
				{
					PushUIMode();
					updateTextEntry(gameTime);
					PopUIMode();
				}
			}
			else
			{
				UpdateTitleScreen(gameTime);
				if (textEntry != null)
				{
					PushUIMode();
					updateTextEntry(gameTime);
					PopUIMode();
				}
				else if (activeClickableMenu != null)
				{
					PushUIMode();
					updateActiveMenu(gameTime);
					PopUIMode();
				}
				if (gameMode == 10)
				{
					UpdateOther(gameTime);
				}
			}
			if (audioEngine != null)
			{
				audioEngine.Update();
			}
			UpdateChatBox();
			_updateTutorialManager(gameTime);
			if (gameMode != 6)
			{
				multiplayer.UpdateLate();
			}
		}

		public static void showTextEntry(TextBox text_box)
		{
			timerUntilMouseFade = 0;
			PushUIMode();
			textEntry = new TextEntryMenu(text_box);
			PopUIMode();
		}

		public static void closeTextEntry()
		{
			if (textEntry != null)
			{
				textEntry = null;
			}
			if (activeClickableMenu != null && options.SnappyMenus)
			{
				if (activeClickableMenu is TitleMenu && TitleMenu.subMenu != null)
				{
					TitleMenu.subMenu.snapCursorToCurrentSnappedComponent();
				}
				else
				{
					activeClickableMenu.snapCursorToCurrentSnappedComponent();
				}
			}
		}

		public static bool isDarkOut()
		{
			return timeOfDay >= getTrulyDarkTime();
		}

		public static bool isStartingToGetDarkOut()
		{
			return timeOfDay >= getStartingToGetDarkTime();
		}

		public static int getStartingToGetDarkTime()
		{
			switch (currentSeason)
			{
			case "spring":
			case "summer":
				return 1800;
			case "fall":
				return 1700;
			case "winter":
				return 1600;
			default:
				return 1800;
			}
		}

		public static void updateCellarAssignments()
		{
			if (!IsMasterGame)
			{
				return;
			}
			player.team.cellarAssignments[1] = MasterPlayer.UniqueMultiplayerID;
			for (int i = 2; i <= netWorldState.Value.HighestPlayerLimit.Value; i++)
			{
				string name = "Cellar" + i;
				if (i == 1 || getLocationFromName(name) == null)
				{
					continue;
				}
				if (player.team.cellarAssignments.ContainsKey(i) && getFarmerMaybeOffline(player.team.cellarAssignments[i]) == null)
				{
					player.team.cellarAssignments.Remove(i);
				}
				if (player.team.cellarAssignments.ContainsKey(i))
				{
					continue;
				}
				foreach (Farmer allFarmer in getAllFarmers())
				{
					if (!player.team.cellarAssignments.Values.Contains(allFarmer.UniqueMultiplayerID))
					{
						player.team.cellarAssignments[i] = allFarmer.UniqueMultiplayerID;
						break;
					}
				}
			}
		}

		public static int getModeratelyDarkTime()
		{
			return (getTrulyDarkTime() + getStartingToGetDarkTime()) / 2;
		}

		public static int getTrulyDarkTime()
		{
			return getStartingToGetDarkTime() + 200;
		}

		public static void playMorningSong()
		{
			if (IsRainingHere() || IsLightningHere() || eventUp || dayOfMonth <= 0 || currentLocation.Name.Equals("Desert"))
			{
				return;
			}
			if (currentLocation.GetLocationContext() == GameLocation.LocationContext.Island)
			{
				if (MasterPlayer.hasOrWillReceiveMail("Island_FirstParrot"))
				{
					morningSongPlayAction = DelayedAction.playMusicAfterDelay("IslandMusic", 500);
				}
			}
			else
			{
				morningSongPlayAction = DelayedAction.playMusicAfterDelay(currentSeason + Math.Max(1, currentSongIndex), 500);
			}
		}

		public static void doMorningStuff()
		{
			playMorningSong();
			DelayedAction.functionAfterDelay(delegate
			{
				while (morningQueue.Count > 0)
				{
					morningQueue.Dequeue()();
				}
			}, 1000);
			if (player.hasPendingCompletedQuests)
			{
				dayTimeMoneyBox.PingQuestLog();
			}
		}

		public static void addMorningFluffFunction(DelayedAction.delayedBehavior func)
		{
			morningQueue.Enqueue(func);
		}

		private Point getViewportCenter()
		{
			if (viewportTarget.X != -2.1474836E+09f)
			{
				if (!(Math.Abs((float)viewportCenter.X - viewportTarget.X) <= viewportSpeed) || !(Math.Abs((float)viewportCenter.Y - viewportTarget.Y) <= viewportSpeed))
				{
					Vector2 velocityTowardPoint = Utility.getVelocityTowardPoint(viewportCenter, viewportTarget, viewportSpeed);
					viewportCenter.X += (int)Math.Round(velocityTowardPoint.X);
					viewportCenter.Y += (int)Math.Round(velocityTowardPoint.Y);
				}
				else
				{
					if (viewportReachedTarget != null)
					{
						viewportReachedTarget();
						viewportReachedTarget = null;
					}
					viewportHold -= currentGameTime.ElapsedGameTime.Milliseconds;
					if (viewportHold <= 0)
					{
						viewportTarget = new Vector2(-2.1474836E+09f, -2.1474836E+09f);
						if (afterViewport != null)
						{
							afterViewport();
						}
					}
				}
				return viewportCenter;
			}
			Farmer playerOrEventFarmer = getPlayerOrEventFarmer();
			viewportCenter.X = playerOrEventFarmer.getStandingX();
			viewportCenter.Y = playerOrEventFarmer.getStandingY();
			return viewportCenter;
		}

		public static void afterFadeReturnViewportToPlayer()
		{
			viewportTarget = new Vector2(-2.1474836E+09f, -2.1474836E+09f);
			viewportHold = 0;
			viewportFreeze = false;
			viewportCenter.X = player.getStandingX();
			viewportCenter.Y = player.getStandingY();
			globalFadeToClear();
		}

		public static bool isViewportOnCustomPath()
		{
			return viewportTarget.X != -2.1474836E+09f;
		}

		public static void moveViewportTo(Vector2 target, float speed, int holdTimer = 0, afterFadeFunction reachedTarget = null, afterFadeFunction endFunction = null)
		{
			viewportTarget = target;
			viewportSpeed = speed;
			viewportHold = holdTimer;
			afterViewport = endFunction;
			viewportReachedTarget = reachedTarget;
		}

		public static Farm getFarm()
		{
			return getLocationFromName("Farm") as Farm;
		}

		public static void setMousePosition(int x, int y, bool ui_scale)
		{
			if (ui_scale)
			{
				setMousePositionRaw((int)((float)x * options.uiScale), (int)((float)y * options.uiScale));
			}
			else
			{
				setMousePositionRaw((int)((float)x * options.zoomLevel), (int)((float)y * options.zoomLevel));
			}
		}

		public static void setMousePosition(int x, int y)
		{
			setMousePosition(x, y, uiMode);
		}

		public static void setMousePosition(Point position, bool ui_scale)
		{
			setMousePosition(position.X, position.Y, ui_scale);
		}

		public static void setMousePosition(Point position)
		{
			setMousePosition(position, uiMode);
		}

		public static void setMousePositionRaw(Point position)
		{
			setMousePositionRaw(position.X, position.Y);
		}

		public static void setMousePositionRaw(int x, int y)
		{
			input.SetMousePosition(x, y);
			InvalidateOldMouseMovement();
			lastCursorMotionWasMouse = false;
		}

		public static Point getMousePositionRaw()
		{
			return new Point(getMouseXRaw(), getMouseYRaw());
		}

		public static Point getMousePosition(bool ui_scale)
		{
			return new Point(getMouseX(ui_scale), getMouseY(ui_scale));
		}

		public static Point getMousePosition()
		{
			return getMousePosition(uiMode);
		}

		private static void ComputeCursorSpeed()
		{
			_cursorSpeedDirty = false;
			GamePadState gamePadState = input.GetGamePadState();
			float num = 0.9f;
			bool flag = false;
			float num2 = gamePadState.ThumbSticks.Left.Length();
			float num3 = gamePadState.ThumbSticks.Right.Length();
			if (num2 > num || num3 > num)
			{
				flag = true;
			}
			float min = 0.7f;
			float max = 2f;
			float num4 = 1f;
			if (_cursorDragEnabled)
			{
				min = 0.5f;
				max = 2f;
				num4 = 1f;
			}
			if (!flag)
			{
				num4 = -5f;
			}
			if (_cursorDragPrevEnabled != _cursorDragEnabled)
			{
				_cursorSpeedScale *= 0.5f;
			}
			_cursorDragPrevEnabled = _cursorDragEnabled;
			_cursorSpeedScale += _cursorUpdateElapsedSec * num4;
			_cursorSpeedScale = MathHelper.Clamp(_cursorSpeedScale, min, max);
			float num5 = 16f / (float)game1.TargetElapsedTime.TotalSeconds * _cursorSpeedScale;
			float num6 = num5 - _cursorSpeed;
			_cursorSpeed = num5;
			_cursorUpdateElapsedSec = 0f;
			if (debugMode)
			{
				Console.WriteLine("_cursorSpeed={0}, _cursorSpeedScale={1}, deltaSpeed={2}", _cursorSpeed.ToString("0.0"), _cursorSpeedScale.ToString("0.0"), num6.ToString("0.0"));
			}
		}

		private static void SetFreeCursorElapsed(float elapsedSec)
		{
			if (elapsedSec != _cursorUpdateElapsedSec)
			{
				_cursorUpdateElapsedSec = elapsedSec;
				_cursorSpeedDirty = true;
			}
		}

		public static void ResetFreeCursorDrag()
		{
			if (_cursorDragEnabled)
			{
				_cursorSpeedDirty = true;
			}
			_cursorDragEnabled = false;
		}

		public static void SetFreeCursorDrag()
		{
			if (!_cursorDragEnabled)
			{
				_cursorSpeedDirty = true;
			}
			_cursorDragEnabled = true;
		}

		public static void updateActiveMenu(GameTime gameTime)
		{
			IClickableMenu childMenu = activeClickableMenu;
			while (childMenu.GetChildMenu() != null)
			{
				childMenu = childMenu.GetChildMenu();
			}
			if (!Program.gamePtr.IsActiveNoOverlay && Program.releaseBuild)
			{
				if (childMenu != null && childMenu.IsActive())
				{
					childMenu.update(gameTime);
				}
				return;
			}
			MouseState mouseState = input.GetMouseState();
			KeyboardState keyboardState = GetKeyboardState();
			GamePadState gamePadState = input.GetGamePadState();
			if (gamePadState.IsConnected)
			{
				TutorialManager.Instance.gamePadHasBeenUsed = true;
			}
			if (CurrentEvent != null)
			{
				if ((mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released) || (options.gamepadControls && gamePadState.IsButtonDown(Buttons.A) && oldPadState.IsButtonUp(Buttons.A)))
				{
					CurrentEvent.receiveMouseClick(getMouseX(), getMouseY());
				}
				else if (options.gamepadControls && gamePadState.IsButtonDown(Buttons.Back) && oldPadState.IsButtonUp(Buttons.Back) && !CurrentEvent.skipped && CurrentEvent.skippable)
				{
					CurrentEvent.skipped = true;
					CurrentEvent.skipEvent();
					freezeControls = false;
				}
				if (CurrentEvent != null && CurrentEvent.skipped)
				{
					oldMouseState = input.GetMouseState();
					oldKBState = keyboardState;
					oldPadState = gamePadState;
					return;
				}
			}
			if (options.gamepadControls && childMenu != null && childMenu.IsActive())
			{
				if (isGamePadThumbstickInMotion() && (!options.snappyMenus || childMenu.overrideSnappyMenuCursorMovementBan()))
				{
					setMousePositionRaw((int)((float)mouseState.X + gamePadState.ThumbSticks.Left.X * thumbstickToMouseModifier), (int)((float)mouseState.Y - gamePadState.ThumbSticks.Left.Y * thumbstickToMouseModifier));
				}
				if (childMenu != null && childMenu.IsActive() && (chatBox == null || !chatBox.isActive()))
				{
					ButtonCollection.ButtonEnumerator enumerator = Utility.getPressedButtons(gamePadState, oldPadState).GetEnumerator();
					while (enumerator.MoveNext())
					{
						Buttons current = enumerator.Current;
						childMenu.receiveGamePadButton(current);
						if (childMenu == null || !childMenu.IsActive())
						{
							break;
						}
					}
					ButtonCollection.ButtonEnumerator enumerator2 = Utility.getHeldButtons(gamePadState).GetEnumerator();
					while (enumerator2.MoveNext())
					{
						Buttons current2 = enumerator2.Current;
						if (childMenu != null && childMenu.IsActive())
						{
							childMenu.gamePadButtonHeld(current2);
						}
						if (childMenu == null || !childMenu.IsActive())
						{
							break;
						}
					}
				}
			}
			if ((getMouseX() != getOldMouseX() || getMouseY() != getOldMouseY()) && !isGamePadThumbstickInMotion() && !isDPadPressed())
			{
				lastCursorMotionWasMouse = true;
			}
			ResetFreeCursorDrag();
			if (childMenu != null && childMenu.IsActive())
			{
				childMenu.performHoverAction(getMouseX(), getMouseY());
			}
			if (childMenu != null && childMenu.IsActive())
			{
				childMenu.update(gameTime);
			}
			if (childMenu != null && childMenu.IsActive() && mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
			{
				if (chatBox != null && chatBox.isActive() && chatBox.isWithinBounds(getMouseX(), getMouseY()))
				{
					chatBox.receiveLeftClick(getMouseX(), getMouseY());
				}
				else
				{
					childMenu.receiveLeftClick(getMouseX(), getMouseY());
				}
				TutorialManager.Instance.receiveLeftClick(getMouseX(), getMouseY());
			}
			else if (childMenu != null && childMenu.IsActive() && mouseState.RightButton == ButtonState.Pressed && (oldMouseState.RightButton == ButtonState.Released || ((float)mouseClickPolling > 650f && !(childMenu is DialogueBox))))
			{
				childMenu.receiveRightClick(getMouseX(), getMouseY());
				if ((float)mouseClickPolling > 650f)
				{
					mouseClickPolling = 600;
				}
				if ((childMenu == null || !childMenu.IsActive()) && activeClickableMenu == null)
				{
					rightClickPolling = 500;
					mouseClickPolling = 0;
				}
			}
			if (mouseState.ScrollWheelValue != oldMouseState.ScrollWheelValue && childMenu != null && childMenu.IsActive())
			{
				if (chatBox != null && chatBox.choosingEmoji && chatBox.emojiMenu.isWithinBounds(getOldMouseX(), getOldMouseY()))
				{
					chatBox.receiveScrollWheelAction(mouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue);
				}
				else
				{
					childMenu.receiveScrollWheelAction(mouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue);
				}
			}
			if (options.gamepadControls && childMenu != null && childMenu.IsActive())
			{
				thumbstickPollingTimer -= currentGameTime.ElapsedGameTime.Milliseconds;
				if (thumbstickPollingTimer <= 0)
				{
					if (gamePadState.ThumbSticks.Right.Y > 0.2f)
					{
						childMenu.receiveScrollWheelAction(1);
					}
					else if (gamePadState.ThumbSticks.Right.Y < -0.2f)
					{
						childMenu.receiveScrollWheelAction(-1);
					}
				}
				if (thumbstickPollingTimer <= 0)
				{
					thumbstickPollingTimer = 220 - (int)(Math.Abs(gamePadState.ThumbSticks.Right.Y) * 170f);
				}
				if (Math.Abs(gamePadState.ThumbSticks.Right.Y) < 0.2f)
				{
					thumbstickPollingTimer = 0;
				}
			}
			if (childMenu != null && childMenu.IsActive() && mouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
			{
				childMenu.releaseLeftClick(getMouseX(), getMouseY());
			}
			else if (childMenu != null && childMenu.IsActive() && mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Pressed)
			{
				childMenu.leftClickHeld(getMouseX(), getMouseY());
			}
			Keys[] pressedKeys = keyboardState.GetPressedKeys();
			foreach (Keys keys in pressedKeys)
			{
				if (childMenu != null && childMenu.IsActive() && !oldKBState.GetPressedKeys().Contains(keys))
				{
					childMenu.receiveKeyPress(keys);
				}
			}
			if (chatBox == null || !chatBox.isActive())
			{
				if (isOneOfTheseKeysDown(oldKBState, options.moveUpButton) || (options.snappyMenus && options.gamepadControls && (Math.Abs(gamePadState.ThumbSticks.Left.X) < gamePadState.ThumbSticks.Left.Y || gamePadState.IsButtonDown(Buttons.DPadUp))))
				{
					directionKeyPolling[0] -= currentGameTime.ElapsedGameTime.Milliseconds;
				}
				else if (isOneOfTheseKeysDown(oldKBState, options.moveRightButton) || (options.snappyMenus && options.gamepadControls && (gamePadState.ThumbSticks.Left.X > Math.Abs(gamePadState.ThumbSticks.Left.Y) || gamePadState.IsButtonDown(Buttons.DPadRight))))
				{
					directionKeyPolling[1] -= currentGameTime.ElapsedGameTime.Milliseconds;
				}
				else if (isOneOfTheseKeysDown(oldKBState, options.moveDownButton) || (options.snappyMenus && options.gamepadControls && (Math.Abs(gamePadState.ThumbSticks.Left.X) < Math.Abs(gamePadState.ThumbSticks.Left.Y) || gamePadState.IsButtonDown(Buttons.DPadDown))))
				{
					directionKeyPolling[2] -= currentGameTime.ElapsedGameTime.Milliseconds;
				}
				else if (isOneOfTheseKeysDown(oldKBState, options.moveLeftButton) || (options.snappyMenus && options.gamepadControls && (Math.Abs(gamePadState.ThumbSticks.Left.X) > Math.Abs(gamePadState.ThumbSticks.Left.Y) || gamePadState.IsButtonDown(Buttons.DPadLeft))))
				{
					directionKeyPolling[3] -= currentGameTime.ElapsedGameTime.Milliseconds;
				}
				if (areAllOfTheseKeysUp(oldKBState, options.moveUpButton) && (!options.snappyMenus || !options.gamepadControls || ((double)gamePadState.ThumbSticks.Left.Y < 0.1 && gamePadState.IsButtonUp(Buttons.DPadUp))))
				{
					directionKeyPolling[0] = 250;
				}
				if (areAllOfTheseKeysUp(oldKBState, options.moveRightButton) && (!options.snappyMenus || !options.gamepadControls || ((double)gamePadState.ThumbSticks.Left.X < 0.1 && gamePadState.IsButtonUp(Buttons.DPadRight))))
				{
					directionKeyPolling[1] = 250;
				}
				if (areAllOfTheseKeysUp(oldKBState, options.moveDownButton) && (!options.snappyMenus || !options.gamepadControls || ((double)gamePadState.ThumbSticks.Left.Y > -0.1 && gamePadState.IsButtonUp(Buttons.DPadDown))))
				{
					directionKeyPolling[2] = 250;
				}
				if (areAllOfTheseKeysUp(oldKBState, options.moveLeftButton) && (!options.snappyMenus || !options.gamepadControls || ((double)gamePadState.ThumbSticks.Left.X > -0.1 && gamePadState.IsButtonUp(Buttons.DPadLeft))))
				{
					directionKeyPolling[3] = 250;
				}
				if (directionKeyPolling[0] <= 0 && childMenu != null && childMenu.IsActive())
				{
					childMenu.receiveKeyPress(options.getFirstKeyboardKeyFromInputButtonList(options.moveUpButton));
					directionKeyPolling[0] = 70;
				}
				if (directionKeyPolling[1] <= 0 && childMenu != null && childMenu.IsActive())
				{
					childMenu.receiveKeyPress(options.getFirstKeyboardKeyFromInputButtonList(options.moveRightButton));
					directionKeyPolling[1] = 70;
				}
				if (directionKeyPolling[2] <= 0 && childMenu != null && childMenu.IsActive())
				{
					childMenu.receiveKeyPress(options.getFirstKeyboardKeyFromInputButtonList(options.moveDownButton));
					directionKeyPolling[2] = 70;
				}
				if (directionKeyPolling[3] <= 0 && childMenu != null && childMenu.IsActive())
				{
					childMenu.receiveKeyPress(options.getFirstKeyboardKeyFromInputButtonList(options.moveLeftButton));
					directionKeyPolling[3] = 70;
				}
				if (options.gamepadControls && childMenu != null && childMenu.IsActive())
				{
					if (!childMenu.areGamePadControlsImplemented() && gamePadState.IsButtonDown(Buttons.A) && !oldPadState.IsButtonDown(Buttons.A))
					{
						childMenu.receiveLeftClick(getMousePosition().X, getMousePosition().Y);
						if ((float)gamePadAButtonPolling > 650f)
						{
							gamePadAButtonPolling = 600;
						}
					}
					else if (!childMenu.areGamePadControlsImplemented() && !gamePadState.IsButtonDown(Buttons.A) && oldPadState.IsButtonDown(Buttons.A))
					{
						childMenu.releaseLeftClick(getMousePosition().X, getMousePosition().Y);
					}
					else if (!childMenu.areGamePadControlsImplemented() && gamePadState.IsButtonDown(Buttons.X) && (!oldPadState.IsButtonDown(Buttons.X) || ((float)gamePadXButtonPolling > 650f && !(childMenu is DialogueBox))))
					{
						childMenu.receiveRightClick(getMousePosition().X, getMousePosition().Y);
						if ((float)gamePadXButtonPolling > 650f)
						{
							gamePadXButtonPolling = 600;
						}
					}
					ButtonCollection.ButtonEnumerator enumerator3 = Utility.getPressedButtons(gamePadState, oldPadState).GetEnumerator();
					while (enumerator3.MoveNext())
					{
						Buttons current3 = enumerator3.Current;
						if (childMenu == null || !childMenu.IsActive())
						{
							break;
						}
						Keys key = Utility.mapGamePadButtonToKey(current3);
						childMenu.receiveKeyPress(key);
					}
					if (childMenu != null && childMenu.IsActive() && !childMenu.areGamePadControlsImplemented() && gamePadState.IsButtonDown(Buttons.A) && oldPadState.IsButtonDown(Buttons.A))
					{
						childMenu.leftClickHeld(getMousePosition().X, getMousePosition().Y);
					}
					if (gamePadState.IsButtonDown(Buttons.X))
					{
						gamePadXButtonPolling += gameTime.ElapsedGameTime.Milliseconds;
					}
					else
					{
						gamePadXButtonPolling = 0;
					}
					if (gamePadState.IsButtonDown(Buttons.A))
					{
						gamePadAButtonPolling += gameTime.ElapsedGameTime.Milliseconds;
					}
					else
					{
						gamePadAButtonPolling = 0;
					}
					if (!childMenu.IsActive() && activeClickableMenu == null)
					{
						rightClickPolling = 500;
						gamePadXButtonPolling = 0;
						gamePadAButtonPolling = 0;
					}
				}
			}
			else
			{
				_ = options.SnappyMenus;
			}
			if (mouseState.RightButton == ButtonState.Pressed)
			{
				mouseClickPolling += gameTime.ElapsedGameTime.Milliseconds;
			}
			else
			{
				mouseClickPolling = 0;
			}
			oldMouseState = input.GetMouseState();
			oldKBState = keyboardState;
			oldPadState = gamePadState;
		}

		public static void AdjustScreenScale(float offset)
		{
		}

		public void ShowScreenScaleMenu()
		{
			if (activeClickableMenu != null && !(activeClickableMenu is ScreenSizeAdjustMenu))
			{
				activeClickableMenu.SetChildMenu(new ScreenSizeAdjustMenu());
			}
			else if (activeClickableMenu == null)
			{
				activeClickableMenu = new ScreenSizeAdjustMenu();
			}
		}

		public bool ShowLocalCoopJoinMenu()
		{
			if (!base.IsMainInstance)
			{
				return false;
			}
			if (gameMode != 3)
			{
				return false;
			}
			Farm farm = getFarm();
			if (farm == null)
			{
				return false;
			}
			int num = 0;
			foreach (Building building in farm.buildings)
			{
				if (building.indoors.Value is Cabin)
				{
					Farmer value = (building.indoors.Value as Cabin).farmhand.Value;
					if (value == null)
					{
						num++;
					}
					else if (!value.isActive())
					{
						num++;
					}
				}
			}
			if (num == 0)
			{
				showRedMessage(content.LoadString("Strings\\UI:CoopMenu_NoSlots"));
				return false;
			}
			if (currentMinigame != null)
			{
				return false;
			}
			if (activeClickableMenu != null)
			{
				return false;
			}
			if (!IsLocalCoopJoinable())
			{
				return false;
			}
			playSound("bigSelect");
			activeClickableMenu = new LocalCoopJoinMenu();
			return true;
		}

		public static void updateTextEntry(GameTime gameTime)
		{
			MouseState mouseState = input.GetMouseState();
			KeyboardState keyboardState = GetKeyboardState();
			GamePadState gamePadState = input.GetGamePadState();
			if (options.gamepadControls && textEntry != null && textEntry != null)
			{
				ButtonCollection.ButtonEnumerator enumerator = Utility.getPressedButtons(gamePadState, oldPadState).GetEnumerator();
				while (enumerator.MoveNext())
				{
					Buttons current = enumerator.Current;
					textEntry.receiveGamePadButton(current);
					if (textEntry == null)
					{
						break;
					}
				}
				ButtonCollection.ButtonEnumerator enumerator2 = Utility.getHeldButtons(gamePadState).GetEnumerator();
				while (enumerator2.MoveNext())
				{
					Buttons current2 = enumerator2.Current;
					if (textEntry != null)
					{
						textEntry.gamePadButtonHeld(current2);
					}
					if (textEntry == null)
					{
						break;
					}
				}
			}
			if (textEntry != null)
			{
				textEntry.performHoverAction(getMouseX(), getMouseY());
			}
			if (textEntry != null)
			{
				textEntry.update(gameTime);
			}
			if (textEntry != null && mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
			{
				textEntry.receiveLeftClick(getMouseX(), getMouseY());
			}
			else if (textEntry != null && mouseState.RightButton == ButtonState.Pressed && (oldMouseState.RightButton == ButtonState.Released || (float)mouseClickPolling > 650f))
			{
				textEntry.receiveRightClick(getMouseX(), getMouseY());
				if ((float)mouseClickPolling > 650f)
				{
					mouseClickPolling = 600;
				}
				if (textEntry == null)
				{
					rightClickPolling = 500;
					mouseClickPolling = 0;
				}
			}
			if (mouseState.ScrollWheelValue != oldMouseState.ScrollWheelValue && textEntry != null)
			{
				if (chatBox != null && chatBox.choosingEmoji && chatBox.emojiMenu.isWithinBounds(getOldMouseX(), getOldMouseY()))
				{
					chatBox.receiveScrollWheelAction(mouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue);
				}
				else
				{
					textEntry.receiveScrollWheelAction(mouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue);
				}
			}
			if (options.gamepadControls && textEntry != null)
			{
				thumbstickPollingTimer -= currentGameTime.ElapsedGameTime.Milliseconds;
				if (thumbstickPollingTimer <= 0)
				{
					if (gamePadState.ThumbSticks.Right.Y > 0.2f)
					{
						textEntry.receiveScrollWheelAction(1);
					}
					else if (gamePadState.ThumbSticks.Right.Y < -0.2f)
					{
						textEntry.receiveScrollWheelAction(-1);
					}
				}
				if (thumbstickPollingTimer <= 0)
				{
					thumbstickPollingTimer = 220 - (int)(Math.Abs(gamePadState.ThumbSticks.Right.Y) * 170f);
				}
				if (Math.Abs(gamePadState.ThumbSticks.Right.Y) < 0.2f)
				{
					thumbstickPollingTimer = 0;
				}
			}
			if (textEntry != null && mouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
			{
				textEntry.releaseLeftClick(getMouseX(), getMouseY());
			}
			else if (textEntry != null && mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Pressed)
			{
				textEntry.leftClickHeld(getMouseX(), getMouseY());
			}
			Keys[] pressedKeys = keyboardState.GetPressedKeys();
			foreach (Keys keys in pressedKeys)
			{
				if (textEntry != null && !oldKBState.GetPressedKeys().Contains(keys))
				{
					textEntry.receiveKeyPress(keys);
				}
			}
			if (isOneOfTheseKeysDown(oldKBState, options.moveUpButton) || (options.snappyMenus && options.gamepadControls && (Math.Abs(gamePadState.ThumbSticks.Left.X) < gamePadState.ThumbSticks.Left.Y || gamePadState.IsButtonDown(Buttons.DPadUp))))
			{
				directionKeyPolling[0] -= currentGameTime.ElapsedGameTime.Milliseconds;
			}
			else if (isOneOfTheseKeysDown(oldKBState, options.moveRightButton) || (options.snappyMenus && options.gamepadControls && (gamePadState.ThumbSticks.Left.X > Math.Abs(gamePadState.ThumbSticks.Left.Y) || gamePadState.IsButtonDown(Buttons.DPadRight))))
			{
				directionKeyPolling[1] -= currentGameTime.ElapsedGameTime.Milliseconds;
			}
			else if (isOneOfTheseKeysDown(oldKBState, options.moveDownButton) || (options.snappyMenus && options.gamepadControls && (Math.Abs(gamePadState.ThumbSticks.Left.X) < Math.Abs(gamePadState.ThumbSticks.Left.Y) || gamePadState.IsButtonDown(Buttons.DPadDown))))
			{
				directionKeyPolling[2] -= currentGameTime.ElapsedGameTime.Milliseconds;
			}
			else if (isOneOfTheseKeysDown(oldKBState, options.moveLeftButton) || (options.snappyMenus && options.gamepadControls && (Math.Abs(gamePadState.ThumbSticks.Left.X) > Math.Abs(gamePadState.ThumbSticks.Left.Y) || gamePadState.IsButtonDown(Buttons.DPadLeft))))
			{
				directionKeyPolling[3] -= currentGameTime.ElapsedGameTime.Milliseconds;
			}
			if (areAllOfTheseKeysUp(oldKBState, options.moveUpButton) && (!options.snappyMenus || !options.gamepadControls || ((double)gamePadState.ThumbSticks.Left.Y < 0.1 && gamePadState.IsButtonUp(Buttons.DPadUp))))
			{
				directionKeyPolling[0] = 250;
			}
			if (areAllOfTheseKeysUp(oldKBState, options.moveRightButton) && (!options.snappyMenus || !options.gamepadControls || ((double)gamePadState.ThumbSticks.Left.X < 0.1 && gamePadState.IsButtonUp(Buttons.DPadRight))))
			{
				directionKeyPolling[1] = 250;
			}
			if (areAllOfTheseKeysUp(oldKBState, options.moveDownButton) && (!options.snappyMenus || !options.gamepadControls || ((double)gamePadState.ThumbSticks.Left.Y > -0.1 && gamePadState.IsButtonUp(Buttons.DPadDown))))
			{
				directionKeyPolling[2] = 250;
			}
			if (areAllOfTheseKeysUp(oldKBState, options.moveLeftButton) && (!options.snappyMenus || !options.gamepadControls || ((double)gamePadState.ThumbSticks.Left.X > -0.1 && gamePadState.IsButtonUp(Buttons.DPadLeft))))
			{
				directionKeyPolling[3] = 250;
			}
			if (directionKeyPolling[0] <= 0 && textEntry != null)
			{
				textEntry.receiveKeyPress(options.getFirstKeyboardKeyFromInputButtonList(options.moveUpButton));
				directionKeyPolling[0] = 70;
			}
			if (directionKeyPolling[1] <= 0 && textEntry != null)
			{
				textEntry.receiveKeyPress(options.getFirstKeyboardKeyFromInputButtonList(options.moveRightButton));
				directionKeyPolling[1] = 70;
			}
			if (directionKeyPolling[2] <= 0 && textEntry != null)
			{
				textEntry.receiveKeyPress(options.getFirstKeyboardKeyFromInputButtonList(options.moveDownButton));
				directionKeyPolling[2] = 70;
			}
			if (directionKeyPolling[3] <= 0 && textEntry != null)
			{
				textEntry.receiveKeyPress(options.getFirstKeyboardKeyFromInputButtonList(options.moveLeftButton));
				directionKeyPolling[3] = 70;
			}
			if (options.gamepadControls && textEntry != null)
			{
				if (!textEntry.areGamePadControlsImplemented() && gamePadState.IsButtonDown(Buttons.A) && (!oldPadState.IsButtonDown(Buttons.A) || (float)gamePadAButtonPolling > 650f))
				{
					textEntry.receiveLeftClick(getMousePosition().X, getMousePosition().Y);
					if ((float)gamePadAButtonPolling > 650f)
					{
						gamePadAButtonPolling = 600;
					}
				}
				else if (!textEntry.areGamePadControlsImplemented() && !gamePadState.IsButtonDown(Buttons.A) && oldPadState.IsButtonDown(Buttons.A))
				{
					textEntry.releaseLeftClick(getMousePosition().X, getMousePosition().Y);
				}
				else if (!textEntry.areGamePadControlsImplemented() && gamePadState.IsButtonDown(Buttons.X) && (!oldPadState.IsButtonDown(Buttons.X) || (float)gamePadXButtonPolling > 650f))
				{
					textEntry.receiveRightClick(getMousePosition().X, getMousePosition().Y);
					if ((float)gamePadXButtonPolling > 650f)
					{
						gamePadXButtonPolling = 600;
					}
				}
				ButtonCollection.ButtonEnumerator enumerator3 = Utility.getPressedButtons(gamePadState, oldPadState).GetEnumerator();
				while (enumerator3.MoveNext())
				{
					Buttons current3 = enumerator3.Current;
					if (textEntry == null)
					{
						break;
					}
					textEntry.receiveKeyPress(Utility.mapGamePadButtonToKey(current3));
				}
				if (textEntry != null && !textEntry.areGamePadControlsImplemented() && gamePadState.IsButtonDown(Buttons.A) && oldPadState.IsButtonDown(Buttons.A))
				{
					textEntry.leftClickHeld(getMousePosition().X, getMousePosition().Y);
				}
				if (gamePadState.IsButtonDown(Buttons.X))
				{
					gamePadXButtonPolling += gameTime.ElapsedGameTime.Milliseconds;
				}
				else
				{
					gamePadXButtonPolling = 0;
				}
				if (gamePadState.IsButtonDown(Buttons.A))
				{
					gamePadAButtonPolling += gameTime.ElapsedGameTime.Milliseconds;
				}
				else
				{
					gamePadAButtonPolling = 0;
				}
				if (textEntry == null)
				{
					rightClickPolling = 500;
					gamePadAButtonPolling = 0;
					gamePadXButtonPolling = 0;
				}
			}
			if (mouseState.RightButton == ButtonState.Pressed)
			{
				mouseClickPolling += gameTime.ElapsedGameTime.Milliseconds;
			}
			else
			{
				mouseClickPolling = 0;
			}
			oldMouseState = input.GetMouseState();
			oldKBState = keyboardState;
			oldPadState = gamePadState;
		}

		public static string DateCompiled()
		{
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			return version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision;
		}

		public static void updatePause(GameTime gameTime)
		{
			pauseTime -= gameTime.ElapsedGameTime.Milliseconds;
			if (player.isCrafting && random.NextDouble() < 0.007)
			{
				playSound("crafting");
			}
			if (!(pauseTime <= 0f))
			{
				return;
			}
			if (currentObjectDialogue.Count == 0)
			{
				messagePause = false;
			}
			pauseTime = 0f;
			if (messageAfterPause != null && !messageAfterPause.Equals(""))
			{
				player.isCrafting = false;
				drawObjectDialogue(messageAfterPause);
				messageAfterPause = "";
				if (player.ActiveObject != null)
				{
					_ = (bool)player.ActiveObject.bigCraftable;
				}
				if (killScreen)
				{
					killScreen = false;
					player.health = 10;
				}
			}
			else if (killScreen)
			{
				multiplayer.globalChatInfoMessage("PlayerDeath", player.Name);
				screenGlow = false;
				if (currentLocation.Name.StartsWith("UndergroundMine") && mine.getMineArea() != 121)
				{
					warpFarmer("Mine", 22, 9, flip: false);
				}
				else if (currentLocation is IslandLocation)
				{
					warpFarmer("IslandSouth", 13, 33, flip: false);
				}
				else
				{
					warpFarmer("Hospital", 20, 12, flip: false);
				}
			}
			progressBar = false;
			if (currentLocation.currentEvent != null)
			{
				currentLocation.currentEvent.CurrentCommand++;
			}
		}

		public static void toggleNonBorderlessWindowedFullscreen()
		{
			int preferredResolutionX = options.preferredResolutionX;
			int preferredResolutionY = options.preferredResolutionY;
			GameRunner.instance.OnWindowSizeChange(null, null);
		}

		public static void toggleFullscreen()
		{
			if (options.windowedBorderlessFullscreen)
			{
				graphics.HardwareModeSwitch = false;
				graphics.IsFullScreen = true;
				graphics.ApplyChanges();
				graphics.PreferredBackBufferWidth = Program.gamePtr.Window.ClientBounds.Width;
				graphics.PreferredBackBufferHeight = Program.gamePtr.Window.ClientBounds.Height;
			}
			else
			{
				toggleNonBorderlessWindowedFullscreen();
			}
			GameRunner.instance.OnWindowSizeChange(null, null);
		}

		private void checkForEscapeKeys()
		{
			KeyboardState keyboardState = input.GetKeyboardState();
			if (!base.IsMainInstance)
			{
				return;
			}
			if (keyboardState.IsKeyDown(Keys.LeftAlt) && keyboardState.IsKeyDown(Keys.Enter) && (oldKBState.IsKeyUp(Keys.LeftAlt) || oldKBState.IsKeyUp(Keys.Enter)))
			{
				if (options.isCurrentlyFullscreen() || options.isCurrentlyWindowedBorderless())
				{
					options.setWindowedOption(1);
				}
				else
				{
					options.setWindowedOption(0);
				}
			}
			if ((player.UsingTool || freezeControls) && keyboardState.IsKeyDown(Keys.RightShift) && keyboardState.IsKeyDown(Keys.R) && keyboardState.IsKeyDown(Keys.Delete))
			{
				freezeControls = false;
				player.forceCanMove();
				player.completelyStopAnimatingOrDoingAction();
				player.UsingTool = false;
			}
		}

		public static bool IsPressEvent(ref KeyboardState state, Keys key)
		{
			if (state.IsKeyDown(key) && !oldKBState.IsKeyDown(key))
			{
				oldKBState = state;
				return true;
			}
			return false;
		}

		public static bool IsPressEvent(ref GamePadState state, Buttons btn)
		{
			if (state.IsConnected && state.IsButtonDown(btn) && !oldPadState.IsButtonDown(btn))
			{
				oldPadState = state;
				return true;
			}
			return false;
		}

		public static bool isOneOfTheseKeysDown(KeyboardState state, InputButton[] keys)
		{
			for (int i = 0; i < keys.Length; i++)
			{
				InputButton inputButton = keys[i];
				if (inputButton.key != 0 && state.IsKeyDown(inputButton.key))
				{
					return true;
				}
			}
			return false;
		}

		public static bool areAllOfTheseKeysUp(KeyboardState state, InputButton[] keys)
		{
			for (int i = 0; i < keys.Length; i++)
			{
				InputButton inputButton = keys[i];
				if (inputButton.key != 0 && !state.IsKeyUp(inputButton.key))
				{
					return false;
				}
			}
			return true;
		}

		private void UpdateTitleScreen(GameTime time)
		{
			if (gameMode == 6)
			{
				_requestedMusicTracks = new Dictionary<MusicContext, KeyValuePair<string, bool>>(MusicContextComparer.Default);
				requestedMusicTrack = "none";
				requestedMusicTrackOverrideable = false;
				requestedMusicDirty = true;
				if (currentLoader != null && !currentLoader.MoveNext())
				{
					if (gameMode == 3)
					{
						setGameMode(3);
						fadeIn = true;
						fadeToBlackAlpha = 0.99f;
					}
					else
					{
						ExitToTitle();
					}
				}
				return;
			}
			if (gameMode == 7)
			{
				currentLoader.MoveNext();
				return;
			}
			if (gameMode == 8)
			{
				pauseAccumulator -= time.ElapsedGameTime.Milliseconds;
				if (pauseAccumulator <= 0f)
				{
					pauseAccumulator = 0f;
					setGameMode(3);
					if (currentObjectDialogue.Count > 0)
					{
						messagePause = true;
						pauseTime = 1E+10f;
						fadeToBlackAlpha = 1f;
						player.CanMove = false;
					}
				}
				return;
			}
			_ = game1.instanceIndex;
			_ = 0;
			if (fadeToBlackAlpha < 1f && fadeIn)
			{
				fadeToBlackAlpha += 0.02f;
			}
			else if (fadeToBlackAlpha > 0f && fadeToBlack)
			{
				fadeToBlackAlpha -= 0.02f;
			}
			if (pauseTime > 0f)
			{
				pauseTime = Math.Max(0f, pauseTime - (float)time.ElapsedGameTime.Milliseconds);
			}
			if (gameMode == 0 && (double)fadeToBlackAlpha >= 0.98)
			{
				_ = fadeToBlackAlpha;
				_ = 1f;
			}
			if (fadeToBlackAlpha >= 1f)
			{
				if (gameMode == 4 && !fadeToBlack)
				{
					fadeIn = false;
					fadeToBlack = true;
					fadeToBlackAlpha = 2.5f;
				}
				else if (gameMode == 0 && currentSong == null && soundBank != null && pauseTime <= 0f && base.IsMainInstance && soundBank != null)
				{
					currentSong = soundBank.GetCue("spring_day_ambient");
					currentSong.Play();
				}
				if (gameMode == 0 && activeClickableMenu == null && !quit)
				{
					activeClickableMenu = new TitleMenu();
				}
			}
			else if (fadeToBlackAlpha <= 0f)
			{
				if (gameMode == 4 && fadeToBlack)
				{
					fadeIn = true;
					fadeToBlack = false;
					setGameMode(0);
					pauseTime = 2000f;
				}
				else if (gameMode == 0 && fadeToBlack && menuChoice == 0)
				{
					currentLoader = Utility.generateNewFarm(IsClient);
					setGameMode(6);
					loadingMessage = (IsClient ? content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2574", client.serverName) : content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2575"));
					exitActiveMenu();
				}
			}
		}

		private void UpdateLocations(GameTime time)
		{
			locationCues.Update(currentLocation);
			if (menuUp && !IsMultiplayer)
			{
				return;
			}
			if (IsClient)
			{
				currentLocation.UpdateWhenCurrentLocation(time);
				{
					foreach (GameLocation item in multiplayer.activeLocations())
					{
						item.updateEvenIfFarmerIsntHere(time);
						if (!(item is BuildableGameLocation))
						{
							continue;
						}
						foreach (Building building in (item as BuildableGameLocation).buildings)
						{
							if (building.indoors.Value != null && building.indoors.Value != currentLocation)
							{
								building.indoors.Value.updateEvenIfFarmerIsntHere(time);
							}
						}
					}
					return;
				}
			}
			foreach (GameLocation location in locations)
			{
				bool flag = location.farmers.Any();
				if (!flag && location.CanBeRemotedlyViewed())
				{
					if (player.currentLocation == location)
					{
						flag = true;
					}
					else
					{
						foreach (Farmer value2 in otherFarmers.Values)
						{
							if (value2.viewingLocation.Value != null && value2.viewingLocation.Value.Equals(location.Name))
							{
								flag = true;
								break;
							}
						}
					}
				}
				if (flag)
				{
					location.UpdateWhenCurrentLocation(time);
				}
				location.updateEvenIfFarmerIsntHere(time);
				if (location.wasInhabited != flag)
				{
					location.wasInhabited = flag;
					if (IsMasterGame)
					{
						location.cleanupForVacancy();
					}
				}
				if (!(location is BuildableGameLocation))
				{
					continue;
				}
				foreach (Building building2 in (location as BuildableGameLocation).buildings)
				{
					GameLocation value = building2.indoors.Value;
					if (value != null)
					{
						if (value.farmers.Any())
						{
							value.UpdateWhenCurrentLocation(time);
						}
						value.updateEvenIfFarmerIsntHere(time);
					}
				}
			}
			if (currentLocation.isTemp())
			{
				currentLocation.UpdateWhenCurrentLocation(time);
				currentLocation.updateEvenIfFarmerIsntHere(time);
			}
			MineShaft.UpdateMines(time);
			VolcanoDungeon.UpdateLevels(time);
		}

		public static void performTenMinuteClockUpdate()
		{
			hooks.OnGame1_PerformTenMinuteClockUpdate(delegate
			{
				int trulyDarkTime = getTrulyDarkTime();
				gameTimeInterval = 0;
				if (IsMasterGame)
				{
					timeOfDay += 10;
				}
				if (timeOfDay % 100 >= 60)
				{
					timeOfDay = timeOfDay - timeOfDay % 100 + 100;
				}
				timeOfDay = Math.Min(timeOfDay, 2600);
				if (isLightning && timeOfDay < 2400 && IsMasterGame)
				{
					Utility.performLightningUpdate(timeOfDay);
				}
				if (timeOfDay == trulyDarkTime)
				{
					currentLocation.switchOutNightTiles();
				}
				else if (timeOfDay == getModeratelyDarkTime())
				{
					if (currentLocation.IsOutdoors && !IsRainingHere())
					{
						ambientLight = Color.White;
					}
					if (!IsRainingHere() && !(currentLocation is MineShaft) && currentSong != null && !currentSong.Name.Contains("ambient") && currentLocation is Town)
					{
						changeMusicTrack("none");
					}
				}
				if (getMusicTrackName().StartsWith(currentSeason) && !getMusicTrackName().Contains("ambient") && !eventUp && isDarkOut())
				{
					changeMusicTrack("none", track_interruptable: true);
				}
				if ((bool)currentLocation.isOutdoors && !IsRainingHere() && !eventUp && getMusicTrackName().Contains("day") && isDarkOut())
				{
					changeMusicTrack("none", track_interruptable: true);
				}
				if (weatherIcon == 1)
				{
					int num = Convert.ToInt32(temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + currentSeason + dayOfMonth)["conditions"].Split('/')[1].Split(' ')[0]);
					if (whereIsTodaysFest == null)
					{
						whereIsTodaysFest = temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + currentSeason + dayOfMonth)["conditions"].Split('/')[0];
					}
					if (timeOfDay == num)
					{
						Dictionary<string, string> dictionary = temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + currentSeason + dayOfMonth);
						string text = dictionary["conditions"].Split('/')[0];
						if (dictionary.ContainsKey("locationDisplayName"))
						{
							text = dictionary["locationDisplayName"];
						}
						else
						{
							switch (text)
							{
							case "Forest":
								text = (currentSeason.Equals("winter") ? content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2634") : content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2635"));
								break;
							case "Town":
								text = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2637");
								break;
							case "Beach":
								text = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2639");
								break;
							}
						}
						showGlobalMessage(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2640", temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + currentSeason + dayOfMonth)["name"]) + text);
					}
				}
				player.performTenMinuteUpdate();
				switch (timeOfDay)
				{
				case 1200:
					if ((bool)currentLocation.isOutdoors && !IsRainingHere() && (currentSong == null || currentSong.IsStopped || currentSong.Name.ToLower().Contains("ambient")))
					{
						playMorningSong();
					}
					break;
				case 2000:
					if (!IsRainingHere() && currentLocation is Town)
					{
						changeMusicTrack("none");
					}
					break;
				case 2400:
					dayTimeMoneyBox.timeShakeTimer = 2000;
					player.doEmote(24);
					showGlobalMessage(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2652"));
					break;
				case 2500:
					dayTimeMoneyBox.timeShakeTimer = 2000;
					player.doEmote(24);
					break;
				case 2600:
					dayTimeMoneyBox.timeShakeTimer = 2000;
					if (player.mount != null)
					{
						player.mount.dismount();
					}
					if (player.IsSitting())
					{
						player.StopSitting(animate: false);
					}
					if (player.UsingTool && (player.CurrentTool == null || !(player.CurrentTool is FishingRod fishingRod) || (!fishingRod.isReeling && !fishingRod.pullingOutOfWater)))
					{
						player.completelyStopAnimatingOrDoingAction();
					}
					break;
				case 2800:
					if (activeClickableMenu != null)
					{
						activeClickableMenu.emergencyShutDown();
						exitActiveMenu();
					}
					player.startToPassOut();
					if (player.mount != null)
					{
						player.mount.dismount();
					}
					break;
				}
				foreach (GameLocation location in locations)
				{
					GameLocation gameLocation = location;
					if (gameLocation.NameOrUniqueName == currentLocation.NameOrUniqueName)
					{
						gameLocation = currentLocation;
					}
					gameLocation.performTenMinuteUpdate(timeOfDay);
					if (gameLocation is Farm)
					{
						((Farm)gameLocation).timeUpdate(10);
					}
				}
				MineShaft.UpdateMines10Minutes(timeOfDay);
				VolcanoDungeon.UpdateLevels10Minutes(timeOfDay);
				if (IsMasterGame && farmEvent == null)
				{
					netWorldState.Value.UpdateFromGame1();
				}
			});
		}

		public static bool shouldPlayMorningSong(bool loading_game = false)
		{
			if (eventUp)
			{
				return false;
			}
			if ((double)options.musicVolumeLevel <= 0.025)
			{
				return false;
			}
			if (timeOfDay >= 1200)
			{
				return false;
			}
			if (!loading_game && currentSong != null && !requestedMusicTrack.ToLower().Contains("ambient"))
			{
				return false;
			}
			return true;
		}

		public static void UpdateGameClock(GameTime time)
		{
			if (shouldTimePass() && !IsClient)
			{
				gameTimeInterval += time.ElapsedGameTime.Milliseconds;
			}
			if (timeOfDay >= getTrulyDarkTime())
			{
				int num = (int)((float)(timeOfDay - timeOfDay % 100) + (float)(timeOfDay % 100 / 10) * 16.66f);
				float num2 = Math.Min(0.93f, 0.75f + ((float)(num - getTrulyDarkTime()) + (float)gameTimeInterval / 7000f * 16.6f) * 0.000625f);
				outdoorLight = (IsRainingHere() ? ambientLight : eveningColor) * num2;
			}
			else if (timeOfDay >= getStartingToGetDarkTime())
			{
				int num3 = (int)((float)(timeOfDay - timeOfDay % 100) + (float)(timeOfDay % 100 / 10) * 16.66f);
				float num4 = Math.Min(0.93f, 0.3f + ((float)(num3 - getStartingToGetDarkTime()) + (float)gameTimeInterval / 7000f * 16.6f) * 0.00225f);
				outdoorLight = (IsRainingHere() ? ambientLight : eveningColor) * num4;
			}
			else if (IsRainingHere())
			{
				outdoorLight = ambientLight * 0.3f;
			}
			if (currentLocation != null && gameTimeInterval > 7000 + currentLocation.getExtraMillisecondsPerInGameMinuteForThisLocation())
			{
				if (panMode)
				{
					gameTimeInterval = 0;
				}
				else
				{
					performTenMinuteClockUpdate();
				}
			}
		}

		public static Event getAvailableWeddingEvent()
		{
			if (weddingsToday.Count > 0)
			{
				long id = weddingsToday[0];
				weddingsToday.RemoveAt(0);
				Farmer farmerMaybeOffline = getFarmerMaybeOffline(id);
				Farmer farmer = farmerMaybeOffline;
				if (farmerMaybeOffline == null)
				{
					return null;
				}
				if (farmerMaybeOffline.hasRoommate())
				{
					return null;
				}
				if (IsMultiplayer)
				{
					farmer = (farmerMaybeOffline.NetFields.Root as NetRoot<Farmer>).Clone().Value;
				}
				Event @event = null;
				if (farmerMaybeOffline.spouse != null)
				{
					return Utility.getWeddingEvent(farmerMaybeOffline);
				}
				long? spouse = farmerMaybeOffline.team.GetSpouse(farmerMaybeOffline.UniqueMultiplayerID);
				Farmer farmerMaybeOffline2 = getFarmerMaybeOffline(spouse.Value);
				if (farmerMaybeOffline2 == null)
				{
					return null;
				}
				if (!getOnlineFarmers().Contains(farmerMaybeOffline) || !getOnlineFarmers().Contains(farmerMaybeOffline2))
				{
					return null;
				}
				player.team.GetFriendship(farmerMaybeOffline.UniqueMultiplayerID, spouse.Value).Status = FriendshipStatus.Married;
				player.team.GetFriendship(farmerMaybeOffline.UniqueMultiplayerID, spouse.Value).WeddingDate = new WorldDate(Date);
				return Utility.getPlayerWeddingEvent(farmerMaybeOffline, farmerMaybeOffline2);
			}
			return null;
		}

		public static void checkForNewLevelPerks()
		{
			Dictionary<string, string> dictionary = content.Load<Dictionary<string, string>>("Data\\CookingRecipes");
			int level = player.Level;
			foreach (string key in dictionary.Keys)
			{
				string[] array = dictionary[key].Split('/')[3].Split(' ');
				if (array[0].Equals("l") && Convert.ToInt32(array[1]) <= level && !player.cookingRecipes.ContainsKey(key))
				{
					player.cookingRecipes.Add(key, 0);
					currentObjectDialogue.Enqueue(parseText(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2666") + key));
					currentDialogueCharacterIndex = 1;
					dialogueUp = true;
					dialogueTyping = true;
				}
				else
				{
					if (!array[0].Equals("s"))
					{
						continue;
					}
					int num = Convert.ToInt32(array[2]);
					bool flag = false;
					switch (array[1])
					{
					case "Farming":
						if (player.FarmingLevel >= num)
						{
							flag = true;
						}
						break;
					case "Fishing":
						if (player.FishingLevel >= num)
						{
							flag = true;
						}
						break;
					case "Mining":
						if (player.MiningLevel >= num)
						{
							flag = true;
						}
						break;
					case "Combat":
						if (player.CombatLevel >= num)
						{
							flag = true;
						}
						break;
					case "Foraging":
						if (player.ForagingLevel >= num)
						{
							flag = true;
						}
						break;
					case "Luck":
						if (player.LuckLevel >= num)
						{
							flag = true;
						}
						break;
					}
					if (flag && !player.cookingRecipes.ContainsKey(key))
					{
						player.cookingRecipes.Add(key, 0);
						currentObjectDialogue.Enqueue(parseText(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2666") + key));
						currentDialogueCharacterIndex = 1;
						dialogueUp = true;
						dialogueTyping = true;
					}
				}
			}
			Dictionary<string, string> dictionary2 = content.Load<Dictionary<string, string>>("Data\\CraftingRecipes");
			foreach (string key2 in dictionary2.Keys)
			{
				string[] array2 = dictionary2[key2].Split('/')[4].Split(' ');
				if (array2[0].Equals("l") && Convert.ToInt32(array2[1]) <= level && !player.craftingRecipes.ContainsKey(key2))
				{
					player.craftingRecipes.Add(key2, 0);
					currentObjectDialogue.Enqueue(parseText(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2677") + key2));
					currentDialogueCharacterIndex = 1;
					dialogueUp = true;
					dialogueTyping = true;
				}
				else
				{
					if (!array2[0].Equals("s"))
					{
						continue;
					}
					int num2 = Convert.ToInt32(array2[2]);
					bool flag2 = false;
					switch (array2[1])
					{
					case "Farming":
						if (player.FarmingLevel >= num2)
						{
							flag2 = true;
						}
						break;
					case "Fishing":
						if (player.FishingLevel >= num2)
						{
							flag2 = true;
						}
						break;
					case "Mining":
						if (player.MiningLevel >= num2)
						{
							flag2 = true;
						}
						break;
					case "Combat":
						if (player.CombatLevel >= num2)
						{
							flag2 = true;
						}
						break;
					case "Foraging":
						if (player.ForagingLevel >= num2)
						{
							flag2 = true;
						}
						break;
					case "Luck":
						if (player.LuckLevel >= num2)
						{
							flag2 = true;
						}
						break;
					}
					if (flag2 && !player.craftingRecipes.ContainsKey(key2))
					{
						player.craftingRecipes.Add(key2, 0);
						currentObjectDialogue.Enqueue(parseText(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2677") + key2));
						currentDialogueCharacterIndex = 1;
						dialogueUp = true;
						dialogueTyping = true;
					}
				}
			}
		}

		public static void exitActiveMenu()
		{
			Toolbar.toolbarPressed = false;
			activeClickableMenu = null;
			if (currentLocation != null && currentLocation.tapToMove != null)
			{
				currentLocation.tapToMove.OnCloseActiveMenu();
			}
			if (input != null)
			{
				GamePadState gamePadState = input.GetGamePadState();
				if (gamePadState.IsConnected && !gamePadState.IsButtonDown(Buttons.DPadUp) && !gamePadState.IsButtonDown(Buttons.DPadDown) && !gamePadState.IsButtonDown(Buttons.DPadLeft) && !gamePadState.IsButtonDown(Buttons.DPadRight) && !gamePadState.IsButtonDown(Buttons.LeftThumbstickUp) && !gamePadState.IsButtonDown(Buttons.LeftThumbstickDown) && !gamePadState.IsButtonDown(Buttons.LeftThumbstickLeft) && !gamePadState.IsButtonDown(Buttons.LeftThumbstickRight) && currentLocation != null && currentLocation.tapToMove != null)
				{
					currentLocation.tapToMove.mobileKeyStates.Reset();
				}
			}
		}

		public static void fadeScreenIn()
		{
			fadeToBlack = true;
			fadeIn = true;
			fadeToBlackAlpha = 1f;
			player.CanMove = false;
		}

		public static void fadeScreenToBlack()
		{
			screenFade.FadeScreenToBlack();
		}

		public static void fadeClear()
		{
			screenFade.FadeClear();
		}

		private bool onFadeToBlackComplete()
		{
			bool result = false;
			if (killScreen)
			{
				viewportFreeze = true;
				viewport.X = -10000;
			}
			if (exitToTitle)
			{
				menuUp = false;
				setGameMode(4);
				menuChoice = 0;
				fadeIn = false;
				fadeToBlack = true;
				fadeToBlackAlpha = 0.01f;
				exitToTitle = false;
				changeMusicTrack("none");
				ClearDebrisWeather(debrisWeather);
				baseDebrisWeatherCount = 0;
				return true;
			}
			if (timeOfDayAfterFade != -1)
			{
				timeOfDay = timeOfDayAfterFade;
				timeOfDayAfterFade = -1;
			}
			if (!nonWarpFade && locationRequest != null && !menuUp)
			{
				GameLocation gameLocation = currentLocation;
				if (emoteMenu != null)
				{
					emoteMenu.exitThisMenuNoSound();
				}
				if (client != null && currentLocation != null)
				{
					currentLocation.StoreCachedMultiplayerMap(multiplayer.cachedMultiplayerMaps);
				}
				currentLocation.cleanupBeforePlayerExit();
				multiplayer.broadcastLocationDelta(currentLocation);
				bool flag = false;
				displayFarmer = true;
				if (eventOver)
				{
					eventFinished();
					if (dayOfMonth == 0)
					{
						newDayAfterFade(delegate
						{
							player.Position = new Vector2(320f, 320f);
						});
					}
					return true;
				}
				if (locationRequest.IsRequestFor(currentLocation) && player.previousLocationName != "" && !eventUp && !currentLocation.Name.StartsWith("UndergroundMine"))
				{
					player.Position = new Vector2(xLocationAfterWarp * 64, yLocationAfterWarp * 64 - (player.Sprite.getHeight() - 32) + 16);
					viewportFreeze = false;
					currentLocation.resetForPlayerEntry();
					flag = true;
				}
				else
				{
					if (locationRequest.Name.StartsWith("UndergroundMine"))
					{
						if (!currentLocation.Name.StartsWith("UndergroundMine"))
						{
							changeMusicTrack("none");
						}
						MineShaft mineShaft = locationRequest.Location as MineShaft;
						if (player.IsSitting())
						{
							player.StopSitting(animate: false);
						}
						player.Halt();
						player.forceCanMove();
						if (!IsClient || (locationRequest.Location != null && locationRequest.Location.Root != null))
						{
							mineShaft.resetForPlayerEntry();
							flag = true;
						}
						currentLocation = mineShaft;
						currentLocation.Map.LoadTileSheets(mapDisplayDevice);
						checkForRunButton(GetKeyboardState());
					}
					if (!eventUp && !menuUp)
					{
						player.Position = new Vector2(xLocationAfterWarp * 64, yLocationAfterWarp * 64 - (player.Sprite.getHeight() - 32) + 16);
					}
					if (!locationRequest.Name.StartsWith("UndergroundMine"))
					{
						currentLocation = locationRequest.Location;
						if (!IsClient)
						{
							locationRequest.Loaded(locationRequest.Location);
							currentLocation.resetForPlayerEntry();
							flag = true;
						}
						currentLocation.Map.LoadTileSheets(mapDisplayDevice);
						if (!viewportFreeze && currentLocation.Map.DisplayWidth <= viewport.Width)
						{
							viewport.X = (currentLocation.Map.DisplayWidth - viewport.Width) / 2;
						}
						if (!viewportFreeze && currentLocation.Map.DisplayHeight <= viewport.Height)
						{
							viewport.Y = (currentLocation.Map.DisplayHeight - viewport.Height) / 2;
						}
						checkForRunButton(GetKeyboardState(), ignoreKeyPressQualifier: true);
					}
					if (!eventUp)
					{
						viewportFreeze = false;
					}
				}
				forceSnapOnNextViewportUpdate = true;
				player.FarmerSprite.PauseForSingleAnimation = false;
				player.faceDirection(facingDirectionAfterWarp);
				_isWarping = false;
				if (player.ActiveObject != null)
				{
					player.showCarrying();
				}
				else
				{
					player.showNotCarrying();
				}
				if (IsClient)
				{
					if (locationRequest.Location != null && locationRequest.Location.Root != null && multiplayer.isActiveLocation(locationRequest.Location))
					{
						currentLocation = locationRequest.Location;
						locationRequest.Loaded(locationRequest.Location);
						if (!flag)
						{
							currentLocation.resetForPlayerEntry();
						}
						player.currentLocation = currentLocation;
						locationRequest.Warped(currentLocation);
						currentLocation.updateSeasonalTileSheets();
						if (IsDebrisWeatherHere())
						{
							populateDebrisWeatherArray(debrisWeather);
						}
						warpingForForcedRemoteEvent = false;
						locationRequest = null;
					}
					else
					{
						requestLocationInfoFromServer();
						if (currentLocation == null)
						{
							return true;
						}
					}
				}
				else
				{
					player.currentLocation = locationRequest.Location;
					locationRequest.Warped(locationRequest.Location);
					locationRequest = null;
				}
				if (locationRequest == null && currentLocation.Name == "Farm" && !eventUp)
				{
					if (player.position.X / 64f >= (float)(currentLocation.map.Layers[0].LayerWidth - 1))
					{
						player.position.X -= 64f;
					}
					else if (player.position.Y / 64f >= (float)(currentLocation.map.Layers[0].LayerHeight - 1))
					{
						player.position.Y -= 32f;
					}
					if (player.position.Y / 64f >= (float)(currentLocation.map.Layers[0].LayerHeight - 2))
					{
						player.position.X -= 48f;
					}
				}
				if (gameLocation != null && gameLocation.Name.StartsWith("UndergroundMine") && currentLocation != null && !currentLocation.Name.StartsWith("UndergroundMine"))
				{
					MineShaft.OnLeftMines();
				}
				result = true;
			}
			if (newDay)
			{
				Action after = delegate
				{
					if (eventOver)
					{
						eventFinished();
						if (dayOfMonth == 0)
						{
							newDayAfterFade(delegate
							{
								player.Position = new Vector2(320f, 320f);
							});
						}
					}
					nonWarpFade = false;
					fadeIn = false;
				};
				newDayAfterFade(after);
				return true;
			}
			if (eventOver)
			{
				eventFinished();
				if (dayOfMonth == 0)
				{
					Action after2 = delegate
					{
						currentLocation.resetForPlayerEntry();
						nonWarpFade = false;
						fadeIn = false;
					};
					newDayAfterFade(after2);
				}
				return true;
			}
			if (boardingBus)
			{
				boardingBus = false;
				drawObjectDialogue(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2694") + (currentLocation.Name.Equals("Desert") ? content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2696") : content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2697")));
				messagePause = true;
				viewportFreeze = false;
			}
			if (IsRainingHere() && currentSong != null && currentSong != null && currentSong.Name.Equals("rain"))
			{
				if (currentLocation.IsOutdoors)
				{
					currentSong.SetVariable("Frequency", 100f);
				}
				else if (!currentLocation.Name.StartsWith("UndergroundMine"))
				{
					currentSong.SetVariable("Frequency", 15f);
				}
			}
			return result;
		}

		public static void ClearDebrisWeather(List<WeatherDebris> debris)
		{
			for (int i = 0; i < debris.Count; i++)
			{
				debrisWeatherPool.Add(debris[i]);
			}
			debris.Clear();
		}

		private static void onFadedBackInComplete()
		{
			if (killScreen)
			{
				pauseThenMessage(1500, "..." + player.Name + "?", showProgressBar: false);
			}
			else if (!eventUp)
			{
				player.CanMove = true;
			}
			checkForRunButton(oldKBState, ignoreKeyPressQualifier: true);
		}

		public static void UpdateOther(GameTime time)
		{
			if (currentLocation == null || (!player.passedOut && screenFade.UpdateFade(time)))
			{
				return;
			}
			if (dialogueUp || currentBillboard != 0)
			{
				player.CanMove = false;
			}
			for (int num = delayedActions.Count - 1; num >= 0; num--)
			{
				DelayedAction delayedAction = delayedActions[num];
				if (delayedAction.update(time) && delayedActions.Contains(delayedAction))
				{
					delayedActions.Remove(delayedAction);
				}
			}
			if (timeOfDay >= 2600 || player.stamina <= -15f)
			{
				if (currentMinigame != null && currentMinigame.forceQuit())
				{
					currentMinigame = null;
				}
				if (currentMinigame == null && player.canMove && player.freezePause <= 0 && !player.UsingTool && !eventUp && (IsMasterGame || (bool)player.isCustomized) && locationRequest == null && activeClickableMenu == null)
				{
					player.startToPassOut();
					player.freezePause = 7000;
				}
			}
			for (int num2 = screenOverlayTempSprites.Count - 1; num2 >= 0; num2--)
			{
				if (screenOverlayTempSprites[num2].update(time))
				{
					screenOverlayTempSprites.RemoveAt(num2);
				}
			}
			for (int num3 = uiOverlayTempSprites.Count - 1; num3 >= 0; num3--)
			{
				if (uiOverlayTempSprites[num3].update(time))
				{
					uiOverlayTempSprites.RemoveAt(num3);
				}
			}
			if (pickingTool)
			{
				pickToolInterval += time.ElapsedGameTime.Milliseconds;
				if (pickToolInterval > 500f)
				{
					pickingTool = false;
					pickToolInterval = 0f;
					if (!eventUp)
					{
						player.CanMove = true;
					}
					player.UsingTool = false;
					switch (player.FacingDirection)
					{
					case 0:
						player.Sprite.currentFrame = 16;
						break;
					case 1:
						player.Sprite.currentFrame = 8;
						break;
					case 2:
						player.Sprite.currentFrame = 0;
						break;
					case 3:
						player.Sprite.currentFrame = 24;
						break;
					}
					if (!GetKeyboardState().IsKeyDown(Keys.LeftShift))
					{
						player.setRunning(options.autoRun);
					}
				}
				else if (pickToolInterval > 83.333336f)
				{
					switch (player.FacingDirection)
					{
					case 0:
						player.FarmerSprite.setCurrentFrame(196);
						break;
					case 1:
						player.FarmerSprite.setCurrentFrame(194);
						break;
					case 2:
						player.FarmerSprite.setCurrentFrame(192);
						break;
					case 3:
						player.FarmerSprite.setCurrentFrame(198);
						break;
					}
				}
			}
			if ((player.CanMove || player.UsingTool) && shouldTimePass())
			{
				buffsDisplay.update(time);
			}
			if (player.CurrentItem != null)
			{
				player.CurrentItem.actionWhenBeingHeld(player);
			}
			float num4 = dialogueButtonScale;
			dialogueButtonScale = (float)(16.0 * Math.Sin(time.TotalGameTime.TotalMilliseconds % 1570.0 / 500.0));
			if (num4 > dialogueButtonScale && !dialogueButtonShrinking)
			{
				dialogueButtonShrinking = true;
			}
			else if (num4 < dialogueButtonScale && dialogueButtonShrinking)
			{
				dialogueButtonShrinking = false;
			}
			if (player.currentUpgrade != null && currentLocation.Name.Equals("Farm") && player.currentUpgrade.daysLeftTillUpgradeDone <= 3)
			{
				player.currentUpgrade.update(time.ElapsedGameTime.Milliseconds);
			}
			if (screenGlow)
			{
				if (screenGlowUp || screenGlowHold)
				{
					if (screenGlowHold)
					{
						screenGlowAlpha = Math.Min(screenGlowAlpha + screenGlowRate, screenGlowMax);
					}
					else
					{
						screenGlowAlpha = Math.Min(screenGlowAlpha + 0.03f, 0.6f);
						if (screenGlowAlpha >= 0.6f)
						{
							screenGlowUp = false;
						}
					}
				}
				else
				{
					screenGlowAlpha -= 0.01f;
					if (screenGlowAlpha <= 0f)
					{
						screenGlow = false;
					}
				}
			}
			for (int num5 = hudMessages.Count - 1; num5 >= 0; num5--)
			{
				if (hudMessages.ElementAt(num5).update(time))
				{
					hudMessages.RemoveAt(num5);
				}
			}
			updateWeather(time);
			if (!fadeToBlack)
			{
				currentLocation.checkForMusic(time);
			}
			if (debrisSoundInterval > 0f)
			{
				debrisSoundInterval -= time.ElapsedGameTime.Milliseconds;
			}
			noteBlockTimer += time.ElapsedGameTime.Milliseconds;
			if (noteBlockTimer > 1000f)
			{
				noteBlockTimer = 0f;
				if (player.health < 20 && CurrentEvent == null)
				{
					hitShakeTimer = 250;
					if (player.health <= 10)
					{
						hitShakeTimer = 500;
						if (showingHealthBar && fadeToBlackAlpha <= 0f)
						{
							int num6 = uiViewport.Width - xEdge - 110 + random.Next(16);
							int num7 = uiViewport.Height - 232 - (player.maxHealth - 100);
							for (int i = 0; i < 3; i++)
							{
								uiOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(366, 412, 5, 6), new Vector2(num6, num7), flipped: false, 0.017f, Color.Red)
								{
									motion = new Vector2(-1.5f, -8 + random.Next(-1, 2)),
									acceleration = new Vector2(0f, 0.5f),
									local = true,
									scale = 4f,
									delayBeforeAnimationStart = i * 150
								});
							}
						}
					}
				}
			}
			if (showKeyHelp && !eventUp)
			{
				keyHelpString = "";
				if (dialogueUp)
				{
					keyHelpString += content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2716");
				}
				else if (menuUp)
				{
					keyHelpString += content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2719");
					keyHelpString += content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2720");
				}
				else if (player.ActiveObject != null)
				{
					keyHelpString += content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2727");
					keyHelpString = keyHelpString + Environment.NewLine + content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2728");
					if (player.numberOfItemsInInventory() < (int)player.maxItems)
					{
						keyHelpString = keyHelpString + Environment.NewLine + content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2729");
					}
					if (player.numberOfItemsInInventory() > 0)
					{
						keyHelpString = keyHelpString + Environment.NewLine + content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2730");
					}
					keyHelpString = keyHelpString + Environment.NewLine + content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2731");
					keyHelpString = keyHelpString + Environment.NewLine + content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2732");
				}
				else
				{
					keyHelpString += content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2733");
					if (player.CurrentTool != null)
					{
						keyHelpString = keyHelpString + Environment.NewLine + content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2734", player.CurrentTool.DisplayName);
					}
					if (player.numberOfItemsInInventory() > 0)
					{
						keyHelpString = keyHelpString + Environment.NewLine + content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2735");
					}
					keyHelpString = keyHelpString + Environment.NewLine + content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2731");
					keyHelpString = keyHelpString + Environment.NewLine + content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2732");
				}
			}
			drawLighting = (currentLocation.IsOutdoors && !outdoorLight.Equals(Color.White)) || !ambientLight.Equals(Color.White) || (currentLocation is MineShaft && !((MineShaft)currentLocation).getLightingColor(time).Equals(Color.White));
			if (player.hasBuff(26))
			{
				drawLighting = true;
			}
			if (hitShakeTimer > 0)
			{
				hitShakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (staminaShakeTimer > 0)
			{
				staminaShakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (background != null)
			{
				background.update(viewport);
			}
			cursorTileHintCheckTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			currentCursorTile.X = (viewport.X + getOldMouseX()) / 64;
			currentCursorTile.Y = (viewport.Y + getOldMouseY()) / 64;
			if (cursorTileHintCheckTimer <= 0 || !currentCursorTile.Equals(lastCursorTile))
			{
				cursorTileHintCheckTimer = 250;
				updateCursorTileHint();
				if (player.CanMove)
				{
					checkForRunButton(oldKBState, ignoreKeyPressQualifier: true);
				}
			}
			if (!currentLocation.Name.StartsWith("UndergroundMine"))
			{
				MineShaft.timeSinceLastMusic = 200000;
			}
			if (activeClickableMenu == null && farmEvent == null && keyboardDispatcher != null && !IsChatting)
			{
				keyboardDispatcher.Subscriber = null;
			}
		}

		public static void updateWeather(GameTime time)
		{
			if (IsSnowingHere() && (bool)currentLocation.isOutdoors && !(currentLocation is Desert))
			{
				snowPos = updateFloatingObjectPositionForMovement(current: new Vector2(viewport.X, viewport.Y), w: snowPos, previous: previousViewportPosition, speed: -1f);
			}
			if (IsRainingHere() && currentLocation.IsOutdoors)
			{
				for (int i = 0; i < rainDrops.Count; i++)
				{
					if (rainDrops[i].frame == 0)
					{
						rainDrops[i].accumulator += time.ElapsedGameTime.Milliseconds;
						if (rainDrops[i].accumulator < 70)
						{
							continue;
						}
						rainDrops[i].position += new Vector2(-16 + i * 8 / rainDrops.Count, 32 - i * 8 / rainDrops.Count);
						rainDrops[i].accumulator = 0;
						if (random.NextDouble() < 0.1)
						{
							rainDrops[i].frame++;
						}
						if (currentLocation is IslandNorth || currentLocation is Caldera)
						{
							Point p = new Point((int)rainDrops[i].position.X / 64, (int)rainDrops[i].position.Y / 64);
							p.Y--;
							if (currentLocation.isTileOnMap(p.X, p.Y) && currentLocation.getTileIndexAt(p, "Back") == -1 && currentLocation.getTileIndexAt(p, "Buildings") == -1)
							{
								rainDrops[i].frame = 0;
							}
						}
						if (rainDrops[i].position.Y > (float)(viewport.Y + viewport.Height + 64))
						{
							rainDrops[i].position.Y = viewport.Y - 64;
						}
						continue;
					}
					rainDrops[i].accumulator += time.ElapsedGameTime.Milliseconds;
					if (rainDrops[i].accumulator > 70)
					{
						rainDrops[i].frame = (rainDrops[i].frame + 1) % 4;
						rainDrops[i].accumulator = 0;
						if (rainDrops[i].frame == 0)
						{
							rainDrops[i].position = new Vector2(viewport.X + random.Next(viewport.Width), viewport.Y + random.Next(viewport.Height));
						}
					}
				}
			}
			else if (IsDebrisWeatherHere() && currentLocation.IsOutdoors && !currentLocation.ignoreDebrisWeather)
			{
				if (currentSeason.Equals("fall") && random.NextDouble() < 0.001 && windGust == 0f && WeatherDebris.globalWind >= -0.5f)
				{
					windGust += (float)random.Next(-10, -1) / 100f;
					if (soundBank != null)
					{
						wind = soundBank.GetCue("wind");
						wind.Play();
					}
				}
				else if (windGust != 0f)
				{
					windGust = Math.Max(-5f, windGust * 1.02f);
					WeatherDebris.globalWind = -0.5f + windGust;
					if (windGust < -0.2f && random.NextDouble() < 0.007)
					{
						windGust = 0f;
					}
				}
				foreach (WeatherDebris item in debrisWeather)
				{
					item.update();
				}
			}
			if (WeatherDebris.globalWind < -0.5f && wind != null)
			{
				WeatherDebris.globalWind = Math.Min(-0.5f, WeatherDebris.globalWind + 0.015f);
				wind.SetVariable("Volume", (0f - WeatherDebris.globalWind) * 20f);
				wind.SetVariable("Frequency", (0f - WeatherDebris.globalWind) * 20f);
				if (WeatherDebris.globalWind == -0.5f)
				{
					wind.Stop(AudioStopOptions.AsAuthored);
				}
			}
			if (currentLocation != null)
			{
				currentLocation.UpdateLocationSpecificWeatherDebris();
			}
		}

		public static void updateCursorTileHint()
		{
			if (activeClickableMenu != null)
			{
				return;
			}
			mouseCursorTransparency = 1f;
			isActionAtCurrentCursorTile = false;
			isInspectionAtCurrentCursorTile = false;
			isSpeechAtCurrentCursorTile = false;
			int xTile = (viewport.X + getOldMouseX()) / 64;
			int num = (viewport.Y + getOldMouseY()) / 64;
			if (currentLocation != null)
			{
				isActionAtCurrentCursorTile = currentLocation.isActionableTile(xTile, num, player);
				if (!isActionAtCurrentCursorTile)
				{
					isActionAtCurrentCursorTile = currentLocation.isActionableTile(xTile, num + 1, player);
				}
			}
			lastCursorTile = currentCursorTile;
		}

		public static void updateMusic()
		{
			if (soundBank == null)
			{
				return;
			}
			if (game1.IsMainInstance)
			{
				Game1 game = null;
				string text = null;
				int num = 0;
				int num2 = 1;
				int num3 = 2;
				int num4 = 5;
				int num5 = 6;
				int num6 = 7;
				int num7 = num;
				float num8 = GetDefaultSongPriority(getMusicTrackName(), game1.instanceIsOverridingTrack);
				MusicContext musicContext = MusicContext.Default;
				foreach (Game1 gameInstance in GameRunner.instance.gameInstances)
				{
					MusicContext instanceActiveMusicContext = gameInstance._instanceActiveMusicContext;
					if (gameInstance.IsMainInstance)
					{
						musicContext = instanceActiveMusicContext;
					}
					string text2 = null;
					string text3 = null;
					if (gameInstance._instanceRequestedMusicTracks.TryGetValue(instanceActiveMusicContext, out var value))
					{
						text2 = value.Key;
					}
					if (gameInstance.instanceIsOverridingTrack && gameInstance.instanceCurrentSong != null)
					{
						text3 = gameInstance.instanceCurrentSong.Name;
					}
					if (instanceActiveMusicContext == MusicContext.Event && num7 < num5)
					{
						if (text2 != null)
						{
							num7 = num5;
							game = gameInstance;
							text = text2;
						}
					}
					else if (instanceActiveMusicContext == MusicContext.MiniGame && num7 < num4)
					{
						if (text2 != null)
						{
							num7 = num4;
							game = gameInstance;
							text = text2;
						}
					}
					else if (instanceActiveMusicContext == MusicContext.SubLocation && num7 < num2)
					{
						if (text2 != null)
						{
							num7 = num2;
							game = gameInstance;
							text = ((text3 == null) ? text2 : text3);
						}
					}
					else if (text2 == "mermaidSong")
					{
						num7 = num6;
						game = gameInstance;
						text = text2;
					}
					if (instanceActiveMusicContext == MusicContext.Default && musicContext <= instanceActiveMusicContext && text2 != null)
					{
						float num9 = GetDefaultSongPriority(text2, gameInstance.instanceIsOverridingTrack);
						if (num8 < num9)
						{
							num8 = num9;
							num7 = num3;
							game = gameInstance;
							text = ((text3 == null) ? text2 : text3);
						}
					}
				}
				if (game == null || game == game1)
				{
					if (doesMusicContextHaveTrack(MusicContext.ImportantSplitScreenMusic))
					{
						stopMusicTrack(MusicContext.ImportantSplitScreenMusic);
					}
				}
				else if (text == null && doesMusicContextHaveTrack(MusicContext.ImportantSplitScreenMusic))
				{
					stopMusicTrack(MusicContext.ImportantSplitScreenMusic);
				}
				else if (text != null && getMusicTrackName(MusicContext.ImportantSplitScreenMusic) != text)
				{
					changeMusicTrack(text, track_interruptable: false, MusicContext.ImportantSplitScreenMusic);
				}
			}
			string text4 = "";
			bool flag = false;
			bool flag2 = false;
			if (currentLocation != null && currentLocation.IsMiniJukeboxPlaying() && (!requestedMusicDirty || requestedMusicTrackOverrideable) && currentTrackOverrideable)
			{
				text4 = "";
				flag2 = true;
				string text5 = currentLocation.miniJukeboxTrack.Value;
				if (text5 == "random")
				{
					text5 = ((currentLocation.randomMiniJukeboxTrack.Value != null) ? currentLocation.randomMiniJukeboxTrack.Value : "");
				}
				if (currentSong == null || !currentSong.IsPlaying || currentSong.Name != text5)
				{
					text4 = text5;
					requestedMusicDirty = false;
					flag = true;
				}
			}
			if (isOverridingTrack != flag2)
			{
				isOverridingTrack = flag2;
				if (!isOverridingTrack)
				{
					requestedMusicDirty = true;
				}
			}
			if (requestedMusicDirty)
			{
				text4 = requestedMusicTrack;
				flag = requestedMusicTrackOverrideable;
			}
			if (!text4.Equals(""))
			{
				musicPlayerVolume = Math.Max(0f, Math.Min(options.musicVolumeLevel, musicPlayerVolume - 0.01f));
				ambientPlayerVolume = Math.Max(0f, Math.Min(options.musicVolumeLevel, ambientPlayerVolume - 0.01f));
				if (game1.IsMainInstance)
				{
					musicCategory.SetVolume(musicPlayerVolume);
					ambientCategory.SetVolume(ambientPlayerVolume);
				}
				if (musicPlayerVolume != 0f || ambientPlayerVolume != 0f || currentSong == null)
				{
					return;
				}
				if (text4.Equals("none"))
				{
					jukeboxPlaying = false;
					currentSong.Stop(AudioStopOptions.Immediate);
				}
				else if ((options.musicVolumeLevel != 0f || options.ambientVolumeLevel != 0f) && (!text4.Equals("rain") || endOfNightMenus.Count == 0))
				{
					if (game1.IsMainInstance)
					{
						currentSong.Stop(AudioStopOptions.Immediate);
						currentSong.Dispose();
					}
					currentSong = soundBank.GetCue(text4);
					if (game1.IsMainInstance)
					{
						currentSong.Play();
					}
					if (game1.IsMainInstance && currentSong != null && currentSong.Name.Equals("rain") && currentLocation != null)
					{
						if (IsRainingHere())
						{
							if (currentLocation.IsOutdoors)
							{
								currentSong.SetVariable("Frequency", 100f);
							}
							else if (!currentLocation.Name.StartsWith("UndergroundMine"))
							{
								currentSong.SetVariable("Frequency", 15f);
							}
						}
						else if (eventUp)
						{
							currentSong.SetVariable("Frequency", 100f);
						}
					}
				}
				else
				{
					currentSong.Stop(AudioStopOptions.Immediate);
				}
				currentTrackOverrideable = flag;
				requestedMusicDirty = false;
			}
			else if (musicPlayerVolume < options.musicVolumeLevel || ambientPlayerVolume < options.ambientVolumeLevel)
			{
				if (musicPlayerVolume < options.musicVolumeLevel)
				{
					musicPlayerVolume = Math.Min(1f, musicPlayerVolume += 0.01f);
					if (game1.IsMainInstance)
					{
						musicCategory.SetVolume(options.musicVolumeLevel);
					}
				}
				if (ambientPlayerVolume < options.ambientVolumeLevel)
				{
					ambientPlayerVolume = Math.Min(1f, ambientPlayerVolume += 0.015f);
					if (game1.IsMainInstance)
					{
						ambientCategory.SetVolume(ambientPlayerVolume);
					}
				}
			}
			else if (currentSong != null && !currentSong.IsPlaying && !currentSong.IsStopped)
			{
				currentSong = soundBank.GetCue(currentSong.Name);
				if (game1.IsMainInstance)
				{
					currentSong.Play();
				}
			}
		}

		public static int GetDefaultSongPriority(string song_name, bool is_playing_override)
		{
			if (is_playing_override)
			{
				return 9;
			}
			if (song_name.Equals("none"))
			{
				return 0;
			}
			if (song_name.EndsWith("_day_ambient") || song_name.EndsWith("_night_ambient") || song_name.Equals("rain"))
			{
				return 1;
			}
			if (song_name.StartsWith(currentSeason))
			{
				return 2;
			}
			if (song_name.Contains("town"))
			{
				return 3;
			}
			if (song_name.Equals("jungle_ambience") || song_name.Contains("Ambient"))
			{
				return 7;
			}
			if (song_name.Equals("IslandMusic"))
			{
				return 8;
			}
			if (song_name.EndsWith("Mine"))
			{
				return 20;
			}
			return 10;
		}

		public static void initializeVolumeLevels()
		{
			if (!LocalMultiplayer.IsLocalMultiplayer() || game1.IsMainInstance)
			{
				soundCategory.SetVolume(options.soundVolumeLevel);
				musicCategory.SetVolume(options.musicVolumeLevel);
				ambientCategory.SetVolume(options.ambientVolumeLevel);
				footstepCategory.SetVolume(options.footstepVolumeLevel);
			}
		}

		public static Vector2 updateFloatingObjectPositionForMovement(Vector2 w, Vector2 current, Vector2 previous, float speed)
		{
			if (current.Y < previous.Y)
			{
				w.Y -= Math.Abs(current.Y - previous.Y) * speed;
			}
			else if (current.Y > previous.Y)
			{
				w.Y += Math.Abs(current.Y - previous.Y) * speed;
			}
			if (current.X > previous.X)
			{
				w.X += Math.Abs(current.X - previous.X) * speed;
			}
			else if (current.X < previous.X)
			{
				w.X -= Math.Abs(current.X - previous.X) * speed;
			}
			return w;
		}

		public static void updateRaindropPosition()
		{
			if (!IsRainingHere())
			{
				return;
			}
			for (int i = 0; i < rainDrops.Count; i++)
			{
				if (rainDrops[i].position.Y > (float)(viewport.Y + viewport.Height + 64))
				{
					rainDrops[i].position.Y = viewport.Y - 64;
				}
				else if (rainDrops[i].position.X < (float)(viewport.X - 64))
				{
					rainDrops[i].position.X = viewport.X + viewport.Width;
				}
				else if (rainDrops[i].position.Y < (float)(viewport.Y - 64))
				{
					rainDrops[i].position.Y = viewport.Y + viewport.Height;
				}
				else if (rainDrops[i].position.X > (float)(viewport.X + viewport.Width + 64))
				{
					rainDrops[i].position.X = viewport.X - 64;
				}
			}
		}

		public static void randomizeRainPositions()
		{
			foreach (RainDrop rainDrop in rainDrops)
			{
				rainDrop.Initialize(viewport.X + random.Next(viewport.Width), viewport.Y + random.Next(viewport.Height), random.Next(4), random.Next(70));
			}
		}

		public static void randomizeDebrisWeatherPositions(List<WeatherDebris> debris)
		{
			if (debris == null)
			{
				return;
			}
			Vector2 vector = new Vector2(viewport.X, viewport.Y);
			foreach (WeatherDebris debri in debris)
			{
				debri.position = vector + new Vector2(random.Next(viewport.Width), random.Next(viewport.Height));
			}
		}

		public static void eventFinished()
		{
			player.canOnlyWalk = false;
			if (player.bathingClothes.Value)
			{
				player.canOnlyWalk = true;
			}
			eventOver = false;
			eventUp = false;
			player.CanMove = true;
			displayHUD = true;
			player.faceDirection(player.orientationBeforeEvent);
			player.completelyStopAnimatingOrDoingAction();
			viewportFreeze = false;
			Action action = null;
			if (currentLocation.currentEvent.onEventFinished != null)
			{
				action = currentLocation.currentEvent.onEventFinished;
				currentLocation.currentEvent.onEventFinished = null;
			}
			LocationRequest locationRequest = null;
			if (currentLocation.currentEvent != null)
			{
				locationRequest = currentLocation.currentEvent.exitLocation;
				currentLocation.currentEvent.cleanup();
				currentLocation.currentEvent = null;
			}
			if (player.ActiveObject != null)
			{
				player.showCarrying();
			}
			if (IsRainingHere() && (currentSong == null || !currentSong.Name.Equals("rain")) && !currentLocation.Name.StartsWith("UndergroundMine"))
			{
				changeMusicTrack("rain", track_interruptable: true);
			}
			else if (!IsRainingHere() && (currentSong == null || currentSong.Name == null || !currentSong.Name.Contains(currentSeason)))
			{
				changeMusicTrack("none", track_interruptable: true);
			}
			if (dayOfMonth != 0)
			{
				currentLightSources.Clear();
			}
			if (locationRequest == null && currentLocation != null && Game1.locationRequest == null)
			{
				locationRequest = new LocationRequest(currentLocation.NameOrUniqueName, currentLocation.isStructure, currentLocation);
			}
			if (locationRequest != null)
			{
				if (locationRequest.Location is Farm && player.positionBeforeEvent.Y == 64f)
				{
					player.positionBeforeEvent.X += 1f;
				}
				locationRequest.OnWarp += delegate
				{
					player.locationBeforeForcedEvent.Value = null;
				};
				warpFarmer(locationRequest, (int)player.positionBeforeEvent.X, (int)player.positionBeforeEvent.Y, player.orientationBeforeEvent);
			}
			else
			{
				player.setTileLocation(player.positionBeforeEvent);
				player.locationBeforeForcedEvent.Value = null;
			}
			nonWarpFade = false;
			fadeToBlackAlpha = 1f;
			action?.Invoke();
		}

		public static void populateDebrisWeatherArray(List<WeatherDebris> debris, int base_debris_count = -1, int debris_index = -1)
		{
			ClearDebrisWeather(debris);
			baseDebrisWeatherCount = 0;
			if (debris == debrisWeather)
			{
				isDebrisWeather = true;
				baseDebrisWeatherCount = random.Next(16, 64);
			}
			if (base_debris_count == -1)
			{
				base_debris_count = baseDebrisWeatherCount;
			}
			int num = _GetWeatherDebrisCountForViewportSize(base_debris_count, viewport);
			if (debris_index == -1)
			{
				debris_index = GetDebrisWeatherIndex();
			}
			for (int i = 0; i < num; i++)
			{
				debris.Add(new WeatherDebris(new Vector2(viewport.X, viewport.Y) + new Vector2(random.Next(0, viewport.Width), random.Next(0, viewport.Height)), debris_index, (float)random.Next(15) / 500f, (float)random.Next(-10, 0) / 50f, (float)random.Next(10) / 50f));
			}
		}

		public static int GetDebrisWeatherIndex()
		{
			if (currentSeason.Equals("spring"))
			{
				return 0;
			}
			if (currentSeason.Equals("winter"))
			{
				return 3;
			}
			return 2;
		}

		private static void newSeason()
		{
			switch (currentSeason)
			{
			case "spring":
				currentSeason = "summer";
				break;
			case "summer":
				currentSeason = "fall";
				break;
			case "fall":
				currentSeason = "winter";
				break;
			case "winter":
				currentSeason = "spring";
				break;
			}
			setGraphicsForSeason();
			dayOfMonth = 1;
			Utility.ForAllLocations(delegate(GameLocation l)
			{
				l.seasonUpdate(GetSeasonForLocation(l));
			});
		}

		public static void playItemNumberSelectSound()
		{
			if (selectedItemsType.Equals("flutePitch"))
			{
				if (soundBank != null)
				{
					ICue cue = soundBank.GetCue("flute");
					cue.SetVariable("Pitch", 100 * numberOfSelectedItems);
					cue.Play();
				}
			}
			else if (selectedItemsType.Equals("drumTone"))
			{
				playSound("drumkit" + numberOfSelectedItems);
			}
			else
			{
				playSound("toolSwap");
			}
		}

		public static void slotsDone()
		{
			Response[] answerChoices = new Response[2]
			{
				new Response("Play", content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2766")),
				new Response("Leave", content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2768"))
			};
			if (slotResult[3] == 'x')
			{
				currentLocation.createQuestionDialogue(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2769", player.clubCoins), answerChoices, content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2771") + currentLocation.map.GetLayer("Buildings").PickTile(new Location((int)(player.GetGrabTile().X * 64f), (int)(player.GetGrabTile().Y * 64f)), viewport.Size).Properties["Action"].ToString().Split(' ')[1]);
				currentDialogueCharacterIndex = currentObjectDialogue.Peek().Length - 1;
				return;
			}
			playSound("money");
			string text = (slotResult.Substring(0, 3).Equals("===") ? content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2776") : "");
			player.clubCoins += Convert.ToInt32(slotResult.Substring(3));
			currentLocation.createQuestionDialogue(parseText(text + content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2777", slotResult.Substring(3))), answerChoices, content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2771") + currentLocation.map.GetLayer("Buildings").PickTile(new Location((int)(player.GetGrabTile().X * 64f), (int)(player.GetGrabTile().Y * 64f)), viewport.Size).Properties["Action"].ToString().Split(' ')[1]);
			currentDialogueCharacterIndex = currentObjectDialogue.Peek().Length - 1;
		}

		public static void prepareMultiplayerWedding(Farmer farmer)
		{
		}

		public static void prepareSpouseForWedding(Farmer farmer)
		{
			NPC characterFromName = getCharacterFromName(farmer.spouse);
			characterFromName.Schedule = null;
			characterFromName.DefaultMap = farmer.homeLocation.Value;
			characterFromName.DefaultPosition = Utility.PointToVector2((getLocationFromName(farmer.homeLocation.Value) as FarmHouse).getSpouseBedSpot(farmer.spouse)) * 64f;
			characterFromName.DefaultFacingDirection = 2;
		}

		public static void AddModNPCs()
		{
			LocalizedContentManager localizedContentManager = new LocalizedContentManager(game1.Content.ServiceProvider, game1.Content.RootDirectory);
			Dictionary<string, string> dictionary = localizedContentManager.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			Dictionary<string, string> dictionary2 = content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			foreach (string key in dictionary2.Keys)
			{
				if (dictionary.ContainsKey(key))
				{
					continue;
				}
				try
				{
					NPC characterFromName = getCharacterFromName(key, mustBeVillager: true, useLocationsListOnly: true);
					if (characterFromName == null)
					{
						string text = dictionary2[key];
						getLocationFromNameInLocationsList(text.Split('/')[10].Split(' ')[0])?.addCharacter(new NPC(new AnimatedSprite("Characters\\" + NPC.getTextureNameForCharacter(key), 0, 16, 32), new Vector2(Convert.ToInt32(text.Split('/')[10].Split(' ')[1]) * 64, Convert.ToInt32(text.Split('/')[10].Split(' ')[2]) * 64), text.Split('/')[10].Split(' ')[0], 0, key, null, content.Load<Texture2D>("Portraits\\" + NPC.getTextureNameForCharacter(key)), eventActor: false));
					}
				}
				catch (Exception)
				{
				}
			}
			localizedContentManager.Unload();
		}

		public static void fixProblems()
		{
			if (!IsMasterGame)
			{
				return;
			}
			List<NPC> pooledList = Utility.getPooledList();
			try
			{
				Utility.getAllCharacters(pooledList);
				Dictionary<string, string> dictionary = content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
				foreach (string key in dictionary.Keys)
				{
					bool flag = false;
					if ((key == "Kent" && year <= 1) || (key == "Leo" && !MasterPlayer.hasOrWillReceiveMail("addedParrotBoy")))
					{
						continue;
					}
					foreach (NPC item4 in pooledList)
					{
						if (!item4.isVillager() || !item4.Name.Equals(key))
						{
							continue;
						}
						flag = true;
						if ((bool)item4.datable && item4.getSpouse() == null)
						{
							string text = dictionary[key].Split('/')[10].Split(' ')[0];
							if (item4.DefaultMap != text && (item4.DefaultMap.ToLower().Contains("cabin") || item4.DefaultMap.Equals("FarmHouse")))
							{
								Console.WriteLine("Fixing " + item4.Name + " who was improperly divorced and left stranded");
								item4.PerformDivorce();
							}
						}
						break;
					}
					if (!flag)
					{
						try
						{
							getLocationFromName(dictionary[key].Split('/')[10].Split(' ')[0]).addCharacter(new NPC(new AnimatedSprite("Characters\\" + NPC.getTextureNameForCharacter(key), 0, 16, 32), new Vector2(Convert.ToInt32(dictionary[key].Split('/')[10].Split(' ')[1]) * 64, Convert.ToInt32(dictionary[key].Split('/')[10].Split(' ')[2]) * 64), dictionary[key].Split('/')[10].Split(' ')[0], 0, key, null, content.Load<Texture2D>("Portraits\\" + NPC.getTextureNameForCharacter(key)), eventActor: false));
						}
						catch (Exception)
						{
						}
					}
				}
			}
			finally
			{
				Utility.returnPooledList(pooledList);
				pooledList = null;
			}
			int num = getAllFarmers().Count();
			Dictionary<Type, int> missingTools = new Dictionary<Type, int>();
			missingTools.Add(typeof(Axe), num);
			missingTools.Add(typeof(Pickaxe), num);
			missingTools.Add(typeof(Hoe), num);
			missingTools.Add(typeof(WateringCan), num);
			missingTools.Add(typeof(Wand), 0);
			foreach (Farmer allFarmer in getAllFarmers())
			{
				if (allFarmer.hasOrWillReceiveMail("ReturnScepter"))
				{
					missingTools[typeof(Wand)]++;
				}
			}
			int missingScythes = num;
			foreach (Farmer allFarmer2 in getAllFarmers())
			{
				if (allFarmer2.toolBeingUpgraded.Value != null && missingTools.ContainsKey(allFarmer2.toolBeingUpgraded.Value.GetType()))
				{
					missingTools[allFarmer2.toolBeingUpgraded.Value.GetType()]--;
				}
				for (int i = 0; i < allFarmer2.items.Count; i++)
				{
					if (allFarmer2.items[i] != null)
					{
						checkIsMissingTool(missingTools, ref missingScythes, allFarmer2.items[i]);
					}
				}
			}
			bool flag2 = true;
			for (int j = 0; j < missingTools.Count; j++)
			{
				if (missingTools.ElementAt(j).Value > 0)
				{
					flag2 = false;
					break;
				}
			}
			if (missingScythes > 0)
			{
				flag2 = false;
			}
			if (flag2)
			{
				return;
			}
			foreach (GameLocation location in locations)
			{
				List<Debris> list = new List<Debris>();
				foreach (Debris debri in location.debris)
				{
					Item item2 = debri.item;
					if (item2 == null)
					{
						continue;
					}
					for (int k = 0; k < missingTools.Count; k++)
					{
						if (item2.GetType() == missingTools.ElementAt(k).Key)
						{
							list.Add(debri);
						}
					}
					if (item2 is MeleeWeapon && (item2 as MeleeWeapon).Name.Equals("Scythe"))
					{
						list.Add(debri);
					}
				}
				foreach (Debris item5 in list)
				{
					location.debris.Remove(item5);
				}
			}
			Utility.iterateChestsAndStorage(delegate(Item item)
			{
				checkIsMissingTool(missingTools, ref missingScythes, item);
			});
			List<string> list2 = new List<string>();
			for (int l = 0; l < missingTools.Count; l++)
			{
				if (missingTools.ElementAt(l).Value > 0)
				{
					for (int m = 0; m < missingTools.ElementAt(l).Value; m++)
					{
						list2.Add(missingTools.ElementAt(l).Key.ToString());
					}
				}
			}
			for (int n = 0; n < missingScythes; n++)
			{
				list2.Add("Scythe");
			}
			if (list2.Count > 0)
			{
				addMailForTomorrow("foundLostTools");
			}
			for (int num2 = 0; num2 < list2.Count; num2++)
			{
				Item item3 = null;
				switch (list2[num2])
				{
				case "StardewValley.Tools.Axe":
					item3 = new Axe();
					break;
				case "StardewValley.Tools.Hoe":
					item3 = new Hoe();
					break;
				case "StardewValley.Tools.WateringCan":
					item3 = new WateringCan();
					break;
				case "Scythe":
					item3 = new MeleeWeapon(47);
					break;
				case "StardewValley.Tools.Pickaxe":
					item3 = new Pickaxe();
					break;
				case "StardewValley.Tools.Wand":
					item3 = new Wand();
					break;
				}
				if (item3 != null)
				{
					if (newDaySync != null)
					{
						player.team.newLostAndFoundItems.Value = true;
					}
					player.team.returnedDonations.Add(item3);
				}
			}
		}

		private static void checkIsMissingTool(Dictionary<Type, int> missingTools, ref int missingScythes, Item item)
		{
			for (int i = 0; i < missingTools.Count; i++)
			{
				if (item.GetType() == missingTools.ElementAt(i).Key)
				{
					missingTools[missingTools.ElementAt(i).Key]--;
				}
			}
			if (item is MeleeWeapon && (item as MeleeWeapon).Name.Equals("Scythe"))
			{
				missingScythes--;
			}
		}

		public static void newDayAfterFade(Action after)
		{
			if (player.currentLocation != null)
			{
				if (player.rightRing.Value != null)
				{
					player.rightRing.Value.onLeaveLocation(player, player.currentLocation);
				}
				if (player.leftRing.Value != null)
				{
					player.leftRing.Value.onLeaveLocation(player, player.currentLocation);
				}
			}
			if (LocalMultiplayer.IsLocalMultiplayer())
			{
				Game1 game = game1;
				hooks.OnGame1_NewDayAfterFade(delegate
				{
					game1.isLocalMultiplayerNewDayActive = true;
					_afterNewDayAction = after;
					GameRunner.instance.activeNewDayProcesses.Add(new KeyValuePair<Game1, IEnumerator<int>>(game1, _newDayAfterFade()));
				});
				return;
			}
			hooks.OnGame1_NewDayAfterFade(delegate
			{
				_afterNewDayAction = after;
				if (_newDayTask != null)
				{
					Console.WriteLine("Warning: There is already a _newDayTask; unusual code path.");
					Console.WriteLine(Environment.StackTrace);
					Console.WriteLine();
				}
				else
				{
					_newDayTask = new Task(delegate
					{
						Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
						IEnumerator<int> enumerator = _newDayAfterFade();
						while (enumerator.MoveNext())
						{
						}
					});
				}
			});
		}

		public static bool CanAcceptDailyQuest()
		{
			if (questOfTheDay == null)
			{
				return false;
			}
			if (player.acceptedDailyQuest.Value)
			{
				return false;
			}
			if (questOfTheDay.questDescription == null || questOfTheDay.questDescription.Length == 0)
			{
				return false;
			}
			return true;
		}

		private static IEnumerator<int> _newDayAfterFade()
		{
			newDaySync.start();
			flushLocationLookup();
			try
			{
				fixProblems();
			}
			catch (Exception)
			{
			}
			foreach (Farmer allFarmer in getAllFarmers())
			{
				allFarmer.FarmerSprite.PauseForSingleAnimation = false;
			}
			whereIsTodaysFest = null;
			if (wind != null)
			{
				wind.Stop(AudioStopOptions.Immediate);
				wind = null;
			}
			List<int> list = new List<int>(player.chestConsumedMineLevels.Keys);
			foreach (int item in list)
			{
				if (item > 120)
				{
					player.chestConsumedMineLevels.Remove(item);
				}
			}
			player.currentEyes = 0;
			int num;
			if (IsMasterGame)
			{
				player.team.announcedSleepingFarmers.Clear();
				num = (int)uniqueIDForThisGame / 100 + (int)(stats.DaysPlayed * 10) + 1 + (int)stats.StepsTaken;
				newDaySync.sendVar<NetInt, int>("seed", num);
			}
			else
			{
				while (!newDaySync.isVarReady("seed"))
				{
					yield return 0;
				}
				num = newDaySync.waitForVar<NetInt, int>("seed");
			}
			random = new Random(num);
			for (int i = 0; i < dayOfMonth; i++)
			{
				random.Next();
			}
			player.team.endOfNightStatus.UpdateState("sleep");
			newDaySync.barrier("sleep");
			while (!newDaySync.isBarrierReady("sleep"))
			{
				yield return 0;
			}
			gameTimeInterval = 0;
			game1.wasAskedLeoMemory = false;
			player.team.Update();
			player.team.NewDay();
			player.passedOut = false;
			player.CanMove = true;
			player.FarmerSprite.PauseForSingleAnimation = false;
			player.FarmerSprite.StopAnimation();
			player.completelyStopAnimatingOrDoingAction();
			changeMusicTrack("none");
			int num2 = random.Next(194, 240);
			while (Utility.getForbiddenDishesOfTheDay().Contains(num2))
			{
				num2 = random.Next(194, 240);
			}
			int initialStack = random.Next(1, 4 + ((random.NextDouble() < 0.08) ? 10 : 0));
			if (IsMasterGame)
			{
				dishOfTheDay = new Object(Vector2.Zero, num2, initialStack);
			}
			newDaySync.barrier("dishOfTheDay");
			while (!newDaySync.isBarrierReady("dishOfTheDay"))
			{
				yield return 0;
			}
			npcDialogues = null;
			foreach (NPC allCharacter in Utility.getAllCharacters())
			{
				allCharacter.updatedDialogueYet = false;
			}
			int timeElapsed = Utility.CalculateMinutesUntilMorning(timeOfDay);
			foreach (GameLocation location in locations)
			{
				location.currentEvent = null;
				if (IsMasterGame)
				{
					location.passTimeForObjects(timeElapsed);
				}
			}
			if (IsMasterGame)
			{
				foreach (Building building in getFarm().buildings)
				{
					if (building.indoors.Value != null)
					{
						building.indoors.Value.passTimeForObjects(timeElapsed);
					}
				}
			}
			globalOutdoorLighting = 0f;
			outdoorLight = Color.White;
			ambientLight = Color.White;
			if (isLightning && IsMasterGame)
			{
				Utility.overnightLightning();
			}
			tmpTimeOfDay = timeOfDay;
			if (MasterPlayer.hasOrWillReceiveMail("ccBulletinThankYou") && !player.hasOrWillReceiveMail("ccBulletinThankYou"))
			{
				addMailForTomorrow("ccBulletinThankYou");
			}
			ReceiveMailForTomorrow();
			if (player.friendshipData.Count() > 0)
			{
				string text = player.friendshipData.Keys.ElementAt(random.Next(player.friendshipData.Keys.Count()));
				if (random.NextDouble() < (double)(player.friendshipData[text].Points / 250) * 0.1 && (player.spouse == null || !player.spouse.Equals(text)) && content.Load<Dictionary<string, string>>("Data\\mail").ContainsKey(text))
				{
					mailbox.Add(text);
				}
			}
			MineShaft.clearActiveMines();
			VolcanoDungeon.ClearAllLevels();
			for (int num3 = player.enchantments.Count - 1; num3 >= 0; num3--)
			{
				player.enchantments[num3].OnUnequip(player);
			}
			player.dayupdate();
			if (IsMasterGame)
			{
				player.team.sharedDailyLuck.Value = Math.Min(0.10000000149011612, (double)random.Next(-100, 101) / 1000.0);
			}
			dayOfMonth++;
			stats.DaysPlayed++;
			startedJukeboxMusic = false;
			player.dayOfMonthForSaveGame = dayOfMonth;
			player.seasonForSaveGame = Utility.getSeasonNumber(currentSeason);
			player.yearForSaveGame = year;
			player.showToolUpgradeAvailability();
			if (IsMasterGame)
			{
				queueWeddingsForToday();
				newDaySync.sendVar<NetRef<NetLongList>, NetLongList>("weddingsToday", new NetLongList(weddingsToday));
			}
			else
			{
				while (!newDaySync.isVarReady("weddingsToday"))
				{
					yield return 0;
				}
				NetLongList collection = newDaySync.waitForVar<NetRef<NetLongList>, NetLongList>("weddingsToday");
				weddingsToday = new List<long>(collection);
			}
			weddingToday = false;
			foreach (long item2 in weddingsToday)
			{
				Farmer farmer = getFarmer(item2);
				if (farmer != null && !farmer.hasCurrentOrPendingRoommate())
				{
					weddingToday = true;
					break;
				}
			}
			if (player.spouse != null && player.isEngaged() && weddingsToday.Contains(player.UniqueMultiplayerID))
			{
				Friendship friendship = player.friendshipData[player.spouse];
				if (friendship.CountdownToWedding <= 1)
				{
					friendship.Status = FriendshipStatus.Married;
					friendship.WeddingDate = new WorldDate(Date);
					prepareSpouseForWedding(player);
				}
			}
			NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>> additional_shipped_items = new NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>();
			if (IsMasterGame)
			{
				Utility.ForAllLocations(delegate(GameLocation location)
				{
					foreach (Object value in location.objects.Values)
					{
						if (value is Chest && value is Chest chest && chest.SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin)
						{
							if ((bool)player.team.useSeparateWallets)
							{
								foreach (long key in chest.separateWalletItems.Keys)
								{
									if (!additional_shipped_items.ContainsKey(key))
									{
										additional_shipped_items[key] = new NetList<Item, NetRef<Item>>();
									}
									List<Item> list5 = new List<Item>(chest.separateWalletItems[key]);
									chest.separateWalletItems[key].Clear();
									foreach (Item item3 in list5)
									{
										if (item3 != null)
										{
											additional_shipped_items[key].Add(item3);
										}
									}
								}
							}
							else
							{
								NetCollection<Item> shippingBin3 = getFarm().getShippingBin(player);
								List<Item> list6 = new List<Item>(chest.items);
								chest.items.Clear();
								foreach (Item item4 in list6)
								{
									if (item4 != null)
									{
										shippingBin3.Add(item4);
									}
								}
							}
							chest.items.Clear();
							chest.separateWalletItems.Clear();
						}
					}
				});
			}
			if (IsMasterGame)
			{
				newDaySync.sendVar<NetRef<NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>, NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>("additional_shipped_items", additional_shipped_items);
			}
			else
			{
				while (!newDaySync.isVarReady("additional_shipped_items"))
				{
					yield return 0;
				}
				additional_shipped_items = newDaySync.waitForVar<NetRef<NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>, NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>("additional_shipped_items");
			}
			if (player.team.useSeparateWallets.Value)
			{
				NetCollection<Item> shippingBin = getFarm().getShippingBin(player);
				if (additional_shipped_items.ContainsKey(player.UniqueMultiplayerID))
				{
					NetList<Item, NetRef<Item>> netList = additional_shipped_items[player.UniqueMultiplayerID];
					foreach (Item item5 in netList)
					{
						shippingBin.Add(item5);
					}
				}
			}
			newDaySync.barrier("handleMiniShippingBins");
			while (!newDaySync.isBarrierReady("handleMiniShippingBins"))
			{
				yield return 0;
			}
			NetCollection<Item> shippingBin2 = getFarm().getShippingBin(player);
			foreach (Item item6 in shippingBin2)
			{
				if (item6 != null)
				{
					player.displayedShippedItems.Add(item6);
				}
			}
			if (player.useSeparateWallets || (!player.useSeparateWallets && player.IsMainPlayer))
			{
				int num4 = 0;
				foreach (Item item7 in shippingBin2)
				{
					if (item7 == null)
					{
						continue;
					}
					int num5 = 0;
					if (item7 is Object)
					{
						num5 = (item7 as Object).sellToStorePrice(-1L) * item7.Stack;
						num4 += num5;
					}
					if (player.team.specialOrders == null)
					{
						continue;
					}
					foreach (SpecialOrder specialOrder2 in player.team.specialOrders)
					{
						if (specialOrder2.onItemShipped != null)
						{
							specialOrder2.onItemShipped(player, item7, num5);
						}
					}
				}
				player.Money += num4;
			}
			if (IsMasterGame)
			{
				if (currentSeason.Equals("winter") && dayOfMonth == 18)
				{
					GameLocation locationFromName = getLocationFromName("Submarine");
					if (locationFromName.objects.Count() >= 0)
					{
						Utility.transferPlacedObjectsFromOneLocationToAnother(locationFromName, null, new Vector2(20f, 20f), getLocationFromName("Beach"));
					}
					locationFromName = getLocationFromName("MermaidHouse");
					if (locationFromName.objects.Count() >= 0)
					{
						Utility.transferPlacedObjectsFromOneLocationToAnother(locationFromName, null, new Vector2(21f, 20f), getLocationFromName("Beach"));
					}
				}
				if (player.hasOrWillReceiveMail("pamHouseUpgrade") && !player.hasOrWillReceiveMail("transferredObjectsPamHouse"))
				{
					addMailForTomorrow("transferredObjectsPamHouse", noLetter: true);
					GameLocation locationFromName2 = getLocationFromName("Trailer");
					GameLocation locationFromName3 = getLocationFromName("Trailer_Big");
					if (locationFromName2.objects.Count() >= 0)
					{
						Utility.transferPlacedObjectsFromOneLocationToAnother(locationFromName2, locationFromName3, new Vector2(14f, 23f));
					}
				}
				if (Utility.HasAnyPlayerSeenEvent(191393) && !player.hasOrWillReceiveMail("transferredObjectsJojaMart"))
				{
					addMailForTomorrow("transferredObjectsJojaMart", noLetter: true);
					GameLocation locationFromName4 = getLocationFromName("JojaMart");
					if (locationFromName4.objects.Count() >= 0)
					{
						Utility.transferPlacedObjectsFromOneLocationToAnother(locationFromName4, null, new Vector2(89f, 51f), getLocationFromName("Town"));
					}
				}
			}
			if (player.useSeparateWallets && player.IsMainPlayer)
			{
				foreach (Farmer allFarmhand in getAllFarmhands())
				{
					if (allFarmhand.isActive() || allFarmhand.isUnclaimedFarmhand)
					{
						continue;
					}
					int num6 = 0;
					foreach (Item item8 in getFarm().getShippingBin(allFarmhand))
					{
						if (item8 == null)
						{
							continue;
						}
						int num7 = 0;
						if (item8 is Object)
						{
							num7 = (item8 as Object).sellToStorePrice(allFarmhand.UniqueMultiplayerID) * item8.Stack;
							num6 += num7;
						}
						if (player.team.specialOrders == null)
						{
							continue;
						}
						foreach (SpecialOrder specialOrder3 in player.team.specialOrders)
						{
							if (specialOrder3.onItemShipped != null)
							{
								specialOrder3.onItemShipped(player, item8, num7);
							}
						}
					}
					player.team.AddIndividualMoney(allFarmhand, num6);
					getFarm().getShippingBin(allFarmhand).Clear();
				}
			}
			List<NPC> divorceNPCs = new List<NPC>();
			if (IsMasterGame)
			{
				foreach (Farmer allFarmer2 in getAllFarmers())
				{
					if (allFarmer2.isActive() && (bool)allFarmer2.divorceTonight && allFarmer2.getSpouse() != null)
					{
						divorceNPCs.Add(allFarmer2.getSpouse());
					}
				}
			}
			newDaySync.barrier("player.dayupdate");
			while (!newDaySync.isBarrierReady("player.dayupdate"))
			{
				yield return 0;
			}
			if ((bool)player.divorceTonight)
			{
				player.doDivorce();
			}
			newDaySync.barrier("player.divorce");
			while (!newDaySync.isBarrierReady("player.divorce"))
			{
				yield return 0;
			}
			if (IsMasterGame)
			{
				foreach (NPC item9 in divorceNPCs)
				{
					if (item9.getSpouse() == null)
					{
						item9.PerformDivorce();
					}
				}
			}
			newDaySync.barrier("player.finishDivorce");
			while (!newDaySync.isBarrierReady("player.finishDivorce"))
			{
				yield return 0;
			}
			if (IsMasterGame && (bool)player.changeWalletTypeTonight)
			{
				if (player.useSeparateWallets)
				{
					ManorHouse.MergeWallets();
				}
				else
				{
					ManorHouse.SeparateWallets();
				}
			}
			newDaySync.barrier("player.wallets");
			while (!newDaySync.isBarrierReady("player.wallets"))
			{
				yield return 0;
			}
			getFarm().lastItemShipped = null;
			getFarm().getShippingBin(player).Clear();
			newDaySync.barrier("clearShipping");
			while (!newDaySync.isBarrierReady("clearShipping"))
			{
				yield return 0;
			}
			if (IsClient)
			{
				multiplayer.sendFarmhand();
				newDaySync.processMessages();
			}
			newDaySync.barrier("sendFarmhands");
			while (!newDaySync.isBarrierReady("sendFarmhands"))
			{
				yield return 0;
			}
			if (IsMasterGame)
			{
				multiplayer.saveFarmhands();
			}
			newDaySync.barrier("saveFarmhands");
			while (!newDaySync.isBarrierReady("saveFarmhands"))
			{
				yield return 0;
			}
			if (IsMasterGame && dayOfMonth >= 15 && dayOfMonth <= 17 && currentSeason.Equals("winter") && IsMasterGame && netWorldState.Value.VisitsUntilY1Guarantee >= 0)
			{
				netWorldState.Value.VisitsUntilY1Guarantee--;
			}
			if (dayOfMonth == 27 && currentSeason.Equals("spring"))
			{
				_ = year;
				_ = 1;
			}
			if (dayOfMonth == 29)
			{
				newSeason();
				if (!currentSeason.Equals("winter"))
				{
					cropsOfTheWeek = Utility.cropsOfTheWeek();
				}
				if (currentSeason.Equals("spring"))
				{
					year++;
					if (year == 2)
					{
						addKentIfNecessary();
					}
				}
				_ = year;
				_ = 3;
			}
			if (IsMasterGame && (dayOfMonth == 1 || dayOfMonth == 8 || dayOfMonth == 15 || dayOfMonth == 22))
			{
				SpecialOrder.UpdateAvailableSpecialOrders(force_refresh: true);
			}
			if (IsMasterGame)
			{
				netWorldState.Value.UpdateFromGame1();
			}
			newDaySync.barrier("date");
			while (!newDaySync.isBarrierReady("date"))
			{
				yield return 0;
			}
			if (IsMasterGame)
			{
				for (int j = 0; j < player.team.specialOrders.Count; j++)
				{
					SpecialOrder specialOrder = player.team.specialOrders[j];
					if ((SpecialOrder.QuestState)specialOrder.questState != SpecialOrder.QuestState.Complete && specialOrder.GetDaysLeft() <= 0)
					{
						specialOrder.OnFail();
						player.team.specialOrders.RemoveAt(j);
						j--;
					}
				}
			}
			newDaySync.barrier("processOrders");
			while (!newDaySync.isBarrierReady("processOrders"))
			{
				yield return 0;
			}
			List<string> list2 = new List<string>(player.team.mailToRemoveOvernight);
			List<int> list3 = new List<int>(player.team.itemsToRemoveOvernight);
			if (IsMasterGame)
			{
				foreach (string item10 in player.team.specialRulesRemovedToday)
				{
					SpecialOrder.RemoveSpecialRuleAtEndOfDay(item10);
				}
			}
			player.team.specialRulesRemovedToday.Clear();
			foreach (int item11 in list3)
			{
				if (IsMasterGame)
				{
					game1._PerformRemoveNormalItemFromWorldOvernight(item11);
					foreach (Farmer allFarmer3 in getAllFarmers())
					{
						game1._PerformRemoveNormalItemFromFarmerOvernight(allFarmer3, item11);
					}
				}
				else
				{
					game1._PerformRemoveNormalItemFromFarmerOvernight(player, item11);
				}
			}
			foreach (string item12 in list2)
			{
				if (IsMasterGame)
				{
					foreach (Farmer allFarmer4 in getAllFarmers())
					{
						allFarmer4.RemoveMail(item12, allFarmer4 == MasterPlayer);
					}
				}
				else
				{
					player.RemoveMail(item12);
				}
			}
			newDaySync.barrier("removeItemsFromWorld");
			while (!newDaySync.isBarrierReady("removeItemsFromWorld"))
			{
				yield return 0;
			}
			if (IsMasterGame)
			{
				player.team.itemsToRemoveOvernight.Clear();
				player.team.mailToRemoveOvernight.Clear();
			}
			if (content.Load<Dictionary<string, string>>("Data\\mail").ContainsKey(currentSeason + "_" + dayOfMonth + "_" + year))
			{
				mailbox.Add(currentSeason + "_" + dayOfMonth + "_" + year);
			}
			else if (content.Load<Dictionary<string, string>>("Data\\mail").ContainsKey(currentSeason + "_" + dayOfMonth))
			{
				mailbox.Add(currentSeason + "_" + dayOfMonth);
			}
			if (IsMasterGame && player.team.toggleMineShrineOvernight.Value)
			{
				player.team.toggleMineShrineOvernight.Value = false;
				player.team.mineShrineActivated.Value = !player.team.mineShrineActivated.Value;
				if (player.team.mineShrineActivated.Value)
				{
					netWorldState.Value.MinesDifficulty++;
				}
				else
				{
					netWorldState.Value.MinesDifficulty--;
				}
			}
			if (IsMasterGame)
			{
				if (!player.team.SpecialOrderRuleActive("MINE_HARD") && netWorldState.Value.MinesDifficulty > 1)
				{
					netWorldState.Value.MinesDifficulty = 1;
				}
				if (!player.team.SpecialOrderRuleActive("SC_HARD") && netWorldState.Value.SkullCavesDifficulty > 0)
				{
					netWorldState.Value.SkullCavesDifficulty = 0;
				}
			}
			RefreshQuestOfTheDay();
			weatherForTomorrow = getWeatherModificationsForDate(Date, weatherForTomorrow);
			if (weddingToday)
			{
				weatherForTomorrow = 6;
			}
			wasRainingYesterday = isRaining || isLightning;
			if (weatherForTomorrow == 1 || weatherForTomorrow == 3)
			{
				isRaining = true;
			}
			if (weatherForTomorrow == 3)
			{
				isLightning = true;
			}
			if (weatherForTomorrow == 0 || weatherForTomorrow == 2 || weatherForTomorrow == 4 || weatherForTomorrow == 5 || weatherForTomorrow == 6)
			{
				isRaining = false;
				isLightning = false;
				isSnowing = false;
				if (weatherForTomorrow == 5)
				{
					isSnowing = true;
				}
			}
			if (!isRaining && !isLightning)
			{
				currentSongIndex++;
				if (currentSongIndex > 3 || dayOfMonth == 1)
				{
					currentSongIndex = 1;
				}
			}
			if (IsMasterGame)
			{
				game1.SetOtherLocationWeatherForTomorrow(random);
			}
			if ((isRaining || isSnowing || isLightning) && currentLocation.GetLocationContext() == GameLocation.LocationContext.Default)
			{
				changeMusicTrack("none");
			}
			else if (weatherForTomorrow == 4 && weatherForTomorrow == 6)
			{
				changeMusicTrack("none");
			}
			ClearDebrisWeather(debrisWeather);
			baseDebrisWeatherCount = 0;
			isDebrisWeather = false;
			if (weatherForTomorrow == 2)
			{
				populateDebrisWeatherArray(debrisWeather);
			}
			if (currentSeason.Equals("summer"))
			{
				chanceToRainTomorrow = ((dayOfMonth > 1) ? (0.12 + (double)((float)dayOfMonth * 0.003f)) : 0.0);
			}
			else if (currentSeason.Equals("winter"))
			{
				chanceToRainTomorrow = 0.63;
			}
			else
			{
				chanceToRainTomorrow = 0.183;
			}
			if (random.NextDouble() < chanceToRainTomorrow)
			{
				weatherForTomorrow = 1;
				if ((currentSeason.Equals("summer") && random.NextDouble() < 0.85) || (!currentSeason.Equals("winter") && random.NextDouble() < 0.25 && dayOfMonth > 2 && stats.DaysPlayed > 27))
				{
					weatherForTomorrow = 3;
				}
				if (currentSeason.Equals("winter"))
				{
					weatherForTomorrow = 5;
				}
			}
			else if (stats.DaysPlayed > 2 && ((currentSeason.Equals("spring") && random.NextDouble() < 0.2) || (currentSeason.Equals("fall") && random.NextDouble() < 0.6)) && !weddingToday)
			{
				weatherForTomorrow = 2;
			}
			else
			{
				weatherForTomorrow = 0;
			}
			if (Utility.isFestivalDay(dayOfMonth + 1, currentSeason))
			{
				weatherForTomorrow = 4;
			}
			if (stats.DaysPlayed == 2)
			{
				weatherForTomorrow = 1;
			}
			if (IsMasterGame)
			{
				netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Default).weatherForTomorrow.Value = weatherForTomorrow;
				netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Default).isRaining.Value = isRaining;
				netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Default).isSnowing.Value = isSnowing;
				netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Default).isLightning.Value = isLightning;
				netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Default).isDebrisWeather.Value = isDebrisWeather;
			}
			foreach (NPC allCharacter2 in Utility.getAllCharacters())
			{
				player.mailReceived.Remove(allCharacter2.Name);
				player.mailReceived.Remove(allCharacter2.Name + "Cooking");
				allCharacter2.drawOffset.Value = Vector2.Zero;
			}
			FarmAnimal.reservedGrass.Clear();
			if (IsMasterGame)
			{
				NPC.hasSomeoneWateredCrops = (NPC.hasSomeoneFedThePet = (NPC.hasSomeoneFedTheAnimals = (NPC.hasSomeoneRepairedTheFences = false)));
				foreach (GameLocation location2 in locations)
				{
					location2.ResetCharacterDialogues();
					location2.DayUpdate(dayOfMonth);
				}
				UpdateHorseOwnership();
				foreach (NPC allCharacter3 in Utility.getAllCharacters())
				{
					allCharacter3.islandScheduleName.Value = null;
					allCharacter3.currentScheduleDelay = 0f;
				}
				foreach (NPC allCharacter4 in Utility.getAllCharacters())
				{
					allCharacter4.dayUpdate(dayOfMonth);
				}
				IslandSouth.SetupIslandSchedules();
				HashSet<NPC> purchased_item_npcs = new HashSet<NPC>();
				UpdateShopPlayerItemInventory("SeedShop", purchased_item_npcs);
				UpdateShopPlayerItemInventory("FishShop", purchased_item_npcs);
			}
			if (IsMasterGame && (bool)netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Island).isRaining)
			{
				Vector2 tile_position = new Vector2(0f, 0f);
				IslandLocation islandLocation = null;
				List<int> list4 = new List<int>();
				for (int k = 0; k < 4; k++)
				{
					list4.Add(k);
				}
				Utility.Shuffle(new Random((int)uniqueIDForThisGame), list4);
				switch (list4[currentGemBirdIndex])
				{
				case 0:
					islandLocation = getLocationFromName("IslandSouth") as IslandLocation;
					tile_position = new Vector2(10f, 30f);
					break;
				case 1:
					islandLocation = getLocationFromName("IslandNorth") as IslandLocation;
					tile_position = new Vector2(56f, 56f);
					break;
				case 2:
					islandLocation = getLocationFromName("Islandwest") as IslandLocation;
					tile_position = new Vector2(53f, 51f);
					break;
				case 3:
					islandLocation = getLocationFromName("IslandEast") as IslandLocation;
					tile_position = new Vector2(21f, 35f);
					break;
				}
				currentGemBirdIndex = (currentGemBirdIndex + 1) % 4;
				if (islandLocation != null)
				{
					islandLocation.locationGemBird.Value = new IslandGemBird(tile_position, IslandGemBird.GetBirdTypeForLocation(islandLocation.Name));
				}
			}
			if (IsMasterGame)
			{
				foreach (GameLocation location3 in locations)
				{
					if (!IsRainingHere(location3) || !location3.IsOutdoors)
					{
						continue;
					}
					foreach (KeyValuePair<Vector2, TerrainFeature> pair in location3.terrainFeatures.Pairs)
					{
						if (pair.Value is HoeDirt && (int)((HoeDirt)pair.Value).state != 2)
						{
							((HoeDirt)pair.Value).state.Value = 1;
						}
					}
				}
				GameLocation locationFromName5 = getLocationFromName("Farm");
				if (IsRainingHere(locationFromName5))
				{
					(locationFromName5 as Farm).petBowlWatered.Value = true;
				}
			}
			if (player.currentUpgrade != null)
			{
				player.currentUpgrade.daysLeftTillUpgradeDone--;
				if (getLocationFromName("Farm").objects.ContainsKey(new Vector2(player.currentUpgrade.positionOfCarpenter.X / 64f, player.currentUpgrade.positionOfCarpenter.Y / 64f)))
				{
					getLocationFromName("Farm").objects.Remove(new Vector2(player.currentUpgrade.positionOfCarpenter.X / 64f, player.currentUpgrade.positionOfCarpenter.Y / 64f));
				}
				if (player.currentUpgrade.daysLeftTillUpgradeDone == 0)
				{
					switch (player.currentUpgrade.whichBuilding)
					{
					case "House":
						player.HouseUpgradeLevel++;
						currentHouseTexture = content.Load<Texture2D>("Buildings\\House" + player.HouseUpgradeLevel);
						break;
					case "Coop":
						player.CoopUpgradeLevel++;
						currentCoopTexture = content.Load<Texture2D>("BuildingUpgrades\\Coop" + player.CoopUpgradeLevel);
						break;
					case "Barn":
						player.BarnUpgradeLevel++;
						currentBarnTexture = content.Load<Texture2D>("BuildingUpgrades\\Barn" + player.BarnUpgradeLevel);
						break;
					case "Greenhouse":
						player.hasGreenhouse = true;
						greenhouseTexture = content.Load<Texture2D>("BuildingUpgrades\\Greenhouse");
						break;
					}
					stats.checkForBuildingUpgradeAchievements();
					removeFrontLayerForFarmBuildings();
					addNewFarmBuildingMaps();
					player.currentUpgrade = null;
					changeInvisibility("Robin", invisibility: false);
				}
				else if (player.currentUpgrade.daysLeftTillUpgradeDone == 3)
				{
					changeInvisibility("Robin", invisibility: true);
				}
			}
			newDaySync.barrier("buildingUpgrades");
			while (!newDaySync.isBarrierReady("buildingUpgrades"))
			{
				yield return 0;
			}
			stats.AverageBedtime = (uint)timeOfDay;
			timeOfDay = 600;
			newDay = false;
			if (IsMasterGame)
			{
				netWorldState.Value.UpdateFromGame1();
			}
			if (player.currentLocation != null)
			{
				player.currentLocation.resetForPlayerEntry();
				BedFurniture.ApplyWakeUpPosition(player);
				forceSnapOnNextViewportUpdate = true;
				UpdateViewPort(overrideFreeze: false, new Point(player.getStandingX(), player.getStandingY()));
				previousViewportPosition = new Vector2(viewport.X, viewport.Y);
			}
			player.sleptInTemporaryBed.Value = false;
			_ = currentWallpaper;
			wallpaperPrice = random.Next(75, 500) + player.HouseUpgradeLevel * 100;
			wallpaperPrice -= wallpaperPrice % 5;
			_ = currentFloor;
			floorPrice = random.Next(75, 500) + player.HouseUpgradeLevel * 100;
			floorPrice -= floorPrice % 5;
			updateWeatherIcon();
			freezeControls = false;
			if (stats.DaysPlayed > 1 || !IsMasterGame)
			{
				farmEvent = null;
				if (IsMasterGame)
				{
					farmEvent = Utility.pickFarmEvent();
					newDaySync.sendVar<NetRef<FarmEvent>, FarmEvent>("farmEvent", farmEvent);
				}
				else
				{
					while (!newDaySync.isVarReady("farmEvent"))
					{
						yield return 0;
					}
					farmEvent = newDaySync.waitForVar<NetRef<FarmEvent>, FarmEvent>("farmEvent");
				}
				if (farmEvent == null)
				{
					farmEvent = Utility.pickPersonalFarmEvent();
				}
				if (farmEvent != null && farmEvent.setUp())
				{
					farmEvent = null;
				}
			}
			if (farmEvent == null)
			{
				RemoveDeliveredMailForTomorrow();
			}
			if (player.team.newLostAndFoundItems.Value)
			{
				morningQueue.Enqueue(delegate
				{
					showGlobalMessage(content.LoadString("Strings\\StringsFromCSFiles:NewLostAndFoundItems"));
				});
			}
			newDaySync.barrier("mail");
			while (!newDaySync.isBarrierReady("mail"))
			{
				yield return 0;
			}
			if (IsMasterGame)
			{
				player.team.newLostAndFoundItems.Value = false;
			}
			foreach (Building building2 in getFarm().buildings)
			{
				if ((int)building2.daysOfConstructionLeft <= 0 && building2.indoors.Value is Cabin)
				{
					player.slotCanHost = true;
					break;
				}
			}
			if (Utility.percentGameComplete() >= 1f)
			{
				player.team.farmPerfect.Value = true;
			}
			newDaySync.barrier("checkcompletion");
			while (!newDaySync.isBarrierReady("checkcompletion"))
			{
				yield return 0;
			}
			UpdateFarmPerfection();
			if (farmEvent == null)
			{
				handlePostFarmEventActions();
				showEndOfNightStuff();
			}
			if (server != null)
			{
				server.updateLobbyData();
			}
		}

		public virtual void SetOtherLocationWeatherForTomorrow(Random random)
		{
			netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Island).InitializeDayWeather();
			if (netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Island).weatherForTomorrow.Value == 1)
			{
				netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Island).isRaining.Value = true;
			}
			netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Island).weatherForTomorrow.Value = 0;
			if (random.NextDouble() < 0.24)
			{
				netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Island).weatherForTomorrow.Value = 1;
			}
			if (!Utility.doesAnyFarmerHaveOrWillReceiveMail("Visited_Island"))
			{
				netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Island).weatherForTomorrow.Value = 0;
			}
		}

		public static void UpdateFarmPerfection()
		{
			if (MasterPlayer.mailReceived.Contains("Farm_Eternal") || (!MasterPlayer.hasCompletedCommunityCenter() && !Utility.hasFinishedJojaRoute()) || !player.team.farmPerfect.Value)
			{
				return;
			}
			addMorningFluffFunction(delegate
			{
				changeMusicTrack("none");
				if (IsMasterGame)
				{
					multiplayer.globalChatInfoMessageEvenInSinglePlayer("Eternal1");
				}
				playSound("discoverMineral");
				if (IsMasterGame)
				{
					DelayedAction.functionAfterDelay(delegate
					{
						multiplayer.globalChatInfoMessageEvenInSinglePlayer("Eternal2", MasterPlayer.farmName);
					}, 4000);
				}
				player.mailReceived.Add("Farm_Eternal");
				DelayedAction.functionAfterDelay(delegate
				{
					playSound("thunder_small");
					if (IsMultiplayer)
					{
						if (IsMasterGame)
						{
							multiplayer.globalChatInfoMessage("Eternal3");
						}
					}
					else
					{
						showGlobalMessage(content.LoadString("Strings\\UI:Chat_Eternal3"));
					}
				}, 12000);
			});
		}

		public static bool IsRainingHere(GameLocation location = null)
		{
			if (netWorldState == null)
			{
				return false;
			}
			if (location == null)
			{
				location = currentLocation;
			}
			if (location == null)
			{
				return false;
			}
			return netWorldState.Value.GetWeatherForLocation(location.GetLocationContext()).isRaining;
		}

		public static bool IsLightningHere(GameLocation location = null)
		{
			if (netWorldState == null)
			{
				return false;
			}
			if (location == null)
			{
				location = currentLocation;
			}
			if (location == null)
			{
				return false;
			}
			return netWorldState.Value.GetWeatherForLocation(location.GetLocationContext()).isLightning;
		}

		public static bool IsSnowingHere(GameLocation location = null)
		{
			if (netWorldState == null)
			{
				return false;
			}
			if (location == null)
			{
				location = currentLocation;
			}
			if (location == null)
			{
				return false;
			}
			return netWorldState.Value.GetWeatherForLocation(location.GetLocationContext()).isSnowing;
		}

		public static bool IsDebrisWeatherHere(GameLocation location = null)
		{
			if (netWorldState == null)
			{
				return false;
			}
			if (location == null)
			{
				location = currentLocation;
			}
			if (location == null)
			{
				return false;
			}
			return netWorldState.Value.GetWeatherForLocation(location.GetLocationContext()).isDebrisWeather;
		}

		public static int getWeatherModificationsForDate(WorldDate date, int default_weather)
		{
			int result = default_weather;
			int num = date.TotalDays - Date.TotalDays;
			if (date.DayOfMonth == 1 || stats.DaysPlayed + num <= 4)
			{
				result = 0;
			}
			if (stats.DaysPlayed + num == 3)
			{
				result = 1;
			}
			if (date.Season.Equals("summer") && date.DayOfMonth % 13 == 0)
			{
				result = 3;
			}
			if (Utility.isFestivalDay(date.DayOfMonth, date.Season))
			{
				result = 4;
			}
			if (date.Season.Equals("winter") && date.DayOfMonth >= 14 && date.DayOfMonth <= 16)
			{
				result = 0;
			}
			return result;
		}

		public static void UpdateShopPlayerItemInventory(string location_name, HashSet<NPC> purchased_item_npcs)
		{
			GameLocation locationFromName = getLocationFromName(location_name);
			if (locationFromName == null)
			{
				return;
			}
			ShopLocation shopLocation = locationFromName as ShopLocation;
			for (int num = shopLocation.itemsFromPlayerToSell.Count - 1; num >= 0; num--)
			{
				for (int i = 0; i < shopLocation.itemsFromPlayerToSell[num].Stack; i++)
				{
					if (random.NextDouble() < 0.04 && shopLocation.itemsFromPlayerToSell[num] is Object && (int)(shopLocation.itemsFromPlayerToSell[num] as Object).edibility != -300)
					{
						NPC randomTownNPC = Utility.getRandomTownNPC();
						if (randomTownNPC.Age != 2 && randomTownNPC.getSpouse() == null)
						{
							if (!purchased_item_npcs.Contains(randomTownNPC))
							{
								randomTownNPC.addExtraDialogues(shopLocation.getPurchasedItemDialogueForNPC(shopLocation.itemsFromPlayerToSell[num] as Object, randomTownNPC));
								purchased_item_npcs.Add(randomTownNPC);
							}
							shopLocation.itemsFromPlayerToSell[num].Stack--;
						}
					}
					else if (random.NextDouble() < 0.15)
					{
						shopLocation.itemsFromPlayerToSell[num].Stack--;
					}
					if (shopLocation.itemsFromPlayerToSell[num].Stack <= 0)
					{
						shopLocation.itemsFromPlayerToSell.RemoveAt(num);
						break;
					}
				}
			}
		}

		private static void handlePostFarmEventActions()
		{
			foreach (GameLocation location in locations)
			{
				if (location is BuildableGameLocation)
				{
					foreach (Building building in (location as BuildableGameLocation).buildings)
					{
						if (building.indoors.Value == null)
						{
							continue;
						}
						foreach (Action postFarmEventOvernightAction in building.indoors.Value.postFarmEventOvernightActions)
						{
							postFarmEventOvernightAction();
						}
						building.indoors.Value.postFarmEventOvernightActions.Clear();
					}
				}
				foreach (Action postFarmEventOvernightAction2 in location.postFarmEventOvernightActions)
				{
					postFarmEventOvernightAction2();
				}
				location.postFarmEventOvernightActions.Clear();
			}
			if (IsMasterGame)
			{
				Mountain mountain = getLocationFromName("Mountain") as Mountain;
				mountain.ApplyTreehouseIfNecessary();
				if (mountain.treehouseDoorDirty)
				{
					mountain.treehouseDoorDirty = false;
					NPC.populateRoutesFromLocationToLocationList();
				}
			}
		}

		public static void ReceiveMailForTomorrow(string mail_to_transfer = null)
		{
			foreach (string item in player.mailForTomorrow)
			{
				if (item == null)
				{
					continue;
				}
				string text = item.Replace("%&NL&%", "");
				if (mail_to_transfer != null && mail_to_transfer != item && mail_to_transfer != text)
				{
					continue;
				}
				mailDeliveredFromMailForTomorrow.Add(item);
				if (item.Contains("%&NL&%"))
				{
					if (!player.mailReceived.Contains(text))
					{
						player.mailReceived.Add(text);
					}
				}
				else
				{
					mailbox.Add(item);
				}
			}
		}

		public static void RemoveDeliveredMailForTomorrow()
		{
			ReceiveMailForTomorrow("abandonedJojaMartAccessible");
			foreach (string item in mailDeliveredFromMailForTomorrow)
			{
				if (player.mailForTomorrow.Contains(item))
				{
					player.mailForTomorrow.Remove(item);
				}
			}
			mailDeliveredFromMailForTomorrow.Clear();
		}

		public static void queueWeddingsForToday()
		{
			weddingsToday.Clear();
			weddingToday = false;
			if (!canHaveWeddingOnDay(dayOfMonth, currentSeason))
			{
				return;
			}
			foreach (Farmer item in from farmer in getOnlineFarmers()
				orderby farmer.UniqueMultiplayerID
				select farmer)
			{
				if (item.spouse != null && item.isEngaged())
				{
					Friendship friendship = item.friendshipData[item.spouse];
					if (friendship.CountdownToWedding <= 1)
					{
						weddingsToday.Add(item.UniqueMultiplayerID);
					}
				}
				if (!item.team.IsEngaged(item.UniqueMultiplayerID))
				{
					continue;
				}
				long? spouse = item.team.GetSpouse(item.UniqueMultiplayerID);
				if (!spouse.HasValue || weddingsToday.Contains(spouse.Value))
				{
					continue;
				}
				Farmer farmerMaybeOffline = getFarmerMaybeOffline(spouse.Value);
				if (farmerMaybeOffline != null && getOnlineFarmers().Contains(farmerMaybeOffline) && getOnlineFarmers().Contains(item))
				{
					Friendship friendship2 = player.team.GetFriendship(item.UniqueMultiplayerID, spouse.Value);
					if (friendship2.CountdownToWedding <= 1)
					{
						weddingsToday.Add(item.UniqueMultiplayerID);
					}
				}
			}
		}

		public static bool PollForEndOfNewDaySync()
		{
			if (!IsMultiplayer)
			{
				newDaySync = null;
				currentLocation.resetForPlayerEntry();
				return true;
			}
			if (newDaySync.readyForFinish())
			{
				if (IsMasterGame && newDaySync != null && !newDaySync.hasFinished())
				{
					newDaySync.finish();
				}
				if (newDaySync != null && newDaySync.hasFinished())
				{
					newDaySync = null;
					currentLocation.resetForPlayerEntry();
					return true;
				}
			}
			return false;
		}

		public static void FinishNewDaySync()
		{
			if (IsMasterGame && newDaySync != null && !newDaySync.hasFinished())
			{
				newDaySync.finish();
			}
			newDaySync = null;
		}

		public static void updateWeatherIcon()
		{
			if (IsSnowingHere())
			{
				weatherIcon = 7;
			}
			else if (IsRainingHere())
			{
				weatherIcon = 4;
			}
			else if (IsDebrisWeatherHere() && currentSeason.Equals("spring"))
			{
				weatherIcon = 3;
			}
			else if (IsDebrisWeatherHere() && currentSeason.Equals("fall"))
			{
				weatherIcon = 6;
			}
			else if (IsDebrisWeatherHere() && currentSeason.Equals("winter"))
			{
				weatherIcon = 7;
			}
			else if (weddingToday)
			{
				weatherIcon = 0;
			}
			else
			{
				weatherIcon = 2;
			}
			if (IsLightningHere())
			{
				weatherIcon = 5;
			}
			if (Utility.isFestivalDay(dayOfMonth, currentSeason))
			{
				weatherIcon = 1;
			}
		}

		public static void showEndOfNightStuff()
		{
			hooks.OnGame1_ShowEndOfNightStuff(delegate
			{
				if (!(activeClickableMenu is TitleMenu))
				{
					bool flag = false;
					if (player.displayedShippedItems.Count > 0)
					{
						endOfNightMenus.Push(new ShippingMenu(player.displayedShippedItems));
						player.displayedShippedItems.Clear();
						flag = true;
					}
					bool flag2 = false;
					if (player.newLevels.Count > 0 && !flag)
					{
						endOfNightMenus.Push(new SaveGameMenu());
					}
					for (int num = player.newLevels.Count - 1; num >= 0; num--)
					{
						bool flag3 = player.newLevels.Count == 1;
						endOfNightMenus.Push(new LevelUpMenu(player.newLevels[num].X, player.newLevels[num].Y));
						flag2 = true;
					}
					if (flag2)
					{
						playSound("newRecord");
					}
					if (client == null || !client.timedOut)
					{
						if (endOfNightMenus.Count > 0)
						{
							showingEndOfNightStuff = true;
							activeClickableMenu = endOfNightMenus.Pop();
						}
						else
						{
							showingEndOfNightStuff = true;
							activeClickableMenu = new SaveGameMenu();
						}
					}
				}
			});
		}

		private static void updateWallpaperInSeedShop()
		{
			GameLocation locationFromName = getLocationFromName("SeedShop");
			for (int i = 9; i < 12; i++)
			{
				locationFromName.Map.GetLayer("Back").Tiles[i, 15] = new StaticTile(locationFromName.Map.GetLayer("Back"), locationFromName.Map.GetTileSheet("Wallpapers"), BlendMode.Alpha, currentWallpaper);
				locationFromName.Map.GetLayer("Back").Tiles[i, 16] = new StaticTile(locationFromName.Map.GetLayer("Back"), locationFromName.Map.GetTileSheet("Wallpapers"), BlendMode.Alpha, currentWallpaper);
			}
		}

		public static void setGraphicsForSeason()
		{
			foreach (GameLocation location in locations)
			{
				string seasonForLocation = GetSeasonForLocation(location);
				location.seasonUpdate(seasonForLocation, onLoad: true);
				location.updateSeasonalTileSheets();
				if (!location.IsOutdoors)
				{
					continue;
				}
				if (seasonForLocation.Equals("spring"))
				{
					foreach (KeyValuePair<Vector2, Object> pair in location.Objects.Pairs)
					{
						if ((pair.Value.Name.Contains("Stump") || pair.Value.Name.Contains("Boulder") || pair.Value.Name.Equals("Stick") || pair.Value.Name.Equals("Stone")) && pair.Value.ParentSheetIndex >= 378 && pair.Value.ParentSheetIndex <= 391)
						{
							pair.Value.ParentSheetIndex -= 376;
						}
					}
					eveningColor = new Color(255, 255, 0);
				}
				else if (seasonForLocation.Equals("summer"))
				{
					foreach (KeyValuePair<Vector2, Object> pair2 in location.Objects.Pairs)
					{
						if (pair2.Value.Name.Contains("Weed") && pair2.Value.ParentSheetIndex != 882 && pair2.Value.ParentSheetIndex != 883 && pair2.Value.ParentSheetIndex != 884)
						{
							if ((int)pair2.Value.parentSheetIndex == 792)
							{
								pair2.Value.ParentSheetIndex++;
							}
							else if (random.NextDouble() < 0.3)
							{
								pair2.Value.ParentSheetIndex = 676;
							}
							else if (random.NextDouble() < 0.3)
							{
								pair2.Value.ParentSheetIndex = 677;
							}
						}
					}
					eveningColor = new Color(255, 255, 0);
				}
				else if (seasonForLocation.Equals("fall"))
				{
					foreach (KeyValuePair<Vector2, Object> pair3 in location.Objects.Pairs)
					{
						if (pair3.Value.Name.Contains("Weed") && pair3.Value.ParentSheetIndex != 882 && pair3.Value.ParentSheetIndex != 883 && pair3.Value.ParentSheetIndex != 884)
						{
							if ((int)pair3.Value.parentSheetIndex == 793)
							{
								pair3.Value.ParentSheetIndex++;
							}
							else if (random.NextDouble() < 0.5)
							{
								pair3.Value.ParentSheetIndex = 678;
							}
							else
							{
								pair3.Value.ParentSheetIndex = 679;
							}
						}
					}
					eveningColor = new Color(255, 255, 0);
					foreach (WeatherDebris item in debrisWeather)
					{
						item.which = 2;
					}
				}
				else
				{
					if (!seasonForLocation.Equals("winter"))
					{
						continue;
					}
					for (int num = location.Objects.Count() - 1; num >= 0; num--)
					{
						Object @object = location.Objects[location.Objects.Keys.ElementAt(num)];
						if (@object.Name.Contains("Weed"))
						{
							if (@object.ParentSheetIndex != 882 && @object.ParentSheetIndex != 883 && @object.ParentSheetIndex != 884)
							{
								location.Objects.Remove(location.Objects.Keys.ElementAt(num));
							}
						}
						else if (((!@object.Name.Contains("Stump") && !@object.Name.Contains("Boulder") && !@object.Name.Equals("Stick") && !@object.Name.Equals("Stone")) || @object.ParentSheetIndex > 100) && location.IsOutdoors && !@object.isHoedirt)
						{
							@object.name.Equals("HoeDirt");
						}
					}
					foreach (WeatherDebris item2 in debrisWeather)
					{
						item2.which = 3;
					}
					eveningColor = new Color(245, 225, 170);
				}
			}
		}

		private static void updateFloorInSeedShop()
		{
			GameLocation locationFromName = getLocationFromName("SeedShop");
			for (int i = 9; i < 12; i++)
			{
				locationFromName.Map.GetLayer("Back").Tiles[i, 17] = new StaticTile(locationFromName.Map.GetLayer("Back"), locationFromName.Map.GetTileSheet("Floors"), BlendMode.Alpha, currentFloor);
				locationFromName.Map.GetLayer("Back").Tiles[i, 18] = new StaticTile(locationFromName.Map.GetLayer("Back"), locationFromName.Map.GetTileSheet("Floors"), BlendMode.Alpha, currentFloor);
			}
		}

		public static void pauseThenMessage(int millisecondsPause, string message, bool showProgressBar)
		{
			messageAfterPause = message;
			pauseTime = millisecondsPause;
			progressBar = showProgressBar;
		}

		public static void updateWallpaperInFarmHouse(int wallpaper)
		{
			GameLocation locationFromName = getLocationFromName("FarmHouse");
			locationFromName.Map.Properties.TryGetValue("Wallpaper", out var value);
			if (value == null)
			{
				return;
			}
			string[] array = value.ToString().Split(' ');
			for (int i = 0; i < array.Length; i += 4)
			{
				int num = Convert.ToInt32(array[i]);
				int num2 = Convert.ToInt32(array[i + 1]);
				int num3 = Convert.ToInt32(array[i + 2]);
				int num4 = Convert.ToInt32(array[i + 3]);
				for (int j = num; j < num + num3; j++)
				{
					for (int k = num2; k < num2 + num4; k++)
					{
						locationFromName.Map.GetLayer("Back").Tiles[j, k] = new StaticTile(locationFromName.Map.GetLayer("Back"), locationFromName.Map.GetTileSheet("Wallpapers"), BlendMode.Alpha, wallpaper);
					}
				}
			}
		}

		public static void updateFloorInFarmHouse(int floor)
		{
			GameLocation locationFromName = getLocationFromName("FarmHouse");
			locationFromName.Map.Properties.TryGetValue("Floor", out var value);
			if (value == null)
			{
				return;
			}
			string[] array = value.ToString().Split(' ');
			for (int i = 0; i < array.Length; i += 4)
			{
				int num = Convert.ToInt32(array[i]);
				int num2 = Convert.ToInt32(array[i + 1]);
				int num3 = Convert.ToInt32(array[i + 2]);
				int num4 = Convert.ToInt32(array[i + 3]);
				for (int j = num; j < num + num3; j++)
				{
					for (int k = num2; k < num2 + num4; k++)
					{
						locationFromName.Map.GetLayer("Back").Tiles[j, k] = new StaticTile(locationFromName.Map.GetLayer("Back"), locationFromName.Map.GetTileSheet("Floors"), BlendMode.Alpha, floor);
					}
				}
			}
		}

		public static bool IsVisitingIslandToday(string npc_name)
		{
			return netWorldState.Value.IslandVisitors.ContainsKey(npc_name);
		}

		public static bool shouldTimePass(bool ignore_multiplayer = false)
		{
			if (isFestival())
			{
				return false;
			}
			if (CurrentEvent != null && CurrentEvent.isWedding)
			{
				return false;
			}
			if (farmEvent != null)
			{
				return false;
			}
			if (IsMultiplayer && !ignore_multiplayer)
			{
				return !netWorldState.Value.IsTimePaused;
			}
			if (paused || freezeControls || overlayMenu != null || isTimePaused)
			{
				return false;
			}
			if (eventUp)
			{
				return false;
			}
			if (activeClickableMenu != null && !(activeClickableMenu is BobberBar))
			{
				return false;
			}
			if (!player.CanMove && !player.UsingTool)
			{
				return player.forceTimePass;
			}
			return true;
		}

		public static Farmer getPlayerOrEventFarmer()
		{
			if (eventUp && CurrentEvent != null && !CurrentEvent.isFestival && CurrentEvent.farmer != null)
			{
				return CurrentEvent.farmer;
			}
			return player;
		}

		public static void UpdateViewPort(bool overrideFreeze, Point centerPoint)
		{
			previousViewportPosition.X = viewport.X;
			previousViewportPosition.Y = viewport.Y;
			Farmer playerOrEventFarmer = getPlayerOrEventFarmer();
			if (currentLocation == null)
			{
				return;
			}
			if (!viewportFreeze || overrideFreeze)
			{
				if (!overrideFreeze && centerPoint.X == playerOrEventFarmer.getStandingX() && centerPoint.Y == playerOrEventFarmer.getStandingY())
				{
					SetCurrentViewportTargetToCenterOnPlayer();
				}
				else
				{
					currentViewportTarget.X = centerPoint.X - viewport.Width / 2;
					currentViewportTarget.Y = centerPoint.Y - viewport.Height / 2;
				}
			}
			if (currentLocation.forceViewportPlayerFollow)
			{
				currentViewportTarget.X = playerOrEventFarmer.Position.X - (float)(viewport.Width / 2);
				currentViewportTarget.Y = playerOrEventFarmer.Position.Y - (float)(viewport.Height / 2);
			}
			bool flag = false;
			if (forceSnapOnNextViewportUpdate)
			{
				flag = true;
				forceSnapOnNextViewportUpdate = false;
			}
			if (currentViewportTarget.X == -2.1474836E+09f || (viewportFreeze && !overrideFreeze))
			{
				return;
			}
			int num = (int)(currentViewportTarget.X - (float)viewport.X);
			if ((num == 0 || Math.Abs(num) > 128) && !PinchZoom.Instance.justPinchZoomed)
			{
				viewportPositionLerp.X = currentViewportTarget.X;
			}
			else
			{
				viewportPositionLerp.X += (float)num * playerOrEventFarmer.getMovementSpeed() * 0.03f;
			}
			num = (int)(currentViewportTarget.Y - (float)viewport.Y);
			if ((num == 0 || Math.Abs(num) > 128) && !PinchZoom.Instance.justPinchZoomed)
			{
				viewportPositionLerp.Y = (int)currentViewportTarget.Y;
			}
			else
			{
				viewportPositionLerp.Y += (float)num * playerOrEventFarmer.getMovementSpeed() * 0.03f;
			}
			if (PinchZoom.Instance.justPinchZoomed)
			{
				float num2 = Vector2.Distance(currentViewportTarget, new Vector2(viewport.X, viewport.Y));
				if (num2 < 64f)
				{
					PinchZoom.Instance.justPinchZoomed = false;
				}
			}
			if (flag)
			{
				viewportPositionLerp.X = (int)currentViewportTarget.X;
				viewportPositionLerp.Y = (int)currentViewportTarget.Y;
			}
			viewport.X = (int)viewportPositionLerp.X;
			viewport.Y = (int)viewportPositionLerp.Y;
		}

		private void UpdateCharacters(GameTime time)
		{
			if (CurrentEvent != null && CurrentEvent.farmer != null && CurrentEvent.farmer != player)
			{
				CurrentEvent.farmer.Update(time, currentLocation);
			}
			player.Update(time, currentLocation);
			foreach (KeyValuePair<long, Farmer> otherFarmer in otherFarmers)
			{
				if (otherFarmer.Key != player.UniqueMultiplayerID)
				{
					otherFarmer.Value.UpdateIfOtherPlayer(time);
				}
			}
		}

		public static void addMail(string mailName, bool noLetter = false, bool sendToEveryone = false)
		{
			if (sendToEveryone)
			{
				multiplayer.broadcastPartyWideMail(mailName, Multiplayer.PartyWideMessageQueue.SeenMail, noLetter);
				return;
			}
			mailName = mailName.Trim();
			mailName = mailName.Replace(Environment.NewLine, "");
			if (!player.hasOrWillReceiveMail(mailName))
			{
				if (noLetter)
				{
					player.mailReceived.Add(mailName);
				}
				else
				{
					player.mailbox.Add(mailName);
				}
			}
		}

		public static void addMailForTomorrow(string mailName, bool noLetter = false, bool sendToEveryone = false)
		{
			if (sendToEveryone)
			{
				multiplayer.broadcastPartyWideMail(mailName, Multiplayer.PartyWideMessageQueue.MailForTomorrow, noLetter);
				return;
			}
			mailName = mailName.Trim();
			mailName = mailName.Replace(Environment.NewLine, "");
			if (player.hasOrWillReceiveMail(mailName))
			{
				return;
			}
			if (noLetter)
			{
				mailName += "%&NL&%";
			}
			player.mailForTomorrow.Add(mailName);
			if (!sendToEveryone || !IsMultiplayer)
			{
				return;
			}
			foreach (Farmer value in otherFarmers.Values)
			{
				if (value != player && !player.hasOrWillReceiveMail(mailName))
				{
					value.mailForTomorrow.Add(mailName);
				}
			}
		}

		public static void drawDialogue(NPC speaker)
		{
			if (speaker.CurrentDialogue.Count != 0)
			{
				activeClickableMenu = new DialogueBox(speaker.CurrentDialogue.Peek());
				dialogueUp = true;
				if (!eventUp)
				{
					player.Halt();
					player.CanMove = false;
				}
				if (speaker != null)
				{
					currentSpeaker = speaker;
				}
			}
		}

		public static void drawDialogueNoTyping(NPC speaker, string dialogue)
		{
			if (speaker == null)
			{
				currentObjectDialogue.Enqueue(dialogue);
			}
			else if (dialogue != null)
			{
				speaker.CurrentDialogue.Push(new Dialogue(dialogue, speaker));
			}
			activeClickableMenu = new DialogueBox(speaker.CurrentDialogue.Peek());
			dialogueUp = true;
			player.CanMove = false;
			if (speaker != null)
			{
				currentSpeaker = speaker;
			}
		}

		public static void multipleDialogues(string[] messages)
		{
			activeClickableMenu = new DialogueBox(messages.ToList());
			dialogueUp = true;
			player.CanMove = false;
		}

		public static void drawDialogueNoTyping(string dialogue)
		{
			drawObjectDialogue(dialogue);
			if (activeClickableMenu != null && activeClickableMenu is DialogueBox)
			{
				(activeClickableMenu as DialogueBox).finishTyping();
			}
		}

		public static void drawDialogue(NPC speaker, string dialogue)
		{
			speaker.CurrentDialogue.Push(new Dialogue(dialogue, speaker));
			drawDialogue(speaker);
		}

		public static void drawDialogue(NPC speaker, string dialogue, Texture2D overridePortrait)
		{
			speaker.CurrentDialogue.Push(new Dialogue(dialogue, speaker)
			{
				overridePortrait = overridePortrait
			});
			drawDialogue(speaker);
		}

		public static void drawItemNumberSelection(string itemType, int price)
		{
			selectedItemsType = itemType;
			numberOfSelectedItems = 0;
			priceOfSelectedItem = price;
			if (itemType.Equals("calicoJackBet"))
			{
				currentObjectDialogue.Enqueue(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2946", player.clubCoins));
			}
			else if (itemType.Equals("flutePitch"))
			{
				currentObjectDialogue.Enqueue(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2949"));
				numberOfSelectedItems = (int)currentLocation.actionObjectForQuestionDialogue.scale.X / 100;
			}
			else if (itemType.Equals("drumTone"))
			{
				currentObjectDialogue.Enqueue(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2951"));
				numberOfSelectedItems = (int)currentLocation.actionObjectForQuestionDialogue.scale.X;
			}
			else if (itemType.Equals("jukebox"))
			{
				currentObjectDialogue.Enqueue(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2953"));
			}
			else if (itemType.Equals("Fuel"))
			{
				drawObjectDialogue(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2955"));
			}
			else if (currentSpeaker != null)
			{
				setDialogue(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2956"), typing: false);
			}
			else
			{
				currentObjectDialogue.Enqueue(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2957"));
			}
		}

		public static void setDialogue(string dialogue, bool typing)
		{
			if (currentSpeaker != null)
			{
				currentSpeaker.CurrentDialogue.Peek().setCurrentDialogue(dialogue);
				if (typing)
				{
					drawDialogue(currentSpeaker);
				}
				else
				{
					drawDialogueNoTyping(currentSpeaker, null);
				}
			}
			else if (typing)
			{
				drawObjectDialogue(dialogue);
			}
			else
			{
				drawDialogueNoTyping(dialogue);
			}
		}

		private static void checkIfDialogueIsQuestion()
		{
			if (currentSpeaker != null && currentSpeaker.CurrentDialogue.Count > 0 && currentSpeaker.CurrentDialogue.Peek().isCurrentDialogueAQuestion())
			{
				questionChoices.Clear();
				isQuestion = true;
				List<NPCDialogueResponse> nPCResponseOptions = currentSpeaker.CurrentDialogue.Peek().getNPCResponseOptions();
				for (int i = 0; i < nPCResponseOptions.Count; i++)
				{
					questionChoices.Add(nPCResponseOptions[i]);
				}
			}
		}

		public static void drawLetterMessage(string message)
		{
			activeClickableMenu = new LetterViewerMenu(message);
		}

		public static void drawObjectDialogue(string dialogue)
		{
			if (activeClickableMenu != null)
			{
				activeClickableMenu.emergencyShutDown();
			}
			currentLocation.tapToMove.Reset();
			activeClickableMenu = new DialogueBox(dialogue);
			player.CanMove = false;
			dialogueUp = true;
		}

		public static void drawObjectQuestionDialogue(string dialogue, List<Response> choices, int width)
		{
			activeClickableMenu = new DialogueBox(dialogue, choices, width);
			dialogueUp = true;
			player.CanMove = false;
		}

		public static void drawObjectQuestionDialogue(string dialogue, List<Response> choices)
		{
			activeClickableMenu = new DialogueBox(dialogue, choices);
			dialogueUp = true;
			player.CanMove = false;
		}

		public static void removeThisCharacterFromAllLocations(NPC toDelete)
		{
			for (int i = 0; i < locations.Count; i++)
			{
				if (locations[i].characters.Contains(toDelete))
				{
					locations[i].characters.Remove(toDelete);
				}
			}
		}

		public static void warpCharacter(NPC character, string targetLocationName, Point position)
		{
			warpCharacter(character, targetLocationName, new Vector2(position.X, position.Y));
		}

		public static void warpCharacter(NPC character, string targetLocationName, Vector2 position)
		{
			warpCharacter(character, getLocationFromName(targetLocationName), position);
		}

		public static void warpCharacter(NPC character, GameLocation targetLocation, Vector2 position)
		{
			if (character.currentLocation == null)
			{
				throw new ArgumentException("In warpCharacter, the character's currentLocation must not be null");
			}
			if (currentSeason.Equals("winter") && dayOfMonth >= 15 && dayOfMonth <= 17 && targetLocation.name.Equals("Beach"))
			{
				targetLocation = getLocationFromName("BeachNightMarket");
			}
			if (targetLocation.name.Equals("Trailer") && MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
			{
				targetLocation = getLocationFromName("Trailer_Big");
				if (position.X == 12f && position.Y == 9f)
				{
					position.X = 13f;
					position.Y = 24f;
				}
			}
			if (IsClient)
			{
				multiplayer.requestCharacterWarp(character, targetLocation, position);
				return;
			}
			if (!targetLocation.characters.Contains(character))
			{
				character.currentLocation.characters.Remove(character);
				targetLocation.addCharacter(character);
			}
			character.isCharging = false;
			character.speed = 2;
			character.blockedInterval = 0;
			string text = NPC.getTextureNameForCharacter(character.Name);
			bool flag = false;
			if (character.isVillager())
			{
				if (character.Name.Equals("Maru"))
				{
					if (targetLocation.Name.Equals("Hospital"))
					{
						text = character.Name + "_" + targetLocation.Name;
						flag = true;
					}
					else if (!targetLocation.Name.Equals("Hospital") && character.Sprite.textureName.Value != character.Name)
					{
						text = character.Name;
						flag = true;
					}
				}
				else if (character.Name.Equals("Shane"))
				{
					if (targetLocation.Name.Equals("JojaMart"))
					{
						text = character.Name + "_" + targetLocation.Name;
						flag = true;
					}
					else if (!targetLocation.Name.Equals("JojaMart") && character.Sprite.textureName.Value != character.Name)
					{
						text = character.Name;
						flag = true;
					}
				}
			}
			if (flag)
			{
				character.Sprite.LoadTexture("Characters\\" + text);
			}
			character.position.X = position.X * 64f;
			character.position.Y = position.Y * 64f;
			if (character.CurrentDialogue.Count > 0 && character.CurrentDialogue.Peek().removeOnNextMove && !character.getTileLocation().Equals(character.DefaultPosition / 64f))
			{
				character.CurrentDialogue.Pop();
			}
			if (targetLocation is FarmHouse)
			{
				character.arriveAtFarmHouse(targetLocation as FarmHouse);
			}
			else
			{
				character.arriveAt(targetLocation);
			}
			if (character.currentLocation != null && !character.currentLocation.Equals(targetLocation))
			{
				character.currentLocation.characters.Remove(character);
			}
			character.currentLocation = targetLocation;
		}

		public static LocationRequest getLocationRequest(string locationName, bool isStructure = false)
		{
			if (locationName == null)
			{
				throw new ArgumentException();
			}
			return new LocationRequest(locationName, isStructure, getLocationFromName(locationName, isStructure));
		}

		public static void warpHome()
		{
			LocationRequest locationRequest = getLocationRequest(player.homeLocation.Value);
			locationRequest.OnWarp += delegate
			{
				player.position.Set(Utility.PointToVector2((currentLocation as FarmHouse).GetPlayerBedSpot()) * 64f);
			};
			warpFarmer(locationRequest, 5, 9, player.FacingDirection);
		}

		public static void warpFarmer(string locationName, int tileX, int tileY, bool flip, bool doFade = true)
		{
			warpFarmer(getLocationRequest(locationName), tileX, tileY, flip ? ((player.FacingDirection + 2) % 4) : player.FacingDirection, doFade);
		}

		public static void warpFarmer(string locationName, int tileX, int tileY, int facingDirectionAfterWarp, bool doFade = true)
		{
			warpFarmer(getLocationRequest(locationName), tileX, tileY, facingDirectionAfterWarp, doFade);
		}

		public static void warpFarmer(string locationName, int tileX, int tileY, int facingDirectionAfterWarp, bool isStructure, bool doFade = true)
		{
			warpFarmer(getLocationRequest(locationName, isStructure), tileX, tileY, facingDirectionAfterWarp, doFade);
		}

		public virtual bool ShouldDismountOnWarp(Horse mount, GameLocation old_location, GameLocation new_location, bool doFade = true)
		{
			if (mount == null)
			{
				return false;
			}
			if (currentLocation != null && currentLocation.IsOutdoors && new_location != null)
			{
				return !new_location.IsOutdoors;
			}
			return false;
		}

		public static void warpFarmer(LocationRequest locationRequest, int tileX, int tileY, int facingDirectionAfterWarp, bool doFade = true)
		{
			currentLocation.tapToMove.Reset();
			int warp_offset_x = nextFarmerWarpOffsetX;
			int warp_offset_y = nextFarmerWarpOffsetY;
			nextFarmerWarpOffsetX = 0;
			nextFarmerWarpOffsetY = 0;
			if (locationRequest.Name.Equals("Beach") && currentSeason.Equals("winter") && dayOfMonth >= 15 && dayOfMonth <= 17 && !eventUp)
			{
				locationRequest = getLocationRequest("BeachNightMarket");
			}
			if (locationRequest.Name.Equals("Farm") && currentLocation.NameOrUniqueName == "Greenhouse")
			{
				bool flag = false;
				foreach (Warp warp in currentLocation.warps)
				{
					if (warp.TargetX == tileX && warp.TargetY == tileY)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					Building building = null;
					foreach (Building building2 in getFarm().buildings)
					{
						if (building2 is GreenhouseBuilding)
						{
							building = building2;
							break;
						}
					}
					if (building != null)
					{
						tileX = building.getPointForHumanDoor().X;
						tileY = building.getPointForHumanDoor().Y + 1;
					}
				}
			}
			if (locationRequest.Name == "IslandSouth" && tileX <= 15 && tileY <= 6)
			{
				tileX = 21;
				tileY = 43;
			}
			if (locationRequest.Name.StartsWith("VolcanoDungeon"))
			{
				warp_offset_x = 0;
				warp_offset_y = 0;
			}
			if (player.isRidingHorse() && currentLocation != null)
			{
				GameLocation gameLocation = locationRequest.Location;
				if (gameLocation == null)
				{
					gameLocation = getLocationFromName(locationRequest.Name);
				}
				if (game1.ShouldDismountOnWarp(player.mount, currentLocation, gameLocation))
				{
					player.mount.dismount();
					warp_offset_x = 0;
					warp_offset_y = 0;
				}
			}
			if (locationRequest.Name.Equals("Trailer") && MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
			{
				locationRequest = getLocationRequest("Trailer_Big");
				tileX = 13;
				tileY = 24;
			}
			if (locationRequest.Name.Equals("Farm"))
			{
				Farm farm = getFarm();
				if (currentLocation.NameOrUniqueName == "FarmCave" && tileX == 34 && tileY == 6)
				{
					if (whichFarm == 6)
					{
						tileX = 34;
						tileY = 16;
					}
					else if (whichFarm == 5)
					{
						tileX = 30;
						tileY = 36;
					}
					Point mapPropertyPosition = farm.GetMapPropertyPosition("FarmCaveEntry", tileX, tileY);
					tileX = mapPropertyPosition.X;
					tileY = mapPropertyPosition.Y;
				}
				else if (currentLocation.NameOrUniqueName == "Forest" && tileX == 41 && tileY == 64)
				{
					if (whichFarm == 6)
					{
						tileX = 82;
						tileY = 103;
					}
					else if (whichFarm == 5)
					{
						tileX = 40;
						tileY = 64;
					}
					Point mapPropertyPosition2 = farm.GetMapPropertyPosition("ForestEntry", tileX, tileY);
					tileX = mapPropertyPosition2.X;
					tileY = mapPropertyPosition2.Y;
				}
				else if (currentLocation.NameOrUniqueName == "BusStop" && tileX == 79 && tileY == 17)
				{
					Point mapPropertyPosition3 = farm.GetMapPropertyPosition("BusStopEntry", tileX, tileY);
					tileX = mapPropertyPosition3.X;
					tileY = mapPropertyPosition3.Y;
				}
				else if (currentLocation.NameOrUniqueName == "Backwoods" && tileX == 40 && tileY == 0)
				{
					Point mapPropertyPosition4 = farm.GetMapPropertyPosition("BackwoodsEntry", tileX, tileY);
					tileX = mapPropertyPosition4.X;
					tileY = mapPropertyPosition4.Y;
				}
				else if (currentLocation.NameOrUniqueName == "FarmHouse" && tileX == 64 && tileY == 15)
				{
					Point mainFarmHouseEntry = farm.GetMainFarmHouseEntry();
					tileX = mainFarmHouseEntry.X;
					tileY = mainFarmHouseEntry.Y;
				}
			}
			if (locationRequest.Name.Equals("Club") && !player.hasClubCard)
			{
				locationRequest = getLocationRequest("SandyHouse");
				locationRequest.OnWarp += delegate
				{
					NPC characterFromName = currentLocation.getCharacterFromName("Bouncer");
					if (characterFromName != null)
					{
						Vector2 vector = new Vector2(17f, 4f);
						characterFromName.showTextAboveHead(content.LoadString("Strings\\Locations:Club_Bouncer_TextAboveHead" + (random.Next(2) + 1)));
						int num = random.Next();
						currentLocation.playSound("thudStep");
						multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite(288, 100f, 1, 24, vector * 64f, flicker: true, flipped: false, currentLocation, player)
						{
							shakeIntensity = 0.5f,
							shakeIntensityChange = 0.002f,
							extraInfoForEndBehavior = num,
							endFunction = currentLocation.removeTemporarySpritesWithID
						}, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, vector * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: false, 0.0263f, 0f, Color.Yellow, 4f, 0f, 0f, 0f)
						{
							id = num
						}, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, vector * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: true, 0.0263f, 0f, Color.Orange, 4f, 0f, 0f, 0f)
						{
							delayBeforeAnimationStart = 100,
							id = num
						}, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, vector * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: false, 0.0263f, 0f, Color.White, 3f, 0f, 0f, 0f)
						{
							delayBeforeAnimationStart = 200,
							id = num
						});
						currentLocation.netAudio.StartPlaying("fuse");
					}
				};
				tileX = 17;
				tileY = 4;
			}
			if (weatherIcon == 1 && whereIsTodaysFest != null && locationRequest.Name.Equals(whereIsTodaysFest) && !warpingForForcedRemoteEvent && timeOfDay <= Convert.ToInt32(temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + currentSeason + dayOfMonth)["conditions"].Split('/')[1].Split(' ')[1]))
			{
				if (timeOfDay < Convert.ToInt32(temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + currentSeason + dayOfMonth)["conditions"].Split('/')[1].Split(' ')[0]))
				{
					if (!currentLocation.Name.Equals("Hospital"))
					{
						player.Position = player.lastPosition;
						drawObjectDialogue(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2973"));
						return;
					}
					locationRequest = getLocationRequest("BusStop");
					tileX = 34;
					tileY = 23;
				}
				else
				{
					if (IsMultiplayer)
					{
						player.team.SetLocalReady("festivalStart", ready: true);
						activeClickableMenu = new ReadyCheckDialog("festivalStart", allowCancel: true, delegate
						{
							exitActiveMenu();
							if (player.mount != null)
							{
								player.mount.dismount();
								warp_offset_x = 0;
								warp_offset_y = 0;
							}
							performWarpFarmer(locationRequest, tileX, tileY, facingDirectionAfterWarp);
						});
						return;
					}
					if (player.mount != null)
					{
						player.mount.dismount();
						warp_offset_x = 0;
						warp_offset_y = 0;
					}
				}
			}
			tileX += warp_offset_x;
			tileY += warp_offset_y;
			performWarpFarmer(locationRequest, tileX, tileY, facingDirectionAfterWarp, doFade);
		}

		private static void performWarpFarmer(LocationRequest locationRequest, int tileX, int tileY, int facingDirectionAfterWarp, bool doFade = true)
		{
			if ((currentLocation.Name.Equals("Town") || jukeboxPlaying) && getLocationFromName(locationRequest.Name).IsOutdoors && currentSong != null && (currentSong.Name.Contains("town") || jukeboxPlaying))
			{
				changeMusicTrack("none");
			}
			if (locationRequest.Location != null)
			{
				if (tileX >= locationRequest.Location.Map.Layers[0].LayerWidth - 1)
				{
					tileX--;
				}
				if (IsMasterGame)
				{
					locationRequest.Location.hostSetup();
				}
			}
			Console.WriteLine("Warping to " + locationRequest.Name);
			if (player.IsSitting())
			{
				player.StopSitting(animate: false);
			}
			player.previousLocationName = ((player.currentLocation != null) ? ((string)player.currentLocation.name) : "");
			Game1.locationRequest = locationRequest;
			xLocationAfterWarp = tileX;
			yLocationAfterWarp = tileY;
			_isWarping = true;
			Game1.facingDirectionAfterWarp = facingDirectionAfterWarp;
			if (doFade)
			{
				fadeScreenToBlack();
			}
			else
			{
				fadeScreenIn();
			}
			setRichPresence("location", locationRequest.Name);
		}

		public static void requestLocationInfoFromServer()
		{
			if (locationRequest != null)
			{
				client.sendMessage(5, (short)xLocationAfterWarp, (short)yLocationAfterWarp, locationRequest.Name, (byte)(locationRequest.IsStructure ? 1 : 0));
			}
			currentLocation = null;
			player.Position = new Vector2(xLocationAfterWarp * 64, yLocationAfterWarp * 64 - (player.Sprite.getHeight() - 32) + 16);
			player.faceDirection(facingDirectionAfterWarp);
		}

		public static void changeInvisibility(string name, bool invisibility)
		{
			getCharacterFromName(name).IsInvisible = invisibility;
		}

		public static T getCharacterFromName<T>(string name, bool mustBeVillager = true) where T : NPC
		{
			if (currentLocation != null)
			{
				foreach (NPC character in currentLocation.getCharacters())
				{
					if (character is T && character.Name.Equals(name) && (!mustBeVillager || character.isVillager()))
					{
						return (T)character;
					}
				}
			}
			for (int i = 0; i < locations.Count; i++)
			{
				foreach (NPC character2 in locations[i].getCharacters())
				{
					if (character2 is T && !(locations[i] is MovieTheater) && character2.Name.Equals(name) && (!mustBeVillager || character2.isVillager()))
					{
						return (T)character2;
					}
				}
			}
			if (getFarm() != null)
			{
				foreach (Building building in getFarm().buildings)
				{
					if (building.indoors.Value == null)
					{
						continue;
					}
					foreach (NPC character3 in building.indoors.Value.characters)
					{
						if (character3 is T && character3.Name.Equals(name) && (!mustBeVillager || character3.isVillager()))
						{
							return (T)character3;
						}
					}
				}
			}
			return null;
		}

		public static NPC getCharacterFromName(string name, bool mustBeVillager = true, bool useLocationsListOnly = false)
		{
			if (!useLocationsListOnly && currentLocation != null && !(currentLocation is MovieTheater))
			{
				foreach (NPC character in currentLocation.getCharacters())
				{
					if (!character.eventActor && character.Name.Equals(name) && (!mustBeVillager || character.isVillager()))
					{
						return character;
					}
				}
			}
			for (int i = 0; i < locations.Count; i++)
			{
				if (locations[i] is MovieTheater)
				{
					continue;
				}
				foreach (NPC character2 in locations[i].getCharacters())
				{
					if (!character2.eventActor && character2.Name.Equals(name) && (!mustBeVillager || character2.isVillager()))
					{
						return character2;
					}
				}
			}
			if (getFarm() != null)
			{
				foreach (Building building in getFarm().buildings)
				{
					if (building.indoors.Value == null)
					{
						continue;
					}
					foreach (NPC character3 in building.indoors.Value.characters)
					{
						if (character3.Name.Equals(name) && (!mustBeVillager || character3.isVillager()))
						{
							return character3;
						}
					}
				}
			}
			return null;
		}

		public static NPC removeCharacterFromItsLocation(string name, bool must_be_villager = true)
		{
			if (!IsMasterGame)
			{
				return null;
			}
			for (int i = 0; i < locations.Count; i++)
			{
				if (locations[i] is MovieTheater)
				{
					continue;
				}
				for (int j = 0; j < locations[i].getCharacters().Count; j++)
				{
					if (locations[i].getCharacters()[j].Name.Equals(name) && (!must_be_villager || locations[i].getCharacters()[j].isVillager()))
					{
						NPC result = locations[i].characters[j];
						locations[i].characters.RemoveAt(j);
						return result;
					}
				}
			}
			return null;
		}

		public static GameLocation getLocationFromName(string name)
		{
			return getLocationFromName(name, isStructure: false);
		}

		public static GameLocation getLocationFromName(string name, bool isStructure)
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}
			if (currentLocation != null)
			{
				if (!isStructure && string.Equals(currentLocation.name, name, StringComparison.OrdinalIgnoreCase))
				{
					return currentLocation;
				}
				if (!isStructure && (bool)currentLocation.isStructure && currentLocation.Root != null && string.Equals(currentLocation.Root.Value.name, name, StringComparison.OrdinalIgnoreCase))
				{
					return currentLocation.Root.Value;
				}
				if (isStructure && currentLocation.uniqueName == name)
				{
					return currentLocation;
				}
			}
			if (_locationLookup.TryGetValue(name, out var value))
			{
				return value;
			}
			return getLocationFromNameInLocationsList(name, isStructure);
		}

		public static GameLocation getLocationFromNameInLocationsList(string name, bool isStructure = false)
		{
			for (int i = 0; i < locations.Count; i++)
			{
				if (!isStructure)
				{
					if (string.Equals(locations[i].Name, name, StringComparison.OrdinalIgnoreCase))
					{
						_locationLookup[locations[i].Name] = locations[i];
						return locations[i];
					}
					continue;
				}
				GameLocation gameLocation = findStructure(locations[i], name);
				if (gameLocation != null)
				{
					_locationLookup[name] = gameLocation;
					return gameLocation;
				}
			}
			if (name.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase))
			{
				return MineShaft.GetMine(name);
			}
			if (name.StartsWith("VolcanoDungeon", StringComparison.OrdinalIgnoreCase))
			{
				return VolcanoDungeon.GetLevel(name);
			}
			if (!isStructure)
			{
				return getLocationFromName(name, isStructure: true);
			}
			return null;
		}

		public static void flushLocationLookup()
		{
			_locationLookup.Clear();
		}

		public static void removeLocationFromLocationLookup(string name_or_unique_name)
		{
			List<string> list = new List<string>();
			foreach (string key in _locationLookup.Keys)
			{
				if (_locationLookup[key].NameOrUniqueName == name_or_unique_name)
				{
					list.Add(key);
				}
			}
			foreach (string item in list)
			{
				_locationLookup.Remove(item);
			}
		}

		public static void removeLocationFromLocationLookup(GameLocation location)
		{
			List<string> list = new List<string>();
			foreach (string key in _locationLookup.Keys)
			{
				if (_locationLookup[key] == location)
				{
					list.Add(key);
				}
			}
			foreach (string item in list)
			{
				_locationLookup.Remove(item);
			}
		}

		public static GameLocation findStructure(GameLocation parentLocation, string name)
		{
			if (!(parentLocation is BuildableGameLocation))
			{
				return null;
			}
			foreach (Building building in (parentLocation as BuildableGameLocation).buildings)
			{
				if (building.indoors.Value != null && building.indoors.Value.uniqueName.Equals(name))
				{
					return building.indoors;
				}
			}
			return null;
		}

		public static void addNewFarmBuildingMaps()
		{
			if (player.CoopUpgradeLevel >= 1 && getLocationFromName("Coop") == null)
			{
				locations.Add(new GameLocation("Maps\\Coop" + player.CoopUpgradeLevel, "Coop"));
				getLocationFromName("Farm").setTileProperty(21, 10, "Buildings", "Action", "Warp 2 9 Coop");
				currentCoopTexture = content.Load<Texture2D>("BuildingUpgrades\\Coop" + player.coopUpgradeLevel);
			}
			else if (getLocationFromName("Coop") != null)
			{
				getLocationFromName("Coop").map = content.Load<Map>("Maps\\Coop" + player.CoopUpgradeLevel);
				currentCoopTexture = content.Load<Texture2D>("BuildingUpgrades\\Coop" + player.coopUpgradeLevel);
			}
			if (player.BarnUpgradeLevel >= 1 && getLocationFromName("Barn") == null)
			{
				locations.Add(new GameLocation("Maps\\Barn" + player.BarnUpgradeLevel, "Barn"));
				getLocationFromName("Farm").warps.Add(new Warp(14, 9, "Barn", 11, 14, flipFarmer: false));
				currentBarnTexture = content.Load<Texture2D>("BuildingUpgrades\\Barn" + player.barnUpgradeLevel);
			}
			else if (getLocationFromName("Barn") != null)
			{
				getLocationFromName("Barn").map = content.Load<Map>("Maps\\Barn" + player.BarnUpgradeLevel);
				currentBarnTexture = content.Load<Texture2D>("BuildingUpgrades\\Barn" + player.barnUpgradeLevel);
			}
			FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(player);
			if (player.HouseUpgradeLevel >= 1 && homeOfFarmer.Map.Id.Equals("FarmHouse"))
			{
				homeOfFarmer.updateMap();
				int num = currentWallpaper;
				int num2 = currentFloor;
				currentWallpaper = farmerWallpaper;
				currentFloor = FarmerFloor;
				updateFloorInFarmHouse(currentFloor);
				updateWallpaperInFarmHouse(currentWallpaper);
				currentWallpaper = num;
				currentFloor = num2;
			}
			if (player.hasGreenhouse && getLocationFromName("FarmGreenHouse") == null)
			{
				locations.Add(new GameLocation("Maps\\FarmGreenHouse", "FarmGreenHouse"));
				getLocationFromName("Farm").setTileProperty(3, 10, "Buildings", "Action", "Warp 5 15 FarmGreenHouse");
				greenhouseTexture = content.Load<Texture2D>("BuildingUpgrades\\Greenhouse");
			}
		}

		public static bool waitingToPassOut()
		{
			if (activeClickableMenu is ReadyCheckDialog && (activeClickableMenu as ReadyCheckDialog).checkName == "sleep")
			{
				return !(activeClickableMenu as ReadyCheckDialog).isCancelable();
			}
			return false;
		}

		public static void PassOutNewDay()
		{
			player.lastSleepLocation.Value = currentLocation.NameOrUniqueName;
			player.lastSleepPoint.Value = player.getTileLocationPoint();
			if (!IsMultiplayer)
			{
				NewDay(0f);
				return;
			}
			player.FarmerSprite.setCurrentSingleFrame(5, 3000);
			player.FarmerSprite.PauseForSingleAnimation = true;
			player.passedOut = true;
			if (activeClickableMenu != null)
			{
				activeClickableMenu.emergencyShutDown();
				exitActiveMenu();
			}
			activeClickableMenu = new ReadyCheckDialog("sleep", allowCancel: false, delegate
			{
				NewDay(0f);
			});
		}

		public static void NewDay(float timeToPause)
		{
			currentMinigame = null;
			newDay = true;
			newDaySync = new NewDaySynchronizer();
			if ((bool)player.isInBed || player.passedOut)
			{
				nonWarpFade = true;
				screenFade.FadeScreenToBlack(player.passedOut ? 1.1f : 0f);
				player.Halt();
				player.currentEyes = 1;
				player.blinkTimer = -4000;
				player.CanMove = false;
				player.passedOut = false;
				pauseTime = timeToPause;
			}
			if (activeClickableMenu != null && !dialogueUp)
			{
				activeClickableMenu.emergencyShutDown();
				exitActiveMenu();
			}
		}

		public static void screenGlowOnce(Color glowColor, bool hold, float rate = 0.005f, float maxAlpha = 0.3f)
		{
			screenGlowMax = maxAlpha;
			screenGlowRate = rate;
			screenGlowAlpha = 0f;
			screenGlowUp = true;
			screenGlowColor = glowColor;
			screenGlow = true;
			screenGlowHold = hold;
		}

		public static void removeTilesFromLayer(GameLocation l, string layer, Microsoft.Xna.Framework.Rectangle area)
		{
			for (int i = area.X; i < area.Right; i++)
			{
				for (int j = area.Y; j < area.Bottom; j++)
				{
					l.Map.GetLayer(layer).Tiles[i, j] = null;
				}
			}
		}

		public static void removeFrontLayerForFarmBuildings()
		{
		}

		public static string shortDayNameFromDayOfSeason(int dayOfSeason)
		{
			return (dayOfSeason % 7) switch
			{
				0 => "Sun", 
				1 => "Mon", 
				2 => "Tue", 
				3 => "Wed", 
				4 => "Thu", 
				5 => "Fri", 
				6 => "Sat", 
				_ => "", 
			};
		}

		public static string shortDayDisplayNameFromDayOfSeason(int dayOfSeason)
		{
			if (dayOfSeason < 0)
			{
				return string.Empty;
			}
			return _shortDayDisplayName[dayOfSeason % 7];
		}

		public static void showNameSelectScreen(string type)
		{
			nameSelectType = type;
			nameSelectUp = true;
		}

		public static void nameSelectionDone()
		{
		}

		public static void tryToBuySelectedItems()
		{
			if (selectedItemsType.Equals("flutePitch"))
			{
				currentObjectDialogue.Clear();
				currentLocation.actionObjectForQuestionDialogue.scale.X = numberOfSelectedItems * 100;
				dialogueUp = false;
				player.CanMove = true;
				numberOfSelectedItems = -1;
			}
			else if (selectedItemsType.Equals("drumTone"))
			{
				currentObjectDialogue.Clear();
				currentLocation.actionObjectForQuestionDialogue.scale.X = numberOfSelectedItems;
				dialogueUp = false;
				player.CanMove = true;
				numberOfSelectedItems = -1;
			}
			else if (selectedItemsType.Equals("jukebox"))
			{
				changeMusicTrack(player.songsHeard.ElementAt(numberOfSelectedItems));
				dialogueUp = false;
				player.CanMove = true;
				numberOfSelectedItems = -1;
			}
			else if (player.Money >= priceOfSelectedItem * numberOfSelectedItems && numberOfSelectedItems > 0)
			{
				bool flag = true;
				switch (selectedItemsType)
				{
				case "Animal Food":
					player.Feed += numberOfSelectedItems;
					setDialogue(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3072"), typing: false);
					break;
				case "Fuel":
					((Lantern)player.getToolFromName("Lantern")).fuelLeft += numberOfSelectedItems;
					break;
				case "Star Token":
					player.festivalScore += numberOfSelectedItems;
					dialogueUp = false;
					player.canMove = true;
					break;
				}
				if (flag)
				{
					player.Money -= priceOfSelectedItem * numberOfSelectedItems;
					numberOfSelectedItems = -1;
					playSound("purchase");
				}
			}
			else if (player.Money < priceOfSelectedItem * numberOfSelectedItems)
			{
				currentObjectDialogue.Dequeue();
				setDialogue(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3078"), typing: false);
				numberOfSelectedItems = -1;
			}
		}

		public static void throwActiveObjectDown()
		{
			player.CanMove = false;
			switch (player.FacingDirection)
			{
			case 0:
				((FarmerSprite)player.Sprite).animateBackwardsOnce(80, 50f);
				break;
			case 1:
				((FarmerSprite)player.Sprite).animateBackwardsOnce(72, 50f);
				break;
			case 2:
				((FarmerSprite)player.Sprite).animateBackwardsOnce(64, 50f);
				break;
			case 3:
				((FarmerSprite)player.Sprite).animateBackwardsOnce(88, 50f);
				break;
			}
			player.reduceActiveItemByOne();
			playSound("throwDownITem");
		}

		public static void runTestEvent()
		{
			StreamReader streamReader = new StreamReader("test_event.txt");
			string text = streamReader.ReadLine();
			string event_string = streamReader.ReadToEnd();
			event_string = event_string.Replace("\r\n", "/");
			Console.WriteLine(event_string);
			LocationRequest locationRequest = getLocationRequest(text);
			locationRequest.OnWarp += delegate
			{
				currentLocation.currentEvent = new Event(event_string);
				currentLocation.checkForEvents();
			};
			int x = 8;
			int y = 8;
			Utility.getDefaultWarpLocation(text, ref x, ref y);
			warpFarmer(locationRequest, x, y, player.FacingDirection);
		}

		public static bool isMusicContextActiveButNotPlaying(MusicContext music_context = MusicContext.Default)
		{
			if (_activeMusicContext != music_context)
			{
				return false;
			}
			if (getMusicTrackName() == "none")
			{
				return true;
			}
			if (currentSong != null && currentSong.Name == getMusicTrackName() && !currentSong.IsPlaying)
			{
				return true;
			}
			return false;
		}

		public static bool IsMusicContextActive(MusicContext music_context = MusicContext.Default)
		{
			if (_activeMusicContext != music_context)
			{
				return true;
			}
			return false;
		}

		public static bool doesMusicContextHaveTrack(MusicContext music_context = MusicContext.Default)
		{
			return _requestedMusicTracks.ContainsKey(music_context);
		}

		public static string getMusicTrackName(MusicContext music_context = MusicContext.Default)
		{
			if (_requestedMusicTracks.TryGetValue(music_context, out var value))
			{
				return value.Key;
			}
			return "none";
		}

		public static void stopMusicTrack(MusicContext music_context)
		{
			if (_requestedMusicTracks.ContainsKey(music_context))
			{
				_requestedMusicTracks.Remove(music_context);
				UpdateRequestedMusicTrack();
			}
		}

		public static void changeMusicTrack(string newTrackName, bool track_interruptable = false, MusicContext music_context = MusicContext.Default)
		{
			if (musicWaveBankState != MusicWaveBankState.Created)
			{
				_pendingTrackName = newTrackName;
				return;
			}
			if (music_context == MusicContext.Default && morningSongPlayAction != null)
			{
				if (delayedActions.Contains(morningSongPlayAction))
				{
					delayedActions.Remove(morningSongPlayAction);
				}
				morningSongPlayAction = null;
			}
			if (music_context != MusicContext.ImportantSplitScreenMusic && !player.songsHeard.Contains(newTrackName))
			{
				Utility.farmerHeardSong(newTrackName);
			}
			_requestedMusicTracks[music_context] = new KeyValuePair<string, bool>(newTrackName, track_interruptable);
			UpdateRequestedMusicTrack();
		}

		public static void UpdateRequestedMusicTrack()
		{
			_activeMusicContext = MusicContext.Default;
			KeyValuePair<string, bool> keyValuePair = new KeyValuePair<string, bool>("none", value: true);
			for (int i = 0; i < 5; i++)
			{
				if (_requestedMusicTracks.ContainsKey((MusicContext)i))
				{
					if (i != 4)
					{
						_activeMusicContext = (MusicContext)i;
					}
					keyValuePair = _requestedMusicTracks[(MusicContext)i];
				}
			}
			if (keyValuePair.Key != requestedMusicTrack || keyValuePair.Value != requestedMusicTrackOverrideable)
			{
				requestedMusicDirty = true;
				requestedMusicTrack = keyValuePair.Key;
				requestedMusicTrackOverrideable = keyValuePair.Value;
			}
		}

		public static void enterMine(int whatLevel)
		{
			inMine = true;
			warpFarmer("UndergroundMine" + whatLevel, 6, 6, 2);
		}

		public static string GetSeasonForLocation(GameLocation location)
		{
			if (location != null)
			{
				if (location.Name == "Greenhouse")
				{
					return "spring";
				}
				return location.GetSeasonForLocation();
			}
			return currentSeason;
		}

		public static void getSteamAchievement(string which)
		{
			if (which.Equals("0"))
			{
				which = "a0";
			}
			Program.sdk.GetAchievement(which);
		}

		public static void getAchievement(int which, bool allowBroadcasting = true)
		{
			if (player.achievements.Contains(which) || gameMode != 3)
			{
				return;
			}
			Dictionary<int, string> dictionary = content.Load<Dictionary<int, string>>("Data\\Achievements");
			if (!dictionary.ContainsKey(which))
			{
				return;
			}
			string message = dictionary[which].Split('^')[0];
			player.achievements.Add(which);
			if (which < 32 && allowBroadcasting)
			{
				if (stats.isSharedAchievement(which))
				{
					multiplayer.sendSharedAchievementMessage(which);
				}
				else
				{
					string text = player.Name;
					if (text == "")
					{
						text = content.LoadString("Strings\\UI:Chat_PlayerJoinedNewName");
					}
					multiplayer.globalChatInfoMessage("Achievement", text, "achievement:" + which);
				}
			}
			playSound("achievement");
			addHUDMessage(new HUDMessage(message, achievement: true));
			if (!player.hasOrWillReceiveMail("hatter"))
			{
				addMailForTomorrow("hatter");
			}
		}

		public static void createMultipleObjectDebris(int index, int xTile, int yTile, int number)
		{
			for (int i = 0; i < number; i++)
			{
				createObjectDebris(index, xTile, yTile);
			}
		}

		public static void createMultipleObjectDebris(int index, int xTile, int yTile, int number, GameLocation location)
		{
			for (int i = 0; i < number; i++)
			{
				createObjectDebris(index, xTile, yTile, -1, 0, 1f, location);
			}
		}

		public static void createMultipleObjectDebris(int index, int xTile, int yTile, int number, float velocityMultiplier)
		{
			for (int i = 0; i < number; i++)
			{
				createObjectDebris(index, xTile, yTile, -1, 0, velocityMultiplier);
			}
		}

		public static void createMultipleObjectDebris(int index, int xTile, int yTile, int number, long who)
		{
			for (int i = 0; i < number; i++)
			{
				createObjectDebris(index, xTile, yTile, who);
			}
		}

		public static void createMultipleObjectDebris(int index, int xTile, int yTile, int number, long who, GameLocation location)
		{
			for (int i = 0; i < number; i++)
			{
				createObjectDebris(index, xTile, yTile, who, location);
			}
		}

		public static void createDebris(int debrisType, int xTile, int yTile, int numberOfChunks)
		{
			createDebris(debrisType, xTile, yTile, numberOfChunks, currentLocation);
		}

		public static void createDebris(int debrisType, int xTile, int yTile, int numberOfChunks, GameLocation location)
		{
			if (location == null)
			{
				location = currentLocation;
			}
			location.debris.Add(new Debris(debrisType, numberOfChunks, new Vector2(xTile * 64 + 32, yTile * 64 + 32), new Vector2(player.getStandingX(), player.getStandingY())));
		}

		public static Debris createItemDebris(Item item, Vector2 origin, int direction, GameLocation location = null, int groundLevel = -1)
		{
			if (player.items.Contains(item))
			{
				player.removeItemFromInventory(item);
			}
			if (location == null)
			{
				location = currentLocation;
			}
			Vector2 targetLocation = new Vector2(origin.X, origin.Y);
			switch (direction)
			{
			case 0:
				origin.X -= 32f;
				origin.Y -= 128 + recentMultiplayerRandom.Next(32);
				targetLocation.Y -= 192f;
				break;
			case 1:
				origin.X += 42f;
				origin.Y -= 32 - recentMultiplayerRandom.Next(8);
				targetLocation.X += 256f;
				break;
			case 2:
				origin.X -= 32f;
				origin.Y += recentMultiplayerRandom.Next(32);
				targetLocation.Y += 96f;
				break;
			case 3:
				origin.X -= 64f;
				origin.Y -= 32 - recentMultiplayerRandom.Next(8);
				targetLocation.X -= 256f;
				break;
			case -1:
				targetLocation = player.getStandingPosition();
				break;
			}
			Debris debris = new Debris(item, origin, targetLocation);
			if (groundLevel != -1)
			{
				debris.chunkFinalYLevel = groundLevel;
			}
			location.debris.Add(debris);
			return debris;
		}

		public static void createRadialDebris(GameLocation location, int debrisType, int xTile, int yTile, int numberOfChunks, bool resource, int groundLevel = -1, bool item = false, int color = -1)
		{
			if (groundLevel == -1)
			{
				groundLevel = yTile * 64 + 32;
			}
			Vector2 vector = new Vector2(xTile * 64 + 64, yTile * 64 + 64);
			if (item)
			{
				while (numberOfChunks > 0)
				{
					switch (random.Next(4))
					{
					case 0:
						location.debris.Add(new Debris(new Object(Vector2.Zero, debrisType, 1), vector, vector + new Vector2(-64f, 0f)));
						break;
					case 1:
						location.debris.Add(new Debris(new Object(Vector2.Zero, debrisType, 1), vector, vector + new Vector2(64f, 0f)));
						break;
					case 2:
						location.debris.Add(new Debris(new Object(Vector2.Zero, debrisType, 1), vector, vector + new Vector2(0f, 64f)));
						break;
					case 3:
						location.debris.Add(new Debris(new Object(Vector2.Zero, debrisType, 1), vector, vector + new Vector2(0f, -64f)));
						break;
					}
					numberOfChunks--;
				}
			}
			if (resource)
			{
				location.debris.Add(new Debris(debrisType, numberOfChunks / 4, vector, vector + new Vector2(-64f, 0f)));
				numberOfChunks++;
				location.debris.Add(new Debris(debrisType, numberOfChunks / 4, vector, vector + new Vector2(64f, 0f)));
				numberOfChunks++;
				location.debris.Add(new Debris(debrisType, numberOfChunks / 4, vector, vector + new Vector2(0f, -64f)));
				numberOfChunks++;
				location.debris.Add(new Debris(debrisType, numberOfChunks / 4, vector, vector + new Vector2(0f, 64f)));
			}
			else
			{
				location.debris.Add(new Debris(debrisType, numberOfChunks / 4, vector, vector + new Vector2(-64f, 0f), groundLevel, color));
				numberOfChunks++;
				location.debris.Add(new Debris(debrisType, numberOfChunks / 4, vector, vector + new Vector2(64f, 0f), groundLevel, color));
				numberOfChunks++;
				location.debris.Add(new Debris(debrisType, numberOfChunks / 4, vector, vector + new Vector2(0f, -64f), groundLevel, color));
				numberOfChunks++;
				location.debris.Add(new Debris(debrisType, numberOfChunks / 4, vector, vector + new Vector2(0f, 64f), groundLevel, color));
			}
		}

		public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int xTile, int yTile, int numberOfChunks)
		{
			createRadialDebris(location, texture, sourcerectangle, xTile, yTile, numberOfChunks, yTile);
		}

		public static void createWaterDroplets(string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int xPosition, int yPosition, int numberOfChunks, int groundLevelTile)
		{
			Vector2 vector = new Vector2(xPosition, yPosition);
			currentLocation.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, vector, vector + new Vector2(-64f, 0f), groundLevelTile * 64));
			currentLocation.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, vector, vector + new Vector2(64f, 0f), groundLevelTile * 64));
			currentLocation.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, vector, vector + new Vector2(0f, -64f), groundLevelTile * 64));
			currentLocation.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, vector, vector + new Vector2(0f, 64f), groundLevelTile * 64));
		}

		public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int xTile, int yTile, int numberOfChunks, int groundLevelTile)
		{
			createRadialDebris(location, texture, sourcerectangle, 8, xTile * 64 + 32 + random.Next(32), yTile * 64 + 32 + random.Next(32), numberOfChunks, groundLevelTile);
		}

		public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int sizeOfSourceRectSquares, int xPosition, int yPosition, int numberOfChunks, int groundLevelTile)
		{
			Vector2 vector = new Vector2(xPosition, yPosition);
			location.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, vector, vector + new Vector2(-64f, 0f), groundLevelTile * 64, sizeOfSourceRectSquares));
			location.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, vector, vector + new Vector2(64f, 0f), groundLevelTile * 64, sizeOfSourceRectSquares));
			location.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, vector, vector + new Vector2(0f, -64f), groundLevelTile * 64, sizeOfSourceRectSquares));
			location.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, vector, vector + new Vector2(0f, 64f), groundLevelTile * 64, sizeOfSourceRectSquares));
		}

		public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int sizeOfSourceRectSquares, int xPosition, int yPosition, int numberOfChunks, int groundLevelTile, Color color)
		{
			createRadialDebris(location, texture, sourcerectangle, sizeOfSourceRectSquares, xPosition, yPosition, numberOfChunks, groundLevelTile, color, 1f);
		}

		public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int sizeOfSourceRectSquares, int xPosition, int yPosition, int numberOfChunks, int groundLevelTile, Color color, float scale)
		{
			Vector2 vector = new Vector2(xPosition, yPosition);
			while (numberOfChunks > 0)
			{
				switch (random.Next(4))
				{
				case 0:
				{
					Debris debris = new Debris(texture, sourcerectangle, 1, vector, vector + new Vector2(-64f, 0f), groundLevelTile * 64, sizeOfSourceRectSquares);
					debris.nonSpriteChunkColor.Value = color;
					location?.debris.Add(debris);
					debris.Chunks[0].scale = scale;
					break;
				}
				case 1:
				{
					Debris debris = new Debris(texture, sourcerectangle, 1, vector, vector + new Vector2(64f, 0f), groundLevelTile * 64, sizeOfSourceRectSquares);
					debris.nonSpriteChunkColor.Value = color;
					location?.debris.Add(debris);
					debris.Chunks[0].scale = scale;
					break;
				}
				case 2:
				{
					Debris debris = new Debris(texture, sourcerectangle, 1, vector, vector + new Vector2(random.Next(-64, 64), -64f), groundLevelTile * 64, sizeOfSourceRectSquares);
					debris.nonSpriteChunkColor.Value = color;
					location?.debris.Add(debris);
					debris.Chunks[0].scale = scale;
					break;
				}
				case 3:
				{
					Debris debris = new Debris(texture, sourcerectangle, 1, vector, vector + new Vector2(random.Next(-64, 64), 64f), groundLevelTile * 64, sizeOfSourceRectSquares);
					debris.nonSpriteChunkColor.Value = color;
					location?.debris.Add(debris);
					debris.Chunks[0].scale = scale;
					break;
				}
				}
				numberOfChunks--;
			}
		}

		public static void createObjectDebris(int objectIndex, int xTile, int yTile, long whichPlayer)
		{
			currentLocation.debris.Add(new Debris(objectIndex, new Vector2(xTile * 64 + 32, yTile * 64 + 32), getFarmer(whichPlayer).getStandingPosition()));
		}

		public static void createObjectDebris(int objectIndex, int xTile, int yTile, long whichPlayer, GameLocation location)
		{
			location.debris.Add(new Debris(objectIndex, new Vector2(xTile * 64 + 32, yTile * 64 + 32), getFarmer(whichPlayer).getStandingPosition()));
		}

		public static void createObjectDebris(int objectIndex, int xTile, int yTile, GameLocation location)
		{
			createObjectDebris(objectIndex, xTile, yTile, -1, 0, 1f, location);
		}

		public static void createObjectDebris(int objectIndex, int xTile, int yTile, int groundLevel = -1, int itemQuality = 0, float velocityMultiplyer = 1f, GameLocation location = null)
		{
			if (location == null)
			{
				location = currentLocation;
			}
			Debris debris = new Debris(objectIndex, new Vector2(xTile * 64 + 32, yTile * 64 + 32), new Vector2(player.getStandingX(), player.getStandingY()))
			{
				itemQuality = itemQuality
			};
			foreach (Chunk chunk in debris.Chunks)
			{
				chunk.xVelocity.Value *= velocityMultiplyer;
				chunk.yVelocity.Value *= velocityMultiplyer;
			}
			if (groundLevel != -1)
			{
				debris.chunkFinalYLevel = groundLevel;
			}
			location.debris.Add(debris);
		}

		public static Farmer getFarmer(long id)
		{
			if (player.UniqueMultiplayerID == id)
			{
				return player;
			}
			foreach (Farmer value in otherFarmers.Values)
			{
				if (value.UniqueMultiplayerID == id)
				{
					return value;
				}
			}
			if (!IsMultiplayer)
			{
				return player;
			}
			return MasterPlayer;
		}

		public static Farmer getFarmerMaybeOffline(long id)
		{
			foreach (Farmer allFarmer in getAllFarmers())
			{
				if (allFarmer.UniqueMultiplayerID == id)
				{
					return allFarmer;
				}
			}
			return null;
		}

		public static IEnumerable<Farmer> getAllFarmers()
		{
			return Enumerable.Repeat(MasterPlayer, 1).Concat(getAllFarmhands());
		}

		public static IEnumerable<Farmer> getAllFarmhands()
		{
			if (getFarm() == null)
			{
				yield break;
			}
			foreach (Building building in getFarm().buildings)
			{
				if (!(building.indoors.Value is Cabin))
				{
					continue;
				}
				Cabin cabin = building.indoors.Value as Cabin;
				Farmer farmer = cabin.farmhand.Value;
				if (farmer != null)
				{
					if (farmer.isActive())
					{
						farmer = otherFarmers[farmer.UniqueMultiplayerID];
					}
					yield return farmer;
				}
			}
		}

		public static FarmerCollection getOnlineFarmers()
		{
			return _onlineFarmers;
		}

		public static void farmerFindsArtifact(int objectIndex)
		{
			player.addItemToInventoryBool(new Object(objectIndex, 1));
		}

		public static bool doesHUDMessageExist(string s)
		{
			for (int i = 0; i < hudMessages.Count; i++)
			{
				if (s.Equals(hudMessages[i].message))
				{
					return true;
				}
			}
			return false;
		}

		public static void addHUDMessage(HUDMessage message)
		{
			if (message.type != null || message.whatType != 0)
			{
				for (int i = 0; i < hudMessages.Count; i++)
				{
					if (message.type != null && hudMessages[i].type != null && hudMessages[i].type.Equals(message.type) && hudMessages[i].add == message.add)
					{
						hudMessages[i].number = (message.add ? (hudMessages[i].number + message.number) : (hudMessages[i].number - message.number));
						hudMessages[i].timeLeft = 3500f;
						hudMessages[i].transparency = 1f;
						if (hudMessages[i].messageSubject is Object && message.messageSubject is Object)
						{
							hudMessages[i].messageSubject = message.messageSubject;
						}
						return;
					}
					if (message.whatType == hudMessages[i].whatType && message.whatType != 1 && message.message != null && message.message.Equals(hudMessages[i].message))
					{
						hudMessages[i].timeLeft = message.timeLeft;
						hudMessages[i].transparency = 1f;
						return;
					}
				}
			}
			hudMessages.Add(message);
			for (int num = hudMessages.Count - 1; num >= 0; num--)
			{
				if (hudMessages[num].noIcon)
				{
					HUDMessage item = hudMessages[num];
					hudMessages.RemoveAt(num);
					hudMessages.Add(item);
				}
			}
		}

		public static void nextMineLevel()
		{
			warpFarmer("UndergroundMine" + (CurrentMineLevel + 1), 16, 16, flip: false);
		}

		public static void showSwordswipeAnimation(int direction, Vector2 source, float animationSpeed, bool flip)
		{
			switch (direction)
			{
			case 0:
				currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(-1, animationSpeed, 5, 1, new Vector2(source.X + 32f, source.Y), flicker: false, flipped: false, !flip, -(float)Math.PI / 2f));
				break;
			case 1:
				currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(-1, animationSpeed, 5, 1, new Vector2(source.X + 96f + 16f, source.Y + 48f), flicker: false, flip, verticalFlipped: false, flip ? (-(float)Math.PI) : 0f));
				break;
			case 2:
				currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(-1, animationSpeed, 5, 1, new Vector2(source.X + 32f, source.Y + 128f), flicker: false, flipped: false, !flip, (float)Math.PI / 2f));
				break;
			case 3:
				currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(-1, animationSpeed, 5, 1, new Vector2(source.X - 32f - 16f, source.Y + 48f), flicker: false, !flip, verticalFlipped: false, flip ? (-(float)Math.PI) : 0f));
				break;
			}
		}

		public static void removeSquareDebrisFromTile(int tileX, int tileY)
		{
			currentLocation.debris.Filter((Debris debris) => (Debris.DebrisType)debris.debrisType != Debris.DebrisType.SQUARES || (int)(debris.Chunks[0].position.X / 64f) != tileX || debris.chunkFinalYLevel / 64 != tileY);
		}

		public static void removeDebris(Debris.DebrisType type)
		{
			currentLocation.debris.Filter((Debris debris) => (Debris.DebrisType)debris.debrisType != type);
		}

		public static void toolAnimationDone(Farmer who)
		{
			float stamina = player.Stamina;
			if (who.CurrentTool == null)
			{
				return;
			}
			if (who.Stamina > 0f)
			{
				int power = (int)((toolHold + 20f) / 600f) + 1;
				Vector2 toolLocation = who.GetToolLocation();
				if (who.CurrentTool is FishingRod && ((FishingRod)who.CurrentTool).isFishing)
				{
					who.canReleaseTool = false;
				}
				else if (!(who.CurrentTool is FishingRod))
				{
					who.UsingTool = false;
					if (who.CurrentTool.Name.Contains("Seeds"))
					{
						if (!eventUp)
						{
							who.CurrentTool.DoFunction(currentLocation, who.getStandingX(), who.getStandingY(), power, who);
							if (((Seeds)who.CurrentTool).NumberInStack <= 0)
							{
								who.removeItemFromInventory(who.CurrentTool);
							}
						}
					}
					else if (who.CurrentTool.Name.Equals("Watering Can"))
					{
						switch (who.FacingDirection)
						{
						case 0:
						case 2:
							who.CurrentTool.DoFunction(currentLocation, (int)toolLocation.X, (int)toolLocation.Y, power, who);
							break;
						case 1:
						case 3:
							who.CurrentTool.DoFunction(currentLocation, (int)toolLocation.X, (int)toolLocation.Y, power, who);
							break;
						}
					}
					else if (who.CurrentTool is MeleeWeapon)
					{
						who.CurrentTool.CurrentParentTileIndex = who.CurrentTool.IndexOfMenuItemView;
					}
					else
					{
						if (who.CurrentTool.Name.Equals("Wand"))
						{
							who.CurrentTool.CurrentParentTileIndex = who.CurrentTool.IndexOfMenuItemView;
						}
						who.CurrentTool.DoFunction(currentLocation, (int)toolLocation.X, (int)toolLocation.Y, power, who);
					}
				}
				else
				{
					who.UsingTool = false;
				}
			}
			else if ((bool)who.CurrentTool.instantUse)
			{
				who.CurrentTool.DoFunction(currentLocation, 0, 0, 0, who);
			}
			else
			{
				who.UsingTool = false;
			}
			who.lastClick = Vector2.Zero;
			toolHold = 0f;
			if (who.IsLocalPlayer && !GetKeyboardState().IsKeyDown(Keys.LeftShift))
			{
				who.setRunning(options.autoRun);
			}
			if (!who.UsingTool && who.FarmerSprite.PauseForSingleAnimation)
			{
				who.FarmerSprite.StopAnimation();
			}
			if (player.Stamina <= 0f && stamina > 0f)
			{
				player.doEmote(36);
			}
		}

		public static bool pressActionButton(KeyboardState currentKBState, MouseState currentMouseState, GamePadState currentPadState)
		{
			if (IsChatting)
			{
				currentKBState = default(KeyboardState);
			}
			if (dialogueTyping)
			{
				bool flag = true;
				dialogueTyping = false;
				if (currentSpeaker != null)
				{
					currentDialogueCharacterIndex = currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue().Length;
				}
				else if (currentObjectDialogue.Count > 0)
				{
					currentDialogueCharacterIndex = currentObjectDialogue.Peek().Length;
				}
				else
				{
					flag = false;
				}
				dialogueTypingInterval = 0;
				oldKBState = currentKBState;
				oldMouseState = input.GetMouseState();
				oldPadState = currentPadState;
				if (flag)
				{
					playSound("dialogueCharacterClose");
					return false;
				}
			}
			if (dialogueUp && numberOfSelectedItems == -1)
			{
				if (isQuestion)
				{
					isQuestion = false;
					if (currentSpeaker != null)
					{
						if (currentSpeaker.CurrentDialogue.Peek().chooseResponse(questionChoices[currentQuestionChoice]))
						{
							currentDialogueCharacterIndex = 1;
							dialogueTyping = true;
							oldKBState = currentKBState;
							oldMouseState = input.GetMouseState();
							oldPadState = currentPadState;
							return false;
						}
					}
					else
					{
						dialogueUp = false;
						if (eventUp && currentLocation.afterQuestion == null)
						{
							currentLocation.currentEvent.answerDialogue(currentLocation.lastQuestionKey, currentQuestionChoice);
							currentQuestionChoice = 0;
							oldKBState = currentKBState;
							oldMouseState = input.GetMouseState();
							oldPadState = currentPadState;
						}
						else if (currentLocation.answerDialogue(questionChoices[currentQuestionChoice]))
						{
							currentQuestionChoice = 0;
							oldKBState = currentKBState;
							oldMouseState = input.GetMouseState();
							oldPadState = currentPadState;
							return false;
						}
						if (dialogueUp)
						{
							currentDialogueCharacterIndex = 1;
							dialogueTyping = true;
							oldKBState = currentKBState;
							oldMouseState = input.GetMouseState();
							oldPadState = currentPadState;
							return false;
						}
					}
					currentQuestionChoice = 0;
				}
				string text = null;
				if (currentSpeaker != null)
				{
					if (currentSpeaker.immediateSpeak)
					{
						currentSpeaker.immediateSpeak = false;
						return false;
					}
					text = ((currentSpeaker.CurrentDialogue.Count > 0) ? currentSpeaker.CurrentDialogue.Peek().exitCurrentDialogue() : null);
				}
				if (text == null)
				{
					if (currentSpeaker != null && currentSpeaker.CurrentDialogue.Count > 0 && currentSpeaker.CurrentDialogue.Peek().isOnFinalDialogue() && currentSpeaker.CurrentDialogue.Count > 0)
					{
						currentSpeaker.CurrentDialogue.Pop();
					}
					dialogueUp = false;
					if (messagePause)
					{
						pauseTime = 500f;
					}
					if (currentObjectDialogue.Count > 0)
					{
						currentObjectDialogue.Dequeue();
					}
					currentDialogueCharacterIndex = 0;
					if (currentObjectDialogue.Count > 0)
					{
						dialogueUp = true;
						questionChoices.Clear();
						oldKBState = currentKBState;
						oldMouseState = input.GetMouseState();
						oldPadState = currentPadState;
						dialogueTyping = true;
						return false;
					}
					tvStation = -1;
					if (currentSpeaker != null && !currentSpeaker.Name.Equals("Gunther") && !eventUp && !currentSpeaker.doingEndOfRouteAnimation)
					{
						currentSpeaker.doneFacingPlayer(player);
					}
					currentSpeaker = null;
					if (!eventUp)
					{
						player.CanMove = true;
					}
					else if (currentLocation.currentEvent.CurrentCommand > 0 || currentLocation.currentEvent.specialEventVariable1)
					{
						if (!isFestival() || !currentLocation.currentEvent.canMoveAfterDialogue())
						{
							currentLocation.currentEvent.CurrentCommand++;
						}
						else
						{
							player.CanMove = true;
						}
					}
					questionChoices.Clear();
					playSound("smallSelect");
				}
				else
				{
					playSound("smallSelect");
					currentDialogueCharacterIndex = 0;
					dialogueTyping = true;
					checkIfDialogueIsQuestion();
				}
				oldKBState = currentKBState;
				oldMouseState = input.GetMouseState();
				oldPadState = currentPadState;
				if (questOfTheDay != null && (bool)questOfTheDay.accepted && questOfTheDay is SocializeQuest)
				{
					((SocializeQuest)questOfTheDay).checkIfComplete(null, -1, -1);
				}
				_ = afterDialogues;
				return false;
			}
			if (currentBillboard != 0)
			{
				currentBillboard = 0;
				player.CanMove = true;
				oldKBState = currentKBState;
				oldMouseState = input.GetMouseState();
				oldPadState = currentPadState;
				return false;
			}
			if (!player.UsingTool && !pickingTool && !menuUp && (!eventUp || (currentLocation.currentEvent != null && currentLocation.currentEvent.playerControlSequence)) && !nameSelectUp && numberOfSelectedItems == -1 && !fadeToBlack)
			{
				if (wasMouseVisibleThisFrame && currentLocation is IAnimalLocation)
				{
					Vector2 position = new Vector2(getOldMouseX() + viewport.X, getOldMouseY() + viewport.Y);
					if (Utility.withinRadiusOfPlayer((int)position.X, (int)position.Y, 1, player))
					{
						if ((currentLocation as IAnimalLocation).CheckPetAnimal(position, player))
						{
							return true;
						}
						if (didPlayerJustRightClick(ignoreNonMouseHeldInput: true) && (currentLocation as IAnimalLocation).CheckInspectAnimal(position, player))
						{
							return true;
						}
					}
				}
				Vector2 vector;
				if ((currentLocation.tapToMove.TileClicked.X == -1f && currentLocation.tapToMove.TileClicked.Y == -1f) || virtualJoypad.ButtonAPressed || options.weaponControl == 2 || options.weaponControl == 3 || (options.weaponControl == 4 && virtualJoypad.ButtonBPressed) || (options.weaponControl == 5 && virtualJoypad.ButtonBPressed) || (options.weaponControl == 6 && virtualJoypad.ButtonBPressed) || (options.weaponControl == 8 && virtualJoypad.ButtonBPressed))
				{
					vector = new Vector2(getOldMouseX() + viewport.X, getOldMouseY() + viewport.Y) / 64f;
					if (mouseCursorTransparency == 0f || !wasMouseVisibleThisFrame || (!lastCursorMotionWasMouse && (player.ActiveObject == null || (!player.ActiveObject.isPlaceable() && player.ActiveObject.Category != -74))))
					{
						vector = player.GetGrabTile();
						if (vector.Equals(player.getTileLocation()))
						{
							vector = Utility.getTranslatedVector2(vector, player.FacingDirection, 1f);
						}
					}
					if (!Utility.tileWithinRadiusOfPlayer((int)vector.X, (int)vector.Y, 1, player))
					{
						vector = player.GetGrabTile();
						if (vector.Equals(player.getTileLocation()) && isAnyGamePadButtonBeingPressed())
						{
							vector = Utility.getTranslatedVector2(vector, player.FacingDirection, 1f);
						}
					}
				}
				else
				{
					vector = currentLocation.tapToMove.TileClicked;
				}
				Vector2 vector2 = vector;
				bool flag2 = false;
				if (eventUp && !isFestival())
				{
					if (CurrentEvent != null)
					{
						CurrentEvent.receiveActionPress((int)vector.X, (int)vector.Y);
					}
					oldKBState = currentKBState;
					oldMouseState = input.GetMouseState();
					oldPadState = currentPadState;
					return false;
				}
				if (tryToCheckAt(vector, player))
				{
					return false;
				}
				if (player.isRidingHorse())
				{
					player.mount.checkAction(player, player.currentLocation);
					return false;
				}
				if (!player.canMove)
				{
					return false;
				}
				if (!flag2)
				{
					NPC nPC = player.currentLocation.isCharacterAtTile(vector);
					if (nPC != null && nPC != null)
					{
						flag2 = true;
					}
				}
				bool flag3 = false;
				if (player.ActiveObject != null && !(player.ActiveObject is Furniture))
				{
					if (player.ActiveObject.performUseAction(currentLocation))
					{
						player.reduceActiveItemByOne();
						oldKBState = currentKBState;
						oldMouseState = input.GetMouseState();
						oldPadState = currentPadState;
						return false;
					}
					int stack = player.ActiveObject.Stack;
					isCheckingNonMousePlacement = !IsPerformingMousePlacement();
					if (isOneOfTheseKeysDown(currentKBState, options.actionButton))
					{
						isCheckingNonMousePlacement = true;
					}
					Vector2 nearbyValidPlacementPosition = Utility.GetNearbyValidPlacementPosition(player, currentLocation, player.ActiveObject, (int)vector.X * 64 + 32, (int)vector.Y * 64 + 32);
					if (!isCheckingNonMousePlacement && player.ActiveObject is Wallpaper && Utility.tryToPlaceItem(currentLocation, player.ActiveObject, (int)vector2.X * 64, (int)vector2.Y * 64))
					{
						isCheckingNonMousePlacement = false;
						return true;
					}
					if (Utility.tryToPlaceItem(currentLocation, player.ActiveObject, (int)nearbyValidPlacementPosition.X, (int)nearbyValidPlacementPosition.Y))
					{
						isCheckingNonMousePlacement = false;
						return true;
					}
					if (!eventUp && (player.ActiveObject == null || player.ActiveObject.Stack < stack || player.ActiveObject.isPlaceable()))
					{
						flag3 = true;
					}
					isCheckingNonMousePlacement = false;
				}
				if (!flag3 && !flag2)
				{
					vector.Y += 1f;
					if (player.FacingDirection >= 0 && player.FacingDirection <= 3)
					{
						Vector2 value = vector - player.getTileLocation();
						if (value.X > 0f || value.Y > 0f)
						{
							value.Normalize();
						}
						if (Vector2.Dot(Utility.DirectionsTileVectors[player.FacingDirection], value) >= 0f && tryToCheckAt(vector, player))
						{
							return false;
						}
					}
					if (player.ActiveObject != null && player.ActiveObject is Furniture && !eventUp)
					{
						(player.ActiveObject as Furniture).rotate();
						playSound("dwoop");
						oldKBState = currentKBState;
						oldMouseState = input.GetMouseState();
						oldPadState = currentPadState;
						return false;
					}
					vector.Y -= 2f;
					if (player.FacingDirection >= 0 && player.FacingDirection <= 3 && !flag2)
					{
						Vector2 value2 = vector - player.getTileLocation();
						if (value2.X > 0f || value2.Y > 0f)
						{
							value2.Normalize();
						}
						if (Vector2.Dot(Utility.DirectionsTileVectors[player.FacingDirection], value2) >= 0f && tryToCheckAt(vector, player))
						{
							return false;
						}
					}
					if (player.ActiveObject != null && player.ActiveObject is Furniture && !eventUp)
					{
						(player.ActiveObject as Furniture).rotate();
						playSound("dwoop");
						oldKBState = currentKBState;
						oldMouseState = input.GetMouseState();
						oldPadState = currentPadState;
						return false;
					}
					vector = player.getTileLocation();
					if (tryToCheckAt(vector, player))
					{
						return false;
					}
					if (player.ActiveObject != null && player.ActiveObject is Furniture && !eventUp)
					{
						(player.ActiveObject as Furniture).rotate();
						playSound("dwoop");
						oldKBState = currentKBState;
						oldMouseState = input.GetMouseState();
						oldPadState = currentPadState;
						return false;
					}
				}
				if (!player.isEating && player.ActiveObject != null && !dialogueUp && !eventUp && !player.canOnlyWalk && !player.FarmerSprite.PauseForSingleAnimation && !fadeToBlack && player.ActiveObject.Edibility != -300 && didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
				{
					if (player.team.SpecialOrderRuleActive("SC_NO_FOOD") && player.currentLocation is MineShaft && (player.currentLocation as MineShaft).getMineArea() == 121)
					{
						addHUDMessage(new HUDMessage(content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"), 3));
						return false;
					}
					if (buffsDisplay.hasBuff(25) && player.ActiveObject != null && !player.ActiveObject.HasContextTag("ginger_item"))
					{
						addHUDMessage(new HUDMessage(content.LoadString("Strings\\StringsFromCSFiles:Nauseous_CantEat"), 3));
						return false;
					}
					player.faceDirection(2);
					player.itemToEat = player.ActiveObject;
					player.FarmerSprite.setCurrentSingleAnimation(304);
					currentLocation.createQuestionDialogue((objectInformation[player.ActiveObject.parentSheetIndex].Split('/').Length > 6 && objectInformation[player.ActiveObject.parentSheetIndex].Split('/')[6].Equals("drink")) ? content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3159", player.ActiveObject.DisplayName) : content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3160", player.ActiveObject.DisplayName), currentLocation.createYesNoResponses(), "Eat");
					oldKBState = currentKBState;
					oldMouseState = input.GetMouseState();
					oldPadState = currentPadState;
					return false;
				}
			}
			else if (numberOfSelectedItems != -1)
			{
				tryToBuySelectedItems();
				playSound("smallSelect");
				oldKBState = currentKBState;
				oldMouseState = input.GetMouseState();
				oldPadState = currentPadState;
				return false;
			}
			if (player.CurrentTool != null && player.CurrentTool is MeleeWeapon && player.CanMove && !player.canOnlyWalk && !eventUp && !player.onBridge && didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
			{
				((MeleeWeapon)player.CurrentTool).animateSpecialMove(player);
				return false;
			}
			return true;
		}

		public static bool IsPerformingMousePlacement()
		{
			if (mouseCursorTransparency == 0f || !wasMouseVisibleThisFrame || (!lastCursorMotionWasMouse && (player.ActiveObject == null || (!player.ActiveObject.isPlaceable() && player.ActiveObject.Category != -74 && !player.ActiveObject.isSapling()))))
			{
				return false;
			}
			return true;
		}

		public static Vector2 GetPlacementGrabTile()
		{
			if (!IsPerformingMousePlacement())
			{
				return player.GetGrabTile();
			}
			return new Vector2(getOldMouseX() + viewport.X, getOldMouseY() + viewport.Y) / 64f;
		}

		public static bool tryToCheckAt(Vector2 grabTile, Farmer who)
		{
			if (player.onBridge.Value)
			{
				return false;
			}
			haltAfterCheck = true;
			int tileRadius = 1;
			if (mouseCursor == 5)
			{
				tileRadius = 2;
			}
			if (Utility.tileWithinRadiusOfPlayer((int)grabTile.X, (int)grabTile.Y, tileRadius, player) && hooks.OnGameLocation_CheckAction(currentLocation, new Location((int)grabTile.X, (int)grabTile.Y), viewport, who, () => currentLocation.checkAction(new Location((int)grabTile.X, (int)grabTile.Y), viewport, who)))
			{
				updateCursorTileHint();
				who.lastGrabTile = grabTile;
				if (who.CanMove && haltAfterCheck)
				{
					who.faceGeneralDirection(grabTile * 64f);
					who.Halt();
				}
				oldKBState = GetKeyboardState();
				oldMouseState = input.GetMouseState();
				oldPadState = input.GetGamePadState();
				return true;
			}
			return false;
		}

		public static void pressSwitchToolButton()
		{
			if (player.netItemStowed.Value)
			{
				player.netItemStowed.Set(newValue: false);
				player.UpdateItemStow();
			}
			int num = ((input.GetMouseState().ScrollWheelValue > oldMouseState.ScrollWheelValue) ? (-1) : ((input.GetMouseState().ScrollWheelValue < oldMouseState.ScrollWheelValue) ? 1 : 0));
			if (options.gamepadControls && num == 0)
			{
				if (input.GetGamePadState().IsButtonDown(Buttons.LeftTrigger))
				{
					num = -1;
				}
				else if (input.GetGamePadState().IsButtonDown(Buttons.RightTrigger))
				{
					num = 1;
				}
			}
			if (options.invertScrollDirection)
			{
				num *= -1;
			}
			if (num == 0)
			{
				return;
			}
			player.CurrentToolIndex = (player.CurrentToolIndex + num) % 12;
			if (player.CurrentToolIndex < 0)
			{
				player.CurrentToolIndex = 11;
			}
			for (int i = 0; i < 12; i++)
			{
				if (player.CurrentItem != null)
				{
					break;
				}
				player.CurrentToolIndex = (num + player.CurrentToolIndex) % 12;
				if (player.CurrentToolIndex < 0)
				{
					player.CurrentToolIndex = 11;
				}
			}
			playSound("toolSwap");
			if (player.ActiveObject != null)
			{
				player.showCarrying();
			}
			else
			{
				player.showNotCarrying();
			}
			if (player.CurrentTool != null && !player.CurrentTool.Name.Equals("Seeds") && !player.CurrentTool.Name.Contains("Sword") && !player.CurrentTool.instantUse)
			{
				player.CurrentTool.CurrentParentTileIndex = player.CurrentTool.CurrentParentTileIndex - player.CurrentTool.CurrentParentTileIndex % 8 + 2;
			}
		}

		public static void switchToolAnimation()
		{
			pickToolInterval = 0f;
			player.CanMove = false;
			pickingTool = true;
			playSound("toolSwap");
			switch (player.FacingDirection)
			{
			case 0:
				player.FarmerSprite.setCurrentFrame(196);
				break;
			case 1:
				player.FarmerSprite.setCurrentFrame(194);
				break;
			case 2:
				player.FarmerSprite.setCurrentFrame(192);
				break;
			case 3:
				player.FarmerSprite.setCurrentFrame(198);
				break;
			}
			if (player.CurrentTool != null && !player.CurrentTool.Name.Equals("Seeds") && !player.CurrentTool.Name.Contains("Sword") && !player.CurrentTool.instantUse)
			{
				player.CurrentTool.CurrentParentTileIndex = player.CurrentTool.CurrentParentTileIndex - player.CurrentTool.CurrentParentTileIndex % 8 + 2;
			}
			if (player.ActiveObject != null)
			{
				player.showCarrying();
			}
		}

		public static bool pressUseToolButton()
		{
			bool didInitiateItemStow = game1._didInitiateItemStow;
			game1._didInitiateItemStow = false;
			if (fadeToBlack)
			{
				return false;
			}
			player.toolPower = 0;
			player.toolHold = 0;
			bool flag = false;
			if (player.CurrentTool == null && player.ActiveObject == null)
			{
				Vector2 key = player.GetToolLocation() / 64f;
				key.X = (int)key.X;
				key.Y = (int)key.Y;
				if (currentLocation.Objects.ContainsKey(key))
				{
					Object @object = currentLocation.Objects[key];
					if (!@object.readyForHarvest && @object.heldObject.Value == null && !(@object is Fence) && !(@object is CrabPot) && @object.type != null && (@object.type.Equals("Crafting") || @object.type.Equals("interactive")) && !@object.name.Equals("Twig"))
					{
						flag = true;
						@object.setHealth(@object.getHealth() - 1);
						@object.shakeTimer = 300;
						currentLocation.playSound("hammer");
						if (@object.getHealth() < 2)
						{
							currentLocation.playSound("hammer");
							if (@object.getHealth() < 1)
							{
								Tool tool = new Pickaxe();
								tool.DoFunction(currentLocation, -1, -1, 0, player);
								if (@object.performToolAction(tool, currentLocation))
								{
									@object.performRemoveAction(@object.tileLocation, currentLocation);
									if (@object.type.Equals("Crafting") && (int)@object.fragility != 2)
									{
										currentLocation.debris.Add(new Debris(@object.bigCraftable ? (-@object.ParentSheetIndex) : @object.ParentSheetIndex, player.GetToolLocation(), new Vector2(player.GetBoundingBox().Center.X, player.GetBoundingBox().Center.Y)));
									}
									currentLocation.Objects.Remove(key);
									return true;
								}
							}
						}
					}
				}
			}
			if (currentMinigame == null && !player.UsingTool && (player.IsSitting() || player.isRidingHorse() || player.onBridge.Value || dialogueUp || (eventUp && !CurrentEvent.canPlayerUseTool() && (!currentLocation.currentEvent.playerControlSequence || (activeClickableMenu == null && currentMinigame == null))) || (player.CurrentTool != null && currentLocation.doesPositionCollideWithCharacter(Utility.getRectangleCenteredAt(player.GetToolLocation(), 64), ignoreMonsters: true) != null && currentLocation.doesPositionCollideWithCharacter(Utility.getRectangleCenteredAt(player.GetToolLocation(), 64), ignoreMonsters: true).isVillager())))
			{
				pressActionButton(GetKeyboardState(), input.GetMouseState(), input.GetGamePadState());
				return false;
			}
			if (player.canOnlyWalk)
			{
				return true;
			}
			Vector2 position;
			if (currentLocation.tapToMove.ClickPoint.X == -1f && currentLocation.tapToMove.ClickPoint.Y == -1f)
			{
				position = ((!wasMouseVisibleThisFrame) ? player.GetToolLocation() : new Vector2(getOldMouseX() + viewport.X, getOldMouseY() + viewport.Y));
				if (isAnyGamePadButtonBeingPressed() || isAnyGamePadButtonBeingHeld())
				{
					if (player.ActiveObject != null)
					{
						position = player.GetLocationNextToWhereYoureFacing(player.ActiveObject.boundingBox.Width);
					}
					else
					{
						position = player.GetLocationNextToWhereYoureFacing();
					}
				}
			}
			else
			{
				position = ((!wasMouseVisibleThisFrame) ? player.GetToolLocation() : currentLocation.tapToMove.ClickPoint);
			}
			if (Utility.canGrabSomethingFromHere((int)position.X, (int)position.Y, player))
			{
				Vector2 tile;
				if (currentLocation.tapToMove.ClickPoint.X == -1f && currentLocation.tapToMove.ClickPoint.Y == -1f)
				{
					tile = new Vector2((getOldMouseX() + viewport.X) / 64, (getOldMouseY() + viewport.Y) / 64);
				}
				else
				{
					tile = currentLocation.tapToMove.TileClicked;
				}
				if (hooks.OnGameLocation_CheckAction(currentLocation, new Location((int)tile.X, (int)tile.Y), viewport, player, () => currentLocation.checkAction(new Location((int)tile.X, (int)tile.Y), viewport, player)))
				{
					updateCursorTileHint();
					return true;
				}
				if (currentLocation.terrainFeatures.ContainsKey(tile))
				{
					currentLocation.terrainFeatures[tile].performUseAction(tile, currentLocation);
					return true;
				}
				return false;
			}
			if (currentLocation.leftClick((int)position.X, (int)position.Y, player))
			{
				return true;
			}
			isCheckingNonMousePlacement = !IsPerformingMousePlacement();
			if (player.ActiveObject != null)
			{
				if (options.allowStowing && CanPlayerStowItem(GetPlacementGrabTile()))
				{
					if (didPlayerJustLeftClick() || didInitiateItemStow)
					{
						game1._didInitiateItemStow = true;
						playSound("stoneStep");
						player.netItemStowed.Set(newValue: true);
						return true;
					}
					return true;
				}
				if (Utility.withinRadiusOfPlayer((int)position.X, (int)position.Y, 1, player) && hooks.OnGameLocation_CheckAction(currentLocation, new Location((int)position.X / 64, (int)position.Y / 64), viewport, player, () => currentLocation.checkAction(new Location((int)position.X / 64, (int)position.Y / 64), viewport, player)))
				{
					return true;
				}
				Vector2 vector;
				if (currentLocation.tapToMove.grabTile != Vector2.Zero)
				{
					vector = currentLocation.tapToMove.grabTile;
					currentLocation.tapToMove.grabTile = Vector2.Zero;
				}
				else
				{
					vector = GetPlacementGrabTile();
				}
				Vector2 nearbyValidPlacementPosition = Utility.GetNearbyValidPlacementPosition(player, currentLocation, player.ActiveObject, (int)vector.X * 64, (int)vector.Y * 64);
				if (Utility.tryToPlaceItem(currentLocation, player.ActiveObject, (int)nearbyValidPlacementPosition.X, (int)nearbyValidPlacementPosition.Y))
				{
					isCheckingNonMousePlacement = false;
					return true;
				}
				isCheckingNonMousePlacement = false;
			}
			if (currentLocation.LowPriorityLeftClick((int)position.X, (int)position.Y, player))
			{
				return true;
			}
			if (options.allowStowing && player.netItemStowed.Value && !flag && (didInitiateItemStow || didPlayerJustLeftClick(ignoreNonMouseHeldInput: true)))
			{
				game1._didInitiateItemStow = true;
				playSound("toolSwap");
				player.netItemStowed.Set(newValue: false);
				return true;
			}
			if (player.UsingTool)
			{
				player.lastClick = new Vector2((int)position.X, (int)position.Y);
				player.CurrentTool.DoFunction(player.currentLocation, (int)player.lastClick.X, (int)player.lastClick.Y, 1, player);
				return true;
			}
			if (player.ActiveObject == null && !player.isEating && player.CurrentTool != null)
			{
				if (player.Stamina <= 20f && player.CurrentTool != null && !(player.CurrentTool is MeleeWeapon) && !eventUp)
				{
					staminaShakeTimer = 1000;
					for (int i = 0; i < 4; i++)
					{
						int num = uiViewport.Width - xEdge - 50 + random.Next(16);
						int num2 = uiViewport.Height - 232 - (player.MaxStamina - 270);
						uiOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(366, 412, 5, 6), new Vector2(num, num2), flipped: false, 0.012f, Color.SkyBlue)
						{
							motion = new Vector2(-2f, -10f),
							acceleration = new Vector2(0f, 0.5f),
							local = true,
							scale = 4 + random.Next(-1, 0),
							delayBeforeAnimationStart = i * 30
						});
					}
				}
				if (player.CurrentTool == null || !(player.CurrentTool is MeleeWeapon) || didPlayerJustLeftClick(ignoreNonMouseHeldInput: true))
				{
					int facingDirection = player.FacingDirection;
					Vector2 toolLocation = player.GetToolLocation(position);
					if (!(player.CurrentTool is MeleeWeapon) || player.CurrentTool.InitialParentTileIndex == 47 || options.weaponControl != 0)
					{
						player.FacingDirection = player.getGeneralDirectionTowards(new Vector2((int)toolLocation.X, (int)toolLocation.Y));
					}
					player.lastClick = new Vector2((int)position.X, (int)position.Y);
					player.BeginUsingTool();
					if (!player.usingTool)
					{
						player.FacingDirection = facingDirection;
					}
					else if (player.FarmerSprite.IsPlayingBasicAnimation(facingDirection, carrying: true) || player.FarmerSprite.IsPlayingBasicAnimation(facingDirection, carrying: false))
					{
						player.FarmerSprite.StopAnimation();
					}
				}
			}
			return false;
		}

		public static bool CanPlayerStowItem(Vector2 position)
		{
			if (player.ActiveObject == null)
			{
				return false;
			}
			if ((bool)player.ActiveObject.bigCraftable)
			{
				return false;
			}
			if (player.ActiveObject is Furniture)
			{
				return false;
			}
			if (player.ActiveObject != null && (player.ActiveObject.Category == -74 || player.ActiveObject.Category == -19))
			{
				Vector2 nearbyValidPlacementPosition = Utility.GetNearbyValidPlacementPosition(player, currentLocation, player.ActiveObject, (int)position.X * 64, (int)position.Y * 64);
				if (Utility.playerCanPlaceItemHere(player.currentLocation, player.ActiveObject, (int)nearbyValidPlacementPosition.X, (int)nearbyValidPlacementPosition.Y, player) && ((!Object.isWildTreeSeed(player.ActiveObject.ParentSheetIndex) && !player.ActiveObject.isSapling()) || IsPerformingMousePlacement()))
				{
					return false;
				}
			}
			return true;
		}

		public static int getMouseXRaw()
		{
			return input.GetMouseState().X;
		}

		public static int getMouseYRaw()
		{
			return input.GetMouseState().Y;
		}

		public static bool IsOnMainThread()
		{
			if (Thread.CurrentThread != null)
			{
				return !Thread.CurrentThread.IsBackground;
			}
			return false;
		}

		public static void PushUIMode()
		{
			if (!IsOnMainThread())
			{
				return;
			}
			uiModeCount++;
			if (uiModeCount <= 0 || uiMode)
			{
				return;
			}
			uiMode = true;
			if (game1.isDrawing && IsOnMainThread())
			{
				if (game1.uiScreen != null && !game1.uiScreen.IsDisposed)
				{
					nonUIRenderTarget = Utility.GetFirstRenderTarget2D(graphics.GraphicsDevice);
					SetRenderTarget(game1.uiScreen);
				}
				if (isRenderingScreenBuffer)
				{
					SetRenderTarget(null);
				}
			}
			xTile.Dimensions.Rectangle rectangle = new xTile.Dimensions.Rectangle(0, 0, (int)Math.Ceiling((float)viewport.Width * options.zoomLevel / options.uiScale), (int)Math.Ceiling((float)viewport.Height * options.zoomLevel / options.uiScale));
			rectangle.X = viewport.X;
			rectangle.Y = viewport.Y;
			uiViewport = rectangle;
		}

		public static void PopUIMode()
		{
			if (!IsOnMainThread())
			{
				return;
			}
			uiModeCount--;
			if (uiModeCount > 0 || !uiMode)
			{
				return;
			}
			if (game1.isDrawing)
			{
				RenderTarget2D firstRenderTarget2D = Utility.GetFirstRenderTarget2D(graphics.GraphicsDevice);
				if (firstRenderTarget2D != null && firstRenderTarget2D == game1.uiScreen)
				{
					if (nonUIRenderTarget != null && !nonUIRenderTarget.IsDisposed)
					{
						SetRenderTarget(nonUIRenderTarget);
					}
					else
					{
						SetRenderTarget(null);
					}
				}
				if (isRenderingScreenBuffer)
				{
					SetRenderTarget(null);
				}
			}
			nonUIRenderTarget = null;
			uiMode = false;
		}

		public static void SetRenderTarget(RenderTarget2D target)
		{
			if (!isRenderingScreenBuffer && IsOnMainThread())
			{
				graphics.GraphicsDevice.SetRenderTarget(target);
			}
		}

		public static void InUIMode(Action action)
		{
			PushUIMode();
			try
			{
				action();
			}
			finally
			{
				PopUIMode();
			}
		}

		public static void StartWorldDrawInUI(SpriteBatch b)
		{
			_oldUIModeCount = 0;
			if (uiMode)
			{
				_oldUIModeCount = uiModeCount;
				b?.End();
				while (uiModeCount > 0)
				{
					PopUIMode();
				}
				b?.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			}
		}

		public static void EndWorldDrawInUI(SpriteBatch b)
		{
			if (_oldUIModeCount > 0)
			{
				b?.End();
				for (int i = 0; i < _oldUIModeCount; i++)
				{
					PushUIMode();
				}
				b?.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			}
			_oldUIModeCount = 0;
		}

		public static int getMouseX()
		{
			return getMouseX(uiMode);
		}

		public static int getMouseX(bool ui_scale)
		{
			if (ui_scale)
			{
				return (int)((float)input.GetMouseState().X / options.uiScale);
			}
			return (int)((float)input.GetMouseState().X * (1f / options.zoomLevel));
		}

		public static int getOldMouseX()
		{
			return getOldMouseX(uiMode);
		}

		public static int getOldMouseX(bool ui_scale)
		{
			if (ui_scale)
			{
				return (int)((float)oldMouseState.X / options.uiScale);
			}
			return (int)((float)oldMouseState.X * (1f / options.zoomLevel));
		}

		public static int getMouseY()
		{
			return getMouseY(uiMode);
		}

		public static int getMouseY(bool ui_scale)
		{
			if (ui_scale)
			{
				return (int)((float)input.GetMouseState().Y / options.uiScale);
			}
			return (int)((float)input.GetMouseState().Y * (1f / options.zoomLevel));
		}

		public static int getOldMouseY()
		{
			return getOldMouseY(uiMode);
		}

		public static int getOldMouseY(bool ui_scale)
		{
			if (ui_scale)
			{
				return (int)((float)oldMouseState.Y / options.uiScale);
			}
			return (int)((float)oldMouseState.Y * (1f / options.zoomLevel));
		}

		public static void pressAddItemToInventoryButton()
		{
		}

		public static int numberOfPlayers()
		{
			return _onlineFarmers.Count;
		}

		public static bool isFestival()
		{
			if (currentLocation != null && currentLocation.currentEvent != null)
			{
				return currentLocation.currentEvent.isFestival;
			}
			return false;
		}

		public bool parseDebugInput(string debugInput)
		{
			lastDebugInput = debugInput;
			debugInput = debugInput.Trim();
			string[] debugSplit = debugInput.Split(' ');
			try
			{
				if (panMode)
				{
					if (debugSplit[0].Equals("exit") || debugSplit[0].ToLower().Equals("panmode"))
					{
						panMode = false;
						viewportFreeze = false;
						panModeString = "";
						debugMode = false;
						debugOutput = "";
						panFacingDirectionWait = false;
						inputSimulator = null;
						return true;
					}
					if (debugSplit[0].Equals("clear"))
					{
						panModeString = "";
						debugOutput = "";
						panFacingDirectionWait = false;
						return true;
					}
					if (!panFacingDirectionWait)
					{
						int result = 0;
						if (int.TryParse(debugSplit[0], out result))
						{
							panModeString = panModeString + ((panModeString.Length > 0) ? "/" : "") + result + " ";
							debugOutput = panModeString + content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3191");
						}
						return true;
					}
					return false;
				}
				string text8;
				char c;
				switch (debugSplit[0].ToLowerInvariant())
				{
				case "growwildtrees":
				{
					for (int num6 = currentLocation.terrainFeatures.Count() - 1; num6 >= 0; num6--)
					{
						Vector2 vector2 = currentLocation.terrainFeatures.Keys.ElementAt(num6);
						if (currentLocation.terrainFeatures[vector2] is Tree)
						{
							(currentLocation.terrainFeatures[vector2] as Tree).growthStage.Value = 4;
							(currentLocation.terrainFeatures[vector2] as Tree).fertilized.Value = true;
							(currentLocation.terrainFeatures[vector2] as Tree).dayUpdate(currentLocation, vector2);
							(currentLocation.terrainFeatures[vector2] as Tree).fertilized.Value = false;
						}
					}
					break;
				}
				case "changestat":
					stats.stat_dictionary[debugSplit[1]] = Convert.ToUInt32(debugSplit[2]);
					break;
				case "eventtestspecific":
					eventTest = new EventTest(debugSplit);
					break;
				case "eventtest":
					eventTest = new EventTest((debugSplit.Count() > 1) ? debugSplit[1] : "", (debugSplit.Count() > 2) ? Convert.ToInt32(debugSplit[2]) : 0);
					break;
				case "getallquests":
				{
					Dictionary<int, string> dictionary8 = content.Load<Dictionary<int, string>>("Data\\Quests");
					foreach (KeyValuePair<int, string> item4 in dictionary8)
					{
						player.addQuest(item4.Key);
					}
					break;
				}
				case "movie":
				{
					List<List<Character>> group1 = new List<List<Character>>();
					List<List<Character>> group2 = new List<List<Character>>();
					int num = random.Next(20);
					Character character = null;
					string movie_title = ((debugSplit.Count() > 1) ? debugSplit[1] : "fall_movie_1");
					if (debugSplit.Length > 1)
					{
						character = Utility.fuzzyCharacterSearch(debugSplit[1]);
					}
					if (debugSplit.Length > 2)
					{
						movie_title = debugSplit[2];
					}
					if (character == null)
					{
						character = Utility.getTownNPCByGiftTasteIndex(num);
					}
					group1.Add(new List<Character> { player, character });
					num = (num + 1) % 25;
					int num2 = random.Next(3);
					for (int l = 0; l < num2; l++)
					{
						if (random.NextDouble() < 0.8)
						{
							if (random.NextDouble() < 0.5)
							{
								group1.Add(new List<Character>
								{
									Utility.getTownNPCByGiftTasteIndex(num),
									Utility.getTownNPCByGiftTasteIndex(num + 1)
								});
								num = (num + 2) % 25;
							}
							else
							{
								group1.Add(new List<Character> { Utility.getTownNPCByGiftTasteIndex(num) });
								num = (num + 1) % 25;
							}
						}
					}
					for (int m = 0; m < 2; m++)
					{
						if (random.NextDouble() < 0.8)
						{
							if (random.NextDouble() < 0.33)
							{
								group2.Add(new List<Character>
								{
									Utility.getTownNPCByGiftTasteIndex(num),
									Utility.getTownNPCByGiftTasteIndex(num + 1)
								});
								num = (num + 2) % 25;
							}
							else if (random.NextDouble() < 5.0)
							{
								group2.Add(new List<Character>
								{
									Utility.getTownNPCByGiftTasteIndex(num),
									Utility.getTownNPCByGiftTasteIndex(num + 1),
									Utility.getTownNPCByGiftTasteIndex(num + 2)
								});
								num = (num + 3) % 25;
							}
							else
							{
								group2.Add(new List<Character> { Utility.getTownNPCByGiftTasteIndex(num) });
								num = (num + 1) % 25;
							}
						}
					}
					MovieTheaterScreeningEvent event_generator = new MovieTheaterScreeningEvent();
					globalFadeToBlack(delegate
					{
						currentLocation.startEvent(event_generator.getMovieEvent(movie_title, group1, group2));
					});
					break;
				}
				case "everythingshop":
				{
					Dictionary<ISalable, int[]> dictionary4 = new Dictionary<ISalable, int[]>();
					dictionary4.Add(new Furniture(1226, Vector2.Zero), new int[2] { 0, 2147483647 });
					foreach (KeyValuePair<int, string> item5 in objectInformation)
					{
						try
						{
							dictionary4.Add(new Object(item5.Key, 1), new int[2] { 0, 2147483647 });
						}
						catch (Exception)
						{
							int num29 = 0;
						}
					}
					foreach (KeyValuePair<int, string> item6 in bigCraftablesInformation)
					{
						try
						{
							dictionary4.Add(new Object(Vector2.Zero, item6.Key), new int[2] { 0, 2147483647 });
						}
						catch (Exception)
						{
							int num30 = 0;
						}
					}
					Dictionary<int, string> dictionary5 = content.Load<Dictionary<int, string>>("Data\\weapons");
					foreach (KeyValuePair<int, string> item7 in dictionary5)
					{
						try
						{
							dictionary4.Add(new MeleeWeapon(item7.Key), new int[2] { 0, 2147483647 });
						}
						catch (Exception)
						{
							int num31 = 0;
						}
					}
					Dictionary<int, string> dictionary6 = content.Load<Dictionary<int, string>>("Data\\furniture");
					foreach (KeyValuePair<int, string> item8 in dictionary6)
					{
						try
						{
							dictionary4.Add(new Furniture(item8.Key, Vector2.Zero), new int[2] { 0, 2147483647 });
						}
						catch (Exception)
						{
							int num32 = 0;
						}
					}
					activeClickableMenu = new ShopMenu(dictionary4);
					break;
				}
				case "dating":
					player.friendshipData[debugSplit[1]].Status = FriendshipStatus.Dating;
					break;
				case "buff":
					buffsDisplay.addOtherBuff(new Buff(Convert.ToInt32(debugSplit[1])));
					break;
				case "clearbuffs":
					player.ClearBuffs();
					break;
				case "pausetime":
					isTimePaused = !isTimePaused;
					if (isTimePaused)
					{
						playSound("bigSelect");
					}
					else
					{
						playSound("bigDeSelect");
					}
					break;
				case "framebyframe":
				case "fbf":
					frameByFrame = !frameByFrame;
					if (frameByFrame)
					{
						playSound("bigSelect");
					}
					else
					{
						playSound("bigDeSelect");
					}
					break;
				case "fbp":
				case "fill":
				case "fillbp":
				case "fillbackpack":
				{
					for (int n = 0; n < player.items.Count; n++)
					{
						if (player.items[n] != null)
						{
							continue;
						}
						int num5 = -1;
						while (!objectInformation.ContainsKey(num5))
						{
							num5 = random.Next(1000);
							if (num5 != 390 && (!objectInformation.ContainsKey(num5) || objectInformation[num5].Split('/')[0] == "Stone"))
							{
								num5 = -1;
							}
							else if (!objectInformation.ContainsKey(num5) || objectInformation[num5].Split('/')[0].Contains("Weed"))
							{
								num5 = -1;
							}
							else if (!objectInformation.ContainsKey(num5) || objectInformation[num5].Split('/')[3].Contains("Crafting"))
							{
								num5 = -1;
							}
							else if (!objectInformation.ContainsKey(num5) || objectInformation[num5].Split('/')[3].Contains("Seed"))
							{
								num5 = -1;
							}
						}
						bool flag = false;
						if (num5 >= 516 && num5 <= 534)
						{
							flag = true;
						}
						if (flag)
						{
							player.items[n] = new Ring(num5);
						}
						else
						{
							player.items[n] = new Object(num5, 1);
						}
					}
					break;
				}
				case "sl":
					player.shiftToolbar(right: false);
					break;
				case "sr":
					player.shiftToolbar(right: true);
					break;
				case "characterinfo":
					showGlobalMessage(currentLocation.characters.Count + " characters on this map");
					break;
				case "doesitemexist":
					showGlobalMessage(Utility.doesItemWithThisIndexExistAnywhere(Convert.ToInt32(debugSplit[1]), debugSplit.Length > 2) ? "Yes" : "No");
					break;
				case "specialitem":
					player.specialItems.Add(Convert.ToInt32(debugSplit[1]));
					break;
				case "animalinfo":
					showGlobalMessage(getFarm().getAllFarmAnimals().Count.ToString() ?? "");
					break;
				case "clearchildren":
					player.getRidOfChildren();
					break;
				case "createsplash":
				{
					Point point = default(Point);
					if ((int)player.facingDirection == 3)
					{
						point.X = -4;
					}
					else if ((int)player.facingDirection == 1)
					{
						point.X = 4;
					}
					else if ((int)player.facingDirection == 0)
					{
						point.Y = 4;
					}
					else if ((int)player.facingDirection == 2)
					{
						point.Y = -4;
					}
					player.currentLocation.fishSplashPoint.Set(new Point(player.getTileX() + point.X, player.getTileX() + point.Y));
					break;
				}
				case "pregnant":
				{
					WorldDate date = Date;
					date.TotalDays++;
					player.GetSpouseFriendship().NextBirthingDate = date;
					break;
				}
				case "spreadseeds":
				{
					Farm farm2 = getFarm();
					foreach (KeyValuePair<Vector2, TerrainFeature> pair in farm2.terrainFeatures.Pairs)
					{
						if (pair.Value is HoeDirt)
						{
							(pair.Value as HoeDirt).crop = new Crop(Convert.ToInt32(debugSplit[1]), (int)pair.Key.X, (int)pair.Key.Y);
						}
					}
					break;
				}
				case "spreaddirt":
				{
					Farm farm2 = getFarm();
					for (int num70 = 0; num70 < farm2.map.Layers[0].LayerWidth; num70++)
					{
						for (int num71 = 0; num71 < farm2.map.Layers[0].LayerHeight; num71++)
						{
							if (!farm2.terrainFeatures.ContainsKey(new Vector2(num70, num71)) && farm2.doesTileHaveProperty(num70, num71, "Diggable", "Back") != null && farm2.isTileLocationTotallyClearAndPlaceable(new Vector2(num70, num71)))
							{
								farm2.terrainFeatures.Add(new Vector2(num70, num71), new HoeDirt());
							}
						}
					}
					break;
				}
				case "removefurniture":
					currentLocation.furniture.Clear();
					break;
				case "makeex":
					player.friendshipData[debugSplit[1]].RoommateMarriage = false;
					player.friendshipData[debugSplit[1]].Status = FriendshipStatus.Divorced;
					break;
				case "darktalisman":
					player.hasDarkTalisman = true;
					getLocationFromName("Railroad").setMapTile(54, 35, 287, "Buildings", "", 1);
					getLocationFromName("Railroad").setMapTile(54, 34, 262, "Front", "", 1);
					getLocationFromName("WitchHut").setMapTile(4, 11, 114, "Buildings", "", 1);
					getLocationFromName("WitchHut").setTileProperty(4, 11, "Buildings", "Action", "MagicInk");
					player.hasMagicInk = false;
					player.mailReceived.Clear();
					break;
				case "conventionmode":
					conventionMode = !conventionMode;
					break;
				case "farmmap":
				{
					for (int num65 = 0; num65 < locations.Count; num65++)
					{
						if (locations[num65] is Farm)
						{
							locations.RemoveAt(num65);
						}
						if (locations[num65] is FarmHouse)
						{
							locations.RemoveAt(num65);
						}
					}
					whichFarm = Convert.ToInt32(debugSplit[1]);
					locations.Add(new Farm("Maps\\" + Farm.getMapNameFromTypeInt(whichFarm), "Farm"));
					locations.Add(new FarmHouse("Maps\\FarmHouse", "FarmHouse"));
					break;
				}
				case "clearmuseum":
					(getLocationFromName("ArchaeologyHouse") as LibraryMuseum).museumPieces.Clear();
					break;
				case "clone":
					currentLocation.characters.Add(Utility.fuzzyCharacterSearch(debugSplit[1]));
					break;
				case "ee":
					pauseTime = 0f;
					nonWarpFade = true;
					eventFinished();
					fadeScreenToBlack();
					viewportFreeze = false;
					break;
				case "zl":
				case "zoomlevel":
					options.desiredBaseZoomLevel = (float)Convert.ToInt32(debugSplit[1]) / 100f;
					PinchZoom.Instance.SetZoomLevel((float)Convert.ToInt32(debugSplit[1]) / 100f);
					break;
				case "us":
				case "uiscale":
					options.desiredUIScale = (float)Convert.ToInt32(debugSplit[1]) / 100f;
					break;
				case "deletearch":
					player.archaeologyFound.Clear();
					player.fishCaught.Clear();
					player.mineralsFound.Clear();
					player.mailReceived.Clear();
					break;
				case "save":
					saveOnNewDay = !saveOnNewDay;
					if (saveOnNewDay)
					{
						playSound("bigSelect");
					}
					else
					{
						playSound("bigDeSelect");
					}
					break;
				case "removelargetf":
					currentLocation.largeTerrainFeatures.Clear();
					break;
				case "test":
					currentMinigame = new Test();
					break;
				case "fencedecay":
					foreach (Object value4 in currentLocation.objects.Values)
					{
						if (value4 is Fence)
						{
							(value4 as Fence).health.Value -= Convert.ToInt32(debugSplit[1]);
						}
					}
					break;
				case "sb":
					Utility.fuzzyCharacterSearch(debugSplit[1]).showTextAboveHead(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3206"));
					break;
				case "pan":
					player.addItemToInventoryBool(new Pan());
					break;
				case "gamepad":
					options.gamepadControls = !options.gamepadControls;
					options.mouseControls = !options.gamepadControls;
					showGlobalMessage(options.gamepadControls ? content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3209") : content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3210"));
					break;
				case "slimecraft":
					player.craftingRecipes.Add("Slime Incubator", 0);
					player.craftingRecipes.Add("Slime Egg-Press", 0);
					playSound("crystal");
					break;
				case "kms":
				case "killmonsterstat":
				{
					string text9 = debugSplit[1].Replace("0", " ");
					int num12 = Convert.ToInt32(debugSplit[2]);
					if (stats.specificMonstersKilled.ContainsKey(text9))
					{
						stats.specificMonstersKilled[text9] = num12;
					}
					else
					{
						stats.specificMonstersKilled.Add(text9, num12);
					}
					debugOutput = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3159", text9, num12);
					break;
				}
				case "fixanimals":
				{
					Farm farm = getFarm();
					foreach (Building building in farm.buildings)
					{
						if (building.indoors.Value == null || !(building.indoors.Value is AnimalHouse))
						{
							continue;
						}
						foreach (FarmAnimal value5 in (building.indoors.Value as AnimalHouse).animals.Values)
						{
							foreach (Building building2 in farm.buildings)
							{
								if (building2.indoors.Value == null || !(building2.indoors.Value is AnimalHouse) || !(building2.indoors.Value as AnimalHouse).animalsThatLiveHere.Contains(value5.myID) || building2.Equals(value5.home))
								{
									continue;
								}
								for (int num15 = (building2.indoors.Value as AnimalHouse).animalsThatLiveHere.Count - 1; num15 >= 0; num15--)
								{
									if ((building2.indoors.Value as AnimalHouse).animalsThatLiveHere[num15] == (long)value5.myID)
									{
										(building2.indoors.Value as AnimalHouse).animalsThatLiveHere.RemoveAt(num15);
										playSound("crystal");
									}
								}
							}
						}
						for (int num16 = (building.indoors.Value as AnimalHouse).animalsThatLiveHere.Count - 1; num16 >= 0; num16--)
						{
							if (Utility.getAnimal((building.indoors.Value as AnimalHouse).animalsThatLiveHere[num16]) == null)
							{
								(building.indoors.Value as AnimalHouse).animalsThatLiveHere.RemoveAt(num16);
								playSound("crystal");
							}
						}
					}
					break;
				}
				case "steaminfo":
				case "sdkinfo":
					Program.sdk.DebugInfo();
					break;
				case "achieve":
					Program.sdk.GetAchievement(debugSplit[1]);
					break;
				case "resetachievements":
					Program.sdk.ResetAchievements();
					break;
				case "divorce":
					player.divorceTonight.Value = true;
					break;
				case "befriendanimals":
					if (!(currentLocation is AnimalHouse))
					{
						break;
					}
					foreach (FarmAnimal value6 in (currentLocation as AnimalHouse).animals.Values)
					{
						value6.friendshipTowardFarmer.Value = ((debugSplit.Length > 1) ? Convert.ToInt32(debugSplit[1]) : 1000);
					}
					break;
				case "pettofarm":
					getCharacterFromName<Pet>(player.getPetName(), mustBeVillager: false).setAtFarmPosition();
					break;
				case "version":
					debugOutput = typeof(Game1).Assembly.GetName().Version?.ToString() ?? "";
					break;
				case "nosave":
				case "ns":
					saveOnNewDay = !saveOnNewDay;
					if (!saveOnNewDay)
					{
						playSound("bigDeSelect");
					}
					else
					{
						playSound("bigSelect");
					}
					debugOutput = "Saving is now " + (saveOnNewDay ? "enabled" : "disabled");
					break;
				case "rfh":
				case "readyforharvest":
					currentLocation.objects[new Vector2(Convert.ToInt32(debugSplit[1]), Convert.ToInt32(debugSplit[2]))].minutesUntilReady.Value = 1;
					break;
				case "beachbridge":
					(getLocationFromName("Beach") as Beach).bridgeFixed.Value = !(getLocationFromName("Beach") as Beach).bridgeFixed;
					if (!(getLocationFromName("Beach") as Beach).bridgeFixed)
					{
						(getLocationFromName("Beach") as Beach).setMapTile(58, 13, 284, "Buildings", null, 1);
					}
					break;
				case "dp":
					stats.daysPlayed = (uint)Convert.ToInt32(debugSplit[1]);
					break;
				case "fo":
				case "frameoffset":
				{
					int num59 = ((!debugSplit[2].Contains('s')) ? 1 : (-1));
					FarmerRenderer.featureXOffsetPerFrame[Convert.ToInt32(debugSplit[1])] = (short)(num59 * Convert.ToInt32(debugSplit[2].Last().ToString() ?? ""));
					num59 = ((!debugSplit[3].Contains('s')) ? 1 : (-1));
					FarmerRenderer.featureYOffsetPerFrame[Convert.ToInt32(debugSplit[1])] = (short)(num59 * Convert.ToInt32(debugSplit[3].Last().ToString() ?? ""));
					if (debugSplit.Length > 4)
					{
						num59 = ((!debugSplit[4].Contains('s')) ? 1 : (-1));
					}
					break;
				}
				case "horse":
					currentLocation.characters.Add(new Horse(GuidHelper.NewGuid(), Convert.ToInt32(debugSplit[1]), Convert.ToInt32(debugSplit[2])));
					break;
				case "owl":
					currentLocation.addOwl();
					break;
				case "pole":
					player.addItemToInventoryBool(new FishingRod((debugSplit.Length > 1) ? Convert.ToInt32(debugSplit[1]) : 0));
					break;
				case "removequest":
					player.removeQuest(Convert.ToInt32(debugSplit[1]));
					break;
				case "completequest":
					player.completeQuest(Convert.ToInt32(debugSplit[1]));
					break;
				case "togglecatperson":
					player.catPerson = !player.catPerson;
					break;
				case "clearcharacters":
					currentLocation.characters.Clear();
					break;
				case "cat":
					currentLocation.characters.Add(new Cat(Convert.ToInt32(debugSplit[1]), Convert.ToInt32(debugSplit[2]), (debugSplit.Count() > 3) ? Convert.ToInt32(debugSplit[3]) : 0));
					break;
				case "dog":
					currentLocation.characters.Add(new Dog(Convert.ToInt32(debugSplit[1]), Convert.ToInt32(debugSplit[2]), (debugSplit.Count() > 3) ? Convert.ToInt32(debugSplit[3]) : 0));
					break;
				case "quest":
					player.questLog.Add(Quest.getQuestFromId(Convert.ToInt32(debugSplit[1])));
					break;
				case "deliveryquest":
					player.questLog.Add(new ItemDeliveryQuest());
					break;
				case "collectquest":
					player.questLog.Add(new ResourceCollectionQuest());
					break;
				case "slayquest":
					player.questLog.Add(new SlayMonsterQuest());
					break;
				case "quests":
				{
					Dictionary<int, string> dictionary3 = content.Load<Dictionary<int, string>>("Data\\Quests");
					foreach (int key2 in dictionary3.Keys)
					{
						if (!player.hasQuest(key2))
						{
							player.addQuest(key2);
						}
					}
					player.questLog.Add(new ItemDeliveryQuest());
					player.questLog.Add(new SlayMonsterQuest());
					break;
				}
				case "clearquests":
					player.questLog.Clear();
					break;
				case "fb":
				case "fillbin":
					getFarm().getShippingBin(player).Add(new Object(24, 1));
					getFarm().getShippingBin(player).Add(new Object(82, 1));
					getFarm().getShippingBin(player).Add(new Object(136, 1));
					getFarm().getShippingBin(player).Add(new Object(16, 1));
					getFarm().getShippingBin(player).Add(new Object(388, 1));
					break;
				case "gold":
					player.Money += 1000000;
					break;
				case "clearfarm":
				{
					int num26 = 0;
					while (num26 < getFarm().map.Layers[0].LayerWidth)
					{
						int num28;
						for (int num27 = 0; num27 < getFarm().map.Layers[0].LayerHeight; num27 = num28)
						{
							getFarm().removeEverythingExceptCharactersFromThisTile(num26, num27);
							num28 = num27 + 1;
						}
						num28 = num26 + 1;
						num26 = num28;
					}
					break;
				}
				case "setupfarm":
				{
					getFarm().buildings.Clear();
					for (int num22 = 0; num22 < getFarm().map.Layers[0].LayerWidth; num22++)
					{
						for (int num23 = 0; num23 < 16 + ((debugSplit.Length > 1) ? 32 : 0); num23++)
						{
							getFarm().removeEverythingExceptCharactersFromThisTile(num22, num23);
						}
					}
					for (int num24 = 56; num24 < 71; num24++)
					{
						for (int num25 = 17; num25 < 34; num25++)
						{
							getFarm().removeEverythingExceptCharactersFromThisTile(num24, num25);
							if (num24 > 57 && num25 > 18 && num24 < 70 && num25 < 29)
							{
								getFarm().terrainFeatures.Add(new Vector2(num24, num25), new HoeDirt());
							}
						}
					}
					getFarm().buildStructure(new BluePrint("Coop"), new Vector2(52f, 11f), player);
					getFarm().buildings.Last().daysOfConstructionLeft.Value = 0;
					getFarm().buildStructure(new BluePrint("Silo"), new Vector2(36f, 9f), player);
					getFarm().buildings.Last().daysOfConstructionLeft.Value = 0;
					getFarm().buildStructure(new BluePrint("Barn"), new Vector2(42f, 10f), player);
					getFarm().buildings.Last().daysOfConstructionLeft.Value = 0;
					player.getToolFromName("Ax").UpgradeLevel = 4;
					player.getToolFromName("Watering Can").UpgradeLevel = 4;
					player.getToolFromName("Hoe").UpgradeLevel = 4;
					player.getToolFromName("Pickaxe").UpgradeLevel = 4;
					player.Money += 20000;
					player.addItemToInventoryBool(new Shears());
					player.addItemToInventoryBool(new MilkPail());
					player.addItemToInventoryBool(new Object(472, 999));
					player.addItemToInventoryBool(new Object(473, 999));
					player.addItemToInventoryBool(new Object(322, 999));
					player.addItemToInventoryBool(new Object(388, 999));
					player.addItemToInventoryBool(new Object(390, 999));
					break;
				}
				case "shears":
				case "scissors":
					player.addItemToInventoryBool(new Shears());
					break;
				case "mp":
					player.addItemToInventoryBool(new MilkPail());
					break;
				case "removebuildings":
					getFarm().buildings.Clear();
					break;
				case "build":
					getFarm().buildStructure(new BluePrint(debugSplit[1].Replace('9', ' ')), (debugSplit.Length > 3) ? new Vector2(Convert.ToInt32(debugSplit[2]), Convert.ToInt32(debugSplit[3])) : new Vector2(player.getTileX() + 1, player.getTileY()), player);
					getFarm().buildings.Last().daysOfConstructionLeft.Value = 0;
					break;
				case "bc":
				case "buildcoop":
					getFarm().buildStructure(new BluePrint("Coop"), new Vector2(Convert.ToInt32(debugSplit[1]), Convert.ToInt32(debugSplit[2])), player);
					getFarm().buildings.Last().daysOfConstructionLeft.Value = 0;
					break;
				case "localInfo":
				{
					debugOutput = "";
					int num17 = 0;
					int num18 = 0;
					int num19 = 0;
					foreach (TerrainFeature value7 in currentLocation.terrainFeatures.Values)
					{
						if (value7 is Grass)
						{
							num17++;
						}
						else if (value7 is Tree)
						{
							num18++;
						}
						else
						{
							num19++;
						}
					}
					debugOutput = debugOutput + "Grass:" + num17 + ",  ";
					debugOutput = debugOutput + "Trees:" + num18 + ",  ";
					debugOutput = debugOutput + "Other Terrain Features:" + num19 + ",  ";
					debugOutput = debugOutput + "Objects: " + currentLocation.objects.Count() + ",  ";
					debugOutput = debugOutput + "temporarySprites: " + currentLocation.temporarySprites.Count + ",  ";
					drawObjectDialogue(debugOutput);
					break;
				}
				case "al":
				case "ambientlight":
					ambientLight = new Color(Convert.ToInt32(debugSplit[1]), Convert.ToInt32(debugSplit[2]), Convert.ToInt32(debugSplit[3]));
					break;
				case "resetmines":
					MineShaft.permanentMineChanges.Clear();
					playSound("jingle1");
					break;
				case "db":
					activeClickableMenu = new DialogueBox(Utility.fuzzyCharacterSearch((debugSplit.Length > 1) ? debugSplit[1] : "Pierre").CurrentDialogue.Peek());
					break;
				case "skullkey":
					player.hasSkullKey = true;
					break;
				case "townkey":
					player.HasTownKey = true;
					break;
				case "specials":
					player.hasRustyKey = true;
					player.hasSkullKey = true;
					player.hasSpecialCharm = true;
					player.hasDarkTalisman = true;
					player.hasMagicInk = true;
					player.hasClubCard = true;
					player.canUnderstandDwarves = true;
					player.hasMagnifyingGlass = true;
					player.eventsSeen.Add(2120303);
					player.eventsSeen.Add(3910979);
					player.HasTownKey = true;
					break;
				case "skullgear":
				{
					player.hasSkullKey = true;
					player.MaxItems = 32;
					player.leftRing.Value = new Ring(527);
					player.rightRing.Value = new Ring(523);
					player.boots.Value = new Boots(514);
					player.clearBackpack();
					Pickaxe pickaxe = new Pickaxe();
					pickaxe.UpgradeLevel = 4;
					player.addItemToInventory(pickaxe);
					player.addItemToInventory(new MeleeWeapon(4));
					player.addItemToInventory(new Object(226, 20));
					player.addItemToInventory(new Object(288, 20));
					player.professions.Add(24);
					player.maxHealth = 75;
					break;
				}
				case "clearspecials":
					player.hasRustyKey = false;
					player.hasSkullKey = false;
					player.hasSpecialCharm = false;
					player.hasDarkTalisman = false;
					player.hasMagicInk = false;
					player.hasClubCard = false;
					player.canUnderstandDwarves = false;
					player.hasMagnifyingGlass = false;
					break;
				case "tv":
					player.addItemToInventoryBool(new TV((random.NextDouble() < 0.5) ? 1466 : 1468, Vector2.Zero));
					break;
				case "sn":
					player.hasMagnifyingGlass = true;
					if (debugSplit.Length > 1)
					{
						int num4 = Convert.ToInt32(debugSplit[1]);
						Object @object = new Object(79, 1);
						Object object2 = @object;
						object2.name = object2.name + " #" + num4;
						player.addItemToInventory(@object);
					}
					else
					{
						player.addItemToInventory(currentLocation.tryToCreateUnseenSecretNote(player));
					}
					break;
				case "child2":
					if (player.getChildrenCount() > 1)
					{
						player.getChildren()[1].Age++;
						player.getChildren()[1].reloadSprite();
					}
					else
					{
						(getLocationFromName("FarmHouse") as FarmHouse).characters.Add(new Child("Baby2", random.NextDouble() < 0.5, random.NextDouble() < 0.5, player));
					}
					break;
				case "child":
				case "kid":
					if (player.getChildren().Count > 0)
					{
						player.getChildren()[0].Age++;
						player.getChildren()[0].reloadSprite();
					}
					else
					{
						(getLocationFromName("FarmHouse") as FarmHouse).characters.Add(new Child("Baby", random.NextDouble() < 0.5, random.NextDouble() < 0.5, player));
					}
					break;
				case "killall":
				{
					string value = debugSplit[1];
					foreach (GameLocation location in locations)
					{
						if (!location.Equals(currentLocation))
						{
							location.characters.Clear();
							continue;
						}
						for (int k = location.characters.Count - 1; k >= 0; k--)
						{
							if (!location.characters[k].Name.Equals(value))
							{
								location.characters.RemoveAt(k);
							}
						}
					}
					break;
				}
				case "resetworldstate":
					worldStateIDs.Clear();
					netWorldState.Value = new NetWorldState();
					parseDebugInput("deleteArch");
					player.mailReceived.Clear();
					player.eventsSeen.Clear();
					break;
				case "killallhorses":
					foreach (GameLocation location2 in locations)
					{
						for (int j = location2.characters.Count - 1; j >= 0; j--)
						{
							if (location2.characters[j] is Horse)
							{
								location2.characters.RemoveAt(j);
								playSound("drumkit0");
							}
						}
					}
					break;
				case "dateplayer":
					foreach (Farmer allFarmer in getAllFarmers())
					{
						if (allFarmer != player && (bool)allFarmer.isCustomized)
						{
							player.team.GetFriendship(player.UniqueMultiplayerID, allFarmer.UniqueMultiplayerID).Status = FriendshipStatus.Dating;
							break;
						}
					}
					break;
				case "engageplayer":
					foreach (Farmer allFarmer2 in getAllFarmers())
					{
						if (allFarmer2 != player && (bool)allFarmer2.isCustomized)
						{
							Friendship friendship2 = player.team.GetFriendship(player.UniqueMultiplayerID, allFarmer2.UniqueMultiplayerID);
							friendship2.Status = FriendshipStatus.Engaged;
							friendship2.WeddingDate = Date;
							friendship2.WeddingDate.TotalDays++;
							break;
						}
					}
					break;
				case "marryplayer":
					foreach (Farmer onlineFarmer in getOnlineFarmers())
					{
						if (onlineFarmer != player && (bool)onlineFarmer.isCustomized)
						{
							Friendship friendship = player.team.GetFriendship(player.UniqueMultiplayerID, onlineFarmer.UniqueMultiplayerID);
							friendship.Status = FriendshipStatus.Married;
							friendship.WeddingDate = Date;
							break;
						}
					}
					break;
				case "marry":
				{
					NPC nPC9 = Utility.fuzzyCharacterSearch(debugSplit[1]);
					if (nPC9 != null && !player.friendshipData.ContainsKey(nPC9.Name))
					{
						player.friendshipData.Add(nPC9.Name, new Friendship());
					}
					player.changeFriendship(2500, nPC9);
					player.spouse = nPC9.Name;
					player.friendshipData[nPC9.Name].WeddingDate = new WorldDate(Date);
					player.friendshipData[nPC9.Name].Status = FriendshipStatus.Married;
					prepareSpouseForWedding(player);
					break;
				}
				case "engaged":
				{
					player.changeFriendship(2500, Utility.fuzzyCharacterSearch(debugSplit[1]));
					player.spouse = debugSplit[1];
					player.friendshipData[debugSplit[1]].Status = FriendshipStatus.Engaged;
					WorldDate date2 = Date;
					date2.TotalDays++;
					player.friendshipData[debugSplit[1]].WeddingDate = date2;
					break;
				}
				case "clearlightglows":
					currentLocation.lightGlows.Clear();
					break;
				case "wp":
				case "wallpaper":
				{
					if (debugSplit.Count() > 1)
					{
						player.addItemToInventoryBool(new Wallpaper(Convert.ToInt32(debugSplit[1])));
						break;
					}
					bool flag4 = random.NextDouble() < 0.5;
					player.addItemToInventoryBool(new Wallpaper(flag4 ? random.Next(40) : random.Next(112), flag4));
					break;
				}
				case "clearfurniture":
					(currentLocation as FarmHouse).furniture.Clear();
					break;
				case "ff":
				case "furniture":
					if (debugSplit.Length < 2)
					{
						Furniture furniture = null;
						while (furniture == null)
						{
							try
							{
								furniture = new Furniture(random.Next(1613), Vector2.Zero);
							}
							catch (Exception)
							{
							}
						}
						player.addItemToInventoryBool(furniture);
					}
					else
					{
						player.addItemToInventoryBool(new Furniture(Convert.ToInt32(debugSplit[1]), Vector2.Zero));
					}
					break;
				case "spawncoopsandbarns":
				{
					if (!(currentLocation is Farm))
					{
						break;
					}
					int num66 = Convert.ToInt32(debugSplit[1]);
					for (int num67 = 0; num67 < num66; num67++)
					{
						for (int num68 = 0; num68 < 20; num68++)
						{
							bool flag3 = random.NextDouble() < 0.5;
							if (getFarm().buildStructure(new BluePrint(flag3 ? "Deluxe Coop" : "Deluxe Barn"), getFarm().getRandomTile(), player))
							{
								getFarm().buildings.Last().daysOfConstructionLeft.Value = 0;
								getFarm().buildings.Last().doAction(Utility.PointToVector2(getFarm().buildings.Last().animalDoor) + new Vector2((int)getFarm().buildings.Last().tileX, (int)getFarm().buildings.Last().tileY), player);
								for (int num69 = 0; num69 < 16; num69++)
								{
									Utility.addAnimalToFarm(new FarmAnimal(flag3 ? "White Chicken" : "Cow", random.Next(2147483647), player.uniqueMultiplayerID));
								}
								break;
							}
						}
					}
					break;
				}
				case "setupfishpondfarm":
				{
					int value3 = ((debugSplit.Count() > 1) ? Convert.ToInt32(debugSplit[1]) : 10);
					parseDebugInput("clearFarm");
					for (int num62 = 4; num62 < 77; num62 += 6)
					{
						for (int num63 = 9; num63 < 60; num63 += 6)
						{
							parseDebugInput("build Fish9Pond " + num62 + " " + num63);
						}
					}
					foreach (Building building3 in getFarm().buildings)
					{
						int num64 = random.Next(128, 159);
						if (random.NextDouble() < 0.15)
						{
							num64 = random.Next(698, 724);
						}
						if (random.NextDouble() < 0.05)
						{
							num64 = random.Next(796, 801);
						}
						if (objectInformation.ContainsKey(num64) && objectInformation[num64].Split('/')[3].Contains("-4"))
						{
							(building3 as FishPond).fishType.Value = num64;
						}
						else
						{
							(building3 as FishPond).fishType.Value = ((random.NextDouble() < 0.5) ? 393 : 397);
						}
						(building3 as FishPond).maxOccupants.Value = 10;
						(building3 as FishPond).currentOccupants.Value = value3;
						(building3 as FishPond).GetFishObject();
					}
					parseDebugInput("dayUpdate 1");
					break;
				}
				case "grass":
				{
					for (int num60 = 0; num60 < getFarm().Map.Layers[0].LayerWidth; num60++)
					{
						for (int num61 = 0; num61 < getFarm().Map.Layers[0].LayerHeight; num61++)
						{
							if (getFarm().isTileLocationTotallyClearAndPlaceable(new Vector2(num60, num61)))
							{
								getFarm().terrainFeatures.Add(new Vector2(num60, num61), new Grass(1, 4));
							}
						}
					}
					break;
				}
				case "setupbigfarm":
				{
					parseDebugInput("clearFarm");
					parseDebugInput("build Deluxe9Coop 4 9");
					parseDebugInput("build Deluxe9Coop 10 9");
					parseDebugInput("build Deluxe9Coop 36 11");
					parseDebugInput("build Deluxe9Barn 16 9");
					parseDebugInput("build Deluxe9Barn 3 16");
					for (int num34 = 0; num34 < 48; num34++)
					{
						parseDebugInput("animal White Chicken");
					}
					for (int num35 = 0; num35 < 32; num35++)
					{
						parseDebugInput("animal Cow");
					}
					for (int num36 = 0; num36 < getFarm().buildings.Count(); num36++)
					{
						getFarm().buildings[num36].doAction(Utility.PointToVector2(getFarm().buildings[num36].animalDoor) + new Vector2((int)getFarm().buildings[num36].tileX, (int)getFarm().buildings[num36].tileY), player);
					}
					parseDebugInput("build Mill 30 20");
					parseDebugInput("build Stable 46 10");
					parseDebugInput("build Silo 54 14");
					parseDebugInput("build Junimo9Hut 48 52");
					parseDebugInput("build Junimo9Hut 55 52");
					parseDebugInput("build Junimo9Hut 59 52");
					parseDebugInput("build Junimo9Hut 65 52");
					for (int num37 = 11; num37 < 23; num37++)
					{
						for (int num38 = 14; num38 < 25; num38++)
						{
							getFarm().terrainFeatures.Add(new Vector2(num37, num38), new Grass(1, 4));
						}
					}
					for (int num39 = 3; num39 < 23; num39++)
					{
						for (int num40 = 57; num40 < 61; num40++)
						{
							getFarm().terrainFeatures.Add(new Vector2(num39, num40), new Grass(1, 4));
						}
					}
					for (int num41 = 17; num41 < 25; num41++)
					{
						getFarm().terrainFeatures.Add(new Vector2(64f, num41), new Flooring(6));
					}
					for (int num42 = 35; num42 < 64; num42++)
					{
						getFarm().terrainFeatures.Add(new Vector2(num42, 24f), new Flooring(6));
					}
					for (int num43 = 38; num43 < 76; num43++)
					{
						for (int num44 = 18; num44 < 52; num44++)
						{
							if (getFarm().isTileLocationTotallyClearAndPlaceable(new Vector2(num43, num44)))
							{
								getFarm().terrainFeatures.Add(new Vector2(num43, num44), new HoeDirt());
								(getFarm().terrainFeatures[new Vector2(num43, num44)] as HoeDirt).plant(472 + random.Next(5), num43, num44, player, isFertilizer: false, getFarm());
							}
						}
					}
					parseDebugInput("growCrops 8");
					getFarm().terrainFeatures.Add(new Vector2(8f, 25f), new FruitTree(628 + random.Next(2), 4));
					getFarm().terrainFeatures.Add(new Vector2(11f, 25f), new FruitTree(628 + random.Next(2), 4));
					getFarm().terrainFeatures.Add(new Vector2(14f, 25f), new FruitTree(628 + random.Next(2), 4));
					getFarm().terrainFeatures.Add(new Vector2(17f, 25f), new FruitTree(628 + random.Next(2), 4));
					getFarm().terrainFeatures.Add(new Vector2(20f, 25f), new FruitTree(628 + random.Next(2), 4));
					getFarm().terrainFeatures.Add(new Vector2(23f, 25f), new FruitTree(628 + random.Next(2), 4));
					getFarm().terrainFeatures.Add(new Vector2(8f, 28f), new FruitTree(628 + random.Next(2), 4));
					getFarm().terrainFeatures.Add(new Vector2(11f, 28f), new FruitTree(628 + random.Next(2), 4));
					getFarm().terrainFeatures.Add(new Vector2(14f, 28f), new FruitTree(628 + random.Next(2), 4));
					getFarm().terrainFeatures.Add(new Vector2(17f, 28f), new FruitTree(628 + random.Next(2), 4));
					getFarm().terrainFeatures.Add(new Vector2(20f, 28f), new FruitTree(628 + random.Next(2), 4));
					getFarm().terrainFeatures.Add(new Vector2(23f, 28f), new FruitTree(628 + random.Next(2), 4));
					getFarm().terrainFeatures.Add(new Vector2(8f, 31f), new FruitTree(628 + random.Next(2), 4));
					getFarm().terrainFeatures.Add(new Vector2(11f, 31f), new FruitTree(628 + random.Next(2), 4));
					getFarm().terrainFeatures.Add(new Vector2(14f, 31f), new FruitTree(628 + random.Next(2), 4));
					getFarm().terrainFeatures.Add(new Vector2(17f, 31f), new FruitTree(628 + random.Next(2), 4));
					getFarm().terrainFeatures.Add(new Vector2(20f, 31f), new FruitTree(628 + random.Next(2), 4));
					getFarm().terrainFeatures.Add(new Vector2(23f, 31f), new FruitTree(628 + random.Next(2), 4));
					for (int num45 = 3; num45 < 15; num45++)
					{
						for (int num46 = 36; num46 < 45; num46++)
						{
							if (getFarm().isTileLocationTotallyClearAndPlaceable(new Vector2(num45, num46)))
							{
								getFarm().objects.Add(new Vector2(num45, num46), new Object(new Vector2(num45, num46), 12));
								getFarm().objects[new Vector2(num45, num46)].performObjectDropInAction(new Object(454, 1), probe: false, player);
							}
						}
					}
					for (int num47 = 16; num47 < 26; num47++)
					{
						for (int num48 = 36; num48 < 45; num48++)
						{
							if (getFarm().isTileLocationTotallyClearAndPlaceable(new Vector2(num47, num48)))
							{
								getFarm().objects.Add(new Vector2(num47, num48), new Object(new Vector2(num47, num48), 13));
							}
						}
					}
					for (int num49 = 3; num49 < 15; num49++)
					{
						for (int num50 = 47; num50 < 57; num50++)
						{
							if (getFarm().isTileLocationTotallyClearAndPlaceable(new Vector2(num49, num50)))
							{
								getFarm().objects.Add(new Vector2(num49, num50), new Object(new Vector2(num49, num50), 16));
							}
						}
					}
					for (int num51 = 16; num51 < 26; num51++)
					{
						for (int num52 = 47; num52 < 57; num52++)
						{
							if (getFarm().isTileLocationTotallyClearAndPlaceable(new Vector2(num51, num52)))
							{
								getFarm().objects.Add(new Vector2(num51, num52), new Object(new Vector2(num51, num52), 15));
							}
						}
					}
					for (int num53 = 28; num53 < 38; num53++)
					{
						for (int num54 = 26; num54 < 46; num54++)
						{
							if (getFarm().isTileLocationTotallyClearAndPlaceable(new Vector2(num53, num54)))
							{
								new Torch(new Vector2(num53, num54), 1, 93).placementAction(getFarm(), num53 * 64, num54 * 64, null);
							}
						}
					}
					break;
				}
				case "houseupgrade":
				case "hu":
				case "house":
					Utility.getHomeOfFarmer(player).moveObjectsForHouseUpgrade(Convert.ToInt32(debugSplit[1]));
					Utility.getHomeOfFarmer(player).setMapForUpgradeLevel(Convert.ToInt32(debugSplit[1]));
					player.HouseUpgradeLevel = Convert.ToInt32(debugSplit[1]);
					removeFrontLayerForFarmBuildings();
					addNewFarmBuildingMaps();
					Utility.getHomeOfFarmer(player).ReadWallpaperAndFloorTileData();
					Utility.getHomeOfFarmer(player).RefreshFloorObjectNeighbors();
					break;
				case "ci":
				case "clear":
					player.clearBackpack();
					break;
				case "w":
				case "wall":
					(getLocationFromName("FarmHouse") as FarmHouse).SetWallpaper(debugSplit[1], null);
					break;
				case "floor":
					(getLocationFromName("FarmHouse") as FarmHouse).SetFloor(debugSplit[1], null);
					break;
				case "sprinkle":
					Utility.addSprinklesToLocation(currentLocation, player.getTileX(), player.getTileY(), 7, 7, 2000, 100, Color.White);
					break;
				case "clearmail":
					player.mailReceived.Clear();
					break;
				case "broadcastmailbox":
					addMail(debugSplit[1], noLetter: false, sendToEveryone: true);
					break;
				case "mft":
				case "mailfortomorrow":
					addMailForTomorrow(debugSplit[1].Replace('0', '_'), debugSplit.Length > 2);
					break;
				case "allmail":
					foreach (string key3 in content.Load<Dictionary<string, string>>("Data\\mail").Keys)
					{
						addMailForTomorrow(key3);
					}
					break;
				case "allmailread":
					foreach (string key4 in content.Load<Dictionary<string, string>>("Data\\mail").Keys)
					{
						player.mailReceived.Add(key4);
					}
					break;
				case "showmail":
				case "showMail":
				{
					if (debugSplit.Length < 2)
					{
						debugOutput = "Not enough parameters, expecting: showMail <mailTitle>";
						break;
					}
					string text12 = debugSplit[1];
					Dictionary<string, string> dictionary2 = content.Load<Dictionary<string, string>>("Data\\mail");
					string mail = (dictionary2.ContainsKey(text12) ? dictionary2[text12] : "");
					activeClickableMenu = new LetterViewerMenu(mail, text12);
					break;
				}
				case "whereis":
				case "where":
				{
					NPC nPC5 = Utility.fuzzyCharacterSearch(debugSplit[1], must_be_villager: false);
					debugOutput = nPC5.Name + " is at " + Utility.getGameLocationOfCharacter(nPC5).NameOrUniqueName + ", " + nPC5.getTileX() + "," + nPC5.getTileY();
					break;
				}
				case "removenpc":
					foreach (GameLocation location3 in locations)
					{
						foreach (NPC character2 in location3.characters)
						{
							if (character2.Name == debugSplit[1])
							{
								location3.characters.Remove(character2);
								debugOutput = "Removed " + debugSplit[1] + " from " + location3.Name;
								return true;
							}
						}
						if (!(location3 is BuildableGameLocation))
						{
							continue;
						}
						foreach (Building building4 in (location3 as BuildableGameLocation).buildings)
						{
							if (building4.indoors.Value == null)
							{
								continue;
							}
							foreach (NPC character3 in building4.indoors.Value.characters)
							{
								if (character3.Name == debugSplit[1])
								{
									building4.indoors.Value.characters.Remove(character3);
									debugOutput = "Removed " + debugSplit[1] + " from " + building4.indoors.Value.uniqueName;
									return true;
								}
							}
						}
					}
					debugOutput = "Couldn't find " + debugSplit[1];
					break;
				case "panmode":
				case "pm":
					panMode = true;
					viewportFreeze = true;
					debugMode = true;
					panFacingDirectionWait = false;
					panModeString = "";
					break;
				case "inputsim":
				case "is":
				{
					if (inputSimulator != null)
					{
						inputSimulator = null;
					}
					if (debugSplit.Length < 2)
					{
						debugOutput = "Invalid arguments, call as: inputSim <simType>";
						break;
					}
					string text10 = debugSplit[1].ToLower();
					if (!(text10 == "spamtool"))
					{
						if (text10 == "spamlr")
						{
							inputSimulator = new LeftRightClickSpamInputSimulator();
						}
						else
						{
							debugOutput = "No input simulator found for " + debugSplit[1];
						}
					}
					else
					{
						inputSimulator = new ToolSpamInputSimulator();
					}
					break;
				}
				case "hurry":
					Utility.fuzzyCharacterSearch(debugSplit[1]).warpToPathControllerDestination();
					break;
				case "morepollen":
				{
					for (int num21 = 0; num21 < Convert.ToInt32(debugSplit[1]); num21++)
					{
						debrisWeather.Add(GetWeatherDebris(new Vector2(random.Next(0, graphics.GraphicsDevice.Viewport.Width), random.Next(0, graphics.GraphicsDevice.Viewport.Height)), 0, (float)random.Next(15) / 500f, (float)random.Next(-10, 0) / 50f, (float)random.Next(10) / 50f));
					}
					break;
				}
				case "fillwithobject":
				{
					int parentSheetIndex = Convert.ToInt32(debugSplit[1]);
					bool flag2 = debugSplit.Count() > 2 && Convert.ToBoolean(debugSplit[2]);
					for (int num13 = 0; num13 < currentLocation.map.Layers[0].LayerHeight; num13++)
					{
						for (int num14 = 0; num14 < currentLocation.map.Layers[0].LayerWidth; num14++)
						{
							Vector2 vector3 = new Vector2(num14, num13);
							if (currentLocation.isTileLocationTotallyClearAndPlaceable(vector3))
							{
								currentLocation.setObject(vector3, flag2 ? new Object(vector3, parentSheetIndex) : new Object(parentSheetIndex, 1));
							}
						}
					}
					break;
				}
				case "spawnweeds":
				{
					for (int num11 = 0; num11 < Convert.ToInt32(debugSplit[1]); num11++)
					{
						currentLocation.spawnWeedsAndStones(1);
					}
					break;
				}
				case "busdriveback":
					(getLocationFromName("BusStop") as BusStop).busDriveBack();
					break;
				case "busdriveoff":
					(getLocationFromName("BusStop") as BusStop).busDriveOff();
					break;
				case "completejoja":
					player.mailReceived.Add("ccCraftsRoom");
					player.mailReceived.Add("ccVault");
					player.mailReceived.Add("ccFishTank");
					player.mailReceived.Add("ccBoilerRoom");
					player.mailReceived.Add("ccPantry");
					player.mailReceived.Add("jojaCraftsRoom");
					player.mailReceived.Add("jojaVault");
					player.mailReceived.Add("jojaFishTank");
					player.mailReceived.Add("jojaBoilerRoom");
					player.mailReceived.Add("jojaPantry");
					player.mailReceived.Add("JojaMember");
					break;
				case "completecc":
				{
					player.mailReceived.Add("ccCraftsRoom");
					player.mailReceived.Add("ccVault");
					player.mailReceived.Add("ccFishTank");
					player.mailReceived.Add("ccBoilerRoom");
					player.mailReceived.Add("ccPantry");
					player.mailReceived.Add("ccBulletin");
					player.mailReceived.Add("ccBoilerRoom");
					player.mailReceived.Add("ccPantry");
					player.mailReceived.Add("ccBulletin");
					CommunityCenter communityCenter3 = getLocationFromName("CommunityCenter") as CommunityCenter;
					for (int num10 = 0; num10 < communityCenter3.areasComplete.Count; num10++)
					{
						communityCenter3.markAreaAsComplete(num10);
					}
					break;
				}
				case "whereore":
					debugOutput = Convert.ToString(currentLocation.orePanPoint.Value);
					break;
				case "allbundles":
				{
					CommunityCenter communityCenter2 = getLocationFromName("CommunityCenter") as CommunityCenter;
					foreach (KeyValuePair<int, NetArray<bool, NetBool>> item9 in communityCenter2.bundles.FieldDict)
					{
						for (int num9 = 0; num9 < item9.Value.Count; num9++)
						{
							item9.Value[num9] = true;
						}
					}
					playSound("crystal");
					break;
				}
				case "junimogoodbye":
					(currentLocation as CommunityCenter).junimoGoodbyeDance();
					break;
				case "bundle":
				{
					CommunityCenter communityCenter = getLocationFromName("CommunityCenter") as CommunityCenter;
					int num7 = Convert.ToInt32(debugSplit[1]);
					foreach (KeyValuePair<int, NetArray<bool, NetBool>> item10 in communityCenter.bundles.FieldDict)
					{
						if (item10.Key == num7)
						{
							for (int num8 = 0; num8 < item10.Value.Count; num8++)
							{
								item10.Value[num8] = true;
							}
						}
					}
					playSound("crystal");
					break;
				}
				case "lookup":
				case "lu":
					foreach (int key5 in objectInformation.Keys)
					{
						if (objectInformation[key5].Substring(0, objectInformation[key5].IndexOf('/')).ToLower().Equals(debugInput.Substring(debugInput.IndexOf(' ') + 1)))
						{
							debugOutput = debugSplit[1] + " " + key5;
						}
					}
					break;
				case "ccloadcutscene":
					(getLocationFromName("CommunityCenter") as CommunityCenter).restoreAreaCutscene(Convert.ToInt32(debugSplit[1]));
					break;
				case "ccload":
					(getLocationFromName("CommunityCenter") as CommunityCenter).loadArea(Convert.ToInt32(debugSplit[1]));
					(getLocationFromName("CommunityCenter") as CommunityCenter).markAreaAsComplete(Convert.ToInt32(debugSplit[1]));
					break;
				case "plaque":
					(getLocationFromName("CommunityCenter") as CommunityCenter).addStarToPlaque();
					break;
				case "junimostar":
					((getLocationFromName("CommunityCenter") as CommunityCenter).characters[0] as Junimo).returnToJunimoHutToFetchStar(getLocationFromName("CommunityCenter") as CommunityCenter);
					break;
				case "j":
				case "aj":
				case "addjunimo":
					(getLocationFromName("CommunityCenter") as CommunityCenter).addCharacter(new Junimo(new Vector2(Convert.ToInt32(debugSplit[1]), Convert.ToInt32(debugSplit[2])) * 64f, Convert.ToInt32(debugSplit[3])));
					break;
				case "resetjunimonotes":
					foreach (NetArray<bool, NetBool> value8 in (getLocationFromName("CommunityCenter") as CommunityCenter).bundles.FieldDict.Values)
					{
						for (int num20 = 0; num20 < value8.Count; num20++)
						{
							value8[num20] = false;
						}
					}
					break;
				case "jn":
				case "junimonote":
					(getLocationFromName("CommunityCenter") as CommunityCenter).addJunimoNote(Convert.ToInt32(debugSplit[1]));
					break;
				case "watercolor":
					currentLocation.waterColor.Value = new Color(Convert.ToInt32(debugSplit[1]), Convert.ToInt32(debugSplit[2]), Convert.ToInt32(debugSplit[3])) * 0.5f;
					break;
				case "festivalscore":
					player.festivalScore += Convert.ToInt32(debugSplit[1]);
					break;
				case "addotherfarmer":
				{
					Farmer farmer = new Farmer(new FarmerSprite("Characters\\Farmer\\farmer_base"), new Vector2(player.Position.X - 64f, player.Position.Y), 2, Dialogue.randomName(), null, isMale: true);
					farmer.changeShirt(random.Next(40));
					farmer.changePants(new Color(random.Next(255), random.Next(255), random.Next(255)));
					farmer.changeHairStyle(random.Next(FarmerRenderer.hairStylesTexture.Height / 96 * 8));
					if (random.NextDouble() < 0.5)
					{
						farmer.changeHat(random.Next(-1, FarmerRenderer.hatsTexture.Height / 80 * 12));
					}
					else
					{
						player.changeHat(-1);
					}
					farmer.changeHairColor(new Color(random.Next(255), random.Next(255), random.Next(255)));
					farmer.changeSkinColor(random.Next(16));
					farmer.FarmerSprite.setOwner(farmer);
					farmer.currentLocation = currentLocation;
					otherFarmers.Add(random.Next(), farmer);
					break;
				}
				case "addkent":
					addKentIfNecessary();
					break;
				case "playmusic":
					changeMusicTrack(debugSplit[1]);
					break;
				case "jump":
				{
					float jumpVelocity = 8f;
					if (debugSplit.Length > 2)
					{
						jumpVelocity = (float)Convert.ToDouble(debugSplit[2]);
					}
					if (debugSplit[1].Equals("farmer"))
					{
						player.jump(jumpVelocity);
					}
					else
					{
						Utility.fuzzyCharacterSearch(debugSplit[1]).jump(jumpVelocity);
					}
					break;
				}
				case "toss":
					currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(738, 2700f, 1, 0, player.getTileLocation() * 64f, flicker: false, flipped: false)
					{
						rotationChange = (float)Math.PI / 32f,
						motion = new Vector2(0f, -6f),
						acceleration = new Vector2(0f, 0.08f)
					});
					break;
				case "rain":
					isRaining = !isRaining;
					isDebrisWeather = false;
					break;
				case "sf":
				case "setframe":
					player.FarmerSprite.PauseForSingleAnimation = true;
					player.FarmerSprite.setCurrentSingleAnimation(Convert.ToInt32(debugSplit[1]));
					break;
				case "endevent":
				case "leaveevent":
					pauseTime = 0f;
					player.eventsSeen.Clear();
					player.dialogueQuestionsAnswered.Clear();
					player.mailReceived.Clear();
					nonWarpFade = true;
					eventFinished();
					fadeScreenToBlack();
					viewportFreeze = false;
					break;
				case "language":
					activeClickableMenu = new LanguageSelectionMenu();
					break;
				case "runtestevent":
				case "rte":
					runTestEvent();
					break;
				case "qb":
				case "qiboard":
					activeClickableMenu = new SpecialOrdersBoard("Qi");
					break;
				case "ob":
				case "ordersboard":
					activeClickableMenu = new SpecialOrdersBoard();
					break;
				case "returneddonations":
					player.team.CheckReturnedDonations();
					break;
				case "completespecialorders":
				case "cso":
					foreach (SpecialOrder specialOrder2 in player.team.specialOrders)
					{
						foreach (OrderObjective objective in specialOrder2.objectives)
						{
							objective.SetCount(objective.maxCount.Value);
						}
					}
					break;
				case "specialorder":
				{
					SpecialOrder specialOrder = SpecialOrder.GetSpecialOrder(debugSplit[1], null);
					if (specialOrder != null)
					{
						player.team.specialOrders.Add(specialOrder);
					}
					break;
				}
				case "boatjourney":
					currentMinigame = new BoatJourney();
					break;
				case "minigame":
				{
					string text7 = debugSplit[1];
					if (text7 == null)
					{
						break;
					}
					switch (text7.Length)
					{
					case 6:
						switch (text7[0])
						{
						case 'c':
							if (text7 == "cowboy")
							{
								updateViewportForScreenSizeChange(fullscreenChange: false, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
								currentMinigame = new AbigailGame();
							}
							break;
						case 't':
							if (text7 == "target")
							{
								currentMinigame = new TargetGame();
							}
							break;
						}
						break;
					case 8:
						switch (text7[0])
						{
						case 'b':
							if (text7 == "blastoff")
							{
								currentMinigame = new RobotBlastoff();
							}
							break;
						case 'm':
							if (text7 == "minecart")
							{
								currentMinigame = new MineCart(0, 3);
							}
							break;
						}
						break;
					case 7:
						switch (text7[0])
						{
						case 'g':
							if (text7 == "grandpa")
							{
								currentMinigame = new GrandpaStory();
							}
							break;
						case 'f':
							if (text7 == "fishing")
							{
								currentMinigame = new FishingGame();
							}
							break;
						}
						break;
					case 9:
						switch (text7[0])
						{
						case 'm':
							if (text7 == "marucomet")
							{
								currentMinigame = new MaruComet();
							}
							break;
						case 'h':
							if (text7 == "haleyCows")
							{
								currentMinigame = new HaleyCowPictures();
							}
							break;
						}
						break;
					case 5:
						switch (text7[0])
						{
						case 'p':
							if (text7 == "plane")
							{
								currentMinigame = new PlaneFlyBy();
							}
							break;
						case 's':
							if (text7 == "slots")
							{
								currentMinigame = new Slots();
							}
							break;
						}
						break;
					}
					break;
				}
				case "event":
				{
					if (debugSplit.Length <= 3)
					{
						player.eventsSeen.Clear();
					}
					GameLocation gameLocation = Utility.fuzzyLocationSearch(debugSplit[1]);
					if (gameLocation == null)
					{
						debugOutput = "No location with name " + debugSplit[1];
						break;
					}
					string locationName = gameLocation.Name;
					if (locationName == "Pool")
					{
						locationName = "BathHouse_Pool";
					}
					if (content.Load<Dictionary<string, string>>("Data\\Events\\" + locationName).ElementAt(Convert.ToInt32(debugSplit[2])).Key.Contains('/'))
					{
						LocationRequest locationRequest2 = getLocationRequest(locationName);
						locationRequest2.OnLoad += delegate
						{
							currentLocation.currentEvent = new Event(content.Load<Dictionary<string, string>>("Data\\Events\\" + locationName).ElementAt(Convert.ToInt32(debugSplit[2])).Value, Convert.ToInt32(content.Load<Dictionary<string, string>>("Data\\Events\\" + locationName).ElementAt(Convert.ToInt32(debugSplit[2])).Key.Split('/')[0]));
						};
						warpFarmer(locationRequest2, 8, 8, player.FacingDirection);
					}
					break;
				}
				case "ebi":
				case "eventbyid":
					if (debugSplit.Length < 1)
					{
						debugOutput = "Event ID not specified";
						return true;
					}
					foreach (GameLocation location4 in locations)
					{
						string text6 = location4.Name;
						if (text6 == "Pool")
						{
							text6 = "BathHouse_Pool";
						}
						Dictionary<string, string> location_events = null;
						try
						{
							location_events = content.Load<Dictionary<string, string>>("Data\\Events\\" + text6);
						}
						catch (Exception)
						{
							continue;
						}
						if (location_events == null)
						{
							continue;
						}
						foreach (string key in location_events.Keys)
						{
							string[] array = key.Split('/');
							if (!(array[0] == debugSplit[1]))
							{
								continue;
							}
							int event_id = -1;
							if (int.TryParse(array[0], out event_id))
							{
								while (player.eventsSeen.Contains(event_id))
								{
									player.eventsSeen.Remove(event_id);
								}
							}
							LocationRequest locationRequest = getLocationRequest(text6);
							locationRequest.OnLoad += delegate
							{
								currentLocation.currentEvent = new Event(location_events[key], event_id);
							};
							int x = 8;
							int y = 8;
							Utility.getDefaultWarpLocation(locationRequest.Name, ref x, ref y);
							warpFarmer(locationRequest, x, y, player.FacingDirection);
							debugOutput = "Starting event " + key;
							return true;
						}
					}
					debugOutput = "Event not found.";
					break;
				case "festival":
				{
					Dictionary<string, string> dictionary = temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + debugSplit[1]);
					if (dictionary != null)
					{
						string text4 = new string(debugSplit[1].Where(char.IsLetter).ToArray());
						int num3 = Convert.ToInt32(new string(debugSplit[1].Where(char.IsDigit).ToArray()));
						parseDebugInput("season " + text4);
						parseDebugInput("day " + num3);
						parseDebugInput("time " + Convert.ToInt32(dictionary["conditions"].Split('/')[1].Split(' ')[0]));
						string text5 = dictionary["conditions"].Split('/')[0];
						parseDebugInput("warp " + text5 + " 1 1");
					}
					break;
				}
				case "ps":
				case "playsound":
					playSound(debugSplit[1]);
					break;
				case "crafting":
					foreach (string key6 in CraftingRecipe.craftingRecipes.Keys)
					{
						if (!player.craftingRecipes.ContainsKey(key6))
						{
							player.craftingRecipes.Add(key6, 0);
						}
					}
					break;
				case "cooking":
					foreach (string key7 in CraftingRecipe.cookingRecipes.Keys)
					{
						if (!player.cookingRecipes.ContainsKey(key7))
						{
							player.cookingRecipes.Add(key7, 0);
						}
					}
					break;
				case "experience":
				{
					int which = 0;
					if (debugSplit[1].Count() > 1)
					{
						switch (debugSplit[1].ToLower())
						{
						case "farming":
							which = 0;
							break;
						case "fishing":
							which = 1;
							break;
						case "mining":
							which = 3;
							break;
						case "foraging":
							which = 2;
							break;
						case "combat":
							which = 4;
							break;
						}
					}
					else
					{
						which = Convert.ToInt32(debugSplit[1]);
					}
					player.gainExperience(which, Convert.ToInt32(debugSplit[2]));
					break;
				}
				case "showexperience":
					debugOutput = Convert.ToString(player.experiencePoints[Convert.ToInt32(debugSplit[1])]);
					break;
				case "profession":
					player.professions.Add(Convert.ToInt32(debugSplit[1]));
					break;
				case "clearfishcaught":
					player.fishCaught.Clear();
					break;
				case "fishcaught":
				case "caughtfish":
					stats.FishCaught = (uint)Convert.ToInt32(debugSplit[1]);
					break;
				case "r":
					currentLocation.cleanupBeforePlayerExit();
					currentLocation.resetForPlayerEntry();
					break;
				case "fish":
					activeClickableMenu = new BobberBar(Convert.ToInt32(debugSplit[1]), 0.5f, treasure: true, ((player.CurrentTool as FishingRod).attachments[1] != null) ? (player.CurrentTool as FishingRod).attachments[1].ParentSheetIndex : (-1));
					break;
				case "growanimals":
					foreach (FarmAnimal value9 in (currentLocation as AnimalHouse).animals.Values)
					{
						value9.age.Value = (byte)value9.ageWhenMature - 1;
						value9.dayUpdate(currentLocation);
					}
					break;
				case "growanimalsfarm":
					foreach (FarmAnimal value10 in (currentLocation as Farm).animals.Values)
					{
						if (value10.isBaby())
						{
							value10.age.Value = (byte)value10.ageWhenMature - 1;
							value10.dayUpdate(currentLocation);
						}
					}
					break;
				case "pauseanimals":
					if (!(currentLocation is IAnimalLocation))
					{
						break;
					}
					foreach (FarmAnimal value11 in (currentLocation as IAnimalLocation).Animals.Values)
					{
						value11.pauseTimer = 2147483647;
					}
					break;
				case "unpauseanimals":
					if (!(currentLocation is IAnimalLocation))
					{
						break;
					}
					foreach (FarmAnimal value12 in (currentLocation as IAnimalLocation).Animals.Values)
					{
						value12.pauseTimer = 0;
					}
					break;
				case "removeterrainfeatures":
				case "removetf":
					currentLocation.terrainFeatures.Clear();
					break;
				case "mushroomtrees":
					foreach (TerrainFeature value13 in currentLocation.terrainFeatures.Values)
					{
						if (value13 is Tree)
						{
							(value13 as Tree).treeType.Value = 7;
						}
					}
					break;
				case "trashcan":
					player.trashCanLevel = Convert.ToInt32(debugSplit[1]);
					break;
				case "addquartz":
				{
					if (debugSplit.Length <= 1)
					{
						break;
					}
					for (int i = 0; i < Convert.ToInt32(debugSplit[1]) - 1; i++)
					{
						Vector2 randomTile = getFarm().getRandomTile();
						if (!getFarm().terrainFeatures.ContainsKey(randomTile))
						{
							getFarm().terrainFeatures.Add(randomTile, new Quartz(1 + random.Next(2), Utility.getRandomRainbowColor()));
						}
					}
					break;
				}
				case "fruittrees":
					foreach (KeyValuePair<Vector2, TerrainFeature> pair2 in currentLocation.terrainFeatures.Pairs)
					{
						if (pair2.Value is FruitTree)
						{
							(pair2.Value as FruitTree).daysUntilMature.Value -= 27;
							pair2.Value.dayUpdate(currentLocation, pair2.Key);
						}
					}
					break;
				case "train":
					(getLocationFromName("Railroad") as Railroad).setTrainComing(7500);
					break;
				case "debrisweather":
					ClearDebrisWeather(debrisWeather);
					baseDebrisWeatherCount = 0;
					isDebrisWeather = !isDebrisWeather;
					if (isDebrisWeather)
					{
						populateDebrisWeatherArray(debrisWeather);
					}
					break;
				case "speed":
				{
					if (debugSplit.Length < 2)
					{
						debugOutput = "Missing parameters. Run as: 'speed <value> (minutes=30)'";
						break;
					}
					for (int num74 = buffsDisplay.otherBuffs.Count - 1; num74 >= 0; num74--)
					{
						if (buffsDisplay.otherBuffs[num74].source == "Debug Speed")
						{
							buffsDisplay.otherBuffs[num74].removeBuff();
							buffsDisplay.otherBuffs.RemoveAt(num74);
						}
					}
					int minutesDuration = 30;
					if (debugSplit.Length > 2)
					{
						minutesDuration = Convert.ToInt32(debugSplit[2]);
					}
					buffsDisplay.addOtherBuff(new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, Convert.ToInt32(debugSplit[1]), 0, 0, minutesDuration, "Debug Speed", "Debug Speed"));
					break;
				}
				case "dayupdate":
					currentLocation.DayUpdate(dayOfMonth);
					if (debugSplit.Length > 1)
					{
						for (int num73 = 0; num73 < Convert.ToInt32(debugSplit[1]) - 1; num73++)
						{
							currentLocation.DayUpdate(dayOfMonth);
						}
					}
					break;
				case "museumloot":
					foreach (KeyValuePair<int, string> item11 in objectInformation)
					{
						string text19 = item11.Value.Split('/')[3];
						if ((text19.Contains("Arch") || text19.Contains("Minerals")) && !player.mineralsFound.ContainsKey(item11.Key) && !player.archaeologyFound.ContainsKey(item11.Key))
						{
							if (text19.Contains("Arch"))
							{
								player.foundArtifact(item11.Key, 1);
							}
							else
							{
								player.addItemToInventoryBool(new Object(item11.Key, 1));
							}
						}
						if (player.freeSpotsInInventory() == 0)
						{
							return true;
						}
					}
					break;
				case "newmuseumloot":
					foreach (KeyValuePair<int, string> item12 in objectInformation)
					{
						string text18 = item12.Value.Split('/')[3];
						if ((text18.Contains("Arch") || text18.Contains("Minerals")) && !netWorldState.Value.MuseumPieces.Values.Contains(item12.Key))
						{
							player.addItemToInventoryBool(new Object(item12.Key, 1));
						}
						if (player.freeSpotsInInventory() == 0)
						{
							return true;
						}
					}
					break;
				case "slingshot":
					player.addItemToInventoryBool(new Slingshot());
					playSound("coin");
					break;
				case "ring":
					player.addItemToInventoryBool(new Ring(Convert.ToInt32(debugSplit[1])));
					playSound("coin");
					break;
				case "boots":
					player.addItemToInventoryBool(new Boots(Convert.ToInt32(debugSplit[1])));
					playSound("coin");
					break;
				case "mainmenu":
				case "createdebris":
					if (debugSplit.Length < 2)
					{
						debugOutput = "Invalid parameters; call like: createDebris <itemId>";
					}
					else
					{
						createObjectDebris(Convert.ToInt32(debugSplit[1]), player.getTileX(), player.getTileY());
					}
					break;
				case "removedebris":
					currentLocation.debris.Clear();
					break;
				case "removedirt":
				{
					for (int num72 = currentLocation.terrainFeatures.Count() - 1; num72 >= 0; num72--)
					{
						if (currentLocation.terrainFeatures.Pairs.ElementAt(num72).Value is HoeDirt)
						{
							currentLocation.terrainFeatures.Remove(currentLocation.terrainFeatures.Pairs.ElementAt(num72).Key);
						}
					}
					break;
				}
				case "dyeAll":
					activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.DyePots);
					break;
				case "dyeshirt":
					activeClickableMenu = new CharacterCustomization(player.shirtItem.Value);
					break;
				case "dyepants":
					activeClickableMenu = new CharacterCustomization(player.pantsItem.Value);
					break;
				case "cmenu":
				case "customize":
				case "customizemenu":
					activeClickableMenu = new CharacterCustomization();
					break;
				case "shrinemenu":
					activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.Wizard);
					break;
				case "skincolor":
					player.changeSkinColor(Convert.ToInt32(debugSplit[1]));
					break;
				case "hat":
					player.changeHat(Convert.ToInt32(debugSplit[1]));
					playSound("coin");
					break;
				case "pants":
					player.changePants(new Color(Convert.ToInt32(debugSplit[1]), Convert.ToInt32(debugSplit[2]), Convert.ToInt32(debugSplit[3])));
					break;
				case "hairstyle":
					player.changeHairStyle(Convert.ToInt32(debugSplit[1]));
					break;
				case "haircolor":
					player.changeHairColor(new Color(Convert.ToInt32(debugSplit[1]), Convert.ToInt32(debugSplit[2]), Convert.ToInt32(debugSplit[3])));
					break;
				case "shirt":
					player.changeShirt(Convert.ToInt32(debugSplit[1]));
					break;
				case "mv":
				case "m":
				case "musicvolume":
					musicPlayerVolume = (float)Convert.ToDouble(debugSplit[1]);
					options.musicVolumeLevel = (float)Convert.ToDouble(debugSplit[1]);
					musicCategory.SetVolume(options.musicVolumeLevel);
					break;
				case "removeobjects":
					currentLocation.objects.Clear();
					break;
				case "removelights":
					currentLightSources.Clear();
					break;
				case "i":
				case "item":
					if (objectInformation.ContainsKey(Convert.ToInt32(debugSplit[1])))
					{
						playSound("coin");
						player.addItemToInventoryBool(new Object(Convert.ToInt32(debugSplit[1]), (debugSplit.Length < 3) ? 1 : Convert.ToInt32(debugSplit[2]), isRecipe: false, -1, (debugSplit.Length >= 4) ? Convert.ToInt32(debugSplit[3]) : 0));
					}
					break;
				case "dyemenu":
					activeClickableMenu = new DyeMenu();
					break;
				case "tailor":
					activeClickableMenu = new TailoringMenu();
					break;
				case "forge":
					activeClickableMenu = new ForgeMenu();
					break;
				case "listtags":
				{
					if (player.CurrentItem == null)
					{
						break;
					}
					string text17 = "Tags on " + player.CurrentItem.DisplayName + ": ";
					foreach (string contextTag in player.CurrentItem.GetContextTagList())
					{
						text17 = text17 + contextTag + " ";
					}
					debugOutput = text17.Trim();
					break;
				}
				case "dye":
				{
					Color color = Color.White;
					switch (debugSplit[2].ToLower().Trim())
					{
					case "black":
						color = Color.Black;
						break;
					case "red":
						color = new Color(220, 0, 0);
						break;
					case "blue":
						color = new Color(0, 100, 220);
						break;
					case "yellow":
						color = new Color(255, 230, 0);
						break;
					case "white":
						color = Color.White;
						break;
					case "green":
						color = new Color(10, 143, 0);
						break;
					}
					float strength = 1f;
					if (debugSplit.Length > 2)
					{
						strength = float.Parse(debugSplit[3]);
					}
					string text16 = debugSplit[1].ToLower().Trim();
					if (!(text16 == "shirt"))
					{
						if (text16 == "pants" && player.pantsItem.Value != null)
						{
							player.pantsItem.Value.Dye(color, strength);
						}
					}
					else if (player.shirtItem.Value != null)
					{
						player.shirtItem.Value.Dye(color, strength);
					}
					break;
				}
				case "clothes":
					playSound("coin");
					player.addItemToInventoryBool(new Clothing(Convert.ToInt32(debugSplit[1])));
					break;
				case "getindex":
				{
					Item item3 = Utility.fuzzyItemSearch(debugSplit[1]);
					if (item3 != null)
					{
						debugOutput = item3.DisplayName + "'s index is " + item3.ParentSheetIndex;
					}
					else
					{
						debugOutput = "No item found with name " + debugSplit[1];
					}
					break;
				}
				case "fuzzyitemnamed":
				case "fin":
				case "f":
				{
					int result3 = -1;
					int result4 = 1;
					if (debugSplit.Length > 2)
					{
						int.TryParse(debugSplit[2], out result4);
					}
					if (debugSplit.Length > 3)
					{
						int.TryParse(debugSplit[3], out result3);
					}
					Item item2 = Utility.fuzzyItemSearch(debugSplit[1], result4);
					if (item2 != null)
					{
						if (result3 >= 0 && item2 is Object)
						{
							(item2 as Object).quality.Value = result3;
						}
						player.addItemToInventory(item2);
						playSound("coin");
						string text15 = item2.GetType().ToString();
						if (text15.Contains('.'))
						{
							text15 = text15.Substring(text15.LastIndexOf('.') + 1);
							if (item2 is Object && (bool)(item2 as Object).bigCraftable)
							{
								text15 = "Big Craftable";
							}
						}
						debugOutput = "Added " + item2.DisplayName + " (" + text15 + ")";
					}
					else
					{
						debugOutput = "No item found with name " + debugSplit[1];
					}
					break;
				}
				case "in":
				case "itemnamed":
					foreach (int key8 in objectInformation.Keys)
					{
						if (objectInformation[key8].Substring(0, objectInformation[key8].IndexOf('/')).ToLower().Replace(" ", "")
							.Equals(debugSplit[1].ToLower()))
						{
							player.addItemToInventory(new Object(key8, (debugSplit.Length < 3) ? 1 : Convert.ToInt32(debugSplit[2]), isRecipe: false, -1, (debugSplit.Length >= 4) ? Convert.ToInt32(debugSplit[3]) : 0));
							playSound("coin");
						}
					}
					break;
				case "achievement":
					getAchievement(Convert.ToInt32(debugSplit[1]));
					break;
				case "heal":
					player.health = player.maxHealth;
					break;
				case "die":
					player.health = 0;
					break;
				case "energize":
					player.Stamina = player.MaxStamina;
					if (debugSplit.Length > 1)
					{
						player.Stamina = Convert.ToInt32(debugSplit[1]);
					}
					break;
				case "exhaust":
					player.Stamina = -15f;
					break;
				case "warp":
				{
					GameLocation gameLocation2 = Utility.fuzzyLocationSearch(debugSplit[1]);
					if (gameLocation2 != null)
					{
						int x2 = 0;
						int y2 = 0;
						if (debugSplit.Length >= 4)
						{
							x2 = Convert.ToInt32(debugSplit[2]);
							y2 = Convert.ToInt32(debugSplit[3]);
						}
						else
						{
							Utility.getDefaultWarpLocation(gameLocation2.Name, ref x2, ref y2);
						}
						warpFarmer(new LocationRequest(gameLocation2.NameOrUniqueName, gameLocation2.uniqueName.Value != null, gameLocation2), x2, y2, 2);
						debugOutput = "Warping player to " + gameLocation2.NameOrUniqueName + " at " + x2 + ", " + y2;
					}
					else
					{
						debugOutput = "No location with name " + debugSplit[1];
					}
					break;
				}
				case "wh":
				case "warphome":
					warpHome();
					break;
				case "money":
					player.Money = Convert.ToInt32(debugSplit[1]);
					break;
				case "perfection":
				{
					parseDebugInput("friendAll");
					parseDebugInput("cooking");
					parseDebugInput("crafting");
					for (int num57 = player.craftingRecipes.Count() - 1; num57 >= 0; num57--)
					{
						player.craftingRecipes[player.craftingRecipes.Pairs.ElementAt(num57).Key] = 1;
					}
					foreach (KeyValuePair<int, string> item13 in objectInformation)
					{
						if (item13.Value.Split('/')[3].Contains("Fish"))
						{
							player.fishCaught.Add(item13.Key, new int[3]);
						}
						string text14 = item13.Value.Split('/')[3];
						if (Object.isPotentialBasicShippedCategory(item13.Key, text14.Substring(text14.Length - 3)))
						{
							player.basicShipped.Add(item13.Key, 1);
						}
						player.recipesCooked.Add(item13.Key, 1);
					}
					parseDebugInput("walnut 130");
					player.mailReceived.Add("CF_Fair");
					player.mailReceived.Add("CF_Fish");
					player.mailReceived.Add("CF_Sewer");
					player.mailReceived.Add("CF_Mines");
					player.mailReceived.Add("CF_Spouse");
					player.mailReceived.Add("CF_Statue");
					player.mailReceived.Add("museumComplete");
					player.miningLevel.Value = 10;
					player.fishingLevel.Value = 10;
					player.foragingLevel.Value = 10;
					player.combatLevel.Value = 10;
					player.farmingLevel.Value = 10;
					getFarm().buildStructure(new BluePrint("Water Obelisk"), new Vector2(0f, 0f), player, magicalConstruction: true, skipSafetyChecks: true);
					getFarm().buildStructure(new BluePrint("Earth Obelisk"), new Vector2(4f, 0f), player, magicalConstruction: true, skipSafetyChecks: true);
					getFarm().buildStructure(new BluePrint("Desert Obelisk"), new Vector2(8f, 0f), player, magicalConstruction: true, skipSafetyChecks: true);
					getFarm().buildStructure(new BluePrint("Island Obelisk"), new Vector2(12f, 0f), player, magicalConstruction: true, skipSafetyChecks: true);
					getFarm().buildStructure(new BluePrint("Gold Clock"), new Vector2(16f, 0f), player, magicalConstruction: true, skipSafetyChecks: true);
					Dictionary<string, string> dictionary7 = content.Load<Dictionary<string, string>>("Data\\Monsters");
					foreach (KeyValuePair<string, string> item14 in dictionary7)
					{
						for (int num58 = 0; num58 < 500; num58++)
						{
							stats.monsterKilled(item14.Key);
						}
					}
					break;
				}
				case "walnut":
					netWorldState.Value.GoldenWalnuts.Value += Convert.ToInt32(debugSplit[1]);
					netWorldState.Value.GoldenWalnutsFound.Value += Convert.ToInt32(debugSplit[1]);
					break;
				case "gem":
					player.QiGems += Convert.ToInt32(debugSplit[1]);
					break;
				case "killnpc":
				{
					for (int num55 = locations.Count - 1; num55 >= 0; num55--)
					{
						for (int num56 = 0; num56 < locations[num55].characters.Count; num56++)
						{
							if (locations[num55].characters[num56].Name.Equals(debugSplit[1]))
							{
								locations[num55].characters.RemoveAt(num56);
								break;
							}
						}
					}
					break;
				}
				case "dap":
				case "daysplayed":
					showGlobalMessage(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3332", (int)stats.DaysPlayed));
					break;
				case "friendall":
					foreach (NPC allCharacter in Utility.getAllCharacters())
					{
						if ((allCharacter.CanSocialize || !(allCharacter.Name != "Sandy") || !(allCharacter.Name == "Krobus")) && !(allCharacter.Name == "Marlon"))
						{
							if (allCharacter != null && !player.friendshipData.ContainsKey(allCharacter.Name))
							{
								player.friendshipData.Add(allCharacter.Name, new Friendship());
							}
							player.changeFriendship((debugSplit.Count() > 1) ? Convert.ToInt32(debugSplit[1]) : 2500, allCharacter);
						}
					}
					break;
				case "friend":
				case "friendship":
				{
					NPC nPC8 = Utility.fuzzyCharacterSearch(debugSplit[1]);
					if (nPC8 != null && !player.friendshipData.ContainsKey(nPC8.Name))
					{
						player.friendshipData.Add(nPC8.Name, new Friendship());
					}
					player.friendshipData[nPC8.Name].Points = Convert.ToInt32(debugSplit[2]);
					break;
				}
				case "getstat":
					debugOutput = stats.GetType().GetProperty(debugSplit[1]).GetValue(stats, null)
						.ToString();
					break;
				case "setstat":
					stats.GetType().GetProperty(debugSplit[1]).SetValue(stats, Convert.ToUInt32(debugSplit[2]), null);
					break;
				case "eventseen":
				case "seenevent":
					player.eventsSeen.Add(Convert.ToInt32(debugSplit[1]));
					break;
				case "seenmail":
					player.mailReceived.Add(debugSplit[1]);
					break;
				case "cookingrecipe":
					player.cookingRecipes.Add(debugInput.Substring(debugInput.IndexOf(' ')).Trim(), 0);
					break;
				case "craftingrecipe":
					player.craftingRecipes.Add(debugInput.Substring(debugInput.IndexOf(' ')).Trim(), 0);
					break;
				case "upgradehouse":
					player.HouseUpgradeLevel = Math.Min(3, player.HouseUpgradeLevel + 1);
					removeFrontLayerForFarmBuildings();
					addNewFarmBuildingMaps();
					break;
				case "coop":
				case "upgradecoop":
					player.CoopUpgradeLevel = Math.Min(3, player.CoopUpgradeLevel + 1);
					removeFrontLayerForFarmBuildings();
					addNewFarmBuildingMaps();
					break;
				case "barn":
				case "upgradebarn":
					player.BarnUpgradeLevel = Math.Min(3, player.BarnUpgradeLevel + 1);
					removeFrontLayerForFarmBuildings();
					addNewFarmBuildingMaps();
					break;
				case "resource":
					Debris.getDebris(Convert.ToInt32(debugSplit[1]), Convert.ToInt32(debugSplit[2]));
					break;
				case "weapon":
					player.addItemToInventoryBool(new MeleeWeapon(Convert.ToInt32(debugSplit[1])));
					break;
				case "stoprafting":
					player.isRafting = false;
					break;
				case "time":
					timeOfDay = Convert.ToInt32(debugSplit[1]);
					outdoorLight = Color.White;
					break;
				case "addminute":
					addMinute();
					break;
				case "addhour":
					addHour();
					break;
				case "water":
					foreach (TerrainFeature value14 in currentLocation.terrainFeatures.Values)
					{
						if (value14 is HoeDirt)
						{
							(value14 as HoeDirt).state.Value = 1;
						}
					}
					break;
				case "growcrops":
					foreach (KeyValuePair<Vector2, TerrainFeature> pair3 in currentLocation.terrainFeatures.Pairs)
					{
						if (!(pair3.Value is HoeDirt) || (pair3.Value as HoeDirt).crop == null)
						{
							continue;
						}
						for (int num33 = 0; num33 < Convert.ToInt32(debugSplit[1]); num33++)
						{
							if ((pair3.Value as HoeDirt).crop != null)
							{
								(pair3.Value as HoeDirt).crop.newDay(1, -1, (int)pair3.Key.X, (int)pair3.Key.Y, currentLocation);
							}
						}
					}
					break;
				case "b":
				case "bi":
				case "big":
				case "bigitem":
					if (bigCraftablesInformation.ContainsKey(Convert.ToInt32(debugSplit[1])))
					{
						playSound("coin");
						player.addItemToInventory(new Object(Vector2.Zero, Convert.ToInt32(debugSplit[1]))
						{
							Stack = ((debugSplit.Count() <= 2) ? 1 : Convert.ToInt32(debugSplit[2]))
						});
					}
					break;
				case "cm":
				case "c":
				case "canmove":
					player.isEating = false;
					player.CanMove = true;
					player.UsingTool = false;
					player.usingSlingshot = false;
					player.FarmerSprite.PauseForSingleAnimation = false;
					if (player.CurrentTool is FishingRod)
					{
						(player.CurrentTool as FishingRod).isFishing = false;
					}
					if (player.mount != null)
					{
						player.mount.dismount();
					}
					break;
				case "backpack":
					player.increaseBackpackSize(Math.Min(36 - player.items.Count(), Convert.ToInt32(debugSplit[1])));
					break;
				case "question":
					player.dialogueQuestionsAnswered.Add(Convert.ToInt32(debugSplit[1]));
					break;
				case "year":
					year = Convert.ToInt32(debugSplit[1]);
					break;
				case "day":
					stats.DaysPlayed = (uint)(Utility.getSeasonNumber(currentSeason) * 28 + Convert.ToInt32(debugSplit[1]) + (year - 1) * 4 * 28);
					dayOfMonth = Convert.ToInt32(debugSplit[1]);
					break;
				case "season":
					if (debugSplit.Length >= 1)
					{
						int seasonNumber = Utility.getSeasonNumber(debugSplit[1].ToLower());
						if (seasonNumber >= 0)
						{
							currentSeason = debugSplit[1].ToLower();
							setGraphicsForSeason();
						}
					}
					break;
				case "dialogue":
					Utility.fuzzyCharacterSearch(debugSplit[1]).CurrentDialogue.Push(new Dialogue(debugInput.Substring(debugInput.IndexOf("0") + 1), Utility.fuzzyCharacterSearch(debugSplit[1])));
					break;
				case "speech":
					Utility.fuzzyCharacterSearch(debugSplit[1]).CurrentDialogue.Push(new Dialogue(debugInput.Substring(debugInput.IndexOf("0") + 1), Utility.fuzzyCharacterSearch(debugSplit[1])));
					drawDialogue(Utility.fuzzyCharacterSearch(debugSplit[1]));
					break;
				case "loaddialogue":
				{
					NPC nPC7 = Utility.fuzzyCharacterSearch(debugSplit[1]);
					string path = debugSplit[2];
					string text13 = content.LoadString(path);
					text13 = text13.Replace("{", "<");
					text13 = text13.Replace("}", ">");
					nPC7.CurrentDialogue.Push(new Dialogue(text13, nPC7));
					drawDialogue(Utility.fuzzyCharacterSearch(debugSplit[1]));
					break;
				}
				case "wedding":
					player.spouse = debugSplit[1];
					weddingsToday.Add(player.UniqueMultiplayerID);
					break;
				case "gamemode":
					setGameMode(Convert.ToByte(debugSplit[1]));
					break;
				case "volcano":
					warpFarmer("VolcanoDungeon" + Convert.ToInt32(debugSplit[1]), 0, 1, 2);
					break;
				case "minelevel":
					enterMine(Convert.ToInt32(debugSplit[1]));
					break;
				case "mineinfo":
					debugOutput = "MineShaft.lowestLevelReached = " + MineShaft.lowestLevelReached + "\nplayer.deepestMineLevel = " + player.deepestMineLevel;
					break;
				case "tool":
					player.getToolFromName(debugSplit[1]).UpgradeLevel = Convert.ToInt32(debugSplit[2]);
					break;
				case "viewport":
					viewport.X = Convert.ToInt32(debugSplit[1]) * 64;
					viewport.Y = Convert.ToInt32(debugSplit[2]) * 64;
					break;
				case "makeinedible":
					if (player.ActiveObject != null)
					{
						player.ActiveObject.edibility.Value = -300;
					}
					break;
				case "watm":
				case "warpanimaltome":
				{
					if (!(currentLocation is IAnimalLocation))
					{
						debugOutput = "Animals not allowed in current location.";
						break;
					}
					IAnimalLocation animalLocation = currentLocation as IAnimalLocation;
					FarmAnimal farmAnimal = Utility.fuzzyAnimalSearch(debugSplit[1]);
					if (farmAnimal != null)
					{
						debugOutput = "Warping " + farmAnimal.displayName;
						(farmAnimal.currentLocation as IAnimalLocation).Animals.Remove(farmAnimal.myID);
						animalLocation.Animals.Add(farmAnimal.myID, farmAnimal);
						farmAnimal.Position = player.Position;
						farmAnimal.controller = null;
					}
					else
					{
						debugOutput = "Couldn't find character named " + debugSplit[1];
					}
					break;
				}
				case "wctm":
				case "warpcharactertome":
				{
					NPC nPC6 = Utility.fuzzyCharacterSearch(debugSplit[1], must_be_villager: false);
					if (nPC6 != null)
					{
						debugOutput = "Warping " + nPC6.displayName;
						warpCharacter(nPC6, currentLocation.Name, new Vector2(player.getTileX(), player.getTileY()));
						nPC6.controller = null;
						nPC6.Halt();
					}
					else
					{
						debugOutput = "Couldn't find character named " + debugSplit[1];
					}
					break;
				}
				case "wc":
				case "warpcharacter":
				{
					NPC nPC4 = Utility.fuzzyCharacterSearch(debugSplit[1], must_be_villager: false);
					if (nPC4 == null)
					{
						break;
					}
					if (debugSplit.Length < 4)
					{
						debugOutput = "Missing parameters, run as: 'wc <npcName> <x> <y> [facingDirection=1]'";
						break;
					}
					int direction2 = 2;
					if (debugSplit.Length >= 5)
					{
						direction2 = Convert.ToInt32(debugSplit[4]);
					}
					warpCharacter(nPC4, currentLocation.Name, new Vector2(Convert.ToInt32(debugSplit[2]), Convert.ToInt32(debugSplit[3])));
					nPC4.faceDirection(direction2);
					nPC4.controller = null;
					nPC4.Halt();
					break;
				}
				case "wtp":
				case "warptoplayer":
				{
					if (debugSplit.Length < 2)
					{
						debugOutput = "Missing parameters, run as: 'wtp <playerName>'";
						break;
					}
					string text11 = debugSplit[1].ToLower().Replace(" ", "");
					Farmer farmer2 = null;
					foreach (Farmer onlineFarmer2 in getOnlineFarmers())
					{
						if (onlineFarmer2.displayName.Replace(" ", "").ToLower() == text11)
						{
							farmer2 = onlineFarmer2;
							break;
						}
					}
					if (farmer2 == null)
					{
						debugOutput = "Could not find other farmer " + debugSplit[1];
						break;
					}
					parseDebugInput("warp " + farmer2.currentLocation.NameOrUniqueName + " " + farmer2.getTileX() + " " + farmer2.getTileY());
					break;
				}
				case "wtc":
				case "warptocharacter":
				{
					if (debugSplit.Length < 2)
					{
						debugOutput = "Missing parameters, run as: 'wtc <npcName>'";
						break;
					}
					NPC nPC3 = Utility.fuzzyCharacterSearch(debugSplit[1]);
					if (nPC3 == null)
					{
						debugOutput = "Could not find valid character " + debugSplit[1];
						break;
					}
					parseDebugInput("warp " + Utility.getGameLocationOfCharacter(nPC3).Name + " " + nPC3.getTileX() + " " + nPC3.getTileY());
					break;
				}
				case "wct":
				case "warpcharacterto":
				{
					NPC nPC2 = Utility.fuzzyCharacterSearch(debugSplit[1]);
					if (nPC2 == null)
					{
						break;
					}
					if (debugSplit.Length < 5)
					{
						debugOutput = "Missing parameters, run as: 'wct <npcName> <locationName> <x> <y> [facingDirection=1]'";
						break;
					}
					int direction = 2;
					if (debugSplit.Length >= 6)
					{
						direction = Convert.ToInt32(debugSplit[4]);
					}
					warpCharacter(nPC2, debugSplit[2], new Vector2(Convert.ToInt32(debugSplit[3]), Convert.ToInt32(debugSplit[4])));
					nPC2.faceDirection(direction);
					nPC2.controller = null;
					nPC2.Halt();
					break;
				}
				case "ws":
				case "warpshop":
					if (debugSplit.Length < 2)
					{
						debugOutput = "Missing argument. Run as: 'warpshop <npcname>'";
						break;
					}
					text8 = debugSplit[1].ToLower();
					if (text8 != null)
					{
						switch (text8.Length)
						{
						case 6:
							break;
						case 5:
							goto IL_bb7a;
						case 3:
							goto IL_bbc1;
						default:
							goto IL_be24;
						}
						c = text8[0];
						if ((uint)c <= 109u)
						{
							if (c != 'k')
							{
								if (c == 'm' && text8 == "marnie")
								{
									parseDebugInput("warp AnimalShop 12 16");
									parseDebugInput("wct Marnie AnimalShop 12 14");
									break;
								}
							}
							else if (text8 == "krobus")
							{
								parseDebugInput("warp Sewer 31 19");
								break;
							}
						}
						else if (c != 'p')
						{
							if (c == 'w' && text8 == "wizard")
							{
								if (!player.eventsSeen.Contains(418172))
								{
									player.eventsSeen.Add(418172);
								}
								player.hasMagicInk = true;
								parseDebugInput("warp WizardHouse 2 14");
								break;
							}
						}
						else if (text8 == "pierre")
						{
							parseDebugInput("warp SeedShop 4 19");
							parseDebugInput("wct Pierre SeedShop 4 17");
							break;
						}
					}
					goto IL_be24;
				case "faceplayer":
					Utility.fuzzyCharacterSearch(debugSplit[1]).faceTowardFarmer = true;
					break;
				case "refuel":
					if (player.getToolFromName("Lantern") != null)
					{
						((Lantern)player.getToolFromName("Lantern")).fuelLeft = 100;
					}
					break;
				case "lantern":
					player.items.Add(new Lantern());
					break;
				case "growgrass":
					currentLocation.spawnWeeds(weedsOnly: false);
					currentLocation.growWeedGrass(Convert.ToInt32(debugSplit[1]));
					break;
				case "blueprint":
					player.blueprints.Add(debugInput.Substring(debugInput.IndexOf(' ')).Trim());
					break;
				case "bluebook":
					player.items.Add(new Blueprints());
					break;
				case "addallcrafting":
					foreach (string key9 in CraftingRecipe.craftingRecipes.Keys)
					{
						player.craftingRecipes.Add(key9, 0);
					}
					break;
				case "animal":
					Utility.addAnimalToFarm(new FarmAnimal(debugInput.Substring(debugInput.IndexOf(' ')).Trim(), multiplayer.getNewID(), player.UniqueMultiplayerID));
					break;
				case "movebuilding":
					getFarm().getBuildingAt(new Vector2(Convert.ToInt32(debugSplit[1]), Convert.ToInt32(debugSplit[2]))).tileX.Value = Convert.ToInt32(debugSplit[3]);
					getFarm().getBuildingAt(new Vector2(Convert.ToInt32(debugSplit[1]), Convert.ToInt32(debugSplit[2]))).tileY.Value = Convert.ToInt32(debugSplit[4]);
					break;
				case "ax":
					player.addItemToInventoryBool(new Axe());
					playSound("coin");
					break;
				case "hoe":
					player.addItemToInventoryBool(new Hoe());
					playSound("coin");
					break;
				case "wateringcan":
				case "can":
					player.addItemToInventoryBool(new WateringCan());
					playSound("coin");
					break;
				case "pickaxe":
				case "pickax":
				case "pick":
					player.addItemToInventoryBool(new Pickaxe());
					playSound("coin");
					break;
				case "wand":
					player.addItemToInventoryBool(new Wand());
					playSound("coin");
					break;
				case "fishing":
					player.FishingLevel = Convert.ToInt32(debugSplit[1]);
					break;
				case "igmt":
				{
					List<Item> list2 = new List<Item>();
					list2.Add(new Object(434, 1));
					list2.Add(new Object(499, 1, isRecipe: true));
					list2.Add(new Object(102, 1));
					list2.Add(new Object(326, 1));
					list2.Add(new Object(74, 1));
					activeClickableMenu = new ItemGrabMenu(list2, (debugSplit.Count() > 1) ? getLocationFromName("ArchaeologyHouse") : null);
					break;
				}
				case "ft":
				case "fishingtreasure":
					if (player.CurrentTool != null && player.CurrentTool is FishingRod)
					{
						(player.CurrentTool as FishingRod).openTreasureMenuEndFunction(-1);
					}
					break;
				case "eventover":
					eventFinished();
					break;
				case "fd":
				case "facedirection":
				case "face":
					if (debugSplit[1].Equals("farmer"))
					{
						player.Halt();
						player.completelyStopAnimatingOrDoingAction();
						player.faceDirection(Convert.ToInt32(debugSplit[2]));
					}
					else
					{
						Utility.fuzzyCharacterSearch(debugSplit[1]).faceDirection(Convert.ToInt32(debugSplit[2]));
					}
					break;
				case "note":
					if (!player.archaeologyFound.ContainsKey(102))
					{
						player.archaeologyFound.Add(102, new int[2]);
					}
					player.archaeologyFound[102][0] = 18;
					netWorldState.Value.LostBooksFound.Value = 18;
					currentLocation.readNote(Convert.ToInt32(debugSplit[1]));
					break;
				case "nethost":
					multiplayer.StartServer();
					break;
				case "levelup":
					if (debugSplit.Length > 3)
					{
						activeClickableMenu = new LevelUpMenu(Convert.ToInt32(debugSplit[1]), Convert.ToInt32(debugSplit[2]));
					}
					else
					{
						activeClickableMenu = new LevelUpMenu(Convert.ToInt32(debugSplit[1]), Convert.ToInt32(debugSplit[2]));
					}
					break;
				case "drawbounds":
					drawbounds = !drawbounds;
					break;
				case "darts":
					currentMinigame = new Darts();
					break;
				case "minegame":
				{
					int mode = 3;
					if (debugSplit.Length >= 2 && debugSplit[1] == "infinite")
					{
						mode = 2;
					}
					currentMinigame = new MineCart(0, mode);
					break;
				}
				case "oldminegame":
					currentMinigame = new OldMineCart(0, 3);
					break;
				case "crane":
					currentMinigame = new CraneGame();
					break;
				case "tailorrecipelisttool":
				case "trlt":
					activeClickableMenu = new TailorRecipeListTool();
					break;
				case "animationpreviewtool":
				case "apt":
					activeClickableMenu = new AnimationPreviewTool();
					break;
				case "createdino":
					currentLocation.characters.Add(new DinoMonster(player.position.Value + new Vector2(100f, 0f)));
					break;
				case "broadcastmail":
					if (debugSplit.Length > 1)
					{
						addMailForTomorrow(string.Join(" ", debugSplit.Skip(1)), noLetter: false, sendToEveryone: true);
					}
					break;
				case "phone":
					ShowTelephoneMenu();
					break;
				case "renovate":
					HouseRenovation.ShowRenovationMenu();
					break;
				case "crib":
				{
					GameLocation locationFromName = getLocationFromName(player.homeLocation.Value);
					if (locationFromName is FarmHouse farmHouse)
					{
						int value2 = Convert.ToInt32(debugSplit[1]);
						farmHouse.cribStyle.Value = value2;
					}
					break;
				}
				case "testnut":
					createItemDebris(new Object(73, 1), Vector2.Zero, 2);
					break;
				case "pstm":
				case "pathspousetome":
					if (player.getSpouse() != null)
					{
						NPC spouse = player.getSpouse();
						if (spouse.currentLocation != currentLocation)
						{
							warpCharacter(spouse, currentLocation.NameOrUniqueName, player.getTileLocationPoint());
						}
						spouse.exploreFarm.Value = true;
						player.getSpouse().PathToOnFarm(player.getTileLocationPoint());
					}
					break;
				case "shufflebundles":
					GenerateBundles(BundleType.Remixed, use_seed: false);
					break;
				case "split":
					if (debugSplit.Length >= 2)
					{
						int player_index = int.Parse(debugSplit[1]);
						GameRunner.instance.AddGameInstance((PlayerIndex)player_index);
					}
					else
					{
						ShowLocalCoopJoinMenu();
					}
					break;
				case "bpm":
				{
					Building buildingAt = getFarm().getBuildingAt(player.getTileLocation() + new Vector2(0f, -1f));
					if (buildingAt != null)
					{
						activeClickableMenu = new BuildingPaintMenu(buildingAt);
						break;
					}
					Farm farm_location = getFarm();
					activeClickableMenu = new BuildingPaintMenu("House", () => (farm_location.paintedHouseTexture != null) ? farm_location.paintedHouseTexture : Farm.houseTextures, farm_location.houseSource.Value, farm_location.housePaintColor.Value);
					break;
				}
				case "minedifficulty":
				case "md":
					if (debugSplit.Length > 1)
					{
						netWorldState.Value.MinesDifficulty = Convert.ToInt32(debugSplit[1]);
					}
					debugOutput = "Mine difficulty: " + netWorldState.Value.MinesDifficulty;
					break;
				case "skullcavedifficulty":
				case "scd":
					netWorldState.Value.SkullCavesDifficulty = Convert.ToInt32(debugSplit[1]);
					debugOutput = "Skull Cave difficulty: " + netWorldState.Value.SkullCavesDifficulty;
					break;
				case "tls":
					useUnscaledLighting = !useUnscaledLighting;
					debugOutput = "Toggled Lighting Scale: useUnscaledLighting: " + useUnscaledLighting;
					break;
				case "fixweapons":
					applySaveFix(SaveGame.SaveFixes.ResetForges);
					debugOutput = "Reset forged weapon attributes.";
					break;
				case "pgb":
					debugOutput = "Gem birds: North " + IslandGemBird.GetBirdTypeForLocation("IslandNorth").ToString() + " South " + IslandGemBird.GetBirdTypeForLocation("IslandSouth").ToString() + " East " + IslandGemBird.GetBirdTypeForLocation("IslandEast").ToString() + " West " + IslandGemBird.GetBirdTypeForLocation("IslandWest");
					break;
				case "ppp":
				case "printplayerpos":
					debugOutput = "Player tile position is " + player.getTileLocation().ToString() + " (World position: " + player.Position.ToString() + ")";
					break;
				case "showplurals":
				{
					List<string> list = new List<string>();
					foreach (string value15 in objectInformation.Values)
					{
						list.Add(value15.Split('/')[0]);
					}
					foreach (string value16 in bigCraftablesInformation.Values)
					{
						list.Add(value16.Split('/')[0]);
					}
					list.Sort();
					foreach (string item15 in list)
					{
						Console.WriteLine(Lexicon.makePlural(item15));
					}
					break;
				}
				case "rm":
				case "runmacro":
				{
					if (isRunningMacro)
					{
						debugOutput = "You cannot run a macro from within a macro.";
						break;
					}
					isRunningMacro = true;
					string text2 = "macro.txt";
					if (debugSplit.Length > 1)
					{
						text2 = string.Join(" ", debugSplit.Skip(1)) + ".txt";
					}
					try
					{
						StreamReader streamReader = new StreamReader(text2);
						string text3 = "";
						while ((text3 = streamReader.ReadLine()) != null)
						{
							chatBox.textBoxEnter(text3);
						}
						debugOutput = "Executed macro file " + text2;
						streamReader.Close();
					}
					catch (Exception ex)
					{
						debugOutput = "Error running macro file " + text2 + "(" + ex.Message + ")";
					}
					isRunningMacro = false;
					break;
				}
				case "invitemovie":
				{
					if (debugSplit.Length < 2)
					{
						debugOutput = "/inviteMovie (npc)";
						break;
					}
					NPC nPC = Utility.fuzzyCharacterSearch(debugSplit[1]);
					if (nPC == null)
					{
						debugOutput = "Invalid NPC";
					}
					else
					{
						MovieTheater.Invite(player, nPC);
					}
					break;
				}
				case "monster":
				{
					Type type = Type.GetType("StardewValley.Monsters." + debugSplit[1]);
					Vector2 vector = new Vector2(Convert.ToSingle(debugSplit[2]), Convert.ToSingle(debugSplit[3])) * 64f;
					object[] args;
					if (debugSplit.Length > 4)
					{
						string text = string.Join(" ", debugSplit.Skip(4));
						int result2 = -1;
						args = ((!int.TryParse(text, out result2)) ? new object[2] { vector, text } : new object[2] { vector, result2 });
					}
					else
					{
						args = new object[1] { vector };
					}
					Monster item = Activator.CreateInstance(type, args) as Monster;
					currentLocation.characters.Add(item);
					break;
				}
				case "shaft":
				case "ladder":
					if (debugSplit.Length > 1)
					{
						mine.createLadderDown(Convert.ToInt32(debugSplit[1]), Convert.ToInt32(debugSplit[2]), debugSplit[0] == "shaft");
					}
					else
					{
						mine.createLadderDown(player.getTileX(), player.getTileY() + 1, debugSplit[0] == "shaft");
					}
					break;
				case "netlog":
					multiplayer.logging.IsLogging = !multiplayer.logging.IsLogging;
					debugOutput = "Turned " + (multiplayer.logging.IsLogging ? "on" : "off") + " network write logging";
					break;
				case "netclear":
					multiplayer.logging.Clear();
					break;
				case "netdump":
					debugOutput = "Wrote log to " + multiplayer.logging.Dump();
					break;
				case "logbandwidth":
					if (IsServer)
					{
						server.LogBandwidth = !server.LogBandwidth;
						debugOutput = "Turned " + (server.LogBandwidth ? "on" : "off") + " server bandwidth logging";
					}
					else if (IsClient)
					{
						client.LogBandwidth = !client.LogBandwidth;
						debugOutput = "Turned " + (client.LogBandwidth ? "on" : "off") + " client bandwidth logging";
					}
					else
					{
						debugOutput = "Cannot toggle bandwidth logging in non-multiplayer games";
					}
					break;
				case "changewallet":
					if (IsMasterGame)
					{
						player.changeWalletTypeTonight.Value = true;
					}
					break;
				case "separatewallets":
					if (IsMasterGame)
					{
						ManorHouse.SeparateWallets();
					}
					break;
				case "mergewallets":
					if (IsMasterGame)
					{
						ManorHouse.MergeWallets();
					}
					break;
				case "nd":
				case "newday":
				case "sleep":
					player.isInBed.Value = true;
					player.sleptInTemporaryBed.Value = true;
					currentLocation.answerDialogueAction("Sleep_Yes", null);
					break;
				case "gm":
				case "inv":
				case "invincible":
					if (player.temporarilyInvincible)
					{
						player.temporaryInvincibilityTimer = 0;
						playSound("bigDeSelect");
					}
					else
					{
						player.temporarilyInvincible = true;
						player.temporaryInvincibilityTimer = -1000000000;
						playSound("bigSelect");
					}
					break;
				case "xedge":
					xEdge = Convert.ToInt32(debugSplit[1]);
					break;
				default:
					return false;
				case "break":
				case "netjoin":
					break;
					IL_bbc1:
					c = text8[0];
					if (c != 'g')
					{
						if (c == 'p' && text8 == "pam")
						{
							parseDebugInput("warp BusStop 7 12");
							parseDebugInput("wct Pam BusStop 11 10");
							break;
						}
					}
					else if (text8 == "gus")
					{
						parseDebugInput("warp Saloon 10 20");
						parseDebugInput("wct Gus Saloon 10 18");
						break;
					}
					goto IL_be24;
					IL_bb7a:
					c = text8[0];
					if ((uint)c <= 100u)
					{
						if (c != 'c')
						{
							if (c == 'd' && text8 == "dwarf")
							{
								parseDebugInput("warp Mine 43 7");
								break;
							}
						}
						else if (text8 == "clint")
						{
							parseDebugInput("warp Blacksmith 3 15");
							parseDebugInput("wct Clint Blacksmith 3 13");
							break;
						}
					}
					else if (c != 'r')
					{
						if (c != 's')
						{
							if (c == 'w' && text8 == "willy")
							{
								parseDebugInput("warp FishShop 6 6");
								parseDebugInput("wct Willy FishShop 6 4");
								break;
							}
						}
						else if (text8 == "sandy")
						{
							parseDebugInput("warp SandyHouse 2 7");
							parseDebugInput("wct Sandy SandyHouse 2 5");
							break;
						}
					}
					else if (text8 == "robin")
					{
						parseDebugInput("warp ScienceHouse 8 20");
						parseDebugInput("wct Robin ScienceHouse 8 18");
						break;
					}
					goto IL_be24;
					IL_be24:
					debugOutput = "That npc doesn't have a shop or it isn't handled by this command";
					break;
				}
				return true;
			}
			catch (Exception ex8)
			{
				Console.WriteLine("Debug command error: " + ex8);
				debugOutput = ex8.Message;
				return false;
			}
		}

		public void RecountWalnuts()
		{
			if (!IsMasterGame || !(getLocationFromName("IslandHut") is IslandHut islandHut))
			{
				return;
			}
			int num = 130;
			int num2 = islandHut.ShowNutHint();
			int num3 = num - num2;
			netWorldState.Value.GoldenWalnutsFound.Value = num3;
			foreach (GameLocation location in locations)
			{
				if (!(location is IslandLocation))
				{
					continue;
				}
				IslandLocation islandLocation = location as IslandLocation;
				foreach (ParrotUpgradePerch parrotUpgradePerch in islandLocation.parrotUpgradePerches)
				{
					if (parrotUpgradePerch.currentState.Value == ParrotUpgradePerch.UpgradeState.Complete)
					{
						num3 -= (int)parrotUpgradePerch.requiredNuts;
					}
				}
			}
			if (MasterPlayer.hasOrWillReceiveMail("Island_VolcanoShortcutOut"))
			{
				num3 -= 5;
			}
			if (MasterPlayer.hasOrWillReceiveMail("Island_VolcanoBridge"))
			{
				num3 -= 5;
			}
			netWorldState.Value.GoldenWalnuts.Value = num3;
		}

		public void ResetIslandLocations()
		{
			netWorldState.Value.GoldenWalnutsFound.Value = 0;
			string[] array = new string[14]
			{
				"birdieQuestBegun", "birdieQuestFinished", "tigerSlimeNut", "Island_W_BuriedTreasureNut", "Island_W_BuriedTreasure", "islandNorthCaveOpened", "Saw_Flame_Sprite_North_North", "Saw_Flame_Sprite_North_South", "Island_N_BuriedTreasureNut", "Island_W_BuriedTreasure",
				"Saw_Flame_Sprite_South", "Visited_Island", "Island_FirstParrot", "gotBirdieReward"
			};
			player.team.collectedNutTracker.Clear();
			for (int i = 0; i < player.mailReceived.Count; i++)
			{
				if (player.mailReceived[i].StartsWith("Island_Upgrade"))
				{
					player.mailReceived.RemoveAt(i);
					i--;
					continue;
				}
				for (int j = 0; j < array.Length; j++)
				{
					if (player.mailReceived[i].Contains(array[j]))
					{
						player.mailReceived.RemoveAt(i);
						i--;
						break;
					}
				}
			}
			for (int k = 0; k < player.mailForTomorrow.Count; k++)
			{
				if (player.mailForTomorrow[k].StartsWith("Island_Upgrade"))
				{
					player.mailForTomorrow.RemoveAt(k);
					k--;
				}
				for (int l = 0; l < array.Length; l++)
				{
					if (player.mailForTomorrow[k].Contains(array[l]))
					{
						player.mailForTomorrow.RemoveAt(k);
						k--;
						break;
					}
				}
			}
			for (int m = 0; m < player.team.broadcastedMail.Count; m++)
			{
				if (player.team.broadcastedMail[m].StartsWith("Island_Upgrade"))
				{
					player.team.broadcastedMail.RemoveAt(m);
					m--;
				}
				for (int n = 0; n < array.Length; n++)
				{
					if (player.team.broadcastedMail[m].Contains(array[n]))
					{
						player.team.broadcastedMail.RemoveAt(m);
						m--;
						break;
					}
				}
			}
			for (int num = 0; num < player.secretNotesSeen.Count; num++)
			{
				if (player.secretNotesSeen[num] >= 1000)
				{
					player.secretNotesSeen.RemoveAt(num);
					num--;
				}
			}
			player.team.limitedNutDrops.Clear();
			netWorldState.Value.GoldenCoconutCracked.Value = false;
			netWorldState.Value.GoldenWalnuts.Set(0);
			netWorldState.Value.ParrotPlatformsUnlocked.Value = false;
			netWorldState.Value.FoundBuriedNuts.Clear();
			for (int num2 = 0; num2 < locations.Count; num2++)
			{
				GameLocation gameLocation = locations[num2];
				if (gameLocation.GetLocationContext() == GameLocation.LocationContext.Island)
				{
					_locationLookup.Clear();
					string value = gameLocation.mapPath.Value;
					string value2 = gameLocation.name.Value;
					object[] args = new object[2] { value, value2 };
					GameLocation gameLocation2 = null;
					try
					{
						gameLocation2 = Activator.CreateInstance(gameLocation.GetType(), args) as GameLocation;
					}
					catch (Exception)
					{
						gameLocation2 = Activator.CreateInstance(gameLocation.GetType()) as GameLocation;
					}
					locations[num2] = gameLocation2;
					_locationLookup.Clear();
				}
			}
			addBirdieIfNecessary();
		}

		public void ShowTelephoneMenu()
		{
			playSound("openBox");
			List<Response> list = new List<Response>();
			list.Add(new Response("Carpenter", getCharacterFromName("Robin").displayName));
			list.Add(new Response("Blacksmith", getCharacterFromName("Clint").displayName));
			list.Add(new Response("SeedShop", getCharacterFromName("Pierre").displayName));
			list.Add(new Response("AnimalShop", getCharacterFromName("Marnie").displayName));
			list.Add(new Response("Saloon", getCharacterFromName("Gus").displayName));
			if (player.mailReceived.Contains("Gil_Telephone"))
			{
				list.Add(new Response("AdventureGuild", getCharacterFromName("Marlon").displayName));
			}
			list.Add(new Response("HangUp", content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")));
			currentLocation.createQuestionDialogue(content.LoadString("Strings\\Characters:Phone_SelectNumber"), list.ToArray(), "telephone");
		}

		public void requestDebugInput()
		{
			chatBox.activate();
			chatBox.setText("/");
		}

		public static WeatherDebris GetWeatherDebris(Vector2 position, int which, float rotationVelocity, float dx, float dy)
		{
			WeatherDebris weatherDebris;
			if (debrisWeatherPool.Count == 0)
			{
				weatherDebris = new WeatherDebris();
			}
			else
			{
				weatherDebris = debrisWeatherPool[debrisWeatherPool.Count - 1];
				debrisWeatherPool.RemoveAt(debrisWeatherPool.Count - 1);
			}
			weatherDebris.Initialize(position, which, rotationVelocity, dx, dy);
			return weatherDebris;
		}

		public static RainDrop GetRainDrop(int x, int y, int frame, int accumulator)
		{
			RainDrop rainDrop;
			if (rainDropPool.Count == 0)
			{
				rainDrop = new RainDrop();
			}
			else
			{
				rainDrop = rainDropPool[rainDropPool.Count - 1];
				rainDropPool.RemoveAt(rainDropPool.Count - 1);
			}
			rainDrop.Initialize(x, y, frame, accumulator);
			return rainDrop;
		}

		private void panModeSuccess(KeyboardState currentKBState)
		{
			panFacingDirectionWait = false;
			playSound("smallSelect");
			if (currentKBState.IsKeyDown(Keys.LeftShift))
			{
				panModeString += " (animation_name_here)";
			}
			debugOutput = panModeString;
		}

		private void updatePanModeControls(MouseState currentMouseState, KeyboardState currentKBState)
		{
			if (currentKBState.IsKeyDown(Keys.F8) && !oldKBState.IsKeyDown(Keys.F8))
			{
				requestDebugInput();
				return;
			}
			if (!panFacingDirectionWait)
			{
				if (currentKBState.IsKeyDown(Keys.W))
				{
					viewport.Y -= 16;
				}
				if (currentKBState.IsKeyDown(Keys.A))
				{
					viewport.X -= 16;
				}
				if (currentKBState.IsKeyDown(Keys.S))
				{
					viewport.Y += 16;
				}
				if (currentKBState.IsKeyDown(Keys.D))
				{
					viewport.X += 16;
				}
			}
			else
			{
				if (currentKBState.IsKeyDown(Keys.W))
				{
					panModeString += "0";
					panModeSuccess(currentKBState);
				}
				if (currentKBState.IsKeyDown(Keys.A))
				{
					panModeString += "3";
					panModeSuccess(currentKBState);
				}
				if (currentKBState.IsKeyDown(Keys.S))
				{
					panModeString += "2";
					panModeSuccess(currentKBState);
				}
				if (currentKBState.IsKeyDown(Keys.D))
				{
					panModeString += "1";
					panModeSuccess(currentKBState);
				}
			}
			if (getMouseX(ui_scale: false) < 192)
			{
				viewport.X -= 8;
				viewport.X -= (192 - getMouseX()) / 8;
			}
			if (getMouseX(ui_scale: false) > viewport.Width - 192)
			{
				viewport.X += 8;
				viewport.X += (getMouseX() - viewport.Width + 192) / 8;
			}
			if (getMouseY(ui_scale: false) < 192)
			{
				viewport.Y -= 8;
				viewport.Y -= (192 - getMouseY()) / 8;
			}
			if (getMouseY(ui_scale: false) > viewport.Height - 192)
			{
				viewport.Y += 8;
				viewport.Y += (getMouseY() - viewport.Height + 192) / 8;
			}
			if (currentMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released && panModeString != null && panModeString.Length > 0)
			{
				int num = (getMouseX() + viewport.X) / 64;
				int num2 = (getMouseY() + viewport.Y) / 64;
				panModeString = panModeString + currentLocation.Name + " " + num + " " + num2 + " ";
				panFacingDirectionWait = true;
				currentLocation.playTerrainSound(new Vector2(num, num2));
				debugOutput = panModeString;
			}
			if (currentMouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released)
			{
				int x = getMouseX() + viewport.X;
				int y = getMouseY() + viewport.Y;
				Warp warp = currentLocation.isCollidingWithWarpOrDoor(new Microsoft.Xna.Framework.Rectangle(x, y, 1, 1));
				if (warp != null)
				{
					currentLocation = getLocationFromName(warp.TargetName);
					currentLocation.map.LoadTileSheets(mapDisplayDevice);
					viewport.X = warp.TargetX * 64 - viewport.Width / 2;
					viewport.Y = warp.TargetY * 64 - viewport.Height / 2;
					playSound("dwop");
				}
			}
			if (currentKBState.IsKeyDown(Keys.Escape) && !oldKBState.IsKeyDown(Keys.Escape))
			{
				Warp warp2 = currentLocation.warps[0];
				currentLocation = getLocationFromName(warp2.TargetName);
				currentLocation.map.LoadTileSheets(mapDisplayDevice);
				viewport.X = warp2.TargetX * 64 - viewport.Width / 2;
				viewport.Y = warp2.TargetY * 64 - viewport.Height / 2;
				playSound("dwop");
			}
			if (viewport.X < -64)
			{
				viewport.X = -64;
			}
			if (viewport.X + viewport.Width > currentLocation.Map.Layers[0].LayerWidth * 64 + 128)
			{
				viewport.X = currentLocation.Map.Layers[0].LayerWidth * 64 + 128 - viewport.Width;
			}
			if (viewport.Y < -64)
			{
				viewport.Y = -64;
			}
			if (viewport.Y + viewport.Height > currentLocation.Map.Layers[0].LayerHeight * 64 + 128)
			{
				viewport.Y = currentLocation.Map.Layers[0].LayerHeight * 64 + 128 - viewport.Height;
			}
			oldMouseState = input.GetMouseState();
			oldKBState = currentKBState;
		}

		public static bool isLocationAccessible(string locationName)
		{
			switch (locationName)
			{
			case "CommunityCenter":
				if (player.eventsSeen.Contains(191393))
				{
					return true;
				}
				break;
			case "JojaMart":
				if (!Utility.HasAnyPlayerSeenEvent(191393))
				{
					return true;
				}
				break;
			case "Railroad":
				if (stats.DaysPlayed > 31)
				{
					return true;
				}
				break;
			default:
				return true;
			}
			return false;
		}

		public static bool isDPadPressed()
		{
			return isDPadPressed(input.GetGamePadState());
		}

		public static bool isDPadPressed(GamePadState pad_state)
		{
			if (pad_state.DPad.Up == ButtonState.Pressed || pad_state.DPad.Down == ButtonState.Pressed || pad_state.DPad.Left == ButtonState.Pressed || pad_state.DPad.Right == ButtonState.Pressed)
			{
				virtualJoypad.mostRecentlyUsedControlType = ControlType.GAMEPAD;
				return true;
			}
			return false;
		}

		public static bool isGamePadThumbstickInMotion(double threshold = 0.2)
		{
			bool flag = false;
			GamePadState gamePadState = input.GetGamePadState();
			if ((double)gamePadState.ThumbSticks.Left.X < 0.0 - threshold || gamePadState.IsButtonDown(Buttons.LeftThumbstickLeft))
			{
				flag = true;
			}
			if ((double)gamePadState.ThumbSticks.Left.X > threshold || gamePadState.IsButtonDown(Buttons.LeftThumbstickRight))
			{
				flag = true;
			}
			if ((double)gamePadState.ThumbSticks.Left.Y < 0.0 - threshold || gamePadState.IsButtonDown(Buttons.LeftThumbstickUp))
			{
				flag = true;
			}
			if ((double)gamePadState.ThumbSticks.Left.Y > threshold || gamePadState.IsButtonDown(Buttons.LeftThumbstickDown))
			{
				flag = true;
			}
			if ((double)gamePadState.ThumbSticks.Right.X < 0.0 - threshold)
			{
				flag = true;
			}
			if ((double)gamePadState.ThumbSticks.Right.X > threshold)
			{
				flag = true;
			}
			if ((double)gamePadState.ThumbSticks.Right.Y < 0.0 - threshold)
			{
				flag = true;
			}
			if ((double)gamePadState.ThumbSticks.Right.Y > threshold)
			{
				flag = true;
			}
			if (flag)
			{
				thumbstickMotionMargin = 50;
				virtualJoypad.mostRecentlyUsedControlType = ControlType.GAMEPAD;
			}
			return thumbstickMotionMargin > 0;
		}

		public static bool isAnyGamePadButtonBeingPressed()
		{
			if (virtualJoypad.ButtonAPressed || virtualJoypad.ButtonBPressed)
			{
				virtualJoypad.mostRecentlyUsedControlType = ControlType.VIRTUAL_PAD;
				return true;
			}
			GamePadState gamePadState = input.GetGamePadState();
			if (Utility.getPressedButtons(gamePadState, oldPadState).Count > 0)
			{
				virtualJoypad.mostRecentlyUsedControlType = ControlType.GAMEPAD;
				return true;
			}
			return false;
		}

		public static bool isAnyGamePadButtonBeingHeld()
		{
			if (virtualJoypad.buttonAHeld || virtualJoypad.buttonBHeld)
			{
				virtualJoypad.mostRecentlyUsedControlType = ControlType.VIRTUAL_PAD;
				return true;
			}
			GamePadState gamePadState = input.GetGamePadState();
			if (Utility.getHeldButtons(gamePadState).Count > 0)
			{
				virtualJoypad.mostRecentlyUsedControlType = ControlType.GAMEPAD;
				return true;
			}
			return false;
		}

		private static void UpdateChatBox()
		{
			if (chatBox == null)
			{
				return;
			}
			KeyboardState keyboardState = input.GetKeyboardState();
			GamePadState gamePadState = input.GetGamePadState();
			if (IsChatting)
			{
				if (textEntry != null)
				{
					return;
				}
				if (gamePadState.IsButtonDown(Buttons.A))
				{
					MouseState mouseState = input.GetMouseState();
					if (chatBox != null && chatBox.isActive() && !chatBox.isHoveringOverClickable(mouseState.X, mouseState.Y))
					{
						oldPadState = gamePadState;
						oldKBState = keyboardState;
						showTextEntry(chatBox.chatBox);
					}
				}
				if (keyboardState.IsKeyDown(Keys.Escape) || gamePadState.IsButtonDown(Buttons.B) || gamePadState.IsButtonDown(Buttons.Back))
				{
					chatBox.clickAway();
					oldKBState = keyboardState;
				}
			}
			else if (keyboardDispatcher.Subscriber == null && ((isOneOfTheseKeysDown(keyboardState, options.chatButton) && game1.HasKeyboardFocus()) || (!gamePadState.IsButtonDown(Buttons.RightStick) && rightStickHoldTime > 0 && rightStickHoldTime < emoteMenuShowTime)))
			{
				chatBox.activate();
				if (keyboardState.IsKeyDown(Keys.OemQuestion))
				{
					chatBox.setText("/");
				}
			}
		}

		public static KeyboardState GetKeyboardState()
		{
			KeyboardState keyboardState = input.GetKeyboardState();
			if (chatBox != null)
			{
				if (IsChatting)
				{
					return default(KeyboardState);
				}
				if (keyboardDispatcher.Subscriber == null && isOneOfTheseKeysDown(keyboardState, options.chatButton) && game1.HasKeyboardFocus())
				{
					return default(KeyboardState);
				}
			}
			return keyboardState;
		}

		private void UpdateControlInput(GameTime time)
		{
			KeyboardState state = GetKeyboardState();
			MouseState mouseState = input.GetMouseState();
			GamePadState state2 = input.GetGamePadState();
			if (options.gamepadControls)
			{
				bool flag = false;
				if (Math.Abs(state2.ThumbSticks.Right.X) > 0f || Math.Abs(state2.ThumbSticks.Right.Y) > 0f)
				{
					setMousePositionRaw((int)((float)mouseState.X + state2.ThumbSticks.Right.X * thumbstickToMouseModifier), (int)((float)mouseState.Y - state2.ThumbSticks.Right.Y * thumbstickToMouseModifier));
					flag = true;
				}
				if (IsChatting)
				{
					flag = true;
				}
				if (((getMouseX() != getOldMouseX() || getMouseY() != getOldMouseY()) && getMouseX() != 0 && getMouseY() != 0) || flag)
				{
					if (flag)
					{
						if (timerUntilMouseFade <= 0)
						{
							lastMousePositionBeforeFade = new Point(localMultiplayerWindow.Width / 2, localMultiplayerWindow.Height / 2);
						}
					}
					else
					{
						lastCursorMotionWasMouse = true;
					}
					if (timerUntilMouseFade <= 0 && !lastCursorMotionWasMouse)
					{
						setMousePositionRaw(lastMousePositionBeforeFade.X, lastMousePositionBeforeFade.Y);
					}
					timerUntilMouseFade = 4000;
				}
			}
			else if (getMouseX() != getOldMouseX() || getMouseY() != getOldMouseY())
			{
				lastCursorMotionWasMouse = true;
			}
			bool actionButtonPressed = false;
			bool switchToolButtonPressed = false;
			bool useToolButtonPressed = false;
			bool useToolButtonReleased = false;
			bool addItemToInventoryButtonPressed = false;
			bool cancelButtonPressed = false;
			bool moveupPressed = false;
			bool moverightPressed = false;
			bool moveleftPressed = false;
			bool movedownPressed = false;
			bool moveupReleased = false;
			bool moverightReleased = false;
			bool moveDownReleased = false;
			bool moveleftReleased = false;
			bool moveupHeld = false;
			bool moverightHeld = false;
			bool movedownHeld = false;
			bool moveleftHeld = false;
			bool useToolHeld = false;
			if ((isOneOfTheseKeysDown(state, options.actionButton) && areAllOfTheseKeysUp(oldKBState, options.actionButton)) || (mouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released))
			{
				actionButtonPressed = true;
				rightClickPolling = 250;
			}
			if (isOneOfTheseKeysDown(state, options.useToolButton) && areAllOfTheseKeysUp(oldKBState, options.useToolButton))
			{
				useToolButtonPressed = true;
			}
			if (areAllOfTheseKeysUp(state, options.useToolButton) && isOneOfTheseKeysDown(oldKBState, options.useToolButton))
			{
				useToolButtonReleased = true;
			}
			if (mouseState.ScrollWheelValue != oldMouseState.ScrollWheelValue)
			{
				switchToolButtonPressed = true;
			}
			if ((isOneOfTheseKeysDown(state, options.cancelButton) && areAllOfTheseKeysUp(oldKBState, options.cancelButton)) || (mouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released))
			{
				cancelButtonPressed = true;
			}
			if (isOneOfTheseKeysDown(state, options.moveUpButton) && areAllOfTheseKeysUp(oldKBState, options.moveUpButton))
			{
				moveupPressed = true;
			}
			if (isOneOfTheseKeysDown(state, options.moveRightButton) && areAllOfTheseKeysUp(oldKBState, options.moveRightButton))
			{
				moverightPressed = true;
			}
			if (isOneOfTheseKeysDown(state, options.moveDownButton) && areAllOfTheseKeysUp(oldKBState, options.moveDownButton))
			{
				movedownPressed = true;
			}
			if (isOneOfTheseKeysDown(state, options.moveLeftButton) && areAllOfTheseKeysUp(oldKBState, options.moveLeftButton))
			{
				moveleftPressed = true;
			}
			if (areAllOfTheseKeysUp(state, options.moveUpButton) && isOneOfTheseKeysDown(oldKBState, options.moveUpButton))
			{
				moveupReleased = true;
			}
			if (areAllOfTheseKeysUp(state, options.moveRightButton) && isOneOfTheseKeysDown(oldKBState, options.moveRightButton))
			{
				moverightReleased = true;
			}
			if (areAllOfTheseKeysUp(state, options.moveDownButton) && isOneOfTheseKeysDown(oldKBState, options.moveDownButton))
			{
				moveDownReleased = true;
			}
			if (areAllOfTheseKeysUp(state, options.moveLeftButton) && isOneOfTheseKeysDown(oldKBState, options.moveLeftButton))
			{
				moveleftReleased = true;
			}
			if (isOneOfTheseKeysDown(state, options.moveUpButton))
			{
				moveupHeld = true;
			}
			if (isOneOfTheseKeysDown(state, options.moveRightButton))
			{
				moverightHeld = true;
			}
			if (isOneOfTheseKeysDown(state, options.moveDownButton))
			{
				movedownHeld = true;
			}
			if (isOneOfTheseKeysDown(state, options.moveLeftButton))
			{
				moveleftHeld = true;
			}
			if (isOneOfTheseKeysDown(state, options.useToolButton) || mouseState.LeftButton == ButtonState.Pressed)
			{
				useToolHeld = true;
			}
			if (isOneOfTheseKeysDown(state, options.actionButton) || mouseState.RightButton == ButtonState.Pressed)
			{
				rightClickPolling -= time.ElapsedGameTime.Milliseconds;
				if (rightClickPolling <= 0)
				{
					rightClickPolling = 100;
					actionButtonPressed = true;
				}
			}
			if (options.gamepadControls)
			{
				if (state.GetPressedKeys().Length != 0 || mouseState.LeftButton == ButtonState.Pressed || mouseState.RightButton == ButtonState.Pressed)
				{
					timerUntilMouseFade = 4000;
				}
				if (state2.IsButtonDown(Buttons.A) && !oldPadState.IsButtonDown(Buttons.A))
				{
					actionButtonPressed = true;
					lastCursorMotionWasMouse = false;
					rightClickPolling = 250;
				}
				if (state2.IsButtonDown(Buttons.X) && !oldPadState.IsButtonDown(Buttons.X))
				{
					useToolButtonPressed = true;
					lastCursorMotionWasMouse = false;
				}
				if (!state2.IsButtonDown(Buttons.X) && oldPadState.IsButtonDown(Buttons.X))
				{
					useToolButtonReleased = true;
				}
				if (state2.IsButtonDown(Buttons.RightTrigger) && !oldPadState.IsButtonDown(Buttons.RightTrigger))
				{
					switchToolButtonPressed = true;
					triggerPolling = 300;
				}
				else if (state2.IsButtonDown(Buttons.LeftTrigger) && !oldPadState.IsButtonDown(Buttons.LeftTrigger))
				{
					switchToolButtonPressed = true;
					triggerPolling = 300;
				}
				if (state2.IsButtonDown(Buttons.X))
				{
					useToolHeld = true;
				}
				if (state2.IsButtonDown(Buttons.A))
				{
					rightClickPolling -= time.ElapsedGameTime.Milliseconds;
					if (rightClickPolling <= 0)
					{
						rightClickPolling = 100;
						actionButtonPressed = true;
					}
				}
				if (state2.IsButtonDown(Buttons.RightTrigger) || state2.IsButtonDown(Buttons.LeftTrigger))
				{
					triggerPolling -= time.ElapsedGameTime.Milliseconds;
					if (triggerPolling <= 0)
					{
						triggerPolling = 100;
						switchToolButtonPressed = true;
					}
				}
				if (state2.IsButtonDown(Buttons.RightShoulder) && !oldPadState.IsButtonDown(Buttons.RightShoulder))
				{
					player.shiftToolbar(right: true);
				}
				if (state2.IsButtonDown(Buttons.LeftShoulder) && !oldPadState.IsButtonDown(Buttons.LeftShoulder))
				{
					player.shiftToolbar(right: false);
				}
				if (state2.IsButtonDown(Buttons.DPadUp) && !oldPadState.IsButtonDown(Buttons.DPadUp))
				{
					moveupPressed = true;
				}
				else if (!state2.IsButtonDown(Buttons.DPadUp) && oldPadState.IsButtonDown(Buttons.DPadUp))
				{
					moveupReleased = true;
				}
				if (state2.IsButtonDown(Buttons.DPadRight) && !oldPadState.IsButtonDown(Buttons.DPadRight))
				{
					moverightPressed = true;
				}
				else if (!state2.IsButtonDown(Buttons.DPadRight) && oldPadState.IsButtonDown(Buttons.DPadRight))
				{
					moverightReleased = true;
				}
				if (state2.IsButtonDown(Buttons.DPadDown) && !oldPadState.IsButtonDown(Buttons.DPadDown))
				{
					movedownPressed = true;
				}
				else if (!state2.IsButtonDown(Buttons.DPadDown) && oldPadState.IsButtonDown(Buttons.DPadDown))
				{
					moveDownReleased = true;
				}
				if (state2.IsButtonDown(Buttons.DPadLeft) && !oldPadState.IsButtonDown(Buttons.DPadLeft))
				{
					moveleftPressed = true;
				}
				else if (!state2.IsButtonDown(Buttons.DPadLeft) && oldPadState.IsButtonDown(Buttons.DPadLeft))
				{
					moveleftReleased = true;
				}
				if (state2.IsButtonDown(Buttons.DPadUp))
				{
					moveupHeld = true;
				}
				if (state2.IsButtonDown(Buttons.DPadRight))
				{
					moverightHeld = true;
				}
				if (state2.IsButtonDown(Buttons.DPadDown))
				{
					movedownHeld = true;
				}
				if (state2.IsButtonDown(Buttons.DPadLeft))
				{
					moveleftHeld = true;
				}
				if ((double)state2.ThumbSticks.Left.X < -0.2)
				{
					moveleftPressed = true;
					moveleftHeld = true;
				}
				else if ((double)state2.ThumbSticks.Left.X > 0.2)
				{
					moverightPressed = true;
					moverightHeld = true;
				}
				if ((double)state2.ThumbSticks.Left.Y < -0.2)
				{
					movedownPressed = true;
					movedownHeld = true;
				}
				else if ((double)state2.ThumbSticks.Left.Y > 0.2)
				{
					moveupPressed = true;
					moveupHeld = true;
				}
				if ((double)oldPadState.ThumbSticks.Left.X < -0.2 && !moveleftHeld)
				{
					moveleftReleased = true;
				}
				if ((double)oldPadState.ThumbSticks.Left.X > 0.2 && !moverightHeld)
				{
					moverightReleased = true;
				}
				if ((double)oldPadState.ThumbSticks.Left.Y < -0.2 && !movedownHeld)
				{
					moveDownReleased = true;
				}
				if ((double)oldPadState.ThumbSticks.Left.Y > 0.2 && !moveupHeld)
				{
					moveupReleased = true;
				}
				if (controllerSlingshotSafeTime > 0f)
				{
					if (!state2.IsButtonDown(Buttons.DPadUp) && !state2.IsButtonDown(Buttons.DPadDown) && !state2.IsButtonDown(Buttons.DPadLeft) && !state2.IsButtonDown(Buttons.DPadRight) && (double)Math.Abs(state2.ThumbSticks.Left.X) < 0.04 && (double)Math.Abs(state2.ThumbSticks.Left.Y) < 0.04)
					{
						controllerSlingshotSafeTime = 0f;
					}
					if (controllerSlingshotSafeTime <= 0f)
					{
						controllerSlingshotSafeTime = 0f;
					}
					else
					{
						controllerSlingshotSafeTime -= (float)time.ElapsedGameTime.TotalSeconds;
						moveupPressed = false;
						movedownPressed = false;
						moveleftPressed = false;
						moverightPressed = false;
						moveupHeld = false;
						movedownHeld = false;
						moveleftHeld = false;
						moverightHeld = false;
					}
				}
			}
			else
			{
				controllerSlingshotSafeTime = 0f;
			}
			ResetFreeCursorDrag();
			_mobileUpdateControlInput(mouseState, out var tapState, ref moveupPressed, ref movedownPressed, ref moveleftPressed, ref moverightPressed, ref moveupReleased, ref moveupReleased, ref moveleftReleased, ref moverightReleased, ref moveupHeld, ref movedownHeld, ref moveleftHeld, ref moverightHeld, ref actionButtonPressed, ref useToolButtonPressed, ref useToolButtonReleased, ref useToolHeld);
			if (useToolHeld && !(player.ActiveObject is Furniture))
			{
				mouseClickPolling += time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				mouseClickPolling = 0;
			}
			if (isOneOfTheseKeysDown(state, options.toolbarSwap) && areAllOfTheseKeysUp(oldKBState, options.toolbarSwap))
			{
				player.shiftToolbar((!state.IsKeyDown(Keys.LeftControl)) ? true : false);
			}
			bool flag2 = false;
			if (mouseClickPolling > 250 && player.mount == null && currentLocation is DecoratableLocation && (player.CurrentTool == null || !(player.CurrentTool is FishingRod) || (int)player.CurrentTool.upgradeLevel <= 0) && !TapToMove.isTapToMoveWeaponControl() && virtualJoypad.buttonAHeld)
			{
				flag2 = true;
			}
			if (virtualJoypad.buttonAHeld && mouseClickPolling > 250 && player.mount == null && player.CurrentTool != null && !(player.CurrentTool is FishingRod) && !player.usingTool)
			{
				flag2 = true;
			}
			if (options.weaponControl == 3)
			{
				flag2 = false;
			}
			if (state2.IsButtonDown(Buttons.X) && oldPadState.IsButtonDown(Buttons.X) && !(player.CurrentTool is MeleeWeapon))
			{
				flag2 = true;
			}
			if (flag2)
			{
				useToolButtonPressed = true;
				mouseClickPolling = 100;
			}
			bool flag3 = mouseState.LeftButton == ButtonState.Pressed && wasMouseVisibleThisFrame;
			PushUIMode();
			if (flag3)
			{
				foreach (IClickableMenu onScreenMenu in onScreenMenus)
				{
					if (displayHUD || onScreenMenu == chatBox)
					{
						if (onScreenMenu is Toolbar)
						{
							onScreenMenu.performHoverAction(getMouseX(), getMouseY());
						}
						else if (onScreenMenu.isWithinBounds(getMouseX(), getMouseY()))
						{
							onScreenMenu.performHoverAction(getMouseX(), getMouseY());
						}
					}
				}
			}
			PopUIMode();
			if (chatBox != null && chatBox.chatBox.Selected && oldMouseState.ScrollWheelValue != mouseState.ScrollWheelValue)
			{
				chatBox.receiveScrollWheelAction(mouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue);
			}
			if (panMode)
			{
				updatePanModeControls(mouseState, state);
				return;
			}
			if (inputSimulator != null)
			{
				if (state.IsKeyDown(Keys.Escape))
				{
					inputSimulator = null;
				}
				else
				{
					inputSimulator.SimulateInput(ref actionButtonPressed, ref switchToolButtonPressed, ref useToolButtonPressed, ref useToolButtonReleased, ref addItemToInventoryButtonPressed, ref cancelButtonPressed, ref moveupPressed, ref moverightPressed, ref moveleftPressed, ref movedownPressed, ref moveupReleased, ref moverightReleased, ref moveleftReleased, ref moveDownReleased, ref moveupHeld, ref moverightHeld, ref moveleftHeld, ref movedownHeld);
				}
			}
			if (useToolButtonReleased && player.CurrentTool != null && CurrentEvent == null && pauseTime <= 0f && player.CurrentTool.onRelease(currentLocation, getMouseX(), getMouseY(), player))
			{
				oldMouseState = input.GetMouseState();
				oldKBState = state;
				oldPadState = state2;
				player.usingSlingshot = false;
				player.canReleaseTool = true;
				player.UsingTool = false;
				player.CanMove = true;
				return;
			}
			if (CurrentEvent != null && mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
			{
				CurrentEvent.receiveMouseClick(getMouseX(), getMouseY());
			}
			if (TutorialManager.Instance.showAttackDialog && TutorialManager.Instance.attackDialog != null && (!eventUp & !dialogueUp))
			{
				PushUIMode();
				if (mouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
				{
					TutorialManager.Instance.releaseLeftClick(getMouseX(), getMouseY());
				}
				else if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Pressed)
				{
					TutorialManager.Instance.leftClickHeld(getMouseX(), getMouseY());
				}
				if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
				{
					TutorialManager.Instance.receiveLeftClick(getMouseX(), getMouseY());
				}
				oldMouseState = input.GetMouseState();
				PopUIMode();
				return;
			}
			if (mouseState.LeftButton == ButtonState.Pressed)
			{
				PushUIMode();
				if (CurrentEvent == null && activeClickableMenu == null && oldMouseState.LeftButton == ButtonState.Released)
				{
					dayTimeMoneyBox.testToOpenDebugConsole(getMouseX(), getMouseY());
				}
				if (oldMouseState.LeftButton == ButtonState.Released)
				{
					virtualJoypad.receiveLeftClick(getMouseX(), getMouseY());
				}
				foreach (IClickableMenu onScreenMenu2 in onScreenMenus)
				{
					if (oldMouseState.LeftButton == ButtonState.Released)
					{
						onScreenMenu2.receiveLeftClick(getMouseX(), getMouseY());
					}
					else
					{
						onScreenMenu2.leftClickHeld(getMouseX(), getMouseY());
					}
				}
				PopUIMode();
			}
			if (TutorialManager.Instance.showAttackDialog && TutorialManager.Instance.attackDialog != null && (!eventUp & !dialogueUp))
			{
				if (mouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
				{
					TutorialManager.Instance.releaseLeftClick(getMouseX(), getMouseY());
				}
				else if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Pressed)
				{
					TutorialManager.Instance.leftClickHeld(getMouseX(), getMouseY());
				}
				if (mouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
				{
					TutorialManager.Instance.receiveLeftClick(getMouseX(), getMouseY());
				}
				oldMouseState = input.GetMouseState();
				return;
			}
			_mobileProcessTaps(tapState);
			if (mouseState.LeftButton == ButtonState.Released)
			{
				PushUIMode();
				if (oldMouseState.LeftButton == ButtonState.Pressed)
				{
					virtualJoypad.releaseLeftClick(getMouseX(), getMouseY());
				}
				foreach (IClickableMenu onScreenMenu3 in onScreenMenus)
				{
					if (mouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
					{
						onScreenMenu3.releaseLeftClick(getMouseX(), getMouseY());
					}
				}
				PopUIMode();
			}
			if (((useToolButtonPressed && !isAnyGamePadButtonBeingPressed()) || (actionButtonPressed && isAnyGamePadButtonBeingPressed())) && pauseTime <= 0f && wasMouseVisibleThisFrame)
			{
				PushUIMode();
				foreach (IClickableMenu onScreenMenu4 in onScreenMenus)
				{
					if (displayHUD || onScreenMenu4 == chatBox)
					{
						if ((!IsChatting || onScreenMenu4 == chatBox) && (!(onScreenMenu4 is LevelUpMenu) || (onScreenMenu4 as LevelUpMenu).informationUp) && onScreenMenu4.isWithinBounds(getMouseX(), getMouseY()))
						{
							onScreenMenu4.receiveLeftClick(getMouseX(), getMouseY());
							PopUIMode();
							oldMouseState = input.GetMouseState();
							oldKBState = state;
							oldPadState = state2;
							return;
						}
						if (onScreenMenu4 == chatBox && options.gamepadControls && IsChatting)
						{
							oldMouseState = input.GetMouseState();
							oldKBState = state;
							oldPadState = state2;
							PopUIMode();
							return;
						}
						onScreenMenu4.clickAway();
					}
				}
				PopUIMode();
			}
			if (IsChatting || player.freezePause > 0)
			{
				if (IsChatting)
				{
					ButtonCollection.ButtonEnumerator enumerator5 = Utility.getPressedButtons(state2, oldPadState).GetEnumerator();
					while (enumerator5.MoveNext())
					{
						Buttons current5 = enumerator5.Current;
						chatBox.receiveGamePadButton(current5);
					}
				}
				oldMouseState = input.GetMouseState();
				oldKBState = state;
				oldPadState = state2;
				return;
			}
			if (paused || HostPaused)
			{
				if (!HostPaused || !IsMasterGame || (!isOneOfTheseKeysDown(state, options.menuButton) && !state2.IsButtonDown(Buttons.B) && !state2.IsButtonDown(Buttons.Back)))
				{
					oldMouseState = input.GetMouseState();
					return;
				}
				netWorldState.Value.IsPaused = false;
				if (chatBox != null)
				{
					chatBox.globalInfoMessage("Resumed");
				}
			}
			if (eventUp)
			{
				if (currentLocation.currentEvent == null && locationRequest == null)
				{
					eventUp = false;
				}
				else if (actionButtonPressed || useToolButtonPressed)
				{
					CurrentEvent?.receiveMouseClick(getMouseX(), getMouseY());
				}
			}
			bool flag4 = eventUp || farmEvent != null;
			if (actionButtonPressed || (dialogueUp && useToolButtonPressed))
			{
				PushUIMode();
				foreach (IClickableMenu onScreenMenu5 in onScreenMenus)
				{
					if (wasMouseVisibleThisFrame && (displayHUD || onScreenMenu5 == chatBox) && onScreenMenu5.isWithinBounds(getMouseX(), getMouseY()) && (!(onScreenMenu5 is LevelUpMenu) || (onScreenMenu5 as LevelUpMenu).informationUp))
					{
						onScreenMenu5.receiveRightClick(getMouseX(), getMouseY());
						oldMouseState = input.GetMouseState();
						if (!isAnyGamePadButtonBeingPressed())
						{
							PopUIMode();
							oldKBState = state;
							oldPadState = state2;
							return;
						}
					}
				}
				PopUIMode();
				if (!pressActionButton(state, mouseState, state2))
				{
					oldKBState = state;
					oldMouseState = input.GetMouseState();
					oldPadState = state2;
					return;
				}
			}
			if (useToolButtonPressed && (!player.UsingTool || player.CurrentTool is MeleeWeapon) && !player.isEating && !pickingTool && !dialogueUp && !menuUp && farmEvent == null && (player.CanMove || (player.CurrentTool != null && (player.CurrentTool.Name.Equals("Fishing Rod") || player.CurrentTool is MeleeWeapon))))
			{
				if (player.CurrentTool != null)
				{
					player.FireTool();
				}
				if (!pressUseToolButton() && player.canReleaseTool && player.UsingTool)
				{
					_ = player.CurrentTool;
				}
				if (player.UsingTool)
				{
					oldMouseState = input.GetMouseState();
					oldKBState = state;
					oldPadState = state2;
					return;
				}
			}
			if (useToolButtonReleased && _didInitiateItemStow)
			{
				_didInitiateItemStow = false;
			}
			if (useToolButtonReleased && player.canReleaseTool && player.UsingTool && player.CurrentTool != null)
			{
				player.EndUsingTool();
			}
			if (switchToolButtonPressed && !player.UsingTool && !dialogueUp && (pickingTool || player.CanMove) && !player.areAllItemsNull() && !flag4)
			{
				pressSwitchToolButton();
			}
			if (cancelButtonPressed)
			{
				if (numberOfSelectedItems != -1)
				{
					numberOfSelectedItems = -1;
					dialogueUp = false;
					player.CanMove = true;
				}
				else if (nameSelectUp && NameSelect.cancel())
				{
					nameSelectUp = false;
					playSound("bigDeSelect");
				}
			}
			if (player.CurrentTool != null && useToolHeld && player.canReleaseTool && !flag4 && !dialogueUp && !menuUp && player.Stamina >= 1f && !(player.CurrentTool is FishingRod))
			{
				int num = (player.CurrentTool.hasEnchantmentOfType<ReachingToolEnchantment>() ? 1 : 0);
				if (player.toolHold <= 0 && (int)player.CurrentTool.upgradeLevel + num > player.toolPower)
				{
					float num2 = 1f;
					if (player.CurrentTool != null)
					{
						num2 = player.CurrentTool.AnimationSpeedModifier;
					}
					player.toolHold = (int)(600f * num2);
				}
				else if ((int)player.CurrentTool.upgradeLevel + num > player.toolPower)
				{
					player.toolHold -= time.ElapsedGameTime.Milliseconds;
					if (player.toolHold <= 0)
					{
						player.toolPowerIncrease();
					}
				}
			}
			if (upPolling >= 650f)
			{
				moveupPressed = true;
				upPolling -= 100f;
			}
			else if (downPolling >= 650f)
			{
				movedownPressed = true;
				downPolling -= 100f;
			}
			else if (rightPolling >= 650f)
			{
				moverightPressed = true;
				rightPolling -= 100f;
			}
			else if (leftPolling >= 650f)
			{
				moveleftPressed = true;
				leftPolling -= 100f;
			}
			else if (!nameSelectUp && pauseTime <= 0f && locationRequest == null && !player.UsingTool && (!flag4 || (CurrentEvent != null && CurrentEvent.playerControlSequence)))
			{
				if (player.movementDirections.Count < 2)
				{
					int count = player.movementDirections.Count;
					if (moveupHeld)
					{
						player.setMoving(1);
					}
					if (moverightHeld)
					{
						player.setMoving(2);
					}
					if (movedownHeld)
					{
						player.setMoving(4);
					}
					if (moveleftHeld)
					{
						player.setMoving(8);
					}
				}
				if (moveupReleased || (player.movementDirections.Contains(0) && !moveupHeld))
				{
					player.setMoving(33);
					if (player.movementDirections.Count == 0)
					{
						player.setMoving(64);
					}
				}
				if (moverightReleased || (player.movementDirections.Contains(1) && !moverightHeld))
				{
					player.setMoving(34);
					if (player.movementDirections.Count == 0)
					{
						player.setMoving(64);
					}
				}
				if (moveDownReleased || (player.movementDirections.Contains(2) && !movedownHeld))
				{
					player.setMoving(36);
					if (player.movementDirections.Count == 0)
					{
						player.setMoving(64);
					}
				}
				if (moveleftReleased || (player.movementDirections.Contains(3) && !moveleftHeld))
				{
					player.setMoving(40);
					if (player.movementDirections.Count == 0)
					{
						player.setMoving(64);
					}
				}
				if ((!moveupHeld && !moverightHeld && !movedownHeld && !moveleftHeld && !player.UsingTool) || activeClickableMenu != null)
				{
					player.Halt();
				}
			}
			else if (isQuestion)
			{
				if (moveupPressed)
				{
					currentQuestionChoice = Math.Max(currentQuestionChoice - 1, 0);
					playSound("toolSwap");
				}
				else if (movedownPressed)
				{
					currentQuestionChoice = Math.Min(currentQuestionChoice + 1, questionChoices.Count - 1);
					playSound("toolSwap");
				}
			}
			else if (numberOfSelectedItems != -1 && !dialogueTyping)
			{
				int val = 99;
				if (selectedItemsType.Equals("Animal Food"))
				{
					val = 999 - player.Feed;
				}
				else if (selectedItemsType.Equals("calicoJackBet"))
				{
					val = Math.Min(player.clubCoins, 999);
				}
				else if (selectedItemsType.Equals("flutePitch"))
				{
					val = 26;
				}
				else if (selectedItemsType.Equals("drumTone"))
				{
					val = 6;
				}
				else if (selectedItemsType.Equals("jukebox"))
				{
					val = player.songsHeard.Count - 1;
				}
				else if (selectedItemsType.Equals("Fuel"))
				{
					val = 100 - ((Lantern)player.getToolFromName("Lantern")).fuelLeft;
				}
				if (moverightPressed)
				{
					numberOfSelectedItems = Math.Min(numberOfSelectedItems + 1, val);
					playItemNumberSelectSound();
				}
				else if (moveleftPressed)
				{
					numberOfSelectedItems = Math.Max(numberOfSelectedItems - 1, 0);
					playItemNumberSelectSound();
				}
				else if (moveupPressed)
				{
					numberOfSelectedItems = Math.Min(numberOfSelectedItems + 10, val);
					playItemNumberSelectSound();
				}
				else if (movedownPressed)
				{
					numberOfSelectedItems = Math.Max(numberOfSelectedItems - 10, 0);
					playItemNumberSelectSound();
				}
			}
			if (moveupHeld && !player.CanMove)
			{
				upPolling += time.ElapsedGameTime.Milliseconds;
			}
			else if (movedownHeld && !player.CanMove)
			{
				downPolling += time.ElapsedGameTime.Milliseconds;
			}
			else if (moverightHeld && !player.CanMove)
			{
				rightPolling += time.ElapsedGameTime.Milliseconds;
			}
			else if (moveleftHeld && !player.CanMove)
			{
				leftPolling += time.ElapsedGameTime.Milliseconds;
			}
			else if (moveupReleased)
			{
				upPolling = 0f;
			}
			else if (moveDownReleased)
			{
				downPolling = 0f;
			}
			else if (moverightReleased)
			{
				rightPolling = 0f;
			}
			else if (moveleftReleased)
			{
				leftPolling = 0f;
			}
			if (debugMode)
			{
				if (state.IsKeyDown(Keys.Q))
				{
					oldKBState.IsKeyDown(Keys.Q);
				}
				if (state.IsKeyDown(Keys.P) && !oldKBState.IsKeyDown(Keys.P))
				{
					NewDay(0f);
				}
				if (state.IsKeyDown(Keys.M) && !oldKBState.IsKeyDown(Keys.M))
				{
					dayOfMonth = 28;
					NewDay(0f);
				}
				if (state.IsKeyDown(Keys.T) && !oldKBState.IsKeyDown(Keys.T))
				{
					addHour();
				}
				if (state.IsKeyDown(Keys.Y) && !oldKBState.IsKeyDown(Keys.Y))
				{
					addMinute();
				}
				if (state.IsKeyDown(Keys.D1) && !oldKBState.IsKeyDown(Keys.D1))
				{
					warpFarmer("Mountain", 15, 35, flip: false);
				}
				if (state.IsKeyDown(Keys.D2) && !oldKBState.IsKeyDown(Keys.D2))
				{
					warpFarmer("Town", 35, 35, flip: false);
				}
				if (state.IsKeyDown(Keys.D3) && !oldKBState.IsKeyDown(Keys.D3))
				{
					warpFarmer("Farm", 64, 15, flip: false);
				}
				if (state.IsKeyDown(Keys.D4) && !oldKBState.IsKeyDown(Keys.D4))
				{
					warpFarmer("Forest", 34, 13, flip: false);
				}
				if (state.IsKeyDown(Keys.D5) && !oldKBState.IsKeyDown(Keys.D4))
				{
					warpFarmer("Beach", 34, 10, flip: false);
				}
				if (state.IsKeyDown(Keys.D6) && !oldKBState.IsKeyDown(Keys.D6))
				{
					warpFarmer("Mine", 18, 12, flip: false);
				}
				if (state.IsKeyDown(Keys.D7) && !oldKBState.IsKeyDown(Keys.D7))
				{
					warpFarmer("SandyHouse", 16, 3, flip: false);
				}
				if (state.IsKeyDown(Keys.K) && !oldKBState.IsKeyDown(Keys.K))
				{
					enterMine(mine.mineLevel + 1);
				}
				if (state.IsKeyDown(Keys.H) && !oldKBState.IsKeyDown(Keys.H))
				{
					player.changeHat(random.Next(FarmerRenderer.hatsTexture.Height / 80 * 12));
				}
				if (state.IsKeyDown(Keys.I) && !oldKBState.IsKeyDown(Keys.I))
				{
					player.changeHairStyle(random.Next(FarmerRenderer.hairStylesTexture.Height / 96 * 8));
				}
				if (state.IsKeyDown(Keys.J) && !oldKBState.IsKeyDown(Keys.J))
				{
					player.changeShirt(random.Next(40));
					player.changePants(new Color(random.Next(255), random.Next(255), random.Next(255)));
				}
				if (state.IsKeyDown(Keys.L) && !oldKBState.IsKeyDown(Keys.L))
				{
					player.changeShirt(random.Next(40));
					player.changePants(new Color(random.Next(255), random.Next(255), random.Next(255)));
					player.changeHairStyle(random.Next(FarmerRenderer.hairStylesTexture.Height / 96 * 8));
					if (random.NextDouble() < 0.5)
					{
						player.changeHat(random.Next(-1, FarmerRenderer.hatsTexture.Height / 80 * 12));
					}
					else
					{
						player.changeHat(-1);
					}
					player.changeHairColor(new Color(random.Next(255), random.Next(255), random.Next(255)));
					player.changeSkinColor(random.Next(16));
				}
				if (state.IsKeyDown(Keys.U) && !oldKBState.IsKeyDown(Keys.U))
				{
					(getLocationFromName("FarmHouse") as FarmHouse).setWallpaper(random.Next(112), -1, persist: true);
					(getLocationFromName("FarmHouse") as FarmHouse).setFloor(random.Next(40), -1, persist: true);
				}
				if (state.IsKeyDown(Keys.F2))
				{
					oldKBState.IsKeyDown(Keys.F2);
				}
				if (state.IsKeyDown(Keys.F5) && !oldKBState.IsKeyDown(Keys.F5))
				{
					displayFarmer = !displayFarmer;
				}
				if (state.IsKeyDown(Keys.F6))
				{
					oldKBState.IsKeyDown(Keys.F6);
				}
				if (state.IsKeyDown(Keys.F7) && !oldKBState.IsKeyDown(Keys.F7))
				{
					drawGrid = !drawGrid;
				}
				if (state.IsKeyDown(Keys.B) && !oldKBState.IsKeyDown(Keys.B))
				{
					player.shiftToolbar(right: false);
				}
				if (state.IsKeyDown(Keys.N) && !oldKBState.IsKeyDown(Keys.N))
				{
					player.shiftToolbar(right: true);
				}
				if (state.IsKeyDown(Keys.F10) && !oldKBState.IsKeyDown(Keys.F10) && server == null)
				{
					multiplayer.StartServer();
				}
			}
			else if (!player.UsingTool)
			{
				if (isOneOfTheseKeysDown(state, options.inventorySlot1) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot1))
				{
					player.CurrentToolIndex = 0;
				}
				else if (isOneOfTheseKeysDown(state, options.inventorySlot2) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot2))
				{
					player.CurrentToolIndex = 1;
				}
				else if (isOneOfTheseKeysDown(state, options.inventorySlot3) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot3))
				{
					player.CurrentToolIndex = 2;
				}
				else if (isOneOfTheseKeysDown(state, options.inventorySlot4) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot4))
				{
					player.CurrentToolIndex = 3;
				}
				else if (isOneOfTheseKeysDown(state, options.inventorySlot5) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot5))
				{
					player.CurrentToolIndex = 4;
				}
				else if (isOneOfTheseKeysDown(state, options.inventorySlot6) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot6))
				{
					player.CurrentToolIndex = 5;
				}
				else if (isOneOfTheseKeysDown(state, options.inventorySlot7) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot7))
				{
					player.CurrentToolIndex = 6;
				}
				else if (isOneOfTheseKeysDown(state, options.inventorySlot8) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot8))
				{
					player.CurrentToolIndex = 7;
				}
				else if (isOneOfTheseKeysDown(state, options.inventorySlot9) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot9))
				{
					player.CurrentToolIndex = 8;
				}
				else if (isOneOfTheseKeysDown(state, options.inventorySlot10) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot10))
				{
					player.CurrentToolIndex = 9;
				}
				else if (isOneOfTheseKeysDown(state, options.inventorySlot11) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot11))
				{
					player.CurrentToolIndex = 10;
				}
				else if (isOneOfTheseKeysDown(state, options.inventorySlot12) && areAllOfTheseKeysUp(oldKBState, options.inventorySlot12))
				{
					player.CurrentToolIndex = 11;
				}
			}
			if (((options.gamepadControls && rightStickHoldTime >= emoteMenuShowTime && activeClickableMenu == null) || (isOneOfTheseKeysDown(input.GetKeyboardState(), options.emoteButton) && areAllOfTheseKeysUp(oldKBState, options.emoteButton))) && !debugMode && player.CanEmote())
			{
				if (player.CanMove)
				{
					player.Halt();
				}
				emoteMenu = new EmoteMenu();
				emoteMenu.gamepadMode = options.gamepadControls && rightStickHoldTime >= emoteMenuShowTime;
				timerUntilMouseFade = 0;
			}
			if (!Program.releaseBuild)
			{
				if (IsPressEvent(ref state, Keys.F3) || IsPressEvent(ref state2, Buttons.LeftStick))
				{
					debugMode = !debugMode;
					if (gameMode == 11)
					{
						gameMode = 3;
					}
				}
				if (IsPressEvent(ref state, Keys.F8))
				{
					requestDebugInput();
				}
			}
			if (state.IsKeyDown(Keys.F4) && !oldKBState.IsKeyDown(Keys.F4))
			{
				displayHUD = !displayHUD;
				playSound("smallSelect");
				if (!displayHUD)
				{
					showGlobalMessage(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3666"));
				}
			}
			bool flag5 = isOneOfTheseKeysDown(state, options.menuButton) && areAllOfTheseKeysUp(oldKBState, options.menuButton);
			bool flag6 = isOneOfTheseKeysDown(state, options.journalButton) && areAllOfTheseKeysUp(oldKBState, options.journalButton);
			bool flag7 = isOneOfTheseKeysDown(state, options.mapButton) && areAllOfTheseKeysUp(oldKBState, options.mapButton);
			if (options.gamepadControls && !flag5)
			{
				flag5 = (state2.IsButtonDown(Buttons.Start) && !oldPadState.IsButtonDown(Buttons.Start)) || (state2.IsButtonDown(Buttons.B) && !oldPadState.IsButtonDown(Buttons.B));
			}
			if (options.gamepadControls && !flag6)
			{
				flag6 = state2.IsButtonDown(Buttons.Back) && !oldPadState.IsButtonDown(Buttons.Back);
			}
			if (options.gamepadControls && !flag7)
			{
				flag7 = state2.IsButtonDown(Buttons.Y) && !oldPadState.IsButtonDown(Buttons.Y);
			}
			if (flag5 && CanShowPauseMenu() && activeClickableMenu == null)
			{
				PushUIMode();
				activeClickableMenu = new GameMenu();
				PopUIMode();
			}
			if (dayOfMonth > 0 && player.CanMove && flag6 && !dialogueUp && !flag4)
			{
				if (activeClickableMenu == null)
				{
					activeClickableMenu = new QuestLog();
				}
			}
			else if (flag4 && CurrentEvent != null && flag6 && !CurrentEvent.skipped && CurrentEvent.skippable)
			{
				CurrentEvent.skipped = true;
				CurrentEvent.skipEvent();
				freezeControls = false;
			}
			if (options.gamepadControls && dayOfMonth > 0 && player.CanMove && isAnyGamePadButtonBeingPressed() && flag7 && !dialogueUp && !flag4)
			{
				if (activeClickableMenu == null)
				{
					PushUIMode();
					activeClickableMenu = new GameMenu(3);
					PopUIMode();
				}
			}
			else if (dayOfMonth > 0 && player.CanMove && flag7 && !dialogueUp && !flag4 && activeClickableMenu == null)
			{
				PushUIMode();
				activeClickableMenu = new GameMenu(4);
				PopUIMode();
			}
			checkForRunButton(state);
			oldKBState = state;
			oldMouseState = input.GetMouseState();
			oldPadState = state2;
		}

		public static bool CanShowPauseMenu()
		{
			if (dayOfMonth > 0 && player.CanMove && !dialogueUp && (!eventUp || (isFestival() && CurrentEvent.festivalTimer <= 0)) && currentMinigame == null)
			{
				return farmEvent == null;
			}
			return false;
		}

		private static void addHour()
		{
			timeOfDay += 100;
			foreach (GameLocation location in locations)
			{
				for (int i = 0; i < location.getCharacters().Count; i++)
				{
					location.getCharacters()[i].checkSchedule(timeOfDay);
					location.getCharacters()[i].checkSchedule(timeOfDay - 50);
					location.getCharacters()[i].checkSchedule(timeOfDay - 60);
					location.getCharacters()[i].checkSchedule(timeOfDay - 70);
					location.getCharacters()[i].checkSchedule(timeOfDay - 80);
					location.getCharacters()[i].checkSchedule(timeOfDay - 90);
				}
			}
			switch (timeOfDay)
			{
			case 1900:
				globalOutdoorLighting = 0.5f;
				currentLocation.switchOutNightTiles();
				break;
			case 2000:
				globalOutdoorLighting = 0.7f;
				if (!IsRainingHere())
				{
					changeMusicTrack("none");
				}
				break;
			case 2100:
				globalOutdoorLighting = 0.9f;
				break;
			case 2200:
				globalOutdoorLighting = 1f;
				break;
			}
		}

		private static void addMinute()
		{
			if (GetKeyboardState().IsKeyDown(Keys.LeftShift))
			{
				timeOfDay -= 10;
			}
			else
			{
				timeOfDay += 10;
			}
			if (timeOfDay % 100 == 60)
			{
				timeOfDay += 40;
			}
			if (timeOfDay % 100 == 90)
			{
				timeOfDay -= 40;
			}
			currentLocation.performTenMinuteUpdate(timeOfDay);
			foreach (GameLocation location in locations)
			{
				for (int i = 0; i < location.getCharacters().Count; i++)
				{
					location.getCharacters()[i].checkSchedule(timeOfDay);
				}
			}
			if (isLightning && IsMasterGame)
			{
				Utility.performLightningUpdate(timeOfDay);
			}
			switch (timeOfDay)
			{
			case 1750:
				globalOutdoorLighting = 0f;
				outdoorLight = Color.White;
				break;
			case 1900:
				globalOutdoorLighting = 0.5f;
				currentLocation.switchOutNightTiles();
				break;
			case 2000:
				globalOutdoorLighting = 0.7f;
				if (!IsRainingHere())
				{
					changeMusicTrack("none");
				}
				break;
			case 2100:
				globalOutdoorLighting = 0.9f;
				break;
			case 2200:
				globalOutdoorLighting = 1f;
				break;
			}
		}

		public static void checkForRunButton(KeyboardState kbState, bool ignoreKeyPressQualifier = false)
		{
			bool running = player.running;
			bool flag = isOneOfTheseKeysDown(kbState, options.runButton) && (!isOneOfTheseKeysDown(oldKBState, options.runButton) || ignoreKeyPressQualifier);
			bool flag2 = !isOneOfTheseKeysDown(kbState, options.runButton) && (isOneOfTheseKeysDown(oldKBState, options.runButton) || ignoreKeyPressQualifier);
			if (options.gamepadControls)
			{
				if (!options.autoRun && Math.Abs(Vector2.Distance(input.GetGamePadState().ThumbSticks.Left, Vector2.Zero)) > 0.9f)
				{
					flag = true;
				}
				else if (Math.Abs(Vector2.Distance(oldPadState.ThumbSticks.Left, Vector2.Zero)) > 0.9f && Math.Abs(Vector2.Distance(input.GetGamePadState().ThumbSticks.Left, Vector2.Zero)) <= 0.9f)
				{
					flag2 = true;
				}
			}
			if (flag && !player.canOnlyWalk)
			{
				player.setRunning(!options.autoRun);
				player.setMoving((byte)(player.running ? 16u : 48u));
			}
			else if (flag2 && !player.canOnlyWalk)
			{
				player.setRunning(options.autoRun);
				player.setMoving((byte)(player.running ? 16u : 48u));
			}
			if (player.running != running && !player.UsingTool)
			{
				player.Halt();
			}
		}

		public static void drawTitleScreenBackground(GameTime gameTime, string dayNight, int weatherDebrisOffsetDay)
		{
		}

		public static Vector2 getMostRecentViewportMotion()
		{
			return new Vector2((float)viewport.X - previousViewportPosition.X, (float)viewport.Y - previousViewportPosition.Y);
		}

		protected virtual void drawOverlays(SpriteBatch spriteBatch)
		{
			if (takingMapScreenshot)
			{
				return;
			}
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (overlayMenu != null)
			{
				overlayMenu.draw(spriteBatch);
			}
			if (chatBox != null)
			{
				chatBox.update(currentGameTime);
				chatBox.draw(spriteBatch);
			}
			if (textEntry != null)
			{
				textEntry.draw(spriteBatch);
			}
			if (TutorialManager.Instance != null)
			{
				TutorialManager.Instance.drawButtonHands(spriteBatch);
				TutorialManager.Instance.draw(spriteBatch);
				TutorialManager.Instance.drawUI(spriteBatch);
			}
			if ((displayHUD || eventUp || currentLocation is Summit) && currentBillboard == 0 && gameMode == 3 && !freezeControls && !panMode)
			{
				drawMouseCursor();
				if (player.CanMove || player.UsingTool)
				{
					DrawVirtualJoypad();
				}
			}
			spriteBatch.End();
		}

		public static void setBGColor(byte r, byte g, byte b)
		{
			bgColor.R = r;
			bgColor.G = g;
			bgColor.B = b;
		}

		public void Instance_Draw(GameTime gameTime)
		{
			Draw(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			isDrawing = true;
			RenderTarget2D renderTarget2D = null;
			if (ShouldDrawOnBuffer())
			{
				renderTarget2D = screen;
			}
			if (uiScreen != null)
			{
				SetRenderTarget(uiScreen);
				base.GraphicsDevice.Clear(Color.Transparent);
				SetRenderTarget(renderTarget2D);
			}
			_draw(gameTime, renderTarget2D);
			isRenderingScreenBuffer = true;
			renderScreenBuffer(renderTarget2D);
			isRenderingScreenBuffer = false;
			if (uiModeCount != 0)
			{
				Console.WriteLine("WARNING: Mismatched UI Mode Push/Pop counts. Correcting.");
				while (uiModeCount < 0)
				{
					PushUIMode();
				}
				while (uiModeCount > 0)
				{
					PopUIMode();
				}
			}
			base.Draw(gameTime);
			isDrawing = false;
		}

		public virtual bool ShouldDrawOnBuffer()
		{
			if (LocalMultiplayer.IsLocalMultiplayer())
			{
				return true;
			}
			if (options.zoomLevel != 1f)
			{
				return true;
			}
			return false;
		}

		public static bool ShouldShowOnscreenUsernames()
		{
			return false;
		}

		public virtual bool checkCharacterTilesForShadowDrawFlag(Character character)
		{
			if (character is Farmer && (character as Farmer).onBridge.Value)
			{
				return true;
			}
			Microsoft.Xna.Framework.Rectangle boundingBox = character.GetBoundingBox();
			boundingBox.Height += 8;
			int num = boundingBox.Right / 64;
			int num2 = boundingBox.Bottom / 64;
			int num3 = boundingBox.Left / 64;
			int num4 = boundingBox.Top / 64;
			for (int i = num3; i <= num; i++)
			{
				for (int j = num4; j <= num2; j++)
				{
					if (currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(new Vector2(i, j)))
					{
						return true;
					}
				}
			}
			return false;
		}

		protected virtual void _draw(GameTime gameTime, RenderTarget2D target_screen)
		{
			showingHealthBar = false;
			if (_newDayTask != null || isLocalMultiplayerNewDayActive)
			{
				base.GraphicsDevice.Clear(bgColor);
				return;
			}
			if (target_screen != null)
			{
				SetRenderTarget(target_screen);
			}
			if (IsSaving)
			{
				base.GraphicsDevice.Clear(bgColor);
				PushUIMode();
				IClickableMenu clickableMenu = activeClickableMenu;
				if (clickableMenu != null)
				{
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
					clickableMenu.draw(spriteBatch);
					spriteBatch.End();
				}
				if (overlayMenu != null)
				{
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
					overlayMenu.draw(spriteBatch);
					spriteBatch.End();
				}
				PopUIMode();
				return;
			}
			base.GraphicsDevice.Clear(bgColor);
			if (activeClickableMenu != null && options.showMenuBackground && activeClickableMenu.showWithoutTransparencyIfOptionIsSet() && !takingMapScreenshot)
			{
				PushUIMode();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				activeClickableMenu.drawBackground(spriteBatch);
				for (IClickableMenu childMenu = activeClickableMenu; childMenu != null; childMenu = childMenu.GetChildMenu())
				{
					childMenu.draw(spriteBatch);
				}
				if (specialCurrencyDisplay != null)
				{
					specialCurrencyDisplay.Draw(spriteBatch);
				}
				spriteBatch.End();
				drawOverlays(spriteBatch);
				PopUIMode();
				return;
			}
			if (emergencyLoading)
			{
				if (!SeenConcernedApeLogo)
				{
					PushUIMode();
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
					if (logoFadeTimer < 5000)
					{
						spriteBatch.Draw(staminaRect, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.White);
					}
					if (logoFadeTimer > 4500)
					{
						float num = Math.Min(1f, (float)(logoFadeTimer - 4500) / 500f);
						spriteBatch.Draw(staminaRect, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black * num);
					}
					spriteBatch.Draw(titleButtonsTexture, new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height / 2 - 90), new Microsoft.Xna.Framework.Rectangle(171 + ((logoFadeTimer / 100 % 2 == 0) ? 111 : 0), 311, 111, 60), Color.White * ((logoFadeTimer < 500) ? ((float)logoFadeTimer / 500f) : ((logoFadeTimer > 4500) ? (1f - (float)(logoFadeTimer - 4500) / 500f) : 1f)), 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.2f);
					spriteBatch.Draw(titleButtonsTexture, new Vector2(Game1.viewport.Width / 2 - 261, Game1.viewport.Height / 2 - 102), new Microsoft.Xna.Framework.Rectangle((logoFadeTimer / 100 % 2 == 0) ? 85 : 0, 306, 85, 69), Color.White * ((logoFadeTimer < 500) ? ((float)logoFadeTimer / 500f) : ((logoFadeTimer > 4500) ? (1f - (float)(logoFadeTimer - 4500) / 500f) : 1f)), 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.2f);
					spriteBatch.End();
					PopUIMode();
				}
				logoFadeTimer -= gameTime.ElapsedGameTime.Milliseconds;
			}
			if (gameMode == 11)
			{
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				spriteBatch.DrawString(dialogueFont, content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3685"), new Vector2(16f, 16f), Color.HotPink);
				spriteBatch.DrawString(dialogueFont, content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3686"), new Vector2(16f, 32f), new Color(0, 255, 0));
				spriteBatch.DrawString(dialogueFont, parseText(errorMessage, dialogueFont, graphics.GraphicsDevice.Viewport.Width), new Vector2(16f, 48f), Color.White);
				spriteBatch.End();
				return;
			}
			if (currentMinigame != null)
			{
				currentMinigame.draw(spriteBatch);
				if (globalFade && !menuUp && (!nameSelectUp || messagePause))
				{
					PushUIMode();
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
					spriteBatch.Draw(fadeToBlackRect, graphics.GraphicsDevice.Viewport.Bounds, Color.Black * ((gameMode == 0) ? (1f - fadeToBlackAlpha) : fadeToBlackAlpha));
					spriteBatch.End();
					PopUIMode();
				}
				PushUIMode();
				drawOverlays(spriteBatch);
				PopUIMode();
				SetRenderTarget(target_screen);
				return;
			}
			if (showingEndOfNightStuff)
			{
				PushUIMode();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				if (activeClickableMenu != null)
				{
					for (IClickableMenu childMenu2 = activeClickableMenu; childMenu2 != null; childMenu2 = childMenu2.GetChildMenu())
					{
						childMenu2.draw(spriteBatch);
					}
				}
				spriteBatch.End();
				drawOverlays(spriteBatch);
				PopUIMode();
				return;
			}
			if (gameMode == 6 || (gameMode == 3 && currentLocation == null))
			{
				PushUIMode();
				base.GraphicsDevice.Clear(bgColor);
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				string text = "";
				for (int i = 0; (double)i < gameTime.TotalGameTime.TotalMilliseconds % 999.0 / 333.0; i++)
				{
					text += ".";
				}
				string text2 = content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3688");
				string s = text2 + text;
				string text3 = text2 + "... ";
				int widthOfString = SpriteText.getWidthOfString(text3);
				int num2 = 64;
				int num3 = 64;
				int num4 = graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - num2;
				int num5 = 20;
				num3 = ((xEdge > 0) ? (51 + xEdge) : (51 + num5));
				num4 = uiViewport.Height - 63 - num5;
				SpriteText.drawString(spriteBatch, s, num3, num4, 999999, widthOfString, num2, 1f, 0.88f, junimoText: false, 0, text3);
				spriteBatch.End();
				drawOverlays(spriteBatch);
				_ = gameMode;
				_ = 3;
				PopUIMode();
				return;
			}
			if (gameMode == 0)
			{
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			}
			else
			{
				if (gameMode == 3 && dayOfMonth == 0 && newDay)
				{
					base.Draw(gameTime);
					return;
				}
				if (drawLighting)
				{
					SetRenderTarget(lightmap);
					base.GraphicsDevice.Clear(Color.White * 0f);
					Matrix value = Matrix.Identity;
					if (useUnscaledLighting)
					{
						value = Matrix.CreateScale(options.zoomLevel);
					}
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, value);
					Color color = ((currentLocation.Name.StartsWith("UndergroundMine") && currentLocation is MineShaft) ? (currentLocation as MineShaft).getLightingColor(gameTime) : ((ambientLight.Equals(Color.White) || (IsRainingHere() && (bool)currentLocation.isOutdoors)) ? outdoorLight : ambientLight));
					float num6 = 1f;
					if (player.hasBuff(26))
					{
						if (color == Color.White)
						{
							color = new Color(0.75f, 0.75f, 0.75f);
						}
						else
						{
							color.R = (byte)Utility.Lerp((int)color.R, 255f, 0.5f);
							color.G = (byte)Utility.Lerp((int)color.G, 255f, 0.5f);
							color.B = (byte)Utility.Lerp((int)color.B, 255f, 0.5f);
						}
						num6 = 0.33f;
					}
					spriteBatch.Draw(staminaRect, lightmap.Bounds, color);
					foreach (LightSource currentLightSource in currentLightSources)
					{
						if ((IsRainingHere() || isDarkOut()) && currentLightSource.lightContext.Value == LightSource.LightContext.WindowLight)
						{
							continue;
						}
						if (currentLightSource.PlayerID != 0L && currentLightSource.PlayerID != player.UniqueMultiplayerID)
						{
							Farmer farmerMaybeOffline = getFarmerMaybeOffline(currentLightSource.PlayerID);
							if (farmerMaybeOffline == null || (farmerMaybeOffline.currentLocation != null && farmerMaybeOffline.currentLocation.Name != currentLocation.Name) || (bool)farmerMaybeOffline.hidden)
							{
								continue;
							}
						}
						if (Utility.isOnScreen(currentLightSource.position, (int)((float)currentLightSource.radius * 64f * 4f)))
						{
							spriteBatch.Draw(currentLightSource.lightTexture, GlobalToLocal(Game1.viewport, currentLightSource.position) / (options.lightingQuality / 2), currentLightSource.lightTexture.Bounds, currentLightSource.color.Value * num6, 0f, new Vector2(currentLightSource.lightTexture.Bounds.Width / 2, currentLightSource.lightTexture.Bounds.Height / 2), (float)currentLightSource.radius / (float)(options.lightingQuality / 2), SpriteEffects.None, 0.9f);
						}
					}
					spriteBatch.End();
					SetRenderTarget(target_screen);
				}
				base.GraphicsDevice.Clear(bgColor);
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				if (background != null)
				{
					background.draw(spriteBatch);
				}
				currentLocation.drawBackground(spriteBatch);
				mapDisplayDevice.BeginScene(spriteBatch);
				currentLocation.Map.GetLayer("Back").Draw(mapDisplayDevice, Game1.viewport, Location.Origin, wrapAround: false, 4);
				currentLocation.drawWater(spriteBatch);
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
				currentLocation.drawFloorDecorations(spriteBatch);
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				_farmerShadows.Clear();
				if (currentLocation.currentEvent != null && !currentLocation.currentEvent.isFestival && currentLocation.currentEvent.farmerActors.Count > 0)
				{
					foreach (Farmer farmerActor in currentLocation.currentEvent.farmerActors)
					{
						if ((farmerActor.IsLocalPlayer && displayFarmer) || !farmerActor.hidden)
						{
							_farmerShadows.Add(farmerActor);
						}
					}
				}
				else
				{
					foreach (Farmer farmer in currentLocation.farmers)
					{
						if ((farmer.IsLocalPlayer && displayFarmer) || !farmer.hidden)
						{
							_farmerShadows.Add(farmer);
						}
					}
				}
				if (!currentLocation.shouldHideCharacters())
				{
					if (CurrentEvent == null)
					{
						foreach (NPC character in currentLocation.characters)
						{
							if (!character.swimming && !character.HideShadow && !character.IsInvisible && !checkCharacterTilesForShadowDrawFlag(character))
							{
								spriteBatch.Draw(shadowTexture, GlobalToLocal(Game1.viewport, character.GetShadowOffset() + character.Position + new Vector2((float)(character.GetSpriteWidthForPositioning() * 4) / 2f, character.GetBoundingBox().Height + ((!character.IsMonster) ? 12 : 0))), shadowTexture.Bounds, Color.White, 0f, new Vector2(shadowTexture.Bounds.Center.X, shadowTexture.Bounds.Center.Y), Math.Max(0f, (4f + (float)character.yJumpOffset / 40f) * (float)character.scale), SpriteEffects.None, Math.Max(0f, (float)character.getStandingY() / 10000f) - 1E-06f);
							}
						}
					}
					else
					{
						foreach (NPC actor in CurrentEvent.actors)
						{
							if ((CurrentEvent == null || !CurrentEvent.ShouldHideCharacter(actor)) && !actor.swimming && !actor.HideShadow && !checkCharacterTilesForShadowDrawFlag(actor))
							{
								spriteBatch.Draw(shadowTexture, GlobalToLocal(Game1.viewport, actor.GetShadowOffset() + actor.Position + new Vector2((float)(actor.GetSpriteWidthForPositioning() * 4) / 2f, actor.GetBoundingBox().Height + ((!actor.IsMonster) ? ((actor.Sprite.SpriteHeight <= 16) ? (-4) : 12) : 0))), shadowTexture.Bounds, Color.White, 0f, new Vector2(shadowTexture.Bounds.Center.X, shadowTexture.Bounds.Center.Y), Math.Max(0f, 4f + (float)actor.yJumpOffset / 40f) * (float)actor.scale, SpriteEffects.None, Math.Max(0f, (float)actor.getStandingY() / 10000f) - 1E-06f);
							}
						}
					}
					foreach (Farmer farmerShadow in _farmerShadows)
					{
						if (!multiplayer.isDisconnecting(farmerShadow.UniqueMultiplayerID) && !farmerShadow.swimming && !farmerShadow.isRidingHorse() && !farmerShadow.IsSitting() && (currentLocation == null || !checkCharacterTilesForShadowDrawFlag(farmerShadow)))
						{
							spriteBatch.Draw(shadowTexture, GlobalToLocal(farmerShadow.GetShadowOffset() + farmerShadow.Position + new Vector2(32f, 24f)), shadowTexture.Bounds, Color.White, 0f, new Vector2(shadowTexture.Bounds.Center.X, shadowTexture.Bounds.Center.Y), 4f - (((farmerShadow.running || farmerShadow.UsingTool) && farmerShadow.FarmerSprite.currentAnimationIndex > 1) ? ((float)Math.Abs(FarmerRenderer.featureYOffsetPerFrame[farmerShadow.FarmerSprite.CurrentFrame]) * 0.5f) : 0f), SpriteEffects.None, 0f);
						}
					}
				}
				Layer layer = currentLocation.Map.GetLayer("Buildings");
				layer.Draw(mapDisplayDevice, Game1.viewport, Location.Origin, wrapAround: false, 4);
				mapDisplayDevice.EndScene();
				if (currentLocation != null && currentLocation.tapToMove.targetNPC != null)
				{
					spriteBatch.Draw(mouseCursors, GlobalToLocal(Game1.viewport, currentLocation.tapToMove.targetNPC.Position + new Vector2((float)(currentLocation.tapToMove.targetNPC.Sprite.SpriteWidth * 4) / 2f - 32f, currentLocation.tapToMove.targetNPC.GetBoundingBox().Height + ((!currentLocation.tapToMove.targetNPC.IsMonster) ? 12 : 0) - 32)), new Microsoft.Xna.Framework.Rectangle(194, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.58f);
				}
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
				if (!currentLocation.shouldHideCharacters())
				{
					if (CurrentEvent == null)
					{
						foreach (NPC character2 in currentLocation.characters)
						{
							if (!character2.swimming && !character2.HideShadow && !character2.isInvisible && checkCharacterTilesForShadowDrawFlag(character2))
							{
								spriteBatch.Draw(shadowTexture, GlobalToLocal(Game1.viewport, character2.GetShadowOffset() + character2.Position + new Vector2((float)(character2.GetSpriteWidthForPositioning() * 4) / 2f, character2.GetBoundingBox().Height + ((!character2.IsMonster) ? 12 : 0))), shadowTexture.Bounds, Color.White, 0f, new Vector2(shadowTexture.Bounds.Center.X, shadowTexture.Bounds.Center.Y), Math.Max(0f, (4f + (float)character2.yJumpOffset / 40f) * (float)character2.scale), SpriteEffects.None, Math.Max(0f, (float)character2.getStandingY() / 10000f) - 1E-06f);
							}
						}
					}
					else
					{
						foreach (NPC actor2 in CurrentEvent.actors)
						{
							if ((CurrentEvent == null || !CurrentEvent.ShouldHideCharacter(actor2)) && !actor2.swimming && !actor2.HideShadow && checkCharacterTilesForShadowDrawFlag(actor2))
							{
								spriteBatch.Draw(shadowTexture, GlobalToLocal(Game1.viewport, actor2.GetShadowOffset() + actor2.Position + new Vector2((float)(actor2.GetSpriteWidthForPositioning() * 4) / 2f, actor2.GetBoundingBox().Height + ((!actor2.IsMonster) ? 12 : 0))), shadowTexture.Bounds, Color.White, 0f, new Vector2(shadowTexture.Bounds.Center.X, shadowTexture.Bounds.Center.Y), Math.Max(0f, (4f + (float)actor2.yJumpOffset / 40f) * (float)actor2.scale), SpriteEffects.None, Math.Max(0f, (float)actor2.getStandingY() / 10000f) - 1E-06f);
							}
						}
					}
					foreach (Farmer farmerShadow2 in _farmerShadows)
					{
						float layerDepth = Math.Max(0.0001f, farmerShadow2.getDrawLayer() + 0.00011f) - 0.0001f;
						if (!farmerShadow2.swimming && !farmerShadow2.isRidingHorse() && !farmerShadow2.IsSitting() && currentLocation != null && checkCharacterTilesForShadowDrawFlag(farmerShadow2))
						{
							spriteBatch.Draw(shadowTexture, GlobalToLocal(farmerShadow2.GetShadowOffset() + farmerShadow2.Position + new Vector2(32f, 24f)), shadowTexture.Bounds, Color.White, 0f, new Vector2(shadowTexture.Bounds.Center.X, shadowTexture.Bounds.Center.Y), 4f - (((farmerShadow2.running || farmerShadow2.UsingTool) && farmerShadow2.FarmerSprite.currentAnimationIndex > 1) ? ((float)Math.Abs(FarmerRenderer.featureYOffsetPerFrame[farmerShadow2.FarmerSprite.CurrentFrame]) * 0.5f) : 0f), SpriteEffects.None, layerDepth);
						}
					}
				}
				if ((eventUp || killScreen) && !killScreen && currentLocation.currentEvent != null)
				{
					currentLocation.currentEvent.draw(spriteBatch);
				}
				if (player.currentUpgrade != null && player.currentUpgrade.daysLeftTillUpgradeDone <= 3 && currentLocation.Name.Equals("Farm"))
				{
					spriteBatch.Draw(player.currentUpgrade.workerTexture, GlobalToLocal(Game1.viewport, player.currentUpgrade.positionOfCarpenter), player.currentUpgrade.getSourceRectangle(), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, (player.currentUpgrade.positionOfCarpenter.Y + 48f) / 10000f);
				}
				currentLocation.draw(spriteBatch);
				foreach (KeyValuePair<Vector2, int> crabPotOverlayTile in crabPotOverlayTiles)
				{
					Vector2 key = crabPotOverlayTile.Key;
					Tile tile = layer.Tiles[(int)key.X, (int)key.Y];
					if (tile != null)
					{
						Vector2 vector = GlobalToLocal(Game1.viewport, key * 64f);
						Location location = new Location((int)vector.X, (int)vector.Y);
						mapDisplayDevice.DrawTile(tile, location, (key.Y * 64f - 1f) / 10000f);
					}
				}
				if (eventUp && currentLocation.currentEvent != null)
				{
					_ = currentLocation.currentEvent.messageToScreen;
				}
				if (player.ActiveObject == null && (player.UsingTool || pickingTool) && player.CurrentTool != null && (!player.CurrentTool.Name.Equals("Seeds") || pickingTool))
				{
					drawTool(player);
				}
				if (currentLocation.Name.Equals("Farm"))
				{
					drawFarmBuildings();
				}
				if (tvStation >= 0)
				{
					spriteBatch.Draw(tvStationTexture, GlobalToLocal(Game1.viewport, new Vector2(400f, 160f)), new Microsoft.Xna.Framework.Rectangle(tvStation * 24, 0, 24, 15), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-08f);
				}
				if (panMode)
				{
					spriteBatch.Draw(fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((int)Math.Floor((double)(getOldMouseX() + Game1.viewport.X) / 64.0) * 64 - Game1.viewport.X, (int)Math.Floor((double)(getOldMouseY() + Game1.viewport.Y) / 64.0) * 64 - Game1.viewport.Y, 64, 64), Color.Lime * 0.75f);
					foreach (Warp warp in currentLocation.warps)
					{
						spriteBatch.Draw(fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(warp.X * 64 - Game1.viewport.X, warp.Y * 64 - Game1.viewport.Y, 64, 64), Color.Red * 0.75f);
					}
				}
				mapDisplayDevice.BeginScene(spriteBatch);
				currentLocation.Map.GetLayer("Front").Draw(mapDisplayDevice, Game1.viewport, Location.Origin, wrapAround: false, 4);
				mapDisplayDevice.EndScene();
				currentLocation.drawAboveFrontLayer(spriteBatch);
				if (currentLocation.tapToMove.targetNPC == null && (displayHUD || eventUp) && currentBillboard == 0 && gameMode == 3 && !freezeControls && !panMode && !HostPaused)
				{
					DrawTapToMoveTarget();
				}
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				if (currentLocation.Map.GetLayer("AlwaysFront") != null)
				{
					mapDisplayDevice.BeginScene(spriteBatch);
					currentLocation.Map.GetLayer("AlwaysFront").Draw(mapDisplayDevice, Game1.viewport, Location.Origin, wrapAround: false, 4);
					mapDisplayDevice.EndScene();
				}
				if (toolHold > 400f && player.CurrentTool.UpgradeLevel >= 1 && player.canReleaseTool)
				{
					Color color2 = Color.White;
					switch ((int)(toolHold / 600f) + 2)
					{
					case 1:
						color2 = Tool.copperColor;
						break;
					case 2:
						color2 = Tool.steelColor;
						break;
					case 3:
						color2 = Tool.goldColor;
						break;
					case 4:
						color2 = Tool.iridiumColor;
						break;
					}
					spriteBatch.Draw(littleEffect, new Microsoft.Xna.Framework.Rectangle((int)player.getLocalPosition(Game1.viewport).X - 2, (int)player.getLocalPosition(Game1.viewport).Y - ((!player.CurrentTool.Name.Equals("Watering Can")) ? 64 : 0) - 2, (int)(toolHold % 600f * 0.08f) + 4, 12), Color.Black);
					spriteBatch.Draw(littleEffect, new Microsoft.Xna.Framework.Rectangle((int)player.getLocalPosition(Game1.viewport).X, (int)player.getLocalPosition(Game1.viewport).Y - ((!player.CurrentTool.Name.Equals("Watering Can")) ? 64 : 0), (int)(toolHold % 600f * 0.08f), 8), color2);
				}
				if (!IsFakedBlackScreen())
				{
					drawWeather(gameTime, target_screen);
				}
				if (farmEvent != null)
				{
					farmEvent.draw(spriteBatch);
				}
				if (currentLocation.LightLevel > 0f && timeOfDay < 2000)
				{
					spriteBatch.Draw(fadeToBlackRect, graphics.GraphicsDevice.Viewport.Bounds, Color.Black * currentLocation.LightLevel);
				}
				if (screenGlow)
				{
					spriteBatch.Draw(fadeToBlackRect, graphics.GraphicsDevice.Viewport.Bounds, screenGlowColor * screenGlowAlpha);
				}
				currentLocation.drawAboveAlwaysFrontLayer(spriteBatch);
				if (player.CurrentTool != null && player.CurrentTool is FishingRod && ((player.CurrentTool as FishingRod).isTimingCast || (player.CurrentTool as FishingRod).castingChosenCountdown > 0f || (player.CurrentTool as FishingRod).fishCaught || (player.CurrentTool as FishingRod).showingTreasure))
				{
					player.CurrentTool.draw(spriteBatch);
				}
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
				if (eventUp && currentLocation.currentEvent != null)
				{
					foreach (NPC actor3 in currentLocation.currentEvent.actors)
					{
						if (actor3.isEmoting)
						{
							Vector2 localPosition = actor3.getLocalPosition(Game1.viewport);
							if (actor3.NeedsBirdieEmoteHack())
							{
								localPosition.X += 64f;
							}
							localPosition.Y -= 140f;
							if (actor3.Age == 2)
							{
								localPosition.Y += 32f;
							}
							else if (actor3.Gender == 1)
							{
								localPosition.Y += 10f;
							}
							spriteBatch.Draw(emoteSpriteSheet, localPosition, new Microsoft.Xna.Framework.Rectangle(actor3.CurrentEmoteIndex * 16 % emoteSpriteSheet.Width, actor3.CurrentEmoteIndex * 16 / emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)actor3.getStandingY() / 10000f);
						}
					}
				}
				spriteBatch.End();
				if (drawLighting && !IsFakedBlackScreen())
				{
					spriteBatch.Begin(SpriteSortMode.Deferred, lightingBlend, SamplerState.LinearClamp);
					Viewport viewport = base.GraphicsDevice.Viewport;
					viewport.Bounds = target_screen?.Bounds ?? base.GraphicsDevice.PresentationParameters.Bounds;
					base.GraphicsDevice.Viewport = viewport;
					float num7 = options.lightingQuality / 2;
					if (useUnscaledLighting)
					{
						num7 /= options.zoomLevel;
					}
					spriteBatch.Draw(lightmap, Vector2.Zero, lightmap.Bounds, Color.White, 0f, Vector2.Zero, num7, SpriteEffects.None, 1f);
					if (IsRainingHere() && (bool)currentLocation.isOutdoors && !(currentLocation is Desert))
					{
						spriteBatch.Draw(staminaRect, viewport.Bounds, Color.OrangeRed * 0.45f);
					}
					spriteBatch.End();
				}
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				if (drawGrid)
				{
					int num8 = -Game1.viewport.X % 64;
					float num9 = -Game1.viewport.Y % 64;
					for (int j = num8; j < graphics.GraphicsDevice.Viewport.Width; j += 64)
					{
						spriteBatch.Draw(staminaRect, new Microsoft.Xna.Framework.Rectangle(j, (int)num9, 1, graphics.GraphicsDevice.Viewport.Height), Color.Red * 0.5f);
					}
					for (float num10 = num9; num10 < (float)graphics.GraphicsDevice.Viewport.Height; num10 += 64f)
					{
						spriteBatch.Draw(staminaRect, new Microsoft.Xna.Framework.Rectangle(num8, (int)num10, graphics.GraphicsDevice.Viewport.Width, 1), Color.Red * 0.5f);
					}
				}
				if (ShouldShowOnscreenUsernames() && currentLocation != null)
				{
					currentLocation.DrawFarmerUsernames(spriteBatch);
				}
				if (currentBillboard != 0 && !takingMapScreenshot)
				{
					drawBillboard();
				}
				if (!eventUp && farmEvent == null && currentBillboard == 0 && gameMode == 3 && !takingMapScreenshot && isOutdoorMapSmallerThanViewport())
				{
					spriteBatch.Draw(fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, -Game1.viewport.X, graphics.GraphicsDevice.Viewport.Height), Color.Black);
					spriteBatch.Draw(fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(-Game1.viewport.X + currentLocation.map.Layers[0].LayerWidth * 64, 0, graphics.GraphicsDevice.Viewport.Width - (-Game1.viewport.X + currentLocation.map.Layers[0].LayerWidth * 64), graphics.GraphicsDevice.Viewport.Height), Color.Black);
					spriteBatch.Draw(fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, -Game1.viewport.Y), Color.Black);
					spriteBatch.Draw(fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, -Game1.viewport.Y + currentLocation.map.Layers[0].LayerHeight * 64, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height - (-Game1.viewport.Y + currentLocation.map.Layers[0].LayerHeight * 64)), Color.Black);
				}
				spriteBatch.End();
				PushUIMode();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				if ((displayHUD || eventUp) && currentBillboard == 0 && gameMode == 3 && !freezeControls && !panMode && !HostPaused && !takingMapScreenshot)
				{
					drawHUD();
					if (!takingMapScreenshot)
					{
						DrawGreenPlacementBounds();
					}
				}
				else if (activeClickableMenu == null)
				{
					_ = farmEvent;
				}
				if (hudMessages.Count > 0 && !takingMapScreenshot)
				{
					for (int num11 = hudMessages.Count - 1; num11 >= 0; num11--)
					{
						hudMessages[num11].draw(spriteBatch, num11);
					}
				}
				spriteBatch.End();
				PopUIMode();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			}
			if (farmEvent != null)
			{
				farmEvent.draw(spriteBatch);
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			}
			PushUIMode();
			if (dialogueUp && !nameSelectUp && !messagePause && (activeClickableMenu == null || !(activeClickableMenu is DialogueBox)) && !takingMapScreenshot)
			{
				drawDialogueBox();
			}
			if (progressBar && !takingMapScreenshot)
			{
				spriteBatch.Draw(fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width - dialogueWidth) / 2, graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 128, dialogueWidth, 32), Color.LightGray);
				spriteBatch.Draw(staminaRect, new Microsoft.Xna.Framework.Rectangle((graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width - dialogueWidth) / 2, graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 128, (int)(pauseAccumulator / pauseTime * (float)dialogueWidth), 32), Color.DimGray);
			}
			spriteBatch.End();
			PopUIMode();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (eventUp && currentLocation != null && currentLocation.currentEvent != null)
			{
				currentLocation.currentEvent.drawAfterMap(spriteBatch);
			}
			if (!IsFakedBlackScreen() && IsRainingHere() && currentLocation != null && (bool)currentLocation.isOutdoors && !(currentLocation is Desert))
			{
				spriteBatch.Draw(staminaRect, graphics.GraphicsDevice.Viewport.Bounds, Color.Blue * 0.2f);
			}
			if ((fadeToBlack || globalFade) && !menuUp && (!nameSelectUp || messagePause) && !takingMapScreenshot)
			{
				spriteBatch.End();
				PushUIMode();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				spriteBatch.Draw(fadeToBlackRect, graphics.GraphicsDevice.Viewport.Bounds, Color.Black * ((gameMode == 0) ? (1f - fadeToBlackAlpha) : fadeToBlackAlpha));
				spriteBatch.End();
				PopUIMode();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			}
			else if (flashAlpha > 0f && !takingMapScreenshot)
			{
				if (options.screenFlash)
				{
					spriteBatch.Draw(fadeToBlackRect, graphics.GraphicsDevice.Viewport.Bounds, Color.White * Math.Min(1f, flashAlpha));
				}
				flashAlpha -= 0.1f;
			}
			if ((messagePause || globalFade) && dialogueUp && !takingMapScreenshot)
			{
				drawDialogueBox();
			}
			if (!takingMapScreenshot)
			{
				foreach (TemporaryAnimatedSprite screenOverlayTempSprite in screenOverlayTempSprites)
				{
					screenOverlayTempSprite.draw(spriteBatch, localPosition: true);
				}
				spriteBatch.End();
				PushUIMode();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				foreach (TemporaryAnimatedSprite uiOverlayTempSprite in uiOverlayTempSprites)
				{
					uiOverlayTempSprite.draw(spriteBatch, localPosition: true);
				}
				spriteBatch.End();
				PopUIMode();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			}
			if (debugMode)
			{
				StringBuilder debugStringBuilder = _debugStringBuilder;
				debugStringBuilder.Clear();
				if (panMode)
				{
					debugStringBuilder.Append((getOldMouseX() + Game1.viewport.X) / 64);
					debugStringBuilder.Append(",");
					debugStringBuilder.Append((getOldMouseY() + Game1.viewport.Y) / 64);
				}
				else
				{
					debugStringBuilder.Append("player: ");
					debugStringBuilder.Append(player.getStandingX() / 64);
					debugStringBuilder.Append(", ");
					debugStringBuilder.Append(player.getStandingY() / 64);
				}
				debugStringBuilder.Append(" mouseTransparency: ");
				debugStringBuilder.Append(mouseCursorTransparency);
				debugStringBuilder.Append(" mousePosition: ");
				debugStringBuilder.Append(getMouseX());
				debugStringBuilder.Append(",");
				debugStringBuilder.Append(getMouseY());
				debugStringBuilder.Append(Environment.NewLine);
				debugStringBuilder.Append(" mouseWorldPosition: ");
				debugStringBuilder.Append(getMouseX() + Game1.viewport.X);
				debugStringBuilder.Append(",");
				debugStringBuilder.Append(getMouseY() + Game1.viewport.Y);
				debugStringBuilder.Append("  debugOutput: ");
				debugStringBuilder.Append(debugOutput);
				spriteBatch.DrawString(smallFont, debugStringBuilder, new Vector2(base.GraphicsDevice.Viewport.GetTitleSafeArea().X, base.GraphicsDevice.Viewport.GetTitleSafeArea().Y + smallFont.LineSpacing * 8), Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
			}
			spriteBatch.End();
			PushUIMode();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (showKeyHelp && !takingMapScreenshot)
			{
				spriteBatch.DrawString(smallFont, keyHelpString, new Vector2(64f, (float)(Game1.viewport.Height - 64 - (dialogueUp ? (192 + (isQuestion ? (questionChoices.Count * 64) : 0)) : 0)) - smallFont.MeasureString(keyHelpString).Y), Color.LightGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
			}
			if (activeClickableMenu != null && !takingMapScreenshot)
			{
				for (IClickableMenu childMenu3 = activeClickableMenu; childMenu3 != null; childMenu3 = childMenu3.GetChildMenu())
				{
					childMenu3.draw(spriteBatch);
				}
			}
			else if (farmEvent != null)
			{
				farmEvent.drawAboveEverything(spriteBatch);
			}
			if (specialCurrencyDisplay != null)
			{
				specialCurrencyDisplay.Draw(spriteBatch);
			}
			if (emoteMenu != null && !takingMapScreenshot)
			{
				emoteMenu.draw(spriteBatch);
			}
			if (HostPaused && !takingMapScreenshot)
			{
				string s2 = content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10378");
				SpriteText.drawStringWithScrollBackground(spriteBatch, s2, 96, 32);
			}
			spriteBatch.End();
			drawOverlays(spriteBatch);
			PopUIMode();
		}

		public virtual void drawWeather(GameTime time, RenderTarget2D target_screen)
		{
			if (IsSnowingHere() && (bool)currentLocation.isOutdoors && !(currentLocation is Desert))
			{
				snowPos.X %= 64f;
				Vector2 position = default(Vector2);
				for (float num = -64f + snowPos.X % 64f; num < (float)viewport.Width; num += 64f)
				{
					for (float num2 = -64f + snowPos.Y % 64f; num2 < (float)viewport.Height; num2 += 64f)
					{
						position.X = (int)num;
						position.Y = (int)num2;
						spriteBatch.Draw(mouseCursors, position, new Microsoft.Xna.Framework.Rectangle(368 + (int)(currentGameTime.TotalGameTime.TotalMilliseconds % 1200.0) / 75 * 16, 192, 16, 16), Color.White * 0.8f * options.snowTransparency, 0f, Vector2.Zero, 4.001f, SpriteEffects.None, 1f);
					}
				}
			}
			if (currentLocation.IsOutdoors && !currentLocation.ignoreDebrisWeather && IsDebrisWeatherHere() && !currentLocation.Name.Equals("Desert"))
			{
				if (takingMapScreenshot)
				{
					if (debrisWeather != null)
					{
						int num3 = _GetWeatherDebrisCountForViewportSize(baseDebrisWeatherCount, viewport);
						if (debrisWeather.Count > 0)
						{
							for (int i = 0; i < num3; i++)
							{
								int index = i % debrisWeather.Count;
								WeatherDebris weatherDebris = debrisWeather[index];
								Vector2 position2 = weatherDebris.position;
								weatherDebris.position = new Vector2(random.Next(viewport.Width - weatherDebris.sourceRect.Width * 3), random.Next(viewport.Height - weatherDebris.sourceRect.Height * 3));
								weatherDebris.draw(spriteBatch);
								weatherDebris.position = position2;
							}
						}
					}
				}
				else if (viewport.X > -viewport.Width)
				{
					foreach (WeatherDebris item in debrisWeather)
					{
						item.draw(spriteBatch);
					}
				}
			}
			if (!IsRainingHere() || !currentLocation.IsOutdoors || currentLocation.Name.Equals("Desert") || currentLocation is Summit)
			{
				return;
			}
			if (takingMapScreenshot)
			{
				int num4 = _GetWeatherDebrisCountForViewportSize(70, viewport);
				for (int j = 0; j < num4; j++)
				{
					Vector2 position3 = new Vector2(random.Next(viewport.Width - 64), random.Next(viewport.Height - 64));
					spriteBatch.Draw(rainTexture, position3, getSourceRectForStandardTileSheet(rainTexture, random.Next(4)), Color.White);
				}
			}
			else if (!eventUp || currentLocation.isTileOnMap(new Vector2(viewport.X / 64, viewport.Y / 64)))
			{
				for (int k = 0; k < rainDrops.Count; k++)
				{
					spriteBatch.Draw(rainTexture, GlobalToLocal(viewport, rainDrops[k].position), getSourceRectForStandardTileSheet(rainTexture, rainDrops[k].frame), Color.White);
				}
			}
		}

		protected virtual void renderScreenBuffer(RenderTarget2D target_screen)
		{
			graphics.GraphicsDevice.SetRenderTarget(null);
			if (!takingMapScreenshot && !LocalMultiplayer.IsLocalMultiplayer() && (target_screen == null || !target_screen.IsContentLost))
			{
				if (ShouldDrawOnBuffer() && target_screen != null)
				{
					base.GraphicsDevice.Clear(bgColor);
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
					spriteBatch.Draw(target_screen, new Vector2(0f, 0f), target_screen.Bounds, Color.White, 0f, Vector2.Zero, options.zoomLevel, SpriteEffects.None, 1f);
					spriteBatch.End();
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
					spriteBatch.Draw(uiScreen, new Vector2(0f, 0f), uiScreen.Bounds, Color.White, 0f, Vector2.Zero, options.uiScale, SpriteEffects.None, 1f);
					spriteBatch.End();
				}
				else
				{
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
					spriteBatch.Draw(uiScreen, new Vector2(0f, 0f), uiScreen.Bounds, Color.White, 0f, Vector2.Zero, options.uiScale, SpriteEffects.None, 1f);
					spriteBatch.End();
				}
			}
		}

		public virtual void DrawSplitScreenWindow()
		{
			if (!LocalMultiplayer.IsLocalMultiplayer())
			{
				return;
			}
			graphics.GraphicsDevice.SetRenderTarget(null);
			if (screen == null || !screen.IsContentLost)
			{
				Viewport viewport = base.GraphicsDevice.Viewport;
				Viewport viewport4 = (base.GraphicsDevice.Viewport = (base.GraphicsDevice.Viewport = defaultDeviceViewport));
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
				spriteBatch.Draw(screen, new Vector2(localMultiplayerWindow.X, localMultiplayerWindow.Y), screen.Bounds, Color.White, 0f, Vector2.Zero, instanceOptions.zoomLevel, SpriteEffects.None, 1f);
				if (uiScreen != null)
				{
					spriteBatch.Draw(uiScreen, new Vector2(localMultiplayerWindow.X, localMultiplayerWindow.Y), uiScreen.Bounds, Color.White, 0f, Vector2.Zero, instanceOptions.uiScale, SpriteEffects.None, 1f);
				}
				spriteBatch.End();
				base.GraphicsDevice.Viewport = viewport;
			}
		}

		public static void drawWithBorder(string message, Color borderColor, Color insideColor, Vector2 position)
		{
			drawWithBorder(message, borderColor, insideColor, position, 0f, 1f, 1f, tiny: false);
		}

		public static void drawWithBorder(string message, Color borderColor, Color insideColor, Vector2 position, float rotate, float scale, float layerDepth)
		{
			drawWithBorder(message, borderColor, insideColor, position, rotate, scale, layerDepth, tiny: false);
		}

		public static void drawWithBorder(string message, Color borderColor, Color insideColor, Vector2 position, float rotate, float scale, float layerDepth, bool tiny)
		{
			string[] array = message.Split(Utility.CharSpace);
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Contains("="))
				{
					spriteBatch.DrawString(tiny ? tinyFont : dialogueFont, array[i], new Vector2(position.X + (float)num, position.Y), Color.Purple, rotate, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
					num += (int)((tiny ? tinyFont : dialogueFont).MeasureString(array[i]).X + 8f);
				}
				else
				{
					spriteBatch.DrawString(tiny ? tinyFont : dialogueFont, array[i], new Vector2(position.X + (float)num, position.Y), insideColor, rotate, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
					num += (int)((tiny ? tinyFont : dialogueFont).MeasureString(array[i]).X + 8f);
				}
			}
		}

		public static bool isOutdoorMapSmallerThanViewport()
		{
			if (uiMode)
			{
				return false;
			}
			if (currentLocation != null && currentLocation.IsOutdoors && !(currentLocation is Summit))
			{
				if (currentLocation.map.Layers[0].LayerWidth * 64 >= viewport.Width)
				{
					return currentLocation.map.Layers[0].LayerHeight * 64 < viewport.Height;
				}
				return true;
			}
			return false;
		}

		protected virtual void drawHUD()
		{
			if (!eventUp && farmEvent == null)
			{
				float num = 0.625f;
				int num2 = 0;
				num2 = 8;
				Vector2 vector = new Vector2(graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Right - xEdge - num2 - 48, graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 224 - 16 - (int)((float)(player.MaxStamina - 270) * num) + num2);
				if (xEdge > num2)
				{
					vector.X += num2;
				}
				if (isOutdoorMapSmallerThanViewport())
				{
					vector.X = Math.Min(vector.X, -viewport.X + currentLocation.map.Layers[0].LayerWidth * 64 - 48);
				}
				if (staminaShakeTimer > 0)
				{
					vector.X += random.Next(-3, 4);
					vector.Y += random.Next(-3, 4);
				}
				spriteBatch.Draw(mouseCursors, vector, new Microsoft.Xna.Framework.Rectangle(256, 408, 12, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				spriteBatch.Draw(mouseCursors, new Microsoft.Xna.Framework.Rectangle((int)vector.X, (int)(vector.Y + 64f), 48, graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 64 - 16 - (int)(vector.Y + 64f - 12f)), new Microsoft.Xna.Framework.Rectangle(256, 424, 12, 16), Color.White);
				spriteBatch.Draw(mouseCursors, new Vector2(vector.X, vector.Y + 224f + (float)(int)((float)(player.MaxStamina - 270) * num) - 64f), new Microsoft.Xna.Framework.Rectangle(256, 448, 12, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				Microsoft.Xna.Framework.Rectangle destinationRectangle = new Microsoft.Xna.Framework.Rectangle((int)vector.X + 12, (int)vector.Y + 16 + 32 + (int)((float)(player.MaxStamina - (int)Math.Max(0f, player.Stamina)) * num), 24, (int)(player.Stamina * num));
				if ((float)getOldMouseX() >= vector.X && (float)getOldMouseY() >= vector.Y)
				{
					drawWithBorder((int)Math.Max(0f, player.Stamina) + "/" + player.MaxStamina, Color.Black * 0f, Color.White, vector + new Vector2(0f - dialogueFont.MeasureString("999/999").X - 16f - (float)(showingHealth ? 64 : 0), 64f));
				}
				Color redToGreenLerpColor = Utility.getRedToGreenLerpColor(player.stamina / (float)(int)player.maxStamina);
				spriteBatch.Draw(staminaRect, destinationRectangle, redToGreenLerpColor);
				destinationRectangle.Height = 4;
				redToGreenLerpColor.R = (byte)Math.Max(0, redToGreenLerpColor.R - 50);
				redToGreenLerpColor.G = (byte)Math.Max(0, redToGreenLerpColor.G - 50);
				spriteBatch.Draw(staminaRect, destinationRectangle, redToGreenLerpColor);
				if ((bool)player.exhausted)
				{
					spriteBatch.Draw(mouseCursors, vector - new Vector2(0f, 11f) * 4f, new Microsoft.Xna.Framework.Rectangle(191, 406, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					if ((float)getOldMouseX() >= vector.X && (float)getOldMouseY() >= vector.Y - 44f)
					{
						drawWithBorder(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3747"), Color.Black * 0f, Color.White, vector + new Vector2(0f - dialogueFont.MeasureString(content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3747")).X - 16f - (float)(showingHealth ? 64 : 0), 96f));
					}
				}
				if (currentLocation is MineShaft || currentLocation is Woods || currentLocation is SlimeHutch || currentLocation is VolcanoDungeon || player.health < player.maxHealth)
				{
					showingHealthBar = true;
					showingHealth = true;
					int num3 = 168 + (player.maxHealth - 100);
					int num4 = (int)((float)player.health / (float)player.maxHealth * (float)num3);
					vector.X -= 56 + ((hitShakeTimer > 0) ? random.Next(-3, 4) : 0);
					vector.Y = graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 224 - (player.maxHealth - 100) - 16 + num2;
					spriteBatch.Draw(mouseCursors, vector, new Microsoft.Xna.Framework.Rectangle(268, 408, 12, 16), (player.health < 20) ? (Color.Pink * ((float)Math.Sin(currentGameTime.TotalGameTime.TotalMilliseconds / (double)((float)player.health * 50f)) / 4f + 0.9f)) : Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					spriteBatch.Draw(mouseCursors, new Microsoft.Xna.Framework.Rectangle((int)vector.X, (int)(vector.Y + 64f), 48, graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 64 - num2 - (int)(vector.Y + 64f)), new Microsoft.Xna.Framework.Rectangle(256, 424, 12, 16), (player.health < 20) ? (Color.Pink * ((float)Math.Sin(currentGameTime.TotalGameTime.TotalMilliseconds / (double)((float)player.health * 50f)) / 4f + 0.9f)) : Color.White);
					spriteBatch.Draw(mouseCursors, new Vector2(vector.X, graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 64 - num2), new Microsoft.Xna.Framework.Rectangle(268, 448, 12, 16), (player.health < 20) ? (Color.Pink * ((float)Math.Sin(currentGameTime.TotalGameTime.TotalMilliseconds / (double)((float)player.health * 50f)) / 4f + 0.9f)) : Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					Microsoft.Xna.Framework.Rectangle destinationRectangle2 = new Microsoft.Xna.Framework.Rectangle((int)vector.X + 12, (int)vector.Y + 16 + 32 + num3 - num4, 24, num4);
					redToGreenLerpColor = Utility.getRedToGreenLerpColor((float)player.health / (float)player.maxHealth);
					spriteBatch.Draw(staminaRect, destinationRectangle2, staminaRect.Bounds, redToGreenLerpColor, 0f, Vector2.Zero, SpriteEffects.None, 1f);
					redToGreenLerpColor.R = (byte)Math.Max(0, redToGreenLerpColor.R - 50);
					redToGreenLerpColor.G = (byte)Math.Max(0, redToGreenLerpColor.G - 50);
					if ((float)getOldMouseX() >= vector.X && (float)getOldMouseY() >= vector.Y && (float)getOldMouseX() < vector.X + 32f)
					{
						drawWithBorder(Math.Max(0, player.health) + "/" + player.maxHealth, Color.Black * 0f, Color.Red, vector + new Vector2(0f - dialogueFont.MeasureString("999/999").X - 32f, 64f));
					}
					destinationRectangle2.Height = 4;
					spriteBatch.Draw(staminaRect, destinationRectangle2, staminaRect.Bounds, redToGreenLerpColor, 0f, Vector2.Zero, SpriteEffects.None, 1f);
				}
				else
				{
					showingHealth = false;
				}
				_ = player.ActiveObject;
				foreach (IClickableMenu onScreenMenu in onScreenMenus)
				{
					if (onScreenMenu != chatBox)
					{
						onScreenMenu.update(currentGameTime);
						onScreenMenu.draw(spriteBatch);
					}
				}
				if (!player.professions.Contains(17) || !currentLocation.IsOutdoors)
				{
					return;
				}
				foreach (KeyValuePair<Vector2, Object> pair in currentLocation.objects.Pairs)
				{
					if (((bool)pair.Value.isSpawnedObject || pair.Value.ParentSheetIndex == 590) && !Utility.isOnScreen(pair.Key * 64f + new Vector2(32f, 32f), 64))
					{
						Microsoft.Xna.Framework.Rectangle bounds = graphics.GraphicsDevice.Viewport.Bounds;
						Vector2 renderPos = default(Vector2);
						float num5 = 0f;
						if (pair.Key.X * 64f > (float)(viewport.MaxCorner.X - 64))
						{
							renderPos.X = bounds.Right - 8;
							num5 = (float)Math.PI / 2f;
						}
						else if (pair.Key.X * 64f < (float)viewport.X)
						{
							renderPos.X = 8f;
							num5 = -(float)Math.PI / 2f;
						}
						else
						{
							renderPos.X = pair.Key.X * 64f - (float)viewport.X;
						}
						if (pair.Key.Y * 64f > (float)(viewport.MaxCorner.Y - 64))
						{
							renderPos.Y = bounds.Bottom - 8;
							num5 = (float)Math.PI;
						}
						else if (pair.Key.Y * 64f < (float)viewport.Y)
						{
							renderPos.Y = 8f;
						}
						else
						{
							renderPos.Y = pair.Key.Y * 64f - (float)viewport.Y;
						}
						if (renderPos.X == 8f && renderPos.Y == 8f)
						{
							num5 += (float)Math.PI / 4f;
						}
						if (renderPos.X == 8f && renderPos.Y == (float)(bounds.Bottom - 8))
						{
							num5 += (float)Math.PI / 4f;
						}
						if (renderPos.X == (float)(bounds.Right - 8) && renderPos.Y == 8f)
						{
							num5 -= (float)Math.PI / 4f;
						}
						if (renderPos.X == (float)(bounds.Right - 8) && renderPos.Y == (float)(bounds.Bottom - 8))
						{
							num5 -= (float)Math.PI / 4f;
						}
						Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(412, 495, 5, 4);
						float num6 = 4f;
						Vector2 position = Utility.makeSafe(renderSize: new Vector2((float)value.Width * num6, (float)value.Height * num6), renderPos: renderPos);
						spriteBatch.Draw(mouseCursors, position, value, Color.White, num5, new Vector2(2f, 2f), num6, SpriteEffects.None, 1f);
					}
				}
				if (!currentLocation.orePanPoint.Equals(Point.Zero) && !Utility.isOnScreen(Utility.PointToVector2(currentLocation.orePanPoint) * 64f + new Vector2(32f, 32f), 64))
				{
					Vector2 position2 = default(Vector2);
					float num7 = 0f;
					if (currentLocation.orePanPoint.X * 64 > viewport.MaxCorner.X - 64)
					{
						position2.X = graphics.GraphicsDevice.Viewport.Bounds.Right - 8;
						num7 = (float)Math.PI / 2f;
					}
					else if (currentLocation.orePanPoint.X * 64 < viewport.X)
					{
						position2.X = 8f;
						num7 = -(float)Math.PI / 2f;
					}
					else
					{
						position2.X = currentLocation.orePanPoint.X * 64 - viewport.X;
					}
					if (currentLocation.orePanPoint.Y * 64 > viewport.MaxCorner.Y - 64)
					{
						position2.Y = graphics.GraphicsDevice.Viewport.Bounds.Bottom - 8;
						num7 = (float)Math.PI;
					}
					else if (currentLocation.orePanPoint.Y * 64 < viewport.Y)
					{
						position2.Y = 8f;
					}
					else
					{
						position2.Y = currentLocation.orePanPoint.Y * 64 - viewport.Y;
					}
					if (position2.X == 8f && position2.Y == 8f)
					{
						num7 += (float)Math.PI / 4f;
					}
					if (position2.X == 8f && position2.Y == (float)(graphics.GraphicsDevice.Viewport.Bounds.Bottom - 8))
					{
						num7 += (float)Math.PI / 4f;
					}
					if (position2.X == (float)(graphics.GraphicsDevice.Viewport.Bounds.Right - 8) && position2.Y == 8f)
					{
						num7 -= (float)Math.PI / 4f;
					}
					if (position2.X == (float)(graphics.GraphicsDevice.Viewport.Bounds.Right - 8) && position2.Y == (float)(graphics.GraphicsDevice.Viewport.Bounds.Bottom - 8))
					{
						num7 -= (float)Math.PI / 4f;
					}
					spriteBatch.Draw(mouseCursors, position2, new Microsoft.Xna.Framework.Rectangle(412, 495, 5, 4), Color.Cyan, num7, new Vector2(2f, 2f), 4f, SpriteEffects.None, 1f);
				}
				return;
			}
			foreach (IClickableMenu onScreenMenu2 in onScreenMenus)
			{
				if (onScreenMenu2 is DayTimeMoneyBox dayTimeMoneyBox)
				{
					dayTimeMoneyBox.update(currentGameTime);
					dayTimeMoneyBox.drawJustTheGameMenuButton(spriteBatch);
				}
			}
		}

		public static void InvalidateOldMouseMovement()
		{
			MouseState mouseState = input.GetMouseState();
			oldMouseState = new MouseState(mouseState.X, mouseState.Y, oldMouseState.ScrollWheelValue, oldMouseState.LeftButton, oldMouseState.MiddleButton, oldMouseState.RightButton, oldMouseState.XButton1, oldMouseState.XButton2);
		}

		public static bool IsRenderingNonNativeUIScale()
		{
			return options.uiScale != options.zoomLevel;
		}

		public virtual void drawMouseCursor()
		{
			if (activeClickableMenu == null && timerUntilMouseFade > 0)
			{
				timerUntilMouseFade -= currentGameTime.ElapsedGameTime.Milliseconds;
				lastMousePositionBeforeFade = getMousePosition();
			}
			if (options.gamepadControls && timerUntilMouseFade <= 0 && activeClickableMenu == null && (emoteMenu == null || emoteMenu.gamepadMode))
			{
				mouseCursorTransparency = 0f;
			}
			if (activeClickableMenu == null && mouseCursor > -1 && currentLocation != null)
			{
				if (IsRenderingNonNativeUIScale())
				{
					spriteBatch.End();
					PopUIMode();
					if (ShouldDrawOnBuffer())
					{
						SetRenderTarget(screen);
					}
					else
					{
						SetRenderTarget(null);
					}
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				}
				if ((!(mouseCursorTransparency > 0f) || !Utility.canGrabSomethingFromHere(getOldMouseX() + viewport.X, getOldMouseY() + viewport.Y, player) || mouseCursor == 3) && (player.ActiveObject == null || mouseCursor == 3 || eventUp || currentMinigame != null || player.isRidingHorse() || !player.CanMove || !displayFarmer))
				{
					if (mouseCursor == 0 && isActionAtCurrentCursorTile && currentMinigame == null)
					{
						mouseCursor = (isSpeechAtCurrentCursorTile ? 4 : (isInspectionAtCurrentCursorTile ? 5 : 2));
					}
					else if (mouseCursorTransparency > 0f)
					{
						NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> netLongDictionary = null;
						if (currentLocation is Farm)
						{
							netLongDictionary = (currentLocation as Farm).animals;
						}
						if (currentLocation is AnimalHouse)
						{
							netLongDictionary = (currentLocation as AnimalHouse).animals;
						}
						if (netLongDictionary != null)
						{
							Vector2 target = new Vector2(getOldMouseX() + uiViewport.X, getOldMouseY() + uiViewport.Y);
							int generalDirectionTowards = player.getGeneralDirectionTowards(target);
							bool flag = Utility.withinRadiusOfPlayer((int)target.X, (int)target.Y, 1, player);
							foreach (KeyValuePair<long, FarmAnimal> pair in netLongDictionary.Pairs)
							{
								Microsoft.Xna.Framework.Rectangle cursorPetBoundingBox = pair.Value.GetCursorPetBoundingBox();
								if (!pair.Value.wasPet && cursorPetBoundingBox.Contains((int)target.X, (int)target.Y))
								{
									mouseCursor = 2;
									if (!flag)
									{
										mouseCursorTransparency = 0.5f;
									}
									break;
								}
							}
						}
					}
				}
				if (IsRenderingNonNativeUIScale())
				{
					spriteBatch.End();
					PushUIMode();
					SetRenderTarget(uiScreen);
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				}
				if (currentMinigame != null)
				{
					mouseCursor = 0;
				}
				wasMouseVisibleThisFrame = mouseCursorTransparency > 0f;
				_lastDrewMouseCursor = wasMouseVisibleThisFrame;
			}
			mouseCursor = 0;
			if (!isActionAtCurrentCursorTile && activeClickableMenu == null)
			{
				mouseCursorTransparency = 1f;
			}
		}

		public static void panScreen(int x, int y, int yOffset = 0)
		{
			int num = uiModeCount;
			while (uiModeCount > 0)
			{
				PopUIMode();
			}
			previousViewportPosition.X = viewport.Location.X;
			previousViewportPosition.Y = viewport.Location.Y;
			viewport.X += x;
			viewport.Y += y;
			clampViewportToGameMap(yOffset);
			updateRaindropPosition();
			for (int i = 0; i < num; i++)
			{
				PushUIMode();
			}
		}

		public static void clampViewportToGameMap(int yOffset = 0)
		{
			if (viewport.X < 0)
			{
				viewport.X = 0;
			}
			if (viewport.X > currentLocation.map.DisplayWidth - viewport.Width)
			{
				viewport.X = currentLocation.map.DisplayWidth - viewport.Width;
			}
			if (viewport.Y < 0)
			{
				viewport.Y = 0;
			}
			if (viewport.Y > currentLocation.map.DisplayHeight - viewport.Height + yOffset)
			{
				viewport.Y = currentLocation.map.DisplayHeight - viewport.Height + yOffset;
			}
		}

		public static Vector2 ClampViewportCornerToGameMap(Vector2 viewport_corner)
		{
			if (viewport_corner.X < 0f)
			{
				viewport_corner.X = 0f;
			}
			if (viewport_corner.X > (float)(currentLocation.map.DisplayWidth - viewport.Width))
			{
				viewport_corner.X = currentLocation.map.DisplayWidth - viewport.Width;
			}
			if (viewport_corner.Y < 0f)
			{
				viewport_corner.Y = 0f;
			}
			if (viewport_corner.Y > (float)(currentLocation.map.DisplayHeight - viewport.Height))
			{
				viewport_corner.Y = currentLocation.map.DisplayHeight - viewport.Height;
			}
			return viewport_corner;
		}

		public void drawBillboard()
		{
		}

		protected void drawDialogueBox()
		{
			int num = 320;
			if (currentSpeaker != null)
			{
				num = (int)dialogueFont.MeasureString(currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue()).Y;
				num = Math.Max(num, 320);
				int x = xEdge;
				int y = base.GraphicsDevice.Viewport.Height - num;
				int width = base.GraphicsDevice.Viewport.Width - 2 * xEdge;
				drawDialogueBox(x, y, width, num, speaker: true, drawOnlyBox: false, null, objectDialoguePortraitPerson != null && currentSpeaker == null);
			}
			else
			{
				_ = currentObjectDialogue.Count;
				_ = 0;
			}
		}

		public static void drawDialogueBox(string message)
		{
			drawDialogueBox(viewport.Width / 2, viewport.Height / 2, speaker: false, drawOnlyBox: false, message);
		}

		public static void drawDialogueBox(int centerX, int centerY, bool speaker, bool drawOnlyBox, string message)
		{
			string text = null;
			if (speaker && currentSpeaker != null)
			{
				text = currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue();
			}
			else if (message != null)
			{
				text = message;
			}
			else if (currentObjectDialogue.Count > 0)
			{
				text = currentObjectDialogue.Peek();
			}
			if (text != null)
			{
				Vector2 vector = dialogueFont.MeasureString(text);
				int num = (int)vector.X + 128;
				int num2 = (int)vector.Y + 128;
				int x = centerX - num / 2;
				int y = centerY - num2 / 2;
				drawDialogueBox(x, y, num, num2, speaker, drawOnlyBox, message, objectDialoguePortraitPerson != null && !speaker);
			}
		}

		public static void DrawBox(int x, int y, int width, int height, Color? color = null)
		{
			Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64);
			value.X = 64;
			value.Y = 128;
			Texture2D texture = menuTexture;
			Color color2 = Color.White;
			Color color3 = Color.White;
			if (color.HasValue)
			{
				color2 = color.Value;
				texture = uncoloredMenuTexture;
				color3 = new Color((int)Utility.Lerp((int)color2.R, Math.Min(255, color2.R + 150), 0.65f), (int)Utility.Lerp((int)color2.G, Math.Min(255, color2.G + 150), 0.65f), (int)Utility.Lerp((int)color2.B, Math.Min(255, color2.B + 150), 0.65f));
			}
			spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x, y, width, height), value, color3);
			value.Y = 0;
			Vector2 vector = new Vector2((float)(-value.Width) * 0.5f, (float)(-value.Height) * 0.5f);
			value.X = 0;
			spriteBatch.Draw(texture, new Vector2((float)x + vector.X, (float)y + vector.Y), value, color2);
			value.X = 192;
			spriteBatch.Draw(texture, new Vector2((float)x + vector.X + (float)width, (float)y + vector.Y), value, color2);
			value.Y = 192;
			spriteBatch.Draw(texture, new Vector2((float)(x + width) + vector.X, (float)(y + height) + vector.Y), value, color2);
			value.X = 0;
			spriteBatch.Draw(texture, new Vector2((float)x + vector.X, (float)(y + height) + vector.Y), value, color2);
			value.X = 128;
			value.Y = 0;
			spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(64 + x + (int)vector.X, y + (int)vector.Y, width - 64, 64), value, color2);
			value.Y = 192;
			spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(64 + x + (int)vector.X, y + (int)vector.Y + height, width - 64, 64), value, color2);
			value.Y = 128;
			value.X = 0;
			spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x + (int)vector.X, y + (int)vector.Y + 64, 64, height - 64), value, color2);
			value.X = 192;
			spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x + width + (int)vector.X, y + (int)vector.Y + 64, 64, height - 64), value, color2);
		}

		public static void drawDialogueBox(int x, int y, int width, int height, bool speaker, bool drawOnlyBox, string message = null, bool objectDialogueWithPortrait = false, bool ignoreTitleSafe = false, int r = -1, int g = -1, int b = -1)
		{
			if (!drawOnlyBox)
			{
				return;
			}
			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(xEdge + 64, 32, graphics.GraphicsDevice.Viewport.Width - 2 * xEdge - 128, graphics.GraphicsDevice.Viewport.Height - 64);
			int height2 = rectangle.Height;
			int width2 = rectangle.Width;
			int num = 0;
			int num2 = 0;
			if (!ignoreTitleSafe)
			{
				num2 = ((y <= rectangle.Y) ? (rectangle.Y - y) : 0);
			}
			int num3 = 0;
			if (!ignoreTitleSafe)
			{
				width = Math.Min(rectangle.Width, width);
			}
			if (!isQuestion && currentSpeaker == null && currentObjectDialogue.Count > 0 && !drawOnlyBox)
			{
				width = (int)dialogueFont.MeasureString(currentObjectDialogue.Peek()).X + 128;
				height = (int)dialogueFont.MeasureString(currentObjectDialogue.Peek()).Y + 64;
				x = width2 / 2 - width / 2;
				num3 = ((height > 256) ? (-(height - 256)) : 0);
			}
			Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64);
			int num4 = -1;
			if (questionChoices.Count >= 3)
			{
				num4 = questionChoices.Count - 3;
			}
			if (!drawOnlyBox && currentObjectDialogue.Count > 0)
			{
				if (dialogueFont.MeasureString(currentObjectDialogue.Peek()).Y >= (float)(height - 128))
				{
					num4 -= (int)(((float)(height - 128) - dialogueFont.MeasureString(currentObjectDialogue.Peek()).Y) / 64f) - 1;
				}
				else
				{
					height += (int)dialogueFont.MeasureString(currentObjectDialogue.Peek()).Y / 2;
					num3 -= (int)dialogueFont.MeasureString(currentObjectDialogue.Peek()).Y / 2;
					if ((int)dialogueFont.MeasureString(currentObjectDialogue.Peek()).Y / 2 > 64)
					{
						num4 = 0;
					}
				}
			}
			if (currentSpeaker != null && isQuestion && currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue().Substring(0, currentDialogueCharacterIndex)
				.Contains(Environment.NewLine))
			{
				num4++;
			}
			value.Width = 64;
			value.Height = 64;
			value.X = 64;
			value.Y = 128;
			Color color = ((r == -1) ? Color.White : new Color(r, g, b));
			Texture2D texture = ((r == -1) ? menuTexture : uncoloredMenuTexture);
			spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(28 + x + num, 28 + y - 64 * num4 + num2 + num3, width - 64, height - 64 + num4 * 64), value, (r == -1) ? color : new Color((int)Utility.Lerp(r, Math.Min(255, r + 150), 0.65f), (int)Utility.Lerp(g, Math.Min(255, g + 150), 0.65f), (int)Utility.Lerp(b, Math.Min(255, b + 150), 0.65f)));
			value.Y = 0;
			value.X = 0;
			spriteBatch.Draw(texture, new Vector2(x + num, y - 64 * num4 + num2 + num3), value, color);
			value.X = 192;
			spriteBatch.Draw(texture, new Vector2(x + width + num - 64, y - 64 * num4 + num2 + num3), value, color);
			value.Y = 192;
			spriteBatch.Draw(texture, new Vector2(x + width + num - 64, y + height + num2 - 64 + num3), value, color);
			value.X = 0;
			spriteBatch.Draw(texture, new Vector2(x + num, y + height + num2 - 64 + num3), value, color);
			value.X = 128;
			value.Y = 0;
			spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(64 + x + num, y - 64 * num4 + num2 + num3, width - 128, 64), value, color);
			value.Y = 192;
			spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(64 + x + num, y + height + num2 - 64 + num3, width - 128, 64), value, color);
			value.Y = 128;
			value.X = 0;
			spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x + num, y - 64 * num4 + num2 + 64 + num3, 64, height - 128 + num4 * 64), value, color);
			value.X = 192;
			spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x + width + num - 64, y - 64 * num4 + num2 + 64 + num3, 64, height - 128 + num4 * 64), value, color);
			NPC nPC;
			string text;
			Microsoft.Xna.Framework.Rectangle rectangle2;
			if ((objectDialogueWithPortrait && objectDialoguePortraitPerson != null) || (speaker && currentSpeaker != null && currentSpeaker.CurrentDialogue.Count > 0 && currentSpeaker.CurrentDialogue.Peek().showPortrait))
			{
				rectangle2 = new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64);
				nPC = (objectDialogueWithPortrait ? objectDialoguePortraitPerson : currentSpeaker);
				text = ((!objectDialogueWithPortrait) ? nPC.CurrentDialogue.Peek().CurrentEmotion : (objectDialoguePortraitPerson.Name.Equals(player.spouse) ? "$l" : "$neutral"));
				if (text != null)
				{
					int length = text.Length;
					if (length != 2)
					{
						if (length == 8 && text == "$neutral")
						{
							goto IL_0753;
						}
					}
					else
					{
						switch (text[1])
						{
						case 'h':
							break;
						case 's':
							goto IL_068c;
						case 'u':
							goto IL_069f;
						case 'l':
							goto IL_06b2;
						case 'a':
							goto IL_06c5;
						case 'k':
							goto IL_06d8;
						default:
							goto IL_0762;
						}
						if (text == "$h")
						{
							rectangle2 = new Microsoft.Xna.Framework.Rectangle(64, 0, 64, 64);
							goto IL_078e;
						}
					}
				}
				goto IL_0762;
			}
			goto IL_0a79;
			IL_078e:
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp);
			if (nPC.Portrait != null)
			{
				spriteBatch.Draw(mouseCursors, new Vector2(num + x + 768, height2 - 320 - 64 * num4 - 256 + num2 + 16 - 60 + num3), new Microsoft.Xna.Framework.Rectangle(333, 305, 80, 87), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.98f);
				spriteBatch.Draw(nPC.Portrait, new Vector2(num + x + 768 + 32, height2 - 320 - 64 * num4 - 256 + num2 + 16 - 60 + num3), rectangle2, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			}
			spriteBatch.End();
			spriteBatch.Begin();
			if (isQuestion)
			{
				spriteBatch.DrawString(dialogueFont, nPC.displayName, new Vector2(928f - dialogueFont.MeasureString(nPC.displayName).X / 2f + (float)num + (float)x, (float)(height2 - 320 - 64 * num4) - dialogueFont.MeasureString(nPC.displayName).Y + (float)num2 + 21f + (float)num3) + new Vector2(2f, 2f), new Color(150, 150, 150));
			}
			spriteBatch.DrawString(dialogueFont, nPC.Name.Equals("DwarfKing") ? content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3754") : (nPC.Name.Equals("Lewis") ? content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3756") : nPC.displayName), new Vector2((float)(num + x + 896 + 32) - dialogueFont.MeasureString(nPC.Name.Equals("Lewis") ? content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3756") : nPC.displayName).X / 2f, (float)(height2 - 320 - 64 * num4) - dialogueFont.MeasureString(nPC.Name.Equals("Lewis") ? content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3756") : nPC.displayName).Y + (float)num2 + 21f + 8f + (float)num3), textColor);
			goto IL_0a79;
			IL_068c:
			if (!(text == "$s"))
			{
				goto IL_0762;
			}
			rectangle2 = new Microsoft.Xna.Framework.Rectangle(0, 64, 64, 64);
			goto IL_078e;
			IL_0a79:
			if (drawOnlyBox || (nameSelectUp && (!messagePause || currentObjectDialogue == null)))
			{
				return;
			}
			string text2 = "";
			if (currentSpeaker != null && currentSpeaker.CurrentDialogue.Count > 0)
			{
				if (currentSpeaker.CurrentDialogue.Peek() == null || currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue().Length < currentDialogueCharacterIndex - 1)
				{
					dialogueUp = false;
					currentDialogueCharacterIndex = 0;
					playSound("dialogueCharacterClose");
					player.forceCanMove();
					return;
				}
				text2 = currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue().Substring(0, currentDialogueCharacterIndex);
			}
			else if (message != null)
			{
				text2 = message;
			}
			else if (currentObjectDialogue.Count > 0)
			{
				text2 = ((currentObjectDialogue.Peek().Length <= 1) ? "" : currentObjectDialogue.Peek().Substring(0, currentDialogueCharacterIndex));
			}
			Vector2 vector = ((dialogueFont.MeasureString(text2).X > (float)(width2 - 256 - num)) ? new Vector2(128 + num, height2 - 64 * num4 - 256 - 16 + num2 + num3) : ((currentSpeaker != null && currentSpeaker.CurrentDialogue.Count > 0) ? new Vector2((float)(width2 / 2) - dialogueFont.MeasureString(currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue()).X / 2f + (float)num, height2 - 64 * num4 - 256 - 16 + num2 + num3) : ((message != null) ? new Vector2((float)(width2 / 2) - dialogueFont.MeasureString(text2).X / 2f + (float)num, y + 96 + 4) : ((!isQuestion) ? new Vector2((float)(width2 / 2) - dialogueFont.MeasureString((currentObjectDialogue.Count == 0) ? "" : currentObjectDialogue.Peek()).X / 2f + (float)num, y + 4 + num3) : new Vector2((float)(width2 / 2) - dialogueFont.MeasureString((currentObjectDialogue.Count == 0) ? "" : currentObjectDialogue.Peek()).X / 2f + (float)num, height2 - 64 * num4 - 256 - (16 + (questionChoices.Count - 2) * 64) + num2 + num3)))));
			if (!drawOnlyBox)
			{
				spriteBatch.DrawString(dialogueFont, text2, vector + new Vector2(3f, 0f), textShadowColor);
				spriteBatch.DrawString(dialogueFont, text2, vector + new Vector2(3f, 3f), textShadowColor);
				spriteBatch.DrawString(dialogueFont, text2, vector + new Vector2(0f, 3f), textShadowColor);
				spriteBatch.DrawString(dialogueFont, text2, vector, textColor);
			}
			if (dialogueFont.MeasureString(text2).Y <= 64f)
			{
				num2 += 64;
			}
			if (isQuestion && !dialogueTyping)
			{
				for (int i = 0; i < questionChoices.Count; i++)
				{
					if (currentQuestionChoice == i)
					{
						vector.X = 80 + num + x;
						vector.Y = (float)(height2 - (5 + num4 + 1) * 64) + ((text2.Trim().Length > 0) ? dialogueFont.MeasureString(text2).Y : 0f) + 128f + (float)(48 * i) - (float)(16 + (questionChoices.Count - 2) * 64) + (float)num2 + (float)num3;
						spriteBatch.End();
						spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp);
						spriteBatch.Draw(objectSpriteSheet, vector + new Vector2((float)Math.Cos((double)currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) * 3f, 0f), GameLocation.getSourceRectForObject(26), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
						spriteBatch.End();
						spriteBatch.Begin();
						vector.X = 160 + num + x;
						vector.Y = (float)(height2 - (5 + num4 + 1) * 64) + ((text2.Trim().Length > 1) ? dialogueFont.MeasureString(text2).Y : 0f) + 128f - (float)((questionChoices.Count - 2) * 64) + (float)(48 * i) + (float)num2 + (float)num3;
						spriteBatch.DrawString(dialogueFont, questionChoices[i].responseText, vector, textColor);
					}
					else
					{
						vector.X = 128 + num + x;
						vector.Y = (float)(height2 - (5 + num4 + 1) * 64) + ((text2.Trim().Length > 1) ? dialogueFont.MeasureString(text2).Y : 0f) + 128f - (float)((questionChoices.Count - 2) * 64) + (float)(48 * i) + (float)num2 + (float)num3;
						spriteBatch.DrawString(dialogueFont, questionChoices[i].responseText, vector, unselectedOptionColor);
					}
				}
			}
			else if (numberOfSelectedItems != -1 && !dialogueTyping)
			{
				drawItemSelectDialogue(x, y, num, num2 + num3, height2, num4, text2);
			}
			if (!drawOnlyBox && !dialogueTyping && message == null)
			{
				spriteBatch.Draw(mouseCursors, new Vector2(x + num + width - 96, (float)(y + height + num2 + num3 - 96) - dialogueButtonScale), getSourceRectForStandardTileSheet(mouseCursors, (!dialogueButtonShrinking && dialogueButtonScale < 8f) ? 3 : 2), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
			}
			return;
			IL_0762:
			rectangle2 = getSourceRectForStandardTileSheet(nPC.Portrait, Convert.ToInt32(nPC.CurrentDialogue.Peek().CurrentEmotion.Substring(1)));
			goto IL_078e;
			IL_06b2:
			if (!(text == "$l"))
			{
				goto IL_0762;
			}
			rectangle2 = new Microsoft.Xna.Framework.Rectangle(0, 128, 64, 64);
			goto IL_078e;
			IL_0753:
			rectangle2 = new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64);
			goto IL_078e;
			IL_069f:
			if (!(text == "$u"))
			{
				goto IL_0762;
			}
			rectangle2 = new Microsoft.Xna.Framework.Rectangle(64, 64, 64, 64);
			goto IL_078e;
			IL_06c5:
			if (!(text == "$a"))
			{
				goto IL_0762;
			}
			rectangle2 = new Microsoft.Xna.Framework.Rectangle(64, 128, 64, 64);
			goto IL_078e;
			IL_06d8:
			if (text == "$k")
			{
				goto IL_0753;
			}
			goto IL_0762;
		}

		private static void drawItemSelectDialogue(int x, int y, int dialogueX, int dialogueY, int screenHeight, int addedTileHeightForQuestions, string text)
		{
			string text2 = "";
			switch (selectedItemsType)
			{
			case "flutePitch":
			case "drumTome":
				text2 = "@ " + numberOfSelectedItems + " >  ";
				break;
			case "jukebox":
				text2 = "@ " + player.songsHeard.ElementAt(numberOfSelectedItems) + " >  ";
				break;
			default:
				text2 = "@ " + numberOfSelectedItems + " >  " + priceOfSelectedItem * numberOfSelectedItems + "g";
				break;
			}
			if (currentLocation.Name.Equals("Club"))
			{
				text2 = "@ " + numberOfSelectedItems + " >  ";
			}
			spriteBatch.DrawString(dialogueFont, text2, new Vector2(dialogueX + x + 64, (float)(screenHeight - (5 + addedTileHeightForQuestions + 1) * 64) + dialogueFont.MeasureString(text).Y + 104f + (float)dialogueY), textColor);
		}

		protected void drawFarmBuildings()
		{
			_ = player.CoopUpgradeLevel;
			_ = 0;
			switch (player.BarnUpgradeLevel)
			{
			case 1:
				spriteBatch.Draw(currentBarnTexture, GlobalToLocal(viewport, new Vector2(768f, 320f)), currentBarnTexture.Bounds, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, Math.Max(0f, 0.0576f));
				break;
			case 2:
				spriteBatch.Draw(currentBarnTexture, GlobalToLocal(viewport, new Vector2(640f, 256f)), currentBarnTexture.Bounds, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, Math.Max(0f, 0.0576f));
				break;
			}
			if (player.hasGreenhouse)
			{
				spriteBatch.Draw(greenhouseTexture, GlobalToLocal(viewport, new Vector2(64f, 320f)), greenhouseTexture.Bounds, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, Math.Max(0f, 0.0576f));
			}
		}

		public static void drawPlayerHeldObject(Farmer f)
		{
			if ((!eventUp || (currentLocation.currentEvent != null && currentLocation.currentEvent.showActiveObject)) && !f.FarmerSprite.PauseForSingleAnimation && !f.isRidingHorse() && !f.bathingClothes && !f.onBridge.Value)
			{
				float num = f.getLocalPosition(viewport).X + (float)((f.rotation < 0f) ? (-8) : ((f.rotation > 0f) ? 8 : 0)) + (float)(f.FarmerSprite.CurrentAnimationFrame.xOffset * 4);
				float num2 = f.getLocalPosition(viewport).Y - 128f + (float)(f.FarmerSprite.CurrentAnimationFrame.positionOffset * 4) + (float)(FarmerRenderer.featureYOffsetPerFrame[f.FarmerSprite.CurrentFrame] * 4);
				if ((bool)f.ActiveObject.bigCraftable)
				{
					num2 -= 64f;
				}
				if (f.isEating)
				{
					num = f.getLocalPosition(viewport).X - 21f;
					num2 = f.getLocalPosition(viewport).Y - 128f + 12f;
				}
				if (!f.isEating || (f.isEating && f.Sprite.currentFrame <= 218))
				{
					f.ActiveObject.drawWhenHeld(spriteBatch, new Vector2((int)num, (int)num2), f);
				}
			}
		}

		public static void drawTool(Farmer f)
		{
			drawTool(f, f.CurrentTool.CurrentParentTileIndex);
		}

		public static void drawTool(Farmer f, int currentToolIndex)
		{
			Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(currentToolIndex * 16 % toolSpriteSheet.Width, currentToolIndex * 16 / toolSpriteSheet.Width * 16, 16, 32);
			Vector2 playerPosition = f.getLocalPosition(viewport) + f.jitter + f.armOffset;
			float num = 0f;
			if (f.FacingDirection == 0)
			{
				num = -0.002f;
			}
			if (pickingTool)
			{
				int num2 = (int)playerPosition.Y - 128;
				spriteBatch.Draw(toolSpriteSheet, new Vector2(playerPosition.X, num2), value, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 32) / 10000f));
				return;
			}
			if (f.CurrentTool is MeleeWeapon)
			{
				((MeleeWeapon)f.CurrentTool).drawDuringUse(((FarmerSprite)f.Sprite).currentAnimationIndex, f.FacingDirection, spriteBatch, playerPosition, f);
				return;
			}
			if (f.FarmerSprite.isUsingWeapon())
			{
				MeleeWeapon.drawDuringUse(((FarmerSprite)f.Sprite).currentAnimationIndex, f.FacingDirection, spriteBatch, playerPosition, f, MeleeWeapon.getSourceRect(f.FarmerSprite.CurrentToolIndex), f.FarmerSprite.getWeaponTypeFromAnimation(), isOnSpecial: false);
				return;
			}
			if (f.CurrentTool is FishingRod)
			{
				if ((f.CurrentTool as FishingRod).fishCaught || (f.CurrentTool as FishingRod).showingTreasure)
				{
					f.CurrentTool.draw(spriteBatch);
					return;
				}
				value = new Microsoft.Xna.Framework.Rectangle(((FarmerSprite)f.Sprite).currentAnimationIndex * 48, 288, 48, 48);
				if (f.FacingDirection == 2 || f.FacingDirection == 0)
				{
					value.Y += 48;
				}
				else if ((f.CurrentTool as FishingRod).isFishing && (!(f.CurrentTool as FishingRod).isReeling || (f.CurrentTool as FishingRod).hit))
				{
					playerPosition.Y += 8f;
				}
				if ((f.CurrentTool as FishingRod).isFishing)
				{
					value.X += (5 - ((FarmerSprite)f.Sprite).currentAnimationIndex) * 48;
				}
				if ((f.CurrentTool as FishingRod).isReeling)
				{
					if (f.FacingDirection == 2 || f.FacingDirection == 0)
					{
						value.X = 288;
						if (f.IsLocalPlayer && didPlayerJustClickAtAll())
						{
							value.X = 0;
						}
					}
					else
					{
						value.X = 288;
						value.Y = 240;
						if (f.IsLocalPlayer && didPlayerJustClickAtAll())
						{
							value.Y += 48;
						}
					}
				}
				if (f.FarmerSprite.CurrentFrame == 57)
				{
					value.Height = 0;
				}
				if (f.FacingDirection == 0)
				{
					playerPosition.X += 16f;
				}
			}
			if (f.CurrentTool != null)
			{
				f.CurrentTool.draw(spriteBatch);
			}
			if (f.CurrentTool is Slingshot || f.CurrentTool is Shears || f.CurrentTool is MilkPail || f.CurrentTool is Pan)
			{
				return;
			}
			int num3 = 0;
			int num4 = 0;
			if (f.CurrentTool is WateringCan)
			{
				num3 += 80;
				num4 = ((f.FacingDirection == 1) ? 32 : ((f.FacingDirection == 3) ? (-32) : 0));
				if (((FarmerSprite)f.Sprite).currentAnimationIndex == 0 || ((FarmerSprite)f.Sprite).currentAnimationIndex == 1)
				{
					num4 = num4 * 3 / 2;
				}
			}
			if (f.FacingDirection == 1)
			{
				int num5 = 0;
				if (((FarmerSprite)f.Sprite).currentAnimationIndex > 2)
				{
					Point tileLocationPoint = f.getTileLocationPoint();
					tileLocationPoint.X++;
					tileLocationPoint.Y--;
					if (!(f.CurrentTool is WateringCan) && f.currentLocation.getTileIndexAt(tileLocationPoint, "Front") != -1)
					{
						return;
					}
					tileLocationPoint.Y++;
					if (f.currentLocation.getTileIndexAt(tileLocationPoint, "Front") == -1)
					{
						num5 += 16;
					}
				}
				else if (f.CurrentTool is WateringCan && ((FarmerSprite)f.Sprite).currentAnimationIndex == 1)
				{
					Point tileLocationPoint2 = f.getTileLocationPoint();
					tileLocationPoint2.X--;
					tileLocationPoint2.Y--;
					if (f.currentLocation.getTileIndexAt(tileLocationPoint2, "Front") != -1 && f.Position.Y % 64f < 32f)
					{
						return;
					}
				}
				if (f.CurrentTool != null && f.CurrentTool is FishingRod)
				{
					Color color = (f.CurrentTool as FishingRod).getColor();
					switch (((FarmerSprite)f.Sprite).currentAnimationIndex)
					{
					case 0:
						if ((f.CurrentTool as FishingRod).isReeling)
						{
							spriteBatch.Draw(toolSpriteSheet, new Vector2(playerPosition.X - 64f + (float)num4, playerPosition.Y - 160f + (float)num3), value, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						}
						else if ((f.CurrentTool as FishingRod).isFishing || (f.CurrentTool as FishingRod).doneWithAnimation)
						{
							spriteBatch.Draw(toolSpriteSheet, new Vector2(playerPosition.X - 64f + (float)num4, playerPosition.Y - 160f + (float)num3), value, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						}
						else if (!(f.CurrentTool as FishingRod).hasDoneFucntionYet || (f.CurrentTool as FishingRod).pullingOutOfWater)
						{
							spriteBatch.Draw(toolSpriteSheet, new Vector2(playerPosition.X - 64f + (float)num4, playerPosition.Y - 160f + (float)num3), value, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						}
						break;
					case 1:
						spriteBatch.Draw(toolSpriteSheet, new Vector2(playerPosition.X - 64f + (float)num4, playerPosition.Y - 160f + 8f + (float)num3), value, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 2:
						spriteBatch.Draw(toolSpriteSheet, new Vector2(playerPosition.X - 96f + 32f + (float)num4, playerPosition.Y - 128f - 24f + (float)num3), value, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 3:
						spriteBatch.Draw(toolSpriteSheet, new Vector2(playerPosition.X - 96f + 24f + (float)num4, playerPosition.Y - 128f - 32f + (float)num3), value, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 4:
						if ((f.CurrentTool as FishingRod).isFishing || (f.CurrentTool as FishingRod).doneWithAnimation)
						{
							spriteBatch.Draw(toolSpriteSheet, new Vector2(playerPosition.X - 64f + (float)num4, playerPosition.Y - 160f + (float)num3), value, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						}
						else
						{
							spriteBatch.Draw(toolSpriteSheet, new Vector2(playerPosition.X - 64f + (float)num4, playerPosition.Y - 160f + 4f + (float)num3), value, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						}
						break;
					case 5:
						spriteBatch.Draw(toolSpriteSheet, new Vector2(playerPosition.X - 64f + (float)num4, playerPosition.Y - 160f + (float)num3), value, color, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						break;
					}
				}
				else if (f.CurrentTool != null && f.CurrentTool.Name.Contains("Sword"))
				{
					switch (((FarmerSprite)f.Sprite).currentAnimationIndex)
					{
					case 0:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + 64f - 20f, playerPosition.Y + 28f)), value, Color.White, -(float)Math.PI / 8f, new Vector2(4f, 60f), 1f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 1:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + 64f - 12f, playerPosition.Y + 64f - 8f)), value, Color.White, 0f, new Vector2(4f, 60f), 1f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 2:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + 64f - 12f, playerPosition.Y + 64f - 4f)), value, Color.White, (float)Math.PI / 8f, new Vector2(4f, 60f), 1f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 3:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + 64f - 12f, playerPosition.Y + 64f)), value, Color.White, 0.7853981f, new Vector2(4f, 60f), 1f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 4:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + 64f - 16f, playerPosition.Y + 64f + 4f)), value, Color.White, (float)Math.PI * 3f / 8f, new Vector2(4f, 60f), 1f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 5:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + 64f - 16f, playerPosition.Y + 64f + 8f)), value, Color.White, (float)Math.PI / 2f, new Vector2(4f, 60f), 1f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 6:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + 64f - 16f, playerPosition.Y + 64f + 12f)), value, Color.White, 1.9634954f, new Vector2(4f, 60f), 1f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 7:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + 64f - 16f, playerPosition.Y + 64f + 12f)), value, Color.White, 1.9634954f, new Vector2(4f, 60f), 1f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						break;
					}
				}
				else if (f.CurrentTool != null && f.CurrentTool is WateringCan)
				{
					switch (((FarmerSprite)f.Sprite).currentAnimationIndex)
					{
					case 0:
					case 1:
						spriteBatch.Draw(toolSpriteSheet, new Vector2((int)(playerPosition.X + (float)num4 - 4f), (int)(playerPosition.Y - 128f + 8f + (float)num3)), value, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.GetBoundingBox().Bottom + num5) / 10000f));
						break;
					case 2:
						spriteBatch.Draw(toolSpriteSheet, new Vector2((int)playerPosition.X + num4 + 24, (int)(playerPosition.Y - 128f - 8f + (float)num3)), value, Color.White, (float)Math.PI / 12f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.GetBoundingBox().Bottom + num5) / 10000f));
						break;
					case 3:
						value.X += 16;
						spriteBatch.Draw(toolSpriteSheet, new Vector2((int)(playerPosition.X + (float)num4 + 8f), (int)(playerPosition.Y - 128f - 24f + (float)num3)), value, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.GetBoundingBox().Bottom + num5) / 10000f));
						break;
					}
				}
				else
				{
					switch (((FarmerSprite)f.Sprite).currentAnimationIndex)
					{
					case 0:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 32f - 4f + (float)num4 - (float)Math.Min(8, f.toolPower * 4), playerPosition.Y - 128f + 24f + (float)num3 + (float)Math.Min(8, f.toolPower * 4))), value, Color.White, -(float)Math.PI / 12f - (float)Math.Min(f.toolPower, 2) * ((float)Math.PI / 64f), new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.GetBoundingBox().Bottom + num5) / 10000f));
						break;
					case 1:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + 32f - 24f + (float)num4, playerPosition.Y - 124f + (float)num3 + 64f)), value, Color.White, (float)Math.PI / 12f, new Vector2(0f, 32f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.GetBoundingBox().Bottom + num5) / 10000f));
						break;
					case 2:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + 32f + (float)num4 - 4f, playerPosition.Y - 132f + (float)num3 + 64f)), value, Color.White, (float)Math.PI / 4f, new Vector2(0f, 32f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.GetBoundingBox().Bottom + num5) / 10000f));
						break;
					case 3:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + 32f + 28f + (float)num4, playerPosition.Y - 64f + (float)num3)), value, Color.White, (float)Math.PI * 7f / 12f, new Vector2(0f, 32f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.GetBoundingBox().Bottom + num5) / 10000f));
						break;
					case 4:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + 32f + 28f + (float)num4, playerPosition.Y - 64f + 4f + (float)num3)), value, Color.White, (float)Math.PI * 7f / 12f, new Vector2(0f, 32f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.GetBoundingBox().Bottom + num5) / 10000f));
						break;
					case 5:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + 64f + 12f + (float)num4, playerPosition.Y - 128f + 32f + (float)num3 + 128f)), value, Color.White, (float)Math.PI / 4f, new Vector2(0f, 32f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.GetBoundingBox().Bottom + num5) / 10000f));
						break;
					case 6:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + 42f + 8f + (float)num4, playerPosition.Y - 64f + 24f + (float)num3 + 128f)), value, Color.White, 0f, new Vector2(0f, 128f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.GetBoundingBox().Bottom + num5) / 10000f));
						break;
					}
				}
			}
			else if (f.FacingDirection == 3)
			{
				int num6 = 0;
				if (((FarmerSprite)f.Sprite).currentAnimationIndex > 2)
				{
					Point tileLocationPoint3 = f.getTileLocationPoint();
					tileLocationPoint3.X--;
					tileLocationPoint3.Y--;
					if (!(f.CurrentTool is WateringCan) && f.currentLocation.getTileIndexAt(tileLocationPoint3, "Front") != -1 && f.Position.Y % 64f < 32f)
					{
						return;
					}
					tileLocationPoint3.Y++;
					if (f.currentLocation.getTileIndexAt(tileLocationPoint3, "Front") == -1)
					{
						num6 += 16;
					}
				}
				else if (f.CurrentTool is WateringCan && ((FarmerSprite)f.Sprite).currentAnimationIndex == 1)
				{
					Point tileLocationPoint4 = f.getTileLocationPoint();
					tileLocationPoint4.X--;
					tileLocationPoint4.Y--;
					if (f.currentLocation.getTileIndexAt(tileLocationPoint4, "Front") != -1 && f.Position.Y % 64f < 32f)
					{
						return;
					}
				}
				if (f.CurrentTool != null && f.CurrentTool is FishingRod)
				{
					Color color2 = (f.CurrentTool as FishingRod).getColor();
					switch (((FarmerSprite)f.Sprite).currentAnimationIndex)
					{
					case 0:
						if ((f.CurrentTool as FishingRod).isReeling)
						{
							spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 64f + (float)num4, playerPosition.Y - 160f + (float)num3)), value, color2, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						}
						else if ((f.CurrentTool as FishingRod).isFishing || (f.CurrentTool as FishingRod).doneWithAnimation)
						{
							spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 64f + (float)num4, playerPosition.Y - 160f + (float)num3)), value, color2, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						}
						else if (!(f.CurrentTool as FishingRod).hasDoneFucntionYet || (f.CurrentTool as FishingRod).pullingOutOfWater)
						{
							spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 64f + (float)num4, playerPosition.Y - 160f + (float)num3)), value, color2, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						}
						break;
					case 1:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 64f + (float)num4, playerPosition.Y - 160f + 8f + (float)num3)), value, color2, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 2:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 96f + 32f + (float)num4, playerPosition.Y - 128f - 24f + (float)num3)), value, color2, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 3:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 96f + 24f + (float)num4, playerPosition.Y - 128f - 32f + (float)num3)), value, color2, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 4:
						if ((f.CurrentTool as FishingRod).isFishing || (f.CurrentTool as FishingRod).doneWithAnimation)
						{
							spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 64f + (float)num4, playerPosition.Y - 160f + (float)num3)), value, color2, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						}
						else
						{
							spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 64f + (float)num4, playerPosition.Y - 160f + 4f + (float)num3)), value, color2, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						}
						break;
					case 5:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 64f + (float)num4, playerPosition.Y - 160f + (float)num3)), value, color2, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, num + (float)(f.getStandingY() + 64) / 10000f));
						break;
					}
				}
				else if (f.CurrentTool != null && f.CurrentTool is WateringCan)
				{
					switch (((FarmerSprite)f.Sprite).currentAnimationIndex)
					{
					case 0:
					case 1:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + (float)num4 - 4f, playerPosition.Y - 128f + 8f + (float)num3)), value, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, num + (float)(f.GetBoundingBox().Bottom + num6) / 10000f));
						break;
					case 2:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + (float)num4 - 16f, playerPosition.Y - 128f + (float)num3)), value, Color.White, -(float)Math.PI / 12f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, num + (float)(f.GetBoundingBox().Bottom + num6) / 10000f));
						break;
					case 3:
						value.X += 16;
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + (float)num4 - 16f, playerPosition.Y - 128f - 24f + (float)num3)), value, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, num + (float)(f.GetBoundingBox().Bottom + num6) / 10000f));
						break;
					}
				}
				else
				{
					switch (((FarmerSprite)f.Sprite).currentAnimationIndex)
					{
					case 0:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + 32f + 8f + (float)num4 + (float)Math.Min(8, f.toolPower * 4), playerPosition.Y - 128f + 8f + (float)num3 + (float)Math.Min(8, f.toolPower * 4))), value, Color.White, (float)Math.PI / 12f + (float)Math.Min(f.toolPower, 2) * ((float)Math.PI / 64f), new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, num + (float)(f.GetBoundingBox().Bottom + num6) / 10000f));
						break;
					case 1:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 16f + (float)num4, playerPosition.Y - 128f + 16f + (float)num3)), value, Color.White, -(float)Math.PI / 12f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, num + (float)(f.GetBoundingBox().Bottom + num6) / 10000f));
						break;
					case 2:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 64f + 4f + (float)num4, playerPosition.Y - 128f + 60f + (float)num3)), value, Color.White, -(float)Math.PI / 4f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, num + (float)(f.GetBoundingBox().Bottom + num6) / 10000f));
						break;
					case 3:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 64f + 20f + (float)num4, playerPosition.Y - 64f + 76f + (float)num3)), value, Color.White, (float)Math.PI * -7f / 12f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, num + (float)(f.GetBoundingBox().Bottom + num6) / 10000f));
						break;
					case 4:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 64f + 24f + (float)num4, playerPosition.Y + 24f + (float)num3)), value, Color.White, (float)Math.PI * -7f / 12f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, num + (float)(f.GetBoundingBox().Bottom + num6) / 10000f));
						break;
					}
				}
			}
			else
			{
				if (f.CurrentTool is MeleeWeapon && f.FacingDirection == 0)
				{
					return;
				}
				if (((FarmerSprite)f.Sprite).currentAnimationIndex > 2 && (!(f.CurrentTool is FishingRod) || (f.CurrentTool as FishingRod).isCasting || (f.CurrentTool as FishingRod).castedButBobberStillInAir || (f.CurrentTool as FishingRod).isTimingCast))
				{
					Point tileLocationPoint5 = f.getTileLocationPoint();
					if (f.currentLocation.getTileIndexAt(tileLocationPoint5, "Front") != -1 && f.Position.Y % 64f < 32f && f.Position.Y % 64f > 16f)
					{
						return;
					}
				}
				else if (f.CurrentTool is FishingRod && ((FarmerSprite)f.Sprite).currentAnimationIndex <= 2)
				{
					Point tileLocationPoint6 = f.getTileLocationPoint();
					tileLocationPoint6.Y--;
					if (f.currentLocation.getTileIndexAt(tileLocationPoint6, "Front") != -1)
					{
						return;
					}
				}
				if ((f.CurrentTool != null && f.CurrentTool is FishingRod) || (currentToolIndex >= 48 && currentToolIndex <= 55 && !(f.CurrentTool as FishingRod).fishCaught))
				{
					Color color3 = (f.CurrentTool as FishingRod).getColor();
					switch (((FarmerSprite)f.Sprite).currentAnimationIndex)
					{
					case 0:
						if (!(f.CurrentTool as FishingRod).showingTreasure && !(f.CurrentTool as FishingRod).fishCaught && (f.FacingDirection != 0 || !(f.CurrentTool as FishingRod).isFishing || (f.CurrentTool as FishingRod).isReeling))
						{
							spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 64f, playerPosition.Y - 128f + 4f)), value, color3, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + ((f.FacingDirection != 0) ? 128 : 0)) / 10000f));
						}
						break;
					case 1:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 64f, playerPosition.Y - 128f + 4f)), value, color3, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + ((f.FacingDirection != 0) ? 128 : 0)) / 10000f));
						break;
					case 2:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 64f, playerPosition.Y - 128f + 4f)), value, color3, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + ((f.FacingDirection != 0) ? 128 : 0)) / 10000f));
						break;
					case 3:
						if (f.FacingDirection == 2)
						{
							spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 64f, playerPosition.Y - 128f + 4f)), value, color3, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + ((f.FacingDirection != 0) ? 128 : 0)) / 10000f));
						}
						break;
					case 4:
						if (f.FacingDirection == 0 && (f.CurrentTool as FishingRod).isFishing)
						{
							spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 80f, playerPosition.Y - 96f)), value, color3, 0f, Vector2.Zero, 4f, SpriteEffects.FlipVertically, Math.Max(0f, num + (float)(f.getStandingY() + 128) / 10000f));
						}
						else if (f.FacingDirection == 2)
						{
							spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 64f, playerPosition.Y - 128f + 4f)), value, color3, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + ((f.FacingDirection != 0) ? 128 : 0)) / 10000f));
						}
						break;
					case 5:
						if (f.FacingDirection == 2 && !(f.CurrentTool as FishingRod).showingTreasure && !(f.CurrentTool as FishingRod).fishCaught)
						{
							spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X - 64f, playerPosition.Y - 128f + 4f)), value, color3, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + ((f.FacingDirection != 0) ? 128 : 0)) / 10000f));
						}
						break;
					}
					return;
				}
				if (f.CurrentTool != null && f.CurrentTool is WateringCan)
				{
					switch (((FarmerSprite)f.Sprite).currentAnimationIndex)
					{
					case 0:
					case 1:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + (float)num4, playerPosition.Y - 128f + 16f + (float)num3)), value, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)f.GetBoundingBox().Bottom / 10000f));
						break;
					case 2:
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + (float)num4, playerPosition.Y - 128f - (float)((f.FacingDirection == 2) ? (-4) : 32) + (float)num3)), value, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)f.GetBoundingBox().Bottom / 10000f));
						break;
					case 3:
						if (f.FacingDirection == 2)
						{
							value.X += 16;
						}
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + (float)num4 - (float)((f.FacingDirection == 2) ? 4 : 0), playerPosition.Y - 128f - (float)((f.FacingDirection == 2) ? (-24) : 64) + (float)num3)), value, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)f.GetBoundingBox().Bottom / 10000f));
						break;
					}
					return;
				}
				switch (((FarmerSprite)f.Sprite).currentAnimationIndex)
				{
				case 0:
					if (f.FacingDirection == 0)
					{
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + (float)num4, playerPosition.Y - 128f - 8f + (float)num3 + (float)Math.Min(8, f.toolPower * 4))), value, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() - 8) / 10000f));
					}
					else
					{
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + (float)num4 - 20f, playerPosition.Y - 128f + 12f + (float)num3 + (float)Math.Min(8, f.toolPower * 4))), value, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 8) / 10000f));
					}
					break;
				case 1:
					if (f.FacingDirection == 0)
					{
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + (float)num4 + 4f, playerPosition.Y - 128f + 40f + (float)num3)), value, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() - 8) / 10000f));
					}
					else
					{
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + (float)num4 - 12f, playerPosition.Y - 128f + 32f + (float)num3)), value, Color.White, -(float)Math.PI / 24f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 8) / 10000f));
					}
					break;
				case 2:
					spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + (float)num4, playerPosition.Y - 128f + 64f + (float)num3)), value, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)((f.getStandingY() + f.FacingDirection == 0) ? (-8) : 8) / 10000f));
					break;
				case 3:
					if (f.FacingDirection != 0)
					{
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + (float)num4, playerPosition.Y - 64f + 44f + (float)num3)), value, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 8) / 10000f));
					}
					break;
				case 4:
					if (f.FacingDirection != 0)
					{
						spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + (float)num4, playerPosition.Y - 64f + 48f + (float)num3)), value, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 8) / 10000f));
					}
					break;
				case 5:
					spriteBatch.Draw(toolSpriteSheet, Utility.snapToInt(new Vector2(playerPosition.X + (float)num4, playerPosition.Y - 64f + 32f + (float)num3)), value, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, num + (float)(f.getStandingY() + 8) / 10000f));
					break;
				}
			}
		}

		public static Vector2 GlobalToLocal(xTile.Dimensions.Rectangle viewport, Vector2 globalPosition)
		{
			return new Vector2(globalPosition.X - (float)viewport.X, globalPosition.Y - (float)viewport.Y);
		}

		public static bool IsEnglish()
		{
			return content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en;
		}

		public static Vector2 GlobalToLocal(Vector2 globalPosition)
		{
			return new Vector2(globalPosition.X - (float)viewport.X, globalPosition.Y - (float)viewport.Y);
		}

		public static Microsoft.Xna.Framework.Rectangle GlobalToLocal(xTile.Dimensions.Rectangle viewport, Microsoft.Xna.Framework.Rectangle globalPosition)
		{
			return new Microsoft.Xna.Framework.Rectangle(globalPosition.X - viewport.X, globalPosition.Y - viewport.Y, globalPosition.Width, globalPosition.Height);
		}

		public static string parseText(string text, SpriteFont whichFont, int width, float scale = 1f)
		{
			if (text == null)
			{
				return "";
			}
			string text2 = string.Empty;
			string text3 = string.Empty;
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.th)
			{
				string text4 = text;
				for (int i = 0; i < text4.Length; i++)
				{
					char c = text4[i];
					if (whichFont.MeasureString(text2 + c).Length() * scale > (float)width || c.Equals(Environment.NewLine))
					{
						text3 = text3 + text2 + Environment.NewLine;
						text2 = string.Empty;
					}
					if (!c.Equals(Environment.NewLine))
					{
						text2 += c;
					}
				}
				return text3 + text2;
			}
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr && text.Contains("^"))
			{
				string[] array = text.Split('^');
				text = ((!player.IsMale) ? array[1] : array[0]);
			}
			string[] array2 = text.Split(' ');
			string[] array3 = array2;
			foreach (string text5 in array3)
			{
				try
				{
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr && text5.StartsWith("\n-"))
					{
						text3 = text3 + text2 + Environment.NewLine;
						text2 = string.Empty;
					}
					if (whichFont.MeasureString(text2 + text5).X * scale > (float)width || text5.Equals(Environment.NewLine))
					{
						text3 = text3 + text2 + Environment.NewLine;
						text2 = string.Empty;
					}
					if (!text5.Equals(Environment.NewLine))
					{
						text2 = text2 + text5 + " ";
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Exception measuring string: " + ex);
				}
			}
			return text3 + text2;
		}

		public static void UpdateHorseOwnership()
		{
			bool flag = false;
			Dictionary<long, Horse> dictionary = new Dictionary<long, Horse>();
			HashSet<Horse> hashSet = new HashSet<Horse>();
			List<Stable> list = new List<Stable>();
			foreach (Building building in getFarm().buildings)
			{
				if (building is Stable && (int)building.daysOfConstructionLeft <= 0)
				{
					Stable item = building as Stable;
					list.Add(item);
				}
			}
			foreach (Stable item2 in list)
			{
				if (item2.owner.Value == -6666666 && getFarmerMaybeOffline(-6666666L) == null)
				{
					item2.owner.Value = player.UniqueMultiplayerID;
				}
				if (!emergencyLoading)
				{
					item2.grabHorse();
				}
			}
			foreach (Stable item3 in list)
			{
				Horse stableHorse = item3.getStableHorse();
				if (stableHorse != null && !hashSet.Contains(stableHorse) && stableHorse.getOwner() != null && !dictionary.ContainsKey(stableHorse.getOwner().UniqueMultiplayerID) && stableHorse.getOwner().horseName.Value != null && stableHorse.getOwner().horseName.Value.Length > 0 && stableHorse.Name == stableHorse.getOwner().horseName.Value)
				{
					dictionary[stableHorse.getOwner().UniqueMultiplayerID] = stableHorse;
					hashSet.Add(stableHorse);
					if (flag)
					{
						Console.WriteLine("Assigned horse " + stableHorse.Name + " to " + stableHorse.getOwner().Name + " (Exact match)");
					}
				}
			}
			Dictionary<string, Farmer> dictionary2 = new Dictionary<string, Farmer>();
			foreach (Farmer allFarmer in getAllFarmers())
			{
				if (allFarmer == null || allFarmer.horseName.Value == null || allFarmer.horseName.Value.Length == 0)
				{
					continue;
				}
				bool flag2 = false;
				foreach (Horse item4 in hashSet)
				{
					if (item4.getOwner() == allFarmer)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					dictionary2[allFarmer.horseName] = allFarmer;
				}
			}
			foreach (Stable item5 in list)
			{
				Horse stableHorse2 = item5.getStableHorse();
				if (stableHorse2 != null && !hashSet.Contains(stableHorse2) && stableHorse2.getOwner() != null && stableHorse2.Name != null && stableHorse2.Name.Length > 0 && dictionary2.ContainsKey(stableHorse2.Name) && !dictionary.ContainsKey(dictionary2[stableHorse2.Name].UniqueMultiplayerID))
				{
					item5.owner.Value = dictionary2[stableHorse2.Name].UniqueMultiplayerID;
					item5.updateHorseOwnership();
					dictionary[stableHorse2.getOwner().UniqueMultiplayerID] = stableHorse2;
					hashSet.Add(stableHorse2);
					if (flag)
					{
						Console.WriteLine("Assigned horse " + stableHorse2.Name + " to " + stableHorse2.getOwner().Name + " (Name match from different owner.)");
					}
				}
			}
			foreach (Stable item6 in list)
			{
				Horse stableHorse3 = item6.getStableHorse();
				if (stableHorse3 != null && !hashSet.Contains(stableHorse3) && stableHorse3.getOwner() != null && !dictionary.ContainsKey(stableHorse3.getOwner().UniqueMultiplayerID))
				{
					dictionary[stableHorse3.getOwner().UniqueMultiplayerID] = stableHorse3;
					hashSet.Add(stableHorse3);
					item6.updateHorseOwnership();
					if (flag)
					{
						Console.WriteLine("Assigned horse " + stableHorse3.Name + " to " + stableHorse3.getOwner().Name + " (Owner's only stable)");
					}
				}
			}
			foreach (Stable item7 in list)
			{
				Horse stableHorse4 = item7.getStableHorse();
				if (stableHorse4 == null || hashSet.Contains(stableHorse4))
				{
					continue;
				}
				foreach (Horse item8 in hashSet)
				{
					if (stableHorse4.ownerId == item8.ownerId)
					{
						item7.owner.Value = 0L;
						item7.updateHorseOwnership();
						if (flag)
						{
							Console.WriteLine("Unassigned horse (stable owner already has a horse).");
						}
						break;
					}
				}
			}
		}

		public static string LoadStringByGender(int npcGender, string key)
		{
			if (npcGender == 0)
			{
				return content.LoadString(key).Split('/').First();
			}
			return content.LoadString(key).Split('/').Last();
		}

		public static string LoadStringByGender(int npcGender, string key, params object[] substitutions)
		{
			string text = "";
			if (npcGender == 0)
			{
				text = content.LoadString(key).Split('/').First();
				if (substitutions.Length != 0)
				{
					try
					{
						return string.Format(text, substitutions);
					}
					catch (Exception)
					{
						return text;
					}
				}
			}
			text = content.LoadString(key).Split('/').Last();
			if (substitutions.Length != 0)
			{
				try
				{
					return string.Format(text, substitutions);
				}
				catch (Exception)
				{
					return text;
				}
			}
			return text;
		}

		public static string parseText(string text)
		{
			return parseText(text, dialogueFont, dialogueWidth);
		}

		public static bool isThisPositionVisibleToPlayer(string locationName, Vector2 position)
		{
			if (locationName.Equals(currentLocation.Name) && new Microsoft.Xna.Framework.Rectangle((int)(player.Position.X - (float)(viewport.Width / 2)), (int)(player.Position.Y - (float)(viewport.Height / 2)), viewport.Width, viewport.Height).Contains(new Point((int)position.X, (int)position.Y)))
			{
				return true;
			}
			return false;
		}

		public static Microsoft.Xna.Framework.Rectangle getSourceRectForStandardTileSheet(Texture2D tileSheet, int tilePosition, int width = -1, int height = -1)
		{
			if (width == -1)
			{
				width = 64;
			}
			if (height == -1)
			{
				height = 64;
			}
			return new Microsoft.Xna.Framework.Rectangle(tilePosition * width % tileSheet.Width, tilePosition * width / tileSheet.Width * height, width, height);
		}

		public static Microsoft.Xna.Framework.Rectangle getSquareSourceRectForNonStandardTileSheet(Texture2D tileSheet, int tileWidth, int tileHeight, int tilePosition)
		{
			return new Microsoft.Xna.Framework.Rectangle(tilePosition * tileWidth % tileSheet.Width, tilePosition * tileWidth / tileSheet.Width * tileHeight, tileWidth, tileHeight);
		}

		public static Microsoft.Xna.Framework.Rectangle getArbitrarySourceRect(Texture2D tileSheet, int tileWidth, int tileHeight, int tilePosition)
		{
			if (tileSheet != null)
			{
				return new Microsoft.Xna.Framework.Rectangle(tilePosition * tileWidth % tileSheet.Width, tilePosition * tileWidth / tileSheet.Width * tileHeight, tileWidth, tileHeight);
			}
			return Microsoft.Xna.Framework.Rectangle.Empty;
		}

		public static string getTimeOfDayString(int time)
		{
			string text = ((time % 100 == 0) ? "0" : string.Empty);
			string text2;
			switch (LocalizedContentManager.CurrentLanguageCode)
			{
			default:
				text2 = ((time / 100 % 12 == 0) ? "12" : (time / 100 % 12).ToString());
				break;
			case LocalizedContentManager.LanguageCode.ja:
				text2 = ((time / 100 % 12 == 0) ? "0" : (time / 100 % 12).ToString());
				break;
			case LocalizedContentManager.LanguageCode.zh:
				text2 = ((time / 100 % 24 == 0) ? "00" : ((time / 100 % 12 == 0) ? "12" : (time / 100 % 12).ToString()));
				break;
			case LocalizedContentManager.LanguageCode.ru:
			case LocalizedContentManager.LanguageCode.pt:
			case LocalizedContentManager.LanguageCode.es:
			case LocalizedContentManager.LanguageCode.de:
			case LocalizedContentManager.LanguageCode.th:
			case LocalizedContentManager.LanguageCode.fr:
			case LocalizedContentManager.LanguageCode.tr:
			case LocalizedContentManager.LanguageCode.hu:
				text2 = (time / 100 % 24).ToString();
				text2 = ((time / 100 % 24 <= 9) ? ("0" + text2) : text2);
				break;
			}
			string text3 = text2 + ":" + time % 100 + text;
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en)
			{
				text3 = text3 + " " + ((time < 1200 || time >= 2400) ? content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370") : content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371"));
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
			{
				text3 = ((time < 1200 || time >= 2400) ? (content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370") + " " + text3) : (content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371") + " " + text3));
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh)
			{
				text3 = ((time < 600 || time >= 2400) ? ("凌晨 " + text3) : ((time < 1200) ? (content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370") + " " + text3) : ((time < 1300) ? ("中午  " + text3) : ((time < 1900) ? (content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371") + " " + text3) : ("晚上  " + text3)))));
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr)
			{
				text3 = ((time % 100 != 0) ? ((time / 100 == 24) ? "00h" : (time / 100 + "h" + time % 100)) : ((time / 100 == 24) ? "00h" : (time / 100 + "h")));
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
			{
				return LocalizedContentManager.FormatTimeString(time, LocalizedContentManager.CurrentModLanguage.TimeFormat).ToString();
			}
			return text3;
		}

		public bool checkBigCraftableBoundariesForFrontLayer()
		{
			if (currentLocation.Map.GetLayer("Front").PickTile(new Location(player.getStandingX() - 32, (int)player.Position.Y - 38), viewport.Size) == null && currentLocation.Map.GetLayer("Front").PickTile(new Location(player.getStandingX() + 32, (int)player.Position.Y - 38), viewport.Size) == null && currentLocation.Map.GetLayer("Front").PickTile(new Location(player.getStandingX() - 32, (int)player.Position.Y - 38 - 64), viewport.Size) == null)
			{
				return currentLocation.Map.GetLayer("Front").PickTile(new Location(player.getStandingX() + 32, (int)player.Position.Y - 38 - 64), viewport.Size) != null;
			}
			return true;
		}

		public static bool[,] getCircleOutlineGrid(int radius)
		{
			bool[,] array = new bool[radius * 2 + 1, radius * 2 + 1];
			int num = 1 - radius;
			int num2 = 1;
			int num3 = -2 * radius;
			int num4 = 0;
			int num5 = radius;
			array[radius, radius + radius] = true;
			array[radius, radius - radius] = true;
			array[radius + radius, radius] = true;
			array[radius - radius, radius] = true;
			while (num4 < num5)
			{
				if (num >= 0)
				{
					num5--;
					num3 += 2;
					num += num3;
				}
				num4++;
				num2 += 2;
				num += num2;
				array[radius + num4, radius + num5] = true;
				array[radius - num4, radius + num5] = true;
				array[radius + num4, radius - num5] = true;
				array[radius - num4, radius - num5] = true;
				array[radius + num5, radius + num4] = true;
				array[radius - num5, radius + num4] = true;
				array[radius + num5, radius - num4] = true;
				array[radius - num5, radius - num4] = true;
			}
			return array;
		}

		public static Color getColorForTreasureType(string type)
		{
			if (type != null)
			{
				switch (type.Length)
				{
				case 4:
					switch (type[0])
					{
					case 'I':
						if (!(type == "Iron"))
						{
							break;
						}
						return Color.LightSlateGray;
					case 'C':
						if (!(type == "Coal"))
						{
							break;
						}
						return Color.Black;
					case 'G':
						if (!(type == "Gold"))
						{
							break;
						}
						return Color.Gold;
					case 'A':
						if (!(type == "Arch"))
						{
							break;
						}
						return Color.White;
					}
					break;
				case 6:
					if (!(type == "Copper"))
					{
						break;
					}
					return Color.Sienna;
				case 7:
					if (!(type == "Iridium"))
					{
						break;
					}
					return Color.Purple;
				case 5:
					if (!(type == "Coins"))
					{
						break;
					}
					return Color.Yellow;
				}
			}
			return Color.SaddleBrown;
		}

		public static string GetFarmTypeID()
		{
			if (whichFarm == 7 && whichModFarm != null)
			{
				return whichModFarm.ID;
			}
			return whichFarm.ToString();
		}

		public static string GetFarmTypeModData(string key)
		{
			if (whichFarm == 7 && whichModFarm != null && whichModFarm.ModData != null && whichModFarm.ModData.ContainsKey(key))
			{
				return whichModFarm.ModData[key];
			}
			return null;
		}

		public void _PerformRemoveNormalItemFromWorldOvernight(int parent_sheet_index)
		{
			foreach (GameLocation location in locations)
			{
				_RecursiveRemoveThisNormalItemLocation(location, parent_sheet_index);
			}
			foreach (MineShaft activeMine in MineShaft.activeMines)
			{
				_RecursiveRemoveThisNormalItemLocation(activeMine, parent_sheet_index);
			}
			foreach (VolcanoDungeon activeLevel in VolcanoDungeon.activeLevels)
			{
				_RecursiveRemoveThisNormalItemLocation(activeLevel, parent_sheet_index);
			}
			for (int i = 0; i < player.team.returnedDonations.Count; i++)
			{
				if (_RecursiveRemoveThisNormalItemItem(player.team.returnedDonations[i], parent_sheet_index))
				{
					player.team.returnedDonations.RemoveAt(i);
					i--;
				}
			}
			for (int j = 0; j < player.team.junimoChest.Count; j++)
			{
				if (_RecursiveRemoveThisNormalItemItem(player.team.junimoChest[j], parent_sheet_index))
				{
					player.team.junimoChest.RemoveAt(j);
					j--;
				}
			}
			foreach (SpecialOrder specialOrder in player.team.specialOrders)
			{
				for (int k = 0; k < specialOrder.donatedItems.Count; k++)
				{
					Item this_item = specialOrder.donatedItems[k];
					if (_RecursiveRemoveThisNormalItemItem(this_item, parent_sheet_index))
					{
						specialOrder.donatedItems[k] = null;
					}
				}
			}
		}

		protected virtual void _PerformRemoveNormalItemFromFarmerOvernight(Farmer farmer, int parent_sheet_index)
		{
			for (int i = 0; i < farmer.items.Count; i++)
			{
				if (_RecursiveRemoveThisNormalItemItem(farmer.items[i], parent_sheet_index))
				{
					farmer.items[i] = null;
				}
			}
			for (int j = 0; j < farmer.itemsLostLastDeath.Count; j++)
			{
				if (_RecursiveRemoveThisNormalItemItem(farmer.itemsLostLastDeath[j], parent_sheet_index))
				{
					farmer.itemsLostLastDeath.RemoveAt(j);
					j--;
				}
			}
			if (farmer.recoveredItem != null && _RecursiveRemoveThisNormalItemItem(farmer.recoveredItem, parent_sheet_index))
			{
				farmer.recoveredItem = null;
				farmer.mailbox.Remove("MarlonRecovery");
				farmer.mailForTomorrow.Remove("MarlonRecovery");
			}
		}

		protected virtual bool _RecursiveRemoveThisNormalItemItem(Item this_item, int parent_sheet_index)
		{
			if (this_item == null)
			{
				return false;
			}
			if (this_item is Object)
			{
				Object @object = this_item as Object;
				if (@object.heldObject.Value != null && _RecursiveRemoveThisNormalItemItem(@object.heldObject.Value, parent_sheet_index))
				{
					@object.heldObject.Value = null;
					@object.readyForHarvest.Value = false;
					@object.showNextIndex.Value = false;
				}
				if (@object is StorageFurniture)
				{
					bool flag = false;
					for (int i = 0; i < (@object as StorageFurniture).heldItems.Count; i++)
					{
						Item item = (@object as StorageFurniture).heldItems[i];
						if (item != null && _RecursiveRemoveThisNormalItemItem(item, parent_sheet_index))
						{
							(@object as StorageFurniture).heldItems[i] = null;
							flag = true;
						}
					}
					if (flag)
					{
						(@object as StorageFurniture).ClearNulls();
					}
				}
				if (@object is IndoorPot)
				{
					IndoorPot indoorPot = @object as IndoorPot;
					if (indoorPot.hoeDirt != null)
					{
						_RecursiveRemoveThisNormalItemDirt(indoorPot.hoeDirt, null, Vector2.Zero, parent_sheet_index);
					}
				}
				if (@object is Chest)
				{
					bool flag2 = false;
					for (int j = 0; j < (@object as Chest).items.Count; j++)
					{
						Item item2 = (@object as Chest).items[j];
						if (item2 != null && _RecursiveRemoveThisNormalItemItem(item2, parent_sheet_index))
						{
							(@object as Chest).items[j] = null;
							flag2 = true;
						}
					}
					if (flag2)
					{
						(@object as Chest).clearNulls();
					}
				}
				if (@object.heldObject.Value != null && _RecursiveRemoveThisNormalItemItem((Object)@object.heldObject, parent_sheet_index))
				{
					@object.heldObject.Value = null;
				}
			}
			return Utility.IsNormalObjectAtParentSheetIndex(this_item, parent_sheet_index);
		}

		protected virtual void _RecursiveRemoveThisNormalItemDirt(HoeDirt dirt, GameLocation location, Vector2 coord, int parent_sheet_index)
		{
			if (dirt.crop != null && dirt.crop.indexOfHarvest.Value == parent_sheet_index)
			{
				dirt.destroyCrop(coord, showAnimation: false, location);
			}
		}

		protected virtual void _RecursiveRemoveThisNormalItemLocation(GameLocation l, int parent_sheet_index)
		{
			if (l == null)
			{
				return;
			}
			if (l != null)
			{
				List<Guid> list = new List<Guid>();
				foreach (Furniture item5 in l.furniture)
				{
					if (_RecursiveRemoveThisNormalItemItem(item5, parent_sheet_index))
					{
						list.Add(l.furniture.GuidOf(item5));
					}
				}
				foreach (Guid item6 in list)
				{
					l.furniture.Remove(item6);
				}
				foreach (NPC character in l.characters)
				{
					if (!(character is Monster))
					{
						continue;
					}
					Monster monster = character as Monster;
					if (monster.objectsToDrop == null || monster.objectsToDrop.Count <= 0)
					{
						continue;
					}
					for (int num = monster.objectsToDrop.Count - 1; num >= 0; num--)
					{
						if (monster.objectsToDrop[num] == parent_sheet_index)
						{
							monster.objectsToDrop.RemoveAt(num);
						}
					}
				}
			}
			if (l is IslandFarmHouse)
			{
				for (int i = 0; i < (l as IslandFarmHouse).fridge.Value.items.Count; i++)
				{
					Item item = (l as IslandFarmHouse).fridge.Value.items[i];
					if (item != null && _RecursiveRemoveThisNormalItemItem(item, parent_sheet_index))
					{
						(l as IslandFarmHouse).fridge.Value.items[i] = null;
					}
				}
			}
			foreach (Vector2 key in l.terrainFeatures.Keys)
			{
				TerrainFeature terrainFeature = l.terrainFeatures[key];
				if (terrainFeature is HoeDirt)
				{
					HoeDirt dirt = terrainFeature as HoeDirt;
					_RecursiveRemoveThisNormalItemDirt(dirt, l, key, parent_sheet_index);
				}
			}
			if (l is FarmHouse)
			{
				for (int j = 0; j < (l as FarmHouse).fridge.Value.items.Count; j++)
				{
					Item item2 = (l as FarmHouse).fridge.Value.items[j];
					if (item2 != null && _RecursiveRemoveThisNormalItemItem(item2, parent_sheet_index))
					{
						(l as FarmHouse).fridge.Value.items[j] = null;
					}
				}
			}
			if (l is BuildableGameLocation)
			{
				foreach (Building building in (l as BuildableGameLocation).buildings)
				{
					if (building.indoors.Value != null)
					{
						_RecursiveRemoveThisNormalItemLocation(building.indoors.Value, parent_sheet_index);
					}
					if (building is Mill)
					{
						for (int k = 0; k < (building as Mill).output.Value.items.Count; k++)
						{
							Item item3 = (building as Mill).output.Value.items[k];
							if (item3 != null && _RecursiveRemoveThisNormalItemItem(item3, parent_sheet_index))
							{
								(building as Mill).output.Value.items[k] = null;
							}
						}
					}
					else
					{
						if (!(building is JunimoHut))
						{
							continue;
						}
						bool flag = false;
						Chest value = (building as JunimoHut).output.Value;
						for (int m = 0; m < value.items.Count; m++)
						{
							Item item4 = value.items[m];
							if (item4 != null && _RecursiveRemoveThisNormalItemItem(item4, parent_sheet_index))
							{
								value.items[m] = null;
								flag = true;
							}
						}
						if (flag)
						{
							value.clearNulls();
						}
					}
				}
			}
			List<Vector2> list2 = new List<Vector2>(l.objects.Keys);
			foreach (Vector2 item7 in list2)
			{
				if (_RecursiveRemoveThisNormalItemItem(l.objects[item7], parent_sheet_index))
				{
					l.objects.Remove(item7);
				}
			}
			for (int n = 0; n < l.debris.Count; n++)
			{
				Debris debris = l.debris[n];
				if (debris.item != null && _RecursiveRemoveThisNormalItemItem(debris.item, parent_sheet_index))
				{
					l.debris.RemoveAt(n);
					n--;
				}
			}
		}

		protected static void _UpdateDebrisWeatherAndRainDropsForResize(xTile.Dimensions.Rectangle old_viewport, xTile.Dimensions.Rectangle new_viewport)
		{
			int num = _GetWeatherDebrisCountForViewportSize(70, new_viewport);
			if (num != rainDrops.Count)
			{
				for (int i = 0; i < rainDrops.Count; i++)
				{
					RainDrop rainDrop = rainDrops[i];
					if (_IsWeatherDebrisOffScreen(rainDrop.position, new_viewport))
					{
						rainDropPool.Add(rainDrop);
						rainDrops.RemoveAt(i);
						i--;
					}
				}
				while (rainDrops.Count < num)
				{
					Vector2 vector = _GetRandomPositionBetweenRectanglesForWeatherDebris(old_viewport, new_viewport);
					rainDrops.Add(GetRainDrop((int)vector.X, (int)vector.Y, random.Next(4), 0));
				}
			}
			UpdateDebrisWeatherForResize(debrisWeather, old_viewport, new_viewport, baseDebrisWeatherCount);
		}

		public static void UpdateDebrisWeatherForResize(List<WeatherDebris> debris, xTile.Dimensions.Rectangle old_viewport, xTile.Dimensions.Rectangle new_viewport, int base_count = -1, int debris_type = -1)
		{
			if (debris == null || base_count == 0)
			{
				return;
			}
			int num = _GetWeatherDebrisCountForViewportSize(base_count, new_viewport);
			if (debris.Count == num)
			{
				return;
			}
			for (int i = 0; i < debris.Count; i++)
			{
				WeatherDebris weatherDebris = debris[i];
				if (_IsWeatherDebrisOffScreen(weatherDebris.position, new_viewport, 16))
				{
					debrisWeatherPool.Add(weatherDebris);
					debris.RemoveAt(i);
					i--;
				}
			}
			if (debris_type == -1)
			{
				debris_type = GetDebrisWeatherIndex();
			}
			while (debris.Count < num)
			{
				debris.Add(GetWeatherDebris(_GetRandomPositionBetweenRectanglesForWeatherDebris(old_viewport, new_viewport), debris_type, (float)random.Next(15) / 500f, (float)random.Next(-10, 0) / 50f, (float)random.Next(10) / 50f));
			}
		}

		protected static bool _IsWeatherDebrisOffScreen(Vector2 position, xTile.Dimensions.Rectangle viewport, int buffer = 0)
		{
			if (position.X + 64f < (float)(viewport.X - buffer))
			{
				return true;
			}
			if (position.Y + 64f < (float)(viewport.Y - buffer))
			{
				return true;
			}
			if (position.X > (float)(viewport.X + viewport.Width + buffer))
			{
				return true;
			}
			if (position.Y > (float)(viewport.Y + viewport.Height + buffer))
			{
				return true;
			}
			return false;
		}

		protected static int _GetWeatherDebrisCountForViewportSize(int base_count, xTile.Dimensions.Rectangle screen)
		{
			int num = screen.Width;
			int num2 = screen.Height;
			if (currentLocation != null && currentLocation.map != null)
			{
				num = Math.Min(num, currentLocation.map.DisplayWidth);
				num2 = Math.Min(num2, currentLocation.map.DisplayHeight);
			}
			return (int)((float)(num * num2) / 921600f * (float)base_count);
		}

		protected static Vector2 _GetRandomPositionBetweenRectanglesForWeatherDebris(xTile.Dimensions.Rectangle a, xTile.Dimensions.Rectangle b, bool weather_particle = false)
		{
			Vector2 zero = Vector2.Zero;
			float t = (float)random.NextDouble();
			xTile.Dimensions.Rectangle rectangle = new xTile.Dimensions.Rectangle((int)Utility.Lerp(a.X, b.X, t), (int)Utility.Lerp(a.Y, b.Y, t), (int)Utility.Lerp(a.Width, b.Width, t), (int)Utility.Lerp(a.Height, b.Height, t));
			if (random.NextDouble() < 0.5)
			{
				if (weather_particle || random.NextDouble() < 0.5)
				{
					zero.Y = rectangle.Y - 64;
				}
				else
				{
					zero.Y = rectangle.Y + rectangle.Height;
				}
				zero.X = Utility.RandomFloat(rectangle.X, rectangle.MaxCorner.X);
			}
			else
			{
				if (weather_particle || random.NextDouble() < 0.5)
				{
					zero.X = rectangle.X + rectangle.Width;
				}
				else
				{
					zero.X = rectangle.X - 64;
				}
				zero.Y = Utility.RandomFloat(rectangle.Y, rectangle.MaxCorner.Y);
			}
			return zero;
		}
	}
}
