using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Mobile;

namespace StardewValley
{
	public class Options
	{
		public enum ItemStowingModes
		{
			Off,
			GamepadOnly,
			Both
		}

		public enum GamepadModes
		{
			Auto,
			ForceOn,
			ForceOff
		}

		public const int menuMargin = 133;

		public const int toolbarPaddingX = 134;

		public const int toggleVerticalToolbar = 135;

		public const int toggleAutoAttack = 136;

		public const int toggleGreenSquaresGuide = 137;

		public const int toggleVibrate = 138;

		public const int selectWeaponControl = 139;

		public const int toggleJoypadButtonVisibility = 140;

		public const int toggleBiggerNumberFont = 141;

		public const int toggleAutoSave = 142;

		public const int adjustSizeJoystick = 143;

		public const int adjustSizeButtonA = 144;

		public const int adjustSizeButtonB = 145;

		public const int adjustInvisibleButtonWidth = 146;

		public const int togglePinchZoom = 147;

		public const int adjustToolbarSlotSize = 148;

		public const int adjustDateTimeScale = 149;

		public const int toggleCameraButton = 150;

		public const int useBiggerFonts = 151;

		public bool verticalToolbar = true;

		public bool autoAttack;

		public bool greenSquaresGuide;

		public bool vibrations;

		public bool bigNumbers;

		public bool bigFonts;

		public bool autoSave;

		public int xEdge;

		public int toolbarPadding;

		public int weaponControl;

		public bool showToggleJoypadButton;

		public bool pinchZoom = true;

		public int invisibleButtonWidth;

		public int daysSinceReviewRequest;

		public bool reviewGiven;

		public int toolbarSlotSize;

		public float dateTimeScale;

		public int lastSeenBuildNumber;

		public bool showCameraButton;

		public SerializableDictionary<int, int[]> joystickConfigs;

		public int sizeJoystick;

		public int sizeButtonA;

		public int sizeButtonB;

		public Point positionJoystick;

		public Point positionButtonA;

		public Point positionButtonB;

		public const float minZoom = 0.75f;

		public const float maxZoom = 2f;

		public const float minUIZoom = 0.75f;

		public const float maxUIZoom = 1.5f;

		public const int toggleAutoRun = 0;

		public const int musicVolume = 1;

		public const int soundVolume = 2;

		public const int toggleDialogueTypingSounds = 3;

		public const int toggleFullscreen = 4;

		public const int toggleWindowedOrTrueFullscreen = 5;

		public const int screenResolution = 6;

		public const int showPortraitsToggle = 7;

		public const int showMerchantPortraitsToggle = 8;

		public const int menuBG = 9;

		public const int toggleFootsteps = 10;

		public const int alwaysShowToolHitLocationToggle = 11;

		public const int hideToolHitLocationWhenInMotionToggle = 12;

		public const int windowMode = 13;

		public const int pauseWhenUnfocused = 14;

		public const int pinToolbar = 15;

		public const int toggleRumble = 16;

		public const int ambientOnly = 17;

		public const int zoom = 18;

		public const int zoomButtonsToggle = 19;

		public const int ambientVolume = 20;

		public const int footstepVolume = 21;

		public const int invertScrollDirectionToggle = 22;

		public const int snowTransparencyToggle = 23;

		public const int screenFlashToggle = 24;

		public const int lightingQualityToggle = 25;

		public const int toggleHardwareCursor = 26;

		public const int toggleShowPlacementTileGamepad = 27;

		public const int stowingModeSelect = 28;

		public const int toggleSnappyMenus = 29;

		public const int toggleIPConnections = 30;

		public const int serverMode = 31;

		public const int toggleFarmhandCreation = 32;

		public const int toggleShowAdvancedCraftingInformation = 34;

		public const int toggleMPReadyStatus = 35;

		public const int mapScreenshot = 36;

		public const int toggleVsync = 37;

		public const int gamepadModeSelect = 38;

		public const int uiScaleSlider = 39;

		public const int moveBuildingPermissions = 40;

		public const int slingshotModeSelect = 41;

		public const int biteChime = 42;

		public const int toggleMuteAnimalSounds = 43;

		public const int input_actionButton = 7;

		public const int input_cancelButton = 9;

		public const int input_useToolButton = 10;

		public const int input_moveUpButton = 11;

		public const int input_moveRightButton = 12;

		public const int input_moveDownButton = 13;

		public const int input_moveLeftButton = 14;

		public const int input_menuButton = 15;

		public const int input_runButton = 16;

		public const int input_chatButton = 17;

		public const int input_journalButton = 18;

		public const int input_mapButton = 19;

		public const int input_slot1 = 20;

		public const int input_slot2 = 21;

		public const int input_slot3 = 22;

		public const int input_slot4 = 23;

		public const int input_slot5 = 24;

		public const int input_slot6 = 25;

		public const int input_slot7 = 26;

		public const int input_slot8 = 27;

		public const int input_slot9 = 28;

		public const int input_slot10 = 29;

		public const int input_slot11 = 30;

		public const int input_slot12 = 31;

		public const int input_toolbarSwap = 32;

		public const int input_emoteButton = 33;

		public const int checkBoxOption = 1;

		public const int sliderOption = 2;

		public const int dropDownOption = 3;

		public const float defaultZoomLevel = 1f;

		public const int defaultLightingQuality = 8;

		public const float defaultSplitScreenZoomLevel = 1f;

		public bool autoRun;

		public bool dialogueTyping;

		public bool showPortraits;

		public bool showMerchantPortraits;

		public bool showMenuBackground;

		public bool playFootstepSounds;

		public bool alwaysShowToolHitLocation;

		public bool hideToolHitLocationWhenInMotion;

		public bool pauseWhenOutOfFocus;

		public bool pinToolbarToggle;

		public bool mouseControls;

		public bool keyboardControls;

		public bool gamepadControls;

		public bool rumble;

		public bool ambientOnlyToggle;

		public bool zoomButtons;

		public bool invertScrollDirection;

		public bool screenFlash;

		public bool showPlacementTileForGamepad;

		public bool snappyMenus;

		public bool showAdvancedCraftingInformation;

		public bool showMPEndOfNightReadyStatus;

		public bool muteAnimalSounds;

		public bool vsyncEnabled;

		public bool fullscreen;

		public bool windowedBorderlessFullscreen;

		[DontLoadDefaultSetting]
		public bool ipConnectionsEnabled;

		[DontLoadDefaultSetting]
		public bool enableServer;

		[DontLoadDefaultSetting]
		public bool enableFarmhandCreation;

		protected bool _hardwareCursor;

		public ItemStowingModes stowingMode;

		[DontLoadDefaultSetting]
		public GamepadModes gamepadMode;

		public bool useLegacySlingshotFiring;

		public float musicVolumeLevel;

		public float soundVolumeLevel;

		public float footstepVolumeLevel;

		public float ambientVolumeLevel;

		public float snowTransparency;

		[XmlIgnore]
		public float baseZoomLevel = 1f;

		[DontLoadDefaultSetting]
		[XmlElement("zoomLevel")]
		public float singlePlayerBaseZoomLevel = 1f;

		[DontLoadDefaultSetting]
		public float localCoopBaseZoomLevel = 1f;

		[DontLoadDefaultSetting]
		[XmlElement("uiScale")]
		public float singlePlayerDesiredUIScale = -1f;

		[DontLoadDefaultSetting]
		public float localCoopDesiredUIScale = 1.5f;

		[XmlIgnore]
		public float baseUIScale = 1f;

		public int preferredResolutionX;

		public int preferredResolutionY;

		public int lightingQuality;

		[DontLoadDefaultSetting]
		public ServerPrivacy serverPrivacy = ServerPrivacy.FriendsOnly;

		public InputButton[] actionButton = new InputButton[2]
		{
			new InputButton(Keys.X),
			new InputButton(mouseLeft: false)
		};

		public InputButton[] cancelButton = new InputButton[1]
		{
			new InputButton(Keys.V)
		};

