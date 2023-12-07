using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Mobile;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley
{
	public class Event
	{
		protected static Dictionary<string, MethodInfo> _commandLookup;

		[InstancedStatic]
		internal static object[] _eventCommandArgs = new object[3];

		public const int weddingEventId = -2;

		private const float timeBetweenSpeech = 500f;

		private const float viewportMoveSpeed = 3f;

		public const string festivalTextureName = "Maps\\Festivals";

		public bool simultaneousCommand;

		public string[] eventCommands;

		public int currentCommand;

		public int farmerAddedSpeed;

		public int int_useMeForAnything;

		public int int_useMeForAnything2;

		public List<NPC> actors = new List<NPC>();

		public List<Object> props = new List<Object>();

		public List<Prop> festivalProps = new List<Prop>();

		public string messageToScreen;

		public string playerControlSequenceID;

		public string spriteTextToDraw;

		public bool showActiveObject;

		public bool continueAfterMove;

		public bool specialEventVariable1;

		public bool forked;

		public bool eventSwitched;

		public bool isFestival;

		public bool showGroundObjects = true;

		public bool isWedding;

		public bool doingSecretSanta;

		public bool showWorldCharacters;

		public bool isMemory;

		public bool ignoreObjectCollisions = true;

		protected bool _playerControlSequence;

		private Dictionary<string, Vector3> actorPositionsAfterMove;

		private float timeAccumulator;

		private float viewportXAccumulator;

		private float viewportYAccumulator;

		public float float_useMeForAnything;

		private Vector3 viewportTarget;

		private Color previousAmbientLight;

		public List<NPC> npcsWithUniquePortraits = new List<NPC>();

		public LocationRequest exitLocation;

		public ICustomEventScript currentCustomEventScript;

		public List<Farmer> farmerActors = new List<Farmer>();

		private HashSet<long> festivalWinners = new HashSet<long>();

		public Action onEventFinished;

		protected bool _repeatingLocationSpecificCommand;

		private readonly LocalizedContentManager festivalContent = Game1.content.CreateTemporary();

		private GameLocation temporaryLocation;

		public Point playerControlTargetTile;

		private Texture2D _festivalTexture;

		public List<NPCController> npcControllers;

		public NPC secretSantaRecipient;

		public NPC mySecretSanta;

		public bool skippable;

		public int id;

		public List<Vector2> characterWalkLocations = new List<Vector2>();

		public bool ignoreTileOffsets;

		public Vector2 eventPositionTileOffset = Vector2.Zero;

		[NonInstancedStatic]
		public static HashSet<string> invalidFestivals = new HashSet<string>();

		private Dictionary<string, string> festivalData;

		private int oldShirt;

		private Color oldPants;

		private bool drawTool;

		public bool skipped;

		private bool waitingForMenuClose;

		private int oldTime;

		public List<TemporaryAnimatedSprite> underwaterSprites;

		public List<TemporaryAnimatedSprite> aboveMapSprites;

		internal NPC festivalHost;

		private string hostMessage;

		public int festivalTimer;

		public int grangeScore = -1000;

		public bool grangeJudged;

		private int previousFacingDirection = -1;

		public Dictionary<string, Dictionary<ISalable, int[]>> festivalShops;

		private int previousAnswerChoice = -1;

		private bool startSecretSantaAfterDialogue;

		public bool specialEventVariable2;

		private List<Farmer> winners;

		public bool playerControlSequence
		{
			get
			{
				return _playerControlSequence;
			}
			set
			{
				if (_playerControlSequence != value)
				{
					_playerControlSequence = value;
					if (!_playerControlSequence)
					{
						OnPlayerControlSequenceEnd(playerControlSequenceID);
					}
				}
			}
		}

		public Farmer farmer
		{
			get
			{
				if (farmerActors.Count <= 0)
				{
					return Game1.player;
				}
				return farmerActors[0];
			}
		}

		public Texture2D festivalTexture
		{
			get
			{
				if (_festivalTexture == null)
				{
					_festivalTexture = festivalContent.Load<Texture2D>("Maps\\Festivals");
				}
				return _festivalTexture;
			}
		}

		public int CurrentCommand
		{
			get
			{
				return currentCommand;
			}
			set
			{
				currentCommand = value;
			}
		}

		public string FestivalName
		{
			get
			{
				if (festivalData == null)
				{
					return "";
				}
				return festivalData["name"];
			}
		}

		public virtual void setupEventCommands()
		{
			if (_commandLookup == null)
			{
				_commandLookup = new Dictionary<string, MethodInfo>(StringComparer.InvariantCultureIgnoreCase);
				MethodInfo[] array = (from method_info in typeof(Event).GetMethods()
					where method_info.Name.StartsWith("command_")
					select method_info).ToArray();
				MethodInfo[] array2 = array;
				foreach (MethodInfo methodInfo in array2)
				{
					_commandLookup.Add(methodInfo.Name.Substring("command_".Length), methodInfo);
				}
				Console.WriteLine("setupEventCommands() registered '{0}' methods", array.Length);
			}
		}

		public virtual void tryEventCommand(GameLocation location, GameTime time, string[] split)
		{
			_eventCommandArgs[0] = location;
			_eventCommandArgs[1] = time;
			_eventCommandArgs[2] = split;
			if (split.Length == 0)
			{
				return;
			}
			if (_commandLookup.TryGetValue(split[0], out var value))
			{
				try
				{
					value.Invoke(this, _eventCommandArgs);
					return;
				}
				catch (TargetInvocationException ex)
				{
					LogErrorAndHalt(ex.InnerException);
					return;
				}
			}
			Console.WriteLine("ERROR: Invalid command: " + split[0]);
		}

		public virtual void command_ignoreEventTileOffset(GameLocation location, GameTime time, string[] split)
		{
			ignoreTileOffsets = true;
			CurrentCommand++;
		}

		public virtual void command_move(GameLocation location, GameTime time, string[] split)
		{
			for (int i = 1; i < split.Length && split.Length - i >= 3; i += 4)
			{
				if (split[i].Contains("farmer") && !actorPositionsAfterMove.ContainsKey(split[i]))
				{
					Farmer farmerFromFarmerNumberString = getFarmerFromFarmerNumberString(split[i], farmer);
					if (farmerFromFarmerNumberString != null)
					{
						farmerFromFarmerNumberString.canOnlyWalk = false;
						farmerFromFarmerNumberString.setRunning(isRunning: false, force: true);
						farmerFromFarmerNumberString.canOnlyWalk = true;
						farmerFromFarmerNumberString.convertEventMotionCommandToMovement(new Vector2(Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2])));
						actorPositionsAfterMove.Add(split[i], getPositionAfterMove(farmer, Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2]), Convert.ToInt32(split[i + 3])));
					}
				}
				else
				{
					NPC actorByName = getActorByName(split[i]);
					string key = (split[i].Equals("rival") ? Utility.getOtherFarmerNames()[0] : split[i]);
					if (!actorPositionsAfterMove.ContainsKey(key))
					{
						actorByName.convertEventMotionCommandToMovement(new Vector2(Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2])));
						actorPositionsAfterMove.Add(key, getPositionAfterMove(actorByName, Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2]), Convert.ToInt32(split[i + 3])));
					}
				}
			}
			if (split.Last().Equals("true"))
			{
				continueAfterMove = true;
				CurrentCommand++;
			}
			else if (split.Last().Equals("false"))
			{
				continueAfterMove = false;
				if (split.Length == 2 && actorPositionsAfterMove.Count == 0)
				{
					CurrentCommand++;
				}
			}
		}

		public virtual void command_speak(GameLocation location, GameTime time, string[] split)
		{
			if (skipped || Game1.dialogueUp)
			{
				return;
			}
			timeAccumulator += time.ElapsedGameTime.Milliseconds;
			if (timeAccumulator < 500f)
			{
				return;
			}
			timeAccumulator = 0f;
			NPC actorByName = getActorByName(split[1]);
			if (actorByName == null)
			{
				Game1.getCharacterFromName(split[1].Equals("rival") ? Utility.getOtherFarmerNames()[0] : split[1]);
			}
			if (actorByName == null)
			{
				Game1.eventFinished();
				return;
			}
			int num = eventCommands[currentCommand].IndexOf('"');
			if (num > 0)
			{
				int num2 = eventCommands[CurrentCommand].Substring(num + 1).LastIndexOf('"');
				Game1.player.checkForQuestComplete(actorByName, -1, -1, null, null, 5);
				if (Game1.NPCGiftTastes.ContainsKey(split[1]) && !Game1.player.friendshipData.ContainsKey(split[1]))
				{
					Game1.player.friendshipData.Add(split[1], new Friendship(0));
				}
				if (num2 > 0)
				{
					actorByName.CurrentDialogue.Push(new Dialogue(eventCommands[CurrentCommand].Substring(num + 1, num2), actorByName));
				}
				else
				{
					actorByName.CurrentDialogue.Push(new Dialogue("...", actorByName));
				}
			}
			else
			{
				actorByName.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString(split[2]), actorByName));
			}
			Game1.drawDialogue(actorByName);
		}

		public virtual void command_beginSimultaneousCommand(GameLocation location, GameTime time, string[] split)
		{
			simultaneousCommand = true;
			CurrentCommand++;
		}

		public virtual void command_endSimultaneousCommand(GameLocation location, GameTime time, string[] split)
		{
			simultaneousCommand = false;
			CurrentCommand++;
		}

		public virtual void command_minedeath(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.dialogueUp)
			{
				return;
			}
			Random random = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + Game1.timeOfDay);
			int val = random.Next(Game1.player.Money / 20, Game1.player.Money / 4);
			val = Math.Min(val, 5000);
			val -= (int)((double)Game1.player.LuckLevel * 0.01 * (double)val);
			val -= val % 100;
			int num = 0;
			double num2 = 0.25 - (double)Game1.player.LuckLevel * 0.05 - Game1.player.DailyLuck;
			Game1.player.itemsLostLastDeath.Clear();
			for (int num3 = Game1.player.Items.Count - 1; num3 >= 0; num3--)
			{
				if (Game1.player.Items[num3] != null && (!(Game1.player.Items[num3] is Tool) || (Game1.player.Items[num3] is MeleeWeapon && (Game1.player.Items[num3] as MeleeWeapon).InitialParentTileIndex != 47 && (Game1.player.Items[num3] as MeleeWeapon).InitialParentTileIndex != 4)) && Game1.player.Items[num3].canBeTrashed() && !(Game1.player.Items[num3] is Ring) && random.NextDouble() < num2)
				{
					Item item = Game1.player.Items[num3];
					Game1.player.Items[num3] = null;
					num++;
					Game1.player.itemsLostLastDeath.Add(item);
				}
				if (num >= 5)
				{
					break;
				}
			}
			Game1.player.Stamina = Math.Min(Game1.player.Stamina, 2f);
			Game1.player.Money = Math.Max(0, Game1.player.Money - val);
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1057") + " " + ((val <= 0) ? "" : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1058", val)) + ((num <= 0) ? ((val <= 0) ? "" : ".") : ((val <= 0) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1060") + ((num == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1061") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1062", num))) : (Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1063") + ((num == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1061") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1062", num))))));
			List<string> list = eventCommands.ToList();
			list.Insert(CurrentCommand + 1, "showItemsLost");
			eventCommands = list.ToArray();
		}

		public virtual void command_hospitaldeath(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.dialogueUp)
			{
				return;
			}
			Random random = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + Game1.timeOfDay);
			int num = 0;
			double num2 = 0.25 - (double)Game1.player.LuckLevel * 0.05 - Game1.player.DailyLuck;
			Game1.player.itemsLostLastDeath.Clear();
			for (int num3 = Game1.player.Items.Count - 1; num3 >= 0; num3--)
			{
				if (Game1.player.Items[num3] != null && (!(Game1.player.Items[num3] is Tool) || (Game1.player.Items[num3] is MeleeWeapon && (Game1.player.Items[num3] as MeleeWeapon).InitialParentTileIndex != 47 && (Game1.player.Items[num3] as MeleeWeapon).InitialParentTileIndex != 4)) && Game1.player.Items[num3].canBeTrashed() && !(Game1.player.Items[num3] is Ring) && random.NextDouble() < num2)
				{
					Item item = Game1.player.Items[num3];
					Game1.player.Items[num3] = null;
					num++;
					Game1.player.itemsLostLastDeath.Add(item);
				}
			}
			Game1.player.Stamina = Math.Min(Game1.player.Stamina, 2f);
			int num4 = Math.Min(1000, Game1.player.Money);
			Game1.player.Money -= num4;
			Game1.drawObjectDialogue(((num4 > 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1068", num4) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1070")) + ((num > 0) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1071") + ((num == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1061") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1062", num))) : ""));
			List<string> list = eventCommands.ToList();
			list.Insert(CurrentCommand + 1, "showItemsLost");
			eventCommands = list.ToArray();
		}

		public virtual void command_showItemsLost(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.activeClickableMenu == null)
			{
				Game1.activeClickableMenu = new ItemListMenu(Game1.content.LoadString("Strings\\UI:ItemList_ItemsLost"), Game1.player.itemsLostLastDeath.ToList());
			}
		}

		public virtual void command_end(GameLocation location, GameTime time, string[] split)
		{
			endBehaviors(split, location);
		}

		public virtual void command_locationSpecificCommand(GameLocation location, GameTime time, string[] split)
		{
			if (split.Length > 1)
			{
				if (location.RunLocationSpecificEventCommand(this, split[1], !_repeatingLocationSpecificCommand, split.Skip(2).ToArray()))
				{
					_repeatingLocationSpecificCommand = false;
					CurrentCommand++;
				}
				else
				{
					_repeatingLocationSpecificCommand = true;
				}
			}
		}

		public virtual void command_unskippable(GameLocation location, GameTime time, string[] split)
		{
			skippable = false;
			CurrentCommand++;
		}

		public virtual void command_skippable(GameLocation location, GameTime time, string[] split)
		{
			skippable = true;
			CurrentCommand++;
		}

		public virtual void command_emote(GameLocation location, GameTime time, string[] split)
		{
			bool flag = split.Length > 3;
			if (split[1].Contains("farmer"))
			{
				Farmer farmerFromFarmerNumberString = getFarmerFromFarmerNumberString(split[1], farmer);
				if (farmerFromFarmerNumberString != null)
				{
					farmer.doEmote(Convert.ToInt32(split[2]), !flag);
				}
			}
			else
			{
				NPC actorByName = getActorByName(split[1]);
				if (!actorByName.isEmoting)
				{
					actorByName.doEmote(Convert.ToInt32(split[2]), !flag);
				}
			}
			if (flag)
			{
				CurrentCommand++;
				checkForNextCommand(location, time);
			}
		}

		public virtual void command_stopMusic(GameLocation location, GameTime time, string[] split)
		{
			Game1.changeMusicTrack("none", track_interruptable: false, Game1.MusicContext.Event);
			CurrentCommand++;
		}

		public virtual void command_playSound(GameLocation location, GameTime time, string[] split)
		{
			Game1.playSound(split[1]);
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_tossConcession(GameLocation location, GameTime time, string[] split)
		{
			NPC actorByName = getActorByName(split[1]);
			int tilePosition = int.Parse(split[2]);
			Game1.playSound("dwop");
			location.temporarySprites.Add(new TemporaryAnimatedSprite
			{
				texture = Game1.concessionsSpriteSheet,
				sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.concessionsSpriteSheet, tilePosition, 16, 16),
				animationLength = 1,
				totalNumberOfLoops = 1,
				motion = new Vector2(0f, -6f),
				acceleration = new Vector2(0f, 0.2f),
				interval = 1000f,
				scale = 4f,
				position = OffsetPosition(new Vector2(actorByName.Position.X, actorByName.Position.Y - 96f)),
				layerDepth = (float)actorByName.getStandingY() / 10000f
			});
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_pause(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.pauseTime <= 0f)
			{
				Game1.pauseTime = Convert.ToInt32(split[1]);
			}
		}

		public virtual void command_resetVariable(GameLocation location, GameTime time, string[] split)
		{
			specialEventVariable1 = false;
			currentCommand++;
		}

		public virtual void command_faceDirection(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Contains("farmer"))
			{
				Farmer farmerFromFarmerNumberString = getFarmerFromFarmerNumberString(split[1], farmer);
				if (farmerFromFarmerNumberString != null)
				{
					farmerFromFarmerNumberString.FarmerSprite.StopAnimation();
					farmerFromFarmerNumberString.completelyStopAnimatingOrDoingAction();
					farmerFromFarmerNumberString.faceDirection(Convert.ToInt32(split[2]));
				}
			}
			else if (split[1].Contains("spouse"))
			{
				if (Game1.player.spouse != null && Game1.player.spouse.Length > 0 && getActorByName(Game1.player.spouse) != null && !Game1.player.isRoommate(Game1.player.spouse))
				{
					getActorByName(Game1.player.spouse).faceDirection(Convert.ToInt32(split[2]));
				}
			}
			else
			{
				getActorByName(split[1])?.faceDirection(Convert.ToInt32(split[2]));
			}
			if (split.Length == 3 && Game1.pauseTime <= 0f)
			{
				Game1.pauseTime = 500f;
			}
			else if (split.Length > 3)
			{
				CurrentCommand++;
				checkForNextCommand(location, time);
			}
		}

		public virtual void command_warp(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Contains("farmer"))
			{
				Farmer farmerFromFarmerNumberString = getFarmerFromFarmerNumberString(split[1], farmer);
				if (farmerFromFarmerNumberString != null)
				{
					farmerFromFarmerNumberString.setTileLocation(OffsetTile(new Vector2(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]))));
					farmerFromFarmerNumberString.position.Y -= 16f;
					if (farmerActors.Contains(farmerFromFarmerNumberString))
					{
						farmerFromFarmerNumberString.completelyStopAnimatingOrDoingAction();
					}
				}
			}
			else if (split[1].Contains("spouse"))
			{
				if (Game1.player.spouse != null && Game1.player.spouse.Length > 0 && getActorByName(Game1.player.spouse) != null && !Game1.player.isRoommate(Game1.player.spouse))
				{
					if (npcControllers != null)
					{
						for (int num = npcControllers.Count - 1; num >= 0; num--)
						{
							if (npcControllers[num].puppet.Name.Equals(Game1.player.spouse))
							{
								npcControllers.RemoveAt(num);
							}
						}
					}
					getActorByName(Game1.player.spouse).Position = OffsetPosition(new Vector2(Convert.ToInt32(split[2]) * 64, Convert.ToInt32(split[3]) * 64));
				}
			}
			else
			{
				NPC actorByName = getActorByName(split[1]);
				if (actorByName != null)
				{
					actorByName.position.X = OffsetPositionX(Convert.ToInt32(split[2]) * 64 + 4);
					actorByName.position.Y = OffsetPositionY(Convert.ToInt32(split[3]) * 64);
				}
			}
			CurrentCommand++;
			if (split.Length > 4)
			{
				checkForNextCommand(location, time);
			}
		}

		public virtual void command_speed(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Equals("farmer"))
			{
				farmerAddedSpeed = Convert.ToInt32(split[2]);
			}
			else
			{
				getActorByName(split[1]).speed = Convert.ToInt32(split[2]);
			}
			CurrentCommand++;
		}

		public virtual void command_stopAdvancedMoves(GameLocation location, GameTime time, string[] split)
		{
			if (split.Count() > 1)
			{
				if (split[1].Equals("next"))
				{
					foreach (NPCController npcController in npcControllers)
					{
						npcController.destroyAtNextCrossroad();
					}
				}
			}
			else
			{
				npcControllers.Clear();
			}
			CurrentCommand++;
		}

		public virtual void command_doAction(GameLocation location, GameTime time, string[] split)
		{
			Location tile_location = new Location(OffsetTileX(Convert.ToInt32(split[1])), OffsetTileY(Convert.ToInt32(split[2])));
			Game1.hooks.OnGameLocation_CheckAction(location, tile_location, Game1.viewport, farmer, () => location.checkAction(tile_location, Game1.viewport, farmer));
			CurrentCommand++;
		}

		public virtual void command_removeTile(GameLocation location, GameTime time, string[] split)
		{
			location.removeTile(OffsetTileX(Convert.ToInt32(split[1])), OffsetTileY(Convert.ToInt32(split[2])), split[3]);
			CurrentCommand++;
		}

		public virtual void command_textAboveHead(GameLocation location, GameTime time, string[] split)
		{
			NPC actorByName = getActorByName(split[1]);
			if (actorByName != null)
			{
				int startIndex = eventCommands[CurrentCommand].IndexOf('"') + 1;
				int length = eventCommands[CurrentCommand].Substring(startIndex).LastIndexOf('"');
				actorByName.showTextAboveHead(eventCommands[CurrentCommand].Substring(startIndex, length));
			}
			CurrentCommand++;
		}

		public virtual void command_showFrame(GameLocation location, GameTime time, string[] split)
		{
			if (split.Length > 2 && !split[2].Equals("flip") && !split[1].Contains("farmer"))
			{
				NPC actorByName = getActorByName(split[1]);
				if (actorByName != null)
				{
					int num = Convert.ToInt32(split[2]);
					if (split[1].Equals("spouse") && actorByName.Gender == 0 && num >= 36 && num <= 38)
					{
						num += 12;
					}
					actorByName.Sprite.CurrentFrame = num;
				}
			}
			else
			{
				Farmer farmerFromFarmerNumberString = getFarmerFromFarmerNumberString(split[1], farmer);
				if (split.Length == 2)
				{
					farmerFromFarmerNumberString = farmer;
				}
				if (farmerFromFarmerNumberString != null)
				{
					if (split.Length > 2)
					{
						split[1] = split[2];
					}
					List<FarmerSprite.AnimationFrame> list = new List<FarmerSprite.AnimationFrame>();
					list.Add(new FarmerSprite.AnimationFrame(Convert.ToInt32(split[1]), 100, secondaryArm: false, split.Length > 2));
					farmerFromFarmerNumberString.FarmerSprite.setCurrentAnimation(list.ToArray());
					farmerFromFarmerNumberString.FarmerSprite.loop = true;
					farmerFromFarmerNumberString.FarmerSprite.loopThisAnimation = true;
					farmerFromFarmerNumberString.FarmerSprite.PauseForSingleAnimation = true;
					farmerFromFarmerNumberString.Sprite.currentFrame = Convert.ToInt32(split[1]);
				}
			}
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_farmerAnimation(GameLocation location, GameTime time, string[] split)
		{
			farmer.FarmerSprite.setCurrentSingleAnimation(Convert.ToInt32(split[1]));
			farmer.FarmerSprite.PauseForSingleAnimation = true;
			CurrentCommand++;
		}

		public virtual void command_ignoreMovementAnimation(GameLocation location, GameTime time, string[] split)
		{
			bool ignoreMovementAnimation = true;
			if (split.Length > 2)
			{
				split[2].Equals("true");
			}
			if (split[1].Contains("farmer"))
			{
				Farmer farmerFromFarmerNumberString = getFarmerFromFarmerNumberString(split[1], farmer);
				if (farmerFromFarmerNumberString != null)
				{
					farmerFromFarmerNumberString.ignoreMovementAnimation = ignoreMovementAnimation;
				}
			}
			else
			{
				NPC actorByName = getActorByName(split[1].Replace('_', ' '));
				if (actorByName != null)
				{
					actorByName.ignoreMovementAnimation = ignoreMovementAnimation;
				}
			}
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_animate(GameLocation location, GameTime time, string[] split)
		{
			int milliseconds = Convert.ToInt32(split[4]);
			bool flip = split[2].Equals("true");
			bool flag = split[3].Equals("true");
			List<FarmerSprite.AnimationFrame> list = new List<FarmerSprite.AnimationFrame>();
			for (int i = 5; i < split.Length; i++)
			{
				list.Add(new FarmerSprite.AnimationFrame(Convert.ToInt32(split[i]), milliseconds, secondaryArm: false, flip));
			}
			if (split[1].Contains("farmer"))
			{
				Farmer farmerFromFarmerNumberString = getFarmerFromFarmerNumberString(split[1], farmer);
				if (farmerFromFarmerNumberString != null)
				{
					farmerFromFarmerNumberString.FarmerSprite.setCurrentAnimation(list.ToArray());
					farmerFromFarmerNumberString.FarmerSprite.loop = true;
					farmerFromFarmerNumberString.FarmerSprite.loopThisAnimation = flag;
					farmerFromFarmerNumberString.FarmerSprite.PauseForSingleAnimation = true;
				}
			}
			else
			{
				NPC actorByName = getActorByName(split[1].Replace('_', ' '));
				if (actorByName != null)
				{
					actorByName.Sprite.setCurrentAnimation(list);
					actorByName.Sprite.loop = flag;
				}
			}
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_stopAnimation(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Contains("farmer"))
			{
				Farmer farmerFromFarmerNumberString = getFarmerFromFarmerNumberString(split[1], farmer);
				if (farmerFromFarmerNumberString != null)
				{
					farmerFromFarmerNumberString.completelyStopAnimatingOrDoingAction();
					farmerFromFarmerNumberString.Halt();
					farmerFromFarmerNumberString.FarmerSprite.CurrentAnimation = null;
					switch (farmerFromFarmerNumberString.FacingDirection)
					{
					case 0:
						farmerFromFarmerNumberString.FarmerSprite.setCurrentSingleFrame(12, 32000);
						break;
					case 1:
						farmerFromFarmerNumberString.FarmerSprite.setCurrentSingleFrame(6, 32000);
						break;
					case 2:
						farmerFromFarmerNumberString.FarmerSprite.setCurrentSingleFrame(0, 32000);
						break;
					case 3:
						farmerFromFarmerNumberString.FarmerSprite.setCurrentSingleFrame(6, 32000, secondaryArm: false, flip: true);
						break;
					}
				}
			}
			else
			{
				NPC actorByName = getActorByName(split[1]);
				if (actorByName != null)
				{
					actorByName.Sprite.StopAnimation();
					if (split.Length > 2)
					{
						actorByName.Sprite.currentFrame = Convert.ToInt32(split[2]);
						actorByName.Sprite.UpdateSourceRect();
					}
				}
			}
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_showRivalFrame(GameLocation location, GameTime time, string[] split)
		{
			getActorByName("rival").Sprite.currentFrame = Convert.ToInt32(split[1]);
			CurrentCommand++;
		}

		public virtual void command_weddingSprite(GameLocation location, GameTime time, string[] split)
		{
			getActorByName("WeddingOutfits").Sprite.currentFrame = Convert.ToInt32(split[1]);
			CurrentCommand++;
		}

		public virtual void command_changeLocation(GameLocation location, GameTime time, string[] split)
		{
			changeLocation(split[1], farmer.getTileX(), farmer.getTileY(), -1, delegate
			{
				CurrentCommand++;
			});
		}

		public virtual void command_halt(GameLocation location, GameTime time, string[] split)
		{
			foreach (NPC actor in actors)
			{
				actor.Halt();
			}
			farmer.Halt();
			CurrentCommand++;
			continueAfterMove = false;
			actorPositionsAfterMove.Clear();
		}

		public virtual void command_message(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.dialogueUp && Game1.activeClickableMenu == null)
			{
				int num = eventCommands[CurrentCommand].IndexOf('"') + 1;
				int num2 = eventCommands[CurrentCommand].LastIndexOf('"');
				if (num2 > 0 && num2 > num)
				{
					Game1.drawDialogueNoTyping(Game1.parseText(eventCommands[CurrentCommand].Substring(num, num2 - num)));
				}
				else
				{
					Game1.drawDialogueNoTyping("...");
				}
			}
		}

		public virtual void command_addCookingRecipe(GameLocation location, GameTime time, string[] split)
		{
			Game1.player.cookingRecipes.Add(eventCommands[CurrentCommand].Substring(eventCommands[CurrentCommand].IndexOf(' ') + 1), 0);
			CurrentCommand++;
		}

		public virtual void command_itemAboveHead(GameLocation location, GameTime time, string[] split)
		{
			if (split.Length > 1 && split[1].Equals("pan"))
			{
				farmer.holdUpItemThenMessage(new Pan());
			}
			else if (split.Length > 1 && split[1].Equals("hero"))
			{
				farmer.holdUpItemThenMessage(new Object(Vector2.Zero, 116));
			}
			else if (split.Length > 1 && split[1].Equals("sculpture"))
			{
				farmer.holdUpItemThenMessage(new Furniture(1306, Vector2.Zero));
			}
			else if (split.Length > 1 && split[1].Equals("samBoombox"))
			{
				farmer.holdUpItemThenMessage(new Furniture(1309, Vector2.Zero));
			}
			else if (split.Length > 1 && split[1].Equals("joja"))
			{
				farmer.holdUpItemThenMessage(new Object(Vector2.Zero, 117));
			}
			else if (split.Length > 1 && split[1].Equals("slimeEgg"))
			{
				farmer.holdUpItemThenMessage(new Object(680, 1));
			}
			else if (split.Length > 1 && split[1].Equals("rod"))
			{
				farmer.holdUpItemThenMessage(new FishingRod());
			}
			else if (split.Length > 1 && split[1].Equals("sword"))
			{
				farmer.holdUpItemThenMessage(new MeleeWeapon(0));
			}
			else if (split.Length > 1 && split[1].Equals("ore"))
			{
				farmer.holdUpItemThenMessage(new Object(378, 1), showMessage: false);
			}
			else if (split.Length > 1 && split[1].Equals("pot"))
			{
				farmer.holdUpItemThenMessage(new Object(Vector2.Zero, 62), showMessage: false);
			}
			else if (split.Length > 1 && split[1].Equals("jukebox"))
			{
				farmer.holdUpItemThenMessage(new Object(Vector2.Zero, 209), showMessage: false);
			}
			else
			{
				farmer.holdUpItemThenMessage(null, showMessage: false);
			}
			CurrentCommand++;
		}

		public virtual void command_addCraftingRecipe(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.player.craftingRecipes.ContainsKey(eventCommands[CurrentCommand].Substring(eventCommands[CurrentCommand].IndexOf(' ') + 1)))
			{
				Game1.player.craftingRecipes.Add(eventCommands[CurrentCommand].Substring(eventCommands[CurrentCommand].IndexOf(' ') + 1), 0);
			}
			CurrentCommand++;
		}

		public virtual void command_hostMail(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.IsMasterGame && !Game1.player.hasOrWillReceiveMail(split[1]))
			{
				Game1.addMailForTomorrow(split[1]);
			}
			CurrentCommand++;
		}

		public virtual void command_mail(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.player.hasOrWillReceiveMail(split[1]))
			{
				Game1.addMailForTomorrow(split[1]);
			}
			CurrentCommand++;
		}

		public virtual void command_shake(GameLocation location, GameTime time, string[] split)
		{
			getActorByName(split[1]).shake(Convert.ToInt32(split[2]));
			CurrentCommand++;
		}

		public virtual void command_temporarySprite(GameLocation location, GameTime time, string[] split)
		{
			location.TemporarySprites.Add(new TemporaryAnimatedSprite(Convert.ToInt32(split[3]), OffsetPosition(new Vector2(Convert.ToInt32(split[1]) * 64, Convert.ToInt32(split[2]) * 64)), Color.White, Convert.ToInt32(split[4]), split.Length > 6 && split[6] == "true", (split.Length > 5) ? ((float)Convert.ToInt32(split[5])) : 300f, 0, 64, (split.Length > 7) ? ((float)Convert.ToDouble(split[7])) : (-1f)));
			CurrentCommand++;
		}

		public virtual void command_removeTemporarySprites(GameLocation location, GameTime time, string[] split)
		{
			location.TemporarySprites.Clear();
			CurrentCommand++;
		}

		public virtual void command_null(GameLocation location, GameTime time, string[] split)
		{
		}

		public virtual void command_specificTemporarySprite(GameLocation location, GameTime time, string[] split)
		{
			addSpecificTemporarySprite(split[1], location, split);
			CurrentCommand++;
		}

		public virtual void command_playMusic(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Equals("samBand"))
			{
				if (Game1.player.DialogueQuestionsAnswered.Contains(78))
				{
					Game1.changeMusicTrack("shimmeringbastion", track_interruptable: false, Game1.MusicContext.Event);
				}
				else if (Game1.player.DialogueQuestionsAnswered.Contains(79))
				{
					Game1.changeMusicTrack("honkytonky", track_interruptable: false, Game1.MusicContext.Event);
				}
				else if (Game1.player.DialogueQuestionsAnswered.Contains(77))
				{
					Game1.changeMusicTrack("heavy", track_interruptable: false, Game1.MusicContext.Event);
				}
				else
				{
					Game1.changeMusicTrack("poppy", track_interruptable: false, Game1.MusicContext.Event);
				}
			}
			else if (Game1.options.musicVolumeLevel > 0f)
			{
				StringBuilder stringBuilder = new StringBuilder(split[1]);
				for (int i = 2; i < split.Length; i++)
				{
					stringBuilder.Append(" " + split[i]);
				}
				Game1.changeMusicTrack(stringBuilder.ToString(), track_interruptable: false, Game1.MusicContext.Event);
			}
			CurrentCommand++;
		}

		public virtual void command_nameSelect(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.nameSelectUp)
			{
				Game1.showNameSelectScreen(split[1]);
			}
		}

		public virtual void command_makeInvisible(GameLocation location, GameTime time, string[] split)
		{
			if (split.Count() == 3)
			{
				int x = OffsetTileX(Convert.ToInt32(split[1]));
				int y = OffsetTileY(Convert.ToInt32(split[2]));
				Object @object = null;
				@object = location.getObjectAtTile(x, y);
				if (@object != null)
				{
					@object.isTemporarilyInvisible = true;
				}
			}
			else
			{
				int num = OffsetTileX(Convert.ToInt32(split[1]));
				int num2 = OffsetTileY(Convert.ToInt32(split[2]));
				int num3 = Convert.ToInt32(split[3]);
				int num4 = Convert.ToInt32(split[4]);
				Object object2 = null;
				for (int i = num; i < num + num3; i++)
				{
					for (int j = num2; j < num2 + num4; j++)
					{
						object2 = location.getObjectAtTile(i, j);
						if (object2 != null)
						{
							object2.isTemporarilyInvisible = true;
						}
						else if (location.terrainFeatures.ContainsKey(new Vector2(i, j)))
						{
							location.terrainFeatures[new Vector2(i, j)].isTemporarilyInvisible = true;
						}
					}
				}
			}
			CurrentCommand++;
		}

		public virtual void command_addObject(GameLocation location, GameTime time, string[] split)
		{
			float layerDepth = (float)(OffsetTileY(Convert.ToInt32(split[2])) * 64) / 10000f;
			if (split.Length > 4)
			{
				layerDepth = Convert.ToSingle(split[4]);
			}
			location.TemporarySprites.Add(new TemporaryAnimatedSprite(Convert.ToInt32(split[3]), 9999f, 1, 9999, OffsetPosition(new Vector2(Convert.ToInt32(split[1]), Convert.ToInt32(split[2])) * 64f), flicker: false, flipped: false)
			{
				layerDepth = layerDepth
			});
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_addBigProp(GameLocation location, GameTime time, string[] split)
		{
			props.Add(new Object(OffsetTile(new Vector2(Convert.ToInt32(split[1]), Convert.ToInt32(split[2]))), Convert.ToInt32(split[3])));
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_addFloorProp(GameLocation location, GameTime time, string[] split)
		{
			command_addProp(location, time, split);
		}

		public virtual void command_addProp(GameLocation location, GameTime time, string[] split)
		{
			int num = OffsetTileX(Convert.ToInt32(split[2]));
			int num2 = OffsetTileY(Convert.ToInt32(split[3]));
			int index = Convert.ToInt32(split[1]);
			int tilesWideSolid = ((split.Length <= 4) ? 1 : Convert.ToInt32(split[4]));
			int num3 = ((split.Length <= 5) ? 1 : Convert.ToInt32(split[5]));
			int tilesHighSolid = ((split.Length > 6) ? Convert.ToInt32(split[6]) : num3);
			bool solid = !split[0].Contains("Floor");
			festivalProps.Add(new Prop(festivalTexture, index, tilesWideSolid, tilesHighSolid, num3, num, num2, solid));
			if (split.Length > 7)
			{
				int num4 = Convert.ToInt32(split[7]);
				for (int num5 = num + num4; num5 != num; num5 -= Math.Sign(num4))
				{
					festivalProps.Add(new Prop(festivalTexture, index, tilesWideSolid, tilesHighSolid, num3, num5, num2, solid));
				}
			}
			if (split.Length > 8)
			{
				int num6 = Convert.ToInt32(split[8]);
				for (int num7 = num2 + num6; num7 != num2; num7 -= Math.Sign(num6))
				{
					festivalProps.Add(new Prop(festivalTexture, index, tilesWideSolid, tilesHighSolid, num3, num, num7, solid));
				}
			}
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_addToTable(GameLocation location, GameTime time, string[] split)
		{
			if (location is FarmHouse)
			{
				(location as FarmHouse).furniture[0].heldObject.Value = new Object(Vector2.Zero, Convert.ToInt32(split[3]), 1);
			}
			else
			{
				location.objects[OffsetTile(new Vector2(Convert.ToInt32(split[1]), Convert.ToInt32(split[2])))].heldObject.Value = new Object(Vector2.Zero, Convert.ToInt32(split[3]), 1);
			}
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_removeObject(GameLocation location, GameTime time, string[] split)
		{
			Vector2 other = OffsetPosition(new Vector2(Convert.ToInt32(split[1]), Convert.ToInt32(split[2])) * 64f);
			for (int num = location.temporarySprites.Count - 1; num >= 0; num--)
			{
				if (location.temporarySprites[num].position.Equals(other))
				{
					location.temporarySprites.RemoveAt(num);
					break;
				}
			}
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_glow(GameLocation location, GameTime time, string[] split)
		{
			bool hold = false;
			if (split.Length > 4 && split[4].Equals("true"))
			{
				hold = true;
			}
			Game1.screenGlowOnce(new Color(Convert.ToInt32(split[1]), Convert.ToInt32(split[2]), Convert.ToInt32(split[3])), hold);
			CurrentCommand++;
		}

		public virtual void command_stopGlowing(GameLocation location, GameTime time, string[] split)
		{
			Game1.screenGlowUp = false;
			Game1.screenGlowHold = false;
			CurrentCommand++;
		}

		public virtual void command_addQuest(GameLocation location, GameTime time, string[] split)
		{
			Game1.player.addQuest(Convert.ToInt32(split[1]));
			CurrentCommand++;
		}

		public virtual void command_removeQuest(GameLocation location, GameTime time, string[] split)
		{
			Game1.player.removeQuest(Convert.ToInt32(split[1]));
			CurrentCommand++;
		}

		public virtual void command_awardFestivalPrize(GameLocation location, GameTime time, string[] split)
		{
			if (festivalWinners.Contains(Game1.player.UniqueMultiplayerID))
			{
				string text = festivalData["file"];
				if (!(text == "spring13"))
				{
					if (!(text == "winter8"))
					{
						return;
					}
					if (!Game1.player.mailReceived.Contains("Ice Festival"))
					{
						if (Game1.activeClickableMenu == null)
						{
							Game1.activeClickableMenu = new ItemGrabMenu(new List<Item>
							{
								new Hat(17),
								new Object(687, 1),
								new Object(691, 1),
								new Object(703, 1)
							}, this).setEssential(essential: true);
						}
						Game1.player.mailReceived.Add("Ice Festival");
						CurrentCommand++;
					}
					else
					{
						Game1.player.Money += 2000;
						Game1.playSound("money");
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1164"));
						CurrentCommand += 2;
					}
				}
				else if (!Game1.player.mailReceived.Contains("Egg Festival"))
				{
					if (Game1.activeClickableMenu == null)
					{
						Game1.player.addItemByMenuIfNecessary(new Hat(4));
					}
					Game1.player.mailReceived.Add("Egg Festival");
					CurrentCommand++;
					if (Game1.activeClickableMenu == null)
					{
						CurrentCommand++;
					}
				}
				else
				{
					Game1.player.Money += 1000;
					Game1.playSound("money");
					CurrentCommand += 2;
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1159"));
				}
			}
			else if (split.Length > 1)
			{
				string text2 = split[1].ToLower();
				if (text2 == null)
				{
					return;
				}
				switch (text2.Length)
				{
				case 12:
					switch (text2[0])
					{
					case 'b':
						if (text2 == "birdiereward")
						{
							List<Item> list = new List<Item>();
							Game1.player.team.RequestLimitedNutDrops("Birdie", null, 0, 0, 5, 5);
							if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotBirdieReward"))
							{
								Game1.addMailForTomorrow("gotBirdieReward", noLetter: true, sendToEveryone: true);
							}
							CurrentCommand++;
							CurrentCommand++;
						}
						break;
					case 'e':
						if (text2 == "emilyclothes")
						{
							Clothing clothing = new Clothing(8);
							clothing.Dye(new Color(0, 143, 239), 1f);
							Game1.player.addItemsByMenuIfNecessary(new List<Item>
							{
								new Boots(804),
								new Hat(41),
								new Clothing(1127),
								clothing
							});
							if (Game1.activeClickableMenu == null)
							{
								CurrentCommand++;
							}
							CurrentCommand++;
						}
						break;
					}
					break;
				case 7:
					switch (text2[0])
					{
					case 'm':
						if (text2 == "memento")
						{
							Object @object = new Object(864, 1)
							{
								specialItem = true
							};
							@object.questItem.Value = true;
							Game1.player.addItemByMenuIfNecessary(@object);
							if (Game1.activeClickableMenu == null)
							{
								CurrentCommand++;
							}
							CurrentCommand++;
						}
						break;
					case 'j':
						if (text2 == "jukebox")
						{
							Game1.player.addItemByMenuIfNecessary(new Object(Vector2.Zero, 209));
							if (Game1.activeClickableMenu == null)
							{
								CurrentCommand++;
							}
							CurrentCommand++;
						}
						break;
					}
					break;
				case 3:
					switch (text2[2])
					{
					case 'n':
						if (text2 == "pan")
						{
							Game1.player.addItemByMenuIfNecessary(new Pan());
							if (Game1.activeClickableMenu == null)
							{
								CurrentCommand++;
							}
							CurrentCommand++;
						}
						break;
					case 'd':
						if (text2 == "rod")
						{
							Game1.player.addItemByMenuIfNecessary(new FishingRod());
							if (Game1.activeClickableMenu == null)
							{
								CurrentCommand++;
							}
							CurrentCommand++;
						}
						break;
					case 't':
						if (text2 == "pot")
						{
							Game1.player.addItemByMenuIfNecessary(new Object(Vector2.Zero, 62));
							if (Game1.activeClickableMenu == null)
							{
								CurrentCommand++;
							}
							CurrentCommand++;
						}
						break;
					}
					break;
				case 4:
					switch (text2[0])
					{
					case 'h':
						if (text2 == "hero")
						{
							Game1.getSteamAchievement("Achievement_LocalLegend");
							Game1.player.addItemByMenuIfNecessary(new Object(Vector2.Zero, 116));
							if (Game1.activeClickableMenu == null)
							{
								CurrentCommand++;
							}
							CurrentCommand++;
						}
						break;
					case 'j':
						if (text2 == "joja")
						{
							Game1.getSteamAchievement("Achievement_Joja");
							Game1.player.addItemByMenuIfNecessary(new Object(Vector2.Zero, 117));
							if (Game1.activeClickableMenu == null)
							{
								CurrentCommand++;
							}
							CurrentCommand++;
						}
						break;
					}
					break;
				case 6:
					if (text2 == "qimilk")
					{
						if (!Game1.player.mailReceived.Contains("qiCave"))
						{
							Game1.player.maxHealth += 25;
							Game1.player.mailReceived.Add("qiCave");
						}
						CurrentCommand++;
					}
					break;
				case 9:
					if (text2 == "sculpture")
					{
						Game1.player.addItemByMenuIfNecessary(new Furniture(1306, Vector2.Zero));
						if (Game1.activeClickableMenu == null)
						{
							CurrentCommand++;
						}
						CurrentCommand++;
					}
					break;
				case 10:
					if (text2 == "samboombox")
					{
						Game1.player.addItemByMenuIfNecessary(new Furniture(1309, Vector2.Zero));
						if (Game1.activeClickableMenu == null)
						{
							CurrentCommand++;
						}
						CurrentCommand++;
					}
					break;
				case 14:
					if (text2 == "marniepainting")
					{
						Game1.player.addItemByMenuIfNecessary(new Furniture(1802, Vector2.Zero));
						if (Game1.activeClickableMenu == null)
						{
							CurrentCommand++;
						}
						CurrentCommand++;
					}
					break;
				case 5:
					if (text2 == "sword")
					{
						Game1.player.addItemByMenuIfNecessary(new MeleeWeapon(0));
						TutorialManager.Instance.triggerAttackChoice();
						if (Game1.activeClickableMenu == null)
						{
							CurrentCommand++;
						}
						CurrentCommand++;
					}
					break;
				case 8:
					if (text2 == "slimeegg")
					{
						Game1.player.addItemByMenuIfNecessary(new Object(680, 1));
						if (Game1.activeClickableMenu == null)
						{
							CurrentCommand++;
						}
						CurrentCommand++;
					}
					break;
				case 11:
				case 13:
					break;
				}
			}
			else
			{
				CurrentCommand += 2;
			}
		}

		public virtual void command_attachCharacterToTempSprite(GameLocation location, GameTime time, string[] split)
		{
			TemporaryAnimatedSprite temporaryAnimatedSprite = location.temporarySprites.Last();
			if (temporaryAnimatedSprite != null)
			{
				temporaryAnimatedSprite.attachedCharacter = getActorByName(split[1]);
			}
		}

		public virtual void command_fork(GameLocation location, GameTime time, string[] split)
		{
			if (split.Length > 2)
			{
				if (Game1.player.mailReceived.Contains(split[1]) || (int.TryParse(split[1], out var result) && Game1.player.dialogueQuestionsAnswered.Contains(result)))
				{
					string[] array = (eventCommands = ((split.Length <= 3) ? Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + Game1.currentLocation.Name)[split[2]].Split('/') : Game1.content.LoadString(split[2]).Split('/')));
					CurrentCommand = 0;
					forked = !forked;
				}
				else
				{
					CurrentCommand++;
				}
			}
			else if (specialEventVariable1)
			{
				string[] array2 = (eventCommands = (isFestival ? festivalData[split[1]].Split('/') : Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + Game1.currentLocation.Name)[split[1]].Split('/')));
				CurrentCommand = 0;
				forked = !forked;
			}
			else
			{
				CurrentCommand++;
			}
		}

		public virtual void command_switchEvent(GameLocation location, GameTime time, string[] split)
		{
			string[] array = (eventCommands = ((!isFestival) ? Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + Game1.currentLocation.Name)[split[1]].Split('/') : festivalData[split[1]].Split('/')));
			CurrentCommand = 0;
			eventSwitched = true;
		}

		public virtual void command_globalFade(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.globalFade)
			{
				if (split.Length > 2)
				{
					Game1.globalFadeToBlack(null, (split.Length > 1) ? ((float)Convert.ToDouble(split[1])) : 0.007f);
					CurrentCommand++;
				}
				else
				{
					Game1.globalFadeToBlack(incrementCommandAfterFade, (split.Length > 1) ? ((float)Convert.ToDouble(split[1])) : 0.007f);
				}
			}
		}

		public virtual void command_globalFadeToClear(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.globalFade)
			{
				if (split.Length > 2)
				{
					Game1.globalFadeToClear(null, (split.Length > 1) ? ((float)Convert.ToDouble(split[1])) : 0.007f);
					CurrentCommand++;
				}
				else
				{
					Game1.globalFadeToClear(incrementCommandAfterFade, (split.Length > 1) ? ((float)Convert.ToDouble(split[1])) : 0.007f);
				}
			}
		}

		public virtual void command_cutscene(GameLocation location, GameTime time, string[] split)
		{
			if (currentCustomEventScript != null)
			{
				if (currentCustomEventScript.update(time, this))
				{
					currentCustomEventScript = null;
					CurrentCommand++;
				}
			}
			else
			{
				if (Game1.currentMinigame != null)
				{
					return;
				}
				string text = split[1];
				if (text != null)
				{
					switch (text.Length)
					{
					case 8:
						switch (text[0])
						{
						case 'g':
							if (text == "greenTea")
							{
								currentCustomEventScript = new EventScript_GreenTea(new Vector2(-64000f, -64000f), this);
							}
							break;
						case 'b':
						{
							if (!(text == "bandFork"))
							{
								break;
							}
							int answerChoice = 76;
							if (Game1.player.dialogueQuestionsAnswered.Contains(77))
							{
								answerChoice = 77;
							}
							else if (Game1.player.dialogueQuestionsAnswered.Contains(78))
							{
								answerChoice = 78;
							}
							else if (Game1.player.dialogueQuestionsAnswered.Contains(79))
							{
								answerChoice = 79;
							}
							answerDialogue("bandFork", answerChoice);
							CurrentCommand++;
							return;
						}
						}
						break;
					case 9:
						switch (text[0])
						{
						case 'm':
							if (text == "marucomet")
							{
								Game1.currentMinigame = new MaruComet();
							}
							break;
						case 'h':
							if (text == "haleyCows")
							{
								Game1.currentMinigame = new HaleyCowPictures();
							}
							break;
						case 'b':
							if (text == "boardGame")
							{
								Game1.currentMinigame = new FantasyBoardGame();
								CurrentCommand++;
							}
							break;
						}
						break;
					case 5:
						switch (text[0])
						{
						case 'r':
							if (text == "robot")
							{
								Game1.currentMinigame = new RobotBlastoff();
							}
							break;
						case 'p':
							if (text == "plane")
							{
								Game1.currentMinigame = new PlaneFlyBy();
							}
							break;
						}
						break;
					case 13:
						switch (text[0])
						{
						case 'b':
						{
							if (!(text == "balloonDepart"))
							{
								break;
							}
							TemporaryAnimatedSprite temporarySpriteByID = location.getTemporarySpriteByID(1);
							temporarySpriteByID.attachedCharacter = farmer;
							temporarySpriteByID.motion = new Vector2(0f, -2f);
							temporarySpriteByID = location.getTemporarySpriteByID(2);
							temporarySpriteByID.attachedCharacter = getActorByName("Harvey");
							temporarySpriteByID.motion = new Vector2(0f, -2f);
							temporarySpriteByID = location.getTemporarySpriteByID(3);
							temporarySpriteByID.scaleChange = -0.01f;
							CurrentCommand++;
							return;
						}
						case 'e':
							if (!(text == "eggHuntWinner"))
							{
								break;
							}
							eggHuntWinner();
							CurrentCommand++;
							return;
						case 'g':
							if (!(text == "governorTaste"))
							{
								break;
							}
							governorTaste();
							currentCommand++;
							return;
						}
						break;
					case 16:
						switch (text[0])
						{
						case 'c':
							if (text == "clearTempSprites")
							{
								location.temporarySprites.Clear();
								CurrentCommand++;
							}
							break;
						case 'b':
							if (text == "balloonChangeMap")
							{
								eventPositionTileOffset = Vector2.Zero;
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1183, 84, 160), 10000f, 1, 99999, OffsetPosition(new Vector2(22f, 36f) * 64f + new Vector2(-23f, 0f) * 4f), flicker: false, flipped: false, 2E-05f, 0f, Color.White, 4f, 0f, 0f, 0f)
								{
									motion = new Vector2(0f, -2f),
									yStopCoordinate = (int)OffsetPositionY(576f),
									reachedStopCoordinate = balloonInSky,
									attachedCharacter = farmer,
									id = 1f
								});
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(84, 1205, 38, 26), 10000f, 1, 99999, OffsetPosition(new Vector2(22f, 36f) * 64f + new Vector2(0f, 134f) * 4f), flicker: false, flipped: false, 0.2625f, 0f, Color.White, 4f, 0f, 0f, 0f)
								{
									motion = new Vector2(0f, -2f),
									id = 2f,
									attachedCharacter = getActorByName("Harvey")
								});
								CurrentCommand++;
							}
							break;
						case 'i':
							if (!(text == "iceFishingWinner"))
							{
								break;
							}
							iceFishingWinner();
							currentCommand++;
							return;
						}
						break;
					case 18:
						switch (text[0])
						{
						case 'a':
						{
							if (!(text == "addSecretSantaItem"))
							{
								break;
							}
							Item giftFromNPC = Utility.getGiftFromNPC(mySecretSanta);
							Game1.player.addItemByMenuIfNecessaryElseHoldUp(giftFromNPC);
							currentCommand++;
							return;
						}
						case 'i':
							if (!(text == "iceFishingWinnerMP"))
							{
								break;
							}
							iceFishingWinnerMP();
							currentCommand++;
							return;
						}
						break;
					case 14:
						if (!(text == "linusMoneyGone"))
						{
							break;
						}
						foreach (TemporaryAnimatedSprite temporarySprite in location.temporarySprites)
						{
							temporarySprite.alphaFade = 0.01f;
							temporarySprite.motion = new Vector2(0f, -1f);
						}
						CurrentCommand++;
						return;
					case 11:
						if (text == "AbigailGame")
						{
							Game1.currentMinigame = new AbigailGame(playingWithAbby: true);
						}
						break;
					}
				}
				if (currentCommand != 59 || !(split[1] == "clearTempSprites") || !(FestivalName == "Feast of the Winter Star"))
				{
					Game1.globalFadeToClear(null, 0.01f);
				}
			}
		}

		public virtual void command_grabObject(GameLocation location, GameTime time, string[] split)
		{
			farmer.grabObject(new Object(Vector2.Zero, Convert.ToInt32(split[1]), null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false));
			showActiveObject = true;
			CurrentCommand++;
		}

		public virtual void command_addTool(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Equals("Sword"))
			{
				if (!Game1.player.addItemToInventoryBool(new Sword("Battered Sword", 67)))
				{
					Game1.player.addItemToInventoryBool(new Sword("Battered Sword", 67));
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1209")));
				}
				else
				{
					for (int i = 0; i < Game1.player.Items.Count(); i++)
					{
						if (Game1.player.Items[i] != null && Game1.player.Items[i] is Tool && Game1.player.Items[i].Name.Contains("Sword"))
						{
							Game1.player.CurrentToolIndex = i;
							Game1.switchToolAnimation();
							break;
						}
					}
				}
			}
			else if (split[1].Equals("Wand") && !Game1.player.addItemToInventoryBool(new Wand()))
			{
				Game1.player.addItemToInventoryBool(new Wand());
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1212")));
			}
			CurrentCommand++;
		}

		public virtual void command_waitForTempSprite(GameLocation location, GameTime time, string[] split)
		{
			int num = int.Parse(split[1]);
			if (Game1.currentLocation.getTemporarySpriteByID(num) != null)
			{
				CurrentCommand++;
			}
		}

		public virtual void command_waitForKey(GameLocation location, GameTime time, string[] split)
		{
			string text = split[1];
			KeyboardState keyboardState = Game1.GetKeyboardState();
			bool flag = false;
			if (!farmer.UsingTool && !Game1.pickingTool)
			{
				Keys[] pressedKeys = keyboardState.GetPressedKeys();
				foreach (Keys keys in pressedKeys)
				{
					if (Enum.GetName(keys.GetType(), keys).Equals(text.ToUpper()))
					{
						flag = true;
						switch (keys)
						{
						case Keys.Z:
							Game1.pressSwitchToolButton();
							break;
						case Keys.C:
							Game1.pressUseToolButton();
							farmer.EndUsingTool();
							break;
						case Keys.S:
							Game1.pressAddItemToInventoryButton();
							showActiveObject = false;
							farmer.showNotCarrying();
							break;
						}
						break;
					}
				}
			}
			int startIndex = eventCommands[CurrentCommand].IndexOf('"') + 1;
			int length = eventCommands[CurrentCommand].Substring(eventCommands[CurrentCommand].IndexOf('"') + 1).IndexOf('"');
			messageToScreen = eventCommands[CurrentCommand].Substring(startIndex, length);
			if (flag)
			{
				messageToScreen = null;
				CurrentCommand++;
			}
		}

		public virtual void command_cave(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.activeClickableMenu == null)
			{
				Response[] answerChoices = new Response[2]
				{
					new Response("Mushrooms", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1220")),
					new Response("Bats", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1222"))
				};
				Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1223"), answerChoices, "cave");
				Game1.dialogueTyping = false;
			}
		}

		public virtual void command_updateMinigame(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.currentMinigame != null)
			{
				Game1.currentMinigame.receiveEventPoke(Convert.ToInt32(split[1]));
			}
			CurrentCommand++;
		}

		public virtual void command_startJittering(GameLocation location, GameTime time, string[] split)
		{
			farmer.jitterStrength = 1f;
			CurrentCommand++;
		}

		public virtual void command_money(GameLocation location, GameTime time, string[] split)
		{
			farmer.Money += Convert.ToInt32(split[1]);
			if (farmer.Money < 0)
			{
				farmer.Money = 0;
			}
			CurrentCommand++;
		}

		public virtual void command_stopJittering(GameLocation location, GameTime time, string[] split)
		{
			farmer.stopJittering();
			CurrentCommand++;
		}

		public virtual void command_addLantern(GameLocation location, GameTime time, string[] split)
		{
			location.TemporarySprites.Add(new TemporaryAnimatedSprite(Convert.ToInt32(split[1]), 999999f, 1, 0, OffsetPosition(new Vector2(Convert.ToInt32(split[2]), Convert.ToInt32(split[3])) * 64f), flicker: false, flipped: false)
			{
				light = true,
				lightRadius = Convert.ToInt32(split[4])
			});
			CurrentCommand++;
		}

		public virtual void command_rustyKey(GameLocation location, GameTime time, string[] split)
		{
			Game1.player.hasRustyKey = true;
			CurrentCommand++;
		}

		public virtual void command_swimming(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Equals("farmer"))
			{
				farmer.bathingClothes.Value = true;
				farmer.swimming.Value = true;
			}
			else
			{
				getActorByName(split[1]).swimming.Value = true;
			}
			CurrentCommand++;
		}

		public virtual void command_stopSwimming(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Equals("farmer"))
			{
				farmer.bathingClothes.Value = location is BathHousePool;
				farmer.swimming.Value = false;
			}
			else
			{
				getActorByName(split[1]).swimming.Value = false;
			}
			CurrentCommand++;
		}

		public virtual void command_tutorialMenu(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.activeClickableMenu == null)
			{
				Game1.activeClickableMenu = new TutorialMenu();
			}
		}

		public virtual void command_animalNaming(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.activeClickableMenu == null)
			{
				Game1.activeClickableMenu = new NamingMenu(delegate(string animal_name)
				{
					(Game1.currentLocation as AnimalHouse).addNewHatchedAnimal(animal_name);
					CurrentCommand++;
				}, Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1236"));
			}
		}

		public virtual void command_splitSpeak(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.dialogueUp)
			{
				return;
			}
			timeAccumulator += time.ElapsedGameTime.Milliseconds;
			if (!(timeAccumulator < 500f))
			{
				timeAccumulator = 0f;
				int startIndex = eventCommands[CurrentCommand].IndexOf('"') + 1;
				int length = eventCommands[CurrentCommand].Substring(eventCommands[CurrentCommand].IndexOf('"') + 1).IndexOf('"');
				string[] array = eventCommands[CurrentCommand].Substring(startIndex, length).Split('~');
				NPC nPC = getActorByName(split[1]);
				if (nPC == null)
				{
					nPC = Game1.getCharacterFromName(split[1].Equals("rival") ? Utility.getOtherFarmerNames()[0] : split[1]);
				}
				if (nPC == null || previousAnswerChoice < 0 || previousAnswerChoice >= array.Length)
				{
					CurrentCommand++;
					return;
				}
				nPC.CurrentDialogue.Push(new Dialogue(array[previousAnswerChoice], nPC));
				Game1.drawDialogue(nPC);
			}
		}

		public virtual void command_catQuestion(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.isQuestion && Game1.activeClickableMenu == null)
			{
				Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1241") + (Game1.player.catPerson ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1242") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1243")) + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1244"), Game1.currentLocation.createYesNoResponses(), "pet");
			}
		}

		public virtual void command_ambientLight(GameLocation location, GameTime time, string[] split)
		{
			if (split.Count() > 4)
			{
				int num = Game1.ambientLight.R;
				int num2 = Game1.ambientLight.G;
				int num3 = Game1.ambientLight.B;
				float_useMeForAnything += time.ElapsedGameTime.Milliseconds;
				if (float_useMeForAnything > 10f)
				{
					bool flag = true;
					if (num != Convert.ToInt32(split[1]))
					{
						num += Math.Sign(Convert.ToInt32(split[1]) - num);
						flag = false;
					}
					if (num2 != Convert.ToInt32(split[2]))
					{
						num2 += Math.Sign(Convert.ToInt32(split[2]) - num2);
						flag = false;
					}
					if (num3 != Convert.ToInt32(split[3]))
					{
						num3 += Math.Sign(Convert.ToInt32(split[3]) - num3);
						flag = false;
					}
					float_useMeForAnything = 0f;
					Game1.ambientLight = new Color(num, num2, num3);
					if (flag)
					{
						CurrentCommand++;
					}
				}
			}
			else
			{
				Game1.ambientLight = new Color(Convert.ToInt32(split[1]), Convert.ToInt32(split[2]), Convert.ToInt32(split[3]));
				CurrentCommand++;
			}
		}

		public virtual void command_bgColor(GameLocation location, GameTime time, string[] split)
		{
			Game1.setBGColor(Convert.ToByte(split[1]), Convert.ToByte(split[2]), Convert.ToByte(split[3]));
			CurrentCommand++;
		}

		public virtual void command_elliottbooktalk(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.dialogueUp)
			{
				string text = "";
				text = (Game1.player.dialogueQuestionsAnswered.Contains(958699) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1257") : (Game1.player.dialogueQuestionsAnswered.Contains(958700) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1258") : ((!Game1.player.dialogueQuestionsAnswered.Contains(9586701)) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1260") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1259"))));
				NPC nPC = getActorByName("Elliott");
				if (nPC == null)
				{
					nPC = Game1.getCharacterFromName("Elliott");
				}
				nPC.CurrentDialogue.Push(new Dialogue(text, nPC));
				Game1.drawDialogue(nPC);
			}
		}

		public virtual void command_removeItem(GameLocation location, GameTime time, string[] split)
		{
			Game1.player.removeFirstOfThisItemFromInventory(Convert.ToInt32(split[1]));
			CurrentCommand++;
		}

		public virtual void command_friendship(GameLocation location, GameTime time, string[] split)
		{
			NPC characterFromName = Game1.getCharacterFromName(split[1]);
			if (characterFromName != null)
			{
				Game1.player.changeFriendship(Convert.ToInt32(split[2]), characterFromName);
			}
			CurrentCommand++;
		}

		public virtual void command_setRunning(GameLocation location, GameTime time, string[] split)
		{
			farmer.setRunning(isRunning: true);
			CurrentCommand++;
		}

		public virtual void command_extendSourceRect(GameLocation location, GameTime time, string[] split)
		{
			if (split[2].Equals("reset"))
			{
				getActorByName(split[1]).reloadSprite();
				getActorByName(split[1]).Sprite.SpriteWidth = 16;
				getActorByName(split[1]).Sprite.SpriteHeight = 32;
				getActorByName(split[1]).HideShadow = false;
			}
			else
			{
				getActorByName(split[1]).extendSourceRect(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]), split.Length <= 4);
			}
			CurrentCommand++;
		}

		public virtual void command_waitForOtherPlayers(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.IsMultiplayer)
			{
				Game1.player.team.SetLocalReady(split[1], ready: true);
				if (Game1.player.team.IsReady(split[1]))
				{
					if (Game1.activeClickableMenu is ReadyCheckDialog)
					{
						Game1.exitActiveMenu();
					}
					CurrentCommand++;
				}
				else if (Game1.activeClickableMenu == null)
				{
					Game1.activeClickableMenu = new ReadyCheckDialog(split[1], allowCancel: false);
				}
			}
			else
			{
				CurrentCommand++;
			}
		}

		public virtual void command_requestMovieEnd(GameLocation location, GameTime time, string[] split)
		{
			Game1.player.team.requestMovieEndEvent.Fire(Game1.player.UniqueMultiplayerID);
		}

		public virtual void command_restoreStashedItem(GameLocation location, GameTime time, string[] split)
		{
			Game1.player.TemporaryItem = null;
			CurrentCommand++;
		}

		public virtual void command_advancedMove(GameLocation location, GameTime time, string[] split)
		{
			setUpAdvancedMove(split);
			CurrentCommand++;
		}

		public virtual void command_stopRunning(GameLocation location, GameTime time, string[] split)
		{
			farmer.setRunning(isRunning: false);
			CurrentCommand++;
		}

		public virtual void command_eyes(GameLocation location, GameTime time, string[] split)
		{
			farmer.currentEyes = Convert.ToInt32(split[1]);
			farmer.blinkTimer = Convert.ToInt32(split[2]);
			CurrentCommand++;
		}

		public virtual void command_addMailReceived(GameLocation location, GameTime time, string[] split)
		{
			Game1.player.mailReceived.Add(split[1]);
			CurrentCommand++;
		}

		public virtual void command_addWorldState(GameLocation location, GameTime time, string[] split)
		{
			Game1.worldStateIDs.Add(split[1]);
			Game1.netWorldState.Value.addWorldStateID(split[1]);
			CurrentCommand++;
		}

		public virtual void command_fade(GameLocation location, GameTime time, string[] split)
		{
			if (split.Count() > 1 && split[1].Equals("unfade"))
			{
				Game1.fadeIn = false;
				Game1.fadeToBlack = false;
				CurrentCommand++;
				return;
			}
			Game1.fadeToBlack = true;
			Game1.fadeIn = true;
			if (Game1.fadeToBlackAlpha >= 0.97f)
			{
				if (split.Length == 1)
				{
					Game1.fadeIn = false;
				}
				CurrentCommand++;
			}
		}

		public virtual void command_changeMapTile(GameLocation location, GameTime time, string[] split)
		{
			string layerId = split[1];
			int x = OffsetTileX(Convert.ToInt32(split[2]));
			int y = OffsetTileY(Convert.ToInt32(split[3]));
			int tileIndex = Convert.ToInt32(split[4]);
			location.map.GetLayer(layerId).Tiles[x, y].TileIndex = tileIndex;
			CurrentCommand++;
		}

		public virtual void command_changeSprite(GameLocation location, GameTime time, string[] split)
		{
			getActorByName(split[1]).Sprite.LoadTexture("Characters\\" + NPC.getTextureNameForCharacter(split[1]) + "_" + split[2]);
			CurrentCommand++;
		}

		public virtual void command_waitForAllStationary(GameLocation location, GameTime time, string[] split)
		{
			bool flag = false;
			if (npcControllers != null && npcControllers.Count > 0)
			{
				flag = true;
			}
			if (!flag)
			{
				foreach (NPC actor in actors)
				{
					if (actor.isMoving())
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				foreach (Farmer farmerActor in farmerActors)
				{
					if (farmerActor.isMoving())
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				CurrentCommand++;
			}
		}

		public virtual void command_proceedPosition(GameLocation location, GameTime time, string[] split)
		{
			continueAfterMove = true;
			try
			{
				Character characterByName = getCharacterByName(split[1]);
				if (characterByName == null)
				{
					CurrentCommand++;
				}
				else if (!characterByName.isMoving() || (npcControllers != null && npcControllers.Count == 0))
				{
					characterByName.Halt();
					CurrentCommand++;
				}
			}
			catch (Exception)
			{
				CurrentCommand++;
			}
		}

		public virtual void command_changePortrait(GameLocation location, GameTime time, string[] split)
		{
			NPC nPC = getActorByName(split[1]);
			if (nPC == null)
			{
				nPC = Game1.getCharacterFromName(split[1]);
			}
			nPC.Portrait = Game1.content.Load<Texture2D>("Portraits\\" + split[1] + "_" + split[2]);
			nPC.uniquePortraitActive = true;
			npcsWithUniquePortraits.Add(nPC);
			CurrentCommand++;
		}

		public virtual void command_changeYSourceRectOffset(GameLocation location, GameTime time, string[] split)
		{
			NPC actorByName = getActorByName(split[1]);
			if (actorByName != null)
			{
				actorByName.ySourceRectOffset = Convert.ToInt32(split[2]);
			}
			CurrentCommand++;
		}

		public virtual void command_changeName(GameLocation location, GameTime time, string[] split)
		{
			NPC actorByName = getActorByName(split[1]);
			if (actorByName != null)
			{
				actorByName.displayName = split[2].Replace('_', ' ');
			}
			CurrentCommand++;
		}

		public virtual void command_playFramesAhead(GameLocation location, GameTime time, string[] split)
		{
			int num = Convert.ToInt32(split[1]);
			CurrentCommand++;
			for (int i = 0; i < num; i++)
			{
				checkForNextCommand(location, time);
			}
		}

		public virtual void command_showKissFrame(GameLocation location, GameTime time, string[] split)
		{
			bool flag = true;
			NPC actorByName = getActorByName(split[1]);
			bool flag2 = split.Count() > 2 && Convert.ToBoolean(split[2]);
			int frame = 28;
			string name = actorByName.Name;
			if (name != null)
			{
				switch (name.Length)
				{
				case 4:
					switch (name[0])
					{
					case 'M':
						if (name == "Maru")
						{
							frame = 28;
							flag = false;
						}
						break;
					case 'L':
						if (name == "Leah")
						{
							frame = 25;
							flag = true;
						}
						break;
					case 'A':
						if (name == "Alex")
						{
							frame = 42;
							flag = true;
						}
						break;
					}
					break;
				case 6:
					switch (name[0])
					{
					case 'H':
						if (name == "Harvey")
						{
							frame = 31;
							flag = false;
						}
						break;
					case 'K':
						if (name == "Krobus")
						{
							frame = 16;
							flag = true;
						}
						break;
					}
					break;
				case 7:
					switch (name[0])
					{
					case 'E':
						if (name == "Elliott")
						{
							frame = 35;
							flag = false;
						}
						break;
					case 'A':
						if (name == "Abigail")
						{
							frame = 33;
							flag = false;
						}
						break;
					}
					break;
				case 5:
					switch (name[0])
					{
					case 'P':
						if (name == "Penny")
						{
							frame = 35;
							flag = true;
						}
						break;
					case 'S':
						if (name == "Shane")
						{
							frame = 34;
							flag = false;
						}
						break;
					case 'E':
						if (name == "Emily")
						{
							frame = 33;
							flag = false;
						}
						break;
					}
					break;
				case 9:
					if (name == "Sebastian")
					{
						frame = 40;
						flag = false;
					}
					break;
				case 3:
					if (name == "Sam")
					{
						frame = 36;
						flag = true;
					}
					break;
				}
			}
			if (flag2)
			{
				flag = !flag;
			}
			actorByName.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(frame, 1000, secondaryArm: false, flag)
			});
			CurrentCommand++;
		}

		public virtual void command_addTemporaryActor(GameLocation location, GameTime time, string[] split)
		{
			string text = "Characters\\";
			bool flag = true;
			if (split.Length > 8 && split[8].ToLower().Equals("animal"))
			{
				text = "Animals\\";
			}
			else if (split.Length > 8 && split[8].ToLower().Equals("monster"))
			{
				text = "Characters\\Monsters\\";
			}
			else if (split.Length <= 8 || !split[8].ToLower().Equals("character"))
			{
				flag = false;
			}
			NPC nPC = new NPC(new AnimatedSprite(festivalContent, text + split[1].Replace('_', ' '), 0, Convert.ToInt32(split[2]), Convert.ToInt32(split[3])), OffsetPosition(new Vector2(Convert.ToInt32(split[4]), Convert.ToInt32(split[5])) * 64f), Convert.ToInt32(split[6]), split[1].Replace('_', ' '), festivalContent);
			if (split.Length > 7)
			{
				nPC.Breather = Convert.ToBoolean(split[7]);
			}
			if (!flag && split.Length > 8)
			{
				nPC.displayName = split[8].Replace('_', ' ');
			}
			if (isFestival)
			{
				try
				{
					nPC.CurrentDialogue.Push(new Dialogue(festivalData[nPC.Name], nPC));
				}
				catch (Exception)
				{
				}
			}
			if (text.Contains("Animals") && split.Length > 9)
			{
				nPC.Name = split[9];
			}
			if (nPC.Sprite.SpriteWidth >= 32)
			{
				nPC.HideShadow = true;
			}
			nPC.eventActor = true;
			actors.Add(nPC);
			CurrentCommand++;
		}

		public virtual void command_changeToTemporaryMap(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Contains("Town"))
			{
				temporaryLocation = new Town("Maps\\" + split[1], "Temp");
			}
			else
			{
				temporaryLocation = new GameLocation("Maps\\" + split[1], "Temp");
			}
			temporaryLocation.map.LoadTileSheets(Game1.mapDisplayDevice);
			Event currentEvent = Game1.currentLocation.currentEvent;
			Game1.currentLocation.cleanupBeforePlayerExit();
			Game1.currentLocation.currentEvent = null;
			Game1.currentLightSources.Clear();
			Game1.currentLocation = temporaryLocation;
			Game1.currentLocation.resetForPlayerEntry();
			Game1.currentLocation.currentEvent = currentEvent;
			CurrentCommand++;
			Game1.player.currentLocation = Game1.currentLocation;
			farmer.currentLocation = Game1.currentLocation;
			if (split.Length < 3)
			{
				Game1.panScreen(0, 0);
			}
		}

		public virtual void command_positionOffset(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Contains("farmer"))
			{
				Farmer farmerFromFarmerNumberString = getFarmerFromFarmerNumberString(split[1], farmer);
				if (farmerFromFarmerNumberString != null)
				{
					farmerFromFarmerNumberString.position.X += Convert.ToInt32(split[2]);
					farmerFromFarmerNumberString.position.Y += Convert.ToInt32(split[3]);
				}
			}
			else
			{
				NPC actorByName = getActorByName(split[1]);
				if (actorByName != null)
				{
					actorByName.position.X += Convert.ToInt32(split[2]);
					actorByName.position.Y += Convert.ToInt32(split[3]);
				}
			}
			CurrentCommand++;
			if (split.Length > 4)
			{
				checkForNextCommand(location, time);
			}
		}

		public virtual void command_question(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.isQuestion && Game1.activeClickableMenu == null)
			{
				string text = eventCommands[Math.Min(eventCommands.Length - 1, CurrentCommand)];
				string[] array = text.Split('"')[1].Split('#');
				string question = array[0];
				Response[] array2 = new Response[array.Length - 1];
				for (int i = 1; i < array.Length; i++)
				{
					array2[i - 1] = new Response((i - 1).ToString(), array[i]);
				}
				Game1.currentLocation.createQuestionDialogue(question, array2, split[1]);
			}
		}

		public virtual void command_quickQuestion(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.isQuestion && Game1.activeClickableMenu == null)
			{
				string text = eventCommands[Math.Min(eventCommands.Length - 1, CurrentCommand)];
				string[] array = text.Substring(text.IndexOf(' ') + 1).Split(new string[1] { "(break)" }, StringSplitOptions.None);
				string[] array2 = array[0].Split('#');
				string question = array2[0];
				Response[] array3 = new Response[array2.Length - 1];
				for (int i = 1; i < array2.Length; i++)
				{
					array3[i - 1] = new Response((i - 1).ToString(), array2[i]);
				}
				Game1.currentLocation.createQuestionDialogue(question, array3, "quickQuestion");
			}
		}

		public virtual void command_drawOffset(GameLocation location, GameTime time, string[] split)
		{
			int num = Convert.ToInt32(split[2]);
			float y = Convert.ToInt32(split[3]);
			Character character = null;
			character = ((!split[1].Equals("farmer")) ? ((Character)getActorByName(split[1])) : ((Character)farmer));
			character.drawOffset.Value = new Vector2(num, y) * 4f;
			CurrentCommand++;
		}

		public virtual void command_hideShadow(GameLocation location, GameTime time, string[] split)
		{
			bool hideShadow = split[2].Equals("true");
			NPC nPC = null;
			nPC = getActorByName(split[1]);
			nPC.HideShadow = hideShadow;
			CurrentCommand++;
		}

		public virtual void command_animateHeight(GameLocation location, GameTime time, string[] split)
		{
			int? num = null;
			float? num2 = null;
			float? num3 = null;
			if (split[2] != "keep")
			{
				num = Convert.ToInt32(split[2]);
			}
			if (split[3] != "keep")
			{
				num2 = (float)Convert.ToDouble(split[3]);
			}
			if (split[4] != "keep")
			{
				num3 = Convert.ToInt32(split[4]);
			}
			Character character = null;
			character = ((!split[1].Equals("farmer")) ? ((Character)getActorByName(split[1])) : ((Character)farmer));
			if (num.HasValue)
			{
				character.yJumpOffset = -num.Value;
			}
			if (num2.HasValue)
			{
				character.yJumpGravity = num2.Value;
			}
			if (num3.HasValue)
			{
				character.yJumpVelocity = num3.Value;
			}
			CurrentCommand++;
		}

		public virtual void command_jump(GameLocation location, GameTime time, string[] split)
		{
			float jumpVelocity = ((split.Length > 2) ? ((float)Convert.ToDouble(split[2])) : 8f);
			if (split[1].Equals("farmer"))
			{
				farmer.jump(jumpVelocity);
			}
			else
			{
				getActorByName(split[1]).jump(jumpVelocity);
			}
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_farmerEat(GameLocation location, GameTime time, string[] split)
		{
			Object o = new Object(Convert.ToInt32(split[1]), 1);
			farmer.eatObject(o, overrideFullness: true);
			CurrentCommand++;
		}

		public virtual void command_spriteText(GameLocation location, GameTime time, string[] split)
		{
			int num = eventCommands[CurrentCommand].IndexOf('"') + 1;
			int num2 = eventCommands[CurrentCommand].LastIndexOf('"');
			int_useMeForAnything2 = Convert.ToInt32(split[1]);
			if (num2 <= 0 || num2 <= num)
			{
				return;
			}
			string text = eventCommands[CurrentCommand].Substring(num, num2 - num);
			float_useMeForAnything += time.ElapsedGameTime.Milliseconds;
			if (float_useMeForAnything > 80f)
			{
				if (int_useMeForAnything >= text.Length - 1)
				{
					if (float_useMeForAnything >= 2500f)
					{
						int_useMeForAnything = 0;
						float_useMeForAnything = 0f;
						spriteTextToDraw = "";
						CurrentCommand++;
					}
				}
				else
				{
					int_useMeForAnything++;
					float_useMeForAnything = 0f;
					Game1.playSound("dialogueCharacter");
				}
			}
			spriteTextToDraw = text;
		}

		public virtual void command_ignoreCollisions(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Contains("farmer"))
			{
				Farmer farmerFromFarmerNumberString = getFarmerFromFarmerNumberString(split[1], farmer);
				if (farmerFromFarmerNumberString != null)
				{
					farmerFromFarmerNumberString.ignoreCollisions = true;
				}
			}
			else
			{
				NPC actorByName = getActorByName(split[1]);
				if (actorByName != null)
				{
					actorByName.isCharging = true;
				}
			}
			CurrentCommand++;
		}

		public virtual void command_screenFlash(GameLocation location, GameTime time, string[] split)
		{
			Game1.flashAlpha = (float)Convert.ToDouble(split[1]);
			CurrentCommand++;
		}

		public virtual void command_grandpaCandles(GameLocation location, GameTime time, string[] split)
		{
			int grandpaCandlesFromScore = Utility.getGrandpaCandlesFromScore(Utility.getGrandpaScore());
			Game1.getFarm().grandpaScore.Value = grandpaCandlesFromScore;
			for (int i = 0; i < grandpaCandlesFromScore; i++)
			{
				DelayedAction.playSoundAfterDelay("fireball", 100 * i);
			}
			Game1.getFarm().addGrandpaCandles();
			CurrentCommand++;
		}

		public virtual void command_grandpaEvaluation2(GameLocation location, GameTime time, string[] split)
		{
			int grandpaScore = Utility.getGrandpaScore();
			switch (Utility.getGrandpaCandlesFromScore(grandpaScore))
			{
			case 1:
				eventCommands[currentCommand] = "speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1306") + "\"";
				break;
			case 2:
				eventCommands[currentCommand] = "speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1307") + "\"";
				break;
			case 3:
				eventCommands[currentCommand] = "speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1308") + "\"";
				break;
			case 4:
				eventCommands[currentCommand] = "speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1309") + "\"";
				break;
			}
			Game1.player.eventsSeen.Remove(2146991);
		}

		public virtual void command_grandpaEvaluation(GameLocation location, GameTime time, string[] split)
		{
			int grandpaScore = Utility.getGrandpaScore();
			switch (Utility.getGrandpaCandlesFromScore(grandpaScore))
			{
			case 1:
				eventCommands[currentCommand] = "speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1315") + "\"";
				break;
			case 2:
				eventCommands[currentCommand] = "speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1316") + "\"";
				break;
			case 3:
				eventCommands[currentCommand] = "speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1317") + "\"";
				break;
			case 4:
				eventCommands[currentCommand] = "speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1318") + "\"";
				break;
			}
		}

		public virtual void command_loadActors(GameLocation location, GameTime time, string[] split)
		{
			if (temporaryLocation != null && temporaryLocation.map.GetLayer(split[1]) != null)
			{
				actors.Clear();
				if (npcControllers != null)
				{
					npcControllers.Clear();
				}
				Dictionary<string, string> source = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
				List<string> list = new List<string>();
				for (int i = 0; i < temporaryLocation.map.GetLayer(split[1]).LayerWidth; i++)
				{
					for (int j = 0; j < temporaryLocation.map.GetLayer(split[1]).LayerHeight; j++)
					{
						if (temporaryLocation.map.GetLayer(split[1]).Tiles[i, j] != null)
						{
							int index = temporaryLocation.map.GetLayer(split[1]).Tiles[i, j].TileIndex / 4;
							int facingDirection = temporaryLocation.map.GetLayer(split[1]).Tiles[i, j].TileIndex % 4;
							string key = source.ElementAt(index).Key;
							if (key != null && Game1.getCharacterFromName(key) != null && (!(key == "Leo") || Game1.MasterPlayer.mailReceived.Contains("leoMoved")))
							{
								addActor(key, i, j, facingDirection, temporaryLocation);
								list.Add(key);
							}
						}
					}
				}
				if (festivalData != null)
				{
					string key2 = split[1] + "_additionalCharacters";
					if (festivalData.ContainsKey(key2))
					{
						string text = festivalData[key2];
						string[] array = text.Split('/');
						string[] array2 = array;
						foreach (string text2 in array2)
						{
							if (string.IsNullOrEmpty(text2))
							{
								continue;
							}
							string[] array3 = text2.Split(' ');
							if (array3.Length < 4)
							{
								continue;
							}
							bool flag = false;
							int result = 0;
							int result2 = 0;
							int result3 = 2;
							if (!flag && !int.TryParse(array3[1], out result))
							{
								flag = true;
							}
							if (!flag && !int.TryParse(array3[2], out result2))
							{
								flag = true;
							}
							if (!flag)
							{
								string text3 = array3[3];
								text3 = text3.ToLowerInvariant();
								switch (text3)
								{
								case "up":
									result3 = 0;
									break;
								case "down":
									result3 = 2;
									break;
								case "left":
									result3 = 3;
									break;
								case "right":
									result3 = 1;
									break;
								default:
									if (!int.TryParse(text3, out result3))
									{
										flag = true;
									}
									break;
								}
							}
							if (flag)
							{
								Console.WriteLine("Warning: Failed to load additional festival character: " + text2);
								continue;
							}
							string text4 = array3[0];
							if (text4 != null && Game1.getCharacterFromName(text4) != null)
							{
								if (!(text4 == "Leo") || Game1.MasterPlayer.mailReceived.Contains("leoMoved"))
								{
									addActor(text4, result, result2, result3, temporaryLocation);
									list.Add(text4);
								}
							}
							else
							{
								Console.WriteLine("Warning: Invalid additional festival character name: " + text4);
							}
						}
					}
				}
				if (split[1] == "Set-Up")
				{
					foreach (string item in list)
					{
						NPC characterFromName = Game1.getCharacterFromName(item);
						if (!characterFromName.isMarried() || characterFromName.getSpouse() == null || characterFromName.getSpouse().getChildren().Count <= 0)
						{
							continue;
						}
						Farmer farmer = Game1.player;
						if (characterFromName.getSpouse() != null)
						{
							farmer = characterFromName.getSpouse();
						}
						List<Child> children = farmer.getChildren();
						characterFromName = getCharacterByName(item) as NPC;
						for (int l = 0; l < children.Count; l++)
						{
							Child child = children[l];
							if (child.Age < 3)
							{
								continue;
							}
							Child child2 = new Child(child.Name, child.Gender == 0, child.darkSkinned, farmer);
							child2.NetFields.CopyFrom(child.NetFields);
							child2.Halt();
							Point point2 = new Point((int)characterFromName.Position.X / 64, (int)characterFromName.Position.Y / 64);
							Point[] array4 = null;
							array4 = characterFromName.FacingDirection switch
							{
								0 => new Point[4]
								{
									new Point(0, 1),
									new Point(-1, 0),
									new Point(1, 0),
									new Point(0, -1)
								}, 
								2 => new Point[4]
								{
									new Point(0, -1),
									new Point(1, 0),
									new Point(-1, 0),
									new Point(0, 1)
								}, 
								3 => new Point[4]
								{
									new Point(1, 0),
									new Point(0, -1),
									new Point(0, 1),
									new Point(-1, 0)
								}, 
								1 => new Point[4]
								{
									new Point(-1, 0),
									new Point(0, 1),
									new Point(0, -1),
									new Point(1, 0)
								}, 
								_ => new Point[4]
								{
									new Point(-1, 0),
									new Point(1, 0),
									new Point(0, -1),
									new Point(0, 1)
								}, 
							};
							Point point3 = new Point(characterFromName.getTileX(), characterFromName.getTileY());
							List<Point> list2 = new List<Point>();
							List<Point> list3 = new List<Point>();
							Point[] array5 = array4;
							for (int m = 0; m < array5.Length; m++)
							{
								Point point4 = array5[m];
								list2.Add(new Point(point3.X + point4.X, point3.Y + point4.Y));
							}
							Func<Point, bool> func = (Point point) => temporaryLocation.isTilePassable(new Location(point.X, point.Y), Game1.viewport) ? true : false;
							Func<Point, bool> func2 = delegate(Point point)
							{
								int num3 = 1;
								for (int num4 = point.X - num3; num4 <= point.X + num3; num4++)
								{
									for (int num5 = point.Y - num3; num5 <= point.Y + num3; num5++)
									{
										if (temporaryLocation.isTileOccupiedForPlacement(new Vector2(num4, num5)))
										{
											return false;
										}
										foreach (NPC actor in actors)
										{
											if (!(actor is Child) && actor.getTileX() == num4 && actor.getTileY() == num5)
											{
												return false;
											}
										}
									}
								}
								return true;
							};
							bool flag2 = false;
							for (int n = 0; n < 5; n++)
							{
								if (flag2)
								{
									break;
								}
								int count = list2.Count;
								for (int num = 0; num < count; num++)
								{
									Point point5 = list2[0];
									list2.RemoveAt(0);
									if (func(point5))
									{
										if (func2(point5))
										{
											flag2 = true;
											point3 = point5;
											break;
										}
										Point[] array6 = array4;
										for (int num2 = 0; num2 < array6.Length; num2++)
										{
											Point point6 = array6[num2];
											list2.Add(new Point(point5.X + point6.X, point5.Y + point6.Y));
										}
									}
								}
							}
							if (flag2)
							{
								child2.setTilePosition(point3.X, point3.Y);
								child2.DefaultPosition = characterFromName.DefaultPosition;
								child2.faceDirection(characterFromName.FacingDirection);
								child2.eventActor = true;
								child2.lastCrossroad = new Microsoft.Xna.Framework.Rectangle(point3.X * 64, point3.Y * 64, 64, 64);
								child2.squareMovementFacingPreference = -1;
								child2.walkInSquare(3, 3, 2000);
								child2.controller = null;
								child2.temporaryController = null;
								actors.Add(child2);
							}
						}
					}
				}
			}
			CurrentCommand++;
		}

		public virtual void command_playerControl(GameLocation location, GameTime time, string[] split)
		{
			if (!playerControlSequence)
			{
				setUpPlayerControlSequence(split[1]);
			}
		}

		public virtual void command_removeSprite(GameLocation location, GameTime time, string[] split)
		{
			Vector2 other = OffsetPosition(new Vector2(Convert.ToInt32(split[1]), Convert.ToInt32(split[2])) * 64f);
			for (int num = Game1.currentLocation.temporarySprites.Count - 1; num >= 0; num--)
			{
				if (Game1.currentLocation.temporarySprites[num].position.Equals(other))
				{
					Game1.currentLocation.temporarySprites.RemoveAt(num);
				}
			}
			CurrentCommand++;
		}

		public virtual void command_viewport(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Equals("move"))
			{
				viewportTarget = new Vector3(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]), Convert.ToInt32(split[4]));
			}
			else
			{
				if (CurrentCommand == 7 && FestivalName == "Feast of the Winter Star" && eventCommands[Math.Min(eventCommands.Length - 1, CurrentCommand)] == "viewport 30 67 true")
				{
					ResetToNativeZoom();
				}
				if (CurrentCommand == 6 && id == 8 && eventCommands[Math.Min(eventCommands.Length - 1, CurrentCommand)] == "viewport 14 23 true" && Game1.currentLocation.Name == "Temp")
				{
					split[2] = "26";
				}
				if (aboveMapSprites != null && Convert.ToInt32(split[1]) < 0)
				{
					aboveMapSprites.Clear();
					aboveMapSprites = null;
				}
				Game1.viewportFreeze = true;
				int num = OffsetTileX(Convert.ToInt32(split[1]));
				int num2 = OffsetTileY(Convert.ToInt32(split[2]));
				if (id == 2146991)
				{
					Point grandpaShrinePosition = Game1.getFarm().GetGrandpaShrinePosition();
					num = grandpaShrinePosition.X;
					num2 = grandpaShrinePosition.Y;
				}
				Game1.viewport.X = num * 64 + 32 - Game1.viewport.Width / 2;
				Game1.viewport.Y = num2 * 64 + 32 - Game1.viewport.Height / 2;
				if (Game1.viewport.X > 0 && Game1.viewport.Width > Game1.currentLocation.Map.DisplayWidth)
				{
					Game1.viewport.X = (Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width) / 2;
				}
				if (Game1.viewport.Y > 0 && Game1.viewport.Height > Game1.currentLocation.Map.DisplayHeight)
				{
					Game1.viewport.Y = (Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height) / 2;
				}
				if (split.Length > 3 && split[3].Equals("true"))
				{
					Game1.fadeScreenToBlack();
					Game1.fadeToBlackAlpha = 1f;
					Game1.nonWarpFade = true;
				}
				else if (split.Length > 3 && split[3].Equals("clamp"))
				{
					if (Game1.currentLocation.map.DisplayWidth >= Game1.viewport.Width)
					{
						if (Game1.viewport.X + Game1.viewport.Width > Game1.currentLocation.Map.DisplayWidth)
						{
							Game1.viewport.X = Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width;
						}
						if (Game1.viewport.X < 0)
						{
							Game1.viewport.X = 0;
						}
					}
					else
					{
						Game1.viewport.X = Game1.currentLocation.Map.DisplayWidth / 2 - Game1.viewport.Width / 2;
					}
					if (Game1.currentLocation.map.DisplayHeight >= Game1.viewport.Height)
					{
						if (Game1.viewport.Y + Game1.viewport.Height > Game1.currentLocation.Map.DisplayHeight)
						{
							Game1.viewport.Y = Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height;
						}
					}
					else
					{
						Game1.viewport.Y = Game1.currentLocation.Map.DisplayHeight / 2 - Game1.viewport.Height / 2;
					}
					if (Game1.viewport.Y < 0)
					{
						Game1.viewport.Y = 0;
					}
					if (split.Length > 4 && split[4].Equals("true"))
					{
						Game1.fadeScreenToBlack();
						Game1.fadeToBlackAlpha = 1f;
						Game1.nonWarpFade = true;
					}
				}
				if (split.Length > 4 && split[4].Equals("unfreeze"))
				{
					Game1.viewportFreeze = false;
				}
				if (Game1.gameMode == 2)
				{
					Game1.viewport.X = Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width;
				}
			}
			CurrentCommand++;
		}

		public virtual void command_broadcastEvent(GameLocation location, GameTime time, string[] split)
		{
			if (farmer == Game1.player)
			{
				bool use_local_farmer = false;
				if (split.Length > 1 && split[1] == "local")
				{
					use_local_farmer = true;
				}
				if (id == 558291 || id == 558292)
				{
					use_local_farmer = true;
				}
				Game1.multiplayer.broadcastEvent(this, Game1.currentLocation, Game1.player.positionBeforeEvent, use_local_farmer);
			}
			CurrentCommand++;
		}

		public virtual void command_addConversationTopic(GameLocation location, GameTime time, string[] split)
		{
			if (isMemory)
			{
				CurrentCommand++;
				return;
			}
			if (!Game1.player.activeDialogueEvents.ContainsKey(split[1]))
			{
				Game1.player.activeDialogueEvents.Add(split[1], (split.Count() > 2) ? Convert.ToInt32(split[2]) : 4);
			}
			CurrentCommand++;
		}

		public virtual void command_dump(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Equals("girls"))
			{
				Game1.player.activeDialogueEvents.Add("dumped_Girls", 7);
				Game1.player.activeDialogueEvents.Add("secondChance_Girls", 14);
			}
			else
			{
				Game1.player.activeDialogueEvents.Add("dumped_Guys", 7);
				Game1.player.activeDialogueEvents.Add("secondChance_Guys", 14);
			}
			CurrentCommand++;
		}

		public Event(string eventString, int eventID = -1, Farmer farmerActor = null)
			: this()
		{
			id = eventID;
			ResetToNativeZoom();
			eventCommands = eventString.Split('/');
			actorPositionsAfterMove = new Dictionary<string, Vector3>();
			previousAmbientLight = Game1.ambientLight;
			if (farmerActor != null)
			{
				farmerActors.Add(farmerActor);
			}
			farmer.canOnlyWalk = true;
			farmer.showNotCarrying();
			drawTool = false;
			if (eventID == -2)
			{
				Game1.displayHUD = false;
				isWedding = true;
			}
		}

		public Event()
		{
			setupEventCommands();
		}

		private void ResetToNativeZoom()
		{
			PinchZoom.Instance.SetZoomLevel(1f);
			Game1.options.desiredBaseZoomLevel = 1f;
		}

		public bool tryToLoadFestival(string festival)
		{
			if (invalidFestivals.Contains(festival))
			{
				return false;
			}
			Game1.player.festivalScore = 0;
			try
			{
				festivalData = festivalContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + festival);
				festivalData["file"] = festival;
			}
			catch (Exception)
			{
				invalidFestivals.Add(festival);
				return false;
			}
			string text = festivalData["conditions"].Split('/')[0];
			int num = Convert.ToInt32(festivalData["conditions"].Split('/')[1].Split(' ')[0]);
			int num2 = Convert.ToInt32(festivalData["conditions"].Split('/')[1].Split(' ')[1]);
			if (!text.Equals(Game1.currentLocation.Name) || Game1.timeOfDay < num || Game1.timeOfDay >= num2)
			{
				return false;
			}
			int num3 = 1;
			while (festivalData.ContainsKey("set-up_y" + num3 + 1))
			{
				num3++;
			}
			int num4 = Game1.year % num3;
			if (num4 == 0)
			{
				num4 = num3;
			}
			eventCommands = festivalData["set-up"].Split('/');
			if (num4 > 1)
			{
				List<string> list = new List<string>(eventCommands);
				list.AddRange(festivalData["set-up_y" + num4].Split('/'));
				eventCommands = list.ToArray();
			}
			actorPositionsAfterMove = new Dictionary<string, Vector3>();
			previousAmbientLight = Game1.ambientLight;
			isFestival = true;
			Game1.setRichPresence("festival", festival);
			return true;
		}

		public string GetFestivalDataForYear(string key)
		{
			int i;
			for (i = 1; festivalData.ContainsKey(key + "_y" + (i + 1)); i++)
			{
			}
			int num = Game1.year % i;
			if (num == 0)
			{
				num = i;
			}
			if (num > 1)
			{
				return festivalData[key + "_y" + num];
			}
			return festivalData[key];
		}

		public void setExitLocation(string location, int x, int y)
		{
			if (Game1.player.locationBeforeForcedEvent.Value == null || Game1.player.locationBeforeForcedEvent.Value == "")
			{
				exitLocation = Game1.getLocationRequest(location);
				Game1.player.positionBeforeEvent = new Vector2(x, y);
			}
		}

		public void endBehaviors(string[] split, GameLocation location)
		{
			if (Game1.getMusicTrackName().Contains(Game1.currentSeason) && !eventCommands[0].Equals("continue"))
			{
				Game1.stopMusicTrack(Game1.MusicContext.Default);
			}
			if (split != null && split.Length > 1)
			{
				string text = split[1];
				if (text != null)
				{
					switch (text.Length)
					{
					case 3:
						switch (text[0])
						{
						case 'L':
							if (text == "Leo" && !isMemory)
							{
								Game1.addMailForTomorrow("leoMoved", noLetter: true, sendToEveryone: true);
								Game1.player.team.requestLeoMove.Fire();
							}
							break;
						case 'b':
							if (text == "bed")
							{
								Game1.player.Position = Game1.player.mostRecentBed + new Vector2(0f, 64f);
							}
							break;
						}
						break;
					case 8:
						switch (text[0])
						{
						case 'b':
							if (text == "busIntro")
							{
								Game1.currentMinigame = new Intro(4);
							}
							break;
						case 'd':
							if (text == "dialogue")
							{
								NPC characterFromName = Game1.getCharacterFromName(split[2]);
								int startIndex = eventCommands[CurrentCommand].IndexOf('"') + 1;
								int length = eventCommands[CurrentCommand].Substring(eventCommands[CurrentCommand].IndexOf('"') + 1).IndexOf('"');
								if (characterFromName != null)
								{
									characterFromName.shouldSayMarriageDialogue.Value = false;
									characterFromName.currentMarriageDialogue.Clear();
									characterFromName.CurrentDialogue.Clear();
									characterFromName.CurrentDialogue.Push(new Dialogue(eventCommands[CurrentCommand].Substring(startIndex, length), characterFromName));
								}
							}
							break;
						case 'p':
							if (text == "position" && (Game1.player.locationBeforeForcedEvent.Value == null || Game1.player.locationBeforeForcedEvent.Value == ""))
							{
								Game1.player.positionBeforeEvent = new Vector2(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]));
							}
							break;
						}
						break;
					case 9:
						switch (text[0])
						{
						case 'i':
							if (text == "invisible" && !isMemory)
							{
								Game1.getCharacterFromName(split[2]).IsInvisible = true;
							}
							break;
						case 'b':
							if (!(text == "beginGame"))
							{
								break;
							}
							Game1.gameMode = 3;
							setExitLocation("FarmHouse", 9, 9);
							Game1.NewDay(1000f);
							exitEvent();
							Game1.eventFinished();
							TutorialManager.Instance.completeTutorial(tutorialType.DUMMY_PAST_INTRO);
							return;
						}
						break;
					case 7:
						switch (text[1])
						{
						case 'a':
							if (text == "warpOut")
							{
								int index = 0;
								if (location is BathHousePool && Game1.player.IsMale)
								{
									index = 1;
								}
								setExitLocation(location.warps[index].TargetName, location.warps[index].TargetX, location.warps[index].TargetY);
								Game1.eventOver = true;
								CurrentCommand += 2;
								Game1.screenGlowHold = false;
							}
							break;
						case 'e':
						{
							if (!(text == "wedding"))
							{
								break;
							}
							if (farmer.IsMale)
							{
								farmer.changeShirt(-1);
								farmer.changePants(oldPants);
								farmer.changePantStyle(-1);
								Game1.getCharacterFromName("Lewis").CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1025"), Game1.getCharacterFromName("Lewis")));
							}
							FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(Game1.player);
							Point porchStandingSpot = homeOfFarmer.getPorchStandingSpot();
							if (homeOfFarmer is Cabin)
							{
								setExitLocation("Farm", porchStandingSpot.X + 1, porchStandingSpot.Y);
							}
							else
							{
								setExitLocation("Farm", porchStandingSpot.X - 1, porchStandingSpot.Y);
							}
							if (!Game1.IsMasterGame)
							{
								break;
							}
							NPC characterFromName2 = Game1.getCharacterFromName(farmer.spouse);
							if (characterFromName2 != null)
							{
								characterFromName2.Schedule = null;
								characterFromName2.ignoreScheduleToday = true;
								characterFromName2.shouldPlaySpousePatioAnimation.Value = false;
								characterFromName2.controller = null;
								characterFromName2.temporaryController = null;
								characterFromName2.currentMarriageDialogue.Clear();
								Game1.warpCharacter(characterFromName2, "Farm", Utility.getHomeOfFarmer(farmer).getPorchStandingSpot());
								characterFromName2.faceDirection(2);
								string text2 = Game1.content.LoadStringReturnNullIfNotFound("Strings\\StringsFromCSFiles:" + characterFromName2.Name + "_AfterWedding");
								if (text2 != null)
								{
									characterFromName2.addMarriageDialogue("Strings\\StringsFromCSFiles", characterFromName2.Name + "_AfterWedding", false);
								}
								else
								{
									characterFromName2.addMarriageDialogue("Strings\\StringsFromCSFiles", "Game1.cs.2782", false);
								}
							}
							break;
						}
						case 'r':
							if (text == "credits")
							{
								Game1.ClearDebrisWeather(Game1.debrisWeather);
								Game1.isDebrisWeather = false;
								Game1.changeMusicTrack("wedding", track_interruptable: false, Game1.MusicContext.Event);
								Game1.gameMode = 10;
								CurrentCommand += 2;
							}
							break;
						}
						break;
					case 12:
						switch (text[0])
						{
						case 'i':
						{
							if (!(text == "islandDepart"))
							{
								break;
							}
							Game1.player.orientationBeforeEvent = 2;
							if (Game1.whereIsTodaysFest != null && Game1.whereIsTodaysFest == "Beach")
							{
								Game1.player.orientationBeforeEvent = 0;
								setExitLocation("Town", 54, 109);
							}
							else if (Game1.whereIsTodaysFest != null && Game1.whereIsTodaysFest == "Town")
							{
								Game1.player.orientationBeforeEvent = 3;
								setExitLocation("BusStop", 33, 23);
							}
							else
							{
								setExitLocation("BoatTunnel", 6, 9);
							}
							GameLocation left_location = Game1.currentLocation;
							exitLocation.OnLoad += delegate
							{
								foreach (NPC actor in actors)
								{
									actor.shouldShadowBeOffset = true;
									actor.drawOffset.Y = 0f;
								}
								foreach (Farmer farmerActor in farmerActors)
								{
									farmerActor.shouldShadowBeOffset = true;
									farmerActor.drawOffset.Y = 0f;
								}
								Game1.player.drawOffset.Value = Vector2.Zero;
								Game1.player.shouldShadowBeOffset = false;
								if (left_location is IslandSouth)
								{
									(left_location as IslandSouth).ResetBoat();
								}
							};
							break;
						}
						case 't':
							if (text == "tunnelDepart" && Game1.player.hasOrWillReceiveMail("seenBoatJourney"))
							{
								Game1.warpFarmer("IslandSouth", 21, 43, 0);
							}
							break;
						}
						break;
					case 6:
						if (!(text == "newDay"))
						{
							break;
						}
						Game1.player.faceDirection(2);
						setExitLocation(Game1.player.homeLocation, (int)Game1.player.mostRecentBed.X / 64, (int)Game1.player.mostRecentBed.Y / 64);
						if (!Game1.IsMultiplayer)
						{
							exitLocation.OnWarp += delegate
							{
								Game1.NewDay(0f);
								Game1.player.currentLocation.lastTouchActionLocation = new Vector2((int)Game1.player.mostRecentBed.X / 64, (int)Game1.player.mostRecentBed.Y / 64);
							};
						}
						Game1.player.completelyStopAnimatingOrDoingAction();
						if ((bool)Game1.player.bathingClothes)
						{
							Game1.player.changeOutOfSwimSuit();
						}
						Game1.player.swimming.Value = false;
						Game1.player.CanMove = false;
						Game1.changeMusicTrack("none");
						break;
					case 16:
						if (text == "invisibleWarpOut")
						{
							Game1.getCharacterFromName(split[2]).IsInvisible = true;
							setExitLocation(location.warps[0].TargetName, location.warps[0].TargetX, location.warps[0].TargetY);
							Game1.fadeScreenToBlack();
							Game1.eventOver = true;
							CurrentCommand += 2;
							Game1.screenGlowHold = false;
						}
						break;
					case 15:
						if (text == "dialogueWarpOut")
						{
							int index = 0;
							if (location is BathHousePool && Game1.player.IsMale)
							{
								index = 1;
							}
							setExitLocation(location.warps[index].TargetName, location.warps[index].TargetX, location.warps[index].TargetY);
							NPC characterFromName = Game1.getCharacterFromName(split[2]);
							int startIndex = eventCommands[CurrentCommand].IndexOf('"') + 1;
							int length = eventCommands[CurrentCommand].Substring(eventCommands[CurrentCommand].IndexOf('"') + 1).IndexOf('"');
							characterFromName.CurrentDialogue.Clear();
							characterFromName.CurrentDialogue.Push(new Dialogue(eventCommands[CurrentCommand].Substring(startIndex, length), characterFromName));
							Game1.eventOver = true;
							CurrentCommand += 2;
							Game1.screenGlowHold = false;
						}
						break;
					case 5:
						if (text == "Maru1")
						{
							Game1.getCharacterFromName("Demetrius").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1018"));
							Game1.getCharacterFromName("Maru").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1020"));
							setExitLocation(location.warps[0].TargetName, location.warps[0].TargetX, location.warps[0].TargetY);
							Game1.fadeScreenToBlack();
							Game1.eventOver = true;
							CurrentCommand += 2;
						}
						break;
					}
				}
			}
			exitEvent();
		}

		public void exitEvent()
		{
			if (id != -1 && !Game1.player.eventsSeen.Contains(id))
			{
				Game1.player.eventsSeen.Add(id);
			}
			if (id == 1039573)
			{
				Game1.player.team.requestAddCharacterEvent.Fire("Leo");
			}
			Game1.stopMusicTrack(Game1.MusicContext.Event);
			Game1.player.ignoreCollisions = false;
			Game1.player.canOnlyWalk = false;
			Game1.nonWarpFade = true;
			if (!Game1.fadeIn || Game1.fadeToBlackAlpha >= 1f)
			{
				Game1.fadeScreenToBlack();
			}
			Game1.eventOver = true;
			Game1.fadeToBlack = true;
			Game1.setBGColor(5, 3, 4);
			CurrentCommand += 2;
			Game1.screenGlowHold = false;
			if (isFestival)
			{
				Game1.timeOfDayAfterFade = 2200;
				string text = festivalData["file"];
				if (festivalData != null && (festivalData["file"].Equals("summer28") || festivalData["file"].Equals("fall27")))
				{
					Game1.timeOfDayAfterFade = 2400;
				}
				int num = Utility.CalculateMinutesBetweenTimes(Game1.timeOfDay, Game1.timeOfDayAfterFade);
				if (Game1.IsMasterGame)
				{
					Point mainFarmHouseEntry = Game1.getFarm().GetMainFarmHouseEntry();
					setExitLocation("Farm", mainFarmHouseEntry.X, mainFarmHouseEntry.Y);
				}
				else
				{
					Point porchStandingSpot = Utility.getHomeOfFarmer(Game1.player).getPorchStandingSpot();
					setExitLocation("Farm", porchStandingSpot.X, porchStandingSpot.Y);
				}
				Game1.player.toolOverrideFunction = null;
				isFestival = false;
				foreach (NPC actor in actors)
				{
					if (actor != null)
					{
						resetDialogueIfNecessary(actor);
					}
				}
				if (Game1.IsMasterGame)
				{
					foreach (NPC allCharacter in Utility.getAllCharacters())
					{
						if (!allCharacter.isVillager())
						{
							continue;
						}
						if (allCharacter.getSpouse() != null)
						{
							Farmer spouse = allCharacter.getSpouse();
							if (spouse.isMarried())
							{
								allCharacter.controller = null;
								allCharacter.temporaryController = null;
								FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(spouse);
								allCharacter.Halt();
								Game1.warpCharacter(allCharacter, homeOfFarmer, Utility.PointToVector2(homeOfFarmer.getSpouseBedSpot(spouse.spouse)));
								if (homeOfFarmer.GetSpouseBed() != null)
								{
									FarmHouse.spouseSleepEndFunction(allCharacter, Utility.getHomeOfFarmer(spouse));
								}
								allCharacter.ignoreScheduleToday = true;
								if (Game1.timeOfDayAfterFade >= 1800)
								{
									allCharacter.currentMarriageDialogue.Clear();
									allCharacter.checkForMarriageDialogue(1800, Utility.getHomeOfFarmer(spouse));
								}
								else if (Game1.timeOfDayAfterFade >= 1100)
								{
									allCharacter.currentMarriageDialogue.Clear();
									allCharacter.checkForMarriageDialogue(1100, Utility.getHomeOfFarmer(spouse));
								}
								continue;
							}
						}
						if (allCharacter.currentLocation != null && allCharacter.defaultMap.Value != null)
						{
							allCharacter.doingEndOfRouteAnimation.Value = false;
							allCharacter.nextEndOfRouteMessage = null;
							allCharacter.endOfRouteMessage.Value = null;
							allCharacter.controller = null;
							allCharacter.temporaryController = null;
							allCharacter.Halt();
							Game1.warpCharacter(allCharacter, allCharacter.defaultMap, allCharacter.DefaultPosition / 64f);
							allCharacter.ignoreScheduleToday = true;
						}
					}
				}
				foreach (GameLocation location in Game1.locations)
				{
					List<Vector2> list = new List<Vector2>(location.objects.Keys);
					foreach (Vector2 item in list)
					{
						Object @object = location.objects[item];
						if (@object.minutesElapsed(num, location))
						{
							location.objects.Remove(item);
						}
					}
					if (location is Farm)
					{
						(location as Farm).timeUpdate(num);
					}
				}
				Game1.player.freezePause = 1500;
			}
			else
			{
				Game1.player.forceCanMove();
			}
		}

		public void resetDialogueIfNecessary(NPC n)
		{
			if (!Game1.player.hasTalkedToFriendToday(n.Name))
			{
				n.resetCurrentDialogue();
			}
			else if (n.CurrentDialogue != null)
			{
				n.CurrentDialogue.Clear();
			}
		}

		public void incrementCommandAfterFade()
		{
			CurrentCommand++;
			Game1.globalFade = false;
		}

		public void cleanup()
		{
			Game1.ambientLight = previousAmbientLight;
			foreach (NPC npcsWithUniquePortrait in npcsWithUniquePortraits)
			{
				npcsWithUniquePortrait.Portrait = Game1.content.Load<Texture2D>("Portraits\\" + npcsWithUniquePortrait.Name);
				npcsWithUniquePortrait.uniquePortraitActive = false;
			}
			if (_festivalTexture != null)
			{
				_festivalTexture = null;
			}
			festivalContent.Unload();
		}

		private void changeLocation(string locationName, int x, int y, int direction = -1, Action onComplete = null)
		{
			if (direction == -1)
			{
				direction = Game1.player.FacingDirection;
			}
			Event e = Game1.currentLocation.currentEvent;
			Game1.currentLocation.currentEvent = null;
			LocationRequest locationRequest = Game1.getLocationRequest(locationName);
			locationRequest.OnLoad += delegate
			{
				if (!e.isFestival)
				{
					Game1.currentLocation.currentEvent = e;
				}
				temporaryLocation = null;
				if (onComplete != null)
				{
					onComplete();
				}
			};
			locationRequest.OnWarp += delegate
			{
				farmer.currentLocation = Game1.currentLocation;
				if (e.isFestival)
				{
					Game1.currentLocation.currentEvent = e;
				}
			};
			Game1.warpFarmer(locationRequest, x, y, farmer.FacingDirection);
		}

		public void LogErrorAndHalt(Exception e)
		{
			Game1.chatBox.addErrorMessage("Event script error: " + e.Message);
			if (eventCommands != null && eventCommands.Length != 0 && CurrentCommand < eventCommands.Length)
			{
				Game1.chatBox.addErrorMessage("On line #" + CurrentCommand + ": " + eventCommands[CurrentCommand]);
				skipEvent();
			}
		}

		public void checkForNextCommand(GameLocation location, GameTime time)
		{
			try
			{
				_checkForNextCommand(location, time);
			}
			catch (Exception e)
			{
				LogErrorAndHalt(e);
			}
		}

		protected void _checkForNextCommand(GameLocation location, GameTime time)
		{
			if (skipped || Game1.farmEvent != null)
			{
				return;
			}
			foreach (NPC actor in actors)
			{
				actor.update(time, Game1.currentLocation);
				if (actor.Sprite.CurrentAnimation != null)
				{
					actor.Sprite.animateOnce(time);
				}
			}
			if (aboveMapSprites != null)
			{
				for (int num = aboveMapSprites.Count - 1; num >= 0; num--)
				{
					if (aboveMapSprites[num].update(time))
					{
						aboveMapSprites.RemoveAt(num);
					}
				}
			}
			if (underwaterSprites != null)
			{
				foreach (TemporaryAnimatedSprite underwaterSprite in underwaterSprites)
				{
					underwaterSprite.update(time);
				}
			}
			if (!playerControlSequence)
			{
				farmer.setRunning(isRunning: false);
			}
			if (npcControllers != null)
			{
				for (int num2 = npcControllers.Count - 1; num2 >= 0; num2--)
				{
					npcControllers[num2].puppet.isCharging = !isFestival;
					if (npcControllers[num2].update(time, location, npcControllers))
					{
						npcControllers.RemoveAt(num2);
					}
				}
			}
			if (isFestival)
			{
				festivalUpdate(time);
			}
			string[] array = eventCommands[Math.Min(eventCommands.Length - 1, CurrentCommand)].Split(' ');
			if (temporaryLocation != null && !Game1.currentLocation.Equals(temporaryLocation))
			{
				temporaryLocation.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush: true);
			}
			if (array.Length != 0 && array[0].StartsWith("--"))
			{
				CurrentCommand++;
				return;
			}
			if (CurrentCommand == 0 && !forked && !eventSwitched)
			{
				farmer.speed = 2;
				farmer.running = false;
				Game1.eventOver = false;
				if (eventCommands.Length > 3 && eventCommands[3] == "ignoreEventTileOffset")
				{
					ignoreTileOffsets = true;
				}
				if ((!eventCommands[0].Equals("none") || !Game1.isRaining) && !eventCommands[0].Equals("continue") && !eventCommands[0].Contains("pause"))
				{
					Game1.changeMusicTrack(eventCommands[0], track_interruptable: false, Game1.MusicContext.Event);
				}
				if (location is Farm && Convert.ToInt32(eventCommands[1].Split(' ')[0]) >= -1000 && id != -2 && !ignoreTileOffsets)
				{
					Point frontDoorPositionForFarmer = Farm.getFrontDoorPositionForFarmer(farmer);
					frontDoorPositionForFarmer.X *= 64;
					frontDoorPositionForFarmer.Y *= 64;
					Game1.viewport.X = (Game1.currentLocation.IsOutdoors ? Math.Max(0, Math.Min(frontDoorPositionForFarmer.X - Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.currentLocation.Map.DisplayWidth - Game1.graphics.GraphicsDevice.Viewport.Width)) : (frontDoorPositionForFarmer.X - Game1.graphics.GraphicsDevice.Viewport.Width / 2));
					Game1.viewport.Y = (Game1.currentLocation.IsOutdoors ? Math.Max(0, Math.Min(frontDoorPositionForFarmer.Y - Game1.graphics.GraphicsDevice.Viewport.Height / 2, Game1.currentLocation.Map.DisplayHeight - Game1.graphics.GraphicsDevice.Viewport.Height)) : (frontDoorPositionForFarmer.Y - Game1.graphics.GraphicsDevice.Viewport.Height / 2));
				}
				else if (!eventCommands[1].Equals("follow"))
				{
					try
					{
						string[] array2 = eventCommands[1].Split(' ');
						Game1.viewportFreeze = true;
						int num3 = OffsetTileX(Convert.ToInt32(array2[0])) * 64 + 32;
						int num4 = OffsetTileY(Convert.ToInt32(array2[1])) * 64 + 32;
						if (array2[0][0] == '-')
						{
							Game1.viewport.X = num3;
							Game1.viewport.Y = num4;
						}
						else
						{
							Game1.viewport.X = (Game1.currentLocation.IsOutdoors ? Math.Max(0, Math.Min(num3 - Game1.viewport.Width / 2, Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width)) : (num3 - Game1.viewport.Width / 2));
							Game1.viewport.Y = (Game1.currentLocation.IsOutdoors ? Math.Max(0, Math.Min(num4 - Game1.viewport.Height / 2, Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height)) : (num4 - Game1.viewport.Height / 2));
						}
						if (num3 > 0 && Game1.graphics.GraphicsDevice.Viewport.Width > Game1.currentLocation.Map.DisplayWidth)
						{
							Game1.viewport.X = (Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width) / 2;
						}
						if (num4 > 0 && Game1.graphics.GraphicsDevice.Viewport.Height > Game1.currentLocation.Map.DisplayHeight)
						{
							Game1.viewport.Y = (Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height) / 2;
						}
					}
					catch (Exception)
					{
						forked = true;
						return;
					}
				}
				setUpCharacters(eventCommands[2], location);
				trySpecialSetUp(location);
				populateWalkLocationsList();
				CurrentCommand = 3;
				return;
			}
			if (!Game1.fadeToBlack || actorPositionsAfterMove.Count > 0 || CurrentCommand > 3 || forked)
			{
				if (eventCommands.Length <= CurrentCommand)
				{
					return;
				}
				_ = viewportTarget;
				if (!viewportTarget.Equals(Vector3.Zero))
				{
					int speed = farmer.speed;
					farmer.speed = (int)viewportTarget.X;
					Game1.viewport.X += (int)viewportTarget.X;
					Game1.viewport.Y += (int)viewportTarget.Y;
					farmer.speed = (int)viewportTarget.Y;
					farmer.speed = speed;
					viewportTarget.Z -= time.ElapsedGameTime.Milliseconds;
					if (viewportTarget.Z <= 0f)
					{
						viewportTarget = Vector3.Zero;
					}
				}
				if (actorPositionsAfterMove.Count > 0)
				{
					string[] array3 = actorPositionsAfterMove.Keys.ToArray();
					string[] array4 = array3;
					foreach (string text in array4)
					{
						Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle((int)actorPositionsAfterMove[text].X * 64, (int)actorPositionsAfterMove[text].Y * 64, 64, 64);
						rectangle.Inflate(-4, 0);
						if (getActorByName(text) != null && getActorByName(text).GetBoundingBox().Width > 64)
						{
							rectangle.Inflate(4, 0);
							rectangle.Width = getActorByName(text).GetBoundingBox().Width + 4;
							rectangle.Height = getActorByName(text).GetBoundingBox().Height + 4;
							rectangle.X += 8;
							rectangle.Y += 16;
						}
						if (text.Contains("farmer"))
						{
							Farmer farmerFromFarmerNumberString = getFarmerFromFarmerNumberString(text, farmer);
							if (farmerFromFarmerNumberString != null && rectangle.Contains(farmerFromFarmerNumberString.GetBoundingBox()) && (((float)(farmerFromFarmerNumberString.GetBoundingBox().Y - rectangle.Top) <= 16f + farmerFromFarmerNumberString.getMovementSpeed() && farmerFromFarmerNumberString.FacingDirection != 2) || ((float)(rectangle.Bottom - farmerFromFarmerNumberString.GetBoundingBox().Bottom) <= 16f + farmerFromFarmerNumberString.getMovementSpeed() && farmerFromFarmerNumberString.FacingDirection == 2)))
							{
								farmerFromFarmerNumberString.showNotCarrying();
								farmerFromFarmerNumberString.Halt();
								farmerFromFarmerNumberString.faceDirection((int)actorPositionsAfterMove[text].Z);
								farmerFromFarmerNumberString.FarmerSprite.StopAnimation();
								farmerFromFarmerNumberString.Halt();
								actorPositionsAfterMove.Remove(text);
							}
							else if (farmerFromFarmerNumberString != null)
							{
								farmerFromFarmerNumberString.canOnlyWalk = false;
								farmerFromFarmerNumberString.setRunning(isRunning: false, force: true);
								farmerFromFarmerNumberString.canOnlyWalk = true;
								farmerFromFarmerNumberString.lastPosition = farmer.Position;
								farmerFromFarmerNumberString.MovePosition(time, Game1.viewport, location);
							}
							continue;
						}
						foreach (NPC actor2 in actors)
						{
							Microsoft.Xna.Framework.Rectangle boundingBox = actor2.GetBoundingBox();
							if (actor2.Name.Equals(text) && rectangle.Contains(boundingBox) && actor2.GetBoundingBox().Y - rectangle.Top <= 16)
							{
								actor2.Halt();
								actor2.faceDirection((int)actorPositionsAfterMove[text].Z);
								actorPositionsAfterMove.Remove(text);
								break;
							}
							if (actor2.Name.Equals(text))
							{
								if (actor2 is Monster)
								{
									actor2.MovePosition(time, Game1.viewport, location);
								}
								else
								{
									actor2.MovePosition(time, Game1.viewport, null);
								}
								break;
							}
						}
					}
					if (actorPositionsAfterMove.Count == 0)
					{
						if (continueAfterMove)
						{
							continueAfterMove = false;
						}
						else
						{
							CurrentCommand++;
						}
					}
					if (!continueAfterMove)
					{
						return;
					}
				}
			}
			tryEventCommand(location, time, array);
		}

		public bool isTileWalkedOn(int x, int y)
		{
			return characterWalkLocations.Contains(new Vector2(x, y));
		}

		private void populateWalkLocationsList()
		{
			Vector2 tileLocation = farmer.getTileLocation();
			characterWalkLocations.Add(tileLocation);
			for (int i = 2; i < eventCommands.Length; i++)
			{
				string[] array = eventCommands[i].Split(' ');
				string text = array[0];
				if (text == "move" && array[1].Equals("farmer"))
				{
					for (int j = 0; j < Math.Abs(Convert.ToInt32(array[2])); j++)
					{
						tileLocation.X += Math.Sign(Convert.ToInt32(array[2]));
						characterWalkLocations.Add(tileLocation);
					}
					for (int k = 0; k < Math.Abs(Convert.ToInt32(array[3])); k++)
					{
						tileLocation.Y += Math.Sign(Convert.ToInt32(array[3]));
						characterWalkLocations.Add(tileLocation);
					}
				}
			}
			foreach (NPC actor in actors)
			{
				tileLocation = actor.getTileLocation();
				characterWalkLocations.Add(tileLocation);
				for (int l = 2; l < eventCommands.Length; l++)
				{
					string[] array2 = eventCommands[l].Split(' ');
					string text2 = array2[0];
					if (text2 == "move" && array2[1].Equals(actor.Name))
					{
						for (int m = 0; m < Math.Abs(Convert.ToInt32(array2[2])); m++)
						{
							tileLocation.X += Math.Sign(Convert.ToInt32(array2[2]));
							characterWalkLocations.Add(tileLocation);
						}
						for (int n = 0; n < Math.Abs(Convert.ToInt32(array2[3])); n++)
						{
							tileLocation.Y += Math.Sign(Convert.ToInt32(array2[3]));
							characterWalkLocations.Add(tileLocation);
						}
					}
				}
			}
		}

		public NPC getActorByName(string name)
		{
			if (name.Equals("rival"))
			{
				name = Utility.getOtherFarmerNames()[0];
			}
			if (name.Equals("spouse"))
			{
				name = farmer.spouse;
			}
			foreach (NPC actor in actors)
			{
				if (actor.Name.Equals(name))
				{
					return actor;
				}
			}
			return null;
		}

		public void applyToAllFarmersByFarmerString(string farmer_string, Action<Farmer> function)
		{
			List<Farmer> list = new List<Farmer>();
			if (farmer_string.Equals("farmer"))
			{
				list.Add(farmer);
			}
			else if (farmer_string.StartsWith("farmer"))
			{
				list.Add(getFarmerFromFarmerNumberString(farmer_string, farmer));
			}
			foreach (Farmer item in list)
			{
				bool flag = false;
				foreach (Farmer farmerActor in farmerActors)
				{
					if (farmerActor.UniqueMultiplayerID == item.UniqueMultiplayerID)
					{
						flag = true;
						function(farmerActor);
						break;
					}
				}
				if (!flag)
				{
					function(item);
				}
			}
		}

		private void addActor(string name, int x, int y, int facingDirection, GameLocation location)
		{
			string textureNameForCharacter = NPC.getTextureNameForCharacter(name);
			if (name.Equals("Krobus_Trenchcoat"))
			{
				name = "Krobus";
			}
			Texture2D portrait = null;
			try
			{
				portrait = Game1.content.Load<Texture2D>("Portraits\\" + (textureNameForCharacter.Equals("WeddingOutfits") ? farmer.spouse : textureNameForCharacter));
			}
			catch (Exception)
			{
			}
			int num = ((name.Contains("Dwarf") || name.Equals("Krobus")) ? 96 : 128);
			NPC nPC = new NPC(new AnimatedSprite("Characters\\" + textureNameForCharacter, 0, 16, num / 4), new Vector2(x * 64, y * 64), location.Name, facingDirection, name.Contains("Rival") ? Utility.getOtherFarmerNames()[0] : name, null, portrait, eventActor: true);
			nPC.eventActor = true;
			if (isFestival)
			{
				try
				{
					nPC.setNewDialogue(GetFestivalDataForYear(nPC.Name));
				}
				catch (Exception)
				{
				}
			}
			if (nPC.name.Equals("MrQi"))
			{
				nPC.displayName = Game1.content.LoadString("Strings\\NPCNames:MisterQi");
			}
			nPC.eventActor = true;
			actors.Add(nPC);
		}

		public Farmer getFarmerFromFarmerNumberString(string name, Farmer defaultFarmer)
		{
			Farmer farmerFromFarmerNumberString = Utility.getFarmerFromFarmerNumberString(name, defaultFarmer);
			if (farmerFromFarmerNumberString == null)
			{
				return null;
			}
			foreach (Farmer farmerActor in farmerActors)
			{
				if (farmerFromFarmerNumberString.UniqueMultiplayerID == farmerActor.UniqueMultiplayerID)
				{
					return farmerActor;
				}
			}
			return farmerFromFarmerNumberString;
		}

		public Character getCharacterByName(string name)
		{
			if (name.Equals("rival"))
			{
				name = Utility.getOtherFarmerNames()[0];
			}
			if (name.Contains("farmer"))
			{
				return getFarmerFromFarmerNumberString(name, farmer);
			}
			foreach (NPC actor in actors)
			{
				if (actor.Name.Equals(name))
				{
					return actor;
				}
			}
			return null;
		}

		public Vector3 getPositionAfterMove(Character c, int xMove, int yMove, int facingDirection)
		{
			Vector2 tileLocation = c.getTileLocation();
			return new Vector3(tileLocation.X + (float)xMove, tileLocation.Y + (float)yMove, facingDirection);
		}

		private void trySpecialSetUp(GameLocation location)
		{
			switch (id)
			{
			case 739330:
				if (!Game1.player.friendshipData.ContainsKey("Willy"))
				{
					Game1.player.friendshipData.Add("Willy", new Friendship(0));
				}
				Game1.player.checkForQuestComplete(Game1.getCharacterFromName("Willy"), -1, -1, null, null, 5);
				break;
			case 9333220:
				if (location is FarmHouse && (location as FarmHouse).upgradeLevel == 1)
				{
					farmer.Position = new Vector2(1920f, 400f);
					getActorByName("Sebastian").setTilePosition(31, 6);
				}
				break;
			case 4324303:
			{
				if (!(location is FarmHouse))
				{
					break;
				}
				Point playerBedSpot = (location as FarmHouse).GetPlayerBedSpot();
				playerBedSpot.X--;
				farmer.Position = new Vector2(playerBedSpot.X * 64, playerBedSpot.Y * 64 + 16);
				getActorByName("Penny").setTilePosition(playerBedSpot.X - 1, playerBedSpot.Y);
				Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(23, 12, 10, 10);
				if ((location as FarmHouse).upgradeLevel == 1)
				{
					rectangle = new Microsoft.Xna.Framework.Rectangle(20, 3, 8, 7);
				}
				Point center = rectangle.Center;
				if (!rectangle.Contains(Game1.player.getTileLocationPoint()))
				{
					List<string> list = new List<string>(eventCommands);
					int num = 56;
					list.Insert(num, "globalFade 0.03");
					num++;
					list.Insert(num, "beginSimultaneousCommand");
					num++;
					list.Insert(num, "viewport " + center.X + " " + center.Y);
					num++;
					list.Insert(num, "globalFadeToClear 0.03");
					num++;
					list.Insert(num, "endSimultaneousCommand");
					num++;
					list.Insert(num, "pause 2000");
					num++;
					list.Insert(num, "globalFade 0.03");
					num++;
					list.Insert(num, "beginSimultaneousCommand");
					num++;
					list.Insert(num, "viewport " + Game1.player.getTileX() + " " + Game1.player.getTileY());
					num++;
					list.Insert(num, "globalFadeToClear 0.03");
					num++;
					list.Insert(num, "endSimultaneousCommand");
					num++;
					eventCommands = list.ToArray();
				}
				for (int i = 0; i < eventCommands.Length; i++)
				{
					if (eventCommands[i].StartsWith("makeInvisible"))
					{
						string[] array = eventCommands[i].Split(' ');
						array[1] = (int.Parse(array[1]) - 26 + playerBedSpot.X).ToString() ?? "";
						array[2] = (int.Parse(array[2]) - 13 + playerBedSpot.Y).ToString() ?? "";
						if (location.getObjectAtTile(int.Parse(array[1]), int.Parse(array[2])) == (location as FarmHouse).GetPlayerBed())
						{
							eventCommands[i] = "makeInvisible -1000 -1000";
						}
						else
						{
							eventCommands[i] = string.Join(" ", array);
						}
					}
				}
				break;
			}
			case 4325434:
				if (location is FarmHouse && (location as FarmHouse).upgradeLevel == 1)
				{
					farmer.Position = new Vector2(512f, 336f);
					getActorByName("Penny").setTilePosition(5, 5);
				}
				break;
			case 3912132:
			{
				if (!(location is FarmHouse))
				{
					break;
				}
				Point playerBedSpot2 = (location as FarmHouse).GetPlayerBedSpot();
				playerBedSpot2.X--;
				if (!location.isTileLocationTotallyClearAndPlaceable(Utility.PointToVector2(playerBedSpot2) + new Vector2(-2f, 0f)))
				{
					playerBedSpot2.X++;
				}
				farmer.setTileLocation(Utility.PointToVector2(playerBedSpot2));
				getActorByName("Elliott").setTileLocation(Utility.PointToVector2(playerBedSpot2) + new Vector2(-2f, 0f));
				for (int j = 0; j < eventCommands.Length; j++)
				{
					if (eventCommands[j].StartsWith("makeInvisible"))
					{
						string[] array2 = eventCommands[j].Split(' ');
						array2[1] = (int.Parse(array2[1]) - 26 + playerBedSpot2.X).ToString() ?? "";
						array2[2] = (int.Parse(array2[2]) - 13 + playerBedSpot2.Y).ToString() ?? "";
						if (location.getObjectAtTile(int.Parse(array2[1]), int.Parse(array2[2])) == (location as FarmHouse).GetPlayerBed())
						{
							eventCommands[j] = "makeInvisible -1000 -1000";
						}
						else
						{
							eventCommands[j] = string.Join(" ", array2);
						}
					}
				}
				break;
			}
			case 8675611:
				if (location is FarmHouse && (location as FarmHouse).upgradeLevel == 1)
				{
					getActorByName("Haley").setTilePosition(4, 5);
					farmer.Position = new Vector2(320f, 336f);
				}
				break;
			case 3917601:
				if (!(location is DecoratableLocation))
				{
					break;
				}
				foreach (Furniture item in (location as DecoratableLocation).furniture)
				{
					if ((int)item.furniture_type == 14 && location.isTileLocationTotallyClearAndPlaceableIgnoreFloors(item.TileLocation + new Vector2(0f, 1f)) && location.isTileLocationTotallyClearAndPlaceableIgnoreFloors(item.TileLocation + new Vector2(1f, 1f)))
					{
						getActorByName("Emily").setTilePosition((int)item.TileLocation.X, (int)item.TileLocation.Y + 1);
						farmer.Position = new Vector2((item.TileLocation.X + 1f) * 64f, (item.tileLocation.Y + 1f) * 64f + 16f);
						item.isOn.Value = true;
						item.setFireplace(location, playSound: false);
						return;
					}
				}
				if (location is FarmHouse && (location as FarmHouse).upgradeLevel == 1)
				{
					getActorByName("Emily").setTilePosition(4, 5);
					farmer.Position = new Vector2(320f, 336f);
				}
				break;
			case 3917666:
				if (location is FarmHouse && (location as FarmHouse).upgradeLevel == 1)
				{
					getActorByName("Maru").setTilePosition(4, 5);
					farmer.Position = new Vector2(320f, 336f);
				}
				break;
			}
		}

		private void setUpCharacters(string description, GameLocation location)
		{
			this.farmer.Halt();
			if ((Game1.player.locationBeforeForcedEvent.Value == null || Game1.player.locationBeforeForcedEvent.Value == "") && !isMemory)
			{
				Game1.player.positionBeforeEvent = Game1.player.getTileLocation();
				Game1.player.orientationBeforeEvent = Game1.player.FacingDirection;
			}
			string[] array = description.Split(' ');
			for (int i = 0; i < array.Length; i += 4)
			{
				if (array[i + 1].Equals("-1") && !array[i].Equals("farmer"))
				{
					foreach (NPC character in location.getCharacters())
					{
						if (character.Name.Equals(array[i]))
						{
							actors.Add(character);
						}
					}
				}
				else if (!array[i].Equals("farmer"))
				{
					if (array[i].Equals("otherFarmers"))
					{
						int num = OffsetTileX(Convert.ToInt32(array[i + 1]));
						int num2 = OffsetTileY(Convert.ToInt32(array[i + 2]));
						int direction = Convert.ToInt32(array[i + 3]);
						foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
						{
							if (onlineFarmer.UniqueMultiplayerID != this.farmer.UniqueMultiplayerID)
							{
								Farmer farmer = onlineFarmer.CreateFakeEventFarmer();
								farmer.completelyStopAnimatingOrDoingAction();
								farmer.hidden.Value = false;
								farmer.faceDirection(direction);
								farmer.setTileLocation(new Vector2(num, num2));
								farmer.currentLocation = Game1.currentLocation;
								num++;
								farmerActors.Add(farmer);
							}
						}
						continue;
					}
					if (array[i].Contains("farmer"))
					{
						int num3 = OffsetTileX(Convert.ToInt32(array[i + 1]));
						int num4 = OffsetTileY(Convert.ToInt32(array[i + 2]));
						int direction2 = Convert.ToInt32(array[i + 3]);
						int number = Convert.ToInt32(array[i].Last().ToString() ?? "");
						Farmer farmerFromFarmerNumber = Utility.getFarmerFromFarmerNumber(number);
						if (farmerFromFarmerNumber != null)
						{
							Farmer farmer2 = farmerFromFarmerNumber.CreateFakeEventFarmer();
							farmer2.completelyStopAnimatingOrDoingAction();
							farmer2.hidden.Value = false;
							farmer2.faceDirection(direction2);
							farmer2.setTileLocation(new Vector2(num3, num4));
							farmer2.currentLocation = Game1.currentLocation;
							farmer2.isFakeEventActor = true;
							farmerActors.Add(farmer2);
						}
						continue;
					}
					string name = array[i];
					if (array[i].Equals("spouse"))
					{
						name = this.farmer.spouse;
					}
					if (array[i].Equals("rival"))
					{
						name = (this.farmer.IsMale ? "maleRival" : "femaleRival");
					}
					if (array[i].Equals("cat"))
					{
						actors.Add(new Cat(OffsetTileX(Convert.ToInt32(array[i + 1])), OffsetTileY(Convert.ToInt32(array[i + 2])), Game1.player.whichPetBreed));
						actors.Last().Name = "Cat";
						actors.Last().position.X -= 32f;
						continue;
					}
					if (array[i].Equals("dog"))
					{
						actors.Add(new Dog(OffsetTileX(Convert.ToInt32(array[i + 1])), OffsetTileY(Convert.ToInt32(array[i + 2])), Game1.player.whichPetBreed));
						actors.Last().Name = "Dog";
						actors.Last().position.X -= 42f;
						continue;
					}
					if (array[i].Equals("golem"))
					{
						actors.Add(new NPC(new AnimatedSprite("Characters\\Monsters\\Wilderness Golem", 0, 16, 24), OffsetPosition(new Vector2(Convert.ToInt32(array[i + 1]), Convert.ToInt32(array[i + 2])) * 64f), 0, "Golem"));
						continue;
					}
					if (array[i].Equals("Junimo"))
					{
						actors.Add(new Junimo(OffsetPosition(new Vector2(Convert.ToInt32(array[i + 1]) * 64, Convert.ToInt32(array[i + 2]) * 64 - 32)), Game1.currentLocation.Name.Equals("AbandonedJojaMart") ? 6 : (-1))
						{
							Name = "Junimo",
							EventActor = true
						});
						continue;
					}
					int x = OffsetTileX(Convert.ToInt32(array[i + 1]));
					int y = OffsetTileY(Convert.ToInt32(array[i + 2]));
					int facingDirection = Convert.ToInt32(array[i + 3]);
					if (location is Farm && id != -2 && !ignoreTileOffsets)
					{
						x = Farm.getFrontDoorPositionForFarmer(this.farmer).X;
						y = Farm.getFrontDoorPositionForFarmer(this.farmer).Y + 2;
						facingDirection = 0;
					}
					addActor(name, x, y, facingDirection, location);
				}
				else if (!array[i + 1].Equals("-1"))
				{
					this.farmer.position.X = OffsetPositionX(Convert.ToInt32(array[i + 1]) * 64);
					this.farmer.position.Y = OffsetPositionY(Convert.ToInt32(array[i + 2]) * 64 + 16);
					this.farmer.faceDirection(Convert.ToInt32(array[i + 3]));
					if (location is Farm && id != -2 && !ignoreTileOffsets)
					{
						this.farmer.position.X = Farm.getFrontDoorPositionForFarmer(this.farmer).X * 64;
						this.farmer.position.Y = (Farm.getFrontDoorPositionForFarmer(this.farmer).Y + 1) * 64;
						this.farmer.faceDirection(2);
					}
					if (isSpecificFestival("winter8"))
					{
						this.farmer.position.Y += 128f;
						this.farmer.faceDirection(2);
					}
					this.farmer.FarmerSprite.StopAnimation();
				}
			}
		}

		private void beakerSmashEndFunction(int extraInfo)
		{
			Game1.playSound("breakingGlass");
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(47, new Vector2(9f, 16f) * 64f, Color.LightBlue, 10));
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(400, 3008, 64, 64), 99999f, 2, 0, new Vector2(9f, 16f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.LightBlue, 1f, 0f, 0f, 0f)
			{
				delayBeforeAnimationStart = 700
			});
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(46, new Vector2(9f, 16f) * 64f, Color.White * 0.75f, 10)
			{
				motion = new Vector2(0f, -1f)
			});
		}

		private void eggSmashEndFunction(int extraInfo)
		{
			Game1.playSound("slimedead");
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(47, new Vector2(9f, 16f) * 64f, Color.White, 10));
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(177, 99999f, 9999, 0, new Vector2(6f, 5f) * 64f, flicker: false, flipped: false)
			{
				layerDepth = 1E-06f
			});
		}

		private void balloonInSky(int extraInfo)
		{
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(2);
			if (temporarySpriteByID != null)
			{
				temporarySpriteByID.motion = Vector2.Zero;
			}
			temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(1);
			if (temporarySpriteByID != null)
			{
				temporarySpriteByID.motion = Vector2.Zero;
			}
		}

		private void marcelloBalloonLand(int extraInfo)
		{
			Game1.playSound("thudStep");
			Game1.playSound("dirtyHit");
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(2);
			if (temporarySpriteByID != null)
			{
				temporarySpriteByID.motion = Vector2.Zero;
			}
			temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(3);
			if (temporarySpriteByID != null)
			{
				temporarySpriteByID.scaleChange = 0f;
			}
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 2944, 64, 64), 120f, 8, 1, (new Vector2(25f, 39f) + eventPositionTileOffset) * 64f + new Vector2(-32f, 32f), flicker: false, flipped: true, 1f, 0f, Color.White, 1f, 0f, 0f, 0f));
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 2944, 64, 64), 120f, 8, 1, (new Vector2(27f, 39f) + eventPositionTileOffset) * 64f + new Vector2(0f, 48f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
			{
				delayBeforeAnimationStart = 300
			});
			CurrentCommand++;
		}

		private void samPreOllie(int extraInfo)
		{
			NPC actorByName = getActorByName("Sam");
			actorByName.Sprite.currentFrame = 27;
			farmer.faceDirection(0);
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
			temporarySpriteByID.xStopCoordinate = 1408;
			temporarySpriteByID.reachedStopCoordinate = samOllie;
			temporarySpriteByID.motion = new Vector2(2f, 0f);
		}

		private void samOllie(int extraInfo)
		{
			Game1.playSound("crafting");
			NPC actorByName = getActorByName("Sam");
			actorByName.Sprite.currentFrame = 26;
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
			temporarySpriteByID.currentNumberOfLoops = 0;
			temporarySpriteByID.totalNumberOfLoops = 1;
			temporarySpriteByID.motion.Y = -9f;
			temporarySpriteByID.motion.X = 2f;
			temporarySpriteByID.acceleration = new Vector2(0f, 0.4f);
			temporarySpriteByID.animationLength = 1;
			temporarySpriteByID.interval = 530f;
			temporarySpriteByID.timer = 0f;
			temporarySpriteByID.endFunction = samGrind;
			temporarySpriteByID.destroyable = false;
		}

		private void samGrind(int extraInfo)
		{
			Game1.playSound("hammer");
			NPC actorByName = getActorByName("Sam");
			actorByName.Sprite.currentFrame = 28;
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
			temporarySpriteByID.currentNumberOfLoops = 0;
			temporarySpriteByID.totalNumberOfLoops = 9999;
			temporarySpriteByID.motion.Y = 0f;
			temporarySpriteByID.motion.X = 2f;
			temporarySpriteByID.acceleration = new Vector2(0f, 0f);
			temporarySpriteByID.animationLength = 1;
			temporarySpriteByID.interval = 99999f;
			temporarySpriteByID.timer = 0f;
			temporarySpriteByID.xStopCoordinate = 1664;
			temporarySpriteByID.yStopCoordinate = -1;
			temporarySpriteByID.reachedStopCoordinate = samDropOff;
		}

		private void samDropOff(int extraInfo)
		{
			NPC actorByName = getActorByName("Sam");
			actorByName.Sprite.currentFrame = 31;
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
			temporarySpriteByID.currentNumberOfLoops = 9999;
			temporarySpriteByID.totalNumberOfLoops = 0;
			temporarySpriteByID.motion.Y = 0f;
			temporarySpriteByID.motion.X = 2f;
			temporarySpriteByID.acceleration = new Vector2(0f, 0.4f);
			temporarySpriteByID.animationLength = 1;
			temporarySpriteByID.interval = 99999f;
			temporarySpriteByID.yStopCoordinate = 5760;
			temporarySpriteByID.reachedStopCoordinate = samGround;
			temporarySpriteByID.endFunction = null;
			actorByName.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(29, 100),
				new FarmerSprite.AnimationFrame(30, 100),
				new FarmerSprite.AnimationFrame(31, 100),
				new FarmerSprite.AnimationFrame(32, 100)
			});
			actorByName.Sprite.loop = false;
		}

		private void samGround(int extraInfo)
		{
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
			Game1.playSound("thudStep");
			temporarySpriteByID.attachedCharacter = null;
			temporarySpriteByID.reachedStopCoordinate = null;
			temporarySpriteByID.totalNumberOfLoops = -1;
			temporarySpriteByID.interval = 0f;
			temporarySpriteByID.destroyable = true;
			CurrentCommand++;
		}

		private void catchFootball(int extraInfo)
		{
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(56232);
			Game1.playSound("fishSlap");
			temporarySpriteByID.motion = new Vector2(2f, -8f);
			temporarySpriteByID.rotationChange = (float)Math.PI / 24f;
			temporarySpriteByID.reachedStopCoordinate = footballLand;
			temporarySpriteByID.yStopCoordinate = 1088;
			farmer.jump();
		}

		private void footballLand(int extraInfo)
		{
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(56232);
			Game1.playSound("sandyStep");
			temporarySpriteByID.motion = new Vector2(0f, 0f);
			temporarySpriteByID.rotationChange = 0f;
			temporarySpriteByID.reachedStopCoordinate = null;
			temporarySpriteByID.animationLength = 1;
			temporarySpriteByID.interval = 999999f;
			CurrentCommand++;
		}

		private void parrotSplat(int extraInfo)
		{
			Game1.playSound("drumkit0");
			DelayedAction.playSoundAfterDelay("drumkit5", 100);
			Game1.playSound("slimeHit");
			foreach (TemporaryAnimatedSprite aboveMapSprite in aboveMapSprites)
			{
				aboveMapSprite.alpha = 0f;
			}
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(174, 168, 4, 11), 99999f, 1, 99999, new Vector2(1504f, 5568f), flicker: false, flipped: false, 0.02f, 0.01f, Color.White, 4f, 0f, (float)Math.PI / 2f, (float)Math.PI / 64f)
			{
				motion = new Vector2(2f, -2f),
				acceleration = new Vector2(0f, 0.1f)
			});
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(174, 168, 4, 11), 99999f, 1, 99999, new Vector2(1504f, 5568f), flicker: false, flipped: false, 0.02f, 0.01f, Color.White, 4f, 0f, (float)Math.PI / 4f, (float)Math.PI / 64f)
			{
				motion = new Vector2(-2f, -1f),
				acceleration = new Vector2(0f, 0.1f)
			});
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(174, 168, 4, 11), 99999f, 1, 99999, new Vector2(1504f, 5568f), flicker: false, flipped: false, 0.02f, 0.01f, Color.White, 4f, 0f, (float)Math.PI, (float)Math.PI / 64f)
			{
				motion = new Vector2(1f, 1f),
				acceleration = new Vector2(0f, 0.1f)
			});
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(174, 168, 4, 11), 99999f, 1, 99999, new Vector2(1504f, 5568f), flicker: false, flipped: false, 0.02f, 0.01f, Color.White, 4f, 0f, 0f, (float)Math.PI / 64f)
			{
				motion = new Vector2(-2f, -2f),
				acceleration = new Vector2(0f, 0.1f)
			});
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(148, 165, 25, 23), 99999f, 1, 99999, new Vector2(1504f, 5568f), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				id = 666f
			});
			CurrentCommand++;
		}

		public virtual Vector2 OffsetPosition(Vector2 original)
		{
			return new Vector2(OffsetPositionX(original.X), OffsetPositionY(original.Y));
		}

		public virtual Vector2 OffsetTile(Vector2 original)
		{
			return new Vector2(OffsetTileX((int)original.X), OffsetTileY((int)original.Y));
		}

		public virtual float OffsetPositionX(float original)
		{
			if (original < 0f || ignoreTileOffsets)
			{
				return original;
			}
			return original + eventPositionTileOffset.X * 64f;
		}

		public virtual float OffsetPositionY(float original)
		{
			if (original < 0f || ignoreTileOffsets)
			{
				return original;
			}
			return original + eventPositionTileOffset.Y * 64f;
		}

		public virtual int OffsetTileX(int original)
		{
			if (original < 0 || ignoreTileOffsets)
			{
				return original;
			}
			return (int)((float)original + eventPositionTileOffset.X);
		}

		public virtual int OffsetTileY(int original)
		{
			if (original < 0 || ignoreTileOffsets)
			{
				return original;
			}
			return (int)((float)original + eventPositionTileOffset.Y);
		}

		private void addSpecificTemporarySprite(string key, GameLocation location, string[] split)
		{
			if (key == null)
			{
				return;
			}
			switch (key.Length)
			{
			case 15:
				switch (key[10])
				{
				case 's':
					if (key == "LeoWillyFishing")
					{
						for (int num9 = 0; num9 < 20; num9++)
						{
							location.TemporarySprites.Add(new TemporaryAnimatedSprite(0, new Vector2(42.5f, 38f) * 64f + new Vector2(Game1.random.Next(64), Game1.random.Next(64)), Color.White * 0.7f)
							{
								layerDepth = (float)(1280 + num9) / 10000f,
								delayBeforeAnimationStart = num9 * 150
							});
						}
					}
					break;
				case 'o':
					if (key == "LeoLinusCooking")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(240, 128, 16, 16), 9999f, 1, 1, new Vector2(29f, 8.5f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							layerDepth = 1f
						});
						for (int num10 = 0; num10 < 10; num10++)
						{
							Utility.addSmokePuff(location, new Vector2(29.5f, 8.6f) * 64f, num10 * 500);
						}
					}
					break;
				case 'L':
					if (key == "BoatParrotLeave")
					{
						TemporaryAnimatedSprite temporaryAnimatedSprite3 = aboveMapSprites.First();
						temporaryAnimatedSprite3.motion = new Vector2(4f, -6f);
						temporaryAnimatedSprite3.sourceRect.X = 48;
						temporaryAnimatedSprite3.sourceRectStartingPos.X = 48f;
						temporaryAnimatedSprite3.animationLength = 3;
						temporaryAnimatedSprite3.pingPong = true;
					}
					break;
				case 'q':
					if (key == "parrotHutSquawk")
					{
						(location as IslandHut).parrotUpgradePerches[0].timeUntilSqwawk = 1f;
					}
					break;
				case 'r':
					if (key == "coldstarMiracle")
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Movies"),
							sourceRect = new Microsoft.Xna.Framework.Rectangle(400, 704, 90, 61),
							sourceRectStartingPos = new Vector2(400f, 704f),
							animationLength = 1,
							totalNumberOfLoops = 1,
							interval = 99999f,
							alpha = 0.01f,
							alphaFade = -0.01f,
							scale = 4f,
							position = new Vector2(4f, 1f) * 64f + new Vector2(3f, 7f) * 4f,
							layerDepth = 0.8535f,
							id = 989f
						});
					}
					break;
				case 'l':
					if (key == "junimoSpotlight")
					{
						actors.First().drawOnTop = true;
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
							sourceRect = new Microsoft.Xna.Framework.Rectangle(316, 123, 67, 43),
							sourceRectStartingPos = new Vector2(316f, 123f),
							animationLength = 1,
							interval = 5000f,
							totalNumberOfLoops = 9999,
							scale = 4f,
							position = Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 268, 172, 0, -20),
							layerDepth = 0.0001f,
							local = true,
							id = 999f
						});
					}
					break;
				case 'e':
					switch (key)
					{
					case "harveyDinnerSet":
					{
						Vector2 vector2 = new Vector2(5f, 16f);
						if (location is DecoratableLocation)
						{
							foreach (Furniture item2 in (location as DecoratableLocation).furniture)
							{
								if ((int)item2.furniture_type == 14 && location.getTileIndexAt((int)item2.tileLocation.X, (int)item2.tileLocation.Y + 1, "Buildings") == -1 && location.getTileIndexAt((int)item2.tileLocation.X + 1, (int)item2.tileLocation.Y + 1, "Buildings") == -1 && location.getTileIndexAt((int)item2.tileLocation.X + 2, (int)item2.tileLocation.Y + 1, "Buildings") == -1 && location.getTileIndexAt((int)item2.tileLocation.X - 1, (int)item2.tileLocation.Y + 1, "Buildings") == -1)
								{
									vector2 = new Vector2((int)item2.TileLocation.X, (int)item2.TileLocation.Y + 1);
									item2.isOn.Value = true;
									item2.setFireplace(location, playSound: false);
									break;
								}
							}
						}
						location.TemporarySprites.Clear();
						getActorByName("Harvey").setTilePosition((int)vector2.X + 2, (int)vector2.Y);
						getActorByName("Harvey").Position = new Vector2(getActorByName("Harvey").Position.X - 32f, getActorByName("Harvey").Position.Y);
						farmer.Position = new Vector2(vector2.X * 64f - 32f, vector2.Y * 64f + 32f);
						Object @object = null;
						@object = location.getObjectAtTile((int)vector2.X, (int)vector2.Y);
						if (@object != null)
						{
							@object.isTemporarilyInvisible = true;
						}
						@object = location.getObjectAtTile((int)vector2.X + 1, (int)vector2.Y);
						if (@object != null)
						{
							@object.isTemporarilyInvisible = true;
						}
						@object = location.getObjectAtTile((int)vector2.X - 1, (int)vector2.Y);
						if (@object != null)
						{
							@object.isTemporarilyInvisible = true;
						}
						@object = location.getObjectAtTile((int)vector2.X + 2, (int)vector2.Y);
						if (@object != null)
						{
							@object.isTemporarilyInvisible = true;
						}
						Texture2D texture7 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture7,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(385, 423, 48, 32),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(385f, 423f),
							interval = 5000f,
							totalNumberOfLoops = 9999,
							position = vector2 * 64f + new Vector2(-8f, -16f) * 4f,
							scale = 4f,
							layerDepth = (vector2.Y + 0.2f) * 64f / 10000f,
							light = true,
							lightRadius = 4f,
							lightcolor = Color.Black
						});
						List<string> list2 = eventCommands.ToList();
						list2.Insert(CurrentCommand + 1, "viewport " + (int)vector2.X + " " + (int)vector2.Y + " true");
						eventCommands = list2.ToArray();
						break;
					}
					case "ClothingTherapy":
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(644, 1405, 28, 46), 999999f, 1, 99999, new Vector2(5f, 6f) * 64f + new Vector2(-32f, -144f), flicker: false, flipped: false, 0.0424f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 999f
						});
						break;
					case "getEndSlideshow":
					{
						Summit summit = location as Summit;
						string[] collection = summit.getEndSlideshow().Split('/');
						List<string> list = eventCommands.ToList();
						list.InsertRange(CurrentCommand + 1, collection);
						eventCommands = list.ToArray();
						summit.isShowingEndSlideshow = true;
						break;
					}
					}
					break;
				case 'n':
					switch (key)
					{
					case "shaneSaloonCola":
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.mouseCursors,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(552, 1862, 31, 21),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(552f, 1862f),
							interval = 999999f,
							totalNumberOfLoops = 99999,
							position = new Vector2(32f, 17f) * 64f + new Vector2(10f, 3f) * 4f,
							scale = 4f,
							layerDepth = 1E-07f
						});
						break;
					case "springOnionPeel":
					{
						TemporaryAnimatedSprite temporarySpriteByID4 = location.getTemporarySpriteByID(777);
						temporarySpriteByID4.sourceRectStartingPos = new Vector2(144f, 327f);
						temporarySpriteByID4.sourceRect = new Microsoft.Xna.Framework.Rectangle(144, 327, 112, 112);
						break;
					}
					case "springOnionDemo":
					{
						Texture2D texture6 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture6,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(144, 215, 112, 112),
							animationLength = 2,
							sourceRectStartingPos = new Vector2(144f, 215f),
							interval = 200f,
							totalNumberOfLoops = 99999,
							id = 777f,
							position = new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 264, Game1.graphics.GraphicsDevice.Viewport.Height / 3 - 264),
							local = true,
							scale = 4f,
							destroyable = false,
							overrideLocationDestroy = true
						});
						break;
					}
					case "sebastianOnBike":
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 1600, 64, 128), 80f, 8, 9999, new Vector2(19f, 27f) * 64f + new Vector2(32f, -16f), flicker: false, flipped: true, 0.1792f, 0f, Color.White, 1f, 0f, 0f, 0f));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(405, 1854, 47, 33), 9999f, 1, 999, new Vector2(17f, 27f) * 64f + new Vector2(0f, -8f), flicker: false, flipped: false, 0.1792f, 0f, Color.White, 4f, 0f, 0f, 0f));
						break;
					}
					break;
				case 'P':
					if (key == "shaneCliffProps")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(549, 1891, 19, 12), 99999f, 1, 99999, new Vector2(104f, 96f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 999f
						});
					}
					break;
				case 'm':
					if (key == "grandpaThumbsUp")
					{
						TemporaryAnimatedSprite temporarySpriteByID3 = location.getTemporarySpriteByID(77777);
						temporarySpriteByID3.texture = Game1.mouseCursors2;
						temporarySpriteByID3.sourceRect = new Microsoft.Xna.Framework.Rectangle(186, 265, 22, 34);
						temporarySpriteByID3.sourceRectStartingPos = new Vector2(186f, 265f);
						temporarySpriteByID3.yPeriodic = true;
						temporarySpriteByID3.yPeriodicLoopTime = 1000f;
						temporarySpriteByID3.yPeriodicRange = 16f;
						temporarySpriteByID3.xPeriodicLoopTime = 2500f;
						temporarySpriteByID3.xPeriodicRange = 16f;
						temporarySpriteByID3.initialPosition = temporarySpriteByID3.position;
					}
					break;
				case 'G':
					if (key == "junimoCageGone2")
					{
						location.removeTemporarySpritesWithID(1);
						Game1.viewportFreeze = true;
						Game1.viewport.X = -1000;
						Game1.viewport.Y = -1000;
					}
					break;
				case 'C':
					if (key == "iceFishingCatch")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 500f, 3, 99999, new Vector2(68f, 30f) * 64f, flicker: false, flipped: false, 0.1984f, 0f, Color.White, 4f, 0f, 0f, 0f));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 510f, 3, 99999, new Vector2(74f, 30f) * 64f, flicker: false, flipped: false, 0.1984f, 0f, Color.White, 4f, 0f, 0f, 0f));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 490f, 3, 99999, new Vector2(67f, 36f) * 64f, flicker: false, flipped: false, 0.2368f, 0f, Color.White, 4f, 0f, 0f, 0f));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 500f, 3, 99999, new Vector2(76f, 35f) * 64f, flicker: false, flipped: false, 0.2304f, 0f, Color.White, 4f, 0f, 0f, 0f));
					}
					break;
				case 'a':
					if (key == "sebastianGarage")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1843, 48, 42), 9999f, 1, 999, new Vector2(17f, 23f) * 64f + new Vector2(0f, 8f), flicker: false, flipped: false, 0.1472f, 0f, Color.White, 4f, 0f, 0f, 0f));
						getActorByName("Sebastian").HideShadow = true;
					}
					break;
				case 'c':
					if (key == "abbyvideoscreen")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(167, 1714, 19, 14), 100f, 3, 9999, new Vector2(2f, 3f) * 64f + new Vector2(7f, 12f) * 4f, flicker: false, flipped: false, 0.0002f, 0f, Color.White, 4f, 0f, 0f, 0f));
					}
					break;
				}
				break;
			case 16:
				switch (key[0])
				{
				case 'B':
					if (key == "BoatParrotSquawk")
					{
						TemporaryAnimatedSprite temporaryAnimatedSprite = aboveMapSprites.First();
						temporaryAnimatedSprite.sourceRect.X = 24;
						temporaryAnimatedSprite.sourceRectStartingPos.X = 24f;
						Game1.playSound("parrot_squawk");
					}
					break;
				case 'i':
					if (key == "islandFishSplash")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(336, 544, 16, 16), 100000f, 1, 1, new Vector2(81f, 92f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 9999f,
							motion = new Vector2(-2f, -8f),
							acceleration = new Vector2(0f, 0.2f),
							flipped = true,
							rotationChange = -0.02f,
							yStopCoordinate = 5952,
							layerDepth = 0.99f,
							reachedStopCoordinate = delegate
							{
								location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(48, 16, 16, 16), 100f, 5, 1, location.getTemporarySpriteByID(9999).position, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
								{
									layerDepth = 1f
								});
								location.removeTemporarySpritesWithID(9999);
								Game1.playSound("waterSlosh");
							}
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(48, 16, 16, 16), 100f, 5, 1, new Vector2(81f, 92f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							layerDepth = 1f
						});
					}
					break;
				case 't':
					if (key == "trashBearPrelude")
					{
						Utility.addStarsAndSpirals(location, 95, 106, 23, 4, 10000, 275, Color.Lime);
					}
					break;
				case 'l':
					if (key == "leahHoldPainting")
					{
						Texture2D texture5 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
						location.getTemporarySpriteByID(999).sourceRect.X += 15;
						location.getTemporarySpriteByID(999).sourceRectStartingPos.X += 15f;
						int num8 = ((!Game1.netWorldState.Value.hasWorldStateID("m_painting0")) ? (Game1.netWorldState.Value.hasWorldStateID("m_painting1") ? 1 : 2) : 0);
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture5,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(400 + num8 * 25, 394, 25, 23),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(400 + num8 * 25, 394f),
							interval = 5000f,
							totalNumberOfLoops = 9999,
							position = new Vector2(73f, 38f) * 64f + new Vector2(-2f, -16f) * 4f,
							scale = 4f,
							layerDepth = 1f,
							id = 777f
						});
					}
					break;
				case 'E':
					if (key == "EmilyBoomBoxStop")
					{
						location.getTemporarySpriteByID(999).pulse = false;
						location.getTemporarySpriteByID(999).scale = 4f;
					}
					break;
				case 'w':
					if (key == "wizardSewerMagic")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), 50f, 4, 20, new Vector2(15f, 13f) * 64f + new Vector2(8f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							light = true,
							lightRadius = 1f,
							lightcolor = Color.Black,
							alphaFade = 0.005f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), 50f, 4, 20, new Vector2(17f, 13f) * 64f + new Vector2(8f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							light = true,
							lightRadius = 1f,
							lightcolor = Color.Black,
							alphaFade = 0.005f
						});
					}
					break;
				case 'm':
					if (key == "moonlightJellies")
					{
						if (npcControllers != null)
						{
							npcControllers.Clear();
						}
						underwaterSprites = new List<TemporaryAnimatedSprite>();
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(26f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2560,
							delayBeforeAnimationStart = 10000,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(29f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2560,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(31f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2624,
							delayBeforeAnimationStart = 12000,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(20f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 1728,
							delayBeforeAnimationStart = 14000,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(17f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 1856,
							delayBeforeAnimationStart = 19500,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(16f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2048,
							delayBeforeAnimationStart = 20300,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(17f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2496,
							delayBeforeAnimationStart = 21500,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(16f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2816,
							delayBeforeAnimationStart = 22400,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(12f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2688,
							delayBeforeAnimationStart = 23200,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(9f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2752,
							delayBeforeAnimationStart = 24000,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(18f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 1920,
							delayBeforeAnimationStart = 24600,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(33f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2560,
							delayBeforeAnimationStart = 25600,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(36f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2496,
							delayBeforeAnimationStart = 26900,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(21f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1.5f),
							xPeriodic = true,
							xPeriodicLoopTime = 2500f,
							xPeriodicRange = 10f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2176,
							delayBeforeAnimationStart = 28000,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(20f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1.5f),
							xPeriodic = true,
							xPeriodicLoopTime = 2500f,
							xPeriodicRange = 10f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2240,
							delayBeforeAnimationStart = 28500,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(22f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1.5f),
							xPeriodic = true,
							xPeriodicLoopTime = 2500f,
							xPeriodicRange = 10f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2304,
							delayBeforeAnimationStart = 28500,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(33f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1.5f),
							xPeriodic = true,
							xPeriodicLoopTime = 2500f,
							xPeriodicRange = 10f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2752,
							delayBeforeAnimationStart = 29000,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(36f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1.5f),
							xPeriodic = true,
							xPeriodicLoopTime = 2500f,
							xPeriodicRange = 10f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2752,
							delayBeforeAnimationStart = 30000,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 32, 16, 16), 250f, 3, 9999, new Vector2(28f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-0.5f, -0.5f),
							xPeriodic = true,
							xPeriodicLoopTime = 4000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 2f,
							xStopCoordinate = 1216,
							yStopCoordinate = 2432,
							delayBeforeAnimationStart = 32000,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(40f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2560,
							delayBeforeAnimationStart = 10000,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(42f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2752,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(43f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2624,
							delayBeforeAnimationStart = 12000,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(45f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2496,
							delayBeforeAnimationStart = 14000,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(46f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 1856,
							delayBeforeAnimationStart = 19500,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(48f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2240,
							delayBeforeAnimationStart = 20300,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(49f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2560,
							delayBeforeAnimationStart = 21500,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(50f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 1920,
							delayBeforeAnimationStart = 22400,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(51f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2112,
							delayBeforeAnimationStart = 23200,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(52f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2432,
							delayBeforeAnimationStart = 24000,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(53f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2240,
							delayBeforeAnimationStart = 24600,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(54f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 1920,
							delayBeforeAnimationStart = 25600,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(55f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2560,
							delayBeforeAnimationStart = 26900,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(4f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 1920,
							delayBeforeAnimationStart = 24000,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(5f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2560,
							delayBeforeAnimationStart = 24600,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(3f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2176,
							delayBeforeAnimationStart = 25600,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(6f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2368,
							delayBeforeAnimationStart = 26900,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(8f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2688,
							delayBeforeAnimationStart = 26900,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(50f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1.5f),
							xPeriodic = true,
							xPeriodicLoopTime = 2500f,
							xPeriodicRange = 10f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2688,
							delayBeforeAnimationStart = 28500,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(51f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1.5f),
							xPeriodic = true,
							xPeriodicLoopTime = 2500f,
							xPeriodicRange = 10f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2752,
							delayBeforeAnimationStart = 28500,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(52f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1.5f),
							xPeriodic = true,
							xPeriodicLoopTime = 2500f,
							xPeriodicRange = 10f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2816,
							delayBeforeAnimationStart = 29000,
							pingPong = true
						});
					}
					break;
				case 'a':
					if (key == "abbyOuijaCandles")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(737, 999999f, 1, 0, new Vector2(5f, 9f) * 64f, flicker: false, flipped: false)
						{
							light = true,
							lightRadius = 1f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(737, 999999f, 1, 0, new Vector2(7f, 8f) * 64f, flicker: false, flipped: false)
						{
							light = true,
							lightRadius = 1f
						});
					}
					break;
				}
				break;
			case 10:
				switch (key[6])
				{
				case 'r':
					if (!(key == "BoatParrot"))
					{
						if (key == "movieFrame")
						{
							int num3 = Convert.ToInt32(split[2]);
							int num4 = Convert.ToInt32(split[3]);
							int num5 = Convert.ToInt32(split[4]);
							int num6 = num3 * 128 + num4 / 5 * 64;
							int num7 = num4 % 5 * 96;
							location.temporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Movies"),
								sourceRect = new Microsoft.Xna.Framework.Rectangle(16 + num7, num6, 90, 61),
								sourceRectStartingPos = new Vector2(16 + num7, num6),
								animationLength = 1,
								totalNumberOfLoops = 1,
								interval = num5,
								scale = 4f,
								position = new Vector2(4f, 1f) * 64f + new Vector2(3f, 7f) * 4f,
								shakeIntensity = 0.25f,
								layerDepth = 0.85f + (float)(num3 * num4) / 10000f,
								id = 997f
							});
						}
						break;
					}
					aboveMapSprites = new List<TemporaryAnimatedSprite>();
					aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Microsoft.Xna.Framework.Rectangle(48, 0, 24, 24), 100f, 3, 99999, new Vector2(Game1.viewport.X - 64, 2112f), flicker: false, flipped: true, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 999f,
						motion = new Vector2(6f, 1f),
						delayBeforeAnimationStart = 0,
						pingPong = true,
						xStopCoordinate = 1040,
						reachedStopCoordinate = delegate
						{
							TemporaryAnimatedSprite temporaryAnimatedSprite9 = aboveMapSprites.First();
							if (temporaryAnimatedSprite9 != null)
							{
								temporaryAnimatedSprite9.motion = new Vector2(0f, 2f);
								temporaryAnimatedSprite9.yStopCoordinate = 2336;
								temporaryAnimatedSprite9.reachedStopCoordinate = delegate
								{
									TemporaryAnimatedSprite temporaryAnimatedSprite10 = aboveMapSprites.First();
									temporaryAnimatedSprite10.animationLength = 1;
									temporaryAnimatedSprite10.pingPong = false;
									temporaryAnimatedSprite10.sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 24, 24);
									temporaryAnimatedSprite10.sourceRectStartingPos = Vector2.Zero;
								};
							}
						}
					});
					break;
				case 'b':
					if (key == "evilRabbit")
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters"),
							sourceRect = new Microsoft.Xna.Framework.Rectangle(264, 209, 19, 16),
							sourceRectStartingPos = new Vector2(264f, 209f),
							animationLength = 1,
							totalNumberOfLoops = 999,
							interval = 999f,
							scale = 4f,
							position = new Vector2(4f, 1f) * 64f + new Vector2(38f, 23f) * 4f,
							layerDepth = 1f,
							motion = new Vector2(-2f, -2f),
							acceleration = new Vector2(0f, 0.1f),
							yStopCoordinate = 204,
							xStopCoordinate = 316,
							flipped = true,
							id = 778f
						});
					}
					break;
				case 'S':
					if (key == "junimoShow")
					{
						Texture2D texture4 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture4,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(393, 350, 19, 14),
							animationLength = 6,
							sourceRectStartingPos = new Vector2(393f, 350f),
							interval = 90f,
							totalNumberOfLoops = 86,
							position = new Vector2(37f, 14f) * 64f + new Vector2(7f, -2f) * 4f,
							scale = 4f,
							layerDepth = 0.95f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture4,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(393, 364, 19, 14),
							animationLength = 4,
							sourceRectStartingPos = new Vector2(393f, 364f),
							interval = 90f,
							totalNumberOfLoops = 31,
							position = new Vector2(37f, 14f) * 64f + new Vector2(7f, -2f) * 4f,
							scale = 4f,
							layerDepth = 0.97f,
							delayBeforeAnimationStart = 11034
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture4,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(393, 378, 19, 14),
							animationLength = 6,
							sourceRectStartingPos = new Vector2(393f, 378f),
							interval = 90f,
							totalNumberOfLoops = 21,
							position = new Vector2(37f, 14f) * 64f + new Vector2(7f, -2f) * 4f,
							scale = 4f,
							layerDepth = 1f,
							delayBeforeAnimationStart = 22069
						});
					}
					break;
				case 'o':
					if (!(key == "luauShorts"))
					{
						if (key == "linusMoney")
						{
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-1002f, -1000f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								startSound = "money",
								delayBeforeAnimationStart = 10,
								overrideLocationDestroy = true
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-1003f, -1002f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								startSound = "money",
								delayBeforeAnimationStart = 100,
								overrideLocationDestroy = true
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-999f, -1000f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								startSound = "money",
								delayBeforeAnimationStart = 200,
								overrideLocationDestroy = true
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-1004f, -1001f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								startSound = "money",
								delayBeforeAnimationStart = 300,
								overrideLocationDestroy = true
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-1001f, -998f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								startSound = "money",
								delayBeforeAnimationStart = 400,
								overrideLocationDestroy = true
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-998f, -999f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								startSound = "money",
								delayBeforeAnimationStart = 500,
								overrideLocationDestroy = true
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-998f, -1002f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								startSound = "money",
								delayBeforeAnimationStart = 600,
								overrideLocationDestroy = true
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-997f, -1001f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								startSound = "money",
								delayBeforeAnimationStart = 700,
								overrideLocationDestroy = true
							});
						}
					}
					else
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(336, 512, 16, 16), 9999f, 1, 99999, new Vector2(35f, 10f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-2f, -8f),
							acceleration = new Vector2(0f, 0.25f),
							yStopCoordinate = 704,
							xStopCoordinate = 2112
						});
					}
					break;
				case 'B':
				{
					if (!(key == "arcaneBook"))
					{
						if (key == "candleBoat")
						{
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(240, 112, 16, 32), 1000f, 2, 99999, new Vector2(22f, 36f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								id = 1f,
								light = true,
								lightRadius = 2f,
								lightcolor = Color.Black
							});
						}
						break;
					}
					for (int m = 0; m < 16; m++)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(128f, 792f) + new Vector2(Game1.random.Next(32), Game1.random.Next(32) - m * 4), flipped: false, 0f, Color.White)
						{
							interval = 50f,
							totalNumberOfLoops = 99999,
							animationLength = 7,
							layerDepth = 1f,
							scale = 4f,
							alphaFade = 0.008f,
							motion = new Vector2(0f, -0.5f)
						});
					}
					aboveMapSprites = new List<TemporaryAnimatedSprite>();
					aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(325, 1977, 18, 18), new Vector2(160f, 800f), flipped: false, 0f, Color.White)
					{
						interval = 25f,
						totalNumberOfLoops = 99999,
						animationLength = 3,
						layerDepth = 1f,
						scale = 1f,
						scaleChange = 1f,
						scaleChangeChange = -0.05f,
						alpha = 0.65f,
						alphaFade = 0.005f,
						motion = new Vector2(-8f, -8f),
						acceleration = new Vector2(0.4f, 0.4f)
					});
					for (int n = 0; n < 16; n++)
					{
						aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(2f, 12f) * 64f + new Vector2(Game1.random.Next(-32, 64), 0f), flipped: false, 0.002f, Color.Gray)
						{
							alpha = 0.75f,
							motion = new Vector2(1f, -1f) + new Vector2((float)(Game1.random.Next(100) - 50) / 100f, (float)(Game1.random.Next(100) - 50) / 100f),
							interval = 99999f,
							layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
							scale = 3f,
							scaleChange = 0.01f,
							rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
							delayBeforeAnimationStart = n * 25
						});
					}
					location.setMapTileIndex(2, 12, 2143, "Front", 1);
					break;
				}
				case 'G':
					if (!(key == "parrotGone"))
					{
						if (key == "secretGift")
						{
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1231, 16, 16), new Vector2(30f, 70f) * 64f + new Vector2(0f, -21f), flipped: false, 0f, Color.White)
							{
								animationLength = 1,
								interval = 999999f,
								id = 666f,
								scale = 4f
							});
						}
					}
					else
					{
						location.removeTemporarySpritesWithID(666);
					}
					break;
				case 'h':
					if (key == "waterShane")
					{
						drawTool = true;
						farmer.TemporaryItem = new WateringCan();
						farmer.CurrentTool.Update(1, 0, farmer);
						farmer.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[4]
						{
							new FarmerSprite.AnimationFrame(58, 0, secondaryArm: false, flip: false),
							new FarmerSprite.AnimationFrame(58, 75, secondaryArm: false, flip: false, Farmer.showToolSwipeEffect),
							new FarmerSprite.AnimationFrame(59, 100, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true),
							new FarmerSprite.AnimationFrame(45, 500, secondaryArm: true, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true)
						});
					}
					break;
				case 'W':
					if (key == "wizardWarp")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(387, 1965, 16, 31), 9999f, 1, 999999, new Vector2(8f, 16f) * 64f + new Vector2(0f, 4f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(2f, -2f),
							acceleration = new Vector2(0.1f, 0f),
							scaleChange = -0.02f,
							alphaFade = 0.001f
						});
					}
					break;
				case 'l':
					if (key == "witchFlyby")
					{
						Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1886, 35, 29), 9999f, 1, 999999, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width, 192f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-4f, 0f),
							acceleration = new Vector2(-0.025f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = 2000f,
							yPeriodicRange = 64f,
							local = true
						});
					}
					break;
				case 'C':
					if (key == "junimoCage")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(325, 1977, 18, 19), 60f, 3, 999999, new Vector2(10f, 17f) * 64f + new Vector2(0f, -4f), flicker: false, flipped: false, 0f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							light = true,
							lightRadius = 1f,
							lightcolor = Color.Black,
							id = 1f,
							shakeIntensity = 0f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(379, 1991, 5, 5), 9999f, 1, 999999, new Vector2(10f, 17f) * 64f + new Vector2(0f, -4f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							light = true,
							lightRadius = 0.5f,
							lightcolor = Color.Black,
							id = 1f,
							xPeriodic = true,
							xPeriodicLoopTime = 2000f,
							xPeriodicRange = 24f,
							yPeriodic = true,
							yPeriodicLoopTime = 2000f,
							yPeriodicRange = 24f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(379, 1991, 5, 5), 9999f, 1, 999999, new Vector2(10f, 17f) * 64f + new Vector2(72f, -4f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							light = true,
							lightRadius = 0.5f,
							lightcolor = Color.Black,
							id = 1f,
							xPeriodic = true,
							xPeriodicLoopTime = 2000f,
							xPeriodicRange = -24f,
							yPeriodic = true,
							yPeriodicLoopTime = 2000f,
							yPeriodicRange = 24f,
							delayBeforeAnimationStart = 250
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(379, 1991, 5, 5), 9999f, 1, 999999, new Vector2(10f, 17f) * 64f + new Vector2(0f, 52f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							light = true,
							lightRadius = 0.5f,
							lightcolor = Color.Black,
							id = 1f,
							xPeriodic = true,
							xPeriodicLoopTime = 2000f,
							xPeriodicRange = -24f,
							yPeriodic = true,
							yPeriodicLoopTime = 2000f,
							yPeriodicRange = 24f,
							delayBeforeAnimationStart = 450
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(379, 1991, 5, 5), 9999f, 1, 999999, new Vector2(10f, 17f) * 64f + new Vector2(72f, 52f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							light = true,
							lightRadius = 0.5f,
							lightcolor = Color.Black,
							id = 1f,
							xPeriodic = true,
							xPeriodicLoopTime = 2000f,
							xPeriodicRange = 24f,
							yPeriodic = true,
							yPeriodicLoopTime = 2000f,
							yPeriodicRange = 24f,
							delayBeforeAnimationStart = 650
						});
					}
					break;
				case 'n':
					if (key == "joshDinner")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(649, 9999f, 1, 9999, new Vector2(6f, 4f) * 64f + new Vector2(8f, 32f), flicker: false, flipped: false)
						{
							layerDepth = 0.0256f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(664, 9999f, 1, 9999, new Vector2(8f, 4f) * 64f + new Vector2(-8f, 32f), flicker: false, flipped: false)
						{
							layerDepth = 0.0256f
						});
					}
					break;
				case 't':
					if (key == "beachStuff")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1887, 47, 29), 9999f, 1, 999, new Vector2(44f, 21f) * 64f, flicker: false, flipped: false, 1E-05f, 0f, Color.White, 4f, 0f, 0f, 0f));
					}
					break;
				case 'p':
					if (key == "leahLaptop")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(130, 1849, 19, 19), 9999f, 1, 999, new Vector2(12f, 10f) * 64f + new Vector2(0f, 24f), flicker: false, flipped: false, 0.1856f, 0f, Color.White, 4f, 0f, 0f, 0f));
					}
					break;
				case 'c':
					if (key == "leahPicnic")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(96, 1808, 32, 48), 9999f, 1, 999, new Vector2(75f, 37f) * 64f, flicker: false, flipped: false, 0.2496f, 0f, Color.White, 4f, 0f, 0f, 0f));
						NPC item = new NPC(new AnimatedSprite(festivalContent, "Characters\\" + (farmer.IsMale ? "LeahExMale" : "LeahExFemale"), 0, 16, 32), new Vector2(-100f, -100f) * 64f, 2, "LeahEx");
						actors.Add(item);
					}
					break;
				case 'a':
					if (key == "maruBeaker")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(738, 1380f, 1, 0, new Vector2(9f, 14f) * 64f + new Vector2(0f, 32f), flicker: false, flipped: false)
						{
							rotationChange = (float)Math.PI / 24f,
							motion = new Vector2(0f, -7f),
							acceleration = new Vector2(0f, 0.2f),
							endFunction = beakerSmashEndFunction,
							layerDepth = 1f
						});
					}
					break;
				case 'e':
					if (key == "abbyOneBat")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(640, 1664, 16, 16), 80f, 4, 9999, new Vector2(23f, 9f) * 64f, flicker: false, flipped: false, 1f, 0.003f, Color.White, 4f, 0f, 0f, 0f)
						{
							xPeriodic = true,
							xPeriodicLoopTime = 2000f,
							xPeriodicRange = 128f,
							motion = new Vector2(0f, -8f)
						});
					}
					break;
				case 'w':
					if (key == "swordswipe")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 960, 128, 128), 60f, 4, 0, new Vector2(Convert.ToInt32(split[2]), Convert.ToInt32(split[3])) * 64f + new Vector2(0f, -32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f));
					}
					break;
				case 'L':
					if (key == "abbyAtLake")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(735, 999999f, 1, 0, new Vector2(48f, 30f) * 64f, flicker: false, flipped: false)
						{
							light = true,
							lightRadius = 2f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(48f, 30f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
						{
							light = true,
							lightRadius = 0.2f,
							xPeriodic = true,
							yPeriodic = true,
							xPeriodicLoopTime = 2000f,
							yPeriodicLoopTime = 1600f,
							xPeriodicRange = 32f,
							yPeriodicRange = 21f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(48f, 30f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
						{
							light = true,
							lightRadius = 0.2f,
							xPeriodic = true,
							yPeriodic = true,
							xPeriodicLoopTime = 1000f,
							yPeriodicLoopTime = 1600f,
							xPeriodicRange = 16f,
							yPeriodicRange = 21f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(48f, 30f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
						{
							light = true,
							lightRadius = 0.2f,
							xPeriodic = true,
							yPeriodic = true,
							xPeriodicLoopTime = 2400f,
							yPeriodicLoopTime = 2800f,
							xPeriodicRange = 21f,
							yPeriodicRange = 32f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(48f, 30f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
						{
							light = true,
							lightRadius = 0.2f,
							xPeriodic = true,
							yPeriodic = true,
							xPeriodicLoopTime = 2000f,
							yPeriodicLoopTime = 2400f,
							xPeriodicRange = 16f,
							yPeriodicRange = 16f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(66f, 34f) * 64f + new Vector2(-32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
						{
							lightcolor = Color.Orange,
							light = true,
							lightRadius = 0.2f,
							xPeriodic = true,
							yPeriodic = true,
							xPeriodicLoopTime = 2000f,
							yPeriodicLoopTime = 2600f,
							xPeriodicRange = 21f,
							yPeriodicRange = 48f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(66f, 34f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
						{
							lightcolor = Color.Orange,
							light = true,
							lightRadius = 0.2f,
							xPeriodic = true,
							yPeriodic = true,
							xPeriodicLoopTime = 2000f,
							yPeriodicLoopTime = 2600f,
							xPeriodicRange = 32f,
							yPeriodicRange = 21f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(66f, 34f) * 64f + new Vector2(32f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
						{
							lightcolor = Color.Orange,
							light = true,
							lightRadius = 0.2f,
							xPeriodic = true,
							yPeriodic = true,
							xPeriodicLoopTime = 4000f,
							yPeriodicLoopTime = 5000f,
							xPeriodicRange = 42f,
							yPeriodicRange = 32f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(66f, 34f) * 64f + new Vector2(0f, -32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
						{
							lightcolor = Color.Orange,
							light = true,
							lightRadius = 0.2f,
							xPeriodic = true,
							yPeriodic = true,
							xPeriodicLoopTime = 4000f,
							yPeriodicLoopTime = 5500f,
							xPeriodicRange = 32f,
							yPeriodicRange = 32f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(69f, 28f) * 64f + new Vector2(-32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
						{
							lightcolor = Color.Orange,
							light = true,
							lightRadius = 0.2f,
							xPeriodic = true,
							yPeriodic = true,
							xPeriodicLoopTime = 2400f,
							yPeriodicLoopTime = 3600f,
							xPeriodicRange = 32f,
							yPeriodicRange = 21f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(69f, 28f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
						{
							lightcolor = Color.Orange,
							light = true,
							lightRadius = 0.2f,
							xPeriodic = true,
							yPeriodic = true,
							xPeriodicLoopTime = 2500f,
							yPeriodicLoopTime = 3600f,
							xPeriodicRange = 42f,
							yPeriodicRange = 51f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(69f, 28f) * 64f + new Vector2(32f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
						{
							lightcolor = Color.Orange,
							light = true,
							lightRadius = 0.2f,
							xPeriodic = true,
							yPeriodic = true,
							xPeriodicLoopTime = 4500f,
							yPeriodicLoopTime = 3000f,
							xPeriodicRange = 21f,
							yPeriodicRange = 32f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(69f, 28f) * 64f + new Vector2(0f, -32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
						{
							lightcolor = Color.Orange,
							light = true,
							lightRadius = 0.2f,
							xPeriodic = true,
							yPeriodic = true,
							xPeriodicLoopTime = 5000f,
							yPeriodicLoopTime = 4500f,
							xPeriodicRange = 64f,
							yPeriodicRange = 48f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(72f, 33f) * 64f + new Vector2(-32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
						{
							lightcolor = Color.Orange,
							light = true,
							lightRadius = 0.2f,
							xPeriodic = true,
							yPeriodic = true,
							xPeriodicLoopTime = 2000f,
							yPeriodicLoopTime = 3000f,
							xPeriodicRange = 32f,
							yPeriodicRange = 21f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(72f, 33f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
						{
							lightcolor = Color.Orange,
							light = true,
							lightRadius = 0.2f,
							xPeriodic = true,
							yPeriodic = true,
							xPeriodicLoopTime = 2900f,
							yPeriodicLoopTime = 3200f,
							xPeriodicRange = 21f,
							yPeriodicRange = 32f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(72f, 33f) * 64f + new Vector2(32f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
						{
							lightcolor = Color.Orange,
							light = true,
							lightRadius = 0.2f,
							xPeriodic = true,
							yPeriodic = true,
							xPeriodicLoopTime = 4200f,
							yPeriodicLoopTime = 3300f,
							xPeriodicRange = 16f,
							yPeriodicRange = 32f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(72f, 33f) * 64f + new Vector2(0f, -32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
						{
							lightcolor = Color.Orange,
							light = true,
							lightRadius = 0.2f,
							xPeriodic = true,
							yPeriodic = true,
							xPeriodicLoopTime = 5100f,
							yPeriodicLoopTime = 4000f,
							xPeriodicRange = 32f,
							yPeriodicRange = 16f
						});
					}
					break;
				}
				break;
			case 14:
				switch (key[6])
				{
				case 'L':
					if (key == "georgeLeekGift")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1231, 16, 16), 100f, 6, 1, new Vector2(17f, 19f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 999f,
							paused = false,
							holdLastFrame = true
						});
					}
					break;
				case 'P':
					if (key == "parrotPerchHut")
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Microsoft.Xna.Framework.Rectangle(0, 0, 24, 24), new Vector2(7f, 4f) * 64f, flipped: false, 0f, Color.White)
						{
							animationLength = 1,
							interval = 999999f,
							scale = 4f,
							layerDepth = 1f,
							id = 999f
						});
					}
					break;
				case 'e':
					if (key == "trashBearMagic")
					{
						Utility.addStarsAndSpirals(location, 95, 103, 24, 12, 2000, 10, Color.Lime);
						(location as Forest).removeSewerTrash();
						Game1.flashAlpha = 0.75f;
						Game1.screenGlowOnce(Color.Lime, hold: false, 0.25f, 1f);
					}
					break;
				case 'l':
					if (key == "gridballGameTV")
					{
						Texture2D texture2 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture2,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 336, 19, 14),
							animationLength = 7,
							sourceRectStartingPos = new Vector2(368f, 336f),
							interval = 5000f,
							totalNumberOfLoops = 99999,
							position = new Vector2(34f, 3f) * 64f + new Vector2(7f, 13f) * 4f,
							scale = 4f,
							layerDepth = 1f
						});
					}
					break;
				case 'h':
					if (key == "waterShaneDone")
					{
						farmer.completelyStopAnimatingOrDoingAction();
						farmer.TemporaryItem = null;
						drawTool = false;
						location.removeTemporarySpritesWithID(999);
					}
					break;
				case 'a':
					if (key == "shanePassedOut")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(533, 1864, 19, 27), 99999f, 1, 99999, new Vector2(25f, 7f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 999f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(552, 1862, 31, 21), 99999f, 1, 99999, new Vector2(25f, 7f) * 64f + new Vector2(-16f, 0f), flicker: false, flipped: false, 0.0001f, 0f, Color.White, 4f, 0f, 0f, 0f));
					}
					break;
				case 'C':
					if (key == "junimoCageGone")
					{
						location.removeTemporarySpritesWithID(1);
					}
					break;
				case 'G':
					if (key == "secretGiftOpen")
					{
						TemporaryAnimatedSprite temporarySpriteByID2 = location.getTemporarySpriteByID(666);
						if (temporarySpriteByID2 != null)
						{
							temporarySpriteByID2.animationLength = 6;
							temporarySpriteByID2.interval = 100f;
							temporarySpriteByID2.totalNumberOfLoops = 1;
							temporarySpriteByID2.timer = 0f;
							temporarySpriteByID2.holdLastFrame = true;
						}
					}
					break;
				case 'B':
					if (key == "candleBoatMove")
					{
						location.getTemporarySpriteByID(1).motion = new Vector2(0f, 2f);
					}
					break;
				case 'i':
					if (key == "pennyFieldTrip")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1813, 86, 54), 999999f, 1, 0, new Vector2(68f, 44f) * 64f, flicker: false, flipped: false, 0.0001f, 0f, Color.White, 4f, 0f, 0f, 0f));
					}
					break;
				}
				break;
			case 12:
				switch (key[4])
				{
				case 'i':
					if (!(key == "staticSprite"))
					{
						if (key == "morrisFlying")
						{
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(105, 1318, 13, 31), 9999f, 1, 99999, new Vector2(32f, 13f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								motion = new Vector2(4f, -8f),
								rotationChange = (float)Math.PI / 16f,
								shakeIntensity = 1f
							});
						}
					}
					else
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite(split[2], new Microsoft.Xna.Framework.Rectangle(Convert.ToInt32(split[3]), Convert.ToInt32(split[4]), Convert.ToInt32(split[5]), Convert.ToInt32(split[6])), new Vector2((float)Convert.ToDouble(split[7]), (float)Convert.ToDouble(split[8])) * 64f, flipped: false, 0f, Color.White)
						{
							animationLength = 1,
							interval = 999999f,
							scale = 4f,
							layerDepth = ((split.Length > 10) ? ((float)Convert.ToDouble(split[10])) : 1f),
							id = ((split.Length > 9) ? Convert.ToInt32(split[9]) : 999)
						});
					}
					break;
				case 'v':
					if (key == "removeSprite" && split != null && split.Count() > 2)
					{
						location.removeTemporarySpritesWithID(Convert.ToInt32(split[2]));
					}
					break;
				case 'y':
					if (!(key == "EmilyCamping"))
					{
						if (key == "EmilyBoomBox")
						{
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(586, 1871, 24, 14), 99999f, 1, 99999, new Vector2(15f, 4f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								id = 999f
							});
						}
						break;
					}
					showGroundObjects = false;
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(644, 1578, 59, 53), 999999f, 1, 99999, new Vector2(26f, 9f) * 64f + new Vector2(-16f, 0f), flicker: false, flipped: false, 0.0788f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 999f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(675, 1299, 29, 24), 999999f, 1, 99999, new Vector2(27f, 14f) * 64f, flicker: false, flipped: false, 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 99f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(27f, 14f) * 64f + new Vector2(8f, 4f) * 4f, flipped: false, 0f, Color.White)
					{
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 4,
						light = true,
						lightID = 666,
						id = 666f,
						lightRadius = 2f,
						scale = 4f,
						layerDepth = 0.01f
					});
					Game1.currentLightSources.Add(new LightSource(4, new Vector2(27f, 14f) * 64f, 2f, LightSource.LightContext.None, 0L));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(585, 1846, 26, 22), 999999f, 1, 99999, new Vector2(25f, 12f) * 64f + new Vector2(-32f, 0f), flicker: false, flipped: false, 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 96f
					});
					AmbientLocationSounds.addSound(new Vector2(27f, 14f), 1);
					break;
				case 'a':
					if (key == "curtainClose")
					{
						location.getTemporarySpriteByID(999).sourceRect.X = 644;
						Game1.playSound("shwip");
					}
					break;
				case 'd':
					if (key == "grandpaNight")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1453, 639, 176), 9999f, 1, 999999, new Vector2(0f, 1f) * 64f, flicker: false, flipped: false, 0.9f, 0f, Color.Cyan, 4f, 0f, 0f, 0f, local: true)
						{
							alpha = 0.01f,
							alphaFade = -0.002f,
							local = true
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1453, 639, 176), 9999f, 1, 999999, new Vector2(0f, 768f), flicker: false, flipped: true, 0.9f, 0f, Color.Blue, 4f, 0f, 0f, 0f, local: true)
						{
							alpha = 0.01f,
							alphaFade = -0.002f,
							local = true
						});
					}
					break;
				case 'C':
					if (key == "jojaCeremony")
					{
						aboveMapSprites = new List<TemporaryAnimatedSprite>();
						for (int l = 0; l < 16; l++)
						{
							Vector2 vector = new Vector2(Game1.random.Next(Game1.viewport.Width - 128), Game1.viewport.Height + l * 64);
							aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(534, 1413, 11, 16), 99999f, 1, 99999, vector, flicker: false, flipped: false, 0.99f, 0f, Color.DeepSkyBlue, 4f, 0f, 0f, 0f)
							{
								local = true,
								motion = new Vector2(0.25f, -1.5f),
								acceleration = new Vector2(0f, -0.001f)
							});
							aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(545, 1413, 11, 34), 99999f, 1, 99999, vector + new Vector2(0f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								local = true,
								motion = new Vector2(0.25f, -1.5f),
								acceleration = new Vector2(0f, -0.001f)
							});
						}
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1363, 114, 58), 99999f, 1, 99999, new Vector2(50f, 20f) * 64f, flicker: false, flipped: false, 0.1472f, 0f, Color.White, 4f, 0f, 0f, 0f));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(595, 1387, 14, 34), 200f, 3, 99999, new Vector2(48f, 20f) * 64f, flicker: false, flipped: false, 0.15720001f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							pingPong = true
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(595, 1387, 14, 34), 200f, 3, 99999, new Vector2(49f, 20f) * 64f, flicker: false, flipped: false, 0.15720001f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							pingPong = true
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(595, 1387, 14, 34), 210f, 3, 99999, new Vector2(62f, 20f) * 64f, flicker: false, flipped: false, 0.15720001f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							pingPong = true
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(595, 1387, 14, 34), 190f, 3, 99999, new Vector2(60f, 20f) * 64f, flicker: false, flipped: false, 0.15720001f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							pingPong = true
						});
					}
					break;
				case 'F':
					if (key == "joshFootball")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(405, 1916, 14, 8), 40f, 6, 9999, new Vector2(25f, 16f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							rotation = -(float)Math.PI / 4f,
							rotationChange = (float)Math.PI / 200f,
							motion = new Vector2(6f, -4f),
							acceleration = new Vector2(0f, 0.2f),
							xStopCoordinate = 1856,
							reachedStopCoordinate = catchFootball,
							layerDepth = 1f,
							id = 56232f
						});
					}
					break;
				case 'o':
					if (key == "balloonBirds")
					{
						int num2 = 0;
						if (split != null && split.Length > 2)
						{
							num2 = Convert.ToInt32(split[2]);
						}
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(48f, num2 + 12) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = 1500
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(47f, num2 + 13) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = 1250
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, num2 + 14) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = 1100
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(45f, num2 + 15) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = 1000
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, num2 + 16) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = 1080
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(47f, num2 + 17) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = 1300
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(48f, num2 + 18) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = 1450
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, num2 + 15) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-4f, 0f),
							delayBeforeAnimationStart = 5450
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(48f, num2 + 10) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
						{
							motion = new Vector2(-2f, 0f),
							delayBeforeAnimationStart = 500
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(47f, num2 + 11) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
						{
							motion = new Vector2(-2f, 0f),
							delayBeforeAnimationStart = 250
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, num2 + 12) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
						{
							motion = new Vector2(-2f, 0f),
							delayBeforeAnimationStart = 100
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(45f, num2 + 13) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
						{
							motion = new Vector2(-2f, 0f)
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, num2 + 14) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
						{
							motion = new Vector2(-2f, 0f),
							delayBeforeAnimationStart = 80
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(47f, num2 + 15) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
						{
							motion = new Vector2(-2f, 0f),
							delayBeforeAnimationStart = 300
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(48f, num2 + 16) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
						{
							motion = new Vector2(-2f, 0f),
							delayBeforeAnimationStart = 450
						});
					}
					break;
				case 'e':
					if (key == "marcelloLand")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1183, 84, 160), 10000f, 1, 99999, (new Vector2(25f, 19f) + eventPositionTileOffset) * 64f + new Vector2(-23f, 0f) * 4f, flicker: false, flipped: false, 2E-05f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, 2f),
							yStopCoordinate = (41 + (int)eventPositionTileOffset.Y) * 64 - 640,
							reachedStopCoordinate = marcelloBalloonLand,
							attachedCharacter = getActorByName("Marcello"),
							id = 1f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(84, 1205, 38, 26), 10000f, 1, 99999, (new Vector2(25f, 19f) + eventPositionTileOffset) * 64f + new Vector2(0f, 134f) * 4f, flicker: false, flipped: false, (41f + eventPositionTileOffset.Y) * 64f / 10000f + 0.0001f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, 2f),
							id = 2f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(24, 1343, 36, 19), 7000f, 1, 99999, (new Vector2(25f, 40f) + eventPositionTileOffset) * 64f, flicker: false, flipped: false, 1E-05f, 0f, Color.White, 0f, 0f, 0f, 0f)
						{
							scaleChange = 0.01f,
							id = 3f
						});
					}
					break;
				case 'T':
					if (key == "maruTrapdoor")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(640, 1632, 16, 32), 150f, 4, 0, new Vector2(1f, 5f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(688, 1632, 16, 32), 99999f, 1, 0, new Vector2(1f, 5f) * 64f, flicker: false, flipped: false, 0.99f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							delayBeforeAnimationStart = 500
						});
					}
					break;
				case 'M':
					if (key == "abbyManyBats")
					{
						for (int j = 0; j < 100; j++)
						{
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(640, 1664, 16, 16), 80f, 4, 9999, new Vector2(23f, 9f) * 64f, flicker: false, flipped: false, 1f, 0.003f, Color.White, 4f, 0f, 0f, 0f)
							{
								xPeriodic = true,
								xPeriodicLoopTime = Game1.random.Next(1500, 2500),
								xPeriodicRange = Game1.random.Next(64, 192),
								motion = new Vector2(Game1.random.Next(-2, 3), Game1.random.Next(-8, -4)),
								delayBeforeAnimationStart = j * 30,
								startSound = ((j % 10 == 0 || Game1.random.NextDouble() < 0.1) ? "batScreech" : null)
							});
						}
						for (int k = 0; k < 100; k++)
						{
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(640, 1664, 16, 16), 80f, 4, 9999, new Vector2(23f, 9f) * 64f, flicker: false, flipped: false, 1f, 0.003f, Color.White, 4f, 0f, 0f, 0f)
							{
								motion = new Vector2(Game1.random.Next(-4, 5), Game1.random.Next(-8, -4)),
								delayBeforeAnimationStart = 10 + k * 30
							});
						}
					}
					break;
				}
				break;
			case 8:
				switch (key[4])
				{
				case 'y':
					if (key == "WillyWad")
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Cursors2"),
							sourceRect = new Microsoft.Xna.Framework.Rectangle(192, 61, 32, 32),
							sourceRectStartingPos = new Vector2(192f, 61f),
							animationLength = 2,
							totalNumberOfLoops = 99999,
							interval = 400f,
							scale = 4f,
							position = new Vector2(50f, 23f) * 64f,
							layerDepth = 0.1536f,
							id = 996f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(51, new Vector2(3328f, 1728f), Color.White, 10, flipped: false, 80f, 999999));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(51, new Vector2(3264f, 1792f), Color.White, 10, flipped: false, 70f, 999999));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(51, new Vector2(3392f, 1792f), Color.White, 10, flipped: false, 85f, 999999));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 500f, 3, 99999, new Vector2(53f, 24f) * 64f, flicker: false, flipped: false, 0.1984f, 0f, Color.White, 4f, 0f, 0f, 0f));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 510f, 3, 99999, new Vector2(54f, 23f) * 64f, flicker: false, flipped: false, 0.1984f, 0f, Color.White, 4f, 0f, 0f, 0f));
					}
					break;
				case 'J':
					if (key == "frogJump")
					{
						TemporaryAnimatedSprite temporarySpriteByID = location.getTemporarySpriteByID(777);
						temporarySpriteByID.motion = new Vector2(-2f, 0f);
						temporarySpriteByID.animationLength = 4;
						temporarySpriteByID.interval = 150f;
					}
					break;
				case 'm':
					if (key == "golemDie")
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite(46, new Vector2(40f, 11f) * 64f, Color.DarkGray, 10));
						Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, new Vector2(40f, 11f) * 64f, Color.LimeGreen, 10), location, 2);
						Texture2D texture = Game1.temporaryContent.Load<Texture2D>("Characters\\Monsters\\Wilderness Golem");
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 24),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(0f, 0f),
							interval = 5000f,
							totalNumberOfLoops = 9999,
							position = new Vector2(40f, 11f) * 64f + new Vector2(2f, -8f) * 4f,
							scale = 4f,
							layerDepth = 0.01f,
							rotation = (float)Math.PI / 2f,
							motion = new Vector2(0f, 4f),
							yStopCoordinate = 832
						});
					}
					break;
				case 'o':
					if (key == "parrots1")
					{
						aboveMapSprites = new List<TemporaryAnimatedSprite>();
						aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 165, 24, 22), 100f, 6, 9999, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width, 256f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-3f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = 2000f,
							yPeriodicRange = 32f,
							delayBeforeAnimationStart = 0,
							local = true
						});
						aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 165, 24, 22), 100f, 6, 9999, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width, 192f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-3f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = 2000f,
							yPeriodicRange = 32f,
							delayBeforeAnimationStart = 600,
							local = true
						});
						aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 165, 24, 22), 100f, 6, 9999, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width, 320f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-3f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = 2000f,
							yPeriodicRange = 32f,
							delayBeforeAnimationStart = 1200,
							local = true
						});
					}
					break;
				case 'e':
					if (key == "umbrella")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1843, 27, 23), 80f, 3, 9999, new Vector2(12f, 39f) * 64f + new Vector2(-20f, -104f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
					}
					break;
				case 'S':
					if (key == "leahShow")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(144, 688, 16, 32), 9999f, 1, 999, new Vector2(29f, 59f) * 64f - new Vector2(0f, 16f), flicker: false, flipped: false, 0.37750003f, 0f, Color.White, 4f, 0f, 0f, 0f));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(112, 656, 16, 64), 9999f, 1, 999, new Vector2(29f, 56f) * 64f, flicker: false, flipped: false, 0.3776f, 0f, Color.White, 4f, 0f, 0f, 0f));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(144, 688, 16, 32), 9999f, 1, 999, new Vector2(33f, 59f) * 64f - new Vector2(0f, 16f), flicker: false, flipped: false, 0.37750003f, 0f, Color.White, 4f, 0f, 0f, 0f));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(128, 688, 16, 32), 9999f, 1, 999, new Vector2(33f, 58f) * 64f, flicker: false, flipped: false, 0.3776f, 0f, Color.White, 4f, 0f, 0f, 0f));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(160, 656, 32, 64), 9999f, 1, 999, new Vector2(29f, 60f) * 64f, flicker: false, flipped: false, 0.4032f, 0f, Color.White, 4f, 0f, 0f, 0f));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(144, 688, 16, 32), 9999f, 1, 999, new Vector2(34f, 63f) * 64f, flicker: false, flipped: false, 0.4031f, 0f, Color.White, 4f, 0f, 0f, 0f));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(113, 592, 16, 64), 100f, 4, 99999, new Vector2(34f, 60f) * 64f, flicker: false, flipped: false, 0.4032f, 0f, Color.White, 4f, 0f, 0f, 0f));
						NPC item = new NPC(new AnimatedSprite(festivalContent, "Characters\\" + (farmer.IsMale ? "LeahExMale" : "LeahExFemale"), 0, 16, 32), new Vector2(46f, 57f) * 64f, 2, "LeahEx");
						actors.Add(item);
					}
					break;
				case 'T':
					if (key == "leahTree")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(744, 999999f, 1, 0, new Vector2(42f, 8f) * 64f, flicker: false, flipped: false));
					}
					break;
				}
				break;
			case 13:
				switch (key[9])
				{
				case 'T':
					if (!(key == "trashBearTown"))
					{
						if (key == "stopShakeTent")
						{
							location.getTemporarySpriteByID(999).shakeIntensity = 0f;
						}
						break;
					}
					aboveMapSprites = new List<TemporaryAnimatedSprite>();
					aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(46, 80, 46, 56), new Vector2(43f, 64f) * 64f, flipped: false, 0f, Color.White)
					{
						animationLength = 1,
						interval = 999999f,
						motion = new Vector2(4f, 0f),
						scale = 4f,
						layerDepth = 1f,
						yPeriodic = true,
						yPeriodicLoopTime = 2000f,
						yPeriodicRange = 32f,
						id = 777f,
						xStopCoordinate = 3392,
						reachedStopCoordinate = delegate
						{
							aboveMapSprites.First().xStopCoordinate = -1;
							aboveMapSprites.First().motion = new Vector2(4f, 0f);
							location.ApplyMapOverride("Town-TrashGone", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(57, 68, 17, 5));
							location.ApplyMapOverride("Town-DogHouse", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(51, 65, 5, 6));
							Game1.flashAlpha = 0.75f;
							Game1.screenGlowOnce(Color.Lime, hold: false, 0.25f, 1f);
							location.playSound("yoba");
							TemporaryAnimatedSprite temporaryAnimatedSprite8 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(497, 1918, 11, 11), new Vector2(3456f, 4160f), flipped: false, 0f, Color.White)
							{
								yStopCoordinate = 4372,
								motion = new Vector2(-0.5f, -10f),
								acceleration = new Vector2(0f, 0.25f),
								scale = 4f,
								alphaFade = 0f,
								extraInfoForEndBehavior = -777
							};
							temporaryAnimatedSprite8.reachedStopCoordinate = temporaryAnimatedSprite8.bounce;
							temporaryAnimatedSprite8.initialPosition.Y = 4372f;
							aboveMapSprites.Add(temporaryAnimatedSprite8);
							aboveMapSprites.AddRange(Utility.getStarsAndSpirals(location, 54, 69, 6, 5, 1000, 10, Color.Lime));
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1936, 12, 20), 80f, 4, 99999, new Vector2(53f, 67f) * 64f + new Vector2(3f, 3f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								id = 1f,
								delayBeforeAnimationStart = 3000,
								startSound = "dogWhining"
							});
						}
					});
					break;
				case 'S':
					if (key == "shakeBushStop")
					{
						location.getTemporarySpriteByID(777).shakeIntensity = 0f;
					}
					break;
				case 'F':
					if (key == "sebastianFrog")
					{
						Texture2D texture15 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture15,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 224, 16, 16),
							animationLength = 4,
							sourceRectStartingPos = new Vector2(0f, 224f),
							interval = 120f,
							totalNumberOfLoops = 9999,
							position = new Vector2(45f, 36f) * 64f,
							scale = 4f,
							layerDepth = 0.00064f,
							motion = new Vector2(2f, 0f),
							xStopCoordinate = 3136,
							id = 777f,
							reachedStopCoordinate = delegate
							{
								int num48 = CurrentCommand;
								CurrentCommand = num48 + 1;
								location.removeTemporarySpritesWithID(777);
							}
						});
					}
					break;
				case 'W':
					if (key == "haleyCakeWalk")
					{
						Texture2D texture16 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture16,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 400, 144, 112),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(0f, 400f),
							interval = 5000f,
							totalNumberOfLoops = 9999,
							position = new Vector2(26f, 65f) * 64f,
							scale = 4f,
							layerDepth = 0.00064f
						});
					}
					break;
				case 'a':
					if (key == "pamYobaStatue")
					{
						location.objects.Remove(new Vector2(26f, 9f));
						location.objects.Add(new Vector2(26f, 9f), new Object(Vector2.Zero, 34));
						Game1.getLocationFromName("Trailer_Big").objects.Remove(new Vector2(26f, 9f));
						Game1.getLocationFromName("Trailer_Big").objects.Add(new Vector2(26f, 9f), new Object(Vector2.Zero, 34));
					}
					break;
				case 'p':
					if (key == "EmilySleeping")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(574, 1892, 11, 11), 1000f, 2, 99999, new Vector2(20f, 3f) * 64f + new Vector2(8f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 999f
						});
					}
					break;
				case 'i':
					if (!(key == "shaneHospital"))
					{
						if (key == "grandpaSpirit")
						{
							TemporaryAnimatedSprite temporaryAnimatedSprite4 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(555, 1956, 18, 35), 9999f, 1, 99999, new Vector2(-1000f, -1010f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f);
							temporaryAnimatedSprite4.yStopCoordinate = -64128;
							temporaryAnimatedSprite4.xPeriodic = true;
							temporaryAnimatedSprite4.xPeriodicLoopTime = 3000f;
							temporaryAnimatedSprite4.xPeriodicRange = 16f;
							temporaryAnimatedSprite4.motion = new Vector2(0f, 1f);
							temporaryAnimatedSprite4.overrideLocationDestroy = true;
							temporaryAnimatedSprite4.id = 77777f;
							TemporaryAnimatedSprite temporaryAnimatedSprite7 = temporaryAnimatedSprite4;
							location.temporarySprites.Add(temporaryAnimatedSprite7);
							for (int num46 = 0; num46 < 19; num46++)
							{
								location.temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(32f, 32f), Color.White)
								{
									parentSprite = temporaryAnimatedSprite7,
									delayBeforeAnimationStart = (num46 + 1) * 500,
									overrideLocationDestroy = true,
									scale = 1f,
									alpha = 1f
								});
							}
						}
					}
					else
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(533, 1864, 19, 10), 99999f, 1, 99999, new Vector2(20f, 3f) * 64f + new Vector2(16f, 12f), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 999f
						});
					}
					break;
				case 'w':
					if (key == "shaneThrowCan")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(542, 1893, 4, 6), 99999f, 1, 99999, new Vector2(103f, 95f) * 64f + new Vector2(0f, 4f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -4f),
							acceleration = new Vector2(0f, 0.25f),
							rotationChange = (float)Math.PI / 128f
						});
						Game1.playSound("shwip");
					}
					break;
				case 'm':
					if (key == "WizardPromise")
					{
						Utility.addSprinklesToLocation(location, 16, 15, 9, 9, 2000, 50, Color.White);
					}
					break;
				case 'f':
					if (key == "linusCampfire")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), 50f, 4, 99999, new Vector2(29f, 9f) * 64f + new Vector2(8f, 0f), flicker: false, flipped: false, 0.0576f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							light = true,
							lightRadius = 3f,
							lightcolor = Color.Black
						});
					}
					break;
				case 't':
					if (key == "ccCelebration")
					{
						aboveMapSprites = new List<TemporaryAnimatedSprite>();
						for (int num47 = 0; num47 < 32; num47++)
						{
							Vector2 vector4 = new Vector2(Game1.random.Next(Game1.viewport.Width - 128), Game1.viewport.Height + num47 * 64);
							aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(534, 1413, 11, 16), 99999f, 1, 99999, vector4, flicker: false, flipped: false, 1f, 0f, Utility.getRandomRainbowColor(), 4f, 0f, 0f, 0f)
							{
								local = true,
								motion = new Vector2(0.25f, -1.5f),
								acceleration = new Vector2(0f, -0.001f)
							});
							aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(545, 1413, 11, 34), 99999f, 1, 99999, vector4 + new Vector2(0f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								local = true,
								motion = new Vector2(0.25f, -1.5f),
								acceleration = new Vector2(0f, -0.001f)
							});
						}
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(558, 1425, 20, 26), 400f, 3, 99999, new Vector2(53f, 21f) * 64f, flicker: false, flipped: false, 0.5f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							pingPong = true
						});
					}
					break;
				case 'g':
					if (key == "alexDiningDog")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1936, 12, 20), 80f, 4, 99999, new Vector2(7f, 2f) * 64f + new Vector2(2f, -8f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 1f
						});
					}
					break;
				case 'd':
					if (key == "skateboardFly")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1875, 16, 6), 9999f, 1, 999, new Vector2(26f, 90f) * 64f, flicker: false, flipped: false, 1E-05f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							rotationChange = (float)Math.PI / 24f,
							motion = new Vector2(-8f, -10f),
							acceleration = new Vector2(0.02f, 0.3f),
							yStopCoordinate = 5824,
							xStopCoordinate = 1024,
							layerDepth = 1f
						});
					}
					break;
				case 'R':
					if (key == "sebastianRide")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(405, 1843, 14, 9), 40f, 4, 999, new Vector2(19f, 8f) * 64f + new Vector2(0f, 28f), flicker: false, flipped: false, 0.1792f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-2f, 0f)
						});
					}
					break;
				case 'D':
					if (key == "haleyRoomDark")
					{
						Game1.currentLightSources.Clear();
						Game1.ambientLight = new Color(200, 200, 100);
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(743, 999999f, 1, 0, new Vector2(4f, 1f) * 64f, flicker: false, flipped: false)
						{
							light = true,
							lightcolor = new Color(0, 255, 255),
							lightRadius = 2f
						});
					}
					break;
				case 'c':
					if (key == "maruTelescope")
					{
						for (int num45 = 0; num45 < 9; num45++)
						{
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(256, 1680, 16, 16), 80f, 5, 0, new Vector2(Game1.random.Next(1, 28), Game1.random.Next(1, 20)) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								delayBeforeAnimationStart = 8000 + num45 * Game1.random.Next(2000),
								motion = new Vector2(4f, 4f)
							});
						}
					}
					break;
				case 'y':
					if (key == "abbyGraveyard")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(736, 999999f, 1, 0, new Vector2(48f, 86f) * 64f, flicker: false, flipped: false));
					}
					break;
				}
				break;
			case 18:
				switch (key[6])
				{
				case 'e':
					if (key == "trashBearUmbrella1")
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(0, 80, 46, 56), new Vector2(102f, 94.5f) * 64f, flipped: false, 0f, Color.White)
						{
							animationLength = 1,
							interval = 999999f,
							motion = new Vector2(0f, -9f),
							acceleration = new Vector2(0f, 0.4f),
							scale = 4f,
							layerDepth = 1f,
							id = 777f,
							yStopCoordinate = 6144,
							reachedStopCoordinate = delegate(int param)
							{
								location.getTemporarySpriteByID(777).yStopCoordinate = -1;
								location.getTemporarySpriteByID(777).motion = new Vector2(0f, (float)param * 0.75f);
								location.getTemporarySpriteByID(777).acceleration = new Vector2(0.04f, -0.19f);
								location.getTemporarySpriteByID(777).accelerationChange = new Vector2(0f, 0.0015f);
								location.getTemporarySpriteByID(777).sourceRect.X += 46;
								location.playSound("batFlap");
								location.playSound("tinyWhip");
							}
						});
					}
					break;
				case 'h':
					if (key == "movieTheater_setup")
					{
						Game1.currentLightSources.Add(new LightSource(7, new Vector2(192f, 64f) + new Vector2(64f, 80f) * 4f, 4f, LightSource.LightContext.None, 0L));
						location.temporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.temporaryContent.Load<Texture2D>("Maps\\MovieTheaterScreen_TileSheet"),
							sourceRect = new Microsoft.Xna.Framework.Rectangle(224, 0, 96, 96),
							sourceRectStartingPos = new Vector2(224f, 0f),
							animationLength = 1,
							interval = 5000f,
							totalNumberOfLoops = 9999,
							scale = 4f,
							position = new Vector2(4f, 5f) * 64f,
							layerDepth = 1f,
							id = 999f,
							delayBeforeAnimationStart = 7950
						});
					}
					break;
				case 'g':
					if (key == "missingJunimoStars")
					{
						location.removeTemporarySpritesWithID(999);
						Texture2D texture11 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
						for (int num44 = 0; num44 < 48; num44++)
						{
							location.TemporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = texture11,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(477, 306, 28, 28),
								sourceRectStartingPos = new Vector2(477f, 306f),
								animationLength = 1,
								interval = 5000f,
								totalNumberOfLoops = 10,
								scale = Game1.random.Next(1, 5),
								position = Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 84, 84) + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)),
								rotationChange = (float)Math.PI / (float)Game1.random.Next(16, 128),
								motion = new Vector2((float)Game1.random.Next(-30, 40) / 10f, (float)Game1.random.Next(20, 90) * -0.1f),
								acceleration = new Vector2(0f, 0.05f),
								local = true,
								layerDepth = (float)num44 / 100f,
								color = ((Game1.random.NextDouble() < 0.5) ? Color.White : Utility.getRandomRainbowColor())
							});
						}
					}
					break;
				case 'i':
					if (key == "sebastianFrogHouse")
					{
						Point spouseRoomCorner = (location as FarmHouse).GetSpouseRoomCorner();
						spouseRoomCorner.X++;
						spouseRoomCorner.Y += 6;
						Vector2 vector3 = Utility.PointToVector2(spouseRoomCorner);
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.mouseCursors,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(641, 1534, 48, 37),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(641f, 1534f),
							interval = 5000f,
							totalNumberOfLoops = 9999,
							position = vector3 * 64f + new Vector2(0f, -5f) * 4f,
							scale = 4f,
							layerDepth = (vector3.Y + 2f + 0.1f) * 64f / 10000f
						});
						Texture2D texture13 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture13,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 224, 16, 16),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(0f, 224f),
							interval = 5000f,
							totalNumberOfLoops = 9999,
							position = vector3 * 64f + new Vector2(25f, 2f) * 4f,
							scale = 4f,
							flipped = true,
							layerDepth = (vector3.Y + 2f + 0.11f) * 64f / 10000f,
							id = 777f
						});
					}
					break;
				case 'K':
					if (!(key == "harveyKitchenFlame"))
					{
						if (key == "harveyKitchenSetup")
						{
							Texture2D texture14 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
							location.TemporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = texture14,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(379, 251, 31, 13),
								animationLength = 1,
								sourceRectStartingPos = new Vector2(379f, 251f),
								interval = 5000f,
								totalNumberOfLoops = 9999,
								position = new Vector2(7f, 12f) * 64f + new Vector2(-2f, 6f) * 4f,
								scale = 4f,
								layerDepth = 0.091520004f
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = texture14,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(391, 235, 5, 13),
								animationLength = 1,
								sourceRectStartingPos = new Vector2(391f, 235f),
								interval = 5000f,
								totalNumberOfLoops = 9999,
								position = new Vector2(6f, 12f) * 64f + new Vector2(8f, 4f) * 4f,
								scale = 4f,
								layerDepth = 0.091520004f
							});
							location.TemporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = texture14,
								sourceRect = new Microsoft.Xna.Framework.Rectangle(399, 229, 11, 21),
								animationLength = 1,
								sourceRectStartingPos = new Vector2(399f, 229f),
								interval = 5000f,
								totalNumberOfLoops = 9999,
								position = new Vector2(4f, 12f) * 64f + new Vector2(8f, -5f) * 4f,
								scale = 4f,
								layerDepth = 0.091520004f
							});
							location.temporarySprites.Add(new TemporaryAnimatedSprite(27, new Vector2(6f, 12f) * 64f + new Vector2(0f, -5f) * 4f, Color.White, 10)
							{
								totalNumberOfLoops = 999,
								layerDepth = 0.092159994f
							});
							location.temporarySprites.Add(new TemporaryAnimatedSprite(27, new Vector2(6f, 12f) * 64f + new Vector2(24f, -5f) * 4f, Color.White, 10)
							{
								totalNumberOfLoops = 999,
								flipped = true,
								delayBeforeAnimationStart = 400,
								layerDepth = 0.092159994f
							});
						}
					}
					else
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.mouseCursors,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11),
							animationLength = 4,
							sourceRectStartingPos = new Vector2(276f, 1985f),
							interval = 100f,
							totalNumberOfLoops = 6,
							position = new Vector2(7f, 12f) * 64f + new Vector2(8f, 5f) * 4f,
							scale = 4f,
							layerDepth = 0.09184f
						});
					}
					break;
				case 'H':
					if (key == "farmerHoldPainting")
					{
						Texture2D texture12 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
						location.getTemporarySpriteByID(888).sourceRect.X += 15;
						location.getTemporarySpriteByID(888).sourceRectStartingPos.X += 15f;
						location.removeTemporarySpritesWithID(444);
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture12,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(476, 394, 25, 22),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(476f, 394f),
							interval = 5000f,
							totalNumberOfLoops = 9999,
							position = new Vector2(75f, 40f) * 64f + new Vector2(-4f, -33f) * 4f,
							scale = 4f,
							layerDepth = 1f,
							id = 777f
						});
					}
					break;
				case 'F':
				{
					if (!(key == "farmerForestVision"))
					{
						break;
					}
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(393, 1973, 1, 1), 9999f, 1, 999999, new Vector2(0f, 0f) * 64f, flicker: false, flipped: false, 0.9f, 0f, Color.LimeGreen * 0.85f, Game1.viewport.Width * 2, 0f, 0f, 0f, local: true)
					{
						alpha = 0f,
						alphaFade = -0.002f,
						id = 1f
					});
					Game1.player.mailReceived.Add("canReadJunimoText");
					int num40 = -64;
					int num41 = -64;
					int num42 = 0;
					int num43 = 0;
					while (num41 < Game1.viewport.Height + 128)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(367 + ((num42 % 2 == 0) ? 8 : 0), 1969, 8, 8), 9999f, 1, 999999, new Vector2(num40, num41), flicker: false, flipped: false, 0.99f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							alpha = 0f,
							alphaFade = -0.0015f,
							xPeriodic = true,
							xPeriodicLoopTime = 4000f,
							xPeriodicRange = 64f,
							yPeriodic = true,
							yPeriodicLoopTime = 5000f,
							yPeriodicRange = 96f,
							rotationChange = (float)Game1.random.Next(-1, 2) * (float)Math.PI / 256f,
							id = 1f,
							delayBeforeAnimationStart = 20 * num42
						});
						num40 += 128;
						if (num40 > Game1.viewport.Width + 64)
						{
							num43++;
							num40 = ((num43 % 2 == 0) ? (-64) : 64);
							num41 += 128;
						}
						num42++;
					}
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width / 2 - 100, Game1.viewport.Height / 2 - 240), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
					{
						alpha = 0f,
						alphaFade = -0.001f,
						id = 1f,
						delayBeforeAnimationStart = 6000,
						scaleChange = 0.004f,
						xPeriodic = true,
						xPeriodicLoopTime = 4000f,
						xPeriodicRange = 64f,
						yPeriodic = true,
						yPeriodicLoopTime = 5000f,
						yPeriodicRange = 32f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width / 4 - 100, Game1.viewport.Height / 4 - 120), flicker: false, flipped: false, 0.99f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
					{
						alpha = 0f,
						alphaFade = -0.001f,
						id = 1f,
						delayBeforeAnimationStart = 9000,
						scaleChange = 0.004f,
						xPeriodic = true,
						xPeriodicLoopTime = 4000f,
						xPeriodicRange = 64f,
						yPeriodic = true,
						yPeriodicLoopTime = 5000f,
						yPeriodicRange = 32f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width * 3 / 4, Game1.viewport.Height / 3 - 120), flicker: false, flipped: false, 0.98f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
					{
						alpha = 0f,
						alphaFade = -0.001f,
						id = 1f,
						delayBeforeAnimationStart = 12000,
						scaleChange = 0.004f,
						xPeriodic = true,
						xPeriodicLoopTime = 4000f,
						xPeriodicRange = 64f,
						yPeriodic = true,
						yPeriodicLoopTime = 5000f,
						yPeriodicRange = 32f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width / 3 - 60, Game1.viewport.Height * 3 / 4 - 120), flicker: false, flipped: false, 0.97f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
					{
						alpha = 0f,
						alphaFade = -0.001f,
						id = 1f,
						delayBeforeAnimationStart = 15000,
						scaleChange = 0.004f,
						xPeriodic = true,
						xPeriodicLoopTime = 4000f,
						xPeriodicRange = 64f,
						yPeriodic = true,
						yPeriodicLoopTime = 5000f,
						yPeriodicRange = 32f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width * 2 / 3, Game1.viewport.Height * 2 / 3 - 120), flicker: false, flipped: false, 0.96f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
					{
						alpha = 0f,
						alphaFade = -0.001f,
						id = 1f,
						delayBeforeAnimationStart = 18000,
						scaleChange = 0.004f,
						xPeriodic = true,
						xPeriodicLoopTime = 4000f,
						xPeriodicRange = 64f,
						yPeriodic = true,
						yPeriodicLoopTime = 5000f,
						yPeriodicRange = 32f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width / 8, Game1.viewport.Height / 5 - 120), flicker: false, flipped: false, 0.95f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
					{
						alpha = 0f,
						alphaFade = -0.001f,
						id = 1f,
						delayBeforeAnimationStart = 19500,
						scaleChange = 0.004f,
						xPeriodic = true,
						xPeriodicLoopTime = 4000f,
						xPeriodicRange = 64f,
						yPeriodic = true,
						yPeriodicLoopTime = 5000f,
						yPeriodicRange = 32f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width * 2 / 3, Game1.viewport.Height / 5 - 120), flicker: false, flipped: false, 0.94f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
					{
						alpha = 0f,
						alphaFade = -0.001f,
						id = 1f,
						delayBeforeAnimationStart = 21000,
						scaleChange = 0.004f,
						xPeriodic = true,
						xPeriodicLoopTime = 4000f,
						xPeriodicRange = 64f,
						yPeriodic = true,
						yPeriodicLoopTime = 5000f,
						yPeriodicRange = 32f
					});
					break;
				}
				}
				break;
			case 11:
				switch (key[8])
				{
				case 'a':
					if (key == "krobusBeach")
					{
						for (int num36 = 0; num36 < 8; num36++)
						{
							location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f, 4, 0, new Vector2(84f + ((num36 % 2 == 0) ? 0.25f : (-0.05f)), 41f) * 64f, flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0.02f, Color.White, 0.75f, 0.003f, 0f, 0f)
							{
								delayBeforeAnimationStart = 500 + num36 * 1000,
								startSound = "waterSlosh"
							});
						}
						underwaterSprites = new List<TemporaryAnimatedSprite>();
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(82f, 52f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2688,
							delayBeforeAnimationStart = 0,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(82f, 52f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 3008,
							delayBeforeAnimationStart = 2000,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(88f, 52f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2688,
							delayBeforeAnimationStart = 150,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(88f, 52f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 3008,
							delayBeforeAnimationStart = 2000,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(90f, 52f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2816,
							delayBeforeAnimationStart = 300,
							pingPong = true
						});
						underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(79f, 52f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -1f),
							xPeriodic = true,
							xPeriodicLoopTime = 3000f,
							xPeriodicRange = 16f,
							light = true,
							lightcolor = Color.Black,
							lightRadius = 1f,
							yStopCoordinate = 2816,
							delayBeforeAnimationStart = 1000,
							pingPong = true
						});
					}
					break;
				case 'k':
					if (key == "woodswalker")
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
							sourceRect = new Microsoft.Xna.Framework.Rectangle(448, 419, 16, 21),
							sourceRectStartingPos = new Vector2(448f, 419f),
							animationLength = 4,
							totalNumberOfLoops = 7,
							interval = 150f,
							scale = 4f,
							position = new Vector2(4f, 1f) * 64f + new Vector2(5f, 22f) * 4f,
							shakeIntensity = 1f,
							motion = new Vector2(1f, 0f),
							xStopCoordinate = 576,
							layerDepth = 1f,
							id = 996f
						});
					}
					break;
				case 'i':
					if (!(key == "springOnion"))
					{
						if (key == "parrotSlide")
						{
							location.getTemporarySpriteByID(666).yStopCoordinate = 5632;
							location.getTemporarySpriteByID(666).motion.X = 0f;
							location.getTemporarySpriteByID(666).motion.Y = 1f;
						}
					}
					else
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(1, 129, 16, 16), 200f, 8, 999999, new Vector2(84f, 39f) * 64f, flicker: false, flipped: false, 0.4736f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 999f
						});
					}
					break;
				case 'p':
					if (!(key == "curtainOpen"))
					{
						if (key == "jasGiftOpen")
						{
							location.getTemporarySpriteByID(999).paused = false;
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(537, 1850, 11, 10), 1500f, 1, 1, new Vector2(23f, 16f) * 64f + new Vector2(16f, -48f), flicker: false, flipped: false, 0.99f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								motion = new Vector2(0f, -0.25f),
								delayBeforeAnimationStart = 500,
								yStopCoordinate = 928
							});
							location.temporarySprites.AddRange(Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(1440, 992, 128, 64), 5, Color.White, 300));
						}
					}
					else
					{
						location.getTemporarySpriteByID(999).sourceRect.X = 672;
						Game1.playSound("shwip");
					}
					break;
				case 'l':
					if (key == "parrotSplat")
					{
						aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 165, 24, 22), 100f, 6, 9999, new Vector2(Game1.viewport.X + Game1.graphics.GraphicsDevice.Viewport.Width, Game1.viewport.Y + 64), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 999f,
							motion = new Vector2(-2f, 4f),
							acceleration = new Vector2(-0.1f, 0f),
							delayBeforeAnimationStart = 0,
							yStopCoordinate = 5568,
							xStopCoordinate = 1504,
							reachedStopCoordinate = parrotSplat
						});
					}
					break;
				case 'f':
					if (key == "shaneCliffs")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(533, 1864, 19, 27), 99999f, 1, 99999, new Vector2(83f, 98f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 999f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(552, 1862, 31, 21), 99999f, 1, 99999, new Vector2(83f, 98f) * 64f + new Vector2(-16f, 0f), flicker: false, flipped: false, 0.0001f, 0f, Color.White, 4f, 0f, 0f, 0f));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(549, 1891, 19, 12), 99999f, 1, 99999, new Vector2(84f, 99f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 999f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(549, 1891, 19, 12), 99999f, 1, 99999, new Vector2(82f, 98f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 999f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(542, 1893, 4, 6), 99999f, 1, 99999, new Vector2(83f, 99f) * 64f + new Vector2(-8f, 4f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
					}
					break;
				case 'v':
				{
					if (!(key == "krobusraven"))
					{
						break;
					}
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(0, 0, 32, 32), 100f, 5, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						pingPong = true,
						motion = new Vector2(-2f, 0f),
						yPeriodic = true,
						yPeriodicLoopTime = 3000f,
						yPeriodicRange = 16f,
						startSound = "shadowpeep"
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(0, 32, 32, 32), 30f, 5, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-2.5f, 0f),
						yPeriodic = true,
						yPeriodicLoopTime = 2800f,
						yPeriodicRange = 16f,
						delayBeforeAnimationStart = 8000
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(0, 64, 32, 39), 100f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						pingPong = true,
						motion = new Vector2(-3f, 0f),
						yPeriodic = true,
						yPeriodicLoopTime = 2000f,
						yPeriodicRange = 16f,
						delayBeforeAnimationStart = 15000,
						startSound = "fireball"
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1886, 35, 29), 9999f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(-3f, 0f),
						yPeriodic = true,
						yPeriodicLoopTime = 2200f,
						yPeriodicRange = 32f,
						local = true,
						delayBeforeAnimationStart = 20000
					});
					for (int num37 = 0; num37 < 12; num37++)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(16, 594, 16, 12), 100f, 2, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f + (float)Game1.random.Next(-128, 128)), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-2f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = Game1.random.Next(1500, 2000),
							yPeriodicRange = 32f,
							local = true,
							delayBeforeAnimationStart = 24000 + num37 * 200,
							startSound = ((num37 == 0) ? "yoba" : null)
						});
					}
					int num38 = 0;
					if (Game1.player.mailReceived.Contains("Capsule_Broken"))
					{
						for (int num39 = 0; num39 < 3; num39++)
						{
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(639, 785, 16, 16), 100f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f + (float)Game1.random.Next(-128, 128)), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								motion = new Vector2(-2f, 0f),
								yPeriodic = true,
								yPeriodicLoopTime = Game1.random.Next(1500, 2000),
								yPeriodicRange = 16f,
								local = true,
								delayBeforeAnimationStart = 30000 + num39 * 500,
								startSound = ((num39 == 0) ? "UFO" : null)
							});
						}
						num38 += 5000;
					}
					if (Game1.year <= 2)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(150, 259, 9, 9), 10f, 4, 9999999, new Vector2(Game1.viewport.Width + 4, (float)Game1.viewport.Height * 0.33f + 44f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-2f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = 3000f,
							yPeriodicRange = 8f,
							delayBeforeAnimationStart = 30000 + num38
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(2, 129, 120, 27), 1090f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-2f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = 3000f,
							yPeriodicRange = 8f,
							startSound = "discoverMineral",
							delayBeforeAnimationStart = 30000 + num38
						});
						num38 += 5000;
					}
					else if (Game1.year <= 3)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(150, 259, 9, 9), 10f, 4, 9999999, new Vector2(Game1.viewport.Width + 4, (float)Game1.viewport.Height * 0.33f + 44f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-2f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = 3000f,
							yPeriodicRange = 8f,
							delayBeforeAnimationStart = 30000 + num38
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(1, 104, 100, 24), 1090f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-2f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = 3000f,
							yPeriodicRange = 8f,
							startSound = "newArtifact",
							delayBeforeAnimationStart = 30000 + num38
						});
						num38 += 5000;
					}
					if (Game1.MasterPlayer.totalMoneyEarned >= 100000000)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(125, 108, 34, 50), 1090f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-2f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = 3000f,
							yPeriodicRange = 8f,
							startSound = "discoverMineral",
							delayBeforeAnimationStart = 30000 + num38
						});
						num38 += 5000;
					}
					break;
				}
				case 'r':
					if (key == "wizardWarp2")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(387, 1965, 16, 31), 9999f, 1, 999999, new Vector2(54f, 34f) * 64f + new Vector2(0f, 4f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-1f, 2f),
							acceleration = new Vector2(-0.1f, 0.2f),
							scaleChange = 0.03f,
							alphaFade = 0.001f
						});
					}
					break;
				case 'h':
					if (key == "linusLights")
					{
						Game1.currentLightSources.Add(new LightSource(2, new Vector2(55f, 62f) * 64f, 2f, LightSource.LightContext.None, 0L));
						Game1.currentLightSources.Add(new LightSource(2, new Vector2(60f, 62f) * 64f, 2f, LightSource.LightContext.None, 0L));
						Game1.currentLightSources.Add(new LightSource(2, new Vector2(57f, 60f) * 64f, 3f, LightSource.LightContext.None, 0L));
						Game1.currentLightSources.Add(new LightSource(2, new Vector2(57f, 60f) * 64f, 2f, LightSource.LightContext.None, 0L));
						Game1.currentLightSources.Add(new LightSource(2, new Vector2(47f, 70f) * 64f, 2f, LightSource.LightContext.None, 0L));
						Game1.currentLightSources.Add(new LightSource(2, new Vector2(52f, 63f) * 64f, 2f, LightSource.LightContext.None, 0L));
					}
					break;
				case 't':
					if (key == "dickGlitter")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f)
						{
							delayBeforeAnimationStart = 200
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f + new Vector2(32f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f)
						{
							delayBeforeAnimationStart = 300
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f + new Vector2(0f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f)
						{
							delayBeforeAnimationStart = 100
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f + new Vector2(16f, 16f), flicker: false, flipped: false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f)
						{
							delayBeforeAnimationStart = 400
						});
					}
					break;
				case 'o':
					if (key == "elliottBoat")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(461, 1843, 32, 51), 1000f, 2, 9999, new Vector2(15f, 26f) * 64f + new Vector2(-28f, 0f), flicker: false, flipped: false, 0.1664f, 0f, Color.White, 4f, 0f, 0f, 0f));
					}
					break;
				}
				break;
			case 7:
				switch (key[1])
				{
				case 'u':
					if (key == "sunroom")
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
							sourceRect = new Microsoft.Xna.Framework.Rectangle(304, 486, 24, 26),
							sourceRectStartingPos = new Vector2(304f, 486f),
							animationLength = 1,
							totalNumberOfLoops = 997,
							interval = 99999f,
							scale = 4f,
							position = new Vector2(4f, 8f) * 64f + new Vector2(8f, -8f) * 4f,
							layerDepth = 0.0512f,
							id = 996f
						});
						location.addCritter(new Butterfly(location.getRandomTile()).setStayInbounds(stayInbounds: true));
						while (Game1.random.NextDouble() < 0.5)
						{
							location.addCritter(new Butterfly(location.getRandomTile()).setStayInbounds(stayInbounds: true));
						}
					}
					break;
				case 'a':
					if (key == "jasGift")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1231, 16, 16), 100f, 6, 1, new Vector2(22f, 16f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 999f,
							paused = true,
							holdLastFrame = true
						});
					}
					break;
				case 'e':
					if (!(key == "wedding"))
					{
						break;
					}
					if (farmer.IsMale)
					{
						oldShirt = farmer.shirt;
						farmer.changeShirt(10);
						oldPants = farmer.pantsColor;
						farmer.changePantStyle(0);
						farmer.changePants(new Color(49, 49, 49));
					}
					foreach (Farmer farmerActor in farmerActors)
					{
						if (farmerActor.IsMale)
						{
							farmerActor.changeShirt(10);
							farmerActor.changePants(new Color(49, 49, 49));
							farmerActor.changePantStyle(0);
						}
					}
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(540, 1196, 98, 54), 99999f, 1, 99999, new Vector2(25f, 60f) * 64f + new Vector2(0f, -64f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(540, 1250, 98, 25), 99999f, 1, 99999, new Vector2(25f, 60f) * 64f + new Vector2(0f, 54f) * 4f + new Vector2(0f, -64f), flicker: false, flipped: false, 0f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(527, 1249, 12, 25), 99999f, 1, 99999, new Vector2(24f, 62f) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(527, 1249, 12, 25), 99999f, 1, 99999, new Vector2(32f, 62f) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(527, 1249, 12, 25), 99999f, 1, 99999, new Vector2(24f, 69f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(527, 1249, 12, 25), 99999f, 1, 99999, new Vector2(32f, 69f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
					break;
				case 'i':
					if (key == "dickBag")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(528, 1435, 16, 16), 99999f, 1, 99999, new Vector2(48f, 7f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
					}
					break;
				case 'o':
				{
					if (!(key == "JoshMom"))
					{
						if (key == "joshDog")
						{
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1916, 12, 20), 500f, 6, 9999, new Vector2(53f, 67f) * 64f + new Vector2(3f, 3f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								id = 1f
							});
						}
						break;
					}
					TemporaryAnimatedSprite temporaryAnimatedSprite4 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(416, 1931, 58, 65), 750f, 2, 99999, new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f);
					temporaryAnimatedSprite4.alpha = 0.6f;
					temporaryAnimatedSprite4.local = true;
					temporaryAnimatedSprite4.xPeriodic = true;
					temporaryAnimatedSprite4.xPeriodicLoopTime = 2000f;
					temporaryAnimatedSprite4.xPeriodicRange = 32f;
					temporaryAnimatedSprite4.motion = new Vector2(0f, -1.25f);
					temporaryAnimatedSprite4.initialPosition = new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height);
					TemporaryAnimatedSprite temporaryAnimatedSprite6 = temporaryAnimatedSprite4;
					location.temporarySprites.Add(temporaryAnimatedSprite6);
					for (int num35 = 0; num35 < 19; num35++)
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(516, 1916, 7, 10), 99999f, 1, 99999, new Vector2(64f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							alphaFade = 0.01f,
							local = true,
							motion = new Vector2(-1f, -1f),
							parentSprite = temporaryAnimatedSprite6,
							delayBeforeAnimationStart = (num35 + 1) * 1000
						});
					}
					break;
				}
				case 'r':
					if (key == "dropEgg")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(176, 800f, 1, 0, new Vector2(6f, 4f) * 64f + new Vector2(0f, 32f), flicker: false, flipped: false)
						{
							rotationChange = (float)Math.PI / 24f,
							motion = new Vector2(0f, -7f),
							acceleration = new Vector2(0f, 0.3f),
							endFunction = eggSmashEndFunction,
							layerDepth = 1f
						});
					}
					break;
				}
				break;
			case 9:
				switch (key[5])
				{
				case 'G':
					if (key == "sauceGood")
					{
						Utility.addSprinklesToLocation(location, OffsetTileX(64), OffsetTileY(16), 3, 1, 800, 200, Color.White);
					}
					break;
				case 'F':
					if (key == "sauceFire")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.mouseCursors,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11),
							animationLength = 4,
							sourceRectStartingPos = new Vector2(276f, 1985f),
							interval = 100f,
							totalNumberOfLoops = 5,
							position = OffsetPosition(new Vector2(64f, 16f) * 64f + new Vector2(3f, -4f) * 4f),
							scale = 4f,
							layerDepth = 1f
						});
						aboveMapSprites = new List<TemporaryAnimatedSprite>();
						for (int num34 = 0; num34 < 8; num34++)
						{
							aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), OffsetPosition(new Vector2(64f, 16f) * 64f) + new Vector2(Game1.random.Next(-16, 32), 0f), flipped: false, 0.002f, Color.Gray)
							{
								alpha = 0.75f,
								motion = new Vector2(1f, -1f) + new Vector2((float)(Game1.random.Next(100) - 50) / 100f, (float)(Game1.random.Next(100) - 50) / 100f),
								interval = 99999f,
								layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
								scale = 3f,
								scaleChange = 0.01f,
								rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
								delayBeforeAnimationStart = num34 * 25
							});
						}
					}
					break;
				case 'B':
					if (!(key == "shakeBush"))
					{
						if (key == "movieBush")
						{
							location.temporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\bushes"),
								sourceRect = new Microsoft.Xna.Framework.Rectangle(65, 58, 30, 35),
								sourceRectStartingPos = new Vector2(65f, 58f),
								animationLength = 1,
								totalNumberOfLoops = 999,
								interval = 999f,
								scale = 4f,
								position = new Vector2(4f, 1f) * 64f + new Vector2(33f, 13f) * 4f,
								layerDepth = 0.99f,
								id = 777f
							});
						}
					}
					else
					{
						location.getTemporarySpriteByID(777).shakeIntensity = 1f;
					}
					break;
				case 'T':
					if (key == "shakeTent")
					{
						location.getTemporarySpriteByID(999).shakeIntensity = 1f;
					}
					break;
				case 'S':
				{
					if (!(key == "EmilySign"))
					{
						break;
					}
					int num30 = 0;
					aboveMapSprites = new List<TemporaryAnimatedSprite>();
					for (int num31 = 0; num31 < 10; num31++)
					{
						num30 = 0;
						int num32 = Game1.random.Next(Game1.graphics.GraphicsDevice.Viewport.Height - 128);
						for (int num33 = Game1.graphics.GraphicsDevice.Viewport.Width; num33 >= -64; num33 -= 48)
						{
							aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(597, 1888, 16, 16), 99999f, 1, 99999, new Vector2(num33, num32), flicker: false, flipped: false, 1f, 0.02f, Color.White, 4f, 0f, 0f, 0f)
							{
								delayBeforeAnimationStart = num31 * 600 + num30 * 25,
								startSound = ((num30 == 0) ? "dwoop" : null),
								local = true
							});
							num30++;
						}
					}
					break;
				}
				case 't':
					if (key == "joshSteak")
					{
						location.temporarySprites.Clear();
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1936, 12, 20), 80f, 4, 99999, new Vector2(53f, 67f) * 64f + new Vector2(3f, 3f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 1f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(497, 1918, 11, 11), 999f, 1, 9999, new Vector2(50f, 68f) * 64f + new Vector2(32f, -8f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
					}
					break;
				case 'a':
					if (key == "samSkate1")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0), 9999f, 1, 999, new Vector2(12f, 90f) * 64f, flicker: false, flipped: false, 1E-05f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(4f, 0f),
							acceleration = new Vector2(-0.008f, 0f),
							xStopCoordinate = 1344,
							reachedStopCoordinate = samPreOllie,
							attachedCharacter = getActorByName("Sam"),
							id = 92473f
						});
					}
					break;
				case 'C':
					if (key == "pennyCook")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), new Vector2(10f, 6f) * 64f, flipped: false, 0f, Color.White)
						{
							layerDepth = 1f,
							animationLength = 6,
							interval = 75f,
							motion = new Vector2(0f, -0.5f)
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), new Vector2(10f, 6f) * 64f + new Vector2(16f, 0f), flipped: false, 0f, Color.White)
						{
							layerDepth = 0.1f,
							animationLength = 6,
							interval = 75f,
							motion = new Vector2(0f, -0.5f),
							delayBeforeAnimationStart = 500
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), new Vector2(10f, 6f) * 64f + new Vector2(-16f, 0f), flipped: false, 0f, Color.White)
						{
							layerDepth = 1f,
							animationLength = 6,
							interval = 75f,
							motion = new Vector2(0f, -0.5f),
							delayBeforeAnimationStart = 750
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), new Vector2(10f, 6f) * 64f, flipped: false, 0f, Color.White)
						{
							layerDepth = 0.1f,
							animationLength = 6,
							interval = 75f,
							motion = new Vector2(0f, -0.5f),
							delayBeforeAnimationStart = 1000
						});
					}
					break;
				case 'M':
					if (key == "pennyMess")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(739, 999999f, 1, 0, new Vector2(10f, 5f) * 64f, flicker: false, flipped: false));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(740, 999999f, 1, 0, new Vector2(15f, 5f) * 64f, flicker: false, flipped: false));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite(741, 999999f, 1, 0, new Vector2(16f, 6f) * 64f, flicker: false, flipped: false));
					}
					break;
				case 'u':
					if (key == "abbyOuija")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 960, 128, 128), 60f, 4, 0, new Vector2(6f, 9f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f));
					}
					break;
				}
				break;
			case 19:
				switch (key[0])
				{
				case 'm':
					if (key == "movieTheater_screen")
					{
						int num26 = int.Parse(split[2]);
						int num27 = int.Parse(split[3]);
						bool flag = bool.Parse(split[4]);
						int num28 = num26 * 128 + num27 / 5 * 64;
						int num29 = num27 % 5 * 96;
						location.removeTemporarySpritesWithIDLocal(998f);
						if (num27 >= 0)
						{
							location.temporarySprites.Add(new TemporaryAnimatedSprite
							{
								texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Movies"),
								sourceRect = new Microsoft.Xna.Framework.Rectangle(16 + num29, num28, 90, 61),
								sourceRectStartingPos = new Vector2(16 + num29, num28),
								animationLength = 1,
								totalNumberOfLoops = 9999,
								interval = 5000f,
								scale = 4f,
								position = new Vector2(4f, 1f) * 64f + new Vector2(3f, 7f) * 4f,
								shakeIntensity = (flag ? 1f : 0f),
								layerDepth = 0.1f + (float)(num26 * num27) / 10000f,
								id = 998f
							});
						}
					}
					break;
				case 'w':
					if (key == "willyCrabExperiment")
					{
						Texture2D texture10 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture10,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 127f),
							pingPong = true,
							interval = 250f,
							totalNumberOfLoops = 99999,
							id = 11f,
							position = new Vector2(2f, 4f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture10,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 146f),
							pingPong = true,
							interval = 200f,
							totalNumberOfLoops = 99999,
							id = 1f,
							initialPosition = new Vector2(2f, 6f) * 64f,
							yPeriodic = true,
							yPeriodicLoopTime = 8000f,
							yPeriodicRange = 32f,
							position = new Vector2(2f, 6f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture10,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 127f),
							pingPong = true,
							interval = 100f,
							totalNumberOfLoops = 99999,
							id = 11f,
							position = new Vector2(1f, 5.75f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture10,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 127f),
							pingPong = true,
							interval = 100f,
							totalNumberOfLoops = 99999,
							id = 11f,
							position = new Vector2(5f, 3f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture10,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 127f),
							pingPong = true,
							interval = 140f,
							totalNumberOfLoops = 99999,
							id = 22f,
							position = new Vector2(4f, 6f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture10,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 127f),
							pingPong = true,
							interval = 140f,
							totalNumberOfLoops = 99999,
							id = 22f,
							position = new Vector2(8.5f, 5f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture10,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 146f),
							pingPong = true,
							interval = 170f,
							totalNumberOfLoops = 99999,
							id = 222f,
							position = new Vector2(6f, 3.25f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture10,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 146f),
							pingPong = true,
							interval = 190f,
							totalNumberOfLoops = 99999,
							id = 222f,
							position = new Vector2(6f, 6f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture10,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 146f),
							pingPong = true,
							interval = 150f,
							totalNumberOfLoops = 99999,
							id = 222f,
							position = new Vector2(7f, 4f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture10,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 146f),
							pingPong = true,
							interval = 200f,
							totalNumberOfLoops = 99999,
							id = 2f,
							position = new Vector2(4f, 7f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture10,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 127f),
							pingPong = true,
							interval = 180f,
							totalNumberOfLoops = 99999,
							id = 3f,
							position = new Vector2(8f, 6f) * 64f,
							yPeriodic = true,
							yPeriodicLoopTime = 10000f,
							yPeriodicRange = 32f,
							initialPosition = new Vector2(8f, 6f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture10,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 146f),
							pingPong = true,
							interval = 220f,
							totalNumberOfLoops = 99999,
							id = 33f,
							position = new Vector2(9f, 6f) * 64f,
							scale = 4f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture10,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
							animationLength = 3,
							sourceRectStartingPos = new Vector2(259f, 146f),
							pingPong = true,
							interval = 150f,
							totalNumberOfLoops = 99999,
							id = 33f,
							position = new Vector2(10f, 5f) * 64f,
							scale = 4f
						});
					}
					break;
				case 'E':
				{
					if (!(key == "EmilySongBackLights"))
					{
						break;
					}
					aboveMapSprites = new List<TemporaryAnimatedSprite>();
					for (int num12 = 0; num12 < 5; num12++)
					{
						for (int num13 = 0; num13 < Game1.graphics.GraphicsDevice.Viewport.Height + 48; num13 += 48)
						{
							aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(681, 1890, 18, 12), 42241f, 1, 1, new Vector2((num12 + 1) * Game1.graphics.GraphicsDevice.Viewport.Width / 5 - Game1.graphics.GraphicsDevice.Viewport.Width / 7, -24 + num13), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
							{
								xPeriodic = true,
								xPeriodicLoopTime = 1760f,
								xPeriodicRange = 128 + num13 / 12 * 4,
								delayBeforeAnimationStart = num12 * 100 + num13 / 4,
								local = true
							});
						}
					}
					for (int num14 = 0; num14 < 27; num14++)
					{
						int num15 = 0;
						int num16 = Game1.random.Next(64, Game1.graphics.GraphicsDevice.Viewport.Height - 64);
						int num17 = Game1.random.Next(800, 2000);
						int num18 = Game1.random.Next(32, 64);
						bool pulse = Game1.random.NextDouble() < 0.25;
						int num19 = Game1.random.Next(-6, -3);
						for (int num20 = 0; num20 < 8; num20++)
						{
							aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(616 + num15 * 10, 1891, 10, 10), 42241f, 1, 1, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width, num16), flicker: false, flipped: false, 0.01f, 0f, Color.White * (1f - (float)num20 * 0.11f), 4f, 0f, 0f, 0f)
							{
								yPeriodic = true,
								motion = new Vector2(num19, 0f),
								yPeriodicLoopTime = num17,
								pulse = pulse,
								pulseTime = 440f,
								pulseAmount = 1.5f,
								yPeriodicRange = num18,
								delayBeforeAnimationStart = 14000 + num14 * 900 + num20 * 100,
								local = true
							});
						}
					}
					for (int num21 = 0; num21 < 15; num21++)
					{
						int num22 = 0;
						int num23 = Game1.random.Next(Game1.graphics.GraphicsDevice.Viewport.Width - 128);
						for (int num24 = Game1.graphics.GraphicsDevice.Viewport.Height; num24 >= -64; num24 -= 48)
						{
							aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(597, 1888, 16, 16), 99999f, 1, 99999, new Vector2(num23, num24), flicker: false, flipped: false, 1f, 0.02f, Color.White, 4f, 0f, -(float)Math.PI / 2f, 0f)
							{
								delayBeforeAnimationStart = 27500 + num21 * 880 + num22 * 25,
								local = true
							});
							num22++;
						}
					}
					for (int num25 = 0; num25 < 120; num25++)
					{
						aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(626 + num25 / 28 * 10, 1891, 10, 10), 2000f, 1, 1, new Vector2(Game1.random.Next(Game1.graphics.GraphicsDevice.Viewport.Width), Game1.random.Next(Game1.graphics.GraphicsDevice.Viewport.Height)), flicker: false, flipped: false, 0.01f, 0f, Color.White, 0.1f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -2f),
							alphaFade = 0.002f,
							scaleChange = 0.5f,
							scaleChangeChange = -0.0085f,
							delayBeforeAnimationStart = 27500 + num25 * 110,
							local = true
						});
					}
					break;
				}
				}
				break;
			case 17:
				switch (key[0])
				{
				case 'l':
					if (key == "leahPaintingSetup")
					{
						Texture2D texture9 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture9,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 393, 15, 28),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(368f, 393f),
							interval = 5000f,
							totalNumberOfLoops = 99999,
							position = new Vector2(72f, 38f) * 64f + new Vector2(3f, -13f) * 4f,
							scale = 4f,
							layerDepth = 0.1f,
							id = 999f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture9,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 393, 15, 28),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(368f, 393f),
							interval = 5000f,
							totalNumberOfLoops = 99999,
							position = new Vector2(74f, 40f) * 64f + new Vector2(3f, -17f) * 4f,
							scale = 4f,
							layerDepth = 0.1f,
							id = 888f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture9,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(369, 424, 11, 15),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(369f, 424f),
							interval = 9999f,
							totalNumberOfLoops = 99999,
							position = new Vector2(75f, 40f) * 64f + new Vector2(-2f, -11f) * 4f,
							scale = 4f,
							layerDepth = 0.01f,
							id = 444f
						});
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.mouseCursors,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(96, 1822, 32, 34),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(96f, 1822f),
							interval = 5000f,
							totalNumberOfLoops = 99999,
							position = new Vector2(79f, 36f) * 64f,
							scale = 4f,
							layerDepth = 0.1f
						});
					}
					break;
				case 's':
					if (key == "springOnionRemove")
					{
						location.removeTemporarySpritesWithID(777);
					}
					break;
				case 'E':
					if (key == "EmilyBoomBoxStart")
					{
						location.getTemporarySpriteByID(999).pulse = true;
						location.getTemporarySpriteByID(999).pulseTime = 420f;
					}
					break;
				case 'd':
					if (key == "doneWithSlideShow")
					{
						Summit summit2 = location as Summit;
						summit2.isShowingEndSlideshow = false;
					}
					break;
				case 'm':
					if (key == "maruElectrocution")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1664, 16, 32), 40f, 1, 20, new Vector2(7f, 5f) * 64f - new Vector2(-4f, 8f), flicker: true, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
					}
					break;
				}
				break;
			case 5:
				switch (key[0])
				{
				case 's':
					if (key == "samTV")
					{
						Texture2D texture8 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
						location.TemporarySprites.Add(new TemporaryAnimatedSprite
						{
							texture = texture8,
							sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 350, 25, 29),
							animationLength = 1,
							sourceRectStartingPos = new Vector2(368f, 350f),
							interval = 5000f,
							totalNumberOfLoops = 99999,
							position = new Vector2(37f, 14f) * 64f + new Vector2(4f, -12f) * 4f,
							scale = 4f,
							layerDepth = 0.9f
						});
					}
					break;
				case 'h':
					if (key == "heart")
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(211, 428, 7, 6), 2000f, 1, 0, OffsetPosition(new Vector2(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]))) * 64f + new Vector2(-16f, -16f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(0f, -0.5f),
							alphaFade = 0.01f
						});
					}
					break;
				case 'r':
					if (key == "robot")
					{
						TemporaryAnimatedSprite temporaryAnimatedSprite4 = new TemporaryAnimatedSprite(getActorByName("robot").Sprite.textureName, new Microsoft.Xna.Framework.Rectangle(35, 42, 35, 42), 50f, 1, 9999, new Vector2(13f, 27f) * 64f - new Vector2(0f, 32f), flicker: false, flipped: false, 0.98f, 0f, Color.White, 4f, 0f, 0f, 0f);
						temporaryAnimatedSprite4.acceleration = new Vector2(0f, -0.01f);
						temporaryAnimatedSprite4.accelerationChange = new Vector2(0f, -0.0001f);
						TemporaryAnimatedSprite temporaryAnimatedSprite5 = temporaryAnimatedSprite4;
						location.temporarySprites.Add(temporaryAnimatedSprite5);
						for (int num11 = 0; num11 < 420; num11++)
						{
							location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(Game1.random.Next(4) * 64, 320, 64, 64), new Vector2(Game1.random.Next(96), 136f), flipped: false, 0.01f, Color.White * 0.75f)
							{
								layerDepth = 1f,
								delayBeforeAnimationStart = num11 * 10,
								animationLength = 1,
								currentNumberOfLoops = 0,
								interval = 9999f,
								motion = new Vector2(Game1.random.Next(-100, 100) / (num11 + 20), 0.25f + (float)num11 / 100f),
								parentSprite = temporaryAnimatedSprite5
							});
						}
					}
					break;
				}
				break;
			case 20:
				if (key == "BoatParrotSquawkStop")
				{
					TemporaryAnimatedSprite temporaryAnimatedSprite2 = aboveMapSprites.First();
					temporaryAnimatedSprite2.sourceRect.X = 0;
					temporaryAnimatedSprite2.sourceRectStartingPos.X = 0f;
				}
				break;
			case 23:
				if (key == "leahStopHoldingPainting")
				{
					location.getTemporarySpriteByID(999).sourceRect.X -= 15;
					location.getTemporarySpriteByID(999).sourceRectStartingPos.X -= 15f;
					location.removeTemporarySpritesWithIDLocal(777f);
					Game1.playSound("thudStep");
				}
				break;
			case 6:
				if (key == "qiCave")
				{
					Texture2D texture3 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = texture3,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(415, 216, 96, 89),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(415f, 216f),
						interval = 999999f,
						totalNumberOfLoops = 99999,
						position = new Vector2(2f, 2f) * 64f + new Vector2(112f, 25f) * 4f,
						scale = 4f,
						layerDepth = 1E-07f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = texture3,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(370, 272, 107, 64),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(370f, 216f),
						interval = 999999f,
						totalNumberOfLoops = 99999,
						position = new Vector2(2f, 2f) * 64f + new Vector2(67f, 81f) * 4f,
						scale = 4f,
						layerDepth = 1.1E-07f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.objectSpriteSheet,
						sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 803, 16, 16),
						sourceRectStartingPos = new Vector2(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 803, 16, 16).X, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 803, 16, 16).Y),
						animationLength = 1,
						interval = 999999f,
						id = 803f,
						totalNumberOfLoops = 99999,
						position = new Vector2(13f, 7f) * 64f + new Vector2(1f, 9f) * 4f,
						scale = 4f,
						layerDepth = 2.1E-06f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = texture3,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
						animationLength = 5,
						sourceRectStartingPos = new Vector2(432f, 171f),
						pingPong = true,
						interval = 100f,
						totalNumberOfLoops = 99999,
						id = 11f,
						position = new Vector2(8f, 6f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = texture3,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
						animationLength = 5,
						sourceRectStartingPos = new Vector2(432f, 171f),
						pingPong = true,
						interval = 90f,
						totalNumberOfLoops = 99999,
						id = 11f,
						position = new Vector2(5f, 7f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = texture3,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
						animationLength = 5,
						sourceRectStartingPos = new Vector2(432f, 171f),
						pingPong = true,
						interval = 120f,
						totalNumberOfLoops = 99999,
						id = 11f,
						position = new Vector2(7f, 10f) * 64f,
						scale = 4f,
						layerDepth = 1f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = texture3,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
						animationLength = 5,
						sourceRectStartingPos = new Vector2(432f, 171f),
						pingPong = true,
						interval = 80f,
						totalNumberOfLoops = 99999,
						id = 11f,
						position = new Vector2(15f, 7f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = texture3,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
						animationLength = 5,
						sourceRectStartingPos = new Vector2(432f, 171f),
						pingPong = true,
						interval = 100f,
						totalNumberOfLoops = 99999,
						id = 11f,
						position = new Vector2(12f, 11f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = texture3,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
						animationLength = 5,
						sourceRectStartingPos = new Vector2(432f, 171f),
						pingPong = true,
						interval = 105f,
						totalNumberOfLoops = 99999,
						id = 11f,
						position = new Vector2(16f, 10f) * 64f,
						scale = 4f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = texture3,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
						animationLength = 5,
						sourceRectStartingPos = new Vector2(432f, 171f),
						pingPong = true,
						interval = 85f,
						totalNumberOfLoops = 99999,
						id = 11f,
						position = new Vector2(3f, 9f) * 64f,
						scale = 4f
					});
				}
				break;
			case 3:
				if (key == "wed")
				{
					aboveMapSprites = new List<TemporaryAnimatedSprite>();
					Game1.flashAlpha = 1f;
					for (int i = 0; i < 150; i++)
					{
						Vector2 position = new Vector2(Game1.random.Next(Game1.viewport.Width - 128), Game1.random.Next(Game1.viewport.Height));
						int num = Game1.random.Next(2, 5);
						aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(424, 1266, 8, 8), 60f + (float)Game1.random.Next(-10, 10), 7, 999999, position, flicker: false, flipped: false, 0.99f, 0f, Color.White, num, 0f, 0f, 0f)
						{
							local = true,
							motion = new Vector2(0.1625f, -0.25f) * num
						});
					}
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(558, 1425, 20, 26), 400f, 3, 99999, new Vector2(26f, 64f) * 64f, flicker: false, flipped: false, 0.416f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						pingPong = true
					});
					Game1.changeMusicTrack("wedding", track_interruptable: false, Game1.MusicContext.Event);
					Game1.musicPlayerVolume = 0f;
				}
				break;
			case 4:
			case 21:
			case 22:
				break;
			}
		}

		private Microsoft.Xna.Framework.Rectangle skipBounds()
		{
			int num = Math.Max(Game1.xEdge, 8);
			int num2 = 88;
			int height = 60;
			return new Microsoft.Xna.Framework.Rectangle(Game1.viewport.Width - num2 - num, 12, num2, height);
		}

		public void receiveGamePadButton(Buttons b)
		{
			if (b == Buttons.Y && !skipped && skippable)
			{
				OnClickSkip();
			}
		}

		public void receiveMouseClick(int x, int y)
		{
			x = Game1.getMouseX(ui_scale: false);
			y = Game1.getMouseY(ui_scale: false);
			if (!skipped && skippable && skipBounds().Contains(x, y))
			{
				OnClickSkip();
			}
		}

		private void OnClickSkip()
		{
			skipped = true;
			skipEvent();
			Game1.freezeControls = false;
		}

		public void skipEvent()
		{
			if (playerControlSequence)
			{
				EndPlayerControlSequence();
			}
			Game1.playSound("drumkit6");
			actorPositionsAfterMove.Clear();
			foreach (NPC actor in actors)
			{
				bool ignoreStopAnimation = actor.Sprite.ignoreStopAnimation;
				actor.Sprite.ignoreStopAnimation = true;
				actor.Halt();
				actor.Sprite.ignoreStopAnimation = ignoreStopAnimation;
				resetDialogueIfNecessary(actor);
			}
			farmer.Halt();
			farmer.ignoreCollisions = false;
			Game1.exitActiveMenu();
			Game1.dialogueUp = false;
			Game1.dialogueTyping = false;
			Game1.pauseTime = 0f;
			switch (id)
			{
			case -157039427:
				endBehaviors(new string[2] { "end", "islandDepart" }, Game1.currentLocation);
				break;
			case -888999:
			{
				Object @object = new Object(864, 1)
				{
					specialItem = true
				};
				@object.questItem.Value = true;
				Game1.player.addItemByMenuIfNecessary(@object);
				Game1.player.addQuest(130);
				endBehaviors(new string[1] { "end" }, Game1.currentLocation);
				break;
			}
			case -666777:
			{
				List<Item> list = new List<Item>();
				Game1.player.team.RequestLimitedNutDrops("Birdie", null, 0, 0, 5, 5);
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotBirdieReward"))
				{
					Game1.addMailForTomorrow("gotBirdieReward", noLetter: true, sendToEveryone: true);
				}
				if (!Game1.player.craftingRecipes.ContainsKey("Fairy Dust"))
				{
					Game1.player.craftingRecipes.Add("Fairy Dust", 0);
				}
				endBehaviors(new string[1] { "end" }, Game1.currentLocation);
				break;
			}
			case 6497428:
				endBehaviors(new string[2] { "end", "Leo" }, Game1.currentLocation);
				break;
			case -78765:
				endBehaviors(new string[2] { "end", "tunnelDepart" }, Game1.currentLocation);
				break;
			case 690006:
				if (Game1.player.hasItemWithNameThatContains("Green Slime Egg") == null)
				{
					Game1.player.addItemByMenuIfNecessary(new Object(680, 1));
				}
				endBehaviors(new string[1] { "end" }, Game1.currentLocation);
				break;
			case 191393:
				if (Game1.player.hasItemWithNameThatContains("Stardew Hero Trophy") == null)
				{
					Game1.player.addItemByMenuIfNecessary(new Object(Vector2.Zero, 116));
				}
				endBehaviors(new string[4] { "end", "position", "52", "20" }, Game1.currentLocation);
				break;
			case 2123343:
				endBehaviors(new string[2] { "end", "newDay" }, Game1.currentLocation);
				break;
			case 404798:
				if (Game1.player.hasItemWithNameThatContains("Copper Pan") == null)
				{
					Game1.player.addItemByMenuIfNecessary(new Pan());
				}
				endBehaviors(new string[1] { "end" }, Game1.currentLocation);
				break;
			case 26:
				if (!Game1.player.craftingRecipes.ContainsKey("Wild Bait"))
				{
					Game1.player.craftingRecipes.Add("Wild Bait", 0);
				}
				endBehaviors(new string[1] { "end" }, Game1.currentLocation);
				break;
			case 611173:
				if (!Game1.player.activeDialogueEvents.ContainsKey("pamHouseUpgrade") && !Game1.player.activeDialogueEvents.ContainsKey("pamHouseUpgradeAnonymous"))
				{
					Game1.player.activeDialogueEvents.Add("pamHouseUpgrade", 4);
				}
				endBehaviors(new string[1] { "end" }, Game1.currentLocation);
				break;
			case 3091462:
				if (Game1.player.hasItemWithNameThatContains("My First Painting") == null)
				{
					Game1.player.addItemByMenuIfNecessary(new Furniture(1802, Vector2.Zero));
				}
				endBehaviors(new string[1] { "end" }, Game1.currentLocation);
				break;
			case 3918602:
				if (Game1.player.hasItemWithNameThatContains("Sam's Boombox") == null)
				{
					Game1.player.addItemByMenuIfNecessary(new Furniture(1309, Vector2.Zero));
				}
				endBehaviors(new string[1] { "end" }, Game1.currentLocation);
				break;
			case 19:
				if (!Game1.player.cookingRecipes.ContainsKey("Cookies"))
				{
					Game1.player.cookingRecipes.Add("Cookies", 0);
				}
				endBehaviors(new string[1] { "end" }, Game1.currentLocation);
				break;
			case 992553:
				if (!Game1.player.craftingRecipes.ContainsKey("Furnace"))
				{
					Game1.player.craftingRecipes.Add("Furnace", 0);
				}
				if (!Game1.player.hasQuest(11))
				{
					Game1.player.addQuest(11);
				}
				endBehaviors(new string[1] { "end" }, Game1.currentLocation);
				break;
			case 900553:
				if (!Game1.player.craftingRecipes.ContainsKey("Garden Pot"))
				{
					Game1.player.craftingRecipes.Add("Garden Pot", 0);
				}
				if (Game1.player.hasItemWithNameThatContains("Garden Pot") == null)
				{
					Game1.player.addItemByMenuIfNecessary(new Object(Vector2.Zero, 62));
				}
				endBehaviors(new string[1] { "end" }, Game1.currentLocation);
				break;
			case 980558:
				if (!Game1.player.craftingRecipes.ContainsKey("Mini-Jukebox"))
				{
					Game1.player.craftingRecipes.Add("Mini-Jukebox", 0);
				}
				if (Game1.player.hasItemWithNameThatContains("Mini-Jukebox") == null)
				{
					Game1.player.addItemByMenuIfNecessary(new Object(Vector2.Zero, 209));
				}
				endBehaviors(new string[1] { "end" }, Game1.currentLocation);
				break;
			case 60367:
				endBehaviors(new string[2] { "end", "beginGame" }, Game1.currentLocation);
				break;
			case 739330:
				if (Game1.player.hasItemWithNameThatContains("Bamboo Pole") == null)
				{
					Game1.player.addItemByMenuIfNecessary(new FishingRod());
				}
				endBehaviors(new string[4] { "end", "position", "43", "36" }, Game1.currentLocation);
				break;
			case 112:
				endBehaviors(new string[1] { "end" }, Game1.currentLocation);
				Game1.player.mailReceived.Add("canReadJunimoText");
				break;
			case 558292:
				Game1.player.eventsSeen.Remove(2146991);
				endBehaviors(new string[2] { "end", "bed" }, Game1.currentLocation);
				break;
			case 100162:
				if (Game1.player.hasItemWithNameThatContains("Rusty Sword") == null)
				{
					Game1.player.addItemByMenuIfNecessary(new MeleeWeapon(0));
					TutorialManager.Instance.triggerAttackChoice();
				}
				Game1.player.Position = new Vector2(-9999f, -99999f);
				endBehaviors(new string[1] { "end" }, Game1.currentLocation);
				break;
			default:
				endBehaviors(new string[1] { "end" }, Game1.currentLocation);
				break;
			}
		}

		public void receiveKeyPress(Keys k)
		{
		}

		public void receiveKeyRelease(Keys k)
		{
		}

		public void receiveActionPress(int xTile, int yTile)
		{
			if (xTile != playerControlTargetTile.X || yTile != playerControlTargetTile.Y)
			{
				return;
			}
			string text = playerControlSequenceID;
			if (!(text == "haleyBeach"))
			{
				if (text == "haleyBeach2")
				{
					EndPlayerControlSequence();
					CurrentCommand++;
				}
			}
			else
			{
				props.Clear();
				Game1.playSound("coin");
				playerControlTargetTile = new Point(35, 11);
				playerControlSequenceID = "haleyBeach2";
			}
		}

		public void startSecretSantaEvent()
		{
			playerControlSequence = false;
			playerControlSequenceID = null;
			eventCommands = festivalData["secretSanta"].Split('/');
			doingSecretSanta = true;
			setUpSecretSantaCommands();
			currentCommand = 0;
		}

		public void festivalUpdate(GameTime time)
		{
			Game1.player.team.festivalScoreStatus.UpdateState(Game1.player.festivalScore.ToString() ?? "");
			if (festivalTimer > 0)
			{
				oldTime = festivalTimer;
				festivalTimer -= time.ElapsedGameTime.Milliseconds;
				string text = playerControlSequenceID;
				if (text == "iceFishing")
				{
					if (!Game1.player.UsingTool)
					{
						Game1.player.forceCanMove();
					}
					if (oldTime % 500 < festivalTimer % 500)
					{
						NPC actorByName = getActorByName("Pam");
						actorByName.Sprite.sourceRect.Offset(actorByName.Sprite.SourceRect.Width, 0);
						if (actorByName.Sprite.sourceRect.X >= actorByName.Sprite.Texture.Width)
						{
							actorByName.Sprite.sourceRect.Offset(-actorByName.Sprite.Texture.Width, 0);
						}
						actorByName = getActorByName("Elliott");
						actorByName.Sprite.sourceRect.Offset(actorByName.Sprite.SourceRect.Width, 0);
						if (actorByName.Sprite.sourceRect.X >= actorByName.Sprite.Texture.Width)
						{
							actorByName.Sprite.sourceRect.Offset(-actorByName.Sprite.Texture.Width, 0);
						}
						actorByName = getActorByName("Willy");
						actorByName.Sprite.sourceRect.Offset(actorByName.Sprite.SourceRect.Width, 0);
						if (actorByName.Sprite.sourceRect.X >= actorByName.Sprite.Texture.Width)
						{
							actorByName.Sprite.sourceRect.Offset(-actorByName.Sprite.Texture.Width, 0);
						}
					}
					if (oldTime % 29900 < festivalTimer % 29900)
					{
						getActorByName("Willy").shake(500);
						Game1.playSound("dwop");
						temporaryLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(112, 432, 16, 16), getActorByName("Willy").Position + new Vector2(0f, -96f), flipped: false, 0.015f, Color.White)
						{
							layerDepth = 1f,
							scale = 4f,
							interval = 9999f,
							motion = new Vector2(0f, -1f)
						});
					}
					if (oldTime % 45900 < festivalTimer % 45900)
					{
						getActorByName("Pam").shake(500);
						Game1.playSound("dwop");
						temporaryLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(112, 432, 16, 16), getActorByName("Pam").Position + new Vector2(0f, -96f), flipped: false, 0.015f, Color.White)
						{
							layerDepth = 1f,
							scale = 4f,
							interval = 9999f,
							motion = new Vector2(0f, -1f)
						});
					}
					if (oldTime % 59900 < festivalTimer % 59900)
					{
						getActorByName("Elliott").shake(500);
						Game1.playSound("dwop");
						temporaryLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(112, 432, 16, 16), getActorByName("Elliott").Position + new Vector2(0f, -96f), flipped: false, 0.015f, Color.White)
						{
							layerDepth = 1f,
							scale = 4f,
							interval = 9999f,
							motion = new Vector2(0f, -1f)
						});
					}
				}
				if (festivalTimer <= 0)
				{
					Game1.player.Halt();
					string text2 = playerControlSequenceID;
					if (!(text2 == "eggHunt"))
					{
						if (text2 == "iceFishing")
						{
							EndPlayerControlSequence();
							eventCommands = festivalData["afterIceFishing"].Split('/');
							currentCommand = 0;
							if (Game1.activeClickableMenu != null)
							{
								Game1.activeClickableMenu.emergencyShutDown();
							}
							Game1.activeClickableMenu = null;
							if (Game1.player.UsingTool && Game1.player.CurrentTool != null && Game1.player.CurrentTool is FishingRod)
							{
								(Game1.player.CurrentTool as FishingRod).doneFishing(Game1.player);
							}
							Game1.screenOverlayTempSprites.Clear();
							Game1.player.forceCanMove();
						}
					}
					else
					{
						EndPlayerControlSequence();
						eventCommands = festivalData["afterEggHunt"].Split('/');
						currentCommand = 0;
					}
				}
			}
			if (startSecretSantaAfterDialogue && !Game1.dialogueUp)
			{
				Game1.globalFadeToBlack(startSecretSantaEvent, 0.01f);
				startSecretSantaAfterDialogue = false;
			}
			Game1.player.festivalScore = Math.Min(Game1.player.festivalScore, 9999);
			if (waitingForMenuClose && Game1.activeClickableMenu == null)
			{
				string text3 = festivalData["file"];
				_ = text3 == "fall16";
				waitingForMenuClose = false;
			}
		}

		private void setUpSecretSantaCommands()
		{
			int num = 0;
			int num2 = 0;
			try
			{
				num = getActorByName(mySecretSanta.Name).getTileX();
				num2 = getActorByName(mySecretSanta.Name).getTileY();
			}
			catch (Exception)
			{
				mySecretSanta = getActorByName("Lewis");
				num = getActorByName(mySecretSanta.Name).getTileX();
				num2 = getActorByName(mySecretSanta.Name).getTileY();
			}
			string newValue = "";
			string newValue2 = "";
			switch (mySecretSanta.Age)
			{
			case 2:
				newValue = Game1.LoadStringByGender(mySecretSanta.gender, "Strings\\StringsFromCSFiles:Event.cs.1497");
				newValue2 = Game1.LoadStringByGender(mySecretSanta.gender, "Strings\\StringsFromCSFiles:Event.cs.1498");
				break;
			case 0:
			case 1:
				switch (mySecretSanta.Manners)
				{
				case 0:
				case 1:
					newValue = Game1.LoadStringByGender(mySecretSanta.gender, "Strings\\StringsFromCSFiles:Event.cs.1499");
					newValue2 = Game1.LoadStringByGender(mySecretSanta.gender, "Strings\\StringsFromCSFiles:Event.cs.1500");
					break;
				case 2:
					newValue = Game1.LoadStringByGender(mySecretSanta.gender, "Strings\\StringsFromCSFiles:Event.cs.1501");
					newValue2 = (mySecretSanta.Name.Equals("George") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1503") : Game1.LoadStringByGender(mySecretSanta.gender, "Strings\\StringsFromCSFiles:Event.cs.1504"));
					break;
				}
				break;
			}
			for (int i = 0; i < eventCommands.Length; i++)
			{
				eventCommands[i] = eventCommands[i].Replace("secretSanta", mySecretSanta.Name);
				eventCommands[i] = eventCommands[i].Replace("warpX", num.ToString() ?? "");
				eventCommands[i] = eventCommands[i].Replace("warpY", num2.ToString() ?? "");
				eventCommands[i] = eventCommands[i].Replace("dialogue1", newValue);
				eventCommands[i] = eventCommands[i].Replace("dialogue2", newValue2);
			}
		}

		public void drawFarmers(SpriteBatch b)
		{
			foreach (Farmer farmerActor in farmerActors)
			{
				farmerActor.draw(b);
			}
		}

		public virtual bool ShouldHideCharacter(NPC n)
		{
			if (n is Child && doingSecretSanta)
			{
				return true;
			}
			return false;
		}

		public void draw(SpriteBatch b)
		{
			if (currentCustomEventScript != null)
			{
				currentCustomEventScript.draw(b);
				return;
			}
			foreach (NPC actor in actors)
			{
				if (!ShouldHideCharacter(actor))
				{
					actor.Name.Equals("Marcello");
					if (actor.ySourceRectOffset == 0)
					{
						actor.draw(b);
					}
					else
					{
						actor.draw(b, actor.ySourceRectOffset);
					}
				}
			}
			foreach (Object prop in props)
			{
				prop.drawAsProp(b);
			}
			foreach (Prop festivalProp in festivalProps)
			{
				festivalProp.draw(b);
			}
			if (isFestival)
			{
				string text = festivalData["file"];
				if (text == "fall16")
				{
					Vector2 vector = Game1.GlobalToLocal(Game1.viewport, new Vector2(37f, 56f) * 64f);
					vector.X += 4f;
					int num = (int)vector.X + 168;
					vector.Y += 8f;
					for (int i = 0; i < Game1.player.team.grangeDisplay.Count; i++)
					{
						if (Game1.player.team.grangeDisplay[i] != null)
						{
							vector.Y += 42f;
							vector.X += 4f;
							b.Draw(Game1.shadowTexture, vector, Game1.shadowTexture.Bounds, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
							vector.Y -= 42f;
							vector.X -= 4f;
							Game1.player.team.grangeDisplay[i].drawInMenu(b, vector, 1f, 1f, (float)i / 1000f + 0.001f, StackDrawType.Hide);
						}
						vector.X += 60f;
						if (vector.X >= (float)num)
						{
							vector.X = num - 168;
							vector.Y += 64f;
						}
					}
				}
			}
			if (drawTool)
			{
				Game1.drawTool(farmer);
			}
		}

		public void drawUnderWater(SpriteBatch b)
		{
			if (underwaterSprites == null)
			{
				return;
			}
			foreach (TemporaryAnimatedSprite underwaterSprite in underwaterSprites)
			{
				underwaterSprite.draw(b);
			}
		}

		public void drawAfterMap(SpriteBatch b)
		{
			if (aboveMapSprites != null)
			{
				foreach (TemporaryAnimatedSprite aboveMapSprite in aboveMapSprites)
				{
					aboveMapSprite.draw(b);
				}
			}
			if (!Game1.game1.takingMapScreenshot && playerControlSequenceID != null)
			{
				switch (playerControlSequenceID)
				{
				case "eggHunt":
					b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(32, 32, 224, 160), Color.Black * 0.5f);
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1514", festivalTimer / 1000), Color.Black, Color.Yellow, new Vector2(64f, 64f), 0f, 1f, 1f, tiny: false);
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1515", Game1.player.festivalScore), Color.Black, Color.Pink, new Vector2(64f, 128f), 0f, 1f, 1f, tiny: false);
					if (Game1.IsMultiplayer)
					{
						Game1.player.team.festivalScoreStatus.Draw(b, new Vector2(32f, Game1.viewport.Height - 32), 4f, 0.99f, PlayerStatusList.HorizontalAlignment.Left, PlayerStatusList.VerticalAlignment.Bottom);
					}
					break;
				case "fair":
					b.End();
					Game1.PushUIMode();
					b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
					b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(16, 16, 128 + ((Game1.player.festivalScore > 999) ? 16 : 0), 64), Color.Black * 0.75f);
					b.Draw(Game1.mouseCursors, new Vector2(32f, 32f), new Microsoft.Xna.Framework.Rectangle(338, 400, 8, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					Game1.drawWithBorder(Game1.player.festivalScore.ToString() ?? "", Color.Black, Color.White, new Vector2(72f, 21 + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? 8 : (LocalizedContentManager.CurrentLanguageLatin ? 16 : 8))), 0f, 1f, 1f, tiny: false);
					if (Game1.activeClickableMenu == null)
					{
						Game1.dayTimeMoneyBox.drawMoneyBox(b, Game1.dayTimeMoneyBox.xPositionOnScreen, 4);
					}
					b.End();
					Game1.PopUIMode();
					b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
					if (Game1.IsMultiplayer)
					{
						Game1.player.team.festivalScoreStatus.Draw(b, new Vector2(32f, Game1.viewport.Height - 32), 4f, 0.99f, PlayerStatusList.HorizontalAlignment.Left, PlayerStatusList.VerticalAlignment.Bottom);
					}
					break;
				case "iceFishing":
					b.End();
					b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
					b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(16, 16, 128 + ((Game1.player.festivalScore > 999) ? 16 : 0), 128), Color.Black * 0.75f);
					b.Draw(festivalTexture, new Vector2(32f, 16f), new Microsoft.Xna.Framework.Rectangle(112, 432, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					Game1.drawWithBorder(Game1.player.festivalScore.ToString() ?? "", Color.Black, Color.White, new Vector2(96f, 21 + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? 8 : (LocalizedContentManager.CurrentLanguageLatin ? 16 : 8))), 0f, 1f, 1f, tiny: false);
					Game1.drawWithBorder(Utility.getMinutesSecondsStringFromMilliseconds(festivalTimer), Color.Black, Color.White, new Vector2(32f, 93f), 0f, 1f, 1f, tiny: false);
					b.End();
					b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
					if (Game1.IsMultiplayer)
					{
						Game1.player.team.festivalScoreStatus.Draw(b, new Vector2(32f, Game1.viewport.Height - 32), 4f, 0.99f, PlayerStatusList.HorizontalAlignment.Left, PlayerStatusList.VerticalAlignment.Bottom);
					}
					break;
				}
			}
			if (spriteTextToDraw != null && spriteTextToDraw.Length > 0)
			{
				SpriteText.drawStringHorizontallyCenteredAt(b, spriteTextToDraw, Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height - 192, int_useMeForAnything, -1, 999999, 1f, 1f, junimoText: false, int_useMeForAnything2);
			}
			foreach (NPC actor in actors)
			{
				actor.drawAboveAlwaysFrontLayer(b);
			}
			if (skippable && !Game1.options.SnappyMenus && !Game1.game1.takingMapScreenshot)
			{
				Microsoft.Xna.Framework.Rectangle rectangle = skipBounds();
				Color white = Color.White;
				b.Draw(sourceRectangle: new Microsoft.Xna.Framework.Rectangle(205, 406, 22, 15), texture: Game1.mouseCursors, position: Utility.PointToVector2(rectangle.Location), color: white, rotation: 0f, origin: Vector2.Zero, scale: 4f, effects: SpriteEffects.None, layerDepth: 0.92f);
			}
			if (currentCustomEventScript != null)
			{
				currentCustomEventScript.drawAboveAlwaysFront(b);
			}
		}

		public void EndPlayerControlSequence()
		{
			playerControlSequence = false;
			playerControlSequenceID = null;
		}

		public void OnPlayerControlSequenceEnd(string id)
		{
			Game1.player.CanMove = false;
			Game1.player.Halt();
		}

		public void setUpPlayerControlSequence(string id)
		{
			playerControlSequenceID = id;
			playerControlSequence = true;
			Game1.player.CanMove = true;
			Game1.viewportFreeze = false;
			Game1.forceSnapOnNextViewportUpdate = true;
			Game1.globalFade = false;
			doingSecretSanta = false;
			if (id == null)
			{
				return;
			}
			switch (id.Length)
			{
			case 10:
				switch (id[0])
				{
				case 'h':
					if (id == "haleyBeach")
					{
						playerControlTargetTile = new Point(53, 8);
						props.Add(new Object(new Vector2(53f, 8f), 742, 1)
						{
							Flipped = false
						});
						Game1.player.canOnlyWalk = false;
					}
					break;
				case 'p':
					if (id == "parrotRide")
					{
						Game1.player.canOnlyWalk = false;
						currentCommand++;
					}
					break;
				case 'i':
					if (id == "iceFishing")
					{
						festivalTimer = 120000;
						farmer.festivalScore = 0;
						farmer.CurrentToolIndex = 0;
						farmer.TemporaryItem = new FishingRod();
						(farmer.CurrentTool as FishingRod).attachments[1] = new Object(687, 1);
						farmer.CurrentToolIndex = 0;
					}
					break;
				}
				break;
			case 11:
				switch (id[0])
				{
				case 'e':
					if (id == "eggFestival")
					{
						festivalHost = getActorByName("Lewis");
						hostMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1521");
					}
					break;
				case 'i':
					if (id == "iceFestival")
					{
						festivalHost = getActorByName("Lewis");
						hostMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1548");
					}
					break;
				}
				break;
			case 4:
				switch (id[0])
				{
				case 'l':
					if (id == "luau")
					{
						festivalHost = getActorByName("Lewis");
						hostMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1527");
					}
					break;
				case 'f':
					if (id == "fair")
					{
						festivalHost = getActorByName("Lewis");
						hostMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1535");
					}
					break;
				}
				break;
			case 7:
				switch (id[0])
				{
				case 'j':
					if (id == "jellies")
					{
						festivalHost = getActorByName("Lewis");
						hostMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1531");
					}
					break;
				case 'e':
				{
					if (!(id == "eggHunt"))
					{
						break;
					}
					for (int i = 0; i < Game1.currentLocation.map.GetLayer("Paths").LayerWidth; i++)
					{
						for (int j = 0; j < Game1.currentLocation.map.GetLayer("Paths").LayerHeight; j++)
						{
							if (Game1.currentLocation.map.GetLayer("Paths").Tiles[i, j] != null)
							{
								festivalProps.Add(new Prop(festivalTexture, Game1.currentLocation.map.GetLayer("Paths").Tiles[i, j].TileIndex, 1, 1, 1, i, j));
							}
						}
					}
					festivalTimer = 52000;
					currentCommand++;
					break;
				}
				}
				break;
			case 9:
				switch (id[0])
				{
				case 'h':
					if (id == "halloween")
					{
						temporaryLocation.objects.Add(new Vector2(33f, 13f), new Chest(0, new List<Item>
						{
							new Object(373, 1)
						}, new Vector2(33f, 13f)));
					}
					break;
				case 'c':
					if (id == "christmas")
					{
						Random r = new Random((int)(Game1.uniqueIDForThisGame / 2uL) ^ Game1.year ^ (int)Game1.player.UniqueMultiplayerID);
						secretSantaRecipient = Utility.getRandomTownNPC(r);
						while (mySecretSanta == null || mySecretSanta.Equals(secretSantaRecipient) || mySecretSanta.isDivorcedFrom(farmer))
						{
							mySecretSanta = Utility.getRandomTownNPC(r);
						}
						Game1.debugOutput = "Secret Santa Recipient: " + secretSantaRecipient.Name + "  My Secret Santa: " + mySecretSanta.Name;
					}
					break;
				}
				break;
			case 14:
				if (id == "flowerFestival")
				{
					festivalHost = getActorByName("Lewis");
					hostMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1524");
					if (NetWorldState.checkAnywhereForWorldStateID("trashBearDone"))
					{
						Game1.currentLocation.setMapTileIndex(62, 29, -1, "Buildings");
						Game1.currentLocation.setMapTileIndex(64, 29, -1, "Buildings");
						Game1.currentLocation.setMapTileIndex(72, 49, -1, "Buildings");
					}
				}
				break;
			case 8:
				if (id == "boatRide")
				{
					Game1.viewportFreeze = true;
					Game1.currentViewportTarget = Utility.PointToVector2(Game1.viewportCenter);
					currentCommand++;
				}
				break;
			case 5:
			case 6:
			case 12:
			case 13:
				break;
			}
		}

		public bool canMoveAfterDialogue()
		{
			if (playerControlSequenceID != null && playerControlSequenceID.Equals("eggHunt"))
			{
				Game1.player.canMove = true;
				CurrentCommand++;
			}
			return playerControlSequence;
		}

		public void forceFestivalContinue()
		{
			if (festivalData["file"].Equals("fall16"))
			{
				initiateGrangeJudging();
				return;
			}
			Game1.dialogueUp = false;
			if (Game1.activeClickableMenu != null)
			{
				Game1.activeClickableMenu.emergencyShutDown();
			}
			Game1.exitActiveMenu();
			string[] array = (eventCommands = GetFestivalDataForYear("mainEvent").Split('/'));
			CurrentCommand = 0;
			eventSwitched = true;
			playerControlSequence = false;
			setUpFestivalMainEvent();
			Game1.player.Halt();
		}

		public bool isSpecificFestival(string festivalID)
		{
			if (isFestival)
			{
				return festivalData["file"].Equals(festivalID);
			}
			return false;
		}

		public void setUpFestivalMainEvent()
		{
			if (!isSpecificFestival("spring24"))
			{
				return;
			}
			List<NetDancePartner> list = new List<NetDancePartner>();
			List<NetDancePartner> list2 = new List<NetDancePartner>();
			List<string> list3 = new List<string> { "Abigail", "Penny", "Leah", "Maru", "Haley", "Emily" };
			List<string> list4 = new List<string> { "Sebastian", "Sam", "Elliott", "Harvey", "Alex", "Shane" };
			List<Farmer> list5 = (from f in Game1.getOnlineFarmers()
				orderby f.UniqueMultiplayerID
				select f).ToList();
			while (list5.Count > 0)
			{
				Farmer farmer = list5[0];
				list5.RemoveAt(0);
				if (Game1.multiplayer.isDisconnecting(farmer) || farmer.dancePartner.Value == null)
				{
					continue;
				}
				if (farmer.dancePartner.GetGender() == 1)
				{
					list.Add(farmer.dancePartner);
					if (farmer.dancePartner.IsVillager())
					{
						list3.Remove(farmer.dancePartner.TryGetVillager().Name);
					}
					list2.Add(new NetDancePartner(farmer));
				}
				else
				{
					list2.Add(farmer.dancePartner);
					if (farmer.dancePartner.IsVillager())
					{
						list4.Remove(farmer.dancePartner.TryGetVillager().Name);
					}
					list.Add(new NetDancePartner(farmer));
				}
				if (farmer.dancePartner.IsFarmer())
				{
					list5.Remove(farmer.dancePartner.TryGetFarmer());
				}
			}
			while (list.Count < 6)
			{
				string text = list3.Last();
				if (list4.Contains(Utility.getLoveInterest(text)))
				{
					list.Add(new NetDancePartner(text));
					list2.Add(new NetDancePartner(Utility.getLoveInterest(text)));
				}
				list3.Remove(text);
			}
			string text2 = GetFestivalDataForYear("mainEvent");
			for (int i = 1; i <= 6; i++)
			{
				string newValue = ((!list[i - 1].IsVillager()) ? ("farmer" + Utility.getFarmerNumberFromFarmer(list[i - 1].TryGetFarmer())) : list[i - 1].TryGetVillager().Name);
				string newValue2 = ((!list2[i - 1].IsVillager()) ? ("farmer" + Utility.getFarmerNumberFromFarmer(list2[i - 1].TryGetFarmer())) : list2[i - 1].TryGetVillager().Name);
				text2 = text2.Replace("Girl" + i, newValue);
				text2 = text2.Replace("Guy" + i, newValue2);
			}
			Regex regex = new Regex("showFrame (?<farmerName>farmer\\d) 44");
			Regex regex2 = new Regex("showFrame (?<farmerName>farmer\\d) 40");
			Regex regex3 = new Regex("animate (?<farmerName>farmer\\d) false true 600 44 45");
			Regex regex4 = new Regex("animate (?<farmerName>farmer\\d) false true 600 43 41 43 42");
			Regex regex5 = new Regex("animate (?<farmerName>farmer\\d) false true 300 46 47");
			Regex regex6 = new Regex("animate (?<farmerName>farmer\\d) false true 600 46 47");
			text2 = regex.Replace(text2, "showFrame $1 12/faceDirection $1 0");
			text2 = regex2.Replace(text2, "showFrame $1 0/faceDirection $1 2");
			text2 = regex3.Replace(text2, "animate $1 false true 600 12 13 12 14");
			text2 = regex4.Replace(text2, "animate $1 false true 596 4 0");
			text2 = regex5.Replace(text2, "animate $1 false true 150 12 13 12 14");
			text2 = regex6.Replace(text2, "animate $1 false true 600 0 3");
			string[] array = (eventCommands = text2.Split('/'));
		}

		private void judgeGrange()
		{
			int num = 14;
			Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
			int num2 = 0;
			bool flag = false;
			foreach (Item item in Game1.player.team.grangeDisplay)
			{
				if (item != null && item is Object)
				{
					if (IsItemMayorShorts(item as Object))
					{
						flag = true;
					}
					num += (item as Object).Quality + 1;
					int num3 = (item as Object).sellToStorePrice(-1L);
					if (num3 >= 20)
					{
						num++;
					}
					if (num3 >= 90)
					{
						num++;
					}
					if (num3 >= 200)
					{
						num++;
					}
					if (num3 >= 300 && (item as Object).Quality < 2)
					{
						num++;
					}
					if (num3 >= 400 && (item as Object).Quality < 1)
					{
						num++;
					}
					switch ((item as Object).Category)
					{
					case -75:
						dictionary[-75] = true;
						break;
					case -79:
						dictionary[-79] = true;
						break;
					case -18:
					case -14:
					case -6:
					case -5:
						dictionary[-5] = true;
						break;
					case -12:
					case -2:
						dictionary[-12] = true;
						break;
					case -4:
						dictionary[-4] = true;
						break;
					case -81:
					case -80:
					case -27:
						dictionary[-81] = true;
						break;
					case -7:
						dictionary[-7] = true;
						break;
					case -26:
						dictionary[-26] = true;
						break;
					}
				}
				else if (item == null)
				{
					num2++;
				}
			}
			num += Math.Min(30, dictionary.Count * 5);
			int num4 = 9 - 2 * num2;
			num = (grangeScore = num + num4);
			if (flag)
			{
				grangeScore = -666;
			}
		}

		private void lewisDoneJudgingGrange()
		{
			if (Game1.activeClickableMenu == null)
			{
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1584")));
				Game1.player.Halt();
			}
			interpretGrangeResults();
		}

		public void interpretGrangeResults()
		{
			List<Character> list = new List<Character>();
			list.Add(getActorByName("Pierre"));
			list.Add(getActorByName("Marnie"));
			list.Add(getActorByName("Willy"));
			if (grangeScore >= 90)
			{
				list.Insert(0, Game1.player);
			}
			else if (grangeScore >= 75)
			{
				list.Insert(1, Game1.player);
			}
			else if (grangeScore >= 60)
			{
				list.Insert(2, Game1.player);
			}
			else
			{
				list.Add(Game1.player);
			}
			if (list[0] is NPC && list[0].Name.Equals("Pierre"))
			{
				getActorByName("Pierre").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1591"));
			}
			else
			{
				getActorByName("Pierre").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1593"));
			}
			getActorByName("Marnie").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1595"));
			getActorByName("Willy").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1597"));
			if (grangeScore == -666)
			{
				getActorByName("Marnie").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1600"));
			}
			grangeJudged = true;
		}

		private void initiateGrangeJudging()
		{
			judgeGrange();
			hostMessage = null;
			setUpAdvancedMove("advancedMove Lewis False 2 0 0 7 8 0 4 3000 3 0 4 3000 3 0 4 3000 3 0 4 3000 -14 0 2 1000".Split(' '), lewisDoneJudgingGrange);
			getActorByName("Lewis").CurrentDialogue.Clear();
			setUpAdvancedMove("advancedMove Marnie False 0 1 4 1000".Split(' '));
			getActorByName("Marnie").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1602"));
			getActorByName("Pierre").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1604"));
			getActorByName("Willy").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1606"));
		}

		public void answerDialogueQuestion(NPC who, string answerKey)
		{
			if (!isFestival)
			{
				return;
			}
			string spouse;
			char c;
			switch (answerKey)
			{
			case "yes":
			{
				if (festivalData["file"].Equals("fall16"))
				{
					initiateGrangeJudging();
					if (Game1.IsServer)
					{
						Game1.multiplayer.sendServerToClientsMessage("festivalEvent");
					}
					break;
				}
				string[] array = (eventCommands = GetFestivalDataForYear("mainEvent").Split('/'));
				CurrentCommand = 0;
				eventSwitched = true;
				playerControlSequence = false;
				setUpFestivalMainEvent();
				if (Game1.IsServer)
				{
					Game1.multiplayer.sendServerToClientsMessage("festivalEvent");
				}
				break;
			}
			case "danceAsk":
				if (Game1.player.spouse != null && who.Name.Equals(Game1.player.spouse))
				{
					Game1.player.dancePartner.Value = who;
					spouse = Game1.player.spouse;
					if (spouse == null)
					{
						goto IL_0367;
					}
					switch (spouse.Length)
					{
					case 7:
						break;
					case 5:
						goto IL_0156;
					case 4:
						goto IL_016d;
					case 9:
						goto IL_021f;
					case 3:
						goto IL_0234;
					case 6:
						goto IL_0249;
					default:
						goto IL_0367;
					}
					c = spouse[0];
					if (c != 'A')
					{
						if (c != 'E' || !(spouse == "Elliott"))
						{
							goto IL_0367;
						}
						who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1629"));
					}
					else
					{
						if (!(spouse == "Abigail"))
						{
							goto IL_0367;
						}
						who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1613"));
					}
					goto IL_037e;
				}
				if (!who.HasPartnerForDance && Game1.player.getFriendshipLevelForNPC(who.Name) >= 1000 && !who.isMarried())
				{
					string s = "";
					switch (who.Gender)
					{
					case 0:
						s = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1633");
						break;
					case 1:
						s = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1634");
						break;
					}
					try
					{
						Game1.player.changeFriendship(250, Game1.getCharacterFromName(who.Name));
					}
					catch (Exception)
					{
					}
					Game1.player.dancePartner.Value = who;
					who.setNewDialogue(s);
					foreach (NPC actor in actors)
					{
						if (actor.CurrentDialogue != null && actor.CurrentDialogue.Count > 0 && actor.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
						{
							actor.CurrentDialogue.Clear();
						}
					}
				}
				else if (who.HasPartnerForDance)
				{
					who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1635"));
				}
				else
				{
					try
					{
						who.setNewDialogue(Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\" + who.Name)["danceRejection"]);
					}
					catch (Exception)
					{
						break;
					}
				}
				goto IL_0564;
			case "no":
				break;
				IL_037e:
				foreach (NPC actor2 in actors)
				{
					if (actor2.CurrentDialogue != null && actor2.CurrentDialogue.Count > 0 && actor2.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
					{
						actor2.CurrentDialogue.Clear();
					}
				}
				goto IL_0564;
				IL_0156:
				c = spouse[0];
				if (c != 'H')
				{
					if (c != 'P' || !(spouse == "Penny"))
					{
						goto IL_0367;
					}
					who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1615"));
				}
				else
				{
					if (!(spouse == "Haley"))
					{
						goto IL_0367;
					}
					who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1621"));
				}
				goto IL_037e;
				IL_0249:
				if (!(spouse == "Harvey"))
				{
					goto IL_0367;
				}
				who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1627"));
				goto IL_037e;
				IL_0367:
				who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1632"));
				goto IL_037e;
				IL_0234:
				if (!(spouse == "Sam"))
				{
					goto IL_0367;
				}
				who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1625"));
				goto IL_037e;
				IL_016d:
				c = spouse[0];
				if (c != 'A')
				{
					if (c != 'L')
					{
						if (c != 'M' || !(spouse == "Maru"))
						{
							goto IL_0367;
						}
						who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1617"));
					}
					else
					{
						if (!(spouse == "Leah"))
						{
							goto IL_0367;
						}
						who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1619"));
					}
				}
				else
				{
					if (!(spouse == "Alex"))
					{
						goto IL_0367;
					}
					who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1631"));
				}
				goto IL_037e;
				IL_021f:
				if (!(spouse == "Sebastian"))
				{
					goto IL_0367;
				}
				who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1623"));
				goto IL_037e;
				IL_0564:
				Game1.drawDialogue(who);
				who.immediateSpeak = true;
				who.facePlayer(Game1.player);
				who.Halt();
				break;
			}
		}

		public void addItemToGrangeDisplay(Item i, int position, bool force)
		{
			while (Game1.player.team.grangeDisplay.Count < 9)
			{
				Game1.player.team.grangeDisplay.Add(null);
			}
			if (position >= 0 && position < Game1.player.team.grangeDisplay.Count && (Game1.player.team.grangeDisplay[position] == null || force))
			{
				Game1.player.team.grangeDisplay[position] = i;
			}
		}

		private bool onGrangeChange(Item i, int position, Item old, StorageContainer container, bool onRemoval)
		{
			if (!onRemoval)
			{
				if (i.Stack > 1 || (i.Stack == 1 && old != null && old.Stack == 1 && i.canStackWith(old)))
				{
					if (old != null && i != null && old.canStackWith(i))
					{
						container.ItemsToGrabMenu.actualInventory[position].Stack = 1;
						container.heldItem = old;
						return false;
					}
					if (old != null)
					{
						Utility.addItemToInventory(old, position, container.ItemsToGrabMenu.actualInventory);
						container.heldItem = i;
						return false;
					}
					int stack = i.Stack - 1;
					Item one = i.getOne();
					one.Stack = stack;
					container.heldItem = one;
					i.Stack = 1;
				}
			}
			else if (old != null && old.Stack > 1 && !old.Equals(i))
			{
				return false;
			}
			addItemToGrangeDisplay((onRemoval && (old == null || old.Equals(i))) ? null : i, position, force: true);
			return true;
		}

		private bool onMobileGrangeChange(Item i, int position, Item old, ItemGrabMenu container, bool onRemoval)
		{
			addItemToGrangeDisplay(onRemoval ? null : i, position, force: true);
			return true;
		}

		public bool canPlayerUseTool()
		{
			if (festivalData != null && festivalData.ContainsKey("file") && festivalData["file"].Equals("winter8") && festivalTimer > 0 && !Game1.player.UsingTool)
			{
				previousFacingDirection = Game1.player.FacingDirection;
				return true;
			}
			return false;
		}

		public bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (isFestival)
			{
				if (temporaryLocation != null && temporaryLocation.objects.ContainsKey(new Vector2(tileLocation.X, tileLocation.Y)))
				{
					temporaryLocation.objects[new Vector2(tileLocation.X, tileLocation.Y)].checkForAction(who);
				}
				string text = null;
				string text2 = "";
				int num = -1;
				num = Game1.currentLocation.getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings");
				text = Game1.currentLocation.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings");
				text2 = Game1.currentLocation.getTileSheetIDAt(tileLocation.X, tileLocation.Y, "Buildings");
				if (Game1.currentSeason == "winter" && Game1.dayOfMonth == 8 && text2 == "fest" && (num == 1009 || num == 1010 || num == 1012 || num == 1013))
				{
					Game1.playSound("pig");
					return true;
				}
				bool flag = true;
				switch (num)
				{
				case 175:
				case 176:
					if (text2 == "untitled tile sheet" && who.IsLocalPlayer && festivalData["file"].Equals("fall16"))
					{
						Game1.player.eatObject(new Object(241, 1), overrideFullness: true);
					}
					break;
				case 308:
				case 309:
				{
					Response[] answerChoices5 = new Response[3]
					{
						new Response("Orange", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1645")),
						new Response("Green", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1647")),
						new Response("I", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1650"))
					};
					if (who.IsLocalPlayer && festivalData["file"].Equals("fall16"))
					{
						Game1.currentLocation.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1652")), answerChoices5, "wheelBet");
					}
					break;
				}
				case 87:
				case 88:
				{
					Response[] answerChoices4 = new Response[2]
					{
						new Response("Buy", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1654")),
						new Response("Leave", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1656"))
					};
					if (who.IsLocalPlayer && festivalData["file"].Equals("fall16"))
					{
						Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1659"), answerChoices4, "StarTokenShop");
					}
					break;
				}
				case 501:
				case 502:
				{
					Response[] answerChoices2 = new Response[2]
					{
						new Response("Play", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1662")),
						new Response("Leave", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1663"))
					};
					if (who.IsLocalPlayer && festivalData["file"].Equals("fall16"))
					{
						Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1666"), answerChoices2, "slingshotGame");
					}
					break;
				}
				case 510:
				case 511:
					if (!who.IsLocalPlayer || !festivalData["file"].Equals("fall16"))
					{
						break;
					}
					if (festivalShops == null)
					{
						festivalShops = new Dictionary<string, Dictionary<ISalable, int[]>>();
					}
					if (!festivalShops.ContainsKey("starTokenShop"))
					{
						Dictionary<ISalable, int[]> dictionary = new Dictionary<ISalable, int[]>();
						dictionary.Add(new Furniture(1307, Vector2.Zero), new int[2] { 100, 1 });
						dictionary.Add(new Hat(19), new int[2] { 500, 1 });
						dictionary.Add(new Object(Vector2.Zero, 110), new int[2] { 800, 1 });
						if (!Game1.player.mailReceived.Contains("CF_Fair"))
						{
							dictionary.Add(new Object(434, 1), new int[2] { 2000, 1 });
						}
						dictionary.Add(new Furniture(2488, Vector2.Zero), new int[2] { 500, 1 });
						Random random = new Random((int)Game1.uniqueIDForThisGame + Game1.year * 17 + 19);
						switch (random.Next(5))
						{
						case 0:
							dictionary.Add(new Object(253, 1), new int[2] { 400, 1 });
							break;
						case 1:
							dictionary.Add(new Object(215, 1), new int[2] { 250, 1 });
							break;
						case 2:
							dictionary.Add(new Ring(888), new int[2] { 1000, 1 });
							break;
						case 3:
							dictionary.Add(new Object(178, 100), new int[2] { 500, 1 });
							break;
						case 4:
							dictionary.Add(new Object(770, 24), new int[2] { 1000, 1 });
							break;
						}
						festivalShops.Add("starTokenShop", dictionary);
					}
					Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1672"), Game1.currentLocation.createYesNoResponses(), "starTokenShop");
					break;
				case 349:
				case 350:
				case 351:
					if (!festivalData["file"].Equals("fall16"))
					{
						break;
					}
					Game1.player.team.grangeMutex.RequestLock(delegate
					{
						while (Game1.player.team.grangeDisplay.Count < 9)
						{
							Game1.player.team.grangeDisplay.Add(null);
						}
						Game1.activeClickableMenu = new ItemGrabMenu(Game1.player.team.grangeDisplay.ToList(), reverseGrab: true, showReceivingMenu: true, Utility.highlightSmallObjects, null, "", null, snapToBottom: false, canBeExitedWithKey: false, playRightClickSound: true, allowRightClick: false, showOrganizeButton: false, 0, null, -1, null, 9, 3, onMobileGrangeChange, allowStack: false, null, rearrangeGrangeOnExit: true);
						waitingForMenuClose = true;
					});
					break;
				case 503:
				case 504:
				{
					Response[] answerChoices3 = new Response[2]
					{
						new Response("Play", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1662")),
						new Response("Leave", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1663"))
					};
					if (who.IsLocalPlayer && festivalData["file"].Equals("fall16"))
					{
						Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1681"), answerChoices3, "fishingGame");
					}
					break;
				}
				case 540:
					if (who.IsLocalPlayer && festivalData["file"].Equals("fall16"))
					{
						if (who.getTileX() == 29)
						{
							Game1.activeClickableMenu = new StrengthGame();
						}
						else
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1684")));
						}
					}
					break;
				case 505:
				case 506:
					if (who.IsLocalPlayer && festivalData["file"].Equals("fall16"))
					{
						if (who.Money >= 100 && !who.mailReceived.Contains("fortuneTeller" + Game1.year))
						{
							Response[] answerChoices = new Response[2]
							{
								new Response("Read", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1688")),
								new Response("No", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1690"))
							};
							Game1.currentLocation.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1691")), answerChoices, "fortuneTeller");
						}
						else if (who.mailReceived.Contains("fortuneTeller" + Game1.year))
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1694")));
						}
						else
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1695")));
						}
						who.Halt();
					}
					break;
				default:
					flag = false;
					break;
				}
				if (flag)
				{
					return true;
				}
				if (text != null)
				{
					try
					{
						string[] array = text.Split(' ');
						switch (array[0])
						{
						case "Shop":
						{
							if (!who.IsLocalPlayer)
							{
								return false;
							}
							if (festivalShops == null)
							{
								festivalShops = new Dictionary<string, Dictionary<ISalable, int[]>>();
							}
							Dictionary<ISalable, int[]> dictionary2 = null;
							if (!festivalShops.ContainsKey(array[1]))
							{
								string text3 = array[1];
								string[] array2 = festivalData[array[1]].Split(' ');
								dictionary2 = new Dictionary<ISalable, int[]>();
								int num2 = 2147483647;
								for (int i = 0; i < array2.Length; i += 4)
								{
									string text4 = array2[i];
									string text5 = array2[i + 1];
									int num3 = Convert.ToInt32(array2[i + 1]);
									int num4 = Convert.ToInt32(array2[i + 2]);
									int num5 = Convert.ToInt32(array2[i + 3]);
									Item item = null;
									if (text4 != null)
									{
										int initialStack;
										switch (text4.Length)
										{
										case 6:
										{
											char c = text4[0];
											if (c != 'O')
											{
												if (c != 'W' || !(text4 == "Weapon"))
												{
													break;
												}
												goto IL_0c2f;
											}
											if (!(text4 == "Object"))
											{
												break;
											}
											goto IL_0bec;
										}
										case 1:
										{
											char c = text4[0];
											if ((uint)c <= 72u)
											{
												if (c == 'B')
												{
													goto IL_0c24;
												}
												if (c != 'F')
												{
													if (c != 'H')
													{
														break;
													}
													goto IL_0c49;
												}
												item = Furniture.GetFurnitureInstance(num3);
												break;
											}
											if (c == 'O')
											{
												goto IL_0bec;
											}
											if (c == 'R')
											{
												goto IL_0c19;
											}
											if (c != 'W')
											{
												break;
											}
											goto IL_0c2f;
										}
										case 9:
										{
											char c = text4[1];
											if (c != 'i')
											{
												if (c != 'l' || !(text4 == "Blueprint"))
												{
													break;
												}
												goto IL_0c3a;
											}
											if (!(text4 == "BigObject"))
											{
												break;
											}
											goto IL_0c08;
										}
										case 2:
										{
											char c = text4[1];
											if (c != 'L')
											{
												if (c != 'O' || !(text4 == "BO"))
												{
													break;
												}
												goto IL_0c08;
											}
											if (!(text4 == "BL"))
											{
												break;
											}
											goto IL_0c3a;
										}
										case 4:
										{
											char c = text4[0];
											if (c != 'B')
											{
												if (c != 'R' || !(text4 == "Ring"))
												{
													break;
												}
												goto IL_0c19;
											}
											if (!(text4 == "Boot"))
											{
												break;
											}
											goto IL_0c24;
										}
										case 3:
										{
											char c = text4[2];
											if (c != 'L')
											{
												if (c != 'l')
												{
													if (c != 't' || !(text4 == "Hat"))
													{
														break;
													}
													goto IL_0c49;
												}
												if (!(text4 == "BBl"))
												{
													break;
												}
											}
											else if (!(text4 == "BBL"))
											{
												break;
											}
											goto IL_0c54;
										}
										case 12:
											{
												if (!(text4 == "BigBlueprint"))
												{
													break;
												}
												goto IL_0c54;
											}
											IL_0bec:
											initialStack = ((num5 <= 0) ? 1 : num5);
											item = new Object(num3, initialStack);
											break;
											IL_0c49:
											item = new Hat(num3);
											break;
											IL_0c3a:
											item = new Object(num3, 1, isRecipe: true);
											break;
											IL_0c24:
											item = new Boots(num3);
											break;
											IL_0c54:
											item = new Object(Vector2.Zero, num3, isRecipe: true);
											break;
											IL_0c08:
											item = new Object(Vector2.Zero, num3);
											break;
											IL_0c2f:
											item = new MeleeWeapon(num3);
											break;
											IL_0c19:
											item = new Ring(num3);
											break;
										}
									}
									if (item is Object && (item as Object).Category == -74)
									{
										num4 = (int)Math.Max(1f, (float)num4 * Game1.MasterPlayer.difficultyModifier);
									}
									if ((!(item is Object) || !(item as Object).isRecipe || !who.knowsRecipe(item.Name)) && item != null)
									{
										dictionary2.Add(item, new int[2]
										{
											num4,
											(num5 <= 0) ? num2 : num5
										});
									}
								}
								festivalShops.Add(array[1], dictionary2);
							}
							else
							{
								dictionary2 = festivalShops[array[1]];
							}
							if (dictionary2 != null && dictionary2.Count > 0)
							{
								Game1.activeClickableMenu = new ShopMenu(dictionary2);
							}
							else
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1714"));
							}
							break;
						}
						case "Message":
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromMaps:" + array[1].Replace("\"", "")));
							break;
						case "Dialogue":
							Game1.drawObjectDialogue(Game1.currentLocation.actionParamsToString(array).Replace("#", " "));
							break;
						case "LuauSoup":
							if (!specialEventVariable2)
							{
								Game1.currentLocation.tapToMove.Reset();
								Game1.player.Halt();
								Game1.activeClickableMenu = new ItemGrabMenu(new List<Item>(), reverseGrab: true, showReceivingMenu: true, Utility.highlightLuauSoupItems, clickToAddItemToLuauSoup, Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1719"), null, snapToBottom: false, canBeExitedWithKey: false, playRightClickSound: true, allowRightClick: false, showOrganizeButton: false, 0, null, -1, null, 1, 1, null, allowStack: false);
							}
							break;
						}
					}
					catch (Exception)
					{
					}
				}
				else if (isFestival)
				{
					if (who.IsLocalPlayer)
					{
						foreach (NPC actor in actors)
						{
							if (actor.getTileX() == tileLocation.X && actor.getTileY() == tileLocation.Y && actor is Child)
							{
								(actor as Child).checkAction(who, temporaryLocation);
								return true;
							}
							if (actor.getTileX() != tileLocation.X || actor.getTileY() != tileLocation.Y || (actor.CurrentDialogue.Count() < 1 && (actor.CurrentDialogue.Count() <= 0 || actor.CurrentDialogue.Peek().isOnFinalDialogue()) && !actor.Equals(festivalHost) && (!actor.datable || !festivalData["file"].Equals("spring24")) && (secretSantaRecipient == null || !actor.Name.Equals(secretSantaRecipient.Name))))
							{
								continue;
							}
							bool flag2 = who.friendshipData.ContainsKey(actor.Name) && who.friendshipData[actor.Name].IsDivorced();
							if ((grangeScore > -100 || grangeScore == -666) && actor.Equals(festivalHost) && grangeJudged)
							{
								string text6 = "";
								if (grangeScore >= 90)
								{
									Game1.playSound("reward");
									text6 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1723", grangeScore);
									Game1.player.festivalScore += 1000;
								}
								else if (grangeScore >= 75)
								{
									Game1.playSound("reward");
									text6 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1726", grangeScore);
									Game1.player.festivalScore += 500;
								}
								else if (grangeScore >= 60)
								{
									Game1.playSound("newArtifact");
									text6 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1729", grangeScore);
									Game1.player.festivalScore += 250;
								}
								else if (grangeScore == -666)
								{
									Game1.playSound("secret1");
									text6 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1730");
									Game1.player.festivalScore += 750;
								}
								else
								{
									Game1.playSound("newArtifact");
									text6 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1732", grangeScore);
									Game1.player.festivalScore += 50;
								}
								grangeScore = -100;
								actor.setNewDialogue(text6);
							}
							else if ((Game1.serverHost == null || Game1.player.Equals(Game1.serverHost.Value)) && actor.Equals(festivalHost) && (actor.CurrentDialogue.Count() == 0 || actor.CurrentDialogue.Peek().isOnFinalDialogue()) && hostMessage != null)
							{
								actor.setNewDialogue(hostMessage);
							}
							else if ((Game1.serverHost == null || Game1.player.Equals(Game1.serverHost.Value)) && actor.Equals(festivalHost) && (actor.CurrentDialogue.Count == 0 || actor.CurrentDialogue.Peek().isOnFinalDialogue()) && hostMessage != null)
							{
								actor.setNewDialogue(hostMessage);
							}
							if (isSpecificFestival("spring24") && !flag2 && ((bool)actor.datable || (who.spouse != null && actor.Name.Equals(who.spouse))))
							{
								actor.grantConversationFriendship(who);
								if (who.dancePartner.Value == null)
								{
									if (actor.CurrentDialogue.Count > 0 && actor.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
									{
										actor.CurrentDialogue.Clear();
									}
									if (actor.CurrentDialogue.Count == 0)
									{
										actor.CurrentDialogue.Push(new Dialogue("...", actor));
										if (actor.name.Equals(who.spouse))
										{
											actor.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1736", actor.displayName), add: true);
										}
										else
										{
											actor.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1738", actor.displayName), add: true);
										}
									}
									else if (actor.CurrentDialogue.Peek().isOnFinalDialogue())
									{
										Dialogue item2 = actor.CurrentDialogue.Peek();
										Game1.drawDialogue(actor);
										actor.faceTowardFarmerForPeriod(3000, 2, faceAway: false, who);
										who.Halt();
										actor.CurrentDialogue = new Stack<Dialogue>();
										actor.CurrentDialogue.Push(new Dialogue("...", actor));
										actor.CurrentDialogue.Push(item2);
										return true;
									}
								}
								else if (actor.CurrentDialogue.Count > 0 && actor.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
								{
									actor.CurrentDialogue.Clear();
								}
							}
							if (!flag2 && secretSantaRecipient != null && actor.Name.Equals(secretSantaRecipient.Name))
							{
								actor.grantConversationFriendship(who);
								Game1.currentLocation.createQuestionDialogue(Game1.parseText(((int)secretSantaRecipient.gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1740", secretSantaRecipient.displayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1741", secretSantaRecipient.displayName)), Game1.currentLocation.createYesNoResponses(), "secretSanta");
								who.Halt();
								return true;
							}
							if (actor.CurrentDialogue.Count == 0)
							{
								return true;
							}
							if (who.spouse != null && actor.Name.Equals(who.spouse) && !festivalData["file"].Equals("spring24"))
							{
								if (actor.isRoommate() && festivalData.ContainsKey(actor.Name + "_roommate"))
								{
									actor.CurrentDialogue.Clear();
									actor.CurrentDialogue.Push(new Dialogue(GetFestivalDataForYear(actor.Name + "_roommate"), actor));
								}
								else if (festivalData.ContainsKey(actor.Name + "_spouse"))
								{
									actor.CurrentDialogue.Clear();
									actor.CurrentDialogue.Push(new Dialogue(GetFestivalDataForYear(actor.Name + "_spouse"), actor));
								}
							}
							if (flag2)
							{
								actor.CurrentDialogue.Clear();
								actor.CurrentDialogue.Push(new Dialogue(Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\" + actor.Name)["divorced"], actor));
							}
							actor.grantConversationFriendship(who);
							Game1.drawDialogue(actor);
							actor.faceTowardFarmerForPeriod(3000, 2, faceAway: false, who);
							who.Halt();
							return true;
						}
					}
					if (festivalData != null && festivalData["file"].Equals("spring13"))
					{
						Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
						for (int num6 = festivalProps.Count - 1; num6 >= 0; num6--)
						{
							if (festivalProps[num6].isColliding(rectangle))
							{
								who.festivalScore++;
								festivalProps.RemoveAt(num6);
								who.team.FestivalPropsRemoved(rectangle);
								if (who.IsLocalPlayer)
								{
									Game1.playSound("coin");
								}
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		public void removeFestivalProps(Microsoft.Xna.Framework.Rectangle rect)
		{
			for (int num = festivalProps.Count - 1; num >= 0; num--)
			{
				if (festivalProps[num].isColliding(rect))
				{
					festivalProps.RemoveAt(num);
				}
			}
		}

		public void checkForSpecialCharacterIconAtThisTile(Vector2 tileLocation)
		{
			if (isFestival && festivalHost != null && festivalHost.getTileLocation().Equals(tileLocation))
			{
				Game1.mouseCursor = 4;
			}
		}

		public void forceEndFestival(Farmer who)
		{
			Game1.currentMinigame = null;
			Game1.exitActiveMenu();
			Game1.player.Halt();
			endBehaviors(null, Game1.currentLocation);
			if (Game1.IsServer)
			{
				Game1.multiplayer.sendServerToClientsMessage("endFest");
			}
			Game1.changeMusicTrack("none");
		}

		public bool checkForCollision(Microsoft.Xna.Framework.Rectangle position, Farmer who)
		{
			foreach (NPC actor in actors)
			{
				if (actor.GetBoundingBox().Intersects(position) && !farmer.temporarilyInvincible && farmer.TemporaryPassableTiles.IsEmpty() && !actor.IsInvisible && !who.GetBoundingBox().Intersects(actor.GetBoundingBox()) && !actor.farmerPassesThrough)
				{
					return true;
				}
			}
			int num = 32;
			int num2 = (isFestival ? (Math.Max(num, Game1.xEdge) + num) : num);
			int num3 = Game1.currentLocation.map.Layers[0].DisplayWidth - num;
			int num4 = num;
			int num5 = Game1.currentLocation.map.Layers[0].DisplayHeight - num;
			if ((Game1.currentMinigame == null || !(Game1.currentMinigame is FishingGame)) && (((position.X < num2 || position.X >= num3 || position.Y < num4 || position.Y >= num5) && who.IsLocalPlayer && isFestival) || position.X < 0 || position.Y < 0 || position.X >= Game1.currentLocation.map.Layers[0].DisplayWidth || position.Y >= Game1.currentLocation.map.Layers[0].DisplayHeight))
			{
				if (who.IsLocalPlayer && isFestival)
				{
					who.Halt();
					who.Position = who.lastPosition;
					Game1.currentLocation.tapToMove.Reset();
					if (!Game1.IsMultiplayer && Game1.activeClickableMenu == null)
					{
						Game1.activeClickableMenu = new ConfirmationDialog(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1758", FestivalName), forceEndFestival);
					}
					else if (Game1.activeClickableMenu == null)
					{
						Game1.player.team.SetLocalReady("festivalEnd", ready: true);
						Game1.activeClickableMenu = new ReadyCheckDialog("festivalEnd", allowCancel: true, forceEndFestival);
					}
				}
				return true;
			}
			foreach (Object prop in props)
			{
				if (prop.getBoundingBox(prop.tileLocation).Intersects(position))
				{
					return true;
				}
			}
			if (temporaryLocation != null)
			{
				foreach (Object value in temporaryLocation.objects.Values)
				{
					if (value.getBoundingBox(value.tileLocation).Intersects(position))
					{
						return true;
					}
				}
			}
			foreach (Prop festivalProp in festivalProps)
			{
				if (festivalProp.isColliding(position))
				{
					return true;
				}
			}
			return false;
		}

		public void answerDialogue(string questionKey, int answerChoice)
		{
			previousAnswerChoice = answerChoice;
			if (questionKey.Contains("fork"))
			{
				int num = Convert.ToInt32(questionKey.Replace("fork", ""));
				if (answerChoice == num)
				{
					specialEventVariable1 = !specialEventVariable1;
				}
			}
			else if (questionKey.Contains("quickQuestion"))
			{
				string text = eventCommands[Math.Min(eventCommands.Length - 1, CurrentCommand)];
				string[] array = text.Substring(text.IndexOf(' ') + 1).Split(new string[1] { "(break)" }, StringSplitOptions.None);
				string[] collection = array[1 + answerChoice].Split('\\');
				List<string> list = eventCommands.ToList();
				list.InsertRange(CurrentCommand + 1, collection);
				eventCommands = list.ToArray();
			}
			else
			{
				if (questionKey == null)
				{
					return;
				}
				switch (questionKey.Length)
				{
				case 11:
					switch (questionKey[1])
					{
					case 'h':
						if (questionKey == "shaneCliffs")
						{
							switch (answerChoice)
							{
							case 0:
								eventCommands[currentCommand + 2] = "speak Shane \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1760") + "\"";
								break;
							case 1:
								eventCommands[currentCommand + 2] = "speak Shane \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1761") + "\"";
								break;
							case 2:
								eventCommands[currentCommand + 2] = "speak Shane \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1763") + "\"";
								break;
							case 3:
								eventCommands[currentCommand + 2] = "speak Shane \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1764") + "\"";
								break;
							}
						}
						break;
					case 'i':
						if (questionKey == "fishingGame")
						{
							if (answerChoice == 0 && Game1.player.Money >= 50)
							{
								Game1.globalFadeToBlack(FishingGame.startMe, 0.01f);
								Game1.player.Money -= 50;
							}
							else if (answerChoice == 0 && Game1.player.Money < 50)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1780"));
							}
						}
						break;
					case 'e':
						if (questionKey == "secretSanta" && answerChoice == 0)
						{
							Game1.activeClickableMenu = new ItemGrabMenu(null, reverseGrab: true, showReceivingMenu: true, Utility.highlightSantaObjects, null, Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1788", secretSantaRecipient.displayName), null, snapToBottom: false, canBeExitedWithKey: false, playRightClickSound: false, allowRightClick: false, showOrganizeButton: false, 0, null, -1, null, 1, 1, null, allowStack: false, chooseSecretSantaGift, rearrangeGrangeOnExit: false, onCloseSantaInventory);
						}
						break;
					case 'f':
					case 'g':
						break;
					}
					break;
				case 13:
					switch (questionKey[0])
					{
					case 'h':
						if (questionKey == "haleyDarkRoom")
						{
							switch (answerChoice)
							{
							case 0:
								specialEventVariable1 = true;
								eventCommands[currentCommand + 1] = "fork decorate";
								break;
							case 1:
								specialEventVariable1 = true;
								eventCommands[currentCommand + 1] = "fork leave";
								break;
							case 2:
								break;
							}
						}
						break;
					case 'S':
						if (questionKey == "StarTokenShop" && answerChoice == 0)
						{
							Game1.activeClickableMenu = new NumberSelectionMenu(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1774"), buyStarTokens, 50, 0, 999);
						}
						break;
					case 'f':
						if (questionKey == "fortuneTeller" && answerChoice == 0)
						{
							Game1.globalFadeToBlack(readFortune);
							Game1.player.Money -= 100;
							Game1.player.mailReceived.Add("fortuneTeller" + Game1.year);
						}
						break;
					case 's':
						if (!(questionKey == "slingshotGame"))
						{
							if (questionKey == "starTokenShop" && answerChoice == 0)
							{
								if (festivalShops["starTokenShop"].Count == 0)
								{
									Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1785")));
								}
								else
								{
									Game1.activeClickableMenu = new ShopMenu(festivalShops["starTokenShop"], 1, null, null, null, "StardewFair");
								}
							}
						}
						else if (answerChoice == 0 && Game1.player.Money >= 50)
						{
							Game1.globalFadeToBlack(TargetGame.startMe, 0.01f);
							Game1.player.Money -= 50;
						}
						else if (answerChoice == 0 && Game1.player.Money < 50)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1780"));
						}
						break;
					}
					break;
				case 8:
					switch (questionKey[0])
					{
					case 'b':
						if (questionKey == "bandFork")
						{
							switch (answerChoice)
							{
							case 76:
								specialEventVariable1 = true;
								eventCommands[currentCommand + 1] = "fork poppy";
								break;
							case 77:
								specialEventVariable1 = true;
								eventCommands[currentCommand + 1] = "fork heavy";
								break;
							case 78:
								specialEventVariable1 = true;
								eventCommands[currentCommand + 1] = "fork techno";
								break;
							case 79:
								specialEventVariable1 = true;
								eventCommands[currentCommand + 1] = "fork honkytonk";
								break;
							}
						}
						break;
					case 'w':
						if (questionKey == "wheelBet")
						{
							specialEventVariable2 = answerChoice == 1;
							if (answerChoice != 2)
							{
								Game1.activeClickableMenu = new NumberSelectionMenu(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1776"), betStarTokens, -1, 1, Game1.player.festivalScore, Math.Min(1, Game1.player.festivalScore));
							}
						}
						break;
					}
					break;
				case 9:
					if (questionKey == "shaneLoan")
					{
						if (answerChoice != 0)
						{
							_ = 1;
							break;
						}
						specialEventVariable1 = true;
						eventCommands[currentCommand + 1] = "fork giveShaneLoan";
						Game1.player.Money -= 3000;
					}
					break;
				case 15:
					if (questionKey == "chooseCharacter")
					{
						switch (answerChoice)
						{
						case 0:
							specialEventVariable1 = true;
							eventCommands[currentCommand + 1] = "fork warrior";
							break;
						case 1:
							specialEventVariable1 = true;
							eventCommands[currentCommand + 1] = "fork healer";
							break;
						case 2:
							break;
						}
					}
					break;
				case 4:
					if (questionKey == "cave")
					{
						if (answerChoice == 0)
						{
							Game1.MasterPlayer.caveChoice.Value = 2;
							(Game1.getLocationFromName("FarmCave") as FarmCave).setUpMushroomHouse();
						}
						else
						{
							Game1.MasterPlayer.caveChoice.Value = 1;
						}
					}
					break;
				case 3:
					if (questionKey == "pet")
					{
						if (answerChoice == 0)
						{
							Game1.activeClickableMenu = new NamingMenu(namePet, Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1236"), (!Game1.player.IsMale) ? (Game1.player.catPerson ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1796") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1797")) : (Game1.player.catPerson ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1794") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1795")));
							break;
						}
						Game1.player.mailReceived.Add("rejectedPet");
						eventCommands = new string[2];
						eventCommands[1] = "end";
						eventCommands[0] = "speak Marnie \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1798") + "\"";
						currentCommand = 0;
						eventSwitched = true;
						specialEventVariable1 = true;
					}
					break;
				case 5:
				case 6:
				case 7:
				case 10:
				case 12:
				case 14:
					break;
				}
			}
		}

		private void namePet(string name)
		{
			Pet pet = ((!Game1.player.catPerson) ? ((Pet)new Dog(68, 13, Game1.player.whichPetBreed)) : ((Pet)new Cat(68, 13, Game1.player.whichPetBreed)));
			pet.warpToFarmHouse(Game1.player);
			pet.Name = name;
			pet.displayName = pet.name;
			Game1.exitActiveMenu();
			CurrentCommand++;
		}

		public void onCloseSantaInventory(Item i, Farmer who)
		{
			if (i == null)
			{
				Game1.player.forceCanMove();
			}
			else
			{
				chooseSecretSantaGift(i, who);
			}
		}

		public void chooseSecretSantaGift(Item i, Farmer who)
		{
			if (i == null)
			{
				return;
			}
			if (i is Object)
			{
				if (i.Stack > 1)
				{
					i.Stack--;
					who.addItemToInventory(i);
				}
				Game1.exitActiveMenu();
				NPC actorByName = getActorByName(secretSantaRecipient.Name);
				actorByName.faceTowardFarmerForPeriod(15000, 5, faceAway: false, who);
				actorByName.receiveGift(i as Object, who, updateGiftLimitInfo: false, 5f, showResponse: false);
				actorByName.CurrentDialogue.Clear();
				if (LocalizedContentManager.CurrentLanguageCode != 0)
				{
					actorByName.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1801", i.DisplayName), actorByName));
				}
				else
				{
					actorByName.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1801", i.DisplayName, Lexicon.getProperArticleForWord(i.DisplayName)), actorByName));
				}
				Game1.drawDialogue(actorByName);
				secretSantaRecipient = null;
				startSecretSantaAfterDialogue = true;
				who.Halt();
				who.completelyStopAnimatingOrDoingAction();
				who.faceGeneralDirection(actorByName.Position, 0, opposite: false, useTileCalculations: false);
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1803"));
			}
		}

		public void perfectFishing()
		{
			if (isFestival && Game1.currentMinigame != null && festivalData["file"].Equals("fall16"))
			{
				(Game1.currentMinigame as FishingGame).perfections++;
			}
		}

		public void caughtFish(int whichFish, int size, Farmer who)
		{
			if (!isFestival)
			{
				return;
			}
			if (whichFish != -1 && Game1.currentMinigame != null && festivalData["file"].Equals("fall16"))
			{
				(Game1.currentMinigame as FishingGame).score += ((size <= 0) ? 1 : (size + 5));
				if (size > 0)
				{
					(Game1.currentMinigame as FishingGame).fishCaught++;
				}
				Game1.player.FarmerSprite.PauseForSingleAnimation = false;
				Game1.player.FarmerSprite.StopAnimation();
			}
			else if (whichFish != -1 && festivalData["file"].Equals("winter8"))
			{
				if (size > 0 && who.getTileX() < 79 && who.getTileY() < 43)
				{
					who.festivalScore++;
					Game1.playSound("newArtifact");
				}
				who.forceCanMove();
				if (previousFacingDirection != -1)
				{
					who.faceDirection(previousFacingDirection);
				}
			}
		}

		public void readFortune()
		{
			Game1.globalFade = true;
			Game1.fadeToBlackAlpha = 1f;
			NPC topRomanticInterest = Utility.getTopRomanticInterest(Game1.player);
			NPC topNonRomanticInterest = Utility.getTopNonRomanticInterest(Game1.player);
			int highestSkill = Utility.getHighestSkill(Game1.player);
			string[] array = new string[5];
			if (topNonRomanticInterest != null && Game1.player.getFriendshipLevelForNPC(topNonRomanticInterest.Name) > 100)
			{
				int numberOfFriendsWithinThisRange = Utility.getNumberOfFriendsWithinThisRange(Game1.player, Game1.player.getFriendshipLevelForNPC(topNonRomanticInterest.Name) - 100, Game1.player.getFriendshipLevelForNPC(topNonRomanticInterest.Name));
				if (numberOfFriendsWithinThisRange > 3 && Game1.random.NextDouble() < 0.5)
				{
					array[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1810");
				}
				else
				{
					switch (Game1.random.Next(4))
					{
					case 0:
						array[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1811", topNonRomanticInterest.displayName);
						break;
					case 1:
						array[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1813", topNonRomanticInterest.displayName) + ((topNonRomanticInterest.Gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1815") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1816"));
						break;
					case 2:
						array[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1818", topNonRomanticInterest.displayName);
						break;
					case 3:
						array[0] = ((topNonRomanticInterest.Gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1820") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1821")) + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1823", topNonRomanticInterest.displayName);
						break;
					}
				}
			}
			else
			{
				array[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1825");
			}
			if (topRomanticInterest != null && Game1.player.getFriendshipLevelForNPC(topRomanticInterest.Name) > 250)
			{
				int numberOfFriendsWithinThisRange2 = Utility.getNumberOfFriendsWithinThisRange(Game1.player, Game1.player.getFriendshipLevelForNPC(topRomanticInterest.Name) - 100, Game1.player.getFriendshipLevelForNPC(topRomanticInterest.Name), romanceOnly: true);
				if (numberOfFriendsWithinThisRange2 > 2)
				{
					array[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1826");
				}
				else
				{
					switch (Game1.random.Next(4))
					{
					case 0:
						array[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1827", topRomanticInterest.displayName);
						break;
					case 1:
						array[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1827", topRomanticInterest.displayName);
						break;
					case 2:
						array[1] = ((topRomanticInterest.Gender != 0) ? ((topRomanticInterest.SocialAnxiety == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1833") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1834")) : ((topRomanticInterest.SocialAnxiety == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1831") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1832"))) + " " + ((topRomanticInterest.Gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1837", topRomanticInterest.displayName[0]) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1838", topRomanticInterest.displayName[0]));
						break;
					case 3:
						array[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1843", topRomanticInterest.displayName);
						break;
					}
				}
			}
			else
			{
				array[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1845");
			}
			switch (highestSkill)
			{
			case 0:
				array[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1846");
				break;
			case 3:
				array[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1847");
				break;
			case 4:
				array[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1848");
				break;
			case 1:
				array[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1849");
				break;
			case 2:
				array[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1850");
				break;
			case 5:
				array[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1851");
				break;
			}
			array[3] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1852");
			array[4] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1853");
			Game1.multipleDialogues(array);
			Game1.afterDialogues = fadeClearAndviewportUnfreeze;
			Game1.viewportFreeze = true;
			Game1.viewport.X = -9999;
		}

		public void fadeClearAndviewportUnfreeze()
		{
			Game1.fadeClear();
			Game1.viewportFreeze = false;
		}

		public void betStarTokens(int value, int price, Farmer who)
		{
			if (value <= who.festivalScore)
			{
				Game1.playSound("smallSelect");
				Game1.activeClickableMenu = new WheelSpinGame(value);
			}
		}

		public void buyStarTokens(int value, int price, Farmer who)
		{
			if (value > 0 && value * price <= who.Money)
			{
				who.Money -= price * value;
				who.festivalScore += value;
				Game1.playSound("purchase");
				Game1.exitActiveMenu();
			}
		}

		public void clickToAddItemToLuauSoup(Item i, Farmer who)
		{
			addItemToLuauSoup(i, who);
		}

		public void setUpAdvancedMove(string[] split, NPCController.endBehavior endBehavior = null)
		{
			if (npcControllers == null)
			{
				npcControllers = new List<NPCController>();
			}
			List<Vector2> list = new List<Vector2>();
			for (int i = 3; i < split.Length; i += 2)
			{
				list.Add(new Vector2(Convert.ToInt32(split[i]), Convert.ToInt32(split[i + 1])));
			}
			if (split[1].Contains("farmer"))
			{
				Farmer farmerFromFarmerNumberString = getFarmerFromFarmerNumberString(split[1], farmer);
				npcControllers.Add(new NPCController(farmerFromFarmerNumberString, list, Convert.ToBoolean(split[2]), endBehavior));
				return;
			}
			NPC actorByName = getActorByName(split[1].Replace('_', ' '));
			if (actorByName != null)
			{
				npcControllers.Add(new NPCController(actorByName, list, Convert.ToBoolean(split[2]), endBehavior));
			}
		}

		public static bool IsItemMayorShorts(Item i)
		{
			if (!Utility.IsNormalObjectAtParentSheetIndex(i, 789))
			{
				return Utility.IsNormalObjectAtParentSheetIndex(i, 71);
			}
			return true;
		}

		public void addItemToLuauSoup(Item i, Farmer who)
		{
			if (i == null)
			{
				return;
			}
			who.team.luauIngredients.Add(i.getOne());
			if (who.IsLocalPlayer)
			{
				specialEventVariable2 = true;
				bool flag = IsItemMayorShorts(i);
				if (i != null && i.Stack > 1 && !flag)
				{
					i.Stack--;
					who.addItemToInventory(i);
				}
				else if (flag)
				{
					who.addItemToInventory(i);
				}
				Game1.exitActiveMenu();
				Game1.playSound("dropItemInWater");
				if (i != null)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1857", i.DisplayName));
				}
				string text = "";
				if (i is Object)
				{
					text = (((i as Object).Quality == 1) ? " ([51])" : (((i as Object).Quality == 2) ? " ([52])" : (((i as Object).Quality == 4) ? " ([53])" : "")));
				}
				Game1.multiplayer.globalChatInfoMessage("LuauSoup", Game1.player.Name, i.DisplayName + text);
			}
		}

		private void governorTaste()
		{
			int num = 5;
			foreach (Item luauIngredient in Game1.player.team.luauIngredients)
			{
				Object @object = luauIngredient as Object;
				int num2 = 5;
				if (IsItemMayorShorts(@object))
				{
					num = 6;
					break;
				}
				if ((@object.Quality >= 2 && (int)@object.price >= 160) || (@object.Quality == 1 && (int)@object.price >= 300 && (int)@object.edibility > 10))
				{
					num2 = 4;
					Utility.improveFriendshipWithEveryoneInRegion(Game1.player, 120, 2);
				}
				else if ((int)@object.edibility >= 20 || (int)@object.price >= 100 || ((int)@object.price >= 70 && @object.Quality >= 1))
				{
					num2 = 3;
					Utility.improveFriendshipWithEveryoneInRegion(Game1.player, 60, 2);
				}
				else if (((int)@object.price > 20 && (int)@object.edibility >= 10) || ((int)@object.price >= 40 && (int)@object.edibility >= 5))
				{
					num2 = 2;
				}
				else if ((int)@object.edibility >= 0)
				{
					num2 = 1;
					Utility.improveFriendshipWithEveryoneInRegion(Game1.player, -50, 2);
				}
				if ((int)@object.edibility > -300 && (int)@object.edibility < 0)
				{
					num2 = 0;
					Utility.improveFriendshipWithEveryoneInRegion(Game1.player, -100, 2);
				}
				if (num2 < num)
				{
					num = num2;
				}
			}
			if (num != 6 && Game1.player.team.luauIngredients.Count < Game1.numberOfPlayers())
			{
				num = 5;
			}
			eventCommands[CurrentCommand + 1] = "switchEvent governorReaction" + num;
		}

		private void eggHuntWinner()
		{
			int num = 12;
			switch (Game1.numberOfPlayers())
			{
			case 1:
				num = 9;
				break;
			case 2:
				num = 6;
				break;
			case 3:
				num = 5;
				break;
			case 4:
				num = 4;
				break;
			}
			List<Farmer> list = new List<Farmer>();
			Farmer player = Game1.player;
			int festivalScore = Game1.player.festivalScore;
			foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
			{
				if (onlineFarmer.festivalScore > festivalScore)
				{
					player = onlineFarmer;
					festivalScore = onlineFarmer.festivalScore;
				}
			}
			foreach (Farmer onlineFarmer2 in Game1.getOnlineFarmers())
			{
				if (onlineFarmer2.festivalScore == festivalScore)
				{
					list.Add(onlineFarmer2);
					festivalWinners.Add(onlineFarmer2.UniqueMultiplayerID);
				}
			}
			string masterDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1862");
			if (festivalScore >= num)
			{
				if (list.Count == 1)
				{
					masterDialogue = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es) ? ("¡" + list[0].displayName + "!") : (list[0].displayName + "!"));
				}
				else
				{
					masterDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1864");
					for (int i = 0; i < list.Count; i++)
					{
						if (i == list.Count() - 1)
						{
							masterDialogue += Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1865");
						}
						masterDialogue = masterDialogue + " " + list[i].displayName;
						if (i < list.Count - 1)
						{
							masterDialogue += ",";
						}
					}
					masterDialogue += "!";
				}
				specialEventVariable1 = false;
			}
			else
			{
				specialEventVariable1 = true;
			}
			getActorByName("Lewis").CurrentDialogue.Push(new Dialogue(masterDialogue, getActorByName("Lewis")));
			Game1.drawDialogue(getActorByName("Lewis"));
		}

		private void iceFishingWinner()
		{
			int num = 5;
			winners = new List<Farmer>();
			Farmer player = Game1.player;
			int festivalScore = Game1.player.festivalScore;
			for (int i = 1; i <= Game1.numberOfPlayers(); i++)
			{
				Farmer farmerFromFarmerNumber = Utility.getFarmerFromFarmerNumber(i);
				if (farmerFromFarmerNumber != null && farmerFromFarmerNumber.festivalScore > festivalScore)
				{
					player = farmerFromFarmerNumber;
					festivalScore = farmerFromFarmerNumber.festivalScore;
				}
			}
			for (int j = 1; j <= Game1.numberOfPlayers(); j++)
			{
				Farmer farmerFromFarmerNumber2 = Utility.getFarmerFromFarmerNumber(j);
				if (farmerFromFarmerNumber2 != null && farmerFromFarmerNumber2.festivalScore == festivalScore)
				{
					winners.Add(farmerFromFarmerNumber2);
					festivalWinners.Add(farmerFromFarmerNumber2.UniqueMultiplayerID);
				}
			}
			string masterDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1871");
			if (festivalScore >= num)
			{
				if (winners.Count == 1)
				{
					masterDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1872", winners[0].displayName, winners[0].festivalScore);
				}
				else
				{
					masterDialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1864");
					for (int k = 0; k < winners.Count; k++)
					{
						if (k == winners.Count() - 1)
						{
							masterDialogue += Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1865");
						}
						masterDialogue = masterDialogue + " " + winners[k].displayName;
						if (k < winners.Count - 1)
						{
							masterDialogue += ",";
						}
					}
					masterDialogue += "!";
				}
				specialEventVariable1 = false;
			}
			else
			{
				specialEventVariable1 = true;
			}
			getActorByName("Lewis").CurrentDialogue.Push(new Dialogue(masterDialogue, getActorByName("Lewis")));
			Game1.drawDialogue(getActorByName("Lewis"));
		}

		private void iceFishingWinnerMP()
		{
			specialEventVariable1 = !winners.Contains(Game1.player);
		}
	}
}
