using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Dimensions;
using xTile.Tiles;

namespace StardewValley.Mobile
{
	public class TapToMove
	{
		private const int MAX_TRIES = 2;

		private const float MIN_DISTANCE = 8f;

		private const int MAX_STUCK_COUNT = 4;

		private const int MAX_REALLY_STUCK_COUNT = 2;

		private const float MIN_MONSTER_RANGE = 96f;

		private const int TICKS_BEFORE_TAP_HOLD_KICKS_IN = 3500000;

		private const int TICKS_BEFORE_REENABLING_MONSTER_CHECK = 5000000;

		private const bool loggingEnabled = true;

		private List<int> _actionableObjectIDs = new List<int>(new int[42]
		{
			286, 287, 288, 298, 322, 323, 324, 325, 599, 621,
			645, 405, 407, 409, 411, 415, 309, 310, 311, 313,
			314, 315, 316, 317, 318, 319, 320, 321, 328, 329,
			331, 401, 93, 94, 294, 295, 297, 461, 463, 464,
			746, 326
		});

		public MobileKeyStates mobileKeyStates = new MobileKeyStates();

		public int mouseCursor;

		public Vector2 nodeCenter = new Vector2(-1f, -1f);

		public Vector2 noPathHere = new Vector2(-1f, -1f);

		public bool preventMountingHorse;

		private bool _endNodeToBeActioned;

		private bool _endTileIsActionable;

		private bool _justUsedWeapon;

		private bool _performActionFromNeighbourTile;

		private bool _warping;

		private bool _enableCheckToAttackMonsters = true;

		private bool _pendingFurnitureAction;

		private Horse _tappedOnHorse;

		private bool _waterSourceAndFishingRodSelected;

		private bool _tappedOnCrop;

		private bool _tapHoldActive;

		private bool _tapPressed;

		private bool _justClosedActiveMenu;

		private bool _waitingToFinishWatering;

		private bool _tappedCinemaTicketBooth;

		private bool _tappedCinemaDoor;

		private bool _wasJustTouchingVirtualJoystick;

		private bool _tappedHaleyBracelet;

		private AStarGraph _aStarGraph;

		private AStarPath _aStarPath;

		private AStarNode _startNode;

		private AStarNode _nodeClicked;

		private AStarNode _endNodeOccupied;

		private AStarNode _farmerNode;

		private AStarNode _finalNode;

		private AStarNode _gateNode;

		private Fence _gateClickedOn;

		private Vector2 _clickPoint = new Vector2(-1f, -1f);

		private Vector2 _tileClicked = new Vector2(-1f, -1f);

		private float _lastDistance;

		private int _stuckCount;

		private int _reallyStuckCount;

		private Monster _monsterTarget;

		private TapToMovePhase _phase;

		private int _mouseX = -1;

		private int _mouseY = -1;

		private int _viewportX = -1;

		private int _viewportY = -1;

		private int _tryCount;

		private NPC _targetNPC;

		private FarmAnimal _targetFarmAnimal;

		private ResourceClump _forestLog;

		private GameLocation gameLocation;

		private DistanceToTarget _distanceToTarget;

		public static long startTime = 9223372036854775807L;

		private long _monsterCheckStartTime;

		private WalkDirection _walkDirectionFarmerToFinger;

		private Furniture _furniture;

		private Furniture _rotatingFurniture;

		private CrabPot _crabPot;

		private Object _forageItem;

		private Building _actionableBuilding;

		private bool _preSlingshotJoypadMode;

		private List<TapQueueItem> _tapQueueItemList = new List<TapQueueItem>();

		private bool _buttonAPressed;

		private int _nextDirection;

		private WalkDirection _nextWalkDirection;

		private List<int> _lastToolIndexList = new List<int>();

		private string _toolToSelect;

		public static MeleeWeapon mostRecentlyChosenMeleeWeapon;

		public Vector2 grabTile = Vector2.Zero;

		public Furniture furniture => _furniture;

		public AStarGraph aStarGraph => _aStarGraph;

		public Vector2 tapLocation => new Vector2(_mouseX, _mouseY);

		public Vector2 viewportLocation => new Vector2(_viewportX, _viewportY);

		public bool Moving => _phase > TapToMovePhase.None;

		public bool TapHoldActive => _tapHoldActive;

		public Vector2 ClickPoint => _clickPoint;

		public Vector2 TileClicked => _tileClicked;

		public NPC targetNPC => _targetNPC;

		public FarmAnimal targetFarmAnimal => _targetFarmAnimal;

		public bool Warping => _warping;

		public static bool isTapToMoveWeaponControl()
		{
			WeaponControl weaponControl = (WeaponControl)Game1.options.weaponControl;
			if ((uint)weaponControl <= 1u || (uint)(weaponControl - 4) <= 3u)
			{
				return true;
			}
			return false;
		}

		public TapToMove(GameLocation gameLocation)
		{
			this.gameLocation = gameLocation;
			Init(gameLocation);
		}

		public void Init(GameLocation gameLocation)
		{
			_aStarGraph = new AStarGraph();
			_aStarGraph.Init(gameLocation);
		}

		private void LogIt(string s)
		{
		}

		public void test()
		{
			AStarNode aStarNode = _aStarGraph.FetchAStarNode(60, 18);
			aStarNode.DebugTileClear();
		}

		public void Reset(bool resetMobileKeyStates = true)
		{
			_phase = TapToMovePhase.None;
			_warping = false;
			_mouseX = -1;
			_mouseY = -1;
			_viewportX = -1;
			_viewportY = -1;
			if (_nodeClicked != null)
			{
				_nodeClicked.FakeTileClear = false;
			}
			_nodeClicked = null;
			_endNodeOccupied = null;
			_gateNode = null;
			_gateClickedOn = null;
			nodeCenter = new Vector2(-1f, -1f);
			_clickPoint = new Vector2(-1f, -1f);
			_tileClicked = new Vector2(-1f, -1f);
			_stuckCount = 0;
			_reallyStuckCount = 0;
			_lastDistance = 0f;
			if (resetMobileKeyStates)
			{
				mobileKeyStates.Reset();
			}
			_endNodeToBeActioned = false;
			_endTileIsActionable = false;
			_performActionFromNeighbourTile = false;
			_tappedCinemaTicketBooth = false;
			_tappedCinemaDoor = false;
			_actionableBuilding = null;
			_targetNPC = null;
			_targetFarmAnimal = null;
			_distanceToTarget = DistanceToTarget.InRange;
			_tappedOnHorse = null;
			_waterSourceAndFishingRodSelected = false;
			_justClosedActiveMenu = false;
			_crabPot = null;
			_forestLog = null;
			_forageItem = null;
			if (Game1.player.mount != null)
			{
				Game1.player.mount.checkActionEnabled = true;
			}
		}

		public void ResetRotatingFurniture()
		{
			_rotatingFurniture = null;
		}

		public void JoystickOverride()
		{
			_phase = TapToMovePhase.None;
			_tileClicked.X = (_tileClicked.Y = -1f);
			_targetNPC = null;
		}

		public void OnCloseActiveMenu()
		{
			_justClosedActiveMenu = true;
		}

		public void OnTapHeld(int mouseX, int mouseY, int viewportX, int viewportY)
		{
			if (PinchZoom.Instance.Pinching || Toolbar.toolbarPressed || Game1.options.weaponControl == 2 || Game1.options.weaponControl == 3 || Game1.options.weaponControl == 8 || (Game1.options.weaponControl == 1 && _justUsedWeapon))
			{
				return;
			}
			if (Game1.virtualJoypad.TouchingJoystickOrButton)
			{
				_wasJustTouchingVirtualJoystick = true;
			}
			else
			{
				if (_wasJustTouchingVirtualJoystick || _justClosedActiveMenu || Toolbar.toolbarPressed || TapToMoveUtils.inMiniGameWhereWeDontWantTaps || Game1.currentMinigame is FishingGame || DateTime.Now.Ticks - startTime < 3500000)
				{
					return;
				}
				_tapHoldActive = true;
				if ((TapToMoveUtils.IsMatureTreeStumpOrBoulderAt(_tileClicked) || _forestLog != null) && mobileKeyStates.realTapHeld && (Game1.player.CurrentTool is Axe || Game1.player.CurrentTool is Pickaxe) && _phase != TapToMovePhase.FollowingAStarPath && _phase != TapToMovePhase.OnFinalTile && _phase != TapToMovePhase.ReachedEndOfPath && _phase != TapToMovePhase.Complete)
				{
					if ((bool)Game1.player.usingTool)
					{
						_phase = TapToMovePhase.None;
						mobileKeyStates.SetUseTool(useTool: false);
						mobileKeyStates.StopMoving();
					}
					else
					{
						_phase = TapToMovePhase.JustDoAction1;
					}
				}
				else if (_waterSourceAndFishingRodSelected && mobileKeyStates.realTapHeld && Game1.player.CurrentTool is FishingRod)
				{
					if (_phase == TapToMovePhase.Complete)
					{
						_phase = TapToMovePhase.JustDoAction1;
					}
				}
				else if ((Game1.player.CurrentItem is Furniture || Game1.player.ActiveObject is Furniture) && Game1.currentLocation is DecoratableLocation)
				{
					mobileKeyStates.SetMovePressed(WalkDirection.None);
					_phase = TapToMovePhase.None;
				}
				else if (_furniture != null && DateTime.Now.Ticks - startTime > 3500000 && !TutorialManager.Instance.ShowingDialogueBox)
				{
					_phase = TapToMovePhase.JustDoAction1;
				}
				else
				{
					if (!Game1.player.canMove || _warping || TapToMoveUtils.IsMatureTreeStumpOrBoulderAt(_tileClicked) || _forestLog != null)
					{
						return;
					}
					if (_phase != 0 && _phase != TapToMovePhase.UsingJoyStick)
					{
						Reset();
					}
					if (_phase != TapToMovePhase.UsingJoyStick)
					{
						_phase = TapToMovePhase.UsingJoyStick;
						noPathHere.X = (noPathHere.Y = -1f);
						TutorialManager.Instance.CheckTapAndHold();
					}
					Vector2 vector = new Vector2(mouseX + viewportX, mouseY + viewportY);
					Vector2 playerOffsetPosition = TapToMoveUtils.PlayerOffsetPosition;
					bool flag = false;
					float num = 192f;
					if ((float)Game1.input.GetMouseState().X < (float)Game1.clientBounds.Width * 0.2f && TapToMoveUtils.PlayerPositionOnScreen.X < num)
					{
						playerOffsetPosition.X = (float)Game1.viewport.X + num;
						flag = true;
					}
					else if ((float)Game1.input.GetMouseState().X < (float)Game1.clientBounds.Width * 0.2f && TapToMoveUtils.PlayerPositionOnScreen.X > (float)Game1.viewport.Width - num)
					{
						playerOffsetPosition.X = (float)(Game1.viewport.X + Game1.viewport.Width) - num;
						flag = true;
					}
					if ((float)Game1.input.GetMouseState().Y < (float)Game1.clientBounds.Height * 0.2f && TapToMoveUtils.PlayerPositionOnScreen.Y < num)
					{
						playerOffsetPosition.Y = (float)Game1.viewport.Y + num;
						flag = true;
					}
					else if ((float)Game1.input.GetMouseState().Y > (float)Game1.clientBounds.Height * 0.8f && TapToMoveUtils.PlayerPositionOnScreen.Y > (float)Game1.viewport.Height - num)
					{
						playerOffsetPosition.Y = (float)(Game1.viewport.Y + Game1.viewport.Height) - num;
						flag = true;
					}
					float num2 = Vector2.Distance(TapToMoveUtils.PlayerOffsetPosition, vector);
					float num3 = 16f / Game1.options.zoomLevel;
					if (num2 > num3 || flag)
					{
						float num4 = (float)Math.Atan2(vector.Y - playerOffsetPosition.Y, vector.X - playerOffsetPosition.X);
						float angleDegrees = num4 / ((float)Math.PI * 2f) * 360f;
						if (num2 > 32f || flag)
						{
							_walkDirectionFarmerToFinger = TapToMoveUtils.WalkDirectionForAngle(angleDegrees);
						}
					}
					else
					{
						_walkDirectionFarmerToFinger = WalkDirection.None;
					}
					mobileKeyStates.SetMovePressed(_walkDirectionFarmerToFinger);
					if (!Game1.player.usingTool && !_warping && TapToMoveUtils.InWarpRange(TapToMoveUtils.PlayerOffsetPosition) && TapToMoveUtils.InWarpRange(vector) && (Game1.CurrentEvent == null || !Game1.CurrentEvent.isFestival))
					{
						TapToMoveUtils.WarpIfInRange(TapToMoveUtils.PlayerOffsetPosition);
						Reset();
						_warping = true;
					}
				}
			}
		}

		public void OnTapRelease(int mouseX = 0, int mouseY = 0, int viewportX = 0, int viewportY = 0)
		{
			_tapPressed = false;
			_tapHoldActive = false;
			if (PinchZoom.Instance.Pinching || Game1.options.weaponControl == 2 || Game1.options.weaponControl == 3 || Game1.options.weaponControl == 8)
			{
				return;
			}
			if (Game1.options.weaponControl == 1 && _justUsedWeapon)
			{
				if (Game1.player.UsingTool)
				{
					return;
				}
				_justUsedWeapon = false;
			}
			if (_justClosedActiveMenu || Toolbar.toolbarPressed)
			{
				_justClosedActiveMenu = false;
			}
			else
			{
				if (TapToMoveUtils.inMiniGameWhereWeDontWantTaps || Game1.virtualJoypad.TouchingJoystickOrButton)
				{
					return;
				}
				if (_wasJustTouchingVirtualJoystick)
				{
					_wasJustTouchingVirtualJoystick = false;
					return;
				}
				if (!(Game1.player.CurrentTool is FishingRod) && !(Game1.player.CurrentTool is Slingshot))
				{
					if (Game1.player.CanMove && Game1.player.UsingTool)
					{
						Farmer.canMoveNow(Game1.player);
					}
					mobileKeyStates.realTapHeld = false;
					mobileKeyStates.actionButtonPressed = false;
					mobileKeyStates.useToolButtonReleased = true;
				}
				if (Game1.player.ActiveObject is Furniture && Game1.currentLocation is DecoratableLocation)
				{
					Furniture furnitureClickedOn = TapToMoveUtils.GetFurnitureClickedOn(mouseX + viewportX, mouseY + viewportY);
					if (furnitureClickedOn != null && (int)furnitureClickedOn.furniture_type != 12)
					{
						furnitureClickedOn.performObjectDropInAction(Game1.player.ActiveObject, probe: false, Game1.player);
					}
					else
					{
						_phase = TapToMovePhase.JustDoAction1;
					}
				}
				else if (_pendingFurnitureAction)
				{
					_pendingFurnitureAction = false;
					if (_furniture != null && !TutorialManager.Instance.ShowingDialogueBox && ((int)_furniture.parentSheetIndex == 1308 || (int)_furniture.parentSheetIndex == 1226 || (int)_furniture.parentSheetIndex == 1402 || (int)_furniture.furniture_type == 14 || _furniture is StorageFurniture || _furniture is TV))
					{
						_phase = TapToMovePhase.JustDoRightClick1;
						return;
					}
					mobileKeyStates.actionButtonPressed = true;
					_phase = TapToMovePhase.Complete;
				}
				else if (Game1.player.CurrentTool != null && (int)Game1.player.CurrentTool.upgradeLevel > 0 && Game1.player.canReleaseTool && !(Game1.player.CurrentTool is FishingRod) && (_phase == TapToMovePhase.None || _phase == TapToMovePhase.PendingComplete || (bool)Game1.player.usingTool))
				{
					_phase = TapToMovePhase.JustDoAction1;
				}
				else if (Game1.player.CurrentTool is Slingshot && Game1.player.usingSlingshot)
				{
					_phase = TapToMovePhase.JustDoAction2;
					if (Game1.currentMinigame == null || !(Game1.currentMinigame is TargetGame))
					{
						Game1.virtualJoypad.showJoypad = _preSlingshotJoypadMode;
					}
				}
				else if (_phase == TapToMovePhase.PendingComplete || _phase == TapToMovePhase.UsingJoyStick)
				{
					Reset();
					CheckForQueuedReadyToHarvestTaps();
				}
			}
		}