		public InputButton[] useToolButton = new InputButton[2]
		{
			new InputButton(Keys.C),
			new InputButton(mouseLeft: true)
		};

		public InputButton[] moveUpButton = new InputButton[1]
		{
			new InputButton(Keys.W)
		};

		public InputButton[] moveRightButton = new InputButton[1]
		{
			new InputButton(Keys.D)
		};

		public InputButton[] moveDownButton = new InputButton[1]
		{
			new InputButton(Keys.S)
		};

		public InputButton[] moveLeftButton = new InputButton[1]
		{
			new InputButton(Keys.A)
		};

		public InputButton[] menuButton = new InputButton[2]
		{
			new InputButton(Keys.E),
			new InputButton(Keys.Escape)
		};

		public InputButton[] runButton = new InputButton[1]
		{
			new InputButton(Keys.LeftShift)
		};

		public InputButton[] tmpKeyToReplace = new InputButton[1]
		{
			new InputButton(Keys.None)
		};

		public InputButton[] chatButton = new InputButton[2]
		{
			new InputButton(Keys.T),
			new InputButton(Keys.OemQuestion)
		};

		public InputButton[] mapButton = new InputButton[1]
		{
			new InputButton(Keys.M)
		};

		public InputButton[] journalButton = new InputButton[1]
		{
			new InputButton(Keys.F)
		};

		public InputButton[] inventorySlot1 = new InputButton[1]
		{
			new InputButton(Keys.D1)
		};

		public InputButton[] inventorySlot2 = new InputButton[1]
		{
			new InputButton(Keys.D2)
		};

		public InputButton[] inventorySlot3 = new InputButton[1]
		{
			new InputButton(Keys.D3)
		};

		public InputButton[] inventorySlot4 = new InputButton[1]
		{
			new InputButton(Keys.D4)
		};

		public InputButton[] inventorySlot5 = new InputButton[1]
		{
			new InputButton(Keys.D5)
		};

		public InputButton[] inventorySlot6 = new InputButton[1]
		{
			new InputButton(Keys.D6)
		};

		public InputButton[] inventorySlot7 = new InputButton[1]
		{
			new InputButton(Keys.D7)
		};

		public InputButton[] inventorySlot8 = new InputButton[1]
		{
			new InputButton(Keys.D8)
		};

		public InputButton[] inventorySlot9 = new InputButton[1]
		{
			new InputButton(Keys.D9)
		};

		public InputButton[] inventorySlot10 = new InputButton[1]
		{
			new InputButton(Keys.D0)
		};

		public InputButton[] inventorySlot11 = new InputButton[1]
		{
			new InputButton(Keys.OemMinus)
		};

		public InputButton[] inventorySlot12 = new InputButton[1]
		{
			new InputButton(Keys.OemPlus)
		};

		public InputButton[] toolbarSwap = new InputButton[1]
		{
			new InputButton(Keys.Tab)
		};

		public InputButton[] emoteButton = new InputButton[1]
		{
			new InputButton(Keys.Y)
		};

		[XmlIgnore]
		public bool optionsDirty;

		[XmlIgnore]
		private XmlSerializer defaultSettingsSerializer = new XmlSerializer(typeof(Options));

		private int appliedLightingQuality = -1;

		public int joystickSize
		{
			get
			{
				return joystickConfigs[weaponControl][0];
			}
			set
			{
				joystickConfigs[weaponControl][0] = value;
				sizeJoystick = value;
			}
		}

		public int buttonASize
		{
			get
			{
				return joystickConfigs[weaponControl][3];
			}
			set
			{
				joystickConfigs[weaponControl][3] = value;
				sizeButtonB = value;
			}
		}

		public int buttonBSize
		{
			get
			{
				return joystickConfigs[weaponControl][6];
			}
			set
			{
				joystickConfigs[weaponControl][6] = value;
				sizeButtonB = value;
			}
		}

		public Point joystickPosition
		{
			get
			{
				return new Point(joystickConfigs[weaponControl][1], joystickConfigs[weaponControl][2]);
			}
			set
			{
				joystickConfigs[weaponControl][1] = value.X;
				joystickConfigs[weaponControl][2] = value.Y;
				positionJoystick = value;
			}
		}

		public Point buttonAPosition
		{
			get
			{
				return new Point(joystickConfigs[weaponControl][4], joystickConfigs[weaponControl][5]);
			}
			set
			{
				joystickConfigs[weaponControl][4] = value.X;
				joystickConfigs[weaponControl][5] = value.Y;
				positionButtonA.X = value.X;
				positionButtonA.Y = value.Y;
			}
		}

		public Point buttonBPosition
		{
			get
			{
				return new Point(joystickConfigs[weaponControl][7], joystickConfigs[weaponControl][8]);
			}
			set
			{
				joystickConfigs[weaponControl][7] = value.X;
				joystickConfigs[weaponControl][8] = value.Y;
				positionButtonB.X = value.X;
				positionButtonB.Y = value.Y;
			}
		}

		public bool hardwareCursor
		{
			get
			{
				if (LocalMultiplayer.IsLocalMultiplayer())
				{
					return false;
				}
				return _hardwareCursor;
			}
			set
			{
				_hardwareCursor = value;
			}
		}

		[XmlIgnore]
		public float zoomLevel
		{
			get
			{
				if (Game1.game1.takingMapScreenshot)
				{
					return baseZoomLevel;
				}
				return baseZoomLevel * Game1.game1.zoomModifier;
			}
		}

		[XmlIgnore]
		public float desiredBaseZoomLevel
		{
			get
			{
				if (LocalMultiplayer.IsLocalMultiplayer() || !Game1.game1.IsMainInstance)
				{
					return localCoopBaseZoomLevel;
				}
				return singlePlayerBaseZoomLevel;
			}
			set
			{
				if (LocalMultiplayer.IsLocalMultiplayer() || !Game1.game1.IsMainInstance)
				{
					localCoopBaseZoomLevel = value;
				}
				else
				{
					singlePlayerBaseZoomLevel = value;
				}
			}
		}

		[XmlIgnore]
		public float desiredUIScale
		{
			get
			{
				if (Game1.gameMode != 3)
				{
					return 1f;
				}
				if (LocalMultiplayer.IsLocalMultiplayer() || !Game1.game1.IsMainInstance)
				{
					return localCoopDesiredUIScale;
				}
				return singlePlayerDesiredUIScale;
			}
			set
			{
				if (Game1.gameMode == 3)
				{
					if (LocalMultiplayer.IsLocalMultiplayer() || !Game1.game1.IsMainInstance)
					{
						localCoopDesiredUIScale = value;
					}
					else
					{
						singlePlayerDesiredUIScale = value;
					}
				}
			}
		}

		[XmlIgnore]
		public float uiScale => baseUIScale * Game1.game1.zoomModifier;

		public bool allowStowing
		{
			get
			{
				if (stowingMode == ItemStowingModes.Off)
				{
					return false;
				}
				if (stowingMode == ItemStowingModes.GamepadOnly)
				{
					if (gamepadControls)
					{
						return true;
					}
					return false;
				}
				return true;
			}
		}

		public bool SnappyMenus
		{
			get
			{
				if (snappyMenus && gamepadControls && Game1.input.GetMouseState().LeftButton != ButtonState.Pressed)
				{
					return Game1.input.GetMouseState().RightButton != ButtonState.Pressed;
				}
				return false;
			}
		}

		public void SetPositionJoystick(int x, int y)
		{
			joystickConfigs[weaponControl][1] = x;
			joystickConfigs[weaponControl][2] = y;
			positionJoystick.X = x;
			positionJoystick.Y = y;
		}

		public void SetPositionButtonA(int x, int y)
		{
			joystickConfigs[weaponControl][4] = x;
			joystickConfigs[weaponControl][5] = y;
			positionButtonA.X = x;
			positionButtonA.Y = y;
		}

