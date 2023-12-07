using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Characters;
using StardewValley.Minigames;
using StardewValley.Objects;

namespace StardewValley.Menus
{
	public class MobileCustomizer : IClickableMenu
	{
		public const int region_nameBox = 536;

		public const int region_farmNameBox = 537;

		public const int colorPickerTimerDelay = 100;

		private int currentShirt;

		private int currentHair;

		private int currentAccessory;

		private int colorPickerTimer;

		public MobileColorPicker pantsColorPicker;

		public MobileColorPicker hairColorPicker;

		public MobileColorPicker eyeColorPicker;

		public List<ClickableComponent> labels = new List<ClickableComponent>();

		public ClickableTextureComponent topLeftSelectButton;

		public ClickableTextureComponent topRightSelectButton;

		public ClickableTextureComponent bottomLeftSelectButton;

		public ClickableTextureComponent bottomRightSelectButton;

		public List<ClickableComponent> genderButtons = new List<ClickableComponent>();

		public List<ClickableComponent> appearanceButtons = new List<ClickableComponent>();

		public List<ClickableComponent> colorPickerCCs = new List<ClickableComponent>();

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent skipIntroButton;

		public ClickableTextureComponent randomButton;

		public ClickableTextureComponent advancedOptionsButton;

		private TextBox nameBox;

		private TextBox farmnameBox;

		private TextBox favThingBox;

		public ClickableComponent nameBoxCC;

		public ClickableComponent farmnameBoxCC;

		public ClickableComponent favThingBoxCC;

		private bool skipIntro;

		private string hoverText;

		private string hoverTitle;

		private CharacterCustomization.Source source;

		private int numAppearanceButtons = 8;

		private int currentAppearanceButton;

		private bool showNotFinishedMessage;

		private int notFinishedTimer;

		private const int NOT_FINISHED_COUNT = 3000;

		private MobileColorPicker _sliderOpTarget;

		private MobileColorPicker lastHeldColorPicker;

		private Action _sliderAction;

		private readonly Action _recolorEyesAction;

		private readonly Action _recolorPantsAction;

		private readonly Action _recolorHairAction;

		private float widthMod;

		private float heightMod;

		private Rectangle portraitBackBox;

		private Rectangle nameBoxRect;

		private Rectangle faveBoxRect;

		private Rectangle farmBoxRect;

		private Rectangle toolsBackBox;

		private Rectangle okPos;

		private string farmNameSuffix;

		private string animalText;

		private int farmNameSuffixLength;

		private Vector2 portraitPos;

		private Vector2 dicePos;

		private Vector2 back1Pos;

		private Vector2 forward1Pos;

		private Vector2 animalTextPos;

		private Vector2 catPos;

		private Vector2 dogPos;

		private Vector2 malePos;

		private Vector2 femalePos;

		private Vector2 sliderTextLeftPos;

		private Vector2 sliderTextRightPos;

		private Rectangle topLeftSelectPos;

		private Rectangle topRightSelectPos;

		private Rectangle bottomLeftSelectPos;

		private Rectangle bottomRightSelectPos;

		private string[] buttonLabels;

		private SliderBar selectSlider;

		private SliderBar animalSlider;

		private int[] numOptions = new int[5] { 24, 56, 112, 20, 4 };

		private int[] oldSliderValue;

		private bool holdingSlider;

		private string nameMessage;

		private string faveMessage;

		private string farmMessage;

		private MobileFarmChooser farmChooser;

		private const float widthModThreshold = 0.95f;

		private int tempPlayerHair;

		private Color tempPlayerHairColor;

		private int tempPlayerShirt;

		private int templPlayerAccessory;

		private Color tempPlayerEyeColor;

		private Color tempPlayerPantsColor;

		private int tempPlayerSkinColor;

		private int animalSliderWidth;

		private const int numPetBreeds = 6;

		protected bool _isDyeMenu;

		protected Farmer _displayFarmer;

		protected Clothing _itemToDye;

		private int _tempSkin;

		private int _tempShirt;

		private Texture2D skinColors;

		private Color[] skinColorsData;

		private Color _actualSkinColor;

		private bool isModifyingExistingPet;

		private bool petChanged;

		private Texture2D shirtsTexture;

		private bool haveReceivedLeftClick;

		private int timesRandom;

		private bool InTutorial
		{
			get
			{
				TutorialManager instance = TutorialManager.Instance;
				if (instance.currentTutorial != null && instance.showTheTutorials)
				{
					return true;
				}
				return false;
			}
		}

		public void DyeItem(Color color)
		{
			if (_itemToDye != null)
			{
				_itemToDye.Dye(color, 1f);
				_displayFarmer.FarmerRenderer.MarkSpriteDirty();
			}
		}

		public Farmer GetOrCreateDisplayFarmer()
		{
			if (_displayFarmer == null)
			{
				if (source == CharacterCustomization.Source.ClothesDye || source == CharacterCustomization.Source.DyePots)
				{
					_displayFarmer = Game1.player.CreateFakeEventFarmer();
				}
				else
				{
					_displayFarmer = Game1.player;
				}
				if (source == CharacterCustomization.Source.NewFarmhand)
				{
					if (_displayFarmer.pants.Value == -1)
					{
						_displayFarmer.pants.Value = _displayFarmer.GetPantsIndex();
					}
					if (_displayFarmer.shirt.Value == -1)
					{
						_displayFarmer.shirt.Value = _displayFarmer.GetShirtIndex();
					}
				}
				_displayFarmer.faceDirection(2);
				_displayFarmer.FarmerSprite.StopAnimation();
			}
			return _displayFarmer;
		}

		private void setUpSkinColorData()
		{
			skinColors = Game1.temporaryContent.Load<Texture2D>("Characters\\Farmer\\skinColors");
			skinColorsData = new Color[skinColors.Width * skinColors.Height];
			skinColors.GetData(0, skinColors.Bounds, skinColorsData, 0, skinColorsData.Length);
		}

		private Color getSkinColor(int which)
		{
			if (skinColorsData == null)
			{
				setUpSkinColorData();
			}
			return skinColorsData[which * 3 % (skinColors.Height * 3) + 2];
		}

		private void setUpShirts()
		{
			shirtsTexture = Game1.temporaryContent.Load<Texture2D>("Characters\\Farmer\\shirts");
		}