		public void DoLeftClick()
		{
			_phase = TapToMovePhase.JustDoAction1;
		}

		public void DoRightClick()
		{
			_phase = TapToMovePhase.JustDoRightClick1;
		}

		public void MoveJoystickHeld(float angle)
		{
			WalkDirection movePressed = TapToMoveUtils.WalkDirectionForAngle(angle);
			mobileKeyStates.SetMovePressed(movePressed);
			_phase = TapToMovePhase.UsingJoyStick;
			_tileClicked.X = (_tileClicked.Y = -1f);
			_targetNPC = null;
		}

		public void StopMoving()
		{
			mobileKeyStates.StopMoving();
		}

		public void OnButtonAHeld(float angle)
		{
			if (_buttonAPressed)
			{
				int num = TapToMoveUtils.FaceDirectionForAngle(angle);
				if ((int)Game1.player.facingDirection != num && (_nextDirection != num || _phase != TapToMovePhase.AttackInNewDirection))
				{
					if (!Game1.player.CanMove && Game1.player.UsingTool && Game1.player.CurrentTool.isHeavyHitter())
					{
						Game1.player.Halt();
						_justUsedWeapon = false;
						mobileKeyStates.SetUseTool(useTool: false);
					}
					_nextWalkDirection = TapToMoveUtils.WalkDirectionForAngle(angle);
					_nextDirection = num;
					_phase = TapToMovePhase.AttackInNewDirection;
				}
			}
			else
			{
				_buttonAPressed = true;
				if (!_justUsedWeapon)
				{
					Reset();
					_justUsedWeapon = true;
					_nextWalkDirection = TapToMoveUtils.WalkDirectionForAngle(angle);
					_nextDirection = TapToMoveUtils.FaceDirectionForAngle(angle);
					_phase = TapToMovePhase.AttackInNewDirection;
				}
			}
		}

		public void OnButtonARelease()
		{
			if (_buttonAPressed)
			{
				_buttonAPressed = false;
				_justUsedWeapon = false;
				mobileKeyStates.SetUseTool(useTool: false);
				Reset();
				_phase = TapToMovePhase.None;
			}
		}

		private bool AddToTapQueueItemList(int mouseX, int mouseY, int viewportX, int viewportY)
		{
			int num = (mouseX + viewportX) / 64;
			int num2 = (mouseY + viewportY) / 64;
			for (int i = 0; i < _tapQueueItemList.Count; i++)
			{
				if (_tapQueueItemList[i].tileX == num && _tapQueueItemList[i].tileY == num2)
				{
					return false;
				}
			}
			_tapQueueItemList.Add(new TapQueueItem(mouseX, mouseY, viewportX, viewportY, num, num2));
			return true;
		}