		public void SetPositionButtonB(int x, int y)
		{
			joystickConfigs[weaponControl][7] = x;
			joystickConfigs[weaponControl][8] = y;
			positionButtonB.X = x;
			positionButtonB.Y = y;
		}

		private void setJoystickConfigsToDefault()
		{
			joystickConfigs = new SerializableDictionary<int, int[]>();
			joystickConfigs[0] = new int[9] { 185, 0, 0, 111, 0, 0, 111, 0, 0 };
			joystickConfigs[1] = new int[9] { 185, 0, 0, 111, 0, 0, 111, 0, 0 };
			joystickConfigs[2] = new int[9] { 185, 0, 0, 200, 0, 0, 200, 0, 0 };
			joystickConfigs[3] = new int[9] { 185, 0, 0, 200, 0, 0, 200, 0, 0 };
			joystickConfigs[4] = new int[9] { 185, 0, 0, 111, 0, 0, 111, 0, 0 };
			joystickConfigs[5] = new int[9] { 185, 0, 0, 111, 0, 0, 111, 0, 0 };
			joystickConfigs[6] = new int[9] { 185, 0, 0, 111, 0, 0, 111, 0, 0 };
			joystickConfigs[7] = new int[9] { 185, 0, 0, 111, 0, 0, 111, 0, 0 };
			joystickConfigs[8] = new int[9] { 185, 0, 0, 111, 0, 0, 111, 0, 0 };
		}

		private void setToDefaults_Mobile()
		{
			setJoystickConfigsToDefault();
			showToggleJoypadButton = false;
			pinchZoom = true;
			verticalToolbar = true;
			showCameraButton = false;
			greenSquaresGuide = true;
			rumble = (vibrations = false);
			weaponControl = 0;
			bigNumbers = false;
			autoSave = true;
			xEdge = Game1.xEdge;
			toolbarPadding = Game1.toolbarPaddingX;
			sizeJoystick = 185;
			sizeButtonA = 111;
			sizeButtonB = 111;
			invisibleButtonWidth = 200;
			daysSinceReviewRequest = -1;
			reviewGiven = false;
			useLegacySlingshotFiring = true;
			if (Game1.graphics.PreferredBackBufferHeight < 720)
			{
				toolbarSlotSize = (Game1.maxItemSlotSize = 60);
			}
			else if (Game1.graphics.PreferredBackBufferHeight < 960)
			{
				toolbarSlotSize = Math.Max(1, (int)Math.Floor((double)Game1.graphics.PreferredBackBufferHeight / 12.0));
			}
			else
			{
				toolbarSlotSize = (Game1.maxItemSlotSize = 80);
			}
			dateTimeScale = Game1.DefaultMenuButtonScale;
		}

		public Options()
		{
			setToDefaults();
		}