		public MobileCustomizer(int x, int y, int width, int height, CharacterCustomization.Source source = CharacterCustomization.Source.NewGame, Clothing item = null)
			: base(x, y, width, height)
		{
			this.source = source;
			if (Game1.player.whichPetBreed >= 3)
			{
				Game1.player.catPerson = true;
			}
			else
			{
				Game1.player.catPerson = false;
			}
			petChanged = false;
			isModifyingExistingPet = false;
			if (source == CharacterCustomization.Source.Wizard)
			{
				Pet characterFromName = Game1.getCharacterFromName<Pet>(Game1.player.getPetName(), mustBeVillager: false);
				if (characterFromName != null)
				{
					Game1.player.whichPetBreed = characterFromName.whichBreed;
					Game1.player.catPerson = characterFromName is Cat;
					isModifyingExistingPet = true;
				}
			}
			numOptions[1] = Farmer.GetAllHairstyleIndices().Count;
			setUpSkinColorData();
			setUpShirts();
			_tempSkin = Game1.player.skin;
			_tempShirt = Game1.player.shirt;
			_actualSkinColor = getSkinColor(_tempSkin);
			if (source == CharacterCustomization.Source.ClothesDye || source == CharacterCustomization.Source.DyePots)
			{
				currentAppearanceButton = 7;
			}
			else
			{
				currentAppearanceButton = 0;
			}
			if (source == CharacterCustomization.Source.ClothesDye && item != null)
			{
				_itemToDye = item;
				_displayFarmer = GetOrCreateDisplayFarmer();
				_recolorPantsAction = delegate
				{
					DyeItem(pantsColorPicker.getSelectedColor());
				};
				if (_itemToDye.clothesType.Value == 0)
				{
					_displayFarmer.shirtItem.Set(_itemToDye);
				}
				else if (_itemToDye.clothesType.Value == 1)
				{
					_displayFarmer.pantsItem.Set(_itemToDye);
				}
				_displayFarmer.UpdateClothing();
			}
			else
			{
				_recolorPantsAction = delegate
				{
					Game1.player.changePants(pantsColorPicker.getSelectedColor());
				};
			}
			widthMod = (float)base.width / 1280f;
			heightMod = (float)base.height / 720f;
			nameMessage = Game1.content.LoadString("Strings\\UI:Character_Name").Replace("\n", " ");
			faveMessage = Game1.content.LoadString("Strings\\UI:Character_FavoriteThing").Replace("\n", " ");
			farmMessage = Game1.content.LoadString("Strings\\UI:Character_Farm").Replace("\n", " ");
			if (Game1.player.name == "")
			{
				Game1.player.name.Value = nameMessage;
			}
			if (Game1.player.favoriteThing == null || Game1.player.favoriteThing == "")
			{
				Game1.player.favoriteThing.Value = faveMessage;
			}
			setUpPositions();
			if (farmnameBox != null && Game1.player.farmName == "")
			{
				Game1.player.farmName.Value = farmMessage;
			}
			Game1.player.faceDirection(2);
			Game1.player.FarmerSprite.StopAnimation();
			_recolorEyesAction = delegate
			{
				Game1.player.changeEyeColor(eyeColorPicker.getSelectedColor());
			};
			_recolorHairAction = delegate
			{
				Game1.player.changeHairColor(hairColorPicker.getSelectedColor());
			};
			int num = 0;
			if (source == CharacterCustomization.Source.ClothesDye || source == CharacterCustomization.Source.DyePots)
			{
				_isDyeMenu = true;
				switch (source)
				{
				case CharacterCustomization.Source.ClothesDye:
					num = 1;
					break;
				case CharacterCustomization.Source.DyePots:
					if (Game1.player.pantsItem.Value != null && Game1.player.pantsItem.Value.dyeable.Value)
					{
						num++;
					}
					if (Game1.player.shirtItem.Value != null && Game1.player.shirtItem.Value.dyeable.Value)
					{
						num++;
					}
					break;
				}
			}
			if (source == CharacterCustomization.Source.DyePots)
			{
				_displayFarmer = GetOrCreateDisplayFarmer();
				_recolorHairAction = delegate
				{
					if (Game1.player.shirtItem.Value != null && Game1.player.shirtItem.Value.dyeable.Value)
					{
						Game1.player.shirtItem.Value.clothesColor.Value = hairColorPicker.getSelectedColor();
						Game1.player.FarmerRenderer.MarkSpriteDirty();
						_displayFarmer.FarmerRenderer.MarkSpriteDirty();
					}
				};
				_recolorPantsAction = delegate
				{
					if (Game1.player.pantsItem.Value != null && Game1.player.pantsItem.Value.dyeable.Value)
					{
						Game1.player.pantsItem.Value.clothesColor.Value = pantsColorPicker.getSelectedColor();
						Game1.player.FarmerRenderer.MarkSpriteDirty();
						_displayFarmer.FarmerRenderer.MarkSpriteDirty();
					}
				};
			}
			setUpPositions();
			okButton.SetSnapAutomatic();
			nameBoxCC.SetSnapAutomatic();
			favThingBoxCC.SetSnapAutomatic();
			if (farmnameBoxCC != null)
			{
				farmnameBoxCC.SetSnapAutomatic();
				nameBoxCC.downNeighborID = 537;
				favThingBoxCC.upNeighborID = 537;
			}
			genderButtons.ForEach(delegate(ClickableComponent c)
			{
				c.SetSnapAutomatic();
			});
			appearanceButtons.ForEach(delegate(ClickableComponent c)
			{
				c.SetSnapAutomatic();
			});
			if (skipIntroButton != null)
			{
				skipIntroButton.SetSnapAutomatic();
			}
			randomButton.SetSnapAutomatic();
			advancedOptionsButton.SetSnapAutomatic();
			topLeftSelectButton.SetSnapAutomatic();
			topRightSelectButton.SetSnapAutomatic();
			bottomLeftSelectButton.SetSnapAutomatic();
			bottomRightSelectButton.SetSnapAutomatic();
			populateClickableComponentList();
			snapToDefaultClickableComponent();
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = nameBoxCC;
			snapCursorToCurrentSnappedComponent();
		}