		public void OnTap(int mouseX, int mouseY, int viewportX, int viewportY, int tryCount = 0)
		{
			if (PinchZoom.Instance.Pinching)
			{
				return;
			}
			if (Game1.virtualJoypad.TouchingJoystickOrButton)
			{
				_wasJustTouchingVirtualJoystick = true;
				return;
			}
			try
			{
				if (Game1.options.weaponControl == 2 || Game1.options.weaponControl == 3 || Game1.options.weaponControl == 8)
				{
					if (!(Game1.player.CurrentTool is Slingshot) || !TappedOnFarmer(mouseX + viewportX, mouseY + viewportY))
					{
						return;
					}
					_preSlingshotJoypadMode = Game1.virtualJoypad.showJoypad;
					if (Game1.currentMinigame == null || !(Game1.currentMinigame is TargetGame))
					{
						Game1.virtualJoypad.showJoypad = false;
					}
				}
				if ((Toolbar.toolbarPressed && Toolbar.visible) || Game1.locationRequest != null || Game1.virtualJoypad.TouchingJoystickOrButton)
				{
					return;
				}
			}
			catch (Exception ex)
			{
				Log.It("TapToMove.OnTap A EXCEPTION -> " + ex.ToString());
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["block"] = "A";
				Reset();
				return;
			}
			_tapPressed = true;
			startTime = DateTime.Now.Ticks;
			Game1.virtualJoypad.mostRecentlyUsedControlType = ControlType.TAP;
			Point point = new Point(mouseX + viewportX, mouseY + viewportY);
			try
			{
				SetMouseCursor(_aStarGraph.FetchAStarNode(point.X / 64, point.Y / 64));
			}
			catch (Exception ex2)
			{
				Log.It("TapToMove.OnTap B EXCEPTION -> " + ex2.ToString());
				Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
				dictionary2["block"] = "B SetMouseCursor";
				Reset();
				return;
			}
			try
			{
				if (_tappedOnCrop)
				{
					if (TappedOnAnotherQueableCrop(point.X, point.Y))
					{
						if (AddToTapQueueItemList(mouseX, mouseY, viewportX, viewportY) && Game1.player.CurrentTool is WateringCan && (bool)Game1.player.usingTool && _phase == TapToMovePhase.None)
						{
							_waitingToFinishWatering = true;
						}
						return;
					}
					if (TappedOnHoeDirtAndHoldingSeed(point.X, point.Y))
					{
						AddToTapQueueItemList(mouseX, mouseY, viewportX, viewportY);
					}
					else
					{
						_tappedOnCrop = false;
						_tapQueueItemList.Clear();
					}
				}
			}
			catch (Exception ex3)
			{
				Log.It("TapToMove.OnTap C EXCEPTION -> " + ex3.ToString());
				Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
				dictionary3["block"] = "C tappedOnCrop";
				Reset();
				return;
			}
			try
			{
				if (Game1.options.weaponControl == 1 && _justUsedWeapon)
				{
					Game1.player.completelyStopAnimatingOrDoingAction();
					Reset();
					_monsterCheckStartTime = startTime + 5000000;
				}
				if (Game1.CurrentEvent != null && Game1.CurrentEvent.id == 0 && Game1.CurrentEvent.FestivalName == "")
				{
					Event currentEvent = Game1.CurrentEvent;
					return;
				}
				if (Game1.CurrentEvent != null && !Game1.CurrentEvent.playerControlSequence)
				{
					Event currentEvent2 = Game1.CurrentEvent;
					return;
				}
				if (Game1.CurrentEvent != null)
				{
					Event currentEvent3 = Game1.CurrentEvent;
				}
				if (Game1.options.weaponControl == 0 && !Game1.player.CanMove && Game1.player.UsingTool && Game1.player.CurrentTool != null && Game1.player.CurrentTool.isHeavyHitter())
				{
					Game1.player.Halt();
					_enableCheckToAttackMonsters = false;
					_justUsedWeapon = false;
					mobileKeyStates.SetUseTool(useTool: false);
				}
				if (Game1.dialogueUp || (Game1.activeClickableMenu != null && !(Game1.activeClickableMenu is AnimalQueryMenu) && !(Game1.activeClickableMenu is CarpenterMenu) && !(Game1.activeClickableMenu is PurchaseAnimalsMenu) && !(Game1.activeClickableMenu is MuseumMenu)) || TutorialManager.Instance.isInDialogBounds(mouseX, mouseY) || (Game1.player.CurrentTool is WateringCan && Game1.player.UsingTool) || TapToMoveUtils.inMiniGameWhereWeDontWantTaps || (Game1.player.ActiveObject != null && Game1.player.ActiveObject is Furniture && Game1.currentLocation is DecoratableLocation))
				{
					return;
				}
			}
			catch (Exception ex4)
			{
				Log.It("TapToMove.OnTap D EXCEPTION -> " + ex4.ToString());
				Dictionary<string, string> dictionary4 = new Dictionary<string, string>();
				dictionary4["block"] = "D Return blocks";
				Reset();
				return;
			}
			try
			{
				if (Game1.currentMinigame == null && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival && Game1.CurrentEvent.FestivalName == "Stardew Valley Fair")
				{
					Game1.player.CurrentToolIndex = -1;
				}
				if (!Game1.player.CanMove && (Game1.eventUp || (Game1.currentLocation is FarmHouse && Game1.dialogueUp)))
				{
					if (Game1.dialogueUp)
					{
						Reset();
						_phase = TapToMovePhase.JustDoRightClick1;
					}
					else if ((Game1.currentSeason == "winter" && Game1.dayOfMonth == 8) || Game1.currentMinigame is FishingGame)
					{
						_phase = TapToMovePhase.JustDoAction1;
					}
					if (!(Game1.player.CurrentTool is FishingRod))
					{
						return;
					}
				}
				if (Game1.player.CurrentTool is Slingshot && (Game1.currentMinigame == null || !(Game1.currentMinigame is TargetGame)) && TappedOnFarmer(point.X, point.Y))
				{
					mobileKeyStates.SetUseTool(useTool: true);
					mobileKeyStates.realTapHeld = true;
					_phase = TapToMovePhase.TapHeld;
					return;
				}
				if (Game1.player.CurrentTool is Wand && TappedOnFarmer(point.X, point.Y))
				{
					_phase = TapToMovePhase.JustDoAction1;
					return;
				}
				if (!Game1.player.CanMove && Game1.player.CurrentTool is FishingRod)
				{
					_phase = TapToMovePhase.JustDoAction1;
					return;
				}
				if (CheckToEatFood(point.X, point.Y))
				{
					return;
				}
			}
			catch (Exception ex5)
			{
				Log.It("TapToMove.OnTap E EXCEPTION -> " + ex5.ToString());
				Dictionary<string, string> dictionary5 = new Dictionary<string, string>();
				dictionary5["block"] = "E Return blocks";
				Reset();
				return;
			}
			TutorialManager.Instance.CheckHasMovedYet();
			if (tryCount >= 2)
			{
				Reset();
				Game1.player.Halt();
				return;
			}
			try
			{
				Reset(resetMobileKeyStates: false);
				noPathHere.X = (noPathHere.Y = -1f);
				mobileKeyStates.ResetLeftOrRightClickButtons();
				_mouseX = mouseX;
				_mouseY = mouseY;
				_viewportX = viewportX;
				_viewportY = viewportY;
				_tryCount = tryCount;
				mobileKeyStates.realTapHeld = true;
				_clickPoint = new Vector2(point.X, point.Y);
				_tileClicked = new Vector2(point.X / 64, point.Y / 64);
				if (Game1.currentLocation is DecoratableLocation)
				{
					Point point2 = TapToMoveUtils.retargetToBedSpot(_aStarGraph, new Point((int)_tileClicked.X, (int)_tileClicked.Y));
					if (point2.X != (int)_tileClicked.X || point2.Y != (int)_tileClicked.Y)
					{
						_tileClicked.X = point2.X;
						_tileClicked.Y = point2.Y;
					}
				}
				else if (Game1.currentLocation is IslandLocation)
				{
					Point point3 = TapToMoveUtils.retargetToParrotExpressSpot(_aStarGraph, new Point((int)_tileClicked.X, (int)_tileClicked.Y));
					if (point3.X != (int)_tileClicked.X || point3.Y != (int)_tileClicked.Y)
					{
						_tileClicked.X = point3.X;
						_tileClicked.Y = point3.Y;
					}
				}
			}
			catch (Exception ex6)
			{
				Log.It("TapToMove.OnTap F EXCEPTION -> " + ex6.ToString());
				Dictionary<string, string> dictionary6 = new Dictionary<string, string>();
				dictionary6["block"] = "F";
				Reset();
				return;
			}
			try
			{
				if (_tileClicked.X == 37f && _tileClicked.Y == 79f && gameLocation is Town && Game1.CurrentEvent != null && Game1.CurrentEvent.FestivalName == "Stardew Valley Fair")
				{
					_tileClicked.Y = 80f;
				}
				if (Game1.player.isRidingHorse() && (Game1.player.mount.GetAlternativeBoundingBox().Contains(point.X, point.Y) || TappedOnFarmer(point.X, point.Y)) && Game1.player.mount.checkAction(Game1.player, gameLocation))
				{
					Reset();
					return;
				}
				if (Game1.player.IsSitting())
				{
					Game1.player.StopSitting();
					Reset();
					return;
				}
				if (holdingWallpaperAndTileClickedIsWallOrFloor())
				{
					mobileKeyStates.actionButtonPressed = true;
					return;
				}
				if (Game1.mailbox.Count > 0 && Game1.player.ActiveObject == null && Game1.currentLocation is Farm && _tileClicked.X == 68f && _tileClicked.Y == 14f)
				{
					OnTap(mouseX, mouseY, viewportX, viewportY + 64, tryCount);
					return;
				}
				if (mouseCursor == 5)
				{
					if (!TapToMoveUtils.TappedEggAtEggFestival(_clickPoint))
					{
						if (!Game1.currentLocation.checkAction(new Location((int)_tileClicked.X, (int)_tileClicked.Y), Game1.viewport, Game1.player))
						{
							Game1.currentLocation.checkAction(new Location((int)_tileClicked.X, (int)_tileClicked.Y + 1), Game1.viewport, Game1.player);
						}
						Reset();
						Game1.player.Halt();
						return;
					}
				}
				else if (Game1.currentLocation is Town && Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater"))
				{
					if (_tileClicked.X >= 48f && _tileClicked.X <= 51f && (_tileClicked.Y == 18f || _tileClicked.Y == 19f))
					{
						Game1.currentLocation.checkAction(new Location((int)_tileClicked.X, 19), Game1.viewport, Game1.player);
						Reset();
						return;
					}
				}
				else if (Game1.currentLocation is Beach && !((Beach)Game1.currentLocation).bridgeFixed && (_tileClicked.X == 58f || _tileClicked.X == 59f) && (_tileClicked.Y == 11f || _tileClicked.Y == 12f))
				{
					Game1.currentLocation.checkAction(new Location(58, 13), Game1.viewport, Game1.player);
				}
				else if (Game1.currentLocation is LibraryMuseum && ((int)_tileClicked.X != 3 || (int)_tileClicked.Y != 9))
				{
					if (((LibraryMuseum)Game1.currentLocation).museumPieces.ContainsKey(new Vector2((int)_tileClicked.X, (int)_tileClicked.Y)))
					{
						if (Game1.currentLocation.checkAction(new Location((int)_tileClicked.X, (int)_tileClicked.Y), Game1.viewport, Game1.player))
						{
							return;
						}
					}
					else
					{
						for (int i = 0; i < 3; i++)
						{
							if (((LibraryMuseum)Game1.currentLocation).doesTileHaveProperty((int)_tileClicked.X, (int)_tileClicked.Y + i, "Action", "Buildings") != null && ((LibraryMuseum)Game1.currentLocation).doesTileHaveProperty((int)_tileClicked.X, (int)_tileClicked.Y + i, "Action", "Buildings").Contains("Notes") && Game1.currentLocation.checkAction(new Location((int)_tileClicked.X, (int)_tileClicked.Y + i), Game1.viewport, Game1.player))
							{
								return;
							}
						}
					}
				}
			}
			catch (Exception ex7)
			{
				Log.It("TapToMove.OnTap G EXCEPTION -> " + ex7.ToString());
				Dictionary<string, string> dictionary7 = new Dictionary<string, string>();
				dictionary7["block"] = "G";
				Reset();
				return;
			}
			if (!TileOnMap((int)_tileClicked.X, (int)_tileClicked.Y))
			{
				Reset();
				return;
			}
			try
			{
				_startNode = (_finalNode = _aStarGraph.FarmerAStarNodeOffset);
				_nodeClicked = _aStarGraph.FetchAStarNode((int)_tileClicked.X, (int)_tileClicked.Y);
				if (_nodeClicked == null)
				{
					Reset();
					return;
				}
			}
			catch (Exception ex8)
			{
				Log.It("TapToMove.OnTap H EXCEPTION -> " + ex8.ToString());
				Dictionary<string, string> dictionary8 = new Dictionary<string, string>();
				dictionary8["block"] = "H";
				Reset();
				return;
			}
			try
			{
				if (TapToMoveUtils.IsWater(_tileClicked) && mouseCursor != 2 && mouseCursor != 6 && !(Game1.player.CurrentTool is WateringCan) && !(Game1.player.CurrentTool is FishingRod) && (Game1.player.ActiveObject == null || Game1.player.ActiveObject.ParentSheetIndex != 710) && !_nodeClicked.isTilePassable() && !TapToMoveUtils.IsOreAt(_tileClicked) && !(gameLocation is BoatTunnel))
				{
					AStarNode aStarNode = TapToMoveUtils.CrabPotNeighbour(_nodeClicked);
					if (aStarNode == null)
					{
						Reset();
						return;
					}
					_crabPot = _nodeClicked.FetchObject() as CrabPot;
					_nodeClicked = aStarNode;
					_tileClicked.X = _nodeClicked.x;
					_tileClicked.Y = _nodeClicked.y;
				}
			}
			catch (Exception ex9)
			{
				Log.It("TapToMove.OnTap I EXCEPTION -> " + ex9.ToString());
				Dictionary<string, string> dictionary9 = new Dictionary<string, string>();
				dictionary9["block"] = "I open water";
				Reset();
				return;
			}
			if (_startNode == null || _nodeClicked == null)
			{
				return;
			}
			try
			{
				if (TapToMoveUtils.NodeContainsFurniture(_nodeClicked) && !TutorialManager.Instance.ShowingDialogueBox)
				{
					Furniture furnitureClickedOn = TapToMoveUtils.GetFurnitureClickedOn((int)_clickPoint.X, (int)_clickPoint.Y);
					if (furnitureClickedOn != null)
					{
						if (_rotatingFurniture == furnitureClickedOn && (int)furnitureClickedOn.rotations > 1)
						{
							if (!TutorialManager.Instance.ShowingDialogueBox)
							{
								_rotatingFurniture.rotate();
							}
							Reset();
							return;
						}
						if ((int)furnitureClickedOn.rotations > 1)
						{
							_rotatingFurniture = furnitureClickedOn;
						}
						else if (furnitureClickedOn != _rotatingFurniture)
						{
							_rotatingFurniture = null;
						}
					}
					_furniture = furnitureClickedOn;
					if (Game1.player.CurrentTool is FishingRod)
					{
						Game1.player.CurrentToolIndex = -1;
					}
				}
				else
				{
					_furniture = null;
					_rotatingFurniture = null;
				}
			}
			catch (Exception ex10)
			{
				Log.It("TapToMove.OnTap J EXCEPTION -> " + ex10.ToString());
				Dictionary<string, string> dictionary10 = new Dictionary<string, string>();
				dictionary10["block"] = "J Furniture";
				Reset();
				return;
			}
			if (_nodeClicked.ContainsSomeKindOfWarp() && Game1.player.CurrentTool is FishingRod)
			{
				Game1.player.CurrentToolIndex = -1;
			}
			try
			{
				if (EndNodeBlocked(_nodeClicked))
				{
					_endNodeOccupied = _nodeClicked;
					_nodeClicked.FakeTileClear = true;
				}
				else
				{
					_endNodeOccupied = null;
				}
				if (_tappedOnHorse != null && Game1.player.CurrentItem is Hat)
				{
					_tappedOnHorse.checkAction(Game1.player, Game1.currentLocation);
					Reset();
					return;
				}
				Game1.player.isRidingHorse();
				if (_targetNPC != null && Game1.CurrentEvent != null && Game1.CurrentEvent.playerControlSequenceID != null && Game1.CurrentEvent.festivalTimer > 0 && Game1.CurrentEvent.playerControlSequenceID == "iceFishing")
				{
					Reset();
					return;
				}
			}
			catch (Exception ex11)
			{
				Log.It("TapToMove.OnTap K EXCEPTION -> " + ex11.ToString());
				Dictionary<string, string> dictionary11 = new Dictionary<string, string>();
				dictionary11["block"] = "K";
				Reset();
				return;
			}
			try
			{
				if (!Game1.player.isRidingHorse() && Game1.player.mount == null && !_performActionFromNeighbourTile && !_endNodeToBeActioned)
				{
					for (int j = 0; j < gameLocation.characters.Count; j++)
					{
						if (gameLocation.characters[j] is Horse)
						{
							Horse horse = (Horse)gameLocation.characters[j];
							float num = Vector2.Distance(_clickPoint, Utility.PointToVector2(horse.GetBoundingBox().Center));
							if (num < 48f && (_tileClicked.X != horse.getTileLocation().X || _tileClicked.Y != horse.getTileLocation().Y))
							{
								Reset();
								OnTap((int)(horse.getTileLocation().X * 64f + 32f - (float)viewportX), (int)(horse.getTileLocation().Y * 64f + 32f - (float)viewportY), viewportX, viewportY);
								return;
							}
						}
					}
				}
			}
			catch (Exception ex12)
			{
				Log.It("TapToMove.OnTap L EXCEPTION -> " + ex12.ToString());
				Dictionary<string, string> dictionary12 = new Dictionary<string, string>();
				dictionary12["block"] = "L horse mount";
				Reset();
				return;
			}
			try
			{
				if (_nodeClicked != null && _endNodeOccupied != null && !_endNodeToBeActioned && !_performActionFromNeighbourTile && !_endTileIsActionable && !_nodeClicked.ContainsSomeKindOfWarp() && _nodeClicked.ContainsBuilding())
				{
					Building building = _nodeClicked.FetchBuilding();
					if (building != null && building.buildingType.Equals("Mill"))
					{
						if (Game1.player.ActiveObject != null && (int)Game1.player.ActiveObject.parentSheetIndex != 284 && (int)Game1.player.ActiveObject.parentSheetIndex != 262)
						{
						}
					}
					else
					{
						if (building != null && building.buildingType.Equals("Silo"))
						{
							building.doAction(new Vector2(_nodeClicked.x, _nodeClicked.y), Game1.player);
							return;
						}
						if (!TapToMoveUtils.IsTreeAt(_nodeClicked) && _actionableBuilding == null && (!(Game1.currentLocation is Farm) || _nodeClicked.x != 21 || _nodeClicked.y != 25 || Game1.whichFarm != 3))
						{
							Reset();
							return;
						}
					}
				}
			}
			catch (Exception ex13)
			{
				Log.It("TapToMove.OnTap M EXCEPTION -> " + ex13.ToString());
				Dictionary<string, string> dictionary13 = new Dictionary<string, string>();
				dictionary13["block"] = "M building";
				Reset();
				return;
			}
			if (_nodeClicked.ContainsCinema() && !_tappedCinemaTicketBooth && !_tappedCinemaDoor)
			{
				noPathHere = _tileClicked;
				Reset();
				return;
			}
			if ((_aStarGraph.IsNeighbouringNodeNoDiagonals(_startNode, _nodeClicked) && _endNodeOccupied != null) || (_aStarGraph.IsNeighbouringNode(_startNode, _nodeClicked) && _endNodeOccupied != null && Game1.player.CurrentTool != null && (Game1.player.CurrentTool is WateringCan || Game1.player.CurrentTool is Hoe || Game1.player.CurrentTool is MeleeWeapon)))
			{
				_phase = TapToMovePhase.OnFinalTile;
				return;
			}
			if (_aStarGraph.IsNeighbouringNodeOnDiagonal(_startNode, _nodeClicked) && _endNodeOccupied != null && _performActionFromNeighbourTile)
			{
				_phase = TapToMovePhase.OnFinalTile;
				return;
			}
			try
			{
				Log.It("TapToMove.OnTap startNode(" + _startNode.x + "," + _startNode.y + ") nodeClicked(" + _nodeClicked.x + "," + _nodeClicked.y + ") sameNode:" + _aStarGraph.IsSameNode(_startNode, _nodeClicked) + ", _endNodeToBeActioned:" + _endNodeToBeActioned + ", _performActionFromNeighbourTile:" + _performActionFromNeighbourTile);
				Stopwatch stopwatch = Stopwatch.StartNew();
				if (aStarGraph.gameLocation is AnimalHouse)
				{
					_aStarPath = _aStarGraph.GetShortestPathAStar(_startNode, _nodeClicked);
				}
				else if (aStarGraph.gameLocation is BoatTunnel && _startNode.x != 6 && _startNode.y != 9)
				{
					_aStarPath = _aStarGraph.GetShortestPathAStar(_nodeClicked, _startNode);
					_aStarPath.nodes.Reverse();
					_aStarPath.nodes.Add(_nodeClicked);
				}
				else if (aStarGraph.gameLocation is IslandNorth && TapToMoveUtils.isOnOrNearSuspensionBridge(Game1.player.getTileX(), Game1.player.getTileY()))
				{
					_aStarPath = TapToMoveUtils.getPathOnIslandNorthBridge(aStarGraph, new Vector2(_startNode.x, _startNode.y), new Vector2(_nodeClicked.x, _nodeClicked.y));
				}
				else if (Game1.whichFarm == 6 && aStarGraph.gameLocation is Farm && _nodeClicked.x > 80 && _nodeClicked.y > 11 && _nodeClicked.y < 20)
				{
					_aStarPath = _aStarGraph.GetShortestPathAStarWithBubbleCheck(_startNode, aStarGraph.FetchAStarNode(83, 17));
				}
				else
				{
					_aStarPath = _aStarGraph.GetShortestPathAStarWithBubbleCheck(_startNode, _nodeClicked);
				}
				stopwatch.Stop();
			}
			catch (Exception ex14)
			{
				Log.It("TapToMove.OnTap N EXCEPTION -> " + ex14.ToString());
				Dictionary<string, string> dictionary14 = new Dictionary<string, string>();
				dictionary14["block"] = "N create _aStarPath";
				Reset();
				return;
			}
			if ((_aStarPath == null || _aStarPath.nodes == null || _aStarPath.nodes.Count == 0 || _aStarPath.nodes[0] == null) && _endNodeOccupied != null && _performActionFromNeighbourTile)
			{
				_aStarPath = _aStarGraph.GetShortestPathToNeighbouringDiagonalAStarWithBubbleCheck(_startNode, _nodeClicked);
				if (_aStarPath != null && _aStarPath.nodes.Count > 0)
				{
					_nodeClicked.FakeTileClear = false;
					_nodeClicked = _aStarPath.nodes[_aStarPath.nodes.Count - 1];
					_endNodeOccupied = null;
					_performActionFromNeighbourTile = false;
				}
			}
			if (_aStarPath != null && _aStarPath.nodes != null && _aStarPath.nodes.Count > 0 && _aStarPath.nodes[0] != null)
			{
				try
				{
					_gateNode = _aStarPath.ContainsGate();
					if (_endNodeOccupied != null)
					{
						_aStarPath = _aStarGraph.SmoothRightAngles(_aStarPath, 2);
					}
					else
					{
						_aStarPath = _aStarGraph.SmoothRightAngles(_aStarPath);
					}
					if (_nodeClicked.FakeTileClear)
					{
						if (_aStarPath.nodes.Count > 0)
						{
							_aStarPath.nodes.RemoveAt(_aStarPath.nodes.Count - 1);
						}
						_nodeClicked.FakeTileClear = false;
					}
					if (_aStarPath.nodes.Count > 0)
					{
						_finalNode = _aStarPath.nodes[_aStarPath.nodes.Count - 1];
					}
					_phase = TapToMovePhase.FollowingAStarPath;
					return;
				}
				catch (Exception ex15)
				{
					Log.It("TapToMove.OnTap O EXCEPTION -> " + ex15.ToString());
					Dictionary<string, string> dictionary15 = new Dictionary<string, string>();
					dictionary15["block"] = "O smooth aStarPath";
					Reset();
					return;
				}
			}
			if (_aStarGraph.IsSameNode(_startNode, _nodeClicked) && (_endNodeToBeActioned || _performActionFromNeighbourTile))
			{
				try
				{
					AStarNode aStarNode2 = _aStarGraph.FetchNeighbourNodeThatIsPassible(_startNode.x, _startNode.y);
					if (aStarNode2 == null)
					{
						Reset();
					}
					else if (_tappedOnCrop)
					{
						_phase = TapToMovePhase.JustDoAction1;
					}
					else if (_waterSourceAndFishingRodSelected)
					{
						faceTileClicked(faceClickPoint: true);
						_phase = TapToMovePhase.JustDoAction1;
					}
					else
					{
						_aStarPath.nodes.Add(aStarNode2);
						_aStarPath.nodes.Add(_startNode);
						noPathHere.X = (noPathHere.Y = -1f);
						_finalNode = _aStarPath.nodes[_aStarPath.nodes.Count - 1];
						_phase = TapToMovePhase.FollowingAStarPath;
					}
					return;
				}
				catch (Exception ex16)
				{
					Log.It("TapToMove.OnTap P EXCEPTION -> " + ex16.ToString());
					Dictionary<string, string> dictionary16 = new Dictionary<string, string>();
					dictionary16["block"] = "P stood on same tile";
					Reset();
					return;
				}
			}
			if (_aStarGraph.IsSameNode(_startNode, _nodeClicked))
			{
				try
				{
					noPathHere.X = (noPathHere.Y = -1f);
					if (TapToMoveUtils.NodeIsWarp(_nodeClicked) && (Game1.CurrentEvent == null || !Game1.CurrentEvent.isFestival))
					{
						TapToMoveUtils.WarpIfInRange(_clickPoint);
					}
					Reset();
					return;
				}
				catch (Exception ex17)
				{
					Log.It("TapToMove.OnTap Q EXCEPTION -> " + ex17.ToString());
					Dictionary<string, string> dictionary17 = new Dictionary<string, string>();
					dictionary17["block"] = "Q warp";
					Reset();
					return;
				}
			}
			try
			{
				if (_startNode != null && Game1.player.ActiveObject != null && Game1.player.ActiveObject.name == "Crab Pot")
				{
					TryTofindAlternatePath(_startNode);
					return;
				}
				if (_crabPot != null && Vector2.Distance(new Vector2(_crabPot.TileLocation.X * 64f + 32f, _crabPot.TileLocation.Y * 64f + 32f), Game1.player.Position) < 128f)
				{
					PerformCrabPotAction();
					return;
				}
				if (_endTileIsActionable)
				{
					mouseY += 64;
					noPathHere = _tileClicked;
					if (tryCount > 0)
					{
						noPathHere.Y -= 1f;
					}
					tryCount++;
					OnTap(mouseX, mouseY, viewportX, viewportY, tryCount);
					return;
				}
				if (_targetNPC != null && _targetNPC.Name == "Robin" && gameLocation is BuildableGameLocation)
				{
					bool flag = _targetNPC.checkAction(Game1.player, Game1.currentLocation);
				}
				noPathHere = _tileClicked;
				if (tryCount > 0)
				{
					noPathHere.Y -= 1f;
				}
				Reset();
			}
			catch (Exception ex18)
			{
				Log.It("TapToMove.OnTap R EXCEPTION -> " + ex18.ToString());
				Dictionary<string, string> dictionary18 = new Dictionary<string, string>();
				dictionary18["block"] = "R no path found";
				Reset();
			}
		}

		private bool TileOnMap(int x, int y)
		{
			int layerWidth = _aStarGraph.map.Layers[0].LayerWidth;
			int layerHeight = _aStarGraph.map.Layers[0].LayerHeight;
			if (x >= 0 && x <= layerWidth && y >= 0)
			{
				return y <= layerHeight;
			}
			return false;
		}

		private bool CheckToEatFood(int clickPointX, int clickPointY)
		{
			bool flag = false;
			if (TappedOnFarmer(clickPointX, clickPointY) && Game1.player.ActiveObject != null)
			{
				if (Game1.player.ActiveObject.Edibility != -300)
				{
					flag = true;
				}
				else if (Game1.player.ActiveObject.name.Length >= 11 && Game1.player.ActiveObject.name.Substring(0, 11) == "Secret Note")
				{
					flag = true;
				}
				else if (Game1.player.ActiveObject.name != null && Game1.player.ActiveObject.name.Contains("Journal Scrap"))
				{
					flag = true;
				}
				else if (Game1.player.ActiveObject.name != null && Game1.player.ActiveObject.name.Contains("Totem"))
				{
					flag = true;
				}
				else if (Game1.player.ActiveObject.ParentSheetIndex == 911)
				{
					flag = true;
				}
				else if (Game1.player.ActiveObject.ParentSheetIndex == 879)
				{
					flag = true;
				}
			}
			if (flag)
			{
				_phase = TapToMovePhase.JustDoRightClick1;
			}
			return flag;
		}

		private bool CheckToDoDefenseAction(int clickPointX, int clickPointY)
		{
			if (Game1.player.CurrentTool != null && Game1.player.CurrentTool.isHeavyHitter() && TappedOnFarmer(clickPointX, clickPointY))
			{
				_phase = TapToMovePhase.JustDoRightClick1;
				return true;
			}
			return false;
		}

		private bool TappedOnFarmer(int x, int y)
		{
			return new Microsoft.Xna.Framework.Rectangle((int)Game1.player.position.X, (int)Game1.player.position.Y - 85, 64, 125).Contains(x, y);
		}

		private bool holdingWallpaperAndTileClickedIsWallOrFloor()
		{
			if (Game1.player.CurrentItem != null && Game1.player.CurrentItem is Wallpaper && gameLocation is DecoratableLocation)
			{
				return ((Wallpaper)Game1.player.CurrentItem).canReallyBePlacedHere((DecoratableLocation)gameLocation, new Vector2(_tileClicked.X, _tileClicked.Y));
			}
			return false;
		}

		private void SelectDifferentEndNode(int x, int y)
		{
			AStarNode aStarNode = _aStarGraph.FetchAStarNode(x, y);
			if (aStarNode != null)
			{
				_nodeClicked = aStarNode;
				_tileClicked.X = x;
				_tileClicked.Y = y;
				_clickPoint.X = x * 64 + 32;
				_clickPoint.Y = y * 64 + 32;
				_mouseX = (int)_clickPoint.X - _viewportX;
				_mouseY = (int)_clickPoint.Y - _viewportY;
			}
		}

		private bool TappedOnAnotherQueableCrop(int clickPointX, int clickPointY)
		{
			AStarNode aStarNode = _aStarGraph.FetchAStarNode(clickPointX / 64, clickPointY / 64);
			if (aStarNode != null)
			{
				Game1.currentLocation.terrainFeatures.TryGetValue(new Vector2(aStarNode.x, aStarNode.y), out var value);
				if (value != null && value is HoeDirt)
				{
					HoeDirt hoeDirt = (HoeDirt)value;
					if ((int)hoeDirt.state != 1 && Game1.player.CurrentTool is WateringCan && ((WateringCan)Game1.player.CurrentTool).WaterLeft > 0)
					{
						return true;
					}
					if ((int)hoeDirt.state != 1 && Game1.player.CurrentTool is WateringCan && ((WateringCan)Game1.player.CurrentTool).WaterLeft <= 0)
					{
						Game1.player.doEmote(4);
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:WateringCan.cs.14335"));
					}
					if (hoeDirt.crop != null && ((Game1.player.CurrentTool is WateringCan && ((WateringCan)Game1.player.CurrentTool).WaterLeft > 0) || (bool)hoeDirt.crop.fullyGrown))
					{
						return true;
					}
				}
				if (mouseCursor == 6 || Utility.canGrabSomethingFromHere(clickPointX, clickPointY, Game1.player))
				{
					return true;
				}
				if (Game1.player.CurrentTool is WateringCan && Game1.player.UsingTool)
				{
					return true;
				}
			}
			return false;
		}

		private bool TappedOnHoeDirtAndHoldingSeed(int clickPointX, int clickPointY)
		{
			AStarNode aStarNode = _aStarGraph.FetchAStarNode(clickPointX / 64, clickPointY / 64);
			if (aStarNode != null)
			{
				Game1.currentLocation.terrainFeatures.TryGetValue(new Vector2(aStarNode.x, aStarNode.y), out var value);
				if (value != null && value is HoeDirt)
				{
					HoeDirt hoeDirt = (HoeDirt)value;
					if (hoeDirt.crop == null && Game1.player.ActiveObject != null && Game1.player.ActiveObject.Category == -74)
					{
						return true;
					}
				}
			}
			return false;
		}

		private void SetMouseCursor(AStarNode endNode)
		{
			mouseCursor = 0;
			if (endNode == null)
			{
				return;
			}
			bool flag = gameLocation.isActionableTile(endNode.x, endNode.y, Game1.player);
			if (!flag)
			{
				flag = gameLocation.isActionableTile(endNode.x, endNode.y + 1, Game1.player);
			}
			if (flag)
			{
				mouseCursor = 2;
			}
			if (gameLocation.doesTileHaveProperty(endNode.x, endNode.y, "Action", "Buildings") != null && gameLocation.doesTileHaveProperty(endNode.x, endNode.y, "Action", "Buildings").Contains("Message"))
			{
				mouseCursor = 5;
			}
			Vector2 vector = new Vector2(endNode.x, endNode.y);
			NPC nPC = Game1.currentLocation.isCharacterAtTile(vector);
			if (nPC != null && !nPC.IsMonster)
			{
				if (!Game1.eventUp && Game1.player.ActiveObject != null && Game1.player.ActiveObject.canBeGivenAsGift() && Game1.player.friendshipData.ContainsKey(nPC.Name) && Game1.player.friendshipData[nPC.Name].GiftsToday != 1)
				{
					mouseCursor = 3;
				}
				else if (nPC.canTalk() && nPC.CurrentDialogue != null && (nPC.CurrentDialogue.Count > 0 || nPC.hasTemporaryMessageAvailable()) && !nPC.isOnSilentTemporaryMessage())
				{
					mouseCursor = 4;
				}
			}
			if (Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival && Game1.CurrentEvent.festivalHost != null && Game1.CurrentEvent.festivalHost.getTileLocation().Equals(vector))
			{
				mouseCursor = 4;
			}
			if (Game1.player.IsLocalPlayer)
			{
				if (Game1.currentLocation.Objects.ContainsKey(vector))
				{
					if ((bool)Game1.currentLocation.Objects[vector].readyForHarvest || (Game1.currentLocation.Objects[vector].Name.Contains("Table") && Game1.currentLocation.Objects[vector].heldObject.Value != null) || (bool)Game1.currentLocation.Objects[vector].isSpawnedObject || (Game1.currentLocation.Objects[vector] is IndoorPot && (Game1.currentLocation.Objects[vector] as IndoorPot).hoeDirt.Value.readyForHarvest()))
					{
						mouseCursor = 6;
					}
				}
				else if (Game1.currentLocation.terrainFeatures.ContainsKey(vector) && Game1.currentLocation.terrainFeatures[vector] is HoeDirt && ((HoeDirt)Game1.currentLocation.terrainFeatures[vector]).readyForHarvest())
				{
					mouseCursor = 6;
				}
			}
			if (Game1.player.usingSlingshot)
			{
				mouseCursor = -1;
			}
		}

		private bool EndNodeBlocked(AStarNode endNode)
		{
			_toolToSelect = null;
			if (Game1.currentLocation is Beach)
			{
				Beach beach = (Beach)Game1.currentLocation;
				if (endNode.x == 53 && endNode.y == 8 && Game1.CurrentEvent != null && Game1.CurrentEvent.id == 13)
				{
					_tappedHaleyBracelet = true;
					_endNodeToBeActioned = true;
					return true;
				}
				if (endNode.x == 57 && endNode.y == 13 && !beach.bridgeFixed)
				{
					_endTileIsActionable = true;
					return false;
				}
				if (beach.oldMariner != null && endNode.x == beach.oldMariner.getTileX() && (endNode.y == beach.oldMariner.getTileY() || endNode.y == beach.oldMariner.getTileY() - 1))
				{
					if (endNode.y == beach.oldMariner.getTileY() - 1)
					{
						SelectDifferentEndNode(endNode.x, endNode.y + 1);
					}
					_performActionFromNeighbourTile = true;
					return true;
				}
				_ = beach.oldMariner;
			}
			if (Game1.currentLocation is VolcanoDungeon && (Game1.currentLocation as VolcanoDungeon).CanRefillWateringCanOnTile(endNode.x, endNode.y))
			{
				int num = (Game1.currentLocation as VolcanoDungeon).level;
				if ((num == 0 || num == 5) && endNode.TileClear)
				{
					if (num == 5)
					{
						SelectDifferentEndNode(30, 31);
					}
					else
					{
						SelectDifferentEndNode(27, 44);
					}
					_endNodeToBeActioned = true;
					_performActionFromNeighbourTile = true;
					return true;
				}
			}
			if (TapToMoveUtils.TappedEggAtEggFestival(_clickPoint))
			{
				_endNodeToBeActioned = true;
				_performActionFromNeighbourTile = !endNode.TileClear;
				return !endNode.TileClear;
			}
			if (TapToMoveUtils.ContainsCinemaTicketOffice(endNode.x, endNode.y))
			{
				SelectDifferentEndNode(endNode.x, 20);
				_endTileIsActionable = true;
				_performActionFromNeighbourTile = true;
				_tappedCinemaTicketBooth = true;
				return true;
			}
			if (TapToMoveUtils.ContainsCinemaDoor(endNode.x, endNode.y))
			{
				SelectDifferentEndNode(endNode.x, 19);
				_endTileIsActionable = true;
				_tappedCinemaDoor = true;
				return true;
			}
			if (gameLocation is CommunityCenter && endNode.x == 14 && endNode.y == 5)
			{
				_performActionFromNeighbourTile = true;
				return true;
			}
			Game1.currentLocation.terrainFeatures.TryGetValue(new Vector2(endNode.x, endNode.y), out var value);
			if (value != null && value is HoeDirt)
			{
				if (Game1.player.CurrentTool is WateringCan && ((WateringCan)Game1.player.CurrentTool).WaterLeft <= 0)
				{
					Game1.player.doEmote(4);
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:WateringCan.cs.14335"));
				}
				if ((int)((HoeDirt)value).state != 1 && Game1.player.CurrentTool is WateringCan && ((WateringCan)Game1.player.CurrentTool).WaterLeft > 0)
				{
					_tappedOnCrop = true;
				}
				Crop crop = ((HoeDirt)value).crop;
				if (crop != null)
				{
					if ((bool)crop.dead)
					{
						if (!(Game1.player.CurrentTool is Hoe))
						{
							AutoSelectTool("Scythe");
						}
						_endNodeToBeActioned = true;
						return true;
					}
					if ((bool)crop.forageCrop && (int)crop.whichForageCrop == 2)
					{
						if (!(Game1.player.CurrentTool is Hoe))
						{
							AutoSelectTool("Hoe");
						}
						_endNodeToBeActioned = true;
						return true;
					}
					if (((int)((HoeDirt)value).state != 1 && Game1.player.CurrentTool is WateringCan && ((WateringCan)Game1.player.CurrentTool).WaterLeft > 0) || crop.ReadyToHarvest)
					{
						_tappedOnCrop = true;
						if (crop.ReadyToHarvest && (int)crop.harvestMethod == 1)
						{
							AutoSelectTool("Scythe");
						}
					}
					else if (Game1.player.CurrentTool is Pickaxe)
					{
						_endNodeToBeActioned = true;
						_performActionFromNeighbourTile = true;
						return true;
					}
				}
				else
				{
					if (Game1.player.CurrentTool is Pickaxe)
					{
						_endNodeToBeActioned = true;
						return true;
					}
					if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.Category == -74)
					{
						_tappedOnCrop = true;
					}
				}
			}
			if (aStarGraph.gameLocation is FarmHouse)
			{
				FarmHouse farmHouse = (FarmHouse)aStarGraph.gameLocation;
				if (farmHouse.upgradeLevel == 2 && _nodeClicked.x == 16 && _nodeClicked.y == 4)
				{
					SelectDifferentEndNode(_nodeClicked.x, _nodeClicked.y + 1);
					_performActionFromNeighbourTile = true;
					return true;
				}
			}
			if (endNode.ContainsFurniture())
			{
				Furniture furniture = endNode.GetFurniture();
				if (furniture != null)
				{
					if ((int)furniture.furniture_type == 14)
					{
						_performActionFromNeighbourTile = true;
						return true;
					}
					if (furniture is TV)
					{
						_performActionFromNeighbourTile = true;
						return true;
					}
					if (furniture is StorageFurniture)
					{
						_performActionFromNeighbourTile = true;
						return true;
					}
					if ((int)furniture.furniture_type == 7)
					{
						_performActionFromNeighbourTile = true;
						_endNodeToBeActioned = false;
						return true;
					}
					if ((int)furniture.parentSheetIndex == 1226 || (int)furniture.parentSheetIndex == 1308)
					{
						_performActionFromNeighbourTile = true;
						return true;
					}
					if ((int)furniture.parentSheetIndex == 1300)
					{
						_performActionFromNeighbourTile = true;
						_endNodeToBeActioned = false;
						furniture.PlaySingingStone();
						_tileClicked.X = (_tileClicked.Y = -1f);
						return true;
					}
					if ((int)furniture.furniture_type <= 3)
					{
						_performActionFromNeighbourTile = false;
						_endNodeToBeActioned = true;
						return true;
					}
				}
			}
			if (endNode.isFence() && Game1.player.CurrentTool != null && Game1.player.CurrentTool.isHeavyHitter() && !(Game1.player.CurrentTool is MeleeWeapon))
			{
				_performActionFromNeighbourTile = true;
				_endNodeToBeActioned = true;
				return true;
			}
			if (endNode.ContainsChest() && Game1.player.CurrentTool != null && Game1.player.CurrentTool.isHeavyHitter() && !(Game1.player.CurrentTool is MeleeWeapon))
			{
				Chest chest = endNode.FetchChest();
				if (chest.itemsCountExcludingNulls() == 0)
				{
					_performActionFromNeighbourTile = true;
					_endNodeToBeActioned = true;
				}
				else
				{
					_performActionFromNeighbourTile = true;
				}
				return true;
			}
			if (TapToMoveUtils.ContainsTravellingCart((int)_clickPoint.X, (int)_clickPoint.Y))
			{
				if (_nodeClicked.y != 11 || (_nodeClicked.x != 23 && _nodeClicked.x != 24))
				{
					SelectDifferentEndNode(27, 11);
				}
				_performActionFromNeighbourTile = true;
				return true;
			}
			if (TapToMoveUtils.ContainsTravellingDesertShop((int)_clickPoint.X, (int)_clickPoint.Y) && (_nodeClicked.y == 23 || _nodeClicked.y == 24))
			{
				_performActionFromNeighbourTile = true;
				if (_nodeClicked.x >= 34 && _nodeClicked.x <= 38)
				{
					SelectDifferentEndNode(_nodeClicked.x, 24);
				}
				else if (_nodeClicked.x == 40 || _nodeClicked.x == 41)
				{
					SelectDifferentEndNode(41, 24);
				}
				else if (_nodeClicked.x == 42 || _nodeClicked.x == 43)
				{
					SelectDifferentEndNode(42, 24);
				}
				return true;
			}
			if (Game1.currentLocation is Forest)
			{
				Forest forest = (Forest)Game1.currentLocation;
				if (forest.log != null && forest.log.getBoundingBox(forest.log.tile).Contains(_nodeClicked.x * 64, _nodeClicked.y * 64))
				{
					_forestLog = forest.log;
					_performActionFromNeighbourTile = true;
					_endNodeToBeActioned = true;
					AutoSelectTool("Axe");
					return true;
				}
			}
			if (endNode.ObjectParentSheetIndexOnTile() == 590)
			{
				AutoSelectTool("Hoe");
				return true;
			}
			if (Game1.currentLocation is Farm && endNode.x == ((Farm)Game1.currentLocation).petBowlPosition.X && endNode.y == ((Farm)Game1.currentLocation).petBowlPosition.Y)
			{
				AutoSelectTool("Watering Can");
				_endNodeToBeActioned = true;
				return true;
			}
			if (Game1.currentLocation is SlimeHutch && endNode.x == 16 && endNode.y >= 6 && endNode.y <= 9)
			{
				AutoSelectTool("Watering Can");
				_endNodeToBeActioned = true;
				return true;
			}
			if (Game1.currentLocation is IslandWest && ((endNode.x == 37 && endNode.y == 87) || (endNode.x == 41 && endNode.y == 86) || (endNode.x == 45 && endNode.y == 86) || (endNode.x == 48 && endNode.y == 87)))
			{
				_endNodeToBeActioned = true;
				return true;
			}
			if (endNode.ContainsNPC() && endNode.FetchNPC() is Horse)
			{
				_tappedOnHorse = (Horse)endNode.FetchNPC();
				if (!(Game1.player.CurrentItem is Hat) && !Game1.player.UsingTool)
				{
					Game1.player.CurrentToolIndex = -1;
				}
				_performActionFromNeighbourTile = true;
				return true;
			}
			if (mouseCursor == 6 || Utility.canGrabSomethingFromHere(_mouseX + _viewportX, _mouseY + _viewportY, Game1.player))
			{
				_tappedOnCrop = true;
				_forageItem = Game1.currentLocation.getObjectAt(_mouseX + _viewportX, _mouseY + _viewportY);
				_performActionFromNeighbourTile = true;
				return true;
			}
			if (Game1.currentLocation is FarmHouse)
			{
				Point bedSpot = ((FarmHouse)Game1.currentLocation).getBedSpot();
				if (bedSpot.X == endNode.x && bedSpot.Y == endNode.y)
				{
					_endNodeToBeActioned = false;
					_performActionFromNeighbourTile = false;
					return false;
				}
			}
			NPC nPC = Game1.currentLocation.isCharacterAtTile(_tileClicked);
			if (nPC != null)
			{
				_performActionFromNeighbourTile = true;
				_targetNPC = nPC;
				if (nPC is Horse)
				{
					_tappedOnHorse = (Horse)nPC;
					if (!(Game1.player.CurrentItem is Hat) && !Game1.player.UsingTool)
					{
						Game1.player.CurrentToolIndex = -1;
					}
				}
				if (Game1.currentLocation is MineShaft && Game1.player.CurrentTool != null && Game1.player.CurrentTool is Pickaxe)
				{
					_endNodeToBeActioned = true;
				}
				return true;
			}
			nPC = Game1.currentLocation.isCharacterAtTile(new Vector2(_tileClicked.X, _tileClicked.Y + 1f));
			if (nPC != null && !(nPC is Duggy) && !(nPC is Grub) && !(nPC is LavaCrab) && !(nPC is MetalHead) && !(nPC is RockCrab) && !(nPC is GreenSlime))
			{
				SelectDifferentEndNode(_nodeClicked.x, _nodeClicked.y + 1);
				_performActionFromNeighbourTile = true;
				_targetNPC = nPC;
				if (nPC is Horse)
				{
					_tappedOnHorse = (Horse)nPC;
					if (!(Game1.player.CurrentItem is Hat) && !Game1.player.UsingTool)
					{
						Game1.player.CurrentToolIndex = -1;
					}
				}
				if (Game1.currentLocation is MineShaft && Game1.player.CurrentTool != null && Game1.player.CurrentTool is Pickaxe)
				{
					_endNodeToBeActioned = true;
				}
				return true;
			}
			_targetFarmAnimal = TapToMoveUtils.FetchFarmAnimal(aStarGraph.gameLocation, (int)_clickPoint.X, (int)_clickPoint.Y);
			if (_targetFarmAnimal != null)
			{
				if (_targetFarmAnimal.getTileX() != _nodeClicked.x || _targetFarmAnimal.getTileY() != _nodeClicked.y)
				{
					SelectDifferentEndNode(_targetFarmAnimal.getTileX(), _targetFarmAnimal.getTileY());
				}
				if ((_targetFarmAnimal.type == "White Cow" || _targetFarmAnimal.type == "Brown Cow" || _targetFarmAnimal.type == "Goat") && Game1.player.CurrentTool is MilkPail)
				{
					return true;
				}
				if (_targetFarmAnimal.type == "Sheep" && Game1.player.CurrentTool is Shears)
				{
					return true;
				}
				_performActionFromNeighbourTile = true;
				return true;
			}
			if (Game1.player.ActiveObject != null && ((bool)Game1.player.ActiveObject.bigCraftable || _actionableObjectIDs.Contains(Game1.player.ActiveObject.parentSheetIndex) || (Game1.player.ActiveObject is Wallpaper && (int)Game1.player.ActiveObject.parentSheetIndex <= 40)) && (!(Game1.player.ActiveObject is Wallpaper) || Game1.currentLocation is DecoratableLocation))
			{
				if (Game1.player.ActiveObject.ParentSheetIndex == 288)
				{
					Building building = _nodeClicked.FetchBuilding();
					if (building is FishPond)
					{
						_actionableBuilding = building;
						Vector2 tileNextToBuildingNearestFarmer = TapToMoveUtils.GetTileNextToBuildingNearestFarmer(aStarGraph, building, Game1.player);
						SelectDifferentEndNode((int)tileNextToBuildingNearestFarmer.X, (int)tileNextToBuildingNearestFarmer.Y);
						_performActionFromNeighbourTile = true;
						return true;
					}
				}
				_performActionFromNeighbourTile = true;
				return true;
			}
			if (Game1.currentLocation is Mountain && endNode.x == 29 && endNode.y == 9)
			{
				_performActionFromNeighbourTile = true;
				return true;
			}
			CrabPot crabPot = TapToMoveUtils.ClickedCrabPot(_aStarGraph, _nodeClicked);
			if (crabPot != null)
			{
				_crabPot = crabPot;
				_performActionFromNeighbourTile = true;
				AStarNode aStarNode = TapToMoveUtils.FetchMostAccessibleNodeToCrabPot(_aStarGraph, endNode);
				if (aStarNode != null && endNode != aStarNode)
				{
					_nodeClicked = aStarNode;
					return false;
				}
				return true;
			}
			if (!endNode.TileClear)
			{
				Game1.currentLocation.objects.TryGetValue(new Vector2(endNode.x, endNode.y), out var value2);
				if (value2 != null)
				{
					if (value2.Category == -9)
					{
						if ((int)value2.parentSheetIndex == 99 || (int)value2.parentSheetIndex == 101 || (int)value2.parentSheetIndex == 163)
						{
							if (Game1.player.CurrentTool is Axe || Game1.player.CurrentTool is Pickaxe)
							{
								_endNodeToBeActioned = true;
							}
							_performActionFromNeighbourTile = true;
							return true;
						}
						if ((int)value2.parentSheetIndex == 163 || (int)value2.parentSheetIndex == 216 || (int)value2.parentSheetIndex == 208)
						{
							_performActionFromNeighbourTile = true;
							if (Game1.player.CurrentTool != null && Game1.player.CurrentTool.isHeavyHitter() && !(Game1.player.CurrentTool is MeleeWeapon))
							{
								_endNodeToBeActioned = true;
							}
							return true;
						}
						if (value2 is BreakableContainer)
						{
							MeleeWeapon meleeWeapon = chooseActiveWeapon();
							if (meleeWeapon != null)
							{
								AutoSelectTool(meleeWeapon.Name);
							}
							else if (Game1.player.CurrentTool == null || !Game1.player.CurrentTool.isHeavyHitter())
							{
								AutoSelectTool("Pickaxe");
							}
							_endNodeToBeActioned = true;
							return true;
						}
						if (Game1.player.CurrentTool != null && (Game1.player.CurrentTool is Axe || Game1.player.CurrentTool is Pickaxe || Game1.player.CurrentTool is Hoe))
						{
							_endNodeToBeActioned = true;
						}
						if (value2.Name.Contains("Chest"))
						{
							_performActionFromNeighbourTile = true;
						}
						return true;
					}
					if (value2.Name == "Stone" || value2.Name == "Boulder")
					{
						AutoSelectTool("Pickaxe");
						return true;
					}
					if (value2.Name == "Weeds")
					{
						if (Game1.player.currentLocation is MineShaft)
						{
							AutoSelectTool(chooseActiveWeapon().Name);
						}
						else if (!AutoSelectTool("Scythe"))
						{
							AutoSelectTool(chooseActiveWeapon().Name);
						}
						return true;
					}
					if (value2.Name == "Twig")
					{
						AutoSelectTool("Axe");
						return true;
					}
					if (value2.Name == "House Plant")
					{
						AutoSelectTool("Pickaxe");
						_endNodeToBeActioned = true;
						return true;
					}
					if (value2.Name == "Sprinkler" && (Game1.player.CurrentTool is Axe || Game1.player.CurrentTool is Pickaxe))
					{
						_endNodeToBeActioned = true;
						return true;
					}
					if (value2.Name == "SupplyCrate")
					{
						if (!AutoSelectTool("Scythe") && !AutoSelectTool(chooseActiveWeapon().Name))
						{
							AutoSelectTool("Axe");
						}
						return true;
					}
					if ((int)value2.parentSheetIndex == 463 || (int)value2.parentSheetIndex == 464)
					{
						if (Game1.player.CurrentTool != null && (Game1.player.CurrentTool is Axe || Game1.player.CurrentTool is Pickaxe || Game1.player.CurrentTool is Hoe))
						{
							_endNodeToBeActioned = true;
						}
						return true;
					}
				}
				else
				{
					if (endNode.ContainsStumpOrBoulder())
					{
						if (endNode.ContainsStumpOrHollowLog())
						{
							AutoSelectTool("Axe");
						}
						else if (endNode.ContainsGiantCrop())
						{
							GiantCrop giantCrop = endNode.FetchGiantCrop();
							if ((int)giantCrop.width == 3 && (int)giantCrop.height == 3 && giantCrop.tile.X + 1f == (float)endNode.x && giantCrop.tile.Y + 1f == (float)endNode.y)
							{
								Point point = TapToMoveUtils.FetchNextPointOut(_aStarGraph.FarmerAStarNodeOffset.x, _aStarGraph.FarmerAStarNodeOffset.y, endNode.x, endNode.y);
								SelectDifferentEndNode(point.X, point.Y);
							}
							AutoSelectTool("Axe");
						}
						else
						{
							AutoSelectTool("Pickaxe");
						}
						return true;
					}
					Building building2 = _nodeClicked.FetchBuilding();
					if (building2 != null && building2.buildingType.Equals("Shipping Bin"))
					{
						_performActionFromNeighbourTile = true;
						return true;
					}
					if (building2 != null && building2.buildingType.Equals("Mill"))
					{
						if (Game1.player.ActiveObject != null && ((int)Game1.player.ActiveObject.parentSheetIndex == 284 || (int)Game1.player.ActiveObject.parentSheetIndex == 262))
						{
							_performActionFromNeighbourTile = true;
							_endNodeToBeActioned = true;
							return true;
						}
						_performActionFromNeighbourTile = true;
						return true;
					}
					if (building2 != null && building2 is Barn)
					{
						Barn barn = building2 as Barn;
						int num2 = (int)barn.tileX + barn.animalDoor.X;
						int num3 = (int)barn.tileY + barn.animalDoor.Y;
						if ((_nodeClicked.x == num2 || _nodeClicked.x == num2 + 1) && (_nodeClicked.y == num3 || _nodeClicked.y == num3 - 1))
						{
							if (_nodeClicked.y == num3 - 1)
							{
								SelectDifferentEndNode(_nodeClicked.x, _nodeClicked.y + 1);
							}
							_performActionFromNeighbourTile = true;
							return true;
						}
					}
					else if (building2 != null && building2 is FishPond && !(Game1.player.CurrentTool is FishingRod) && !(Game1.player.CurrentTool is WateringCan))
					{
						_actionableBuilding = building2;
						Vector2 tileNextToBuildingNearestFarmer2 = TapToMoveUtils.GetTileNextToBuildingNearestFarmer(aStarGraph, building2, Game1.player);
						SelectDifferentEndNode((int)tileNextToBuildingNearestFarmer2.X, (int)tileNextToBuildingNearestFarmer2.Y);
						_performActionFromNeighbourTile = true;
						return true;
					}
					AStarNode aStarNode2 = _aStarGraph.FetchAStarNode(_nodeClicked.x, _nodeClicked.y - 1);
					if (aStarNode2 != null && aStarNode2.ContainsFurniture() && (int)aStarNode2.GetFurniture().parentSheetIndex == 1402)
					{
						SelectDifferentEndNode(_nodeClicked.x, _nodeClicked.y + 1);
						_performActionFromNeighbourTile = true;
						return true;
					}
				}
				if (TapToMoveUtils.IsTreeAt(endNode))
				{
					if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.edibility.Value > -300)
					{
						Game1.player.CurrentToolIndex = -1;
					}
					TerrainFeature treeAt = TapToMoveUtils.GetTreeAt(endNode.x, endNode.y);
					if (treeAt is Tree)
					{
						Tree tree = treeAt as Tree;
						if ((int)tree.growthStage <= 1)
						{
							AutoSelectTool("Scythe");
						}
						else
						{
							AutoSelectTool("Axe");
						}
					}
					return true;
				}
				if (TapToMoveUtils.IsStumpAt(endNode))
				{
					AutoSelectTool("Axe");
					return true;
				}
				if (TapToMoveUtils.IsBoulderAt(endNode))
				{
					AutoSelectTool("Pickaxe");
					return true;
				}
				if (Game1.currentLocation is Town && endNode.x == 108 && endNode.y == 41)
				{
					_performActionFromNeighbourTile = true;
					_endNodeToBeActioned = true;
					_endTileIsActionable = true;
					return true;
				}
				if (Game1.currentLocation is Town && endNode.x == 100 && endNode.y == 66)
				{
					_performActionFromNeighbourTile = true;
					_endNodeToBeActioned = true;
					return true;
				}
				if (TapToMoveUtils.IsBushAt(endNode))
				{
					Bush bush = TapToMoveUtils.FetchBushAt(endNode);
					if (Game1.player.CurrentTool is Axe && bush.isDestroyable(Game1.currentLocation, _tileClicked))
					{
						_endNodeToBeActioned = true;
						_performActionFromNeighbourTile = true;
						return true;
					}
					_performActionFromNeighbourTile = true;
					return true;
				}
				if (TapToMoveUtils.IsOreAt(_tileClicked) && AutoSelectTool("Copper Pan"))
				{
					AStarNode aStarNode3 = TapToMoveUtils.FetchAStarNodeNearestWaterSource(_aStarGraph, _nodeClicked);
					if (aStarNode3 != null)
					{
						_nodeClicked = aStarNode3;
						_tileClicked = new Vector2(_nodeClicked.x, _nodeClicked.y);
						_clickPoint = _nodeClicked.NodeCenterOnMap;
						_endNodeToBeActioned = true;
						return true;
					}
				}
				if (Game1.isActionAtCurrentCursorTile && Game1.player.CurrentTool is FishingRod)
				{
					Game1.player.CurrentToolIndex = -1;
				}
				if (TapToMoveUtils.IsWateringCanFillingSource(_tileClicked) && (Game1.player.CurrentTool is WateringCan || Game1.player.CurrentTool is FishingRod))
				{
					if (Game1.player.CurrentTool is FishingRod && Game1.currentLocation is Town && _nodeClicked.x >= 50 && _nodeClicked.x <= 53 && _nodeClicked.y >= 103 && _nodeClicked.y <= 105)
					{
						_nodeClicked = _aStarGraph.FetchAStarNode(52, _nodeClicked.y);
					}
					else
					{
						AStarNode aStarNode4 = TapToMoveUtils.FetchNearestAStarLandNodePerpendicularToWaterSource(_aStarGraph, _aStarGraph.FarmerAStarNodeOffset, _nodeClicked);
						float num4 = 2.5f;
						if (Game1.player.CurrentTool is FishingRod)
						{
							num4 += (float)FishingRod.getAddedDistance(Game1.player);
						}
						if (aStarNode4 != null && Game1.player.CurrentTool is FishingRod && Vector2.Distance(TapToMoveUtils.PlayerOffsetPosition, aStarNode4.NodeCenterOnMap) < 64f * num4)
						{
							faceTileClicked();
							_nodeClicked = _startNode;
						}
						else if (aStarNode4 != null)
						{
							_nodeClicked = aStarNode4;
						}
					}
					_waterSourceAndFishingRodSelected = true;
					_endNodeToBeActioned = true;
					_tileClicked = new Vector2(_nodeClicked.x, _nodeClicked.y);
					_performActionFromNeighbourTile = true;
					return true;
				}
				if (TapToMoveUtils.IsWizardBuilding(_tileClicked))
				{
					_performActionFromNeighbourTile = true;
					return true;
				}
				_endTileIsActionable = Game1.currentLocation.isActionableTile(_nodeClicked.x, _nodeClicked.y, Game1.player) || Game1.currentLocation.isActionableTile(_nodeClicked.x, _nodeClicked.y + 1, Game1.player);
				_ = _endTileIsActionable;
				if (!_endTileIsActionable)
				{
					Tile tile = Game1.currentLocation.map.GetLayer("Buildings").PickTile(new Location((int)_tileClicked.X * 64, (int)_tileClicked.Y * 64), Game1.viewport.Size);
					_endTileIsActionable = tile != null;
				}
				_ = _endTileIsActionable;
				if (Game1.currentLocation is BoatTunnel && endNode.x == 6 && endNode.y == 9)
				{
					return false;
				}
				return true;
			}
			Game1.currentLocation.terrainFeatures.TryGetValue(new Vector2(endNode.x, endNode.y), out var value3);
			if (value3 != null)
			{
				if (value3 is Grass && Game1.player.CurrentTool != null && Game1.player.CurrentTool is MeleeWeapon && (int)((MeleeWeapon)Game1.player.CurrentTool).type != 2)
				{
					_endNodeToBeActioned = true;
					return true;
				}
				if (value3 is Flooring && Game1.player.CurrentTool != null && (Game1.player.CurrentTool is Pickaxe || Game1.player.CurrentTool is Axe))
				{
					_endNodeToBeActioned = true;
					return true;
				}
			}
			Game1.currentLocation.objects.TryGetValue(new Vector2(endNode.x, endNode.y), out var value4);
			if (value4 != null && ((int)value4.parentSheetIndex == 93 || (int)value4.parentSheetIndex == 94) && Game1.player.CurrentTool != null && (Game1.player.CurrentTool is Pickaxe || Game1.player.CurrentTool is Axe))
			{
				_endNodeToBeActioned = true;
				return true;
			}
			if (Game1.player.CurrentTool is FishingRod && Game1.currentLocation is Town && _nodeClicked.x >= 50 && _nodeClicked.x <= 53 && _nodeClicked.y >= 103 && _nodeClicked.y <= 105)
			{
				_waterSourceAndFishingRodSelected = true;
				_nodeClicked = _aStarGraph.FetchAStarNode(52, _nodeClicked.y);
				_tileClicked = new Vector2(_nodeClicked.x, _nodeClicked.y);
				_endNodeToBeActioned = true;
				return true;
			}
			if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.canBePlacedHere(Game1.currentLocation, _tileClicked) && ((bool)Game1.player.ActiveObject.bigCraftable || (int)Game1.player.ActiveObject.parentSheetIndex == 104 || (int)Game1.player.ActiveObject.parentSheetIndex == 688 || (int)Game1.player.ActiveObject.parentSheetIndex == 689 || (int)Game1.player.ActiveObject.parentSheetIndex == 690 || (int)Game1.player.ActiveObject.parentSheetIndex == 681 || (int)Game1.player.ActiveObject.parentSheetIndex == 886 || (int)Game1.player.ActiveObject.parentSheetIndex == 161 || (int)Game1.player.ActiveObject.parentSheetIndex == 155 || (int)Game1.player.ActiveObject.parentSheetIndex == 162 || Game1.player.ActiveObject.name.Contains("Sapling")))
			{
				_performActionFromNeighbourTile = true;
				return true;
			}
			if (Game1.player.mount == null)
			{
				_endNodeToBeActioned = TapToMoveUtils.HoeSelectedAndTileHoeable(_tileClicked);
				if (_endNodeToBeActioned)
				{
					_performActionFromNeighbourTile = true;
				}
			}
			if (!_endNodeToBeActioned)
			{
				_endNodeToBeActioned = WateringCanActionAtEndNode();
				_ = _endNodeToBeActioned;
			}
			if (!_endNodeToBeActioned && Game1.player.ActiveObject != null && Game1.player.ActiveObject != null)
			{
				_endNodeToBeActioned = Game1.player.ActiveObject.isPlaceable() && Game1.player.ActiveObject.canBePlacedHere(Game1.currentLocation, _tileClicked);
				Crop crop2 = new Crop(Game1.player.ActiveObject.parentSheetIndex, endNode.x, endNode.y);
				if (crop2 != null && ((int)Game1.player.ActiveObject.parentSheetIndex == 368 || (int)Game1.player.ActiveObject.parentSheetIndex == 369))
				{
					_endNodeToBeActioned = true;
				}
				if (crop2 != null && (bool)crop2.raisedSeeds)
				{
					_ = _endNodeToBeActioned;
					_performActionFromNeighbourTile = true;
				}
				_ = _endNodeToBeActioned;
			}
			if (TapToMoveUtils.IsTreeAt(endNode) && (Game1.player.CurrentTool is Hoe || Game1.player.CurrentTool is Axe || Game1.player.CurrentTool is Pickaxe))
			{
				_endNodeToBeActioned = true;
				_ = _endNodeToBeActioned;
			}
			if (mouseCursor == 2 || mouseCursor == 5 || mouseCursor == 4)
			{
				AStarNode aStarNode5 = _aStarGraph.FetchAStarNode(_nodeClicked.x, _nodeClicked.y + 1);
				if (_nodeClicked.ContainsGate())
				{
					_gateClickedOn = TapToMoveUtils.FetchGate(gameLocation, _nodeClicked);
					if (_gateClickedOn.gateOpen)
					{
						_gateClickedOn = null;
					}
					_performActionFromNeighbourTile = true;
				}
				else if (!_nodeClicked.ContainsScarecrow() && aStarNode5.ContainsScarecrow() && Game1.player.CurrentTool != null)
				{
					_endTileIsActionable = true;
					_endNodeToBeActioned = true;
					_performActionFromNeighbourTile = true;
				}
				else
				{
					_endTileIsActionable = true;
					_ = _endTileIsActionable;
				}
			}
			if (TapToMoveUtils.NodeIsWarp(endNode))
			{
				_endNodeToBeActioned = false;
				return false;
			}
			if (!_endNodeToBeActioned)
			{
				AStarNode aStarNode6 = aStarGraph.FetchAStarNode(endNode.x, endNode.y + 1);
				if (aStarNode6 != null)
				{
					Building building3 = aStarNode6.FetchBuilding();
					if (building3 != null && building3.buildingType.Equals("Shipping Bin"))
					{
						SelectDifferentEndNode(endNode.x, endNode.y + 1);
						_performActionFromNeighbourTile = true;
						return true;
					}
				}
			}
			return _endNodeToBeActioned;
		}

		private bool AutoSelectTool(string toolName)
		{
			if (TapToMoveUtils.PlayerHasTool(toolName))
			{
				_toolToSelect = toolName;
				return true;
			}
			return false;
		}

		private void AutoSelectPendingTool()
		{
			if (_toolToSelect != null)
			{
				_lastToolIndexList.Add(Game1.player.CurrentToolIndex);
				TapToMoveUtils.SelectTool(_toolToSelect);
				_toolToSelect = null;
			}
		}

		public void SwitchBackToLastTool()
		{
			if (((TapToMoveUtils.IsMatureTreeStumpOrBoulderAt(_tileClicked) || _forestLog != null) && mobileKeyStates.realTapHeld) || _lastToolIndexList.Count <= 0)
			{
				return;
			}
			int currentToolIndex = _lastToolIndexList[_lastToolIndexList.Count - 1];
			_lastToolIndexList.RemoveAt(_lastToolIndexList.Count - 1);
			if (_lastToolIndexList.Count == 0)
			{
				Game1.player.CurrentToolIndex = currentToolIndex;
				if ((Game1.player.CurrentTool != null && Game1.player.CurrentTool is FishingRod) || Game1.player.CurrentTool is Slingshot)
				{
					Reset();
					startTime = DateTime.Now.Ticks;
				}
			}
		}

		public static MeleeWeapon chooseActiveWeapon()
		{
			if (mostRecentlyChosenMeleeWeapon != null && mostRecentlyChosenMeleeWeapon.Name != "Scythe")
			{
				return mostRecentlyChosenMeleeWeapon;
			}
			MeleeWeapon bestAvailableWeapon = TapToMoveUtils.getBestAvailableWeapon();
			if (bestAvailableWeapon != null)
			{
				return bestAvailableWeapon;
			}
			if (mostRecentlyChosenMeleeWeapon != null)
			{
				return mostRecentlyChosenMeleeWeapon;
			}
			return null;
		}

		public void ClearAutoSelectTool()
		{
			_lastToolIndexList.Clear();
			_toolToSelect = null;
		}

		private void TryTofindAlternatePath(AStarNode startNode)
		{
			if (_endNodeOccupied == null || (!FindAlternatePath(startNode, _nodeClicked.x + 1, _nodeClicked.y + 1) && !FindAlternatePath(startNode, _nodeClicked.x - 1, _nodeClicked.y + 1) && !FindAlternatePath(startNode, _nodeClicked.x + 1, _nodeClicked.y - 1) && !FindAlternatePath(startNode, _nodeClicked.x - 1, _nodeClicked.y - 1)))
			{
				Reset();
			}
		}

		private bool FindAlternatePath(AStarNode start, int x, int y)
		{
			AStarNode aStarNode = _aStarGraph.FetchAStarNode(x, y);
			if (start != null && aStarNode != null && aStarNode.TileClear)
			{
				_aStarPath = _aStarGraph.GetShortestPathAStar(start, aStarNode);
				if (_aStarPath != null && _aStarPath.nodes != null)
				{
					_aStarPath = _aStarGraph.SmoothRightAngles(_aStarPath);
					_phase = TapToMovePhase.FollowingAStarPath;
					return true;
				}
			}
			return false;
		}

		public void Update()
		{
			mobileKeyStates.UpdateReleasedStates();
			if (Game1.options.weaponControl == 8 || Game1.options.weaponControl == 2 || Game1.options.weaponControl == 2 || !PinchZoom.Instance.CheckForPinchZoom())
			{
				if (Game1.eventUp && !Game1.player.CanMove && !Game1.dialogueUp && _phase != 0 && (!(Game1.currentSeason == "winter") || Game1.dayOfMonth != 8) && !(Game1.currentMinigame is FishingGame))
				{
					Reset();
				}
				else if (_phase == TapToMovePhase.FollowingAStarPath && Game1.player.CanMove)
				{
					FollowAStarPathToNextNode();
				}
				else if (_phase == TapToMovePhase.OnFinalTile && Game1.player.CanMove)
				{
					MoveOnFinalTile();
				}
				else if (_phase == TapToMovePhase.ReachedEndOfPath)
				{
					StopMovingAfterReachingEndOfPath();
				}
				else if (_phase == TapToMovePhase.Complete)
				{
					OnTapToMoveComplete();
				}
				else if (_phase == TapToMovePhase.JustDoAction1)
				{
					mobileKeyStates.SetUseTool(useTool: true);
					_phase = TapToMovePhase.JustDoAction2;
				}
				else if (_phase == TapToMovePhase.JustDoAction2)
				{
					mobileKeyStates.SetUseTool(useTool: false);
					_phase = TapToMovePhase.JustDoAction3;
				}
				else if (_phase == TapToMovePhase.JustDoAction3)
				{
					Reset();
					CheckForQueuedReadyToHarvestTaps();
				}
				else if (_phase == TapToMovePhase.JustDoRightClick1)
				{
					mobileKeyStates.actionButtonPressed = true;
					_phase = TapToMovePhase.JustDoRightClick2;
				}
				else if (_phase == TapToMovePhase.JustDoRightClick2)
				{
					mobileKeyStates.actionButtonPressed = false;
					_phase = TapToMovePhase.None;
				}
				else if (_phase == TapToMovePhase.AttackInNewDirection)
				{
					attackInNewDirectionUpdate();
				}
				if (!CheckToAttackMonsters())
				{
					CheckToRetargetNPC();
					CheckToRetargetFarmAnimal();
					CheckToOpenClosedGate();
					CheckToWaterNextTile();
				}
				Game1.virtualJoypad.CheckForManualWeaponControlTaps();
				if (Game1.options.weaponControl == 8 || Game1.options.weaponControl == 2 || Game1.options.weaponControl == 2)
				{
					PinchZoom.Instance.CheckForPinchZoom();
				}
			}
		}

		private void attackInNewDirectionUpdate()
		{
			if (Game1.player.CurrentTool != null && Game1.player.CanMove && !Game1.player.UsingTool && Game1.player.CurrentTool.isHeavyHitter())
			{
				mobileKeyStates.SetMovePressed(_nextWalkDirection);
				Game1.player.faceDirection(_nextDirection);
				_buttonAPressed = true;
				_justUsedWeapon = true;
				mobileKeyStates.SetUseTool(useTool: true);
				_phase = TapToMovePhase.None;
			}
		}

		private void CheckToRetargetNPC()
		{
			if (_targetNPC != null && (_tileClicked.X != -1f || _tileClicked.Y != -1f))
			{
				if (_targetNPC.currentLocation != Game1.currentLocation)
				{
					Reset();
				}
				else if (TapToMoveUtils.NpcAtWarpOrDoor(_targetNPC, Game1.currentLocation))
				{
					Reset();
				}
				else if (_tileClicked.X != (float)_targetNPC.getTileX() || _tileClicked.Y != (float)_targetNPC.getTileY())
				{
					int mouseX = _targetNPC.getTileX() * 64 - Game1.viewport.X + 32;
					int mouseY = _targetNPC.getTileY() * 64 - Game1.viewport.Y + 32;
					OnTap(mouseX, mouseY, Game1.viewport.X, Game1.viewport.Y);
				}
			}
		}

		private void CheckToRetargetFarmAnimal()
		{
			if (_targetFarmAnimal != null && _tileClicked.X != -1f && (_tileClicked.X != (float)_targetFarmAnimal.getTileX() || _tileClicked.Y != (float)_targetFarmAnimal.getTileY()))
			{
				int mouseX = _targetFarmAnimal.getTileX() * 64 - Game1.viewport.X + 32;
				int mouseY = _targetFarmAnimal.getTileY() * 64 - Game1.viewport.Y + 32;
				OnTap(mouseX, mouseY, Game1.viewport.X, Game1.viewport.Y);
			}
		}

		private bool CheckToAttackMonsters()
		{
			if (Game1.options.weaponControl != 0 && Game1.options.weaponControl != 7)
			{
				return false;
			}
			if (Game1.player.stamina <= 0f)
			{
				return false;
			}
			if (!_enableCheckToAttackMonsters)
			{
				if (DateTime.Now.Ticks - _monsterCheckStartTime < 5000000)
				{
					return false;
				}
				_enableCheckToAttackMonsters = true;
			}
			if (_justUsedWeapon)
			{
				_justUsedWeapon = false;
				mobileKeyStates.Reset();
				return false;
			}
			if (_phase != TapToMovePhase.FollowingAStarPath && _phase != TapToMovePhase.OnFinalTile && !Game1.player.UsingTool && (Game1.player.CurrentTool != null || Game1.player.CurrentItem == null))
			{
				_monsterTarget = null;
				float num = 3.4028235E+38f;
				Microsoft.Xna.Framework.Rectangle boundingBox = Game1.player.GetBoundingBox();
				boundingBox.Inflate(64, 64);
				foreach (NPC character in Game1.currentLocation.characters)
				{
					if (character is Monster)
					{
						Monster monster = character as Monster;
						Vector2 value = new Vector2(monster.GetBoundingBox().Center.X, monster.GetBoundingBox().Center.Y);
						Vector2 value2 = new Vector2(Game1.player.GetBoundingBox().Center.X, Game1.player.GetBoundingBox().Center.Y);
						float num2 = Vector2.Distance(value, value2);
						if (num2 < num && boundingBox.Intersects(monster.GetBoundingBox()) && !IsObjectBlockingMonster(monster))
						{
							num = num2;
							_monsterTarget = monster;
						}
					}
				}
				if (_monsterTarget != null)
				{
					if (Game1.options.weaponControl != 7)
					{
						Vector2 vector = new Vector2(_monsterTarget.GetBoundingBox().Center.X, _monsterTarget.GetBoundingBox().Center.Y);
						Vector2 vector2 = new Vector2(Game1.player.GetBoundingBox().Center.X, Game1.player.GetBoundingBox().Center.Y);
						WalkDirection walkDirectionFacing = TapToMoveUtils.GetWalkDirectionFacing(vector, vector2);
						int directionFacing = TapToMoveUtils.GetDirectionFacing(vector, vector2);
						if (Game1.player.FacingDirection != directionFacing)
						{
							Game1.player.faceDirection(directionFacing);
						}
					}
					MeleeWeapon meleeWeapon = chooseActiveWeapon();
					if (_monsterTarget is RockCrab && ((RockCrab)_monsterTarget).isHidingInShell && !(Game1.player.CurrentTool is Pickaxe))
					{
						TapToMoveUtils.SelectTool("Pickaxe");
					}
					else if (meleeWeapon != null && meleeWeapon != Game1.player.CurrentTool)
					{
						_lastToolIndexList.Clear();
						TapToMoveUtils.SelectTool(meleeWeapon.Name);
					}
					_justUsedWeapon = true;
					mobileKeyStates.SetUseTool(useTool: true);
					noPathHere.X = (noPathHere.Y = -1f);
					return true;
				}
			}
			return false;
		}

		private bool IsObjectBlockingMonster(Monster monster)
		{
			int num = Game1.player.getTileX();
			int num2 = Math.Abs(num - monster.getTileX());
			if (num2 == 2)
			{
				num = ((num <= monster.getTileX()) ? (num + 1) : (num - 1));
			}
			int num3 = Game1.player.getTileY();
			int num4 = Math.Abs(num3 - monster.getTileY());
			if (num4 == 2)
			{
				num3 = ((num3 <= monster.getTileY()) ? (num3 + 1) : (num3 - 1));
			}
			Game1.currentLocation.objects.TryGetValue(new Vector2(num, num3), out var value);
			if (value != null && (((int)value.parentSheetIndex >= 118 && (int)value.parentSheetIndex <= 125) || value.Name == "Stone" || value.Name == "Boulder"))
			{
				return true;
			}
			return _aStarGraph.FetchAStarNode(num, num3)?.ContainsStumpOrBoulder() ?? false;
		}

		private void FollowAStarPathToNextNode()
		{
			if (_aStarPath.nodes.Count > 0)
			{
				Game1.player.isEating = false;
				_farmerNode = _aStarGraph.FarmerAStarNodeOffset;
				if (_farmerNode == null)
				{
					Reset();
					return;
				}
				if (_aStarPath.nodes[0] == _farmerNode)
				{
					_aStarPath.nodes.RemoveAt(0);
					_lastDistance = 0f;
					_stuckCount = 0;
					_reallyStuckCount = 0;
				}
				if (_aStarPath.nodes.Count > 0)
				{
					if (_aStarPath.nodes[0].ContainsAnimals() || (_aStarPath.nodes[0].ContainsNPC() && !Game1.player.isRidingHorse() && !(_aStarPath.nodes[0].FetchNPC() is Horse)))
					{
						OnTap(_mouseX, _mouseY, _viewportX, _viewportY);
						return;
					}
					nodeCenter = _aStarPath.nodes[0].NodeCenterOnMap;
					WalkDirection walkDirection = _aStarGraph.WalkDirectionBetweenTwoPoints(TapToMoveUtils.PlayerOffsetPosition, nodeCenter, Game1.player.getMovementSpeed());
					float num = Vector2.Distance(TapToMoveUtils.PlayerOffsetPosition, nodeCenter);
					if (num == _lastDistance || num > _lastDistance)
					{
						_stuckCount++;
					}
					_lastDistance = num;
					if (num < Game1.player.getMovementSpeed() || _stuckCount >= 4)
					{
						if (_reallyStuckCount >= 2)
						{
							walkDirection = _aStarGraph.OppositeWalkDirection(walkDirection);
							_reallyStuckCount++;
							if (_reallyStuckCount == 8)
							{
								if (Game1.player.isRidingHorse())
								{
									Reset();
								}
								else if (_tappedOnHorse != null)
								{
									_tappedOnHorse.checkAction(Game1.player, Game1.currentLocation);
									Reset();
								}
								else if (_aStarGraph.FarmerAStarNodeOffset.FetchNPC() is Horse)
								{
									((Horse)_aStarGraph.FarmerAStarNodeOffset.FetchNPC()).checkAction(Game1.player, Game1.currentLocation);
								}
								else
								{
									OnTap(_mouseX, _mouseY, _viewportX, _viewportY, _tryCount + 1);
								}
								return;
							}
						}
						else
						{
							WalkDirection walkDirection2 = _aStarGraph.WalkDirectionToNextNode(_farmerNode, _aStarPath.nodes[0]);
							if (walkDirection2 != walkDirection)
							{
								_reallyStuckCount++;
								walkDirection = walkDirection2;
							}
							else
							{
								walkDirection2 = _aStarGraph.WalkDirectionBetweenTwoPoints(TapToMoveUtils.PlayerOffsetPosition, nodeCenter);
								if (walkDirection2 != walkDirection)
								{
									_reallyStuckCount++;
									walkDirection = walkDirection2;
								}
							}
							_stuckCount = 0;
						}
					}
					mobileKeyStates.SetMovePressed(walkDirection);
				}
			}
			if (_aStarPath.nodes.Count == 0)
			{
				_aStarPath = null;
				_stuckCount = 0;
				_phase = TapToMovePhase.OnFinalTile;
			}
		}

		private void CheckToOpenClosedGate()
		{
			if (_gateNode == null)
			{
				return;
			}
			float num = Vector2.Distance(TapToMoveUtils.PlayerOffsetPosition, _gateNode.NodeCenterOnMap);
			if (num < 83.2f)
			{
				Fence fence = TapToMoveUtils.FetchGate(_aStarGraph.gameLocation, _gateNode);
				if (fence != null && !fence.gateOpen)
				{
					fence.checkForAction(Game1.player);
					_gateNode = null;
				}
			}
		}

		private void MoveOnFinalTile()
		{
			if (_performActionFromNeighbourTile)
			{
				float num = Vector2.Distance(TapToMoveUtils.PlayerOffsetPosition, _nodeClicked.NodeCenterOnMap);
				float val = Math.Abs(_nodeClicked.NodeCenterOnMap.X - TapToMoveUtils.PlayerOffsetPosition.X) - (float)Game1.player.speed;
				float val2 = Math.Abs(_nodeClicked.NodeCenterOnMap.Y - TapToMoveUtils.PlayerOffsetPosition.Y) - (float)Game1.player.speed;
				if (num == _lastDistance)
				{
					_stuckCount++;
				}
				_lastDistance = num;
				if (Game1.player.GetBoundingBox().Intersects(_nodeClicked.GetBoundingBox()) && _distanceToTarget != DistanceToTarget.TooFar && _crabPot == null)
				{
					_distanceToTarget = DistanceToTarget.TooClose;
					WalkDirection movePressed = _aStarGraph.WalkDirectionBetweenTwoPoints(_clickPoint, TapToMoveUtils.PlayerOffsetPosition);
					mobileKeyStates.SetMovePressed(movePressed);
				}
				else if (_distanceToTarget != DistanceToTarget.TooClose && _stuckCount < 4 && Math.Max(val, val2) > 64f)
				{
					_distanceToTarget = DistanceToTarget.TooFar;
					float num2 = (float)Math.Atan2(_nodeClicked.NodeCenterOnMap.Y - TapToMoveUtils.PlayerOffsetPosition.Y, _nodeClicked.NodeCenterOnMap.X - TapToMoveUtils.PlayerOffsetPosition.X);
					float angleDegrees = num2 / ((float)Math.PI * 2f) * 360f;
					WalkDirection movePressed2 = TapToMoveUtils.WalkDirectionForAngle(angleDegrees);
					mobileKeyStates.SetMovePressed(movePressed2);
				}
				else
				{
					_distanceToTarget = DistanceToTarget.InRange;
					OnReachEndOfPath();
				}
			}
			else
			{
				float num3 = Vector2.Distance(TapToMoveUtils.PlayerOffsetPosition, _nodeClicked.NodeCenterOnMap);
				if (num3 == _lastDistance || num3 > _lastDistance)
				{
					_stuckCount++;
				}
				_lastDistance = num3;
				if (num3 < Game1.player.getMovementSpeed() || _stuckCount >= 4 || (_endNodeToBeActioned && num3 < 64f) || (_endNodeOccupied != null && num3 < 66f))
				{
					OnReachEndOfPath();
					return;
				}
				WalkDirection movePressed3 = _aStarGraph.WalkDirectionBetweenTwoPoints(TapToMoveUtils.PlayerOffsetPosition, _finalNode.NodeCenterOnMap, Game1.player.getMovementSpeed());
				mobileKeyStates.SetMovePressed(movePressed3);
			}
		}

		private void OnReachEndOfPath()
		{
			AutoSelectPendingTool();
			if (_endNodeOccupied != null)
			{
				WalkDirection walkDirection;
				if (_endNodeToBeActioned)
				{
					if (Game1.currentMinigame is FishingGame)
					{
						walkDirection = TapToMoveUtils.GetWalkDirectionFacing(_clickPoint, TapToMoveUtils.PlayerOffsetPosition);
						faceTileClicked();
					}
					else
					{
						walkDirection = _aStarGraph.WalkDirectionBetweenTwoPoints(TapToMoveUtils.PlayerOffsetPosition, _clickPoint);
						if (Game1.player.CurrentTool != null && (Game1.player.CurrentTool is FishingRod || (Game1.player.CurrentTool is WateringCan && _waterSourceAndFishingRodSelected)))
						{
							Game1.player.faceDirection(TapToMoveUtils.GetDirectionFacing(_clickPoint, TapToMoveUtils.PlayerOffsetPosition));
						}
					}
				}
				else
				{
					walkDirection = _aStarGraph.WalkDirectionToNextNode(_aStarGraph.FarmerAStarNode, _endNodeOccupied);
					if (walkDirection == WalkDirection.None)
					{
						walkDirection = _aStarGraph.WalkDirectionBetweenTwoPoints(TapToMoveUtils.PlayerOffsetPosition, _endNodeOccupied.NodeCenterOnMap, 16f);
					}
				}
				if (walkDirection == WalkDirection.None)
				{
					walkDirection = mobileKeyStates.lastWalkDirection;
				}
				mobileKeyStates.SetMovePressed(walkDirection);
				if (_endNodeToBeActioned || !PerformAction())
				{
					if (Game1.player.CurrentTool is WateringCan)
					{
						faceTileClicked(faceClickPoint: true);
					}
					if (!(Game1.player.CurrentTool is FishingRod) || _waterSourceAndFishingRodSelected)
					{
						grabTile = _tileClicked;
						if (TapToMoveUtils.NodeContainsFurniture(_endNodeOccupied))
						{
							Furniture furnitureClickedOn = TapToMoveUtils.GetFurnitureClickedOn((int)_clickPoint.X, (int)_clickPoint.Y);
							if ((int)furnitureClickedOn.furniture_type <= 3)
							{
								Game1.currentLocation.tapToMove.mobileKeyStates.actionButtonPressed = true;
							}
						}
						if (!TapToMoveUtils.IsMatureTreeStumpOrBoulderAt(_tileClicked) && _forestLog == null)
						{
							_tileClicked.X = -1f;
							_tileClicked.Y = -1f;
						}
						if (_tappedHaleyBracelet && Game1.CurrentEvent != null)
						{
							Game1.CurrentEvent.receiveActionPress(53, 8);
							_tappedHaleyBracelet = false;
						}
						else
						{
							mobileKeyStates.SetUseTool(useTool: true);
						}
					}
				}
			}
			else if (_endNodeToBeActioned)
			{
				mobileKeyStates.SetUseTool(useTool: true);
			}
			else if (!PerformAction())
			{
				mobileKeyStates.SetPressed(up: false, down: false, left: false, right: false);
			}
			_phase = TapToMovePhase.ReachedEndOfPath;
		}

		private void StopMovingAfterReachingEndOfPath()
		{
			mobileKeyStates.SetMovePressed(WalkDirection.None);
			mobileKeyStates.actionButtonPressed = false;
			if ((TapToMoveUtils.IsMatureTreeStumpOrBoulderAt(_tileClicked) || _forestLog != null) && mobileKeyStates.realTapHeld && (Game1.player.CurrentTool is Axe || Game1.player.CurrentTool is Pickaxe))
			{
				mobileKeyStates.SetUseTool(useTool: true);
				_phase = TapToMovePhase.PendingComplete;
				if (!_tapPressed)
				{
					OnTapRelease();
				}
			}
			else if ((bool)Game1.player.usingTool && (Game1.player.CurrentTool is WateringCan || Game1.player.CurrentTool is Hoe) && mobileKeyStates.realTapHeld)
			{
				mobileKeyStates.SetUseTool(useTool: true);
				_phase = TapToMovePhase.PendingComplete;
				if (!_tapPressed)
				{
					OnTapRelease();
				}
			}
			else
			{
				mobileKeyStates.SetUseTool(useTool: false);
				_phase = TapToMovePhase.Complete;
			}
		}

		private void OnTapToMoveComplete()
		{
			if (!Game1.player.usingTool && !_warping && _tappedOnHorse == null && TapToMoveUtils.InWarpRange(TapToMoveUtils.PlayerOffsetPosition) && TapToMoveUtils.InWarpRange(_clickPoint) && (Game1.CurrentEvent == null || !Game1.CurrentEvent.isFestival))
			{
				TapToMoveUtils.WarpIfInRange(_clickPoint);
				Reset();
				_warping = true;
			}
			else
			{
				Reset();
			}
			CheckForQueuedReadyToHarvestTaps();
		}

		private bool CheckForQueuedReadyToHarvestTaps()
		{
			_tappedOnCrop = false;
			if (Game1.player.CurrentTool is WateringCan && Game1.player.UsingTool)
			{
				_waitingToFinishWatering = true;
				_tappedOnCrop = true;
				return false;
			}
			if (_tapQueueItemList.Count > 0)
			{
				if (Game1.player.CurrentTool is WateringCan && ((WateringCan)Game1.player.CurrentTool).WaterLeft <= 0)
				{
					Game1.player.doEmote(4);
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:WateringCan.cs.14335"));
					_tapQueueItemList.Clear();
					return false;
				}
				TapQueueItem tapQueueItem = _tapQueueItemList[0];
				_tapQueueItemList.RemoveAt(0);
				OnTap(tapQueueItem.mouseX, tapQueueItem.mouseY, tapQueueItem.viewportX, tapQueueItem.viewportY);
				if (Game1.player.CurrentTool is WateringCan)
				{
					OnTapRelease();
				}
				return true;
			}
			return false;
		}

		private void CheckToWaterNextTile()
		{
			if (_waitingToFinishWatering && !Game1.player.UsingTool)
			{
				_waitingToFinishWatering = false;
				_tappedOnCrop = false;
				CheckForQueuedReadyToHarvestTaps();
			}
		}

		private void faceTileClicked(bool faceClickPoint = false)
		{
			int num = Game1.player.facingDirection;
			int num2 = ((!faceClickPoint) ? TapToMoveUtils.GetDirectionFacing(_tileClicked, new Vector2((int)Game1.player.position.X / 64, (int)Game1.player.position.Y / 64)) : TapToMoveUtils.GetDirectionFacing(_clickPoint, Game1.player.position));
			if (num2 != num)
			{
				Game1.player.Halt();
				Game1.player.faceDirection(num2);
			}
		}

		private void faceClickPoint()
		{
			int num = Game1.player.facingDirection;
			int directionFacing = TapToMoveUtils.GetDirectionFacing(_clickPoint, Game1.player.position);
			if (directionFacing != num)
			{
				Game1.player.Halt();
				Game1.player.faceDirection(directionFacing);
			}
		}

		private bool WateringCanActionAtEndNode()
		{
			if (Game1.player.CurrentTool is WateringCan)
			{
				if (((WateringCan)Game1.player.CurrentTool).WaterLeft > 0)
				{
					Game1.currentLocation.terrainFeatures.TryGetValue(new Vector2(_nodeClicked.x, _nodeClicked.y), out var value);
					if (value != null && value is HoeDirt && (int)((HoeDirt)value).state != 1)
					{
						return true;
					}
				}
				if (Game1.currentLocation is SlimeHutch && _nodeClicked.x == 16 && _nodeClicked.y >= 6 && _nodeClicked.y <= 9)
				{
					return true;
				}
				if (TapToMoveUtils.IsWateringCanFillingSource(_tileClicked))
				{
					return true;
				}
			}
			return false;
		}

		private bool PerformCrabPotAction()
		{
			if (_crabPot != null)
			{
				if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.Category == -21)
				{
					if (_crabPot.performObjectDropInAction(Game1.player.ActiveObject, probe: false, Game1.player))
					{
						Game1.player.reduceActiveItemByOne();
					}
				}
				else if (!_crabPot.checkForAction(Game1.player))
				{
					_crabPot.performRemoveAction(_tileClicked, Game1.currentLocation);
				}
				_crabPot = null;
				return true;
			}
			return false;
		}

		private bool PerformAction()
		{
			if (PerformCrabPotAction())
			{
				return true;
			}
			if (_actionableBuilding != null)
			{
				_actionableBuilding.doAction(new Vector2((int)_actionableBuilding.tileX, (int)_actionableBuilding.tileY), Game1.player);
				return true;
			}
			if (_tappedCinemaTicketBooth)
			{
				_tappedCinemaTicketBooth = false;
				Game1.currentLocation.checkAction(new Location(55, 20), Game1.viewport, Game1.player);
				return true;
			}
			if (_tappedCinemaDoor)
			{
				_tappedCinemaDoor = false;
				Game1.currentLocation.checkAction(new Location(53, 19), Game1.viewport, Game1.player);
				return true;
			}
			if ((_endTileIsActionable || _performActionFromNeighbourTile) && Game1.player.mount != null && _forageItem != null)
			{
				Game1.currentLocation.checkAction(new Location(_nodeClicked.x, _nodeClicked.y), Game1.viewport, Game1.player);
				_forageItem = null;
				return true;
			}
			if (_tappedOnHorse != null)
			{
				_tappedOnHorse.checkAction(Game1.player, Game1.currentLocation);
				Reset();
				return false;
			}
			if (Game1.player.mount != null && _tappedOnHorse == null)
			{
				Game1.player.mount.checkActionEnabled = false;
			}
			_ = _furniture;
			if (mobileKeyStates.realTapHeld && _furniture != null && _forageItem == null)
			{
				_pendingFurnitureAction = true;
				return true;
			}
			if (mouseCursor == 2 && Game1.currentLocation.name == "Blacksmith" && _tileClicked.X == 3f && (_tileClicked.Y == 12f || _tileClicked.Y == 13f || _tileClicked.Y == 14f))
			{
				Game1.currentLocation.performAction("Blacksmith", Game1.player, new Location(3, 14));
				Game1.player.Halt();
				return false;
			}
			if (Game1.currentLocation.isActionableTile((int)_tileClicked.X, (int)_tileClicked.Y, Game1.player) && !_nodeClicked.ContainsGate())
			{
				if (TapToMoveUtils.IsMatureTreeStumpOrBoulderAt(_tileClicked) || _forestLog != null)
				{
					if (_tapHoldActive)
					{
						return false;
					}
					SwitchBackToLastTool();
				}
				Game1.player.Halt();
				mobileKeyStates.actionButtonPressed = true;
				return true;
			}
			if (_endNodeOccupied != null && !_endTileIsActionable && !_performActionFromNeighbourTile)
			{
				if (_furniture != null)
				{
					return true;
				}
				return false;
			}
			if (Game1.currentLocation is Farm && Game1.currentLocation.isActionableTile((int)_tileClicked.X, (int)_tileClicked.Y + 1, Game1.player) && !_nodeClicked.ContainsGate())
			{
				mobileKeyStates.SetMovePressed(WalkDirection.Down);
				mobileKeyStates.actionButtonPressed = true;
				return true;
			}
			if (_targetNPC is Child)
			{
				_targetNPC.checkAction(Game1.player, Game1.currentLocation);
				Reset();
				return false;
			}
			if (_endTileIsActionable || _performActionFromNeighbourTile)
			{
				_gateNode = null;
				if (_gateClickedOn != null)
				{
					_gateClickedOn = null;
					return false;
				}
				faceTileClicked();
				Game1.player.Halt();
				mobileKeyStates.actionButtonPressed = true;
				return true;
			}
			Game1.player.Halt();
			return false;
		}
	}
}
