using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewValley.Mobile
{
	public class VirtualJoypad : IClickableMenu
	{
		private const int MIN_JOYSTICK_MOVE_THRESHOLD = 20;

		public ControlType mostRecentlyUsedControlType;

		public ClickableTextureComponent buttonToggleJoypad;

		public ClickableTextureComponent joystick;

		public ClickableTextureComponent buttonA;

		public ClickableTextureComponent buttonB;

		public long buttonATouchStartTime = 9223372036854775807L;

		public bool buttonAHeld;

		public bool buttonBHeld;

		public bool justUsedSlingshot;

		private int _maxJoystickMoveRadius = 222;

		private Point _joystickStartPosition = Point.Zero;

		private Point _joystickTapPoint = Point.Zero;

		private Point _joystickLastTapPoint = Point.Zero;

		private Vector2 _centerOfJoystick = Vector2.Zero;

		private ClickableComponent _selectedButton;

		private int _lastWeaponControl = -1;

		private bool _showJoypad = true;

		private bool _joystickWasJustHeld;

		private bool _joystickHeld;

		public const int DEFAULT_JOYSTICK_DIAMETER = 185;

		public const int DEFAULT_BUTTON_A_DIAMETER = 111;

		public const int DEFAULT_BUTTON_B_DIAMETER = 111;

		public const int DEFAULT_INVISIBLE_BUTTON_WIDTH = 200;

		private const int ADJUSTER_PANEL_HEIGHT = 244;

		public const int MIN_BUTTON_SIZE = 20;

		public const int MAX_BUTTON_SIZE = 300;

		private bool _adjustmentMode;

		private Point _initialJoystickPosition;

		private Point _initialButtonAPosition;

		private Point _initialButtonBPosition;

		private int _initialJoystickSize = 185;

		private int _initialButtonASize = 111;

		private int _initialButtonBSize = 111;

		public ClickableTextureComponent buttonTick;

		public ClickableTextureComponent buttonCancel;

		public OptionsSlider sizeSlider;

		public OptionsButton buttonDefaults;

		public float buttonAlpha = 0.4f;

		private Rectangle _joystickSourceRect = new Rectangle(0, 116, 37, 37);

		private ClickableTextureComponent radioButtonJoystick;

		private ClickableTextureComponent radioButtonButtonA;

		private ClickableTextureComponent radioButtonButtonB;

		private int _selectedBoxX = 12;

		private int _lastMouseX;

		private int _lastMouseY;

		private bool _leftButtonDown;

		private double _joystickAngle;

		private const int TICKS_BEFORE_BUTTON_B = 2500000;

		private const int TICKS_BEFORE_RESET = 2000000;

		private bool _touchingJoystick;

		private bool _touchingButtonA;

		private bool _touchingButtonB;

		private const int PULL_THRESHOLD = 10;

		private float buttonFadeAlpha;

		private float buttonFadeStep = 0.1f;

		public bool joystickWasJustHeld => _joystickWasJustHeld;

		public bool joystickHeld
		{
			get
			{
				return _joystickHeld;
			}
			set
			{
				if (_joystickHeld && !value)
				{
					_joystickWasJustHeld = true;
				}
				_joystickHeld = value;
				if (_joystickHeld && justUsedSlingshot)
				{
					justUsedSlingshot = false;
				}
			}
		}

		public bool adjustmentMode
		{
			get
			{
				return _adjustmentMode;
			}
			set
			{
				_adjustmentMode = value;
				if (_adjustmentMode)
				{
					CheckToSetDefaults();
					UpdateSettings();
					BackupSizeAndPositions();
					if (buttonTick == null)
					{
						CreateAdjusterControls();
					}
					_selectedBoxX = radioButtonJoystick.bounds.X - 4;
					sizeSlider.whichOption = 143;
					sizeSlider.value = sizeJoystick;
					UpdateButtonSizes();
				}
			}
		}

		public int sizeJoystick
		{
			get
			{
				return Game1.options.joystickSize;
			}
			set
			{
				Game1.options.joystickSize = value;
			}
		}

		public int sizeButtonA
		{
			get
			{
				return Game1.options.buttonASize;
			}
			set
			{
				Game1.options.buttonASize = value;
			}
		}

		public int sizeButtonB
		{
			get
			{
				return Game1.options.buttonBSize;
			}
			set
			{
				Game1.options.buttonBSize = value;
			}
		}

		public Point positionJoystick
		{
			get
			{
				return Game1.options.joystickPosition;
			}
			set
			{
				Game1.options.SetPositionJoystick(value.X, value.Y);
			}
		}

		public Point positionButtonA
		{
			get
			{
				return Game1.options.buttonAPosition;
			}
			set
			{
				Game1.options.SetPositionButtonA(value.X, value.Y);
			}
		}

		public Point positionButtonB
		{
			get
			{
				return Game1.options.buttonBPosition;
			}
			set
			{
				Game1.options.SetPositionButtonB(value.X, value.Y);
			}
		}

		public int screenWidth => Game1.uiViewport.Width;

		public int screenHeight => Game1.uiViewport.Height;

		private float joystickScale => 4f * ((float)sizeJoystick / ((float)_joystickSourceRect.Width * 4f));

		private float buttonAScale => 4f * ((float)sizeButtonA / ((float)_joystickSourceRect.Width * 4f));

		private float buttonBScale => 4f * ((float)sizeButtonB / ((float)_joystickSourceRect.Width * 4f));

		private string settingsStr => "joystick(" + positionJoystick.X + "," + positionJoystick.Y + "), size:" + sizeJoystick + ", scale:" + joystickScale + ", buttonA(" + positionButtonA.X + "," + positionButtonA.Y + "), size:" + sizeButtonA + ", scale:" + buttonAScale + ", buttonB(" + positionButtonB.X + "," + positionButtonB.Y + "), size:" + sizeButtonB + ", scale:" + buttonBScale;

		public Vector2 PositionFromStart => new Vector2(_joystickStartPosition.X - _joystickLastTapPoint.X, _joystickStartPosition.Y - _joystickLastTapPoint.Y);

		public bool showJoystick
		{
			get
			{
				if (Game1.fadeToBlack || Game1.globalFade)
				{
					return false;
				}
				if (Game1.CurrentEvent != null && Game1.CurrentEvent.skippable)
				{
					if (Game1.player.canMove)
					{
						return true;
					}
					return false;
				}
				if (Game1.options.weaponControl == 4 || Game1.options.weaponControl == 7 || Game1.options.weaponControl == 6 || Game1.options.weaponControl == 8)
				{
					return true;
				}
				if (Game1.player.CurrentTool != null && (Game1.player.CurrentTool is MeleeWeapon || Game1.player.CurrentTool is Slingshot) && Game1.player.CurrentTool.InitialParentTileIndex != 47 && (Game1.options.weaponControl == 1 || Game1.options.weaponControl == 5))
				{
					return true;
				}
				return false;
			}
		}

		public double joystickAngle => _joystickAngle;

		public bool showJoypad
		{
			get
			{
				return _showJoypad;
			}
			set
			{
				_showJoypad = value;
				if (_lastWeaponControl == -1)
				{
					_lastWeaponControl = Game1.options.weaponControl;
				}
				if (_showJoypad)
				{
					Game1.options.weaponControl = _lastWeaponControl;
				}
				else
				{
					_lastWeaponControl = Game1.options.weaponControl;
					Game1.options.weaponControl = 0;
				}
				joystickHeld = (_joystickWasJustHeld = false);
			}
		}

		public bool TouchingTwoOrMoreButtons => (joystickHeld ? 1 : 0) + (buttonAHeld ? 1 : 0) + (buttonBHeld ? 1 : 0) > 1;

		public bool TouchingJoystickOrButton
		{
			get
			{
				if (!joystickHeld && !buttonAHeld)
				{
					return buttonBHeld;
				}
				return true;
			}
		}

		public bool ButtonAPressed
		{
			get
			{
				if (_touchingButtonA)
				{
					return buttonAHeld;
				}
				return false;
			}
		}

		public bool ButtonBPressed
		{
			get
			{
				if (_touchingButtonB)
				{
					return buttonBHeld;
				}
				return false;
			}
		}

		public static Vector2 GrabTile
		{
			get
			{
				int num = 1;
				int num2 = 1;
				if (Game1.player.ActiveObject != null)
				{
					num = Game1.player.ActiveObject.boundingBox.Width / 64;
					num2 = Game1.player.ActiveObject.boundingBox.Height / 64;
				}
				Rectangle boundingBox = Game1.player.GetBoundingBox();
				int num3 = boundingBox.Center.X / 64;
				int num4 = boundingBox.Center.Y / 64;
				if (Game1.player.FacingDirection == 3)
				{
					num3 = boundingBox.X / 64 - num;
				}
				else if (Game1.player.FacingDirection != 1)
				{
					num4 = ((Game1.player.FacingDirection != 0) ? ((boundingBox.Y + boundingBox.Height) / 64 + 1) : (boundingBox.Y / 64 - num2));
				}
				else
				{
					num3 = (boundingBox.X + boundingBox.Width) / 64 + 1;
				}
				return new Vector2(num3, num4);
			}
		}

		public void SetPositionJoystick(int x, int y)
		{
			Game1.options.SetPositionJoystick(x, y);
		}

		public void SetPositionButtonA(int x, int y)
		{
			Game1.options.SetPositionButtonA(x, y);
		}

		public void SetPositionButtonB(int x, int y)
		{
			Game1.options.SetPositionButtonB(x, y);
		}

		public VirtualJoypad()
		{
			joystick = new ClickableTextureComponent(new Rectangle(positionJoystick.X, positionJoystick.Y, sizeJoystick, sizeJoystick), Game1.mobileSpriteSheet, _joystickSourceRect, 4f);
			buttonA = new ClickableTextureComponent(new Rectangle(positionButtonA.X, positionButtonA.Y, sizeButtonA, sizeButtonA), Game1.mobileSpriteSheet, _joystickSourceRect, 4f);
			buttonB = new ClickableTextureComponent(new Rectangle(positionButtonB.X, positionButtonB.Y, sizeButtonB, sizeButtonB), Game1.mobileSpriteSheet, _joystickSourceRect, 4f);
			buttonToggleJoypad = new ClickableTextureComponent(new Rectangle(Game1.toolbarPaddingX + 36, 12, 64, 64), Game1.mobileSpriteSheet, new Rectangle(98, 44, 16, 16), 4f);
		}

		private void CreateAdjusterControls()
		{
			buttonCancel = new ClickableTextureComponent("Cancel", new Rectangle(screenWidth - Game1.xEdge - 80 - 20, 20, 80, 80), null, null, Game1.mobileSpriteSheet, new Rectangle(20, 0, 20, 20), 4f);
			buttonTick = new ClickableTextureComponent("OK", new Rectangle(buttonCancel.bounds.X, 144, 80, 80), null, null, Game1.mobileSpriteSheet, new Rectangle(0, 0, 20, 20), 4f);
			radioButtonJoystick = new ClickableTextureComponent(new Rectangle(Game1.xEdge + 20, 20, 74, 70), Game1.controllerMaps, new Rectangle(512, 8, 74, 70), 1f);
			radioButtonButtonA = new ClickableTextureComponent(new Rectangle(radioButtonJoystick.bounds.X + radioButtonJoystick.bounds.Width + 8, radioButtonJoystick.bounds.Y, 74, 70), Game1.controllerMaps, new Rectangle(596, 8, 74, 70), 1f);
			radioButtonButtonB = new ClickableTextureComponent(new Rectangle(radioButtonButtonA.bounds.X + radioButtonButtonA.bounds.Width + 8, radioButtonJoystick.bounds.Y, 74, 70), Game1.controllerMaps, new Rectangle(512, 74, 74, 70), 1f);
			buttonDefaults = new OptionsButton(Game1.content.LoadString("Strings\\UI:Default"), OnClickSetToDefaults, 0, 20);
			buttonDefaults.bounds.Height = (buttonDefaults.button.bounds.Height = 70);
			sizeSlider = new OptionsSlider(Game1.content.LoadString("Strings\\UI:button_diameter"), 143, Game1.xEdge + 20, radioButtonJoystick.bounds.Y + radioButtonJoystick.bounds.Height + 8);
			sizeSlider.sliderMinValue = 20;
			sizeSlider.sliderMaxValue = 300;
			sizeSlider.value = sizeJoystick;
			_selectedBoxX = radioButtonJoystick.bounds.X - 4;
		}

		public void OnClickSetToDefaults()
		{
			SetJoystickDefaults();
			SetButtonBDefaults();
			SetButtonADefaults();
			UpdateSettings();
		}

		private void CheckToSetDefaults()
		{
			if (sizeJoystick == 0 || (positionJoystick.X == 0 && positionJoystick.Y == 0))
			{
				SetJoystickDefaults();
			}
			if (sizeButtonB == 0 || (positionButtonB.X == 0 && positionButtonB.Y == 0))
			{
				SetButtonBDefaults();
			}
			if (sizeButtonA == 0 || (positionButtonA.X == 0 && positionButtonA.Y == 0))
			{
				SetButtonADefaults();
			}
		}

		private void SetJoystickDefaults()
		{
			sizeJoystick = 185;
			int num = 64;
			_maxJoystickMoveRadius = sizeJoystick / 2;
			_joystickStartPosition = new Point(Game1.toolbar.itemSlotSize + _maxJoystickMoveRadius + num, screenHeight - sizeJoystick - _maxJoystickMoveRadius - num);
			SetPositionJoystick(_joystickStartPosition.X, _joystickStartPosition.Y);
			_centerOfJoystick.X = _joystickStartPosition.X + sizeJoystick / 2;
			_centerOfJoystick.Y = _joystickStartPosition.Y + sizeJoystick / 2;
		}

		private void SetButtonADefaults()
		{
			sizeButtonA = 111;
			SetPositionButtonA(screenWidth - sizeButtonB - 124 - sizeButtonA - 64, _joystickStartPosition.Y);
		}

		private void SetButtonBDefaults()
		{
			sizeButtonB = 111;
			SetPositionButtonB(screenWidth - sizeButtonB - 124, _joystickStartPosition.Y);
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			CheckToSetDefaults();
			UpdateSettings();
		}

		public void UpdateSettings()
		{
			UpdateJoystickAndButtonsSizePositionAndScale();
			joystick.bounds.X = positionJoystick.X;
			joystick.bounds.Y = positionJoystick.Y;
		}

		public void UpdateJoystickAndButtonsSizePositionAndScale()
		{
			UpdateButtonSizes();
			UpdateJoystickAndButtonsStartPositions();
			UpdateButtonScales();
		}

		private void UpdateJoystickAndButtonsStartPositions()
		{
			if (joystick != null)
			{
				_joystickStartPosition.X = positionJoystick.X;
				_joystickStartPosition.Y = positionJoystick.Y;
			}
			if (buttonA != null)
			{
				buttonA.bounds.X = positionButtonA.X;
				buttonA.bounds.Y = positionButtonA.Y;
			}
			if (buttonB != null)
			{
				buttonB.bounds.X = positionButtonB.X;
				buttonB.bounds.Y = positionButtonB.Y;
			}
		}

		private void SetAttackJoystickStartPosition()
		{
			_joystickStartPosition.X = screenWidth - 124 - sizeJoystick;
			_joystickStartPosition.Y = screenHeight - 124 - sizeJoystick;
			_centerOfJoystick.X = _joystickStartPosition.X + sizeJoystick / 2;
			_centerOfJoystick.Y = _joystickStartPosition.Y + sizeJoystick / 2;
		}

		private void SetInvisbleJoystickBounds()
		{
			buttonA.bounds.X = screenWidth - Game1.options.invisibleButtonWidth * 2;
			buttonA.bounds.Y = screenHeight / 2;
			buttonB.bounds.X = screenWidth - Game1.options.invisibleButtonWidth;
			buttonB.bounds.Y = screenHeight / 2;
		}

		private void SetInvisbleJoystickBoundsOneButton()
		{
			buttonA.bounds.X = screenWidth / 2;
			buttonA.bounds.Y = screenHeight / 2;
			buttonA.bounds.Width = screenWidth / 2;
			buttonB.bounds.X = screenWidth;
			buttonB.bounds.Y = screenHeight / 2;
			buttonB.bounds.Width = 0;
		}

		public bool TappedInvisibleAttackButtonA(int x, int y)
		{
			if (y > screenHeight / 2 && x > screenWidth - 2 * Game1.options.invisibleButtonWidth)
			{
				return x < screenWidth - Game1.options.invisibleButtonWidth;
			}
			return false;
		}

		public bool TappedInvisibleAttackButtonB(int x, int y)
		{
			if (y > screenHeight / 2)
			{
				return x > screenWidth - Game1.options.invisibleButtonWidth;
			}
			return false;
		}

		public bool TappedInvisibleSingleAttackButton(int x, int y)
		{
			if (x > screenWidth / 2)
			{
				if (Game1.options.verticalToolbar)
				{
					return true;
				}
				if (Game1.toolbar.alignTop)
				{
					if (y > Game1.toolbar.itemSlotSize)
					{
						return true;
					}
				}
				else if (y < screenHeight - Game1.toolbar.itemSlotSize)
				{
					return true;
				}
			}
			return false;
		}

		public bool OnTapInvisibleJoystick(int x, int y)
		{
			if (x < screenWidth / 2 && ((Game1.options.verticalToolbar && x > Game1.toolbar.itemSlotSize + Game1.toolbarPaddingX && !joystickHeld) || joystickHeld || (!Game1.options.verticalToolbar && y > Game1.toolbar.itemSlotSize)))
			{
				if (!Game1.options.verticalToolbar && !Game1.toolbar.alignTop && y > screenHeight - Game1.toolbar.itemSlotSize)
				{
					return false;
				}
				if (!joystickHeld)
				{
					_joystickTapPoint.X = (_joystickLastTapPoint.X = x);
					_joystickTapPoint.Y = (_joystickLastTapPoint.Y = y);
					_joystickStartPosition = _joystickTapPoint;
					joystickHeld = true;
				}
				if (justUsedSlingshot)
				{
					Game1.currentLocation.tapToMove.StopMoving();
					return false;
				}
				if (joystickHeld)
				{
					float num = Vector2.Distance(new Vector2(_joystickStartPosition.X, _joystickStartPosition.Y), new Vector2(x, y));
					if (num >= 20f)
					{
						float num2 = (float)Math.Atan2(y - _joystickStartPosition.Y, x - _joystickStartPosition.X);
						float angle = num2 / ((float)Math.PI * 2f) * 360f;
						Game1.currentLocation.tapToMove.MoveJoystickHeld(angle);
					}
					else
					{
						Game1.currentLocation.tapToMove.StopMoving();
						Game1.player.Halt();
					}
					_joystickLastTapPoint.X = x;
					_joystickLastTapPoint.Y = y;
					return true;
				}
			}
			return false;
		}

		public bool OnTapJoystick(int x, int y)
		{
			double num = Utility.Distance(_joystickStartPosition.X, _joystickStartPosition.Y, x, y);
			double num2 = Utility.Distance(joystick.bounds.X, joystick.bounds.Y, x, y);
			if (!joystickHeld && num2 <= (double)(sizeJoystick / 2))
			{
				_joystickTapPoint.X = (_joystickLastTapPoint.X = x);
				_joystickTapPoint.Y = (_joystickLastTapPoint.Y = y);
				joystickHeld = true;
			}
			double num3 = Utility.Distance(buttonA.bounds.X, buttonA.bounds.Y, x, y);
			double num4 = Utility.Distance(buttonB.bounds.X, buttonB.bounds.Y, x, y);
			if (joystickHeld && num3 > (double)(sizeButtonA / 2) && num4 > (double)(sizeButtonB / 2))
			{
				OnTapHeldJoystick(x, y);
				_joystickLastTapPoint.X = x;
				_joystickLastTapPoint.Y = y;
				return true;
			}
			return false;
		}

		private bool TappedButtonA(int x, int y)
		{
			double num = Utility.Distance(buttonA.bounds.X, buttonA.bounds.Y, x, y);
			if (num <= (double)(sizeButtonA / 2))
			{
				return true;
			}
			return false;
		}

		private bool TappedButtonB(int x, int y)
		{
			double num = Utility.Distance(buttonB.bounds.X, buttonB.bounds.Y, x, y);
			if (num <= (double)(sizeButtonB / 2))
			{
				return true;
			}
			return false;
		}

		private void OnTapHeldJoystick(int x, int y)
		{
			if (!showJoystick)
			{
				return;
			}
			float num = Vector2.Distance(new Vector2(_joystickTapPoint.X, _joystickTapPoint.Y), new Vector2(x, y));
			float num2 = Vector2.Distance(new Vector2(_joystickLastTapPoint.X, _joystickLastTapPoint.Y), new Vector2(x, y));
			if (num >= 20f && num2 <= (float)_maxJoystickMoveRadius)
			{
				float num3 = (float)Math.Atan2(y - _joystickTapPoint.Y, x - _joystickTapPoint.X);
				float angle = num3 / ((float)Math.PI * 2f) * 360f;
				if (!(Game1.currentMinigame is TargetGame) && justUsedSlingshot)
				{
					Game1.currentLocation.tapToMove.StopMoving();
					return;
				}
				if (Game1.options.weaponControl == 1)
				{
					Game1.currentLocation.tapToMove.OnButtonAHeld(angle);
				}
				else if (Game1.options.weaponControl == 4 || Game1.options.weaponControl == 7 || Game1.options.weaponControl == 6 || Game1.options.weaponControl == 8 || (Game1.options.weaponControl == 5 && Game1.player.CurrentTool != null && Game1.player.CurrentTool is MeleeWeapon && Game1.player.CurrentTool.InitialParentTileIndex != 47))
				{
					Game1.currentLocation.tapToMove.MoveJoystickHeld(angle);
				}
				num = Math.Min(num, (float)_maxJoystickMoveRadius * 0.6f);
				num3 = (float)Math.Atan2(_joystickTapPoint.Y - y, _joystickTapPoint.X - x);
				joystick.bounds.X = _joystickStartPosition.X - (int)(Math.Cos(num3) * (double)num);
				joystick.bounds.Y = _joystickStartPosition.Y - (int)(Math.Sin(num3) * (double)num);
				_joystickAngle = num3;
				mostRecentlyUsedControlType = ControlType.VIRTUAL_PAD;
			}
			else if (num < 20f)
			{
				Game1.currentLocation.tapToMove.StopMoving();
				Game1.player.Halt();
			}
		}

		private void UpdateJoystick()
		{
			if (joystickHeld)
			{
				return;
			}
			if (joystick.bounds.X != _joystickStartPosition.X)
			{
				joystick.bounds.X += (_joystickStartPosition.X - joystick.bounds.X) / 2;
				if (Math.Abs(joystick.bounds.X) - Math.Abs(_joystickStartPosition.X) < 2)
				{
					joystick.bounds.X = _joystickStartPosition.X;
				}
			}
			if (joystick.bounds.Y != _joystickStartPosition.Y)
			{
				joystick.bounds.Y += (_joystickStartPosition.Y - joystick.bounds.Y) / 2;
				if (Math.Abs(joystick.bounds.Y) - Math.Abs(_joystickStartPosition.Y) < 2)
				{
					joystick.bounds.Y = _joystickStartPosition.Y;
				}
			}
			_joystickLastTapPoint.X = joystick.bounds.X + joystick.bounds.Width / 2;
			_joystickLastTapPoint.Y = joystick.bounds.Y + joystick.bounds.Height / 2;
		}

		private void BackupSizeAndPositions()
		{
			_initialJoystickPosition = positionJoystick;
			_initialButtonAPosition = positionButtonA;
			_initialButtonBPosition = positionButtonB;
			_initialJoystickSize = sizeJoystick;
			_initialButtonASize = sizeButtonA;
			_initialButtonBSize = sizeButtonB;
		}

		private void RevertSizeAndPositions()
		{
			positionJoystick = _initialJoystickPosition;
			positionButtonA = _initialButtonAPosition;
			positionButtonB = _initialButtonBPosition;
			sizeJoystick = _initialJoystickSize;
			sizeButtonA = _initialButtonASize;
			sizeButtonB = _initialButtonBSize;
			UpdateJoystickAndButtonsSizePositionAndScale();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (adjustmentMode)
			{
				_leftButtonDown = true;
				_lastMouseX = x;
				_lastMouseY = y;
				Toolbar.toolbarPressed = true;
				if (radioButtonJoystick.containsPoint(x, y) || Utility.Distance(joystick.bounds.X, joystick.bounds.Y, x, y) < (double)(sizeJoystick / 2))
				{
					_selectedBoxX = radioButtonJoystick.bounds.X - 4;
					sizeSlider.whichOption = 143;
					sizeSlider.value = sizeJoystick;
					_selectedButton = joystick;
				}
				else if ((radioButtonButtonA.containsPoint(x, y) || Utility.Distance(buttonA.bounds.X, buttonA.bounds.Y, x, y) < (double)(sizeButtonA / 2)) && Game1.options.weaponControl != 1)
				{
					_selectedBoxX = radioButtonButtonA.bounds.X - 4;
					sizeSlider.whichOption = 144;
					sizeSlider.value = sizeButtonA;
					_selectedButton = buttonA;
				}
				else if ((radioButtonButtonB.containsPoint(x, y) || Utility.Distance(buttonB.bounds.X, buttonB.bounds.Y, x, y) < (double)(sizeButtonB / 2)) && Game1.options.weaponControl != 1)
				{
					_selectedBoxX = radioButtonButtonB.bounds.X - 4;
					sizeSlider.whichOption = 145;
					sizeSlider.value = sizeButtonB;
					_selectedButton = buttonB;
				}
				else if (buttonCancel.containsPoint(x, y))
				{
					adjustmentMode = false;
					Toolbar.toolbarPressed = false;
					RevertSizeAndPositions();
					Game1.exitActiveMenu();
				}
				else if (buttonTick.containsPoint(x, y))
				{
					adjustmentMode = false;
					Toolbar.toolbarPressed = false;
					OptionsPage.SaveStartupPreferences();
					Game1.exitActiveMenu();
				}
				else if (buttonDefaults.button.containsPoint(x, y))
				{
					buttonDefaults.receiveLeftClick(x, y);
					OnClickSetToDefaults();
				}
				else
				{
					sizeSlider.receiveLeftClick(x, y);
				}
				if (buttonDefaults.button.containsPoint(x, y))
				{
					buttonDefaults.receiveLeftClick(x, y);
					OnClickSetToDefaults();
				}
				return;
			}
			if (Game1.options.showToggleJoypadButton && buttonToggleJoypad.containsPoint(x, y))
			{
				Toolbar.toolbarPressed = true;
				showJoypad = !showJoypad;
			}
			if (!joystickHeld && !_joystickWasJustHeld && Game1.displayHUD && !Game1.player.isEating && (!Game1.currentLocation.tapToMove.TapHoldActive || Game1.options.weaponControl == 4 || Game1.options.weaponControl == 7 || Game1.options.weaponControl == 6 || Game1.options.weaponControl == 8 || (Game1.options.weaponControl == 5 && Game1.player.CurrentTool != null && Game1.player.CurrentTool is MeleeWeapon && Game1.player.CurrentTool.InitialParentTileIndex != 47)))
			{
				if (Game1.options.weaponControl == 2)
				{
					SetInvisbleJoystickBounds();
				}
				else if (Game1.options.weaponControl == 3)
				{
					SetInvisbleJoystickBoundsOneButton();
				}
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			Toolbar.toolbarPressed = false;
			_leftButtonDown = false;
			_selectedButton = null;
			if (adjustmentMode)
			{
				sizeSlider.releaseLeftClick(x, y);
				buttonDefaults.releaseLeftClick(x, y);
			}
			else if ((_joystickWasJustHeld && !_joystickHeld) || joystickHeld)
			{
				_joystickWasJustHeld = false;
			}
		}

		public override void update(GameTime time)
		{
			if (Game1.options == null || Game1.player == null || PinchZoom.Instance.Pinching)
			{
				return;
			}
			if (adjustmentMode)
			{
				int mouseX = Game1.getMouseX(ui_scale: true);
				int mouseY = Game1.getMouseY(ui_scale: true);
				UpdateSliderPosition(mouseX, mouseY);
				UpdateButtonSizes();
				MoveButtonPositions(mouseX, mouseY);
				UpdateButtonScales();
				_lastMouseX = mouseX;
				_lastMouseY = mouseY;
				return;
			}
			if (showJoystick && (Game1.options.weaponControl == 4 || Game1.options.weaponControl == 7 || Game1.options.weaponControl == 6 || Game1.options.weaponControl == 8 || (Game1.options.weaponControl == 5 && Game1.player.CurrentTool != null && Game1.player.CurrentTool is MeleeWeapon && Game1.player.CurrentTool.InitialParentTileIndex != 47)))
			{
				UpdateJoystick();
			}
			if (Game1.options.weaponControl != 0 && Game1.options.weaponControl != _lastWeaponControl)
			{
				_lastWeaponControl = Game1.options.weaponControl;
				if (!showJoypad)
				{
					showJoypad = true;
				}
			}
			else if (_lastWeaponControl == -1 && Game1.options.weaponControl == 0 && !showJoypad)
			{
				showJoypad = true;
			}
			if (Game1.currentMinigame == null && Game1.player.usingSlingshot && !buttonAHeld && !_touchingButtonA && (Game1.options.weaponControl == 4 || Game1.options.weaponControl == 6 || Game1.options.weaponControl == 8 || Game1.options.weaponControl == 5))
			{
				((Slingshot)Game1.player.CurrentTool).finish();
			}
		}

		private void UpdateSliderPosition(int currentMouseX, int currentMouseY)
		{
			if (_leftButtonDown)
			{
				sizeSlider.leftClickHeld(currentMouseX, currentMouseY);
			}
		}

		private void UpdateButtonSizes()
		{
			if (joystick.bounds.Width != sizeJoystick)
			{
				joystick.bounds.Width = (joystick.bounds.Height = sizeJoystick);
				_maxJoystickMoveRadius = sizeJoystick;
				_centerOfJoystick.X = _joystickStartPosition.X + sizeJoystick / 2;
				_centerOfJoystick.Y = _joystickStartPosition.Y + sizeJoystick / 2;
			}
			if (buttonA.bounds.Width != sizeButtonA)
			{
				buttonA.bounds.Width = (buttonA.bounds.Height = sizeButtonA);
			}
			if (buttonB.bounds.Width != sizeButtonB)
			{
				buttonB.bounds.Width = (buttonB.bounds.Height = sizeButtonB);
			}
		}

		private void UpdateButtonScales()
		{
			if (joystick.scale != joystickScale || buttonA.scale != buttonAScale || buttonB.scale != buttonBScale)
			{
				joystick.scale = joystickScale;
				buttonA.scale = buttonAScale;
				buttonB.scale = buttonBScale;
			}
		}

		private void MoveButtonPositions(int currentMouseX, int currentMouseY)
		{
			if (_leftButtonDown)
			{
				int num = currentMouseX - _lastMouseX;
				int num2 = currentMouseY - _lastMouseY;
				if (_selectedButton == joystick)
				{
					joystick.bounds.X += num;
					joystick.bounds.Y += num2;
					_joystickStartPosition.X = joystick.bounds.X;
					_joystickStartPosition.Y = joystick.bounds.Y;
					SetPositionJoystick(joystick.bounds.X, joystick.bounds.Y);
				}
				else if (_selectedButton == buttonA)
				{
					buttonA.bounds.X += num;
					buttonA.bounds.Y += num2;
					SetPositionButtonA(buttonA.bounds.X, buttonA.bounds.Y);
				}
				else if (_selectedButton == buttonB)
				{
					buttonB.bounds.X += num;
					buttonB.bounds.Y += num2;
					SetPositionButtonB(buttonB.bounds.X, buttonB.bounds.Y);
				}
			}
		}

		private void UpdateButtonToogleBounds()
		{
			if (Game1.options.verticalToolbar)
			{
				buttonToggleJoypad.bounds.X = Game1.toolbarPaddingX + Game1.toolbar.itemSlotSize + 48;
				buttonToggleJoypad.bounds.Y = 10;
			}
			else
			{
				buttonToggleJoypad.bounds.X = ((!(Game1.currentLocation is MineShaft)) ? Math.Max(8, Game1.toolbarPaddingX) : 0);
				if (Game1.toolbar.alignTop)
				{
					buttonToggleJoypad.bounds.Y = Game1.toolbar.itemSlotSize + 40;
				}
				else
				{
					buttonToggleJoypad.bounds.Y = 10;
				}
			}
			if (Game1.currentLocation is MineShaft)
			{
				buttonToggleJoypad.bounds.Y += 70;
			}
			else if (Game1.currentLocation is Club)
			{
				buttonToggleJoypad.bounds.Y += 80;
			}
			buttonToggleJoypad.bounds.X = (int)(float)buttonToggleJoypad.bounds.X;
			buttonToggleJoypad.bounds.Y = (int)(float)buttonToggleJoypad.bounds.Y;
		}

		public void CheckForManualWeaponControlTaps()
		{
			if (!adjustmentMode)
			{
				CheckForTapAttackJoystick();
				CheckForTapJoystickAndButtons();
			}
		}

		public void CheckForTapAttackJoystick()
		{
			if (Game1.options.weaponControl != 1)
			{
				return;
			}
			_touchingJoystick = false;
			if (Game1.player.CurrentTool == null || !(Game1.player.CurrentTool is MeleeWeapon))
			{
				return;
			}
			TouchCollection getTouchState = Game1.input.GetTouchState;
			for (int i = 0; i < getTouchState.Count; i++)
			{
				if (getTouchState[i].State == TouchLocationState.Pressed || getTouchState[i].State == TouchLocationState.Moved)
				{
					int x = (int)(getTouchState[i].Position.X / Game1.options.uiScale);
					int y = (int)(getTouchState[i].Position.Y / Game1.options.uiScale);
					_touchingJoystick = OnTapJoystick(x, y);
					_ = _touchingJoystick;
				}
			}
			if (!_touchingJoystick)
			{
				joystickHeld = false;
				Game1.currentLocation.tapToMove.OnButtonARelease();
			}
		}

		public void CheckForTapJoystickAndButtons()
		{
			if (Game1.options.weaponControl != 2 && Game1.options.weaponControl != 3 && Game1.options.weaponControl != 4 && Game1.options.weaponControl != 7 && Game1.options.weaponControl != 6 && Game1.options.weaponControl != 8 && (Game1.options.weaponControl != 5 || Game1.player.CurrentTool == null || !(Game1.player.CurrentTool is MeleeWeapon) || Game1.player.CurrentTool.InitialParentTileIndex == 47))
			{
				return;
			}
			_touchingJoystick = false;
			_touchingButtonA = false;
			_touchingButtonB = false;
			TouchCollection getTouchState = Game1.input.GetTouchState;
			for (int i = 0; i < getTouchState.Count; i++)
			{
				if (getTouchState[i].State != TouchLocationState.Pressed && getTouchState[i].State != TouchLocationState.Moved)
				{
					continue;
				}
				int x = (int)(getTouchState[i].Position.X / Game1.options.uiScale);
				int y = (int)(getTouchState[i].Position.Y / Game1.options.uiScale);
				if (Game1.options.weaponControl == 2)
				{
					if (OnTapInvisibleJoystick(x, y))
					{
						_touchingJoystick = true;
					}
					else if (TappedInvisibleAttackButtonA(x, y))
					{
						_touchingButtonA = true;
					}
					else if (TappedInvisibleAttackButtonB(x, y))
					{
						_touchingButtonB = true;
					}
				}
				else if (Game1.options.weaponControl == 3)
				{
					if (OnTapInvisibleJoystick(x, y))
					{
						_touchingJoystick = true;
					}
					if (TappedInvisibleSingleAttackButton(x, y))
					{
						_touchingButtonA = true;
					}
				}
				else if (OnTapJoystick(x, y))
				{
					_touchingJoystick = true;
				}
				else if (TappedButtonA(x, y) && Game1.options.weaponControl != 7)
				{
					_touchingButtonA = true;
				}
				else if (TappedButtonB(x, y) && Game1.options.weaponControl != 7)
				{
					_touchingButtonB = true;
				}
			}
			if (!_touchingJoystick && joystickHeld)
			{
				joystickHeld = false;
				Game1.currentLocation.tapToMove.StopMoving();
			}
			if (_touchingButtonA && !buttonAHeld)
			{
				buttonAHeld = true;
				if (Game1.options.weaponControl == 3)
				{
					buttonATouchStartTime = DateTime.Now.Ticks;
				}
				else
				{
					SetGrabTile();
					Game1.currentLocation.tapToMove.mobileKeyStates.SetUseTool(useTool: true);
				}
			}
			else if (Game1.options.weaponControl == 3)
			{
				if (!_touchingButtonA && buttonAHeld)
				{
					buttonAHeld = false;
					long num = DateTime.Now.Ticks - buttonATouchStartTime;
					if (num < 2500000)
					{
						Game1.currentLocation.tapToMove.DoLeftClick();
					}
					else
					{
						Game1.currentLocation.tapToMove.DoRightClick();
					}
				}
			}
			else if (_touchingButtonA && buttonAHeld && Game1.player.CurrentTool != null && !Game1.player.usingTool && Game1.player.CurrentTool.UpgradeLevel == 0)
			{
				Game1.currentLocation.tapToMove.DoLeftClick();
			}
			else if (_touchingButtonA && buttonAHeld)
			{
				Game1.currentLocation.tapToMove.mobileKeyStates.useToolButtonPressed = false;
				Game1.currentLocation.tapToMove.mobileKeyStates.useToolButtonReleased = false;
				Game1.currentLocation.tapToMove.mobileKeyStates.useToolHeld = true;
			}
			else if (!_touchingButtonA && buttonAHeld)
			{
				buttonAHeld = false;
				Game1.currentLocation.tapToMove.mobileKeyStates.SetUseTool(useTool: false);
			}
			if (_touchingButtonB && !buttonBHeld)
			{
				buttonBHeld = true;
				Game1.currentLocation.tapToMove.mobileKeyStates.actionButtonPressed = true;
				return;
			}
			if (buttonBHeld)
			{
				Game1.currentLocation.tapToMove.mobileKeyStates.actionButtonPressed = false;
			}
			if (!_touchingButtonB && buttonBHeld)
			{
				buttonBHeld = false;
			}
		}

		private void SetGrabTile()
		{
			if (Game1.player.ActiveObject is Furniture && Game1.currentLocation is DecoratableLocation)
			{
				Game1.currentLocation.tapToMove.grabTile = GrabTile;
			}
			else if (Game1.currentLocation is DecoratableLocation)
			{
				DecoratableLocation decoratableLocation = (DecoratableLocation)Game1.currentLocation;
				Vector2 grabTile = Game1.player.GetGrabTile();
				int x = (int)grabTile.X * 64 + 32;
				int y = (int)grabTile.Y * 64 + 32;
				Object objectAt = Game1.currentLocation.getObjectAt(x, y);
				if (objectAt is Furniture)
				{
					Game1.currentLocation.tapToMove.grabTile = grabTile;
				}
			}
			else if (Game1.currentLocation != null && Game1.player.ActiveObject != null && Game1.player.ActiveObject.isPlaceable())
			{
				Game1.currentLocation.tapToMove.grabTile = GrabTile;
			}
		}

		public override void draw(SpriteBatch b)
		{
			drawAdjusters(b);
			drawJoystickAndButtons(b);
		}

		public void drawAndUpdateToggleShowJoypadButton(SpriteBatch b)
		{
			UpdateButtonToogleBounds();
			drawToggleShowJoypadButton(b);
		}

		public void drawJustToggleShowJoypadButton(SpriteBatch b)
		{
			int num = Math.Max(12, Game1.xEdge);
			buttonToggleJoypad.bounds.X = (int)((float)screenWidth - 64f - (float)num - 80f);
			buttonToggleJoypad.bounds.Y = 12;
			drawToggleShowJoypadButton(b);
		}

		private void drawToggleShowJoypadButton(SpriteBatch b)
		{
			if (Game1.options.showToggleJoypadButton)
			{
				float num = 1f;
				if (!showJoypad)
				{
					num = 0.5f;
				}
				buttonToggleJoypad.draw(b, Color.White * num, 1E-07f);
			}
			else if (!showJoypad)
			{
				showJoypad = true;
			}
		}

		private void drawJoystickAndButtons(SpriteBatch b)
		{
			if (showJoystick || adjustmentMode)
			{
				UpdateButtonSizes();
				b.Draw(Game1.mobileSpriteSheet, joystick.bounds, _joystickSourceRect, joystickHeld ? (Color.White * buttonAlpha * 2f * buttonFadeAlpha) : (Color.White * buttonAlpha * buttonFadeAlpha), 0f, new Vector2(_joystickSourceRect.Width / 2, _joystickSourceRect.Height / 2), SpriteEffects.None, 0.001f);
				if (Game1.options.weaponControl == 4 || Game1.options.weaponControl == 8 || ((Game1.options.weaponControl == 5 || Game1.options.weaponControl == 6) && Game1.player.CurrentTool != null && Game1.player.CurrentTool is MeleeWeapon && Game1.player.CurrentTool.InitialParentTileIndex != 47))
				{
					b.Draw(Game1.mobileSpriteSheet, buttonA.bounds, _joystickSourceRect, ButtonAPressed ? (Color.White * buttonAlpha * 2f * buttonFadeAlpha) : (Color.White * buttonAlpha * buttonFadeAlpha), 0f, new Vector2(_joystickSourceRect.Width / 2, _joystickSourceRect.Height / 2), SpriteEffects.None, 0.001f);
					b.Draw(Game1.mobileSpriteSheet, buttonB.bounds, _joystickSourceRect, ButtonBPressed ? (Color.White * buttonAlpha * 2f * buttonFadeAlpha) : (Color.White * buttonAlpha * buttonFadeAlpha), 0f, new Vector2(_joystickSourceRect.Width / 2, _joystickSourceRect.Height / 2), SpriteEffects.None, 0.001f);
				}
				if (buttonFadeAlpha < 1f)
				{
					buttonFadeAlpha = Math.Min(buttonFadeAlpha + buttonFadeStep, 1f);
				}
			}
			else
			{
				buttonFadeAlpha = 0f;
			}
		}

		private void drawAdjusters(SpriteBatch b)
		{
			if (adjustmentMode)
			{
				IClickableMenu.drawTextureBox(b, Game1.xEdge, 0, screenWidth - 2 * Game1.xEdge, 244, Color.White);
				buttonCancel.bounds.X = screenWidth - Game1.xEdge - 100;
				buttonCancel.bounds.Y = 20;
				buttonCancel.draw(b);
				buttonTick.bounds.X = buttonCancel.bounds.X;
				buttonTick.bounds.Y = 144;
				buttonTick.draw(b);
				b.Draw(Game1.mouseCursors, new Rectangle(_selectedBoxX, 16, 82, 78), new Rectangle(150, 470, 4, 4), Color.Red, 0f, Vector2.Zero, SpriteEffects.None, 1E-06f);
				radioButtonJoystick.draw(b);
				radioButtonButtonA.draw(b);
				radioButtonButtonB.draw(b);
				b.Draw(Game1.controllerMaps, new Rectangle(radioButtonButtonA.bounds.X + 24, radioButtonButtonA.bounds.Y + 28, 26, 26), new Rectangle(570, 548, 26, 26), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1E-06f);
				b.Draw(Game1.controllerMaps, new Rectangle(radioButtonButtonB.bounds.X + 24, radioButtonButtonB.bounds.Y + 28, 26, 26), new Rectangle(570, 548, 26, 26), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1E-06f);
				buttonDefaults.bounds.X = (buttonDefaults.button.bounds.X = buttonCancel.bounds.X - buttonDefaults.bounds.Width - 44);
				buttonDefaults.draw(b, 0, 0);
				sizeSlider.bounds.Width = screenWidth - 2 * Game1.xEdge - 164;
				sizeSlider.buttonRightArrow.bounds.X = screenWidth - Game1.xEdge - 80 - 24;
				sizeSlider.draw(b, 0, 0);
			}
		}

		public void resetJoypad()
		{
			_touchingJoystick = false;
			_touchingButtonA = false;
			_touchingButtonB = false;
			joystickHeld = false;
			buttonAHeld = false;
			buttonBHeld = false;
		}
	}
}
