using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Mobile;
using StardewValley.Objects;
using xTile.Dimensions;

namespace StardewValley.Menus
{
	public class CarpenterMenu : IClickableMenu
	{
		private enum BottomButton
		{
			None,
			Move,
			BuildOrUpgrade,
			Demolish,
			Paint
		}

		public const int region_backButton = 101;

		public const int region_forwardButton = 102;

		public const int region_upgradeIcon = 103;

		public const int region_demolishButton = 104;

		public const int region_moveBuitton = 105;

		public const int region_okButton = 106;

		public const int region_cancelButton = 107;

		public const int region_paintButton = 108;

		public int maxWidthOfBuildingViewer = 448;

		public int maxHeightOfBuildingViewer = 512;

		public int maxWidthOfDescription = 416;

		private List<BluePrint> blueprints;

		private int currentBlueprintIndex;

		public ClickableComponent moveButton;

		public ClickableComponent buildButton;

		public ClickableComponent demolishButton;

		public ClickableComponent paintButton;

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent cancelButton;

		public ClickableTextureComponent backButton;

		public ClickableTextureComponent forwardButton;

		public ClickableTextureComponent upgradeIcon;

		private bool okButtonHeld;

		private bool cancelButtonHeld;

		private bool hoveringFarmHouse;

		private Building currentBuilding;

		private Building buildingToMove;

		private string buildingDescription;

		private string buildingName;

		private List<Item> ingredients = new List<Item>();

		private int price;

		private bool onFarm;

		private bool drawBG = true;

		private bool freeze;

		private bool upgrading;

		private bool demolishing;

		private bool moving;

		private bool magicalConstruction;

		private bool painting;

		protected BluePrint _demolishCheckBlueprint;

		private bool canPlace;

		private RasterizerState _rasterizerState = new RasterizerState
		{
			ScissorTestEnable = true
		};

		private Microsoft.Xna.Framework.Rectangle scissorRectangleForBuildingImage;

		private float widthMod;

		private float heightMod;

		private int buildingBoxX;

		private int buildingBoxY;

		private int buildingBoxWidth;

		private int buildingBoxHeight;

		private int scrollBoxX;

		private int scrollBoxY;

		private int scrollBoxWidth;

		private int messageBoxX;

		private int messageBoxY;

		private int messageBoxWidth;

		private int messageBoxHeight;

		private int messageXText;

		private int messageYText;

		private int ingredientsYText;

		private int buttonX;

		private int buttonY;

		private int buttonWidth;

		private int buttonHeight;

		private int button2X;

		private int button3X;

		private int button4X;

		private string moveButtonText;

		private string buildButtonText;

		private string demolishButtonText;

		private string upgradeButtonText;

		private string paintButtonText;

		private bool demolishButtonHeld;

		private bool buildButtonHeld;

		private bool moveButtonHeld;

		private bool paintButtonHeld;

		private int _drawAtX = -1;

		private int _drawAtY = -1;

		private int _lastTapX = -1;

		private int _lastTapY = -1;

		private Building _selectedBuilding;

		private bool _onBottomButtons;

		private BottomButton _selectedBottomButton;

		private string hoverText = "";

		public bool readOnly
		{
			set
			{
				if (value)
				{
					upgradeIcon.visible = false;
					demolishButton.visible = false;
					moveButton.visible = false;
					okButton.visible = false;
					paintButton.visible = false;
					buildButton.visible = false;
					cancelButton.leftNeighborID = 102;
				}
			}
		}

		public BluePrint CurrentBlueprint => blueprints[currentBlueprintIndex];