		public virtual void LoadDefaultOptions()
		{
			if (!Game1.game1.IsMainInstance)
			{
				return;
			}
			Options options = null;
			if (options == null)
			{
				return;
			}
			Type typeFromHandle = typeof(Options);
			FieldInfo[] fields = typeFromHandle.GetFields(BindingFlags.Instance | BindingFlags.Public);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.GetCustomAttributes(typeof(DontLoadDefaultSetting), inherit: true).Length == 0 && fieldInfo.GetCustomAttributes(typeof(XmlIgnoreAttribute), inherit: true).Length == 0)
				{
					fieldInfo.SetValue(this, fieldInfo.GetValue(options));
				}
			}
			PropertyInfo[] properties = typeFromHandle.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.GetCustomAttributes(typeof(DontLoadDefaultSetting), inherit: true).Length == 0 && propertyInfo.GetCustomAttributes(typeof(XmlIgnoreAttribute), inherit: true).Length == 0 && propertyInfo.GetSetMethod() != null && propertyInfo.GetGetMethod() != null)
				{
					propertyInfo.SetValue(this, propertyInfo.GetValue(options, null), null);
				}
			}
		}

		public virtual void SaveDefaultOptions()
		{
			optionsDirty = false;
			_ = Game1.game1.IsMainInstance;
		}

		public void platformClampValues()
		{
			showMerchantPortraits = false;
			autoRun = false;
			fullscreen = true;
			windowedBorderlessFullscreen = false;
			showPlacementTileForGamepad = false;
			desiredBaseZoomLevel = (baseZoomLevel = 1f);
			autoRun = true;
			desiredBaseZoomLevel = (baseZoomLevel = FetchZoom());
			singlePlayerDesiredUIScale = (baseUIScale = Game1.DefaultMenuButtonScale);
		}

		public float FetchZoom()
		{
			if (Game1.gameMode == 3 && Game1.currentLocation != null && !(Game1.currentLocation is MermaidHouse))
			{
				return PinchZoom.Instance.ZoomLevel;
			}
			return Game1.NativeZoomLevel;
		}

		public Keys getFirstKeyboardKeyFromInputButtonList(InputButton[] inputButton)
		{
			for (int i = 0; i < inputButton.Length; i++)
			{
				_ = ref inputButton[i];
				if (inputButton[i].key != 0)
				{
					return inputButton[i].key;
				}
			}
			return Keys.None;
		}

		public void reApplySetOptions()
		{
			platformClampValues();
			if (lightingQuality != appliedLightingQuality)
			{
				Program.gamePtr.refreshWindowSettings();
				appliedLightingQuality = lightingQuality;
			}
			Program.gamePtr.IsMouseVisible = hardwareCursor;
		}

		public void setToDefaults()
		{
			playFootstepSounds = true;
			showMenuBackground = false;
			showMerchantPortraits = true;
			showPortraits = true;
			autoRun = true;
			alwaysShowToolHitLocation = false;
			hideToolHitLocationWhenInMotion = true;
			dialogueTyping = true;
			rumble = true;
			fullscreen = false;
			pinToolbarToggle = false;
			baseZoomLevel = 1f;
			localCoopBaseZoomLevel = 1f;
			if (Game1.options == this)
			{
				Game1.forceSnapOnNextViewportUpdate = true;
			}
			zoomButtons = false;
			pauseWhenOutOfFocus = true;
			screenFlash = true;
			snowTransparency = 1f;
			invertScrollDirection = false;
			ambientOnlyToggle = false;
			showAdvancedCraftingInformation = false;
			stowingMode = ItemStowingModes.Off;
			useLegacySlingshotFiring = false;
			gamepadMode = GamepadModes.Auto;
			windowedBorderlessFullscreen = true;
			showPlacementTileForGamepad = true;
			lightingQuality = 8;
			hardwareCursor = false;
			musicVolumeLevel = 0.75f;
			ambientVolumeLevel = 0.75f;
			footstepVolumeLevel = 0.9f;
			soundVolumeLevel = 1f;
			preferredResolutionX = Game1.graphics.GraphicsDevice.Adapter.SupportedDisplayModes.Last().Width;
			preferredResolutionY = Game1.graphics.GraphicsDevice.Adapter.SupportedDisplayModes.Last().Height;
			vsyncEnabled = true;
			GameRunner.instance.OnWindowSizeChange(null, null);
			snappyMenus = true;
			ipConnectionsEnabled = true;
			enableServer = true;
			serverPrivacy = ServerPrivacy.FriendsOnly;
			enableFarmhandCreation = true;
			showMPEndOfNightReadyStatus = false;
			muteAnimalSounds = false;
			setToDefaults_Mobile();
		}

		public void setControlsToDefault()
		{
			actionButton = new InputButton[2]
			{
				new InputButton(Keys.X),
				new InputButton(mouseLeft: false)
			};
			cancelButton = new InputButton[1]
			{
				new InputButton(Keys.V)
			};
			useToolButton = new InputButton[2]
			{
				new InputButton(Keys.C),
				new InputButton(mouseLeft: true)
			};
			moveUpButton = new InputButton[1]
			{
				new InputButton(Keys.W)
			};
			moveRightButton = new InputButton[1]
			{
				new InputButton(Keys.D)
			};
			moveDownButton = new InputButton[1]
			{
				new InputButton(Keys.S)
			};
			moveLeftButton = new InputButton[1]
			{
				new InputButton(Keys.A)
			};
			menuButton = new InputButton[2]
			{
				new InputButton(Keys.E),
				new InputButton(Keys.Escape)
			};
			runButton = new InputButton[1]
			{
				new InputButton(Keys.LeftShift)
			};
			tmpKeyToReplace = new InputButton[1]
			{
				new InputButton(Keys.None)
			};
			chatButton = new InputButton[2]
			{
				new InputButton(Keys.T),
				new InputButton(Keys.OemQuestion)
			};
			mapButton = new InputButton[1]
			{
				new InputButton(Keys.M)
			};
			journalButton = new InputButton[1]
			{
				new InputButton(Keys.F)
			};
			inventorySlot1 = new InputButton[1]
			{
				new InputButton(Keys.D1)
			};
			inventorySlot2 = new InputButton[1]
			{
				new InputButton(Keys.D2)
			};
			inventorySlot3 = new InputButton[1]
			{
				new InputButton(Keys.D3)
			};
			inventorySlot4 = new InputButton[1]
			{
				new InputButton(Keys.D4)
			};
			inventorySlot5 = new InputButton[1]
			{
				new InputButton(Keys.D5)
			};
			inventorySlot6 = new InputButton[1]
			{
				new InputButton(Keys.D6)
			};
			inventorySlot7 = new InputButton[1]
			{
				new InputButton(Keys.D7)
			};
			inventorySlot8 = new InputButton[1]
			{
				new InputButton(Keys.D8)
			};
			inventorySlot9 = new InputButton[1]
			{
				new InputButton(Keys.D9)
			};
			inventorySlot10 = new InputButton[1]
			{
				new InputButton(Keys.D0)
			};
			inventorySlot11 = new InputButton[1]
			{
				new InputButton(Keys.OemMinus)
			};
			inventorySlot12 = new InputButton[1]
			{
				new InputButton(Keys.OemPlus)
			};
			emoteButton = new InputButton[1]
			{
				new InputButton(Keys.Y)
			};
			toolbarSwap = new InputButton[1]
			{
				new InputButton(Keys.Tab)
			};
		}

		public string getNameOfOptionFromIndex(int index)
		{
			return index switch
			{
				0 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4556"), 
				1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4557"), 
				2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4558"), 
				3 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4559"), 
				4 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4560"), 
				5 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4561"), 
				6 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4562"), 
				_ => "", 
			};
		}

		public int whatTypeOfOption(int index)
		{
			switch (index)
			{
			case 1:
			case 2:
				return 2;
			case 6:
				return 3;
			default:
				return 1;
			}
		}

		public void changeCheckBoxOption(int which, bool value)
		{
			switch (which)
			{
			case 0:
				autoRun = value;
				Game1.player.setRunning(autoRun);
				break;
			case 3:
				dialogueTyping = value;
				break;
			case 7:
				showPortraits = value;
				break;
			case 8:
				showMerchantPortraits = value;
				break;
			case 9:
				showMenuBackground = value;
				break;
			case 10:
				playFootstepSounds = value;
				break;
			case 11:
				alwaysShowToolHitLocation = value;
				break;
			case 12:
				hideToolHitLocationWhenInMotion = value;
				break;
			case 14:
				pauseWhenOutOfFocus = value;
				break;
			case 15:
				pinToolbarToggle = value;
				break;
			case 16:
				rumble = value;
				break;
			case 17:
				ambientOnlyToggle = value;
				break;
			case 19:
				zoomButtons = value;
				break;
			case 22:
				invertScrollDirection = value;
				break;
			case 24:
				screenFlash = value;
				break;
			case 26:
				hardwareCursor = value;
				Program.gamePtr.IsMouseVisible = hardwareCursor;
				break;
			case 27:
				showPlacementTileForGamepad = value;
				break;
			case 37:
				vsyncEnabled = value;
				GameRunner.instance.OnWindowSizeChange(null, null);
				break;
			case 29:
				snappyMenus = value;
				break;
			case 30:
				ipConnectionsEnabled = value;
				break;
			case 32:
				enableFarmhandCreation = value;
				Game1.server?.updateLobbyData();
				break;
			case 34:
				showAdvancedCraftingInformation = value;
				break;
			case 35:
				showMPEndOfNightReadyStatus = value;
				break;
			case 43:
				muteAnimalSounds = value;
				break;
			case 135:
				verticalToolbar = value;
				if (!verticalToolbar)
				{
					showCameraButton = false;
				}
				break;
			case 137:
				greenSquaresGuide = value;
				break;
			case 138:
				rumble = (vibrations = value);
				break;
			case 141:
				bigNumbers = value;
				break;
			case 151:
				bigFonts = value;
				checkForBiggerFontSwap();
				break;
			case 142:
				autoSave = value;
				if (autoSave)
				{
				}
				break;
			case 140:
				showToggleJoypadButton = value;
				break;
			case 147:
				if (!value)
				{
					PinchZoom.Instance.ZoomLevel = zoomLevel;
				}
				pinchZoom = value;
				break;
			case 150:
				showCameraButton = value;
				if (!verticalToolbar)
				{
					showCameraButton = false;
				}
				break;
			}
			optionsDirty = true;
		}

		public void checkForBiggerFontSwap()
		{
			if (bigFonts)
			{
				Game1.smallFont = Game1.content.Load<SpriteFont>("Fonts\\SpriteFont1");
				return;
			}
			Game1.smallFont = Game1.content.Load<SpriteFont>("Fonts\\SmallFont");
			Game1.dialogueFont = Game1.content.Load<SpriteFont>("Fonts\\SpriteFont1");
		}

		public void changeSliderOption(int which, int value)
		{
			switch (which)
			{
			case 1:
				musicVolumeLevel = (float)value / 100f;
				Game1.musicCategory.SetVolume(musicVolumeLevel);
				Game1.musicPlayerVolume = musicVolumeLevel;
				break;
			case 2:
				soundVolumeLevel = (float)value / 100f;
				Game1.soundCategory.SetVolume(soundVolumeLevel);
				break;
			case 20:
				ambientVolumeLevel = (float)value / 100f;
				Game1.ambientCategory.SetVolume(ambientVolumeLevel);
				Game1.ambientPlayerVolume = ambientVolumeLevel;
				break;
			case 21:
				footstepVolumeLevel = (float)value / 100f;
				Game1.footstepCategory.SetVolume(footstepVolumeLevel);
				break;
			case 23:
				snowTransparency = (float)value / 100f;
				break;
			case 39:
			{
				int num6 = (int)(desiredUIScale * 100f);
				int num7 = num6;
				int num8 = (int)((float)value * 100f);
				if (num8 >= num6 + 10 || num8 >= 100)
				{
					num6 += 10;
					num6 = Math.Min(100, num6);
				}
				else if (num8 <= num6 - 10 || num8 <= 50)
				{
					num6 -= 10;
					num6 = Math.Max(50, num6);
				}
				desiredUIScale = (float)num6 / 100f;
				break;
			}
			case 18:
			{
				int num3 = (int)(desiredBaseZoomLevel * 100f);
				int num4 = num3;
				int num5 = (int)((float)value * 100f);
				if (num5 >= num3 + 10 || num5 >= 100)
				{
					num3 += 10;
					num3 = Math.Min(100, num3);
				}
				else if (num5 <= num3 - 10 || num5 <= 50)
				{
					num3 -= 10;
					num3 = Math.Max(50, num3);
				}
				if (num3 != num4)
				{
					desiredBaseZoomLevel = (float)num3 / 100f;
					Game1.forceSnapOnNextViewportUpdate = true;
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4563") + zoomLevel);
				}
				break;
			}
			case 133:
				xEdge = value;
				break;
			case 134:
				Game1.toolbarPaddingX = (toolbarPadding = value);
				Game1.toolbar.resetToolbar();
				break;
			case 143:
				joystickSize = value;
				break;
			case 144:
				buttonASize = value;
				break;
			case 145:
				buttonBSize = value;
				break;
			case 146:
				invisibleButtonWidth = value;
				break;
			case 148:
			{
				int num2 = (toolbarSlotSize = (Game1.toolbar.itemSlotSize = (Game1.maxItemSlotSize = value)));
				Game1.toolbar.resetToolbar();
				break;
			}
			case 149:
				dateTimeScale = (float)value / 100f;
				break;
			}
			optionsDirty = true;
		}

		public void setStowingMode(string setting)
		{
			switch (setting)
			{
			case "off":
				stowingMode = ItemStowingModes.Off;
				break;
			case "gamepad":
				stowingMode = ItemStowingModes.GamepadOnly;
				break;
			case "both":
				stowingMode = ItemStowingModes.Both;
				break;
			}
		}

		public void setSlingshotMode(string setting)
		{
			if (setting == "legacy")
			{
				useLegacySlingshotFiring = true;
			}
			else
			{
				useLegacySlingshotFiring = false;
			}
		}

		public void setBiteChime(string setting)
		{
			try
			{
				Game1.player.biteChime.Value = int.Parse(setting);
			}
			catch (Exception)
			{
				Game1.player.biteChime.Value = -1;
			}
		}

		public void setGamepadMode(string setting)
		{
			switch (setting)
			{
			case "auto":
				gamepadMode = GamepadModes.Auto;
				break;
			case "force_on":
				gamepadMode = GamepadModes.ForceOn;
				break;
			case "force_off":
				gamepadMode = GamepadModes.ForceOff;
				break;
			}
		}

		public void setMoveBuildingPermissions(string setting)
		{
			if (setting == "off")
			{
				Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.Off;
			}
			if (setting == "on")
			{
				Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.On;
			}
			if (setting == "owned")
			{
				Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.OwnedBuildings;
			}
		}

		public void setServerMode(string setting)
		{
			switch (setting)
			{
			case "offline":
				enableServer = false;
				Game1.multiplayer.Disconnect(Multiplayer.DisconnectType.ServerOfflineMode);
				return;
			case "friends":
				serverPrivacy = ServerPrivacy.FriendsOnly;
				break;
			case "invite":
				serverPrivacy = ServerPrivacy.InviteOnly;
				break;
			}
			if (Game1.server == null && Game1.client == null)
			{
				enableServer = true;
				Game1.multiplayer.StartServer();
			}
			else if (Game1.server != null)
			{
				enableServer = true;
				Game1.server.setPrivacy(serverPrivacy);
			}
		}

		public void setWindowedOption(string setting)
		{
			switch (setting)
			{
			case "Windowed":
				setWindowedOption(1);
				break;
			case "Fullscreen":
				setWindowedOption(2);
				break;
			case "Windowed Borderless":
				setWindowedOption(0);
				break;
			}
		}

		public void setWindowedOption(int setting)
		{
			windowedBorderlessFullscreen = isCurrentlyWindowedBorderless();
			fullscreen = !windowedBorderlessFullscreen && Game1.graphics.IsFullScreen;
			int num = -1;
			switch (setting)
			{
			case 1:
				if (Game1.graphics.IsFullScreen && !windowedBorderlessFullscreen)
				{
					fullscreen = false;
					Game1.toggleNonBorderlessWindowedFullscreen();
					windowedBorderlessFullscreen = false;
				}
				else if (windowedBorderlessFullscreen)
				{
					fullscreen = false;
					windowedBorderlessFullscreen = false;
					Game1.toggleFullscreen();
				}
				num = 1;
				break;
			case 2:
				if (windowedBorderlessFullscreen)
				{
					fullscreen = true;
					windowedBorderlessFullscreen = false;
					Game1.toggleFullscreen();
				}
				else if (!Game1.graphics.IsFullScreen)
				{
					fullscreen = true;
					windowedBorderlessFullscreen = false;
					Game1.toggleNonBorderlessWindowedFullscreen();
					hardwareCursor = false;
					Program.gamePtr.IsMouseVisible = false;
				}
				num = 2;
				break;
			case 0:
				if (!windowedBorderlessFullscreen)
				{
					windowedBorderlessFullscreen = true;
					Game1.toggleFullscreen();
					fullscreen = false;
				}
				num = 0;
				break;
			}
			_ = Game1.gameMode;
			_ = 3;
		}

		public void changeDropDownOption(int which, string value)
		{
			switch (which)
			{
			case 25:
				switch (value)
				{
				case "Lowest":
					lightingQuality = 128;
					break;
				case "Low":
					lightingQuality = 64;
					break;
				case "Med.":
					lightingQuality = 32;
					break;
				case "High":
					lightingQuality = 16;
					break;
				case "Ultra":
					lightingQuality = 8;
					break;
				}
				Game1.overrideGameMenuReset = true;
				Program.gamePtr.refreshWindowSettings();
				Game1.overrideGameMenuReset = false;
				break;
			case 39:
			{
				string text2 = value;
				text2 = text2.Replace("%", "");
				int num3 = Convert.ToInt32(text2);
				desiredUIScale = (float)num3 / 100f;
				break;
			}
			case 18:
			{
				string text = value;
				text = text.Replace("%", "");
				int num2 = Convert.ToInt32(text);
				desiredBaseZoomLevel = (float)num2 / 100f;
				Game1.forceSnapOnNextViewportUpdate = true;
				if (Game1.debrisWeather != null)
				{
					Game1.randomizeDebrisWeatherPositions(Game1.debrisWeather);
				}
				Game1.randomizeRainPositions();
				break;
			}
			case 6:
			{
				Rectangle rectangle = new Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height);
				Rectangle rectangle2 = new Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height);
				int preferredBackBufferWidth = Convert.ToInt32(value.Split(' ')[0]);
				int preferredBackBufferHeight = Convert.ToInt32(value.Split(' ')[2]);
				preferredResolutionX = preferredBackBufferWidth;
				preferredResolutionY = preferredBackBufferHeight;
				Game1.graphics.PreferredBackBufferWidth = preferredBackBufferWidth;
				Game1.graphics.PreferredBackBufferHeight = preferredBackBufferHeight;
				Game1.graphics.ApplyChanges();
				GameRunner.instance.OnWindowSizeChange(null, null);
				break;
			}
			case 13:
				setWindowedOption(value);
				break;
			case 31:
				setServerMode(value);
				break;
			case 28:
				setStowingMode(value);
				break;
			case 38:
				setGamepadMode(value);
				break;
			case 40:
				setMoveBuildingPermissions(value);
				break;
			case 41:
				setSlingshotMode(value);
				break;
			case 42:
				setBiteChime(value);
				Game1.player.PlayFishBiteChime();
				break;
			case 139:
			{
				int num = 0;
				if (value != null)
				{
					switch (value.Length)
					{
					case 4:
						if (value == "auto")
						{
							num = 0;
						}
						break;
					case 8:
						if (value == "joystick")
						{
							num = 1;
						}
						break;
					case 17:
						if (value == "invisibleJoystick")
						{
							num = 2;
						}
						break;
					case 26:
						if (value == "InvisibleJoystickOneButton")
						{
							num = 3;
						}
						break;
					case 18:
						if (value == "joystickAndButtons")
						{
							num = 4;
						}
						break;
					case 29:
						if (value == "joystickAndButtonsWeaponsOnly")
						{
							num = 5;
						}
						break;
					case 28:
						if (value == "JoystickAndButtonsForWeapons")
						{
							num = 6;
						}
						break;
					case 12:
						if (value == "JoystickOnly")
						{
							num = 7;
						}
						break;
					case 23:
						if (value == "joystickAndButtonsNoTap")
						{
							num = 8;
						}
						break;
					}
				}
				if (weaponControl != num)
				{
					weaponControl = num;
					if (Game1.currentLocation != null && Game1.currentLocation.tapToMove != null)
					{
						Game1.currentLocation.tapToMove.Reset();
					}
				}
				Game1.virtualJoypad.resetJoypad();
				if (Game1.virtualJoypad.positionJoystick == Point.Zero)
				{
					Game1.virtualJoypad.OnClickSetToDefaults();
				}
				break;
			}
			}
			optionsDirty = true;
		}

		public bool isKeyInUse(Keys key)
		{
			foreach (InputButton allUsedInputButton in getAllUsedInputButtons())
			{
				if (allUsedInputButton.key == key)
				{
					return true;
				}
			}
			return false;
		}

		public List<InputButton> getAllUsedInputButtons()
		{
			List<InputButton> list = new List<InputButton>();
			list.AddRange(useToolButton);
			list.AddRange(actionButton);
			list.AddRange(moveUpButton);
			list.AddRange(moveRightButton);
			list.AddRange(moveDownButton);
			list.AddRange(moveLeftButton);
			list.AddRange(runButton);
			list.AddRange(menuButton);
			list.AddRange(journalButton);
			list.AddRange(mapButton);
			list.AddRange(chatButton);
			list.AddRange(inventorySlot1);
			list.AddRange(inventorySlot2);
			list.AddRange(inventorySlot3);
			list.AddRange(inventorySlot4);
			list.AddRange(inventorySlot5);
			list.AddRange(inventorySlot6);
			list.AddRange(inventorySlot7);
			list.AddRange(inventorySlot8);
			list.AddRange(inventorySlot9);
			list.AddRange(inventorySlot10);
			list.AddRange(inventorySlot11);
			list.AddRange(inventorySlot12);
			list.AddRange(toolbarSwap);
			list.AddRange(emoteButton);
			return list;
		}

		public void setCheckBoxToProperValue(OptionsCheckbox checkbox)
		{
			switch (checkbox.whichOption)
			{
			case 0:
				checkbox.isChecked = autoRun;
				break;
			case 3:
				checkbox.isChecked = dialogueTyping;
				break;
			case 4:
				fullscreen = Game1.graphics.IsFullScreen || windowedBorderlessFullscreen;
				checkbox.isChecked = fullscreen;
				break;
			case 5:
				checkbox.isChecked = windowedBorderlessFullscreen;
				checkbox.greyedOut = !fullscreen;
				break;
			case 7:
				checkbox.isChecked = showPortraits;
				break;
			case 8:
				checkbox.isChecked = showMerchantPortraits;
				break;
			case 9:
				checkbox.isChecked = showMenuBackground;
				break;
			case 10:
				checkbox.isChecked = playFootstepSounds;
				break;
			case 11:
				checkbox.isChecked = alwaysShowToolHitLocation;
				break;
			case 12:
				checkbox.isChecked = hideToolHitLocationWhenInMotion;
				break;
			case 14:
				checkbox.isChecked = pauseWhenOutOfFocus;
				break;
			case 15:
				checkbox.isChecked = pinToolbarToggle;
				break;
			case 135:
				checkbox.isChecked = verticalToolbar;
				break;
			case 137:
				checkbox.isChecked = greenSquaresGuide;
				break;
			case 138:
				checkbox.isChecked = vibrations;
				break;
			case 141:
				checkbox.isChecked = bigNumbers;
				break;
			case 151:
				checkbox.isChecked = bigFonts;
				break;
			case 142:
				checkbox.isChecked = autoSave;
				break;
			case 140:
				checkbox.isChecked = showToggleJoypadButton;
				break;
			case 147:
				checkbox.isChecked = pinchZoom;
				break;
			case 150:
				checkbox.isChecked = showCameraButton;
				break;
			case 16:
				checkbox.isChecked = rumble;
				checkbox.greyedOut = !gamepadControls;
				break;
			case 17:
				checkbox.isChecked = ambientOnlyToggle;
				break;
			case 19:
				checkbox.isChecked = zoomButtons;
				break;
			case 22:
				checkbox.isChecked = invertScrollDirection;
				break;
			case 24:
				checkbox.isChecked = screenFlash;
				break;
			case 26:
				checkbox.isChecked = _hardwareCursor;
				checkbox.greyedOut = fullscreen;
				break;
			case 27:
				checkbox.isChecked = showPlacementTileForGamepad;
				checkbox.greyedOut = !gamepadControls;
				break;
			case 29:
				checkbox.isChecked = snappyMenus;
				break;
			case 30:
				checkbox.isChecked = ipConnectionsEnabled;
				break;
			case 32:
				checkbox.isChecked = enableFarmhandCreation;
				break;
			case 34:
				checkbox.isChecked = showAdvancedCraftingInformation;
				break;
			case 35:
				checkbox.isChecked = showMPEndOfNightReadyStatus;
				break;
			case 37:
				checkbox.isChecked = vsyncEnabled;
				break;
			case 43:
				checkbox.isChecked = muteAnimalSounds;
				break;
			}
		}

		public void setPlusMinusToProperValue(OptionsPlusMinus plusMinus)
		{
			switch (plusMinus.whichOption)
			{
			case 39:
			{
				string value3 = Math.Round(desiredUIScale * 100f) + "%";
				for (int k = 0; k < plusMinus.options.Count; k++)
				{
					if (plusMinus.options[k].Equals(value3))
					{
						plusMinus.selected = k;
						break;
					}
				}
				break;
			}
			case 18:
			{
				string value2 = Math.Round(desiredBaseZoomLevel * 100f) + "%";
				for (int j = 0; j < plusMinus.options.Count; j++)
				{
					if (plusMinus.options[j].Equals(value2))
					{
						plusMinus.selected = j;
						break;
					}
				}
				break;
			}
			case 25:
			{
				string value = "";
				switch (lightingQuality)
				{
				case 128:
					value = "Lowest";
					break;
				case 64:
					value = "Low";
					break;
				case 32:
					value = "Med.";
					break;
				case 16:
					value = "High";
					break;
				case 8:
					value = "Ultra";
					break;
				}
				for (int i = 0; i < plusMinus.options.Count; i++)
				{
					if (plusMinus.options[i].Equals(value))
					{
						plusMinus.selected = i;
						break;
					}
				}
				break;
			}
			}
		}

		public void setSliderToProperValue(OptionsSlider slider)
		{
			switch (slider.whichOption)
			{
			case 1:
				slider.value = (int)(musicVolumeLevel * 100f);
				break;
			case 2:
				slider.value = (int)(soundVolumeLevel * 100f);
				break;
			case 20:
				slider.value = (int)(ambientVolumeLevel * 100f);
				break;
			case 21:
				slider.value = (int)(footstepVolumeLevel * 100f);
				break;
			case 23:
				slider.value = (int)(snowTransparency * 100f);
				break;
			case 18:
				slider.value = (int)(desiredBaseZoomLevel * 100f);
				break;
			case 39:
				slider.value = (int)(desiredUIScale * 100f);
				break;
			case 133:
				slider.sliderMaxValue = 90;
				slider.value = Game1.xEdge;
				break;
			case 134:
				slider.value = Game1.toolbarPaddingX;
				break;
			case 146:
				slider.sliderMaxValue = (int)Math.Floor((float)Game1.viewport.Width / 4f);
				slider.value = invisibleButtonWidth;
				break;
			case 148:
				slider.value = toolbarSlotSize;
				break;
			case 149:
				slider.value = (int)(dateTimeScale * 100f);
				break;
			}
		}

		public bool doesInputListContain(InputButton[] list, Keys key)
		{
			for (int i = 0; i < list.Length; i++)
			{
				if (list[i].key == key)
				{
					return true;
				}
			}
			return false;
		}

		public void changeInputListenerValue(int whichListener, Keys key)
		{
			switch (whichListener)
			{
			case 7:
				actionButton[0] = new InputButton(key);
				break;
			case 17:
				chatButton[0] = new InputButton(key);
				break;
			case 15:
				menuButton[0] = new InputButton(key);
				break;
			case 13:
				moveDownButton[0] = new InputButton(key);
				break;
			case 14:
				moveLeftButton[0] = new InputButton(key);
				break;
			case 12:
				moveRightButton[0] = new InputButton(key);
				break;
			case 11:
				moveUpButton[0] = new InputButton(key);
				break;
			case 16:
				runButton[0] = new InputButton(key);
				break;
			case 10:
				useToolButton[0] = new InputButton(key);
				break;
			case 18:
				journalButton[0] = new InputButton(key);
				break;
			case 19:
				mapButton[0] = new InputButton(key);
				break;
			case 20:
				inventorySlot1[0] = new InputButton(key);
				break;
			case 21:
				inventorySlot2[0] = new InputButton(key);
				break;
			case 22:
				inventorySlot3[0] = new InputButton(key);
				break;
			case 23:
				inventorySlot4[0] = new InputButton(key);
				break;
			case 24:
				inventorySlot5[0] = new InputButton(key);
				break;
			case 25:
				inventorySlot6[0] = new InputButton(key);
				break;
			case 26:
				inventorySlot7[0] = new InputButton(key);
				break;
			case 27:
				inventorySlot8[0] = new InputButton(key);
				break;
			case 28:
				inventorySlot9[0] = new InputButton(key);
				break;
			case 29:
				inventorySlot10[0] = new InputButton(key);
				break;
			case 30:
				inventorySlot11[0] = new InputButton(key);
				break;
			case 31:
				inventorySlot12[0] = new InputButton(key);
				break;
			case 32:
				toolbarSwap[0] = new InputButton(key);
				break;
			case 33:
				emoteButton[0] = new InputButton(key);
				break;
			}
			optionsDirty = true;
		}

		public void setInputListenerToProperValue(OptionsInputListener inputListener)
		{
			inputListener.buttonNames.Clear();
			switch (inputListener.whichOption)
			{
			case 7:
			{
				InputButton[] array18 = actionButton;
				for (int num12 = 0; num12 < array18.Length; num12++)
				{
					InputButton inputButton18 = array18[num12];
					inputListener.buttonNames.Add(inputButton18.ToString());
				}
				break;
			}
			case 17:
			{
				InputButton[] array2 = chatButton;
				for (int j = 0; j < array2.Length; j++)
				{
					InputButton inputButton2 = array2[j];
					inputListener.buttonNames.Add(inputButton2.ToString());
				}
				break;
			}
			case 15:
			{
				InputButton[] array10 = menuButton;
				for (int num4 = 0; num4 < array10.Length; num4++)
				{
					InputButton inputButton10 = array10[num4];
					inputListener.buttonNames.Add(inputButton10.ToString());
				}
				break;
			}
			case 13:
			{
				InputButton[] array22 = moveDownButton;
				for (int num16 = 0; num16 < array22.Length; num16++)
				{
					InputButton inputButton22 = array22[num16];
					inputListener.buttonNames.Add(inputButton22.ToString());
				}
				break;
			}
			case 14:
			{
				InputButton[] array14 = moveLeftButton;
				for (int num8 = 0; num8 < array14.Length; num8++)
				{
					InputButton inputButton14 = array14[num8];
					inputListener.buttonNames.Add(inputButton14.ToString());
				}
				break;
			}
			case 12:
			{
				InputButton[] array6 = moveRightButton;
				for (int n = 0; n < array6.Length; n++)
				{
					InputButton inputButton6 = array6[n];
					inputListener.buttonNames.Add(inputButton6.ToString());
				}
				break;
			}
			case 11:
			{
				InputButton[] array24 = moveUpButton;
				for (int num18 = 0; num18 < array24.Length; num18++)
				{
					InputButton inputButton24 = array24[num18];
					inputListener.buttonNames.Add(inputButton24.ToString());
				}
				break;
			}
			case 16:
			{
				InputButton[] array20 = runButton;
				for (int num14 = 0; num14 < array20.Length; num14++)
				{
					InputButton inputButton20 = array20[num14];
					inputListener.buttonNames.Add(inputButton20.ToString());
				}
				break;
			}
			case 10:
			{
				InputButton[] array16 = useToolButton;
				for (int num10 = 0; num10 < array16.Length; num10++)
				{
					InputButton inputButton16 = array16[num10];
					inputListener.buttonNames.Add(inputButton16.ToString());
				}
				break;
			}
			case 32:
			{
				InputButton[] array12 = toolbarSwap;
				for (int num6 = 0; num6 < array12.Length; num6++)
				{
					InputButton inputButton12 = array12[num6];
					inputListener.buttonNames.Add(inputButton12.ToString());
				}
				break;
			}
			case 18:
			{
				InputButton[] array8 = journalButton;
				for (int num2 = 0; num2 < array8.Length; num2++)
				{
					InputButton inputButton8 = array8[num2];
					inputListener.buttonNames.Add(inputButton8.ToString());
				}
				break;
			}
			case 19:
			{
				InputButton[] array4 = mapButton;
				for (int l = 0; l < array4.Length; l++)
				{
					InputButton inputButton4 = array4[l];
					inputListener.buttonNames.Add(inputButton4.ToString());
				}
				break;
			}
			case 20:
			{
				InputButton[] array25 = inventorySlot1;
				for (int num19 = 0; num19 < array25.Length; num19++)
				{
					InputButton inputButton25 = array25[num19];
					inputListener.buttonNames.Add(inputButton25.ToString());
				}
				break;
			}
			case 21:
			{
				InputButton[] array23 = inventorySlot2;
				for (int num17 = 0; num17 < array23.Length; num17++)
				{
					InputButton inputButton23 = array23[num17];
					inputListener.buttonNames.Add(inputButton23.ToString());
				}
				break;
			}
			case 22:
			{
				InputButton[] array21 = inventorySlot3;
				for (int num15 = 0; num15 < array21.Length; num15++)
				{
					InputButton inputButton21 = array21[num15];
					inputListener.buttonNames.Add(inputButton21.ToString());
				}
				break;
			}
			case 23:
			{
				InputButton[] array19 = inventorySlot4;
				for (int num13 = 0; num13 < array19.Length; num13++)
				{
					InputButton inputButton19 = array19[num13];
					inputListener.buttonNames.Add(inputButton19.ToString());
				}
				break;
			}
			case 24:
			{
				InputButton[] array17 = inventorySlot5;
				for (int num11 = 0; num11 < array17.Length; num11++)
				{
					InputButton inputButton17 = array17[num11];
					inputListener.buttonNames.Add(inputButton17.ToString());
				}
				break;
			}
			case 25:
			{
				InputButton[] array15 = inventorySlot6;
				for (int num9 = 0; num9 < array15.Length; num9++)
				{
					InputButton inputButton15 = array15[num9];
					inputListener.buttonNames.Add(inputButton15.ToString());
				}
				break;
			}
			case 26:
			{
				InputButton[] array13 = inventorySlot7;
				for (int num7 = 0; num7 < array13.Length; num7++)
				{
					InputButton inputButton13 = array13[num7];
					inputListener.buttonNames.Add(inputButton13.ToString());
				}
				break;
			}
			case 27:
			{
				InputButton[] array11 = inventorySlot8;
				for (int num5 = 0; num5 < array11.Length; num5++)
				{
					InputButton inputButton11 = array11[num5];
					inputListener.buttonNames.Add(inputButton11.ToString());
				}
				break;
			}
			case 28:
			{
				InputButton[] array9 = inventorySlot9;
				for (int num3 = 0; num3 < array9.Length; num3++)
				{
					InputButton inputButton9 = array9[num3];
					inputListener.buttonNames.Add(inputButton9.ToString());
				}
				break;
			}
			case 29:
			{
				InputButton[] array7 = inventorySlot10;
				for (int num = 0; num < array7.Length; num++)
				{
					InputButton inputButton7 = array7[num];
					inputListener.buttonNames.Add(inputButton7.ToString());
				}
				break;
			}
			case 30:
			{
				InputButton[] array5 = inventorySlot11;
				for (int m = 0; m < array5.Length; m++)
				{
					InputButton inputButton5 = array5[m];
					inputListener.buttonNames.Add(inputButton5.ToString());
				}
				break;
			}
			case 31:
			{
				InputButton[] array3 = inventorySlot12;
				for (int k = 0; k < array3.Length; k++)
				{
					InputButton inputButton3 = array3[k];
					inputListener.buttonNames.Add(inputButton3.ToString());
				}
				break;
			}
			case 33:
			{
				InputButton[] array = emoteButton;
				for (int i = 0; i < array.Length; i++)
				{
					InputButton inputButton = array[i];
					inputListener.buttonNames.Add(inputButton.ToString());
				}
				break;
			}
			case 8:
			case 9:
				break;
			}
		}

		public void setDropDownToProperValue(OptionsDropDown dropDown)
		{
			switch (dropDown.whichOption)
			{
			case 6:
			{
				int num = 0;
				foreach (DisplayMode supportedDisplayMode in Game1.graphics.GraphicsDevice.Adapter.SupportedDisplayModes)
				{
					if (supportedDisplayMode.Width >= 1280)
					{
						dropDown.dropDownOptions.Add(supportedDisplayMode.Width + " x " + supportedDisplayMode.Height);
						dropDown.dropDownDisplayOptions.Add(supportedDisplayMode.Width + " x " + supportedDisplayMode.Height);
						if (supportedDisplayMode.Width == preferredResolutionX && supportedDisplayMode.Height == preferredResolutionY)
						{
							dropDown.selectedOption = num;
						}
						num++;
					}
				}
				dropDown.greyedOut = !fullscreen || windowedBorderlessFullscreen;
				break;
			}
			case 13:
				windowedBorderlessFullscreen = isCurrentlyWindowedBorderless();
				fullscreen = Game1.graphics.IsFullScreen && !windowedBorderlessFullscreen;
				dropDown.dropDownOptions.Add("Windowed");
				if (!windowedBorderlessFullscreen)
				{
					dropDown.dropDownOptions.Add("Fullscreen");
				}
				if (!fullscreen)
				{
					dropDown.dropDownOptions.Add("Windowed Borderless");
				}
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4564"));
				if (!windowedBorderlessFullscreen)
				{
					dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4560"));
				}
				if (!fullscreen)
				{
					dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4561"));
				}
				if (Game1.graphics.IsFullScreen || windowedBorderlessFullscreen)
				{
					dropDown.selectedOption = 1;
				}
				else
				{
					dropDown.selectedOption = 0;
				}
				break;
			case 28:
				dropDown.dropDownOptions.Add("off");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_StowingMode_Off"));
				dropDown.dropDownOptions.Add("gamepad");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_StowingMode_GamepadOnly"));
				dropDown.dropDownOptions.Add("both");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_StowingMode_On"));
				if (stowingMode == ItemStowingModes.Off)
				{
					dropDown.selectedOption = 0;
				}
				else if (stowingMode == ItemStowingModes.GamepadOnly)
				{
					dropDown.selectedOption = 1;
				}
				else if (stowingMode == ItemStowingModes.Both)
				{
					dropDown.selectedOption = 2;
				}
				break;
			case 38:
				dropDown.dropDownOptions.Add("auto");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_GamepadMode_Auto"));
				dropDown.dropDownOptions.Add("force_on");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_GamepadMode_ForceOn"));
				dropDown.dropDownOptions.Add("force_off");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_GamepadMode_ForceOff"));
				if (gamepadMode == GamepadModes.Auto)
				{
					dropDown.selectedOption = 0;
				}
				else if (gamepadMode == GamepadModes.ForceOn)
				{
					dropDown.selectedOption = 1;
				}
				else if (gamepadMode == GamepadModes.ForceOff)
				{
					dropDown.selectedOption = 2;
				}
				break;
			case 41:
				dropDown.dropDownOptions.Add("hold");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_SlingshotMode_Hold"));
				dropDown.dropDownOptions.Add("legacy");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_SlingshotMode_Pull"));
				if (useLegacySlingshotFiring)
				{
					dropDown.selectedOption = 1;
				}
				else
				{
					dropDown.selectedOption = 0;
				}
				break;
			case 42:
			{
				dropDown.dropDownOptions.Add("-1");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:BiteChime_Default"));
				for (int i = 0; i <= 3; i++)
				{
					dropDown.dropDownOptions.Add(i.ToString());
					dropDown.dropDownDisplayOptions.Add((i + 1).ToString());
				}
				dropDown.selectedOption = Game1.player.biteChime.Value + 1;
				break;
			}
			case 31:
				dropDown.dropDownOptions.Add("offline");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode_Offline"));
				if (Program.sdk.Networking != null)
				{
					dropDown.dropDownOptions.Add("friends");
					dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode_FriendsOnly"));
					dropDown.dropDownOptions.Add("invite");
					dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode_InviteOnly"));
				}
				else
				{
					dropDown.dropDownOptions.Add("online");
					dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode_Online"));
				}
				if (Game1.server == null)
				{
					dropDown.selectedOption = 0;
				}
				else if (Program.sdk.Networking != null)
				{
					if (serverPrivacy == ServerPrivacy.FriendsOnly)
					{
						dropDown.selectedOption = 1;
					}
					else if (serverPrivacy == ServerPrivacy.InviteOnly)
					{
						dropDown.selectedOption = 2;
					}
				}
				else
				{
					dropDown.selectedOption = 1;
				}
				Console.WriteLine("setDropDownToProperValue( serverMode, {0} ) called.", dropDown.dropDownOptions[dropDown.selectedOption]);
				break;
			case 40:
				dropDown.dropDownOptions.Add("on");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_MoveBuildingPermissions_On"));
				dropDown.dropDownOptions.Add("owned");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_MoveBuildingPermissions_Owned"));
				dropDown.dropDownOptions.Add("off");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_MoveBuildingPermissions_Off"));
				if (Game1.player.team.farmhandsCanMoveBuildings.Value == FarmerTeam.RemoteBuildingPermissions.On)
				{
					dropDown.selectedOption = 0;
				}
				if (Game1.player.team.farmhandsCanMoveBuildings.Value == FarmerTeam.RemoteBuildingPermissions.OwnedBuildings)
				{
					dropDown.selectedOption = 1;
				}
				if (Game1.player.team.farmhandsCanMoveBuildings.Value == FarmerTeam.RemoteBuildingPermissions.Off)
				{
					dropDown.selectedOption = 2;
				}
				break;
			case 139:
				dropDown.dropDownOptions.Add("auto");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:control_method_tap_to_move"));
				dropDown.dropDownOptions.Add("joystick");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:control_method_attack_joystick"));
				dropDown.dropDownOptions.Add("invisibleJoystick");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:control_method_invis_2"));
				dropDown.dropDownOptions.Add("InvisibleJoystickOneButton");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:control_method_invis_1"));
				dropDown.dropDownOptions.Add("joystickAndButtons");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:control_method_joystick_buttons"));
				dropDown.dropDownOptions.Add("joystickAndButtonsWeaponsOnly");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:control_method_joystick_buttons_b"));
				dropDown.dropDownOptions.Add("JoystickAndButtonsForWeapons");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:control_method_joystick_buttons_c"));
				dropDown.dropDownOptions.Add("JoystickOnly");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:control_method_joystick"));
				dropDown.dropDownOptions.Add("joystickAndButtonsNoTap");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:control_method_joystick_buttons_no_tap_to_move"));
				dropDown.selectedOption = weaponControl;
				break;
			}
		}

		public bool isCurrentlyWindowedBorderless()
		{
			if (Game1.graphics.IsFullScreen)
			{
				return !Game1.graphics.HardwareModeSwitch;
			}
			return false;
		}

		public bool isCurrentlyFullscreen()
		{
			if (Game1.graphics.IsFullScreen)
			{
				return Game1.graphics.HardwareModeSwitch;
			}
			return false;
		}

		public bool isCurrentlyWindowed()
		{
			if (!isCurrentlyWindowedBorderless())
			{
				return !isCurrentlyFullscreen();
			}
			return false;
		}
	}
}