		private void setUpPositions()
		{
			int num = 72;
			int num2 = 16;
			int num3 = (width - 32) / numAppearanceButtons - num2;
			int num4 = (int)(88f * heightMod);
			int num5 = xPositionOnScreen + 16 + num2 / 2;
			int num6 = (int)(465f * heightMod) + yPositionOnScreen;
			int num7 = 64;
			oldSliderValue = new int[5];
			for (int i = 0; i < oldSliderValue.Length; i++)
			{
				oldSliderValue[i] = 0;
			}
			if (source == CharacterCustomization.Source.Wizard || source == CharacterCustomization.Source.DyePots || source == CharacterCustomization.Source.ClothesDye)
			{
				initializeUpperRightCloseButton();
				if (source == CharacterCustomization.Source.Wizard)
				{
					upperRightCloseButton = null;
				}
				tempPlayerHair = GetCurrentHairIndex();
				tempPlayerHairColor = Game1.player.hairstyleColor;
				tempPlayerShirt = Game1.player.shirt;
				templPlayerAccessory = Game1.player.accessory;
				tempPlayerEyeColor = Game1.player.newEyeColor;
				tempPlayerPantsColor = Game1.player.pantsColor;
				tempPlayerSkinColor = Game1.player.skin;
				portraitBackBox = new Rectangle(Game1.uiViewport.Width / 2 - 180, (int)(100f * heightMod) + yPositionOnScreen, 360, 264);
				portraitPos = new Vector2(portraitBackBox.X + (portraitBackBox.Width - 128) / 2, portraitBackBox.Y + 32);
				int num8 = portraitBackBox.Width;
				int x = portraitBackBox.X + portraitBackBox.Width + 16;
				back1Pos = new Vector2(portraitBackBox.X + 48, portraitPos.Y + 78f);
				forward1Pos = new Vector2(portraitBackBox.X + portraitBackBox.Width - 16 - 64, portraitPos.Y + 78f);
				malePos = new Vector2((int)(portraitPos.X - 4f), (int)(portraitPos.Y + 192f + 16f));
				femalePos = new Vector2((int)(portraitPos.X + 64f + 4f), (int)(portraitPos.Y + 192f + 16f));
				dicePos = new Vector2(portraitBackBox.X + portraitBackBox.Width / 2 - 30, 32f * heightMod + (float)yPositionOnScreen);
				okPos = new Rectangle((int)((float)(xPositionOnScreen + width) - 40f * widthMod - 80f), (int)(592f * heightMod) + yPositionOnScreen, 80, 80);
				toolsBackBox = new Rectangle((int)(148f * widthMod) + xPositionOnScreen, (int)(582f * heightMod) + yPositionOnScreen, width - (int)(296f * widthMod), (int)(100f * heightMod));
				nameBoxRect = new Rectangle(x, portraitBackBox.Y, num8, num7);
				farmBoxRect = new Rectangle(x, portraitBackBox.Y + portraitBackBox.Height / 4, num8, num7);
				faveBoxRect = new Rectangle(x, portraitBackBox.Y + portraitBackBox.Height / 2, num8, num7);
				animalSliderWidth = farmBoxRect.Width * 2 / 3;
				catPos = new Vector2(portraitBackBox.X + num8 + 32, portraitBackBox.Y + portraitBackBox.Height / 2 - 32);
				dogPos = new Vector2(portraitBackBox.X + num8 + 32, portraitBackBox.Y + portraitBackBox.Height / 2 - 32);
				num6 -= 16;
				num4 += 16;
			}
			else if (source == CharacterCustomization.Source.NewGame)
			{
				portraitBackBox = new Rectangle(Game1.xEdge + 16 + (int)(16f * widthMod), (int)((float)num * heightMod) + yPositionOnScreen, 280, 320);
				portraitPos = new Vector2(portraitBackBox.X + (portraitBackBox.Width - 128) / 2, portraitBackBox.Y + 32);
				int num8 = portraitBackBox.Width + 60;
				int x = portraitBackBox.X + portraitBackBox.Width + 16;
				farmChooser = new MobileFarmChooser(x + num8 + 16, (int)((float)num * heightMod) + yPositionOnScreen, width - x - num8 - 52 + Game1.xEdge, 320, source, isStandaloneScreen: false);
				back1Pos = new Vector2(portraitBackBox.X + 32, portraitPos.Y + 78f);
				forward1Pos = new Vector2(portraitBackBox.X + portraitBackBox.Width - 64, portraitPos.Y + 78f);
				malePos = new Vector2((int)(portraitPos.X - 4f), (int)(portraitPos.Y + 192f + 16f));
				femalePos = new Vector2((int)(portraitPos.X + 64f + 4f), (int)(portraitPos.Y + 192f + 16f));
				dicePos = new Vector2(portraitBackBox.X + portraitBackBox.Width / 2 - 30, 32f * heightMod + (float)yPositionOnScreen);
				okPos = new Rectangle((int)((float)(xPositionOnScreen + width) - 40f * widthMod - 80f), (int)(592f * heightMod) + yPositionOnScreen, 80, 80);
				toolsBackBox = new Rectangle((int)(148f * widthMod) + xPositionOnScreen, (int)(582f * heightMod) + yPositionOnScreen, width - (int)(296f * widthMod), (int)(100f * heightMod));
				farmNameSuffix = Game1.content.LoadString("Strings\\UI:Character_FarmNameSuffix");
				farmNameSuffixLength = (int)Game1.dialogueFont.MeasureString(farmNameSuffix).X;
				nameBoxRect = new Rectangle(x, portraitBackBox.Y, num8, num7);
				farmBoxRect = new Rectangle(x, portraitBackBox.Y + portraitBackBox.Height / 4, num8 - farmNameSuffixLength - 8, num7);
				faveBoxRect = new Rectangle(x, portraitBackBox.Y + portraitBackBox.Height / 2, num8, num7);
				animalSliderWidth = farmBoxRect.Width;
				catPos = new Vector2(nameBoxRect.X + nameBoxRect.Width - 64 + 4, portraitBackBox.Y + portraitBackBox.Height * 3 / 4);
				dogPos = new Vector2(nameBoxRect.X + nameBoxRect.Width - 128 - 8, portraitBackBox.Y + portraitBackBox.Height * 3 / 4);
				num6 -= 16;
				num4 += 16;
			}
			else
			{
				portraitBackBox = new Rectangle(Game1.xEdge + (int)(180f * widthMod), (int)(40f * heightMod) + yPositionOnScreen, 360, 264);
				portraitPos = new Vector2(portraitBackBox.X + (portraitBackBox.Width - 128) / 2, portraitBackBox.Y + 32);
				int num8 = portraitBackBox.Width;
				int x = portraitBackBox.X + portraitBackBox.Width + 16;
				back1Pos = new Vector2(portraitBackBox.X + 48, portraitPos.Y + 78f);
				forward1Pos = new Vector2(portraitBackBox.X + portraitBackBox.Width - 16 - 64, portraitPos.Y + 78f);
				malePos = new Vector2(portraitBackBox.X + portraitBackBox.Width + 16 - 4, portraitBackBox.Y - 4);
				femalePos = new Vector2(portraitBackBox.X + portraitBackBox.Width + 16 + 64 + 4, portraitBackBox.Y - 4);
				dicePos = new Vector2(portraitBackBox.X + portraitBackBox.Width / 2 - 30, portraitBackBox.Y);
				toolsBackBox = new Rectangle((int)(148f * widthMod) + xPositionOnScreen, (int)(572f * heightMod) + yPositionOnScreen, width - (int)(296f * widthMod), (int)(120f * heightMod));
				okPos = new Rectangle(toolsBackBox.X + toolsBackBox.Width + (int)(12f * widthMod), toolsBackBox.Y + (toolsBackBox.Height - 80) / 2, 80, 80);
				nameBoxRect = new Rectangle(x, portraitBackBox.Y + portraitBackBox.Height / 4 + 4, num8, num7);
				faveBoxRect = new Rectangle(x, portraitBackBox.Y + portraitBackBox.Height / 2 + 8, num8, num7);
				catPos = new Vector2(nameBoxRect.X + nameBoxRect.Width - 64 + 4, portraitBackBox.Y + 16 + portraitBackBox.Height * 3 / 4);
				dogPos = new Vector2(nameBoxRect.X + nameBoxRect.Width - 160 + 8, portraitBackBox.Y + 16 + portraitBackBox.Height * 3 / 4);
				animalSliderWidth = num8 - 128;
				num6 -= 16;
				num4 += 16;
			}
			appearanceButtons.Clear();
			labels.Clear();
			genderButtons.Clear();
			buttonLabels = new string[numAppearanceButtons];
			buttonLabels[0] = Game1.content.LoadString("Strings\\UI:Character_Skin");
			buttonLabels[1] = Game1.content.LoadString("Strings\\UI:Character_Hair");
			buttonLabels[2] = Game1.content.LoadString("Strings\\UI:Character_Shirt");
			buttonLabels[3] = Game1.content.LoadString("Strings\\UI:Character_Accessory");
			buttonLabels[4] = Game1.content.LoadString("Strings\\UI:Character_Pants");
			buttonLabels[5] = Game1.content.LoadString("Strings\\UI:Character_EyeColor");
			for (int i = 0; i < numAppearanceButtons; i++)
			{
				appearanceButtons.Add(new ClickableComponent(new Rectangle(num5 + i * (num3 + num2), num6, num3, num4), "Button " + i));
			}
			if (source == CharacterCustomization.Source.DyePots)
			{
				buttonLabels[6] = Game1.content.LoadString("Strings\\UI:Character_Shirt");
				buttonLabels[7] = Game1.content.LoadString("Strings\\UI:Character_Pants");
				appearanceButtons[6].bounds = appearanceButtons[3].bounds;
				appearanceButtons[7].bounds = appearanceButtons[4].bounds;
			}
			else
			{
				buttonLabels[6] = Game1.content.LoadString("Strings\\UI:Character_HairColor");
				buttonLabels[7] = Game1.content.LoadString("Strings\\UI:Character_PantsColor");
			}
			okButton = new ClickableTextureComponent("OK", okPos, null, null, Game1.mobileSpriteSheet, new Rectangle(0, 0, 20, 20), 4f, drawShadow: true);
			nameBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor, drawBackground: false)
			{
				X = nameBoxRect.X,
				Y = nameBoxRect.Y,
				Width = nameBoxRect.Width,
				Height = nameBoxRect.Height,
				Text = Game1.player.name,
				textLimit = 15,
				TitleText = Game1.content.LoadString("Strings\\UI:Character_Name").Replace("\n", " ")
			};
			nameBoxCC = new ClickableComponent(nameBoxRect, "")
			{
				myID = 536
			};
			if (farmChooser != null)
			{
				farmnameBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor, drawBackground: false)
				{
					X = farmBoxRect.X,
					Y = farmBoxRect.Y,
					Width = farmBoxRect.Width,
					Height = farmBoxRect.Height,
					Text = Game1.player.farmName,
					textLimit = 9,
					TitleText = Game1.content.LoadString("Strings\\UI:Character_Farm").Replace("\n", " ")
				};
				farmnameBoxCC = new ClickableComponent(farmBoxRect, "")
				{
					myID = 537
				};
			}
			favThingBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor, drawBackground: false)
			{
				X = faveBoxRect.X,
				Y = faveBoxRect.Y,
				Width = faveBoxRect.Width,
				Height = faveBoxRect.Height,
				Text = Game1.player.favoriteThing,
				textLimit = 20,
				TitleText = Game1.content.LoadString("Strings\\UI:Character_FavoriteThing").Replace("\n", " ")
			};
			favThingBoxCC = new ClickableComponent(faveBoxRect, "");
			randomButton = new ClickableTextureComponent(new Rectangle((int)dicePos.X, (int)dicePos.Y, 60, 60), Game1.mobileSpriteSheet, new Rectangle(87, 22, 20, 20), 3f, drawShadow: true);
			int num9 = 128;
			topLeftSelectPos = new Rectangle((int)back1Pos.X, (int)back1Pos.Y, 80, 76);
			topRightSelectPos = new Rectangle((int)forward1Pos.X, (int)forward1Pos.Y, 80, 76);
			topLeftSelectButton = new ClickableTextureComponent("DirectionL", topLeftSelectPos, null, "", Game1.mobileSpriteSheet, new Rectangle(108, 26, 8, 11), 4f, drawShadow: true);
			topRightSelectButton = new ClickableTextureComponent("DirectionR", topRightSelectPos, null, "", Game1.mobileSpriteSheet, new Rectangle(119, 26, 8, 11), 4f, drawShadow: true);
			selectSlider = new SliderBar(toolsBackBox.X + 180, toolsBackBox.Y, 1);
			selectSlider.bounds.Width = toolsBackBox.Width - 500;
			selectSlider.bounds.Height = toolsBackBox.Height;
			sliderTextLeftPos = new Vector2(selectSlider.bounds.X - 64, selectSlider.bounds.Y + toolsBackBox.Height / 2 - 16);
			sliderTextRightPos = new Vector2(selectSlider.bounds.X + selectSlider.bounds.Width + 40, selectSlider.bounds.Y + toolsBackBox.Height / 2 - 16);
			bottomLeftSelectPos = new Rectangle(toolsBackBox.X + 8, toolsBackBox.Y + (toolsBackBox.Height - 80) / 2, 80, 76);
			bottomRightSelectPos = new Rectangle((int)(sliderTextRightPos.X + 48f), toolsBackBox.Y + (toolsBackBox.Height - 80) / 2, 80, 76);
			bottomLeftSelectButton = new ClickableTextureComponent("BottomL", bottomLeftSelectPos, null, "", Game1.mobileSpriteSheet, new Rectangle(80, 0, 20, 19), 4f, drawShadow: true);
			bottomRightSelectButton = new ClickableTextureComponent("BottomR", bottomRightSelectPos, null, "", Game1.mobileSpriteSheet, new Rectangle(100, 0, 20, 19), 4f, drawShadow: true);
			advancedOptionsButton = new ClickableTextureComponent("Advanced", new Rectangle(xPositionOnScreen + 40, bottomLeftSelectPos.Y, 80, 80), null, null, Game1.mouseCursors2, new Rectangle(154, 154, 20, 20), 4f);
			animalText = Game1.content.LoadString("Strings\\UI:animalText");
			Vector2 vector = Game1.dialogueFont.MeasureString(animalText);
			animalTextPos = new Vector2(nameBox.X, (int)(catPos.Y + (64f - vector.Y) / 2f));
			if (source == CharacterCustomization.Source.Wizard)
			{
				animalSlider = new SliderBar((int)(catPos.X + 64f), (int)catPos.Y, Game1.player.whichPetBreed * 33);
			}
			else
			{
				animalSlider = new SliderBar(nameBoxRect.X, (int)catPos.Y, 0);
			}
			animalSlider.bounds.Width = animalSliderWidth;
			genderButtons.Add(new ClickableTextureComponent("Male", new Rectangle((int)malePos.X, (int)malePos.Y, 60, 64), null, "Male", Game1.mouseCursors, new Rectangle(129, 192, 16, 16), 4f));
			genderButtons.Add(new ClickableTextureComponent("Female", new Rectangle((int)femalePos.X, (int)femalePos.Y, 64, 64), null, "Female", Game1.mouseCursors, new Rectangle(144, 192, 16, 16), 4f));
			eyeColorPicker = new MobileColorPicker("Eyes", toolsBackBox);
			eyeColorPicker.setColor(Game1.player.newEyeColor);
			hairColorPicker = new MobileColorPicker("Hair", toolsBackBox);
			hairColorPicker.setColor(Game1.player.hairstyleColor);
			pantsColorPicker = new MobileColorPicker("Pants", toolsBackBox);
			pantsColorPicker.setColor(Game1.player.pantsColor);
			skipIntroButton = new ClickableTextureComponent("Skip Intro", new Rectangle(bottomRightSelectButton.bounds.Right + 64, bottomRightSelectButton.bounds.Top + 8, 48, 48), null, Game1.content.LoadString("Strings\\UI:Character_SkipIntro"), Game1.mouseCursors, new Rectangle(227, 425, 9, 9), 5f);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (InTutorial)
			{
				return;
			}
			haveReceivedLeftClick = true;
			if (source == CharacterCustomization.Source.ClothesDye && upperRightCloseButton != null && upperRightCloseButton.containsPoint(x, y))
			{
				optionButtonClick("OK");
				return;
			}
			if (upperRightCloseButton != null && upperRightCloseButton.containsPoint(x, y) && source == CharacterCustomization.Source.Wizard)
			{
				SetCurrentHairIndex(tempPlayerHair);
				Game1.player.changeHairColor(tempPlayerHairColor);
				Game1.player.changeAccessory(templPlayerAccessory);
				Game1.player.changeEyeColor(tempPlayerEyeColor);
				Game1.player.changeSkinColor(tempPlayerSkinColor);
				exitThisMenu();
			}
			if (TutorialManager.Instance.isInDialogBounds(x, y))
			{
				return;
			}
			if (farmChooser != null)
			{
				farmChooser.receiveLeftClick(x, y, playSound);
			}
			if (currentAppearanceButton < 5)
			{
				if (bottomRightSelectButton.containsPoint(x, y))
				{
					selectionClick(1);
				}
				if (bottomLeftSelectButton.containsPoint(x, y))
				{
					selectionClick(-1);
				}
			}
			if (okButton.containsPoint(x, y) && canLeaveMenu())
			{
				Game1.playSound("smallSelect");
			}
			if (source == CharacterCustomization.Source.ClothesDye)
			{
				currentAppearanceButton = 7;
			}
			else
			{
				int num = 0;
				if (source == CharacterCustomization.Source.DyePots)
				{
					num = 6;
				}
				for (int i = num; i < appearanceButtons.Count; i++)
				{
					if (!appearanceButtons[i].containsPoint(x, y))
					{
						continue;
					}
					Game1.playSound("smallSelect");
					if (source == CharacterCustomization.Source.Wizard)
					{
						if (i != 2 && i != 4 && i != 7)
						{
							currentAppearanceButton = i;
						}
					}
					else
					{
						currentAppearanceButton = i;
					}
					switch (currentAppearanceButton)
					{
					case 0:
						selectSlider.value = (int)((float)(int)Game1.player.skin * 100f / (float)numOptions[0]);
						oldSliderValue[currentAppearanceButton] = Game1.player.skin;
						break;
					case 1:
						selectSlider.value = (int)((float)GetCurrentHairIndex() * 100f / (float)numOptions[1]);
						oldSliderValue[currentAppearanceButton] = GetCurrentHairIndex();
						break;
					case 2:
						selectSlider.value = (int)((float)(int)Game1.player.shirt * 100f / (float)numOptions[2]);
						oldSliderValue[currentAppearanceButton] = Game1.player.shirt;
						break;
					case 3:
						selectSlider.value = (int)((float)Math.Max(Game1.player.accessory, 0) * 100f / (float)numOptions[3]);
						oldSliderValue[currentAppearanceButton] = (int)Game1.player.accessory + 1;
						break;
					case 4:
						selectSlider.value = (int)((float)(int)Game1.player.pants * 100f / (float)numOptions[4]);
						oldSliderValue[currentAppearanceButton] = Game1.player.pants;
						break;
					}
				}
			}
			foreach (ClickableComponent genderButton in genderButtons)
			{
				if (genderButton.containsPoint(x, y))
				{
					optionButtonClick(genderButton.name);
				}
			}
			if (topLeftSelectButton.containsPoint(x, y))
			{
				Game1.player.faceDirection(((int)Game1.player.facingDirection + 1 + 4) % 4);
				Game1.player.FarmerSprite.StopAnimation();
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.playSound("pickUpItem");
			}
			if (topRightSelectButton.containsPoint(x, y))
			{
				Game1.player.faceDirection(((int)Game1.player.facingDirection - 1 + 4) % 4);
				Game1.player.FarmerSprite.StopAnimation();
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.playSound("pickUpItem");
			}
			if (source == CharacterCustomization.Source.DyePots || source == CharacterCustomization.Source.ClothesDye)
			{
				return;
			}
			nameBox.Update();
			if (farmnameBox != null)
			{
				farmnameBox.Update();
			}
			favThingBox.Update();
			if (source != CharacterCustomization.Source.Wizard)
			{
				if (skipIntroButton.containsPoint(x, y))
				{
					Game1.playSound("drumkit6");
					skipIntro = !skipIntro;
				}
				if (advancedOptionsButton != null && advancedOptionsButton.containsPoint(x, y))
				{
					CharacterCustomization.clickedOnMenu = false;
					Game1.playSound("drumkit6");
					ShowAdvancedOptions();
				}
			}
			if (!randomButton.containsPoint(x, y))
			{
				return;
			}
			string cueName = "drumkit6";
			if (timesRandom > 0)
			{
				switch (Game1.random.Next(15))
				{
				case 0:
					cueName = "drumkit1";
					break;
				case 1:
					cueName = "dirtyHit";
					break;
				case 2:
					cueName = "axchop";
					break;
				case 3:
					cueName = "hoeHit";
					break;
				case 4:
					cueName = "fishSlap";
					break;
				case 5:
					cueName = "drumkit6";
					break;
				case 6:
					cueName = "drumkit5";
					break;
				case 7:
					cueName = "drumkit6";
					break;
				case 8:
					cueName = "junimoMeep1";
					break;
				case 9:
					cueName = "coin";
					break;
				case 10:
					cueName = "axe";
					break;
				case 11:
					cueName = "hammer";
					break;
				case 12:
					cueName = "drumkit2";
					break;
				case 13:
					cueName = "drumkit4";
					break;
				case 14:
					cueName = "drumkit3";
					break;
				}
			}
			Game1.playSound(cueName);
			timesRandom++;
			if (Game1.random.NextDouble() < 0.33)
			{
				if (Game1.player.IsMale)
				{
					Game1.player.changeAccessory(Game1.random.Next(19));
				}
				else
				{
					Game1.player.changeAccessory(Game1.random.Next(6, 19));
				}
			}
			else
			{
				Game1.player.changeAccessory(-1);
			}
			if (Game1.player.IsMale)
			{
				SetCurrentHairIndex(Game1.random.Next(16));
			}
			else
			{
				SetCurrentHairIndex(Game1.random.Next(16, 32));
			}
			Color c = new Color(Game1.random.Next(25, 254), Game1.random.Next(25, 254), Game1.random.Next(25, 254));
			if (Game1.random.NextDouble() < 0.5)
			{
				c.R /= 2;
				c.G /= 2;
				c.B /= 2;
			}
			if (Game1.random.NextDouble() < 0.5)
			{
				c.R = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextDouble() < 0.5)
			{
				c.G = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextDouble() < 0.5)
			{
				c.B = (byte)Game1.random.Next(15, 50);
			}
			Game1.player.changeHairColor(c);
			Game1.player.changeSkinColor(Game1.random.Next(6));
			if (Game1.random.NextDouble() < 0.25)
			{
				Game1.player.changeSkinColor(Game1.random.Next(24));
			}
			if (source != CharacterCustomization.Source.Wizard)
			{
				Game1.player.changeShirt(Game1.random.Next(112));
				Color color = new Color(Game1.random.Next(25, 254), Game1.random.Next(25, 254), Game1.random.Next(25, 254));
				if (Game1.random.NextDouble() < 0.5)
				{
					color.R /= 2;
					color.G /= 2;
					color.B /= 2;
				}
				if (Game1.random.NextDouble() < 0.5)
				{
					color.R = (byte)Game1.random.Next(15, 50);
				}
				if (Game1.random.NextDouble() < 0.5)
				{
					color.G = (byte)Game1.random.Next(15, 50);
				}
				if (Game1.random.NextDouble() < 0.5)
				{
					color.B = (byte)Game1.random.Next(15, 50);
				}
				Game1.player.changePants(color);
			}
			Color c2 = new Color(Game1.random.Next(25, 254), Game1.random.Next(25, 254), Game1.random.Next(25, 254));
			c2.R /= 2;
			c2.G /= 2;
			c2.B /= 2;
			if (Game1.random.NextDouble() < 0.5)
			{
				c2.R = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextDouble() < 0.5)
			{
				c2.G = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextDouble() < 0.5)
			{
				c2.B = (byte)Game1.random.Next(15, 50);
			}
			Game1.player.changeEyeColor(c2);
			pantsColorPicker.setColor(Game1.player.pantsColor);
			eyeColorPicker.setColor(Game1.player.newEyeColor);
			hairColorPicker.setColor(Game1.player.hairstyleColor);
			setSliderPositions();
		}

		public void resetAllButtons()
		{
			topLeftSelectButton.drawShadow = true;
			topLeftSelectButton.bounds.X = topLeftSelectPos.X;
			topLeftSelectButton.bounds.Y = topLeftSelectPos.Y;
			topRightSelectButton.drawShadow = true;
			topRightSelectButton.bounds.X = topRightSelectPos.X;
			topRightSelectButton.bounds.Y = topRightSelectPos.Y;
			bottomLeftSelectButton.drawShadow = true;
			bottomLeftSelectButton.bounds.X = bottomLeftSelectPos.X;
			bottomLeftSelectButton.bounds.Y = bottomLeftSelectPos.Y;
			bottomRightSelectButton.drawShadow = true;
			bottomRightSelectButton.bounds.X = bottomRightSelectPos.X;
			bottomRightSelectButton.bounds.Y = bottomRightSelectPos.Y;
			okButton.drawShadow = true;
			okButton.bounds.X = okPos.X;
			okButton.bounds.Y = okPos.Y;
			randomButton.drawShadow = true;
			randomButton.bounds.X = (int)dicePos.X;
			randomButton.bounds.Y = (int)dicePos.Y;
		}

		public override void leftClickHeld(int x, int y)
		{
			if (InTutorial || !haveReceivedLeftClick)
			{
				return;
			}
			if (farmChooser != null)
			{
				farmChooser.leftClickHeld(x, y);
			}
			resetAllButtons();
			if (topLeftSelectButton.bounds.Contains(x, y))
			{
				topLeftSelectButton.drawShadow = false;
				topLeftSelectButton.bounds.X = topLeftSelectPos.X - 4;
				topLeftSelectButton.bounds.Y = topLeftSelectPos.Y + 4;
				return;
			}
			if (topRightSelectButton.bounds.Contains(x, y))
			{
				topRightSelectButton.drawShadow = false;
				topRightSelectButton.bounds.X = topRightSelectPos.X - 4;
				topRightSelectButton.bounds.Y = topRightSelectPos.Y + 4;
				return;
			}
			if (okButton.bounds.Contains(x, y))
			{
				okButton.drawShadow = false;
				okButton.bounds.X = okPos.X - 4;
				okButton.bounds.Y = okPos.Y + 4;
			}
			if (source != CharacterCustomization.Source.Wizard && source != CharacterCustomization.Source.DyePots && source != CharacterCustomization.Source.ClothesDye)
			{
				if (randomButton.bounds.Contains(x, y))
				{
					randomButton.drawShadow = true;
					randomButton.bounds.X = (int)(dicePos.X - 4f);
					randomButton.bounds.Y = (int)(dicePos.Y + 4f);
				}
				if (animalSlider.bounds.Contains(x, y))
				{
					int num = animalSlider.click(x, y);
					int num2 = (int)((float)num / 100f * 6f);
					if (num2 >= 3)
					{
						num2 -= 3;
						Game1.player.whichPetBreed = num2;
						Game1.player.catPerson = true;
					}
					else
					{
						Game1.player.whichPetBreed = num2;
						Game1.player.catPerson = false;
					}
				}
			}
			if (source == CharacterCustomization.Source.Wizard && animalSlider.bounds.Contains(x, y))
			{
				int num3 = animalSlider.click(x, y);
				int num4 = (int)((float)num3 / 100f * 6f / 2f);
				if (num4 != Game1.player.whichPetBreed)
				{
					petChanged = true;
					Game1.player.whichPetBreed = num4;
				}
			}
			if (currentAppearanceButton < 5)
			{
				if (selectSlider.bounds.Contains(x, y))
				{
					int num5 = selectSlider.click(x, y);
					int num6 = (int)((float)num5 / 100f * (float)numOptions[currentAppearanceButton]);
					if (oldSliderValue[currentAppearanceButton] != num6)
					{
						selectionClick(num6 - oldSliderValue[currentAppearanceButton]);
						oldSliderValue[currentAppearanceButton] = num6;
					}
					holdingSlider = true;
				}
				else
				{
					holdingSlider = false;
				}
				if (bottomLeftSelectButton.bounds.Contains(x, y))
				{
					bottomLeftSelectButton.drawShadow = false;
					bottomLeftSelectButton.bounds.X = bottomLeftSelectPos.X - 4;
					bottomLeftSelectButton.bounds.Y = bottomLeftSelectPos.Y + 4;
				}
				else if (bottomRightSelectButton.bounds.Contains(x, y))
				{
					bottomRightSelectButton.drawShadow = false;
					bottomRightSelectButton.bounds.X = bottomRightSelectPos.X - 4;
					bottomRightSelectButton.bounds.Y = bottomRightSelectPos.Y + 4;
				}
			}
			else if (currentAppearanceButton == 6 && hairColorPicker.containsPoint(x, y))
			{
				hairColorPicker.click(x, y);
				_recolorHairAction();
				lastHeldColorPicker = hairColorPicker;
			}
			else if (currentAppearanceButton == 7 && pantsColorPicker.containsPoint(x, y))
			{
				pantsColorPicker.click(x, y);
				lastHeldColorPicker = pantsColorPicker;
			}
			else if (currentAppearanceButton == 5 && eyeColorPicker.containsPoint(x, y))
			{
				eyeColorPicker.click(x, y);
				lastHeldColorPicker = eyeColorPicker;
			}
		}

		public bool petHasChanges(Pet pet)
		{
			if (Game1.player.catPerson && pet == null)
			{
				return true;
			}
			if (Game1.player.whichPetBreed != pet.whichBreed.Value)
			{
				return true;
			}
			return false;
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (petChanged)
			{
				if (Game1.gameMode == 3 && Game1.locations != null)
				{
					Pet characterFromName = Game1.getCharacterFromName<Pet>(Game1.player.getPetName(), mustBeVillager: false);
					if (characterFromName != null && petHasChanges(characterFromName))
					{
						characterFromName.whichBreed.Value = Game1.player.whichPetBreed;
					}
				}
				petChanged = false;
			}
			if (currentAppearanceButton == 5)
			{
				_recolorEyesAction();
			}
			else if (currentAppearanceButton == 0)
			{
				Game1.player.changeSkinColor(_tempSkin);
			}
			else if (currentAppearanceButton == 2)
			{
				Game1.player.changeShirt(_tempShirt);
			}
			else if (currentAppearanceButton == 7)
			{
				_recolorPantsAction();
			}
			else if (currentAppearanceButton == 6 && source == CharacterCustomization.Source.DyePots)
			{
				_recolorHairAction();
			}
			if (!InTutorial && haveReceivedLeftClick)
			{
				haveReceivedLeftClick = false;
				if (farmChooser != null)
				{
					farmChooser.releaseLeftClick(x, y);
				}
				resetAllButtons();
				holdingSlider = false;
				hairColorPicker.releaseClick();
				pantsColorPicker.releaseClick();
				eyeColorPicker.releaseClick();
				lastHeldColorPicker = null;
				if (okButton.containsPoint(x, y) && canLeaveMenu())
				{
					Game1.playSound("bigSelect");
					optionButtonClick(okButton.name);
					showNotFinishedMessage = false;
				}
			}
		}

		public void optionButtonClick(string name)
		{
			if (currentAppearanceButton < 0)
			{
				currentAppearanceButton = 0;
			}
			switch (name)
			{
			case "Male":
				if (source != CharacterCustomization.Source.DyePots && source != CharacterCustomization.Source.ClothesDye)
				{
					Game1.player.changeGender(male: true);
					Game1.player.changeHairStyle(0);
					selectSlider.value = (int)((float)GetCurrentHairIndex() * 100f / (float)numOptions[1]);
				}
				break;
			case "Female":
				if (source != CharacterCustomization.Source.DyePots && source != CharacterCustomization.Source.ClothesDye)
				{
					Game1.player.changeGender(male: false);
					Game1.player.changeHairStyle(16);
					selectSlider.value = (int)((float)GetCurrentHairIndex() * 100f / (float)numOptions[1]);
				}
				break;
			case "Cat":
				if (source != CharacterCustomization.Source.Wizard && source != CharacterCustomization.Source.DyePots && source != CharacterCustomization.Source.ClothesDye)
				{
					Game1.player.catPerson = true;
				}
				break;
			case "Dog":
				if (source != CharacterCustomization.Source.Wizard && source != CharacterCustomization.Source.DyePots && source != CharacterCustomization.Source.ClothesDye)
				{
					Game1.player.catPerson = false;
				}
				break;
			case "OK":
			{
				if (!canLeaveMenu())
				{
					return;
				}
				if (_itemToDye != null)
				{
					if (!Game1.player.IsEquippedItem(_itemToDye))
					{
						Utility.CollectOrDrop(_itemToDye);
					}
					_itemToDye = null;
				}
				if (source == CharacterCustomization.Source.ClothesDye)
				{
					Game1.exitActiveMenu();
					break;
				}
				int num = 15;
				Game1.player.Name = nameBox.Text.Trim();
				if (Game1.player.Name.Length > num)
				{
					Game1.player.Name = Game1.player.Name.Substring(0, num);
				}
				Game1.player.displayName = Game1.player.Name;
				if (farmnameBox != null)
				{
					Game1.player.farmName.Value = farmnameBox.Text.Trim();
					if (Game1.player.farmName.Length > num)
					{
						Game1.player.farmName.Value = Game1.player.farmName.Value.Substring(0, num);
					}
				}
				Game1.player.favoriteThing.Value = favThingBox.Text.Trim();
				if (Game1.player.favoriteThing.Length > num)
				{
					Game1.player.favoriteThing.Value = Game1.player.favoriteThing.Value.Substring(0, num);
				}
				if (source == CharacterCustomization.Source.Wizard || source == CharacterCustomization.Source.DyePots || source == CharacterCustomization.Source.ClothesDye)
				{
					Game1.flashAlpha = 1f;
					Game1.playSound("yoba");
					Game1.exitActiveMenu();
					if (Game1.currentMinigame != null && Game1.currentMinigame is Intro)
					{
						(Game1.currentMinigame as Intro).doneCreatingCharacter();
					}
				}
				else if (farmChooser != null)
				{
					Game1.player.Name = nameBox.Text.Trim();
					Game1.player.farmName.Value = farmnameBox.Text.Trim();
					TutorialManager.Instance.completeTutorial(tutorialType.DUMMY_FINISH_CUSTOMIZE);
					if (Game1.activeClickableMenu is TitleMenu)
					{
						(Game1.activeClickableMenu as TitleMenu).createdNewCharacter(skipIntro);
						break;
					}
					Game1.exitActiveMenu();
					if (Game1.currentMinigame != null && Game1.currentMinigame is Intro)
					{
						(Game1.currentMinigame as Intro).doneCreatingCharacter();
					}
				}
				else if (Game1.activeClickableMenu is TitleMenu)
				{
					TitleMenu.subMenu = new MobileFarmChooser(xPositionOnScreen, yPositionOnScreen, width, height, source);
				}
				else
				{
					TutorialManager.Instance.completeTutorial(tutorialType.DUMMY_FINISH_CUSTOMIZE);
					Game1.exitActiveMenu();
					if (Game1.currentMinigame != null && Game1.currentMinigame is Intro)
					{
						(Game1.currentMinigame as Intro).doneCreatingCharacter();
					}
				}
				break;
			}
			}
			Game1.playSound("coin");
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (_sliderOpTarget != null)
			{
				Color selectedColor = _sliderOpTarget.getSelectedColor();
				if (_sliderOpTarget.Dirty && _sliderOpTarget.LastColor == selectedColor)
				{
					_sliderAction();
					_sliderOpTarget.LastColor = _sliderOpTarget.getSelectedColor();
					_sliderOpTarget.Dirty = false;
					_sliderOpTarget = null;
				}
				else
				{
					_sliderOpTarget.LastColor = selectedColor;
				}
			}
			if (Game1.player.name == "")
			{
				Game1.player.name.Value = nameMessage;
			}
			if (Game1.player.favoriteThing == "")
			{
				Game1.player.favoriteThing.Value = faveMessage;
			}
			if (farmnameBox != null && Game1.player.farmName == "")
			{
				Game1.player.farmName.Value = farmMessage;
			}
		}

		private void setSliderPositions()
		{
			switch (currentAppearanceButton)
			{
			case 0:
				selectSlider.value = (int)((float)_tempSkin * 100f / (float)numOptions[0]);
				oldSliderValue[currentAppearanceButton] = _tempSkin;
				_actualSkinColor = getSkinColor(_tempSkin);
				break;
			case 1:
				selectSlider.value = (int)((float)GetCurrentHairIndex() * 100f / (float)numOptions[1]);
				oldSliderValue[currentAppearanceButton] = GetCurrentHairIndex();
				break;
			case 2:
				selectSlider.value = (int)((float)_tempShirt * 100f / (float)numOptions[2]);
				oldSliderValue[currentAppearanceButton] = _tempShirt;
				break;
			case 3:
				selectSlider.value = (int)((float)(int)Game1.player.accessory * 100f / (float)numOptions[3]);
				oldSliderValue[currentAppearanceButton] = Game1.player.accessory;
				break;
			case 4:
				selectSlider.value = (int)((float)(int)Game1.player.pants * 100f / (float)numOptions[4]);
				oldSliderValue[currentAppearanceButton] = Game1.player.pants;
				break;
			}
		}

		private int GetCurrentHairIndex()
		{
			List<int> allHairstyleIndices = Farmer.GetAllHairstyleIndices();
			return allHairstyleIndices.IndexOf(Game1.player.hair);
		}

		private void SetCurrentHairIndex(int index)
		{
			List<int> allHairstyleIndices = Farmer.GetAllHairstyleIndices();
			if (index >= allHairstyleIndices.Count)
			{
				index = 0;
			}
			else if (index < 0)
			{
				index = allHairstyleIndices.Count() - 1;
			}
			Game1.player.changeHairStyle(allHairstyleIndices[index]);
		}

		private void selectionClick(int change)
		{
			switch (currentAppearanceButton)
			{
			case 0:
				_tempSkin += change;
				if (_tempSkin < 0)
				{
					_tempSkin = 23;
				}
				else if (_tempSkin >= 24)
				{
					_tempSkin = 0;
				}
				_actualSkinColor = getSkinColor(_tempSkin);
				if (!holdingSlider)
				{
					selectSlider.value = (int)((float)_tempSkin * 100f / (float)numOptions[0]);
					oldSliderValue[currentAppearanceButton] = _tempSkin;
				}
				break;
			case 1:
				SetCurrentHairIndex(GetCurrentHairIndex() + change);
				if (!holdingSlider)
				{
					selectSlider.value = (int)((float)GetCurrentHairIndex() * 100f / (float)numOptions[1]);
					oldSliderValue[currentAppearanceButton] = GetCurrentHairIndex();
				}
				break;
			case 2:
				_tempShirt += change;
				if (_tempShirt < 0)
				{
					_tempShirt = 111;
				}
				else if (_tempShirt > 111)
				{
					_tempShirt = 0;
				}
				if (!holdingSlider)
				{
					selectSlider.value = (int)((float)_tempShirt * 100f / (float)numOptions[2]);
					oldSliderValue[currentAppearanceButton] = _tempShirt;
				}
				break;
			case 3:
				Game1.player.changeAccessory((int)Game1.player.accessory + change);
				if (!holdingSlider)
				{
					selectSlider.value = (int)((float)(1 + (int)Game1.player.accessory) * 100f / (float)numOptions[3]);
					oldSliderValue[currentAppearanceButton] = 1 + (int)Game1.player.accessory;
				}
				break;
			case 4:
				Game1.player.changePantStyle((int)Game1.player.pants + change, is_customization_screen: true);
				if (!holdingSlider)
				{
					selectSlider.value = (int)((float)(int)Game1.player.pants * 100f / ((float)numOptions[4] - 1f));
					oldSliderValue[currentAppearanceButton] = Game1.player.pants;
				}
				break;
			}
		}

		public bool canLeaveMenu()
		{
			if (farmnameBox != null)
			{
				if (source != CharacterCustomization.Source.Wizard && source != CharacterCustomization.Source.DyePots && source != CharacterCustomization.Source.ClothesDye)
				{
					if (Game1.player.name.Length > 0 && Game1.player.farmName.Length > 0 && Game1.player.favoriteThing.Length > 0 && Game1.player.name != nameMessage && Game1.player.farmName != farmMessage)
					{
						return Game1.player.favoriteThing != faveMessage;
					}
					return false;
				}
				return true;
			}
			if (source != CharacterCustomization.Source.Wizard && source != CharacterCustomization.Source.DyePots && source != CharacterCustomization.Source.ClothesDye)
			{
				if (Game1.player.name.Length > 0 && Game1.player.favoriteThing.Length > 0 && Game1.player.name != nameMessage)
				{
					return Game1.player.favoriteThing != faveMessage;
				}
				return false;
			}
			return true;
		}

		public override void draw(SpriteBatch b)
		{
			IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);
			if (source == CharacterCustomization.Source.DyePots)
			{
				for (int i = 6; i < numAppearanceButtons; i++)
				{
					IClickableMenu.drawTextureBoxWithIconAndText(b, (widthMod >= 1f && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ko && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.es && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.pt && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.tr && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.hu) ? Game1.dialogueFont : Game1.smallFont, Game1.mouseCursors, (i == currentAppearanceButton) ? new Rectangle(267, 256, 10, 10) : new Rectangle(256, 256, 10, 10), null, new Rectangle(1, 1, 1, 1), buttonLabels[i], appearanceButtons[i].bounds.X, appearanceButtons[i].bounds.Y, appearanceButtons[i].bounds.Width, appearanceButtons[i].bounds.Height, Color.White, 4f, drawShadow: true, iconLeft: false, isClickable: true, i == currentAppearanceButton, drawIcon: false, reverseColors: false, bold: false);
				}
			}
			else if (source != CharacterCustomization.Source.ClothesDye)
			{
				if (source == CharacterCustomization.Source.Wizard)
				{
					for (int j = 0; j < numAppearanceButtons; j++)
					{
						if (j != 2 && j != 4 && j != 7)
						{
							IClickableMenu.drawTextureBoxWithIconAndText(b, (widthMod >= 1f && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ko && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.es && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.pt && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.tr && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.hu) ? Game1.dialogueFont : Game1.smallFont, Game1.mouseCursors, (j == currentAppearanceButton) ? new Rectangle(267, 256, 10, 10) : new Rectangle(256, 256, 10, 10), null, new Rectangle(1, 1, 1, 1), buttonLabels[j], appearanceButtons[j].bounds.X, appearanceButtons[j].bounds.Y, appearanceButtons[j].bounds.Width, appearanceButtons[j].bounds.Height, Color.White, 4f, drawShadow: true, iconLeft: false, isClickable: true, j == currentAppearanceButton, drawIcon: false, reverseColors: false, bold: false);
						}
					}
				}
				else
				{
					for (int k = 0; k < numAppearanceButtons; k++)
					{
						IClickableMenu.drawTextureBoxWithIconAndText(b, (widthMod >= 1f && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ko && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.es && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.pt && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.tr && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.hu) ? Game1.dialogueFont : Game1.smallFont, Game1.mouseCursors, (k == currentAppearanceButton) ? new Rectangle(267, 256, 10, 10) : new Rectangle(256, 256, 10, 10), null, new Rectangle(1, 1, 1, 1), buttonLabels[k], appearanceButtons[k].bounds.X, appearanceButtons[k].bounds.Y, appearanceButtons[k].bounds.Width, appearanceButtons[k].bounds.Height, Color.White, 4f, drawShadow: true, iconLeft: false, isClickable: true, k == currentAppearanceButton, drawIcon: false, reverseColors: false, bold: false);
					}
				}
			}
			b.Draw(Game1.daybg, new Vector2(portraitPos.X, portraitPos.Y), Color.White);
			if (_displayFarmer != null && source == CharacterCustomization.Source.ClothesDye)
			{
				_displayFarmer.FarmerRenderer.draw(b, _displayFarmer.FarmerSprite.CurrentAnimationFrame, _displayFarmer.FarmerSprite.CurrentFrame, _displayFarmer.FarmerSprite.SourceRect, new Vector2(portraitPos.X + 32f, portraitPos.Y + 32f), Vector2.Zero, 0.8f, Color.White, 0f, 1f, _displayFarmer);
			}
			else
			{
				Game1.player.FarmerRenderer.draw(b, Game1.player.FarmerSprite.CurrentAnimationFrame, Game1.player.FarmerSprite.CurrentFrame, Game1.player.FarmerSprite.SourceRect, new Vector2(portraitPos.X + 32f, portraitPos.Y + 32f), Vector2.Zero, 0.8f, Color.White, 0f, 1f, Game1.player);
			}
			if (source != CharacterCustomization.Source.DyePots && source != CharacterCustomization.Source.ClothesDye)
			{
				IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 320, 60, 60), nameBoxRect.X, nameBoxRect.Y, nameBoxRect.Width, nameBoxRect.Height, nameBox.Selected ? Color.White : Color.Wheat, 1f, drawShadow: false);
				if (farmnameBox != null)
				{
					IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 320, 60, 60), farmBoxRect.X, farmBoxRect.Y, farmBoxRect.Width, farmBoxRect.Height, farmnameBox.Selected ? Color.White : Color.Wheat, 1f, drawShadow: false);
				}
				IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 320, 60, 60), faveBoxRect.X, faveBoxRect.Y, faveBoxRect.Width, faveBoxRect.Height, favThingBox.Selected ? Color.White : Color.Wheat, 1f, drawShadow: false);
				Game1.player.name.Value = nameBox.Text.Trim();
				Game1.player.favoriteThing.Value = favThingBox.Text.Trim();
				if (farmnameBox != null)
				{
					Game1.player.farmName.Value = farmnameBox.Text.Trim();
				}
				if (Game1.player.name == nameMessage || Game1.player.name == "")
				{
					nameBox.setTextColor(Color.Red);
				}
				else
				{
					nameBox.setTextColor(Game1.textColor);
				}
				if (farmnameBox != null)
				{
					if (Game1.player.farmName == farmMessage || Game1.player.farmName == "")
					{
						farmnameBox.setTextColor(Color.Red);
					}
					else
					{
						farmnameBox.setTextColor(Game1.textColor);
					}
				}
				if (Game1.player.favoriteThing == faveMessage || Game1.player.favoriteThing == "")
				{
					favThingBox.setTextColor(Color.Red);
				}
				else
				{
					favThingBox.setTextColor(Game1.textColor);
				}
				if (source == CharacterCustomization.Source.Wizard)
				{
					Utility.drawTextWithShadow(b, nameMessage, Game1.dialogueFont, new Vector2(nameBox.X, nameBox.Y - 48), Game1.textColor);
					Utility.drawTextWithShadow(b, faveMessage, Game1.dialogueFont, new Vector2(favThingBox.X, favThingBox.Y - 48), Game1.textColor);
				}
			}
			if (source != CharacterCustomization.Source.DyePots && source != CharacterCustomization.Source.ClothesDye)
			{
				foreach (ClickableTextureComponent genderButton in genderButtons)
				{
					genderButton.draw(b);
					if ((genderButton.name.Equals("Male") && (bool)Game1.player.isMale) || (genderButton.name.Equals("Female") && !Game1.player.isMale))
					{
						IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(20, 96, 20, 20), genderButton.bounds.X - 4, genderButton.bounds.Y - 8, genderButton.bounds.Width + 8, genderButton.bounds.Height + 16, Color.White, 4f, drawShadow: false);
					}
				}
			}
			topLeftSelectButton.draw(b);
			topRightSelectButton.draw(b);
			if (source != CharacterCustomization.Source.Wizard && source != CharacterCustomization.Source.DyePots && source != CharacterCustomization.Source.ClothesDye)
			{
				animalSlider.draw(b);
				b.Draw(Game1.mouseCursors, catPos, new Rectangle(160 + ((!Game1.MasterPlayer.catPerson) ? 48 : 0) + Game1.MasterPlayer.whichPetBreed * 16, 208, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
			}
			if (currentAppearanceButton < 5)
			{
				selectSlider.draw(b);
				bottomLeftSelectButton.draw(b);
				bottomRightSelectButton.draw(b);
				if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ko)
				{
					Utility.drawTextWithShadow(b, "1", Game1.smallFont, sliderTextLeftPos, Game1.textColor);
					Utility.drawTextWithShadow(b, numOptions[currentAppearanceButton].ToString() ?? "", Game1.smallFont, sliderTextRightPos, Game1.textColor);
				}
				if (currentAppearanceButton == 0)
				{
					b.Draw(Game1.staminaRect, new Rectangle(selectSlider.bounds.X + (int)((float)selectSlider.value / 100f * (float)selectSlider.bounds.Width - 40f) + 12, selectSlider.bounds.Y - (holdingSlider ? 96 : 0) + 12, 64, selectSlider.bounds.Height - 4 - 24), _actualSkinColor);
					IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(56, 101, 15, 15), selectSlider.bounds.X + (int)((float)selectSlider.value / 100f * (float)selectSlider.bounds.Width - 40f), selectSlider.bounds.Y - (holdingSlider ? 96 : 0), 88, selectSlider.bounds.Height - 4, Color.White, 4f, drawShadow: false);
				}
				else if (currentAppearanceButton == 2)
				{
					if (shirtsTexture != null)
					{
						IClickableMenu.drawTextureBox(b, selectSlider.bounds.X + (int)((float)selectSlider.value / 100f * (float)selectSlider.bounds.Width - 40f), selectSlider.bounds.Y - (holdingSlider ? 96 : 0), 88, selectSlider.bounds.Height - 4, Color.White);
						b.Draw(shirtsTexture, new Vector2(selectSlider.bounds.X + (int)((float)selectSlider.value / 100f * (float)selectSlider.bounds.Width - 40f) + 12, selectSlider.bounds.Y - (holdingSlider ? 96 : 0) + 12), new Rectangle(_tempShirt * 8 % 128, _tempShirt * 8 / 128 * 32, 8, 8), Color.White, 0f, new Vector2(0f, 0f), 8f, SpriteEffects.None, 0.08f);
					}
				}
				else
				{
					IClickableMenu.drawTextureBoxWithIconAndText(b, Game1.dialogueFont, Game1.menuTexture, new Rectangle(0, 256, 60, 60), null, new Rectangle(1, 1, 1, 1), (oldSliderValue[currentAppearanceButton] + 1).ToString() ?? "", selectSlider.bounds.X + (int)((float)selectSlider.value / 100f * (float)selectSlider.bounds.Width - 40f), selectSlider.bounds.Y - (holdingSlider ? 96 : 0), 88, selectSlider.bounds.Height - 4, Color.White, 1f, drawShadow: false, iconLeft: false, isClickable: true, heldDown: true, drawIcon: false, reverseColors: true, bold: false);
				}
			}
			else
			{
				switch (currentAppearanceButton)
				{
				case 5:
					eyeColorPicker.draw(b);
					break;
				case 6:
					hairColorPicker.draw(b);
					break;
				case 7:
					pantsColorPicker.draw(b);
					break;
				}
			}
			if (farmChooser != null)
			{
				farmChooser.draw(b);
			}
			if (canLeaveMenu())
			{
				okButton.draw(b, Color.White, 0.75f);
			}
			else
			{
				okButton.draw(b, Color.White, 0.75f);
				okButton.draw(b, Color.LightGray * 0.5f, 0.751f);
			}
			if (source != CharacterCustomization.Source.DyePots && source != CharacterCustomization.Source.ClothesDye)
			{
				nameBox.Draw(b);
				if (farmnameBox != null)
				{
					farmnameBox.Draw(b);
					Utility.drawTextWithShadow(b, farmNameSuffix, Game1.dialogueFont, new Vector2(farmnameBox.X + favThingBox.Width - farmNameSuffixLength, farmnameBox.Y + 12), Game1.textColor);
				}
				favThingBox.Draw(b);
				randomButton.draw(b);
				if (source != CharacterCustomization.Source.Wizard)
				{
					advancedOptionsButton.draw(b);
					if (skipIntroButton != null && skipIntroButton.visible)
					{
						skipIntroButton.sourceRect.X = (skipIntro ? 236 : 227);
						skipIntroButton.draw(b);
						string text = Game1.content.LoadString("Strings\\UI:Character_SkipIntro");
						Vector2 vector = Game1.smallFont.MeasureString(text);
						Utility.drawTextWithShadow(b, text, Game1.smallFont, new Vector2((float)(skipIntroButton.bounds.X + skipIntroButton.bounds.Width / 2) - vector.X / 2f, skipIntroButton.bounds.Bottom + 8), Game1.textColor);
					}
				}
			}
			if (hoverText != null && hoverTitle != null && hoverText.Count() > 0)
			{
				IClickableMenu.drawHoverText(b, Game1.parseText(hoverText, Game1.smallFont, 256), Game1.smallFont, 0, 0, -1, hoverTitle);
			}
			drawMouse(b);
		}

		public void ShowAdvancedOptions()
		{
			AddDependency();
			AdvancedGameOptions advancedGameOptions = new AdvancedGameOptions();
			IClickableMenu currentSubMenu = TitleMenu.subMenu;
			TitleMenu.subMenu = advancedGameOptions;
			advancedGameOptions.exitFunction = delegate
			{
				TitleMenu.subMenu = currentSubMenu;
				RemoveDependency();
				populateClickableComponentList();
			};
		}
	}
}