		public CarpenterMenu(bool magicalConstruction = false)
		{
			this.magicalConstruction = magicalConstruction;
			Game1.player.forceCanMove();
			resetBounds();
			blueprints = new List<BluePrint>();
			moveButtonText = Game1.content.LoadString("Strings\\UI:Carpenter_MoveBuildings");
			buildButtonText = Game1.content.LoadString("Strings\\UI:Carpenter_Build");
			demolishButtonText = Game1.content.LoadString("Strings\\UI:Carpenter_Demolish");
			paintButtonText = Game1.content.LoadString("Strings\\UI:Carpenter_PaintBuildings");
			if (magicalConstruction)
			{
				blueprints.Add(new BluePrint("Junimo Hut"));
				blueprints.Add(new BluePrint("Earth Obelisk"));
				blueprints.Add(new BluePrint("Water Obelisk"));
				blueprints.Add(new BluePrint("Desert Obelisk"));
				if (Game1.stats.getStat("boatRidesToIsland") >= 1)
				{
					blueprints.Add(new BluePrint("Island Obelisk"));
				}
				blueprints.Add(new BluePrint("Gold Clock"));
			}
			else
			{
				blueprints.Add(new BluePrint("Coop"));
				blueprints.Add(new BluePrint("Barn"));
				blueprints.Add(new BluePrint("Well"));
				blueprints.Add(new BluePrint("Silo"));
				blueprints.Add(new BluePrint("Mill"));
				blueprints.Add(new BluePrint("Shed"));
				blueprints.Add(new BluePrint("Fish Pond"));
				int num = 0;
				int numberBuildingsConstructed = Game1.getFarm().getNumberBuildingsConstructed("Stable");
				if (numberBuildingsConstructed < num + 1)
				{
					blueprints.Add(new BluePrint("Stable"));
				}
				blueprints.Add(new BluePrint("Slime Hutch"));
				if (Game1.getFarm().isBuildingConstructed("Coop"))
				{
					blueprints.Add(new BluePrint("Big Coop"));
				}
				if (Game1.getFarm().isBuildingConstructed("Big Coop"))
				{
					blueprints.Add(new BluePrint("Deluxe Coop"));
				}
				if (Game1.getFarm().isBuildingConstructed("Barn"))
				{
					blueprints.Add(new BluePrint("Big Barn"));
				}
				if (Game1.getFarm().isBuildingConstructed("Big Barn"))
				{
					blueprints.Add(new BluePrint("Deluxe Barn"));
				}
				if (Game1.getFarm().isBuildingConstructed("Shed"))
				{
					blueprints.Add(new BluePrint("Big Shed"));
				}
				blueprints.Add(new BluePrint("Shipping Bin"));
			}
			setNewActiveBlueprint();
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public override bool shouldClampGamePadCursor()
		{
			return onFarm;
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(107);
			snapCursorToCurrentSnappedComponent();
		}

		private void resetBounds()
		{
			xPositionOnScreen = Game1.xEdge;
			yPositionOnScreen = 0;
			width = Game1.uiViewport.Width - Game1.xEdge * 2;
			height = Game1.uiViewport.Height;
			if (height > 1080)
			{
				height = (height = 1080);
				yPositionOnScreen = Utility.To4(yPositionOnScreen = (IClickableMenu.viewport.Height - height) / 2);
			}
			int num = 0;
			widthMod = (float)width / 1280f;
			heightMod = (float)height / 720f;
			if (width < 1000)
			{
				num = (int)(72f / widthMod);
			}
			buildingBoxX = 100;
			buildingBoxY = 28 + yPositionOnScreen;
			buildingBoxWidth = 512;
			buildingBoxHeight = 578;
			if (height >= 720)
			{
				scrollBoxX = 624;
				scrollBoxY = 28 + yPositionOnScreen;
				scrollBoxWidth = 484;
				messageBoxX = 624;
				messageBoxY = 108 + yPositionOnScreen;
				messageBoxWidth = 484;
				messageBoxHeight = 498;
				messageXText = 32;
				messageYText = 32;
				ingredientsYText = 20;
			}
			else
			{
				scrollBoxX = buildingBoxX;
				scrollBoxY = 28 + yPositionOnScreen;
				scrollBoxWidth = buildingBoxWidth;
				messageBoxX = 624;
				messageBoxY = buildingBoxY;
				messageBoxWidth = 484;
				messageBoxHeight = buildingBoxHeight;
				messageXText = 32;
				messageYText = 32;
				ingredientsYText = 20;
			}
			buttonX = 100;
			buttonY = 620 + yPositionOnScreen;
			buttonWidth = (int)((float)(width / 4) - 32f * widthMod);
			buttonHeight = 76;
			demolishButtonHeld = false;
			buildButtonHeld = false;
			moveButtonHeld = false;
			paintButtonHeld = false;
			backButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle((int)((float)xPositionOnScreen + 10f * widthMod), (int)(322f * heightMod) + yPositionOnScreen, 80, 76), Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(80, 0, 20, 19), 4f);
			forwardButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(Game1.uiViewport.Width - Game1.xEdge - 76, (int)(322f * heightMod) + yPositionOnScreen, 80, 76), Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(100, 0, 20, 19), 4f);
			buttonY = (int)((float)buttonY * heightMod);
			if (num > 0)
			{
				buildingBoxX = Game1.xEdge;
				buttonX = Game1.xEdge;
				buttonWidth = Math.Max(buttonWidth, 300);
				button2X = buttonX + buttonWidth + (int)(32f * widthMod);
				button3X = button2X + buttonWidth + (int)(32f * widthMod);
				button4X = button3X + buttonWidth + (int)(32f * widthMod);
				scrollBoxX = buildingBoxX;
				scrollBoxWidth = buildingBoxWidth;
				messageBoxX = buildingBoxX + buildingBoxWidth + 16;
				int num2 = button3X + buttonWidth;
				messageBoxWidth = num2 - messageBoxX;
				messageBoxY = yPositionOnScreen;
				buildingBoxY = yPositionOnScreen;
				scrollBoxY = yPositionOnScreen;
				messageBoxHeight = buttonY - 4 - messageBoxY;
				buildingBoxHeight = buttonY - 4 - buildingBoxY;
			}
			else
			{
				buildingBoxX = backButton.bounds.X + backButton.bounds.Width + 16 - num;
				buttonX = buildingBoxX;
				buttonWidth = (int)((float)((Game1.uiViewport.Width - buildingBoxX * 2) / 4) - 32f * widthMod);
				button2X = buttonX + buttonWidth + (int)(32f * widthMod);
				button3X = button2X + buttonWidth + (int)(32f * widthMod);
				button4X = button3X + buttonWidth + (int)(32f * widthMod);
				int num3 = forwardButton.bounds.X - buttonWidth - 16 + num + buttonWidth;
				scrollBoxX = buildingBoxX + buildingBoxWidth + 16;
				scrollBoxWidth = num3 - scrollBoxX;
				messageBoxWidth = scrollBoxWidth;
				messageBoxX = scrollBoxX;
				buildingBoxHeight = (int)((float)buildingBoxHeight * heightMod);
				messageBoxHeight = buildingBoxHeight - messageBoxY + buildingBoxY;
				buttonY = messageBoxHeight + messageBoxY + 24;
				if (height < 720)
				{
					scrollBoxWidth = buildingBoxWidth;
					scrollBoxY = buildingBoxY;
					scrollBoxX = buildingBoxX;
				}
			}
			scissorRectangleForBuildingImage = new Microsoft.Xna.Framework.Rectangle(buildingBoxX + 16, buildingBoxY + 16, buildingBoxWidth - 32, buildingBoxHeight - 32);
			demolishButtonHeld = (buildButtonHeld = (moveButtonHeld = (paintButtonHeld = false)));
			initialize(xPositionOnScreen, yPositionOnScreen, width, height, showUpperRightCloseButton: true);
			buildButton = new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(button2X, buttonY, buttonWidth, buttonHeight), "");
			moveButton = new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(buttonX, buttonY, buttonWidth, buttonHeight), "");
			demolishButton = new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(button3X, buttonY, buttonWidth, buttonHeight), "");
			paintButton = new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(button4X, buttonY, buttonWidth, buttonHeight), "");
			okButton = new ClickableTextureComponent("OK", new Microsoft.Xna.Framework.Rectangle(-100, -100, 80, 80), null, null, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(0, 0, 20, 20), 4f)
			{
				myID = 106
			};
			cancelButton = new ClickableTextureComponent("Cancel", new Microsoft.Xna.Framework.Rectangle(-100, -100, 80, 80), null, null, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(20, 0, 20, 20), 4f)
			{
				myID = 107
			};
			upgradeIcon = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(buildingBoxX + buildingBoxWidth - 130, buildingBoxY + 16, 100, 100), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(402, 328, 9, 13), 4f)
			{
				myID = 103,
				rightNeighborID = 104,
				leftNeighborID = 105
			};
			bool flag = false;
			bool visible = CanPaintHouse() && HasPermissionsToPaint(null);
			foreach (Building building in Game1.getFarm().buildings)
			{
				if (building.hasCarpenterPermissions())
				{
					flag = true;
				}
				if (building.CanBePainted() && HasPermissionsToPaint(building))
				{
					visible = true;
				}
			}
			demolishButton.visible = Game1.IsMasterGame;
			moveButton.visible = Game1.IsMasterGame || Game1.player.team.farmhandsCanMoveBuildings.Value == FarmerTeam.RemoteBuildingPermissions.On || (Game1.player.team.farmhandsCanMoveBuildings.Value == FarmerTeam.RemoteBuildingPermissions.OwnedBuildings && flag);
			paintButton.visible = visible;
			if (magicalConstruction)
			{
				paintButton.visible = false;
			}
			if (!demolishButton.visible)
			{
				upgradeIcon.rightNeighborID = demolishButton.rightNeighborID;
				okButton.rightNeighborID = demolishButton.rightNeighborID;
				cancelButton.leftNeighborID = demolishButton.leftNeighborID;
			}
			if (!moveButton.visible)
			{
				upgradeIcon.leftNeighborID = moveButton.leftNeighborID;
				forwardButton.rightNeighborID = -99998;
				okButton.leftNeighborID = moveButton.leftNeighborID;
			}
		}

		public void setNewActiveBlueprint()
		{
			if (blueprints[currentBlueprintIndex].name.Contains("Coop"))
			{
				currentBuilding = new Coop(blueprints[currentBlueprintIndex], Vector2.Zero);
			}
			else if (blueprints[currentBlueprintIndex].name.Contains("Barn"))
			{
				currentBuilding = new Barn(blueprints[currentBlueprintIndex], Vector2.Zero);
			}
			else if (blueprints[currentBlueprintIndex].name.Contains("Mill"))
			{
				currentBuilding = new Mill(blueprints[currentBlueprintIndex], Vector2.Zero);
			}
			else if (blueprints[currentBlueprintIndex].name.Contains("Junimo Hut"))
			{
				currentBuilding = new JunimoHut(blueprints[currentBlueprintIndex], Vector2.Zero);
			}
			else if (blueprints[currentBlueprintIndex].name.Contains("Shipping Bin"))
			{
				currentBuilding = new ShippingBin(blueprints[currentBlueprintIndex], Vector2.Zero);
			}
			else if (blueprints[currentBlueprintIndex].name.Contains("Fish Pond"))
			{
				currentBuilding = new FishPond(blueprints[currentBlueprintIndex], Vector2.Zero);
			}
			else if (blueprints[currentBlueprintIndex].name.Contains("Greenhouse"))
			{
				currentBuilding = new GreenhouseBuilding(blueprints[currentBlueprintIndex], Vector2.Zero);
			}
			else
			{
				currentBuilding = new Building(blueprints[currentBlueprintIndex], Vector2.Zero);
			}
			price = blueprints[currentBlueprintIndex].moneyRequired;
			ingredients.Clear();
			foreach (KeyValuePair<int, int> item in blueprints[currentBlueprintIndex].itemsRequired)
			{
				ingredients.Add(new Object(item.Key, item.Value));
			}
			buildingDescription = blueprints[currentBlueprintIndex].description;
			buildingName = blueprints[currentBlueprintIndex].displayName;
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public bool hasPermissionsToDemolish(Building b)
		{
			if (Game1.IsMasterGame)
			{
				return CanDemolishThis(b);
			}
			return false;
		}

		public bool CanPaintHouse()
		{
			return Game1.MasterPlayer.HouseUpgradeLevel >= 2;
		}

		public bool HasPermissionsToPaint(Building b)
		{
			if (b == null)
			{
				if (Game1.player.UniqueMultiplayerID == Game1.MasterPlayer.UniqueMultiplayerID)
				{
					return true;
				}
				if (Game1.player.spouse == Game1.MasterPlayer.UniqueMultiplayerID.ToString())
				{
					return true;
				}
				return false;
			}
			if (b.isCabin && b.indoors.Value is Cabin)
			{
				Farmer owner = (b.indoors.Value as Cabin).owner;
				if (Game1.player.UniqueMultiplayerID == owner.UniqueMultiplayerID)
				{
					return true;
				}
				if (Game1.player.spouse == owner.UniqueMultiplayerID.ToString())
				{
					return true;
				}
				return false;
			}
			return true;
		}

		public bool hasPermissionsToMove(Building b)
		{
			if (!Game1.getFarm().greenhouseUnlocked.Value && b is GreenhouseBuilding)
			{
				return false;
			}
			if (Game1.IsMasterGame)
			{
				return true;
			}
			if (Game1.player.team.farmhandsCanMoveBuildings.Value == FarmerTeam.RemoteBuildingPermissions.On)
			{
				return true;
			}
			if (Game1.player.team.farmhandsCanMoveBuildings.Value == FarmerTeam.RemoteBuildingPermissions.OwnedBuildings && b.hasCarpenterPermissions())
			{
				return true;
			}
			return false;
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			if (onFarm)
			{
				switch (b)
				{
				case Buttons.A:
					OnReleaseOKButton();
					break;
				case Buttons.B:
					OnReleaseCancelButton();
					break;
				}
				return;
			}
			switch (b)
			{
			case Buttons.DPadLeft:
			case Buttons.LeftThumbstickLeft:
				if (_onBottomButtons)
				{
					if (_selectedBottomButton == BottomButton.Move)
					{
						_selectedBottomButton = BottomButton.Demolish;
					}
					else if (_selectedBottomButton == BottomButton.BuildOrUpgrade)
					{
						_selectedBottomButton = BottomButton.Move;
					}
					else if (_selectedBottomButton == BottomButton.Demolish)
					{
						_selectedBottomButton = BottomButton.BuildOrUpgrade;
					}
					else if (_selectedBottomButton == BottomButton.Paint)
					{
						_selectedBottomButton = BottomButton.Demolish;
					}
				}
				else
				{
					OnTapButtonLeftArrow();
				}
				break;
			case Buttons.DPadRight:
			case Buttons.LeftThumbstickRight:
				if (_onBottomButtons)
				{
					if (_selectedBottomButton == BottomButton.Move)
					{
						_selectedBottomButton = BottomButton.BuildOrUpgrade;
					}
					else if (_selectedBottomButton == BottomButton.BuildOrUpgrade)
					{
						_selectedBottomButton = BottomButton.Demolish;
					}
					else if (_selectedBottomButton == BottomButton.Demolish)
					{
						_selectedBottomButton = BottomButton.Paint;
					}
					else if (_selectedBottomButton == BottomButton.Paint)
					{
						_selectedBottomButton = BottomButton.Move;
					}
				}
				else
				{
					OnTapButtonRightArrow();
				}
				break;
			case Buttons.DPadUp:
			case Buttons.LeftThumbstickUp:
				_onBottomButtons = false;
				_selectedBottomButton = BottomButton.None;
				break;
			case Buttons.DPadDown:
			case Buttons.LeftThumbstickDown:
				_onBottomButtons = true;
				_selectedBottomButton = BottomButton.Move;
				break;
			case Buttons.A:
				if (_selectedBottomButton == BottomButton.Move)
				{
					OnReleaseMoveButton();
				}
				else if (_selectedBottomButton == BottomButton.BuildOrUpgrade)
				{
					OnReleaseBuildButton();
				}
				else if (_selectedBottomButton == BottomButton.Demolish)
				{
					OnReleaseDemolishButton();
				}
				else if (_selectedBottomButton == BottomButton.Paint)
				{
					OnReleasePaintButton();
				}
				break;
			case Buttons.B:
				Game1.playSound("bigDeSelect");
				exitThisMenu();
				break;
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (!freeze)
			{
				if (!onFarm)
				{
					base.receiveKeyPress(key);
				}
				if (!Game1.IsFading() && onFarm && Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose() && Game1.locationRequest == null)
				{
					Game1.globalFadeToBlack(returnToCarpentryMenu);
				}
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (!onFarm || Game1.IsFading())
			{
				return;
			}
			if (hoveringFarmHouse)
			{
				((Farm)Game1.getLocationFromName("Farm")).frameHouseColor = Color.Lime;
			}
			if (Game1.IsMultiplayer)
			{
				return;
			}
			Farm farm = Game1.getFarm();
			foreach (FarmAnimal value in farm.animals.Values)
			{
				value.MovePosition(Game1.currentGameTime, Game1.viewport, farm);
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (freeze || Game1.globalFade)
			{
				return;
			}
			if (!onFarm)
			{
				base.receiveLeftClick(x, y, playSound);
			}
			if (cancelButton.containsPoint(x, y))
			{
				Game1.playSound("smallSelect");
				cancelButtonHeld = true;
			}
			if (!okButton.containsPoint(x, y))
			{
				hoveringFarmHouse = false;
			}
			if (!onFarm && backButton.containsPoint(x, y))
			{
				OnTapButtonLeftArrow();
			}
			if (!onFarm && forwardButton.containsPoint(x, y))
			{
				OnTapButtonRightArrow();
			}
			if (!onFarm && demolishButton.containsPoint(x, y) && demolishButton.visible && CanDemolishThis(blueprints[currentBlueprintIndex]))
			{
				demolishButtonHeld = true;
			}
			if (!onFarm && moveButton.containsPoint(x, y) && moveButton.visible)
			{
				moveButtonHeld = true;
			}
			if (!onFarm && paintButton.containsPoint(x, y) && paintButton.visible)
			{
				paintButtonHeld = true;
			}
			if (buildButton.containsPoint(x, y) && buildButton.visible)
			{
				buildButtonHeld = true;
			}
			if (onFarm && x > cancelButton.bounds.X - cancelButton.bounds.Width && y > cancelButton.bounds.Y - cancelButton.bounds.Height)
			{
				if (cancelButton.containsPoint(x, y))
				{
					cancelButtonHeld = true;
					okButtonHeld = false;
				}
				else if (moving)
				{
					if (((_selectedBuilding != null && buildingToMove == null) || (buildingToMove != null && canPlace)) && okButton.containsPoint(x, y))
					{
						Game1.playSound("smallSelect");
						okButtonHeld = true;
						cancelButtonHeld = false;
					}
				}
				else if (okButton.containsPoint(x, y))
				{
					Game1.playSound("smallSelect");
					okButtonHeld = true;
					cancelButtonHeld = false;
				}
				return;
			}
			_lastTapX = -1;
			_lastTapY = -1;
			if (!onFarm || freeze || (!upgrading && !demolishing && !moving && !painting) || buildingToMove != null)
			{
				return;
			}
			Vector2 vector = new Vector2((Game1.viewport.X + Game1.getMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getMouseY(ui_scale: false)) / 64);
			Farm farm = (Farm)Game1.getLocationFromName("Farm");
			Building buildingAt = farm.getBuildingAt(vector);
			if (buildingAt == null)
			{
				vector.Y += 1f;
				buildingAt = farm.getBuildingAt(vector);
				if (buildingAt == null)
				{
					vector.Y += 1f;
					buildingAt = farm.getBuildingAt(vector);
				}
				if (buildingAt == null)
				{
					_selectedBuilding = null;
				}
			}
			if (_selectedBuilding == buildingAt)
			{
				Game1.playSound("smallSelect");
				_selectedBuilding = null;
			}
			else if (buildingAt != null)
			{
				Game1.playSound("smallSelect");
				_selectedBuilding = buildingAt;
			}
			foreach (Building building in ((Farm)Game1.getLocationFromName("Farm")).buildings)
			{
				building.color.Value = Color.White;
			}
			if (_selectedBuilding != null)
			{
				if (upgrading)
				{
					if (CurrentBlueprint.nameOfBuildingToUpgrade != null && CurrentBlueprint.nameOfBuildingToUpgrade.Equals(_selectedBuilding.buildingType))
					{
						_selectedBuilding.color.Value = Color.Lime * 0.8f;
					}
					else
					{
						_selectedBuilding.color.Value = Color.Red * 0.8f;
					}
				}
				else if (demolishing)
				{
					if (hasPermissionsToDemolish(_selectedBuilding) && CanDemolishThis(_selectedBuilding))
					{
						_selectedBuilding.color.Value = Color.Lime * 0.8f;
					}
					else
					{
						_selectedBuilding.color.Value = Color.Red * 0.8f;
					}
				}
				else if (moving)
				{
					if (hasPermissionsToMove(_selectedBuilding))
					{
						_selectedBuilding.color.Value = Color.Lime * 0.8f;
					}
					else
					{
						_selectedBuilding.color.Value = Color.Red * 0.8f;
					}
				}
				else if (painting)
				{
					if (_selectedBuilding.CanBePainted() && HasPermissionsToPaint(_selectedBuilding))
					{
						_selectedBuilding.color.Value = Color.Lime * 0.8f;
					}
					else
					{
						_selectedBuilding.color.Value = Color.Red * 0.8f;
					}
				}
			}
			else if (painting && farm.GetHouseRect().Contains(Utility.Vector2ToPoint(vector)) && HasPermissionsToPaint(null) && CanPaintHouse())
			{
				hoveringFarmHouse = true;
				farm.frameHouseColor = Color.Lime;
			}
		}

		private void OnClickOK()
		{
			if (!onFarm || freeze || Game1.IsFading())
			{
				return;
			}
			if (demolishing)
			{
				Farm farm = Game1.getLocationFromName("Farm") as Farm;
				Building destroyed = _selectedBuilding;
				Action buildingLockFailed = delegate
				{
					if (demolishing)
					{
						Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_LockFailed"), Color.Red, 3500f));
					}
				};
				Action continueDemolish = delegate
				{
					if (demolishing && destroyed != null && farm.buildings.Contains(destroyed))
					{
						if ((int)destroyed.daysOfConstructionLeft > 0 || (int)destroyed.daysUntilUpgrade > 0)
						{
							Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_DuringConstruction"), Color.Red, 3500f));
						}
						else if (destroyed.indoors.Value != null && destroyed.indoors.Value is AnimalHouse && (destroyed.indoors.Value as AnimalHouse).animalsThatLiveHere.Count > 0)
						{
							Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_AnimalsHere"), Color.Red, 3500f));
						}
						else if (destroyed.indoors.Value != null && destroyed.indoors.Value.farmers.Any())
						{
							Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_PlayerHere"), Color.Red, 3500f));
						}
						else
						{
							if (destroyed.indoors.Value != null && destroyed.indoors.Value is Cabin)
							{
								foreach (Farmer allFarmer in Game1.getAllFarmers())
								{
									if (allFarmer.currentLocation != null && allFarmer.currentLocation.Name == (destroyed.indoors.Value as Cabin).GetCellarName())
									{
										Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_PlayerHere"), Color.Red, 3500f));
										return;
									}
								}
							}
							if (destroyed.indoors.Value is Cabin && (destroyed.indoors.Value as Cabin).farmhand.Value.isActive())
							{
								Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_FarmhandOnline"), Color.Red, 3500f));
							}
							else
							{
								destroyed.BeforeDemolish();
								Chest chest = null;
								if (destroyed.indoors.Value is Cabin)
								{
									List<Item> list = (destroyed.indoors.Value as Cabin).demolish();
									if (list.Count > 0)
									{
										chest = new Chest(playerChest: true);
										chest.fixLidFrame();
										chest.items.Set(list);
									}
								}
								if (farm.destroyStructure(destroyed))
								{
									int num = (int)destroyed.tileY + (int)destroyed.tilesHigh;
									Game1.flashAlpha = 1f;
									destroyed.showDestroyedAnimation(Game1.getFarm());
									Game1.playSound("explosion");
									Utility.spreadAnimalsAround(destroyed, farm);
									DelayedAction.functionAfterDelay(returnToCarpentryMenu, 1500);
									freeze = true;
									if (chest != null)
									{
										farm.objects[new Vector2((int)destroyed.tileX + (int)destroyed.tilesWide / 2, (int)destroyed.tileY + (int)destroyed.tilesHigh / 2)] = chest;
									}
								}
							}
						}
					}
				};
				if (destroyed != null)
				{
					if (destroyed.indoors.Value != null && destroyed.indoors.Value is Cabin && !Game1.IsMasterGame)
					{
						Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_LockFailed"), Color.Red, 3500f));
						destroyed = null;
						return;
					}
					if (!CanDemolishThis(destroyed))
					{
						destroyed = null;
						return;
					}
					if (!Game1.IsMasterGame && !hasPermissionsToDemolish(destroyed))
					{
						destroyed = null;
						return;
					}
				}
				if (destroyed != null && destroyed.indoors.Value is Cabin)
				{
					Cabin cabin = destroyed.indoors.Value as Cabin;
					if (cabin.farmhand.Value != null && (bool)cabin.farmhand.Value.isCustomized)
					{
						Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\UI:Carpenter_DemolishCabinConfirm", cabin.farmhand.Value.Name), Game1.currentLocation.createYesNoResponses(), delegate(Farmer f, string answer)
						{
							if (answer == "Yes")
							{
								Game1.activeClickableMenu = this;
								Game1.player.team.demolishLock.RequestLock(continueDemolish, buildingLockFailed);
							}
							else
							{
								DelayedAction.functionAfterDelay(returnToCarpentryMenu, 500);
							}
						});
						return;
					}
				}
				if (destroyed != null)
				{
					Game1.player.team.demolishLock.RequestLock(continueDemolish, buildingLockFailed);
				}
				return;
			}
			if (upgrading)
			{
				Building selectedBuilding = _selectedBuilding;
				if (selectedBuilding != null && CurrentBlueprint.name != null && selectedBuilding.buildingType.Equals(CurrentBlueprint.nameOfBuildingToUpgrade))
				{
					CurrentBlueprint.consumeResources();
					selectedBuilding.daysUntilUpgrade.Value = 2;
					selectedBuilding.showUpgradeAnimation(Game1.getFarm());
					Game1.playSound("axe");
					returnToCarpentryMenuAfterSuccessfulBuild();
					freeze = true;
					Game1.multiplayer.globalChatInfoMessage("BuildingBuild", Game1.player.Name, Utility.AOrAn(CurrentBlueprint.displayName), CurrentBlueprint.displayName, Game1.player.farmName);
				}
				else if (selectedBuilding != null)
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantUpgrade_BuildingType"), Color.Red, 3500f));
				}
				return;
			}
			if (painting)
			{
				Farm farm_location = Game1.getFarm();
				Vector2 vector = new Vector2((Game1.viewport.X + Game1.getMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getMouseY(ui_scale: false)) / 64);
				Building selectedBuilding2 = _selectedBuilding;
				if (selectedBuilding2 != null)
				{
					if (!selectedBuilding2.CanBePainted())
					{
						Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CannotPaint"), Color.Red, 3500f));
						return;
					}
					if (!HasPermissionsToPaint(selectedBuilding2))
					{
						Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CannotPaint_Permission"), Color.Red, 3500f));
						return;
					}
					selectedBuilding2.color.Value = Color.White;
					SetChildMenu(new BuildingPaintMenu(selectedBuilding2));
				}
				else
				{
					if (!hoveringFarmHouse)
					{
						return;
					}
					if (!CanPaintHouse())
					{
						Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CannotPaint"), Color.Red, 3500f));
						return;
					}
					if (!HasPermissionsToPaint(null))
					{
						Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CannotPaint_Permission"), Color.Red, 3500f));
						return;
					}
					SetChildMenu(new BuildingPaintMenu("House", () => (farm_location.paintedHouseTexture != null) ? farm_location.paintedHouseTexture : Farm.houseTextures, farm_location.houseSource.Value, farm_location.housePaintColor.Value));
				}
				return;
			}
			if (moving)
			{
				if (buildingToMove == null)
				{
					buildingToMove = _selectedBuilding;
					if (buildingToMove != null && ((int)buildingToMove.daysOfConstructionLeft > 0 || !hasPermissionsToMove(buildingToMove)))
					{
						buildingToMove = null;
					}
					if (buildingToMove != null)
					{
						_drawAtX = Game1.uiViewport.Width / 2 - (int)buildingToMove.tilesWide * 64 / 2;
						_drawAtY = Game1.uiViewport.Height / 2 - (int)buildingToMove.tilesHigh * 64 / 2;
					}
					return;
				}
				Farm farm2 = (Farm)Game1.getLocationFromName("Farm");
				Vector2 tileLocation = new Vector2((Game1.uiViewport.X + _drawAtX) / 64, (Game1.uiViewport.Y + _drawAtY) / 64);
				Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(buildingToMove.tileX, buildingToMove.tileY, buildingToMove.tilesWide, buildingToMove.tilesHigh);
				Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X, (int)tileLocation.Y, buildingToMove.tilesWide, buildingToMove.tilesHigh);
				if (rectangle.Intersects(value))
				{
					farm2.buildings.Remove(buildingToMove);
				}
				if (farm2.buildStructure(buildingToMove, tileLocation, Game1.player, skipSafetyChecks: true))
				{
					buildingToMove.isMoving = false;
					if (buildingToMove is ShippingBin)
					{
						(buildingToMove as ShippingBin).initLid();
					}
					if (buildingToMove is GreenhouseBuilding)
					{
						Game1.getFarm().greenhouseMoved.Value = true;
					}
					buildingToMove.performActionOnBuildingPlacement();
					buildingToMove = null;
					Game1.playSound("axchop");
					DelayedAction.playSoundAfterDelay("dirtyHit", 50);
					DelayedAction.playSoundAfterDelay("dirtyHit", 150);
					_selectedBuilding = null;
					{
						foreach (Building building in ((Farm)Game1.getLocationFromName("Farm")).buildings)
						{
							building.color.Value = Color.White;
						}
						return;
					}
				}
				Game1.playSound("cancel");
				return;
			}
			Game1.player.team.buildLock.RequestLock(delegate
			{
				if (onFarm && Game1.locationRequest == null)
				{
					if (tryToBuild())
					{
						CurrentBlueprint.consumeResources();
						returnToCarpentryMenuAfterSuccessfulBuild();
						freeze = true;
					}
					else
					{
						Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantBuild"), Color.Red, 3500f));
					}
				}
				Game1.player.team.buildLock.ReleaseLock();
			});
		}

		public bool tryToBuild()
		{
			return ((Farm)Game1.getLocationFromName("Farm")).buildStructure(CurrentBlueprint, new Vector2((Game1.viewport.X + _drawAtX) / 64, (Game1.viewport.Y + _drawAtY) / 64), Game1.player, magicalConstruction);
		}

		public void returnToCarpentryMenu()
		{
			freeze = true;
			_selectedBuilding = null;
			foreach (Building building in ((Farm)Game1.getLocationFromName("Farm")).buildings)
			{
				building.color.Value = Color.White;
			}
			demolishing = false;
			moveButtonHeld = false;
			demolishButtonHeld = false;
			paintButtonHeld = false;
			LocationRequest locationRequest = Game1.getLocationRequest(magicalConstruction ? "WizardHouse" : "ScienceHouse");
			locationRequest.OnWarp += delegate
			{
				onFarm = false;
				Game1.player.viewingLocation.Value = null;
				resetBounds();
				upgrading = false;
				moving = false;
				painting = false;
				buildingToMove = null;
				freeze = false;
				Game1.displayHUD = true;
				Game1.viewportFreeze = false;
				Game1.viewport.Location = new Location(320, 1536);
				drawBG = true;
				demolishing = false;
				buildButtonHeld = false;
				Game1.displayFarmer = true;
				if (Game1.options.SnappyMenus)
				{
					populateClickableComponentList();
					snapToDefaultClickableComponent();
				}
				resetBounds();
			};
			Game1.warpFarmer(locationRequest, Game1.player.getTileX(), Game1.player.getTileY(), Game1.player.facingDirection);
		}

		public void returnToCarpentryMenuAfterSuccessfulBuild()
		{
			LocationRequest locationRequest = Game1.getLocationRequest(magicalConstruction ? "WizardHouse" : "ScienceHouse");
			locationRequest.OnWarp += delegate
			{
				Game1.displayHUD = true;
				Game1.player.viewingLocation.Value = null;
				Game1.viewportFreeze = false;
				Game1.viewport.Location = new Location(320, 1536);
				freeze = true;
				Game1.displayFarmer = true;
				robinConstructionMessage();
				resetBounds();
			};
			Game1.warpFarmer(locationRequest, Game1.player.getTileX(), Game1.player.getTileY(), Game1.player.facingDirection);
			demolishing = false;
			moveButtonHeld = false;
			demolishButtonHeld = false;
			paintButtonHeld = false;
		}

		public void robinConstructionMessage()
		{
			exitThisMenu();
			Game1.player.forceCanMove();
			if (!magicalConstruction)
			{
				string text = "Data\\ExtraDialogue:Robin_" + (upgrading ? "Upgrade" : "New") + "Construction";
				if (Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.currentSeason))
				{
					text += "_Festival";
				}
				if (CurrentBlueprint.daysToConstruct <= 0)
				{
					Game1.drawDialogue(Game1.getCharacterFromName("Robin"), Game1.content.LoadString("Data\\ExtraDialogue:Robin_Instant", (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de) ? CurrentBlueprint.displayName : CurrentBlueprint.displayName.ToLower()));
				}
				else
				{
					Game1.drawDialogue(Game1.getCharacterFromName("Robin"), Game1.content.LoadString(text, (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de) ? CurrentBlueprint.displayName : CurrentBlueprint.displayName.ToLower(), (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de) ? CurrentBlueprint.displayName.Split(' ').Last().Split('-')
						.Last() : ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.it) ? CurrentBlueprint.displayName.ToLower().Split(' ').First() : CurrentBlueprint.displayName.ToLower().Split(' ').Last())));
				}
			}
		}

		public override bool overrideSnappyMenuCursorMovementBan()
		{
			return onFarm;
		}

		public void setUpForBuildingPlacement()
		{
			Game1.currentLocation.cleanupBeforePlayerExit();
			hoverText = "";
			Game1.currentLocation = Game1.getLocationFromName("Farm");
			Game1.player.viewingLocation.Value = "Farm";
			Game1.currentLocation.resetForPlayerEntry();
			Game1.globalFadeToClear();
			onFarm = true;
			SetCancelButtonBounds();
			SetOKButtonBounds();
			Game1.displayHUD = false;
			Game1.viewportFreeze = true;
			Game1.viewport.Location = new Location(3136, 320);
			Game1.panScreen(0, 0);
			drawBG = false;
			freeze = false;
			Game1.displayFarmer = false;
			if (!demolishing && CurrentBlueprint.nameOfBuildingToUpgrade != null && CurrentBlueprint.nameOfBuildingToUpgrade.Length > 0 && !moving && !painting)
			{
				upgrading = true;
				if (TutorialManager.Instance != null)
				{
					TutorialManager.Instance.completeAllTutorials();
				}
			}
		}

		public override void gameWindowSizeChanged(Microsoft.Xna.Framework.Rectangle oldBounds, Microsoft.Xna.Framework.Rectangle newBounds)
		{
			resetBounds();
		}

		public virtual bool CanDemolishThis(Building building)
		{
			if (building == null)
			{
				return false;
			}
			if (_demolishCheckBlueprint == null || _demolishCheckBlueprint.name != building.buildingType.Value)
			{
				_demolishCheckBlueprint = new BluePrint(building.buildingType);
			}
			if (_demolishCheckBlueprint != null)
			{
				return CanDemolishThis(_demolishCheckBlueprint);
			}
			return true;
		}

		public virtual bool CanDemolishThis(BluePrint blueprint)
		{
			if (blueprint.moneyRequired < 0)
			{
				return false;
			}
			if (blueprint.name == "Shipping Bin")
			{
				int num = 0;
				foreach (Building building in Game1.getFarm().buildings)
				{
					if (building is ShippingBin)
					{
						num++;
					}
					if (num > 1)
					{
						break;
					}
				}
				if (num <= 1)
				{
					return false;
				}
			}
			return true;
		}

		public override void draw(SpriteBatch b)
		{
			if (drawBG)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);
			}
			if (Game1.IsFading() || freeze)
			{
				return;
			}
			if (!onFarm)
			{
				IClickableMenu.drawTextureBox(b, buildingBoxX, buildingBoxY, buildingBoxWidth, buildingBoxHeight, magicalConstruction ? Color.RoyalBlue : Color.White);
				b.End();
				b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, _rasterizerState);
				Microsoft.Xna.Framework.Rectangle scissorRectangle = b.GraphicsDevice.ScissorRectangle;
				b.GraphicsDevice.ScissorRectangle = scissorRectangleForBuildingImage;
				currentBuilding.drawInMenu(b, buildingBoxX + (buildingBoxWidth - (int)currentBuilding.tilesWide * 64) / 2, buildingBoxY + (buildingBoxHeight - currentBuilding.getSourceRectForMenu().Height * 4) / 2 - 12);
				if (CurrentBlueprint.isUpgrade())
				{
					upgradeIcon.draw(b);
				}
				b.GraphicsDevice.ScissorRectangle = scissorRectangle;
				b.End();
				b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
				IClickableMenu.drawTextureBox(b, messageBoxX, messageBoxY, messageBoxWidth, messageBoxHeight, magicalConstruction ? Color.RoyalBlue : Color.White);
				SpriteText.drawScrollText(b, buildingName, Game1.dialogueFont, scrollBoxX, scrollBoxY, scrollBoxWidth);
				string text = ((width <= 1000) ? Game1.parseText(buildingDescription, Game1.smallFont, messageBoxWidth - 64 - 2 * messageXText) : Game1.parseText(buildingDescription, Game1.smallFont, messageBoxWidth - 2 * messageXText));
				if (magicalConstruction)
				{
					Utility.drawTextWithShadow(b, text, Game1.smallFont, new Vector2(messageBoxX + messageXText, messageBoxY + messageYText + 2), Color.Black, 1f, -1f, -1, -1, 0f);
				}
				Utility.drawTextWithShadow(b, text, Game1.smallFont, new Vector2(messageBoxX + messageXText, messageBoxY + messageYText), magicalConstruction ? Color.PaleGoldenrod : Game1.textColor, 1f, -1f, -1, -1, magicalConstruction ? 0f : 0.25f);
				Vector2 location = new Vector2(messageBoxX + messageXText + 16, messageBoxY + ingredientsYText + (int)Game1.dialogueFont.MeasureString(text).Y);
				SpriteText.drawString(b, "$", (int)location.X + 21 + 4, (int)location.Y + 21);
				if (magicalConstruction)
				{
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", price), Game1.smallFont, new Vector2(location.X + 64f + 12f, location.Y + 8f + 21f), Game1.textColor * 0.25f, 1f, -1f, -1, -1, magicalConstruction ? 0f : 0.25f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", price), Game1.smallFont, new Vector2(location.X + 64f + 16f - 1f, location.Y + 8f + 21f), Game1.textColor * 0.25f, 1f, -1f, -1, -1, magicalConstruction ? 0f : 0.25f);
				}
				Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", price), Game1.smallFont, new Vector2(location.X + 64f + 16f, location.Y + 4f + 21f), (Game1.player.Money < price) ? Color.Red : (magicalConstruction ? Color.PaleGoldenrod : Game1.textColor), 1f, -1f, -1, -1, magicalConstruction ? 0f : 0.25f);
				b.Draw(Game1.menuTexture, new Microsoft.Xna.Framework.Rectangle(messageBoxX + messageXText, (int)location.Y, messageBoxWidth - messageXText * 2, 4), new Microsoft.Xna.Framework.Rectangle(44, 300, 4, 4), Color.White);
				backButton.draw(b);
				forwardButton.draw(b);
				foreach (Item ingredient in ingredients)
				{
					location.Y += (int)(heightMod * 68f);
					ingredient.drawInMenu(b, location, 1f);
					bool flag = ((!(ingredient is Object) || Game1.player.hasItemInInventory((ingredient as Object).parentSheetIndex, ingredient.Stack)) ? true : false);
					if (magicalConstruction)
					{
						Utility.drawTextWithShadow(b, ingredient.DisplayName, Game1.smallFont, new Vector2(location.X + 64f + 12f, location.Y + 24f), Game1.textColor * 0.25f, 1f, -0.93f, -1, -1, magicalConstruction ? 0f : 0.25f);
						Utility.drawTextWithShadow(b, ingredient.DisplayName, Game1.smallFont, new Vector2(location.X + 64f + 16f - 1f, location.Y + 24f), Game1.textColor * 0.25f, 1f, 0.07f, -1, -1, magicalConstruction ? 0f : 0.25f);
					}
					Utility.drawTextWithShadow(b, ingredient.DisplayName, Game1.smallFont, new Vector2(location.X + 64f + 16f, location.Y + 20f), flag ? (magicalConstruction ? Color.PaleGoldenrod : Game1.textColor) : Color.Red, 1f, -1f, -1, -1, magicalConstruction ? 0f : 0.25f);
				}
				if (moveButton.visible)
				{
					IClickableMenu.drawTextureBoxWithIconAndText(b, Game1.smallFont, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(256, 256, 10, 10), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(257, 284, 16, 16), moveButtonText, moveButton.bounds.X, moveButton.bounds.Y, moveButton.bounds.Width, moveButton.bounds.Height, Color.White, 4f, drawShadow: true, iconLeft: true, isClickable: true, moveButtonHeld);
				}
				if (_selectedBottomButton == BottomButton.Move && moveButton.visible)
				{
					IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(20, 96, 20, 20), moveButton.bounds.X - 4, moveButton.bounds.Y - 4, moveButton.bounds.Width + 8, moveButton.bounds.Height + 8, Color.White, 4f, drawShadow: false);
				}
				if (CurrentBlueprint.isUpgrade())
				{
					upgradeButtonText = Game1.content.LoadString("Strings\\UI:Carpenter_Upgrade", new BluePrint(CurrentBlueprint.nameOfBuildingToUpgrade).displayName);
					if (buildButton.visible)
					{
						IClickableMenu.drawTextureBoxWithIconAndText(b, Game1.smallFont, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(256, 256, 10, 10), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(366, 373, 16, 16), upgradeButtonText, buildButton.bounds.X, buildButton.bounds.Y, buildButton.bounds.Width, buildButton.bounds.Height, Color.White, 4f, drawShadow: true, iconLeft: true, isClickable: true, buildButtonHeld);
					}
				}
				else if (buildButton.visible)
				{
					IClickableMenu.drawTextureBoxWithIconAndText(b, Game1.smallFont, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(256, 256, 10, 10), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(366, 373, 16, 16), buildButtonText, buildButton.bounds.X, buildButton.bounds.Y, buildButton.bounds.Width, buildButton.bounds.Height, Color.White, 4f, drawShadow: true, iconLeft: true, isClickable: true, buildButtonHeld);
				}
				if (_selectedBottomButton == BottomButton.BuildOrUpgrade && buildButton.visible)
				{
					IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(20, 96, 20, 20), buildButton.bounds.X - 4, buildButton.bounds.Y - 4, buildButton.bounds.Width + 8, buildButton.bounds.Height + 8, Color.White, 4f, drawShadow: false);
				}
				if (demolishButton.visible)
				{
					IClickableMenu.drawTextureBoxWithIconAndText(b, Game1.smallFont, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(256, 256, 10, 10), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(348, 372, 17, 17), demolishButtonText, demolishButton.bounds.X, demolishButton.bounds.Y, demolishButton.bounds.Width, demolishButton.bounds.Height, Color.White, 4f, drawShadow: true, iconLeft: true, isClickable: true, demolishButtonHeld);
				}
				if (_selectedBottomButton == BottomButton.Demolish && demolishButton.visible)
				{
					IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(20, 96, 20, 20), demolishButton.bounds.X - 4, demolishButton.bounds.Y - 4, demolishButton.bounds.Width + 8, demolishButton.bounds.Height + 8, Color.White, 4f, drawShadow: false);
				}
				if (paintButton.visible)
				{
					IClickableMenu.drawTextureBoxWithIconAndText(b, Game1.smallFont, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(256, 256, 10, 10), Game1.mouseCursors2, new Microsoft.Xna.Framework.Rectangle(80, 208, 16, 16), paintButtonText, paintButton.bounds.X, paintButton.bounds.Y, paintButton.bounds.Width, paintButton.bounds.Height, Color.White, 4f, drawShadow: true, iconLeft: true, isClickable: true, paintButtonHeld);
				}
				if (_selectedBottomButton == BottomButton.Paint && paintButton.visible)
				{
					IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(20, 96, 20, 20), paintButton.bounds.X - 4, paintButton.bounds.Y - 4, paintButton.bounds.Width + 8, paintButton.bounds.Height + 8, Color.White, 4f, drawShadow: false);
				}
				base.draw(b);
			}
			else
			{
				string text2 = "";
				text2 = (upgrading ? Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Upgrade", new BluePrint(CurrentBlueprint.nameOfBuildingToUpgrade).displayName) : (demolishing ? Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Demolish") : (painting ? Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Paint") : ((buildingToMove != null) ? Game1.content.LoadString("Strings\\UI:Carpenter_ChooseLocation") : Game1.content.LoadString("Strings\\UI:select_building")))));
				SpriteText.drawStringWithScrollCenteredAt(b, text2, Game1.uiViewport.Width / 2, 16);
				Game1.StartWorldDrawInUI(b);
				DrawPlacementSquares(b);
				Game1.EndWorldDrawInUI(b);
			}
			if (onFarm)
			{
				SetOKButtonBounds();
				if (moving)
				{
					if ((_selectedBuilding != null && buildingToMove == null) || (buildingToMove != null && canPlace))
					{
						okButton.draw(b, Color.White, 0.086f + (float)okButton.bounds.Y / 20000f);
					}
				}
				else if (demolishing)
				{
					if (_selectedBuilding != null)
					{
						okButton.draw(b, Color.White, 0.086f + (float)okButton.bounds.Y / 20000f);
					}
				}
				else
				{
					okButton.draw(b, Color.White, 0.086f + (float)okButton.bounds.Y / 20000f);
				}
				SetCancelButtonBounds();
				cancelButton.draw(b, Color.White, 0.086f + (float)okButton.bounds.Y / 20000f);
			}
			if (hoverText.Length > 0)
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.dialogueFont);
			}
		}

		private void SetCancelButtonBounds()
		{
			cancelButton.bounds.X = Game1.uiViewport.Width - Game1.xEdge - cancelButton.bounds.Width - 100;
			cancelButton.bounds.Y = Game1.uiViewport.Height - cancelButton.bounds.Height - 10;
			if (cancelButtonHeld)
			{
				cancelButton.bounds.X += 4;
				cancelButton.bounds.Y += 4;
			}
		}

		private void SetOKButtonBounds()
		{
			okButton.bounds.X = Game1.uiViewport.Width - Game1.xEdge - okButton.bounds.Width - 10;
			okButton.bounds.Y = Game1.uiViewport.Height - okButton.bounds.Height - 10;
			if (okButtonHeld)
			{
				okButton.bounds.X += 4;
				okButton.bounds.Y += 4;
			}
		}

		private void OnTapButtonLeftArrow()
		{
			currentBlueprintIndex--;
			if (currentBlueprintIndex < 0)
			{
				currentBlueprintIndex = blueprints.Count - 1;
			}
			setNewActiveBlueprint();
			forwardButton.scale = forwardButton.baseScale;
			Game1.playSound("shwip");
		}

		private void OnTapButtonRightArrow()
		{
			currentBlueprintIndex = (currentBlueprintIndex + 1) % blueprints.Count;
			setNewActiveBlueprint();
			backButton.scale = backButton.baseScale;
			Game1.playSound("shwip");
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (!Game1.globalFade && !freeze)
			{
				base.releaseLeftClick(x, y);
				if (demolishButtonHeld && !onFarm && demolishButton.containsPoint(x, y))
				{
					OnReleaseDemolishButton();
				}
				else if (paintButtonHeld && !onFarm && paintButton.containsPoint(x, y))
				{
					OnReleasePaintButton();
				}
				else if (moveButtonHeld && !onFarm && moveButton.containsPoint(x, y))
				{
					OnReleaseMoveButton();
				}
				else if (buildButtonHeld && buildButton.containsPoint(x, y))
				{
					OnReleaseBuildButton();
				}
				else if (cancelButton.containsPoint(x, y))
				{
					OnReleaseCancelButton();
				}
				else if (onFarm && okButtonHeld && okButton.bounds.Contains(x, y))
				{
					OnReleaseOKButton();
				}
				ResetButtonHeldStates();
			}
		}

		public void DrawPlacementSquares(SpriteBatch b)
		{
			if (Game1.globalFade || freeze || !onFarm || !(Game1.currentLocation is BuildableGameLocation))
			{
				return;
			}
			if (!upgrading && !demolishing && !moving && !painting)
			{
				Vector2 vector = new Vector2((Game1.viewport.X + _drawAtX) / 64, (Game1.viewport.Y + _drawAtY) / 64);
				for (int i = 0; i < CurrentBlueprint.tilesHeight; i++)
				{
					for (int j = 0; j < CurrentBlueprint.tilesWidth; j++)
					{
						int num = CurrentBlueprint.getTileSheetIndexForStructurePlacementTile(j, i);
						Vector2 vector2 = new Vector2(vector.X + (float)j, vector.Y + (float)i);
						if (!(Game1.currentLocation as BuildableGameLocation).isBuildable(vector2))
						{
							num++;
						}
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, vector2 * 64f), new Microsoft.Xna.Framework.Rectangle(194 + num * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
					}
				}
				{
					foreach (Point additionalPlacementTile in CurrentBlueprint.additionalPlacementTiles)
					{
						int x = additionalPlacementTile.X;
						int y = additionalPlacementTile.Y;
						int num2 = CurrentBlueprint.getTileSheetIndexForStructurePlacementTile(x, y);
						Vector2 vector3 = new Vector2(vector.X + (float)x, vector.Y + (float)y);
						if (!(Game1.currentLocation as BuildableGameLocation).isBuildable(vector3))
						{
							num2++;
						}
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, vector3 * 64f), new Microsoft.Xna.Framework.Rectangle(194 + num2 * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
					}
					return;
				}
			}
			if (painting || !moving || buildingToMove == null)
			{
				return;
			}
			Vector2 vector4 = new Vector2((Game1.viewport.X + _drawAtX) / 64, (Game1.viewport.Y + _drawAtY) / 64);
			BuildableGameLocation buildableGameLocation = Game1.currentLocation as BuildableGameLocation;
			canPlace = true;
			for (int k = 0; k < (int)buildingToMove.tilesHigh; k++)
			{
				for (int l = 0; l < (int)buildingToMove.tilesWide; l++)
				{
					int num3 = buildingToMove.getTileSheetIndexForStructurePlacementTile(l, k);
					Vector2 vector5 = new Vector2(vector4.X + (float)l, vector4.Y + (float)k);
					bool flag = buildableGameLocation.buildings.Contains(buildingToMove) && buildingToMove.occupiesTile(vector5);
					if (!buildableGameLocation.isBuildable(vector5) && !flag && (!(vector5.X >= (float)(int)buildingToMove.tileX) || !(vector5.X < (float)((int)buildingToMove.tileX + (int)buildingToMove.tilesWide)) || !(vector5.Y >= (float)(int)buildingToMove.tileY) || !(vector5.Y < (float)((int)buildingToMove.tileY + (int)buildingToMove.tilesHigh))))
					{
						num3++;
						canPlace = false;
					}
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, vector5 * 64f), new Microsoft.Xna.Framework.Rectangle(194 + num3 * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
				}
			}
			foreach (Point additionalPlacementTile2 in buildingToMove.additionalPlacementTiles)
			{
				int x2 = additionalPlacementTile2.X;
				int y2 = additionalPlacementTile2.Y;
				int num4 = buildingToMove.getTileSheetIndexForStructurePlacementTile(x2, y2);
				Vector2 vector6 = new Vector2(vector4.X + (float)x2, vector4.Y + (float)y2);
				bool flag2 = buildableGameLocation.buildings.Contains(buildingToMove) && buildingToMove.occupiesTile(vector6);
				if (!buildableGameLocation.isBuildable(vector6) && !flag2)
				{
					num4++;
				}
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, vector6 * 64f), new Microsoft.Xna.Framework.Rectangle(194 + num4 * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
			}
		}

		private void OnReleasePaintButton()
		{
			Game1.globalFadeToBlack(setUpForBuildingPlacement);
			Game1.playSound("smallSelect");
			onFarm = true;
			painting = true;
		}

		private void OnReleaseDemolishButton()
		{
			Game1.globalFadeToBlack(setUpForBuildingPlacement);
			Game1.playSound("smallSelect");
			onFarm = true;
			demolishing = true;
		}

		private void OnReleaseMoveButton()
		{
			Game1.globalFadeToBlack(setUpForBuildingPlacement);
			Game1.playSound("smallSelect");
			onFarm = true;
			moving = true;
		}

		private void OnReleaseBuildButton()
		{
			if (!onFarm && price >= 0 && Game1.player.Money >= price && blueprints[currentBlueprintIndex].doesFarmerHaveEnoughResourcesToBuild())
			{
				Game1.globalFadeToBlack(setUpForBuildingPlacement);
				Game1.playSound("smallSelect");
				onFarm = true;
			}
		}

		private void OnReleaseCancelButton()
		{
			if (!onFarm)
			{
				exitThisMenu();
				Game1.player.forceCanMove();
				Game1.playSound("bigDeSelect");
			}
			else if (moving && buildingToMove != null)
			{
				buildingToMove = null;
				_selectedBuilding = null;
				foreach (Building building in ((Farm)Game1.getLocationFromName("Farm")).buildings)
				{
					building.color.Value = Color.White;
				}
				Game1.playSound("cancel");
			}
			else
			{
				returnToCarpentryMenu();
				Game1.playSound("smallSelect");
			}
		}

		private void OnReleaseOKButton()
		{
			Game1.playSound("smallSelect");
			OnClickOK();
		}

		private void ResetButtonHeldStates()
		{
			demolishButtonHeld = false;
			moveButtonHeld = false;
			buildButtonHeld = false;
			paintButtonHeld = false;
			cancelButtonHeld = false;
			okButtonHeld = false;
		}

		private void TestToPan(int x, int y)
		{
			if ((x <= cancelButton.bounds.X - cancelButton.bounds.Width || y <= cancelButton.bounds.Y - cancelButton.bounds.Height) && !okButtonHeld)
			{
				if (_lastTapX != -1 && _lastTapY != -1)
				{
					int x2 = (int)((float)(_lastTapX - x) / Game1.options.zoomLevel);
					int y2 = (int)((float)(_lastTapY - y) / Game1.options.zoomLevel);
					Game1.panScreen(x2, y2);
				}
				_drawAtX = (int)((float)x / Game1.options.zoomLevel);
				_drawAtY = (int)((float)y / Game1.options.zoomLevel);
				_lastTapX = x;
				_lastTapY = y;
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			if (onFarm && !Game1.globalFade)
			{
				if (!PinchZoom.Instance.CheckForPinchZoom())
				{
					TestToPan(Game1.input.GetMouseState().X, Game1.input.GetMouseState().Y);
				}
				return;
			}
			if (demolishButtonHeld && !demolishButton.containsPoint(x, y))
			{
				demolishButtonHeld = false;
			}
			if (moveButtonHeld && !moveButton.containsPoint(x, y))
			{
				moveButtonHeld = false;
			}
			if (buildButtonHeld && !buildButton.containsPoint(x, y))
			{
				buildButtonHeld = false;
			}
			if (paintButtonHeld && !paintButton.containsPoint(x, y))
			{
				paintButtonHeld = false;
			}
		}

		protected override void cleanupBeforeExit()
		{
			base.cleanupBeforeExit();
			((Farm)Game1.getLocationFromName("Farm")).frameHouseColor = Color.White;
		}

		public override bool readyToClose()
		{
			if (base.readyToClose())
			{
				return buildingToMove == null;
			}
			return false;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}
	}
}
